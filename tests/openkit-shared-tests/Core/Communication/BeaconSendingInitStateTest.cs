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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Protocol;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core.Communication
{
    [TestFixture]
    public class BeaconSendingInitStateTest
    {
        private ILogger logger;
        private IHTTPClient httpClient;
        private IBeaconSendingContext context;

        [SetUp]
        public void Setup()
        {
            logger = Substitute.For<ILogger>();
            httpClient = Substitute.For<IHTTPClient>();
            context = Substitute.For<IBeaconSendingContext>();
            context.GetHTTPClient().Returns(httpClient);
            httpClient.SendStatusRequest().Returns(new StatusResponse(logger, string.Empty, Response.HttpOk, new Dictionary<string, List<string>>()));
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
        public void ToStringReturnsTheStateName()
        {
            // given
            var target = new BeaconSendingInitState();

            // then
            Assert.That(target.ToString(), Is.EqualTo("Initial"));
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

            // when
            var target = new BeaconSendingInitState();
            target.Execute(context);

            // then
            context.Received(2).InitCompleted(false);
            context.Received(1).NextState = Arg.Any<BeaconSendingTerminalState>();
        }

        [Test]
        public void ReinitializeSleepsBeforeSendingStatusRequests()
        {
            // given
            var count = 0;
            var erroneousResponse = new StatusResponse(logger, string.Empty, Response.HttpBadRequest, new Dictionary<string, List<string>>());
            httpClient.SendStatusRequest().Returns(erroneousResponse); // always return erroneous response
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
            var erroneousResponse = new StatusResponse(logger, string.Empty, Response.HttpBadRequest, new Dictionary<string, List<string>>());
            httpClient.SendStatusRequest().Returns(erroneousResponse); // always return erroneous response
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
            // when
            var target = new BeaconSendingInitState();
            target.Execute(context);

            // then
            context.Received(1).NextState = Arg.Any<BeaconSendingTimeSyncState>();
        }

        [Test]
        public void ReceivingTooManyRequestsResponseUsesSleepTimeFromResponse()
        {
            // given
            var responseHeaders = new Dictionary<string, List<string>>
            {
                { Response.ResponseKeyRetryAfter, new List<string> { "1234" } }
            };
            var tooManyRequestsResponse = new StatusResponse(logger, string.Empty, Response.HttpTooManyRequests, responseHeaders);

            httpClient.SendStatusRequest().Returns(tooManyRequestsResponse);
            context.IsShutdownRequested.Returns(false, true);

            var target = new BeaconSendingInitState();
        
            // when
            target.Execute(context);

            // verify sleep was performed accordingly
            context.Received(1).Sleep(1234 * 1000);
        }

        [Test]
        public void ReceivingTooManyRequestsResponseDisablesCapturing()
        {
            // given
            var responseHeaders = new Dictionary<string, List<string>>
            {
                { Response.ResponseKeyRetryAfter, new List<string> { "1234" } }
            };
            var tooManyRequestsResponse = new StatusResponse(logger, string.Empty, Response.HttpTooManyRequests, responseHeaders);

            httpClient.SendStatusRequest().Returns(tooManyRequestsResponse);
            context.IsShutdownRequested.Returns(false, true);

            var target = new BeaconSendingInitState();

            // when
            target.Execute(context);

            // verify sleep was performed accordingly
            context.Received(1).DisableCapture();
        }
    }
}
