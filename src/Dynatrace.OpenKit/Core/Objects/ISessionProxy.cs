//
// Copyright 2018-2019 Dynatrace LLC
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
using Dynatrace.OpenKit.Core.Configuration;

namespace Dynatrace.OpenKit.Core.Objects
{
    /// <summary>
    /// Implements a surrogate for a <see cref="ISession"/> to perform transparent session splitting after a
    /// configured number of top level events or after the expiration of session duration or idle timeout.
    /// </summary>
    public interface ISessionProxy : ISession
    {
        /// <summary>
        /// Indicates whether this session proxy was finished or is still open.
        /// </summary>
        bool IsFinished { get; }

        /// <summary>
        /// Callback method which is to be invoked when the server configuration on a session is updated.
        /// </summary>
        /// <param name="serverConfig">the updated server configuration</param>
        void OnServerConfigurationUpdate(IServerConfiguration serverConfig);

        /// <summary>
        /// Will end the current active session and start a new one but only if the following conditions are met:
        ///
        /// <list type="bullet">
        /// <item>this session proxy is not <see cref="IsFinished"/></item>
        /// <item>
        ///     session splitting by idle timeout is enabled and the current session was idle for longer than the
        ///     configured timeout.
        /// </item>
        /// <item>
        ///     session splitting by maximum session duration is enabled and the session was open for longer than the
        ///     maximum configured session duration.
        /// </item>
        /// </list>
        /// </summary>
        /// <returns>
        ///     the time when the session might be split next. This can either be the time when the maximum duration
        ///     is reached or the time when the idle timeout expires. In case the session proxy is finished, <code>-1</code>
        ///     is returned.
        /// </returns>
        long SplitSessionByTime();
    }
}