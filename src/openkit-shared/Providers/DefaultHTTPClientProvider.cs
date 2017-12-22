using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Providers
{
    public class DefaultHTTPClientProvider : IHTTPClientProvider
    {
        public IHTTPClient CreateClient(ILogger logger, HTTPClientConfiguration configuration)
        {
#if NET40 || NET35
            return new HTTPClientWebClient(logger, configuration); // HttpClient is not availalbe in .NET 4.0
#else
            return new HTTPClientHttpClient(logger, configuration);
#endif
        }
    }
}
