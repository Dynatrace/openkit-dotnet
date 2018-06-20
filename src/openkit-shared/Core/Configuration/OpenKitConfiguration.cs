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
        private bool capture;                                       // capture on/off; can be written/read by different threads -> atomic (bool should be accessed atomic in .NET)
        private int sendInterval;                                   // beacon send interval; is only written/read by beacon sender thread -> non-atomic
        private int maxBeaconSize;                                  // max beacon size; is only written/read by beacon sender thread -> non-atomic
        private bool captureErrors;                                 // capture errors on/off; can be written/read by different threads -> atomic (bool should be accessed atomic in .NET)
        private bool captureCrashes;                                // capture crashes on/off; can be written/read by different threads -> atomic (bool should be accessed atomic in .NET)
        // TODO stefan.eberl@dynatrace.com - 2017-12-06 - capture/captureErrors/captureCrashes must be thread safe (APM-114816)

        // application and device settings
        private string applicationVersion;
        private readonly Device device;

        // caching settings
        private readonly BeaconCacheConfiguration beaconCacheConfiguration;

        private readonly ISessionIDProvider sessionIDProvider;

        // *** constructors ***
       public OpenKitConfiguration(OpenKitType openKitType, string applicationName, string applicationID, long deviceID, string endpointURL,
            ISessionIDProvider sessionIDProvider, ISSLTrustManager trustManager, Device device, string applicationVersion,
            BeaconCacheConfiguration beaconCacheConfiguration, BeaconConfiguration beaconConfiguration)
        {
            OpenKitType = openKitType;

            // immutable settings
            ApplicationName = applicationName;
            ApplicationID = applicationID;
            DeviceID = deviceID;
            EndpointURL = endpointURL;

            // mutable settings
            capture = DEFAULT_CAPTURE;
            sendInterval = DEFAULT_SEND_INTERVAL;
            maxBeaconSize = DEFAULT_MAX_BEACON_SIZE;
            captureErrors = DEFAULT_CAPTURE_ERRORS;
            captureCrashes = DEFAULT_CAPTURE_CRASHES;

            this.device = device;

            HTTPClientConfig = new HTTPClientConfiguration(
                endpointURL,
                openKitType.DefaultServerID,
                applicationID,
                trustManager);

            this.applicationVersion = applicationVersion;

            this.beaconCacheConfiguration = beaconCacheConfiguration;

            this.sessionIDProvider = sessionIDProvider;

            BeaconConfig = beaconConfiguration;
        }

        // *** public methods ***

        // return next session number
        public int NextSessionNumber
        {
            get
            {
                return sessionIDProvider.GetNextSessionID();
            }
        }

        public bool IsDynatrace => OpenKitType == OpenKitType.DYNATRACE;

        public OpenKitType OpenKitType { get; }

        public string ApplicationName { get; }

        public string ApplicationID { get; }

        public long DeviceID { get; }

        public string EndpointURL { get; }

        // TODO stefan.eberl@dynatrace.com is accessed from multiple threads
        public bool IsCaptureOn
        {
            get
            {
                return capture;
            }
            private set
            {
                capture = value;
            }
        }

        public int SendInterval
        {
            get
            {
                return sendInterval;
            }
        }

        public int MaxBeaconSize
        {
            get
            {
                return maxBeaconSize;
            }
        }

        // TODO stefan.eberl@dynatrace.com is accessed from multiple threads
        public bool CaptureErrors
        {
            get
            {
                return captureErrors;
            }
            private set
            {
                captureErrors = value;
            }
        }

        // TODO stefan.eberl@dynatrace.com is accessed from multiple threads
        public bool CaptureCrashes
        {
            get
            {
                return captureCrashes;
            }
            private set
            {
                captureCrashes = value;
            }
        }

        public string ApplicationVersion
        {
            get
            {
                return applicationVersion;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.applicationVersion = value;
                }
            }
        }

        public Device Device
        {
            get
            {
                return device;
            }
        }

        public BeaconCacheConfiguration BeaconCacheConfig
        {
            get
            {
                return beaconCacheConfiguration;
            }
        }

        public HTTPClientConfiguration HTTPClientConfig { get; private set; }

        public BeaconConfiguration BeaconConfig { get; }

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
            string newMonitorName = statusResponse.MonitorName;
            if (newMonitorName == null)
            {
                newMonitorName = OpenKitType.DefaultMonitorName;
            }

            // use server id from beacon response or default
            int newServerID = statusResponse.ServerID;
            if (newServerID == -1)
            {
                newServerID = OpenKitType.DefaultServerID;
            }

            // check if http config needs to be updated
            if (HTTPClientConfig.ServerID != newServerID)
            {
                HTTPClientConfig = new HTTPClientConfiguration(
                    EndpointURL,
                    newServerID,
                    ApplicationID,
                    HTTPClientConfig.SSLTrustManager);
            }

            // use send interval from beacon response or default
            int newSendInterval = statusResponse.SendInterval;
            if (newSendInterval == -1)
            {
                newSendInterval = DEFAULT_SEND_INTERVAL;
            }
            // check if send interval has to be updated
            if (sendInterval != newSendInterval)
            {
                sendInterval = newSendInterval;
            }

            // use max beacon size from beacon response or default
            int newMaxBeaconSize = statusResponse.MaxBeaconSize;
            if (newMaxBeaconSize == -1)
            {
                newMaxBeaconSize = DEFAULT_MAX_BEACON_SIZE;
            }
            if (maxBeaconSize != newMaxBeaconSize)
            {
                maxBeaconSize = newMaxBeaconSize;
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
