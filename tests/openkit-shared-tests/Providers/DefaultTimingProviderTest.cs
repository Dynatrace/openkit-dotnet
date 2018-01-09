//
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

using NUnit.Framework;
using System;

namespace Dynatrace.OpenKit.Providers
{
    public class DefaultTimingProviderTest
    {
        private long clusterOffset = 1234L;
        private long now;
        private TestDefaultTimingProvider provider;

        [SetUp]
        public void SetUp()
        {
            now = DateTime.Now.Millisecond;
            provider = new TestDefaultTimingProvider(now);
        }

        [Test]
        public void TimeSyncIsSupportedByDefault()
        {
            // given
            var provider = new DefaultTimingProvider();

            // then
            Assert.That(provider.IsTimeSyncSupported, Is.True);
        }

        [Test]
        public void TimeSyncIsSupportedIfInitCalledWithTrue()
        {
            // given
            provider.Initialze(0L, true);

            // then
            Assert.That(provider.IsTimeSyncSupported, Is.True);
        }

        [Test]
        public void TimeSyncIsNotSupportedIfInitCalledWithFalse()
        {
            // given
            provider.Initialze(0L, false);

            // then
            Assert.That(provider.IsTimeSyncSupported, Is.False);
        }

        [Test]
        public void CanConvertToClusterTime()
        {
            // given
            provider.Initialze(clusterOffset, true);

            // when
            var target = provider.ConvertToClusterTime(1000);

            // then
            Assert.That(target, Is.EqualTo(clusterOffset + 1000));
        }

        private class TestDefaultTimingProvider : DefaultTimingProvider
        {
            private readonly long now;

            public TestDefaultTimingProvider(long now)
            {
                this.now = now;
            }

            public override long ProvideTimestampInMilliseconds()
            {
                return now;
            }
        }
    }
}
