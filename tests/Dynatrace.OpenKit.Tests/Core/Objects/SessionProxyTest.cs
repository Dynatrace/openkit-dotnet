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

using System.Linq;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Objects
{
    public class SessionProxyTest
    {
        private ILogger mockLogger;
        private IOpenKitComposite mockParent;
        private ISessionInternals mockSession;
        private IBeacon mockBeacon;
        private ISessionInternals mockSplitSession1;
        private IBeacon mockSplitBeacon1;
        private ISessionInternals mockSplitSession2;
        private IBeacon mockSplitBeacon2;
        private ISessionCreator mockSessionCreator;
        private IServerConfiguration mockServerConfiguration;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
            mockLogger.IsDebugEnabled.Returns(true);
            mockLogger.IsWarnEnabled.Returns(true);

            mockParent = Substitute.For<IOpenKitComposite>();
            mockServerConfiguration = Substitute.For<IServerConfiguration>();

            mockBeacon = Substitute.For<IBeacon>();
            mockSession = Substitute.For<ISessionInternals>();
            mockSession.Beacon.Returns(mockBeacon);

            mockSplitBeacon1 = Substitute.For<IBeacon>();
            mockSplitSession1 = Substitute.For<ISessionInternals>();
            mockSplitSession1.Beacon.Returns(mockSplitBeacon1);

            mockSplitBeacon2 = Substitute.For<IBeacon>();
            mockSplitSession2 = Substitute.For<ISessionInternals>();
            mockSplitSession2.Beacon.Returns(mockSplitBeacon2);

            mockSessionCreator = Substitute.For<ISessionCreator>();
            mockSessionCreator.CreateSession(Arg.Any<IOpenKitComposite>())
                .Returns(mockSession, mockSplitSession1, mockSplitSession2);
        }

        [Test]
        public void ConstructingASessionCreatorCreatesASessionInitially()
        {
            // given, when
            var target = CreateSessionProxy();

            // then
            mockSessionCreator.Received(1).CreateSession(target);
        }

        [Test]
        public void InitiallyCreatedSessionRegistersServerConfigurationUpdateCallback()
        {
            // given, when
            var target = CreateSessionProxy();

            // then
            mockBeacon.Received(1).OnServerConfigurationUpdate += target.OnServerConfigurationUpdate;
        }

        #region enter action tests

        [Test]
        public void EnterActionWithNullActionNameGivesNullRootActionObject()
        {
            // given
            var target = CreateSessionProxy();

            // when
            var obtained = target.EnterAction(null);

            // then
            Assert.That(obtained, Is.InstanceOf<NullRootAction>());

            mockLogger.Received(1).Warn($"{target} EnterAction: actionName must not be null or empty");
        }

        [Test]
        public void EnterActionWithEmptyActionNameGivesNullRootActionObject()
        {
            // given
            var target = CreateSessionProxy();

            // when
            var obtained = target.EnterAction("");

            // then
            Assert.That(obtained, Is.InstanceOf<NullRootAction>());

            // ensure that some log message has been written
            mockLogger.Received(1).Warn($"{target} EnterAction: actionName must not be null or empty");
        }

        [Test]
        public void EnterActionDelegatesToRealSession()
        {
            // given
            var actionName = "some action";
            var target = CreateSessionProxy();

            // when entering the first time
            target.EnterAction(actionName);

            // then
            mockSession.Received(1).EnterAction(actionName);
        }

        [Test]
        public void EnterActionLogsInvocation()
        {
            // given
            var actionName = "Some action";
            var target = CreateSessionProxy();

            // when
            target.EnterAction(actionName);

            // then
            mockLogger.Received(1).Debug($"{target} EnterAction({actionName})");
            _ = mockLogger.Received(1).IsDebugEnabled;
        }

        [Test]
        public void EnterActionGivesNullRootActionIfSessionIsAlreadyEnded()
        {
            // given
            var target = CreateSessionProxy();
            target.End();

            // when entering an action on already ended session
            var obtained = target.EnterAction("Test");

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<NullRootAction>());
        }

        [Test]
        public void EnterActionIncreasesTopLevelEventCount()
        {
            // given
            var target = CreateSessionProxy();
            Assert.That(target.TopLevelActionCount, Is.EqualTo(0));

            // when
            target.EnterAction("test");

            // then
            Assert.That(target.TopLevelActionCount, Is.EqualTo(1));
        }

        [Test]
        public void EnterActionDoesNotSplitSessionIfNoServerConfigurationIsSet()
        {
            // given
            const int eventCount = 10;

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);

            // when
            for (var i = 0; i < eventCount; i++)
            {
                target.EnterAction("some action");
            }

            // then
            Assert.That(target.TopLevelActionCount, Is.EqualTo(eventCount));
            mockSessionCreator.Received(1).CreateSession(target);
        }

        [Test]
        public void EnterActionDoesNotSplitSessionIfSessionSplitByEventDisabled()
        {
            // given
            const int eventCount = 10;
            mockServerConfiguration.IsSessionSplitByEventsEnabled.Returns(false);
            mockServerConfiguration.MaxEventsPerSession.Returns(1);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);

            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // when
            for (var i = 0; i < eventCount; i++)
            {
                target.EnterAction("some action");
            }

            // then
            Assert.That(target.TopLevelActionCount, Is.EqualTo(eventCount));
            mockSessionCreator.Received(1).CreateSession(target);
        }

        [Test]
        public void EnterActionSplitsSessionIfSessionSplitByEventsEnabled()
        {
            // given
            mockServerConfiguration.IsSessionSplitByEventsEnabled.Returns(true);
            mockServerConfiguration.MaxEventsPerSession.Returns(1);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);

            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // when
            target.EnterAction("some action");

            // then
            mockSessionCreator.Received(1).CreateSession(target);

            // and when
            target.EnterAction("some other action");

            // then
            mockSessionCreator.Received(2).CreateSession(target);
        }

        [Test]
        public void EnterActionSplitsSessionEveryNthEvent()
        {
            // given
            const int maxEventCount = 3;
            mockServerConfiguration.IsSessionSplitByEventsEnabled.Returns(true);
            mockServerConfiguration.MaxEventsPerSession.Returns(maxEventCount);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);

            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // when
            target.EnterAction("action 1");
            target.EnterAction("action 2");
            target.EnterAction("action 3");

            // then
            mockSessionCreator.Received(1).CreateSession(target);

            // and when
            target.EnterAction("action 4");

            // then
            mockSessionCreator.Received(2).CreateSession(target);

            // and when
            target.EnterAction("action 5");
            target.EnterAction("action 6");

            // then
            mockSessionCreator.Received(2).CreateSession(target);

            // and when
            target.EnterAction("action 7");

            // then
            mockSessionCreator.Received(3).CreateSession(target);
        }

        [Test]
        public void EnterActionSplitsSessionEveryNthEventFromFirstServerConfiguration()
        {
            // given
            const int maxEventCount = 3;
            mockServerConfiguration.IsSessionSplitByEventsEnabled.Returns(true);
            mockServerConfiguration.MaxEventsPerSession.Returns(maxEventCount);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);

            target.OnServerConfigurationUpdate(mockServerConfiguration);

            var ignoredServerConfig = Substitute.For<IServerConfiguration>();
            ignoredServerConfig.IsSessionSplitByEventsEnabled.Returns(true);
            ignoredServerConfig.MaxEventsPerSession.Returns(5);

            target.OnServerConfigurationUpdate(ignoredServerConfig);

            // when
            target.EnterAction("action 1");
            target.EnterAction("action 2");
            target.EnterAction("action 3");

            // then
            mockSessionCreator.Received(1).CreateSession(target);

            // and when
            target.EnterAction("action 4");

            // then
            mockSessionCreator.Received(2).CreateSession(target);

            // and when
            target.EnterAction("action 5");
            target.EnterAction("action 6");

            // then
            mockSessionCreator.Received(2).CreateSession(target);

            // and when
            target.EnterAction("action 7");

            // then
            mockSessionCreator.Received(3).CreateSession(target);
        }

        #endregion

        #region idntify user tests

        [Test]
        public void IdentifyUserWithNullTagDoesNothing()
        {
            // given
            var target = CreateSessionProxy();

            // when
            target.IdentifyUser(null);

            // then
            mockLogger.Received(1).Warn($"{target} IdentifyUser: userTag must not be null or empty");
            mockSession.Received(0).IdentifyUser(Arg.Any<string>());
        }

        [Test]
        public void IdentifyUserWithEmptyTagDoesNothing()
        {
            // given
            var target = CreateSessionProxy();

            // when
            target.IdentifyUser("");

            // then
            mockLogger.Received(1).Warn($"{target} IdentifyUser: userTag must not be null or empty");
            mockSession.Received(0).IdentifyUser(Arg.Any<string>());
        }

        [Test]
        public void IdentifyUserWithNonEmptyTagReportsUser()
        {
            // given
            const string userTag = "user";
            var target = CreateSessionProxy();

            // when
            target.IdentifyUser(userTag);

            // then
            mockLogger.Received(0).Warn(Arg.Any<string>());
            mockSession.Received(1).IdentifyUser(userTag);
        }

        [Test]
        public void IdentifyUserLogsInvocation()
        {
            // given
            const string userTag = "user";
            var target = CreateSessionProxy();

            // when
            target.IdentifyUser(userTag);

            // then
            mockLogger.Received(1).Debug($"{target} IdentifyUser({userTag})");
            _ = mockLogger.Received(1).IsDebugEnabled;
        }

        [Test]
        public void IdentifyUserDoesNothingIfSessionIsEnded()
        {
            // given
            var target = CreateSessionProxy();
            target.End();

            // when trying to identify a user on an ended session
            target.IdentifyUser("Jane Doe");

            // then
            mockSession.Received(0).IdentifyUser(Arg.Any<string>());
        }

        [Test]
        public void IdentifyUserDoesNotIncreaseTopLevelEventCount()
        {
            // given
            var target = CreateSessionProxy();
            Assert.That(target.TopLevelActionCount, Is.EqualTo(0));

            // when
            target.IdentifyUser("Jane Doe");

            // then
            Assert.That(target.TopLevelActionCount, Is.EqualTo(0));
        }

        [Test]
        public void IdentifyUserDoesNotSplitSession()
        {
            // given
            const int eventCount = 10;
            mockServerConfiguration.IsSessionSplitByEventsEnabled.Returns(true);
            mockServerConfiguration.MaxEventsPerSession.Returns(1);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);

            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // when
            for (var i = 0; i < eventCount; i++)
            {
                target.IdentifyUser("some user");
            }

            // then
            Assert.That(target.TopLevelActionCount, Is.EqualTo(0));
            mockSessionCreator.Received(1).CreateSession(target);
        }

        #endregion

        #region report crash tests

        [Test]
        public void ReportingCrashWithNullErrorNameDoesNotReportAnything()
        {
            // given
            var target = CreateSessionProxy();

            // when reporting a crash, passing null values
            target.ReportCrash(null, "some reason", "some stack trace");

            // then verify the correct methods being called
            mockLogger.Received(1).Warn($"{target} ReportCrash: errorName must not be null or empty");
            mockSession.Received(0).ReportCrash(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void ReportingCrashWithEmptyErrorNameDoesNotReportAnything()
        {
            // given
            var target = CreateSessionProxy();

            // when reporting a crash, passing empty errorName
            target.ReportCrash("", "some reason", "some stack trace");

            // verify the correct methods being called
            mockLogger.Received(1).Warn($"{target} ReportCrash: errorName must not be null or empty");
            mockSession.Received(0).ReportCrash(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void ReportingCrashWithNullReasonAndStacktraceWorks()
        {
            // given
            const string errorName = "errorName";
            const string errorReason = null;
            const string stacktrace = null;
            var target = CreateSessionProxy();

            // when reporting a crash, passing null values
            target.ReportCrash(errorName, errorReason, stacktrace);

            // then verify the correct methods being called
            mockSession.Received(1).ReportCrash(errorName, errorReason, stacktrace);
        }

        [Test]
        public void ReportingCrashWithEmptyReasonAndStacktraceStringWorks()
        {
            // given
            const string errorName = "errorName";
            const string errorReason = "";
            const string stacktrace = "";
            var target = CreateSessionProxy();

            // when reporting a crash, passing null values
            target.ReportCrash(errorName, errorReason, stacktrace);

            // then verify the correct methods being called
            mockSession.Received(1).ReportCrash(errorName, errorReason, stacktrace);
        }

        [Test]
        public void ReportCrashLogsInvocation()
        {
            // given
            var target = CreateSessionProxy();

            const string errorName = "error name";
            const string reason = "error reason";
            const string stacktrace = "the stacktrace causing the error";

            // when
            target.ReportCrash(errorName, reason, stacktrace);

            // verify the correct methods being called
            _ = mockLogger.Received(1).IsDebugEnabled;
            mockLogger.Received(1).Debug($"{target} ReportCrash({errorName}, {reason}, {stacktrace})");
        }

        [Test]
        public void ReportCrashDoesNothingIfSessionIsEnded()
        {
            // given
            var target = CreateSessionProxy();
            target.End();

            // when trying to identify a user on an ended session
            target.ReportCrash("errorName", "reason", "stacktrace");

            // then
            mockSession.Received(0).ReportCrash(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void ReportCrashDoesNotIncreaseTopLevelEventCount()
        {
            // given
            var target = CreateSessionProxy();
            Assert.That(target.TopLevelActionCount, Is.EqualTo(0));

            // when
            target.ReportCrash("errorName", "reason", "stacktrace");

            // then
            Assert.That(target.TopLevelActionCount, Is.EqualTo(0));
        }

        [Test]
        public void ReportCrashUserDoesNotSplitSession()
        {
            // given
            const int eventCount = 10;

            mockServerConfiguration.IsSessionSplitByEventsEnabled.Returns(true);
            mockServerConfiguration.MaxEventsPerSession.Returns(1);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);

            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // when
            for (var i = 0; i < eventCount; i++)
            {
                target.ReportCrash("error", "reason", "stacktrace");
            }

            // then
            Assert.That(target.TopLevelActionCount, Is.EqualTo(0));
            mockSessionCreator.Received(1).CreateSession(target);
        }

        #endregion

        #region trace web request tests

        [Test]
        public void TraceWebRequestWithValidUrlStringDelegatesToRealSession()
        {
            // given
            const string url = "https://www.google.com";
            var target = CreateSessionProxy();

            // when
            target.TraceWebRequest(url);

            // then
            mockSession.Received(1).TraceWebRequest(url);
        }

        [Test]
        public void TracingANullStringWebRequestIsNotAllowed()
        {
            // given
            var target = CreateSessionProxy();

            // when
            var obtained = target.TraceWebRequest(null);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<NullWebRequestTracer>());

            // and a warning message has been generated
            mockLogger.Received(1).Warn($"{target} TraceWebRequest(string): url must not be null or empty");
            mockSession.Received(0).TraceWebRequest(Arg.Any<string>());
        }

        [Test]
        public void TracingAnEmptyStringWebRequestIsNotAllowed()
        {
            // given
            var target = CreateSessionProxy();

            // when
            var obtained = target.TraceWebRequest("");

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<NullWebRequestTracer>());

            // and a warning message has been generated
            mockLogger.Received(1).Warn($"{target} TraceWebRequest(string): url must not be null or empty");
            mockSession.Received(0).TraceWebRequest(Arg.Any<string>());
        }

        [Test]
        public void TracingAStringWebRequestWithInvalidUrlIsNotAllowed()
        {
            // given
            var target = CreateSessionProxy();

            // when
            var obtained = target.TraceWebRequest("foobar/://");

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<NullWebRequestTracer>());

            // and a warning message has been generated
            mockLogger.Received(1)
                .Warn($"{target} TraceWebRequest(string): url \"foobar/://\" does not have a valid scheme");
            mockSession.Received(0).TraceWebRequest(Arg.Any<string>());
        }

        [Test]
        public void TraceWebRequestWithStringArgumentGivesNullTracerIfSessionIsEnded()
        {
            // given
            var target = CreateSessionProxy();
            target.End();

            // when
            var obtained = target.TraceWebRequest("http://www.google.com");

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<NullWebRequestTracer>());
        }

        [Test]
        public void TraceWebRequestWithStringLogsInvocation()
        {
            // given
            const string url = "https://localhost";
            var target = CreateSessionProxy();

            // when
            target.TraceWebRequest(url);

            // then
            _ = mockLogger.Received(1).IsDebugEnabled;
            mockLogger.Received(1).Debug($"{target} TraceWebRequest({url})");
        }

        [Test]
        public void TraceWebRequestWithStringDoesNotIncreaseTopLevelEventCount()
        {
            // given
            var target = CreateSessionProxy();
            Assert.That(target.TopLevelActionCount, Is.EqualTo(0));

            // when
            target.TraceWebRequest("https://localhost");

            // then
            Assert.That(target.TopLevelActionCount, Is.EqualTo(0));
        }

        [Test]
        public void TraceWebRequestWithStringUrlDoesNotSplitSession()
        {
            // given
            const int eventCount = 10;

            mockServerConfiguration.IsSessionSplitByEventsEnabled.Returns(true);
            mockServerConfiguration.MaxEventsPerSession.Returns(1);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);

            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // when
            for (var i = 0; i < eventCount; i++)
            {
                target.TraceWebRequest("https://localhost");
            }

            // then
            Assert.That(target.TopLevelActionCount, Is.EqualTo(0));
            mockSessionCreator.Received(1).CreateSession(target);
        }

        #endregion

        #region end tests

        [Test]
        public void EndFinishesTheSession()
        {
            // given
            var target = CreateSessionProxy();

            // when
            target.End();

            // then
            mockParent.Received(1).OnChildClosed(target);
        }

        [Test]
        public void EndingAnAlreadyEndedSessionDoesNothing()
        {
            // given
            var target = CreateSessionProxy();

            // when ending a session twice
            target.End();


            // then
            mockParent.Received(1).OnChildClosed(target);

            // and when
            target.End();

            // then
            mockParent.Received(1).OnChildClosed(target);
        }

        [Test]
        public void EndingASessionImplicitlyClosesAllOpenChildObjects()
        {
            // given
            var childObjectOne = Substitute.For<IOpenKitObject>();
            var childObjectTwo = Substitute.For<IOpenKitObject>();

            var target = CreateSessionProxy();
            IOpenKitComposite targetExplicit = target;

            targetExplicit.StoreChildInList(childObjectOne);
            targetExplicit.StoreChildInList(childObjectTwo);

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
            var target = CreateSessionProxy();

            // when
            target.End();

            // then
            mockLogger.Received(1).Debug($"{target} End()");
        }

        [Test]
        public void DisposeSessionEndsTheSession()
        {
            // given
            var target = CreateSessionProxy();

            // when
            target.Dispose();

            // then
            mockParent.Received(1).OnChildClosed(target);
        }

        #endregion

        #region further tests

        [Test]
        public void OnChildClosedRemovesChildFromList()
        {
            // given
            var target = CreateSessionProxy() as IOpenKitComposite;
            Assert.That(target.GetCopyOfChildObjects().Count, Is.EqualTo(1)); // initial session

            // when
            var childObject = Substitute.For<IOpenKitObject>();

            target.StoreChildInList(childObject);

            // then
            Assert.That(target.GetCopyOfChildObjects().Count, Is.EqualTo(2));

            // when child gets closed
            target.OnChildClosed(childObject);

            // then
            Assert.That(target.GetCopyOfChildObjects().Count, Is.EqualTo(1));
        }

        [Test]
        public void ToStringReturnsAppropriateResult()
        {
            // given
            var target = CreateSessionProxy();

            // when
            var obtained = target.ToString();

            // then
            Assert.That(obtained, Is.EqualTo("SessionProxy"));
        }

        #endregion

        private SessionProxy CreateSessionProxy()
        {
            return new SessionProxy(mockLogger, mockParent, mockSessionCreator);
        }
    }
}