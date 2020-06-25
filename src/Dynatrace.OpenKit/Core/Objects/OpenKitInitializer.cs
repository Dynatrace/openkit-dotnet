//
// Copyright 2018-2020 Dynatrace LLC
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
using Dynatrace.OpenKit.Core.Util;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Core.Objects
{
    public class OpenKitInitializer : IOpenKitInitializer
    {
        private readonly ILogger logger;
        private readonly IPrivacyConfiguration privacyConfiguration;
        private readonly IOpenKitConfiguration openKitConfiguration;
        private readonly ITimingProvider timingProvider;
        private readonly IThreadIdProvider threadIdProvider;
        private readonly ISessionIdProvider sessionIdProvider;
        private readonly IBeaconCache beaconCache;
        private readonly IBeaconCacheEvictor beaconCacheEvictor;
        private readonly IBeaconSender beaconSender;
        private readonly ISessionWatchdog sessionWatchdog;

        public OpenKitInitializer(IOpenKitBuilder builder)
        {
            logger = builder.Logger;
            privacyConfiguration = PrivacyConfiguration.From(builder);
            openKitConfiguration = OpenKitConfiguration.From(builder);

            timingProvider = new DefaultTimingProvider();
            threadIdProvider = new DefaultThreadIdProvider();
            sessionIdProvider = new DefaultSessionIdProvider();

            beaconCache = new BeaconCache(logger);
            beaconCacheEvictor = new BeaconCacheEvictor(logger, beaconCache, BeaconCacheConfiguration.From(builder),
                timingProvider);

            var httpClientConfig = HttpClientConfiguration.From(openKitConfiguration);
            // shared thread suspender between BeaconSender and HttpClient. HttpClient will be woken up when
            // BeaconSender shuts down
            var beaconSenderThreadSuspender = new InterruptibleThreadSuspender();
            beaconSender = new BeaconSender(
                logger,
                httpClientConfig,
                new DefaultHttpClientProvider(logger, beaconSenderThreadSuspender),
                timingProvider,
                beaconSenderThreadSuspender);
            var watchdogThreadSuspender = new InterruptibleThreadSuspender();
            sessionWatchdog = new SessionWatchdog(
                logger,
                new SessionWatchdogContext(timingProvider, watchdogThreadSuspender));
        }

        ILogger IOpenKitInitializer.Logger => logger;

        IPrivacyConfiguration IOpenKitInitializer.PrivacyConfiguration => privacyConfiguration;

        IOpenKitConfiguration IOpenKitInitializer.OpenKitConfiguration => openKitConfiguration;

        ITimingProvider IOpenKitInitializer.TimingProvider => timingProvider;

        IThreadIdProvider IOpenKitInitializer.ThreadIdProvider => threadIdProvider;

        ISessionIdProvider IOpenKitInitializer.SessionIdProvider => sessionIdProvider;

        IBeaconCache IOpenKitInitializer.BeaconCache => beaconCache;

        IBeaconCacheEvictor IOpenKitInitializer.BeaconCacheEvictor => beaconCacheEvictor;

        IBeaconSender IOpenKitInitializer.BeaconSender => beaconSender;

        ISessionWatchdog IOpenKitInitializer.SessionWatchdog => sessionWatchdog;
    }
}