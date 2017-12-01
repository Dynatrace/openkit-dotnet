/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
namespace Dynatrace.OpenKit.API {

    /// <summary>
    ///  This interface allows tracing and timing of a web request.
    /// </summary>
    public interface IWebRequestTracer {

        /// <summary>
        ///  Returns the Dynatrace tag which has to be set manually as Dynatrace HTTP header (OpenKitFactory.WEBREQUEST_TAG_HEADER).
        ///  This is only necessary for tracing web requests via 3rd party HTTP clients.
        /// </summary>
        /// <returns>the Dynatrace tag to be set as HTTP header value or an empty String if capture is off</returns>
        string Tag { get; }

        /// <summary>
        ///  Sets the response code of this web request. Has to be called before WebRequestTracer.stopTiming().
        /// </summary>
        int ResponseCode { set; }

        /// <summary>
        ///  Starts the web request timing. Should be called when the web request is initiated.
        /// </summary>
        void Start();

        /// <summary>
        ///  Stops the web request timing. Should be called when the web request is finished.
        /// </summary>
        void Stop();

    }

}
