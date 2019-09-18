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

using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.API;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Protocol
{
#if NET40 || NET35

    public class HttpClientWebClient : HttpClient
    {
        private static bool _remoteCertificateValidationCallbackInitialized = false;

        public HttpClientWebClient(ILogger logger, IHttpClientConfiguration configuration) : base(logger, configuration)
        {
            if (!_remoteCertificateValidationCallbackInitialized)
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback += configuration.SslTrustManager?.ServerCertificateValidationCallback;
                _remoteCertificateValidationCallbackInitialized = true;
            }
        }

        protected override HttpResponse GetRequest(string url, string clientIpAddress)
        {
            using (var webClient = new WrappedWebClient(clientIpAddress))
            {
                using (System.Net.HttpWebResponse response = webClient.Get(url))
                {
                    return ReadResponse(response);
                }
            }
        }

        protected override HttpResponse PostRequest(string url, string clientIpAddress, byte[] gzippedPayload)
        {
            using (var webClient = new WrappedWebClient(clientIpAddress))
            {
                using (System.Net.HttpWebResponse response = webClient.Post(url, gzippedPayload))
                {
                    return ReadResponse(response);
                }
            }
        }

        private static HttpResponse ReadResponse(System.Net.WebResponse webResponse)
        {
            string response;
            using (var reader = new System.IO.StreamReader(webResponse.GetResponseStream()))
            {
                response = reader.ReadToEnd();
            }
            var responseCode = ((System.Net.HttpWebResponse)webResponse).StatusCode;
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

        private class WrappedWebClient : System.Net.WebClient
        {
            public WrappedWebClient(string clientIpAddress)
            {
                if (clientIpAddress != null)
                {
                    Headers.Add("X-Client-IP", clientIpAddress);
                }
            }

            public System.Net.HttpWebResponse Get(string url)
            {
                var webRequest = (System.Net.HttpWebRequest)GetWebRequest(new System.Uri(url));
                return (System.Net.HttpWebResponse)webRequest.GetResponse();
            }

            public System.Net.HttpWebResponse Post(string url, byte[] gzippedPayload)
            {
                var request = (System.Net.HttpWebRequest)GetWebRequest(new System.Uri(url));
                request.Method = "POST";

                // if there is compressed data, post it to the server
                if (gzippedPayload != null && gzippedPayload.Length > 0)
                {
                    request.ContentLength = gzippedPayload.Length;
                    request.Headers.Add("Content-Encoding", "gzip");

                    Headers["Content-Encoding"] = "gzip";

                    using(System.IO.Stream stream = request.GetRequestStream())
                    {
                        stream.Write(gzippedPayload, 0, gzippedPayload.Length);
                        stream.Close();
                    }
                }

                // get servers response
                return (System.Net.HttpWebResponse)request.GetResponse();
            }
        }
    }

#endif
}
