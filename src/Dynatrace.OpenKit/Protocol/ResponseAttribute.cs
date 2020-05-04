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

namespace Dynatrace.OpenKit.Protocol
{
    /// <summary>
    /// Specifies an attribute in the <see cref="ResponseAttributes"/> sent by the server.
    /// </summary>
    public enum ResponseAttribute
    {
        /// <summary>
        /// Maximum POST body size when sending beacon data.
        /// </summary>
        MAX_BEACON_SIZE,

        /// <summary>
        /// Maximum duration after which a session is split.
        /// </summary>
        MAX_SESSION_DURATION,

        /// <summary>
        /// Maximum number of top level actions after which a session is split.
        /// </summary>
        MAX_EVENTS_PER_SESSION,

        /// <summary>
        /// Idle timeout after which a session is split.
        /// </summary>
        SESSION_IDLE_TIMEOUT,

        /// <summary>
        /// Interval at which beacon data is sent to the server.
        /// </summary>
        SEND_INTERVAL,

        /// <summary>
        /// Version of the visit store to be used.
        /// </summary>
        VISIT_STORE_VERSION,

        /// <summary>
        /// Indicator whether capturing data is allowed or not.
        /// </summary>
        IS_CAPTURE,

        /// <summary>
        /// Indicator whether crashes should be captured or not.
        /// </summary>
        IS_CAPTURE_CRASHES,

        /// <summary>
        /// Indicator whether errors should be captured or not.
        /// </summary>
        IS_CAPTURE_ERRORS,

        /// <summary>
        /// The ID of the application to which a configuration applies.
        /// </summary>
        APPLICATION_ID,

        /// <summary>
        /// Multiplicity
        /// </summary>
        MULTIPLICITY,

        /// <summary>
        /// The ID of the server to which data should be sent to.
        /// </summary>
        SERVER_ID,

        /// <summary>
        /// Timestamp of the configuration sent by the server.
        /// </summary>
        TIMESTAMP
    }
}