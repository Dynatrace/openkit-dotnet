/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.Protocol;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Dynatrace.OpenKit.Core {

    /// <summary>
    ///  The BeaconSender is responsible for asynchronously sending the Beacons to the provided endpoint.
    /// </summary>
    class BeaconSender {

        private static readonly int MAX_INITIAL_STATUS_REQUEST_RETRIES = 5;        // execute max 5 status request retries for getting initial settings
        private static readonly int TIME_SYNC_REQUESTS = 5;                        // execute 5 time sync requests for time sync calculation
        private static readonly int STATUS_CHECK_INTERVAL = 2 * 60 * 60 * 1000;    // wait 2h (in ms) for next status request
        private static readonly int SHUTDOWN_TIMEOUT = 10 * 1000;                  // wait max 10s (in ms) for beacon sender to complete data sending during shutdown

        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // data structures for managing open and finished Sessions
        private SynchronizedQueue<Session> openSessions = new SynchronizedQueue<Session>();
        private SynchronizedQueue<Session> finishedSessions = new SynchronizedQueue<Session>();

        // Configuration reference
        private Configuration configuration;

        // beacon sender thread
        private Thread beaconSenderThread;

        // indicates that shutdown() was called and beacon sender thread should be ended
        private bool shutdown;

	    // timestamps for last beacon send and status check
	    private long lastOpenSessionBeaconSendTime;
        private long lastStatusCheckTime;

        // *** constructors ***

        public BeaconSender(Configuration configuration) {
            this.configuration = configuration;
            shutdown = false;
        }

        // *** Runnable interface methods ***

        public void Run() {
            // loop until shutdown() is called
            while (!shutdown) {
                // sleep 1s
                Sleep();

                StatusResponse statusResponse = null;
                // check capture mode
                if (configuration.Capture) {
                    statusResponse = null;

                    // check if there's finished Sessions to be sent -> immediately send beacon(s) of finished Sessions
                    while (!finishedSessions.IsEmpty()) {
                        Session session = finishedSessions.Get();
                        statusResponse = session.SendBeacon();
                    }

                    // check if send interval spent -> send current beacon(s) of open Sessions
                    if (CurrentTimeMillis() > lastOpenSessionBeaconSendTime + configuration.SendInterval) {
                        foreach (Session session in openSessions.ToList()) {
                            statusResponse = session.SendBeacon();
                        }

                        // update beacon send timestamp in this case
                        lastOpenSessionBeaconSendTime = CurrentTimeMillis();
                    }

                    // if at least one beacon was sent AND a (valid) status response was received -> update settings
                    if (statusResponse != null) {
                        configuration.UpdateSettings(statusResponse);
                    }
                } else {
                    // check if status check interval spent -> send status request & update settings
                    if (CurrentTimeMillis() > lastStatusCheckTime + STATUS_CHECK_INTERVAL) {
                        statusResponse = configuration.CurrentHTTPClient.SendStatusRequest();

                        // if a (valid) status response was received -> update settings
                        if (statusResponse != null) {
                            configuration.UpdateSettings(statusResponse);
                        }

                        // update status check timestamp in any case
                        lastStatusCheckTime = CurrentTimeMillis();
                    }
                }
            }

            // during shutdown, end all open Sessions
            while (!openSessions.IsEmpty()) {
                Session session = openSessions.Get();
                session.End();
            }

            // and finally send all the (now) finished Sessions
            while (!finishedSessions.IsEmpty()) {
                Session session = finishedSessions.Get();
                session.SendBeacon();
            }
        }

        // *** public methods ***

        // called indirectly by OpenKit.initialize(); tries to get initial settings and starts beacon sender thread
        public void Initialize() {
            lastOpenSessionBeaconSendTime = CurrentTimeMillis();
            lastStatusCheckTime = lastOpenSessionBeaconSendTime;

            // try status check until max retries were executed or shutdown() is called, but at least once!
            StatusResponse statusResponse = null;
            int retry = 0;
            do {
                retry++;
                statusResponse = configuration.CurrentHTTPClient.SendStatusRequest();

                // if no (valid) status response was received -> sleep 1s and then retry (max 5 times altogether)
                if (statusResponse == null) {
                    Sleep();
                }
            } while (!shutdown && (statusResponse == null) && (retry < MAX_INITIAL_STATUS_REQUEST_RETRIES));

            // update settings based on (valid) status response
            // if status response is null, updateSettings() will turn capture off, as there were no initial settings received
            configuration.UpdateSettings(statusResponse);

            if (configuration.IsDynatrace) {
                // initialize time provider with cluster time offset -> time sync
                TimeProvider.Initialize(CalculateClusterTimeOffset(), true);
            } else {
                // initialize time provider -> no time sync
                TimeProvider.Initialize(0, false);
            }

            // start beacon sender thread
            beaconSenderThread = new Thread(new ThreadStart(Run));
            beaconSenderThread.Start();
        }

        // when starting a new Session, put it into open Sessions
        public void StartSession(Session session) {
            openSessions.Put(session);
        }

        // when finishing a Session, remove it from open Sessions and put it into finished Sessions
        public void FinishSession(Session session) {
            openSessions.Remove(session);
            finishedSessions.Put(session);
        }

        // clear all open and finished Sessions
        public void ClearSessions() {
            openSessions.Clear();
            finishedSessions.Clear();
        }

        // shutdown beacon sender thread, with a timeout
        public void Shutdown() {
            shutdown = true;
            if (beaconSenderThread != null) {
                beaconSenderThread.Interrupt();
                beaconSenderThread.Join(SHUTDOWN_TIMEOUT);
            }
        }

        // *** private methods ***

        // helper method for getting local timestamp (and not use TimeProvider)
        private long CurrentTimeMillis() {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        // helper method for sleeping 1s
        private void Sleep() {
            try {
                Thread.Sleep(1000);
            } catch (ThreadInterruptedException) {
                // when interrupted (most probably by shutdown()), just wake up and continue execution
            }
        }

        // calculate the cluster time offset by doing time sync with the Dynatrace cluster
        private long CalculateClusterTimeOffset() {
            List<long> timeSyncOffsets = new List<long>(TIME_SYNC_REQUESTS);
            // no check for shutdown here, time sync has to be completed
            while (timeSyncOffsets.Count < TIME_SYNC_REQUESTS) {
                // execute time-sync request and take timestamps
                long requestSendTime = TimeProvider.GetTimestamp();
                TimeSyncResponse timeSyncResponse = configuration.CurrentHTTPClient.SendTimeSyncRequest();
                long responseReceiveTime = TimeProvider.GetTimestamp();

                if (timeSyncResponse != null) {
                    long requestReceiveTime = timeSyncResponse.RequestReceiveTime;
                    long responseSendTime = timeSyncResponse.ResponseSendTime;

                    // check both timestamps for being > 0
                    if ((requestReceiveTime > 0) && (responseSendTime > 0)) {
                        // if yes -> continue time-sync
                        long offset = (long)(((requestReceiveTime - requestSendTime) +
                                (responseSendTime - responseReceiveTime)) / 2.0);

                        timeSyncOffsets.Add(offset);
                    } else {
                        // if no -> stop time-sync
                        break;
                    }
                } else {
                    Sleep();
                }
            }

            // time sync requests were *not* successful -> use 0 as cluster time offset
            if (timeSyncOffsets.Count < TIME_SYNC_REQUESTS) {
                return 0;
            }

            // time sync requests were successful -> calculate cluster time offset
            timeSyncOffsets.Sort();

            // take median value from sorted offset list
            long median = timeSyncOffsets[TIME_SYNC_REQUESTS / 2];

            // calculate variance from median
            long medianVariance = 0;
            for (int i = 0; i < TIME_SYNC_REQUESTS; i++) {
                long diff = timeSyncOffsets[i] - median;
                medianVariance += diff * diff;
            }
            medianVariance = medianVariance / TIME_SYNC_REQUESTS;

            // calculate cluster time offset as arithmetic mean of all offsets that are in range of 1x standard deviation
            long sum = 0;
            long count = 0;
            for (int i = 0; i < TIME_SYNC_REQUESTS; i++) {
                long diff = timeSyncOffsets[i] - median;
                if (diff * diff <= medianVariance) {
                    sum += timeSyncOffsets[i];
                    count++;
                }
            }

            return (long)Math.Round(sum / (double)count);       // return cluster time offset
        }

    }

}
