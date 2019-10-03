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
    internal class BeaconConfiguration : IBeaconConfiguration
    {
        /// <summary>
        /// Configuration holding server related details.
        /// </summary>
        private IServerConfiguration serverConfiguration;

        /// <summary>
        /// Object for synchronization.
        /// </summary>
        private readonly  object lockObject = new object();

        private BeaconConfiguration(
            IOpenKitConfiguration openKitConfiguration,
            IPrivacyConfiguration privacyConfiguration,
            int serverId)
        {
            OpenKitConfiguration = openKitConfiguration;
            PrivacyConfiguration = privacyConfiguration;
            HttpClientConfiguration = Configuration.HttpClientConfiguration.ModifyWith(openKitConfiguration)
                .WithServerId(serverId)
                .Build();
            serverConfiguration = null;
        }

        /// <summary>
        /// Creates a <see cref="IBeaconConfiguration"/> from the given <see cref="IOpenKitConfiguration"/> and
        /// <see cref="IPrivacyConfiguration"/>.
        /// </summary>
        /// <param name="openKitConfiguration">OpenKit configuration.</param>
        /// <param name="privacyConfiguration">privacy related settings.</param>
        /// <param name="serverId">identifier of the server to communicate with.</param>
        /// <returns></returns>
        public static IBeaconConfiguration From(
            IOpenKitConfiguration openKitConfiguration,
            IPrivacyConfiguration privacyConfiguration,
            int serverId)
        {
            if (openKitConfiguration == null || privacyConfiguration == null)
            {
                return null;
            }
            return new BeaconConfiguration(openKitConfiguration, privacyConfiguration, serverId);
        }

        /// <summary>
        /// OpenKit related configuration that has been configured in the <see cref="IOpenKitBuilder"/>.
        /// </summary>
        public IOpenKitConfiguration OpenKitConfiguration { get; }

        /// <summary>
        /// Privacy configuration that has been configured in the <see cref="IOpenKitBuilder"/>.
        /// </summary>
        public IPrivacyConfiguration PrivacyConfiguration { get; }

        /// <summary>
        /// Returns configuration for HTTP related data.
        /// </summary>
        public IHttpClientConfiguration HttpClientConfiguration { get; }

        /// <summary>
        /// Returns the sever configuration that was set before.
        ///
        /// <para>
        /// If no server configuration was set the
        /// <see cref="Dynatrace.OpenKit.Core.Configuration.ServerConfiguration.Default">default configuration</see> is
        /// returned.
        /// </para>
        /// </summary>
        public IServerConfiguration ServerConfiguration
        {
            get
            {
                lock (lockObject)
                {
                    return serverConfiguration ?? Configuration.ServerConfiguration.Default;
                }
            }
        }

        void IBeaconConfiguration.UpdateServerConfiguration(IServerConfiguration newServerConfiguration)
        {
            if (newServerConfiguration == null)
            {
                return;
            }

            lock (lockObject)
            {
                if (serverConfiguration == null)
                {
                    // no server configuration was set so far so just take the new one.
                    serverConfiguration = newServerConfiguration;
                }
                else
                {
                    serverConfiguration = serverConfiguration.Merge(newServerConfiguration);
                }
            }
        }

        /// <summary>
        /// Enables capturing and implicitly sets <see cref="IsServerConfigurationSet"/>.
        /// </summary>
        public void EnableCapture()
        {
            UpdateCaptureWith(true);
        }

        /// <summary>
        /// Disables capturing and implicitly sets <see cref="IsServerConfigurationSet"/>.
        /// </summary>
        public void DisableCapture()
        {
            UpdateCaptureWith(false);
        }

        /// <summary>
        /// Enables/disables capture according to the given <paramref name="captureState">state</paramref>.
        /// </summary>
        /// <param name="captureState">the state to which capture will be set.</param>
        private void UpdateCaptureWith(bool captureState)
        {
            lock (lockObject)
            {
                var currentServerConfig = ServerConfiguration;
                serverConfiguration = new ServerConfiguration.Builder(currentServerConfig)
                    .WithCapture(captureState)
                    .Build();
            }
        }

        /// <summary>
        /// Indicates whether the server configuration has been set before or not.
        /// </summary>
        public bool IsServerConfigurationSet
        {
            get
            {
                lock (lockObject)
                {
                    return serverConfiguration != null;
                }
            }
        }
    }
}
