# Dynatrace OpenKit - .NET Reference Implementation

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![Build status](https://ci.appveyor.com/api/projects/status/ug034eehokcyw1ks/branch/master?svg=true)](https://ci.appveyor.com/project/openkitdt/openkit-dotnet/branch/master)
[![Coverage Status](https://coveralls.io/repos/github/Dynatrace/openkit-dotnet/badge.svg?branch=master)](https://coveralls.io/github/Dynatrace/openkit-dotnet?branch=master)


## What is the OpenKit?

The OpenKit provides an easy and lightweight way to get insights into applications with Dynatrace/AppMon by instrumenting the source code of those applications.

It is best suited for applications running separated from their backend and communicating via HTTP, like rich-client-applications, embedded devices, terminals, and so on.

The big advantages of the OpenKit are that it's designed to
* be as easy-to-use as possible
* be as dependency-free as possible (no third party libraries or Dynatrace/AppMon Agent needed)
* be easily portable to other languages and platforms

This repository contains the reference implementation in pure .NET/C#. Other implementations are listed as follows:
* Java: https://github.com/Dynatrace/openkit-java/

## What you can do with the OpenKit
* Create Sessions and User Actions
* Report values, events, errors and crashes
* Trace web requests to server-side PurePaths
* Tag Sessions with a user tag
* Use it together with Dynatrace or AppMon

## What you cannot do with the OpenKit
* Create server-side PurePaths (this functionality is provided by [OneAgent SDKs](https://github.com/Dynatrace/OneAgent-SDK))
* Create metrics (use the [Custom network devices & metrics API](https://www.dynatrace.com/support/help/dynatrace-api/timeseries/what-does-the-custom-network-devices-and-metrics-api-provide/) to report metrics)

## Design Principles
* API should be as simple and easy-to-understand as possible
* Incorrect usage of the OpenKit should still lead to valid results, if possible
* In case of errors, the API should not throw exceptions, but only log those errors (in verbose mode)
* No usage of third-party libraries, should run without any dependencies
* Avoid usage of newest .NET APIs, should be running on older .NET runtimes, too
* Avoid usage of too much .NET-specific APIs to allow rather easy porting to other languages
* Design reentrant APIs and document them

## Prerequisites

### Running the OpenKit
* .NET Framework 3.5+ or .NET Core 1.0+

### Building the Source
* .NET Framework 3.5+ or .NET Core 1.0+
* Visual Studio 2017 (to open VS solution)
* In order to run all unit tests you must install the latest SDKs for both Framework and Core (approved: .Net Core 1.1.7+, .Net Core 2.1.4+, Framework 4.6.2+)

## Building the Source

Open `openkit-dotnet.sln` in Visual Studio 2017 and build the needed project(s).
The built dll file(s) `openkit-<version>-dotnet<dotnet_framework>-<dotnet_version>.dll` will be located under the `<project_name>/bin` directory.

## General Concepts

In this part the concepts used throughout OpenKit are explained. A short sample how to use OpenKit is
also provided. For detailed code samples have a look into [example.md](docs/example.md).

### OpenKit

An `IOpenKit` instance is responsible for getting and setting application relevant information, e.g.
the application's version and device specific information.  
Furthermore the `IOpenKit` is responsible for creating user sessions (see `ISession`).
  
Although it would be possible to have multiple `IOpenKit` instances connected to the same endpoint
(Dynatrace/AppMon) within one process, there should be one unique instance. `IOpenKit` is designed to be
thread safe and therefore the instance can be shared among threads.  

On application shutdown, `Shutdown()` needs to be called on the OpenKit instance.

### Device

An `IDevice` instance, which can be retrieved from an `IOpenKit` instance, contains methods
for setting device specific information. It's not mandatory for the application developer to
provide this information, reasonable default values exist.  
However when the application is run on multiple different devices it might be quite handy
to know details about the used device (e.g device identifier, device manufacturer, operating system).

### Session

An `ISession` represents kind of a user session, similar to a browser session in a web application.
However the application developer is free to choose how to treat an `ISession`.  
The `ISession` is used to create `IRootAction` instances and report application crashes.  

When an `ISession` is no longer required, it's highly recommended to end it, using the `ISession.End()` method. 

### RootAction and Action

The `IRootAction` and `IAction` are named hierarchical nodes for timing and attaching further details.
An `IRootAction` is created from the `ISession` and it can create `IAction` instances. Both, `IRootAction` and
`IAction`, provide the possibility to attach key-value pairs, named events and errors, and are used 
for tracing web requests.

### WebRequestTracer

When the application developer wants to trace a web request, which is served by a service 
instrumented by Dynatrace, an `IWebRequestTracer` should be used, which can be
requested from an `IAction`.  

### Named Events

A named `Event` is attached to an `IAction` and contains a name.

### Key-Value Pairs

For an `IAction` key-value pairs can also be reported. The key is always a string
and the value may be an integer (int), a floating point (double) or a string.

### Errors & Crashes

Errors are a way to report an erroneous condition on an `IAction`.  
Crashes are used to report (unhandled) exceptions on an `ISession`.

### Identify Users

OpenKit enables you to tag sessions with unique user tags. The user tag is a String 
that allows to uniquely identify a single user.

## Example

This small example provides a rough overview how OpenKit can be used.  
Detailed explanation is available in [example.md](docs/example.md).

```cs
string applicationName = "My OpenKit application";
string applicationID = "application-id";
long deviceID = 42L;
string endpointURL = "https://tenantid.beaconurl.com/mbeacon";

IOpenKit openKit = new DynatraceOpenKitBuilder(endpointURL, applicationID, deviceID)
    .WithApplicationName(applicationName)
    .WithApplicationVersion("1.0.0.0")
    .WithOperatingSystem("Windows 10")
    .WithManufacturer("MyCompany")
    .WithModelID("MyModelID")
    .Build();

string clientIP = "8.8.8.8";
ISession session = openKit.CreateSession(clientIP);

session.IdentifyUser("jane.doe@example.com");

string rootActionName = "rootActionName";
IRootAction rootAction = session.EnterAction(rootActionName);

string childActionName = "childAction";
IAction childAction = rootAction.EnterAction(childActionName);

childAction.LeaveAction();
rootAction.LeaveAction();
session.End();
openKit.Shutdown();
``` 

