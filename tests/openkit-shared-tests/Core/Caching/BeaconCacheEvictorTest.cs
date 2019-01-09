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
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Dynatrace.OpenKit.Core.Caching
{
    public class BeaconCacheEvictorTest
    {
        private ILogger mockLogger;
        private IBeaconCache mockBeaconCache;
        private IBeaconCacheEvictionStrategy mockStrategyOne;
        private IBeaconCacheEvictionStrategy mockStrategyTwo;

        private BeaconCacheEvictor evictor;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
            mockLogger.IsDebugEnabled.Returns(true);
            mockLogger.IsInfoEnabled.Returns(true);
            mockLogger.IsWarnEnabled.Returns(true);
            mockLogger.IsErrorEnabled.Returns(true);

            mockBeaconCache = Substitute.For<IBeaconCache>();
            mockStrategyOne = Substitute.For<IBeaconCacheEvictionStrategy>();
            mockStrategyTwo = Substitute.For<IBeaconCacheEvictionStrategy>();

            evictor = null;
        }

        [TearDown]
        public void TearDown()
        {
            if (evictor != null)
            {
                evictor.Stop();
            }
        }

        [Test]
        public void ADefaultConstructedBeaconCacheEvictorIsNotAlive()
        {
            // given
            evictor = new BeaconCacheEvictor(mockLogger, mockBeaconCache);

            // then
            Assert.That(evictor.IsAlive, Is.False);
        }

        [Test]
        public void AfterStartingABeaconCacheEvictorItIsAlive()
        {
            // given
            evictor = new BeaconCacheEvictor(mockLogger, mockBeaconCache);

            // when
            var obtained = evictor.Start();

            // then
            Assert.That(obtained, Is.True);
            Assert.That(evictor.IsAlive, Is.True);
        }

        [Test]
        public void StartingAnAlreadyAliveBeaconCacheEvictorDoesNothing()
        {
            // given
            evictor = new BeaconCacheEvictor(mockLogger, mockBeaconCache);
            evictor.Start();

            // when trying to start the evictor again
            var obtained = evictor.Start();

            // then
            Assert.That(obtained, Is.False);
            Assert.That(evictor.IsAlive, Is.True);
        }

        [Test]
        public void StoppingABeaconCacheEvictorWhichIsNotAliveDoesNothing()
        {
            // given
            evictor = new BeaconCacheEvictor(mockLogger, mockBeaconCache);

            // when
            var obtained = evictor.Stop();

            // then
            Assert.That(obtained, Is.False);
            Assert.That(evictor.IsAlive, Is.False);
        }

        [Test]
        public void StoppingAnAliveBeaconCacheEvictor()
        {
            // given
            evictor = new BeaconCacheEvictor(mockLogger, mockBeaconCache);
            evictor.Start();

            // when
            var obtained = evictor.Stop();

            // then
            Assert.That(obtained, Is.True);
            Assert.That(evictor.IsAlive, Is.False);
        }

        [Test]
        public void TriggeringEvictionStrategiesInThread()
        {
            var resetEvent = new AutoResetEvent(false); // non-signaled, all threads will block
            mockBeaconCache.When(c => c.RecordAdded += Arg.Any<EventHandler>()).Do(x => resetEvent.Set());

            // also do reset event when executing mock strategy
            mockStrategyTwo.When(s => s.Execute()).Do(x => resetEvent.Set());

            // first step start the eviction thread
            evictor = new BeaconCacheEvictor(mockLogger, mockBeaconCache, mockStrategyOne, mockStrategyTwo);
            evictor.Start();
            resetEvent.WaitOne(); // wait until the evictor thread subscribed

            for (int i = 0; i < 10; i++)
            {
                mockBeaconCache.RecordAdded += Raise.Event();
                resetEvent.WaitOne();
            }

            // stop the thread
            var stopped = evictor.Stop();

            // verify stop worked
            Assert.That(stopped, Is.True);
            Assert.That(evictor.IsAlive, Is.False);

            // ensure that the mocks were called accordingly
            mockStrategyOne.Received(10).Execute();
            mockStrategyTwo.Received(10).Execute();
        }
    }
}
