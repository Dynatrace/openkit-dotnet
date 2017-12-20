/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Protocol;
using System.Threading;

namespace Dynatrace.OpenKit.Core.Configuration
{

    /// <summary>
    ///  The Configuration class holds all configuration settings, both provided by the user and the Dynatrace/AppMon server.
    /// </summary>
    public abstract class AbstractConfiguration
    {
        private const bool DEFAULT_CAPTURE = false;                     // default: capture off
        private const int DEFAULT_SEND_INTERVAL = 2 * 60 * 1000;        // default: wait 2m (in ms) to send beacon
        private const int DEFAULT_MAX_BEACON_SIZE = 30 * 1024;          // default: max 30KB (in B) to send in one beacon
        private const bool DEFAULT_CAPTURE_ERRORS = true;               // default: capture errors on
        private const bool DEFAULT_CAPTURE_CRASHES = true;              // default: capture crashes on

        // immutable settings
        private readonly OpenKitType openKitType;
        private readonly string applicationName;
        private readonly string applicationID;
        private readonly long deviceID;
        private readonly string endpointURL;
        private readonly bool verbose;

        // mutable settings
        private bool capture;                                       // capture on/off; can be written/read by different threads -> atomic (bool should be accessed atomic in .NET)
        private int sendInterval;                                   // beacon send interval; is only written/read by beacon sender thread -> non-atomic
        private string monitorName;                                 // monitor name part of URL; is only written/read by beacon sender thread -> non-atomic
        private int maxBeaconSize;                                  // max beacon size; is only written/read by beacon sender thread -> non-atomic
        private bool captureErrors;                                 // capture errors on/off; can be written/read by different threads -> atomic (bool should be accessed atomic in .NET)
        private bool captureCrashes;		                        // capture crashes on/off; can be written/read by different threads -> atomic (bool should be accessed atomic in .NET)
        // TODO stefan.eberl@dynatrace.com - 2017-12-06 - capture/captureErrors/captureCrashes must be thread safe (APM-114816)

        // application and device settings
        private string applicationVersion;
        private Device device;

        private int nextSessionNumber = 0;

        // *** constructors ***

        public AbstractConfiguration(OpenKitType openKitType, string applicationName, string applicationID, long deviceID, string endpointURL, bool verbose)
        {
            this.verbose = verbose;

            this.openKitType = openKitType;

            // immutable settings
            this.applicationName = applicationName;
            this.applicationID = applicationID;
            this.deviceID = deviceID;
            this.endpointURL = endpointURL;

            // mutable settings
            capture = DEFAULT_CAPTURE;
            sendInterval = DEFAULT_SEND_INTERVAL;
            monitorName = openKitType.DefaultMonitorName;
            maxBeaconSize = DEFAULT_MAX_BEACON_SIZE;
            captureErrors = DEFAULT_CAPTURE_ERRORS;
            captureCrashes = DEFAULT_CAPTURE_CRASHES;

            device = new Device();
            applicationVersion = OpenKitConstants.DEFAULT_APPLICATION_VERSION;
        }

        // *** public methods ***

        // return next session number
        public int NextSessionNumber
        {
            get
            {
                return Interlocked.Increment(ref nextSessionNumber);
            }
        }

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
                newMonitorName = openKitType.DefaultMonitorName;
            }

            // use server id from beacon response or default
            int newServerID = statusResponse.ServerID;
            if (newServerID == -1)
            {
                newServerID = openKitType.DefaultServerID;
            }

            // check if http config needs to be updated
            if (!monitorName.Equals(newMonitorName)
                || HTTPClientConfig.ServerID != newServerID)
            {
                HTTPClientConfig = new HTTPClientConfiguration(
                    CreateBaseURL(endpointURL, newMonitorName),
                    newServerID,
                    applicationID,
                    verbose,
                    HTTPClientConfig.SSLTrustManager);

                monitorName = newMonitorName;
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

        // *** protected methods ***

        protected abstract string CreateBaseURL(string endpointURL, string monitorName);

        // *** properties ***

        public bool IsDynatrace
        {
            get
            {
                return openKitType == OpenKitType.DYNATRACE;            // object comparison is possible here
            }
        }

        public string ApplicationName
        {
            get
            {
                return applicationName;
            }
        }

        public string ApplicationID
        {
            get
            {
                return applicationID;
            }
        }

        public long DeviceID
        {
            get
            {
                return deviceID;
            }
        }

        public bool IsVerbose
        {
            get
            {
                return verbose;
            }
        }

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

        public HTTPClientConfiguration HTTPClientConfig { get; protected set; }
    }
}
