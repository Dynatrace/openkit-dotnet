namespace Dynatrace.OpenKit.Providers
{
    /// <summary>
    /// Interface that provides the thread id
    /// </summary>
    public interface IThreadIDProvider
    {
        /// <summary>
        /// Returns the current thread id
        /// </summary>
        int ThreadID { get; }
    }
}
