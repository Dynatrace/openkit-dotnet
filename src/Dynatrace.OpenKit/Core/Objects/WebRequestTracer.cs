//
// Copyright 2018-2021 Dynatrace LLC
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
using System.Text;
using System.Text.RegularExpressions;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Util;

namespace Dynatrace.OpenKit.Core.Objects
{

    /// <summary>
    ///  Standard implementation of the IWebRequestTracer interface.
    /// </summary>
    public class WebRequestTracer : IWebRequestTracerInternals
    {
        private static readonly Regex SchemaValidationPattern = new Regex("^[a-z][a-z0-9+\\-.]*://.+", RegexOptions.IgnoreCase);

        /// <summary>
        /// Helper to reduce ToString() effort.
        /// </summary>
        private string toString;

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
        private readonly IOpenKitComposite parent;

        /// <summary>
        /// Dynatrace tag that has to be used for tracing the web request
        /// </summary>
        private readonly string tag;

        /// <summary>
        /// Beacon and Action references
        /// </summary>
        private readonly IBeacon beacon;

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
        internal WebRequestTracer(ILogger logger, IOpenKitComposite parent, IBeacon beacon)
        {
            this.logger = logger;
            this.beacon = beacon;
            this.parent = parent;
            parentActionId = parent.ActionId;

            // creating start sequence number has to be done here, because it's needed for the creation of the tag
            StartSequenceNo = beacon.NextSequenceNumber;

            tag = beacon.CreateTag(parentActionId, StartSequenceNo);

            StartTime = beacon.CurrentTimestamp;
        }

        /// <summary>
        ///  This constructor can be used for tracing and timing of a web request handled by any 3rd party HTTP Client.
        ///  Setting the Dynatrace tag to the <see cref="OpenKitConstants.WEBREQUEST_TAG_HEADER"/> HTTP header has to
        /// be done manually by the user.
        /// </summary>
        internal WebRequestTracer(ILogger logger, IOpenKitComposite parent, IBeacon beacon, string url)
            : this(logger, parent, beacon)
        {
            if (IsValidUrlScheme(url))
            {
                Url = url.Split(new [] { '?' }, 2)[0];
            }
        }

        #endregion

        private IWebRequestTracerInternals ThisTracer => this;

        /// <summary>
        /// Checks whether the given URL is valid or not
        /// </summary>
        /// <param name="url">the URL to be checked</param>
        /// <returns><code>true</code> if the given URL is valid, <code>false</code> otherwise.</returns>
        internal static bool IsValidUrlScheme(string url)
        {
            return url != null && SchemaValidationPattern.Match(url).Success;
        }

        #region IWebRequestTracerInternals impementation

        public string Url { get; } = "<unknown>";

        public long StartTime { get; private set; }

        public long EndTime { get; private set; } = -1;

        public int StartSequenceNo { get; }

        public int EndSequenceNo { get; private set; } = -1;

        public int ResponseCode { get; private set; } = -1;

        public int BytesSent { get; private set; } = -1;

        public int BytesReceived { get; private set; } = -1;

        bool IWebRequestTracerInternals.IsStopped => EndTime != -1;

        IOpenKitComposite IWebRequestTracerInternals.Parent => parent;

        #endregion

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
                if (!ThisTracer.IsStopped)
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

            DoStop(responseCode, false);
        }

        private void DoStop(int responseCode, bool discardData)
        {
            lock (lockObject)
            {
                if (ThisTracer.IsStopped)
                {
                    // stop was already previously called
                    return;
                }

                EndSequenceNo = beacon.NextSequenceNumber;
                EndTime = beacon.CurrentTimestamp;
            }

            ResponseCode = responseCode;

            // add web request to beacon
            if (!discardData)
            {
                beacon.AddWebRequest(parentActionId, this);
            }

            // last but not least notify the parent & detach from parent
            parent.OnChildClosed(this);
        }

        [Obsolete("Use Stop(int) instead")]
        public IWebRequestTracer SetResponseCode(int responseCode)
        {
            lock (lockObject)
            {
                if (!ThisTracer.IsStopped)
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
                if (!ThisTracer.IsStopped)
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
                if (!ThisTracer.IsStopped)
                {
                    BytesReceived = bytesReceived;
                }
            }

            return this;
        }

        #endregion

        #region ICancelableOpenKitObject implementation

        void ICancelableOpenKitObject.Cancel()
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(this + " Cancel()");
            }

            DoStop(ResponseCode, true);
        }

        #endregion

        public override string ToString()
        {
            return toString ??
                (toString = new StringBuilder(GetType().Name)
                    .Append(" [sn=").Append(beacon.SessionNumber.ToInvariantString())
                    .Append(", seq=").Append(beacon.SessionSequenceNumber.ToInvariantString())
                    .Append(", pa=").Append(parentActionId.ToInvariantString())
                    .Append(", url='").Append(Url)
                    .Append("']").ToString());
        }
    }
}
