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
    /// Specifies a callback which will be invoked when the server configuration is updated.
    /// </summary>
    /// <param name="serverConfig">the updated server configuration.</param>
    public delegate void ServerConfigurationUpdateCallback(IServerConfiguration serverConfig);

    /// <summary>
    /// This interface holds the relevant configuration for the <see cref="IBeacon"/>.
    /// </summary>
    internal interface IBeaconConfiguration
    {
        /// <summary>
        /// OpenKit related configuration details.
        /// </summary>
        IOpenKitConfiguration OpenKitConfiguration { get; }

        /// <summary>
        /// Privacy related configuration details.
        /// </summary>
        IPrivacyConfiguration PrivacyConfiguration { get; }

        /// <summary>
        /// Returns configuration for HTTP related details.
        /// </summary>
        IHttpClientConfiguration HttpClientConfiguration { get; }

        /// <summary>
        /// Returns the configuration about server related details.
        /// </summary>
        IServerConfiguration ServerConfiguration { get; }

        /// <summary>
        ///  Initializes this beacon configuration with the given server configuration. This will not set
        /// <see cref="IsServerConfigurationSet"/> to <code>true</code> so that new session requests to the server will
        /// still be done. In case the <see cref="IsServerConfigurationSet"/> was already set, this method does nothing.
        /// </summary>
        /// <param name="initialServerConfiguration">the server configuration to initialize this beacon configuration with.</param>
        void InitializeServerConfiguration(IServerConfiguration initialServerConfiguration);

        /// <summary>
        /// Updates the server configuration.
        ///
        /// <para>
        ///     The first call to this method will take over the configuration as is. Further calls to this method
        ///     will <see cref="IServerConfiguration.Merge">merge</see> the given configuration with the already
        ///     existing one.
        /// </para>
        /// </summary>
        /// <param name="newServerConfiguration">new server configuration as received from the server.</param>
        void UpdateServerConfiguration(IServerConfiguration newServerConfiguration);

        /// <summary>
        /// Enables capturing and sets <see cref="IsServerConfigurationSet"/>.
        /// </summary>
        void EnableCapture();

        /// <summary>
        /// Disables capturing and sets <see cref="IsServerConfigurationSet"/>.
        /// </summary>
        void DisableCapture();

        /// <summary>
        /// Indicates whether the server configuration has been set before or not.
        /// </summary>
        bool IsServerConfigurationSet { get; }

        /// <summary>
        /// Represents an event which gets fired when the server configuration is updated.
        /// </summary>
        event ServerConfigurationUpdateCallback OnServerConfigurationUpdate;
    }
}