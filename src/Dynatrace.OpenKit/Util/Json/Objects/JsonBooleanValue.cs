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

using Dynatrace.OpenKit.Util.Json.Constants;
using Dynatrace.OpenKit.Util.Json.Writer;

namespace Dynatrace.OpenKit.Util.Json.Objects
{
    /// <summary>
    ///     JSON value class representing boolean values
    /// </summary>
    public class JsonBooleanValue : JsonValue
    {
        /// <summary>
        ///     Singleton instance representing the value <code>true</code>
        /// </summary>
        public static readonly JsonBooleanValue True = new JsonBooleanValue(true);

        /// <summary>
        ///     Singleton instance representing the value <code>false</code>.
        /// </summary>
        public static readonly JsonBooleanValue False = new JsonBooleanValue(false);

        /// <summary>
        ///     Constructor taking the given boolean value.
        ///     <para>
        ///         For creating an instance the respective factory methods <see cref="FromValue" /> and <see cref="FromLiteral" />
        ///         should be used.
        ///     </para>
        /// </summary>
        /// <param name="value">the boolean value represented by this instance</param>
        private JsonBooleanValue(bool value)
        {
            Value = value;
        }

        /// <summary>
        ///     Indicates that this value is of type boolean
        /// </summary>
        public override JsonValueType ValueType => JsonValueType.BOOLEAN;

        /// <summary>
        ///     Returns the value represented by this instance
        /// </summary>
        public bool Value { get; }

        /// <summary>
        ///     Returns the respective instance of <see cref="JsonBooleanValue" /> for the given bool value.
        ///     <para>
        ///         The returned value is one of the two singleton instances <see cref="True" /> or <see cref="False" />
        ///     </para>
        /// </summary>
        /// <param name="value">the boolean value</param>
        /// <returns>either <see cref="True"/> or <see cref="False"/></returns>
        public static JsonBooleanValue FromValue(bool value)
        {
            return value ? True : False;
        }

        /// <summary>
        ///     Returns an instance of <see cref="JsonBooleanValue" /> for the given JSON string literal.
        /// </summary>
        /// <param name="literal">
        ///     the string literal for which to return the respective <see cref="JsonBooleanValue" />
        ///     representation.
        /// </param>
        /// <returns>
        ///     <see cref="True" /> if the given literal equals to <see cref="JsonLiterals.BooleanTrueLiteral" />,
        ///     <see cref="False" /> if the literal is equal to <see cref="JsonLiterals.BooleanFalseLiteral" />
        ///     and <code>null</code> otherwise.
        /// </returns>
        public static JsonBooleanValue FromLiteral(string literal)
        {
            if (literal == null)
            {
                return null;
            }

            if (JsonLiterals.BooleanTrueLiteral.Equals(literal))
            {
                return True;
            }

            if (JsonLiterals.BooleanFalseLiteral.Equals(literal))
            {
                return False;
            }

            return null;
        }

        internal override void WriteJSONString(JsonValueWriter writer)
        {
            writer.InsertValue(Value ? JsonLiterals.BooleanTrueLiteral : JsonLiterals.BooleanFalseLiteral);
        }
    }
}