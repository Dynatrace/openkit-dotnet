using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// Initial state for beacon sending.
    /// 
    /// The initial state is used to retrieve the configuration from the server and update the configuration.
    /// 
    /// Transitions to:
    /// <ul>
    ///     <li><code>BeaconSendingTerminalState</code> if initial status request failed or on shutdown</li>
    ///     <li><code>BeaconSendingTimeSyncState</code> if initial status request succeeded</li>
    /// </ul>
    /// </summary>
    internal class BeaconSendingInitState : AbstractBeaconSendingState
    {
        public const int MAX_INITIAL_STATUS_REQUEST_RETRIES = 5;
        public const long INITIAL_RETRY_SLEEP_TIME_MILLISECONDS = 1000;

        internal BeaconSendingInitState() : base(false) { }

        protected override AbstractBeaconSendingState ShutdownState => new BeaconSendingTerminalState();

        protected override void OnInterrupted(BeaconSendingContext context)
        {
            context.InitCompleted(false);
        }

        protected override void DoExecute(BeaconSendingContext context)
        {
            var currentTimestamp = context.CurrentTimestamp;
            context.LastOpenSessionBeaconSendTime = currentTimestamp;
            context.LastStatusCheckTime = currentTimestamp;

            StatusResponse statusResponse;
            var retry = 0;
            var sleepTimeInMillis = INITIAL_RETRY_SLEEP_TIME_MILLISECONDS;
            do
            {
                retry++;
                statusResponse = context.GetHTTPClient().SendStatusRequest();

                // if no (valid) status response was received -> sleep 2s [4s, 8s, ...] and then retry (max 5 times altogether)
                if (statusResponse == null)
                {
                    context.Sleep(sleepTimeInMillis);
                    sleepTimeInMillis *= 2;
                }
            }
            while (!context.IsShutdownRequested && (statusResponse == null) && (retry < MAX_INITIAL_STATUS_REQUEST_RETRIES));

            if (context.IsShutdownRequested || (statusResponse == null))
            {
                // initial configuration request was either terminated from outside or the config could not be retrieved
                // TODO stefan.eberl@dynatrace.com - Define some better error handling if init failed!
                context.InitCompleted(false);
                context.CurrentState = new BeaconSendingTerminalState();
            }
            else
            {
                // success -> continue with time sync
                context.HandleStatusResponse(statusResponse);
                context.CurrentState = new BeaconSendingTimeSyncState(true);
            }
        }
    }
}
