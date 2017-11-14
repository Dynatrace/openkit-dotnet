using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// State where no data is captured. Periodically issues a status request to check 
    /// if capuring should be enabled again. The check interval is defined in <code>STATUS_CHECK_INTERVAL</code> 
    /// 
    /// Transitions to
    /// <ul>
    ///     <li><code>BeaconSendingTimeSyncState</code> if IsCaptureOn is <code>true</code> and time sync is required</li>
    ///     <li><code>BeaconSendingStateCaptureOnState</code> if IsCaputreOn is <code>true</code> and time sync is NOT required</li>
    ///     <li><code>BeaconSendingFlushSessionsState</code> on shutdown</li>
    /// </ul>
    /// </summary>
    internal class BeaconSendingStateCaptureOffState : AbstractBeaconSendingState
    {
        /// <summary>
        /// Number of retries for the status request
        /// </summary>
        public const int STATUS_REQUEST_RETRIES = 5;
        /// <summary>
        /// Inital sleep time for retries in milliseconds
        /// </summary>
        public const int INITIAL_RETRY_SLEEP_TIME_MILLISECONDS = 1000;

        /// <summary>
        ///  Time period for re-execute of status check
        /// </summary>
        public const int STATUS_CHECK_INTERVAL = 2 * 60 * 60 * 1000;    // wait 2h (in ms) for next status request

        private StatusResponse statusResponse;

        public BeaconSendingStateCaptureOffState() : base(false) {}

        protected override AbstractBeaconSendingState ShutdownState => new BeaconSendingFlushSessionsState();

        protected override void DoExecute(BeaconSendingContext context)
        {
            var currentTime = context.CurrentTimestamp;

            var delta = (int) (currentTime - (context.LastStatusCheckTime + STATUS_CHECK_INTERVAL));
            if (delta > 0 && !context.IsShutdownRequested)
            {
                // still have some time to sleep
                context.Sleep(delta);
            }

            // send the status request
            SendStatusRequests(context);

            // process the response
            HandleStatusResponse(context);

            // update the last status check time in any case
            context.LastStatusCheckTime = currentTime;
        }

        private void SendStatusRequests(BeaconSendingContext context)
        {
            var retry = 0;
            int sleepTimeInMillis = INITIAL_RETRY_SLEEP_TIME_MILLISECONDS;

            while (retry++ < STATUS_REQUEST_RETRIES && !context.IsShutdownRequested)
            {
                statusResponse = context.GetHTTPClient().SendStatusRequest();
                if (statusResponse != null)
                {
                    break; // got the response
                }

                if (retry < STATUS_REQUEST_RETRIES)
                {
                    context.Sleep(sleepTimeInMillis);
                    sleepTimeInMillis *= 2;
                }
            }
        }

        private void HandleStatusResponse(BeaconSendingContext context)
        {
            if (statusResponse == null)
            {
                return; // nothing to handle
            }

            context.HandleStatusResponse(statusResponse);
            if (context.IsCaptureOn)
            {
                if (BeaconSendingTimeSyncState.IsTimeSyncRequired(context))
                {
                    // switch to time sync
                    context.CurrentState = new BeaconSendingTimeSyncState(false);
                }
                else
                {
                    // switch to capture on
                    context.CurrentState = new BeaconSendingStateCaptureOnState();
                }                
            }
        }
    }
}
