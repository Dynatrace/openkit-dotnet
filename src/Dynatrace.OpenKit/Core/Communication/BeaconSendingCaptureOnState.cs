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
    /// The sending state when init is completed and IsCaptureOn == <code>true</code>
    ///
    /// Transitions to
    /// <ul>
    ///     <li><see cref="BeaconSendingCaptureOffState"/> if IsCaptureOn is <code>false</code></li>
    ///     <li><see cref="BeaconSendingFlushSessionsState"/> on shutdown</li>
    /// </ul>
    /// </summary>
    internal class BeaconSendingCaptureOnState : AbstractBeaconSendingState
    {

        public BeaconSendingCaptureOnState() : base(false) { }

        internal override AbstractBeaconSendingState ShutdownState => new BeaconSendingFlushSessionsState();

        protected override void DoExecute(IBeaconSendingContext context)
        {
            context.Sleep();

            // send new session request for all sessions that are new
            var newSessionsResponse = SendNewSessionRequests(context);
            if (BeaconSendingResponseUtil.IsTooManyRequestsResponse(newSessionsResponse))
            {
                // server is currently overloaded, temporarily switch to capture off
                context.NextState = new BeaconSendingCaptureOffState(newSessionsResponse.GetRetryAfterInMilliseconds());
                return;
            }

            // send all finished sessions
            var finishedSessionsResponse = SendFinishedSessions(context);
            if (BeaconSendingResponseUtil.IsTooManyRequestsResponse(finishedSessionsResponse))
            {
                // server is currently overloaded, temporarily switch to capture off
                context.NextState = new BeaconSendingCaptureOffState(finishedSessionsResponse.GetRetryAfterInMilliseconds());
                return;
            }

            // check if we need to send open sessions & do it if necessary
            var openSessionsResponse = SendOpenSessions(context);
            if (BeaconSendingResponseUtil.IsTooManyRequestsResponse(openSessionsResponse))
            {
                // server is currently overloaded, temporarily switch to capture off
                context.NextState = new BeaconSendingCaptureOffState(openSessionsResponse.GetRetryAfterInMilliseconds());
                return;
            }

            // collect the last status response
            var lastStatusResponse = newSessionsResponse;
            if (openSessionsResponse != null)
            {
                lastStatusResponse = openSessionsResponse;
            }
            else if (finishedSessionsResponse != null)
            {
                lastStatusResponse = finishedSessionsResponse;
            }

            // check if send interval spent -> send current beacon(s) of open Sessions
            HandleStatusResponse(context, lastStatusResponse);
        }

        /// <summary>
        /// Send new session requests for all sessions where we currently don't have a multiplicity configuration.
        /// </summary>
        /// <param name="context">The state context.</param>
        /// <returns>The last status response received.</returns>
        private IStatusResponse SendNewSessionRequests(IBeaconSendingContext context)
        {
            IStatusResponse statusResponse = null;
            var notConfiguredSessions = context.GetAllNotConfiguredSessions();

            foreach(var session in notConfiguredSessions)
            {
                if (!session.CanSendNewSessionRequest)
                {
                    // already exceeded the maximum number of session requests, disable any further data collecting
                    session.DisableCapture();
                    continue;
                }

                statusResponse = context.GetHttpClient().SendNewSessionRequest();
                if (BeaconSendingResponseUtil.IsSuccessfulResponse(statusResponse))
                {
                    var updatedAttributes = context.UpdateLastResponseAttributesFrom(statusResponse);
                    var newConfiguration = ServerConfiguration.From(updatedAttributes);
                    session.UpdateServerConfiguration(newConfiguration);
                }
                else if (BeaconSendingResponseUtil.IsTooManyRequestsResponse(statusResponse))
                {
                    // server is currently overloaded, return immediately
                    break;
                }
                else
                {
                    // any other unsuccessful response
                    session.DecreaseNumRemainingSessionRequests();
                }
            }

            return statusResponse;
        }

        /// <summary>
        /// Send all sessions which have been finished previously.
        /// </summary>
        /// <param name="context">The state's context</param>
        /// <returns>The last status response received.</returns>
        private IStatusResponse SendFinishedSessions(IBeaconSendingContext context)
        {
            IStatusResponse statusResponse = null;
            // check if there's finished Sessions to be sent -> immediately send beacon(s) of finished Sessions
            var finishedSessions = context.GetAllFinishedAndConfiguredSessions();

            foreach (var session in finishedSessions)
            {
                if (session.IsDataSendingAllowed)
                {
                    statusResponse = session.SendBeacon(context.HttpClientProvider);
                    if (!BeaconSendingResponseUtil.IsSuccessfulResponse(statusResponse))
                    {
                        // something went wrong,
                        if (BeaconSendingResponseUtil.IsTooManyRequestsResponse(statusResponse) || !session.IsEmpty)
                        {
                            break; //  sending did not work, break out for now and retry it later
                        }
                    }
                }

                // session was sent - so remove it from beacon cache
                context.RemoveSession(session);
                session.ClearCapturedData();
            }

            return statusResponse;
        }

        /// <summary>
        /// Check if the send interval (configured by server) has expired and start to send open sessions if it has expired.
        /// </summary>
        /// <param name="context">The state's context</param>
        /// <returns>The last status response received.</returns>
        private IStatusResponse SendOpenSessions(IBeaconSendingContext context)
        {
            IStatusResponse statusResponse = null;

            var currentTimestamp = context.CurrentTimestamp;
            if (currentTimestamp <= context.LastOpenSessionBeaconSendTime + context.SendInterval)
            {
                return null; // some time left until open sessions need to be sent
            }

            var openSessions = context.GetAllOpenAndConfiguredSessions();
            foreach (var session in openSessions)
            {
                if (session.IsDataSendingAllowed)
                {
                    statusResponse = session.SendBeacon(context.HttpClientProvider);
                    if (BeaconSendingResponseUtil.IsTooManyRequestsResponse(statusResponse))
                    {
                        // server is currently overloaded, return immediately
                        break;
                    }
                }
                else
                {
                    session.ClearCapturedData();
                }
            }

            // update open session send timestamp
            context.LastOpenSessionBeaconSendTime = currentTimestamp;

            return statusResponse;
        }

        private static void HandleStatusResponse(IBeaconSendingContext context, IStatusResponse statusResponse)
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
