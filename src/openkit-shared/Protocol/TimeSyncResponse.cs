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

namespace Dynatrace.OpenKit.Protocol
{

    /// <summary>
    ///  Implements a time sync response which is sent for time sync requests.
    /// </summary>
    public class TimeSyncResponse : Response
    {
        internal static readonly char[] PARTS_SEPARATOR = new char[] { '&' };

        // time sync response constants
        internal const string RESPONSE_KEY_REQUEST_RECEIVE_TIME = "t1";
        internal const string RESPONSE_KEY_RESPONSE_SEND_TIME = "t2";
        
        internal TimeSyncResponse(string response, int responseCode) : base(responseCode)
        {
            ParseResponse(response);
        }

        public long RequestReceiveTime { get; private set; } = -1L;

        public long ResponseSendTime { get; private set; } = -1L;
        
        /// <summary>
        /// parses time sync response 
        /// </summary>
        /// <param name="response">Dynatrace/AppMon response to parse</param>
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

                if (RESPONSE_KEY_REQUEST_RECEIVE_TIME.Equals(key))
                {
                    RequestReceiveTime = Int64.Parse(value);
                }
                else if (RESPONSE_KEY_RESPONSE_SEND_TIME.Equals(key))
                {
                    ResponseSendTime = Int64.Parse(value);
                }
            }
        }
    }
}
