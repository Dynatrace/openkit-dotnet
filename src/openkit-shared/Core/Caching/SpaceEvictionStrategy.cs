using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using System;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core.Caching
{
    /// <summary>
    /// Space based eviction strategy for the beacon cache.
    /// </summary>
    /// <remarks>
    /// This strategy checks if the number of cached bytes is grewater than <see cref="BeaconCacheConfiguration.CacheSizeLowerBound"/>
    /// and in this case runs the strategy.
    /// </remarks>
    internal class SpaceEvictionStrategy : IBeaconCacheEvictionStrategy
    {
        private readonly ILogger logger;
        private readonly IBeaconCache beaconCache;
        private readonly BeaconCacheConfiguration configuration;
        private readonly Func<bool> isShutdownFunc;

        private bool infoShown = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">nstance implementing the <see cref="ILogger"/> interface for writing some useful debug messages.</param>
        /// <param name="beaconCache">The beacon cache to evict if necessary.</param>
        /// <param name="configuration"></param>
        internal SpaceEvictionStrategy(ILogger logger, IBeaconCache beaconCache, BeaconCacheConfiguration configuration, Func<bool> isShutdownFunc)
        {
            this.logger = logger;
            this.beaconCache = beaconCache;
            this.configuration = configuration;
            this.isShutdownFunc = isShutdownFunc;
        }

        /// <summary>
        /// Checks if the strategy is disabled.
        /// </summary>
        /// <remarks>
        /// The strategy might be disabled on purpose, if upper and lower boundary are less than or equal to 0
        /// or accidentally, when either lower or upper boundary is less than or equal to zero or upper boundary is less
        /// than lower boundary.
        /// </remarks>
        internal bool IsStrategyDisabled => configuration.CacheSizeLowerBound <= 0 
            || configuration.CacheSizeUpperBound <= 0 
            || configuration.CacheSizeUpperBound < configuration.CacheSizeLowerBound;

        /// <summary>
        /// Checks if the strategy should run.
        /// </summary>
        /// <remarks>
        /// The strategy should run, if the currently stored number of bytes in the Beacon cache exceeds the 
        /// </remarks>
        internal bool ShouldRun => beaconCache.NumBytesInCache > configuration.CacheSizeUpperBound;

        public void Execute()
        {
            if (IsStrategyDisabled)
            {
                // immediately return if this strategy is disabled
                if (!infoShown && logger.IsInfoEnabled)
                {
                    logger.Info("SpaceEvictionStrategy is disabled");
                    // suppress any further log output
                    infoShown = true;
                }
                return;
            }

            if (ShouldRun)
            {
                DoExecute();
            }
        }

        /// <summary>
        /// Performs execution of strategy.
        /// </summary>
        private void DoExecute()
        {
            var removedRecordsPerBeacon = new Dictionary<int, int>();

            while (!isShutdownFunc() && beaconCache.NumBytesInCache > configuration.CacheSizeLowerBound)
            {
                var beaconIDs = beaconCache.BeaconIDs;
                IEnumerator<int> iterator = beaconIDs.GetEnumerator();

                while(!isShutdownFunc() && iterator.MoveNext() && beaconCache.NumBytesInCache > configuration.CacheSizeLowerBound)
                {
                    var beaconID = iterator.Current;

                    // remove 1 record from Beacon cache for given beaconID
                    // the result is the number of records removed, which might be in range [0, numRecords=1]
                    int numRecordsRemoved = beaconCache.EvictRecordsByNumber(beaconID, 1);

                    if (logger.IsDebugEnabled)
                    {
                        if (!removedRecordsPerBeacon.ContainsKey(beaconID))
                        {
                            removedRecordsPerBeacon.Add(beaconID, numRecordsRemoved);
                        }
                        else
                        {
                            removedRecordsPerBeacon[beaconID] += numRecordsRemoved;
                        }
                    }
                }
            }

            if (logger.IsDebugEnabled)
            {
                foreach (var entry in removedRecordsPerBeacon)
                {
                    logger.Debug("Removed " + entry.Value + " records from Beacon with ID " + entry.Key);
                }
            }
        }
    }
}
