# Dynatrace OpenKit - .NET Example

The following document provides an in depth overview, how OpenKit can be used from
developer's point of view. It explains the usage of all the API methods.  

To make it more obvious for the reader which data types are used, all examples below
avoid C#'s auto type deduction feature.

## Obtaining an OpenKit Instance

Depending on the backend a new OpenKit instance can be obtained by using either `DynatraceOpenKitBuilder` 
or `AppMonOpenKitBuilder`. Despite from this, the developer does not need to distinguish between 
different backend systems.

### Dynatrace
 
For Dynatrace SaaS and Dynatrace Managed the `DynatraceOpenKitBuilder` is used to build new OpenKit instances. 

:information_source: VisualBasic developers should use `DynatraceOpenKitBuilderVB` for now to workaround an
ambiguity error caused by two methods differing in case only.

```cs
string applicationName = "My OpenKit application";
string applicationID = "application-id";
long deviceID = 42L;
string endpointURL = "https://tenantid.beaconurl.com/mbeacon";

// by default verbose logging is disabled
IOpenKit openKit = new DynatraceOpenKitBuilder(endpointURL, applicationID, deviceID).Build();
```

* The `endpointURL` denotes the Dynatrace SaaS cluster endpoint OpenKit communicates with and 
  is shown when creating the application in Dynatrace SaaS. The endpoint URL can be found in the settings 
  page of the custom application in Dynatrace.
* The `applicationID` parameter is the unique identifier of the application in Dynatrace Saas. The
application's id can be found in the settings page of the custom application in Dynatrace.
* The `deviceID` is a unique identifier, which might be used to uniquely identify a device.

:grey_exclamation: For Dynatrace Managed the endpoint URL looks a bit different.


### AppMon

An OpenKit instance for AppMon can be obtained by using the `AppMonOpenKitBuilder`.

:information_source: VisualBasic developers should use `AppMonOpenKitBuilderVB` for now to workaround an
ambiguity error caused by two methods differing in case only.

The example below demonstrates how to connect an OpenKit application to an AppMon endpoint.
```cs
string applicationName = "My OpenKit application";
long deviceID = 42L;
string endpointURL = "https://beaconurl.com/dynaTraceMonitor";

// by default verbose logging is disabled
IOpenKit openKit = new AppMonOpenKitBuilder(endpointURL, applicationName, deviceID).Build();
```

* The `endpointURL` denotes the AppMon endpoint OpenKit communicates with.
* The `applicationName` parameter is the application's name in AppMon and is also used as the application's id.
* The `deviceID` is a unique identifier, which might be used to uniquely identify a device.

### Optional Configuration

In addition to the mandatory parameters described above, the builder provides methods to further 
customize OpenKit. This includes device specific information like operating system, manufacturer, or model id. 

| Method Name | Description | Default Value |
| ------------- | ------------- | ---------- |
| `WithApplicationVersion`  | sets the application version  | `"2.2.2"` |
| `WithOperatingSystem`  | sets the operating system name | `"OpenKit 2.2.2"` |
| `WithManufacturer`  | sets the manufacturer | `"Dynatrace"` |
| `WithModelId`  | sets the model id  | `"OpenKitDevice"` |
| `WithBeaconCacheMaxRecordAge`  | sets the maximum age of an entry in the beacon cache in milliseconds | 1 h 45 min |
| `WithBeaconCacheLowerMemoryBoundary`  | sets the lower memory boundary of the beacon cache in bytes  | 80 MB |
| `WithBeaconCacheUpperMemoryBoundary`  |  sets the upper memory boundary of the beacon cache in bytes | 100 MB |
| `WithTrustManager` | sets a custom `ISSLTrustManager` instance, replacing the builtin default one.<br>Details are described in section [SSL/TLS Security in OpenKit](#ssltls-security-in-openkit). | `SSLStrictTrustManager` |
| `EnableVerbose`  | *Deprecated*, use `WithLogLevel` instead.<br>Enables extended log output for OpenKit if the default logger is used. | `false` |
| `WithLogLevel` | sets the log level if the default logger is used | `LogLevel.WARN` |
| `WithLogger` | sets a custom logger, replacing the builtin default one.<br>Details are described in section [Logging](#logging). | `DefaultLogger` |
| `WithHttpRequestInterceptor` | sets a custom `IHttpRequestInterceptor` instance,  replacing the builtin default one.<br>Details are described in section [Intercepting HTTP traffic to Dynatrace/AppMon](#intercepting-http-traffic-to-dynatraceappmon). | `NullHttpRequestInterceptor` |
| `WithHttpResponseInterceptor` | sets a custom `IHttpResponseInterceptor` instance,  replacing the builtin default one.<br>Details are described in section [Intercepting HTTP traffic to Dynatrace/AppMon](#intercepting-http-traffic-to-dynatraceappmon). | `NullHttpResponseInterceptor` |


## SSL/TLS Security in OpenKit

All OpenKit communication to the backend happens via HTTPS (TLS/SSL based on .NET Framework support).
By default, OpenKit expects valid server certificates.
However it is possible, if really needed, to bypass TLS/SSL certificate validation. This can be achieved by
passing an implementation of `ISSLTrustManager` to the previously mentioned OpenKit builder.

:warning: We do **NOT** recommend bypassing TLS/SSL server certificate validation, since this allows
man-in-the-middle attacks.

:warning: Dynatrace SaaS [supports only TLS 1.2+](https://www.dynatrace.com/support/help/whats-new/tls-1-0-and-1-1-end-of-support-for-rum-data). Using .NET Framework below 4.7 might require [additional configuration](https://docs.microsoft.com/en-us/dotnet/framework/network-programming/tls#if-your-app-targets-a-net-framework-version-earlier-than-47).

Keep in mind on .NET 3.5 and 4.0 server certificate validation can only be overwritten on global level via
the `ServicePointManager`. In versions .NET 4.5+ overwriting happens on request basis.  

## Intercepting HTTP traffic to Dynatrace/AppMon

When routing traffic through own network infrastructure it might be necessary  to intercept HTTP traffic
to Dynatrace/AppMon and add or overwrite HTTP headers. This can be achieved by implementing the 
`IHttpRequestInterceptor` interface and passing an instance to the builder by calling 
the `WithHttpRequestInterceptor` method. OpenKit invokes the `IHttpRequestInterceptor.Intercept(IHttpRequest)` 
method for each request sent to Dynatrace/AppMon.  
It might be required to intercept the HTTP response and read custom response headers. This
can be achieved by implementing the `IHttpResponseInterceptor` interface and passing an instance to the builder
by calling `WithHttpResponseInterceptor`. OpenKit calls the `IHttpResponseInterceptor.Intercept(IHttpResponse)`
for each HTTP response received from the backend.

## Logging

By default, OpenKit uses a logger implementation that logs to stdout. If the default logger is used, the desired
minimum log level can be set by calling `WithLogLevel` in the builder, and only messages with the same or higher 
priorities are logged.

A custom logger can be set by calling `WithLogger` in the builder. When a custom logger is used, a call to 
`WithLogLevel` has no effect. In that case, debug and info logs are logged depending on the values returned 
in `IsDebugEnabled` and `IsInfoEnabled`.

## Initializing OpenKit

When obtaining an OpenKit instance from the OpenKit builder the instance starts an automatic 
initialization phase. Since initialization happens asynchronously the application developers 
might want to wait until initialization completes, as shown in the example below.

```cs
// wait until the OpenKit instance is fully initialized
bool success = openKit.WaitForInitCompletion();
```

The method `WaitForInitCompletion` blocks the calling thread until OpenKit is initialized. In case
of misconfiguration this might block the calling thread indefinitely. The return value
indicates whether the OpenKit instance has been initialized or `Shutdown` has been called meanwhile.    
An overloaded method exists to wait a given amount of time for OpenKit to initialize as shown in the
following example.
```cs
// wait up to 10 seconds for OpenKit to complete initialization
int timeoutInMilliseconds = 10 * 1000;
bool success = openKit.WaitForInitCompletion(timeoutInMilliseconds);
```

The method returns `false` in case the timeout expired or `Shutdown` has been invoked in the meantime
and `true` to indicate successful initialization.  

To verify if OpenKit has been initialized, use the `IsInitialized` property as shown in the example below.
```cs
bool isInitialized = openKit.IsInitialized;
if (isInitialized)
{
    Console.WriteLine("OpenKit is initialized");
}
else
{
    Console.WriteLine("OpenKit is not yet initialized");
}
```

## Creating a Session

After setting application version and device information, which is not mandatory, but might be useful,
an `ISession` can be created by invoking the `CreateSession` method.  
There are two `CreateSession` methods:
1. Taking an IP address as string argument, which might be a valid IPv4 or IPv6 address.
If the argument is not a valid IP address a reasonable default value is used.
2. An overload taking no arguments. In this case the IP which communicates with the server is assigned
on the server.

The example shows how to create sessions.
```cs
// create a session and pass an IP address
string clientIPAddress = "12.34.56.78";
ISession sessionWithArgument = openKit.CreateSession(clientIPAddress);

// create a session and let the IP be assigned on the server side
ISession sessionWithoutArgument = openKit.CreateSession();
```

## Identify User

Users can be identified by calling `IdentifyUser` on an `ISession` instance. This enables you to search and 
filter specific user sessions and analyze individual user behavior over time in the backend.

```cs
session.IdentifyUser("jane.doe@example.com");
```

## Finishing a Session

When an `ISession` is no longer needed, it should be ended by invoking the `End` method.  
Although all open sessions are automatically ended when OpenKit is shut down (see "Terminating the OpenKit instance")
it's highly recommended to end sessions which are no longer in use manually.
```cs
session.End();
session = null; // not needed, just used to indicate that the session is no longer valid.
```

## Reporting a Crash

Unexpected application crashes can be reported via an `ISession` by invoking the `ReportCrash` method.  
The example below shows how an exception might be reported.
```cs
    private static int div(int numerator, int denominator) 
    {
        return numerator / denominator;
    }

    public static void divWithCrash() 
    {
        int numerator = 5;
        int denominator = 0;
        try 
        {
            Console.WriteLine("Got: " + div(numerator, denominator));
        }
        catch (Exception e) 
        {
            string errorName = e.GetType().ToString();
            string reason = e.Message;
            string stacktrace = e.StackTrace;
            // and now report the application crash via the session
            session.ReportCrash(errorName, reason, stacktrace);
        }
    }
```

Alternatively the `ReportCrash(Exception)` overloaded method can be used, which is provided for convenience.
The example below shows how to report an `Exception` as crash.

```cs
    private static int div(int numerator, int denominator) 
    {
        return numerator / denominator;
    }

    public static void divWithCrash() 
    {
        int numerator = 5;
        int denominator = 0;
        try 
        {
            Console.WriteLine("Got: " + div(numerator, denominator));
        }
        catch (Exception e) 
        {
            // report the caught Exception as crash
            session.ReportCrash(e);
        }
    }
```

## Starting a RootAction

As mentioned in the [README](#../README.md) root actions and actions are hierarchical named events, where
an `IRootAction` represents the first hierarchy level. An `IRootAction` can have child actions (`IAction`) and
is created from an `ISession` as shown in the example below.
```cs
string rootActionName = "rootActionName";
IRootAction rootAction = session.EnterAction(rootActionName);
```

Since `IRootAction` extends the `IAction` interface all further methods are the same for both interfaces, except
for creating child actions, which can only be done with an `IRootAction`.

## Entering a Child Action

To start a child `IAction` from a previously started `IRootAction` use the `EnterAction` method from
`IRootAction`, as demonstrated below.

```cs
string childActionName = "childActionName";
IAction childAction = rootAction.EnterAction(childActionName);
```

## Leaving Actions

To leave an `IAction` simply use the `LeaveAction` method. The method returns the parent action or `null`
if it has no parent.

```cs
IAction parentAction = action.LeaveAction(); // returns the appropriate IRootAction
IAction parent = parentAction.LeaveAction(); // will always return null
```

## Canceling Actions

Canceling an `IAction` is similar to leaving an `IAction`, except that the `IAction` will be discarded
and not reported to Dynatrace. Open child objects, like child actions and web request tracers, will be
discarded as well.
To cancel an `IAction` simply use the method `CancelAction` as shown in the example below.

```cs
IAction parentAction = action.CancelAction(); // returns the appropriate IRootAction
IAction parent = parentAction.CancelAction(); // will always return null
```

## Obtaining an Action duration

To get the `IAction` duration use the property `Duration`. The property returns the difference 
between the end time and start time, if the `IAction` is left or canceled.
If the `IAction` is still ongoing, the duration is the difference between the current time and start time.

```cs
TimeSpan duration = action.Duration; // gives current time - action start time
action.LeaveAction();
duration = action.Duration; // gives action end time - action start time
```

## Report Named Event

To report a named event use the `ReportEvent` method on `IAction`.
```cs
string eventName = "eventName";
action.ReportEvent(eventName);

// also report on the RootAction
rootAction.ReportEvent(eventName);
```

## Report Key-Value Pairs

Key-value pairs can also be reported via an `IAction` as shown in the example below.
Overloaded methods exist for the following value types:
* int
* long
* double
* string
```cs
// first report an int value
string keyIntType = "intType";
int valueInt = 42;
action.ReportValue(keyIntType, valueInt);

// let's also report a long value 
string keyLongType = "longType";
long valueLong = long.MaxValue;
action.reportValue(keyLongType, valueLong);

// then let's report a double value
string keyDoubleType = "doubleType";
double valueDouble = 3.141592653589793;
action.ReportValue(keyDoubleType, valueDouble);

// and also a string value
string keyStringType = "stringType";
string valueString = "The quick brown fox jumps over the lazy dog";
action.ReportValue(keyStringType, valueString);
```

## Report an Error

An `IAction` also has the possibility to report an error with a given 
name and error code.  
The code fragment below shows how.
```cs
string errorName = "Unknown Error";
int errorCode = 42;

action.ReportError(errorName, errorCode);
```

Errors can also be reported with the method
`IAction.ReportError(string errorName, string causeName, string causeDescription, string causeStackTrace)`, where 
* `errorName` is the name of the reported error
* `causeName` is an optional short name of the cause, typically an `Exception` class name
* `causeDescription` is an optional short description of the cause, typically `Exception.Message`
* `causeStackTrace` is an optional stack trace of the cause

The fragment below shows how to report such an error.

```cs
public void RestrictedMethod()
{
    if (!IsUserAuthorized())
    {
        // user is not authorized - report this as an error
        string errorName = "Authorization error";
        string causeName = "User not authorized";
        string causeDescription = "The current user is not authorized to call restrictedMethod.";
        string stackTrace = null; // no stack trace is reported

        action.ReportError(errorName, causeName, causeDescription, stackTrace);

        return;
    }

    // ... further processing ...
}
```

It is also possible to report a caught exception as error. This is a convenience method for the
`IAction.ReportError(string, string, string, string)` method mentioned above.

The example below demonstrates how to report an `Exception` as error.

```cs
try
{
    // call a method that is throwing an exception 
    CallMethodThrowingException();
}
catch(Exception caughtException)
{
    // report the caught exception as error via OpenKit
    string errorName = "Unknown Error";
    action.ReportError(errorName, caughtException);
}
```

## Tracing Web Requests

One of the most powerful OpenKit features is web request tracing. When the application starts a web
request (e.g. HTTP GET) a special tag can be attached to the header. This special header allows
Dynatrace SaaS/Dynatrace Managed/AppMon to correlate actions with a server side PurePath. 

A tracer instance can be obtained from an `IAction` by invoking `TraceWebRequest`. In addition to 
capture the execution time of a request, `IWebRequestTracer` defines methods to add additional metadata 
like bytes sent, bytes received, and the response code of a request.

The web request tag header has to be set to correlate web request executed in the application
with server side service calls. The field name can be obtained from `OpenKitConstants.WEBREQUEST_TAG_HEADER` 
and the field's value is obtained from the `Tag` property (see class `IWebRequestTracer`).

```cs
string url = "http://www.my-backend.com/api/v3/users";

// create the WebRequestTracer
IWebRequestTracer webRequestTracer = action.TraceWebRequest(url);

// this is the HTTP header name & value which needs to be added to the HTTP request.
string headerName = OpenKitConstants.WEBREQUEST_TAG_HEADER;
string headerValue = webRequestTracer.Tag;

// perform the request here & do not forget to add the HTTP header
using (HttpClient httpClient = new HttpClient()) {
    
    // start timing
    webRequestTracer.Start();
    
    // set the tag
    httpClient.DefaultRequestHeaders.Add(headerName, headerValue);
    
    // ... perform the request and process the response ...
    
    // set metadata
    webRequestTracer.SetBytesSent(12345);     // 12345 bytes sent
    webRequestTracer.SetBytesReceived(67890); // 67890 bytes received

    // stop timing
    webRequestTracer.Stop(200);               // 200 was the response code
}

```


## Terminating the OpenKit Instance

When an OpenKit instance is no longer needed (e.g. the application using OpenKit is shut down), the previously
obtained instance can be cleared by invoking the `Shutdown` method.  
Calling the `Shutdown` method blocks the calling thread while the OpenKit flushes data which has not been
transmitted yet to the backend (Dynatrace SaaS/Dynatrace Managed/AppMon).  
Details are explained in [internals.md](#internals.md)
