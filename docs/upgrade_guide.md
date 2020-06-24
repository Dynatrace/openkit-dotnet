# Upgrade guide for OpenKit .NET

## OpenKit .NET 1.4 to 2.0
There are no breaking API changes and upgrading is straightforward, by [updating][update] the library
to the latest 2.0 release.

### Deprecated API
* `IWebRequestTracer.SetResponseCode(int responseCode)` and `IWebRequestTracer.Stop()`  
  Use `IWebRequestTracer.Stop(int responseCode)` instead as replacement.
* `DynatraceOpenKitBuilder.WithApplicationName(string applicationName)`  
  The application name is configured in Dynatrace Web UI.

## OpenKit .NET 1.3 and below to 1.4
There are no breaking API changes and upgrading is straightforward, by [updating][update] the library
to the latest 1.4 release.

### Deprecated API
* `DynatraceOpenKitBuilder(string endPointUrl, string applicationId, string deviceId)`  
   Use `DynatraceOpenKitBuilder(string endPointUrl, string applicationId, long deviceId)` instead.
* `AppMonOpenKitBuilder(string endpointUrl, string applicationName, string deviceId)`  
   Use `AppMonOpenKitBuilder(string endpointUrl, string applicationName, long deviceId)` instead.

[update]: ./installing.md#Updating-OpenKit-.NET