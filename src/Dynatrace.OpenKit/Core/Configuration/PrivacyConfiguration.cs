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

namespace Dynatrace.OpenKit.Core.Configuration
{
    /// <summary>
    /// Configuration class for storing user configured privacy settings.
    /// </summary>
    public class PrivacyConfiguration : IPrivacyConfiguration
    {
        private PrivacyConfiguration(IOpenKitBuilder builder)
        {
            DataCollectionLevel = builder.DataCollectionLevel;
            CrashReportingLevel = builder.CrashReportingLevel;
        }

        /// <summary>
        /// Create a <see cref="IPrivacyConfiguration"/> from the given <see cref="IOpenKitBuilder"/>.
        /// </summary>
        /// <param name="builder">the OpenKit builder for which to create a <see cref="IPrivacyConfiguration"/>.</param>
        /// <returns>
        ///     the newly created <see cref="IPrivacyConfiguration"/> or <code>null</code> if the given
        ///     <see cref="builder"/> is <code>null</code>.
        /// </returns>
        public static IPrivacyConfiguration From(IOpenKitBuilder builder)
        {
            if (builder == null)
            {
                return null;
            }
            return new PrivacyConfiguration(builder);
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