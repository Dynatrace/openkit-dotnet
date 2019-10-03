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

using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using ILogger = Dynatrace.OpenKit.API.ILogger;

namespace Dynatrace.OpenKit.Core.Objects
{
    internal class TestOpenKitBuilder
    {
        private ILogger logger;
        private IPrivacyConfiguration privacyConfig;
        private IOpenKitConfiguration openKitConfig;
        private IThreadIdProvider threadIdProvider;
        private ITimingProvider timingProvider;
        private ISessionIdProvider sessionIdProvider;
        private IBeaconCache beaconCache;
        private IBeaconSender beaconSender;
        private IBeaconCacheEvictor beaconCacheEvictor;

        public TestOpenKitBuilder()
        {
            logger = Substitute.For<ILogger>();
            privacyConfig = Substitute.For<IPrivacyConfiguration>();
            openKitConfig = Substitute.For<IOpenKitConfiguration>();
            threadIdProvider = Substitute.For<IThreadIdProvider>();
            timingProvider = Substitute.For<ITimingProvider>();
            sessionIdProvider = Substitute.For<ISessionIdProvider>();
            beaconCache = Substitute.For<IBeaconCache>();
            beaconSender = Substitute.For<IBeaconSender>();
            beaconCacheEvictor = Substitute.For<IBeaconCacheEvictor>();
        }

        internal TestOpenKitBuilder With(ILogger logger)
        {
            this.logger = logger;
            return this;
        }

        internal TestOpenKitBuilder With(IPrivacyConfiguration config)
        {
            privacyConfig = config;
            return this;
        }

        internal TestOpenKitBuilder With(IOpenKitConfiguration config)
        {
            openKitConfig = config;
            return this;
        }

        internal TestOpenKitBuilder With(IThreadIdProvider provider)
        {
            this.threadIdProvider = provider;
            return this;
        }

        internal TestOpenKitBuilder With(ITimingProvider provider)
        {
            this.timingProvider = provider;
            return this;
        }

        internal TestOpenKitBuilder With(ISessionIdProvider provider)
        {
            this.sessionIdProvider = provider;
            return this;
        }

        internal TestOpenKitBuilder With(IBeaconCache cache)
        {
            this.beaconCache = cache;
            return this;
        }

        internal TestOpenKitBuilder With(IBeaconSender sender)
        {
            beaconSender = sender;
            return this;
        }

        internal TestOpenKitBuilder With(IBeaconCacheEvictor evictor)
        {
            beaconCacheEvictor = evictor;
            return this;
        }

        internal OpenKit Build()
        {
            return new OpenKit(
                logger,
                privacyConfig,
                openKitConfig,
                timingProvider,
                threadIdProvider,
                sessionIdProvider,
                beaconCache,
                beaconSender,
                beaconCacheEvictor
            );
        }
    }
}