//
// Copyright 2018-2020 Dynatrace LLC
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
using System;

namespace Dynatrace.OpenKit.Core.Objects
{
    /// <summary>
    /// This class is returned as Action by <see cref="IRootAction.EnterAction(string)"/>
    /// when the <see cref="IAction.LeaveAction()"/> has been called before.
    /// </summary>
    public class NullAction : IAction
    {
        private readonly IAction parentAction;

        internal NullAction() : this(null)
        {
        }

        internal NullAction(IAction parentAction)
        {
            this.parentAction = parentAction;
        }

        public void Dispose()
        {
            LeaveAction();
        }

        public IAction LeaveAction()
        {
            return parentAction;
        }

        public IAction ReportError(string errorName, int errorCode, string reason)
        {
            return this;
        }

        public IAction ReportError(string errorName, int errorCode)
        {
            return this;
        }

        public IAction ReportError(string errorName, string causeName, string causeDescription, string causeStackTrace)
        {
            return this;
        }

        public IAction ReportError(string errorName, Exception exception)
        {
            return this;
        }

        public IAction ReportEvent(string eventName)
        {
            return this;
        }

        public IAction ReportValue(string valueName, int value)
        {
            return this;
        }

        public IAction ReportValue(string valueName, long value)
        {
            return this;
        }


        public IAction ReportValue(string valueName, double value)
        {
            return this;
        }

        public IAction ReportValue(string valueName, string value)
        {
            return this;
        }

        public IWebRequestTracer TraceWebRequest(string url)
        {
            return NullWebRequestTracer.Instance;
        }
    }
}
