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
            Assert.That(target[0], Does.StartWith(string.Format("et=60&na={0}&it=0&pa=0&s0=1&t0=", userID)));
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
