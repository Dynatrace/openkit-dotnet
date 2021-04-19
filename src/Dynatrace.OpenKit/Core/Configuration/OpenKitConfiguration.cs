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

using System.Text;
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.API.HTTP;
using Dynatrace.OpenKit.Core.Util;

namespace Dynatrace.OpenKit.Core.Configuration
{

    /// <summary>
    ///  The OpenKitConfiguration class holds all configuration settings, both provided by the user and the Dynatrace/AppMon server.
    /// </summary>
    public class OpenKitConfiguration : IOpenKitConfiguration
    {
        /// <summary>
        /// Character set used to encode application IDs
        /// </summary>
        private static readonly Encoding EncodingCharset = Encoding.UTF8;
        /// <summary>
        /// Reserved characters which also need encoding (underscore is a reserved character at server side).
        /// </summary>
        private static readonly char[] ReservedCharacters = {'_'};

        #region constructors

        private OpenKitConfiguration(IOpenKitBuilder builder)
        {
            EndpointUrl = builder.EndpointUrl;
            DeviceId = builder.DeviceId;
            OrigDeviceId = builder.OrigDeviceId;
            OpenKitType = builder.OpenKitType;
            ApplicationId = builder.ApplicationId;
            ApplicationIdPercentEncoded = PercentEncoder.Encode(ApplicationId, EncodingCharset, ReservedCharacters);
            ApplicationName = builder.ApplicationName;
            ApplicationVersion = builder.ApplicationVersion;
            OperatingSystem = builder.OperatingSystem;
            Manufacturer = builder.Manufacturer;
            ModelId = builder.ModelId;
            DefaultServerId = builder.DefaultServerId;
            TrustManager = builder.TrustManager;
            HttpRequestInterceptor = builder.HttpRequestInterceptor;
            HttpResponseInterceptor = builder.HttpResponseInterceptor;
        }

        /// <summary>
        /// Creates a <see cref="IOpenKitConfiguration"/> from the given <paramref name="builder"/>
        /// </summary>
        /// <param name="builder">the OpenKit builder for which to create a <see cref="IOpenKitConfiguration"/></param>.
        /// <returns>
        ///     A newly created <see cref="IOpenKitConfiguration"/> or <code>null</code> if the given
        ///     <paramref name="builder">argument</paramref> is <code>null</code>.
        /// </returns>
        internal static IOpenKitConfiguration From(IOpenKitBuilder builder)
        {
            if (builder == null)
            {
                return null;
            }

            return new OpenKitConfiguration(builder);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Returns the beacon endpoint communication URL.
        /// </summary>
        public string EndpointUrl { get; }

        /// <summary>
        /// Returns the unique device identifier.
        /// </summary>
        public long DeviceId { get; }

        /// <summary>
        /// Returns the device identifier before it was hashed to a 64 bit long value (in case the initial value passed
        /// to the OpenKit builder was not numerical).
        /// </summary>
        public string OrigDeviceId { get; }

        /// <summary>
        /// Returns the OpenKit type.
        /// </summary>
        public string OpenKitType { get; }

        /// <summary>
        /// Returns the identifier of the application.
        /// </summary>
        public string ApplicationId { get; }

        /// <summary>
        /// Returns the percent encoded representation of the application's identifier.
        /// </summary>
        public string ApplicationIdPercentEncoded { get; }

        /// <summary>
        /// Returns the name of the application.
        /// </summary>
        public string ApplicationName { get; }

        /// <summary>
        /// Returns the application's version.
        /// </summary>
        public string ApplicationVersion { get; }

        /// <summary>
        /// Returns the device's operating system.
        /// </summary>
        public string OperatingSystem { get; }

        /// <summary>
        /// Returns the device's manufacturer.
        /// </summary>
        public string Manufacturer { get; }

        /// <summary>
        /// Returns the device's model ID.
        /// </summary>
        public string ModelId { get; }

        /// <summary>
        /// Returns the default identifier of the Dynatrace/AppMon server to communicate with.
        /// </summary>
        public int DefaultServerId { get; }

        /// <summary>
        /// Returns the <see cref="ISSLTrustManager"/>
        /// </summary>
        public ISSLTrustManager TrustManager { get; }
        
        /// <summary>
        /// Returns the <see cref="IHttpRequestInterceptor"/>
        /// </summary>
        public IHttpRequestInterceptor HttpRequestInterceptor { get; }

        /// <summary>
        /// Returns the <see cref="IHttpResponseInterceptor"/>
        /// </summary>
        public IHttpResponseInterceptor HttpResponseInterceptor { get; }

        #endregion
    }
}
