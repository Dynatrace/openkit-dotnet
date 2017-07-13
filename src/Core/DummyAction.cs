/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Core {

    /// <summary>
    ///  Dummy implementation of the IAction interface, used when capture is off.
    /// </summary>
    class DummyAction : IAction {

        private static DummyWebRequestTag dummyWebRequestTagInstance = new DummyWebRequestTag();

        public IAction EnterAction(string actionName) {
            // do nothing
            return this;
        }

        public IAction ReportEvent(string eventName) {
            // do nothing
            return this;
        }

        public IAction ReportValue(string valueName, int value) {
            // do nothing
            return this;
        }

        public IAction ReportValue(string valueName, double value) {
            // do nothing
            return this;
        }

        public IAction ReportValue(string valueName, string value) {
            // do nothing
            return this;
        }

        public IAction ReportError(string errorName, int errorCode, string reason) {
            // do nothing
            return this;
        }

        public IWebRequestTag TagWebRequest(System.Net.Http.HttpClient httpClient) {
            // return DummyWebRequestTag and do nothing
            return dummyWebRequestTagInstance;
        }

        public IWebRequestTag TagWebRequest(string url) {
            // return DummyWebRequestTag and do nothing
            return dummyWebRequestTagInstance;
        }

        public IAction LeaveAction() {
            // do nothing
            return this;
        }
    }

}
