using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Dynatrace.OpenKit.Core.Caching
{
    internal class BeaconCacheEntry
    {
        /// <summary>
        /// List storing all active event data.
        /// </summary>
        private LinkedList<BeaconCacheRecord> eventData = new LinkedList<BeaconCacheRecord>();

        /// <summary>
        /// List storing all active session data.
        /// </summary>
        private LinkedList<BeaconCacheRecord> actionData = new LinkedList<BeaconCacheRecord>();

        /// <summary>
        /// Lock object for locking access to session & event data.
        /// </summary>
        private readonly object lockObject = new object();

        /// <summary>
        /// List storing all event data being sent.
        /// </summary>
        private LinkedList<BeaconCacheRecord> eventDataBeingSent = null;

        /// <summary>
        /// List storing all action data being sent.
        /// </summary>
        private LinkedList<BeaconCacheRecord> actionDataBeingSent = null;

        /// <summary>
        /// Total number of bytes consumed by this entry.
        /// </summary>
        internal long TotalNumBytes { get; private set; }

        #region for testing purposes

        /// <summary>
        /// Get a shallow copy of event data.
        /// </summary>
        /// <remarks>
        /// This method shall only be used for testing purposes.
        /// </remarks>
        internal List<BeaconCacheRecord> EventData => new List<BeaconCacheRecord>(eventData);

        /// <summary>
        /// Get a shallow copy of action data.
        /// </summary>
        /// <remarks>
        /// This method shall only be used for testing purposes.
        /// </remarks>
        internal List<BeaconCacheRecord> ActionData => new List<BeaconCacheRecord>(actionData);

        /// <summary>
        /// Get a shallow copy of action data being sent.
        /// </summary>
        /// <remarks>
        /// This method shall only be used for testing purposes.
        /// </remarks>
        internal List<BeaconCacheRecord> ActionDataBeingSent => actionDataBeingSent == null ? null : new List<BeaconCacheRecord>(actionDataBeingSent);

        /// <summary>
        /// Get a shallow copy of event data being sent.
        /// </summary>
        /// <remarks>
        /// This method shall only be used for testing purposes.
        /// </remarks>
        internal List<BeaconCacheRecord> EventDataBeingSent => eventDataBeingSent == null ? null : new List<BeaconCacheRecord>(eventDataBeingSent);

        #endregion for testing purposes

        /// <summary>
        /// Test if data shall be copied, before creating chunks for sending.
        /// </summary>
        /// <remarks></remarks>
        /// This property returns <code>true</code> if data needs to be copied, <code>false</code> otherwise.
        /// </remakrs>
        internal bool NeedsDataCopyBeforeChunking => actionDataBeingSent == null && eventDataBeingSent == null;

        /// <summary>
        /// Test if there is more data to send (to chunk).
        /// </summary>
        /// <remarks>
        /// This property returns <code>true</code> if there is more data to send, <code>false</code> otherwise.
        /// </remarks>
        private bool HasDataToSend => ((eventDataBeingSent != null && eventDataBeingSent.Count > 0) || (actionDataBeingSent != null && actionDataBeingSent.Count > 0));

        /// <summary>
        /// Lock this <see cref="BeaconCacheEntry"/> for reading & writing.
        /// </summary>
        /// <remarks>
        /// When locking is no longer required, <see cref="Unlock"/> must be called.
        /// </remarks>
        internal void Lock()
        {
            Monitor.Enter(lockObject);
        }

        /// <summary>
        /// Release this <see cref="BeaconCacheEntry"/> lock, so that other threads can access this object.
        /// </summary>
        /// <remarks>
        /// When calling this method ensure <see cref="Lock"/> was called before.
        /// </remarks>
        internal void Unlock()
        {
            Monitor.Exit(lockObject);
        }

        /// <summary>
        /// Add new event data record to cache.
        /// </summary>
        /// <param name="record">The new record to add.</param>
        internal void AddEventData(BeaconCacheRecord record)
        {
            eventData.AddLast(record);
            TotalNumBytes += record.DataSizeInBytes;
        }

        /// <summary>
        /// Add new action data record to the cache.
        /// </summary>
        /// <param name="record">The new record to add.</param>
        internal void AddActionData(BeaconCacheRecord record)
        {
            actionData.AddLast(record);
            TotalNumBytes += record.DataSizeInBytes;
        }

        /// <summary>
        /// Copy data for sending.
        /// </summary>
        internal void CopyDataForChunking()
        {
            eventDataBeingSent = eventData;
            actionDataBeingSent = actionData;
            eventData = new LinkedList<BeaconCacheRecord>();
            actionData = new LinkedList<BeaconCacheRecord>();
            TotalNumBytes = 0; // data which is being sent is not counted
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chunkPrefix">The prefix to add to each chunk</param>
        /// <param name="maxSize">The maximum size in characters for one chunk</param>
        /// <param name="delimiter">The delimiter between data chunks</param>
        /// <returns></returns>
        internal string GetChunk(string chunkPrefix, int maxSize, char delimiter)
        {
            if (!HasDataToSend)
            {
                // nothing to send - reset to null, so next time lists get copied again
                eventDataBeingSent = null;
                actionDataBeingSent = null;
                return string.Empty;
            }
            return GetNextChunk(chunkPrefix, maxSize, delimiter);
        }

        private string GetNextChunk(string chunkPrefix, int maxSize, char delimiter)
        {
            var beaconBuilder = new StringBuilder(maxSize);

            // append the chunk prefix
            beaconBuilder.Append(chunkPrefix);

            // append data from both lists
            // note the order is currently important -> event data goes first, then action data
            ChunkifyDataList(beaconBuilder, eventDataBeingSent, maxSize, delimiter);
            ChunkifyDataList(beaconBuilder, actionDataBeingSent, maxSize, delimiter);

            return beaconBuilder.ToString();
        }

        private void ChunkifyDataList(StringBuilder chunkBuilder, LinkedList<BeaconCacheRecord> dataBeingSent, int maxSize, char delimiter)
        {
            IEnumerator<BeaconCacheRecord> iterator = dataBeingSent.GetEnumerator();

            while (iterator.MoveNext() && chunkBuilder.Length <= maxSize)
            {
                // mark the record for sending
                var record = iterator.Current;
                record.MarkForSending();

                // append delimiter & data
                chunkBuilder.Append(delimiter).Append(record.Data);
            }
        }

        /// <summary>
        /// Remove data that was previously marked for sending when <see cref="GetNextChunk(string, int, char)"/> was called.
        /// </summary>
        internal void RemoveDataMarkedForSending()
        {
            while (eventDataBeingSent.Count > 0 && eventDataBeingSent.First.Value.IsMarkedForSending)
            {
                eventDataBeingSent.RemoveFirst();
            }
            if (eventDataBeingSent.Count == 0)
            {
                // only check action data, if all event data has been traversed, otherwise it's just waste of cpu time
                while (actionDataBeingSent.Count > 0 && actionDataBeingSent.First.Value.IsMarkedForSending)
                {
                    actionDataBeingSent.RemoveFirst();
                }
            }
        }

        /// <summary>
        /// This method removes the marked for sending and prepends the copied data back to the data.
        /// </summary>
        internal void ResetDataMarkedForSending()
        {
            // reset the "sending marks" and in the same traversal count the bytes which are added back
            long numBytes = 0;
            foreach (var record in eventDataBeingSent)
            {
                record.UnsetSending();
                numBytes += record.DataSizeInBytes;
            }

            foreach (var record in actionDataBeingSent)
            {
                record.UnsetSending();
                numBytes += record.DataSizeInBytes;
            }

            // merge data
            foreach (var record in eventData)
            {
                eventDataBeingSent.AddLast(record);
            }
            foreach (var reacord in actionData)
            {
                actionDataBeingSent.AddLast(reacord);
            }
            eventData = eventDataBeingSent;
            actionData = actionDataBeingSent;
            eventDataBeingSent = null;
            actionDataBeingSent = null;

            TotalNumBytes += numBytes;
        }

        /// <summary>
        /// Remove all <see cref="BeaconCacheRecord"/> from event and action data 
        /// which are older than given <paramref name="minTimestamp"/>
        /// </summary>
        /// <param name="minTimestamp">The minimum timestamp allowed</param>
        /// <returns>The total number of removed records.</returns>
        internal int RemoveRecordsOlderThan(long minTimestamp)
        {
            var numRecordsRemoved = RemoveRecordsOlderThan(eventData, minTimestamp);
            numRecordsRemoved += RemoveRecordsOlderThan(actionData, minTimestamp);

            return numRecordsRemoved;
        }

        /// <summary>
        ///  Remove all <see cref="BeaconCacheRecord"/> from <paramref name="records"/> that are older than <paramref name="minTimestamp"/>
        /// </summary>
        /// <param name="records">The list to check for records and remove them.</param>
        /// <param name="minTimestamp">The minimum timestamp allowed.</param>
        /// <returns></returns>
        private static int RemoveRecordsOlderThan(LinkedList<BeaconCacheRecord> records, long minTimestamp)
        {
            var numRecordsRemoved = 0;
            var currentNode = records.First;
            while (currentNode != null)
            {
                if (currentNode.Value.Timestamp < minTimestamp)
                {
                    var toRemove = currentNode;
                    currentNode = currentNode.Next;
                    records.Remove(toRemove);
                    numRecordsRemoved += 1;
                }
                else
                {
                    currentNode = currentNode.Next;
                }
            }

            return numRecordsRemoved;
        }

        /// <summary>
        /// Remove up to <paramref name="numRecords"/> records from event & action data, compared by their age.
        /// </summary>
        /// <remarks>
        /// Note not all event/action data entries are traversed, only the first action data & first event
        /// data is removed and compared against each other, which one to remove first.If the first action's timestamp and
        /// first event's timestamp are equal, the first event is removed.
        /// </remarks>
        /// <param name="numRecords">The number of records.</param>
        /// <returns>Number of actually removed records.</returns>
        internal int RemoveOldestRecords(int numRecords)
        {
            int numRecordsRemoved = 0;
            
            var currentEvent = eventData.First?.Value;
            var currentAction = actionData.First?.Value;

            while (numRecordsRemoved < numRecords && (currentEvent != null || currentAction != null))
            {
                if (currentEvent == null)
                {
                    // actions is not null -> remove action
                    actionData.RemoveFirst();
                    currentAction = actionData.First?.Value;
                }
                else if (currentAction == null)
                {
                    // events is not null -> remove event
                    eventData.RemoveFirst();
                    currentEvent = eventData.First?.Value;
                }
                else
                {
                    // both are not null -> compare by timestamp and take the older one
                    if (currentAction.Timestamp < currentEvent.Timestamp)
                    {
                        // first action is older than first event
                        actionData.RemoveFirst();
                        currentAction = actionData.First?.Value;
                    }
                    else
                    {
                        // first event is older than first action
                        eventData.RemoveFirst();
                        currentEvent = eventData.First?.Value;
                    }
                }

                numRecordsRemoved++;
            }

            return numRecordsRemoved;
        }
    }
}
