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

using Dynatrace.OpenKit.Core.Communication;
using Dynatrace.OpenKit.API;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core
{
    public class BeaconSenderTest
    {
        private IBeaconSendingContext context;

        [SetUp]
        public void SetUp()
        {
            context = Substitute.For<IBeaconSendingContext>();
        }

        [Test]
        public void IsInitializedGetsValueFromContext()
        {
            // given
            var logger = Substitute.For<ILogger>();
            var target = new BeaconSender(logger, context);

            // when context is not initialized
            context.IsInitialized.Returns(false);

            // then
            Assert.That(target.IsInitialized, Is.False);

            // and when context is initialized
            context.IsInitialized.Returns(true);

            // then
            Assert.That(target.IsInitialized, Is.True);
        }
    }
}
