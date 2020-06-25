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
    [TestFixture]
    public class InetAddressValidatorTest
    {
        [Test]
        public void IpV4AddressIsValid()
        {
            //given
            const string ipv4TestString = "122.133.55.22";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv4TestString), Is.True);
        }

        [Test]
        public void IpV4AddressIsValidAllZero()
        {
            //given
            const string ipv4TestString = "0.0.0.0";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv4TestString), Is.True);
        }

        [Test]
        public void IpV4AddressIsValidAllEight()
        {
            //given
            const string ipv4TestString = "8.8.8.8";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv4TestString), Is.True);
        }

        [Test]
        public void IpV4AddressIsValidHighestPossible()
        {
            //given
            const string ipv4TestString = "255.255.255.255";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv4TestString), Is.True);
        }

        [Test]
        public void IpV4AddressIsInvalidBecauseOfOverflow()
        {
            //given
            const string ipv4TestString = "255.255.255.256";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv4TestString), Is.False);
        }

        [Test]
        public void IpV4AddressIsInvalidDoubleColonsInsteadOfPoints()
        {
            //given
            const string ipv4TestString = "255:255:255:255";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv4TestString), Is.False);
        }

        [Test]
        public void IpV4AddressIsInvalidDueToAdditionalCharacterInFirstBlock()
        {
            //given
            const string ipv4TestString = "122x.133.55.22";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv4TestString), Is.False);
        }

        [Test]
        public void IpV4AddressIsInvalidDueToAdditionalCharacterInSecondBlock()
        {
            //given
            const string ipv4TestString = "122.133x.55.22";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv4TestString), Is.False);
        }

        [Test]
        public void IpV4AddressIsInvalidDueToAdditionalCharacterInThirdBlock()
        {
            //given
            const string ipv4TestString = "122.133.55x.22";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv4TestString), Is.False);
        }

        [Test]
        public void IpV4AddressIsInvalidDueToAdditionalCharacterInFourthBlock()
        {
            //given
            const string ipv4TestString = "122.133.55.22x";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv4TestString), Is.False);
        }

        [Test]
        public void IpV4AddressIsInvalidDueToIllegalValueOverrun()
        {
            //given
            const string ipv4TestString = "122.133.256.22";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv4TestString), Is.False);
        }

        [Test]
        public void IpV4AddressIsInvalidDueToIllegalValueNegative()
        {
            //given
            const string ipv4TestString = "122.133.256.-22";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv4TestString), Is.False);
        }

        [Test]
        public void IpV6AddressIsValid()
        {
            //given
            const string ipv6TestString = "23fe:33af:1232:5522:abcd:2532:1a2b:1";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.True);
        }

        [Test]
        public void IpV6AddressIsInvalidOverflow()
        {
            //given
            const string ipv6TestString = "23fec:33af:1232:5522:abcd:2532:1a2b:1";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.False);
        }

        [Test]
        public void IpV6AddressIsInvalidIllegalCharacter()
        {
            //given
            const string ipv6TestString = "23fl:33af:1232:5522:abcd:2532:1a2b:1";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.False);
        }

        [Test]
        public void IpV6AddressIsInvalidTooManyBlocks()
        {
            //given
            const string ipv6TestString = "23fl:33af:1232:5522:abcd:2532:1a2b:1:2:3";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.False);
        }

        [Test]
        public void IpV6AddressHexCompressedIsValidBlock4()
        {
            //given
            const string ipv6TestString = "2001:db:85:b::1A";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.True);
        }

        [Test]
        public void IpV6AddressHexCompressedIsValidBlock3()
        {
            //given
            const string ipv6TestString = "2001:db:85::b:1A";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.True);
        }

        [Test]
        public void IpV6AddressHexCompressedIsValidBlock2()
        {
            //given
            const string ipv6TestString = "2001:db::85:b:1A";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.True);
        }

        [Test]
        public void IpV6AddressHexCompressedIsValidBlock1()
        {
            //given
            const string ipv6TestString = "2001::db:85:b:1A";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.True);
        }

        [Test]
        public void IpV6AddressHexCompressedIsValidShortestPossible()
        {
            //given
            const string ipv6TestString = "2001::b1A";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.True);
        }

        [Test]
        public void IpV6AddressHexCompressedIsInvalidTwoCompressedBlocks()
        {
            //given
            const string ipv6TestString = "2001::db:85::b1A";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.False);
        }

        [Test]
        public void IpV6AddressHexCompressedIsInvalidFirstBlockMissing()
        {
            //given
            const string ipv6TestString = ":4::5:6";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.False);
        }

        [Test]
        public void IpV6AddressMixedNotationIsValid_ZerosIPv6NonCompressed()
        {
            //given
            const string ipv6MixedTestString = "0:0:0:0:0:0:172.12.55.18";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6MixedTestString), Is.True);
        }

        [Test]
        public void IpV6AddressMixedNotationIsValid_ZerosIPv6Compressed()
        {
            //given
            const string ipv6MixedTestString = "::172.12.55.18";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6MixedTestString), Is.True);
        }

        [Test]
        public void IpV6AddressMixedNotationIsValid_NonZeroIPv6NonCompressed()
        {
            //given
            const string ipv6MixedTestString = "1:2:3:4:5:6:172.12.55.18";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6MixedTestString), Is.True);
        }

        [Test]
        public void IpV6AddressMixedNotationIsValid_NonZeroIPv6Compressed()
        {
            //given
            const string ipv6MixedTestString = "2018:f::172.12.55.18";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6MixedTestString), Is.True);
        }

        [Test]
        public void IpV6AddressMixedNotationIsInvalidOnly3IPv4Blocks()
        {
            //given
            const string ipv6TestString = "0::FF:FF:172.12.55";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.False);
        }

        [Test]
        public void IpV6AddressMixedNotationIsValidIpV6PartInvalid()
        {
            //given
            const string ipv6TestString = "0::FF::FF:172.12.55.34";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.False);
        }

        [Test]
        public void IpV6AddressMixedNotationIsValidIpV6()
        {
            //given
            const string ipv6TestString = "0::FF:FF:FF:172.12.55.34";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.True);
        }

        [Test]
        public void IpV6AddressMixedNotationIsValidStartingWithDoubleColon()
        {
            //given
            const string ipv6TestString = "::FF:FF:172.12.55.43";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.True);
        }

        [Test]
        public void IpV6AddressMixedNotationInvalid_Compressed3Colon()
        {
            //given
            const string ipv6TestString = "123:::172.12.55.43";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.False);
        }

        [Test]
        public void IpV6AddressLinkLocalIsValid()
        {
            //given
            const string ipv6TestStringLinkLocal = "fe80::208:74ff:feda:625c%5";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestStringLinkLocal), Is.True);
        }

        [Test]
        public void IpV6AddressLinkLocalIsValidVeryShortLinkLocal()
        {
            //given
            const string ipv6TestStringLinkLocal = "fe80::625c%5";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestStringLinkLocal), Is.True);
        }

        [Test]
        public void IpV6AddressLinkLocalIsInvalidTooManyBlocks()
        {
            //given
            const string ipv6TestStringLinkLocal = "fe80:34:208:74ff:feda:dada:625c:8976:abcd%5";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestStringLinkLocal), Is.False);
        }

        [Test]
        public void IpV6AddressLinkLocalIsInvalidIllegalNonHexCharacter()
        {
            //given
            const string ipv6TestStringLinkLocal = "fe80::208t:74ff:feda:dada:625c%5";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestStringLinkLocal), Is.False);
        }

        [Test]
        public void IpV6AddressLinkLocalIsInvalidDueToTwoDoubleColonsInAddress()
        {
            //given
            const string ipv6TestStringLinkLocal = "fe80::208:74ff::dada:625c%5";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestStringLinkLocal), Is.False);
        }

        [Test]
        public void IpV6AddressLinkLocalIsInvalidZoneIndexUsedWithInvalidPrefix()
        {
            //given
            const string ipv6TestStringLinkLocal = "fedd::208:74ff::dada:625c%5";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestStringLinkLocal), Is.False);
        }

        // the following two addresses are not valid according to RFC5952 but are accepted by glibc's implementation and also ours

        [Test]
        public void IpV6AddressValid_RFCLeadingZeros()
        {
            //given
            const string ipv6TestString = "2001:0db8::0001";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.True);
        }

        [Test]
        public void IpV6AddressValid_RFCEmptyBlockNotShortened()
        {
            //given
            const string ipv6TestString = "2001:db8::0:1";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.True);
        }

        [Test]
        public void IpV6AddressValid_RFCExample()
        {
            //given
            const string ipv6TestString = "2001:db8::1:0:0:1";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.True);
        }

        [Test]
        public void IpV6AddressValid_CharactersOnlyLowerCase()
        {
            //given
            var ipv6TestString = "20ae:db8::1f:4edd:344f:1abc";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.True);
        }

        [Test]
        public void IpV6AddressValid_CharactersMixedCase()
        {
            //given
            const string ipv6TestString = "20aE:Db8::1f:4EDd:344f:1aBc";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.True);
        }

        [Test]
        public void IpV6AddressValid_CharactersUpperCase()
        {
            //given
            var ipv6TestString = "20AE:DB8::1F:4EDD:344F:1ABC";

            //then
            Assert.That(InetAddressValidator.IsValidIP(ipv6TestString), Is.True);
        }

        [Test]
        public void IpV6AddressMappedWithValidIpV4Address()
        {
            // given
            const string ipAddress = "::ffff:127.0.0.1";

            // when
            var obtained = InetAddressValidator.IsValidIP(ipAddress);

            // then
            Assert.That(obtained, Is.True);
        }

        [Test]
        public void IpV6AddressMappedWithInvalidIpV4Address()
        {
            // given
            const string ipAddress = "::ffff:127.0.0.300";

            // when
            var obtained = InetAddressValidator.IsValidIP(ipAddress);

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void NullIpAddressIsInvalid()
        {
            // given
            const string ipAddress = null;

            // when
            var obtained = InetAddressValidator.IsValidIP(ipAddress);

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void EmptyIpAddressIsInvalid()
        {
            // given
            const string ipAddress = "";

            // when
            var obtained = InetAddressValidator.IsValidIP(ipAddress);

            // then
            Assert.That(obtained, Is.False);
        }
    }
}
