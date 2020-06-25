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

namespace Dynatrace.OpenKit.Core.Objects
{
    /// <summary>
    /// Defines a creator for new sessions.
    /// </summary>
    internal interface ISessionCreator
    {
        /// <summary>
        /// Returns a newly created <see cref="ISessionInternals"/>
        /// </summary>
        /// <param name="parent">the parent composite of the session to create.</param>
        ISessionInternals CreateSession(IOpenKitComposite parent);

        /// <summary>
        /// Resets the internal state of this session creator. A reset includes the following:
        ///
        /// <list type="bullet">
        /// <item>resetting the consecutive session sequence number which is increased every time a session is created.</item>
        /// <item>use a new session ID (which will stay the same for all newly created sessions).</item>
        /// <item>use a new randomized number (which will stay the same for all newly created sessions).</item>
        /// </list>
        /// </summary>
        void Reset();
    }
}