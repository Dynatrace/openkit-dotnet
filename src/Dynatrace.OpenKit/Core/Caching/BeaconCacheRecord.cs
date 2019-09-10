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

namespace Dynatrace.OpenKit.Core.Caching
{
    /// <summary>
    /// A single record in the BeaconCache.
    ///
    /// <para>
    /// A record is described by
    ///
    /// <list type="bullet">
    /// <item>The timestamp when it was created/ended</item>
    /// <item>Serialized data</item>
    /// </list>
    /// </para>
    /// </summary>
    internal class BeaconCacheRecord
    {
        /// <summary>
        /// Create a new BeaconCacheRecord
        /// </summary>
        /// <param name="timestamp">Timestamp for this record.</param>
        /// <param name="data">Data to store for this record.</param>
        internal BeaconCacheRecord(long timestamp, string data)
        {
            Timestamp = timestamp;
            Data = data;
        }

        /// <summary>
        /// Get timestamp
        /// </summary>
        internal long Timestamp { get; }

        /// <summary>
        /// Get data
        /// </summary>
        internal string Data { get; }

        /// <summary>
        /// Get data size estimation of this record.
        ///
        /// <para>
        /// Note that this is just a very rough estimation required for cache eviction.
        ///
        /// It's sufficient to approximate the bytes required by the string and omit any other information like
        /// the timestamp, any references and so on.
        /// </para>
        /// </summary>
        internal long DataSizeInBytes => Data?.Length * sizeof(char) ?? 0;

        /// <summary>
        /// Boolean indicating whether this record is already marked for sending or not.
        /// </summary>
        internal bool IsMarkedForSending { get; private set; }

        /// <summary>
        /// Mark this record for sending.
        /// </summary>
        internal void MarkForSending()
        {
            IsMarkedForSending = true;
        }

        /// <summary>
        /// Reset the sending mark.
        /// </summary>
        internal void UnsetSending()
        {
            IsMarkedForSending = false;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (BeaconCacheRecord)obj;
            return Timestamp == other.Timestamp
                && Data == other.Data
                && IsMarkedForSending == other.IsMarkedForSending;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Timestamp.GetHashCode()
                ^ (Data?.GetHashCode()).GetValueOrDefault(0)
                ^ IsMarkedForSending.GetHashCode();
        }
    }
}
