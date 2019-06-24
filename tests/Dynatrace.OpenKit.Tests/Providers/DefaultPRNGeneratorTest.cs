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

namespace Dynatrace.OpenKit.Providers
{
    public class DefaultSessionPRNGeneratorTest
    {
        [Test]
        public void DefaultPRNGeneratorProvidesPositiveInt()
        {
            // given
            var randomGenerator = new DefaultPRNGenerator();

            //when
            var randomNumber = randomGenerator.NextInt(int.MaxValue);

            // then
            Assert.That(randomNumber, Is.GreaterThanOrEqualTo(0));
            Assert.That(randomNumber, Is.LessThan(int.MaxValue));
        }

        [Test]
        public void DefaultPRNGeneratorProvidesPositiveLong()
        {
            // given
            var randomGenerator = new DefaultPRNGenerator();

            //when
            var randomNumber = randomGenerator.NextLong(long.MaxValue);

            // then
            Assert.That(randomNumber, Is.GreaterThanOrEqualTo(0));
            Assert.That(randomNumber, Is.LessThan(long.MaxValue));
        }
    }
}
