using System;
using System.Net.Security;
using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Protocol.SSL
{
    /// <summary>
    /// Implementation of <see cref="SSLTrustManager"/> blindly trusting every certificate and every host.
    /// </summary>
    /// 
    /// <remarks>
    /// This class is intended to be used only during development phase. Since local
    /// development environments use self-signed certificates only.
    /// 
    /// This implementation disables any X509 certificate validation & hostname validation.
    /// 
    /// NOTE: DO NOT USE THIS IN PRODUCTION!!
    /// </remarks>
    public class SSLBlindTrustManager : ISSLTrustManager
    {
        public SSLBlindTrustManager()
        {
            Console.WriteLine("###########################################################");
            Console.WriteLine("# WARNING: YOU ARE BYPASSING SSL CERTIFICATE VALIDATION!! #");
            Console.WriteLine("#                 USE AT YOUR OWN RISK!!                  #");
            Console.WriteLine("###########################################################");
        }

        public RemoteCertificateValidationCallback ServerCertificateValidationCallback
        {
            get
            {
                return (sender, certificate, chain, sslPolicyErrors) => true;
            }
        }
    }
}
