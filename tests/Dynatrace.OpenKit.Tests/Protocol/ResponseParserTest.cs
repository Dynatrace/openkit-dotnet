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

using System.Text;
using Dynatrace.OpenKit.Util.Json.Parser;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Protocol
{
    public class ResponseParserTest
    {
        [Test]
        public void ParsingEmptyStringThrowsException()
        {
            // given
            const string input = "";

            // when
            Assert.Throws<JsonParserException>(() => ResponseParser.ParseResponse(input));
        }

        [Test]
        public void ParsingArbitraryResponseThrowsException()
        {
            // given
            const string input = "some response text";

            // when
            Assert.Throws<JsonParserException>(() => ResponseParser.ParseResponse(input));
        }

        [Test]
        public void ParseKeyValueResponseWorks()
        {
            // given
            const string input = "type=m&bl=17&id=18&cp=0";

            // when
            var obtained = ResponseParser.ParseResponse(input);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxBeaconSizeInBytes, Is.EqualTo(17 * 1024));
            Assert.That(obtained.ServerId, Is.EqualTo(18));
            Assert.That(obtained.IsCapture, Is.False);
        }

        [Test]
        public void ParseWithPartiallyMatchingKeyValuePrefixThrowsException()
        {
            // given
            const string input = "type=mobile&bl=17";

            // when
            Assert.Throws<JsonParserException>(() => ResponseParser.ParseResponse(input));
        }

        [Test]
        public void ParseWithOnlyKeyValuePrefixReturnsDefaultResponse()
        {
            // given
            var defaults = ResponseAttributesDefaults.KeyValueResponse;
            const string input = "type=m";

            // when
            var obtained = ResponseParser.ParseResponse(input);

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
        public void ParseJsonResponseWorks()
        {
            // given
            var inputBuilder = new StringBuilder();
            inputBuilder.Append("{");
            inputBuilder.Append("\"").Append(JsonResponseParser.ResponseKeyAgentConfig).Append("\": {");
            inputBuilder.Append("\"").Append(JsonResponseParser.ResponseKeyMaxBeaconSizeInKb).Append("\": 17");
            inputBuilder.Append("},");
            inputBuilder.Append("\"").Append(JsonResponseParser.ResponseKeyAppConfig).Append("\": {");
            inputBuilder.Append("\"").Append(JsonResponseParser.ResponseKeyCapture).Append("\": 0");
            inputBuilder.Append("},");
            inputBuilder.Append("\"").Append(JsonResponseParser.ResponseKeyDynamicConfig).Append("\": {");
            inputBuilder.Append("\"").Append(JsonResponseParser.ResponseKeyServerId).Append("\": 18");
            inputBuilder.Append("},");
            inputBuilder.Append("\"").Append(JsonResponseParser.ResponseKeyTimestampInMillis).Append("\": 19");
            inputBuilder.Append("}");

            // when
            var obtained = ResponseParser.ParseResponse(inputBuilder.ToString());

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxBeaconSizeInBytes, Is.EqualTo(17 * 1024));
            Assert.That(obtained.ServerId, Is.EqualTo(18));
            Assert.That(obtained.IsCapture, Is.False);
            Assert.That(obtained.TimestampInMilliseconds, Is.EqualTo(19L));
        }
    }
}