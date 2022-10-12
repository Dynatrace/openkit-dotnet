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

namespace Dynatrace.OpenKit.API
{
    /// <summary>
    /// Specifies the type of a network connection.
    /// </summary>
    public class ConnectionType
    {
        private ConnectionType(string value) { Value = value; }

        public readonly string Value;

        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Mobile connection type
        /// </summary>
        public static ConnectionType MOBILE => new("m");

        /// <summary>
        /// Wireless connection type
        /// </summary>
        public static ConnectionType WIFI => new("w");

        /// <summary>
        /// Offline
        /// </summary>
        public static ConnectionType OFFLINE => new("o");

        /// <summary>
        /// Connection via local area network
        /// </summary>
        public static ConnectionType LAN => new("l");
    }
}
