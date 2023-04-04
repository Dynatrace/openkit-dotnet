//
// Copyright 2018-2021 Dynatrace LLC
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
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Core.Util;
using Dynatrace.OpenKit.Providers;
using Dynatrace.OpenKit.Util;
using Dynatrace.OpenKit.Util.Json.Objects;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Protocol
{
    public class BeaconTest
    {
        private const string AppId = "appID";
        private const string AppVersion = "1.0";
        private const int ActionId = 17;
        private const int ServerId = 123;
        private const long DeviceId = 456;
        private const int ThreadId = 1234567;
        private const int SessionId = 73;
        private const int SessionSeqNo = 13;
        private const int Multiplicity = 1;
        private const string Url = "https://www.google.com";

        private IBeaconConfiguration mockBeaconConfiguration;
        private IOpenKitConfiguration mockOpenKitConfiguration;
        private IPrivacyConfiguration mockPrivacyConfiguration;
        private IServerConfiguration mockServerConfiguration;
        private IAdditionalQueryParameters mockAdditionalQueryParameters;

        private ISessionIdProvider mockSessionIdProvider;
        private IThreadIdProvider mockThreadIdProvider;
        private ITimingProvider mockTimingProvider;
        private IOpenKitComposite mockParent;

        private ILogger mockLogger;
        private IBeaconCache mockBeaconCache;
        private ISupplementaryBasicData mockSupplementaryBasicData;

#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
        private CultureInfo currentCulture;
#endif

        [SetUp]
        public void Setup()
        {
            mockOpenKitConfiguration = Substitute.For<IOpenKitConfiguration>();
            mockOpenKitConfiguration.ApplicationId.Returns(AppId);
            mockOpenKitConfiguration.ApplicationIdPercentEncoded.Returns(AppId);
            mockOpenKitConfiguration.ApplicationVersion.Returns(AppVersion);
            mockOpenKitConfiguration.OperatingSystem.Returns(string.Empty);
            mockOpenKitConfiguration.Manufacturer.Returns(string.Empty);
            mockOpenKitConfiguration.ModelId.Returns(string.Empty);
            mockOpenKitConfiguration.DeviceId.Returns(DeviceId);

            mockPrivacyConfiguration = Substitute.For<IPrivacyConfiguration>();
            mockPrivacyConfiguration.DataCollectionLevel.Returns(ConfigurationDefaults.DefaultDataCollectionLevel);
            mockPrivacyConfiguration.CrashReportingLevel.Returns(ConfigurationDefaults.DefaultCrashReportingLevel);
            mockPrivacyConfiguration.IsDeviceIdSendingAllowed.Returns(true);
            mockPrivacyConfiguration.IsSessionReportingAllowed.Returns(true);
            mockPrivacyConfiguration.IsSessionNumberReportingAllowed.Returns(true);
            mockPrivacyConfiguration.IsWebRequestTracingAllowed.Returns(true);
            mockPrivacyConfiguration.IsActionReportingAllowed.Returns(true);
            mockPrivacyConfiguration.IsValueReportingAllowed.Returns(true);
            mockPrivacyConfiguration.IsEventReportingAllowed.Returns(true);
            mockPrivacyConfiguration.IsErrorReportingAllowed.Returns(true);
            mockPrivacyConfiguration.IsCrashReportingAllowed.Returns(true);
            mockPrivacyConfiguration.IsUserIdentificationIsAllowed.Returns(true);

            mockServerConfiguration = Substitute.For<IServerConfiguration>();
            mockServerConfiguration.IsSendingDataAllowed.Returns(true);
            mockServerConfiguration.IsSendingErrorsAllowed.Returns(true);
            mockServerConfiguration.IsCaptureEnabled.Returns(true);
            mockServerConfiguration.IsSendingErrorsAllowed.Returns(true);
            mockServerConfiguration.IsSendingCrashesAllowed.Returns(true);
            mockServerConfiguration.ServerId.Returns(ServerId);
            mockServerConfiguration.BeaconSizeInBytes.Returns(30 * 1024); // 30kB
            mockServerConfiguration.TrafficControlPercentage.Returns(100); // 100%
            mockServerConfiguration.Multiplicity.Returns(Multiplicity);

            var mockHttpClientConfig = Substitute.For<IHttpClientConfiguration>();
            mockHttpClientConfig.ServerId.Returns(ServerId);

            mockBeaconConfiguration = Substitute.For<IBeaconConfiguration>();
            mockBeaconConfiguration.OpenKitConfiguration.Returns(mockOpenKitConfiguration);
            mockBeaconConfiguration.PrivacyConfiguration.Returns(mockPrivacyConfiguration);
            mockBeaconConfiguration.ServerConfiguration.Returns(mockServerConfiguration);
            mockBeaconConfiguration.HttpClientConfiguration.Returns(mockHttpClientConfig);

            mockAdditionalQueryParameters = Substitute.For<IAdditionalQueryParameters>();

            mockSessionIdProvider = Substitute.For<ISessionIdProvider>();
            mockSessionIdProvider.GetNextSessionId().Returns(SessionId);

            mockThreadIdProvider = Substitute.For<IThreadIdProvider>();
            mockThreadIdProvider.ThreadId.Returns(ThreadId);

            mockTimingProvider = Substitute.For<ITimingProvider>();
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(0);

            mockLogger = Substitute.For<ILogger>();
            mockBeaconCache = Substitute.For<IBeaconCache>();
            mockSupplementaryBasicData = Substitute.For<ISupplementaryBasicData>();

            mockParent = Substitute.For<IOpenKitComposite>();
            mockParent.ActionId.Returns(0);

#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
            // explicitly manipulate the CurrentCulture to Austrian German
            // ensure it's restored in TearDown
            // reason - some number formatting behaves different in German
            // Note: .NET Core 1.0/1.1 does not allow manipulating the thread's culture
            // Manipulating the culture for all threads might have negative impact
            var newCulture = new CultureInfo("de-AT");
            newCulture.NumberFormat.NegativeSign = "~"; // use tilde for negative numbers
            newCulture.NumberFormat.NumberGroupSizes = new int[2] { 1, 2 };
            newCulture.NumberFormat.NumberGroupSeparator = "_";
            currentCulture = CultureInfo.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = newCulture;
#endif
        }

        [TearDown]
        public void TearDown()
        {
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
#endif
        }

        #region generic defaults, instance creation and smaller getters/creators

        [Test]
        public void DefaultBeaconConfigurationDoesNotDisableCapturing()
        {
            // given
            var target = CreateBeacon().Build();

            // then
            Assert.That(target.IsDataCapturingEnabled, Is.True);
        }

        [Test]
        public void CreateInstanceWithInvalidIpAddress()
        {
            // given when
            string capturedIpAddress = null;
            mockLogger.IsWarnEnabled.Returns(true);
            var httpClient = Substitute.For<IHttpClient>();
            httpClient.SendBeaconRequest(Arg.Do<string>(c => capturedIpAddress = c), Arg.Any<byte[]>(),
                    Arg.Any<IAdditionalQueryParameters>(), Arg.Any<int>(), Arg.Any<long>())
                .Returns(null as IStatusResponse);

            var httpClientProvider = Substitute.For<IHttpClientProvider>();
            httpClientProvider.CreateClient(Arg.Any<IHttpClientConfiguration>()).Returns(httpClient);

            const string ipAddress = "invalid";

            var target = CreateBeacon()
                .WithIpAddress(ipAddress)
                .Build();

            // then
            mockLogger.Received(1).Warn($"Beacon: Client IP address validation failed: {ipAddress}");

            mockBeaconCache.HasDataForSending(Arg.Any<BeaconKey>()).Returns(true, false);
            mockBeaconCache.GetNextBeaconChunk(Arg.Any<BeaconKey>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<char>())
                .Returns("dummy");

            // when
            target.Send(httpClientProvider, mockAdditionalQueryParameters);

            // then
            Assert.That(capturedIpAddress, Is.Null);
            httpClient.Received(1)
                .SendBeaconRequest(capturedIpAddress, Arg.Any<byte[]>(), mockAdditionalQueryParameters, Arg.Any<int>(), Arg.Any<long>());
        }

        [Test]
        public void CreateInstanceWithNullIpAddress()
        {
            // given when
            string capturedIpAddress = null;
            mockLogger.IsWarnEnabled.Returns(true);
            var httpClient = Substitute.For<IHttpClient>();
            httpClient.SendBeaconRequest(Arg.Do<string>(c => capturedIpAddress = c), Arg.Any<byte[]>(),
                    Arg.Any<IAdditionalQueryParameters>(), Arg.Any<int>(), Arg.Any<long>())
                .Returns(null as IStatusResponse);

            var httpClientProvider = Substitute.For<IHttpClientProvider>();
            httpClientProvider.CreateClient(Arg.Any<IHttpClientConfiguration>()).Returns(httpClient);

            var target = CreateBeacon()
                .WithIpAddress(null)
                .Build();

            // then
            mockLogger.Received(0).Warn(Arg.Any<string>());

            mockBeaconCache.HasDataForSending(Arg.Any<BeaconKey>()).Returns(true, false);
            mockBeaconCache.GetNextBeaconChunk(Arg.Any<BeaconKey>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<char>())
                .Returns("dummy");

            // when
            target.Send(httpClientProvider, mockAdditionalQueryParameters);

            // then
            Assert.That(capturedIpAddress, Is.Null);
            httpClient.Received(1)
                .SendBeaconRequest(capturedIpAddress, Arg.Any<byte[]>(), mockAdditionalQueryParameters, Arg.Any<int>(), Arg.Any<long>());
        }

        [Test]
        public void NextId()
        {
            // given
            var target = CreateBeacon().Build();

            // when, then
            for (var i = 1; i <= 3; i++)
            {
                var id = target.NextId;
                Assert.That(id, Is.EqualTo(i));
            }
        }

        [Test]
        public void CurrentTimeStamp()
        {
            // given
            const long expectedTimeStamp = 42;
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(expectedTimeStamp);

            var target = CreateBeacon().Build();

            mockTimingProvider.ClearReceivedCalls();

            // when
            var obtained = target.CurrentTimestamp;

            // then
            Assert.That(obtained, Is.EqualTo(expectedTimeStamp));
            mockTimingProvider.Received(1).ProvideTimestampInMilliseconds();
        }

        [Test]
        public void NextSequenceNumber()
        {
            // given
            var target = CreateBeacon().Build();

            // when, then
            for (var i = 1; i <= 3; i++)
            {
                var obtained = target.NextSequenceNumber;
                Assert.That(obtained, Is.EqualTo(i));
            }
        }

        [Test]
        public void SessionStartTime()
        {
            // given
            const long startTime = 73;
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(startTime, 1L);

            // when
            var target = CreateBeacon().Build();

            // then
            Assert.That(target.SessionStartTime, Is.EqualTo(startTime));
        }

        #endregion

        #region CreateTag - creating web request tag tests

        [Test]
        public void CreateTag()
        {
            // given
            const int sequenceNumber = 42;
            var target = CreateBeacon().Build();

            // when
            var obtained = target.CreateTag(ActionId, sequenceNumber);

            // then
            Assert.That(obtained, Is.EqualTo(
                "MT"                                                           // tag prefix
                + $"_{ProtocolConstants.ProtocolVersion.ToInvariantString()}"  // protocol version
                + $"_{ServerId.ToInvariantString()}"                           // server ID
                + $"_{DeviceId.ToInvariantString()}"                           // device ID
                + $"_{SessionId.ToInvariantString()}"                          // session number
                + $"_{AppId}"                                                  // application ID
                + $"_{ActionId.ToInvariantString()}"                           // action ID
                + $"_{ThreadId.ToInvariantString()}"                           // thread ID
                + $"_{sequenceNumber.ToInvariantString()}"                     // sequence number
                ));
        }

        [Test]
        public void CreateWebRequestTagEncodesDeviceIdProperly()
        {
            // given
            const int sequenceNumber = 1;
            mockOpenKitConfiguration.DeviceId.Returns(DeviceId);
            var target = CreateBeacon().Build();

            // when
            var obtained = target.CreateTag(ActionId, sequenceNumber);

            // then
            Assert.That(obtained, Is.EqualTo(
                "MT"                                                          // tag prefix
                + $"_{ProtocolConstants.ProtocolVersion.ToInvariantString()}" // protocol version
                + $"_{ServerId.ToInvariantString()}"                          // server ID
                + $"_{DeviceId.ToInvariantString()}"                          // device ID
                + $"_{SessionId.ToInvariantString()}"                         // session number
                + $"_{AppId}"                                                 // application ID
                + $"_{ActionId.ToInvariantString()}"                          // action ID
                + $"_{ThreadId.ToInvariantString()}"                          // thread ID
                + $"_{sequenceNumber.ToInvariantString()}"                    // sequence number
                ));
        }

        [Test]
        public void CreateTagDoesNotAppendSessionSequenceNumberForVisitStoreVersionsLowerTwo()
        {
            const int tracerSeqNo = 42;
            const int sessionSeqNo = 73;
            for (var version = 1; version > -2; version--)
            {
                // given
                mockServerConfiguration.VisitStoreVersion.Returns(version);
                var target = CreateBeacon().WithSessionSequenceNumber(sessionSeqNo).Build();

                // when
                var obtained = target.CreateTag(ActionId, tracerSeqNo);

                // then
                Assert.That(obtained, Is.EqualTo(
                    "MT"                                                          // tag prefix
                    + $"_{ProtocolConstants.ProtocolVersion.ToInvariantString()}" // protocol version
                    + $"_{ServerId.ToInvariantString()}"                          // server ID
                    + $"_{DeviceId.ToInvariantString()}"                          // device ID percent encoded
                    + $"_{SessionId.ToInvariantString()}"                         // session number
                    + $"_{AppId}"                                                 // application ID
                    + $"_{ActionId.ToInvariantString()}"                          // parent action ID
                    + $"_{ThreadId.ToInvariantString()}"                          // thread ID
                    + $"_{tracerSeqNo.ToInvariantString()}"                       // sequence number
                ));
            }
        }

        [Test]
        public void CreateTagAddsSessionSequenceNumberForVisitStoreVersionHigherOne()
        {
            const int tracerSeqNo = 4242;
            const int sessionSeqNo = 7321;
            for (var version = 2; version < 5; version++)
            {
                // given
                mockServerConfiguration.VisitStoreVersion.Returns(version);
                var target = CreateBeacon().WithSessionSequenceNumber(sessionSeqNo).Build();

                // when
                var obtained = target.CreateTag(ActionId, tracerSeqNo);

                // then
                Assert.That(obtained, Is.EqualTo(
                    "MT"                                                          // tag prefix
                    + $"_{ProtocolConstants.ProtocolVersion.ToInvariantString()}" // protocol version
                    + $"_{ServerId.ToInvariantString()}"                          // server ID
                    + $"_{DeviceId.ToInvariantString()}"                          // device ID percent encoded
                    + $"_{SessionId.ToInvariantString()}"                         // session number
                    + $"-{sessionSeqNo.ToInvariantString()}"                      // session sequence number
                    + $"_{AppId}"                                                 // application ID
                    + $"_{ActionId.ToInvariantString()}"                          // parent action ID
                    + $"_{ThreadId.ToInvariantString()}"                          // thread ID
                    + $"_{tracerSeqNo.ToInvariantString()}"                       // sequence number
                ));
            }
        }

        [Test]
        public void BeaconReturnsEmptyTagIfWebRequestTracingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsWebRequestTracingAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            var tagReturned = target.CreateTag(ActionId, 1);

            // then
            Assert.That(tagReturned, Is.Empty);
        }

        [Test]
        public void BeaconReturnsValidTagIfWebRequestTracingIsAllowed()
        {
            // given
            const int sequenceNo = 1;
            mockPrivacyConfiguration.IsWebRequestTracingAllowed.Returns(true);
            var target = CreateBeacon().Build();

            // when
            var obtained = target.CreateTag(ActionId, sequenceNo);

            // then
            Assert.That(obtained, Is.EqualTo(
                "MT" +                                        // tag prefix
                 "_3" +                                       // protocol version
                $"_{ServerId.ToInvariantString()}" +          // server ID
                $"_{DeviceId.ToInvariantString()}" +          // device ID
                $"_{SessionId.ToInvariantString()}" +         // session number
                $"_{AppId}" +                                 // application ID
                $"_{ActionId.ToInvariantString()}" +          // parent action ID
                $"_{ThreadId.ToInvariantString()}" +          // thread ID
                $"_{sequenceNo.ToInvariantString()}"          // sequence number
            ));
        }

        [Test]
        public void BeaconReturnsValidTagWithSessionNumberOneIfSessionNumberReportingDisallowed()
        {
            // given
            const int sequenceNo = 1;
            mockPrivacyConfiguration.IsSessionNumberReportingAllowed.Returns(false);
            mockPrivacyConfiguration.IsWebRequestTracingAllowed.Returns(true);

            var target = CreateBeacon().Build();

            // when
            var obtained = target.CreateTag(ActionId, sequenceNo);

            // then
            Assert.That(obtained, Is.EqualTo(
                "MT" +                                        // tag prefix
                 "_3" +                                       // protocol version
                $"_{ServerId.ToInvariantString()}" +          // server ID
                $"_{DeviceId.ToInvariantString()}" +          // device ID
                 "_1" +                                       // session number (must be one if session number reporting disallowed)
                $"_{AppId}" +                                 // application ID
                $"_{ActionId.ToInvariantString()}" +          // parent action ID
                $"_{ThreadId.ToInvariantString()}" +          // thread ID
                $"_{sequenceNo.ToInvariantString()}"          // sequence number
            ));
        }

        #endregion

        #region AddAction tests

        [Test]
        public void AddValidActionEvent()
        {
            // given
            var target = CreateBeacon().Build();

            const int parentId = 13;
            const string actionName = "myAction";
            var action = Substitute.For<IActionInternals>();
            action.Id.Returns(ActionId);
            action.ParentId.Returns(parentId);
            action.Name.Returns(actionName);

            // when
            target.AddAction(action);

            // then
            mockBeaconCache.Received(1).AddActionData(
                new BeaconKey(SessionId, SessionSeqNo),               // beacon key
                0,                                                    // timestamp
                $"et={EventType.ACTION.ToInt().ToInvariantString()}"  // event type
                + $"&it={ThreadId.ToInvariantString()}"               // thread ID
                + $"&na={actionName}"                                 // action name
                + $"&ca={ActionId.ToInvariantString()}"               // action ID
                + $"&pa={parentId.ToInvariantString()}"               // parent action ID
                + "&s0=0"                                            // action start sequence number
                + "&t0=0"                                            // action start time
                + "&s1=0"                                            // action end sequence number
                + "&t1=0"                                            // action end time
                );
        }

        [Test]
        public void AddingNullActionThrowsException()
        {
            // given
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.AddAction(null));
            Assert.That(exception.Message, Is.EqualTo("action is null or action.Name is null or empty"));
        }

        [Test]
        public void AddingActionWithNullNameThrowsException()
        {
            // given
            var action = Substitute.For<IActionInternals>();
            action.Name.ReturnsNull();

            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.AddAction(action));
            Assert.That(exception.Message, Is.EqualTo("action is null or action.Name is null or empty"));
        }

        [Test]
        public void AddingActionWithEmptyNameThrowsException()
        {
            // given
            var action = Substitute.For<IActionInternals>();
            action.Name.Returns(string.Empty);

            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.AddAction(action));
            Assert.That(exception.Message, Is.EqualTo("action is null or action.Name is null or empty"));
        }

        [Test]
        public void ActionNotReportedIfDataSendingDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);
            var action = Substitute.For<IActionInternals>();
            action.Name.Returns("actionName");

            var target = CreateBeacon().Build();

            // when
            target.AddAction(action);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ActionNotReportedIfDisallowedByTrafficControl()
        {
            // given
            const int trafficControlPercentage = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlPercentage);

            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlPercentage);

            var action = Substitute.For<IActionInternals>();
            action.Id.Returns(ActionId);
            action.Name.Returns("actionName");

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            target.AddAction(action);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ActionNotReportedIfActionReportingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsActionReportingAllowed.Returns(false);
            var action = Substitute.For<IActionInternals>();
            action.Name.Returns("actionName");

            var target = CreateBeacon().Build();

            // when
            target.AddAction(action);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        #endregion

        #region startSession tests

        [Test]
        public void AddStartSessionEvent()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            target.StartSession();

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                       // beacon key
                0,                                                            // timestamp
                $"et={EventType.SESSION_START.ToInt().ToInvariantString()}"   // event type
                + $"&it={ThreadId.ToInvariantString()}"                       // thread ID
                + "&pa=0"                                                     // parent action ID
                + "&s0=1"                                                     // session end sequence number
                + "&t0=0"                                                     // session end time
                );
        }

        [Test]
        public void SessionStartIsReportedRegardlessOfPrivacyConfiguration()
        {
            // given
            var target = CreateBeacon().Build();
            mockPrivacyConfiguration.ClearSubstitute();

            // when
            target.StartSession();

            // then
            mockBeaconCache.Received(1).AddEventData(Arg.Any<BeaconKey>(), Arg.Any<long>(), Arg.Any<string>());
            Assert.That(mockPrivacyConfiguration.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoSessionStartIsReportedIfDataSendingIsDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);

            var target = CreateBeacon().Build();

            // when
            target.StartSession();

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoSessionStartIsReportedIfDisallowedByTrafficControl()
        {
            // given
            const int trafficControlPercentage = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlPercentage);

            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlPercentage);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            target.StartSession();

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        #endregion

        #region endSession tests

        [Test]
        public void AddEndSessionEvent()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            target.EndSession();

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                       // beacon key
                0,                                                            // timestamp
                $"et={EventType.SESSION_END.ToInt().ToInvariantString()}"     // event type
                + $"&it={ThreadId.ToInvariantString()}"                       // thread ID
                + "&pa=0"                                                    // parent action ID
                + "&s0=1"                                                    // session end sequence number
                + "&t0=0"                                                    // session end time
                );
        }

        [Test]
        public void SessionNotReportedIfSessionReportingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsSessionReportingAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.EndSession();

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void SessionNotReportedIfDataSendingDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.EndSession();

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void SessionNotReportedIfDisallowedByTrafficControl()
        {
            // given
            const int trafficControlPercentage = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlPercentage);

            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlPercentage);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            target.EndSession();

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        #endregion

        #region ReportValue(int) tests

        [Test]
        public void ReportValidValueInt()
        {
            // given
            var target = CreateBeacon().Build();
            const string valueName = "intValue";
            const int value = 42;

            // when
            target.ReportValue(ActionId, valueName, value);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                 // beacon key
                0,                                                      // timestamp
                $"et={EventType.VALUE_INT.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                 // thread ID
                + $"&na={valueName}"                                    // action name
                + $"&pa={ActionId.ToInvariantString()}"                 // parent action ID
                + "&s0=1"                                               // event sequence number
                + "&t0=0"                                               // event timestamp
                + $"&vl={value.ToInvariantString()}"                    // reported value
                );
        }

        [Test]
        public void ReportingIntValueWithNullValueNameThrowsException()
        {
            // given
            const int value = 42;
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportValue(ActionId, null, value));
            Assert.That(exception.Message, Is.EqualTo("valueName is null or empty"));
        }

        [Test]
        public void ReportingIntValueWithEmptyValueNameThrowsException()
        {
            // given
            const int value = 42;
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportValue(ActionId, string.Empty, value));
            Assert.That(exception.Message, Is.EqualTo("valueName is null or empty"));
        }

        [Test]
        public void IntValueNotReportedIfValueReportingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsValueReportingAllowed.Returns(false);

            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test int value", 13);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void IntValueNotReportedIfDataSendingDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test value", 123);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void IntValueNotReportedIfDisallowedByTrafficControl()
        {
            // given
            const int trafficControlPercentage = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlPercentage);

            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlPercentage);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            target.ReportValue(ActionId, "test value", 123);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        #endregion

        #region ReportValue(long) tests

        [Test]
        public void ReportValidValueLong()
        {
            // given
            var target = CreateBeacon().Build();
            const string valueName = "longValue";
            const long value = 21;

            // when
            target.ReportValue(ActionId, valueName, value);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                 // beacon key
                0,                                                      // timestamp
                $"et={EventType.VALUE_INT.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                 // thread ID
                + $"&na={valueName}"                                    // value name
                + $"&pa={ActionId.ToInvariantString()}"                 // parent action ID
                + "&s0=1"                                               // event sequence number
                + "&t0=0"                                               // event timestamp
                + $"&vl={value.ToInvariantString()}"                    // reported value
                );
        }

        [Test]
        public void ReportingLongValueWithNullValueNameThrowsException()
        {
            // given
            const long value = long.MaxValue;
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportValue(ActionId, null, value));
            Assert.That(exception.Message, Is.EqualTo("valueName is null or empty"));
        }

        [Test]
        public void ReportingLongValueWithEmptyValueNameThrowsException()
        {
            // given
            const long value = long.MinValue;
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportValue(ActionId, string.Empty, value));
            Assert.That(exception.Message, Is.EqualTo("valueName is null or empty"));
        }

        [Test]
        public void LongValueNotReportedIfValueReportingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsValueReportingAllowed.Returns(false);

            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test long value", long.MinValue);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void LongValueNotReportedIfDataSendingDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test value", long.MaxValue);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void LongValueNotReportedIfDisallowedByTrafficControl()
        {
            // given
            const int trafficControlPercentage = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlPercentage);

            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlPercentage);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            target.ReportValue(ActionId, "test value", long.MinValue);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        #endregion

        #region ReportValue(double) tests

        [Test]
        public void ReportValidValueDouble()
        {
            // given
            var target = CreateBeacon().Build();
            const string valueName = "doubleValue";
            const double value = 3.1415;

            // when
            target.ReportValue(ActionId, valueName, value);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                    // beacon key
                0,                                                         // timestamp
                $"et={EventType.VALUE_DOUBLE.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                    // thread ID
                + $"&na={valueName}"                                       // value name
                + $"&pa={ActionId.ToInvariantString()}"                    // parent action ID
                +  "&s0=1"                                                 // event sequence number
                +  "&t0=0"                                                 // event timestamp
                + $"&vl={value.ToInvariantString()}"                       // reported value
                );
        }

        [Test]
        public void ReportingDoubleValueWithNullValueNameThrowsException()
        {
            // given
            const double value = Math.PI;
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportValue(ActionId, null, value));
            Assert.That(exception.Message, Is.EqualTo("valueName is null or empty"));
        }

        [Test]
        public void ReportingDoubleValueWithEmptyValueNameThrowsException()
        {
            // given
            const double value = Math.E;
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportValue(ActionId, string.Empty, value));
            Assert.That(exception.Message, Is.EqualTo("valueName is null or empty"));
        }

        [Test]
        public void DoubleValueNotReportedIfValueReportingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsValueReportingAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test double value", 2.71);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void DoubleValueNotReportedIfDataSendingDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test double value", 2.71);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void DoubleValueNotReportedIfDisallowedByTrafficControl()
        {
            // given
            const int trafficControlPercentage = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlPercentage);

            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlPercentage);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            target.ReportValue(ActionId, "test double value", 2.71);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        #endregion

        #region ReportValue(string) tests

        [Test]
        public void ReportValidValueString()
        {
            // given
            var target = CreateBeacon().Build();
            const string valueName = "stringValue";
            const string value = "HelloWorld";

            // when
            target.ReportValue(ActionId, valueName, value);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                    // beacon key
                0,                                                         // timestamp
                $"et={EventType.VALUE_STRING.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                    // thread ID
                + $"&na={valueName}"                                       // value name
                + $"&pa={ActionId.ToInvariantString()}"                    // parent action ID
                +  "&s0=1"                                                 // event sequence number
                +  "&t0=0"                                                 // event timestamp
                + $"&vl={value}"                                           // reported value
                );
        }

        [Test]
        public void ReportingStringValueWithNullValueNameThrowsException()
        {
            // given
            const string value = "foo";
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportValue(ActionId, null, value));
            Assert.That(exception.Message, Is.EqualTo("valueName is null or empty"));
        }

        [Test]
        public void ReportingStringValueWithEmptyValueNameThrowsException()
        {
            // given
            const string value = "bar";
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportValue(ActionId, string.Empty, value));
            Assert.That(exception.Message, Is.EqualTo("valueName is null or empty"));
        }

        [Test]
        public void ReportValueStringWithValueNull()
        {
            // given
            var target = CreateBeacon().Build();
            const string valueName = "stringValue";

            // when
            target.ReportValue(ActionId, valueName, null);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                    // beacon key
                0,                                                         // timestamp
                $"et={EventType.VALUE_STRING.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                    // thread ID
                + $"&na={valueName}"                                       // value name
                + $"&pa={ActionId.ToInvariantString()}"                    // parent action ID
                + "&s0=1"                                                 // event sequence number
                + "&t0=0"                                                 // event timestamp
                );
        }

        [Test]
        public void StringValueNotReportedIValueReportingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsValueReportingAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test string value", "test data");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void StringValueNotReportedIfDataSendingDisallowed()
        {
            //given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test string value", "test data");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void StringValueNotReportedIfDisallowedByTrafficControl()
        {
            // given
            const int trafficControlPercentage = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlPercentage);

            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlPercentage);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            target.ReportValue(ActionId, "test value", "test data");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        #endregion

        #region Report Mutable Basic Data

        [Test]
        public void ReportNetworkTechnology()
        {
            // given
            const int sessionSequence = 1213;
            const int visitStoreVersion = 2;
            const string appVersion = "1111";
            const string ipAddress = "192.168.0.1";
            mockOpenKitConfiguration.ApplicationVersion.Returns(appVersion);
            mockOpenKitConfiguration.OperatingSystem.Returns("system");
            mockOpenKitConfiguration.Manufacturer.Returns("manufacturer");
            mockOpenKitConfiguration.ModelId.Returns("model");
            mockBeaconCache.HasDataForSending(Arg.Any<BeaconKey>()).Returns(true, false);
            mockBeaconCache.GetNextBeaconChunk(Arg.Any<BeaconKey>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<char>())
                .ReturnsNull();
            mockServerConfiguration.VisitStoreVersion.Returns(visitStoreVersion);
            mockSupplementaryBasicData.NetworkTechnology.Returns("TestValue");
            var target = CreateBeacon().WithIpAddress(ipAddress).WithSessionSequenceNumber(sessionSequence).Build();

            // when
            target.Send(Substitute.For<IHttpClientProvider>(), null);

            // then
            var expectedPrefix = $"vv={ProtocolConstants.ProtocolVersion.ToInvariantString()}" +
                $"&va={ProtocolConstants.OpenKitVersion}" +
                $"&ap={AppId}" +
                $"&vn={appVersion}" +
                $"&pt={ProtocolConstants.PlatformTypeOpenKit.ToInvariantString()}" +
                $"&tt={ProtocolConstants.AgentTechnologyType}" +
                $"&vi={DeviceId.ToInvariantString()}" +
                $"&sn={SessionId.ToInvariantString()}" +
                $"&ip={ipAddress}" +
                "&os=system" +
                "&mf=manufacturer" +
                "&md=model" +
                "&dl=2" +
                "&cl=2" +
                $"&vs={visitStoreVersion.ToInvariantString()}" +
                $"&ss={sessionSequence.ToInvariantString()}" +
                "&tx=0" +
                "&tv=0" +
                $"&mp={Multiplicity.ToInvariantString()}" + 
                "&np=TestValue";

            var expectedBeaconKey = new BeaconKey(SessionId, sessionSequence);
            mockBeaconCache.Received(1).PrepareDataForSending(expectedBeaconKey);
            mockBeaconCache.Received(1).HasDataForSending(expectedBeaconKey);
            mockBeaconCache.Received(1).GetNextBeaconChunk(
                expectedBeaconKey, expectedPrefix, Arg.Any<int>(), Arg.Any<char>());
        }

        [Test]
        public void ReportCarrier()
        {
            // given
            const int sessionSequence = 1213;
            const int visitStoreVersion = 2;
            const string appVersion = "1111";
            const string ipAddress = "192.168.0.1";
            mockOpenKitConfiguration.ApplicationVersion.Returns(appVersion);
            mockOpenKitConfiguration.OperatingSystem.Returns("system");
            mockOpenKitConfiguration.Manufacturer.Returns("manufacturer");
            mockOpenKitConfiguration.ModelId.Returns("model");
            mockBeaconCache.HasDataForSending(Arg.Any<BeaconKey>()).Returns(true, false);
            mockBeaconCache.GetNextBeaconChunk(Arg.Any<BeaconKey>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<char>())
                .ReturnsNull();
            mockServerConfiguration.VisitStoreVersion.Returns(visitStoreVersion);
            mockSupplementaryBasicData.Carrier.Returns("TestValue");
            var target = CreateBeacon().WithIpAddress(ipAddress).WithSessionSequenceNumber(sessionSequence).Build();

            // when
            target.Send(Substitute.For<IHttpClientProvider>(), null);

            // then
            var expectedPrefix = $"vv={ProtocolConstants.ProtocolVersion.ToInvariantString()}" +
                $"&va={ProtocolConstants.OpenKitVersion}" +
                $"&ap={AppId}" +
                $"&vn={appVersion}" +
                $"&pt={ProtocolConstants.PlatformTypeOpenKit.ToInvariantString()}" +
                $"&tt={ProtocolConstants.AgentTechnologyType}" +
                $"&vi={DeviceId.ToInvariantString()}" +
                $"&sn={SessionId.ToInvariantString()}" +
                $"&ip={ipAddress}" +
                "&os=system" +
                "&mf=manufacturer" +
                "&md=model" +
                "&dl=2" +
                "&cl=2" +
                $"&vs={visitStoreVersion.ToInvariantString()}" +
                $"&ss={sessionSequence.ToInvariantString()}" +
                "&tx=0" +
                "&tv=0" +
                $"&mp={Multiplicity.ToInvariantString()}" +
                "&cr=TestValue";

            var expectedBeaconKey = new BeaconKey(SessionId, sessionSequence);
            mockBeaconCache.Received(1).PrepareDataForSending(expectedBeaconKey);
            mockBeaconCache.Received(1).HasDataForSending(expectedBeaconKey);
            mockBeaconCache.Received(1).GetNextBeaconChunk(
                expectedBeaconKey, expectedPrefix, Arg.Any<int>(), Arg.Any<char>());
        }

        [Test]
        public void ReportConnectionType()
        {
            // given
            const int sessionSequence = 1213;
            const int visitStoreVersion = 2;
            const string appVersion = "1111";
            const string ipAddress = "192.168.0.1";
            mockOpenKitConfiguration.ApplicationVersion.Returns(appVersion);
            mockOpenKitConfiguration.OperatingSystem.Returns("system");
            mockOpenKitConfiguration.Manufacturer.Returns("manufacturer");
            mockOpenKitConfiguration.ModelId.Returns("model");
            mockBeaconCache.HasDataForSending(Arg.Any<BeaconKey>()).Returns(true, false);
            mockBeaconCache.GetNextBeaconChunk(Arg.Any<BeaconKey>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<char>())
                .ReturnsNull();
            mockServerConfiguration.VisitStoreVersion.Returns(visitStoreVersion);
            mockSupplementaryBasicData.ConnectionType.Returns(ConnectionType.LAN);
            var target = CreateBeacon().WithIpAddress(ipAddress).WithSessionSequenceNumber(sessionSequence).Build();

            // when
            target.Send(Substitute.For<IHttpClientProvider>(), null);

            // then
            var expectedPrefix = $"vv={ProtocolConstants.ProtocolVersion.ToInvariantString()}" +
                $"&va={ProtocolConstants.OpenKitVersion}" +
                $"&ap={AppId}" +
                $"&vn={appVersion}" +
                $"&pt={ProtocolConstants.PlatformTypeOpenKit.ToInvariantString()}" +
                $"&tt={ProtocolConstants.AgentTechnologyType}" +
                $"&vi={DeviceId.ToInvariantString()}" +
                $"&sn={SessionId.ToInvariantString()}" +
                $"&ip={ipAddress}" +
                "&os=system" +
                "&mf=manufacturer" +
                "&md=model" +
                "&dl=2" +
                "&cl=2" +
                $"&vs={visitStoreVersion.ToInvariantString()}" +
                $"&ss={sessionSequence.ToInvariantString()}" +
                "&tx=0" +
                "&tv=0" +
                $"&mp={Multiplicity.ToInvariantString()}" +
                "&ct=l";

            var expectedBeaconKey = new BeaconKey(SessionId, sessionSequence);
            mockBeaconCache.Received(1).PrepareDataForSending(expectedBeaconKey);
            mockBeaconCache.Received(1).HasDataForSending(expectedBeaconKey);
            mockBeaconCache.Received(1).GetNextBeaconChunk(
                expectedBeaconKey, expectedPrefix, Arg.Any<int>(), Arg.Any<char>());
        }

        #endregion

        #region SendEvent tests

        [Test]
        public void SendValidEvent()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventName = "SomeEvent";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add("TestString", JsonStringValue.FromString("Test"));
            attributes.Add("TestBool", JsonBooleanValue.FromValue(false));

            // when
            target.SendEvent(eventName, attributes);

            Dictionary<string, JsonValue> actualAttributes = new Dictionary<string, JsonValue>();
            actualAttributes.Add("TestString", JsonStringValue.FromString("Test"));
            actualAttributes.Add("TestBool", JsonBooleanValue.FromValue(false));

            actualAttributes.Add(EventPayloadAttributes.TIMESTAMP, JsonNumberValue.FromLong(0));
            actualAttributes.Add(Beacon.EventPayloadApplicationId, JsonStringValue.FromString(AppId));
            actualAttributes.Add(Beacon.EventPayloadInstanceId, JsonStringValue.FromString(DeviceId.ToInvariantString()));
            actualAttributes.Add(Beacon.EventPayloadSessionId, JsonStringValue.FromString(SessionId.ToInvariantString()));
            actualAttributes.Add("dt.rum.schema_version", JsonStringValue.FromString("1.2"));
            actualAttributes.Add(EventPayloadAttributes.APP_VERSION, JsonStringValue.FromString(AppVersion));
            actualAttributes.Add(EventPayloadAttributes.OS_NAME, JsonStringValue.FromString(""));
            actualAttributes.Add(EventPayloadAttributes.DEVICE_MANUFACTURER, JsonStringValue.FromString(""));
            actualAttributes.Add(EventPayloadAttributes.DEVICE_MODEL_IDENTIFIER, JsonStringValue.FromString(""));
            actualAttributes.Add(EventPayloadAttributes.EVENT_PROVIDER, JsonStringValue.FromString(AppId));
            actualAttributes.Add("event.name", JsonStringValue.FromString(eventName));
            actualAttributes.Add(EventPayloadAttributes.EVENT_KIND, JsonStringValue.FromString("RUM_EVENT"));

            // then
            string encodedPayload = PercentEncoder.Encode(JsonObjectValue.FromDictionary(actualAttributes).ToString(), Encoding.UTF8, Beacon.ReservedCharacters);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.EVENT.ToInt().ToInvariantString()}" // event type
                + $"&pl={encodedPayload}"                                             
                );
        }

        [Test]
        public void SendValidEventTryingToOverrideDtValuesWhichAreNotAllowed()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventName = "SomeEvent";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add(Beacon.EventPayloadApplicationId, JsonStringValue.FromString("Test"));
            attributes.Add(Beacon.EventPayloadInstanceId, JsonStringValue.FromString("Test"));
            attributes.Add(Beacon.EventPayloadSessionId, JsonStringValue.FromString("Test"));
            attributes.Add("dt.rum.schema_version", JsonStringValue.FromString("Test"));

            // when
            target.SendEvent(eventName, attributes);

            Dictionary<string, JsonValue> actualAttributes = new Dictionary<string, JsonValue>();

            actualAttributes.Add(Beacon.EventPayloadSessionId, JsonStringValue.FromString(SessionId.ToInvariantString()));
            actualAttributes.Add(Beacon.EventPayloadInstanceId, JsonStringValue.FromString(DeviceId.ToInvariantString()));
            actualAttributes.Add(Beacon.EventPayloadApplicationId, JsonStringValue.FromString(AppId));
            actualAttributes.Add(EventPayloadAttributes.TIMESTAMP, JsonNumberValue.FromLong(0));
            actualAttributes.Add("dt.rum.schema_version", JsonStringValue.FromString("1.2"));
            actualAttributes.Add(EventPayloadAttributes.APP_VERSION, JsonStringValue.FromString(AppVersion));
            actualAttributes.Add(EventPayloadAttributes.OS_NAME, JsonStringValue.FromString(""));
            actualAttributes.Add(EventPayloadAttributes.DEVICE_MANUFACTURER, JsonStringValue.FromString(""));
            actualAttributes.Add(EventPayloadAttributes.DEVICE_MODEL_IDENTIFIER, JsonStringValue.FromString(""));
            actualAttributes.Add(EventPayloadAttributes.EVENT_PROVIDER, JsonStringValue.FromString(AppId));
            actualAttributes.Add("event.name", JsonStringValue.FromString(eventName));
            actualAttributes.Add(EventPayloadAttributes.EVENT_KIND, JsonStringValue.FromString("RUM_EVENT"));

            // then
            string encodedPayload = PercentEncoder.Encode(JsonObjectValue.FromDictionary(actualAttributes).ToString(), Encoding.UTF8, Beacon.ReservedCharacters);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.EVENT.ToInt().ToInvariantString()}" // event type
                + $"&pl={encodedPayload}"
                );
        }

        [Test]
        public void SendValidEventTryingToOverrideTimestamp()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventName = "SomeEvent";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add(EventPayloadAttributes.TIMESTAMP, JsonStringValue.FromString("Test"));

            // when
            target.SendEvent(eventName, attributes);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("timestamp%22%3A%22Test%22%2C%22"))
                );
        }

        [Test]
        public void SendValidEventTryingToOverrideDtType()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventName = "SomeEvent";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add(EventPayloadAttributes.EVENT_KIND, JsonStringValue.FromString("Test"));

            // when
            target.SendEvent(eventName, attributes);

            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("event.kind%22%3A%22Test%22%2C%22"))
                );
        }

        [Test]
        public void SendValidEventTryingToOverrideAppVersion()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventName = "SomeEvent";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add(EventPayloadAttributes.APP_VERSION, JsonStringValue.FromString("Test"));

            // when
            target.SendEvent(eventName, attributes);

            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("app.version%22%3A%22Test%22%2C%22"))
                );
        }

        [Test]
        public void SendValidEventTryingToOverrideOsName()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventName = "SomeEvent";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add(EventPayloadAttributes.OS_NAME, JsonStringValue.FromString("Test"));

            // when
            target.SendEvent(eventName, attributes);

            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("os.name%22%3A%22Test%22%2C%22"))
                );
        }

        [Test]
        public void SendValidEventTryingToOverrideDeviceManufacturer()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventName = "SomeEvent";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add(EventPayloadAttributes.DEVICE_MANUFACTURER, JsonStringValue.FromString("Test"));

            // when
            target.SendEvent(eventName, attributes);
          
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("device.manufacturer%22%3A%22Test%22%2C%22"))
                );
        }

        [Test]
        public void SendValidEventTryingToOverrideDeviceModelIdentifier()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventName = "SomeEvent";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add(EventPayloadAttributes.DEVICE_MODEL_IDENTIFIER, JsonStringValue.FromString("Test"));

            // when
            target.SendEvent(eventName, attributes);

            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("device.model.identifier%22%3A%22Test%22%2C%22"))
                );
        }

        [Test]
        public void SendValidEventTryingToOverrideEventProvider()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventName = "SomeEvent";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add(EventPayloadAttributes.EVENT_PROVIDER, JsonStringValue.FromString("Test"));

            // when
            target.SendEvent(eventName, attributes);

            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("event.provider%22%3A%22Test%22%2C%22"))
                );
        }

        [Test]
        public void SendEventWithNullEventNameThrowsException()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            var exception = Assert.Throws<ArgumentException>(() => target.SendEvent(null, null));
            Assert.That(exception.Message, Is.EqualTo("name is null or empty"));
        }

        [Test]
        public void SendEventWithEmptyEventNameThrowsException()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            var exception = Assert.Throws<ArgumentException>(() => target.SendEvent("", null));
            Assert.That(exception.Message, Is.EqualTo("name is null or empty"));
        }

        [Test]
        public void SendEventWithNullAttributes()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            target.SendEvent("name", null);

            Dictionary<string, JsonValue> actualAttributes = new Dictionary<string, JsonValue>();

            actualAttributes.Add(EventPayloadAttributes.TIMESTAMP, JsonNumberValue.FromLong(0));
            actualAttributes.Add("dt.rum.application.id", JsonStringValue.FromString(AppId));
            actualAttributes.Add("dt.rum.instance.id", JsonStringValue.FromString(DeviceId.ToInvariantString()));
            actualAttributes.Add(Beacon.EventPayloadSessionId, JsonStringValue.FromString(SessionId.ToInvariantString()));
            actualAttributes.Add("dt.rum.schema_version", JsonStringValue.FromString("1.2"));
            actualAttributes.Add(EventPayloadAttributes.APP_VERSION, JsonStringValue.FromString(AppVersion));
            actualAttributes.Add(EventPayloadAttributes.OS_NAME, JsonStringValue.FromString(""));
            actualAttributes.Add(EventPayloadAttributes.DEVICE_MANUFACTURER, JsonStringValue.FromString(""));
            actualAttributes.Add(EventPayloadAttributes.DEVICE_MODEL_IDENTIFIER, JsonStringValue.FromString(""));

            actualAttributes.Add(EventPayloadAttributes.EVENT_PROVIDER, JsonStringValue.FromString(AppId));
            actualAttributes.Add("event.name", JsonStringValue.FromString("name"));
            actualAttributes.Add(EventPayloadAttributes.EVENT_KIND, JsonStringValue.FromString("RUM_EVENT"));

            // then
            string encodedPayload = PercentEncoder.Encode(JsonObjectValue.FromDictionary(actualAttributes).ToString(), Encoding.UTF8, Beacon.ReservedCharacters);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.EVENT.ToInt().ToInvariantString()}" // event type
                + $"&pl={encodedPayload}"
                );
        }

        [Test]
        public void SendEventNotReportedIfDataSendingDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);
            var target = CreateBeacon().Build();

            const string eventName = "SomeEvent";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add("TestString", JsonStringValue.FromString("Test"));
            attributes.Add("TestBool", JsonBooleanValue.FromValue(false));
            attributes.Add("name", JsonStringValue.FromString(eventName));

            // when
            target.SendEvent(eventName, attributes);

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void SendEventNotReportedIfEventReportingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsEventReportingAllowed.Returns(false);
            var target = CreateBeacon().Build();

            const string eventName = "SomeEvent";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add("TestString", JsonStringValue.FromString("Test"));
            attributes.Add("TestBool", JsonBooleanValue.FromValue(false));
            attributes.Add("name", JsonStringValue.FromString(eventName));

            // when
            target.SendEvent(eventName, attributes);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void SendEventNotReportedIfDisallowedByTrafficControl()
        {
            // given
            const int trafficControlPercentage = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlPercentage);

            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlPercentage);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            const string eventName = "SomeEvent";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add("TestString", JsonStringValue.FromString("Test"));
            attributes.Add("TestBool", JsonBooleanValue.FromValue(false));
            attributes.Add("name", JsonStringValue.FromString(eventName));

            // when
            target.SendEvent(eventName, attributes);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void SendEventPayloadIsToBig()
        {
            var target = CreateBeacon().Build();
            const string eventName = "SomeEvent";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();

            for (int i = 0; i < 500; i++)
            {
                attributes.Add("TestNameForOversizeMap" + i, JsonStringValue.FromString(eventName));
            }

            var exception = Assert.Throws<ArgumentException>(() => target.SendEvent(eventName, attributes));
            Assert.That(exception.Message, Is.EqualTo($"Event payload is exceeding { Beacon.EventPayloadBytesLength } bytes!"));
        }

        [Test]
        public void SendValidEventWithNonFiniteValue()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventName = "SomeEvent";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add("custom", JsonNumberValue.FromDouble(Double.NaN));

            // when
            target.SendEvent(eventName, attributes);

            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("dt.rum.has%5Fnfn%5Fvalues%22%3Atrue"))
                );
        }

        #endregion

        #region SendBizEvent tests

        [Test]
        public void SendValidBizEvent()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventType = "SomeType";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add("TestString", JsonStringValue.FromString("Test"));
            attributes.Add("TestBool", JsonBooleanValue.FromValue(false));

            // when
            target.SendBizEvent(eventType, attributes);

            Dictionary<string, JsonValue> actualAttributes = new Dictionary<string, JsonValue>();
            actualAttributes.Add("TestString", JsonStringValue.FromString("Test"));
            actualAttributes.Add("TestBool", JsonBooleanValue.FromValue(false));

            actualAttributes.Add("event.type", JsonStringValue.FromString("SomeType"));
            actualAttributes.Add("dt.rum.custom_attributes_size", JsonNumberValue.FromLong(62));

            actualAttributes.Add(EventPayloadAttributes.TIMESTAMP, JsonNumberValue.FromLong(0));
            actualAttributes.Add(Beacon.EventPayloadApplicationId, JsonStringValue.FromString(AppId));
            actualAttributes.Add(Beacon.EventPayloadInstanceId, JsonStringValue.FromString(DeviceId.ToInvariantString()));
            actualAttributes.Add(Beacon.EventPayloadSessionId, JsonStringValue.FromString(SessionId.ToInvariantString()));
            actualAttributes.Add("dt.rum.schema_version", JsonStringValue.FromString("1.2"));
            actualAttributes.Add(EventPayloadAttributes.APP_VERSION, JsonStringValue.FromString(AppVersion));
            actualAttributes.Add(EventPayloadAttributes.OS_NAME, JsonStringValue.FromString(""));
            actualAttributes.Add(EventPayloadAttributes.DEVICE_MANUFACTURER, JsonStringValue.FromString(""));
            actualAttributes.Add(EventPayloadAttributes.DEVICE_MODEL_IDENTIFIER, JsonStringValue.FromString(""));

            actualAttributes.Add(EventPayloadAttributes.EVENT_PROVIDER, JsonStringValue.FromString(AppId));
            actualAttributes.Add(EventPayloadAttributes.EVENT_KIND, JsonStringValue.FromString("BIZ_EVENT"));

            // then
            string encodedPayload = PercentEncoder.Encode(JsonObjectValue.FromDictionary(actualAttributes).ToString(), Encoding.UTF8, Beacon.ReservedCharacters);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.EVENT.ToInt().ToInvariantString()}" // event type
                + $"&pl={encodedPayload}"
                );
        }

        [Test]
        public void SendValidBizEventTryingToOverrideDtValuesWhichAreNotAllowed()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventType = "SomeType";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add(Beacon.EventPayloadApplicationId, JsonStringValue.FromString("Test"));
            attributes.Add(Beacon.EventPayloadInstanceId, JsonStringValue.FromString("Test"));
            attributes.Add(Beacon.EventPayloadSessionId, JsonStringValue.FromString("Test"));
            attributes.Add("dt.rum.schema_version", JsonStringValue.FromString("1.2"));

            // when
            target.SendBizEvent(eventType, attributes);

            Dictionary<string, JsonValue> actualAttributes = new Dictionary<string, JsonValue>();
            
            actualAttributes.Add(Beacon.EventPayloadInstanceId, JsonStringValue.FromString(DeviceId.ToInvariantString()));
            actualAttributes.Add(Beacon.EventPayloadApplicationId, JsonStringValue.FromString(AppId));
            actualAttributes.Add(EventPayloadAttributes.TIMESTAMP, JsonNumberValue.FromLong(0));

            actualAttributes.Add("dt.rum.custom_attributes_size", JsonNumberValue.FromLong(134));
            actualAttributes.Add("event.type", JsonStringValue.FromString("SomeType"));

            actualAttributes.Add(Beacon.EventPayloadSessionId, JsonStringValue.FromString(SessionId.ToInvariantString()));
            actualAttributes.Add("dt.rum.schema_version", JsonStringValue.FromString("1.2"));
            actualAttributes.Add(EventPayloadAttributes.APP_VERSION, JsonStringValue.FromString(AppVersion));
            actualAttributes.Add(EventPayloadAttributes.OS_NAME, JsonStringValue.FromString(""));
            actualAttributes.Add(EventPayloadAttributes.DEVICE_MANUFACTURER, JsonStringValue.FromString(""));
            actualAttributes.Add(EventPayloadAttributes.DEVICE_MODEL_IDENTIFIER, JsonStringValue.FromString(""));

            actualAttributes.Add(EventPayloadAttributes.EVENT_PROVIDER, JsonStringValue.FromString(AppId));
            actualAttributes.Add(EventPayloadAttributes.EVENT_KIND, JsonStringValue.FromString("BIZ_EVENT"));

            // then
            string encodedPayload = PercentEncoder.Encode(JsonObjectValue.FromDictionary(actualAttributes).ToString(), Encoding.UTF8, Beacon.ReservedCharacters);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.EVENT.ToInt().ToInvariantString()}" // event type
                + $"&pl={encodedPayload}"
                );
        }

        [Test]
        public void SendValidBizEventTryingToOverrideTimestamp()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventType = "SomeType";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add(EventPayloadAttributes.TIMESTAMP, JsonStringValue.FromString("Test"));

            // when
            target.SendBizEvent(eventType, attributes);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("timestamp%22%3A%22Test%22%2C%22"))
                );
        }

        [Test]
        public void SendValidBizEventTryingToOverrideDtRumCustomAttributeSize()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventType = "customType";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add("dt.rum.custom_attributes_size", JsonStringValue.FromString("overridden"));

            // when
            target.SendBizEvent(eventType, attributes);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("dt.rum.custom%5Fattributes%5Fsize%22%3A72%2C%22"))
                );
        }

        [Test]
        public void SendValidBizEventTryingToOverrideEventProvider()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventType = "SomeType";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add(EventPayloadAttributes.EVENT_PROVIDER, JsonStringValue.FromString("Test"));

            // when
            target.SendBizEvent(eventType, attributes);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("event.provider%22%3A%22Test%22%2C%22"))
                );
        }

        [Test]
        public void SendValidBizEventTryingToOverrideDtType()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventType = "SomeType";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add(EventPayloadAttributes.EVENT_KIND, JsonStringValue.FromString("Test"));

            // when
            target.SendBizEvent(eventType, attributes);

            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("event.kind%22%3A%22BIZ%5FEVENT%22%2C%22"))
                );
        }

        [Test]
        public void SendValidBizEventTryingToOverrideAppVersion()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventType = "SomeType";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add(EventPayloadAttributes.APP_VERSION, JsonStringValue.FromString("Test"));

            // when
            target.SendBizEvent(eventType, attributes);

            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("app.version%22%3A%22Test%22%2C%22"))
                );
        }

        [Test]
        public void SendValidBizEventTryingToOverrideOsName()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventType = "SomeType";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add(EventPayloadAttributes.OS_NAME, JsonStringValue.FromString("Test"));

            // when
            target.SendBizEvent(eventType, attributes);

            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("os.name%22%3A%22Test%22%2C%22"))
                );
        }

        [Test]
        public void SendValidBizEventTryingToOverrideDeviceManufacturer()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventType = "SomeType";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add(EventPayloadAttributes.DEVICE_MANUFACTURER, JsonStringValue.FromString("Test"));

            // when
            target.SendBizEvent(eventType, attributes);

            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("device.manufacturer%22%3A%22Test%22%2C%22"))
                );
        }

        [Test]
        public void SendValidBizEventTryingToOverrideDeviceModelIdentifier()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventType = "SomeType";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add(EventPayloadAttributes.DEVICE_MODEL_IDENTIFIER, JsonStringValue.FromString("Test"));

            // when
            target.SendBizEvent(eventType, attributes);

            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("device.model.identifier%22%3A%22Test%22%2C%22"))
                );
        }

        [Test]
        public void SendValidBizEventWithNameInAttributes()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventType = "SomeType";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add("name", JsonStringValue.FromString("Test"));

            // when
            target.SendBizEvent(eventType, attributes);

            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("name%22%3A%22Test%22%2C%22"))
                );
        }

        [Test]
        public void SendBizEventWithNullEventTypeThrowsException()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            var exception = Assert.Throws<ArgumentException>(() => target.SendBizEvent(null, null));
            Assert.That(exception.Message, Is.EqualTo("type is null or empty"));
        }

        [Test]
        public void SendBizEventWithEmptyEventTypeThrowsException()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            var exception = Assert.Throws<ArgumentException>(() => target.SendBizEvent("", null));
            Assert.That(exception.Message, Is.EqualTo("type is null or empty"));
        }

        [Test]
        public void SendBizEventWithNullAttributes()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            target.SendBizEvent("type", null);

            Dictionary<string, JsonValue> actualAttributes = new Dictionary<string, JsonValue>();

            actualAttributes.Add("event.type", JsonStringValue.FromString("type"));
            actualAttributes.Add("dt.rum.custom_attributes_size", JsonNumberValue.FromLong(21));

            actualAttributes.Add(EventPayloadAttributes.TIMESTAMP, JsonNumberValue.FromLong(0));
            actualAttributes.Add("dt.rum.application.id", JsonStringValue.FromString(AppId));
            actualAttributes.Add("dt.rum.instance.id", JsonStringValue.FromString(DeviceId.ToInvariantString()));
            actualAttributes.Add(Beacon.EventPayloadSessionId, JsonStringValue.FromString(SessionId.ToInvariantString()));
            actualAttributes.Add("dt.rum.schema_version", JsonStringValue.FromString("1.2"));
            actualAttributes.Add(EventPayloadAttributes.APP_VERSION, JsonStringValue.FromString(AppVersion));
            actualAttributes.Add(EventPayloadAttributes.OS_NAME, JsonStringValue.FromString(""));
            actualAttributes.Add(EventPayloadAttributes.DEVICE_MANUFACTURER, JsonStringValue.FromString(""));
            actualAttributes.Add(EventPayloadAttributes.DEVICE_MODEL_IDENTIFIER, JsonStringValue.FromString(""));

            actualAttributes.Add(EventPayloadAttributes.EVENT_PROVIDER, JsonStringValue.FromString(AppId));
            actualAttributes.Add(EventPayloadAttributes.EVENT_KIND, JsonStringValue.FromString("BIZ_EVENT"));

            // then
            string encodedPayload = PercentEncoder.Encode(JsonObjectValue.FromDictionary(actualAttributes).ToString(), Encoding.UTF8, Beacon.ReservedCharacters);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.EVENT.ToInt().ToInvariantString()}" // event type
                + $"&pl={encodedPayload}"
                );
        }

        [Test]
        public void SendBizEventNotReportedIfDataSendingDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);
            var target = CreateBeacon().Build();

            const string eventType = "SomeType";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();

            // when
            target.SendBizEvent(eventType, attributes);

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void SendBizEventNotReportedIfEventReportingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsEventReportingAllowed.Returns(false);
            var target = CreateBeacon().Build();

            const string eventType = "SomeType";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();

            // when
            target.SendBizEvent(eventType, attributes);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void SendBizEventNotReportedIfDisallowedByTrafficControl()
        {
            // given
            const int trafficControlPercentage = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlPercentage);

            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlPercentage);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            const string eventType = "SomeType";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();;

            // when
            target.SendBizEvent(eventType, attributes);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void SendBizEventPayloadIsToBig()
        {
            var target = CreateBeacon().Build();
            const string eventType = "SomeType";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();

            for (int i = 0; i < 500; i++)
            {
                attributes.Add("TestNameForOversizeMap" + i, JsonStringValue.FromString(eventType));
            }

            var exception = Assert.Throws<ArgumentException>(() => target.SendBizEvent(eventType, attributes));
            Assert.That(exception.Message, Is.EqualTo($"Event payload is exceeding { Beacon.EventPayloadBytesLength } bytes!"));
        }

        [Test]
        public void SendValidBizEventWithNonFiniteValue()
        {
            // given
            var target = CreateBeacon().Build();
            const string eventType = "SomeType";

            Dictionary<string, JsonValue> attributes = new Dictionary<string, JsonValue>();
            attributes.Add("custom", JsonNumberValue.FromDouble(Double.NaN));

            // when
            target.SendBizEvent(eventType, attributes);

            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                Arg.Is<string>(str => str.Contains("dt.rum.has%5Fnfn%5Fvalues%22%3Atrue"))
                );
        }

        #endregion

        #region ReportEvent tests

        [Test]
        public void ReportValidEvent()
        {
            // given
            const string eventName = "someEvent";
            var target = CreateBeacon().Build();

            // when
            target.ReportEvent(ActionId, eventName);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.NAMED_EVENT.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                   // thread ID
                + $"&na={eventName}"                                      // event name
                + $"&pa={ActionId.ToInvariantString()}"                   // parent action ID
                +  "&s0=1"                                                // event sequence number
                +  "&t0=0"                                                // event timestamp
                );
        }

        [Test]
        public void ReportingEventWithNullEventNameThrowsException()
        {
            // given
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportEvent(ActionId, null));
            Assert.That(exception.Message, Is.EqualTo("eventName is null or empty"));
        }

        [Test]
        public void ReportingEventWithEmptyEventNameThrowsException()
        {
            // given
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportEvent(ActionId, string.Empty));
            Assert.That(exception.Message, Is.EqualTo("eventName is null or empty"));
        }

        [Test]
        public void NamedEventNotReportedIfDataSendingDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportEvent(ActionId, "Event name");

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NamedEventNotReportedIfEventReportingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsEventReportingAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportEvent(ActionId, "test event");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NamedEventNotReportedIfDisallowedByTrafficControl()
        {
            // given
            const int trafficControlPercentage = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlPercentage);

            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlPercentage);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            target.ReportEvent(ActionId, "Event name");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        #endregion

        #region ReportError with errorCode tests

        [Test]
        public void ReportErrorCode()
        {
            // given
            const string errorName = "someError";
            const int errorCode = -123;

            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, errorName, errorCode);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),             // beacon key
                0,                                                  // timestamp
                $"et={EventType.ERROR.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"             // thread ID
                + $"&na={errorName}"                                // action name
                + $"&pa={ActionId.ToInvariantString()}"             // parent action ID
                +  "&s0=1"                                          // event sequence number
                +  "&t0=0"                                          // event timestamp
                + $"&ev={errorCode.ToInvariantString()}"            // reported error code
                +  "&tt=c"                                          // error technology type
                );
        }
        
        [Test]
        public void ReportingErrorCodeWithNullErrorNameThrowsException()
        {
            // given
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportError(ActionId, null, 1234));
            Assert.That(exception.Message, Is.EqualTo("errorName is null or empty"));
        }

        [Test]
        public void ReportingErrorCodeWithEmptyErrorNameThrowsException()
        {
            // given
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportError(ActionId, string.Empty, 1234));
            Assert.That(exception.Message, Is.EqualTo("errorName is null or empty"));
        }

        [Test]
        public void ErrorNotReportedIfDataSendingDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, "Error name", 123);

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ErrorNotReportedIfErrorSendingDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingErrorsAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, "Error name", 123);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ErrorNotReportedIfErrorReportingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsErrorReportingAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, "error", 42);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void errorCodeNotReportedIfDisallowedByTrafficControl()
        {
            // given
            const int trafficControlPercentage = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlPercentage);

            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlPercentage);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            //when
            target.ReportError(ActionId, "DivByZeroError", 127);

            //then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        #endregion

        #region ReportError with cause tests

        [Test]
        public void ReportErrorWithCause()
        {
            // given
            const string errorName = "SomeError";
            const string causeName = "CausedBy";
            const string causeReason = "SomeReason";
            const string causeStackTrace = "HereComesTheTrace";

            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, errorName, causeName, causeReason, causeStackTrace);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                 // beacon key
                0,                                                      // timestamp
                $"et={EventType.EXCEPTION.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                 // thread ID
                + $"&na={errorName}"                                    // action name
                + $"&pa={ActionId.ToInvariantString()}"                 // parent action ID
                +  "&s0=1"                                              // event sequence number
                +  "&t0=0"                                              // event timestamp
                + $"&ev={causeName}"                                    // reported error value
                + $"&rs={causeReason}"                                  // reported error reason
                + $"&st={causeStackTrace}"                              // reported error stack trace
                +  "&tt=c"                                              // error technology type
                );
        }

        [Test]
        public void ReportingErrorWithCauseWithNullErrorNameThrowsException()
        {
            // given
            const string causeName = "CausedBy";
            const string causeReason = "SomeReason";
            const string causeStackTrace = "HereComesTheTrace";

            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportError(ActionId, null, causeName, causeReason, causeStackTrace));
            Assert.That(exception.Message, Is.EqualTo("errorName is null or empty"));
        }

        [Test]
        public void ReportingErrorWithCauseWithEmptyErrorNameThrowsException()
        {
            // given
            const string causeName = "CausedBy";
            const string causeReason = "SomeReason";
            const string causeStackTrace = "HereComesTheTrace";

            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportError(ActionId, string.Empty, causeName, causeReason, causeStackTrace));
            Assert.That(exception.Message, Is.EqualTo("errorName is null or empty"));
        }

        [Test]
        public void ReportErrorWithNullCauseNameWorks()
        {
            // given
            const string errorName = "SomeError";
            const string causeReason = "SomeReason";
            const string causeStackTrace = "HereComesTheTrace";

            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, errorName, null, causeReason, causeStackTrace);

            // then
            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                 // beacon key
                0,                                                      // timestamp
                $"et={EventType.EXCEPTION.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                 // thread ID
                + $"&na={errorName}"                                    // action name
                + $"&pa={ActionId.ToInvariantString()}"                 // parent action ID
                +  "&s0=1"                                              // event sequence number
                +  "&t0=0"                                              // event timestamp
                + $"&rs={causeReason}"                                  // reported error reason
                + $"&st={causeStackTrace}"                              // reported error stack trace
                +  "&tt=c"                                              // error technology type
                );
        }

        [Test]
        public void ReportErrorWithNullCauseDescriptionWorks()
        {
            // given
            const string errorName = "SomeError";
            const string causeName = "CausedBy";
            const string causeStackTrace = "HereComesTheTrace";

            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, errorName, causeName, null, causeStackTrace);

            // then
            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                 // beacon key
                0,                                                      // timestamp
                $"et={EventType.EXCEPTION.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                 // thread ID
                + $"&na={errorName}"                                    // action name
                + $"&pa={ActionId.ToInvariantString()}"                 // parent action ID
                +  "&s0=1"                                              // event sequence number
                +  "&t0=0"                                              // event timestamp
                + $"&ev={causeName}"                                    // reported error value
                + $"&st={causeStackTrace}"                              // reported error stack trace
                +  "&tt=c"                                              // error technology type
                );
        }

        [Test]
        public void ReportErrorWithNullCauseStackTraceWorks()
        {
            // given
            const string errorName = "SomeError";
            const string causeName = "CausedBy";
            const string causeReason = "SomeReason";

            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, errorName, causeName, causeReason, null);

            // then
            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                 // beacon key
                0,                                                      // timestamp
                $"et={EventType.EXCEPTION.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                 // thread ID
                + $"&na={errorName}"                                    // action name
                + $"&pa={ActionId.ToInvariantString()}"                 // parent action ID
                +  "&s0=1"                                              // event sequence number
                +  "&t0=0"                                              // event timestamp
                + $"&ev={causeName}"                                    // reported error value
                + $"&rs={causeReason}"                                  // reported error reason
                +  "&tt=c"                                              // error technology type
                );
        }

        [Test]
        public void ReportErrorIsTruncatingReasonIfTooLong()
        {
            // given
            const string errorName = "SomeError";
            const string causeName = "CausedBy";
            string causeReason = new string('a', 1001);
            string causeReasonTruncated = new string('a', 1000);

            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, errorName, causeName, causeReason, null);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                 // beacon key
                0,                                                      // timestamp
                $"et={EventType.EXCEPTION.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                 // thread ID
                + $"&na={errorName}"                                    // action name
                + $"&pa={ActionId.ToInvariantString()}"                 // parent action ID
                + "&s0=1"                                              // event sequence number
                + "&t0=0"                                              // event timestamp
                + $"&ev={causeName}"                                    // reported error value
                + $"&rs={causeReasonTruncated}"                                  // reported error reason
                + "&tt=c"                                              // error technology type
                );
        }

        [Test]
        public void ReportErrorIsTruncatingStacktraceIfTooLong()
        {
            // given
            const string errorName = "SomeError";
            const string causeName = "CausedBy";
            const string causeReason = "SomeReason";
            string causeStacktrace = new string('a', 128001);
            string causeStacktraceTruncated = new string('a', 128000);

            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, errorName, causeName, causeReason, causeStacktrace);

            // then
            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                 // beacon key
                0,                                                      // timestamp
                $"et={EventType.EXCEPTION.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                 // thread ID
                + $"&na={errorName}"                                    // action name
                + $"&pa={ActionId.ToInvariantString()}"                 // parent action ID
                + "&s0=1"                                              // event sequence number
                + "&t0=0"                                              // event timestamp
                + $"&ev={causeName}"                                    // reported error value
                + $"&rs={causeReason}"                                  // reported error reason
                + $"&st={causeStacktraceTruncated}"                          // reported stacktrace
                + "&tt=c"                                              // error technology type
                );
        }

        [Test]
        public void ReportErrorIsTruncatingStacktraceUntilLastBreakIfTooLong()
        {
            // given
            const string errorName = "SomeError";
            const string causeName = "CausedBy";
            const string causeReason = "SomeReason";
            string causeStacktraceTruncated = new string('a', 127900);
            string causeStacktrace = causeStacktraceTruncated + '\n' + new string('a', 1000);

            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, errorName, causeName, causeReason, causeStacktrace);

            // then
            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                 // beacon key
                0,                                                      // timestamp
                $"et={EventType.EXCEPTION.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                 // thread ID
                + $"&na={errorName}"                                    // action name
                + $"&pa={ActionId.ToInvariantString()}"                 // parent action ID
                + "&s0=1"                                              // event sequence number
                + "&t0=0"                                              // event timestamp
                + $"&ev={causeName}"                                    // reported error value
                + $"&rs={causeReason}"                                  // reported error reason
                + $"&st={causeStacktraceTruncated}"                          // reported stacktrace
                + "&tt=c"                                              // error technology type
                );
        }

        [Test]
        public void ErrorWithCauseNotReportedIfDataSendingIsDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, "Error name", "cause", "description", "stack trace");

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ErrorWithCauseNotReportedIfErrorSendingIsDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingErrorsAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, "Error name", "cause", "description", "stack trace");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ErrorWithCauseNotReportedIfErrorReportingIsDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsErrorReportingAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, "Error name", "cause", "description", "stack trace");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ErrorWithCauseNotReportedIfDisallowedByTrafficControl()
        {
            // given
            const int trafficControlPercentage = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlPercentage);

            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlPercentage);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            target.ReportError(ActionId, "error", "causeName", "causeDescription", "stackTrace");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        #endregion

        #region ReportError with Exception tests

        [Test]
        public void ReportErrorWithException()
        {
            // given
            const string errorName = "SomeError";
            var exception = CreateException();
            CrashFormatter crashFormatter = new CrashFormatter(exception);
            var causeName = crashFormatter.Name;
            var causeReason = PercentEncoder.Encode(crashFormatter.Reason, Encoding.UTF8, Beacon.ReservedCharacters);
            var causeStackTrace = PercentEncoder.Encode(crashFormatter.StackTrace.Trim(), Encoding.UTF8, Beacon.ReservedCharacters);

            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, errorName, exception);

            // then
            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                 // beacon key
                0,                                                      // timestamp
                $"et={EventType.EXCEPTION.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                 // thread ID
                + $"&na={errorName}"                                    // action name
                + $"&pa={ActionId.ToInvariantString()}"                 // parent action ID
                +  "&s0=1"                                              // event sequence number
                +  "&t0=0"                                              // event timestamp
                + $"&ev={causeName}"                                    // reported error value
                + $"&rs={causeReason}"                                  // reported error reason
                + $"&st={causeStackTrace}"                              // reported error stack trace
                +  "&tt=c"                                              // error technology type
                );
        }

        [Test]
        public void ReportingErrorWithExceptionWithNullErrorNameThrowsException()
        {
            // given
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportError(ActionId, null, CreateException()));
            Assert.That(exception.Message, Is.EqualTo("errorName is null or empty"));
        }

        [Test]
        public void ReportingErrorWithExceptionWithEmptyErrorNameThrowsException()
        {
            // given
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportError(ActionId, string.Empty, CreateException()));
            Assert.That(exception.Message, Is.EqualTo("errorName is null or empty"));
        }

        [Test]
        public void ReportingErrorWithNullExceptionWorks()
        {
            // given
            const string errorName = "SomeError";

            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, errorName, null);

            // then
            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                 // beacon key
                0,                                                      // timestamp
                $"et={EventType.EXCEPTION.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                 // thread ID
                + $"&na={errorName}"                                    // action name
                + $"&pa={ActionId.ToInvariantString()}"                 // parent action ID
                +  "&s0=1"                                              // event sequence number
                +  "&t0=0"                                              // event timestamp
                +  "&tt=c"                                              // error technology type
                );
        }

        [Test]
        public void ErrorWithExceptionNotReportedIfDataSendingIsDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, "Error name", CreateException());

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ErrorWithExceptionNotReportedIfErrorSendingIsDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingErrorsAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, "Error name", CreateException());

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ErrorWithExceptionNotReportedIfErrorReportingIsDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsErrorReportingAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, "Error name", CreateException());

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        #endregion

        #region ReportCrash with string tests

        [Test]
        public void ReportValidCrash()
        {
            // given
            const string errorName = "someError";
            const string reason = "someReason";
            const string stacktrace = "someStackTrace";

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash(errorName, reason, stacktrace);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),             // beacon key
                0,                                                  // timestamp
                $"et={EventType.CRASH.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"             // thread ID
                + $"&na={errorName}"                                // action name
                +  "&pa=0"                                          // parent action ID
                +  "&s0=1"                                          // event sequence number
                +  "&t0=0"                                          // event timestamp
                + $"&rs={reason}"                                   // error reason
                + $"&st={stacktrace}"                               // reported stacktrace
                +  "&tt=c"                                          // error technology type
                );
        }

        [Test]
        public void ReportingCrashWithNullErrorNameThrowsException()
        {
            // given
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportCrash(null, "reason", "stack trace"));
            Assert.That(exception.Message, Is.EqualTo("errorName is null or empty"));
        }

        [Test]
        public void ReportingCrashWithEmptyErrorNameThrowsException()
        {
            // given
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.ReportCrash(string.Empty, "reason", "stack trace"));
            Assert.That(exception.Message, Is.EqualTo("errorName is null or empty"));
        }

        [Test]
        public void ReportCrashWithNullReasonWorks()
        {
            // given
            const string errorName = "someError";
            const string stacktrace = "someStackTrace";

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash(errorName, null, stacktrace);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),             // beacon key
                0,                                                  // timestamp
                $"et={EventType.CRASH.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"             // thread ID
                + $"&na={errorName}"                                // action name
                +  "&pa=0"                                          // parent action ID
                +  "&s0=1"                                          // event sequence number
                +  "&t0=0"                                          // event timestamp
                + $"&st={stacktrace}"                               // reported stacktrace
                +  "&tt=c"                                          // error technology type
                );
        }

        [Test]
        public void ReportCrashWithNullStacktraceWorks()
        {
            // given
            const string errorName = "someError";
            const string reason = "someReason";

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash(errorName, reason, null);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),             // beacon key
                0,                                                  // timestamp
                $"et={EventType.CRASH.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"             // thread ID
                + $"&na={errorName}"                                // action name
                +  "&pa=0"                                          // parent action ID
                +  "&s0=1"                                          // event sequence number
                +  "&t0=0"                                          // event timestamp
                + $"&rs={reason}"                                   // error reason
                +  "&tt=c"                                          // error technology type
                );
        }

        [Test]
        public void ReportCrashIsTruncatingReasonIfTooLong()
        {
            // given
            const string errorName = "someError";
            string reason = new string('a', 1001);
            string reasonTruncated = new string('a', 1000);
            const string stacktrace = "someStackTrace";

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash(errorName, reason, stacktrace);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),             // beacon key
                0,                                                  // timestamp
                $"et={EventType.CRASH.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"             // thread ID
                + $"&na={errorName}"                                // action name
                + "&pa=0"                                          // parent action ID
                + "&s0=1"                                          // event sequence number
                + "&t0=0"                                          // event timestamp
                + $"&rs={reasonTruncated}"                                   // error reason
                + $"&st={stacktrace}"                               // reported stacktrace
                + "&tt=c"                                          // error technology type
                );
        }

        [Test]
        public void ReportCrashIsTruncatingStacktraceIfTooLong()
        {
            // given
            const string errorName = "someError";
            const string reason = "someReason";
            string stacktrace = new string('a', 128001);
            string stacktraceTruncated = new string('a', 128000);

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash(errorName, reason, stacktrace);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),             // beacon key
                0,                                                  // timestamp
                $"et={EventType.CRASH.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"             // thread ID
                + $"&na={errorName}"                                // action name
                + "&pa=0"                                          // parent action ID
                + "&s0=1"                                          // event sequence number
                + "&t0=0"                                          // event timestamp
                + $"&rs={reason}"                                   // error reason
                + $"&st={stacktraceTruncated}"                               // reported stacktrace
                + "&tt=c"                                          // error technology type
                );
        }

        [Test]
        public void ReportCrashIsTruncatingStacktraceUntilLastBreakIfTooLong()
        {
            // given
            const string errorName = "someError";
            const string reason = "someReason";
            string stacktraceTruncated = new string('a', 127900);
            string stacktrace = stacktraceTruncated + '\n' + new string('a', 1000);

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash(errorName, reason, stacktrace);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),             // beacon key
                0,                                                  // timestamp
                $"et={EventType.CRASH.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"             // thread ID
                + $"&na={errorName}"                                // action name
                + "&pa=0"                                          // parent action ID
                + "&s0=1"                                          // event sequence number
                + "&t0=0"                                          // event timestamp
                + $"&rs={reason}"                                   // error reason
                + $"&st={stacktraceTruncated}"                               // reported stacktrace
                + "&tt=c"                                          // error technology type
                );
        }

        [Test]
        public void ReportCrashDoesNotReportIfDataSendingDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash("Error name", "The reason for this error", "the stack trace");

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ReportCrashDoesNotReportIfCrashSendingDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingCrashesAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportCrash("Error name", "The reason for this error", "the stack trace");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ReportCrashDoesNotReportIfCrashReportingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsCrashReportingAllowed.Returns(false);

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash("OutOfMemory exception", "insufficient memory", "stacktrace:123");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ReportCrashDoesNotReportIfDisallowedByTrafficControl()
        {
            // given
            const int trafficControlPercentage = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlPercentage);

            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlPercentage);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            //when
            target.ReportCrash("OutOfMemory exception", "insufficient memory", "stacktrace:123");

            //then
            //verify error has not been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        #endregion

        #region ReportCrash with Exception tests

        [Test]
        public void ReportValidCrashException()
        {
            // given
            Exception exception = CreateException();
            var crashFormatter = new CrashFormatter(exception);
            var errorName = crashFormatter.Name;
            var reason = PercentEncoder.Encode(crashFormatter.Reason, Encoding.UTF8, Beacon.ReservedCharacters);
            var stacktrace = PercentEncoder.Encode(crashFormatter.StackTrace.Trim(), Encoding.UTF8, Beacon.ReservedCharacters);

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash(exception);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),             // beacon key
                0,                                                  // timestamp
                $"et={EventType.CRASH.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"             // thread ID
                + $"&na={errorName}"                                // action name
                + "&pa=0"                                           // parent action ID
                + "&s0=1"                                           // event sequence number
                + "&t0=0"                                           // event timestamp
                + $"&rs={reason}"                                   // error reason
                + $"&st={stacktrace}"                        // reported stacktrace
                + "&tt=c"                                           // error technology type
                );
        }

        [Test]
        public void ReportCrashWithNullExceptionThrowsException()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            var exception = Assert.Throws<ArgumentNullException>(() => target.ReportCrash(null));
            Assert.That(exception.ParamName, Is.EqualTo("exception"));
        }

        [Test]
        public void ReportCrashExceptionDoesNotReportIfDataSendingDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash(new ArgumentException("The reason for this error"));

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ReportCrashExceptionDoesNotReportIfCrashSendingDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingCrashesAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportCrash(new ArgumentException("The reason for this error"));

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ReportCrashExceptionDoesNotReportIfCrashReportingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsCrashReportingAllowed.Returns(false);

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash(new ArgumentException("The reason for this error"));

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ReportCrashExceptionDoesNotReportIfDisallowedByTrafficControl()
        {
            // given
            const int trafficControlPercentage = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlPercentage);

            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlPercentage);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            target.ReportCrash(new ArgumentException("The reason for this error"));

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        #endregion

        #region AddWebRequest tests

        [Test]
        public void AddWebRequest()
        {
            // given
            var target = CreateBeacon().Build();

            const int sentBytes = 1337;
            const int receivedBytes = 1447;
            const int responseCode = 418;
            var tracer = Substitute.For<IWebRequestTracerInternals>();
            tracer.Url.Returns(Url);
            tracer.BytesSent.Returns(sentBytes);
            tracer.BytesReceived.Returns(receivedBytes);
            tracer.ResponseCode.Returns(responseCode);

            // when
            target.AddWebRequest(ActionId, tracer);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                   // thread ID
                + $"&na={PercentEncoder.Encode(Url, Encoding.UTF8)}"      // reported URL
                + $"&pa={ActionId.ToInvariantString()}"                   // parent action ID
                +  "&s0=0"                                                // web request start sequence number
                +  "&t0=0"                                                // web request start timestamp
                +  "&s1=0"                                                // web request end sequence number
                +  "&t1=0"                                                // web request end timestamp
                + $"&bs={sentBytes.ToInvariantString()}"                  // number bytes sent
                + $"&br={receivedBytes.ToInvariantString()}"              // number bytes received
                + $"&rc={responseCode.ToInvariantString()}"               // number bytes received
                );
        }

        [Test]
        public void AddWebRequestWithNullWebRequestTracerThrowsException()
        {
            // given
            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.AddWebRequest(ActionId, null));
            Assert.That(exception.Message, Is.EqualTo("webRequestTracer is null or webRequestTracer.Url is null or empty"));
        }

        [Test]
        public void AddWebRequestWithNullUrlThrowsException()
        {
            // given
            var tracer = Substitute.For<IWebRequestTracerInternals>();
            tracer.Url.ReturnsNull();

            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.AddWebRequest(ActionId, null));
            Assert.That(exception.Message, Is.EqualTo("webRequestTracer is null or webRequestTracer.Url is null or empty"));
        }

        [Test]
        public void AddWebRequestWithEmptyUrlThrowsException()
        {
            // given
            var tracer = Substitute.For<IWebRequestTracerInternals>();
            tracer.Url.Returns(string.Empty);

            var target = CreateBeacon().Build();

            // when, then
            var exception = Assert.Throws<ArgumentException>(() => target.AddWebRequest(ActionId, null));
            Assert.That(exception.Message, Is.EqualTo("webRequestTracer is null or webRequestTracer.Url is null or empty"));
        }

        [Test]
        public void CanAddSentBytesEqualToZeroToWebRequestTracer()
        {
            // given
            var target = CreateBeacon().Build();

            const int sentBytes = 0;
            const int receivedBytes = 1447;
            const int responseCode = 418;
            var tracer = Substitute.For<IWebRequestTracerInternals>();
            tracer.Url.Returns(Url);
            tracer.BytesSent.Returns(sentBytes);
            tracer.BytesReceived.Returns(receivedBytes);
            tracer.ResponseCode.Returns(responseCode);

            // when
            target.AddWebRequest(ActionId, tracer);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                   // thread ID
                + $"&na={PercentEncoder.Encode(Url, Encoding.UTF8)}"      // reported URL
                + $"&pa={ActionId.ToInvariantString()}"                   // parent action ID
                +  "&s0=0"                                                // web request start sequence number
                +  "&t0=0"                                                // web request start timestamp
                +  "&s1=0"                                                // web request end sequence number
                +  "&t1=0"                                                // web request end timestamp
                + $"&bs={sentBytes.ToInvariantString()}"                  // number bytes sent
                + $"&br={receivedBytes.ToInvariantString()}"              // number bytes received
                + $"&rc={responseCode.ToInvariantString()}"               // number bytes received
                );
        }

        [Test]
        public void CannnotAddSentBytesLessThanZeroToWebRequestTracer()
        {
            // given
            var target = CreateBeacon().Build();

            const int sentBytes = -1;
            const int receivedBytes = 1447;
            const int responseCode = 418;
            var tracer = Substitute.For<IWebRequestTracerInternals>();
            tracer.Url.Returns(Url);
            tracer.BytesSent.Returns(sentBytes);
            tracer.BytesReceived.Returns(receivedBytes);
            tracer.ResponseCode.Returns(responseCode);

            // when
            target.AddWebRequest(ActionId, tracer);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                   // thread ID
                + $"&na={PercentEncoder.Encode(Url, Encoding.UTF8)}"      // reported URL
                + $"&pa={ActionId.ToInvariantString()}"                   // parent action ID
                +  "&s0=0"                                                // web request start sequence number
                +  "&t0=0"                                                // web request start timestamp
                +  "&s1=0"                                                // web request end sequence number
                +  "&t1=0"                                                // web request end timestamp
                + $"&br={receivedBytes.ToInvariantString()}"              // number bytes received
                + $"&rc={responseCode.ToInvariantString()}"               // number bytes received
                );
        }

        [Test]
        public void CanAddReceivedBytesEqualToZeroToWebRequestTracer()
        {
            // given
            var target = CreateBeacon().Build();

            const int sentBytes = 1337;
            const int receivedBytes = 0;
            const int responseCode = 418;
            var tracer = Substitute.For<IWebRequestTracerInternals>();
            tracer.Url.Returns(Url);
            tracer.BytesSent.Returns(sentBytes);
            tracer.BytesReceived.Returns(receivedBytes);
            tracer.ResponseCode.Returns(responseCode);

            // when
            target.AddWebRequest(ActionId, tracer);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                   // thread ID
                + $"&na={PercentEncoder.Encode(Url, Encoding.UTF8)}"      // reported URL
                + $"&pa={ActionId.ToInvariantString()}"                   // parent action ID
                +  "&s0=0"                                                // web request start sequence number
                +  "&t0=0"                                                // web request start timestamp
                +  "&s1=0"                                                // web request end sequence number
                +  "&t1=0"                                                // web request end timestamp
                + $"&bs={sentBytes.ToInvariantString()}"                  // number bytes sent
                + $"&br={receivedBytes.ToInvariantString()}"              // number bytes received
                + $"&rc={responseCode.ToInvariantString()}"               // number bytes received
                );
        }

        [Test]
        public void CannnotAddReceivedBytesLessThanZeroToWebRequestTracer()
        {
            // given
            var target = CreateBeacon().Build();

            const int sentBytes = 1337;
            const int receivedBytes = -1;
            const int responseCode = 418;
            var tracer = Substitute.For<IWebRequestTracerInternals>();
            tracer.Url.Returns(Url);
            tracer.BytesSent.Returns(sentBytes);
            tracer.BytesReceived.Returns(receivedBytes);
            tracer.ResponseCode.Returns(responseCode);

            // when
            target.AddWebRequest(ActionId, tracer);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                   // thread ID
                + $"&na={PercentEncoder.Encode(Url, Encoding.UTF8)}"      // reported URL
                + $"&pa={ActionId.ToInvariantString()}"                   // parent action ID
                +  "&s0=0"                                                // web request start sequence number
                +  "&t0=0"                                                // web request start timestamp
                +  "&s1=0"                                                // web request end sequence number
                +  "&t1=0"                                                // web request end timestamp
                + $"&bs={sentBytes.ToInvariantString()}"                  // number bytes sent
                + $"&rc={responseCode.ToInvariantString()}"               // number bytes received
                );
        }

        [Test]
        public void CanAddResponseCodeEqualToZeroToWebRequestTracer()
        {
            // given
            var target = CreateBeacon().Build();

            const int sentBytes = 1337;
            const int receivedBytes = 1447;
            const int responseCode = 0;
            var tracer = Substitute.For<IWebRequestTracerInternals>();
            tracer.Url.Returns(Url);
            tracer.BytesSent.Returns(sentBytes);
            tracer.BytesReceived.Returns(receivedBytes);
            tracer.ResponseCode.Returns(responseCode);

            // when
            target.AddWebRequest(ActionId, tracer);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                   // thread ID
                + $"&na={PercentEncoder.Encode(Url, Encoding.UTF8)}"      // reported URL
                + $"&pa={ActionId.ToInvariantString()}"                   // parent action ID
                +  "&s0=0"                                                // web request start sequence number
                +  "&t0=0"                                                // web request start timestamp
                +  "&s1=0"                                                // web request end sequence number
                +  "&t1=0"                                                // web request end timestamp
                + $"&bs={sentBytes.ToInvariantString()}"                  // number bytes sent
                + $"&br={receivedBytes.ToInvariantString()}"              // number bytes received
                + $"&rc={responseCode.ToInvariantString()}"               // number bytes received
                );
        }

        [Test]
        public void CannnotAddResponseCodeLessThanZeroToWebRequestTracer()
        {
            // given
            var target = CreateBeacon().Build();

            const int sentBytes = 1337;
            const int receivedBytes = 1447;
            const int responseCode = -1;
            var tracer = Substitute.For<IWebRequestTracerInternals>();
            tracer.Url.Returns(Url);
            tracer.BytesSent.Returns(sentBytes);
            tracer.BytesReceived.Returns(receivedBytes);
            tracer.ResponseCode.Returns(responseCode);

            // when
            target.AddWebRequest(ActionId, tracer);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                   // thread ID
                + $"&na={PercentEncoder.Encode(Url, Encoding.UTF8)}"      // reported URL
                + $"&pa={ActionId.ToInvariantString()}"                   // parent action ID
                +  "&s0=0"                                                // web request start sequence number
                +  "&t0=0"                                                // web request start timestamp
                +  "&s1=0"                                                // web request end sequence number
                +  "&t1=0"                                                // web request end timestamp
                + $"&bs={sentBytes.ToInvariantString()}"                  // number bytes sent
                + $"&br={receivedBytes.ToInvariantString()}"              // number bytes received
                );
        }

        [Test]
        public void NoWebRequestIsReportedIfDataSendingIsDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);
            var target = CreateBeacon().Build();
            var webRequestTracer = CreateWebRequestTracer(target).WithUrl("https://foo.bar").Build();

            // when
            target.AddWebRequest(ActionId, webRequestTracer);

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoWebRequestIsReportedIfWebRequestTracingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsWebRequestTracingAllowed.Returns(false);
            var webRequestTracer = Substitute.For<IWebRequestTracerInternals>();
            webRequestTracer.Url.Returns(Url);

            var target = CreateBeacon().Build();

            // when
            target.AddWebRequest(ActionId, webRequestTracer);

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoWebRequestIsReportedIfDisallowedByTrafficControl()
        {
            // given
            const int trafficControlPercentage = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlPercentage);

            var webRequestTracer = Substitute.For<IWebRequestTracerInternals>();
            webRequestTracer.Url.Returns(Url);

            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlPercentage);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            target.AddWebRequest(ActionId, webRequestTracer);

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

    #endregion

        #region IdentifyUser tests

        [Test]
        public void ValidIdentifyUserEvent()
        {
            // given
            const string userId = "myTestUser";

            var beacon = CreateBeacon().Build();

            // when
            beacon.IdentifyUser(userId);

            // then
             mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                     // beacon key
                0,                                                          // timestamp
                $"et={EventType.IDENTIFY_USER.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                     // thread ID
                + $"&na={userId}"                                           // reported user ID
                +  "&pa=0"                                                  // parent action ID
                +  "&s0=1"                                                  // web request start sequence number
                +  "&t0=0"                                                  // web request start timestamp
                );
        }

        [Test]
        public void IdentifyUserWithNullUserTagWorks()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            target.IdentifyUser(null);

             // then
             mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                     // beacon key
                0,                                                          // timestamp
                $"et={EventType.IDENTIFY_USER.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                     // thread ID
                +  "&pa=0"                                                  // parent action ID
                +  "&s0=1"                                                  // web request start sequence number
                +  "&t0=0"                                                  // web request start timestamp
                );
        }

        [Test]
        public void IdentifyUserWithEmptyUserTagWorks()
        {
            // given
            var beacon = CreateBeacon().Build();

            // when
            beacon.IdentifyUser(string.Empty);

            // then
            mockBeaconCache.Received(1).AddEventData(
               new BeaconKey(SessionId, SessionSeqNo),                     // beacon key
               0,                                                          // timestamp
               $"et={EventType.IDENTIFY_USER.ToInt().ToInvariantString()}" // event type
               + $"&it={ThreadId.ToInvariantString()}"                     // thread ID
               + $"&na="                                                   // reported user ID
               +  "&pa=0"                                                  // parent action ID
               +  "&s0=1"                                                  // web request start sequence number
               +  "&t0=0"                                                  // web request start timestamp
               );
        }

        [Test]
        public void CannotIdentifyUserIfDataSendingDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.IdentifyUser("jane.doe@acme.com");

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }
        
        [Test]
        public void CannotIdentifyUserIfUserIdentificationDisabled()
        {
            // given
            mockPrivacyConfiguration.IsUserIdentificationIsAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.IdentifyUser("test user");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void CannotIdentifyUserIfDisallowedByTrafficControl()
        {
            // given
            const int trafficControlPercentage = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlPercentage);

            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlPercentage);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            target.IdentifyUser("jane@doe.com");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        #endregion

        #region send tests

        [Test]
        public void CanHandleNoDataInBeaconSend()
        {
            // given
            var mockHttpClient = Substitute.For<IHttpClient>();
            var mockHttpClientProvider = Substitute.For<IHttpClientProvider>();
            mockHttpClientProvider.CreateClient(Arg.Any<HttpClientConfiguration>()).Returns(mockHttpClient);

            var target = CreateBeacon().Build();

            // when
            var response = target.Send(mockHttpClientProvider, mockAdditionalQueryParameters);

            // then
            Assert.That(response, Is.Null);
        }

        [Test]
        public void SendValidData()
        {
            // given
            const string ipAddress = "127.0.0.1";
            const int responseCode = 200;
            var successResponse = StatusResponse.CreateSuccessResponse(
                mockLogger,
                ResponseAttributes.WithJsonDefaults().Build(),
                responseCode,
                new Dictionary<string, List<string>>()
            );

            var target = CreateBeacon().With(new BeaconCache(mockLogger)).WithIpAddress(ipAddress).Build();

            var httpClient = Substitute.For<IHttpClient>();
            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<IAdditionalQueryParameters>(), Arg.Any<int>(), Arg.Any<long>())
                .Returns(successResponse);

            var httpClientProvider = Substitute.For<IHttpClientProvider>();
            httpClientProvider.CreateClient(Arg.Any<IHttpClientConfiguration>()).Returns(httpClient);

            // when
            target.ReportCrash("errorName", "errorReason", "stackTrace");
            var response = target.Send(httpClientProvider, mockAdditionalQueryParameters);

            // then
            Assert.That(response, Is.Not.Null);
            Assert.That(response.ResponseCode, Is.EqualTo(responseCode));
            httpClient.Received(1).SendBeaconRequest(ipAddress, Arg.Any<byte[]>(), mockAdditionalQueryParameters, Arg.Any<int>(), Arg.Any<long>());
        }

        [Test]
        public void SendCanHandleMultipleChunks()
        {
            // given
            var firstChunk = "some beacon string";
            var secondChunk = "some more beacon string";
            mockBeaconCache.HasDataForSending(Arg.Any<BeaconKey>()).Returns(true, true, false);
            mockBeaconCache.GetNextBeaconChunk(Arg.Any<BeaconKey>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<char>())
                            .Returns(firstChunk, secondChunk);

            const int responseCode = 200;
            var firstResponse = StatusResponse.CreateSuccessResponse(
                mockLogger, ResponseAttributesDefaults.JsonResponse, responseCode, new Dictionary<string, List<string>>());
            var secondResponse = StatusResponse.CreateSuccessResponse(
                mockLogger, ResponseAttributesDefaults.JsonResponse, responseCode, new Dictionary<string, List<string>>());

            var httpClient = Substitute.For<IHttpClient>();
            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<IAdditionalQueryParameters>(), Arg.Any<int>(), Arg.Any<long>())
                .Returns(firstResponse, secondResponse);

            var httpClientProvider = Substitute.For<IHttpClientProvider>();
            httpClientProvider.CreateClient(Arg.Any<IHttpClientConfiguration>()).Returns(httpClient);

            var target = CreateBeacon().Build();

            // when
            var response = target.Send(httpClientProvider, mockAdditionalQueryParameters);

            // then
            Assert.That(response, Is.Not.Null.And.SameAs(secondResponse));

            httpClient.Received(2).SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>(), mockAdditionalQueryParameters, Arg.Any<int>(), Arg.Any<long>());

            mockBeaconCache.Received(1).PrepareDataForSending(Arg.Any<BeaconKey>());
            mockBeaconCache.Received(3).HasDataForSending(Arg.Any<BeaconKey>());
            mockBeaconCache.Received(2).GetNextBeaconChunk(Arg.Any<BeaconKey>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<char>());
        }

    #endregion

        #region misc tests

        [Test]
        public void SendDataAndFakeErrorResponse()
        {
            // given
            const string ipAddress = "127.0.0.1";
            const int responseCode = 418;

            var target = CreateBeacon().With(new BeaconCache(mockLogger)).WithIpAddress(ipAddress).Build();

            var httpClient = Substitute.For<IHttpClient>();
            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<IAdditionalQueryParameters>(), Arg.Any<int>(), Arg.Any<long>())
                .Returns(StatusResponse.CreateErrorResponse(mockLogger, responseCode));

            var httpClientProvider = Substitute.For<IHttpClientProvider>();
            httpClientProvider.CreateClient(Arg.Any<IHttpClientConfiguration>()).Returns(httpClient);

            // when
            target.ReportCrash("errorName", "errorReason", "stackTrace");
            var response = target.Send(httpClientProvider, mockAdditionalQueryParameters);

            // then
            Assert.That(response, Is.Not.Null);
            Assert.That(response.ResponseCode, Is.EqualTo(responseCode));
            httpClient.Received(1).SendBeaconRequest(ipAddress, Arg.Any<byte[]>(), mockAdditionalQueryParameters, Arg.Any<int>(), Arg.Any<long>());
        }

        [Test]
        public void BeaconDataPrefixVS2()
        {
            // given
            const int sessionSequence = 1213;
            const int visitStoreVersion = 2;
            const string appVersion = "1111";
            const string ipAddress = "192.168.0.1";
            mockOpenKitConfiguration.ApplicationVersion.Returns(appVersion);
            mockOpenKitConfiguration.OperatingSystem.Returns("system");
            mockOpenKitConfiguration.Manufacturer.Returns("manufacturer");
            mockOpenKitConfiguration.ModelId.Returns("model");
            mockBeaconCache.HasDataForSending(Arg.Any<BeaconKey>()).Returns(true, false);
            mockBeaconCache.GetNextBeaconChunk(Arg.Any<BeaconKey>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<char>())
                .ReturnsNull();
            mockServerConfiguration.VisitStoreVersion.Returns(visitStoreVersion);
            var target = CreateBeacon().WithIpAddress(ipAddress).WithSessionSequenceNumber(sessionSequence).Build();

            // when
            target.Send(Substitute.For<IHttpClientProvider>(), null);

            // then
            var expectedPrefix = $"vv={ProtocolConstants.ProtocolVersion.ToInvariantString()}" +
                $"&va={ProtocolConstants.OpenKitVersion}" +
                $"&ap={AppId}" +
                $"&vn={appVersion}" +
                $"&pt={ProtocolConstants.PlatformTypeOpenKit.ToInvariantString()}" +
                $"&tt={ProtocolConstants.AgentTechnologyType}" +
                $"&vi={DeviceId.ToInvariantString()}" +
                $"&sn={SessionId.ToInvariantString()}" +
                $"&ip={ipAddress}" +
                "&os=system" +
                "&mf=manufacturer" +
                "&md=model" +
                "&dl=2" +
                "&cl=2" +
                $"&vs={visitStoreVersion.ToInvariantString()}" +
                $"&ss={sessionSequence.ToInvariantString()}" +
                "&tx=0" +
                "&tv=0" +
                $"&mp={Multiplicity.ToInvariantString()}";

            var expectedBeaconKey = new BeaconKey(SessionId, sessionSequence);
            mockBeaconCache.Received(1).PrepareDataForSending(expectedBeaconKey);
            mockBeaconCache.Received(1).HasDataForSending(expectedBeaconKey);
            mockBeaconCache.Received(1).GetNextBeaconChunk(
                expectedBeaconKey, expectedPrefix, Arg.Any<int>(), Arg.Any<char>());
        }

        [Test]
        public void ClearDataFromBeaconCache()
        {
            // given
            var action = Substitute.For<IActionInternals>();
            action.Id.Returns(ActionId);
            action.Name.Returns("actionName");

            var beaconCache = new BeaconCache(mockLogger);
            var target = CreateBeacon().With(beaconCache).Build();

            target.AddAction(action);
            target.ReportValue(ActionId, "IntValue", 42);
            target.ReportValue(ActionId, "LongValue", 21L);
            target.ReportValue(ActionId, "DoubleValue", 3.1415);
            target.ReportValue(ActionId, "StringValue", "HelloWorld");
            target.ReportEvent(ActionId, "SomeEvent");
            target.ReportError(ActionId, "SomeError", -123);
            target.ReportError(ActionId, "OtherError", "causeName", "causeReason", "causeStackTrace");
            target.ReportError(ActionId, "Caught Exception", CreateException());
            target.ReportCrash("SomeCrash", "SomeReason", "SomeStacktrace");
            target.ReportCrash(CreateException());
            target.EndSession();

            var beaconKey = new BeaconKey(SessionId, SessionSeqNo);
            Assert.That(beaconCache.GetActions(beaconKey), Is.Not.Empty);
            Assert.That(beaconCache.GetEvents(beaconKey), Is.Not.Empty);

            // when
            target.ClearData();

            // then
            Assert.That(beaconCache.GetActions(beaconKey), Is.Null);
            Assert.That(beaconCache.GetEvents(beaconKey), Is.Null);
        }

        [Test]
        public void DeviceIdIsRandomizedIfDeviceIdSendingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsDeviceIdSendingAllowed.Returns(false);
            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            _ = target.DeviceId;

            // then
            _ = mockOpenKitConfiguration.Received(0).DeviceId;
            mockRandomGenerator.Received(1).NextPositiveLong();
        }

        [Test]
        public void GivenDeviceIdIsUsedOnIfDeviceIdSendingIsAllowed()
        {
            // given
            const long deviceId = 999;
            mockPrivacyConfiguration.IsDeviceIdSendingAllowed.Returns(true);
            mockOpenKitConfiguration.DeviceId.Returns(deviceId);
            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            var obtained = target.DeviceId;

            // then
            _ = mockOpenKitConfiguration.Received(1).DeviceId;
            mockRandomGenerator.Received(0).NextPositiveLong();
            Assert.That(obtained, Is.EqualTo(deviceId));
        }

        [Test]
        public void SessionIdIsAlwaysValueOneIfSessionNumberReportingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsSessionNumberReportingAllowed.Returns(false);

            var target = CreateBeacon().Build();

            // when
            var sessionId = target.SessionNumber;

            // then
            Assert.That(sessionId, Is.EqualTo(1));
        }

        [Test]
        public void SessionIdIsValueFromSessionIdProviderIfSessionNumberReportingAllowed()
        {
            // given
            const int sessionId = 1234;
            mockSessionIdProvider.GetNextSessionId().Returns(sessionId);
            mockPrivacyConfiguration.IsSessionReportingAllowed.Returns(true);

            var target = CreateBeacon().Build();

            // when
            var obtained = target.SessionNumber;

            // then
            Assert.That(obtained, Is.EqualTo(sessionId));
            _ = mockSessionIdProvider.Received(1).GetNextSessionId();
        }

        [Test]
        public void InitializeServerConfigurationDelegatesToBeacon()
        {
            // given
            var target = CreateBeacon().Build();
            var serverConfig = Substitute.For<IServerConfiguration>();

            // when
            target.InitializeServerConfiguration(serverConfig);

            // then
            mockBeaconConfiguration.Received(1).InitializeServerConfiguration(serverConfig);
            Assert.That(serverConfig.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void UpdateServerConfigurationDelegatesToBeaconConfig()
        {
            // given
            var target = CreateBeacon().Build();
            var serverConfig = Substitute.For<IServerConfiguration>();

            // when
            target.UpdateServerConfiguration(serverConfig);

            // then
            mockBeaconConfiguration.Received(1).UpdateServerConfiguration(serverConfig);
            Assert.That(serverConfig.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void IsServerConfigurationSetDelegatesToBeaconConfig()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            mockBeaconConfiguration.IsServerConfigurationSet.Returns(false);
            var obtained = target.IsServerConfigurationSet;

            // then
            Assert.That(obtained, Is.False);

            // and when
            mockBeaconConfiguration.IsServerConfigurationSet.Returns(true);
            obtained = target.IsServerConfigurationSet;

            // then
            Assert.That(obtained, Is.True);
        }

        [Test]
        public void IsActionReportingAllowedByPrivacySettingsDelegatesToPrivacyConfig()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            mockPrivacyConfiguration.IsActionReportingAllowed.Returns(true);
            var obtained = target.IsActionReportingAllowedByPrivacySettings;

            // then
            Assert.That(obtained, Is.True);

            // and when
            mockPrivacyConfiguration.IsActionReportingAllowed.Returns(false);
            obtained = target.IsActionReportingAllowedByPrivacySettings;

            // then
            Assert.That(obtained, Is.False);

            // ensure calls
            var _ = mockPrivacyConfiguration.Received(2).IsActionReportingAllowed;
        }

        [Test]
        public void IsDataCapturingEnabledReturnsFalseIfDataSendingIsDisallowed()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);
            var obtained = target.IsDataCapturingEnabled;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void IsDataCapturingEnabledReturnsFalseIfTcValueEqualToTcPercentageFromServerConfig()
        {
            // given
            const int trafficControlValue = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlValue);
            
            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            mockServerConfiguration.IsSendingDataAllowed.Returns(true);
            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlValue);
            var obtained = target.IsDataCapturingEnabled;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void IsDataCapturingEnabledReturnsFalseIfTcValueGreaterThanTcPercentageFromServerConfig()
        {
            // given
            const int trafficControlValue = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlValue);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            mockServerConfiguration.IsSendingDataAllowed.Returns(true);
            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlValue - 1);
            var obtained = target.IsDataCapturingEnabled;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void IsDataCapturingEnabledReturnsTrueIfDataSendingIsAllowedAndTcValueGreaterThanTcPercentageFromServerConfig()
        {
            // given
            const int trafficControlValue = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlValue);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            mockServerConfiguration.IsCaptureEnabled.Returns(true);
            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlValue + 1);
            var obtained = target.IsDataCapturingEnabled;

            // then
            Assert.That(obtained, Is.True);
        }

        [Test]
        public void IsErrorCapturingEnabledReturnsFalseIfSendingErrorsIsDisallowed()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            mockServerConfiguration.IsSendingErrorsAllowed.Returns(false);
            var obtained = target.IsErrorCapturingEnabled;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void IsErrorCapturingEnabledReturnsFalseIfTcValueEqualToTcPercentageFromServerConfig()
        {
            // given
            const int trafficControlValue = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlValue);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            mockServerConfiguration.IsSendingErrorsAllowed.Returns(true);
            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlValue);
            var obtained = target.IsErrorCapturingEnabled;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void IsErrorCapturingEnabledReturnsFalseIfTcValueGreaterThanTcPercentageFromServerConfig()
        {
            // given
            const int trafficControlValue = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlValue);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            mockServerConfiguration.IsSendingErrorsAllowed.Returns(true);
            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlValue - 1);
            var obtained = target.IsErrorCapturingEnabled;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void IsErrorCapturingEnabledReturnsTrueIfDataSendingIsAllowedAndTcValueGreaterThanTcPercentageFromServerConfig()
        {
            // given
            const int trafficControlValue = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlValue);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            mockServerConfiguration.IsSendingErrorsAllowed.Returns(true);
            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlValue + 1);
            var obtained = target.IsErrorCapturingEnabled;

            // then
            Assert.That(obtained, Is.True);
        }

        [Test]
        public void IsCrashCapturingEnabledReturnsFalseIfSendingErrorsIsDisallowed()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            mockServerConfiguration.IsSendingCrashesAllowed.Returns(false);
            var obtained = target.IsCrashCapturingEnabled;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void IsCrashCapturingEnabledReturnsFalseIfTcValueEqualToTcPercentageFromServerConfig()
        {
            // given
            const int trafficControlValue = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlValue);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            mockServerConfiguration.IsSendingCrashesAllowed.Returns(true);
            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlValue);
            var obtained = target.IsCrashCapturingEnabled;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void IsCrashCapturingEnabledReturnsFalseIfTcValueGreaterThanTcPercentageFromServerConfig()
        {
            // given
            const int trafficControlValue = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlValue);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            mockServerConfiguration.IsSendingCrashesAllowed.Returns(true);
            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlValue - 1);
            var obtained = target.IsCrashCapturingEnabled;

            // then
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void IsCrashCapturingEnabledReturnsTrueIfDataSendingIsAllowedAndTcValueGreaterThanTcPercentageFromServerConfig()
        {
            // given
            const int trafficControlValue = 50;

            var mockRandomGenerator = Substitute.For<IPrnGenerator>();
            mockRandomGenerator.NextPercentageValue().Returns(trafficControlValue);

            var target = CreateBeacon().With(mockRandomGenerator).Build();

            // when
            mockServerConfiguration.IsSendingCrashesAllowed.Returns(true);
            mockServerConfiguration.TrafficControlPercentage.Returns(trafficControlValue + 1);
            var obtained = target.IsCrashCapturingEnabled;

            // then
            Assert.That(obtained, Is.True);
        }

        [Test]
        public void EnableCaptureDelegatesToBeaconConfig()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            target.EnableCapture();

            // then
            mockBeaconConfiguration.Received(1).EnableCapture();
        }

        [Test]
        public void DisableCaptureDelegatesToBeaconConfig()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            target.DisableCapture();

            // then
            mockBeaconConfiguration.Received(1).DisableCapture();
        }

        [Test]
        public void UseInternalBeaconIdForAccessingBeaconCacheWhenSessionNumberReportingDisallowed()
        {
            // given
            var beaconKey = new BeaconKey(99, SessionSeqNo);
            mockSessionIdProvider.GetNextSessionId().Returns(beaconKey.BeaconId);
            mockPrivacyConfiguration.IsSessionNumberReportingAllowed.Returns(false);

            var target = CreateBeacon().Build();

            // when
            target.ClearData();

            // then
            Assert.That(target.SessionNumber, Is.EqualTo(1));
            mockBeaconCache.Received(1).DeleteCacheEntry(beaconKey);
        }

        [Test]
        public void SendConstructsCorrectBeaconPrefixVisitStore1()
        {
            // given
            var httpClient = Substitute.For<IHttpClient>();
            var httpClientProvider = Substitute.For<IHttpClientProvider>();
            httpClientProvider.CreateClient(Arg.Any<HttpClientConfiguration>()).Returns(httpClient);
            mockServerConfiguration.VisitStoreVersion.Returns(1);

            mockBeaconCache.HasDataForSending(Arg.Any<BeaconKey>()).Returns(true, false);
            mockBeaconCache.GetNextBeaconChunk(Arg.Any<BeaconKey>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<char>())
                .ReturnsNull();

            var target = CreateBeacon().WithSessionSequenceNumber(0).Build();

            // when
            var response = target.Send(httpClientProvider, mockAdditionalQueryParameters);

            // then
            Assert.That(response, Is.Null);

            var expectedPrefix =
                $"vv={ProtocolConstants.ProtocolVersion.ToInvariantString()}" +
                $"&va={ProtocolConstants.OpenKitVersion}" +
                $"&ap={AppId}" +
                $"&vn={AppVersion}" +
                $"&pt={ProtocolConstants.PlatformTypeOpenKit.ToInvariantString()}" +
                $"&tt={ProtocolConstants.AgentTechnologyType}" +
                $"&vi={DeviceId.ToInvariantString()}" +
                $"&sn={SessionId.ToInvariantString()}" +
                "&ip=127.0.0.1" +
                $"&os={string.Empty}" +
                $"&mf={string.Empty}" +
                $"&md={string.Empty}" +
                $"&dl={((int)ConfigurationDefaults.DefaultDataCollectionLevel).ToInvariantString()}" +
                $"&cl={((int)ConfigurationDefaults.DefaultCrashReportingLevel).ToInvariantString()}" +
                "&vs=1" +
                "&tx=0" +
                "&tv=0" +
                $"&mp={Multiplicity.ToInvariantString()}";

            var expectedBeaconKey = new BeaconKey(SessionId, 0);
            mockBeaconCache.Received(1).PrepareDataForSending(expectedBeaconKey);
            mockBeaconCache.Received(1).HasDataForSending(expectedBeaconKey);
            mockBeaconCache.Received(1).GetNextBeaconChunk(
                expectedBeaconKey, expectedPrefix, Arg.Any<int>(), Arg.Any<char>());
        }

        [Test]
        public void OnServerConfigurationUpdateAttachesEventOnBeaconConfiguration()
        {
            // given
            var sessionProxy = Substitute.For<ISessionProxy>();
            var target = CreateBeacon().Build();

            // when
            target.OnServerConfigurationUpdate += sessionProxy.OnServerConfigurationUpdate;

            // then
            mockBeaconConfiguration.Received(1).OnServerConfigurationUpdate += sessionProxy.OnServerConfigurationUpdate;
        }

        [Test]
        public void OnServerConfigurationUpdateDetachesEventOnBeaconConfiguration()
        {
            // given
            var sessionProxy = Substitute.For<ISessionProxy>();
            var target = CreateBeacon().Build();

            // when
            target.OnServerConfigurationUpdate -= sessionProxy.OnServerConfigurationUpdate;

            // then
            mockBeaconConfiguration.Received(1).OnServerConfigurationUpdate -= sessionProxy.OnServerConfigurationUpdate;
        }

        #endregion

        private TestBeaconBuilder CreateBeacon()
        {
            return new TestBeaconBuilder()
                    .With(mockLogger)
                    .With(mockBeaconCache)
                    .With(mockBeaconConfiguration)
                    .WithIpAddress("127.0.0.1")
                    .With(mockSessionIdProvider)
                    .With(mockThreadIdProvider)
                    .With(mockTimingProvider)
                    .WithSessionSequenceNumber(SessionSeqNo)
                    .With(mockSupplementaryBasicData)
                ;
        }

        private TestWebRequestTracerBuilder CreateWebRequestTracer(IBeacon beacon)
        {
            return new TestWebRequestTracerBuilder()
                    .With(beacon)
                    .With(mockLogger)
                    .With(mockParent)
                ;
        }

        private static Exception CreateException()
        {
            try
            {
                // need to throw an exception to populate the stack trace
                throw new InvalidOperationException();
            }
            catch(Exception caught)
            {
                return caught;
            }
        }
    }
}