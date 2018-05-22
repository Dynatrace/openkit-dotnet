﻿//
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
    public class BeaconSendingFlushSessionsStateTest
    {
        private Queue<Session> finishedSessions;
        private List<Session> openSessions;
        private ITimingProvider timingProvider;
        private IHTTPClient httpClient;
        private IBeaconSendingContext context;
        private IHTTPClientProvider httpClientProvider;
        private BeaconSender beaconSender;

        [SetUp]
        public void Setup()
        {
            httpClient = Substitute.For<IHTTPClient>();
            finishedSessions = new Queue<Session>();
            openSessions = new List<Session>();

            // provider
            timingProvider = Substitute.For<ITimingProvider>();
            httpClientProvider = Substitute.For<IHTTPClientProvider>();
            httpClientProvider.CreateClient(Arg.Any<HTTPClientConfiguration>()).Returns(x => httpClient);

            // context
            context = Substitute.For<IBeaconSendingContext>();
            context.GetHTTPClient().Returns(httpClient);
            context.HTTPClientProvider.Returns(httpClientProvider);

            // beacon sender
            var logger = Substitute.For<ILogger>();
            beaconSender = new BeaconSender(logger, context);

            // sessions
            context.GetAllOpenSessions().Returns(openSessions);
            context.GetNextFinishedSession().Returns(x => (finishedSessions.Count == 0) ? null : finishedSessions.Dequeue());
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
        public void FinishedSessionsAreSent()
        {
            // given 
            finishedSessions.Enqueue(CreateValidSession("127.0.0.1"));
            finishedSessions.Enqueue(CreateValidSession("127.0.0.1"));

            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>()).Returns(x => new StatusResponse(string.Empty, 200));

            // when
            var target = new BeaconSendingFlushSessionsState();
            target.Execute(context);

            // then
            httpClient.Received(2).SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>());
        }

        private Session CreateValidSession(string clientIP)
        {
            var logger = Substitute.For<ILogger>();
            var session = new Session(logger, beaconSender, new Beacon(logger, new BeaconCache(logger),
                new TestConfiguration(), clientIP, Substitute.For<IThreadIDProvider>(), timingProvider));

            session.EnterAction("Foo").LeaveAction();

            return session;
        }

    }
}
