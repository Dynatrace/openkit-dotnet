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
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;
using System;

namespace Dynatrace.OpenKit.Core
{
    public class WebRequestTracerBaseTest
    {
        private Beacon beacon;
        private ILogger logger;
        private OpenKitConfiguration testConfiguration;
        private ITimingProvider mockTimingProvider;

        [SetUp]
        public void SetUp()
        {
            logger = Substitute.For<ILogger>();
            mockTimingProvider = Substitute.For<ITimingProvider>();
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OPT_IN_CRASHES);
            testConfiguration = new TestConfiguration("1", beaconConfig);
            beacon = new Beacon(logger,
                                new BeaconCache(logger),
                                testConfiguration,
                                "127.0.0.1",
                                Substitute.For<IThreadIDProvider>(),
                                mockTimingProvider);
        }

        [Test]
        public void DefaultValues()
        {
            // given
            var target = new TestWebRequestTracerBase(logger, beacon, 17);

            // then
            Assert.That(target.URL, Is.EqualTo("<unknown>"));
            Assert.That(target.ResponseCode, Is.EqualTo(-1));
            Assert.That(target.StartTime, Is.EqualTo(0L));
            Assert.That(target.EndTime, Is.EqualTo(-1L));
            Assert.That(target.StartSequenceNo, Is.EqualTo(1));
            Assert.That(target.EndSequenceNo, Is.EqualTo(-1));
            Assert.That(target.BytesSent, Is.EqualTo(-1));
            Assert.That(target.BytesReceived, Is.EqualTo(-1));
        }

        [Test]
        public void GetTag()
        {
            // given
            var target = new TestWebRequestTracerBase(logger, beacon, 17);

            // then
            var expectedTag = beacon.CreateTag(17, 1);
            Assert.That(target.Tag, Is.EqualTo(expectedTag));
        }

        [Test]
        public void ANewlyCreatedWebRequestTracerIsNotStopped()
        {
            // given
            var target = new TestWebRequestTracerBase(logger, beacon, 17);

            // then
            Assert.That(target.IsStopped, Is.False);
        }

        [Test]
        public void AWebRequestTracerIsStoppedAfterStopHasBeenCalled()
        {
            // given
            var target = new TestWebRequestTracerBase(logger, beacon, 17);

            // when calling the stop method
            target.Stop();

            // then
            Assert.That(target.IsStopped, Is.True);
        }
        
        [Test]
        public void DisposingAWebRequestTracerStopsIt()
        {
            // given
            IDisposable target = new TestWebRequestTracerBase(logger, beacon, 17);

            // when disposing the target
            target.Dispose();

            // then
            Assert.That(((WebRequestTracerBase)target).IsStopped, Is.True);
        }

        [Test]
        public void SetResponseCodeSetsTheResponseCode()
        {
            // given
            var target = new TestWebRequestTracerBase(logger, beacon, 17);

            // when setting response code
            var obtained = target.SetResponseCode(418);

            // then
            Assert.That(target.ResponseCode, Is.EqualTo(418));
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void SetResponseCodeDoesNotSetTheResponseCodeIfStopped()
        {
            // given
            var target = new TestWebRequestTracerBase(logger, beacon, 17);
            target.Stop();

            // when setting response code
            var obtained = target.SetResponseCode(418);

            // then
            Assert.That(target.ResponseCode, Is.EqualTo(-1));
            Assert.That(obtained, Is.SameAs(target));
        }


        [Test]
        public void SetBytesSentSetsTheNumberOfSentBytes()
        {
            // given
            var target = new TestWebRequestTracerBase(logger, beacon, 17);

            // when setting the sent bytes
            var obtained = target.SetBytesSent(1234);

            // then
            Assert.That(target.BytesSent, Is.EqualTo(1234));
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void SetBytesSentDoesNotSetAnythingIfStopped()
        {
            // given
            var target = new TestWebRequestTracerBase(logger, beacon, 17);
            target.Stop();

            // when setting the sent bytes
            var obtained = target.SetBytesSent(1234);

            // then
            Assert.That(target.BytesSent, Is.EqualTo(-1));
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void SetBytesReceivedSetsTheNumberOfReceivedBytes()
        {
            // given
            var target = new TestWebRequestTracerBase(logger, beacon, 17);

            // when setting the received bytes
            var obtained = target.SetBytesReceived(4321);

            // then
            Assert.That(target.BytesReceived, Is.EqualTo(4321));
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void SetBytesReceivedDoesNotSetAnythingIfStopped()
        {
            // given
            var target = new TestWebRequestTracerBase(logger, beacon, 17);
            target.Stop();

            // when setting the received bytes
            var obtained = target.SetBytesReceived(4321);

            // then
            Assert.That(target.BytesReceived, Is.EqualTo(-1));
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void StartSetsTheStartTime()
        {
            // given
            var target = new TestWebRequestTracerBase(logger, beacon, 17);
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(123456789L);

            // when starting web request tracing
            var obtained = target.Start();

            // then
            Assert.That(target.StartTime, Is.EqualTo(123456789L));
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void StartDoesNothingIfAlreadyStopped()
        {
            // given
            var target = new TestWebRequestTracerBase(logger, beacon, 17);
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(123456789L);
            target.Stop();

            // when starting web request tracing
            var obtained = target.Start();

            // then
            Assert.That(target.StartTime, Is.EqualTo(0L));
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void StopCanOnlyBeExecutedOnce()
        {
            // given
            var target = new TestWebRequestTracerBase(logger, beacon, 17);
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(123L, 321L);

            // when executed the first time
            beacon.ClearData();
            target.Stop();

            // then
            Assert.That(target.EndSequenceNo, Is.EqualTo(2));
            Assert.That(target.EndTime, Is.EqualTo(123L));
            Assert.That(beacon.IsEmpty, Is.False);

            // and when executed the second time
            beacon.ClearData();
            target.Stop();

            // then
            Assert.That(target.EndSequenceNo, Is.EqualTo(2));
            Assert.That(target.EndTime, Is.EqualTo(123L));
            Assert.That(beacon.IsEmpty, Is.True);
        }

        private sealed class TestWebRequestTracerBase : WebRequestTracerBase
        {
            public TestWebRequestTracerBase(ILogger logger, Beacon beacon, int parentActionID) : base(logger, beacon, parentActionID)
            {
            }
        }
    }
}
