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
using Dynatrace.OpenKit.Providers;
using NSubstitute;

namespace Dynatrace.OpenKit.Protocol
{
    internal class TestBeaconBuilder
    {
        private ILogger logger;
        private IBeaconCache beaconCache;
        private IBeaconConfiguration configuration;
        private string clientIpAddress;
        private ISessionIdProvider sessionIdProvider;
        private int sessionSequenceNumber;
        private IThreadIdProvider threadIdProvider;
        private ITimingProvider timingProvider;
        private IPrnGenerator randomGenerator;

        internal TestBeaconBuilder()
        {
            logger = Substitute.For<ILogger>();
            beaconCache = Substitute.For<IBeaconCache>();
            clientIpAddress = "127.0.0.1";
            sessionIdProvider = Substitute.For<ISessionIdProvider>();
            threadIdProvider = Substitute.For<IThreadIdProvider>();
            timingProvider = Substitute.For<ITimingProvider>();
            randomGenerator = Substitute.For<IPrnGenerator>();
        }

        internal TestBeaconBuilder With(ILogger logger)
        {
            this.logger = logger;
            return this;
        }

        internal TestBeaconBuilder With(IBeaconCache cache)
        {
            beaconCache = cache;
            return this;
        }

        internal TestBeaconBuilder With(IBeaconConfiguration config)
        {
            configuration = config;
            return this;
        }

        internal TestBeaconBuilder WithIpAddress(string ipAddress)
        {
            clientIpAddress = ipAddress;
            return this;
        }

        internal TestBeaconBuilder With(ISessionIdProvider provider)
        {
            sessionIdProvider = provider;
            return this;
        }

        internal TestBeaconBuilder WithSessionSequenceNumber(int sessionSeqNo)
        {
            sessionSequenceNumber = sessionSeqNo;
            return this;
        }

        internal TestBeaconBuilder With(IThreadIdProvider provider)
        {
            threadIdProvider = provider;
            return this;
        }

        internal TestBeaconBuilder With(ITimingProvider provider)
        {
            timingProvider = provider;
            return this;
        }

        internal TestBeaconBuilder With(IPrnGenerator generator)
        {
            randomGenerator = generator;
            return this;
        }

        internal IBeacon Build()
        {
            var initializer = Substitute.For<IBeaconInitializer>();
            initializer.Logger.Returns(logger);
            initializer.BeaconCache.Returns(beaconCache);
            initializer.ClientIpAddress.Returns(clientIpAddress);
            initializer.SessionIdProvider.Returns(sessionIdProvider);
            initializer.SessionSequenceNumber.Returns(sessionSequenceNumber);
            initializer.ThreadIdProvider.Returns(threadIdProvider);
            initializer.TimingProvider.Returns(timingProvider);
            initializer.RandomNumberGenerator.Returns(randomGenerator);

            return new Beacon(initializer, configuration);
        }
    }
}