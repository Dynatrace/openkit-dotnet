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
using Dynatrace.OpenKit.Core.Util;
using Dynatrace.OpenKit.Util;

namespace Dynatrace.OpenKit.Protocol
{
    /// <summary>
    ///  HTTP client helper which abstracts the 2 basic request types:
    ///  - status check
    ///  - beacon send
    /// </summary>
    public abstract class HttpClient : IHttpClient
    {
        public class RequestType
        {
            public static readonly RequestType Status = new RequestType("Status"); // status check
            public static readonly RequestType Beacon = new RequestType("Beacon"); // beacon send
            public static readonly RequestType NewSession = new RequestType("NewSession"); // new session

            public string RequestName { get; }

            private RequestType(string requestName)
            {
                RequestName = requestName;
            }
        }

        public class HttpResponse
        {
            public string Response { get; set; }
            public int ResponseCode { get; set; }
            public Dictionary<string, List<string>> Headers { get; set; }
        }

        // request type constants
        internal const string RequestTypeMobile = "type=m";

        // query parameter constants
        internal const string QueryKeyServerId = "srvid";
        internal const string QueryKeyApplication = "app";
        internal const string QueryKeyVersion = "va";
        internal const string QueryKeyPlatformType = "pt";
        internal const string QueryKeyAgentTechnologyType = "tt";
        internal const string QueryKeyResponseType = "resp";
        internal const string QueryKeyConfigTimestamp = "cts";
        internal const string QueryKeyNewSession = "ns";

        // additional reserved characters for URL encoding
        private static readonly char[] QueryReservedCharacters = {'_'};

        // connection constants
        private const int MaxSendRetries = 3;
        private const int RetrySleepTime = 200; // retry sleep time in ms

        // URLs for requests
        private readonly string monitorUrl;
        private readonly string newSessionUrl;

        private readonly ILogger logger;
        private readonly IInterruptibleThreadSuspender threadSuspender;

        #region constructors

        protected HttpClient(ILogger logger, IHttpClientConfiguration configuration,
            IInterruptibleThreadSuspender threadSuspender)
        {
            this.logger = logger;
            this.threadSuspender = threadSuspender;
            ServerId = configuration.ServerId;
            monitorUrl = BuildMonitorUrl(configuration.BaseUrl, configuration.ApplicationId, ServerId);
            newSessionUrl = BuildNewSessionUrl(configuration.BaseUrl, configuration.ApplicationId, ServerId);
        }

        #endregion

        public int ServerId { get; }

        // sends a status check request and returns a status response
        public IStatusResponse SendStatusRequest(IAdditionalQueryParameters additionalParameters)
        {
            var url = AppendAdditionalQueryParameters(monitorUrl, additionalParameters);
            return SendRequest(RequestType.Status, url, null, null, "GET")
                   ?? StatusResponse.CreateErrorResponse(logger, int.MaxValue);
        }

        // sends a beacon send request and returns a status response
        public IStatusResponse SendBeaconRequest(string clientIpAddress, byte[] data,
            IAdditionalQueryParameters additionalParameters)
        {
            var url = AppendAdditionalQueryParameters(monitorUrl, additionalParameters);
            return SendRequest(RequestType.Beacon, url, clientIpAddress, data, "POST")
                   ?? StatusResponse.CreateErrorResponse(logger, int.MaxValue);
        }

        public IStatusResponse SendNewSessionRequest(IAdditionalQueryParameters additionalParameters)
        {
            var url = AppendAdditionalQueryParameters(newSessionUrl, additionalParameters);
            return SendRequest(RequestType.NewSession, url, null, null, "GET")
                   ?? StatusResponse.CreateErrorResponse(logger, int.MaxValue);
        }

        // generic request send with some verbose output and exception handling
        protected IStatusResponse SendRequest(RequestType requestType, string url, string clientIpAddress, byte[] data,
            string method)
        {
            try
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug(GetType().Name + " HTTP " + requestType.RequestName + " Request: " + url);
                }

                return SendRequestInternal(requestType, url, clientIpAddress, data, method);
            }
            catch (Exception e)
            {
                logger.Error(GetType().Name + " " + requestType.RequestName + " Request failed!", e);
            }

            return UnknownErrorResponse(requestType);
        }

        // generic internal request send
        protected IStatusResponse SendRequestInternal(RequestType requestType, string url, string clientIpAddress,
            byte[] data, string method)
        {
            int retry = 1;
            while (true)
            {
                try
                {
                    // if beacon data is available gzip it
                    byte[] gzippedData = null;
                    if ((data != null) && (data.Length > 0))
                    {
                        gzippedData = CompressByteArray(data);

                        if (logger.IsDebugEnabled)
                        {
                            logger.Debug(GetType().Name + " Beacon Payload: " +
                                         Encoding.UTF8.GetString(data, 0, data.Length));
                        }
                    }

                    HttpResponse httpResponse;
                    if (method == "GET")
                    {
                        httpResponse = GetRequest(url, clientIpAddress);
                    }
                    else if (method == "POST")
                    {
                        httpResponse = PostRequest(url, clientIpAddress, gzippedData);
                    }
                    else
                    {
                        return UnknownErrorResponse(requestType);
                    }

                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug(GetType().Name + " HTTP Response: " + httpResponse.Response);
                        logger.Debug(GetType().Name + " HTTP Response Code: " + httpResponse.ResponseCode);
                    }

                    // create typed response based on request type and response content
                    if ((requestType.RequestName == RequestType.Beacon.RequestName)
                        || (requestType.RequestName == RequestType.Status.RequestName)
                        || (requestType.RequestName == RequestType.NewSession.RequestName))
                    {
                        return httpResponse.Response == null || httpResponse.ResponseCode >= 400
                            ? StatusResponse.CreateErrorResponse(logger, httpResponse.ResponseCode, httpResponse.Headers)
                            : ParseStatusResponse(httpResponse);
                    }

                    logger.Warn(GetType().Name + " Unknown request type " + requestType + " - ignoring response");
                    return UnknownErrorResponse(requestType);
                }
                catch (Exception)
                {
                    retry++;
                    if (retry > MaxSendRetries)
                    {
                        throw;
                    }

                    threadSuspender.Sleep(RetrySleepTime);
                }
            }
        }

        private IStatusResponse ParseStatusResponse(HttpResponse httpResponse)
        {
            try
            {
                var responseAttributes = ResponseParser.ParseResponse(httpResponse.Response);
                return StatusResponse.CreateSuccessResponse(logger, responseAttributes, httpResponse.ResponseCode,
                    httpResponse.Headers);
            }
            catch (Exception e)
            {
                logger.Error(GetType().Name + " Failed to parse StatusResponse", e);
                return UnknownErrorResponse(RequestType.Status);
            }
        }

        protected abstract HttpResponse GetRequest(string url, string clientIpAddress);

        protected abstract HttpResponse PostRequest(string url, string clientIpAddress, byte[] gzippedPayload);

        // build URL used for status check and beacon send requests
        private static string BuildMonitorUrl(string baseUrl, string applicationId, int serverId)
        {
            var monitorUrlBuilder = new StringBuilder();

            monitorUrlBuilder.Append(baseUrl);
            monitorUrlBuilder.Append('?');
            monitorUrlBuilder.Append(RequestTypeMobile);

            AppendQueryParam(monitorUrlBuilder, QueryKeyServerId, serverId.ToInvariantString());
            AppendQueryParam(monitorUrlBuilder, QueryKeyApplication, applicationId);
            AppendQueryParam(monitorUrlBuilder, QueryKeyVersion, ProtocolConstants.OpenKitVersion);
            AppendQueryParam(monitorUrlBuilder, QueryKeyPlatformType, ProtocolConstants.PlatformTypeOpenKit.ToInvariantString());
            AppendQueryParam(monitorUrlBuilder, QueryKeyAgentTechnologyType, ProtocolConstants.AgentTechnologyType);
            AppendQueryParam(monitorUrlBuilder, QueryKeyResponseType, ProtocolConstants.ResponseType);

            return monitorUrlBuilder.ToString();
        }

        private string BuildNewSessionUrl(string baseUrl, string applicationId, int serverId)
        {
            var newSessionUrlBuilder = new StringBuilder(BuildMonitorUrl(baseUrl, applicationId, serverId));

            AppendQueryParam(newSessionUrlBuilder, QueryKeyNewSession, "1");

            return newSessionUrlBuilder.ToString();
        }

        private static string AppendAdditionalQueryParameters(string baseUrl, IAdditionalQueryParameters parameters)
        {
            if (parameters == null)
            {
                return baseUrl;
            }

            var builder = new StringBuilder(baseUrl);
            AppendQueryParam(builder, QueryKeyConfigTimestamp, parameters.ConfigurationTimestamp.ToString());

            return builder.ToString();
        }

        // helper method for appending query parameters
        private static void AppendQueryParam(StringBuilder urlBuilder, string key, string value)
        {
            urlBuilder.Append('&');
            urlBuilder.Append(key);
            urlBuilder.Append('=');
            urlBuilder.Append(PercentEncoder.Encode(value, Encoding.UTF8, QueryReservedCharacters));
        }

        private static byte[] CompressByteArray(byte[] raw)
        {
            using (var memory = new MemoryStream())
            {
                using (var gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }

                return memory.ToArray();
            }
        }

        private IStatusResponse UnknownErrorResponse(RequestType requestType)
        {
            if (requestType == null)
            {
                return null;
            }

            if (requestType.RequestName == RequestType.Status.RequestName
                || requestType.RequestName == RequestType.Beacon.RequestName
                || requestType.RequestName == RequestType.NewSession.RequestName)
            {
                return StatusResponse.CreateErrorResponse(logger, int.MaxValue);
            }

            // should not be reached
            return null;
        }
    }
}