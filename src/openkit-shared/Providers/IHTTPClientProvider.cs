using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Providers
{
    public interface IHTTPClientProvider
    {
        /// <summary>
        /// Returns an HTTPClient based on the provided configuration
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        IHTTPClient CreateClient(HTTPClientConfiguration configuration);
    }
}
