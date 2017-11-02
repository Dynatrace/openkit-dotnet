/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using System;

namespace Dynatrace.OpenKit.Protocol
{

    /// <summary>
    ///  Implements a time sync response which is sent for time sync requests.
    /// </summary>
    public class TimeSyncResponse : Response
    {

        // time sync response constants
        private const string RESPONSE_KEY_REQUEST_RECEIVE_TIME = "t1";
        private const string RESPONSE_KEY_RESPONSE_SEND_TIME = "t2";

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
