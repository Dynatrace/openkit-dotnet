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

using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Communication
{
    public class BeaconSendingTerminalStateTest
    {
        private IBeaconSendingContext mockContext;

        [SetUp]
        public void Setup()
        {
            mockContext = Substitute.For<IBeaconSendingContext>();
        }

        [Test]
        public void IsTerminalStateIsTrueForTheTerminalState()
        {
            // given
            var target = new BeaconSendingTerminalState();

            // then
            Assert.That(target.IsTerminalState, Is.True);
        }

        [Test]
        public void TheShutdownStateIsAlwaysTheSameReference()
        {
            //given
            var target = new BeaconSendingTerminalState();

            // when
            var obtained = target.ShutdownState;

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ToStringReturnsTheStateName()
        {
            // given
            var target = new BeaconSendingTerminalState();

            // when, then
            Assert.That(target.ToString(), Is.EqualTo("Terminal"));
        }

        [Test]
        public void ExecuteRequestsShutdown()
        {
            //given
            var target = new BeaconSendingTerminalState();

            // when
            target.Execute(mockContext);

            // then
            mockContext.Received(1).RequestShutdown();
            _ = mockContext.Received(2).IsShutdownRequested;
        }
    }
}
