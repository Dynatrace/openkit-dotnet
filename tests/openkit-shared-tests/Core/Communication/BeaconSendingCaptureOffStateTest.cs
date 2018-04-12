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

namespace Dynatrace.OpenKit.Core.Communication
{
    public class BeaconSendingCaptureOffStateTest
    {
        private long currentTime = 0;
        private long lastStatusCheckTime = -1;

        private IHTTPClient httpClient;
        private IBeaconSendingContext context;


        [SetUp]
        public void Setup()
        {
            currentTime = 0;
            lastStatusCheckTime = -1;

            httpClient = Substitute.For<IHTTPClient>();
            context = Substitute.For<IBeaconSendingContext>();
            context.GetHTTPClient().Returns(httpClient);

            // return true by default
            context.IsTimeSyncSupported.Returns(true);

            // current time getter
            context.CurrentTimestamp.Returns(x => { return ++currentTime; });

            // last time sycn getter + setter
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
        public void TransitionToCaptureOnStateIsPerformed()
        {
            // given
            context.IsCaptureOn.Returns(true);
            context.IsTimeSyncSupported.Returns(true);
            context.IsTimeSynced.Returns(true);
            httpClient.SendStatusRequest().Returns(new StatusResponse(string.Empty, 200));

            // when
            var target = new BeaconSendingCaptureOffState();
            target.Execute(context);

            // then
            context.Received(1).CurrentState = Arg.Any<BeaconSendingCaptureOnState>();
        }

        [Test]
        public void TransitionToTimeSyncIsPerformedIfNotDoneYet()
        {
            // given
            context.IsCaptureOn.Returns(true);
            context.IsTimeSyncSupported.Returns(true);
            httpClient.SendStatusRequest().Returns(new StatusResponse(string.Empty, 200));

            var target = new BeaconSendingCaptureOffState();

            // when
            target.Execute(context);

            // then
            context.Received(1).CurrentState = Arg.Any<BeaconSendingTimeSyncState>();
            context.Received(1).CurrentState = Arg.Is<BeaconSendingTimeSyncState>(arg => arg.IsInitialTimeSync == true);
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
            context.Received(1).CurrentState = Arg.Any<BeaconSendingFlushSessionsState>();
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
            httpClient.SendStatusRequest().Returns(new StatusResponse(string.Empty, 200));

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
            httpClient.SendStatusRequest().Returns(new StatusResponse(string.Empty, 200));

            // when
            var target = new BeaconSendingCaptureOffState();
            target.Execute(context);

            // then
            context.DidNotReceive().Sleep(Arg.Any<int>());
        }
    }
}
