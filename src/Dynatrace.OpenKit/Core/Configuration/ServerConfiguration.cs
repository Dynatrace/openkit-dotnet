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

using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core.Configuration
{
    /// <summary>
    /// Configuration class storing all configuration parameters as returned by Dynatrace/AppMon
    /// </summary>
    public class ServerConfiguration : IServerConfiguration
    {
        #region default values

        /// <summary>
        /// Default server configuration instance.
        /// </summary>
        public static readonly IServerConfiguration Default = new Builder().Build();

        /// <summary>
        /// Capturing is enabled by default.
        /// </summary>
        public const bool DefaultCaptureEnabled = true;

        /// <summary>
        /// Crash reporting is enabled by default.
        /// </summary>
        public const bool DefaultCrashReportingEnabled = true;

        /// <summary>
        /// Error reporting is enabled by default.
        /// </summary>
        public const bool DefaultErrorReportingEnabled = true;

        /// <summary>
        /// Default send interval is not defined.
        /// </summary>
        public const int DefaultSendInterval = -1;

        /// <summary>
        /// Default server ID is not defined and depends on the backend.
        /// </summary>
        public const int DefaultServerId = -1;

        /// <summary>
        /// Default beacon size is not defined.
        /// </summary>
        public const int DefaultBeaconSize = -1;

        /// <summary>
        /// Default multiplicity is <code>1</code>.
        /// </summary>
        public const int DefaultMultiplicity = 1;

        public const int DefaultMaxSessionDuration = -1;
        public const int DefaultMaxEventsPerSession = -1;
        public const int DefaultSessionTimeout = -1;
        public const int DefaultVisitStoreVersion = 1;

        #endregion

        private readonly bool isSessionSplitByEventsEnabled;

        /// <summary>
        /// Creates a server configuration from the given builder.
        /// </summary>
        /// <param name="builder">the builder used to configure this instance.</param>
        private ServerConfiguration(Builder builder)
        {
            IsCaptureEnabled = builder.IsCaptureEnabled;
            IsCrashReportingEnabled = builder.IsCrashReportingEnabled;
            IsErrorReportingEnabled = builder.IsErrorReportingEnabled;
            SendIntervalInMilliseconds = builder.SendIntervalInMilliseconds;
            ServerId = builder.ServerId;
            BeaconSizeInBytes = builder.BeaconSizeInBytes;
            Multiplicity = builder.Multiplicity;
            MaxSessionDurationInMilliseconds = builder.MaxSessionDurationInMilliseconds;
            MaxEventsPerSession = builder.MaxEventsPerSession;
            isSessionSplitByEventsEnabled = builder.IsSessionSplitByEventsEnabled;
            SessionTimeoutInMilliseconds = builder.SessionTimeoutInMilliseconds;
            VisitStoreVersion = builder.VisitStoreVersion;
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

        public int SendIntervalInMilliseconds { get; }

        public int ServerId { get; }

        public int BeaconSizeInBytes { get; }

        public int Multiplicity { get; }

        public int MaxSessionDurationInMilliseconds { get; }

        public int MaxEventsPerSession { get; }

        public bool IsSessionSplitByEventsEnabled => isSessionSplitByEventsEnabled && MaxEventsPerSession > 0;

        public int SessionTimeoutInMilliseconds { get; }

        public int VisitStoreVersion { get; }

        public bool IsSendingDataAllowed => IsCaptureEnabled && Multiplicity > 0;

        public bool IsSendingCrashesAllowed => IsCrashReportingEnabled && IsSendingDataAllowed;

        public bool IsSendingErrorsAllowed => IsErrorReportingEnabled && IsSendingDataAllowed;

        public IServerConfiguration Merge(IServerConfiguration other)
        {
            var builder = new Builder(other);

            // take everything from other instance except multiplicity and server ID.
            builder.WithMultiplicity(Multiplicity);
            builder.WithServerId(ServerId);

            return builder.Build();
        }

        /// <summary>
        /// Builder class for creating a custom <see cref="IServerConfiguration"/> instance.
        /// </summary>
        public class Builder
        {
            /// <summary>
            /// Default constructor.
            /// </summary>
            public Builder()
            {
            }

            /// <summary>
            /// Creates a new builder instance with pre-initialized fields from the given <see cref="IResponseAttributes"/>.
            /// </summary>
            /// <param name="responseAttributes">response attributes used for initializing this builder.</param>
            public Builder(IResponseAttributes responseAttributes)
            {
                IsCaptureEnabled = responseAttributes.IsCapture;
                IsCrashReportingEnabled = responseAttributes.IsCaptureCrashes;
                IsErrorReportingEnabled = responseAttributes.IsCaptureErrors;
                SendIntervalInMilliseconds = responseAttributes.SendIntervalInMilliseconds;
                ServerId = responseAttributes.ServerId;
                BeaconSizeInBytes = responseAttributes.MaxBeaconSizeInBytes;
                Multiplicity = responseAttributes.Multiplicity;
                MaxSessionDurationInMilliseconds = responseAttributes.MaxSessionDurationInMilliseconds;
                MaxEventsPerSession = responseAttributes.MaxEventsPerSession;
                IsSessionSplitByEventsEnabled =
                    responseAttributes.IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION);
                SessionTimeoutInMilliseconds = responseAttributes.SessionTimeoutInMilliseconds;
                VisitStoreVersion = responseAttributes.VisitStoreVersion;
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
                SendIntervalInMilliseconds = serverConfiguration.SendIntervalInMilliseconds;
                ServerId = serverConfiguration.ServerId;
                BeaconSizeInBytes = serverConfiguration.BeaconSizeInBytes;
                Multiplicity = serverConfiguration.Multiplicity;
                MaxSessionDurationInMilliseconds = serverConfiguration.MaxSessionDurationInMilliseconds;
                MaxEventsPerSession = serverConfiguration.MaxEventsPerSession;
                IsSessionSplitByEventsEnabled = serverConfiguration.IsSessionSplitByEventsEnabled;
                SessionTimeoutInMilliseconds = serverConfiguration.SessionTimeoutInMilliseconds;
                VisitStoreVersion = serverConfiguration.VisitStoreVersion;
            }

            internal bool IsCaptureEnabled { get; private set; } = DefaultCaptureEnabled;

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

            internal bool IsCrashReportingEnabled { get; private set; } = DefaultCrashReportingEnabled;

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

            internal bool IsErrorReportingEnabled { get; private set; } = DefaultErrorReportingEnabled;

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

            internal int SendIntervalInMilliseconds { get; private set; } = DefaultSendInterval;

            /// <summary>
            /// Configures the send interval.
            /// </summary>
            /// <param name="sendIntervalInMilliseconds">the send interval in milliseconds.</param>
            /// <returns><code>this</code></returns>
            public Builder WithSendingIntervalInMilliseconds(int sendIntervalInMilliseconds)
            {
                SendIntervalInMilliseconds = sendIntervalInMilliseconds;
                return this;
            }

            internal int ServerId { get; private set; } = DefaultServerId;

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

            internal int BeaconSizeInBytes { get; private set; } = DefaultBeaconSize;

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

            internal int Multiplicity { get; private set; } = DefaultMultiplicity;

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

            internal int MaxSessionDurationInMilliseconds { get; private set; } = DefaultMaxSessionDuration;

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

            internal bool IsSessionSplitByEventsEnabled { get; }

            internal int MaxEventsPerSession { get; private set; } = DefaultMaxEventsPerSession;

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

            internal int SessionTimeoutInMilliseconds { get; private set; } = DefaultSessionTimeout;

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

            internal int VisitStoreVersion { get; private set; } = DefaultVisitStoreVersion;

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