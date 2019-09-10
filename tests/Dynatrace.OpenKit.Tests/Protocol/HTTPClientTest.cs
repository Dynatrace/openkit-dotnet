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
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Util;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Dynatrace.OpenKit.Protocol
{
    public class HTTPClientTest
    {
        private ILogger mockLogger;
        private ISSLTrustManager trustManager;
        private StubHTTPClient spyClient;

        private const string BaseURL = "https://localhost";
        private const int ServerID = 1;
        private const string ApplicationID = "ApplicationID";

        private static readonly string MonitorURL = BaseURL
            + "?" + HTTPClient.RequestTypeMobile
            + "&" + HTTPClient.QueryKeyServerId + "=" + ServerID
            + "&" + HTTPClient.QueryKeyApplication + "=" + ApplicationID
            + "&" + HTTPClient.QueryKeyVersion + "=" + ProtocolConstants.OpenKitVersion
            + "&" + HTTPClient.QueryKeyPlatformType + "=" + ProtocolConstants.PlatformTypeOpenKit
            + "&" + HTTPClient.QueryKeyAgentTechnologyType + "=" + ProtocolConstants.AgentTechnologyType;

        private static readonly string NewSessionUrl = MonitorURL
            + $"&{HTTPClient.QueryKeyNewSession}=1";

        private static readonly HTTPClient.HTTPResponse StatusResponse = new HTTPClient.HTTPResponse
        {
            ResponseCode = 200,
            Response = HTTPClient.RequestTypeMobile,
            Headers = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>()
        };

        private static readonly HTTPClient.HTTPResponse InvalidStatusResponseShort = new HTTPClient.HTTPResponse
        {
            ResponseCode = 200,
            Response = "type=mts",
            Headers = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>()
        };

        private static readonly HTTPClient.HTTPResponse InvalidStatusResponseLong = new HTTPClient.HTTPResponse
        {
            ResponseCode = 200,
            Response = "type=mts&t1=-1&t2=-1",
            Headers = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>()
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
            var httpConfiguration = new HTTPClientConfiguration(BaseURL, ServerID, ApplicationID, trustManager);
            spyClient = Substitute.ForPartsOf<StubHTTPClient>(mockLogger, httpConfiguration);
        }

        [Test]
        public void SendStatusRequestSendsOneHTTPGetRequest()
        {
            // given
            HTTPClient target = spyClient;
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
        public void SendStatusRequestSendRequestToMonitorURL()
        {
            // given
            HTTPClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(StatusResponse);


            // when
            var obtained = target.SendStatusRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(200));

            spyClient.Received(1).DoGetRequest(MonitorURL, null);
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
            HTTPClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(new HTTPClient.HTTPResponse { ResponseCode = 200, Headers = headers, Response = null });

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
            HTTPClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(new HTTPClient.HTTPResponse { ResponseCode = 400, Headers = headers, Response = null });

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
            HTTPClient target = spyClient;
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
            HTTPClient target = spyClient;
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
            HTTPClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(
                new HTTPClient.HTTPResponse { ResponseCode = 200, Headers = new Dictionary<string, List<string>>(), Response = StatusResponse.Response + "&cp=a" });

            // when
            var obtained = target.SendStatusRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(int.MaxValue));
            Assert.That(obtained.Headers, Is.Empty);

            spyClient.ReceivedWithAnyArgs(1).DoGetRequest(string.Empty, string.Empty);
        }

        [Test]
        public void SendNewSessionRequestSendsOneHTTPGetRequest()
        {
            // given
            HTTPClient target = spyClient;
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
        public void SendNewSessionRequestSendRequestToNewSessionURL()
        {
            // given
            HTTPClient target = spyClient;
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
            HTTPClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(new HTTPClient.HTTPResponse { ResponseCode = 200, Headers = headers, Response = null });

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
            HTTPClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(new HTTPClient.HTTPResponse { ResponseCode = 400, Headers = headers, Response = null });

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
            HTTPClient target = spyClient;
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
            HTTPClient target = spyClient;
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
            HTTPClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoGetRequest(string.Empty, string.Empty)).DoNotCallBase();
            spyClient.DoGetRequest(string.Empty, string.Empty).ReturnsForAnyArgs(
                new HTTPClient.HTTPResponse { ResponseCode = 200, Headers = new Dictionary<string, List<string>>(), Response = StatusResponse.Response + "&cp=a" });

            // when
            var obtained = target.SendNewSessionRequest();

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(int.MaxValue));
            Assert.That(obtained.Headers, Is.Empty);

            spyClient.ReceivedWithAnyArgs(1).DoGetRequest(string.Empty, string.Empty);
        }

        [Test]
        public void SendBeaconRequestSendsOneHTTPPostRequest()
        {
            // given
            HTTPClient target = spyClient;
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
            HTTPClient target = spyClient;
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
            HTTPClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoPostRequest(string.Empty, string.Empty, null)).DoNotCallBase();
            spyClient.DoPostRequest(string.Empty, string.Empty, null).ReturnsForAnyArgs(StatusResponse);

            // when
            var obtained = target.SendBeaconRequest("175.45.176.1", System.Text.Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog"));

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(200));

            spyClient.Received(1).DoPostRequest(Arg.Is<string>(x => !string.IsNullOrEmpty(x)),
                                                Arg.Is<string>(x => !string.IsNullOrEmpty(x)),
                                                Arg.Is<byte[]>(x => System.Text.Encoding.UTF8.GetString(Unzip(x)) == "The quick brown fox jumps over the lazy dog"));
        }

        [Test]
        public void SendBeaconRequestSendsRequestToMonitorURL()
        {
            // given
            HTTPClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoPostRequest(string.Empty, string.Empty, null)).DoNotCallBase();
            spyClient.DoPostRequest(string.Empty, string.Empty, null).ReturnsForAnyArgs(StatusResponse);

            // when
            var obtained = target.SendBeaconRequest("192.168.0.1", null);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ResponseCode, Is.EqualTo(200));

            spyClient.Received(1).DoPostRequest(Arg.Is<string>(x => x == MonitorURL),
                                                Arg.Is<string>(x => !string.IsNullOrEmpty(x)),
                                                null);
        }

        [Test]
        public void SendBeaconRequestForwardsClientIP()
        {
            // given
            HTTPClient target = spyClient;
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
            HTTPClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoPostRequest(string.Empty, string.Empty, null)).DoNotCallBase();
            spyClient.DoPostRequest(string.Empty, string.Empty, null).ReturnsForAnyArgs(new HTTPClient.HTTPResponse { ResponseCode = 200, Headers = headers, Response = null });

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
            HTTPClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoPostRequest(string.Empty, string.Empty, null)).DoNotCallBase();
            spyClient.DoPostRequest(string.Empty, string.Empty, null).ReturnsForAnyArgs(new HTTPClient.HTTPResponse { ResponseCode = 400, Headers = headers, Response = null });

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
            HTTPClient target = spyClient;
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
            HTTPClient target = spyClient;
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
            HTTPClient target = spyClient;
            spyClient.WhenForAnyArgs(x => x.DoPostRequest(string.Empty, string.Empty, null)).DoNotCallBase();
            spyClient.DoPostRequest(string.Empty, string.Empty, null).ReturnsForAnyArgs(
                new HTTPClient.HTTPResponse { ResponseCode = 200, Headers = new Dictionary<string, List<string>>(), Response = StatusResponse.Response + "&cp=a" });

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
        public class StubHTTPClient : HTTPClient
        {
            public StubHTTPClient(ILogger logger, HTTPClientConfiguration configuration) : base(logger, configuration)
            {
            }

            public virtual HTTPResponse DoGetRequest(string url, string clientIPAddress)
            {
                throw new NotImplementedException();
            }

            protected override HTTPResponse GetRequest(string url, string clientIPAddress)
            {
                return DoGetRequest(url, clientIPAddress);
            }

            public virtual HTTPResponse DoPostRequest(string url, string clientIPAddress, byte[] gzippedPayload)
            {
                throw new NotImplementedException();
            }

            protected override HTTPResponse PostRequest(string url, string clientIPAddress, byte[] gzippedPayload)
            {
                return DoPostRequest(url, clientIPAddress, gzippedPayload);
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
