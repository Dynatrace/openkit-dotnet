using System.Text;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class DynatraceManagedConfiguration : AbstractConfiguration
    {
        private readonly string tenantID;

        public DynatraceManagedConfiguration(string tenantID, string applicationName, string applicationID, long visitorID, string endpointURL, bool verbose)
            : base(OpenKitType.DYNATRACE, applicationName, applicationID, visitorID, endpointURL, verbose)
        {
            this.tenantID = tenantID;
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
            urlBuilder.Append('/');

            urlBuilder.Append(tenantID);

            return urlBuilder.ToString();
        }
    }
}
