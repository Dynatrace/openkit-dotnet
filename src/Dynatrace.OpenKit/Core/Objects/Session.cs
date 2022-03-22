//
// Copyright 2018-2021 Dynatrace LLC
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
using System.Text;
using System.Threading;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using Dynatrace.OpenKit.Util;
using Dynatrace.OpenKit.Util.Json.Objects;

namespace Dynatrace.OpenKit.Core.Objects
{
    /// <summary>
    ///  Actual implementation of the ISession interface.
    /// </summary>
    internal class Session : OpenKitComposite, ISessionInternals
    {
        /// <summary>
        /// Helper to reduce ToString() effort.
        /// </summary>
        private string toString;

        /// <summary>
        /// The maximum number of "new session requests" to send per session.
        /// </summary>
        internal const int MaxNewSessionRequests = 4;

        /// <summary>
        /// <see cref="ILogger"/> for tracing log messages.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Parent object of this session.
        /// </summary>
        private readonly IOpenKitComposite parent;

        /// <summary>
        /// Beacon reference.
        /// </summary>
        private readonly IBeacon beacon;

        /// <summary>
        /// Current state of the session (also used for synchronization).
        /// </summary>
        private readonly SessionState state;

        /// <summary>
        /// the number of retries for new session requests.
        /// </summary>
        private int numRemainingNewSessionRequests = MaxNewSessionRequests;

        /// <summary>
        /// the grace period after which the session gets closed by the session watchdog thread.
        /// </summary>
        private long splitByEventsGracePeriodEndTimeInMillis = -1;


        internal Session(
            ILogger logger,
            IOpenKitComposite parent,
            IBeacon beacon
        )
        {
            state = new SessionState(this);
            this.logger = logger;
            this.parent = parent;
            this.beacon = beacon;

            beacon.StartSession();
        }

        /// <summary>
        /// Accessor for simplified explicit access to <see cref="IOpenKitComposite"/>.
        /// </summary>
        private IOpenKitComposite ThisComposite => this;

        #region ISessionInternals implementation

        bool ISessionInternals.IsEmpty => beacon.IsEmpty;

        void ISessionInternals.ClearCapturedData()
        {
            beacon.ClearData();
        }

        IStatusResponse ISessionInternals.SendBeacon(IHttpClientProvider clientProvider,
            IAdditionalQueryParameters additionalParameters)
        {
            return beacon.Send(clientProvider, additionalParameters);
        }

        void ISessionInternals.InitializeServerConfiguration(IServerConfiguration initialServerConfig)
        {
            beacon.InitializeServerConfiguration(initialServerConfig);
        }

        void ISessionInternals.UpdateServerConfiguration(IServerConfiguration serverConfiguration)
        {
            beacon.UpdateServerConfiguration(serverConfiguration);
        }

        ISessionState ISessionInternals.State => state;

        bool ISessionInternals.IsDataSendingAllowed => state.IsConfigured && beacon.IsDataCapturingEnabled;

        void ISessionInternals.EnableCapture()
        {
            beacon.EnableCapture();
        }

        void ISessionInternals.DisableCapture()
        {
            beacon.DisableCapture();
        }

        bool ISessionInternals.CanSendNewSessionRequest => numRemainingNewSessionRequests > 0;

        void ISessionInternals.DecreaseNumRemainingSessionRequests()
        {
            numRemainingNewSessionRequests--;
        }

        IBeacon ISessionInternals.Beacon => beacon;

        bool ISessionInternals.TryEnd()
        {
            lock (state)
            {
                if (state.IsFinishingOrFinished)
                {
                    return true;
                }

                if (ThisComposite.GetChildCount() == 0)
                {
                    End(false);
                    return true;
                }

                state.MarkAsWasTriedForEnding();
                return false;
            }
        }

        long ISessionInternals.SplitByEventsGracePeriodEndTimeInMillis
        {
            get => Interlocked.Read(ref splitByEventsGracePeriodEndTimeInMillis);
            set => Interlocked.CompareExchange(ref splitByEventsGracePeriodEndTimeInMillis, value, -1);
        }

        #endregion

        public override void Dispose()
        {
            End();
        }

        #region ISession implementation

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

            lock (state)
            {
                if (!state.IsFinishingOrFinished)
                {
                    var result = new RootAction(logger, this, actionName, beacon);
                    ThisComposite.StoreChildInList(result);

                    return result;
                }
            }

            return NullRootAction.Instance;
        }

        public void IdentifyUser(string userTag)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{this} IdentifyUser({userTag})");
            }

            lock (state)
            {
                if (!state.IsFinishingOrFinished)
                {
                    beacon.IdentifyUser(userTag);
                }
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

            lock (state)
            {
                if (!state.IsFinishingOrFinished)
                {
                    beacon.ReportCrash(errorName, reason, stacktrace);
                }
            }
        }

        public void ReportCrash(Exception exception)
        {
            if (exception == null)
            {
                logger.Warn($"{this} ReportCrash: exception must not be null");
                return;
            }

            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{this} ReportCrash({exception})");
            }

            lock (state)
            {
                if (!state.IsFinishingOrFinished)
                {
                    beacon.ReportCrash(exception);
                }
            }
        }

        void ISession.SendEvent(string name, Dictionary<string, JsonValue> attributes)
        {
            if (string.IsNullOrEmpty(name))
            {
                logger.Warn($"{this} SendEvent: name must not be null or empty");
                return;
            }

            if(attributes != null && attributes.ContainsKey("name"))
            {
                logger.Warn($"{this} SendEvent: name must not be used in the attributes as it will be overridden!");
            }

            if(attributes == null)
            {
                attributes = new Dictionary<string, JsonValue>();
            }

            attributes["name"] = JsonStringValue.FromString(name);

            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{this} SendEvent({name},{attributes})");
            }

            lock (state)
            {
                if (!state.IsFinishingOrFinished)
                {
                    beacon.SendEvent(name, JsonObjectValue.FromDictionary(attributes).ToString());
                }
            }
        }

        public IWebRequestTracer TraceWebRequest(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                logger.Warn($"{this} TraceWebRequest(String): url must not be null or empty");
                return NullWebRequestTracer.Instance;
            }

            if (!WebRequestTracer.IsValidUrlScheme(url))
            {
                logger.Warn($"{this} TraceWebRequest(String): url \"{url}\" does not have a valid scheme");
                return NullWebRequestTracer.Instance;
            }

            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{this} TraceWebRequest({url})");
            }

            lock (state)
            {
                if (!state.IsFinishingOrFinished)
                {
                    var webRequestTracer = new WebRequestTracer(logger, this, beacon, url);
                    ThisComposite.StoreChildInList(webRequestTracer);

                    return webRequestTracer;
                }
            }

            return NullWebRequestTracer.Instance;
        }

        public void End()
        {
            End(true);
        }

        public void End(bool sendSessionEndEvent)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{this} End()");
            }

            if (!state.MarkAsIsFinishing())
            {
                return; // end was already called before
            }

            // forcefully leave all child elements
            // Since the end time was set, no further child objects are added to the internal list so the following
            // operations are safe outside the lock block.
            var childObjects = ThisComposite.GetCopyOfChildObjects();
            foreach (var childObject in childObjects)
            {
                childObject.Dispose();
            }

            // send the end event, only if a session is explicitly ended
            if (sendSessionEndEvent)
            {
                beacon.EndSession();
            }


            state.MarkAsFinished();

            // last but not least update parent relation
            parent.OnChildClosed(this);
        }

        #endregion


        #region IOpenKitComposite implementation

        private protected override void OnChildClosed(IOpenKitObject childObject)
        {
            lock (state)
            {
                ThisComposite.RemoveChildFromList(childObject);

                if (state.WasTriedForEnding && ThisComposite.GetChildCount() == 0)
                {
                    End(false);
                }
            }
        }

        #endregion

        public override string ToString()
        {
            return toString ??
                (toString = new StringBuilder(GetType().Name)
                    .Append(" [sn=").Append(beacon.SessionNumber.ToInvariantString())
                    .Append(", seq=").Append(beacon.SessionSequenceNumber.ToInvariantString())
                    .Append("]").ToString());
        }

        #region session state

        private class SessionState : ISessionState
        {
            private readonly Session session;

            private bool isFinishing;
            private bool isFinished;
            private bool wasTriedForEnding;

            internal SessionState(Session session)
            {
                this.session = session;
            }

            public bool WasTriedForEnding
            {
                get
                {
                    lock (this)
                    {
                        return wasTriedForEnding;
                    }
                }
            }


            public bool IsConfigured
            {
                get
                {
                    lock (this)
                    {
                        return session.beacon.IsServerConfigurationSet;
                    }
                }
            }

            public bool IsConfiguredAndFinished
            {
                get
                {
                    lock (this)
                    {
                        return IsConfigured && isFinished;
                    }
                }
            }

            public bool IsConfiguredAndOpen
            {
                get
                {
                    lock (this)
                    {
                        return IsConfigured && !isFinished;
                    }
                }
            }

            public bool IsFinished
            {
                get
                {
                    lock (this)
                    {
                        return isFinished;
                    }
                }
            }

            public bool IsFinishingOrFinished
            {
                get
                {
                    lock (this)
                    {
                        return isFinishing || isFinished;
                    }
                }
            }

            internal bool MarkAsIsFinishing()
            {
                lock (this)
                {
                    if (IsFinishingOrFinished)
                    {
                        return false;
                    }

                    isFinishing = true;
                    return true;
                }
            }

            internal void MarkAsFinished()
            {
                lock (this)
                {
                    isFinished = true;
                }
            }

            internal void MarkAsWasTriedForEnding()
            {
                lock (this)
                {
                    wasTriedForEnding = true;
                }
            }
        }

        #endregion
    }
}