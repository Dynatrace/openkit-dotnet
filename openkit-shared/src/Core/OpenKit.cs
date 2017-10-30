/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;

namespace Dynatrace.OpenKit.Core {

    /// <summary>
    ///  Actual implementation of the IOpenKit interface.
    /// </summary>
    public class OpenKit : IOpenKit {        

        // only set to true after initialized() was called and calls to the OpenKit are allowed
        private bool initialized;

        // Configuration reference
        private AbstractConfiguration configuration;

        // dummy Session implementation, used if capture is set to off
        private static DummySession dummySessionInstance = new DummySession();

        // *** constructors ***

        public OpenKit(AbstractConfiguration configuration) {
            this.configuration = configuration;
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
