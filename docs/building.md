# Building OpenKit .NET

## Prerequisites for building
* .NET Framework 3.5+ or .NET Core 1.0+
* Visual Studio 2017 version 15.7 (to open VS solution) or Rider .NET IDE
* In order to run all unit tests you must install the latest SDKs for both Framework and Core (approved: .Net Core 1.1.7+, .Net Core 2.1.4+, Framework 4.6.2+)

## Building the Source
Open `openkit-dotnet.sln` in Visual Studio 2017 or Rider and build the needed project(s).


The built dll file(s) `Dynatrace.OpenKit.dll` will be located under the `.build/bin/<configuration>/Dynatrace.OpenKit` directory.
