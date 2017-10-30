using System.Text;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class DynatraceConfiguration : AbstractConfiguration
    {
        public DynatraceConfiguration(string applicationName, string applicationID, long visitorID, string endpointURL, bool verbose) 
            : base(OpenKitType.DYNATRACE, applicationName, applicationID, visitorID, endpointURL, verbose)
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
