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

namespace Dynatrace.OpenKit.Core.Util
{
    public class StringUtilTest
    {
        [Test]
        public void HashNullString()
        {
            // given, when 
            var hash = StringUtil.To64BitHash(null);

            // then
            Assert.That(hash, Is.EqualTo(0));
        }

        [Test]
        public void HashEmptyString()
        {
            // given
            var emptString = string.Empty;

            // when
            var hash = StringUtil.To64BitHash(emptString);

            //then
            Assert.That(hash, Is.EqualTo(0L));
        }

        [Test]
        public void DifferentStringsDifferentHash()
        {
            // given
            var firstString = "some string";
            var secondString = "some other string";

            // when
            var firstHash = StringUtil.To64BitHash(firstString);
            var secondHash = StringUtil.To64BitHash(secondString);

            //then
            Assert.That(firstHash, Is.Not.EqualTo(secondHash));
        }

        [Test]
        public void EqualStringSameHash()
        {
            // given
            var firstString = "str";
            var secondString = new string(new char[] { 's', 't', 'r' });

            // when
            var firstHash = StringUtil.To64BitHash(firstString);
            var secondHash = StringUtil.To64BitHash(secondString);

            // then
            Assert.AreNotSame(firstString, secondString);
            Assert.That(firstString, Is.EqualTo(secondString));
            Assert.That(firstHash, Is.EqualTo(secondHash));
        }

        [Test]
        public void CaseSensitiveStringsDifferentHash()
        {
            // given
            var lowerCase = "value";
            var upperCase = "Value";

            // when
            var lowerCaseHash = StringUtil.To64BitHash(lowerCase);
            var upperCaseHash = StringUtil.To64BitHash(upperCase);

            //
            Assert.That(lowerCaseHash, Is.Not.EqualTo(upperCaseHash));
        }

        [Test]
        public void HashStringOfEvenLength()
        {
            // given
            var evenLengthString = "even";

            // when
            var hash = StringUtil.To64BitHash(evenLengthString);

            // then
            Assert.That(hash, Is.GreaterThan(0));
        }

        [Test]
        public void HashStringOfOddLength()
        {
            // given
            var oddLengthString = "odd";

            // when
            var hash = StringUtil.To64BitHash(oddLengthString);

            // then
            Assert.That(hash, Is.GreaterThan(0));
        }
    }
}