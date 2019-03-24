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

using System.Threading;

namespace Dynatrace.OpenKit.Providers
{
    /// <summary>
    ///  Class for providing a thread ID.
    /// </summary>
    public class DefaultThreadIDProvider : IThreadIDProvider
    {
        /// <summary>
        /// Get ID of calling thread
        /// </summary>
        /// <remarks>
        /// Thread.CurrentThread.ManagedThreadId returns a int value which may be negative.
        /// The Beacon protocol requires the thread id to be a positive integer value.
        /// The most significant bit is forced to '0' by a bitwise-and operation with an integer
        /// where all bits except for the most significant bit are set to '1'.
        /// </remarks>
        public int ThreadID =>
#if WINDOWS_UWP || NETSTANDARD1_1
            System.Environment.CurrentManagedThreadId & 0x7fffffff;
#else
            Thread.CurrentThread.ManagedThreadId & 0x7fffffff;
#endif
    }
}
