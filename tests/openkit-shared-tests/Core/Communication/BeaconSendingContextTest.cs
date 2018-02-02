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

using NUnit.Framework;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Caching;

namespace Dynatrace.OpenKit.Core.Communication
{
    public class BeaconSendingContextTest
    {
        private OpenKitConfiguration config;
        private IHTTPClientProvider clientProvider;
        private ITimingProvider timingProvider;
        private AbstractBeaconSendingState nonTerminalStateMock;
        private ILogger logger = new DefaultLogger(true);

        [SetUp]
        public void Setup()
        {
            config = new TestConfiguration();
            clientProvider = Substitute.For<IHTTPClientProvider>();
            timingProvider = Substitute.For<ITimingProvider>();
            nonTerminalStateMock = Substitute.For<AbstractBeaconSendingState>(false);
        }

        [Test]
        public void ContextIsInitializedWithInitState()
        {
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);

            Assert.IsNotNull(target.CurrentState);
            Assert.AreEqual(typeof(BeaconSendingInitState), target.CurrentState.GetType());
        }

        [Test]
        public void CurrentStateIsSet()
        {
            BeaconSendingContext target = new BeaconSendingContext(config, clientProvider, timingProvider)
            {
                CurrentState = nonTerminalStateMock
            };

            Assert.AreSame(nonTerminalStateMock, target.CurrentState);
        }

        [Test]
        public void ExecuteIsCalledOnCurrentState()
        {
            var target = new BeaconSendingContext(config, clientProvider, timingProvider)
            {
                CurrentState = nonTerminalStateMock
            };

            target.ExecuteCurrentState();

            nonTerminalStateMock.Received(1).Execute(target);
        }

        [Test]
        public void ResetEventIsSetOnInitSuccess()
        {
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);

            target.InitCompleted(true);
            var actual = target.WaitForInit();

            Assert.That(actual, Is.True);
        }

        [Test]
        public void ResetEventIsSetOnInitFailed()
        {

            var target = new BeaconSendingContext(config, clientProvider, timingProvider);

            target.InitCompleted(false);
            var actual = target.WaitForInit();

            Assert.That(actual, Is.False);
        }

        [Test]
        public void IsInitializedOnInitSuccess()
        {
            // given
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);
            target.InitCompleted(true);

            // when, then
            Assert.That(target.IsInitialized, Is.True);
        }

        [Test]
        public void IsInitializedOnInitFailed()
        {
            // given
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);
            target.InitCompleted(false);

            // when, then
            Assert.That(target.IsInitialized, Is.False);
        }

        [Test]
        public void WaitForInitWhenTimeoutExpires()
        {
            // given
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);

            // when waiting for init completion with a timeout of 1ms
            var obtained = target.WaitForInit(1);

            // then the result must be false, since init was never set, but timeout expired
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void WaitForInitWhenWithTimeoutSuccess()
        {
            // given
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);
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
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);
            target.InitCompleted(false);

            // when waiting for init completion with a timeout of 1ms
            var obtained = target.WaitForInit(1);

            // then the result must be false, since init was never set, but timeout expired
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void IsShutdownRequestedIsSetCorrectly()
        {
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);

            Assert.False(target.IsShutdownRequested);

            target.RequestShutdown();

            Assert.True(target.IsShutdownRequested);
        }

        [Test]
        public void LastOpenSessionSendTimeIsSet()
        {
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);

            Assert.AreEqual(0, target.LastOpenSessionBeaconSendTime);

            var expected = 17;
            target.LastOpenSessionBeaconSendTime = expected;

            Assert.AreEqual(expected, target.LastOpenSessionBeaconSendTime);
        }

        [Test]
        public void LastStatusCheckTimeIsSet()
        {
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);

            Assert.AreEqual(0, target.LastStatusCheckTime);

            var expected = 17;
            target.LastStatusCheckTime = expected;

            Assert.AreEqual(expected, target.LastStatusCheckTime);
        }

        [Test]
        public void TimeSyncSupportIsTrueByDefault()
        {
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);

            Assert.That(target.IsTimeSyncSupported, Is.True);
        }

        [Test]
        public void LastTimeSyncTimeIsInitializedWithMinus1()
        {
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);

            var expected = -1;

            Assert.AreEqual(expected, target.LastTimeSyncTime);
        }

        [Test]
        public void LastTimeSyncTimeIsSet()
        {
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);

            Assert.AreEqual(-1, target.LastTimeSyncTime);

            var expected = 17;
            target.LastTimeSyncTime = expected;

            Assert.AreEqual(expected, target.LastTimeSyncTime);
        }

        [Test]
        public void CanGetHttpClient()
        {
            var expected = Substitute.For<HTTPClient>(new DefaultLogger(true), new HTTPClientConfiguration("", 0, "", null));

            clientProvider.CreateClient(Arg.Any<HTTPClientConfiguration>()).Returns(expected);

            var target = new BeaconSendingContext(config, clientProvider, timingProvider);

            var actual = target.GetHTTPClient();

            Assert.NotNull(actual);
            Assert.AreSame(expected, actual);
            clientProvider.Received(1).CreateClient(Arg.Any<HTTPClientConfiguration>());
        }

        [Test]
        public void GetHttpClientUsesCurrentHttpConfig()
        {
            clientProvider
                .CreateClient(Arg.Any<HTTPClientConfiguration>())
                .Returns(Substitute.For<HTTPClient>(new DefaultLogger(true), new HTTPClientConfiguration("", 0, "", null)));

            var target = new BeaconSendingContext(config, clientProvider, timingProvider);

            var actual = target.GetHTTPClient();

            clientProvider.Received(1).CreateClient(config.HTTPClientConfig);
        }

        [Test]
        public void CanGetCurrentTimestamp()
        {
            var expected = 12356789;
            timingProvider.ProvideTimestampInMilliseconds().Returns(expected);

            var target = new BeaconSendingContext(config, clientProvider, timingProvider);

            var actual = target.CurrentTimestamp;

            Assert.AreEqual(expected, actual);
            timingProvider.Received(1).ProvideTimestampInMilliseconds();
        }

        [Test]
        public void DefaultSleepTimeIsUsed()
        {
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);

            target.Sleep();

            timingProvider.Received(1).Sleep(BeaconSendingContext.DEFAULT_SLEEP_TIME_MILLISECONDS);
        }

        [Test]
        public void CanSleepCustomPeriod()
        {
            var expected = 1717;

            var target = new BeaconSendingContext(config, clientProvider, timingProvider);

            target.Sleep(expected);

#if !NETCOREAPP1_0
            timingProvider.Received(1).Sleep(expected);
#else
            timingProvider.Received(2).Sleep(Arg.Any<int>());
            timingProvider.Received(1).Sleep(1000);
            timingProvider.Received(1).Sleep(717);
#endif
        }

        [Test]
        public void SessionIsMovedToFinished()
        {
            // given
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);

            var session = new Session(new BeaconSender(target), new Beacon(Substitute.For<ILogger>(), new BeaconCache(),
                config, "127.0.0.1", Substitute.For<IThreadIDProvider>(), timingProvider));

            Assert.That(target.GetAllOpenSessions().Count, Is.EqualTo(1));

            // when
            target.FinishSession(session);

            // then
            Assert.That(target.GetAllOpenSessions(), Is.Empty);
            Assert.That(target.GetNextFinishedSession(), Is.SameAs(session));
            Assert.That(target.GetNextFinishedSession(), Is.Null);
        }

        [Test]
        public void DisableCaptureDisablesItInTheConfiguration()
        {
            // given
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);
            config.EnableCapture();

            // when disabling capture
            target.DisableCapture();

            // then it's disabled again
            Assert.That(config.IsCaptureOn, Is.False);
        }
    }
}
