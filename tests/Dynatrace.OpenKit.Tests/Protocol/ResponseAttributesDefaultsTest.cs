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

using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Protocol
{
    public class ResponseAttributesDefaultsTest
    {
        #region Json resposne tests

        [Test]
        public void DefaultJsonBeaconSize()
        {
            Assert.That(ResponseAttributesDefaults.JsonResponse.MaxBeaconSizeInBytes, Is.EqualTo(150 * 1024)); // 150 kB
        }

        [Test]
        public void DefaultJsonSessionDuration()
        {
            Assert.That(ResponseAttributesDefaults.JsonResponse.MaxSessionDurationInMilliseconds,
                Is.EqualTo(360 * 60 * 1000)); // 360 minutes
        }

        [Test]
        public void DefaultJsonEventsPerSession()
        {
            Assert.That(ResponseAttributesDefaults.JsonResponse.MaxEventsPerSession, Is.EqualTo(200));
        }

        [Test]
        public void DefaultJsonSessionTimeout()
        {
            Assert.That(ResponseAttributesDefaults.JsonResponse.SessionTimeoutInMilliseconds,
                Is.EqualTo(600 * 1000)); // 600 sec
        }

        [Test]
        public void DefaultJsonSendInterval()
        {
            Assert.That(ResponseAttributesDefaults.JsonResponse.SendIntervalInMilliseconds,
                Is.EqualTo(120 * 1000)); // 120 sec
        }

        [Test]
        public void DefaultJsonVisitStoreVersion()
        {
            Assert.That(ResponseAttributesDefaults.JsonResponse.VisitStoreVersion, Is.EqualTo(1));
        }

        [Test]
        public void DefaultJsonIsCapture()
        {
            Assert.That(ResponseAttributesDefaults.JsonResponse.IsCapture, Is.EqualTo(true));
        }

        [Test]
        public void DefaultJsonIsCaptureCrashes()
        {
            Assert.That(ResponseAttributesDefaults.JsonResponse.IsCaptureCrashes, Is.EqualTo(true));
        }

        [Test]
        public void DefaultJsonIsCaptureErrors()
        {
            Assert.That(ResponseAttributesDefaults.JsonResponse.IsCaptureErrors, Is.EqualTo(true));
        }

        [Test]
        public void DefaultJsonMultiplicity()
        {
            Assert.That(ResponseAttributesDefaults.JsonResponse.Multiplicity, Is.EqualTo(1));
        }

        [Test]
        public void DefaultJsonServerId()
        {
            Assert.That(ResponseAttributesDefaults.JsonResponse.ServerId, Is.EqualTo(1));
        }

        [Test]
        public void DefaultJsonTimestamp()
        {
            Assert.That(ResponseAttributesDefaults.JsonResponse.TimestampInMilliseconds, Is.EqualTo(0L));
        }

        [Test]
        public void DefaultJsonMergeReturnsPassedValue()
        {
            // given
            var responseAttributes = Substitute.For<IResponseAttributes>();

            // when
            var obtained = ResponseAttributesDefaults.JsonResponse.Merge(responseAttributes);

            // then
            Assert.That(obtained, Is.SameAs(responseAttributes));
        }

        #endregion

        #region Key/Value pair response tests

        [Test]
        public void DefaultKeyValueBeaconSize()
        {
            Assert.That(ResponseAttributesDefaults.KeyValueResponse.MaxBeaconSizeInBytes,
                Is.EqualTo(30 * 1024)); // 30 kB
        }

        [Test]
        public void DefaultKeyValueSessionDuration()
        {
            Assert.That(ResponseAttributesDefaults.KeyValueResponse.MaxSessionDurationInMilliseconds,
                Is.EqualTo(-1)); // not set
        }

        [Test]
        public void DefaultKeyValueEventsPerSession()
        {
            Assert.That(ResponseAttributesDefaults.KeyValueResponse.MaxEventsPerSession, Is.EqualTo(-1)); // not set
        }

        [Test]
        public void DefaultKeyValueSessionTimeout()
        {
            Assert.That(ResponseAttributesDefaults.KeyValueResponse.SessionTimeoutInMilliseconds,
                Is.EqualTo(-1)); // not set
        }

        [Test]
        public void DefaultKeyValueSendInterval()
        {
            Assert.That(ResponseAttributesDefaults.KeyValueResponse.SendIntervalInMilliseconds,
                Is.EqualTo(120000)); // 120 sec
        }

        [Test]
        public void DefaultKeyValueVisitStoreVersion()
        {
            Assert.That(ResponseAttributesDefaults.KeyValueResponse.VisitStoreVersion, Is.EqualTo(1));
        }

        [Test]
        public void DefaultKeyValueIsCapture()
        {
            Assert.That(ResponseAttributesDefaults.KeyValueResponse.IsCapture, Is.EqualTo(true));
        }

        [Test]
        public void DefaultKeyValueIsCaptureCrashes()
        {
            Assert.That(ResponseAttributesDefaults.KeyValueResponse.IsCaptureCrashes, Is.EqualTo(true));
        }

        [Test]
        public void DefaultKeyValueIsCaptureErrors()
        {
            Assert.That(ResponseAttributesDefaults.KeyValueResponse.IsCaptureErrors, Is.EqualTo(true));
        }

        [Test]
        public void DefaultKeyValueMultiplicity()
        {
            Assert.That(ResponseAttributesDefaults.KeyValueResponse.Multiplicity, Is.EqualTo(1));
        }

        [Test]
        public void DefaultKeyValueServerId()
        {
            Assert.That(ResponseAttributesDefaults.KeyValueResponse.ServerId, Is.EqualTo(1));
        }

        [Test]
        public void DefaultKeyValueTimestamp()
        {
            Assert.That(ResponseAttributesDefaults.KeyValueResponse.TimestampInMilliseconds, Is.EqualTo(0L));
        }

        [Test]
        public void DefaultKeyValueMergeReturnsPassedValue()
        {
            // given
            var responseAttributes = Substitute.For<IResponseAttributes>();

            // when
            var obtained = ResponseAttributesDefaults.KeyValueResponse.Merge(responseAttributes);

            // then
            Assert.That(obtained, Is.SameAs(responseAttributes));
        }

        #endregion

        #region Undefined defaults tests

        [Test]
        public void DefaultUndefinedBeaconSize()
        {
            Assert.That(ResponseAttributesDefaults.Undefined.MaxBeaconSizeInBytes, Is.EqualTo(30 * 1024));
        }

        [Test]
        public void DefaultUndefinedSessionDuration()
        {
            Assert.That(ResponseAttributesDefaults.Undefined.MaxSessionDurationInMilliseconds,
                Is.EqualTo(-1)); // not set
        }

        [Test]
        public void DefaultUndefinedEventsPerSession()
        {
            Assert.That(ResponseAttributesDefaults.Undefined.MaxEventsPerSession, Is.EqualTo(-1)); // not set
        }

        [Test]
        public void DefaultUndefinedSessionTimeout()
        {
            Assert.That(ResponseAttributesDefaults.Undefined.SessionTimeoutInMilliseconds, Is.EqualTo(-1)); // not set
        }

        [Test]
        public void DefaultUndefinedSendInterval()
        {
            Assert.That(ResponseAttributesDefaults.Undefined.SendIntervalInMilliseconds,
                Is.EqualTo(120 * 1000)); // 120 sec
        }

        [Test]
        public void DefaultUndefinedVisitStoreVersion()
        {
            Assert.That(ResponseAttributesDefaults.Undefined.VisitStoreVersion, Is.EqualTo(1));
        }

        [Test]
        public void DefaultUndefinedIsCapture()
        {
            Assert.That(ResponseAttributesDefaults.Undefined.IsCapture, Is.True);
        }

        [Test]
        public void DefaultUndefinedIsCaptureCrashes()
        {
            Assert.That(ResponseAttributesDefaults.Undefined.IsCaptureCrashes, Is.True);
        }

        [Test]
        public void DefaultUndefinedIsCaptureErrors()
        {
            Assert.That(ResponseAttributesDefaults.Undefined.IsCaptureErrors, Is.True);
        }

        [Test]
        public void DefaultUndefinedMultiplicity()
        {
            Assert.That(ResponseAttributesDefaults.Undefined.Multiplicity, Is.EqualTo(1));
        }

        [Test]
        public void DefaultUndefinedServerId()
        {
            Assert.That(ResponseAttributesDefaults.Undefined.ServerId, Is.EqualTo(-1));
        }

        [Test]
        public void DefaultUndefinedTimestamp()
        {
            Assert.That(ResponseAttributesDefaults.Undefined.TimestampInMilliseconds, Is.EqualTo(0L));
        }

        [Test]
        public void DefaultUndefinedMergeReturnsPassedValue()
        {
            // given
            var responseAttributes = Substitute.For<IResponseAttributes>();

            // when
            var obtained = ResponseAttributesDefaults.Undefined.Merge(responseAttributes);

            // then
            Assert.That(obtained, Is.SameAs(responseAttributes));
        }

        #endregion
    }
}