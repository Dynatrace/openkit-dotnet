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


using Dynatrace.OpenKit.Util.Json.Constants;
using Dynatrace.OpenKit.Util.Json.Lexer;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Util.Json.Lexer
{
    public class JsonTokenTest
    {
        [Test]
        public void TokenTypesOfPredefinedTokensAreCorrect()
        {
            // when boolean true token, then
            Assert.That(JsonToken.BooleanTrueToken.TokenType, Is.EqualTo(JsonTokenType.LITERAL_BOOLEAN));
            // when boolean false token, then
            Assert.That(JsonToken.BooleanFalseToken.TokenType, Is.EqualTo(JsonTokenType.LITERAL_BOOLEAN));
            // when null token, then
            Assert.That(JsonToken.NullToken.TokenType, Is.EqualTo(JsonTokenType.LITERAL_NULL));
            // when left brace token, then
            Assert.That(JsonToken.LeftBraceToken.TokenType, Is.EqualTo(JsonTokenType.LEFT_BRACE));
            // when right brace token, then
            Assert.That(JsonToken.RightBraceToken.TokenType, Is.EqualTo(JsonTokenType.RIGHT_BRACE));
            // when left square bracket token, then
            Assert.That(JsonToken.LeftSquareBracketToken.TokenType, Is.EqualTo(JsonTokenType.LEFT_SQUARE_BRACKET));
            // when right square bracket token, then
            Assert.That(JsonToken.RightSquareBracketToken.TokenType, Is.EqualTo(JsonTokenType.RIGHT_SQUARE_BRACKET));
            // when comma token, then
            Assert.That(JsonToken.CommaToken.TokenType, Is.EqualTo(JsonTokenType.COMMA));
            // when colon token, then
            Assert.That(JsonToken.ColonToken.TokenType, Is.EqualTo(JsonTokenType.COLON));
        }

        [Test]
        public void TokenValuesOfPredefinedTokensAreCorrect()
        {
            // when boolean true token, then
            Assert.That(JsonToken.BooleanTrueToken.Value, Is.EqualTo(JsonLiterals.BooleanTrueLiteral));
            // when boolean false token, then
            Assert.That(JsonToken.BooleanFalseToken.Value, Is.EqualTo(JsonLiterals.BooleanFalseLiteral));
            // when null token, then
            Assert.That(JsonToken.NullToken.Value, Is.EqualTo(JsonLiterals.NullLiteral));
            // when left brace token, then
            Assert.That(JsonToken.LeftBraceToken.Value, Is.Null);
            // when right brace token, then
            Assert.That(JsonToken.RightBraceToken.Value, Is.Null);
            // when left square bracket token, then
            Assert.That(JsonToken.LeftSquareBracketToken.Value, Is.Null);
            // when right square bracket token, then
            Assert.That(JsonToken.RightSquareBracketToken.Value, Is.Null);
            // when comma token, then
            Assert.That(JsonToken.CommaToken.Value, Is.Null);
            // when colon token, then
            Assert.That(JsonToken.ColonToken.Value, Is.Null);
        }

        [Test]
        public void CreateStringTokenReturnsAppropriateJsonToken()
        {
            // when
            var obtained = JsonToken.CreateStringToken("foobar");

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_STRING));
            Assert.That(obtained.Value, Is.EqualTo("foobar"));
        }

        [Test]
        public void CreateNumberTokenReturnsAppropriateJsonToken()
        {
            // when
            var obtained = JsonToken.CreateNumberToken("12345");

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_NUMBER));
            Assert.That(obtained.Value, Is.EqualTo("12345"));
        }

        [Test]
        public void TokenTypeAsStringReturnsAppropriateStringRepresentation()
        {
            // when called with number, then
            Assert.That(JsonTokenType.VALUE_NUMBER.AsString(), Is.EqualTo("NUMBER"));
            // when called with string, then
            Assert.That(JsonTokenType.VALUE_STRING.AsString(), Is.EqualTo("STRING"));
            // when called with boolean, then
            Assert.That(JsonTokenType.LITERAL_BOOLEAN.AsString(), Is.EqualTo("BOOLEAN"));
            // when called with null, then
            Assert.That(JsonTokenType.LITERAL_NULL.AsString(), Is.EqualTo(JsonLiterals.NullLiteral));
            // when called with left brace, then
            Assert.That(JsonTokenType.LEFT_BRACE.AsString(), Is.EqualTo("{"));
            // when called with right brace, then
            Assert.That(JsonTokenType.RIGHT_BRACE.AsString(), Is.EqualTo("}"));
            // when called with left square bracket, then
            Assert.That(JsonTokenType.LEFT_SQUARE_BRACKET.AsString(), Is.EqualTo("["));
            // when called with right square bracket, then
            Assert.That(JsonTokenType.RIGHT_SQUARE_BRACKET.AsString(), Is.EqualTo("]"));
            // when called with comma, then
            Assert.That(JsonTokenType.COMMA.AsString(), Is.EqualTo(","));
            // when called with colon, then
            Assert.That(JsonTokenType.COLON.AsString(), Is.EqualTo(":"));
        }

        [Test]
        public void ToStringForTokenWithoutValueGivesAppropriateStringRepresentation()
        {
            // given a token that does not store a value
            var target = JsonToken.ColonToken;

            // when
            var obtained = target.ToString();

            // then
            Assert.That(obtained, Is.EqualTo("JsonToken {tokenType=:, value=null}"));
        }

        [Test]
        public void ToStringForTokenWithValueGivesAppropriateStringRepresentation()
        {
            // given
            var target = JsonToken.CreateNumberToken("12345");

            // when
            var obtained = target.ToString();

            // then
            Assert.That(obtained, Is.EqualTo("JsonToken {tokenType=NUMBER, value=12345}"));
        }
    }
}