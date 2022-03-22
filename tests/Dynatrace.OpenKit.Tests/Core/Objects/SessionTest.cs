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
using System.Collections.Generic;
using System.Linq;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using Dynatrace.OpenKit.Util.Json.Objects;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Objects
{
    public class SessionTest
    {
        private ILogger mockLogger;
        private IBeacon mockBeacon;
        private IAdditionalQueryParameters mockAdditionalParameters;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
            mockLogger.IsDebugEnabled.Returns(true);
            mockLogger.IsDebugEnabled.Returns(true);

            mockBeacon = Substitute.For<IBeacon>();
            mockAdditionalParameters = Substitute.For<IAdditionalQueryParameters>();
        }

        [Test]
        public void EnterActionWithNullActionNameGivesNullRootActionObject()
        {
            // given
            var target = CreateSession().Build();
            mockBeacon.ClearReceivedCalls();

            // when
            var obtained = target.EnterAction(null);

            // then
            Assert.That(obtained, Is.Not.Null.And.TypeOf<NullRootAction>());
            mockLogger.Received(1).Warn("Session [sn=0, seq=0] EnterAction: actionName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void EnterActionWithEmptyActionNameGivesNullRootActionObject()
        {
            // given
            var target = CreateSession().Build();

            // when
            var obtained = target.EnterAction(string.Empty);

            // then
            Assert.That(obtained, Is.Not.Null.And.TypeOf<NullRootAction>());
            mockLogger.Received(1).Warn("Session [sn=0, seq=0] EnterAction: actionName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void EnterActionWithNonEmptyNameGivesRootAction()
        {
            // given
            var target = CreateSession().Build();

            // when
            var obtained = target.EnterAction("action name");

            // then
            Assert.That(obtained, Is.Not.Null.And.TypeOf<RootAction>());
        }

        [Test]
        public void EnterActionAlwaysGivesANewInstance()
        {
            // given
            const string actionName = "some action";
            var target = CreateSession().Build();

            // when
            var obtainedOne = target.EnterAction(actionName);
            var obtainedTwo = target.EnterAction(actionName);

            // then
            Assert.That(obtainedOne, Is.Not.Null);
            Assert.That(obtainedOne, Is.Not.Null);
            Assert.That(obtainedOne, Is.Not.SameAs(obtainedTwo));
        }

        [Test]
        public void EnterActionAddsNewlyCreatedActionToTheListOfChildObjects()
        {
            // given
            const string actionName = "some action";
            var target = CreateSession().Build();

            // when
            var obtainedOne = target.EnterAction(actionName);
            var obtainedTwo = target.EnterAction(actionName);

            // then
            var childObjects = target.GetCopyOfChildObjects();
            Assert.That(childObjects.Count, Is.EqualTo(2));
            Assert.That(childObjects[0], Is.SameAs(obtainedOne));
            Assert.That(childObjects[1], Is.SameAs(obtainedTwo));
        }

        [Test]
        public void EnterActionLogsInvocation()
        {
            // given
            var actionName = "action name";
            var target = CreateSession().Build();

            // when
            target.EnterAction(actionName);

            // then
            mockLogger.Received(1).Debug($"Session [sn=0, seq=0] EnterAction({actionName})");
        }

        [Test]
        public void IdentifyUserWithNullTagReportsUser()
        {
            // given
            var target = CreateSession().Build();

            // when
            target.IdentifyUser(null);

            // then
            mockBeacon.Received(1).IdentifyUser(null);
            mockLogger.Received(1).Debug($"Session [sn=0, seq=0] IdentifyUser()");
        }

        [Test]
        public void IdentifyUserWithEmptyTagReportsUser()
        {
            // given
            var target = CreateSession().Build();

            // when
            target.IdentifyUser(string.Empty);

            // then
            mockBeacon.Received(1).IdentifyUser("");
            mockLogger.Received(1).Debug($"Session [sn=0, seq=0] IdentifyUser()");
        }

        [Test]
        public void IdentifyUserWithNonEmptyTagReportsUser()
        {
            // given
            const string userTag = "john.doe@acme.com";
            var target = CreateSession().Build();

            // when
            target.IdentifyUser(userTag);

            // then
            mockLogger.Received(0).Warn(Arg.Any<string>());
            mockBeacon.Received(1).IdentifyUser(userTag);
        }

        [Test]
        public void IdentifyUserMultipleTimesAlwaysCallsBeacon()
        {
            // given
            const string user = "user";
            var target = CreateSession().Build();

            // when
            target.IdentifyUser(user);
            target.IdentifyUser(user);

            // then
            mockLogger.Received(0).Warn(Arg.Any<string>());
            mockBeacon.Received(2).IdentifyUser(user);
        }

        [Test]
        public void IdentifyUserLogsInvocation()
        {
            // given
            const string userTag = "john.doe@acme.com";
            var target = CreateSession().Build();

            // when
            target.IdentifyUser(userTag);

            // then
            mockLogger.Received(1).Debug($"Session [sn=0, seq=0] IdentifyUser({userTag})");
        }

        [Test]
        public void ReportCrashWithNullErrorNameDoesNotReportAnything()
        {
            // given
            const string errorName = null;
            const string errorReason = "reason for crash";
            const string stacktrace = "the stacktrace";
            var target = CreateSession().Build();

            // when
            target.ReportCrash(errorName, errorReason, stacktrace);

            // then
            mockBeacon.Received(0).ReportCrash(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            mockLogger.Received(1).Warn("Session [sn=0, seq=0] ReportCrash: errorName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportCrashWithEmptyErrorNameDoesNotReportAnything()
        {
            // given
            var errorName = string.Empty;
            const string errorReason = "reason for crash";
            const string stacktrace = "the stacktrace";
            var target = CreateSession().Build();

            // when
            target.ReportCrash(errorName, errorReason, stacktrace);

            // then
            mockBeacon.Received(0).ReportCrash(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            mockLogger.Received(1).Warn("Session [sn=0, seq=0] ReportCrash: errorName must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportCrashWithNullReasonAndStacktraceWorks()
        {
            // given
            const string errorName = "error name";
            const string errorReason = null;
            const string stacktrace = null;
            var target = CreateSession().Build();

            // when
            target.ReportCrash(errorName, errorReason, stacktrace);

            // then
            mockBeacon.Received(1).ReportCrash(errorName, errorReason, stacktrace);
        }

        [Test]
        public void ReportCrashWithEmptyReasonAndStacktraceWorks()
        {
            // given
            const string errorName = "error name";
            var errorReason = string.Empty;
            var stacktrace = string.Empty;
            var target = CreateSession().Build();

            // when
            target.ReportCrash(errorName, errorReason, stacktrace);

            // then
            mockBeacon.Received(1).ReportCrash(errorName, errorReason, stacktrace);
        }

        [Test]
        public void ReportCrashWithSameDataMultipleTimesForwardsEachCallToBeacon()
        {
            // given
            const string errorName = "error name";
            const string errorReason = "error reason";
            const string stacktrace = "stacktrace";
            var target = CreateSession().Build();

            // when
            target.ReportCrash(errorName, errorReason, stacktrace);
            target.ReportCrash(errorName, errorReason, stacktrace);

            // then
            mockBeacon.Received(2).ReportCrash(errorName, errorReason, stacktrace);
        }

        [Test]
        public void ReportCrashLogsInvocation()
        {
            // given
            const string errorName = "error name";
            const string errorReason = "error reason";
            const string stacktrace = "stacktrace";
            var target = CreateSession().Build();

            // when
            target.ReportCrash(errorName, errorReason, stacktrace);

            // then
            mockLogger.Received(1).Debug($"Session [sn=0, seq=0] ReportCrash({errorName}, {errorReason}, {stacktrace})");
        }

        [Test]
        public void ReportCrashWithNullExceptionDoesNotReportAnything()
        {
            // given
            var target = CreateSession().Build();

            // when
            target.ReportCrash(null);

            // then
            mockBeacon.Received(0).ReportCrash(Arg.Any<Exception>());
            mockLogger.Received(1).Warn("Session [sn=0, seq=0] ReportCrash: exception must not be null");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ReportCrashExceptionWithSameDataMultipleTimesForwardsEachCallToBeacon()
        {
            // given
            var exception = new ArgumentException("foo is not in range");
            var target = CreateSession().Build();

            // when
            target.ReportCrash(exception);
            target.ReportCrash(exception);

            // then
            mockBeacon.Received(2).ReportCrash(exception);
        }

        [Test]
        public void ReportCrashExceptionLogsInvocation()
        {
            // given
            var exception = new ArgumentException("foo is not in range");
            var target = CreateSession().Build();

            // when
            target.ReportCrash(exception);

            // then
            mockLogger.Received(1).Debug($"Session [sn=0, seq=0] ReportCrash({exception})");
        }

        [Test]
        public void EndingASessionFinishesSessionOnBeacon()
        {
            // given
            const long timestamp = 4321;
            mockBeacon.CurrentTimestamp.Returns(timestamp);

            var target = CreateSession().Build();

            // when
            target.End();

            // then
            mockBeacon.Received(1).EndSession();
        }

        [Test]
        public void EndingASessionDoesNotFinishSessionOnBeacon()
        {
            // given
            const long timestamp = 4321;
            mockBeacon.CurrentTimestamp.Returns(timestamp);

            var target = CreateSession().Build();

            // when
            target.End(false);

            // then
            mockBeacon.Received(0).EndSession();
        }

        [Test]
        public void EndingAnAlreadyEndedSessionDoesNothing()
        {
            // given
            var target = CreateSession().Build();

            // when
            target.End();

            // then
            mockBeacon.Received(1).EndSession();
            Assert.That(target.State.IsFinished, Is.True);

            mockBeacon.ClearReceivedCalls();

            // and when
            target.End();

            // then
            mockBeacon.Received(0).EndSession();
            Assert.That(target.State.IsFinished, Is.True);
        }

        [Test]
        public void EndingASessionImplicitlyClosesAllOpenChildObjects()
        {
            // given
            var childObjectOne = Substitute.For<IOpenKitObject>();
            var childObjectTwo = Substitute.For<IOpenKitObject>();

            var target = CreateSession().Build();

            target.StoreChildInList(childObjectOne);
            target.StoreChildInList(childObjectTwo);

            // when
            target.End();

            // then
            childObjectOne.Received(1).Dispose();
            childObjectTwo.Received(1).Dispose();
        }

        [Test]
        public void EndLogsInvocation()
        {
            // given
            var target = CreateSession().Build();

            // when
            target.End();

            // then
            mockLogger.Received(1).Debug("Session [sn=0, seq=0] End()");
        }

        [Test]
        public void TryEndEndsSessionIfNoMoreChildObjects()
        {
            // given
            var target = CreateSession().Build();
            var action = target.EnterAction("action");
            var tracer = target.TraceWebRequest("https://localhost");

            // when
            var obtained = target.TryEnd();

            // then
            Assert.That(obtained, Is.False);
            mockBeacon.Received(0).EndSession();

            // and when
            action.LeaveAction();
            obtained = target.TryEnd();

            // then
            Assert.That(obtained, Is.False);
            mockBeacon.Received(0).EndSession();

            // and when
            tracer.Stop(200);
            obtained = target.TryEnd();

            // then
            Assert.That(obtained, Is.True);
            mockBeacon.Received(0).EndSession();
        }

        [Test]
        public void TryEndReturnsTrueIfSessionAlreadyEnded()
        {
            // given
            var target = CreateSession().Build();
            target.End();

            // when
            var obtained = target.TryEnd();

            // then
            Assert.That(obtained, Is.True);
        }

        [Test]
        public void TryEndMarksSessionStateAsWasTriedForEndingIfSessionNotClosable()
        {
            // given
            var target = CreateSession().Build();
            target.EnterAction("action");

            // when
            var obtained = target.TryEnd();

            // then
            Assert.That(obtained, Is.False);
            Assert.That(target.State.WasTriedForEnding, Is.True);
            Assert.That(target.State.IsFinished, Is.False);
        }

        [Test]
        public void TryEndDoesNotMarkSessionStateAsWasTriedForEndingIfSessionIsClosable()
        {
            // given
            var target = CreateSession().Build();

            // when
            var obtained = target.TryEnd();

            // then
            Assert.That(obtained, Is.True);
            Assert.That(target.State.WasTriedForEnding, Is.False);
            Assert.That(target.State.IsFinished, Is.True);
        }

        [Test]
        public void SendBeaconForwardsCallToBeacon()
        {
            // given
            var target = CreateSession().Build();
            var clientProvider = Substitute.For<IHttpClientProvider>();

            // when
            target.SendBeacon(clientProvider, mockAdditionalParameters);

            // then
            mockBeacon.Received(1).Send(clientProvider, mockAdditionalParameters);
        }

        [Test]
        public void ClearCapturedDataForwardsCallToBeacon()
        {
            // given
            var target = CreateSession().Build();

            // when
            target.ClearCapturedData();

            // then
            mockBeacon.Received(1).ClearData();
        }

        [Test]
        public void IsEmptyForwardsCallToBeacon()
        {
            // given
            var target = CreateSession().Build();

            // when
            _ = target.IsEmpty;

            // then
            _ = mockBeacon.Received(1).IsEmpty;
        }

        [Test]
        public void InitializeServerConfigurationForwardsCallToBeacon()
        {
            // given
            var target = CreateSession().Build();
            var mockServerConfiguration = Substitute.For<IServerConfiguration>();

            // when
            target.InitializeServerConfiguration(mockServerConfiguration);

            // then
            mockBeacon.Received(1).StartSession();
            mockBeacon.Received(1).InitializeServerConfiguration(mockServerConfiguration);
            Assert.That(mockBeacon.ReceivedCalls().Count(), Is.EqualTo(2));
        }

        [Test]
        public void UpdateServerConfigurationForwardsCallToBeacon()
        {
            // given
            var target = CreateSession().Build();
            var mockServerConfig = Substitute.For<IServerConfiguration>();

            // when
            target.UpdateServerConfiguration(mockServerConfig);

            // then
            mockBeacon.Received(1).UpdateServerConfiguration(mockServerConfig);
        }

        [Test]
        public void ANewlyCreatedSessionIsNotFinished()
        {
            // given
            var target = CreateSession().Build();

            // then
            Assert.That(target.State.IsFinished, Is.False);
            Assert.That(target.State.IsConfiguredAndFinished, Is.False);
        }

        [Test]
        public void ANewlyCreatedSessionIsNotInStateConfigured()
        {
            // given
            var target = CreateSession().Build();

            // when, then
            Assert.That(target.State.IsConfigured, Is.False);
            Assert.That(target.State.IsConfiguredAndFinished, Is.False);
            Assert.That(target.State.IsConfiguredAndOpen, Is.False);
        }

        [Test]
        public void ANewlyCreatedSessionIsNotInStateAsWasTriedForEnding()
        {
            // given
            var target = CreateSession().Build();

            // when, then
            Assert.That(target.State.WasTriedForEnding, Is.False);
        }

        [Test]
        public void ANotConfiguredNotFinishedSessionHasCorrectState()
        {
            // given
            mockBeacon.IsServerConfigurationSet.Returns(false);
            var target = CreateSession().Build();

            // then, then
            Assert.That(target.State.IsConfigured, Is.False);
            Assert.That(target.State.IsConfiguredAndOpen, Is.False);
            Assert.That(target.State.IsConfiguredAndFinished, Is.False);
            Assert.That(target.State.IsFinished, Is.False);
        }

        [Test]
        public void AConfiguredNotFinishedSessionHasCorrectState()
        {
            // given
            mockBeacon.IsServerConfigurationSet.Returns(true);
            var target = CreateSession().Build();

            // then, then
            Assert.That(target.State.IsConfigured, Is.True);
            Assert.That(target.State.IsConfiguredAndOpen, Is.True);
            Assert.That(target.State.IsConfiguredAndFinished, Is.False);
            Assert.That(target.State.IsFinished, Is.False);
        }

        [Test]
        public void ANotConfiguredFinishedSessionHasCorrectState()
        {
            // given
            mockBeacon.IsServerConfigurationSet.Returns(false);
            var target = CreateSession().Build();
            target.End();

            // then, then
            Assert.That(target.State.IsConfigured, Is.False);
            Assert.That(target.State.IsConfiguredAndOpen, Is.False);
            Assert.That(target.State.IsConfiguredAndFinished, Is.False);
            Assert.That(target.State.IsFinished, Is.True);
        }

        [Test]
        public void AConfiguredFinishedSessionHasCorrectState()
        {
            // given
            mockBeacon.IsServerConfigurationSet.Returns(true);
            var target = CreateSession().Build();
            target.End();

            // then, then
            Assert.That(target.State.IsConfigured, Is.True);
            Assert.That(target.State.IsConfiguredAndOpen, Is.False);
            Assert.That(target.State.IsConfiguredAndFinished, Is.True);
            Assert.That(target.State.IsFinished, Is.True);
        }

        [Test]
        public void ASessionIsFinishedIfEndIsCalled()
        {
            // given
            var target = CreateSession().Build();

            // when
            target.End();

            // then
            Assert.That(target.State.IsFinished, Is.True);
        }

        [Test]
        public void EnterActionGivesNullActionIfSessionIsAlreadyEnded()
        {
            // given
            var target = CreateSession().Build();
            target.End();

            // when
            var obtained = target.EnterAction("action name");

            // then
            Assert.That(obtained, Is.Not.Null.And.TypeOf<NullRootAction>());
        }

        [Test]
        public void IdentifyUserDoesNothingIfSessionIsEnded()
        {
            // given
            var target = CreateSession().Build();
            target.End();

            // when
            target.IdentifyUser("john.doe@acme.com");

            // then
            mockBeacon.Received(0).IdentifyUser(Arg.Any<string>());
        }

        [Test]
        public void ReportCrashDoesNothingIfSessionIsEnded()
        {
            // given
            const string errorName = "crash";
            const string errorReason = "reason for crash";
            const string stacktrace = "stacktrace";
            var target = CreateSession().Build();
            target.End();

            // when
            target.ReportCrash(errorName, errorReason, stacktrace);

            // then
            mockBeacon.Received(0).ReportCrash(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void DisposingASessionEndsTheSession()
        {
            // given
            const long timestamp = 4321;
            mockBeacon.CurrentTimestamp.Returns(timestamp);

            var target = CreateSession().Build();

            // when
            target.Dispose();

            // then
            mockBeacon.Received(1).EndSession();
            Assert.That(target.State.IsFinished, Is.True);
        }

        [Test]
        public void EndingASessionEndsOpenRootActionsFirst()
        {
            // given
            int timestamp = 100;
            mockBeacon.CurrentTimestamp.Returns(_ => timestamp++);

            var target = CreateSession().Build();
            var rootActionOne = target.EnterAction("Root Action One");
            var rootActionTwo = target.EnterAction("Root Action Two");

            // when
            target.End();

            // then
            Assert.That(((IActionInternals) rootActionOne).IsActionLeft, Is.True);
            Assert.That(((IActionInternals) rootActionTwo).IsActionLeft, Is.True);
            Received.InOrder(() =>
                {
                    rootActionOne.LeaveAction();
                    rootActionTwo.LeaveAction();
                    mockBeacon.EndSession();
                }
            );
        }

        [Test]
        public void TraceWebRequestGivesAppropriateTracer()
        {
            // given
            var target = CreateSession().Build();

            // when
            var obtained = target.TraceWebRequest("http://example.com/pages/");

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<WebRequestTracer>());
        }

        [Test]
        public void TraceWebRequestAddsTracerToListOfChildren()
        {
            // given
            const string url = "https://www.google.com";
            var target = CreateSession().Build();

            // when
            var obtained = target.TraceWebRequest(url);

            // then
            var childObjects = target.GetCopyOfChildObjects();
            Assert.That(childObjects.Count, Is.EqualTo(1));
            Assert.That(childObjects[0], Is.SameAs(obtained));
        }

        [Test]
        public void TracingANullStringWebRequestIsNotAllowed()
        {
            // given
            var target = CreateSession().Build();

            // when
            var obtained = target.TraceWebRequest(null);

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<NullWebRequestTracer>());
            mockLogger.Received(1).Warn("Session [sn=0, seq=0] TraceWebRequest(String): url must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void TracingAnEmptyStringWebRequestIsNotAllowed()
        {
            // given
            var target = CreateSession().Build();

            // when
            var obtained = target.TraceWebRequest(string.Empty);

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<NullWebRequestTracer>());
            mockLogger.Received(1).Warn("Session [sn=0, seq=0] TraceWebRequest(String): url must not be null or empty");
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void TracingAWebRequestWithInvalidUrlIsNotAllowed()
        {
            // given
            const string url = "foo:bar://test.com";
            var target = CreateSession().Build();

            // when
            var obtained = target.TraceWebRequest(url);

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<NullWebRequestTracer>());
            mockLogger.Received(1)
                .Warn($"Session [sn=0, seq=0] TraceWebRequest(String): url \"{url}\" does not have a valid scheme");
        }

        [Test]
        public void TraceWebRequestGivesNullWebRequestTracerIfSessionIsAlreadyEnded()
        {
            // given
            var target = CreateSession().Build();
            target.End();

            // when
            var obtained = target.TraceWebRequest("http://example.com/pages/");

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<NullWebRequestTracer>());
        }

        [Test]
        public void TraceWebRequestLogsInvocation()
        {
            // given
            const string url = "https://localhost";
            var target = CreateSession().Build();

            // when
            target.TraceWebRequest(url);

            // then
            mockLogger.Received(1).Debug($"Session [sn=0, seq=0] TraceWebRequest({url})");
        }

        [Test]
        public void OnChildClosedRemovesChildFromList()
        {
            // given
            var childObject = Substitute.For<IOpenKitObject>();
            var target = CreateSession().Build();
            target.StoreChildInList(childObject);

            // when
            target.OnChildClosed(childObject);

            // then
            Assert.That(target.GetCopyOfChildObjects(), Is.Empty);
        }

        [Test]
        public void OnChildClosedEndsSessionWithoutChildrenIfInStateWasTriedForEnding()
        {
            // given
            var mockParent = Substitute.For<IOpenKitComposite>();
            var target = CreateSession().With(mockParent).Build();
            var childObject = Substitute.For<IOpenKitObject>();
            target.StoreChildInList(childObject);

            var wasClosed = target.TryEnd();
            Assert.That(wasClosed, Is.False);
            var state = target.State;
            Assert.That(state.WasTriedForEnding, Is.True);
            Assert.That(state.IsFinished, Is.False);

            // when
            target.OnChildClosed(childObject);

            // then
            Assert.That(state.IsFinished, Is.True);
            mockParent.Received(1).OnChildClosed(target);
        }

        [Test]
        public void OnChildClosedDoesNotEndSessionWithChildrenIfInStateWasTriedForEnding()
        {
            // given
            var mockParent = Substitute.For<IOpenKitComposite>();
            var target = CreateSession().With(mockParent).Build();
            var childObjectOne = Substitute.For<IOpenKitObject>();
            var childObjectTwo = Substitute.For<IOpenKitObject>();
            target.StoreChildInList(childObjectOne);
            target.StoreChildInList(childObjectTwo);

            var wasClosed = target.TryEnd();
            Assert.That(wasClosed, Is.False);
            var state = target.State;
            Assert.That(state.WasTriedForEnding, Is.True);
            Assert.That(state.IsFinished, Is.False);

            // when
            target.OnChildClosed(childObjectOne);

            // then
            Assert.That(state.IsFinished, Is.False);
            mockParent.Received(0).OnChildClosed(Arg.Any<IOpenKitObject>());
        }

        [Test]
        public void OnChildClosedDoesNotEndSessionIfNotInStateWasTriedForEnding()
        {
            // given
            var mockParent = Substitute.For<IOpenKitComposite>();
            var target = CreateSession().With(mockParent).Build();
            var childObject = Substitute.For<IOpenKitObject>();
            target.StoreChildInList(childObject);

            // when
            target.OnChildClosed(childObject);

            // then
            Assert.That(target.State.IsFinished, Is.False);
            mockParent.Received(0).OnChildClosed(Arg.Any<IOpenKitObject>());
        }

        [Test]
        public void ToStringReturnsAppropriateResult()
        {
            // given
            const int sessionNumber = 42;
            const int sessionSequence = 21;
            mockBeacon.SessionNumber.Returns(sessionNumber);
            mockBeacon.SessionSequenceNumber.Returns(sessionSequence);

            var target = CreateSession().Build();

            // when
            var obtained = target.ToString();

            // then
            Assert.That(obtained, Is.EqualTo($"Session [sn={sessionNumber}, seq={sessionSequence}]"));
        }

        [Test]
        public void ANewSessionCanSendNewSessionRequests()
        {
            // given
            var target = CreateSession().Build();

            // when
            var obtained = target.CanSendNewSessionRequest;

            // then
            Assert.That(obtained, Is.True);
        }

        [Test]
        public void CanSendNewSessionRequestIsFalseIfAllRequestsAreUsedUp()
        {
            // given
            var target = CreateSession().Build();

            // when, then
            for (var i = Session.MaxNewSessionRequests; i > 0; i--)
            {
                Assert.That(target.CanSendNewSessionRequest, Is.True);

                target.DecreaseNumRemainingSessionRequests();
            }

            // then
            Assert.That(target.CanSendNewSessionRequest, Is.False);
        }

        [Test]
        public void IsDataSendingAllowedReturnsTrueForConfiguredAndDataCaptureEnabledSession()
        {
            // given
            mockBeacon.IsDataCapturingEnabled.Returns(true);
            mockBeacon.IsServerConfigurationSet.Returns(true);

            var target = CreateSession().Build();

            // when
            var obtained = target.IsDataSendingAllowed;

            // then
            Assert.That(obtained, Is.True);
        }

        [Test]
        public void IsDataSendingAllowedReturnsFalseForNotConfiguredSession()
        {
            // given
            mockBeacon.IsDataCapturingEnabled.Returns(true);
            mockBeacon.IsServerConfigurationSet.Returns(false);

            var target = CreateSession().Build();

            // when
            var obtained = target.IsDataSendingAllowed;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void IsDataSendingAllowedReturnsFalseForDataCaptureDisabledSession()
        {
            // given
            mockBeacon.IsDataCapturingEnabled.Returns(false);
            mockBeacon.IsServerConfigurationSet.Returns(true);

            var target = CreateSession().Build();

            // when
            var obtained = target.IsDataSendingAllowed;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void IsDataSendingAllowedReturnsFalseForNotConfiguredAndDataCaptureDisabledSession()
        {
            // given
            mockBeacon.IsDataCapturingEnabled.Returns(false);
            mockBeacon.IsServerConfigurationSet.Returns(false);

            var target = CreateSession().Build();

            // when
            var obtained = target.IsDataSendingAllowed;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void EnableCaptureDelegatesToBeacon()
        {
            // given
            var target = CreateSession().Build();

            // when
            target.EnableCapture();

            // then
            mockBeacon.Received(1).EnableCapture();
        }

        [Test]
        public void DisableCaptureDelegatesToBeacon()
        {
            // given
            var target = CreateSession().Build();

            // when
            target.DisableCapture();

            // then
            mockBeacon.Received(1).DisableCapture();
        }

        [Test]
        public void SendEventWithNullEventName()
        {
            // given
            var target = CreateSession().Build();

            // when
            target.SendEvent(null, new Dictionary<string, JsonValue>());

            // then
            mockLogger.Received(1).Warn($"{target} SendEvent: name must not be null or empty");
        }

        [Test]
        public void SendEventWithEmptyEventName()
        {
            // given
            var target = CreateSession().Build();

            // when
            target.SendEvent("", new Dictionary<string, JsonValue>());

            // then
            mockLogger.Received(1).Warn($"{target} SendEvent: name must not be null or empty");
        }

        [Test]
        public void SendEventWithNameInPayload()
        {
            // given
            var target = CreateSession().Build();
            const string eventName = "SomeEvent";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add("name", JsonStringValue.FromString("Test"));

            // when
            target.SendEvent(eventName, attributes);

            Dictionary<string, JsonValue> actualAttributes = new Dictionary<string, JsonValue>();
            actualAttributes.Add("name", JsonStringValue.FromString(eventName));

            // then
            mockLogger.Received(1).Warn($"{target} SendEvent: name must not be used in the attributes as it will be overridden!");
            mockLogger.Received(1).Debug($"{target} SendEvent({eventName},{actualAttributes.ToString()})");
            mockBeacon.Received(1).SendEvent("SomeEvent", "{\"name\":\"SomeEvent\"}");
        }

        [Test]
        public void SendEventWithValidPayload()
        {
            // given
            var target = CreateSession().Build();
            const string eventName = "SomeEvent";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add("value", JsonStringValue.FromString("Test"));

            // when
            target.SendEvent(eventName, attributes);

            // then
            mockLogger.Received(1).Debug($"{target} SendEvent({eventName},{attributes.ToString()})");
            mockBeacon.Received(1).SendEvent("SomeEvent", "{\"value\":\"Test\",\"name\":\"SomeEvent\"}");
        }

        [Test]
        public void SendEventDoesNothingIfSessionIsEnded()
        {
            // given
            var target = CreateSession().Build();
            target.End();

            // when
            target.SendEvent("EventName", new Dictionary<string, JsonValue>());

            // then
            mockBeacon.Received(0).SendEvent(Arg.Any<string>(), Arg.Any<string>());
        }

        private TestSessionBuilder CreateSession()
        {
            return new TestSessionBuilder()
                    .With(mockLogger)
                    .With(mockBeacon)
                ;
        }
    }
}