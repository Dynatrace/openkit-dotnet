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

using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Core.Util;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core
{
    public class SessionWatchdogContextTest
    {
        private ITimingProvider mockTimingProvider;
        private IInterruptibleThreadSuspender mockThreadSuspender;
        private ISessionInternals mockSession;
        private ISessionProxy mockSessionProxy;

        [SetUp]
        public void SetUp()
        {
            mockTimingProvider = Substitute.For<ITimingProvider>();
            mockSession = Substitute.For<ISessionInternals>();

            mockThreadSuspender = Substitute.For<IInterruptibleThreadSuspender>();
            mockThreadSuspender.Sleep(Arg.Any<int>()).Returns(true);

            mockSessionProxy = Substitute.For<ISessionProxy>();
        }

        [Test]
        public void OnDefaultShutdownIsFalse()
        {
            // given
            var target = CreateContext();

            // then
            Assert.That(target.IsShutdownRequested, Is.False);
        }

        [Test]
        public void OnDefaultSessionsToCloseIsEmpty()
        {
            // given
            var target = CreateContext();

            // then
            Assert.That(target.GetSessionsToClose().Count, Is.EqualTo(0));
        }

        [Test]
        public void OnDefaultSessionsToSplitByTimeoutIsEmpty()
        {
            // given
            var target = CreateContext();

            // then
            Assert.That(target.SessionsToSplitByTimeout.Count, Is.EqualTo(0));
        }

        [Test]
        public void CloseOrEnqueueForClosingDoesNotAddSessionIfItCanBeClosed()
        {
            // given
            mockSession.TryEnd().Returns(true);
            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;

            // when
            targetExplicit.CloseOrEnqueueForClosing(mockSession, 0);

            // then
            Assert.That(target.GetSessionsToClose().Count, Is.EqualTo(0));
            mockSession.Received(1).TryEnd();
        }

        [Test]
        public void CloseOrEnqueueForClosingAddsSessionIfSessionCannotBeClosed()
        {
            // given
            mockSession.TryEnd().Returns(false);
            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;

            // when
            targetExplicit.CloseOrEnqueueForClosing(mockSession, 17);

            // then
            Assert.That(target.GetSessionsToClose().Count, Is.EqualTo(1));
        }

        [Test]
        public void CloseOrEnqueueForClosingSetsSplitByEventsGracePeriodEndTimeIfSessionCannotBeClosed()
        {
            // given
            const long timestamp = 10;
            const int gracePeriod = 5;
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(timestamp);
            mockSession.TryEnd().Returns(false);
            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;

            // when
            targetExplicit.CloseOrEnqueueForClosing(mockSession, gracePeriod);

            // then
            mockSession.Received(1).SplitByEventsGracePeriodEndTimeInMillis = (timestamp + gracePeriod);
        }

        [Test]
        public void DequeueFromClosingRemovesSession()
        {
            // given
            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;
            targetExplicit.CloseOrEnqueueForClosing(mockSession, 0);
            Assert.That(target.GetSessionsToClose().Count, Is.EqualTo(1));

            // when
            targetExplicit.DequeueFromClosing(mockSession);

            // then
            Assert.That(target.GetSessionsToClose().Count, Is.EqualTo(0));
        }

        [Test]
        public void AddToSplitByTimeOutAddsSessionProxyIfNotFinished()
        {
            // given
            mockSessionProxy.IsFinished.Returns(false);

            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;

            // when
            targetExplicit.AddToSplitByTimeout(mockSessionProxy);

            // then
            Assert.That(target.SessionsToSplitByTimeout.Count, Is.EqualTo(1));
        }

        [Test]
        public void AddToSplitByTimeOutDoesNotAddSessionProxyIfFinished()
        {
            // given
            mockSessionProxy.IsFinished.Returns(true);

            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;

            // when
            targetExplicit.AddToSplitByTimeout(mockSessionProxy);

            // then
            Assert.That(target.SessionsToSplitByTimeout.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveFromSplitByTimeoutRemovesSessionProxy()
        {
            // given
            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;
            targetExplicit.AddToSplitByTimeout(mockSessionProxy);

            Assert.That(target.SessionsToSplitByTimeout.Count, Is.EqualTo(1));

            // when
            targetExplicit.RemoveFromSplitByTimeout(mockSessionProxy);

            // then
            Assert.That(target.SessionsToSplitByTimeout.Count, Is.EqualTo(0));
        }

        [Test]
        public void ExecuteEndsSessionsWithExpiredGracePeriod()
        {
            // given
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(5L, 10L);
            mockSession.TryEnd().Returns(false);

            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;
            targetExplicit.CloseOrEnqueueForClosing(mockSession, 4);

            // when
            target.Execute();

            // then
            mockSession.Received(1).End();
            Assert.That(target.GetSessionsToClose().Count, Is.EqualTo(0));
        }

        [Test]
        public void ExecuteEndsSessionsWithGraceEndTimeSameAsCurrentTime()
        {
            // given
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(5L);
            mockSession.TryEnd().Returns(false);

            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;
            targetExplicit.CloseOrEnqueueForClosing(mockSession, 0);

            // when
            target.Execute();

            // then
            mockSession.Received(1).End();
            Assert.That(target.GetSessionsToClose().Count, Is.EqualTo(0));
        }

        [Test]
        public void ExecuteDoesNotEndSessionsWhenGracePeriodIsNotExpired()
        {
            // given
            const int gracePeriod = 1;
            const int now = 5;
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(now);
            mockSession.TryEnd().Returns(false);

            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;
            targetExplicit.CloseOrEnqueueForClosing(mockSession, gracePeriod); // close at now + gracePeriod

            // when
            target.Execute();

            // then
            mockSession.Received(0).End();
            mockSession.Received(1).SplitByEventsGracePeriodEndTimeInMillis = (now + gracePeriod);
            Assert.That(target.GetSessionsToClose().Count, Is.EqualTo(1));
        }

        [Test]
        public void ExecuteSleepsDefaultTimeIfSessionIsExpiredAndNoFurtherNonExpiredSessions()
        {
            // given
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(5L, 10L);
            mockSession.TryEnd().Returns(false);
            var target = CreateContext() as ISessionWatchdogContext;
            target.CloseOrEnqueueForClosing(mockSession, 3);

            // when
            target.Execute();

            // then
            mockThreadSuspender.Received(1).Sleep(SessionWatchdogContext.DefaultSleepTimeInMillis);
            mockSession.Received(1).End();
        }

        [Test]
        public void ExecuteSleepsMinimumTimeToNextSessionGraceEndPeriod()
        {
            // given
            mockSession.SplitByEventsGracePeriodEndTimeInMillis.Returns(4L);
            mockSession.TryEnd().Returns(false);
            var mockSession1 = Substitute.For<ISessionInternals>();

            mockSession1.SplitByEventsGracePeriodEndTimeInMillis.Returns(3L);
            mockSession1.TryEnd().Returns(false);

            var mockSession2 = Substitute.For<ISessionInternals>();
            mockSession2.SplitByEventsGracePeriodEndTimeInMillis.Returns(5L);
            mockSession2.TryEnd().Returns(false);

            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(0L);

            var target = CreateContext() as ISessionWatchdogContext;
            target.CloseOrEnqueueForClosing(mockSession, 4);
            target.CloseOrEnqueueForClosing(mockSession1, 3);
            target.CloseOrEnqueueForClosing(mockSession2, 5);

            // when
            target.Execute();

            // then
            mockThreadSuspender.Received(1).Sleep(3);
            mockSession.Received(0).End();
            mockSession1.Received(0).End();
            mockSession2.Received(0).End();
        }

        [Test]
        public void ExecuteRemovesSessionProxyIfNextSplitTimeIsNegative()
        {
            // given
            mockSessionProxy.SplitSessionByTime().Returns(-1L);

            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;
            targetExplicit.AddToSplitByTimeout(mockSessionProxy);

            // when
            target.Execute();

            // then
            mockSessionProxy.Received(1).SplitSessionByTime();
            Assert.That(target.SessionsToSplitByTimeout.Count, Is.EqualTo(0));
        }

        [Test]
        public void ExecuteDoesNotRemoveSessionProxyIfNextSplitTimeIsNegative()
        {
            // given
            mockSessionProxy.SplitSessionByTime().Returns(10L);

            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;
            targetExplicit.AddToSplitByTimeout(mockSessionProxy);

            // when
            target.Execute();

            // then
            mockSessionProxy.Received(1).SplitSessionByTime();
            Assert.That(target.SessionsToSplitByTimeout.Count, Is.EqualTo(1));
        }

        [Test]
        public void ExecuteSleepsDefaultTimeIfSessionProxySplitTimeIsNegativeAndNoFurtherSessionProxyExists()
        {
            // given
            mockSessionProxy.SplitSessionByTime().Returns(-1L);

            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;
            targetExplicit.AddToSplitByTimeout(mockSessionProxy);

            // when
            target.Execute();

            // then
            mockSessionProxy.Received(1).SplitSessionByTime();
            mockThreadSuspender.Received(1).Sleep(SessionWatchdogContext.DefaultSleepTimeInMillis);
        }

        [Test]
        public void ExecuteSleepsDefaultTimeIfSleepDurationToNextSplitIsNegativeAndNoFurtherSessionProxyExists()
        {
            // given
            mockSessionProxy.SplitSessionByTime().Returns(10L);
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(20L);

            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;
            targetExplicit.AddToSplitByTimeout(mockSessionProxy);

            // when
            target.Execute();

            // then
            mockSessionProxy.Received(1).SplitSessionByTime();
            mockThreadSuspender.Received(1).Sleep(SessionWatchdogContext.DefaultSleepTimeInMillis);
        }

        [Test]
        public void ExecuteSleepsDurationToNextSplitByTimeout()
        {
            // given
            const int nextSplitTime = 100;
            const int currentTime = 50;
            mockSessionProxy.SplitSessionByTime().Returns(nextSplitTime);
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(currentTime);

            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;
            targetExplicit.AddToSplitByTimeout(mockSessionProxy);

            // when
            target.Execute();

            // then
            mockSessionProxy.Received(1).SplitSessionByTime();
            mockThreadSuspender.Received(1).Sleep(nextSplitTime - currentTime);
        }

        [Test]
        public void ExecuteDoesNotSleepLongerThanDefaultSleepTimeForDurationToNextSplitByTime()
        {
            // given
            var nextSplitTime = SessionWatchdogContext.DefaultSleepTimeInMillis + 20;
            const long currentTime = 5;
            mockSessionProxy.SplitSessionByTime().Returns(nextSplitTime);
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(currentTime);

            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;
            targetExplicit.AddToSplitByTimeout(mockSessionProxy);

            // when
            target.Execute();

            // then
            mockSessionProxy.Received(1).SplitSessionByTime();
            mockThreadSuspender.Received(1).Sleep(SessionWatchdogContext.DefaultSleepTimeInMillis);
        }

        [Test]
        public void ExecuteSleepsMinimumTimeToNextSplitByTime()
        {
            // given
            const int nextSplitTimeProxy1 = 120;
            const int nextSplitTimeProxy2 = 100;
            const int currentTime = 50;

            mockSessionProxy.SplitSessionByTime().Returns(nextSplitTimeProxy1);

            var mockSessionProxy2 = Substitute.For<ISessionProxy>();
            mockSessionProxy2.SplitSessionByTime().Returns(nextSplitTimeProxy2);

            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(currentTime);

            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;
            targetExplicit.AddToSplitByTimeout(mockSessionProxy);
            targetExplicit.AddToSplitByTimeout(mockSessionProxy2);

            // when
            target.Execute();

            // then
            mockSessionProxy.Received(1).SplitSessionByTime();
            mockSessionProxy2.Received(1).SplitSessionByTime();
            mockThreadSuspender.Received(1).Sleep(nextSplitTimeProxy2 - currentTime);
        }

        [Test]
        public void ExecuteSleepsDefaultTimeIfNoSessionToCloseAndNoSessionProxyToSplitExists()
        {
            // given
            var target = CreateContext();

            // when
            target.Execute();

            // then
            mockThreadSuspender.Received(1).Sleep(SessionWatchdogContext.DefaultSleepTimeInMillis);
        }

        [Test]
        public void ExecuteSleepsMinimumDurationToNextSplitByTime()
        {
            // given
            const int gracePeriod = 150;
            const int nextSessionProxySplitTime = 100;
            const int currentTime = 50;
            mockSessionProxy.SplitSessionByTime().Returns(nextSessionProxySplitTime);
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(currentTime);

            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;
            targetExplicit.CloseOrEnqueueForClosing(mockSession, gracePeriod);
            targetExplicit.AddToSplitByTimeout(mockSessionProxy);

            // when
            target.Execute();

            // then
            mockSessionProxy.Received(1).SplitSessionByTime();
            mockSession.Received(0).End();
            mockThreadSuspender.Received(1).Sleep(nextSessionProxySplitTime - currentTime);
        }

        [Test]
        public void ExecuteSleepsMinimumDurationToNextGracePeriodEnd()
        {
            // given
            const int gracePeriod = 100;
            const int nextSessionProxySplitTime = 200;
            const int currentTime = 50;
            mockSessionProxy.SplitSessionByTime().Returns(nextSessionProxySplitTime);
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(currentTime);

            var target = CreateContext();
            ISessionWatchdogContext targetExplicit = target;
            targetExplicit.CloseOrEnqueueForClosing(mockSession, gracePeriod);
            targetExplicit.AddToSplitByTimeout(mockSessionProxy);

            // when
            target.Execute();

            // then
            mockSessionProxy.Received(1).SplitSessionByTime();
            mockSession.Received(0).End();
            mockThreadSuspender.Received(1).Sleep(gracePeriod);
        }

        [Test]
        public void ExecuteDoesNotSleepLongerThanDefaultSleepTimeForDurationToNextSessionClose()
        {
            {
                // given
                mockSession.TryEnd().Returns(false);

                var target = CreateContext() as ISessionWatchdogContext;
                target.CloseOrEnqueueForClosing(mockSession, SessionWatchdogContext.DefaultSleepTimeInMillis + 10);

                // when
                target.Execute();

                // then
                mockThreadSuspender.Received(1).Sleep(SessionWatchdogContext.DefaultSleepTimeInMillis);
            }
        }

        [Test]
        public void RequestShutdownSetsIsShutdownRequestedToTrue()
        {
            // given
            var target = CreateContext() as ISessionWatchdogContext;
            Assert.That(target.IsShutdownRequested, Is.False);

            // when
            target.RequestShutdown();

            // then
            Assert.That(target.IsShutdownRequested, Is.True);
        }

        [Test]
        public void RequestShutdownIsSetIfThreadSuspenderWasInterrupted()
        {
            // given
            mockThreadSuspender.Sleep(Arg.Any<int>()).Returns(false);
            var target = CreateContext() as ISessionWatchdogContext;

            Assert.That(target.IsShutdownRequested, Is.False);

            // when
            target.Execute();

            // then
            Assert.That(target.IsShutdownRequested, Is.True);
        }

        [Test]
        public void RequestShutdownWakesUpThreadSuspender()
        {
            // given
            var target = CreateContext() as ISessionWatchdogContext;

            // when
            target.RequestShutdown();

            // then
            mockThreadSuspender.Received(1).WakeUp();
        }

        private SessionWatchdogContext CreateContext()
        {
            return new SessionWatchdogContext(mockTimingProvider, mockThreadSuspender);
        }
    }
}