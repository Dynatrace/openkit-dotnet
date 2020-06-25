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
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Objects
{
    public class OpenKitTest
    {
        private const string AppId = "appId";
        private const long DeviceId = 1234;
        private const string AppName = "appName";

        private ILogger mockLogger;
        private IPrivacyConfiguration mockPrivacyConfig;
        private IOpenKitConfiguration mockOpenKitConfig;
        private ITimingProvider mockTimingProvider;
        private IThreadIdProvider mockThreadIdProvider;
        private ISessionIdProvider mockSessionIdProvider;
        private IBeaconCache mockBeaconCache;
        private IBeaconSender mockBeaconSender;
        private IBeaconCacheEvictor mockBeaconCacheEvictor;
        private ISessionWatchdog mockSessionWatchdog;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
            mockLogger.IsInfoEnabled.Returns(true);
            mockLogger.IsDebugEnabled.Returns(true);

            mockPrivacyConfig = Substitute.For<IPrivacyConfiguration>();
            mockPrivacyConfig.DataCollectionLevel.Returns(ConfigurationDefaults.DefaultDataCollectionLevel);
            mockPrivacyConfig.CrashReportingLevel.Returns(ConfigurationDefaults.DefaultCrashReportingLevel);

            mockOpenKitConfig = Substitute.For<IOpenKitConfiguration>();
            mockOpenKitConfig.ApplicationId.Returns(AppId);
            mockOpenKitConfig.DeviceId.Returns(DeviceId);
            mockOpenKitConfig.ApplicationName.Returns(AppName);
            mockOpenKitConfig.OperatingSystem.Returns(string.Empty);
            mockOpenKitConfig.Manufacturer.Returns(string.Empty);
            mockOpenKitConfig.ModelId.Returns(string.Empty);

            mockTimingProvider = Substitute.For<ITimingProvider>();
            mockThreadIdProvider = Substitute.For<IThreadIdProvider>();
            mockSessionIdProvider = Substitute.For<ISessionIdProvider>();
            mockBeaconCache = Substitute.For<IBeaconCache>();
            mockBeaconSender = Substitute.For<IBeaconSender>();
            mockBeaconCacheEvictor = Substitute.For<IBeaconCacheEvictor>();
            mockSessionWatchdog = Substitute.For<ISessionWatchdog>();
        }

        [Test]
        public void InitializeStartsTheBeaconCacheEvictor()
        {
            // given
            var target = CreateOpenKit().Build();

            // when
            target.Initialize();

            // then
            mockBeaconCacheEvictor.Received(1).Start();
        }

        [Test]
        public void InitializeInitializesBeaconSender()
        {
            // given
            var target = CreateOpenKit().Build();

            // when
            target.Initialize();

            // then
            mockBeaconSender.Received(1).Initialize();
        }

        [Test]
        public void InitializeInitializesSessionWatchdog()
        {
            // given
            var target = CreateOpenKit().Build();

            // when
            target.Initialize();

            // then
            mockSessionWatchdog.Received(1).Initialize();
        }

        [Test]
        public void WaitForInitCompletionForwardsTheCallToBeaconSender()
        {
            // given
            mockBeaconSender.WaitForInitCompletion().Returns(false, true);
            var target = CreateOpenKit().Build();

            // when
            var obtained = target.WaitForInitCompletion();

            // then
            Assert.That(obtained, Is.False);
            mockBeaconSender.Received(1).WaitForInitCompletion();

            // and when
            obtained = target.WaitForInitCompletion();

            // then
            Assert.That(obtained, Is.True);
            mockBeaconSender.Received(2).WaitForInitCompletion();
        }

        [Test]
        public void WaitForInitCompletionWithTimeoutForwardsTheCallToBeaconSender()
        {
            mockBeaconSender.WaitForInitCompletion(Arg.Any<int>()).Returns(false, true);
            var target = CreateOpenKit().Build();

            // when
            var obtained = target.WaitForInitCompletion(100);

            // then
            Assert.That(obtained, Is.False);
            mockBeaconSender.Received(1).WaitForInitCompletion(100);

            // and when
            obtained = target.WaitForInitCompletion(200);

            // then
            Assert.That(obtained, Is.True);
            mockBeaconSender.Received(1).WaitForInitCompletion(200);
        }

        [Test]
        public void IsInitializedForwardsCallToBeaconSender()
        {
            // given
            mockBeaconSender.IsInitialized.Returns(false, true);
            var target = CreateOpenKit().Build();

            // when
            var obtained = target.IsInitialized;

            // then
            Assert.That(obtained, Is.False);
            _ = mockBeaconSender.Received(1).IsInitialized;

            // and when
            obtained = target.IsInitialized;

            // then
            Assert.That(obtained, Is.True);
            _ = mockBeaconSender.Received(2).IsInitialized;
        }

        [Test]
        public void ShutdownStopsTheBeaconCacheEvictor()
        {
            // given
            var target = CreateOpenKit().Build();

            // when
            target.Shutdown();

            // then
            mockBeaconCacheEvictor.Received(1).Stop();
        }

        [Test]
        public void ShutdownShutsDownBeaconSender()
        {
            // given
            var target = CreateOpenKit().Build();

            // when
            target.Shutdown();

            // then
            mockBeaconSender.Received(1).Shutdown();
        }

        [Test]
        public void ShutdownShutsDownSessionWatchdog()
        {
            // given
            var target = CreateOpenKit().Build();

            // when
            target.Shutdown();

            // then
            mockSessionWatchdog.Received(1).Shutdown();
        }

        [Test]
        public void ShutdownDisposesAllChildObjects()
        {
            // given
            var target = CreateOpenKit().Build();
            IOpenKitComposite targetComposite = target;

            var childObjectOne = Substitute.For<IOpenKitObject>();
            var childObjectTwo = Substitute.For<IOpenKitObject>();
            targetComposite.StoreChildInList(childObjectOne);
            targetComposite.StoreChildInList(childObjectTwo);

            // when
            target.Shutdown();

            // then
            childObjectOne.Received(1).Dispose();
            childObjectTwo.Received(1).Dispose();
        }

        [Test]
        public void DisposeCallsShutdown()
        {
            // given
            var target = CreateOpenKit().Build();

            // when
            target.Dispose();

            // then
            mockBeaconSender.Received(1).Shutdown();
            mockBeaconCacheEvictor.Received(1).Stop();
        }

        [Test]
        public void CallingShutdownASecondTimeReturnsImmediately()
        {
            // given
            var target = CreateOpenKit().Build();
            IOpenKitComposite targetComposite = target;

            var childObjectOne = Substitute.For<IOpenKitObject>();
            var childObjectTwo = Substitute.For<IOpenKitObject>();
            targetComposite.StoreChildInList(childObjectOne);
            targetComposite.StoreChildInList(childObjectTwo);

            // when
            target.Shutdown();

            // then
            mockBeaconCacheEvictor.Received(1).Stop();
            mockBeaconSender.Received(1).Shutdown();
            childObjectOne.Received(1).Dispose();
            childObjectTwo.Received(1).Dispose();

            mockBeaconCacheEvictor.ClearReceivedCalls();
            mockBeaconSender.ClearReceivedCalls();
            childObjectOne.ClearReceivedCalls();
            childObjectTwo.ClearReceivedCalls();

            // and when
            target.Shutdown();

            // then
            Assert.That(mockBeaconCacheEvictor.ReceivedCalls(), Is.Empty);
            Assert.That(mockBeaconSender.ReceivedCalls(), Is.Empty);
            Assert.That(childObjectOne.ReceivedCalls(), Is.Empty);
            Assert.That(childObjectTwo.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void CreateSessionReturnsSessionProxyObject()
        {
            // given
            var target = CreateOpenKit().Build();

            // when
            var obtained = target.CreateSession("127.0.0.1");

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<SessionProxy>());
        }

        [Test]
        public void CreateSessionWithoutIpAddressReturnsSessionProxyObject()
        {
            // given
            var target = CreateOpenKit().Build();

            // when
            var obtained = target.CreateSession();

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<SessionProxy>());
        }

        [Test]
        public void CreateSessionAddsNewlyCreatedSessionToListOfChildren()
        {
            // given
            var target = CreateOpenKit().Build();
            IOpenKitComposite targetComposite = target;

            // when
            var sessionOne = target.CreateSession("127.0.0.1");

            // then
            var childObjects = targetComposite.GetCopyOfChildObjects();
            Assert.That(sessionOne, Is.Not.Null);
            Assert.That(childObjects.Count, Is.EqualTo(1));
            Assert.That(childObjects[0], Is.SameAs(sessionOne));

            // when
            var sessionTwo = target.CreateSession("192.168.0.1");

            // then
            childObjects = targetComposite.GetCopyOfChildObjects();
            Assert.That(sessionTwo, Is.Not.Null);
            Assert.That(childObjects.Count, Is.EqualTo(2));
            Assert.That(childObjects[0], Is.SameAs(sessionOne));
            Assert.That(childObjects[1], Is.SameAs(sessionTwo));
        }

        [Test]
        public void CreateSessionWithoutIpAddsNewlyCreatedSessionToListOfChildren()
        {
            // given
            var target = CreateOpenKit().Build();
            IOpenKitComposite targetComposite = target;

            // when
            var sessionOne = target.CreateSession();

            // then
            var childObjects = targetComposite.GetCopyOfChildObjects();
            Assert.That(sessionOne, Is.Not.Null);
            Assert.That(childObjects.Count, Is.EqualTo(1));
            Assert.That(childObjects[0], Is.SameAs(sessionOne));

            // when
            var sessionTwo = target.CreateSession();

            // then
            childObjects = targetComposite.GetCopyOfChildObjects();
            Assert.That(sessionTwo, Is.Not.Null);
            Assert.That(childObjects.Count, Is.EqualTo(2));
            Assert.That(childObjects[0], Is.SameAs(sessionOne));
            Assert.That(childObjects[1], Is.SameAs(sessionTwo));
        }

        [Test]
        public void CreateSessionAfterShutdownHasBeenCalledReturnsNullSession()
        {
            // given
            var target = CreateOpenKit().Build();
            target.Shutdown();

            // when
            var obtained = target.CreateSession("127.0.0.1");

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<NullSession>());
            Assert.That(obtained, Is.SameAs(NullSession.Instance));
        }

        [Test]
        public void CreateSessionWithoutIpAfterShutdownHasBeenCalledReturnsNullSession()
        {
            // given
            var target = CreateOpenKit().Build();
            target.Shutdown();

            // when
            var obtained = target.CreateSession();

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<NullSession>());
            Assert.That(obtained, Is.SameAs(NullSession.Instance));
        }

        [Test]
        public void OnChildClosedRemovesArgumentFromListOfChildren()
        {
            // given
            IOpenKitComposite target = CreateOpenKit().Build();

            var childObjectOne = Substitute.For<IOpenKitObject>();
            var childObjectTwo = Substitute.For<IOpenKitObject>();
            target.StoreChildInList(childObjectOne);
            target.StoreChildInList(childObjectTwo);

            // when
            target.OnChildClosed(childObjectOne);

            // then
            var childObjects = target.GetCopyOfChildObjects();
            Assert.That(childObjects.Count, Is.EqualTo(1));
            Assert.That(childObjects[0], Is.SameAs(childObjectTwo));

            // when
            target.OnChildClosed(childObjectTwo);

            // then
            Assert.That(target.GetCopyOfChildObjects(), Is.Empty);
        }

        private TestOpenKitBuilder CreateOpenKit()
        {
            return new TestOpenKitBuilder()
                    .With(mockLogger)
                    .With(mockPrivacyConfig)
                    .With(mockOpenKitConfig)
                    .With(mockThreadIdProvider)
                    .With(mockTimingProvider)
                    .With(mockSessionIdProvider)
                    .With(mockBeaconCache)
                    .With(mockBeaconSender)
                    .With(mockBeaconCacheEvictor)
                    .With(mockSessionWatchdog)
                ;
        }
    }
}