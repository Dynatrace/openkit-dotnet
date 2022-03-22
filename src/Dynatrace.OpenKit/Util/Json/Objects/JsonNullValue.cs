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
    ///     JSON value class representing null values.
    /// </summary>
    public class JsonNullValue : JsonValue
    {
        /// <summary>
        ///     Sole instance of this class.
        /// </summary>
        public static readonly JsonNullValue Null = new JsonNullValue();

        /// <summary>
        ///     Constructor.
        /// </summary>
        private JsonNullValue()
        {
        }

        /// <summary>
        ///     Indicates that this value is of type null
        /// </summary>
        public override JsonValueType ValueType => JsonValueType.NULL;

        internal override void WriteJSONString(JsonValueWriter writer)
        {
            writer.InsertValue(JsonLiterals.NullLiteral);
        }
    }
}