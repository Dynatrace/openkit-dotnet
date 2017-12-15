using NUnit.Framework;
using System.Threading;

namespace Dynatrace.OpenKit.Providers
{
    public class DefaultThreadIDProviderTest
    {
        [Test]
        public void CurrentThreadIDIsReturned()
        {
            // given
            var provider = new DefaultThreadIDProvider();

            // then
            Assert.That(provider.ThreadID, Is.EqualTo(Thread.CurrentThread.ManagedThreadId));
        }
    }
}
