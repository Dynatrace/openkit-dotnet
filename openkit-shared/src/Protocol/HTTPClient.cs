/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dynatrace.OpenKit.Protocol {

    /// <summary>
    ///  HTTP client helper which abstracts the 3 basic request types:
    ///  - status check
    ///  - beacon send
    ///  - time sync
    /// </summary>
    public class HTTPClient {

        public class RequestType {

            public static readonly RequestType STATUS = new RequestType("Status");              // status check
            public static readonly RequestType BEACON = new RequestType("Beacon");				// beacon send
            public static readonly RequestType TIMESYNC = new RequestType("TimeSync");          // time sync

            private string requestName;

            private RequestType(string requestName) {
                this.requestName = requestName;
            }

            public string getRequestName() {
                return requestName;
            }

        }

        // request type constants
        private const string REQUEST_TYPE_MOBILE = "type=m";
        private const string REQUEST_TYPE_TIMESYNC = "type=mts";

        // query parameter constants
        private const string QUERY_KEY_SERVER_ID = "srvid";
        private const string QUERY_KEY_APPLICATION = "app";
        private const string QUERY_KEY_VERSION = "va";

        // connection constants
        private const int MAX_SEND_RETRIES = 3;
        private const int RETRY_SLEEP_TIME = 200;       // retry sleep time in ms

        // URLs for requests
        private string monitorURL;
        private string timeSyncURL;

        private int serverID;
        private bool verbose;

        // *** constructors ***

        public HTTPClient(string baseURL, string applicationID, int serverID, bool verbose) {
            this.serverID = serverID;
            this.verbose = verbose;
            this.monitorURL = BuildMonitorURL(baseURL, applicationID, serverID);
            this.timeSyncURL = BuildTimeSyncURL(baseURL);
        }

        // *** public methods ***

        // sends a status check request and returns a status response
        public StatusResponse SendStatusRequest() {
            return (StatusResponse)SendRequest(RequestType.STATUS, monitorURL, null, null, "GET");
        }

        // sends a beacon send request and returns a status response
        public StatusResponse SendBeaconRequest(string clientIPAddress, byte[] data) {
            return (StatusResponse)SendRequest(RequestType.BEACON, monitorURL, clientIPAddress, data, "POST");
        }

        // sends a time sync request and returns a time sync response
        public TimeSyncResponse SendTimeSyncRequest() {
            return (TimeSyncResponse)SendRequest(RequestType.TIMESYNC, timeSyncURL, null, null, "GET");
        }

        // *** private methods ***

        // generic request send with some verbose output and exception handling
        protected Response SendRequest(RequestType requestType, string url, string clientIPAddress, byte[] data, string method) {
            try {
                if (verbose) {
                    Console.WriteLine("HTTP " + requestType.getRequestName() + " Request: " + url);
                }
                return SendRequestInternal(requestType, url, clientIPAddress, data, method);
            } catch (Exception e) {
                if (verbose) {
                    Console.WriteLine("ERROR: " + requestType.getRequestName() + " Request failed!");
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
            return null;
        }

        // generic internal request send
        private Response SendRequestInternal(RequestType requestType, string url, string clientIPAddress, byte[] data, string method) {
            int retry = 1;
            while (true) {
                try {
                    System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();

                    if (clientIPAddress != null) {
                        httpClient.DefaultRequestHeaders.Add("X-Client-IP", clientIPAddress);
                    }

                    // gzip beacon data, if available
                    ByteArrayContent content = new ByteArrayContent(new byte[] { });            // initialize with empty content, just to be sure
                    if ((data != null) && (data.Length > 0)) {
                        byte[] gzippedData = GZip(data);

                        if (verbose) {
                            Console.WriteLine("Beacon Payload: " + Encoding.UTF8.GetString(data));
                        }

                        content = new ByteArrayContent(gzippedData);
                        content.Headers.Add("Content-Encoding", "gzip");
                        content.Headers.Add("Content-Length", gzippedData.Length.ToString());
                    }

                    // sending HTTP request
                    Task<HttpResponseMessage> responseTask = null;
                    if (method.Equals("GET")) {
                        responseTask = httpClient.GetAsync(url);
                    } else if (method.Equals("POST")) {
                        responseTask = httpClient.PostAsync(url, content);
                    } else {
                        return null;
                    }
                    responseTask.Wait();

                    // reading HTTP response
                    HttpResponseMessage httpResponse = responseTask.Result;
                    Task<string> httpResponseContentTask = httpResponse.Content.ReadAsStringAsync();
                    httpResponseContentTask.Wait();
                    string response = httpResponseContentTask.Result;
                    HttpStatusCode responseCode = httpResponse.StatusCode;

                    if (verbose) {
                        Console.WriteLine("HTTP Response: " + response);
                        Console.WriteLine("HTTP Response Code: " + (int)responseCode);
                    }

                    // create typed response based on response content
                    if (response.StartsWith(REQUEST_TYPE_TIMESYNC)) {
                        return new TimeSyncResponse(response, (int)responseCode);
                    } else if (response.StartsWith(REQUEST_TYPE_MOBILE)) {
                        return new StatusResponse(response, (int)responseCode);
                    } else {
                        return null;
                    }
                } catch (Exception exception) {
                    retry++;
                    if (retry > MAX_SEND_RETRIES) {
                        throw exception;
                    }

                    Thread.Sleep(RETRY_SLEEP_TIME);
                }
            }
        }

        // build URL used for status check and beacon send requests
        private string BuildMonitorURL(string baseURL, string applicationID, int serverID) {
            StringBuilder monitorURLBuilder = new StringBuilder();

            monitorURLBuilder.Append(baseURL);
            monitorURLBuilder.Append('?');
            monitorURLBuilder.Append(REQUEST_TYPE_MOBILE);

            AppendQueryParam(monitorURLBuilder, QUERY_KEY_SERVER_ID, serverID.ToString());
            AppendQueryParam(monitorURLBuilder, QUERY_KEY_APPLICATION, applicationID);
            AppendQueryParam(monitorURLBuilder, QUERY_KEY_VERSION, Beacon.OPENKIT_VERSION);

            return monitorURLBuilder.ToString();
        }

        // build URL used for time sync requests
        private string BuildTimeSyncURL(string baseURL) {
            StringBuilder timeSyncURLBuilder = new StringBuilder();

            timeSyncURLBuilder.Append(baseURL);
            timeSyncURLBuilder.Append('?');
            timeSyncURLBuilder.Append(REQUEST_TYPE_TIMESYNC);

            return timeSyncURLBuilder.ToString();
        }

        // helper method for appending query parameters
        private void AppendQueryParam(StringBuilder urlBuilder, string key, string value) {
            urlBuilder.Append('&');
            urlBuilder.Append(key);
            urlBuilder.Append('=');
            urlBuilder.Append(System.Uri.EscapeDataString(value));
        }

        // helper method for gzipping beacon data
        private static byte[] GZip(byte[] data) {
            // gzip code taken from DotNetZip: http://dotnetzip.codeplex.com/
            return Ionic.Zlib.GZipStream.CompressBuffer(data);
        }

        // *** properties ***

        public int ServerID {
            get {
                return serverID;
            }
        }

    }

}
