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
using Dynatrace.OpenKit.Protocol;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Objects
{
    public class LeafActionTest
    {
        private const string ActionName = "TestAction";
        private const int SessionId = 73;

        private ILogger mockLogger;
        private IBeacon mockBeacon;
        private IRootActionInternals mockRootAction;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
            mockLogger.IsInfoEnabled.Returns(true);
            mockLogger.IsDebugEnabled.Returns(true);

            mockBeacon = Substitute.For<IBeacon>();
            mockRootAction = Substitute.For<IRootActionInternals>();
        }

        [Test]
        public void ParentActionReturnsValuePassedInConstructor()
        {
            // given
            var target = CreateLeafAction();

            // when
            var obtained = target.ParentAction;

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.SameAs(mockRootAction));
        }

        [Test]
        public void ToStringReturnsAppropriateResult()
        {
            // given
            const int id = 42;
            const int parentId = 37;
            mockBeacon.NextId.Returns(id);
            mockBeacon.SessionNumber.Returns(SessionId);
            mockBeacon.SessionSequenceNumber.Returns(0);
            mockRootAction.ActionId.Returns(parentId);

            var target = CreateLeafAction();

            // when
            var obtained = target.ToString();

            // then
            Assert.That(obtained, Is.EqualTo($"{typeof(LeafAction).Name} [sn={SessionId}, seq=0, id={id}, name={ActionName}, pa={parentId}]"));
        }

        private LeafAction CreateLeafAction()
        {
            return new LeafAction(
                mockLogger,
                mockRootAction,
                ActionName,
                mockBeacon
                );
        }
    }
}