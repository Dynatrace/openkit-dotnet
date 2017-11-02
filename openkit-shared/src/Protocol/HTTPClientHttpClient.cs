namespace Dynatrace.OpenKit.Protocol
{
#if !NET40

    public class HTTPClientHttpClient : HTTPClient
    {
        public HTTPClientHttpClient(string baseURL, string applicationID, int serverID, bool verbose) : base(baseURL, applicationID, serverID, verbose)
        {
        }

        protected override HTTPResponse GetRequest(string url, string clientIPAddress)
        {
            using (System.Net.Http.HttpClient httpClient = CreateHttpClient(clientIPAddress))
            {
                System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> responseTask = httpClient.GetAsync(url);
                responseTask.Wait();

                return CreateHttpResponse(responseTask.Result);
            }
        }

        protected override HTTPResponse PostRequest(string url, string clientIPAddress, byte[] gzippedPayload)
        {
            using (System.Net.Http.HttpClient httpClient = CreateHttpClient(clientIPAddress))
            {
                System.Net.Http.ByteArrayContent content = CreatePostContent(gzippedPayload);
                System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> responseTask = responseTask = httpClient.PostAsync(url, content);
                responseTask.Wait();

                return CreateHttpResponse(responseTask.Result);
            }
        }

        private static System.Net.Http.HttpClient CreateHttpClient (string clientIPAddress)
        {
            System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();
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

        private static HTTPResponse CreateHttpResponse(System.Net.Http.HttpResponseMessage result)
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
