# Upgrade guide for OpenKit .NET

## OpenKit .NET 3.2 to 3.3
There are no breaking API changes and upgrading is straightforward, by [updating][update] the library
to the latest 3.3 release.

### Deprecated API
* `IWebRequestTracer.SetBytesSent(int bytes)`
  Use `IWebRequestTracer.SetBytesSent(long bytes)` to increase the size range
* `IWebRequestTracer.SetBytesReceived(int bytes)`
  Use `IWebRequestTracer.SetBytesReceived(long bytes)` to increase the size range

## OpenKit .NET 3.1 to 3.2
There are no breaking API changes and upgrading is straightforward, by [updating][update] the library
to the latest 3.2 release.

## OpenKit .NET 3.0 to 3.1
There are no breaking API changes and upgrading is straightforward, by [updating][update] the library
to the latest 3.1 release.

## OpenKit .NET 2.2 to 3.0
Appmon has been removed from OpenKit .NET. If you don't want to replace your AppMon related code, stay on the latest 2.2.x release.
### Removed API
* `AbstractOpenKitBuilder` has been removed as it was not needed anymore due to AppMon removal. All functionalities have been consolidated into the `DynatraceOpenKitBuilder`.
* `IAction.ReportError(string errorName, int errorCode, string reason)`  
  Use `IAction.ReportError(string errorName, int errorCode)` without `string reason` argument, as
  `reason` is unhandled in Dynatrace.
* `IOpenKit.ApplicationVersion`  
  Use `OpenKitBuilder` to set the `ApplicationVersion`
* `IWebRequestTracer.SetResponseCode(int responseCode)` and `IWebRequestTracer.Stop()`  
  Use `IWebRequestTracer.Stop(int responseCode)` instead as replacement.
* `DynatraceOpenKitBuilder.WithApplicationName(string applicationName)`  
  The application name is configured in Dynatrace Web UI.
* `DynatraceOpenKitBuilder.EnableVerbose()`  
  Use `DynatraceOpenKitBuilder.WithLogLevel(LogLevel.DEBUG)` instead.

## OpenKit .NET 2.1 to 2.2
There are no breaking API changes and upgrading is straightforward, by [updating][update] the library
to the latest 2.2 release.

## OpenKit .NET 2.0 to 2.1
There are no breaking API changes and upgrading is straightforward, by [updating][update] the library
to the latest 2.1 release.

### Deprecated API
* `IAction.ReportError(string errorName, int errorCode, string reason)`  
  Use `IAction.ReportError(string errorName, int errorCode)` without `string reason` argument, as
  `reason` is unhandled in Dynatrace.

## OpenKit .NET 1.4 to 2.0
There are no breaking API changes and upgrading is straightforward, by [updating][update] the library
to the latest 2.0 release.

### Deprecated API
* `IWebRequestTracer.SetResponseCode(int responseCode)` and `IWebRequestTracer.Stop()`  
  Use `IWebRequestTracer.Stop(int responseCode)` instead as replacement.
* `DynatraceOpenKitBuilder.WithApplicationName(string applicationName)`  
  The application name is configured in Dynatrace Web UI.
* `AbstractOpenKitBuilder.EnableVerbose()`  
  Use `AbstractOpenKitBuilder.WithLogLevel(LogLevel.DEBUG)` instead.
* `AbstractOpenKitBuilder.WithModelID(string modelId)`  
  Use `AbstractOpenKitBuilder.WithModelId(string modelId)` instead.

## OpenKit .NET 1.3 and below to 1.4
There are no breaking API changes and upgrading is straightforward, by [updating][update] the library
to the latest 1.4 release.

### Deprecated API
* `DynatraceOpenKitBuilder(string endPointUrl, string applicationId, string deviceId)`  
   Use `DynatraceOpenKitBuilder(string endPointUrl, string applicationId, long deviceId)` instead.

[update]: ./installing.md#Updating-OpenKit-.NET