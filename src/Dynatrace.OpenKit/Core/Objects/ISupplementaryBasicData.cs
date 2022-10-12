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
    /// <summary>
    /// Specifies supplementary basic data which will be written to the <see cref="Protocol.Beacon"/>.
    /// </summary>
    public interface ISupplementaryBasicData
    {
        /// <summary>
        /// Network technology used by the device
        /// </summary>
        string NetworkTechnology { get; set; }

        /// <summary>
        /// Carrier used by the device
        /// </summary>
        string Carrier { get; set; }

        /// <summary>
        /// Connection type used by the device
        /// </summary>
        ConnectionType ConnectionType { get; set; } 
    }
}
