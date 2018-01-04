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

using System;

namespace Dynatrace.OpenKit.Protocol
{

    /// <summary>
    ///  Implements a status response which is sent for the request types status check & beacon send.
    /// </summary>
    public class StatusResponse : Response
    {

        // status response constants
        public const string RESPONSE_KEY_CAPTURE = "cp";
        public const string RESPONSE_KEY_SEND_INTERVAL = "si";
        public const string RESPONSE_KEY_MONITOR_NAME = "bn";
        public const string RESPONSE_KEY_SERVER_ID = "id";
        public const string RESPONSE_KEY_MAX_BEACON_SIZE = "bl";
        public const string RESPONSE_KEY_CAPTURE_ERRORS = "er";
        public const string RESPONSE_KEY_CAPTURE_CRASHES = "cr";

        // settings contained in status response
        private bool capture = true;
        private int sendInterval = -1;
        private string monitorName = null;
        private int serverID = -1;
        private int maxBeaconSize = -1;
        private bool captureErrors = true;
        private bool captureCrashes = true;

        // *** constructors ***

        public StatusResponse(string response, int responseCode) : base(responseCode)
        {
            ParseResponse(response);
        }

        // *** private methods ***

        // parses status check response
        private void ParseResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
                return;

            string[] tokens = response.Split(new Char[] { '&', '=' });

            int index = 0;
            while (tokens.Length > index)
            {
                string key = tokens[index++];
                string value = tokens[index++];

                if (RESPONSE_KEY_CAPTURE.Equals(key))
                {
                    capture = (Int32.Parse(value) == 1);
                }
                else if (RESPONSE_KEY_SEND_INTERVAL.Equals(key))
                {
                    sendInterval = Int32.Parse(value) * 1000;
                }
                else if (RESPONSE_KEY_MONITOR_NAME.Equals(key))
                {
                    monitorName = value;
                }
                else if (RESPONSE_KEY_SERVER_ID.Equals(key))
                {
                    serverID = Int32.Parse(value);
                }
                else if (RESPONSE_KEY_MAX_BEACON_SIZE.Equals(key))
                {
                    maxBeaconSize = Int32.Parse(value) * 1024;
                }
                else if (RESPONSE_KEY_CAPTURE_ERRORS.Equals(key))
                {
                    captureErrors = (Int32.Parse(value) != 0);                  // 1 (always on) and 2 (only on WiFi) are treated the same
                }
                else if (RESPONSE_KEY_CAPTURE_CRASHES.Equals(key))
                {
                    captureCrashes = (Int32.Parse(value) != 0);                 // 1 (always on) and 2 (only on WiFi) are treated the same
                }
            }
        }

        // *** properties ***

        public bool Capture
        {
            get
            {
                return capture;
            }
        }

        public int SendInterval
        {
            get
            {
                return sendInterval;
            }
        }

        public string MonitorName
        {
            get
            {
                return monitorName;
            }
        }

        public int ServerID
        {
            get
            {
                return serverID;
            }
        }

        public int MaxBeaconSize
        {
            get
            {
                return maxBeaconSize;
            }
        }

        public bool CaptureErrors
        {
            get
            {
                return captureErrors;
            }
        }

        public bool CaptureCrashes
        {
            get
            {
                return captureCrashes;
            }
        }

    }

}
