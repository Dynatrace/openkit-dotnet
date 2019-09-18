﻿//
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

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class BeaconConfiguration : IBeaconConfiguration
    {
        public const DataCollectionLevel DefaultDataCollectionLevel = DataCollectionLevel.USER_BEHAVIOR;

        public const CrashReportingLevel DefaultCrashReportingLevel = CrashReportingLevel.OPT_IN_CRASHES;

        public const int DefaultMultiplicity = 1;

        public BeaconConfiguration()
            : this(DefaultMultiplicity, DefaultDataCollectionLevel, DefaultCrashReportingLevel)
        {
        }

        public BeaconConfiguration(int multiplicity, DataCollectionLevel dataCollectionLevel, CrashReportingLevel crashReportingLevel)
        {
            Multiplicity = multiplicity;
            DataCollectionLevel = dataCollectionLevel;
            CrashReportingLevel = crashReportingLevel;
        }

        public DataCollectionLevel DataCollectionLevel { get; }

        public CrashReportingLevel CrashReportingLevel { get; }

        public int Multiplicity { get; }

        public bool CapturingAllowed => Multiplicity > 0;
    }
}
