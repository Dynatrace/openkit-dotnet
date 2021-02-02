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

namespace Dynatrace.OpenKit.Util.Json.Lexer
{
    /// <summary>
    ///     Class providing JSON lexer constants
    /// </summary>
    internal static class JsonLexerConstants
    {
        /// <summary>
        ///     end of file character
        /// </summary>
        internal const int Eof = -1;

        /// <summary>
        ///     space character (0x09)
        /// </summary>
        internal const char Space = ' ';

        /// <summary>
        ///     Horizontal tabulator character (0x09)
        /// </summary>
        internal const char HorizontalTab = '\t';

        /// <summary>
        ///     Line feed or new line character (0x0A)
        /// </summary>
        internal const char LineFeed = '\n';

        /// <summary>
        ///     Carriage return character (0x0D)
        /// </summary>
        internal const char CarriageReturn = '\r';

        /// <summary>
        ///     Begin-array (left square bracket)
        /// </summary>
        internal const char LeftSquareBracket = '[';

        /// <summary>
        ///     End-array (right square bracket)
        /// </summary>
        internal const char RightSquareBracket = ']';

        /// <summary>
        ///     Begin-object (left brace)
        /// </summary>
        internal const char LeftBrace = '{';

        /// <summary>
        ///     End-object (right brace)
        /// </summary>
        internal const char RightBrace = '}';

        /// <summary>
        ///     Name-separator (colon)
        /// </summary>
        internal const char Colon = ':';

        /// <summary>
        ///     Value-separator (comma)
        /// </summary>
        internal const char Comma = ',';

        /// <summary>
        ///     Escape character (reverse solidus, aka. backslash)
        /// </summary>
        internal const char ReverseSolidus = '\\';

        /// <summary>
        ///     Character that may be escaped
        /// </summary>
        internal const char Solidus = '/';

        /// <summary>
        ///     Start and end of JSON string (quotation mark)
        /// </summary>
        internal const char QuotationMark = '"';

        /// <summary>
        ///     First character of true literal
        /// </summary>
        internal const char TrueLiteralStart = 't';

        /// <summary>
        ///     First character of false literal
        /// </summary>
        internal const char FalseLiteralStart = 'f';

        /// <summary>
        ///     First character of null literal
        /// </summary>
        internal const char NullLiteralStart = 'n';

        /// <summary>
        ///     Backspace character
        /// </summary>
        internal const char Backspace = '\b';

        /// <summary>
        ///     Form feed character
        /// </summary>
        internal const char FormFeed = '\f';

        /// <summary>
        ///     The number of characters used, after an unicode escape sequence is encountered
        /// </summary>
        internal const int NumUnicodeCharacters = 4;
    }
}