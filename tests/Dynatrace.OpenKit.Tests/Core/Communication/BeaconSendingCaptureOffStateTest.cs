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
    public class BeaconSendingCaptureOffStateTest
    {
        private IHttpClient mockHttpClient;
        private IBeaconSendingContext mockContext;

        [SetUp]
        public void Setup()
        {
            var mockResponse = Substitute.For<IStatusResponse>();
            mockResponse.ResponseCode.Returns(StatusResponse.HttpOk);
            mockResponse.IsErroneousResponse.Returns(false);

            mockHttpClient = Substitute.For<IHttpClient>();
            mockHttpClient.SendStatusRequest(Arg.Any<IAdditionalQueryParameters>()).Returns(mockResponse);

            mockContext = Substitute.For<IBeaconSendingContext>();
            mockContext.GetHttpClient().Returns(mockHttpClient);
        }

        [Test]
        public void ABeaconSendingCaptureOffStateIsNotATerminalState()
        {
            // when
            var target = new BeaconSendingCaptureOffState();

            // then
            Assert.That(target.IsTerminalState, Is.False);
        }

        [Test]
        public void ToStringReturnsTheStateName()
        {
            // given
            var target = new BeaconSendingCaptureOffState();

            // then
            Assert.That(target.ToString(), Is.EqualTo("CaptureOff"));
        }

        [Test]
        public void ABeaconSendingCaptureOffStateHasTerminalStateBeaconSendingFlushSessions()
        {
            // when
            var target = new BeaconSendingCaptureOffState();

            // then
            Assert.That(target.ShutdownState, Is.InstanceOf(typeof(BeaconSendingFlushSessionsState)));
        }


        [Test]
        public void ABeaconSendingCaptureOffStateTransitionsToCaptureOnStateWhenCapturingActive()
        {
            // given
            var target = new BeaconSendingCaptureOffState();
            mockContext.IsCaptureOn.Returns(true);

            // when
            target.Execute(mockContext);

            // then
            mockContext.Received(1).DisableCaptureAndClear();
            mockContext.Received(1).LastStatusCheckTime = Arg.Any<long>();
            mockContext.Received(1).NextState = Arg.Any<BeaconSendingCaptureOnState>();
        }

        [Test]
        public void ABeaconSendingCaptureOffStateWaitsForGivenTime()
        {
            //given
            const int sleepTime = 12345;
            var target = new BeaconSendingCaptureOffState(sleepTime);
            mockContext.IsCaptureOn.Returns(true);

            // when
            target.Execute(mockContext);

            // then
            mockContext.Received(1).Sleep(sleepTime);
        }

        [Test]
        public void ABeaconSendingCaptureOffStateStaysInOffStateWhenServerRespondsWithTooManyRequests()
        {
            //given
            var target = new BeaconSendingCaptureOffState(12345);

            const int retryTimeout = 1234;
            var tooManyRequestsResponse = Substitute.For<IStatusResponse>();
            tooManyRequestsResponse.ResponseCode.Returns(StatusResponse.HttpTooManyRequests);
            tooManyRequestsResponse.IsErroneousResponse.Returns(true);
            tooManyRequestsResponse.GetRetryAfterInMilliseconds().Returns(retryTimeout);

            mockHttpClient.SendStatusRequest(Arg.Any<IAdditionalQueryParameters>()).Returns(tooManyRequestsResponse);
            mockContext.IsCaptureOn.Returns(false);

            AbstractBeaconSendingState capturedState = null;
            mockContext.NextState = Arg.Do<AbstractBeaconSendingState>(x => capturedState = x);

            // when calling execute
            target.Execute(mockContext);

            // then verify next state
            Assert.That(capturedState, Is.Not.Null);
            Assert.That(capturedState, Is.InstanceOf<BeaconSendingCaptureOffState>());
            Assert.That(((BeaconSendingCaptureOffState)capturedState).SleepTimeInMilliseconds, Is.EqualTo(retryTimeout));
        }

        [Test]
        public void TransitionToFlushStateIsPerformedOnShutdown()
        {
            // given
            mockContext.IsShutdownRequested.Returns(true);
            var target = new BeaconSendingCaptureOffState();

            // when
            target.Execute(mockContext);

            // then
            mockContext.Received(1).NextState = Arg.Any<BeaconSendingFlushSessionsState>();
        }

        [Test]
        public void StatusRequestIsRetried()
        {
            // given
            mockHttpClient.SendStatusRequest(Arg.Any<IAdditionalQueryParameters>()).Returns((StatusResponse)null); // always return null
            var target = new BeaconSendingCaptureOffState();

            // when
            target.Execute(mockContext);

            // then
            mockHttpClient.Received(6).SendStatusRequest(mockContext);
        }

        [Test]
        public void SleepIsCalledOnEntry()
        {
            // given
            var target = new BeaconSendingCaptureOffState();

            // when
            target.Execute(mockContext);

            // then
            mockContext.Received(1).Sleep(BeaconSendingCaptureOffState.StatusCheckInterval);
        }

        [Test]
        public void SleepIsNotCalledIfShutdownIsRequested()
        {
            // given
            mockContext.IsShutdownRequested.Returns(true);
            var target = new BeaconSendingCaptureOffState();

            // when
            target.Execute(mockContext);

            // then
            mockContext.DidNotReceive().Sleep(Arg.Any<int>());
        }
    }
}
