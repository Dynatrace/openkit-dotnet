//
// Copyright 2018-2019 Dynatrace LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using NUnit.Framework;

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
