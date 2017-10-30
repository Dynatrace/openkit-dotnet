using System.Text;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class AppMonConfiguration : AbstractConfiguration
    {
        public AppMonConfiguration(string applicationName, string applicationID, long visitorID, string endpointURL, bool verbose) 
            : base(OpenKitType.APPMON, applicationName, applicationID, visitorID, endpointURL, verbose)
        {
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
