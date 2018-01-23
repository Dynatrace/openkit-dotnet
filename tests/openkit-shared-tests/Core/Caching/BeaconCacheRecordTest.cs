using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Caching
{
    public class BeaconCacheRecordTest
    {
        [Test]
        public void Data()
        {
            // when passing null as argument, then
            Assert.That(new BeaconCacheRecord(0L, null).Data, Is.Null);

            // when passing an emtpy string as argument, then
            Assert.That(new BeaconCacheRecord(0L, string.Empty).Data, Is.EqualTo(string.Empty));

            // when passing non-null and non-empty string as argument, then
            Assert.That(new BeaconCacheRecord(0L, "foobar").Data, Is.EqualTo("foobar"));
        }

        [Test]
        public void Timestamp()
        {
            // when passing negative timestamp, then
            Assert.That(new BeaconCacheRecord(-1L, "a").Timestamp, Is.EqualTo(-1));

            // when passing zero as timestamp, then
            Assert.That(new BeaconCacheRecord(0L, "a").Timestamp, Is.EqualTo(0));

            // and when passing a positive timestamp, then
            Assert.That(new BeaconCacheRecord(1L, "a").Timestamp, Is.EqualTo(1L));
        }

        [Test]
        public void DataSizeInBytes()
        {
            // when data is null, then
            Assert.That(new BeaconCacheRecord(0L, null).DataSizeInBytes, Is.EqualTo(0L));

            // and when data is an empty string, then
            Assert.That(new BeaconCacheRecord(0L, string.Empty).DataSizeInBytes, Is.EqualTo(0L));

            // and when data is valid, then
            Assert.That(new BeaconCacheRecord(0L, "a").DataSizeInBytes, Is.EqualTo(2L));
            Assert.That(new BeaconCacheRecord(0L, "ab").DataSizeInBytes, Is.EqualTo(4L));
            Assert.That(new BeaconCacheRecord(0L, "abc").DataSizeInBytes, Is.EqualTo(6L));
        }

        [Test]
        public void MarkForSending()
        {
            // given
            var target = new BeaconCacheRecord(0L, "abc");

            // then a newly created record is not marked for sending
            Assert.That(target.IsMarkedForSending, Is.False);

            // and when explicitly marked for sending
            target.MarkForSending();

            // then
            Assert.That(target.IsMarkedForSending, Is.True);

            // and when the sending mark is removed
            target.UnsetSending();

            // then
            Assert.That(target.IsMarkedForSending, Is.False);
        }
    }
}
