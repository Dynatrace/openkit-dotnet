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
using System.Collections.Generic;
using System.Globalization;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Providers;
using Dynatrace.OpenKit.Util;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Protocol
{
    public class BeaconTest
    {
        private const string AppId = "appID";
        private const string AppName = "appName";
        private const string AppVersion = "1.0";
        private const int ActionId = 17;
        private const int ServerId = 123;
        private const long DeviceId = 456;
        private const int ThreadId = 1234567;
        private const int SessionId = 73;
        private const int SessionSeqNo = 13;
        private const int Multiplicity = 1;

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

#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
        private CultureInfo currentCulture;
#endif

        [SetUp]
        public void Setup()
        {
            mockOpenKitConfiguration = Substitute.For<IOpenKitConfiguration>();
            mockOpenKitConfiguration.ApplicationId.Returns(AppId);
            mockOpenKitConfiguration.ApplicationIdPercentEncoded.Returns(AppId);
            mockOpenKitConfiguration.ApplicationName.Returns(AppName);
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


        [Test]
        public void DefaultBeaconConfigurationDoesNotDisableCapturing()
        {
            // given
            var target = CreateBeacon().Build();

            // then
            Assert.That(target.IsCaptureEnabled, Is.True);
        }

        [Test]
        public void CreateInstanceWithInvalidIpAddress()
        {
            // given when
            string capturedIpAddress = null;
            mockLogger.IsWarnEnabled.Returns(true);
            var httpClient = Substitute.For<IHttpClient>();
            httpClient.SendBeaconRequest(Arg.Do<string>(c => capturedIpAddress = c), Arg.Any<byte[]>(),
                    Arg.Any<IAdditionalQueryParameters>())
                .Returns(null as IStatusResponse);

            var httpClientProvider = Substitute.For<IHttpClientProvider>();
            httpClientProvider.CreateClient(Arg.Any<IHttpClientConfiguration>()).Returns(httpClient);

            const string ipAddress = "invalid";

            var target = CreateBeacon()
                .WithIpAddress(ipAddress)
                .Build();

            // then
            mockLogger.Received(1).Warn($"Beacon: Client IP address validation failed: {ipAddress}");

            mockBeaconCache.GetNextBeaconChunk(Arg.Any<BeaconKey>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<char>())
                .Returns("dummy");

            // when
            target.Send(httpClientProvider, mockAdditionalQueryParameters);

            // then
            Assert.That(capturedIpAddress, Is.Empty);
            httpClient.Received(1)
                .SendBeaconRequest(capturedIpAddress, Arg.Any<byte[]>(), mockAdditionalQueryParameters);
        }

        [Test]
        public void CreateInstanceWithNullIpAddress()
        {
            // given when
            string capturedIpAddress = null;
            mockLogger.IsWarnEnabled.Returns(true);
            var httpClient = Substitute.For<IHttpClient>();
            httpClient.SendBeaconRequest(Arg.Do<string>(c => capturedIpAddress = c), Arg.Any<byte[]>(),
                    Arg.Any<IAdditionalQueryParameters>())
                .Returns(null as IStatusResponse);

            var httpClientProvider = Substitute.For<IHttpClientProvider>();
            httpClientProvider.CreateClient(Arg.Any<IHttpClientConfiguration>()).Returns(httpClient);

            var target = CreateBeacon()
                .WithIpAddress(null)
                .Build();

            // then
            mockLogger.Received(0).Warn(Arg.Any<string>());

            mockBeaconCache.GetNextBeaconChunk(Arg.Any<BeaconKey>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<char>())
                .Returns("dummy");

            // when
            target.Send(httpClientProvider, mockAdditionalQueryParameters);

            // then
            Assert.That(capturedIpAddress, Is.Not.Null);
            Assert.That(capturedIpAddress, Is.EqualTo(string.Empty));
            httpClient.Received(1)
                .SendBeaconRequest(capturedIpAddress, Arg.Any<byte[]>(), mockAdditionalQueryParameters);
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
                + $"&na={actionName}"                                 // action name
                + $"&it={ThreadId.ToInvariantString()}"               // thread ID
                + $"&ca={ActionId.ToInvariantString()}"               // action ID
                + $"&pa={parentId.ToInvariantString()}"               // parent action ID
                +  "&s0=0"                                            // action start sequence number
                +  "&t0=0"                                            // action start time
                +  "&s1=0"                                            // action end sequence number
                +  "&t1=0"                                            // action end time
                );
        }

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
                +  "&pa=0"                                                    // parent action ID
                +  "&s0=1"                                                    // session end sequence number
                +  "&t0=0"                                                    // session end time
                );
        }

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
                + $"&na={valueName}"                                    // action name
                + $"&it={ThreadId.ToInvariantString()}"                 // thread ID
                + $"&pa={ActionId.ToInvariantString()}"                 // parent action ID
                +  "&s0=1"                                              // event sequence number
                +  "&t0=0"                                              // event timestamp
                + $"&vl={value.ToInvariantString()}"                    // reported value
                );
        }

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
                + $"&na={valueName}"                                       // action name
                + $"&it={ThreadId.ToInvariantString()}"                    // thread ID
                + $"&pa={ActionId.ToInvariantString()}"                    // parent action ID
                +  "&s0=1"                                                 // event sequence number
                +  "&t0=0"                                                 // event timestamp
                + $"&vl={value.ToInvariantString()}"                       // reported value
                );
        }

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
                + $"&na={valueName}"                                       // action name
                + $"&it={ThreadId.ToInvariantString()}"                    // thread ID
                + $"&pa={ActionId.ToInvariantString()}"                    // parent action ID
                +  "&s0=1"                                                 // event sequence number
                +  "&t0=0"                                                 // event timestamp
                + $"&vl={value}"                                           // reported value
                );
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
                + $"&na={valueName}"                                       // action name
                + $"&it={ThreadId.ToInvariantString()}"                    // thread ID
                + $"&pa={ActionId.ToInvariantString()}"                    // parent action ID
                +  "&s0=1"                                                 // event sequence number
                +  "&t0=0"                                                 // event timestamp
                );
        }

        [Test]
        public void ReportValueStringWithValueNullAndNameNull()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, null, null);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                    // beacon key
                0,                                                         // timestamp
                $"et={EventType.VALUE_STRING.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                    // thread ID
                + $"&pa={ActionId.ToInvariantString()}"                    // parent action ID
                +  "&s0=1"                                                 // event sequence number
                +  "&t0=0"                                                 // event timestamp
                );
        }

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
                + $"&na={eventName}"                                      // action name
                + $"&it={ThreadId.ToInvariantString()}"                   // thread ID
                + $"&pa={ActionId.ToInvariantString()}"                   // parent action ID
                +  "&s0=1"                                                // event sequence number
                +  "&t0=0"                                                // event timestamp
                );
        }

        [Test]
        public void ReportEventWithNameNull()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            target.ReportEvent(ActionId, null);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.NAMED_EVENT.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"                   // thread ID
                + $"&pa={ActionId.ToInvariantString()}"                   // parent action ID
                +  "&s0=1"                                                // event sequence number
                +  "&t0=0"                                                // event timestamp
                );
        }

        [Test]
        public void ReportError()
        {
            // given
            const string errorName = "someError";
            const int errorCode = -123;
            const string reason = "someReason";

            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, errorName, errorCode, reason);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),             // beacon key
                0,                                                  // timestamp
                $"et={EventType.ERROR.ToInt().ToInvariantString()}" // event type
                + $"&na={errorName}"                                // action name
                + $"&it={ThreadId.ToInvariantString()}"             // thread ID
                + $"&pa={ActionId.ToInvariantString()}"             // parent action ID
                +  "&s0=1"                                          // event sequence number
                +  "&t0=0"                                          // event timestamp
                + $"&ev=-123"                                       // reported error code
                + $"&rs={reason}"                                   // error reason
                +  "&tt=c"                                          // error technology type
                );
        }

        [Test]
        public void ReportErrorNull()
        {
            // given
            const int errorCode = -123;
            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, null, errorCode, null);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),             // beacon key
                0,                                                  // timestamp
                $"et={EventType.ERROR.ToInt().ToInvariantString()}" // event type
                + $"&it={ThreadId.ToInvariantString()}"             // thread ID
                + $"&pa={ActionId.ToInvariantString()}"             // parent action ID
                +  "&s0=1"                                          // event sequence number
                +  "&t0=0"                                          // event timestamp
                + $"&ev=-123"                                       // reported error code
                +  "&tt=c"                                          // error technology type
                );
        }

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
                + $"&na={errorName}"                                // action name
                + $"&it={ThreadId.ToInvariantString()}"             // thread ID
                +  "&pa=0"                                          // parent action ID
                +  "&s0=1"                                          // event sequence number
                +  "&t0=0"                                          // event timestamp
                + $"&rs={reason}"                                   // error reason
                + $"&st={stacktrace}"                               // reported stacktrace
                +  "&tt=c"                                          // error technology type
                );
        }

        [Test]
        public void ReportCrashWithDetailsNull()
        {
            // given
            const string errorName = "someError";

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash(errorName, null, null);

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),             // beacon key
                0,                                                  // timestamp
                $"et={EventType.CRASH.ToInt().ToInvariantString()}" // event type
                + $"&na={errorName}"                                // action name
                + $"&it={ThreadId.ToInvariantString()}"             // thread ID
                +  "&pa=0"                                          // parent action ID
                +  "&s0=1"                                          // event sequence number
                +  "&t0=0"                                          // event timestamp
                +  "&tt=c"                                          // error technology type
                );
        }

        [Test]
        public void AddWebRequest()
        {
            // given
            var target = CreateBeacon().Build();

            const int sentBytes = 1337;
            const int receivedBytes = 1447;
            const int responseCode = 418;
            var tracer = Substitute.For<IWebRequestTracerInternals>();
            tracer.Url.Returns((string) null);
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
        public void AddUserIdentifyEvent()
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
                + $"&na={userId}"                                           // number bytes received
                + $"&it={ThreadId.ToInvariantString()}"                     // thread ID
                +  "&pa=0"                                                  // parent action ID
                +  "&s0=1"                                                  // web request start sequence number
                +  "&t0=0"                                                  // web request start timestamp
                );
        }

        [Test]
        public void AddUserIdentifyWithNullUserIdEvent()
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
        public void CanAddSentBytesToWebRequestTracer()
        {
            //given
            const string testUrl = "https://127.0.0.1";
            const int sentBytes = 123;
            mockParent.ActionId.Returns(ActionId);

            var target = CreateBeacon().Build();

            var webRequest = CreateWebRequestTracer(target).WithUrl(testUrl).Build();

            // when
            webRequest.Start().SetBytesSent(sentBytes).Stop(-1); //stop will add the web request to the beacon

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt().ToInvariantString()}" // event type
                + $"&na={Uri.EscapeDataString(testUrl)}"                  // traced URL
                + $"&it={ThreadId.ToInvariantString()}"                   // thread ID
                + $"&pa={ActionId.ToInvariantString()}"                   // parent action ID
                +  "&s0=1"                                                // web request start sequence number
                +  "&t0=0"                                                // web request start timestamp
                +  "&s1=2"                                                // web request end sequence number
                +  "&t1=0"                                                // web request end timestamp
                + $"&bs={sentBytes.ToInvariantString()}"                  // number bytes sent
                );
        }

        [Test]
        public void CanAddSentBytesValueZeroToWebRequestTracer()
        {
            //given
            const string testUrl = "https://127.0.0.1";
            const int sentBytes = 0;
            mockParent.ActionId.Returns(ActionId);

            var target = CreateBeacon().Build();

            var webRequest = CreateWebRequestTracer(target).WithUrl(testUrl).Build();

            // when
            webRequest.Start().SetBytesSent(sentBytes).Stop(-1); //stop will add the web request to the beacon

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt().ToInvariantString()}" // event type
                + $"&na={Uri.EscapeDataString(testUrl)}"                  // traced URL
                + $"&it={ThreadId.ToInvariantString()}"                   // thread ID
                + $"&pa={ActionId.ToInvariantString()}"                   // parent action ID
                +  "&s0=1"                                                // web request start sequence number
                +  "&t0=0"                                                // web request start timestamp
                +  "&s1=2"                                                // web request end sequence number
                +  "&t1=0"                                                // web request end timestamp
                + $"&bs={sentBytes.ToInvariantString()}"                  // number bytes sent
                );
        }

        [Test]
        public void CannotAddSentBytesWithInvalidValueSmallerZeroToWebRequestTracer()
        {
            //given
            const string testUrl = "https://127.0.0.1";
            mockParent.ActionId.Returns(ActionId);

            var target = CreateBeacon().Build();

            var webRequest = CreateWebRequestTracer(target).WithUrl(testUrl).Build();

            // when
            webRequest.Start().SetBytesSent(-1).Stop(-1); //stop will add the web request to the beacon

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt().ToInvariantString()}" // event type
                + $"&na={Uri.EscapeDataString(testUrl)}"                  // traced URL
                + $"&it={ThreadId.ToInvariantString()}"                   // thread ID
                + $"&pa={ActionId.ToInvariantString()}"                   // parent action ID
                +  "&s0=1"                                                // web request start sequence number
                +  "&t0=0"                                                // web request start timestamp
                +  "&s1=2"                                                // web request end sequence number
                +  "&t1=0"                                                // web request end timestamp
                );
        }

        [Test]
        public void CanAddReceivedBytesToWebRequestTracer()
        {
            //given
            const string testUrl = "https://127.0.0.1";
            const int receivedBytes = 12321;
            mockParent.ActionId.Returns(ActionId);

            var target = CreateBeacon().Build();

            var webRequest = CreateWebRequestTracer(target).WithUrl(testUrl).Build();

            // when
            webRequest.Start().SetBytesReceived(receivedBytes).Stop(-1); //stop will add the web request to the beacon

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                    // beacon key
                0,                                                         // timestamp
                $"et={EventType.WEB_REQUEST.ToInt().ToInvariantString()}"  // event type
                + $"&na={Uri.EscapeDataString(testUrl)}"                   // traced URL
                + $"&it={ThreadId.ToInvariantString()}"                    // thread ID
                + $"&pa={ActionId.ToInvariantString()}"                    // parent action ID
                +  "&s0=1"                                                 // web request start sequence number
                +  "&t0=0"                                                 // web request start timestamp
                +  "&s1=2"                                                 // web request end sequence number
                +  "&t1=0"                                                 // web request end timestamp
                + $"&br={receivedBytes.ToInvariantString()}"               // number bytes received
                );
        }

        [Test]
        public void CanAddReceivedBytesValueZeroToWebRequestTracer()
        {
            //given
            const string testUrl = "https://127.0.0.1";
            const int receivedBytes = 0;
            mockParent.ActionId.Returns(ActionId);

            var target = CreateBeacon().Build();

            var webRequest = CreateWebRequestTracer(target).WithUrl(testUrl).Build();

            // when
            webRequest.Start().SetBytesReceived(receivedBytes).Stop(-1); //stop will add the web request to the beacon

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt().ToInvariantString()}" // event type
                + $"&na={Uri.EscapeDataString(testUrl)}"                  // traced URL
                + $"&it={ThreadId.ToInvariantString()}"                   // thread ID
                + $"&pa={ActionId.ToInvariantString()}"                   // parent action ID
                +  "&s0=1"                                                // web request start sequence number
                +  "&t0=0"                                                // web request start timestamp
                +  "&s1=2"                                                // web request end sequence number
                +  "&t1=0"                                                // web request end timestamp
                + $"&br={receivedBytes.ToInvariantString()}"              // number bytes received
                );
        }

        [Test]
        public void CannotAddReceivedBytesWithInvalidValueSmallerZeroToWebRequestTracer()
        {
            //given
            const string testUrl = "https://127.0.0.1";
            mockParent.ActionId.Returns(ActionId);

            var target = CreateBeacon().Build();

            var webRequest = CreateWebRequestTracer(target).WithUrl(testUrl).Build();

            // when
            webRequest.Start().SetBytesReceived(-1).Stop(-1); //stop will add the web request to the beacon

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt().ToInvariantString()}" // event type
                + $"&na={Uri.EscapeDataString(testUrl)}"                  // traced URL
                + $"&it={ThreadId.ToInvariantString()}"                   // thread ID
                + $"&pa={ActionId.ToInvariantString()}"                   // parent action ID
                +  "&s0=1"                                                // web request start sequence number
                +  "&t0=0"                                                // web request start timestamp
                +  "&s1=2"                                                // web request end sequence number
                +  "&t1=0"                                                // web request end timestamp
                );
        }

        [Test]
        public void CanAddBothSentBytesAndReceivedBytesToWebRequestTracer()
        {
            //given
            const string testUrl = "https://127.0.0.1";
            const int receivedBytes = 12321;
            const int sentBytes = 123;
            mockParent.ActionId.Returns(ActionId);

            var target = CreateBeacon().Build();

            var webRequest = CreateWebRequestTracer(target).WithUrl(testUrl).Build();

            // when
            webRequest.Start().SetBytesSent(sentBytes).SetBytesReceived(receivedBytes)
                .Stop(-1); //stop will add the web request to the beacon

            // then
            mockBeaconCache.Received(1).AddEventData(
                new BeaconKey(SessionId, SessionSeqNo),                   // beacon key
                0,                                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt().ToInvariantString()}" // event type
                + $"&na={Uri.EscapeDataString(testUrl)}"                  // traced URL
                + $"&it={ThreadId.ToInvariantString()}"                   // thread ID
                + $"&pa={ActionId.ToInvariantString()}"                   // parent action ID
                +  "&s0=1"                                                // web request start sequence number
                +  "&t0=0"                                                // web request start timestamp
                +  "&s1=2"                                                // web request end sequence number
                +  "&t1=0"                                                // web request end timestamp
                + $"&bs={sentBytes.ToInvariantString()}"                  // number bytes sent
                + $"&br={receivedBytes.ToInvariantString()}"              // number bytes received
                );
        }

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
            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<IAdditionalQueryParameters>())
                .Returns(successResponse);

            var httpClientProvider = Substitute.For<IHttpClientProvider>();
            httpClientProvider.CreateClient(Arg.Any<IHttpClientConfiguration>()).Returns(httpClient);

            // when
            target.ReportCrash("errorName", "errorReason", "stackTrace");
            var response = target.Send(httpClientProvider, mockAdditionalQueryParameters);

            // then
            Assert.That(response, Is.Not.Null);
            Assert.That(response.ResponseCode, Is.EqualTo(responseCode));
            httpClient.Received(1).SendBeaconRequest(ipAddress, Arg.Any<byte[]>(), mockAdditionalQueryParameters);
        }

        [Test]
        public void SendDataAndFakeErrorResponse()
        {
            // given
            const string ipAddress = "127.0.0.1";
            const int responseCode = 418;

            var target = CreateBeacon().With(new BeaconCache(mockLogger)).WithIpAddress(ipAddress).Build();

            var httpClient = Substitute.For<IHttpClient>();
            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<IAdditionalQueryParameters>())
                .Returns(StatusResponse.CreateErrorResponse(mockLogger, responseCode));

            var httpClientProvider = Substitute.For<IHttpClientProvider>();
            httpClientProvider.CreateClient(Arg.Any<IHttpClientConfiguration>()).Returns(httpClient);

            // when
            target.ReportCrash("errorName", "errorReason", "stackTrace");
            var response = target.Send(httpClientProvider, mockAdditionalQueryParameters);

            // then
            Assert.That(response, Is.Not.Null);
            Assert.That(response.ResponseCode, Is.EqualTo(responseCode));
            httpClient.Received(1).SendBeaconRequest(ipAddress, Arg.Any<byte[]>(), mockAdditionalQueryParameters);
        }

        [Test]
        public void BeaconDataPrefix()
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
                $"&an={AppName}" +
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

            mockBeaconCache.Received(1)
                .GetNextBeaconChunk(new BeaconKey(SessionId, sessionSequence), expectedPrefix, Arg.Any<int>(),
                    Arg.Any<char>());
        }

        [Test]
        public void ClearDataFromBeaconCache()
        {
            // given
            var action = Substitute.For<IActionInternals>();
            action.Id.Returns(ActionId);

            var beaconCache = new BeaconCache(mockLogger);
            var target = CreateBeacon().With(beaconCache).Build();

            target.AddAction(action);
            target.ReportValue(ActionId, "IntValue", 42);
            target.ReportValue(ActionId, "DoubleValue", 3.1415);
            target.ReportValue(ActionId, "StringValue", "HelloWorld");
            target.ReportEvent(ActionId, "SomeEvent");
            target.ReportError(ActionId, "SomeError", -123, "SomeReason");
            target.ReportCrash("SomeCrash", "SomeReason", "SomeStacktrace");
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
        public void NoSessionIsAddedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            mockServerConfiguration.IsCaptureEnabled.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.EndSession();

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoActionIsAddedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            mockServerConfiguration.IsCaptureEnabled.Returns(false);
            var action = Substitute.For<IActionInternals>();
            action.Id.Returns(ActionId);

            var target = CreateBeacon().Build();

            // when
            target.AddAction(action);

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoIntValueIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            mockServerConfiguration.IsCaptureEnabled.Returns(false);
            const int intValue = 42;

            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "intValue", intValue);

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoDoubleValueIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            mockServerConfiguration.IsCaptureEnabled.Returns(false);
            const double doubleValue = Math.E;

            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "doubleValue", doubleValue);

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoStringValueIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            mockServerConfiguration.IsCaptureEnabled.Returns(false);
            const string stringValue = "Write once, debug everywhere";

            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "doubleValue", stringValue);

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoEventIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            mockServerConfiguration.IsCaptureEnabled.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportEvent(ActionId, "Event name");

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoEventIsReportedIfDataSendingDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportEvent(ActionId, "Event name");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoErrorIsReportedIfCapturingDisabled()
        {
            // given
            mockServerConfiguration.IsCaptureEnabled.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, "Error name", 123, "The reason for this error");

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoErrorIsReportedIfSendingErrorDataDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingErrorsAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, "Error name", 123, "The reason for this error");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoCrashIsReportedIfCapturingDisabled()
        {
            // given
            mockServerConfiguration.IsCaptureEnabled.Returns(false);

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash("Error name", "The reason for this error", "the stack trace");

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoCrashIsReportedIfSendingCrashDataDisallowed()
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
        public void NoWebRequestIsReportedIfCapturingDisabled()
        {
            // given
            mockServerConfiguration.IsCaptureEnabled.Returns(false);
            var target = CreateBeacon().Build();
            var webRequestTracer = CreateWebRequestTracer(target).WithUrl("https://foo.bar").Build();

            // when
            target.AddWebRequest(ActionId, webRequestTracer);

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoUserIdentificationIsReportedIfCapturingDisabled()
        {
            // given
            mockServerConfiguration.IsCaptureEnabled.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.IdentifyUser("jane.doe@acme.com");

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoWebRequestIsReportedIfWebRequestTracingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsWebRequestTracingAllowed.Returns(false);
            var webRequestTracer = Substitute.For<IWebRequestTracerInternals>();

            var target = CreateBeacon().Build();

            // when
            target.AddWebRequest(ActionId, webRequestTracer);

            // then ensure nothing has been serialized
            Assert.That(webRequestTracer.ReceivedCalls(), Is.Empty);
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void WebRequestIsReportedIfWebRequestTracingAllowed()
        {
            // given
            mockPrivacyConfiguration.IsWebRequestTracingAllowed.Returns(true);
            var webRequestTracer = Substitute.For<IWebRequestTracerInternals>();

            var target = CreateBeacon().Build();

            // when
            target.AddWebRequest(ActionId, webRequestTracer);


            // then ensure nothing has been serialized
            _ = webRequestTracer.Received(1).BytesReceived;
            _ = webRequestTracer.Received(1).BytesSent;
            _ = webRequestTracer.Received(1).ResponseCode;
            Assert.That(target.IsEmpty, Is.False);
            mockBeaconCache.Received(1).AddEventData(Arg.Any<BeaconKey>(), Arg.Any<long>(), Arg.Any<string>());
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
        public void BeaconReturnsValidTagWithSessionNumberIfSessionNumberReportingAllowed()
        {
            // given
            const int sequenceNo = 1;
            mockPrivacyConfiguration.IsSessionNumberReportingAllowed.Returns(true);
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
        public void CanIdentifyUserIfUserIdentificationIsAllowed()
        {
            // given
            mockPrivacyConfiguration.IsUserIdentificationIsAllowed.Returns(true);

            var target = CreateBeacon().Build();

            // when
            target.IdentifyUser("test user");

            // then
            mockBeaconCache.Received(1).AddEventData(Arg.Any<BeaconKey>(), Arg.Any<long>(), Arg.Any<string>());
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
            Assert.That(mockRandomGenerator.ReceivedCalls(), Is.Empty);
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
        public void ReportCrashDoesReportIfCrashReportingAllowed()
        {
            // given
            mockPrivacyConfiguration.IsCrashReportingAllowed.Returns(true);

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash("OutOfMemory exception", "insufficient memory", "stacktrace:123");

            // then
            mockBeaconCache.Received(1).AddEventData(Arg.Any<BeaconKey>(), Arg.Any<long>(), Arg.Any<string>());
        }

        [Test]
        public void ActionNotReportedIfActionReportingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsActionReportingAllowed.Returns(false);
            var action = Substitute.For<IActionInternals>();

            var target = CreateBeacon().Build();

            // when
            target.AddAction(action);

            // then
            Assert.That(action.ReceivedCalls(), Is.Empty);
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ActionNotReportedIfDataSendingDisallowed()
        {
            // given
            mockServerConfiguration.IsSendingDataAllowed.Returns(false);
            var action = Substitute.For<IActionInternals>();
            var target = CreateBeacon().Build();

            // when
            target.AddAction(action);

            // then
            Assert.That(action.ReceivedCalls(), Is.Empty);
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ActionReportedIfActionReportingAllowed()
        {
            // given
            mockPrivacyConfiguration.IsActionReportingAllowed.Returns(true);
            var action = Substitute.For<IActionInternals>();
            action.Id.Returns(ActionId);

            var target = CreateBeacon().Build();

            // when
            target.AddAction(action);

            // then
            mockBeaconCache.Received(1).AddActionData(Arg.Any<BeaconKey>(), Arg.Any<long>(), Arg.Any<string>());
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
        public void SessionReportedIfSessionReportingAllowed()
        {
            // given
            mockPrivacyConfiguration.IsSessionReportingAllowed.Returns(true);
            var target = CreateBeacon().Build();

            // when
            target.EndSession();

            // then
            mockBeaconCache.Received(1).AddEventData(Arg.Any<BeaconKey>(), Arg.Any<long>(), Arg.Any<string>());
        }

        [Test]
        public void ErrorNotReportedIfErrorReportingDisallowed()
        {
            // given
            mockPrivacyConfiguration.IsErrorReportingAllowed.Returns(false);
            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, "error", 42, "the answer");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ErrorReportedIfErrorReportingAllowed()
        {
            // given
            mockPrivacyConfiguration.IsErrorReportingAllowed.Returns(true);
            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, "error", 42, "the answer");

            // then
            mockBeaconCache.Received(1).AddEventData(Arg.Any<BeaconKey>(), Arg.Any<long>(), Arg.Any<string>());
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
        public void IntValueIsReportedIfValueReportingAllowed()
        {
            // given
            mockPrivacyConfiguration.IsValueReportingAllowed.Returns(true);

            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test int value", 13);

            // then
            mockBeaconCache.Received(1).AddEventData(Arg.Any<BeaconKey>(), Arg.Any<long>(), Arg.Any<string>());
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
        public void DoubleValueReportedIfValueReportingAllowed()
        {
            // given
            mockPrivacyConfiguration.IsValueReportingAllowed.Returns(true);
            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test double value", 2.71);

            // then
            mockBeaconCache.Received(1).AddEventData(Arg.Any<BeaconKey>(), Arg.Any<long>(), Arg.Any<string>());
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
        public void StringValueReportedIfValueReportingAllowed()
        {
            // given
            mockPrivacyConfiguration.IsValueReportingAllowed.Returns(true);
            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test string value", "test data");

            // then
            mockBeaconCache.Received(1).AddEventData(Arg.Any<BeaconKey>(), Arg.Any<long>(), Arg.Any<string>());
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
        public void NamedEventReportedIfEventReportingAllowed()
        {
            // given
            mockPrivacyConfiguration.IsEventReportingAllowed.Returns(true);
            var target = CreateBeacon().Build();

            // when
            target.ReportEvent(ActionId, "test event");

            // then
            mockBeaconCache.Received(1).AddEventData(Arg.Any<BeaconKey>(), Arg.Any<long>(), Arg.Any<string>());
        }

        [Test]
        public void SessionStartIsReported()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            target.StartSession();

            // then
            mockBeaconCache.Received(1).AddEventData(Arg.Any<BeaconKey>(), Arg.Any<long>(), Arg.Any<string>());
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
        public void NoSessionStartIsReportedIfCapturingDisabled()
        {
            // given
            mockServerConfiguration.IsCaptureEnabled.Returns(false);

            var target = CreateBeacon().Build();

            // when
            target.StartSession();

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
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
        public void IsCaptureEnabledReturnsValueFromServerConfig()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            mockServerConfiguration.IsCaptureEnabled.Returns(false);
            var obtained = target.IsCaptureEnabled;

            // then
            Assert.That(obtained, Is.False);

            // and when
            mockServerConfiguration.IsCaptureEnabled.Returns(true);
            obtained = target.IsCaptureEnabled;

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

            var target = CreateBeacon().Build();

            // when
            var response = target.Send(httpClientProvider, mockAdditionalQueryParameters);

            // then
            Assert.That(response, Is.Null);
            mockBeaconCache.Received(1).GetNextBeaconChunk(
                Arg.Any<BeaconKey>(),
                $"vv={ProtocolConstants.ProtocolVersion.ToInvariantString()}" +
                $"&va={ProtocolConstants.OpenKitVersion}" +
                $"&ap={AppId}" +
                $"&an={AppName}" +
                $"&vn={AppVersion}" +
                $"&pt={ProtocolConstants.PlatformTypeOpenKit.ToInvariantString()}" +
                $"&tt={ProtocolConstants.AgentTechnologyType}" +
                $"&vi={DeviceId.ToInvariantString()}" +
                $"&sn={SessionId.ToInvariantString()}" +
                "&ip=127.0.0.1" +
                $"&os={string.Empty}" +
                $"&mf={string.Empty}" +
                $"&md={string.Empty}" +
                $"&dl={((int) ConfigurationDefaults.DefaultDataCollectionLevel).ToInvariantString()}" +
                $"&cl={((int) ConfigurationDefaults.DefaultCrashReportingLevel).ToInvariantString()}" +
                "&vs=1" +
                "&tx=0" +
                "&tv=0" +
                $"&mp={Multiplicity.ToInvariantString()}",
                Arg.Any<int>(),
                Arg.Any<char>()
            );
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
    }
}