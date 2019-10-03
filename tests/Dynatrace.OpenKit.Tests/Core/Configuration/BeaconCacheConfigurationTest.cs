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

using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class BeaconCacheConfigurationTest
    {
        [Test]
        public void BeaconCacheConfigurationFromNullReturnsNull()
        {
            // given, when
            var obtained = BeaconCacheConfiguration.From(null);

            // then
            Assert.That(obtained, Is.Null);
        }

        [Test]
        public void PositiveMaxOrderAgeIsTakenOverFromOpenKitBuilder()
        {
            // given
            const long maxRecordAge = 73;
            var builder = Substitute.For<IOpenKitBuilder>();
            builder.BeaconCacheMaxBeaconAge.Returns(maxRecordAge);

            // when
            var obtained = BeaconCacheConfiguration.From(builder);

            // then
            _ = builder.Received(1).BeaconCacheMaxBeaconAge;
            Assert.That(obtained.MaxRecordAge, Is.EqualTo(maxRecordAge));
        }

        [Test]
        public void NegativeMaxOrderAgeIsTakenOverFromOpenKitBuilder()
        {
            // given
            const long maxRecordAge = -73;
            var builder = Substitute.For<IOpenKitBuilder>();
            builder.BeaconCacheMaxBeaconAge.Returns(maxRecordAge);

            // when
            var obtained = BeaconCacheConfiguration.From(builder);

            // then
            _ = builder.Received(1).BeaconCacheMaxBeaconAge;
            Assert.That(obtained.MaxRecordAge, Is.EqualTo(maxRecordAge));
        }

        [Test]
        public void ZeroMaxOrderAgeIsTakenOverFromOpenKitBuilder()
        {
            // given
            const long maxRecordAge = 0;
            var builder = Substitute.For<IOpenKitBuilder>();
            builder.BeaconCacheMaxBeaconAge.Returns(maxRecordAge);

            // when
            var obtained = BeaconCacheConfiguration.From(builder);

            // then
            _ = builder.Received(1).BeaconCacheMaxBeaconAge;
            Assert.That(obtained.MaxRecordAge, Is.EqualTo(maxRecordAge));
        }

        [Test]
        public void PositiveLowerCacheSizeBoundIsTakenOverFromOpenKitBuilder()
        {
            // given
            const long lowerBound = 73;
            var builder = Substitute.For<IOpenKitBuilder>();
            builder.BeaconCacheLowerMemoryBoundary.Returns(lowerBound);

            // when
            var obtained = BeaconCacheConfiguration.From(builder);

            // then
            _ = builder.Received(1).BeaconCacheLowerMemoryBoundary;
            Assert.That(obtained.CacheSizeLowerBound, Is.EqualTo(lowerBound));
        }

        [Test]
        public void NegativeLowerCacheSizeBoundIsTakenOverFromOpenKitBuilder()
        {
            // given
            const long lowerBound = -73;
            var builder = Substitute.For<IOpenKitBuilder>();
            builder.BeaconCacheLowerMemoryBoundary.Returns(lowerBound);

            // when
            var obtained = BeaconCacheConfiguration.From(builder);

            // then
            _ = builder.Received(1).BeaconCacheLowerMemoryBoundary;
            Assert.That(obtained.CacheSizeLowerBound, Is.EqualTo(lowerBound));
        }

        [Test]
        public void ZeroLowerCacheSizeBoundIsTakenOverFromOpenKitBuilder()
        {
            // given
            const long lowerBound = 0;
            var builder = Substitute.For<IOpenKitBuilder>();
            builder.BeaconCacheLowerMemoryBoundary.Returns(lowerBound);

            // when
            var obtained = BeaconCacheConfiguration.From(builder);

            // then
            _ = builder.Received(1).BeaconCacheLowerMemoryBoundary;
            Assert.That(obtained.CacheSizeLowerBound, Is.EqualTo(lowerBound));
        }

        [Test]
        public void PositiveUpperCacheSizeBoundIsTakenOverFromOpenKitBuilder()
        {
            // given
            const long upperBound = 73;
            var builder = Substitute.For<IOpenKitBuilder>();
            builder.BeaconCacheUpperMemoryBoundary.Returns(upperBound);

            // when
            var obtained = BeaconCacheConfiguration.From(builder);

            // then
            _ = builder.Received(1).BeaconCacheUpperMemoryBoundary;
            Assert.That(obtained.CacheSizeUpperBound, Is.EqualTo(upperBound));
        }

        [Test]
        public void NegativeUpperCacheSizeBoundIsTakenOverFromOpenKitBuilder()
        {
            // given
            const long upperBound = -73;
            var builder = Substitute.For<IOpenKitBuilder>();
            builder.BeaconCacheUpperMemoryBoundary.Returns(upperBound);

            // when
            var obtained = BeaconCacheConfiguration.From(builder);

            // then
            _ = builder.Received(1).BeaconCacheUpperMemoryBoundary;
            Assert.That(obtained.CacheSizeUpperBound, Is.EqualTo(upperBound));
        }

        [Test]
        public void ZeroUpperCacheSizeBoundIsTakenOverFromOpenKitBuilder()
        {
            // given
            const long upperBound = 0;
            var builder = Substitute.For<IOpenKitBuilder>();
            builder.BeaconCacheUpperMemoryBoundary.Returns(upperBound);

            // when
            var obtained = BeaconCacheConfiguration.From(builder);

            // then
            _ = builder.Received(1).BeaconCacheUpperMemoryBoundary;
            Assert.That(obtained.CacheSizeUpperBound, Is.EqualTo(upperBound));
        }
    }
}
