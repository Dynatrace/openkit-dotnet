//
// Copyright 2018-2019 Dynatrace LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Core.Objects
{

    /// <summary>
    ///  Actual implementation of the IOpenKit interface.
    /// </summary>
    public class OpenKit : OpenKitComposite, IOpenKit
    {
        /// <summary>
        /// Cache instance used for storing serialized <see cref="IBeacon">beacon</see> data.
        /// </summary>
        private readonly IBeaconCache beaconCache;

        /// <summary>
        /// <see cref="IBeaconCache"/> eviction thread
        /// </summary>
        private readonly IBeaconCacheEvictor beaconCacheEvictor;

        /// <summary>
        /// Thread for sending <see cref="IBeacon">beacons</see>
        /// </summary>
        private readonly IBeaconSender beaconSender;

        /// <summary>
        /// Container for storing the configuration set by the <see cref="AbstractOpenKitBuilder">OpenKit builder</see>.
        /// </summary>
        private readonly IOpenKitConfiguration configuration;

        /// <summary>
        /// Provider of the thread identifier.
        /// </summary>
        private readonly IThreadIdProvider threadIdProvider;

        /// <summary>
        /// Provider for the current time.
        /// </summary>
        private readonly ITimingProvider timingProvider;

        /// <summary>
        /// <see cref="ILogger"/> for tracing log messages.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Indicator whether this <see cref="OpenKit"/> instance is shutdown or not.
        /// </summary>
        private volatile bool isShutdown;

        private readonly object lockObject = new object();

        #region constructors

        internal OpenKit(ILogger logger, IOpenKitConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
            timingProvider = new DefaultTimingProvider();
            threadIdProvider = new DefaultThreadIdProvider();
            beaconCache = new BeaconCache(logger);
            beaconSender = new BeaconSender(logger, configuration, new DefaultHttpClientProvider(logger),
                timingProvider);
            beaconCacheEvictor = new BeaconCacheEvictor(logger, beaconCache, configuration.BeaconCacheConfig,
                timingProvider);
        }

        /// <summary>
        /// Constructor intended to be used for testing only.
        /// </summary>
        /// <param name="logger">Logger for logging messages.</param>
        /// <param name="configuration">the configuration for this OpenKit instance.</param>
        /// <param name="timingProvider">provider for retrieving timing information.</param>
        /// <param name="threadIdProvider">provider for retrieving the ID of the current thread.</param>
        /// <param name="beaconCache">the cache where the beacon data is stored.</param>
        /// <param name="beaconSender">instance that is responsible for sending beacon related data.</param>
        /// <param name="beaconCacheEvictor">cache evictor to prevent memory over-consumption.</param>
        internal OpenKit(
            ILogger logger,
            IOpenKitConfiguration configuration,
            ITimingProvider timingProvider,
            IThreadIdProvider threadIdProvider,
            IBeaconCache beaconCache,
            IBeaconSender beaconSender,
            IBeaconCacheEvictor beaconCacheEvictor
            )
        {
            LogOpenKitInstanceCreation(logger, configuration);

            this.logger = logger;
            this.configuration = configuration;
            this.threadIdProvider = threadIdProvider;
            this.timingProvider = timingProvider;
            this.beaconCache = beaconCache;
            this.beaconSender = beaconSender;
            this.beaconCacheEvictor = beaconCacheEvictor;
        }

        /// <summary>
        /// Helper method for logging instance creation.
        /// </summary>
        /// <param name="logger">the logger to which the log message is written to</param>
        /// <param name="configuration">the OpenKit related configuration</param>
        private static void LogOpenKitInstanceCreation(ILogger logger, IOpenKitConfiguration configuration)
        {
            if (logger.IsInfoEnabled)
            {
                logger.Info($"{typeof(OpenKit).Name} - {configuration.OpenKitType} " +
                    $"OpenKit {OpenKitConstants.DefaultApplicationVersion} instantiated");
            }
            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{typeof(OpenKit).Name} "
                  + $"- applicationName={configuration.ApplicationName}"
                  + $", applicationID={configuration.ApplicationId}"
                  + $", deviceID={configuration.DeviceId}"
                  + $", origDeviceID={configuration.OrigDeviceId}"
                  + $", endpointURL={configuration.EndpointUrl}"
                );
            }
        }

        #endregion

        /// <summary>
        ///
        /// </summary>
        private IOpenKitComposite ThisComposite => this;

        /// <summary>
        /// Initialize this OpenKit instance.
        /// </summary>
        /// <remarks>
        /// This method starts the <see cref="BeaconSender"/>  and is called directly after
        /// the instance has been created in <see cref="AbstractOpenKitBuilder.Build"/>.
        /// </remarks>
        internal void Initialize()
        {
            beaconCacheEvictor.Start();
            beaconSender.Initialize();
        }

        public override void Dispose()
        {
            Shutdown();
        }

        #region IOpenKit implementation

        public bool WaitForInitCompletion()
        {
            return beaconSender.WaitForInitCompletion();
        }

        public bool WaitForInitCompletion(int timeoutMillis)
        {
            return beaconSender.WaitForInitCompletion(timeoutMillis);
        }

        public bool IsInitialized => beaconSender.IsInitialized;

        public string ApplicationVersion
        {
            set => configuration.ApplicationVersion = value;
        }

        public ISession CreateSession(string clientIpAddress)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{GetType().Name} CreateSession({clientIpAddress})");
            }

            lock (lockObject)
            {
                if (isShutdown)
                {
                    return NullSession.Instance;
                }

                // create beacon for session
                var beacon = new Beacon(logger, beaconCache, configuration, clientIpAddress, threadIdProvider,
                    timingProvider);

                var session = new Session(logger, this, beaconSender, beacon);
                ThisComposite.StoreChildInList(session);

                return session;
            }
        }

        public void Shutdown()
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{GetType().Name} shutdown requested");
            }

            lock (lockObject)
            {
                if (isShutdown)
                {
                    return;
                }
                isShutdown = true;
            }

            // close all open children
            var childObjects = ThisComposite.GetCopyOfChildObjects();
            foreach(var childObject in childObjects)
            {
                childObject.Dispose();
            }

            beaconCacheEvictor.Stop();
            beaconSender.Shutdown();
        }

        #endregion

        #region IOpenkitComposite implementation

        private protected override void OnChildClosed(IOpenKitObject childObject)
        {
            lock (lockObject)
            {
                ThisComposite.RemoveChildFromList(childObject);
            }
        }

        #endregion
    }
}
