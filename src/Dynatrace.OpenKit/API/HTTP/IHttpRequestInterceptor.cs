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

namespace Dynatrace.OpenKit.API.HTTP
{
    /// <summary>
    /// An interface allowing to intercept an HTTP request, before it is sent to the backend system.
    /// <para>
    /// This interceptor is only applied to HTTP requests which are sent to Dynatrace backends.
    /// </para>
    /// </summary>
    public interface IHttpRequestInterceptor
    {
        /// <summary>
        /// Intercept the HTTP request and manipulate it.
        /// <para>
        /// Currently it's only possible to set custom HTTP headers.
        /// </para>
        /// </summary>
        /// <param name="httpRequest">The HTTP request to Dynatrace backend.</param>
        void Intercept(IHttpRequest httpRequest);
    }
}
