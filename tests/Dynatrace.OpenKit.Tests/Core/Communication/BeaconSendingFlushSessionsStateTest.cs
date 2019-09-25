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
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Communication
{
    public class BeaconSendingFlushSessionsStateTest
    {
        private List<SessionWrapper> newSessions;
        private List<SessionWrapper> openSessions;
        private List<SessionWrapper> finishedSessions;
        private ITimingProvider timingProvider;
        private IHttpClient httpClient;
        private IBeaconSendingContext context;
        private IHttpClientProvider httpClientProvider;
        private BeaconSender beaconSender;

        private IOpenKitComposite mockParent;

        [SetUp]
        public void Setup()
        {
            httpClient = Substitute.For<IHttpClient>();
            newSessions = new List<SessionWrapper>();
            openSessions = new List<SessionWrapper>();
            finishedSessions = new List<SessionWrapper>();

            // provider
            timingProvider = Substitute.For<ITimingProvider>();
            httpClientProvider = Substitute.For<IHttpClientProvider>();
            httpClientProvider.CreateClient(Arg.Any<HttpClientConfiguration>()).Returns(x => httpClient);

            // context
            context = Substitute.For<IBeaconSendingContext>();
            context.GetHttpClient().Returns(httpClient);
            context.HttpClientProvider.Returns(httpClientProvider);

            // beacon sender
            var logger = Substitute.For<ILogger>();
            beaconSender = new BeaconSender(logger, context);

            // sessions
            context.NewSessions.Returns(newSessions);
            context.OpenAndConfiguredSessions.Returns(openSessions);
            context.FinishedAndConfiguredSessions.Returns(finishedSessions);

            mockParent = Substitute.For<IOpenKitComposite>();
        }

        [Test]
        public void StateIsNotTerminal()
        {
            // when
            var target = new BeaconSendingFlushSessionsState();

            // then
            Assert.That(target.IsTerminalState, Is.False);
        }

        [Test]
        public void ShutdownStateIsTerminalState()
        {
            // when
            var target = new BeaconSendingFlushSessionsState();

            // then
            Assert.That(target.ShutdownState, Is.InstanceOf(typeof(BeaconSendingTerminalState)));
        }

        [Test]
        public void ToStringReturnStateName()
        {
            // given
            var target = new BeaconSendingFlushSessionsState();

            // then
            Assert.That(target.ToString(), Is.EqualTo("FlushSessions"));
        }

        [Test]
        public void ABeaconSendingFlushSessionsStateTransitionsToTerminalStateWhenDataIsSent()
        {
            // given
            var target = new BeaconSendingFlushSessionsState();

            // when
            target.Execute(context);

            // then verify transition to terminal state
            context.Received(1).NextState = Arg.Any<BeaconSendingTerminalState>();
        }

        [Test]
        public void ABeaconSendingFlushSessionsStateConfiguresAllNotConfiguredSessions()
        {
            // given
            var target = new BeaconSendingFlushSessionsState();

            var sessionOne = new SessionWrapper(CreateValidSession("127.0.0.1"));
            var sessionTwo = new SessionWrapper(CreateValidSession("127.0.0.2"));
            var sessionThree = new SessionWrapper(CreateValidSession("127.0.0.2"));
            // end one session to demonstrate that those which are already ended are also configured
            sessionThree.End();
            newSessions.AddRange(new[] { sessionOne, sessionTwo, sessionThree });

            // when
            target.Execute(context);

            // verify that all three sessions are configured
            Assert.That(sessionOne.IsBeaconConfigurationSet, Is.True);
            Assert.That(sessionOne.BeaconConfiguration.Multiplicity, Is.EqualTo(1));
            Assert.That(sessionTwo.IsBeaconConfigurationSet, Is.True);
            Assert.That(sessionTwo.BeaconConfiguration.Multiplicity, Is.EqualTo(1));
            Assert.That(sessionThree.IsBeaconConfigurationSet, Is.True);
            Assert.That(sessionThree.BeaconConfiguration.Multiplicity, Is.EqualTo(1));
        }

        [Test]
        public void ABeaconSendingFlushSessionsStateClosesOpenSessions()
        {
            // given
            var target = new BeaconSendingFlushSessionsState();

            var sessionOne = new SessionWrapper(CreateValidSession("127.0.0.1"));
            sessionOne.UpdateBeaconConfiguration(new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF));
            var sessionTwo = new SessionWrapper(CreateValidSession("127.0.0.2"));
            sessionTwo.UpdateBeaconConfiguration(new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF));

            openSessions.AddRange(new[] { sessionOne, sessionTwo });

            // when
            target.Execute(context);

            // verify that open sessions are closed
            context.Received(1).FinishSession(sessionOne.Session);
            context.Received(1).FinishSession(sessionTwo.Session);
        }

        private Session CreateValidSession(string clientIp)
        {
            var logger = Substitute.For<ILogger>();
            var session = new Session(logger, mockParent, beaconSender, new Beacon(logger, new BeaconCache(logger),
                new TestConfiguration(), clientIp, Substitute.For<IThreadIdProvider>(), timingProvider));

            session.EnterAction("Foo").LeaveAction();

            return session;
        }

    }
}
