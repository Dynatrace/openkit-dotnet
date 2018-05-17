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
    [TestFixture]
    public class BeaconSendingInitStateTest
    {
        private IHTTPClient httpClient;
        private IBeaconSendingContext context;

        [SetUp]
        public void Setup()
        {
            httpClient = Substitute.For<IHTTPClient>();
            context = Substitute.For<IBeaconSendingContext>();
            context.GetHTTPClient().Returns(httpClient);
        }

        [Test]
        public void StateIsNotTerminal()
        {
            // when
            var target = new BeaconSendingInitState();

            // then
            Assert.That(target.IsTerminalState, Is.False);
        }

        [Test]
        public void ShutdownStateIsTerminalState()
        {
            // when
            var target = new BeaconSendingInitState();

            // then
            Assert.That(target.ShutdownState, Is.InstanceOf(typeof(BeaconSendingTerminalState)));
        }

        [Test]
        public void InitCompleteIsCalledOnInterrupt()
        {
            // when 
            var target = new BeaconSendingInitState();
            target.OnInterrupted(context);

            // then
            context.Received(1).InitCompleted(false);
        }

        [Test]
        public void LastOpenSessionBeaconSendTimeIsSetInExecute()
        {
            // given
            context.IsShutdownRequested.Returns(true); // shutdown is requested
            context.CurrentTimestamp.Returns(123456L);
            httpClient.SendStatusRequest().Returns(new StatusResponse(string.Empty, 200)); // return valid status response (!= null)

            // when
            var target = new BeaconSendingInitState();
            target.Execute(context);

            // then
            context.Received(1).LastOpenSessionBeaconSendTime = 123456L;
        }

        [Test]
        public void LastStatusCheckTimeIsSetInExecute()
        {
            // given
            context.IsShutdownRequested.Returns(true); // shutdown is requested
            context.CurrentTimestamp.Returns(654321L);
            httpClient.SendStatusRequest().Returns(new StatusResponse(string.Empty, 200)); // return valid status response (!= null)

            // when
            var target = new BeaconSendingInitState();
            target.Execute(context);

            // then
            context.Received(1).LastStatusCheckTime = 654321L;
        }

        [Test]
        public void InitCompleteIsCalledIfShutdownIsRequested()
        {
            // given
            context.IsShutdownRequested.Returns(true); // shutdown is requested
            httpClient.SendStatusRequest().Returns(new StatusResponse(string.Empty, 200)); // return valid status response (!= null)

            // when
            var target = new BeaconSendingInitState();
            target.Execute(context);

            // then
            context.Received(1).InitCompleted(false);
            context.Received(1).NextState = Arg.Any<BeaconSendingTerminalState>();
        }

        [Test]
        public void ReinitializeSleepsBeforeSendingStatusRequests()
        {
            // given
            var count = 0;
            httpClient.SendStatusRequest().Returns((StatusResponse)null); // always return null
            context.IsShutdownRequested.Returns(_ => { return count++ > 40; });

            var target = new BeaconSendingInitState();

            // when
            target.Execute(context);

            // then
            // verify sleeps - first total number and then correct order
            context.ReceivedWithAnyArgs(41).Sleep(0);

            Received.InOrder(() =>
            {
                // from first round
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 2);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 4);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 8);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 16);
                // delay between first and second attempt
                context.Sleep(BeaconSendingInitState.REINIT_DELAY_MILLISECONDS[0]);
                // and again the sequence
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 2);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 4);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 8);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 16);
                // delay between second and third attempt
                context.Sleep(BeaconSendingInitState.REINIT_DELAY_MILLISECONDS[1]);
                // and again the sequence
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 2);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 4);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 8);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 16);
                // delay between third and fourth attempt
                context.Sleep(BeaconSendingInitState.REINIT_DELAY_MILLISECONDS[2]);
                // and again the sequence
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 2);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 4);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 8);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 16);
                // delay between fourth and fifth attempt
                context.Sleep(BeaconSendingInitState.REINIT_DELAY_MILLISECONDS[3]);
                // and again the sequence
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 2);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 4);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 8);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 16);
                // delay between fifth and sixth attempt
                context.Sleep(BeaconSendingInitState.REINIT_DELAY_MILLISECONDS[4]);
                // and again the sequence
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 2);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 4);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 8);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 16);
                // delay between sixth and seventh attempt
                context.Sleep(BeaconSendingInitState.REINIT_DELAY_MILLISECONDS[4]);
                // and again the sequence
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 2);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 4);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 8);
                context.Sleep(BeaconSendingInitState.INITIAL_RETRY_SLEEP_TIME_MILLISECONDS * 16);
            });
        }

        [Test]
        public void StatusRequestIsRetried()
        {
            // given
            httpClient.SendStatusRequest().Returns((StatusResponse)null); // always return null
            context.IsShutdownRequested.Returns(false, false, false, false, false, true);

            // when
            var target = new BeaconSendingInitState();
            target.Execute(context);

            // then
            httpClient.Received(6).SendStatusRequest();
        }

        [Test]
        public void TransitionToTimeSyncIsPerformedOnSuccess()
        {
            // given
            httpClient.SendStatusRequest().Returns(new StatusResponse(string.Empty, 200)); // return valid status response (!= null)

            // when
            var target = new BeaconSendingInitState();
            target.Execute(context);

            // then
            context.Received(1).NextState = Arg.Any<BeaconSendingTimeSyncState>();
        }
    }
}
