//
// Copyright 2018 Dynatrace LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// The sending state when init is completed and IsCaputreOn == <code>true</code>
    /// 
    /// Transitions to 
    /// <ul>
    ///     <li><see cref="BeaconSendingTimeSyncState"/> if <code><see cref="BeaconSendingTimeSyncState.IsTimeSyncRequired(IBeaconSendingContext)"/> == true</code></li>
    ///     <li><see cref="BeaconSendingCaptureOffState"/> if IsCaptureOn is <code>false</code></li>
    ///     <li><see cref="BeaconSendingFlushSessionsState"/> on shutdown</li>
    /// </ul>
    /// </summary>
    internal class BeaconSendingCaptureOnState : AbstractBeaconSendingState
    {
        /// <summary>
        /// stores the last status response
        /// </summary>
        private StatusResponse statusResponse = null;

        public BeaconSendingCaptureOnState() : base(false) { }

        internal override AbstractBeaconSendingState ShutdownState => new BeaconSendingFlushSessionsState();

        protected override void DoExecute(IBeaconSendingContext context)
        {
            // every two hours a time sync shall be performed
            if (BeaconSendingTimeSyncState.IsTimeSyncRequired(context))
            {
                // transition to time sync state. if cature on is still true after time sync we will end up in the CaputerOnState again
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
            HandleStatusResponse(context, statusResponse);
        }

        private void SendFinishedSessions(IBeaconSendingContext context)
        {
            // check if there's finished Sessions to be sent -> immediately send beacon(s) of finished Sessions
            var finishedSession = context.GetNextFinishedSession();
            while (finishedSession != null)
            {
                statusResponse = finishedSession.SendBeacon(context.HTTPClientProvider);
                if (statusResponse == null)
                {
                    // something went wrong,
                    if (!finishedSession.IsEmpty)
                    {
                        // well there is more data to send, and we could not do it (now)
                        // just push it back
                        context.PushBackFinishedSession(finishedSession);
                        break; //  sending did not work, break out for now and retry it later
                    }
                }

                // session was sent - so remove it from beacon cache
                finishedSession.ClearCapturedData();
                finishedSession = context.GetNextFinishedSession();
            }
        }

        private void SendOpenSessions(IBeaconSendingContext context)
        {
            var currentTimestamp = context.CurrentTimestamp;
            if (currentTimestamp <= context.LastOpenSessionBeaconSendTime + context.SendInterval)
            {
                return; // some time left until open sessions need to be sent
            }

            var openSessions = context.GetAllOpenSessions();
            foreach (var session in openSessions)
            {
                statusResponse = session.SendBeacon(context.HTTPClientProvider);
            }

            // update open session send timestamp
            context.LastOpenSessionBeaconSendTime = currentTimestamp;
        }

        private static void HandleStatusResponse(IBeaconSendingContext context, StatusResponse statusResponse)
        {
            if (statusResponse == null)
            {
                return; // nothing to handle
            }

            context.HandleStatusResponse(statusResponse);
            if (!context.IsCaptureOn)
            {
                // capturing is turned off -> make state transition
                context.NextState = new BeaconSendingCaptureOffState();
            }
        }
    }
}
