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

namespace Dynatrace.OpenKit.Util.Json.Reader
{

    /// <summary>
    ///     Specifies a reader for subsequently consuming single characters. Implementors of this interface support
    ///     <see cref="Mark">marking</see> a specific position to which the reader can be <see cref="Reset">reset</see> to.
    /// </summary>
    public interface IResettableReader : IDisposable
    {
        /// <summary>
        ///     Reads the next character.
        /// </summary>
        int Read();

        /// <summary>
        ///     Marks the current position to which the reader can be <see cref="Reset"/>, with a given read ahead
        ///     limit.
        /// </summary>
        /// <param name="readAheadLimit">the number of characters which can read after marking for a successful reset</param>
        void Mark(int readAheadLimit);

        /// <summary>
        ///     Resets the reader to the previously <see cref="Mark">marked</see> position.
        /// </summary>
        void Reset();

        /// <summary>
        ///    Closes and frees up resources used by this reader.
        /// </summary>
        void Close();
    }
}