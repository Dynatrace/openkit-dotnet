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
using Dynatrace.OpenKit.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;

namespace Dynatrace.OpenKit.Protocol
{

    /// <summary>
    ///  HTTP client helper which abstracts the 2 basic request types:
    ///  - status check
    ///  - beacon send
    /// </summary>
    public abstract class HTTPClient : IHTTPClient
    {

        public class RequestType
        {

            public static readonly RequestType Status = new RequestType("Status");              // status check
            public static readonly RequestType Beacon = new RequestType("Beacon");              // beacon send
            public static readonly RequestType NewSession = new RequestType("NewSession");     // new session

            public string RequestName { get; }

            private RequestType(string requestName)
            {
                RequestName = requestName;
            }
        }

        public class HTTPResponse
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

        // additional reserved characters for URL encoding
        private static readonly char[] QueryReservedCharacters = { '_' };

        // connection constants
        internal const int MaxSendRetries = 3;
        private const int RetrySleepTime = 200;       // retry sleep time in ms

        // URLs for requests
        private readonly string monitorURL;

        private readonly int serverID;
        private readonly ILogger logger;

        #region constructors

        public HTTPClient(ILogger logger, HTTPClientConfiguration configuration)
        {
            this.logger = logger;
            serverID = configuration.ServerID;
            monitorURL = BuildMonitorURL(configuration.BaseURL, configuration.ApplicationID, configuration.ServerID);
        }

        #endregion

        // sends a status check request and returns a status response
        public StatusResponse SendStatusRequest()
        {
            return (StatusResponse)SendRequest(RequestType.Status, monitorURL, null, null, "GET") ?? new StatusResponse(logger, string.Empty, int.MaxValue, new Dictionary<string, List<string>>());
        }

        // sends a beacon send request and returns a status response
        public StatusResponse SendBeaconRequest(string clientIPAddress, byte[] data)
        {
            return (StatusResponse)SendRequest(RequestType.Beacon, monitorURL, clientIPAddress, data, "POST") ?? new StatusResponse(logger, string.Empty, int.MaxValue, new Dictionary<string, List<string>>());
        }

        public StatusResponse SendNewSessionRequest()
        {
            return (StatusResponse)SendRequest(RequestType.NewSession, monitorURL, null, null, "GET") ?? new StatusResponse(logger, string.Empty, int.MaxValue, new Dictionary<string, List<string>>());
        }

        // generic request send with some verbose output and exception handling
        protected Response SendRequest(RequestType requestType, string url, string clientIPAddress, byte[] data, string method)
        {
            try
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug(GetType().Name + " HTTP " + requestType.RequestName + " Request: " + url);
                }
                return SendRequestInternal(requestType, url, clientIPAddress, data, method);
            }
            catch (Exception e)
            {
                logger.Error(GetType().Name + " " + requestType.RequestName + " Request failed!", e);
            }
            return UnknownErrorResponse(requestType);
        }

        // generic internal request send
        protected Response SendRequestInternal(RequestType requestType, string url, string clientIPAddress, byte[] data, string method)
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
                            logger.Debug(GetType().Name + " Beacon Payload: " + Encoding.UTF8.GetString(data, 0, data.Length));
                        }
                    }

                    HTTPResponse httpResponse = null;
                    if (method == "GET")
                    {
                        httpResponse = GetRequest(url, clientIPAddress);
                    }
                    else if (method == "POST")
                    {
                        httpResponse = PostRequest(url, clientIPAddress, gzippedData);
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
                            ? new StatusResponse(logger, string.Empty, httpResponse.ResponseCode, httpResponse.Headers)
                            : ParseStatusResponse(httpResponse);
                    }
                    else
                    {
                        logger.Warn(GetType().Name + " Unknown request type " + requestType + " - ignoring response");
                        return UnknownErrorResponse(requestType);
                    }
                }
                catch (Exception exception)
                {
                    retry++;
                    if (retry > MaxSendRetries)
                    {
                        throw exception;
                    }
#if WINDOWS_UWP || NETSTANDARD1_1
                    System.Threading.Tasks.Task.Delay(RetrySleepTime).Wait();
#else
                    Thread.Sleep(RetrySleepTime);
#endif
                }
            }
        }

        private Response ParseStatusResponse(HTTPResponse httpResponse)
        {
            if (IsStatusResponse(httpResponse))
            {
                try
                {
                    return new StatusResponse(logger, httpResponse.Response, httpResponse.ResponseCode, httpResponse.Headers);
                }
                catch (Exception e)
                {
                    logger.Error(GetType().Name + " Failed to parse StatusResponse", e);
                    return UnknownErrorResponse(RequestType.Status);
                }
            }

            // invalid/unexpected response
            logger.Warn(GetType().Name + " The HTTPResponse \"" + httpResponse.Response + "\" is not a valid status response");
            return UnknownErrorResponse(RequestType.Status);
        }

        private static bool IsStatusResponse(HTTPResponse httpResponse)
        {
            return httpResponse.Response == RequestTypeMobile
                || httpResponse.Response.StartsWith($"{RequestTypeMobile}&");
        }

        protected abstract HTTPResponse GetRequest(string url, string clientIPAddress);

        protected abstract HTTPResponse PostRequest(string url, string clientIPAddress, byte[] gzippedPayload);

        // build URL used for status check and beacon send requests
        private string BuildMonitorURL(string baseURL, string applicationID, int serverID)
        {
            StringBuilder monitorURLBuilder = new StringBuilder();

            monitorURLBuilder.Append(baseURL);
            monitorURLBuilder.Append('?');
            monitorURLBuilder.Append(RequestTypeMobile);

            AppendQueryParam(monitorURLBuilder, QueryKeyServerId, serverID.ToString());
            AppendQueryParam(monitorURLBuilder, QueryKeyApplication, applicationID);
            AppendQueryParam(monitorURLBuilder, QueryKeyVersion, ProtocolConstants.OpenKitVersion);
            AppendQueryParam(monitorURLBuilder, QueryKeyPlatformType, Convert.ToString(ProtocolConstants.PlatformTypeOpenKit));
            AppendQueryParam(monitorURLBuilder, QueryKeyAgentTechnologyType, ProtocolConstants.AgentTechnologyType);

            return monitorURLBuilder.ToString();
        }

        // helper method for appending query parameters
        private void AppendQueryParam(StringBuilder urlBuilder, string key, string value)
        {
            urlBuilder.Append('&');
            urlBuilder.Append(key);
            urlBuilder.Append('=');
            urlBuilder.Append(PercentEncoder.Encode(value, Encoding.UTF8, QueryReservedCharacters));
        }

        private byte[] CompressByteArray(byte[] raw)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory,
                    CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }

        private Response UnknownErrorResponse(RequestType requestType)
        {
            if (requestType == null)
            {
                return null;
            }
            else if ((requestType.RequestName == RequestType.Status.RequestName)
                || (requestType.RequestName == RequestType.Beacon.RequestName)
                || (requestType.RequestName == RequestType.NewSession.RequestName))
            {
                return new StatusResponse(logger, string.Empty, int.MaxValue, new Dictionary<string, List<string>>());
            }
            else
            {
                // should not be reached
                return null;
            }
        }
    }
}
