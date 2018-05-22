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
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core.Caching
{
    public class TimeEvicitionStrategyTest
    {
        private ILogger mockLogger;
        private IBeaconCache mockBeaconCache;
        private ITimingProvider mockTimingProvider;
        private Func<bool> isShutdownFunc;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
            mockBeaconCache = Substitute.For<IBeaconCache>();
            mockTimingProvider = Substitute.For<ITimingProvider>();
            isShutdownFunc = () => false;
        }

        [Test]
        public void TheInitialLastRunTimestampIsMinusOne()
        {
            // given
            var configuration = new BeaconCacheConfiguration(-1L, 1000L, 2000L);
            var target = new TimeEvictionStrategy(mockLogger, mockBeaconCache, configuration, mockTimingProvider, isShutdownFunc);

            // then
            Assert.That(target.LastRunTimestamp, Is.EqualTo(-1L));
        }

        [Test]
        public void TheStrategyIsDisabledIfBeaconMaxAgeIsSetToLessThanZero()
        {
            // given
            var configuration = new BeaconCacheConfiguration(-1L, 1000L, 2000L);
            var target = new TimeEvictionStrategy(mockLogger, mockBeaconCache, configuration, mockTimingProvider, isShutdownFunc);

            // then
            Assert.That(target.IsStrategyDisabled, Is.True);
        }

        [Test]
        public void TheStrategyIsDisabledIfBeaconMaxAgeIsSetToZero()
        {
            // given
            var configuration = new BeaconCacheConfiguration(0L, 1000L, 2000L);
            var target = new TimeEvictionStrategy(mockLogger, mockBeaconCache, configuration, mockTimingProvider, isShutdownFunc);

            // then
            Assert.That(target.IsStrategyDisabled, Is.True);
        }

        [Test]
        public void TheStrategyIsNotDisabledIFMaxRecordAgeIsGreaterThanZero()
        {
            // given
            var configuration = new BeaconCacheConfiguration(1L, 1000L, 2000L);
            var target = new TimeEvictionStrategy(mockLogger, mockBeaconCache, configuration, mockTimingProvider, isShutdownFunc);

            // then
            Assert.That(target.IsStrategyDisabled, Is.False);
        }

        [Test]
        public void ShouldRunGivesFalseIfLastRunIsLessThanMaxAgeMillisecondsAgo()
        {
            // given
            var configuration = new BeaconCacheConfiguration(1000L, 1000L, 2000L);
            var target = new TimeEvictionStrategy(mockLogger, mockBeaconCache, configuration, mockTimingProvider, isShutdownFunc)
            {
                LastRunTimestamp = 1000
            };
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(target.LastRunTimestamp + configuration.MaxRecordAge - 1);

            // then
            Assert.That(target.ShouldRun, Is.False);
        }

        [Test]
        public void ShouldRunGivesTrueIfLastRunIsExactlyMaxAgeMillisecondsAgo()
        {
            // given
            var configuration = new BeaconCacheConfiguration(1000L, 1000L, 2000L);
            var target = new TimeEvictionStrategy(mockLogger, mockBeaconCache, configuration, mockTimingProvider, isShutdownFunc)
            {
                LastRunTimestamp = 1000
            };
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(target.LastRunTimestamp + configuration.MaxRecordAge);

            // then
            Assert.That(target.ShouldRun, Is.True);
        }

        [Test]
        public void ShouldRunGivesTrueIfLastRunIsMoreThanMaxAgeMillisecondsAgo()
        {
            // given
            var configuration = new BeaconCacheConfiguration(1000L, 1000L, 2000L);
            var target = new TimeEvictionStrategy(mockLogger, mockBeaconCache, configuration, mockTimingProvider, isShutdownFunc)
            {
                LastRunTimestamp = 1000
            };
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(target.LastRunTimestamp + configuration.MaxRecordAge + 1);

            // then
            Assert.That(target.ShouldRun, Is.True);
        }

        [Test]
        public void ExecuteEvictionLogsAMessageOnceAndReturnsIfStrategyIsDisabled()
        {
            // given
            var configuration = new BeaconCacheConfiguration(0L, 1000L, 2000L);
            var target = new TimeEvictionStrategy(mockLogger, mockBeaconCache, configuration, mockTimingProvider, isShutdownFunc);

            mockLogger.IsInfoEnabled.Returns(true);

            // when executing the first time
            target.Execute();

            // then
            var tmp = mockLogger.Received(1).IsInfoEnabled;
            mockLogger.ReceivedWithAnyArgs(1).Info(string.Empty);
            mockBeaconCache.DidNotReceiveWithAnyArgs().EvictRecordsByAge(0, 0L);

            // and when executing a second time
            mockLogger.ClearReceivedCalls();
            target.Execute();

            // then
            tmp = mockLogger.DidNotReceive().IsInfoEnabled;
            mockLogger.DidNotReceiveWithAnyArgs().Info(string.Empty);
            mockBeaconCache.DidNotReceiveWithAnyArgs().EvictRecordsByAge(0, 0L);
        }
        
        [Test]
        public void ExecuteEvictionDoesNotLogIfStrategyIsDisabledAndInfoIsDisabledInLogger()
        {
            // given
            var configuration = new BeaconCacheConfiguration(0L, 1000L, 2000L);
            var target = new TimeEvictionStrategy(mockLogger, mockBeaconCache, configuration, mockTimingProvider, isShutdownFunc);

            mockLogger.IsInfoEnabled.Returns(false);

            // when executing the first time
            target.Execute();

            // then
            var tmp = mockLogger.Received(1).IsInfoEnabled;
            mockLogger.DidNotReceiveWithAnyArgs().Info(string.Empty);
            mockBeaconCache.DidNotReceiveWithAnyArgs().EvictRecordsByAge(0, 0L);

            // and when executing a second time
            target.Execute();

            // then
            tmp = mockLogger.Received(2).IsInfoEnabled;
            mockLogger.DidNotReceiveWithAnyArgs().Info(string.Empty);
            mockBeaconCache.DidNotReceiveWithAnyArgs().EvictRecordsByAge(0, 0L);
        }

        [Test]
        public void LastRuntimeStampIsAdjustedDuringFirstExecution()
        {
            // given
            var configuration = new BeaconCacheConfiguration(1000L, 1000L, 2000L);
            var target = new TimeEvictionStrategy(mockLogger, mockBeaconCache, configuration, mockTimingProvider, isShutdownFunc);

            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(1000L, 1001L);

            // when executing the first time
            target.Execute();

            // then
            Assert.That(target.LastRunTimestamp, Is.EqualTo(1000L));
            mockTimingProvider.Received(2).ProvideTimestampInMilliseconds();

            // when executing the second time
            target.Execute();

            // then
            Assert.That(target.LastRunTimestamp, Is.EqualTo(1000L));
            mockTimingProvider.Received(3).ProvideTimestampInMilliseconds();
        }

        [Test]
        public void ExecuteEvictionStopsIfNoBeaconIdsAreAvailableInCache()
        {
            // given
            var configuration = new BeaconCacheConfiguration(1000L, 1000L, 2000L);
            var target = new TimeEvictionStrategy(mockLogger, mockBeaconCache, configuration, mockTimingProvider, isShutdownFunc);

            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(1000L, 2000L);
            mockBeaconCache.BeaconIDs.Returns(new HashSet<int>());

            // when
            target.Execute();

            // then verify interactions
            var tmp = mockBeaconCache.Received(1).BeaconIDs;
            mockTimingProvider.Received(3).ProvideTimestampInMilliseconds();
            mockBeaconCache.DidNotReceiveWithAnyArgs().EvictRecordsByAge(0, 0L);

            // also ensure that the last run timestamp was updated
            Assert.That(target.LastRunTimestamp, Is.EqualTo(2000L));
        }

        [Test]
        public void ExecuteEvictionCallsEvictionForEachBeaconSeparately()
        {

            // given
            var configuration = new BeaconCacheConfiguration(1000L, 1000L, 2000L);
            var target = new TimeEvictionStrategy(mockLogger, mockBeaconCache, configuration, mockTimingProvider, isShutdownFunc);

            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(1000L, 2099L);
            mockBeaconCache.BeaconIDs.Returns(new HashSet<int> { 1, 42 });
           
            // when
            target.Execute();

            // then verify interactions
            var tmp = mockBeaconCache.Received(1).BeaconIDs;
            mockBeaconCache.Received(1).EvictRecordsByAge(1, 2099L - configuration.MaxRecordAge);
            mockBeaconCache.Received(1).EvictRecordsByAge(42, 2099L - configuration.MaxRecordAge);
            mockTimingProvider.Received(3).ProvideTimestampInMilliseconds();

            // also ensure that the last run timestamp was updated
            Assert.That(target.LastRunTimestamp, Is.EqualTo(2099L));
        }

        [Test]
        public void ExecuteEvictionLogsTheNumberOfRecordsRemoved()
        {
            // given
            var configuration = new BeaconCacheConfiguration(1000L, 1000L, 2000L);
            var target = new TimeEvictionStrategy(mockLogger, mockBeaconCache, configuration, mockTimingProvider, isShutdownFunc);

            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(1000L, 2099L);

            mockBeaconCache.BeaconIDs.Returns(new HashSet<int> { 1, 42 });
            mockBeaconCache.EvictRecordsByAge(1, Arg.Any<long>()).Returns(2);
            mockBeaconCache.EvictRecordsByAge(42, Arg.Any<long>()).Returns(5);

            mockLogger.IsDebugEnabled.Returns(true);

            // when
            target.Execute();

            // then verify that the logger was invoked
            var tmp = mockLogger.Received(2).IsDebugEnabled;
            mockLogger.Received(1).Debug(target.GetType().Name + " - Removed 2 records from Beacon with ID 1");
            mockLogger.Received(1).Debug(target.GetType().Name + " - Removed 5 records from Beacon with ID 42");
        }

        [Test]
        public void ExecuteEvictionIsStoppedIfThreadGetsInterrupted()
        {
            // given
            var shutdown = false;
            var configuration = new BeaconCacheConfiguration(1000L, 1000L, 2000L);
            var target = new TimeEvictionStrategy(mockLogger, mockBeaconCache, configuration, mockTimingProvider, () => shutdown);

            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(1000L, 2099L);

            mockBeaconCache.BeaconIDs.Returns(new HashSet<int> { 1, 42 });
            mockBeaconCache.EvictRecordsByAge(Arg.Any<int>(), Arg.Any<long>())
                .Returns(x =>
                {
                    shutdown = true;
                    return 2;
                });

            // when
            target.Execute();

            // then verify interactions
            var tmp = mockBeaconCache.Received(1).BeaconIDs;
            mockBeaconCache.Received(1).EvictRecordsByAge(Arg.Any<int>(), 2099L - configuration.MaxRecordAge);
        }
    }
}
