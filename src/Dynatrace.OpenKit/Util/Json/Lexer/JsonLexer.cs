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
using System.Globalization;
using System.IO;
using System.Text;
using Dynatrace.OpenKit.Util.Json.Constants;
using Dynatrace.OpenKit.Util.Json.Reader;

namespace Dynatrace.OpenKit.Util.Json.Lexer
{
    public class JsonLexer : IDisposable
    {
        /// <summary>
        ///     Reader from where to read the input JSON.
        /// </summary>
        private readonly IResettableReader reader;

        /// <summary>
        ///     Current state of the lexer.
        /// </summary>
        private JsonLexerState lexerState = JsonLexerState.INITIAL;

        /// <summary>
        ///     Storage of all consumed characters, when parsing through the JSON string.
        /// </summary>
        private StringBuilder stringValueBuilder;

        /// <summary>
        /// Constructor taking the JSON string
        /// </summary>
        /// <param name="input">JSON string for lexical analysis</param>
        public JsonLexer(string input) : this(new DefaultResettableReader(input))
        {
        }

        /// <summary>
        ///     Constructor taking a <see cref="IResettableReader"/> from where to read the JSON data.
        /// </summary>
        /// <param name="reader">A <see cref="IResettableReader"/> instance from  where to read the data.</param>
        public JsonLexer(IResettableReader reader)
        {
            this.reader = reader;
        }

        /// <summary>
        ///     Returns the next token or <code>null</code> if no next token is available.
        /// </summary>
        /// <returns>the next <see cref="JsonToken"/> or <code>null</code> if there is no such token.</returns>
        /// <exception cref="JsonLexerException">
        ///     in case the next token is invalid or the lexer is already in state <see cref="JsonLexerState.ERROR"/>
        /// </exception>
        public virtual JsonToken NextToken()
        {
            if (lexerState == JsonLexerState.ERROR)
            {
                throw new JsonLexerException("JSON Lexer is in erroneous state");
            }

            if (lexerState == JsonLexerState.EOF)
            {
                return null;
            }

            // "Insignificant whitespace is allowed before or after any of the six
            //  structural characters." (https://tools.ietf.org/html/rfc8259#section-2)
            // Therefore consume all whitespace characters
            JsonToken nextToken;
            try
            {
                var isEndOfFileReached = ConsumeWhitespaceCharacters();
                if (isEndOfFileReached)
                {
                    lexerState = JsonLexerState.EOF;
                    nextToken = null;
                }
                else
                {
                    lexerState = JsonLexerState.PARSING;
                    nextToken = DoParseNextToken();
                }
            }
            catch (JsonLexerException)
            {
                lexerState = JsonLexerState.ERROR;
                throw;
            }
            catch (IOException e)
            {
                lexerState = JsonLexerState.ERROR;
                throw new JsonLexerException("IOException occured", e);
            }

            return nextToken;
        }

        /// <summary>
        ///     Parses the token after all whitespace characters were consumed.
        ///     <para>
        ///     It must be guaranteed that the first character read is
        ///     <list type="bullet">
        ///         <item>a non-whitespace character</item>
        ///         <item>not EOF (int value -1</item>
        ///     </list>
        ///     </para>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="JsonLexerException"></exception>
        private JsonToken DoParseNextToken()
        {
           reader.Mark(1);
           var nextChar = (char) reader.Read();
            switch (nextChar)
            {
                case JsonLexerConstants.LeftSquareBracket:
                    return JsonToken.LeftSquareBracketToken;
                case JsonLexerConstants.RightSquareBracket:
                    return JsonToken.RightSquareBracketToken;
                case JsonLexerConstants.LeftBrace:
                    return JsonToken.LeftBraceToken;
                case JsonLexerConstants.RightBrace:
                    return JsonToken.RightBraceToken;
                case JsonLexerConstants.Colon:
                    return JsonToken.ColonToken;
                case JsonLexerConstants.Comma:
                    return JsonToken.CommaToken;
                case JsonLexerConstants.TrueLiteralStart: // fallthrough
                case JsonLexerConstants.FalseLiteralStart:
                    // could be boolean literal (true|false)
                    reader.Reset();
                    return TryParseBooleanLiteral();
                case JsonLexerConstants.NullLiteralStart:
                    reader.Reset();
                    return TryParseNullLiteral();
                case JsonLexerConstants.QuotationMark:
                    return TryParseStringToken();
                default:
                {
                    // check if it is a number or a completely unknown token
                    if (IsDigitOrMinus(nextChar))
                    {
                        reader.Reset();
                        return TryParseNumberToken();
                    }

                    reader.Reset();
                    var literalToken = ParseLiteral();
                    throw new JsonLexerException(UnexpectedLiteralTokenMessage(literalToken));
                }
            }
        }

        /// <summary>
        ///     Tries to parse a boolean literal, which is either true or false with all lower case characters.
        ///     The definition of a boolean literal follows the specs of https://tools.ietf.org/html/rfc8259#section-3
        /// </summary>
        /// <returns>the parsed boolean token.</returns>
        /// <exception cref="JsonLexerException">in case the token is not a valid boolean literal according to RFC 9259</exception>
        private JsonToken TryParseBooleanLiteral()
        {
            var literalToken = ParseLiteral();
            if (JsonLiterals.BooleanTrueLiteral.Equals(literalToken))
            {
                return JsonToken.BooleanTrueToken;
            }

            if (JsonLiterals.BooleanFalseLiteral.Equals(literalToken))
            {
                return JsonToken.BooleanFalseToken;
            }

            // not a valid boolean literal
            throw new JsonLexerException(UnexpectedLiteralTokenMessage(literalToken));
        }

        /// <summary>
        ///     Tries to parse a null literal.
        ///     The definition of a null literal follows the specs of https://tools.ietf.org/html/rfc8259#section-3
        /// </summary>
        /// <returns>the parsed null token.</returns>
        /// <exception cref="JsonLexerException">if token is not a valid null literal according to RFC 8259</exception>
        private JsonToken TryParseNullLiteral()
        {
            var literalToken = ParseLiteral();
            if (JsonLiterals.NullLiteral.Equals(literalToken))
            {
                return JsonToken.NullToken;
            }

            // not a valid null literal
            throw new JsonLexerException(UnexpectedLiteralTokenMessage(literalToken));
        }

        /// <summary>
        ///     Tries to parse a string JSON string token.
        /// </summary>
        /// <returns>the parsed string token.</returns>
        /// <exception cref="JsonLexerException">if token is not a valid null literal according to RFC 8259</exception>
        private JsonToken TryParseStringToken()
        {
            stringValueBuilder = new StringBuilder();

            var nextChar = reader.Read();
            while (nextChar != JsonLexerConstants.Eof && nextChar != JsonLexerConstants.QuotationMark)
            {
                if (IsEscapeCharacter(nextChar))
                {
                    TryParseEscapeSequence();
                }
                else if (IsCharacterThatNeedsEscaping(nextChar))
                {
                    stringValueBuilder = null;
                    throw new JsonLexerException($"Invalid control character \"\\u{nextChar:X4}\"");
                }
                else
                {
                    stringValueBuilder.Append((char) nextChar);
                }

                nextChar = reader.Read();
            }

            var stringValue = stringValueBuilder.ToString();
            stringValueBuilder = null;

            if (nextChar != JsonLexerConstants.Eof)
            {
                return JsonToken.CreateStringToken(stringValue);
            }

            throw new JsonLexerException($"Unterminated string literal \"{stringValue}\"");
        }

        /// <summary>
        ///     Tries to parse an escape sequence.
        /// </summary>
        /// <exception cref="JsonLexerException">if the token is not a valid escape sequence according to RFC 8259</exception>
        private void TryParseEscapeSequence()
        {
            var nextChar = reader.Read();
            switch (nextChar)
            {
                case JsonLexerConstants.Eof:
                    var stringValue = stringValueBuilder.ToString();
                    stringValueBuilder = null;
                    throw new JsonLexerException($"Unterminated string literal \"{stringValue}\"");
                case JsonLexerConstants.QuotationMark: // fallthrough
                case JsonLexerConstants.ReverseSolidus: // fallthrough
                case JsonLexerConstants.Solidus:
                    stringValueBuilder.Append((char) nextChar);
                    break;
                case 'b':
                    stringValueBuilder.Append(JsonLexerConstants.Backspace);
                    break;
                case 'f':
                    stringValueBuilder.Append(JsonLexerConstants.FormFeed);
                    break;
                case 'n':
                    stringValueBuilder.Append(JsonLexerConstants.LineFeed);
                    break;
                case 'r':
                    stringValueBuilder.Append(JsonLexerConstants.CarriageReturn);
                    break;
                case 't':
                    stringValueBuilder.Append(JsonLexerConstants.HorizontalTab);
                    break;
                case 'u':
                    TryParseUnicodeEscapeSequence();
                    break;
                default:
                    throw new JsonLexerException($"Invalid escape sequence \"\\{(char) nextChar}\"");
            }
        }

        /// <summary>
        ///     Tries to parse a unicode escape sequence.
        /// </summary>
        /// <exception cref="JsonLexerException">if token is not a valid unicode escape sequence according to RFC 8259</exception>
        private void TryParseUnicodeEscapeSequence()
        {
            var unicodeSequence = ReadUnicodeEscapeSequence();

            // parse out the hex character
            var parsedChar = (char) int.Parse(unicodeSequence.ToString(), NumberStyles.HexNumber);
            if (char.IsHighSurrogate(parsedChar))
            {
                // try to parse subsequent low surrogate
                var lowSurrogate = TryParseLowSurrogateCharacter();
                if (!lowSurrogate.HasValue || !char.IsLowSurrogate(lowSurrogate.Value))
                {
                    throw new JsonLexerException($"Invalid UTF-16 surrogate pair \"\\u{unicodeSequence}\"");
                }

                stringValueBuilder.Append(parsedChar).Append(lowSurrogate.Value);
            }
            else if (char.IsLowSurrogate(parsedChar))
            {
                throw new JsonLexerException($"Invalid UTF-16 surrogate pair \"\\u{unicodeSequence}\"");
            }
            else
            {
                stringValueBuilder.Append(parsedChar);
            }
        }

        /// <summary>
        ///     Tries to parse a low surrogate UTF-16 character.
        /// </summary>
        /// <returns>
        ///     the low surrogate character or <code>null</code> if the first two characters are not the start of a unicode
        ///     escape sequence.
        /// </returns>
        private char? TryParseLowSurrogateCharacter()
        {
            reader.Mark(2);
            if (reader.Read() == JsonLexerConstants.ReverseSolidus && reader.Read() == 'u')
            {
                var unicodeSequence = ReadUnicodeEscapeSequence();
                return (char) int.Parse(unicodeSequence.ToString(), NumberStyles.HexNumber);
            }

            // first two encountered characters did not represent a unicode escape sequence prefix
            reader.Reset();
            return null;
        }

        /// <summary>
        ///     Reads a unicode escape sequence and returns the read data.
        ///     <para>
        ///         A unicode escape sequence is a 4 character sequence where each of those characters is a hex (base-16)
        ///         character.
        ///         The characters a-f may be lower, upper or mixed case.
        ///     </para>
        /// </summary>
        /// <exception cref="JsonLexerException">in case a unicode escape sequence cannot be parsed.</exception>
        private StringBuilder ReadUnicodeEscapeSequence()
        {
            var builder = new StringBuilder(JsonLexerConstants.NumUnicodeCharacters);

            var nextChar = reader.Read();
            while (nextChar != JsonLexerConstants.Eof && builder.Length < JsonLexerConstants.NumUnicodeCharacters)
            {
                if (!IsHexCharacter(nextChar))
                {
                    throw new JsonLexerException($"Invalid unicode escape sequence \"\\u{builder}{(char) nextChar}\"");
                }

                builder.Append((char) nextChar);

                reader.Mark(1);
                nextChar = reader.Read();
            }

            if (nextChar == JsonLexerConstants.Eof)
            {
                // string is not properly terminated because EOF was reached
                throw new JsonLexerException($"Unterminated string literal \"\\u{builder}\"");
            }

            // 4 hex characters were parsed
            reader.Reset();

            return builder;
        }

        /// <summary>
        ///     Tries to parse a number token.
        /// </summary>
        /// <returns>the parsed number token</returns>
        /// <exception cref="JsonLexerException">if the number token is not valid according to the RFC specification</exception>
        private JsonToken TryParseNumberToken()
        {
            var literalToken = ParseLiteral();
            if (JsonLiterals.NumberPattern.Match(literalToken).Success)
            {
                return JsonToken.CreateNumberToken(literalToken);
            }

            throw new JsonLexerException($"Invalid number literal \"{literalToken}\"");
        }

        /// <summary>
        ///     Parses a literal.
        ///     <para>
        ///         A literal is considered to be terminated either by
        ///         <list type="bullet">
        ///             <item>One of the four whitespace characters</item>
        ///             <item>One of the six structural characters</item>
        ///         </list>
        ///     </para>
        /// </summary>
        /// <returns>the parsed literal as string.</returns>
        private string ParseLiteral()
        {
            var literalTokenBuilder = new StringBuilder(16);

            reader.Mark(1);
            var chr = reader.Read();
            while (chr != JsonLexerConstants.Eof && !IsJsonWhitespaceCharacter(chr) && !IsJsonStructuralCharacter(chr))
            {
                literalTokenBuilder.Append((char) chr);

                reader.Mark(1);
                chr = reader.Read();
            }

            if (chr != JsonLexerConstants.Eof)
            {
                // reset read position which might be a whitespace or structural character
                reader.Reset();
            }

            return literalTokenBuilder.ToString();
        }

        /// <summary>
        ///     Consume all whitespace characters until the first non-whitespace character is encountered.
        /// </summary>
        /// <returns><code>true</code> if EOF (end of file) is reached, <code>false</code> otherwise.</returns>
        private bool ConsumeWhitespaceCharacters()
        {
            int chr;
            do
            {
                reader.Mark(1);
                chr = reader.Read();

            } while (chr != JsonLexerConstants.Eof && IsJsonWhitespaceCharacter(chr));

            if (chr == JsonLexerConstants.Eof)
            {
                return true;
            }

            // reset read position to the first non-whitespace character
            reader.Reset();
            return false;

        }

        /// <summary>
        ///     Tests if the given character is a JSON whitespace character according to RFC-8259.
        /// </summary>
        /// <param name="chr">the character to test whether it is a whitespace character or not.</param>
        /// <returns><code>true</code> if it is a whitespace character, <code>false</code> otherwise.</returns>
        private static bool IsJsonWhitespaceCharacter(int chr)
        {
            switch (chr)
            {
                case JsonLexerConstants.Space: // fallthrough
                case JsonLexerConstants.HorizontalTab: // fallthrough
                case JsonLexerConstants.LineFeed: // fallthrough
                case JsonLexerConstants.CarriageReturn:
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     Tests if the given character is a JSON structural character according to RFC-8259.
        /// </summary>
        /// <param name="chr">the character to test whether it is a structural character or not.</param>
        /// <returns><code>true</code> if it is a structural character, <code>false</code> otherwise.</returns>
        private static bool IsJsonStructuralCharacter(int chr)
        {
            switch (chr)
            {
                case JsonLexerConstants.LeftSquareBracket: // fallthrough
                case JsonLexerConstants.RightSquareBracket: // fallthrough
                case JsonLexerConstants.LeftBrace: // fallthrough
                case JsonLexerConstants.RightBrace: // fallthrough
                case JsonLexerConstants.Colon: // fallthrough
                case JsonLexerConstants.Comma:
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     Tests if the given character is a digit or a minus character.
        /// </summary>
        /// <param name="chr">the character to test</param>
        /// <returns><code>true</code> if it is a digit or minus character, <code>false</code>otherwise.</returns>
        private static bool IsDigitOrMinus(int chr)
        {
            return chr == '-' || chr >= '0' && chr <= '9';
        }

        /// <summary>
        ///     Tests if the given character is the escape character for escaping other characters in a JSON string.
        /// </summary>
        /// <param name="chr">the character to test.</param>
        /// <returns><code>true</code> if it is the escape character, <code>false</code> otherwise.</returns>
        private static bool IsEscapeCharacter(int chr)
        {
            return chr == JsonLexerConstants.ReverseSolidus;
        }

        /// <summary>
        ///     Tests if the given character needs escaping according to https://tools.ietf.org/html/rfc8259#section-7
        ///     <para>
        ///         Characters that need to be escaped according to RFC:
        ///         <list type="bullet">
        ///             <item>quotation mark</item>
        ///             <item>reverse solidus</item>
        ///             <item>control characters [U+0000, U+001F]</item>
        ///         </list>
        ///     </para>
        /// </summary>
        /// <param name="chr">the character to test.</param>
        /// <returns>
        ///     <code>true</code> if the character needs to be escaped in JSON strings, <code>false</code> otherwise.
        /// </returns>
        private static bool IsCharacterThatNeedsEscaping(int chr)
        {
            return chr == JsonLexerConstants.QuotationMark
                   || chr == JsonLexerConstants.ReverseSolidus
                   || chr >= 0 && chr <= 0x1F;
        }

        /// <summary>
        ///     Helper method to test if the given character is a hex character.
        ///     <para>
        ///         A character is considered to be a hex character, if
        ///         <list type="bullet">
        ///             <item>it is in the range 0-9</item>
        ///             <item>it is in the range a-f</item>
        ///             <item>it is in the range A-F</item>
        ///         </list>
        ///     </para>
        /// </summary>
        /// <param name="chr">the character to test</param>
        /// <returns><code>true</code> if the character is a hex character, <code>false</code> otherwise.</returns>
        private static bool IsHexCharacter(int chr)
        {
            return (chr >= '0' && chr <= '9')
                   || (chr >= 'a' && chr <= 'f')
                   || (chr >= 'A' && chr <= 'F');
        }

        /// <summary>
        ///     Helper method to return the "unexpected literal" exception message
        ///     <para>
        ///         This method is a helper to avoid code duplication.
        ///     </para>
        /// </summary>
        /// <param name="literalToken">the unexpected literal token.</param>
        /// <returns>an error message for an exception.</returns>
        private static string UnexpectedLiteralTokenMessage(string literalToken)
        {
            return $"Unexpected literal \"{literalToken}\"";
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}