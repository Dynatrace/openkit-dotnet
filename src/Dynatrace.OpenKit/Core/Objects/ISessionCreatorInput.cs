//
// Copyright 2018-2021 Dynatrace LLC
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
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Core.Objects
{
    internal interface ISessionCreatorInput
    {
        /// <summary>
        /// Returns the logger to report/trace messages.
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        /// Returns the application/device related configuration.
        /// </summary>
        IOpenKitConfiguration OpenKitConfiguration { get; }

        /// <summary>
        /// Returns the privacy related configuration.
        /// </summary>
        IPrivacyConfiguration PrivacyConfiguration { get; }

        /// <summary>
        /// Returns the beacon cache in which new sessions/beacons will be stored until they are sent.
        /// </summary>
        IBeaconCache BeaconCache { get; }

        /// <summary>
        /// Returns the provider to obtain the next session ID.
        /// </summary>
        ISessionIdProvider SessionIdProvider { get; }

        /// <summary>
        /// Returns the provider to obtain the ID of the current thread.
        /// </summary>
        IThreadIdProvider ThreadIdProvider { get; }

        /// <summary>
        /// Returns the provider to obtain the current timestamp.
        /// </summary>
        ITimingProvider TimingProvider { get; }

        /// <summary>
        /// Returns the current server ID.
        /// </summary>
        /// <returns></returns>
        int CurrentServerId { get; }
    }
}