namespace Dynatrace.OpenKit.Providers
{
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
        void Sleep(long milliseconds);
    }
}
