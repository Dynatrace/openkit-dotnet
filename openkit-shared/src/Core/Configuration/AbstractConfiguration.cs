/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using System.Text;

using Dynatrace.OpenKit.Protocol;
using System.Threading;
using static Dynatrace.OpenKit.Core.OpenKit;

namespace Dynatrace.OpenKit.Core.Configuration {

    /// <summary>
    ///  The Configuration class holds all configuration settings, both provided by the user and the Dynatrace/AppMon server.
    /// </summary>
    public abstract class AbstractConfiguration {

        private static readonly bool DEFAULT_CAPTURE = true;							    // default: capture on
    	private static readonly int DEFAULT_SEND_INTERVAL = 2 * 60 * 1000;                  // default: wait 2m (in ms) to send beacon
        private static readonly int DEFAULT_MAX_BEACON_SIZE = 30 * 1024;                    // default: max 30KB (in B) to send in one beacon
        private static readonly bool DEFAULT_CAPTURE_ERRORS = true; 						// default: capture errors on
    	private static readonly bool DEFAULT_CAPTURE_CRASHES = true;    					// default: capture crashes on

        // immutable settings
        private OpenKitType openKitType;
    	private string applicationName;
	    private string applicationID;
	    private long visitorID;
        private string endpointURL;
	    private bool verbose;

	    // mutable settings
	    private bool capture;                       // capture on/off; can be written/read by different threads -> atomic (bool should be accessed atomic in .NET)
        private int sendInterval;                   // beacon send interval; is only written/read by beacon sender thread -> non-atomic
        private string monitorName;                 // monitor name part of URL; is only written/read by beacon sender thread -> non-atomic
        private int serverID;                       // Server ID (needed for Dynatrace cluster); is only written/read by beacon sender thread -> non-atomic
        private int maxBeaconSize;                  // max beacon size; is only written/read by beacon sender thread -> non-atomic
        private bool captureErrors;                 // capture errors on/off; can be written/read by different threads -> atomic (bool should be accessed atomic in .NET)
        private bool captureCrashes;		        // capture crashes on/off; can be written/read by different threads -> atomic (bool should be accessed atomic in .NET)

        // application and device settings
        private string applicationVersion;
        private Device device;

        private HTTPClient currentHTTPClient;       // current HTTP client (depending on endpoint, monitor, name application ID and server ID)

        private BeaconSender beaconSender;

        private int nextSessionNumber = 0;

        // *** constructors ***

        public AbstractConfiguration(OpenKitType openKitType, string applicationName, string applicationID, long visitorID, string endpointURL, bool verbose) {
            this.verbose = verbose;

            this.openKitType = openKitType;

            // immutable settings
            this.applicationName = applicationName;
            this.applicationID = applicationID;
            this.visitorID = visitorID;
            this.endpointURL = endpointURL;

            // mutable settings
            capture = DEFAULT_CAPTURE;
            sendInterval = DEFAULT_SEND_INTERVAL;
            monitorName = openKitType.DefaultMonitorName;
            serverID = openKitType.DefaultServerID;
            maxBeaconSize = DEFAULT_MAX_BEACON_SIZE;
            captureErrors = DEFAULT_CAPTURE_ERRORS;
            captureCrashes = DEFAULT_CAPTURE_CRASHES;

            device = new Device();
            applicationVersion = null;
        }

        // *** public methods ***

        // initializes the configuration, called by OpenKit.Initialize()
        public void Initialize() {
            // create beacon sender, but do not start beacon sender thread yet
            beaconSender = new BeaconSender(this);

            // create initial HTTP client
            UpdateCurrentHTTPClient();

            // block until initial settings are received; initialize beacon sender & starts thread
            beaconSender.Initialize();
        }

        // return next session number
        public int NextSessionNumber {
            get {
                return Interlocked.Increment(ref nextSessionNumber);
            }
        }

        // called when new Session is created, passes Session to beacon sender to start managing it
        public void StartSession(Session session) {
            // if capture is off, there's no need to manage open Sessions
            if (Capture) {
                beaconSender.StartSession(session);
            }
        }

        // called when Session is ended, passes Session to beacon sender to stop managing it
        public void FinishSession(Session session) {
            // if capture is off, there's no need to manage open and finished Sessions
            if (Capture) {
                beaconSender.FinishSession(session);
            }
        }

        // updates settings based on a status response
        public void UpdateSettings(StatusResponse statusResponse) {
            // if invalid status response OR response code != 200 -> capture off
            if ((statusResponse == null) || (statusResponse.ResponseCode != 200)) {
                capture = false;
            } else {
                capture = statusResponse.Capture;
            }

            // if capture is off -> clear Sessions on beacon sender and leave other settings on their current values
            if (!Capture) {
                beaconSender.ClearSessions();
                return;
            }

            // use monitor name from beacon response or default
            string newMonitorName = statusResponse.MonitorName;
            if (newMonitorName == null) {
                newMonitorName = openKitType.DefaultMonitorName;
            }

            // use server id from beacon response or default
            int newServerID = statusResponse.ServerID;
            if (newServerID == -1) {
                newServerID = openKitType.DefaultServerID;
            }

            // check if URL changed
            bool urlChanged = false;
            if (!monitorName.Equals(newMonitorName)) {
                monitorName = newMonitorName;
                urlChanged = true;
            }
            if (serverID != newServerID) {
                serverID = newServerID;
                urlChanged = true;
            }

            // URL changed -> HTTP client has to be updated
            if (urlChanged) {
                UpdateCurrentHTTPClient();
            }

            // use send interval from beacon response or default
            int newSendInterval = statusResponse.SendInterval;
            if (newSendInterval == -1) {
                newSendInterval = DEFAULT_SEND_INTERVAL;
            }
            // check if send interval has to be updated
            if (sendInterval != newSendInterval) {
                sendInterval = newSendInterval;
            }

            // use max beacon size from beacon response or default
            int newMaxBeaconSize = statusResponse.MaxBeaconSize;
            if (newMaxBeaconSize == -1) {
                newMaxBeaconSize = DEFAULT_MAX_BEACON_SIZE;
            }
            if (maxBeaconSize != newMaxBeaconSize) {
                maxBeaconSize = newMaxBeaconSize;
            }

            // use capture settings for errors and crashes
            captureErrors = statusResponse.CaptureErrors;
            captureCrashes = statusResponse.CaptureCrashes;
        }

        // shut down configuration -> shut down beacon sender
        public void Shutdown() {
            if (beaconSender != null) {
                beaconSender.Shutdown();
            }
        }

        // *** private methods ***

        private void UpdateCurrentHTTPClient() {
            this.currentHTTPClient = new HTTPClient(CreateBaseURL(endpointURL, monitorName), applicationID, serverID, verbose);
        }

        protected abstract string CreateBaseURL(string endpointURL, string monitorName);

        // *** properties ***

        public bool IsDynatrace {
            get {
                return openKitType == OpenKitType.DYNATRACE;            // object comparison is possible here
            }
        }

        public string ApplicationName {
            get {
                return applicationName;
            }
        }

        public string ApplicationID {
            get {
                return applicationID;
            }
        }

        public long VisitorID {
            get {
                return visitorID;
            }
        }

        public bool IsVerbose {
            get {
                return verbose;
            }
        }

        public bool Capture {
            get {
                return capture;
            }
        }

        public int SendInterval {
            get {
                return sendInterval;
            }
        }

        public int MaxBeaconSize {
            get {
                return maxBeaconSize;
            }
        }

        public bool CaptureErrors {
            get {
                return captureErrors;
            }
        }

        public bool CaptureCrashes {
            get {
                return captureCrashes;
            }
        }

        public string ApplicationVersion {
            get {
                return applicationVersion;
            }
            set {
                this.applicationVersion = value;
            }
        }

        public Device Device {
            get {
                return device;
            }
        }

        public HTTPClient CurrentHTTPClient {
            get {
                return currentHTTPClient;
            }
        }

    }

}
