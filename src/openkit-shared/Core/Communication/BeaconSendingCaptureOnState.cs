//
// Copyright 2018-2019 Dynatrace LLC
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

using Dynatrace.OpenKit.Core.Configuration;
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
                context.NextState = new BeaconSendingTimeSyncState();
                return;
            }

            context.Sleep();

            // send new session request for all sessions that are new
            SendNewSessionRequests(context);

            statusResponse = null;

            // send all finished sessions
            SendFinishedSessions(context);

            // check if we need to send open sessions & do it if necessary
            SendOpenSessions(context);

            // check if send interval spent -> send current beacon(s) of open Sessions
            HandleStatusResponse(context, statusResponse);
        }

        private void SendNewSessionRequests(IBeaconSendingContext context)
        {
            var newSessions = context.NewSessions;

            foreach(var newSession in newSessions)
            {
                if (!newSession.CanSendNewSessionRequest)
                {
                    // already exceeded the maximum number of session requests, disable any further data collecting
                    var currentConfiguration = newSession.BeaconConfiguration;
                    var newConfiguration = new BeaconConfiguration(0,
                        currentConfiguration.DataCollectionLevel, currentConfiguration.CrashReportingLevel);
                    newSession.UpdateBeaconConfiguration(newConfiguration);
                    continue;
                }

                var response = context.GetHTTPClient().SendNewSessionRequest();
                if (response != null)
                {
                    var currentConfiguration = newSession.BeaconConfiguration;
                    var newConfiguration = new BeaconConfiguration(response.Multiplicity,
                        currentConfiguration.DataCollectionLevel, currentConfiguration.CrashReportingLevel);
                    newSession.UpdateBeaconConfiguration(newConfiguration);
                }
                else
                {
                    // did not retrieve any response from server, maybe the cluster is down?
                    newSession.DecreaseNumNewSessionRequests();
                }
            }
        }

        private void SendFinishedSessions(IBeaconSendingContext context)
        {
            // check if there's finished Sessions to be sent -> immediately send beacon(s) of finished Sessions
            var finishedSessions = context.FinishedAndConfiguredSessions;

            foreach (var session in finishedSessions)
            {
                if (session.IsDataSendingAllowed)
                {
                    statusResponse = session.SendBeacon(context.HTTPClientProvider);
                    if (statusResponse == null)
                    {
                        // something went wrong,
                        if (!session.IsEmpty)
                        {
                            break; //  sending did not work, break out for now and retry it later
                        }
                    }
                }

                // session was sent - so remove it from beacon cache
                context.RemoveSession(session);
                session.ClearCapturedData();
            }
        }

        private void SendOpenSessions(IBeaconSendingContext context)
        {
            var currentTimestamp = context.CurrentTimestamp;
            if (currentTimestamp <= context.LastOpenSessionBeaconSendTime + context.SendInterval)
            {
                return; // some time left until open sessions need to be sent
            }

            var openSessions = context.OpenAndConfiguredSessions;
            foreach (var session in openSessions)
            {
                if (session.IsDataSendingAllowed)
                {
                    statusResponse = session.SendBeacon(context.HTTPClientProvider);
                }
                else
                {
                    session.ClearCapturedData();
                }
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
        public override string ToString()
        {
            return "CaptureOn";
        }
    }
}
