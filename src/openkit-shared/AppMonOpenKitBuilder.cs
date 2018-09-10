//
// Copyright 2018 Dynatrace LLC
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

using Dynatrace.OpenKit.Core;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;
using System.Globalization;

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
        /// <param name="endpointURL">endpoint OpenKit connects to</param>
        /// <param name="applicationName">unique application id</param>
        /// <param name="deviceID">unique device id</param>
        public AppMonOpenKitBuilder(string endpointURL, string applicationName, long deviceID) 
            : this(endpointURL, applicationName, deviceID.ToString(CultureInfo.InvariantCulture))
        {
        }

        /// <summary>
        /// Creates a new instance of type AppMonOpenKitBuilder
        /// </summary>
        /// <param name="endpointURL">endpoint OpenKit connects to</param>
        /// <param name="applicationName">unique application id</param>
        /// <param name="deviceID">unique device id</param>
        public AppMonOpenKitBuilder(string endpointURL, string applicationName, string deviceID)
            : base(endpointURL, deviceID)
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
                OpenKitType.APPMON,
                applicationName,
                applicationName,
                DeviceID,
                EndpointURL,
                new DefaultSessionIDProvider(),
                TrustManager,
                device,
                ApplicationVersion,
                beaconCacheConfig,
                beaconConfig);
        }
    }
}
