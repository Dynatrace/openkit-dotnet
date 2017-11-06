/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;
using Dynatrace.OpenKit.src.Providers;

namespace Dynatrace.OpenKit.Core {

    /// <summary>
    ///  Actual implementation of the IOpenKit interface.
    /// </summary>
    public class OpenKit : IOpenKit {        

        // only set to true after initialized() was called and calls to the OpenKit are allowed
        private bool initialized;

        // Configuration reference
        private AbstractConfiguration configuration;
        private readonly BeaconSender beaconSender;

        // dummy Session implementation, used if capture is set to off
        private static DummySession dummySessionInstance = new DummySession();

        // *** constructors ***

        public OpenKit(AbstractConfiguration configuration)
            : this(configuration, new DefaultHTTPClientProvider())
        {
        }

        protected OpenKit(AbstractConfiguration configuration, IHTTPClientProvider httpClientProvider) {
            this.configuration = configuration;
            beaconSender = new BeaconSender(configuration, httpClientProvider);
        }

        // *** IOpenKit interface methods ***

        public void Initialize() {
            beaconSender.Initialize();
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
            if (initialized && configuration.IsCapture) {
                return new Session(configuration, clientIPAddress, beaconSender);
            } else {
                return dummySessionInstance;
            }
        }

        public void Shutdown() {
            beaconSender.Shutdown();
        }

    }
}
