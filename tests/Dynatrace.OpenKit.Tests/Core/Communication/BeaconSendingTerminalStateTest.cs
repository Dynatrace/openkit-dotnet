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

namespace Dynatrace.OpenKit.Core.Communication
{
    public class BeaconSendingTerminalStateTest
    {
        private IHTTPClient httpClient;
        private IBeaconSendingContext context;

        [SetUp]
        public void Setup()
        {
            httpClient = Substitute.For<IHTTPClient>();
            context = Substitute.For<IBeaconSendingContext>();
        }

        [Test]
        public void StateIsTerminal()
        {
            // given
            var target = new BeaconSendingTerminalState();

            // then
            Assert.That(target.IsTerminalState, Is.True);
        }

        [Test]
        public void TerminalStateIsNextStateIfTransitionFromTerminalStateOccurs()
        {
            //given
            var target = new BeaconSendingTerminalState();

            //then
            Assert.That(target.ShutdownState, Is.SameAs(target));
        }

        [Test]
        public void TerminalStateCallsShutdownOnExecution()
        {
            //given
            var target = new BeaconSendingTerminalState();

            // when
            target.Execute(context);

            // then
            context.Received(1).RequestShutdown();
        }
    }
}
