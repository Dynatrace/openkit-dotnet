using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// State where no data is captured. Periodically issues a status request to check 
    /// if capuring should be enabled again. The check interval is defined in <code>STATUS_CHECK_INTERVAL</code> 
    /// 
    /// Transitions to
    /// <ul>
    ///     <li><code>BeaconSendingStateCaptureOnState</code> if IsCaputreOn is <code>true</code> and time sync is NOT required</li>
    ///     <li><code>BeaconSendingFlushSessionsState</code> on shutdown</li>
    /// </ul>
    /// </summary>
    internal class BeaconSendingCaptureOffState : AbstractBeaconSendingState
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

        public BeaconSendingCaptureOffState() : base(false) {}

        internal override AbstractBeaconSendingState ShutdownState => new BeaconSendingFlushSessionsState();

        protected override void DoExecute(IBeaconSendingContext context)
        {
            var currentTime = context.CurrentTimestamp;

            var delta = (int) (STATUS_CHECK_INTERVAL - (currentTime - context.LastStatusCheckTime));
            if (delta > 0 && !context.IsShutdownRequested)
            {
                // still have some time to sleep
                context.Sleep(delta);
            }

            // send the status request
            var statusResponse = SendStatusRequest(context);

            // process the response
            HandleStatusResponse(context, statusResponse);

            // update the last status check time in any case
            context.LastStatusCheckTime = currentTime;
        }

        private static StatusResponse SendStatusRequest(IBeaconSendingContext context)
        {
            var retry = 0;
            int sleepTimeInMillis = INITIAL_RETRY_SLEEP_TIME_MILLISECONDS;

            StatusResponse statusResponse = null;

            while (true)
            {
                statusResponse = context.GetHTTPClient().SendStatusRequest();

                // if no (valid) status response was received -> sleep 1s [2s, 4s, 8s, 16s] and then retry (max 6 times altogether)
                if (!RetryStatusRequest(context, statusResponse, retry))
                {
                    break;
                }

                context.Sleep(sleepTimeInMillis);
                sleepTimeInMillis *= 2;
                retry++;
            }

            return statusResponse;
        }

        private static void HandleStatusResponse(IBeaconSendingContext context, StatusResponse statusResponse)
        {
            if (statusResponse == null)
            {
                return; // nothing to handle
            }

            context.HandleStatusResponse(statusResponse);
            if (context.IsCaptureOn)
            {
                // transition to capture on state
                context.CurrentState = new BeaconSendingCaptureOnState();
            }
        }

        private static bool RetryStatusRequest(IBeaconSendingContext context, StatusResponse statusResponse, int retry)
        {
            return (statusResponse == null)
                && (retry < STATUS_REQUEST_RETRIES)
                && !context.IsShutdownRequested;
        }
    }
}
