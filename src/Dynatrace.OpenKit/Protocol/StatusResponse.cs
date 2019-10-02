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

using System;
using System.Collections.Generic;
using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Protocol
{

    /// <summary>
    ///  Implements a status response which is sent for the request types status check & beacon send.
    /// </summary>
    public class StatusResponse : Response, IStatusResponse
    {
        private static readonly char[] PartsSeparator = { '&' };

        // status response constants
        public const string ResponseKeyCapture = "cp";
        public const string ResponseKeySendInterval = "si";
        public const string ResponseKeyMonitorName = "bn";
        public const string ResponseKeyServerId = "id";
        public const string ResponseKeyMaxBeaconSize = "bl";
        public const string ResponseKeyCaptureErrors = "er";
        public const string ResponseKeyCaptureCrashes = "cr";
        public const string ResponseKeyMultiplicity = "mp";

        public StatusResponse(ILogger logger, string response, int responseCode, Dictionary<string, List<string>> headers) :
            base(logger, responseCode, headers)
        {
            ParseResponse(response);
        }

        public bool Capture { get; private set; } = true;

        public int SendInterval { get; private set; } = -1;

        public string MonitorName { get; private set; } = null;

        public int ServerId { get; private set; } = -1;

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

            foreach (var parts in response.Split(PartsSeparator, StringSplitOptions.RemoveEmptyEntries))
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
                    case ResponseKeyCapture:
                        Capture = (int.Parse(value) == 1);
                        break;
                    case ResponseKeySendInterval:
                        SendInterval = int.Parse(value) * 1000;
                        break;
                    case ResponseKeyMonitorName:
                        MonitorName = value;
                        break;
                    case ResponseKeyServerId:
                        ServerId = int.Parse(value);
                        break;
                    case ResponseKeyMaxBeaconSize:
                        MaxBeaconSize = int.Parse(value) * 1024;
                        break;
                    case ResponseKeyCaptureErrors:
                        CaptureErrors = (int.Parse(value) != 0);                  // 1 (always on) and 2 (only on WiFi) are treated the same
                        break;
                    case ResponseKeyCaptureCrashes:
                        CaptureCrashes = (int.Parse(value) != 0);                 // 1 (always on) and 2 (only on WiFi) are treated the same
                        break;
                    case ResponseKeyMultiplicity:
                        Multiplicity = int.Parse(value);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
