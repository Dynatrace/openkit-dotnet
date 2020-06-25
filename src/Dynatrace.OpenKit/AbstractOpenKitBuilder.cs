//
// Copyright 2018-2020 Dynatrace LLC
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
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Core.Util;
using Dynatrace.OpenKit.Protocol.SSL;

namespace Dynatrace.OpenKit
{
    /// <summary>
    /// Abstract base class for concrete builder. Using the builder an IOpenKit instance can be created
    /// </summary>
    public abstract class AbstractOpenKitBuilder : IOpenKitBuilder
    {
        // mutable fields
        private ILogger logger;

        #region constructor

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

        #endregion

        /// <summary>
        /// Enables verbose mode. Verbose mode is only enabled if the the default logger is used.
        /// If a custom logger is provided by calling <code>WithLogger</code> debug and info log output
        /// depends on the values returned by <code>IsDebugEnabled</code> and <code>IsInfoEnabled</code>.
        /// </summary>
        /// <remarks>
        /// With the introduction <see cref="LogLevel"/> prefer the
        /// <see cref="WithLogLevel(Dynatrace.OpenKit.API.LogLevel)"/> method.
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
            LogLevel = logLevel;
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
                ApplicationVersion = applicationVersion;
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
                TrustManager = trustManager;
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
                OperatingSystem = operatingSystem;
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
                Manufacturer = manufacturer;
            }
            return this;
        }

        [Obsolete("Use WithModelId instead")]
        public AbstractOpenKitBuilder WithModelID(string modelId)
        {
            return WithModelId(modelId);
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
                ModelId = modelId;
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
            DataCollectionLevel = dataCollectionLevel;
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
            CrashReportingLevel = crashReportingLevel;
            return this;
        }

        /// <summary>
        /// Builds a new <code>IOpenKit</code> instance
        /// </summary>
        /// <returns></returns>
        public IOpenKit Build()
        {
            var initializer = new OpenKitInitializer(this);
            var openKit = new Core.Objects.OpenKit(initializer);
            openKit.Initialize();

            return openKit;
        }

        /// <summary>
        /// Returns the string identifying the OpenKit tpe that gets created by the builder.
        ///
        /// <para>
        ///     The only real purpose is for logging reasons.
        /// </para>
        /// </summary>
        /// <returns>some string identifying the OpenKit's type.</returns>
        public abstract string OpenKitType { get; }

        /// <summary>
        /// Returns the application identifier for which the OpenKit reports data.
        /// </summary>
        public abstract string ApplicationId { get; }

        /// <summary>
        /// Returns the application's name
        ///
        /// <para>
        ///     It depends on the concrete builder whether the application name is configurable or not.
        ///     In any case, the derived classes have to return a string that is neither <code>null</code> nor empty.
        /// </para>
        /// </summary>
        public abstract string ApplicationName { get; }

        /// <summary>
        /// Returns the ID of the server to communicate with.
        ///
        /// <para>
        ///     This might be changed based on the OpenKit type.
        /// </para>
        /// </summary>
        public abstract int DefaultServerId { get; }

        /// <summary>
        /// Returns the application version that has been set with <see cref="WithApplicationVersion"/>.
        ///
        /// <para>
        ///     If no version was set, the <see cref="OpenKitConstants.DefaultApplicationVersion">default version</see>
        ///     is returned.
        /// </para>
        /// </summary>
        public string ApplicationVersion { get; private set; } = OpenKitConstants.DefaultApplicationVersion;

        /// <summary>
        /// Returns the operating system that has been set with <see cref="WithOperatingSystem"/>.
        ///
        /// <para>
        ///     If no operating system was set, the <see cref="OpenKitConstants.DefaultOperatingSystem">default
        ///     operating system</see> is returned.
        /// </para>
        /// </summary>
        public string OperatingSystem { get; private set; } = OpenKitConstants.DefaultOperatingSystem;

        /// <summary>
        /// Returns the manufacturer that has been set with <see cref="WithManufacturer"/>.
        ///
        /// <para>
        ///     If no manufacturer was set, the <see cref="OpenKitConstants.DefaultManufacturer">default manufacturer</see>
        ///     is returned.
        /// </para>
        /// </summary>
        public string Manufacturer { get; private set; } = OpenKitConstants.DefaultManufacturer;

        /// <summary>
        /// Returns the model identifier that has been set with <see cref="WithModelId"/>.
        ///
        /// <para>
        ///     If no model ID was set, the <see cref="OpenKitConstants.DefaultModelId">default model ID</see> is
        ///     returned.
        /// </para>
        /// </summary>
        public string ModelId { get; private set; } = OpenKitConstants.DefaultModelId;

        /// <summary>
        /// Returns the endpoint ULR that has ben set in the constructor.
        ///
        /// <para>
        ///     The endpoint ULR is where the beacon data is sent to.
        /// </para>
        /// </summary>
        public string EndpointUrl { get; }

        /// <summary>
        /// Returns the device identifier that has been set in the constructor.
        ///
        /// <para>
        ///     The device identifier is a unique numeric value that identifies the device or the installation.
        ///     The user of the OpenKit library is responsible of providing a unique value which stays consistent
        ///     per device/installation.
        /// </para>
        /// </summary>
        public long DeviceId { get; }

        /// <summary>
        /// Returns the <see cref="DeviceId"/> as it was passed (not <see cref="StringUtil.To64BitHash">hashed</see>) to
        /// the constructor.
        /// </summary>
        public string OrigDeviceId { get; }

        /// <summary>
        /// Returns the SSL trust manager that has been set with <see cref="WithTrustManager"/>.
        ///
        /// <para>
        ///     The <see cref="ISSLTrustManager"/> implementation is responsible for checking the X509 certificate chain
        ///     and also to reject untrusted/invalid certificates.
        ///     The default implementation rejects every untrusted/invalid (including self-signed) certificate.
        /// </para>
        /// </summary>
        public ISSLTrustManager TrustManager { get; private set; } = new SSLStrictTrustManager();

        /// <summary>
        /// Returns the maximum beacon cache record age that has been set with <see cref="WithBeaconCacheMaxRecordAge"/>.
        ///
        /// <para>
        ///     Is no max age was set, the <see cref="ConfigurationDefaults.DefaultMaxRecordAgeInMillis">default max
        ///     age</see> is returned.
        /// </para>
        /// </summary>
        public long BeaconCacheMaxBeaconAge { get; private set; } =
            ConfigurationDefaults.DefaultMaxRecordAgeInMillis;

        /// <summary>
        /// Returns the beacon cache's lower memory boundary that has been set with
        /// <see cref="WithBeaconCacheLowerMemoryBoundary"/>.
        ///
        /// <para>
        ///     If no lower memory boundary was set, the
        ///     <see cref="ConfigurationDefaults.DefaultLowerMemoryBoundaryInBytes">default lower boundary</see> is
        ///     returned.
        /// </para>
        /// </summary>
        public long BeaconCacheLowerMemoryBoundary { get; private set; } =
            ConfigurationDefaults.DefaultLowerMemoryBoundaryInBytes;

        /// <summary>
        /// Returns the beacon cache's upper memory boundary that has been set with
        /// <see cref="WithBeaconCacheUpperMemoryBoundary"/>.
        ///
        /// <para>
        ///     If no upper memory boundary was set, the
        ///     <see cref="ConfigurationDefaults.DefaultUpperMemoryBoundaryInBytes">default upper boundary</see> is
        ///     returned.
        /// </para>
        /// </summary>
        public long BeaconCacheUpperMemoryBoundary { get; private set; } =
            ConfigurationDefaults.DefaultUpperMemoryBoundaryInBytes;

        /// <summary>
        /// Returns the data collection level that has been set with <see cref="WithDataCollectionLevel"/>.
        ///
        /// <para>
        ///     If no data collection level was set, the <see cref="ConfigurationDefaults.DefaultDataCollectionLevel"/>
        ///     is returned.
        /// </para>
        /// </summary>
        public DataCollectionLevel DataCollectionLevel { get; private set; } =
            ConfigurationDefaults.DefaultDataCollectionLevel;

        /// <summary>
        /// Returns the crash reporting level that has been set with <see cref="WithCrashReportingLevel"/>.
        ///
        /// <para>
        ///     If no crash reporting level was set, the <see cref="ConfigurationDefaults.DefaultCrashReportingLevel"/>
        ///     is returned.
        /// </para>
        /// </summary>
        public CrashReportingLevel CrashReportingLevel { get; private set; } =
            ConfigurationDefaults.DefaultCrashReportingLevel;

        /// <summary>
        /// Returns the log level that has been set with <see cref="WithLogLevel"/>.
        ///
        /// <para>
        ///     If no log level was set, the <see cref="Dynatrace.OpenKit.API.LogLevel.WARN">default log level</see>
        ///     is returned.
        /// </para>
        /// </summary>
        public LogLevel LogLevel { get; private set; } = LogLevel.WARN;

        /// <summary>
        /// Returns the <see cref="ILogger"/> that has been set with <see cref="WithLogger"/>.
        ///
        /// <para>
        ///     If no logger was set, a <see cref="DefaultLogger">default logger</see> instance is returned.
        /// </para>
        /// </summary>
        public ILogger Logger => logger ?? new DefaultLogger(LogLevel);
    }
}
