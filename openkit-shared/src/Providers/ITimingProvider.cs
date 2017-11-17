namespace Dynatrace.OpenKit.Providers
{
    /// <summary>
    /// Interface providing timing related functionality
    /// </summary>
    public interface ITimingProvider
    {
        /// <summary>
        /// Provide the current timestamp in milliseconds.
        /// </summary>
        /// <returns></returns>
        long ProvideTimestampInMilliseconds();

        
        /// <summary>
        /// Sleep given amount of milliseconds.
        /// </summary>
        /// <param name="milliseconds">Milliseconds to sleep</param>
        void Sleep(int milliseconds);
    }
}
