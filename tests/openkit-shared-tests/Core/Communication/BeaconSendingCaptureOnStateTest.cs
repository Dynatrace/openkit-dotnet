using Dynatrace.OpenKit.Protocol;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Communication
{
    public class BeaconSendingCaptureOnStateTest
    {
        private long currentTime = 0;
        private long lastTimeSyncTime = -1;

        private IHTTPClient httpClient;
        private IBeaconSendingContext context;

        [SetUp]
        public void Setup()
        {
            currentTime = 0;
            lastTimeSyncTime = -1;

            httpClient = Substitute.For<IHTTPClient>();
            context = Substitute.For<IBeaconSendingContext>();
            context.GetHTTPClient().Returns(httpClient);

            // return true by default
            context.IsTimeSyncSupported.Returns(true);

            // current time getter
            context.CurrentTimestamp.Returns(x => { return ++currentTime; }); // every access is a tick

            // last time sycn getter + setter
            context.LastTimeSyncTime = Arg.Do<long>(x => lastTimeSyncTime = x); 
            context.LastTimeSyncTime = lastTimeSyncTime; // init with -1
        }

        [Test]
        public void StateIsNotTerminal()
        {
            // when
            var target = new BeaconSendingCaptureOnState();

            // then
            Assert.That(target.IsTerminalState, Is.EqualTo(false));
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
    }
}
