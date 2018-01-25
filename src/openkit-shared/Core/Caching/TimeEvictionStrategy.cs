﻿//
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
using Dynatrace.OpenKit.Providers;
using System;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core.Caching
{
    internal class TimeEvictionStrategy : IBeaconCacheEvictionStrategy
    {
        private readonly ILogger logger;
        private readonly IBeaconCache beaconCache;
        private readonly BeaconCacheConfiguration configuration;
        private readonly ITimingProvider timingProvider;
        private readonly Func<bool> isShutdownFunc;

        private bool infoShown = false;

        internal TimeEvictionStrategy(ILogger logger, IBeaconCache beaconCache, BeaconCacheConfiguration configuration, ITimingProvider timingProvider, Func<bool> isShutdownFunc)
        {
            this.logger = logger;
            this.beaconCache = beaconCache;
            this.configuration = configuration;
            this.timingProvider = timingProvider;
            this.isShutdownFunc = isShutdownFunc;
            LastRunTimestamp = -1;
        }

        internal bool ShouldRun => (timingProvider.ProvideTimestampInMilliseconds() - LastRunTimestamp) >= configuration.MaxRecordAge;

        internal bool IsStrategyDisabled => configuration.MaxRecordAge <= 0;

        internal long LastRunTimestamp { get; set; }

        public void Execute()
        {
            if (IsStrategyDisabled)
            {
                if (!infoShown && logger.IsInfoEnabled)
                {
                    logger.Info("TimeEvictionStrategy is disabled");
                    // suppress any further log output
                    infoShown = true;
                }
                return;
            }

            if (LastRunTimestamp < 0)
            {
                // first time execution
                LastRunTimestamp = timingProvider.ProvideTimestampInMilliseconds();
            }

            if (ShouldRun)
            {
                DoExecute();
            }
        }

        private void DoExecute()
        {
            // first get a snapshot of all inserted beacons
            var beaconIDs = beaconCache.BeaconIDs;
            if (beaconIDs.Count == 0)
            {
                // no beacons - set last run timestamp and return immediately
                LastRunTimestamp = timingProvider.ProvideTimestampInMilliseconds();
                return;
            }

            // retrieve the timestamp when we start with execution
            var currentTimestamp = timingProvider.ProvideTimestampInMilliseconds();
            var smallestAllowedBeaconTimestamp = currentTimestamp - configuration.MaxRecordAge;

            // iterate over the previously obtained set and evict for each beacon
            IEnumerator<int> beaconIDIterator = beaconIDs.GetEnumerator();
            while (!isShutdownFunc() && beaconIDIterator.MoveNext())
            {
                var beaconID = beaconIDIterator.Current;
                var numRecordsRemoved = beaconCache.EvictRecordsByAge(beaconID, smallestAllowedBeaconTimestamp);
                if (numRecordsRemoved > 0 && logger.IsDebugEnabled)
                {
                    logger.Debug("Removed " + numRecordsRemoved + " records from Beacon with ID " + beaconID);
                }
            }

            // last but not least update the last runtime
            LastRunTimestamp = currentTimestamp;
        }
    }
}
