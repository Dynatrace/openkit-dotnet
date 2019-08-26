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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dynatrace.OpenKit.Util.Json.Objects;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Util.Json.Objects
{
    public class JsonArrayValueTest
    {
        private static readonly ICollection<JsonValue> EmptyList =
            new ReadOnlyCollection<JsonValue>(new List<JsonValue>());

        [Test]
        public void JsonValueTypeIsArray()
        {
            // when, then
            Assert.That(JsonArrayValue.FromList(EmptyList).ValueType, Is.EqualTo(JsonValueType.ARRAY));
        }

        [Test]
        public void FromListGivesNullIfArgumentIsNull()
        {
            // when null, then
            Assert.That(JsonArrayValue.FromList(null), Is.Null);
        }

        [Test]
        public void CountDelegatesCallToUnderlyingList()
        {
            // given
            var jsonValues = Substitute.For<ICollection<JsonValue>>();
            jsonValues.Count.Returns(42);
            var target = JsonArrayValue.FromList(jsonValues);

            // when
            var obtained = target.Count;

            // then
            Assert.That(obtained, Is.EqualTo(42));
            _ = jsonValues.Received(1).Count;
        }

      [Test]
        public void GetEnumeratorDelegatesTheCallToTheUnderlyingList()
        {
            // given
            var enumerator = Substitute.For<IEnumerator<JsonValue>>();
            var jsonValues = Substitute.For<ICollection<JsonValue>>();
            var jsonValuesEnumerable = ((IEnumerable<JsonValue>) jsonValues);
            jsonValuesEnumerable.GetEnumerator().Returns(enumerator);

            IEnumerable<JsonValue> target = JsonArrayValue.FromList(jsonValues);

            // when
            var obtained = target.GetEnumerator();

            //then
            Assert.That(obtained, Is.Not.Null);
            using (jsonValues.Received(1).GetEnumerator()) {}
        }
    }
}