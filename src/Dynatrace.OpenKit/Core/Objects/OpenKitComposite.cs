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

using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core.Objects
{
    /// <summary>
    /// A composite base class for OpenKit objects.
    ///
    /// <para>
    ///     It features a container to store child objects.
    ///     The container is not thread thus, synchronization must be taken care of by the implementing class.
    /// </para>
    /// </summary>
    public abstract class OpenKitComposite : IOpenKitObject, IOpenKitComposite
    {
        /// <summary>
        /// action ID default value
        /// </summary>
        private const int DefaultActionId = 0;

        /// <summary>
        /// Container for storing children of this composite.
        /// </summary>
        private readonly IList<IOpenKitObject> children = new List<IOpenKitObject>();

        void IOpenKitComposite.StoreChildInList(IOpenKitObject childObject)
        {
            children.Add(childObject);
        }

        bool IOpenKitComposite.RemoveChildFromList(IOpenKitObject childObject)
        {
            return children.Remove(childObject);
        }

        IList<IOpenKitObject> IOpenKitComposite.GetCopyOfChildObjects()
        {
            return new List<IOpenKitObject>(children);
        }

        int IOpenKitComposite.GetChildCount()
        {
            return children.Count;
        }

        /// <summary>
        /// Abstract method to notify the composite about closing/ending a child object.
        ///
        /// <para>
        ///     The implementing class is fully responsible to handle the implementation.
        ///     In most cases removing the child from the container <see cref="IOpenKitComposite.RemoveChildFromList"/>
        ///     is sufficient.
        /// </para>
        /// </summary>
        /// <param name="childObject"></param>
        private protected abstract void OnChildClosed(IOpenKitObject childObject);

        void IOpenKitComposite.OnChildClosed(IOpenKitObject childObject)
        {
            OnChildClosed(childObject);
        }


        public virtual int ActionId => DefaultActionId;


        public abstract void Dispose();
    }
}