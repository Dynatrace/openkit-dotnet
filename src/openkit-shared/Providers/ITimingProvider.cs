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

namespace Dynatrace.OpenKit.Providers
{
    /// <summary>
    /// Interface providing timing related functionality
    /// </summary>
    public interface ITimingProvider
    {
        /// <summary>
        /// Returns whether time sync is supported or not
        /// </summary>
        bool IsTimeSyncSupported { get; }

        /// <summary>
        /// Provide the current timestamp in milliseconds in local time.
        /// </summary>
        /// <returns></returns>
        long ProvideTimestampInMilliseconds();

        /// <summary>
        /// Sleep given amount of milliseconds.
        /// </summary>
        /// <param name="milliseconds">Milliseconds to sleep</param>
        void Sleep(int milliseconds);

        /// <summary>
        /// Initialize time provider with cluster time offset
        /// </summary>
        /// <param name="clusterTimeOffset">cluster time offset in milliseconds</param>
        /// <param name="isTimeSyncSupported"><code>true</code> if time sync is supported otherwise <code>false</code></param>
        void Initialze(long clusterTimeOffset, bool isTimeSyncSupported);

        /// <summary>
        /// Converts a local timestamp to cluster time. 
        /// </summary>
        /// <param name="timestamp">timestamp in local time</param>
        /// <returns>Returns local time if not time synced or if not yet initialized</returns>
        long ConvertToClusterTime(long timestamp);
    }
}
