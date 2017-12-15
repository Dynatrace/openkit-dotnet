/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using System.Collections.Generic;
using System.Text;

using Dynatrace.OpenKit.Core;
using System.Threading;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;
using System.Collections.ObjectModel;
using Dynatrace.OpenKit.Core.Util;

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
        private const string BEACON_KEY_VISITOR_ID = "vi";
        private const string BEACON_KEY_SESSION_NUMBER = "sn";
        private const string BEACON_KEY_CLIENT_IP_ADDRESS = "ip";

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

        // version constants
        public const string OPENKIT_VERSION = "7.0.0000";
        private const int PROTOCOL_VERSION = 3;
        private const int PLATFORM_TYPE_OPENKIT = 1;

        // max name length
        private const int MAX_NAME_LEN = 250;

        // web request tag prefix constant
        private const string TAG_PREFIX = "MT";

        // next ID and sequence number
        private int nextID = 0;
        private int nextSequenceNumber = 0;

        // session number & start time
        private int sessionNumber;
        private long sessionStartTime;

        // client IP address
        private string clientIPAddress;

        // providers
        private readonly IThreadIDProvider threadIdProvider;
        private readonly ITimingProvider timingProvider;

        // configuration
        private readonly HTTPClientConfiguration httpConfiguration;

        // basic beacon protocol data
        private string basicBeaconData;

        // Configuration reference
        private AbstractConfiguration configuration;

        // lists of events and actions currently on the Beacon
        private LinkedList<string> eventDataList = new LinkedList<string>();
        private LinkedList<string> actionDataList = new LinkedList<string>();

        // *** constructors ***

        /// <summary>
        /// Creates a new instance of type Beacon
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="clientIPAddress"></param>
        public Beacon(AbstractConfiguration configuration, string clientIPAddress)
            : this(configuration, clientIPAddress, new DefaultThreadIDProvider(), new DefaultTimingProvider())
        {
        }

        /// <summary>
        /// Internal constructor for testing purposes
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="clientIPAddress"></param>
        /// <param name="threadIdProvider"></param>
        internal Beacon(AbstractConfiguration configuration, string clientIPAddress, 
            IThreadIDProvider threadIdProvider, ITimingProvider timingProvider)
        {
            this.threadIdProvider = threadIdProvider;
            this.timingProvider = timingProvider;

            this.sessionNumber = configuration.NextSessionNumber;
            this.sessionStartTime = timingProvider.ProvideTimestampInMilliseconds();

            this.configuration = configuration;
            if (InetAddressValidator.IsValidIP(clientIPAddress))
            {
                this.clientIPAddress = clientIPAddress;
            }
            else
            {
                this.clientIPAddress = string.Empty;
            }

            // store the current http configuration
            this.httpConfiguration = configuration.HttpClientConfig;

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
        internal ReadOnlyCollection<string> EventDataList
        {
            get { return (new List<string>(eventDataList)).AsReadOnly(); }
        }

        /// <summary>
        /// Returns an immutable list of the action data
        /// 
        /// Used for testing
        /// </summary>
        internal ReadOnlyCollection<string> ActionDataList
        {
            get { return (new List<string>(actionDataList)).AsReadOnly(); }
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
            return TAG_PREFIX + "_"
                       + PROTOCOL_VERSION + "_"
                       + httpConfiguration.ServerID + "_"
                       + configuration.DeviceID + "_"
                       + sessionNumber + "_"
                       + configuration.ApplicationID + "_"
                       + parentAction.ID + "_"
                       + threadIdProvider.ThreadID + "_"
                       + sequenceNo;
        }

        /// <summary>
        /// add an Action to this Beacon
        /// </summary>
        /// <param name="action"></param>
        public void AddAction(Action action)
        {
            StringBuilder actionBuilder = new StringBuilder();

            BuildBasicEventData(actionBuilder, EventType.ACTION, action.Name);

            AddKeyValuePair(actionBuilder, BEACON_KEY_ACTION_ID, action.ID);
            AddKeyValuePair(actionBuilder, BEACON_KEY_PARENT_ACTION_ID, action.ParentID);
            AddKeyValuePair(actionBuilder, BEACON_KEY_START_SEQUENCE_NUMBER, action.StartSequenceNo);
            AddKeyValuePair(actionBuilder, BEACON_KEY_TIME_0, timingProvider.GetTimeSinceLastInitTime(action.StartTime));
            AddKeyValuePair(actionBuilder, BEACON_KEY_END_SEQUENCE_NUMBER, action.EndSequenceNo);
            AddKeyValuePair(actionBuilder, BEACON_KEY_TIME_1, action.EndTime - action.StartTime);

            AddActionData(actionBuilder);
        }

        /// <summary>
        /// end Session on this Beacon
        /// </summary>
        /// <param name="session"></param>
        public void EndSession(Session session)
        {
            StringBuilder eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.SESSION_END, null);

            AddKeyValuePair(eventBuilder, BEACON_KEY_PARENT_ACTION_ID, 0);
            AddKeyValuePair(eventBuilder, BEACON_KEY_START_SEQUENCE_NUMBER, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BEACON_KEY_TIME_0, timingProvider.GetTimeSinceLastInitTime(session.EndTime));

            AddEventData(eventBuilder);
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

            BuildEvent(eventBuilder, EventType.VALUE_INT, valueName, parentAction);
            AddKeyValuePair(eventBuilder, BEACON_KEY_VALUE, value);

            AddEventData(eventBuilder);
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

            BuildEvent(eventBuilder, EventType.VALUE_DOUBLE, valueName, parentAction);
            AddKeyValuePair(eventBuilder, BEACON_KEY_VALUE, value);

            AddEventData(eventBuilder);
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

            BuildEvent(eventBuilder, EventType.VALUE_STRING, valueName, parentAction);
            AddKeyValuePair(eventBuilder, BEACON_KEY_VALUE, Truncate(value));

            AddEventData(eventBuilder);
        }

        /// <summary>
        /// report named event on the provided Action
        /// </summary>
        /// <param name="parentAction"></param>
        /// <param name="eventName"></param>
        public void ReportEvent(Action parentAction, string eventName)
        {
            StringBuilder eventBuilder = new StringBuilder();

            BuildEvent(eventBuilder, EventType.NAMED_EVENT, eventName, parentAction);

            AddEventData(eventBuilder);
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

            AddKeyValuePair(eventBuilder, BEACON_KEY_PARENT_ACTION_ID, parentAction.ID);
            AddKeyValuePair(eventBuilder, BEACON_KEY_START_SEQUENCE_NUMBER, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BEACON_KEY_TIME_0, timingProvider.TimeSinceLastInitTime);
            AddKeyValuePair(eventBuilder, BEACON_KEY_ERROR_CODE, errorCode);
            AddKeyValuePair(eventBuilder, BEACON_KEY_ERROR_REASON, reason);

            AddEventData(eventBuilder);
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

            AddKeyValuePair(eventBuilder, BEACON_KEY_PARENT_ACTION_ID, 0);                                  // no parent action
            AddKeyValuePair(eventBuilder, BEACON_KEY_START_SEQUENCE_NUMBER, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BEACON_KEY_TIME_0, timingProvider.TimeSinceLastInitTime);
            AddKeyValuePair(eventBuilder, BEACON_KEY_ERROR_REASON, reason);
            AddKeyValuePair(eventBuilder, BEACON_KEY_ERROR_STACKTRACE, stacktrace);

            AddEventData(eventBuilder);
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

            AddKeyValuePair(eventBuilder, BEACON_KEY_PARENT_ACTION_ID, parentAction.ID);
            AddKeyValuePair(eventBuilder, BEACON_KEY_START_SEQUENCE_NUMBER, webRequestTracer.StartSequenceNo);
            AddKeyValuePair(eventBuilder, BEACON_KEY_TIME_0, timingProvider.GetTimeSinceLastInitTime(webRequestTracer.StartTime));
            AddKeyValuePair(eventBuilder, BEACON_KEY_END_SEQUENCE_NUMBER, webRequestTracer.EndSequenceNo);
            AddKeyValuePair(eventBuilder, BEACON_KEY_TIME_1, webRequestTracer.EndTime - webRequestTracer.StartTime);
            if (webRequestTracer.ResponseCode != -1)
            {
                AddKeyValuePair(eventBuilder, BEACON_KEY_WEBREQUEST_RESPONSECODE, webRequestTracer.ResponseCode);
            }
            if (webRequestTracer.BytesSent != -1)
            {
                AddKeyValuePair(eventBuilder, BEACON_KEY_WEBREQUEST_BYTES_SEND, webRequestTracer.BytesSent);
            }
            if (webRequestTracer.BytesReceived != -1)
            {
                AddKeyValuePair(eventBuilder, BEACON_KEY_WEBREQUEST_BYTES_RECEIVED, webRequestTracer.BytesReceived);
            }

            AddEventData(eventBuilder);
        }

        /// <summary>
        /// Identify the user
        /// </summary>
        /// <param name="userTag"></param>
        public void IdentifyUser(string userTag)
        {
            StringBuilder eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.IDENTIFY_USER, userTag);

            AddKeyValuePair(eventBuilder, BEACON_KEY_PARENT_ACTION_ID, 0);
            AddKeyValuePair(eventBuilder, BEACON_KEY_START_SEQUENCE_NUMBER, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BEACON_KEY_TIME_0, timingProvider.TimeSinceLastInitTime);

            AddEventData(eventBuilder);
        }

        /// <summary>
        /// send current state of Beacon
        /// </summary>
        /// <param name="httpClientProvider"></param>
        /// <param name="numRetries"></param>
        /// <returns></returns>
        public StatusResponse Send(IHTTPClientProvider httpClientProvider, int numRetries)
        {
            var httpClient = httpClientProvider.CreateClient(httpConfiguration);
            var beaconDataChunks = CreateBeaconDataChunks();
            StatusResponse response = null;
            foreach (byte[] beaconData in beaconDataChunks)
            {
                response = SendBeaconRequest(httpClient, beaconData, numRetries);
            }

            // only return last status response for updating the settings
            return response;
        }

        internal void ClearData()
        {
            lock (eventDataList)
            {
                lock (actionDataList)
                {
                    eventDataList.Clear();
                    actionDataList.Clear();
                }
            }
        }

        // *** private methods ***
        private void AddActionData(StringBuilder actionBuilder)
        {
            lock (actionDataList)
            {
                if (configuration.IsCaptureOn)
                {
                    actionDataList.AddLast(actionBuilder.ToString());
                }
            }
        }

        private void AddEventData(StringBuilder eventBuilder)
        {
            lock (eventDataList)
            {
                if (configuration.IsCaptureOn)
                {
                    eventDataList.AddLast(eventBuilder.ToString());
                }
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
        private void BuildEvent(StringBuilder builder, EventType eventType, string name, Action parentAction)
        {
            BuildBasicEventData(builder, eventType, name);

            AddKeyValuePair(builder, BEACON_KEY_PARENT_ACTION_ID, parentAction.ID);
            AddKeyValuePair(builder, BEACON_KEY_START_SEQUENCE_NUMBER, NextSequenceNumber);
            AddKeyValuePair(builder, BEACON_KEY_TIME_0, timingProvider.TimeSinceLastInitTime);
        }

        // helper method for building basic event data
        private void BuildBasicEventData(StringBuilder builder, EventType eventType, string name)
        {
            AddKeyValuePair(builder, BEACON_KEY_EVENT_TYPE, (int)eventType);
            if (name != null)
            {
                AddKeyValuePair(builder, BEACON_KEY_NAME, Truncate(name));
            }
            AddKeyValuePair(builder, BEACON_KEY_THREAD_ID, threadIdProvider.ThreadID);
        }

        // creates (possibly) multiple beacon chunks based on max beacon size
        private List<byte[]> CreateBeaconDataChunks()
        {
            List<byte[]> beaconDataChunks = new List<byte[]>();

            lock (eventDataList)
            {
                lock (actionDataList)
                {
                    while (!(eventDataList.Count == 0) || !(actionDataList.Count == 0))
                    {
                        StringBuilder beaconBuilder = new StringBuilder();

                        beaconBuilder.Append(basicBeaconData);
                        beaconBuilder.Append('&');
                        beaconBuilder.Append(CreateTimestampData());

                        while (!(eventDataList.Count == 0))
                        {
                            if (beaconBuilder.Length > configuration.MaxBeaconSize - 1024)
                            {
                                break;
                            }

                            string eventData = eventDataList.First.Value;
                            eventDataList.RemoveFirst();

                            beaconBuilder.Append('&');
                            beaconBuilder.Append(eventData);
                        }

                        while (!(actionDataList.Count == 0))
                        {
                            if (beaconBuilder.Length > configuration.MaxBeaconSize - 1024)
                            {
                                break;
                            }

                            string actionData = actionDataList.First.Value;
                            actionDataList.RemoveFirst();

                            beaconBuilder.Append('&');
                            beaconBuilder.Append(actionData);
                        }

                        beaconDataChunks.Add(Encoding.UTF8.GetBytes(beaconBuilder.ToString()));
                    }
                }
            }

            return beaconDataChunks;
        }

        // helper method for creating basic beacon protocol data
        private string CreateBasicBeaconData()
        {
            StringBuilder basicBeaconBuilder = new StringBuilder();

            // version and application information
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_PROTOCOL_VERSION, PROTOCOL_VERSION);
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_OPENKIT_VERSION, OPENKIT_VERSION);
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_APPLICATION_ID, configuration.ApplicationID);
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_APPLICATION_NAME, configuration.ApplicationName);
            if (configuration.ApplicationVersion != null)
            {
                AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_APPLICATION_VERSION, configuration.ApplicationVersion);
            }
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_PLATFORM_TYPE, PLATFORM_TYPE_OPENKIT);

            // device/visitor ID, session number and IP address
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_VISITOR_ID, configuration.DeviceID);
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_SESSION_NUMBER, sessionNumber);
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_CLIENT_IP_ADDRESS, clientIPAddress);

            // platform information
            if (configuration.Device.OperatingSystem != null)
            {
                AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_DEVICE_OS, configuration.Device.OperatingSystem);
            }
            if (configuration.Device.Manufacturer != null)
            {
                AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_DEVICE_MANUFACTURER, configuration.Device.Manufacturer);
            }
            if (configuration.Device.ModelID != null)
            {
                AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_DEVICE_MODEL, configuration.Device.ModelID);
            }

            return basicBeaconBuilder.ToString();
        }

        // helper method for creating basic timestamp data
        private string CreateTimestampData()
        {
            StringBuilder timestampBuilder = new StringBuilder();

            AddKeyValuePair(timestampBuilder, BEACON_KEY_SESSION_START_TIME, timingProvider.ConvertToClusterTime(sessionStartTime));
            AddKeyValuePair(timestampBuilder, BEACON_KEY_TIMESYNC_TIME, timingProvider.LastInitTimeInClusterTime);
            if (!timingProvider.IsTimeSyncSupported)
            {
                AddKeyValuePair(timestampBuilder, BEACON_KEY_TRANSMISSION_TIME, timingProvider.ProvideTimestampInMilliseconds());
            }

            return timestampBuilder.ToString();
        }

        // helper method for adding key/value pairs with string values
        private void AddKeyValuePair(StringBuilder builder, string key, string stringValue)
        {
            AppendKey(builder, key);
            builder.Append(System.Uri.EscapeDataString(stringValue));
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
            if (name.Length > MAX_NAME_LEN)
            {
                name = name.Substring(0, MAX_NAME_LEN);
            }
            return name;
        }
    }
}
