/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Core {

    /// <summary>
    ///  Dummy implementation of the IWebRequestTag interface, used when capture is off.
    /// </summary>
    class DummyWebRequestTag : IWebRequestTag {

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
