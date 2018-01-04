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
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Core
{

    /// <summary>
    ///  Actual implementation of the IAction interface.
    /// </summary>
    public class Action : IAction
    {

        // Action ID, name and parent ID (default: null)
        private int id;
        private string name;
        private Action parentAction = null;

        // start/end time & sequence number
        private long startTime;
        private long endTime = -1;
        private int startSequenceNo;
        private int endSequenceNo = -1;

        // Beacon reference
        private Beacon beacon;

        // data structures for managing IAction hierarchies
        private SynchronizedQueue<IAction> thisLevelActions = null;

        // *** constructors ***

        public Action(Beacon beacon, string name, SynchronizedQueue<IAction> parentActions) : this(beacon, name, null, parentActions)
        {
        }

        internal Action(Beacon beacon, string name, Action parentAction, SynchronizedQueue<IAction> thisLevelActions)
        {
            this.beacon = beacon;
            this.parentAction = parentAction;

            this.startTime = beacon.CurrentTimestamp;
            this.startSequenceNo = beacon.NextSequenceNumber;
            this.id = beacon.NextID;
            this.name = name;

            this.thisLevelActions = thisLevelActions;
            this.thisLevelActions.Put(this);
        }

        // *** IAction interface methods ***

        public IAction ReportEvent(string eventName)
        {
            beacon.ReportEvent(this, eventName);
            return this;
        }

        public IAction ReportValue(string valueName, string value)
        {
            beacon.ReportValue(this, valueName, value);
            return this;
        }

        public IAction ReportValue(string valueName, double value)
        {
            beacon.ReportValue(this, valueName, value);
            return this;
        }

        public IAction ReportValue(string valueName, int value)
        {
            beacon.ReportValue(this, valueName, value);
            return this;
        }

        public IAction ReportError(string errorName, int errorCode, string reason)
        {
            beacon.ReportError(this, errorName, errorCode, reason);
            return this;
        }

#if NET40 || NET35

        public IWebRequestTracer TraceWebRequest(System.Net.WebClient webClient) {
            return new WebRequestTracerWebClient(beacon, this, webClient);
        }

#else

        public IWebRequestTracer TraceWebRequest(System.Net.Http.HttpClient httpClient)
        {
            return new WebRequestTracerHttpClient(beacon, this, httpClient);
        }

#endif

        public IWebRequestTracer TraceWebRequest(string url)
        {
            return new WebRequestTracerStringURL(beacon, this, url);
        }

        public IAction LeaveAction()
        {
            // check if leaveAction() was already called before by looking at endTime
            if (endTime != -1)
            {
                return parentAction;
            }

            return DoLeaveAction();
        }

        protected virtual IAction DoLeaveAction()
        {
            // set end time and end sequence number
            endTime = beacon.CurrentTimestamp;
            endSequenceNo = beacon.NextSequenceNumber;

            // add Action to Beacon
            beacon.AddAction(this);

            // remove Action from the Actions on this level
            thisLevelActions.Remove(this);

            return parentAction; // can be null if there's no parent Action!
        }

        // *** properties ***

        public int ID
        {
            get
            {
                return id;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public int ParentID
        {
            get
            {
                return parentAction == null ? 0 : parentAction.ID;
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

    }

}
