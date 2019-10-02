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

using Dynatrace.OpenKit.Core.Objects;
using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core.Configuration
{
    /// <summary>
    /// This interface represents a <see cref="OpenKitConfiguration"/> which is internally used.
    ///
    /// <para>
    /// The main purpose of this interface is to make components which require a <see cref="OpenKitConfiguration"/> to
    /// be more easily testable.
    /// </para>
    /// </summary>
    internal interface IOpenKitConfiguration
    {
        /// <summary>
        /// Returns the next session number.
        /// </summary>
        int NextSessionNumber { get; }

        /// <summary>
        /// Returns the <see cref="Dynatrace.OpenKit.Core.Configuration.OpenKitType"/> of this configuration
        /// </summary>
        OpenKitType OpenKitType { get; }

        /// <summary>
        /// Returns the name of the application for which this configuration is used.
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// Returns the identifier of the application for which this configuration is used.
        /// </summary>
        string ApplicationId { get; }

        /// <summary>
        /// Returns the <see cref="ApplicationId">identifier of the application </see> in a percent encoded representation.
        /// </summary>
        string ApplicationIdPercentEncoded { get; }

        /// <summary>
        /// Returns the version of the application for which this configuration is used.
        /// </summary>
        string ApplicationVersion { get; set; }

        /// <summary>
        /// Returns the identifier of the device for which this configuration is used.
        /// </summary>
        long DeviceId { get; }

        /// <summary>
        /// Returns information about the device for which this configuration is used.
        /// </summary>
        Device Device { get; }

        /// <summary>
        /// Returns the original (not hashed) <see cref="DeviceId">device identifier</see>
        /// </summary>
        string OrigDeviceId { get; }

        /// <summary>
        /// Returns the endpoint URL to which beacon data will be sent.
        /// </summary>
        string EndpointUrl { get; }

        /// <summary>
        /// Indicates whether capturing is currently enabled or not.
        /// </summary>
        bool IsCaptureOn { get; }

        /// <summary>
        /// Returns the currently configured interval at which beacon data will be sent.
        /// </summary>
        int SendInterval { get; }

        /// <summary>
        /// Returns the currently configured maximum size (in bytes) which a beacon can have.
        /// </summary>
        int MaxBeaconSize { get; }

        /// <summary>
        /// Indicates if errors are currently captured or not.
        /// </summary>
        bool CaptureErrors { get; }

        /// <summary>
        /// Indicates if crashes are currently captured or not.
        /// </summary>
        bool CaptureCrashes { get; }

        #region Extended configuration

        /// <summary>
        /// Returns the <see cref="IBeaconConfiguration">configuration</see> of the beacon cache.
        /// </summary>
        IBeaconCacheConfiguration BeaconCacheConfig { get; }

        /// <summary>
        /// Returns the <see cref="IPrivacyConfiguration">privacy configuration</see> used to determine which data is
        /// allowed to be sent.
        /// </summary>
        IPrivacyConfiguration PrivacyConfig { get; }

        /// <summary>
        /// Returns the <see cref="IHttpClientConfiguration">configuration</see> for communication via HTTP.
        /// </summary>
        IHttpClientConfiguration HttpClientConfig { get; }

        /// <summary>
        /// Returns the configuration of the <see cref="IBeacon"/>.
        /// </summary>
        IBeaconConfiguration BeaconConfig { get; }

        #endregion

        /// <summary>
        /// Updates the settings of this configuration based on the given status response.
        /// </summary>
        /// <param name="statusResponse">the status response from which to update this settings.</param>
        void UpdateSettings(StatusResponse statusResponse);

        /// <summary>
        /// Enables capturing
        /// </summary>
        void EnableCapture();

        /// <summary>
        /// Disables capturing
        /// </summary>
        void DisableCapture();
    }
}