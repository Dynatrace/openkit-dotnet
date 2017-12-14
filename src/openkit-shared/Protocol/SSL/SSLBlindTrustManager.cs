using System.Net.Security;
using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Protocol.SSL
{
    public class SSLBlindTrustManager : ISSLTrustManager
    {
        public RemoteCertificateValidationCallback ServerCertificateValidationCallback
        {
            get
            {
                return (sender, certificate, chain, sslPolicyErrors) => true;
            }
        }
    }
}
