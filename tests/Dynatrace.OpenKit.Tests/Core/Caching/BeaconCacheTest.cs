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

using System.Collections.Generic;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Protocol;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Caching
{
    public class BeaconCacheTest
    {
        private ILogger logger;

        [SetUp]
        public void SetUp()
        {
            logger = Substitute.For<ILogger>();
        }

        [Test]
        public void ADefaultConstructedCacheDoesNotContainBeacons()
        {
            // given
            var target = new BeaconCache(logger);

            // then
            Assert.That(target.BeaconKeys, Is.Empty);
            Assert.That(target.NumBytesInCache, Is.EqualTo(0L));
        }

        [Test]
        public void AddEventDataAddsBeaconIdToCache()
        {
            // given
            var target = new BeaconCache(logger);
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(2, 0);

            // when adding beacon with id 1
            target.AddEventData(keyOne, 1000L, "a");

            // then
            Assert.That(target.BeaconKeys, Is.EqualTo(new HashSet<BeaconKey> { keyOne }));
            Assert.That(target.GetEvents(keyOne), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a") }));

            // and when adding beacon with id 2
            target.AddEventData(keyTwo, 1100L, "b");

            // then
            Assert.That(target.BeaconKeys, Is.EqualTo(new HashSet<BeaconKey> { keyOne, keyTwo }));
            Assert.That(target.GetEvents(keyOne), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a") }));
            Assert.That(target.GetEvents(keyTwo), Is.EqualTo(new[] { new BeaconCacheRecord(1100L, "b") }));
        }

        [Test]
        public void AddEventDataAddsDataToAlreadyExistingBeaconId()
        {
            // given
            var target = new BeaconCache(logger);
            BeaconKey key = new BeaconKey(1, 0);

            // when adding beacon with id 1
            target.AddEventData(key, 1000L, "a");

            // then
            Assert.That(target.BeaconKeys, Is.EqualTo(new HashSet<BeaconKey> { key }));
            Assert.That(target.GetEvents(key), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a") }));

            // and when adding other data with beacon id 1
            target.AddEventData(key, 1100L, "bc");

            // then
            Assert.That(target.BeaconKeys, Is.EqualTo(new HashSet<BeaconKey> { key }));
            Assert.That(target.GetEvents(key), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a"), new BeaconCacheRecord(1100L, "bc") }));
        }

        [Test]
        public void AddEventDataIncreasesCacheSize()
        {
            // given
            var target = new BeaconCache(logger);
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(42, 0);

            // when adding some data
            target.AddEventData(keyOne, 1000L, "a");
            target.AddEventData(keyTwo, 1000L, "z");
            target.AddEventData(keyOne, 1000L, "iii");

            // then
            Assert.That(target.NumBytesInCache, Is.EqualTo(new BeaconCacheRecord(1000L, "a").DataSizeInBytes
                                                            + new BeaconCacheRecord(1000L, "z").DataSizeInBytes
                                                            + new BeaconCacheRecord(1000L, "iii").DataSizeInBytes));
        }

        [Test]
        public void AddEventDataRaisesEvent()
        {
            // given
            var target = new BeaconCache(logger);
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(666, 0);

            var notifyCount = 0;
            target.RecordAdded += (s, a) => { notifyCount += 1; };

            // when adding an element
            target.AddEventData(keyOne, 1000L, "a");

            // then verify event got raised
            Assert.That(notifyCount, Is.EqualTo(1));

            // when adding some more data
            target.AddEventData(keyOne, 1100L, "b");
            target.AddEventData(keyTwo, 1200L, "xyz");

            // then verify event got raised another two times
            Assert.That(notifyCount, Is.EqualTo(3));
        }

        [Test]
        public void AddActionDataAddsBeaconIdToCache()
        {
            // given
            var target = new BeaconCache(logger);
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(2, 0);

            // when adding beacon with id 1
            target.AddActionData(keyOne, 1000L, "a");

            // then
            Assert.That(target.BeaconKeys, Is.EqualTo(new HashSet<BeaconKey> { keyOne }));
            Assert.That(target.GetActions(keyOne), Is.EqualTo(new [] { new BeaconCacheRecord(1000L, "a") }));

            // and when adding beacon with id 2
            target.AddActionData(keyTwo, 1100L, "b");

            // then
            Assert.That(target.BeaconKeys, Is.EqualTo(new HashSet<BeaconKey> { keyOne, keyTwo }));
            Assert.That(target.GetActions(keyOne), Is.EqualTo(new [] { new BeaconCacheRecord(1000L, "a") }));
            Assert.That(target.GetActions(keyTwo), Is.EqualTo(new [] { new BeaconCacheRecord(1100L, "b") }));
        }

        [Test]
        public void AddActionDataAddsDataToAlreadyExistingBeaconId()
        {
            // given
            var target = new BeaconCache(logger);
            var key = new BeaconKey(1, 0);

            // when adding beacon with id 1
            target.AddActionData(key, 1000L, "a");

            // then
            Assert.That(target.BeaconKeys, Is.EqualTo(new HashSet<BeaconKey> { key }));
            Assert.That(target.GetActions(key), Is.EqualTo(new [] { new BeaconCacheRecord(1000L, "a") }));

            // and when adding other data with beacon id 1
            target.AddActionData(key, 1100L, "bc");

            // then
            Assert.That(target.BeaconKeys, Is.EqualTo(new HashSet<BeaconKey> { key }));
            Assert.That(target.GetActions(key), Is.EqualTo(new [] { new BeaconCacheRecord(1000L, "a"), new BeaconCacheRecord(1100L, "bc") }));
        }

        [Test]
        public void AddActionDataIncreasesCacheSize()
        {
            // given
            var target = new BeaconCache(logger);
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(42, 0);

            // when adding some data
            target.AddActionData(keyOne, 1000L, "a");
            target.AddActionData(keyTwo, 1000L, "z");
            target.AddActionData(keyOne, 1000L, "iii");

            // then
            Assert.That(target.NumBytesInCache, Is.EqualTo(new BeaconCacheRecord(1000L, "a").DataSizeInBytes
                                                            + new BeaconCacheRecord(1000L, "z").DataSizeInBytes
                                                            + new BeaconCacheRecord(1000L, "iii").DataSizeInBytes));
        }

        [Test]
        public void AddActionDataRaisesEvent()
        {
            // given
            var target = new BeaconCache(logger);
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(42, 0);

            var notifyCount = 0;
            target.RecordAdded += (s, a) => { notifyCount += 1; };

            // when adding an element
            target.AddActionData(keyOne, 1000L, "a");

            // then verify event was raised
            Assert.That(notifyCount, Is.EqualTo(1));

            // when adding some more data
            target.AddActionData(keyOne, 1100L, "b");
            target.AddActionData(keyTwo, 1200L, "xyz");

            // then verify event got raised another two times
            Assert.That(notifyCount, Is.EqualTo(3));
        }

        [Test]
        public void DeleteCacheEntryRemovesTheGivenBeacon()
        {
            // given
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(42, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(keyOne, 1000L, "a");
            target.AddActionData(keyTwo, 1000L, "z");
            target.AddEventData(keyOne, 1000L, "iii");

            // when removing beacon with id 1
            target.DeleteCacheEntry(keyOne);

            // then
            Assert.That(target.BeaconKeys, Is.EqualTo(new HashSet<BeaconKey> { keyTwo }));

            // and when removing beacon with id 42
            target.DeleteCacheEntry(keyTwo);

            // then
            Assert.That(target.BeaconKeys, Is.Empty);
        }

        [Test]
        public void DeleteCacheEntryDecrementsCacheSize()
        {
            // given
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(42, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(keyOne, 1000L, "a");
            target.AddActionData(keyTwo, 1000L, "z");
            target.AddEventData(keyOne, 1000L, "iii");

            // when deleting entry with beacon id 42
            target.DeleteCacheEntry(keyTwo);

            // then
            Assert.That(target.NumBytesInCache, Is.EqualTo(new BeaconCacheRecord(1000L, "a").DataSizeInBytes
                                                            + new BeaconCacheRecord(1000L, "iii").DataSizeInBytes));
        }

        [Test]
        public void DeleteCacheEntryDoesNotRaiseEvent()
        {
            // given
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(42, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(keyOne, 1000L, "a");
            target.AddActionData(keyTwo, 1000L, "z");
            target.AddEventData(keyOne, 1000L, "iii");

            var notifyCount = 0;
            target.RecordAdded += (s, a) => { notifyCount += 1; };

            // when deleting both entries
            target.DeleteCacheEntry(keyOne);
            target.DeleteCacheEntry(keyTwo);

            // then
            Assert.That(notifyCount, Is.EqualTo(0));
        }

        [Test]
        public void DeleteCacheEntriesDoesNothingIfGivenBeaconIdIsNotInCache()
        {
            // given
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(42, 0);
            var keyThree = new BeaconKey(666, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(keyOne, 1000L, "a");
            target.AddActionData(keyTwo, 1000L, "z");
            target.AddEventData(keyOne, 1000L, "iii");

            var notifyCount = 0;
            target.RecordAdded += (s, a) => { notifyCount += 1; };

            var cachedSize = target.NumBytesInCache;

            // when
            target.DeleteCacheEntry(keyThree);

            // then
            Assert.That(target.BeaconKeys, Is.EqualTo(new HashSet<BeaconKey> { keyOne, keyTwo }));
            Assert.That(target.NumBytesInCache, Is.EqualTo(cachedSize));
            Assert.That(notifyCount, Is.EqualTo(0));
        }

        [Test]
        public void GetNextBeaconChunkReturnsNullIfGivenBeaconIdDoesNotExist()
        {
            // given
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(42, 0);
            var keyThree = new BeaconKey(666, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(keyOne, 1000L, "a");
            target.AddActionData(keyTwo, 1000L, "z");
            target.AddEventData(keyOne, 1000L, "iii");

            // when
            var obtained = target.GetNextBeaconChunk(keyThree, "", 1024, '&');

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void GetNextBeaconChunkCopiesDataForSending()
        {
            // given
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(42, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(keyOne, 1000L, "a");
            target.AddActionData(keyOne, 1001L, "iii");
            target.AddActionData(keyTwo, 2000L, "z");
            target.AddEventData(keyOne, 1000L, "b");
            target.AddEventData(keyOne, 1001L, "jjj");

            // when
            var obtained = target.GetNextBeaconChunk(keyOne, "prefix", 0, '&');

            // then
            Assert.That(obtained, Is.EqualTo("prefix"));

            Assert.That(target.GetActions(keyOne), Is.Empty);
            Assert.That(target.GetEvents(keyOne), Is.Empty);
            Assert.That(target.GetActionsBeingSent(keyOne), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a"), new BeaconCacheRecord(1001L, "iii") }));
            Assert.That(target.GetEventsBeingSent(keyOne), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "b"), new BeaconCacheRecord(1001L, "jjj") }));
        }

        [Test]
        public void GetNextBeaconChunkDecreasesBeaconCacheSize()
        {
            // given
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(42, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(keyOne, 1000L, "a");
            target.AddActionData(keyOne, 1001L, "iii");
            target.AddActionData(keyTwo, 2000L, "z");
            target.AddEventData(keyOne, 1000L, "b");
            target.AddEventData(keyOne, 1001L, "jjj");

            // when
            target.GetNextBeaconChunk(keyOne, "prefix", 0, '&');

            // cache stats are also adjusted
            Assert.That(target.NumBytesInCache, Is.EqualTo(new BeaconCacheRecord(2000L, "z").DataSizeInBytes));
        }

        [Test]
        public void GetNextBeaconChunkRetrievesNextChunk()
        {
            // given
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(42, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(keyOne, 1000L, "a");
            target.AddActionData(keyOne, 1001L, "iii");
            target.AddActionData(keyTwo, 2000L, "z");
            target.AddEventData(keyOne, 1000L, "b");
            target.AddEventData(keyOne, 1001L, "jjj");

            // when retrieving the first chunk
            var obtained = target.GetNextBeaconChunk(keyOne, "prefix", 10, '&');

            // then
            Assert.That(obtained, Is.EqualTo("prefix&b&jjj"));

            // then
            Assert.That(target.GetActionsBeingSent(keyOne), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a"), new BeaconCacheRecord(1001L, "iii") }));
            var expectedEventRecords = new[] { new BeaconCacheRecord(1000L, "b"), new BeaconCacheRecord(1001L, "jjj") };
            foreach (var record in expectedEventRecords)
            {
                record.MarkForSending();
            }
            Assert.That(target.GetEventsBeingSent(keyOne), Is.EqualTo(expectedEventRecords));
        }

        [Test]
        public void RemoveChunkedDataClearsAlreadyRetrievedChunks()
        {
            // given
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(42, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(keyOne, 1000L, "a");
            target.AddActionData(keyOne, 1001L, "iii");
            target.AddActionData(keyTwo, 2000L, "z");
            target.AddEventData(keyOne, 1000L, "b");
            target.AddEventData(keyOne, 1001L, "jjj");

            // when retrieving the first chunk and removing retrieved chunks
            var obtained = target.GetNextBeaconChunk(keyOne, "prefix", 10, '&');
            target.RemoveChunkedData(keyOne);

            // then
            Assert.That(obtained, Is.EqualTo("prefix&b&jjj"));

            Assert.That(target.GetActionsBeingSent(keyOne), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a"), new BeaconCacheRecord(1001L, "iii") }));
            Assert.That(target.GetEventsBeingSent(keyOne), Is.Empty);

            // when retrieving the second chunk and removing retrieved chunks
            obtained = target.GetNextBeaconChunk(keyOne, "prefix", 10, '&');
            target.RemoveChunkedData(keyOne);

            // then
            Assert.That(obtained, Is.EqualTo("prefix&a&iii"));

            Assert.That(target.GetActionsBeingSent(keyOne), Is.Empty);
            Assert.That(target.GetEventsBeingSent(keyOne), Is.Empty);
        }

        [Test]
        public void RemoveChunkedDataDoesNothingIfCalledWithNonExistingBeaconId()
        {
            // given
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(42, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(keyOne, 1000L, "a");
            target.AddActionData(keyOne, 1001L, "iii");
            target.AddActionData(keyTwo, 2000L, "z");
            target.AddEventData(keyOne, 1000L, "b");
            target.AddEventData(keyOne, 1001L, "jjj");

            // when retrieving the first chunk and removing the wrong beacon chunk
            target.GetNextBeaconChunk(keyOne, "prefix", 10, '&');
            target.RemoveChunkedData(keyTwo);

            // then
            Assert.That(target.GetActionsBeingSent(keyOne), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a"), new BeaconCacheRecord(1001L, "iii") }));
            var expectedEventRecords = new[] { new BeaconCacheRecord(1000L, "b"), new BeaconCacheRecord(1001L, "jjj") };
            foreach (var record in expectedEventRecords)
            {
                record.MarkForSending();
            }
            Assert.That(target.GetEventsBeingSent(keyOne), Is.EqualTo(expectedEventRecords));
        }

        [Test]
        public void ResetChunkedRestoresData()
        {
            // given
            var key = new BeaconKey(1, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(key, 1000L, "a");
            target.AddActionData(key, 1001L, "iii");
            target.AddEventData(key, 1000L, "b");
            target.AddEventData(key, 1001L, "jjj");

            // do same step we'd do when we send the
            target.GetNextBeaconChunk(key, "prefix", 10, '&');

            // data has been copied, but still add some new event & action data
            target.AddActionData(key, 6666L, "123");
            target.AddEventData(key, 6666L, "987");

            // and when resetting the previously copied data
            target.ResetChunkedData(key);

            // then
            Assert.That(target.GetActionsBeingSent(key), Is.Null);
            Assert.That(target.GetEventsBeingSent(key), Is.Null);
            Assert.That(target.GetActions(key), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "a"), new BeaconCacheRecord(1001L, "iii"), new BeaconCacheRecord(6666L, "123") }));
            Assert.That(target.GetEvents(key), Is.EqualTo(new[] { new BeaconCacheRecord(1000L, "b"), new BeaconCacheRecord(1001L, "jjj"), new BeaconCacheRecord(6666L, "987") }));
        }

        [Test]
        public void ResetChunkedRestoresCacheSize()
        {
            // given
            var key = new BeaconKey(1, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(key, 1000L, "a");
            target.AddActionData(key, 1001L, "iii");
            target.AddEventData(key, 1000L, "b");
            target.AddEventData(key, 1001L, "jjj");

            // do same step we'd do when we send the
            target.GetNextBeaconChunk(key, "prefix", 10, '&');

            // data has been copied, but still add some new event & action data
            target.AddActionData(key, 6666L, "123");
            target.AddEventData(key, 6666L, "987");

            // and when resetting the previously copied data
            target.ResetChunkedData(key);

            // then
            Assert.That(target.NumBytesInCache, Is.EqualTo(28L));
        }

        [Test]
        public void ResetChunkedRaisesEvent()
        {
            // given
            var key = new BeaconKey(1, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(key, 1000L, "a");
            target.AddActionData(key, 1001L, "iii");
            target.AddEventData(key, 1000L, "b");
            target.AddEventData(key, 1001L, "jjj");

            // do same step we'd do when we send the
            target.GetNextBeaconChunk(key, "prefix", 10, '&');

            // data has been copied, but still add some new event & action data
            target.AddActionData(key, 6666L, "123");
            target.AddEventData(key, 6666L, "987");

            var notifyCount = 0;
            target.RecordAdded += (s, a) => { notifyCount += 1; };

            // and when resetting the previously copied data
            target.ResetChunkedData(key);

            // then
            Assert.That(notifyCount, Is.EqualTo(1));
        }

        [Test]
        public void ResetChunkedDoesNothingIfEntryDoesNotExist()
        {
            // given
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(666, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(keyOne, 1000L, "a");
            target.AddActionData(keyOne, 1001L, "iii");
            target.AddEventData(keyOne, 1000L, "b");
            target.AddEventData(keyOne, 1001L, "jjj");

            // do same step we'd do when we send the
            target.GetNextBeaconChunk(keyOne, "prefix", 10, '&');

            // data has been copied, but still add some new event & action data
            target.AddActionData(keyOne, 6666L, "123");
            target.AddEventData(keyOne, 6666L, "987");

            var notifyCount = 0;
            target.RecordAdded += (s, a) => { notifyCount += 1; };

            // and when resetting the previously copied data
            target.ResetChunkedData(keyTwo);

            // then
            Assert.That(target.NumBytesInCache, Is.EqualTo(12L));
            Assert.That(notifyCount, Is.EqualTo(0));
        }

        [Test]
        public void EvictRecordsByAgeDoesNothingAndReturnsZeroIfBeaconIdDoesNotExist()
        {
            // given
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(666, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(keyOne, 1000L, "a");
            target.AddActionData(keyOne, 1001L, "iii");
            target.AddEventData(keyOne, 1000L, "b");
            target.AddEventData(keyOne, 1001L, "jjj");

            // when
            var obtained = target.EvictRecordsByAge(keyTwo, 0);

            // then
            Assert.That(obtained, Is.EqualTo(0));
        }

        [Test]
        public void EvictRecordsByAge()
        {
            // given
            var key = new BeaconKey(1, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(key, 1000L, "a");
            target.AddActionData(key, 1001L, "iii");
            target.AddEventData(key, 1000L, "b");
            target.AddEventData(key, 1001L, "jjj");

            // when
            var obtained = target.EvictRecordsByAge(key, 1001);

            // then
            Assert.That(obtained, Is.EqualTo(2));
        }

        [Test]
        public void EvictRecordsByNumberDoesNothingAndReturnsZeroIfBeaconIdDoesNotExist()
        {
            // given
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(666, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(keyOne, 1000L, "a");
            target.AddActionData(keyOne, 1001L, "iii");
            target.AddEventData(keyOne, 1000L, "b");
            target.AddEventData(keyOne, 1001L, "jjj");

            // when
            var obtained = target.EvictRecordsByNumber(keyTwo, 100);

            // then
            Assert.That(obtained, Is.EqualTo(0));
        }

        [Test]
        public void EvictRecordsByNumber()
        {
            // given
            var key = new BeaconKey(1, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(key, 1000L, "a");
            target.AddActionData(key, 1001L, "iii");
            target.AddEventData(key, 1000L, "b");
            target.AddEventData(key, 1001L, "jjj");

            // when
            var obtained = target.EvictRecordsByNumber(key, 2);

            // then
            Assert.That(obtained, Is.EqualTo(2));
        }

        [Test]
        public void IsEmptyGivesTrueIfBeaconDoesNotExistInCache()
        {
            // given
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(666, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(keyOne, 1000L, "a");
            target.AddActionData(keyOne, 1001L, "iii");
            target.AddEventData(keyOne, 1000L, "b");
            target.AddEventData(keyOne, 1001L, "jjj");

            // then
            Assert.That(target.IsEmpty(keyTwo), Is.True);
        }

        [Test]
        public void IsEmptyGivesFalseIfBeaconDataSizeIsNotEqualToZero()
        {
            // given
            var key = new BeaconKey(1, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(key, 1000L, "a");
            target.AddEventData(key, 1000L, "b");

            // then
            Assert.That(target.IsEmpty(key), Is.False);
        }

        [Test]
        public void IsEmptyGivesTrueIfBeaconDoesNotContainActiveData()
        {
            // given
            var key = new BeaconKey(1, 0);

            var target = new BeaconCache(logger);
            target.AddActionData(key, 1000L, "a");
            target.AddEventData(key, 1000L, "b");

            target.GetNextBeaconChunk(key, "prefix", 0, '&');

            // then
            Assert.That(target.IsEmpty(key), Is.True);
        }
    }
}
