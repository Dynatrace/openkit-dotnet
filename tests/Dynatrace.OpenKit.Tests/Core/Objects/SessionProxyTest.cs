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
        private ITimingProvider mockTimingProvider;
        private IServerConfiguration mockServerConfiguration;
        private IBeaconSender mockBeaconSender;
        private ISessionWatchdog mockSessionWatchdog;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
            mockLogger.IsDebugEnabled.Returns(true);
            mockLogger.IsWarnEnabled.Returns(true);

            mockParent = Substitute.For<IOpenKitComposite>();
            mockTimingProvider = Substitute.For<ITimingProvider>();
            mockServerConfiguration = Substitute.For<IServerConfiguration>();

            mockBeacon = Substitute.For<IBeacon>();
            mockBeacon.IsActionReportingAllowedByPrivacySettings.Returns(true);
            mockSession = Substitute.For<ISessionInternals>();
            mockSession.Beacon.Returns(mockBeacon);

            mockSplitBeacon1 = Substitute.For<IBeacon>();
            mockSplitBeacon1.IsActionReportingAllowedByPrivacySettings.Returns(true);
            mockSplitSession1 = Substitute.For<ISessionInternals>();
            mockSplitSession1.Beacon.Returns(mockSplitBeacon1);

            mockSplitBeacon2 = Substitute.For<IBeacon>();
            mockSplitBeacon2.IsActionReportingAllowedByPrivacySettings.Returns(true);
            mockSplitSession2 = Substitute.For<ISessionInternals>();
            mockSplitSession2.Beacon.Returns(mockSplitBeacon2);

            mockSessionCreator = Substitute.For<ISessionCreator>();
            mockSessionCreator.CreateSession(Arg.Any<IOpenKitComposite>())
                .Returns(mockSession, mockSplitSession1, mockSplitSession2);

            mockBeaconSender = Substitute.For<IBeaconSender>();
            mockSessionWatchdog = Substitute.For<ISessionWatchdog>();
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
        public void InitiallyCreatedSessionIsInitializedWithServerConfiguration()
        {
            // given
            var initialServerConfig = Substitute.For<IServerConfiguration>();
            mockBeaconSender.LastServerConfiguration.Returns(initialServerConfig);

            // when
            var target = CreateSessionProxy();

            // then
            mockSessionCreator.Received(1).CreateSession(target);
            mockSession.Received(1).InitializeServerConfiguration(initialServerConfig);
        }

        [Test]
        public void ANewlyCreatedSessionProxyIsNotFinished()
        {
            // given
            var target = CreateSessionProxy();

            // then
            Assert.That(target.IsFinished, Is.False);
        }

        [Test]
        public void InitiallyCreatedSessionRegistersServerConfigurationUpdateCallback()
        {
            // given, when
            var target = CreateSessionProxy();

            // then
            mockBeacon.Received(1).OnServerConfigurationUpdate += target.OnServerConfigurationUpdate;
        }

        [Test]
        public void InitiallyCreatedSessionIsAddedToTheBeaconSender()
        {
            // given
            CreateSessionProxy();

            // then
            mockBeaconSender.Received(1).AddSession(mockSession);
        }

        [Test]
        public void InitiallyCreatedSessionProvidesStartTimeAsLastInteractionTime()
        {
            // given
            const int startTime = 73;
            mockBeacon.SessionStartTime.Returns(startTime);

            // when
            var target = CreateSessionProxy();

            // then
            Assert.That(target.LastInteractionTime, Is.EqualTo(startTime));
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
        public void EnterActionSetsLastInterActionTime()
        {
            // given
            const long sessionCreationTime = 13;
            const long lastInteractionTime = 17;
            mockBeacon.SessionStartTime.Returns(sessionCreationTime);
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(lastInteractionTime);

            var target = CreateSessionProxy();
            Assert.That(target.LastInteractionTime, Is.EqualTo(sessionCreationTime));

            // when
            target.EnterAction("test");

            // then
            Assert.That(target.LastInteractionTime, Is.EqualTo(lastInteractionTime));
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
            var serverConfig = ServerConfiguration.From(ResponseAttributes.WithUndefinedDefaults()
                .WithMaxEventsPerSession(maxEventCount).Build());

            Assert.That(serverConfig.IsSessionSplitByEventsEnabled, Is.True);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);

            target.OnServerConfigurationUpdate(serverConfig);

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

        [Test]
        public void EnterActionCallsWatchdogToCloseOldSessionOnSplitByEvents()
        {
            // given
            const int sessionIdleTimeout = 10;
            mockServerConfiguration.IsSessionSplitByEventsEnabled.Returns(true);
            mockServerConfiguration.MaxEventsPerSession.Returns(1);
            mockServerConfiguration.SessionTimeoutInMilliseconds.Returns(sessionIdleTimeout);

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
            mockSessionWatchdog.Received(1).CloseOrEnqueueForClosing(mockSession, sessionIdleTimeout / 2);
        }

        [Test]
        public void EnterActionAddsSplitSessionToBeaconSenderOnSplitByEvents()
        {
            // given
            mockServerConfiguration.IsSessionSplitByEventsEnabled.Returns(true);
            mockServerConfiguration.MaxEventsPerSession.Returns(1);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);
            mockBeaconSender.Received(1).AddSession(mockSession);

            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // when
            target.EnterAction("some action");

            // then
            mockSessionCreator.Received(1).CreateSession(target);

            // and when
            target.EnterAction("some other action");

            // then
            mockSessionCreator.Received(2).CreateSession(target);
            mockBeaconSender.Received(1).AddSession(mockSplitSession1);
        }

        [Test]
        public void EnterActionOnlySetsLastInteractionTimeIfActionReportingIsNotAllowed()
        {
            // given
            long sessionCreationTime = 13;
            long lastInteractionTime = 17;
            mockBeacon.SessionStartTime.Returns(sessionCreationTime);
            mockBeacon.IsActionReportingAllowedByPrivacySettings.Returns(false);
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(lastInteractionTime);

            var target = CreateSessionProxy();

            mockSessionCreator.Received(1).CreateSession(target);

            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // when
            target.EnterAction("test");

            // then
            Assert.That(target.TopLevelActionCount, Is.EqualTo(0));
            Assert.That(target.LastInteractionTime, Is.EqualTo(lastInteractionTime));
        }

        #endregion

        #region identify user tests

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
        public void IdentifyUserSetsLastInterActionTime()
        {
            // given
            const long sessionCreationTime = 13;
            const long lastInteractionTime = 17;
            mockBeacon.SessionStartTime.Returns(sessionCreationTime);
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(lastInteractionTime);

            var target = CreateSessionProxy();
            Assert.That(target.LastInteractionTime, Is.EqualTo(sessionCreationTime));

            // when
            target.IdentifyUser("Jane Doe");

            // then
            Assert.That(target.LastInteractionTime, Is.EqualTo(lastInteractionTime));
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

            target.OnServerConfigurationUpdate(mockServerConfiguration);

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

            target.OnServerConfigurationUpdate(mockServerConfiguration);

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

            target.OnServerConfigurationUpdate(mockServerConfiguration);

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

            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // when
            target.ReportCrash("errorName", "reason", "stacktrace");

            // then
            Assert.That(target.TopLevelActionCount, Is.EqualTo(0));
        }

        [Test]
        public void ReportCrashAlwaysSplitsSessionAfterReportingCrash()
        {
            // given
            // explicitly disable session splitting
            mockServerConfiguration.IsSessionSplitByEventsEnabled.Returns(false);
            mockServerConfiguration.IsSessionSplitByIdleTimeoutEnabled.Returns(false);
            mockServerConfiguration.IsSessionSplitBySessionDurationEnabled.Returns(false);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);

            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // when
            target.ReportCrash("error 1", "reason 1", "stacktrace 1");

            // then
            mockSession.Received(1).ReportCrash("error 1", "reason 1", "stacktrace 1");
            mockSessionCreator.Received(2).CreateSession(target);
            mockSessionWatchdog.Received(1).CloseOrEnqueueForClosing(mockSession,
                mockServerConfiguration.SendIntervalInMilliseconds);

            // and when
            target.ReportCrash("error 2", "reason 2", "stacktrace 2");

            // then
            mockSplitSession1.Received(1).ReportCrash("error 2", "reason 2", "stacktrace 2");
            mockSessionCreator.Received(3).CreateSession(target);
            mockSessionWatchdog.Received(1).CloseOrEnqueueForClosing(mockSplitSession1,
                mockServerConfiguration.SendIntervalInMilliseconds);
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
        public void TraceWebRequestWithStringUrlSetsLastInterActionTime()
        {
            // given
            const long sessionCreationTime = 13;
            const long lastInteractionTime = 17;
            mockBeacon.SessionStartTime.Returns(sessionCreationTime);
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(lastInteractionTime);

            var target = CreateSessionProxy();
            Assert.That(target.LastInteractionTime, Is.EqualTo(sessionCreationTime));

            // when
            target.TraceWebRequest("https://localhost");

            // then
            Assert.That(target.LastInteractionTime, Is.EqualTo(lastInteractionTime));
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
        public void EndRemovesSessionProxyFromSessionWatchdog()
        {
            // given
            var target = CreateSessionProxy();

            // when
            target.End();

            // then
            mockSessionWatchdog.Received(1).RemoveFromSplitByTimeout(target);
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

        #region split session by time

        [Test]
        public void SplitSessionByTimeReturnsMinusOneIfSessionProxyIsFinished()
        {
            // given
            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);

            target.End();

            // when
            var obtained = target.SplitSessionByTime();

            // then
            Assert.That(obtained, Is.EqualTo(-1L));
            mockSessionCreator.Received(1).CreateSession(target);
        }

        [Test]
        public void SplitSessionByTimeReturnsMinusOneIfServerConfigurationIsNotSet()
        {
            // given
            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);

            // when
            var obtained = target.SplitSessionByTime();

            // then
            Assert.That(obtained, Is.EqualTo(-1L));
            mockSessionCreator.Received(1).CreateSession(target);
        }

        [Test]
        public void SplitByTimeDoesNotPerformSplitIfNeitherSplitByIdleTimeoutNorSplitByDurationEnabled()
        {
            // given
            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);

            mockServerConfiguration.IsSessionSplitByIdleTimeoutEnabled.Returns(false);
            mockServerConfiguration.IsSessionSplitBySessionDurationEnabled.Returns(false);

            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // when
            var obtained = target.SplitSessionByTime();

            // then
            Assert.That(obtained, Is.EqualTo(-1L));
            mockSessionCreator.Received(1).CreateSession(target);
        }

        [Test]
        public void SplitByTimeSplitsCurrentSessionIfIdleTimeoutReached()
        {
            // given
            const long lastInteractionTimeSessionOne = 60;
            const int idleTimeout = 10; // time to split: last interaction + idle => 70
            const long currentTime = 70;
            const long sessionTwoCreationTime = 80;
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(lastInteractionTimeSessionOne, currentTime);
            mockSplitBeacon1.SessionStartTime.Returns(sessionTwoCreationTime);

            mockServerConfiguration.IsSessionSplitByIdleTimeoutEnabled.Returns(true);
            mockServerConfiguration.SessionTimeoutInMilliseconds.Returns(idleTimeout);
            mockServerConfiguration.IsSessionSplitBySessionDurationEnabled.Returns(false);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);
            target.OnServerConfigurationUpdate(mockServerConfiguration);

            target.IdentifyUser("test"); // update last interaction time
            Assert.That(target.LastInteractionTime, Is.EqualTo(lastInteractionTimeSessionOne));

            // when
            var obtained = target.SplitSessionByTime();

            // then
            mockSessionWatchdog.Received(1).CloseOrEnqueueForClosing(mockSession, idleTimeout / 2);
            mockSessionCreator.Received(1).Reset();
            mockSessionCreator.Received(2).CreateSession(target);
            Assert.That(obtained, Is.EqualTo(sessionTwoCreationTime + idleTimeout));
        }

        [Test]
        public void SplitByTimeSplitsCurrentSessionIfIdleTimeoutExceeded()
        {
            // given
            const long lastInteractionTimeSessionOne = 60;
            const int idleTimeout = 10; // time to split: last interaction + idle => 70
            const long currentTime = 80;
            const long sessionTwoCreationTime = 90;
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(lastInteractionTimeSessionOne, currentTime);
            mockSplitBeacon1.SessionStartTime.Returns(sessionTwoCreationTime);

            mockServerConfiguration.IsSessionSplitByIdleTimeoutEnabled.Returns(true);
            mockServerConfiguration.SessionTimeoutInMilliseconds.Returns(idleTimeout);
            mockServerConfiguration.IsSessionSplitBySessionDurationEnabled.Returns(false);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);
            target.OnServerConfigurationUpdate(mockServerConfiguration);

            target.IdentifyUser("test"); // update last interaction time
            Assert.That(target.LastInteractionTime, Is.EqualTo(lastInteractionTimeSessionOne));

            // when
            var obtained = target.SplitSessionByTime();

            // then
            mockSessionWatchdog.Received(1).CloseOrEnqueueForClosing(mockSession, idleTimeout / 2);
            mockSessionCreator.Received(1).Reset();
            mockSessionCreator.Received(2).CreateSession(target);
            Assert.That(obtained, Is.EqualTo(sessionTwoCreationTime + idleTimeout));
        }

        [Test]
        public void SplitByTimeDoesNotSplitCurrentSessionIfIdleTimeoutNotExpired()
        {
            // given
            const long lastInteractionTime = 60;
            const int idleTimeout = 20; // time to split: list interaction + idle => 80
            const long currentTime = 70;
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(lastInteractionTime, currentTime);

            mockServerConfiguration.IsSessionSplitByIdleTimeoutEnabled.Returns(true);
            mockServerConfiguration.SessionTimeoutInMilliseconds.Returns(idleTimeout);
            mockServerConfiguration.IsSessionSplitBySessionDurationEnabled.Returns(false);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);
            target.OnServerConfigurationUpdate(mockServerConfiguration);

            target.IdentifyUser("test"); // update last interaction time
            Assert.That(target.LastInteractionTime, Is.EqualTo(lastInteractionTime));

            // when
            var obtained = target.SplitSessionByTime();

            // then
            Assert.That(obtained, Is.EqualTo(lastInteractionTime + idleTimeout));
            mockSessionCreator.Received(1).CreateSession(target);
        }

        [Test]
        public void SplitByTimeSplitsCurrentSessionIfMaxDurationReached()
        {
            // given
            const long startTimeFirstSession = 60;
            const int sessionDuration = 10; // split time: session start + duration = 70
            const int sendInterval = 15;
            const long currentTime = 70;
            const long startTimeSecondSession = 80;

            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(currentTime);
            mockBeacon.SessionStartTime.Returns(startTimeFirstSession);
            mockSplitBeacon1.SessionStartTime.Returns(startTimeSecondSession);

            mockServerConfiguration.IsSessionSplitBySessionDurationEnabled.Returns(true);
            mockServerConfiguration.MaxSessionDurationInMilliseconds.Returns(sessionDuration);
            mockServerConfiguration.IsSessionSplitByIdleTimeoutEnabled.Returns(false);
            mockServerConfiguration.SendIntervalInMilliseconds.Returns(sendInterval);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);
            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // when
            var obtained = target.SplitSessionByTime();

            // then
            mockSessionWatchdog.Received(1).CloseOrEnqueueForClosing(mockSession, sendInterval);
            mockSessionCreator.Received(1).Reset();
            mockSessionCreator.Received(2).CreateSession(target);
            Assert.That(obtained, Is.EqualTo(startTimeSecondSession + sessionDuration));
        }

        [Test]
        public void SplitByTimeSplitsCurrentSessionIfMaxDurationExceeded()
        {
            // given
            const long startTimeFirstSession = 60;
            const int sessionDuration = 10; // split time: session start + duration => 70
            const int sendInterval = 15;
            const long currentTime = 80;
            const long startTimeSecondSession = 90;

            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(currentTime);
            mockBeacon.SessionStartTime.Returns(startTimeFirstSession);
            mockSplitBeacon1.SessionStartTime.Returns(startTimeSecondSession);

            mockServerConfiguration.IsSessionSplitBySessionDurationEnabled.Returns(true);
            mockServerConfiguration.MaxSessionDurationInMilliseconds.Returns(sessionDuration);
            mockServerConfiguration.IsSessionSplitByIdleTimeoutEnabled.Returns(false);
            mockServerConfiguration.SendIntervalInMilliseconds.Returns(sendInterval);


            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);
            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // when
            var obtained = target.SplitSessionByTime();

            // then
            mockSessionWatchdog.Received(1).CloseOrEnqueueForClosing(mockSession, sendInterval);
            mockSessionCreator.Received(1).Reset();
            mockSessionCreator.Received(2).CreateSession(target);
            Assert.That(obtained, Is.EqualTo(startTimeSecondSession + sessionDuration));
        }

        [Test]
        public void SplitByTimeDoesNotSplitCurrentSessionIfMaxDurationNotReached()
        {
            // given
            const long sessionStartTime = 60;
            const int sessionDuration = 20; // split time: start time + duration => 80
            const long currentTime = 70;

            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(currentTime);
            mockBeacon.SessionStartTime.Returns(sessionStartTime);

            mockServerConfiguration.IsSessionSplitBySessionDurationEnabled.Returns(true);
            mockServerConfiguration.MaxSessionDurationInMilliseconds.Returns(sessionDuration);
            mockServerConfiguration.IsSessionSplitByIdleTimeoutEnabled.Returns(false);


            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);
            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // when
            var obtained = target.SplitSessionByTime();

            // then
            mockSessionCreator.Received(1).CreateSession(target);
            Assert.That(obtained, Is.EqualTo(sessionStartTime + sessionDuration));
        }

        [Test]
        public void SplitBySessionTimeReturnsIdleSplitTimeWhenBeforeSessionDurationSplitTime()
        {
            // given
            const long sessionStartTime = 50;
            const int sessionDuration = 40; // duration split time: start time + duration => 90
            const long lastInteractionTime = 60;
            const int idleTimeout = 20; // idle split time: last interaction + idle => 80
            const long currentTime = 70;

            mockBeacon.SessionStartTime.Returns(sessionStartTime);
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(lastInteractionTime, currentTime);

            mockServerConfiguration.IsSessionSplitByIdleTimeoutEnabled.Returns(true);
            mockServerConfiguration.SessionTimeoutInMilliseconds.Returns(idleTimeout);
            mockServerConfiguration.IsSessionSplitBySessionDurationEnabled.Returns(true);
            mockServerConfiguration.MaxSessionDurationInMilliseconds.Returns(sessionDuration);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);
            target.OnServerConfigurationUpdate(mockServerConfiguration);

            target.IdentifyUser("test"); // update last interaction time

            // when
            long obtained = target.SplitSessionByTime();

            // then
            mockSessionCreator.Received(1).CreateSession(target);
            Assert.That(obtained, Is.EqualTo(lastInteractionTime + idleTimeout));
        }

        [Test]
        public void SplitBySessionTimeReturnsDurationSplitTimeWhenBeforeIdleSplitTime()
        {
            // given
            const long sessionStartTime = 50;
            const int sessionDuration = 30; // duration split time: start time + duration => 80
            const long lastInteractionTime = 60;
            const int idleTimeout = 50; // idle split time: last interaction + idle => 110
            const long currentTime = 70;

            mockBeacon.SessionStartTime.Returns(sessionStartTime);
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(lastInteractionTime, currentTime);

            mockServerConfiguration.IsSessionSplitByIdleTimeoutEnabled.Returns(true);
            mockServerConfiguration.SessionTimeoutInMilliseconds.Returns(idleTimeout);
            mockServerConfiguration.IsSessionSplitBySessionDurationEnabled.Returns(true);
            mockServerConfiguration.MaxSessionDurationInMilliseconds.Returns(sessionDuration);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);
            target.OnServerConfigurationUpdate(mockServerConfiguration);

            target.IdentifyUser("test"); // update last interaction time

            // when
            var obtained = target.SplitSessionByTime();

            // then
            mockSessionCreator.Received(1).CreateSession(target);
            Assert.That(obtained, Is.EqualTo(sessionStartTime + sessionDuration));
        }

        #endregion

        #region identifyUser on split sessions

        [Test]
        public void SplitByTimeReAppliesUserIdentificationTag()
        {
            // given
            const long lastInteractionTimeSessionOne = 60;
            const int idleTimeout = 10; // time to split: last interaction + idle => 70
            const long currentTime = 70;
            const long sessionTwoCreationTime = 80;
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(lastInteractionTimeSessionOne, currentTime);
            mockSplitBeacon1.SessionStartTime.Returns(sessionTwoCreationTime);

            mockServerConfiguration.IsSessionSplitByIdleTimeoutEnabled.Returns(true);
            mockServerConfiguration.SessionTimeoutInMilliseconds.Returns(idleTimeout);
            mockServerConfiguration.IsSessionSplitBySessionDurationEnabled.Returns(false);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);
            target.OnServerConfigurationUpdate(mockServerConfiguration);

            target.IdentifyUser("test"); // update last interaction time
            Assert.That(target.LastInteractionTime, Is.EqualTo(lastInteractionTimeSessionOne));

            // when
            target.SplitSessionByTime();

            // then
            mockSplitSession1.Received(1).IdentifyUser("test");
        }

        [Test]
        public void SplitByEventCountReAppliesUserIdentificationTag()
        {
            // given
            const int maxEventCount = 1;
            mockServerConfiguration.IsSessionSplitByEventsEnabled.Returns(true);
            mockServerConfiguration.MaxEventsPerSession.Returns(maxEventCount);

            var target = CreateSessionProxy();
            mockSessionCreator.Received(1).CreateSession(target);

            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // when
            target.IdentifyUser("test1");
            target.EnterAction("action 1");
            target.EnterAction("action 2");

            // then
            mockSplitSession1.Received(1).IdentifyUser("test1");

            // and when
            target.EnterAction("action 3");

            // then
            mockSplitSession2.Received(1).IdentifyUser("test1");
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
        public void OnChildClosedCallsDequeueOnSessionWatchdog()
        {
            // given
            var target = CreateSessionProxy() as IOpenKitComposite;
            var session = Substitute.For<ISessionInternals>();
            target.StoreChildInList(session);

            // when
            target.OnChildClosed(session);

            // then
            mockSessionWatchdog.Received(1).DequeueFromClosing(session);
        }

        [Test]
        public void OnServerConfigurationUpdateTakesOverServerConfigurationOnFirstCall()
        {
            // given
            var target = CreateSessionProxy();

            mockServerConfiguration.IsSessionSplitByEventsEnabled.Returns(true);
            mockServerConfiguration.MaxEventsPerSession.Returns(1);

            Assert.That(target.ServerConfiguration, Is.Null);

            // when
            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // then
            Assert.That(target.ServerConfiguration, Is.EqualTo(mockServerConfiguration));
        }

        [Test]
        public void OnServerConfigurationUpdateMergesServerConfigurationOnConsecutiveCalls()
        {
            // given
            var target = CreateSessionProxy();

            var mockFirstConfig = Substitute.For<IServerConfiguration>();
            var mockSecondConfig = Substitute.For<IServerConfiguration>();

            // when
            target.OnServerConfigurationUpdate(mockFirstConfig);

            // then
            mockFirstConfig.Received(0).Merge(Arg.Any<IServerConfiguration>());
            _ = mockFirstConfig.Received(1).IsSessionSplitBySessionDurationEnabled;
            _ = mockFirstConfig.Received(1).IsSessionSplitByIdleTimeoutEnabled;

            // and when
            target.OnServerConfigurationUpdate(mockSecondConfig);

            // then
            mockFirstConfig.Received(1).Merge(mockSecondConfig);
            Assert.That(mockSecondConfig.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void OnServerConfigurationUpdateAddsSessionProxyToWatchdogIfSplitByDurationEnabled()
        {
            // given
            var target = CreateSessionProxy();
            mockServerConfiguration.IsSessionSplitBySessionDurationEnabled.Returns(true);

            // when
            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // then
            mockSessionWatchdog.Received(1).AddToSplitByTimeout(target);
        }

        [Test]
        public void OnServerConfigurationUpdateAddsSessionProxyToWatchdogIfSplitByIdleTimeoutEnabled()
        {
            // given
            var target = CreateSessionProxy();
            mockServerConfiguration.IsSessionSplitByIdleTimeoutEnabled.Returns(true);

            // when
            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // then
            mockSessionWatchdog.Received(1).AddToSplitByTimeout(target);
        }

        [Test]
        public void OnServerConfigurationUpdateDoesNotAddSessionProxyToWatchdogIfSplitByIdleTimeoutAndDurationDisabled()
        {
            // given
            var target = CreateSessionProxy();
            mockServerConfiguration.IsSessionSplitByIdleTimeoutEnabled.Returns(false);
            mockServerConfiguration.IsSessionSplitBySessionDurationEnabled.Returns(false);

            // when
            target.OnServerConfigurationUpdate(mockServerConfiguration);

            // then
            Assert.That(mockSessionWatchdog.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void OnServerConfigurationUpdateDoesNotAddSessionProxyToWatchdogOnConsecutiveCalls()
        {
            // given
            var target = CreateSessionProxy();
            mockServerConfiguration.IsSessionSplitByIdleTimeoutEnabled.Returns(false);
            mockServerConfiguration.IsSessionSplitBySessionDurationEnabled.Returns(false);

            target.OnServerConfigurationUpdate(mockServerConfiguration);

            var mockServerConfigTwo = Substitute.For<IServerConfiguration>();
            mockServerConfigTwo.IsSessionSplitByIdleTimeoutEnabled.Returns(true);
            mockServerConfigTwo.IsSessionSplitBySessionDurationEnabled.Returns(true);

            // when
            target.OnServerConfigurationUpdate(mockServerConfigTwo);

            // then
            Assert.That(mockSessionWatchdog.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ToStringReturnsAppropriateResult()
        {
            // given
            mockBeacon.SessionNumber.Returns(37);
            mockBeacon.SessionSequenceNumber.Returns(73);
            var target = CreateSessionProxy();

            // when
            var obtained = target.ToString();

            // then
            Assert.That(obtained, Is.EqualTo($"SessionProxy [sn=37, seq=73]"));
        }

        #endregion

        private SessionProxy CreateSessionProxy()
        {
            return new SessionProxy(mockLogger, mockParent, mockSessionCreator, mockTimingProvider, mockBeaconSender,
                mockSessionWatchdog);
        }
    }
}