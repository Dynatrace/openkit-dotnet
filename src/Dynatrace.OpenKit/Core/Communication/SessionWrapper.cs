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
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// Wrapper around the <see cref="Session"/> which holds additional data
    /// required only in the communication package, therefore leave it package internal.
    /// </summary>
    internal class SessionWrapper
    {
        /// <summary>
        /// The maximum number of "new session requests" to send per session.
        /// </summary>
        private const int MaxNewSessionRequests = 4;

        private volatile bool beaconConfigurationSet = false;
        private volatile bool sessionFinished = false;

        /// <summary>
        /// Constructor taking the wrapped <see cref="Session"/>.
        /// </summary>
        /// <param name="session">The wrapped session.</param>
        internal SessionWrapper(Session session)
        {
            Session = session;
            NumNewSessionRequestsLeft = MaxNewSessionRequests;
        }

        /// <summary>
        /// Get the wrapped session.
        /// </summary>
        internal Session Session { get; }

        /// <summary>
        /// Get a boolean flag indicating whether <see cref="UpdateBeaconConfiguration(BeaconConfiguration)"/>
        /// has been called before.
        /// </summary>
        internal bool IsBeaconConfigurationSet => beaconConfigurationSet;

        /// <summary>
        /// Get Session's BeaconConfiguration.
        /// </summary>
        internal BeaconConfiguration BeaconConfiguration => Session.BeaconConfiguration;

        /// <summary>
        /// Get a boolean flag indicating whether this session is finished or not.
        /// </summary>
        internal bool IsSessionFinished => sessionFinished;

        /// <summary>
        /// Get and set the Number of new session requests left.
        /// </summary>
        internal int NumNewSessionRequestsLeft { get; private set; }

        /// <summary>
        /// Get a flag indicating whether new session request can be sent, or not.
        /// </summary>
        internal bool CanSendNewSessionRequest => NumNewSessionRequestsLeft > 0;

        /// <summary>
        /// Test if this Session is empty or not.
        /// </summary>
        internal bool IsEmpty => Session.IsEmpty;

        /// <summary>
        /// Get a boolean flag indicating if data sending is allowed or not.
        /// </summary>
        internal bool IsDataSendingAllowed => IsBeaconConfigurationSet && BeaconConfiguration.Multiplicity > 0;

        /// <summary>
        /// Update the beacon's configuration.
        /// </summary>
        /// <param name="beaconConfiguration">The new beacon configuration.</param>
        internal void UpdateBeaconConfiguration(BeaconConfiguration beaconConfiguration)
        {
            Session.BeaconConfiguration = beaconConfiguration;
            beaconConfigurationSet = true;
        }

        /// <summary>
        /// Finish this session.
        /// </summary>
        internal void FinishSession()
        {
            sessionFinished = true;
        }

        /// <summary>
        /// Will be called each time a new session request was made for a session.
        /// </summary>
        internal void DecreaseNumNewSessionRequests() {
            NumNewSessionRequestsLeft -= 1;
        }
        
        /// <summary>
        /// Clear captured data.
        /// </summary>
        internal void ClearCapturedData()
        {
            Session.ClearCapturedData();
        }

        /// <summary>
        /// Send Beacon.
        /// </summary>
        /// <param name="httpClientProvider"></param>
        /// <returns></returns>
        internal StatusResponse SendBeacon(IHTTPClientProvider httpClientProvider)
        {
            return Session.SendBeacon(httpClientProvider);
        }
        
        /// <summary>
        /// Ends the session.
        /// </summary>
        internal void End()
        {
            Session.End();
        }
    }
}
