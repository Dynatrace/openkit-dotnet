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

using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core.Objects
{

    /// <summary>
    /// SynchronizedQueue is an implementation of a data structure that fulfills the following requirements:
    /// - has to be thread-safe for access from multiple threads
    /// - should be non-blocking for performance reasons
    /// - random-delete has to be possible
    /// - first-in, first-out
    /// - shallow copies should be possible
    /// - should be easy to clear
    ///
    /// It's for sure not the best-performing implementation and it could make sense to introduce upper bounds, but it works well enough.
    /// </summary>
    public class SynchronizedQueue<T>
    {

        // use a linked list as basic data structure
        private readonly LinkedList<T> list;

        public SynchronizedQueue()
        {
            this.list = new LinkedList<T>();
        }

        // put an item into the queue (at the end)
        public bool Put(T entry)
        {
            lock (list)
            {
                return list.AddLast(entry) != null;
            }
        }

        // get an item from the queue i.e. removes it (from the beginning)
        public T Get()
        {
            lock (list)
            {
                var first = list.First;

                if (first == null)
                    return default;

                list.RemoveFirst();
                return first.Value;
            }
        }

        // remove specific item from the queue
        public bool Remove(T entry)
        {
            lock (list)
            {
                return list.Remove(entry);
            }
        }

        // clear queue
        public void Clear()
        {
            lock (list)
            {
                list.Clear();
            }
        }

        // check if queue is empty
        public bool IsEmpty()
        {
            lock (list)
            {
                return list.Count == 0;
            }
        }

        // return shallow-copy of all the items in the queue
        public List<T> ToList()
        {
            lock (list)
            {
                return new List<T>(list);
            }
        }

        public int Count
        {
            get
            {
                lock (list)
                {
                    return list.Count;
                }
            }
        }
    }

}
