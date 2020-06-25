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
using System.Globalization;

namespace Dynatrace.OpenKit.Util
{
    public class ToStringExtensionsTest
    {
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
        private CultureInfo currentCulture;
#endif

        [SetUp]
        public void Setup()
        {
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
            // Note: .NET Core 1.0/1.1 does not allow manipulating the thread's culture
            // Manipulating the culture for all threads might have negative impact
            var newCulture = new CultureInfo("de-AT");
            // use tilde for negative numbers
            newCulture.NumberFormat.NegativeSign = "~";
            // make groups after every digits
            newCulture.NumberFormat.NumberGroupSizes = new int[1] { 1 };
            newCulture.NumberFormat.NumberGroupSeparator = "_";
            currentCulture = CultureInfo.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = newCulture;
#endif
        }

        [TearDown]
        public void TearDown()
        {
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
#endif
        }

        [Test]
        public void ShortToInvariantString()
        {
            // given
            const short positiveShort = 4242;
            const short negativeShort = -1777;

            // when, then
            Assert.That(positiveShort.ToInvariantString(), Is.EqualTo("4242"));
            Assert.That(negativeShort.ToInvariantString(), Is.EqualTo("-1777"));
        }

        [Test]
        public void UShortToInvariantString()
        {
            // given
            const ushort unsignedShort = 12345;

            // when, then
            Assert.That(unsignedShort.ToInvariantString(), Is.EqualTo("12345"));
        }

        [Test]
        public void IntToInvariantString()
        {
            // given
            const int positiveInt = 12345;
            const int negativeInt = -54321;

            // when, then
            Assert.That(positiveInt.ToInvariantString(), Is.EqualTo("12345"));
            Assert.That(negativeInt.ToInvariantString(), Is.EqualTo("-54321"));
        }

        [Test]
        public void UIntToInvariantString()
        {
            // given
            const uint unsignedInt = 123456789;

            // when, then
            Assert.That(unsignedInt.ToInvariantString(), Is.EqualTo("123456789"));
        }

        [Test]
        public void LongToInvariantString()
        {
            // given
            const long positiveLong = 9876543210;
            const long negativeLong = -9876543210;

            // when, then
            Assert.That(positiveLong.ToInvariantString(), Is.EqualTo("9876543210"));
            Assert.That(negativeLong.ToInvariantString(), Is.EqualTo("-9876543210"));
        }

        [Test]
        public void ULongToInvariantString()
        {
            // given
            const ulong unsignedLong = 1234512345;

            // when, then
            Assert.That(unsignedLong.ToInvariantString(), Is.EqualTo("1234512345"));
        }

        [Test]
        public void FloatToInvariantString()
        {
            // given
            const float positiveFloat = 1.570796F;
            const float negativeFloat = -2.71828F;

            // when, then
            Assert.That(positiveFloat.ToInvariantString(), Is.EqualTo("1.570796"));
            Assert.That(negativeFloat.ToInvariantString(), Is.EqualTo("-2.71828"));
        }

        [Test]
        public void DoubleToInvariantString()
        {
            // given
            const double positiveDouble = 2.7182818284;
            const double negativeDouble = -3.14159265359;

            // when, then
            Assert.That(positiveDouble.ToInvariantString(), Is.EqualTo("2.7182818284"));
            Assert.That(negativeDouble.ToInvariantString(), Is.EqualTo("-3.14159265359"));
        }
    }
}