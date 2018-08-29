//
// Copyright 2018 Dynatrace LLC
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

using Dynatrace.OpenKit.Util;
using System.Reflection;

namespace Dynatrace.OpenKit.API
{
    /// <summary>
    /// Defines constant values used in OpenKit
    /// </summary>
    public class OpenKitConstants
    {
        // default values used in configuration
        public static readonly string DEFAULT_APPLICATION_VERSION = "<unknown version>";
        public static readonly string DEFAULT_OPERATING_SYSTEM = "OpenKit " + DEFAULT_APPLICATION_VERSION;
        public static readonly string DEFAULT_MANUFACTURER = "Dynatrace";
        public const string DEFAULT_MODEL_ID = "OpenKitDevice";

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
                DEFAULT_APPLICATION_VERSION = assemblyVersionAttribute.Version;
            }
            else if (assemblyFileVersionAttribute != null)
            {
                DEFAULT_APPLICATION_VERSION = assemblyFileVersionAttribute.Version;
            }

            // re-initialize the default operating system, after determining the version
            DEFAULT_OPERATING_SYSTEM = "OpenKit " + DEFAULT_APPLICATION_VERSION;

            // set the default manufacturer 
            var assemblyCompanyAttribute = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
            if (assemblyCompanyAttribute != null)
            {
                DEFAULT_MANUFACTURER = assemblyCompanyAttribute.Company;
            }
        }
    }
}
