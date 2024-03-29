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
        ///  Reports a long value with a specified name.
        /// </summary>
        /// <remarks>
        /// If given <paramref name="valueName"/> is <code>null</code> or an empty string,
        /// no value is reported.
        /// </remarks>
        /// <param name="valueName">name of this value</param>
        /// <param name="value">value itself</param>
        /// <returns>this Action (for usage as fluent API)</returns>
        IAction ReportValue(string valueName, long value);

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
        ///  Reports an error with a specified name and error code.
        /// </summary>
        /// <remarks>
        /// If given <paramref name="errorName"/> is <code>null</code> or an empty string,
        /// no error is reported.
        /// </remarks>
        /// <param name="errorName">name of this error</param>
        /// <param name="errorCode">numeric error code of this error</param>
        /// <returns>this Action (for usage as fluent API)</returns>
        IAction ReportError(string errorName, int errorCode);

        /// <summary>
        /// Reports an error with a specified name and parameters describing the cause of this error.
        /// </summary>
        /// <remarks>
        /// If given <paramref name="errorName"/> is <code>null</code> or an empty string,
        /// no error is reported.
        /// If the <paramref name="causeDescription"/> is longer than 1000 characters, it is truncated to this value.
        /// If the <paramref name="causeStackTrace"/> is longer than 128.000 characters, it is truncated according to the last line break.
        /// </remarks>
        /// <param name="errorName">name of this error</param>
        /// <param name="causeName">name describing the cuase of the error (e.g. Exception class name).</param>
        /// <param name="causeDescription">description what caused the eror (e.g. Exception message).</param>
        /// <param name="causeStackTrace">stack trace of the error</param>
        /// <returns>this Action (for usage as fluent API)</returns>
        IAction ReportError(string errorName, string causeName, string causeDescription, string causeStackTrace);

        /// <summary>
        /// Reports an error with a specified name and <see cref="Exception"/>
        /// </summary>
        /// If given <paramref name="errorName"/> is <code>null</code> or an empty string,
        /// no error is reported.
        /// </remarks>
        /// <param name="errorName">name of this error</param>
        /// <param name="exception"><see cref="Exception"/> causing this error</param>
        /// <returns>this Action (for usage as fluent API)</returns>
        IAction ReportError(string errorName, Exception exception);

        /// <summary>
        ///  Allows tracing and timing of a web request handled by any 3rd party HTTP Client (e.g. Apache, Google, ...).
        ///  In this case the Dynatrace HTTP header (<see cref="OpenKitConstants.WEBREQUEST_TAG_HEADER"/>) has to be set manually to the
        ///  traces value of this WebRequestTracer.
        ///  If the web request is continued on a server-side Agent (e.g. Java, .NET, ...) this Session will be correlated to
        ///  the resulting server-side PurePath.
        /// </summary>
        /// <remarks>
        /// If given <paramref name="url"/> is <code>null</code> or an empty string, then
        /// a <see cref="Dynatrace.OpenKit.Core.Objects.NullWebRequestTracer"/> is returned and nothing is reported to the server.
        /// </remarks>
        /// <param name="url">the URL of the web request to be traced and timed</param>
        /// <returns>a WebRequestTracer which allows getting the tag value and adding timing information</returns>
        IWebRequestTracer TraceWebRequest(string url);

        /// <summary>
        /// Leaves this <see cref="IAction"/>.
        /// </summary>
        /// <returns>the parent <see cref="IAction"/>, or <c>null</c> if there is no parent <see cref="IAction"/></returns>
        IAction LeaveAction();

        /// <summary>
        /// Cancels this <see cref="IAction"/>.
        /// <para>
        /// Canceling an <see cref="IAction"/> is similar to <see cref="IAction.LeaveAction">leaving an action</see>,
        /// except that the data and all unfinished child objects are discarded instead of being sent.
        /// </para>
        /// </summary>
        /// <returns>the parent <see cref="IAction"/>, or <c>null</c> if there is no parent <see cref="IAction"/></returns>
        IAction CancelAction();

        /// <summary>
        /// Get the <see cref="IAction">action's</see> duration.
        /// <para>
        /// The duration of an <see cref="IAction"/> is equal to the <c>current timestamp - start timestamp</c>,
        /// if the action is still open, or <c>end timestamp - start timestamp</c> if <see cref="IAction.LeaveAction"/>
        /// or <see cref="IAction.CancelAction"/> was already called.
        /// </para>
        /// </summary>
        TimeSpan Duration { get; }
    }
}
