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

using Dynatrace.OpenKit.Util;

namespace Dynatrace.OpenKit.Core.Caching
{
    public class BeaconKey
    {
        public BeaconKey(int beaconId, int beaconSeqNo)
        {
            BeaconId = beaconId;
            BeaconSeqNo = beaconSeqNo;
        }

        /// <summary>
        /// Returns the beacon / session number of this key
        /// </summary>
        public int BeaconId { get; }

        /// <summary>
        /// Returns the beacon / session sequence number of this key
        /// </summary>
        public int BeaconSeqNo { get; }

        #region Equality
        protected bool Equals(BeaconKey other)
        {
            return BeaconId == other.BeaconId && BeaconSeqNo == other.BeaconSeqNo;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((BeaconKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (BeaconId * 397) ^ BeaconSeqNo;
            }
        }

        #endregion

        public override string ToString()
        {
            return $"[sn={BeaconId.ToInvariantString()}, seq={BeaconSeqNo.ToInvariantString()}]";
        }
    }
}