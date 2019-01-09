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
using Dynatrace.OpenKit.Protocol;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

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
            var erroneousResponse = new StatusResponse(Substitute.For<ILogger>(), string.Empty, Response.HttpBadRequest, new Dictionary<string, List<string>>());

            // when, then
            Assert.That(BeaconSendingResponseUtil.IsSuccessfulResponse(erroneousResponse), Is.False);
        }

        [Test]
        public void IsSuccessfulResponseReturnsTrueIfResponseIsNotErroneous()
        {
            // given
            var successfulResponse = new StatusResponse(Substitute.For<ILogger>(), string.Empty, Response.HttpOk, new Dictionary<string, List<string>>());

            // when, then
            Assert.That(BeaconSendingResponseUtil.IsSuccessfulResponse(successfulResponse), Is.True);
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
            var erroneousResponse = new StatusResponse(Substitute.For<ILogger>(), string.Empty, Response.HttpBadRequest, new Dictionary<string, List<string>>());

            // when, then
            Assert.That(BeaconSendingResponseUtil.IsTooManyRequestsResponse(erroneousResponse), Is.False);
        }

        [Test]
        public void IsTooManyRequestsResponseReturnsTrueIfResponseCodeIndicatesTooManyRequests()
        {
            // given
            var erroneousResponse = new StatusResponse(Substitute.For<ILogger>(), string.Empty, Response.HttpTooManyRequests, new Dictionary<string, List<string>>());

            // when, then
            Assert.That(BeaconSendingResponseUtil.IsTooManyRequestsResponse(erroneousResponse), Is.True);
        }
    }
}
