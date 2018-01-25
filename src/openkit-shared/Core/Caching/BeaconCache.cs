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
