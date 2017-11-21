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
            beaconSender = new BeaconSender(context);

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
            Assert.That(target.IsTerminalState, Is.EqualTo(false));
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

        private Session CreateValidSession(string clientIp)
        {
            var session = new Session(new TestConfiguration(), clientIp, beaconSender);

            session.EnterAction("Foo").LeaveAction();

            return session;
        }

    }
}
