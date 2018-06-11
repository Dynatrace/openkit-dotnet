using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class BeaconConfigurationTest
    {
        [Test]
        public void GetMultiplicityReturnsMultiplicitySetInConstructor()
        {
            // when multiplicity is positive, then
            Assert.That(new BeaconConfiguration(4).Multiplicity, Is.EqualTo(4));
            // when multiplicity is zero, then
            Assert.That(new BeaconConfiguration(0).Multiplicity, Is.EqualTo(0));
            // when multiplicity is negative, then
            Assert.That(new BeaconConfiguration(-3).Multiplicity, Is.EqualTo(-3));
        }

        [Test]
        public void CapturingIsAllowedWhenMultiplicityIsGreaterThanZero()
        {
            // when, then
            Assert.That(new BeaconConfiguration(1).CapturingAllowed, Is.True);
            Assert.That(new BeaconConfiguration(2).CapturingAllowed, Is.True);
            Assert.That(new BeaconConfiguration(int.MaxValue).CapturingAllowed, Is.True);
        }

        [Test]
        public void CapturingIsDisallowedWhenMultiplicityIsLessThanOrEqualToZero()
        {
            // when, then
            Assert.That(new BeaconConfiguration(0).CapturingAllowed, Is.False);
            Assert.That(new BeaconConfiguration(-1).CapturingAllowed, Is.False);
            Assert.That(new BeaconConfiguration(int.MinValue).CapturingAllowed, Is.False);
        }
    }
}
