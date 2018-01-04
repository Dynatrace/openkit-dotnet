using Dynatrace.OpenKit.Core;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit
{
    /// <summary>
    /// Concrete builder that creates an <code>IOpenKit</code> instance for Dynatrace SaaS/Managed
    /// </summary>
    public class DynatraceOpenKitBuilder : AbstractOpenKitBuilder
    {
        private readonly string applicationID;
        private string applicationName;

        /// <summary>
        /// Creates a new instance of type DynatraceOpenKitBuilder
        /// </summary>
        /// <param name="endointURL">endpoint OpenKit connects to</param>
        /// <param name="applicationID">unique application id</param>
        /// <param name="deviceID">unique device id</param>
        public DynatraceOpenKitBuilder(string endointURL, string applicationID, long deviceID)
            : base(endointURL, deviceID)
        {
            this.applicationID = applicationID;
        }

        /// <summary>
        /// Sets the application name
        /// </summary>
        /// <param name="applicationName">name of the application</param>
        /// <returns><code>this</code></returns>
        public AbstractOpenKitBuilder WithApplicationName(string applicationName)
        {
            this.applicationName = applicationName;
            return this;
        }

        internal override OpenKitConfiguration BuildConfiguration()
        {
            var device = new Device(OperatingSystem, Manufacturer, ModelID);

            return new OpenKitConfiguration(
               OpenKitType.DYNATRACE,
               applicationName,
               applicationID,
               DeviceID,
               EndpointURL,
               new DefaultSessionIDProvider(),
               TrustManager,
               device,
               ApplicationVersion);
        }
    }
}
