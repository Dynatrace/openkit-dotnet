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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Core.Objects
{
    /// <summary>
    /// This interface represents a <see cref="Session"/> which is internally used.
    ///
    /// <para>
    /// The main purpose of this interface is to make components which require a <see cref="Session"/> to be more easily
    /// testable.
    /// </para>
    /// </summary>
    internal interface ISessionInternals : ISession, IOpenKitComposite
    {
        /// <summary>
        /// Tests if the Session is empty or not
        ///
        /// <para>
        /// A session is considered as empty if it does not contains any action or event data.
        /// </para>
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Returns the current end time of this session.
        ///
        /// <para>
        /// In case the session is not yet ended <code>-1</code> is returned.
        /// </para>
        /// </summary>
        long EndTime { get; }

        /// <summary>
        /// Gets or sets the <see cref="BeaconConfiguration"/> for this session.
        /// </summary>
        IBeaconConfiguration BeaconConfiguration { get; set; }

        /// <summary>
        /// Indicates whether the session is already <see cref="ISession.End()">ended</see> or not
        /// </summary>
        bool IsSessionEnded { get; }

        /// <summary>
        /// Clears the captured beacon data.
        /// </summary>
        void ClearCapturedData();

        /// <summary>
        /// Sends the current <see cref="Beacon"/> state with the <see cref="IHttpClient"/> provided by the given
        /// <see cref="IHttpClientProvider"/>.
        /// </summary>
        /// <param name="clientProvider">the HTTP client provider.</param>
        /// <returns>the response by the server.</returns>
        StatusResponse SendBeacon(IHttpClientProvider clientProvider);
    }
}