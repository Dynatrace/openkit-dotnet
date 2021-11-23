//
// Copyright 2018-2021 Dynatrace LLC
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
using System.Diagnostics;

namespace Dynatrace.OpenKit.Providers
{
    public class DefaultTimingProvider : ITimingProvider
    {
        internal static readonly DateTime EpochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal long ReferenceTimestampTicks { get; }

        public DefaultTimingProvider()
        {
            ReferenceTimestampTicks = (DateTime.UtcNow - EpochDateTime).Ticks - StopwatchTimestampAsTimeSpanTicks;
        }

        public virtual long ProvideTimestampInMilliseconds()
        {
            return (long)TimeSpan.FromTicks(ReferenceTimestampTicks + StopwatchTimestampAsTimeSpanTicks).TotalMilliseconds;
        }

        /// <summary>
        /// Get the current <see cref="Stopwatch.GetTimestamp()"/> value as <see cref="TimeSpan"/> tick value.
        /// </summary>
        private static long StopwatchTimestampAsTimeSpanTicks => 
            (long)((Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency) * TimeSpan.TicksPerSecond);
    }
}