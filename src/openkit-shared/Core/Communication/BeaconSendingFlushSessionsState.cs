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

using System;

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// In this state open sessions are finished. After that all sessions are sent to the server.
    /// 
    /// Transitions to:
    /// <ul>
    ///     <li><see cref="BeaconSendingTerminalState"/></li>
    /// </ul> 
    /// </summary>
    internal class BeaconSendingFlushSessionsState : AbstractBeaconSendingState
    {
        public const int BEACON_SEND_RETRY_ATTEMPTS = 0; // do not retry beacon sending on error

        public BeaconSendingFlushSessionsState() : base(false) { }

        internal override AbstractBeaconSendingState ShutdownState => new BeaconSendingTerminalState();

        protected override void DoExecute(IBeaconSendingContext context)
        {
            // end open sessions -> will be flushed afterwards
            var openSessions = context.GetAllOpenSessions();
            foreach (var openSession in openSessions)
            {
                openSession.End();
            }

            // flush finished (and previously ended) sessions
            var finishedSession = context.GetNextFinishedSession();
            while (finishedSession != null)
            {
                finishedSession.SendBeacon(context.HTTPClientProvider, BEACON_SEND_RETRY_ATTEMPTS);
                finishedSession = context.GetNextFinishedSession();
            }

            // make last state transition to terminal state
            context.CurrentState = new BeaconSendingTerminalState();
        }
    }
}
