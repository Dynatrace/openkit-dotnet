//
// Copyright 2018-2019 Dynatrace LLC
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

using System;
using System.Linq;
using System.Text;
using Dynatrace.OpenKit.Util.Json.Parser;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Protocol
{
    public class JsonResponseParserTest
    {
        private StringBuilder inputBuilder;

        [SetUp]
        public void SetUp()
        {
            inputBuilder = new StringBuilder("{");
        }

        [Test]
        public void ParsingAnEmptyStringThrowsException()
        {
            // given
            const string input = "";

            // when
            Assert.Throws<JsonParserException>(() => JsonResponseParser.Parse(input));
        }

        [Test]
        public void ParsingAnEmptyObjectReturnsInstanceWithDefaultValues()
        {
            // given
            var defaults = ResponseAttributesDefaults.JsonResponse;
            const string input = "{}";

            // when
            var obtained = JsonResponseParser.Parse(input);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxBeaconSizeInBytes, Is.EqualTo(defaults.MaxBeaconSizeInBytes));
            Assert.That(obtained.MaxSessionDurationInMilliseconds,
                Is.EqualTo(defaults.MaxSessionDurationInMilliseconds));
            Assert.That(obtained.MaxEventsPerSession, Is.EqualTo(defaults.MaxEventsPerSession));
            Assert.That(obtained.SessionTimeoutInMilliseconds, Is.EqualTo(defaults.SessionTimeoutInMilliseconds));
            Assert.That(obtained.SendIntervalInMilliseconds, Is.EqualTo(defaults.SendIntervalInMilliseconds));
            Assert.That(obtained.VisitStoreVersion, Is.EqualTo(defaults.VisitStoreVersion));
            Assert.That(obtained.IsCapture, Is.EqualTo(defaults.IsCapture));
            Assert.That(obtained.IsCaptureCrashes, Is.EqualTo(defaults.IsCaptureCrashes));
            Assert.That(obtained.IsCaptureErrors, Is.EqualTo(defaults.IsCaptureErrors));
            Assert.That(obtained.Multiplicity, Is.EqualTo(defaults.Multiplicity));
            Assert.That(obtained.ServerId, Is.EqualTo(defaults.ServerId));
            Assert.That(obtained.TimestampInMilliseconds, Is.EqualTo(defaults.TimestampInMilliseconds));
        }

        [Test]
        public void ParseExtractsMaxBeaconSize()
        {
            // given
            var beaconSize = 73;
            Begin(JsonResponseParser.ResponseKeyAgentConfig);
            AppendLastParameter(JsonResponseParser.ResponseKeyMaxBeaconSizeInKb, beaconSize);
            Close(2);

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxBeaconSizeInBytes, Is.EqualTo(beaconSize * 1024));
        }

        [Test]
        public void ParseExtractsMaxSessionDuration()
        {
            // given
            const int sessionDuration = 73;
            Begin(JsonResponseParser.ResponseKeyAgentConfig);
            AppendLastParameter(JsonResponseParser.ResponseKeyMaxSessionDurationInMin, sessionDuration);
            Close(2);

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxSessionDurationInMilliseconds, Is.EqualTo(sessionDuration * 60 * 1000));
        }

        [Test]
        public void ParseExtractsMaxEventsPerSession()
        {
            // given
            const int eventsPerSession = 73;
            Begin(JsonResponseParser.ResponseKeyAgentConfig);
            AppendLastParameter(JsonResponseParser.ResponseKeyMaxEventsPerSession, eventsPerSession);
            Close(2);

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxEventsPerSession, Is.EqualTo(eventsPerSession));
        }

        [Test]
        public void ParseExtractsSessionTimeout()
        {
            // given
            const int sessionTimeout = 73;
            Begin(JsonResponseParser.ResponseKeyAgentConfig);
            AppendLastParameter(JsonResponseParser.ResponseKeySessionTimeoutInSec, sessionTimeout);
            Close(2);

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.SessionTimeoutInMilliseconds, Is.EqualTo(sessionTimeout * 1000));
        }

        [Test]
        public void ParseExtractsSendInterval()
        {
            // given
            const int sendInterval = 73;
            Begin(JsonResponseParser.ResponseKeyAgentConfig);
            AppendLastParameter(JsonResponseParser.ResponseKeySendIntervalInSec, sendInterval);
            Close(2);

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.SendIntervalInMilliseconds, Is.EqualTo(sendInterval * 1000));
        }

        [Test]
        public void ParseExtractsVisitStoreVersion()
        {
            // given
            const int visitStoreVersion = 73;
            Begin(JsonResponseParser.ResponseKeyAgentConfig);
            AppendLastParameter(JsonResponseParser.ResponseKeyVisitStoreVersion, visitStoreVersion);
            Close(2);

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.VisitStoreVersion, Is.EqualTo(visitStoreVersion));
        }

        [Test]
        public void ParseExtractsCaptureEnabled()
        {
            // given
            Begin(JsonResponseParser.ResponseKeyAppConfig);
            AppendLastParameter(JsonResponseParser.ResponseKeyCapture, 1);
            Close(2);

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCapture, Is.True);
        }

        [Test]
        public void ParseExtractsCaptureDisabled()
        {
            // given
            Begin(JsonResponseParser.ResponseKeyAppConfig);
            AppendLastParameter(JsonResponseParser.ResponseKeyCapture, 0);
            Close(2);

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCapture, Is.False);
        }

        [Test]
        public void ParseExtractsReportCrashesEnabled()
        {
            // given
            Begin(JsonResponseParser.ResponseKeyAppConfig);
            AppendLastParameter(JsonResponseParser.ResponseKeyReportCrashes, 1);
            Close(2);

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCaptureCrashes, Is.True);
        }

        [Test]
        public void ParseExtractsReportCrashesDisabled()
        {
            // given
            Begin(JsonResponseParser.ResponseKeyAppConfig);
            AppendLastParameter(JsonResponseParser.ResponseKeyReportCrashes, 0);
            Close(2);

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCaptureCrashes, Is.False);
        }

        [Test]
        public void ParseExtractsReportErrorsEnabled()
        {
            // given
            Begin(JsonResponseParser.ResponseKeyAppConfig);
            AppendLastParameter(JsonResponseParser.ResponseKeyReportErrors, 1);
            Close(2);

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCaptureErrors, Is.True);
        }

        [Test]
        public void ParseExtractsReportErrorsDisabled()
        {
            // given
            Begin(JsonResponseParser.ResponseKeyAppConfig);
            AppendLastParameter(JsonResponseParser.ResponseKeyReportErrors, 0);
            Close(2);

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCaptureErrors, Is.EqualTo(false));
        }

        [Test]
        public void ParseExtractsApplicationId()
        {
            // given
            var applicationId = Guid.NewGuid().ToString();
            Begin(JsonResponseParser.ResponseKeyAppConfig);
            AppendLastParameter(JsonResponseParser.ResponseKeyApplicationId, applicationId);
            Close(2);

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ApplicationId, Is.EqualTo(applicationId));
        }

        [Test]
        public void ParseExtractsMultiplicity()
        {
            // given
            const int multiplicity = 73;
            Begin(JsonResponseParser.ResponseKeyDynamicConfig);
            AppendLastParameter(JsonResponseParser.ResponseKeyMultiplicity, multiplicity);
            Close(2);

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.Multiplicity, Is.EqualTo(multiplicity));
        }

        [Test]
        public void ParseExtractsServerId()
        {
            // given
            const int serverId = 73;
            Begin(JsonResponseParser.ResponseKeyDynamicConfig);
            AppendLastParameter(JsonResponseParser.ResponseKeyServerId, serverId);
            Close(2);

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ServerId, Is.EqualTo(serverId));
        }

        [Test]
        public void ParseExtractsTimestamp()
        {
            // given
            const long timestamp = 73;
            AppendLastParameter(JsonResponseParser.ResponseKeyTimestampInMillis, timestamp);
            Close();

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TimestampInMilliseconds, Is.EqualTo(timestamp));
        }

        [Test]
        public void ParseResponseWithAllValuesSet()
        {
            // given
            const int beaconSize = 73;
            const int sessionDuration = 74;
            const int eventsPerSession = 75;
            const int sessionTimeout = 76;
            const int sendInterval = 77;
            const int visitStoreVersion = 78;
            const int multiplicity = 79;
            const int serverId = 80;
            const long timestamp = 81;
            var applicationId = Guid.NewGuid().ToString();

            Begin(JsonResponseParser.ResponseKeyAgentConfig);
            AppendParameter(JsonResponseParser.ResponseKeyMaxBeaconSizeInKb, beaconSize);
            AppendParameter(JsonResponseParser.ResponseKeyMaxSessionDurationInMin, sessionDuration);
            AppendParameter(JsonResponseParser.ResponseKeyMaxEventsPerSession, eventsPerSession);
            AppendParameter(JsonResponseParser.ResponseKeySessionTimeoutInSec, sessionTimeout);
            AppendParameter(JsonResponseParser.ResponseKeySendIntervalInSec, sendInterval);
            AppendLastParameter(JsonResponseParser.ResponseKeyVisitStoreVersion, visitStoreVersion);
            Close();
            inputBuilder.Append(",");
            Begin(JsonResponseParser.ResponseKeyAppConfig);
            AppendParameter(JsonResponseParser.ResponseKeyCapture, 0);
            AppendParameter(JsonResponseParser.ResponseKeyReportCrashes, 1);
            AppendParameter(JsonResponseParser.ResponseKeyReportErrors, 0);
            AppendLastParameter(JsonResponseParser.ResponseKeyApplicationId, applicationId);
            Close();
            inputBuilder.Append(",");
            Begin(JsonResponseParser.ResponseKeyDynamicConfig);
            AppendParameter(JsonResponseParser.ResponseKeyMultiplicity, multiplicity);
            AppendLastParameter(JsonResponseParser.ResponseKeyServerId, serverId);
            Close();
            inputBuilder.Append(",");
            AppendLastParameter(JsonResponseParser.ResponseKeyTimestampInMillis, timestamp);
            Close();

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxBeaconSizeInBytes, Is.EqualTo(beaconSize * 1024));
            Assert.That(obtained.MaxSessionDurationInMilliseconds, Is.EqualTo(sessionDuration * 60 * 1000));
            Assert.That(obtained.MaxEventsPerSession, Is.EqualTo(eventsPerSession));
            Assert.That(obtained.SessionTimeoutInMilliseconds, Is.EqualTo(sessionTimeout * 1000));
            Assert.That(obtained.SendIntervalInMilliseconds, Is.EqualTo(sendInterval * 1000));
            Assert.That(obtained.VisitStoreVersion, Is.EqualTo(visitStoreVersion));
            Assert.That(obtained.IsCapture, Is.False);
            Assert.That(obtained.IsCaptureCrashes, Is.True);
            Assert.That(obtained.IsCaptureErrors, Is.False);
            Assert.That(obtained.ApplicationId, Is.EqualTo(applicationId));
            Assert.That(obtained.Multiplicity, Is.EqualTo(multiplicity));
            Assert.That(obtained.ServerId, Is.EqualTo(serverId));
            Assert.That(obtained.TimestampInMilliseconds, Is.EqualTo(timestamp));
            foreach (var attribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                Assert.That(obtained.IsAttributeSet(attribute), Is.True);
            }
        }

        [Test]
        public void ParseIgnoresUnknownTokens()
        {
            // given
            Begin("unknownObject");
            Close();
            inputBuilder.Append(",");
            Begin(JsonResponseParser.ResponseKeyAgentConfig);
            AppendParameter(JsonResponseParser.ResponseKeyMaxEventsPerSession, 999);
            AppendLastParameter("unknownAttribute", 777);
            Close();
            inputBuilder.Append(",");
            AppendLastParameter("anotherUnknownAttribute", 666);
            Close();

            // when
            var obtained = JsonResponseParser.Parse(inputBuilder.ToString());

            // then
            foreach (var attribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                if (attribute == ResponseAttribute.MAX_EVENTS_PER_SESSION)
                {
                    continue;
                }

                Assert.That(obtained.IsAttributeSet(attribute), Is.False);
            }
        }

        private void Begin(string objectName)
        {
            inputBuilder.Append("\"").Append(objectName).Append("\": {");
        }

        private void AppendLastParameter(string key, long value)
        {
            inputBuilder.Append("\"").Append(key).Append("\":").Append(value);
        }

        private void AppendLastParameter(string key, string value)
        {
            inputBuilder.Append("\"").Append(key).Append("\":\"").Append(value).Append("\"");
        }

        private void AppendParameter(string key, long value)
        {
            AppendLastParameter(key, value);
            inputBuilder.Append(",");
        }

        private void Close()
        {
            Close(1);
        }

        private void Close(int numClosingBrackets)
        {
            for (int i = 0;
                i < numClosingBrackets;
                i++)
            {
                inputBuilder.Append("}");
            }
        }
    }
}