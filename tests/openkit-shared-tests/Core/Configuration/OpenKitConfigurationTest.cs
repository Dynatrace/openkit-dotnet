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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Protocol.SSL;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class OpenKitConfigurationTest
    {
        private ILogger logger;

        [SetUp]
        public void SetUp()
        {
            logger = Substitute.For<ILogger>();
        }

        [Test]
        public void ADefaultConstructedConfigurationEnablesCapturing()
        {
            // given
            var target = CreateDefaultConfig();

            // then
            Assert.That(target.IsCaptureOn, Is.True);
        }

        [Test]
        public void EnableAndDisableCapturing()
        {
            // given
            var target = CreateDefaultConfig();

            // when capturing is enabled
            target.EnableCapture();

            // then
            Assert.That(target.IsCaptureOn, Is.True);

            // and when capturing is disabled
            target.DisableCapture();

            // then
            Assert.That(target.IsCaptureOn, Is.False);
        }

        [Test]
        public void CapturingIsDisabledIfStatusResponseIsNull()
        {
            // given
            var target = CreateDefaultConfig();
            target.EnableCapture();

            // when status response to handle is null
            target.UpdateSettings(null);

            // then
            Assert.That(target.IsCaptureOn, Is.False);
        }

        [Test]
        public void CapturingIsDisabledIfResponseCodeIndicatesFailures()
        {
            // given
            var target = CreateDefaultConfig();
            target.EnableCapture();

            // when status response indicates erroneous response
            target.UpdateSettings(new StatusResponse(logger, string.Empty, 400, new Dictionary<string, List<string>>()));

            // then
            Assert.That(target.IsCaptureOn, Is.False);
        }

        [Test]
        public void CapturingIsEnabledFromStatusResponse()
        {
            // given
            var target = CreateDefaultConfig();
            target.EnableCapture();

            var response = new StatusResponse(logger, StatusResponse.RESPONSE_KEY_CAPTURE + "=" + "1", 200, new Dictionary<string, List<string>>());

            // when capturing is enabled in status response
            target.UpdateSettings(response);

            // then
            Assert.That(target.IsCaptureOn, Is.True);
        }

        [Test]
        public void CapturingIsDisabledFromStatusResponse()
        {
            // given
            var target = CreateDefaultConfig();
            target.EnableCapture();

            var response = new StatusResponse(logger, StatusResponse.RESPONSE_KEY_CAPTURE + "=" + "0", 200, new Dictionary<string, List<string>>());

            // when capturing is enabled in status response
            target.UpdateSettings(response);

            // then
            Assert.That(target.IsCaptureOn, Is.False);
        }

        [Test]
        public void ConsecutiveCallsToNextSessionNumberIncrementTheSessionID()
        {
            //given
            var target = CreateDefaultConfig();

            // when retrieving two sessionIDs
            var sessionIDOne = target.NextSessionNumber;
            var sessionIDTwo = target.NextSessionNumber;

            //then
            Assert.That(sessionIDTwo, Is.EqualTo(sessionIDOne + 1));
        }

        [Test]
        public void ADefaultConstructedConfigurationDisablesDataCollection()
        {
            // given
            var target = CreateDefaultConfig();

            //when retrieving data collection level
            var dataCollectionLevel = target.BeaconConfig.DataCollectionLevel;

            //then
            Assert.That(dataCollectionLevel, Is.EqualTo(DataCollectionLevel.USER_BEHAVIOR));
        }

        [Test]
        public void ADefaultConstructedConfigurationDisablesCrashReporting()
        {
            // given
            var target = CreateDefaultConfig();

            //when retrieving data collection level
            var crashReportingLevel = target.BeaconConfig.CrashReportingLevel;

            //then
            Assert.That(crashReportingLevel, Is.EqualTo(CrashReportingLevel.OPT_IN_CRASHES));
        }

        [Test]
        public void ADefaultConstructedConfigurationUsesStrictTrustManager()
        {
            // given
            var target = CreateDefaultConfig();

            //when retrieving SSL Trust manager
            var sslTrustManager = target.HTTPClientConfig.SSLTrustManager;

            // then
            Assert.That(sslTrustManager, Is.InstanceOf<SSLStrictTrustManager>());
        }

        [Test]
        public void GetApplicationID()
        {
            // given
            var target = CreateDefaultConfig();

            // then
            Assert.That(target.ApplicationID, Is.EqualTo("/App_ID%"));
        }

        [Test]
        public void GetApplicationIDPercentEncodedDoesPropperEncoding()
        {
            // given
            var target = CreateDefaultConfig();

            // then
            Assert.That(target.ApplicationIDPercentEncoded, Is.EqualTo("%2FApp%5FID%25"));
        }

        private static OpenKitConfiguration CreateDefaultConfig()
        {
            var defaultCacheConfig = new BeaconCacheConfiguration(
                BeaconCacheConfiguration.DEFAULT_MAX_RECORD_AGE_IN_MILLIS,
                BeaconCacheConfiguration.DEFAULT_LOWER_MEMORY_BOUNDARY_IN_BYTES,
                BeaconCacheConfiguration.DEFAULT_UPPER_MEMORY_BOUNDARY_IN_BYTES);

            var defaultBeaconConfig = new BeaconConfiguration();

            return new OpenKitConfiguration(OpenKitType.DYNATRACE, "", "/App_ID%", "0", "", new Providers.TestSessionIDProvider(),
                  new SSLStrictTrustManager(), new Core.Device("", "", ""), "", defaultCacheConfig, defaultBeaconConfig);
        }
    }
}
