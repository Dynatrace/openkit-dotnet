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

namespace Dynatrace.OpenKit.Util.Json.Lexer
{
    /// <summary>
    ///     Container class representing a token.
    /// </summary>
    public class JsonToken
    {
        /// <summary>
        ///     <see cref="JsonToken" /> to be used for boolean <code>true</code> literal
        /// </summary>
        internal static readonly JsonToken BooleanTrueToken =
            new JsonToken(JsonTokenType.LITERAL_BOOLEAN, JsonLiterals.BooleanTrueLiteral);

        /// <summary>
        ///     <see cref="JsonToken" /> to be used for boolean <code>false</code> literal
        /// </summary>
        internal static readonly JsonToken BooleanFalseToken =
            new JsonToken(JsonTokenType.LITERAL_BOOLEAN, JsonLiterals.BooleanFalseLiteral);

        /// <summary>
        ///     <see cref="JsonToken" /> to be used for null literal.
        /// </summary>
        internal static readonly JsonToken NullToken =
            new JsonToken(JsonTokenType.LITERAL_NULL, JsonLiterals.NullLiteral);

        /// <summary>
        ///     <see cref="JsonToken" /> to be used for left brace.
        /// </summary>
        internal static readonly JsonToken LeftBraceToken = new JsonToken(JsonTokenType.LEFT_BRACE);

        /// <summary>
        ///     <see cref="JsonToken" /> to be used for right brace.
        /// </summary>
        internal static readonly JsonToken RightBraceToken = new JsonToken(JsonTokenType.RIGHT_BRACE);

        /// <summary>
        ///     <see cref="JsonToken" /> to be used for left square bracket.
        /// </summary>
        internal static readonly JsonToken LeftSquareBracketToken = new JsonToken(JsonTokenType.LEFT_SQUARE_BRACKET);

        /// <summary>
        ///     <see cref="JsonToken" /> to be used for right square bracket.
        /// </summary>
        internal static readonly JsonToken RightSquareBracketToken = new JsonToken(JsonTokenType.RIGHT_SQUARE_BRACKET);

        /// <summary>
        ///     <see cref="JsonToken" /> to be used for comma.
        /// </summary>
        internal static readonly JsonToken CommaToken = new JsonToken(JsonTokenType.COMMA);

        /// <summary>
        ///     <see cref="JsonToken" /> to be used for colon.
        /// </summary>
        internal static readonly JsonToken ColonToken = new JsonToken(JsonTokenType.COLON);

        /// <summary>
        ///     Constructs the token with the given type and value.
        ///     <para>
        ///         Instead of using this constructor use one of the following instead:
        ///         <list type="bullet">
        ///             <item>the predefined instances e.g. <see cref="BooleanTrueToken" /></item>
        ///             <item>
        ///                 <see cref="CreateStringToken" />
        ///             </item>
        ///             <item>
        ///                 <see cref="CreateNumberToken" />
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        /// <param name="tokenType"></param>
        /// <param name="value"></param>
        private JsonToken(JsonTokenType tokenType, string value = null)
        {
            TokenType = tokenType;
            Value = value;
        }

        /// <summary>
        ///     Returns the type of this token.
        /// </summary>
        public JsonTokenType TokenType { get; }

        /// <summary>
        ///     Returns the token value as string.
        ///     <para>
        ///         This property is only relevant for number, boolean and string tokens.
        ///     </para>
        /// </summary>
        public string Value { get; }

        /// <summary>
        ///     Creates a new <see cref="JsonToken" /> of type <see cref="JsonTokenType.VALUE_STRING" /> and the given value.
        /// </summary>
        /// <param name="stringValue">the value to be used for the token.</param>
        /// <returns>the newly created token</returns>
        internal static JsonToken CreateStringToken(string stringValue)
        {
            return new JsonToken(JsonTokenType.VALUE_STRING, stringValue);
        }

        /// <summary>
        ///     Creates a new <see cref="JsonToken" /> of type <see cref="JsonTokenType.VALUE_NUMBER" /> and the given value.
        /// </summary>
        /// <param name="numericValue">the value to be used for the token.</param>
        /// <returns>the newly created token.</returns>
        internal static JsonToken CreateNumberToken(string numericValue)
        {
            return new JsonToken(JsonTokenType.VALUE_NUMBER, numericValue);
        }

        public override string ToString()
        {
            return $"JsonToken {{tokenType={TokenType.AsString()}, value={Value ?? "null"}}}";
        }
    }
}