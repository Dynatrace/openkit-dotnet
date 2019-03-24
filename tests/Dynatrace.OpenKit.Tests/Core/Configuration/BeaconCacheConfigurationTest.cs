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

using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class BeaconCacheConfigurationTest
    {
        [Test]
        public void MaxRecordAge()
        {
            // then
            Assert.That(new BeaconCacheConfiguration(-100L, 1L, 2L).MaxRecordAge, Is.EqualTo(-100L));
            Assert.That(new BeaconCacheConfiguration(0L, 1L, 2L).MaxRecordAge, Is.EqualTo(0L));
            Assert.That(new BeaconCacheConfiguration(200L, 1L, 2L).MaxRecordAge, Is.EqualTo(200L));
        }

        [Test]
        public void CacheSizeLowerBound()
        {
            // then
            Assert.That(new BeaconCacheConfiguration(0L, -1L, 2L).CacheSizeLowerBound, Is.EqualTo(-1L));
            Assert.That(new BeaconCacheConfiguration(-1L, 0L, 2L).CacheSizeLowerBound, Is.EqualTo(0L));
            Assert.That(new BeaconCacheConfiguration(0L, 1L, 2L).CacheSizeLowerBound, Is.EqualTo(1L));
        }

        [Test]
        public void CacheSizeUpperBound()
        {
            // then
            Assert.That(new BeaconCacheConfiguration(0L, -1L, -2L).CacheSizeUpperBound, Is.EqualTo(-2L));
            Assert.That(new BeaconCacheConfiguration(-1L, 1L, 0L).CacheSizeUpperBound, Is.EqualTo(0L));
            Assert.That(new BeaconCacheConfiguration(0L, 1L, 2L).CacheSizeUpperBound, Is.EqualTo(2L));
        }
    }
}
