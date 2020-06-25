//
// Copyright 2018-2020 Dynatrace LLC
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
using Dynatrace.OpenKit.Util;
using System.Text;

namespace Dynatrace.OpenKit.Core.Objects
{
    /// <summary>
    /// Implementation of a leaf action.
    ///
    /// <para>
    ///     A leaf action is an <see cref="IAction"/> which has no further sub actions.
    ///     Sub objects (e.g.  <see cref="Dynatrace.OpenKit.API.IWebRequestTracer"/>) may still be attached to
    ///     this <see cref="IAction"/>
    /// </para>
    /// </summary>
    public class LeafAction : BaseAction
    {
        /// <summary>
        /// Helper to reduce ToString() effort.
        /// </summary>
        private string toString;

        /// <summary>
        /// Constructor for creating a leaf action instance.
        /// </summary>
        /// <param name="logger">logger used to log information</param>
        /// <param name="parentAction">the root action to which this leaf action belongs to</param>
        /// <param name="name">the name of the action</param>
        /// <param name="beacon">the beacon for retrieving certain data and for sending data</param>
        internal LeafAction(ILogger logger, IRootActionInternals parentAction, string name, IBeacon beacon)
            : base(logger, parentAction, name, beacon)
        {
            ParentAction = parentAction;
        }

        /// <summary>
        /// The parent action of this leaf action
        /// </summary>
        internal override IAction ParentAction { get; }

        public override string ToString()
        {
            return toString ??
                (toString = new StringBuilder(GetType().Name)
                    .Append(" [sn=").Append(Beacon.SessionNumber.ToInvariantString())
                    .Append(", seq=").Append(Beacon.SessionSequenceNumber.ToInvariantString())
                    .Append(", id=").Append(Id.ToInvariantString())
                    .Append(", name=").Append(Name)
                    .Append(", pa=").Append(ParentId.ToInvariantString())
                    .Append("]").ToString());
        }
    }
}