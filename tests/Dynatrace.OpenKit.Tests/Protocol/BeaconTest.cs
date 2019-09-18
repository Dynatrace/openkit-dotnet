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
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Providers;
using Dynatrace.OpenKit.Util;
using NSubstitute;
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

        private IOpenKitConfiguration mockConfiguration;
        private IBeaconCache mockBeaconCache;
        private IThreadIdProvider mockThreadIdProvider;
        private ITimingProvider mockTimingProvider;
        private IPrnGenerator mockRandomGenerator;
        private ILogger mockLogger;
        private IBeaconConfiguration mockBeaconConfig;
        private OpenKitComposite mockParent;

        [SetUp]
        public void Setup()
        {
            mockThreadIdProvider = Substitute.For<IThreadIdProvider>();
            mockThreadIdProvider.ThreadId.Returns(ThreadId);

            mockTimingProvider = Substitute.For<ITimingProvider>();
            mockTimingProvider.ProvideTimestampInMilliseconds().Returns(0);

            mockRandomGenerator = Substitute.For<IPrnGenerator>();

            mockLogger = Substitute.For<ILogger>();
            mockParent = Substitute.For<OpenKitComposite>();
            mockBeaconCache = Substitute.For<IBeaconCache>();

            var mockDevice = Substitute.For<Device>(string.Empty, string.Empty, string.Empty);

            var mockHttpClientConfig = Substitute.For<IHttpClientConfiguration>();
            mockHttpClientConfig.ServerId.Returns(ServerId);

            mockBeaconConfig = Substitute.For<IBeaconConfiguration>();
            mockBeaconConfig.Multiplicity.Returns(BeaconConfiguration.DefaultMultiplicity);
            mockBeaconConfig.DataCollectionLevel.Returns(BeaconConfiguration.DefaultDataCollectionLevel);
            mockBeaconConfig.CrashReportingLevel.Returns(BeaconConfiguration.DefaultCrashReportingLevel);
            mockBeaconConfig.CapturingAllowed.Returns(true);

            mockConfiguration = Substitute.For<IOpenKitConfiguration>();
            mockConfiguration.ApplicationId.Returns(AppId);
            mockConfiguration.ApplicationIdPercentEncoded.Returns(AppId);
            mockConfiguration.ApplicationName.Returns(AppName);
            mockConfiguration.ApplicationVersion.Returns(AppVersion);
            mockConfiguration.Device.Returns(mockDevice);
            mockConfiguration.DeviceId.Returns(DeviceId);
            mockConfiguration.IsCaptureOn.Returns(true);
            mockConfiguration.CaptureErrors.Returns(true);
            mockConfiguration.CaptureCrashes.Returns(true);
            mockConfiguration.MaxBeaconSize.Returns(30 * 1024); // 30kB
            mockConfiguration.HttpClientConfig.Returns(mockHttpClientConfig);
            mockConfiguration.BeaconConfig.Returns(mockBeaconConfig);
        }

        [Test]
        public void DefaultBeaconConfigurationDoesNotDisableCapturing()
        {
            // given
            var target = CreateBeacon().Build();

            // then
            Assert.That(target.CapturingDisabled, Is.False);
        }

        [Test]
        public void DefaultBeaconConfigurationSetsMultiplicityToOne()
        {
            // given
            var target = CreateBeacon().Build();

            // then
            Assert.That(target.Multiplicity, Is.EqualTo(1));
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
        public void CreateTag()
        {
            // given
            const int sequenceNumber = 42;
            var target = CreateBeacon().Build();

            // when
            var obtained = target.CreateTag(ActionId, sequenceNumber);

            // then
            Assert.That(obtained, Is.EqualTo(
                "MT"                                             // tag prefix
                + $"_{ProtocolConstants.ProtocolVersion}"        // protocol version
                + $"_{ServerId}"                                 // server ID
                + $"_{DeviceId}"                                 // device ID
                + "_0"                                           // session number
                + $"_{AppId}"                                    // application ID
                + $"_{ActionId}"                                 // action ID
                + $"_{ThreadId}"                                 // thread ID
                + $"_{sequenceNumber}"                           // sequence number
                ));
        }

        [Test]
        public void CreateWebRequestTagEncodesDeviceIdProperly()
        {
            // given
            const long deviceId = -42;
            const int sequenceNumber = 1;
            mockConfiguration.DeviceId.Returns(deviceId);
            var target = CreateBeacon().Build();

            // when
            var obtained = target.CreateTag(ActionId, sequenceNumber);

            // then
            Assert.That(obtained, Is.EqualTo(
                "MT"                                             // tag prefix
                + $"_{ProtocolConstants.ProtocolVersion}"        // protocol version
                + $"_{ServerId}"                                 // server ID
                + $"_{deviceId}"                                 // device ID
                +  "_0"                                          // session number
                + $"_{AppId}"                                    // application ID
                + $"_{ActionId}"                                 // action ID
                + $"_{ThreadId}"                                 // thread ID
                + $"_{sequenceNumber}"                           // sequence number
                ));
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.ACTION.ToInt()}"          // event type
                + $"&na={actionName}"                     // action name
                + $"&it={ThreadId}"                       // thread ID
                + $"&ca={ActionId}"                       // action ID
                + $"&pa={parentId}"                       // parent action ID
                +  "&s0=0"                                // action start sequence number
                +  "&t0=0"                                // action start time
                +  "&s1=0"                                // action end sequence number
                +  "&t1=0"                                // action end time
                );
        }

        [Test]
        public void AddEndSessionEvent()
        {
            // given
            var target = CreateBeacon().Build();
            var session = Substitute.For<ISessionInternals>();

            // when
            target.EndSession(session);

            // then
            mockBeaconCache.Received(1).AddEventData(
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.SESSION_END.ToInt()}"     // event type
                + $"&it={ThreadId}"                       // thread ID
                +  "&pa=0"                                // parent action ID
                +  "&s0=1"                                // session end sequence number
                +  "&t0=0"                                // session end time
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.VALUE_INT.ToInt()}"       // event type
                + $"&na={valueName}"                      // action name
                + $"&it={ThreadId}"                       // thread ID
                + $"&pa={ActionId}"                       // parent action ID
                +  "&s0=1"                                // event sequence number
                +  "&t0=0"                                // event timestamp
                + $"&vl={value}"                          // reported value
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.VALUE_DOUBLE.ToInt()}"    // event type
                + $"&na={valueName}"                      // action name
                + $"&it={ThreadId}"                       // thread ID
                + $"&pa={ActionId}"                       // parent action ID
                +  "&s0=1"                                // event sequence number
                +  "&t0=0"                                // event timestamp
                + $"&vl={value}"                          // reported value
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.VALUE_STRING.ToInt()}"    // event type
                + $"&na={valueName}"                      // action name
                + $"&it={ThreadId}"                       // thread ID
                + $"&pa={ActionId}"                       // parent action ID
                +  "&s0=1"                                // event sequence number
                +  "&t0=0"                                // event timestamp
                + $"&vl={value}"                          // reported value
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.VALUE_STRING.ToInt()}"    // event type
                + $"&na={valueName}"                      // action name
                + $"&it={ThreadId}"                       // thread ID
                + $"&pa={ActionId}"                       // parent action ID
                +  "&s0=1"                                // event sequence number
                +  "&t0=0"                                // event timestamp
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.VALUE_STRING.ToInt()}"    // event type
                + $"&it={ThreadId}"                       // thread ID
                + $"&pa={ActionId}"                       // parent action ID
                +  "&s0=1"                                // event sequence number
                +  "&t0=0"                                // event timestamp
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.NAMED_EVENT.ToInt()}"     // event type
                + $"&na={eventName}"                      // action name
                + $"&it={ThreadId}"                       // thread ID
                + $"&pa={ActionId}"                       // parent action ID
                +  "&s0=1"                                // event sequence number
                +  "&t0=0"                                // event timestamp
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.NAMED_EVENT.ToInt()}"     // event type
                + $"&it={ThreadId}"                       // thread ID
                + $"&pa={ActionId}"                       // parent action ID
                +  "&s0=1"                                // event sequence number
                +  "&t0=0"                                // event timestamp
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.ERROR.ToInt()}"           // event type
                + $"&na={errorName}"                      // action name
                + $"&it={ThreadId}"                       // thread ID
                + $"&pa={ActionId}"                       // parent action ID
                +  "&s0=1"                                // event sequence number
                +  "&t0=0"                                // event timestamp
                + $"&ev={errorCode}"                      // reported error code
                + $"&rs={reason}"                         // error reason
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.ERROR.ToInt()}"           // event type
                + $"&it={ThreadId}"                       // thread ID
                + $"&pa={ActionId}"                       // parent action ID
                +  "&s0=1"                                // event sequence number
                +  "&t0=0"                                // event timestamp
                + $"&ev={errorCode}"                      // reported error code
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.CRASH.ToInt()}"           // event type
                + $"&na={errorName}"                      // action name
                + $"&it={ThreadId}"                       // thread ID
                +  "&pa=0"                                // parent action ID
                +  "&s0=1"                                // event sequence number
                +  "&t0=0"                                // event timestamp
                + $"&rs={reason}"                         // error reason
                + $"&st={stacktrace}"                     // reported stacktrace
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.CRASH.ToInt()}"           // event type
                + $"&na={errorName}"                      // action name
                + $"&it={ThreadId}"                       // thread ID
                +  "&pa=0"                                // parent action ID
                +  "&s0=1"                                // event sequence number
                +  "&t0=0"                                // event timestamp
                );
        }

        [Test]
        public void AddWebRequest()
        {
            // given
            var target = CreateBeacon().Build();

            const int sentBytes = 13;
            const int receivedBytes = 14;
            const int responseCode = 15;
            var tracer = Substitute.For<IWebRequestTracerInternals>();
            tracer.Url.Returns((string)null);
            tracer.BytesSent.Returns(sentBytes);
            tracer.BytesReceived.Returns(receivedBytes);
            tracer.ResponseCode.Returns(responseCode);

            // when
            target.AddWebRequest(ActionId, tracer);

            // then
            mockBeaconCache.Received(1).AddEventData(
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt()}"     // event type
                + $"&it={ThreadId}"                       // thread ID
                + $"&pa={ActionId}"                       // parent action ID
                +  "&s0=0"                                // web request start sequence number
                +  "&t0=0"                                // web request start timestamp
                +  "&s1=0"                                // web request end sequence number
                +  "&t1=0"                                // web request end timestamp
                + $"&bs={sentBytes}"                      // number bytes sent
                + $"&br={receivedBytes}"                  // number bytes received
                + $"&rc={responseCode}"                   // number bytes received
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.IDENTIFY_USER.ToInt()}"   // event type
                + $"&na={userId}"                         // number bytes received
                + $"&it={ThreadId}"                       // thread ID
                +  "&pa=0"                                // parent action ID
                +  "&s0=1"                                // web request start sequence number
                +  "&t0=0"                                // web request start timestamp
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.IDENTIFY_USER.ToInt()}"   // event type
                + $"&it={ThreadId}"                       // thread ID
                +  "&pa=0"                                // parent action ID
                +  "&s0=1"                                // web request start sequence number
                +  "&t0=0"                                // web request start timestamp
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt()}"     // event type
                + $"&na={Uri.EscapeDataString(testUrl)}"  // traced URL
                + $"&it={ThreadId}"                       // thread ID
                + $"&pa={ActionId}"                       // parent action ID
                +  "&s0=1"                                // web request start sequence number
                +  "&t0=0"                                // web request start timestamp
                +  "&s1=2"                                // web request end sequence number
                +  "&t1=0"                                // web request end timestamp
                + $"&bs={sentBytes}"                      // number bytes sent
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt()}"     // event type
                + $"&na={Uri.EscapeDataString(testUrl)}"  // traced URL
                + $"&it={ThreadId}"                       // thread ID
                + $"&pa={ActionId}"                       // parent action ID
                +  "&s0=1"                                // web request start sequence number
                +  "&t0=0"                                // web request start timestamp
                +  "&s1=2"                                // web request end sequence number
                +  "&t1=0"                                // web request end timestamp
                + $"&bs={sentBytes}"                      // number bytes sent
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt()}"     // event type
                + $"&na={Uri.EscapeDataString(testUrl)}"  // traced URL
                + $"&it={ThreadId}"                       // thread ID
                + $"&pa={ActionId}"                       // parent action ID
                +  "&s0=1"                                // web request start sequence number
                +  "&t0=0"                                // web request start timestamp
                +  "&s1=2"                                // web request end sequence number
                +  "&t1=0"                                // web request end timestamp
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt()}"     // event type
                + $"&na={Uri.EscapeDataString(testUrl)}"  // traced URL
                + $"&it={ThreadId}"                       // thread ID
                + $"&pa={ActionId}"                       // parent action ID
                +  "&s0=1"                                // web request start sequence number
                +  "&t0=0"                                // web request start timestamp
                +  "&s1=2"                                // web request end sequence number
                +  "&t1=0"                                // web request end timestamp
                + $"&br={receivedBytes}"                  // number bytes received
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt()}"     // event type
                + $"&na={Uri.EscapeDataString(testUrl)}"  // traced URL
                + $"&it={ThreadId}"                       // thread ID
                + $"&pa={ActionId}"                       // parent action ID
                +  "&s0=1"                                // web request start sequence number
                +  "&t0=0"                                // web request start timestamp
                +  "&s1=2"                                // web request end sequence number
                +  "&t1=0"                                // web request end timestamp
                + $"&br={receivedBytes}"                  // number bytes received
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
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt()}"     // event type
                + $"&na={Uri.EscapeDataString(testUrl)}"  // traced URL
                + $"&it={ThreadId}"                       // thread ID
                + $"&pa={ActionId}"                       // parent action ID
                +  "&s0=1"                                // web request start sequence number
                +  "&t0=0"                                // web request start timestamp
                +  "&s1=2"                                // web request end sequence number
                +  "&t1=0"                                // web request end timestamp
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
            webRequest.Start().SetBytesSent(sentBytes).SetBytesReceived(receivedBytes).Stop(-1); //stop will add the web request to the beacon

            // then
            mockBeaconCache.Received(1).AddEventData(
                0,                                        // beacon ID
                0,                                        // timestamp
                $"et={EventType.WEB_REQUEST.ToInt()}"     // event type
                + $"&na={Uri.EscapeDataString(testUrl)}"  // traced URL
                + $"&it={ThreadId}"                       // thread ID
                + $"&pa={ActionId}"                       // parent action ID
                +  "&s0=1"                                // web request start sequence number
                +  "&t0=0"                                // web request start timestamp
                +  "&s1=2"                                // web request end sequence number
                +  "&t1=0"                                // web request end timestamp
                + $"&bs={sentBytes}"                      // number bytes sent
                + $"&br={receivedBytes}"                  // number bytes received
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
            var response = target.Send(mockHttpClientProvider);

            // then
            Assert.That(response, Is.Null);
        }

        [Test]
        public void SendValidData()
        {
            // given
            const string ipAddress = "127.0.0.1";
            const int responseCode = 200;

            var target = CreateBeacon().With(new BeaconCache(mockLogger)).WithIpAddress(ipAddress).Build();

             var httpClient = Substitute.For<IHttpClient>();
            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>())
                .Returns(new StatusResponse(mockLogger, "", responseCode, new Dictionary<string, List<string>>()));

            var httpClientProvider = Substitute.For<IHttpClientProvider>();
            httpClientProvider.CreateClient(Arg.Any<IHttpClientConfiguration>()).Returns(httpClient);

            // when
            target.ReportCrash("errorName", "errorReason", "stackTrace");
            var response = target.Send(httpClientProvider);

            // then
            Assert.That(response, Is.Not.Null);
            Assert.That(response.ResponseCode, Is.EqualTo(responseCode));
            httpClient.Received(1).SendBeaconRequest(ipAddress, Arg.Any<byte[]>());
        }

        [Test]
        public void SendDataAndFakeErrorResponse()
        {
            // given
            const string ipAddress = "127.0.0.1";
            const int responseCode = 418;

            var target = CreateBeacon().With(new BeaconCache(mockLogger)).WithIpAddress(ipAddress).Build();

            var httpClient = Substitute.For<IHttpClient>();
            httpClient.SendBeaconRequest(Arg.Any<string>(), Arg.Any<byte[]>())
                .Returns(new StatusResponse(mockLogger, "", responseCode, new Dictionary<string, List<string>>()));

            var httpClientProvider = Substitute.For<IHttpClientProvider>();
            httpClientProvider.CreateClient(Arg.Any<IHttpClientConfiguration>()).Returns(httpClient);

            // when
            target.ReportCrash("errorName", "errorReason", "stackTrace");
            var response = target.Send(httpClientProvider);

            // then
            Assert.That(response, Is.Not.Null);
            Assert.That(response.ResponseCode, Is.EqualTo(responseCode));
            httpClient.Received(1).SendBeaconRequest(ipAddress, Arg.Any<byte[]>());
        }

        [Test]
        public void ClearDataFromBeaconCache()
        {
            // given
            var session = Substitute.For<ISessionInternals>();
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
            target.EndSession(session);

            Assert.That(beaconCache.GetActions(target.SessionNumber), Is.Not.Empty);
            Assert.That(beaconCache.GetEvents(target.SessionNumber), Is.Not.Empty);

            // when
            target.ClearData();

            // then
            Assert.That(beaconCache.GetActions(target.SessionNumber), Is.Null);
            Assert.That(beaconCache.GetEvents(target.SessionNumber), Is.Null);
        }

        [Test]
        public void NoSessionIsAddedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            mockBeaconConfig.CapturingAllowed.Returns(false);
            var session = Substitute.For<ISessionInternals>();

            var target = CreateBeacon().Build();

            // when
            target.EndSession(session);

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoActionIsAddedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            mockBeaconConfig.CapturingAllowed.Returns(false);
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
            mockBeaconConfig.CapturingAllowed.Returns(false);
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
            mockBeaconConfig.CapturingAllowed.Returns(false);
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
            mockBeaconConfig.CapturingAllowed.Returns(false);
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
            mockBeaconConfig.CapturingAllowed.Returns(false);

            var target = CreateBeacon().Build();

            // when
            target.ReportEvent(ActionId, "Event name");

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoErrorIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            mockBeaconConfig.CapturingAllowed.Returns(false);

            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, "Error name", 123, "The reason for this error");

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoCrashIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            mockBeaconConfig.CapturingAllowed.Returns(false);

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash("Error name", "The reason for this error", "the stack trace");

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoWebRequestIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            mockBeaconConfig.CapturingAllowed.Returns(false);

            var target = CreateBeacon().Build();

            var webRequestTracer = CreateWebRequestTracer(target).WithUrl("https://foo.bar").Build();

            // when
            target.AddWebRequest(17, webRequestTracer);

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NoUserIdentificationIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            mockBeaconConfig.CapturingAllowed.Returns(false);

            var target = CreateBeacon().Build();

            // when
            target.IdentifyUser("jane.doe@acme.com");

            // then ensure nothing has been serialized
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }


        [Test]
        public void NoWebRequestIsReportedForDataCollectionLevel0()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.OFF);
            var webRequestTracer = Substitute.For<IWebRequestTracerInternals>();

            var target = CreateBeacon().Build();

            // when
            target.AddWebRequest(ActionId, webRequestTracer);

            // then ensure nothing has been serialized
            Assert.That(webRequestTracer.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void WebRequestIsReportedForDataCollectionLevel1()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.PERFORMANCE);
            var webRequestTracer = Substitute.For<IWebRequestTracerInternals>();

            var target = CreateBeacon().Build();

            // when
            target.AddWebRequest(ActionId, webRequestTracer);


            // then ensure nothing has been serialized
            _ = webRequestTracer.Received(1).BytesReceived;
            _ = webRequestTracer.Received(1).BytesSent;
            _ = webRequestTracer.Received(1).ResponseCode;
            Assert.That(target.IsEmpty, Is.False);
            mockBeaconCache.Received(1).AddEventData(Arg.Any<int>(), Arg.Any<long>(), Arg.Any<string>());
        }

        [Test]
        public void WebRequestIsReportedForDataCollectionLevel2()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.USER_BEHAVIOR);

            var target = CreateBeacon().Build();

            var webRequestTracer = Substitute.For<IWebRequestTracerInternals>();

            // when
            target.AddWebRequest(ActionId, webRequestTracer);


            // then ensure nothing has been serialized
            _ = webRequestTracer.Received(1).BytesReceived;
            _ = webRequestTracer.Received(1).BytesSent;
            _ = webRequestTracer.Received(1).ResponseCode;
            Assert.That(target.IsEmpty, Is.False);
            mockBeaconCache.Received(1).AddEventData(Arg.Any<int>(), Arg.Any<long>(), Arg.Any<string>());
        }

        [Test]
        public void CreateTagReturnsEmptyStringForDataCollectionLevel0()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.OFF);

            var target = CreateBeacon().Build();

            // when
            var tagReturned = target.CreateTag(ActionId, 1);

            // then
            Assert.That(tagReturned, Is.Empty);
        }

        [Test]
        public void CreateTagReturnsTagStringForDataCollectionLevel1()
        {
            // given
            const long deviceId = 37;
            const int sequenceNo = 1;
            mockRandomGenerator.NextLong(Arg.Any<long>()).Returns(deviceId);
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.PERFORMANCE);

            var target = CreateBeacon().Build();

            // when
            var obtained = target.CreateTag(ActionId, sequenceNo);

            // then
            Assert.That(obtained, Is.EqualTo(
                "MT" +                    // tag prefix
                 "_3" +                   // protocol version
                $"_{ServerId}" +          // server ID
                $"_{deviceId}" +          // device ID
                 "_1" +                   // session number (must always 1 for data collection level performance)
                $"_{AppId}" +             // application ID
                $"_{ActionId}" +          // parent action ID
                $"_{ThreadId}" +          // thread ID
                $"_{sequenceNo}"          // sequence number
            ));
        }

        [Test]
        public void CreateTagReturnsTagStringForDataCollectionLevel2()
        {
            // given
            const int sessionId = 73;
            const long deviceId = 37;
            const int sequenceNo = 1;
            mockConfiguration.DeviceId.Returns(deviceId);
            mockConfiguration.NextSessionNumber.Returns(sessionId);

            var target = CreateBeacon().Build();

            // when
            var obtained = target.CreateTag(ActionId, sequenceNo);

            // then
            Assert.That(obtained, Is.EqualTo(
                "MT" +                    // tag prefix
                 "_3" +                   // protocol version
                $"_{ServerId}" +          // server ID
                $"_{deviceId}" +          // device ID
                $"_{sessionId}" +         // session number (must always 1 for data collection level performance)
                $"_{AppId}" +             // application ID
                $"_{ActionId}" +          // parent action ID
                $"_{ThreadId}" +          // thread ID
                $"_{sequenceNo}"          // sequence number
            ));
        }

        [Test]
        public void IdentifyUserDoesNotReportOnDataCollectionLevel0()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.OFF);

            var target = CreateBeacon().Build();

            // when
            target.IdentifyUser("test user");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void IdentifyUserDoesNotReportOnDataCollectionLevel1()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.PERFORMANCE);

            var target = CreateBeacon().Build();

            // when
            target.IdentifyUser("test user");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void IdentifyUserDoesReportOnDataCollectionLevel2()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.USER_BEHAVIOR);

            var target = CreateBeacon().Build();

            // when
            target.IdentifyUser("test user");

            // then
            mockBeaconCache.AddEventData(Arg.Any<int>(), Arg.Any<long>(), Arg.Any<string>());
        }

        [Test]
        public void DeviceIdIsRandomizedOnDataCollectionLevel0()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.OFF);

            var target = CreateBeacon().Build();

            // when
            _ = target.DeviceId;

            // then
            mockRandomGenerator.Received(1).NextLong(long.MaxValue);
        }

        [Test]
        public void DeviceIdIsRandomizedOnDataCollectionLevel1()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.PERFORMANCE);

            var target = CreateBeacon().Build();

            // when
            _ = target.DeviceId;

            // then
            mockRandomGenerator.Received(1).NextLong(long.MaxValue);
        }

        [Test]
        public void GivenDeviceIdIsUsedOnDataCollectionLevel2()
        {
            // given
            const long deviceId = 12345;
            mockConfiguration.DeviceId.Returns(deviceId);
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.USER_BEHAVIOR);

            var target = CreateBeacon().Build();

            // when
            var obtained = target.DeviceId;

            // then
            Assert.That(mockRandomGenerator.ReceivedCalls(), Is.Empty);
            Assert.That(obtained, Is.EqualTo(deviceId));
        }

        [Test]
        public void RandomDeviceIdCannotBeNegativeOnDataCollectionLevel0()
        {
            // given
            mockRandomGenerator.NextLong(Arg.Any<long>()).Returns(-123456789);
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.OFF);

            var target = CreateBeacon().Build();

            // when
            var deviceId = target.DeviceId;

            // then
            mockRandomGenerator.Received(1).NextLong(Arg.Any<long>());
            Assert.That(deviceId, Is.GreaterThanOrEqualTo(0L));
            Assert.That(deviceId, Is.LessThanOrEqualTo(long.MaxValue));
        }

        [Test]
        public void RandomDeviceIdCannotBeNegativeOnDataCollectionLevel1()
        {
            // given
            mockRandomGenerator.NextLong(Arg.Any<long>()).Returns(-123456789);
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.PERFORMANCE);

            var target = CreateBeacon().Build();

            // when
            var deviceId = target.DeviceId;

            // then
            mockRandomGenerator.Received(1).NextLong(Arg.Any<long>());
            Assert.That(deviceId, Is.GreaterThanOrEqualTo(0L));
            Assert.That(deviceId, Is.LessThanOrEqualTo(long.MaxValue));
        }

        [Test]
        public void SessionIdIsAlwaysValue1OnDataCollectionLevel0()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.OFF);

            var target = CreateBeacon().Build();

            // when
            var sessionId = target.SessionNumber;

            // then
            Assert.That(sessionId, Is.EqualTo(1));
        }

        [Test]
        public void SessionIdIsAlwaysValue1OnDataCollectionLevel1()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.PERFORMANCE);

            var target = CreateBeacon().Build();

            // when
            var sessionId = target.SessionNumber;

            // then
            Assert.That(sessionId, Is.EqualTo(1));
        }

        [Test]
        public void SessionIdIsValueFromSessionIdProviderOnDataCollectionLevel2()
        {
            // given
            const int sessionId = 73;
            mockConfiguration.NextSessionNumber.Returns(sessionId);
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.USER_BEHAVIOR);

            var target = CreateBeacon().Build();

            // when
            var obtained = target.SessionNumber;

            // then
            Assert.That(obtained, Is.EqualTo(target.SessionNumber));
            _ = mockConfiguration.Received(1).NextSessionNumber;
        }

        [Test]
        public void ReportCrashDoesNotReportOnCrashReportingLevel0()
        {
            // given
            mockBeaconConfig.CrashReportingLevel.Returns(CrashReportingLevel.OFF);

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash("OutOfMemory exception", "insufficient memory", "stacktrace:123");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ReportCrashDoesNotReportOnCrashReportingLevel1()
        {
            // given
            mockBeaconConfig.CrashReportingLevel.Returns(CrashReportingLevel.OPT_OUT_CRASHES);

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash("OutOfMemory exception", "insufficient memory", "stacktrace:123");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ReportCrashDoesReportOnCrashReportingLevel2()
        {
            // given
            mockBeaconConfig.CrashReportingLevel.Returns(CrashReportingLevel.OPT_IN_CRASHES);

            var target = CreateBeacon().Build();

            // when
            target.ReportCrash("OutOfMemory exception", "insufficient memory", "stacktrace:123");

            // then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void ActionNotReportedForDataCollectionLevel0()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.OFF);
            var action = Substitute.For<IActionInternals>();

            var target = CreateBeacon().Build();

            // when
            target.AddAction(action);

            // then
            Assert.That(action.ReceivedCalls(), Is.Empty);
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ActionReportedForDataCollectionLevel1()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.PERFORMANCE);
            var action = Substitute.For<IActionInternals>();
            action.Id.Returns(ActionId);

            var target = CreateBeacon().Build();

            // when
            target.AddAction(action);

            // then
            Assert.That(target.IsEmpty, Is.False);
        }


        [Test]
        public void ActionReportedForDataCollectionLevel2()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.USER_BEHAVIOR);
            var action = Substitute.For<IActionInternals>();
            action.Id.Returns(ActionId);

            var target = CreateBeacon().Build();

            // when
            target.AddAction(action);

            // then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void SessionNotReportedForDataCollectionLevel0()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.OFF);
            var session = Substitute.For<ISessionInternals>();

            var target = CreateBeacon().Build();

            // when
            target.EndSession(session);

            // then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void SessionReportedForDataCollectionLevel1()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.PERFORMANCE);
            var session = Substitute.For<ISessionInternals>();

            var target = CreateBeacon().Build();

            // when
            target.EndSession(session);

            // then
            Assert.That(target.IsEmpty, Is.False);
        }


        [Test]
        public void SessionReportedForDataCollectionLevel2()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.USER_BEHAVIOR);
            var session = Substitute.For<ISessionInternals>();

            var target = CreateBeacon().Build();

            // when
            target.EndSession(session);

            // then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void ReportErrorDoesNotReportOnDataCollectionLevel0()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.OFF);

            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, "error", 42, "the answer");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void ReportErrorDoesReportOnDataCollectionLevel1()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.PERFORMANCE);

            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, "error", 42, "the answer");

            // then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void ReportErrorDoesReportOnDataCollectionLevel2()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.USER_BEHAVIOR);
            var target = CreateBeacon().Build();

            // when
            target.ReportError(ActionId, "error", 42, "the answer");

            // then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void IntValueNotReportedForDataCollectionLevel0()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.OFF);

            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test int value", 13);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void IntValueNotReportedForDataCollectionLevel1()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.PERFORMANCE);

            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test int value", 13);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }


        [Test]
        public void IntValueReportedForDataCollectionLevel2()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.USER_BEHAVIOR);

            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test int value", 13);

            // then
            Assert.That(target.IsEmpty, Is.False);
        }


        [Test]
        public void DoubleValueNotReportedForDataCollectionLevel0()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.OFF);

            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test double value", 2.71);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void DoubleValueNotReportedForDataCollectionLevel1()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.PERFORMANCE);

            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test double value", 2.71);

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }


        [Test]
        public void DoubleValueReportedForDataCollectionLevel2()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.USER_BEHAVIOR);

            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test double value", 2.71);

            // then
            mockBeaconCache.Received(1).AddEventData(Arg.Any<int>(), Arg.Any<long>(), Arg.Any<string>());
        }


        [Test]
        public void StringValueNotReportedForDataCollectionLevel0()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.OFF);

            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test string value", "test data");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void StringValueNotReportedForDataCollectionLevel1()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.PERFORMANCE);

            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test string value", "test data");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }


        [Test]
        public void StringValueReportedForDataCollectionLevel2()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.USER_BEHAVIOR);

            var target = CreateBeacon().Build();

            // when
            target.ReportValue(ActionId, "test string value", "test data");

            // then
            mockBeaconCache.Received(1).AddEventData(Arg.Any<int>(), Arg.Any<long>(), Arg.Any<string>());
        }


        [Test]
        public void NamedEventNotReportedForDataCollectionLevel0()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.OFF);

            var target = CreateBeacon().Build();

            // when
            target.ReportEvent(ActionId, "test event");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NamedEventNotReportedForDataCollectionLevel1()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.PERFORMANCE);

            var target = CreateBeacon().Build();

            // when
            target.ReportEvent(ActionId, "test event");

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }

        [Test]
        public void NamedEventReportedForDataCollectionLevel2()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.USER_BEHAVIOR);

            var target = CreateBeacon().Build();

            // when
            target.ReportEvent(ActionId, "test event");

            // then
            mockBeaconCache.Received(1).AddEventData(Arg.Any<int>(), Arg.Any<long>(), Arg.Any<string>());
        }

        [Test]
        public void SessionStartIsReported()
        {
            // given
            var target = CreateBeacon().Build();

            // when
            target.StartSession();

            // then
            mockBeaconCache.Received(1).AddEventData(Arg.Any<int>(), Arg.Any<long>(), Arg.Any<string>());
        }

        [Test]
        public void SessionStartIsReportedForDataCollectionLevel0()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.OFF);

            var target = CreateBeacon().Build();

            // when
            target.StartSession();

            // then
            mockBeaconCache.Received(1).AddEventData(Arg.Any<int>(), Arg.Any<long>(), Arg.Any<string>());
        }


        [Test]
        public void SessionStartIsReportedForDataCollectionLevel1()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.PERFORMANCE);

            var target = CreateBeacon().Build();

            // when
            target.StartSession();

            // then
            mockBeaconCache.Received(1).AddEventData(Arg.Any<int>(), Arg.Any<long>(), Arg.Any<string>());
        }


        [Test]
        public void SessionStartIsReportedForDataCollectionLevel2()
        {
            // given
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.USER_BEHAVIOR);

            var target = CreateBeacon().Build();

            // when
            target.StartSession();

            // then
            mockBeaconCache.Received(1).AddEventData(Arg.Any<int>(), Arg.Any<long>(), Arg.Any<string>());
        }

        [Test]
        public void NoSessionStartIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            mockBeaconConfig.CapturingAllowed.Returns(false);

            var target = CreateBeacon().Build();

            // when
            target.StartSession();

            // then
            Assert.That(mockBeaconCache.ReceivedCalls(), Is.Empty);
        }


        [Test]
        public void UseInternalBeaconIdForAccessingBeaconCacheWhenSessionNumberReportingDisallowed()
        {
            // given
            const int beaconId = 73;
            mockConfiguration.NextSessionNumber.Returns(beaconId);
            mockBeaconConfig.DataCollectionLevel.Returns(DataCollectionLevel.PERFORMANCE);

            var target = CreateBeacon().Build();

            // when
            target.ClearData();

            // then
            Assert.That(target.SessionNumber, Is.EqualTo(1));
            mockBeaconCache.Received(1).DeleteCacheEntry(beaconId);
        }

        [Test]
        public void SendConstructsCorrectBeaconPrefix()
        {
            // given
            var httpClient = Substitute.For<IHttpClient>();
            var httpClientProvider = Substitute.For<IHttpClientProvider>();
            httpClientProvider.CreateClient(Arg.Any<HttpClientConfiguration>()).Returns(httpClient);

            var target = CreateBeacon().Build();

            // when
            var response = target.Send(httpClientProvider);

            // then
            Assert.That(response, Is.Null);
            mockBeaconCache.Received(1).GetNextBeaconChunk(
                Arg.Any<int>(),
                $"vv={ProtocolConstants.ProtocolVersion}" +
                $"&va={ProtocolConstants.OpenKitVersion}" +
                $"&ap={AppId}" +
                $"&an={AppName}" +
                $"&vn={AppVersion}" +
                $"&pt={ProtocolConstants.PlatformTypeOpenKit}" +
                $"&tt={ProtocolConstants.AgentTechnologyType}" +
                $"&vi={DeviceId}" +
                "&sn=0" +
                "&ip=127.0.0.1" +
                $"&os={string.Empty}" +
                $"&mf={string.Empty}" +
                $"&md={string.Empty}" +
                $"&dl={(int)BeaconConfiguration.DefaultCrashReportingLevel}" +
                $"&cl={(int)BeaconConfiguration.DefaultCrashReportingLevel}" +
                "&tx=0" +
                "&tv=0" +
                $"&mp={BeaconConfiguration.DefaultMultiplicity}",
                Arg.Any<int>(),
                Arg.Any<char>()
            );
        }

        private TestBeaconBuilder CreateBeacon()
        {
            return new TestBeaconBuilder()
                    .With(mockLogger)
                    .With(mockBeaconCache)
                    .With(mockConfiguration)
                    .WithIpAddress("127.0.0.1")
                    .With(mockThreadIdProvider)
                    .With(mockTimingProvider)
                    .With(mockRandomGenerator)
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
