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
using System.Text.RegularExpressions;
using System.Threading;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core.Objects
{

    /// <summary>
    ///  Standard implementation of the IWebRequestTracer interface.
    /// </summary>
    public class WebRequestTracer : IWebRequestTracer, IOpenKitObject
    {
        private static readonly Regex SchemaValidationPattern = new Regex("^[a-z][a-z0-9+\\-.]*://.+", RegexOptions.IgnoreCase);

        /// <summary>
        /// Logger for trace log messages
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Object for synchronization
        /// </summary>
        private readonly  object lockObject  = new object();

        /// <summary>
        /// Parent object of this web request tracer
        /// </summary>
        private OpenKitComposite parent;

        /// <summary>
        /// Dynatrace tag that has to be used for tracing the web request
        /// </summary>
        private readonly string tag;

        /// <summary>
        /// Beacon and Action references
        /// </summary>
        private readonly Beacon beacon;

        /// <summary>
        /// action ID of the parent
        /// </summary>
        private readonly int parentActionId;

        #region constructors

        /// <summary>
        /// Constructor for creating a new WebRequestTracer instance.
        /// </summary>
        /// <param name="logger">the logger used to log information.</param>
        /// <param name="parent">the parent object to which this web request tracer belongs to</param>
        /// <param name="beacon">the <see cref="Dynatrace.OpenKit.Protocol.Beacon"/> for sending data and tag creation</param>
        internal WebRequestTracer(ILogger logger, OpenKitComposite parent, Beacon beacon)
        {
            this.logger = logger;
            this.beacon = beacon;
            this.parent = parent;
            this.parentActionId = parent.ActionId;

            // creating start sequence number has to be done here, because it's needed for the creation of the tag
            StartSequenceNo = beacon.NextSequenceNumber;

            tag = beacon.CreateTag(parentActionId, StartSequenceNo);

            StartTime = beacon.CurrentTimestamp;
        }

        /// <summary>
        ///  This constructor can be used for tracing and timing of a web request handled by any 3rd party HTTP Client.
        ///  Setting the Dynatrace tag to the OpenKit.WEBREQUEST_TAG_HEADER HTTP header has to be done manually by the user.
        /// </summary>
        public WebRequestTracer(ILogger logger, OpenKitComposite parent, Beacon beacon, string url)
            : this(logger, parent, beacon)
        {
            if (IsValidUrlScheme(url))
            {
                Url = url.Split(new [] { '?' }, 2)[0];
            }
        }

        #endregion

        /// <summary>
        /// Checks whether the given URL is valid or not
        /// </summary>
        /// <param name="url">the URL to be checked</param>
        /// <returns><code>true</code> if the given URL is valid, <code>false</code> otherwise.</returns>
        internal static bool IsValidUrlScheme(string url)
        {
            return url != null && SchemaValidationPattern.Match(url).Success;
        }

        /// <summary>
        /// Returns the URL to be traced (excluding query arguments)
        /// </summary>
        public string Url { get; } = "<unknown>";

        /// <summary>
        /// Start time of the web request
        /// </summary>
        public long StartTime { get; private set; }

        /// <summary>
        /// End time of the web request
        /// </summary>
        public long EndTime { get; private set; } = -1;

        /// <summary>
        /// Sequence number when the web request started
        /// </summary>
        public int StartSequenceNo { get; }

        /// <summary>
        /// Sequence number when the web request ended
        /// </summary>
        public int EndSequenceNo { get; private set; } = -1;

        /// <summary>
        /// The response code of the web request
        /// </summary>
        public int ResponseCode { get; private set; } = -1;

        /// <summary>
        /// The number of bytes sent
        /// </summary>
        public int BytesSent { get; private set; } = -1;

        /// <summary>
        /// The number of received bytes
        /// </summary>
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
                logger.Debug($"{this} Start()");
            }

            lock (lockObject)
            {
                if (!IsStopped)
                {
                    StartTime = beacon.CurrentTimestamp;
                }
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

            lock (lockObject)
            {
                if (IsStopped)
                {
                    // stop was already previously called
                    return;
                }

                EndSequenceNo = beacon.NextSequenceNumber;
                EndTime = beacon.CurrentTimestamp;
            }

            ResponseCode = responseCode;

            // add web request to beacon
            beacon.AddWebRequest(parentActionId, this);

            // last but not least notify the parent & detach from parent
            parent.OnChildClosed(this);
            parent = null;
        }

        [Obsolete("Use Stop(int) instead")]
        public IWebRequestTracer SetResponseCode(int responseCode)
        {
            lock (lockObject)
            {
                if (!IsStopped)
                {
                    ResponseCode = responseCode;
                }
            }

            return this;
        }

        public IWebRequestTracer SetBytesSent(int bytesSent)
        {
            lock (lockObject)
            {
                if (!IsStopped)
                {
                    BytesSent = bytesSent;
                }
            }

            return this;
        }

        public IWebRequestTracer SetBytesReceived(int bytesReceived)
        {
            lock (lockObject)
            {
                if (!IsStopped)
                {
                    BytesReceived = bytesReceived;
                }
            }

            return this;
        }

        #endregion

        #region OpenKitComposite implementation



        #endregion

        public override string ToString()
        {
            return $"{GetType().Name} [sn={beacon.SessionNumber}, id={parentActionId}, url='{Url}'] ";
        }
    }
}
