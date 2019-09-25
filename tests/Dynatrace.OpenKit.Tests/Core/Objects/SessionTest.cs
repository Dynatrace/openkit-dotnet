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
    public class SessionTest
    {
        private ILogger mockLogger;
        private IBeacon mockBeacon;
        private IBeaconSender mockBeaconSender;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
            mockLogger.IsDebugEnabled.Returns(true);
            mockLogger.IsDebugEnabled.Returns(true);

            mockBeacon = Substitute.For<IBeacon>();
            mockBeaconSender = Substitute.For<IBeaconSender>();
        }

        [Test]
        public void ANewlyCreatedSessionIsNotEnded()
        {
            // given
            var target = CreateSession().Build();

            // then
            Assert.That(target.EndTime, Is.EqualTo(-1L));
            Assert.That(target.IsSessionEnded, Is.False);
        }

        [Test]
        public void AfterCallingEndASessionIsEnded()
        {
            // given
            var target = CreateSession().Build();

            // when
            target.End();

            // then
            Assert.That(target.IsSessionEnded, Is.True);
        }

        [Test]
        public void EnterActionGivesNewRootAction()
        {
            // given
            var target = CreateSession().Build();

            // when
            var obtained = target.EnterAction("action name");

            // then
            Assert.That(obtained, Is.Not.Null.And.TypeOf<RootAction>());
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
        public void EnterActionGivesNullActionIfActionNameIsNull()
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
        public void EnterActionGivesNullActionIfActionNameIsEmptyString()
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
        public void IdentifyUser()
        {
            // given
            const string userTag = "john.doe@acme.com";
            var target = CreateSession().Build();

            // when
            target.IdentifyUser(userTag);

            // then
            mockBeacon.Received(1).IdentifyUser(userTag);
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
        public void IdentifyUserDoesNothingIfUserTagIsNull()
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
        public void IdentifyUserDoesNothingIfUserTagIsAnEmptyString()
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
        public void ReportCrash()
        {
            // given
            const string errorName = "crash";
            const string errorReason = "reason for crash";
            const string stacktrace = "the stacktrace";
            var target = CreateSession().Build();

            // when
            target.ReportCrash(errorName, errorReason, stacktrace);

            // then
            mockBeacon.Received(1).ReportCrash(errorName, errorReason, stacktrace);
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
        public void ReportCrashDoesNothingIfErrorNameIsNull()
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
        public void ReportCrashDoesNothingIfErrorNameIsAnEmptyString()
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
            Assert.That(((IActionInternals)rootActionOne).EndTime, Is.LessThan(((IActionInternals)rootActionTwo).EndTime));
            Assert.That(((IActionInternals)rootActionTwo).EndTime, Is.LessThan(target.EndTime));
        }

        [Test]
        public void EndingASessionFinishesSessionContext()
        {
            // given
            const long timestamp = 4321;
            mockBeacon.CurrentTimestamp.Returns(timestamp);

            var target = CreateSession().Build();

            // when
            target.End();

            // then
            Assert.That(target.EndTime, Is.EqualTo(timestamp));
            mockBeacon.Received(1).EndSession(target);
            mockBeaconSender.Received(1).FinishSession(target);
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
            Assert.That(target.EndTime, Is.EqualTo(timestamp));
            mockBeacon.Received(1).EndSession(target);
            mockBeaconSender.Received(1).FinishSession(target);
        }

        [Test]
        public void EndingASessionReturnsImmediatelyIfEndedBefore()
        {
            // given
            var target = CreateSession().Build();
            target.End();

            mockBeacon.ClearReceivedCalls();
            mockBeaconSender.ClearReceivedCalls();

            // when
            target.End();


            // then
            mockBeacon.Received(0).EndSession(Arg.Any<ISessionInternals>());
            Assert.That(mockBeaconSender.ReceivedCalls(), Is.Empty);
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
        public void TraceWebRequestGivesValidWebRequestTracer()
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
        public void TraceWebRequestGivesNullWebRequestTracerIfUrlIsNull()
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
        public void TraceWebRequestGivesNullWebRequestTracerIfUrlIsAnEmptyString()
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
        public void TraceWebRequestGivesNullWebRequestTracerIfUrlHasAnInvalidScheme()
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

        private TestSessionBuilder CreateSession()
        {
            return new TestSessionBuilder()
                .With(mockLogger)
                .With(mockBeacon)
                .With(mockBeaconSender);
        }
    }
}
