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

using System;

namespace Dynatrace.OpenKit.Core.Util
{
    public class ThreadSurrogate
    {
#if WINDOWS_UWP || NETSTANDARD1_1
        private System.Threading.Tasks.Task thread;
#else
        private System.Threading.Thread thread;
#endif

        private ThreadSurrogate(string threadName)
        {
            ThreadName = threadName;
        }


        public static ThreadSurrogate Create(string threadName)
        {
            return new ThreadSurrogate(threadName);
        }

#if WINDOWS_UWP || NETSTANDARD1_1
        public ThreadSurrogate Start(System.Action threadStart)
#else
        public ThreadSurrogate Start(System.Threading.ThreadStart threadStart)
#endif
        {
            lock (this)
            {
                if (thread != null)
                {
                    throw new InvalidOperationException($"Thread {ThreadName} already started.");
                }
#if WINDOWS_UWP || NETSTANDARD1_1
                thread = System.Threading.Tasks.Task.Factory.StartNew(threadStart);
#else
                thread = new System.Threading.Thread(threadStart)
                {
                    IsBackground = true,
                    Name = ThreadName
                };
                thread.Start();
#endif
            }

            return this;
        }

        public void Join(int waitTimeInMillis)
        {
            lock (this)
            {
                if (thread == null)
                {
                    return;
                }
            }

#if !(NETCOREAPP1_0 || NETCOREAPP1_1 || WINDOWS_UWP || NETSTANDARD1_1)
            thread.Interrupt(); // not available in .NET Core 1.0 & .NET Core 1.1
#endif
#if WINDOWS_UWP || NETSTANDARD1_1
            thread.Wait(waitTimeInMillis);
#else
            thread.Join(waitTimeInMillis);
#endif
        }

        public string ThreadName { get; }
    }
}