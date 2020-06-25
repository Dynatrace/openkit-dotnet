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

namespace Dynatrace.OpenKit.API
{
    /// <summary>
    ///  This interface provides the same functionality as IAction, additionally it allows to create child actions
    /// </summary>
    public interface IRootAction : IAction
    {
        /// <summary>
        ///  Enters a child Action with a specified name on this Action.
        /// </summary>
        /// <remarks>
        /// If <paramref name="actionName"/> is <code>null</code> or an empty string,
        /// a <see cref="Dynatrace.OpenKit.Core.Objects.NullAction"/> is entered and therefore no action tracing happens.
        /// </remarks>
        /// <param name="actionName">name of the Action</param>
        /// <returns>Action instance to work with</returns>
        IAction EnterAction(string actionName);
    }
}
