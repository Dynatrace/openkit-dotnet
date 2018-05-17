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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Protocol;
using System.Threading;

namespace Dynatrace.OpenKit.Core
{

    /// <summary>
    ///  Abstract base class implementation of the IWebRequestTracer interface.
    /// </summary>
    public abstract class WebRequestTracerBase : IWebRequestTracer
    {
        private ILogger logger = null;

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

        public WebRequestTracerBase(ILogger logger, Beacon beacon, Action action)
        {
            this.logger = logger;
            this.beacon = beacon;
            this.action = action;

            // creating start sequence number has to be done here, because it's needed for the creation of the tag
            startSequenceNo = beacon.NextSequenceNumber;

            tag = beacon.CreateTag(action, startSequenceNo);

            startTime = beacon.CurrentTimestamp;
        }

        // *** IWebRequestTracer interface methods ***

        public string URL => url;

        public long StartTime => startTime;

        public long EndTime => Interlocked.Read(ref endTime);

        public int StartSequenceNo => startSequenceNo;

        public int EndSequenceNo => endSequenceNo;

        public string Tag
        {
            get
            {
                if(logger.IsDebugEnabled)
                {
                    logger.Debug(this + "Tag returning '" + tag + "'");
                }
                return tag;
            }
        }

        public int ResponseCode => responseCode;

        public int BytesSent => bytesSent;

        public int BytesReceived => bytesReceived;

        internal bool IsStopped => EndTime != -1;

        public void Dispose()
        {
            Stop();
        }

        public IWebRequestTracer Start()
        {
            if( logger.IsDebugEnabled)
            {
                logger.Debug(this + "Start()");
            }
            if (!IsStopped)
            {
                startTime = beacon.CurrentTimestamp;
            }
            return this;
        }

        public void Stop()
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(this + "Stop()");
            }
            if (Interlocked.CompareExchange(ref endTime, beacon.CurrentTimestamp, -1L) != -1L)
            {
                return;
            }

            endSequenceNo = beacon.NextSequenceNumber;

            // add web request to beacon
            beacon.AddWebRequest(action, this);
        }

        public IWebRequestTracer SetResponseCode(int responseCode)
        {
            if (!IsStopped)
            {
                this.responseCode = responseCode;
            }
            return this;
        }

        public IWebRequestTracer SetBytesSent(int bytesSent)
        {
            if (!IsStopped)
            {
                this.bytesSent = bytesSent;
            }
            return this;
        }

        public IWebRequestTracer SetBytesReceived(int bytesReceived)
        {
            if (!IsStopped)
            {
                this.bytesReceived = bytesReceived;
            }
            return this;
        }

        public override string ToString()
        {
            return "WebRequestTracer [sn=" + beacon.GetSessionNumber() + ", id=" + action.ID + ", url='" + url + "'] ";
        }
    }
}
