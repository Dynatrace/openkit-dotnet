# Dynatrace OpenKit - .NET Reference Implementation

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
* Tag web requests to server-side PurePaths
* Use it together with Dynatrace or AppMon

## What you cannot do with the OpenKit
* Create server-side PurePaths (you have to use an ADK for that)

## Design Principles
* API should be as simple and easy-to-understand as possible
* Incorrect usage of the OpenKit should still lead to valid results, if possible
* In case of errors, the API should not throw exceptions, but only log those errors (in verbose mode)
* No usage of third-party libraries, should run without any dependencies
* Avoid usage of newest .NET APIs, should be running on older .NET runtimes, too
* Avoid usage of too much .NET-specific APIs to allow rather easy porting to other languages

## Prerequisites

### Running the OpenKit
* .NET Framework 4.x+

### Building the Source
* .NET Framework 4.x+
* Visual Studio 2017 (to open VS solution)

## Building the Source

Open `openkit-dotnetfull.sln` in Visual Studio 2017 and build the needed project(s).

Note: System.Net.Http.dll has to be deployed when using OpenKit for .NET Framework 4.0.

## General Concepts
* TBD

## Known Current Limitations

* ignored configs: capture lifecycle, crash reporting, error reporting, session timeout
* it's only possible to have one OpenKit instance running as providers are static

## TODOs

* add solutions for .NET Core and UWP
* add samples/tests
* add multiple time syncs for Dynatrace
* move providers from static to instance (multiple OpenKits -> multiple providers)
* prevent re-entrances e.g. of startup/shutdown
* add HTTPS support and certificate verification
* HTTP optimizations (reuse connection, pool http client?)
* provide simple samples to get started as markdown
* mobile sampling
* currently gzipping is done with code taken from DotNetZip (http://dotnetzip.codeplex.com/), investigate other solution with .NET framework (no luck so far!)
