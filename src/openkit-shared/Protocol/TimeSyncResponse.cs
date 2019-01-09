﻿//
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

        // time sync response constants
        internal const string RESPONSE_KEY_REQUEST_RECEIVE_TIME = "t1";
        internal const string RESPONSE_KEY_RESPONSE_SEND_TIME = "t2";

        // timestamps contained in time sync response
        private long requestReceiveTime = -1;
        private long responseSendTime = -1;

        // *** constructors ***

        public TimeSyncResponse(string response, int responseCode) : base(responseCode)
        {
            ParseResponse(response);
        }

        // *** private methods ***

        // parses time sync response
        private void ParseResponse(string response)
        {
            string[] tokens = response.Split(new Char[] { '&', '=' });

            int index = 0;
            while (tokens.Length > index)
            {
                string key = tokens[index++];
                string value = tokens[index++];

                if (RESPONSE_KEY_REQUEST_RECEIVE_TIME.Equals(key))
                {
                    requestReceiveTime = Int64.Parse(value);
                }
                else if (RESPONSE_KEY_RESPONSE_SEND_TIME.Equals(key))
                {
                    responseSendTime = Int64.Parse(value);
                }
            }
        }

        // *** properties ***

        public long RequestReceiveTime
        {
            get
            {
                return requestReceiveTime;
            }
        }

        public long ResponseSendTime
        {
            get
            {
                return responseSendTime;
            }
        }

    }

}
