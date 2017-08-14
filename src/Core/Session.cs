/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core {

    /// <summary>
    ///  Actual implementation of the ISession interface.
    /// </summary>
    public class Session : ISession {

        // end time of this Session
        private long endTime = -1;

        // Configuration and Beacon reference
        private Configuration configuration;
        private Beacon beacon;

        // used for taking care to really leave all Actions at the end of this Session
        private SynchronizedQueue<IAction> openRootActions = new SynchronizedQueue<IAction>();

        // *** constructors ***

        public Session(Configuration configuration, string clientIPAddress) {
            this.configuration = configuration;

            // beacon has to be created immediately, as the session start time is taken at beacon construction
            beacon = new Beacon(configuration, clientIPAddress);
            configuration.StartSession(this);
        }

        // *** ISession interface methods ***

        public IAction EnterAction(string actionName) {
            return new Action(beacon, actionName, openRootActions);
        }

        public void ReportCrash(string errorName, string reason, string stacktrace) {
            beacon.ReportCrash(errorName, reason, stacktrace);
        }

        public void End() {
            // check if end() was already called before by looking at endTime
            if (endTime != -1) {
                return;
            }

            // leave all Root-Actions for sanity reasons
            while (!openRootActions.IsEmpty()) {
                IAction action = openRootActions.Get();
                action.LeaveAction();
            }

            endTime = TimeProvider.GetTimestamp();

            // create end session data on beacon
            beacon.EndSession(this);

            // finish session on configuration and stop managing it
            configuration.FinishSession(this);
        }

        // *** public methods ***

        // sends the current Beacon state
        public StatusResponse SendBeacon() {
            return beacon.Send();
        }

        // *** properties ***

        public long EndTime {
            get {
                return endTime;
            }
        }

    }

}
