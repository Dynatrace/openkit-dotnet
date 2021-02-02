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

using System;
using Dynatrace.OpenKit.Core.Caching;

namespace Dynatrace.OpenKit.Core.Configuration
{
    /// <summary>
    /// Class containing all default values for all configurations
    /// </summary>
    public static class ConfigurationDefaults
    {
        /// <summary>
        /// The default <see cref="BeaconCacheConfiguration"/> when a user does not override it.
        ///
        /// Default settings allow beacons which are max 2 hours old and unbounded memory limits
        /// </summary>
        public static readonly long DefaultMaxRecordAgeInMillis = (long)TimeSpan.FromMinutes(105).TotalMilliseconds; // 1 hour and 45 minutes

        /// <summary>
        /// Defines the default upper memory boundary of the <see cref="IBeaconCache"/>.
        ///
        /// <para>
        ///     The upper boundary is the size limit at which the <see cref="IBeaconCache"/> will start evicting records.
        ///     The default upper boundary is 100 MB.
        /// </para>
        /// </summary>
        public const long DefaultUpperMemoryBoundaryInBytes = 100 * 1024 * 1024;

        /// <summary>
        /// Defines the lower memory boundary of the <see cref="IBeaconCache"/>.
        ///
        /// <para>
        ///     The lower boundary is the size until which the <see cref="IBeaconCache"/> will evict records once the
        ///     upper boundary was exceeded. The default lower boundary is 80 MB.
        /// </para>
        /// </summary>
        public const long DefaultLowerMemoryBoundaryInBytes = 80 * 1024 * 1024;

        /// <summary>
        /// Default data collection level used if no other value was specified
        /// </summary>
        public const DataCollectionLevel DefaultDataCollectionLevel = DataCollectionLevel.USER_BEHAVIOR;

        /// <summary>
        /// Default crash reporting level used if no other value was specified.
        /// </summary>
        public const CrashReportingLevel DefaultCrashReportingLevel = CrashReportingLevel.OPT_IN_CRASHES;
    }
}