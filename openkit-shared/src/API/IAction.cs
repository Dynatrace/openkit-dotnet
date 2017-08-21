/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
namespace Dynatrace.OpenKit.API {

    /// <summary>
    ///  This interface provides functionality to create (child) Actions, report events/values/errors and tag web requests.
    /// </summary>
    public interface IAction {

        /// <summary>
        ///  Enters a (child) Action with a specified name on this Action.
        /// </summary>
        /// <param name="actionName">name of the Action</param>
        /// <returns>Action instance to work with</returns>
        IAction EnterAction(string actionName);

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
        ///  Reports an error with a specified name, error code and a reason.
        /// </summary>
        /// <param name="errorName">name of this error</param>
        /// <param name="errorCode">numeric error code of this error</param>
        /// <param name="reason">reason for this error</param>
        /// <returns>this Action (for usage as fluent API)</returns>
        IAction ReportError(string errorName, int errorCode, string reason);

        /// <summary>
        ///  Tags a web request - which is provided via an HttpClient - and allows adding timing information to this request.
        ///  If the web request is continued on a server-side Agent (e.g. Java, .NET, ...) this Session will be correlated to
        ///  the resulting server-side PurePath.
        /// </summary>
        /// <param name="httpClient">the URLConnection of the HTTP request to be tagged and timed</param>
        /// <returns>a WebRequestTag which allows adding timing information</returns>
        IWebRequestTag TagWebRequest(System.Net.Http.HttpClient httpClient);

        /// <summary>
        ///  Allows tagging and timing of a web request handled by any 3rd party HTTP Client (e.g. Apache, Google, ...).
        ///  In this case the Dynatrace HTTP header (OpenKitFactory.WEBREQUEST_TAG_HEADER) has to be set manually to the
        ///  tag value of this WebRequestTag.
        ///  If the web request is continued on a server-side Agent (e.g. Java, .NET, ...) this Session will be correlated to
        ///  the resulting server-side PurePath.
        /// </summary>
        /// <param name="url">the URL of the web request to be tagged and timed</param>
        /// <returns>a WebRequestTag which allows getting the tag value and adding timing information</returns>
        IWebRequestTag TagWebRequest(string url);

        /// <summary>
        ///  Leaves this Action.
        /// </summary>
        /// <returns>this Action (for usage as fluent API)</returns>
        IAction LeaveAction();

    }

}
