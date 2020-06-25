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

namespace Dynatrace.OpenKit.Util.Json.Objects
{
    /// <summary>
    ///     JSON value class representing a string value.
    /// </summary>
    public class JsonStringValue : JsonValue
    {
        /// <summary>
        ///     Constructor taking the underlying string value.
        ///     <para>
        ///         To create an instance of <see cref="JsonStringValue" /> use the factory method
        ///         <see cref="JsonStringValue.FromString" />
        ///     </para>
        /// </summary>
        /// <param name="stringValue">the string value of the JSON string.</param>
        private JsonStringValue(string stringValue)
        {
            Value = stringValue;
        }

        /// <summary>
        ///     Indicates that this value is of type string.
        /// </summary>
        public override JsonValueType ValueType => JsonValueType.STRING;

        /// <summary>
        ///     Returns the underlying string
        /// </summary>
        public string Value { get; }

        /// <summary>
        ///     Factory method to create a <see cref="JsonStringValue" /> and initialize it with the given string.
        /// </summary>
        /// <param name="stringValue">the string value used for initializing this instance.</param>
        /// <returns>
        ///     newly created <see cref="JsonStringValue" /> or <code>null</code> if the given string is <code>null</code>.
        /// </returns>
        public static JsonStringValue FromString(string stringValue)
        {
            return stringValue == null ? null : new JsonStringValue(stringValue);
        }
    }
}