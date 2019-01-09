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
using Dynatrace.OpenKit.Providers;
using System;
using System.Threading;

namespace Dynatrace.OpenKit.Core.Caching
{
    /// <summary>
    /// Class responsible for handling an eviction thread, to ensure BeaconCache stays in configured boundaries.
    /// </summary>
    public class BeaconCacheEvictor
    {
        public static readonly int EVICTION_THREAD_JOIN_TIMEOUT = 2 * 1000; // 2seconds in milliseconds

        private readonly ILogger logger;
        private readonly IBeaconCache beaconCache;
        private readonly IBeaconCacheEvictionStrategy[] strategies;

#if WINDOWS_UWP || NETPCL4_5
        private readonly System.Threading.Tasks.Task evictionThread;
#else
        private readonly Thread evictionThread;
#endif

        private readonly object startStopLock = new object();
        private volatile bool isShutdownRequested = false;

        private readonly object syncObject = new object();


        /// <summary>
        /// Public constructor, initializing the eviction thread with the default <see cref="TimeEvictionStrategy"/>
        /// and <see cref="SpaceEvictionStrategy"/> strategies.
        /// </summary>
        /// <param name="logger">Logger to write some debug output</param>
        /// <param name="beaconCache">The Beacon cache to check if entries need to be evicted</param>
        /// <param name="configuration">Beacon cache configuration</param>
        /// <param name="timingProvider">Timing provider required for time retrieval</param>
        public BeaconCacheEvictor(ILogger logger, IBeaconCache beaconCache, BeaconCacheConfiguration configuration, ITimingProvider timingProvider) :
            this(logger, beaconCache)
        {
            strategies = new IBeaconCacheEvictionStrategy[]
            {
                new TimeEvictionStrategy(logger, beaconCache, configuration, timingProvider, () => IsShutdownRequested),
                new SpaceEvictionStrategy(logger, beaconCache, configuration, () => IsShutdownRequested)
            };
        }

        /// <summary>
        /// Internal testing constructor.
        /// </summary>
        /// <param name="logger">Logger to write some debug output</param>
        /// <param name="beaconCache">The Beacon cache to check if entries need to be evicted</param>
        /// <param name="strategies">Strategies executed in the thread</param>
        internal BeaconCacheEvictor(ILogger logger, IBeaconCache beaconCache, params IBeaconCacheEvictionStrategy[] strategies)
        {
            this.logger = logger;
            this.beaconCache = beaconCache;
            this.strategies = strategies;
#if WINDOWS_UWP || NETPCL4_5
            evictionThread = new System.Threading.Tasks.Task(RunEvictionThread);
#else
            evictionThread = new Thread(RunEvictionThread)
            {
                Name = this.GetType().Name,
                IsBackground = true
            };
#endif

        }

        /// <summary>
        /// Property giving <code>true</code> if eviction was started by calling <see cref="Start"/>,
        /// otherwise <code>false</code> is returned.
        /// </summary>
        public bool IsAlive =>
#if WINDOWS_UWP || NETPCL4_5
            !evictionThread.IsCompleted && evictionThread.Status != System.Threading.Tasks.TaskStatus.Created;
#else
            evictionThread.IsAlive;
#endif

        /// <summary>
        /// Get or set a flag indicating whether a shutdown is requested.
        /// </summary>
        private bool IsShutdownRequested
        {
            get { return isShutdownRequested; }
            set { isShutdownRequested = value; }
        }

        /// <summary>
        /// Starts the eviction thread.
        /// </summary>
        /// <returns>
        /// <code>true</code> if the eviction thread was started, <code>false</code> if the thread was already running.
        /// </returns>
        public bool Start()
        {
            lock(startStopLock)
            {
                return DoStart();
            }
        }

        /// <summary>
        /// Internal start method.
        /// </summary>
        /// <returns>
        /// <code>true</code> if the eviction thread was started, <code>false</code> if the thread was already running.
        /// </returns>
        private bool DoStart()
        {
            var result = false;

            if (!IsAlive)
            {
                evictionThread.Start();
                result = true;
            }
            else
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug(GetType().Name + "Not starting BeaconCacheEviction thread, since it's already running");
                }
            }

            return result;
        }

        /// <summary>
        /// Stops the eviction thread if the thread is alive and joins the thread.
        /// </summary>
        /// <remarks>
        /// This method calls <see cref="Stop(int)"/> with default join timeout <see cref="BeaconCacheEvictor.EVICTION_THREAD_JOIN_TIMEOUT"/>.
        /// </remarks>
        /// <returns>
        /// <code>true</code> if stopping was successful,
        /// <code>false</code> if eviction thread is not running or could not be stopped in time.
        /// </returns>
        public bool Stop()
        {
            return Stop(EVICTION_THREAD_JOIN_TIMEOUT);
        }

        /// <summary>
        /// Stops the eviction thread if the thread is alive and joins the thread with given timeout in milliseconds.
        /// </summary>
        /// <returns>
        /// <code>true</code> if stopping was successful,
        /// <code>false</code> if eviction thread is not running or could not be stopped in time.
        /// </returns>
        public bool Stop(int joinTimeout)
        {
            lock(startStopLock)
            {
                return DoStop(joinTimeout);
            }
        }

        /// <summary>
        /// Internal stop method.
        /// </summary>
        /// <returns>
        /// <code>true</code> if stopping was successful,
        /// <code>false</code> if eviction thread is not running or could not be stopped in time.
        /// </returns>
        private bool DoStop(int joinTimeout)
        {
            var success = false;

            if (IsAlive)
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug(GetType().Name + "Stopping BeaconCacheEviction thread.");
                }
                IsShutdownRequested = true;
                lock(syncObject)
                {
                    Monitor.PulseAll(syncObject);
                }
#if WINDOWS_UWP || NETPCL4_5
                evictionThread.Wait(joinTimeout);
#else
                evictionThread.Join(joinTimeout);
#endif
                success = !IsAlive;
            }
            else
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug(GetType().Name + "Not stopping BeaconCacheEviction thread, since it's not alive");
                }
            }

            return success;
        }

        /// <summary>
        /// Method called by the eviction thread.
        /// </summary>
        private void RunEvictionThread()
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(GetType().Name + " thread started");
            }

            var recordAdded = false;

            beaconCache.RecordAdded += (sender, e) =>
            {
                lock(syncObject)
                {
                    recordAdded = true;
                    Monitor.PulseAll(syncObject);
                }
            };

            while (!IsShutdownRequested)
            {
                lock(syncObject)
                {
                    while (!IsShutdownRequested && !recordAdded)
                    {
                        Monitor.Wait(syncObject);
                    }
                    // reset the added flag
                    recordAdded = false;
                }

                if (IsShutdownRequested)
                {
                    // shutdown request was made
                    break;
                }

                // a new record has been added to the cache
                // run all eviction strategies, to perform cache cleanup
                foreach (var strategy in strategies)
                {
                    strategy.Execute();
                }
            }

            if (logger.IsDebugEnabled)
            {
                logger.Debug(GetType().Name + " thread is stopped");
            }
        }
    }
}
