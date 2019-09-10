﻿//
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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Util;
using Dynatrace.OpenKit.Protocol.SSL;
using NSubstitute;
using NUnit.Framework;
using Dynatrace.OpenKit.Util;

namespace Dynatrace.OpenKit
{
    public class OpenKitBuilderTest
    {
        private const string Endpoint = "https://localhost:12345";
        private const string AppID = "asdf123";
        private const string AppName = "myName";
        private const long DeviceID = 1234L;
        private const string AppVersion = "1.2.3.4";
        private const string OS = "custom OS";
        private const string Manufacturer = "custom manufacturer";
        private const string ModelID = "custom model id";

        [Test]
        public void DefaultsAreSetForAppMon()
        {
            // when
            var obtained = new AppMonOpenKitBuilder(Endpoint, AppName, DeviceID).BuildConfiguration();

            // then
            Assert.That(obtained.EndpointUrl, Is.EqualTo(Endpoint));
            Assert.That(obtained.DeviceId, Is.EqualTo(1234));
            Assert.That(obtained.ApplicationName, Is.EqualTo(AppName));
            Assert.That(obtained.ApplicationId, Is.EqualTo(AppName));

            // ensure remaining defaults
            VerifyDefaultsAreSet(obtained);
        }

        [Test]
        public void AppMonOpenKitBuilderTakesStringDeviceID()
        {
            // given
            var deviceIdAsString = "stringDeviceID";
            var target = new AppMonOpenKitBuilder(Endpoint, AppName, deviceIdAsString);

            // when, then
            var hashedDeviceId = StringUtil.To64BitHash(deviceIdAsString);
            Assert.That(target.BuildConfiguration().DeviceId, Is.EqualTo(hashedDeviceId));
        }

        [Test]
        public void AppMonOpenKitBuilderTakesOverNumericDeviceIdString()
        {
            // given
            var deviceId = 42;
            AbstractOpenKitBuilder target = new AppMonOpenKitBuilder(Endpoint, AppName, deviceId.ToString());

            // when, then
            Assert.That(target.BuildConfiguration().DeviceId, Is.EqualTo(deviceId));
        }

        [Test]
        public void AppMonOpenKitBuilderTrimsDeviceIdString()
        {
            // given
            var deviceIdString = " 42 ";
            AbstractOpenKitBuilder target = new AppMonOpenKitBuilder(Endpoint, AppID, deviceIdString);

            // when, then
            Assert.That(target.BuildConfiguration().DeviceId, Is.EqualTo(42L));
        }

        [Test]
        public void AppMonOpenKitBuilderTakesNumericDeviceId()
        {
            // given
            var deviceId = 42;
            AbstractOpenKitBuilder target = new AppMonOpenKitBuilder(Endpoint, AppID, deviceId);

            // when, then
            Assert.That(target.BuildConfiguration().DeviceId, Is.EqualTo(deviceId));
        }

        [Test]
        public void DefaultsAreSetForDynatrace()
        {
            // when
            var obtained = new DynatraceOpenKitBuilder(Endpoint, AppID, DeviceID).BuildConfiguration();

            // then
            Assert.That(obtained.EndpointUrl, Is.EqualTo(Endpoint));
            Assert.That(obtained.DeviceId, Is.EqualTo(1234));
            Assert.That(obtained.ApplicationName, Is.EqualTo(string.Empty));
            Assert.That(obtained.ApplicationId, Is.EqualTo(AppID));

            // ensure remaining defaults
            VerifyDefaultsAreSet(obtained);
        }

        [Test]
        public void DynatraceOpenKitBuilderTakesStringDeviceID()
        {
            // given
            var deviceIdAsString = "stringDeviceID";
            var target = new DynatraceOpenKitBuilder(Endpoint, AppID, deviceIdAsString);

            // when, then
            var hashedDeviceId = StringUtil.To64BitHash(deviceIdAsString);
            Assert.That(target.BuildConfiguration().DeviceId, Is.EqualTo(hashedDeviceId));
        }

        [Test]
        public void DynatraceOpenKitBuilderTakesOverNumericDeviceIdString()
        {
            // given
            var deviceId = 42;
            var target = new DynatraceOpenKitBuilder(Endpoint, AppID, deviceId.ToString());

            // when, then
            Assert.That(target.BuildConfiguration().DeviceId, Is.EqualTo(deviceId));
        }

        [Test]
        public void DynatraceOpenKitBuilderTrimsDeviceIdString()
        {
            // given
            var deviceIdString = " 42 ";
            AbstractOpenKitBuilder target = new DynatraceOpenKitBuilder(Endpoint, AppID, deviceIdString);

            // when, then
            Assert.That(target.BuildConfiguration().DeviceId, Is.EqualTo(42L));
        }

        [Test]
        public void DynatraceOpenKitBuilderTakesNumericDeviceId()
        {
            // given
            var deviceId = 42;
            AbstractOpenKitBuilder target = new DynatraceOpenKitBuilder(Endpoint, AppID, deviceId);

            // when, then
            Assert.That(target.BuildConfiguration().DeviceId, Is.EqualTo(deviceId));
        }

        private void VerifyDefaultsAreSet(OpenKitConfiguration configuration)
        {
            // default values
            Assert.That(configuration.ApplicationVersion, Is.EqualTo(OpenKitConstants.DefaultApplicationVersion));
            Assert.That(configuration.Device.Manufacturer, Is.EqualTo(OpenKitConstants.DefaultManufacturer));
            Assert.That(configuration.Device.OperatingSystem, Is.EqualTo(OpenKitConstants.DefaultOperatingSystem));
            Assert.That(configuration.Device.ModelId, Is.EqualTo(OpenKitConstants.DefaultModelId));

            // default trust manager
            Assert.That(configuration.HttpClientConfig.SslTrustManager, Is.InstanceOf<SSLStrictTrustManager>());

            // default values for beacon cache configuration
            Assert.That(configuration.BeaconCacheConfig, Is.Not.Null);
            Assert.That(configuration.BeaconCacheConfig.MaxRecordAge, Is.EqualTo(BeaconCacheConfiguration.DEFAULT_MAX_RECORD_AGE_IN_MILLIS));
            Assert.That(configuration.BeaconCacheConfig.CacheSizeUpperBound, Is.EqualTo(BeaconCacheConfiguration.DEFAULT_UPPER_MEMORY_BOUNDARY_IN_BYTES));
            Assert.That(configuration.BeaconCacheConfig.CacheSizeLowerBound, Is.EqualTo(BeaconCacheConfiguration.DEFAULT_LOWER_MEMORY_BOUNDARY_IN_BYTES));
            Assert.That(configuration.BeaconConfig.DataCollectionLevel, Is.EqualTo(BeaconConfiguration.DEFAULT_DATA_COLLECTION_LEVEL));
            Assert.That(configuration.BeaconConfig.CrashReportingLevel, Is.EqualTo(BeaconConfiguration.DEFAULT_CRASH_REPORTING_LEVEL));
        }

        [Test]
        public void ApplicationNameIsSetCorrectlyForAppMon()
        {
            var target = new AppMonOpenKitBuilder(Endpoint, AppName, DeviceID).BuildConfiguration();

            Assert.That(target.ApplicationName, Is.EqualTo(AppName));
            Assert.That(target.ApplicationName, Is.EqualTo(target.ApplicationId));
        }

        [Test]
        public void CanOverrideTrustManagerForAppMon()
        {
            // given
            var trustManager = Substitute.For<ISSLTrustManager>();

            // when
            var target = new AppMonOpenKitBuilder(Endpoint, AppName, DeviceID)
                .WithTrustManager(trustManager)
                .BuildConfiguration();

            // then
            Assert.That(target.HttpClientConfig.SslTrustManager, Is.SameAs(trustManager));
        }

        [Test]
        public void CannotSetNullTrustManagerForAppMon()
        {
            // when
            var target = new AppMonOpenKitBuilder(Endpoint, AppName, DeviceID)
                .WithTrustManager(null)
                .BuildConfiguration();

            // then
            Assert.That(target.HttpClientConfig.SslTrustManager, Is.InstanceOf<SSLStrictTrustManager>());
        }

        [Test]
        public void CanOverrideTrustManagerForDynatrace()
        {
            // given 
            var trustManager = Substitute.For<ISSLTrustManager>();

            // when
            var target = new DynatraceOpenKitBuilder(Endpoint, AppID, DeviceID)
                    .WithTrustManager(trustManager)
                    .BuildConfiguration();

            // then
            Assert.That(target.HttpClientConfig.SslTrustManager, Is.SameAs(trustManager));
        }

        [Test]
        public void CannotSetNullTrustManagerForDynatrace()
        {
            // when
            var target = new DynatraceOpenKitBuilder(Endpoint, AppID, DeviceID)
                .WithTrustManager(null)
                .BuildConfiguration();

            // then
            Assert.That(target.HttpClientConfig.SslTrustManager, Is.InstanceOf<SSLStrictTrustManager>());
        }

        [Test]
        public void CanSetApplicationVersionForAppMon()
        {
            // when
            var target = new AppMonOpenKitBuilder(Endpoint, AppName, DeviceID)
                .WithApplicationVersion(AppVersion)
                .BuildConfiguration();

            // then
            Assert.That(target.ApplicationVersion, Is.EqualTo(AppVersion));
        }

        [Test]
        public void CanSetApplicationVersionForDynatrace()
        {
            // when
            var target = new DynatraceOpenKitBuilder(Endpoint, AppID, DeviceID)
                .WithApplicationVersion(AppVersion)
                .BuildConfiguration();

            // then
            Assert.That(target.ApplicationVersion, Is.EqualTo(AppVersion));
        }

        [Test]
        public void CanSetOperatingSystemForAppMon()
        {
            // when
            var target = new AppMonOpenKitBuilder(Endpoint, AppName, DeviceID)
                .WithOperatingSystem(OS)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.OperatingSystem, Is.EqualTo(OS));
        }

        [Test]
        public void CanSetOperatingSystemForDynatrace()
        {
            // when
            var target = new DynatraceOpenKitBuilder(Endpoint, AppID, DeviceID)
                .WithOperatingSystem(OS)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.OperatingSystem, Is.EqualTo(OS));
        }

        [Test]
        public void CanSetManufacturerForAppMon()
        {
            // when
            var target = new AppMonOpenKitBuilder(Endpoint, AppName, DeviceID)
                .WithManufacturer(Manufacturer)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.Manufacturer, Is.EqualTo(Manufacturer));
        }

        [Test]
        public void CanSetManufactureForDynatrace()
        {
            // when
            var target = new DynatraceOpenKitBuilder(Endpoint, AppID, DeviceID)
                .WithManufacturer(Manufacturer)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.Manufacturer, Is.EqualTo(Manufacturer));
        }

        [Test]
        public void CanSetModelIDForAppMon()
        {
            // when
            var target = new AppMonOpenKitBuilder(Endpoint, AppName, DeviceID)
                .WithModelId(ModelID)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.ModelId, Is.EqualTo(ModelID));
        }

        [Test]
        public void CanSetModelIDForDynatrace()
        {
            // when
            var target = new DynatraceOpenKitBuilder(Endpoint, AppID, DeviceID)
                .WithModelId(ModelID)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.ModelId, Is.EqualTo(ModelID));
        }

        [Test]
        public void CanSetAppNameForDynatrace()
        {
            // when
            var target = new DynatraceOpenKitBuilder(Endpoint, AppID, DeviceID)
                .WithApplicationName(AppName)
                .BuildConfiguration();

            // then
            Assert.That(target.ApplicationName, Is.EqualTo(AppName));
        }

        [Test]
        public void CanSetLogger()
        {
            // given
            var logger = Substitute.For<ILogger>();

            // when
            var target = (TestOpenKitBuilder)new TestOpenKitBuilder()
                .WithLogger(logger);

            // then
            Assert.That(target.GetLogger(), Is.SameAs(logger));
        }

        [Test]
        public void DefaultLoggerIsUsedByDefault()
        {
            // when
            var target = new TestOpenKitBuilder().GetLogger();

            // then
            Assert.That(target, Is.InstanceOf<DefaultLogger>());
            Assert.That(target.IsDebugEnabled, Is.False);
            Assert.That(target.IsInfoEnabled, Is.False);
        }

        [Test]
        public void VerboseIsUsedInDefaultLogger()
        {
            // when
            var target = ((TestOpenKitBuilder)new TestOpenKitBuilder().EnableVerbose()).GetLogger();

            // then
            Assert.That(target, Is.InstanceOf<DefaultLogger>());
            Assert.That(target.IsDebugEnabled, Is.True);
            Assert.That(target.IsInfoEnabled, Is.True);
        }

        [Test]
        public void CanSetCustomMaxBeaconRecordAgeForDynatrace()
        {
            // given
            DynatraceOpenKitBuilder target = new DynatraceOpenKitBuilder(Endpoint, AppID, DeviceID);
            const long maxRecordAge = 123456L;

            // when
            var obtained = target.WithBeaconCacheMaxRecordAge(maxRecordAge);
            var config = target.BuildConfiguration().BeaconCacheConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<DynatraceOpenKitBuilder>());
            Assert.That((DynatraceOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.MaxRecordAge, Is.EqualTo(maxRecordAge));
        }

        [Test]
        public void CanSetCustomMaxBeaconRecordAgeForAppMon()
        {
            // given
            var target = new AppMonOpenKitBuilder(Endpoint, AppID, DeviceID);
            const long maxRecordAge = 123456L;

            // when
            var obtained = target.WithBeaconCacheMaxRecordAge(maxRecordAge);
            var config = target.BuildConfiguration().BeaconCacheConfig;


            // then
            Assert.That(obtained, Is.InstanceOf<AppMonOpenKitBuilder>());
            Assert.That((AppMonOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.MaxRecordAge, Is.EqualTo(maxRecordAge));
        }

        [Test]
        public void CanSetBeaconCacheLowerMemoryBoundaryForDynatrace()
        {
            // given
            var target = new DynatraceOpenKitBuilder(Endpoint, AppID, DeviceID);
            const long lowerMemoryBoundary = 42L * 1024L;

            // when
            var obtained = target.WithBeaconCacheLowerMemoryBoundary(lowerMemoryBoundary);
            var config = target.BuildConfiguration().BeaconCacheConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<DynatraceOpenKitBuilder>());
            Assert.That((DynatraceOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.CacheSizeLowerBound, Is.EqualTo(lowerMemoryBoundary));
        }

        [Test]
        public void CanSetBeaconCacheLowerMemoryBoundaryForAppMon()
        {
            // given
            var target = new AppMonOpenKitBuilder(Endpoint, AppID, DeviceID);
            const long lowerMemoryBoundary = 42L * 1024L;

            // when
            var obtained = target.WithBeaconCacheLowerMemoryBoundary(lowerMemoryBoundary);
            var config = target.BuildConfiguration().BeaconCacheConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<AppMonOpenKitBuilder>());
            Assert.That((AppMonOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.CacheSizeLowerBound, Is.EqualTo(lowerMemoryBoundary));
        }

        [Test]
        public void CanSetBeaconCacheUpperMemoryBoundaryForDynatrace()
        {
            // given
            var target = new DynatraceOpenKitBuilder(Endpoint, AppID, DeviceID);
            const long upperMemoryBoundary = 42L * 1024L;

            // when
            var obtained = target.WithBeaconCacheUpperMemoryBoundary(upperMemoryBoundary);
            var config = target.BuildConfiguration().BeaconCacheConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<DynatraceOpenKitBuilder>());
            Assert.That((DynatraceOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.CacheSizeUpperBound, Is.EqualTo(upperMemoryBoundary));
        }

        [Test]
        public void CanSetBeaconCacheUpperMemoryBoundaryForAppMon()
        {
            // given
            var target = new AppMonOpenKitBuilder(Endpoint, AppID, DeviceID);
            const long upperMemoryBoundary = 42L * 1024L;

            // when
            var obtained = target.WithBeaconCacheUpperMemoryBoundary(upperMemoryBoundary);
            var config = target.BuildConfiguration().BeaconCacheConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<AppMonOpenKitBuilder>());
            Assert.That((AppMonOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.CacheSizeUpperBound, Is.EqualTo(upperMemoryBoundary));
        }

        [Test]
        public void CanSetDataCollectionLevelForDynatrace()
        {
            // given
            var target = new DynatraceOpenKitBuilder(Endpoint, AppID, DeviceID);

            // when
            var level = DataCollectionLevel.USER_BEHAVIOR;
            var obtained = target.WithDataCollectionLevel(level);
            var config = target.BuildConfiguration().BeaconConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<DynatraceOpenKitBuilder>());
            Assert.That((DynatraceOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.DataCollectionLevel, Is.EqualTo(level));
        }

        [Test]
        public void CanSetDataCollectionLevelForAppMon()
        {
            // given
            var target = new AppMonOpenKitBuilder(Endpoint, AppID, DeviceID);

            // when
            var level = DataCollectionLevel.USER_BEHAVIOR;
            var obtained = target.WithDataCollectionLevel(level);
            var config = target.BuildConfiguration().BeaconConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<AppMonOpenKitBuilder>());
            Assert.That((AppMonOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.DataCollectionLevel, Is.EqualTo(level));
        }

        [Test]
        public void CanSetCrashReportingLevelForDynatrace()
        {
            // given
            var target = new DynatraceOpenKitBuilder(Endpoint, AppID, DeviceID);

            // when
            var level = CrashReportingLevel.OPT_IN_CRASHES;
            var obtained = target.WithCrashReportingLevel(level);
            var config = target.BuildConfiguration().BeaconConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<DynatraceOpenKitBuilder>());
            Assert.That((DynatraceOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.CrashReportingLevel, Is.EqualTo(level));
        }

        [Test]
        public void CanSetCrashReportingLevelForAppMon()
        {
            // given
            var target = new AppMonOpenKitBuilder(Endpoint, AppID, DeviceID);

            // when
            var level = CrashReportingLevel.OPT_IN_CRASHES;
            var obtained = target.WithCrashReportingLevel(level);
            var config = target.BuildConfiguration().BeaconConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<AppMonOpenKitBuilder>());
            Assert.That((AppMonOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.CrashReportingLevel, Is.EqualTo(level));
        }

        private class TestOpenKitBuilder : AbstractOpenKitBuilder
        {
            internal TestOpenKitBuilder() : base("", "0")
            {
            }

            internal override OpenKitConfiguration BuildConfiguration()
            {
                return null;
            }

            public ILogger GetLogger()
            {
                return Logger;
            }
        }
    }
}
