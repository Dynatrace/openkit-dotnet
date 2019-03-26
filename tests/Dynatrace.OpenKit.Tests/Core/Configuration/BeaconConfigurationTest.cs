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
using System;
using System.Collections.Generic;
using System.Text;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class BeaconConfigurationTest
    {
        [Test]
        public void GetMultiplicityReturnsMultiplicitySetInConstructor()
        {
            // when multiplicity is positive, then
            Assert.That(new BeaconConfiguration(4, DataCollectionLevel.OFF, CrashReportingLevel.OFF).Multiplicity, Is.EqualTo(4));
            // when multiplicity is zero, then
            Assert.That(new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OFF).Multiplicity, Is.EqualTo(0));
            // when multiplicity is negative, then
            Assert.That(new BeaconConfiguration(-3, DataCollectionLevel.OFF, CrashReportingLevel.OFF).Multiplicity, Is.EqualTo(-3));
        }

        [Test]
        public void CapturingIsAllowedWhenMultiplicityIsGreaterThanZero()
        {
            // when, then
            Assert.That(new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF).CapturingAllowed, Is.True);
            Assert.That(new BeaconConfiguration(2, DataCollectionLevel.OFF, CrashReportingLevel.OFF).CapturingAllowed, Is.True);
            Assert.That(new BeaconConfiguration(int.MaxValue, DataCollectionLevel.OFF, CrashReportingLevel.OFF).CapturingAllowed, Is.True);
        }

        [Test]
        public void CapturingIsDisallowedWhenMultiplicityIsLessThanOrEqualToZero()
        {
            // when, then
            Assert.That(new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OFF).CapturingAllowed, Is.False);
            Assert.That(new BeaconConfiguration(-1, DataCollectionLevel.OFF, CrashReportingLevel.OFF).CapturingAllowed, Is.False);
            Assert.That(new BeaconConfiguration(int.MinValue, DataCollectionLevel.OFF, CrashReportingLevel.OFF).CapturingAllowed, Is.False);
        }

        [Test]
        public void GetDataCollectionLevel()
        {
            Assert.That(new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OFF).DataCollectionLevel, Is.EqualTo(DataCollectionLevel.OFF));
            Assert.That(new BeaconConfiguration(0, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF).DataCollectionLevel, Is.EqualTo(DataCollectionLevel.PERFORMANCE));
            Assert.That(new BeaconConfiguration(0, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF).DataCollectionLevel, Is.EqualTo(DataCollectionLevel.USER_BEHAVIOR));
        }

        [Test]
        public void GetCrashReportingLevel()
        {
            Assert.That(new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OFF).CrashReportingLevel, Is.EqualTo(CrashReportingLevel.OFF));
            Assert.That(new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OPT_OUT_CRASHES).CrashReportingLevel, Is.EqualTo(CrashReportingLevel.OPT_OUT_CRASHES));
            Assert.That(new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OPT_IN_CRASHES).CrashReportingLevel, Is.EqualTo(CrashReportingLevel.OPT_IN_CRASHES));
        }
    }
}
