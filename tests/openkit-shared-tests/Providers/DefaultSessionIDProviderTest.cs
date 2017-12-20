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
            var provider = new DefaultSessionIDProvider();

            // when
            var sessionIDOne = provider.GetNextSessionID();
            var sessionIDTwo = provider.GetNextSessionID();

            // then
            Assert.That(sessionIDOne + 1 == sessionIDTwo);
        }
    }
}
