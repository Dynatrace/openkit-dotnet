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

        public BeaconSender(Configuration.OpenKitConfiguration configuration, IHTTPClientProvider clientProvider, ITimingProvider provider)
            : this(new BeaconSendingContext(configuration, clientProvider, provider))
        {
        }

        internal BeaconSender(IBeaconSendingContext context)
        {
            this.context = context;
        }

        public bool IsInitialized => context.IsInitialized;

        public void Initialize()
        {
            // create sending thread
            beaconSenderThread = new Thread(new ThreadStart(() =>
            {
                while (!context.IsInTerminalState)
                {
                    context.ExecuteCurrentState();
                }
            }))
            {
                IsBackground = true,
                Name = this.GetType().Name
            };
            // start thread
            beaconSenderThread.Start();
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
