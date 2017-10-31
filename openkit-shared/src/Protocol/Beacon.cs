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

namespace Dynatrace.OpenKit.Protocol {

    /// <summary>
    ///  The Beacon class holds all the beacon data and the beacon protocol implementation.
    /// </summary>
    public class Beacon {

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

        // HTTP client to be used for this Beacon
        private HTTPClient httpClient = null;

        // session number & start time
        private int sessionNumber;
        private long sessionStartTime;

        // client IP address
        private string clientIPAddress;

        // basic beacon protocol data
        private string basicBeaconData;

        // Configuration reference
        private AbstractConfiguration configuration;

        // lists of events and actions currently on the Beacon
        private LinkedList<string> eventDataList = new LinkedList<string>();
        private LinkedList<string> actionDataList = new LinkedList<string>();

        // *** constructors ***

        public Beacon(AbstractConfiguration configuration, string clientIPAddress) {
            this.sessionNumber = configuration.NextSessionNumber;
            this.sessionStartTime = TimeProvider.GetTimestamp();
            this.configuration = configuration;
            this.clientIPAddress = clientIPAddress;

            basicBeaconData = CreateBasicBeaconData();

            httpClient = configuration.CurrentHTTPClient;
        }

        // *** public methods ***

        // create next ID
        public int NextID {
            get {
                return Interlocked.Increment(ref nextID);
            }
        }

        // create next sequence number
        public int NextSequenceNumber {
            get {
                return Interlocked.Increment(ref nextSequenceNumber);
            }
        }

        // create web request tag
        public string CreateTag(Action parentAction, int sequenceNo) {
            return TAG_PREFIX + "_"
                       + PROTOCOL_VERSION + "_"
                       + httpClient.ServerID + "_"
                       + configuration.VisitorID + "_"
                       + sessionNumber + "_"
                       + configuration.ApplicationID + "_"
                       + parentAction.ID + "_"
                       + ThreadIDProvider.ThreadID + "_"
                       + sequenceNo;
        }

        // add an Action to this Beacon
        public void AddAction(Action action) {
            StringBuilder actionBuilder = new StringBuilder();

            BuildBasicEventData(actionBuilder, EventType.ACTION, action.Name);

            AddKeyValuePair(actionBuilder, BEACON_KEY_ACTION_ID, action.ID);
            AddKeyValuePair(actionBuilder, BEACON_KEY_PARENT_ACTION_ID, action.ParentID);
            AddKeyValuePair(actionBuilder, BEACON_KEY_START_SEQUENCE_NUMBER, action.StartSequenceNo);
            AddKeyValuePair(actionBuilder, BEACON_KEY_TIME_0, TimeProvider.GetTimeSinceLastInitTime(action.StartTime));
            AddKeyValuePair(actionBuilder, BEACON_KEY_END_SEQUENCE_NUMBER, action.EndSequenceNo);
            AddKeyValuePair(actionBuilder, BEACON_KEY_TIME_1, action.EndTime - action.StartTime);

            lock (actionDataList) {
                actionDataList.AddLast(actionBuilder.ToString());
            }
        }

        // end Session on this Beacon
        public void EndSession(Session session) {
            StringBuilder eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.SESSION_END, null);

            AddKeyValuePair(eventBuilder, BEACON_KEY_PARENT_ACTION_ID, 0);
            AddKeyValuePair(eventBuilder, BEACON_KEY_START_SEQUENCE_NUMBER, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BEACON_KEY_TIME_0, TimeProvider.GetTimeSinceLastInitTime(session.EndTime));

            lock (eventDataList) {
                eventDataList.AddLast(eventBuilder.ToString());
            }
        }

        // report int value on the provided Action
        public void ReportValue(Action parentAction, string valueName, int value) {
            StringBuilder eventBuilder = new StringBuilder();

            BuildEvent(eventBuilder, EventType.VALUE_INT, valueName, parentAction);
            AddKeyValuePair(eventBuilder, BEACON_KEY_VALUE, value);

            lock (eventDataList) {
                eventDataList.AddLast(eventBuilder.ToString());
            }
        }

        // report double value on the provided Action
        public void ReportValue(Action parentAction, string valueName, double value) {
            StringBuilder eventBuilder = new StringBuilder();

            BuildEvent(eventBuilder, EventType.VALUE_DOUBLE, valueName, parentAction);
            AddKeyValuePair(eventBuilder, BEACON_KEY_VALUE, value);

            lock (eventDataList) {
                eventDataList.AddLast(eventBuilder.ToString());
            }
        }

        // report string value on the provided Action
        public void ReportValue(Action parentAction, string valueName, string value) {
            StringBuilder eventBuilder = new StringBuilder();

            BuildEvent(eventBuilder, EventType.VALUE_STRING, valueName, parentAction);
            AddKeyValuePair(eventBuilder, BEACON_KEY_VALUE, Truncate(value));

            lock (eventDataList) {
                eventDataList.AddLast(eventBuilder.ToString());
            }
        }

        // report named event on the provided Action
        public void ReportEvent(Action parentAction, string eventName) {
            StringBuilder eventBuilder = new StringBuilder();

            BuildEvent(eventBuilder, EventType.NAMED_EVENT, eventName, parentAction);

            lock (eventDataList) {
                eventDataList.AddLast(eventBuilder.ToString());
            }
        }

        // report error on the provided Action
        public void ReportError(Action parentAction, string errorName, int errorCode, string reason) {
            // if capture errors is off -> do nothing
            if (!configuration.CaptureErrors) {
                return;
            }

            StringBuilder eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.ERROR, errorName);

            AddKeyValuePair(eventBuilder, BEACON_KEY_PARENT_ACTION_ID, parentAction.ID);
            AddKeyValuePair(eventBuilder, BEACON_KEY_START_SEQUENCE_NUMBER, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BEACON_KEY_TIME_0, TimeProvider.GetTimeSinceLastInitTime());
            AddKeyValuePair(eventBuilder, BEACON_KEY_ERROR_CODE, errorCode);
            AddKeyValuePair(eventBuilder, BEACON_KEY_ERROR_REASON, reason);

            lock (eventDataList) {
                eventDataList.AddLast(eventBuilder.ToString());
            }
        }

        // report a crash
        public void ReportCrash(string errorName, string reason, string stacktrace) {
            // if capture crashes is off -> do nothing
            if (!configuration.CaptureCrashes) {
                return;
            }

            StringBuilder eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.CRASH, errorName);

            AddKeyValuePair(eventBuilder, BEACON_KEY_PARENT_ACTION_ID, 0);                                  // no parent action
            AddKeyValuePair(eventBuilder, BEACON_KEY_START_SEQUENCE_NUMBER, NextSequenceNumber);
            AddKeyValuePair(eventBuilder, BEACON_KEY_TIME_0, TimeProvider.GetTimeSinceLastInitTime());
            AddKeyValuePair(eventBuilder, BEACON_KEY_ERROR_REASON, reason);
            AddKeyValuePair(eventBuilder, BEACON_KEY_ERROR_STACKTRACE, stacktrace);

            lock (eventDataList) {
                eventDataList.AddLast(eventBuilder.ToString());
            }
        }

        // add web request to the provided Action
        public void AddWebRequest(Action parentAction, WebRequestTagBase webRequestTag) {
            StringBuilder eventBuilder = new StringBuilder();

            BuildBasicEventData(eventBuilder, EventType.WEBREQUEST, webRequestTag.URL);

            AddKeyValuePair(eventBuilder, BEACON_KEY_PARENT_ACTION_ID, parentAction.ID);
            AddKeyValuePair(eventBuilder, BEACON_KEY_START_SEQUENCE_NUMBER, webRequestTag.StartSequenceNo);
            AddKeyValuePair(eventBuilder, BEACON_KEY_TIME_0, TimeProvider.GetTimeSinceLastInitTime(webRequestTag.StartTime));
            AddKeyValuePair(eventBuilder, BEACON_KEY_END_SEQUENCE_NUMBER, webRequestTag.EndSequenceNo);
            AddKeyValuePair(eventBuilder, BEACON_KEY_TIME_1, webRequestTag.EndTime - webRequestTag.StartTime);
            if (webRequestTag.ResponseCode != -1) {
                AddKeyValuePair(eventBuilder, BEACON_KEY_WEBREQUEST_RESPONSECODE, webRequestTag.ResponseCode);
            }

            lock (eventDataList) {
                eventDataList.AddLast(eventBuilder.ToString());
            }
        }

        // send current state of Beacon
        public StatusResponse Send() {
            List<byte[]> beaconDataChunks = CreateBeaconDataChunks();
            StatusResponse response = null;
            foreach (byte[] beaconData in beaconDataChunks) {
                response = httpClient.SendBeaconRequest(clientIPAddress, beaconData);
            }

            // only return last status response for updating the settings
            return response;
        }

        // *** private methods ***

        // helper method for building events
        private void BuildEvent(StringBuilder builder, EventType eventType, string name, Action parentAction) {
            BuildBasicEventData(builder, eventType, name);

            AddKeyValuePair(builder, BEACON_KEY_PARENT_ACTION_ID, parentAction.ID);
            AddKeyValuePair(builder, BEACON_KEY_START_SEQUENCE_NUMBER, NextSequenceNumber);
            AddKeyValuePair(builder, BEACON_KEY_TIME_0, TimeProvider.GetTimeSinceLastInitTime());
        }

        // helper method for building basic event data
        private void BuildBasicEventData(StringBuilder builder, EventType eventType, string name) {
            AddKeyValuePair(builder, BEACON_KEY_EVENT_TYPE, (int)eventType);
            if (name != null) {
                AddKeyValuePair(builder, BEACON_KEY_NAME, Truncate(name));
            }
            AddKeyValuePair(builder, BEACON_KEY_THREAD_ID, ThreadIDProvider.ThreadID);
        }

        // creates (possibly) multiple beacon chunks based on max beacon size
        private List<byte[]> CreateBeaconDataChunks() {
            List<byte[]> beaconDataChunks = new List<byte[]>();

            lock (eventDataList) {
                lock (actionDataList) {
                    while (!(eventDataList.Count == 0) || !(actionDataList.Count == 0)) {
                        StringBuilder beaconBuilder = new StringBuilder();

                        beaconBuilder.Append(basicBeaconData);
                        beaconBuilder.Append('&');
                        beaconBuilder.Append(CreateTimestampData());

                        while (!(eventDataList.Count == 0)) {
                            if (beaconBuilder.Length > configuration.MaxBeaconSize - 1024) {
                                break;
                            }

                            string eventData = eventDataList.First.Value;
                            eventDataList.RemoveFirst();

                            beaconBuilder.Append('&');
                            beaconBuilder.Append(eventData);
                        }

                        while (!(actionDataList.Count == 0)) {
                            if (beaconBuilder.Length > configuration.MaxBeaconSize - 1024) {
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
        private string CreateBasicBeaconData() {
            StringBuilder basicBeaconBuilder = new StringBuilder();

            // version and application information
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_PROTOCOL_VERSION, PROTOCOL_VERSION);
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_OPENKIT_VERSION, OPENKIT_VERSION);
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_APPLICATION_ID, configuration.ApplicationID);
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_APPLICATION_NAME, configuration.ApplicationName);
            if (configuration.ApplicationVersion != null) {
                AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_APPLICATION_VERSION, configuration.ApplicationVersion);
            }
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_PLATFORM_TYPE, PLATFORM_TYPE_OPENKIT);

            // visitor ID, session number and IP address
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_VISITOR_ID, configuration.VisitorID);
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_SESSION_NUMBER, sessionNumber);
            AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_CLIENT_IP_ADDRESS, clientIPAddress);

            // platform information
            if (configuration.Device.OperatingSystem != null) {
                AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_DEVICE_OS, configuration.Device.OperatingSystem);
            }
            if (configuration.Device.Manufacturer != null) {
                AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_DEVICE_MANUFACTURER, configuration.Device.Manufacturer);
            }
            if (configuration.Device.ModelID != null) {
                AddKeyValuePair(basicBeaconBuilder, BEACON_KEY_DEVICE_MODEL, configuration.Device.ModelID);
            }

            return basicBeaconBuilder.ToString();
        }

        // helper method for creating basic timestamp data
        private string CreateTimestampData() {
            StringBuilder timestampBuilder = new StringBuilder();

            AddKeyValuePair(timestampBuilder, BEACON_KEY_SESSION_START_TIME, TimeProvider.ConvertToClusterTime(sessionStartTime));
            AddKeyValuePair(timestampBuilder, BEACON_KEY_TIMESYNC_TIME, TimeProvider.GetLastInitTimeInClusterTime());
            if (!TimeProvider.TimeSynced) {
                AddKeyValuePair(timestampBuilder, BEACON_KEY_TRANSMISSION_TIME, TimeProvider.GetTimestamp());
            }

            return timestampBuilder.ToString();
        }

        // helper method for adding key/value pairs with string values
        private void AddKeyValuePair(StringBuilder builder, string key, string stringValue) {
            AppendKey(builder, key);
            builder.Append(System.Uri.EscapeDataString(stringValue));
        }

        // helper method for adding key/value pairs with long values
        private void AddKeyValuePair(StringBuilder builder, string key, long longValue) {
            AppendKey(builder, key);
            builder.Append(longValue);
        }

        // helper method for adding key/value pairs with int values
        private void AddKeyValuePair(StringBuilder builder, string key, int intValue) {
            AppendKey(builder, key);
            builder.Append(intValue);
        }

        // helper method for adding key/value pairs with double values
        private void AddKeyValuePair(StringBuilder builder, string key, double doubleValue) {
            AppendKey(builder, key);
            builder.Append(doubleValue);
        }

        // helper method for appending a key
        private void AppendKey(StringBuilder builder, string key) {
            if (builder.Length > 0) {
                builder.Append('&');
            }
            builder.Append(key);
            builder.Append('=');
        }

        // helper method for truncating name at max name size
        private string Truncate(string name) {
            name = name.Trim();
            if (name.Length > MAX_NAME_LEN) {
                name = name.Substring(0, MAX_NAME_LEN);
            }
            return name;
        }

    }

}
