//
// Copyright 2018-2021 Dynatrace LLC
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

using System;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using Dynatrace.OpenKit.Util;

namespace Dynatrace.OpenKit.Core.Objects
{
    /// <summary>
    ///  Actual implementation of the IOpenKit interface.
    /// </summary>
    public class OpenKit : OpenKitComposite, IOpenKit, ISessionCreatorInput
    {
        /// <summary>
        /// <see cref="ILogger"/> for tracing log messages.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Provider of the thread identifier.
        /// </summary>
        private readonly IThreadIdProvider threadIdProvider;

        /// <summary>
        /// Provider for the current time.
        /// </summary>
        private readonly ITimingProvider timingProvider;

        /// <summary>
        /// Provider for the session IDs.
        /// </summary>
        private readonly ISessionIdProvider sessionIdProvider;

        /// <summary>
        /// Configuration object storing privacy related configuration.
        /// </summary>
        private readonly IPrivacyConfiguration privacyConfiguration;

        /// <summary>
        /// Configuration object storing OpenKit configuration.
        /// </summary>
        private readonly IOpenKitConfiguration openKitConfiguration;

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
        /// Watchdog thread to perform certain actions on a session after a specific time.
        /// </summary>
        private readonly ISessionWatchdog sessionWatchdog;

        /// <summary>
        /// Indicator whether this <see cref="OpenKit"/> instance is shutdown or not.
        /// </summary>
        private volatile bool isShutdown;

        /// <summary>
        /// Object for synchronizing access.
        /// </summary>
        private readonly object lockObject = new object();

        #region constructors

        /// <summary>
        /// Constructor for creating an OpenKit instance.
        /// </summary>
        /// <param name="initializer">provider to get all OpenKit related configuration parameters.</param>
        internal OpenKit(IOpenKitInitializer initializer)
        {
            logger = initializer.Logger;
            privacyConfiguration = initializer.PrivacyConfiguration;
            openKitConfiguration = initializer.OpenKitConfiguration;

            timingProvider = initializer.TimingProvider;
            threadIdProvider = initializer.ThreadIdProvider;
            sessionIdProvider = initializer.SessionIdProvider;

            beaconCache = initializer.BeaconCache;
            beaconCacheEvictor = initializer.BeaconCacheEvictor;

            beaconSender = initializer.BeaconSender;
            sessionWatchdog = initializer.SessionWatchdog;

            LogOpenKitInstanceCreation(logger, openKitConfiguration);
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
                             + $", applicationID={configuration.ApplicationId}"
                             + $", deviceID={configuration.DeviceId.ToInvariantString()}"
                             + $", origDeviceID={configuration.OrigDeviceId}"
                             + $", endpointURL={configuration.EndpointUrl}"
                );
            }
        }

        #endregion

        /// <summary>
        /// Accessor for simplified explicit access to <see cref="IOpenKitComposite"/>.
        /// </summary>
        private IOpenKitComposite ThisComposite => this;

        /// <summary>
        /// Initialize this OpenKit instance.
        /// </summary>
        /// <remarks>
        /// This method starts the <see cref="BeaconSender"/>  and is called directly after
        /// the instance has been created in <see cref="DynatraceOpenKitBuilder.Build"/>.
        /// </remarks>
        internal void Initialize()
        {
            beaconCacheEvictor.Start();
            sessionWatchdog.Initialize();
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

                var sessionCreator = new SessionCreator(this, clientIpAddress);
                var sessionProxy = new SessionProxy(logger, this, sessionCreator, timingProvider, beaconSender,
                    sessionWatchdog);

                ThisComposite.StoreChildInList(sessionProxy);

                return sessionProxy;
            }
        }

        public ISession CreateSession()
        {
            return CreateSession(null);
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
            foreach (var childObject in childObjects)
            {
                childObject.Dispose();
            }

            beaconCacheEvictor.Stop();
            sessionWatchdog.Shutdown();
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

        #region ISessionCreatorInput implementation

        ILogger ISessionCreatorInput.Logger => logger;

        IOpenKitConfiguration ISessionCreatorInput.OpenKitConfiguration => openKitConfiguration;

        IPrivacyConfiguration ISessionCreatorInput.PrivacyConfiguration => privacyConfiguration;

        IBeaconCache ISessionCreatorInput.BeaconCache => beaconCache;

        ISessionIdProvider ISessionCreatorInput.SessionIdProvider => sessionIdProvider;

        IThreadIdProvider ISessionCreatorInput.ThreadIdProvider => threadIdProvider;

        ITimingProvider ISessionCreatorInput.TimingProvider => timingProvider;

        int ISessionCreatorInput.CurrentServerId => beaconSender.CurrentServerId;

        #endregion
    }
}