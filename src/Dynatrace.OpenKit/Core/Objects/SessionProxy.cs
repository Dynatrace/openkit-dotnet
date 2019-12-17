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

namespace Dynatrace.OpenKit.Core.Objects
{
    public class SessionProxy : OpenKitComposite, ISession
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
        /// the current session instance
        /// </summary>
        private ISessionInternals currentSession;

        /// <summary>
        /// holds the number of received calls <see cref="EnterAction"/>
        /// </summary>
        private int topLevelActionCount;

        /// <summary>
        /// the server configuration of the first session (will be initialized when the first session is updated with
        /// the server configuration)
        /// </summary>
        private IServerConfiguration serverConfiguration;

        /// <summary>
        /// Indicator if this session proxy is already finished or not.
        /// </summary>
        private bool isFinished;

        internal SessionProxy(ILogger logger, IOpenKitComposite parent, ISessionCreator sessionCreator)
        {
            this.logger = logger;
            this.parent = parent;
            this.sessionCreator = sessionCreator;

            currentSession = CreateSession();
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

                var session = GetOrSplitCurrentSession();
                RecordTopLevelActionEvent();
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

                var session = GetOrSplitCurrentSession();
                RecordTopLevelEventInteraction();
                session.IdentifyUser(userTag);
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

                var session = GetOrSplitCurrentSession();
                RecordTopLevelEventInteraction();
                session.ReportCrash(errorName, reason, stacktrace);
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

                var session = GetOrSplitCurrentSession();
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
        }

        #endregion

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
            }
        }

        #endregion

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

        private ISessionInternals GetOrSplitCurrentSession()
        {
            if (IsSessionSplitRequired())
            {
                currentSession = CreateSession();
                currentSession.UpdateServerConfiguration(serverConfiguration);
                topLevelActionCount = 0;
            }

            return currentSession;
        }

        private bool IsSessionSplitRequired()
        {
            if (serverConfiguration == null || !serverConfiguration.IsSessionSplitByEventsEnabled)
            {
                return false;
            }

            return serverConfiguration.MaxEventsPerSession <= topLevelActionCount;
        }

        private ISessionInternals CreateSession()
        {
            var session = sessionCreator.CreateSession(this);
            session.Beacon.OnServerConfigurationUpdate += OnServerConfigurationUpdate;

            ThisComposite.StoreChildInList(session);

            return session;
        }

        private void RecordTopLevelEventInteraction()
        {
            // nothing for now
        }

        private void RecordTopLevelActionEvent()
        {
            topLevelActionCount++;
            RecordTopLevelEventInteraction();
        }

        #region IServerConfigurationUpdateCallback implementation

        public void OnServerConfigurationUpdate(IServerConfiguration serverConfig)
        {
            lock (lockObject)
            {
                if (serverConfiguration == null)
                {
                    serverConfiguration = serverConfig;
                }
            }
        }

        #endregion

        public override string ToString()
        {
            return $"{GetType().Name}";
        }
    }
}