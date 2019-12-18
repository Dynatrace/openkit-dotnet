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
        /// Configuration for storing the last valid server side configuration.
        ///
        /// This field is initialized in the constructor and must only be modified within the context of the
        /// <see cref="BeaconSender">beacon sending thread</see>.
        /// </summary>
        private IServerConfiguration serverConfiguration;

        /// <summary>
        /// Configuration storing the last valid HTTP client configuration, independent of a session.
        ///
        /// This field is initialized in the constructor and must only be modified within the context of the
        /// <see cref="BeaconSender">beacon sending thread</see>
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
            LastResponseAttributes = ResponseAttributes.WithUndefinedDefaults().Build();

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
        public int SendInterval => LastResponseAttributes.SendIntervalInMilliseconds;
        public bool IsCaptureOn => serverConfiguration.IsCaptureEnabled;
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
            serverConfiguration = new ServerConfiguration.Builder(serverConfiguration)
                .WithCapture(false)
                .Build();
        }

        public void HandleStatusResponse(IStatusResponse statusResponse)
        {
            if (statusResponse == null || statusResponse.ResponseCode != StatusResponse.HttpOk)
            {
                DisableCaptureAndClear();
                return;
            }

            var updatedAttributes = UpdateLastResponseAttributesFrom(statusResponse);
            serverConfiguration = new ServerConfiguration.Builder(updatedAttributes).Build();

            if (!IsCaptureOn)
            {
                // capture was turned off
                ClearAllSessionData();
            }

            var serverId = serverConfiguration.ServerId;
            if (serverId != httpClientConfiguration.ServerId)
            {
                httpClientConfiguration = CreateHttpClientConfigurationWith(serverId);
            }
        }

        public IResponseAttributes UpdateLastResponseAttributesFrom(IStatusResponse statusResponse)
        {
            if (BeaconSendingResponseUtil.IsSuccessfulResponse(statusResponse))
            {
                LastResponseAttributes = LastResponseAttributes.Merge(statusResponse.ResponseAttributes);
            }

            return LastResponseAttributes;
        }

        /// <summary>
        /// This property will be subsequently updated from every successful server response (e.g. from session
        /// requests) by merging the received attributes.
        /// <para>
        /// Modification of this field must only happen within the context of the BeaconSending thread.
        /// </para>
        /// </summary>
        public IResponseAttributes LastResponseAttributes { get; private set; }

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
    }
}
