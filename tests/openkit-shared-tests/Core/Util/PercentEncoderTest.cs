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
using System.Text;

namespace Dynatrace.OpenKit.Core.Util
{
    [TestFixture]
    class PercentEncoderTest
    {
        /// <summary>
        /// All unreserved characters based on RFC-3986
        /// </summary>
        private const string UnreservedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~";
        
        [Test]
        public void RFC3986UnreservedCharactersAreNotEncoded()
        {
            // when
            var obtained = PercentEncoder.Encode(UnreservedCharacters, Encoding.UTF8);

            // then
            Assert.That(obtained, Is.EqualTo(UnreservedCharacters));
        }

        [Test]
        public void ReservedCharactersArePercentEncoded()
        {
            // when
            var obtained = PercentEncoder.Encode("+()/\\&%$#@!`?<>[]{}", Encoding.UTF8);

            // then
            var expected = "%2B%28%29%2F%5C%26%25%24%23%40%21%60%3F%3C%3E%5B%5D%7B%7D"; // precomputed using Python
            Assert.That(obtained, Is.EqualTo(expected));
        }

        [Test]
        public void MixingReservedAndUnreservedCharactersWorks()
        {
            // when
            var obtained = PercentEncoder.Encode("a+bc()~/\\&0_", Encoding.UTF8);

            // then
            var expected = "a%2Bbc%28%29~%2F%5C%260_"; // precomputed using Python
            Assert.That(obtained, Is.EqualTo(expected));
        }

        [Test]
        public void CharactersOutsideOfAsciiRangeAreEncodedFirst()
        {
            // when
            var obtained = PercentEncoder.Encode("aösÖ€dÁF", Encoding.UTF8);

            // then
            var expected = "a%C3%B6s%C3%96%E2%82%ACd%C3%81F";
            Assert.That(obtained, Is.EqualTo(expected));
        }

        [Test]
        public void ItIsPossibleToMarkAdditionalCharactersAsReserved()
        {
            // when
            var additionalReservedCharacters = new char[] { '€', '0', '_' };
            var obtained = PercentEncoder.Encode("0123456789-._~", Encoding.UTF8, additionalReservedCharacters);

            // then
            var expected = "%30123456789-.%5F~";
            Assert.That(obtained, Is.EqualTo(expected));
        }

        [Test]
        public void NullIsReturnedIfNullIsPassedIn()
        {
            // when
            var obtained = PercentEncoder.Encode(null, Encoding.ASCII);

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void EmptyStringIsHandledSeparately()
        {
            // when
            var obtained = PercentEncoder.Encode(string.Empty, Encoding.ASCII);

            // then
            Assert.That(obtained, Is.EqualTo(string.Empty));
        }
    }
}
