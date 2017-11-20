using System;
using System.Threading;

namespace Dynatrace.OpenKit.Providers
{
    public class DefaultTimingProvider : ITimingProvider
    {
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public long ProvideTimestampInMilliseconds()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        public void Sleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }
    }
}
