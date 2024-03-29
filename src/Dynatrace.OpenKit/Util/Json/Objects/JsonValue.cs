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

using Dynatrace.OpenKit.Util.Json.Writer;

namespace Dynatrace.OpenKit.Util.Json.Objects
{
    /// <summary>
    ///     Base class for all JSON value classes (e.g. string, number, ...)
    /// </summary>
    public abstract class JsonValue
    {
        /// <summary>
        ///     Returns an indicator whether this instance represents a JSON null value or not.
        /// </summary>
        public virtual JsonValueType ValueType => JsonValueType.NULL;

        public override string ToString()
        {
            JsonValueWriter Writer = new JsonValueWriter();
            WriteJSONString(Writer);
            return Writer.ToString();
        }

        internal abstract void WriteJSONString(JsonValueWriter writer);
    }
}