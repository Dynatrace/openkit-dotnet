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

namespace Dynatrace.OpenKit.Util.Json.Parser
{
    /// <summary>
    ///     Specifies the state of a <see cref="JsonParser" />
    /// </summary>
    public enum JsonParserState
    {
        /// <summary>
        ///     Initial state of the JSON parser
        /// </summary>
        INIT,

        /// <summary>
        ///     State when start of an array was encountered
        /// </summary>
        IN_ARRAY_START,

        /// <summary>
        ///     State in an array after value has been parsed
        /// </summary>
        IN_ARRAY_VALUE,

        /// <summary>
        ///     State in an array after delimiter has been parsed
        /// </summary>
        IN_ARRAY_DELIMITER,

        /// <summary>
        ///     State when start of an object was encountered
        /// </summary>
        IN_OBJECT_START,

        /// <summary>
        ///     State in an object after key has been parsed
        /// </summary>
        IN_OBJECT_KEY,

        /// <summary>
        ///     State in an object after key value delimiter (":") has been parsed
        /// </summary>
        IN_OBJECT_COLON,

        /// <summary>
        ///     State in an object after the value has been parsed
        /// </summary>
        IN_OBJECT_VALUE,

        /// <summary>
        ///     State in an object after the comma delimiter has been parsed
        /// </summary>
        IN_OBJECT_DELIMITER,

        /// <summary>
        ///     End state of the JSON parser
        /// </summary>
        END,

        /// <summary>
        ///     Error state
        /// </summary>
        ERROR
    }
}