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

using System;

namespace Dynatrace.OpenKit.Util.Json.Lexer
{
    /// <summary>
    ///     Exception class thrown by the lexical analyzer in case of an error.
    /// </summary>
#if !NETSTANDARD1_1 && !WINDOWS_UWP && !NETCOREAPP1_0 && !NETCOREAPP1_1
    [Serializable]
#endif
    public class JsonLexerException : Exception
    {
        /// <summary>
        ///     Constructor taking an exception message.
        /// </summary>
        /// <param name="message">the message describing the cause of the exception.</param>
        public JsonLexerException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Constructor taking an exception message and an inner <see cref="Exception" />.
        /// </summary>
        /// <param name="message">the message describing the cause of the exception.</param>
        /// <param name="innerException">the inner <see cref="Exception" /> originally causing this exception.</param>
        public JsonLexerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}