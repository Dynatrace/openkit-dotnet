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

namespace Dynatrace.OpenKit.Core
{

    /// <summary>
    ///  Actual implementation of the IOpenKit interface.
    /// </summary>
    public class OpenKit : IOpenKit
    {
        private static readonly ISession NullSession = new NullSession();

        // Configuration reference
        private readonly OpenKitConfiguration configuration;
        private readonly ITimingProvider timingProvider;
        private readonly BeaconSender beaconSender;
        private readonly IThreadIDProvider threadIDProvider;
        private readonly BeaconCache beaconCache;

        // Cache eviction thread
        private readonly BeaconCacheEvictor beaconCacheEvictor;

        // logging context
        private readonly ILogger logger;

        private volatile bool isShutdown = false;

        #region constructors

        public OpenKit(ILogger logger, OpenKitConfiguration configuration)
            : this(logger, configuration, new DefaultHTTPClientProvider(logger), new DefaultTimingProvider(), new DefaultThreadIDProvider())
        {
        }

        protected OpenKit(ILogger logger,
            OpenKitConfiguration configuration,
            IHTTPClientProvider httpClientProvider,
            ITimingProvider timingProvider,
            IThreadIDProvider threadIDProvider)
        {
            if (logger.IsInfoEnabled)
            {
                //TODO: Use proper version information (incl. the build number)
                logger.Info(configuration.OpenKitType + " " + GetType().Name + " " + OpenKitConstants.DEFAULT_APPLICATION_VERSION + " instantiated");
            }
            if (logger.IsDebugEnabled)
            {
                logger.Debug(
                  "applicationName=" + configuration.ApplicationName + ", applicationID=" + configuration.ApplicationID
                  + ", deviceID=" + configuration.DeviceID + ", endpointURL=" + configuration.EndpointURL
                    );
            }
            this.configuration = configuration;
            this.logger = logger;
            this.timingProvider = timingProvider;
            this.threadIDProvider = threadIDProvider;

            beaconCache = new BeaconCache(logger);
            beaconSender = new BeaconSender(logger, configuration, httpClientProvider, timingProvider);
            beaconCacheEvictor = new BeaconCacheEvictor(logger, beaconCache, configuration.BeaconCacheConfig, timingProvider);
        }

        #endregion

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

        public void Dispose()
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
            set
            {
                configuration.ApplicationVersion = value;
            }
        }

        public ISession CreateSession(string clientIPAddress)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(GetType().Name + " CreateSession(" + clientIPAddress + ")");
            }
            if (isShutdown)
            {
                return NullSession;
            }
            // create beacon for session
            var beacon = new Beacon(logger, beaconCache, configuration, clientIPAddress, threadIDProvider, timingProvider);
            // create session
            return new Session(logger, beaconSender, beacon);
        }

        public void Shutdown()
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(GetType().Name + " shutdown requested");
            }
            isShutdown = true;
            beaconCacheEvictor.Stop();
            beaconSender.Shutdown();
        }

        #endregion
    }
}
