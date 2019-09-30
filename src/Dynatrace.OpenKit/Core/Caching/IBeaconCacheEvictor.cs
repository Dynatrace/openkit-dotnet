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

namespace Dynatrace.OpenKit.Core.Caching
{
    /// <summary>
    /// This interface represents a <see cref="BeaconCacheEvictor"/> for internal usage.
    ///
    /// <para>
    /// The main purpose of this interface is to make components which require a <see cref="BeaconCacheEvictor"/> to
    /// be more easily testable.
    /// </para>
    /// </summary>
    internal interface IBeaconCacheEvictor
    {
        /// <summary>
        /// Property giving <code>true</code> if eviction was started by calling <see cref="Start"/>,
        /// otherwise <code>false</code> is returned.
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Starts the eviction thread.
        /// </summary>
        /// <returns>
        /// <code>true</code> if the eviction thread was started, <code>false</code> if the thread was already running.
        /// </returns>
        bool Start();

        /// <summary>
        /// Stops the eviction thread if the thread is alive and joins the thread.
        /// </summary>
        /// <remarks>
        /// This method calls <see cref="Stop(int)"/> with default join timeout.
        /// </remarks>
        /// <returns>
        /// <code>true</code> if stopping was successful,
        /// <code>false</code> if eviction thread is not running or could not be stopped in time.
        /// </returns>
        bool Stop();

        /// <summary>
        /// Stops the eviction thread if the thread is alive and joins the thread with given timeout in milliseconds.
        /// </summary>
        /// <returns>
        /// <code>true</code> if stopping was successful,
        /// <code>false</code> if eviction thread is not running or could not be stopped in time.
        /// </returns>
        bool Stop(int joinTimeout);
    }
}