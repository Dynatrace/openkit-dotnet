//
// Copyright 2018-2022 Dynatrace LLC
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

using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Core.Objects
{
    internal class SupplementaryBasicData : ISupplementaryBasicData
    {
        /// <summary>
        /// object used for synchronization
        /// </summary>
        private readonly object lockObject = new object();

        private string _NetworkTechnology;
        private string _Carrier;
        private ConnectionType _ConnectionType;

        public string NetworkTechnology {
            get 
            { 
                lock(lockObject) { return _NetworkTechnology; }
            } 
            set
            {
                lock(lockObject) { _NetworkTechnology = value; }
            }
        }

        public string Carrier
        {
            get
            {
                lock (lockObject) { return _Carrier; }
            }
            set
            {
                lock (lockObject) { _Carrier = value; }
            }
        }

        public ConnectionType ConnectionType
        {
            get
            {
                lock (lockObject) { return _ConnectionType; }
            }
            set
            {
                lock (lockObject) { _ConnectionType = value; }
            }
        }
    }
}
