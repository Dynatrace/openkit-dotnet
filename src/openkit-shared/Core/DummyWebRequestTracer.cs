/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Core {

    /// <summary>
    ///  Dummy implementation of the IWebRequestTracer interface, used when capture is off.
    /// </summary>
    class DummyWebRequestTracer : IWebRequestTracer {

        public string Tag {
            get {
                // return empty string
                return "";
            }
        }

        public int ResponseCode {
            set {
                // do nothing
            }
        }

        public void StartTiming() {
            // do nothing
        }

        public void StopTiming() {
            // do nothing
        }
    }

}
