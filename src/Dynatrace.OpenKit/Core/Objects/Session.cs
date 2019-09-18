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

using System.Threading;
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
        private static readonly NullRootAction NullRootAction = new NullRootAction();
        private static readonly NullWebRequestTracer NullWebRequestTracer = new NullWebRequestTracer();

        private readonly ILogger logger;

        // end time of this Session
        private long endTime = -1;

        // Configuration and Beacon reference
        private readonly IBeaconSender beaconSender;
        private readonly IBeacon beacon;

        // used for taking care to really leave all Actions at the end of this Session
        private readonly SynchronizedQueue<IAction> openRootActions = new SynchronizedQueue<IAction>();


        internal Session(ILogger logger, IBeaconSender beaconSender, IBeacon beacon)
        {
            this.logger = logger;
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

        long ISessionInternals.EndTime => Interlocked.Read(ref endTime);

        IBeaconConfiguration ISessionInternals.BeaconConfiguration
        {
            get => beacon.BeaconConfiguration;
            set => beacon.BeaconConfiguration = value;
        }

        bool ISessionInternals.IsSessionEnded => ThisSession.EndTime != -1;

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
                return NullRootAction;
            }
            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{this} EnterAction({actionName})");
            }
            if (!ThisSession.IsSessionEnded)
            {
                var result = new RootAction(logger, this, actionName, beacon);
                openRootActions.Put(result);

                return result;
            }

            return NullRootAction;
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
            if (!ThisSession.IsSessionEnded)
            {
                beacon.IdentifyUser(userTag);
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
            if (!ThisSession.IsSessionEnded)
            {
                beacon.ReportCrash(errorName, reason, stacktrace);
            }
        }

        public IWebRequestTracer TraceWebRequest(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                logger.Warn($"{this} TraceWebRequest(String): url must not be null or empty");
                return NullWebRequestTracer;
            }
            if (!WebRequestTracer.IsValidUrlScheme(url))
            {
                logger.Warn($"{this} TraceWebRequest(String): url \"{url}\" does not have a valid scheme");
                return NullWebRequestTracer;
            }
            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{this} TraceWebRequest({url})");
            }
            if (!ThisSession.IsSessionEnded)
            {
                return new WebRequestTracer(logger, this, beacon, url);
            }

            return NullWebRequestTracer;
        }

        public void End()
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug($"{this} End()");
            }

            // check if end() was already called before by looking at endTime
            if (Interlocked.CompareExchange(ref endTime, beacon.CurrentTimestamp, -1L) != -1L)
            {
                return;
            }

            // leave all Root-Actions for sanity reasons
            while (!openRootActions.IsEmpty())
            {
                var action = openRootActions.Get();
                action.LeaveAction();
            }

            endTime = beacon.CurrentTimestamp;

            // create end session data on beacon
            beacon.EndSession(this);

            // finish session on configuration and stop managing it
            beaconSender.FinishSession(this);
        }

        #endregion


        #region IOpenKitComposite implementation

        private protected override void OnChildClosed(IOpenKitObject childObject)
        {
            ThisComposite.RemoveChildFromList(childObject);
        }

        #endregion

        public override string ToString()
        {
            return $"{GetType().Name} [sn={beacon.SessionNumber}]";
        }
    }
}
