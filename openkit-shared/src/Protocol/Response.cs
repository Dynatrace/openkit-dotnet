/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
namespace Dynatrace.OpenKit.Protocol {

    /// <summary>
    ///  Abstract base class for a response to one of the 3 request types (status check, beacon send, time sync).
    /// </summary>
    public abstract class Response {

        private int responseCode;

        // *** constructors ***

        public Response(int responseCode) {
            this.responseCode = responseCode;
        }

        // *** properties ***

        public int ResponseCode {
            get {
                return responseCode;
            }
        }

    }

}
