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

using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Core.Objects
{
    /// <summary>
    /// This interface represents a <see cref="WebRequestTracer"/> which is internally used.
    ///
    /// <para>
    /// The main purpose of this interface is to make components which require a <see cref="WebRequestTracer"/> to be
    /// more easily testable.
    /// </para>
    /// </summary>
    internal interface IWebRequestTracerInternals : IWebRequestTracer, ICancelableOpenKitObject
    {
        /// <summary>
        /// Returns the URL to be traced (excluding query arguments)
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Returns the start time of the web request
        /// </summary>
        long StartTime { get; }

        /// <summary>
        /// End time of the web request
        /// </summary>
        long EndTime { get; }

        /// <summary>
        /// Sequence number when the web request started
        /// </summary>
        int StartSequenceNo { get; }

        /// <summary>
        /// Sequence number when the web request ended
        /// </summary>
        int EndSequenceNo { get; }

        /// <summary>
        /// The response code of the web request
        /// </summary>
        int ResponseCode { get; }

        /// <summary>
        /// The number of bytes sent
        /// </summary>
        int BytesSent { get; }

        /// <summary>
        /// The number of received bytes
        /// </summary>
        int BytesReceived { get; }

        /// <summary>
        /// Indicates whether this <see cref="IWebRequestTracer"/> was <see cref="IWebRequestTracer.Stop(int)">stopped</see>
        /// or not.
        /// </summary>
        bool IsStopped { get; }

        /// <summary>
        /// Parent object of this web request tracer
        /// </summary>
        IOpenKitComposite Parent { get; }
    }
}