//
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

using System.Collections.Generic;
using System.Text;

using Dynatrace.OpenKit.Core;
using System.Threading;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;
using System.Collections.ObjectModel;
using Dynatrace.OpenKit.Core.Util;
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.API;
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
        private const string BeaconKeyProtocolVersion = "vv";
        private const string BeaconKeyOpenKitVersion = "va";
        private const string BeaconKeyApplicationID = "ap";
        private const string BeaconKeyApplicationName = "an";
        private const string BeaconKeyApplicationVersion = "vn";
        private const string BeaconKeyPlatformType = "pt";
        private const string BeaconKeyAgentTechnologyType = "tt";
        private const string BeaconKeyVisitorID = "vi";
        private const string BeaconKeySessionNumber = "sn";
        private const string BeaconKeyClientIPAddress = "ip";
        private const string BeaconKeyMultiplicity = "mp";
        private const string BeaconKeyDataCollectionLevel = "dl";
        private const string BeaconKeyCrashReportingLevel = "cl";

        // device data constants
        private const string BeaconKeyDeviceOS = "os";
        private const string BeaconKeyDeviceManufacturer = "mf";
        private const string BeaconKeyDeviceModel = "md";

        // timestamp constants
        private const string BeaconKeySessionStartTime = "tv";
        private const string BeaconKeyTimeSyncTime = "ts";
        private const string BeaconKeyTransmissionTime = "tx";

        // Action related constants
        private const string BeaconKeyEventType = "et";
        private const string BeaconKeyName = "na";
        private const string BeaconKeyThreadID = "it";
        private const string BeaconKeyActionID = "ca";
        private const string BeaconKeyParentActionID = "pa";
        private const string BeaconKeyStartSequenceNumber = "s0";
        private const string BeaconKeyTime0 = "t0";
        private const string BeaconKeyEndSequenceNumber = "s1";
        private const string BeaconKeyTime1 = "t1";

        // data and error capture constants
        private const string BeaconKeyValue = "vl";
        private const string BeaconKeyErrorCode = "ev";
        private const string BeaconKeyErrorReason = "rs";
        private const string BeaconKeyErrorStacktrace = "st";
        private const string BeaconKeyWebrequestResponseCode = "rc";
        private const string BeaconKeyWebrequestBytesSent = "bs";
        private const string BeaconKeyWebrequestBytesReceived = "br";

        // version constants
        public const string OpenKitVersion = "7.0.0000";
        private const int ProtocolVersion = 3;
        private const int PlatformTypeOpenKit = 1;
        private const string AgentTechnologyType = "okdotnet";

        // max name length
        private const int MaxNameLen = 250;

        // web request tag prefix constant
        private const string WebRequestTagPrefix = "MT";

        // web request tag reserved characters
        private static readonly char[] ReservedCharacters = { '_' };

        private const char BeaconDataDelimiter = '&';

        // next ID and sequence number
        private int nextID = 0;
        private int nextSequenceNumber = 0;

        // session number & start time
        private int sessionNumber;
        private long sessionStartTime;

        // client IP address
        private string clientIPAddress;

        // providers
        private readonly IThreadIDProvider threadIDProvider;
        private readonly ITimingProvider timingProvider;

        // configuration
        private readonly HTTPClientConfiguration httpConfiguration;

        // basic beacon protocol data
        private string basicBeaconData;

        // Configuration reference
        private OpenKitConfiguration configuration;

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
        {
            this.logger = logger;
            this.beaconCache = beaconCache;
            this.sessionNumber = configuration.NextSessionNumber;
            this.timingProvider = timingProvider;

            this.configuration = configuration;
            this.threadIDProvider = threadIDProvider;
            this.sessionStartTime = timingProvider.ProvideTimestampInMilliseconds();

            if (InetAddressValidator.IsValidIP(clientIPAddress))
            {
                this.clientIPAddress = clientIPAddress;
            }
            else
            {
                this.clientIPAddress = string.Empty;
            }

            // store the current http configuration
            this.httpConfiguration = configuration.HTTPClientConfig;

            basicBeaconData = CreateBasicBeaconData();
        }

        // *** public properties ***

        /// <summary>
        /// create next ID
        /// </summary>
        public int NextID
        {
            get
            {
                return Interlocked.Increment(ref nextID);
            }
        }

        /// <summary>
        /// create next sequence number
        /// </summary>
        public int NextSequenceNumber
        {
            get
            {
                return Interlocked.Increment(ref nextSequenceNumber);
            }
        }

        /// <summary>
        /// Get the current timestamp in milliseconds by delegating to TimingProvider
        /// </summary>
        public long CurrentTimestamp
        {
            get
            {
                return timingProvider.ProvideTimestampInMilliseconds();
            }
        }

        /// <summary>
        /// Returns an immutable list of the event data
        /// 
        /// Used for testing
        /// </summary>
        internal List<string> EventDataList
        {
            get
            {
                var events = beaconCache.GetEvents(sessionNumber);
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
                var actions = beaconCache.GetActions(sessionNumber);
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
            
            return $"{WebRequestTagPrefix}_"
                + $"{ProtocolVersion}_"
                + $"{httpConfiguration.ServerID}_"
                + $"{configuration.DeviceID}_"
                + $"{sessionNumber}_"
                + $"{configuration.ApplicationIDPercentEncoded}_"
                + $"{parentAction.ID}_"
                + $"{threadIDProvider.ThreadID}_"
                + $"{sequenceNo}";
        }

        /// <summary>
        /// add an Action to this Beacon
        /// </summary>
        /// <param name="action"></param>
        public void AddAction(Action action)
        {
            StringBuilder actionBuilder = new StringBuilder();

            BuildBasicEventData(actionBuilder, EventType.ACTION, action.Name);

            AddKeyValuePair(actionBuilder, BeaconKeyActionID, action.ID);
            AddKeyValuePair(actionBuilder, BeaconKeyParentActionID, action.ParentID);
            AddKeyValuePair(actionBuilder, BeaconKeyStartSequenceNumber, action.StartSequenceNo);
            AddKeyValuePair(actionBuilder, BeaconKeyTime0, GetTimeSinceBeaconCreation(action.StartTime));
            AddKeyValuePair(actionBuilder, BeaconKeyEndSequenceNumber, action.EndSequenceNo);
            AddKeyValuePair(actionBuilder, BeaconKeyTime1, action.EndTime - action.StartTime);

            AddActionData(action.StartTime, actionBuilder);
        }

        /// <summary>
        /// end Session on this Beacon
        /// </summary>
        /// <param name="session"></param>
        public void EndSession(Session session)
        {
            StringBuilder eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.SESSION_END, null);

            AddKeyValuePair(eventBuilder, BeaconKeyParentActionID, 0);
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BeaconKeyTime0, GetTimeSinceBeaconCreation(session.EndTime));

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
            StringBuilder eventBuilder = new StringBuilder();

            var eventTimestamp = BuildEvent(eventBuilder, EventType.VALUE_INT, valueName, parentAction);
            AddKeyValuePair(eventBuilder, BeaconKeyValue, value);

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
            StringBuilder eventBuilder = new StringBuilder();

            var eventTimestamp = BuildEvent(eventBuilder, EventType.VALUE_DOUBLE, valueName, parentAction);
            AddKeyValuePair(eventBuilder, BeaconKeyValue, value);

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
            StringBuilder eventBuilder = new StringBuilder();
            
            var eventTimestamp = BuildEvent(eventBuilder, EventType.VALUE_STRING, valueName, parentAction);
            AddKeyValuePair(eventBuilder, BeaconKeyValue, Truncate(value));

            AddEventData(eventTimestamp, eventBuilder);
        }

        /// <summary>
        /// report named event on the provided Action
        /// </summary>
        /// <param name="parentAction"></param>
        /// <param name="eventName"></param>
        public void ReportEvent(Action parentAction, string eventName)
        {
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
            if (!configuration.CaptureErrors)
            {
                return;
            }

            StringBuilder eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.ERROR, errorName);

            var timestamp = timingProvider.ProvideTimestampInMilliseconds();
            AddKeyValuePair(eventBuilder, BeaconKeyParentActionID, parentAction.ID);
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BeaconKeyTime0, GetTimeSinceBeaconCreation(timestamp));
            AddKeyValuePair(eventBuilder, BeaconKeyErrorCode, errorCode);
            AddKeyValuePair(eventBuilder, BeaconKeyErrorReason, reason);

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
            if (!configuration.CaptureCrashes)
            {
                return;
            }

            StringBuilder eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.CRASH, errorName);

            var timestamp = timingProvider.ProvideTimestampInMilliseconds();
            AddKeyValuePair(eventBuilder, BeaconKeyParentActionID, 0);                                  // no parent action
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BeaconKeyTime0, GetTimeSinceBeaconCreation(timestamp));
            AddKeyValuePair(eventBuilder, BeaconKeyErrorReason, reason);
            AddKeyValuePair(eventBuilder, BeaconKeyErrorStacktrace, stacktrace);

            AddEventData(timestamp, eventBuilder);
        }

        /// <summary>
        /// add web request to the provided Action
        /// </summary>
        /// <param name="parentAction"></param>
        /// <param name="webRequestTracer"></param>
        public void AddWebRequest(Action parentAction, WebRequestTracerBase webRequestTracer)
        {
            StringBuilder eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.WEBREQUEST, webRequestTracer.URL);

            AddKeyValuePair(eventBuilder, BeaconKeyParentActionID, parentAction.ID);
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, webRequestTracer.StartSequenceNo);
            AddKeyValuePair(eventBuilder, BeaconKeyTime0, GetTimeSinceBeaconCreation(webRequestTracer.StartTime));
            AddKeyValuePair(eventBuilder, BeaconKeyEndSequenceNumber, webRequestTracer.EndSequenceNo);
            AddKeyValuePair(eventBuilder, BeaconKeyTime1, webRequestTracer.EndTime - webRequestTracer.StartTime);
            if (webRequestTracer.ResponseCode != -1)
            {
                AddKeyValuePair(eventBuilder, BeaconKeyWebrequestResponseCode, webRequestTracer.ResponseCode);
            }
            if (webRequestTracer.BytesSent > -1)
            {
                AddKeyValuePair(eventBuilder, BeaconKeyWebrequestBytesSent, webRequestTracer.BytesSent);
            }
            if (webRequestTracer.BytesReceived > -1)
            {
                AddKeyValuePair(eventBuilder, BeaconKeyWebrequestBytesReceived, webRequestTracer.BytesReceived);
            }

            AddEventData(webRequestTracer.StartTime, eventBuilder);
        }

        /// <summary>
        /// Identify the user
        /// </summary>
        /// <param name="userTag"></param>
        public void IdentifyUser(string userTag)
        {
            StringBuilder eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.IDENTIFY_USER, userTag);

            var timestamp = timingProvider.ProvideTimestampInMilliseconds();
            AddKeyValuePair(eventBuilder, BeaconKeyParentActionID, 0);
            AddKeyValuePair(eventBuilder, BeaconKeyStartSequenceNumber, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BeaconKeyTime0, GetTimeSinceBeaconCreation(timestamp));

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
                var prefix = basicBeaconData + BeaconDataDelimiter + CreateTimestampData();
                // subtract 1024 to ensure that the chunk does not exceed the send size configured on server side?
                // i guess that was the original intention, but i'm not sure about this
                // TODO stefan.eberl - This is a quite uncool algorithm and should be improved, avoid subtracting some "magic" number
                var chunk = beaconCache.GetNextBeaconChunk(sessionNumber, prefix, configuration.MaxBeaconSize - 1024, BeaconDataDelimiter);
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
                    beaconCache.ResetChunkedData(sessionNumber);
                    break;
                }
                else
                {
                    // worked -> remove previously retrieved chunk from cache
                    beaconCache.RemoveChunkedData(sessionNumber);
                }
            }

            return response;
        }

        internal void ClearData()
        {
            // remove all cached data for this Beacon from the cache
            beaconCache.DeleteCacheEntry(sessionNumber);
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
                beaconCache.AddActionData(sessionNumber, timestamp, actionBuilder.ToString());
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
                beaconCache.AddEventData(sessionNumber, timestamp, eventBuilder.ToString());
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

            AddKeyValuePair(builder, BeaconKeyParentActionID, parentAction.ID);
            AddKeyValuePair(builder, BeaconKeyStartSequenceNumber, NextSequenceNumber);
            AddKeyValuePair(builder, BeaconKeyTime0, GetTimeSinceBeaconCreation(eventTimestamp));

            return eventTimestamp;
        }

        // helper method for building basic event data
        private void BuildBasicEventData(StringBuilder builder, EventType eventType, string name)
        {
            AddKeyValuePair(builder, BeaconKeyEventType, (int)eventType);
            if (name != null)
            {
                AddKeyValuePair(builder, BeaconKeyName, Truncate(name));
            }
            AddKeyValuePair(builder, BeaconKeyThreadID, threadIDProvider.ThreadID);
        }
       

        // helper method for creating basic beacon protocol data
        private string CreateBasicBeaconData()
        {
            StringBuilder basicBeaconBuilder = new StringBuilder();

            // version and application information
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyProtocolVersion, ProtocolVersion);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyOpenKitVersion, OpenKitVersion);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyApplicationID, configuration.ApplicationID);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyApplicationName, configuration.ApplicationName);
            if (configuration.ApplicationVersion != null)
            {
                AddKeyValuePair(basicBeaconBuilder, BeaconKeyApplicationVersion, configuration.ApplicationVersion);
            }
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyPlatformType, PlatformTypeOpenKit);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyAgentTechnologyType, AgentTechnologyType);

            // device/visitor ID, session number and IP address
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyVisitorID, configuration.DeviceID);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeySessionNumber, sessionNumber);
            AddKeyValuePair(basicBeaconBuilder, BeaconKeyClientIPAddress, clientIPAddress);

            // platform information
            if (configuration.Device.OperatingSystem != null)
            {
                AddKeyValuePair(basicBeaconBuilder, BeaconKeyDeviceOS, configuration.Device.OperatingSystem);
            }
            if (configuration.Device.Manufacturer != null)
            {
                AddKeyValuePair(basicBeaconBuilder, BeaconKeyDeviceManufacturer, configuration.Device.Manufacturer);
            }
            if (configuration.Device.ModelID != null)
            {
                AddKeyValuePair(basicBeaconBuilder, BeaconKeyDeviceModel, configuration.Device.ModelID);
            }

            return basicBeaconBuilder.ToString();
        }

        // helper method for creating basic timestamp data
        private string CreateTimestampData()
        {
            StringBuilder timestampBuilder = new StringBuilder();

            AddKeyValuePair(timestampBuilder, BeaconKeySessionStartTime, timingProvider.ConvertToClusterTime(sessionStartTime));
            AddKeyValuePair(timestampBuilder, BeaconKeyTimeSyncTime, timingProvider.ConvertToClusterTime(sessionStartTime));
            if (!timingProvider.IsTimeSyncSupported)
            {
                AddKeyValuePair(timestampBuilder, BeaconKeyTransmissionTime, timingProvider.ProvideTimestampInMilliseconds());
            }

            return timestampBuilder.ToString();
        }

        // helper method for adding key/value pairs with string values
        private void AddKeyValuePair(StringBuilder builder, string key, string stringValue)
        {
            AppendKey(builder, key);
            builder.Append(PercentEncoder.Encode(stringValue, Encoding.UTF8, ReservedCharacters));
        }

        // helper method for adding key/value pairs with long values
        private void AddKeyValuePair(StringBuilder builder, string key, long longValue)
        {
            AppendKey(builder, key);
            builder.Append(longValue);
        }

        // helper method for adding key/value pairs with int values
        private void AddKeyValuePair(StringBuilder builder, string key, int intValue)
        {
            AppendKey(builder, key);
            builder.Append(intValue);
        }

        // helper method for adding key/value pairs with double values
        private void AddKeyValuePair(StringBuilder builder, string key, double doubleValue)
        {
            AppendKey(builder, key);
            builder.Append(doubleValue);
        }

        // helper method for appending a key
        private void AppendKey(StringBuilder builder, string key)
        {
            if (builder.Length > 0)
            {
                builder.Append('&');
            }
            builder.Append(key);
            builder.Append('=');
        }

        // helper method for truncating name at max name size
        private string Truncate(string name)
        {
            name = name.Trim();
            if (name.Length > MaxNameLen)
            {
                name = name.Substring(0, MaxNameLen);
            }
            return name;
        }

        private long GetTimeSinceBeaconCreation(long timestamp)
        {
            return timestamp - sessionStartTime;
        }

        public bool IsEmpty { get { return beaconCache.IsEmpty(sessionNumber); } }
    }
}
