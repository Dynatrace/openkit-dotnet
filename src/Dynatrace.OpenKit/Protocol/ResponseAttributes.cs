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

namespace Dynatrace.OpenKit.Protocol
{
    /// <summary>
    /// Implements <see cref="IResponseAttributes"/> providing all attributes received from the server.
    /// </summary>
    public class ResponseAttributes : IResponseAttributes
    {
        /// <summary>
        /// Represents the set of attributes which are set / were sent by the server.
        /// </summary>
        private readonly HashSet<ResponseAttribute> setAttributes;

        private ResponseAttributes(Builder builder)
        {
            setAttributes = new HashSet<ResponseAttribute>(builder.SetAttributes);

            MaxBeaconSizeInBytes = builder.MaxBeaconSizeInBytes;
            MaxSessionDurationInMilliseconds = builder.MaxSessionDurationInMilliseconds;
            MaxEventsPerSession = builder.MaxEventsPerSession;
            SessionTimeoutInMilliseconds = builder.SessionTimeoutInMilliseconds;
            SendIntervalInMilliseconds = builder.SendIntervalInMilliseconds;
            VisitStoreVersion = builder.VisitStoreVersion;

            IsCapture = builder.IsCapture;
            IsCaptureCrashes = builder.IsCaptureCrashes;
            IsCaptureErrors = builder.IsCaptureErrors;
            ApplicationId = builder.ApplicationId;

            Multiplicity = builder.Multiplicity;
            ServerId = builder.ServerId;
            Status = builder.Status;

            TimestampInMilliseconds = builder.TimestampInMilliseconds;
        }

        #region Factory methods

        /// <summary>
        /// Creates a new builder initialized with the defaults value for <see cref="KeyValueResponseParser"/> key-value
        /// parsing.
        /// </summary>
        public static Builder WithKeyValueDefaults()
        {
            return new Builder(ResponseAttributesDefaults.KeyValueResponse);
        }

        /// <summary>
        /// Creates a new builder instance with default values for <see cref="JsonResponseParser"/> JSON parsing.
        /// </summary>
        /// <returns></returns>
        public static Builder WithJsonDefaults()
        {
            return new Builder(ResponseAttributesDefaults.JsonResponse);
        }

        /// <summary>
        /// Creates a new builder instance with undefined default values.
        /// </summary>
        /// <returns></returns>
        public static Builder WithUndefinedDefaults()
        {
            return new Builder(ResponseAttributesDefaults.Undefined);
        }

        #endregion

        #region IResponseAttributes implementation
        public int MaxBeaconSizeInBytes { get; }

        public int MaxSessionDurationInMilliseconds { get; }

        public int MaxEventsPerSession { get; }

        public int SessionTimeoutInMilliseconds { get; }

        public int SendIntervalInMilliseconds { get; }

        public int VisitStoreVersion { get; }

        public bool IsCapture { get; }

        public bool IsCaptureCrashes { get; }

        public bool IsCaptureErrors { get; }

        public string ApplicationId { get; }

        public int Multiplicity { get; }

        public int ServerId { get; }

        public string Status { get; }

        public long TimestampInMilliseconds { get; }

        public bool IsAttributeSet(ResponseAttribute attribute)
        {
            return setAttributes.Contains(attribute);
        }

        #region Merge

        public IResponseAttributes Merge(IResponseAttributes responseAttributes)
        {
            var builder = new Builder(this);

            ApplyBeaconSize(builder, responseAttributes);
            ApplySessionDuration(builder, responseAttributes);
            ApplyEventsPerSession(builder, responseAttributes);
            ApplySessionTimeout(builder, responseAttributes);
            ApplySendInterval(builder, responseAttributes);
            ApplyVisitStoreVersion(builder, responseAttributes);
            ApplyCapture(builder, responseAttributes);
            ApplyCaptureCrashes(builder, responseAttributes);
            ApplyCaptureErrors(builder, responseAttributes);
            ApplyApplicationId(builder, responseAttributes);
            ApplyMultiplicity(builder, responseAttributes);
            ApplyServerId(builder, responseAttributes);
            ApplyStatus(builder, responseAttributes);
            ApplyTimestamp(builder, responseAttributes);

            return builder.Build();
        }

        private static void ApplyBeaconSize(Builder builder, IResponseAttributes attributes)
        {
            if (attributes.IsAttributeSet(ResponseAttribute.MAX_BEACON_SIZE))
            {
                builder.WithMaxBeaconSizeInBytes(attributes.MaxBeaconSizeInBytes);
            }
        }

        private static void ApplySessionDuration(Builder builder, IResponseAttributes attributes)
        {
            if (attributes.IsAttributeSet(ResponseAttribute.MAX_SESSION_DURATION))
            {
                builder.WithMaxSessionDurationInMilliseconds(attributes.MaxSessionDurationInMilliseconds);
            }
        }

        private static void ApplyEventsPerSession(Builder builder, IResponseAttributes attributes)
        {
            if (attributes.IsAttributeSet(ResponseAttribute.MAX_EVENTS_PER_SESSION))
            {
                builder.WithMaxEventsPerSession(attributes.MaxEventsPerSession);
            }
        }

        private static void ApplySessionTimeout(Builder builder, IResponseAttributes attributes)
        {
            if (attributes.IsAttributeSet(ResponseAttribute.SESSION_IDLE_TIMEOUT))
            {
                builder.WithSessionTimeoutInMilliseconds(attributes.SessionTimeoutInMilliseconds);
            }
        }

        private static void ApplySendInterval(Builder builder, IResponseAttributes attributes)
        {
            if (attributes.IsAttributeSet(ResponseAttribute.SEND_INTERVAL))
            {
                builder.WithSendIntervalInMilliseconds(attributes.SendIntervalInMilliseconds);
            }
        }

        private static void ApplyVisitStoreVersion(Builder builder, IResponseAttributes attributes)
        {
            if (attributes.IsAttributeSet(ResponseAttribute.VISIT_STORE_VERSION))
            {
                builder.WithVisitStoreVersion(attributes.VisitStoreVersion);
            }
        }

        private static void ApplyCapture(Builder builder, IResponseAttributes attributes)
        {
            if (attributes.IsAttributeSet(ResponseAttribute.IS_CAPTURE))
            {
                builder.WithCapture(attributes.IsCapture);
            }
        }

        private static void ApplyCaptureCrashes(Builder builder, IResponseAttributes attributes)
        {
            if (attributes.IsAttributeSet(ResponseAttribute.IS_CAPTURE_CRASHES))
            {
                builder.WithCaptureCrashes(attributes.IsCaptureCrashes);
            }
        }

        private static void ApplyCaptureErrors(Builder builder, IResponseAttributes attributes)
        {
            if (attributes.IsAttributeSet(ResponseAttribute.IS_CAPTURE_ERRORS))
            {
                builder.WithCaptureErrors(attributes.IsCaptureErrors);
            }
        }

        private static void ApplyApplicationId(Builder builder, IResponseAttributes attributes)
        {
            if (attributes.IsAttributeSet(ResponseAttribute.APPLICATION_ID))
            {
                builder.WithApplicationId(attributes.ApplicationId);
            }
        }

        private static void ApplyMultiplicity(Builder builder, IResponseAttributes attributes)
        {
            if (attributes.IsAttributeSet(ResponseAttribute.MULTIPLICITY))
            {
                builder.WithMultiplicity(attributes.Multiplicity);
            }
        }

        private static void ApplyServerId(Builder builder, IResponseAttributes attributes)
        {
            if (attributes.IsAttributeSet(ResponseAttribute.SERVER_ID))
            {
                builder.WithServerId(attributes.ServerId);
            }
        }

        private static void ApplyStatus(Builder builder, IResponseAttributes attributes)
        {
            if (attributes.IsAttributeSet(ResponseAttribute.STATUS))
            {
                builder.WithStatus(attributes.Status);
            }
        }

        private static void ApplyTimestamp(Builder builder, IResponseAttributes attributes)
        {
            if (attributes.IsAttributeSet(ResponseAttribute.TIMESTAMP))
            {
                builder.WithTimestampInMilliseconds(attributes.TimestampInMilliseconds);
            }
        }

        #endregion
        #endregion

        public class Builder
        {
            internal readonly HashSet<ResponseAttribute> SetAttributes;

            public Builder(IResponseAttributes defaults)
            {
                SetAttributes = new HashSet<ResponseAttribute>();

                MaxBeaconSizeInBytes = defaults.MaxBeaconSizeInBytes;
                MaxSessionDurationInMilliseconds = defaults.MaxSessionDurationInMilliseconds;
                MaxEventsPerSession = defaults.MaxEventsPerSession;
                SessionTimeoutInMilliseconds = defaults.SessionTimeoutInMilliseconds;
                SendIntervalInMilliseconds = defaults.SendIntervalInMilliseconds;
                VisitStoreVersion = defaults.VisitStoreVersion;

                IsCapture = defaults.IsCapture;
                IsCaptureCrashes = defaults.IsCaptureCrashes;
                IsCaptureErrors = defaults.IsCaptureErrors;
                ApplicationId = defaults.ApplicationId;

                Multiplicity = defaults.Multiplicity;
                ServerId = defaults.ServerId;
                Status = defaults.Status;

                TimestampInMilliseconds = defaults.TimestampInMilliseconds;

                foreach (var attribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
                {
                    if (defaults.IsAttributeSet(attribute))
                    {
                        SetAttribute(attribute);
                    }
                }
            }

            internal int MaxBeaconSizeInBytes { get; private set; }

            /// <summary>
            /// Sets the maximum beacon size in bytes.
            /// </summary>
            /// <param name="maxBeaconSizeInBytes">the maximum size in bytes when sending beacon data.</param>
            /// <returns><code>this</code></returns>
            public Builder WithMaxBeaconSizeInBytes(int maxBeaconSizeInBytes)
            {
                MaxBeaconSizeInBytes = maxBeaconSizeInBytes;
                SetAttribute(ResponseAttribute.MAX_BEACON_SIZE);

                return this;
            }

            internal int MaxSessionDurationInMilliseconds { get; private set; }

            /// <summary>
            /// Sets the maximum duration after which a session is to be split.
            /// </summary>
            /// <param name="maxSessionDurationInMilliseconds">maximum duration of a session in milliseconds</param>
            /// <returns><code>this</code></returns>
            public Builder WithMaxSessionDurationInMilliseconds(int maxSessionDurationInMilliseconds)
            {
                MaxSessionDurationInMilliseconds = maxSessionDurationInMilliseconds;
                SetAttribute(ResponseAttribute.MAX_SESSION_DURATION);

                return this;
            }

            internal int MaxEventsPerSession { get; private set; }

            /// <summary>
            /// Sets the maximum number of top level actions after which a session is to be split.
            /// </summary>
            /// <param name="maxEventsPerSession">the maximum number of top level actions</param>
            /// <returns></returns>
            public Builder WithMaxEventsPerSession(int maxEventsPerSession)
            {
                MaxEventsPerSession = maxEventsPerSession;
                SetAttribute(ResponseAttribute.MAX_EVENTS_PER_SESSION);

                return this;
            }

            internal int SessionTimeoutInMilliseconds { get; private set; }

            /// <summary>
            /// Sets the idle timeout after which a session is to be split.
            /// </summary>
            /// <param name="sessionTimeoutInMilliseconds">the maximum idle timeout of a session in milliseconds</param>
            /// <returns><code>this</code></returns>
            public Builder WithSessionTimeoutInMilliseconds(int sessionTimeoutInMilliseconds)
            {
                SessionTimeoutInMilliseconds = sessionTimeoutInMilliseconds;
                SetAttribute(ResponseAttribute.SESSION_IDLE_TIMEOUT);

                return this;
            }

            internal int SendIntervalInMilliseconds { get; private set; }

            /// <summary>
            /// Sets the send interval in milliseconds at which beacon data is sent to the server.
            /// </summary>
            /// <param name="sendIntervalInMilliseconds"> the send interval in milliseconds</param>
            /// <returns><code>this</code></returns>
            public Builder WithSendIntervalInMilliseconds(int sendIntervalInMilliseconds)
            {
                SendIntervalInMilliseconds = sendIntervalInMilliseconds;
                SetAttribute(ResponseAttribute.SEND_INTERVAL);

                return this;
            }

            internal int VisitStoreVersion { get; private set; }

            /// <summary>
            /// Sets the version of the visit store that should be used.
            /// </summary>
            /// <param name="visitStoreVersion">version of the visit store</param>
            /// <returns><code>this</code></returns>
            public Builder WithVisitStoreVersion(int visitStoreVersion)
            {
                VisitStoreVersion = visitStoreVersion;
                SetAttribute(ResponseAttribute.VISIT_STORE_VERSION);

                return this;
            }

            internal bool IsCapture { get; private set; }

            /// <summary>
            /// Sets whether capturing is enabled/disabled.
            /// </summary>
            /// <param name="capture">capture state</param>
            /// <returns><code>this</code></returns>
            public Builder WithCapture(bool capture)
            {
                IsCapture = capture;
                SetAttribute(ResponseAttribute.IS_CAPTURE);

                return this;
            }

            internal bool IsCaptureCrashes { get; private set; }

            /// <summary>
            /// Sets whether capturing of crashes is enabled/disabled
            /// </summary>
            /// <param name="captureCrashes">crash capture state</param>
            /// <returns></returns>
            public Builder WithCaptureCrashes(bool captureCrashes)
            {
                IsCaptureCrashes = captureCrashes;
                SetAttribute(ResponseAttribute.IS_CAPTURE_CRASHES);

                return this;
            }

            internal bool IsCaptureErrors { get; private set; }

            /// <summary>
            /// Sets whether capturing of errors is enabled/disabled
            /// </summary>
            /// <param name="captureErrors">error capture state</param>
            /// <returns><code>this</code></returns>
            public Builder WithCaptureErrors(bool captureErrors)
            {
                IsCaptureErrors = captureErrors;
                SetAttribute(ResponseAttribute.IS_CAPTURE_ERRORS);

                return this;
            }

            internal string ApplicationId { get; private set; }

            /// <summary>
            /// Set application UUID to which this configuration belongs to.
            /// </summary>
            /// <param name="applicationId">application's UUID</param>
            /// <returns><code>this</code></returns>
            public Builder WithApplicationId(string applicationId)
            {
                ApplicationId = applicationId;
                SetAttribute(ResponseAttribute.APPLICATION_ID);

                return this;
            }

            internal int Multiplicity { get; private set; }

            /// <summary>
            /// Sets the multiplicity
            /// </summary>
            /// <param name="multiplicity">the multiplicity to set</param>
            /// <returns><code>this</code></returns>
            public Builder WithMultiplicity(int multiplicity)
            {
                Multiplicity = multiplicity;
                SetAttribute(ResponseAttribute.MULTIPLICITY);

                return this;
            }

            internal int ServerId { get; private set; }

            /// <summary>
            /// Sets the ID of the server to which data should be sent to.
            /// </summary>
            /// <param name="serverId">the ID of the server to communicate with</param>
            /// <returns><code>this</code></returns>
            public Builder WithServerId(int serverId)
            {
                ServerId = serverId;
                SetAttribute(ResponseAttribute.SERVER_ID);

                return this;
            }

            internal string Status { get; private set; }

            /// <summary>
            /// Sets the response status received for a new session request.
            /// </summary>
            /// <param name="status">the status received for new session request</param>
            /// <returns><code>this</code></returns>
            public Builder WithStatus(string status)
            {
                Status = status;
                SetAttribute(ResponseAttribute.STATUS);

                return this;
            }

            internal long TimestampInMilliseconds { get; private set; }

            /// <summary>
            /// Sets the timestamp of the configuration sent by the server.
            /// </summary>
            /// <param name="timestampInMilliseconds">the timestamp of the configuration in milliseconds</param>
            /// <returns><code>this</code></returns>
            public Builder WithTimestampInMilliseconds(long timestampInMilliseconds)
            {
                TimestampInMilliseconds = timestampInMilliseconds;
                SetAttribute(ResponseAttribute.TIMESTAMP);

                return this;
            }

            /// <summary>
            /// Creates a new <see cref="IResponseAttributes"/> instance with all the attributes set in this builder.
            /// </summary>
            public IResponseAttributes Build()
            {
                return new ResponseAttributes(this);
            }

            private void SetAttribute(ResponseAttribute attribute)
            {
                SetAttributes.Add(attribute);
            }
        }
    }
}