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
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Util;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Providers
{
    public class DefaultHttpClientProviderTest
    {
        private ILogger mockLogger;
        private IInterruptibleThreadSuspender mockThreadSuspender;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
            mockThreadSuspender = Substitute.For<IInterruptibleThreadSuspender>();
        }

        [Test]
        public void CreateClientReturnsNewHttpClient()
        {
            // given
            var httpClientConfig = Substitute.For<IHttpClientConfiguration>();
            httpClientConfig.BaseUrl.Returns("https://localhost:9999/1");
            httpClientConfig.ApplicationId.Returns("some cryptic appId");

            var target = new DefaultHttpClientProvider(mockLogger, mockThreadSuspender);

            // when
            var obtained = target.CreateClient(httpClientConfig);

            // then
            Assert.That(obtained, Is.Not.Null);
        }
    }
}