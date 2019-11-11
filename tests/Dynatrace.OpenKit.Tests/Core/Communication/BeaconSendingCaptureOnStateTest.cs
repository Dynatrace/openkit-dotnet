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
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Communication
{
    public class BeaconSendingCaptureOnStateTest
    {
        private ILogger mockLogger;
        private IBeaconSendingContext mockContext;
        private ISessionInternals mockSession1Open;
        private ISessionInternals mockSession2Open;
        private ISessionInternals mockSession3Finished;
        private ISessionInternals mockSession4Finished;
        private ISessionInternals mockSession5New;
        private ISessionInternals mockSession6New;

        [SetUp]
        public void SetUp()
        {
            mockSession1Open = Substitute.For<ISessionInternals>();
            mockSession2Open = Substitute.For<ISessionInternals>();
            mockSession3Finished = Substitute.For<ISessionInternals>();
            mockSession4Finished = Substitute.For<ISessionInternals>();
            mockSession5New = Substitute.For<ISessionInternals>();
            mockSession6New = Substitute.For<ISessionInternals>();

            mockLogger = Substitute.For<ILogger>();

            var okResponse = Substitute.For<IStatusResponse>();
            okResponse.ResponseCode.Returns(StatusResponse.HttpOk);
            okResponse.IsErroneousResponse.Returns(false);

            var errorResponse = Substitute.For<IStatusResponse>();
            errorResponse.ResponseCode.Returns(404);
            errorResponse.IsErroneousResponse.Returns(true);

            mockSession1Open.IsDataSendingAllowed.Returns(true);
            mockSession1Open.SendBeacon(Arg.Any<IHttpClientProvider>()).Returns(okResponse);
            mockSession2Open.SendBeacon(Arg.Any<IHttpClientProvider>()).Returns(errorResponse);

            var mockHttpClientProvider = Substitute.For<IHttpClientProvider>();

            mockContext = Substitute.For<IBeaconSendingContext>();
            mockContext.HttpClientProvider.Returns(mockHttpClientProvider);
            mockContext.CurrentTimestamp.Returns(42);
            mockContext.GetAllNotConfiguredSessions().Returns(new List<ISessionInternals>());
            mockContext.GetAllOpenAndConfiguredSessions().Returns(new List<ISessionInternals>
                {mockSession1Open, mockSession2Open});
            mockContext.GetAllFinishedAndConfiguredSessions().Returns(new List<ISessionInternals>
                {mockSession3Finished, mockSession4Finished});
        }

        [Test]
        public void ABeaconSendingCaptureOnStateIsNotATerminalState()
        {
            // when
            var target = new BeaconSendingCaptureOnState();

            // then
            Assert.That(target.IsTerminalState, Is.False);
        }

        [Test]
        public void ABeaconSendingCaptureOnStateHasTerminalStateBeaconSendingFlushSessions()
        {
            // when
            var target = new BeaconSendingCaptureOnState();

            // then
            Assert.That(target.ShutdownState, Is.InstanceOf(typeof(BeaconSendingFlushSessionsState)));
        }

        [Test]
        public void ToStringReturnStateName()
        {
            // given
            var target = new BeaconSendingCaptureOnState();

            // then
            Assert.That(target.ToString(), Is.EqualTo("CaptureOn"));
        }

        [Test]
        public void NewSessionRequestsAreMadeForAllNotConfiguredSessions()
        {
            // given
            const int multiplicity = 5;
            var target = new BeaconSendingCaptureOnState();

            var mockClient = Substitute.For<IHttpClient>();
            mockContext.GetHttpClient().Returns(mockClient);
            mockContext.GetAllNotConfiguredSessions().Returns(new List<ISessionInternals> {mockSession5New, mockSession6New});
            mockClient.SendNewSessionRequest()
                .Returns(
                    new StatusResponse(mockLogger, $"mp={multiplicity}", 200, new Dictionary<string, List<string>>()),
                    new StatusResponse(mockLogger, string.Empty, StatusResponse.HttpBadRequest,
                        new Dictionary<string, List<string>>())
                    );
            mockSession5New.CanSendNewSessionRequest.Returns(true);
            mockSession6New.CanSendNewSessionRequest.Returns(true);

            IServerConfiguration serverConfigCapture = null;
            mockSession5New.UpdateServerConfiguration(Arg.Do<IServerConfiguration>(c => serverConfigCapture = c));

            // when
            target.Execute(mockContext);

            // verify for both new sessions a new session request has been made
            mockClient.Received(2).SendNewSessionRequest();

            // verify first new session has been updated
            Assert.That(serverConfigCapture, Is.Not.Null);
            mockSession5New.Received(1).UpdateServerConfiguration(serverConfigCapture);
            Assert.That(serverConfigCapture.Multiplicity, Is.EqualTo(multiplicity));

            // verify second new session decreased number of retries
            mockSession6New.Received(1).DecreaseNumRemainingSessionRequests();
        }

        [Test]
        public void CaptureIsDisabledIfNoFurtherNewSessionRequestsAreAllowed()
        {
            // given
            var target = new BeaconSendingCaptureOnState();

            var mockClient = Substitute.For<IHttpClient>();
            mockContext.GetHttpClient().Returns(mockClient);
            mockContext.GetAllNotConfiguredSessions().Returns(new List<ISessionInternals> {mockSession5New, mockSession6New});
            mockClient.SendNewSessionRequest()
                .Returns(
                    new StatusResponse(mockLogger, "mp=5", 200, new Dictionary<string, List<string>>()),
                    new StatusResponse(mockLogger, string.Empty, StatusResponse.HttpBadRequest,
                        new Dictionary<string, List<string>>())
                    );
            mockSession5New.CanSendNewSessionRequest.Returns(false);
            mockSession6New.CanSendNewSessionRequest.Returns(false);

            // when
            target.Execute(mockContext);

            // verify for no session a new session request has been made
            mockClient.Received(0).SendNewSessionRequest();

            // verify both sessions disabled capture
            mockSession5New.Received(1).DisableCapture();
            mockSession6New.Received(1).DisableCapture();
        }

         [Test]
        public void NewSessionRequestsAreAbortedWhenTooManyRequestsResponseIsReceived()
        {
            // given
            const int sleepTime = 6543;
            var target = new BeaconSendingCaptureOnState();

            var statusResponse = Substitute.For<IStatusResponse>();
            statusResponse.ResponseCode.Returns(StatusResponse.HttpTooManyRequests);
            statusResponse.IsErroneousResponse.Returns(true);
            statusResponse.GetRetryAfterInMilliseconds().Returns(sleepTime);

            var mockClient = Substitute.For<IHttpClient>();
            mockClient.SendNewSessionRequest().Returns(statusResponse);
            mockContext.GetHttpClient().Returns(mockClient);
            mockContext.GetAllNotConfiguredSessions().Returns(new List<ISessionInternals> {mockSession5New, mockSession6New});

            mockSession5New.CanSendNewSessionRequest.Returns(true);
            mockSession6New.CanSendNewSessionRequest.Returns(true);

            BeaconSendingCaptureOffState capturedState = null;
            mockContext.NextState = Arg.Do<BeaconSendingCaptureOffState>(x => capturedState = x);

            // when
            target.Execute(mockContext);

            // verify for first new sessions a new session request has been made
            mockClient.Received(1).SendNewSessionRequest();

            // verify no changes on first
            _ = mockSession5New.Received(1).CanSendNewSessionRequest;
            mockSession5New.Received(0).UpdateServerConfiguration(Arg.Any<IServerConfiguration>());
            mockSession5New.Received(0).DecreaseNumRemainingSessionRequests();

            // verify second new session is not used at all
            Assert.That(mockSession6New.ReceivedCalls(), Is.Empty);

            // ensure also transition to CaptureOffState
            Assert.That(capturedState, Is.Not.Null);
            mockContext.Received(1).NextState = capturedState;
            Assert.That(capturedState.SleepTimeInMilliseconds, Is.EqualTo(sleepTime));
        }

        [Test]
        public void ABeaconSendingCaptureOnStateSendsFinishedSessions()
        {
            // given
            var target = new BeaconSendingCaptureOnState();

            var statusResponse = Substitute.For<IStatusResponse>();
            statusResponse.ResponseCode.Returns(StatusResponse.HttpOk);
            statusResponse.IsErroneousResponse.Returns(false);

            mockSession3Finished.SendBeacon(Arg.Any<IHttpClientProvider>()).Returns(statusResponse);
            mockSession3Finished.IsDataSendingAllowed.Returns(true);
            mockSession4Finished.SendBeacon(Arg.Any<IHttpClientProvider>()).Returns(statusResponse);
            mockSession4Finished.IsDataSendingAllowed.Returns(true);

            // when
            target.Execute(mockContext);

            // then
            mockSession3Finished.Received(1).SendBeacon(Arg.Any<IHttpClientProvider>());
            mockSession4Finished.Received(1).SendBeacon(Arg.Any<IHttpClientProvider>());

            // also verify that the sessions are removed
            mockContext.Received(1).RemoveSession(mockSession3Finished);
            mockContext.Received(1).RemoveSession(mockSession4Finished);
        }

        [Test]
        public void ABeaconSendingCaptureOnStateClearsFinishedSessionsIfSendingIsNotAllowed()
        {
            // given
            var target = new BeaconSendingCaptureOnState();

            var statusResponse = Substitute.For<IStatusResponse>();
            statusResponse.ResponseCode.Returns(StatusResponse.HttpOk);
            statusResponse.IsErroneousResponse.Returns(false);

            mockSession3Finished.SendBeacon(Arg.Any<IHttpClientProvider>()).Returns(statusResponse);
            mockSession3Finished.IsDataSendingAllowed.Returns(false);
            mockSession4Finished.SendBeacon(Arg.Any<IHttpClientProvider>()).Returns(statusResponse);
            mockSession4Finished.IsDataSendingAllowed.Returns(false);

            // when
            target.Execute(mockContext);

            // then
            mockSession3Finished.Received(0).SendBeacon(Arg.Any<IHttpClientProvider>());
            mockSession4Finished.Received(0).SendBeacon(Arg.Any<IHttpClientProvider>());

            mockContext.Received(1).RemoveSession(mockSession3Finished);
            mockContext.Received(1).RemoveSession(mockSession4Finished);
        }

        [Test]
        public void ABeaconSendingCaptureOnStateDoesNotRemoveFinishedSessionIfSendingWasUnsuccessful()
        {
            //given
            var target = new BeaconSendingCaptureOnState();

            var statusResponse = Substitute.For<IStatusResponse>();
            statusResponse.ResponseCode.Returns(StatusResponse.HttpBadRequest);
            statusResponse.IsErroneousResponse.Returns(true);

            mockSession3Finished.SendBeacon(Arg.Any<IHttpClientProvider>()).Returns(statusResponse);
            mockSession3Finished.IsEmpty.Returns(false);
            mockSession3Finished.IsDataSendingAllowed.Returns(true);

            mockSession4Finished.SendBeacon(Arg.Any<IHttpClientProvider>()).Returns(statusResponse);
            mockSession4Finished.IsDataSendingAllowed.Returns(true);

            // when
            target.Execute(mockContext);

            mockSession3Finished.Received(1).SendBeacon(Arg.Any<IHttpClientProvider>());
            mockSession4Finished.Received(0).SendBeacon(Arg.Any<IHttpClientProvider>());

            _ = mockContext.Received(1).GetAllFinishedAndConfiguredSessions();
            mockContext.Received(0).RemoveSession(Arg.Any<ISessionInternals>());
        }

        [Test]
        public void ABeaconSendingCaptureOnStateContinuesWithNextFinishedSessionIfSendingWasUnsuccessfulButBeaconIsEmpty()
        {
            // given
            var target = new BeaconSendingCaptureOnState();

            var errorResponse = Substitute.For<IStatusResponse>();
            errorResponse.ResponseCode.Returns(StatusResponse.HttpBadRequest);
            errorResponse.IsErroneousResponse.Returns(true);

            var okResponse = Substitute.For<IStatusResponse>();
            okResponse.ResponseCode.Returns(StatusResponse.HttpOk);
            okResponse.IsErroneousResponse.Returns(false);

            mockSession3Finished.SendBeacon(Arg.Any<IHttpClientProvider>()).Returns(errorResponse);
            mockSession3Finished.IsEmpty.Returns(true);
            mockSession3Finished.IsDataSendingAllowed.Returns(true);

            mockSession4Finished.SendBeacon(Arg.Any<IHttpClientProvider>()).Returns(okResponse);
            mockSession4Finished.IsDataSendingAllowed.Returns(true);

            // when
            target.Execute(mockContext);

            // then
            mockSession3Finished.Received(1).SendBeacon(Arg.Any<IHttpClientProvider>());
            mockSession3Finished.Received(1).ClearCapturedData();

            mockSession4Finished.Received(1).SendBeacon(Arg.Any<IHttpClientProvider>());
            mockSession4Finished.Received(1).ClearReceivedCalls();


            _ = mockContext.Received(1).GetAllFinishedAndConfiguredSessions();
            mockContext.Received(1).RemoveSession(mockSession3Finished);
            mockContext.Received(1).RemoveSession(mockSession4Finished);
        }

        [Test]
        public void SendingFinishedSessionsIsAbortedImmediatelyWhenTooManyRequestsResponseIsReceived()
        {
            // given
            const int sleepTime = 4321;
            var target = new BeaconSendingCaptureOnState();

            var statusResponse = Substitute.For<IStatusResponse>();
            statusResponse.ResponseCode.Returns(StatusResponse.HttpTooManyRequests);
            statusResponse.IsErroneousResponse.Returns(true);
            statusResponse.GetRetryAfterInMilliseconds().Returns(sleepTime);

            mockSession3Finished.SendBeacon(Arg.Any<IHttpClientProvider>()).Returns(statusResponse);
            mockSession3Finished.IsDataSendingAllowed.Returns(true);
            mockSession4Finished.SendBeacon(Arg.Any<IHttpClientProvider>()).Returns(statusResponse);
            mockSession4Finished.IsDataSendingAllowed.Returns(true);

            BeaconSendingCaptureOffState captureState = null;
            mockContext.NextState = Arg.Do<BeaconSendingCaptureOffState>(c => captureState = c);

            // when
            target.Execute(mockContext);

            // then
            _ = mockSession3Finished.Received(1).IsDataSendingAllowed;
            mockSession3Finished.Received(1).SendBeacon(Arg.Any<IHttpClientProvider>());
            mockSession3Finished.Received(0).UpdateServerConfiguration(Arg.Any<IServerConfiguration>());
            mockSession3Finished.Received(0).DecreaseNumRemainingSessionRequests();

            // verify no interactions with second finished session
            Assert.That(mockSession4Finished.ReceivedCalls(), Is.Empty);

            // verify no interactions with open sessions
            Assert.That(mockSession1Open.ReceivedCalls(), Is.Empty);
            Assert.That(mockSession2Open.ReceivedCalls(), Is.Empty);

            _ = mockContext.Received(1).GetAllFinishedAndConfiguredSessions();
            mockContext.Received(0).RemoveSession(Arg.Any<ISessionInternals>());

            Assert.That(captureState, Is.Not.Null);
            mockContext.Received(1).NextState = captureState;
            Assert.That(captureState.SleepTimeInMilliseconds, Is.EqualTo(sleepTime));
        }

        [Test]
        public void ABeaconSendingCaptureOnStateSendsOpenSessionsIfNotExpired()
        {
            // given
            var target = new BeaconSendingCaptureOnState();
            mockSession1Open.IsDataSendingAllowed.Returns(true);
            mockSession2Open.IsDataSendingAllowed.Returns(true);

            // when
            target.Execute(mockContext);

            // then
            mockSession1Open.Received(1).SendBeacon(Arg.Any<IHttpClientProvider>());
            mockSession2Open.Received(1).SendBeacon(Arg.Any<IHttpClientProvider>());
            mockContext.Received(1).LastOpenSessionBeaconSendTime = Arg.Any<long>();
        }

        [Test]
        public void ABeaconSendingCaptureOnStateClearsOpenSessionDataIfSendingIsNotAllowed()
        {
            // given
            var target = new BeaconSendingCaptureOnState();
            mockSession1Open.IsDataSendingAllowed.Returns(false);
            mockSession2Open.IsDataSendingAllowed.Returns(false);

            // when
            target.Execute(mockContext);

            // then
            mockSession1Open.Received(0).SendBeacon(Arg.Any<IHttpClientProvider>());
            mockSession1Open.Received(1).ClearCapturedData();
            mockSession2Open.Received(0).SendBeacon(Arg.Any<IHttpClientProvider>());
            mockSession2Open.Received(1).ClearCapturedData();
            mockContext.Received(1).LastOpenSessionBeaconSendTime = Arg.Any<long>();
        }

        [Test]
        public void SendingOpenSessionsIsAbortedImmediatelyWhenTooManyRequestsResponseIsReceived()
        {
            //given
            const int sleepTime = 987654;
            var statusResponse = Substitute.For<IStatusResponse>();
            statusResponse.ResponseCode.Returns(StatusResponse.HttpTooManyRequests);
            statusResponse.IsErroneousResponse.Returns(true);
            statusResponse.GetRetryAfterInMilliseconds().Returns(sleepTime);

            mockSession1Open.SendBeacon(Arg.Any<IHttpClientProvider>()).Returns(statusResponse);
            mockSession1Open.IsDataSendingAllowed.Returns(true);
            mockSession2Open.SendBeacon(Arg.Any<IHttpClientProvider>()).Returns(statusResponse);
            mockSession2Open.IsDataSendingAllowed.Returns(true);

            BeaconSendingCaptureOffState capturedState = null;
            mockContext.NextState = Arg.Do<BeaconSendingCaptureOffState>(x => capturedState = x);

            var target = new BeaconSendingCaptureOnState();

            // when
            target.Execute(mockContext);

            // then
            mockSession1Open.Received(1).SendBeacon(Arg.Any<IHttpClientProvider>());
            _ = mockSession1Open.Received(1).IsDataSendingAllowed;

            Assert.That(mockSession2Open.ReceivedCalls(), Is.Empty);

            Assert.That(capturedState, Is.Not.Null);
            Assert.That(capturedState.SleepTimeInMilliseconds, Is.EqualTo(sleepTime));
        }

        [Test]
        public void ABeaconSendingCaptureOnStateTransitionsToCaptureOffStateWhenCapturingGotDisabled()
        {
            // given
            mockContext.IsCaptureOn.Returns(false);
            var target = new BeaconSendingCaptureOnState();

            // when
            target.Execute(mockContext);

            // then
            mockContext.Received(1).HandleStatusResponse(Arg.Any<IStatusResponse>());
            _ = mockContext.Received(1).IsCaptureOn;

            mockContext.Received(1).NextState = Arg.Any<BeaconSendingCaptureOffState>();
        }

        [Test]
        public void ABeaconSendingCaptureOnStateTransitionToFlushStateIsPerformedOnShutdown()
        {
            // given
            mockContext.IsShutdownRequested.Returns(true);
            var target = new BeaconSendingCaptureOnState();

            // when
            target.Execute(mockContext);

            // then
            mockContext.Received(1).NextState = Arg.Any<BeaconSendingFlushSessionsState>();
        }

        [Test]
        public void ABeaconSendingCaptureOnStateSendsOpenSessionsIfSendIntervalIsExceeded()
        {
            // given
            const int lastSendTime = 1;
            const int sendInterval = 1000;
            mockContext.LastOpenSessionBeaconSendTime.Returns(lastSendTime);
            mockContext.SendInterval.Returns(sendInterval);
            mockContext.CurrentTimestamp.Returns(lastSendTime + sendInterval + 1);

            var statusResponse = Substitute.For<IStatusResponse>();
            statusResponse.ResponseCode.Returns(StatusResponse.HttpOk);
            statusResponse.IsErroneousResponse.Returns(false);

            mockSession1Open.SendBeacon(Arg.Any<IHttpClientProvider>()).Returns(statusResponse);
            mockSession1Open.IsDataSendingAllowed.Returns(true);

            var target = new BeaconSendingCaptureOnState();

            // when
            target.Execute(mockContext);

            // then
            mockSession1Open.Received(1).SendBeacon(Arg.Any<IHttpClientProvider>());
            mockContext.Received(1).HandleStatusResponse(statusResponse);
            Assert.That(mockContext.LastOpenSessionBeaconSendTime, Is.EqualTo(mockContext.CurrentTimestamp)); // assert send time update
        }

        [Test]
        public void ABeaconSendingCaptureOnStateDoesNotSendOpenSessionsIfSendIntervalIsNotExceeded()
        {
            // given
            const int lastSendTime = 1;
            const int sendInterval = 1000;
            mockContext.LastOpenSessionBeaconSendTime.Returns(lastSendTime);
            mockContext.SendInterval.Returns(sendInterval);
            mockContext.CurrentTimestamp.Returns(lastSendTime + 1);

            var statusResponse = Substitute.For<IStatusResponse>();
            statusResponse.ResponseCode.Returns(StatusResponse.HttpOk);
            statusResponse.IsErroneousResponse.Returns(false);

            mockSession1Open.SendBeacon(Arg.Any<IHttpClientProvider>()).Returns(statusResponse);
            mockSession1Open.IsDataSendingAllowed.Returns(true);

            var target = new BeaconSendingCaptureOnState();

            // when
            target.Execute(mockContext);

            // then
            mockSession1Open.DidNotReceive().SendBeacon(Arg.Any<IHttpClientProvider>());
            mockContext.DidNotReceive().HandleStatusResponse(Arg.Any<StatusResponse>());
        }
    }
}
