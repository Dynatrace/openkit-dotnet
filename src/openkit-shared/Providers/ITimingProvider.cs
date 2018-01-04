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
        /// Gets the last init time in milliseconds in cluster time or 0 if not yet initialized
        /// </summary>
        long LastInitTimeInClusterTime { get; }

        /// <summary>
        /// Gets the time in milliseconds since last init or since 1970-01-01 if not yet initialized
        /// </summary>
        long TimeSinceLastInitTime { get; }

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

        /// <summary>
        /// Gets the time between provided timestamp and last init in milliseconds, or between provided
        /// timestamp and 1970-01-01 if not yet initialized
        /// </summary>
        /// <param name="timestamp">timestamp in local time</param>
        /// <returns>Time between the provided timestamp and last init</returns>
        long GetTimeSinceLastInitTime(long timestamp);
    }
}
