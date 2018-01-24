using System;

namespace Dynatrace.OpenKit.Core.Configuration
{
    /// <summary>
    /// Configuration for <see cref="BeaconCache"/>
    /// </summary>
    public class BeaconCacheConfiguration
    {
        public static readonly long DEFAULT_MAX_RECORD_AGE_IN_MILLIS = (long)TimeSpan.FromMinutes(105).TotalMilliseconds; // 1 hour and 45 minutes
        public const long DEFAULT_UPPER_MEMORY_BOUNDARY_IN_BYTES = 100 * 1024 * 1024;
        public const long DEFAULT_LOWER_MEMORY_BOUNDARY_IN_BYTES = 80 * 1024 * 1024;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxRecordAge">Maximum record age in milliseconds</param>
        /// <param name="cacheSizeLowerBound">lower memory limit for cache</param>
        /// <param name="cacheSizeUpperBound">upper memory limit for cache</param>
        public BeaconCacheConfiguration(long maxRecordAge, long cacheSizeLowerBound, long cacheSizeUpperBound)
        {
            MaxRecordAge = maxRecordAge;
            CacheSizeLowerBound = cacheSizeLowerBound;
            CacheSizeUpperBound = cacheSizeUpperBound;        }

        /// <summary>
        /// Get maximum record age in millisecond.s
        /// </summary>
        public long MaxRecordAge { get; }
        /// <summary>
        /// Get lower memory limit for cache.
        /// </summary>
        public long CacheSizeLowerBound { get; }
        /// <summary>
        /// Get upper memory limit for cache.
        /// </summary>
        public long CacheSizeUpperBound { get; }
    }
}
