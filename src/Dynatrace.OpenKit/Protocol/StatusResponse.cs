//
// Copyright 2018-2021 Dynatrace LLC
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
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Util;

namespace Dynatrace.OpenKit.Protocol
{
    /// <summary>
    ///  Implements a status response which is sent for the request types status check & beacon send.
    /// </summary>
    public class StatusResponse : IStatusResponse
    {
        /// <summary>
        /// Response code sent by HTTP server to indicate success.
        /// </summary>
        public const int HttpOk = 200;

        /// <summary>
        /// First error code indicating an HTTP error and therefore an erroneous response.
        /// </summary>
        public const int HttpBadRequest = 400;

        /// <summary>
        /// Too many requests sent by client (rate limiting) error code.
        /// </summary>
        public const int HttpTooManyRequests = 429;

        /// <summary>
        /// Key in the HTTP response headers for Retry-After
        /// </summary>
        internal const string ResponseKeyRetryAfter = "retry-after";

        /// <summary>
        /// Default "Retry-After" is 10 minutes.
        /// </summary>
        internal const int DefaultRetryAfterInMilliseconds = 10 * 60 * 1000;

        /// <summary>
        /// Value that is sent if response status indicates an error.
        /// <para>
        /// The status is part of the payload <see cref="ResponseAttributes"/>.
        /// </para>
        /// </summary>
        internal const string ResponseStatusError = "ERROR";

        /// <summary>
        /// Logger for logging messages.
        /// </summary>
        private readonly ILogger logger;

        private StatusResponse(ILogger logger, IResponseAttributes responseAttributes, int responseCode,
            Dictionary<string, List<string>> headers)
        {
            this.logger = logger;
            ResponseCode = responseCode;
            Headers = headers;
            ResponseAttributes = responseAttributes;
        }

        public static StatusResponse CreateSuccessResponse(ILogger logger, IResponseAttributes responseAttributes,
            int responseCode, Dictionary<string, List<string>> headers)
        {
            return new StatusResponse(logger, responseAttributes, responseCode, headers);
        }

        public static StatusResponse CreateErrorResponse(ILogger logger, int responseCode)
        {
            return CreateErrorResponse(logger, responseCode, new Dictionary<string, List<string>>());
        }

        public static StatusResponse CreateErrorResponse(ILogger logger, int responseCode,
            Dictionary<string, List<string>> headers)
        {
            var responseAttributes = Protocol.ResponseAttributes.WithUndefinedDefaults().Build();
            return new StatusResponse(logger, responseAttributes, responseCode, headers);
        }

        /// <summary>
        /// Gives a boolean indicating whether this response is erroneous or not.
        /// </summary>
        public bool IsErroneousResponse => ResponseCode >= HttpBadRequest || IsStatusSetToError;

        private bool IsStatusSetToError =>
            ResponseAttributes.IsAttributeSet(ResponseAttribute.STATUS) && ResponseStatusError == ResponseAttributes.Status;

        /// <summary>
        /// Get the HTTP response code.
        /// </summary>
        public int ResponseCode { get; }

        public IResponseAttributes ResponseAttributes { get; }

        /// <summary>
        /// Get the HTTP response headers.
        /// </summary>
        public Dictionary<string, List<string>> Headers { get; }

        /// <summary>
        /// Get Retry-After response header value in milliseconds.
        /// </summary>
        /// <remarks>
        /// Retry-After response header can either be an HTTP date or a delay in seconds.
        /// This function can only correctly parse the delay seconds.
        /// </remarks>
        /// <returns>Retry-After value in milliseconds.</returns>
        public int GetRetryAfterInMilliseconds()
        {
            if (!Headers.TryGetValue(ResponseKeyRetryAfter, out List<string> values))
            {
                // the Retry-After response header is missing
                logger.Warn(
                    $"{ResponseKeyRetryAfter} is not available - using default value ${DefaultRetryAfterInMilliseconds.ToInvariantString()}");
                return DefaultRetryAfterInMilliseconds;
            }

            if (values.Count != 1)
            {
                // the Retry-After response header has multiple values, but only one is expected
                logger.Warn(
                    $"{ResponseKeyRetryAfter} has unexpected number of values - using default value {DefaultRetryAfterInMilliseconds.ToInvariantString()}");
                return DefaultRetryAfterInMilliseconds;
            }

            // according to RFC 7231 Section 7.1.3 (https://tools.ietf.org/html/rfc7231#section-7.1.3)
            // Retry-After value can either be a delay seconds value, which is a non-negative decimal integer
            // or it is an HTTP date.
            // Our implementation assumes only delay seconds value here
            if (!int.TryParse(values[0], out int delaySeconds))
            {
                logger.Error(
                    $"Failed to parse {ResponseKeyRetryAfter} value \"${values[0]}\" - using default value ${DefaultRetryAfterInMilliseconds.ToInvariantString()}");
                return DefaultRetryAfterInMilliseconds;
            }

            // convert delay seconds to milliseconds
            return delaySeconds * 1000;
        }
    }
}