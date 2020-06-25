//
// Copyright 2018-2020 Dynatrace LLC
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
    public class HttpClientConfiguration : IHttpClientConfiguration
    {
        private HttpClientConfiguration(Builder builder)
        {
            BaseUrl = builder.BaseUrl;
            ServerId = builder.ServerId;
            ApplicationId = builder.ApplicationId;
            SslTrustManager = builder.TrustManager;
        }

        /// <summary>
        /// Creates a new new HTTP configuration initialized with the given <paramref name="openKitConfig"/>.
        /// </summary>
        /// <param name="openKitConfig">the OpenKit configuration from which the newly created instance will be
        /// initialized with.</param>
        /// <returns>a new <see cref="IHttpClientConfiguration"/> instance initialized with the given
        ///   <see cref="IOpenKitConfiguration"/>.
        /// </returns>
        internal static IHttpClientConfiguration From(IOpenKitConfiguration openKitConfig)
        {
            return ModifyWith(openKitConfig).Build();
        }

        /// <summary>
        /// Creates a new builder instance and initializes it from the given <paramref name="openKitConfig"/>.
        /// </summary>
        /// <param name="openKitConfig">the <see cref="IOpenKitConfiguration"/> from which the builder will be
        /// initialized.</param>
        /// <returns>a pre initialized builder instance for creating a new <see cref="IHttpClientConfiguration"/>.</returns>
        internal static Builder ModifyWith(IOpenKitConfiguration openKitConfig)
        {
            return new Builder()
                .WithBaseUrl(openKitConfig.EndpointUrl)
                .WithApplicationId(openKitConfig.ApplicationId)
                .WithTrustManager(openKitConfig.TrustManager)
                .WithServerId(openKitConfig.DefaultServerId);
        }

        internal static Builder ModifyWith(IHttpClientConfiguration httpClientConfig)
        {
            return new Builder()
                .WithBaseUrl(httpClientConfig.BaseUrl)
                .WithApplicationId(httpClientConfig.ApplicationId)
                .WithTrustManager(httpClientConfig.SslTrustManager)
                .WithServerId(httpClientConfig.ServerId);
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

        public class Builder
        {
            public string BaseUrl { get; private set; }
            public int ServerId { get; private set; } = -1;
            public string ApplicationId { get; private set; }
            public ISSLTrustManager TrustManager { get; private set; }

            public Builder WithBaseUrl(string baseUrl)
            {
                BaseUrl = baseUrl;
                return this;
            }

            public Builder WithServerId(int serverId)
            {
                ServerId = serverId;
                return this;
            }

            public Builder WithApplicationId(string applicationId)
            {
                ApplicationId = applicationId;
                return this;
            }

            public Builder WithTrustManager(ISSLTrustManager trustManager)
            {
                TrustManager = trustManager;
                return this;
            }

            public IHttpClientConfiguration Build()
            {
                return new HttpClientConfiguration(this);
            }
        }
    }
}
