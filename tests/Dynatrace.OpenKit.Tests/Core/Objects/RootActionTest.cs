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
using Dynatrace.OpenKit.Protocol;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Objects
{
    public class RootActionTest
    {
        private const string RootActionName = "root action";
        private const string ChildActionName = "child action";

        private ILogger mockLogger;
        private IBeacon mockBeacon;
        private ISessionInternals mockSession;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
            mockBeacon = Substitute.For<IBeacon>();
            mockSession = Substitute.For<ISessionInternals>();
        }

        [Test]
        public void ParentActionReturnsNull()
        {
            // given
            var target = CreateRootAction();

            // when
            var obtained = target.ParentAction;

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void EnterActionWithNullNameGivesNullAction()
        {
            // given
            var target = CreateRootAction();

            // when
            var childOne = target.EnterAction(null);

            // then
            Assert.That(childOne, Is.Not.Null.And.TypeOf<NullAction>());

            // also verify that warning has been written to log
            mockLogger.Received(1).Warn($"{target} EnterAction: actionName must not be null or empty");
        }

        [Test]
        public void EnterActionWithEmptyNameGivesNullAction()
        {
            // given
            var target = CreateRootAction();

            // when
            var childOne = target.EnterAction(string.Empty);

            // then
            Assert.That(childOne, Is.Not.Null.And.TypeOf<NullAction>());

            // also verify that warning has been written to log
            mockLogger.Received(1).Warn($"{target} EnterAction: actionName must not be null or empty");
        }

        [Test]
        public void EnterActionReturnsLeafActionInstance()
        {
            // given
            var target = CreateRootAction();

            // when
            var obtained = target.EnterAction(ChildActionName);

            // then
            Assert.That(obtained, Is.Not.Null.And.TypeOf<LeafAction>());
        }

        [Test]
        public void EnterActionReturnsNewInstanceEvenIfSameName()
        {
            // given
            var target = CreateRootAction();

            // when
            var obtainedOne = target.EnterAction(ChildActionName);
            var obtainedTwo = target.EnterAction(ChildActionName);

            // then
            Assert.That(obtainedOne, Is.Not.Null.And.TypeOf<LeafAction>());
            Assert.That(obtainedTwo, Is.Not.Null.And.TypeOf<LeafAction>());
            Assert.That(obtainedOne, Is.Not.SameAs(obtainedTwo));
        }

        [Test]
        public void EnterActionInstanceHasCorrectParentId()
        {
            // given
            var target = CreateRootAction();

            // when
            var obtained = target.EnterAction(RootActionName);

            // then
            Assert.That(((LeafAction)obtained).ParentId, Is.EqualTo(target.Id));
        }

        [Test]
        public void EnterActionAddsLeafActionToListOfChildObjects()
        {
            // given
            var target = CreateRootAction();
            IOpenKitComposite targetComposite = target;

            // when
            var obtained = target.EnterAction(ChildActionName);

            // then
            var childObjects = targetComposite.GetCopyOfChildObjects();
            Assert.That(childObjects.Count, Is.EqualTo(1));
            Assert.That(childObjects[0], Is.SameAs(obtained));
        }

        [Test]
        public void EnterActionReturnsNullActionIfAlreadyLeft()
        {
            // given
            var target = CreateRootAction();
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
        public void ToStringReturnsAppropriateResult()
        {
            // given
            const int sessionNumber = 42;
            const int id = 73;
            mockBeacon.SessionNumber.Returns(sessionNumber);
            mockBeacon.NextId.Returns(id);
            var target = CreateRootAction();

            // when
            var obtained = target.ToString();

            // then
            Assert.That(obtained, Is.EqualTo($"{typeof(RootAction).Name} [sn={sessionNumber}, id={id}, name={RootActionName}]"));
        }

        private RootAction CreateRootAction()
        {
            return new RootAction(
                mockLogger,
                mockSession,
                RootActionName,
                mockBeacon
                );
        }
    }
}
