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
using Dynatrace.OpenKit.Protocol;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Communication
{
    public class BeaconSendingCaptureOffStateTest
    {
        private long currentTime = 0;
        private long lastStatusCheckTime = -1;

        private ILogger logger;
        private IHttpClient httpClient;
        private IBeaconSendingContext context;


        [SetUp]
        public void Setup()
        {
            currentTime = 0;
            lastStatusCheckTime = -1;

            logger = Substitute.For<ILogger>();
            httpClient = Substitute.For<IHttpClient>();
            context = Substitute.For<IBeaconSendingContext>();
            context.GetHttpClient().Returns(httpClient);

            // default return success
            httpClient.SendStatusRequest().Returns(new StatusResponse(logger, string.Empty, 200, new Dictionary<string, List<string>>()));

            // current time getter
            context.CurrentTimestamp.Returns(x => ++currentTime);

            // last time sync getter + setter
            context.LastStatusCheckTime = Arg.Do<long>(x => lastStatusCheckTime = x);
            context.LastStatusCheckTime = lastStatusCheckTime; // init with -1
        }

        [Test]
        public void StateIsNotTerminal()
        {
            // when
            var target = new BeaconSendingCaptureOffState();

            // then
            Assert.That(target.IsTerminalState, Is.False);
        }

        [Test]
        public void ShutdownStateIsFlushState()
        {
            // when
            var target = new BeaconSendingCaptureOffState();

            // then
            Assert.That(target.ShutdownState, Is.InstanceOf(typeof(BeaconSendingFlushSessionsState)));
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
        public void TransitionToCaptureOnStateIsPerformed()
        {
            // given
            context.IsCaptureOn.Returns(true);
            httpClient.SendStatusRequest().Returns(new StatusResponse(logger, string.Empty, 200, new Dictionary<string, List<string>>()));

            // when
            var target = new BeaconSendingCaptureOffState();
            target.Execute(context);

            // then
            context.Received(1).NextState = Arg.Any<BeaconSendingCaptureOnState>();
        }

        [Test]
        public void TransitionToFlushStateIsPerformedOnShutdown()
        {
            // given
            context.IsShutdownRequested.Returns(true);

            // when
            var target = new BeaconSendingCaptureOffState();
            target.Execute(context);

            // then
            context.Received(1).NextState = Arg.Any<BeaconSendingFlushSessionsState>();
        }

        [Test]
        public void StatusRequestIsRetried()
        {
            // given
            httpClient.SendStatusRequest().Returns((StatusResponse)null); // always return null

            // when
            var target = new BeaconSendingCaptureOffState();
            target.Execute(context);

            // then
            httpClient.Received(6).SendStatusRequest();
        }

        [Test]
        public void SleepIsCalledOnEntry()
        {
            // given
            context.CurrentTimestamp.Returns(0);
            context.LastStatusCheckTime.Returns(0);
            httpClient.SendStatusRequest().Returns(new StatusResponse(logger, string.Empty, 200, new Dictionary<string, List<string>>()));

            // when
            var target = new BeaconSendingCaptureOffState();
            target.Execute(context);

            // then
            context.Received(1).Sleep(BeaconSendingCaptureOffState.STATUS_CHECK_INTERVAL);
        }

        [Test]
        public void SleepIsNotCalledIfShutdownIsRequested()
        {
            // given
            context.IsShutdownRequested.Returns(true);
            context.CurrentTimestamp.Returns(0);
            context.LastStatusCheckTime.Returns(0);
            httpClient.SendStatusRequest().Returns(new StatusResponse(logger, string.Empty, 200, new Dictionary<string, List<string>>()));

            // when
            var target = new BeaconSendingCaptureOffState();
            target.Execute(context);

            // then
            context.DidNotReceive().Sleep(Arg.Any<int>());
        }

        [Test]
        public void ABeaconSendingCaptureOffStateWaitsForGivenTime()
        {
            //given
            var target = new BeaconSendingCaptureOffState(12345);
            context.IsCaptureOn.Returns(true);

            // when calling execute
            target.Execute(context);

            // then verify the custom amount of time was waited
            context.Received(1).Sleep(12345);
        }

        [Test]
        public void ABeaconSendingCaptureOffStateStaysInOffStateWhenServerRespondsWithTooManyRequests()
        {
            //given
            var target = new BeaconSendingCaptureOffState(12345);
            var responseHeaders = new Dictionary<string, List<string>>
            {
                { Response.ResponseKeyRetryAfter, new List<string> { "1234" } }
            };
            var tooManyRequestsResponse = new StatusResponse(logger, string.Empty, Response.HttpTooManyRequests, responseHeaders);
            httpClient.SendStatusRequest().Returns(tooManyRequestsResponse);
            context.IsCaptureOn.Returns(false);

            AbstractBeaconSendingState capturedState = null;
            context.NextState = Arg.Do<AbstractBeaconSendingState>(x => capturedState = x);

            // when calling execute
            target.Execute(context);

            // then verify next state
            Assert.That(capturedState, Is.Not.Null);
            Assert.That(capturedState, Is.InstanceOf<BeaconSendingCaptureOffState>());
            Assert.That(((BeaconSendingCaptureOffState)capturedState).sleepTimeInMilliseconds, Is.EqualTo(1234 * 1000));
        }
    }
}
