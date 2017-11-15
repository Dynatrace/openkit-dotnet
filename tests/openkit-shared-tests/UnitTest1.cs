using NUnit.Framework;

namespace Dynatrace.OpenKit
{
    public class UnitTest1
    {
        [Test]
        public void Test1()
        {
            Assert.AreEqual(17, 16);
        }

        [Test]
        public void Test2()
        {
            Assert.That(17, Is.EqualTo(17));
        }
    }
}
