using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class BeaconCacheConfigurationTest
    {
        [Test]
        public void MaxRecordAge()
        {
            // then
            Assert.That(new BeaconCacheConfiguration(-100L, 1L, 2L).MaxRecordAge, Is.EqualTo(-100L));
            Assert.That(new BeaconCacheConfiguration(0L, 1L, 2L).MaxRecordAge, Is.EqualTo(0L));
            Assert.That(new BeaconCacheConfiguration(200L, 1L, 2L).MaxRecordAge, Is.EqualTo(200L));
        }

        [Test]
        public void CacheSizeLowerBound()
        {
            // then
            Assert.That(new BeaconCacheConfiguration(0L, -1L, 2L).CacheSizeLowerBound, Is.EqualTo(-1L));
            Assert.That(new BeaconCacheConfiguration(-1L, 0L, 2L).CacheSizeLowerBound, Is.EqualTo(0L));
            Assert.That(new BeaconCacheConfiguration(0L, 1L, 2L).CacheSizeLowerBound, Is.EqualTo(1L));
        }

        [Test]
        public void CacheSizeUpperBound()
        {
            // then
            Assert.That(new BeaconCacheConfiguration(0L, -1L, -2L).CacheSizeUpperBound, Is.EqualTo(-2L));
            Assert.That(new BeaconCacheConfiguration(-1L, 1L, 0L).CacheSizeUpperBound, Is.EqualTo(0L));
            Assert.That(new BeaconCacheConfiguration(0L, 1L, 2L).CacheSizeUpperBound, Is.EqualTo(2L));
        }
    }
}
