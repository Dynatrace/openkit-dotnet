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
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class OpenKitConfigurationTest
    {
        private const string EndpointUrl = "https://localhost:9999/1";
        private const long DeviceId = 37;
        private const string OpenKitType = "Dynatrace NextGen";
        private const string ApplicationId = "Application-Id";
        private const string ApplicationName = "Application Name";
        private const string ApplicationVersion = "1.2.3.4-b4321";
        private const string OperatingSystem = "Linux #253-Microsoft Mon Dec 31 17:49:00 PST 2018 x86_64 GNU/Linux";
        private const string Manufacturer = "Dynatrace";
        private const string ModelId = "Latest Model";
        private const int DefaultServerId = 777;

        private IOpenKitBuilder mockOpenKitBuilder;

        [SetUp]
        public void SetUp()
        {
            mockOpenKitBuilder = Substitute.For<IOpenKitBuilder>();
            mockOpenKitBuilder.EndpointUrl.Returns(EndpointUrl);
            mockOpenKitBuilder.DeviceId.Returns(DeviceId);
            mockOpenKitBuilder.OpenKitType.Returns(OpenKitType);
            mockOpenKitBuilder.ApplicationId.Returns(ApplicationId);
            mockOpenKitBuilder.ApplicationName.Returns(ApplicationName);
            mockOpenKitBuilder.ApplicationVersion.Returns(ApplicationVersion);
            mockOpenKitBuilder.OperatingSystem.Returns(OperatingSystem);
            mockOpenKitBuilder.Manufacturer.Returns(Manufacturer);
            mockOpenKitBuilder.ModelId.Returns(ModelId);
            mockOpenKitBuilder.DefaultServerId.Returns(DefaultServerId);
        }

        [Test]
        public void CreatingOpenKitConfigurationFromNullBuilderGivesNull()
        {
            // when, then
            Assert.That(OpenKitConfiguration.From(null), Is.Null);
        }

        [Test]
        public void CreatingOpenKitConfigurationFromNonNullBuilderGivesNonNullConfiguration()
        {
            // when
            var obtained = OpenKitConfiguration.From(mockOpenKitBuilder);

            // then
            Assert.That(obtained, Is.Not.Null);
        }

        [Test]
        public void CreatingAnOpenKitConfigurationFromBuilderCopiesEndpointUrl()
        {
            // given, when
            var target = OpenKitConfiguration.From(mockOpenKitBuilder);

            // then
            Assert.That(target.EndpointUrl, Is.EqualTo(EndpointUrl));
            _ = mockOpenKitBuilder.Received(1).EndpointUrl;
        }

        [Test]
        public void CreatingOpenKitConfigurationFromBuilderCopiesDeviceId()
        {
            // given, when
            var target = OpenKitConfiguration.From(mockOpenKitBuilder);

            // then
            Assert.That(target.DeviceId, Is.EqualTo(DeviceId));
            _ = mockOpenKitBuilder.Received(1).DeviceId;
        }

        [Test]
        public void CreatingOpenKitConfigurationFromBuilderCopiesType()
        {
            // given, when
            var target = OpenKitConfiguration.From(mockOpenKitBuilder);

            // then
            Assert.That(target.OpenKitType, Is.EqualTo(OpenKitType));
            _ = mockOpenKitBuilder.Received(1).OpenKitType;
        }

        [Test]
        public void CreatingOpenKitConfigurationFromBuilderCopiesApplicationId()
        {
            // given, when
            var target = OpenKitConfiguration.From(mockOpenKitBuilder);

            // then
            Assert.That(target.ApplicationId, Is.EqualTo(ApplicationId));
            _ = mockOpenKitBuilder.Received(1).ApplicationId;
        }

        [Test]
        public void CreatingOpenKitConfigurationFromBuilderPercentEncodesApplicationId()
        {
            // given
            mockOpenKitBuilder.ApplicationId.Returns("/App_ID%");
            var target = OpenKitConfiguration.From(mockOpenKitBuilder);

            // when
            var obtained = target.ApplicationIdPercentEncoded;

            // then
            Assert.That(obtained, Is.EqualTo("%2FApp%5FID%25"));
        }

        [Test]
        public void CreatingOpenKitConfigurationFromBuilderCopiesApplicationName()
        {
            // given, when
            var target = OpenKitConfiguration.From(mockOpenKitBuilder);

            // then
            Assert.That(target.ApplicationName, Is.EqualTo(ApplicationName));
            _ = mockOpenKitBuilder.Received(1).ApplicationName;
        }

        [Test]
        public void CreatingOpenKitConfigurationFromBuilderCopiesApplicationVersion()
        {
            // given, when
            var target = OpenKitConfiguration.From(mockOpenKitBuilder);

            // then
            Assert.That(target.ApplicationVersion, Is.EqualTo(ApplicationVersion));
            _ = mockOpenKitBuilder.Received(1).ApplicationVersion;
        }

        [Test]
        public void CreatingOpenKitConfigurationFromBuilderCopiesOperatingSystem()
        {
            // given, when
            var target = OpenKitConfiguration.From(mockOpenKitBuilder);

            // then
            Assert.That(target.OperatingSystem, Is.EqualTo(OperatingSystem));
            _ = mockOpenKitBuilder.Received(1).OperatingSystem;
        }

        [Test]
        public void CreatingOpenKitConfigurationFromBuilderCopiesManufacturer()
        {
            // given, when
            var target = OpenKitConfiguration.From(mockOpenKitBuilder);

            // then
            Assert.That(target.Manufacturer, Is.EqualTo(Manufacturer));
            _ = mockOpenKitBuilder.Received(1).Manufacturer;
        }

        [Test]
        public void CreatingOpenKitConfigurationFromBuilderCopiesModelId()
        {
            // given, when
            var target = OpenKitConfiguration.From(mockOpenKitBuilder);

            // then
            Assert.That(target.ModelId, Is.EqualTo(ModelId));
            _ = mockOpenKitBuilder.Received(1).ModelId;
        }

        [Test]
        public void CreatingOpenKitConfigurationFromBuilderCopiesDefaultServerId()
        {
            // given, when
            var target = OpenKitConfiguration.From(mockOpenKitBuilder);

            // then
            Assert.That(target.DefaultServerId, Is.EqualTo(DefaultServerId));
            _ = mockOpenKitBuilder.Received(1).DefaultServerId;
        }

        [Test]
        public void CreatingOpenKitConfigurationFromBuilderCopiesTrustManager()
        {
            // given, when
            var trustManager = Substitute.For<ISSLTrustManager>();
            mockOpenKitBuilder.TrustManager.Returns(trustManager);

            // when
            var target = OpenKitConfiguration.From(mockOpenKitBuilder);

            // then
            Assert.That(target.TrustManager, Is.SameAs(trustManager));
            _ = mockOpenKitBuilder.Received(1).TrustManager;
        }
    }
}
