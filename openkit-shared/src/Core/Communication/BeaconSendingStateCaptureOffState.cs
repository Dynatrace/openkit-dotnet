using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// State where no data is captured. Periodically issues a status request to check 
    /// if capuring should be enabled again. The check interval is defined in <code>STATUS_CHECK_INTERVAL</code> 
    /// 
    /// Transitions to
    /// <ul>
    ///     <li><code>BeaconSendingTimeSyncState</code> if IsCaptureOn is <code>true</code></li>
    ///     <li><code>BeaconSendingFlushSessionsState</code> on shutdown</li>
    /// </ul>
    /// </summary>
    internal class BeaconSendingStateCaptureOffState : AbstractBeaconSendingState
    {
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
            statusResponse = context.GetHTTPClient().SendStatusRequest();

            HandleStatusResponse(context);

            // update the last status check time in any case
            context.LastStatusCheckTime = currentTime;
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
                // capturing is turned on -> make state transition to CaptureOne (via TimeSync)
                context.CurrentState = new BeaconSendingTimeSyncState(false);
            }
        }
    }
}
