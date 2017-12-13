using Dynatrace.OpenKit.Protocol;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Communication
{
    [TestFixture]
    public class BeaconSendingTerminalStateTest
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
        public void StateIsTerminal()
        {
            // when
            var target = new BeaconSendingTerminalState();

            // then
            Assert.That(target.IsTerminalState, Is.True);
        }

        [Test]
        public void TerminalStateIsNextStateIfTransitionFromTerminalStateOccurs()
        {
            //when
            var target = new BeaconSendingTerminalState();

            //then
            Assert.That(Is.ReferenceEquals(target.ShutdownState, target), Is.True);
        }

        [Test]
        public void TerminalStateCallsShutdownOnExecution()
        {
            // given
            
            // when
            var target = new BeaconSendingTerminalState();
            target.Execute(context);

            // then
            context.Received(1).RequestShutdown();
        }
    }
}
