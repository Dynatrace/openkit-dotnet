/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core
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
        private LinkedList<T> list;

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
                    return default(T);

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

    }

}
