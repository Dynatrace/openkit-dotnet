using System.Text;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class DynatraceManagedConfiguration : AbstractConfiguration
    {
        private readonly string tenantId;

        public DynatraceManagedConfiguration(string tenantId, string applicationName, string applicationID, long visitorID, string endpointURL, bool verbose)
            : base(OpenKitType.DYNATRACE, applicationName, applicationID, visitorID, endpointURL, verbose)
        {
            this.tenantId = tenantId;
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

            urlBuilder.Append(tenantId);

            return urlBuilder.ToString();
        }
    }
}
