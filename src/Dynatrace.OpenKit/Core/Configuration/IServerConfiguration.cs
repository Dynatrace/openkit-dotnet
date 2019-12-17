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

namespace Dynatrace.OpenKit.Core.Configuration
{
    public interface IServerConfiguration
    {
        /// <summary>
        /// Indicates whether capturing is enabled in Dynatrace/AppMon or not.
        /// </summary>
        /// <returns><code>true</code> if capturing is enabled, <code>false</code> otherwise.</returns>
        bool IsCaptureEnabled { get; }

        /// <summary>
        /// Indicates whether crash reporting is enabled by Dynatrace/AppMon or not.
        /// </summary>
        /// <returns><code>true</code> if crash reporting is enabled, <code>false</code> otherwise.</returns>
        bool IsCrashReportingEnabled { get; }

        /// <summary>
        /// Indicates whether error reporting is enabled in Dynatrace/AppMon or not.
        /// </summary>
        /// <returns><code>true</code> if error reporting is enabled, <code>false</code> otherwise.</returns>
        bool IsErrorReportingEnabled { get; }

        /// <summary>
        /// Returns the send interval in milliseconds.
        /// </summary>
        int SendIntervalInMilliseconds { get; }

        /// <summary>
        /// Returns the ID of the server which is communicated with.
        /// </summary>
        int ServerId { get; }

        /// <summary>
        /// Returns the maximum beacon size, that is the post body, in bytes.
        /// </summary>
        int BeaconSizeInBytes { get; }

        /// <summary>
        /// Returns the multiplicity value.
        ///
        /// <para>
        ///     Multiplicity is a factor on the server side, which is greater than 1 to prevent overload situations.
        ///     This value comes from the server and has to be sent back to the server.
        /// </para>
        /// </summary>
        int Multiplicity { get; }

        /// <summary>
        /// Returns the maximum duration in milliseconds after which a session is to be split.
        /// </summary>
        int MaxSessionDurationInMilliseconds { get; }

        /// <summary>
        /// Returns the maximum number of top level actions after which a session is to be split.
        /// </summary>
        int MaxEventsPerSession { get; }

        /// <summary>
        /// Returns <code>true</code> if session splitting when exceeding the maximum number of top level events is
        /// enabled, <code>false</code> otherwise.
        /// </summary>
        bool IsSessionSplitByEventsEnabled { get; }

        /// <summary>
        /// Returns the idle timeout after which a session is to be split.
        /// </summary>
        int SessionTimeoutInMilliseconds { get; }

        /// <summary>
        /// Returns the version of the visit store that is being used.
        /// </summary>
        int VisitStoreVersion { get; }

        /// <summary>
        /// Indicates whether sending arbitrary data to the server is allowed or not.
        ///
        /// <para>
        ///     Sending data is only allowed if all of the following conditions evaluate to true.
        ///     <list type="bullet">
        ///         <item><see cref="IsCaptureEnabled"/> is <code>true</code></item>
        ///         <item><see cref="Multiplicity"/> is greater than <code>0</code></item>
        ///     </list>
        ///
        ///     To check if sending errors is allowed, use <see cref="IsSendingErrorsAllowed"/>
        ///     To check if sending crashes is allowed, use <see cref="IsSendingCrashesAllowed"/>
        /// </para>
        /// </summary>
        /// <returns><code>true</code> if data sending is allowed, <code>false</code> otherwise.</returns>
        bool IsSendingDataAllowed { get; }

        /// <summary>
        /// Indicates whether sending crashes to the server is allowed or not.
        ///
        /// <para>
        ///     Sending crashes is only allowed if all of the following conditions evaluate to true.
        ///     <list type="bullet">
        ///         <item><see cref="IsSendingDataAllowed"/> is <code>true</code></item>
        ///         <item><see cref="IsCrashReportingEnabled"/> is <code>true</code></item>
        ///     </list>
        /// </para>
        /// </summary>
        /// <returns><code>true</code> if sending crashes is allowed, <code>false</code> otherwise.</returns>
        bool IsSendingCrashesAllowed { get; }

        /// <summary>
        /// Indicates whether sending errors to the server is allowed or not.
        ///
        /// <para>
        ///     Sending errors is only allowed if all of the following conditions evaluate to true.
        ///     <list type="bullet">
        ///     <item><see cref="IsSendingDataAllowed"/> is <code>true</code></item>
        ///     <item><see cref="IsErrorReportingEnabled"/> is <code>true</code></item>
        ///     </list>
        /// </para>
        /// </summary>
        bool IsSendingErrorsAllowed { get; }

        /// <summary>
        /// Merges the given <paramref name="other">server configuration</paramref> with <code>this</code> server
        /// configuration instance and returns the newly created merged instance.
        ///
        /// <para>
        ///     Most fields are taken from the <paramref name="other">other server configuration</paramref> except for
        ///     <see cref="Multiplicity"/> and <see cref="ServerId"/>
        /// </para>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        IServerConfiguration Merge(IServerConfiguration other);
    }
}