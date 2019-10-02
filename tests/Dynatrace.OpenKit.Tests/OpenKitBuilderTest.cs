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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Util;
using Dynatrace.OpenKit.Protocol.SSL;
using Dynatrace.OpenKit.Util;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit
{
    public class OpenKitBuilderTest
    {
        private const string Endpoint = "https://localhost:12345";
        private const string AppId = "asdf123";
        private const string AppName = "myName";
        private const long DeviceId = 1234L;
        private const string AppVersion = "1.2.3.4";
        private const string OS = "custom OS";
        private const string Manufacturer = "custom manufacturer";
        private const string ModelId = "custom model id";

        [Test]
        public void DefaultsAreSetForAppMon()
        {
            // when
            var obtained = new AppMonOpenKitBuilder(Endpoint, AppName, DeviceId).BuildConfiguration();

            // then
            Assert.That(obtained.EndpointUrl, Is.EqualTo(Endpoint));
            Assert.That(obtained.DeviceId, Is.EqualTo(1234));
            Assert.That(obtained.ApplicationName, Is.EqualTo(AppName));
            Assert.That(obtained.ApplicationId, Is.EqualTo(AppName));

            // ensure remaining defaults
            VerifyDefaultsAreSet(obtained);
        }

        [Test]
        public void AppMonOpenKitBuilderTakesStringDeviceId()
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
            AbstractOpenKitBuilder target = new AppMonOpenKitBuilder(Endpoint, AppId, deviceIdString);

            // when, then
            Assert.That(target.BuildConfiguration().DeviceId, Is.EqualTo(42L));
        }

        [Test]
        public void AppMonOpenKitBuilderTakesNumericDeviceId()
        {
            // given
            var deviceId = 42;
            AbstractOpenKitBuilder target = new AppMonOpenKitBuilder(Endpoint, AppId, deviceId);

            // when, then
            Assert.That(target.BuildConfiguration().DeviceId, Is.EqualTo(deviceId));
        }

        [Test]
        public void DefaultsAreSetForDynatrace()
        {
            // when
            var obtained = new DynatraceOpenKitBuilder(Endpoint, AppId, DeviceId).BuildConfiguration();

            // then
            Assert.That(obtained.EndpointUrl, Is.EqualTo(Endpoint));
            Assert.That(obtained.DeviceId, Is.EqualTo(1234));
            Assert.That(obtained.ApplicationName, Is.EqualTo(string.Empty));
            Assert.That(obtained.ApplicationId, Is.EqualTo(AppId));

            // ensure remaining defaults
            VerifyDefaultsAreSet(obtained);
        }

        [Test]
        public void DynatraceOpenKitBuilderTakesStringDeviceId()
        {
            // given
            var deviceIdAsString = "stringDeviceID";
            var target = new DynatraceOpenKitBuilder(Endpoint, AppId, deviceIdAsString);

            // when, then
            var hashedDeviceId = StringUtil.To64BitHash(deviceIdAsString);
            Assert.That(target.BuildConfiguration().DeviceId, Is.EqualTo(hashedDeviceId));
        }

        [Test]
        public void DynatraceOpenKitBuilderTakesOverNumericDeviceIdString()
        {
            // given
            var deviceId = 42;
            var target = new DynatraceOpenKitBuilder(Endpoint, AppId, deviceId.ToString());

            // when, then
            Assert.That(target.BuildConfiguration().DeviceId, Is.EqualTo(deviceId));
        }

        [Test]
        public void DynatraceOpenKitBuilderTrimsDeviceIdString()
        {
            // given
            var deviceIdString = " 42 ";
            AbstractOpenKitBuilder target = new DynatraceOpenKitBuilder(Endpoint, AppId, deviceIdString);

            // when, then
            Assert.That(target.BuildConfiguration().DeviceId, Is.EqualTo(42L));
        }

        [Test]
        public void DynatraceOpenKitBuilderTakesNumericDeviceId()
        {
            // given
            var deviceId = 42;
            AbstractOpenKitBuilder target = new DynatraceOpenKitBuilder(Endpoint, AppId, deviceId);

            // when, then
            Assert.That(target.BuildConfiguration().DeviceId, Is.EqualTo(deviceId));
        }

        private void VerifyDefaultsAreSet(IOpenKitConfiguration configuration)
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
            Assert.That(configuration.BeaconCacheConfig.MaxRecordAge, Is.EqualTo(BeaconCacheConfiguration.DefaultMaxRecordAgeInMillis));
            Assert.That(configuration.BeaconCacheConfig.CacheSizeUpperBound, Is.EqualTo(BeaconCacheConfiguration.DefaultUpperMemoryBoundaryInBytes));
            Assert.That(configuration.BeaconCacheConfig.CacheSizeLowerBound, Is.EqualTo(BeaconCacheConfiguration.DefaultLowerMemoryBoundaryInBytes));
            Assert.That(configuration.PrivacyConfig.DataCollectionLevel, Is.EqualTo(PrivacyConfiguration.DefaultDataCollectionLevel));
            Assert.That(configuration.PrivacyConfig.CrashReportingLevel, Is.EqualTo(PrivacyConfiguration.DefaultCrashReportingLevel));
        }

        [Test]
        public void ApplicationNameIsSetCorrectlyForAppMon()
        {
            var target = new AppMonOpenKitBuilder(Endpoint, AppName, DeviceId).BuildConfiguration();

            Assert.That(target.ApplicationName, Is.EqualTo(AppName));
            Assert.That(target.ApplicationName, Is.EqualTo(target.ApplicationId));
        }

        [Test]
        public void CanOverrideTrustManagerForAppMon()
        {
            // given
            var trustManager = Substitute.For<ISSLTrustManager>();

            // when
            IOpenKitConfiguration target = new AppMonOpenKitBuilder(Endpoint, AppName, DeviceId)
                .WithTrustManager(trustManager)
                .BuildConfiguration();

            // then
            Assert.That(target.HttpClientConfig.SslTrustManager, Is.SameAs(trustManager));
        }

        [Test]
        public void CannotSetNullTrustManagerForAppMon()
        {
            // when
            IOpenKitConfiguration target = new AppMonOpenKitBuilder(Endpoint, AppName, DeviceId)
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
            IOpenKitConfiguration target = new DynatraceOpenKitBuilder(Endpoint, AppId, DeviceId)
                    .WithTrustManager(trustManager)
                    .BuildConfiguration();

            // then
            Assert.That(target.HttpClientConfig.SslTrustManager, Is.SameAs(trustManager));
        }

        [Test]
        public void CannotSetNullTrustManagerForDynatrace()
        {
            // when
            IOpenKitConfiguration target = new DynatraceOpenKitBuilder(Endpoint, AppId, DeviceId)
                .WithTrustManager(null)
                .BuildConfiguration();

            // then
            Assert.That(target.HttpClientConfig.SslTrustManager, Is.InstanceOf<SSLStrictTrustManager>());
        }

        [Test]
        public void CanSetApplicationVersionForAppMon()
        {
            // when
            var target = new AppMonOpenKitBuilder(Endpoint, AppName, DeviceId)
                .WithApplicationVersion(AppVersion)
                .BuildConfiguration();

            // then
            Assert.That(target.ApplicationVersion, Is.EqualTo(AppVersion));
        }

        [Test]
        public void CanSetApplicationVersionForDynatrace()
        {
            // when
            var target = new DynatraceOpenKitBuilder(Endpoint, AppId, DeviceId)
                .WithApplicationVersion(AppVersion)
                .BuildConfiguration();

            // then
            Assert.That(target.ApplicationVersion, Is.EqualTo(AppVersion));
        }

        [Test]
        public void CanSetOperatingSystemForAppMon()
        {
            // when
            var target = new AppMonOpenKitBuilder(Endpoint, AppName, DeviceId)
                .WithOperatingSystem(OS)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.OperatingSystem, Is.EqualTo(OS));
        }

        [Test]
        public void CanSetOperatingSystemForDynatrace()
        {
            // when
            var target = new DynatraceOpenKitBuilder(Endpoint, AppId, DeviceId)
                .WithOperatingSystem(OS)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.OperatingSystem, Is.EqualTo(OS));
        }

        [Test]
        public void CanSetManufacturerForAppMon()
        {
            // when
            var target = new AppMonOpenKitBuilder(Endpoint, AppName, DeviceId)
                .WithManufacturer(Manufacturer)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.Manufacturer, Is.EqualTo(Manufacturer));
        }

        [Test]
        public void CanSetManufactureForDynatrace()
        {
            // when
            var target = new DynatraceOpenKitBuilder(Endpoint, AppId, DeviceId)
                .WithManufacturer(Manufacturer)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.Manufacturer, Is.EqualTo(Manufacturer));
        }

        [Test]
        public void CanSetModelIdForAppMon()
        {
            // when
            var target = new AppMonOpenKitBuilder(Endpoint, AppName, DeviceId)
                .WithModelId(ModelId)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.ModelId, Is.EqualTo(ModelId));
        }

        [Test]
        public void CanSetModelIdForDynatrace()
        {
            // when
            var target = new DynatraceOpenKitBuilder(Endpoint, AppId, DeviceId)
                .WithModelId(ModelId)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.ModelId, Is.EqualTo(ModelId));
        }

        [Test]
        public void CanSetAppNameForDynatrace()
        {
            // when
            var target = new DynatraceOpenKitBuilder(Endpoint, AppId, DeviceId)
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
            var target = new DynatraceOpenKitBuilder(Endpoint, AppId, DeviceId);
            const long maxRecordAge = 123456L;

            // when
            var obtained = target.WithBeaconCacheMaxRecordAge(maxRecordAge);
            IOpenKitConfiguration openKitConfig = target.BuildConfiguration();
            var config = openKitConfig.BeaconCacheConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<DynatraceOpenKitBuilder>());
            Assert.That((DynatraceOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.MaxRecordAge, Is.EqualTo(maxRecordAge));
        }

        [Test]
        public void CanSetCustomMaxBeaconRecordAgeForAppMon()
        {
            // given
            var target = new AppMonOpenKitBuilder(Endpoint, AppId, DeviceId);
            const long maxRecordAge = 123456L;

            // when
            var obtained = target.WithBeaconCacheMaxRecordAge(maxRecordAge);
            IOpenKitConfiguration openKitConfig = target.BuildConfiguration();
            var config = openKitConfig.BeaconCacheConfig;


            // then
            Assert.That(obtained, Is.InstanceOf<AppMonOpenKitBuilder>());
            Assert.That((AppMonOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.MaxRecordAge, Is.EqualTo(maxRecordAge));
        }

        [Test]
        public void CanSetBeaconCacheLowerMemoryBoundaryForDynatrace()
        {
            // given
            var target = new DynatraceOpenKitBuilder(Endpoint, AppId, DeviceId);
            const long lowerMemoryBoundary = 42L * 1024L;

            // when
            var obtained = target.WithBeaconCacheLowerMemoryBoundary(lowerMemoryBoundary);
            IOpenKitConfiguration openKitConfig = target.BuildConfiguration();
            var config = openKitConfig.BeaconCacheConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<DynatraceOpenKitBuilder>());
            Assert.That((DynatraceOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.CacheSizeLowerBound, Is.EqualTo(lowerMemoryBoundary));
        }

        [Test]
        public void CanSetBeaconCacheLowerMemoryBoundaryForAppMon()
        {
            // given
            var target = new AppMonOpenKitBuilder(Endpoint, AppId, DeviceId);
            const long lowerMemoryBoundary = 42L * 1024L;

            // when
            var obtained = target.WithBeaconCacheLowerMemoryBoundary(lowerMemoryBoundary);
            IOpenKitConfiguration openKitConfig = target.BuildConfiguration();
            var config = openKitConfig.BeaconCacheConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<AppMonOpenKitBuilder>());
            Assert.That((AppMonOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.CacheSizeLowerBound, Is.EqualTo(lowerMemoryBoundary));
        }

        [Test]
        public void CanSetBeaconCacheUpperMemoryBoundaryForDynatrace()
        {
            // given
            var target = new DynatraceOpenKitBuilder(Endpoint, AppId, DeviceId);
            const long upperMemoryBoundary = 42L * 1024L;

            // when
            var obtained = target.WithBeaconCacheUpperMemoryBoundary(upperMemoryBoundary);
            IOpenKitConfiguration openKitConfig = target.BuildConfiguration();
            var config = openKitConfig.BeaconCacheConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<DynatraceOpenKitBuilder>());
            Assert.That((DynatraceOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.CacheSizeUpperBound, Is.EqualTo(upperMemoryBoundary));
        }

        [Test]
        public void CanSetBeaconCacheUpperMemoryBoundaryForAppMon()
        {
            // given
            var target = new AppMonOpenKitBuilder(Endpoint, AppId, DeviceId);
            const long upperMemoryBoundary = 42L * 1024L;

            // when
            var obtained = target.WithBeaconCacheUpperMemoryBoundary(upperMemoryBoundary);
            IOpenKitConfiguration openKitConfig = target.BuildConfiguration();
            var config = openKitConfig.BeaconCacheConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<AppMonOpenKitBuilder>());
            Assert.That((AppMonOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.CacheSizeUpperBound, Is.EqualTo(upperMemoryBoundary));
        }

        [Test]
        public void CanSetDataCollectionLevelForDynatrace()
        {
            // given
            var target = new DynatraceOpenKitBuilder(Endpoint, AppId, DeviceId);

            // when
            var level = DataCollectionLevel.USER_BEHAVIOR;
            var obtained = target.WithDataCollectionLevel(level);
            IOpenKitConfiguration openKitConfig = target.BuildConfiguration();
            var config = openKitConfig.PrivacyConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<DynatraceOpenKitBuilder>());
            Assert.That((DynatraceOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.DataCollectionLevel, Is.EqualTo(level));
        }

        [Test]
        public void CanSetDataCollectionLevelForAppMon()
        {
            // given
            var target = new AppMonOpenKitBuilder(Endpoint, AppId, DeviceId);

            // when
            var level = DataCollectionLevel.USER_BEHAVIOR;
            var obtained = target.WithDataCollectionLevel(level);
            IOpenKitConfiguration openKitConfig = target.BuildConfiguration();
            var config = openKitConfig.PrivacyConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<AppMonOpenKitBuilder>());
            Assert.That((AppMonOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.DataCollectionLevel, Is.EqualTo(level));
        }

        [Test]
        public void CanSetCrashReportingLevelForDynatrace()
        {
            // given
            var target = new DynatraceOpenKitBuilder(Endpoint, AppId, DeviceId);

            // when
            var level = CrashReportingLevel.OPT_IN_CRASHES;
            var obtained = target.WithCrashReportingLevel(level);
            IOpenKitConfiguration openKitConfig = target.BuildConfiguration();
            var config = openKitConfig.PrivacyConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<DynatraceOpenKitBuilder>());
            Assert.That((DynatraceOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.CrashReportingLevel, Is.EqualTo(level));
        }

        [Test]
        public void CanSetCrashReportingLevelForAppMon()
        {
            // given
            var target = new AppMonOpenKitBuilder(Endpoint, AppId, DeviceId);

            // when
            var level = CrashReportingLevel.OPT_IN_CRASHES;
            var obtained = target.WithCrashReportingLevel(level);
            IOpenKitConfiguration openKitConfig = target.BuildConfiguration();
            var config = openKitConfig.PrivacyConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<AppMonOpenKitBuilder>());
            Assert.That((AppMonOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.CrashReportingLevel, Is.EqualTo(level));
        }

        private class TestOpenKitBuilder : AbstractOpenKitBuilder
        {
            internal TestOpenKitBuilder() : base("", 0)
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
