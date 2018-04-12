using Dynatrace.OpenKit.Core;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit
{
    /// <summary>
    /// Concrete builder that creates an <code>IOpenKit</code> instance for AppMon
    /// </summary>
    public class AppMonOpenKitBuilder : AbstractOpenKitBuilder
    {
        private readonly string applicationName;

        /// <summary>
        /// Creates a new instance of type AppMonOpenKitBuilder
        /// </summary>
        /// <param name="endpointURL">endpoint OpenKit connects to</param>
        /// <param name="applicationName">unique application id</param>
        /// <param name="deviceID">unique device id</param>
        public AppMonOpenKitBuilder(string endpointURL, string applicationName, long deviceID) 
            : base(endpointURL, deviceID)
        {
            this.applicationName = applicationName;
        }

        internal override OpenKitConfiguration BuildConfiguration()
        {
            var device = new Device(OperatingSystem, Manufacturer, ModelID);

            var beaconCacheConfig = new BeaconCacheConfiguration(
                BeaconCacheMaxBeaconAge, BeaconCacheLowerMemoryBoundary, BeaconCacheUpperMemoryBoundary);

            return new OpenKitConfiguration(
                OpenKitType.APPMON,
                applicationName,
                applicationName,
                DeviceID,
                EndpointURL,
                new DefaultSessionIDProvider(),
                TrustManager,
                device,
                ApplicationVersion,
                beaconCacheConfig);
        }
    }
}
