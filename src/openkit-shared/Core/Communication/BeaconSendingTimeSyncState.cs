using Dynatrace.OpenKit.Providers;
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
    /// </summary>
    internal class BeaconSendingTimeSyncState : AbstractBeaconSendingState
    {
        public const int TIME_SYNC_REQUESTS = 5;
        public const int TIME_SYNC_RETRY_COUNT = 5;
        public const int INITIAL_RETRY_SLEEP_TIME_MILLISECONDS = 1000;
        public const int TIME_SYNC_INTERVAL_IN_MILLIS = 2 * 60 * 60 * 1000;

        private readonly bool initialTimeSync;

        public BeaconSendingTimeSyncState() : base(false) { }
        public BeaconSendingTimeSyncState(bool initialTimeSync) : this()
        {
            this.initialTimeSync = initialTimeSync;
        }

        internal override AbstractBeaconSendingState ShutdownState => new BeaconSendingTerminalState();

        /// <summary>
        /// Sets init completed to false
        /// </summary>
        /// <param name="context"></param>
        internal override void OnInterrupted(IBeaconSendingContext context)
        {
            if (initialTimeSync)
            {
                context.InitCompleted(false);
            }
        }

        protected override void DoExecute(IBeaconSendingContext context)
        {
            if (!IsTimeSyncRequired(context))
            {
                // make state transition based on configuration - since time sync does not need to be supported or might not be required
                SetNextState(context);
                return;
            }

            // execute time sync requests - note during initial sync it might be possible
            // that the time sync capability is disabled.
            var timeSyncOffsets = ExecuteTimeSyncRequests(context);

            HandleTimeSyncResponses(context, timeSyncOffsets);

            // set init complete if initial time sync
            if (initialTimeSync)
            {
                context.InitCompleted(true);
            }
        }

        private void HandleTimeSyncResponses(IBeaconSendingContext context, List<long> timeSyncOffsets)
        {
            // time sync requests were *not* successful
            // -OR-
            // time sync requests are not supported by the server (e.g. AppMon)
            // -> use 0 as cluster time offset
            if (timeSyncOffsets.Count < TIME_SYNC_REQUESTS)
            {
                HandleErroneousTimeSyncRequest(context);
                return;
            }

            // initialize time provider with cluster time offset
            TimeProvider.Initialize(ComputeClusterTimeOffset(timeSyncOffsets), true);

            // update the last sync time
            context.LastTimeSyncTime = context.CurrentTimestamp;

            // perform transition to next state
            SetNextState(context);
        }

        private void HandleErroneousTimeSyncRequest(IBeaconSendingContext context)
        {
            // if this is the initial sync try, we have to initialize the time provider
            // in every other case we keep the previous setting
            if (initialTimeSync) 
            {
                TimeProvider.Initialize(0, false);
            }

            if (context.IsTimeSyncSupported)
            {
                // in case of time sync failure when it's supported, go to capture off state
                context.CurrentState = new BeaconSendingCaptureOffState();
            }
            else
            {
                // otherwise set the next state based on the configuration
                SetNextState(context);
            }
        }

        private List<long> ExecuteTimeSyncRequests(IBeaconSendingContext context)
        {
            var timeSyncOffsets = new List<long>(TIME_SYNC_REQUESTS);

            var retry = 0;
            var sleepTimeInMillis = INITIAL_RETRY_SLEEP_TIME_MILLISECONDS;

            // no check for shutdown here, time sync has to be completed
            while (timeSyncOffsets.Count < TIME_SYNC_REQUESTS && !context.IsShutdownRequested)
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
                        context.DisableTimeSyncSupport();
                        break;
                    }
                }
                else if (retry >= TIME_SYNC_RETRY_COUNT)
                {
                    // retry limit reached
                    break;
                }
                else
                {
                    context.Sleep(sleepTimeInMillis);
                    sleepTimeInMillis *= 2;
                    retry++;
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

        /// <summary>
        /// Helper method to check if a time sync is required
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsTimeSyncRequired(IBeaconSendingContext context)
        {
            if (!context.IsTimeSyncSupported)
            {
                // time sync not supported by server, therefore not required
                return false;
            }

            return ((context.LastTimeSyncTime < 0) 
                || (context.CurrentTimestamp - context.LastTimeSyncTime > TIME_SYNC_INTERVAL_IN_MILLIS));
        }

        private static void SetNextState(IBeaconSendingContext context)
        {
            // state transition
            if (context.IsCaptureOn)
            {
                context.CurrentState = new BeaconSendingCaptureOnState();
            }
            else
            {
                context.CurrentState = new BeaconSendingCaptureOffState();
            }
        }
    }
}
