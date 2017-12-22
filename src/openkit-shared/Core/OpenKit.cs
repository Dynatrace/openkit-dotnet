/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Core
{

    /// <summary>
    ///  Actual implementation of the IOpenKit interface.
    /// </summary>
    public class OpenKit : IOpenKit
    {        

        // Configuration reference
        private readonly AbstractConfiguration configuration;
        private readonly ITimingProvider timingProvider;
        private readonly BeaconSender beaconSender;
        private readonly IThreadIDProvider threadIDProvider;

        // *** constructors ***

        public OpenKit(ILogger logger, AbstractConfiguration configuration)
            : this(logger, configuration, new DefaultHTTPClientProvider(), new DefaultTimingProvider(), new DefaultThreadIDProvider())
        {
        }

        protected OpenKit(ILogger logger,
            AbstractConfiguration configuration,
            IHTTPClientProvider httpClientProvider, 
            ITimingProvider timingProvider, 
            IThreadIDProvider threadIDProvider)
        {
            this.configuration = configuration;
            this.timingProvider = timingProvider;
            beaconSender = new BeaconSender(configuration, httpClientProvider, timingProvider);
            this.threadIDProvider = threadIDProvider;
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
            // create beacon for session
            var beacon = new Beacon(configuration, clientIPAddress, threadIDProvider, timingProvider);
            // create session
            return new Session(beaconSender, beacon);
        }

        public void Shutdown()
        {
            beaconSender.Shutdown();
        }
    }
}
