//
// Copyright 2018-2020 Dynatrace LLC
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

using Dynatrace.OpenKit.Protocol;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Communication
{
    public class BeaconSendingResponseUtilTest
    {
        [Test]
        public void IsSuccessfulResponseReturnsFalseIfResponseIsNull()
        {
            // when, then
            Assert.That(BeaconSendingResponseUtil.IsSuccessfulResponse(null), Is.False);
        }

        [Test]
        public void IsSuccessfulResponseReturnsFalseIfResponseIsErroneous()
        {
            // given
            var response = Substitute.For<IStatusResponse>();
            response.IsErroneousResponse.Returns(true);

            // when
            var obtained = BeaconSendingResponseUtil.IsSuccessfulResponse(response);

            // then
            _ = response.Received(1).IsErroneousResponse;
            Assert.That(obtained, Is.False);
        }

        [Test]
        public void IsSuccessfulResponseReturnsTrueIfResponseIsNotErroneous()
        {
            // given
            var response = Substitute.For<IStatusResponse>();
            response.IsErroneousResponse.Returns(false);

            // when
            var obtained = BeaconSendingResponseUtil.IsSuccessfulResponse(response);

            // then
            Assert.That(obtained, Is.True);
            _ = response.Received(1).IsErroneousResponse;
        }

        [Test]
        public void IsTooManyRequestsResponseReturnsFalseIfResponseIsNull()
        {
            // when, then
            Assert.That(BeaconSendingResponseUtil.IsTooManyRequestsResponse(null), Is.False);
        }

        [Test]
        public void IsTooManyRequestsResponseReturnsFalseIfResponseCodeIsNotEqualToTooManyRequestsCode()
        {
            // given
            var response = Substitute.For<IStatusResponse>();
            response.ResponseCode.Returns(StatusResponse.HttpBadRequest);

            // when
            var obtained = BeaconSendingResponseUtil.IsTooManyRequestsResponse(response);

            // then
            Assert.That(obtained, Is.False);
            _ = response.Received(1).ResponseCode;
        }

        [Test]
        public void IsTooManyRequestsResponseReturnsTrueIfResponseCodeIndicatesTooManyRequests()
        {
            // given
            var response = Substitute.For<IStatusResponse>();
            response.ResponseCode.Returns(StatusResponse.HttpTooManyRequests);

            // when
            var obtained = BeaconSendingResponseUtil.IsTooManyRequestsResponse(response);

            // then
            Assert.That(obtained, Is.True);
            _ = response.Received(1).ResponseCode;
        }
    }
}
