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

using System;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core.Caching
{
    class BeaconCache : IBeaconCache
    {
        public BeaconCache()
        {

        }

        public HashSet<int> BeaconIDs => throw new NotImplementedException();

        public long NumBytesInCache => throw new NotImplementedException();

        public event EventHandler RecordAdded;

        public void AddActionData(int beaconID, long timestamp, string data)
        {
            throw new NotImplementedException();
        }

        public void AddEventData(int beaconID, long timestamp, string data)
        {
            throw new NotImplementedException();
        }

        public void DeleteCacheEntry(int beaconID)
        {
            throw new NotImplementedException();
        }

        public int EvictRecordsByAge(int beaconID, long minTimestamp)
        {
            throw new NotImplementedException();
        }

        public int EvictRecordsByNumber(int beaconID, int numRecords)
        {
            throw new NotImplementedException();
        }

        public string GetNextBeaconChunk(int beaconID, string chunkPrefix, int maxSize, char delimiter)
        {
            throw new NotImplementedException();
        }

        public bool IsEmpty(int beaconID)
        {
            throw new NotImplementedException();
        }

        public void RemoveChunkedData(int beaconID)
        {
            throw new NotImplementedException();
        }

        public void ResetChunkedData(int beaconID)
        {
            throw new NotImplementedException();
        }
    }
}
