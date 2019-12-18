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

namespace Dynatrace.OpenKit.Core.Util
{
    public interface IInterruptibleThreadSuspender
    {
        /// <summary>
        /// Suspends the current thread for the given amount of time.
        /// </summary>
        /// <param name="millis">the time in milliseconds for which the current thread is suspended.</param>
        /// <returns>
        ///     <code>true</code> if sleeping ended normally (includes also being woken up by <see cref="WakeUp"/>, only
        ///     returns <code>false</code> if the sleep was interrupted by a ThreadInterruptedException.
        /// </returns>
        bool Sleep(int millis);

        /// <summary>
        /// Wakes up the threads which are currently sleeping due to a call to <see cref="Sleep"/>
        /// </summary>
        void WakeUp();
    }
}