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

using Dynatrace.OpenKit.Protocol;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class ServerConfigurationTest
    {
        private readonly IResponseAttributes defaultValues = ResponseAttributesDefaults.Undefined;
        private IResponseAttributes mockAttributes;

        private IServerConfiguration mockServerConfig;

        [SetUp]
        public void SetUp()
        {
            mockAttributes = Substitute.For<IResponseAttributes>();
            mockAttributes.IsCapture.Returns(defaultValues.IsCapture);
            mockAttributes.IsCaptureCrashes.Returns(defaultValues.IsCaptureCrashes);
            mockAttributes.IsCaptureErrors.Returns(defaultValues.IsCaptureErrors);
            mockAttributes.SendIntervalInMilliseconds.Returns(defaultValues.SendIntervalInMilliseconds);
            mockAttributes.ServerId.Returns(defaultValues.ServerId);
            mockAttributes.MaxBeaconSizeInBytes.Returns(defaultValues.MaxBeaconSizeInBytes);
            mockAttributes.Multiplicity.Returns(defaultValues.Multiplicity);
            mockAttributes.SendIntervalInMilliseconds.Returns(defaultValues.SendIntervalInMilliseconds);
            mockAttributes.MaxSessionDurationInMilliseconds.Returns(defaultValues.MaxSessionDurationInMilliseconds);
            mockAttributes.MaxEventsPerSession.Returns(defaultValues.MaxEventsPerSession);
            mockAttributes.SessionTimeoutInMilliseconds.Returns(defaultValues.SessionTimeoutInMilliseconds);
            mockAttributes.VisitStoreVersion.Returns(defaultValues.VisitStoreVersion);

            mockServerConfig = Substitute.For<IServerConfiguration>();
            mockServerConfig.IsCaptureEnabled.Returns(defaultValues.IsCapture);
            mockServerConfig.IsCrashReportingEnabled.Returns(defaultValues.IsCaptureCrashes);
            mockServerConfig.IsErrorReportingEnabled.Returns(defaultValues.IsCaptureErrors);
            mockServerConfig.ServerId.Returns(defaultValues.ServerId);
            mockServerConfig.BeaconSizeInBytes.Returns(defaultValues.MaxBeaconSizeInBytes);
            mockServerConfig.Multiplicity.Returns(defaultValues.Multiplicity);
            mockServerConfig.SendIntervalInMilliseconds.Returns(defaultValues.SendIntervalInMilliseconds);
            mockServerConfig.MaxSessionDurationInMilliseconds.Returns(defaultValues.MaxSessionDurationInMilliseconds);
            mockServerConfig.MaxEventsPerSession.Returns(defaultValues.MaxEventsPerSession);
            mockServerConfig.IsSessionSplitByEventsEnabled.Returns(false);
            mockServerConfig.SessionTimeoutInMilliseconds.Returns(defaultValues.SessionTimeoutInMilliseconds);
            mockServerConfig.VisitStoreVersion.Returns(defaultValues.VisitStoreVersion);
        }

        #region test defaults

        [Test]
        public void InDefaultServerConfigurationCapturingIsEnabled()
        {
            Assert.That(ServerConfiguration.Default.IsCaptureEnabled, Is.True);
        }

        [Test]
        public void InDefaultServerConfigurationCrashReportingIsEnabled()
        {
            Assert.That(ServerConfiguration.Default.IsCrashReportingEnabled, Is.True);
        }

        [Test]
        public void InDefaultServerConfigurationErrorReportingIsEnabled()
        {
            Assert.That(ServerConfiguration.Default.IsErrorReportingEnabled, Is.True);
        }

        [Test]
        public void InDefaultServerConfigurationServerIdIsMinusOne()
        {
            Assert.That(ServerConfiguration.Default.ServerId, Is.EqualTo(-1));
        }

        [Test]
        public void InDefaultServerConfigurationBeaconSizeIsThirtyKb()
        {
            Assert.That(ServerConfiguration.Default.BeaconSizeInBytes, Is.EqualTo(30 * 1024));
        }

        [Test]
        public void InDefaultServerConfigurationMultiplicityIsOne()
        {
            Assert.That(ServerConfiguration.Default.Multiplicity, Is.EqualTo(1));
        }

        [Test]
        public void InDefaultServerConfigurationSendIntervalIs120Seconds()
        {
            Assert.That(ServerConfiguration.Default.SendIntervalInMilliseconds, Is.EqualTo(120 * 1000));
        }

        [Test]
        public void InDefaultServerConfigurationMaxSessionDurationIsMinusOne()
        {
            Assert.That(ServerConfiguration.Default.MaxSessionDurationInMilliseconds, Is.EqualTo(-1));
        }

        [Test]
        public void InDefaultServerConfigurationIsSessionSplitBySessionDurationEnabledIsFalse()
        {
            Assert.That(ServerConfiguration.Default.IsSessionSplitBySessionDurationEnabled, Is.False);
        }

        [Test]
        public void InDefaultServerConfigurationMaxEventsPerSessionIsMinusOne()
        {
            Assert.That(ServerConfiguration.Default.MaxEventsPerSession, Is.EqualTo(-1));
        }

        [Test]
        public void InDefaultServerConfigurationIsSessionSplitByEventsEnabledIsFalse()
        {
            Assert.That(ServerConfiguration.Default.IsSessionSplitByEventsEnabled, Is.False);
        }

        [Test]
        public void InDefaultServerConfigurationSessionTimeoutIsMinusOne()
        {
            Assert.That(ServerConfiguration.Default.SessionTimeoutInMilliseconds, Is.EqualTo(-1));
        }

        [Test]
        public void InDefaultServerConfigurationIsSessionSplitByIdleTimeoutEnabledIsFalse()
        {
            Assert.That(ServerConfiguration.Default.IsSessionSplitByIdleTimeoutEnabled, Is.False);
        }

        [Test]
        public void InDefaultServerConfigurationVisitStoreVersionIsOne()
        {
            Assert.That(ServerConfiguration.Default.VisitStoreVersion, Is.EqualTo(1));
        }

        #endregion

        #region test creation with 'From' method

        [Test]
        public void CreatingAServerConfigurationFromNullResponseAttributesGivesNull()
        {
            Assert.That(ServerConfiguration.From(null), Is.Null);
        }

        [Test]
        public void CreatingAServerConfigurationFromResponseAttributesCopiesCaptureSettings()
        {
            // given, when
            mockAttributes.IsCapture.Returns(false);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsCaptureEnabled, Is.False);
            _ = mockAttributes.Received(1).IsCapture;
        }

        [Test]
        public void CreatingAServerConfigurationFromResponseAttributesCopiesCrashReportingSettings()
        {
            // given, when
            mockAttributes.IsCaptureCrashes.Returns(false);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsCrashReportingEnabled, Is.False);
            _ = mockAttributes.Received(1).IsCaptureCrashes;
        }

        [Test]
        public void CreatingAServerConfigurationFromResponseAttributesCopiesErrorReportingSettings()
        {
            // given, when
            mockAttributes.IsCaptureErrors.Returns(false);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsErrorReportingEnabled, Is.False);
            _ = mockAttributes.Received(1).IsCaptureErrors;
        }

        [Test]
        public void CreatingAServerConfigurationFromResponseAttributesCopiesServerIdSettings()
        {
            // given, when
            const int serverId = 73;
            mockAttributes.ServerId.Returns(serverId);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.ServerId, Is.EqualTo(serverId));
            _ = mockAttributes.Received(1).ServerId;
        }

        [Test]
        public void CreatingAServerConfigurationFromResponseAttributesCopiesBeaconSizeSettings()
        {
            // given, when
            const int beaconSize = 37;
            mockAttributes.MaxBeaconSizeInBytes.Returns(beaconSize);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.BeaconSizeInBytes, Is.EqualTo(beaconSize));
            _ = mockAttributes.Received(1).MaxBeaconSizeInBytes;
        }

        [Test]
        public void CreatingAServerConfigurationFromResponseAttributesCopiesMultiplicitySettings()
        {
            // given, when
            const int multiplicity = 42;
            mockAttributes.Multiplicity.Returns(multiplicity);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.Multiplicity, Is.EqualTo(multiplicity));
            _ = mockAttributes.Received(1).Multiplicity;
        }

        [Test]
        public void CreatingAServerConfigurationFromResponseAttributesCopiesSendInterval()
        {
            // given, when
            const int sendInterval = 1234;
            mockAttributes.SendIntervalInMilliseconds.Returns(sendInterval);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.SendIntervalInMilliseconds, Is.EqualTo(sendInterval));
            _ = mockAttributes.Received(1).SendIntervalInMilliseconds;
        }

        [Test]
        public void CreatingAServerConfigurationFromResponseAttributesCopiesSessionDuration()
        {
            // given
            const int sessionDuration = 73;
            mockAttributes.MaxSessionDurationInMilliseconds.Returns(sessionDuration);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.MaxSessionDurationInMilliseconds, Is.EqualTo(sessionDuration));
            _ = mockAttributes.Received(1).MaxSessionDurationInMilliseconds;
        }

        [Test]
        public void
            CreatingAServerConfigurationFromResponseAttributesHasSplitBySessionDurationEnabledIfMaxSessionDurationGreaterZero()
        {
            // given
            const int sessionDuration = 1;
            mockAttributes.MaxSessionDurationInMilliseconds.Returns(sessionDuration);
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_SESSION_DURATION).Returns(true);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsSessionSplitBySessionDurationEnabled, Is.True);
            _ = mockAttributes.Received(1).MaxSessionDurationInMilliseconds;
            mockAttributes.Received(1).IsAttributeSet(ResponseAttribute.MAX_SESSION_DURATION);
        }

        [Test]
        public void CreatingAServerConfigurationStatusResponseHasSplitBySessionDurationDisabledIfMaxDurationZero()
        {
            // given
            const int sessionDuration = 0;
            mockAttributes.MaxSessionDurationInMilliseconds.Returns(sessionDuration);
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_SESSION_DURATION).Returns(true);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsSessionSplitBySessionDurationEnabled, Is.False);
            _ = mockAttributes.Received(1).MaxSessionDurationInMilliseconds;
            mockAttributes.Received(1).IsAttributeSet(ResponseAttribute.MAX_SESSION_DURATION);
        }

        [Test]
        public void
            CreatingAServerConfigurationStatusResponseHasSplitBySessionDurationDisabledIfMaxDurationEventsSmallerZero()
        {
            // given
            const int sessionDuration = -1;
            mockAttributes.MaxSessionDurationInMilliseconds.Returns(sessionDuration);
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_SESSION_DURATION).Returns(true);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsSessionSplitBySessionDurationEnabled, Is.False);
            _ = mockAttributes.Received(1).MaxSessionDurationInMilliseconds;
            mockAttributes.Received(1).IsAttributeSet(ResponseAttribute.MAX_SESSION_DURATION);
        }

        [Test]
        public void CreatingAServerConfigurationStatusResponseHasSplitBySessionDurationDisabledIfMaxDurationIsNotSet()
        {
            // given
            const int sessionDuration = 1;
            mockAttributes.MaxSessionDurationInMilliseconds.Returns(sessionDuration);
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_SESSION_DURATION).Returns(false);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsSessionSplitBySessionDurationEnabled, Is.False);
            _ = mockAttributes.Received(1).MaxSessionDurationInMilliseconds;
            mockAttributes.Received(1).IsAttributeSet(ResponseAttribute.MAX_SESSION_DURATION);
        }

        [Test]
        public void CreatingAServerConfigurationFromResponseAttributesCopiesMaxEventsPerSession()
        {
            // given
            const int eventsPerSession = 37;
            mockAttributes.MaxEventsPerSession.Returns(eventsPerSession);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.MaxEventsPerSession, Is.EqualTo(eventsPerSession));
            _ = mockAttributes.Received(1).MaxEventsPerSession;
        }

        [Test]
        public void CreatingAServerConfigurationFromResponseAttributesHasSplitByEventsEnabledIfMaxEventsGreaterZero()
        {
            // given
            const int eventsPerSession = 1;
            mockAttributes.MaxEventsPerSession.Returns(eventsPerSession);
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION).Returns(true);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsSessionSplitByEventsEnabled, Is.True);
            _ = mockAttributes.Received(1).MaxEventsPerSession;
            mockAttributes.Received(1).IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION);
        }

        [Test]
        public void CreatingAServerConfigurationFromResponseAttributesHasSplitByEventsDisabledIfMaxEventsZero()
        {
            // given
            const int eventsPerSession = 0;
            mockAttributes.MaxEventsPerSession.Returns(eventsPerSession);
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION).Returns(true);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsSessionSplitByEventsEnabled, Is.False);
            _ = mockAttributes.Received(1).MaxEventsPerSession;
            mockAttributes.Received(1).IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION);
        }

        [Test]
        public void CreatingAServerConfigurationFromResponseAttributesHasSplitByEventsDisabledIfMaxEventsEventsSmallerZero()
        {
            // given
            const int eventsPerSession = -1;
            mockAttributes.MaxEventsPerSession.Returns(eventsPerSession);
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION).Returns(true);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsSessionSplitByEventsEnabled, Is.False);
            _ = mockAttributes.Received(1).MaxEventsPerSession;
            mockAttributes.Received(1).IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION);
        }

        [Test]
        public void CreatingAServerConfigurationFromResponseAttributesHasSplitByEventsDisabledIfMaxEventsIsNotSet()
        {
            // given
            const int eventsPerSession = 1;
            mockAttributes.MaxEventsPerSession.Returns(eventsPerSession);
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION).Returns(false);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsSessionSplitByEventsEnabled, Is.False);
            _ = mockAttributes.Received(1).MaxEventsPerSession;
            mockAttributes.Received(1).IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION);
        }

        [Test]
        public void CreatingAServerConfigurationFromResponseAttributesCopiesSessionTimeout()
        {
            // given
            const int sessionTimeout = 42;
            mockAttributes.SessionTimeoutInMilliseconds.Returns(sessionTimeout);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.SessionTimeoutInMilliseconds, Is.EqualTo(sessionTimeout));
            _ = mockAttributes.Received(1).SessionTimeoutInMilliseconds;
        }

        [Test]
        public void CreatingAServerConfigurationFromResponseAttributesHasSplitByIdleTimeoutEnabledIfTimeoutGreaterZero()
        {
            // given
            const int idleTimeout = 1;
            mockAttributes.SessionTimeoutInMilliseconds.Returns(idleTimeout);
            mockAttributes.IsAttributeSet(ResponseAttribute.SESSION_IDLE_TIMEOUT).Returns(true);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsSessionSplitByIdleTimeoutEnabled, Is.True);
            _ = mockAttributes.Received(1).SessionTimeoutInMilliseconds;
            mockAttributes.Received(1).IsAttributeSet(ResponseAttribute.SESSION_IDLE_TIMEOUT);
        }

        [Test]
        public void CreatingAServerConfigurationStatusResponseHasSplitByIdleTimeoutDisabledIfTimeoutZero()
        {
            // given
            const int idleTimeout = 0;
            mockAttributes.SessionTimeoutInMilliseconds.Returns(idleTimeout);
            mockAttributes.IsAttributeSet(ResponseAttribute.SESSION_IDLE_TIMEOUT).Returns(true);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsSessionSplitByIdleTimeoutEnabled, Is.False);
            _ = mockAttributes.Received(1).SessionTimeoutInMilliseconds;
            mockAttributes.Received(1).IsAttributeSet(ResponseAttribute.SESSION_IDLE_TIMEOUT);
        }

        [Test]
        public void CreatingAServerConfigurationStatusResponseHasSplitByIdleTimeoutDisabledIfTimeoutSmallerZero()
        {
            // given
            const int idleTimeout = -1;
            mockAttributes.SessionTimeoutInMilliseconds.Returns(idleTimeout);
            mockAttributes.IsAttributeSet(ResponseAttribute.SESSION_IDLE_TIMEOUT).Returns(true);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsSessionSplitByIdleTimeoutEnabled, Is.False);
            _ = mockAttributes.Received(1).SessionTimeoutInMilliseconds;
            mockAttributes.Received(1).IsAttributeSet(ResponseAttribute.SESSION_IDLE_TIMEOUT);
        }

        [Test]
        public void CreatingAServerConfigurationStatusResponseHasSplitByIdleTimeoutDisabledIfTimeoutIsNotSet()
        {
            // given
            const int idleTimeout = 1;
            mockAttributes.SessionTimeoutInMilliseconds.Returns(idleTimeout);
            mockAttributes.IsAttributeSet(ResponseAttribute.SESSION_IDLE_TIMEOUT).Returns(false);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsSessionSplitByIdleTimeoutEnabled, Is.False);
            _ = mockAttributes.Received(1).SessionTimeoutInMilliseconds;
            mockAttributes.Received(1).IsAttributeSet(ResponseAttribute.SESSION_IDLE_TIMEOUT);
        }

        [Test]
        public void CreatingASessionConfigurationFromResponseAttributesCopiesVisitStoreVersion()
        {
            // given
            const int visitStoreVersion = 73;
            mockAttributes.VisitStoreVersion.Returns(visitStoreVersion);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.VisitStoreVersion, Is.EqualTo(visitStoreVersion));
            _ = mockAttributes.Received(1).VisitStoreVersion;
        }

        [Test]
        public void SendingDataToTheServerIsAllowedIfCapturingIsEnabledAndMultiplicityIsGreaterThanZero()
        {
            // given
            mockAttributes.IsCapture.Returns(true);
            mockAttributes.Multiplicity.Returns(1);
            var target = ServerConfiguration.From(mockAttributes);

            // when
            var obtained = target.IsSendingDataAllowed;

            // then
            Assert.That(obtained, Is.True);
        }

        [Test]
        public void SendingDataToTheServerIsNotAllowedIfCapturingIsDisabled()
        {
            // given
            mockAttributes.IsCapture.Returns(false);
            mockAttributes.Multiplicity.Returns(1);
            var target = ServerConfiguration.From(mockAttributes);

            // when
            var obtained = target.IsSendingDataAllowed;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void SendingDataToTheServerIsNotAllowedIfCapturingIsEnabledButMultiplicityIsZero()
        {
            // given
            mockAttributes.IsCapture.Returns(false);
            mockAttributes.Multiplicity.Returns(0);
            var target = ServerConfiguration.From(mockAttributes);

            // when
            var obtained = target.IsSendingDataAllowed;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void SendingCrashesToTheServerIsAllowedIfDataSendingIsAllowedAndCaptureCrashesIsEnabled()
        {
            // given
            mockAttributes.IsCapture.Returns(true);
            mockAttributes.Multiplicity.Returns(1);
            mockAttributes.IsCaptureCrashes.Returns(true);
            var target = ServerConfiguration.From(mockAttributes);

            // when
            var obtained = target.IsSendingCrashesAllowed;

            // then
            Assert.That(obtained, Is.True);
        }

        [Test]
        public void SendingCrashesToTheServerIsNotAllowedIfDataSendingIsNotAllowed()
        {
            // given
            mockAttributes.IsCapture.Returns(false);
            mockAttributes.Multiplicity.Returns(1);
            mockAttributes.IsCaptureCrashes.Returns(true);
            var target = ServerConfiguration.From(mockAttributes);

            // when
            var obtained = target.IsSendingCrashesAllowed;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void SendingCrashesToTheServerIsNotAllowedIfDataSendingIsAllowedButCaptureCrashesIsDisabled()
        {
            // given
            mockAttributes.IsCapture.Returns(true);
            mockAttributes.Multiplicity.Returns(1);
            mockAttributes.IsCaptureCrashes.Returns(false);
            var target = ServerConfiguration.From(mockAttributes);

            // when
            var obtained = target.IsSendingCrashesAllowed;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void SendingErrorToTheServerIsAllowedIfDataSendingIsAllowedAndCaptureErrorIsEnabled()
        {
            // given
            mockAttributes.IsCapture.Returns(true);
            mockAttributes.Multiplicity.Returns(1);
            mockAttributes.IsCaptureErrors.Returns(true);
            var target = ServerConfiguration.From(mockAttributes);

            // when
            var obtained = target.IsSendingErrorsAllowed;

            // then
            Assert.That(obtained, Is.True);
        }

        [Test]
        public void SendingErrorToTheServerIsNotAllowedIfDataSendingIsNotAllowed()
        {
            // given
            mockAttributes.IsCapture.Returns(false);
            mockAttributes.Multiplicity.Returns(1);
            mockAttributes.IsCaptureErrors.Returns(true);
            var target = ServerConfiguration.From(mockAttributes);

            // when
            var obtained = target.IsSendingErrorsAllowed;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void SendingErrorsToTheServerIsNotAllowedIfDataSendingIsAllowedButCaptureErrorsDisabled()
        {
            // given
            mockAttributes.IsCapture.Returns(true);
            mockAttributes.Multiplicity.Returns(1);
            mockAttributes.IsCaptureErrors.Returns(false);
            var target = ServerConfiguration.From(mockAttributes);

            // when
            var obtained = target.IsSendingErrorsAllowed;

            // then
            Assert.That(obtained, Is.False);
        }

        #endregion

        #region creating builder from server config

        [Test]
        public void BuilderFromServerConfigCopiesCaptureSettings()
        {
            // given
            mockServerConfig.IsCaptureEnabled.Returns(false);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.IsCaptureEnabled, Is.False);
            _ = mockServerConfig.Received(1).IsCaptureEnabled;
        }

        [Test]
        public void BuilderFromServerConfigCopiesCrashReportingSettings()
        {
            // given
            mockServerConfig.IsCrashReportingEnabled.Returns(false);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.IsCrashReportingEnabled, Is.False);
            _ = mockServerConfig.Received(1).IsCrashReportingEnabled;
        }

        [Test]
        public void BuilderFromServerConfigCopiesErrorReportingSettings()
        {
            // given
            mockServerConfig.IsErrorReportingEnabled.Returns(false);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.IsErrorReportingEnabled, Is.False);
            _ = mockServerConfig.Received(1).IsErrorReportingEnabled;
        }

        [Test]
        public void BuilderFromServerConfigCopiesServerIdSettings()
        {
            // given
            mockServerConfig.ServerId.Returns(42);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.ServerId, Is.EqualTo(42));
            _ = mockServerConfig.Received(1).ServerId;
        }

        [Test]
        public void BuilderFromServerConfigCopiesBeaconSizeSettings()
        {
            // given
            mockServerConfig.BeaconSizeInBytes.Returns(100 * 1024);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.BeaconSizeInBytes, Is.EqualTo(100 * 1024));
            _ = mockServerConfig.Received(1).BeaconSizeInBytes;
        }

        [Test]
        public void BuilderFromServerConfigCopiesMultiplicitySettings()
        {
            // given
            mockServerConfig.Multiplicity.Returns(7);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.Multiplicity, Is.EqualTo(7));
            _ = mockServerConfig.Received(1).Multiplicity;
        }

        [Test]
        public void BuilderFromServerConfigCopiesSendInterval()
        {
            // given
            const int sendInterval = 4321;
            mockServerConfig.SendIntervalInMilliseconds.Returns(sendInterval);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.SendIntervalInMilliseconds, Is.EqualTo(sendInterval));
            _ = mockServerConfig.Received(1).SendIntervalInMilliseconds;
        }

        [Test]
        public void BuilderFromServerConfigCopiesSessionDuration()
        {
            // given
            const int sessionDuration = 73;
            mockServerConfig.MaxSessionDurationInMilliseconds.Returns(sessionDuration);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.MaxSessionDurationInMilliseconds, Is.EqualTo(sessionDuration));
            _ = mockServerConfig.Received(1).MaxSessionDurationInMilliseconds;
        }

        [Test]
        public void BuilderFromServerConfigHasSplitBySessionDurationEnabledIfMaxEventsGreaterZero()
        {
            // given
            const int sessionDuration = 1;
            mockServerConfig.MaxSessionDurationInMilliseconds.Returns(sessionDuration);
            mockServerConfig.IsSessionSplitBySessionDurationEnabled.Returns(true);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.IsSessionSplitBySessionDurationEnabled, Is.True);
        }

        [Test]
        public void BuilderFromServerConfigHasSplitBySessionDurationDisabledIfMaxEventsZero()
        {
            // given
            const int sessionDuration = 0;
            mockServerConfig.MaxSessionDurationInMilliseconds.Returns(sessionDuration);
            mockServerConfig.IsSessionSplitBySessionDurationEnabled.Returns(true);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.IsSessionSplitBySessionDurationEnabled, Is.False);
        }

        [Test]
        public void BuilderFromServerConfigHasSplitBySessionDurationDisabledIfMaxEventsEventsSmallerZero()
        {
            // given
            const int sessionDuration = -1;
            mockServerConfig.MaxSessionDurationInMilliseconds.Returns(sessionDuration);
            mockServerConfig.IsSessionSplitBySessionDurationEnabled.Returns(true);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.IsSessionSplitBySessionDurationEnabled, Is.False);
        }

        [Test]
        public void BuilderFromServerConfigHasSplitBySessionDurationDisabledIfMaxEventsIsNotSet()
        {
            // given
            const int sessionDuration = 1;
            mockServerConfig.MaxSessionDurationInMilliseconds.Returns(sessionDuration);
            mockServerConfig.IsSessionSplitBySessionDurationEnabled.Returns(false);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.IsSessionSplitBySessionDurationEnabled, Is.False);
        }

        [Test]
        public void BuilderFromServerConfigCopiesMaxEventsPerSession()
        {
            // given
            const int eventsPerSession = 37;
            mockServerConfig.MaxEventsPerSession.Returns(eventsPerSession);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.MaxEventsPerSession, Is.EqualTo(eventsPerSession));
            _ = mockServerConfig.Received(1).MaxEventsPerSession;
        }

        [Test]
        public void BuilderFromServerConfigHasSplitByEventsEnabledIfMaxEventsGreaterZero()
        {
            // given
            const int eventsPerSession = 1;
            mockServerConfig.MaxEventsPerSession.Returns(eventsPerSession);
            mockServerConfig.IsSessionSplitByEventsEnabled.Returns(true);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.IsSessionSplitByEventsEnabled, Is.True);
        }

        [Test]
        public void BuilderFromServerConfigHasSplitByEventsDisabledIfMaxEventsZero()
        {
            // given
            const int eventsPerSession = 0;
            mockServerConfig.MaxEventsPerSession.Returns(eventsPerSession);
            mockServerConfig.IsSessionSplitByEventsEnabled.Returns(true);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.IsSessionSplitByEventsEnabled, Is.False);
        }

        [Test]
        public void BuilderFromServerConfigHasSplitByEventsDisabledIfMaxEventsSmallerZero()
        {
            // given
            const int eventsPerSession = -1;
            mockServerConfig.MaxEventsPerSession.Returns(eventsPerSession);
            mockServerConfig.IsSessionSplitByEventsEnabled.Returns(true);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.IsSessionSplitByEventsEnabled, Is.False);
        }

        [Test]
        public void BuilderFromServerConfigHasSplitByEventsDisabledIfMaxEventsIsNotSet()
        {
            // given
            const int eventsPerSession = 1;
            mockServerConfig.MaxEventsPerSession.Returns(eventsPerSession);
            mockServerConfig.IsSessionSplitByEventsEnabled.Returns(false);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.IsSessionSplitByEventsEnabled, Is.False);
        }

        [Test]
        public void BuilderFromServerConfigCopiesSessionTimeout()
        {
            // given
            const int sessionTimeout = 42;
            mockServerConfig.SessionTimeoutInMilliseconds.Returns(sessionTimeout);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.SessionTimeoutInMilliseconds, Is.EqualTo(sessionTimeout));
            _ = mockServerConfig.Received(1).SessionTimeoutInMilliseconds;
        }

        [Test]
        public void BuilderFromServerConfigHasSplitByIdleTimeoutEnabledIfMaxEventsGreaterZero()
        {
            // given
            const int idleTimeout = 1;
            mockServerConfig.SessionTimeoutInMilliseconds.Returns(idleTimeout);
            mockServerConfig.IsSessionSplitByIdleTimeoutEnabled.Returns(true);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.IsSessionSplitByIdleTimeoutEnabled, Is.True);
        }

        [Test]
        public void BuilderFromServerConfigHasSplitByIdleTimeoutDisabledIfMaxEventsZero()
        {
            // given
            const int idleTimeout = 0;
            mockServerConfig.SessionTimeoutInMilliseconds.Returns(idleTimeout);
            mockServerConfig.IsSessionSplitByIdleTimeoutEnabled.Returns(true);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.IsSessionSplitByIdleTimeoutEnabled, Is.False);
        }

        [Test]
        public void BuilderFromServerConfigHasSplitByIdleTimeoutDisabledIfMaxEventsEventsSmallerZero()
        {
            // given
            const int idleTimeout = -1;
            mockServerConfig.SessionTimeoutInMilliseconds.Returns(idleTimeout);
            mockServerConfig.IsSessionSplitByIdleTimeoutEnabled.Returns(true);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.IsSessionSplitByIdleTimeoutEnabled, Is.False);
        }

        [Test]
        public void BuilderFromServerConfigHasSplitByIdleTimeoutDisabledIfMaxEventsIsNotSet()
        {
            // given
            const int idleTimeout = 1;
            mockServerConfig.SessionTimeoutInMilliseconds.Returns(idleTimeout);
            mockServerConfig.IsSessionSplitByIdleTimeoutEnabled.Returns(false);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.IsSessionSplitByIdleTimeoutEnabled, Is.False);
        }

        [Test]
        public void BuilderFromServerConfigCopiesVisitStoreVersion()
        {
            // given
            const int visitStoreVersion = 73;
            mockServerConfig.VisitStoreVersion.Returns(visitStoreVersion);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.VisitStoreVersion, Is.EqualTo(visitStoreVersion));
            _ = mockServerConfig.Received(1).VisitStoreVersion;
        }

        [Test]
        public void
            BuilderFromServerConfigSendingDataToTheServerIsAllowedIfCapturingIsEnabledAndMultiplicityIsGreaterThanZero()
        {
            // given
            mockServerConfig.IsCaptureEnabled.Returns(true);
            mockServerConfig.Multiplicity.Returns(1);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // when
            var obtained = target.IsSendingDataAllowed;

            // then
            Assert.That(obtained, Is.True);
        }

        [Test]
        public void BuilderFromServerConfigSendingDataToTheServerIsNotAllowedIfCapturingIsDisabled()
        {
            // given
            mockServerConfig.IsCaptureEnabled.Returns(false);
            mockServerConfig.Multiplicity.Returns(1);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // when
            var obtained = target.IsSendingDataAllowed;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void BuilderFromServerConfigSendingDataToTheServerIsNotAllowedIfCapturingIsEnabledButMultiplicityIsZero()
        {
            // given
            mockServerConfig.IsCaptureEnabled.Returns(true);
            mockServerConfig.Multiplicity.Returns(0);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // when
            var obtained = target.IsSendingDataAllowed;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void
            BuilderFromServerConfigSendingCrashesToTheServerIsAllowedIfDataSendingIsAllowedAndCaptureCrashesIsEnabled()
        {
            // given
            mockServerConfig.IsCaptureEnabled.Returns(true);
            mockServerConfig.Multiplicity.Returns(1);
            mockServerConfig.IsCrashReportingEnabled.Returns(true);

            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // when
            var obtained = target.IsSendingCrashesAllowed;

            // then
            Assert.That(obtained, Is.True);
        }

        [Test]
        public void BuilderFromServerConfigSendingCrashesToTheServerIsNotAllowedIfDataSendingIsNotAllowed()
        {
            // given
            mockServerConfig.IsCaptureEnabled.Returns(false);
            mockServerConfig.Multiplicity.Returns(1);
            mockServerConfig.IsCrashReportingEnabled.Returns(true);

            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // when
            var obtained = target.IsSendingCrashesAllowed;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void
            BuilderFromServerConfigSendingCrashesToTheServerIsNotAllowedIfDataSendingIsAllowedButCaptureCrashesIsDisabled()
        {
            // given
            mockServerConfig.IsCaptureEnabled.Returns(true);
            mockServerConfig.Multiplicity.Returns(1);
            mockServerConfig.IsCrashReportingEnabled.Returns(false);

            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // when
            var obtained = target.IsSendingCrashesAllowed;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void
            BuilderFromServerConfigSendingErrorToTheServerIsAllowedIfDataSendingIsAllowedAndCaptureErrorIsEnabled()
        {
            // given
            mockServerConfig.IsCaptureEnabled.Returns(true);
            mockServerConfig.Multiplicity.Returns(1);
            mockServerConfig.IsErrorReportingEnabled.Returns(true);

            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // when
            var obtained = target.IsSendingErrorsAllowed;

            // then
            Assert.That(obtained, Is.True);
        }

        [Test]
        public void BuilderFromServerConfigSendingErrorToTheServerIsNotAllowedIfDataSendingIsNotAllowed()
        {
            // given
            mockServerConfig.IsCaptureEnabled.Returns(false);
            mockServerConfig.Multiplicity.Returns(1);
            mockServerConfig.IsErrorReportingEnabled.Returns(true);

            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // when
            var obtained = target.IsSendingErrorsAllowed;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void
            BuilderFromServerConfigSendingErrorsToTheServerIsNotAllowedIfDataSendingIsAllowedButCaptureErrorsDisabled()
        {
            // given
            mockServerConfig.IsCaptureEnabled.Returns(true);
            mockServerConfig.Multiplicity.Returns(1);
            mockServerConfig.IsErrorReportingEnabled.Returns(false);

            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // when
            var obtained = target.IsSendingErrorsAllowed;

            // then
            Assert.That(obtained, Is.False);
        }

        #endregion

        #region test 'Merge' method

        [Test]
        public void MergeTakesOverEnabledCapture()
        {
            // given
            var target = new ServerConfiguration.Builder(defaultValues).WithCapture(false).Build();
            var other = new ServerConfiguration.Builder(defaultValues).WithCapture(true).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsCaptureEnabled, Is.True);
        }

        [Test]
        public void MergeTakesOverDisabledCapture()
        {
            // given
            var target = new ServerConfiguration.Builder(defaultValues).WithCapture(true).Build();
            var other = new ServerConfiguration.Builder(defaultValues).WithCapture(false).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsCaptureEnabled, Is.False);
        }

        [Test]
        public void MergeTakesOverEnabledCrashReporting()
        {
            // given
            var target = new ServerConfiguration.Builder(defaultValues).WithCrashReporting(false).Build();
            var other = new ServerConfiguration.Builder(defaultValues).WithCrashReporting(true).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsCrashReportingEnabled, Is.True);
        }

        [Test]
        public void MergeTakesOverDisabledCrashReporting()
        {
            // given
            var target = new ServerConfiguration.Builder(defaultValues).WithCrashReporting(true).Build();
            var other = new ServerConfiguration.Builder(defaultValues).WithCrashReporting(false).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsCrashReportingEnabled, Is.False);
        }

        [Test]
        public void MergeTakesOverEnabledErrorReporting()
        {
            // given
            var target = new ServerConfiguration.Builder(defaultValues).WithErrorReporting(false).Build();
            var other = new ServerConfiguration.Builder(defaultValues).WithErrorReporting(true).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsErrorReportingEnabled, Is.True);
        }

        [Test]
        public void MergeTakesOverDisabledErrorReporting()
        {
            // given
            var target = new ServerConfiguration.Builder(defaultValues).WithErrorReporting(true).Build();
            var other = new ServerConfiguration.Builder(defaultValues).WithErrorReporting(false).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsErrorReportingEnabled, Is.False);
        }

        [Test]
        public void MergeTakesOverBeaconSize()
        {
            // given
            const int beaconSize = 73;
            var target = new ServerConfiguration.Builder(defaultValues).WithBeaconSizeInBytes(37).Build();
            var other = new ServerConfiguration.Builder(defaultValues).WithBeaconSizeInBytes(beaconSize).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.BeaconSizeInBytes, Is.EqualTo(beaconSize));
        }

        [Test]
        public void MergeIgnoresMultiplicity()
        {
            // given
            const int multiplicity = 73;
            var target = new ServerConfiguration.Builder(defaultValues).WithMultiplicity(multiplicity).Build();
            var other = new ServerConfiguration.Builder(defaultValues).WithMultiplicity(37).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.Multiplicity, Is.EqualTo(multiplicity));
        }

        [Test]
        public void MergeIgnoresServerId()
        {
            // given
            const int serverId = 73;
            var target = new ServerConfiguration.Builder(defaultValues).WithServerId(serverId).Build();
            var other = new ServerConfiguration.Builder(defaultValues).WithServerId(37).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.ServerId, Is.EqualTo(serverId));
        }

        [Test]
        public void MergeKeepsOriginalMaxSessionDuration()
        {
            // given
            const int sessionDuration = 73;
            var target = new ServerConfiguration.Builder(defaultValues)
                .WithMaxSessionDurationInMilliseconds(sessionDuration).Build();
            var other = new ServerConfiguration.Builder(defaultValues)
                .WithMaxSessionDurationInMilliseconds(37).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.MaxSessionDurationInMilliseconds, Is.EqualTo(sessionDuration));
        }

        [Test]
        public void MergeKeepsIsSessionSplitBySessionDurationEnabledWhenMaxEventsIsGreaterZeroAndAttributeIsSet()
        {
            // given
            const int sessionDuration = 73;
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_SESSION_DURATION).Returns(true);
            mockAttributes.MaxSessionDurationInMilliseconds.Returns(sessionDuration);
            var target = ServerConfiguration.From(mockAttributes);
            var other = new ServerConfiguration.Builder(defaultValues).Build();

            Assert.That(other.IsSessionSplitBySessionDurationEnabled, Is.False);
            Assert.That(target.IsSessionSplitBySessionDurationEnabled, Is.True);

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsSessionSplitBySessionDurationEnabled, Is.True);
        }

        [Test]
        public void MergeKeepsIsSessionSplitBySessionDurationEnabledWhenMaxEventsIsSmallerZeroButAttributeIsSet()
        {
            // given
            const int sessionDuration = 0;
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_SESSION_DURATION).Returns(true);
            mockAttributes.MaxSessionDurationInMilliseconds.Returns(sessionDuration);
            var target = ServerConfiguration.From(mockAttributes);
            var other = Substitute.For<IServerConfiguration>();
            other.IsSessionSplitBySessionDurationEnabled.Returns(true);

            Assert.That(target.IsSessionSplitBySessionDurationEnabled, Is.False);

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsSessionSplitBySessionDurationEnabled, Is.False);
        }

        [Test]
        public void MergeKeepsIsSessionSplitBySessionDurationEnabledWhenMaxEventsIsGreaterZeroButAttributeIsNotSet()
        {
            // given
            const int sessionDuration = 73;
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_SESSION_DURATION).Returns(false);
            mockAttributes.MaxSessionDurationInMilliseconds.Returns(sessionDuration);
            var target = ServerConfiguration.From(mockAttributes);
            var other = Substitute.For<IServerConfiguration>();
            other.IsSessionSplitBySessionDurationEnabled.Returns(true);

            Assert.That(target.IsSessionSplitBySessionDurationEnabled, Is.False);

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsSessionSplitBySessionDurationEnabled, Is.False);
        }

        [Test]
        public void MergeKeepsOriginalMaxEventsPerSession()
        {
            // given
            const int eventsPerSession = 73;
            var target = new ServerConfiguration.Builder(defaultValues).WithMaxEventsPerSession(eventsPerSession)
                .Build();
            var other = new ServerConfiguration.Builder(defaultValues).WithMaxEventsPerSession(37).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.MaxEventsPerSession, Is.EqualTo(eventsPerSession));
        }

        [Test]
        public void MergeKeepsIsSessionSplitByEventsEnabledWhenMaxEventsIsGreaterZeroAndAttributeIsSet()
        {
            // given
            const int eventsPerSession = 73;
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION).Returns(true);
            mockAttributes.MaxEventsPerSession.Returns(eventsPerSession);
            var target = ServerConfiguration.From(mockAttributes);
            var other = new ServerConfiguration.Builder(defaultValues).Build();

            Assert.That(other.IsSessionSplitByEventsEnabled, Is.False);
            Assert.That(target.IsSessionSplitByEventsEnabled, Is.True);

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsSessionSplitByEventsEnabled, Is.True);
        }

        [Test]
        public void MergeKeepsIsSessionSplitByEventsEnabledWhenMaxEventsIsSmallerZeroButAttributeIsSet()
        {
            // given
            const int eventsPerSession = 0;
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION).Returns(true);
            mockAttributes.MaxEventsPerSession.Returns(eventsPerSession);
            var target = ServerConfiguration.From(mockAttributes);
            var other = Substitute.For<IServerConfiguration>();
            other.IsSessionSplitByEventsEnabled.Returns(true);

            Assert.That(target.IsSessionSplitByEventsEnabled, Is.False);

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsSessionSplitByEventsEnabled, Is.False);
        }

        [Test]
        public void MergeKeepsIsSessionSplitByEventsEnabledWhenMaxEventsIsGreaterZeroButAttributeIsNotSet()
        {
            // given
            const int eventsPerSession = 73;
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION).Returns(false);
            mockAttributes.MaxEventsPerSession.Returns(eventsPerSession);
            var target = ServerConfiguration.From(mockAttributes);
            var other = Substitute.For<IServerConfiguration>();
            other.IsSessionSplitByEventsEnabled.Returns(true);

            Assert.That(target.IsSessionSplitByEventsEnabled, Is.False);

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsSessionSplitByEventsEnabled, Is.False);
        }

        [Test]
        public void MergeKeepsOriginalSessionTimeout()
        {
            // given
            const int sessionTimeout = 73;
            var target = new ServerConfiguration.Builder(defaultValues).WithSessionTimeoutInMilliseconds(sessionTimeout)
                .Build();
            var other = new ServerConfiguration.Builder(defaultValues).WithMaxSessionDurationInMilliseconds(37)
                .Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.SessionTimeoutInMilliseconds, Is.EqualTo(sessionTimeout));
        }

        [Test]
        public void MergeKeepsIsSessionSplitByIdleTimeoutEnabledWhenMaxEventsIsGreaterZeroAndAttributeIsSet()
        {
            // given
            int idleTimeout = 73;
            mockAttributes.IsAttributeSet(ResponseAttribute.SESSION_IDLE_TIMEOUT).Returns(true);
            mockAttributes.SessionTimeoutInMilliseconds.Returns(idleTimeout);
            var target = ServerConfiguration.From(mockAttributes);
            var other = new ServerConfiguration.Builder(defaultValues).Build();

            Assert.That(other.IsSessionSplitByIdleTimeoutEnabled, Is.False);
            Assert.That(target.IsSessionSplitByIdleTimeoutEnabled, Is.True);

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsSessionSplitByIdleTimeoutEnabled, Is.True);
        }

        [Test]
        public void MergeKeepsIsSessionSplitByIdleTimeoutEnabledWhenMaxEventsIsSmallerZeroButAttributeIsSet()
        {
            // given
            const int idleTimeout = 0;
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_SESSION_DURATION).Returns(true);
            mockAttributes.SessionTimeoutInMilliseconds.Returns(idleTimeout);
            var target = ServerConfiguration.From(mockAttributes);
            var other = Substitute.For<IServerConfiguration>();
            other.IsSessionSplitByIdleTimeoutEnabled.Returns(true);

            Assert.That(target.IsSessionSplitByIdleTimeoutEnabled, Is.False);

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsSessionSplitByIdleTimeoutEnabled, Is.False);
        }

        [Test]
        public void MergeKeepsIsSessionSplitByIdleTimeoutEnabledWhenMaxEventsIsGreaterZeroButAttributeIsNotSet()
        {
            // given
            const int idleTimeout = 73;
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_SESSION_DURATION).Returns(false);
            mockAttributes.SessionTimeoutInMilliseconds.Returns(idleTimeout);
            var target = ServerConfiguration.From(mockAttributes);
            var other = Substitute.For<IServerConfiguration>();
            other.IsSessionSplitByIdleTimeoutEnabled.Returns(true);

            Assert.That(target.IsSessionSplitByIdleTimeoutEnabled, Is.False);

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsSessionSplitByIdleTimeoutEnabled, Is.False);
        }

        [Test]
        public void MergeKeepsOriginalVisitStoreVersion()
        {
            // given
            const int visitStoreVersion = 73;
            var target = new ServerConfiguration.Builder(defaultValues).WithVisitStoreVersion(visitStoreVersion)
                .Build();
            var other = new ServerConfiguration.Builder(defaultValues).WithVisitStoreVersion(37).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.VisitStoreVersion, Is.EqualTo(visitStoreVersion));
        }

        #endregion

        #region test builder

        [Test]
        public void BuildPropagatesCaptureEnabledToInstance()
        {
            // given
            var capture = !defaultValues.IsCapture;

            // when
            var obtained = new ServerConfiguration.Builder(defaultValues).WithCapture(capture).Build();

            // then
            Assert.That(obtained.IsCaptureEnabled, Is.EqualTo(capture));
        }

        [Test]
        public void BuildPropagatesCrashReportingEnabledToInstance()
        {
            // given
            var crashReporting = !defaultValues.IsCaptureCrashes;

            // when
            var obtained = new ServerConfiguration.Builder(defaultValues).WithCrashReporting(crashReporting).Build();

            // then
            Assert.That(obtained.IsCrashReportingEnabled, Is.EqualTo(crashReporting));
        }

        [Test]
        public void BuildPropagatesErrorReportingEnabledToInstance()
        {
            // given
            var errorReporting = !defaultValues.IsCaptureErrors;

            // when
            var obtained = new ServerConfiguration.Builder(defaultValues).WithErrorReporting(errorReporting).Build();

            // then
            Assert.That(obtained.IsErrorReportingEnabled, Is.EqualTo(errorReporting));
        }

        [Test]
        public void BuildPropagatesServerIdToInstance()
        {
            // given
            const int serverId = 73;

            // when
            var obtained = new ServerConfiguration.Builder(defaultValues).WithServerId(serverId).Build();

            // then
            Assert.That(obtained.ServerId, Is.EqualTo(serverId));
        }

        [Test]
        public void BuildPropagatesBeaconSizeToInstance()
        {
            // given
            const int beaconSize = 73;

            // when
            var obtained = new ServerConfiguration.Builder(defaultValues).WithBeaconSizeInBytes(beaconSize).Build();

            // then
            Assert.That(obtained.BeaconSizeInBytes, Is.EqualTo(beaconSize));
        }

        [Test]
        public void BuildPropagatesMultiplicityToInstance()
        {
            // given
            const int multiplicity = 73;

            // when
            var obtained = new ServerConfiguration.Builder(defaultValues).WithMultiplicity(multiplicity).Build();

            // then
            Assert.That(obtained.Multiplicity, Is.EqualTo(multiplicity));
        }

        [Test]
        public void BuildPropagatesSendIntervalToInstance()
        {
            // given
            const int sendInterval = 777;

            // when
            var obtained = new ServerConfiguration.Builder(defaultValues)
                .WithSendIntervalInMilliseconds(sendInterval).Build();

            // then
            Assert.That(obtained.SendIntervalInMilliseconds, Is.EqualTo(sendInterval));
        }

        [Test]
        public void BuildPropagatesMaxSessionDurationToInstance()
        {
            // given
            const int sessionDuration = 73;

            // when
            var obtained = new ServerConfiguration.Builder(defaultValues)
                .WithMaxSessionDurationInMilliseconds(sessionDuration).Build();

            // then
            Assert.That(obtained.MaxSessionDurationInMilliseconds, Is.EqualTo(sessionDuration));
        }

        [Test]
        public void BuildPropagatesMaxEventsPerSessionToInstance()
        {
            // given
            const int eventsPerSession = 73;

            // when
            var obtained = new ServerConfiguration.Builder(defaultValues)
                .WithMaxEventsPerSession(eventsPerSession).Build();

            // then
            Assert.That(obtained.MaxEventsPerSession, Is.EqualTo(eventsPerSession));
        }

        [Test]
        public void BuildPropagatesSessionTimeoutToInstance()
        {
            // given
            const int sessionTimeout = 73;

            // when
            var obtained = new ServerConfiguration.Builder(defaultValues)
                .WithSessionTimeoutInMilliseconds(sessionTimeout).Build();

            // then
            Assert.That(obtained.SessionTimeoutInMilliseconds, Is.EqualTo(sessionTimeout));
        }

        [Test]
        public void BuildPropagatesVisitStoreVersionToInstance()
        {
            // given
            const int visitStoreVersion = 73;

            // when
            var obtained = new ServerConfiguration.Builder(defaultValues)
                .WithVisitStoreVersion(visitStoreVersion).Build();

            // then
            Assert.That(obtained.VisitStoreVersion, Is.EqualTo(visitStoreVersion));
        }

        #endregion
    }
}