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

using System.Linq;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Communication
{
    public class BeaconSendingContextTest
    {
        private IOpenKitConfiguration config;
        private IHttpClientProvider clientProvider;
        private ITimingProvider timingProvider;
        private AbstractBeaconSendingState nonTerminalStateMock;
        private ILogger logger = new DefaultLogger(LogLevel.DEBUG);

        [SetUp]
        public void Setup()
        {
            config = new TestConfiguration();
            clientProvider = Substitute.For<IHttpClientProvider>();
            timingProvider = Substitute.For<ITimingProvider>();
            nonTerminalStateMock = Substitute.For<AbstractBeaconSendingState>(false);
        }

        [Test]
        public void ContextIsInitializedWithInitState()
        {
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);

            Assert.IsNotNull(target.CurrentState);
            Assert.AreEqual(typeof(BeaconSendingInitState), target.CurrentState.GetType());
        }

        [Test]
        public void CurrentStateIsSet()
        {
            BeaconSendingContext target = new BeaconSendingContext(logger, config, clientProvider, timingProvider)
            {
                CurrentState = nonTerminalStateMock
            };

            Assert.AreSame(nonTerminalStateMock, target.CurrentState);
        }

        [Test]
        public void ExecuteIsCalledOnCurrentState()
        {
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider)
            {
                CurrentState = nonTerminalStateMock
            };

            target.ExecuteCurrentState();

            nonTerminalStateMock.Received(1).Execute(target);
        }

        [Test]
        public void ResetEventIsSetOnInitSuccess()
        {
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);

            target.InitCompleted(true);
            var actual = target.WaitForInit();

            Assert.That(actual, Is.True);
        }

        [Test]
        public void ResetEventIsSetOnInitFailed()
        {

            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);

            target.InitCompleted(false);
            var actual = target.WaitForInit();

            Assert.That(actual, Is.False);
        }

        [Test]
        public void IsInitializedOnInitSuccess()
        {
            // given
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);
            target.InitCompleted(true);

            // when, then
            Assert.That(target.IsInitialized, Is.True);
        }

        [Test]
        public void IsInitializedOnInitFailed()
        {
            // given
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);
            target.InitCompleted(false);

            // when, then
            Assert.That(target.IsInitialized, Is.False);
        }

        [Test]
        public void WaitForInitWhenTimeoutExpires()
        {
            // given
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);

            // when waiting for init completion with a timeout of 1ms
            var obtained = target.WaitForInit(1);

            // then the result must be false, since init was never set, but timeout expired
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void WaitForInitWhenWithTimeoutSuccess()
        {
            // given
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);
            target.InitCompleted(true);

            // when waiting for init completion with a timeout of 1ms
            var obtained = target.WaitForInit(1);

            // then the result must be false, since init was never set, but timeout expired
            Assert.That(obtained, Is.True);
        }

        [Test]
        public void WaitForInitWhenWithTimeoutFailed()
        {
            // given
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);
            target.InitCompleted(false);

            // when waiting for init completion with a timeout of 1ms
            var obtained = target.WaitForInit(1);

            // then the result must be false, since init was never set, but timeout expired
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void IsShutdownRequestedIsSetCorrectly()
        {
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);

            Assert.False(target.IsShutdownRequested);

            target.RequestShutdown();

            Assert.True(target.IsShutdownRequested);
        }

        [Test]
        public void LastOpenSessionSendTimeIsSet()
        {
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);

            Assert.AreEqual(0, target.LastOpenSessionBeaconSendTime);

            var expected = 17;
            target.LastOpenSessionBeaconSendTime = expected;

            Assert.AreEqual(expected, target.LastOpenSessionBeaconSendTime);
        }

        [Test]
        public void LastStatusCheckTimeIsSet()
        {
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);

            Assert.AreEqual(0, target.LastStatusCheckTime);

            var expected = 17;
            target.LastStatusCheckTime = expected;

            Assert.AreEqual(expected, target.LastStatusCheckTime);
        }

        [Test]
        public void CanGetHttpClient()
        {
            var expected = Substitute.For<HttpClient>(new DefaultLogger(LogLevel.DEBUG), new HttpClientConfiguration("", 0, "", null));

            clientProvider.CreateClient(Arg.Any<HttpClientConfiguration>()).Returns(expected);

            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);

            var actual = target.GetHttpClient();

            Assert.NotNull(actual);
            Assert.AreSame(expected, actual);
            clientProvider.Received(1).CreateClient(Arg.Any<HttpClientConfiguration>());
        }

        [Test]
        public void GetHttpClientUsesCurrentHttpConfig()
        {
            clientProvider
                .CreateClient(Arg.Any<HttpClientConfiguration>())
                .Returns(Substitute.For<HttpClient>(new DefaultLogger(LogLevel.DEBUG), new HttpClientConfiguration("", 0, "", null)));

            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);

            var actual = target.GetHttpClient();

            clientProvider.Received(1).CreateClient(config.HttpClientConfig);
        }

        [Test]
        public void CanGetCurrentTimestamp()
        {
            var expected = 12356789;
            timingProvider.ProvideTimestampInMilliseconds().Returns(expected);

            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);

            var actual = target.CurrentTimestamp;

            Assert.AreEqual(expected, actual);
            timingProvider.Received(1).ProvideTimestampInMilliseconds();
        }

        [Test]
        public void DefaultSleepTimeIsUsed()
        {
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);

            target.Sleep();

            timingProvider.Received(1).Sleep(BeaconSendingContext.DefaultSleepTimeMilliseconds);
        }

        [Test]
        public void CanSleepCustomPeriod()
        {
            var expected = 1717;

            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);

            target.Sleep(expected);

#if !NETCOREAPP1_0 || !NETCOREAPP1_1
            timingProvider.Received(1).Sleep(expected);
#else
            timingProvider.Received(2).Sleep(Arg.Any<int>());
            timingProvider.Received(1).Sleep(1000);
            timingProvider.Received(1).Sleep(717);
#endif
        }

        [Test]
        public void CanInterruptLongSleep()
        {
            // given
            var expected = 101717;
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);
            target.RequestShutdown();
            target.Sleep(expected);

            // then
#if !NETCOREAPP1_0 || !NETCOREAPP1_1
            // normal sleep as thread interrupt exception exists
            timingProvider.Received(1).Sleep(expected);
#else
            // no interrupt exception exists, therefore "sliced" sleep break after first iteration
            timingProvider.Received(1).Sleep(Arg.Any<int>());
            timingProvider.Received(1).Sleep(BeaconSendingContext.DEFAULT_SLEEP_TIME_MILLISECONDS);
#endif
        }

        [Test]
        public void CanSleepLonger()
        {
            // given
            var expected = 101717;
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);
            target.Sleep(expected);

            // then
#if !NETCOREAPP1_0 || !NETCOREAPP1_1
            // normal sleep as thread interrupt exception exists
            timingProvider.Received(1).Sleep(expected);

#else
            // no interrupt exception exists, therefore "sliced" sleeps until total sleep amount
            var expectedCount = (int)Math.Ceiling(expected / (double)BeaconSendingContext.DEFAULT_SLEEP_TIME_MILLISECONDS);
            timingProvider.Received(expectedCount).Sleep(Arg.Any<int>());
            timingProvider.Received(expectedCount - 1).Sleep(BeaconSendingContext.DEFAULT_SLEEP_TIME_MILLISECONDS);
            timingProvider.Received(1).Sleep(expected % BeaconSendingContext.DEFAULT_SLEEP_TIME_MILLISECONDS);
#endif
        }

        [Test]
        public void ADefaultConstructedContextDoesNotStoreAnySessions()
        {
            // given
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);

            // then
            Assert.That(target.NewSessions, Is.Empty);
            Assert.That(target.OpenAndConfiguredSessions, Is.Empty);
            Assert.That(target.FinishedAndConfiguredSessions, Is.Empty);
        }

        [Test]
        public void WhenStartingASessionTheSessionIsConsideredAsNew()
        {
            // given
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);

            // when starting the first session
            // Caution: Session CTOR implicitly starts itself!!!
            var sessionOne = new Session(logger, new BeaconSender(logger, target), new Beacon(logger, new BeaconCache(logger),
                config, "127.0.0.1", Substitute.For<IThreadIdProvider>(), timingProvider));

            // then
            Assert.That(target.NewSessions.Select(s => s.Session), Is.EquivalentTo(new[] { sessionOne }));
            Assert.That(target.OpenAndConfiguredSessions, Is.Empty);
            Assert.That(target.FinishedAndConfiguredSessions, Is.Empty);

            // when starting the second session
            // Caution: Session CTOR implicitly starts itself!!!
            var sessionTwo = new Session(logger, new BeaconSender(logger, target), new Beacon(logger, new BeaconCache(logger),
                config, "127.0.0.1", Substitute.For<IThreadIdProvider>(), timingProvider));

            // then
            Assert.That(target.NewSessions.Select(s => s.Session), Is.EquivalentTo(new[] { sessionOne, sessionTwo }));
            Assert.That(target.OpenAndConfiguredSessions, Is.Empty);
            Assert.That(target.FinishedAndConfiguredSessions, Is.Empty);
        }

        [Test]
        public void DisableCaptureDisablesItInTheConfiguration()
        {
            // given
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);
            config.EnableCapture();

            // when disabling capture
            target.DisableCapture();

            // then it's disabled again
            Assert.That(config.IsCaptureOn, Is.False);
        }

        [Test]
        public void FinishingANewSessionStillLeavesItNew()
        {
            // given
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);

            var session = new Session(logger, new BeaconSender(logger, target), new Beacon(logger, new BeaconCache(logger),
                config, "127.0.0.1", Substitute.For<IThreadIdProvider>(), timingProvider));

            // then
            Assert.That(target.NewSessions.Select(s => s.Session), Is.EquivalentTo(new[] { session }));
            Assert.That(target.OpenAndConfiguredSessions, Is.Empty);
            Assert.That(target.FinishedAndConfiguredSessions, Is.Empty);

            // and when finishing the session
            target.FinishSession(session);

            // then it's in the list of new ones
            Assert.That(target.NewSessions.Select(s => s.Session), Is.EquivalentTo(new[] { session }));
            Assert.That(target.OpenAndConfiguredSessions, Is.Empty);
            Assert.That(target.FinishedAndConfiguredSessions, Is.Empty);

        }

        [Test]
        public void AfterASessionHasBeenConfiguredItsOpenAndConfigured()
        {
            // given
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);

            // when both session are added
            var sessionOne = new Session(logger, new BeaconSender(logger, target), new Beacon(logger, new BeaconCache(logger),
                config, "127.0.0.1", Substitute.For<IThreadIdProvider>(), timingProvider));
            var sessionTwo = new Session(logger, new BeaconSender(logger, target), new Beacon(logger, new BeaconCache(logger),
                config, "127.0.0.1", Substitute.For<IThreadIdProvider>(), timingProvider));

            // and configuring the first one
            target.NewSessions[0].UpdateBeaconConfiguration(new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF));

            // then
            Assert.That(target.NewSessions.Select(s => s.Session), Is.EqualTo(new[] { sessionTwo }));
            Assert.That(target.OpenAndConfiguredSessions.Select(s => s.Session), Is.EqualTo(new[] { sessionOne }));
            Assert.That(target.FinishedAndConfiguredSessions, Is.Empty);

            // and when configuring the second open session
            target.NewSessions[0].UpdateBeaconConfiguration(new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF));

            // then
            Assert.That(target.NewSessions, Is.Empty);
            Assert.That(target.OpenAndConfiguredSessions.Select(s => s.Session), Is.EqualTo(new[] { sessionOne, sessionTwo }));
            Assert.That(target.FinishedAndConfiguredSessions, Is.Empty);
        }

        [Test]
        public void AfterAFinishedSessionHasBeenConfiguredItsFinishedAndConfigured()
        {
            // given
            var target = new BeaconSendingContext(logger, config, clientProvider, timingProvider);

            var sessionOne = new Session(logger, new BeaconSender(logger, target), new Beacon(logger, new BeaconCache(logger),
                config, "127.0.0.1", Substitute.For<IThreadIdProvider>(), timingProvider));
            var sessionTwo = new Session(logger, new BeaconSender(logger, target), new Beacon(logger, new BeaconCache(logger),
                config, "127.0.0.1", Substitute.For<IThreadIdProvider>(), timingProvider));

            // when both session are added
            // Caution: Session CTOR implicitly starts itself!!!
            target.FinishSession(sessionOne);
            target.FinishSession(sessionTwo);

            // and configuring the first one
            target.NewSessions[0].UpdateBeaconConfiguration(new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF));

            // then
            Assert.That(target.NewSessions.Select(s => s.Session), Is.EqualTo(new[] { sessionTwo }));
            Assert.That(target.OpenAndConfiguredSessions, Is.Empty);
            Assert.That(target.FinishedAndConfiguredSessions.Select(s => s.Session), Is.EqualTo(new[] { sessionOne }));

            // and when configuring the second open session
            target.NewSessions[0].UpdateBeaconConfiguration(new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF));

            // then
            Assert.That(target.NewSessions, Is.Empty);
            Assert.That(target.OpenAndConfiguredSessions, Is.Empty);
            Assert.That(target.FinishedAndConfiguredSessions.Select(s => s.Session), Is.EqualTo(new[] { sessionOne, sessionTwo }));
        }
    }
}
