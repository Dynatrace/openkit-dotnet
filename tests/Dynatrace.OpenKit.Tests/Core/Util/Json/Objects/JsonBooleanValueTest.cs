//
// Copyright 2018-2021 Dynatrace LLC
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

using Dynatrace.OpenKit.Util.Json.Objects;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Util.Json.Objects
{
    public class JsonBooleanValueTest
    {
        [Test]
        public void IsBooleanType()
        {
            Assert.That(JsonBooleanValue.FromValue(true).ValueType, Is.EqualTo(JsonValueType.BOOLEAN));
        }

        [Test]
        public void ValueRepresentsAppropriateBooleanValue()
        {
            Assert.That(JsonBooleanValue.True.Value, Is.True);
            Assert.That(JsonBooleanValue.False.Value, Is.False);
        }

        [Test]
        public void FromValueReturnsTrueSingletonInstance()
        {
            // when
            var obtained = JsonBooleanValue.FromValue(true);

            // then
            Assert.That(obtained, Is.SameAs(JsonBooleanValue.True));
        }

        [Test]
        public void FromValueReturnsFalseSingletonInstance()
        {
            // when
            var obtained = JsonBooleanValue.FromValue(false);

            // then
            Assert.That(obtained, Is.SameAs(JsonBooleanValue.False));
        }

        [Test]
        public void FromLiteralReturnsNullIfLiteralNull()
        {
            // then
            Assert.That(JsonBooleanValue.FromLiteral(null), Is.Null);
        }

        [Test]
        public void FromLiteralReturnsTrueSingletonInstance()
        {
            // when
            var obtained = JsonBooleanValue.FromLiteral("true");

            // then
            Assert.That(obtained, Is.SameAs(JsonBooleanValue.True));
        }

        [Test]
        public void FromLiteralReturnsFalseSingletonInstance()
        {
            // when
            var obtained = JsonBooleanValue.FromLiteral("false");

            // then
            Assert.That(obtained, Is.SameAs(JsonBooleanValue.False));
        }

        [Test]
        public void FromLiteralReturnsNullForNonBooleanLiterals()
        {
            // when invalid case is used
            Assert.That(JsonBooleanValue.FromLiteral("TRUE"), Is.Null);
            Assert.That(JsonBooleanValue.FromLiteral("FALSE"), Is.Null);

            // when invalid literal strings is used
            Assert.That(JsonBooleanValue.FromLiteral("1234"), Is.Null);

            // when an empty string is passed
            Assert.That(JsonBooleanValue.FromLiteral(""), Is.Null);
        }
    }
}