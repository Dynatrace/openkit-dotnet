# OpenKit .NET Changelog

## [Unreleased](https://github.com/Dynatrace/openkit-dotnet/compare/v3.3.0...HEAD)

## 3.3.0 [Release date: 2024-08-02]
[GitHub Releases](https://github.com/Dynatrace/openkit-dotnet/releases/tag/v3.3.0)

### Added

- `IWebRequestTracer.SetBytesSent(long bytes)` to increase the size range
- `IWebRequestTracer.SetBytesReceived(long bytes)` to increase the size range

### Changed

- `HttpClientWebClient` for NET3.5 is now using User-Agent property directly
- Improved cancelation or disposal of childs when leaving an action
- Deprecated `IWebRequestTracer.SetBytesSent(int bytes)` due to datatype limitations
- Deprecated `IWebRequestTracer.SetBytesReceived(int bytes)` due to datatype limitations

## 3.2.0 [Release date: 2023-12-06]
[GitHub Releases](https://github.com/Dynatrace/openkit-dotnet/releases/tag/v3.2.0)

### Changed
- `Session.SendBizEvent` will always send an event regardless of the `DataCollectionLevel`

### Added
- Support for .NET 8.0
- Support for .NET Framework 4.8.1

## 3.1.0 [Release date: 2023-06-05]
[GitHub Releases](https://github.com/Dynatrace/openkit-dotnet/releases/tag/v3.1.0)

### Added
- Support for .NET 7.0
- Non-finite numeric values are serialized as JSON null in reported events, and a special field is added for supportability.

## 3.0.0 [Release date: 2022-12-06]
[GitHub Releases](https://github.com/Dynatrace/openkit-dotnet/releases/tag/v3.0.0)

### Added
- Support for .NET 6.0
- User-Agent header to http requests
- Business events capturing API `SendBizEvent`
- `Session.ReportNetworkTechnology(string technology)`
- `Session.ReportCarrier(string carrier)`
- `Session.ReportConnectionType(ConnectionType connectionType)`

### Removed
- Support for .NET 4.5.1 / 4.5 / 4.0
- Support for .NET Core App 2.1 / 2.0 / 1.1 / 1.0

### Changed
- Fix issue in `DefaultTimingProvider` with `Stopwatch` frequency
- Default maximum age of an entry in the beacon cache changed from 1h 45m to 45m
- Removed AppMon functionality
- Maximum length of reported error/crash stacktrace has been limited to 128k.
- Maximum length of reported error/crash reason has been limited to 1000.

### Removed
* `DynatraceOpenKitBuilder.WithApplicationName(string applicationName)`
* `DynatraceOpenKitBuilder.EnableVerbose()`
* `IAction.ReportError(string errorName, int errorCode, string reason)`
* `IOpenKit.ApplicationVersion`
* `IWebRequestTracer.SetResponseCode(int responseCode)`
* `IWebRequestTracer.Stop()`

## 2.2.0 [Release date: 2021-05-19]
[GitHub Releases](https://github.com/Dynatrace/openkit-dotnet/releases/tag/v2.2.0)

### Added
- Support cost control configured in the Dynatrace UI.
- It is possible to get the duration from an `IAction`.
- An `IAction` can be canceled.  
  Canceling an `IAction` is similar to leaving it, without reporting it.
- Requests to Dynatrace can be intercepted to add custom HTTP headers.
- Excplicit support for additional .NET target frameworks.
  - .NET Core 2.1 and 3.1
  - .NET Standard 2.1
  - .NET Framework 4.5.1, 4.5.2, 4.6.1, 4.6.2, 4.7.1, 4.7.2 and 4.8
  - .NET 5.0

### Changed
- Provide a more reliable way to determine monotonic timestamps.
- Fix potential endless loop in beacon sending, when lots of data
  is generated in a short period of time.
- Update testing dependencies to latest possible version.

## 2.1.0 [Release date: 2020-11-16]
[GitHub Releases](https://github.com/Dynatrace/openkit-dotnet/releases/tag/v2.1.0)

### Added
- `DynatraceOpenKitBuilderVB` and `AppMonOpenKitBuilderVB` are added to workaround
  an ambiguity error affecting VisualBasic developers. Since VB is case insensitive
 `AbstractOpenKitBuilder.WithModelID(string)` and `AbstractOpenKitBuilder.WithModelId(string)`
  are considered the same methods.  
  This workaround is marked as obsolete and will be removed when the obsolete method
  `AbstractOpenKitBuilder.WithModelID(string)` is removed.
- Overloaded `IAction.ReportValue(string, long`) method for reporting 64-bit integer values.
- Overloaded `ISession.ReportCrash(Exception)` as convenience method for reporting an `Exception` as crash.
- Overloaded `IAction.ReportError(string, int)` method for reporting an integer error code without description.  
  The old `IAction.ReportError(string, int, string)` has been deprecated in favor of the new one.
- Overloaded `IAction.ReportError(string, Exception)` for reporting caught exceptions as error.
- Overloaded `IAction.ReportError(string, string, string, string)` for reporting generic errors to Dynatrace.

### Changed
- Fix issue with sessions being closed after splitting.
  This happened because OpenKit was sending an end session event right after splitting.
  New behavior is to only send the end session event if explicitly requested via
  the `Session.End()` method and only for the active session.
- `IdentifyUser` can be called with `null` or an empty string.  
  This simulates a log off event and no longer re-applies the user tag on split sessions.
- Improve handling of client IP in combination with server-side detection.
- Fix in `JsonResponseParser` to parse correct key for max session duration.
- Fix multithreading issues in `SessionProxy`, leading to a potential `NullReferenceException`.
- Fix potential memory leak for very short-lived sessions in `SessionProxy`.

## 2.0.0 [Release date: 2020-06-24]
[GitHub Releases](https://github.com/Dynatrace/openkit-dotnet/releases/tag/v2.0.0)

### Added
- Support for parsing JSON
- Technology type support for error and crashes
- Support for session splitting. Sessions are split transparently after either the maximum session duration,
  the idle timeout or the number of top level actions are exceeded. Session splitting is only applicable,
  if it is also supported by Dynatrace. The internal details are described [here](./docs/internals.md#session-splitting).
- Re-apply user tag on split sessions.

### Changed
- On OpenKitBuilder creation device ID is parsed from the given string. Non-numeric
  device IDs are hashed to a corresponding numeric value. Internally a numeric
  type is used for the device ID.
- Response code is now a parameter of WebRequestTracer's stop method.
  Existing methods for stopping and setting the response code have been deprecated.
- Fix missing parameter in request URL when requesting a new session.
- `WithModelID` on `AppMonOpenKitBuilder` / `DynatraceOpenKitBuilder` was deprecated. `WithModelId` should be used instead.
- Setting property `ApplicationVersion` on `IOpenKit` was deprecated and calls to this setter are ignored. 
  `WithApplicationVersion` on `AppMonOpenKitBuilder` / `DynatraceOpenKitBuilder` should be used instead.
- Add OpenKit.createSession overload without IP address parameter.  
  The IP address is determined in this case on the server side.
- Fix coveralls.io build
- Fixed issue with reporting localized beacon data  
  Integer and floating point numbers might have been reported with the thread's locale
  causing issues with session properties.
- Reporting a crash causes a session split, which is transparently handled

### Improvements
- Reformatted text files to unix style line endings.

## 1.4.0 [Release date: 2019-02-14]
[GitHub Releases](https://github.com/Dynatrace/openkit-dotnet/releases/tag/v1.4.0)

### Added
- Package distribution via NuGet
  Distribution via GitHub releases changed as well, since a single zip file is deployed.

### Changed
- Application ID and Visitor ID are correctly encoded for special characters
  The encoding is a percent-encoding based on RFC 3986 with additional encoding of underscore characters.
- OpenKit assemblies are strongly named
- Fixed problem with infinite time sync requests
  This problem occurred mainly in AppMon settings.

## 1.3.0 [Release date: 2018-10-25]
[GitHub Releases](https://github.com/Dynatrace/openkit-dotnet/releases/tag/v1.3.0)

### Added
- Device ID can be specified as String in addition to long  
  This allows to send UUIDs or other specifiers
- Standalone web request tagging  
  A WebRequestTracer can also be obtained from a Session

## 1.2.0 [Release date: 2018-09-14]
[GitHub Releases](https://github.com/Dynatrace/openkit-dotnet/releases/tag/v1.2.0)

### Added
- New project for .NET Standard 2.0
- New project for UWP and PCL 4.5 target 111   
  For those two projects, self signed SSL certificates are not manageable by OpenKit .NET.
- Server overload prevention  
  Additional HTTP 429 response code handling

### Changed
- Fix wrong Session start time
- Fix wrong device ID in web requests
- Add Thread identifier to log output  
  This makes the log output similar to the Java/Native implementation

### Improved
- OpenKit internal version handling

## 1.1.0 [Release date: 2018-07-20]
[GitHub Releases](https://github.com/Dynatrace/openkit-dotnet/releases/tag/v1.1.0)

### Added
- Extend OpenKit objects from IDisposable to be able to wrap them in `using` blocks.
  The following objects are affected
  - IOpenKit
  - ISession
  - IRootAction/IAction
  - IWebRequestTracer
- coveralls.io Coverage integration
- New projects for .NET 4.7 and .NET Core 1.1
- Server overload prevention
- GDPR compliance

### Changed
- Replace DotNetZip library with .NET builtin gzip library
- Internally used sleep method does sleep in small intervals  
  .NET Core 1.X cannot interrupt a thread
- Further actions on already left/closed OpenKit objects are no longer possible  
  Calling the methods is still allowed, but nothing is reported to the backend 
- HTTPClient checks response type from server when parsing
- Enhanced `null` checks in public interface implementation  
  No exception are thrown, if nulls are passed via public interfaces
- OpenKit internal threads are background threads
- Enhanced state transition in internal state engine
- Thread IDs are reported as positive 32-bit integers only  
  This is handled by masking out the sign bit
- WebRequestTracer's start time is initialized in constructor
- Advanced URL validation when tracing web requests  
  The URL must have the form  
  `<scheme>://<any character>[<any character>]*`  
  Where scheme must be as defined in RFC 3986
- InetAddress validation for IPv6 mixed mode addresses
- Major logging improvements (more and better messages) 
- OpenKit initialization is now truly asynchronously

### Improved
- Enhanced BeaconCache documentation
- Add WCF Autotracing sample (See samples/WcfAutoTracerSample.cs)
- Various typos fixed
- Increase dependent packages versions

## 1.0.2 [Release date: 2018-07-20]
[GitHub Releases](https://github.com/Dynatrace/openkit-dotnet/releases/tag/v1.0.2)

### Changed
- OpenKit initialization is now truly asynchronously

## 1.0.1 [Release date: 2018-01-29]
[GitHub Releases](https://github.com/Dynatrace/openkit-dotnet/releases/tag/v1.0.1)
### Initial public release

Note: Although this release was initially created on January 29th, 2018 a data loss occurred.
The linked release was re-created on April 12th, 2018.