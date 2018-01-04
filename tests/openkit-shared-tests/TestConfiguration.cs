using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol.SSL;

namespace Dynatrace.OpenKit
{
    public class TestConfiguration : OpenKitConfiguration
    {
        public TestConfiguration()
            : base(OpenKitType.DYNATRACE, "", "", 0, "", new Providers.TestSessionIDProvider(), 
                  new SSLStrictTrustManager(), new Core.Device("", "", ""), "")
        {
            EnableCapture();
        }
    }
}
