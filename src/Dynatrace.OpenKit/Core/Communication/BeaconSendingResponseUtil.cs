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
    internal static class BeaconSendingResponseUtil
    {

        /// <summary>
        /// Test if given <paramref name="response"/> is a successful response.
        /// </summary>
        /// <param name="response">The given response to check whether it is successful or not.</param>
        /// <returns><code>true</code> if response is successful, <code>false</code> otherwise.</returns>
        internal static bool IsSuccessfulResponse(IStatusResponse response)
        {
            return response != null && !response.IsErroneousResponse;
        }

        /// <summary>
        /// Test if the given <paramref name="response"/> is a "too many requests" response.
        /// </summary>
        /// <remarks>
        /// A "too many requests" response is an HTTP response with response code 429.
        /// </remarks>
        /// <param name="response">The given response to check whether it is a "too many requests" response or not.</param>
        /// <returns><code>true</code> if response indicates too many requests, <code>false</code> otherwise.</returns>
        internal static bool IsTooManyRequestsResponse(IStatusResponse response)
        {
            return response != null && response.ResponseCode == StatusResponse.HttpTooManyRequests;
        }
    }
}
