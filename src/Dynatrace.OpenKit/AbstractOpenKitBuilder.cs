//
// Copyright 2018-2019 Dynatrace LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Globalization;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Util;
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
        private LogLevel logLevel = LogLevel.WARN;
        private string operatingSystem = OpenKitConstants.DefaultOperatingSystem;
        private string manufacturer = OpenKitConstants.DefaultManufacturer;
        private string modelId = OpenKitConstants.DefaultModelId;
        private string applicationVersion = OpenKitConstants.DefaultApplicationVersion;

        protected AbstractOpenKitBuilder(string endpointUrl, long deviceId)
            : this(endpointUrl, deviceId, deviceId.ToString(CultureInfo.InvariantCulture))
        {
        }

        [Obsolete("use AbstractOpenKitBuilder(string, long) instead")]
        protected AbstractOpenKitBuilder(string endpointUrl, string deviceId)
            : this(endpointUrl, DeviceIdFromString(deviceId), deviceId)
        {
        }

        private AbstractOpenKitBuilder(string endpointUrl, long deviceId, string origDeviceId)
        {
            EndpointUrl = endpointUrl;
            DeviceId = deviceId;
            OrigDeviceId = origDeviceId;
        }

        private static long DeviceIdFromString(string deviceId)
        {
            deviceId = deviceId?.Trim() ?? string.Empty;

            try
            {
                return long.Parse(deviceId, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return StringUtil.To64BitHash(deviceId);
            }
        }

        protected string OperatingSystem => operatingSystem;
        protected string Manufacturer => manufacturer;
        protected string ModelID => modelId;
        protected string ApplicationVersion => applicationVersion;
        protected ISSLTrustManager TrustManager { get; private set; } = new SSLStrictTrustManager();

        protected ILogger Logger => logger ?? new DefaultLogger(logLevel);
        protected string EndpointUrl { get; }
        protected long DeviceId { get; }
        protected string OrigDeviceId { get; }

        protected long BeaconCacheMaxBeaconAge { get; private set; } =
            BeaconCacheConfiguration.DefaultMaxRecordAgeInMillis;

        protected long BeaconCacheLowerMemoryBoundary { get; private set; } =
            BeaconCacheConfiguration.DefaultLowerMemoryBoundaryInBytes;

        protected long BeaconCacheUpperMemoryBoundary { get; private set; } =
            BeaconCacheConfiguration.DefaultUpperMemoryBoundaryInBytes;

        protected DataCollectionLevel DataCollectionLevel { get; private set; } =
            PrivacyConfiguration.DefaultDataCollectionLevel;

        protected CrashReportingLevel CrashReportingLevel { get; private set; } =
            PrivacyConfiguration.DefaultCrashReportingLevel;

        /// <summary>
        /// Enables verbose mode. Verbose mode is only enabled if the the default logger is used.
        /// If a custom logger is provided by calling <code>WithLogger</code> debug and info log output
        /// depends on the values returned by <code>IsDebugEnabled</code> and <code>IsInfoEnabled</code>.
        /// </summary>
        /// <remarks>
        /// With the introduction <see cref="LogLevel"/> prefer the <see cref="WithLogLevel(LogLevel)"/> method.
        /// </remarks>
        /// <returns><code>this</code></returns>
        [Obsolete("EnableVerbose is deprecated, use WithLogLevel instead.")]
        public AbstractOpenKitBuilder EnableVerbose()
        {
            return WithLogLevel(LogLevel.DEBUG);
        }

        /// <summary>
        /// Sets the default log level if the default logger is used.
        /// If a custom logger is provided by calling <see cref="WithLogger(ILogger)"/>, debug and info log output
        /// depends on the values returned by <see cref="ILogger.IsDebugEnabled"/> and <see cref="ILogger.IsInfoEnabled"/>.
        /// </summary>
        /// <param name="logLevel">The logLevel for the custom logger</param>
        /// <returns><code>this</code></returns>
        public AbstractOpenKitBuilder WithLogLevel(LogLevel logLevel)
        {
            this.logLevel = logLevel;
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
        /// <remarks>
        /// The value is only set, if it is not <code>null</code>.
        /// </remarks>
        /// <param name="trustManager">trust manager implementation</param>
        /// <returns><code>this</code></returns>
        public AbstractOpenKitBuilder WithTrustManager(ISSLTrustManager trustManager)
        {
            if (trustManager != null)
            {
                this.TrustManager = trustManager;
            }
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
        /// <param name="modelId">the model id</param>
        /// <returns><code>this</code></returns>
        public AbstractOpenKitBuilder WithModelId(string modelId)
        {
            if (!string.IsNullOrEmpty(modelId))
            {
                this.modelId = modelId;
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
            BeaconCacheMaxBeaconAge = maxBeaconAgeInMilliseconds;
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
            BeaconCacheLowerMemoryBoundary = lowerMemoryBoundary;
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
            BeaconCacheUpperMemoryBoundary = upperMemoryBoundary;
            return this;
        }

        /// <summary>
        /// Set the data collection level
        ///
        ///
        /// <list type="bullet">
        /// <item>
        /// <description><see cref="Dynatrace.OpenKit.DataCollectionLevel.OFF"/> no data collected</description>
        /// </item>
        /// <item>
        /// <description><see cref="Dynatrace.OpenKit.DataCollectionLevel.PERFORMANCE"/> only performance related data is collected</description>
        /// </item>
        /// <item>
        /// <description><see cref="Dynatrace.OpenKit.DataCollectionLevel.USER_BEHAVIOR"/> all available RUM data including performance related data is collected</description>
        /// </item>
        /// </list>
        ///
        /// Default value: <see cref="Dynatrace.OpenKit.DataCollectionLevel.USER_BEHAVIOR"/>
        /// </summary>
        /// <param name="dataCollectionLevel">Data collection level to apply.</param>
        /// <returns><code>this</code></returns>
        public AbstractOpenKitBuilder WithDataCollectionLevel(DataCollectionLevel dataCollectionLevel)
        {
            this.DataCollectionLevel = dataCollectionLevel;
            return this;
        }

        /// <summary>
        /// Set the crash reporting level
        ///
        /// <list type="bullet">
        /// <item>
        /// <description><see cref="Dynatrace.OpenKit.CrashReportingLevel.OFF"/> Crashes are not sent to the server</description>
        /// </item>
        /// <item>
        /// <description><see cref="Dynatrace.OpenKit.CrashReportingLevel.OPT_OUT_CRASHES"/> Crashes are not sent to the server</description>
        /// </item>
        /// <item>
        /// <description><see cref="Dynatrace.OpenKit.CrashReportingLevel.OPT_IN_CRASHES"/> Crashes are sent to the server</description>
        /// </item>
        /// </list>
        ///
        /// Default value: <see cref="Dynatrace.OpenKit.CrashReportingLevel.OPT_IN_CRASHES"/>
        /// </summary>
        /// <param name="crashReportingLevel"></param>
        /// <returns><code>this</code></returns>
        public AbstractOpenKitBuilder WithCrashReportingLevel(CrashReportingLevel crashReportingLevel)
        {
            this.CrashReportingLevel = crashReportingLevel;
            return this;
        }

        /// <summary>
        /// Builds a new <code>IOpenKit</code> instance
        /// </summary>
        /// <returns></returns>
        public IOpenKit Build()
        {
            var openKit = new Core.Objects.OpenKit(Logger, BuildConfiguration());
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
