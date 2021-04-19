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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.API.HTTP;

namespace Dynatrace.OpenKit.Core.Configuration
{
    /// <summary>
    /// This interface holds all HTTP client related settings.
    /// </summary>
    public interface IHttpClientConfiguration
    {
        /// <summary>
        /// Returns the base URL for the <see cref="Dynatrace.OpenKit.Protocol.IHttpClient"/>.
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// Returns the server ID used for communication by the <see cref="Dynatrace.OpenKit.Protocol.IHttpClient"/>.
        /// </summary>
        int ServerId { get; }

        /// <summary>
        /// Returns the application used when communicating with the server via the
        /// <see cref="Dynatrace.OpenKit.Protocol.IHttpClient"/>.
        /// </summary>
        string ApplicationId { get; }

        /// <summary>
        /// Returns the <see cref="ISSLTrustManager">trust manager</see> used to encrypt the communication with the
        /// server.
        /// </summary>
        ISSLTrustManager SslTrustManager { get; }

        /// <summary>
        /// Returns the <see cref="IHttpRequestInterceptor" /> used to intercept HTTP requests, before they are sent 
        /// to the Dynatrace backend.
        /// </summary>
        IHttpRequestInterceptor HttpRequestInterceptor { get; }

        /// <summary>
        /// Returns the <see cref="IHttpResponseInterceptor" />used to intercept HTTP responses received 
        /// from Dynatrace backend.
        /// </summary>
        IHttpResponseInterceptor HttpResponseInterceptor { get; }
    }
}