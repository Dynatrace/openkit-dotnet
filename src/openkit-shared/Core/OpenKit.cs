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

        // *** constructors ***

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
            this.configuration = configuration;
            this.logger = logger;
            this.timingProvider = timingProvider;
            this.threadIDProvider = threadIDProvider;

            beaconCache = new BeaconCache();
            beaconSender = new BeaconSender(configuration, httpClientProvider, timingProvider);
            beaconCacheEvictor = new BeaconCacheEvictor(logger, beaconCache, configuration.BeaconCacheConfig, timingProvider);
        }

        /// <summary>
        /// Initialize this OpenKit instance.
        /// </summary>
        /// <remarks>
        /// This method starts the <see cref="BeaconSender"/>  and is called directly after
        /// the instance has been created in <see cref="OpenKitFactory"/>.
        /// </remarks>
        internal void Initialize()
        {
            beaconCacheEvictor.Start();
            beaconSender.Initialize();
        }

        // *** IOpenKit interface methods ***

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
            // create beacon for session
            var beacon = new Beacon(logger, beaconCache, configuration, clientIPAddress, threadIDProvider, timingProvider);
            // create session
            return new Session(beaconSender, beacon);
        }

        public void Shutdown()
        {
            beaconCacheEvictor.Stop();
            beaconSender.Shutdown();
        }
    }
}
