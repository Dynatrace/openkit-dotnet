/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
namespace Dynatrace.OpenKit.API
{

    /// <summary>
    ///  This interface allows tracing and timing of a web request.
    /// </summary>
    public interface IWebRequestTracer
    {

        /// <summary>
        ///  Returns the Dynatrace tag which has to be set manually as Dynatrace HTTP header (OpenKitFactory.WEBREQUEST_TAG_HEADER).
        ///  This is only necessary for tracing web requests via 3rd party HTTP clients.
        /// </summary>
        /// <returns>the Dynatrace tag to be set as HTTP header value or an empty String if capture is off</returns>
        string Tag { get; }

        /// <summary>
        ///  Starts the web request timing. Should be called when the web request is initiated.
        /// </summary>
        IWebRequestTracer Start();

        /// <summary>
        ///  Stops the web request timing. Should be called when the web request is finished.
        /// </summary>
        void Stop();

        /// <summary>
        ///  Sets the response code on the web request
        /// </summary>
        /// <param name="responseCode">return code provided by server response</param>
        /// <returns></returns>
        IWebRequestTracer SetResponseCode(int responseCode);

        /// <summary>
        /// Sets the amount of sent bytes of the web request
        /// </summary>
        /// <param name="bytesSent">number of bytes sent by the web request</param>
        /// <returns></returns>
        IWebRequestTracer SetBytesSent(int bytesSent);

        /// <summary>
        /// Sets the amount of received bytes of the web request
        /// </summary>
        /// <param name="bytesReceived">number of bytes received by the web request</param>
        /// <returns></returns>
        IWebRequestTracer SetBytesReceived(int bytesReceived);




    }

}
