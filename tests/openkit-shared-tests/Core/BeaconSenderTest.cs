using Dynatrace.OpenKit.Core.Communication;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dynatrace.OpenKit.Core
{
    public class BeaconSenderTest
    {
        private IBeaconSendingContext context;

        [SetUp]
        public void SetUp()
        {
            context = Substitute.For<IBeaconSendingContext>();
        }

        [Test]
        public void IsInitializedGetsValueFromContext()
        {
            // given
            var target = new BeaconSender(context);

            // when context is not initialized
            context.IsInitialized.Returns(false);

            // then
            Assert.That(target.IsInitialized, Is.False);

            // and when context is initialized
            context.IsInitialized.Returns(true);

            // then
            Assert.That(target.IsInitialized, Is.True);
        }
    }
}
