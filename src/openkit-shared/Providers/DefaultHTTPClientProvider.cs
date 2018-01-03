using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Providers
{
    public class DefaultHTTPClientProvider : IHTTPClientProvider
    {
        private readonly ILogger logger;

        public DefaultHTTPClientProvider(ILogger logger)
        {
            this.logger = logger;
        }

        public IHTTPClient CreateClient(HTTPClientConfiguration configuration)
        {
#if NET40 || NET35
            return new HTTPClientWebClient(logger, configuration); // HttpClient is not availalbe in .NET 4.0
#else
            return new HTTPClientHttpClient(logger, configuration);
#endif
        }
    }
}
