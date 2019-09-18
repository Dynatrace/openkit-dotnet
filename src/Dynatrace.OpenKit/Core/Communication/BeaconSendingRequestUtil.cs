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
        /// <param name="context">Used to retrieve the <see cref="IHttpClient"/> and for delaying methods.</param>
        /// <param name="numRetries">The number of retries (total number of tries = numRetries + 1)</param>
        /// <param name="initialRetryDelayInMillis">The initial delay which is doubled between one unsuccessful attempt and the next retry.</param>
        /// <returns> A status response or <code>null</code> if shutdown was requested or number of retries was reached.</returns>
        internal static StatusResponse SendStatusRequest(IBeaconSendingContext context, int numRetries, int initialRetryDelayInMillis)
        {
            StatusResponse statusResponse;
            var sleepTimeInMillis = initialRetryDelayInMillis;
            var retry = 0;

            while (true)
            {
                statusResponse = context.GetHttpClient().SendStatusRequest();
                if (BeaconSendingResponseUtil.IsSuccessfulResponse(statusResponse)
                      || BeaconSendingResponseUtil.IsTooManyRequestsResponse(statusResponse) // is handled by the states
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
    }
}
