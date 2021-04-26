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

#if (!NET40 && !NET35)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.API.HTTP;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Util;
using Dynatrace.OpenKit.Protocol.HTTP;
using Dynatrace.OpenKit.Util;

namespace Dynatrace.OpenKit.Protocol
{

    public class HttpClientHttpClient : HttpClient
    {
        private readonly IHttpClientConfiguration configuration;

        public HttpClientHttpClient(ILogger logger, IHttpClientConfiguration configuration,
            IInterruptibleThreadSuspender threadSuspender)
            : base(logger, configuration, threadSuspender)
        {
            this.configuration = configuration;
        }

        protected override HttpResponse GetRequest(string url, string clientIpAddress)
        {
            using (var message = CreateRequestMessage(HttpMethod.Get, url, clientIpAddress))
            {
                return CreateHttpResponse(SendRequest(message));
            }
        }

        protected override HttpResponse PostRequest(string url, string clientIpAddress, byte[] gzippedPayload)
        {
            using (var message = CreateRequestMessage(HttpMethod.Post, url, clientIpAddress))
            {
                message.Content = CreatePostContent(gzippedPayload);
                return CreateHttpResponse(SendRequest(message));
            }
        }

        private HttpRequestMessage CreateRequestMessage(HttpMethod httpMethod, string url, string clientIpAddress)
        {
            var requestMessage = new HttpRequestMessage(httpMethod, url);
            if (clientIpAddress != null)
            {
                requestMessage.Headers.Add("X-Client-IP", clientIpAddress);
            }

            return requestMessage;
        }

        private HttpResponseMessage SendRequest(HttpRequestMessage httpRequestMessage)
        {
            // intercept request
            HttpClientConfiguration.HttpRequestInterceptor.Intercept(new HttpRequestMessageAdapter(httpRequestMessage));

            HttpResponseMessage httpResponseMessage;

            using (var httpClient = CreateHttpClient())
            {
                var responseTask = httpClient.SendAsync(httpRequestMessage);
                responseTask.Wait();

                httpResponseMessage = responseTask.Result;
            }

            // intercept response
            HttpClientConfiguration.HttpResponseInterceptor.Intercept(new HttpResponseMessageAdapter(httpResponseMessage));

            return httpResponseMessage;
        }

        private static ByteArrayContent CreatePostContent(byte[] gzippedPayload)
        {
            if (gzippedPayload == null || gzippedPayload.Length == 0)
                return new ByteArrayContent(new byte[] { });

            var content = new ByteArrayContent(gzippedPayload);
            content.Headers.Add("Content-Encoding", "gzip");
            content.Headers.Add("Content-Length", gzippedPayload.Length.ToInvariantString());

            return content;
        }

        private static HttpResponse CreateHttpResponse(HttpResponseMessage result)
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
                ServicePointManager.ServerCertificateValidationCallback += configuration.SslTrustManager.ServerCertificateValidationCallback;
                remoteCertificateValidationCallbackInitialized = true;
            }
            return new System.Net.Http.HttpClient();
        }

#elif NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48

        private System.Net.Http.HttpClient CreateHttpClient()
        {
            var webRequestHandler = new WebRequestHandler();
            webRequestHandler.ServerCertificateValidationCallback += configuration.SslTrustManager.ServerCertificateValidationCallback;
            return new System.Net.Http.HttpClient(webRequestHandler, true);
        }

#else

        private System.Net.Http.HttpClient CreateHttpClient()
        {
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = configuration.SslTrustManager.ServerCertificateValidationCallback.Invoke
            };
            return new System.Net.Http.HttpClient(httpClientHandler, true);
        }

#endif

#endregion

        private class HttpRequestMessageAdapter : IHttpRequest
        {
            private readonly HttpRequestMessage httpRequestMessage;

            internal HttpRequestMessageAdapter(HttpRequestMessage httpRequestMessage)
            {
                this.httpRequestMessage = httpRequestMessage;
            }

            public Uri Uri => httpRequestMessage.RequestUri;

            public string Method => httpRequestMessage.Method.ToString();

            public Dictionary<string, List<string>> Headers => httpRequestMessage.Headers.ToDictionary(entry => entry.Key, entry => entry.Value.ToList(), StringComparer.OrdinalIgnoreCase);

            public bool ExistsHeader(string name) => httpRequestMessage.Headers.Contains(name);

            public List<string> GetHeader(string name) => httpRequestMessage.Headers.GetValues(name).ToList();

            public void AddHeader(string name, string value)
            {
                if (string.IsNullOrEmpty(name) || RestrictedRequestHeaders.IsHeaderRestricted(name))
                {
                    return;
                }

                httpRequestMessage.Headers.Add(name, value);
            }

            public void RemoveHeader(string name)
            {
                if (string.IsNullOrEmpty(name) || RestrictedRequestHeaders.IsHeaderRestricted(name))
                {
                    return;
                }

                httpRequestMessage.Headers.Remove(name);
            }
        }

        private class HttpResponseMessageAdapter : IHttpResponse
        {
            private readonly HttpResponseMessage httpResponseMessage;

            internal HttpResponseMessageAdapter(HttpResponseMessage httpResponseMessage)
            {
                this.httpResponseMessage = httpResponseMessage;
            }

            public Uri RequestUri => httpResponseMessage.RequestMessage.RequestUri;

            public string RequestMethod => httpResponseMessage.RequestMessage.Method.ToString();

            public HttpStatusCode HttpStatusCode => httpResponseMessage.StatusCode;

            public string ResponseMessage => httpResponseMessage.ReasonPhrase;

            public Dictionary<string, List<string>> Headers => httpResponseMessage.Headers.ToDictionary(entry => entry.Key, entry => entry.Value.ToList());

            public List<string> GetHeader(string name) => httpResponseMessage.Headers.GetValues(name).ToList();
        }
    }
}
#endif
