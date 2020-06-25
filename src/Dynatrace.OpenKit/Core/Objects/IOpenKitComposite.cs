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

using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core.Objects
{
    /// <summary>
    /// This interface represents a <see cref="OpenKitComposite"/> which is internally used.
    ///
    /// <para>
    /// The main purpose of this interface is to make components which require a <see cref="OpenKitComposite"/> to be
    /// more easily testable.
    /// </para>
    /// </summary>
    internal interface IOpenKitComposite
    {
        /// <summary>
        /// Adds a child object to the list of this <see cref="IOpenKitComposite"/>'s children.
        /// </summary>
        ///
        /// <param name="childObject">the <see cref="IOpenKitObject">child object</see> to add.</param>
        void StoreChildInList(IOpenKitObject childObject);

        /// <summary>
        /// Remove a child object from the list of this <see cref="IOpenKitComposite"/>'s children.
        /// </summary>
        ///
        /// <param name="childObject">the <see cref="IOpenKitComposite">child object</see> to remove.</param>
        ///
        /// <returns>
        ///     <code>true</code> if the given <code>childObject</code> was successfully removed,
        ///     <code>false</code> otherwise.
        /// </returns>
        bool RemoveChildFromList(IOpenKitObject childObject);

        /// <summary>
        /// Returns a shallow copy of the <see cref="IOpenKitObject"/> child objects
        /// </summary>
        /// <returns>shallow copy of the child objects</returns>
        IList<IOpenKitObject> GetCopyOfChildObjects();

        /// <summary>
        /// Returns the current number of children held by this composite.
        /// </summary>
        int GetChildCount();

        /// <summary>
        /// Abstract method to notify the composite about closing/ending a child object.
        ///
        /// <para>
        ///     The implementing class is fully responsible to handle the implementation.
        ///     In most cases removing the child from the container <see cref="RemoveChildFromList"/> is sufficient.
        /// </para>
        /// </summary>
        /// <param name="childObject"></param>
         void OnChildClosed(IOpenKitObject childObject);

        /// <summary>
        /// Returns the action ID of this composite or <code>0</code> if the composite is not an action.
        ///
        /// <para>
        ///     The default implementation returns <code>0</code>.
        ///     Action related composites need to override this method and return the appropriate value.
        /// </para>
        /// </summary>
        int ActionId { get; }
    }
}