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

using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("OpenKit .NET")]
[assembly: AssemblyDescription("Dynatrace OpenKit SDK for .NET")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Dynatrace LLC")]
[assembly: AssemblyProduct("Dynatrace.OpenKit.Properties")]
[assembly: AssemblyCopyright("(c) 2016-2019 Dynatrace LLC")]
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
[assembly: AssemblyVersion("1.4.4.0")]
[assembly: AssemblyFileVersion("1.4.4.0")]

// Expose internal classes to test assemblies
[assembly: InternalsVisibleTo("openkit-dotnetcore-1.0.Tests, PublicKey=" + AssemblyInfoConstants.PublicKey)]
[assembly: InternalsVisibleTo("openkit-dotnetcore-1.1.Tests, PublicKey=" + AssemblyInfoConstants.PublicKey)]
[assembly: InternalsVisibleTo("openkit-dotnetcore-2.0.Tests, PublicKey=" + AssemblyInfoConstants.PublicKey)]
[assembly: InternalsVisibleTo("openkit-dotnetfull-3.5.Tests, PublicKey=" + AssemblyInfoConstants.PublicKey)]
[assembly: InternalsVisibleTo("openkit-dotnetfull-4.0.Tests, PublicKey=" + AssemblyInfoConstants.PublicKey)]
[assembly: InternalsVisibleTo("openkit-dotnetfull-4.5.Tests, PublicKey=" + AssemblyInfoConstants.PublicKey)]
[assembly: InternalsVisibleTo("openkit-dotnetfull-4.6.Tests, PublicKey=" + AssemblyInfoConstants.PublicKey)]
[assembly: InternalsVisibleTo("openkit-dotnetfull-4.7.Tests, PublicKey=" + AssemblyInfoConstants.PublicKey)]
[assembly: InternalsVisibleTo("openkit-dotnetpcl-4.5.Tests, PublicKey=" + AssemblyInfoConstants.PublicKey)]
[assembly: InternalsVisibleTo("openkit-dotnetstandard-2.0.Tests, PublicKey=" + AssemblyInfoConstants.PublicKey)]
// Expose internal classes to NSubstitute
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=" + AssemblyInfoConstants.PublicKeyToDynamicProxyGenAssembly2)]

class AssemblyInfoConstants
{
    public const string PublicKey =
        "002400000480000094000000060200000024000052534131000400000100010005df99dd5dd29e" +
        "6ff6d75a55caadeb2969f3c9b68535f2a68755af814a61bd93e81c0f524898e7d41d06ffebd8b6" +
        "8538916aac769da3876656df30306585e3fca8a5ce3744d534767287344418d29687a67bdc949e" +
        "4424f086983f7c66b936f8b40fb32eb52732a7d7c9b11175c3d9b56f57b75000abec9e381724be" +
        "eeb4a298";

    public const string PublicKeyToDynamicProxyGenAssembly2 =
        "0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99" +
        "c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654" +
        "753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46" +
        "ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484c" +
        "f7045cc7";
}