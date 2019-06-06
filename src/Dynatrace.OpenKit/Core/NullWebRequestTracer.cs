﻿//
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
using System;

namespace Dynatrace.OpenKit.Core
{
    /// <summary>
    /// This class is returned as WebRequestTracer by <see cref="IAction.TraceWebRequest(string)"/>
    /// when the <see cref="IAction.LeaveAction()"/> has been called before.
    /// </summary>
    public class NullWebRequestTracer : IWebRequestTracer
    {
        public string Tag => "";

        public IWebRequestTracer SetBytesReceived(int bytesReceived)
        {
            return this;
        }

        public IWebRequestTracer SetBytesSent(int bytesSent)
        {
            return this;
        }

        [Obsolete("Use Stop(int) instead")]
        public IWebRequestTracer SetResponseCode(int responseCode)
        {
            return this;
        }

        public IWebRequestTracer Start()
        {
            return this;
        }

        [Obsolete("Use Stop(int) instead")]
        public void Stop()
        {
            // intentionally left empty, due to NullObject pattern
        }

        public void Stop(int responseCode)
        {
            // nothing, NullObject pattern
        }

        public void Dispose()
        {
            // nothing, NullObject pattern
        }
    }
}
