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

#if NET35

using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.API;
using System.Collections.Generic;
using Dynatrace.OpenKit.Core.Util;
using System.Net;
using System;
using Dynatrace.OpenKit.API.HTTP;
using System.Linq;
using Dynatrace.OpenKit.Protocol.HTTP;

namespace Dynatrace.OpenKit.Protocol
{

    public class HttpClientWebClient : HttpClient
    {
        private static bool _remoteCertificateValidationCallbackInitialized = false;

        public HttpClientWebClient(ILogger logger, IHttpClientConfiguration configuration,
            IInterruptibleThreadSuspender threadSuspender)
            : base(logger, configuration, threadSuspender)
        {
            if (!_remoteCertificateValidationCallbackInitialized)
            {
                ServicePointManager.ServerCertificateValidationCallback +=
                    configuration.SslTrustManager?.ServerCertificateValidationCallback;
                _remoteCertificateValidationCallbackInitialized = true;
            }
        }

        protected override HttpResponse GetRequest(string url, string clientIpAddress)
        {
            using (var webClient = new WrappedWebClient(HttpClientConfiguration, clientIpAddress))
            {
                using (var response = webClient.Get(url))
                {
                    return ReadResponse(response);
                }
            }
        }

        protected override HttpResponse PostRequest(string url, string clientIpAddress, byte[] gzippedPayload)
        {
            using (var webClient = new WrappedWebClient(HttpClientConfiguration, clientIpAddress))
            {
                using (var response = webClient.Post(url, gzippedPayload))
                {
                    return ReadResponse(response);
                }
            }
        }

        private static HttpResponse ReadResponse(WebResponse webResponse)
        {
            string response;
            using (var reader = new System.IO.StreamReader(webResponse.GetResponseStream()))
            {
                response = reader.ReadToEnd();
            }
            var responseCode = ((HttpWebResponse)webResponse).StatusCode;
            var headers = new Dictionary<string, List<string>>();
            foreach (var header in webResponse.Headers.AllKeys)
            {
                headers.Add(header.ToLowerInvariant(), new List<string>(webResponse.Headers.GetValues(header)));
            }

            return new HttpResponse
            {
                Response = response,
                ResponseCode = (int)responseCode,
                Headers = headers
            };
        }

        private class WrappedWebClient : WebClient
        {
            private readonly IHttpClientConfiguration httpClientConfiguration;
            private readonly string clientIpAddress;


            public WrappedWebClient(IHttpClientConfiguration httpClientConfiguration, string clientIpAddress)
            {
                this.httpClientConfiguration = httpClientConfiguration;
                this.clientIpAddress = clientIpAddress;
            }

            public HttpWebResponse Get(string url)
            {
                var request = GetHttpWebRequest(url);
                httpClientConfiguration.HttpRequestInterceptor.Intercept(new HttpWebRequestAdapter(request));

                var response = (HttpWebResponse)request.GetResponse();
                httpClientConfiguration.HttpResponseInterceptor.Intercept(new HttpWebResponseAdapter(request, response));

                return response;
            }

            public HttpWebResponse Post(string url, byte[] gzippedPayload)
            {
                var request = GetHttpWebRequest(url);
                request.Method = "POST";

                httpClientConfiguration.HttpRequestInterceptor.Intercept(new HttpWebRequestAdapter(request));

                // if there is compressed data, post it to the server
                if (gzippedPayload != null && gzippedPayload.Length > 0)
                {
                    request.ContentLength = gzippedPayload.Length;
                    request.Headers.Add("Content-Encoding", "gzip");

                    Headers["Content-Encoding"] = "gzip";

                    using(var stream = request.GetRequestStream())
                    {
                        stream.Write(gzippedPayload, 0, gzippedPayload.Length);
                        stream.Close();
                    }
                }

                // get servers response
                var response = (HttpWebResponse)request.GetResponse();
                httpClientConfiguration.HttpResponseInterceptor.Intercept(new HttpWebResponseAdapter(request, response));

                return response;
            }

            private HttpWebRequest GetHttpWebRequest(string url)
            {
                var httpWebRequest = (HttpWebRequest)GetWebRequest(new Uri(url));
                if (clientIpAddress != null)
                {
                    httpWebRequest.Headers.Add("X-Client-IP", clientIpAddress);
                }

                httpWebRequest.UserAgent = "OpenKit/" + ProtocolConstants.OpenKitVersion;

                return httpWebRequest;
            }
        }

        private class HttpWebRequestAdapter : IHttpRequest
        {
            private readonly HttpWebRequest httpWebRequest;

            internal HttpWebRequestAdapter(HttpWebRequest httpWebRequest)
            {
                this.httpWebRequest = httpWebRequest;
            }

            public Uri Uri => httpWebRequest.RequestUri;

            public string Method => httpWebRequest.Method;

            public Dictionary<string, List<string>> Headers => httpWebRequest.Headers.ToHeadersDictionary();

            public bool ExistsHeader(string name)
            {
                return httpWebRequest.Headers.AllKeys.Any(header => header.Equals(name, StringComparison.OrdinalIgnoreCase));
            }

            public List<string> GetHeader(string name)
            {
                return new List<string>(httpWebRequest.Headers.GetValues(name));
            }

            public void AddHeader(string name, string value)
            {
                if (string.IsNullOrEmpty(name) || RestrictedRequestHeaders.IsHeaderRestricted(name))
                {
                    return;
                }

                if (WebHeaderCollection.IsRestricted(name))
                {
                    // restricted header cannot be set by us in the collection
                    SetFrameworkRestrictedHeader(name, value);
                    return;
                }

                httpWebRequest.Headers.Add(name, value);
            }

            public void RemoveHeader(string name)
            {
                if (string.IsNullOrEmpty(name) || RestrictedRequestHeaders.IsHeaderRestricted(name))
                {
                    return;
                }

                if (WebHeaderCollection.IsRestricted(name))
                {
                    // restricted header cannot be removed directly
                    // Workaround by Adding restricted header with null value
                    SetFrameworkRestrictedHeader(name, null);
                    return;
                }

                httpWebRequest.Headers.Remove(name);
            }

            private void SetFrameworkRestrictedHeader(string name, string value)
            {
                switch (name.ToLowerInvariant())
                {
                    case "referer":
                        httpWebRequest.Referer = value;
                        break;
                    case "user-agent":
                        httpWebRequest.UserAgent = value;
                        break;
                    default:
                        // all other headers restricted by .NET framework
                        // are ignored
                        break;
                }
            }
        }

        private class HttpWebResponseAdapter : IHttpResponse
        {
            private readonly HttpWebRequest httpWebRequest;
            private readonly HttpWebResponse httpWebResponse;

            internal HttpWebResponseAdapter(HttpWebRequest httpWebRequest, HttpWebResponse httpWebResponse)
            {
                this.httpWebRequest = httpWebRequest;
                this.httpWebResponse = httpWebResponse;
            }

            public Uri RequestUri => httpWebRequest.RequestUri;

            public string RequestMethod => httpWebRequest.Method;

            public HttpStatusCode HttpStatusCode => httpWebResponse.StatusCode;

            public string ResponseMessage => httpWebResponse.StatusDescription;

            public Dictionary<string, List<string>> Headers => httpWebResponse.Headers.ToHeadersDictionary();

            public List<string> GetHeader(string name)
            {
                return new List<string>(httpWebRequest.Headers.GetValues(name));
            }
        }
    }

    internal static class WebHeaderCollectionExtensions
    {
        internal static Dictionary<string, List<string>> ToHeadersDictionary(this WebHeaderCollection headers)
        {
            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in headers.AllKeys)
            {
                var values = headers.GetValues(header);
                result.Add(header, new List<string>(values));
            }

            return result;
        }
    }
}
#endif
