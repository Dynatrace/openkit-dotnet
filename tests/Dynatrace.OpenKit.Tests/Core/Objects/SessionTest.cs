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
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Objects
{
    public class SessionTest
    {
        private ILogger mockLogger;
        private IBeacon mockBeacon;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
            mockLogger.IsDebugEnabled.Returns(true);
            mockLogger.IsDebugEnabled.Returns(true);

            mockBeacon = Substitute.For<IBeacon>();
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
            mockLogger.Received(1).Warn("Session [sn=0] EnterAction: actionName must not be null or empty");
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
            mockLogger.Received(1).Warn("Session [sn=0] EnterAction: actionName must not be null or empty");
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
        public void IdentifyUserWithNullTagDoesNothing()
        {
            // given
            var target = CreateSession().Build();

            // when
            target.IdentifyUser(null);

            // then
            mockBeacon.Received(0).IdentifyUser(Arg.Any<string>());
            mockLogger.Received(1).Warn("Session [sn=0] IdentifyUser: userTag must not be null or empty");
        }

        [Test]
        public void IdentifyUserWithEmptyTagDoesNothing()
        {
            // given
            var target = CreateSession().Build();

            // when
            target.IdentifyUser(string.Empty);

            // then
            mockBeacon.Received(0).IdentifyUser(Arg.Any<string>());
            mockLogger.Received(1).Warn("Session [sn=0] IdentifyUser: userTag must not be null or empty");
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
            mockLogger.Received(1).Warn("Session [sn=0] ReportCrash: errorName must not be null or empty");
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
            mockLogger.Received(1).Warn("Session [sn=0] ReportCrash: errorName must not be null or empty");
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
        public void SendBeaconForwardsCallToBeacon()
        {
            // given
            var target = CreateSession().Build();
            var clientProvider = Substitute.For<IHttpClientProvider>();

            // when
            target.SendBeacon(clientProvider);

            // then
            mockBeacon.Received(1).Send(clientProvider);
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
        }

        [Test]
        public void ANewlyCreatedSessionIsInStateNew()
        {
            // given
            var target = CreateSession().Build();

            // when, then
            Assert.That(target.State.IsNew, Is.True);
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
        public void AConfiguredSessionIsNotInStateNew()
        {
            // given
            var target = CreateSession().Build();

            // when
            mockBeacon.IsServerConfigurationSet.Returns(true);

            // then
            Assert.That(target.State.IsNew, Is.False);
            Assert.That(target.State.IsConfiguredAndOpen, Is.True);
        }

        [Test]
        public void ANotConfiguredFinishedSessionIsNotInStateNew()
        {
            // given
            var target = CreateSession().Build();

            // when
            target.End();

            // then
            Assert.That(target.State.IsNew, Is.False);
            Assert.That(target.State.IsConfigured, Is.False);
            Assert.That(target.State.IsFinished, Is.True);
            Assert.That(target.State.IsConfiguredAndFinished, Is.False);
        }

        [Test]
        public void AConfiguredFinishedSessionIsNotNew()
        {
            // given
            mockBeacon.IsServerConfigurationSet.Returns(true);
            var target = CreateSession().Build();
            target.End();

            // when
            target.UpdateServerConfiguration(Substitute.For<IServerConfiguration>());

            // then
            Assert.That(target.State.IsNew, Is.False);
            Assert.That(target.State.IsConfigured, Is.True);
            Assert.That(target.State.IsFinished, Is.True);
            Assert.That(target.State.IsConfiguredAndFinished, Is.True);
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
            Assert.That(((IActionInternals)rootActionOne).IsActionLeft, Is.True);
            Assert.That(((IActionInternals)rootActionTwo).IsActionLeft, Is.True);
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
            mockLogger.Received(1).Warn("Session [sn=0] TraceWebRequest(String): url must not be null or empty");
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
            mockLogger.Received(1).Warn("Session [sn=0] TraceWebRequest(String): url must not be null or empty");
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
            mockLogger.Received(1).Warn($"Session [sn=0] TraceWebRequest(String): url \"{url}\" does not have a valid scheme");
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
        public void ToStringReturnsAppropriateResult()
        {
            // given
            const int sessionNumber = 42;
            mockBeacon.SessionNumber.Returns(sessionNumber);

            var target = CreateSession().Build();

            // when
            var obtained = target.ToString();

            // then
            Assert.That(obtained, Is.EqualTo($"Session [sn={sessionNumber}]"));
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
        public void IsDataSendingAllowedReturnsTrueForConfiguredAndCaptureEnabledSession()
        {
            // given
            mockBeacon.IsCaptureEnabled.Returns(true);
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
            mockBeacon.IsCaptureEnabled.Returns(true);
            mockBeacon.IsServerConfigurationSet.Returns(false);

            var target = CreateSession().Build();

            // when
            var obtained = target.IsDataSendingAllowed;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void IsDataSendingAllowedReturnsFalseForCaptureDisabledSession()
        {
            // given
            mockBeacon.IsCaptureEnabled.Returns(false);
            mockBeacon.IsServerConfigurationSet.Returns(true);

            var target = CreateSession().Build();

            // when
            var obtained = target.IsDataSendingAllowed;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void IsDataSendingAllowedReturnsFalseForNotConfiguredAndCaptureDisabledSession()
        {
            // given
            mockBeacon.IsCaptureEnabled.Returns(false);
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

        private TestSessionBuilder CreateSession()
        {
            return new TestSessionBuilder()
                    .With(mockLogger)
                    .With(mockBeacon)
                ;
        }
    }
}
