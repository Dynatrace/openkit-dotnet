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

using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Core.Objects
{
    internal interface ISessionState
    {
        /// <summary>
        /// Indicates if the <see cref="ISession"/> is still new.
        ///
        /// <para>
        ///     A <see cref="ISession"/> is considered as new if it has not yet received any configuration updates from
        ///     the server and if it also is not finished.
        /// </para>
        /// </summary>
        bool IsNew { get; }

        /// <summary>
        /// Indicates whether the <see cref="ISession"/> is configured or not.
        ///
        /// <para>
        ///     A <see cref="ISession"/> is considered as configured if it received configuration updates from the
        ///     server.
        /// </para>
        /// </summary>
        bool IsConfigured { get; }

        /// <summary>
        /// Indicates if the <see cref="ISession"/> is finished and was configured.
        /// </summary>
        bool IsConfiguredAndFinished { get; }

        /// <summary>
        /// Indicates if the <see cref="ISession"/> is configured and not yet finished.
        /// </summary>
        bool IsConfiguredAndOpen { get; }

        /// <summary>
        /// Indicates if the <see cref="ISession"/> is finished.
        ///
        /// <para>
        ///     A session is considered as finished after the <see cref="ISession.End()"/> method was called.
        /// </para>
        /// </summary>
        bool IsFinished { get; }
    }
}