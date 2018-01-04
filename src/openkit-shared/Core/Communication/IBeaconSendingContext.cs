using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// State context for beacon sending
    /// </summary>
    internal interface IBeaconSendingContext
    {
        /// <summary>
        /// Returns the configuration
        /// </summary>
        OpenKitConfiguration Configuration { get; }

        /// <summary>
        /// Returns the HTTP client provider
        /// </summary>
        IHTTPClientProvider HTTPClientProvider { get; }

        /// <summary>
        /// Returns the timing provider
        /// </summary>
        ITimingProvider TimingProvider { get; }

        /// <summary>
        /// Returns the current state
        /// </summary>
        AbstractBeaconSendingState CurrentState { get; set; }

        /// <summary>
        /// Last time open sessions were sent 
        /// </summary>
        long LastOpenSessionBeaconSendTime { get; set; }

        /// <summary>
        /// Last time a successful status request was performed
        /// </summary>
        long LastStatusCheckTime { get; set; }

        /// <summary>
        /// Last time a successful time sync was peformed
        /// </summary>
        long LastTimeSyncTime { get; set; }

        /// <summary>
        /// Returns <code>true</code> if the initialization was performed otherwise <code>false</code>
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Returns <code>true</code> if time sync is supported otherwise <code>false</code>
        /// </summary>
        bool IsTimeSyncSupported { get; }

        /// <summary>
        /// Returns <code>true</code> if the time sync was performed otherwise <code>false</code>
        /// </summary>
        bool IsTimeSynced { get; }

        /// <summary>
        /// Returns <code>true</code> if a shutdown was requested otherwise <code>false</code>
        /// </summary>
        bool IsShutdownRequested { get; }

        /// <summary>
        /// Returns the current timestamp in milliseconds since epoch
        /// </summary>
        long CurrentTimestamp { get; }

        /// <summary>
        /// Returns the interval for sending open sessions
        /// </summary>
        int SendInterval { get; }

        /// <summary>
        /// Returns <code>true</code> if capture is enabled otherwise <code>false</code>
        /// </summary>
        bool IsCaptureOn { get; }

        /// <summary>
        /// Returns <code>true</code> if the current state is a terminal state otherwise <code>false</code>
        /// </summary>
        bool IsInTerminalState { get; }

        /// <summary>
        /// Disables the time sync support
        /// </summary>
        void DisableTimeSyncSupport();

        /// <summary>
        /// Executes the current state
        /// </summary>
        void ExecuteCurrentState();

        /// <summary>
        /// Requests a shutdown
        /// </summary>
        void RequestShutdown();

        /// <summary>
        /// Waits for the initialization to be finished
        /// </summary>
        /// <returns>Returns <code>true</code> if initialization was successful otherwise <code>false</code></returns>
        bool WaitForInit();

        /// <summary>
        /// Waits for the initialization to be finished or <code>timeoutMillis</code> milliseconds
        /// </summary>
        /// <param name="timeoutMillis">Milliseconds to wait</param>
        /// <returns>Returns <code>true</code> if initialization was successful otherwise <code>false</code></returns>
        bool WaitForInit(int timeoutMillis);

        /// <summary>
        /// Set the result of the init step. 
        /// </summary>
        /// <param name="success"><code>true</code> if init was successful otherwise <code>false</code></param>
        void InitCompleted(bool success);

        /// <summary>
        /// Initialize time synchronization with cluster time.
        /// </summary>
        /// <param name="clusterTimeOffset">the cluster offset</param>
        /// <param name="isTimeSyncSupported"><code>true</code> if time sync is supported, otherwise <code>false</code></param>
        void InitializeTimeSync(long clusterTimeOffset, bool isTimeSyncSupported);

        /// <summary>
        /// Returns an instance of HTTPClient using the current configuration
        /// </summary>
        /// <returns></returns>
        IHTTPClient GetHTTPClient();

        /// <summary>
        /// Sleeps <code>DEFAULT_SLEEP_TIME_MILLISECONDS</code> millis
        /// </summary>
        void Sleep();

        /// <summary>
        /// Sleeps the given amount of milliseconds
        /// </summary>
        /// <param name="millis"></param>
        void Sleep(int millis);

        /// <summary>
        /// Disables capturing
        /// </summary>
        void DisableCapture();

        /// <summary>
        /// Updates the configuration based on the provided status response.
        /// </summary>
        /// <param name="statusResponse"></param>
        void HandleStatusResponse(StatusResponse statusResponse);

        /// <summary>
        /// Gets the next finished sessions and removes it from the list of finished sessions
        /// </summary>
        /// <returns></returns>
        Session GetNextFinishedSession();

        /// <summary>
        /// Gets a list of all open sessions
        /// </summary>
        /// <returns></returns>
        List<Session> GetAllOpenSessions();

        /// <summary>
        /// Adds the provided session to the list of open sessions
        /// </summary>
        /// <param name="session"></param>
        void StartSession(Session session);

        /// <summary>
        /// Removes the provided session from the list of open sessions and adds it to the list of finished sessions
        /// </summary>
        /// <remarks>If the provided session is not contained in the list of open sessions, it is nod added to the finished sessions</remarks>
        /// <param name="session"></param>
        void FinishSession(Session session);
    }
}
