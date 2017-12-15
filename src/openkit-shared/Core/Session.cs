/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Core
{

    /// <summary>
    ///  Actual implementation of the ISession interface.
    /// </summary>
    public class Session : ISession
    {
        // end time of this Session
        private long endTime = -1;

        // Configuration and Beacon reference
        private readonly BeaconSender beaconSender;
        private readonly Beacon beacon;

        // used for taking care to really leave all Actions at the end of this Session
        private SynchronizedQueue<IAction> openRootActions = new SynchronizedQueue<IAction>();

        // *** constructors ***

        public Session(BeaconSender beaconSender, Beacon beacon)
        {
            this.beaconSender = beaconSender;
            this.beacon = beacon;

            beaconSender.StartSession(this);
        }

        // *** ISession interface methods ***

        public IRootAction EnterAction(string actionName)
        {
            return new RootAction(beacon, actionName, openRootActions);
        }

        public void ReportCrash(string errorName, string reason, string stacktrace)
        {
            beacon.ReportCrash(errorName, reason, stacktrace);
        }

        public void End()
        {
            // check if end() was already called before by looking at endTime
            if (endTime != -1)
            {
                return;
            }

            // leave all Root-Actions for sanity reasons
            while (!openRootActions.IsEmpty())
            {
                IAction action = openRootActions.Get();
                action.LeaveAction();
            }

            endTime = beacon.CurrentTimestamp;

            // create end session data on beacon
            beacon.EndSession(this);

            // finish session on configuration and stop managing it
            beaconSender.FinishSession(this);
        }

        // *** public methods ***

        // sends the current Beacon state
        public StatusResponse SendBeacon(IHTTPClientProvider clientProvider, int numRetries)
        {
            return beacon.Send(clientProvider, numRetries);
        }

        public void IdentifyUser(string userTag)
        {
            beacon.IdentifyUser(userTag);
        }

        /// <summary>
        /// Clear captured beacon data.
        /// </summary>
        internal void ClearCapturedData()
        {
            beacon.ClearData();
        }

        // *** properties ***

        public long EndTime
        {
            get
            {
                return endTime;
            }
        }

    }
}
