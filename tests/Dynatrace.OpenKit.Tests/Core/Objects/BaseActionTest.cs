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

using System;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Objects
{
    public class BaseActionTest
    {
        private Beacon beacon;
        private OpenKitConfiguration testConfiguration;
        private ITimingProvider mockTimingProvider;
        private ILogger logger;
        private OpenKitComposite parentComposite;

        [SetUp]
        public void SetUp()
        {
            mockTimingProvider = Substitute.For<ITimingProvider>();
            testConfiguration = new TestConfiguration();
            logger = Substitute.For<ILogger>();
            beacon = new Beacon(logger,
                                new BeaconCache(logger),
                                testConfiguration,
                                "127.0.0.1",
                                Substitute.For<IThreadIdProvider>(),
                                mockTimingProvider);

            parentComposite = Substitute.For<OpenKitComposite>();
        }

        [Test]
        public void ReportEvent()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when reporting an event
            var obtained = target.ReportEvent("test event");

            // verify that beacon within the action is called properly
            Assert.That(beacon.IsEmpty, Is.False);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportEventDoesNothingIfEventNameIsNull()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when reporting an event
            var obtained = target.ReportEvent(null);

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.SameAs(target));

            // also verify that warning has been written to log
            logger.Received(1).Warn($"{target} ReportEvent: eventName must not be null or empty");
        }

        [Test]
        public void ReportEventDoesNothingIfEventNameIsEmpty()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when reporting an event
            var obtained = target.ReportEvent(string.Empty);

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.SameAs(target));

            // also verify that warning has been written to log
            logger.Received(1).Warn($"{target} ReportEvent: eventName must not be null or empty");
        }

        [Test]
        public void ReportValueIntWithNullNameDoesNotReportValue()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when
            const int value = 42;
            var obtained = target.ReportValue(null, value);

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.SameAs(target));

            // also verify that warning has been written to log
            logger.Received(1).Warn($"{target} ReportValue (int): valueName must not be null or empty");
        }

        [Test]
        public void ReportValueIntWithEmptyNameDoesNotReportValue()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when
            const int value = 42;
            var obtained = target.ReportValue(string.Empty, value);

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.SameAs(target));

            // also verify that warning has been written to log
            logger.Received(1).Warn($"{target} ReportValue (int): valueName must not be null or empty");
        }

        [Test]
        public void ReportValueIntWithValidValue()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when reporting an event
            const int value = 42;
            var obtained = target.ReportValue("intValue", value);

            // then
            Assert.That(beacon.IsEmpty, Is.False);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportValueDoubleWithNullNameDoesNotReportValue()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when
            const double value = 42.125;
            var obtained = target.ReportValue(null, value);

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.SameAs(target));

            // also verify that warning has been written to log
            logger.Received(1).Warn($"{target} ReportValue (double): valueName must not be null or empty");
        }

        [Test]
        public void ReportValueDoubleWithEmptyNameDoesNotReportValue()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when
            const double value = 42.25;
            var obtained = target.ReportValue(string.Empty, value);

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.SameAs(target));

            // also verify that warning has been written to log
            logger.Received(1).Warn($"{target} ReportValue (double): valueName must not be null or empty");
        }

        [Test]
        public void ReportValueDoubleWithValidValue()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when reporting an event
            const double value = 42.5;
            var obtained = target.ReportValue("doubleValue", value);

            // then
            Assert.That(beacon.IsEmpty, Is.False);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportValueStringWithValidValue()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when reporting an event
            const string value = "some value";
            var obtained = target.ReportValue("doubleValue", value);

            // then
            Assert.That(beacon.IsEmpty, Is.False);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportValueStringWithNullNameDoesNotReportValue()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when
            const string value = "42";
            var obtained = target.ReportValue(null, value);

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.SameAs(target));

            // also verify that warning has been written to log
            logger.Received(1).Warn($"{target} ReportValue (string): valueName must not be null or empty");
        }

        [Test]
        public void ReportValueStringWithEmptyNameDoesNotReportValue()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when
            const string value = "42";
            var obtained = target.ReportValue(string.Empty, value);

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.SameAs(target));

            // also verify that warning has been written to log
            logger.Received(1).Warn($"{target} ReportValue (string): valueName must not be null or empty");
        }

        [Test]
        public void ReportValueStringWithValueNull()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when
            var obtained = target.ReportValue("valueName", null);

            // then
            Assert.That(beacon.IsEmpty, Is.False);
            Assert.That(obtained, Is.SameAs(target));
        }

            [Test]
        public void ReportErrorWithAllValuesSet()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when reporting an event
            var obtained = target.ReportError("teapot", 418, "I'm a teapot");

            // then
            Assert.That(beacon.IsEmpty, Is.False);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportErrorWithNullErrorNameDoesNotReportTheError()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when
            var obtained = target.ReportError(null, 418, "I'm a teapot");

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.SameAs(target));

            // also verify that warning has been written to log
            logger.Received(1).Warn($"{target} ReportError: errorName must not be null or empty");
        }

        [Test]
        public void ReportErrorWithEmptyErrorNameDoesNotReportTheError()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when
            var obtained = target.ReportError(string.Empty, 418, "I'm a teapot");

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.SameAs(target));

            // also verify that warning has been written to log
            logger.Received(1).Warn($"{target} ReportError: errorName must not be null or empty");
        }

        [Test]
        public void ReportErrorWithNullErrorReasonDesReport()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when
            var obtained = target.ReportError("errorName", 418, null);

            // then
            Assert.That(beacon.IsEmpty, Is.False);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void TraceWebRequestGivesValidWebRequestTracer()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when
            var obtained = target.TraceWebRequest("http://example.com/pages/");

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<WebRequestTracer>());
        }

        [Test]
        public void TraceWebRequestAttachesWebRequestTracerAsChildObject()
        {
            // given
            var target = CreateStubAction();

            // when
            var obtained = target.TraceWebRequest("http://example.com/pages/");

            // then
            var childObjects = target.GetCopyOfChildObjects();
            Assert.That(childObjects.Count, Is.EqualTo(1));
            Assert.That(childObjects[0], Is.SameAs(obtained));
        }

        [Test]
        public void TracingANullStringWebRequestIsNotAllowed()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when
            var obtained = target.TraceWebRequest(null);

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<NullWebRequestTracer>());

            // also verify that warning has been written to log
            logger.Received(1).Warn($"{target} TraceWebRequest (String): url must not be null or empty");
        }

        [Test]
        public void TracingAnEmptyStringWebRequestIsNotAllowed()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when
            var obtained = target.TraceWebRequest(string.Empty);

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<NullWebRequestTracer>());

            // also verify that warning has been written to log
            logger.Received(1).Warn($"{target} TraceWebRequest (String): url must not be null or empty");
        }

        [Test]
        public void TracingAStringWebRequestWithInvalidUrlIsNotAllowed()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when
            var obtained = target.TraceWebRequest("foo:bar://test.com");

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<NullWebRequestTracer>());

            // also verify that warning has been written to log
            logger.Received(1).Warn($"{target} TraceWebRequest (String): url \"foo:bar://test.com\" does not have a valid scheme");
        }

        [Test]
        public void ParentIdIsZeroIfActionHasNoParent()
        {
            // given
            var target = CreateStubAction();

            // then
            Assert.That(target.ParentId, Is.EqualTo(0));
        }

        [Test]
        public void ParentActionIdIsInitializedInTheConstructor()
        {
            // given
            const int parentActionId = 37;
            parentComposite.ActionId.Returns(parentActionId);

            // when
            var obtained = CreateStubAction();
            beacon.ClearData();

            // then
            Assert.That(obtained.ParentId, Is.EqualTo(parentActionId));
        }

        [Test]
        public void IdIsInitializedInTheConstructor()
        {
            // given
            while (beacon.NextId < 10) {} // increment the id
            var target = CreateStubAction();

            // then
            Assert.That(target.Id, Is.EqualTo(11));
        }

        [Test]
        public void NameIsInitializedInTheConstructor()
        {
            // given
            const string name = "test name";
            var target = CreateStubAction(name);

            // then
            Assert.That(target.Name, Is.EqualTo(name));
        }

        [Test]
        public void StartTimeIsInitializedInTheConstructor()
        {
            // given
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(1234L);
            var target = CreateStubAction();

            // then
            Assert.That(target.StartTime, Is.EqualTo(1234L));
        }


        [Test]
        public void EndTimeIsMinusOneForNewlyCreatedAction()
        {
            // given
            var target = CreateStubAction();

            // then
            Assert.That(target.EndTime, Is.EqualTo(-1L));
        }

        [Test]
        public void StartSequenceNumberIsInitializedInTheConstructor()
        {
            // given
            while (beacon.NextSequenceNumber < 5) {} // increment sequence number
            var target = CreateStubAction();

            // then
            Assert.That(target.StartSequenceNo, Is.EqualTo(6));
        }

        [Test]
        public void EndSequenceNumberIsMinusOneForNewlyCreatedAction()
        {
            // given
            while (beacon.NextSequenceNumber < 5) {} // increment sequence number
            var target = CreateStubAction();

            // then
            Assert.That(target.EndSequenceNo, Is.EqualTo(-1));
        }

        [Test]
        public void ANewlyCreatedActionIsNotLeft()
        {
            // given
            var target = CreateStubAction();

            // then
            Assert.That(target.IsActionLeft, Is.False);
        }

        [Test]
        public void AfterLeavingAnActionItIsLeft()
        {
            // given
            var target = CreateStubAction();

            // when
            target.LeaveAction();

            // then
            Assert.That(target.IsActionLeft, Is.True);
        }

        [Test]
        public void LeavingAnActionSetsTheEndTimestamp()
        {
            // given
            const long endTime = 999;
            var target = CreateStubAction();
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(endTime);

            // when leaving the action
            target.LeaveAction();

            // then
            Assert.That(target.EndTime, Is.EqualTo(endTime));
        }


        [Test]
        public void LeavingAnActionSetsTheEndSequenceNumber()
        {
            // given
            var target = CreateStubAction();
            while (beacon.NextSequenceNumber < 41) {} // increase the sequence number

            // when leaving the action
            target.LeaveAction();

            // then
            Assert.That(target.EndSequenceNo, Is.EqualTo(42));
        }

        [Test]
        public void LeavingAnActionSerializesItself()
        {
            // given
            var target = CreateStubAction();
            beacon.ClearData();

            // when leaving the action
            target.LeaveAction();

            // then
            Assert.That(beacon.IsEmpty, Is.False);
        }

        [Test]
        public void LeavingAnActionClosesAllChildObjects()
        {
            // given
            var childObjectOne = Substitute.For<IOpenKitObject>();
            var childObjectTwo = Substitute.For<IOpenKitObject>();

            var target = CreateStubAction();
            target.StoreChildInList(childObjectOne);
            target.StoreChildInList(childObjectTwo);

            // when
            target.LeaveAction();

            // then
            childObjectOne.Received(1).Dispose();
            childObjectTwo.Received(1).Dispose();
        }

        [Test]
        public void LeaveActionNotifiesTheParentCompositeObject()
        {
            // given
            var target = CreateStubAction();

            // when
            target.LeaveAction();

            // then
            parentComposite.Received(1).OnChildClosed(target);
        }

        [Test]
        public void LeavingAnActionReturnsTheParentAction()
        {
            // given
            var parent = CreateStubAction();
            var child = CreateStubAction("test", parent);

            // when leaving the child action
            var obtained = child.LeaveAction();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.SameAs(parent));
        }

        [Test]
        public void LeavingAnAlreadyLeftActionReturnsTheParentAction()
        {
            // given
            var parentAction = Substitute.For<IAction>();
            var target = CreateStubAction("test", parentAction);
            target.LeaveAction();

            // when
            var obtained = target.LeaveAction();

            // then
            Assert.That(obtained, Is.SameAs(parentAction));
        }

        [Test]
        public void LeavingAnAlreadyLeftActionReturnsImmediately()
        {
            // given
            const long endTime = 999;
            var parent = Substitute.For<IAction>();
            var target = CreateStubAction("test", parent);
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(endTime);

            var sequenceNumber = beacon.NextSequenceNumber + 1;

            // when leaving the action first time
            var obtained = target.LeaveAction();

            // then
            Assert.That(obtained, Is.SameAs(parent));
            Assert.That(target.EndTime, Is.EqualTo(endTime));
            Assert.That(target.EndSequenceNo, Is.EqualTo(sequenceNumber));
            mockTimingProvider.Received(3).ProvideTimestampInMilliseconds();

            // and when leaving the action second time
            obtained = target.LeaveAction();

            // then
            Assert.That(obtained, Is.SameAs(parent));
            Assert.That(target.EndTime, Is.EqualTo(endTime));
            Assert.That(target.EndSequenceNo, Is.EqualTo(sequenceNumber));
            mockTimingProvider.Received(3).ProvideTimestampInMilliseconds();
        }

        [Test]
        public void ReportEventDoesNothingIfActionIsLeft()
        {
            // given
            var target = CreateStubAction();
            target.LeaveAction();
            beacon.ClearData();

            // when reporting an event
            var obtained = target.ReportEvent("test event");

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportIntValueDoesNothingIfActionIsLeft()
        {
            // given
            var target = CreateStubAction();
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
        public void ReportDoubleValueDoesNothingIfActionIsLeft()
        {
            // given
            var target = CreateStubAction();
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
        public void ReportStringValueDoesNothingIfActionIsLeft()
        {
            // given
            var target = CreateStubAction();
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
        public void ReportErrorDoesNothingIfActionIsLeft()
        {
            // given
            var target = CreateStubAction();
            target.LeaveAction();
            beacon.ClearData();

            // when
            var obtained = target.ReportError("teapot", 418, "I'm a teapot");

            // then
            Assert.That(beacon.IsEmpty, Is.True);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void TraceWebRequestGivesNullWebRequestTracerIfActionIsAlreadyLeft()
        {
            // given
            var target = CreateStubAction();
            target.LeaveAction();
            beacon.ClearData();

            // when
            var obtained = target.TraceWebRequest("http://example.com/pages/");

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<NullWebRequestTracer>());
        }


        [Test]
        public void DisposingAnActionLeavesTheAction()
        {
            // given
            const long endTime = 999;
            IDisposable target = CreateStubAction();
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(endTime);
            while (beacon.NextSequenceNumber < 41) {} // increase the sequence number

            // when disposing the target
            target.Dispose();

            // then
            Assert.That(((BaseAction)target).EndSequenceNo, Is.EqualTo(42));
            Assert.That(((BaseAction)target).EndTime, Is.EqualTo(endTime));
        }

        private StubBaseAction CreateStubAction(string name = "test", IAction parentAction = null)
        {
            return new StubBaseAction(
                logger,
                parentComposite,
                name,
                beacon,
                parentAction
            );
        }

        private  class StubBaseAction : BaseAction
        {
            internal StubBaseAction(ILogger logger, OpenKitComposite parentComposite, string name, Beacon beacon,
                IAction parentAction)
                : base(logger, parentComposite, name, beacon)
            {
                ParentAction = parentAction;
            }

            internal override IAction ParentAction { get; }
        }
    }
}