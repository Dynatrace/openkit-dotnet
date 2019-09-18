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
using System.IO;
using System.IO.Compression;
using System.Text;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Util;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Protocol
{
    public class HttpClientTest
    {
        private ILogger mockLogger;
        private ISSLTrustManager trustManager;
        private StubHttpClient spyClient;

        private const string BaseUrl = "https://localhost";
        private const int ServerId = 1;
        private const string ApplicationId = "ApplicationID";

        private static readonly string MonitorUrl = BaseUrl
            + "?" + HttpClient.RequestTypeMobile
            + "&" + HttpClient.QueryKeyServerId + "=" + ServerId
            + "&" + HttpClient.QueryKeyApplication + "=" + ApplicationId
            + "&" + HttpClient.QueryKeyVersion + "=" + ProtocolConstants.OpenKitVersion
            + "&" + HttpClient.QueryKeyPlatformType + "=" + ProtocolConstants.PlatformTypeOpenKit
            + "&" + HttpClient.QueryKeyAgentTechnologyType + "=" + ProtocolConstants.AgentTechnologyType;

        private static readonly string NewSessionUrl = MonitorUrl
            + $"&{HttpClient.QueryKeyNewSession}=1";

        private static readonly HttpClient.HttpResponse StatusResponse = new HttpClient.HttpResponse
        {
            ResponseCode = 200,
            Response = HttpClient.RequestTypeMobile,
            Headers = new Dictionary<string, List<string>>()
        };

        private static readonly HttpClient.HttpResponse InvalidStatusResponseShort = new HttpClient.HttpResponse
        {
            ResponseCode = 200,
            Response = "type=mts",
            Headers = new Dictionary<string, List<string>>()
        };

        private static readonly HttpClient.HttpResponse InvalidStatusResponseLong = new HttpClient.HttpResponse
        {
            ResponseCode = 200,
            Response = "type=mts&t1=-1&t2=-1",
            Headers = new Dictionary<string, List<string>>()
        };

        [SetUp]
        public void SetUp()
        {
            // mock logger
            mockLogger = Substitute.For<ILogger>();
            mockLogger.IsDebugEnabled.Returns(true);
            mockLogger.IsInfoEnabled.Returns(true);
            mockLogger.IsWarnEnabled.Returns(true);
            mockLogger.IsErrorEnabled.Returns(true);

            // mock trust manager
            trustManager = Substitute.For<ISSLTrustManager>();

            // HTTPClient spy
            var httpConfiguration = new HttpClientConfiguration(BaseUrl, ServerId, ApplicationId, trustManager);
            spyClient = Substitute.ForPartsOf<StubHttpClient>(mockLogger, httpConfiguration);
        }

        [Test]
        public void SendStatusRequestSendsOneHttpGetRequest()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(StatusResponse);


            // when
            var obtained = target.SendStatusRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(200));

            spyClient.ReceivedWithAnyArgs(1).DoGetRequest(string.Empty, string.Empty);
        }

        [Test]
        public void SendStatusRequestSendRequestToMonitorUrl()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(StatusResponse);


            // when
            var obtained = target.SendStatusRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(200));

            spyClient.Received(1).DoGetRequest(MonitorUrl, null);
        }

        [Test]
        public void SendStatusRequestWorksIfResponseIsNull()
        {
            // given
            var headers = new Dictionary<string, List<string>>
            {
                {"Content-Length", new List<string> {"42"} },
                {"Content-Type", new List<string> { "application/json" } }
            };
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(new HttpClient.HttpResponse { ResponseCode = 200, Headers = headers, Response = null });

            // when
            var obtained = target.SendStatusRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(200));
            Assert.That(obtained.Headers, Is.EqualTo(headers));

            spyClient.ReceivedWithAnyArgs(1).DoGetRequest(string.Empty, string.Empty);
        }

        [Test]
        public void SendStatusRequestReturnsErrorCode()
        {
            // given
            var headers = new Dictionary<string, List<string>>
            {
                {"Content-Length", new List<string> {"42"} },
                {"Content-Type", new List<string> { "application/json" } }
            };
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(new HttpClient.HttpResponse { ResponseCode = 400, Headers = headers, Response = null });

            // when
            var obtained = target.SendStatusRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(400));
            Assert.That(obtained.Headers, Is.EqualTo(headers));

            spyClient.ReceivedWithAnyArgs(1).DoGetRequest(string.Empty, string.Empty);
        }

        [Test]
        public void SendStatusRequestIsRetriedThreeTimesBeforeGivingUp()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).Do(x => throw new Exception("dummy"));

            // when
            var obtained = target.SendStatusRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(int.MaxValue));
            Assert.That(obtained.Headers, Is.Empty);

            spyClient.ReceivedWithAnyArgs(3).DoGetRequest(string.Empty, string.Empty);
        }

        [Test]
        public void SendStatusRequestReturnsAnUnknownErrorResponseForWrongHttpResponse()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();

            // when
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(InvalidStatusResponseShort);
            var obtained = target.SendStatusRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(int.MaxValue));
            Assert.That(obtained.Headers, Is.Empty);

            // and when
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(InvalidStatusResponseLong);
            obtained = target.SendStatusRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(int.MaxValue));
            Assert.That(obtained.Headers, Is.Empty);

            spyClient.ReceivedWithAnyArgs(2).DoGetRequest(string.Empty, string.Empty);
        }

        [Test]
        public void SendStatusRequestReturnsAnUnknownErrorResponseForUnparseableStatusResponse()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(
                new HttpClient.HttpResponse { ResponseCode = 200, Headers = new Dictionary<string, List<string>>(), Response = StatusResponse.Response + "&cp=a" });

            // when
            var obtained = target.SendStatusRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(int.MaxValue));
            Assert.That(obtained.Headers, Is.Empty);

            spyClient.ReceivedWithAnyArgs(1).DoGetRequest(string.Empty, string.Empty);
        }

        [Test]
        public void SendNewSessionRequestSendsOneHttpGetRequest()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(StatusResponse);


            // when
            var obtained = target.SendNewSessionRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(200));

            spyClient.ReceivedWithAnyArgs(1).DoGetRequest(string.Empty, string.Empty);
        }

        [Test]
        public void SendNewSessionRequestSendRequestToNewSessionUrl()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(StatusResponse);


            // when
            var obtained = target.SendNewSessionRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(200));

            spyClient.Received(1).DoGetRequest(NewSessionUrl, null);
        }

        [Test]
        public void SendNewSessionRequestWorksIfResponseIsNull()
        {
            // given
            var headers = new Dictionary<string, List<string>>
            {
                {"Content-Length", new List<string> {"42"} },
                {"Content-Type", new List<string> { "application/json" } }
            };
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(new HttpClient.HttpResponse { ResponseCode = 200, Headers = headers, Response = null });

            // when
            var obtained = target.SendNewSessionRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(200));
            Assert.That(obtained.Headers, Is.EqualTo(headers));

            spyClient.ReceivedWithAnyArgs(1).DoGetRequest(string.Empty, string.Empty);
        }

        [Test]
        public void SendNewSessionRequestReturnsErrorCode()
        {
            // given
            var headers = new Dictionary<string, List<string>>
            {
                {"Content-Length", new List<string> {"42"} },
                {"Content-Type", new List<string> { "application/json" } }
            };
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(new HttpClient.HttpResponse { ResponseCode = 400, Headers = headers, Response = null });

            // when
            var obtained = target.SendNewSessionRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(400));
            Assert.That(obtained.Headers, Is.EqualTo(headers));

            spyClient.ReceivedWithAnyArgs(1).DoGetRequest(string.Empty, string.Empty);
        }

        [Test]
        public void SendNewSessionRequestIsRetriedThreeTimesBeforeGivingUp()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).Do(x => throw new Exception("dummy"));

            // when
            var obtained = target.SendNewSessionRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(int.MaxValue));
            Assert.That(obtained.Headers, Is.Empty);

            spyClient.ReceivedWithAnyArgs(3).DoGetRequest(string.Empty, string.Empty);
        }

        [Test]
        public void SendNewSessionRequestReturnsAnUnknownErrorResponseForWrongHttpResponse()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();

            // when
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(InvalidStatusResponseShort);
            var obtained = target.SendNewSessionRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(int.MaxValue));
            Assert.That(obtained.Headers, Is.Empty);

            // and when
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(InvalidStatusResponseLong);
            obtained = target.SendNewSessionRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(int.MaxValue));
            Assert.That(obtained.Headers, Is.Empty);

            spyClient.ReceivedWithAnyArgs(2).DoGetRequest(string.Empty, string.Empty);
        }

        [Test]
        public void SendNewSessionRequestReturnsAnUnknownErrorResponseForUnparseableStatusResponse()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(
                new HttpClient.HttpResponse { ResponseCode = 200, Headers = new Dictionary<string, List<string>>(), Response = StatusResponse.Response + "&cp=a" });

            // when
            var obtained = target.SendNewSessionRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(int.MaxValue));
            Assert.That(obtained.Headers, Is.Empty);

            spyClient.ReceivedWithAnyArgs(1).DoGetRequest(string.Empty, string.Empty);
        }

        [Test]
        public void SendBeaconRequestSendsOneHttpPostRequest()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoPostRequest(string.Empty, string.Empty, null)).DoNotCallBase();
            spyClient.DoPostRequest(string.Empty, string.Empty, null).ReturnsForAnyArgs(StatusResponse);


            // when
            var obtained = target.SendBeaconRequest("175.45.176.1", new byte[] { 0xba, 0xad, 0xbe, 0xef });

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(200));

            spyClient.ReceivedWithAnyArgs(1).DoPostRequest(string.Empty, string.Empty, null);
        }

        [Test]
        public void SendBeaconRequestSendsNullDataIfNullWasGiven()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoPostRequest(string.Empty, string.Empty, null)).DoNotCallBase();
            spyClient.DoPostRequest(string.Empty, string.Empty, null).ReturnsForAnyArgs(StatusResponse);

            // when
            var obtained = target.SendBeaconRequest("175.45.176.1", null);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(200));

            spyClient.Received(1).DoPostRequest(Arg.Is<string>(x => !string.IsNullOrEmpty(x)), Arg.Is<string>(x => !string.IsNullOrEmpty(x)), null);
        }

        [Test]
        public void SendBeaconRequestSendsGzipCompressedDataIfNonNullDataWasGiven()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoPostRequest(string.Empty, string.Empty, null)).DoNotCallBase();
            spyClient.DoPostRequest(string.Empty, string.Empty, null).ReturnsForAnyArgs(StatusResponse);

            // when
            var obtained = target.SendBeaconRequest("175.45.176.1", Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog"));

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(200));

            spyClient.Received(1).DoPostRequest(Arg.Is<string>(x => !string.IsNullOrEmpty(x)),
                                                Arg.Is<string>(x => !string.IsNullOrEmpty(x)),
                                                Arg.Is<byte[]>(x => Encoding.UTF8.GetString(Unzip(x)) == "The quick brown fox jumps over the lazy dog"));
        }

        [Test]
        public void SendBeaconRequestSendsRequestToMonitorUrl()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoPostRequest(string.Empty, string.Empty, null)).DoNotCallBase();
            spyClient.DoPostRequest(string.Empty, string.Empty, null).ReturnsForAnyArgs(StatusResponse);

            // when
            var obtained = target.SendBeaconRequest("192.168.0.1", null);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(200));

            spyClient.Received(1).DoPostRequest(Arg.Is<string>(x => x == MonitorUrl),
                                                Arg.Is<string>(x => !string.IsNullOrEmpty(x)),
                                                null);
        }

        [Test]
        public void SendBeaconRequestForwardsClientIp()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoPostRequest(string.Empty, string.Empty, null)).DoNotCallBase();
            spyClient.DoPostRequest(string.Empty, string.Empty, null).ReturnsForAnyArgs(StatusResponse);

            // when
            var obtained = target.SendBeaconRequest("156.33.241.5", null);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(200));

            spyClient.Received(1).DoPostRequest(Arg.Is<string>(x => !string.IsNullOrEmpty(x)),
                                                Arg.Is<string>(x => x == "156.33.241.5"),
                                                null);
        }

        [Test]
        public void SendBeaconRequestWorksIfResponseIsNull()
        {
            // given
            var headers = new Dictionary<string, List<string>>
            {
                {"Content-Length", new List<string> {"42"} },
                {"Content-Type", new List<string> { "application/json" } }
            };
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoPostRequest(string.Empty, string.Empty, null)).DoNotCallBase();
            spyClient.DoPostRequest(string.Empty, string.Empty, null).ReturnsForAnyArgs(new HttpClient.HttpResponse { ResponseCode = 200, Headers = headers, Response = null });

            // when
            var obtained = target.SendBeaconRequest("156.33.241.5", null);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(200));
            Assert.That(obtained.Headers, Is.EqualTo(headers));

            spyClient.ReceivedWithAnyArgs(1).DoPostRequest(string.Empty, string.Empty, null);
        }

        [Test]
        public void SendBeaconRequestReturnsErrorCode()
        {
            // given
            var headers = new Dictionary<string, List<string>>
            {
                {"Content-Length", new List<string> {"42"} },
                {"Content-Type", new List<string> { "application/json" } }
            };
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoPostRequest(string.Empty, string.Empty, null)).DoNotCallBase();
            spyClient.DoPostRequest(string.Empty, string.Empty, null).ReturnsForAnyArgs(new HttpClient.HttpResponse { ResponseCode = 400, Headers = headers, Response = null });

            // when
            var obtained = target.SendBeaconRequest("156.33.241.5", null);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(400));
            Assert.That(obtained.Headers, Is.EqualTo(headers));

            spyClient.ReceivedWithAnyArgs(1).DoPostRequest(string.Empty, string.Empty, null);
        }

        [Test]
        public void SendBeaconRequestIsRetriedThreeTimesBeforeGivingUp()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoPostRequest(string.Empty, string.Empty, null)).DoNotCallBase();
            spyClient.WhenForAnyArgs(x => x.DoPostRequest(string.Empty, string.Empty, null)).Do(x => throw new Exception("dummy"));

            // when
            var obtained = target.SendBeaconRequest("156.33.241.5", null);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(int.MaxValue));
            Assert.That(obtained.Headers, Is.Empty);

            spyClient.ReceivedWithAnyArgs(3).DoPostRequest(string.Empty, string.Empty, null);
        }

        [Test]
        public void SendBeaconRequestReturnsAnUnknownErrorResponseForWrongHttpResponse()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoPostRequest(string.Empty, string.Empty, null)).DoNotCallBase();

            // when
            spyClient.DoPostRequest(string.Empty, string.Empty, null).ReturnsForAnyArgs(InvalidStatusResponseShort);
            var obtained = target.SendBeaconRequest("156.33.241.5", null);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(int.MaxValue));
            Assert.That(obtained.Headers, Is.Empty);

            // and when
            spyClient.DoPostRequest(string.Empty, string.Empty, null).ReturnsForAnyArgs(InvalidStatusResponseLong);
            obtained = target.SendBeaconRequest("156.33.241.5", null);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(int.MaxValue));
            Assert.That(obtained.Headers, Is.Empty);

            spyClient.ReceivedWithAnyArgs(2).DoPostRequest(string.Empty, string.Empty, null);
        }

        [Test]
        public void SendBeaconRequestReturnsAnUnknownErrorResponseForUnparseableStatusResponse()
        {
            // given
            HttpClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoPostRequest(string.Empty, string.Empty, null)).DoNotCallBase();
            spyClient.DoPostRequest(string.Empty, string.Empty, null).ReturnsForAnyArgs(
                new HttpClient.HttpResponse { ResponseCode = 200, Headers = new Dictionary<string, List<string>>(), Response = StatusResponse.Response + "&cp=a" });

            // when
            var obtained = target.SendBeaconRequest("156.33.241.5", null);


            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(int.MaxValue));
            Assert.That(obtained.Headers, Is.Empty);

            spyClient.ReceivedWithAnyArgs(1).DoPostRequest(string.Empty, string.Empty, null);
        }

        /// <summary>
        /// Stub class for NSubstitute to work.
        /// </summary>
        public class StubHttpClient : HttpClient
        {
            public StubHttpClient(ILogger logger, HttpClientConfiguration configuration) : base(logger, configuration)
            {
            }

            public virtual HttpResponse DoGetRequest(string url, string clientIpAddress)
            {
                throw new NotImplementedException();
            }

            protected override HttpResponse GetRequest(string url, string clientIpAddress)
            {
                return DoGetRequest(url, clientIpAddress);
            }

            public virtual HttpResponse DoPostRequest(string url, string clientIpAddress, byte[] gzippedPayload)
            {
                throw new NotImplementedException();
            }

            protected override HttpResponse PostRequest(string url, string clientIpAddress, byte[] gzippedPayload)
            {
                return DoPostRequest(url, clientIpAddress, gzippedPayload);
            }
        }

        private static byte[] Unzip(byte[] data)
        {
            byte[] result;

            using (var inputStream = new MemoryStream(data))
            {
                using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress, true))
                {
                    using (var outputStream = new MemoryStream())
                    {
                        gzipStream.CopyTo(outputStream);
                        result = outputStream.ToArray();
                    }
                }
            }

            return result;
        }
    }
}
