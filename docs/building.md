# Building OpenKit .NET

## Prerequisites for building
* .NET Framework 3.5+ or .NET Core 3.1+
* Visual Studio:
    * <= .NET 4.8: Visual Studio 2019 version 16.11 (to open VS solution) or Rider .NET IDE
    * .NET 7.0, 6.0 & 5.0: Visual Studio 2022 
* In order to run all unit tests you must install the latest SDKs for both Framework and Core (approved: .Net Core 1.1.7+, .Net Core 2.1.4+, Framework 4.6.2+)

## Building the Source
Open `openkit-dotnet.sln` in Visual Studio 2019 or Rider and build the needed project(s).


The built dll file(s) `Dynatrace.OpenKit.dll` will be located under the `.build/bin/<configuration>/Dynatrace.OpenKit` directory.
