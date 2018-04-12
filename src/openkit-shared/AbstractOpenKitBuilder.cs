using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol.SSL;

namespace Dynatrace.OpenKit
{
    /// <summary>
    /// Abstract base class for concrete builder. Using the builder an IOpenKit instance can be created
    /// </summary>
    public abstract class AbstractOpenKitBuilder
    {
        // mutable fields
        private ILogger logger;
        private ISSLTrustManager trustManager = new SSLStrictTrustManager();
        private bool verbose;
        private string operatingSystem = OpenKitConstants.DEFAULT_OPERATING_SYSTEM;
        private string manufacturer = OpenKitConstants.DEFAULT_MANUFACTURER;
        private string modelID = OpenKitConstants.DEFAULT_MODEL_ID;
        private string applicationVersion = OpenKitConstants.DEFAULT_APPLICATION_VERSION;
        private long beaconCacheMaxBeaconAge = BeaconCacheConfiguration.DEFAULT_MAX_RECORD_AGE_IN_MILLIS;
        private long beaconCacheLowerMemoryBoundary = BeaconCacheConfiguration.DEFAULT_LOWER_MEMORY_BOUNDARY_IN_BYTES;
        private long beaconCacheUpperMemoryBoundary = BeaconCacheConfiguration.DEFAULT_UPPER_MEMORY_BOUNDARY_IN_BYTES;


        protected AbstractOpenKitBuilder(string endpointURL, long deviceID)
        {
            EndpointURL = endpointURL;
            DeviceID = deviceID;
        }

        protected string OperatingSystem => operatingSystem;
        protected string Manufacturer => manufacturer;
        protected string ModelID => modelID;
        protected string ApplicationVersion => applicationVersion;
        protected ISSLTrustManager TrustManager => trustManager;
        protected ILogger Logger => logger ?? new DefaultLogger(verbose);
        protected string EndpointURL { get; private set; }
        protected long DeviceID { get; private set; }
        protected long BeaconCacheMaxBeaconAge => beaconCacheMaxBeaconAge;
        protected long BeaconCacheLowerMemoryBoundary => beaconCacheLowerMemoryBoundary;
        protected long BeaconCacheUpperMemoryBoundary => beaconCacheUpperMemoryBoundary;

        /// <summary>
        /// Enables verbose mode. Verbose mode is only enabled if the the default logger is used.
        /// If a custom logger is provided by calling <code>WithLogger</code> debug and info log output 
        /// depends on the values returned by <code>IsDebugEnabled</code> and <code>IsInfoEnabled</code>.
        /// </summary>
        /// <returns><code>this</code></returns>
        public AbstractOpenKitBuilder EnableVerbose()
        {
            verbose = true;
            return this;
        }

        /// <summary>
        /// Sets the logger. If no logger is set the default console logger is used. For the default
        /// logger verbose mode is enabled by calling <code>EnableVerbose</code>
        /// </summary>
        /// <param name="logger">the logger</param>
        /// <returns><code>this</code></returns>
        public AbstractOpenKitBuilder WithLogger(ILogger logger)
        {
            this.logger = logger;
            return this;
        }

        /// <summary>
        /// Defines the version of the application. The value is only set if it is neither null nor empty.
        /// </summary>
        /// <param name="applicationVersion">the application version</param>
        /// <returns><code>this</code></returns>
        public AbstractOpenKitBuilder WithApplicationVersion(string applicationVersion)
        {
            if (!string.IsNullOrEmpty(applicationVersion))
            {
                this.applicationVersion = applicationVersion;
            }
            return this;
        }

        /// <summary>
        /// Sets the trust manager. Overrides the default trust manager which is <code>SSLStrictTrustManager</code>
        /// </summary>
        /// <param name="trustManager">trust manager implementation</param>
        /// <returns><code>this</code></returns>
        public AbstractOpenKitBuilder WithTrustManager(ISSLTrustManager trustManager)
        {
            this.trustManager = trustManager;
            return this;
        }

        /// <summary>
        /// Sets the operating system information. The value is only set if it is neither null nor empty.
        /// </summary>
        /// <param name="operatingSystem">the operating system</param>
        /// <returns><code>this</code></returns>
        public AbstractOpenKitBuilder WithOperatingSystem(string operatingSystem)
        {
            if (!string.IsNullOrEmpty(operatingSystem))
            {
                this.operatingSystem = operatingSystem;
            }
            return this;
        }

        /// <summary>
        /// Sets the manufacturer information. The value is only set if it is neither null nor empty.
        /// </summary>
        /// <param name="manufacturer">the manufacturer</param>
        /// <returns><code>this</code></returns>
        public AbstractOpenKitBuilder WithManufacturer(string manufacturer)
        {
            if (!string.IsNullOrEmpty(manufacturer))
            {
                this.manufacturer = manufacturer;
            }
            return this;
        }

        /// <summary>
        /// Sets the model id. The value is only set if it is neither null nor empty.
        /// </summary>
        /// <param name="modelID">the model id</param>
        /// <returns><code>this</code></returns>
        public AbstractOpenKitBuilder WithModelID(string modelID)
        {
            if (!string.IsNullOrEmpty(modelID))
            {
                this.modelID = modelID;
            }
            return this;
        }

        /// <summary>
        /// Sets the maximum beacon age of beacon data in cache.
        /// </summary>
        /// <param name="maxBeaconAgeInMilliseconds">The maximum beacon age in milliseconds, or unbounded if negative.</param>
        /// <returns><code>this</code></returns>
        public AbstractOpenKitBuilder WithBeaconCacheMaxRecordAge(long maxBeaconAgeInMilliseconds)
        {
            this.beaconCacheMaxBeaconAge = maxBeaconAgeInMilliseconds;
            return this;
        }

        /// <summary>
        /// Sets the lower memory boundary of the beacon cache.
        /// 
        /// When this is set to a positive value the memory based eviction strategy clears the collected data,
        /// until the data size in the cache falls below the configured limit.
        /// 
        /// </summary>
        /// <param name="lowerMemoryBoundary">The lower boundary of the beacon cache or negative if unlimited.</param>
        /// <returns><code>this</code></returns>
        public AbstractOpenKitBuilder WithBeaconCacheLowerMemoryBoundary(long lowerMemoryBoundary)
        {
            this.beaconCacheLowerMemoryBoundary = lowerMemoryBoundary;
            return this;
        }

        /// <summary>
        /// Sets the upper memory boundary of the beacon cache.
        /// 
        /// When this is set to a positive value the memory based eviction strategy starts to clear
        /// data from the beacon cache when the cache size exceeds this setting.
        /// 
        /// </summary>
        /// <param name="upperMemoryBoundary">The upper boundary of the beacon cache or negative if unlimited.</param>
        /// <returns><code>this</code></returns>
        public AbstractOpenKitBuilder WithBeaconCacheUpperMemoryBoundary(long upperMemoryBoundary)
        {
            this.beaconCacheUpperMemoryBoundary = upperMemoryBoundary;
            return this;
        }

        /// <summary>
        /// Builds a new <code>IOpenKit</code> instance
        /// </summary>
        /// <returns></returns>
        public IOpenKit Build()
        {
            var openKit = new Core.OpenKit(Logger, BuildConfiguration());
            openKit.Initialize();

            return openKit;
        }

        /// <summary>
        /// Builds the configuration for the OpenKit instance
        /// </summary>
        /// <returns></returns>
        internal abstract OpenKitConfiguration BuildConfiguration();
    }
}
