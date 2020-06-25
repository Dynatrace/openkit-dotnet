//
// Copyright 2018-2020 Dynatrace LLC
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

namespace Dynatrace.OpenKit.Core.Caching
{
    public class BeaconKeyTest
    {
        [Test]
        public void ABeaconKeyDoesNotEqualNull()
        {
            // given
            var key = new BeaconKey(1, 0);

            // when
            var obtained = key.Equals(null);

            // then
            Assert.That(obtained, Is.EqualTo(false));
        }

        [Test]
        public void ABeaconKeyDoesNotEqualObjectOfDifferentType()
        {
            // given
            var key = new BeaconKey(1, 0);

            // when
            var obtained = key.Equals(new object());

            // when, then
            Assert.That(obtained, Is.EqualTo(false));
        }

        [Test]
        public void InstancesWithSameValuesAreEqual()
        {
            // given
            var keyOne = new BeaconKey(17, 18);
            var keyTwo = new BeaconKey(17, 18);

            // when
            var obtained = keyOne.Equals(keyTwo);

            // then
            Assert.That(obtained, Is.EqualTo(true));
        }

        [Test]
        public void InstancesWithSameValuesHaveSameHash()
        {
            // given
            var keyOne = new BeaconKey(17, 18);
            var keyTwo = new BeaconKey(17, 18);

            // when, then
            Assert.That(keyOne.GetHashCode(), Is.EqualTo(keyTwo.GetHashCode()));
        }

        [Test]
        public void InstancesWithDifferentBeaconIdAreNotEqual()
        {
            // given
            var keyOne = new BeaconKey(37, 18);
            var keyTwo = new BeaconKey(38, 18);

            // when
            var obtained = keyOne.Equals(keyTwo);

            // then
            Assert.That(obtained, Is.EqualTo(false));
        }

        [Test]
        public void InstancesWithDifferentBeaconIdHaveDifferentHash()
        {
            // given
            var keyOne = new BeaconKey(37, 18);
            var keyTwo = new BeaconKey(38, 18);

            // when, then
            Assert.That(keyOne.GetHashCode(), Is.Not.EqualTo(keyTwo.GetHashCode()));
        }

        [Test]
        public void InstancesWithDifferentBeaconSeqNoAreNotEqual()
        {
            // given
            var keyOne = new BeaconKey(17, 37);
            var keyTwo = new BeaconKey(17, 73);

            // when
            var obtained = keyOne.Equals(keyTwo);

            // then
            Assert.That(obtained, Is.EqualTo(false));
        }

        [Test]
        public void InstancesWithDifferentBeaconSeqNoHaveDifferentHash()
        {
            // given
            var keyOne = new BeaconKey(17, 37);
            var keyTwo = new BeaconKey(17, 73);

            // when, then
            Assert.That(keyOne.GetHashCode(), Is.Not.EqualTo(keyTwo.GetHashCode()));
        }
    }
}