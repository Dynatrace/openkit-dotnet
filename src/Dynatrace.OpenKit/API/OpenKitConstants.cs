//
// Copyright 2018-2020 Dynatrace LLC
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

using System.Reflection;
using Dynatrace.OpenKit.Util;

namespace Dynatrace.OpenKit.API
{
    /// <summary>
    /// Defines constant values used in OpenKit
    /// </summary>
    public static class OpenKitConstants
    {
        // default values used in configuration
        public static readonly string DefaultApplicationVersion = "<unknown version>";
        public static readonly string DefaultOperatingSystem = "OpenKit " + DefaultApplicationVersion;
        public static readonly string DefaultManufacturer = "Dynatrace";
        public const string DefaultModelId = "OpenKitDevice";

        // Name of Dynatrace HTTP header which is used for tracing web requests.
        public const string WEBREQUEST_TAG_HEADER = "X-dynaTrace";

        /// <summary>
        /// Static constructor used to initialize certain fields from AssemblyInfo.
        /// </summary>
        static OpenKitConstants()
        {
#if (!NET35 && !NET40)
            var assembly = typeof(OpenKitConstants).GetTypeInfo().Assembly;

#elif (NET35 || NET40)
            var assembly = Assembly.GetExecutingAssembly();
#else
#error .NET preprocessor directive is missing
#endif

            // determine OpenKit version
            // first try AssemblyVersion and if not present try with AssemblyFileVersion
            // if both attributes are not present, leave a "dummy" default
            var assemblyVersionAttribute = assembly.GetCustomAttribute<AssemblyVersionAttribute>();
            var assemblyFileVersionAttribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            if (assemblyVersionAttribute != null)
            {
                DefaultApplicationVersion = assemblyVersionAttribute.Version;
            }
            else if (assemblyFileVersionAttribute != null)
            {
                DefaultApplicationVersion = assemblyFileVersionAttribute.Version;
            }

            // re-initialize the default operating system, after determining the version
            DefaultOperatingSystem = "OpenKit " + DefaultApplicationVersion;

            // set the default manufacturer
            var assemblyCompanyAttribute = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
            if (assemblyCompanyAttribute != null)
            {
                DefaultManufacturer = assemblyCompanyAttribute.Company;
            }
        }
    }
}
