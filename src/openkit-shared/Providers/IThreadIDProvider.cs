namespace Dynatrace.OpenKit.Providers
{
    public interface IThreadIDProvider
    {
        /// <summary>
        /// Returns the current thread id
        /// </summary>
        int ThreadID { get; }
    }
}
