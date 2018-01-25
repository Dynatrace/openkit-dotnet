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

        [Test]
        public void SameInstancesAreEqual()
        {
            // given
            var target = new BeaconCacheRecord(0L, "abc");

            // then
            Assert.That(target.Equals(target), Is.True);
        }

        [Test]
        public void NullIsNotEqual()
        {
            // given
            var target = new BeaconCacheRecord(0L, "abc");

            // then
            Assert.That(target.Equals(null), Is.False);
        }

        [Test]
        public void ADifferentTypeIsNotConsideredEqual()
        {
            // given
            var target = new BeaconCacheRecord(0L, "abc");

            // then
            Assert.That(target.Equals("abc"), Is.False);
        }

        [Test]
        public void TwoInstancesAreEqualIfFieldsAreEqual()
        {
            // given
            var target = new BeaconCacheRecord(1234L, "abc");
            var other = new BeaconCacheRecord(1234L, "abc");

            // then
            Assert.That(target.Equals(other), Is.True);

            // and when setting send flag
            target.MarkForSending();
            other.MarkForSending();

            // then
            Assert.That(target.Equals(other), Is.True);
        }

        [Test]
        public void TwoInstancesAreNotEqualIfTimestampDiffers()
        {
            // given
            var target = new BeaconCacheRecord(1234L, "abc");
            BeaconCacheRecord other = new BeaconCacheRecord(4321L, "abc");

            // then
            Assert.That(target.Equals(other), Is.False);
        }

        [Test]
        public void TwoInstancesAreNotEqualIfDataDiffers()
        {
            // given
            var target = new BeaconCacheRecord(1234L, "abc");
            var other = new BeaconCacheRecord(1234L, "abcd");

            // then
            Assert.That(target.Equals(other), Is.False);
        }

        [Test]
        public void TwoInstancesAreNotEqualIfMarkedForSendingDiffers()
        {
            // given
            var target = new BeaconCacheRecord(1234L, "abc");
            var other = new BeaconCacheRecord(1234L, "abc");
            other.MarkForSending();

            // then
            Assert.That(target.Equals(other), Is.False);
        }

        [Test]
        public void SameInstancesHaveSameHashCode()
        {
            // given
            var target = new BeaconCacheRecord(1234L, "abc");
            var other = target;

            // then
            Assert.That(target.GetHashCode(), Is.EqualTo(other.GetHashCode()));
        }

        [Test]
        public void TwoEqualInstancesHaveSameHashCode()
        {

            // given
            var target = new BeaconCacheRecord(1234L, "abc");
            var other = new BeaconCacheRecord(1234L, "abc");

            // then
            Assert.That(target.GetHashCode(), Is.EqualTo(other.GetHashCode()));

            // and when marking both for sending
            target.MarkForSending();
            other.MarkForSending();

            // then
            Assert.That(target.GetHashCode(), Is.EqualTo(other.GetHashCode()));
        }

        [Test]
        public void NotEqualInstancesHaveDifferentHashCode()
        {

            // given
            var target = new BeaconCacheRecord(1234L, "abc");
            var otherOne = new BeaconCacheRecord(4321L, "abc");
            var otherTwo = new BeaconCacheRecord(1234L, "abcd");
            var otherThree = new BeaconCacheRecord(1234L, "abcd");
            otherThree.MarkForSending();

            // then
            Assert.That(target.GetHashCode(), Is.Not.EqualTo(otherOne.GetHashCode()));
            Assert.That(target.GetHashCode(), Is.Not.EqualTo(otherTwo.GetHashCode()));
            Assert.That(target.GetHashCode(), Is.Not.EqualTo(otherThree.GetHashCode()));
        }
    }
}
