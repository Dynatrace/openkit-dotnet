//
// Copyright 2018 Dynatrace LLC
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
        private readonly AbstractConfiguration configuration;
        private readonly ITimingProvider timingProvider;
        private readonly BeaconSender beaconSender;
        private readonly IThreadIDProvider threadIDProvider;
        private readonly ILogger logger;

        // *** constructors ***

        public OpenKit(ILogger logger, AbstractConfiguration configuration)
            : this(configuration, new DefaultHTTPClientProvider(logger), new DefaultTimingProvider(), new DefaultThreadIDProvider())
        {
        }

        protected OpenKit(AbstractConfiguration configuration,
            IHTTPClientProvider httpClientProvider,
            ITimingProvider timingProvider,
            IThreadIDProvider threadIDProvider)
        {
            this.configuration = configuration;
            this.timingProvider = timingProvider;
            beaconSender = new BeaconSender(configuration, httpClientProvider, timingProvider);
            this.threadIDProvider = threadIDProvider;
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

        public IDevice Device => configuration.Device;

        public ISession CreateSession(string clientIPAddress)
        {
            // create beacon for session
            var beacon = new Beacon(configuration, clientIPAddress, threadIDProvider, timingProvider);
            // create session
            return new Session(beaconSender, beacon);
        }

        public void Shutdown()
        {
            beaconSender.Shutdown();
        }
    }
}
