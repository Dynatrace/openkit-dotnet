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
    public class AppMonConfiguration : AbstractConfiguration
    {
        /// <summary>
        /// Constructs new instance of AppMonConfiguration
        /// For AppMon applicationId and applicationName are identical. Use application name to initialize both fields.
        /// </summary>
        /// <param name="applicationName">name of the application</param>
        /// <param name="deviceID">id identifying the device</param>
        /// <param name="endpointURL">URL of the endpoint</param>
        /// <param name="verbose">if set to <code>true</code> enables debug output</param>
        public AppMonConfiguration(string applicationName, long deviceID, string endpointURL, ISSLTrustManager sslTrustManager, ISessionIDProvider sessionIDProvider)
            : base(OpenKitType.APPMON, applicationName, applicationName, deviceID, endpointURL, sessionIDProvider)
        {
            HTTPClientConfig = new HTTPClientConfiguration(
                    CreateBaseURL(endpointURL, OpenKitType.APPMON.DefaultMonitorName),
                    OpenKitType.APPMON.DefaultServerID,
                    applicationName,
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

            return urlBuilder.ToString();
        }
    }
}
