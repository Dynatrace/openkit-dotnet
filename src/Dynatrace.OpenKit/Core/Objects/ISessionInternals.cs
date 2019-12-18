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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Core.Objects
{
    /// <summary>
    /// This interface represents a <see cref="Session"/> which is internally used.
    ///
    /// <para>
    /// The main purpose of this interface is to make components which require a <see cref="Session"/> to be more easily
    /// testable.
    /// </para>
    /// </summary>
    internal interface ISessionInternals : ISession, IOpenKitComposite, IOpenKitObject
    {
        /// <summary>
        /// Tests if the Session is empty or not
        ///
        /// <para>
        /// A session is considered as empty if it does not contains any action or event data.
        /// </para>
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Clears the captured beacon data.
        /// </summary>
        void ClearCapturedData();

        /// <summary>
        /// Sends the current <see cref="Beacon"/> state with the <see cref="IHttpClient"/> provided by the given
        /// <see cref="IHttpClientProvider"/>.
        /// </summary>
        /// <param name="clientProvider">the HTTP client provider.</param>
        /// <returns>the response by the server.</returns>
        IStatusResponse SendBeacon(IHttpClientProvider clientProvider);

        /// <summary>
        /// Indicates whether sending data for this session is allowed or not.
        /// </summary>
        bool IsDataSendingAllowed { get; }

        /// <summary>
        /// Returns the current state of this session.
        /// </summary>
        ISessionState State { get; }

        /// <summary>
        /// Tries to end the current session by checking if there are no more child objects (actions / web request
        /// tracers) open. In case no more child objects are open, the session is ended, otherwise it is kept open.
        /// </summary>
        /// <returns>
        /// <code>true</code> if the session was successfully ended (or was already ended before). <code>false</code>
        /// in case there are / were still open child objects (actions / web request tracers).
        /// </returns>
        bool TryEnd();

        /// <summary>
        /// Represents the end time when the session is to be forcefully ended after a session is split by exceeding the
        /// maximum top level event count was performed but the session could not be ended at that time due to open
        /// child objects (actions, tracers). Such sessions will be automatically be closed by the
        /// <see cref="SessionWatchdog"/> thread after this grace period is elapsed.
        /// </summary>
        long SplitByEventsGracePeriodEndTimeInMillis { get; set; }

        /// <summary>
        /// Updates the <see cref="IBeacon"/> with the given <see cref="IServerConfiguration"/>.
        /// </summary>
        void UpdateServerConfiguration(IServerConfiguration serverConfiguration);

        /// <summary>
        /// Enables capturing for this session.
        ///
        /// <para>
        ///     Will implicitly also set the <see cref="State">session state</see> to
        ///     <see cref="ISessionState.IsConfigured">configured</see>.
        /// </para>
        /// </summary>
        void EnableCapture();

        /// <summary>
        /// Disables capturing for this session.
        ///
        /// <para>
        ///     Will implicitly also set the <see cref="State">session state</see> to
        ///     <see cref="ISessionState.IsConfigured">configured</see>.
        /// </para>
        /// </summary>
        void DisableCapture();

        /// <summary>
        /// Indicates whether a new session request can be sent or not.
        ///
        /// <para>
        ///     This is directly related to <see cref="DecreaseNumRemainingSessionRequests()"/>
        /// </para>
        /// </summary>
        bool CanSendNewSessionRequest { get; }

        /// <summary>
        /// Decreases the number of remaining new session requests.
        ///
        /// <para>
        ///     In case no more new session requests remain, <see cref="CanSendNewSessionRequest"/> will return
        ///     <code>false</code>.
        /// </para>
        /// </summary>
        void DecreaseNumRemainingSessionRequests();

        /// <summary>
        /// Returns the beacon associated with this session.
        /// </summary>
        IBeacon Beacon { get; }
    }
}