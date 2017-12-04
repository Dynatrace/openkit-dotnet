/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
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
[assembly: AssemblyCopyright("(c) 2016-2017 Dynatrace LLC")]
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
[assembly: AssemblyVersion("0.3.0.0")]
[assembly: AssemblyFileVersion("0.3.0.0")]

// Expose internal classes to test assemblies
[assembly: InternalsVisibleTo("openkit-dotnetcore-2.0.Tests")]
[assembly: InternalsVisibleTo("openkit-dotnetfull-3.5.Tests")]
[assembly: InternalsVisibleTo("openkit-dotnetfull-4.0.Tests")]
[assembly: InternalsVisibleTo("openkit-dotnetfull-4.5.Tests")]
[assembly: InternalsVisibleTo("openkit-dotnetfull-4.6.Tests")]
// Expose internal classes to NSubstitute
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] 