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

namespace Dynatrace.OpenKit.Protocol
{
    public interface IHTTPClient
    {
        /// <summary>
        /// Sends a status request and returns a status response
        /// </summary>
        /// <returns></returns>
        StatusResponse SendStatusRequest();

        /// <summary>
        /// Sends a beacon send request and returns a status response
        /// </summary>
        /// <param name="clientIPAddress"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        StatusResponse SendBeaconRequest(string clientIPAddress, byte[] data);

        /// <summary>
        /// Sends a time sync request and returns a time sync response
        /// </summary>
        /// <returns></returns>
        TimeSyncResponse SendTimeSyncRequest();

        /// <summary>
        /// Sends a special status request for a new session.
        /// </summary>
        /// <returns>Returns the status response.</returns>
        StatusResponse SendNewSessionRequest();
    }
}
