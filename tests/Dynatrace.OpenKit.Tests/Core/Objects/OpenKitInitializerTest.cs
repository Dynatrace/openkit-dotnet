//
// Copyright 2018-2021 Dynatrace LLC
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

namespace Dynatrace.OpenKit.Core.Objects
{
    public class OpenKitInitializerTest
    {
        private const string AppId = "appId";
        private const string AppName = "appName";
        private const string AppVersion = "1.2.3";
        private IOpenKitBuilder mockBuilder;
        private ILogger mockLogger;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();

            mockBuilder = Substitute.For<IOpenKitBuilder>();
            mockBuilder.Logger.Returns(mockLogger);
            mockBuilder.ApplicationId.Returns(AppId);
            mockBuilder.ApplicationName.Returns(AppName);
            mockBuilder.ApplicationVersion.Returns(AppVersion);
        }

        [Test]
        public void ConstructorTakesOverLogger()
        {
            // given, when
            var target = CreateOpenKitInitializer() as IOpenKitInitializer;

            // then
            Assert.That(target.Logger, Is.EqualTo(mockLogger));
        }

        [Test]
        public void ConstructorInitializesPrivacyConfiguration()
        {
            // given, when
            var target = CreateOpenKitInitializer() as IOpenKitInitializer;

            // then
            Assert.That(target.PrivacyConfiguration, Is.Not.Null);
        }

        [Test]
        public void ConstructorInitializesOpenKitConfiguration()
        {
            // given, when
            var target = CreateOpenKitInitializer() as IOpenKitInitializer;

            // then
            Assert.That(target.OpenKitConfiguration, Is.Not.Null);
        }

        [Test]
        public void ConstructorInitializesTimingProvider()
        {
            // given, when
            var target = CreateOpenKitInitializer() as IOpenKitInitializer;

            // then
            Assert.That(target.TimingProvider, Is.Not.Null);
        }

        [Test]
        public void ConstructorInitializesThreadIdProvider()
        {
            // given, when
            var target = CreateOpenKitInitializer() as IOpenKitInitializer;

            // then
            Assert.That(target.ThreadIdProvider, Is.Not.Null);
        }

        [Test]
        public void ConstructorInitializesSessionIdProvider()
        {
            // given, when
            var target = CreateOpenKitInitializer() as IOpenKitInitializer;

            // then
            Assert.That(target.SessionIdProvider, Is.Not.Null);
        }

        [Test]
        public void ConstructorInitializesBeaconCache()
        {
            // given, when
            var target = CreateOpenKitInitializer() as IOpenKitInitializer;

            // then
            Assert.That(target.BeaconCache, Is.Not.Null);
        }

        [Test]
        public void ConstructorInitializesBeaconCacheEvictor()
        {
            // given, when
            var target = CreateOpenKitInitializer() as IOpenKitInitializer;

            // then
            Assert.That(target.BeaconCacheEvictor, Is.Not.Null);
        }

        [Test]
        public void ConstructorInitializesBeaconSender()
        {
            // given, when
            var target = CreateOpenKitInitializer() as IOpenKitInitializer;

            // then
            Assert.That(target.BeaconSender, Is.Not.Null);
        }

        [Test]
        public void ConstructorInitializesSessionWatchdog()
        {
            // given, when
            var target = CreateOpenKitInitializer() as IOpenKitInitializer;

            // then
            Assert.That(target.SessionWatchdog, Is.Not.Null);
        }

        private OpenKitInitializer CreateOpenKitInitializer()
        {
            return new OpenKitInitializer(mockBuilder);
        }
    }
}