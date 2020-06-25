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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Core.Util;

namespace Dynatrace.OpenKit.Core
{
    public class SessionWatchdog : ISessionWatchdog
    {
        private const int ShutdownTimeout = 2 * 1000; // 2 seconds

        private readonly ILogger logger;

        private ThreadSurrogate sessionWatchdogThread;

        /// <summary>
        /// context holding sessions to be closed after a grace period and sessions to be split after idle/max timeout
        /// </summary>
        private readonly ISessionWatchdogContext context;

        internal SessionWatchdog(ILogger logger, ISessionWatchdogContext context)
        {
            this.logger = logger;
            this.context = context;
        }

        public void Initialize()
        {
            if(logger.IsDebugEnabled)
            {
                logger.Debug($"{GetType().Name} Initialize() - thread started");
            }

            lock (this)
            {
                sessionWatchdogThread = ThreadSurrogate.Create(GetType().Name).Start(WatchdogThreadStart);
            }
        }

        private void WatchdogThreadStart()
        {
            while (!context.IsShutdownRequested)
            {
                context.Execute();
            }
        }

        public void Shutdown()
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{GetType().Name} Shutdown() - thread request shutdown");
            }

            context.RequestShutdown();

            lock (this)
            {
                if (sessionWatchdogThread == null)
                {
                    return;
                }

                sessionWatchdogThread.Join(ShutdownTimeout);
                sessionWatchdogThread = null;
            }

            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{GetType().Name} Shutdown() - thread stopped");
            }
        }

        void ISessionWatchdog.CloseOrEnqueueForClosing(ISessionInternals session, int closeGracePeriodInMillis)
        {
            context.CloseOrEnqueueForClosing(session, closeGracePeriodInMillis);
        }

        void ISessionWatchdog.DequeueFromClosing(ISessionInternals session)
        {
            context.DequeueFromClosing(session);
        }

        void ISessionWatchdog.AddToSplitByTimeout(ISessionProxy sessionProxy)
        {
            context.AddToSplitByTimeout(sessionProxy);
        }

        void ISessionWatchdog.RemoveFromSplitByTimeout(ISessionProxy sessionProxy)
        {
            context.RemoveFromSplitByTimeout(sessionProxy);
        }
    }
}