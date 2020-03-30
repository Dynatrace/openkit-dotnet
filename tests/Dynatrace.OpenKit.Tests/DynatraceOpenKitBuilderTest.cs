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

using NUnit.Framework;

namespace Dynatrace.OpenKit
{

    public class DynatraceOpenKitBuilderTest
    {
        private const string EndpointUrl = "https://localhost";
        private const string ApplicationId = "the-application-identiifer";
        private const string ApplicationName = "the-application-name";
        private const long DeviceId = 777;

        [Test]
        public void ConstructorInitializesApplicationId()
        {
            // given, when
            var target = new DynatraceOpenKitBuilder(EndpointUrl, ApplicationId, DeviceId);

            // then
            Assert.That(target.ApplicationId, Is.EqualTo(ApplicationId));
        }

        [Test]
        public void ConstructorInitializesDeviceIdString()
        {
            // given, when
            var target = new DynatraceOpenKitBuilder(EndpointUrl, ApplicationId, DeviceId.ToString());

            // then
            Assert.That(target.DeviceId, Is.EqualTo(DeviceId));
            Assert.That(target.OrigDeviceId, Is.EqualTo(DeviceId.ToString()));
        }

        [Test]
        public void OpenKitTypeGivesAppropriateValue()
        {
            // given, when
            var target = new DynatraceOpenKitBuilder(EndpointUrl, ApplicationId, DeviceId);

            // then
            Assert.That(target.OpenKitType, Is.EqualTo(DynatraceOpenKitBuilder.Type));
        }

        [Test]
        public void DefaultServerIdGivesAppropriateValue()
        {
            // given, when
            var target = new DynatraceOpenKitBuilder(EndpointUrl, ApplicationId, DeviceId);

            // then
            Assert.That(target.DefaultServerId, Is.EqualTo(DynatraceOpenKitBuilder.DefaultServerIdValue));
        }

        [Test]
        public void DefaultApplicationNameIsEmptyString()
        {
            // given
            var target = new DynatraceOpenKitBuilder(EndpointUrl, ApplicationId, DeviceId);

            // when
            var obtained = target.ApplicationName;

            // then
            Assert.That(obtained, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ApplicationNameGivesPreviouslySetApplicationName()
        {
            // given
            var target = new DynatraceOpenKitBuilder(EndpointUrl, ApplicationId, DeviceId);

            // when
            target.WithApplicationName(ApplicationName);
            var obtained = target.ApplicationName;

            // then
            Assert.That(obtained, Is.EqualTo(ApplicationName));
        }

        [Test]
        public void WithApplicationNameIgnoresNullAsArgument()
        {
            // given
            var target = new DynatraceOpenKitBuilder(EndpointUrl, ApplicationId, DeviceId);

            // when
            target.WithApplicationName(null);

            // then
            Assert.That(target.ApplicationName, Is.EqualTo(string.Empty));
        }
    }
}