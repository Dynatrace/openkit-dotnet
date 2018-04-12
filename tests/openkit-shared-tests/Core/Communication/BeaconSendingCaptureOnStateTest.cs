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
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core.Communication
{
    public class BeaconSendingCaptureOnStateTest
    {
        private OpenKitConfiguration config = new TestConfiguration();
        private Queue<Session> finishedSessions;
        private List<Session> openSessions;
        private long currentTime = 0;
        private long lastTimeSyncTime = 1;

        private IHTTPClient httpClient;
        private ITimingProvider timingProvider;
        private IBeaconSendingContext context;
        private BeaconSender beaconSender;
        private IHTTPClientProvider httpClientProvider;

        [SetUp]
        public void Setup()
        {
            currentTime = 1;
            lastTimeSyncTime = 1;
            openSessions = new List<Session>();
            finishedSessions = new Queue<Session>();

            // http client
            httpClient = Substitute.For<IHTTPClient>();

            // provider
            timingProvider = Substitute.For<ITimingProvider>();
            timingProvider.ProvideTimestampInMilliseconds().Returns(x => { return ++currentTime; }); // every access is a tick
            httpClientProvider = Substitute.For<IHTTPClientProvider>();
            httpClientProvider.CreateClient(Arg.Any<HTTPClientConfiguration>()).Returns(x => httpClient);

            // context
            context = Substitute.For<IBeaconSendingContext>();
            context.HTTPClientProvider.Returns(x => httpClientProvider);
            context.GetHTTPClient().Returns(x => httpClient);
            context.LastTimeSyncTime.Returns(x => currentTime); // always return the current time to prevent re-sync
            context.IsCaptureOn.Returns(true);

            // beacon sender
            beaconSender = new BeaconSender(config, httpClientProvider, timingProvider);

            // return true by default
            context.IsTimeSyncSupported.Returns(true);

            // current time getter
            context.CurrentTimestamp.Returns(x => timingProvider.ProvideTimestampInMilliseconds());

            // last time sycn getter + setter
            context.LastTimeSyncTime = Arg.Do<long>(x => lastTimeSyncTime = x);
            context.LastTimeSyncTime = lastTimeSyncTime;

            // sessions
            context.GetAllOpenSessions().Returns(openSessions);
            context.GetNextFinishedSession().Returns(x => (finishedSessions.Count == 0) ? null : finishedSessions.Dequeue());
        }

        [Test]
        public void StateIsNotTerminal()
        {
            // when
            var target = new BeaconSendingCaptureOnState();

            // then
            Assert.That(target.IsTerminalState, Is.False);
        }

        [Test]
        public void ShutdownStateIsFlushState()
        {
            // when
            var target = new BeaconSendingCaptureOnState();

            // then
            Assert.That(target.ShutdownState, Is.InstanceOf(typeof(BeaconSendingFlushSessionsState)));
        }

        [Test]
        public void TransitionToTimeSycnIsPerformed()
        {
            // given
            var lastTimeSync = 1;

            context.LastTimeSyncTime.Returns(lastTimeSync); // return fixed value
            context.CurrentTimestamp.Returns(lastTimeSync + BeaconSendingTimeSyncState.TIME_SYNC_INTERVAL_IN_MILLIS + 1); // timesync interval + 1 sec

            // when
            var target = new BeaconSendingCaptureOnState();
            target.Execute(context);

            // then
            context.Received(1).CurrentState = Arg.Any<BeaconSendingTimeSyncState>();
        }

        [Test]
        public void TransitionToCaptureOffStateIsPerformed()
        {
            // given
            var clientIp = "127.0.0.1";
            context.IsCaptureOn.Returns(false);
            var statusResponse = new StatusResponse(string.Empty, 200);

            finishedSessions.Enqueue(CreateValidSession(clientIp));
            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(x => statusResponse);

            // when
            var target = new BeaconSendingCaptureOnState();
            target.Execute(context);

            // then
            context.Received(1).CurrentState = Arg.Any<BeaconSendingCaptureOffState>();
        }

        [Test]
        public void TransitionToFlushStateIsPerformedOnShutdown()
        {
            // given
            context.IsShutdownRequested.Returns(true);

            // when
            var target = new BeaconSendingCaptureOnState();
            target.Execute(context);

            // then
            context.Received(1).CurrentState = Arg.Any<BeaconSendingFlushSessionsState>();
        }

        [Test]
        public void FinishedSessionsAreSent()
        {
            // given 
            var clientIp = "127.0.0.1";
            var statusResponse = new StatusResponse(string.Empty, 200);

            finishedSessions.Enqueue(CreateValidSession(clientIp));
            finishedSessions.Enqueue(CreateValidSession(clientIp));
            finishedSessions.Enqueue(CreateValidSession(clientIp));

            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(x => statusResponse);

            // when 
            var target = new BeaconSendingCaptureOnState();
            target.Execute(context);

            // then
            httpClient.Received(3).SendBeaconRequest(clientIp, Arg.Any<byte[]>());
            context.Received(1).HandleStatusResponse(statusResponse);
            Assert.That(finishedSessions, Is.Empty);
        }

        [Test]
        public void EmptyFinishedSessionsAreNotSent()
        {
            // given 
            var clientIp = "127.0.0.1";

            finishedSessions.Enqueue(CreateEmptySession(clientIp));

            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(x => new StatusResponse(string.Empty, 200));

            // when 
            var target = new BeaconSendingCaptureOnState();
            target.Execute(context);

            // then
            httpClient.DidNotReceive().SendBeaconRequest(clientIp, Arg.Any<byte[]>());
            Assert.That(finishedSessions.Count, Is.EqualTo(0)); // assert empty sessions
            context.DidNotReceive().HandleStatusResponse(Arg.Any<StatusResponse>());
        }
        
        [Test]
        public void UnsuccessfulFinishedSessionsAreMovedBackToCache()
        {
            //given
            var target = new BeaconSendingCaptureOnState();

            var finishedSession = CreateValidSession("127.0.0.1");
            context.GetNextFinishedSession().Returns(finishedSession);
            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>()).Returns((StatusResponse)null);


            //when calling execute
            target.Execute(context);

            context.Received(1).GetNextFinishedSession();
            context.Received(1).PushBackFinishedSession(finishedSession);
        }

        [Test]
        public void ABeaconSendingCaptureOnStateContinuesWithNextFinishedSessionIfSendingWasUnsuccessfulButBeaconIsEmtpy()
        {
            //given
            var target = new BeaconSendingCaptureOnState();

            var finishedEmptySession = CreateEmptySession("127.0.0.2");
            var finishedSession = CreateValidSession("127.0.0.1");

            var statusResponses = new Queue<StatusResponse>();
            context.GetNextFinishedSession().Returns(finishedEmptySession, finishedSession, null);
            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(new StatusResponse(string.Empty, 200));
            
            //when calling execute
            target.Execute(context);

            context.Received(3).GetNextFinishedSession();
            context.Received(0).PushBackFinishedSession(finishedSession);
        }

    [Test]
        public void OpenSessionsAreSentIfSendIntervalIsExceeded()
        {
            // given
            var clientIp = "127.0.0.1";

            var lastSendTime = 1;
            var sendInterval = 1000;
            var statusResponse = new StatusResponse(string.Empty, 200);

            context.LastOpenSessionBeaconSendTime.Returns(lastSendTime);
            context.SendInterval.Returns(sendInterval);
            context.CurrentTimestamp.Returns(lastSendTime + sendInterval + 1);

            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(x => statusResponse);

            openSessions.Add(CreateValidSession(clientIp));

            // when 
            var target = new BeaconSendingCaptureOnState();
            target.Execute(context);

            // then
            httpClient.Received(1).SendBeaconRequest(clientIp, Arg.Any<byte[]>());
            context.Received(1).HandleStatusResponse(statusResponse);
            Assert.That(context.LastOpenSessionBeaconSendTime, Is.EqualTo(context.CurrentTimestamp)); // assert send time update
        }

        [Test]
        public void OpenSessionsAreNotSentIfSendIntervalIsNotExceeded()
        {
            // given
            var clientIp = "127.0.0.1";

            var lastSendTime = 1;
            var sendInterval = 1000;

            context.LastOpenSessionBeaconSendTime.Returns(lastSendTime);
            context.SendInterval.Returns(sendInterval);
            context.CurrentTimestamp.Returns(lastSendTime + 1);

            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(x => new StatusResponse(string.Empty, 200));

            openSessions.Add(CreateValidSession(clientIp));

            // when 
            var target = new BeaconSendingCaptureOnState();
            target.Execute(context);

            // then
            httpClient.DidNotReceive().SendBeaconRequest(clientIp, Arg.Any<byte[]>());
            context.DidNotReceive().HandleStatusResponse(Arg.Any<StatusResponse>());
        }

        private Session CreateValidSession(string clientIP)
        {
            var logger = Substitute.For<ILogger>();
            var session = new Session(logger, beaconSender, new Beacon(logger, new BeaconCache(), 
                config, clientIP, Substitute.For<IThreadIDProvider>(), timingProvider));

            session.EnterAction("Foo").LeaveAction();

            return session;
        }

        private Session CreateEmptySession(string clientIP)
        {
            var logger = Substitute.For<ILogger>();
            return new Session(logger, beaconSender, new Beacon(logger, new BeaconCache(),
                config, clientIP, Substitute.For<IThreadIDProvider>(), timingProvider));
        }
    }
}
