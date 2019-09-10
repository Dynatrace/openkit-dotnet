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
using System.Threading;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core.Objects
{

    /// <summary>
    ///  Actual implementation of the IAction interface.
    /// </summary>
    public class Action : IAction
    {
        private static readonly IWebRequestTracer NullWebRequestTracer = new NullWebRequestTracer();

        // Action ID, name and parent ID (default: null)
        private readonly Action parentAction;

        // start/end time & sequence number
        private long endTime = -1;

        // Beacon reference
        private readonly Beacon beacon;

        // data structures for managing IAction hierarchies
        private readonly SynchronizedQueue<IAction> thisLevelActions;

        public Action(ILogger logger, Beacon beacon, string name, SynchronizedQueue<IAction> thisLevelActions)
            : this(logger, beacon, name, null, thisLevelActions)
        {
        }

        internal Action(ILogger logger, Beacon beacon, string name, Action parentAction, SynchronizedQueue<IAction> thisLevelActions)
        {
            Logger = logger;
            this.beacon = beacon;
            this.parentAction = parentAction;

            StartTime = beacon.CurrentTimestamp;
            StartSequenceNo = beacon.NextSequenceNumber;
            Id = beacon.NextId;
            Name = name;

            this.thisLevelActions = thisLevelActions;
            this.thisLevelActions.Put(this);
        }

        public int Id { get; }

        public string Name { get; }

        public int ParentId => parentAction?.Id ?? 0;

        public long StartTime { get; }

        public long EndTime => Interlocked.Read(ref endTime);

        public int StartSequenceNo { get; }

        public int EndSequenceNo { get; private set; } = -1;

        internal bool IsActionLeft => EndTime != -1L;

        internal ILogger Logger { get; }

        public IAction ReportEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Logger.Warn(this + "ReportEvent: eventName must not be null or empty");
                return this;
            }
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(this + "ReportEvent(" + eventName + ")");
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
                Logger.Warn(this + "ReportValue (string): valueName must not be null or empty");
                return this;
            }
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(this + "ReportValue(string) (" + valueName + ", " + value + ")");
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
                Logger.Warn(this + "ReportValue (double): valueName must not be null or empty");
                return this;
            }
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(this + "ReportValue(double) (" + valueName + ", " + value + ")");
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
                Logger.Warn(this + "ReportValue (int): valueName must not be null or empty");
                return this;
            }
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(this + "ReportValue(int) (" + valueName + ", " + value + ")");
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
                Logger.Warn(this + "ReportError: errorName must not be null or empty");
                return this;
            }
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(this + "ReportError(" + errorName + ", " + errorCode + ", " + reason + ")");
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
                Logger.Warn(this + "TraceWebRequest (String): url must not be null or empty");
                return NullWebRequestTracer;
            }
            if (!WebRequestTracer.IsValidUrlScheme(url))
            {
                Logger.Warn(this + $"TraceWebRequest (String): url \"{url}\" does not have a valid scheme");
                return NullWebRequestTracer;
            }
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(this + "TraceWebRequest (String) (" + url + ")");
            }
            if (!IsActionLeft)
            {
                return new WebRequestTracer(Logger, beacon, Id, url);
            }

            return NullWebRequestTracer;
        }

        public IAction LeaveAction()
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(this + "LeaveAction(" + Name + ")");
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
            EndSequenceNo = beacon.NextSequenceNumber;

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
            return GetType().Name + " [sn=" + beacon.SessionNumber + ", id=" + Id + ", name=" + Name + ", pa="
                + (parentAction != null ? Convert.ToString(parentAction.Id) : "no parent") + "] ";
        }
    }
}
