using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol.SSL;

namespace Dynatrace.OpenKit
{
    public class TestConfiguration : AbstractConfiguration
    {
        public TestConfiguration()
            : base(OpenKitType.DYNATRACE, "", "", 0, "", false)
        {
            HTTPClientConfig = new HTTPClientConfiguration("", 0, "", false, new SSLStrictTrustManager());
            EnableCapture();
        }

        protected override string CreateBaseURL(string endpointURL, string monitorName)
        {
            return "";
        }
    }
}
