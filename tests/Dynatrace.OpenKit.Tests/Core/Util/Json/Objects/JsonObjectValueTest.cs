﻿//
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

using System.Collections.Generic;
using System.Text;
using Dynatrace.OpenKit.Util.Json.Objects;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Util.Json.Objects
{
    public class JsonObjectValueTest
    {
        [Test]
        public void IsObjectType()
        {
            // given
            var emptyDict = new Dictionary<string, JsonValue>();

            // then
            Assert.That(JsonObjectValue.FromDictionary(emptyDict).ValueType, Is.EqualTo(JsonValueType.OBJECT));
        }

        [Test]
        public void FromDictionaryReturnsNullIfArgumentIsNull()
        {
            // when constructed with null
            Assert.That(JsonObjectValue.FromDictionary(null), Is.Null);
        }

        [Test]
        public void KeysDelegatesToUnderlyingDictionary()
        {
            //given
            var jsonObjectDict = Substitute.For<IDictionary<string, JsonValue>>();
            ICollection<string> keys = new HashSet<string> {"foobar"};
            jsonObjectDict.Keys.Returns(keys);
            var target = JsonObjectValue.FromDictionary(jsonObjectDict);

            // when
            var obtained = target.Keys;

            // then
            Assert.That(obtained, Is.Not.Null);
            _ = jsonObjectDict.Received(1).Keys;
        }

        [Test]
        public void CountDelegatesToUnderlyingDictionary()
        {
            // given
            var jsonObjectDict = Substitute.For<IDictionary<string, JsonValue>>();
            jsonObjectDict.Count.Returns(42);
            var target = JsonObjectValue.FromDictionary(jsonObjectDict);

            // when
            var obtained = target.Count;

            // then
            Assert.That(obtained, Is.EqualTo(42));
            _ = jsonObjectDict.Received(1).Count;
        }

        [Test]
        public void ContainsDelegatesToUnderlyingDictionary()
        {
            // given
            var jsonObjectDict = Substitute.For<IDictionary<string, JsonValue>>();
            jsonObjectDict.ContainsKey(Arg.Any<string>()).Returns(true);
            var target = JsonObjectValue.FromDictionary(jsonObjectDict);

            // when
            var obtained = target.ContainsKey("foo");

            // then
            Assert.That(obtained, Is.True);
            _ = jsonObjectDict.Received(1).ContainsKey("foo");

            // and when
            obtained = target.ContainsKey("bar");

            // then
            Assert.That(obtained, Is.True);
            _ = jsonObjectDict.Received(1).ContainsKey("bar");
        }

        [Test]
        public void IndexerDelegatesToUnderlyingDictionary()
        {
            // given
            var valueOne = Substitute.For<JsonValue>();
            var valueTwo = Substitute.For<JsonValue>();

            var jsonObjectDict = new Dictionary<string, JsonValue> {["foo"] = valueOne, ["bar"] = valueTwo};

            var target = JsonObjectValue.FromDictionary(jsonObjectDict);

            // when
            var obtained = target["foo"];

            // then
            Assert.That(obtained, Is.Not.Null.And.SameAs(valueOne));

            // and when
            obtained = target["bar"];

            // then
            Assert.That(obtained, Is.Not.Null.And.SameAs(valueTwo));
        }

        [Test]
        public void IndexerReturnsNullIfKeyDoesNotExist()
        {
            // given
            var jsonObjectDict = new Dictionary<string, JsonValue>();

            var target = JsonObjectValue.FromDictionary(jsonObjectDict);

            // when
            var obtained = target["foo"];

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void EmptyObjectJsonString()
        {
            // given
            var jsonObjectDict = new Dictionary<string, JsonValue>();

            Assert.That(JsonObjectValue.FromDictionary(jsonObjectDict).ToString(), Is.EqualTo("{}"));
        }

        [Test]
        public void SingleElementObjectJsonString()
        {
            // given
            var jsonObjectDict = new Dictionary<string, JsonValue>();
            jsonObjectDict.Add("Test", JsonBooleanValue.FromValue(false));

            Assert.That(JsonObjectValue.FromDictionary(jsonObjectDict).ToString(), Is.EqualTo("{\"Test\":false}"));
        }

        [Test]
        public void MultipleElementObjectJsonString()
        {
            // given
            var jsonObjectDict = new Dictionary<string, JsonValue>();
            jsonObjectDict.Add("Test", JsonBooleanValue.FromValue(false));
            jsonObjectDict.Add("Test2", JsonStringValue.FromString("Value"));

            Assert.That(JsonObjectValue.FromDictionary(jsonObjectDict).ToString(), Is.EqualTo("{\"Test\":false,\"Test2\":\"Value\"}"));
        }

        [Test]
        public void NestedObjectInObjectJsonString()
        {
            // given
            var jsonObjectDict = new Dictionary<string, JsonValue>();
            jsonObjectDict.Add("Test", JsonBooleanValue.FromValue(false));

            var jsonNestedObjectDict = new Dictionary<string, JsonValue>();
            jsonNestedObjectDict.Add("Test3", JsonNumberValue.FromLong(1));

            jsonObjectDict.Add("Test2", JsonObjectValue.FromDictionary(jsonNestedObjectDict));

            Assert.That(JsonObjectValue.FromDictionary(jsonObjectDict).ToString(), Is.EqualTo("{\"Test\":false,\"Test2\":{\"Test3\":1}}"));
        }

        [Test]
        public void ParsingSpecialUnicodeCharacter()
        {
            var jsonObjectDict = new Dictionary<string, JsonValue>();

            jsonObjectDict.Add("Test", JsonStringValue.FromString("/\b\f\n\r\t\"\\\ud834\uDD1E"));
            Assert.That(JsonObjectValue.FromDictionary(jsonObjectDict).ToString(), Is.EqualTo("{\"Test\":\"\\/\\b\\f\\n\\r\\t\\\"\\\\𝄞\"}"));
        }


    }
}