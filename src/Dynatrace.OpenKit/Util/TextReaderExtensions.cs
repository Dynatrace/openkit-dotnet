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

using System.IO;

namespace Dynatrace.OpenKit.Util
{
    /// <summary>
    ///     <see cref="TextReader"/> extensions
    /// </summary>
    public static class TextReaderExtensions
    {
#if NETCOREAPP1_0 || NETCOREAPP1_1 || NETSTANDARD1_1 || WINDOWS_UWP
        public static void Close(this TextReader reader)
        {
            // nothing
        }
#endif
    }
}