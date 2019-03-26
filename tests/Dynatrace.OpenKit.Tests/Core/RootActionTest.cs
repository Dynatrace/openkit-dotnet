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
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core
{
    public class RootActionTest
    {
        private ITimingProvider mockTimingProvider;
        private ILogger logger;
        private Beacon beacon;

        [SetUp]
        public void SetUp()
        {
            mockTimingProvider = Substitute.For<ITimingProvider>();
            logger = Substitute.For<ILogger>();
            beacon = new Beacon(logger,
                                new BeaconCache(logger),
                                new TestConfiguration(),
                                "127.0.0.1",
                                Substitute.For<IThreadIDProvider>(),
                                mockTimingProvider);
        }

        [Test]
        public void EnterActionReturnsNewChildAction()
        {
            // given
            var target = new RootAction(logger, beacon, "root action", new SynchronizedQueue<IAction>());

            // when entering first child
            var childOne = target.EnterAction("child one");

            // then
            Assert.That(childOne, Is.Not.Null.And.TypeOf<Action>());
            Assert.That(((Action)childOne).ParentID, Is.EqualTo(target.ID));

            // when entering second child
            var childTwo = target.EnterAction("child one");

            // then
            Assert.That(childTwo, Is.Not.Null.And.TypeOf<Action>());
            Assert.That(((Action)childTwo).ParentID, Is.EqualTo(target.ID));
            Assert.That(childTwo, Is.Not.SameAs(childOne));
        }

        [Test]
        public void EnterActionReturnsNullActionIfAlreadyLeft()
        {
            // given
            var target = new RootAction(logger, beacon, "root action", new SynchronizedQueue<IAction>());
            target.LeaveAction();

            // when entering first child
            var childOne = target.EnterAction("child one");

            // then
            Assert.That(childOne, Is.Not.Null.And.TypeOf<NullAction>());

            // when entering second child
            var childTwo = target.EnterAction("child one");

            // then
            Assert.That(childTwo, Is.Not.Null.And.TypeOf<NullAction>());
            Assert.That(childTwo, Is.Not.SameAs(childOne));
        }

        [Test]
        public void EnterActionReturnsNullActionIfActionNameIsNull()
        {
            // given
            var target = new RootAction(logger, beacon, "root action", new SynchronizedQueue<IAction>());

            // when
            var childOne = target.EnterAction(null);

            // then
            Assert.That(childOne, Is.Not.Null.And.TypeOf<NullAction>());
            
            // also verify that warning has been written to log
            logger.Received(1).Warn("RootAction [sn=1, id=1, name=root action] EnterAction: actionName must not be null or empty");
        }

        [Test]
        public void EnterActionReturnsNullActionIfActionNameIsAnEmptyString()
        {
            // given
            var target = new RootAction(logger, beacon, "root action", new SynchronizedQueue<IAction>());

            // when
            var childOne = target.EnterAction(string.Empty);

            // then
            Assert.That(childOne, Is.Not.Null.And.TypeOf<NullAction>());

            // also verify that warning has been written to log
            logger.Received(1).Warn("RootAction [sn=1, id=1, name=root action] EnterAction: actionName must not be null or empty");
        }

        [Test]
        public void LeavingAParentActionWillLeaveAllOpenChildActionsFirst()
        {
            // given
            int startTimestamp = 100;
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(x =>
            {
                startTimestamp += 1;
                return startTimestamp;
            });
            var target = new RootAction(logger, beacon, "root action", new SynchronizedQueue<IAction>());

            var childActionOne = target.EnterAction("child one");
            var childActionTwo = target.EnterAction("child two");

            // when leaving the parent action
            target.LeaveAction();

            // then
            Assert.That(target.IsActionLeft, Is.True);
            Assert.That(((Action)childActionOne).IsActionLeft, Is.True);
            Assert.That(((Action)childActionTwo).IsActionLeft, Is.True);

            Assert.That(((Action)childActionTwo).EndTime, Is.EqualTo(((Action)childActionOne).EndTime + 2));
            Assert.That(target.EndTime, Is.EqualTo(((Action)childActionTwo).EndTime + 1));
        }

        [Test]
        public void LeavingRootActionReturnsNullValueAsParent()
        {
            // given
            var target = new RootAction(logger, beacon, "root action", new SynchronizedQueue<IAction>());

            // when
            var obtained = target.LeaveAction();

            // then
            Assert.That(obtained, Is.Null);
        }
    }
}
