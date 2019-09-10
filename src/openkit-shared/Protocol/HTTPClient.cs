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
    ///  HTTP client helper which abstracts the 3 basic request types:
    ///  - status check
    ///  - beacon send
    ///  - time sync
    /// </summary>
    public abstract class HTTPClient : IHTTPClient
    {

        public class RequestType
        {

            public static readonly RequestType Status = new RequestType("Status");              // status check
            public static readonly RequestType Beacon = new RequestType("Beacon");              // beacon send
            public static readonly RequestType TimeSync = new RequestType("TimeSync");          // time sync
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
        internal const string RequestTypeTimeSync = "type=mts";

        // query parameter constants
        internal const string QueryKeyServerId = "srvid";
        internal const string QueryKeyApplication = "app";
        internal const string QueryKeyVersion = "va";
        internal const string QueryKeyPlatformType = "pt";
        internal const string QueryKeyAgentTechnologyType = "tt";
        internal const string QueryKeyNewSession = "ns";

        // additional reserved characters for URL encoding
        private static readonly char[] QueryReservedCharacters = { '_' };

        // connection constants
        internal const int MaxSendRetries = 3;
        private const int RetrySleepTime = 200;       // retry sleep time in ms

        // URLs for requests
        private readonly string monitorURL;
        private readonly string newSessionUrl;
        private readonly string timeSyncURL;

        private readonly int serverID;
        private readonly ILogger logger;

        // *** constructors ***

        public HTTPClient(ILogger logger, HTTPClientConfiguration configuration)
        {
            this.logger = logger;
            serverID = configuration.ServerID;
            monitorURL = BuildMonitorURL(configuration.BaseURL, configuration.ApplicationID, configuration.ServerID);
            newSessionUrl = BuildNewSessionUrl(configuration.BaseURL, configuration.ApplicationID, configuration.ServerID);
            timeSyncURL = BuildTimeSyncURL(configuration.BaseURL);
        }

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

        // sends a time sync request and returns a time sync response
        public TimeSyncResponse SendTimeSyncRequest()
        {
            return (TimeSyncResponse)SendRequest(RequestType.TimeSync, timeSyncURL, null, null, "GET") ?? new TimeSyncResponse(logger, string.Empty, int.MaxValue, new Dictionary<string, List<string>>());
        }

        public StatusResponse SendNewSessionRequest()
        {
            return (StatusResponse)SendRequest(RequestType.NewSession, newSessionUrl, null, null, "GET") ?? new StatusResponse(logger, string.Empty, int.MaxValue, new Dictionary<string, List<string>>());
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
                    if (requestType.RequestName == RequestType.TimeSync.RequestName)
                    {
                        return httpResponse.Response == null || httpResponse.ResponseCode >= 400
                            ? new TimeSyncResponse(logger, string.Empty, httpResponse.ResponseCode, httpResponse.Headers)
                            : ParseTimeSyncResponse(httpResponse);
                    }
                    else if ((requestType.RequestName == RequestType.Beacon.RequestName)
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
#if WINDOWS_UWP || NETPCL4_5
                    System.Threading.Tasks.Task.Delay(RetrySleepTime).Wait();
#else
                    Thread.Sleep(RetrySleepTime);
#endif
                }
            }
        }

        private Response ParseStatusResponse(HTTPResponse httpResponse)
        {
            if (IsStatusResponse(httpResponse) && !IsTimeSyncResponse(httpResponse))
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

        private Response ParseTimeSyncResponse(HTTPResponse httpResponse)
        {
            if (IsTimeSyncResponse(httpResponse))
            {
                try
                {
                    return new TimeSyncResponse(logger, httpResponse.Response, httpResponse.ResponseCode, httpResponse.Headers);
                }
                catch(Exception e)
                {
                    logger.Error(GetType().Name + " Failed to parse TimeSyncResponse", e);
                    return UnknownErrorResponse(RequestType.TimeSync);
                }
            }

            // invalid/unexpected response
            logger.Warn(GetType().Name + " The HTTPResponse \"" + httpResponse.Response + "\" is not a valid time sync response");
            return UnknownErrorResponse(RequestType.TimeSync);
        }

        private static bool IsStatusResponse(HTTPResponse httpResponse)
        {
            return httpResponse.Response.StartsWith(RequestTypeMobile);
        }

        private static bool IsTimeSyncResponse(HTTPResponse httpResponse)
        {
            return httpResponse.Response.StartsWith(RequestTypeTimeSync);
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

        private string BuildNewSessionUrl(string baseUrl, string applicationId, int serverId)
        {
            var newSessionUrlBuilder = new StringBuilder(BuildMonitorURL(baseUrl, applicationId, serverId));

            AppendQueryParam(newSessionUrlBuilder, QueryKeyNewSession, "1");

            return newSessionUrlBuilder.ToString();
        }

        // build URL used for time sync requests
        private string BuildTimeSyncURL(string baseURL)
        {
            StringBuilder timeSyncURLBuilder = new StringBuilder();

            timeSyncURLBuilder.Append(baseURL);
            timeSyncURLBuilder.Append('?');
            timeSyncURLBuilder.Append(RequestTypeTimeSync);

            return timeSyncURLBuilder.ToString();
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
            else if ((requestType.RequestName == RequestType.TimeSync.RequestName))
            {
                return new TimeSyncResponse(logger, string.Empty, int.MaxValue, new Dictionary<string, List<string>>());
            }
            else
            {
                // should not be reached
                return null;
            }
        }
    }
}
