using Dynatrace.OpenKit.Core.Configuration;

namespace Dynatrace.OpenKit
{
    public class TestConfiguration : AbstractConfiguration
    {
        public TestConfiguration()
            : base(OpenKitType.DYNATRACE, "", "", 0, "", false)
        {
            HttpClientConfig = new HTTPClientConfiguration("", 0, "", false);
        }

        protected override string CreateBaseURL(string endpointURL, string monitorName)
        {
            return "";
        }
    }
}
