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
        internal static readonly char[] PARTS_SEPARATOR = new char[] { '&' };

        // status response constants
        public const string RESPONSE_KEY_CAPTURE = "cp";
        public const string RESPONSE_KEY_SEND_INTERVAL = "si";
        public const string RESPONSE_KEY_MONITOR_NAME = "bn";
        public const string RESPONSE_KEY_SERVER_ID = "id";
        public const string RESPONSE_KEY_MAX_BEACON_SIZE = "bl";
        public const string RESPONSE_KEY_CAPTURE_ERRORS = "er";
        public const string RESPONSE_KEY_CAPTURE_CRASHES = "cr";
        public const string RESPONSE_KEY_MULTIPLICITY = "mp";

        public StatusResponse(string response, int responseCode) : base(responseCode)
        {
            ParseResponse(response);
        }

        public bool Capture { get; private set; } = true;

        public int SendInterval { get; private set; } = -1;

        public string MonitorName { get; private set; } = null;

        public int ServerID { get; private set; } = -1;

        public int MaxBeaconSize { get; private set; } = -1;

        public bool CaptureErrors { get; private set; } = true;

        public bool CaptureCrashes { get; private set; } = true;

        public int Multiplicity { get; private set; } = 1;

        // parses status check response
        private void ParseResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                return;
            }

            foreach (var parts in response.Split(PARTS_SEPARATOR, StringSplitOptions.RemoveEmptyEntries))
            {
                var tokens = parts.Split('=');
                if (tokens.Length != 2)
                {
                    throw new ArgumentException("Invalid response; even number of tokens expected.");
                }

                var key = tokens[0];
                var value = tokens[1];

                switch (key)
                {
                    case RESPONSE_KEY_CAPTURE:
                        Capture = (int.Parse(value) == 1);
                        break;
                    case RESPONSE_KEY_SEND_INTERVAL:
                        SendInterval = int.Parse(value) * 1000;
                        break;
                    case RESPONSE_KEY_MONITOR_NAME:
                        MonitorName = value;
                        break;
                    case RESPONSE_KEY_SERVER_ID:
                        ServerID = int.Parse(value);
                        break;
                    case RESPONSE_KEY_MAX_BEACON_SIZE:
                        MaxBeaconSize = int.Parse(value) * 1024;
                        break;
                    case RESPONSE_KEY_CAPTURE_ERRORS:
                        CaptureErrors = (int.Parse(value) != 0);                  // 1 (always on) and 2 (only on WiFi) are treated the same
                        break;
                    case RESPONSE_KEY_CAPTURE_CRASHES:
                        CaptureCrashes = (int.Parse(value) != 0);                 // 1 (always on) and 2 (only on WiFi) are treated the same
                        break;
                    case RESPONSE_KEY_MULTIPLICITY:
                        Multiplicity = int.Parse(value);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
