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

using System;

namespace Dynatrace.OpenKit.Util.Json.Parser
{
    /// <summary>
    ///     Exception class thrown by the <see cref="JsonParser" /> in case of an error.
    /// </summary>
    public class JsonParserException : Exception
    {
        /// <summary>
        ///     Constructor taking an exception message.
        /// </summary>
        /// <param name="message">the message describing the cause of this exception.</param>
        public JsonParserException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Constructor taking an exception message and a nested exception.
        /// </summary>
        /// <param name="message">the message describing the cause of the exception.</param>
        /// <param name="exception">the nested exception, originally causing this exception.</param>
        public JsonParserException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}