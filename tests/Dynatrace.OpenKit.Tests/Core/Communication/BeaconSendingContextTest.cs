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

using System.Collections.Generic;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Core.Util;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Communication
{
    public class BeaconSendingContextTest
    {
        private ILogger mockLogger;
        private IHttpClientConfiguration mockHttpClientConfig;
        private IHttpClientProvider mockHttpClientProvider;
        private ITimingProvider mockTimingProvider;
        private IInterruptibleThreadSuspender mockThreadSuspender;

        [SetUp]
        public void Setup()
        {
            mockLogger = Substitute.For<ILogger>();
            mockLogger.IsInfoEnabled.Returns(true);
            mockLogger.IsDebugEnabled.Returns(true);

            mockHttpClientConfig = Substitute.For<IHttpClientConfiguration>();

            var statusResponse = Substitute.For<IStatusResponse>();
            statusResponse.ResponseCode.Returns(StatusResponse.HttpOk);

            var httpClient = Substitute.For<IHttpClient>();
            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<IAdditionalQueryParameters>())
                .Returns(statusResponse);

            mockHttpClientProvider = Substitute.For<IHttpClientProvider>();
            mockHttpClientProvider.CreateClient(Arg.Any<IHttpClientConfiguration>()).Returns(httpClient);

            mockTimingProvider = Substitute.For<ITimingProvider>();
            mockThreadSuspender = Substitute.For<IInterruptibleThreadSuspender>();
        }

        [Test]
        public void CurrentStateIsInitializedAccordingly()
        {
            // given, when
            var initState = new BeaconSendingInitState();
            var target = CreateSendingContext().With(initState).Build();

            // then
            Assert.That(target.CurrentState, Is.Not.Null);
            Assert.That(target.CurrentState, Is.InstanceOf<BeaconSendingInitState>());
        }

        [Test]
        public void NextStateSetChangesState()
        {
            // given
            var state = Substitute.For<AbstractBeaconSendingState>(false);
            var target = CreateSendingContext().Build();

            // when
            target.NextState = state;

            // then
            Assert.That(target.NextState, Is.SameAs(state));
        }


        [Test]
        public void ExecuteCurrentStateCallsExecuteOnCurrentState()
        {
            // given
            var mockState = Substitute.For<AbstractBeaconSendingState>(false);
            var target = CreateSendingContext().With(mockState).Build();
            Assert.That(mockState.ReceivedCalls(), Is.Empty);

            // when
            target.ExecuteCurrentState();

            // then
            mockState.Received(1).Execute(target);
        }

        [Test]
        public void InitCompletionSuccessAndWait()
        {
            // given
            var target = CreateSendingContext().Build();

            // when
            target.InitCompleted(true);
            var obtained = target.WaitForInit();

            // then
            Assert.That(obtained, Is.True);
        }

        [Test]
        public void RequestShutdown()
        {
            // given
            var target = CreateSendingContext().Build();
            Assert.False(target.IsShutdownRequested);

            // when
            target.RequestShutdown();

            // then
            Assert.True(target.IsShutdownRequested);
        }

        [Test]
        public void RequestShutdownWakesUpThreadSuspender()
        {
            // given
            var target = CreateSendingContext().Build();

            // when
            target.RequestShutdown();

            // then
            mockThreadSuspender.Received(1).WakeUp();
        }

        [Test]
        public void InitCompleteFailureAndWait()
        {
            // given
            var target = CreateSendingContext().Build();

            // when
            target.InitCompleted(false);
            var obtained = target.WaitForInit();

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void WaitForInitCompleteTimeout()
        {
            // given
            var target = CreateSendingContext().Build();

            // when waiting for init completion with a timeout of 1ms
            var obtained = target.WaitForInit(1);

            // then the result must be false, since init was never set, but timeout expired
            Assert.That(obtained, Is.False);
        }


        [Test]
        public void WaitForInitCompleteWhenInitCompletedSuccessfully()
        {
            // given
            var target = CreateSendingContext().Build();
            target.InitCompleted(true);

            // when waiting for init completion with a timeout of 1ms
            var obtained = target.WaitForInit(1);

            // then the result must be false, since init was never set, but timeout expired
            Assert.That(obtained, Is.True);
        }

        [Test]
        public void WaitForInitCompleteWhenInitCompletedNotSuccessfully()
        {
            // given
            var target = CreateSendingContext().Build();
            target.InitCompleted(false);

            // when waiting for init completion with a timeout of 1ms
            var obtained = target.WaitForInit(1);

            // then the result must be false, since init was never set, but timeout expired
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void ADefaultConstructedContextIsNotInitialized()
        {
            // given, when
            var target = CreateSendingContext().Build();

            // then
            Assert.That(target.IsInitialized, Is.False);
        }

        [Test]
        public void SuccessfullyInitializedContextIsInitialized()
        {
            // given
            var target = CreateSendingContext().Build();
            target.InitCompleted(true);

            // when, then
            Assert.That(target.IsInitialized, Is.True);
        }

        [Test]
        public void NotSuccessfullyInitializedContextIsNotInitialized()
        {
            // given
            var target = CreateSendingContext().Build();
            target.InitCompleted(false);

            // when, then
            Assert.That(target.IsInitialized, Is.False);
        }

        [Test]
        public void IsInTerminalStateChecksCurrentState()
        {
            // given
            var target = CreateSendingContext().Build();
            Assert.That(target.IsInTerminalState, Is.False);
            Assert.That(target.CurrentState, Is.InstanceOf<BeaconSendingInitState>());

            var terminalState = new BeaconSendingTerminalState();

            // when
            target.NextState = terminalState;

            // then
            Assert.That(target.IsInTerminalState, Is.False);
            Assert.That(target.CurrentState, Is.InstanceOf<BeaconSendingInitState>());
        }

        [Test]
        public void IsCaptureOnIsTakenFromDefaultServerConfig()
        {
            // given, when
            var target = CreateSendingContext().Build();

            // then
            Assert.That(target.IsCaptureOn, Is.EqualTo(ServerConfiguration.Default.IsCaptureEnabled));
        }

        [Test]
        public void LastOpenSessionSendTimeIsSet()
        {
            // given
            long[] sendTimes = {1234, 5678};
            var target = CreateSendingContext().Build();

            foreach (var sendTime in sendTimes)
            {
                // when
                target.LastOpenSessionBeaconSendTime = sendTime;

                // then
                Assert.That(target.LastOpenSessionBeaconSendTime, Is.EqualTo(sendTime));
            }
        }


        [Test]
        public void LastStatusCheckTimeIsSet()
        {
            // given
            long[] statusCheckTimes = {1234, 56789};
            var target = CreateSendingContext().Build();

            foreach (var statusCheckTime in statusCheckTimes)
            {
                // when
                target.LastStatusCheckTime = statusCheckTime;

                // then
                Assert.That(target.LastStatusCheckTime, Is.EqualTo(statusCheckTime));
            }
        }

        [Test]
        public void SendIntervalIsTakenFromDefaultResponseAttributes()
        {
            // given
            var target = CreateSendingContext().Build();

            // when
            var obtained = target.SendInterval;

            // then
            Assert.That(obtained, Is.EqualTo(ResponseAttributesDefaults.Undefined.SendIntervalInMilliseconds));
        }

        [Test]
        public void HttpClientProviderGet()
        {
            // given
            var target = CreateSendingContext().Build();

            // when
            var obtained = target.HttpClientProvider;

            // then
            Assert.That(obtained, Is.SameAs(mockHttpClientProvider));
        }

        [Test]
        public void GetHttpClient()
        {
            // given
            var mockClient = Substitute.For<IHttpClient>();
            mockHttpClientProvider.CreateClient(Arg.Any<IHttpClientConfiguration>()).Returns(mockClient);

            var target = CreateSendingContext().Build();

            Assert.That(mockHttpClientConfig.ReceivedCalls(), Is.Empty);
            Assert.That(mockHttpClientProvider.ReceivedCalls(), Is.Empty);

            // when
            var obtained = target.GetHttpClient();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.SameAs(mockClient));

            mockHttpClientProvider.Received(1).CreateClient(mockHttpClientConfig);
            Assert.That(mockClient.ReceivedCalls(), Is.Empty);
            Assert.That(mockHttpClientConfig.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void CurrentTimeStampGet()
        {
            // given
            const long expected = 12356789;
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(expected);

            var target = CreateSendingContext().Build();
            Assert.That(mockTimingProvider.ReceivedCalls(), Is.Empty);

            // when
            var obtained = target.CurrentTimestamp;

            // then
            Assert.That(obtained, Is.EqualTo(expected));
            mockTimingProvider.Received(1).ProvideTimestampInMilliseconds();
        }

        [Test]
        public void SleepDefaultTime()
        {
            // given
            var target = CreateSendingContext().Build();
            Assert.That(mockTimingProvider.ReceivedCalls(), Is.Empty);

            // when
            target.Sleep();

            // then
            mockThreadSuspender.Received(1).Sleep(BeaconSendingContext.DefaultSleepTimeMilliseconds);
        }

        [Test]
        public void SleepWithGivenTime()
        {
            // given
            const int expected = 1717;
            var target = CreateSendingContext().Build();

            // when
            target.Sleep(expected);

            // then
            mockThreadSuspender.Received(1).Sleep(expected);
        }

        [Test]
        public void CanInterruptLongSleep()
        {
            // given
            const int expected = 101717;
            var target = CreateSendingContext().Build();
            target.RequestShutdown();
            target.Sleep(expected);

            // then
            mockThreadSuspender.Received(1).Sleep(expected);
        }

        [Test]
        public void ADefaultConstructedContextDoesNotStoreAnySessions()
        {
            // given
            var target = CreateSendingContext().Build();

            // then
            Assert.That(target.GetAllNotConfiguredSessions(), Is.Empty);
            Assert.That(target.GetAllOpenAndConfiguredSessions(), Is.Empty);
            Assert.That(target.GetAllFinishedAndConfiguredSessions(), Is.Empty);
        }

        [Test]
        public void AddSession()
        {
            // given
            var target = CreateSendingContext().Build();
            var mockSessionOne = Substitute.For<ISessionInternals>();
            var mockSessionTwo = Substitute.For<ISessionInternals>();

            // when
            target.AddSession(mockSessionOne);

            // then
            Assert.That(target.SessionCount, Is.EqualTo(1));

            // and when
            target.AddSession(mockSessionTwo);

            // then
            Assert.That(target.SessionCount, Is.EqualTo(2));
        }

        [Test]
        public void RemoveSession()
        {
            // given
            var target = CreateSendingContext().Build();
            var mockSessionOne = Substitute.For<ISessionInternals>();
            var mockSessionTwo = Substitute.For<ISessionInternals>();

            target.AddSession(mockSessionOne);
            target.AddSession(mockSessionTwo);
            Assert.That(target.SessionCount, Is.EqualTo(2));

            // when
            target.RemoveSession(mockSessionOne);

            // then
            Assert.That(target.SessionCount, Is.EqualTo(1));

            // and when
            target.RemoveSession(mockSessionTwo);

            // then
            Assert.That(target.SessionCount, Is.EqualTo(0));
        }

        [Test]
        public void DisableCaptureAndClearModifiesCaptureFlag()
        {
            // given
            var target = CreateSendingContext().Build();
            Assert.That(target.IsCaptureOn, Is.True);

            // when
            target.DisableCaptureAndClear();

            // then
            Assert.That(target.IsCaptureOn, Is.False);
        }

        [Test]
        public void DisableCaptureAndClearClearsCapturedSessionData()
        {
            // given
            var sessionState = Substitute.For<ISessionState>();
            var session = Substitute.For<ISessionInternals>();
            session.State.Returns(sessionState);

            var target = CreateSendingContext().Build();
            target.AddSession(session);

            // when
            target.DisableCaptureAndClear();

            // then
            Assert.That(target.SessionCount, Is.EqualTo(1));
            session.Received(1).ClearCapturedData();
        }

        [Test]
        public void DisableCaptureAndClearRemovesFinishedSession()
        {
            // given
            var sessionState = Substitute.For<ISessionState>();
            sessionState.IsFinished.Returns(true);

            var session = Substitute.For<ISessionInternals>();
            session.State.Returns(sessionState);

            var target = CreateSendingContext().Build();
            target.AddSession(session);

            // when
            target.DisableCaptureAndClear();

            // then
            Assert.That(target.SessionCount, Is.EqualTo(0));
            session.Received(1).ClearCapturedData();
        }

        [Test]
        public void HandleStatusResponseDisablesCaptureIfResponseIsNull()
        {
            // given
            var target = CreateSendingContext().Build();
            Assert.That(target.IsCaptureOn, Is.True);

            // when
            target.HandleStatusResponse(null);

            // then
            Assert.That(target.IsCaptureOn, Is.False);
        }

        [Test]
        public void HandleStatusResponseClearsSessionDataIfResponseIsNull()
        {
            // given
            var state = Substitute.For<ISessionState>();
            var session = Substitute.For<ISessionInternals>();
            session.State.Returns(state);

            var target = CreateSendingContext().Build();
            target.AddSession(session);

            // when
            target.HandleStatusResponse(null);

            // then
            Assert.That(target.SessionCount, Is.EqualTo(1));
            session.Received(1).ClearCapturedData();
        }

        [Test]
        public void HandleStatusResponseRemovesFinishedSessionsIfResponseIsNull()
        {
            // given
            var state = Substitute.For<ISessionState>();
            state.IsFinished.Returns(true);
            var session = Substitute.For<ISessionInternals>();
            session.State.Returns(state);

            var target = CreateSendingContext().Build();
            target.AddSession(session);

            // when
            target.HandleStatusResponse(null);

            // then
            Assert.That(target.SessionCount, Is.EqualTo(0));
            session.Received(1).ClearCapturedData();
        }

        [Test]
        public void HandleStatusResponseDisablesCaptureIfResponseCodeIsNotOk()
        {
            // given
            var response = Substitute.For<IStatusResponse>();
            response.ResponseCode.Returns(404);

            var target = CreateSendingContext().Build();
            Assert.That(target.IsCaptureOn, Is.True);

            // when
            target.HandleStatusResponse(response);

            // then
            Assert.That(target.IsCaptureOn, Is.False);
        }

        [Test]
        public void HandleStatusResponseClearsSessionDataIfResponseCodeIsNotOk()
        {
            // given
            var response = Substitute.For<IStatusResponse>();
            response.ResponseCode.Returns(404);

            var sessionState = Substitute.For<ISessionState>();
            var session = Substitute.For<ISessionInternals>();
            session.State.Returns(sessionState);

            var target = CreateSendingContext().Build();
            target.AddSession(session);

            // when
            target.HandleStatusResponse(response);

            // then
            Assert.That(target.SessionCount, Is.EqualTo(1));
            session.Received(1).ClearReceivedCalls();
        }

        [Test]
        public void HandleStatusResponseRemovesFinishedSessionsIfResponseCodeIsNotOk()
        {
            // given
            var response = Substitute.For<IStatusResponse>();
            response.ResponseCode.Returns(404);

            var sessionState = Substitute.For<ISessionState>();
            sessionState.IsFinished.Returns(true);
            var session = Substitute.For<ISessionInternals>();
            session.State.Returns(sessionState);

            var target = CreateSendingContext().Build();
            target.AddSession(session);

            // when
            target.HandleStatusResponse(response);

            // then
            Assert.That(target.SessionCount, Is.EqualTo(0));
            session.Received(1).ClearCapturedData();
        }

        [Test]
        public void HandleStatusResponseClearsSessionDataIfResponseIsCaptureOff()
        {
            // given
            var responseAttributes = ResponseAttributes.WithUndefinedDefaults().WithCapture(false).Build();
            var response = StatusResponse.CreateSuccessResponse(
                mockLogger,
                responseAttributes,
                StatusResponse.HttpOk,
                new Dictionary<string, List<string>>()
            );

            var sessionState = Substitute.For<ISessionState>();
            var session = Substitute.For<ISessionInternals>();
            session.State.Returns(sessionState);

            var target = CreateSendingContext().Build();
            target.AddSession(session);

            // when
            target.HandleStatusResponse(response);

            // then
            Assert.That(target.SessionCount, Is.EqualTo(1));
            session.Received(1).ClearCapturedData();
        }

        [Test]
        public void HandleStatusResponseRemovesFinishedSessionsIfResponseIsCaptureOff()
        {
            // given
            var responseAttributes = ResponseAttributes.WithUndefinedDefaults().WithCapture(false).Build();
            var response = StatusResponse.CreateSuccessResponse(
                mockLogger,
                responseAttributes,
                StatusResponse.HttpOk,
                new Dictionary<string, List<string>>()
            );

            var sessionState = Substitute.For<ISessionState>();
            sessionState.IsFinished.Returns(true);
            var session = Substitute.For<ISessionInternals>();
            session.State.Returns(sessionState);

            var target = CreateSendingContext().Build();
            target.AddSession(session);

            // when
            target.HandleStatusResponse(response);

            // then
            Assert.That(target.SessionCount, Is.EqualTo(0));
            session.Received(1).ClearCapturedData();
        }

        [Test]
        public void HandleStatusResponseUpdatesSendInterval()
        {
            // given
            const int sendInterval = 999;
            var responseAttributes = ResponseAttributes.WithUndefinedDefaults()
                .WithSendIntervalInMilliseconds(sendInterval).Build();
            var response = StatusResponse.CreateSuccessResponse(
                mockLogger,
                responseAttributes,
                StatusResponse.HttpOk,
                new Dictionary<string, List<string>>()
            );

            var session = Substitute.For<ISessionInternals>();

            var target = CreateSendingContext().Build();
            target.AddSession(session);
            Assert.That(target.SendInterval, Is.Not.EqualTo(sendInterval));

            // then
            target.HandleStatusResponse(response);
            var obtained = target.SendInterval;

            // then
            Assert.That(session.ReceivedCalls(), Is.Empty);
            Assert.That(obtained, Is.EqualTo(sendInterval));
        }

        [Test]
        public void HandleStatusResponseUpdatesCaptureStateToFalse()
        {
            // given
            var responseAttributes = ResponseAttributes.WithUndefinedDefaults().WithCapture(false).Build();
            var response = StatusResponse.CreateSuccessResponse(
                mockLogger,
                responseAttributes,
                StatusResponse.HttpOk,
                new Dictionary<string, List<string>>()
            );

            var sessionState = Substitute.For<ISessionState>();
            var session = Substitute.For<ISessionInternals>();
            session.State.Returns(sessionState);

            var target = CreateSendingContext().Build();
            target.AddSession(session);
            Assert.That(target.IsCaptureOn, Is.True);

            // when
            target.HandleStatusResponse(response);

            // then
            session.Received(1).ClearCapturedData();
            Assert.That(target.IsCaptureOn, Is.False);
        }

        [Test]
        public void HandleStatusResponseUpdatesCaptureStateToTrue()
        {
            // given
            var responseAttributes = ResponseAttributes.WithUndefinedDefaults().WithCapture(true).Build();
            var response = StatusResponse.CreateSuccessResponse(
                mockLogger,
                responseAttributes,
                StatusResponse.HttpOk,
                new Dictionary<string, List<string>>()
            );

            var session = Substitute.For<ISessionInternals>();

            var target = CreateSendingContext().Build();
            target.DisableCaptureAndClear();
            target.AddSession(session);
            Assert.That(target.IsCaptureOn, Is.False);

            // when
            target.HandleStatusResponse(response);

            // then
            Assert.That(session.ReceivedCalls(), Is.Empty);
            Assert.That(target.IsCaptureOn, Is.True);
        }

        [Test]
        public void HandleStatusResponseUpdatesHttpClientConfig()
        {
            mockHttpClientConfig.BaseUrl.Returns("https://localhost:9999/1");
            mockHttpClientConfig.ApplicationId.Returns("some cryptic appId");
            mockHttpClientConfig.ServerId.Returns(42);
            mockHttpClientConfig.SslTrustManager.Returns(Substitute.For<ISSLTrustManager>());

            const int serverId = 73;
            var responseAttributes = ResponseAttributes.WithUndefinedDefaults().WithServerId(serverId).Build();
            var response = StatusResponse.CreateSuccessResponse(
                mockLogger,
                responseAttributes,
                StatusResponse.HttpOk,
                new Dictionary<string, List<string>>()
            );

            var target = Substitute.ForPartsOf<BeaconSendingContext>(
                mockLogger,
                mockHttpClientConfig,
                mockHttpClientProvider,
                mockTimingProvider,
                mockThreadSuspender
            );
            Assert.That(mockHttpClientConfig.ReceivedCalls(), Is.Empty);

            // when
            target.HandleStatusResponse(response);

            // then
            target.Received(1).CreateHttpClientConfigurationWith(serverId);
            _ = mockHttpClientConfig.Received(2).ServerId;
            _ = mockHttpClientConfig.Received(1).BaseUrl;
            _ = mockHttpClientConfig.Received(1).ApplicationId;
            _ = mockHttpClientConfig.Received(1).SslTrustManager;

            // and when
            IHttpClientConfiguration configCapture = null;
            mockHttpClientProvider.CreateClient(Arg.Do<IHttpClientConfiguration>(c => configCapture = c));

            target.GetHttpClient();

            // then
            Assert.That(configCapture, Is.Not.Null);
            mockHttpClientProvider.Received(1).CreateClient(configCapture);

            Assert.That(configCapture, Is.Not.SameAs(mockHttpClientConfig));
            Assert.That(configCapture.ServerId, Is.EqualTo(serverId));
            Assert.That(configCapture.BaseUrl, Is.EqualTo(mockHttpClientConfig.BaseUrl));
            Assert.That(configCapture.ApplicationId, Is.EqualTo(mockHttpClientConfig.ApplicationId));
            Assert.That(configCapture.SslTrustManager, Is.SameAs(mockHttpClientConfig.SslTrustManager));
        }

        [Test]
        public void HandleStatusResponseMergesLastStatusResponse()
        {
            // given
            const int beaconSize = 1234;
            var responseAttributes = ResponseAttributes.WithJsonDefaults().WithMaxBeaconSizeInBytes(beaconSize).Build();
            var response = StatusResponse.CreateSuccessResponse(
                mockLogger,
                responseAttributes,
                StatusResponse.HttpOk,
                new Dictionary<string, List<string>>()
            );

            var target = CreateSendingContext().Build();
            var initialAttributes = target.LastResponseAttributes;

            // when
            target.HandleStatusResponse(response);
            var obtained = target.LastResponseAttributes;

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(initialAttributes, Is.Not.EqualTo(obtained));
            Assert.That(obtained.MaxBeaconSizeInBytes, Is.EqualTo(beaconSize));
        }

        [Test]
        public void CreateHttpClientConfigUpdatesOnlyServerId()
        {
            // given
            const int serverId = 73;
            const string baseUrl = "https://localhost:9999/1";
            const string applicationId = "some cryptic appId";
            var trustManager = Substitute.For<ISSLTrustManager>();
            mockHttpClientConfig.ServerId.Returns(37);
            mockHttpClientConfig.BaseUrl.Returns(baseUrl);
            mockHttpClientConfig.ApplicationId.Returns(applicationId);
            mockHttpClientConfig.SslTrustManager.Returns(trustManager);

            var target = CreateSendingContext().Build();
            Assert.That(mockHttpClientConfig.ServerId, Is.Not.EqualTo(serverId));

            // when
            var obtained = ((BeaconSendingContext) target).CreateHttpClientConfigurationWith(serverId);

            // then
            Assert.That(obtained.ServerId, Is.EqualTo(serverId));
            Assert.That(obtained.BaseUrl, Is.EqualTo(baseUrl));
            Assert.That(obtained.ApplicationId, Is.EqualTo(applicationId));
            Assert.That(obtained.SslTrustManager, Is.SameAs(trustManager));
        }

        [Test]
        public void GetAllNotConfiguredSessionsReturnsOnlyNotConfiguredSessions()
        {
            // given
            var relevantSessionState = Substitute.For<ISessionState>();
            relevantSessionState.IsConfigured.Returns(false);
            var relevantSession = Substitute.For<ISessionInternals>();
            relevantSession.State.Returns(relevantSessionState);

            var ignoredSessionState = Substitute.For<ISessionState>();
            ignoredSessionState.IsConfigured.Returns(true);
            var ignoredSession = Substitute.For<ISessionInternals>();
            ignoredSession.State.Returns(ignoredSessionState);

            var target = CreateSendingContext().Build();
            target.AddSession(relevantSession);
            target.AddSession(ignoredSession);

            Assert.That(target.SessionCount, Is.EqualTo(2));

            // when
            var obtained = target.GetAllNotConfiguredSessions();

            // then
            Assert.That(obtained.Count, Is.EqualTo(1));
            Assert.That(obtained[0], Is.SameAs(relevantSession));
        }

        [Test]
        public void GetAllOpenAndConfiguredSessionsReturnsOnlyConfiguredNotFinishedSessions()
        {
            // given
            var relevantState = Substitute.For<ISessionState>();
            relevantState.IsConfiguredAndOpen.Returns(true);
            var relevantSession = Substitute.For<ISessionInternals>();
            relevantSession.State.Returns(relevantState);

            var ignoredState = Substitute.For<ISessionState>();
            ignoredState.IsConfiguredAndOpen.Returns(false);
            var ignoredSession = Substitute.For<ISessionInternals>();
            ignoredSession.State.Returns(ignoredState);

            var target = CreateSendingContext().Build();
            target.AddSession(relevantSession);
            target.AddSession(ignoredSession);

            Assert.That(target.SessionCount, Is.EqualTo(2));

            // when
            var obtained = target.GetAllOpenAndConfiguredSessions();

            // then
            Assert.That(obtained.Count, Is.EqualTo(1));
            Assert.That(obtained[0], Is.SameAs(relevantSession));
        }

        [Test]
        public void GetAllFinishedAndConfiguredSessionsReturnsOnlyConfiguredAndFinishedSessions()
        {
            // given
            var relevantState = Substitute.For<ISessionState>();
            relevantState.IsConfiguredAndFinished.Returns(true);
            var relevantSession = Substitute.For<ISessionInternals>();
            relevantSession.State.Returns(relevantState);

            var ignoredState = Substitute.For<ISessionState>();
            ignoredState.IsConfiguredAndFinished.Returns(false);
            var ignoredSession = Substitute.For<ISessionInternals>();
            ignoredSession.State.Returns(ignoredState);

            var target = CreateSendingContext().Build();
            target.AddSession(relevantSession);
            target.AddSession(ignoredSession);

            Assert.That(target.SessionCount, Is.EqualTo(2));

            // when
            var obtained = target.GetAllFinishedAndConfiguredSessions();

            // then
            Assert.That(obtained.Count, Is.EqualTo(1));
            Assert.That(obtained[0], Is.SameAs(relevantSession));
        }

        [Test]
        public void CurrentServerIdReturnsServerIdOfHttpClientConfig()
        {
            // given
            const int serverId = 37;
            mockHttpClientConfig.ServerId.Returns(serverId);

            var target = CreateSendingContext().Build();

            // when
            var obtained = target.CurrentServerId;

            // then
            Assert.That(obtained, Is.EqualTo(serverId));
            _ = mockHttpClientConfig.Received(1).ServerId;
        }


        [Test]
        public void UpdateFromDoesNothingIfStatusResponseIsNull()
        {
            // given
            var target = CreateSendingContext().Build();
            var initialAttributes = target.LastResponseAttributes;

            // when
            var obtained = target.UpdateFrom(null);

            // then
            Assert.That(obtained, Is.EqualTo(initialAttributes));
        }

        [Test]
        public void UpdateFromDoesNothingIfStatusResponseIsNotSuccessful()
        {
            // given
            var response = Substitute.For<IStatusResponse>();
            response.IsErroneousResponse.Returns(true);

            var target = CreateSendingContext().Build();
            var initialAttributes = target.LastResponseAttributes;

            // when
            var obtained = target.UpdateFrom(response);

            // then
            Assert.That(obtained, Is.EqualTo(initialAttributes));
            _ = response.Received(1).IsErroneousResponse;
        }

        [Test]
        public void UpdateFromMergesResponseAttributesFromStatusResponse()
        {
            // given
            const int serverId = 9999;
            var attributes = ResponseAttributes.WithUndefinedDefaults().WithServerId(serverId).Build();
            var response = Substitute.For<IStatusResponse>();
            response.ResponseAttributes.Returns(attributes);
            response.IsErroneousResponse.Returns(false);

            var target = CreateSendingContext().Build();
            var initialAttributes = target.LastResponseAttributes;

            // when
            var obtained = target.UpdateFrom(response);

            // then
            Assert.That(obtained, Is.EqualTo(target.LastResponseAttributes));
            Assert.That(obtained, Is.Not.EqualTo(initialAttributes));
            Assert.That(obtained, Is.Not.EqualTo(attributes));
            Assert.That(obtained.ServerId, Is.EqualTo(serverId));
        }

        [Test]
        public void UpdateFromDisablesCapturingIfReceivedApplicationIdMismatches()
        {
            // given
            mockHttpClientConfig.ApplicationId.Returns("some application id");
            var attributes = ResponseAttributes.WithUndefinedDefaults()
                .WithApplicationId("different application id")
                .Build();
            var response = Substitute.For<IStatusResponse>();
            response.ResponseAttributes.Returns(attributes);
            response.IsErroneousResponse.Returns(false);

            var target = CreateSendingContext().Build();
            var initialCaptureOn = target.IsCaptureOn;

            // when
            target.UpdateFrom(response);

            // then
            Assert.That(initialCaptureOn, Is.True);
            Assert.That(target.IsCaptureOn, Is.False);

        }

        [Test]
        public void ConfigurationTimestampReturnsZeroOnDefault()
        {
            // given
            var target = CreateSendingContext().Build();

            // when
            var obtained = target.ConfigurationTimestamp;

            // then
            Assert.That(obtained, Is.EqualTo(0L));
        }

        [Test]
        public void ConfigurationTimestampReturnsValueFromResponseAttributes()
        {
            // given
            const long timestamp = 1234;
            var attributes = ResponseAttributes.WithUndefinedDefaults().WithTimestampInMilliseconds(timestamp).Build();
            var response =
                StatusResponse.CreateSuccessResponse(mockLogger, attributes, 200,
                    new Dictionary<string, List<string>>());
            var target = CreateSendingContext().Build();

            // when
            target.UpdateFrom(response);

            // then
            Assert.That(target.ConfigurationTimestamp, Is.EqualTo(timestamp));
        }

        [Test]
        public void ApplicationIdMatchesIfApplicationIdWasNotReceived()
        {
            // given
            mockHttpClientConfig.ApplicationId.Returns("application id");
            var attributes = ResponseAttributes.WithUndefinedDefaults().Build();

            var target = CreateSendingContext().Build();

            // when
            var obtained = target.IsApplicationIdMismatch(attributes);

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void ApplicationIdMatchesIfStoredAndReceivedApplicationIdsAreEqual()
        {
            // given
            const string applicationId = "application id";
            mockHttpClientConfig.ApplicationId.Returns("application id");
            var attributes = ResponseAttributes.WithUndefinedDefaults()
                .WithApplicationId(applicationId)
                .Build();

            var target = CreateSendingContext().Build();

            // when
            var obtained = target.IsApplicationIdMismatch(attributes);

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void ApplicationIdMismatchesIfStoredAndReceivedApplicationIdsAreNotEqual()
        {
            // given
            const string applicationId = "application id";
            mockHttpClientConfig.ApplicationId.Returns("application ID");
            var attributes = ResponseAttributes.WithUndefinedDefaults()
                .WithApplicationId(applicationId)
                .Build();

            var target = CreateSendingContext().Build();

            // when
            var obtained = target.IsApplicationIdMismatch(attributes);

            // then
            Assert.That(obtained, Is.True);
        }

        private TestBeaconSendingContextBuilder CreateSendingContext()
        {
            return new TestBeaconSendingContextBuilder()
                    .With(mockLogger)
                    .With(mockHttpClientConfig)
                    .With(mockHttpClientProvider)
                    .With(mockTimingProvider)
                    .With(mockThreadSuspender)
                ;
        }
    }
}