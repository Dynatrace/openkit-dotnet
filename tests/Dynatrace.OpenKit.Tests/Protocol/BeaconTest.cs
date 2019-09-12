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
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core;
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Communication;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;
using Action = Dynatrace.OpenKit.Core.Objects.Action;

namespace Dynatrace.OpenKit.Protocol
{
    public class BeaconTest
    {
        private IThreadIdProvider threadIdProvider;
        private ITimingProvider timingProvider;
        private IPrnGenerator randomGenerator;
        private ILogger logger;
        private BeaconSender beaconSender;
        private BeaconConfiguration defaultBeaconConfig;

        [SetUp]
        public void Setup()
        {
            threadIdProvider = Substitute.For<IThreadIdProvider>();
            timingProvider = Substitute.For<ITimingProvider>();
            randomGenerator = Substitute.For<IPrnGenerator>();
            logger = Substitute.For<ILogger>();

            var beaconSendingContext = Substitute.For<IBeaconSendingContext>();
            beaconSender = new BeaconSender(logger, beaconSendingContext);
            defaultBeaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OPT_IN_CRASHES);
        }

        [Test]
        public void DefaultBeaconConfigurationDoesNotDisableCapturing()
        {
            // given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIdProvider, timingProvider);

            // then
            Assert.That(target.CapturingDisabled, Is.False);
        }

        [Test]
        public void DefaultBeaconConfigurationSetsMultiplicityToOne()
        {
            // given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIdProvider, timingProvider);

            // then
            Assert.That(target.Multiplicity, Is.EqualTo(1));
        }

        [Test]
        public void CanAddUserIdentifyEvent()
        {
            // given
            var beacon = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIdProvider, timingProvider)
            {
                BeaconConfiguration = defaultBeaconConfig
            };

            var userTag = "myTestUser";

            // when
            beacon.IdentifyUser(userTag);
            var target = beacon.EventDataList;

            // then
            Assert.That(target, Is.EquivalentTo(new[] { $"et=60&na={userTag}&it=0&pa=0&s0=1&t0=0" }));
        }

        [Test]
        public void CanAddSentBytesToWebRequestTracer()
        {
            //given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIdProvider, timingProvider)
            {
                BeaconConfiguration = defaultBeaconConfig
            };

            const string testUrl = "https://127.0.0.1";
            var webRequest = CreateWebRequestTracer(target, testUrl);
            var bytesSent = 123;

            // when
            webRequest.Start().SetBytesSent(bytesSent).Stop(-1); //stop will add the web request to the beacon

            // then
            Assert.That(target.EventDataList, Is.EquivalentTo(new[] { $"et=30&na={Uri.EscapeDataString(testUrl)}&it=0&pa=1&s0=1&t0=0&s1=2&t1=0&bs={bytesSent}" }));
        }

        [Test]
        public void CanAddSentBytesValueZeroToWebRequestTracer()
        {
            //given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIdProvider, timingProvider)
            {
                BeaconConfiguration = defaultBeaconConfig
            };

            const string testUrl = "https://127.0.0.1";
            var webRequest = CreateWebRequestTracer(target, testUrl);
            var bytesSent = 0;

            // when
            webRequest.Start().SetBytesSent(bytesSent).Stop(-1); //stop will add the web request to the beacon

            // then
            Assert.That(target.EventDataList, Is.EquivalentTo(new[] { $"et=30&na={Uri.EscapeDataString(testUrl)}&it=0&pa=1&s0=1&t0=0&s1=2&t1=0&bs={bytesSent}" }));
        }

        [Test]
        public void CannotAddSentBytesWithInvalidValueSmallerZeroToWebRequestTracer()
        {
            //given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIdProvider, timingProvider)
            {
                BeaconConfiguration = defaultBeaconConfig
            };

            const string testUrl = "https://127.0.0.1";
            var webRequest = CreateWebRequestTracer(target, testUrl);

            // when
            webRequest.Start().SetBytesSent(-1).Stop(-1); //stop will add the web request to the beacon

            // then
            Assert.That(target.EventDataList, Is.EquivalentTo(new[] { $"et=30&na={Uri.EscapeDataString(testUrl)}&it=0&pa=1&s0=1&t0=0&s1=2&t1=0" }));
        }

        [Test]
        public void CanAddReceivedBytesToWebRequestTracer()
        {
            //given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIdProvider, timingProvider)
            {
                BeaconConfiguration = defaultBeaconConfig
            };

            const string testUrl = "https://127.0.0.1";
            var webRequest = CreateWebRequestTracer(target, testUrl);
            var bytesReceived = 12321;

            // when
            webRequest.Start().SetBytesReceived(bytesReceived).Stop(-1); //stop will add the web request to the beacon

            // then
            Assert.That(target.EventDataList, Is.EquivalentTo(new[] { $"et=30&na={Uri.EscapeDataString(testUrl)}&it=0&pa=1&s0=1&t0=0&s1=2&t1=0&br={bytesReceived}" }));
        }

        [Test]
        public void CanAddReceivedBytesValueZeroToWebRequestTracer()
        {
            //given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIdProvider, timingProvider)
            {
                BeaconConfiguration = defaultBeaconConfig
            };

            const string testUrl = "https://127.0.0.1";
            var webRequest = CreateWebRequestTracer(target, testUrl);
            var bytesReceived = 0;

            // when
            webRequest.Start().SetBytesReceived(bytesReceived).Stop(-1); //stop will add the web request to the beacon

            // then
            Assert.That(target.EventDataList, Is.EquivalentTo(new[] { $"et=30&na={Uri.EscapeDataString(testUrl)}&it=0&pa=1&s0=1&t0=0&s1=2&t1=0&br={bytesReceived}" }));
        }

        [Test]
        public void CannotAddReceivedBytesWithInvalidValueSmallerZeroToWebRequestTracer()
        {
            //given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIdProvider, timingProvider)
            {
                BeaconConfiguration = defaultBeaconConfig
            };

            const string testUrl = "https://127.0.0.1";
            var webRequest = CreateWebRequestTracer(target, testUrl);

            // when
            webRequest.Start().SetBytesReceived(-1).Stop(-1); //stop will add the web request to the beacon

            // then
            Assert.That(target.EventDataList, Is.EquivalentTo(new[] { $"et=30&na={Uri.EscapeDataString(testUrl)}&it=0&pa=1&s0=1&t0=0&s1=2&t1=0" }));
        }

        [Test]
        public void CanAddBothSentBytesAndReceivedBytesToWebRequestTracer()
        {
            //given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIdProvider, timingProvider)
            {
                BeaconConfiguration = defaultBeaconConfig
            };

            const string testUrl = "https://127.0.0.1";
            var webRequest = CreateWebRequestTracer(target, testUrl);
            var bytesReceived = 12321;
            var bytesSent = 123;

            // when
            webRequest.Start().SetBytesSent(bytesSent).SetBytesReceived(bytesReceived).Stop(-1); //stop will add the web request to the beacon

            // then
            Assert.That(target.EventDataList, Is.EquivalentTo(new[] { $"et=30&na={Uri.EscapeDataString(testUrl)}&it=0&pa=1&s0=1&t0=0&s1=2&t1=0&bs=123&br=12321" }));
        }

        [Test]
        public void CanAddRootActionIfCaptureIsOn()
        {
            // given
            var configuration = new TestConfiguration();
            configuration.EnableCapture();

            var target = new Beacon(logger, new BeaconCache(logger), configuration, "127.0.0.1", threadIdProvider, timingProvider)
            {
                BeaconConfiguration = defaultBeaconConfig
            };

            const string rootActionName = "TestRootAction";


            // when adding the root action
            var action = new RootAction(logger, target, rootActionName, new SynchronizedQueue<IAction>()); // the action is added to the beacon in the constructor
            action.LeaveAction();

            // then
            Assert.That(target.ActionDataList, Is.EquivalentTo(new[] { $"et=1&na={rootActionName}&it=0&ca=1&pa=0&s0=1&t0=0&s1=2&t1=0" }));
        }

        [Test]
        public void CannotAddRootActionIfCaptureIsOff()
        {
            // given
            var configuration = new TestConfiguration();
            configuration.DisableCapture();

            var target = new Beacon(logger, new BeaconCache(logger),
                configuration, "127.0.0.1", threadIdProvider, timingProvider);

            // when adding the root action
            const string rootActionName = "TestRootAction";
            target.AddAction(new RootAction(logger, target, rootActionName, new SynchronizedQueue<IAction>()));

            // then
            Assert.That(target.ActionDataList, Is.Empty);
        }

        [Test]
        public void ClearDataClearsActionAndEventData()
        {
            // given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIdProvider, timingProvider);
            var action = new Action(logger, target, "TestAction", new SynchronizedQueue<IAction>());
            action.ReportEvent("TestEvent").ReportValue("TheAnswerToLifeTheUniverseAndEverything", 42);
            target.AddAction(action);

            // then check data both lists are not empty
            Assert.That(target.ActionDataList, Is.Not.Empty);
            Assert.That(target.EventDataList, Is.Not.Empty);

            // and when clearing the data
            target.ClearData();

            // then check data both lists are empty
            Assert.That(target.ActionDataList, Is.Empty);
            Assert.That(target.EventDataList, Is.Empty);
        }

        [Test]
        public void NoSessionIsAddedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            var beaconConfig = new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            var session = new Session(logger, beaconSender, target); // will

            // when
            target.EndSession(session);

            // then ensure nothing has been serialized
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void NoActionIsAddedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            var beaconConfig = new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            var action = new Action(logger, target, "ActionName", new SynchronizedQueue<IAction>());

            // when
            target.AddAction(action);

            // then ensure nothing has been serialized
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void NoIntValueIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            var beaconConfig = new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            var intValue = 42;
            var parentAction = new Action(logger, target, "ActionName", new SynchronizedQueue<IAction>());

            // when
            target.ReportValue(parentAction, "intValue", intValue);

            // then ensure nothing has been serialized
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void NoDoubleValueIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            var beaconConfig = new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            var doubleValue = Math.E;
            var parentAction = new Action(logger, target, "ActionName", new SynchronizedQueue<IAction>());

            // when
            target.ReportValue(parentAction, "doubleValue", doubleValue);

            // then ensure nothing has been serialized
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void NoStringValueIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            var beaconConfig = new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            var stringValue = "Write once, debug everywhere";
            var parentAction = new Action(logger, target, "ActionName", new SynchronizedQueue<IAction>());

            // when
            target.ReportValue(parentAction, "doubleValue", stringValue);

            // then ensure nothing has been serialized
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void NoEventIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            var beaconConfig = new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            var parentAction = new Action(logger, target, "ActionName", new SynchronizedQueue<IAction>());

            // when
            target.ReportEvent(parentAction, "Event name");

            // then ensure nothing has been serialized
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void NoErrorIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            var beaconConfig = new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            var parentAction = new Action(logger, target, "ActionName", new SynchronizedQueue<IAction>());

            // when
            target.ReportError(parentAction, "Error name", 123, "The reason for this error");

            // then ensure nothing has been serialized
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void NoCrashIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            var beaconConfig = new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            target.ReportCrash("Error name", "The reason for this error", "the stack trace");

            // then ensure nothing has been serialized
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void NoWebRequestIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            var beaconConfig = new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            var webRequestTracer = CreateWebRequestTracer(target, "https://foo.bar");

            // when
            target.AddWebRequest(17, webRequestTracer);

            // then ensure nothing has been serialized
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void NoUserIdentificationIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            var beaconConfig = new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            target.IdentifyUser("jane.doe@acme.com");

            // then ensure nothing has been serialized
            Assert.That(target.IsEmpty, Is.True);
        }


        [Test]
        public void NoWebRequestIsReportedForDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var parent = Substitute.For<OpenKitComposite>();
            parent.ActionId.Returns(17);

            var webRequestTracer = Substitute.For<WebRequestTracer>(logger, parent, target);

            // when
            target.AddWebRequest(17, webRequestTracer);
            _ = webRequestTracer.Received(0).BytesReceived;
            _ = webRequestTracer.Received(0).BytesSent;
            _ = webRequestTracer.Received(0).ResponseCode;

            // then ensure nothing has been serialized
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void WebRequestIsReportedForDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var parent = Substitute.For<OpenKitComposite>();
            parent.ActionId.Returns(17);

            var webRequestTracer = Substitute.For<WebRequestTracer>(logger, parent, target);

            // when
            target.AddWebRequest(17, webRequestTracer);

            _ = webRequestTracer.Received(1).BytesReceived;
            _ = webRequestTracer.Received(1).BytesSent;
            _ = webRequestTracer.Received(1).ResponseCode;

            // then ensure nothing has been serialized
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void WebRequestIsReportedForDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var parent = Substitute.For<OpenKitComposite>();
            parent.ActionId.Returns(17);

            var webRequestTracer = Substitute.For<WebRequestTracer>(logger, parent, target);

            // when
            target.AddWebRequest(17, webRequestTracer);

            _ = webRequestTracer.Received(1).BytesReceived;
            _ = webRequestTracer.Received(1).BytesSent;
            _ = webRequestTracer.Received(1).ResponseCode;

            // then ensure nothing has been serialized
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void CreateTagReturnsEmptyStringForDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            var tagReturned = target.CreateTag(42, 1);

            // then
            Assert.That(tagReturned, Is.Empty);
        }

        [Test]
        public void CreateTagReturnsTagStringForDataCollectionLevel1()
        {
            // given
            long deviceId = 37;
            randomGenerator.NextLong(Arg.Any<long>()).Returns(deviceId);
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            var obtained = target.CreateTag(42, 1);

            // then
            Assert.That(obtained, Is.EqualTo(
                "MT" +                    // tag prefix
                "_3" +                    // protocol version
                "_-1" +                   // server ID
                "_" + deviceId +          // device ID
                "_1" +                    // session number (must always 1 for data collection level performance)
                "_" +                     // application ID
                "_42" +                   // parent action ID
                "_0" +                    // thread ID
                "_1"                      // sequence number
            ));
        }

        [Test]
        public void CreateTagReturnsTagStringForDataCollectionLevel2()
        {
            // given
            int sessionId = 73;
            long deviceId = 37;
            var sessionIdProvider = Substitute.For<ISessionIdProvider>();
            sessionIdProvider.GetNextSessionId().Returns(sessionId);

            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration(deviceId, beaconConfig, sessionIdProvider);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            var obtained = target.CreateTag(42, 1);

            // then
            Assert.That(obtained, Is.EqualTo(
                "MT" +                    // tag prefix
                "_3" +                    // protocol version
                "_-1" +                   // server ID
                "_" + deviceId +          // device ID
                "_" + sessionId +         // session number (must always 1 for data collection level performance)
                "_" +                     // application ID
                "_42" +                   // parent action ID
                "_0" +                    // thread ID
                "_1"                      // sequence number
            ));
        }

        [Test]
        public void CreateWebRequestTagEncodesDeviceIdProperly()
        {
            // given
            var sessionIdProvider = Substitute.For<ISessionIdProvider>();
            sessionIdProvider.GetNextSessionId().Returns(666);
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OPT_IN_CRASHES);
            var config = new TestConfiguration("app_ID", -42, beaconConfig, sessionIdProvider);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            var obtained = target.CreateTag(42, 1);

            // then
            var expectedDeviceID = "MT_3_-1_-42_666_app%5FID_42_0_1";
            Assert.That(obtained, Is.EqualTo(expectedDeviceID));
        }

        [Test]
        public void DeviceIdIsRandomizedOnDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(12345, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(0, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            _ = target.DeviceId;

            // then
            randomGenerator.Received(1).NextLong(long.MaxValue);
        }

        [Test]
        public void DeviceIdIsRandomizedOnDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(12345, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            _ = target.DeviceId;

            // then
            randomGenerator.Received(1).NextLong(long.MaxValue);
        }

        [Test]
        public void GivenDeviceIdIsUsedOnDataCollectionLevel2()
        {
            var deviceID = 12345;
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration(deviceID, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            var obtained = target.DeviceId;

            // then
            randomGenerator.Received(0).NextLong(long.MaxValue);
            Assert.That(obtained, Is.EqualTo(deviceID));
        }

        [Test]
        public void RandomDeviceIdCannotBeNegativeOnDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            var deviceId = target.DeviceId;

            // then
            Assert.That(deviceId, Is.GreaterThanOrEqualTo(0L));
            Assert.That(deviceId, Is.LessThanOrEqualTo(long.MaxValue));
        }

        [Test]
        public void RandomDeviceIdCannotBeNegativeOnDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            var deviceId = target.DeviceId;

            // then
            Assert.That(deviceId, Is.GreaterThanOrEqualTo(0L));
            Assert.That(deviceId, Is.LessThanOrEqualTo(long.MaxValue));
        }

        [Test]
        public void SessionIdIsAlwaysValue1OnDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            var sessionId = target.SessionNumber;

            // then
            Assert.That(sessionId, Is.EqualTo(1));
        }

        [Test]
        public void SessionIdIsAlwaysValue1OnDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            var sessionId = target.SessionNumber;

            // then
            Assert.That(sessionId, Is.EqualTo(1));
        }

        [Test]
        public void SessionIdIsValueFromSessionIdProviderOnDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var sessionIdProvider = Substitute.For<ISessionIdProvider>();
            var config = new TestConfiguration(1, beaconConfig, sessionIdProvider);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            var sessionId = target.SessionNumber;

            // then
            Assert.That(sessionId, Is.EqualTo(target.SessionNumber));
            sessionIdProvider.Received(1).GetNextSessionId();
        }

        [Test]

        public void ActionNotReportedForDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            // when
            target.AddAction(action);

            // then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void ActionReportedForDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            // when
            target.AddAction(action);

            // then
            Assert.That(target.IsEmpty, Is.False);
        }


        [Test]
        public void ActionReportedForDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            // when
            target.AddAction(action);

            // then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void SessionNotReportedForDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var session = new Session(logger, beaconSender, target);

            // when
            target.EndSession(session);

            // then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void SessionReportedForDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var session = new Session(logger, beaconSender, target);

            // when
            target.EndSession(session);

            // then
            Assert.That(target.IsEmpty, Is.False);
        }


        [Test]
        public void SessionReportedForDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            var session = new Session(logger, beaconSender, target);

            // when
            target.EndSession(session);

            // then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void IdentifyUserDoesNotReportOnDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            target.IdentifyUser("test user");

            // then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void IdentifyUserDoesNotReportOnDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            target.IdentifyUser("test user");

            // then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void IdentifyUserDoesReportOnDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            target.IdentifyUser("test user");

            // then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void ReportErrorDoesNotReportOnDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "test action", new SynchronizedQueue<IAction>());

            // when
            target.ReportError(action, "error", 42, "the answer");

            // then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void ReportErrorDoesReportOnDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "test action", new SynchronizedQueue<IAction>());

            // when
            target.ReportError(action, "error", 42, "the answer");

            // then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void ReportErrorDoesReportOnDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "test action", new SynchronizedQueue<IAction>());

            // when
            target.ReportError(action, "error", 42, "the answer");

            // then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void ReportCrashDoesNotReportOnDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            target.ReportCrash("OutOfMemory exception", "insufficient memory", "stacktrace:123");

            // then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void ReportCrashDoesNotReportOnDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OPT_OUT_CRASHES);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            target.ReportCrash("OutOfMemory exception", "insufficient memory", "stacktrace:123");

            // then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void ReportCrashDoesReportOnCrashReportingLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OPT_IN_CRASHES);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            target.ReportCrash("OutOfMemory exception", "insufficient memory", "stacktrace:123");

            // then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void IntValueNotReportedForDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            // when
            target.ReportValue(action, "test int value", 13);

            // then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void IntValueNotReportedForDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            // when
            target.ReportValue(action, "test int value", 13);

            // then
            Assert.That(target.IsEmpty, Is.True);
        }


        [Test]
        public void IntValueReportedForDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            // when
            target.ReportValue(action, "test int value", 13);

            // then
            Assert.That(target.IsEmpty, Is.False);
        }


        [Test]
        public void DoubleValueNotReportedForDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            // when
            target.ReportValue(action, "test double value", 2.71);

            // then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void DoubleValueNotReportedForDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            // when
            target.ReportValue(action, "test double value", 2.71);

            // then
            Assert.That(target.IsEmpty, Is.True);
        }


        [Test]
        public void DoubleValueReportedForDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            // when
            target.ReportValue(action, "test double value", 2.71);

            // then
            Assert.That(target.IsEmpty, Is.False);
        }


        [Test]
        public void StringValueNotReportedForDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            // when
            target.ReportValue(action, "test string value", "test data");

            // then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void StringValueNotReportedForDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            // when
            target.ReportValue(action, "test string value", "test data");

            // then
            Assert.That(target.IsEmpty, Is.True);
        }


        [Test]
        public void StringValueReportedForDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            // when
            target.ReportValue(action, "test string value", "test data");

            // then
            Assert.That(target.IsEmpty, Is.False);
        }


        [Test]
        public void NamedEventNotReportedForDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            // when
            target.ReportEvent(action, "test event");

            // then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void NamedEventNotReportedForDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            // when
            target.ReportEvent(action, "test event");

            // then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void NamedEventReportedForDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            // when
            target.ReportEvent(action, "test event");

            // then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void SessionStartIsReported()
        {
            // given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            // session constructor is calling StartSession implicitly
            _ = new Session(logger, beaconSender, target);

            // then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void SessionStartIsReportedForDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            // session constructor is calling StartSession implicitly
            _ = new Session(logger, beaconSender, target);

            // then
            Assert.That(target.IsEmpty, Is.False);
        }


        [Test]
        public void SessionStartIsReportedForDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            // session constructor is calling StartSession implicitly
            _ = new Session(logger, beaconSender, target);

            // then
            Assert.That(target.IsEmpty, Is.False);
        }


        [Test]
        public void SessionStartIsReportedForDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            // session constructor is calling StartSession implicitly
            _ = new Session(logger, beaconSender, target);

            // then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void NoSessionStartIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            var beaconConfig = new BeaconConfiguration(0, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OPT_IN_CRASHES);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            // session constructor is calling StartSession implicitly
            _ = new Session(logger, beaconSender, target);

            // then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void UseInternalBeaconIdForAccessingBeaconCacheWhenSessionNumberReportingDisallowed()
        {
            // given
            int beaconId = 73;
            var beaconCache = Substitute.For<BeaconCache>(logger);
            var sessionIdProvider = Substitute.For<ISessionIdProvider>();
            sessionIdProvider.GetNextSessionId().Returns(beaconId);

            var beaconConfig = new BeaconConfiguration(0, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OPT_IN_CRASHES);
            var config = new TestConfiguration(1, beaconConfig);
            var target = new Beacon(logger, beaconCache, config, "127.0.0.1", threadIdProvider, timingProvider, randomGenerator);

            // when
            target.ClearData();

            // then
            Assert.That(target.SessionNumber, Is.EqualTo(1));
            beaconCache.Received(1).DeleteCacheEntry(beaconId);
        }

        private WebRequestTracer CreateWebRequestTracer(Beacon beacon, string url)
        {
            var parent = Substitute.For<OpenKitComposite>();
            parent.ActionId.Returns(1);

            return new WebRequestTracer(logger, parent, beacon, url);
        }
    }
}
