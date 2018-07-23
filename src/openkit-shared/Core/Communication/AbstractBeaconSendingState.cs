//
// Copyright 2018 Dynatrace LLC
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

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// Base class for all beacon sending states
    /// </summary>
    internal abstract class AbstractBeaconSendingState
    {
        internal AbstractBeaconSendingState(bool isTerminalState)
        {
            IsTerminalState = isTerminalState;
        }

        /// <summary>
        /// Get <code>true</code> if this state is a terminal state, <code>false</code> otherwise
        /// </summary>
        public bool IsTerminalState { get; }

        /// <summary>
        /// Get an instance of the <code>AbstractBeaconSendingState</code> to which a transition is made upon shutdown request.
        /// </summary>
        internal abstract AbstractBeaconSendingState ShutdownState { get; }

        /// <summary>
        /// Execute the current state
        /// 
        /// In case shutdown was requested, a state transition is performed by this method to the <code>ShutdownState</code>
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IBeaconSendingContext context)
        {
#if !(NETCOREAPP1_0 || NETCOREAPP1_1 || WINDOWS_UWP)
            try
            {
#endif
                DoExecute(context);
                if (context.IsShutdownRequested)
                {
                    // call on interruped
                    OnInterrupted(context);
                    // request shutdown
                    context.RequestShutdown();
                }
#if !(NETCOREAPP1_0 || NETCOREAPP1_1 || WINDOWS_UWP)
        }
            catch (System.Threading.ThreadInterruptedException)
            {
                // call on interruped
                OnInterrupted(context);
                // request shutdown
                context.RequestShutdown();
            }
#endif

            if (context.IsShutdownRequested)
            {
                context.NextState = ShutdownState;
            }
        }

        /// <summary>
        /// Performs cleanup on interrupt if necessary
        /// </summary>
        internal virtual void OnInterrupted(IBeaconSendingContext context)
        {
            // default -> do nothing
        }

        /// <summary>
        /// Executes the current state
        /// </summary>
        /// <param name="context">The state's context</param>
        protected abstract void DoExecute(IBeaconSendingContext context);
    }
}
