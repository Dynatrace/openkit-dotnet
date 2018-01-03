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
    /// The BeaconSender is responsible for asynchronously sending the Beacons to the provided endpoint.
    /// </summary>
    /// <remarks>
    /// The <code>BeaconSender</code> manages the thread running OpenKit communication in the background.
    /// </remarks>
    public class BeaconSender
    {
        private const int SHUTDOWN_TIMEOUT = 10 * 1000;                  // wait max 10s (in ms) for beacon sender to complete data sending during shutdown

        // beacon sender thread
        private Thread beaconSenderThread;
        // sending state context
        private readonly IBeaconSendingContext context;

        public BeaconSender(AbstractConfiguration configuration, IHTTPClientProvider clientProvider, ITimingProvider provider)
            : this(new BeaconSendingContext(configuration, clientProvider, provider))
        {
        }

        internal BeaconSender(IBeaconSendingContext context)
        {
            this.context = context;
        }

        public bool IsInitialized => context.IsInitialized;

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

        public bool WaitForInitCompletion()
        {
            return context.WaitForInit();
        }

        public bool WaitForInitCompletion(int timeoutMillis)
        {
            return context.WaitForInit(timeoutMillis);
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
            context.StartSession(session);
        }

        // when finishing a Session, remove it from open Sessions and put it into finished Sessions
        public void FinishSession(Session session)
        {
            context.FinishSession(session);
        }
    }
}
