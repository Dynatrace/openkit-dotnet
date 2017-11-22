using Dynatrace.OpenKit.Protocol;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Communication
{
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
        public void InitCompleteIsCalledOnExecuteIfNoStatusIsReturned()
        {
            // given 
            httpClient.SendStatusRequest().Returns((StatusResponse)null);

            // when
            var target = new BeaconSendingInitState();
            target.Execute(context);

            // then
            context.Received(1).InitCompleted(false);
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
        }

        [Test]
        public void StatusRequestIsRetried()
        {
            // given
            httpClient.SendStatusRequest().Returns((StatusResponse)null); // always return null

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
            context.Received(1).CurrentState = Arg.Any<BeaconSendingTimeSyncState>();
        }

        [Test]
        public void TransitionToTerminalStateIsPerformedOnFailure()
        {
            // given
            httpClient.SendStatusRequest().Returns((StatusResponse)null); // always return null

            // when
            var target = new BeaconSendingInitState();
            target.Execute(context);

            // then
            context.Received(1).CurrentState = Arg.Any<BeaconSendingTerminalState>();
        }
    }
}
