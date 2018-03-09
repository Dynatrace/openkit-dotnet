//
// Copyright 2018 Dynatrace LLC
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
using Dynatrace.OpenKit.Core.Communication;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;
using System;

namespace Dynatrace.OpenKit.Core
{
    public class SessionTest
    {
        private ITimingProvider mockTimingProvider;
        private IBeaconSendingContext beaconSendingContext;
        private Beacon beacon;
        private BeaconSender beaconSender;

        [SetUp]
        public void SetUp()
        {
            var logger = Substitute.For<ILogger>();
            logger.IsDebugEnabled.Returns(true);

            beaconSendingContext = Substitute.For<IBeaconSendingContext>();
            beaconSender = new BeaconSender(beaconSendingContext);

            mockTimingProvider = Substitute.For<ITimingProvider>();
            var configuration = new TestConfiguration();
            beacon = new Beacon(logger, new Caching.BeaconCache(), configuration, "127.0.0.1", Substitute.For<IThreadIDProvider>(), mockTimingProvider);
        }

        [Test]
        public void ANewlyCreatedSessionIsEmpty()
        {
            // given
            var target = new Session(beaconSender, beacon);

            // then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void ANewlyCreatedSessionIsNotEnded()
        {
            // given
            var target = new Session(beaconSender, beacon);

            // then
            Assert.That(target.EndTime, Is.EqualTo(-1L));
            Assert.That(target.IsSessionEnded, Is.False);
        }

        [Test]
        public void AfterCallingEndASessionIsEnded()
        {
            // given
            var target = new Session(beaconSender, beacon);

            // when
            target.End();

            // then
            Assert.That(target.IsSessionEnded, Is.True);
        }

        [Test]
        public void EnterActionGivesNewRootAction()
        {
            // given
            var target = new Session(beaconSender, beacon);

            // when
            var obtained = target.EnterAction("action name");

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.TypeOf<RootAction>());
        }

        [Test]
        public void EnterActionGivesNullActionIfSessionIsAlreadyEnded()
        {
            // given
            var target = new Session(beaconSender, beacon);
            target.End();

            // when
            var obtained = target.EnterAction("action name");

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.TypeOf<NullRootAction>());
        }

        [Test]
        public void IdentifyUser()
        {
            // given
            var target = new Session(beaconSender, beacon);
            beacon.ClearData();

            // when
            target.IdentifyUser("john.doe@acme.com");

            // then
            Assert.That(beacon.IsEmpty, Is.False);
        }

        [Test]
        public void IdentifyUserDoesNothingIfSessionIsEnded()
        {
            // given
            var target = new Session(beaconSender, beacon);
            target.End();
            beacon.ClearData();

            // when
            target.IdentifyUser("john.doe@acme.com");

            // then
            Assert.That(beacon.IsEmpty, Is.True);
        }

        [Test]
        public void ReportCrash()
        {
            // given
            var target = new Session(beaconSender, beacon);
            beacon.ClearData();

            // when
            target.ReportCrash("crash", "reason for crash", "the stacktrace");

            // then
            Assert.That(beacon.IsEmpty, Is.False);
        }

        [Test]
        public void ReportCrashDoesNothingIfSessionIsEnded()
        {
            // given
            var target = new Session(beaconSender, beacon);
            target.End();
            beacon.ClearData();

            // when
            target.ReportCrash("crash", "reason for crash", "the stacktrace");

            // then
            Assert.That(beacon.IsEmpty, Is.True);
        }

        [Test]
        public void EndingASessionEndsOpenRootActionsFirst()
        {
            // given
            int timestamp = 100;
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(x =>
            {
                timestamp++;
                return timestamp;
            });

            var target = new Session(beaconSender, beacon);
            var rootActionOne = target.EnterAction("Root Action One");
            var rootActionTwo = target.EnterAction("Root Action Two");

            // when
            target.End();

            // then
            Assert.That(((RootAction)rootActionOne).IsActionLeft, Is.True);
            Assert.That(((RootAction)rootActionTwo).IsActionLeft, Is.True);
            Assert.That(((RootAction)rootActionOne).EndTime, Is.LessThan(((RootAction)rootActionTwo).EndTime));
            Assert.That(((RootAction)rootActionTwo).EndTime, Is.LessThan(target.EndTime));
        }

        [Test]
        public void EndingASessionFinishesSessionContext()
        {
            // given
            int timestamp = 100;
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(x =>
            {
                timestamp++;
                return timestamp;
            });

            var target = new Session(beaconSender, beacon);

            // when
            target.End();

            // then
            beaconSendingContext.Received(1).FinishSession(target);
        }

        [Test]
        public void DisposingASessionEndsTheSession()
        {
            // given
            int timestamp = 100;
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(x =>
            {
                timestamp++;
                return timestamp;
            });

            IDisposable target = new Session(beaconSender, beacon);

            // when
            target.Dispose();

            // then
            beaconSendingContext.Received(1).FinishSession((Session)target);
        }

        [Test]
        public void EndingASessionReturnsImmediatelyIfEndedBefore()
        {
            // given
            int timestamp = 100;
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(x =>
            {
                timestamp++;
                return timestamp;
            });

            var target = new Session(beaconSender, beacon);
            target.End();

            beacon.ClearData();

            // when ending an already ended session
            target.End();


            // then
            Assert.That(beacon.IsEmpty, Is.True);
            beaconSendingContext.Received(1).FinishSession(target);
        }
    }
}
