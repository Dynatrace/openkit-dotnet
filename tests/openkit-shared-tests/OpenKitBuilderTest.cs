using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol.SSL;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit
{
    public class OpenKitBuilderTest
    {
        private const string endpoint = "https://localhost:12345";
        private const string appID = "asdf123";
        private const string appName = "myName";
        private const long deviceID = 1234L;
        private const string appVersion = "1.2.3.4";
        private const string os = "custom OS";
        private const string manufacturer = "custom manufacturer";
        private const string modelID = "custom model id";

        [Test]
        public void DefaultsAreSetForAppMon()
        {
            VerifyDefaultsAreSet(new AppMonOpenKitBuilder(endpoint, appID, deviceID).BuildConfiguration());
        }

        [Test]
        public void DefaultsAreSetForDynatrace()
        {
            VerifyDefaultsAreSet(new DynatraceOpenKitBuilder(endpoint, appName, deviceID).BuildConfiguration());
        }

        private void VerifyDefaultsAreSet(OpenKitConfiguration configuration)
        {
            // default values
            Assert.That(configuration.ApplicationVersion, Is.EqualTo(OpenKitConstants.DEFAULT_APPLICATION_VERSION));
            Assert.That(configuration.Device.Manufacturer, Is.EqualTo(OpenKitConstants.DEFAULT_MANUFACTURER));
            Assert.That(configuration.Device.OperatingSystem, Is.EqualTo(OpenKitConstants.DEFAULT_OPERATING_SYSTEM));
            Assert.That(configuration.Device.ModelID, Is.EqualTo(OpenKitConstants.DEFAULT_MODEL_ID));

            // default trust manager
            Assert.That(configuration.HTTPClientConfig.SSLTrustManager, Is.InstanceOf<SSLStrictTrustManager>());

            // default values for beacon cache configuration
            Assert.That(configuration.BeaconCacheConfig, Is.Not.Null);
            Assert.That(configuration.BeaconCacheConfig.MaxRecordAge, Is.EqualTo(BeaconCacheConfiguration.DEFAULT_MAX_RECORD_AGE_IN_MILLIS));
            Assert.That(configuration.BeaconCacheConfig.CacheSizeUpperBound, Is.EqualTo(BeaconCacheConfiguration.DEFAULT_UPPER_MEMORY_BOUNDARY_IN_BYTES));
            Assert.That(configuration.BeaconCacheConfig.CacheSizeLowerBound, Is.EqualTo(BeaconCacheConfiguration.DEFAULT_LOWER_MEMORY_BOUNDARY_IN_BYTES));
        }

        [Test]
        public void ApplicationNameIsSetCorrectlyForAppMon()
        {
            var target = new AppMonOpenKitBuilder(endpoint, appName, deviceID).BuildConfiguration();

            Assert.That(target.ApplicationName, Is.EqualTo(appName));
            Assert.That(target.ApplicationName, Is.EqualTo(target.ApplicationID));
        }

        [Test]
        public void CanOverrideTrustManagerForAppMon()
        {
            // given
            var trustManager = Substitute.For<ISSLTrustManager>();

            // when
            var target = new AppMonOpenKitBuilder(endpoint, appName, deviceID)
                .WithTrustManager(trustManager)
                .BuildConfiguration();

            // then
            Assert.That(target.HTTPClientConfig.SSLTrustManager, Is.SameAs(trustManager));
        }

        [Test]
        public void CanOverrideTrustManagerForDynatrace()
        {
            // given 
            var trustManager = Substitute.For<ISSLTrustManager>();

            // when
            var target = new DynatraceOpenKitBuilder(endpoint, appID, deviceID)
                    .WithTrustManager(trustManager)
                    .BuildConfiguration();

            // then
            Assert.That(target.HTTPClientConfig.SSLTrustManager, Is.SameAs(trustManager));
        }

        [Test]
        public void CanSetApplicationVersionForAppMon()
        {
            // when
            var target = new AppMonOpenKitBuilder(endpoint, appName, deviceID)
                .WithApplicationVersion(appVersion)
                .BuildConfiguration();

            // then
            Assert.That(target.ApplicationVersion, Is.EqualTo(appVersion));
        }

        [Test]
        public void CanSetApplicationVersionForDynatrace()
        {
            // when
            var target = new DynatraceOpenKitBuilder(endpoint, appID, deviceID)
                .WithApplicationVersion(appVersion)
                .BuildConfiguration();

            // then
            Assert.That(target.ApplicationVersion, Is.EqualTo(appVersion));
        }

        [Test]
        public void CanSetOperatingSystemForAppMon()
        {
            // when
            var target = new AppMonOpenKitBuilder(endpoint, appName, deviceID)
                .WithOperatingSystem(os)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.OperatingSystem, Is.EqualTo(os));
        }

        [Test]
        public void CanSetOperatingSystemForDynatrace()
        {
            // when
            var target = new DynatraceOpenKitBuilder(endpoint, appID, deviceID)
                .WithOperatingSystem(os)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.OperatingSystem, Is.EqualTo(os));
        }

        [Test]
        public void CanSetManufacturerForAppMon()
        {
            // when
            var target = new AppMonOpenKitBuilder(endpoint, appName, deviceID)
                .WithManufacturer(manufacturer)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.Manufacturer, Is.EqualTo(manufacturer));
        }

        [Test]
        public void CanSetManufactureForDynatrace()
        {
            // when
            var target = new DynatraceOpenKitBuilder(endpoint, appID, deviceID)
                .WithManufacturer(manufacturer)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.Manufacturer, Is.EqualTo(manufacturer));
        }

        [Test]
        public void CanSetModelIDForAppMon()
        {
            // when
            var target = new AppMonOpenKitBuilder(endpoint, appName, deviceID)
                .WithModelID(modelID)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.ModelID, Is.EqualTo(modelID));
        }

        [Test]
        public void CanSetModelIDForDynatrace()
        {
            // when
            var target = new DynatraceOpenKitBuilder(endpoint, appID, deviceID)
                .WithModelID(modelID)
                .BuildConfiguration();

            // then
            Assert.That(target.Device.ModelID, Is.EqualTo(modelID));
        }

        [Test]
        public void CanSetAppNameForDynatrace()
        {
            // when
            var target = new DynatraceOpenKitBuilder(endpoint, appID, deviceID)
                .WithApplicationName(appName)
                .BuildConfiguration();

            // then
            Assert.That(target.ApplicationName, Is.EqualTo(appName));
        }

        [Test]
        public void CanSetLogger() {
            // given
            var logger = Substitute.For<ILogger>();

            // when
            var target = (TestOpenKitBuilder) new TestOpenKitBuilder()
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
            DynatraceOpenKitBuilder target = new DynatraceOpenKitBuilder(endpoint, appID, deviceID);
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
            var target = new AppMonOpenKitBuilder(endpoint, appID, deviceID);
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
            var target = new DynatraceOpenKitBuilder(endpoint, appID, deviceID);
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
            var target = new AppMonOpenKitBuilder(endpoint, appID, deviceID);
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
            var target = new DynatraceOpenKitBuilder(endpoint, appID, deviceID);
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
            var target = new AppMonOpenKitBuilder(endpoint, appID, deviceID);
            const long upperMemoryBoundary = 42L * 1024L;

            // when
            var obtained = target.WithBeaconCacheUpperMemoryBoundary(upperMemoryBoundary);
            var config = target.BuildConfiguration().BeaconCacheConfig;

            // then
            Assert.That(obtained, Is.InstanceOf<AppMonOpenKitBuilder>());
            Assert.That((AppMonOpenKitBuilder)obtained, Is.SameAs(target));
            Assert.That(config.CacheSizeUpperBound, Is.EqualTo(upperMemoryBoundary));
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
