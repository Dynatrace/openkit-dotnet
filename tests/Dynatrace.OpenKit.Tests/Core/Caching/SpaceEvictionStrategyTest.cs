//
// Copyright 2018-2020 Dynatrace LLC
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
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Caching
{
    public class SpaceEvictionStrategyTest
    {
        private ILogger mockLogger;
        private IBeaconCache mockBeaconCache;
        private Func<bool> isShutdownFunc;

        [SetUp]
        public void SetUp()
        {
            mockLogger = Substitute.For<ILogger>();
            mockBeaconCache = Substitute.For<IBeaconCache>();
            isShutdownFunc = () => false;
        }

        [Test]
        public void TheStrategyIsDisabledIfCacheSizeLowerBoundIsLessThanZero()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, -1L, 2000L);
            var target = CreateSpaceEvictionStrategyWith(configuration);

            // then
            Assert.That(target.IsStrategyDisabled, Is.True);
        }

        [Test]
        public void TheStrategyIsDisabledIfCacheSizeLowerBoundIsEqualToZero()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 0L, 2000L);
            var target = CreateSpaceEvictionStrategyWith(configuration);

            // then
            Assert.That(target.IsStrategyDisabled, Is.True);
        }

        [Test]
        public void TheStrategyIsDisabledIfCacheSizeUpperBoundIsLessThanZero()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, -1L);
            var target = CreateSpaceEvictionStrategyWith(configuration);

            // then
            Assert.That(target.IsStrategyDisabled, Is.True);
        }

        [Test]
        public void TheStrategyIsDisabledIfCacheSizeUpperBoundIsEqualToZero()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 0L);
            var target = CreateSpaceEvictionStrategyWith(configuration);

            // then
            Assert.That(target.IsStrategyDisabled, Is.True);
        }

        [Test]
        public void TheStrategyIsDisabledIfCacheSizeUpperBoundIsLessThanLowerBound()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 999L);
            var target = CreateSpaceEvictionStrategyWith(configuration);

            // then
            Assert.That(target.IsStrategyDisabled, Is.True);
        }

        [Test]
        public void ShouldRunGivesTrueIfNumBytesInCacheIsGreaterThanUpperBoundLimit()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 2000L);
            var target = CreateSpaceEvictionStrategyWith(configuration);

            mockBeaconCache.NumBytesInCache.Returns(configuration.CacheSizeUpperBound + 1);

            // then
            Assert.That(target.ShouldRun, Is.True);
        }

        [Test]
        public void ShouldRunGivesFalseIfNumBytesInCacheIsEqualToUpperBoundLimit()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 2000L);
            var target = CreateSpaceEvictionStrategyWith(configuration);

            mockBeaconCache.NumBytesInCache.Returns(configuration.CacheSizeUpperBound);

            // then
            Assert.That(target.ShouldRun, Is.False);
        }

        [Test]
        public void ShouldRunGivesFalseIfNumBytesInCacheIsLessThanUpperBoundLimit()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 2000L);
            var target = CreateSpaceEvictionStrategyWith(configuration);

            mockBeaconCache.NumBytesInCache.Returns(configuration.CacheSizeUpperBound - 1);

            // then
            Assert.That(target.ShouldRun, Is.False);
        }

        [Test]
        public void ExecuteEvictionLogsAMessageOnceAndReturnsIfStrategyIsDisabled()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, -1L);
            var target = CreateSpaceEvictionStrategyWith(configuration);

            mockLogger.IsInfoEnabled.Returns(true);

            // when executing the first time
            target.Execute();

            // then
            _ = mockLogger.Received(1).IsInfoEnabled;
            mockLogger.Received(1).Info(Arg.Any<string>());

            // and when executing a second time
            mockLogger.ClearReceivedCalls();
            target.Execute();

            // then
            _ = mockLogger.DidNotReceive().IsInfoEnabled;
            mockLogger.DidNotReceive().Info(Arg.Any<string>());
        }

        [Test]
        public void ExecuteEvictionDoesNotLogIfStrategyIsDisabledAndInfoIsDisabledInLogger()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, -1L);
            var target = CreateSpaceEvictionStrategyWith(configuration);

            mockLogger.IsInfoEnabled.Returns(false);

            // when executing the first time
            target.Execute();

            // then
            _ = mockLogger.Received(1).IsInfoEnabled;
            mockLogger.DidNotReceive().Info(Arg.Any<string>());

            // and when executing a second time
            target.Execute();

            // then
            _ = mockLogger.Received(2).IsInfoEnabled;
            mockLogger.DidNotReceive().Info(Arg.Any<string>());
        }

        [Test]
        public void ExecuteEvictionCallsCacheMethodForEachBeacon()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 2000L);
            var target = CreateSpaceEvictionStrategyWith(configuration);

            mockBeaconCache.NumBytesInCache.Returns(configuration.CacheSizeUpperBound + 1,
                configuration.CacheSizeUpperBound + 1,
                configuration.CacheSizeUpperBound + 1,
                configuration.CacheSizeUpperBound + 1,
                0L);
            var keyOne = new BeaconKey(42, 0);
            var keyTwo = new BeaconKey(1, 0);
            mockBeaconCache.BeaconKeys.Returns(new HashSet<BeaconKey> { keyOne, keyTwo });

            // when executing the first time
            target.Execute();

            // then
            _ = mockBeaconCache.Received(5).NumBytesInCache;
            mockBeaconCache.Received(1).EvictRecordsByNumber(keyTwo, 1);
            mockBeaconCache.Received(1).EvictRecordsByNumber(keyOne, 1);
        }

        [Test]
        public void ExecuteEvictionLogsEvictionResultIfDebugIsEnabled()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 2000L);
            var target = CreateSpaceEvictionStrategyWith(configuration);

            mockBeaconCache.NumBytesInCache.Returns(configuration.CacheSizeUpperBound + 1,
                configuration.CacheSizeUpperBound + 1,
                configuration.CacheSizeUpperBound + 1,
                configuration.CacheSizeUpperBound + 1,
                0L);
            var keyOne = new BeaconKey(42, 0);
            var keyTwo = new BeaconKey(1, 0);
            mockBeaconCache.BeaconKeys.Returns(new HashSet<BeaconKey> { keyOne, keyTwo });
            mockBeaconCache.EvictRecordsByNumber(keyTwo, Arg.Any<int>()).Returns(5);
            mockBeaconCache.EvictRecordsByNumber(keyOne, Arg.Any<int>()).Returns(1);

            mockLogger.IsDebugEnabled.Returns(true);

            // when executing the first time
            target.Execute();

            // then
            _ = mockLogger.Received(3).IsDebugEnabled;
            mockLogger.Received(1).Debug($"{target.GetType().Name} - Removed 1 records from Beacon with key {keyOne}");
            mockLogger.Received(1).Debug($"{target.GetType().Name} - Removed 5 records from Beacon with key {keyTwo}");
        }

        [Test]
        public void ExecuteEvictionDoesNotLogEvictionResultIfDebugIsDisabled()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 2000L);
            var target = CreateSpaceEvictionStrategyWith(configuration);

            mockBeaconCache.NumBytesInCache.Returns(configuration.CacheSizeUpperBound + 1,
                configuration.CacheSizeUpperBound + 1,
                configuration.CacheSizeUpperBound + 1,
                configuration.CacheSizeUpperBound + 1,
                0L);
            var keyOne = new BeaconKey(42, 0);
            var keyTwo = new BeaconKey(1, 0);
            mockBeaconCache.BeaconKeys.Returns(new HashSet<BeaconKey> { keyOne, keyTwo });
            mockBeaconCache.EvictRecordsByNumber(keyTwo, Arg.Any<int>()).Returns(5);
            mockBeaconCache.EvictRecordsByNumber(keyOne, Arg.Any<int>()).Returns(1);

            mockLogger.IsDebugEnabled.Returns(false);

            // when executing the first time
            target.Execute();

            // then
            _ = mockLogger.Received(3).IsDebugEnabled;
            mockLogger.DidNotReceive().Debug(Arg.Any<string>());
        }

        [Test]
        public void ExecuteEvictionRunsUntilTheCacheSizeIsLessThanOrEqualToLowerBound()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 2000L);
            var target = CreateSpaceEvictionStrategyWith(configuration);

            mockBeaconCache.NumBytesInCache.Returns(configuration.CacheSizeUpperBound + 1,  //should run method
                configuration.CacheSizeUpperBound, // first iteration
                configuration.CacheSizeUpperBound, // first iteration
                configuration.CacheSizeUpperBound, // first iteration
                (configuration.CacheSizeUpperBound + configuration.CacheSizeLowerBound) / 2, // second iteration
                (configuration.CacheSizeUpperBound + configuration.CacheSizeLowerBound) / 2, // second iteration
                (configuration.CacheSizeUpperBound + configuration.CacheSizeLowerBound) / 2, // second iteration
                configuration.CacheSizeLowerBound, // stops already
                0L); // just for safety

            var keyOne = new BeaconKey(42, 0);
            var keyTwo = new BeaconKey(1, 0);
            mockBeaconCache.BeaconKeys.Returns(new HashSet<BeaconKey> { keyOne, keyTwo });


            // when executing the first time
            target.Execute();

            // then
            _ = mockBeaconCache.Received(8).NumBytesInCache;
            mockBeaconCache.Received(2).EvictRecordsByNumber(keyTwo, 1);
            mockBeaconCache.Received(2).EvictRecordsByNumber(keyOne, 1);
        }

        [Test]
        public void ExecuteEvictionStopsIfThreadGetsInterruptedBetweenTwoBeacons()
        {
            // given
            var shutdown = false;
            isShutdownFunc = () => shutdown;
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 2000L);
            var target = CreateSpaceEvictionStrategyWith(configuration);

            mockBeaconCache.NumBytesInCache.Returns(configuration.CacheSizeUpperBound + 1,  //should run method
                configuration.CacheSizeUpperBound, // first iteration
                configuration.CacheSizeUpperBound, // first iteration
                configuration.CacheSizeUpperBound, // first iteration
                (configuration.CacheSizeUpperBound + configuration.CacheSizeLowerBound) / 2, // second iteration
                (configuration.CacheSizeUpperBound + configuration.CacheSizeLowerBound) / 2, // second iteration
                (configuration.CacheSizeUpperBound + configuration.CacheSizeLowerBound) / 2, // second iteration
                configuration.CacheSizeLowerBound, // stops already
                0L); // just for safety

            var keyOne = new BeaconKey(42, 0);
            var keyTwo = new BeaconKey(1, 0);
            mockBeaconCache.BeaconKeys.Returns(new HashSet<BeaconKey> { keyOne, keyTwo });
            mockBeaconCache.EvictRecordsByNumber(Arg.Any<BeaconKey>(), 1).Returns(x =>
            {
                shutdown = true;
                return 5;
            });

            // when executing
            target.Execute();

            // then
            _ = mockBeaconCache.Received(3).NumBytesInCache;
            mockBeaconCache.Received(1).EvictRecordsByNumber(Arg.Any<BeaconKey>(), 1);
        }

        [Test]
        public void ExecuteEvictionStopsIfNumBytesInCacheFallsBelowLowerBoundBetweenTwoBeacons()
        {
            // given
            var configuration = MockBeaconCacheConfig(1000L, 1000L, 2000L);
            var target = CreateSpaceEvictionStrategyWith(configuration);

            mockBeaconCache.NumBytesInCache.Returns(configuration.CacheSizeUpperBound + 1,  //should run method
                configuration.CacheSizeUpperBound, // first iteration
                configuration.CacheSizeUpperBound, // first iteration
                configuration.CacheSizeUpperBound, // first iteration
                (configuration.CacheSizeUpperBound + configuration.CacheSizeLowerBound) / 2, // second iteration
                (configuration.CacheSizeUpperBound + configuration.CacheSizeLowerBound) / 2, // second iteration
                configuration.CacheSizeLowerBound, // stops already (second iteration)
                0L); // just for safety

            var keyOne = new BeaconKey(42, 0);
            var keyTwo = new BeaconKey(1, 0);
            mockBeaconCache.BeaconKeys.Returns(new HashSet<BeaconKey> { keyOne, keyTwo });

            // when executing
            target.Execute();

            // then
            _ = mockBeaconCache.Received(8).NumBytesInCache;
            mockBeaconCache.Received(3).EvictRecordsByNumber(Arg.Any<BeaconKey>(), 1);
        }

        private SpaceEvictionStrategy CreateSpaceEvictionStrategyWith(IBeaconCacheConfiguration config)
        {
            return new SpaceEvictionStrategy(
                mockLogger,
                mockBeaconCache,
                config,
                isShutdownFunc
                );
        }

        private static IBeaconCacheConfiguration MockBeaconCacheConfig(long maxRecordAge, long lowerSizeBound, long upperSizeBound)
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
