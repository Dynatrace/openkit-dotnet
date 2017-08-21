/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Core {

    /// <summary>
    ///  Actual implementation of the IOpenKit interface.
    /// </summary>
    public class OpenKit : IOpenKit {

        public class OpenKitType {

            public static readonly OpenKitType APPMON = new OpenKitType("dynaTraceMonitor", 1);         // AppMon: default monitor URL name contains "dynaTraceMonitor" and default Server ID is 1
            public static readonly OpenKitType DYNATRACE = new OpenKitType("mbeacon", -1);              // Dynatrace: default monitor URL name contains "mbeacon" and default Server ID is -1

            private string defaultMonitorName;
            private int defaultServerID;
            
            OpenKitType(string defaultMonitorName, int defaultServerID) {
                this.defaultMonitorName = defaultMonitorName;
                this.defaultServerID = defaultServerID;
            }

            public string DefaultMonitorName {
                get {
                    return defaultMonitorName;
                }
            }

            public int DefaultServerID {
                get {
                    return defaultServerID;
                }
            }

        }

        // only set to true after initialized() was called and calls to the OpenKit are allowed
        private bool initialized;

        // Configuration reference
        private Configuration configuration;

        // dummy Session implementation, used if capture is set to off
        private static DummySession dummySessionInstance = new DummySession();

        // *** constructors ***

        public OpenKit(OpenKitType type, string applicationName, string applicationID, long visitorID, string endpointURL, bool verbose) {
            configuration = new Configuration(applicationName, applicationID, visitorID, endpointURL, type, verbose);
        }

        // *** IOpenKit interface methods ***

        public void Initialize() {
            configuration.Initialize();
            initialized = true;
        }

        public string ApplicationVersion {
            set {
                configuration.ApplicationVersion = value;
            }
        }
           
        public IDevice Device {
            get {
                return configuration.Device;
            }
        }

        public ISession CreateSession(string clientIPAddress) {
            if (initialized && configuration.Capture) {
                return new Session(configuration, clientIPAddress);
            } else {
                return dummySessionInstance;
            }
        }

        public void Shutdown() {
            configuration.Shutdown();
        }

    }
}
