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
    public interface IHttpClient
    {
        /// <summary>
        /// Sends a status request and returns a status response
        /// </summary>
        /// <param name="additionalParameters">
        ///     additional parameters that will be send with the beacon request (can be <code>null</code>.
        /// </param>
        /// <returns></returns>
        IStatusResponse SendStatusRequest(IAdditionalQueryParameters additionalParameters);

        /// <summary>
        /// Sends a beacon send request and returns a status response
        /// </summary>
        /// <param name="clientIpAddress"></param>
        /// <param name="data"></param>
        /// <param name="additionalParameters">
        ///     additional parameters that will be send with the beacon request (can be <code>null</code>.
        /// </param>
        /// <returns></returns>
        IStatusResponse SendBeaconRequest(string clientIpAddress, byte[] data,
            IAdditionalQueryParameters additionalParameters);

        /// <summary>
        /// Sends a special status request for a new session.
        /// </summary>
        /// <param name="additionalParameters">
        ///     additional parameters that will be send with the beacon request (can be <code>null</code>.
        /// </param>
        /// <returns>Returns the status response.</returns>
        IStatusResponse SendNewSessionRequest(IAdditionalQueryParameters additionalParameters);
    }
}
