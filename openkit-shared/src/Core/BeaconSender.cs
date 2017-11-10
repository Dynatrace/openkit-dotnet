/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.Core.Communication;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;
using System.Threading;

namespace Dynatrace.OpenKit.Core
{
    /// <summary>
    ///  The BeaconSender is responsible for asynchronously sending the Beacons to the provided endpoint.
    /// </summary>
    public class BeaconSender
    {
        private static readonly int SHUTDOWN_TIMEOUT = 10 * 1000;                  // wait max 10s (in ms) for beacon sender to complete data sending during shutdown

        // beacon sender thread
        private Thread beaconSenderThread;
        // sending state context
        private readonly BeaconSendingContext context;

        public BeaconSender(AbstractConfiguration configuration, IHTTPClientProvider clientProvider, ITimingProvider provider)
        {
            context = new BeaconSendingContext(configuration, clientProvider, provider);
        }

        public bool Initialize()
        {
            // create sending thread
            beaconSenderThread = new Thread(new ThreadStart(() =>
            {
                while (!context.IsInTerminalState)
                {
                    context.ExecuteCurrentState();
                }
            }));
            // start thread
            beaconSenderThread.Start();

            var success = context.WaitForInit();
            if (!success)
            {
                beaconSenderThread.Join();
            }

            return success;
        }

        public void Shutdown()
        {
            context.RequestShutdown();

            if (beaconSenderThread != null)
            {
#if !NETCOREAPP1_0
                beaconSenderThread.Interrupt();                     // not available in .NET Core 1.0, might cause up to 1s delay at shutdown
#endif
                beaconSenderThread.Join(SHUTDOWN_TIMEOUT);
            }
        }

        // when starting a new Session, put it into open Sessions
        public void StartSession(Session session)
        {
            if (context.IsCaptureOn)
            {
                context.StartSession(session);
            }
        }

        // when finishing a Session, remove it from open Sessions and put it into finished Sessions
        public void FinishSession(Session session)
        {
            context.FinishSession(session);
        }
    }

}
