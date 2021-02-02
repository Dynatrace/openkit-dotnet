//
// Copyright 2018-2021 Dynatrace LLC
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

namespace Dynatrace.OpenKit.Core.Objects
{
    /// <summary>
    /// This class is returned as <see cref="RootAction"/> by
    /// <see cref="Session.EnterAction(string)"/> when the <see cref="Session.End()"/>
    /// has been called before.
    /// </summary>
    public class NullRootAction : NullAction, IRootAction
    {
        /// <summary>
        /// Singleton null root action instance
        /// </summary>
        public static readonly  NullRootAction Instance = new NullRootAction();

        /// <summary>
        /// Private constructor only instantiated by <see cref="Instance">singleton instance</see>
        /// </summary>
        private NullRootAction()
        {
        }

        public IAction EnterAction(string actionName)
        {
            return new NullAction(this);
        }
    }
}
