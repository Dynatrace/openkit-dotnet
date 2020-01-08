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
using Dynatrace.OpenKit.Core.Util;

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
        private ThreadSurrogate beaconSenderThread;

        // sending state context
        private readonly IBeaconSendingContext context;

        internal BeaconSender(
            ILogger logger,
            IHttpClientConfiguration httpClientConfiguration,
            IHttpClientProvider clientProvider,
            ITimingProvider timingProvider,
            IInterruptibleThreadSuspender threadSuspender)
        {
            this.logger = logger;
            context = new BeaconSendingContext(logger, httpClientConfiguration, clientProvider, timingProvider,
                threadSuspender);
        }

        public bool IsInitialized => context.IsInitialized;

        public void Initialize()
        {
            if(logger.IsDebugEnabled)
            {
                logger.Debug($"{GetType().Name} thread started");
            }

            // create sending thread
            beaconSenderThread = ThreadSurrogate.Create(GetType().Name).Start(SenderThread);
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
                beaconSenderThread.Join(SHUTDOWN_TIMEOUT);
                if (logger.IsDebugEnabled)
                {
                    logger.Debug($"{GetType().Name} thread stopped");
                }
            }
        }

        /// <summary>
        /// Returns the last known server configuration.
        /// </summary>
        public IServerConfiguration LastServerConfiguration => context.LastServerConfiguration;

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
