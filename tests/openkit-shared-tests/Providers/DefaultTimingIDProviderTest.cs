using NUnit.Framework;
using System;

namespace Dynatrace.OpenKit.Providers
{
    public class DefaultTimingIDProviderTest
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
        
        [Test]
        public void LastInitTimeIsSetCorrectly()
        {
            // given
            provider.Initialze(clusterOffset, true);

            // when
            var target = provider.LastInitTimeInClusterTime;

            // then
            Assert.That(target, Is.EqualTo(clusterOffset + now));
        }

        [Test]
        public void CanGetTimeSineLastInit()
        {
            // given
            provider.Initialze(clusterOffset, true);

            // when
            var target = provider.TimeSinceLastInitTime;

            // then
            Assert.That(target, Is.EqualTo(0L));
        }

        [Test]
        public void CanGetTimeSinceLastInitWithTimestamp()
        {
            // given
            provider.Initialze(clusterOffset, true);

            // when
            var target = provider.GetTimeSinceLastInitTime(now + 1);

            // then
            Assert.That(target, Is.EqualTo(1L));
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
