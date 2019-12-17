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
    public class SessionCreatorTest
    {
        private ISessionCreatorInput mockInput;
        private ILogger mockLogger;
        private IOpenKitConfiguration mockOpenKitConfiguration;
        private IPrivacyConfiguration mockPrivacyConfiguration;
        private IBeaconCache mockBeaconCache;
        private ISessionIdProvider mockSessionIdProvider;
        private IThreadIdProvider mockThreadIdProvider;
        private ITimingProvider mockTimingProvider;
        private IOpenKitComposite mockParent;
        private const int ServerId = 999;
        private const int SessionId = 777;
        private const long DeviceId = 1;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
            mockOpenKitConfiguration = Substitute.For<IOpenKitConfiguration>();
            mockOpenKitConfiguration.ApplicationId.Returns(string.Empty);
            mockOpenKitConfiguration.ApplicationName.Returns(string.Empty);
            mockOpenKitConfiguration.ApplicationVersion.Returns(string.Empty);
            mockOpenKitConfiguration.DeviceId.Returns(DeviceId);

            mockPrivacyConfiguration = Substitute.For<IPrivacyConfiguration>();
            mockBeaconCache = Substitute.For<IBeaconCache>();
            mockSessionIdProvider = Substitute.For<ISessionIdProvider>();
            mockThreadIdProvider = Substitute.For<IThreadIdProvider>();
            mockTimingProvider = Substitute.For<ITimingProvider>();
            mockParent = Substitute.For<IOpenKitComposite>();

            mockInput = Substitute.For<ISessionCreatorInput>();
            mockInput.Logger.Returns(mockLogger);
            mockInput.OpenKitConfiguration.Returns(mockOpenKitConfiguration);
            mockInput.PrivacyConfiguration.Returns(mockPrivacyConfiguration);
            mockInput.BeaconCache.Returns(mockBeaconCache);
            mockInput.SessionIdProvider.Returns(mockSessionIdProvider);
            mockInput.ThreadIdProvider.Returns(mockThreadIdProvider);
            mockInput.TimingProvider.Returns(mockTimingProvider);
            mockInput.CurrentServerId.Returns(ServerId);
        }

        [Test]
        public void ConstructorTakesOverLogger()
        {
            // when
            CreateSessionCreator();

            // then
            _ = mockInput.Received(1).Logger;
            Assert.That(mockLogger.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ConstructorTakesOverOpenKitConfiguration()
        {
            // given, when
            CreateSessionCreator();

            // then
            _ = mockInput.Received(1).OpenKitConfiguration;
            Assert.That(mockOpenKitConfiguration.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ConstructorTakesOverPrivacyConfiguration()
        {
            // when
            CreateSessionCreator();

            // then
            _ = mockInput.Received(1).PrivacyConfiguration;
            Assert.That(mockPrivacyConfiguration.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ConstructorTakesOverBeaconCache()
        {
            // when
            CreateSessionCreator();

            // then
            _ = mockInput.Received(1).BeaconCache;
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ConstructorTakesOverThreadIdProvider()
        {
            // when
            CreateSessionCreator();

            // then
            _ = mockInput.Received(1).ThreadIdProvider;
            Assert.That(mockThreadIdProvider.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ConstructorTakesOverTimingProvider()
        {
            // given, when
            CreateSessionCreator();

            // then
            _ = mockInput.Received(1).TimingProvider;
            Assert.That(mockTimingProvider.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ConstructorTakesOverServerId()
        {
            //  when
            CreateSessionCreator();

            // then
            _ = mockInput.Received(1).CurrentServerId;
        }

        [Test]
        public void ConstructorDrawsNextSessionId()
        {
            // when
            CreateSessionCreator();

            // then
            mockSessionIdProvider.Received(1).GetNextSessionId();
        }

        [Test]
        public void CreateSessionReturnsNewSessionInstance()
        {
            // given
            var target = CreateSessionCreator() as ISessionCreator;

            // when
            var obtained = target.CreateSession(mockParent);

            // then
            Assert.That(obtained,  Is.Not.Null);
        }

        [Test]
        public void CreateSessionGivesSessionsWithAlwaysSameSessionNumber()
        {
            // given
            mockPrivacyConfiguration.IsSessionNumberReportingAllowed.Returns(true);
            mockSessionIdProvider.GetNextSessionId().Returns(SessionId, 1, 2, 3);
            var target = CreateSessionCreator() as ISessionCreator;

            // when
            var obtainedOne = target.CreateSession(mockParent);
            var obtainedTwo = target.CreateSession(mockParent);
            var obtainedThree = target.CreateSession(mockParent);

            // then
            Assert.That(obtainedOne.Beacon.SessionNumber,  Is.EqualTo(SessionId));
            Assert.That(obtainedTwo.Beacon.SessionNumber,  Is.EqualTo(SessionId));
            Assert.That(obtainedThree.Beacon.SessionNumber,  Is.EqualTo(SessionId));
        }

        [Test]
        public void CreateSessionGivesSessionsWithSameRandomizedDeviceId()
        {
            // given
            mockPrivacyConfiguration.IsDeviceIdSendingAllowed.Returns(false);

            var target = CreateSessionCreator();
            ISessionCreator targetExplicit = target;
            var randomizedDeviceId = target.RandomNumberGenerator.NextPositiveLong();

            // when
            var obtainedOne = targetExplicit.CreateSession(mockParent);
            var obtainedTwo = targetExplicit.CreateSession(mockParent);
            var obtainedThree = targetExplicit.CreateSession(mockParent);

            // then
            Assert.That(obtainedOne.Beacon.DeviceId,  Is.EqualTo(randomizedDeviceId));
            Assert.That(obtainedTwo.Beacon.DeviceId,  Is.EqualTo(randomizedDeviceId));
            Assert.That(obtainedThree.Beacon.DeviceId,  Is.EqualTo(randomizedDeviceId));
        }

        [Test]
        public void CreateSessionIncreasesSessionSequenceNumber()
        {
            // given
            var target = CreateSessionCreator();
            ISessionCreator targetExplicit = target;

            Assert.That(target.SessionSequenceNumber,  Is.EqualTo(0));

            // when
            targetExplicit.CreateSession(mockParent);

            // then
            Assert.That(target.SessionSequenceNumber,  Is.EqualTo(1));

            // and when
            targetExplicit.CreateSession(mockParent);

            // then
            Assert.That(target.SessionSequenceNumber,  Is.EqualTo(2));
        }

        private SessionCreator CreateSessionCreator()
        {
            return new SessionCreator(mockInput, "https://localhost");
        }
    }
}