using Dynatrace.OpenKit.Providers;
using NUnit.Framework;
using NSubstitute;
using Dynatrace.OpenKit.Core;
using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Protocol
{
    public class BeaconTest
    {
        private IThreadIDProvider threadIdProvider;

        [SetUp]
        public void Setup()
        {
            threadIdProvider = Substitute.For<IThreadIDProvider>();
        }

        [Test]
        public void CanAddUserIdentifyEvent()
        {
            // given
            var beacon = new Beacon(new TestConfiguration(), "127.0.0.1", threadIdProvider);
            var userID = "myTestUser";
    
            // when
            beacon.IdentifyUser(userID);
            var target = beacon.EventDataList;

            // then
            Assert.That(target.Count, Is.EqualTo(1));
            // TODO - thomas.grassaue@dynatrace.com - ignore timestamp for now (due to static provider)
            Assert.That(target[0], Does.StartWith($"et=60&na={userID}&it=0&pa=0&s0=1&t0="));
        }

        [Test]
        public void CanAddRootActionIfCaptureIsOn()
        {
            // given
            var configuration = new TestConfiguration();
            configuration.EnableCapture();

            var target = new Beacon(configuration, "127.0.0.1", threadIdProvider);

            // when adding the root action
            const string RootActionName = "TestRootAction";
            target.AddAction(new RootAction(target, RootActionName, new SynchronizedQueue<IAction>()));

            // then
            Assert.That(target.ActionDataList.Count, Is.EqualTo(1));
            // TODO - stefan.eberl@dynatrace.com - ignore timestamp for now (due to static provider)
            Assert.That(target.ActionDataList[0], Does.StartWith($"et=1&na={RootActionName}&it=0&ca=1&pa=0&s0=1&t0="));
        }

        [Test]
        public void CannotAddRootActionIfCaptureIsOff()
        {
            // given
            var configuration = new TestConfiguration();
            configuration.DisableCapture();

            var target = new Beacon(configuration, "127.0.0.1", threadIdProvider);

            // when adding the root action
            const string RootActionName = "TestRootAction";
            target.AddAction(new RootAction(target, RootActionName, new SynchronizedQueue<IAction>()));

            // then
            Assert.That(target.ActionDataList, Is.Empty);
        }

        [Test]
        public void ClearDataClearsActionAndEventData()
        {
            // given
            var target = new Beacon(new TestConfiguration(), "127.0.0.1", threadIdProvider);
            var action = new Action(target, "TestAction", new SynchronizedQueue<IAction>());
            action.ReportEvent("TestEvent").ReportValue("TheAnswerToLifeTheUniverseAndEverything", 42);
            target.AddAction(action);

            // then check data both lists are not empty
            Assert.That(target.ActionDataList, Is.Not.Empty);
            Assert.That(target.EventDataList, Is.Not.Empty);

            // and when clearing the data
            target.ClearData();

            // then check data both lists are emtpy
            Assert.That(target.ActionDataList, Is.Empty);
            Assert.That(target.EventDataList, Is.Empty);
        }
    }
}
