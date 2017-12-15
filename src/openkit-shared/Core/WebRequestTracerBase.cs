/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core {

    /// <summary>
    ///  Abstract base class implementation of the IWebRequestTracer interface.
    /// </summary>
    public abstract class WebRequestTracerBase : IWebRequestTracer {

        // Dynatrace tag that has to be used for tracing the web request
        private string tag = null;

        // HTTP information: URL & response code, bytesSent, bytesReceived
        protected string url = "<unknown>";
        private int responseCode = -1;
        private int bytesSent = -1;
        private int bytesReceived = -1;

        // start/end time & sequence number
        private long startTime = -1;
        private long endTime = -1;
        private int startSequenceNo = -1;
        private int endSequenceNo = -1;

        // Beacon and Action references
        private Beacon beacon;
        private Action action;

        // *** constructors ***

        public WebRequestTracerBase(Beacon beacon, Action action)
        {
            this.beacon = beacon;
            this.action = action;

            // creating start sequence number has to be done here, because it's needed for the creation of the tag
            startSequenceNo = beacon.NextSequenceNumber;

            tag = beacon.CreateTag(action, startSequenceNo);
        }

        // *** IWebRequestTracer interface methods ***

        public string URL
        {
            get
            {
                return url;
            }
        }

        public long StartTime
        {
            get
            {
                return startTime;
            }
        }

        public long EndTime
        {
            get
            {
                return endTime;
            }
        }

        public int StartSequenceNo
        {
            get
            {
                return startSequenceNo;
            }
        }

        public int EndSequenceNo
        {
            get
            {
                return endSequenceNo;
            }
        }

        public string Tag
        {
            get {
                return tag;
            }
        }

        public int ResponseCode
        {
            get
            {
                return responseCode;
            }
        }

        public int BytesSent
        {
            get {
                return bytesSent;
            }
        }

        public int BytesReceived
        {
            get
            {
                return bytesReceived;
            }
        }
   
        public IWebRequestTracer Start()
        {
            startTime = beacon.CurrentTimestamp;
            return this;
        }

        public void Stop()
        {
            endTime = beacon.CurrentTimestamp;
            endSequenceNo = beacon.NextSequenceNumber;

            // add web request to beacon
            beacon.AddWebRequest(action, this);
        }

        public IWebRequestTracer SetResponseCode(int responseCode)
        {
            this.responseCode = responseCode;
            return this;
        }

        public IWebRequestTracer SetBytesSent(int bytesSent)
        {
            this.bytesSent = bytesSent;
            return this;
        }

        public IWebRequestTracer SetBytesReceived(int bytesReceived)
        {
            this.bytesReceived = bytesReceived;
            return this;
        }
    }

}
