//
// Copyright 2018-2020 Dynatrace LLC
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
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;
using Dynatrace.OpenKit.Util;

namespace Dynatrace.OpenKit.Core.Objects
{
    public class SessionProxy : OpenKitComposite, ISessionProxy
    {
        /// <summary>
        /// object used for synchronization
        /// </summary>
        private readonly object lockObject = new object();

        /// <summary>
        /// log message reporter
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// parent object of this session proxy.
        /// </summary>
        private readonly IOpenKitComposite parent;

        /// <summary>
        /// creator for new sessions
        /// </summary>
        private readonly ISessionCreator sessionCreator;

        /// <summary>
        /// provider to obtain the current time
        /// </summary>
        private readonly ITimingProvider timingProvider;

        /// <summary>
        /// sender of beacon data
        /// </summary>
        private readonly IBeaconSender beaconSender;

        /// <summary>
        /// watchdog to split sessions after idle/max timeout or to close split off session after a grace period
        /// </summary>
        private readonly ISessionWatchdog sessionWatchdog;

        /// <summary>
        /// the current session instance
        /// </summary>
        private ISessionInternals currentSession;

        /// <summary>
        /// holds the number of received calls <see cref="EnterAction"/>
        /// </summary>
        private int topLevelActionCount;

        /// <summary>
        /// specifies the timestamp when the last top level event happened
        /// </summary>
        private long lastInteractionTime;

        /// <summary>
        /// the server configuration of the first session (will be initialized when the first session is updated with
        /// the server configuration)
        /// </summary>
        private IServerConfiguration serverConfiguration;

        /// <summary>
        /// Indicator if this session proxy is already finished or not.
        /// </summary>
        private bool isFinished;

        /// <summary>
        /// last user tag reported via <see cref="IdentifyUser"/>
        /// </summary>
        private string lastUserTag = null;

        internal SessionProxy(
            ILogger logger,
            IOpenKitComposite parent,
            ISessionCreator sessionCreator,
            ITimingProvider timingProvider,
            IBeaconSender beaconSender,
            ISessionWatchdog sessionWatchdog)
        {
            this.logger = logger;
            this.parent = parent;
            this.sessionCreator = sessionCreator;
            this.timingProvider = timingProvider;
            this.beaconSender = beaconSender;
            this.sessionWatchdog = sessionWatchdog;

            var currentServerConfig = beaconSender.LastServerConfiguration;
            currentSession = CreateInitialSession(currentServerConfig);
        }

        /// <summary>
        /// Accessor for simplified access to <see cref="IOpenKitComposite"/>
        /// </summary>
        private IOpenKitComposite ThisComposite => this;

        #region ISession Implementation

        public IRootAction EnterAction(string actionName)
        {
            if (string.IsNullOrEmpty(actionName))
            {
                logger.Warn($"{this} EnterAction: actionName must not be null or empty");
                return NullRootAction.Instance;
            }

            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{this} EnterAction({actionName})");
            }

            lock (lockObject)
            {
                if (isFinished)
                {
                    return NullRootAction.Instance;
                }

                var session = GetOrSplitCurrentSessionByEvents();
                if (session.Beacon.IsActionReportingAllowedByPrivacySettings)
                {
                    // avoid session splitting by action count, if user opted out of action collection
                    RecordTopLevelActionEvent();
                }
                else
                {
                    RecordTopLevelEventInteraction();
                }
                return session.EnterAction(actionName);
            }
        }

        public void IdentifyUser(string userTag)
        {
            if (string.IsNullOrEmpty(userTag))
            {
                logger.Warn($"{this} IdentifyUser: userTag must not be null or empty");
                return;
            }

            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{this} IdentifyUser({userTag})");
            }

            lock (lockObject)
            {
                if (isFinished)
                {
                    return;
                }

                var session = GetOrSplitCurrentSessionByEvents();
                RecordTopLevelEventInteraction();
                session.IdentifyUser(userTag);
                lastUserTag = userTag;
            }
        }

        public void ReportCrash(string errorName, string reason, string stacktrace)
        {
            if (string.IsNullOrEmpty(errorName))
            {
                logger.Warn($"{this} ReportCrash: errorName must not be null or empty");
                return;
            }

            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{this} ReportCrash({errorName}, {reason}, {stacktrace})");
            }

            lock (lockObject)
            {
                if (isFinished)
                {
                    return;
                }

                var session = GetOrSplitCurrentSessionByEvents();
                RecordTopLevelEventInteraction();
                session.ReportCrash(errorName, reason, stacktrace);

                // create new session after crash report
                SplitAndCreateNewInitialSession();
            }
        }

        public IWebRequestTracer TraceWebRequest(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                logger.Warn($"{this} TraceWebRequest(string): url must not be null or empty");
                return NullWebRequestTracer.Instance;
            }

            if (!WebRequestTracer.IsValidUrlScheme(url))
            {
                logger.Warn($"{this} TraceWebRequest(string): url \"{url}\" does not have a valid scheme");
                return NullWebRequestTracer.Instance;
            }

            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{this} TraceWebRequest({url})");
            }

            lock (lockObject)
            {
                if (isFinished)
                {
                    return NullWebRequestTracer.Instance;
                }

                var session = GetOrSplitCurrentSessionByEvents();
                RecordTopLevelEventInteraction();
                return session.TraceWebRequest(url);
            }
        }

        public void End()
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{this} End()");
            }

            lock (lockObject)
            {
                if (isFinished)
                {
                    return;
                }

                isFinished = true;
            }

            var childObjects = ThisComposite.GetCopyOfChildObjects();
            foreach (var childObject in childObjects)
            {
                childObject.Dispose();
            }

            parent.OnChildClosed(this);
            sessionWatchdog.RemoveFromSplitByTimeout(this);
        }

        #endregion

        /// <summary>
        /// Indicates whether the session proxy was finished or is still open.
        /// </summary>
        public bool IsFinished
        {
            get
            {
                lock (lockObject)
                {
                    return isFinished;
                }
            }
        }

        #region OpenKitComposite overrides

        public override void Dispose()
        {
            End();
        }

        private protected override void OnChildClosed(IOpenKitObject childObject)
        {
            lock (lockObject)
            {
                ThisComposite.RemoveChildFromList(childObject);
                if (childObject is ISessionInternals session)
                {
                    sessionWatchdog.DequeueFromClosing(session);
                }
            }
        }

        #endregion

        /// <summary>
        /// Returns the number of top level actions which were made to the current session. Intended to be used by unit
        /// tests only.
        /// </summary>
        internal int TopLevelActionCount
        {
            get
            {
                lock (lockObject)
                {
                    return topLevelActionCount;
                }
            }
        }

        /// <summary>
        /// Returns the time when the last top level event was called. Intended to be used by unit tests only.
        /// </summary>
        internal long LastInteractionTime
        {
            get
            {
                lock (lockObject)
                {
                    return lastInteractionTime;
                }
            }
        }

        /// <summary>
        /// Returns the server configuration of this session proxy. Intended to be used by unit tests only.
        /// </summary>
        internal IServerConfiguration ServerConfiguration => serverConfiguration;

        private ISessionInternals GetOrSplitCurrentSessionByEvents()
        {
            if (IsSessionSplitByEventsRequired())
            {
                CloseOrEnqueueCurrentSessionForClosing();
                currentSession = CreateSplitSession(serverConfiguration);

                ReTagCurrentSession();
            }

            return currentSession;
        }

        /// <summary>
        /// Checks if the maximum number of top level actions is reached and session splitting by events needs to be
        /// performed.
        /// </summary>
        private bool IsSessionSplitByEventsRequired()
        {
            if (serverConfiguration == null || !serverConfiguration.IsSessionSplitByEventsEnabled)
            {
                return false;
            }

            return serverConfiguration.MaxEventsPerSession <= topLevelActionCount;
        }

        public long SplitSessionByTime()
        {
            lock (lockObject)
            {
                if (IsFinished)
                {
                    return -1;
                }

                var nextSplitTime = CalculateNextSplitTime();
                var now = timingProvider.ProvideTimestampInMilliseconds();
                if (nextSplitTime < 0 || now < nextSplitTime)
                {
                    return nextSplitTime;
                }

                SplitAndCreateNewInitialSession();

                return CalculateNextSplitTime();
            }
        }

        /// <summary>
        /// Will end the current active session, enque the old one for closing, and create a new session.
        /// 
        /// <para>
        /// The new session is created using the <see cref="CreateInitialSession(IServerConfiguration)"/> method.
        /// </para>
        /// 
        /// <para>
        /// This method must be called only when the <see cref="lockObject"/> is held.
        /// </para>
        /// </summary>
        private void SplitAndCreateNewInitialSession()
        {
            CloseOrEnqueueCurrentSessionForClosing();

            // create a completely new Session
            sessionCreator.Reset();
            currentSession = CreateInitialSession(serverConfiguration);

            ReTagCurrentSession();
        }

        private void CloseOrEnqueueCurrentSessionForClosing()
        {
            // for grace period use half of the idle timeout
            // or fallback to session interval if not configured
            var closeGracePeriodInMillis = serverConfiguration.SessionTimeoutInMilliseconds > 0
                ? serverConfiguration.SessionTimeoutInMilliseconds / 2
                : serverConfiguration.SendIntervalInMilliseconds;

            sessionWatchdog.CloseOrEnqueueForClosing(currentSession, closeGracePeriodInMillis);
        }

        /// <summary>
        /// Calculates and returns the next point in time when the session is to be split. The returned time might
        /// either be,
        /// <list type="bullet">
        /// <item>the time when the session expires after the max. session duration elapsed.</item>
        /// <item>the time when the session expires after being idle.</item>
        /// </list>
        /// , depending on what happens earlier.
        /// </summary>
        private long CalculateNextSplitTime()
        {
            if (serverConfiguration == null)
            {
                return -1;
            }

            var splitByIdleTimeout = serverConfiguration.IsSessionSplitByIdleTimeoutEnabled;
            var splitBySessionDuration = serverConfiguration.IsSessionSplitBySessionDurationEnabled;

            var idleTimeout = lastInteractionTime + serverConfiguration.SessionTimeoutInMilliseconds;
            var sessionMaxTime = currentSession.Beacon.SessionStartTime +
                                 serverConfiguration.MaxSessionDurationInMilliseconds;

            if (splitByIdleTimeout && splitBySessionDuration)
            {
                return Math.Min(idleTimeout, sessionMaxTime);
            }
            else if (splitByIdleTimeout)
            {
                return idleTimeout;
            }
            else if (splitBySessionDuration)
            {
                return sessionMaxTime;
            }

            return -1;
        }

        private ISessionInternals CreateInitialSession(IServerConfiguration initialServerConfig)
        {
            return CreateSession(initialServerConfig, null);
        }

        private ISessionInternals CreateSplitSession(IServerConfiguration updatedServerConfig)
        {
            return CreateSession(null, updatedServerConfig);
        }

        /// <summary>
        /// Creates a new session and adds it to the beacon sender. The top level action count is reset to zero and the
        /// last interaction time is set to the current timestamp.
        /// <para>
        /// In case the given <code>initialServerConfig</code> is not null, the new session will be initialized with
        /// this server configuration. The created session however will not be in state
        /// <see cref="ISessionState.IsConfigured">configured</see>, meaning new session requests will be performed for
        /// this session.
        /// </para>
        /// <para>
        /// In case the given <code>updatedServerConfig</code> is not null, the new session will be updated with this
        /// server configuration. The created session will be in state
        /// <see cref="ISessionState.IsConfigured">configured</see>, meaning new session requests will be omitted.
        /// </para>
        /// </summary>
        /// <param name="initialServerConfig">
        /// the server configuration with which the session will be initialized. Can be <code>null</code>
        /// </param>
        /// <param name="updatedServerConfig">
        /// the server configuration with which the session will be updated. Can be <code>null</code>.
        /// </param>
        /// <returns>the newly created session.</returns>
        private ISessionInternals CreateSession(IServerConfiguration initialServerConfig,
            IServerConfiguration updatedServerConfig)
        {
            var session = sessionCreator.CreateSession(this);
            var beacon = session.Beacon;
            beacon.OnServerConfigurationUpdate += OnServerConfigurationUpdate;

            ThisComposite.StoreChildInList(session);

            lastInteractionTime = beacon.SessionStartTime;
            topLevelActionCount = 0;

            if (initialServerConfig != null)
            {
                session.InitializeServerConfiguration(initialServerConfig);
            }

            if (updatedServerConfig != null)
            {
                session.UpdateServerConfiguration(updatedServerConfig);
            }

            beaconSender.AddSession(session);

            return session;
        }

        private void RecordTopLevelEventInteraction()
        {
            lastInteractionTime = timingProvider.ProvideTimestampInMilliseconds();
        }

        private void RecordTopLevelActionEvent()
        {
            topLevelActionCount++;
            RecordTopLevelEventInteraction();
        }

        private void ReTagCurrentSession()
        {
            if (lastUserTag == null || currentSession == null)
            {
                return;
            }

            currentSession.IdentifyUser(lastUserTag);
        }

        #region IServerConfigurationUpdateCallback implementation

        public void OnServerConfigurationUpdate(IServerConfiguration serverConfig)
        {
            lock (lockObject)
            {
                if (serverConfiguration != null)
                {
                    serverConfiguration = serverConfiguration.Merge(serverConfig);
                    return;
                }

                serverConfiguration = serverConfig;

                if (serverConfig.IsSessionSplitBySessionDurationEnabled ||
                    serverConfig.IsSessionSplitByIdleTimeoutEnabled)
                {
                    sessionWatchdog.AddToSplitByTimeout(this);
                }
            }
        }

        #endregion

        public override string ToString()
        {
            var beacon = currentSession.Beacon;
            return $"{GetType().Name} [sn={beacon.SessionNumber.ToInvariantString()}, seq={beacon.SessionSequenceNumber.ToInvariantString()}]";
        }
    }
}