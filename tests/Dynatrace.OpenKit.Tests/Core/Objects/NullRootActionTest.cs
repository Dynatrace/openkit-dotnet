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

using NUnit.Framework;
using System;

namespace Dynatrace.OpenKit.Core.Objects
{
    public class NullRootActionTest
    {
        [Test]
        public void EnterActionReturnsNewNullAction()
        {
            // given
            var target = NullRootAction.Instance;

            // when
            var obtained = target.EnterAction("action name");

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<NullAction>());
        }

        [Test]
        public void EnterActionHasNullRotActionAsParent()
        {
            // given
            var target = NullRootAction.Instance;

            // when
            var action = target.EnterAction("action name");
            var obtained = action.LeaveAction();

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<NullRootAction>());
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportEventReturnsSelf()
        {
            // given
            var target = NullRootAction.Instance;

            // when
            var obtained = target.ReportEvent("event name");

            // then
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportIntValueReturnsSelf()
        {
            // given
            var target = NullRootAction.Instance;

            // when
            var obtained = target.ReportValue("value name", 12);

            // then
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportLongValueReturnsSelf()
        {
            // given
            var target = NullRootAction.Instance;

            // when
            var obtained = target.ReportValue("value name", long.MaxValue);

            // then
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportDoubleValueReturnsSelf()
        {
            // given
            var target = NullRootAction.Instance;

            // when
            var obtained = target.ReportValue("value name", 37.73);

            // then
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportStringValueReturnsSelf()
        {
            // given
            var target = NullRootAction.Instance;

            // when
            var obtained = target.ReportValue("value name", "value");

            // then
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportErrorReturnsSelf()
        {
            // given
            var target = NullRootAction.Instance;

            // when
            var obtained = target.ReportError("error name", 1337, "something bad");

            // then
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void TraceWebRequestReturnsNullWebRequestTracer()
        {
            // given
            var target = NullRootAction.Instance;

            // when
            var obtained = target.TraceWebRequest("https://localhost");

            // then
            Assert.That(obtained, Is.SameAs(NullWebRequestTracer.Instance));
        }

        [Test]
        public void LeaveActionReturnsNull()
        {
            // given
            var target = NullRootAction.Instance;

            // when
            var obtained = target.LeaveAction();

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void CancelActionReturnsNull()
        {
            // given
            var target = NullRootAction.Instance;

            // when
            var obtained = target.CancelAction();

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void getDurationInMillisecondsReturnsZero()
        {
            // given
            var target = NullRootAction.Instance;

            // when
            var obtained = target.Duration;

            // then
            Assert.That(obtained, Is.EqualTo(TimeSpan.Zero));
        }
    }
}