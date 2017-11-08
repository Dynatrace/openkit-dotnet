namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// Terminal state for beacon sending.
    /// </summary>
    internal class BeaconSendingTerminalState : AbstractBeaconSendingState
    {
        public BeaconSendingTerminalState() : base(true) { }

        protected override AbstractBeaconSendingState ShutdownState => this;

        protected override void DoExecute(BeaconSendingContext context)
        {
            context.RequestShutdown();
        }
    }
}
