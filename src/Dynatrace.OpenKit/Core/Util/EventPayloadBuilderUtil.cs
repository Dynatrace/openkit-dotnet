//
// Copyright 2018-2023 Dynatrace LLC
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

using Dynatrace.OpenKit.Util.Json.Objects;

namespace Dynatrace.OpenKit.Core.Util
{
    internal static class EventPayloadBuilderUtil
    {
        internal static bool IsObjectContainingNonFiniteNumericValues(JsonObjectValue jsonObject)
        {
            foreach (var key in jsonObject.Keys)
            {
                if (IsItemContainingNonFiniteNumericValues(jsonObject[key]))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsArrayContainingNonFiniteNumericValues(JsonArrayValue jsonArray)
        {
            foreach(JsonValue value in jsonArray)
            {
                if (IsItemContainingNonFiniteNumericValues(value))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsItemContainingNonFiniteNumericValues(JsonValue jsonValue)
        {
            return (jsonValue.ValueType == JsonValueType.OBJECT && IsObjectContainingNonFiniteNumericValues((JsonObjectValue)jsonValue))
                    || (jsonValue.ValueType == JsonValueType.ARRAY && IsArrayContainingNonFiniteNumericValues((JsonArrayValue)jsonValue))
                    || (jsonValue.ValueType == JsonValueType.NUMBER && !((JsonNumberValue)jsonValue).IsFinite());
        }
    }
}
