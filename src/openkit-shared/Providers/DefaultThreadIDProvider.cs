/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using System.Threading;

namespace Dynatrace.OpenKit.Providers
{

    /// <summary>
    ///  Class for providing a thread ID.
    /// </summary>
    public class DefaultThreadIDProvider : IThreadIDProvider
    {
        public int ThreadID => Thread.CurrentThread.ManagedThreadId;
    }
}
