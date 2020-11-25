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

using NUnit.Framework;
using System;

namespace Dynatrace.OpenKit.Providers
{
    class DefaultTimingProviderTest
    {
        [Test]
        public void ProvideTimeStampInMillisecondsReturnsCurrentTime()
        {
            // given
            var target = new DefaultTimingProvider();

            // when
            var timeBefore = (long)(DateTime.UtcNow - DefaultTimingProvider.EpochDateTime).TotalMilliseconds;
            var obtained = target.ProvideTimestampInMilliseconds();
            var timeAfter = (long)(DateTime.UtcNow - DefaultTimingProvider.EpochDateTime).TotalMilliseconds;

            // then
            Assert.That(obtained, Is.GreaterThanOrEqualTo(timeBefore));
            Assert.That(obtained, Is.LessThanOrEqualTo(timeAfter));
        }
    }
}
