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

using System;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit
{
    /// <summary>
    /// Concrete builder that creates an <code>IOpenKit</code> instance for AppMon
    /// </summary>
    public class AppMonOpenKitBuilder : AbstractOpenKitBuilder
    {
        private readonly string applicationName;

        /// <summary>
        /// Creates a new instance of type AppMonOpenKitBuilder
        /// </summary>
        /// <param name="endpointUrl">endpoint OpenKit connects to</param>
        /// <param name="applicationName">unique application id</param>
        /// <param name="deviceId">unique device id</param>
        public AppMonOpenKitBuilder(string endpointUrl, string applicationName, long deviceId)
           : base(endpointUrl, deviceId)
        {
            this.applicationName = applicationName;
        }

        /// <summary>
        /// Creates a new instance of type AppMonOpenKitBuilder
        /// </summary>
        /// <param name="endpointUrl">endpoint OpenKit connects to</param>
        /// <param name="applicationName">unique application id</param>
        /// <param name="deviceId">unique device id</param>
        [Obsolete("use AppMonOpenKitBuilder(string, string, long) instead")]
        public AppMonOpenKitBuilder(string endpointUrl, string applicationName, string deviceId)
            : base(endpointUrl, deviceId)
        {
            this.applicationName = applicationName;
        }

        internal override OpenKitConfiguration BuildConfiguration()
        {
            var device = new Device(OperatingSystem, Manufacturer, ModelID);

            var beaconCacheConfig = new BeaconCacheConfiguration(
                BeaconCacheMaxBeaconAge, BeaconCacheLowerMemoryBoundary, BeaconCacheUpperMemoryBoundary);

            var beaconConfig = new BeaconConfiguration(BeaconConfiguration.DEFAULT_MULITPLICITY, DataCollectionLevel, CrashReportingLevel);

            return new OpenKitConfiguration(
                OpenKitType.AppMon,
                applicationName,
                applicationName,
                DeviceId,
                OrigDeviceId,
                EndpointUrl,
                new DefaultSessionIdProvider(),
                TrustManager,
                device,
                ApplicationVersion,
                beaconCacheConfig,
                beaconConfig);
        }
    }
}
