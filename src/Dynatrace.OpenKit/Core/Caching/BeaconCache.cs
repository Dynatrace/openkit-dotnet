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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Util;

namespace Dynatrace.OpenKit.Core.Caching
{
    public class BeaconCache : IBeaconCache
    {
        private readonly ILogger logger;
        private readonly ReaderWriterLockSlim globalCacheLock = new ReaderWriterLockSlim();
        private readonly IDictionary<BeaconKey, BeaconCacheEntry> beacons = new Dictionary<BeaconKey, BeaconCacheEntry>();
        private long cacheSizeInBytes = 0;

        private EventHandler recordsAdded;
        private readonly object recordsAddedLock = new object();

        public HashSet<BeaconKey> BeaconKeys => CopyBeaconKeys();

        public long NumBytesInCache => Interlocked.Read(ref cacheSizeInBytes);

        public BeaconCache(ILogger logger)
        {
            this.logger = logger;
        }

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

        public void AddActionData(BeaconKey beaconKey, long timestamp, string data)
        {
            if (logger.IsDebugEnabled)
            {
                var logString = new StringBuilder(GetType().Name)
                    .Append(" AddActionData(sn=").Append(beaconKey.BeaconId.ToInvariantString())
                    .Append(", seq=").Append(beaconKey.BeaconSeqNo.ToInvariantString())
                    .Append(", timestamp=").Append(timestamp.ToInvariantString())
                    .Append(", data='").Append(data).Append("')").ToString();

                logger.Debug(logString);
            }
            // get a reference to the cache entry
            var entry = GetCachedEntryOrInsert(beaconKey);

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

        public void AddEventData(BeaconKey beaconKey, long timestamp, string data)
        {
            if(logger.IsDebugEnabled)
            {
                var logString = new StringBuilder(GetType().Name)
                    .Append(" AddEventData(sn=").Append(beaconKey.BeaconId.ToInvariantString())
                    .Append(", seq=").Append(beaconKey.BeaconSeqNo.ToInvariantString())
                    .Append(", timestamp=").Append(timestamp.ToInvariantString())
                    .Append(", data='").Append(data).Append("')").ToString();

                logger.Debug(logString);
            }
            // get a reference to the cache entry
            var entry = GetCachedEntryOrInsert(beaconKey);

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

        public void DeleteCacheEntry(BeaconKey beaconKey)
        {
            if(logger.IsDebugEnabled)
            {
                var logString = new StringBuilder(GetType().Name)
                    .Append(" DeleteCacheEntry(sn=").Append(beaconKey.BeaconId.ToInvariantString())
                    .Append(", seq=").Append(beaconKey.BeaconSeqNo.ToInvariantString())
                    .Append(")")
                    .ToString();

                logger.Debug(logString);
            }
            BeaconCacheEntry entry = null;
            try
            {
                globalCacheLock.EnterWriteLock();
                if (beacons.ContainsKey(beaconKey))
                {
                    entry = beacons[beaconKey];
                    beacons.Remove(beaconKey);
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

        public int EvictRecordsByAge(BeaconKey beaconKey, long minTimestamp)
        {
            var entry = GetCachedEntry(beaconKey);
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

            if(logger.IsDebugEnabled)
            {
                var logString = new StringBuilder(GetType().Name)
                    .Append(" EvictRecordsByAge(sn=").Append(beaconKey.BeaconId.ToInvariantString())
                    .Append(", seq=").Append(beaconKey.BeaconSeqNo.ToInvariantString())
                    .Append(", minTimestamp=").Append(minTimestamp.ToInvariantString())
                    .Append(")")
                    .Append(" has evicted ").Append(numRecordsRemoved.ToInvariantString()).Append(" records")
                    .ToString();

                logger.Debug(logString);
            }

            return numRecordsRemoved;
        }

        public int EvictRecordsByNumber(BeaconKey beaconKey, int numRecords)
        {
            var entry = GetCachedEntry(beaconKey);
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

            if (logger.IsDebugEnabled)
            {
                var logString = new StringBuilder(GetType().Name)
                    .Append(" EvictRecordsByNumber(sn=").Append(beaconKey.BeaconId.ToInvariantString())
                    .Append(", seq=").Append(beaconKey.BeaconSeqNo.ToInvariantString())
                    .Append(", numRecords=").Append(numRecords.ToInvariantString())
                    .Append(")")
                    .Append(" has evicted ").Append(numRecordsRemoved.ToInvariantString()).Append(" records")
                    .ToString();

                logger.Debug(logString);
            }

            return numRecordsRemoved;
        }

        public virtual string GetNextBeaconChunk(BeaconKey beaconKey, string chunkPrefix, int maxSize, char delimiter)
        {
            var entry = GetCachedEntry(beaconKey);
            if (entry == null)
            {
                // a cache entry for the given beaconKey does not exist
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

        public bool IsEmpty(BeaconKey beaconKey)
        {
            var entry = GetCachedEntry(beaconKey);
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

        public void RemoveChunkedData(BeaconKey beaconKey)
        {
            var entry = GetCachedEntry(beaconKey);
            if (entry == null)
            {
                // a cache entry for the given beaconKey does not exist
                return;
            }

            entry.RemoveDataMarkedForSending();
        }

        public void ResetChunkedData(BeaconKey beaconKey)
        {
            var entry = GetCachedEntry(beaconKey);
            if (entry == null)
            {
                // a cache entry for the given beaconKey does not exist
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
        /// Get cached <see cref="BeaconCacheEntry"/> or insert new one if nothing exists for given <paramref name="beaconKey"/>.
        /// </summary>
        /// <param name="beaconKey">The beacon key to search for.</param>
        /// <returns>The already cached entry or newly created one.</returns>
        private BeaconCacheEntry GetCachedEntryOrInsert(BeaconKey beaconKey)
        {
            // get the appropriate cache entry
            var entry = GetCachedEntry(beaconKey);

            if (entry == null)
            {
                try
                {
                    // does not exist, and needs to be inserted
                    globalCacheLock.EnterWriteLock();
                    if (!beacons.ContainsKey(beaconKey))
                    {
                        // double check since this could have been added in the mean time
                        entry = new BeaconCacheEntry();
                        beacons.Add(beaconKey, entry);
                    }
                    else
                    {
                        entry = beacons[beaconKey];
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
        /// Get cached <see cref="BeaconCacheEntry"/> or <code>null</code> if nothing exists for given <paramref name="beaconKey"/>.
        /// </summary>
        /// <param name="beaconKey">The beacon key to search for.</param>
        /// <returns>The cached entry or <code>null</code>.</returns>
        private BeaconCacheEntry GetCachedEntry(BeaconKey beaconKey)
        {
            BeaconCacheEntry entry = null;

            // acquire read lock and get the entry
            try
            {
                globalCacheLock.EnterReadLock();
                if (beacons.TryGetValue(beaconKey, out BeaconCacheEntry result))
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

        private HashSet<BeaconKey> CopyBeaconKeys()
        {
            HashSet<BeaconKey> result;

            try
            {
                globalCacheLock.EnterReadLock();
                result = new HashSet<BeaconKey>(beacons.Keys);
            }
            finally
            {
                globalCacheLock.ExitReadLock();
            }

            return result;
        }

        #region for testing purposes

        /// <summary>
        /// Get events stored for given <paramref name="beaconKey"/>
        /// </summary>
        /// <remarks>
        /// This method is only for testing purposes and is not thread safe.
        /// </remarks>
        /// <param name="beaconKey">The beacon key for which to get the stored events.</param>
        /// <returns>Events stored for given <paramref name="beaconKey"/></returns>
        internal IList<BeaconCacheRecord> GetEvents(BeaconKey beaconKey)
        {
            return GetCachedEntry(beaconKey)?.EventData?.AsReadOnly();
        }

        /// <summary>
        /// Get actions stored for given <paramref name="beaconKey"/>
        /// </summary>
        /// <remarks>
        /// This method is only for testing purposes and is not thread safe.
        /// </remarks>
        /// <param name="beaconKey">The beacon key for which to get the stored actions.</param>
        /// <returns>Actions stored for given <paramref name="beaconKey"/></returns>
        internal IList<BeaconCacheRecord> GetActions(BeaconKey beaconKey)
        {
            return GetCachedEntry(beaconKey)?.ActionData?.AsReadOnly();
        }

        /// <summary>
        /// Get events being sent for given <paramref name="beaconKey"/>
        /// </summary>
        /// <remarks>
        /// This method is only for testing purposes and is not thread safe.
        /// </remarks>
        /// <param name="beaconKey">The beacon key for which to get the events being sent.</param>
        /// <returns>Events being sent for given <paramref name="beaconKey"/></returns>
        internal IList<BeaconCacheRecord> GetEventsBeingSent(BeaconKey beaconKey)
        {
            return GetCachedEntry(beaconKey)?.EventDataBeingSent?.AsReadOnly();
        }

        /// <summary>
        /// Get actions being sent for given <paramref name="beaconKey"/>
        /// </summary>
        /// <remarks>
        /// This method is only for testing purposes and is not thread safe.
        /// </remarks>
        /// <param name="beaconKey">The beacon key for which to get the actions being sent.</param>
        /// <returns>Actions being sent for given <paramref name="beaconKey"/></returns>
        internal IList<BeaconCacheRecord> GetActionsBeingSent(BeaconKey beaconKey)
        {
            return GetCachedEntry(beaconKey)?.ActionDataBeingSent?.AsReadOnly();
        }

        #endregion for testing purposes
    }
}
