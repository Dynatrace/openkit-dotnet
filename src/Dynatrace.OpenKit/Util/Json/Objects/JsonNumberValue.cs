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
using System.Globalization;
using Dynatrace.OpenKit.Util.Json.Constants;
using Dynatrace.OpenKit.Util.Json.Writer;

namespace Dynatrace.OpenKit.Util.Json.Objects
{
    /// <summary>
    ///     JSON value class representing a number value.
    ///     <para>
    ///         A number can either be a floating point number or an integer number.
    ///         To avoid platform specific problems, all integer numbers can be up to signed 64 bits,
    ///         and all floating point values are 64 bits.
    ///     </para>
    /// </summary>
    public class JsonNumberValue : JsonValue
    {
        /// <summary>
        ///     Constructor initializing this <see cref="JsonNumberValue" /> instance with the given long value.
        ///     <para>
        ///         Instead of using this constructor, use the <see cref="JsonNumberValue.FromLong" /> factory method.
        ///     </para>
        /// </summary>
        /// <param name="longValue">the value representing this instance</param>
        private JsonNumberValue(long longValue)
        {
            IsInteger = true;
            this.LongValue = longValue;
            DoubleValue = longValue;
        }

        /// <summary>
        ///     Constructor initializing this <see cref="JsonNumberValue" /> instance with the given double value.
        ///     <para>
        ///         Instead of using this constructor, use the <see cref="JsonNumberValue.FromDouble" /> factory method.
        ///     </para>
        /// </summary>
        /// <param name="doubleValue">the value representing this instance</param>
        private JsonNumberValue(double doubleValue)
        {
            IsInteger = false;
            LongValue = (long) doubleValue;
            this.DoubleValue = doubleValue;
        }

        /// <summary>
        ///     Indicates that this value is of type number.
        /// </summary>
        public override JsonValueType ValueType => JsonValueType.NUMBER;

        /// <summary>
        ///     Indicates whether this <see cref="JsonNumberValue" /> represents an integer value or not.
        /// </summary>
        /// <returns>
        ///     <code>true</code> if this instance represents a non decimal numeric value, <code>false</code> otherwise.
        /// </returns>
        public bool IsInteger { get; }

        /// <summary>
        ///     Indicates whether this <see cref="JsonNumberValue" /> represents a 32-bit integer or not.
        /// </summary>
        /// <returns>
        ///     <code>true</code> if this instance represents a 32-bit integer value, <code>false</code> otherwise.
        /// </returns>
        public bool IsIntValue => IsInteger && LongValue >= int.MinValue && LongValue <= int.MaxValue;

        /// <summary>
        ///     Returns a 32-bit integer value.
        ///     <para>
        ///         If this instance is representing a double or long value, then the result is the value casted to an
        ///         <code>int</code>.
        ///     </para>
        /// </summary>
        /// <returns>the value of this instance represented as 32-bit integer.</returns>
        public int IntValue => unchecked((int) LongValue);

        /// <summary>
        ///     Returns a 64-bit integer value.
        ///     <para>
        ///         If the instance is representing a double, then the result is a value casted to a <code>long</code>.
        ///     </para>
        /// </summary>
        /// <returns>the value of this instance represented as 64-bit integer.</returns>
        public long LongValue { get; }

        /// <summary>
        ///     Returns a 32-bit floating point value.
        /// </summary>
        /// <returns>the value of this instance represented as 32-bit floating point.</returns>
        public float FloatValue => (float) DoubleValue;

        /// <summary>
        ///     Returns a 64-bit floating point value.
        /// </summary>
        /// <returns>the value of this instance represented as 64-bit floating point.</returns>
        public double DoubleValue { get; }

        /// <summary>
        ///     Factory method for constructing a <see cref="JsonNumberValue" /> from a <code>long</code>.
        /// </summary>
        /// <param name="longValue">the long value</param>
        /// <returns>the newly created <see cref="JsonNumberValue" /></returns>
        public static JsonNumberValue FromLong(long longValue)
        {
            return new JsonNumberValue(longValue);
        }

        /// <summary>
        ///     Factory method for constructing a <see cref="JsonNumberValue" /> from a <code>double</code>.
        /// </summary>
        /// <param name="doubleValue">the double value</param>
        /// <returns>the newly created <see cref="JsonNumberValue" /></returns>
        public static JsonNumberValue FromDouble(double doubleValue)
        {
            return new JsonNumberValue(doubleValue);
        }

        /// <summary>
        ///     Factory method fro constructing a <see cref="JsonNumberValue" /> from a number literal.
        /// </summary>
        /// <param name="literalValue"> the number literal, which might either be an integer value or a floating point value.</param>
        /// <returns>
        ///     <code>null</code> if <code>literalValue</code> is <code>null</code> or does not represent a number.
        ///     Otherwise a newly created <see cref="JsonNumberValue" /> instance.
        /// </returns>
        public static JsonNumberValue FromNumberLiteral(string literalValue)
        {
            if (literalValue == null)
            {
                return null;
            }

            var matcher = JsonLiterals.NumberPattern.Match(literalValue);
            if (!matcher.Success)
            {
                return null;
            }

            try
            {
                if (!matcher.Groups[2].Success && !matcher.Groups[3].Success)
                {
                    // only integer part matched
                    return FromLong(long.Parse(literalValue, CultureInfo.InvariantCulture));
                }

                return FromDouble(double.Parse(literalValue, CultureInfo.InvariantCulture));
            }
            catch (ArgumentException) // not a number
            {
                return null;
            }
            catch (OverflowException)
            {
                return null;
            }
        }

        internal bool IsFinite()
        {
            return IsInteger || (!double.IsNaN(DoubleValue) && !double.IsInfinity(DoubleValue));
        }

        internal override void WriteJSONString(JsonValueWriter writer)
        {
            if (!IsFinite())
            {
                writer.InsertValue("null");
            }
            else
            {
                if (IsInteger)
                {
                    writer.InsertValue(LongValue.ToInvariantString());
                }
                else
                {
                    writer.InsertValue(DoubleValue.ToInvariantString());
                }
            }
        }
    }
}