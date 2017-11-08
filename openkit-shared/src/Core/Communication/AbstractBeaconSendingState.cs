using System.Threading;

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// Base class for all beacon sending states
    /// </summary>
    internal abstract class AbstractBeaconSendingState
    {
        protected AbstractBeaconSendingState(bool isTerminalState)
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
        protected abstract AbstractBeaconSendingState ShutdownState { get; }

        /// <summary>
        /// Execute the current state
        /// 
        /// In case shutdown was requested, a state transition is performed by this method to the <see cref=""/>
        /// </summary>
        /// <param name="context"></param>
        public void Execute(BeaconSendingContext context)
        {
#if !NETCOREAPP1_0
            try
            {
#endif
                DoExecute(context);
#if !NETCOREAPP1_0
            }
            catch (ThreadInterruptedException)
            {
                // call on interruped
                OnInterrupted(context);
                // request shutdown
                context.RequestShutdown();
            }
#endif

            if (context.IsShutdownRequested)
            {
                context.CurrentState = ShutdownState;
            }
        }

        /// <summary>
        /// Performs cleanup on interrupt if necessary
        /// </summary>
        protected virtual void OnInterrupted(BeaconSendingContext context)
        {
            // default -> do nothing
        }

        /// <summary>
        /// Executes the current state
        /// </summary>
        /// <param name="context">The state's context</param>
        protected abstract void DoExecute(BeaconSendingContext context);
    }
}
