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
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Communication
{
    public class BeaconSendingFlushSessionsStateTest
    {
        private IBeaconSendingContext mockContext;
        private ISessionInternals mockSession1Open;
        private ISessionInternals mockSession2Open;
        private ISessionInternals mockSession3Closed;

        [SetUp]
        public void Setup()
        {
            var mockResponse = Substitute.For<IStatusResponse>();
            mockResponse.ResponseCode.Returns(StatusResponse.HttpOk);
            mockResponse.IsErroneousResponse.Returns(false);

            mockSession1Open = Substitute.For<ISessionInternals>();
            mockSession1Open.IsDataSendingAllowed.Returns(true);
            mockSession1Open.SendBeacon(Arg.Any<IHttpClientProvider>(), Arg.Any<IAdditionalQueryParameters>())
                .Returns(mockResponse);

            mockSession2Open = Substitute.For<ISessionInternals>();
            mockSession2Open.IsDataSendingAllowed.Returns(true);
            mockSession2Open.SendBeacon(Arg.Any<IHttpClientProvider>(), Arg.Any<IAdditionalQueryParameters>())
                .Returns(mockResponse);

            mockSession3Closed = Substitute.For<ISessionInternals>();
            mockSession3Closed.IsDataSendingAllowed.Returns(true);
            mockSession3Closed.SendBeacon(Arg.Any<IHttpClientProvider>(), Arg.Any<IAdditionalQueryParameters>())
                .Returns(mockResponse);

            var mockHttpClient = Substitute.For<IHttpClient>();
            mockContext = Substitute.For<IBeaconSendingContext>();
            mockContext.GetHttpClient().Returns(mockHttpClient);
            mockContext.GetAllNotConfiguredSessions().Returns(new List<ISessionInternals>());
            mockContext.GetAllOpenAndConfiguredSessions().Returns(new List<ISessionInternals>
                {mockSession1Open, mockSession2Open});
            mockContext.GetAllFinishedAndConfiguredSessions().Returns(new List<ISessionInternals>
                {mockSession3Closed, mockSession2Open, mockSession1Open});
        }

        [Test]
        public void ToStringReturnStateName()
        {
            // given
            var target = new BeaconSendingFlushSessionsState();

            // then
            Assert.That(target.ToString(), Is.EqualTo("FlushSessions"));
        }

        [Test]
        public void ABeaconSendingFlushSessionStateIsNotATerminalState()
        {
            // when
            var target = new BeaconSendingFlushSessionsState();

            // then
            Assert.That(target.IsTerminalState, Is.False);
        }

        [Test]
        public void ABeaconSendingFlushSessionStateHasTerminalStateBeaconSendingTerminalState()
        {
            // when
            var target = new BeaconSendingFlushSessionsState();

            // then
            Assert.That(target.ShutdownState, Is.InstanceOf(typeof(BeaconSendingTerminalState)));
        }


        [Test]
        public void ABeaconSendingFlushSessionsStateTransitionsToTerminalStateWhenDataIsSent()
        {
            // given
            var target = new BeaconSendingFlushSessionsState();

            // when
            target.Execute(mockContext);

            // then verify transition to terminal state
            mockContext.Received(1).NextState = Arg.Any<BeaconSendingTerminalState>();
        }

        [Test]
        public void ABeaconSendingFlushSessionsStateClosesOpenSessions()
        {
            // given
            var target = new BeaconSendingFlushSessionsState();

            // when
            target.Execute(mockContext);

            // then
            mockSession1Open.Received(1).End();
            mockSession2Open.Received(1).End();
        }

        [Test]
        public void ABeaconSendingFlushSessionStateSendsAllOpenAndClosedBeacons()
        {
            // given
            var target = new BeaconSendingFlushSessionsState();

            // when
            target.Execute(mockContext);

            // then
            mockSession1Open.Received(1).SendBeacon(Arg.Any<IHttpClientProvider>(), mockContext);
            mockSession2Open.Received(1).SendBeacon(Arg.Any<IHttpClientProvider>(), mockContext);
            mockSession3Closed.Received(1).SendBeacon(Arg.Any<IHttpClientProvider>(), mockContext);
        }

        [Test]
        public void ABeaconSendingFlushSessionStateDoesNotSendIfSendingIsNotAllowed()
        {
            // given
            var target = new BeaconSendingFlushSessionsState();
            mockSession1Open.IsDataSendingAllowed.Returns(false);
            mockSession2Open.IsDataSendingAllowed.Returns(false);
            mockSession3Closed.IsDataSendingAllowed.Returns(false);

            // when
            target.Execute(mockContext);

            // then
            mockSession1Open.Received(0)
                .SendBeacon(Arg.Any<IHttpClientProvider>(), Arg.Any<IAdditionalQueryParameters>());
            mockSession1Open.Received(1).ClearCapturedData();

            mockSession2Open.Received(0)
                .SendBeacon(Arg.Any<IHttpClientProvider>(), Arg.Any<IAdditionalQueryParameters>());
            mockSession2Open.Received(1).ClearCapturedData();

            mockSession3Closed.Received(0)
                .SendBeacon(Arg.Any<IHttpClientProvider>(), Arg.Any<IAdditionalQueryParameters>());
            mockSession3Closed.Received(1).ClearCapturedData();
        }

        [Test]
        public void ABeaconSendingFlushSessionStateStopsSendingIfTooManyRequestsResponseWasReceived()
        {
            // given
            var target = new BeaconSendingFlushSessionsState();

            var response = Substitute.For<IStatusResponse>();
            response.ResponseCode.Returns(StatusResponse.HttpTooManyRequests);
            response.IsErroneousResponse.Returns(true);
            mockSession3Closed.SendBeacon(Arg.Any<IHttpClientProvider>(), Arg.Any<IAdditionalQueryParameters>())
                .Returns(response);

            // when
            target.Execute(mockContext);

            // then
            mockSession3Closed.Received(1).SendBeacon(Arg.Any<IHttpClientProvider>(), mockContext);
            mockSession3Closed.Received(1).ClearCapturedData();

            mockSession1Open.Received(0)
                .SendBeacon(Arg.Any<IHttpClientProvider>(), Arg.Any<IAdditionalQueryParameters>());
            mockSession1Open.Received(1).ClearCapturedData();

            mockSession2Open.Received(0)
                .SendBeacon(Arg.Any<IHttpClientProvider>(), Arg.Any<IAdditionalQueryParameters>());
            mockSession2Open.Received(1).ClearCapturedData();
        }

        [Test]
        public void ABeaconSendingFlushSessionStateEnablesCaptureForNotConfiguredSessions()
        {
            // given
            mockContext.GetAllNotConfiguredSessions().Returns(new List<ISessionInternals> {mockSession1Open, mockSession2Open});
            var target = new BeaconSendingFlushSessionsState();

            // when
            target.Execute(mockContext);

            // then
            mockSession1Open.Received(1).EnableCapture();
            mockSession2Open.Received(1).EnableCapture();
        }
    }
}
