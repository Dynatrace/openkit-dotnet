using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// State context for beacon sending
    /// </summary>
    internal class BeaconSendingContext : IBeaconSendingContext
    {
        public const int DEFAULT_SLEEP_TIME_MILLISECONDS = 1000;

        // container storing all open sessions 
        private readonly SynchronizedQueue<Session> openSessions = new SynchronizedQueue<Session>();

        // container storing all finished sessions 
        private readonly SynchronizedQueue<Session> finishedSessions = new SynchronizedQueue<Session>();

        // reset event is set when init was done - which can either be success or failure 
        private readonly ManualResetEvent resetEvent = new ManualResetEvent(false);

        // boolean indicating whether shutdown was requested or not (accessed by multiple threads)
        private volatile bool isShutdownRequested = false;
        
        // boolean indicating whether init was successful or not (accessed by multiple threads)
        private volatile bool initSucceeded = false;

        /// <summary>
        /// Constructor
        /// 
        /// Current state is initialized to <see cref="Dynatrace.OpenKit.Core.Communication."/>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="httpClientProvider"></param>
        /// <param name="timingProvider"></param>
        public BeaconSendingContext(AbstractConfiguration configuration, IHTTPClientProvider httpClientProvider, ITimingProvider timingProvider)
        {
            Configuration = configuration;
            HTTPClientProvider = httpClientProvider;
            TimingProvider = timingProvider;

            CurrentState = new BeaconSendingInitState();
        }

        public AbstractConfiguration Configuration { get; }
        public IHTTPClientProvider HTTPClientProvider { get; }
        public ITimingProvider TimingProvider { get; }

        public AbstractBeaconSendingState CurrentState { get; set; }
        public long LastOpenSessionBeaconSendTime { get; set; }
        public long LastStatusCheckTime { get; set; }
        public long LastTimeSyncTime { get; set; }

        public bool IsInitialized => initSucceeded;
        public bool IsTimeSyncSupported { get; private set; }
        public bool IsShutdownRequested
        {
            get
            {
                return isShutdownRequested;
            }
            private set
            {
                isShutdownRequested = value;
            }
        }
        public long CurrentTimestamp { get { return TimingProvider.ProvideTimestampInMilliseconds(); } }
        public int SendInterval { get { return Configuration.SendInterval; } }
        public bool IsCaptureOn { get { return Configuration.IsCaptureOn; } }
        public bool IsInTerminalState { get { return CurrentState.IsTerminalState; } }

        /// <summary>
        /// Disables the time sync support
        /// </summary>
        public void DisableTimeSyncSupport()
        {
            IsTimeSyncSupported = false;
        }

        /// <summary>
        /// Executes the current state
        /// </summary>
        public void ExecuteCurrentState()
        {
            CurrentState.Execute(this);
        }

        /// <summary>
        /// Requests a shotdown
        /// </summary>
        public void RequestShutdown()
        {
            IsShutdownRequested = true;
        }

        /// <summary>
        /// Waits for the init to be finished
        /// </summary>
        /// <returns></returns>
        public bool WaitForInit()
        {
            resetEvent.WaitOne();
            return initSucceeded;
        }

        /// <summary>
        /// Waits for the init to be finished or time
        /// </summary>
        /// <returns></returns>
        public bool WaitForInit(int timeoutMillis)
        {
            resetEvent.WaitOne(TimeSpan.FromMilliseconds(timeoutMillis));
            return initSucceeded;
        }

        /// <summary>
        /// Set the result of the init step. 
        /// </summary>
        /// <param name="success"><code>True</code> if init was successful otherwise false</param>
        public void InitCompleted(bool success)
        {
            initSucceeded = success;
            resetEvent.Set();
        }

        /// <summary>
        /// Returns an instance of HTTPClient using the current configuration
        /// </summary>
        /// <returns></returns>
        public IHTTPClient GetHTTPClient()
        {
            return HTTPClientProvider.CreateClient(Configuration.HttpClientConfig);
        }

        /// <summary>
        /// Sleeps <code>DEFAULT_SLEEP_TIME_MILLISECONDS</code> millis
        /// </summary>
        public void Sleep()
        {
            Sleep(DEFAULT_SLEEP_TIME_MILLISECONDS);
        }

        /// <summary>
        /// Sleeps the given amount of time
        /// </summary>
        /// <param name="millis"></param>
        public void Sleep(int millis)
        {
            TimingProvider.Sleep(millis);
        }
        
        /// <summary>
        /// Updates the configuration based on the provided status response.
        /// </summary>
        /// <param name="statusResponse"></param>
        public void HandleStatusResponse(StatusResponse statusResponse)
        {
            Configuration.UpdateSettings(statusResponse);

            if (!IsCaptureOn) {
                // capture was turned off
                ClearAllSessions();
            }
        }

        public Session GetNextFinishedSession()
        {
            return finishedSessions.Get();
        }

        public List<Session> GetAllOpenSessions()
        {
            return openSessions.ToList();
        }

        public void StartSession(Session session)
        {
            openSessions.Put(session);
        }

        public void FinishSession(Session session)
        {
            if (openSessions.Remove(session))
            {
                finishedSessions.Put(session);
            }
        }

        private void ClearAllSessions()
        {
            openSessions.Clear();
            finishedSessions.Clear();
        }
    }
}
