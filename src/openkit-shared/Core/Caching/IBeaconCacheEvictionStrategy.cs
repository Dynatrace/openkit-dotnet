using System;
using System.Collections.Generic;
using System.Text;

namespace Dynatrace.OpenKit.Core.Caching
{
    /// <summary>
    /// An implementor of this interface shall evict <see cref="BeaconCacheEntry"/> based
    /// on strategy's rules.
    /// </summary>
    public interface IBeaconCacheEvictionStrategy
    {
        /// <summary>
        /// Called when this strategy is executed.
        /// </summary>
        void Execute();
    }
}
