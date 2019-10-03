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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Communication;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Objects;
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
    public class BeaconSender : IBeaconSender
    {
        private const int SHUTDOWN_TIMEOUT = 10 * 1000;                  // wait max 10s (in ms) for beacon sender to complete data sending during shutdown

        private readonly ILogger logger;

        // beacon sender thread
#if WINDOWS_UWP || NETSTANDARD1_1
        private System.Threading.Tasks.Task beaconSenderThread;
#else
        private Thread beaconSenderThread;
#endif
        // sending state context
        private readonly IBeaconSendingContext context;

        internal BeaconSender(
            ILogger logger,
            IHttpClientConfiguration httpClientConfiguration,
            IHttpClientProvider clientProvider,
            ITimingProvider timingProvider)
        {
            this.logger = logger;
            context = new BeaconSendingContext(logger, httpClientConfiguration, clientProvider, timingProvider);
        }

        public bool IsInitialized => context.IsInitialized;

        public void Initialize()
        {
            if(logger.IsDebugEnabled)
            {
                logger.Debug($"{GetType().Name} thread started");
            }

            // create sending thread
#if WINDOWS_UWP || NETSTANDARD1_1
            beaconSenderThread = System.Threading.Tasks.Task.Factory.StartNew(SenderThread);
#else
            beaconSenderThread = new Thread(SenderThread)
            {
                IsBackground = true,
                Name = GetType().Name
            };
            // start thread
            beaconSenderThread.Start();
#endif
        }

        private void SenderThread() {
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
                logger.Debug($"{GetType().Name} thread request shutdown");
            }
            context.RequestShutdown();

            if (beaconSenderThread != null)
            {
#if !(NETCOREAPP1_0 || NETCOREAPP1_1 || WINDOWS_UWP || NETSTANDARD1_1)
                beaconSenderThread.Interrupt();                     // not available in .NET Core 1.0 & .NET Core 1.1
                                                                    // might cause up to 1s delay at shutdown
#endif
#if WINDOWS_UWP || NETSTANDARD1_1
                beaconSenderThread.Wait(SHUTDOWN_TIMEOUT);
#else
                beaconSenderThread.Join(SHUTDOWN_TIMEOUT);
#endif
                if (logger.IsDebugEnabled)
                {
                    logger.Debug($"{GetType().Name} thread stopped");
                }
            }
        }

        /// <summary>
        /// Returns the current server ID to be used for creating new sessions.
        /// </summary>
        public int CurrentServerId => context.CurrentServerId;

        // when starting a new Session, put it into open Sessions
        void IBeaconSender.AddSession(ISessionInternals session)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{GetType().Name} AddSession()");
            }
            context.AddSession(session);
        }
    }
}
