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

namespace Dynatrace.OpenKit.Core.Configuration
{
    /// <summary>
    /// Configuration class for storing user configured privacy settings.
    /// </summary>
    public class PrivacyConfiguration : IPrivacyConfiguration
    {
        /// <summary>
        /// Default data collection level used if no other value was specified
        /// </summary>
        public const DataCollectionLevel DefaultDataCollectionLevel = DataCollectionLevel.USER_BEHAVIOR;

        /// <summary>
        /// Default crash reporting level used if no other value was specified.
        /// </summary>
        public const CrashReportingLevel DefaultCrashReportingLevel = CrashReportingLevel.OPT_IN_CRASHES;

        public PrivacyConfiguration(DataCollectionLevel dataCollectionLevel, CrashReportingLevel crashReportingLevel)
        {
            DataCollectionLevel = dataCollectionLevel;
            CrashReportingLevel = crashReportingLevel;
        }

        public DataCollectionLevel DataCollectionLevel { get; }

        public CrashReportingLevel CrashReportingLevel { get; }

        public bool IsDeviceIdSendingAllowed => DataCollectionLevel == DataCollectionLevel.USER_BEHAVIOR;

        public bool IsSessionNumberReportingAllowed => DataCollectionLevel == DataCollectionLevel.USER_BEHAVIOR;

        public bool IsWebRequestTracingAllowed => DataCollectionLevel != DataCollectionLevel.OFF;

        public bool IsSessionReportingAllowed => DataCollectionLevel != DataCollectionLevel.OFF;

        public bool IsActionReportingAllowed =>  DataCollectionLevel != DataCollectionLevel.OFF;

        public bool IsValueReportingAllowed => DataCollectionLevel == DataCollectionLevel.USER_BEHAVIOR;

        public bool IsEventReportingAllowed => DataCollectionLevel == DataCollectionLevel.USER_BEHAVIOR;

        public bool IsErrorReportingAllowed => DataCollectionLevel != DataCollectionLevel.OFF;

        public bool IsCrashReportingAllowed => CrashReportingLevel == CrashReportingLevel.OPT_IN_CRASHES;

        public bool IsUserIdentificationIsAllowed => DataCollectionLevel == DataCollectionLevel.USER_BEHAVIOR;
    }
}