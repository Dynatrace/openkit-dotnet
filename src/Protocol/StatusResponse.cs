/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using System;

namespace Dynatrace.OpenKit.Protocol {

    /// <summary>
    ///  Implements a status response which is sent for the request types status check & beacon send.
    /// </summary>
    public class StatusResponse : Response {

        // status response constants
        private const string RESPONSE_KEY_CAPTURE = "cp";
	    private const string RESPONSE_KEY_SEND_INTERVAL = "si";
	    private const string RESPONSE_KEY_MONITOR_NAME = "bn";
	    private const string RESPONSE_KEY_SERVER_ID = "id";
	    private const string RESPONSE_KEY_MAX_BEACON_SIZE = "bl";

	    // settings contained in status response
	    private bool capture = true;
        private int sendInterval = -1;
        private string monitorName = null;
        private int serverID = -1;
        private int maxBeaconSize = -1;

        // *** constructors ***

        public StatusResponse(string response, int responseCode) : base(responseCode) {
            ParseResponse(response);
        }

        // *** private methods ***

        // parses status check response
        private void ParseResponse(string response) {
            string[] tokens = response.Split(new Char[] { '&', '=' });

            int index = 0;
            while (tokens.Length > index) {
                string key = tokens[index++];
                string value = tokens[index++];

                if (RESPONSE_KEY_CAPTURE.Equals(key)) {
                    capture = (Int32.Parse(value) == 1);
                } else if (RESPONSE_KEY_SEND_INTERVAL.Equals(key)) {
                    sendInterval = Int32.Parse(value) * 1000;
                } else if (RESPONSE_KEY_MONITOR_NAME.Equals(key)) {
                    monitorName = value;
                } else if (RESPONSE_KEY_SERVER_ID.Equals(key)) {
                    serverID = Int32.Parse(value);
                } else if (RESPONSE_KEY_MAX_BEACON_SIZE.Equals(key)) {
                    maxBeaconSize = Int32.Parse(value) * 1024;
                }
            }
        }

        // *** properties ***

        public bool Capture {
            get {
                return capture;
            }
        }

        public int SendInterval {
            get {
                return sendInterval;
            }
        }

        public string MonitorName {
            get {
                return monitorName;
            }
        }

        public int ServerID {
            get {
                return serverID;
            }
        }

        public int MaxBeaconSize {
            get {
                return maxBeaconSize;
            }
        }

    }

}
