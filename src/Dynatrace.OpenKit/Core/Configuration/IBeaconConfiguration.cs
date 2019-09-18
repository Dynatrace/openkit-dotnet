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

using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core.Configuration
{
    /// <summary>
    /// This interface holds the relevant configuration for the <see cref="IBeacon"/>.
    /// </summary>
    internal interface IBeaconConfiguration
    {
        /// <summary>
        /// Returns the configured <see cref="DataCollectionLevel"/>
        /// </summary>
        DataCollectionLevel DataCollectionLevel { get; }

        /// <summary>
        /// Returns the configured <see cref="CrashReportingLevel"/>
        /// </summary>
        CrashReportingLevel CrashReportingLevel { get; }

        /// <summary>
        /// Returns the configured multiplicity
        /// </summary>
        int Multiplicity { get; }

        /// <summary>
        /// Indicates whether capturing is currently allowed or not.
        /// </summary>
        bool CapturingAllowed { get; }
    }
}