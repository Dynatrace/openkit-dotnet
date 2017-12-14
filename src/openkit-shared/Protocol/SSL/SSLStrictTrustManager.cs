using Dynatrace.OpenKit.API;
using System.Net.Security;

namespace Dynatrace.OpenKit.Protocol.SSL
{
    public class SSLStrictTrustManager : ISSLTrustManager
    {
        public RemoteCertificateValidationCallback ServerCertificateValidationCallback
        {
            get
            {
                return (sender, certificate, chain, sslPolicyErrors) => sslPolicyErrors == SslPolicyErrors.None;
            }
        }
    }
}
