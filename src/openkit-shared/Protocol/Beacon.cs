﻿//
// Copyright 2018 Dynatrace LLC
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
using Dynatrace.OpenKit.Core;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;
using Dynatrace.OpenKit.Core.Util;
using Dynatrace.OpenKit.Core.Caching;

using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;

namespace Dynatrace.OpenKit.Protocol
{

    /// <summary>
    ///  The Beacon class holds all the beacon data and the beacon protocol implementation.
    /// </summary>
    public class Beacon
    {
        // Initial time to sleep after the first failed beacon send attempt.
        private const int INITIAL_RETRY_SLEEP_TIME_MILLISECONDS = 1000;


        // basic data constants
        private const string BEACON_KEY_PROTOCOL_VERSION = "vv";
        private const string BEACON_KEY_OPENKIT_VERSION = "va";
        private const string BEACON_KEY_APPLICATION_ID = "ap";
        private const string BEACON_KEY_APPLICATION_NAME = "an";
        private const string BEACON_KEY_APPLICATION_VERSION = "vn";
        private const string BEACON_KEY_PLATFORM_TYPE = "pt";
        private const string BEACON_KEY_AGENT_TECHNOLOGY_TYPE = "tt";
        private const string BEACON_KEY_VISITOR_ID = "vi";
        private const string BEACON_KEY_SESSION_NUMBER = "sn";
        private const string BEACON_KEY_CLIENT_IP_ADDRESS = "ip";
        private const string BEACON_KEY_MULTIPLICITY = "mp";

        // device data constants
        private const string BEACON_KEY_DEVICE_OS = "os";
        private const string BEACON_KEY_DEVICE_MANUFACTURER = "mf";
        private const string BEACON_KEY_DEVICE_MODEL = "md";

        // timestamp constants
        private const string BEACON_KEY_SESSION_START_TIME = "tv";
        private const string BEACON_KEY_TIMESYNC_TIME = "ts";
        private const string BEACON_KEY_TRANSMISSION_TIME = "tx";

        // Action related constants
        private const string BEACON_KEY_EVENT_TYPE = "et";
        private const string BEACON_KEY_NAME = "na";
        private const string BEACON_KEY_THREAD_ID = "it";
        private const string BEACON_KEY_ACTION_ID = "ca";
        private const string BEACON_KEY_PARENT_ACTION_ID = "pa";
        private const string BEACON_KEY_START_SEQUENCE_NUMBER = "s0";
        private const string BEACON_KEY_TIME_0 = "t0";
        private const string BEACON_KEY_END_SEQUENCE_NUMBER = "s1";
        private const string BEACON_KEY_TIME_1 = "t1";

        // data and error capture constants
        private const string BEACON_KEY_VALUE = "vl";
        private const string BEACON_KEY_ERROR_CODE = "ev";
        private const string BEACON_KEY_ERROR_REASON = "rs";
        private const string BEACON_KEY_ERROR_STACKTRACE = "st";
        private const string BEACON_KEY_WEBREQUEST_RESPONSECODE = "rc";
        private const string BEACON_KEY_WEBREQUEST_BYTES_SEND = "bs";
        private const string BEACON_KEY_WEBREQUEST_BYTES_RECEIVED = "br";

        // max name length
        private const int MAX_NAME_LEN = 250;

        // web request tag prefix constant
        private const string TAG_PREFIX = "MT";

        private const char BEACON_DATA_DELIMITER = '&';

        // next ID and sequence number
        private int nextID = 0;
        private int nextSequenceNumber = 0;

        // session number & start time
        private readonly long sessionStartTime;

        // client IP address
        private readonly string clientIPAddress;

        // providers
        private readonly IThreadIDProvider threadIDProvider;
        private readonly ITimingProvider timingProvider;
        private readonly IPRNGenerator randomNumberGenerator;

        // configuration
        private readonly HTTPClientConfiguration httpConfiguration;

        // basic beacon protocol data
        private readonly string basicBeaconData;

        // Configuration reference
        private readonly OpenKitConfiguration configuration;

        // Beacon configuration
        private volatile BeaconConfiguration beaconConfiguration;

        private readonly ILogger logger;

        private readonly BeaconCache beaconCache;

        // *** constructor ***

        /// <summary>
        /// Creates a new instance of type Beacon
        /// </summary>
        /// <param name="logger">Logger for logging messages</param>
        /// <param name="cache">Cache storing beacon related data</param>
        /// <param name="configuration">OpenKit related configuration</param>
        /// <param name="clientIPAddress">The client's IP address</param>
        /// <param name="threadIdProvider">Provider for retrieving thread id</param>
        /// <param name="timingProvider">Provider for time related methods</param>
        public Beacon(ILogger logger, BeaconCache beaconCache, OpenKitConfiguration configuration, string clientIPAddress,
            IThreadIDProvider threadIDProvider, ITimingProvider timingProvider)
            : this(logger, beaconCache, configuration, clientIPAddress, threadIDProvider, timingProvider, new DefaultPRNGenerator())
        {
        }

        /// <summary>
        /// Creates a new instance of type Beacon
        /// </summary>
        /// <param name="logger">Logger for logging messages</param>
        /// <param name="cache">Cache storing beacon related data</param>
        /// <param name="configuration">OpenKit related configuration</param>
        /// <param name="clientIPAddress">The client's IP address</param>
        /// <param name="threadIdProvider">Provider for retrieving thread id</param>
        /// <param name="timingProvider">Provider for time related methods</param>
        /// <param name="randomNumberGenerator">Random number generator</param>
        internal Beacon(ILogger logger, BeaconCache beaconCache, OpenKitConfiguration configuration, string clientIPAddress,
            IThreadIDProvider threadIDProvider, ITimingProvider timingProvider, IPRNGenerator randomNumberGenerator)
        {
            this.logger = logger;
            this.beaconCache = beaconCache;
            BeaconConfiguration = configuration.BeaconConfig;

            if (beaconConfiguration.DataCollectionLevel == DataCollectionLevel.USER_BEHAVIOR)
            {
                SessionNumber = configuration.NextSessionNumber;
                DeviceID = configuration.DeviceID;
            }
            else
            {
                SessionNumber = 1;
                DeviceID = randomNumberGenerator.NextLong(long.MaxValue);
            }

            this.timingProvider = timingProvider;

            this.configuration = configuration;
            this.threadIDProvider = threadIDProvider;
            this.randomNumberGenerator = randomNumberGenerator;
            sessionStartTime = timingProvider.ProvideTimestampInMilliseconds();

            if (InetAddressValidator.IsValidIP(clientIPAddress))
            {
                this.clientIPAddress = clientIPAddress;
            }
            else
            {
                this.clientIPAddress = string.Empty;
            }
            // store the current http configuration
            httpConfiguration = configuration.HTTPClientConfig;
            basicBeaconData = CreateBasicBeaconData();
        }
        // *** public properties ***
        public int SessionNumber { get; }

        public long DeviceID { get; }

        public bool IsEmpty => beaconCache.IsEmpty(SessionNumber);

        /// <summary>
        /// create next ID
        /// </summary>
        public int NextID => Interlocked.Increment(ref nextID);

        /// <summary>
        /// create next sequence number
        /// </summary>
        public int NextSequenceNumber => Interlocked.Increment(ref nextSequenceNumber);

        /// <summary>
        /// Get the current timestamp in milliseconds by delegating to TimingProvider
        /// </summary>
        public long CurrentTimestamp => timingProvider.ProvideTimestampInMilliseconds();

        internal BeaconConfiguration BeaconConfiguration
        {
            set
            {
                if (value != null)
                {
                    beaconConfiguration = value;
                }
            }
            get
            {
                return beaconConfiguration;
            }
        }

        internal int Multiplicity => BeaconConfiguration.Multiplicity;

        internal bool CapturingDisabled => !BeaconConfiguration.CapturingAllowed;

        /// <summary>
        /// Returns an immutable list of the event data
        /// 
        /// Used for testing
        /// </summary>
        internal List<string> EventDataList
        {
            get
            {
                var events = beaconCache.GetEvents(SessionNumber);
                if (events == null)
                {                    
                    return new List<string>();
                }

                return events.Select(x => x.Data).ToList();
            }
        }

        /// <summary>
        /// Returns an immutable list of the action data
        /// 
        /// Used for testing
        /// </summary>
        internal List<string> ActionDataList
        {
            get
            {
                var actions = beaconCache.GetActions(SessionNumber);
                if (actions == null)
                {
                    return new List<string>();
                }                   

                return actions.Select(x => x.Data).ToList();
            }
        }

        // *** public methods ***

        /// <summary>
        /// create web request tag
        /// </summary>
        /// <param name="parentAction"></param>
        /// <param name="sequenceNo"></param>
        /// <returns></returns>
        public string CreateTag(Action parentAction, int sequenceNo)
        {
            if (BeaconConfiguration.DataCollectionLevel == DataCollectionLevel.OFF)
            {
                return string.Empty;
            }

            return TAG_PREFIX + "_"
                       + ProtocolConstants.PROTOCOL_VERSION + "_"
                       + httpConfiguration.ServerID + "_"
                       + configuration.DeviceID + "_"
                       + SessionNumber + "_"
                       + configuration.ApplicationID + "_"
                       + parentAction.ID + "_"
                       + threadIDProvider.ThreadID + "_"
                       + sequenceNo;
        }

        /// <summary>
        /// add an Action to this Beacon
        /// </summary>
        /// <param name="action"></param>
        public void AddAction(Action action)
        {
            if (CapturingDisabled)
            {
                return;
            }

            if (beaconConfiguration.DataCollectionLevel == DataCollectionLevel.OFF)
            {
                return;
            }

            StringBuilder actionBuilder = new StringBuilder();

            BuildBasicEventData(actionBuilder, EventType.ACTION, action.Name);

            AddKeyValuePair(actionBuilder, BEACON_KEY_ACTION_ID, action.ID);
            AddKeyValuePair(actionBuilder, BEACON_KEY_PARENT_ACTION_ID, action.ParentID);
            AddKeyValuePair(actionBuilder, BEACON_KEY_START_SEQUENCE_NUMBER, action.StartSequenceNo);
            AddKeyValuePair(actionBuilder, BEACON_KEY_TIME_0, GetTimeSinceBeaconCreation(action.StartTime));
            AddKeyValuePair(actionBuilder, BEACON_KEY_END_SEQUENCE_NUMBER, action.EndSequenceNo);
            AddKeyValuePair(actionBuilder, BEACON_KEY_TIME_1, action.EndTime - action.StartTime);

            AddActionData(action.StartTime, actionBuilder);
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

            StringBuilder eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.SESSION_END, null);

            AddKeyValuePair(eventBuilder, BEACON_KEY_PARENT_ACTION_ID, 0);
            AddKeyValuePair(eventBuilder, BEACON_KEY_START_SEQUENCE_NUMBER, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BEACON_KEY_TIME_0, GetTimeSinceBeaconCreation(session.EndTime));

            AddEventData(session.EndTime, eventBuilder);
        }


        /// <summary>
        /// report int value on the provided Action
        /// </summary>
        /// <param name="parentAction"></param>
        /// <param name="valueName"></param>
        /// <param name="value"></param>
        public void ReportValue(Action parentAction, string valueName, int value)
        {
            if (CapturingDisabled)
            {
                return;
            }

            StringBuilder eventBuilder = new StringBuilder();

            var eventTimestamp = BuildEvent(eventBuilder, EventType.VALUE_INT, valueName, parentAction);
            AddKeyValuePair(eventBuilder, BEACON_KEY_VALUE, value);

            AddEventData(eventTimestamp, eventBuilder);
        }

        /// <summary>
        /// report double value on the provided Action
        /// </summary>
        /// <param name="parentAction"></param>
        /// <param name="valueName"></param>
        /// <param name="value"></param>
        public void ReportValue(Action parentAction, string valueName, double value)
        {
            if (CapturingDisabled)
            {
                return;
            }

            StringBuilder eventBuilder = new StringBuilder();

            var eventTimestamp = BuildEvent(eventBuilder, EventType.VALUE_DOUBLE, valueName, parentAction);
            AddKeyValuePair(eventBuilder, BEACON_KEY_VALUE, value);

            AddEventData(eventTimestamp, eventBuilder);
        }

        /// <summary>
        /// report string value on the provided Action
        /// </summary>
        /// <param name="parentAction"></param>
        /// <param name="valueName"></param>
        /// <param name="value"></param>        
        public void ReportValue(Action parentAction, string valueName, string value)
        {
            if (CapturingDisabled)
            {
                return;
            }

            StringBuilder eventBuilder = new StringBuilder();

            var eventTimestamp = BuildEvent(eventBuilder, EventType.VALUE_STRING, valueName, parentAction);
            AddKeyValuePairIfValueIsNotNull(eventBuilder, BEACON_KEY_VALUE, TruncateNullSafe(value));

            AddEventData(eventTimestamp, eventBuilder);
        }

        /// <summary>
        /// report named event on the provided Action
        /// </summary>
        /// <param name="parentAction"></param>
        /// <param name="eventName"></param>
        public void ReportEvent(Action parentAction, string eventName)
        {
            if (CapturingDisabled)
            {
                return;
            }

            StringBuilder eventBuilder = new StringBuilder();

            var eventTimestamp = BuildEvent(eventBuilder, EventType.NAMED_EVENT, eventName, parentAction);

            AddEventData(eventTimestamp, eventBuilder);
        }

        /// <summary>
        /// report error on the provided Action
        /// </summary>
        /// <param name="parentAction"></param>
        /// <param name="errorName"></param>
        /// <param name="errorCode"></param>
        /// <param name="reason"></param>
        public void ReportError(Action parentAction, string errorName, int errorCode, string reason)
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

            StringBuilder eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.ERROR, errorName);

            var timestamp = timingProvider.ProvideTimestampInMilliseconds();
            AddKeyValuePair(eventBuilder, BEACON_KEY_PARENT_ACTION_ID, parentAction.ID);
            AddKeyValuePair(eventBuilder, BEACON_KEY_START_SEQUENCE_NUMBER, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BEACON_KEY_TIME_0, GetTimeSinceBeaconCreation(timestamp));
            AddKeyValuePair(eventBuilder, BEACON_KEY_ERROR_CODE, errorCode);
            AddKeyValuePairIfValueIsNotNull(eventBuilder, BEACON_KEY_ERROR_REASON, reason);

            AddEventData(timestamp, eventBuilder);
        }

        /// <summary>
        /// report a crash
        /// </summary>
        /// <param name="errorName"></param>
        /// <param name="reason"></param>
        /// <param name="stacktrace"></param>
        public void ReportCrash(string errorName, string reason, string stacktrace)
        {
            // if capture crashes is off -> do nothing
            if (CapturingDisabled || !configuration.CaptureCrashes)
            {
                return;
            }

            StringBuilder eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.CRASH, errorName);

            var timestamp = timingProvider.ProvideTimestampInMilliseconds();
            AddKeyValuePair(eventBuilder, BEACON_KEY_PARENT_ACTION_ID, 0);                                  // no parent action
            AddKeyValuePair(eventBuilder, BEACON_KEY_START_SEQUENCE_NUMBER, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BEACON_KEY_TIME_0, GetTimeSinceBeaconCreation(timestamp));
            AddKeyValuePairIfValueIsNotNull(eventBuilder, BEACON_KEY_ERROR_REASON, reason);
            AddKeyValuePairIfValueIsNotNull(eventBuilder, BEACON_KEY_ERROR_STACKTRACE, stacktrace);

            AddEventData(timestamp, eventBuilder);
        }

        /// <summary>
        /// add web request to the provided Action
        /// </summary>
        /// <param name="parentAction"></param>
        /// <param name="webRequestTracer"></param>
        public void AddWebRequest(Action parentAction, WebRequestTracerBase webRequestTracer)
        {
            if (CapturingDisabled)
            {
                return;
            }

            if (BeaconConfiguration.DataCollectionLevel == DataCollectionLevel.OFF)
            {
                return;
            }

            StringBuilder eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.WEBREQUEST, webRequestTracer.URL);

            AddKeyValuePair(eventBuilder, BEACON_KEY_PARENT_ACTION_ID, parentAction.ID);
            AddKeyValuePair(eventBuilder, BEACON_KEY_START_SEQUENCE_NUMBER, webRequestTracer.StartSequenceNo);
            AddKeyValuePair(eventBuilder, BEACON_KEY_TIME_0, GetTimeSinceBeaconCreation(webRequestTracer.StartTime));
            AddKeyValuePair(eventBuilder, BEACON_KEY_END_SEQUENCE_NUMBER, webRequestTracer.EndSequenceNo);
            AddKeyValuePair(eventBuilder, BEACON_KEY_TIME_1, webRequestTracer.EndTime - webRequestTracer.StartTime);
            if (webRequestTracer.ResponseCode != -1)
            {
                AddKeyValuePair(eventBuilder, BEACON_KEY_WEBREQUEST_RESPONSECODE, webRequestTracer.ResponseCode);
            }
            if (webRequestTracer.BytesSent > -1)
            {
                AddKeyValuePair(eventBuilder, BEACON_KEY_WEBREQUEST_BYTES_SEND, webRequestTracer.BytesSent);
            }
            if (webRequestTracer.BytesReceived > -1)
            {
                AddKeyValuePair(eventBuilder, BEACON_KEY_WEBREQUEST_BYTES_RECEIVED, webRequestTracer.BytesReceived);
            }

            AddEventData(webRequestTracer.StartTime, eventBuilder);
        }

        /// <summary>
        /// Identify the user
        /// </summary>
        /// <param name="userTag"></param>
        public void IdentifyUser(string userTag)
        {
            if (CapturingDisabled)
            {
                return;
            }

            StringBuilder eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.IDENTIFY_USER, userTag);

            var timestamp = timingProvider.ProvideTimestampInMilliseconds();
            AddKeyValuePair(eventBuilder, BEACON_KEY_PARENT_ACTION_ID, 0);
            AddKeyValuePair(eventBuilder, BEACON_KEY_START_SEQUENCE_NUMBER, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BEACON_KEY_TIME_0, GetTimeSinceBeaconCreation(timestamp));

            AddEventData(timestamp, eventBuilder);
        }

        /// <summary>
        /// send current state of Beacon
        /// </summary>
        /// <param name="httpClientProvider"></param>
        /// <param name="numRetries"></param>
        /// <returns></returns>
        public StatusResponse Send(IHTTPClientProvider httpClientProvider)
        {
            var httpClient = httpClientProvider.CreateClient(httpConfiguration);
            StatusResponse response = null;

            while (true)
            {
                // prefix for this chunk - must be built up newly, due to changing timestamps
                var prefix = basicBeaconData + BEACON_DATA_DELIMITER + CreateTimestampData();
                // subtract 1024 to ensure that the chunk does not exceed the send size configured on server side?
                // i guess that was the original intention, but i'm not sure about this
                // TODO stefan.eberl - This is a quite uncool algorithm and should be improved, avoid subtracting some "magic" number
                var chunk = beaconCache.GetNextBeaconChunk(SessionNumber, prefix, configuration.MaxBeaconSize - 1024, BEACON_DATA_DELIMITER);
                if (string.IsNullOrEmpty(chunk))
                {
                    // no data added so far or no data to send
                    return response;
                }

                byte[] encodedBeacon = Encoding.UTF8.GetBytes(chunk);

                // send the request
                response = httpClient.SendBeaconRequest(clientIPAddress, encodedBeacon);
                if (response == null)
                {
                    // error happened - but don't know what exactly
                    // reset the previously retrieved chunk (restore it in internal cache) & retry another time
                    beaconCache.ResetChunkedData(SessionNumber);
                    break;
                }
                else
                {
                    // worked -> remove previously retrieved chunk from cache
                    beaconCache.RemoveChunkedData(SessionNumber);
                }
            }

            return response;
        }

        internal void ClearData()
        {
            // remove all cached data for this Beacon from the cache
            beaconCache.DeleteCacheEntry(SessionNumber);
        }

        // *** private methods ***

        /// <summary>
        /// Add previously serialized action data to the beacon cache.
        /// </summary>
        /// <param name="timestamp">The timestamp when the action data occurred.</param>
        /// <param name="actionBuilder">Contains the serialized action data.</param>
        private void AddActionData(long timestamp, StringBuilder actionBuilder)
        {
            if (configuration.IsCaptureOn)
            {
                beaconCache.AddActionData(SessionNumber, timestamp, actionBuilder.ToString());
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
                beaconCache.AddEventData(SessionNumber, timestamp, eventBuilder.ToString());
            }
        }

        // helper method for beacon sending with retries
        private StatusResponse SendBeaconRequest(IHTTPClient httpClient, byte[] beaconData, int numRetries)
        {
            StatusResponse response = null;
            var retry = 0;
            var retrySleepMillis = INITIAL_RETRY_SLEEP_TIME_MILLISECONDS;

            while (true)
            {
                response = httpClient.SendBeaconRequest(clientIPAddress, beaconData);
                if (response != null || (retry >= numRetries))
                {
                    // success OR max retry count reached
                    break;
                }

                timingProvider.Sleep(retrySleepMillis);
                retrySleepMillis *= 2;
                retry++;
            }

            return response;
        }

        // helper method for building events

        /// <summary>
        /// Serialization helper for event data.
        /// </summary>
        /// <param name="builder">String builder storing the serialized data.</param>
        /// <param name="eventType">The event's type.</param>
        /// <param name="name">Event name</param>
        /// <param name="parentAction">The action on which this event was reported.</param>
        /// <returns>The timestamp associated with the event (timestamp since session start time).</returns>
        private long BuildEvent(StringBuilder builder, EventType eventType, string name, Action parentAction)
        {
            BuildBasicEventData(builder, eventType, name);

            var eventTimestamp = timingProvider.ProvideTimestampInMilliseconds();

            AddKeyValuePair(builder, BEACON_KEY_PARENT_ACTION_ID, parentAction.ID);
            AddKeyValuePair(builder, BEACON_KEY_START_SEQUENCE_NUMBER, NextSequenceNumber);
            AddKeyValuePair(builder, BEACON_KEY_TIME_0, GetTimeSinceBeaconCreation(eventTimestamp));

            return eventTimestamp;
        }

        // helper method for building basic event data
        private void BuildBasicEventData(StringBuilder builder, EventType eventType, string name)
        {
            AddKeyValuePair(builder, BEACON_KEY_EVENT_TYPE, (int)eventType);
            AddKeyValuePairIfValueIsNotNull(builder, BEACON_KEY_NAME, TruncateNullSafe(name));
            AddKeyValuePair(builder, BEACON_KEY_THREAD_ID, threadIDProvider.ThreadID);
        }
       

        // helper method for creating basic beacon protocol data
        private string CreateBasicBeaconData()
        {
            StringBuilder basicBeaconBuilder = new StringBuilder();

            // version and application information
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_PROTOCOL_VERSION, ProtocolConstants.PROTOCOL_VERSION);
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_OPENKIT_VERSION, ProtocolConstants.OPENKIT_VERSION);
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_APPLICATION_ID, configuration.ApplicationID);
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_APPLICATION_NAME, configuration.ApplicationName);
            AddKeyValuePairIfValueIsNotNull(basicBeaconBuilder, BEACON_KEY_APPLICATION_VERSION, configuration.ApplicationVersion);
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_PLATFORM_TYPE, ProtocolConstants.PLATFORM_TYPE_OPENKIT);
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_AGENT_TECHNOLOGY_TYPE, ProtocolConstants.AGENT_TECHNOLOGY_TYPE);

            // device/visitor ID, session number and IP address
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_VISITOR_ID, DeviceID);
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_SESSION_NUMBER, SessionNumber);
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_CLIENT_IP_ADDRESS, clientIPAddress);

            // platform information
            AddKeyValuePairIfValueIsNotNull(basicBeaconBuilder, BEACON_KEY_DEVICE_OS, configuration.Device.OperatingSystem);
            AddKeyValuePairIfValueIsNotNull(basicBeaconBuilder, BEACON_KEY_DEVICE_MANUFACTURER, configuration.Device.Manufacturer);
            AddKeyValuePairIfValueIsNotNull(basicBeaconBuilder, BEACON_KEY_DEVICE_MODEL, configuration.Device.ModelID);
            

            return basicBeaconBuilder.ToString();
        }

        // helper method for creating basic timestamp data
        private string CreateTimestampData()
        {
            StringBuilder timestampBuilder = new StringBuilder();

            AddKeyValuePair(timestampBuilder, BEACON_KEY_SESSION_START_TIME, timingProvider.ConvertToClusterTime(sessionStartTime));
            AddKeyValuePair(timestampBuilder, BEACON_KEY_TIMESYNC_TIME, timingProvider.ConvertToClusterTime(sessionStartTime));
            if (!timingProvider.IsTimeSyncSupported)
            {
                AddKeyValuePair(timestampBuilder, BEACON_KEY_TRANSMISSION_TIME, timingProvider.ProvideTimestampInMilliseconds());
            }

            return timestampBuilder.ToString();
        }

        // helper method for adding key/value pairs with string values
        private static void AddKeyValuePair(StringBuilder builder, string key, string stringValue)
        {
            AppendKey(builder, key);
            builder.Append(System.Uri.EscapeDataString(stringValue));
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
            if (name.Length > MAX_NAME_LEN)
            {
                name = name.Substring(0, MAX_NAME_LEN);
            }
            return name;
        }

        private long GetTimeSinceBeaconCreation(long timestamp)
        {
            return timestamp - sessionStartTime;
        }
    }
}
