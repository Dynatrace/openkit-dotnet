﻿//
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
using System.Collections.Generic;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Util.Json.Objects;

namespace Dynatrace.OpenKit.Core.Objects
{
    public class NullSession : ISession
    {
        /// <summary>
        /// Singleton null session instance.
        /// </summary>
        public static readonly NullSession Instance = new NullSession();

        /// <summary>
        /// Private constructor only instantiated by <see cref="Instance">singleton instance</see>
        /// </summary>
        private NullSession()
        {
        }

        public void Dispose()
        {
            End();
        }

        public void End()
        {
            // intentionally left empty, due to NullObject pattern
        }

        public IRootAction EnterAction(string actionName)
        {
            return NullRootAction.Instance;
        }

        public void IdentifyUser(string userTag)
        {
            // intentionally left empty, due to NullObject pattern
        }

        public void ReportCrash(string errorName, string reason, string stacktrace)
        {
            // intentionally left empty, due to NullObject pattern
        }

        public void ReportCrash(Exception exception)
        {
            // intentionally left empty, due to NullObject pattern
        }

        public void ReportNetworkTechnology(string technology)
        {
            // intentionally left empty, due to NullObject pattern
        }

        public void ReportConnectionType(ConnectionType connectionType)
        {
            // intentionally left empty, due to NullObject pattern
        }

        public void ReportCarrier(string carrier)
        {
            // intentionally left empty, due to NullObject pattern
        }

        void ISession.SendEvent(string name, Dictionary<string, JsonValue> attributes)
        {
            // intentionally left empty, due to NullObject pattern
        }

        public void SendBizEvent(string type, Dictionary<string, JsonValue> attributes)
        {
            // intentionally left empty, due to NullObject pattern
        }

        public IWebRequestTracer TraceWebRequest(string url)
        {
            return NullWebRequestTracer.Instance;
        }
    }
}
