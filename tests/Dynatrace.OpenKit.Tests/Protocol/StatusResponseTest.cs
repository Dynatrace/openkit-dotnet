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

using System.Collections.Generic;
using Dynatrace.OpenKit.API;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Protocol
{
    public class StatusResponseTest
    {
        private ILogger logger;
        private IResponseAttributes attributes;

        [SetUp]
        public void SetUp()
        {
            logger = Substitute.For<ILogger>();
            attributes = ResponseAttributes.WithJsonDefaults().Build();
        }

        [Test]
        public void IsErroneousResponseGivesTrueForErrorCodeEqualTo400()
        {
            // when, then
            Assert.That(StatusResponse.CreateErrorResponse(logger, 400).IsErroneousResponse, Is.True);
        }

        [Test]
        public void IsErroneousResponseGivesTrueForErrorCodeGreaterThan400()
        {
            // when, then
            Assert.That(StatusResponse.CreateErrorResponse(logger, 401).IsErroneousResponse, Is.True);
        }

        [Test]
        public void IsErroneousResponseGivesFalseForErrorCodeLessThan400()
        {
            // when, then
            Assert.That(
                StatusResponse.CreateSuccessResponse(logger, attributes, 399, new Dictionary<string, List<string>>())
                    .IsErroneousResponse, Is.False);
        }

        [Test]
        public void ResponseCodeIsSet()
        {
            // given
            Assert.That(StatusResponse.CreateErrorResponse(logger, 418).ResponseCode, Is.EqualTo(418));
        }

        [Test]
        public void HeadersAreSet()
        {
            // given
            var headers = new Dictionary<string, List<string>>
            {
                {"X-Foo", new List<string> {"X-BAR"}},
                {"X-YZ", new List<string>()}
            };

            // then
            Assert.That(StatusResponse.CreateSuccessResponse(logger, attributes, 418, headers).Headers,
                Is.EqualTo(headers));
        }

        [Test]
        public void GetRetryAfterReturnsDefaultValueIfResponseKeyDoesNotExist()
        {
            // given
            var target =
                StatusResponse.CreateSuccessResponse(logger, attributes, 429, new Dictionary<string, List<string>>());

            // when
            var obtained = target.GetRetryAfterInMilliseconds();

            // then
            Assert.That(obtained, Is.EqualTo(StatusResponse.DefaultRetryAfterInMilliseconds));
        }

        [Test]
        public void GetRetryAfterReturnsDefaultValueIfMultipleValuesWereRetrieved()
        {
            // given
            var responseHeaders = new Dictionary<string, List<string>>
            {
                {StatusResponse.ResponseKeyRetryAfter, new List<string> {"100", "200"}}
            };
            var target = StatusResponse.CreateSuccessResponse(logger, attributes, 429, responseHeaders);

            // when
            var obtained = target.GetRetryAfterInMilliseconds();

            // then
            Assert.That(obtained, Is.EqualTo(StatusResponse.DefaultRetryAfterInMilliseconds));
        }

        [Test]
        public void GetRetryAfterReturnsDefaultValueIfValueIsNotParsableAsInteger()
        {
            // given
            var responseHeaders = new Dictionary<string, List<string>>
            {
                {StatusResponse.ResponseKeyRetryAfter, new List<string> {"a"}}
            };
            var target = StatusResponse.CreateSuccessResponse(logger, attributes, 429, responseHeaders);

            // when
            var obtained = target.GetRetryAfterInMilliseconds();

            // then
            Assert.That(obtained, Is.EqualTo(StatusResponse.DefaultRetryAfterInMilliseconds));
        }

        [Test]
        public void GetRetryAfterReturnsParsedValue()
        {
            // given
            var responseHeaders = new Dictionary<string, List<string>>
            {
                {StatusResponse.ResponseKeyRetryAfter, new List<string> {"1234"}}
            };
            var target = StatusResponse.CreateSuccessResponse(logger, attributes, 429, responseHeaders);

            // when
            var obtained = target.GetRetryAfterInMilliseconds();

            // then
            Assert.That(obtained, Is.EqualTo(1234L * 1000L));
        }

        [Test]
        public void ErrorResponseDefaultCaptureIsOn()
        {
            // given
            var target = StatusResponse.CreateErrorResponse(logger, 200);

            // then
            Assert.That(target.ResponseAttributes.IsCapture, Is.True);
        }

        [Test]
        public void ErrorResponseDefaultSendIntervalIs120Sec()
        {
            // given
            var target = StatusResponse.CreateErrorResponse(logger, 200);

            // then
            Assert.That(target.ResponseAttributes.SendIntervalInMilliseconds, Is.EqualTo(120 * 1000)); // 120 sec
        }

        [Test]
        public void ErrorResponseDefaultServerIdIsMinusOne()
        {
            // given
            var target = StatusResponse.CreateErrorResponse(logger, 200);

            // then
            Assert.That(target.ResponseAttributes.ServerId, Is.EqualTo(-1));
        }

        [Test]
        public void ErrorResponseDefaultMaxBeaconSizeIsThirtyKb()
        {
            // given
            var target = StatusResponse.CreateErrorResponse(logger, 200);

            // then
            Assert.That(target.ResponseAttributes.MaxBeaconSizeInBytes, Is.EqualTo(30 * 1024));
        }

        [Test]
        public void ErrorResponseDefaultCaptureCrashesIsOn()
        {
            // given
            var target = StatusResponse.CreateErrorResponse(logger, 200);

            // then
            Assert.That(target.ResponseAttributes.IsCaptureCrashes, Is.True);
        }

        [Test]
        public void ErrorResponseDefaultCaptureErrorsIsOn()
        {
            // given
            var target = StatusResponse.CreateErrorResponse(logger, 200);

            // then
            Assert.That(target.ResponseAttributes.IsCaptureErrors, Is.True);
        }

        [Test]
        public void ErrorResponseDefaultMultiplicityIsOne()
        {
            // given
            var target = StatusResponse.CreateErrorResponse(logger, 200);

            // then
            Assert.That(target.ResponseAttributes.Multiplicity, Is.EqualTo(1));
        }

        [Test]
        public void SuccessResponseGetResponseAttributesReturnsAttributesPassedInConstructor()
        {
            // given
            var target =
                StatusResponse.CreateSuccessResponse(logger, attributes, 200, new Dictionary<string, List<string>>());

            // then
            Assert.That(target.ResponseAttributes, Is.SameAs(attributes));
        }
    }
}