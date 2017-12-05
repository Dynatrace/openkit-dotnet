/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Core {

    /// <summary>
    ///  Dummy implementation of the ISession interface, used when capture is off.
    /// </summary>
    class DummySession : ISession {

        private static DummyAction dummyActionInstance = new DummyAction();

        public IRootAction EnterAction(string actionName) {
            // return DummyAction and do nothing
            return dummyActionInstance;
        }
        public void ReportCrash(string errorName, string reason, string stacktrace) {
            // do nothing
        }

        public void End() {
            // do nothing
        }

        public void IdentifyUser(string userId)
        {
            // do nothing
        }
    }

}
