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

using Dynatrace.OpenKit.API;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Protocol
{
    public class TimeSyncResponseTest
    {
        private ILogger logger;

        [SetUp]
        public void SetUp()
        {
            logger = Substitute.For<ILogger>();
        }

        [Test]
        public void PassingNullResponseStringDoesNotThrow()
        {
            // then
            Assert.That(() => new TimeSyncResponse(logger, null, 200, new Dictionary<string, List<string>>()), Throws.Nothing);
        }

        [Test]
        public void PassingEmptyResponseStringDoesNotThrow()
        {
            // then
            Assert.That(() => new TimeSyncResponse(logger, string.Empty, 200, new Dictionary<string, List<string>>()), Throws.Nothing);
        }

        [Test]
        public void TheDefaultRequestReceiveTimeIsMinusOne()
        {
            // given
            var target = new TimeSyncResponse(logger, string.Empty, 200, new Dictionary<string, List<string>>());

            // then
            Assert.That(target.RequestReceiveTime, Is.EqualTo(-1L));
        }

        [Test]
        public void TheDefaultResponseSendTimeIsMinusOne()
        {
            // given
            var target = new TimeSyncResponse(logger, string.Empty, 200, new Dictionary<string, List<string>>());

            // then
            Assert.That(target.ResponseSendTime, Is.EqualTo(-1L));
        }

        [Test]
        public void OddNumberOfTokensThrowsException()
        {
            // given
            const string responseString = TimeSyncResponse.RESPONSE_KEY_RESPONSE_SEND_TIME + "=100"
                + "&" + TimeSyncResponse.RESPONSE_KEY_REQUEST_RECEIVE_TIME;

            // when, then
            Assert.That(() => new TimeSyncResponse(logger, responseString, 200, new Dictionary<string, List<string>>()),
                Throws.ArgumentException.With.Message.EqualTo("Invalid response; even number of tokens expected."));
        }

        [Test]
        public void AmpersandIsNotAValidKeyValueSeparator()
        {
            // given
            const string responseString = TimeSyncResponse.RESPONSE_KEY_RESPONSE_SEND_TIME + "&100";

            // when, then
            Assert.That(() => new TimeSyncResponse(logger, responseString, 200, new Dictionary<string, List<string>>()),
                Throws.ArgumentException.With.Message.EqualTo("Invalid response; even number of tokens expected."));
        }

        [Test]
        public void ResponseSendTimeIsParsed()
        {
            // given
            var target = new TimeSyncResponse(logger, TimeSyncResponse.RESPONSE_KEY_RESPONSE_SEND_TIME + "=100", 200, new Dictionary<string, List<string>>());

            // then
            Assert.That(target.ResponseSendTime, Is.EqualTo(100L));
        }

        [Test]
        public void ParsingInvalidResponseSendTimeThrowsException()
        {
            // when wrong format is used, then
            Assert.That(() => new TimeSyncResponse(logger, TimeSyncResponse.RESPONSE_KEY_RESPONSE_SEND_TIME + "=", 200, new Dictionary<string, List<string>>()),
                Throws.InstanceOf<FormatException>());

            // when wrong format is used, then
            Assert.That(() => new TimeSyncResponse(logger, TimeSyncResponse.RESPONSE_KEY_RESPONSE_SEND_TIME + "=a", 200, new Dictionary<string, List<string>>()),
                Throws.InstanceOf<FormatException>());

            // and when numeric overflow (2^63 in this case) occurs, then
            Assert.That(() => new TimeSyncResponse(logger, TimeSyncResponse.RESPONSE_KEY_RESPONSE_SEND_TIME + "=9223372036854775808", 200, new Dictionary<string, List<string>>()),
                Throws.InstanceOf<OverflowException>());
        }

        [Test]
        public void RequestReceiveTimeIsParsed()
        {
            // given
            var target = new TimeSyncResponse(logger, TimeSyncResponse.RESPONSE_KEY_REQUEST_RECEIVE_TIME + "=42", 200, new Dictionary<string, List<string>>());

            // then
            Assert.That(target.RequestReceiveTime, Is.EqualTo(42L));
        }

        [Test]
        public void ParsingInvalidRequestReceiveTimeThrowsException()
        {
            // when wrong format is used, then
            Assert.That(() => new TimeSyncResponse(logger, TimeSyncResponse.RESPONSE_KEY_REQUEST_RECEIVE_TIME + "=", 200, new Dictionary<string, List<string>>()),
                Throws.InstanceOf<FormatException>());

            // when wrong format is used, then
            Assert.That(() => new TimeSyncResponse(logger, TimeSyncResponse.RESPONSE_KEY_REQUEST_RECEIVE_TIME + "=a", 200, new Dictionary<string, List<string>>()),
                Throws.InstanceOf<FormatException>());

            // and when numeric overflow (2^63 in this case) occurs, then
            Assert.That(() => new TimeSyncResponse(logger, TimeSyncResponse.RESPONSE_KEY_REQUEST_RECEIVE_TIME + "=9223372036854775808", 200, new Dictionary<string, List<string>>()),
                Throws.InstanceOf<OverflowException>());
        }

        [Test]
        public void ParsingUnknownKeyValueDoesNothing()
        {
            // then
            Assert.That(() => new TimeSyncResponse(logger, "key=value", 200, new Dictionary<string, List<string>>()), Throws.Nothing);
        }
    }
}
