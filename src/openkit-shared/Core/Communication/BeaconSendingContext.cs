using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public BeaconSendingContext(OpenKitConfiguration configuration, IHTTPClientProvider httpClientProvider, ITimingProvider timingProvider)
        {
            Configuration = configuration;
            HTTPClientProvider = httpClientProvider;
            TimingProvider = timingProvider;

            // set time sync supported to true
            IsTimeSyncSupported = true;
            // set last time sync time to -1
            LastTimeSyncTime = -1;

            // set current state to init state
            CurrentState = new BeaconSendingInitState();
        }

        public OpenKitConfiguration Configuration { get; }
        public IHTTPClientProvider HTTPClientProvider { get; }
        public ITimingProvider TimingProvider { get; }

        public AbstractBeaconSendingState CurrentState { get; set; }
        public long LastOpenSessionBeaconSendTime { get; set; }
        public long LastStatusCheckTime { get; set; }
        public long LastTimeSyncTime { get; set; }
        public bool IsTimeSyncSupported { get; private set; }
        public bool IsTimeSynced
        {
            get { return !IsTimeSyncSupported || LastTimeSyncTime >= 0; }
        }

        public bool IsInitialized => initSucceeded;

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
        /// Gets a readonly list of all finished sessions.
        /// </summary>
        /// <remarks>
        /// This property is only for testing purposes.
        /// </remarks>
        internal ReadOnlyCollection<Session> FinishedSessions => finishedSessions.ToList().AsReadOnly();

        public void DisableTimeSyncSupport()
        {
            IsTimeSyncSupported = false;
        }

        public void ExecuteCurrentState()
        {
            CurrentState.Execute(this);
        }

        public void RequestShutdown()
        {
            IsShutdownRequested = true;
        }

        public bool WaitForInit()
        {
            resetEvent.WaitOne();
            return initSucceeded;
        }

        public bool WaitForInit(int timeoutMillis)
        {
            resetEvent.WaitOne(TimeSpan.FromMilliseconds(timeoutMillis));
            return initSucceeded;
        }

        public void InitCompleted(bool success)
        {
            initSucceeded = success;
            resetEvent.Set();
        }

        public void InitializeTimeSync(long clusterTimeOffset, bool isTimeSyncSupported)
        {
            TimingProvider.Initialze(clusterTimeOffset, isTimeSyncSupported);
        }

        public IHTTPClient GetHTTPClient()
        {
            return HTTPClientProvider.CreateClient(Configuration.HTTPClientConfig);
        }

        public void Sleep()
        {
            Sleep(DEFAULT_SLEEP_TIME_MILLISECONDS);
        }

        public void Sleep(int millis)
        {
            TimingProvider.Sleep(millis);
        }

        public void DisableCapture()
        {
            Configuration.DisableCapture();
            ClearAllSessionData();
        }

        public void HandleStatusResponse(StatusResponse statusResponse)
        {
            Configuration.UpdateSettings(statusResponse);

            if (!IsCaptureOn)
            {
                // capture was turned off
                ClearAllSessionData();
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

        private void ClearAllSessionData()
        {
            var session = finishedSessions.Get();
            while (session != null)
            {
                session.ClearCapturedData();
                session = finishedSessions.Get();
            }

            foreach (var openSession in openSessions.ToList())
            {
                openSession.ClearCapturedData();
            }
        }
    }
}
