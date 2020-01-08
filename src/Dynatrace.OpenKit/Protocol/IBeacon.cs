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
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Protocol
{
    internal interface IBeacon
    {
        #region Properties

        /// <summary>
        /// Returns the session number of this beacon.
        /// </summary>
        int SessionNumber { get; }

        /// <summary>
        /// Returns the sequence number of the session.
        ///
        /// <para>
        /// The session sequence number is a consecutive number which is increased when a session is split due to
        /// exceeding the maximum number of allowed events. The split session will have the same session number but an
        /// increased session sequence number.
        /// </para>
        /// </summary>
        int SessionSequenceNumber { get; }

        /// <summary>
        /// Returns the device ID associated to this beacon.
        /// </summary>
        long DeviceId { get; }

        /// <summary>
        /// Indicates whether this beacon contains any event/action data or not.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Returns the next ID for an <see cref="IAction"/>
        /// </summary>
        int NextId { get; }

        /// <summary>
        /// Returns the next sequence number
        /// </summary>
        int NextSequenceNumber { get; }

        /// <summary>
        /// Returns the time when the session was started (in milliseconds).
        /// </summary>
        long SessionStartTime { get; }

        /// <summary>
        /// Returns the current timestamp.
        /// </summary>
        long CurrentTimestamp { get; }

        /// <summary>
        /// Indicates whether capturing is currently enabled or not.
        /// </summary>
        bool IsCaptureEnabled { get; }

        /// <summary>
        /// Indicates whether a server configuration is set on this beacon or not.
        /// </summary>
        bool IsServerConfigurationSet { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a web request tag for this Beacon.
        /// </summary>
        /// <param name="parentActionId">the parent action on which the tag is created.</param>
        /// <param name="sequenceNo">the sequence number of the tag.</param>
        /// <returns>a tag in its serialized representation.</returns>
        string CreateTag(int parentActionId, int sequenceNo);

        /// <summary>
        /// Adds an <see cref="IActionInternals">action</see> to this beacon.
        ///
        /// <para>
        ///     The serialized data is added to the <see cref="IBeaconCache"/>
        /// </para>
        /// </summary>
        /// <param name="action">the action to add</param>
        void AddAction(IActionInternals action);

        /// <summary>
        /// Adds the session start event to this beacon.
        ///
        /// <para>
        ///     The serialized data is added to the <see cref="IBeaconCache"/>
        /// </para>
        /// </summary>
        void StartSession();

        /// <summary>
        /// Ends the given session on this beacon
        /// </summary>
        void EndSession();

        /// <summary>
        /// Reports the given integer value on the action belonging to the given ID.
        ///
        /// <para>
        ///     The serialized data is added to the <see cref="IBeaconCache"/>.
        /// </para>
        /// </summary>
        /// <param name="actionId">the identifier of the <see cref="IAction"/> on which the value was reported.</param>
        /// <param name="valueName">the name of the reported value.</param>
        /// <param name="value">the reported integer value.</param>
        void ReportValue(int actionId, string valueName, int value);

        /// <summary>
        /// Reports the given double value on the action belonging to the given ID.
        ///
        /// <para>
        ///     The serialized data is added to the <see cref="IBeaconCache"/>.
        /// </para>
        /// </summary>
        /// <param name="actionId">the identifier of the <see cref="IAction"/> on which the value was reported.</param>
        /// <param name="valueName">the name of the reported value.</param>
        /// <param name="value">the reported double value.</param>
        void ReportValue(int actionId, string valueName, double value);

        /// <summary>
        /// Reports the given string value on the action belonging to the given ID.
        ///
        /// <para>
        ///     The serialized data is added to the <see cref="IBeaconCache"/>.
        /// </para>
        /// </summary>
        /// <param name="actionId">the identifier of the <see cref="IAction"/> on which the value was reported.</param>
        /// <param name="valueName">the name of the reported value.</param>
        /// <param name="value">the reported string value.</param>
        void ReportValue(int actionId, string valueName, string value);

        /// <summary>
        /// Reports the given named event on the action belonging to the given ID.
        ///
        /// <para>
        ///     The serialized data is added to the <see cref="IBeaconCache"/>.
        /// </para>
        /// </summary>
        /// <param name="actionId">the identifier of the <see cref="IAction"/> on which the event was reported.</param>
        /// <param name="eventName">the name of the reported event.</param>
        void ReportEvent(int actionId, string eventName);

        /// <summary>
        /// Reports the given error code on the action belonging to the given ID.
        ///
        /// <para>
        ///     The serialized data is added to the <see cref="IBeaconCache"/>.
        /// </para>
        /// </summary>
        /// <param name="actionId">the identifier of the <see cref="IAction"/> on which the error code was reported.</param>
        /// <param name="errorName">the name of the reported error.</param>
        /// <param name="errorCode">the reported error code.</param>
        /// <param name="reason">the reported reason of the error.</param>
        void ReportError(int actionId, string errorName, int errorCode, string reason);

        /// <summary>
        /// Reports the given crash by adding it to the beacon.
        ///
        /// <para>
        ///     The serialized data is added to the <see cref="IBeaconCache"/>.
        /// </para>
        /// </summary>
        /// <param name="errorName">the name of the reported crash.</param>
        /// <param name="reason">the reason of the crash.</param>
        /// <param name="stacktrace">the stacktrace of the crash.</param>
        void ReportCrash(string errorName, string reason, string stacktrace);

        /// <summary>
        /// Adds the given traced web request on the action belonging to the given ID.
        ///
        /// <para>
        ///     The serialized data is added to the <see cref="IBeaconCache"/>.
        /// </para>
        /// </summary>
        /// <param name="parentActionId">the identifier of the <see cref="IAction"/> in which the web request was traced.</param>
        /// <param name="webRequestTracer">the traced web request.</param>
        void AddWebRequest(int parentActionId, IWebRequestTracerInternals webRequestTracer);

        /// <summary>
        /// Reports a user identification event.
        ///
        /// <para>
        ///     The serialized data is added to the <see cref="IBeaconCache"/>.
        /// </para>
        /// </summary>
        /// <param name="userTag">the name/tag of the user</param>
        void IdentifyUser(string userTag);

        /// <summary>
        /// Sends the current state of the beacon to the <see cref="IHttpClient"/> of the given HTTP client provider.
        /// </summary>
        /// <param name="httpClientProvider">provider of the HTTP client to which to send the serialized beacon data.</param>
        /// <param name="additionalParameters">
        ///     additional parameters that will be send with the beacon request (can be <code>null</code>)
        /// </param>
        /// <returns>the response of the sent beacon.</returns>
        IStatusResponse Send(IHttpClientProvider httpClientProvider, IAdditionalQueryParameters additionalParameters);

        /// <summary>
        /// Clears all event/action data of this beacon.
        /// </summary>
        void ClearData();

        /// <summary>
        /// Initializes the beacon with the given {@link ServerConfiguration}.
        /// </summary>
        /// <param name="serverConfiguration">the server configuration which will be used for initialization.</param>
        void InitializeServerConfiguration(IServerConfiguration serverConfiguration);

        /// <summary>
        /// Updates this beacon with the given <see cref="IServerConfiguration"/>
        /// </summary>
        /// <param name="serverConfiguration">the server configuration which will be used to update this beacon.</param>
        void UpdateServerConfiguration(IServerConfiguration serverConfiguration);

        /// <summary>
        /// Enables capturing for this beacon.
        ///
        /// <para>
        ///     This will implicitly cause <see cref="IsServerConfigurationSet"/> to return <code>true</code>.
        /// </para>
        /// </summary>
        void EnableCapture();


        /// <summary>
        /// Disables capturing for this session.
        ///
        /// <para>
        ///     This will implicitly cause <see cref="IsServerConfigurationSet"/> to return <code>true</code>.
        /// </para>
        /// </summary>
        void DisableCapture();

        #endregion

        #region events

        /// <summary>
        /// Represents an event which will get fired when the server configuration is updated.
        /// </summary>
        event ServerConfigurationUpdateCallback OnServerConfigurationUpdate;

        #endregion
    }
}