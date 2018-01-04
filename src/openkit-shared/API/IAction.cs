/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
namespace Dynatrace.OpenKit.API
{

    /// <summary>
    ///  This interface provides functionality to report events/values/errors and traces web requests.
    /// </summary>
    public interface IAction
    {

        /// <summary>
        ///  Reports an event with a specified name (but without any value).
        /// </summary>
        /// <param name="eventName">name of the event</param>
        /// <returns>this Action (for usage as fluent API)</returns>
        IAction ReportEvent(string eventName);

        /// <summary>
        ///  Reports an int value with a specified name.
        /// </summary>
        /// <param name="valueName">name of this value</param>
        /// <param name="value">value itself</param>
        /// <returns>this Action (for usage as fluent API)</returns>
        IAction ReportValue(string valueName, int value);

        /// <summary>
        ///  Reports a double value with a specified name.
        /// </summary>
        /// <param name="valueName">name of this value</param>
        /// <param name="value">value itself</param>
        /// <returns>this Action (for usage as fluent API)</returns>
        IAction ReportValue(string valueName, double value);

        /// <summary>
        ///  Reports a string value with a specified name.
        /// </summary>
        /// <param name="valueName">name of this value</param>
        /// <param name="value">value itself</param>
        /// <returns>this Action (for usage as fluent API)</returns>
        IAction ReportValue(string valueName, string value);

        /// <summary>
        ///  Reports an error with a specified name, error code and reason.
        /// </summary>
        /// <param name="errorName">name of this error</param>
        /// <param name="errorCode">numeric error code of this error</param>
        /// <param name="reason">reason for this error</param>
        /// <returns>this Action (for usage as fluent API)</returns>
        IAction ReportError(string errorName, int errorCode, string reason);

#if NET40 || NET35

        /// <summary>
        ///  Traces a web request - which is provided via an WebClient - and allows adding timing information to this request.
        ///  If the web request is continued on a server-side Agent (e.g. Java, .NET, ...) this Session will be correlated to
        ///  the resulting server-side PurePath.
        /// </summary>
        /// <param name="webClient">the WebClient of the HTTP request to be traced and timed</param>
        /// <returns>a WebRequestTracer which allows adding timing information</returns>
        /// <remarks>This method is for .NET 4.0 and .NET 3.5 only, since <code>System.Net.Http.HttpClient</code> is not available.</remarks>
        IWebRequestTracer TraceWebRequest(System.Net.WebClient webClient);

#else

        /// <summary>
        ///  Traces a web request - which is provided via an HttpClient - and allows adding timing information to this request.
        ///  If the web request is continued on a server-side Agent (e.g. Java, .NET, ...) this Session will be correlated to
        ///  the resulting server-side PurePath.
        /// </summary>
        /// <param name="httpClient">the HttpClient of the HTTP request to be traced and timed</param>
        /// <returns>a WebRequestTracer which allows adding timing information</returns>
        IWebRequestTracer TraceWebRequest(System.Net.Http.HttpClient httpClient);

#endif

        /// <summary>
        ///  Allows tracing and timing of a web request handled by any 3rd party HTTP Client (e.g. Apache, Google, ...).
        ///  In this case the Dynatrace HTTP header (OpenKitFactory.WEBREQUEST_TAG_HEADER) has to be set manually to the
        ///  traces value of this WebRequestTracer.
        ///  If the web request is continued on a server-side Agent (e.g. Java, .NET, ...) this Session will be correlated to
        ///  the resulting server-side PurePath.
        /// </summary>
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
