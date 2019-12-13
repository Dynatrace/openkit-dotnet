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
    [TestFixture]
    public class BeaconSendingInitStateTest
    {
        private IHttpClient mockHttpClient;
        private IBeaconSendingContext mockContext;
        private IStatusResponse mockResponse;
        private IResponseAttributes mockAttributes;

        [SetUp]
        public void Setup()
        {
            mockAttributes = Substitute.For<IResponseAttributes>();
            mockResponse = Substitute.For<IStatusResponse>();
            mockResponse.ResponseCode.Returns(StatusResponse.HttpOk);
            mockResponse.IsErroneousResponse.Returns(false);
            mockResponse.ResponseAttributes.Returns(mockAttributes);

            mockHttpClient = Substitute.For<IHttpClient>();
            mockHttpClient.SendStatusRequest().Returns(mockResponse);

            mockContext = Substitute.For<IBeaconSendingContext>();
            mockContext.GetHttpClient().Returns(mockHttpClient);
        }

        [Test]
        public void InitStateIsNotATerminalState()
        {
            // given
            var target = new BeaconSendingInitState();

            // then
            Assert.That(target.IsTerminalState, Is.False);
        }

        [Test]
        public void ToStringReturnsTheStateName()
        {
            // given
            var target = new BeaconSendingInitState();

            // then
            Assert.That(target.ToString(), Is.EqualTo("Initial"));
        }

        [Test]
        public void ShutdownStateGivesABeaconSendingTerminalStateInstance()
        {
            // given
            var target = new BeaconSendingInitState();

            // then
            Assert.That(target.ShutdownState, Is.Not.Null.And.InstanceOf(typeof(BeaconSendingTerminalState)));
        }

        [Test]
        public void ShutdownStateAlwaysCreatesANewInstance()
        {
            // given
            var target = new BeaconSendingInitState();

            // when
            var obtainedOne = target.ShutdownState;
            var obtainedTwo = target.ShutdownState;

            // then
            Assert.That(obtainedOne, Is.Not.Null);
            Assert.That(obtainedTwo, Is.Not.Null);
            Assert.That(obtainedOne, Is.Not.SameAs(obtainedTwo));
        }


        [Test]
        public void OnInterruptedCallsInitCompletedInContext()
        {
            // given
            var target = new BeaconSendingInitState();

            // when
            target.OnInterrupted(mockContext);

            // then
            mockContext.Received(1).InitCompleted(false);
        }

        [Test]
        public void ExecuteSetsLastOpenSessionBeaconSendTime()
        {
            // given
            const long timestamp = 123456;
            mockContext.IsShutdownRequested.Returns(true); // shutdown is requested
            mockContext.CurrentTimestamp.Returns(timestamp);
            var target = new BeaconSendingInitState();

            // when
            target.Execute(mockContext);

            // then
            mockContext.Received(1).LastOpenSessionBeaconSendTime = timestamp;
        }

        [Test]
        public void ExecuteSetsLastStatusCheckTime()
        {
            // given
            const long timestamp = 654321;
            mockContext.IsShutdownRequested.Returns(true); // shutdown is requested
            mockContext.CurrentTimestamp.Returns(timestamp);
            var target = new BeaconSendingInitState();

            // when
            target.Execute(mockContext);

            // then
            mockContext.Received(1).LastStatusCheckTime = timestamp;
        }

        [Test]
        public void InitIsTerminatedIfShutdownRequestedWithValidResponse()
        {
            // given
            mockContext.IsShutdownRequested.Returns(true); // shutdown is requested
            var target = new BeaconSendingInitState();

            // when
            target.Execute(mockContext);

            // then
            mockContext.Received(2).InitCompleted(false);
            mockContext.Received(1).NextState = Arg.Any<BeaconSendingTerminalState>();
        }

        [Test]
        public void ReinitializeSleepsBeforeSendingStatusRequestsAgain()
        {
            // given
            var count = 0;
            var erroneousResponse = Substitute.For<IStatusResponse>();
            erroneousResponse.ResponseCode.Returns(StatusResponse.HttpBadRequest);
            erroneousResponse.IsErroneousResponse.Returns(true);
            mockHttpClient.SendStatusRequest().Returns(erroneousResponse); // always return erroneous response
            mockContext.IsShutdownRequested.Returns(_ => count++ > 40);

            var target = new BeaconSendingInitState();

            // when
            target.Execute(mockContext);

            // then
            // verify sleeps - first total number and then correct order
            mockContext.ReceivedWithAnyArgs(41).Sleep(0);

            Received.InOrder(() =>
            {
                // from first round
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 2);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 4);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 8);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 16);
                // delay between first and second attempt
                mockContext.Sleep(BeaconSendingInitState.ReInitDelayMilliseconds[0]);
                // and again the sequence
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 2);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 4);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 8);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 16);
                // delay between second and third attempt
                mockContext.Sleep(BeaconSendingInitState.ReInitDelayMilliseconds[1]);
                // and again the sequence
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 2);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 4);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 8);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 16);
                // delay between third and fourth attempt
                mockContext.Sleep(BeaconSendingInitState.ReInitDelayMilliseconds[2]);
                // and again the sequence
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 2);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 4);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 8);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 16);
                // delay between fourth and fifth attempt
                mockContext.Sleep(BeaconSendingInitState.ReInitDelayMilliseconds[3]);
                // and again the sequence
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 2);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 4);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 8);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 16);
                // delay between fifth and sixth attempt
                mockContext.Sleep(BeaconSendingInitState.ReInitDelayMilliseconds[4]);
                // and again the sequence
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 2);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 4);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 8);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 16);
                // delay between sixth and seventh attempt
                mockContext.Sleep(BeaconSendingInitState.ReInitDelayMilliseconds[4]);
                // and again the sequence
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 2);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 4);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 8);
                mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 16);
            });
        }

        [Test]
        public void SleepTimeIsDoubledBetweenStatusRequestRetries()
        {
            //given
            var errorResponse = Substitute.For<IStatusResponse>();
            errorResponse.ResponseCode.Returns(StatusResponse.HttpBadRequest);
            errorResponse.IsErroneousResponse.Returns(true);

            mockHttpClient.SendStatusRequest().Returns(errorResponse);
            mockContext.IsShutdownRequested.Returns(false, false, false, false, false, true);

            var target = new BeaconSendingInitState();

            // when
            target.Execute(mockContext);

            mockContext.Received(5).Sleep(Arg.Any<int>());
            Received.InOrder(() =>
                {
                    mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds);
                    mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 2);
                    mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 4);
                    mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 8);
                    mockContext.Sleep(BeaconSendingInitState.InitialRetrySleepTimeMilliseconds * 16);
                }
            );
        }

        [Test]
        public void SendStatusRequestIsRetried()
        {
            // given
            var errorResponse = Substitute.For<IStatusResponse>();
            errorResponse.ResponseCode.Returns(StatusResponse.HttpBadRequest);
            errorResponse.IsErroneousResponse.Returns(true);
            mockHttpClient.SendStatusRequest().Returns(errorResponse); // always return erroneous response
            mockContext.IsShutdownRequested.Returns(false, false, false, false, false, true);

            var target = new BeaconSendingInitState();

            // when
            target.Execute(mockContext);

            // then
            mockHttpClient.Received(6).SendStatusRequest();
        }

        [Test]
        public void InitialStatusRequestGivesUpWhenShutdownRequestIsSetDuringExecution()
        {
            // given
            var errorResponse = Substitute.For<IStatusResponse>();
            errorResponse.ResponseCode.Returns(StatusResponse.HttpBadRequest);
            errorResponse.IsErroneousResponse.Returns(true);
            mockHttpClient.SendStatusRequest().Returns(errorResponse);
            mockContext.IsShutdownRequested.Returns(false, false, true);

            var target = new BeaconSendingInitState();

            // when
            target.Execute(mockContext);

            // then
            mockContext.Received(2).InitCompleted(false); // init completed with error
            mockContext.Received(1).NextState = Arg.Any<BeaconSendingTerminalState>(); // transition to terminal state

            mockContext.Received(3).GetHttpClient();
            mockHttpClient.Received(3).SendStatusRequest();

            mockContext.Received(2).Sleep(Arg.Any<int>());
        }

        [Test]
        public void ASuccessfulStatusResponseSetsInitCompletedToTrueForCaptureOn()
        {
            // given
            mockAttributes.IsCapture.Returns(true);
            var target = new BeaconSendingInitState();

            // when
            target.Execute(mockContext);

            // then
            mockContext.Received(1).InitCompleted(true);
        }

        [Test]
        public void ASuccessfulStatusResponseSetsInitCompletedToTrueForCaptureOff()
        {
            // given
            mockAttributes.IsCapture.Returns(false);
            var target = new BeaconSendingInitState();

            // when
            target.Execute(mockContext);

            // then
            mockContext.Received(1).InitCompleted(true);
        }

        [Test]
        public void ASuccessfulStatusResponsePerformsStateTransitionToCaptureOnIfCapturingIsEnabled()
        {
            // given
            mockContext.IsCaptureOn.Returns(true);
            var target = new BeaconSendingInitState();

            // when
            target.Execute(mockContext);

            // then
            mockContext.Received(1).HandleStatusResponse(mockResponse);
            mockContext.Received(1).NextState = Arg.Any<BeaconSendingCaptureOnState>();
        }

        [Test]
        public void ASuccessfulStatusResponsePerformsStateTransitionToCaptureOffIfCapturingIsDisabled()
        {
            // given
            mockContext.IsCaptureOn.Returns(false);
            var target = new BeaconSendingInitState();

            // when
            target.Execute(mockContext);

            // then
            mockContext.Received(1).HandleStatusResponse(mockResponse);
            mockContext.Received(1).NextState = Arg.Any<BeaconSendingCaptureOffState>();
        }

        [Test]
        public void ReceivingTooManyRequestsResponseUsesSleepTimeFromResponse()
        {
            // given
            const int retryTimeout = 1234;
            var errorResponse = Substitute.For<IStatusResponse>();
            errorResponse.ResponseCode.Returns(StatusResponse.HttpTooManyRequests);
            errorResponse.IsErroneousResponse.Returns(true);
            errorResponse.GetRetryAfterInMilliseconds().Returns(retryTimeout);
            mockHttpClient.SendStatusRequest().Returns(errorResponse);
            mockContext.IsShutdownRequested.Returns(false, true);

            var target = new BeaconSendingInitState();

            // when
            target.Execute(mockContext);

            // verify sleep was performed accordingly
            mockContext.Received(1).Sleep(retryTimeout);
        }

        [Test]
        public void ReceivingTooManyRequestsResponseDisablesCapturing()
        {
            // given
            const int retryTimeout = 1234;
            var errorResponse = Substitute.For<IStatusResponse>();
            errorResponse.ResponseCode.Returns(StatusResponse.HttpTooManyRequests);
            errorResponse.IsErroneousResponse.Returns(true);
            errorResponse.GetRetryAfterInMilliseconds().Returns(retryTimeout);
            mockHttpClient.SendStatusRequest().Returns(errorResponse);
            mockContext.IsShutdownRequested.Returns(false, true);

            var target = new BeaconSendingInitState();

            // when
            target.Execute(mockContext);

            // verify sleep was performed accordingly
            mockContext.Received(1).DisableCaptureAndClear();
        }
    }
}
