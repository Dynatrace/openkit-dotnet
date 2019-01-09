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

using Dynatrace.OpenKit.Protocol;
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
    ///     <li><see cref="BeaconSendingCaptureOnState"/> if IsCaptureOn is <code>true</code></li>
    ///     <li><see cref="BeaconSendingCaptureOffState"/> if IsCaptureOn is <code>false</code> or time sync failed</li>
    ///     <li><see cref="BeaconSendingFlushSessionsState"/> on shutdown if not initial time sync</li>
    ///     <li><see cref="BeaconSendingTerminalState"/> on shutdown if initial time sync</li>
    /// </ul>
    /// </summary>
    internal class BeaconSendingTimeSyncState : AbstractBeaconSendingState
    {
        public const int TIME_SYNC_REQUESTS = 5;
        public const int TIME_SYNC_RETRY_COUNT = 5;
        public const int INITIAL_RETRY_SLEEP_TIME_MILLISECONDS = 1000;
        public const int TIME_SYNC_INTERVAL_IN_MILLIS = 2 * 60 * 60 * 1000; // 2 h

        internal bool IsInitialTimeSync { get; private set; }

        public BeaconSendingTimeSyncState() : this(false) { }
        public BeaconSendingTimeSyncState(bool initialTimeSync) : base(false)
        {
            IsInitialTimeSync = initialTimeSync;
        }

        internal override AbstractBeaconSendingState ShutdownState
        {
            get
            {
                if (IsInitialTimeSync)
                {
                    return new BeaconSendingTerminalState();
                }
                return new BeaconSendingFlushSessionsState();
            }
        }

        /// <summary>
        /// Sets init completed to false
        /// </summary>
        /// <param name="context"></param>
        internal override void OnInterrupted(IBeaconSendingContext context)
        {
            if (IsInitialTimeSync)
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
            var response = ExecuteTimeSyncRequests(context);

            HandleTimeSyncResponses(context, response);

            // set init complete if initial time sync
            if (IsInitialTimeSync)
            {
                context.InitCompleted(true);
            }
        }

        private void HandleTimeSyncResponses(IBeaconSendingContext context, TimeSyncRequestsResponse response)
        {
            // time sync requests were *not* successful
            // -OR-
            // time sync requests are not supported by the server (e.g. AppMon)
            // -> use 0 as cluster time offset
            if (response.timeSyncOffsets.Count < TIME_SYNC_REQUESTS)
            {
                HandleErroneousTimeSyncRequest(context, response.response);
                return;
            }

            //sanity check to catch case with div/0
            var calculatedOffset = ComputeClusterTimeOffset(response.timeSyncOffsets);
            if (calculatedOffset < 0)
            {
                return;
            }

            // initialize time provider with cluster time offset
            context.InitializeTimeSync(calculatedOffset, true);

            // update the last sync time
            context.LastTimeSyncTime = context.CurrentTimestamp;

            // perform transition to next state
            SetNextState(context);
        }

        private void HandleErroneousTimeSyncRequest(IBeaconSendingContext context, TimeSyncResponse response)
        {
            // if this is the initial sync try, we have to initialize the time provider
            // in every other case we keep the previous setting
            if (IsInitialTimeSync)
            {
                context.InitializeTimeSync(0, context.IsTimeSyncSupported);
            }

            if (BeaconSendingResponseUtil.IsTooManyRequestsResponse(response))
            {
                // server is currently overloaded, change to CaptureOff state temporarily
                context.NextState = new BeaconSendingCaptureOffState(response.GetRetryAfterInMilliseconds());
            }
            else if (context.IsTimeSyncSupported)
            {
                // in case of time sync failure when it's supported, go to capture off state
                context.NextState = new BeaconSendingCaptureOffState();
            }
            else
            {
                // otherwise set the next state based on the configuration
                SetNextState(context);
            }
        }

        private TimeSyncRequestsResponse ExecuteTimeSyncRequests(IBeaconSendingContext context)
        {
            var response = new TimeSyncRequestsResponse();

            var retry = 0;
            var sleepTimeInMillis = INITIAL_RETRY_SLEEP_TIME_MILLISECONDS;

            // no check for shutdown here, time sync has to be completed
            while (response.timeSyncOffsets.Count < TIME_SYNC_REQUESTS && !context.IsShutdownRequested)
            {
                // doExecute time-sync request and take timestamps
                var requestSendTime = context.CurrentTimestamp;
                var timeSyncResponse = context.GetHTTPClient().SendTimeSyncRequest();
                var responseReceiveTime = context.CurrentTimestamp;

                if (BeaconSendingResponseUtil.IsSuccessfulResponse(timeSyncResponse))
                {
                    var requestReceiveTime = timeSyncResponse.RequestReceiveTime;
                    var responseSendTime = timeSyncResponse.ResponseSendTime;

                    // check both timestamps for being > 0
                    if ((requestReceiveTime > 0) && (responseSendTime > 0))
                    {
                        // if yes -> continue time-sync
                        var offset = ((requestReceiveTime - requestSendTime) + (responseSendTime - responseReceiveTime)) / 2;
                        response.timeSyncOffsets.Add(offset);
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
                else if (BeaconSendingResponseUtil.IsTooManyRequestsResponse(timeSyncResponse))
                {
                    // special handling for too many requests
                    // clear all time sync offsets captured so far and store the response, which is handled later
                    response.timeSyncOffsets.Clear();
                    response.response = timeSyncResponse;
                    break;
                }
                else
                {
                    context.Sleep(sleepTimeInMillis);
                    sleepTimeInMillis *= 2;
                    retry++;
                }
            }

            return response;
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

            if (count == 0)
            { // shouldn't come here under normal circumstances
                return -1; // prevents div/0
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
                context.NextState = new BeaconSendingCaptureOnState();
            }
            else
            {
                context.NextState = new BeaconSendingCaptureOffState();
            }
        }

        public override string ToString()
        {
            return "TimeSync";
        }


        /// <summary>
        /// Container class storing data for processing requests.
        /// </summary>
        private sealed class TimeSyncRequestsResponse
        {

            /// <summary>
            /// List storing time sync offsets.
            /// </summary>
            internal readonly List<long> timeSyncOffsets = new List<long>(TIME_SYNC_REQUESTS);

            /// <summary>
            /// List storing time sync response.
            /// </summary>
            /// <remarks>
            /// This might be required for e.g.handling 429 response error.
            /// </remarks>
            internal TimeSyncResponse response = null;
        }
    }
}
