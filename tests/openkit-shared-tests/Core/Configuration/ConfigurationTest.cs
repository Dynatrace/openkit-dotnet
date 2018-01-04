//
// Copyright 2018 Dynatrace LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

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

            var target = new DynatraceConfiguration("", "", 17, tenantURL, new SSLStrictTrustManager(), new Providers.TestSessionIDProvider());

            var expected = String.Format("{0}/mbeacon", tenantURL);

            Assert.That(expected, Is.EqualTo(target.HTTPClientConfig.BaseURL));
        }

        [Test]
        public void ManagedURLIsCorrect()
        {
            var tenantURL = string.Format("https://{0}", host);

            var target = new DynatraceManagedConfiguration(tenantId, "", "", 17, tenantURL, new SSLStrictTrustManager(), new Providers.TestSessionIDProvider());

            var expected = String.Format("{0}/mbeacon/{1}", tenantURL, tenantId);

            Assert.That(expected, Is.EqualTo(target.HTTPClientConfig.BaseURL));
        }

        [Test]
        public void AppMonURLIsCorrect()
        {
            var tenantURL = string.Format("https://{0}", host);

            var target = new AppMonConfiguration("", 17, tenantURL, new SSLStrictTrustManager(), new Providers.TestSessionIDProvider());

            var expected = String.Format("{0}/dynaTraceMonitor", tenantURL);

            Assert.That(expected, Is.EqualTo(target.HTTPClientConfig.BaseURL));
        }

        [Test]
        public void ApplicationIdAndApplicationNameIdenticalForAppMonConfig()
        {
            var target = new AppMonConfiguration(applicationName, 17, "", new SSLStrictTrustManager(), new Providers.TestSessionIDProvider());

            var expected = applicationName;

            Assert.That(expected, Is.EqualTo(target.ApplicationID));
            Assert.That(expected, Is.EqualTo(target.ApplicationName));
        }
    }
}
