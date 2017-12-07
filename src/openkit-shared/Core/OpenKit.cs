/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Core
{

    /// <summary>
    ///  Actual implementation of the IOpenKit interface.
    /// </summary>
    public class OpenKit : IOpenKit
    {        

        // Configuration reference
        private AbstractConfiguration configuration;
        private readonly BeaconSender beaconSender;

        // *** constructors ***

        public OpenKit(AbstractConfiguration configuration)
            : this(configuration, new DefaultHTTPClientProvider(), new DefaultTimingProvider())
        {
        }

        protected OpenKit(AbstractConfiguration configuration, IHTTPClientProvider httpClientProvider, ITimingProvider timingProvider)
        {
            this.configuration = configuration;
            beaconSender = new BeaconSender(configuration, httpClientProvider, timingProvider);
        }
        
        /// <summary>
        /// Initialize this OpenKit instance.
        /// </summary>
        /// <remarks>
        /// This method starts the <see cref="BeaconSender"/>  and is called directly after
        /// the instance has been created in <see cref="OpenKitFactory"/>.
        /// </remarks>
        internal void Initialize()
        {
            beaconSender.Initialize();
        }

        // *** IOpenKit interface methods ***

        public  bool WaitForInitCompletion()
        {
            return beaconSender.WaitForInitCompletion();
        }
        
        public bool WaitForInitCompletion(int timeoutMillis)
        {
            return beaconSender.WaitForInitCompletion(timeoutMillis);
        }

        public bool IsInitialized => beaconSender.IsInitialized;

        public string ApplicationVersion
        {
            set
            {
                configuration.ApplicationVersion = value;
            }
        }

        public IDevice Device => configuration.Device;

        public ISession CreateSession(string clientIPAddress)
        {
            return new Session(configuration, clientIPAddress, beaconSender);
        }

        public void Shutdown()
        {
            beaconSender.Shutdown();
        }
    }
}
