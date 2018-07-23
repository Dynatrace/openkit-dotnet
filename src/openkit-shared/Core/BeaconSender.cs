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

using Dynatrace.OpenKit.Core.Communication;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;
using Dynatrace.OpenKit.API;
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

        private readonly ILogger logger;

        // beacon sender thread
#if WINDOWS_UWP
        private System.Threading.Tasks.Task beaconSenderThread;
#else
        private Thread beaconSenderThread;
#endif
        // sending state context
        private readonly IBeaconSendingContext context;

        public BeaconSender(ILogger logger, OpenKitConfiguration configuration, IHTTPClientProvider clientProvider, ITimingProvider provider)
            : this(logger, new BeaconSendingContext(logger, configuration, clientProvider, provider))
        {
        }

        internal BeaconSender(ILogger logger, IBeaconSendingContext context)
        {
            this.logger = logger;
            this.context = context;
        }

        public bool IsInitialized => context.IsInitialized;

        public bool Initialize()
        {
            if(logger.IsDebugEnabled)
            {
                logger.Debug(GetType().Name + " thread started");
            }

            // create sending thread
#if WINDOWS_UWP
            beaconSenderThread = System.Threading.Tasks.Task.Factory.StartNew(Loop);
#else
            beaconSenderThread = new Thread(new ThreadStart(Loop));
            beaconSenderThread.IsBackground = true;
            // start thread
            beaconSenderThread.Start();
#endif

            var success = context.WaitForInit();
            if (!success)
            {
#if WINDOWS_UWP
                beaconSenderThread.Wait();
#else
                beaconSenderThread.Join();
#endif
            }

            return success;
        }

        private void Loop() {
            while (!context.IsInTerminalState) {
                context.ExecuteCurrentState();
            }
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
            if (logger.IsDebugEnabled)
            {
                logger.Debug(GetType().Name + " thread request shutdown");
            }
            context.RequestShutdown();

            if (beaconSenderThread != null)
            {
#if !(NETCOREAPP1_0 || NETCOREAPP1_1 || WINDOWS_UWP)
                beaconSenderThread.Interrupt();                     // not available in .NET Core 1.0 & .NET Core 1.1
                                                                    // might cause up to 1s delay at shutdown
#endif
#if WINDOWS_UWP
                beaconSenderThread.Wait(SHUTDOWN_TIMEOUT);
#else
                beaconSenderThread.Join(SHUTDOWN_TIMEOUT);
#endif
                if (logger.IsDebugEnabled)
                {
                    logger.Debug(GetType().Name + " thread stopped");
                }
            }
        }

        // when starting a new Session, put it into open Sessions
        public void StartSession(Session session)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(GetType().Name + " StartSession");
            }
            context.StartSession(session);
        }

        // when finishing a Session, remove it from open Sessions and put it into finished Sessions
        public void FinishSession(Session session)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(GetType().Name + " FinishSession");
            }
            context.FinishSession(session);
        }
    }
}
