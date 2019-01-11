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

using System;
using System.Collections.Generic;
using System.Threading;

namespace Dynatrace.OpenKit.Core.Caching
{
    public class BeaconCache : IBeaconCache
    {
        private readonly ReaderWriterLockSlim globalCacheLock = new ReaderWriterLockSlim();
        private readonly IDictionary<int, BeaconCacheEntry> beacons = new Dictionary<int, BeaconCacheEntry>();
        private long cacheSizeInBytes = 0;

        private EventHandler recordsAdded;
        private readonly object recordsAddedLock = new object();
        
        public HashSet<int> BeaconIDs => CopyBeaconIDs();

        public long NumBytesInCache => Interlocked.Read(ref cacheSizeInBytes);

        public event EventHandler RecordAdded
        {
            add
            {
                lock(recordsAddedLock)
                {
                    recordsAdded += value;
                }
            }
            remove
            {
                lock(recordsAddedLock)
                {
                    recordsAdded -= value;
                }
            }
        }

        public void AddActionData(int beaconID, long timestamp, string data)
        {
            // get a reference to the cache entry
            var entry = GetCachedEntryOrInsert(beaconID);

            // add action data for that beacon
            var record = new BeaconCacheRecord(timestamp, data);

            try
            {
                entry.Lock();
                entry.AddActionData(record);
            }
            finally
            {
                entry.Unlock();
            }

            // update cache stats
            Interlocked.Add(ref cacheSizeInBytes, record.DataSizeInBytes);

            // handle event listeners
            OnDataAdded();
        }

        public void AddEventData(int beaconID, long timestamp, string data)
        {
            // get a reference to the cache entry
            var entry = GetCachedEntryOrInsert(beaconID);

            // add event data for that beacon
            var record = new BeaconCacheRecord(timestamp, data);

            try
            {
                entry.Lock();
                entry.AddEventData(record);
            }
            finally
            {
                entry.Unlock();
            }

            // update cache stats
            Interlocked.Add(ref cacheSizeInBytes, record.DataSizeInBytes);

            // handle event listeners
            OnDataAdded();
        }

        public void DeleteCacheEntry(int beaconID)
        {
            BeaconCacheEntry entry = null;
            try
            {
                globalCacheLock.EnterWriteLock();
                if (beacons.ContainsKey(beaconID))
                {
                    entry = beacons[beaconID];
                    beacons.Remove(beaconID);
                }
            }
            finally
            {
                globalCacheLock.ExitWriteLock();
            }

            if (entry != null)
            {
                Interlocked.Add(ref cacheSizeInBytes, -1L * entry.TotalNumBytes);
            }
        }

        public int EvictRecordsByAge(int beaconID, long minTimestamp)
        {
            var entry = GetCachedEntry(beaconID);
            if (entry == null)
            {
                // already removed
                return 0;
            }

            int numRecordsRemoved;
            try
            {
                entry.Lock();
                numRecordsRemoved = entry.RemoveRecordsOlderThan(minTimestamp);
            }
            finally
            {
                entry.Unlock();
            }

            return numRecordsRemoved;
        }

        public int EvictRecordsByNumber(int beaconID, int numRecords)
        {
            var entry = GetCachedEntry(beaconID);
            if (entry == null)
            {
                // already removed
                return 0;
            }

            int numRecordsRemoved;
            try
            {
                entry.Lock();
                numRecordsRemoved = entry.RemoveOldestRecords(numRecords);
            }
            finally
            {
                entry.Unlock();
            }

            return numRecordsRemoved;
        }

        public string GetNextBeaconChunk(int beaconID, string chunkPrefix, int maxSize, char delimiter)
        {
            var entry = GetCachedEntry(beaconID);
            if (entry == null)
            {
                // a cache entry for the given beaconID does not exist
                return null;
            }

            if (entry.NeedsDataCopyBeforeChunking)
            {
                // both entries are null, prepare data for sending
                long numBytes;
                try
                {
                    entry.Lock();
                    numBytes = entry.TotalNumBytes;
                    entry.CopyDataForChunking();

                }
                finally
                {
                    entry.Unlock();
                }
                // assumption: sending will work fine, and everything we copied will be removed quite soon
                Interlocked.Add(ref cacheSizeInBytes, -1L * numBytes);
            }

            // data for chunking is available
            return entry.GetChunk(chunkPrefix, maxSize, delimiter);
        }

        public bool IsEmpty(int beaconID)
        {
            var entry = GetCachedEntry(beaconID);
            if (entry == null)
            {
                // already removed
                return true;
            }

            bool isEmpty;
            try
            {
                entry.Lock();
                isEmpty = entry.TotalNumBytes == 0;
            }
            finally
            {
                entry.Unlock();
            }

            return isEmpty;
        }

        public void RemoveChunkedData(int beaconID)
        {
            var entry = GetCachedEntry(beaconID);
            if (entry == null)
            {
                // a cache entry for the given beaconID does not exist
                return;
            }

            entry.RemoveDataMarkedForSending();
        }

        public void ResetChunkedData(int beaconID)
        {
            var entry = GetCachedEntry(beaconID);
            if (entry == null)
            {
                // a cache entry for the given beaconID does not exist
                return;
            }

            long numBytes;
            try
            {
                entry.Lock();
                var oldSize = entry.TotalNumBytes;
                entry.ResetDataMarkedForSending();
                var newSize = entry.TotalNumBytes;
                numBytes = newSize - oldSize;
            }
            finally
            {
                entry.Unlock();
            }

            Interlocked.Add(ref cacheSizeInBytes, numBytes);

            // notify observers
            OnDataAdded();
        }

        /// <summary>
        /// Get cached <see cref="BeaconCacheEntry"/> or insert new one if nothing exists for given <paramref name="beaconID"/>.
        /// </summary>
        /// <param name="beaconID">The beacon id to search for.</param>
        /// <returns>The already cached entry or newly created one.</returns>
        private BeaconCacheEntry GetCachedEntryOrInsert(int beaconID)
        {
            // get the appropriate cache entry
            var entry = GetCachedEntry(beaconID);

            if (entry == null)
            {
                try
                {
                    // does not exist, and needs to be inserted
                    globalCacheLock.EnterWriteLock();
                    if (!beacons.ContainsKey(beaconID))
                    {
                        // double check since this could have been added in the mean time
                        entry = new BeaconCacheEntry();
                        beacons.Add(beaconID, entry);
                    }
                    else
                    {
                        entry = beacons[beaconID];
                    }
                }
                finally
                {
                    globalCacheLock.ExitWriteLock();
                }
            }

            return entry;
        }

        /// <summary>
        /// Get cached <see cref="BeaconCacheEntry"/> or <code>null</code> if nothing exists for given <paramref name="beaconID"/>.
        /// </summary>
        /// <param name="beaconID">The beacon id to search for.</param>
        /// <returns>The cached entry or <code>null</code>.</returns>
        private BeaconCacheEntry GetCachedEntry(int beaconID)
        {
            BeaconCacheEntry entry = null;

            // acquuire read lock and get the entry
            try
            {
                globalCacheLock.EnterReadLock();
                if (beacons.TryGetValue(beaconID, out BeaconCacheEntry result))
                {
                    entry = result;
                }
            }
            finally
            {
                globalCacheLock.ExitReadLock();
            }

            return entry;
        }

        /// <summary>
        /// Call this method when something was added (size of cache increased).
        /// </summary>
        private void OnDataAdded()
        {
            EventHandler handler;
            lock(recordsAddedLock)
            {
                handler = recordsAdded;
            }

            handler?.Invoke(this, new EventArgs());
        }

        private HashSet<int> CopyBeaconIDs()
        {
            HashSet<int> result;

            try
            {
                globalCacheLock.EnterReadLock();
                result = new HashSet<int>(beacons.Keys);
            }
            finally
            {
                globalCacheLock.ExitReadLock();
            }

            return result;
        }

        #region for testing purposes

        /// <summary>
        /// Get events stored for given <paramref name="beaconID"/>
        /// </summary>
        /// <remarks>
        /// This method is only for testing purposes and is not thread safe.
        /// </remarks>
        /// <param name="beaconID">The beacon ID for which to get the stored events.</param>
        /// <returns>Events stored for given <paramref name="beaconID"/></returns>
        internal IList<BeaconCacheRecord> GetEvents(int beaconID)
        {
            return GetCachedEntry(beaconID)?.EventData?.AsReadOnly();
        }

        /// <summary>
        /// Get actions stored for given <paramref name="beaconID"/>
        /// </summary>
        /// <remarks>
        /// This method is only for testing purposes and is not thread safe.
        /// </remarks>
        /// <param name="beaconID">The beacon ID for which to get the stored actions.</param>
        /// <returns>Actions stored for given <paramref name="beaconID"/></returns>
        internal IList<BeaconCacheRecord> GetActions(int beaconID)
        {
            return GetCachedEntry(beaconID)?.ActionData?.AsReadOnly();
        }

        /// <summary>
        /// Get events being sent for given <paramref name="beaconID"/>
        /// </summary>
        /// <remarks>
        /// This method is only for testing purposes and is not thread safe.
        /// </remarks>
        /// <param name="beaconID">The beacon ID for which to get the events being sent.</param>
        /// <returns>Events being sent for given <paramref name="beaconID"/></returns>
        internal IList<BeaconCacheRecord> GetEventsBeingSent(int beaconID)
        {
            return GetCachedEntry(beaconID)?.EventDataBeingSent?.AsReadOnly();
        }

        /// <summary>
        /// Get actions being sent for given <paramref name="beaconID"/>
        /// </summary>
        /// <remarks>
        /// This method is only for testing purposes and is not thread safe.
        /// </remarks>
        /// <param name="beaconID">The beacon ID for which to get the actions being sent.</param>
        /// <returns>Actions being sent for given <paramref name="beaconID"/></returns>
        internal IList<BeaconCacheRecord> GetActionsBeingSent(int beaconID)
        {
            return GetCachedEntry(beaconID)?.ActionDataBeingSent?.AsReadOnly();
        }

        #endregion for testing purposes
    }
}
