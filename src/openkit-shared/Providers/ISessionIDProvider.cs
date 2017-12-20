namespace Dynatrace.OpenKit.Providers
{
    /// <summary>
    /// Interface providing consecutive session ids
    /// </summary>
    public interface ISessionIDProvider
    {
        /// <summary>
        /// Returns the next SessionID
        /// </summary>
        int GetNextSessionID();
    }
}
