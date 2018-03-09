//
// Copyright 2018 Dynatrace LLC
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
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core
{
    public class ActionTest
    {
        private Beacon beacon;
        private OpenKitConfiguration testConfiguration;
        private ITimingProvider mockTimingProvider;
        
        [SetUp]
        public void SetUp()
        {
            mockTimingProvider = Substitute.For<ITimingProvider>();
            testConfiguration = new TestConfiguration();
            beacon = new Beacon(Substitute.For<ILogger>(),
                                new BeaconCache(),
                                testConfiguration,
                                "127.0.0.1",
                                Substitute.For<IThreadIDProvider>(),
                                mockTimingProvider);
        }

        [Test]
        public void ANewlyCreatedActionHasAnID()
        {
            // given
            while (beacon.NextID < 10) ; // increment the id
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());

            // then
            Assert.That(target.ID, Is.EqualTo(11));
        }

        [Test]
        public void NameIsSet()
        {
            // given
            var target = new Action(beacon, "test name", new SynchronizedQueue<IAction>());

            // then
            Assert.That(target.Name, Is.EqualTo("test name"));
        }

        [Test]
        public void ParentIDIsZeroIfActionHasNoParent()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());

            // then
            Assert.That(target.ParentID, Is.EqualTo(0));
        }

        [Test]
        public void ParentIDIsEqualToParentActionsID()
        {
            // given
            while (beacon.NextID < 10) ; // increment the id
            var parent = new Action(beacon, "parent", new SynchronizedQueue<IAction>());
            var target = new Action(beacon, "test", parent, new SynchronizedQueue<IAction>());

            // then
            Assert.That(target.ParentID, Is.EqualTo(11));
        }

        [Test]
        public void StartTimeIsSetInConstructor()
        {
            // given
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(1234L);
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());

            // then
            Assert.That(target.StartTime, Is.EqualTo(1234L));
        }

        [Test]
        public void EndTimeIsSetToMinusOneInConstructor()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());

            // then
            Assert.That(target.EndTime, Is.EqualTo(-1L));
        }

        [Test]
        public void StartSequenceNumberIsSetInConstructor()
        {
            // given
            while (beacon.NextSequenceNumber < 5) ; // increment sequence number
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());

            // then
            Assert.That(target.StartSequenceNo, Is.EqualTo(6));
        }

        [Test]
        public void EndSequenceNumberIsSetToMinusOne()
        {
            // given
            while (beacon.NextSequenceNumber < 5) ; // increment sequence number
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());

            // then
            Assert.That(target.EndSequenceNo, Is.EqualTo(-1));
        }

        [Test]
        public void ANewlyCreatedActionIsNotLeft()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());

            // then
            Assert.That(target.IsActionLeft, Is.False);
        }

        [Test]
        public void AnActionAddsItselfIntoListInContstructor()
        {
            // given
            var actions = new SynchronizedQueue<IAction>();

            // when constructing the action
            var target = new Action(beacon, "test", actions);

            // then
            Assert.That(actions.IsEmpty, Is.False);
            Assert.That(actions.ToList(), Is.EqualTo(new List<Action> { target }));
        }

        [Test]
        public void AfterLeavingAnActionItIsLeft()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());

            // when
            target.LeaveAction();

            // then
            Assert.That(target.IsActionLeft, Is.True);
        }

        [Test]
        public void ReportEvent()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());
            beacon.ClearData();

            // when reporting an event
            var obtained = target.ReportEvent("test event");

            // verify that beacon within the action is called properly
            Assert.That(beacon.IsEmpty, Is.False);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportEventDoesNothingIfActionIsLeft()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());
            target.LeaveAction();
            beacon.ClearData();

            // when reporting an event
            var obtained = target.ReportEvent("test event");

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportIntValue()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());
            beacon.ClearData();

            // when reporting an event
            const int value = 42;
            var obtained = target.ReportValue("intValue", value);

            // then
            Assert.That(beacon.IsEmpty, Is.False);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportIntValueDoesNothingIfActionIsLeft()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());
            target.LeaveAction();
            beacon.ClearData();

            // when
            const int value = 42;
            var obtained = target.ReportValue("intValue", value);

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportDoubleValue()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());
            beacon.ClearData();

            // when reporting an event
            const double value = 42.5;
            var obtained = target.ReportValue("doubleValue", value);

            // then
            Assert.That(beacon.IsEmpty, Is.False);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportDoubleValueDoesNothingIfActionIsLeft()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());
            target.LeaveAction();
            beacon.ClearData();

            // when
            const double value = 42.5;
            var obtained = target.ReportValue("doubleValue", value);

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportStringValue()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());
            beacon.ClearData();

            // when reporting an event
            const string value = "some value";
            var obtained = target.ReportValue("doubleValue", value);

            // then
            Assert.That(beacon.IsEmpty, Is.False);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportStringValueDoesNothingIfActionIsLeft()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());
            target.LeaveAction();
            beacon.ClearData();

            // when
            const string value = "some value";
            var obtained = target.ReportValue("doubleValue", value);

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportError()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());
            beacon.ClearData();

            // when reporting an event
            var obtained = target.ReportError("teapot", 418, "I'm a teapot");

            // then
            Assert.That(beacon.IsEmpty, Is.False);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportErrorDoesNothingIfActionIsLeft()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());
            target.LeaveAction();
            beacon.ClearData();

            // when
            var obtained = target.ReportError("teapot", 418, "I'm a teapot");

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void TraceWebRequestGivesValidWebRequestTracer()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());
            beacon.ClearData();

            // when
            var obtained = target.TraceWebRequest("http://example.com/pages/");

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<WebRequestTracerStringURL>());
        }

        [Test]
        public void TraceWebRequestGivesNullWebRequestTracerIfActionIsAlreadyLeft()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());
            target.LeaveAction();
            beacon.ClearData();

            // when
            var obtained = target.TraceWebRequest("http://example.com/pages/");

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<NullWebRequestTracer>());
        }

        [Test]
        public void LeavingAnActionReturnsTheParentAction()
        {
            // given
            var parent = new Action(beacon, "parent", new SynchronizedQueue<IAction>());
            var child = new Action(beacon, "test", parent, new SynchronizedQueue<IAction>());

            // when leaving the child action
            var obtained = child.LeaveAction();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.SameAs(parent));

            // and when leaving the parent
            obtained = parent.LeaveAction();

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void LeavingAnActionSetsTheEndTimestamp()
        {
            // given
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(123L, 321L, 322L, 323L);
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());

            // when leaving the action
            target.LeaveAction();

            // then
            Assert.That(target.EndTime, Is.EqualTo(322L));
            mockTimingProvider.Received(4).ProvideTimestampInMilliseconds();
        }

        [Test]
        public void LeavingAnActionSetsTheEndSequenceNumber()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());
            while (beacon.NextSequenceNumber < 41) ; // increase the sequence number

            // when leaving the action
            target.LeaveAction();

            // then
            Assert.That(target.EndSequenceNo, Is.EqualTo(42));
        }

        [Test]
        public void LeavingAnActionAddsItselfInTheBeacon()
        {
            // given
            var target = new Action(beacon, "test", new SynchronizedQueue<IAction>());
            beacon.ClearData();

            // when leaving the action
            target.LeaveAction();

            // then
            Assert.That(beacon.IsEmpty, Is.False);
        }

        [Test]
        public void LeavingAnActionRemovesItselfFromListPassedInCtor()
        {
            // given
            var actions = new SynchronizedQueue<IAction>();
            var target = new Action(beacon, "test", actions);

            // when leaving the action
            target.LeaveAction();

            // then
            Assert.That(actions.IsEmpty, Is.True);
        }

        [Test]
        public void AnActionCanOnlyBeLeftOnce()
        {
            // given
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(123L, 321L, 322L, 323L, 324L, 325L);
            var parent = new Action(beacon, "parent", new SynchronizedQueue<IAction>());
            var target = new Action(beacon, "test", parent, new SynchronizedQueue<IAction>());

            // when leaving the action first time
            var obtained = target.LeaveAction();

            // then
            Assert.That(obtained, Is.SameAs(parent));
            Assert.That(target.EndTime, Is.EqualTo(323L));
            Assert.That(target.EndSequenceNo, Is.EqualTo(3));
            mockTimingProvider.Received(5).ProvideTimestampInMilliseconds();

            // and when leaving the action second time
            obtained = target.LeaveAction();

            // then
            Assert.That(obtained, Is.SameAs(parent));
            Assert.That(target.EndTime, Is.EqualTo(323L));
            Assert.That(target.EndSequenceNo, Is.EqualTo(3));
            mockTimingProvider.Received(6).ProvideTimestampInMilliseconds();
        }

        [Test]
        public void DisposingAnActionLeavesTheAction()
        {
            // given
            var actions = new SynchronizedQueue<IAction>();
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(123L, 321L, 322L, 323L);
            IDisposable target = new Action(beacon, "test", actions);
            while (beacon.NextSequenceNumber < 41) ; // increase the sequence number

            // when disposing the target
            target.Dispose();

            // then
            Assert.That(actions.IsEmpty, Is.True);
            Assert.That(((Action)target).EndSequenceNo, Is.EqualTo(42));
            Assert.That(((Action)target).EndTime, Is.EqualTo(322L));
        }
    }
}
