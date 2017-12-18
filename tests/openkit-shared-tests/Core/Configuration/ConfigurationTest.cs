using Dynatrace.OpenKit.Protocol.SSL;
using NUnit.Framework;
using System;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class ConfigurationTest
    {
        private const string host = "localhost:9999";
        private const string tenantId = "asdf";
        private const string applicationName = "testApp";

        [Test]
        public void SaasURLIsCorrect()
        {
            var tenantURL = string.Format("https://{0}.{1}", tenantId, host);

            var target = new DynatraceConfiguration("", "", 17, tenantURL, false, new SSLStrictTrustManager());

            var expected = String.Format("{0}/mbeacon", tenantURL);

            Assert.That(expected, Is.EqualTo(target.HTTPClientConfig.BaseURL));
        }

        [Test]
        public void ManagedURLIsCorrect()
        {
            var tenantURL = string.Format("https://{0}", host);

            var target = new DynatraceManagedConfiguration(tenantId, "", "", 17, tenantURL, false, new SSLStrictTrustManager());

            var expected = String.Format("{0}/mbeacon/{1}", tenantURL, tenantId);

            Assert.That(expected, Is.EqualTo(target.HTTPClientConfig.BaseURL));
        }

        [Test]
        public void AppMonURLIsCorrect()
        {
            var tenantURL = string.Format("https://{0}", host);

            var target = new AppMonConfiguration("", 17, tenantURL, false, new SSLStrictTrustManager());

            var expected = String.Format("{0}/dynaTraceMonitor", tenantURL);

            Assert.That(expected, Is.EqualTo(target.HTTPClientConfig.BaseURL));
        }

        [Test]
        public void ApplicationIdAndApplicationNameIdenticalForAppMonConfig()
        {
            var target = new AppMonConfiguration(applicationName, 17, "", false, new SSLStrictTrustManager());

            var expected = applicationName;

            Assert.That(expected, Is.EqualTo(target.ApplicationID));
            Assert.That(expected, Is.EqualTo(target.ApplicationName));
        }
    }
}
