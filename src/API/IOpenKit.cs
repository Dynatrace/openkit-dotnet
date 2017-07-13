/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
namespace Dynatrace.OpenKit.API {

    /// <summary>
    ///  This interface provides basic OpenKit functionality, like creating a Session and shutting down the OpenKit.
    /// </summary>
    public interface IOpenKit {

        /// <summary>
        ///  Initializes the OpenKit, which includes waiting for the OpenKit to receive its initial settings from the Dynatrace/Appmon server.
        ///  Must be done before any other calls to the OpenKit, otherwise those calls to the OpenKit will do nothing.
        /// </summary>
        void Initialize();

        /// <summary>
        ///  Returns the Device used by this OpenKit instance. This can be used to provide basic information, like operating system,
        ///  manufacturer and model information.
        /// </summary>
        /// <returns>Device used by this OpenKit instance</returns>
        IDevice Device { get; }

        /// <summary>
        ///  Creates a Session instance which can then be used to create Actions.
        /// </summary>
        /// <param name="clientIPAddress">client IP address where this Session is coming from</param>
        /// <returns>Session instance to work with</returns>
        ISession CreateSession(string clientIPAddress);

        /// <summary>
        ///  Shuts down the OpenKit, ending all open Sessions and waiting for them to be sent.
        /// </summary>
        void Shutdown();

    }

}
