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
using System.Linq;
using System.Text;
using System.Threading;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Core.Util;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Protocol
{

    /// <summary>
    ///  The Beacon class holds all the beacon data and the beacon protocol implementation.
    /// </summary>
    public class Beacon
    {
        // Initial time to sleep after the first failed beacon send attempt.
        private const int InitialRetrySleepTimeMilliseconds = 1000;


        // basic data constants
        private const string BeaconKeyProtocolVersion = "vv";
        private const string BeaconKeyOpenKitVersion = "va";
        private const string BeaconKeyApplicationId = "ap";
        private const string BeaconKeyApplicationName = "an";
        private const string BeaconKeyApplicationVersion = "vn";
        private const string BeaconKeyPlatformType = "pt";
        private const string BeaconKeyAgentTechnologyType = "tt";
        private const string BeaconKeyVisitorId = "vi";
        private const string BeaconKeySessionNumber = "sn";
        private const string BeaconKeyClientIpAddress = "ip";
        private const string BeaconKeyMultiplicity = "mp";
        private const string BeaconKeyDataCollectionLevel = "dl";
        private const string BeaconKeyCrashReportingLevel = "cl";

        // device data constants
        private const string BeaconKeyDeviceOs = "os";
        private const string BeaconKeyDeviceManufacturer = "mf";
        private const string BeaconKeyDeviceModel = "md";

        // timestamp constants
        private const string BeaconKeySessionStartTime = "tv";
        private const string BeaconKeyTransmissionTime = "tx";

        // Action related constants
        private const string BeaconKeyEventType = "et";
        private const string BeaconKeyName = "na";
        private const string BeaconKeyThreadId = "it";
        private const string BeaconKeyActionId = "ca";
        private const string BeaconKeyParentActionId = "pa";
        private const string BeaconKeyStartSequenceNumber = "s0";
        private const string BeaconKeyTimeZero = "t0";
        private const string BeaconKeyEndSequenceNumber = "s1";
        private const string BeaconKeyTimeOne = "t1";

        // data and error capture constants
        private const string BeaconKeyValue = "vl";
        private const string BeaconKeyErrorCode = "ev";
        private const string BeaconKeyErrorReason = "rs";
        private const string BeaconKeyErrorStacktrace = "st";
        private const string BeaconKeyWebRequestResponseCode = "rc";
        private const string BeaconKeyWebRequestBytesSent = "bs";
        private const string BeaconKeyWebRequestBytesReceived = "br";

        // max name length
        private const int MaximumNameLength = 250;

        // web request tag prefix constant
        private const string WebRequestTagPrefix = "MT";

        // web request tag reserved characters
        private static readonly char[] ReservedCharacters = { '_' };

        private const char BeaconDataDelimiter = '&';

        // next ID and sequence number
        private int nextId = 0;
        private int nextSequenceNumber = 0;

        // session number & start time
        private readonly long sessionStartTime;

        // client IP address
        private readonly string clientIpAddress;

        // providers
        private readonly IThreadIdProvider threadIdProvider;
        private readonly ITimingProvider timingProvider;

        // configuration
        private readonly HttpClientConfiguration httpConfiguration;

        // basic beacon protocol data
        private readonly string basicBeaconData;

        // Configuration reference
        private readonly OpenKitConfiguration configuration;

        // Beacon configuration
        private volatile BeaconConfiguration beaconConfiguration;

        private readonly ILogger logger;

        private readonly BeaconCache beaconCache;
        private readonly int beaconId;

        #region constructors

        /// <summary>
        /// Creates a new instance of type Beacon
        /// </summary>
        /// <param name="logger">Logger for logging messages</param>
        /// <param name="beaconCache">Cache storing beacon related data</param>
        /// <param name="configuration">OpenKit related configuration</param>
        /// <param name="clientIpAddress">The client's IP address</param>
        /// <param name="threadIdProvider">Provider for retrieving thread id</param>
        /// <param name="timingProvider">Provider for time related methods</param>
        public Beacon(ILogger logger, BeaconCache beaconCache, OpenKitConfiguration configuration, string clientIpAddress,
            IThreadIdProvider threadIdProvider, ITimingProvider timingProvider)
            : this(logger, beaconCache, configuration, clientIpAddress, threadIdProvider, timingProvider, new DefaultPrnGenerator())
        {
        }

        /// <summary>
        /// Creates a new instance of type Beacon
        /// </summary>
        /// <param name="logger">Logger for logging messages</param>
        /// <param name="beaconCache">Cache storing beacon related data</param>
        /// <param name="configuration">OpenKit related configuration</param>
        /// <param name="clientIpAddress">The client's IP address</param>
        /// <param name="threadIdProvider">Provider for retrieving thread id</param>
        /// <param name="timingProvider">Provider for time related methods</param>
        /// <param name="randomNumberGenerator">Random number generator</param>
        internal Beacon(ILogger logger, BeaconCache beaconCache, OpenKitConfiguration configuration, string clientIpAddress,
            IThreadIdProvider threadIdProvider, ITimingProvider timingProvider, IPrnGenerator randomNumberGenerator)
        {
            this.logger = logger;
            this.beaconCache = beaconCache;
            BeaconConfiguration = configuration.BeaconConfig;

            beaconId = configuration.NextSessionNumber;
            if (beaconConfiguration.DataCollectionLevel == DataCollectionLevel.USER_BEHAVIOR)
            {
                SessionNumber = beaconId;
                DeviceId = configuration.DeviceId;
            }
            else
            {
                SessionNumber = 1;
                DeviceId = randomNumberGenerator.NextLong(long.MaxValue);
            }

            this.timingProvider = timingProvider;

            this.configuration = configuration;
            this.threadIdProvider = threadIdProvider;
            sessionStartTime = timingProvider.ProvideTimestampInMilliseconds();

            this.clientIpAddress = InetAddressValidator.IsValidIP(clientIpAddress) ? clientIpAddress : string.Empty;
            // store the current http configuration
            httpConfiguration = configuration.HttpClientConfig;
            basicBeaconData = CreateBasicBeaconData();
        }

        #endregion

        #region public properties

        public int SessionNumber { get; }

        public long DeviceId { get; }

        public bool IsEmpty => beaconCache.IsEmpty(beaconId);

        /// <summary>
        /// create next ID
        /// </summary>
        public int NextId => Interlocked.Increment(ref nextId);

        /// <summary>
        /// create next sequence number
        /// </summary>
        public int NextSequenceNumber => Interlocked.Increment(ref nextSequenceNumber);

        /// <summary>
        /// Get the current timestamp in milliseconds by delegating to TimingProvider
        /// </summary>
        public long CurrentTimestamp => timingProvider.ProvideTimestampInMilliseconds();

        #endregion

        #region internal properties

        internal BeaconConfiguration BeaconConfiguration
        {
            set
            {
                if (value != null)
                {
                    beaconConfiguration = value;
                }
            }
            get => beaconConfiguration;
        }

        internal int Multiplicity => BeaconConfiguration.Multiplicity;

        internal bool CapturingDisabled => !BeaconConfiguration.CapturingAllowed;

        /// <summary>
        /// Returns an immutable list of the event data
        /// </summary>
        /// <remarks>
        /// Used for testing
        /// </remarks>
        internal List<string> EventDataList
        {
            get
            {
                var events = beaconCache.GetEvents(beaconId);
                if (events == null)
                {
                    return new List<string>();
                }

                return events.Select(x => x.Data).ToList();
            }
        }

        /// <summary>
        /// Returns an immutable list of the action data
        /// </summary>
        /// <remarks>
        /// Used for testing
        /// </remarks>
        internal List<string> ActionDataList
        {
            get
            {
                var actions = beaconCache.GetActions(beaconId);
                if (actions == null)
                {
                    return new List<string>();
                }

                return actions.Select(x => x.Data).ToList();
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// create web request tag
        /// </summary>
        /// <param name="parentActionId"></param>
        /// <param name="sequenceNo"></param>
        /// <returns></returns>
        public string CreateTag(int parentActionId, int sequenceNo)
        {
            if (BeaconConfiguration.DataCollectionLevel == DataCollectionLevel.OFF)
            {
                return string.Empty;
            }

            return $"{WebRequestTagPrefix}_"
                + $"{ProtocolConstants.ProtocolVersion}_"
                + $"{httpConfiguration.ServerId}_"
                + $"{DeviceId}_"
                + $"{SessionNumber}_"
                + $"{configuration.ApplicationIdPercentEncoded}_"
                + $"{parentActionId}_"
                + $"{threadIdProvider.ThreadId}_"
                + $"{sequenceNo}";
        }

        /// <summary>
        /// Adds an <see cref="BaseAction">action</see> to this beacon.
        ///
        /// <para>
        ///     The serialized data is added to the <see cref="IBeaconCache"/>
        /// </para>
        /// </summary>
        /// <param name="action">the action to add</param>
        public void AddAction(BaseAction action)
        {
            if (CapturingDisabled)
            {
                return;
            }

            if (beaconConfiguration.DataCollectionLevel == DataCollectionLevel.OFF)
            {
                return;
            }

            var actionBuilder = new StringBuilder();

            BuildBasicEventData(actionBuilder, EventType.ACTION, action.Name);

            AddKeyValuePair(actionBuilder, BeaconKeyActionId, action.Id);
            AddKeyValuePair(actionBuilder, BeaconKeyParentActionId, action.ParentId);
            AddKeyValuePair(actionBuilder, BeaconKeyStartSequenceNumber, action.StartSequenceNo);
            AddKeyValuePair(actionBuilder, BeaconKeyTimeZero, GetTimeSinceBeaconCreation(action.StartTime));
            AddKeyValuePair(actionBuilder, BeaconKeyEndSequenceNumber, action.EndSequenceNo);
            AddKeyValuePair(actionBuilder, BeaconKeyTimeOne, action.EndTime - action.StartTime);

            AddActionData(action.StartTime, actionBuilder);
        }

        /// <summary>
        /// start the session
        /// </summary>
        public void StartSession()
        {
            if (CapturingDisabled)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.SESSION_START, null);

            AddKeyValuePair(eventBuilder, BeaconKeyParentActionId, 0);
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BeaconKeyTimeZero, 0L);

            AddEventData(sessionStartTime, eventBuilder);
        }

        /// <summary>
        /// end Session on this Beacon
        /// </summary>
        /// <param name="session"></param>
        public void EndSession(Session session)
        {
            if (CapturingDisabled)
            {
                return;
            }

            if (beaconConfiguration.DataCollectionLevel == DataCollectionLevel.OFF)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.SESSION_END, null);

            AddKeyValuePair(eventBuilder, BeaconKeyParentActionId, 0);
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BeaconKeyTimeZero, GetTimeSinceBeaconCreation(session.EndTime));

            AddEventData(session.EndTime, eventBuilder);
        }


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
        public void ReportValue(int actionId, string valueName, int value)
        {
            if (CapturingDisabled)
            {
                return;
            }

            if (beaconConfiguration.DataCollectionLevel != DataCollectionLevel.USER_BEHAVIOR)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            var eventTimestamp = BuildEvent(eventBuilder, EventType.VALUE_INT, valueName, actionId);
            AddKeyValuePair(eventBuilder, BeaconKeyValue, value);

            AddEventData(eventTimestamp, eventBuilder);
        }

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
        public void ReportValue(int actionId, string valueName, double value)
        {
            if (CapturingDisabled)
            {
                return;
            }

            if (beaconConfiguration.DataCollectionLevel != DataCollectionLevel.USER_BEHAVIOR)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            var eventTimestamp = BuildEvent(eventBuilder, EventType.VALUE_DOUBLE, valueName, actionId);
            AddKeyValuePair(eventBuilder, BeaconKeyValue, value);

            AddEventData(eventTimestamp, eventBuilder);
        }

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
        public void ReportValue(int actionId, string valueName, string value)
        {
            if (CapturingDisabled)
            {
                return;
            }

            if (beaconConfiguration.DataCollectionLevel != DataCollectionLevel.USER_BEHAVIOR)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            var eventTimestamp = BuildEvent(eventBuilder, EventType.VALUE_STRING, valueName, actionId);
            AddKeyValuePairIfValueIsNotNull(eventBuilder, BeaconKeyValue, TruncateNullSafe(value));

            AddEventData(eventTimestamp, eventBuilder);
        }

        /// <summary>
        /// Reports the given named event on the action belonging to the given ID.
        ///
        /// <para>
        ///     The serialized data is added to the <see cref="IBeaconCache"/>.
        /// </para>
        /// </summary>
        /// <param name="actionId">the identifier of the <see cref="IAction"/> on which the event was reported.</param>
        /// <param name="eventName">the name of the reported event.</param>
        public void ReportEvent(int actionId, string eventName)
        {
            if (CapturingDisabled)
            {
                return;
            }

            if (beaconConfiguration.DataCollectionLevel != DataCollectionLevel.USER_BEHAVIOR)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            var eventTimestamp = BuildEvent(eventBuilder, EventType.NAMED_EVENT, eventName, actionId);

            AddEventData(eventTimestamp, eventBuilder);
        }

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
        public void ReportError(int actionId, string errorName, int errorCode, string reason)
        {
            // if capture errors is off -> do nothing
            if (CapturingDisabled || !configuration.CaptureErrors)
            {
                return;
            }

            if (beaconConfiguration.DataCollectionLevel == DataCollectionLevel.OFF)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.ERROR, errorName);

            var timestamp = timingProvider.ProvideTimestampInMilliseconds();
            AddKeyValuePair(eventBuilder, BeaconKeyParentActionId, actionId);
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BeaconKeyTimeZero, GetTimeSinceBeaconCreation(timestamp));
            AddKeyValuePair(eventBuilder, BeaconKeyErrorCode, errorCode);
            AddKeyValuePairIfValueIsNotNull(eventBuilder, BeaconKeyErrorReason, reason);

            AddEventData(timestamp, eventBuilder);
        }

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
        public void ReportCrash(string errorName, string reason, string stacktrace)
        {
            // if capture crashes is off -> do nothing
            if (CapturingDisabled || !configuration.CaptureCrashes)
            {
                return;
            }

            if (beaconConfiguration.CrashReportingLevel != CrashReportingLevel.OPT_IN_CRASHES)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.CRASH, errorName);

            var timestamp = timingProvider.ProvideTimestampInMilliseconds();
            AddKeyValuePair(eventBuilder, BeaconKeyParentActionId, 0);                                  // no parent action
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BeaconKeyTimeZero, GetTimeSinceBeaconCreation(timestamp));
            AddKeyValuePairIfValueIsNotNull(eventBuilder, BeaconKeyErrorReason, reason);
            AddKeyValuePairIfValueIsNotNull(eventBuilder, BeaconKeyErrorStacktrace, stacktrace);

            AddEventData(timestamp, eventBuilder);
        }

        /// <summary>
        /// Adds the given traced web request on the action belonging to the given ID.
        ///
        /// <para>
        ///     The serialized data is added to the <see cref="IBeaconCache"/>.
        /// </para>
        /// </summary>
        /// <param name="parentActionId">the identifier of the <see cref="IAction"/> in which the web request was traced.</param>
        /// <param name="webRequestTracer">the traced web request.</param>
        public void AddWebRequest(int parentActionId, WebRequestTracer webRequestTracer)
        {
            if (CapturingDisabled)
            {
                return;
            }

            if (BeaconConfiguration.DataCollectionLevel == DataCollectionLevel.OFF)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.WEBREQUEST, webRequestTracer.Url);

            AddKeyValuePair(eventBuilder, BeaconKeyParentActionId, parentActionId);
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, webRequestTracer.StartSequenceNo);
            AddKeyValuePair(eventBuilder, BeaconKeyTimeZero, GetTimeSinceBeaconCreation(webRequestTracer.StartTime));
            AddKeyValuePair(eventBuilder, BeaconKeyEndSequenceNumber, webRequestTracer.EndSequenceNo);
            AddKeyValuePair(eventBuilder, BeaconKeyTimeOne, webRequestTracer.EndTime - webRequestTracer.StartTime);
            if (webRequestTracer.ResponseCode != -1)
            {
                AddKeyValuePair(eventBuilder, BeaconKeyWebRequestResponseCode, webRequestTracer.ResponseCode);
            }
            if (webRequestTracer.BytesSent > -1)
            {
                AddKeyValuePair(eventBuilder, BeaconKeyWebRequestBytesSent, webRequestTracer.BytesSent);
            }
            if (webRequestTracer.BytesReceived > -1)
            {
                AddKeyValuePair(eventBuilder, BeaconKeyWebRequestBytesReceived, webRequestTracer.BytesReceived);
            }

            AddEventData(webRequestTracer.StartTime, eventBuilder);
        }

        /// <summary>
        /// Reports a user identification event.
        ///
        /// <para>
        ///     The serialized data is added to the <see cref="IBeaconCache"/>.
        /// </para>
        /// </summary>
        /// <param name="userTag">the name/tag of the user</param>
        public void IdentifyUser(string userTag)
        {
            if (CapturingDisabled)
            {
                return;
            }

            if (beaconConfiguration.DataCollectionLevel != DataCollectionLevel.USER_BEHAVIOR)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.IDENTIFY_USER, userTag);

            var timestamp = timingProvider.ProvideTimestampInMilliseconds();
            AddKeyValuePair(eventBuilder, BeaconKeyParentActionId, 0);
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BeaconKeyTimeZero, GetTimeSinceBeaconCreation(timestamp));

            AddEventData(timestamp, eventBuilder);
        }

        /// <summary>
        /// send current state of Beacon
        /// </summary>
        /// <param name="httpClientProvider"></param>
        /// <returns></returns>
        public StatusResponse Send(IHttpClientProvider httpClientProvider)
        {
            var httpClient = httpClientProvider.CreateClient(httpConfiguration);
            StatusResponse response = null;

            while (true)
            {
                // prefix for this chunk - must be built up newly, due to changing timestamps
                var prefix = AppendMutableBeaconData(basicBeaconData);
                // subtract 1024 to ensure that the chunk does not exceed the send size configured on server side?
                // i guess that was the original intention, but i'm not sure about this
                // TODO stefan.eberl - This is a quite uncool algorithm and should be improved, avoid subtracting some "magic" number
                var chunk = beaconCache.GetNextBeaconChunk(beaconId, prefix, configuration.MaxBeaconSize - 1024, BeaconDataDelimiter);
                if (string.IsNullOrEmpty(chunk))
                {
                    // no data added so far or no data to send
                    return response;
                }

                var encodedBeacon = Encoding.UTF8.GetBytes(chunk);

                // send the request
                response = httpClient.SendBeaconRequest(clientIpAddress, encodedBeacon);
                if (response == null || response.IsErroneousResponse)
                {
                    // error happened - but don't know what exactly
                    // reset the previously retrieved chunk (restore it in internal cache) & retry another time
                    beaconCache.ResetChunkedData(beaconId);
                    break;
                }

                // worked -> remove previously retrieved chunk from cache
                beaconCache.RemoveChunkedData(beaconId);
            }

            return response;
        }

        #endregion

        #region internal methods

        internal void ClearData()
        {
            // remove all cached data for this Beacon from the cache
            beaconCache.DeleteCacheEntry(beaconId);
        }

        #endregion

        #region private methods

        /// <summary>
        /// Add previously serialized action data to the beacon cache.
        /// </summary>
        /// <param name="timestamp">The timestamp when the action data occurred.</param>
        /// <param name="actionBuilder">Contains the serialized action data.</param>
        private void AddActionData(long timestamp, StringBuilder actionBuilder)
        {
            if (configuration.IsCaptureOn)
            {
                beaconCache.AddActionData(beaconId, timestamp, actionBuilder.ToString());
            }
        }

        /// <summary>
        /// Add previously serialized event data to the beacon cache.
        /// </summary>
        /// <param name="timestamp">The timestamp when the event data occurred.</param>
        /// <param name="eventBuilder">Contains the serialized event data.</param>
        private void AddEventData(long timestamp, StringBuilder eventBuilder)
        {
            if (configuration.IsCaptureOn)
            {
                beaconCache.AddEventData(beaconId, timestamp, eventBuilder.ToString());
            }
        }

        // helper method for building events

        /// <summary>
        /// Serialization helper for event data.
        /// </summary>
        /// <param name="builder">String builder storing the serialized data.</param>
        /// <param name="eventType">The event's type.</param>
        /// <param name="name">Event name</param>
        /// <param name="parentActionId">the unique identifier of the <see cref="IAction"/> on which the event was reported.</param>
        /// <returns>The timestamp associated with the event (timestamp since session start time).</returns>
        private long BuildEvent(StringBuilder builder, EventType eventType, string name, int parentActionId)
        {
            BuildBasicEventData(builder, eventType, name);

            var eventTimestamp = timingProvider.ProvideTimestampInMilliseconds();

            AddKeyValuePair(builder, BeaconKeyParentActionId, parentActionId);
            AddKeyValuePair(builder, BeaconKeyStartSequenceNumber, NextSequenceNumber);
            AddKeyValuePair(builder, BeaconKeyTimeZero, GetTimeSinceBeaconCreation(eventTimestamp));

            return eventTimestamp;
        }

        // helper method for building basic event data
        private void BuildBasicEventData(StringBuilder builder, EventType eventType, string name)
        {
            AddKeyValuePair(builder, BeaconKeyEventType, (int)eventType);
            AddKeyValuePairIfValueIsNotNull(builder, BeaconKeyName, TruncateNullSafe(name));
            AddKeyValuePair(builder, BeaconKeyThreadId, threadIdProvider.ThreadId);
        }


        // helper method for creating basic beacon protocol data
        private string CreateBasicBeaconData()
        {
            StringBuilder basicBeaconBuilder = new StringBuilder();

            // version and application information
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyProtocolVersion, ProtocolConstants.ProtocolVersion);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyOpenKitVersion, ProtocolConstants.OpenKitVersion);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyApplicationId, configuration.ApplicationId);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyApplicationName, configuration.ApplicationName);
            AddKeyValuePairIfValueIsNotNull(basicBeaconBuilder, BeaconKeyApplicationVersion, configuration.ApplicationVersion);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyPlatformType, ProtocolConstants.PlatformTypeOpenKit);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyAgentTechnologyType, ProtocolConstants.AgentTechnologyType);

            // device/visitor ID, session number and IP address
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyVisitorId, DeviceId);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeySessionNumber, SessionNumber);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyClientIpAddress, clientIpAddress);

            // platform information
            AddKeyValuePairIfValueIsNotNull(basicBeaconBuilder, BeaconKeyDeviceOs, configuration.Device.OperatingSystem);
            AddKeyValuePairIfValueIsNotNull(basicBeaconBuilder, BeaconKeyDeviceManufacturer, configuration.Device.Manufacturer);
            AddKeyValuePairIfValueIsNotNull(basicBeaconBuilder, BeaconKeyDeviceModel, configuration.Device.ModelId);

            AddKeyValuePair(basicBeaconBuilder, BeaconKeyDataCollectionLevel, (int)beaconConfiguration.DataCollectionLevel);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyCrashReportingLevel, (int)beaconConfiguration.CrashReportingLevel);

            return basicBeaconBuilder.ToString();
        }

        private string AppendMutableBeaconData(string immutableBasicBeaconData)
        {
            var builder = new StringBuilder();

            builder.Append(immutableBasicBeaconData);
            builder.Append(BeaconDataDelimiter);

            // append timestamp data
            builder.Append(CreateTimestampData());

            // append multiplicity
            builder.Append(BeaconDataDelimiter).Append(CreateMultiplicityData());

            return builder.ToString();
        }

        // helper method for creating basic timestamp data
        private string CreateTimestampData()
        {
            StringBuilder timestampBuilder = new StringBuilder();

            AddKeyValuePair(timestampBuilder, BeaconKeyTransmissionTime, timingProvider.ProvideTimestampInMilliseconds());
            AddKeyValuePair(timestampBuilder, BeaconKeySessionStartTime, sessionStartTime);

            return timestampBuilder.ToString();
        }

        private string CreateMultiplicityData()
        {
            var builder = new StringBuilder();

            // multiplicity information
            AddKeyValuePair(builder, BeaconKeyMultiplicity, Multiplicity);

            return builder.ToString();
        }

        // helper method for adding key/value pairs with string values
        private static void AddKeyValuePair(StringBuilder builder, string key, string stringValue)
        {
            AppendKey(builder, key);
            builder.Append(PercentEncoder.Encode(stringValue, Encoding.UTF8, ReservedCharacters));
        }

        private static void AddKeyValuePairIfValueIsNotNull(StringBuilder builder, string key, string stringValue)
        {
            if (stringValue == null)
            {
                return;
            }

            AddKeyValuePair(builder, key, stringValue);
        }

        // helper method for adding key/value pairs with long values
        private static void AddKeyValuePair(StringBuilder builder, string key, long longValue)
        {
            AppendKey(builder, key);
            builder.Append(longValue);
        }

        // helper method for adding key/value pairs with int values
        private static void AddKeyValuePair(StringBuilder builder, string key, int intValue)
        {
            AppendKey(builder, key);
            builder.Append(intValue);
        }

        // helper method for adding key/value pairs with double values
        private static void AddKeyValuePair(StringBuilder builder, string key, double doubleValue)
        {
            AppendKey(builder, key);
            builder.Append(doubleValue);
        }

        // helper method for appending a key
        private static void AppendKey(StringBuilder builder, string key)
        {
            if (builder.Length > 0)
            {
                builder.Append('&');
            }
            builder.Append(key);
            builder.Append('=');
        }

        private static string TruncateNullSafe(string name)
        {
            if (name == null)
            {
                return null;
            }

            return Truncate(name);
        }

        // helper method for truncating name at max name size
        private static string Truncate(string name)
        {
            name = name.Trim();
            if (name.Length > MaximumNameLength)
            {
                name = name.Substring(0, MaximumNameLength);
            }
            return name;
        }

        private long GetTimeSinceBeaconCreation(long timestamp)
        {
            return timestamp - sessionStartTime;
        }

        #endregion
    }
}
