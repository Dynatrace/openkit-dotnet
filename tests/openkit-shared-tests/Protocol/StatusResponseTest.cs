//
// Copyright 2018 Dynatrace LLC
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

using NUnit.Framework;
using System;

namespace Dynatrace.OpenKit.Protocol
{
    public class StatusResponseTest
    {

        [Test]
        public void PassingNullResponseStringDoesNotThrow()
        {
            // then
            Assert.That(() => new StatusResponse(null, 200), Throws.Nothing);
        }

        [Test]
        public void PassingEmptyResponseStringDoesNotThrow()
        {
            // then
            Assert.That(() => new StatusResponse(string.Empty, 200), Throws.Nothing);
        }

        [Test]
        public void DefaultCaptureIsOn()
        {
            // given
            var target = new StatusResponse(string.Empty, 200);

            // then
            Assert.That(target.Capture, Is.True);
        }

        [Test]
        public void DefaultSendIntervalIsMinusOne()
        {
            // given
            var target = new StatusResponse(string.Empty, 200);

            // then
            Assert.That(target.SendInterval, Is.EqualTo(-1));
        }

        [Test]
        public void DefaultMonitorNameIsNull()
        {
            // given
            var target = new StatusResponse(string.Empty, 200);

            // then
            Assert.That(target.MonitorName, Is.Null);
        }

        [Test]
        public void DefaultServerIDIsMinusOne()
        {
            // given
            var target = new StatusResponse(string.Empty, 200);

            // then
            Assert.That(target.ServerID, Is.EqualTo(-1));
        }

        [Test]
        public void DefaultMaxBeaconSizeIsMinusOne()
        {
            // given
            var target = new StatusResponse(string.Empty, 200);

            // then
            Assert.That(target.MaxBeaconSize, Is.EqualTo(-1));
        }

        [Test]
        public void DefaultCaptureCrashesIsOn()
        {
            // given
            var target = new StatusResponse(string.Empty, 200);

            // then
            Assert.That(target.CaptureCrashes, Is.True);
        }

        [Test]
        public void DefaultCaptureErrorsIsOn()
        {
            // given
            var target = new StatusResponse(string.Empty, 200);

            // then
            Assert.That(target.CaptureErrors, Is.True);
        }

        [Test]
        public void DefaultMultiplicityIsOne()
        {
            // given
            var target = new StatusResponse(string.Empty, 200);

            // then
            Assert.That(target.Multiplicity, Is.EqualTo(1));
        }

        [Test]
        public void OddNumberOfTokensThrowsException()
        {
            // given
            const string responseString = StatusResponse.RESPONSE_KEY_CAPTURE + "=100"
                + "&" + StatusResponse.RESPONSE_KEY_CAPTURE_CRASHES;

            // when, then
            Assert.That(() => new StatusResponse(responseString, 200),
                Throws.ArgumentException.With.Message.EqualTo("Invalid response; even number of tokens expected."));
        }

        [Test]
        public void AmpersandIsNotAValidKeyValueSeparator()
        {
            // given
            const string responseString = StatusResponse.RESPONSE_KEY_CAPTURE + "&100";

            // when, then
            Assert.That(() => new StatusResponse(responseString, 200),
                Throws.ArgumentException.With.Message.EqualTo("Invalid response; even number of tokens expected."));
        }

        [Test]
        public void CaptureIsTrueWhenItIsEqualToOne()
        {
            // given
            var target = new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE + "=1", 200);

            // then
            Assert.That(target.Capture, Is.True);
        }

        [Test]
        public void CaptureIsFalseWhenItIsNotEqualToOne()
        {
            // when it's a positive number greater than 1, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE + "=2", 200).Capture, Is.False);

            // and when it's zero, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE + "=0", 200).Capture, Is.False);

            // and when it's a negative number, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE + "=-2", 200).Capture, Is.False);
        }

        [Test]
        public void ParsingInvalidNumericValueForCaptureThrowsException()
        {
            // when wrong format is used, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE + "=", 200), Throws.InstanceOf<FormatException>());

            // when wrong format is used, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE + "=a", 200), Throws.InstanceOf<FormatException>());

            // and when numeric overflow (2^31 in this case) occurs, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE + "=2147483648", 200), Throws.InstanceOf<OverflowException>());
        }

        [Test]
        public void ParsingSendInterval()
        {
            // when it's a positive number, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_SEND_INTERVAL + "=1", 200).SendInterval, Is.EqualTo(1000));
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_SEND_INTERVAL + "=1200", 200).SendInterval, Is.EqualTo(1200000));

            // and when it's zero, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_SEND_INTERVAL + "=0", 200).SendInterval, Is.EqualTo(0));

            // and when it's a negative number, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_SEND_INTERVAL + "=-1", 200).SendInterval, Is.EqualTo(-1000));
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_SEND_INTERVAL + "=-42", 200).SendInterval, Is.EqualTo(-42000));
        }

        [Test]
        public void ParsingTooBigSendIntervalOverflows()
        {
            // when the value is positive, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_SEND_INTERVAL + "=2147484", 200).SendInterval, Is.EqualTo(-2147483296));

            // when the value is negative, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_SEND_INTERVAL + "=-2147485", 200).SendInterval, Is.EqualTo(2147482296));
        }

        [Test]
        public void ParsingInvalidSendIntervalThrowsException()
        {
            // when wrong format is used, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_SEND_INTERVAL + "=", 200), Throws.InstanceOf<FormatException>());

            // when wrong format is used, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_SEND_INTERVAL + "=a", 200), Throws.InstanceOf<FormatException>());

            // and when numeric overflow (2^31 in this case) occurs, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_SEND_INTERVAL + "=2147483648", 200), Throws.InstanceOf<OverflowException>());
        }

        [Test]
        public void ParsingMonitorNames()
        {
            // when it's a positive number, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_MONITOR_NAME + "=", 200).MonitorName, Is.EqualTo(string.Empty));
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_MONITOR_NAME + "=foobar", 200).MonitorName, Is.EqualTo("foobar"));
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_MONITOR_NAME + "=1234", 200).MonitorName, Is.EqualTo("1234"));
        }

        [Test]
        public void ServerIDIsParsed()
        {
            // given
            var target = new StatusResponse(StatusResponse.RESPONSE_KEY_SERVER_ID + "=1234", 200);

            // then
            Assert.That(target.ServerID, Is.EqualTo(1234));
        }

        [Test]
        public void ParsingInvalidServerIDThrowsException()
        {
            // when wrong format is used, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_SERVER_ID + "=", 200), Throws.InstanceOf<FormatException>());

            // when wrong format is used, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_SERVER_ID + "=a", 200), Throws.InstanceOf<FormatException>());

            // and when numeric overflow (2^31 in this case) occurs, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_SERVER_ID + "=2147483648", 200), Throws.InstanceOf<OverflowException>());
        }

        [Test]
        public void ParsingMaxBeaconSize()
        {
            // when it's a positive number, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_MAX_BEACON_SIZE + "=1", 200).MaxBeaconSize, Is.EqualTo(1 * 1024));
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_MAX_BEACON_SIZE + "=1200", 200).MaxBeaconSize, Is.EqualTo(1200 * 1024));

            // and when it's zero, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_MAX_BEACON_SIZE + "=0", 200).MaxBeaconSize, Is.EqualTo(0));

            // and when it's a negative number, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_MAX_BEACON_SIZE + "=-1", 200).MaxBeaconSize, Is.EqualTo(-1 * 1024));
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_MAX_BEACON_SIZE + "=-42", 200).MaxBeaconSize, Is.EqualTo(-42 * 1024));
        }

        [Test]
        public void ParsingTooBigMaxBeaconSizeOverflows()
        {
            // when the value is positive, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_MAX_BEACON_SIZE + "=2097152", 200).MaxBeaconSize, Is.EqualTo(-2147483648));

            // when the value is negative, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_MAX_BEACON_SIZE + "=-2097153", 200).MaxBeaconSize, Is.EqualTo(2147482624));
        }

        [Test]
        public void ParsingInvalidMaxBeaconSizeThrowsException()
        {
            // when wrong format is used, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_MAX_BEACON_SIZE + "=", 200), Throws.InstanceOf<FormatException>());

            // when wrong format is used, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_MAX_BEACON_SIZE + "=a", 200), Throws.InstanceOf<FormatException>());

            // and when numeric overflow (2^31 in this case) occurs, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_MAX_BEACON_SIZE + "=2147483648", 200), Throws.InstanceOf<OverflowException>());
        }

        [Test]
        public void CaptureErrorsIsTrueWhenItIsNotEqualToZero()
        {
            // when it's a positive number greater than 1, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE_ERRORS + "=2", 200).CaptureErrors, Is.True);

            // when it's one, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE_ERRORS + "=1", 200).CaptureErrors, Is.True);

            // and when it's a negative number, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE_ERRORS + "=-2", 200).CaptureErrors, Is.True);
        }

        [Test]
        public void CaptureErrorsIsFalseWhenItIsEqualToZero()
        {
            // given
            var target = new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE_ERRORS + "=0", 200);

            // then
            Assert.That(target.CaptureErrors, Is.False);
        }

        [Test]
        public void ParsingInvalidNumericValueForCaptureErrorsThrowsException()
        {
            // when wrong format is used, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE_ERRORS + "=", 200), Throws.InstanceOf<FormatException>());

            // when wrong format is used, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE_ERRORS + "=a", 200), Throws.InstanceOf<FormatException>());

            // and when numeric overflow (2^31 in this case) occurs, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE_ERRORS + "=2147483648", 200), Throws.InstanceOf<OverflowException>());
        }

        [Test]
        public void CaptureCrashesIsTrueWhenItIsNotEqualToZero()
        {
            // when it's a positive number greater than 1, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE_CRASHES + "=2", 200).CaptureCrashes, Is.True);

            // when it's one, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE_CRASHES + "=1", 200).CaptureCrashes, Is.True);

            // and when it's a negative number, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE_CRASHES + "=-2", 200).CaptureCrashes, Is.True);
        }

        [Test]
        public void CaptureCrashesIsFalseWhenItIsEqualToZero()
        {
            // given
            var target = new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE_CRASHES + "=0", 200);

            // then
            Assert.That(target.CaptureCrashes, Is.False);
        }

        [Test]
        public void ParsingInvalidNumericValueForCaptureCrashesThrowsException()
        {
            // when wrong format is used, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE_CRASHES + "=", 200), Throws.InstanceOf<FormatException>());

            // when wrong format is used, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE_CRASHES + "=a", 200), Throws.InstanceOf<FormatException>());

            // and when numeric overflow (2^31 in this case) occurs, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_CAPTURE_CRASHES + "=2147483648", 200), Throws.InstanceOf<OverflowException>());
        }

        [Test]
        public void ParsingMultiplictyWorks()
        {
            // when it's a positive number greater than 1, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_MULTIPLICITY + "=3", 200).Multiplicity, Is.EqualTo(3));

            // when it's one, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_MULTIPLICITY + "=0", 200).Multiplicity, Is.EqualTo(0));

            // and when it's a negative number, then
            Assert.That(new StatusResponse(StatusResponse.RESPONSE_KEY_MULTIPLICITY + "=-5", 200).Multiplicity, Is.EqualTo(-5));
        }


        [Test]
        public void ParsingInvalidNumericValueForMultiplicityThrowsException()
        {
            // when wrong format is used, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_MULTIPLICITY + "=", 200), Throws.InstanceOf<FormatException>());

            // when wrong format is used, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_MULTIPLICITY + "=a", 200), Throws.InstanceOf<FormatException>());

            // and when numeric overflow (2^31 in this case) occurs, then
            Assert.That(() => new StatusResponse(StatusResponse.RESPONSE_KEY_MULTIPLICITY + "=2147483648", 200), Throws.InstanceOf<OverflowException>());
        }

        [Test]
        public void ResponseCodeIsSet()
        {
            // given
            Assert.That(new StatusResponse("key=value", 418).ResponseCode, Is.EqualTo(418));
        }
    }
}
