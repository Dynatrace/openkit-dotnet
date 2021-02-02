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

namespace Dynatrace.OpenKit.Util.Json.Objects
{
    /// <summary>
    ///     Specifies the type of a respective <see cref="JsonValue" />
    /// </summary>
    public enum JsonValueType
    {
        /// <summary>
        ///     Specifies a JSON null value.
        /// </summary>
        NULL,

        /// <summary>
        ///     Specifies a JSON boolean value.
        /// </summary>
        BOOLEAN,

        /// <summary>
        ///     Specifies a JSON numeric value.
        /// </summary>
        NUMBER,

        /// <summary>
        ///     Specifies a JSON string value.
        /// </summary>
        STRING,

        /// <summary>
        ///     Specifies a JSON array value.
        /// </summary>
        ARRAY,

        /// <summary>
        ///     Specifies a JSON object value.
        /// </summary>
        OBJECT
    }
}