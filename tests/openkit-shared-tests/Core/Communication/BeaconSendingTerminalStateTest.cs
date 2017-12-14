using Dynatrace.OpenKit.Protocol;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Communication
{
    public class BeaconSendingTerminalStateTest
    {
        private IHTTPClient httpClient;
        private IBeaconSendingContext context;

        [SetUp]
        public void Setup()
        {
            httpClient = Substitute.For<IHTTPClient>();
            context = Substitute.For<IBeaconSendingContext>();
        }

        [Test]
        public void StateIsTerminal()
        {
            // given
            var target = new BeaconSendingTerminalState();

            // then
            Assert.That(target.IsTerminalState, Is.True);
        }

        [Test]
        public void TerminalStateIsNextStateIfTransitionFromTerminalStateOccurs()
        {
            //given
            var target = new BeaconSendingTerminalState();

            //then
            Assert.That(target.ShutdownState,Is.SameAs(target));
        }

        [Test]
        public void TerminalStateCallsShutdownOnExecution()
        {
            //given
            var target = new BeaconSendingTerminalState();

            // when
            target.Execute(context);

            // then
            context.Received(1).RequestShutdown();
        }
    }
}
