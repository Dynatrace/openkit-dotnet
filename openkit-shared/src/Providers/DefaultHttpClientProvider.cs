using Dynatrace.OpenKit.Providers;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.src.Providers
{
    public class DefaultHTTPClientProvider : IHTTPClientProvider
    {
        public HTTPClient CreateClient(HttpClientConfiguration configuration)
        {
#if NET40
            return new HTTPClientWebClient(configuration.BaseUrl, configuration.ApplicationId, configuration.ServerId, configuration.IsVerbose); // HttpClient is not availalbe in .NET 4.0
#else
            return new HTTPClientHttpClient(configuration.BaseUrl, configuration.ApplicationId, configuration.ServerId, configuration.IsVerbose);
#endif
        }
    }
}
