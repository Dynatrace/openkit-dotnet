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

using System;
using System.Net;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.API.HTTP
{
    /// <summary>
    /// Interface representing an HTTP response.
    /// </summary>
    public interface IHttpResponse
    {
        /// <summary>
        /// Gets the HTTP request <see cref="Uri"/> associated with this response.
        /// </summary>
        Uri RequestUri { get; }

        /// <summary>
        /// Gets the HTTP request method associated with this response.
        /// </summary>
        string RequestMethod { get; }

        /// <summary>
        /// Gets the HTTP response code.
        /// </summary>
        HttpStatusCode HttpStatusCode { get; }

        /// <summary>
        /// Gets the HTTP response message.
        /// </summary>
        string ResponseMessage { get; }

        /// <summary>
        /// Gets the HTTP response headers and their values.
        /// </summary>
        Dictionary<string, List<string>> Headers { get; }

        /// <summary>
        /// Gets the values of an HTTP response header.
        /// </summary>
        /// <param name="name">The HTTP response header name for which to retrieve the values.</param>
        /// <returns>Values for HTTP response header <paramref name="name"/>.</returns>
        List<string> GetHeader(string name);
    }
}
