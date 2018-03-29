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

using System;

namespace Dynatrace.OpenKit.API
{

    /// <summary>
    ///  This interface provides functionality to report events/values/errors and traces web requests.
    /// </summary>
    public interface IAction : IDisposable
    {

        /// <summary>
        ///  Reports an event with a specified name (but without any value).
        /// </summary>
        /// <remarks>
        /// If given <paramref name="eventName"/> is <code>null</code>
        /// or an empty string, then no event is reported to the system.
        /// </remarks>
        /// <param name="eventName">name of the event</param>
        /// <returns>this Action (for usage as fluent API)</returns>
        IAction ReportEvent(string eventName);

        /// <summary>
        ///  Reports an int value with a specified name.
        /// </summary>
        /// <remarks>
        /// If given <paramref name="valueName"/> is <code>null</code> or an empty string,
        /// no value is reported.
        /// </remarks>
        /// <param name="valueName">name of this value</param>
        /// <param name="value">value itself</param>
        /// <returns>this Action (for usage as fluent API)</returns>
        IAction ReportValue(string valueName, int value);

        /// <summary>
        ///  Reports a double value with a specified name.
        /// </summary>
        /// <remarks>
        /// If given <paramref name="valueName"/> is <code>null</code> or an empty string,
        /// no value is reported.
        /// </remarks>
        /// <param name="valueName">name of this value</param>
        /// <param name="value">value itself</param>
        /// <returns>this Action (for usage as fluent API)</returns>
        IAction ReportValue(string valueName, double value);

        /// <summary>
        ///  Reports a string value with a specified name.
        /// </summary>
        /// <remarks>
        /// If given <paramref name="valueName"/> is <code>null</code> or an empty string,
        /// no value is reported.
        /// </remarks>
        /// <param name="valueName">name of this value</param>
        /// <param name="value">value itself</param>
        /// <returns>this Action (for usage as fluent API)</returns>
        IAction ReportValue(string valueName, string value);

        /// <summary>
        ///  Reports an error with a specified name, error code and reason.
        /// </summary>
        /// <remarks>
        /// If given <paramref name="errorName"/> is <code>null</code> or an empty string,
        /// no error is reported.
        /// </remarks>
        /// <param name="errorName">name of this error</param>
        /// <param name="errorCode">numeric error code of this error</param>
        /// <param name="reason">reason for this error</param>
        /// <returns>this Action (for usage as fluent API)</returns>
        IAction ReportError(string errorName, int errorCode, string reason);

        /// <summary>
        ///  Allows tracing and timing of a web request handled by any 3rd party HTTP Client (e.g. Apache, Google, ...).
        ///  In this case the Dynatrace HTTP header (OpenKitFactory.WEBREQUEST_TAG_HEADER) has to be set manually to the
        ///  traces value of this WebRequestTracer.
        ///  If the web request is continued on a server-side Agent (e.g. Java, .NET, ...) this Session will be correlated to
        ///  the resulting server-side PurePath.
        /// </summary>
        /// <remarks>
        /// If given <paramref name="url"/> is <code>null</code> or an empty string, then
        /// a <see cref="NullWebRequestTracer"/> is returned and nothing is reported to the server.
        /// </remarks>
        /// <param name="url">the URL of the web request to be traced and timed</param>
        /// <returns>a WebRequestTracer which allows getting the tag value and adding timing information</returns>
        IWebRequestTracer TraceWebRequest(string url);

        /// <summary>
        ///  Leaves this Action.
        /// </summary>
        /// <returns>the parent Action, or null if there is no parent Action</returns>
        IAction LeaveAction();
    }

}
