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

using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Caching
{
    public class BeaconCacheEntryTest
    {
        [Test]
        public void ADefaultConstructedInstanceHasNoData()
        {
            // given
            var target = new BeaconCacheEntry();

            // then
            Assert.That(target.ActionData, Is.Empty);
            Assert.That(target.EventData, Is.Empty);
            Assert.That(target.EventDataBeingSent, Is.Null);
            Assert.That(target.ActionDataBeingSent, Is.Null);
        }

        [Test]
        public void AddingActionData()
        {
            // given
            var dataOne = new BeaconCacheRecord(0L, "foo");
            var dataTwo = new BeaconCacheRecord(1L, "bar");

            var target = new BeaconCacheEntry();

            // when adding first record
            target.AddActionData(dataOne);

            // then
            Assert.That(target.ActionData, Is.EqualTo(new object[] { dataOne }));
            Assert.That(target.EventData, Is.Empty);
            Assert.That(target.EventDataBeingSent, Is.Null);
            Assert.That(target.ActionDataBeingSent, Is.Null);

            // and when adding second record
            target.AddActionData(dataTwo);

            // then
            Assert.That(target.ActionData, Is.EqualTo(new object[] { dataOne, dataTwo }));
            Assert.That(target.EventData, Is.Empty);
            Assert.That(target.EventDataBeingSent, Is.Null);
            Assert.That(target.ActionDataBeingSent, Is.Null);
        }

        [Test]
        public void AddingEventData()
        {
            // given
            var dataOne = new BeaconCacheRecord(0L, "foo");
            var dataTwo = new BeaconCacheRecord(1L, "bar");

            var target = new BeaconCacheEntry();

            // when adding first record
            target.AddEventData(dataOne);

            // then
            Assert.That(target.ActionData, Is.Empty);
            Assert.That(target.EventData, Is.EqualTo(new object[] { dataOne }));
            Assert.That(target.EventDataBeingSent, Is.Null);
            Assert.That(target.ActionDataBeingSent, Is.Null);

            // and when adding second record
            target.AddEventData(dataTwo);

            // then
            Assert.That(target.ActionData, Is.Empty);
            Assert.That(target.EventData, Is.EqualTo(new object[] { dataOne, dataTwo }));
            Assert.That(target.EventDataBeingSent, Is.Null);
            Assert.That(target.ActionDataBeingSent, Is.Null);
        }

        [Test]
        public void CopyDataForChunkingMovesData()
        {
            // given
            var dataOne = new BeaconCacheRecord(0L, "One");
            var dataTwo = new BeaconCacheRecord(0L, "Two");
            var dataThree = new BeaconCacheRecord(1L, "Three");
            var dataFour = new BeaconCacheRecord(1L, "Four");

            var target = new BeaconCacheEntry();
            target.AddEventData(dataOne);
            target.AddEventData(dataFour);
            target.AddActionData(dataTwo);
            target.AddActionData(dataThree);

            // when copying data for later chunking
            target.CopyDataForChunking();

            // then the data was moved
            Assert.That(target.EventDataBeingSent, Is.EqualTo(new[] { dataOne, dataFour }));
            Assert.That(target.ActionDataBeingSent, Is.EqualTo(new[] { dataTwo, dataThree }));
            Assert.That(target.EventData, Is.Empty);
            Assert.That(target.ActionData, Is.Empty);
        }

        [Test]
        public void NeedsDataCopyBeforeChunkingGivesTrueBeforeDataIsCopied()
        {
            // given
            var target = new BeaconCacheEntry();

            var dataOne = new BeaconCacheRecord(0L, "One");
            var dataTwo = new BeaconCacheRecord(0L, "Two");
            var dataThree = new BeaconCacheRecord(1L, "Three");
            var dataFour = new BeaconCacheRecord(1L, "Four");


            target.AddEventData(dataOne);
            target.AddEventData(dataFour);
            target.AddActionData(dataTwo);
            target.AddActionData(dataThree);

            // when, then
            Assert.That(target.NeedsDataCopyBeforeChunking, Is.True);
        }

        [Test]
        public void NeedsDataCopyBeforeChunkingGivesFalseAfterDataHasBeenCopied()
        {
            // given
            var target = new BeaconCacheEntry();

            var dataOne = new BeaconCacheRecord(0L, "One");
            var dataTwo = new BeaconCacheRecord(0L, "Two");
            var dataThree = new BeaconCacheRecord(1L, "Three");
            var dataFour = new BeaconCacheRecord(1L, "Four");


            target.AddEventData(dataOne);
            target.AddEventData(dataFour);
            target.AddActionData(dataTwo);
            target.AddActionData(dataThree);

            target.CopyDataForChunking();

            // when, then
            Assert.That(target.NeedsDataCopyBeforeChunking, Is.False);
        }

        [Test]
        public void NeedsDataCopyBeforeChunkingGivesFalseEvenIfListsAreEmpty()
        {
            // given
            var target = new BeaconCacheEntry();

            target.CopyDataForChunking();

            // when, then
            Assert.That(target.NeedsDataCopyBeforeChunking, Is.False);
        }

        [Test]
        public void GetChunkMarksRetrievedData()
        {
            // given
            var dataOne = new BeaconCacheRecord(0L, "One");
            var dataTwo = new BeaconCacheRecord(0L, "Two");
            var dataThree = new BeaconCacheRecord(1L, "Three");
            var dataFour = new BeaconCacheRecord(1L, "Four");

            var target = new BeaconCacheEntry();
            target.AddEventData(dataOne);
            target.AddEventData(dataFour);
            target.AddActionData(dataTwo);
            target.AddActionData(dataThree);

            target.CopyDataForChunking();

            // when retrieving data
            var obtained = target.GetChunk("prefix", 1024, '&');

            // then
            Assert.That(obtained, Is.EqualTo("prefix&" + dataOne.Data + "&" + dataFour.Data + "&" + dataTwo.Data + "&" + dataThree.Data));
            // and all of them are marked
            Assert.That(dataOne.IsMarkedForSending, Is.True);
            Assert.That(dataTwo.IsMarkedForSending, Is.True);
            Assert.That(dataThree.IsMarkedForSending, Is.True);
            Assert.That(dataFour.IsMarkedForSending, Is.True);
        }

        [Test]
        public void GetChunkGetsChunksFromEventDataBeforeActionData()
        {
            // given
            var dataOne = new BeaconCacheRecord(0L, "One");
            var dataTwo = new BeaconCacheRecord(0L, "Two");
            var dataThree = new BeaconCacheRecord(1L, "Three");
            var dataFour = new BeaconCacheRecord(1L, "Four");

            var target = new BeaconCacheEntry();
            target.AddEventData(dataOne);
            target.AddEventData(dataFour);
            target.AddActionData(dataTwo);
            target.AddActionData(dataThree);

            target.CopyDataForChunking();

            // when getting data to send
            var obtained = target.GetChunk("a", 2, '&');

            // then it's the first event data
            Assert.That(obtained, Is.EqualTo("a&" + dataOne.Data));

            // and when removing already sent data and getting next chunk
            target.RemoveDataMarkedForSending();
            obtained = target.GetChunk("a", 2, '&');

            // then it's second event data
            Assert.That(obtained, Is.EqualTo("a&" + dataFour.Data));

            // and when removing already sent data and getting next chunk
            target.RemoveDataMarkedForSending();
            obtained = target.GetChunk("a", 2, '&');

            // then it's the first action data
            Assert.That(obtained, Is.EqualTo("a&" + dataTwo.Data));

            // and when removing already sent data and getting next chunk
            target.RemoveDataMarkedForSending();
            obtained = target.GetChunk("a", 2, '&');

            // then it's the second action data
            Assert.That(obtained, Is.EqualTo("a&" + dataThree.Data));

            // and when removing already sent data and getting next chunk
            target.RemoveDataMarkedForSending();
            obtained = target.GetChunk("a", 2, '&');

            // then we get an empty string, since all chunks were sent & deleted
            Assert.That(obtained, Is.Empty);
        }

        [Test]
        public void GetChunkGetsAlreadyMarkedData()
        {
            // given
            var dataOne = new BeaconCacheRecord(0L, "One");
            var dataTwo = new BeaconCacheRecord(0L, "Two");
            var dataThree = new BeaconCacheRecord(1L, "Three");
            var dataFour = new BeaconCacheRecord(1L, "Four");

            var target = new BeaconCacheEntry();
            target.AddEventData(dataOne);
            target.AddEventData(dataFour);
            target.AddActionData(dataTwo);
            target.AddActionData(dataThree);

            target.CopyDataForChunking();

            // when getting data to send
            var obtained = target.GetChunk("a", 100, '&');

            // then
            Assert.That(obtained, Is.EqualTo("a&One&Four&Two&Three"));
            Assert.That(dataOne.IsMarkedForSending, Is.True);
            Assert.That(dataTwo.IsMarkedForSending, Is.True);
            Assert.That(dataThree.IsMarkedForSending, Is.True);
            Assert.That(dataFour.IsMarkedForSending, Is.True);

            // when getting data to send once more
            obtained = target.GetChunk("a", 100, '&');

            // then
            Assert.That(obtained, Is.EqualTo("a&One&Four&Two&Three"));
            Assert.That(dataOne.IsMarkedForSending, Is.True);
            Assert.That(dataTwo.IsMarkedForSending, Is.True);
            Assert.That(dataThree.IsMarkedForSending, Is.True);
            Assert.That(dataFour.IsMarkedForSending, Is.True);
        }

        [Test]
        public void GetChunksTakesSizeIntoAccount()
        {
            // given
            var dataOne = new BeaconCacheRecord(0L, "One");
            var dataTwo = new BeaconCacheRecord(0L, "Two");
            var dataThree = new BeaconCacheRecord(1L, "Three");
            var dataFour = new BeaconCacheRecord(1L, "Four");

            var target = new BeaconCacheEntry();
            target.AddEventData(dataOne);
            target.AddEventData(dataFour);
            target.AddActionData(dataTwo);
            target.AddActionData(dataThree);

            target.CopyDataForChunking();

            // when requesting first chunk
            var obtained = target.GetChunk("prefix", 1, '&');

            // then only prefix is returned, since "prefix".length > maxSize (=1)
            Assert.That(obtained, Is.EqualTo("prefix"));

            // and when retrieving something which is one character longer than "prefix"
            obtained = target.GetChunk("prefix", "prefix".Length, '&');

            // then based on the algorithm prefix and first element are retrieved
            Assert.That(obtained, Is.EqualTo("prefix&One"));

            // and when retrieving another chunk
            obtained = target.GetChunk("prefix", "prefix&One".Length, '&');

            // then
            Assert.That(obtained, Is.EqualTo("prefix&One&Four"));
        }

        [Test]
        public void RemoveDataMarkedForSendingReturnsIfDataHasNotBeenCopied()
        {

            // given
            var dataOne = new BeaconCacheRecord(0L, "One");
            var dataTwo = new BeaconCacheRecord(0L, "Two");
            var dataThree = new BeaconCacheRecord(1L, "Three");
            var dataFour = new BeaconCacheRecord(1L, "Four");

            var target = new BeaconCacheEntry();
            target.AddEventData(dataOne);
            target.AddEventData(dataFour);
            target.AddActionData(dataTwo);
            target.AddActionData(dataThree);

            // when
            target.RemoveDataMarkedForSending();

            // then
            Assert.That(target.EventData, Is.EqualTo(new[] { dataOne, dataFour }));
            Assert.That(target.ActionData, Is.EqualTo(new[] { dataTwo, dataThree }));
            Assert.That(target.EventDataBeingSent, Is.Null);
            Assert.That(target.ActionDataBeingSent, Is.Null);
        }

        [Test]
        public void ResetDataMarkedForSendingReturnsIfDataHasNotBeenCopied()
        {

            // given
            var dataOne = new BeaconCacheRecord(0L, "One");
            var dataTwo = new BeaconCacheRecord(0L, "Two");
            var dataThree = new BeaconCacheRecord(1L, "Three");
            var dataFour = new BeaconCacheRecord(1L, "Four");

            var target = new BeaconCacheEntry();
            target.AddEventData(dataOne);
            target.AddEventData(dataFour);
            target.AddActionData(dataTwo);
            target.AddActionData(dataThree);

            // when
            target.ResetDataMarkedForSending();

            // then
            Assert.That(target.EventData, Is.EqualTo(new[] { dataOne, dataFour }));
            Assert.That(target.ActionData, Is.EqualTo(new[] { dataTwo, dataThree }));
            Assert.That(target.EventDataBeingSent, Is.Null);
            Assert.That(target.ActionDataBeingSent, Is.Null);
        }

        [Test]
        public void ResetDataMarkedForSendingMovesPreviouslyCopiedDataBack()
        {
            // given
            var dataOne = new BeaconCacheRecord(0L, "One");
            var dataTwo = new BeaconCacheRecord(0L, "Two");
            var dataThree = new BeaconCacheRecord(1L, "Three");
            var dataFour = new BeaconCacheRecord(1L, "Four");

            var target = new BeaconCacheEntry();
            target.AddEventData(dataOne);
            target.AddEventData(dataFour);
            target.AddActionData(dataTwo);
            target.AddActionData(dataThree);

            target.CopyDataForChunking();

            // when data is reset
            target.ResetDataMarkedForSending();

            // then
            Assert.That(target.EventData, Is.EqualTo(new[] { dataOne, dataFour }));
            Assert.That(target.ActionData, Is.EqualTo(new[] { dataTwo, dataThree }));
            Assert.That(target.EventDataBeingSent, Is.Null);
            Assert.That(target.ActionDataBeingSent, Is.Null);
        }

        [Test]
        public void ResetDataMarkedForSendingResetsMarkedForSendingFlag()
        {
            // given
            var dataOne = new BeaconCacheRecord(0L, "One");
            var dataTwo = new BeaconCacheRecord(0L, "Two");
            var dataThree = new BeaconCacheRecord(1L, "Three");
            var dataFour = new BeaconCacheRecord(1L, "Four");

            var target = new BeaconCacheEntry();
            target.AddEventData(dataOne);
            target.AddEventData(dataFour);
            target.AddActionData(dataTwo);
            target.AddActionData(dataThree);

            target.CopyDataForChunking();

            // when data is retrieved
            target.GetChunk("", 1024, '&');

            // then all records are marked for sending
            Assert.That(dataOne.IsMarkedForSending, Is.True);
            Assert.That(dataTwo.IsMarkedForSending, Is.True);
            Assert.That(dataThree.IsMarkedForSending, Is.True);
            Assert.That(dataFour.IsMarkedForSending, Is.True);

            // and when
            target.ResetDataMarkedForSending();

            // then
            Assert.That(dataOne.IsMarkedForSending, Is.False);
            Assert.That(dataTwo.IsMarkedForSending, Is.False);
            Assert.That(dataThree.IsMarkedForSending, Is.False);
            Assert.That(dataFour.IsMarkedForSending, Is.False);
        }

        [Test]
        public void GetTotalNumberOfBytesCountsAddedRecordBytes()
        {
            // given
            var dataOne = new BeaconCacheRecord(0L, "One");
            var dataTwo = new BeaconCacheRecord(0L, "Two");
            var dataThree = new BeaconCacheRecord(1L, "Three");
            var dataFour = new BeaconCacheRecord(1L, "Four");

            var target = new BeaconCacheEntry();

            // when getting total number of bytes on an empty entry, then
            Assert.That(target.TotalNumBytes, Is.EqualTo(0L));

            // and when adding first entry
            target.AddActionData(dataOne);

            // then
            Assert.That(target.TotalNumBytes, Is.EqualTo(dataOne.DataSizeInBytes));

            // and when adding next entry
            target.AddEventData(dataTwo);

            // then
            Assert.That(target.TotalNumBytes, Is.EqualTo(dataOne.DataSizeInBytes + dataTwo.DataSizeInBytes));

            // and when adding next entry
            target.AddEventData(dataThree);

            // then
            Assert.That(target.TotalNumBytes, Is.EqualTo(dataOne.DataSizeInBytes + dataTwo.DataSizeInBytes + dataThree.DataSizeInBytes));

            // and when adding next entry
            target.AddActionData(dataFour);

            // Assert.That
            Assert.That(target.TotalNumBytes, Is.EqualTo(dataOne.DataSizeInBytes + dataTwo.DataSizeInBytes
                + dataThree.DataSizeInBytes + dataFour.DataSizeInBytes));
        }

        [Test]
        public void RemoveRecordsOlderThanRemovesNothingIfNoActionOrEventDataExists()
        {
            // given
            var target = new BeaconCacheEntry();

            // when
            var obtained = target.RemoveRecordsOlderThan(0);

            // then
            Assert.That(obtained, Is.EqualTo(0));
        }

        [Test]
        public void RemoveRecordsOlderThanRemovesRecordsFromActionData()
        {
            // given
            var dataOne = new BeaconCacheRecord(4000L, "One");
            var dataTwo = new BeaconCacheRecord(3000L, "Two");
            var dataThree = new BeaconCacheRecord(2000L, "Three");
            var dataFour = new BeaconCacheRecord(1000L, "Four");

            var target = new BeaconCacheEntry();
            target.AddActionData(dataOne);
            target.AddActionData(dataTwo);
            target.AddActionData(dataThree);
            target.AddActionData(dataFour);

            // when removing everything older than 3000
            var obtained = target.RemoveRecordsOlderThan(dataTwo.Timestamp);

            // then
            Assert.That(obtained, Is.EqualTo(2)); // two were removed
            Assert.That(target.ActionData, Is.EqualTo(new[] { dataOne, dataTwo }));
        }

        [Test]
        public void RemoveRecordsOlderThanRemovesRecordsFromEventData()
        {

            // given
            var dataOne = new BeaconCacheRecord(4000L, "One");
            var dataTwo = new BeaconCacheRecord(3000L, "Two");
            var dataThree = new BeaconCacheRecord(2000L, "Three");
            var dataFour = new BeaconCacheRecord(1000L, "Four");

            var target = new BeaconCacheEntry();
            target.AddEventData(dataOne);
            target.AddEventData(dataTwo);
            target.AddEventData(dataThree);
            target.AddEventData(dataFour);

            // when removing everything older than 3000
            var obtained = target.RemoveRecordsOlderThan(dataTwo.Timestamp);

            // then
            Assert.That(obtained, Is.EqualTo(2)); // two were removed
            Assert.That(target.EventData, Is.EqualTo(new[] { dataOne, dataTwo }));
        }

        [Test]
        public void RemoveOldestRecordsRemovesNothingIfEntryIsEmpty()
        {
            // given
            var target = new BeaconCacheEntry();

            // when
            var obtained = target.RemoveOldestRecords(1);

            // then
            Assert.That(obtained, Is.EqualTo(0));
        }

        [Test]
        public void RemoveOldestRecordsRemovesActionDataIfEventDataIsEmpty()
        {
            // given
            var dataOne = new BeaconCacheRecord(4000L, "One");
            var dataTwo = new BeaconCacheRecord(3000L, "Two");
            var dataThree = new BeaconCacheRecord(2000L, "Three");
            var dataFour = new BeaconCacheRecord(1000L, "Four");

            var target = new BeaconCacheEntry();
            target.AddActionData(dataOne);
            target.AddActionData(dataTwo);
            target.AddActionData(dataThree);
            target.AddActionData(dataFour);

            // when
            var obtained = target.RemoveOldestRecords(2);

            // then
            Assert.That(obtained, Is.EqualTo(2)); // two were removed
            Assert.That(target.ActionData, Is.EqualTo(new[] { dataThree, dataFour }));
        }

        [Test]
        public void RemoveOldestRecordsRemovesEventDataIfActionDataIsEmpty()
        {
            // given
            var dataOne = new BeaconCacheRecord(4000L, "One");
            var dataTwo = new BeaconCacheRecord(3000L, "Two");
            var dataThree = new BeaconCacheRecord(2000L, "Three");
            var dataFour = new BeaconCacheRecord(1000L, "Four");

            var target = new BeaconCacheEntry();
            target.AddEventData(dataOne);
            target.AddEventData(dataTwo);
            target.AddEventData(dataThree);
            target.AddEventData(dataFour);

            // when
            var obtained = target.RemoveOldestRecords(2);

            // then
            Assert.That(obtained, Is.EqualTo(2)); // two were removed
            Assert.That(target.EventData, Is.EqualTo(new[] { dataThree, dataFour }));
        }

        [Test]
        public void RemoveOldestRecordsComparesTopActionAndEventDataAndRemovesOldest()
        {

            // given
            var dataOne = new BeaconCacheRecord(1000, "One");
            var dataTwo = new BeaconCacheRecord(1100L, "Two");
            var dataThree = new BeaconCacheRecord(950L, "Three");
            var dataFour = new BeaconCacheRecord(1200L, "Four");

            var target = new BeaconCacheEntry();
            target.AddEventData(dataOne);
            target.AddActionData(dataTwo);
            target.AddActionData(dataThree);
            target.AddEventData(dataFour);

            // when
            var obtained = target.RemoveOldestRecords(1);

            // then
            Assert.That(obtained, Is.EqualTo(1));
            Assert.That(target.ActionData, Is.EqualTo(new[] { dataTwo, dataThree }));
            Assert.That(target.EventData, Is.EqualTo(new[] { dataFour }));

            // when removing the next two
            obtained = target.RemoveOldestRecords(2);

            // then
            Assert.That(obtained, Is.EqualTo(2));
            Assert.That(target.ActionData, Is.Empty);
            Assert.That(target.EventData, Is.EqualTo(new[] { dataFour }));
        }

        [Test]
        public void RemoveOldestRecordsRemovesEventDataIfTopEventDataAndActionDataHaveSameTimestamp()
        {
            // given
            var dataOne = new BeaconCacheRecord(1000, "One");
            var dataTwo = new BeaconCacheRecord(1100L, "Two");
            var dataThree = new BeaconCacheRecord(dataOne.Timestamp, "Three");
            var dataFour = new BeaconCacheRecord(dataTwo.Timestamp, "Four");

            var target = new BeaconCacheEntry();
            target.AddEventData(dataOne);
            target.AddEventData(dataTwo);
            target.AddActionData(dataThree);
            target.AddActionData(dataFour);

            // when
            var obtained = target.RemoveOldestRecords(1);

            // then
            Assert.That(obtained, Is.EqualTo(1));
            Assert.That(target.ActionData, Is.EqualTo(new[] { dataThree, dataFour }));
            Assert.That(target.EventData, Is.EqualTo(new[] { dataTwo }));
        }

        [Test]
        public void RemoveOldestRecordsStopsIfListsAreEmpty()
        {
            // given
            var dataOne = new BeaconCacheRecord(4000L, "One");
            var dataTwo = new BeaconCacheRecord(3000L, "Two");
            var dataThree = new BeaconCacheRecord(2000L, "Three");
            var dataFour = new BeaconCacheRecord(1000L, "Four");

            var target = new BeaconCacheEntry();
            target.AddEventData(dataOne);
            target.AddEventData(dataTwo);
            target.AddEventData(dataThree);
            target.AddEventData(dataFour);

            // when
            var obtained = target.RemoveOldestRecords(100);

            // then
            Assert.That(obtained, Is.EqualTo(4));
            Assert.That(target.EventData, Is.Empty);
            Assert.That(target.ActionData, Is.Empty);
        }

        [Test]
        public void RemoveRecordsOlderThanDoesNotRemoveAnythingFromEventAndActionsBeingSent()
        {
            // given
            var dataOne = new BeaconCacheRecord(1000L, "One");
            var dataTwo = new BeaconCacheRecord(1500L, "Two");
            var dataThree = new BeaconCacheRecord(2000L, "Three");
            var dataFour = new BeaconCacheRecord(2500L, "Four");

            var target = new BeaconCacheEntry();
            target.AddEventData(dataOne);
            target.AddEventData(dataFour);
            target.AddActionData(dataTwo);
            target.AddActionData(dataThree);

            target.CopyDataForChunking();

            // when
            var obtained = target.RemoveRecordsOlderThan(10000);

            // then
            Assert.That(obtained, Is.EqualTo(0));
            Assert.That(target.EventDataBeingSent, Is.EqualTo(new[] { dataOne, dataFour }));
            Assert.That(target.ActionDataBeingSent, Is.EqualTo(new[] { dataTwo, dataThree }));
        }

        [Test]
        public void RemoveOldestRecordsDoesNotRemoveAnythingFromEventAndActionsBeingSent() {

            // given
            var dataOne = new BeaconCacheRecord(1000L, "One");
            var dataTwo = new BeaconCacheRecord(1500L, "Two");
            var dataThree = new BeaconCacheRecord(2000L, "Three");
            var dataFour = new BeaconCacheRecord(2500L, "Four");

            var target = new BeaconCacheEntry();
            target.AddEventData(dataOne);
            target.AddEventData(dataFour);
            target.AddActionData(dataTwo);
            target.AddActionData(dataThree);

            target.CopyDataForChunking();

            // when
            var obtained = target.RemoveOldestRecords(10000);

            // then
            Assert.That(obtained, Is.EqualTo(0));
            Assert.That(target.EventDataBeingSent, Is.EqualTo(new[] { dataOne, dataFour }));
            Assert.That(target.ActionDataBeingSent, Is.EqualTo(new[] { dataTwo, dataThree }));
        }
    }
}
