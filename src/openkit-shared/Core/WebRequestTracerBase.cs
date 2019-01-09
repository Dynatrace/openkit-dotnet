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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core
{

    /// <summary>
    ///  Abstract base class implementation of the IWebRequestTracer interface.
    /// </summary>
    public abstract class WebRequestTracerBase : IWebRequestTracer
    {

        // Dynatrace tag that has to be used for tracing the web request
        private string tag = null;

        // HTTP information: URL & response code, bytesSent, bytesReceived
        protected string url = "<unknown>";
        private int responseCode = -1;
        private int bytesSent = -1;
        private int bytesReceived = -1;

        // start/end time & sequence number
        private long startTime = -1;
        private long endTime = -1;
        private int startSequenceNo = -1;
        private int endSequenceNo = -1;

        // Beacon and Action references
        private Beacon beacon;
        private Action action;

        // *** constructors ***

        public WebRequestTracerBase(Beacon beacon, Action action)
        {
            this.beacon = beacon;
            this.action = action;

            // creating start sequence number has to be done here, because it's needed for the creation of the tag
            startSequenceNo = beacon.NextSequenceNumber;

            tag = beacon.CreateTag(action, startSequenceNo);
        }

        // *** IWebRequestTracer interface methods ***

        public string URL
        {
            get
            {
                return url;
            }
        }

        public long StartTime
        {
            get
            {
                return startTime;
            }
        }

        public long EndTime
        {
            get
            {
                return endTime;
            }
        }

        public int StartSequenceNo
        {
            get
            {
                return startSequenceNo;
            }
        }

        public int EndSequenceNo
        {
            get
            {
                return endSequenceNo;
            }
        }

        public string Tag
        {
            get
            {
                return tag;
            }
        }

        public int ResponseCode
        {
            get
            {
                return responseCode;
            }
        }

        public int BytesSent
        {
            get
            {
                return bytesSent;
            }
        }

        public int BytesReceived
        {
            get
            {
                return bytesReceived;
            }
        }

        public IWebRequestTracer Start()
        {
            startTime = beacon.CurrentTimestamp;
            return this;
        }

        public void Stop()
        {
            endTime = beacon.CurrentTimestamp;
            endSequenceNo = beacon.NextSequenceNumber;

            // add web request to beacon
            beacon.AddWebRequest(action, this);
        }

        public IWebRequestTracer SetResponseCode(int responseCode)
        {
            this.responseCode = responseCode;
            return this;
        }

        public IWebRequestTracer SetBytesSent(int bytesSent)
        {
            this.bytesSent = bytesSent;
            return this;
        }

        public IWebRequestTracer SetBytesReceived(int bytesReceived)
        {
            this.bytesReceived = bytesReceived;
            return this;
        }
    }

}
