using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using System.Collections.Generic;
using System.Threading;

namespace Dynatrace.OpenKit.Core.Communication
{
    internal class BeaconSendingContext
    {
        internal const long DEFAULT_SLEEP_TIME_MILLISECONDS = 1000;

        /** container storing all open sessions */
        private readonly SynchronizedQueue<Session> openSessions = new SynchronizedQueue<Session>();

        /** container storing all finished sessions */
        private readonly SynchronizedQueue<Session> finishedSessions = new SynchronizedQueue<Session>();

        /** boolean indicating whether shutdown was requested or not */
        private bool shutdown;

        /** countdown latch updated when init was done - which can either be success or failure */
        private readonly CountdownEvent countdownEvent = new CountdownEvent(1);
        /** boolean indicating whether init was successful or not */
        private bool initSucceeded = false;

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

        public bool IsShutdownRequested { get { return shutdown; } }
        public long CurrentTimestamp { get { return TimingProvider.ProvideTimestampInMilliseconds(); } }
        public int SendInterval { get { return Configuration.SendInterval; } }
        public bool IsCaptureOn { get { return Configuration.IsCaptureOn; } }

        public void ExecuteCurrentState()
        {
            CurrentState.Execute(this);
        }

        public void RequestShutdown()
        {
            shutdown = true;
        }

        public bool WaitForInit()
        {
            countdownEvent.Wait();
            return initSucceeded;
        }

        public void InitCompleted(bool success)
        {
            initSucceeded = success;
            countdownEvent.Signal();
        }

        public HTTPClient GetHTTPClient()
        {
            return HTTPClientProvider.CreateClient(Configuration.HttpClientConfig);
        }

        public void Sleep()
        {
            Sleep(DEFAULT_SLEEP_TIME_MILLISECONDS);
        }

        public void Sleep(long millis)
        {
            TimingProvider.Sleep(millis);
        }
        
        public void HandleStatusResponse(StatusResponse statusResponse)
        {
            Configuration.UpdateSettings(statusResponse);

            if (!Configuration.IsCaptureOn) {
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

        private void ClearAllSessions()
        {
            openSessions.Clear();
            finishedSessions.Clear();
        }
    }
}
