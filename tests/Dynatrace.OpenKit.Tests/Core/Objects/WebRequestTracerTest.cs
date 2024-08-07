﻿//
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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Protocol;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Objects
{
    public class WebRequestTracerTest
    {
        private IBeacon mockBeacon;
        private ILogger mockLogger;
        private IOpenKitComposite mockParent;

        private const int SequenceNumber = 1234;
        private const string Tag = "THE_TAG";

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
            mockParent = Substitute.For<IOpenKitComposite>();

            mockBeacon = Substitute.For<IBeacon>();
            mockBeacon.CreateTag(Arg.Any<int>(), Arg.Any<int>()).Returns(Tag);
            mockBeacon.NextSequenceNumber.Returns(SequenceNumber);
        }

        [Test]
        public void DefaultValues()
        {
            // given
            var target = CreateWebRequestTracer().Build();

            // then
            Assert.That(target.Url, Is.EqualTo("<unknown>"));
            Assert.That(target.ResponseCode, Is.EqualTo(-1));
            Assert.That(target.StartTime, Is.EqualTo(0L));
            Assert.That(target.EndTime, Is.EqualTo(-1L));
            Assert.That(target.StartSequenceNo, Is.EqualTo(SequenceNumber));
            Assert.That(target.EndSequenceNo, Is.EqualTo(-1));
            Assert.That(target.BytesSent, Is.EqualTo(-1));
            Assert.That(target.BytesReceived, Is.EqualTo(-1));
        }

        [Test]
        public void ANewlyCreatedWebRequestTracerDoesNotAttachToTheParent()
        {
            // given, when
            CreateWebRequestTracer().Build();

            // then
            _ = mockParent.Received(1).ActionId;
        }

        [Test]
        public void TagGet()
        {
            // given
            var target = CreateWebRequestTracer().Build();

            // then
            Assert.That(target.Tag, Is.EqualTo(Tag));
        }

        [Test]
        public void ANewlyCreatedWebRequestTracerIsNotStopped()
        {
            // given
            var target = CreateWebRequestTracer().Build();

            // then
            Assert.That(target.IsStopped, Is.False);
        }

        [Test]
        public void AWebRequestTracerIsStoppedWithResponseCodeAfterStopHasBeenCalled()
        {
            // given
            var target = CreateWebRequestTracer().Build();
            // when calling the stop method with response code
            target.Stop(200);

            // then
            Assert.That(target.IsStopped, Is.True);
        }

        [Test]
        public void StopWithResponseCodeSetsTheResponseCode()
        {
            // given
            var target = CreateWebRequestTracer().Build();

            // when stopping with response code
            target.Stop(418);

            // then
            Assert.That(target.ResponseCode, Is.EqualTo(418));
        }

        [Test]
        public void StopWithResponseCodeDoesNotSetTheResponseCodeIfStopped()
        {
            // given
            var target = CreateWebRequestTracer().Build();
            target.Stop(200);

            // when stopping with response code
            target.Stop(404);

            // then
            Assert.That(target.ResponseCode, Is.EqualTo(200));
        }

        [Test]
        public void SetBytesSentSetsTheNumberOfSentBytes()
        {
            // given
            var target = CreateWebRequestTracer().Build();

            // when setting the sent bytes
            var obtained = target.SetBytesSent(1234);

            // then
            Assert.That(target.BytesSent, Is.EqualTo(1234L));
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void SetBytesSentDoesNotSetAnythingIfStoppedWithResponseCode()
        {
            // given
            var target = CreateWebRequestTracer().Build();
            target.Stop(200);

            // when setting the sent bytes
            var obtained = target.SetBytesSent(1234);

            // then
            Assert.That(target.BytesSent, Is.EqualTo(-1L));
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void SetBytesSentLongSetsTheNumberOfSentBytes()
        {
            // given
            var target = CreateWebRequestTracer().Build();

            // when setting the sent bytes
            var obtained = target.SetBytesSent(1234L);

            // then
            Assert.That(target.BytesSent, Is.EqualTo(1234L));
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void SetBytesSentLongDoesNotSetAnythingIfStoppedWithResponseCode()
        {
            // given
            var target = CreateWebRequestTracer().Build();
            target.Stop(200);

            // when setting the sent bytes
            var obtained = target.SetBytesSent(1234L);

            // then
            Assert.That(target.BytesSent, Is.EqualTo(-1L));
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void SetBytesReceivedSetsTheNumberOfReceivedBytes()
        {
            // given
            var target = CreateWebRequestTracer().Build();

            // when setting the received bytes
            var obtained = target.SetBytesReceived(4321);

            // then
            Assert.That(target.BytesReceived, Is.EqualTo(4321L));
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void SetBytesReceivedDoesNotSetAnythingIfStoppedWithResponseCode()
        {
            // given
            var target = CreateWebRequestTracer().Build();
            target.Stop(200);

            // when setting the received bytes
            var obtained = target.SetBytesReceived(4321);

            // then
            Assert.That(target.BytesReceived, Is.EqualTo(-1L));
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void SetBytesReceivedLongSetsTheNumberOfReceivedBytes()
        {
            // given
            var target = CreateWebRequestTracer().Build();

            // when setting the received bytes
            var obtained = target.SetBytesReceived(4321L);

            // then
            Assert.That(target.BytesReceived, Is.EqualTo(4321L));
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void SetBytesReceivedLongDoesNotSetAnythingIfStoppedWithResponseCode()
        {
            // given
            var target = CreateWebRequestTracer().Build();
            target.Stop(200);

            // when setting the received bytes
            var obtained = target.SetBytesReceived(4321L);

            // then
            Assert.That(target.BytesReceived, Is.EqualTo(-1L));
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void StartSetsTheStartTime()
        {
            // given
            const long timestamp = 12345;
            mockBeacon.CurrentTimestamp.Returns(timestamp);
            var target = CreateWebRequestTracer().Build();

            // when starting web request tracing
            var obtained = target.Start();

            // then
            Assert.That(target.StartTime, Is.EqualTo(timestamp));
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void StartDoesNothingIfAlreadyStoppedWithResponseCode()
        {
            // given
            var target = CreateWebRequestTracer().Build();
            const long timestamp = 12345;
            mockBeacon.CurrentTimestamp.Returns(timestamp);
            target.Stop(200);

            mockBeacon.ClearReceivedCalls();

            // when starting web request tracing
            var obtained = target.Start();

            // then
            Assert.That(target.StartTime, Is.EqualTo(0L));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void StopWithResponseCodeCanOnlyBeExecutedOnce()
        {
            // given
            const int responseCode = 200;
            const int sequenceNumber = 42;
            var target = CreateWebRequestTracer().Build();
            mockBeacon.NextSequenceNumber.Returns(sequenceNumber);
            mockBeacon.ClearReceivedCalls();

            // when executed the first time
            target.Stop(responseCode);

            // then
            Assert.That(target.EndSequenceNo, Is.EqualTo(sequenceNumber));
            _ = mockBeacon.Received(1).NextSequenceNumber;
            mockBeacon.Received(1).AddWebRequest(0, target);

            mockBeacon.ClearReceivedCalls();

            // and when executed the second time
            target.Stop(responseCode);

            // then
            Assert.That(target.EndSequenceNo, Is.EqualTo(sequenceNumber));
            Assert.That(mockBeacon.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void StopWithResponseCodeNotifiesParentAndDetachesFromParent()
        {
            var target = CreateWebRequestTracer().Build();
            mockBeacon.NextSequenceNumber.Returns(42);

            // when
            target.Stop(200);

            // then
            mockParent.Received(1).OnChildClosed(target);
            _ = mockParent.Received(1).ActionId;
        }

        [Test]
        public void DisposingAWebRequestTracerStopsIt()
        {
            // given
            var target = CreateWebRequestTracer().Build();

            // when disposing the target
            target.Dispose();

            // then
            Assert.That(target.IsStopped, Is.True);
        }

        [Test]
        public void CancellingAWebRequestLogsInvocation()
        {
            // given
            mockLogger.IsDebugEnabled.Returns(true);
            var target = CreateWebRequestTracer().Build();

            // when
            target.Cancel();

            // then
            var _ = mockLogger.Received(1).IsDebugEnabled;
            mockLogger.Received(1).Debug($"{target.ToString()} Cancel()");
        }

        [Test]
        public void CancellingAWebRequestStopsItWithoutReportingIt()
        {
            // given
            var target = CreateWebRequestTracer().Build();
            mockBeacon.NextSequenceNumber.Returns(42);

            // when
            target.Cancel();

            // then
            Assert.That(target.EndSequenceNo, Is.EqualTo(42));
            var _ = mockBeacon.Received(2).NextSequenceNumber;
            mockBeacon.DidNotReceive().AddWebRequest(Arg.Any<int>(), Arg.Any<IWebRequestTracerInternals>());
        }

        private TestWebRequestTracerBuilder CreateWebRequestTracer()
        {
            return new TestWebRequestTracerBuilder()
                    .With(mockLogger)
                    .With(mockBeacon)
                    .With(mockParent)
                ;
        }
    }
}
