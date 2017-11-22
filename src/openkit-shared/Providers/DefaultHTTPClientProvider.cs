using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Providers
{
    public class DefaultHTTPClientProvider : IHTTPClientProvider
    {
        public IHTTPClient CreateClient(HTTPClientConfiguration configuration)
        {
#if NET40 || NET35
            return new HTTPClientWebClient(configuration.BaseUrl, configuration.ApplicationID, configuration.ServerID, configuration.IsVerbose); // HttpClient is not availalbe in .NET 4.0
#else
            return new HTTPClientHttpClient(configuration.BaseUrl, configuration.ApplicationID, configuration.ServerID, configuration.IsVerbose);
#endif
        }
    }
}
