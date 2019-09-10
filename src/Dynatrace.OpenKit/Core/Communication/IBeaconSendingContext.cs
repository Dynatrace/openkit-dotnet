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
using Dynatrace.OpenKit.Core.Configuration;
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
        /// Returns the configuration
        /// </summary>
        OpenKitConfiguration Configuration { get; }

        /// <summary>
        /// Returns the HTTP client provider
        /// </summary>
        IHttpClientProvider HTTPClientProvider { get; }

        /// <summary>
        /// Returns the timing provider
        /// </summary>
        ITimingProvider TimingProvider { get; }

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
        List<SessionWrapper> NewSessions { get; }

        /// <summary>
        /// Get a list of all sessions that have been configured and are currently open.
        /// </summary>
        List<SessionWrapper> OpenAndConfiguredSessions { get; }

        /// <summary>
        /// Get a list of all sessions that have been configured and already finished.
        /// </summary>
        List<SessionWrapper> FinishedAndConfiguredSessions { get; }

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
        IHttpClient GetHTTPClient();

        /// <summary>
        /// Sleeps <code>DEFAULT_SLEEP_TIME_MILLISECONDS</code> millis
        /// </summary>
        void Sleep();

        /// <summary>
        /// Sleeps the given amount of milliseconds
        /// </summary>
        /// <param name="millis"></param>
        void Sleep(int millis);

        /// <summary>
        /// Disables capturing
        /// </summary>
        void DisableCapture();

        /// <summary>
        /// Updates the configuration based on the provided status response.
        /// </summary>
        /// <param name="statusResponse"></param>
        void HandleStatusResponse(StatusResponse statusResponse);

        /// <summary>
        /// Adds the provided session to the list of open sessions
        /// </summary>
        /// <param name="session"></param>
        void StartSession(Session session);

        /// <summary>
        /// Removes the provided session from the list of open sessions and adds it to the list of finished sessions
        /// </summary>
        /// <remarks>If the provided session is not contained in the list of open sessions, it is nod added to the finished sessions</remarks>
        /// <param name="session"></param>
        void FinishSession(Session session);

        /// <summary>
        /// Remove <paramref name="sessionWrapper"/> from internal list of sessions.
        /// </summary>
        /// <param name="sessionWrapper">The session wrapper to remove</param>
        /// <returns><code>true</code> on success, <code>false</code> otherwise.</returns>
        bool RemoveSession(SessionWrapper sessionWrapper);
    }
}
