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
    public class OpenKitConfiguration
    {
        private const bool DEFAULT_CAPTURE = true;                      // default: capture on
        private const int DEFAULT_SEND_INTERVAL = 2 * 60 * 1000;        // default: wait 2m (in ms) to send beacon
        private const int DEFAULT_MAX_BEACON_SIZE = 30 * 1024;          // default: max 30KB (in B) to send in one beacon
        private const bool DEFAULT_CAPTURE_ERRORS = true;               // default: capture errors on
        private const bool DEFAULT_CAPTURE_CRASHES = true;              // default: capture crashes on

        // mutable settings
        // TODO stefan.eberl@dynatrace.com - 2017-12-06 - capture/captureErrors/captureCrashes must be thread safe (APM-114816)

        // application and device settings
        private string applicationVersion;

        // caching settings

        private readonly ISessionIdProvider sessionIdProvider;

        #region constructors

        public OpenKitConfiguration(OpenKitType openKitType, string applicationName, string applicationId, long deviceId, string origDeviceId, string endpointUrl,
            ISessionIdProvider sessionIdProvider, ISSLTrustManager trustManager, Device device, string applicationVersion,
            BeaconCacheConfiguration beaconCacheConfiguration, BeaconConfiguration beaconConfiguration)
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
            IsCaptureOn = DEFAULT_CAPTURE;
            SendInterval = DEFAULT_SEND_INTERVAL;
            MaxBeaconSize = DEFAULT_MAX_BEACON_SIZE;
            CaptureErrors = DEFAULT_CAPTURE_ERRORS;
            CaptureCrashes = DEFAULT_CAPTURE_CRASHES;

            this.Device = device;

            HttpClientConfig = new HttpClientConfiguration(
                endpointUrl,
                openKitType.DefaultServerId,
                applicationId,
                trustManager);

            this.applicationVersion = applicationVersion;

            this.BeaconCacheConfig = beaconCacheConfiguration;

            this.sessionIdProvider = sessionIdProvider;

            BeaconConfig = beaconConfiguration;
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

        public BeaconCacheConfiguration BeaconCacheConfig { get; }

        public HttpClientConfiguration HttpClientConfig { get; private set; }

        public BeaconConfiguration BeaconConfig { get; }

        #endregion

        // updates settings based on a status response
        public void UpdateSettings(StatusResponse statusResponse)
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
            if (HttpClientConfig.ServerId != newServerId)
            {
                HttpClientConfig = new HttpClientConfiguration(
                    EndpointUrl,
                    newServerId,
                    ApplicationId,
                    HttpClientConfig.SslTrustManager);
            }

            // use send interval from beacon response or default
            int newSendInterval = statusResponse.SendInterval;
            if (newSendInterval == -1)
            {
                newSendInterval = DEFAULT_SEND_INTERVAL;
            }
            // check if send interval has to be updated
            if (SendInterval != newSendInterval)
            {
                SendInterval = newSendInterval;
            }

            // use max beacon size from beacon response or default
            int newMaxBeaconSize = statusResponse.MaxBeaconSize;
            if (newMaxBeaconSize == -1)
            {
                newMaxBeaconSize = DEFAULT_MAX_BEACON_SIZE;
            }
            if (MaxBeaconSize != newMaxBeaconSize)
            {
                MaxBeaconSize = newMaxBeaconSize;
            }

            // use capture settings for errors and crashes
            CaptureErrors = statusResponse.CaptureErrors;
            CaptureCrashes = statusResponse.CaptureCrashes;
        }

        internal void EnableCapture()
        {
            IsCaptureOn = true;
        }

        internal void DisableCapture()
        {
            IsCaptureOn = false;
        }
    }
}
