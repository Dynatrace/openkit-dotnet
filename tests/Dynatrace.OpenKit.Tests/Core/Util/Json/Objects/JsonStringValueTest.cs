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
    public class JsonStringValueTest
    {
        [Test]
        public void IsStringType()
        {
            Assert.That(JsonStringValue.FromString("").ValueType, Is.EqualTo(JsonValueType.STRING));
        }

        [Test]
        public void FromStringReturnsNullIfArgumentIsNull()
        {
            Assert.That(JsonStringValue.FromString(null), Is.Null);
        }

        [Test]
        public void ValueReturnsValueOfFromStringMethod()
        {
            Assert.That(JsonStringValue.FromString("").Value, Is.EqualTo(""));
            Assert.That(JsonStringValue.FromString("a").Value, Is.EqualTo("a"));
            Assert.That(JsonStringValue.FromString("foobar").Value, Is.EqualTo("foobar"));
        }

        [Test]
        public void EmptyValueJsonString()
        {
            Assert.That(JsonStringValue.FromString("").ToString(), Is.EqualTo("\"\""));
        }

        [Test]
        public void AnyValueJsonString()
        {
            Assert.That(JsonStringValue.FromString("Test").ToString(), Is.EqualTo("\"Test\""));
        }
    }
}