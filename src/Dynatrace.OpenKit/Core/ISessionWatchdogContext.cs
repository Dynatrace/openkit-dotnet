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

using Dynatrace.OpenKit.Core.Objects;

namespace Dynatrace.OpenKit.Core
{
    /// <summary>
    /// A context keeping track of:
    /// <list type="bullet">
    ///     <item>
    ///         <see cref="ISessionInternals"/> which could not be finished after session splitting and which will
    ///         forcefully finished after a grace period.
    ///     </item>
    ///     <item>
    ///         <see cref="ISessionProxy"/> which need to be split after maximum session duration / idle timeout
    ///     </item>
    /// </list>
    /// </summary>
    internal interface ISessionWatchdogContext
    {
        /// <summary>
        /// Looks over the tracked sessions / session proxies and finishes / splits the ones where the respective time
        /// is expired.
        /// </summary>
        void Execute();

        /// <summary>
        /// Requests this context to finish up and shutdown.
        /// </summary>
        void RequestShutdown();

        /// <summary>
        /// Indicates whether shutdown was requested on this context or not.
        /// </summary>
        bool IsShutdownRequested { get; }

        /// <summary>
        /// Tries to close/end the given session or enqueues it for closing after the given grace period.
        /// </summary>
        /// <param name="session">the session to be closed or enqueued for closing.</param>
        /// <param name="closeGracePeriodInMillis">the grace period after which the session is to be forcefully closed</param>
        void CloseOrEnqueueForClosing(ISessionInternals session, long closeGracePeriodInMillis);

        /// <summary>
        /// Removes the given session for 'auto-closing' from this watchdog.
        /// </summary>
        /// <param name="session">the session to be removed.</param>
        void DequeueFromClosing(ISessionInternals session);

        /// <summary>
        /// Adds the given session proxy so that it will be automatically split the underlying session when the idle
        /// timeout or the maximum session duration is reached.
        /// </summary>
        /// <param name="sessionProxy">the session proxy to be added.</param>
        void AddToSplitByTimeout(ISessionProxy sessionProxy);

        /// <summary>
        /// Removes the given session proxy from automatically splitting it after idle or session max duration expired.
        /// </summary>
        /// <param name="sessionProxy">the session proxy to be removed.</param>
        void RemoveFromSplitByTimeout(ISessionProxy sessionProxy);
    }
}