//
// Copyright 2018-2021 Dynatrace LLC
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
using System.Text;
using System.Threading;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Core.Util;
using Dynatrace.OpenKit.Providers;
using Dynatrace.OpenKit.Util;

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
        private const string BeaconKeySessionSequence = "ss";
        private const string BeaconKeyClientIpAddress = "ip";
        private const string BeaconKeyMultiplicity = "mp";
        private const string BeaconKeyDataCollectionLevel = "dl";
        private const string BeaconKeyCrashReportingLevel = "cl";
        private const string BeaconKeyVisitStoreVersion = "vs";

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
        private const string BeaconKeyErrorValue = "ev"; // can be an integer code or string (Exception class name)
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
        internal static readonly char[] ReservedCharacters = { '_' };

        private const char BeaconDataDelimiter = '&';

        // next ID and sequence number
        private int nextId;
        private int nextSequenceNumber;

        // session number & start time

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
        private readonly BeaconKey beaconKey;

        // this Beacon's traffic control value
        private int TrafficControlValue { get; }

        #region constructors

        /// <summary>
        /// Creates a new instance of type Beacon
        /// </summary>
        /// <param name="initializer">provider of relevant parameters to initialize / create the beacon</param>
        /// <param name="configuration">OpenKit related configuration</param>
        internal Beacon(IBeaconInitializer initializer, IBeaconConfiguration configuration)
        {
            beaconCache = initializer.BeaconCache;

            var beaconId = initializer.SessionIdProvider.GetNextSessionId();
            SessionSequenceNumber = initializer.SessionSequenceNumber;
            beaconKey = new BeaconKey(beaconId, SessionSequenceNumber);
            SessionNumber = DetermineSessionNumber(configuration, beaconId);

            this.configuration = configuration;
            threadIdProvider = initializer.ThreadIdProvider;
            timingProvider = initializer.TimingProvider;
            SessionStartTime = timingProvider.ProvideTimestampInMilliseconds();

            DeviceId = CreateDeviceId(configuration, initializer.RandomNumberGenerator);

            TrafficControlValue = initializer.RandomNumberGenerator.NextPercentageValue();

            var ipAddress = initializer.ClientIpAddress;
            if (ipAddress == null)
            {
                // A client IP address, which is a null, is valid.
                // The real IP address is determined on the server side.
                clientIpAddress = null;
            }
            else if (InetAddressValidator.IsValidIP(ipAddress))
            {
                clientIpAddress = ipAddress;
            }
            else
            {
                var logger = initializer.Logger;
                if (logger.IsWarnEnabled)
                {
                    logger.Warn($"Beacon: Client IP address validation failed: {ipAddress}");
                }
                clientIpAddress = null; // determined on server side, based on remote IP address
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

        public int SessionSequenceNumber { get; }

        public long DeviceId { get; }

        public bool IsEmpty => beaconCache.IsEmpty(beaconKey);

        /// <summary>
        /// create next ID
        /// </summary>
        public int NextId => Interlocked.Increment(ref nextId);

        /// <summary>
        /// create next sequence number
        /// </summary>
        public int NextSequenceNumber => Interlocked.Increment(ref nextSequenceNumber);

        public long SessionStartTime { get; }

        /// <summary>
        /// Get the current timestamp in milliseconds by delegating to TimingProvider
        /// </summary>
        public long CurrentTimestamp => timingProvider.ProvideTimestampInMilliseconds();

        bool IBeacon.IsDataCapturingEnabled
        {
            get
            {
                var serverConfiguration = configuration.ServerConfiguration;

                return serverConfiguration.IsSendingDataAllowed
                    && TrafficControlValue < serverConfiguration.TrafficControlPercentage;
            }
        }

        bool IBeacon.IsErrorCapturingEnabled
        {
            get
            {
                var serverConfiguration = configuration.ServerConfiguration;

                return serverConfiguration.IsSendingErrorsAllowed
                    && TrafficControlValue < serverConfiguration.TrafficControlPercentage;
            }
        }

        bool IBeacon.IsCrashCapturingEnabled
        {
            get
            {
                var serverConfiguration = configuration.ServerConfiguration;

                return serverConfiguration.IsSendingCrashesAllowed
                    && TrafficControlValue < serverConfiguration.TrafficControlPercentage;
            }
        }

        void IBeacon.InitializeServerConfiguration(IServerConfiguration serverConfiguration)
        {
            configuration.InitializeServerConfiguration(serverConfiguration);
        }

        void IBeacon.UpdateServerConfiguration(IServerConfiguration serverConfiguration)
        {
            configuration.UpdateServerConfiguration(serverConfiguration);
        }

        /// <summary>
        /// Indicates whether a server configuration is set on this beacon or not.
        /// </summary>
        bool IBeacon.IsServerConfigurationSet => configuration.IsServerConfigurationSet;

        bool IBeacon.IsActionReportingAllowedByPrivacySettings => configuration.PrivacyConfiguration.IsActionReportingAllowed;

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

        #region internal methods & properties

        string IBeacon.CreateTag(int parentActionId, int sequenceNo)
        {
            if (!configuration.PrivacyConfiguration.IsWebRequestTracingAllowed)
            {
                return string.Empty;
            }

            var openKitConfig = configuration.OpenKitConfiguration;
            var httpConfiguration = configuration.HttpClientConfiguration;
            var builder = new StringBuilder();

            builder.Append(WebRequestTagPrefix);
            builder.Append("_").Append(ProtocolConstants.ProtocolVersion.ToInvariantString());
            builder.Append("_").Append(httpConfiguration.ServerId.ToInvariantString());
            builder.Append("_").Append(DeviceId.ToInvariantString());
            builder.Append("_").Append(SessionNumber.ToInvariantString());
            if (VisitStoreVersion > 1)
            {
                builder.Append("-").Append(SessionSequenceNumber.ToInvariantString());
            }

            builder.Append("_").Append(openKitConfig.ApplicationIdPercentEncoded);
            builder.Append("_").Append(parentActionId.ToInvariantString());
            builder.Append("_").Append(threadIdProvider.ThreadId.ToInvariantString());
            builder.Append("_").Append(sequenceNo.ToInvariantString());

            return builder.ToString();
        }

        private int VisitStoreVersion => configuration.ServerConfiguration.VisitStoreVersion;

        void IBeacon.AddAction(IActionInternals action)
        {
            if (string.IsNullOrEmpty(action?.Name))
            {
                throw new ArgumentException("action is null or action.Name is null or empty");
            }

            if (!configuration.PrivacyConfiguration.IsActionReportingAllowed)
            {
                return;
            }

            if (!ThisBeacon.IsDataCapturingEnabled)
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
            if (!ThisBeacon.IsDataCapturingEnabled)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            BuildBasicEventDataWithoutName(eventBuilder, EventType.SESSION_START);

            AddKeyValuePair(eventBuilder, BeaconKeyParentActionId, 0);
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BeaconKeyTimeZero, 0L);

            AddEventData(SessionStartTime, eventBuilder);
        }

        void IBeacon.EndSession()
        {
            if (!configuration.PrivacyConfiguration.IsSessionReportingAllowed)
            {
                return;
            }

            if (!ThisBeacon.IsDataCapturingEnabled)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            BuildBasicEventDataWithoutName(eventBuilder, EventType.SESSION_END);

            var sessionEndTime = ThisBeacon.CurrentTimestamp;
            AddKeyValuePair(eventBuilder, BeaconKeyParentActionId, 0);
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BeaconKeyTimeZero, GetTimeSinceBeaconCreation(sessionEndTime));

            AddEventData(sessionEndTime, eventBuilder);
        }

        void IBeacon.ReportValue(int actionId, string valueName, int value)
        {
            ThisBeacon.ReportValue(actionId, valueName, (long)value);
        }

        void IBeacon.ReportValue(int actionId, string valueName, long value)
        {
            if (string.IsNullOrEmpty(valueName))
            {
                throw new ArgumentException("valueName is null or empty");
            }

            if (!configuration.PrivacyConfiguration.IsValueReportingAllowed)
            {
                return;
            }

            if (!ThisBeacon.IsDataCapturingEnabled)
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
            if (string.IsNullOrEmpty(valueName))
            {
                throw new ArgumentException("valueName is null or empty");
            }

            if (!configuration.PrivacyConfiguration.IsValueReportingAllowed)
            {
                return;
            }

            if (!ThisBeacon.IsDataCapturingEnabled)
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
            if (string.IsNullOrEmpty(valueName))
            {
                throw new ArgumentException("valueName is null or empty");
            }

            if (!configuration.PrivacyConfiguration.IsValueReportingAllowed)
            {
                return;
            }

            if (!ThisBeacon.IsDataCapturingEnabled)
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
            if (string.IsNullOrEmpty(eventName))
            {
                throw new ArgumentException("eventName is null or empty");
            }

            if (!configuration.PrivacyConfiguration.IsEventReportingAllowed)
            {
                return;
            }

            if (!ThisBeacon.IsDataCapturingEnabled)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            var eventTimestamp = BuildEvent(eventBuilder, EventType.NAMED_EVENT, eventName, actionId);

            AddEventData(eventTimestamp, eventBuilder);
        }

        void IBeacon.ReportError(int actionId, string errorName, int errorCode)
        {
            if (string.IsNullOrEmpty(errorName))
            {
                throw new ArgumentException("errorName is null or empty");
            }

            if (!configuration.PrivacyConfiguration.IsErrorReportingAllowed)
            {
                return;
            }

            if (!ThisBeacon.IsErrorCapturingEnabled)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.ERROR, errorName);

            var timestamp = timingProvider.ProvideTimestampInMilliseconds();
            AddKeyValuePair(eventBuilder, BeaconKeyParentActionId, actionId);
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BeaconKeyTimeZero, GetTimeSinceBeaconCreation(timestamp));
            AddKeyValuePair(eventBuilder, BeaconKeyErrorValue, errorCode);
            AddKeyValuePair(eventBuilder, BeaconKeyErrorTechnologyType, ProtocolConstants.ErrorTechnologyType);

            AddEventData(timestamp, eventBuilder);
        }

        void IBeacon.ReportError(int actionId, string errorName, string causeName, string causeDescription, string causeStackTrace)
        {
            ReportError(actionId, errorName, causeName, causeDescription, causeStackTrace, ProtocolConstants.ErrorTechnologyType);
        }

        void IBeacon.ReportError(int actionId, string errorName, Exception exception)
        {
            var crashFormatter = exception != null
                ? new CrashFormatter(exception)
                : null;

            ReportError(actionId,
                errorName,
                crashFormatter?.Name,
                crashFormatter?.Reason,
                crashFormatter?.StackTrace,
                ProtocolConstants.ErrorTechnologyType); // TODO stefan.eberl - find better technology type
        }

        private void ReportError(int actionId,
            string errorName,
            string causeName,
            string causeDescription,
            string causeStackTrace,
            string errorTechnologyType)
        {
            if (string.IsNullOrEmpty(errorName))
            {
                throw new ArgumentException("errorName is null or empty");
            }

            if (!configuration.PrivacyConfiguration.IsErrorReportingAllowed)
            {
                return;
            }

            if (!ThisBeacon.IsErrorCapturingEnabled)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.EXCEPTION, errorName);

            var timestamp = timingProvider.ProvideTimestampInMilliseconds();
            AddKeyValuePair(eventBuilder, BeaconKeyParentActionId, actionId);
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BeaconKeyTimeZero, GetTimeSinceBeaconCreation(timestamp));
            AddKeyValuePairIfValueIsNotNull(eventBuilder, BeaconKeyErrorValue, causeName);
            AddKeyValuePairIfValueIsNotNull(eventBuilder, BeaconKeyErrorReason, causeDescription);
            AddKeyValuePairIfValueIsNotNull(eventBuilder, BeaconKeyErrorStacktrace, causeStackTrace);
            AddKeyValuePair(eventBuilder, BeaconKeyErrorTechnologyType, errorTechnologyType);

            AddEventData(timestamp, eventBuilder);
        }

        void IBeacon.ReportCrash(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var crashFormatter = new CrashFormatter(exception);

            ReportCrash(crashFormatter.Name,
                crashFormatter.Reason,
                crashFormatter.StackTrace,
                ProtocolConstants.ErrorTechnologyType); // TODO stefan.eberl - report better crash technology type
        }

        void IBeacon.ReportCrash(string errorName, string reason, string stacktrace)
        {
            ReportCrash(errorName, reason, stacktrace, ProtocolConstants.ErrorTechnologyType);
        }

        private void ReportCrash(string errorName, string reason, string stacktrace, string crashTechnolgyType)
        {
            if (string.IsNullOrEmpty(errorName))
            {
                throw new ArgumentException("errorName is null or empty");
            }

            if (!configuration.PrivacyConfiguration.IsCrashReportingAllowed)
            {
                return;
            }

            if (!ThisBeacon.IsCrashCapturingEnabled)
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
            AddKeyValuePair(eventBuilder, BeaconKeyErrorTechnologyType, crashTechnolgyType);

            AddEventData(timestamp, eventBuilder);
        }

        void IBeacon.AddWebRequest(int parentActionId, IWebRequestTracerInternals webRequestTracer)
        {
            if (string.IsNullOrEmpty(webRequestTracer?.Url))
            {
                throw new ArgumentException("webRequestTracer is null or webRequestTracer.Url is null or empty");
            }

            if (!configuration.PrivacyConfiguration.IsWebRequestTracingAllowed)
            {
                return;
            }

            if (!ThisBeacon.IsDataCapturingEnabled)
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

            if (!ThisBeacon.IsDataCapturingEnabled)
            {
                return;
            }

            var eventBuilder = new StringBuilder();

            if (userTag != null)
            {
                BuildBasicEventData(eventBuilder, EventType.IDENTIFY_USER, userTag);
            }
            else
            {
                BuildBasicEventDataWithoutName(eventBuilder, EventType.IDENTIFY_USER);
            }

            var timestamp = timingProvider.ProvideTimestampInMilliseconds();
            AddKeyValuePair(eventBuilder, BeaconKeyParentActionId, 0);
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BeaconKeyTimeZero, GetTimeSinceBeaconCreation(timestamp));

            AddEventData(timestamp, eventBuilder);
        }

        IStatusResponse IBeacon.Send(IHttpClientProvider httpClientProvider, IAdditionalQueryParameters additionalParameters)
        {
            var httpClient = httpClientProvider.CreateClient(configuration.HttpClientConfiguration);
            IStatusResponse response = null;

            beaconCache.PrepareDataForSending(beaconKey);
            while (beaconCache.HasDataForSending(beaconKey))
            {
                // prefix for this chunk - must be built up newly, due to changing timestamps
                var prefix = AppendMutableBeaconData(basicBeaconData);
                // subtract 1024 to ensure that the chunk does not exceed the send size configured on server side?
                // i guess that was the original intention, but i'm not sure about this
                // TODO stefan.eberl - This is a quite uncool algorithm and should be improved, avoid subtracting some "magic" number
                var chunk = beaconCache.GetNextBeaconChunk(beaconKey, prefix, configuration.ServerConfiguration.BeaconSizeInBytes - 1024, BeaconDataDelimiter);
                if (string.IsNullOrEmpty(chunk))
                {
                    // no data added so far or no data to send
                    return response;
                }

                var encodedBeacon = Encoding.UTF8.GetBytes(chunk);

                // send the request
                response = httpClient.SendBeaconRequest(clientIpAddress, encodedBeacon, additionalParameters);
                if (response == null || response.IsErroneousResponse)
                {
                    // error happened - but don't know what exactly
                    // reset the previously retrieved chunk (restore it in internal cache) & retry another time
                    beaconCache.ResetChunkedData(beaconKey);
                    break;
                }

                // worked -> remove previously retrieved chunk from cache
                beaconCache.RemoveChunkedData(beaconKey);
            }

            return response;
        }

        void IBeacon.ClearData()
        {
            // remove all cached data for this Beacon from the cache
            beaconCache.DeleteCacheEntry(beaconKey);
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
            if (ThisBeacon.IsDataCapturingEnabled)
            {
                beaconCache.AddActionData(beaconKey, timestamp, actionBuilder.ToString());
            }
        }

        /// <summary>
        /// Add previously serialized event data to the beacon cache.
        /// </summary>
        /// <param name="timestamp">The timestamp when the event data occurred.</param>
        /// <param name="eventBuilder">Contains the serialized event data.</param>
        private void AddEventData(long timestamp, StringBuilder eventBuilder)
        {
            if (ThisBeacon.IsDataCapturingEnabled)
            {
                beaconCache.AddEventData(beaconKey, timestamp, eventBuilder.ToString());
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
            BuildBasicEventDataWithoutName(builder, eventType);
            AddKeyValuePairIfValueIsNotNull(builder, BeaconKeyName, Truncate(name));
        }

        // helper method for building basic event data that has no name
        private void BuildBasicEventDataWithoutName(StringBuilder builder, EventType eventType)
        {
            AddKeyValuePair(builder, BeaconKeyEventType, (int)eventType);
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
            AddKeyValuePairIfValueIsNotNull(basicBeaconBuilder, BeaconKeyApplicationName, openKitConfig.ApplicationName);
            AddKeyValuePairIfValueIsNotNull(basicBeaconBuilder, BeaconKeyApplicationVersion, openKitConfig.ApplicationVersion);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyPlatformType, ProtocolConstants.PlatformTypeOpenKit);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyAgentTechnologyType, ProtocolConstants.AgentTechnologyType);

            // device/visitor ID, session number and IP address
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyVisitorId, DeviceId);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeySessionNumber, SessionNumber);
            AddKeyValuePairIfValueIsNotNull(basicBeaconBuilder, BeaconKeyClientIpAddress, clientIpAddress);

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
            AddKeyValuePair(builder, BeaconKeyVisitStoreVersion, VisitStoreVersion);
            if (VisitStoreVersion > 1)
            {
                AddKeyValuePair(builder, BeaconKeySessionSequence, SessionSequenceNumber.ToInvariantString());
            }

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
            AddKeyValuePair(timestampBuilder, BeaconKeySessionStartTime, SessionStartTime);

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
            builder.Append(longValue.ToInvariantString());
        }

        // helper method for adding key/value pairs with int values
        private static void AddKeyValuePair(StringBuilder builder, string key, int intValue)
        {
            AppendKey(builder, key);
            builder.Append(intValue.ToInvariantString());
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
            builder.Append(doubleValue.ToInvariantString());
        }

        // helper method for appending a key
        private static void AppendKey(StringBuilder builder, string key)
        {
            if (builder.Length > 0)
            {
                builder.Append(BeaconDataDelimiter);
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
            return timestamp - SessionStartTime;
        }

        #endregion
    }
}
