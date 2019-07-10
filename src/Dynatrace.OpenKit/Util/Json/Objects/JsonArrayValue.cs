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

using System.Collections;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Util.Json.Objects
{
    /// <summary>
    ///     JSON value class representing an array value.
    ///     <para>
    ///         A JSON array is a composite object that stores other <see cref="JsonValue" />.
    ///     </para>
    /// </summary>
    public class JsonArrayValue : JsonValue, IEnumerable<JsonValue>
    {
        /// <summary>
        ///     the actual holder of the array elements
        /// </summary>
        private readonly ICollection<JsonValue> jsonValues;

        /// <summary>
        ///     Constructor for initializing this JSON array with a list of values.
        /// </summary>
        /// <param name="values">the underlying values for this JSON array</param>
        private JsonArrayValue(ICollection<JsonValue> values)
        {
            jsonValues = values;
        }

        /// <summary>
        ///     Indicates that this is an array JSON value.
        /// </summary>
        public override JsonValueType ValueType => JsonValueType.ARRAY;

        /// <summary>
        ///     Returns the size of this JSON array
        /// </summary>
        public int Count => jsonValues.Count;

        /// <summary>
        ///     Returns an enumerator over the elements in this JSON array in proper sequence.
        /// </summary>
        /// <returns>an enumerator over the elements in this JSON array in proper sequence</returns>
        IEnumerator<JsonValue> IEnumerable<JsonValue>.GetEnumerator()
        {
            return jsonValues.GetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator over the elements in this JSON array in proper sequence.
        /// </summary>
        /// <returns>an enumerator over the elements in this JSON array in proper sequence</returns>
        public IEnumerator GetEnumerator()
        {
            return jsonValues.GetEnumerator();
        }

        /// <summary>
        ///     Create a new JSON array from the given list.
        /// </summary>
        /// <param name="values">the list of JSON values the created array should hold</param>
        /// <returns>a newly created <see cref="JsonArrayValue" /> or <code>null</code> if the given value list is null</returns>
        public static JsonArrayValue FromList(ICollection<JsonValue> values)
        {
            return values == null ? null : new JsonArrayValue(values);
        }
    }
}