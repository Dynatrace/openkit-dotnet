using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// The sending state when init is completed and IsCaputreOn == <code>true</code>
    /// 
    /// Transitions to 
    /// <ul>
    ///     <li><code>BeaconSendingTimeSyncState</code> if <code>BeaconSendingTimeSyncState.IsTimeSyncRequired == true</code></li>
    ///     <li><code>BeaconSendingStateCaptureOffState</code> if IsCaptureOn is <code>false</code></li>
    ///     <li><code>BeaconSendingFlushSessionsState</code> on shutdown</li>
    /// </ul>
    /// </summary>
    internal class BeaconSendingStateCaptureOnState : AbstractBeaconSendingState
    {
        public const int BEACON_SEND_RETRY_ATTEMPTS = 3;

        /// <summary>
        /// stores the last status response
        /// </summary>
        private StatusResponse statusResponse = null;

        public BeaconSendingStateCaptureOnState() : base(false) {}

        protected override AbstractBeaconSendingState ShutdownState => new BeaconSendingFlushSessionsState();

        protected override void DoExecute(BeaconSendingContext context)
        {
            // every two hours a time sync shall be performed
            if (BeaconSendingTimeSyncState.IsTimeSyncRequired(context))
            {
                context.CurrentState = new BeaconSendingTimeSyncState();
                return;
            }

            context.Sleep();

            statusResponse = null;

            // send all finished sessions
            SendFinishedSessions(context);

            // check if we need to send open sessions & do it if necessary
            SendOpenSessions(context);

            // check if send interval spent -> send current beacon(s) of open Sessions
            HandleStatusResponse(context);
        }

        private void SendFinishedSessions(BeaconSendingContext context)
        {
            // check if there's finished Sessions to be sent -> immediately send beacon(s) of finished Sessions
            var finishedSession = context.GetNextFinishedSession();
            while (finishedSession != null)
            {
                finishedSession.SendBeacon(context.HTTPClientProvider, BEACON_SEND_RETRY_ATTEMPTS);
                finishedSession = context.GetNextFinishedSession();
            }
        }

        private void SendOpenSessions(BeaconSendingContext context)
        {
            var currentTimestamp = context.CurrentTimestamp;
            if (currentTimestamp <= context.LastOpenSessionBeaconSendTime + context.SendInterval)
            {
                return; // still some time to send open sessions
            }

            var openSessions = context.GetAllOpenSessions();
            foreach (var session in openSessions)
            {
                session.SendBeacon(context.HTTPClientProvider, BEACON_SEND_RETRY_ATTEMPTS);
            }
        }

        private void HandleStatusResponse(BeaconSendingContext context)
        {
            if (statusResponse == null)
            {
                return; // nothing to handle
            }

            context.HandleStatusResponse(statusResponse);
            if (!context.IsCaptureOn)
            {
                // capturing is turned off -> make state transition
                context.CurrentState = new BeaconSendingStateCaptureOffState();
            }
        }        
    }
}
