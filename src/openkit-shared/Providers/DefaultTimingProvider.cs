using System;
using System.Threading;

namespace Dynatrace.OpenKit.Providers
{
    public class DefaultTimingProvider : ITimingProvider
    {
        private static readonly DateTime jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private long lastInitTime = 0;
        private long clusterTimeOffset = 0;
        private bool isTimeSyncSupported = true;

        public bool IsTimeSyncSupported
        {
            get
            {
                lock (this)
                {
                    return isTimeSyncSupported;
                }
            }
        }

        public long LastInitTimeInClusterTime
        {
            get
            {
                return ConvertToClusterTime(lastInitTime);
            }
        }

        public long TimeSinceLastInitTime
        {
            get
            {
                return GetTimeSinceLastInitTime(ProvideTimestampInMilliseconds());
            }
        }

        public virtual long ProvideTimestampInMilliseconds()
        {
            return (long)(DateTime.UtcNow - jan1st1970).TotalMilliseconds;
        }

        public virtual void Sleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        public void Initialze(long clusterTimeOffset, bool isTimeSyncSupported)
        {
            lock (this)
            {
                // set init time in milliseconds since 1970-01-01
                lastInitTime = ProvideTimestampInMilliseconds();
                this.isTimeSyncSupported = isTimeSyncSupported;
                if (isTimeSyncSupported)
                {
                    this.clusterTimeOffset = clusterTimeOffset;
                }
                else
                {
                    this.clusterTimeOffset = 0;
                }
            }
        }

        public long ConvertToClusterTime(long timestamp)
        {
            lock (this)
            {
                return timestamp + clusterTimeOffset;
            }
        }

        public long GetTimeSinceLastInitTime(long timestamp)
        {
            lock (this)
            {
                return timestamp - lastInitTime;
            }
        }
    }
}
