using System;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// The state responsible for the time sync
    /// 
    /// In this state a time sync is performed.
    /// 
    /// Transitions to:
    /// <ul>
    ///     <li><code>BeaconSendingStateCaptureOnState</code> if IsCaptureOn is <code>true</code></li>
    ///     <li><code>BeaconSendingStateCaptureOffState</code> if IsCaptureOn is <code>false</code></li>
    ///     li><code>BeaconSendingTerminalState</code> on shutdown</li>
    /// </ul>
    /// 
    /// </summary>
    internal class BeaconSendingTimeSyncState : AbstractBeaconSendingState
    {
        public const int TIME_SYNC_REQUESTS = 5;
        public const int TIME_SYNC_RETRY_COUNT = 5;
        public const long INITIAL_RETRY_SLEEP_TIME_MILLISECONDS = 1000;

        private readonly bool initialTimeSync;

        public BeaconSendingTimeSyncState() : base(false) { }
        public BeaconSendingTimeSyncState(bool initialTimeSync) : this()
        {
            this.initialTimeSync = initialTimeSync;
        }

        protected override AbstractBeaconSendingState ShutdownState => new BeaconSendingTerminalState();

        /// <summary>
        /// Sets init completed to false
        /// </summary>
        /// <param name="context"></param>
        protected override void OnInterrupted(BeaconSendingContext context)
        {
            if (initialTimeSync)
            {
                context.InitCompleted(false);
            }
        }

        protected override void DoExecute(BeaconSendingContext context)
        {
            var timeSyncOffsets = ExecuteTimeSyncRequests(context);

            // time sync requests were NOT succesfull -> use 0 as cluster time offset
            if (timeSyncOffsets.Count < TIME_SYNC_REQUESTS)
            {
                // if this is the initial sync try, we have to initialize the time provider
                // in every other case we keep the previous setting
                if (initialTimeSync)
                {
                    // TODO thomas.grassauer@dynatrace.com - initialize timeprovider with 0
                }

                // if time sync failed, always go to capture off staet
                context.CurrentState = new BeaconSendingStateCaptureOffState();
                return;
            }

            var clusterTimeOffset = ComputeClusterTimeOffset(timeSyncOffsets);

            // state transition
            if(context.IsCaptureOn)
            {
                context.CurrentState = new BeaconSendingStateCaptureOnState();
            }
            else
            {
                context.CurrentState = new BeaconSendingStateCaptureOffState();
            }

            // set init complete if initial time sync
            if (initialTimeSync)
            {
                context.InitCompleted(true);
            }

            context.LastTimeSyncTime = context.CurrentTimestamp;
        }        

        private List<long> ExecuteTimeSyncRequests(BeaconSendingContext context)
        {
            var timeSyncOffsets = new List<long>(TIME_SYNC_REQUESTS);

            var retry = 0;
            var sleepTimeInMillis = INITIAL_RETRY_SLEEP_TIME_MILLISECONDS;

            // no check for shutdown here, time sync has to be completed
            while (timeSyncOffsets.Count < TIME_SYNC_REQUESTS && retry++ < TIME_SYNC_RETRY_COUNT && !context.IsShutdownRequested)
            {
                // doExecute time-sync request and take timestamps
                var requestSendTime = context.CurrentTimestamp;
                var timeSyncResponse = context.GetHTTPClient().SendTimeSyncRequest();
                var responseReceiveTime = context.CurrentTimestamp;

                if (timeSyncResponse != null)
                {
                    var requestReceiveTime = timeSyncResponse.RequestReceiveTime;
                    var responseSendTime = timeSyncResponse.ResponseSendTime;

                    // check both timestamps for being > 0
                    if ((requestReceiveTime > 0) && (responseSendTime > 0))
                    {
                        // if yes -> continue time-sync
                        var offset = ((requestReceiveTime - requestSendTime) + (responseSendTime - responseReceiveTime)) / 2;
                        timeSyncOffsets.Add(offset);
                        retry = 0; // on successful response reset the retry count & initial sleep time
                        sleepTimeInMillis = INITIAL_RETRY_SLEEP_TIME_MILLISECONDS;
                    }
                    else
                    {
                        // if no -> stop time sync, it's not supported
                        break;
                    }
                }
                else
                {
                    context.Sleep(sleepTimeInMillis);
                    sleepTimeInMillis *= 2;
                }
            }

            return timeSyncOffsets;
        }

        private long ComputeClusterTimeOffset(List<long> timeSyncOffsets)
        {
            // time sync requests were successful -> calculate cluster time offset
            timeSyncOffsets.Sort();

            // take median value from sorted offset list
            var median = timeSyncOffsets[TIME_SYNC_REQUESTS / 2];

            // calculate variance from median
            var medianVariance = 0L;
            for (var i = 0; i < TIME_SYNC_REQUESTS; i++)
            {
                var diff = timeSyncOffsets[i] - median;
                medianVariance += diff * diff;
            }
            medianVariance = medianVariance / TIME_SYNC_REQUESTS;

            // calculate cluster time offset as arithmetic mean of all offsets that are in range of 1x standard deviation
            var sum = 0L;
            var count = 0;
            for (var i = 0; i < TIME_SYNC_REQUESTS; i++)
            {
                var diff = timeSyncOffsets[i] - median;
                if (diff * diff <= medianVariance)
                {
                    sum += timeSyncOffsets[i];
                    count++;
                }
            }

            return (long)Math.Round(sum / (double)count);
        }
    }
}
