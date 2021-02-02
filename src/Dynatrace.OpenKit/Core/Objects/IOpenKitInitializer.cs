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
    /// <summary>
    /// Provides relevant data for initializing / creating an <see cref="OpenKit"/> instance.
    /// </summary>
    internal interface IOpenKitInitializer
    {
        /// <summary>
        /// Logger for reporting messages.
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        /// Privacy settings determining which kind of data is collected.
        /// </summary>
        IPrivacyConfiguration PrivacyConfiguration { get; }

        /// <summary>
        /// OpenKit / application related configuration
        /// </summary>
        IOpenKitConfiguration OpenKitConfiguration { get; }

        /// <summary>
        /// Provider to obtain the current timestamp
        /// </summary>
        ITimingProvider TimingProvider { get; }

        /// <summary>
        /// Provider for the identifier of the current thread.
        /// </summary>
        IThreadIdProvider ThreadIdProvider { get; }

        /// <summary>
        /// Provider to obtain the identifier for the next session.
        /// </summary>
        ISessionIdProvider SessionIdProvider { get; }

        /// <summary>
        /// Cache where beacon data is stored until it is sent.
        /// </summary>
        IBeaconCache BeaconCache { get; }

        /// <summary>
        /// Eviction thread to prevent beacon cache from overflowing.
        /// </summary>
        IBeaconCacheEvictor BeaconCacheEvictor { get; }

        /// <summary>
        /// Sender thread for sending beacons to the server.
        /// </summary>
        IBeaconSender BeaconSender { get; }

        /// <summary>
        /// Watchdog thread to perform certain actions for sessions at/after a specific time.
        /// </summary>
        ISessionWatchdog SessionWatchdog { get; }
    }
}