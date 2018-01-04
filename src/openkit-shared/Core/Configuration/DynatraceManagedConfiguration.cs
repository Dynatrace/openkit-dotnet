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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Providers;
using System.Text;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class DynatraceManagedConfiguration : AbstractConfiguration
    {
        private readonly string tenantID;

        public DynatraceManagedConfiguration(string tenantID, string applicationName, string applicationID, long deviceID, string endpointURL, ISSLTrustManager sslTrustManager, ISessionIDProvider sessionIDProvider)
            : base(OpenKitType.DYNATRACE, applicationName, applicationID, deviceID, endpointURL, sessionIDProvider)
        {
            this.tenantID = tenantID;

            HTTPClientConfig = new HTTPClientConfiguration(
                    CreateBaseURL(endpointURL, OpenKitType.DYNATRACE.DefaultMonitorName),
                    OpenKitType.DYNATRACE.DefaultServerID,
                    applicationID,
                    sslTrustManager);
        }

        protected override string CreateBaseURL(string endpointURL, string monitorName)
        {
            StringBuilder urlBuilder = new StringBuilder();

            urlBuilder.Append(endpointURL);
            if (!endpointURL.EndsWith("/") && !monitorName.StartsWith("/"))
            {
                urlBuilder.Append('/');
            }
            urlBuilder.Append(monitorName);
            urlBuilder.Append('/');

            urlBuilder.Append(tenantID);

            return urlBuilder.ToString();
        }
    }
}
