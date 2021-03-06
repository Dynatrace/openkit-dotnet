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

using System;
using System.Collections.Generic;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Caching
{
    public class TimeEvictionStrategyTest
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
            var configuration = MockBeaconCacheConfig(-1L, 1000L, 2000L);
            var target = CreateTimeEvictionStrategyWith(configuration);

            // then
            Assert.That(target.LastRunTimestamp, Is.EqualTo(-1L));
        }

        [Test]
        public void TheStrategyIsDisabledIfBeaconMaxAgeIsSetToLessThanZero()
        {
            // given
            var configuration = MockBeaconCacheConfig(-1L, 1000L, 2000L);
            var target = CreateTimeEvictionStrategyWith(configuration);

            // then
            Assert.That(target.IsStrategyDisabled, Is.True);
            Assert.That(mockLogger.ReceivedCalls(), Is.Empty);
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
            Assert.That(mockTimingProvider.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void TheStrategyIsDisabledIfBeaconMaxAgeIsSetToZero()
        {
            // given
            var configuration = MockBeaconCacheConfig(0L, 1000L, 2000L);
            var target = CreateTimeEvictionStrategyWith(configuration);

            // then
            Assert.That(target.IsStrategyDisabled, Is.True);
            Assert.That(mockLogger.ReceivedCalls(), Is.Empty);
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
            Assert.That(mockTimingProvider.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void TheStrategyIsNotDisabledIfMaxRecordAgeIsGreaterThanZero()
        {
            // given
            var configuration = MockBeaconCacheConfig(1L, 1000L, 2000L);
            var target = CreateTimeEvictionStrategyWith(configuration);

            // then
            Assert.That(target.IsStrategyDisabled, Is.False);
            Assert.That(mockLogger.ReceivedCalls(), Is.Empty);
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
            Assert.That(mockTimingProvider.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ShouldRunGivesFalseIfLastRunIsLessThanMaxAgeMillisecondsAgo()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 2000L);
            var target = CreateTimeEvictionStrategyWith(configuration);
            target.LastRunTimestamp = 1000;

            mockTimingProvider.ProvideTimestampInMilliseconds()
                .Returns(target.LastRunTimestamp + configuration.MaxRecordAge - 1);

            // then
            Assert.That(target.ShouldRun, Is.False);
        }

        [Test]
        public void ShouldRunGivesTrueIfLastRunIsExactlyMaxAgeMillisecondsAgo()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 2000L);
            var target = CreateTimeEvictionStrategyWith(configuration);
            target.LastRunTimestamp = 1000;

            mockTimingProvider.ProvideTimestampInMilliseconds()
                .Returns(target.LastRunTimestamp + configuration.MaxRecordAge);

            // then
            Assert.That(target.ShouldRun, Is.True);
        }

        [Test]
        public void ShouldRunGivesTrueIfLastRunIsMoreThanMaxAgeMillisecondsAgo()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 2000L);
            var target = CreateTimeEvictionStrategyWith(configuration);
            target.LastRunTimestamp = 1000;

            mockTimingProvider.ProvideTimestampInMilliseconds()
                .Returns(target.LastRunTimestamp + configuration.MaxRecordAge + 1);

            // then
            Assert.That(target.ShouldRun, Is.True);
        }

        [Test]
        public void ExecuteEvictionLogsAMessageOnceAndReturnsIfStrategyIsDisabled()
        {
            // given
            var configuration = MockBeaconCacheConfig(0L, 1000L, 2000L);
            var target = CreateTimeEvictionStrategyWith(configuration);
            var key = new BeaconKey(0, 0);

            mockLogger.IsInfoEnabled.Returns(true);

            // when executing the first time
            target.Execute();

            // then
            _ = mockLogger.Received(1).IsInfoEnabled;
            mockLogger.ReceivedWithAnyArgs(1).Info(string.Empty);
            mockBeaconCache.DidNotReceiveWithAnyArgs().EvictRecordsByAge(key, 0L);

            // and when executing a second time
            mockLogger.ClearReceivedCalls();
            target.Execute();

            // then
            _ = mockLogger.DidNotReceive().IsInfoEnabled;
            mockLogger.DidNotReceiveWithAnyArgs().Info(string.Empty);
            mockBeaconCache.DidNotReceiveWithAnyArgs().EvictRecordsByAge(key, 0L);
        }

        [Test]
        public void ExecuteEvictionDoesNotLogIfStrategyIsDisabledAndInfoIsDisabledInLogger()
        {
            // given
            var configuration = MockBeaconCacheConfig(0L, 1000L, 2000L);
            var target = CreateTimeEvictionStrategyWith(configuration);
            var key = new BeaconKey(0, 0);

            mockLogger.IsInfoEnabled.Returns(false);

            // when executing the first time
            target.Execute();

            // then
            _ = mockLogger.Received(1).IsInfoEnabled;
            mockLogger.DidNotReceiveWithAnyArgs().Info(string.Empty);
            mockBeaconCache.DidNotReceiveWithAnyArgs().EvictRecordsByAge(key, 0L);

            // and when executing a second time
            target.Execute();

            // then
            _ = mockLogger.Received(2).IsInfoEnabled;
            mockLogger.DidNotReceiveWithAnyArgs().Info(string.Empty);
            mockBeaconCache.DidNotReceiveWithAnyArgs().EvictRecordsByAge(key, 0L);
        }

        [Test]
        public void LastRuntimeStampIsAdjustedDuringFirstExecution()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 2000L);
            var target = CreateTimeEvictionStrategyWith(configuration);

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
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 2000L);
            var target = CreateTimeEvictionStrategyWith(configuration);
            var key = new BeaconKey(0, 0);

            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(1000L, 2000L);
            mockBeaconCache.BeaconKeys.Returns(new HashSet<BeaconKey>());

            // when
            target.Execute();

            // then verify interactions
            _ = mockBeaconCache.Received(1).BeaconKeys;
            mockTimingProvider.Received(3).ProvideTimestampInMilliseconds();
            mockBeaconCache.DidNotReceiveWithAnyArgs().EvictRecordsByAge(key, 0L);

            // also ensure that the last run timestamp was updated
            Assert.That(target.LastRunTimestamp, Is.EqualTo(2000L));
        }

        [Test]
        public void ExecuteEvictionCallsEvictionForEachBeaconSeparately()
        {

            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 2000L);
            var target = CreateTimeEvictionStrategyWith(configuration);
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(42, 0);

            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(1000L, 2099L);
            mockBeaconCache.BeaconKeys.Returns(new HashSet<BeaconKey> { keyOne, keyTwo });

            // when
            target.Execute();

            // then verify interactions
            _ = mockBeaconCache.Received(1).BeaconKeys;
            mockBeaconCache.Received(1).EvictRecordsByAge(keyOne, 2099L - configuration.MaxRecordAge);
            mockBeaconCache.Received(1).EvictRecordsByAge(keyTwo, 2099L - configuration.MaxRecordAge);
            mockTimingProvider.Received(3).ProvideTimestampInMilliseconds();

            // also ensure that the last run timestamp was updated
            Assert.That(target.LastRunTimestamp, Is.EqualTo(2099L));
        }

        [Test]
        public void ExecuteEvictionLogsTheNumberOfRecordsRemoved()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 2000L);
            var target = CreateTimeEvictionStrategyWith(configuration);
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(42, 0);

            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(1000L, 2099L);

            mockBeaconCache.BeaconKeys.Returns(new HashSet<BeaconKey> { keyOne, keyTwo });
            mockBeaconCache.EvictRecordsByAge(keyOne, Arg.Any<long>()).Returns(2);
            mockBeaconCache.EvictRecordsByAge(keyTwo, Arg.Any<long>()).Returns(5);

            mockLogger.IsDebugEnabled.Returns(true);

            // when
            target.Execute();

            // then verify that the logger was invoked
            _ = mockLogger.Received(2).IsDebugEnabled;
            mockLogger.Received(1).Debug($"{target.GetType().Name} - Removed 2 records from Beacon with key {keyOne}");
            mockLogger.Received(1).Debug($"{target.GetType().Name} - Removed 5 records from Beacon with key {keyTwo}");
        }

        [Test]
        public void ExecuteEvictionIsStoppedIfThreadGetsInterrupted()
        {
            // given
            var shutdown = false;
            isShutdownFunc = () => shutdown;
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 2000L);
            var target = CreateTimeEvictionStrategyWith(configuration);
            var keyOne = new BeaconKey(1, 0);
            var keyTwo = new BeaconKey(42, 0);

            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(1000L, 2099L);

            mockBeaconCache.BeaconKeys.Returns(new HashSet<BeaconKey> { keyOne, keyTwo });
            mockBeaconCache.EvictRecordsByAge(Arg.Any<BeaconKey>(), Arg.Any<long>())
                .Returns(x =>
                {
                    shutdown = true;
                    return 2;
                });

            // when
            target.Execute();

            // then verify interactions
            _ = mockBeaconCache.Received(1).BeaconKeys;
            mockBeaconCache.Received(1).EvictRecordsByAge(Arg.Any<BeaconKey>(), 2099L - configuration.MaxRecordAge);
        }

        private TimeEvictionStrategy CreateTimeEvictionStrategyWith(IBeaconCacheConfiguration config)
        {
            return new TimeEvictionStrategy(
                mockLogger,
                mockBeaconCache,
                config,
                mockTimingProvider,
                isShutdownFunc
            );
        }

        private IBeaconCacheConfiguration MockBeaconCacheConfig(long maxRecordAge, long lowerSizeBound, long upperSizeBound)
        {
            var builder = Substitute.For<IOpenKitBuilder>();
            builder.BeaconCacheMaxBeaconAge.Returns(maxRecordAge);
            builder.BeaconCacheLowerMemoryBoundary.Returns(lowerSizeBound);
            builder.BeaconCacheUpperMemoryBoundary.Returns(upperSizeBound);

            var config = BeaconCacheConfiguration.From(builder);
            return config;
        }
    }
}
