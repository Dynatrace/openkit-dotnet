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

namespace Dynatrace.OpenKit.Protocol
{
    /// <summary>
    /// Defines the attributes received as response from the server.
    /// </summary>
    public interface IResponseAttributes
    {
        /// <summary>
        /// Returns the maximum POST body size when sending beacons.
        /// </summary>
        int MaxBeaconSizeInBytes { get; }

        /// <summary>
        /// Returns the maximum duration of a session in milliseconds after which a session will be split.
        /// </summary>
        int MaxSessionDurationInMilliseconds { get; }

        /// <summary>
        /// Returns the maximum number of top level actions after which a session will be split.
        /// </summary>
        int MaxEventsPerSession { get; }

        /// <summary>
        /// Returns the idle timeout in milliseconds after which a session will be split.
        /// </summary>
        int SessionTimeoutInMilliseconds { get; }

        /// <summary>
        /// Returns the send interval in milliseconds.
        /// </summary>
        int SendIntervalInMilliseconds { get; }

        /// <summary>
        /// Returns the version of the visit store to be used.
        /// </summary>
        int VisitStoreVersion { get; }

        /// <summary>
        /// Indicator whether capturing data is generally allowed or not.
        /// </summary>
        bool IsCapture { get; }

        /// <summary>
        /// Indicator whether crashes should be captured or not.
        /// </summary>
        bool IsCaptureCrashes { get; }

        /// <summary>
        /// Indicator whether errors should be captured or not.
        /// </summary>
        bool IsCaptureErrors { get; }

        /// <summary>
        /// Returns the multiplicity
        /// </summary>
        int Multiplicity { get; }

        /// <summary>
        /// Returns the ID of the server to where all data should be sent.
        /// </summary>
        int ServerId { get; }

        /// <summary>
        /// Returns the timestamp of the attributes which were returned from the server.
        /// <para>
        ///     The timestamp is the duration from January, 1st
        /// </para>
        /// </summary>
        long TimestampInMilliseconds { get; }

        /// <summary>
        /// Checks whether the given attribute was set / sent by the server.
        /// </summary>
        /// <param name="attribute">the attribute to be checked if it was sent by the server</param>
        /// <returns><code>true</code> if the given attribute was sent from the server with this attributes, <code>false</code> otherwise.</returns>
        bool IsAttributeSet(ResponseAttribute attribute);

        /// <summary>
        /// Creates a new response attributes object by merging the given attributes into this one. Single attributes
        /// are selectively taken over from the given response as long as the respective attribute has
        /// <see cref="IsAttributeSet"/> return <code>true</code>.
        /// </summary>
        /// <param name="responseAttributes">the response attributes which will be merged together with this one into a
        /// new response attributes object.</param>
        /// <returns>a new response attributes instance as merge of this and the given object.</returns>
        IResponseAttributes Merge(IResponseAttributes responseAttributes);
    }
}