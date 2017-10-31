using System;
using System.Collections.Generic;
using System.Text;

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
            throw new NotImplementedException();
        }

        protected override HTTPResponse PostRequest(string url, string clientIPAddress, byte[] gzippedPayload)
        {
            throw new NotImplementedException();
        }
    }

#endif
}
