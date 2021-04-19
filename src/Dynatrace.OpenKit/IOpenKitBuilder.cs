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

namespace Dynatrace.OpenKit
{
    public interface IOpenKitBuilder
    {
        /// <summary>
        /// Returns the string identifying the OpenKit tpe that gets created by the builder.
        ///
        /// <para>
        ///     The only real purpose is for logging reasons.
        /// </para>
        /// </summary>
        /// <returns>some string identifying the OpenKit's type.</returns>
        string OpenKitType { get; }

        /// <summary>
        /// Returns the application identifier for which the OpenKit reports data.
        /// </summary>
        string ApplicationId { get; }

        /// <summary>
        /// Returns the application's name
        ///
        /// <para>
        ///     It depends on the concrete builder whether the application name is configurable or not.
        ///     In any case, the derived classes have to return a string that is neither <code>null</code> nor empty.
        /// </para>
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// Returns the ID of the server to communicate with.
        ///
        /// <para>
        ///     This might be changed based on the OpenKit type.
        /// </para>
        /// </summary>
        int DefaultServerId { get; }

        /// <summary>
        /// Returns the application version that has been set with <see cref="AbstractOpenKitBuilder.WithApplicationVersion(string)"/>.
        ///
        /// <para>
        ///     If no version was set, the <see cref="OpenKitConstants.DefaultApplicationVersion">default version</see>
        ///     is returned.
        /// </para>
        /// </summary>
        string ApplicationVersion { get; }

        /// <summary>
        /// Returns the operating system that has been set with <see cref="AbstractOpenKitBuilder.WithOperatingSystem(string)"/>.
        ///
        /// <para>
        ///     If no operating system was set, the <see cref="OpenKitConstants.DefaultOperatingSystem">default
        ///     operating system</see> is returned.
        /// </para>
        /// </summary>
        string OperatingSystem { get; }

        /// <summary>
        /// Returns the manufacturer that has been set with <see cref="AbstractOpenKitBuilder.WithManufacturer(string)"/>.
        ///
        /// <para>
        ///     If no manufacturer was set, the <see cref="OpenKitConstants.DefaultManufacturer">default manufacturer</see>
        ///     is returned.
        /// </para>
        /// </summary>
        string Manufacturer { get; }

        /// <summary>
        /// Returns the model identifier that has been set with <see cref="AbstractOpenKitBuilder.WithModelId(string)"/>.
        ///
        /// <para>
        ///     If no model ID was set, the <see cref="OpenKitConstants.DefaultModelId">default model ID</see> is
        ///     returned.
        /// </para>
        /// </summary>
        string ModelId { get; }

        /// <summary>
        /// Returns the endpoint ULR that has ben set in the constructor.
        ///
        /// <para>
        ///     The endpoint ULR is where the beacon data is sent to.
        /// </para>
        /// </summary>
        string EndpointUrl { get; }

        /// <summary>
        /// Returns the device identifier that has been set in the constructor.
        ///
        /// <para>
        ///     The device identifier is a unique numeric value that identifies the device or the installation.
        ///     The user of the OpenKit library is responsible of providing a unique value which stays consistent
        ///     per device/installation.
        /// </para>
        /// </summary>
        long DeviceId { get; }

        /// <summary>
        /// Returns the <see cref="DeviceId"/> as it was passed (not <see cref="StringUtil.To64BitHash">hashed</see>) to
        /// the constructor.
        /// </summary>
        string OrigDeviceId { get; }

        /// <summary>
        /// Returns the SSL trust manager that has been set with <see cref="AbstractOpenKitBuilder.WithTrustManager(ISSLTrustManager)"/>.
        ///
        /// <para>
        ///     The <see cref="ISSLTrustManager"/> implementation is responsible for checking the X509 certificate chain
        ///     and also to reject untrusted/invalid certificates.
        ///     The default implementation rejects every untrusted/invalid (including self-signed) certificate.
        /// </para>
        /// </summary>
        ISSLTrustManager TrustManager { get; }

        /// <summary>
        /// Returns the maximum beacon cache record age that has been set with
        /// <see cref="AbstractOpenKitBuilder.WithBeaconCacheMaxRecordAge(long)"/>.
        ///
        /// <para>
        ///     Is no max age was set, the <see cref="BeaconCacheConfiguration.DefaultMaxRecordAgeInMillis">default max
        ///     age</see> is returned.
        /// </para>
        /// </summary>
        long BeaconCacheMaxBeaconAge { get; }

        /// <summary>
        /// Returns the beacon cache's lower memory boundary that has been set with
        /// <see cref="AbstractOpenKitBuilder.WithBeaconCacheLowerMemoryBoundary(long)"/>.
        ///
        /// <para>
        ///     If no lower memory boundary was set, the
        ///     <see cref="BeaconCacheConfiguration.DefaultLowerMemoryBoundaryInBytes">default lower boundary</see> is
        ///     returned.
        /// </para>
        /// </summary>
        long BeaconCacheLowerMemoryBoundary { get; }

        /// <summary>
        /// Returns the beacon cache's upper memory boundary that has been set with
        /// <see cref="AbstractOpenKitBuilder.WithBeaconCacheUpperMemoryBoundary(long)"/>.
        ///
        /// <para>
        ///     If no upper memory boundary was set, the
        ///     <see cref="BeaconCacheConfiguration.DefaultUpperMemoryBoundaryInBytes">default upper boundary</see> is
        ///     returned.
        /// </para>
        /// </summary>
        long BeaconCacheUpperMemoryBoundary { get; }

        /// <summary>
        /// Returns the data collection level that has been set with 
        /// <see cref="AbstractOpenKitBuilder.WithDataCollectionLevel(DataCollectionLevel)"/>.
        ///
        /// <para>
        ///     If no data collection level was set, the <see cref="PrivacyConfiguration.DefaultDataCollectionLevel"/>
        ///     is returned.
        /// </para>
        /// </summary>
        DataCollectionLevel DataCollectionLevel { get; }

        /// <summary>
        /// Returns the crash reporting level that has been set with 
        /// <see cref="AbstractOpenKitBuilder.WithCrashReportingLevel(CrashReportingLevel)"/>.
        ///
        /// <para>
        ///     If no crash reporting level was set, the <see cref="PrivacyConfiguration.DefaultCrashReportingLevel"/>
        ///     is returned.
        /// </para>
        /// </summary>
        CrashReportingLevel CrashReportingLevel { get; }

        /// <summary>
        /// Returns the log level that has been set with <see cref="AbstractOpenKitBuilder.WithLogLevel(LogLevel)"/>.
        ///
        /// <para>
        ///     If no log level was set, the <see cref="Dynatrace.OpenKit.API.LogLevel.WARN">default log level</see>
        ///     is returned.
        /// </para>
        /// </summary>
        LogLevel LogLevel { get; }

        /// <summary>
        /// Returns the <see cref="ILogger"/> that has been set with <see cref="AbstractOpenKitBuilder.WithLogger(ILogger)"/>.
        ///
        /// <para>
        ///     If no logger was set, a <see cref="DefaultLogger">default logger</see> instance is returned.
        /// </para>
        /// </summary>
        ILogger Logger { get; }


        /// <summary>
        /// Returns the <see cref="IHttpRequestInterceptor"/> that has been set with
        /// <see cref="AbstractOpenKitBuilder.WithHttpRequestInterceptor(IHttpRequestInterceptor)"/>
        /// 
        /// <para>
        ///     If no request interceptor was set, a <see cref="NullHttpRequestInterceptor">default request interceptor</see>
        ///     is returned.
        /// </para>
        /// </summary>
        IHttpRequestInterceptor HttpRequestInterceptor { get; }

        /// <summary>
        /// Returns the <see cref="IHttpResponseInterceptor"/>
        /// 
        /// <para>
        ///     If no response interceptor was set, a <see cref="NullHttpResponseInterceptor">default response interceptor</see>
        ///     is returned.
        /// </para>
        /// </summary>
        IHttpResponseInterceptor HttpResponseInterceptor { get; }
    }
}