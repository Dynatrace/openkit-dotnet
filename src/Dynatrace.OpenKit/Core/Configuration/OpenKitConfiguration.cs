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

using System.Text;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Core.Util;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Core.Configuration
{

    /// <summary>
    ///  The OpenKitConfiguration class holds all configuration settings, both provided by the user and the Dynatrace/AppMon server.
    /// </summary>
    public class OpenKitConfiguration : IOpenKitConfiguration
    {
        private const bool DefaultCapture = true;                      // default: capture on
        private const int DefaultSendInterval = 2 * 60 * 1000;         // default: wait 2m (in ms) to send beacon
        private const int DefaultMaxBeaconSize = 30 * 1024;            // default: max 30KB (in B) to send in one beacon
        private const bool DefaultCaptureErrors = true;                // default: capture errors on
        private const bool DefaultCaptureCrashes = true;               // default: capture crashes on

        // application and device settings
        private string applicationVersion;

        private IHttpClientConfiguration httpClientConfig;
        private readonly IBeaconConfiguration beaconConfig;
        private readonly IBeaconCacheConfiguration beaconCacheConfig;
        private readonly IPrivacyConfiguration privacyConfig;

        // caching settings

        private readonly ISessionIdProvider sessionIdProvider;

        #region constructors

        internal OpenKitConfiguration(
            OpenKitType openKitType,
            string applicationName,
            string applicationId,
            long deviceId,
            string origDeviceId,
            string endpointUrl,
            ISessionIdProvider sessionIdProvider,
            ISSLTrustManager trustManager,
            Device device, string applicationVersion,
            IBeaconCacheConfiguration beaconCacheConfiguration,
            IBeaconConfiguration beaconConfiguration,
            IPrivacyConfiguration privacyConfiguration
            )
        {
            OpenKitType = openKitType;

            // immutable settings
            ApplicationName = applicationName;
            ApplicationId = applicationId;
            ApplicationIdPercentEncoded = PercentEncoder.Encode(applicationId, Encoding.UTF8, new[] { '_' });
            DeviceId = deviceId;
            OrigDeviceId = origDeviceId;
            EndpointUrl = endpointUrl;

            // mutable settings
            IsCaptureOn = DefaultCapture;
            SendInterval = DefaultSendInterval;
            MaxBeaconSize = DefaultMaxBeaconSize;
            CaptureErrors = DefaultCaptureErrors;
            CaptureCrashes = DefaultCaptureCrashes;

            Device = device;

            httpClientConfig = new HttpClientConfiguration(
                endpointUrl,
                openKitType.DefaultServerId,
                applicationId,
                trustManager);

            this.applicationVersion = applicationVersion;
            this.sessionIdProvider = sessionIdProvider;

            beaconConfig = beaconConfiguration;
            beaconCacheConfig = beaconCacheConfiguration;
            privacyConfig = privacyConfiguration;
        }

        #endregion

        #region public properties

        // return next session number
        public int NextSessionNumber => sessionIdProvider.GetNextSessionId();

        public OpenKitType OpenKitType { get; }

        public string ApplicationName { get; }

        public string ApplicationId { get; }

        public string ApplicationIdPercentEncoded { get; }

        public long DeviceId { get; }

        public string OrigDeviceId { get; }

        public string EndpointUrl { get; }

        // TODO stefan.eberl@dynatrace.com is accessed from multiple threads
        public bool IsCaptureOn { get; private set; }

        public int SendInterval { get; private set; }

        public int MaxBeaconSize { get; private set; }

        // TODO stefan.eberl@dynatrace.com is accessed from multiple threads
        public bool CaptureErrors { get; private set; }

        // TODO stefan.eberl@dynatrace.com is accessed from multiple threads
        public bool CaptureCrashes { get; private set; }

        public string ApplicationVersion
        {
            get => applicationVersion;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    applicationVersion = value;
                }
            }
        }

        public Device Device { get; }

        IBeaconCacheConfiguration IOpenKitConfiguration.BeaconCacheConfig => beaconCacheConfig;

        IPrivacyConfiguration IOpenKitConfiguration.PrivacyConfig => privacyConfig;

        IHttpClientConfiguration IOpenKitConfiguration.HttpClientConfig => httpClientConfig;

        IBeaconConfiguration IOpenKitConfiguration.BeaconConfig => beaconConfig;

        #endregion

        // updates settings based on a status response
        void IOpenKitConfiguration.UpdateSettings(StatusResponse statusResponse)
        {
            // if invalid status response OR response code != 200 -> capture off
            if ((statusResponse == null) || (statusResponse.ResponseCode != 200))
            {
                IsCaptureOn = false;
            }
            else
            {
                IsCaptureOn = statusResponse.Capture;
            }

            // if capture is off -> leave other settings on their current values
            if (!IsCaptureOn)
            {
                return;
            }

            // use monitor name from beacon response or default
            var newMonitorName = statusResponse.MonitorName ?? OpenKitType.DefaultMonitorName;

            // use server id from beacon response or default
            var newServerId = statusResponse.ServerId;
            if (newServerId == -1)
            {
                newServerId = OpenKitType.DefaultServerId;
            }

            // check if http config needs to be updated
            if (httpClientConfig.ServerId != newServerId)
            {
                httpClientConfig = new HttpClientConfiguration(
                    EndpointUrl,
                    newServerId,
                    ApplicationId,
                    httpClientConfig.SslTrustManager);
            }

            // use send interval from beacon response or default
            var newSendInterval = statusResponse.SendInterval;
            if (newSendInterval == -1)
            {
                newSendInterval = DefaultSendInterval;
            }
            // check if send interval has to be updated
            if (SendInterval != newSendInterval)
            {
                SendInterval = newSendInterval;
            }

            // use max beacon size from beacon response or default
            var newMaxBeaconSize = statusResponse.MaxBeaconSize;
            if (newMaxBeaconSize == -1)
            {
                newMaxBeaconSize = DefaultMaxBeaconSize;
            }
            if (MaxBeaconSize != newMaxBeaconSize)
            {
                MaxBeaconSize = newMaxBeaconSize;
            }

            // use capture settings for errors and crashes
            CaptureErrors = statusResponse.CaptureErrors;
            CaptureCrashes = statusResponse.CaptureCrashes;
        }

        void IOpenKitConfiguration.EnableCapture()
        {
            IsCaptureOn = true;
        }

        void IOpenKitConfiguration.DisableCapture()
        {
            IsCaptureOn = false;
        }
    }
}
