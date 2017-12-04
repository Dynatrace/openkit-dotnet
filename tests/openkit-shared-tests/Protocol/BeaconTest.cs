using Dynatrace.OpenKit.Providers;
using NUnit.Framework;
using NSubstitute;

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
        public void canAddUserIdentifyEvent()
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
            StringAssert.StartsWith(string.Format("et=60&na={0}&it=0&pa=0&s0=1&t0=", userID), target[0]);
        }
    }
}
