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
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core.Communication
{
    class BeaconSendingRequestUtilTest
    {
        private IBeaconSendingContext context;
        private IHTTPClient httpClient;
        private StatusResponse statusResponse;

        [SetUp]
        public void Setup()
        {
            httpClient = Substitute.For<IHTTPClient>();
            context = Substitute.For<IBeaconSendingContext>();
            context.GetHTTPClient().Returns(httpClient);
            statusResponse = new StatusResponse(string.Empty, 200, new Dictionary<string, List<string>>());
        }

        [Test]
        public void SendStatusRequestIsAbortedWhenShutDownIsRequested()
        {
            // given
            context.IsShutdownRequested.Returns(true);
            httpClient.SendStatusRequest().Returns((StatusResponse)null);

            // when
            var obtained = BeaconSendingRequestUtil.SendStatusRequest(context, 5, 1000);

            // then
            Assert.That(obtained, Is.Null);

            context.Received(1).GetHTTPClient();
            context.ReceivedWithAnyArgs(0).Sleep(0);

            httpClient.Received(1).SendStatusRequest();
        }

        [Test]
        public void SendStatusRequestIsAbortedIfNumberOfRetriesIsExceeded()
        {
            // given
            context.IsShutdownRequested.Returns(false);
            httpClient.SendStatusRequest().Returns((StatusResponse)null);

            // when
            var obtained = BeaconSendingRequestUtil.SendStatusRequest(context, 3, 1000);

            // then
            Assert.That(obtained, Is.Null);

            context.Received(4).GetHTTPClient();
            context.ReceivedWithAnyArgs(3).Sleep(0);

            httpClient.Received(4).SendStatusRequest();
        }

        [Test]
        public void SendStatusRequestWhenHttpClientReturnsANonNullResponse()
        {
            // given
            context.IsShutdownRequested.Returns(false);
            httpClient.SendStatusRequest().Returns(statusResponse);

            // when
            var obtained = BeaconSendingRequestUtil.SendStatusRequest(context, 5, 1000);

            // then
            Assert.That(obtained, Is.SameAs(statusResponse));

            context.Received(1).GetHTTPClient();
            context.ReceivedWithAnyArgs(0).Sleep(0);

            httpClient.Received(1).SendStatusRequest();
        }

        [Test]
        public void SleepTimeIsDoubledBetweenConsecutiveRetries()
        {
            // given
            context.IsShutdownRequested.Returns(false);
            httpClient.SendStatusRequest().Returns((StatusResponse)null);

            // when
            var obtained = BeaconSendingRequestUtil.SendStatusRequest(context, 5, 1000);

            // then
            Assert.That(obtained, Is.Null);

            context.Received(6).GetHTTPClient();
            httpClient.Received(6).SendStatusRequest();

            Received.InOrder(() =>
            {
                context.Sleep(1000);
                context.Sleep(2 * 1000);
                context.Sleep(4 * 1000);
                context.Sleep(8 * 1000);
                context.Sleep(16 * 1000);
            });

        }
    }
}
