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
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Protocol
{
    /// <summary>
    /// Provides relevant data for initializing/ creating a <see cref="Beacon"/>
    /// </summary>
    public interface IBeaconInitializer
    {
        /// <summary>
        /// Returns the logger for reporting messages.
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        /// Returns the cache where the data of the beacon is stored until it gets sent.
        /// </summary>
        IBeaconCache BeaconCache { get; }

        /// <summary>
        /// Returns the client IP address of the session / beacon.
        /// </summary>
        string ClientIpAddress { get; }

        /// <summary>
        /// Returns the <see cref="ISessionIdProvider"/> to obtain the identifier for a session / beacon.
        /// </summary>
        ISessionIdProvider SessionIdProvider { get; }

        /// <summary>
        /// Returns the sequence number for the beacon/session for identification in case of session split by events.
        /// The session sequence number complements the session ID.
        /// </summary>
        int SessionSequenceNumber { get; }

        /// <summary>
        /// Returns the <see cref="IThreadIdProvider"/> to obtain the identifier of the current thread.
        /// </summary>
        IThreadIdProvider ThreadIdProvider { get; }

        /// <summary>
        /// Returns the <see cref="ITimingProvider"/> to obtain the current timestamp.
        /// </summary>
        ITimingProvider TimingProvider { get; }

        /// <summary>
        /// Returns the <see cref="IPrnGenerator"/> to obtain random numbers (e.g. for randomizing device IDs).
        /// </summary>
        IPrnGenerator RandomNumberGenerator { get; }
    }
}