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

        /// <summary>
        /// holds all session proxies which are to be split after expiration of either session duration or idle timeout.
        /// </summary>
        private readonly SynchronizedQueue<ISessionProxy> sessionsToSplitByTimeout =
            new SynchronizedQueue<ISessionProxy>();

        public SessionWatchdogContext(ITimingProvider timingProvider, IInterruptibleThreadSuspender threadSuspender)
        {
            this.timingProvider = timingProvider;
            this.threadSuspender = threadSuspender;
        }

        private ISessionWatchdogContext ThisContext => this;

        public void Execute()
        {
            var durationToNextCloseInMillis = CloseExpiredSessions();
            var durationToNextSplitInMillis = SplitTimedOutSessions();

            var sleepTime = Math.Min(durationToNextCloseInMillis, durationToNextSplitInMillis);
            if (!threadSuspender.Sleep(sleepTime))
            {
                ThisContext.RequestShutdown();
            }
        }

        private int SplitTimedOutSessions()
        {
            var sleepTimeInMillis = DefaultSleepTimeInMillis;
            var sessionProxiesToRemove = new List<ISessionProxy>();
            var allSessionProxies = sessionsToSplitByTimeout.ToList();
            foreach (var sessionProxy in allSessionProxies)
            {
                var nextSessionSplitInMillis = sessionProxy.SplitSessionByTime();
                if (nextSessionSplitInMillis < 0)
                {
                    sessionProxiesToRemove.Add(sessionProxy);
                    continue;
                }

                var nowInMillis = timingProvider.ProvideTimestampInMilliseconds();
                var durationToNextSplit = (int) (nextSessionSplitInMillis - nowInMillis);
                if (durationToNextSplit < 0)
                {
                    continue;
                }

                sleepTimeInMillis = Math.Min(sleepTimeInMillis, durationToNextSplit);
            }

            foreach (var sessionProxy in sessionProxiesToRemove)
            {
                sessionsToSplitByTimeout.Remove(sessionProxy);
            }

            return sleepTimeInMillis;
        }

        private int CloseExpiredSessions()
        {
            var sleepTimeInMillis = DefaultSleepTimeInMillis;
            var closableSessions = new List<ISessionInternals>();
            var allSessions = sessionsToClose.ToList();
            foreach (var session in allSessions)
            {
                var nowInMillis = timingProvider.ProvideTimestampInMilliseconds();
                var gracePeriodEndTimeInMillis = session.SplitByEventsGracePeriodEndTimeInMillis;
                var isGracePeriodExpired = gracePeriodEndTimeInMillis <= nowInMillis;
                if (isGracePeriodExpired)
                {
                    closableSessions.Add(session);
                    continue;
                }

                var sleepTimeToGracePeriodEndInMillis = (int) (gracePeriodEndTimeInMillis - nowInMillis);
                sleepTimeInMillis = Math.Min(sleepTimeInMillis, sleepTimeToGracePeriodEndInMillis);
            }

            foreach (var session in closableSessions)
            {
                sessionsToClose.Remove(session);
                session.End();
            }

            return sleepTimeInMillis;
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

        void ISessionWatchdogContext.AddToSplitByTimeout(ISessionProxy sessionProxy)
        {
            if (sessionProxy.IsFinished)
            {
                return;
            }

            sessionsToSplitByTimeout.Put(sessionProxy);
        }

        void ISessionWatchdogContext.RemoveFromSplitByTimeout(ISessionProxy sessionProxy)
        {
            sessionsToSplitByTimeout.Remove(sessionProxy);
        }

        internal SynchronizedQueue<ISessionProxy> SessionsToSplitByTimeout => sessionsToSplitByTimeout;
    }
}