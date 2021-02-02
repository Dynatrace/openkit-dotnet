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
using System.IO;
using Dynatrace.OpenKit.Util.Json.Constants;
using Dynatrace.OpenKit.Util.Json.Lexer;
using Dynatrace.OpenKit.Util.Json.Reader;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Util.Json.Lexer
{
    public class JsonLexerTest
    {
        [Test]
        public void LexingEmptyStringReturnsNullAsNextToken()
        {
            // given
            var target = new JsonLexer("");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void LexingStringWithWhitespaceOnlyReturnsNullAsNextToken()
        {
            // given
            var target = new JsonLexer(" \r\n\t");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void LexingStructuralCharacterBeginArrayGivesExpectedToken()
        {
            // given
            var target = new JsonLexer("[");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.LEFT_SQUARE_BRACKET));
            Assert.That(obtained.Value, Is.Null);
        }

        [Test]
        public void LexingStructuralCharacterEndArrayGivesExpectedToken()
        {
            // given
            var target = new JsonLexer("]");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.RIGHT_SQUARE_BRACKET));
            Assert.That(obtained.Value, Is.Null);
        }

        [Test]
        public void LexingArrayTokesWithoutWhitespaceWorks()
        {
            // given
            var target = new JsonLexer("[]");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.LEFT_SQUARE_BRACKET));
            Assert.That(obtained.Value, Is.Null);

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.RIGHT_SQUARE_BRACKET));
            Assert.That(obtained.Value, Is.Null);

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void LexingArrayTokenWorksWithWhiteSpaces()
        {
            // given
            var target = new JsonLexer(" \t[ \r\n]\t");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.LEFT_SQUARE_BRACKET));
            Assert.That(obtained.Value, Is.Null);

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.RIGHT_SQUARE_BRACKET));
            Assert.That(obtained.Value, Is.Null);

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void LexingStructuralCharacterBeginObjectGivesExpectedToken()
        {
            // given
            var target = new JsonLexer("{");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.LEFT_BRACE));
            Assert.That(obtained.Value, Is.Null);
        }

        [Test]
        public void LexingStructuralCharacterEndObjectGivesExpectedToken()
        {
            // given
            var target = new JsonLexer("}");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.RIGHT_BRACE));
            Assert.That(obtained.Value, Is.Null);
        }

        [Test]
        public void LexingObjectTokensWithoutWhitespacesWorks()
        {
            // given
            var target = new JsonLexer("{}");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.LEFT_BRACE));
            Assert.That(obtained.Value, Is.Null);

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.RIGHT_BRACE));
            Assert.That(obtained.Value, Is.Null);

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void LexingObjectTokensWorksWithWhitespaces()
        {
            // given
            var target = new JsonLexer(" \t{ \r\n}\t");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.LEFT_BRACE));
            Assert.That(obtained.Value, Is.Null);

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.RIGHT_BRACE));
            Assert.That(obtained.Value, Is.Null);
        }

        [Test]
        public void LexingNameSeparatorTokenGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer(":");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.COLON));
            Assert.That(obtained.Value, Is.Null);
        }

        [Test]
        public void LexingValueSeparatorTokenGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer(",");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.COMMA));
            Assert.That(obtained.Value, Is.Null);
        }

        [Test]
        public void LexingBooleanTrueLiteralGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("true");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.LITERAL_BOOLEAN));
            Assert.That(obtained.Value, Is.EqualTo(JsonLiterals.BooleanTrueLiteral));
        }

        [Test]
        public void LexingBooleanTrueLiteralWithLeadingAndTrailingWhitespaces()
        {
            // given
            var target = new JsonLexer("\t true \t");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.LITERAL_BOOLEAN));
            Assert.That(obtained.Value, Is.EqualTo(JsonLiterals.BooleanTrueLiteral));

            // and then
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void LexingBooleanTrueLiteralWithWrongCasingThrowsAnError()
        {
            // given
            var target = new JsonLexer("trUe");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Unexpected literal \"trUe\""));
        }

        [Test]
        public void LexingBooleanFalseLiteralGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("false");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.LITERAL_BOOLEAN));
            Assert.That(obtained.Value, Is.EqualTo(JsonLiterals.BooleanFalseLiteral));
        }

        [Test]
        public void LexingBooleanFalseLiteralWithLeadingAndTrailingWhitespaces()
        {
            // given
            var target = new JsonLexer("\t false \t");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.LITERAL_BOOLEAN));
            Assert.That(obtained.Value, Is.EqualTo(JsonLiterals.BooleanFalseLiteral));

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void LexingNullLiteralGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("null");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.LITERAL_NULL));
            Assert.That(obtained.Value, Is.EqualTo(JsonLiterals.NullLiteral));
        }

        [Test]
        public void LexingNullLiteralWithLeadingAndTrailingWhitespacesGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("\t\tnull\t\t");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.LITERAL_NULL));
            Assert.That(obtained.Value, Is.EqualTo(JsonLiterals.NullLiteral));

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void LexingNullLiteralWithWrongCasingThrowsAnError()
        {
            // given
            var target = new JsonLexer("nUll");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Unexpected literal \"nUll\""));
        }

        [Test]
        public void LexingIntegerNumberGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("42");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_NUMBER));
            Assert.That(obtained.Value, Is.EqualTo("42"));
        }

        [Test]
        public void LexingNegativeIntegerNumberGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("-42");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_NUMBER));
            Assert.That(obtained.Value, Is.EqualTo("-42"));
        }

        [Test]
        public void LexingMinusSignWithoutSubsequentDigitsThrowsException()
        {
            // given
            var target = new JsonLexer("-");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid number literal \"-\""));
        }

        [Test]
        public void LexingIntegerNumberWithLeadingPlusThrowsException()
        {
            // given
            var target = new JsonLexer("+42");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Unexpected literal \"+42\""));
        }

        [Test]
        public void LexingNumberWithLeadingZeroThrowsException()
        {
            // given
            var target = new JsonLexer("01234");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid number literal \"01234\""));
        }

        [Test]
        public void LexingNumberWithFractionPartGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("123.45");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_NUMBER));
            Assert.That(obtained.Value, Is.EqualTo("123.45"));
        }

        [Test]
        public void LexingNegativeNumberWithFractionPartGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("-123.45");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_NUMBER));
            Assert.That(obtained.Value, Is.EqualTo("-123.45"));
        }

        [Test]
        public void LexingNumberWithDecimalSeparatorAndNoSubsequentDigitsThrowsException()
        {
            // given
            var target = new JsonLexer("1234.");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid number literal \"1234.\""));
        }

        [Test]
        public void LexingNumberWithOnlyZeroInDecimalPartGivesAppropriate()
        {
            // given
            var target = new JsonLexer("123.00");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_NUMBER));
            Assert.That(obtained.Value, Is.EqualTo("123.00"));
        }

        [Test]
        public void LexingNumberWithExponentGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("1e3");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_NUMBER));
            Assert.That(obtained.Value, Is.EqualTo("1e3"));
        }

        [Test]
        public void LexingNumberWithUpperCaeExponentGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("1E2");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_NUMBER));
            Assert.That(obtained.Value, Is.EqualTo("1E2"));
        }

        [Test]
        public void LexingNumberWithExplicitPositiveExponentGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("1e+5");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_NUMBER));
            Assert.That(obtained.Value, Is.EqualTo("1e+5"));
        }

        [Test]
        public void LexingNumberWithExplicitPositiveUpperCaseExponentGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("1E+5");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_NUMBER));
            Assert.That(obtained.Value, Is.EqualTo("1E+5"));
        }

        [Test]
        public void LexingNumberWithNegativeExponentGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("1e-2");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_NUMBER));
            Assert.That(obtained.Value, Is.EqualTo("1e-2"));
        }

        [Test]
        public void LexingNumberWithNegativeUpperCaseExponentGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("1E-2");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_NUMBER));
            Assert.That(obtained.Value, Is.EqualTo("1E-2"));
        }

        [Test]
        public void LexingNumberWithExponentAndNoDigitsThrowsException()
        {
            // given
            var target = new JsonLexer("1e");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid number literal \"1e\""));
        }

        [Test]
        public void LexingNumberWithUpperCaseExponentAndNoDigitsThrowsException()
        {
            // given
            var target = new JsonLexer("1E");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid number literal \"1E\""));
        }

        [Test]
        public void LexingNumberWithExponentFollowedByPlusAndNoDigitsThrowsException()
        {
            // given
            var target = new JsonLexer("1e+");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid number literal \"1e+\""));
        }

        [Test]
        public void LexingNumberWithUpperCaseExponentFollowedByPlusAndNoDigitsThrowsException()
        {
            // given
            var target = new JsonLexer("2E+");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid number literal \"2E+\""));
        }

        [Test]
        public void LexingNumberWithExponentFollowedByMinusAndNoDigitsThrowsException()
        {
            // given
            var target = new JsonLexer("1e-");

            // when
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid number literal \"1e-\""));
        }

        [Test]
        public void LexingNumberWithUpperCaseExponentFollowedByMinusAndNoDigitsThrowsException()
        {
            // given
            var target = new JsonLexer("2E-");

            // when
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid number literal \"2E-\""));
        }

        [Test]
        public void LexingNumberWithFractionAndExponentGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("1.234e-2");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_NUMBER));
            Assert.That(obtained.Value, Is.EqualTo("1.234e-2"));
        }

        [Test]
        public void LexingNumberWithFractionAndUpperExponentGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("1.25E-3");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_NUMBER));
            Assert.That(obtained.Value, Is.EqualTo("1.25E-3"));
        }

        [Test]
        public void LexingNumberWithDecimalSeparatorImmediatelyFollowedByExponentThrowsException()
        {
            // given
            var target = new JsonLexer("1.e-2");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid number literal \"1.e-2\""));
        }

        [Test]
        public void LexingNumberWithDecimalSeparatorImmediatelyFollowedByUpperCaseExponentThrowsException()
        {
            // given
            var target = new JsonLexer("1.E-5");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid number literal \"1.E-5\""));
        }

        [Test]
        public void LexingStringGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("\"foobar\"");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_STRING));
            Assert.That(obtained.Value, Is.EqualTo("foobar"));
        }

        [Test]
        public void LexingEmptyStringGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("\"\"");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_STRING));
            Assert.That(obtained.Value, Is.EqualTo(""));
        }

        [Test]
        public void LexingUnterminatedStringThrowsAnException()
        {
            // given
            var target = new JsonLexer("\"foo");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Unterminated string literal \"foo\""));
        }

        [Test]
        public void LexingStringWithEscapedCharactersWorks()
        {
            // given
            var target = new JsonLexer("\"\\u0000\\u0001\\u0010\\n\\\"\\\\\\/\\b\\f\\n\\r\\t\"");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_STRING));
            Assert.That(obtained.Value, Is.EqualTo("\0\u0001\u0010\n\"\\/\b\f\n\r\t"));
        }

        [Test]
        public void LexingStringWithInvalidEscapeSequenceThrowsException()
        {
            // given
            var target = new JsonLexer("\"Hello \\a World!\'");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid escape sequence \"\\a\""));
        }

        [Test]
        public void LexingUnterminatedStringAfterExcapesequenceThrowsAnException()
        {
            // given
            var target = new JsonLexer("\"foo \\");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Unterminated string literal \"foo \""));
        }

        [Test]
        public void LexingStringWithEscapeAsciiCharacterGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("\"\\u0048\\u0065\\u006c\\u006c\\u006f\\u0020\\u0057\\u006f\\u0072\\u006c\\u0064\\u0021\"");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_STRING));
            Assert.That(obtained.Value, Is.EqualTo("Hello World!"));
        }

        [Test]
        public void LexingStringWithEscapedUpperCaseAsciiCharactersGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("\"\\u0048\\u0065\\u006C\\u006C\\u006F\\u0020\\u0057\\u006F\\u0072\\u006C\\u0064\\u0021\"");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_STRING));
            Assert.That(obtained.Value, Is.EqualTo("Hello World!"));
        }

        [Test]
        public void LexingStringWithCharactersThatMustBeEscapedButAreNotThrowsAnException()
        {
            // given
            var target = new JsonLexer("\"\n\"");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid control character \"\\u000A\""));
        }

        [Test]
        public void LexingStringWithSurrogatePairGivesAppropriateToken()
        {
            // given
            var target = new JsonLexer("\"\\u0048\\u0065\\u006C\\u006C\\u006F\\u0020\\uD834\\uDD1E\\u0021\"");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_STRING));
            Assert.That(obtained.Value, Is.EqualTo("Hello \ud834\udd1e!"));
        }

        [Test]
        public void LexingStringWithHigSurrogateOnlyThrowsAnException()
        {
            // given
            var target = new JsonLexer("\"\\u0048\\u0065\\u006C\\u006C\\u006F\\u0020\\uD834\\u0021\"");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid UTF-16 surrogate pair \"\\uD834\""));
        }

        [Test]
        public void LexingStringWithLowSurrogateOnlyThrowsAnException()
        {
            // given
            var target = new JsonLexer("\"\\u0048\\u0065\\u006C\\u006C\\u006F\\u0020\\uDD1E\\u0021\"");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid UTF-16 surrogate pair \"\\uDD1E\""));
        }

        [Test]
        public void LexingStringWithNonHexCharacterInUnicodeEscapeSequenceThrowsAnException()
        {
            // given
            var target = new JsonLexer("\"\\u0048\\u0065\\u006C\\u006C\\u006F\\u0020\\uDDGE\\u0021\"");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid unicode escape sequence \"\\uDDG\""));
        }

        [Test]
        public void LexingStringWithTooShortUnicodeEscapeSequenceThrowsAnException()
        {
            // given
            var target = new JsonLexer("\"\\u007\"");

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid unicode escape sequence \"\\u007\"\""));
        }

        [Test]
        public void LexingCompoundTokenStringGivesTokensInAppropriateOrder()
        {
            // given
            var target = new JsonLexer("{\"asdf\": [1234.45e-3, \"a\", null, true, false] } ");

            // when
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.LEFT_BRACE));

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_STRING));
            Assert.That(obtained.Value, Is.EqualTo("asdf"));

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.COLON));

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.LEFT_SQUARE_BRACKET));

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_NUMBER));
            Assert.That(obtained.Value, Is.EqualTo("1234.45e-3"));

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.COMMA));

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.VALUE_STRING));
            Assert.That(obtained.Value, Is.EqualTo("a"));

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.COMMA));

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.LITERAL_NULL));
            Assert.That(obtained.Value, Is.EqualTo(JsonLiterals.NullLiteral));

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.COMMA));

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.LITERAL_BOOLEAN));
            Assert.That(obtained.Value, Is.EqualTo(JsonLiterals.BooleanTrueLiteral));

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.COMMA));

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.LITERAL_BOOLEAN));
            Assert.That(obtained.Value, Is.EqualTo(JsonLiterals.BooleanFalseLiteral));

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.RIGHT_SQUARE_BRACKET));

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TokenType, Is.EqualTo(JsonTokenType.RIGHT_BRACE));

            // and when
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void IoExceptionsAreCaughtAndTransformedToLexerExceptions()
        {
            // given
            var exception = new IOException("Does not work");
            var reader = Substitute.For<IResettableReader>();
            reader.When(r => r.Read()).Throw(exception);

            var target = new JsonLexer(reader);

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("IOException occured"));
            Assert.That(ex.InnerException, Is.SameAs(exception));
        }

        [Test]
        public void RequestingNextTokenAfterIoExceptionHasBeenThrownThrowsAnException()
        {
            // given
            var exception = new IOException("Does not work");
            var reader = Substitute.For<IResettableReader>();
            reader.When(r => r.Read()).Throw(exception);

            var target = new JsonLexer(reader);

            // when, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("IOException occured"));
            Assert.That(ex.InnerException, Is.SameAs(exception));

            // and when requesting next token a second time, then
            ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("JSON Lexer is in erroneous state"));
        }

        [Test]
        public void RequestingNextTokenAfterEofReturnsNullAsNextToken()
        {
            // given
            var target = new JsonLexer("");

            // when parsing empty string
            var obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Null);
            Assert.That(target.State, Is.EqualTo(JsonLexerState.EOF));

            // when lexer is in state eof
            obtained = target.NextToken();

            // then
            Assert.That(obtained, Is.Null);
            Assert.That(target.State, Is.EqualTo(JsonLexerState.EOF));
        }

        [Test]
        public void RequestingNextTokenAfterLexerExceptionHasBeenThrownThrowsAnException()
        {
            // given
            var target = new JsonLexer("1. 1.234");

            // when requesting token first time, then
            var ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("Invalid number literal \"1.\""));

            // and when requesting next token a second time
            ex = Assert.Throws<JsonLexerException>(() => target.NextToken());
            Assert.That(ex.Message, Is.EqualTo("JSON Lexer is in erroneous state"));
        }
    }
}