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

namespace Dynatrace.OpenKit.Util.Json.Lexer
{
    /// <summary>
    ///     Specifies the state of <see cref="JsonLexer" />
    /// </summary>
    internal enum JsonLexerState
    {
        /// <summary>
        ///     Initial parsing state
        /// </summary>
        INITIAL,

        /// <summary>
        ///     Parsing state
        /// </summary>
        PARSING,

        /// <summary>
        ///     End of file state
        /// </summary>
        EOF,

        /// <summary>
        ///     Error state
        /// </summary>
        ERROR
    }
}