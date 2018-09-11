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
    ///  Actual implementation of the IAction interface.
    /// </summary>
    public class Action : IAction
    {
        private static readonly IWebRequestTracer NullWebRequestTracer = new NullWebRequestTracer();

        private readonly ILogger logger;

        // Action ID, name and parent ID (default: null)
        private int id;
        private readonly string name;
        private Action parentAction = null;

        // start/end time & sequence number
        private readonly long startTime;
        private long endTime = -1;
        private readonly int startSequenceNo;
        private int endSequenceNo = -1;

        // Beacon reference
        private Beacon beacon;

        // data structures for managing IAction hierarchies
        private SynchronizedQueue<IAction> thisLevelActions = null;

        public Action(ILogger logger, Beacon beacon, string name, SynchronizedQueue<IAction> thisLevelActions) : this(logger, beacon, name, null, thisLevelActions)
        {
        }

        internal Action(ILogger logger, Beacon beacon, string name, Action parentAction, SynchronizedQueue<IAction> thisLevelActions)
        {
            this.logger = logger;
            this.beacon = beacon;
            this.parentAction = parentAction;

            this.startTime = beacon.CurrentTimestamp;
            this.startSequenceNo = beacon.NextSequenceNumber;
            this.id = beacon.NextID;
            this.name = name;

            this.thisLevelActions = thisLevelActions;
            this.thisLevelActions.Put(this);
        }

        public int ID => id;

        public string Name => name;

        public int ParentID => parentAction == null ? 0 : parentAction.ID;

        public long StartTime => startTime;

        public long EndTime => Interlocked.Read(ref endTime);

        public int StartSequenceNo => startSequenceNo;

        public int EndSequenceNo => endSequenceNo;

        internal bool IsActionLeft => EndTime != -1L;

        internal ILogger Logger => logger;

        public IAction ReportEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                logger.Warn(this + "ReportEvent: eventName must not be null or empty");
                return this;
            }
            if (logger.IsDebugEnabled)
            {
                logger.Debug(this + "ReportEvent(" + eventName + ")");
            }
            if (!IsActionLeft)
            {
                beacon.ReportEvent(this, eventName);
            }
            return this;
        }

        public IAction ReportValue(string valueName, string value)
        {
            if (string.IsNullOrEmpty(valueName))
            {
                logger.Warn(this + "ReportValue (string): valueName must not be null or empty");
                return this;
            }
            if (logger.IsDebugEnabled)
            {
                logger.Debug(this + "ReportValue(string) (" + valueName + ", " + value + ")");
            }
            if (!IsActionLeft)
            {
                beacon.ReportValue(this, valueName, value);
            }
            return this;
        }

        public IAction ReportValue(string valueName, double value)
        {
            if (string.IsNullOrEmpty(valueName))
            {
                logger.Warn(this + "ReportValue (double): valueName must not be null or empty");
                return this;
            }
            if (logger.IsDebugEnabled)
            {
                logger.Debug(this + "ReportValue(double) (" + valueName + ", " + value + ")");
            }
            if (!IsActionLeft)
            {
                beacon.ReportValue(this, valueName, value);
            }
            return this;
        }

        public IAction ReportValue(string valueName, int value)
        {
            if (string.IsNullOrEmpty(valueName))
            {
                logger.Warn(this + "ReportValue (int): valueName must not be null or empty");
                return this;
            }
            if (logger.IsDebugEnabled)
            {
                logger.Debug(this + "ReportValue(int) (" + valueName + ", " + value + ")");
            }
            if (!IsActionLeft)
            {
                beacon.ReportValue(this, valueName, value);
            }
            return this;
        }

        public IAction ReportError(string errorName, int errorCode, string reason)
        {
            if (string.IsNullOrEmpty(errorName))
            {
                logger.Warn(this + "ReportError: errorName must not be null or empty");
                return this;
            }
            if (logger.IsDebugEnabled)
            {
                logger.Debug(this + "ReportError(" + errorName + ", " + errorCode + ", " + reason + ")");
            }
            if (!IsActionLeft)
            {
                beacon.ReportError(this, errorName, errorCode, reason);
            }
            return this;
        }

        public IWebRequestTracer TraceWebRequest(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                logger.Warn(this + "TraceWebRequest (String): url must not be null or empty");
                return NullWebRequestTracer;
            }
            if (!WebRequestTracerStringURL.IsValidURLScheme(url))
            {
                logger.Warn(this + $"TraceWebRequest (String): url \"{url}\" does not have a valid scheme");
                return NullWebRequestTracer;
            }
            if (logger.IsDebugEnabled)
            {
                logger.Debug(this + "TraceWebRequest (String) (" + url + ")");
            }
            if (!IsActionLeft)
            {
                return new WebRequestTracerStringURL(logger, beacon, ID, url);
            }

            return NullWebRequestTracer;
        }

        public IAction LeaveAction()
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(this + "LeaveAction(" + name + ")");
            }
            // check if leaveAction() was already called before by looking at endTime
            if (Interlocked.CompareExchange(ref endTime, beacon.CurrentTimestamp, -1L) != -1L)
            {
                return parentAction;
            }

            return DoLeaveAction();
        }

        protected virtual IAction DoLeaveAction()
        {
            // set end time and end sequence number
            Interlocked.Exchange(ref endTime, beacon.CurrentTimestamp);
            endSequenceNo = beacon.NextSequenceNumber;

            // add Action to Beacon
            beacon.AddAction(this);

            // remove Action from the Actions on this level
            thisLevelActions.Remove(this);

            return parentAction; // can be null if there's no parent Action!
        }

        public void Dispose()
        {
            LeaveAction();
        }

        public override string ToString()
        {
            return GetType().Name + " [sn=" + beacon.SessionNumber + ", id=" + id + ", name=" + name + ", pa="
                + (parentAction != null ? System.Convert.ToString(parentAction.id) : "no parent") + "] ";
        }
    }
}
