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
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Core.Objects
{

    /// <summary>
    ///  Actual implementation of the ISession interface.
    /// </summary>
    internal class Session : OpenKitComposite, ISessionInternals
    {
        /// <summary>
        /// <see cref="ILogger"/> for tracing log messages.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Object for synchronization.
        /// </summary>
        private readonly object lockObject = new object();

        /// <summary>
        /// Parent object of this session.
        /// </summary>
        private readonly IOpenKitComposite parent;

        // end time of this Session
        private long endTime = -1;

        /// <summary>
        /// Indicates whether this session has already been ended or not.
        /// </summary>
        private bool isSessionEnded;

        // Configuration and Beacon reference
        private readonly IBeaconSender beaconSender;
        private readonly IBeacon beacon;

        internal Session(
            ILogger logger,
            IOpenKitComposite parent,
            IBeaconSender beaconSender,
            IBeacon beacon
            )
        {
            this.logger = logger;
            this.parent = parent;
            this.beaconSender = beaconSender;
            this.beacon = beacon;

            beaconSender.StartSession(this);
            beacon.StartSession();
        }

        /// <summary>
        /// Accessor for simplified explicit access to <see cref="ISessionInternals"/>.
        /// </summary>
        private ISessionInternals ThisSession => this;

        /// <summary>
        /// Accessor for simplified explicit access to <see cref="IOpenKitComposite"/>.
        /// </summary>
        private IOpenKitComposite ThisComposite => this;

        #region ISessionInternals implementation
        bool ISessionInternals.IsEmpty => beacon.IsEmpty;

        long ISessionInternals.EndTime => endTime;

        IBeaconConfiguration ISessionInternals.BeaconConfiguration
        {
            get => beacon.BeaconConfiguration;
            set => beacon.BeaconConfiguration = value;
        }

        bool ISessionInternals.IsSessionEnded => isSessionEnded;

        void ISessionInternals.ClearCapturedData()
        {
            beacon.ClearData();
        }

        StatusResponse ISessionInternals.SendBeacon(IHttpClientProvider clientProvider)
        {
            return beacon.Send(clientProvider);
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

            lock (lockObject)
            {
                if (!ThisSession.IsSessionEnded)
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
                if (!ThisSession.IsSessionEnded)
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

            lock (lockObject)
            {
                if (!ThisSession.IsSessionEnded)
                {
                    beacon.ReportCrash(errorName, reason, stacktrace);
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

            lock (lockObject)
            {
                if (!ThisSession.IsSessionEnded)
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
            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{this} End()");
            }

            lock (lockObject)
            {
                // check if end() was already called before by looking at endTime
                if (ThisSession.IsSessionEnded)
                {
                    return;
                }

                isSessionEnded = true;
            }

            // forcefully leave all child elements
            // Since the end time was set, no further child objects are added to the internal list so the following
            // operations are safe outside the lock block.
            var childObjects = ThisComposite.GetCopyOfChildObjects();
            foreach (var childObject in childObjects)
            {
                childObject.Dispose();
            }

            endTime = beacon.CurrentTimestamp;

            // create end session data on beacon
            beacon.EndSession(this);

            // finish session on configuration and stop managing it
            beaconSender.FinishSession(this);

             parent.OnChildClosed(this);
        }

        #endregion


        #region IOpenKitComposite implementation

        private protected override void OnChildClosed(IOpenKitObject childObject)
        {
            lock (lockObject)
            {
                ThisComposite.RemoveChildFromList(childObject);
            }
        }

        #endregion

        public override string ToString()
        {
            return $"{GetType().Name} [sn={beacon.SessionNumber}]";
        }
    }
}
