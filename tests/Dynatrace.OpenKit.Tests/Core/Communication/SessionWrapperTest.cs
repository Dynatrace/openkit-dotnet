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
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Communication
{
    public class SessionWrapperTest
    {
        private Session wrappedSession;

        [SetUp]
        public void SetUp()
        {
            var logger = Substitute.For<ILogger>();
            logger.IsDebugEnabled.Returns(true);

            var beaconSendingContext = Substitute.For<IBeaconSendingContext>();
            var beaconSender = new BeaconSender(logger, beaconSendingContext);

            var configuration = new TestConfiguration();
            var beacon = new Beacon(logger,
                new BeaconCache(logger), configuration, "127.0.0.1", Substitute.For<IThreadIdProvider>(), Substitute.For<ITimingProvider>());

            wrappedSession = new Session(logger, beaconSender, beacon);
        }

        [Test]
        public void ByDefaultBeaconConfigurationIsNotSet()
        {
            // wrappedSession
            var target = new SessionWrapper(wrappedSession);

            // then
            Assert.That(target.IsBeaconConfigurationSet, Is.False);
        }

        [Test]
        public void AfterUpdatingTheBeaconConfigurationTheBeaconConfigurationIsSet()
        {
            // given
            var target = new SessionWrapper(wrappedSession);
            var newConfiguration = new BeaconConfiguration(42, DataCollectionLevel.OFF, CrashReportingLevel.OFF);

            // when updating
            target.UpdateBeaconConfiguration(newConfiguration);

            // then
            Assert.That(target.IsBeaconConfigurationSet, Is.True);

            // also verify that Session has been invoked
            Assert.That(wrappedSession.BeaconConfiguration, Is.SameAs(newConfiguration));
        }

        [Test]
        public void ByDefaultTheSessionIsNotFinished()
        {
            // given
            var target = new SessionWrapper(wrappedSession);

            // then
            Assert.That(target.IsSessionFinished, Is.False);
        }

        [Test]
        public void TheSessionIsFinishedAfterCallingFinishSession()
        {
            // given
            var target = new SessionWrapper(wrappedSession);

            // when
            target.FinishSession();

            // then
            Assert.That(target.IsSessionFinished, Is.True);
        }

        [Test]
        public void ADefaultConstructedSessionWrapperCanSendRequests()
        {
            // given
            var target = new SessionWrapper(wrappedSession);

            // then
            Assert.That(target.CanSendNewSessionRequest, Is.True);
        }

        [Test]
        public void AfterDecreasingNumNewSessionRequestsFourTimesSendingRequestsIsNoLongerAllowed()
        {
            // given
            var target = new SessionWrapper(wrappedSession);

            // when decreasing first time
            target.DecreaseNumNewSessionRequests();

            // then sending is still allowed
            Assert.That(target.CanSendNewSessionRequest, Is.True);

            // when decreasing second time
            target.DecreaseNumNewSessionRequests();

            // then sending is still allowed
            Assert.That(target.CanSendNewSessionRequest, Is.True);

            // when decreasing third time
            target.DecreaseNumNewSessionRequests();

            // then sending is still allowed
            Assert.That(target.CanSendNewSessionRequest, Is.True);

            // when decreasing fourth time
            target.DecreaseNumNewSessionRequests();

            // then sending is no longer allowed
            Assert.That(target.CanSendNewSessionRequest, Is.False);
        }

        [Test]
        public void GetBeaconConfigurationCallsWrappedSession()
        {
            // given
            var target = new SessionWrapper(wrappedSession);

            // when, then
            Assert.That(target.BeaconConfiguration, Is.SameAs(wrappedSession.BeaconConfiguration));
        }

        [Test]
        public void ClearCapturedDataClearsDataFromSession()
        {
            // given
            var target = new SessionWrapper(wrappedSession);
            wrappedSession.EnterAction("foo").LeaveAction();

            // ensure data is not empty
            Assert.That(wrappedSession.IsEmpty, Is.False);

            // when
            target.ClearCapturedData();

            // verify forwarded calls - now wrapped session must be empty
            Assert.That(wrappedSession.IsEmpty, Is.True);
        }

        [Test]
        public void IsEmptyCallsWrappedSession()
        {
            // given
            var target = new SessionWrapper(wrappedSession);
            target.ClearCapturedData();

            // when, then
            Assert.That(wrappedSession.IsEmpty, Is.True);
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void EndCallsWrappedSession()
        {
            // given
            var target = new SessionWrapper(wrappedSession);

            // ensure that session is not ended
            Assert.That(wrappedSession.IsSessionEnded, Is.False);

            // when
            target.End();

            // verify forwarded calls
            Assert.That(wrappedSession.IsSessionEnded, Is.True);
        }

        [Test]
        public void GetSessionReturnsWrappedSession()
        {
            // given
            var target = new SessionWrapper(wrappedSession);

            // when
            var obtained = target.Session;

            // then
            Assert.That(obtained, Is.SameAs(wrappedSession));
        }

        [Test]
        public void WhenBeaconConfigurationIsNotSetSendingIsNotAllowed()
        {
            // given
            var target = new SessionWrapper(wrappedSession);

            // when, then
            Assert.That(target.IsDataSendingAllowed, Is.False);
        }

        [Test]
        public void WhenBeaconConfigurationIsSetSendingIsAllowedIfMultiplicityIsGreaterThanZero()
        {
            // given
            var target = new SessionWrapper(wrappedSession);

            // when
            target.UpdateBeaconConfiguration(new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF));

            // then
            Assert.That(target.IsDataSendingAllowed, Is.True);
        }

        [Test]
        public void WhenBeaconConfigurationIsSetSendingIsDisallowedIfMultiplicityIsZero()
        {
            // given
            var target = new SessionWrapper(wrappedSession);

            // when
            target.UpdateBeaconConfiguration(new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OFF));

            // then
            Assert.That(target.IsDataSendingAllowed, Is.False);
        }

        [Test]
        public void WhenBeaconConfigurationIsSetSendingIsDisallowedIfMultiplicityIsLessThanZero()
        {
            // given
            var target = new SessionWrapper(wrappedSession);

            // when
            target.UpdateBeaconConfiguration(new BeaconConfiguration(-1, DataCollectionLevel.OFF, CrashReportingLevel.OFF));

            // then
            Assert.That(target.IsDataSendingAllowed, Is.False);
        }
    }
}

