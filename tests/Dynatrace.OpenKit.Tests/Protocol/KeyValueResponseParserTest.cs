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
using System.Text;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Protocol
{
    public class KeyValueResponseParserTest
    {
        private StringBuilder inputBuilder;

        [SetUp]
        public void SetUp()
        {
            inputBuilder = new StringBuilder("type=m");
        }

        [Test]
        public void ParsingAnEmptyStringReturnsResponseWithDefaultValues()
        {
            // given
            var defaults = ResponseAttributesDefaults.KeyValueResponse;
            const string input = "";

            // when
            var obtained = KeyValueResponseParser.Parse(input);

            // then
            Assert.That(obtained, Is.Not.Null);

            // then
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
        public void ParsingKeyWithoutValueDelimiterThrowsAnException()
        {
            // given
            inputBuilder.Append("&param");

            // when
            Assert.Throws<ArgumentException>(() => KeyValueResponseParser.Parse(inputBuilder.ToString()),
                "Invalid response; even number of tokens expected.");
        }

        [Test]
        public void ParseExtractsBeaconSize()
        {
            // given
            const int beaconSize = 37;
            AppendParameter(KeyValueResponseParser.ResponseKeyMaxBeaconSizeInKb, beaconSize);

            // when
            var obtained = KeyValueResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxBeaconSizeInBytes, Is.EqualTo(beaconSize * 1024));
        }

        [Test]
        public void ParseExtractsSendInterval()
        {
            // given
            const int sendInterval = 37;
            AppendParameter(KeyValueResponseParser.ResponseKeySendIntervalInSec, sendInterval);

            // when
            var obtained = KeyValueResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.SendIntervalInMilliseconds, Is.EqualTo(sendInterval * 1000));
        }

        [Test]
        public void ParseExtractsCaptureEnabled()
        {
            // given
            AppendParameter(KeyValueResponseParser.ResponseKeyCapture, 1);

            // when
            var obtained = KeyValueResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCapture, Is.True);
        }

        [Test]
        public void ParseExtractsCaptureDisabled()
        {
            // given
            AppendParameter(KeyValueResponseParser.ResponseKeyCapture, 0);

            // when
            var obtained = KeyValueResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCapture, Is.False);
        }

        [Test]
        public void ParseExtractsCaptureCrashesEnabled()
        {
            // given
            AppendParameter(KeyValueResponseParser.ResponseKeyReportCrashes, 1);

            // when
            var obtained = KeyValueResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCaptureCrashes, Is.True);
        }

        [Test]
        public void ParseExtractsCaptureCrashesDisabled()
        {
            // given
            AppendParameter(KeyValueResponseParser.ResponseKeyReportCrashes, 0);

            // when
            var obtained = KeyValueResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCaptureCrashes, Is.False);
        }

        [Test]
        public void ParseExtractsCaptureErrorsEnabled()
        {
            // given
            AppendParameter(KeyValueResponseParser.ResponseKeyReportErrors, 1);

            // when
            var obtained = KeyValueResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCaptureErrors, Is.True);
        }

        [Test]
        public void ParseExtractsCaptureErrorsDisabled()
        {
            // given
            AppendParameter(KeyValueResponseParser.ResponseKeyReportErrors, 0);

            // when
            var obtained = KeyValueResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCaptureErrors, Is.False);
        }

        [Test]
        public void ParseExtractsServerId()
        {
            // given
            const int serverId = 73;
            AppendParameter(KeyValueResponseParser.ResponseKeyServerId, serverId);

            // when
            var obtained = KeyValueResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ServerId, Is.EqualTo(serverId));
        }

        [Test]
        public void ParseExtractsMultiplicity()
        {
            // given
            const int multiplicity = 73;
            AppendParameter(KeyValueResponseParser.ResponseKeyMultiplicity, multiplicity);

            // when
            var obtained = KeyValueResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.Multiplicity, Is.EqualTo(multiplicity));
        }

        [Test]
        public void ParseResponseWithAllParametersSet()
        {
            // given
            var defaults = ResponseAttributesDefaults.KeyValueResponse;
            const int beaconSize = 73;
            const int sendInterval = 74;
            const int serverId = 75;
            const int multiplicity = 76;

            AppendParameter(KeyValueResponseParser.ResponseKeyMaxBeaconSizeInKb, beaconSize);
            AppendParameter(KeyValueResponseParser.ResponseKeySendIntervalInSec, sendInterval);
            AppendParameter(KeyValueResponseParser.ResponseKeyCapture, 0);
            AppendParameter(KeyValueResponseParser.ResponseKeyReportCrashes, 1);
            AppendParameter(KeyValueResponseParser.ResponseKeyReportErrors, 0);
            AppendParameter(KeyValueResponseParser.ResponseKeyServerId, serverId);
            AppendParameter(KeyValueResponseParser.ResponseKeyMultiplicity, multiplicity);

            // when
            var obtained = KeyValueResponseParser.Parse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxBeaconSizeInBytes, Is.EqualTo(beaconSize * 1024));
            Assert.That(obtained.IsAttributeSet(ResponseAttribute.MAX_BEACON_SIZE), Is.True);

            Assert.That(obtained.SendIntervalInMilliseconds, Is.EqualTo(sendInterval * 1000));
            Assert.That(obtained.IsAttributeSet(ResponseAttribute.SEND_INTERVAL), Is.True);

            Assert.That(obtained.IsCapture, Is.False);
            Assert.That(obtained.IsAttributeSet(ResponseAttribute.IS_CAPTURE), Is.True);

            Assert.That(obtained.IsCaptureCrashes, Is.True);
            Assert.That(obtained.IsAttributeSet(ResponseAttribute.IS_CAPTURE_CRASHES), Is.True);

            Assert.That(obtained.IsCaptureErrors, Is.False);
            Assert.That(obtained.IsAttributeSet(ResponseAttribute.IS_CAPTURE_ERRORS), Is.True);

            Assert.That(obtained.Multiplicity, Is.EqualTo(multiplicity));
            Assert.That(obtained.IsAttributeSet(ResponseAttribute.MULTIPLICITY), Is.True);

            Assert.That(obtained.ServerId, Is.EqualTo(serverId));
            Assert.That(obtained.IsAttributeSet(ResponseAttribute.SERVER_ID), Is.True);

            Assert.That(obtained.MaxSessionDurationInMilliseconds,
                Is.EqualTo(defaults.MaxSessionDurationInMilliseconds));
            Assert.That(obtained.IsAttributeSet(ResponseAttribute.MAX_SESSION_DURATION), Is.False);
            Assert.That(obtained.MaxEventsPerSession, Is.EqualTo(defaults.MaxEventsPerSession));
            Assert.That(obtained.IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION), Is.False);
            Assert.That(obtained.SessionTimeoutInMilliseconds, Is.EqualTo(defaults.SessionTimeoutInMilliseconds));
            Assert.That(obtained.IsAttributeSet(ResponseAttribute.SESSION_IDLE_TIMEOUT), Is.False);
            Assert.That(obtained.VisitStoreVersion, Is.EqualTo(defaults.VisitStoreVersion));
            Assert.That(obtained.IsAttributeSet(ResponseAttribute.VISIT_STORE_VERSION), Is.False);
            Assert.That(obtained.TimestampInMilliseconds, Is.EqualTo(defaults.TimestampInMilliseconds));
            Assert.That(obtained.IsAttributeSet(ResponseAttribute.TIMESTAMP), Is.False);
        }

        private void AppendParameter(string key, int value)
        {
            AppendParameter(key, value.ToString());
        }

        private void AppendParameter(string key, string value)
        {
            inputBuilder.Append("&").Append(key).Append("=").Append(value);
        }
    }
}