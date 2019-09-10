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

using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Core.Configuration
{
    /// <summary>
    /// The HttpClientConfiguration holds all http client related settings
    /// </summary>
    public class HttpClientConfiguration
    {
        public HttpClientConfiguration(string baseUrl, int serverId, string applicationId, ISSLTrustManager sslTrustManager)
        {
            BaseUrl = baseUrl;
            ServerId = serverId;
            ApplicationId = applicationId;
            SslTrustManager = sslTrustManager;
        }

        /// <summary>
        /// The base URL for the http client
        /// </summary>
        public string BaseUrl { get; }

        /// <summary>
        /// The server id to be used for the http client
        /// </summary>
        public int ServerId { get; }

        /// <summary>
        /// The application id for the http client
        /// </summary>
        public string ApplicationId { get; }

        public ISSLTrustManager SslTrustManager { get; }
    }
}
