//
// Copyright 2018-2019 Dynatrace LLC
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
using System.Collections.Generic;
using System.Text;

namespace Dynatrace.OpenKit.Core.Caching
{
    public class BeaconCacheTest
    {
        [Test]
        public void ADefaultConstructedCacheDoesNotContainBeacons()
        {
            // given
            var target = new BeaconCache();

            // then
            Assert.That(target.BeaconIDs, Is.Empty);
            Assert.That(target.NumBytesInCache, Is.EqualTo(0L));
        }

        [Test]
        public void AddEventDataAddsBeaconIdToCache()
        {
            // given
            var target = new BeaconCache();

            // when adding beacon with id 1
            target.AddEventData(1, 1000L, "a");

            // then
            Assert.That(target.BeaconIDs, Is.EqualTo(new HashSet<int> { 1 }));
            Assert.That(target.GetEvents(1), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a") }));
            
            // and when adding beacon with id 2
            target.AddEventData(2, 1100L, "b");

            // then
            Assert.That(target.BeaconIDs, Is.EqualTo(new HashSet<int> { 1, 2 }));
            Assert.That(target.GetEvents(1), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a") }));
            Assert.That(target.GetEvents(2), Is.EqualTo(new[] { new BeaconCacheRecord(1100L, "b") }));
        }

        [Test]
        public void AddEventDataAddsDataToAlreadyExistingBeaconId()
        {
            // given
            var target = new BeaconCache();

            // when adding beacon with id 1
            target.AddEventData(1, 1000L, "a");

            // then
            Assert.That(target.BeaconIDs, Is.EqualTo(new HashSet<int> { 1 }));
            Assert.That(target.GetEvents(1), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a") }));

            // and when adding other data with beacon id 1
            target.AddEventData(1, 1100L, "bc");

            // then
            Assert.That(target.BeaconIDs, Is.EqualTo(new HashSet<int> { 1 }));
            Assert.That(target.GetEvents(1), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a"), new BeaconCacheRecord(1100L, "bc") }));
        }

        [Test]
        public void AddEventDataIncreasesCacheSize()
        {
            // given
            var target = new BeaconCache();

            // when adding some data
            target.AddEventData(1, 1000L, "a");
            target.AddEventData(42, 1000L, "z");
            target.AddEventData(1, 1000L, "iii");

            // then
            Assert.That(target.NumBytesInCache, Is.EqualTo(new BeaconCacheRecord(1000L, "a").DataSizeInBytes 
                                                            + new BeaconCacheRecord(1000L, "z").DataSizeInBytes
                                                            + new BeaconCacheRecord(1000L, "iii").DataSizeInBytes));
        }

        public void AddEventDataRaisesEvent()
        {
            // given
            var target = new BeaconCache();
            
            var notifyCount = 0;
            target.RecordAdded += (s, a) => { notifyCount += 1; };

            // when adding an element
            target.AddEventData(1, 1000L, "a");

            // then verify event got raised
            Assert.That(notifyCount, Is.EqualTo(1));

            // when adding some more data
            target.AddEventData(1, 1100L, "b");
            target.AddEventData(666, 1200L, "xyz");

            // then verify event got raised another two times
            Assert.That(notifyCount, Is.EqualTo(3));
        }

        [Test]
        public void AddActionDataAddsBeaconIdToCache()
        {
            // given
            var target = new BeaconCache();

            // when adding beacon with id 1
            target.AddActionData(1, 1000L, "a");

            // then
            Assert.That(target.BeaconIDs, Is.EqualTo(new HashSet<int> { 1 }));
            Assert.That(target.GetActions(1), Is.EqualTo(new [] { new BeaconCacheRecord(1000L, "a") }));

            // and when adding beacon with id 2
            target.AddActionData(2, 1100L, "b");

            // then
            Assert.That(target.BeaconIDs, Is.EqualTo(new HashSet<int> { 1, 2 }));
            Assert.That(target.GetActions(1), Is.EqualTo(new [] { new BeaconCacheRecord(1000L, "a") }));
            Assert.That(target.GetActions(2), Is.EqualTo(new [] { new BeaconCacheRecord(1100L, "b") }));
        }

        [Test]
        public void AddActionDataAddsDataToAlreadyExistingBeaconId()
        {
            // given
            var target = new BeaconCache();

            // when adding beacon with id 1
            target.AddActionData(1, 1000L, "a");

            // then
            Assert.That(target.BeaconIDs, Is.EqualTo(new HashSet<int> { 1 }));
            Assert.That(target.GetActions(1), Is.EqualTo(new [] { new BeaconCacheRecord(1000L, "a") }));

            // and when adding other data with beacon id 1
            target.AddActionData(1, 1100L, "bc");

            // then
            Assert.That(target.BeaconIDs, Is.EqualTo(new HashSet<int> { 1 }));
            Assert.That(target.GetActions(1), Is.EqualTo(new [] { new BeaconCacheRecord(1000L, "a"), new BeaconCacheRecord(1100L, "bc") }));
        }

        [Test]
        public void AddActionDataIncreasesCacheSize()
        {
            // given
            var target = new BeaconCache();

            // when adding some data
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(42, 1000L, "z");
            target.AddActionData(1, 1000L, "iii");

            // then
            Assert.That(target.NumBytesInCache, Is.EqualTo(new BeaconCacheRecord(1000L, "a").DataSizeInBytes
                                                            + new BeaconCacheRecord(1000L, "z").DataSizeInBytes
                                                            + new BeaconCacheRecord(1000L, "iii").DataSizeInBytes));
        }

        [Test]
        public void AddActionDataRaisesEvent()
        {
            // given
            var target = new BeaconCache();

            var notifyCount = 0;
            target.RecordAdded += (s, a) => { notifyCount += 1; };

            // when adding an element
            target.AddActionData(1, 1000L, "a");

            // then verify event was raised
            Assert.That(notifyCount, Is.EqualTo(1));

            // when adding some more data
            target.AddActionData(1, 1100L, "b");
            target.AddActionData(666, 1200L, "xyz");

            // then verify event got raised another two times
            Assert.That(notifyCount, Is.EqualTo(3));
        }

        [Test]
        public void DeleteCacheEntryRemovesTheGivenBeacon()
        {
            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(42, 1000L, "z");
            target.AddEventData(1, 1000L, "iii");

            // when removing beacon with id 1
            target.DeleteCacheEntry(1);

            // then
            Assert.That(target.BeaconIDs, Is.EqualTo(new HashSet<int> { 42 }));

            // and when removing beacon with id 42
            target.DeleteCacheEntry(42);

            // then
            Assert.That(target.BeaconIDs, Is.Empty);
        }

        [Test]
        public void DeleteCacheEntryDecrementsCacheSize()
        {
            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(42, 1000L, "z");
            target.AddEventData(1, 1000L, "iii");

            // when deleting entry with beacon id 42
            target.DeleteCacheEntry(42);

            // then
            Assert.That(target.NumBytesInCache, Is.EqualTo(new BeaconCacheRecord(1000L, "a").DataSizeInBytes
                                                            + new BeaconCacheRecord(1000L, "iii").DataSizeInBytes));
        }

        [Test]
        public void DeleteCacheEntryDoesNotRaiseEvent()
        {
            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(42, 1000L, "z");
            target.AddEventData(1, 1000L, "iii");

            var notifyCount = 0;
            target.RecordAdded += (s, a) => { notifyCount += 1; };

            // when deleting both entries
            target.DeleteCacheEntry(1);
            target.DeleteCacheEntry(42);

            // then
            Assert.That(notifyCount, Is.EqualTo(0));
        }

        [Test]
        public void DeleteCacheEntriesDoesNothingIfGivenBeaconIDIsNotInCache()
        {
            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(42, 1000L, "z");
            target.AddEventData(1, 1000L, "iii");

            var notifyCount = 0;
            target.RecordAdded += (s, a) => { notifyCount += 1; };

            var cachedSize = target.NumBytesInCache;

            // when
            target.DeleteCacheEntry(666);

            // then
            Assert.That(target.BeaconIDs, Is.EqualTo(new HashSet<int> { 1, 42 }));
            Assert.That(target.NumBytesInCache, Is.EqualTo(cachedSize));
            Assert.That(notifyCount, Is.EqualTo(0));
        }

        [Test]
        public void GetNextBeaconChunkReturnsNullIfGivenBeaconIDDoesNotExist()
        {
            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(42, 1000L, "z");
            target.AddEventData(1, 1000L, "iii");

            // when
            var obtained = target.GetNextBeaconChunk(666, "", 1024, '&');

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void GetNextBeaconChunkCopiesDataForSending()
        {
            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(1, 1001L, "iii");
            target.AddActionData(42, 2000L, "z");
            target.AddEventData(1, 1000L, "b");
            target.AddEventData(1, 1001L, "jjj");

            // when
            var obtained = target.GetNextBeaconChunk(1, "prefix", 0, '&');

            // then
            Assert.That(obtained, Is.EqualTo("prefix"));

            Assert.That(target.GetActions(1), Is.Empty);
            Assert.That(target.GetEvents(1), Is.Empty);
            Assert.That(target.GetActionsBeingSent(1), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a"), new BeaconCacheRecord(1001L, "iii") }));
            Assert.That(target.GetEventsBeingSent(1), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "b"), new BeaconCacheRecord(1001L, "jjj") }));
        }

        [Test]
        public void GetNextBeaconChunkDecreasesBeaconCacheSize()
        {
            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(1, 1001L, "iii");
            target.AddActionData(42, 2000L, "z");
            target.AddEventData(1, 1000L, "b");
            target.AddEventData(1, 1001L, "jjj");

            // when
            target.GetNextBeaconChunk(1, "prefix", 0, '&');

            // cache stats are also adjusted
            Assert.That(target.NumBytesInCache, Is.EqualTo(new BeaconCacheRecord(2000L, "z").DataSizeInBytes));
        }

        [Test]
        public void getNextBeaconChunkRetrievesNextChunk()
        {
            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(1, 1001L, "iii");
            target.AddActionData(42, 2000L, "z");
            target.AddEventData(1, 1000L, "b");
            target.AddEventData(1, 1001L, "jjj");

            // when retrieving the first chunk
            var obtained = target.GetNextBeaconChunk(1, "prefix", 10, '&');

            // then
            Assert.That(obtained, Is.EqualTo("prefix&b&jjj"));

            // then
            Assert.That(target.GetActionsBeingSent(1), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a"), new BeaconCacheRecord(1001L, "iii") }));
            var expectedEventRecords = new[] { new BeaconCacheRecord(1000L, "b"), new BeaconCacheRecord(1001L, "jjj") };
            foreach (var record in expectedEventRecords)
            {
                record.MarkForSending();
            }
            Assert.That(target.GetEventsBeingSent(1), Is.EqualTo(expectedEventRecords));
        }

        [Test]
        public void RemoveChunkedDataClearsAlreadyRetrievedChunks()
        {
            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(1, 1001L, "iii");
            target.AddActionData(42, 2000L, "z");
            target.AddEventData(1, 1000L, "b");
            target.AddEventData(1, 1001L, "jjj");

            // when retrieving the first chunk and removing retrieved chunks
            var obtained = target.GetNextBeaconChunk(1, "prefix", 10, '&');
            target.RemoveChunkedData(1);

            // then
            Assert.That(obtained, Is.EqualTo("prefix&b&jjj"));

            Assert.That(target.GetActionsBeingSent(1), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a"), new BeaconCacheRecord(1001L, "iii") }));
            Assert.That(target.GetEventsBeingSent(1), Is.Empty);

            // when retrieving the second chunk and removing retrieved chunks
            obtained = target.GetNextBeaconChunk(1, "prefix", 10, '&');
            target.RemoveChunkedData(1);

            // then
            Assert.That(obtained, Is.EqualTo("prefix&a&iii"));

            Assert.That(target.GetActionsBeingSent(1), Is.Empty);
            Assert.That(target.GetEventsBeingSent(1), Is.Empty);
        }

        [Test]
        public void RemoveChunkedDataDoesNothingIfCalledWithNonExistingBeaconID()
        {
            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(1, 1001L, "iii");
            target.AddActionData(42, 2000L, "z");
            target.AddEventData(1, 1000L, "b");
            target.AddEventData(1, 1001L, "jjj");

            // when retrieving the first chunk and removing the wrong beacon chunk
            target.GetNextBeaconChunk(1, "prefix", 10, '&');
            target.RemoveChunkedData(2);

            // then
            Assert.That(target.GetActionsBeingSent(1), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a"), new BeaconCacheRecord(1001L, "iii") }));
            var expectedEventRecords = new[] { new BeaconCacheRecord(1000L, "b"), new BeaconCacheRecord(1001L, "jjj") };
            foreach (BeaconCacheRecord record in expectedEventRecords)
            {
                record.MarkForSending();
            }
            Assert.That(target.GetEventsBeingSent(1), Is.EqualTo(expectedEventRecords));
        }

        [Test]
        public void ResetChunkedRestoresData()
        {
            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(1, 1001L, "iii");
            target.AddEventData(1, 1000L, "b");
            target.AddEventData(1, 1001L, "jjj");

            // do same step we'd do when we send the
            target.GetNextBeaconChunk(1, "prefix", 10, '&');

            // data has been copied, but still add some new event & action data
            target.AddActionData(1, 6666L, "123");
            target.AddEventData(1, 6666L, "987");

            // and when resetting the previously copied data
            target.ResetChunkedData(1);

            // then
            Assert.That(target.GetActionsBeingSent(1), Is.Null);
            Assert.That(target.GetEventsBeingSent(1), Is.Null);
            Assert.That(target.GetActions(1), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a"), new BeaconCacheRecord(1001L, "iii"), new BeaconCacheRecord(6666L, "123") }));
            Assert.That(target.GetEvents(1), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "b"), new BeaconCacheRecord(1001L, "jjj"), new BeaconCacheRecord(6666L, "987") }));
        }

        [Test]
        public void ResetChunkedRestoresCacheSize()
        {
            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(1, 1001L, "iii");
            target.AddEventData(1, 1000L, "b");
            target.AddEventData(1, 1001L, "jjj");

            // do same step we'd do when we send the
            target.GetNextBeaconChunk(1, "prefix", 10, '&');

            // data has been copied, but still add some new event & action data
            target.AddActionData(1, 6666L, "123");
            target.AddEventData(1, 6666L, "987");

            // and when resetting the previously copied data
            target.ResetChunkedData(1);

            // then
            Assert.That(target.NumBytesInCache, Is.EqualTo(28L));
        }

        [Test]
        public void ResetChunkedRaisesEvent()
        {
            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(1, 1001L, "iii");
            target.AddEventData(1, 1000L, "b");
            target.AddEventData(1, 1001L, "jjj");

            // do same step we'd do when we send the
            target.GetNextBeaconChunk(1, "prefix", 10, '&');

            // data has been copied, but still add some new event & action data
            target.AddActionData(1, 6666L, "123");
            target.AddEventData(1, 6666L, "987");

            var notifyCount = 0;
            target.RecordAdded += (s, a) => { notifyCount += 1; };

            // and when resetting the previously copied data
            target.ResetChunkedData(1);

            // then
            Assert.That(notifyCount, Is.EqualTo(1));
        }

        [Test]
        public void ResetChunkedDoesNothingIfEntryDoesNotExist()
        {
            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(1, 1001L, "iii");
            target.AddEventData(1, 1000L, "b");
            target.AddEventData(1, 1001L, "jjj");

            // do same step we'd do when we send the
            target.GetNextBeaconChunk(1, "prefix", 10, '&');

            // data has been copied, but still add some new event & action data
            target.AddActionData(1, 6666L, "123");
            target.AddEventData(1, 6666L, "987");

            var notifyCount = 0;
            target.RecordAdded += (s, a) => { notifyCount += 1; };

            // and when resetting the previously copied data
            target.ResetChunkedData(666);

            // then
            Assert.That(target.NumBytesInCache, Is.EqualTo(12L));
            Assert.That(notifyCount, Is.EqualTo(0));
        }

        [Test]
        public void EvictRecordsByAgeDoesNothingAndReturnsZeroIfBeaconIDDoesNotExist()
        {

            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(1, 1001L, "iii");
            target.AddEventData(1, 1000L, "b");
            target.AddEventData(1, 1001L, "jjj");

            // when
            var obtained = target.EvictRecordsByAge(666, 0);

            // then
            Assert.That(obtained, Is.EqualTo(0));
        }

        [Test]
        public void EvictRecordsByAge()
        {
            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(1, 1001L, "iii");
            target.AddEventData(1, 1000L, "b");
            target.AddEventData(1, 1001L, "jjj");

            // when
            var obtained = target.EvictRecordsByAge(1, 1001);

            // then
            Assert.That(obtained, Is.EqualTo(2));
        }

        [Test]
        public void EvictRecordsByNumberDoesNothingAndReturnsZeroIfBeaconIDDoesNotExist()
        {
            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(1, 1001L, "iii");
            target.AddEventData(1, 1000L, "b");
            target.AddEventData(1, 1001L, "jjj");

            // when
            var obtained = target.EvictRecordsByNumber(666, 100);

            // then
            Assert.That(obtained, Is.EqualTo(0));
        }

        [Test]
        public void EvictRecordsByNumber()
        {

            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(1, 1001L, "iii");
            target.AddEventData(1, 1000L, "b");
            target.AddEventData(1, 1001L, "jjj");

            // when
            int obtained = target.EvictRecordsByNumber(1, 2);

            // then
            Assert.That(obtained, Is.EqualTo(2));
        }

        [Test]
        public void IsEmptyGivesTrueIfBeaconDoesNotExistInCache()
        {
            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddActionData(1, 1001L, "iii");
            target.AddEventData(1, 1000L, "b");
            target.AddEventData(1, 1001L, "jjj");

            // then
            Assert.That(target.IsEmpty(666), Is.True);
        }

        [Test]
        public void IsEmptyGivesFalseIfBeaconDataSizeIsNotEqualToZero()
        {

            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddEventData(1, 1000L, "b");

            // then
            Assert.That(target.IsEmpty(1), Is.False);
        }

        [Test]
        public void IsEmptyGivesTrueIfBeaconDoesNotContainActiveData()
        {

            // given
            var target = new BeaconCache();
            target.AddActionData(1, 1000L, "a");
            target.AddEventData(1, 1000L, "b");

            target.GetNextBeaconChunk(1, "prefix", 0, '&');

            // then
            Assert.That(target.IsEmpty(1), Is.True);
        }
    }
}
