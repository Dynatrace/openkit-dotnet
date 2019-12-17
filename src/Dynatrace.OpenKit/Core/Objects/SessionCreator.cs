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

using System;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Core.Objects
{
    public class SessionCreator : ISessionCreator
    {
        // log message reporter
        private readonly ILogger logger;
        // OpenKit related configuration
        private readonly IOpenKitConfiguration openKitConfiguration;
        // privacy related configuration
        private readonly IPrivacyConfiguration privacyConfiguration;

        private readonly IThreadIdProvider threadIdProvider;
        private readonly ITimingProvider timingProvider;

        private readonly IBeaconCache beaconCache;

        private readonly string clientIpAddress;
        private readonly int serverId;

        private readonly ISessionIdProvider sessionIdProvider;
        private readonly IPrnGenerator randomNumberGenerator;

        internal SessionCreator(ISessionCreatorInput input, string clientIpAddress)
        {
            logger = input.Logger;
            openKitConfiguration = input.OpenKitConfiguration;
            privacyConfiguration = input.PrivacyConfiguration;
            beaconCache = input.BeaconCache;
            threadIdProvider = input.ThreadIdProvider;
            timingProvider = input.TimingProvider;
            this.clientIpAddress = clientIpAddress;

            serverId = input.CurrentServerId;
            sessionIdProvider = new FixedSessionIdProvider(input.SessionIdProvider);
            randomNumberGenerator = new FixedPrnGenerator(new DefaultPrnGenerator());
        }

        ISessionInternals ISessionCreator.CreateSession(IOpenKitComposite parent)
        {
            var configuration = BeaconConfiguration.From(openKitConfiguration, privacyConfiguration, serverId);
            var beacon = new Beacon(
                logger,
                beaconCache,
                configuration,
                clientIpAddress,
                sessionIdProvider,
                threadIdProvider,
                timingProvider,
                randomNumberGenerator
            );

            var session = new Session(logger, parent, beacon);
            SessionSequenceNumber++;

            return session;
        }

        internal int SessionSequenceNumber { get; private set; }

        internal IPrnGenerator RandomNumberGenerator => randomNumberGenerator;
    }
}