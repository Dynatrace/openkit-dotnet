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
    /// State where no data is captured. Periodically issues a status request to check 
    /// if capuring should be enabled again. The check interval is defined in <code>STATUS_CHECK_INTERVAL</code> 
    /// 
    /// Transitions to
    /// <ul>
    ///     <li><see cref="BeaconSendingCaptureOnState"/> if IsCaputreOn is <code>true</code> and time sync is NOT required</li>
    ///     <li><see cref="BeaconSendingFlushSessionsState"/> on shutdown</li>
    ///     <li><see cref="BeaconSendingTimeSyncState"/> if initial time sync failed</li>
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

        public BeaconSendingCaptureOffState() : base(false) { }

        internal override AbstractBeaconSendingState ShutdownState => new BeaconSendingFlushSessionsState();

        protected override void DoExecute(IBeaconSendingContext context)
        {
            context.DisableCapture();

            var currentTime = context.CurrentTimestamp;

            var delta = (int)(STATUS_CHECK_INTERVAL - (currentTime - context.LastStatusCheckTime));
            if (delta > 0 && !context.IsShutdownRequested)
            {
                // still have some time to sleep
                context.Sleep(delta);
            }

            // send the status request
            var statusResponse = BeaconSendingRequestUtil.SendStatusRequest(context, STATUS_REQUEST_RETRIES, INITIAL_RETRY_SLEEP_TIME_MILLISECONDS);

            // process the response
            HandleStatusResponse(context, statusResponse);

            // update the last status check time in any case
            context.LastStatusCheckTime = currentTime;
        }

        private static void HandleStatusResponse(IBeaconSendingContext context, StatusResponse statusResponse)
        {
            if (statusResponse != null)
            {
                context.HandleStatusResponse(statusResponse);
            }
            // if initial time sync failed before
            if (context.IsTimeSyncSupported && !context.IsTimeSynced)
            {
                // then retry initial time sync
                context.CurrentState = new BeaconSendingTimeSyncState(true);
            }
            else if (statusResponse != null && context.IsCaptureOn)
            {
                // capturing is re-enabled again, but only if we received a response from the server
                context.CurrentState = new BeaconSendingCaptureOnState();
            }
        }

        public override string ToString()
        {
            return "CaptureOff";
        }
    }
}
