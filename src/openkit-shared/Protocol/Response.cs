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


using System.Collections.Generic;

namespace Dynatrace.OpenKit.Protocol
{

    /// <summary>
    ///  Abstract base class for a response to one of the 3 request types (status check, beacon send, time sync).
    /// </summary>
    public abstract class Response
    {
        /// <summary>
        /// First error code indicating an HTTP error and therefore an erroneous response.
        /// </summary>
        private const int HTTP_BAD_REQUEST = 400;

        /// <summary>
        /// Create the base response.
        /// </summary>
        /// <param name="responseCode">HTTP response code</param>
        /// <param name="headers">HTTP response headers</param>
        protected Response(int responseCode, Dictionary<string, List<string>> headers)
        {
            ResponseCode = responseCode;
            Headers = headers;
        }

        /// <summary>
        /// Gives a boolean indicating whether this response is erroneous or not.
        /// </summary>
        public bool IsErroneousResponse => ResponseCode >= HTTP_BAD_REQUEST;

        /// <summary>
        /// Get the HTTP response code.
        /// </summary>
        public int ResponseCode { get; }

        /// <summary>
        /// Get the HTTP response headers.
        /// </summary>
        public Dictionary<string, List<string>> Headers { get; }

    }
}
