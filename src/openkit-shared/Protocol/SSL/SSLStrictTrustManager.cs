using Dynatrace.OpenKit.API;
using System.Net.Security;

namespace Dynatrace.OpenKit.Protocol.SSL
{
    /// <summary>
    /// Default SSL Trust Manager, which only accepts valid certificates.
    /// </summary>
    public class SSLStrictTrustManager : ISSLTrustManager
    {
#if !WINDOWS_UWP
        public RemoteCertificateValidationCallback ServerCertificateValidationCallback
        {
            get
            {
                return (sender, certificate, chain, sslPolicyErrors) => sslPolicyErrors == SslPolicyErrors.None;
            }
        }
#endif
    }
}
