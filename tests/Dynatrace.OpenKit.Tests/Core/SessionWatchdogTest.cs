//
// Copyright 2018-2021 Dynatrace LLC
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

using System.Threading;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Objects;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core
{
    public class SessionWatchdogTest
    {
        private ILogger mockLogger;
        private ISessionWatchdogContext mockContext;
        private ISessionInternals mockSession;

        private ManualResetEvent threadEvent;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
            mockLogger.IsDebugEnabled.Returns(true);

            mockContext = Substitute.For<ISessionWatchdogContext>();
            mockSession = Substitute.For<ISessionInternals>();

            threadEvent = new ManualResetEvent(false);
        }

        [Test]
        public void InitializeLogsWatchdogThreadStart()
        {
            // given
            mockContext.IsShutdownRequested.ReturnsForAnyArgs(x => true).AndDoes(x => threadEvent.Set());
            var target = CreateWatchdog();

            // when
            target.Initialize();

            threadEvent.WaitOne(1000); // wait for thread

            // then
            mockLogger.Received(1)
                .Debug($"{typeof(SessionWatchdog).Name} Initialize() - thread started");
            _ = mockContext.Received(1).IsShutdownRequested;
            mockContext.Received(0).Execute();

            // cleanup
            target.Shutdown();
        }

        [Test]
        public void ContextIsExecutedUntilShutdownIsRequested()
        {
            // given
            mockContext.IsShutdownRequested.Returns(false, false, true);
            mockContext.When(c => _ = c.IsShutdownRequested)
                .Do(Callback.First(x => { }).Then(x => { }).Then(x => threadEvent.Set()));
            var target = CreateWatchdog();

            // when
            target.Initialize();
            threadEvent.WaitOne(1000); // wait till shutdown requested returns true

            // then
            _ = mockContext.Received(3).IsShutdownRequested;
            mockContext.Received(2).Execute();
        }

        [Test]
        public void ShutdownLogsWatchdogThreadStop()
        {
            // given
            mockContext.IsShutdownRequested.Returns(true);
            mockContext.When(c => _ = c.IsShutdownRequested).Do(Callback.First(x => threadEvent.Set()));
            var target = CreateWatchdog();
            target.Initialize();

            threadEvent.WaitOne(1000); // wait for thread

            // when
            target.Shutdown();

            // then
            mockLogger.Received(1).Debug($"{typeof(SessionWatchdog).Name} Shutdown() - thread stopped");
        }

        [Test]
        public void ShutdownLogsInvocation()
        {
            // given
            var target = CreateWatchdog();

            // when
            target.Shutdown();

            // then
            mockLogger.Received(1)
                .Debug($"{typeof(SessionWatchdog).Name} Shutdown() - thread request shutdown");
        }

        [Test]
        public void CloseOrEnqueueForClosingDelegatesToSessionWatchdogContext()
        {
            // given
            const int gracePeriod = 1;
            var target = CreateWatchdog() as ISessionWatchdog;

            // when
            target.CloseOrEnqueueForClosing(mockSession, gracePeriod);

            // then
            mockContext.Received(1).CloseOrEnqueueForClosing(mockSession, gracePeriod);
        }

        [Test]
        public void DequeueFromClosingDelegatesToSessionWatchdogContext()
        {
            // given
            var target = CreateWatchdog() as ISessionWatchdog;

            // when
            target.DequeueFromClosing(mockSession);

            // then
            mockContext.Received(1).DequeueFromClosing(mockSession);
        }

        [Test]
        public void AddToSplitByTimeoutDelegatesToSessionWatchdogContext()
        {
            // given
            var mockSessionProxy = Substitute.For<ISessionProxy>();
            var target = CreateWatchdog() as ISessionWatchdog;

            // when
            target.AddToSplitByTimeout(mockSessionProxy);

            // then
            mockContext.Received(1).AddToSplitByTimeout(mockSessionProxy);
        }

        [Test]
        public void RemoveFromSplitByTimeoutDelegatesToSessionWatchdogContext()
        {
            // given
            var mockSessionProxy = Substitute.For<ISessionProxy>();
            var target = CreateWatchdog() as ISessionWatchdog;

            // when
            target.RemoveFromSplitByTimeout(mockSessionProxy);

            // then
            mockContext.Received(1).RemoveFromSplitByTimeout(mockSessionProxy);
        }

        private SessionWatchdog CreateWatchdog()
        {
            return new SessionWatchdog(mockLogger, mockContext);
        }
    }
}