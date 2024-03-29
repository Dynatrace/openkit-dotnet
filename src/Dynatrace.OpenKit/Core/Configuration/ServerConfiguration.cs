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

using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core.Configuration
{
    /// <summary>
    /// Configuration class storing all configuration parameters as returned by Dynatrace
    /// </summary>
    public class ServerConfiguration : IServerConfiguration
    {
        private static readonly IResponseAttributes DefaultValues = ResponseAttributesDefaults.Undefined;

        public static readonly  IServerConfiguration Default = From(DefaultValues);

        /// <summary>
        /// indicator whether session splitting by events is enabled or not.
        /// </summary>
        private readonly bool isSessionSplitByEventsEnabled;
        /// <summary>
        /// indicator whether session splitting by exceeding the maximum session duration is enabled or not.
        /// </summary>
        private readonly bool isSessionSplitBySessionDurationEnabled;
        /// <summary>
        /// indicator whether session splitting by exceeding the idle timeout is enabled or not.
        /// </summary>
        private readonly bool isSessionSplitByIdleTimeoutEnabled;

        /// <summary>
        /// Creates a server configuration from the given builder.
        /// </summary>
        /// <param name="builder">the builder used to configure this instance.</param>
        private ServerConfiguration(Builder builder)
        {
            IsCaptureEnabled = builder.IsCaptureEnabled;
            IsCrashReportingEnabled = builder.IsCrashReportingEnabled;
            IsErrorReportingEnabled = builder.IsErrorReportingEnabled;
            ServerId = builder.ServerId;
            BeaconSizeInBytes = builder.BeaconSizeInBytes;
            Multiplicity = builder.Multiplicity;
            SendIntervalInMilliseconds = builder.SendIntervalInMilliseconds;
            MaxSessionDurationInMilliseconds = builder.MaxSessionDurationInMilliseconds;
            isSessionSplitBySessionDurationEnabled = builder.IsSessionSplitBySessionDurationEnabled;
            MaxEventsPerSession = builder.MaxEventsPerSession;
            isSessionSplitByEventsEnabled = builder.IsSessionSplitByEventsEnabled;
            SessionTimeoutInMilliseconds = builder.SessionTimeoutInMilliseconds;
            isSessionSplitByIdleTimeoutEnabled = builder.IsSessionSplitByIdleTimeoutEnabled;
            VisitStoreVersion = builder.VisitStoreVersion;
            TrafficControlPercentage = builder.TrafficControlPercentage;
        }

        /// <summary>
        /// Creates a new server configuration from the given <see cref="IResponseAttributes"/>.
        /// </summary>
        /// <param name="responseAttributes">the response attributes from which to create the server configuration.</param>
        /// <returns>the newly created server configuration.</returns>
        public static IServerConfiguration From(IResponseAttributes responseAttributes)
        {
            if (responseAttributes == null)
            {
                return null;
            }

            return new Builder(responseAttributes).Build();
        }

        public bool IsCaptureEnabled { get; }

        public bool IsCrashReportingEnabled { get; }

        public bool IsErrorReportingEnabled { get; }

        public int ServerId { get; }

        public int BeaconSizeInBytes { get; }

        public int Multiplicity { get; }

        public int SendIntervalInMilliseconds { get; }

        public int MaxSessionDurationInMilliseconds { get; }

        public bool IsSessionSplitBySessionDurationEnabled =>
            isSessionSplitBySessionDurationEnabled && MaxSessionDurationInMilliseconds > 0;

        public int MaxEventsPerSession { get; }

        public bool IsSessionSplitByEventsEnabled => isSessionSplitByEventsEnabled && MaxEventsPerSession > 0;

        public int SessionTimeoutInMilliseconds { get; }

        public bool IsSessionSplitByIdleTimeoutEnabled =>
            isSessionSplitByIdleTimeoutEnabled && SessionTimeoutInMilliseconds > 0;

        public int VisitStoreVersion { get; }

        public int TrafficControlPercentage { get; }

        public bool IsSendingDataAllowed => IsCaptureEnabled && Multiplicity > 0;

        public bool IsSendingCrashesAllowed => IsCrashReportingEnabled && IsSendingDataAllowed;

        public bool IsSendingErrorsAllowed => IsErrorReportingEnabled && IsSendingDataAllowed;

        public IServerConfiguration Merge(IServerConfiguration other)
        {
            var builder = new Builder(other);

            // settings from this
            builder.WithMultiplicity(Multiplicity);
            builder.WithServerId(ServerId);
            builder.WithMaxSessionDurationInMilliseconds(MaxSessionDurationInMilliseconds);
            builder.WithMaxEventsPerSession(MaxEventsPerSession);
            builder.WithSessionTimeoutInMilliseconds(SessionTimeoutInMilliseconds);
            builder.WithVisitStoreVersion(VisitStoreVersion);
            builder.WithTrafficControlPercentage(TrafficControlPercentage);
            builder.IsSessionSplitByEventsEnabled = IsSessionSplitByEventsEnabled;
            builder.IsSessionSplitBySessionDurationEnabled = IsSessionSplitBySessionDurationEnabled;
            builder.IsSessionSplitByIdleTimeoutEnabled = IsSessionSplitByIdleTimeoutEnabled;

            return builder.Build();
        }

        /// <summary>
        /// Builder class for creating a custom <see cref="IServerConfiguration"/> instance.
        /// </summary>
        public class Builder
        {
            /// <summary>
            /// Creates a new builder instance with pre-initialized fields from the given <see cref="IResponseAttributes"/>.
            /// </summary>
            /// <param name="responseAttributes">response attributes used for initializing this builder.</param>
            public Builder(IResponseAttributes responseAttributes)
            {
                IsCaptureEnabled = responseAttributes.IsCapture;
                IsCrashReportingEnabled = responseAttributes.IsCaptureCrashes;
                IsErrorReportingEnabled = responseAttributes.IsCaptureErrors;
                ServerId = responseAttributes.ServerId;
                BeaconSizeInBytes = responseAttributes.MaxBeaconSizeInBytes;
                Multiplicity = responseAttributes.Multiplicity;

                SendIntervalInMilliseconds = responseAttributes.SendIntervalInMilliseconds;

                MaxSessionDurationInMilliseconds = responseAttributes.MaxSessionDurationInMilliseconds;
                IsSessionSplitBySessionDurationEnabled =
                    responseAttributes.IsAttributeSet(ResponseAttribute.MAX_SESSION_DURATION);

                MaxEventsPerSession = responseAttributes.MaxEventsPerSession;
                IsSessionSplitByEventsEnabled =
                    responseAttributes.IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION);

                SessionTimeoutInMilliseconds = responseAttributes.SessionTimeoutInMilliseconds;
                IsSessionSplitByIdleTimeoutEnabled =
                    responseAttributes.IsAttributeSet(ResponseAttribute.SESSION_IDLE_TIMEOUT);

                VisitStoreVersion = responseAttributes.VisitStoreVersion;

                TrafficControlPercentage = responseAttributes.TrafficControlPercentage;
            }

            /// <summary>
            /// Creates a new builder instance with pre-initialized fields from the given <see cref="IServerConfiguration"/>
            /// </summary>
            /// <param name="serverConfiguration">the server configuration from which to initialize the builder instance.</param>
            public Builder(IServerConfiguration serverConfiguration)
            {
                IsCaptureEnabled = serverConfiguration.IsCaptureEnabled;
                IsCrashReportingEnabled = serverConfiguration.IsCrashReportingEnabled;
                IsErrorReportingEnabled = serverConfiguration.IsErrorReportingEnabled;
                ServerId = serverConfiguration.ServerId;
                BeaconSizeInBytes = serverConfiguration.BeaconSizeInBytes;
                Multiplicity = serverConfiguration.Multiplicity;

                SendIntervalInMilliseconds = serverConfiguration.SendIntervalInMilliseconds;
                MaxSessionDurationInMilliseconds = serverConfiguration.MaxSessionDurationInMilliseconds;
                IsSessionSplitBySessionDurationEnabled = serverConfiguration.IsSessionSplitBySessionDurationEnabled;

                MaxEventsPerSession = serverConfiguration.MaxEventsPerSession;
                IsSessionSplitByEventsEnabled = serverConfiguration.IsSessionSplitByEventsEnabled;

                SessionTimeoutInMilliseconds = serverConfiguration.SessionTimeoutInMilliseconds;
                IsSessionSplitByIdleTimeoutEnabled = serverConfiguration.IsSessionSplitByIdleTimeoutEnabled;

                VisitStoreVersion = serverConfiguration.VisitStoreVersion;

                TrafficControlPercentage = serverConfiguration.TrafficControlPercentage;
            }

            internal bool IsCaptureEnabled { get; private set; }

            /// <summary>
            /// Enables/disables capturing by setting <see cref="IsCaptureEnabled"/> to the corresponding value.
            /// </summary>
            /// <param name="captureState">the capture state to set.</param>
            /// <returns><code>this</code></returns>
            public Builder WithCapture(bool captureState)
            {
                IsCaptureEnabled = captureState;
                return this;
            }

            internal bool IsCrashReportingEnabled { get; private set; }

            /// <summary>
            /// Enables/disables crash reporting by setting <see cref="IsCrashReportingEnabled"/> to the corresponding
            /// value.
            /// </summary>
            /// <param name="crashReportingState">the crash reporting state to set.</param>
            /// <returns><code>this</code></returns>
            public Builder WithCrashReporting(bool crashReportingState)
            {
                IsCrashReportingEnabled = crashReportingState;
                return this;
            }

            internal bool IsErrorReportingEnabled { get; private set; }

            /// <summary>
            /// Enables/disables error reporting by setting <see cref="IsErrorReportingEnabled"/> to the corresponding
            /// value.
            /// </summary>
            /// <param name="errorReportingState">the error reporting state to set.</param>
            /// <returns><code>this</code></returns>
            public Builder WithErrorReporting(bool errorReportingState)
            {
                IsErrorReportingEnabled = errorReportingState;
                return this;
            }

            internal int ServerId { get; private set; }

            /// <summary>
            /// Configures the server ID.
            /// </summary>
            /// <param name="serverId">the ID of the server to communicate with.</param>
            /// <returns></returns>
            public Builder WithServerId(int serverId)
            {
                ServerId = serverId;
                return this;
            }

            internal int BeaconSizeInBytes { get; private set; }

            /// <summary>
            /// Configures the beacon size in bytes.
            /// </summary>
            /// <param name="beaconSize">teh maximum allowed beacon size in bytes.</param>
            /// <returns><code>this</code></returns>
            public Builder WithBeaconSizeInBytes(int beaconSize)
            {
                BeaconSizeInBytes = beaconSize;
                return this;
            }

            internal int Multiplicity { get; private set; }

            /// <summary>
            /// Configures the multiplicity factor.
            /// </summary>
            /// <param name="multiplicity">multiplicity factor</param>
            /// <returns><code>this</code></returns>
            public Builder WithMultiplicity(int multiplicity)
            {
                Multiplicity = multiplicity;
                return this;
            }

            internal int SendIntervalInMilliseconds { get; set; }

            /// <summary>
            /// Configure the send interval in milliseconds.
            /// </summary>
            /// <param name="sendIntervalInMilliseconds">Send interval in milliseconds</param>
            /// <returns></returns>
            public Builder WithSendIntervalInMilliseconds(int sendIntervalInMilliseconds)
            {
                SendIntervalInMilliseconds = sendIntervalInMilliseconds;
                return this;
            }

            internal bool IsSessionSplitBySessionDurationEnabled { get; set; }
            internal int MaxSessionDurationInMilliseconds { get; private set; }

            /// <summary>
            /// Configures the maximum duration after which the session gets split.
            /// </summary>
            /// <param name="maxSessionDurationInMilliseconds">the maximum duration of a session in milliseconds</param>
            /// <returns><code>this</code></returns>
            public Builder WithMaxSessionDurationInMilliseconds(int maxSessionDurationInMilliseconds)
            {
                MaxSessionDurationInMilliseconds = maxSessionDurationInMilliseconds;
                return this;
            }

            internal bool IsSessionSplitByEventsEnabled { get; set; }
            internal int MaxEventsPerSession { get; private set; }

            /// <summary>
            /// Configures the maximum number of events after which the session gets split.
            /// </summary>
            /// <param name="maxEventsPerSession">the maximum number of top level actions after which a session gets split</param>
            /// <returns><code>this</code></returns>
            public Builder WithMaxEventsPerSession(int maxEventsPerSession)
            {
                MaxEventsPerSession = maxEventsPerSession;
                return this;
            }

            internal bool IsSessionSplitByIdleTimeoutEnabled { get; set; }
            internal int SessionTimeoutInMilliseconds { get; private set; }

            /// <summary>
            /// Configures the idle timeout after which a session gets split.
            /// </summary>
            /// <param name="sessionTimeoutInMilliseconds">the idle timeout in milliseconds after which a session gets split</param>
            /// <returns><code>this</code></returns>
            public Builder WithSessionTimeoutInMilliseconds(int sessionTimeoutInMilliseconds)
            {
                SessionTimeoutInMilliseconds = sessionTimeoutInMilliseconds;
                return this;
            }

            internal int VisitStoreVersion { get; private set; }

            /// <summary>
            /// Configures the version of the visit store that is to be used.
            /// </summary>
            /// <param name="visitStoreVersion">the version of the visit store to be used.</param>
            /// <returns><code>this</code></returns>
            public Builder WithVisitStoreVersion(int visitStoreVersion)
            {
                VisitStoreVersion = visitStoreVersion;
                return this;
            }

            internal int TrafficControlPercentage { get; private set; }

            /// <summary>
            /// Configures the traffic control/cost control percentage.
            /// <para>
            /// This value is used as rate limit to limit the number of sessions being captured.
            /// </para>
            /// </summary>
            /// <param name="trafficControlPercentage">Percentage of sessions being captured.</param>
            /// <returns><code>this</code></returns>
            public Builder WithTrafficControlPercentage(int trafficControlPercentage)
            {
                TrafficControlPercentage = trafficControlPercentage;
                return this;
            }

            /// <summary>
            /// Builds a new instance of <see cref="IServerConfiguration"/>.
            /// </summary>
            /// <returns>a newly created <see cref="IServerConfiguration"/> instance.</returns>
            public IServerConfiguration Build()
            {
                return new ServerConfiguration(this);
            }
        }
    }
}