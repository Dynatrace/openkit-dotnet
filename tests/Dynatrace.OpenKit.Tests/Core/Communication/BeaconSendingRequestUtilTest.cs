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

using Dynatrace.OpenKit.Protocol;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Communication
{
    public class BeaconSendingRequestUtilTest
    {
        private IBeaconSendingContext mockContext;
        private IHttpClient mockHttpClient;
        private IStatusResponse mockResponse;

        [SetUp]
        public void Setup()
        {
            mockResponse = Substitute.For<IStatusResponse>();
            mockResponse.ResponseCode.Returns(StatusResponse.HttpOk);
            mockResponse.IsErroneousResponse.Returns(false);

            mockHttpClient = Substitute.For<IHttpClient>();
            mockHttpClient.SendStatusRequest(Arg.Any<IAdditionalQueryParameters>()).Returns(mockResponse);

            mockContext = Substitute.For<IBeaconSendingContext>();
            mockContext.GetHttpClient().Returns(mockHttpClient);
        }

        [Test]
        public void SendStatusRequestIsAbortedWhenShutDownIsRequested()
        {
            // given
            mockResponse.ResponseCode.Returns(StatusResponse.HttpBadRequest);
            mockResponse.IsErroneousResponse.Returns(true);
            mockContext.IsShutdownRequested.Returns(true);

            // when
            var obtained = BeaconSendingRequestUtil.SendStatusRequest(mockContext, 5, 1000);

            // then
            Assert.That(obtained, Is.SameAs(mockResponse));

            _ = mockContext.Received(1).IsShutdownRequested;
            mockContext.Received(1).GetHttpClient();
            mockContext.ReceivedWithAnyArgs(0).Sleep(0);

            mockHttpClient.Received(1).SendStatusRequest(mockContext);
        }

        [Test]
        public void SendStatusRequestIsAbortedIfNumberOfRetriesIsExceeded()
        {
            // given
            mockResponse.ResponseCode.Returns(StatusResponse.HttpBadRequest);
            mockResponse.IsErroneousResponse.Returns(true);
            mockContext.IsShutdownRequested.Returns(false);

            // when
            var obtained = BeaconSendingRequestUtil.SendStatusRequest(mockContext, 3, 1000);

            // then
            Assert.That(obtained, Is.SameAs(mockResponse));

            mockContext.Received(4).GetHttpClient();
            mockContext.ReceivedWithAnyArgs(3).Sleep(0);

            mockHttpClient.Received(4).SendStatusRequest(mockContext);
        }

        [Test]
        public void SendStatusRequestIsDoneWhenHttpClientReturnsASuccessfulResponse()
        {
            // given
            mockContext.IsShutdownRequested.Returns(false);

            // when
            var obtained = BeaconSendingRequestUtil.SendStatusRequest(mockContext, 5, 1000);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.SameAs(mockResponse));

            mockContext.Received(1).GetHttpClient();
            mockContext.ReceivedWithAnyArgs(0).Sleep(0);

            mockHttpClient.Received(1).SendStatusRequest(mockContext);
        }

        [Test]
        public void SleepTimeIsDoubledBetweenConsecutiveRetries()
        {
            // given
            mockResponse.ResponseCode.Returns(StatusResponse.HttpBadRequest);
            mockResponse.IsErroneousResponse.Returns(true);
            mockContext.IsShutdownRequested.Returns(false);

            // when
            var obtained = BeaconSendingRequestUtil.SendStatusRequest(mockContext, 5, 1000);

            // then
            Assert.That(obtained, Is.SameAs(mockResponse));
            mockContext.Received(6).GetHttpClient();
            mockHttpClient.Received(6).SendStatusRequest(mockContext);

            Received.InOrder(() =>
            {
                mockContext.Sleep(1000);
                mockContext.Sleep(2000);
                mockContext.Sleep(4000);
                mockContext.Sleep(8000);
                mockContext.Sleep(16000);
            });
        }

        [Test]
        public void SendStatusRequestHandlesNullResponsesSameAsErroneousResponses()
        {
            // given
            mockContext.IsShutdownRequested.Returns(false);
            mockHttpClient.SendStatusRequest(mockContext).Returns((StatusResponse)null);

            // when
            var obtained = BeaconSendingRequestUtil.SendStatusRequest(mockContext, 3, 1000);

            // then
            Assert.That(obtained, Is.Null);

            mockContext.Received(4).GetHttpClient();
            mockContext.ReceivedWithAnyArgs(3).Sleep(0);
            mockHttpClient.Received(4).SendStatusRequest(mockContext);
        }

        [Test]
        public void SendStatusRequestReturnsTooManyRequestsResponseImmediately()
        {
            // given
            mockResponse.ResponseCode.Returns(StatusResponse.HttpTooManyRequests);
            mockResponse.IsErroneousResponse.Returns(true);
            mockContext.IsShutdownRequested.Returns(false);

            // when
            var obtained = BeaconSendingRequestUtil.SendStatusRequest(mockContext, 3, 1000);

            // then
            Assert.That(obtained, Is.SameAs(mockResponse));

            mockContext.Received(1).GetHttpClient();
            mockContext.DidNotReceiveWithAnyArgs().Sleep(0);
            mockHttpClient.Received(1).SendStatusRequest(mockContext);
        }
    }
}
