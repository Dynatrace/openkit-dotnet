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

using System;
using Dynatrace.OpenKit.Util.Json.Constants;

namespace Dynatrace.OpenKit.Util.Json.Lexer
{
    /// <summary>
    ///     Specifies the type of a <see cref="JsonToken" />
    /// </summary>
    public enum JsonTokenType
    {
        /// <summary>Numeric value, the value is the number text.</summary>
        VALUE_NUMBER,

        /// <summary>String value, the value does not include leading and trailing " characters.</summary>
        VALUE_STRING,

        /// <summary>Boolean literal, the value is either true or false.</summary>
        LITERAL_BOOLEAN,

        /// <summary>null literal, value will be "null".</summary>
        LITERAL_NULL,

        /// <summary>{</summary>
        LEFT_BRACE,

        /// <summary>}</summary>
        RIGHT_BRACE,

        /// <summary>[</summary>
        LEFT_SQUARE_BRACKET,

        /// <summary>]</summary>
        RIGHT_SQUARE_BRACKET,

        /// <summary>,</summary>
        COMMA,

        /// <summary>:</summary>
        COLON
    }

    public static class JsonTokenTypeExtensions
    {
        /// <summary>
        ///     Returns a string representation of the given <see cref="JsonTokenType" />
        /// </summary>
        /// <param name="tokenType"> the <see cref="JsonTokenType" /> for which to obtain the string representation.</param>
        /// <returns>string representation of the given token type</returns>
        /// <exception cref="InvalidOperationException">in case of an unknown enum literal</exception>
        public static string AsString(this JsonTokenType tokenType)
        {
            switch (tokenType)
            {
                case JsonTokenType.VALUE_NUMBER: return "NUMBER";
                case JsonTokenType.VALUE_STRING: return "STRING";
                case JsonTokenType.LITERAL_BOOLEAN: return "BOOLEAN";
                case JsonTokenType.LITERAL_NULL: return JsonLiterals.NullLiteral;
                case JsonTokenType.LEFT_BRACE: return "{";
                case JsonTokenType.RIGHT_BRACE: return "}";
                case JsonTokenType.LEFT_SQUARE_BRACKET: return "[";
                case JsonTokenType.RIGHT_SQUARE_BRACKET: return "]";
                case JsonTokenType.COMMA: return ",";
                case JsonTokenType.COLON: return ":";
            }

            throw new InvalidOperationException($"Unknown token type {tokenType}.");
        }
    }
}