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

using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("OpenKit .NET")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Dynatrace LLC")]
[assembly: AssemblyProduct("Dynatrace.OpenKit.Properties")]
[assembly: AssemblyCopyright("(c) 2016-2018 Dynatrace LLC")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("d5432fb2-9862-4a87-93d6-4e7df6d2ee03")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.2.0.0")]
[assembly: AssemblyFileVersion("1.2.0.0")]

// Expose internal classes to test assemblies
[assembly: InternalsVisibleTo("openkit-dotnetcore-1.0.Tests")]
[assembly: InternalsVisibleTo("openkit-dotnetcore-1.1.Tests")]
[assembly: InternalsVisibleTo("openkit-dotnetcore-2.0.Tests")]
[assembly: InternalsVisibleTo("openkit-dotnetfull-3.5.Tests")]
[assembly: InternalsVisibleTo("openkit-dotnetfull-4.0.Tests")]
[assembly: InternalsVisibleTo("openkit-dotnetfull-4.5.Tests")]
[assembly: InternalsVisibleTo("openkit-dotnetfull-4.6.Tests")]
[assembly: InternalsVisibleTo("openkit-dotnetfull-4.7.Tests")]
[assembly: InternalsVisibleTo("openkit-dotnetstandard-2.0.Tests")]
// Expose internal classes to NSubstitute
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]