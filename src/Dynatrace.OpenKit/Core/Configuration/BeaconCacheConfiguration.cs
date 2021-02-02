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

using Dynatrace.OpenKit.Core.Caching;

namespace Dynatrace.OpenKit.Core.Configuration
{
    /// <summary>
    /// Configuration for <see cref="BeaconCache"/>
    /// </summary>
    public class BeaconCacheConfiguration : IBeaconCacheConfiguration
    {
        #region constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="builder">OpenKit builder providing the required configuration.</param>
        private BeaconCacheConfiguration(IOpenKitBuilder builder)
        {
            MaxRecordAge = builder.BeaconCacheMaxBeaconAge;
            CacheSizeLowerBound = builder.BeaconCacheLowerMemoryBoundary;
            CacheSizeUpperBound = builder.BeaconCacheUpperMemoryBoundary;
        }

        /// <summary>
        /// Creates a new <see cref="IBeaconCacheConfiguration"/> from the given <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">the OpenKit builder for which to create a <see cref="IBeaconCacheConfiguration"/></param>
        /// <returns>
        ///     a newly created <see cref="IBeaconCacheConfiguration"/> or <code>null</code> if the given
        ///     <paramref name="builder">argument</paramref> is <code>null</code>.
        /// </returns>
        internal static IBeaconCacheConfiguration From(IOpenKitBuilder builder)
        {
            if (builder == null)
            {
                return null;
            }

            return new BeaconCacheConfiguration(builder);
        }

        #endregion

        /// <summary>
        /// Get maximum record age in millisecond.s
        /// </summary>
        public long MaxRecordAge { get; }
        /// <summary>
        /// Get lower memory limit for cache.
        /// </summary>
        public long CacheSizeLowerBound { get; }
        /// <summary>
        /// Get upper memory limit for cache.
        /// </summary>
        public long CacheSizeUpperBound { get; }
    }
}
