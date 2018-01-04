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

using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Core
{

    /// <summary>
    ///  Actual implementation of the IDevice interface.
    /// </summary>
    public class Device : IDevice
    {

        // platform information
        private string operatingSystem = OpenKitConstants.DEFAULT_OPERATING_SYSTEM;
        private string manufacturer = OpenKitConstants.DEFAULT_MANUFACTURER;
        private string modelID = OpenKitConstants.DEFAULT_DEVICE_ID;

        // *** IDevice interface methods & propreties ***

        public string OperatingSystem
        {
            get
            {
                return operatingSystem;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.operatingSystem = value;
                }
            }
        }

        public string Manufacturer
        {
            get
            {
                return manufacturer;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.manufacturer = value;
                }
            }
        }

        public string ModelID
        {
            get
            {
                return modelID;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.modelID = value;
                }
            }
        }

    }

}