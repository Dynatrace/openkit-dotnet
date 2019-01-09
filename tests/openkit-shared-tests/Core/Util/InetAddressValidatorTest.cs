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

using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Util
{
    [TestFixture]
    public class InetAddressValidatorTest
    {
        [Test]
        public void IPV4AddressIsValid()
        {
            //given
            var validIPV4Address = "122.133.55.22";

            //then
            Assert.That(InetAddressValidator.IsValidIP(validIPV4Address), Is.True);
        }

        [Test]
        public void IPV4AddressIsInvalidDueToExtraBlock()
        {
            //given
            var invalidIPV4Address = "122.133.55.22.1";

            //then
            Assert.That(InetAddressValidator.IsValidIP(invalidIPV4Address), Is.False);
        }

        [Test]
        public void IPV4AddressIsInvalidDueToHighNumber()
        {
            //given
            var invalidIPV4Address = "122.133.555.22";

            //then
            Assert.That(InetAddressValidator.IsValidIP(invalidIPV4Address), Is.False);
        }

        [Test]
        public void IPV4AddressIsInvalidDueToNegativeNumber()
        {
            //given
            var invalidIPV4Address = "122.133.555.-22";

            //then
            Assert.That(InetAddressValidator.IsValidIP(invalidIPV4Address), Is.False);
        }

        [Test]
        public void IPV4AddressIsInvalidDueToLetter()
        {
            //given
            var invalidIPV4Address = "122.133.555.e33";

            //then
            Assert.That(InetAddressValidator.IsValidIP(invalidIPV4Address), Is.False);
        }


        [Test]
        public void IPV4AddressIsInvalidDueToMissingBlock()
        {
            //given
            var invalidIPV4Address = "122.133.555";

            //then
            Assert.That(InetAddressValidator.IsValidIP(invalidIPV4Address), Is.False);
        }

        [Test]
        public void IPV6AddressIsValid()
        {
            //given
            var validIPV6Address = "2045:FEFE:0D22:0123:DAD2:3345:ABB2:0003";

            //then
            Assert.That(InetAddressValidator.IsValidIP(validIPV6Address), Is.True);
        }

        [Test]
        public void IPV6AddressIsInvalidDueToExtraBlock()
        {
            //given
            var invalidIPV6Address = "2045:FEFE:0D22:0123:DAD2:3345:ABB2:0003:1001";

            //then
            Assert.That(InetAddressValidator.IsValidIP(invalidIPV6Address), Is.False);
        }

        [Test]
        public void IPV6AddressIsInvalidDueTo5HexDigitNumber()
        {
            //given
            var invalidIPV6Address = "2045:FEFE3:0D22:0123:DAD2:3345:ABB2:0003";

            //then
            Assert.That(InetAddressValidator.IsValidIP(invalidIPV6Address), Is.False);
        }

        [Test]
        public void IPV6AddressIsInvalidDueToNonHexLetter()
        {
            //given
            var invalidIPV6Address = "2045:GEFE:0D22:0123:DAD2:3345:ABB2:0003";

            //then
            Assert.That(InetAddressValidator.IsValidIP(invalidIPV6Address), Is.False);
        }

        [Test]
        public void IPV6AddressWithLessThanEightBlocksIsValid()
        {
            //given
            var validIPV6Address = "2045:defe:d22:123::1444";

            //then
            Assert.That(InetAddressValidator.IsValidIP(validIPV6Address), Is.True);
        }

        [Test]
        public void IPAddressEmptyIsInvalid()
        {
            //given
            var invalidIPAddress = "";

            //then
            Assert.That(InetAddressValidator.IsValidIP(invalidIPAddress), Is.False);
        }

        [Test]
        public void IPAddressNullIsInvalid()
        {
            //given
            string invalidIPAddress = null;

            //then
            Assert.That(InetAddressValidator.IsValidIP(invalidIPAddress), Is.False);
        }

    }
}
