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
using Dynatrace.OpenKit.Protocol;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Objects
{
    public class BaseActionTest
    {
        private const string ActionName = "TestAction";
        private const int IdBaseOffset = 124;

        private ILogger mockLogger;
        private IBeacon mockBeacon;
        private IOpenKitComposite parentComposite;

        private int nextBeaconId;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
            mockLogger.IsDebugEnabled.Returns(true);
            mockLogger.IsDebugEnabled.Returns(true);

            nextBeaconId = IdBaseOffset;
            mockBeacon = Substitute.For<IBeacon>();
            mockBeacon.NextId.Returns(_ => nextBeaconId++);

            parentComposite = Substitute.For<IOpenKitComposite>();
        }

        [Test]
        public void ReportEvent()
        {
            // given
            const string eventName = "TestEvent";
            var target = CreateStubAction();

            // when
            var obtained = target.ReportEvent(eventName);

            // then
            Assert.That(obtained, Is.SameAs(target));
            mockBeacon.Received(1).ReportEvent(IdBaseOffset, eventName);
        }

        [Test]
        public void ReportEventDoesNothingIfEventNameIsNull()
        {
            // given
            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportEvent(null);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportEvent: eventName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportEventDoesNothingIfEventNameIsEmpty()
        {
            // given
            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportEvent(string.Empty);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportEvent: eventName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportEventLogsInvocation()
        {
            // given
            const string eventName = "event name";
            var target = CreateStubAction();

            // when
            target.ReportEvent(eventName);

            // then
            mockLogger.Received(1).Debug($"{target} ReportEvent({eventName})");
        }

        [Test]
        public void ReportValueIntWithNullNameDoesNotReportValue()
        {
            // given
            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            const int value = 42;
            var obtained = target.ReportValue(null, value);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportValue (int): valueName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportValueIntWithEmptyNameDoesNotReportValue()
        {
            // given
            const int value = 42;
            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportValue(string.Empty, value);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportValue (int): valueName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportValueIntWithValidValue()
        {
            // given
            const int value = 42;
            const string valueName = "intValue";
            var target = CreateStubAction();

            // when
            var obtained = target.ReportValue(valueName, value);

            // then
            Assert.That(obtained, Is.SameAs(target));
            mockBeacon.Received(1).ReportValue(IdBaseOffset, valueName, value);
        }

        [Test]
        public void ReportValueIntLogsInvocation()
        {
             // given
            const int value = 42;
            const string valueName = "intValue";
            var target = CreateStubAction();

            // when
            target.ReportValue(valueName, value);

            // then
            mockLogger.Received(1).Debug($"{target} ReportValue (int) ({valueName}, {value})");
        }

        [Test]
        public void ReportValueDoubleWithNullNameDoesNotReportValue()
        {
            // given
            const double value = 42.125;
            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportValue(null, value);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportValue (double): valueName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportValueDoubleWithEmptyNameDoesNotReportValue()
        {
            // given
            const double value = 42.25;
            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportValue(string.Empty, value);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportValue (double): valueName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportValueDoubleWithValidValue()
        {
            // given
            const string valueName = "doubleValue";
            const double value = 42.5;
            var target = CreateStubAction();

            // when
            var obtained = target.ReportValue(valueName, value);

            // then
            Assert.That(obtained, Is.SameAs(target));
            mockBeacon.Received(1).ReportValue(IdBaseOffset, valueName, value);
        }

        [Test]
        public void ReportValueDoubleLogsInvocation()
        {
            // given
            const string valueName = "doubleValue";
            const double value = 42.5;
            var target = CreateStubAction();

            // when
            target.ReportValue(valueName, value);

            // then
           mockLogger.Debug($"{target} ReportValue (double) ({valueName}, {value})");
        }

        [Test]
        public void ReportValueStringWithValidValue()
        {
            // given
            const string valueName = "stringValue";
            const string value = "some value";
            var target = CreateStubAction();

            // when
            var obtained = target.ReportValue(valueName, value);

            // then
            Assert.That(obtained, Is.SameAs(target));
            mockBeacon.Received(1).ReportValue(IdBaseOffset, valueName, value);
        }

        [Test]
        public void ReportValueStringWithNullNameDoesNotReportValue()
        {
            // given
            const string value = "42";
            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportValue(null, value);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportValue (string): valueName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportValueStringWithEmptyNameDoesNotReportValue()
        {
            // given
            const string value = "42";
            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportValue(string.Empty, value);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportValue (string): valueName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportValueStringWithValueNull()
        {
            // given
            const string valueName = "valueName";
            const string value = null;
            var target = CreateStubAction();

            // when
            var obtained = target.ReportValue(valueName, value);

            // then
            Assert.That(obtained, Is.SameAs(target));
            mockBeacon.Received(1).ReportValue(IdBaseOffset, valueName, value);
        }

        [Test]
        public void ReportValueStringLogsInvocation()
        {
            // given
            const string valueName = "valueName";
            const string value = "value";
            var target = CreateStubAction();

            // when
            var obtained = target.ReportValue(valueName, value);

            // then
            mockLogger.Received(1).Debug($"{target} ReportValue (string) ({valueName}, {value})");
        }

        [Test]
        public void ReportErrorWithAllValuesSet()
        {
            // given
            const string errorName = "teapot";
            const string errorReason = "I'm a teapot";
            const int errorCode = 418;
            var target = CreateStubAction();

            // when reporting an event
            var obtained = target.ReportError(errorName, errorCode,  errorReason);

            // then
            Assert.That(obtained, Is.SameAs(target));
            mockBeacon.Received(1).ReportError(IdBaseOffset, errorName, errorCode, errorReason);
        }

        [Test]
        public void ReportErrorWithNullErrorNameDoesNotReportTheError()
        {
            // given
            const string errorName = null;
            const string errorReason = "I'm a teapot";
            const int errorCode = 418;
            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportError(errorName, errorCode, errorReason);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportError: errorName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportErrorWithEmptyErrorNameDoesNotReportTheError()
        {
            // given
            var errorName = string.Empty;
            const string errorReason = "I'm a teapot";
            const int errorCode = 418;

            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportError(errorName, errorCode, errorReason);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportError: errorName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportErrorWithNullErrorReasonDesReport()
        {
            // given
            const string errorName = "errorName";
            const string errorReason = null;
            const int errorCode = 418;
            var target = CreateStubAction();

            // when
            var obtained = target.ReportError(errorName, errorCode, errorReason);

            // then
            Assert.That(obtained, Is.SameAs(target));
            mockBeacon.Received(1).ReportError(IdBaseOffset, errorName, errorCode, errorReason);
        }

        [Test]
        public void ReportErrorLogsInvocation()
        {
            // given
            const string errorName = "errorName";
            const string errorReason = "reason";
            const int errorCode = 418;
            var target = CreateStubAction();

            // when
            target.ReportError(errorName, errorCode, errorReason);

            // then
            mockLogger.Received(1).Debug($"{target} ReportError({errorName}, {errorCode}, {errorReason})");
        }

        [Test]
        public void TraceWebRequestGivesValidWebRequestTracer()
        {
            // given
            var target = CreateStubAction();

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
            IOpenKitComposite targetComposite = target;

            // when
            var obtained = target.TraceWebRequest("http://example.com/pages/");

            // then
            var childObjects = targetComposite.GetCopyOfChildObjects();
            Assert.That(childObjects.Count, Is.EqualTo(1));
            Assert.That(childObjects[0], Is.SameAs(obtained));
        }

        [Test]
        public void OnChildClosedRemovesChildFromList()
        {
            // given
            IOpenKitComposite target = CreateStubAction();
            var childObject = Substitute.For<IOpenKitObject>();
            target.StoreChildInList(childObject);

            // when
            target.OnChildClosed(childObject);

            // then
            Assert.That(target.GetCopyOfChildObjects(), Is.Empty);
        }

        [Test]
        public void TracingANullStringWebRequestIsNotAllowed()
        {
            // given
            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.TraceWebRequest(null);

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<NullWebRequestTracer>());
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} TraceWebRequest (String): url must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void TracingAnEmptyStringWebRequestIsNotAllowed()
        {
            // given
            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.TraceWebRequest(string.Empty);

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<NullWebRequestTracer>());
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} TraceWebRequest (String): url must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void TracingAStringWebRequestWithInvalidUrlIsNotAllowed()
        {
            // given
            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.TraceWebRequest("foo:bar://test.com");

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<NullWebRequestTracer>());
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} TraceWebRequest (String): url \"foo:bar://test.com\" does not have a valid scheme");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void TraceWebRequestLogsInvocation()
        {
            // given
            var url = "https://localhost";
            var target = CreateStubAction();

            // when
            target.TraceWebRequest(url);

            // then
            mockLogger.Received(1).Debug($"{target} TraceWebRequest(${url})");
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

            // then
            Assert.That(obtained.ParentId, Is.EqualTo(parentActionId));
        }

        [Test]
        public void IdIsInitializedInTheConstructor()
        {
            // given
            const int beaconId = 777;
            mockBeacon.NextId.Returns(beaconId);
            var target = CreateStubAction();

            // then
            Assert.That(target.Id, Is.EqualTo(beaconId));
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
            const long timestamp = 1234;
            mockBeacon.CurrentTimestamp.Returns(timestamp);
            var target = CreateStubAction();

            // then
            Assert.That(target.StartTime, Is.EqualTo(timestamp));
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
            const int sequenceNumber = 73;
            mockBeacon.NextSequenceNumber.Returns(sequenceNumber);
            var target = CreateStubAction();

            // then
            Assert.That(target.StartSequenceNo, Is.EqualTo(sequenceNumber));
        }

        [Test]
        public void EndSequenceNumberIsMinusOneForNewlyCreatedAction()
        {
            // given
            var target = CreateStubAction();

            // then
            Assert.That(target.EndSequenceNo, Is.EqualTo(-1));
        }

        [Test]
        public void ANewlyCreatedActionIsNotLeft()
        {
            // given
            IActionInternals target = CreateStubAction();

            // then
            Assert.That(target.IsActionLeft, Is.False);
        }

        [Test]
        public void AfterLeavingAnActionItIsLeft()
        {
            // given
            IActionInternals target = CreateStubAction();

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
            mockBeacon.CurrentTimestamp.Returns(endTime);

            // when leaving the action
            target.LeaveAction();

            // then
            Assert.That(target.EndTime, Is.EqualTo(endTime));
        }


        [Test]
        public void LeavingAnActionSetsTheEndSequenceNumber()
        {
            // given
            const int sequenceNumber = 73;
            var target = CreateStubAction();
            mockBeacon.NextSequenceNumber.Returns(sequenceNumber);

            // when leaving the action
            target.LeaveAction();

            // then
            Assert.That(target.EndSequenceNo, Is.EqualTo(sequenceNumber));
        }

        [Test]
        public void LeavingAnActionSerializesItself()
        {
            // given
            var target = CreateStubAction();

            // when leaving the action
            target.LeaveAction();

            // then
            mockBeacon.Received(1).AddAction(target);
        }

        [Test]
        public void LeavingAnActionClosesAllChildObjects()
        {
            // given
            var childObjectOne = Substitute.For<IOpenKitObject>();
            var childObjectTwo = Substitute.For<IOpenKitObject>();

            var target = CreateStubAction();
            IOpenKitComposite targetComposite = target;

            targetComposite.StoreChildInList(childObjectOne);
            targetComposite.StoreChildInList(childObjectTwo);

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
            var parent = Substitute.For<IAction>();
            var target = CreateStubAction("test", parent);
            target.LeaveAction();

            parent.ClearReceivedCalls();
            mockBeacon.ClearReceivedCalls();

            // when
            target.LeaveAction();

            // then
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            Assert.That(parent.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void LeaveActionLogsInvocation()
        {
            // given
            var target = CreateStubAction();

            // when
            target.LeaveAction();

            // then
            mockLogger.Received(1).Debug($"{target} LeaveAction({ActionName})");
        }

        [Test]
        public void ReportEventDoesNothingIfActionIsLeft()
        {
            // given
            var target = CreateStubAction();
            target.LeaveAction();
            mockBeacon.ClearReceivedCalls();

            // when reporting an event
            var obtained = target.ReportEvent("test event");

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ReportIntValueDoesNothingIfActionIsLeft()
        {
            // given
            var target = CreateStubAction();
            target.LeaveAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportValue("intValue", 42);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ReportDoubleValueDoesNothingIfActionIsLeft()
        {
            // given
            var target = CreateStubAction();
            target.LeaveAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportValue("doubleValue", 42.5);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
        }


        [Test]
        public void ReportStringValueDoesNothingIfActionIsLeft()
        {
            // given
            var target = CreateStubAction();
            target.LeaveAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportValue("doubleValue", "someValue");

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
        }



        [Test]
        public void ReportErrorDoesNothingIfActionIsLeft()
        {
            // given
            var target = CreateStubAction();
            target.LeaveAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportError("teapot", 418, "I'm a teapot");

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void TraceWebRequestGivesNullWebRequestTracerIfActionIsAlreadyLeft()
        {
            // given
            var target = CreateStubAction();
            target.LeaveAction();

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
            const int sequenceNumber = 42;
            IDisposable target = CreateStubAction();
            mockBeacon.CurrentTimestamp.Returns(endTime);
            mockBeacon.NextSequenceNumber.Returns(sequenceNumber);

            // when disposing the target
            target.Dispose();

            // then
            Assert.That(((BaseAction)target).EndSequenceNo, Is.EqualTo(sequenceNumber));
            Assert.That(((BaseAction)target).EndTime, Is.EqualTo(endTime));
        }

        private StubBaseAction CreateStubAction(string name = ActionName, IAction parentAction = null)
        {
            return new StubBaseAction(
                mockLogger,
                parentComposite,
                name,
                mockBeacon,
                parentAction
            );
        }

        private  class StubBaseAction : BaseAction
        {
            internal StubBaseAction(ILogger logger, IOpenKitComposite parentComposite, string name, IBeacon beacon,
                IAction parentAction)
                : base(logger, parentComposite, name, beacon)
            {
                ParentAction = parentAction;
            }

            internal override IAction ParentAction { get; }
        }
    }
}