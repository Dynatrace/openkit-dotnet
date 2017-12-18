using Dynatrace.OpenKit.Core.Configuration;

namespace Dynatrace.OpenKit.Protocol
{
#if (!NET40 && !NET35)

    public class HTTPClientHttpClient : HTTPClient
    {
        private readonly System.Net.Security.RemoteCertificateValidationCallback remoteCertificateValidationCallback;

        public HTTPClientHttpClient(HTTPClientConfiguration configuration) : base(configuration)
        {
            remoteCertificateValidationCallback = configuration.SSLTrustManager?.ServerCertificateValidationCallback;
        }

        protected override HTTPResponse GetRequest(string url, string clientIPAddress)
        {
            using (System.Net.Http.HttpClient httpClient = CreateHTTPClient(clientIPAddress))
            {
                System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> responseTask = httpClient.GetAsync(url);
                responseTask.Wait();

                return CreateHTTPResponse(responseTask.Result);
            }
        }

        protected override HTTPResponse PostRequest(string url, string clientIPAddress, byte[] gzippedPayload)
        {
            using (System.Net.Http.HttpClient httpClient = CreateHTTPClient(clientIPAddress))
            {
                System.Net.Http.ByteArrayContent content = CreatePostContent(gzippedPayload);
                System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> responseTask = responseTask = httpClient.PostAsync(url, content);
                responseTask.Wait();

                return CreateHTTPResponse(responseTask.Result);
            }
        }

        private System.Net.Http.HttpClient CreateHTTPClient (string clientIPAddress)
        {
            System.Net.Http.HttpClient httpClient;

            if (remoteCertificateValidationCallback == null)
            {
                httpClient = new System.Net.Http.HttpClient();
            }
            else
            {
#if NET45 || NET46
                // special handling for .NET 4.5 & 4.6, since the HttpClientHandler does not have the ServerCertificateValidationCallback
                System.Net.Http.WebRequestHandler webRequestHandler = new System.Net.Http.WebRequestHandler();
                webRequestHandler.ServerCertificateValidationCallback += remoteCertificateValidationCallback;
                httpClient = new System.Net.Http.HttpClient(webRequestHandler, true);
#else
                System.Net.Http.HttpClientHandler httpClientHandler = new System.Net.Http.HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = remoteCertificateValidationCallback.Invoke
                };
                httpClient = new System.Net.Http.HttpClient(httpClientHandler, true);
#endif
            }

            if (clientIPAddress != null)
            {
                httpClient.DefaultRequestHeaders.Add("X-Client-IP", clientIPAddress);
            }

            return httpClient;
        }

        private static System.Net.Http.ByteArrayContent CreatePostContent(byte[] gzippedPayload)
        {
            if (gzippedPayload == null || gzippedPayload.Length == 0)
                return new System.Net.Http.ByteArrayContent(new byte[] { });

            System.Net.Http.ByteArrayContent content = new System.Net.Http.ByteArrayContent(gzippedPayload);
            content.Headers.Add("Content-Encoding", "gzip");
            content.Headers.Add("Content-Length", gzippedPayload.Length.ToString());

            return content;
        }

        private static HTTPResponse CreateHTTPResponse(System.Net.Http.HttpResponseMessage result)
        {
            System.Threading.Tasks.Task<string> httpResponseContentTask = result.Content.ReadAsStringAsync();
            httpResponseContentTask.Wait();
            string response = httpResponseContentTask.Result;
            System.Net.HttpStatusCode responseCode = result.StatusCode;

            return new HTTPResponse
            {
                Response = response,
                ResponseCode = (int)responseCode
            };
        }
    }

#endif
            }
