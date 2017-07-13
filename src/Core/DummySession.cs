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

        public IAction EnterAction(string actionName) {
            // return DummyAction and do nothing
            return dummyActionInstance;
        }

        public void End() {
            // do nothing
        }

    }

}
