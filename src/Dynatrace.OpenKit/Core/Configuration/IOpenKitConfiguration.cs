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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.API.HTTP;

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
        /// Returns the endpoint URL to which beacon data will be sent.
        /// </summary>
        string EndpointUrl { get; }

        /// <summary>
        /// Returns the identifier of the device for which this configuration is used.
        /// </summary>
        long DeviceId { get; }

        /// <summary>
        /// Returns the original (not hashed) <see cref="DeviceId">device identifier</see>
        /// </summary>
        string OrigDeviceId { get; }

        /// <summary>
        /// Returns the type of this configuration
        /// </summary>
        string OpenKitType { get; }

        /// <summary>
        /// Returns the identifier of the application for which this configuration is used.
        /// </summary>
        string ApplicationId { get; }

        /// <summary>
        /// Returns the <see cref="ApplicationId">identifier of the application </see> in a percent encoded representation.
        /// </summary>
        string ApplicationIdPercentEncoded { get; }

        /// <summary>
        /// Returns the name of the application for which this configuration is used.
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// Returns the version of the application for which this configuration is used.
        /// </summary>
        string ApplicationVersion { get; }

        /// <summary>
        /// Returns the device's operating system.
        /// </summary>
        string OperatingSystem { get; }

        /// <summary>
        /// Returns the device's manufacturer.
        /// </summary>
        string Manufacturer { get; }

        /// <summary>
        /// Returns the device's model ID.
        /// </summary>
        string ModelId { get; }

        /// <summary>
        /// Returns the default identifier of the Dynatrace/AppMon server to communicate with.
        /// </summary>
        int DefaultServerId { get; }

        /// <summary>
        /// Returns the <see cref="ISSLTrustManager"/>
        /// </summary>
        ISSLTrustManager TrustManager { get; }

        /// <summary>
        /// Returns the <see cref="IHttpRequestInterceptor"/>
        /// </summary>
        IHttpRequestInterceptor HttpRequestInterceptor { get; }

        /// <summary>
        /// Returns the <see cref="IHttpResponseInterceptor"/>
        /// </summary>
        IHttpResponseInterceptor HttpResponseInterceptor { get; }
    }
}