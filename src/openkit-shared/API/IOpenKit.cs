/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
namespace Dynatrace.OpenKit.API
{

    /// <summary>
    ///  This interface provides basic OpenKit functionality, like creating a Session and shutting down the OpenKit.
    /// </summary>
    public interface IOpenKit
    {

        /// <summary>
        /// Waits until OpenKit is fully initialized.
        /// </summary>
        /// 
        /// <remarks>
        /// The calling thread is blocked until OpenKit is fully initialized or until OpenKit is shut down using the
        /// <see cref="IOpenKit.Shutdown"/>@link #shutdown()} method.
        /// 
        /// Due to misconfiguration in the {@link com.dynatrace.openkit.OpenKitFactory} when creating the 
        /// instance (e.g.wrong endpoint URL) this method might hang indefinitely, unless <see cref="IOpenKit.Shutdown"/> is called.
        /// </remarks>
        /// 
        /// <returns><code>true</code> if OpenKit is fully initialized, <code>false</code> when a shutdown request was made. </returns>
        bool WaitForInitCompletion();

        /// <summary>
        /// Waits until OpenKit is fully initialized or the given timeout expired.
        /// </summary>
        /// 
        /// <remarks>
        /// The calling thread is blocked until OpenKit is fully initialized or until OpenKit is shut down using the
        /// <see cref="IOpenKit.Shutdown"/> method or the timeout expired.
        /// 
        /// Due to misconfiguration in the {@link com.dynatrace.openkit.OpenKitFactory} when creating the 
        /// instance (e.g.wrong endpoint URL) this method might hang indefinitely, unless <see cref="IOpenKit.Shutdown"/> is called or timeout expires.
        /// </remarks>
        /// 
        /// <returns><code>true</code> if OpenKit is fully initialized, <code>false</code> when a shutdown request was made. </returns>
        bool WaitForInitCompletion(int timeoutMillis);
        
        /// <summary>
        /// Gets a boolean flag indicating whether OpenKit is initialized or not.
        /// </summary>
        /// <returns><code>true</code> if OpenKit is fully initialized, <code>false</code> if OpenKit still performs initialization.</returns>
        bool IsInitialized { get; }

        /// <summary>
        ///  Defines the version of the application.
        /// </summary>
        /// <param name="value">application version</param>
        string ApplicationVersion { set; }

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
