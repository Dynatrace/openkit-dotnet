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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dynatrace.OpenKit.Util.Json;
using Dynatrace.OpenKit.Util.Json.Constants;
using Dynatrace.OpenKit.Util.Json.Lexer;
using Dynatrace.OpenKit.Util.Json.Objects;
using Dynatrace.OpenKit.Util.Json.Parser;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Util.Json
{
    public class JsonParserTest
    {
        private JsonLexer lexer;
        private readonly string PI = Math.PI.ToString("G17", CultureInfo.InvariantCulture);
        private readonly string E = Math.E.ToString("G17", CultureInfo.InvariantCulture);

        [SetUp]
        public void SetUp()
        {
            lexer = Substitute.For<JsonLexer>("dummy-input");
        }

        [Test]
        public void ALexerExceptionIsCaughtAndRethrownAsParserException()
        {
            // given
            var lexerException = new JsonLexerException("dummy");
            lexer.When(l => l.NextToken()).Throw(lexerException);
            var target = new TestableJsonParser(lexer);

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message, Is.EqualTo("Caught exception from lexical analysis"));
            Assert.That(ex.InnerException, Is.SameAs(lexerException));
        }

        [Test]
        public void AJsonParserInErroneousStateThrowsExceptionIfParseIsCalledAgain()
        {
            // given
            var lexerException = new JsonLexerException("dummy");
            lexer.When(l => l.NextToken()).Throw(lexerException);
            var target = new TestableJsonParser(lexer);

            // when parse is called the first time, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message, Is.EqualTo("Caught exception from lexical analysis"));
            Assert.That(ex.InnerException, Is.SameAs(lexerException));

            // ensure that parser is in erroneous state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));

            // and when called a second time, then
            ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message, Is.EqualTo("JSON parser is in erroneous state"));
            Assert.That(ex.InnerException, Is.Null);
        }

        [Test]
        public void ParsingAnEmptyJsonInputStringThrowsAnException()
        {
            // given
            var target = new JsonParser("");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message, Is.EqualTo("No JSON object could be decoded"));
            Assert.That(ex.InnerException, Is.Null);
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingNullLiteralValueWorks()
        {
            // given
            var target = new JsonParser(JsonLiterals.NullLiteral);

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonNullValue>());

            // and the parser is in end state
            Assert.That(target.State, Is.EqualTo(JsonParserState.END));
        }

        [Test]
        public void ParsingBooleanFalseLiteralValueWorks()
        {
            // given
            var target = new JsonParser(JsonLiterals.BooleanFalseLiteral);

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue) obtained).Value, Is.False);

            // and the parser is in end state
            Assert.That(target.State, Is.EqualTo(JsonParserState.END));
        }

        [Test]
        public void ParsingBooleanTrueLiteralValueWorks()
        {
            // given
            var target = new JsonParser(JsonLiterals.BooleanTrueLiteral);

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue) obtained).Value, Is.True);

            // and the parser is in end state
            Assert.That(target.State, Is.EqualTo(JsonParserState.END));
        }

        [Test]
        public void ParsingStringValueWorks()
        {
            // given
            var target = new JsonParser("\"foobar\"");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonStringValue>());
            Assert.That(((JsonStringValue) obtained).Value, Is.EqualTo("foobar"));

            // and the parser is in end state
        }

        [Test]
        public void ParsingIntegerNumberValueWorks()
        {
            // given
            var target = new JsonParser("1234567890");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonNumberValue>());
            Assert.That(((JsonNumberValue) obtained).IsInteger, Is.True);
            Assert.That(((JsonNumberValue) obtained).LongValue, Is.EqualTo(1234567890));

            // and the parser is in end state
            Assert.That(target.State, Is.EqualTo(JsonParserState.END));
        }

        [Test]
        public void ParsingFloatingPointNumberValueWorks()
        {
            // given
            var target = new JsonParser("5.125");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonNumberValue>());
            Assert.That(((JsonNumberValue) obtained).IsInteger, Is.False);
            Assert.That(((JsonNumberValue) obtained).DoubleValue, Is.EqualTo(5.125));

            // and the parser is in end state
            Assert.That(target.State, Is.EqualTo(JsonParserState.END));
        }

        [Test]
        public void CallingParseSubsequentlyReturnsAlreadyParsedObject()
        {
            // given
            var target = new JsonParser(JsonLiterals.NullLiteral);

            // when
            var obtainedOne = target.Parse();
            var obtainedTwo = target.Parse();

            // then
            Assert.That(obtainedOne, Is.SameAs(obtainedTwo));
        }

        [Test]
        public void ParsingMultipleJsonValuesFails()
        {
            // given
            var target = new JsonParser($"{JsonLiterals.NullLiteral} {JsonLiterals.BooleanTrueLiteral}");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message,
                Is.EqualTo("Unexpected token \"JsonToken {tokenType=BOOLEAN, value=true}\" at end of input"));
            Assert.That(ex.InnerException, Is.Null);

            // and als check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingRightSquareBracketAsFirstTokenThrowsAnException()
        {
            // given
            var target = new JsonParser("]");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message,
                Is.EqualTo("Unexpected token \"JsonToken {tokenType=], value=null}\" at start of input"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingRightBraceAsFirstTokenThrowsAnException()
        {
            // given
            var target = new JsonParser("}");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message,
                Is.EqualTo("Unexpected token \"JsonToken {tokenType=}, value=null}\" at start of input"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingCommaAsFirstTokenThrowsAnException()
        {
            // given
            var target = new JsonParser(",");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message,
                Is.EqualTo("Unexpected token \"JsonToken {tokenType=,, value=null}\" at start of input"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingColonAsFirstTokenThrowsAnException()
        {
            // given
            var target = new JsonParser(":");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message,
                Is.EqualTo("Unexpected token \"JsonToken {tokenType=:, value=null}\" at start of input"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingUnterminatedEmptyArrayThrowsAnException()
        {
            // given
            var target = new JsonParser("[");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message, Is.EqualTo("Unterminated JSON array"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingEmptyArrayWorks()
        {
            // given
            var target = new JsonParser("[]");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonArrayValue>());
            Assert.That(((JsonArrayValue) obtained).Count, Is.EqualTo(0));
        }

        [Test]
        public void ParsingArrayWithSingleTrueValueWorks()
        {
            // given
            var target = new JsonParser($"[{JsonLiterals.BooleanTrueLiteral}]");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonArrayValue>());

            var obtainedArray = (JsonArrayValue) obtained;
            Assert.That(obtainedArray.Count, Is.EqualTo(1));

            var obtainedArrayValue = getValueAt(obtainedArray, 0);
            Assert.That(obtainedArrayValue, Is.Not.Null);
            Assert.That(obtainedArrayValue, Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue) obtainedArrayValue).Value, Is.True);
        }

        [Test]
        public void ParsingArrayWithSingleFalseValueWorks()
        {
            // given
            var target = new JsonParser($"[{JsonLiterals.BooleanFalseLiteral}]");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonArrayValue>());

            var obtainedArray = (JsonArrayValue) obtained;
            Assert.That(obtainedArray.Count, Is.EqualTo(1));

            var obtainedArrayValue = getValueAt(obtainedArray, 0);
            Assert.That(obtainedArrayValue, Is.Not.Null);
            Assert.That(obtainedArrayValue, Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue) obtainedArrayValue).Value, Is.False);
        }

        [Test]
        public void ParsingArrayWithSingleNumberValueWorks()
        {
            // given
            var target = new JsonParser($"[{PI}]");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonArrayValue>());

            var obtainedArray = (JsonArrayValue) obtained;
            Assert.That(obtainedArray.Count, Is.EqualTo(1));

            var obtainedArrayValue = getValueAt(obtainedArray, 0);
            Assert.That(obtainedArrayValue, Is.Not.Null);
            Assert.That(obtainedArrayValue, Is.InstanceOf<JsonNumberValue>());
            Assert.That(((JsonNumberValue) obtainedArrayValue).DoubleValue, Is.EqualTo(Math.PI));
        }

        [Test]
        public void ParsingArrayWithSingleStringValueWorks()
        {
            // given
            var target = new JsonParser("[\"foobar\"]");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonArrayValue>());

            var obtainedArray = (JsonArrayValue) obtained;
            Assert.That(obtainedArray.Count, Is.EqualTo(1));

            var obtainedArrayValue = getValueAt(obtainedArray, 0);
            Assert.That(obtainedArrayValue, Is.Not.Null);
            Assert.That(obtainedArrayValue, Is.InstanceOf<JsonStringValue>());
            Assert.That(((JsonStringValue) obtainedArrayValue).Value, Is.EqualTo("foobar"));
        }

        [Test]
        public void ParsingArrayWithMultipleSimpleValuesWorks()
        {
            // given
            var target = new JsonParser($"[{JsonLiterals.NullLiteral}, {JsonLiterals.BooleanTrueLiteral}, {JsonLiterals.BooleanFalseLiteral}, {E}, \"Hello World!\"]");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonArrayValue>());

            var obtainedArray = (JsonArrayValue) obtained;
            Assert.That(obtainedArray.Count, Is.EqualTo(5));

            var obtainedFirstArrayValue = getValueAt(obtainedArray, 0);
            Assert.That(obtainedFirstArrayValue, Is.Not.Null);
            Assert.That(obtainedFirstArrayValue, Is.InstanceOf<JsonNullValue>());

            var obtainedSecondArrayValue = getValueAt(obtainedArray, 1);
            Assert.That(obtainedSecondArrayValue, Is.Not.Null);
            Assert.That(obtainedSecondArrayValue, Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue) obtainedSecondArrayValue).Value, Is.True);

            var obtainedThirdArrayValue = getValueAt(obtainedArray, 2);
            Assert.That(obtainedThirdArrayValue, Is.Not.Null);
            Assert.That(obtainedThirdArrayValue, Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue) obtainedThirdArrayValue).Value, Is.False);

            var obtainedFourthArrayValue = getValueAt(obtainedArray, 3);
            Assert.That(obtainedFourthArrayValue, Is.Not.Null);
            Assert.That(obtainedFourthArrayValue, Is.InstanceOf<JsonNumberValue>());
            Assert.That(((JsonNumberValue) obtainedFourthArrayValue).DoubleValue, Is.EqualTo(Math.E));

            var obtainedFifthArrayValue = getValueAt(obtainedArray, 4);
            Assert.That(obtainedFifthArrayValue, Is.Not.Null);
            Assert.That(obtainedFifthArrayValue, Is.InstanceOf<JsonStringValue>());
            Assert.That(((JsonStringValue) obtainedFifthArrayValue).Value, Is.EqualTo("Hello World!"));
        }

        [Test]
        public void ParsingCommaRightAfterArrayStartThrowsAnException()
        {
            // given
            var target = new JsonParser("[,");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message,
                Is.EqualTo("Unexpected token \"JsonToken {tokenType=,, value=null}\" at beginning of array"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition to error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingColonRightAfterArrayStartThrowsAnException()
        {
            // given
            var target = new JsonParser("[:");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message,
                Is.EqualTo("Unexpected token \"JsonToken {tokenType=:, value=null}\" at beginning of array"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingUnterminatedArrayAfterValueThrowsAnException()
        {
            // given
            var target = new JsonParser("[42");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message, Is.EqualTo("Unterminated JSON array"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingUnterminatedArrayAfterValueDelimiterThrowsAnException()
        {
            // given
            var target = new JsonParser("[42, ");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message, Is.EqualTo("Unterminated JSON array"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingMultipleArrayValuesWithoutDelimiterThrowsAnException()
        {
            // given
            var target = new JsonParser("[42 45]");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message,
                Is.EqualTo(
                    "Unexpected token \"JsonToken {tokenType=NUMBER, value=45}\" in array after value has been parsed"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingValueDelimiterAsLastArrayTokenThrowsAnException()
        {
            // given
            var target = new JsonParser("[42, 45,]");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message,
                Is.EqualTo("Unexpected token \"JsonToken {tokenType=], value=null}\" in array after delimiter"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingNestedEmptyArrayWorks()
        {
            // given
            var target = new JsonParser("[[]]");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonArrayValue>());

            var obtainedArray = (JsonArrayValue) obtained;
            Assert.That(obtainedArray.Count, Is.EqualTo(1));

            var obtainedArrayValue = getValueAt(obtainedArray, 0);
            Assert.That(obtainedArrayValue, Is.Not.Null);
            Assert.That(obtainedArrayValue, Is.InstanceOf<JsonArrayValue>());
            Assert.That(((JsonArrayValue) obtainedArrayValue).Count, Is.EqualTo(0));
        }

        [Test]
        public void ParsingMultipleNestedEmptyArraysWork()
        {
            // given
            var target = new JsonParser("[[], []]");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonArrayValue>());

            var obtainedArray = (JsonArrayValue) obtained;
            Assert.That(obtainedArray.Count, Is.EqualTo(2));

            var obtainedInnerArray1 = getValueAt(obtainedArray, 0);
            Assert.That(obtainedInnerArray1, Is.Not.Null);
            Assert.That(obtainedInnerArray1, Is.InstanceOf<JsonArrayValue>());
            Assert.That(((JsonArrayValue) obtainedInnerArray1).Count, Is.EqualTo(0));

            var obtainedInnerArray2 = getValueAt(obtainedArray, 1);
            Assert.That(obtainedInnerArray2, Is.Not.Null);
            Assert.That(obtainedInnerArray2, Is.InstanceOf<JsonArrayValue>());
            Assert.That(((JsonArrayValue) obtainedInnerArray2).Count, Is.EqualTo(0));
        }

        [Test]
        public void ParsingNestedArrayWithValuesWorks()
        {
            // given
            var target =
                new JsonParser(
                    $"[[{JsonLiterals.NullLiteral}, {JsonLiterals.BooleanTrueLiteral}, {JsonLiterals.BooleanFalseLiteral}, {E}, \"Hello World!\"]]");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonArrayValue>());

            var obtainedArray = (JsonArrayValue) obtained;
            Assert.That(obtainedArray.Count, Is.EqualTo(1));

            var levelOneValue = getValueAt(obtainedArray, 0);
            Assert.That(levelOneValue, Is.Not.Null);
            Assert.That(levelOneValue, Is.InstanceOf<JsonArrayValue>());

            var levelOneArray = (JsonArrayValue) levelOneValue;
            Assert.That(levelOneArray.Count, Is.EqualTo(5));

            var levelTwoValueOne = getValueAt(levelOneArray, 0);
            Assert.That(levelTwoValueOne, Is.Not.Null);
            Assert.That(levelTwoValueOne, Is.InstanceOf<JsonNullValue>());

            var levelTwoValueTwo = getValueAt(levelOneArray, 1);
            Assert.That(levelTwoValueTwo, Is.Not.Null);
            Assert.That(levelTwoValueTwo, Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue) levelTwoValueTwo).Value, Is.True);

            var levelTwoValueThree = getValueAt(levelOneArray, 2);
            Assert.That(levelTwoValueThree, Is.Not.Null);
            Assert.That(levelTwoValueThree, Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue) levelTwoValueThree).Value, Is.False);

            var levelTwoValueFour = getValueAt(levelOneArray, 3);
            Assert.That(levelTwoValueFour, Is.Not.Null);
            Assert.That(levelTwoValueFour, Is.InstanceOf<JsonNumberValue>());
            Assert.That(((JsonNumberValue) levelTwoValueFour).DoubleValue, Is.EqualTo(Math.E));

            var levelTwoValueFive = getValueAt(levelOneArray, 4);
            Assert.That(levelTwoValueFive, Is.Not.Null);
            Assert.That(levelTwoValueFive, Is.InstanceOf<JsonStringValue>());
            Assert.That(((JsonStringValue) levelTwoValueFive).Value, Is.EqualTo("Hello World!"));
        }

        [Test]
        public void ParsingMultipleNestedArrayWithValuesWorks()
        {
            // given
            var target =
                new JsonParser(
                    $"[{JsonLiterals.NullLiteral}, [{JsonLiterals.BooleanTrueLiteral}, [{JsonLiterals.BooleanFalseLiteral}, {E}, \"Hello World!\"]]]");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonArrayValue>());

            var obtainedArray = (JsonArrayValue) obtained;
            Assert.That(obtainedArray.Count, Is.EqualTo(2));

            var levelOneValueOne = getValueAt(obtainedArray, 0);
            Assert.That(levelOneValueOne, Is.Not.Null);
            Assert.That(levelOneValueOne, Is.InstanceOf<JsonNullValue>());

            var levelOneValueTwo = getValueAt(obtainedArray, 1);
            Assert.That(levelOneValueTwo, Is.Not.Null);
            Assert.That(levelOneValueTwo, Is.InstanceOf<JsonArrayValue>());

            var levelOneValueTwoArray = (JsonArrayValue) levelOneValueTwo;
            Assert.That(levelOneValueTwoArray.Count, Is.EqualTo(2));

            var levelTwoValueOne = getValueAt(levelOneValueTwoArray, 0);
            Assert.That(levelTwoValueOne, Is.Not.Null);
            Assert.That(levelTwoValueOne, Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue) levelTwoValueOne).Value, Is.True);

            var levelTwoValueTwo = getValueAt(levelOneValueTwoArray, 1);
            Assert.That(levelTwoValueTwo, Is.Not.Null);
            Assert.That(levelTwoValueTwo, Is.InstanceOf<JsonArrayValue>());

            var levelTwoValueTwoArray = (JsonArrayValue) levelTwoValueTwo;
            Assert.That(levelTwoValueTwoArray.Count, Is.EqualTo(3));

            var levelThreeValueOne = getValueAt(levelTwoValueTwoArray, 0);
            Assert.That(levelThreeValueOne, Is.Not.Null);
            Assert.That(levelThreeValueOne, Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue) levelThreeValueOne).Value, Is.False);

            var levelThreeValueTwo = getValueAt(levelTwoValueTwoArray, 1);
            Assert.That(levelThreeValueTwo, Is.Not.Null);
            Assert.That(levelThreeValueTwo, Is.InstanceOf<JsonNumberValue>());
            Assert.That(((JsonNumberValue) levelThreeValueTwo).DoubleValue, Is.EqualTo(Math.E));

            var levelThreeValueThree = getValueAt(levelTwoValueTwoArray, 2);
            Assert.That(levelThreeValueThree, Is.Not.Null);
            Assert.That(levelThreeValueThree, Is.InstanceOf<JsonStringValue>());
            Assert.That(((JsonStringValue) levelThreeValueThree).Value, Is.EqualTo("Hello World!"));
        }

        [Test]
        public void ParsingUnterminatedNestedArrayThrowsAnException()
        {
            // given
            var target =
                new JsonParser(
                    $"[[{JsonLiterals.NullLiteral}, {JsonLiterals.BooleanTrueLiteral}, {JsonLiterals.BooleanFalseLiteral}, {E}, \"Hello World!\"]");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message, Is.EqualTo("Unterminated JSON array"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingUnterminatedEmptyObjectThrowsAnException()
        {
            // given
            var target = new JsonParser("{");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message, Is.EqualTo("Unterminated JSON object"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingEmptyObjectWorks()
        {
            // given
            var target = new JsonParser("{}");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonObjectValue>());
            Assert.That(((JsonObjectValue) obtained).Count, Is.EqualTo(0));
        }

        [Test]
        public void ParsingObjectWithNullLiteralAsKeyThrowsAnException()
        {
            // given
            var target = new JsonParser("{null: \"foo\"}");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message,
                Is.EqualTo(
                    "Unexpected token \"JsonToken {tokenType=null, value=null}\" encountered - object key expected"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingObjectWithBooleanTrueLiteralAsKeyThrowsAnException()
        {
            // given
            var target = new JsonParser("{true: \"foo\"}");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message,
                Is.EqualTo(
                    "Unexpected token \"JsonToken {tokenType=BOOLEAN, value=true}\" encountered - object key expected"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingObjectWithBooleanFalseLiteralAsKeyThrowsAnException()
        {
            // given
            var target = new JsonParser("{false: \"foo\"}");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message,
                Is.EqualTo(
                    "Unexpected token \"JsonToken {tokenType=BOOLEAN, value=false}\" encountered - object key expected"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingObjectWithNumberAsKeyThrowsAnException()
        {
            // given
            var target = new JsonParser("{42: \"foo\"}");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message,
                Is.EqualTo(
                    "Unexpected token \"JsonToken {tokenType=NUMBER, value=42}\" encountered - object key expected"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingObjectWithArrayAsKeyThrowsAnException()
        {
            // given
            var target = new JsonParser("{[]: \"foo\"}");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message,
                Is.EqualTo(
                    "Unexpected token \"JsonToken {tokenType=[, value=null}\" encountered - object key expected"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingObjectWithObjectAsKeyThrowsAnException()
        {
            // given
            var target = new JsonParser("{{}: \"foo\"}");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message,
                Is.EqualTo(
                    "Unexpected token \"JsonToken {tokenType={, value=null}\" encountered - object key expected"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingObjectWithoutKeyTokenThrowsAnException()
        {
            // given
            var target = new JsonParser("{: \"foo\"}");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message, Is.EqualTo("Unexpected token \"JsonToken {tokenType=:, value=null}\" encountered - object key expected"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingObjectWithCommaAsKeyThrowsAnException()
        {
            // given
            var target = new JsonParser("{, \"foo\"}");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message, Is.EqualTo("Unexpected token \"JsonToken {tokenType=,, value=null}\" encountered - object key expected"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingObjectWithValidKeyButNoKeyValueDelimiterThrowsAnException()
        {
            // given
            var target = new JsonParser("{\"foo\" \"bar\"}");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message, Is.EqualTo("Unexpected token \"JsonToken {tokenType=STRING, value=bar}\" encountered - key-value delimiter expected"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingObjectWithStringKeyAndNullValueWorks()
        {
            // given
            var target = new JsonParser("{\"bar\": null}");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonObjectValue>());

            var obtainedObject = (JsonObjectValue) obtained;
            Assert.That(obtainedObject.Keys, Is.EqualTo(new HashSet<string> {"bar"}));
            Assert.That(obtainedObject["bar"], Is.Not.Null);
            Assert.That(obtainedObject["bar"], Is.InstanceOf<JsonNullValue>());
        }

        [Test]
        public void ParsingObjectWithStringKeyAndBooleanTrueValueWorks()
        {
            // given
            var target = new JsonParser("{\"foo\": true}");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonObjectValue>());

            var obtainedObject = (JsonObjectValue) obtained;
            Assert.That(obtainedObject.Count, Is.EqualTo(1));
            Assert.That(obtainedObject.Keys, Is.EqualTo(new HashSet<string> {"foo"}));
            Assert.That(obtainedObject["foo"], Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue)obtainedObject["foo"]).Value, Is.True);
        }

        [Test]
        public void ParsingObjectWithStringKeyAndBooleanFalseValueWorks()
        {
            // given
            var target = new JsonParser("{\"foo\": false}");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonObjectValue>());

            var obtainedObject = (JsonObjectValue) obtained;
            Assert.That(obtainedObject.Count, Is.EqualTo(1));
            Assert.That(obtainedObject.Keys, Is.EqualTo(new HashSet<string> {"foo"}));
            Assert.That(obtainedObject["foo"], Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue)obtainedObject["foo"]).Value, Is.False);
        }

        [Test]
        public void ParsingObjectWithStringKeyAndNumericValueWorks()
        {
            // given
            var target = new JsonParser("{\"foo\": 42}");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonObjectValue>());

            var obtainedObject = (JsonObjectValue) obtained;
            Assert.That(obtainedObject.Count, Is.EqualTo(1));
            Assert.That(obtainedObject.Keys, Is.EqualTo(new HashSet<string> {"foo"}));
            Assert.That(obtainedObject["foo"], Is.InstanceOf<JsonNumberValue>());
            Assert.That(((JsonNumberValue)obtainedObject["foo"]).LongValue, Is.EqualTo(42));
        }

        [Test]
        public void ParsingObjectsWithStringKeyAndStringValueWorks()
        {
            // given
            var target = new JsonParser("{\"foo\": \"bar\"}");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonObjectValue>());

            var obtainedObject = (JsonObjectValue) obtained;
            Assert.That(obtainedObject.Count, Is.EqualTo(1));
            Assert.That(obtainedObject.Keys, Is.EqualTo(new HashSet<string> {"foo"}));
            Assert.That(obtainedObject["foo"], Is.InstanceOf<JsonStringValue>());
            Assert.That(((JsonStringValue)obtainedObject["foo"]).Value, Is.EqualTo("bar"));
        }

        [Test]
        public void ParsingUnterminatedObjectAfterKeyHasBeenParsedThrowsAnException()
        {
            // given
            var target = new JsonParser("{\"foo\"");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message, Is.EqualTo("Unterminated JSON object"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingUnterminatedObjectAfterValueHasBeenParsedThrowsAnException()
        {
            // given
            var target = new JsonParser("{\"foo\": \"bar\"");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message, Is.EqualTo("Unterminated JSON object"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingMultipleKeyValuePairsWorks()
        {
            // given
            var target = new JsonParser("{\"a\": null, \"b\": false, \"c\": true, \"d\": 123.5, \"e\": \"foobar\"}");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonObjectValue>());

            var obtainedObject = (JsonObjectValue) obtained;
            Assert.That(obtainedObject.Count, Is.EqualTo(5));
            Assert.That(obtainedObject.Keys, Is.EqualTo(new HashSet<string>{"a", "b", "c", "d", "e"}));

            Assert.That(obtainedObject["a"], Is.Not.Null);
            Assert.That(obtainedObject["a"], Is.InstanceOf<JsonNullValue>());

            Assert.That(obtainedObject["b"], Is.Not.Null);
            Assert.That(obtainedObject["b"], Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue)obtainedObject["b"]).Value, Is.False);

            Assert.That(obtainedObject["c"], Is.Not.Null);
            Assert.That(obtainedObject["c"], Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue)obtainedObject["c"]).Value, Is.True);

            Assert.That(obtainedObject["d"], Is.Not.Null);
            Assert.That(obtainedObject["d"], Is.InstanceOf<JsonNumberValue>());
            Assert.That(((JsonNumberValue)obtainedObject["d"]).DoubleValue, Is.EqualTo(123.5));

            Assert.That(obtainedObject["e"], Is.Not.Null);
            Assert.That(obtainedObject["e"], Is.InstanceOf<JsonStringValue>());
            Assert.That(((JsonStringValue)obtainedObject["e"]).Value, Is.EqualTo("foobar"));
        }

        [Test]
        public void ParsingJsonObjectOverwritesValuesIfKeyIsDuplicate()
        {
            // given a JSON object key "a" being duplicated
            var target = new JsonParser("{\"a\": null, \"b\": false, \"a\": true, \"c\": 123.5, \"a\": \"foobar\"}");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonObjectValue>());

            var obtainedObject = (JsonObjectValue)obtained;
            Assert.That(obtainedObject.Count, Is.EqualTo(3));
            Assert.That(obtainedObject.Keys, Is.EqualTo(new HashSet<string>{"a", "b", "c"}));

            Assert.That(obtainedObject["a"], Is.Not.Null);
            Assert.That(obtainedObject["a"], Is.InstanceOf<JsonStringValue>());
            Assert.That(((JsonStringValue)obtainedObject["a"]).Value, Is.EqualTo("foobar"));

            Assert.That(obtainedObject["b"], Is.Not.Null);
            Assert.That(obtainedObject["b"], Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue)obtainedObject["b"]).Value, Is.False);

            Assert.That(obtainedObject["c"], Is.Not.Null);
            Assert.That(obtainedObject["c"], Is.InstanceOf<JsonNumberValue>());
            Assert.That(((JsonNumberValue)obtainedObject["c"]).DoubleValue, Is.EqualTo(123.5));
        }

        [Test]
        public void ParsingJsonObjectWithIllegalTokenAfterThrowsAnException()
        {
            // given
            var target = new JsonParser("{\"foo\": \"bar\" :}");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message, Is.EqualTo("Unexpected token \"JsonToken {tokenType=:, value=null}\" after key-value pair encountered"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingUnterminatedObjectAfterCommaThrowsAnException()
        {
            // given
            var target = new JsonParser("{\"foo\": \"bar\", ");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message, Is.EqualTo("Unterminated JSON object"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingNoStringTokenAsSecondKeyThrowsAnException()
        {
             // given
            var target = new JsonParser("{\"foo\": \"bar\", 123: 456}");

            // when, then
            var ex = Assert.Throws<JsonParserException>(() => target.Parse());
            Assert.That(ex.Message, Is.EqualTo("Unexpected token \"JsonToken {tokenType=NUMBER, value=123}\" encountered - object key expected"));
            Assert.That(ex.InnerException, Is.Null);

            // and also check transition into error state
            Assert.That(target.State, Is.EqualTo(JsonParserState.ERROR));
        }

        [Test]
        public void ParsingEmptyJsonArrayAsObjectValueWorks()
        {
            // given
            var target = new JsonParser("{\"a\": [], \"b\": []}");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonObjectValue>());

            var obtainedObject = (JsonObjectValue) obtained;
            Assert.That(obtainedObject.Count, Is.EqualTo(2));
            Assert.That(obtainedObject.Keys, Is.EqualTo(new HashSet<string> {"a", "b"}));

            Assert.That(obtainedObject["a"], Is.Not.Null);
            Assert.That(obtainedObject["a"], Is.InstanceOf<JsonArrayValue>());
            Assert.That(((JsonArrayValue)obtainedObject["a"]).Count, Is.EqualTo(0));

            Assert.That(obtainedObject["b"], Is.Not.Null);
            Assert.That(obtainedObject["b"], Is.InstanceOf<JsonArrayValue>());
            Assert.That(((JsonArrayValue)obtainedObject["b"]).Count, Is.EqualTo(0));
        }

        [Test]
        public void ParsingNonEmptyJsonArrayAsObjectValueWorks()
        {
            // given
            var target = new JsonParser("{\"a\": [null, true], \"b\": [false, 42.24, \"foobar\"]}");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonObjectValue>());

            var obtainedObject = (JsonObjectValue) obtained;
            Assert.That(obtainedObject.Count, Is.EqualTo(2));
            Assert.That(obtainedObject.Keys, Is.EqualTo(new HashSet<string> {"a", "b"}));

            Assert.That(obtainedObject["a"], Is.Not.Null);
            Assert.That(obtainedObject["a"], Is.InstanceOf<JsonArrayValue>());

            var objectFirstValue = (JsonArrayValue) obtainedObject["a"];
            Assert.That(objectFirstValue.Count, Is.EqualTo(2));

            var firstArrayFirstValue = getValueAt(objectFirstValue, 0);
            Assert.That(firstArrayFirstValue, Is.Not.Null);
            Assert.That(firstArrayFirstValue, Is.InstanceOf<JsonNullValue>());

            var firstArraySecondValue = getValueAt(objectFirstValue, 1);
            Assert.That(firstArraySecondValue, Is.Not.Null);
            Assert.That(firstArraySecondValue, Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue)firstArraySecondValue).Value, Is.True);

            var objectSecondValue = (JsonArrayValue) obtainedObject["b"];
            Assert.That(objectSecondValue.Count, Is.EqualTo(3));

            var secondArrayFirstValue = getValueAt(objectSecondValue, 0);
            Assert.That(secondArrayFirstValue, Is.Not.Null);
            Assert.That(secondArrayFirstValue, Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue)secondArrayFirstValue).Value, Is.False);

            var secondArraySecondValue = getValueAt(objectSecondValue, 1);
            Assert.That(secondArraySecondValue, Is.Not.Null);
            Assert.That(secondArraySecondValue, Is.InstanceOf<JsonNumberValue>());
            Assert.That(((JsonNumberValue)secondArraySecondValue).DoubleValue, Is.EqualTo(42.24));

            var secondArrayThirdValue = getValueAt(objectSecondValue, 2);
            Assert.That(secondArrayThirdValue, Is.Not.Null);
            Assert.That(secondArrayThirdValue, Is.InstanceOf<JsonStringValue>());
            Assert.That(((JsonStringValue)secondArrayThirdValue).Value, Is.EqualTo("foobar"));
        }

        [Test]
        public void ParsingEmptyJsonObjectAsObjectValueWorks()
        {
            // given
            var target = new JsonParser("{\"a\": {}, \"b\": {}}");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonObjectValue>());

            var obtainedObject = (JsonObjectValue) obtained;
            Assert.That(obtainedObject.Count, Is.EqualTo(2));
            Assert.That(obtainedObject.Keys, Is.EqualTo(new HashSet<string> {"a", "b"}));

            Assert.That(obtainedObject["a"], Is.Not.Null);
            Assert.That(obtainedObject["a"], Is.InstanceOf<JsonObjectValue>());
            Assert.That(((JsonObjectValue)obtainedObject["a"]).Count, Is.EqualTo(0));

            Assert.That(obtainedObject["b"], Is.Not.Null);
            Assert.That(obtainedObject["b"], Is.InstanceOf<JsonObjectValue>());
            Assert.That(((JsonObjectValue)obtainedObject["b"]).Count, Is.EqualTo(0));
        }

        [Test]
        public void ParsingNonEmptyJsonObjectAsObjectValueWorks()
        {
            // given
            var target = new JsonParser("{\"a\": {\"a\" : null, \"b\": true, \"c\": [false, 42.25, \"foobar\"]}}");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonObjectValue>());

            var obtainedObject = (JsonObjectValue) obtained;
            Assert.That(obtainedObject.Count, Is.EqualTo(1));
            Assert.That(obtainedObject.Keys, Is.EqualTo(new HashSet<string> {"a"}));

            Assert.That(obtainedObject["a"], Is.Not.Null);
            Assert.That(obtainedObject["a"], Is.InstanceOf<JsonObjectValue>());

            var innerObject = (JsonObjectValue) obtainedObject["a"];
            Assert.That(innerObject.Count, Is.EqualTo(3));
            Assert.That(innerObject.Keys, Is.EqualTo(new HashSet<string> {"a", "b", "c"}));

            Assert.That(innerObject["a"], Is.Not.Null);
            Assert.That(innerObject["a"], Is.InstanceOf<JsonNullValue>());

            Assert.That(innerObject["b"], Is.Not.Null);
            Assert.That(innerObject["b"], Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue)innerObject["b"]).Value, Is.True);

            Assert.That(innerObject["c"], Is.Not.Null);
            Assert.That(innerObject["c"], Is.InstanceOf<JsonArrayValue>());

            var innerArray = (JsonArrayValue) innerObject["c"];
            Assert.That(innerArray.Count, Is.EqualTo(3));

            var innerArrayFirstValue = getValueAt(innerArray, 0);
            Assert.That(innerArrayFirstValue, Is.Not.Null);
            Assert.That(innerArrayFirstValue, Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue)innerArrayFirstValue).Value, Is.False);

            var innerArraySecondValue = getValueAt(innerArray, 1);
            Assert.That(innerArraySecondValue, Is.Not.Null);
            Assert.That(innerArraySecondValue, Is.InstanceOf<JsonNumberValue>());
            Assert.That(((JsonNumberValue)innerArraySecondValue).DoubleValue, Is.EqualTo(42.25));

            var innerArrayThirdValue = getValueAt(innerArray, 2);
            Assert.That(innerArrayThirdValue, Is.Not.Null);
            Assert.That(innerArrayThirdValue, Is.InstanceOf<JsonStringValue>());
            Assert.That(((JsonStringValue)innerArrayThirdValue).Value, Is.EqualTo("foobar"));
        }

        [Test]
        public void ParsingEmptyJsonObjectAsArrayValueWorks()
        {
            // given
            var target = new JsonParser("[{}, {}]");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonArrayValue>());

            var obtainedArray = (JsonArrayValue) obtained;
            Assert.That(obtainedArray.Count, Is.EqualTo(2));

            var arrayFirstValue = getValueAt(obtainedArray, 0);
            Assert.That(arrayFirstValue, Is.Not.Null);
            Assert.That(arrayFirstValue, Is.InstanceOf<JsonObjectValue>());
            Assert.That(((JsonObjectValue)arrayFirstValue).Count, Is.EqualTo(0));

            var arraySecondValue = getValueAt(obtainedArray, 1);
            Assert.That(arraySecondValue, Is.Not.Null);
            Assert.That(arraySecondValue, Is.InstanceOf<JsonObjectValue>());
            Assert.That(((JsonObjectValue)arraySecondValue).Count, Is.EqualTo(0));
        }

        [Test]
        public void ParsingNonEmptyJsonObjectAsArrayValueWorks()
        {
            // given
            var target = new JsonParser("[{\"a\": null, \"b\": 42}, {\"x\": [true, false]}]");

            // when
            var obtained = target.Parse();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<JsonArrayValue>());

            var obtainedArray = (JsonArrayValue) obtained;
            Assert.That(obtainedArray.Count, Is.EqualTo(2));

            var arrayFirstValue = getValueAt(obtainedArray, 0);
            Assert.That(arrayFirstValue, Is.Not.Null);
            Assert.That(arrayFirstValue, Is.InstanceOf<JsonObjectValue>());

            var arrayFirstObject = (JsonObjectValue) arrayFirstValue;
            Assert.That(arrayFirstObject.Count, Is.EqualTo(2));
            Assert.That(arrayFirstObject.Keys, Is.EqualTo(new HashSet<string> {"a", "b"}));

            Assert.That(arrayFirstObject["a"], Is.Not.Null);
            Assert.That(arrayFirstObject["a"], Is.InstanceOf<JsonNullValue>());

            Assert.That(arrayFirstObject["b"], Is.Not.Null);
            Assert.That(arrayFirstObject["b"], Is.InstanceOf<JsonNumberValue>());
            Assert.That(((JsonNumberValue)arrayFirstObject["b"]).LongValue, Is.EqualTo(42));

            var arraySecondValue = getValueAt(obtainedArray, 1);
            Assert.That(arraySecondValue, Is.Not.Null);
            Assert.That(arraySecondValue, Is.InstanceOf<JsonObjectValue>());

            var arraySecondObject = (JsonObjectValue) arraySecondValue;
            Assert.That(arraySecondObject.Count, Is.EqualTo(1));
            Assert.That(arraySecondObject.Keys, Is.EqualTo(new HashSet<string> {"x"}));

            Assert.That(arraySecondObject["x"], Is.Not.Null);
            Assert.That(arraySecondObject["x"], Is.InstanceOf<JsonArrayValue>());

            var innerArray = (JsonArrayValue) arraySecondObject["x"];
            Assert.That(innerArray.Count, Is.EqualTo(2));

            var innerArrayFirstValue = getValueAt(innerArray, 0);
            Assert.That(innerArrayFirstValue, Is.Not.Null);
            Assert.That(innerArrayFirstValue, Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue)innerArrayFirstValue).Value, Is.True);

            var innerArraySecondValue = getValueAt(innerArray, 1);
            Assert.That(innerArraySecondValue, Is.Not.Null);
            Assert.That(innerArraySecondValue, Is.InstanceOf<JsonBooleanValue>());
            Assert.That(((JsonBooleanValue)innerArraySecondValue).Value, Is.False);
        }

        private JsonValue getValueAt(IEnumerable<JsonValue> array, int index)
        {
            var i = 0;
            return array.FirstOrDefault(arrayValue => i++ == index);
        }
    }

    class TestableJsonParser : JsonParser
    {
        internal TestableJsonParser(JsonLexer lexer) : base(lexer)
        {
        }
    }
}