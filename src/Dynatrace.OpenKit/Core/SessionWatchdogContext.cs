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

using System;
using System.Collections.Generic;
using System.Threading;
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Core.Util;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Core
{
    public class SessionWatchdogContext : ISessionWatchdogContext
    {
        /// <summary>
        /// the default sleep time if no session is to be split / closed.
        /// </summary>
        /// <returns></returns>
        internal static readonly int DefaultSleepTimeInMillis = (int) TimeSpan.FromSeconds(5).TotalMilliseconds;

        /// <summary>
        /// Indicator whether shutdown was requested or not.
        /// </summary>
        private long isShutdown;

        /// <summary>
        /// provider of the current time and for suspending the thread for a certain amount of time.
        /// </summary>
        private readonly ITimingProvider timingProvider;
        /// <summary>
        /// instance for suspending the current thread for a given amount of time
        /// </summary>
        private readonly IInterruptibleThreadSuspender threadSuspender;

        /// <summary>
        /// holds all sessions which are to be closed after a certain grace period.
        /// </summary>
        private readonly SynchronizedQueue<ISessionInternals> sessionsToClose =
            new SynchronizedQueue<ISessionInternals>();

        public SessionWatchdogContext(ITimingProvider timingProvider, IInterruptibleThreadSuspender threadSuspender)
        {
            this.timingProvider = timingProvider;
            this.threadSuspender = threadSuspender;
        }

        private ISessionWatchdogContext ThisContext => this;

        public void Execute()
        {
            var sleepTime = CloseExpiredSessions();

            if (!threadSuspender.Sleep(sleepTime))
            {
                ThisContext.RequestShutdown();
            }
        }

        private int CloseExpiredSessions()
        {
            var sleepTime = DefaultSleepTimeInMillis;
            var closableSessions = new List<ISessionInternals>();
            var allSessions = sessionsToClose.ToList();
            foreach (var session in allSessions)
            {
                var now = timingProvider.ProvideTimestampInMilliseconds();
                var gracePeriodEndTime = session.SplitByEventsGracePeriodEndTimeInMillis;
                var isGracePeriodExpired = gracePeriodEndTime <= now;
                if (isGracePeriodExpired)
                {
                    closableSessions.Add(session);
                    continue;
                }

                var sleepTimeToGracePeriodEnd = (int) (gracePeriodEndTime - now);
                sleepTime = Math.Min(sleepTime, sleepTimeToGracePeriodEnd);
            }

            foreach (var session in closableSessions)
            {
                sessionsToClose.Remove(session);
                session.End();
            }

            return sleepTime;
        }

        void ISessionWatchdogContext.RequestShutdown()
        {
            Interlocked.CompareExchange(ref isShutdown, 1, 0);
            threadSuspender.WakeUp();
        }

        public bool IsShutdownRequested => Interlocked.Read(ref isShutdown) > 0;

        void ISessionWatchdogContext.CloseOrEnqueueForClosing(ISessionInternals session, long closeGracePeriodInMillis)
        {
            if (session.TryEnd())
            {
                return;
            }

            var closeTime = timingProvider.ProvideTimestampInMilliseconds() + closeGracePeriodInMillis;
            session.SplitByEventsGracePeriodEndTimeInMillis = closeTime;
            sessionsToClose.Put(session);
        }

        void ISessionWatchdogContext.DequeueFromClosing(ISessionInternals session)
        {
            sessionsToClose.Remove(session);
        }

        internal SynchronizedQueue<ISessionInternals> GetSessionsToClose()
        {
            return sessionsToClose;
        }
    }
}