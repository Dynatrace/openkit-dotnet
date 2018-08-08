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
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Protocol
{
    public class ResponseTest
    {
        private ILogger mockLogger;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
        }

        [Test]
        public void IsErroneousResponseGivesTrueForErrorCodeEqualTo400()
        {
            // when, then
            Assert.That(new TestResponse(mockLogger, 400, new Dictionary<string, List<string>>()).IsErroneousResponse, Is.True);
        }

        [Test]
        public void IsErroneousResponseGivesTrueForErrorCodeGreaterThan400()
        {
            // when, then
            Assert.That(new TestResponse(mockLogger, 401, new Dictionary<string, List<string>>()).IsErroneousResponse, Is.True);
        }

        [Test]
        public void IsErroneousResponseGivesFalseForErrorCodeLessThan400()
        {
            // when, then
            Assert.That(new TestResponse(mockLogger, 399, new Dictionary<string, List<string>>()).IsErroneousResponse, Is.False);
        }

        [Test]
        public void ResponseCodeIsSet()
        {
            // given
            Assert.That(new TestResponse(mockLogger, 418, new Dictionary<string, List<string>>()).ResponseCode, Is.EqualTo(418));
        }

        [Test]
        public void HeadersAreSet()
        {
            // given
            var headers = new Dictionary<string, List<string>>
            {
                { "X-Foo", new List<string> { "X-BAR" }  },
                { "X-YZ", new List<string>() }
            };

            // then
            Assert.That(new TestResponse(mockLogger, 418, headers).Headers, Is.EqualTo(headers));
        }

        [Test]
        public void GetRetryAfterReturnsDefaultValueIfResponseKeyDoesNotExist()
        {
            // given
            var target = new TestResponse(mockLogger, 429, new Dictionary<string, List<string>>());

            // when
            var obtained = target.GetRetryAfterInMilliseconds();

            // then
            Assert.That(obtained, Is.EqualTo(Response.DefaultRetryAfterInMilliseconds));
        }

        [Test]
        public void GetRetryAfterReturnsDefaultValueIfMultipleValuesWereRetrieved()
        {
            // given
            var responseHeaders = new Dictionary<string, List<string>>
            {
                { Response.ResponseKeyRetryAfter, new List<string>{ "100", "200" } }
            };
            var target = new TestResponse(mockLogger, 429, responseHeaders);

            // when
            var obtained = target.GetRetryAfterInMilliseconds();

            // then
            Assert.That(obtained, Is.EqualTo(Response.DefaultRetryAfterInMilliseconds));
        }

        [Test]
        public void GetRetryAfterReturnsDefaultValueIfValueIsNotParsableAsInteger()
        {
            // given
            var responseHeaders = new Dictionary<string, List<string>>
            {
                { Response.ResponseKeyRetryAfter, new List<string>{ "a" } }
            };
            var target = new TestResponse(mockLogger, 429, responseHeaders);

            // when
            var obtained = target.GetRetryAfterInMilliseconds();

            // then
            Assert.That(obtained, Is.EqualTo(Response.DefaultRetryAfterInMilliseconds));
        }

        [Test]
        public void GetRetryAfterReturnsParsedValue()
        {
            // given
            var responseHeaders = new Dictionary<string, List<string>>
            {
                { Response.ResponseKeyRetryAfter, new List<string>{ "1234" } }
            };
            var target = new TestResponse(mockLogger, 429, responseHeaders);

            // when
            var obtained = target.GetRetryAfterInMilliseconds();


            // then
            Assert.That(obtained, Is.EqualTo(1234L * 1000L));
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class TestResponse : Response
        {
            internal TestResponse(ILogger logger, int responseCode, Dictionary<string, List<string>> headers) : base(logger, responseCode, headers)
            {
            }
        }
    }
}
