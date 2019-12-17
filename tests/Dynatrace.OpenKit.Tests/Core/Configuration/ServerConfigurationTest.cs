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
            mockAttributes.IsCapture.Returns(ServerConfiguration.DefaultCaptureEnabled);
            mockAttributes.IsCaptureCrashes.Returns(ServerConfiguration.DefaultCrashReportingEnabled);
            mockAttributes.IsCaptureErrors.Returns(ServerConfiguration.DefaultErrorReportingEnabled);
            mockAttributes.SendIntervalInMilliseconds.Returns(ServerConfiguration.DefaultSendInterval);
            mockAttributes.ServerId.Returns(ServerConfiguration.DefaultServerId);
            mockAttributes.MaxBeaconSizeInBytes.Returns(ServerConfiguration.DefaultBeaconSize);
            mockAttributes.Multiplicity.Returns(ServerConfiguration.DefaultMultiplicity);
            mockAttributes.MaxSessionDurationInMilliseconds.Returns(ServerConfiguration.DefaultMaxSessionDuration);
            mockAttributes.MaxEventsPerSession.Returns(ServerConfiguration.DefaultMaxEventsPerSession);
            mockAttributes.SessionTimeoutInMilliseconds.Returns(ServerConfiguration.DefaultSessionTimeout);
            mockAttributes.VisitStoreVersion.Returns(ServerConfiguration.DefaultVisitStoreVersion);

            mockServerConfig = Substitute.For<IServerConfiguration>();
            mockServerConfig.IsCaptureEnabled.Returns(ServerConfiguration.DefaultCaptureEnabled);
            mockServerConfig.IsCrashReportingEnabled.Returns(ServerConfiguration.DefaultCrashReportingEnabled);
            mockServerConfig.IsErrorReportingEnabled.Returns(ServerConfiguration.DefaultErrorReportingEnabled);
            mockServerConfig.SendIntervalInMilliseconds.Returns(ServerConfiguration.DefaultSendInterval);
            mockServerConfig.ServerId.Returns(ServerConfiguration.DefaultServerId);
            mockServerConfig.BeaconSizeInBytes.Returns(ServerConfiguration.DefaultBeaconSize);
            mockServerConfig.Multiplicity.Returns(ServerConfiguration.DefaultMultiplicity);
            mockServerConfig.MaxSessionDurationInMilliseconds.Returns(ServerConfiguration.DefaultMaxSessionDuration);
            mockServerConfig.MaxEventsPerSession.Returns(ServerConfiguration.DefaultMaxEventsPerSession);
            mockServerConfig.IsSessionSplitByEventsEnabled.Returns(false);
            mockServerConfig.SessionTimeoutInMilliseconds.Returns(ServerConfiguration.DefaultSessionTimeout);
            mockServerConfig.VisitStoreVersion.Returns(ServerConfiguration.DefaultVisitStoreVersion);
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
        public void InDefaultServerConfigurationSendIntervalIsMinusOne()
        {
            Assert.That(ServerConfiguration.Default.SendIntervalInMilliseconds, Is.EqualTo(-1));
        }

        [Test]
        public void InDefaultServerConfigurationServerIdIsMinusOne()
        {
            Assert.That(ServerConfiguration.Default.ServerId, Is.EqualTo(-1));
        }

        [Test]
        public void InDefaultServerConfigurationBeaconSizeIsMinusOne()
        {
            Assert.That(ServerConfiguration.Default.BeaconSizeInBytes, Is.EqualTo(-1));
        }

        [Test]
        public void InDefaultServerConfigurationMultiplicityIsOne()
        {
            Assert.That(ServerConfiguration.Default.Multiplicity, Is.EqualTo(1));
        }


        [Test]
        public void InDefaultServerConfigurationMaxSessionDurationIsMinusOne()
        {
            Assert.That(ServerConfiguration.Default.MaxSessionDurationInMilliseconds, Is.EqualTo(-1));
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
        public void InDefaultServerConfigurationVisitStoreVersionIsMinusOne()
        {
            Assert.That(ServerConfiguration.Default.VisitStoreVersion, Is.EqualTo(1));
        }

        #endregion

        #region test creation with 'From' method

        [Test]
        public void CreatingAServerConfigurationFromNullStatusResponseGivesNull()
        {
            Assert.That(ServerConfiguration.From(null), Is.Null);
        }


        [Test]
        public void CreatingAServerConfigurationFromStatusResponseCopiesCaptureSettings()
        {
            // given, when
            mockAttributes.IsCapture.Returns(false);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsCaptureEnabled, Is.False);
            _ = mockAttributes.Received(1).IsCapture;
        }

        [Test]
        public void CreatingAServerConfigurationFromStatusResponseCopiesCrashReportingSettings()
        {
            // given, when
            mockAttributes.IsCaptureCrashes.Returns(false);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsCrashReportingEnabled, Is.False);
            _ = mockAttributes.Received(1).IsCaptureCrashes;
        }

        [Test]
        public void CreatingAServerConfigurationFromStatusResponseCopiesErrorReportingSettings()
        {
            // given, when
            mockAttributes.IsCaptureErrors.Returns(false);
            var target = ServerConfiguration.From(mockAttributes);

            // then
            Assert.That(target.IsErrorReportingEnabled, Is.False);
            _ = mockAttributes.Received(1).IsCaptureErrors;
        }

        [Test]
        public void CreatingAServerConfigurationFromStatusResponseCopiesSendingIntervalSettings()
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
        public void CreatingAServerConfigurationFromStatusResponseCopiesServerIdSettings()
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
        public void CreatingAServerConfigurationFromStatusResponseCopiesBeaconSizeSettings()
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
        public void CreatingAServerConfigurationFromStatusResponseCopiesMultiplicitySettings()
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
        public void CreatingAServerConfigurationFromStatusResponseCopiesSessionDuration()
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
        public void CreatingAServerConfigurationFromStatusResponseCopiesMaxEventsPerSession()
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
        public void CreatingAServerConfigurationFromStatusResponseHasSplitBySessionEnabledIfMaxEventsGreaterZero()
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
        public void CreatingAServerConfigurationStatusResponseHasSplitBySessionDisabledIfMaxEventsZero()
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
        public void CreatingAServerConfigurationStatusResponseHasSplitBySessionDisabledIfMaxEventsEventsSmallerZero()
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
        public void CreatingAServerConfigurationStatusResponseHasSplitBySessionDisabledIfMaxEventsIsNotSet()
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
        public void CreatingAServerConfigurationFromStatusResponseCopiesSessionTimeout()
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
        public void CreatingASessionConfigurationFromStatusResponseCopiesVisitStoreVersion()
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
        public void BuilderFromServerConfigCopiesSendingIntervalSettings()
        {
            // given
            mockServerConfig.SendIntervalInMilliseconds.Returns(1234);
            var target = new ServerConfiguration.Builder(mockServerConfig).Build();

            // then
            Assert.That(target.SendIntervalInMilliseconds, Is.EqualTo(1234));
            _ = mockServerConfig.Received(1).SendIntervalInMilliseconds;
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
        public void BuilderFromServerConfigHasSplitBySessionEnabledIfMaxEventsGreaterZero()
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
        public void BuilderFromServerConfigHasSplitBySessionDisabledIfMaxEventsZero()
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
        public void BuilderFromServerConfigHasSplitBySessionDisabledIfMaxEventsSmallerZero()
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
        public void BuilderFromServerConfigHasSplitBySessionDisabledIfMaxEventsIsNotSet()
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
            var target = new ServerConfiguration.Builder().WithCapture(false).Build();
            var other = new ServerConfiguration.Builder().WithCapture(true).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsCaptureEnabled, Is.True);
        }

        [Test]
        public void MergeTakesOverDisabledCapture()
        {
            // given
            var target = new ServerConfiguration.Builder().WithCapture(true).Build();
            var other = new ServerConfiguration.Builder().WithCapture(false).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsCaptureEnabled, Is.False);
        }

        [Test]
        public void MergeTakesOverEnabledCrashReporting()
        {
            // given
            var target = new ServerConfiguration.Builder().WithCrashReporting(false).Build();
            var other = new ServerConfiguration.Builder().WithCrashReporting(true).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsCrashReportingEnabled, Is.True);
        }

        [Test]
        public void MergeTakesOverDisabledCrashReporting()
        {
            // given
            var target = new ServerConfiguration.Builder().WithCrashReporting(true).Build();
            var other = new ServerConfiguration.Builder().WithCrashReporting(false).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsCrashReportingEnabled, Is.False);
        }

        [Test]
        public void MergeTakesOverEnabledErrorReporting()
        {
            // given
            var target = new ServerConfiguration.Builder().WithErrorReporting(false).Build();
            var other = new ServerConfiguration.Builder().WithErrorReporting(true).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsErrorReportingEnabled, Is.True);
        }

        [Test]
        public void MergeTakesOverDisabledErrorReporting()
        {
            // given
            var target = new ServerConfiguration.Builder().WithErrorReporting(true).Build();
            var other = new ServerConfiguration.Builder().WithErrorReporting(false).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsErrorReportingEnabled, Is.False);
        }

        [Test]
        public void MergeTakesOverSendInterval()
        {
            // given
            const int sendInterval = 73;
            var target = new ServerConfiguration.Builder().WithSendingIntervalInMilliseconds(37).Build();
            var other = new ServerConfiguration.Builder().WithSendingIntervalInMilliseconds(sendInterval).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.SendIntervalInMilliseconds, Is.EqualTo(sendInterval));
        }

        [Test]
        public void MergeTakesOverBeaconSize()
        {
            // given
            const int beaconSize = 73;
            var target = new ServerConfiguration.Builder().WithBeaconSizeInBytes(37).Build();
            var other = new ServerConfiguration.Builder().WithBeaconSizeInBytes(beaconSize).Build();

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
            var target = new ServerConfiguration.Builder().WithMultiplicity(multiplicity).Build();
            var other = new ServerConfiguration.Builder().WithMultiplicity(37).Build();

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
            var target = new ServerConfiguration.Builder().WithServerId(serverId).Build();
            var other = new ServerConfiguration.Builder().WithServerId(37).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.ServerId, Is.EqualTo(serverId));
        }

        [Test]
        public void MergeTakesOverMaxSessionDuration()
        {
            // given
            const int sessionDuration = 73;
            var target = new ServerConfiguration.Builder(defaultValues)
                .WithMaxSessionDurationInMilliseconds(37).Build();
            var other = new ServerConfiguration.Builder()
                .WithMaxSessionDurationInMilliseconds(sessionDuration).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.MaxSessionDurationInMilliseconds, Is.EqualTo(sessionDuration));
        }

        [Test]
        public void MergeTakesOverMaxEventsPerSession()
        {
            // given
            const int eventsPerSession = 73;
            var target = new ServerConfiguration.Builder(defaultValues).WithMaxEventsPerSession(37).Build();
            var other = new ServerConfiguration.Builder(defaultValues).WithMaxEventsPerSession(eventsPerSession)
                .Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.MaxEventsPerSession, Is.EqualTo(eventsPerSession));
        }

        [Test]
        public void MergeTakesOverIsSessionSplitByEventsEnabledWhenMaxEventsIsGreaterZeroAndAttributeIsSet()
        {
            // given
            const int eventsPerSession = 73;
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION).Returns(true);
            mockAttributes.MaxEventsPerSession.Returns(eventsPerSession);
            var target = new ServerConfiguration.Builder().Build();
            var other = ServerConfiguration.From(mockAttributes);

            Assert.That(target.IsSessionSplitByEventsEnabled, Is.False);

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsSessionSplitByEventsEnabled, Is.True);
        }

        [Test]
        public void MergeTakesOverIsSessionSplitByEventsEnabledWhenMaxEventsIsSmallerZeroButAttributeIsSet()
        {
            // given
            const int eventsPerSession = 0;
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION).Returns(true);
            mockAttributes.MaxEventsPerSession.Returns(eventsPerSession);
            var target = new ServerConfiguration.Builder().Build();
            var other = ServerConfiguration.From(mockAttributes);

            Assert.That(target.IsSessionSplitByEventsEnabled, Is.False);

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsSessionSplitByEventsEnabled, Is.False);
        }

        [Test]
        public void MergeTakesOverIsSessionSplitByEventsEnabledWhenMaxEventsIsGreaterZeroButAttributeIsNotSet()
        {
            // given
            const int eventsPerSession = 73;
            mockAttributes.IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION).Returns(false);
            mockAttributes.MaxEventsPerSession.Returns(eventsPerSession);
            var target = new ServerConfiguration.Builder().Build();
            var other = ServerConfiguration.From(mockAttributes);

            Assert.That(target.IsSessionSplitByEventsEnabled, Is.False);

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.IsSessionSplitByEventsEnabled, Is.False);
        }

        [Test]
        public void MergeTakesOverSessionTimeout()
        {
            // given
            const int sessionTimeout = 73;
            var target = new ServerConfiguration.Builder(defaultValues).WithMaxSessionDurationInMilliseconds(37)
                .Build();
            var other = new ServerConfiguration.Builder().WithSessionTimeoutInMilliseconds(sessionTimeout).Build();

            // when
            var obtained = target.Merge(other);

            // then
            Assert.That(obtained.SessionTimeoutInMilliseconds, Is.EqualTo(sessionTimeout));
        }

        [Test]
        public void MergeTakesOverVisitStoreVersion()
        {
            // given
            const int visitStoreVersion = 73;
            var target = new ServerConfiguration.Builder().WithVisitStoreVersion(37).Build();
            var other = new ServerConfiguration.Builder().WithVisitStoreVersion(visitStoreVersion).Build();

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
            const bool capture = !ServerConfiguration.DefaultCaptureEnabled;

            // when
            var obtained = new ServerConfiguration.Builder().WithCapture(capture).Build();

            // then
            Assert.That(obtained.IsCaptureEnabled, Is.EqualTo(capture));
        }

        [Test]
        public void BuildPropagatesCrashReportingEnabledToInstance()
        {
            // given
            const bool crashReporting = !ServerConfiguration.DefaultCrashReportingEnabled;

            // when
            var obtained = new ServerConfiguration.Builder().WithCrashReporting(crashReporting).Build();

            // then
            Assert.That(obtained.IsCrashReportingEnabled, Is.EqualTo(crashReporting));
        }

        [Test]
        public void BuildPropagatesErrorReportingEnabledToInstance()
        {
            // given
            const bool errorReporting = !ServerConfiguration.DefaultErrorReportingEnabled;

            // when
            var obtained = new ServerConfiguration.Builder().WithErrorReporting(errorReporting).Build();

            // then
            Assert.That(obtained.IsErrorReportingEnabled, Is.EqualTo(errorReporting));
        }

        [Test]
        public void BuildPropagatesSendIntervalToInstance()
        {
            // given
            const int sendInterval = 73;

            // when
            var obtained = new ServerConfiguration.Builder().WithSendingIntervalInMilliseconds(sendInterval).Build();

            // then
            Assert.That(obtained.SendIntervalInMilliseconds, Is.EqualTo(sendInterval));
        }

        [Test]
        public void BuildPropagatesServerIdToInstance()
        {
            // given
            const int serverId = 73;

            // when
            var obtained = new ServerConfiguration.Builder().WithServerId(serverId).Build();

            // then
            Assert.That(obtained.ServerId, Is.EqualTo(serverId));
        }

        [Test]
        public void BuildPropagatesBeaconSizeToInstance()
        {
            // given
            const int beaconSize = 73;

            // when
            var obtained = new ServerConfiguration.Builder().WithBeaconSizeInBytes(beaconSize).Build();

            // then
            Assert.That(obtained.BeaconSizeInBytes, Is.EqualTo(beaconSize));
        }

        [Test]
        public void BuildPropagatesMultiplicityToInstance()
        {
            // given
            const int multiplicity = 73;

            // when
            var obtained = new ServerConfiguration.Builder().WithMultiplicity(multiplicity).Build();

            // then
            Assert.That(obtained.Multiplicity, Is.EqualTo(multiplicity));
        }

        #endregion
    }
}