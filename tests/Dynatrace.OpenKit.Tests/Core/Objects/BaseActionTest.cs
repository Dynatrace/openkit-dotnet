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

using System;
using System.Linq;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Util;
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
        public void ReportValueLongWithNullNameDoesNotReportValue()
        {
            // given
            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            const long value = 21;
            var obtained = target.ReportValue(null, value);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportValue (long): valueName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportValueLongWithEmptyNameDoesNotReportValue()
        {
            // given
            const long value = 21;
            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportValue(string.Empty, value);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportValue (long): valueName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportValueLongWithValidValue()
        {
            // given
            const long value = 21;
            const string valueName = "longValue";
            var target = CreateStubAction();

            // when
            var obtained = target.ReportValue(valueName, value);

            // then
            Assert.That(obtained, Is.SameAs(target));
            mockBeacon.Received(1).ReportValue(IdBaseOffset, valueName, value);
        }

        [Test]
        public void ReportValueLongLogsInvocation()
        {
            // given
            const long value = 21;
            const string valueName = "longValue";
            var target = CreateStubAction();

            // when
            target.ReportValue(valueName, value);

            // then
            mockLogger.Received(1).Debug($"{target} ReportValue (long) ({valueName}, {value})");
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
            mockLogger.Received(1).Debug($"{target} ReportValue (double) ({valueName}, {value.ToInvariantString()})");
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
            target.ReportValue(valueName, value);

            // then
            mockLogger.Received(1).Debug($"{target} ReportValue (string) ({valueName}, {value})");
        }

        [Test]
        public void ReportErrorCodeWithAllValuesSet()
        {
            // given
            const string errorName = "teapot";
            const int errorCode = 418;
            var target = CreateStubAction();

            // when reporting an event
            var obtained = target.ReportError(errorName, errorCode);

            // then
            Assert.That(obtained, Is.SameAs(target));
            mockBeacon.Received(1).ReportError(IdBaseOffset, errorName, errorCode);
        }

        [Test]
        public void ReportErrorCodeWithNullErrorNameDoesNotReportTheError()
        {
            // given
            const string errorName = null;
            const int errorCode = 418;
            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportError(errorName, errorCode);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportError: errorName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportErrorCodeWithEmptyErrorNameDoesNotReportTheError()
        {
            // given
            var errorName = string.Empty;
            const int errorCode = 418;

            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportError(errorName, errorCode);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportError: errorName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportErrorCodeLogsInvocation()
        {
            // given
            const string errorName = "errorName";
            const int errorCode = 418;

            var target = CreateStubAction();

            // when
            target.ReportError(errorName, errorCode);

            // then
            mockLogger.Received(1).Debug($"{target} ReportError({errorName}, {errorCode})");
        }

        [Test]
        public void ReportErrorCauseWithAllValuesSet()
        {
            // given
            const string errorName = "FATAL ERROR";
            const string causeName = "name";
            const string causeDescription = "description";
            const string causeStackTrace = "stackTrace";

            var target = CreateStubAction();

            // when
            var obtained = target.ReportError(errorName, causeName, causeDescription, causeStackTrace);

            // then
            mockBeacon.Received(1).ReportError(IdBaseOffset, errorName, causeName, causeDescription, causeStackTrace);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void ReportErrorCauseWithNullErrorNameDoesNotReportTheError()
        {
            // given
            const string causeName = "name";
            const string causeDescription = "description";
            const string causeStackTrace = "stackTrace";

            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportError(null, causeName, causeDescription, causeStackTrace);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportError: errorName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportErrorCauseWithEmptyErrorNameDoesNotReportTheError()
        {
            // given
            const string causeName = "name";
            const string causeDescription = "description";
            const string causeStackTrace = "stackTrace";

            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportError(string.Empty, causeName, causeDescription, causeStackTrace);

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportError: errorName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportErrorCauseWithNullValuesWork()
        {
            // given
            const string errorName = "FATAL ERROR";

            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportError(errorName, null, null, null);

            // then
            Assert.That(obtained, Is.SameAs(target));
            mockBeacon.Received(1).ReportError(IdBaseOffset, errorName, null, null, null);
        }

        [Test]
        public void ReportErrorCauseWithEmptyValuesWork()
        {
            // given
            const string errorName = "FATAL ERROR";

            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportError(errorName, string.Empty, string.Empty, string.Empty);

            // then
            Assert.That(obtained, Is.SameAs(target));
            mockBeacon.Received(1).ReportError(IdBaseOffset, errorName, string.Empty, string.Empty, string.Empty);
        }

        [Test]
        public void ReportErrorCauseLogsInvocation()
        {
            // given
            const string errorName = "error name";
            const string causeName = "name";
            const string causeDescription = "description";
            const string causeStackTrace = "stackTrace";

            var target = CreateStubAction();

            // when
            target.ReportError(errorName, causeName, causeDescription, causeStackTrace);

            // then
            mockLogger.Received(1).Debug($"{target} ReportError({errorName}, {causeName}, {causeDescription}, {causeStackTrace})");

        }

        [Test]
        public void ReportErrorWithException()
        {
            // given
            const string errorName = "FATAL ERROR";
            var exception = new ArgumentException("invalid");

            var target = CreateStubAction();

            // when
            var obtained = target.ReportError(errorName, exception);

            // then
            Assert.That(obtained, Is.SameAs(target));
            mockBeacon.Received(1).ReportError(IdBaseOffset, errorName, exception);
        }

        [Test]
        public void ReportErrorExceptionWithNullErrorNameDoesNotReportTheError()
        {
            // given
            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportError(null, new InvalidOperationException());

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportError: errorName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportErrorExceptionWithEmptyErrorNameDoesNotReportTheError()
        {
            // given
            var target = CreateStubAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportError(string.Empty, new ArgumentException("invalid"));

            // then
            Assert.That(obtained, Is.SameAs(target));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            mockLogger.Received(1).Warn($"{target} ReportError: errorName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportErrorExceptionLogsInvocation()
        {
            // given
            const string errorName = "FATAL ERROR";
            var exception = new ArgumentException("invalid");

            var target = CreateStubAction();

            // when
            target.ReportError(errorName, exception);

            // then
            mockLogger.Received(1).Debug($"{target} ReportError({errorName}, {exception})");
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
        public void ReportLongValueDoesNothingIfActionIsLeft()
        {
            // given
            var target = CreateStubAction();
            target.LeaveAction();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.ReportValue("longValue", long.MaxValue);

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

        [Test]
        public void AfterCancelingAnActionItIsLeft()
        {
            // given
            IActionInternals target = CreateStubAction();

            // when
            target.CancelAction();

            // then
            Assert.That(target.IsActionLeft, Is.True);
        }

        [Test]
        public void CancelingAnActionSetsTheEndTime()
        {
            // given
            mockBeacon.CurrentTimestamp.Returns(1234L, 5678L, 9012L);

            var target = CreateStubAction();

            // when
            target.CancelAction();

            // then
            Assert.That(target.EndTime, Is.EqualTo(5678L));
            var _ = mockBeacon.Received(2).CurrentTimestamp;
        }
        
        [Test]
        public void CancelingAnActionSetsTheEndSequenceNumber()
        {
            // given
            mockBeacon.NextSequenceNumber.Returns(1, 10, 20);

            var target = CreateStubAction();

            // when
            target.CancelAction();

            // then
            Assert.That(target.EndSequenceNo, Is.EqualTo(10));
            var _ = mockBeacon.Received(2).NextSequenceNumber;
        }
        
        [Test]
        public void CancelingAnActionDoesNotSerializeItself()
        {
            // given
            var target = CreateStubAction();

            // when
            target.CancelAction();

            // then
            mockBeacon.DidNotReceive().AddAction(Arg.Any<IActionInternals>());
        }
        
        [Test]
        public void CancelingAnActionCancelsAllChildObjects()
        {
            // given
            var childObjectOne = Substitute.For<ICancelableOpenKitObject>();
            var childObjectTwo = Substitute.For<ICancelableOpenKitObject>();

            var target = CreateStubAction();
            IOpenKitComposite targetComposite = target;
            targetComposite.StoreChildInList(childObjectOne);
            targetComposite.StoreChildInList(childObjectTwo);

            // when
            target.CancelAction();

            // then
            childObjectOne.Received(1).Cancel();
            childObjectTwo.Received(1).Cancel();
        }
        
        [Test]
        public void CancelingAnActionClosesAllChildObjectsThatAreNotCancelable()
        {
            // given
            var childObjectOne = Substitute.For<IOpenKitObject>();
            var childObjectTwo = Substitute.For<IOpenKitObject>();

            var target = CreateStubAction();
            IOpenKitComposite targetComposite = target;
            targetComposite.StoreChildInList(childObjectOne);
            targetComposite.StoreChildInList(childObjectTwo);

            // when
            target.CancelAction();

            // then
            childObjectOne.Received(1).Dispose();
            childObjectTwo.Received(1).Dispose();

#if NET35
            // old NSubstitute does only print the class, nothing about instance
            mockLogger.Received(2).Warn(Arg.Is<string>(message => message.EndsWith("is not cancelable - falling back to Dispose() instead")));
#else
            mockLogger.Received(1).Warn($"{childObjectOne} is not cancelable - falling back to Dispose() instead");
            mockLogger.Received(1).Warn($"{childObjectTwo} is not cancelable - falling back to Dispose() instead");
#endif
        }
        
        [Test]
        public void CancelingAnActionNotifiesTheParentCompositeObject()
        {
            // given
            var target = CreateStubAction();
            parentComposite.ClearReceivedCalls();

            // when
            target.CancelAction();

            // then
            Assert.That(parentComposite.ReceivedCalls().Count(), Is.EqualTo(1));
            parentComposite.Received(1).OnChildClosed(target);
        }

        [Test]
        public void CancelingAnActionReturnsTheParentAction()
        {
            // given
            var parentAction = Substitute.For<IAction>();
            var target = CreateStubAction(parentAction: parentAction);

            // when
            var obtained = target.CancelAction();

            // then
            Assert.That(obtained, Is.SameAs(parentAction));
        }
        
        [Test]
        public void CancelingAnAlreadyCanceledActionReturnsTheParentAction()
        {
            // given
            var parentAction = Substitute.For<IAction>();
            var target = CreateStubAction(parentAction: parentAction);
            target.CancelAction(); // cancelling the first time

            // when leaving a second time
            var obtained = target.CancelAction();

            // then
            Assert.That(obtained, Is.SameAs(parentAction));
        }
        
        [Test]
        public void CancelingAnAlreadyCancelledActionReturnsImmediately()
        {
            // given
            var target = CreateStubAction();
            target.CancelAction(); // cancelling the first time
            parentComposite.ClearReceivedCalls();

            // when
            target.CancelAction();

            // then
            Assert.That(parentComposite.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void CancelingActionLogsInvocation()
        {
            // given
            mockLogger.IsDebugEnabled.Returns(true);

            var target = CreateStubAction();

            // when
            target.CancelAction();

            // then
            var _ = mockLogger.Received(1).IsDebugEnabled;
            mockLogger.Received(1).Debug($"{target} CancelAction({ActionName})");
        }

        [Test]
        public void CancelCancelsTheAction()
        {
            // given
            mockBeacon.CurrentTimestamp.Returns(1234L);
            mockBeacon.NextSequenceNumber.Returns(42);

            var target = CreateStubAction();
            ICancelableOpenKitObject cancelableTarget = target;

            // when
            cancelableTarget.Cancel();

            // then
            Assert.That(target.EndTime, Is.EqualTo(1234L));
            Assert.That(target.EndSequenceNo, Is.EqualTo(42));

            mockBeacon.DidNotReceiveWithAnyArgs().AddAction(Arg.Any<IActionInternals>());
            var _1 = mockBeacon.Received(2).CurrentTimestamp;
            var _2 = mockBeacon.Received(2).NextSequenceNumber;
        }

        [Test]
        public void DurationGivesDurationSinceStartIfActionIsNotLeft()
        {
            // given
            mockBeacon.CurrentTimestamp.Returns(12L, 42L);
            var target = CreateStubAction();

            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.Duration;

            // then
            Assert.That(obtained, Is.EqualTo(TimeSpan.FromMilliseconds(30)));
            var _ = mockBeacon.Received(1).CurrentTimestamp;
        }

        [Test]
        public void getDurationInMillisecondsGivesDurationBetweenEndAndStartTimeIfActionIsLeft()
        {
            // given
            mockBeacon.CurrentTimestamp.Returns(12L, 42L);
            var target = CreateStubAction();
            target.LeaveAction();

            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.Duration;

            // then
            Assert.That(obtained, Is.EqualTo(TimeSpan.FromMilliseconds(30)));
            Assert.That(mockBeacon.ReceivedCalls, Is.Empty);
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