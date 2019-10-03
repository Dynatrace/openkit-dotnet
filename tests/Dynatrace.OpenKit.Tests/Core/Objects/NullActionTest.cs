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
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Objects
{
    public class NullActionTest
    {
        private IAction mockParent;

        [SetUp]
        public void SetUp()
        {
            mockParent = Substitute.For<IAction>();
        }

        [Test]
        public void CreateNewInstance()
        {
            // given, when
            var target = CreateNullAction();

            Assert.That(target, Is.Not.Null);
            Assert.That(mockParent.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ReportEventReturnsSelf()
        {
            // given
            var target = CreateNullAction();

            // when
            var obtained = target.ReportEvent("event name");

            // then
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportIntValueReturnsSelf()
        {
            // given
            var target = CreateNullAction();

            // when
            var obtained = target.ReportValue("value name", 12);

            // then
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportDoubleValueReturnsSelf()
        {
            // given
            var target = CreateNullAction();

            // when
            var obtained = target.ReportValue("value name", 37.73);

            // then
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportStringValueReturnsSelf()
        {
            // given
            var target = CreateNullAction();

            // when
            var obtained = target.ReportValue("value name", "value");

            // then
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportErrorReturnsSelf()
        {
            // given
            var target = CreateNullAction();

            // when
            var obtained = target.ReportError("error name", 1337, "something bad");

            // then
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void TraceWebRequestReturnsNullWebRequestTracer()
        {
            // given
            var target = CreateNullAction();

            // when
            var obtained = target.TraceWebRequest("https://localhost");

            // then
            Assert.That(obtained, Is.SameAs(NullWebRequestTracer.Instance));
        }

        [Test]
        public void LeaveActionReturnsParentAction()
        {
            // given
            var target = CreateNullAction();

            // when
            var obtained = target.LeaveAction();

            // then
            Assert.That(obtained, Is.SameAs(mockParent));
        }

        [Test]
        public void LeaveActionWithNullParent()
        {
            // given
            var target = new NullAction(null);

            // when
            var obtained = target.LeaveAction();

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void DisposeDoesNothing()
        {
            // given
            var target = CreateNullAction();

            // when
            target.Dispose();

            // then
            Assert.That(mockParent.ReceivedCalls(), Is.Empty);
        }

        private NullAction CreateNullAction()
        {
            return new NullAction(mockParent);
        }
    }
}