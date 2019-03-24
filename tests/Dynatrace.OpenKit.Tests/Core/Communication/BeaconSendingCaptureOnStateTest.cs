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
        private readonly OpenKitConfiguration config = new TestConfiguration();
        private List<SessionWrapper> newSessions;
        private List<SessionWrapper> openSessions;
        private List<SessionWrapper> finishedSessions;
        private long currentTime = 0;

        private ILogger logger;
        private IHTTPClient httpClient;
        private ITimingProvider timingProvider;
        private IBeaconSendingContext context;
        private BeaconSender beaconSender;
        private IHTTPClientProvider httpClientProvider;

        [SetUp]
        public void Setup()
        {
            currentTime = 1;
            newSessions = new List<SessionWrapper>();
            openSessions = new List<SessionWrapper>();
            finishedSessions = new List<SessionWrapper>();

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
            context.IsCaptureOn.Returns(true);

            // beacon sender
            logger = Substitute.For<ILogger>();
            beaconSender = new BeaconSender(logger, config, httpClientProvider, timingProvider);
            
            // current time getter
            context.CurrentTimestamp.Returns(x => timingProvider.ProvideTimestampInMilliseconds());

            // sessions
            context.NewSessions.Returns(newSessions);
            context.OpenAndConfiguredSessions.Returns(openSessions);
            context.FinishedAndConfiguredSessions.Returns(finishedSessions);
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
        public void ToStringReturnStateName()
        {
            // given
            var target = new BeaconSendingCaptureOnState();

            // then
            Assert.That(target.ToString(), Is.EqualTo("CaptureOn"));
        }

        [Test]
        public void TransitionToCaptureOffStateIsPerformed()
        {
            // given
            var clientIp = "127.0.0.1";
            context.IsCaptureOn.Returns(false);
            var statusResponse = new StatusResponse(logger, string.Empty, 200, new Dictionary<string, List<string>>());

            var session = new SessionWrapper(CreateValidSession(clientIp));
            session.UpdateBeaconConfiguration(new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF));
            finishedSessions.Add(session);
            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(x => statusResponse);

            // when
            var target = new BeaconSendingCaptureOnState();
            target.Execute(context);

            // then
            context.Received(1).NextState = Arg.Any<BeaconSendingCaptureOffState>();
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
            context.Received(1).NextState = Arg.Any<BeaconSendingFlushSessionsState>();
        }

        [Test]
        public void NewSessionRequestsAreMadeForAllNewSessions()
        {
            // given
            var target = new BeaconSendingCaptureOnState();

            var sessionOne = new SessionWrapper(CreateValidSession("127.0.0.1"));
            var sessionTwo = new SessionWrapper(CreateEmptySession("127.0.0.2"));
            newSessions.AddRange(new[] { sessionOne, sessionTwo });

            httpClient.SendNewSessionRequest().Returns(new StatusResponse(logger, "mp=5", Response.HttpOk, new Dictionary<string, List<string>>()),
                                                       new StatusResponse(logger, "mp=5", Response.HttpBadRequest, new Dictionary<string, List<string>>()),
                                                       new StatusResponse(logger, "mp=3", Response.HttpOk, new Dictionary<string, List<string>>()));

            // when
            target.Execute(context);

            // verify for both new sessions a new session request has been made
            httpClient.Received(2).SendNewSessionRequest();

            // also verify that sessionOne got a new configuration
            Assert.That(sessionOne.IsBeaconConfigurationSet, Is.True);
            Assert.That(sessionOne.BeaconConfiguration.Multiplicity, Is.EqualTo(5));

            // for session two the number of requests was decremented
            Assert.That(sessionTwo.IsBeaconConfigurationSet, Is.False);
            Assert.That(sessionTwo.NumNewSessionRequestsLeft, Is.EqualTo(3));
        }

        [Test]
        public void NewSessionRequestsAreAbortedWhenTooManyRequestsResponseIsReceived()
        {
            // given
            var target = new BeaconSendingCaptureOnState();

            var sessionOne = new SessionWrapper(CreateValidSession("127.0.0.1"));
            var sessionTwo = new SessionWrapper(CreateValidSession("127.0.0.2"));
            newSessions.AddRange(new[] { sessionOne, sessionTwo });

            var responseHeaders = new Dictionary<string, List<string>>
            {
                { Response.ResponseKeyRetryAfter, new List<string> { "1234  "} }
            };
            httpClient.SendNewSessionRequest().Returns(new StatusResponse(logger, string.Empty, Response.HttpTooManyRequests, responseHeaders),
                                           new StatusResponse(logger, "mp=1", Response.HttpOk, new Dictionary<string, List<string>>()),
                                           new StatusResponse(logger, "mp=1", Response.HttpOk, new Dictionary<string, List<string>>()));

            AbstractBeaconSendingState capturedState = null;
            context.NextState = Arg.Do<AbstractBeaconSendingState>(x => capturedState = x);

            // when
            target.Execute(context);

            // verify for first new sessions a new session request has been made
            httpClient.Received(1).SendNewSessionRequest();

            // verify no changes on first & second sesion
            Assert.That(sessionOne.CanSendNewSessionRequest, Is.True);
            Assert.That(sessionOne.IsBeaconConfigurationSet, Is.False);

            Assert.That(sessionTwo.CanSendNewSessionRequest, Is.True);
            Assert.That(sessionTwo.IsBeaconConfigurationSet, Is.False);

            // ensure that state transition has been made
            context.ReceivedWithAnyArgs(1).NextState = null;
            Assert.That(capturedState, Is.Not.Null);
            Assert.That(capturedState, Is.InstanceOf<BeaconSendingCaptureOffState>());
            Assert.That(((BeaconSendingCaptureOffState)capturedState).sleepTimeInMilliseconds, Is.EqualTo(1234 * 1000));

            context.ReceivedWithAnyArgs(0).HandleStatusResponse(null);
        }

        [Test]
        public void MultiplicityIsSetToZeroIfNoFurtherNewSessionRequestsAreAllowed()
        {
            // given
            var target = new BeaconSendingCaptureOnState();

            var sessionOne = new SessionWrapper(CreateValidSession("127.0.0.1"));
            var sessionTwo = new SessionWrapper(CreateEmptySession("127.0.0.2"));
            newSessions.AddRange(new[] { sessionOne, sessionTwo });

            httpClient.SendNewSessionRequest().Returns(new StatusResponse(logger, "mp=5", 200, new Dictionary<string, List<string>>()), null);

            // ensure that it's no longer possible to send session requests for both session wrapper
            while (sessionOne.CanSendNewSessionRequest)
            {
                sessionOne.DecreaseNumNewSessionRequests();
            }
            while (sessionTwo.CanSendNewSessionRequest)
            {
                sessionTwo.DecreaseNumNewSessionRequests();
            }

            // when
            target.Execute(context);

            // verify for no session a new session request has been made
            httpClient.Received(0).SendNewSessionRequest();

            // also ensure that both got a configuration set
            Assert.That(sessionOne.IsBeaconConfigurationSet, Is.True);
            Assert.That(sessionOne.BeaconConfiguration.Multiplicity, Is.EqualTo(0));

            Assert.That(sessionTwo.IsBeaconConfigurationSet, Is.True);
            Assert.That(sessionTwo.BeaconConfiguration.Multiplicity, Is.EqualTo(0));
        }

        [Test]
        public void FinishedSessionsAreSent()
        {
            // given 
            var clientIp = "127.0.0.1";
            var statusResponse = new StatusResponse(logger, string.Empty, Response.HttpOk, new Dictionary<string, List<string>>());

            finishedSessions.AddRange(new[] {
                new SessionWrapper(CreateValidSession(clientIp)),
                new SessionWrapper(CreateValidSession(clientIp)),
                new SessionWrapper(CreateValidSession(clientIp)) });
            finishedSessions.ForEach(s => s.UpdateBeaconConfiguration(new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF)));

            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(x => statusResponse);

            // when 
            var target = new BeaconSendingCaptureOnState();
            target.Execute(context);

            // then
            httpClient.Received(3).SendBeaconRequest(clientIp, Arg.Any<byte[]>());
            context.Received(1).HandleStatusResponse(statusResponse);
        }

        [Test]
        public void SendingFinishedSessionsIsAbortedImmediatelyWhenTooManyRequestsResponseIsReceived()
        {
            // given 
            var clientIp = "127.0.0.1";
            var responseHeaders = new Dictionary<string, List<string>>
            {
                { Response.ResponseKeyRetryAfter, new List<string> { "4321"} }
            };
            var statusResponse = new StatusResponse(logger, string.Empty, Response.HttpTooManyRequests, responseHeaders);

            finishedSessions.AddRange(new[] {
                new SessionWrapper(CreateValidSession(clientIp)),
                new SessionWrapper(CreateValidSession(clientIp)),
                new SessionWrapper(CreateValidSession(clientIp)) });
            finishedSessions.ForEach(s => s.UpdateBeaconConfiguration(new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF)));

            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(x => statusResponse);

            AbstractBeaconSendingState capturedState = null;
            context.NextState = Arg.Do<AbstractBeaconSendingState>(x => capturedState = x);

            var target = new BeaconSendingCaptureOnState();

            // when 
            target.Execute(context);

            // then
            // verify only one session request has been made
            httpClient.Received(1).SendBeaconRequest(clientIp, Arg.Any<byte[]>());

            // ensure that state transition has been made
            context.ReceivedWithAnyArgs(1).NextState = null;
            Assert.That(capturedState, Is.Not.Null);
            Assert.That(capturedState, Is.InstanceOf<BeaconSendingCaptureOffState>());
            Assert.That(((BeaconSendingCaptureOffState)capturedState).sleepTimeInMilliseconds, Is.EqualTo(4321 * 1000));

            context.ReceivedWithAnyArgs(0).HandleStatusResponse(null);
        }

        [Test]
        public void UnsuccessfulFinishedSessionsAreNotRemovedFromCache()
        {
            //given
            var statusResponse = new StatusResponse(logger, string.Empty, Response.HttpBadRequest, new Dictionary<string, List<string>>());
            var target = new BeaconSendingCaptureOnState();

            var finishedSession = new SessionWrapper(CreateValidSession("127.0.0.1"));
            finishedSession.UpdateBeaconConfiguration(new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF));
            finishedSessions.Add(finishedSession);
            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(statusResponse);

            //when calling execute
            target.Execute(context);

            var tmp = context.Received(1).FinishedAndConfiguredSessions;
            context.Received(0).RemoveSession(finishedSession);
        }

        [Test]
        public void ABeaconSendingCaptureOnStateContinuesWithNextFinishedSessionIfSendingWasUnsuccessfulButBeaconIsEmtpy()
        {
            //given
            var target = new BeaconSendingCaptureOnState();

            var sessionOne = new SessionWrapper(CreateEmptySession("127.0.0.2"));
            var sessionTwo = new SessionWrapper(CreateValidSession("127.0.0.1"));
            finishedSessions.AddRange(new[] { sessionOne, sessionTwo });

            var statusResponses = new Queue<StatusResponse>();
            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(new StatusResponse(logger, string.Empty, Response.HttpOk, new Dictionary<string, List<string>>()));

            //when calling execute
            target.Execute(context);

            var tmp = context.Received(1).FinishedAndConfiguredSessions;
            context.Received(1).RemoveSession(sessionOne);
            context.Received(1).RemoveSession(sessionTwo);
        }

        [Test]
        public void OpenSessionsAreSentIfSendIntervalIsExceeded()
        {
            // given
            var clientIp = "127.0.0.1";

            var lastSendTime = 1;
            var sendInterval = 1000;
            var statusResponse = new StatusResponse(logger, string.Empty, Response.HttpOk, new Dictionary<string, List<string>>());

            context.LastOpenSessionBeaconSendTime.Returns(lastSendTime);
            context.SendInterval.Returns(sendInterval);
            context.CurrentTimestamp.Returns(lastSendTime + sendInterval + 1);

            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(x => statusResponse);

            var session = new SessionWrapper(CreateValidSession(clientIp));
            session.UpdateBeaconConfiguration(new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF));
            openSessions.Add(session);

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

            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(x => new StatusResponse(logger, string.Empty, Response.HttpOk, new Dictionary<string, List<string>>()));

            var session = new SessionWrapper(CreateValidSession(clientIp));
            session.UpdateBeaconConfiguration(new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF));
            openSessions.Add(session);

            // when 
            var target = new BeaconSendingCaptureOnState();
            target.Execute(context);

            // then
            httpClient.DidNotReceive().SendBeaconRequest(clientIp, Arg.Any<byte[]>());
            context.DidNotReceive().HandleStatusResponse(Arg.Any<StatusResponse>());
        }

        [Test]
        public void SendingOpenSessionsIsAbortedImmediatelyWhenTooManyRequestsResponseIsReceived()
        {
            //given 
            var clientIp = "127.0.0.1";

            var lastSendTime = 1;
            var sendInterval = 1000;
            var responseHeaders = new Dictionary<string, List<string>>
            {
                { Response.ResponseKeyRetryAfter, new List<string> { "987654"} }
            };
            var statusResponse = new StatusResponse(logger, string.Empty, Response.HttpTooManyRequests, responseHeaders);
            openSessions.AddRange(new[] {
                new SessionWrapper(CreateValidSession(clientIp)),
                new SessionWrapper(CreateValidSession(clientIp)),
                new SessionWrapper(CreateValidSession(clientIp)) });
            openSessions.ForEach(s => s.UpdateBeaconConfiguration(new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF)));

            context.LastOpenSessionBeaconSendTime.Returns(lastSendTime);
            context.SendInterval.Returns(sendInterval);
            context.CurrentTimestamp.Returns(lastSendTime + sendInterval + 1);

            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(x => statusResponse);

            AbstractBeaconSendingState capturedState = null;
            context.NextState = Arg.Do<AbstractBeaconSendingState>(x => capturedState = x);

            var target = new BeaconSendingCaptureOnState();

            // when
            target.Execute(context);

            // then
            // verify only one session request has been made
            httpClient.Received(1).SendBeaconRequest(clientIp, Arg.Any<byte[]>());

            // ensure that state transition has been made
            context.ReceivedWithAnyArgs(1).NextState = null;
            Assert.That(capturedState, Is.Not.Null);
            Assert.That(capturedState, Is.InstanceOf<BeaconSendingCaptureOffState>());
            Assert.That(((BeaconSendingCaptureOffState)capturedState).sleepTimeInMilliseconds, Is.EqualTo(987654 * 1000));

            context.ReceivedWithAnyArgs(0).HandleStatusResponse(null);
        }

        private Session CreateValidSession(string clientIP)
        {
            var logger = Substitute.For<ILogger>();
            var session = new Session(logger, beaconSender, new Beacon(logger, new BeaconCache(logger),
                config, clientIP, Substitute.For<IThreadIDProvider>(), timingProvider));

            session.EnterAction("Foo").LeaveAction();

            return session;
        }

        private Session CreateEmptySession(string clientIP)
        {
            var logger = Substitute.For<ILogger>();
            return new Session(logger, beaconSender, new Beacon(logger, new BeaconCache(logger),
                config, clientIP, Substitute.For<IThreadIDProvider>(), timingProvider));
        }
    }
}
