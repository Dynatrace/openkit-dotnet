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
    /// <summary>
    /// This interface represents a <see cref="BaseAction"/> which is internally used.
    ///
    /// <para>
    /// The main purpose of this interface is to make components which require a <see cref="BaseAction"/> to be more
    /// easily testable.
    /// </para>
    /// </summary>
    internal interface IActionInternals : IAction, IOpenKitComposite
    {
        /// <summary>
        /// Unique identifier of this <see cref="IActionInternals"/>.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Name of this <see cref="IActionInternals"/>.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Unique identifier of the parent <see cref="IActionInternals"/>.
        /// </summary>
        int ParentId { get; }

        /// <summary>
        /// Returns the time when this <see cref="IActionInternals"/> was started.
        /// </summary>
        long StartTime { get; }

        /// <summary>
        /// Returns the time when this <see cref="IActionInternals"/> was ended or <code>-1</code> if the action was not ended yet.
        /// </summary>
        long EndTime { get; }

        /// <summary>
        /// Returns the start sequence number of this <see cref="IActionInternals"/>.
        /// </summary>
        int StartSequenceNo { get; }

        /// <summary>
        /// Returns the end sequence number of this <see cref="IActionInternals"/>.
        /// </summary>
        int EndSequenceNo { get; }

        /// <summary>
        /// Indicates if this action was left.
        /// </summary>
        bool IsActionLeft { get; }

        /// <summary>
        /// Returns the parent <see cref="IAction"/> which might be <code>null</code> in case the parent is not an
        /// implementor of the <see cref="IAction"/> interface.
        /// </summary>
        IAction ParentAction { get; }
    }
}