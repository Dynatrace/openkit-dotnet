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
    internal class Beacon : IBeacon
    {
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
        private const string BeaconKeyErrorTechnologyType = "tt";

        // web request constants
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
        private int nextId;
        private int nextSequenceNumber;

        // session number & start time
        private readonly long sessionStartTime;

        // client IP address
        private readonly string clientIpAddress;

        // providers
        private readonly IThreadIdProvider threadIdProvider;
        private readonly ITimingProvider timingProvider;

        // basic beacon protocol data
        private readonly string basicBeaconData;

        // Beacon configuration
        private readonly IBeaconConfiguration configuration;

        private readonly IBeaconCache beaconCache;
        private readonly int beaconId;

        #region constructors

        /// <summary>
        /// Creates a new instance of type Beacon
        /// </summary>
        /// <param name="logger">Logger for logging messages</param>
        /// <param name="beaconCache">Cache storing beacon related data</param>
        /// <param name="configuration">OpenKit related configuration</param>
        /// <param name="clientIpAddress">The client's IP address</param>
        /// <param name="sessionIdProvider">Provider for session IDs</param>
        /// <param name="threadIdProvider">Provider for retrieving thread id</param>
        /// <param name="timingProvider">Provider for time related methods</param>
        internal Beacon(
            ILogger logger,
            IBeaconCache beaconCache,
            IBeaconConfiguration configuration,
            string clientIpAddress,
            ISessionIdProvider sessionIdProvider,
            IThreadIdProvider threadIdProvider,
            ITimingProvider timingProvider
            )
            : this(
                logger,
                beaconCache,
                configuration,
                clientIpAddress,
                sessionIdProvider,
                threadIdProvider,
                timingProvider,
                new DefaultPrnGenerator()
                )
        {
        }

        /// <summary>
        /// Creates a new instance of type Beacon
        /// </summary>
        /// <param name="logger">Logger for logging messages</param>
        /// <param name="beaconCache">Cache storing beacon related data</param>
        /// <param name="configuration">OpenKit related configuration</param>
        /// <param name="clientIpAddress">The client's IP address</param>
        /// <param name="sessionIdProvider">Provider for session IDs</param>
        /// <param name="threadIdProvider">Provider for retrieving thread id</param>
        /// <param name="timingProvider">Provider for time related methods</param>
        /// <param name="randomNumberGenerator">Random number generator</param>
        internal Beacon(
            ILogger logger,
            IBeaconCache beaconCache,
            IBeaconConfiguration configuration,
            string clientIpAddress,
            ISessionIdProvider sessionIdProvider,
            IThreadIdProvider threadIdProvider,
            ITimingProvider timingProvider,
            IPrnGenerator randomNumberGenerator
            )
        {
            this.beaconCache = beaconCache;
            beaconId = sessionIdProvider.GetNextSessionId();
            SessionNumber = DetermineSessionNumber(configuration, beaconId);

            this.timingProvider = timingProvider;
            this.configuration = configuration;
            this.threadIdProvider = threadIdProvider;
            sessionStartTime = timingProvider.ProvideTimestampInMilliseconds();

            DeviceId = CreateDeviceId(configuration, randomNumberGenerator);

            if (clientIpAddress == null)
            {
                // A client IP address, which is a null, is valid.
                // The real IP address is determined on the server side.
                this.clientIpAddress = string.Empty;
            }
            else if (InetAddressValidator.IsValidIP(clientIpAddress))
            {
                this.clientIpAddress = clientIpAddress;
            }
            else
            {
                if (logger.IsWarnEnabled)
                {
                    logger.Warn($"Beacon: Client IP address validation failed: {clientIpAddress}");
                }
                this.clientIpAddress = string.Empty;
            }

            basicBeaconData = CreateBasicBeaconData();
        }

        private static long CreateDeviceId(IBeaconConfiguration configuration, IPrnGenerator randomGenerator)
        {
            if (configuration.PrivacyConfiguration.IsDeviceIdSendingAllowed)
            {
                return configuration.OpenKitConfiguration.DeviceId;
            }

            return randomGenerator.NextPositiveLong();
        }

        private static int DetermineSessionNumber(IBeaconConfiguration configuration, int beaconId)
        {
            if (configuration.PrivacyConfiguration.IsSessionNumberReportingAllowed)
            {
                return beaconId;
            }

            return 1;
        }

        #endregion

        private IBeacon ThisBeacon => this;

        #region IBeacon implementation
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

        bool IBeacon.IsCaptureEnabled => configuration.ServerConfiguration.IsCaptureEnabled;

        void IBeacon.UpdateServerConfiguration(IServerConfiguration serverConfiguration)
        {
            configuration.UpdateServerConfiguration(serverConfiguration);
        }

        /// <summary>
        /// Indicates whether a server configuration is set on this beacon or not.
        /// </summary>
        bool IBeacon.IsServerConfigurationSet => configuration.IsServerConfigurationSet;

        void IBeacon.EnableCapture()
        {
            configuration.EnableCapture();
        }

        void IBeacon.DisableCapture()
        {
            configuration.DisableCapture();
        }

        event ServerConfigurationUpdateCallback IBeacon.OnServerConfigurationUpdate
        {
            add => configuration.OnServerConfigurationUpdate += value;
            remove => configuration.OnServerConfigurationUpdate -= value;
        }

        #endregion

        #region internal methods

        string IBeacon.CreateTag(int parentActionId, int sequenceNo)
        {
            if (!configuration.PrivacyConfiguration.IsWebRequestTracingAllowed)
            {
                return string.Empty;
            }

            var openKitConfig = configuration.OpenKitConfiguration;
            var httpConfiguration = configuration.HttpClientConfiguration;
            return $"{WebRequestTagPrefix}_"
                + $"{ProtocolConstants.ProtocolVersion}_"
                + $"{httpConfiguration.ServerId}_"
                + $"{DeviceId}_"
                + $"{SessionNumber}_"
                + $"{openKitConfig.ApplicationIdPercentEncoded}_"
                + $"{parentActionId}_"
                + $"{threadIdProvider.ThreadId}_"
                + $"{sequenceNo}";
        }

        void IBeacon.AddAction(IActionInternals action)
        {
            if (!configuration.PrivacyConfiguration.IsActionReportingAllowed)
            {
                return;
            }

            if (!configuration.ServerConfiguration.IsSendingDataAllowed)
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

        void IBeacon.StartSession()
        {
            if (!ThisBeacon.IsCaptureEnabled)
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

        void IBeacon.EndSession()
        {
            if (!configuration.PrivacyConfiguration.IsSessionReportingAllowed)
            {
                return;
            }

            if (!configuration.ServerConfiguration.IsSendingDataAllowed)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.SESSION_END, null);

            var sessionEndTime = ThisBeacon.CurrentTimestamp;
            AddKeyValuePair(eventBuilder, BeaconKeyParentActionId, 0);
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BeaconKeyTimeZero, GetTimeSinceBeaconCreation(sessionEndTime));

            AddEventData(sessionEndTime, eventBuilder);
        }

        void IBeacon.ReportValue(int actionId, string valueName, int value)
        {
            if (!configuration.PrivacyConfiguration.IsValueReportingAllowed)
            {
                return;
            }

            if (!configuration.ServerConfiguration.IsSendingDataAllowed)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            var eventTimestamp = BuildEvent(eventBuilder, EventType.VALUE_INT, valueName, actionId);
            AddKeyValuePair(eventBuilder, BeaconKeyValue, value);

            AddEventData(eventTimestamp, eventBuilder);
        }

        void IBeacon.ReportValue(int actionId, string valueName, double value)
        {
            if (!configuration.PrivacyConfiguration.IsValueReportingAllowed)
            {
                return;
            }

            if (!configuration.ServerConfiguration.IsSendingDataAllowed)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            var eventTimestamp = BuildEvent(eventBuilder, EventType.VALUE_DOUBLE, valueName, actionId);
            AddKeyValuePair(eventBuilder, BeaconKeyValue, value);

            AddEventData(eventTimestamp, eventBuilder);
        }

        void IBeacon.ReportValue(int actionId, string valueName, string value)
        {
            if (!configuration.PrivacyConfiguration.IsValueReportingAllowed)
            {
                return;
            }

            if (!configuration.ServerConfiguration.IsSendingDataAllowed)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            var eventTimestamp = BuildEvent(eventBuilder, EventType.VALUE_STRING, valueName, actionId);
            AddKeyValuePairIfValueIsNotNull(eventBuilder, BeaconKeyValue, TruncateNullSafe(value));

            AddEventData(eventTimestamp, eventBuilder);
        }

        void IBeacon.ReportEvent(int actionId, string eventName)
        {
            if (!configuration.PrivacyConfiguration.IsEventReportingAllowed)
            {
                return;
            }

            if (!configuration.ServerConfiguration.IsSendingDataAllowed)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            var eventTimestamp = BuildEvent(eventBuilder, EventType.NAMED_EVENT, eventName, actionId);

            AddEventData(eventTimestamp, eventBuilder);
        }

        void IBeacon.ReportError(int actionId, string errorName, int errorCode, string reason)
        {
            if (!configuration.PrivacyConfiguration.IsErrorReportingAllowed)
            {
                return;
            }

            if (!configuration.ServerConfiguration.IsSendingErrorsAllowed)
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
            AddKeyValuePair(eventBuilder, BeaconKeyErrorTechnologyType, ProtocolConstants.ErrorTechnologyType);

            AddEventData(timestamp, eventBuilder);
        }

        void IBeacon.ReportCrash(string errorName, string reason, string stacktrace)
        {
            if (!configuration.PrivacyConfiguration.IsCrashReportingAllowed)
            {
                return;
            }

            if (!configuration.ServerConfiguration.IsSendingCrashesAllowed)
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
            AddKeyValuePair(eventBuilder, BeaconKeyErrorTechnologyType, ProtocolConstants.ErrorTechnologyType);

            AddEventData(timestamp, eventBuilder);
        }

        void IBeacon.AddWebRequest(int parentActionId, IWebRequestTracerInternals webRequestTracer)
        {
            if (!configuration.PrivacyConfiguration.IsWebRequestTracingAllowed)
            {
                return;
            }

            if (!ThisBeacon.IsCaptureEnabled)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.WEB_REQUEST, webRequestTracer.Url);

            AddKeyValuePair(eventBuilder, BeaconKeyParentActionId, parentActionId);
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, webRequestTracer.StartSequenceNo);
            AddKeyValuePair(eventBuilder, BeaconKeyTimeZero, GetTimeSinceBeaconCreation(webRequestTracer.StartTime));
            AddKeyValuePair(eventBuilder, BeaconKeyEndSequenceNumber, webRequestTracer.EndSequenceNo);
            AddKeyValuePair(eventBuilder, BeaconKeyTimeOne, webRequestTracer.EndTime - webRequestTracer.StartTime);

            AddKeyValuePairIfNotNegative(eventBuilder, BeaconKeyWebRequestBytesSent, webRequestTracer.BytesSent);
            AddKeyValuePairIfNotNegative(eventBuilder, BeaconKeyWebRequestBytesReceived, webRequestTracer.BytesReceived);
            AddKeyValuePairIfNotNegative(eventBuilder, BeaconKeyWebRequestResponseCode, webRequestTracer.ResponseCode);

            AddEventData(webRequestTracer.StartTime, eventBuilder);
        }

        void IBeacon.IdentifyUser(string userTag)
        {
            if (!configuration.PrivacyConfiguration.IsUserIdentificationIsAllowed)
            {
                return;
            }

            if (!ThisBeacon.IsCaptureEnabled)
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

        IStatusResponse IBeacon.Send(IHttpClientProvider httpClientProvider)
        {
            var httpClient = httpClientProvider.CreateClient(configuration.HttpClientConfiguration);
            IStatusResponse response = null;

            while (true)
            {
                // prefix for this chunk - must be built up newly, due to changing timestamps
                var prefix = AppendMutableBeaconData(basicBeaconData);
                // subtract 1024 to ensure that the chunk does not exceed the send size configured on server side?
                // i guess that was the original intention, but i'm not sure about this
                // TODO stefan.eberl - This is a quite uncool algorithm and should be improved, avoid subtracting some "magic" number
                var chunk = beaconCache.GetNextBeaconChunk(beaconId, prefix, configuration.ServerConfiguration.BeaconSizeInBytes - 1024, BeaconDataDelimiter);
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

        void IBeacon.ClearData()
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
            if (ThisBeacon.IsCaptureEnabled)
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
            if (ThisBeacon.IsCaptureEnabled)
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
            var openKitConfig = configuration.OpenKitConfiguration;
            var basicBeaconBuilder = new StringBuilder();

            // version and application information
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyProtocolVersion, ProtocolConstants.ProtocolVersion);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyOpenKitVersion, ProtocolConstants.OpenKitVersion);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyApplicationId, openKitConfig.ApplicationId);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyApplicationName, openKitConfig.ApplicationName);
            AddKeyValuePairIfValueIsNotNull(basicBeaconBuilder, BeaconKeyApplicationVersion, openKitConfig.ApplicationVersion);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyPlatformType, ProtocolConstants.PlatformTypeOpenKit);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyAgentTechnologyType, ProtocolConstants.AgentTechnologyType);

            // device/visitor ID, session number and IP address
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyVisitorId, DeviceId);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeySessionNumber, SessionNumber);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyClientIpAddress, clientIpAddress);

            // platform information
            AddKeyValuePairIfValueIsNotNull(basicBeaconBuilder, BeaconKeyDeviceOs, openKitConfig.OperatingSystem);
            AddKeyValuePairIfValueIsNotNull(basicBeaconBuilder, BeaconKeyDeviceManufacturer, openKitConfig.Manufacturer);
            AddKeyValuePairIfValueIsNotNull(basicBeaconBuilder, BeaconKeyDeviceModel, openKitConfig.ModelId);

            var privacyConfig = configuration.PrivacyConfiguration;
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyDataCollectionLevel, (int)privacyConfig.DataCollectionLevel);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyCrashReportingLevel, (int)privacyConfig.CrashReportingLevel);

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
            var timestampBuilder = new StringBuilder();

            AddKeyValuePair(timestampBuilder, BeaconKeyTransmissionTime, timingProvider.ProvideTimestampInMilliseconds());
            AddKeyValuePair(timestampBuilder, BeaconKeySessionStartTime, sessionStartTime);

            return timestampBuilder.ToString();
        }

        private string CreateMultiplicityData()
        {
            var builder = new StringBuilder();

            var multiplicity = configuration.ServerConfiguration.Multiplicity;
            AddKeyValuePair(builder, BeaconKeyMultiplicity, multiplicity);

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

        /// <summary>
        /// Serialization helper method for adding key/value pairs with int values.
        ///
        /// <para>
        /// The key value pair is only added if the given <paramref name="intValue"/> is not negative.
        /// </para>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="key"></param>
        /// <param name="intValue"></param>
        private static void AddKeyValuePairIfNotNegative(StringBuilder builder, string key, int intValue)
        {
            if (intValue >= 0)
            {
                AddKeyValuePair(builder, key, intValue);
            }
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
