using System.Net.Security;

namespace Dynatrace.OpenKit.API
{
    /// <summary>
    /// Interface offering a method to check SSL certificates.
    /// </summary>
    public interface ISSLTrustManager
    {
        RemoteCertificateValidationCallback ServerCertificateValidationCallback { get; }
    }
}
