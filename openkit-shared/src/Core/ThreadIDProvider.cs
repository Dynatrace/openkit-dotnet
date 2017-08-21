/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using System.Threading;

namespace Dynatrace.OpenKit.Protocol {

    /// <summary>
    ///  Class for providing a thread ID.
    /// </summary>
    public class ThreadIDProvider {

        public static int ThreadID {
            get {
                return Thread.CurrentThread.ManagedThreadId;
            }
        }

    }

}
