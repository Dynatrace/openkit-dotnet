using NUnit.Framework;
using System.Threading;

namespace Dynatrace.OpenKit.Providers
{
    public class DefaultSessionIDProviderTest
    {
        [Test]
        public void DefaultSessionIDProviderReturnsNonNegativeID()
        {
            // given
            var provider = new DefaultSessionIDProvider();

            // then
            Assert.That(provider.GetNextSessionID(), Is.GreaterThan(0));
        }

        [Test]
        public void DefaultSessionIDProviderReturnsConsecutiveIDs()
        {
            // given
            var provider = new DefaultSessionIDProvider(int.MaxValue/2);

            // when
            var sessionIDOne = provider.GetNextSessionID();
            var sessionIDTwo = provider.GetNextSessionID();

            // then
            Assert.That(sessionIDTwo, Is.EqualTo(sessionIDOne + 1));
        }

        [Test]
        public void aProviderInitializedWithMaxIntValueProvidesMinSessionIdValueAtNextCall()
        {
            //given
            DefaultSessionIDProvider provider = new DefaultSessionIDProvider(int.MaxValue);

            //when
            int actual = provider.GetNextSessionID();

            //then
            Assert.That(actual, Is.EqualTo(1));
        }

        [Test]
        public void aProviderInitializedWithZeroProvidesMinSessionIdValueAtNextCall()
        {
            //given
            DefaultSessionIDProvider provider = new DefaultSessionIDProvider(0);

            //when
            int actual = provider.GetNextSessionID();

            //then
            Assert.That(actual, Is.EqualTo(1));
        }
    }
}
