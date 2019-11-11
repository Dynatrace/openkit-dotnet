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

#if (!NET40 && !NET35)

using System.Linq;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;

namespace Dynatrace.OpenKit.Protocol
{

    public class HttpClientHttpClient : HttpClient
    {
        private readonly IHttpClientConfiguration configuration;

        public HttpClientHttpClient(ILogger logger, IHttpClientConfiguration configuration) : base(logger, configuration)
        {
            this.configuration = configuration;
        }

        protected override HttpResponse GetRequest(string url, string clientIpAddress)
        {
            using (System.Net.Http.HttpClient httpClient = CreateHttpClient(clientIpAddress))
            {
                var responseTask = httpClient.GetAsync(url);
                responseTask.Wait();

                return CreateHttpResponse(responseTask.Result);
            }
        }

        protected override HttpResponse PostRequest(string url, string clientIpAddress, byte[] gzippedPayload)
        {
            using (System.Net.Http.HttpClient httpClient = CreateHttpClient(clientIpAddress))
            {
                var content = CreatePostContent(gzippedPayload);
                var responseTask = httpClient.PostAsync(url, content);
                responseTask.Wait();

                return CreateHttpResponse(responseTask.Result);
            }
        }

        private System.Net.Http.HttpClient CreateHttpClient(string clientIpAddress)
        {
            // The implementation of CreateHttpClient varies based on the .NET technology
            var httpClient = CreateHttpClient();

            if (clientIpAddress != null)
            {
                httpClient.DefaultRequestHeaders.Add("X-Client-IP", clientIpAddress);
            }

            return httpClient;
        }

        private static System.Net.Http.ByteArrayContent CreatePostContent(byte[] gzippedPayload)
        {
            if (gzippedPayload == null || gzippedPayload.Length == 0)
                return new System.Net.Http.ByteArrayContent(new byte[] { });

            var content = new System.Net.Http.ByteArrayContent(gzippedPayload);
            content.Headers.Add("Content-Encoding", "gzip");
            content.Headers.Add("Content-Length", gzippedPayload.Length.ToString());

            return content;
        }

        private static HttpResponse CreateHttpResponse(System.Net.Http.HttpResponseMessage result)
        {
            System.Threading.Tasks.Task<string> httpResponseContentTask = result.Content.ReadAsStringAsync();
            httpResponseContentTask.Wait();
            var response = httpResponseContentTask.Result;
            var responseCode = result.StatusCode;
            var headers = result.Headers.ToDictionary(pair => pair.Key.ToLowerInvariant(), pair => pair.Value.ToList());

            return new HttpResponse
            {
                Response = response,
                ResponseCode = (int)responseCode,
                Headers = headers
            };
        }

        #region CreateHttpClient implementations

#if WINDOWS_UWP || NETSTANDARD1_1

        // handling for all frameworks that do not support certificate validation
        private System.Net.Http.HttpClient CreateHttpClient()
        {
            return new System.Net.Http.HttpClient();
        }

#elif NETSTANDARD2_0

        // .NET standard uses the ServicePointManager's global callback
        private static bool remoteCertificateValidationCallbackInitialized = false;

        private System.Net.Http.HttpClient CreateHttpClient()
        {
            if (!remoteCertificateValidationCallbackInitialized)
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback += configuration.SslTrustManager.ServerCertificateValidationCallback;
                remoteCertificateValidationCallbackInitialized = true;
            }
            return new System.Net.Http.HttpClient();
        }

#elif NET45 || NET46 || NET47

        private System.Net.Http.HttpClient CreateHttpClient()
        {
            var webRequestHandler = new System.Net.Http.WebRequestHandler();
            webRequestHandler.ServerCertificateValidationCallback += configuration.SslTrustManager.ServerCertificateValidationCallback;
            return new System.Net.Http.HttpClient(webRequestHandler, true);
        }

#else

        private System.Net.Http.HttpClient CreateHttpClient()
        {
            var httpClientHandler = new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = configuration.SslTrustManager.ServerCertificateValidationCallback.Invoke
            };
            return new System.Net.Http.HttpClient(httpClientHandler, true);
        }

#endif

        #endregion

    }
}
#endif
