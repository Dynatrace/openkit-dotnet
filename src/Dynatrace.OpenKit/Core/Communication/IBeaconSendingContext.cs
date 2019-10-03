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

using System.Collections.Generic;
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// State context for beacon sending
    /// </summary>
    internal interface IBeaconSendingContext
    {
        /// <summary>
        /// Returns the HTTP client provider
        /// </summary>
        IHttpClientProvider HttpClientProvider { get; }

        /// <summary>
        /// Returns the current state
        /// </summary>
        AbstractBeaconSendingState CurrentState { get; }

        /// <summary>
        /// Returns the next state
        /// </summary>
        AbstractBeaconSendingState NextState { get; set; }


        /// <summary>
        /// Last time open sessions were sent
        /// </summary>
        long LastOpenSessionBeaconSendTime { get; set; }

        /// <summary>
        /// Last time a successful status request was performed
        /// </summary>
        long LastStatusCheckTime { get; set; }

        /// <summary>
        /// Returns <code>true</code> if the initialization was performed otherwise <code>false</code>
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Returns <code>true</code> if a shutdown was requested otherwise <code>false</code>
        /// </summary>
        bool IsShutdownRequested { get; }

        /// <summary>
        /// Returns the current timestamp in milliseconds since epoch
        /// </summary>
        long CurrentTimestamp { get; }

        /// <summary>
        /// Returns the interval for sending open sessions
        /// </summary>
        int SendInterval { get; }

        /// <summary>
        /// Returns <code>true</code> if capture is enabled otherwise <code>false</code>
        /// </summary>
        bool IsCaptureOn { get; }

        /// <summary>
        /// Returns <code>true</code> if the current state is a terminal state otherwise <code>false</code>
        /// </summary>
        bool IsInTerminalState { get; }

        /// <summary>
        /// Get all sessions that are considered new.
        /// </summary>
        List<ISessionInternals> NewSessions { get; }

        /// <summary>
        /// Get a list of all sessions that have been configured and are currently open.
        /// </summary>
        List<ISessionInternals> OpenAndConfiguredSessions { get; }

        /// <summary>
        /// Get a list of all sessions that have been configured and already finished.
        /// </summary>
        List<ISessionInternals> FinishedAndConfiguredSessions { get; }

        /// <summary>
        /// Executes the current state
        /// </summary>
        void ExecuteCurrentState();

        /// <summary>
        /// Requests a shutdown
        /// </summary>
        void RequestShutdown();

        /// <summary>
        /// Waits for the initialization to be finished
        /// </summary>
        /// <returns>Returns <code>true</code> if initialization was successful otherwise <code>false</code></returns>
        bool WaitForInit();

        /// <summary>
        /// Waits for the initialization to be finished or <code>timeoutMillis</code> milliseconds
        /// </summary>
        /// <param name="timeoutMillis">Milliseconds to wait</param>
        /// <returns>Returns <code>true</code> if initialization was successful otherwise <code>false</code></returns>
        bool WaitForInit(int timeoutMillis);

        /// <summary>
        /// Set the result of the init step.
        /// </summary>
        /// <param name="success"><code>true</code> if init was successful otherwise <code>false</code></param>
        void InitCompleted(bool success);

        /// <summary>
        /// Returns an instance of HTTPClient using the current configuration
        /// </summary>
        /// <returns></returns>
        IHttpClient GetHttpClient();

        /// <summary>
        /// Sleeps <code>DEFAULT_SLEEP_TIME_MILLISECONDS</code> millis
        /// </summary>
        void Sleep();

        /// <summary>
        /// Sleeps the given amount of milliseconds
        /// </summary>
        /// <param name="millis">the time to sleep in milliseconds</param>
        void Sleep(int millis);

        /// <summary>
        /// Disables capturing and clears all session data. Finished sessions are removed from the beacon.
        /// </summary>
        void DisableCaptureAndClear();

        /// <summary>
        /// Updates the configuration based on the provided status response.
        /// </summary>
        /// <param name="statusResponse">the status response received from the server</param>
        void HandleStatusResponse(IStatusResponse statusResponse);

        /// <summary>
        /// Returns the current server ID to be used for creating new sessions.
        /// </summary>
        int CurrentServerId { get; }

        /// <summary>
        /// Returns the number of sessions currently known to this context.
        /// </summary>
        int SessionCount { get; }

        /// <summary>
        /// Adds the given session to the internal container of sessions.
        /// </summary>
        /// <param name="session">the new session to add.</param>
        void AddSession(ISessionInternals session);

        /// <summary>
        /// Remove <paramref name="session"/> from internal list of sessions.
        /// </summary>
        /// <param name="session">The session wrapper to remove</param>
        /// <returns><code>true</code> on success, <code>false</code> otherwise.</returns>
        bool RemoveSession(ISessionInternals session);
    }
}
