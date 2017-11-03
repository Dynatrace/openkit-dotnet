using System;

namespace Dynatrace.OpenKit.Protocol
{
#if NET40

    public class HTTPClientWebClient : HTTPClient
    {

        public HTTPClientWebClient(string baseURL, string applicationID, int serverID, bool verbose) : base(baseURL, applicationID, serverID, verbose)
        {
        }

        protected override HTTPResponse GetRequest(string url, string clientIPAddress)
        {
            using (MyWebClient webClient = new MyWebClient(clientIPAddress))
            {
                using (System.Net.HttpWebResponse response = webClient.Get(url))
                {
                    return ReadResponse(response);
                }
            }
        }

        protected override HTTPResponse PostRequest(string url, string clientIPAddress, byte[] gzippedPayload)
        {
            using (MyWebClient webClient = new MyWebClient(clientIPAddress))
            {
                using (System.Net.HttpWebResponse response = webClient.Post(url, gzippedPayload))
                {
                    return ReadResponse(response);
                }
            }
        }

        private static HTTPResponse ReadResponse(System.Net.WebResponse webResponse)
        {
            string response;
            using (System.IO.StreamReader reader = new System.IO.StreamReader(webResponse.GetResponseStream()))
            {
                response = reader.ReadToEnd();
            }
            System.Net.HttpStatusCode responseCode = ((System.Net.HttpWebResponse)webResponse).StatusCode;

            return new HTTPResponse
            {
                Response = response,
                ResponseCode = (int)responseCode
            };
        }

        private class MyWebClient : System.Net.WebClient
        {
            public MyWebClient(string clientIPAddress)
            {
                if (clientIPAddress != null)
                {
                    Headers.Add("X-Client-IP", clientIPAddress);
                }
            }

            public System.Net.HttpWebResponse Get(string url)
            {
                System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)GetWebRequest(new Uri(url));
                return (System.Net.HttpWebResponse)webRequest.GetResponse();
            }

            public System.Net.HttpWebResponse Post(string url, byte[] gzippedPayload)
            {
                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)GetWebRequest(new Uri(url));
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
