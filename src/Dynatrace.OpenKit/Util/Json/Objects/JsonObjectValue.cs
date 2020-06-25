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

using System.Collections.Generic;

namespace Dynatrace.OpenKit.Util.Json.Objects
{
    /// <summary>
    ///     JSON value class representing an object value.
    /// </summary>
    public class JsonObjectValue : JsonValue
    {
        /// <summary>
        ///     Underlying dictionary for this JSON object.
        /// </summary>
        private readonly IDictionary<string, JsonValue> jsonObjectDictionary;

        /// <summary>
        ///     Factory method for creating a new <see cref="JsonObjectValue" />
        /// </summary>
        /// <param name="jsonObjectDictionary">the dictionary storing the keys and values of the JSON object.</param>
        private JsonObjectValue(IDictionary<string, JsonValue> jsonObjectDictionary)
        {
            this.jsonObjectDictionary = jsonObjectDictionary;
        }

        /// <summary>
        ///     Indicates that this value is of type object.
        /// </summary>
        public override JsonValueType ValueType => JsonValueType.OBJECT;

        /// <summary>
        ///     Returns a view of the keys contained in this JSON object.
        /// </summary>
        public ICollection<string> Keys => jsonObjectDictionary.Keys;

        /// <summary>
        ///     Returns the number of underlying <see cref="JsonValue" /> this object consists of.
        /// </summary>
        public int Count => jsonObjectDictionary.Count;

        /// <summary>
        ///     Returns the value to which the given key is mapped, or <code>null</code> if the given key is not mapped.
        /// </summary>
        /// <param name="key">the key whose associated JSON value is to be returned.</param>
        /// <returns>the JSON value this key is associated with or <code>null</code> if no such key exists.</returns>
        public JsonValue this[string key]
        {
            get
            {
                jsonObjectDictionary.TryGetValue(key, out var value);
                return value;
            }
        }


        /// <summary>
        ///     Factory method for creating a new <see cref="JsonObjectValue" />.
        /// </summary>
        /// <param name="jsonObjectDictionary">the dictionary storing the keys and values of the JSON object.</param>
        /// <returns>
        ///     newly created <see cref="JsonObjectValue" /> or <code>null</code> if the given dictionary is
        ///     <code>null</code>
        /// </returns>
        public static JsonObjectValue FromDictionary(IDictionary<string, JsonValue> jsonObjectDictionary)
        {
            return jsonObjectDictionary == null ? null : new JsonObjectValue(jsonObjectDictionary);
        }

        /// <summary>
        ///     Returns <code>true</code> if and only if the given key is present in this JSON object.
        /// </summary>
        /// <param name="key">the key to test whether it is present or not in this JSON object.</param>
        /// <returns><code>true</code> if the key is present in this JSON object, <code>false</code> otherwise.</returns>
        public bool ContainsKey(string key)
        {
            return jsonObjectDictionary.ContainsKey(key);
        }
    }
}