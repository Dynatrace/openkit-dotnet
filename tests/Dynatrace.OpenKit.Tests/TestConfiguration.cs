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

using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol.SSL;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit
{
    public class TestConfiguration : OpenKitConfiguration
    {
        public TestConfiguration()
            : this(0, new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OPT_IN_CRASHES), new TestSessionIDProvider())
        {
        }

        internal TestConfiguration(long deviceID, BeaconConfiguration beaconConfig)
            : this(deviceID, beaconConfig, new DefaultSessionIdProvider())
        {

        }

        internal TestConfiguration(long deviceID, BeaconConfiguration beaconConfig, ISessionIdProvider sessionIDProvider)
            : this("", deviceID, beaconConfig, sessionIDProvider)
        {
        }

        internal TestConfiguration(string appID, long deviceID, BeaconConfiguration beaconConfig, ISessionIdProvider sessionIDProvider)
            : base(OpenKitType.Dynatrace, "", appID, deviceID, deviceID.ToString(), "", sessionIDProvider,
          new SSLStrictTrustManager(), new Core.Objects.Device("", "", ""), "",
          new BeaconCacheConfiguration(
            BeaconCacheConfiguration.DEFAULT_MAX_RECORD_AGE_IN_MILLIS,
            BeaconCacheConfiguration.DEFAULT_LOWER_MEMORY_BOUNDARY_IN_BYTES,
            BeaconCacheConfiguration.DEFAULT_UPPER_MEMORY_BOUNDARY_IN_BYTES),
          beaconConfig)
        {
            EnableCapture();
        }
    }
}
