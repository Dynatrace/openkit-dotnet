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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Core.Util;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// State context for beacon sending
    /// </summary>
    internal class BeaconSendingContext : IBeaconSendingContext
    {
        /// <summary>
        /// Default sleep time in milliseconds (used by <see cref="Sleep()"/>).
        /// </summary>
        public const int DefaultSleepTimeMilliseconds = 1000;

        private readonly ILogger logger;

        /// <summary>
        /// synchronization object for updating and reading server configuration and last response attributes
        /// </summary>
        private readonly object lockObject = new object();

        /// <summary>
        /// Configuration for storing the last valid server side configuration.
        /// </summary>
        private IServerConfiguration serverConfiguration;

        /// <summary>
        /// Represents the last attributes received as response from the server.
        /// <para>
        /// This field will be initially filled with the first response from the server when OpenKit initializes.
        /// Subsequent server responses (e.g. from session requests) will update the last server response by merging
        /// received fields.
        /// </para>
        /// </summary>
        private IResponseAttributes lastResponseAttributes;

        /// <summary>
        /// Configuration storing the last valid HTTP client configuration, independent of a session.
        /// </summary>
        private IHttpClientConfiguration httpClientConfiguration;

        /// <summary>
        /// Provider for timing information.
        /// </summary>
        private readonly ITimingProvider timingProvider;
        /// <summary>
        /// instance for suspending the thread for a certain time span.
        /// </summary>
        private readonly IInterruptibleThreadSuspender threadSuspender;

        // container storing all sessions
        private readonly SynchronizedQueue<ISessionInternals> sessions = new SynchronizedQueue<ISessionInternals>();

        // reset event is set when init was done - which can either be success or failure
        private readonly ManualResetEvent resetEvent = new ManualResetEvent(false);

        // boolean indicating whether shutdown was requested or not (accessed by multiple threads)
        private volatile bool isShutdownRequested;

        // boolean indicating whether init was successful or not (accessed by multiple threads)
        private volatile bool initSucceeded;

        /// <summary>
        /// Constructor
        ///
        /// Current state is initialized to <see cref="Dynatrace.OpenKit.Core.Communication.BeaconSendingInitState"/>.
        /// </summary>
        internal BeaconSendingContext(
            ILogger logger,
            IHttpClientConfiguration httpClientConfiguration,
            IHttpClientProvider httpClientProvider,
            ITimingProvider timingProvider,
            IInterruptibleThreadSuspender threadSuspender
        )
            : this(logger, httpClientConfiguration, httpClientProvider, timingProvider, threadSuspender,
                new BeaconSendingInitState())
        {
        }

        internal BeaconSendingContext(
            ILogger logger,
            IHttpClientConfiguration httpClientConfiguration,
            IHttpClientProvider httpClientProvider,
            ITimingProvider timingProvider,
            IInterruptibleThreadSuspender threadSuspender,
            AbstractBeaconSendingState initialState
            )
        {
            this.logger = logger;
            this.httpClientConfiguration = httpClientConfiguration;
            serverConfiguration = ServerConfiguration.Default;
            HttpClientProvider = httpClientProvider;
            this.timingProvider = timingProvider;
            this.threadSuspender = threadSuspender;
            lastResponseAttributes = ResponseAttributes.WithUndefinedDefaults().Build();

            CurrentState = initialState;
        }

        public IHttpClientProvider HttpClientProvider { get; }
        public AbstractBeaconSendingState CurrentState { get; internal set; }
        public AbstractBeaconSendingState NextState { get; set; }
        public long LastOpenSessionBeaconSendTime { get; set; }
        public long LastStatusCheckTime { get; set; }

        public bool IsInitialized => initSucceeded;

        public bool IsShutdownRequested
        {
            get => isShutdownRequested;
            private set => isShutdownRequested = value;
        }

        public long CurrentTimestamp => timingProvider.ProvideTimestampInMilliseconds();
        public int SendInterval
        {
            get
            {
                lock (lockObject)
                {
                    return lastResponseAttributes.SendIntervalInMilliseconds;
                }
            }
        }

        public bool IsCaptureOn
        {
            get
            {
                lock (lockObject)
                {
                    return serverConfiguration.IsCaptureEnabled;
                }
            }
        }

        public bool IsInTerminalState => CurrentState.IsTerminalState;

        public void ExecuteCurrentState()
        {
            NextState = null;
            CurrentState.Execute(this);
            if (NextState != null && ! CurrentState.IsTerminalState) // CurrentState.Execute(...) can trigger state changes
            {
                if(logger.IsInfoEnabled)
                {
                    logger.Info($"BeaconSendingContext State change from '{CurrentState}' to '{NextState}'");
                }
                CurrentState = NextState;
                NextState = null;
            }
        }

        public void RequestShutdown()
        {
            IsShutdownRequested = true;
            threadSuspender.WakeUp();
        }

        public bool WaitForInit()
        {
            resetEvent.WaitOne();
            return initSucceeded;
        }

        public bool WaitForInit(int timeoutMillis)
        {
            resetEvent.WaitOne(TimeSpan.FromMilliseconds(timeoutMillis));
            return initSucceeded;
        }

        public void InitCompleted(bool success)
        {
            initSucceeded = success;
            resetEvent.Set();
        }

        public IHttpClient GetHttpClient()
        {
            return HttpClientProvider.CreateClient(httpClientConfiguration);
        }

        public void Sleep()
        {
            Sleep(DefaultSleepTimeMilliseconds);
        }

        public void Sleep(int millis)
        {
            threadSuspender.Sleep(millis);
        }

        public void DisableCaptureAndClear()
        {
            DisableCapture();
            ClearAllSessionData();
        }

        private void DisableCapture()
        {
            lock (lockObject)
            {
                serverConfiguration = new ServerConfiguration.Builder(serverConfiguration)
                    .WithCapture(false)
                    .Build();
            }
        }

        public void HandleStatusResponse(IStatusResponse statusResponse)
        {
            if (statusResponse == null || statusResponse.ResponseCode != StatusResponse.HttpOk)
            {
                DisableCaptureAndClear();
                return;
            }

            UpdateFrom(statusResponse);

            if (!IsCaptureOn)
            {
                // capture was turned off
                ClearAllSessionData();
            }
        }

        public IResponseAttributes UpdateFrom(IStatusResponse statusResponse)
        {
            lock (lockObject)
            {
                if (!BeaconSendingResponseUtil.IsSuccessfulResponse(statusResponse))
                {
                    return lastResponseAttributes;
                }

                lastResponseAttributes = lastResponseAttributes.Merge(statusResponse.ResponseAttributes);

                var builder = new ServerConfiguration.Builder(lastResponseAttributes);
                if (IsApplicationIdMismatch(lastResponseAttributes))
                {
                    builder.WithCapture(false);
                }
                serverConfiguration = builder.Build();

                var serverId = serverConfiguration.ServerId;
                if (serverId != httpClientConfiguration.ServerId)
                {
                    httpClientConfiguration = CreateHttpClientConfigurationWith(serverId);
                }

                return lastResponseAttributes;
            }
        }

        /// <summary>
        /// Ensure that the application id coming with the response matches the one that was configured for OpenKit.
        /// </summary>
        /// <remarks>
        /// Mismatch check prevents a rare Jetty bug, where responses might be dispatched to the wrong receiver.
        /// </remarks>
        /// <param name="lastResponseAttributes">The last response attributes received from Dynatrace/AppMon.</param>
        /// <returns><code>false</code> if application id is matching, <code>true</code> if a mismatch occurred.</returns>
        internal bool IsApplicationIdMismatch(IResponseAttributes lastResponseAttributes)
        {
            if (lastResponseAttributes.IsAttributeSet(ResponseAttribute.APPLICATION_ID))
            {
                return httpClientConfiguration.ApplicationId != lastResponseAttributes.ApplicationId;
            }

            // if it's not set it's either the old response format, or an older Dynatrace version
            // in this case no mismatch is happening and everything is fine
            return false;
        }

        public IResponseAttributes LastResponseAttributes
        {
            get
            {
                lock (lockObject)
                {
                    return lastResponseAttributes;
                }
            }
        }

        public IServerConfiguration LastServerConfiguration
        {
            get
            {
                lock (lockObject)
                {
                    return serverConfiguration;
                }
            }
        }

        internal virtual IHttpClientConfiguration CreateHttpClientConfigurationWith(int serverId)
        {
            return HttpClientConfiguration.ModifyWith(httpClientConfiguration).WithServerId(serverId).Build();
        }

        /// <summary>
        /// Clear captured data from all sessions.
        /// </summary>
        private void ClearAllSessionData()
        {
            foreach (var session in sessions.ToList())
            {
                session.ClearCapturedData();
                if (session.State.IsFinished)
                {
                    sessions.Remove(session);
                }
            }
        }

        public List<ISessionInternals> GetAllNotConfiguredSessions()
        {
            return sessions.ToList().Where(session => !session.State.IsConfigured).ToList();
        }

        public List<ISessionInternals> GetAllOpenAndConfiguredSessions()
        {
            return sessions.ToList().Where(session => session.State.IsConfiguredAndOpen).ToList();
        }

        public List<ISessionInternals> GetAllFinishedAndConfiguredSessions()
        {
            return sessions.ToList().Where(session => session.State.IsConfiguredAndFinished).ToList();
        }

        /// <summary>
        /// Returns the number of sessions currently known to this context.
        /// </summary>
        public int SessionCount => sessions.Count;

        /// <summary>
        /// Returns the current server ID to e used for creating new sessions.
        /// </summary>
        public int CurrentServerId => httpClientConfiguration.ServerId;


        /// <summary>
        /// Adds the given session to the internal container of sessions.
        /// </summary>
        /// <param name="session">the session to add.</param>
        public void AddSession(ISessionInternals session)
        {
            sessions.Put(session);
        }

        /// <summary>
        /// Removes the given session from the sessions known by this context.
        /// </summary>
        /// <param name="session">the session to be removed.</param>
        public bool RemoveSession(ISessionInternals session)
        {
            return sessions.Remove(session);
        }

        #region IAdditionalQueryParameters implementation

        public long ConfigurationTimestamp
        {
            get
            {
                lock (lockObject)
                {
                    return lastResponseAttributes.TimestampInMilliseconds;
                }
            }
        }

        #endregion
    }
}
