using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core
{
    /// <summary>
    /// Actual implementation of the RootAction interface.
    /// </summary>
    public class RootAction : Action, IRootAction
    {
        // Beacon reference
        private readonly Beacon beacon;
        // data structures for managing actions
        private SynchronizedQueue<IAction> openChildActions = new SynchronizedQueue<IAction>();

        // *** constructors ***

        public RootAction(Beacon beacon, string name, SynchronizedQueue<IAction> thisLevelActions)
            : base(beacon, name, thisLevelActions)
        {
            this.beacon = beacon;
        }

        // *** interface methods ***

        public IAction EnterAction(string actionName)
        {
            return new Action(beacon, actionName, this, openChildActions);
        }

        // *** protected methods ***

        protected override IAction DoLeaveAction()
        {
            // leave all open Child-Actions
            while (!openChildActions.IsEmpty())
            {
                IAction action = openChildActions.Get();
                action.LeaveAction();
            }

            return base.DoLeaveAction();
        }
    }
}
