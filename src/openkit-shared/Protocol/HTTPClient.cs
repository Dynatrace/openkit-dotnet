/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using System;
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

            public static readonly RequestType STATUS = new RequestType("Status");              // status check
            public static readonly RequestType BEACON = new RequestType("Beacon");				// beacon send
            public static readonly RequestType TIMESYNC = new RequestType("TimeSync");          // time sync

            private string requestName;

            private RequestType(string requestName)
            {
                this.requestName = requestName;
            }

            public string getRequestName()
            {
                return requestName;
            }

        }

        public class HTTPResponse
        {
            public string Response { get; set; }
            public int ResponseCode { get; set; }
        }
        
        // request type constants
        private const string REQUEST_TYPE_MOBILE = "type=m";
        private const string REQUEST_TYPE_TIMESYNC = "type=mts";

        // query parameter constants
        private const string QUERY_KEY_SERVER_ID = "srvid";
        private const string QUERY_KEY_APPLICATION = "app";
        private const string QUERY_KEY_VERSION = "va";
        private const string QUERY_KEY_PLATFORM_TYPE = "pt";

        // constant query parameter values
        private const string PLATFORM_TYPE_OPENKIT = "1";

        // connection constants
        private const int MAX_SEND_RETRIES = 3;
        private const int RETRY_SLEEP_TIME = 200;       // retry sleep time in ms

        // URLs for requests
        private readonly string monitorURL;
        private readonly string timeSyncURL;

        private readonly int serverID;
        private readonly ILogger logger;

        // *** constructors ***

        public HTTPClient(ILogger logger, HTTPClientConfiguration configuration)
        {
            this.logger = logger;
            serverID = configuration.ServerID;
            monitorURL = BuildMonitorURL(configuration.BaseURL, configuration.ApplicationID, configuration.ServerID);
            timeSyncURL = BuildTimeSyncURL(configuration.BaseURL);
        }

        // *** public methods ***

        // sends a status check request and returns a status response
        public StatusResponse SendStatusRequest()
        {
            return (StatusResponse)SendRequest(RequestType.STATUS, monitorURL, null, null, "GET");
        }

        // sends a beacon send request and returns a status response
        public StatusResponse SendBeaconRequest(string clientIPAddress, byte[] data)
        {
            return (StatusResponse)SendRequest(RequestType.BEACON, monitorURL, clientIPAddress, data, "POST");
        }

        // sends a time sync request and returns a time sync response
        public TimeSyncResponse SendTimeSyncRequest()
        {
            return (TimeSyncResponse)SendRequest(RequestType.TIMESYNC, timeSyncURL, null, null, "GET");
        }

        // *** private methods ***

        // generic request send with some verbose output and exception handling
        protected Response SendRequest(RequestType requestType, string url, string clientIPAddress, byte[] data, string method)
        {
            try
            {
                if (logger.IsDebugEnabled())
                {
                    logger.Debug("HTTP " + requestType.getRequestName() + " Request: " + url);
                }
                return SendRequestInternal(url, clientIPAddress, data, method);
            }
            catch (Exception e)
            {
                logger.Error(requestType.getRequestName() + " Request failed!", e);
            }
            return null;
        }

        // generic internal request send
        protected Response SendRequestInternal(string url, string clientIPAddress, byte[] data, string method)
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
                        gzippedData = GZip(data);

                        if (logger.IsDebugEnabled())
                        {
                           logger.Debug("Beacon Payload: " + Encoding.UTF8.GetString(data));
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
                        return null;
                    }

                    if (logger.IsDebugEnabled())
                    {
                        logger.Debug("HTTP Response: " + httpResponse.Response);
                        logger.Debug("HTTP Response Code: " + httpResponse.ResponseCode);
                    }

                    if (httpResponse.ResponseCode >= 400)
                    {
                        // an error occurred -> return null
                        return null;
                    }

                    // create typed response based on response content
                    if (httpResponse.Response.StartsWith(REQUEST_TYPE_TIMESYNC))
                    {
                        return new TimeSyncResponse(httpResponse.Response, httpResponse.ResponseCode);
                    }
                    else if (httpResponse.Response.StartsWith(REQUEST_TYPE_MOBILE))
                    {
                        return new StatusResponse(httpResponse.Response, httpResponse.ResponseCode);
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception exception)
                {
                    retry++;
                    if (retry > MAX_SEND_RETRIES)
                    {
                        throw exception;
                    }

                    Thread.Sleep(RETRY_SLEEP_TIME);
                }
            }
        }

        protected abstract HTTPResponse GetRequest(string url, string clientIPAddress);

        protected abstract HTTPResponse PostRequest(string url, string clientIPAddress, byte[] gzippedPayload);

        // build URL used for status check and beacon send requests
        private string BuildMonitorURL(string baseURL, string applicationID, int serverID)
        {
            StringBuilder monitorURLBuilder = new StringBuilder();

            monitorURLBuilder.Append(baseURL);
            monitorURLBuilder.Append('?');
            monitorURLBuilder.Append(REQUEST_TYPE_MOBILE);

            AppendQueryParam(monitorURLBuilder, QUERY_KEY_SERVER_ID, serverID.ToString());
            AppendQueryParam(monitorURLBuilder, QUERY_KEY_APPLICATION, applicationID);
            AppendQueryParam(monitorURLBuilder, QUERY_KEY_VERSION, Beacon.OPENKIT_VERSION);
            AppendQueryParam(monitorURLBuilder, QUERY_KEY_PLATFORM_TYPE, PLATFORM_TYPE_OPENKIT);

            return monitorURLBuilder.ToString();
        }

        // build URL used for time sync requests
        private string BuildTimeSyncURL(string baseURL)
        {
            StringBuilder timeSyncURLBuilder = new StringBuilder();

            timeSyncURLBuilder.Append(baseURL);
            timeSyncURLBuilder.Append('?');
            timeSyncURLBuilder.Append(REQUEST_TYPE_TIMESYNC);

            return timeSyncURLBuilder.ToString();
        }

        // helper method for appending query parameters
        private void AppendQueryParam(StringBuilder urlBuilder, string key, string value)
        {
            urlBuilder.Append('&');
            urlBuilder.Append(key);
            urlBuilder.Append('=');
            urlBuilder.Append(System.Uri.EscapeDataString(value));
        }

        // helper method for gzipping beacon data
        private static byte[] GZip(byte[] data)
        {
            // gzip code taken from DotNetZip: http://dotnetzip.codeplex.com/
            return Ionic.Zlib.GZipStream.CompressBuffer(data);
        }
    }
}
