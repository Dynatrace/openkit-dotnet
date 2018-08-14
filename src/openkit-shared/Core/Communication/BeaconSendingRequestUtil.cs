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

using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// Utility class for sending requests to the server and retry several times
    /// </summary>
    internal static class BeaconSendingRequestUtil
    {
        /// <summary>
        /// Send a status request to the server and try to get the status response.
        /// </summary>
        /// <param name="context">Used to retrieve the <see cref="IHTTPClient"/> and for delaying methods.</param>
        /// <param name="numRetries">The number of retries (total number of tries = numRetries + 1)</param>
        /// <param name="initialRetryDelayInMillis">The initial delay which is doubled between one unsuccessful attempt and the next retry.</param>
        /// <returns> A status response or <code>null</code> if shutdown was requested or number of retries was reached.</returns>
        internal static StatusResponse SendStatusRequest(IBeaconSendingContext context, int numRetries, int initialRetryDelayInMillis)
        {
            StatusResponse statusResponse = null;
            var sleepTimeInMillis = initialRetryDelayInMillis;
            var retry = 0;

            while (true)
            {
                statusResponse = context.GetHTTPClient().SendStatusRequest();
                if (IsSuccessfulStatusResponse(statusResponse)
                      || IsTooManyRequestsResponse(statusResponse) // is handled by the states
                      || retry >= numRetries
                      || context.IsShutdownRequested)
                {
                    break;
                }

                // if no (valid) status response was received -> sleep and double the delay for each retry
                context.Sleep(sleepTimeInMillis);
                sleepTimeInMillis *= 2;
                retry++;
            }

            return statusResponse;
        }

        /// <summary>
        /// Test if given <paramref name="statusResponse"/> is a successful response.
        /// </summary>
        /// <param name="statusResponse">The given status response to check whether it is successful or not.</param>
        /// <returns><code>true</code> if status response is successful, <code>false</code> otherwise.</returns>
        internal static bool IsSuccessfulStatusResponse(StatusResponse statusResponse)
        {
            return statusResponse != null && !statusResponse.IsErroneousResponse;
        }

        /// <summary>
        /// Test if the given <paramref name="statusResponse"/> is a "too many requests" response.
        /// </summary>
        /// <remarks>
        /// A "too many requests" response is an HTTP response with response code 429.
        /// </remarks>
        /// <param name="statusResponse">The given status response to check whether it is a "too many requests" response or not.</param>
        /// <returns><code>true</code> if status response indicates too many requests, <code>false</code> otherwise.</returns>
        internal static bool IsTooManyRequestsResponse(StatusResponse statusResponse)
        {
            return statusResponse != null && statusResponse.ResponseCode == Response.HttpTooManyRequests;
        }
    }
}
