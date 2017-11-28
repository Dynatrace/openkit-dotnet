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
        public void SaasUrlIsCorrect()
        {
            var tenantUrl = string.Format("https://{0}.{1}", tenantId, host);

            var target = new DynatraceConfiguration("", "", 17, tenantUrl, false);

            var expected = String.Format("{0}/mbeacon", tenantUrl);

            Assert.That(expected, Is.EqualTo(target.HttpClientConfig.BaseUrl));
        }

        [Test]
        public void ManagedUrlIsCorrect()
        {
            var tenantUrl = string.Format("https://{0}", host);

            var target = new DynatraceManagedConfiguration(tenantId, "", "", 17, tenantUrl, false);

            var expected = String.Format("{0}/mbeacon/{1}", tenantUrl, tenantId);

            Assert.That(expected, Is.EqualTo(target.HttpClientConfig.BaseUrl));
        }

        [Test]
        public void AppMonUrlIsCorrect()
        {
            var tenantUrl = string.Format("https://{0}", host);

            var target = new AppMonConfiguration("", 17, tenantUrl, false);

            var expected = String.Format("{0}/dynaTraceMonitor", tenantUrl);

            Assert.That(expected, Is.EqualTo(target.HttpClientConfig.BaseUrl));
        }

        [Test]
        public void ApplicationIdAndApplicationNameIdenticalForAppMonConfig()
        {
            var target = new AppMonConfiguration(applicationName, 17, "", false);

            var expected = applicationName;

            Assert.That(expected, Is.EqualTo(target.ApplicationID));
            Assert.That(expected, Is.EqualTo(target.ApplicationName));
        }
    }
}
