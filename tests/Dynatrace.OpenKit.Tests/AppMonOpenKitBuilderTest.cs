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

using Dynatrace.OpenKit.Core.Configuration;
using NUnit.Framework;

namespace Dynatrace.OpenKit
{
    public class AppMonOpenKitBuilderTest
    {
        private const string EndpointUrl = "https://localhost";
        private const string ApplicationName = "the-application-name";
        private const long DeviceId = 777;

        [Test]
        public void ConstructorInitializesApplicationName()
        {
            // given, when
            var target = new AppMonOpenKitBuilder(EndpointUrl, ApplicationName, DeviceId);

            // then
            Assert.That(target.ApplicationName, Is.EqualTo(ApplicationName));
        }

        [Test]
        public void ConstructorInitializesDeviceIdString()
        {
            // given, when
            var target = new AppMonOpenKitBuilder(EndpointUrl, ApplicationName, DeviceId.ToString());

            // then
            Assert.That(target.DeviceId, Is.EqualTo(DeviceId));
            Assert.That(target.OrigDeviceId, Is.EqualTo(DeviceId.ToString()));
        }

        [Test]
        public void ApplicationIdGivesSameValueAsApplicationName()
        {
            // given, when
            var target = new AppMonOpenKitBuilder(EndpointUrl, ApplicationName, DeviceId);

            // then
            Assert.That(target.ApplicationId, Is.EqualTo(ApplicationName));
        }

        [Test]
        public void OpenKitTypeGivesAppropriateValue()
        {
            // given, when
            var target = new AppMonOpenKitBuilder(EndpointUrl, ApplicationName, DeviceId);

            // then
            Assert.That(target.OpenKitType, Is.EqualTo(AppMonOpenKitBuilder.Type));
        }

        [Test]
        public void DefaultServerIdGivesAppropriateValue()
        {
            // given, when
            var target = new AppMonOpenKitBuilder(EndpointUrl, ApplicationName, DeviceId);

            // then
            Assert.That(target.DefaultServerId, Is.EqualTo(AppMonOpenKitBuilder.DefaultServerIdValue));
        }
    }
}