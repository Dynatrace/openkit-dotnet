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

using System;
using Dynatrace.OpenKit.Util.Json.Objects;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Util.Json.Objects
{
    public class JsonNumberValueTest
    {
        [Test]
        public void IsNumberType()
        {
            // then
            Assert.That(JsonNumberValue.FromLong(0).ValueType, Is.EqualTo(JsonValueType.NUMBER));
        }

        [Test]
        public void IsIntegerWhenConstructingWithLong()
        {
            // given
            var target = JsonNumberValue.FromLong(42);

            // then
            Assert.That(target.IsInteger, Is.True);
        }

        [Test]
        public void IsIntValueReturnsTrueIfIsInsideIntRange()
        {
            // when minimum int value is used
            Assert.That(JsonNumberValue.FromLong(int.MinValue).IsIntValue, Is.True);

            // when maximum int value is used
            Assert.That(JsonNumberValue.FromLong(int.MaxValue).IsIntValue, Is.True);
        }

        [Test]
        public void IsIntValueReturnsFalseIfOutsideOfIntRange()
        {
            // when one less then minimum int value
            Assert.That(JsonNumberValue.FromLong(int.MinValue - 1L).IsIntValue, Is.False);

            // when one more then maximum int value
            Assert.That(JsonNumberValue.FromLong(int.MaxValue + 1L).IsIntValue, Is.False);
        }

        [Test]
        public void IsIntValueReturnsFalseIfFromFloatingPointValue()
        {
            // when fit into integer value
            Assert.That(JsonNumberValue.FromDouble(42).IsIntValue, Is.False);

            // when an actual floating point number
            Assert.That(JsonNumberValue.FromDouble(3.14159).IsIntValue, Is.False);
        }

        [Test]
        public void IntValueReturns32BitIntValue()
        {
            // when constructed from a long fitting into an int
            Assert.That(JsonNumberValue.FromLong(int.MinValue).IntValue, Is.EqualTo(int.MinValue));
        }

        [Test]
        public void IntValueReturnsCastedValueFromLong()
        {
            // given
            const int expected = unchecked((int)0xdeadbabe);

            // when constructed from a that does not fit into 32-bit
            Assert.That(JsonNumberValue.FromLong(0x11111111deadbabe).IntValue, Is.EqualTo(expected));
        }

        [Test]
        public void IntValueReturnsCastedValueFromDouble()
        {
            // when constructed from a double with fractional part
            Assert.That(JsonNumberValue.FromDouble(Math.PI).IntValue, Is.EqualTo(3));

            // when constructed from another double with fractional part
            Assert.That(JsonNumberValue.FromDouble(Math.E).IntValue, Is.EqualTo(2));
        }

        [Test]
        public void LongValueReturnsValueWhenConstructedFromLong()
        {
            // when
            Assert.That(JsonNumberValue.FromLong(-77129852519530769L).LongValue, Is.EqualTo(-77129852519530769L));
        }

        [Test]
        public void LongValueReturnsTruncatedValueWhenConstructedFromDouble()
        {
            // when constructed from a double with fractional part
            Assert.That(JsonNumberValue.FromDouble(Math.PI).LongValue, Is.EqualTo(3));

            // when constructed from another double with fractional part
            Assert.That(JsonNumberValue.FromDouble(Math.E).LongValue, Is.EqualTo(2));
        }

        [Test]
        public void FloatValueReturns32BitFloatingPointValue()
        {
            // when
            Assert.That(JsonNumberValue.FromDouble(Math.PI).FloatValue, Is.EqualTo(3.1415927410F));
        }

        [Test]
        public void DoubleValueReturns64BitFloatingPointValue()
        {
            // when
            Assert.That(JsonNumberValue.FromDouble(Math.PI).DoubleValue, Is.EqualTo(Math.PI));

            Assert.That(JsonNumberValue.FromDouble(Math.E).DoubleValue, Is.EqualTo(Math.E));
        }

        [Test]
        public void FromNumberLiteralReturnsNullIfLiteralIsNull()
        {
            // when
            Assert.That(JsonNumberValue.FromNumberLiteral(null), Is.Null);
        }

        [Test]
        public void FromNumberLiteralReturnsNullIfLiteralIsInvalid()
        {
            // when constructed with empty string
            Assert.That(JsonNumberValue.FromNumberLiteral(""), Is.Null);

            // when constructed with arbitrary string
            Assert.That(JsonNumberValue.FromNumberLiteral("foobar"), Is.Null);

            // when constructed with alpha-numeric string
            Assert.That(JsonNumberValue.FromNumberLiteral("1234foo"), Is.Null);
        }

        [Test]
        public void FromNumberLiteralReturnsIntegerRepresentationForIntegerNumbers()
        {
            // when constructed with positive integer literal
            var obtained = JsonNumberValue.FromNumberLiteral("1234567890");

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsInteger, Is.True);
            Assert.That(obtained.LongValue, Is.EqualTo(1234567890));
        }

        [Test]
        public void FromNumberLiteralReturnsNullForNotParsableLong()
        {
            // when constructed from a positive integer literal
            var obtained = JsonNumberValue.FromNumberLiteral("9223372036854775808");

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void FromNumberLiteralReturnsDoubleIfLiteralContainsFractionPart()
        {
            // when constructed from positive floating point literal
            var obtained = JsonNumberValue.FromNumberLiteral("1.25");

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsInteger, Is.False);
            Assert.That(obtained.DoubleValue, Is.EqualTo(1.25));
        }

        [Test]
        public void FromNumberLiteralReturnsDoubleIfLiteralContainsExponentPart()
        {
            // when constructed from positive floating point literal
            var obtained = JsonNumberValue.FromNumberLiteral("15E-1");

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsInteger, Is.False);
            Assert.That(obtained.DoubleValue, Is.EqualTo(1.5));
        }

        [Test]
        public void FromNumberLiteralReturnsDoubleIfLiteralContainsFractionAndExponentPart()
        {
            // when
            var obtained = JsonNumberValue.FromNumberLiteral("0.0625e+2");

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsInteger, Is.False);
            Assert.That(obtained.DoubleValue, Is.EqualTo(6.25d));
        }

        [Test]
        public void FromNumberLiteralReturnsLongIfLiteralContainsNegativeValue()
        {
            // when constructed from a negative integer literal
            var obtained = JsonNumberValue.FromNumberLiteral("-1234567890");

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsInteger, Is.True);
            Assert.That(obtained.LongValue, Is.EqualTo(-1234567890));
        }

        [Test]
        public void FromNumberLiteralReturnsDoubleIfLiteralContainsNegativeValue()
        {
            // when constructed from a negative floating point literal
            var obtained = JsonNumberValue.FromNumberLiteral("-1.25");

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsInteger, Is.False);
            Assert.That(obtained.DoubleValue, Is.EqualTo(-1.25));
        }
    }
}