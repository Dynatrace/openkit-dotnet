using System.Text;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class AppMonConfiguration : AbstractConfiguration
    {
        /// <summary>
        /// Constructs new instance of AppMonConfiguration
        /// For AppMon applicationId and applicationName are identical. Use application name to initialize both fields.
        /// </summary>
        /// <param name="applicationName">name of the application</param>
        /// <param name="visitorID">id identifying the visitor</param>
        /// <param name="endpointURL">URL of the endpoint</param>
        /// <param name="verbose">if set to <code>true</code> enables debug output</param>
        public AppMonConfiguration(string applicationName, long visitorID, string endpointURL, bool verbose) 
            : base(OpenKitType.APPMON, applicationName, applicationName, visitorID, endpointURL, verbose)
        {
            HttpClientConfig = new HTTPClientConfiguration(
                    CreateBaseURL(endpointURL, OpenKitType.APPMON.DefaultMonitorName),
                    OpenKitType.APPMON.DefaultServerID,
                    applicationName,
                    verbose);
        }

        protected override string CreateBaseURL(string endpointURL, string monitorName)
        {
            StringBuilder urlBuilder = new StringBuilder();

            urlBuilder.Append(endpointURL);
            if (!endpointURL.EndsWith("/") && !monitorName.StartsWith("/"))
            {
                urlBuilder.Append('/');
            }
            urlBuilder.Append(monitorName);

            return urlBuilder.ToString();
        }
    }
}
