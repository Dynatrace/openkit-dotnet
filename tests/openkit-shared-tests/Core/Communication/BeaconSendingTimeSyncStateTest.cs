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
using Dynatrace.OpenKit.Protocol;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core.Communication
{
    public class BeaconSendingTimeSyncStateTest
    {
        private long currentTime = 0;
        private long lastTimeSyncTime = 0;

        private IHTTPClient httpClient;
        private IBeaconSendingContext context;


        [SetUp]
        public void Setup()
        {
            currentTime = 0;
            lastTimeSyncTime = -1;

            httpClient = Substitute.For<IHTTPClient>();
            context = Substitute.For<IBeaconSendingContext>();
            context.GetHTTPClient().Returns(httpClient);

            // return true by default
            context.IsTimeSyncSupported.Returns(true);

            // current time getter
            context.CurrentTimestamp.Returns(x => { return ++currentTime; });

            // last time sycn getter + setter
            context.LastTimeSyncTime = Arg.Do<long>(x => lastTimeSyncTime = x);
            context.LastTimeSyncTime = lastTimeSyncTime; // init with -1

            // by default return erroneous responses
            httpClient.SendTimeSyncRequest()
                .Returns(new TimeSyncResponse(Substitute.For<ILogger>(), string.Empty, Response.HttpBadRequest, new Dictionary<string, List<string>>()));
        }

        [Test]
        public void StateIsNotTerminal()
        {
            // when
            var target = new BeaconSendingTimeSyncState();

            // then
            Assert.That(target.IsTerminalState, Is.False);
        }

        [Test]
        public void ShutdownStateIsTerminalStateForInitialTimeSync()
        {
            // when
            var target = new BeaconSendingTimeSyncState(true);

            // then
            Assert.That(target.ShutdownState, Is.InstanceOf<BeaconSendingTerminalState>());
        }

        [Test]
        public void ShutdownStateIsFlushSessionStateForTimeReSync()
        {
            // when
            var target = new BeaconSendingTimeSyncState();

            // then
            Assert.That(target.ShutdownState, Is.InstanceOf<BeaconSendingFlushSessionsState>());
        }

        [Test]
        public void ToStringReturnsTheStateName()
        {
            // given
            var target = new BeaconSendingTimeSyncState();

            // then
            Assert.That(target.ToString(), Is.EqualTo("TimeSync"));
        }

        [Test]
        public void TransitionToCaptureOnIsPerformedOnSuccess()
        {
            // given
            httpClient.SendTimeSyncRequest().Returns(x => CreateValidTimeResponse(currentTime, 10)); // always return valid response
            context.IsCaptureOn.Returns(true); // captureOn is true

            // when
            var target = new BeaconSendingTimeSyncState();
            target.Execute(context);

            // then
            context.Received(1).NextState = Arg.Any<BeaconSendingCaptureOnState>();
        }

        [Test]
        public void TransitionToCaptureOffIsPerformedOnSuccess()
        {
            // given
            httpClient.SendTimeSyncRequest().Returns(x => CreateValidTimeResponse(currentTime, 10)); // alwasys return valid response
            context.IsCaptureOn.Returns(false); // captureOn is false

            // when
            var target = new BeaconSendingTimeSyncState();
            target.Execute(context);

            // then
            context.Received(1).NextState = Arg.Any<BeaconSendingCaptureOffState>();
        }

        [Test]
        public void InitCompleteIsCalledForInitialRequestOnSuccess()
        {
            // given
            httpClient.SendTimeSyncRequest().Returns(x => CreateValidTimeResponse(currentTime, 10)); // alwasys return valid response

            // when
            var target = new BeaconSendingTimeSyncState(true);
            target.Execute(context);

            // then
            context.Received(1).InitCompleted(true);
        }

        [Test]
        public void InitCompleteIsCalledForInitialRequestOnFailure()
        {
            // given
            var erroneousResponse = new TimeSyncResponse(Substitute.For<ILogger>(), string.Empty, Response.HttpBadRequest, new Dictionary<string, List<string>>());
            httpClient.SendTimeSyncRequest().Returns(erroneousResponse); // alwasys return invalid response

            // when
            var target = new BeaconSendingTimeSyncState(true);
            target.Execute(context);

            // then
            context.Received(1).InitCompleted(true);
        }

        [Test]
        public void InitCompleteIsNotCalledForNonInitialRequest()
        {
            // given
            var erroneousResponse = new TimeSyncResponse(Substitute.For<ILogger>(), string.Empty, Response.HttpBadRequest, new Dictionary<string, List<string>>());
            httpClient.SendTimeSyncRequest().Returns(erroneousResponse); // alwasys return invalid response

            // when
            var target = new BeaconSendingTimeSyncState();
            target.Execute(context);

            // then
            context.DidNotReceive().InitCompleted(Arg.Any<bool>());
        }

        [Test]
        public void TimeSyncRequetsAreRetried()
        {
            // given
            var erroneousResponse = new TimeSyncResponse(Substitute.For<ILogger>(), string.Empty, Response.HttpBadRequest, new Dictionary<string, List<string>>());
            httpClient.SendTimeSyncRequest().Returns(
                    // request 1 fails 2 times
                    x => erroneousResponse,
                    x => erroneousResponse,
                    x => CreateValidTimeResponse(currentTime, 10),
                    // request 2 fails 1 time
                    x => erroneousResponse,
                    x => CreateValidTimeResponse(currentTime, 10),
                    // request 3 fails 4 times
                    x => erroneousResponse,
                    x => erroneousResponse,
                    x => erroneousResponse,
                    x => erroneousResponse,
                    x => CreateValidTimeResponse(currentTime, 10),
                    // request 4 fails 0 times
                    x => CreateValidTimeResponse(currentTime, 10),
                    // request 5 fails 1 time
                    x => erroneousResponse,
                    x => CreateValidTimeResponse(currentTime, 10)
                );

            // when
            var target = new BeaconSendingTimeSyncState();
            target.Execute(context);

            // then
            httpClient.Received(13).SendTimeSyncRequest();
        }

        [Test]
        public void SleepTimeIsDoubledAndResetAfterSuccess()
        {
            // given
            var erroneousResponse = new TimeSyncResponse(Substitute.For<ILogger>(), string.Empty, Response.HttpBadRequest, new Dictionary<string, List<string>>());
            httpClient.SendTimeSyncRequest().Returns(
                    // request 1 fails 2 times
                    x => erroneousResponse,
                    x => erroneousResponse,
                    x => erroneousResponse,
                    x => erroneousResponse,
                    x => erroneousResponse,
                    x => CreateValidTimeResponse(currentTime, 10),
                    // request 2 fails 1 time
                    x => erroneousResponse,
                    // other requets do not fail
                    x => CreateValidTimeResponse(currentTime, 10),
                    x => CreateValidTimeResponse(currentTime, 10),
                    x => CreateValidTimeResponse(currentTime, 10),
                    x => CreateValidTimeResponse(currentTime, 10)
                );

            // when
            var target = new BeaconSendingTimeSyncState();
            target.Execute(context);

            // then
            httpClient.Received(11).SendTimeSyncRequest();
            context.Received(6).Sleep(Arg.Any<int>());
            Received.InOrder(() =>
            {
                // sleeps for first request
                context.Sleep(1000);  // start with 1 sec
                context.Sleep(2000);  // double
                context.Sleep(4000);  // double
                context.Sleep(8000);  // double
                context.Sleep(16000); // double
                // sleeps for second request
                context.Sleep(1000);  // start with 1 sec again
            });
        }

        [Test]
        public void TimeSyncRequestsAreInterruptedAfterUnsuccessfulRetries()
        {
            // given
            var target = new BeaconSendingTimeSyncState();

            // when
            target.Execute(context);

            // then
            httpClient.Received(BeaconSendingTimeSyncState.TIME_SYNC_REQUESTS + 1).SendTimeSyncRequest();
        }

        [Test]
        public void TimeSyncRequestsAreInterruptedAfterUnsuccessfulRetriesWithNullResponse()
        {
            // given
            httpClient.SendTimeSyncRequest().Returns((TimeSyncResponse)null);
            var target = new BeaconSendingTimeSyncState();

            // when
            target.Execute(context);

            // then
            httpClient.Received(BeaconSendingTimeSyncState.TIME_SYNC_REQUESTS + 1).SendTimeSyncRequest();
        }

        [Test]
        public void StateTransitionToCaptureOffIsMadeIfTooManyRequestsResponseIsReceived()
        {
            // given
            var responseHeaders = new Dictionary<string, List<string>>
            {
                { Response.ResponseKeyRetryAfter, new List<string> { "456" } }
            };
            var response = new TimeSyncResponse(Substitute.For<ILogger>(), string.Empty, Response.HttpTooManyRequests, responseHeaders);
            httpClient.SendTimeSyncRequest().Returns(response);

            context.IsTimeSyncSupported.Returns(true);
            context.IsCaptureOn.Returns(true);

            AbstractBeaconSendingState capturedState = null;
            context.NextState = Arg.Do<AbstractBeaconSendingState>(x => capturedState = x);

            var target = new BeaconSendingTimeSyncState(true);
            
            // when
            target.Execute(context);

            // then
            context.ReceivedWithAnyArgs(1).NextState = null;
            Assert.That(capturedState, Is.Not.Null);
            Assert.That(capturedState, Is.InstanceOf<BeaconSendingCaptureOffState>());
            Assert.That(((BeaconSendingCaptureOffState)capturedState).sleepTimeInMilliseconds, Is.EqualTo(456 * 1000));
        }

        [Test]
        public void SuccessfulTimeSyncInitializesTimeProvider()
        {
            // given
            httpClient.SendTimeSyncRequest().Returns(
                    // request 1 fails 2 times
                    x => CreateValidTimeResponse(6, 1),
                    x => CreateValidTimeResponse(20, 2),
                    x => CreateValidTimeResponse(40, 1),
                    x => CreateValidTimeResponse(48, 2),
                    x => CreateValidTimeResponse(60, 1)
                );

            context.CurrentTimestamp.Returns(
                    x => 2L,
                    x => 8L,
                    x => 10L,
                    x => 23L,
                    x => 32L,
                    x => 42L,
                    x => 44L,
                    x => 52L,
                    x => 54L,
                    x => 62L,
                    x => 66L
                );

            var target = new BeaconSendingTimeSyncState(true);

            // when being executed
            target.Execute(context);

            // verify init was done
            context.Received(1).InitializeTimeSync(2L, true);

            // verify number of method calls
            httpClient.Received(BeaconSendingTimeSyncState.TIME_SYNC_REQUESTS).SendTimeSyncRequest();
            var tmp = context.Received(2 * BeaconSendingTimeSyncState.TIME_SYNC_REQUESTS + 1).CurrentTimestamp;

            context.Received(1).LastTimeSyncTime = 66;
        }

        [Test]
        public void ClusterTimeOffsetCanBeNegativeAsWell()
        {
            // given
            httpClient.SendTimeSyncRequest().Returns(
                    // request 1 fails 2 times
                    x => CreateValidTimeResponse(3, 3),
                    x => CreateValidTimeResponse(12, 7),
                    x => CreateValidTimeResponse(34, 2),
                    x => CreateValidTimeResponse(45, 2),
                    x => CreateValidTimeResponse(56, 2)
                );

            context.CurrentTimestamp.Returns(
                    x => 2L,
                    x => 8L,
                    x => 10L,
                    x => 23L,
                    x => 32L,
                    x => 42L,
                    x => 44L,
                    x => 52L,
                    x => 54L,
                    x => 62L,
                    x => 66L
                );

            var target = new BeaconSendingTimeSyncState(true);

            // when being executed
            target.Execute(context);

            // verify init was done
            context.Received(1).InitializeTimeSync(-1, true);

            // verify number of method calls
            httpClient.Received(BeaconSendingTimeSyncState.TIME_SYNC_REQUESTS).SendTimeSyncRequest();
            var tmp = context.Received(2 * BeaconSendingTimeSyncState.TIME_SYNC_REQUESTS + 1).CurrentTimestamp;

            context.Received(1).LastTimeSyncTime = 66;
        }

        private static TimeSyncResponse CreateValidTimeResponse(long receiveTime, long delta)
        {
            var logger = Substitute.For<ILogger>();
            var responseContent =$"{TimeSyncResponse.RESPONSE_KEY_REQUEST_RECEIVE_TIME}={receiveTime}&{TimeSyncResponse.RESPONSE_KEY_RESPONSE_SEND_TIME}={receiveTime + delta}";

            return new TimeSyncResponse(logger, responseContent, 200, new Dictionary<string, List<string>>());
        }
    }
}
