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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Communication;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Objects
{
    public class LeafActionTest
    {
        private const string ActionName = "TestAction";
        private const int SessionId = 73;

        private ILogger logger;
        private RootAction rootAction;
        private Beacon beacon;

        [SetUp]
        public void SetUp()
        {
            logger = Substitute.For<ILogger>();

            var threadIdProvider = Substitute.For<IThreadIdProvider>();
            var timingProvider = Substitute.For<ITimingProvider>();

            var sessionIdProvider = Substitute.For<ISessionIdProvider>();
            sessionIdProvider.GetNextSessionId().Returns(SessionId);

            var configuration = new TestConfiguration(42, new BeaconConfiguration(), sessionIdProvider);

            beacon = new Beacon(logger, new BeaconCache(logger), configuration, "127.0.0.1", threadIdProvider, timingProvider);

            var context = Substitute.For<IBeaconSendingContext>();
            var beaconSender = new BeaconSender(logger, context);

            var session = new Session(logger, beaconSender, beacon);

            rootAction = new RootAction(logger, session, "Root action", beacon);
        }

        [Test]
        public void ParentActionReturnsValuePassedInConstructor()
        {
            // given
            var target = new LeafAction(logger, rootAction, ActionName, beacon);

            // when
            var obtained = target.ParentAction;

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.SameAs(rootAction));
        }

        [Test]
        public void ToStringReturnsAppropriateResult()
        {
            // given
            var target = new LeafAction(logger, rootAction, ActionName, beacon);

            // when
            var obtained = target.ToString();

            // then
            Assert.That(obtained, Is.EqualTo($"{typeof(LeafAction).Name} [sn={SessionId}, id=2, name={ActionName}, pa=1]"));
        }
    }
}