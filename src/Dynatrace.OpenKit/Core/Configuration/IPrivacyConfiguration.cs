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
    /// Configuration for storing user configured privacy settings.
    /// </summary>
    public interface IPrivacyConfiguration
    {
        /// <summary>
        /// Returns the data collection level.
        /// </summary>
        DataCollectionLevel DataCollectionLevel { get; }

        /// <summary>
        /// Returns the crash reporting level.
        /// </summary>
        CrashReportingLevel CrashReportingLevel { get; }

        /// <summary>
        /// Indicates whether sending the device identifier is allowed or not.
        /// </summary>
        /// <returns><code>true</code> if sending device identifier is allowed, <code>false</code> otherwise.</returns>
        bool IsDeviceIdSendingAllowed { get; }

        /// <summary>
        /// Indicates whether sending the session number is allowed or not.
        /// </summary>
        /// <returns><code>true</code> if sending the session number is allowed, <code>false</code> otherwise.</returns>
        bool IsSessionNumberReportingAllowed { get; }

        /// <summary>
        /// Indicates whether tracing web requests is allowed or not.
        /// </summary>
        /// <returns><code>true</code> if web request tracing is allowed, <code>false</code> otherwise.</returns>
        bool IsWebRequestTracingAllowed { get; }

        /// <summary>
        /// Indicates whether reporting ended sessions is allowed or not.
        /// </summary>
        /// <returns><code>true</code> if ended sessions can be reported, <code>false</code> otherwise.</returns>
        bool IsSessionReportingAllowed { get; }

        /// <summary>
        /// Indicates whether reporting actions is allowed or not.
        /// </summary>
        /// <returns><code>true</code> if action reporting is allowed, <code>false</code> otherwise.</returns>
        bool IsActionReportingAllowed { get; }

        /// <summary>
        /// Indicates whether reporting values is allowed or not.
        /// </summary>
        /// <returns><code>true</code> if value reporting is allowed, <code>false</code> otherwise.</returns>
        bool IsValueReportingAllowed { get; }

        /// <summary>
        /// Indicates whether reporting events is allowed or not.
        /// </summary>
        /// <returns><code>true</code> if event reporting is allowed, <code>false</code> otherwise.</returns>
        bool IsEventReportingAllowed { get; }

        /// <summary>
        /// Indicates whether reporting errors is allowed or not.
        /// </summary>
        /// <returns><code>true</code> if error reporting is allowed, <code>false</code> otherwise.</returns>
        bool IsErrorReportingAllowed { get; }

        /// <summary>
        /// Indicates whether reporting crashes is allowed or not.
        /// </summary>
        /// <returns><code>true</code> if crash reporting is allowed, <code>false</code> otherwise.</returns>
        bool IsCrashReportingAllowed { get; }

        /// <summary>
        /// Indicates whether identifying users is allowed or not.
        /// </summary>
        /// <returns><code>true</code> if user identification is allowed, <code>false</code> otherwise.</returns>
        bool IsUserIdentificationIsAllowed { get; }
    }
}