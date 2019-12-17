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

using System;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Objects;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class BeaconConfigurationTest
    {
        private IOpenKitConfiguration mockOpenKitConfig;
        private IPrivacyConfiguration mockPrivacyConfig;
        private const int ServerId = 1;

        [SetUp]
        public void SetUp()
        {
            mockOpenKitConfig = Substitute.For<IOpenKitConfiguration>();
            mockPrivacyConfig = Substitute.For<IPrivacyConfiguration>();
        }

        [Test]
        public void FromWithNullOpenKitConfigurationGivesNull()
        {
            // when, then
            Assert.That(BeaconConfiguration.From(null, mockPrivacyConfig, ServerId), Is.Null);
        }

        [Test]
        public void FromWithNullPrivacyConfigurationGivesNull()
        {
            // when, then
            Assert.That(BeaconConfiguration.From(mockOpenKitConfig, null, ServerId), Is.Null);
        }

        [Test]
        public void FromWithNonNullArgumentsGivesNonNullBeaconConfiguration()
        {
            // when
            var obtained = CreateBeaconConfig();

            // then
            Assert.That(obtained, Is.Not.Null);
        }

        [Test]
        public void OpenKitConfigurationReturnsPassedObject()
        {
            // given
            var target = CreateBeaconConfig();

            // when
            var obtained = target.OpenKitConfiguration;

            // then
            Assert.That(obtained, Is.SameAs(mockOpenKitConfig));
        }

        [Test]
        public void PrivacyConfigurationReturnsPassedObject()
        {
            // given
            var target = CreateBeaconConfig();

            // when
            var obtained = target.PrivacyConfiguration;

            // then
            Assert.That(obtained, Is.SameAs(mockPrivacyConfig));
        }

        [Test]
        public void NewInstanceReturnsDefaultServerConfiguration()
        {
            // given
            var target = CreateBeaconConfig();

            // when
            var obtained = target.ServerConfiguration;

            // then
            Assert.That(obtained, Is.SameAs(ServerConfiguration.Default));
        }

        [Test]
        public void NewInstanceReturnsIsServerConfigurationSetFalse()
        {
            // given
            var target = CreateBeaconConfig();

            // when
            var obtained = target.IsServerConfigurationSet;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void NewInstanceReturnHttpClientConfigWithGivenServerId()
        {
            // given
            const string endpointUrl = "https://localhost:9999/1";
            const string applicationId = "some cryptic appId";
            var trustManager = Substitute.For<ISSLTrustManager>();
            const int serverId = 73;
            mockOpenKitConfig.EndpointUrl.Returns(endpointUrl);
            mockOpenKitConfig.ApplicationId.Returns(applicationId);
            mockOpenKitConfig.TrustManager.Returns(trustManager);
            mockOpenKitConfig.DefaultServerId.Returns(serverId);

            var target = BeaconConfiguration.From(mockOpenKitConfig, mockPrivacyConfig, serverId);

            // when
            var obtained = target.HttpClientConfiguration;

            // then
            Assert.That(obtained.BaseUrl, Is.EqualTo(endpointUrl));
            Assert.That(obtained.ApplicationId, Is.EqualTo(applicationId));
            Assert.That(obtained.SslTrustManager, Is.SameAs(trustManager));
            Assert.That(obtained.ServerId, Is.EqualTo(serverId));
        }

        [Test]
        public void UpdateServerConfigurationSetsIsServerConfigurationSet()
        {
            // given
            var serverConfig = Substitute.For<IServerConfiguration>();
            var target = CreateBeaconConfig();

            // when
            target.UpdateServerConfiguration(serverConfig);

            // then
            Assert.That(target.IsServerConfigurationSet, Is.True);
            Assert.That(serverConfig.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void UpdateServerConfigurationTakesOverServerConfigurationIfNotSet()
        {
            // given
            var serverConfig = Substitute.For<IServerConfiguration>();
            var target = CreateBeaconConfig();

            // then
            target.UpdateServerConfiguration(serverConfig);

            // then
            Assert.That(target.ServerConfiguration, Is.SameAs(serverConfig));
        }

        [Test]
        public void UpdateServerConfigurationMergesServerConfigIfAlreadySet()
        {
            // given
            var serverConfig1 = Substitute.For<IServerConfiguration>();
            var serverConfig2 = Substitute.For<IServerConfiguration>();
            serverConfig1.Merge(serverConfig2).Returns(serverConfig2);

            var target = CreateBeaconConfig();

            // when
            target.UpdateServerConfiguration(serverConfig1);

            // then
            Assert.That(serverConfig1.ReceivedCalls(), Is.Empty);

            // when
            target.UpdateServerConfiguration(serverConfig2);

            // then
            serverConfig1.Received(1).Merge(serverConfig2);
        }

        [Test]
        public void UpdateServerConfigurationDoesNotUpdateHttpClientConfig()
        {
            // given
            const int serverId = 73;
            var serverConfig = Substitute.For<IServerConfiguration>();
            serverConfig.ServerId.Returns(serverId);

            var target = CreateBeaconConfig();
            var httpConfig = target.HttpClientConfiguration;

            // when
            target.UpdateServerConfiguration(serverConfig);
            var obtained = target.HttpClientConfiguration;

            // then
            Assert.That(obtained.ServerId, Is.EqualTo(ServerId));
            Assert.That(obtained, Is.SameAs(httpConfig));
        }

        [Test]
        public void UpdateServerConfigurationDoesNotUpdateHttpClientConfigurationIfServerIdEquals()
        {
            // given
            const int serverId = 73;
            var serverConfig = Substitute.For<IServerConfiguration>();
            serverConfig.ServerId.Returns(serverId);
            mockOpenKitConfig.DefaultServerId.Returns(serverId);

            var target = CreateBeaconConfig();
            var httpConfig = target.HttpClientConfiguration;

            // when
            target.UpdateServerConfiguration(serverConfig);
            var obtained = target.HttpClientConfiguration;

            // then
            Assert.That(obtained, Is.SameAs(httpConfig));
        }

        [Test]
        public void UpdateServerConfigurationWithNullDoesNothing()
        {
            // given
            var target = CreateBeaconConfig();

            // when
            target.UpdateServerConfiguration(null);

            // then
            Assert.That(target.IsServerConfigurationSet, Is.False);
            Assert.That(target.ServerConfiguration, Is.SameAs(ServerConfiguration.Default));
        }

        [Test]
        public void UpdateServerConfigurationDoesInvokeCallbackIfCallbackIsSet()
        {
            // given
            var sessionProxy = Substitute.For<ISessionProxy>();
            var serverConfig = Substitute.For<IServerConfiguration>();
            var target = CreateBeaconConfig();
            target.OnServerConfigurationUpdate += sessionProxy.OnServerConfigurationUpdate;

            // when
            target.UpdateServerConfiguration(serverConfig);

            // then
            sessionProxy.Received(1).OnServerConfigurationUpdate(serverConfig);
        }

        [Test]
        public void UpdateServerConfigurationDoesNotInvokeCallbackIfNoCallbackIsSet()
        {
            // given
            var sessionProxy = Substitute.For<ISessionProxy>();
            var serverConfig = Substitute.For<IServerConfiguration>();
            var target = CreateBeaconConfig();
            target.OnServerConfigurationUpdate += sessionProxy.OnServerConfigurationUpdate;
            target.OnServerConfigurationUpdate -= sessionProxy.OnServerConfigurationUpdate;

            // when
            target.UpdateServerConfiguration(serverConfig);

            // then
            Assert.That(sessionProxy.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void EnableCaptureSetsIsConfigurationSet()
        {
            // given
            var target = CreateBeaconConfig();

            // when
            target.EnableCapture();


            // then
            Assert.That(target.IsServerConfigurationSet, Is.True);
        }

        [Test]
        public void EnableCaptureUpdatesServerConfigIfCaptureIsDisabled()
        {
            // given
            var initialServerConfig = Substitute.For<IServerConfiguration>();
            initialServerConfig.IsCaptureEnabled.Returns(false);

            var target = CreateBeaconConfig();
            target.UpdateServerConfiguration(initialServerConfig);

            // when
            target.EnableCapture();
            var obtained = target.ServerConfiguration;

            // then
            Assert.That(obtained, Is.Not.SameAs(initialServerConfig));
            Assert.That(obtained, Is.Not.SameAs(ServerConfiguration.Default));
            Assert.That(obtained.IsCaptureEnabled, Is.True);
        }

        [Test]
        public void EnableCaptureDoesOnlyModifyCaptureFlag()
        {
            // given
            var initialServerConfig = MockServerConfig(false);

            var target = CreateBeaconConfig();
            target.UpdateServerConfiguration(initialServerConfig);

            // when
            target.EnableCapture();
            var obtained = target.ServerConfiguration;

            // then
            Assert.That(obtained, Is.Not.SameAs(initialServerConfig));
            Assert.That(obtained.IsCaptureEnabled, Is.Not.EqualTo(initialServerConfig.IsCaptureEnabled));
            Assert.That(obtained.IsCrashReportingEnabled, Is.EqualTo(initialServerConfig.IsCrashReportingEnabled));
            Assert.That(obtained.IsErrorReportingEnabled, Is.EqualTo(initialServerConfig.IsErrorReportingEnabled));
            Assert.That(obtained.SendIntervalInMilliseconds,
                Is.EqualTo(initialServerConfig.SendIntervalInMilliseconds));
            Assert.That(obtained.ServerId, Is.EqualTo(initialServerConfig.ServerId));
            Assert.That(obtained.BeaconSizeInBytes, Is.EqualTo(initialServerConfig.BeaconSizeInBytes));
            Assert.That(obtained.Multiplicity, Is.EqualTo(initialServerConfig.Multiplicity));
        }

        [Test]
        public void DisableCaptureSetsIsServerConfigurationSet()
        {
            // given
            var target = CreateBeaconConfig();

            // when
            target.DisableCapture();

            // then
            Assert.That(target.IsServerConfigurationSet, Is.True);
        }

        [Test]
        public void DisableCaptureUpdatesServerConfigIfCaptureGetsDisabled()
        {
            // given
            var initialServerConfig = Substitute.For<IServerConfiguration>();
            initialServerConfig.IsCaptureEnabled.Returns(true);

            var target = CreateBeaconConfig();
            target.UpdateServerConfiguration(initialServerConfig);

            // when
            target.DisableCapture();
            var obtained = target.ServerConfiguration;

            // then
            Assert.That(obtained, Is.Not.SameAs(initialServerConfig));
            Assert.That(obtained, Is.Not.SameAs(ServerConfiguration.Default));
            Assert.That(obtained.IsCaptureEnabled, Is.EqualTo(false));
        }

        [Test]
        public void DisableCaptureDoesOnlyModifyCaptureFlag()
        {
            // given
            var initialServerConfig = MockServerConfig(true);

            var target = CreateBeaconConfig();
            target.UpdateServerConfiguration(initialServerConfig);

            // when
            target.DisableCapture();
            var obtained = target.ServerConfiguration;

            // then
            Assert.That(obtained, Is.Not.SameAs(initialServerConfig));
            Assert.That(obtained.IsCaptureEnabled, Is.Not.EqualTo(initialServerConfig.IsCaptureEnabled));
            Assert.That(obtained.IsCrashReportingEnabled, Is.EqualTo(initialServerConfig.IsCrashReportingEnabled));
            Assert.That(obtained.IsErrorReportingEnabled, Is.EqualTo(initialServerConfig.IsErrorReportingEnabled));
            Assert.That(obtained.SendIntervalInMilliseconds,
                Is.EqualTo(initialServerConfig.SendIntervalInMilliseconds));
            Assert.That(obtained.ServerId, Is.EqualTo(initialServerConfig.ServerId));
            Assert.That(obtained.BeaconSizeInBytes, Is.EqualTo(initialServerConfig.BeaconSizeInBytes));
            Assert.That(obtained.Multiplicity, Is.EqualTo(initialServerConfig.Multiplicity));
        }

        private IBeaconConfiguration CreateBeaconConfig()
        {
            return BeaconConfiguration.From(mockOpenKitConfig, mockPrivacyConfig, ServerId);
        }

        private IServerConfiguration MockServerConfig(Boolean enableCapture)
        {
            var serverConfig = Substitute.For<IServerConfiguration>();
            serverConfig.IsCaptureEnabled.Returns(enableCapture);
            serverConfig.IsCrashReportingEnabled.Returns(true);
            serverConfig.IsErrorReportingEnabled.Returns(true);
            serverConfig.SendIntervalInMilliseconds.Returns(999);
            serverConfig.ServerId.Returns(73);
            serverConfig.BeaconSizeInBytes.Returns(1024);
            serverConfig.Multiplicity.Returns(37);

            return serverConfig;
        }
    }
}