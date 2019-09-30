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
        private IOpenKitConfiguration mockConfig;
        private ITimingProvider mockTimingProvider;
        private IThreadIdProvider mockThreadIdProvider;
        private IBeaconCache mockBeaconCache;
        private IBeaconSender mockBeaconSender;
        private IBeaconCacheEvictor mockBeaconCacheEvictor;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
            mockLogger.IsInfoEnabled.Returns(true);
            mockLogger.IsDebugEnabled.Returns(true);

            mockConfig = Substitute.For<IOpenKitConfiguration>();
            mockConfig.ApplicationId.Returns(AppId);
            mockConfig.DeviceId.Returns(DeviceId);
            mockConfig.ApplicationName.Returns(AppName);
            mockConfig.Device.Returns(new Device("", "", ""));

            mockTimingProvider = Substitute.For<ITimingProvider>();
            mockThreadIdProvider = Substitute.For<IThreadIdProvider>();
            mockBeaconCache = Substitute.For<IBeaconCache>();
            mockBeaconSender = Substitute.For<IBeaconSender>();
            mockBeaconCacheEvictor = Substitute.For<IBeaconCacheEvictor>();
        }

        [Test]
        public void InitializeStartsTheBeaconCacheEvictor()
        {
            // given
            var target = CreateOpenKit();

            // when
            target.Initialize();

            // then
            mockBeaconCacheEvictor.Received(1).Start();
        }

        [Test]
        public void InitializeInitializesBeaconSender()
        {
            // given
            var target = CreateOpenKit();

            // when
            target.Initialize();

            // then
            mockBeaconSender.Received(1).Initialize();
        }

        [Test]
        public void WaitForInitCompletionForwardsTheCallToBeaconSender()
        {
            // given
            mockBeaconSender.WaitForInitCompletion().Returns(false, true);
            var target = CreateOpenKit();

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
            var target = CreateOpenKit();

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
            var target = CreateOpenKit();

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
            var target = CreateOpenKit();

            // when
            target.Shutdown();

            // then
            mockBeaconCacheEvictor.Received(1).Stop();
        }

        [Test]
        public void ShutdownShutsDownBeaconSender()
        {
            // given
            var target = CreateOpenKit();

            // when
            target.Shutdown();

            // then
            mockBeaconSender.Received(1).Shutdown();
        }

        [Test]
        public void ShutdownClosesAllChildObjects()
        {
            // given
            var target = CreateOpenKit();
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
        public void CallingShutdownASecondTimeReturnsImmediately()
        {
            // given
            var target = CreateOpenKit();
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
        public void CreateSessionReturnsSessionObject()
        {
            // given
            var target = CreateOpenKit();

            // when
            var obtained = target.CreateSession("127.0.0.1");

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<Session>());
        }

        [Test]
        public void CreateSessionAddsNewlyCreatedSessionToListOfChildren()
        {
            // given
            var target = CreateOpenKit();
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
        public void CreateSessionAfterShutdownHasBeenCalledReturnsNullSession()
        {
            // given
            var target = CreateOpenKit();
            target.Shutdown();

            // when
            var obtained = target.CreateSession("127.0.0.1");

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<NullSession>());
            Assert.That(obtained, Is.SameAs(NullSession.Instance));
        }

        [Test]
        public void OnChildClosedRemovesArgumentFromListOfChildren()
        {
            // given
            IOpenKitComposite target = CreateOpenKit();

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

        private OpenKit CreateOpenKit()
        {
            return new OpenKit(
                mockLogger,
                mockConfig,
                mockTimingProvider,
                mockThreadIdProvider,
                mockBeaconCache,
                mockBeaconSender,
                mockBeaconCacheEvictor
                );
        }
    }
}