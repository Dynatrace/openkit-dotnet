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

namespace Dynatrace.OpenKit.Core.Objects
{
    /// <summary>
    ///  Class holding device specific information
    /// </summary>
    public class Device
    {

        public Device(string operatingSystem, string manufacturer, string modelId)
        {
            OperatingSystem = operatingSystem;
            Manufacturer = manufacturer;
            ModelId = modelId;
        }

        public string OperatingSystem { get; }

        public string Manufacturer { get; }

        public string ModelId { get; }

    }

}
