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
using System.Threading;
using System;
using System.Text.RegularExpressions;

namespace Dynatrace.OpenKit.Core
{

    /// <summary>
    ///  Standard implementation of the IWebRequestTracer interface.
    /// </summary>
    public class WebRequestTracer : IWebRequestTracer
    {
        private readonly ILogger logger = null;

        private static readonly Regex SchemaValidationPattern = new Regex("^[a-z][a-z0-9+\\-.]*://.+", RegexOptions.IgnoreCase);

        // Dynatrace tag that has to be used for tracing the web request
        private readonly string tag = null;

        // HTTP information: URL & response code, bytesSent, bytesReceived
        protected string url = "<unknown>";

        // start/end time & sequence number
        private long endTime = -1;

        // Beacon and Action references
        private readonly Beacon beacon;
        private readonly int parentActionID;

        #region constructors

        public WebRequestTracer(ILogger logger, Beacon beacon, int parentActionID)
        {
            this.logger = logger;
            this.beacon = beacon;
            this.parentActionID = parentActionID;

            // creating start sequence number has to be done here, because it's needed for the creation of the tag
            StartSequenceNo = beacon.NextSequenceNumber;

            tag = beacon.CreateTag(parentActionID, StartSequenceNo);

            StartTime = beacon.CurrentTimestamp;
        }

        /// <summary>
        ///  This constructor can be used for tracing and timing of a web request handled by any 3rd party HTTP Client.
        ///  Setting the Dynatrace tag to the OpenKit.WEBREQUEST_TAG_HEADER HTTP header has to be done manually by the user.
        /// </summary>
        public WebRequestTracer(ILogger logger, Beacon beacon, int parentActionID, string url) : this(logger, beacon, parentActionID)
        {
            if (IsValidURLScheme(url))
            {
                this.url = url.Split(new char[] { '?' }, 2)[0];
            }
        }

        #endregion

        internal static bool IsValidURLScheme(string url)
        {
            return url != null && SchemaValidationPattern.Match(url).Success;
        }

        public string URL => url;

        public long StartTime { get; private set; }

        public long EndTime => Interlocked.Read(ref endTime);

        public int StartSequenceNo { get; }

        public int EndSequenceNo { get; private set; } = -1;

        public int ResponseCode { get; private set; } = -1;

        public int BytesSent { get; private set; } = -1;

        public int BytesReceived { get; private set; } = -1;

        internal bool IsStopped => EndTime != -1;

        public void Dispose()
        {
            Stop(ResponseCode);
        }

        #region IWebRequestTracer implementation

        public string Tag
        {
            get
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug($"{this} Tag returning '{tag}'");
                }
                return tag;
            }
        }

        public IWebRequestTracer Start()
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(this + "Start()");
            }
            if (!IsStopped)
            {
                StartTime = beacon.CurrentTimestamp;
            }
            return this;
        }

        [Obsolete("Use Stop(int) instead")]
        public void Stop()
        {
            Stop(ResponseCode);
        }

        public void Stop(int responseCode)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{this} Stop(rc='{responseCode}')");
            }
            if (Interlocked.CompareExchange(ref endTime, beacon.CurrentTimestamp, -1L) != -1L)
            {
                return;
            }

            ResponseCode = responseCode;
            EndSequenceNo = beacon.NextSequenceNumber;

            // add web request to beacon
            beacon.AddWebRequest(parentActionID, this);
        }

        [Obsolete("Use Stop(int) instead")]
        public IWebRequestTracer SetResponseCode(int responseCode)
        {
            if (!IsStopped)
            {
                ResponseCode = responseCode;
            }
            return this;
        }

        public IWebRequestTracer SetBytesSent(int bytesSent)
        {
            if (!IsStopped)
            {
                BytesSent = bytesSent;
            }
            return this;
        }

        public IWebRequestTracer SetBytesReceived(int bytesReceived)
        {
            if (!IsStopped)
            {
                BytesReceived = bytesReceived;
            }
            return this;
        }

        #endregion

        public override string ToString()
        {
            return $"{GetType().Name} [sn={beacon.SessionNumber}, id={parentActionID}, url='{url}'] ";
        }
    }
}
