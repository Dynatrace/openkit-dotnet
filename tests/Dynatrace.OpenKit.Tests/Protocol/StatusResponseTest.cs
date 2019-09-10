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
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Protocol
{
    public class StatusResponseTest
    {
        private ILogger logger;

        [SetUp]
        public void SetUp()
        {
            logger = Substitute.For<ILogger>();
        }

        [Test]
        public void PassingNullResponseStringDoesNotThrow()
        {
            // then
            Assert.That(() => new StatusResponse(logger, null, 200, new Dictionary<string, List<string>>()), Throws.Nothing);
        }

        [Test]
        public void PassingEmptyResponseStringDoesNotThrow()
        {
            // then
            Assert.That(() => new StatusResponse(logger, string.Empty, 200, new Dictionary<string, List<string>>()), Throws.Nothing);
        }

        [Test]
        public void DefaultCaptureIsOn()
        {
            // given
            var target = new StatusResponse(logger, string.Empty, 200, new Dictionary<string, List<string>>());

            // then
            Assert.That(target.Capture, Is.True);
        }

        [Test]
        public void DefaultSendIntervalIsMinusOne()
        {
            // given
            var target = new StatusResponse(logger, string.Empty, 200, new Dictionary<string, List<string>>());

            // then
            Assert.That(target.SendInterval, Is.EqualTo(-1));
        }

        [Test]
        public void DefaultMonitorNameIsNull()
        {
            // given
            var target = new StatusResponse(logger, string.Empty, 200, new Dictionary<string, List<string>>());

            // then
            Assert.That(target.MonitorName, Is.Null);
        }

        [Test]
        public void DefaultServerIDIsMinusOne()
        {
            // given
            var target = new StatusResponse(logger, string.Empty, 200, new Dictionary<string, List<string>>());

            // then
            Assert.That(target.ServerId, Is.EqualTo(-1));
        }

        [Test]
        public void DefaultMaxBeaconSizeIsMinusOne()
        {
            // given
            var target = new StatusResponse(logger, string.Empty, 200, new Dictionary<string, List<string>>());

            // then
            Assert.That(target.MaxBeaconSize, Is.EqualTo(-1));
        }

        [Test]
        public void DefaultCaptureCrashesIsOn()
        {
            // given
            var target = new StatusResponse(logger, string.Empty, 200, new Dictionary<string, List<string>>());

            // then
            Assert.That(target.CaptureCrashes, Is.True);
        }

        [Test]
        public void DefaultCaptureErrorsIsOn()
        {
            // given
            var target = new StatusResponse(logger, string.Empty, 200, new Dictionary<string, List<string>>());

            // then
            Assert.That(target.CaptureErrors, Is.True);
        }

        [Test]
        public void DefaultMultiplicityIsOne()
        {
            // given
            var target = new StatusResponse(logger, string.Empty, 200, new Dictionary<string, List<string>>());

            // then
            Assert.That(target.Multiplicity, Is.EqualTo(1));
        }

        [Test]
        public void OddNumberOfTokensThrowsException()
        {
            // given
            const string responseString = StatusResponse.ResponseKeyCapture + "=100"
                + "&" + StatusResponse.ResponseKeyCaptureCrashes;

            // when, then
            Assert.That(() => new StatusResponse(logger, responseString, 200, new Dictionary<string, List<string>>()),
                Throws.ArgumentException.With.Message.EqualTo("Invalid response; even number of tokens expected."));
        }

        [Test]
        public void AmpersandIsNotAValidKeyValueSeparator()
        {
            // given
            const string responseString = StatusResponse.ResponseKeyCapture + "&100";

            // when, then
            Assert.That(() => new StatusResponse(logger, responseString, 200, new Dictionary<string, List<string>>()),
                Throws.ArgumentException.With.Message.EqualTo("Invalid response; even number of tokens expected."));
        }

        [Test]
        public void CaptureIsTrueWhenItIsEqualToOne()
        {
            // given
            var target = new StatusResponse(logger, StatusResponse.ResponseKeyCapture + "=1", 200, new Dictionary<string, List<string>>());

            // then
            Assert.That(target.Capture, Is.True);
        }

        [Test]
        public void CaptureIsFalseWhenItIsNotEqualToOne()
        {
            // when it's a positive number greater than 1, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyCapture + "=2", 200, new Dictionary<string, List<string>>()).Capture, Is.False);

            // and when it's zero, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyCapture + "=0", 200, new Dictionary<string, List<string>>()).Capture, Is.False);

            // and when it's a negative number, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyCapture + "=-2", 200, new Dictionary<string, List<string>>()).Capture, Is.False);
        }

        [Test]
        public void ParsingInvalidNumericValueForCaptureThrowsException()
        {
            // when wrong format is used, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyCapture + "=", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<FormatException>());

            // when wrong format is used, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyCapture + "=a", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<FormatException>());

            // and when numeric overflow (2^31 in this case) occurs, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyCapture + "=2147483648", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<OverflowException>());
        }

        [Test]
        public void ParsingSendInterval()
        {
            // when it's a positive number, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeySendInterval + "=1", 200, new Dictionary<string, List<string>>()).SendInterval, Is.EqualTo(1000));
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeySendInterval + "=1200", 200, new Dictionary<string, List<string>>()).SendInterval, Is.EqualTo(1200000));

            // and when it's zero, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeySendInterval + "=0", 200, new Dictionary<string, List<string>>()).SendInterval, Is.EqualTo(0));

            // and when it's a negative number, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeySendInterval + "=-1", 200, new Dictionary<string, List<string>>()).SendInterval, Is.EqualTo(-1000));
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeySendInterval + "=-42", 200, new Dictionary<string, List<string>>()).SendInterval, Is.EqualTo(-42000));
        }

        [Test]
        public void ParsingTooBigSendIntervalOverflows()
        {
            // when the value is positive, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeySendInterval + "=2147484", 200, new Dictionary<string, List<string>>()).SendInterval, Is.EqualTo(-2147483296));

            // when the value is negative, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeySendInterval + "=-2147485", 200, new Dictionary<string, List<string>>()).SendInterval, Is.EqualTo(2147482296));
        }

        [Test]
        public void ParsingInvalidSendIntervalThrowsException()
        {
            // when wrong format is used, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeySendInterval + "=", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<FormatException>());

            // when wrong format is used, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeySendInterval + "=a", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<FormatException>());

            // and when numeric overflow (2^31 in this case) occurs, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeySendInterval + "=2147483648", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<OverflowException>());
        }

        [Test]
        public void ParsingMonitorNames()
        {
            // when it's a positive number, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyMonitorName + "=", 200, new Dictionary<string, List<string>>()).MonitorName, Is.EqualTo(string.Empty));
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyMonitorName + "=foobar", 200, new Dictionary<string, List<string>>()).MonitorName, Is.EqualTo("foobar"));
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyMonitorName + "=1234", 200, new Dictionary<string, List<string>>()).MonitorName, Is.EqualTo("1234"));
        }

        [Test]
        public void ServerIDIsParsed()
        {
            // given
            var target = new StatusResponse(logger, StatusResponse.ResponseKeyServerId + "=1234", 200, new Dictionary<string, List<string>>());

            // then
            Assert.That(target.ServerId, Is.EqualTo(1234));
        }

        [Test]
        public void ParsingInvalidServerIDThrowsException()
        {
            // when wrong format is used, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyServerId + "=", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<FormatException>());

            // when wrong format is used, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyServerId + "=a", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<FormatException>());

            // and when numeric overflow (2^31 in this case) occurs, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyServerId + "=2147483648", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<OverflowException>());
        }

        [Test]
        public void ParsingMaxBeaconSize()
        {
            // when it's a positive number, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyMaxBeaconSize + "=1", 200, new Dictionary<string, List<string>>()).MaxBeaconSize, Is.EqualTo(1 * 1024));
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyMaxBeaconSize + "=1200", 200, new Dictionary<string, List<string>>()).MaxBeaconSize, Is.EqualTo(1200 * 1024));

            // and when it's zero, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyMaxBeaconSize + "=0", 200, new Dictionary<string, List<string>>()).MaxBeaconSize, Is.EqualTo(0));

            // and when it's a negative number, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyMaxBeaconSize + "=-1", 200, new Dictionary<string, List<string>>()).MaxBeaconSize, Is.EqualTo(-1 * 1024));
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyMaxBeaconSize + "=-42", 200, new Dictionary<string, List<string>>()).MaxBeaconSize, Is.EqualTo(-42 * 1024));
        }

        [Test]
        public void ParsingTooBigMaxBeaconSizeOverflows()
        {
            // when the value is positive, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyMaxBeaconSize + "=2097152", 200, new Dictionary<string, List<string>>()).MaxBeaconSize, Is.EqualTo(-2147483648));

            // when the value is negative, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyMaxBeaconSize + "=-2097153", 200, new Dictionary<string, List<string>>()).MaxBeaconSize, Is.EqualTo(2147482624));
        }

        [Test]
        public void ParsingInvalidMaxBeaconSizeThrowsException()
        {
            // when wrong format is used, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyMaxBeaconSize + "=", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<FormatException>());

            // when wrong format is used, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyMaxBeaconSize + "=a", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<FormatException>());

            // and when numeric overflow (2^31 in this case) occurs, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyMaxBeaconSize + "=2147483648", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<OverflowException>());
        }

        [Test]
        public void CaptureErrorsIsTrueWhenItIsNotEqualToZero()
        {
            // when it's a positive number greater than 1, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyCaptureErrors + "=2", 200, new Dictionary<string, List<string>>()).CaptureErrors, Is.True);

            // when it's one, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyCaptureErrors + "=1", 200, new Dictionary<string, List<string>>()).CaptureErrors, Is.True);

            // and when it's a negative number, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyCaptureErrors + "=-2", 200, new Dictionary<string, List<string>>()).CaptureErrors, Is.True);
        }

        [Test]
        public void CaptureErrorsIsFalseWhenItIsEqualToZero()
        {
            // given
            var target = new StatusResponse(logger, StatusResponse.ResponseKeyCaptureErrors + "=0", 200, new Dictionary<string, List<string>>());

            // then
            Assert.That(target.CaptureErrors, Is.False);
        }

        [Test]
        public void ParsingInvalidNumericValueForCaptureErrorsThrowsException()
        {
            // when wrong format is used, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyCaptureErrors + "=", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<FormatException>());

            // when wrong format is used, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyCaptureErrors + "=a", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<FormatException>());

            // and when numeric overflow (2^31 in this case) occurs, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyCaptureErrors + "=2147483648", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<OverflowException>());
        }

        [Test]
        public void CaptureCrashesIsTrueWhenItIsNotEqualToZero()
        {
            // when it's a positive number greater than 1, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyCaptureCrashes + "=2", 200, new Dictionary<string, List<string>>()).CaptureCrashes, Is.True);

            // when it's one, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyCaptureCrashes + "=1", 200, new Dictionary<string, List<string>>()).CaptureCrashes, Is.True);

            // and when it's a negative number, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyCaptureCrashes + "=-2", 200, new Dictionary<string, List<string>>()).CaptureCrashes, Is.True);
        }

        [Test]
        public void CaptureCrashesIsFalseWhenItIsEqualToZero()
        {
            // given
            var target = new StatusResponse(logger, StatusResponse.ResponseKeyCaptureCrashes + "=0", 200, new Dictionary<string, List<string>>());

            // then
            Assert.That(target.CaptureCrashes, Is.False);
        }

        [Test]
        public void ParsingInvalidNumericValueForCaptureCrashesThrowsException()
        {
            // when wrong format is used, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyCaptureCrashes + "=", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<FormatException>());

            // when wrong format is used, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyCaptureCrashes + "=a", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<FormatException>());

            // and when numeric overflow (2^31 in this case) occurs, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyCaptureCrashes + "=2147483648", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<OverflowException>());
        }

        [Test]
        public void ParsingMultiplictyWorks()
        {
            // when it's a positive number greater than 1, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyMultiplicity + "=3", 200, new Dictionary<string, List<string>>()).Multiplicity, Is.EqualTo(3));

            // when it's one, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyMultiplicity + "=0", 200, new Dictionary<string, List<string>>()).Multiplicity, Is.EqualTo(0));

            // and when it's a negative number, then
            Assert.That(new StatusResponse(logger, StatusResponse.ResponseKeyMultiplicity + "=-5", 200, new Dictionary<string, List<string>>()).Multiplicity, Is.EqualTo(-5));
        }
        
        [Test]
        public void ParsingInvalidNumericValueForMultiplicityThrowsException()
        {
            // when wrong format is used, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyMultiplicity + "=", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<FormatException>());

            // when wrong format is used, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyMultiplicity + "=a", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<FormatException>());

            // and when numeric overflow (2^31 in this case) occurs, then
            Assert.That(() => new StatusResponse(logger, StatusResponse.ResponseKeyMultiplicity + "=2147483648", 200, new Dictionary<string, List<string>>()), Throws.InstanceOf<OverflowException>());
        }
    }
}
