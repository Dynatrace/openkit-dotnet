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

using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Protocol.SSL;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Configuration
{
    class AbstractConfigurationTest
    {
        [Test]
        public void ADefaultConstructedConfigurationDisablesCapturing()
        {
            // given
            var target = new TestConfiguration();

            // then
            Assert.That(target.IsCaptureOn, Is.False);

        }

        [Test]
        public void EnableAndDisableCapturing()
        {
            // given
            var target = new TestConfiguration();

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
            var target = new TestConfiguration();
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
            var target = new TestConfiguration();
            target.EnableCapture();

            // when status response indicates erroneous response
            target.UpdateSettings(new StatusResponse(string.Empty, 400));

            // then
            Assert.That(target.IsCaptureOn, Is.False);
        }

        [Test]
        public void CapturingIsEnabledFromStatusResponse()
        {
            // given
            var target = new TestConfiguration();
            target.EnableCapture();

            var response = new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE + "=" + "1", 200);

            // when capturing is enabled in status response
            target.UpdateSettings(response);

            // then
            Assert.That(target.IsCaptureOn, Is.True);
        }

        [Test]
        public void CapturingIsDisabledFromStatusResponse()
        {
            // given
            var target = new TestConfiguration();
            target.EnableCapture();

            var response = new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE + "=" + "0", 200);

            // when capturing is enabled in status response
            target.UpdateSettings(response);

            // then
            Assert.That(target.IsCaptureOn, Is.False);
        }

        [Test]
        public void ConsecutiveCallsToNextSessionNumberIncrementTheSessionID()
        {
            //given
            var target = new TestConfiguration();

            // when retrieving two sessionIDs
            var sessionIDOne = target.NextSessionNumber;
            var sessionIDTwo = target.NextSessionNumber;

            //then
            Assert.That(sessionIDTwo, Is.EqualTo(sessionIDOne + 1));
        }

        private sealed class TestConfiguration : AbstractConfiguration
        {
            internal TestConfiguration() :
                this(OpenKitType.DYNATRACE, "", "", 42, "", new TestSessionIDProvider())
            {
            }

            internal TestConfiguration(OpenKitType openKitType, string applicationName, string applicationID, long deviceID, string endpointURL, ISessionIDProvider sessionIDProvider) :
                base(openKitType, applicationName, applicationID, deviceID, endpointURL, sessionIDProvider)
            {
                HTTPClientConfig = new HTTPClientConfiguration(endpointURL, 1, applicationID, new SSLStrictTrustManager());
            }

            protected override string CreateBaseURL(string endpointURL, string monitorName)
            {
                return "https://www.dynatrace.com/";
            }
        }
    }
}
