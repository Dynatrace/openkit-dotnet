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
using NSubstitute;
using NUnit.Framework;
using ILogger = Dynatrace.OpenKit.API.ILogger;

namespace Dynatrace.OpenKit
{
    public class AbstractOpenKitBuilderTest
    {
        private const string EndpointUrl = "https://localhost";
        private const long DeviceId = 777;
        private const string ApplicationVersion = "application-version";
        private const string OperatingSystem = "ultimate-operating-system";
        private const string Manufacturer = "ACME Inc.";
        private const string ModelId = "the-model-identifier";
        private const long MaxRecordAgeInMillis = 42000;
        private const long LowerMemoryBoundaryInBytes = 999;
        private const long UpperMemoryBoundaryInBytes = 9999;

        [Test]
        public void ConstructorInitializesEndpointUrl()
        {
            // given, when
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // then
            Assert.That(target.EndpointUrl, Is.EqualTo(EndpointUrl));
        }

        [Test]
        public void ConstructorInitializesDeviceId()
        {
             // given, when
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // then
            Assert.That(target.DeviceId, Is.EqualTo(DeviceId));
        }

        [Test]
        public void ConstructorInitializesAndHashesStringDeviceId()
        {
            // given
            const string deviceIdAsString = "stringDeviceID";
            var target = new StubOpenKitBuilder(EndpointUrl, deviceIdAsString);

            // when, then
            var hashedDeviceId = StringUtil.To64BitHash(deviceIdAsString);
            Assert.That(target.DeviceId, Is.EqualTo(hashedDeviceId));
            Assert.That(target.OrigDeviceId, Is.EqualTo(deviceIdAsString));
        }

        [Test]
        public void ConstructorInitializesNumericDeviceIdString()
        {
            // given
            const long deviceId = 42;
            var target = new StubOpenKitBuilder(EndpointUrl, deviceId.ToString());

            // when, then
            Assert.That(target.DeviceId, Is.EqualTo(deviceId));
            Assert.That(target.OrigDeviceId, Is.EqualTo(deviceId.ToString()));
        }

        [Test]
        public void ConstructorTrimsDeviceIdString()
        {
            // given
            const string deviceIdString = " 42 ";
            var target = new StubOpenKitBuilder(EndpointUrl, deviceIdString);

            // when, then
            Assert.That(target.DeviceId, Is.EqualTo(42L));
            Assert.That(target.OrigDeviceId, Is.EqualTo(deviceIdString));
        }

        [Test]
        public void LoggerGivesADefaultImplementationIfNoneHasBeenProvided()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            var obtained = target.Logger;

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<DefaultLogger>());
        }

        [Test]
        public void DefaultLoggerHasErrorAndWarningLevelEnabled()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            var obtained = target.Logger;

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsErrorEnabled, Is.True);
            Assert.That(obtained.IsWarnEnabled, Is.True);
            Assert.That(obtained.IsInfoEnabled, Is.False);
            Assert.That(obtained.IsDebugEnabled, Is.False);
        }

        [Test]
        public void WhenEnablingVerboseAllLogLevelsAreEnabledInDefaultLogger()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.EnableVerbose();
            var obtained = target.Logger;

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsErrorEnabled, Is.True);
            Assert.That(obtained.IsWarnEnabled, Is.True);
            Assert.That(obtained.IsInfoEnabled, Is.True);
            Assert.That(obtained.IsDebugEnabled, Is.True);
        }

        [Test]
        public void WithLogLevelAppliesLogLevelToDefaultLogger()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithLogLevel(LogLevel.INFO);
            var obtained = target.Logger;

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsErrorEnabled, Is.True);
            Assert.That(obtained.IsWarnEnabled, Is.True);
            Assert.That(obtained.IsInfoEnabled, Is.True);
            Assert.That(obtained.IsDebugEnabled, Is.False);
        }

        [Test]
        public void LoggerReturnsPreviouslySetLogger()
        {
            // given
            var logger = Substitute.For<ILogger>();
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithLogger(logger);
            var obtained = target.Logger;

            // then
            Assert.That(obtained, Is.SameAs(logger));
        }

        [Test]
        public void ApplicationVersionUsesDefaultIfNotSet()
        {
            // given, when
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // then
            Assert.That(target.ApplicationVersion, Is.EqualTo(OpenKitConstants.DefaultApplicationVersion));
        }

        [Test]
        public void ApplicationVersionCanBeChanged()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithApplicationVersion(ApplicationVersion);

            // then
            Assert.That(target.ApplicationVersion, Is.EqualTo(ApplicationVersion));
        }

        [Test]
        public void ApplicationVersionCannotBeChangedToNull()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithApplicationVersion(null);

            // then
            Assert.That(target.ApplicationVersion, Is.Not.Null);
            Assert.That(target.ApplicationVersion, Is.EqualTo(OpenKitConstants.DefaultApplicationVersion));
        }

        [Test]
        public void ApplicationVersionCannotBeChangedToEmptyString()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithApplicationVersion(string.Empty);

            // then
            Assert.That(target.ApplicationVersion, Is.Not.EqualTo(string.Empty));
            Assert.That(target.ApplicationVersion, Is.EqualTo(OpenKitConstants.DefaultApplicationVersion));
        }

        [Test]
        public void TrustManagerGivesStrictTrustManagerByDefault()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            var obtained = target.TrustManager;

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.InstanceOf<SSLStrictTrustManager>());
        }

        [Test]
        public void TrustManagerGivesPreviouslySetTrustManager()
        {
            // given
            var trustManager = Substitute.For<ISSLTrustManager>();
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithTrustManager(trustManager);
            var obtained = target.TrustManager;

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained, Is.SameAs(trustManager));
        }

        [Test]
        public void TrustManagerCannotBeChangedToNull()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithTrustManager(null);
            var obtained = target.TrustManager;

            // then
            Assert.That(obtained, Is.Not.Null.And.InstanceOf<SSLStrictTrustManager>());
        }

        [Test]
        public void OperatingSystemReturnsADefaultValue()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            var obtained = target.OperatingSystem;

            // then
            Assert.That(obtained, Is.Not.Null.And.Not.EqualTo(string.Empty));
            Assert.That(obtained, Is.EqualTo(OpenKitConstants.DefaultOperatingSystem));
        }

        [Test]
        public void OperatingSystemGivesChangedOperatingSystem()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithOperatingSystem(OperatingSystem);
            var obtained = target.OperatingSystem;

            // then
            Assert.That(obtained, Is.EqualTo(OperatingSystem));
        }

        [Test]
        public void OperatingSystemCannotBeChangedToNull()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithOperatingSystem(null);
            var obtained = target.OperatingSystem;

            // then
            Assert.That(obtained, Is.Not.Null);
        }

        [Test]
        public void OperatingSystemCannotBeChangedToEmptyString()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithOperatingSystem(string.Empty);
            var obtained = target.OperatingSystem;

            // then
            Assert.That(obtained, Is.Not.EqualTo(string.Empty));
        }

        [Test]
        public void ManufacturerReturnsADefaultValue()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            var obtained = target.Manufacturer;

            // then
            Assert.That(obtained, Is.EqualTo(OpenKitConstants.DefaultManufacturer));
        }

        [Test]
        public void ManufacturerGivesChangedManufacturer()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithManufacturer(Manufacturer);
            var obtained = target.Manufacturer;

            // then
            Assert.That(obtained, Is.EqualTo(Manufacturer));
        }

        [Test]
        public void ManufacturerCannotBeChangedToNull()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithManufacturer(null);
            var obtained = target.Manufacturer;

            // then
            Assert.That(obtained, Is.Not.Null);
        }

        [Test]
        public void ManufacturerCannotBeChangedToEmptyString()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithManufacturer(string.Empty);
            var obtained = target.Manufacturer;

            // then
            Assert.That(obtained, Is.Not.EqualTo(string.Empty));
        }

        [Test]
        public void ModelIdReturnsADefaultValue()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            var obtained = target.ModelId;

            // then
            Assert.That(obtained, Is.EqualTo(OpenKitConstants.DefaultModelId));
        }

        [Test]
        public void ModelIdGivesChangedModelId()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithModelId(ModelId);
            var obtained = target.ModelId;

            // then
            Assert.That(obtained, Is.EqualTo(ModelId));
        }

        [Test]
        public void ModelIdCannotBeChangedToNull()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithModelId(null);
            var obtained = target.ModelId;

            // then
            Assert.That(obtained, Is.Not.Null);
        }

        [Test]
        public void ModelIdCannotBeChangedToEmptyString()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithModelId(string.Empty);
            var obtained = target.ModelId;

            // then
            Assert.That(obtained, Is.Not.EqualTo(string.Empty));
        }

        [Test]
        public void BeaconCacheMaxRecordAgeReturnsADefaultValue()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            var obtained = target.BeaconCacheMaxBeaconAge;

            // then
            Assert.That(obtained, Is.EqualTo(ConfigurationDefaults.DefaultMaxRecordAgeInMillis));
        }

        [Test]
        public void BeaconCacheMaxRecordAgeGivesChangedValue()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithBeaconCacheMaxRecordAge(MaxRecordAgeInMillis);
            var obtained = target.BeaconCacheMaxBeaconAge;

            // then
            Assert.That(obtained, Is.EqualTo(MaxRecordAgeInMillis));
        }

        [Test]
        public void BeaconCacheLowerMemoryBoundaryReturnsADefaultValue()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            var obtained = target.BeaconCacheLowerMemoryBoundary;

            // then
            Assert.That(obtained, Is.EqualTo(ConfigurationDefaults.DefaultLowerMemoryBoundaryInBytes));
        }

        [Test]
        public void BeaconCacheLowerMemoryBoundaryGivesChangedValue()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithBeaconCacheLowerMemoryBoundary(LowerMemoryBoundaryInBytes);
            var obtained = target.BeaconCacheLowerMemoryBoundary;

            // then
            Assert.That(obtained, Is.EqualTo(LowerMemoryBoundaryInBytes));
        }

        [Test]
        public void BeaconCacheUpperMemoryBoundaryReturnsADefaultValue()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            var obtained = target.BeaconCacheUpperMemoryBoundary;

            // then
            Assert.That(obtained, Is.EqualTo(ConfigurationDefaults.DefaultUpperMemoryBoundaryInBytes));
        }

        [Test]
        public void BeaconCacheUpperMemoryBoundaryGivesChangedValue()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithBeaconCacheUpperMemoryBoundary(UpperMemoryBoundaryInBytes);
            var obtained = target.BeaconCacheUpperMemoryBoundary;

            // then
            Assert.That(obtained, Is.EqualTo(UpperMemoryBoundaryInBytes));
        }

        [Test]
        public void DefaultDataCollectionLevelIsUserBehavior()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            var obtained = target.DataCollectionLevel;

            // then
            Assert.That(obtained, Is.EqualTo(DataCollectionLevel.USER_BEHAVIOR));
        }

        [Test]
        public void DataCollectionLevelReturnsChangedDataCollectionLevel()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithDataCollectionLevel(DataCollectionLevel.PERFORMANCE);
            var obtained = target.DataCollectionLevel;

            // then
            Assert.That(obtained, Is.EqualTo(DataCollectionLevel.PERFORMANCE));
        }

        [Test]
        public void DefaultCrashReportingLevelIsOptInCrashes()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            var obtained = target.CrashReportingLevel;

            // then
            Assert.That(obtained, Is.EqualTo(CrashReportingLevel.OPT_IN_CRASHES));
        }

        [Test]
        public void CrashReportingLevelReturnsChangedCrashReportingLevel()
        {
            // given
            var target = new StubOpenKitBuilder(EndpointUrl, DeviceId);

            // when
            target.WithCrashReportingLevel(CrashReportingLevel.OPT_OUT_CRASHES);
            var obtained = target.CrashReportingLevel;

            // then
            Assert.That(obtained, Is.EqualTo(CrashReportingLevel.OPT_OUT_CRASHES));
        }

        private sealed class StubOpenKitBuilder : AbstractOpenKitBuilder
        {
            internal StubOpenKitBuilder(string endpointUrl, long deviceId)
                : base(endpointUrl, deviceId)
            {
            }

            internal StubOpenKitBuilder(string endpointUrl, string deviceId)
                : base(endpointUrl, deviceId)
            {
            }

            public override string OpenKitType => null;

            public override string ApplicationId => null;

            public override string ApplicationName => null;

            public override int DefaultServerId => 0;
        }
    }
}