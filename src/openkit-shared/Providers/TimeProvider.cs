/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using System;

namespace Dynatrace.OpenKit.Providers {

    /// <summary>
    ///  Class for providing timestamps, time durations and cluster time conversions.
    /// </summary>
    public class TimeProvider
    {

        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long lastInitTime = 0;
        private static long clusterTimeOffset = 0;
        private static bool timeSynced = false;

        private static long ProvideTimestamp()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        // initialize time provider with cluster time offset or 0 if not time synced
        public static void Initialize(long clusterTimeOffset, bool timeSynced)
        {
            // set init time in milliseconds since 1970-01-01
            lastInitTime = GetTimestamp();
            TimeProvider.timeSynced = timeSynced;
            if (timeSynced)
            {
                TimeProvider.clusterTimeOffset = clusterTimeOffset;
            }
            else
            {
                TimeProvider.clusterTimeOffset = 0;
            }
        }

        public static bool IsTimeSynced
        {
            get
            {
                return TimeProvider.timeSynced;
            }
        }

        // convert a local timestamp to cluster time
        // return local time if not time synced or if not yet initialized
        public static long ConvertToClusterTime(long timestamp)
        {
            return timestamp + clusterTimeOffset;
        }

        // return timestamp in milliseconds since 1970-01-01 in local time
        public static long GetTimestamp()
        {
            return ProvideTimestamp();
        }

        // return last init time in cluster time, or 0 if not yet initialized
        public static long GetLastInitTimeInClusterTime()
        {
            return lastInitTime + clusterTimeOffset;
        }

        // return time since last init in milliseconds, or since 1970-01-01 if not yet initialized
        public static long GetTimeSinceLastInitTime()
        {
            return ProvideTimestamp() - lastInitTime;
        }

        // return time between provided timestamp and last init in milliseconds, or between provided timestamp and 1970-01-01 if not yet initialized
        public static long GetTimeSinceLastInitTime(long timestamp)
        {
            return timestamp - lastInitTime;
        }

    }

}
