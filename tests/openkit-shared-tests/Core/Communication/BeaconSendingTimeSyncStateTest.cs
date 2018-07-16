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
        public void InitCompleteIsCalledForInitialRequest_OnSuccess()
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
        public void InitCompleteIsCalledForInitialRequest_OnFailure()
        {
            // given
            httpClient.SendTimeSyncRequest().Returns((TimeSyncResponse)null); // alwasys return invalid response

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
            httpClient.SendTimeSyncRequest().Returns((TimeSyncResponse)null); // alwasys return invalid response

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
            httpClient.SendTimeSyncRequest().Returns(
                    // request 1 fails 2 times
                    x => null,
                    x => null,
                    x => CreateValidTimeResponse(currentTime, 10),
                    // request 2 fails 1 time
                    x => null,
                    x => CreateValidTimeResponse(currentTime, 10),
                    // request 3 fails 4 times
                    x => null,
                    x => null,
                    x => null,
                    x => null,
                    x => CreateValidTimeResponse(currentTime, 10),
                    // request 4 fails 0 times
                    x => CreateValidTimeResponse(currentTime, 10),
                    // request 5 fails 1 time
                    x => null,
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
            httpClient.SendTimeSyncRequest().Returns(
                    // request 1 fails 2 times
                    x => null,
                    x => null,
                    x => null,
                    x => null,
                    x => null,
                    x => CreateValidTimeResponse(currentTime, 10),
                    // request 2 fails 1 time
                    x => null,
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

        private static TimeSyncResponse CreateValidTimeResponse(long receiveTime, long delta)
        {
            var responseFormatString = TimeSyncResponse.RESPONSE_KEY_REQUEST_RECEIVE_TIME + "={0}&" + TimeSyncResponse.RESPONSE_KEY_RESPONSE_SEND_TIME + "={1}";

            return new TimeSyncResponse(string.Format(responseFormatString, receiveTime, receiveTime + delta), 200, new Dictionary<string, System.Collections.Generic.List<string>>());
        }
    }
}
