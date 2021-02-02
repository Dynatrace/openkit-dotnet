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

using System.Text.RegularExpressions;

namespace Dynatrace.OpenKit.Util.Json.Constants
{
    /// <summary>
    ///     Class providing constant literals
    /// </summary>
    public static class JsonLiterals
    {
        /// <summary>
        ///     boolean true literal
        /// </summary>
        public const string BooleanTrueLiteral = "true";

        /// <summary>
        ///     boolean false literal
        /// </summary>
        public const string BooleanFalseLiteral = "false";

        /// <summary>
        ///     null literal
        /// </summary>
        public const string NullLiteral = "null";

        /// <summary>
        ///     Regex pattern for parsing number literals
        /// </summary>
        public static readonly Regex NumberPattern = new Regex("^-?(0|[1-9]\\d*)(\\.\\d+)?([eE][+-]?\\d+)?$",
#if !NETSTANDARD1_1
            RegexOptions.Compiled | RegexOptions.Singleline);
#else
            RegexOptions.Singleline);
#endif
    }
}