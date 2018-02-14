//
// Copyright 2018 Dynatrace LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

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
            if (!IsActionLeft)
            {
                return new Action(beacon, actionName, this, openChildActions);
            }
            return new NullAction(this);
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
