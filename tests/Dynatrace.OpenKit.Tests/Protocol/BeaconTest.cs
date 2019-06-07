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

using Dynatrace.OpenKit.Providers;
using NUnit.Framework;
using NSubstitute;
using Dynatrace.OpenKit.Core;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Core.Communication;
using Dynatrace.OpenKit.Core.Configuration;
using System.Text;

namespace Dynatrace.OpenKit.Protocol
{
    public class BeaconTest
    {
        private IThreadIDProvider threadIDProvider;
        private ITimingProvider timingProvider;
        private IPRNGenerator randomGenerator;
        private ILogger logger;
        private BeaconSender beaconSender;
        private BeaconConfiguration beaconConfig;

        [SetUp]
        public void Setup()
        {
            threadIDProvider = Substitute.For<IThreadIDProvider>();
            timingProvider = Substitute.For<ITimingProvider>();
            randomGenerator = Substitute.For<IPRNGenerator>();
            logger = Substitute.For<ILogger>();

            var beaconSendingContext = Substitute.For<IBeaconSendingContext>();
            beaconSender = new BeaconSender(logger, beaconSendingContext);
            beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OPT_IN_CRASHES);
        }

        [Test]
        public void DefaultBeaconConfigurationDoesNotDisableCapturing()
        {
            // given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIDProvider, timingProvider);

            // then
            Assert.That(target.CapturingDisabled, Is.False);
        }

        [Test]
        public void DefaultBeaconConfigurationSetsMultiplicityToOne()
        {
            // given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIDProvider, timingProvider);

            // then
            Assert.That(target.Multiplicity, Is.EqualTo(1));
        }

        [Test]
        public void CanAddUserIdentifyEvent()
        {
            // given
            var beacon = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIDProvider, timingProvider)
            {
                BeaconConfiguration = beaconConfig
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
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIDProvider, timingProvider)
            {
                BeaconConfiguration = beaconConfig
            };

            const string testURL = "https://127.0.0.1";
            var webRequest = new WebRequestTracer(logger, target, 1, testURL);
            var bytesSent = 123;

            //when
            webRequest.Start().SetBytesSent(bytesSent).Stop(-1); //stop will add the web request to the beacon

            //then
            Assert.That(target.EventDataList, Is.EquivalentTo(new[] { $"et=30&na={System.Uri.EscapeDataString(testURL)}&it=0&pa=1&s0=1&t0=0&s1=2&t1=0&bs={bytesSent}" }));
        }

        [Test]
        public void CanAddSentBytesValueZeroToWebRequestTracer()
        {
            //given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIDProvider, timingProvider)
            {
                BeaconConfiguration = beaconConfig
            };

            const string testURL = "https://127.0.0.1";
            var webRequest = new WebRequestTracer(logger, target, 1, testURL);
            var bytesSent = 0;

            //when
            webRequest.Start().SetBytesSent(bytesSent).Stop(-1); //stop will add the web request to the beacon

            //then
            Assert.That(target.EventDataList, Is.EquivalentTo(new[] { $"et=30&na={System.Uri.EscapeDataString(testURL)}&it=0&pa=1&s0=1&t0=0&s1=2&t1=0&bs={bytesSent}" }));
        }

        [Test]
        public void CannotAddSentBytesWithInvalidValueSmallerZeroToWebRequestTracer()
        {
            //given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIDProvider, timingProvider)
            {
                BeaconConfiguration = beaconConfig
            };

            const string testURL = "https://127.0.0.1";
            var webRequest = new WebRequestTracer(logger, target, 1, testURL);

            //when
            webRequest.Start().SetBytesSent(-1).Stop(-1); //stop will add the web request to the beacon

            //then
            Assert.That(target.EventDataList, Is.EquivalentTo(new[] { $"et=30&na={System.Uri.EscapeDataString(testURL)}&it=0&pa=1&s0=1&t0=0&s1=2&t1=0" }));
        }

        [Test]
        public void CanAddReceivedBytesToWebRequestTracer()
        {
            //given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIDProvider, timingProvider)
            {
                BeaconConfiguration = beaconConfig
            };

            const string testURL = "https://127.0.0.1";
            var webRequest = new WebRequestTracer(logger, target, 1, testURL);
            var bytesReceived = 12321;

            //when
            webRequest.Start().SetBytesReceived(bytesReceived).Stop(-1); //stop will add the web request to the beacon

            //then
            Assert.That(target.EventDataList, Is.EquivalentTo(new[] { $"et=30&na={System.Uri.EscapeDataString(testURL)}&it=0&pa=1&s0=1&t0=0&s1=2&t1=0&br={bytesReceived}" }));
        }

        [Test]
        public void CanAddReceivedBytesValueZeroToWebRequestTracer()
        {
            //given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIDProvider, timingProvider)
            {
                BeaconConfiguration = beaconConfig
            };

            const string testURL = "https://127.0.0.1";
            var webRequest = new WebRequestTracer(logger, target, 1, testURL);
            var bytesReceived = 0;

            //when
            webRequest.Start().SetBytesReceived(bytesReceived).Stop(-1); //stop will add the web request to the beacon

            //then
            Assert.That(target.EventDataList, Is.EquivalentTo(new[] { $"et=30&na={System.Uri.EscapeDataString(testURL)}&it=0&pa=1&s0=1&t0=0&s1=2&t1=0&br={bytesReceived}" }));
        }

        [Test]
        public void CannotAddReceivedBytesWithInvalidValueSmallerZeroToWebRequestTracer()
        {
            //given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIDProvider, timingProvider)
            {
                BeaconConfiguration = beaconConfig
            };

            const string testURL = "https://127.0.0.1";
            var webRequest = new WebRequestTracer(logger, target, 1, testURL);

            //when
            webRequest.Start().SetBytesReceived(-1).Stop(-1); //stop will add the web request to the beacon

            //then
            Assert.That(target.EventDataList, Is.EquivalentTo(new[] { $"et=30&na={System.Uri.EscapeDataString(testURL)}&it=0&pa=1&s0=1&t0=0&s1=2&t1=0" }));
        }

        [Test]
        public void CanAddBothSentBytesAndReceivedBytesToWebRequestTracer()
        {
            //given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIDProvider, timingProvider)
            {
                BeaconConfiguration = beaconConfig
            };

            const string testURL = "https://127.0.0.1";
            var webRequest = new WebRequestTracer(logger, target, 1, testURL);
            var bytesReceived = 12321;
            var bytesSent = 123;

            //when
            webRequest.Start().SetBytesSent(bytesSent).SetBytesReceived(bytesReceived).Stop(-1); //stop will add the web request to the beacon

            //then
            Assert.That(target.EventDataList, Is.EquivalentTo(new[] { $"et=30&na={System.Uri.EscapeDataString(testURL)}&it=0&pa=1&s0=1&t0=0&s1=2&t1=0&bs=123&br=12321" }));
        }

        [Test]
        public void CanAddRootActionIfCaptureIsOn()
        {
            // given
            var configuration = new TestConfiguration();
            configuration.EnableCapture();

            var target = new Beacon(logger, new BeaconCache(logger), configuration, "127.0.0.1", threadIDProvider, timingProvider)
            {
                BeaconConfiguration = beaconConfig
            };

            const string RootActionName = "TestRootAction";


            // when adding the root action
            var action = new RootAction(logger, target, RootActionName, new SynchronizedQueue<IAction>()); // the action is added to the beacon in the constructor
            action.LeaveAction();

            // then
            Assert.That(target.ActionDataList, Is.EquivalentTo(new[] { $"et=1&na={RootActionName}&it=0&ca=1&pa=0&s0=1&t0=0&s1=2&t1=0" }));
        }

        [Test]
        public void CannotAddRootActionIfCaptureIsOff()
        {
            // given
            var configuration = new TestConfiguration();
            configuration.DisableCapture();

            var target = new Beacon(logger, new BeaconCache(logger),
                configuration, "127.0.0.1", threadIDProvider, timingProvider);

            // when adding the root action
            const string RootActionName = "TestRootAction";
            target.AddAction(new RootAction(logger, target, RootActionName, new SynchronizedQueue<IAction>()));

            // then
            Assert.That(target.ActionDataList, Is.Empty);
        }

        [Test]
        public void ClearDataClearsActionAndEventData()
        {
            // given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIDProvider, timingProvider);
            var action = new Action(logger, target, "TestAction", new SynchronizedQueue<IAction>());
            action.ReportEvent("TestEvent").ReportValue("TheAnswerToLifeTheUniverseAndEverything", 42);
            target.AddAction(action);

            // then check data both lists are not empty
            Assert.That(target.ActionDataList, Is.Not.Empty);
            Assert.That(target.EventDataList, Is.Not.Empty);

            // and when clearing the data
            target.ClearData();

            // then check data both lists are emtpy
            Assert.That(target.ActionDataList, Is.Empty);
            Assert.That(target.EventDataList, Is.Empty);
        }

        [Test]
        public void NoSessionIsAddedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            var beaconConfig = new BeaconConfiguration(0, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

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
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

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
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

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
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            var doubleValue = System.Math.E;
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
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

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
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

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
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

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
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

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
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            var webRequestTracer = new WebRequestTracer(logger, target, 17, "https://foo.bar");

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
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

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
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            var webRequestTracer = Substitute.For<WebRequestTracer>(logger, target, 17);

            //when
            target.AddWebRequest(17, webRequestTracer);
            var temp = webRequestTracer.Received(0).BytesReceived;
            temp = webRequestTracer.Received(0).BytesSent;
            temp = webRequestTracer.Received(0).ResponseCode;

            // then ensure nothing has been serialized
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void WebRequestIsReportedForDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            var webRequestTracer = Substitute.For<WebRequestTracer>(logger, target, 17);

            //when
            target.AddWebRequest(17, webRequestTracer);

            var temp = webRequestTracer.Received(1).BytesReceived;
            temp = webRequestTracer.Received(1).BytesSent;
            temp = webRequestTracer.Received(1).ResponseCode;

            // then ensure nothing has been serialized
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void WebRequestIsReportedForDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            var webRequestTracer = Substitute.For<WebRequestTracer>(logger, target, 17);

            //when
            target.AddWebRequest(17, webRequestTracer);

            var temp = webRequestTracer.Received(1).BytesReceived;
            temp = webRequestTracer.Received(1).BytesSent;
            temp = webRequestTracer.Received(1).ResponseCode;

            // then ensure nothing has been serialized
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void CreateTagReturnsEmptyStringForDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            //when
            var tagReturned = target.CreateTag(42, 1);

            //then
            Assert.That(tagReturned, Is.Empty);
        }

        [Test]
        public void CreateTagReturnsTagStringForDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            //when
            var obtained = target.CreateTag(42, 1);

            //then
            Assert.That(obtained, Is.Not.Empty);
        }

        [Test]
        public void CreateTagReturnsTagStringForDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            //when
            var obtained = target.CreateTag(42, 1);

            //then
            Assert.That(obtained, Is.Not.Empty);
        }

        [Test]
        public void CreateWebRequestTagEncodesDeviceIDPropperly()
        {
            // given
            var sessionIDProvider = Substitute.For<ISessionIDProvider>();
            sessionIDProvider.GetNextSessionID().Returns(666);
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OPT_IN_CRASHES);
            var config = new TestConfiguration("app_ID", "device_id/", beaconConfig, sessionIDProvider);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            //when
            var obtained = target.CreateTag(42, 1);

            // then
            var expectedDeviceID = "MT_3_-1_device%5Fid%2F_666_app%5FID_42_0_1";
            Assert.That(obtained, Is.EqualTo(expectedDeviceID));
        }

        [Test]
        public void DeviceIDIsRandomizedOnDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(12345, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration("0", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            //when
            var deviceID = target.DeviceID;

            //then
            randomGenerator.Received(1).NextLong(long.MaxValue);
        }

        [Test]
        public void DeviceIDIsRandomizedOnDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(12345, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            //when
            var deviceID = target.DeviceID;

            //then
            randomGenerator.Received(1).NextLong(long.MaxValue);
        }

        [Test]
        public void GivenDeviceIDIsUsedOnDataCollectionLevel2()
        {
            var deviceID = "12345";
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration(deviceID, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            //when
            var obtained = target.DeviceID;

            //then
            randomGenerator.Received(0).NextLong(long.MaxValue);
            Assert.That(obtained, Is.EqualTo(deviceID));
        }

        [Test]
        public void RandomDeviceIDCannotBeNegativeOnDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            //when
            var deviceID = long.Parse(target.DeviceID);

            //then
            Assert.That(deviceID, Is.GreaterThanOrEqualTo(0L));
            Assert.That(deviceID, Is.LessThanOrEqualTo(long.MaxValue));
        }

        [Test]
        public void RandomDeviceIDCannotBeNegativeOnDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            //when
            var deviceID = long.Parse(target.DeviceID);

            //then
            Assert.That(deviceID, Is.GreaterThanOrEqualTo(0L));
            Assert.That(deviceID, Is.LessThanOrEqualTo(long.MaxValue));
        }

        [Test]
        public void DeviceIDIsTruncatedTo250Characters()
        {
            // given
            var deviceIDBuilder = new StringBuilder();
            for (var i = 0; i < Beacon.MaximumNameLength - 1; i++)
            {
                deviceIDBuilder.Append("a");
            }
            deviceIDBuilder.Append("bc");
            var deviceID = deviceIDBuilder.ToString();
            var config = new TestConfiguration(deviceID, beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            // when
            var obtained = target.DeviceID;

            // then
            Assert.That(obtained.Length, Is.EqualTo(Beacon.MaximumNameLength));
            Assert.That(obtained, Is.EqualTo(deviceID.Substring(0, Beacon.MaximumNameLength)));
        }

        [Test]
        public void SessionIDIsAlwaysValue1OnDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            //when
            var sessionID = target.SessionNumber;

            //then
            Assert.That(sessionID, Is.EqualTo(1));
        }

        [Test]
        public void SessionIDIsAlwaysValue1OnDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            //when
            var sessionID = target.SessionNumber;

            //then
            Assert.That(sessionID, Is.EqualTo(1));
        }

        [Test]
        public void SessionIDIsValueFromSessionIDProviderOnDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var sessionIDProvider = Substitute.For<ISessionIDProvider>();
            var config = new TestConfiguration("1", beaconConfig, sessionIDProvider);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            //when
            var sessionID = target.SessionNumber;

            //then
            Assert.That(sessionID, Is.EqualTo(target.SessionNumber));
            sessionIDProvider.Received(1).GetNextSessionID();
        }

        [Test]

        public void ActionNotReportedForDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            //when
            target.AddAction(action);

            //then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void ActionReportedForDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            //when
            target.AddAction(action);

            //then
            Assert.That(target.IsEmpty, Is.False);
        }


        [Test]
        public void ActionReportedForDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            //when
            target.AddAction(action);

            //then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void SessionNotReportedForDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var session = new Session(logger, beaconSender, target);

            //when
            target.EndSession(session);

            //then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void SessionReportedForDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var session = new Session(logger, beaconSender, target);

            //when
            target.EndSession(session);

            //then
            Assert.That(target.IsEmpty, Is.False);
        }


        [Test]
        public void SessionReportedForDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            var session = new Session(logger, beaconSender, target);

            //when
            target.EndSession(session);

            //then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void IdentifyUserDoesNotReportOnDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            //when
            target.IdentifyUser("test user");

            //then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void IdentifyUserDoesNotReportOnDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            //when
            target.IdentifyUser("test user");

            //then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void IdentifyUserDoesReportOnDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);

            //when
            target.IdentifyUser("test user");

            //then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void ReportErrorDoesNotReportOnDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "test action", new SynchronizedQueue<IAction>());

            //when
            target.ReportError(action, "error", 42, "the answer");

            //then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void ReportErrorDoesReportOnDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "test action", new SynchronizedQueue<IAction>());

            //when
            target.ReportError(action, "error", 42, "the answer");

            //then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void ReportErrorDoesReportOnDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "test action", new SynchronizedQueue<IAction>());

            //when
            target.ReportError(action, "error", 42, "the answer");

            //then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void ReportCrashDoesNotReportOnDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "test action", new SynchronizedQueue<IAction>());

            //when
            target.ReportCrash("OutOfMemory exception", "insufficient memory", "stacktrace:123");

            //then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void ReportCrashDoesNotReportOnDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OPT_OUT_CRASHES);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "test action", new SynchronizedQueue<IAction>());

            //when
            target.ReportCrash("OutOfMemory exception", "insufficient memory", "stacktrace:123");

            //then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void ReportCrashDoesReportOnCrashReportingLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OPT_IN_CRASHES);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "test action", new SynchronizedQueue<IAction>());

            //when
            target.ReportCrash("OutOfMemory exception", "insufficient memory", "stacktrace:123");

            //then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void IntValueNotReportedForDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            //when
            target.ReportValue(action, "test int value", 13);

            //then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void IntValueNotReportedForDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            //when
            target.ReportValue(action, "test int value", 13);

            //then
            Assert.That(target.IsEmpty, Is.True);
        }


        [Test]
        public void IntValueReportedForDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            //when
            target.ReportValue(action, "test int value", 13);

            //then
            Assert.That(target.IsEmpty, Is.False);
        }


        [Test]
        public void DoubleValueNotReportedForDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            //when
            target.ReportValue(action, "test double value", 2.71);

            //then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void DoubleValueNotReportedForDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            //when
            target.ReportValue(action, "test double value", 2.71);

            //then
            Assert.That(target.IsEmpty, Is.True);
        }


        [Test]
        public void DoubleValueReportedForDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            //when
            target.ReportValue(action, "test double value", 2.71);

            //then
            Assert.That(target.IsEmpty, Is.False);
        }


        [Test]
        public void StringValueNotReportedForDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            //when
            target.ReportValue(action, "test string value", "test data");

            //then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void StringValueNotReportedForDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            //when
            target.ReportValue(action, "test string value", "test data");

            //then
            Assert.That(target.IsEmpty, Is.True);
        }


        [Test]
        public void StringValueReportedForDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            //when
            target.ReportValue(action, "test string value", "test data");

            //then
            Assert.That(target.IsEmpty, Is.False);
        }


        [Test]
        public void NamedEventNotReportedForDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            //when
            target.ReportEvent(action, "test event");

            //then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void NamedEventNotReportedForDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            //when
            target.ReportEvent(action, "test event");

            //then
            Assert.That(target.IsEmpty, Is.True);
        }

        [Test]
        public void NamedEventReportedForDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var action = new Action(logger, target, "TestRootAction", new SynchronizedQueue<IAction>());

            //when
            target.ReportEvent(action, "test event");

            //then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void SessionStartIsReported()
        {
            // given
            var target = new Beacon(logger, new BeaconCache(logger), new TestConfiguration(), "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var session = new Session(logger, beaconSender, target);

            //when
            // session contructor is calling StartSession implicitly

            //then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void SessionStartIsReportedForDataCollectionLevel0()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.OFF, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var session = new Session(logger, beaconSender, target);

            //when
            // session contructor is calling StartSession implicitly

            //then
            Assert.That(target.IsEmpty, Is.False);
        }


        [Test]
        public void SessionStartIsReportedForDataCollectionLevel1()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var session = new Session(logger, beaconSender, target);

            //when
            // session contructor is calling StartSession implicitly

            //then
            Assert.That(target.IsEmpty, Is.False);
        }


        [Test]
        public void SessionStartIsReportedForDataCollectionLevel2()
        {
            // given
            var beaconConfig = new BeaconConfiguration(1, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var session = new Session(logger, beaconSender, target);

            //when
            // session contructor is calling StartSession implicitly

            //then
            Assert.That(target.IsEmpty, Is.False);
        }

        [Test]
        public void NoSessionStartIsReportedIfBeaconConfigurationDisablesCapturing()
        {
            // given
            var beaconConfig = new BeaconConfiguration(0, DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OPT_IN_CRASHES);
            var config = new TestConfiguration("1", beaconConfig);
            var target = new Beacon(logger, new BeaconCache(logger), config, "127.0.0.1", threadIDProvider, timingProvider, randomGenerator);
            var session = new Session(logger, beaconSender, target);

            //when
            // session contructor is calling StartSession implicitly

            //then
            Assert.That(target.IsEmpty, Is.True);
        }
    }
}
