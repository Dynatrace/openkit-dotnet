# Dynatrace OpenKit - .NET Example

The following document provides an in depth overview, how OpenKit can be used from
developer's point of view. It explains the usage of all the API methods.  

To make it more obvious for the reader which data types are used, all examples below
avoid C#'s auto type deduction feature.

## Obtaining an OpenKit Instance

OpenKit instances are obtained from the `OpenKitFactory` class.  
Depending on the used backend system (Dynatrace SaaS/Dynatrace Managed/AppMon), the factory provides 
different methods to create a new  OpenKit instance. Despite from this, the developer does not 
need to distinguish between different backend systems.

### Dynatrace SaaS
 
```cs
string applicationName = "My OpenKit application";
string applicationID = "application-id";
long deviceID = 42L;
string endpointURL = "https://tenantid.beaconurl.com";

// by default verbose logging is disabled
IOpenKit openKit = OpenKitFactory.CreateDynatraceInstance(applicationName, applicationID, deviceID, endpointURL);
```

* The `applicationName` parameter is the application's name created before in Dynatrace SaaS.
* The `applicationID` parameter is the unique identifier of the application in Dynatrace Saas. The
application's id can be found in the settings page of the custom application in Dynatrace.
* The `deviceID` is a unique identifier, which might be used to uniquely identify a device.
* The `endpointURL` denotes the Dynatrace SaaS cluster endpoint OpenKit communicates with and 
  is shown when creating the application in Dynatrace SaaS.
The endpoint URL can be found in the settings page of the custom application in Dynatrace.

OpenKit provides extended log output by activating the verbose mode. This feature might come in quite handy during development,
therefore an overloaded method exists, where verbose mode can be enabled or disabled.  
To enable verbose mode, use the following example.

```cs
string applicationName = "My OpenKit application";
string applicationID = "application-id";
long deviceID = 42L;
string endpointURL = "https://tenantid.beaconurl.com";
bool verbose = true;

// by default verbose logging is disabled
IOpenKit openKit = OpenKitFactory.CreateDynatraceInstance(applicationName, applicationID, deviceID, endpointURL, verbose);
```

### Dynatrace Managed

An OpenKit instance for Dynatrace Managed can be obtained in a similar manner, as shown in the example below.
```cs
string applicationName = "My OpenKit application";
string applicationID = "application-id";
long deviceID = 42L;
string endpointURL = "https://tenantid.beaconurl.com";
string tenantID = "tenant-id";

// by default verbose logging is disabled
IOpenKit openKit = OpenKitFactory.CreateDynatraceManagedInstance(applicationName, applicationID, deviceID, endpointURL, tenantID);
```

* The `applicationName` parameter is the application's name created before in Dynatrace Managed.
* The `applicationID` parameter is the unique identifier of the application in Dynatrace Managed. The
application's id can be found in the settings page of the custom application in Dynatrace.
* The `deviceID` is a unique identifier, which might be used to uniquely identify a device.
* The `endpointURL` denotes the Dynatrace Managed endpoint OpenKit communicates with. The endpoint URL can be found in 
the settings page of the custom application in Dynatrace.
* The `tenantID` is the tenant used by Dynatrace Managed.

Again an overloaded method exists to enable verbose logging, as shown below.
```cs
string applicationName = "My OpenKit application";
string applicationID = "application-id";
long deviceID = 42L;
string endpointURL = "https://beaconurl.com";
string tenantID = "tenant-id";
bool verbose = true;

// by default verbose logging is disabled
IOpenKit openKit = OpenKitFactory.CreateDynatraceManagedInstance(applicationName, applicationID, deviceID, endpointURL, tenantID, verbose);
```

### AppMon

The example below demonstrates how to connect an OpenKit application to an AppMon endpoint.
```cs
string applicationName = "My OpenKit application";
long deviceID = 42L;
string endpointURL = "https://beaconurl.com";

// by default verbose logging is disabled
OpenKit openKit = OpenKitFactory.CreateAppMonInstance(applicationName, deviceID, endpointURL);
```

* The `applicationName` parameter is the application's name in AppMon and is also used as the application's id.
* The `deviceID` is a unique identifier, which might be used to uniquely identify a device.
* The `endpointURL` denotes the AppMon endpoint OpenKit communicates with.

If verbose OpenKit logging output is wanted, an overloaded method can be used as demonstrated below.
```cs
string applicationName = "My OpenKit application";
long deviceID = 42L;
string endpointURL = "https://tenantid.beaconurl.com";
bool verbose = true;

// by default verbose logging is disabled
IOpenKit openKit = OpenKitFactory.CreateAppMonInstance(applicationName, deviceID, endpointURL, verbose);
```

## SSL/TLS Security in OpenKit

All OpenKit internal communication to the backend happens via HTTPS (TLS/SSL based on .NET Framework support).
By default OpenKit expects valid server certificates.
However it is possible, if really needed, to bypass TLS/SSL certificate validation. This can be achieved by
passing an implementation of `ISSLTrustManager` to the previously mentioned OpenKit factory methods.

<aside class="warning">
We do **NOT** recommend bypassing TLS/SSL server certificate validation, since this opens
a vulnerability to man-in-the-middle attacks.
</aside>

Keep in mind on .NET 3.5 and 4.0 server certificate validation can only be overwritten on global level via
the `ServicePointManager`. In versions .NET 4.5+ overwriting happens on request basis.  

## Initializing OpenKit

When obtaining an OpenKit instance from `OpenKitFactory` the instance starts an automatic 
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
// wait 10 seconds for OpenKit to complete initialization
int timeoutInMilliseconds = 10 * 1000;
bool success = openKit.WaitForInitCompletion(timeoutInMilliseconds);
```

The method returns `false` in case the timeout expired or `Shutdown` has been invoked in the meantime
and `true` to indicate successful initialization.  

To verify if OpenKit has been initialized, use the `IsInitialized` property as shown in the example below.
```cs
bool isInitialized = openKit.IsInitialized;
if (isInitialized) {
    System.out.println("OpenKit is initialized");
} else {
    System.out.println("OpenKit is not yet initialized");
}
```

## Providing further Application Information

If multiple version's of the same applications are monitored by OpenKit, it's quite useful
to set the application's version in OpenKit.  
This can be achieved by setting the `ApplicationVersion` value.
```cs
string applicationVersion = "1.2.3.4";
openKit.ApplicationVersion = applicationVersion;
```

## Providing Device specific Information

Sometimes it might also be quite useful to provide information about the device the application
is running on. The example below shows how to achieve this.
```cs
// set operating system
string operatingSystem = "Custom OS";
openKit.Device.OperatingSystem = operatingSystem;

// set device manufacturer
string deviceManufacturer = "ACME Inc.";
openKit.Device.Manufacturer = deviceManufacturer;

// set device/model identifier
string deviceID = "12-34-56-78-90";
openKit.Device.ModelID = deviceID;
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
    private static int div(int numerator, int denominator) {
        return numerator / denominator;
    }

    public static void divWithCrash() {
        int numerator = 5;
        int denominator = 0;
        try {
            Console.WriteLine("Got: " + div(numerator, denominator));
        } catch (Exception e) {
            string errorName = e.GetType().ToString();
            string reason = e.Message;
            string stacktrace = e.StackTrace;
            // and now report the application crash via the session
            session.reportCrash(errorName, reason, stacktrace);
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

To leave an `IAction` simply use the `Leave` method. The method returns the parent action or `null`
if it has no parent.

```cs
IAction parentAction = action.Leave(); // returns the appropriate RootAction
IAction parent = parentAction.Leave(); // will always return null
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
* double
* string
```cs
// first report an int value
string keyIntType = "intType";
int valueInt = 42;
action.ReportValue(keyIntType, valueInt);

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

An `Action` also has the possibility to report an error with a given 
name, code and a reason. The code fragment below shows how.
```cs
string errorName = "Unknown Error";
int errorCode = 42;
string reason = "Not sure what's going on here";

action.ReportError(errorName, errorCode, reason);
```

## Tracing Web Requests

One of the most powerful OpenKit features is web request tracing. When the application starts a web
request (e.g. HTTP GET) a special tag can be attached to the header. This special header allows
Dynatrace SaaS/Dynatrace Managed/AppMon to correlate actions with a server side PurePath. 

Since .NET 3.5 and 4.0 do not support `HttpClient`, the `WebClient` class is supported for those two
versions. `HttpClient` is supported for .NET 4.5 and above.

The following example demonstrates how to trace web requests using `WebClient`.
```cs

byte[] downloadedData;
string url = "http://www.my-backend.com/api/v3/users";

// create the WebClient
using (WebClient webClient = new WebClient()) 
{
    // create WebRequestTracer and start timing
    IWebRequestTracer webRequestTracer = action.TraceWebRequest(webClient);
    webRequestTracer.StartTiming();

    // donwload data
    downloadedData = client.DownloadData(url);

    // stop timing
    webRequestTracer.StopTiming();
}

// do something useful with downloadedData

```

This example demonstrates how to trace web requesting using `HttpClient`, used for .NET 4.5 and above.
```cs

string downloadedData;
string url = "http://www.my-backend.com/api/v3/users";

// ... Use HttpClient.
using (HttpClient httpClient = new HttpClient())
{
    // create WebRequestTracer and start timing 
    IWebRequestTracer webRequestTracer = action.TraceWebRequest(httpClient);
    webRequestTracer.StartTiming();

    using (HttpResponseMessage response = await httpClient.GetAsync(url))
    {
        // set response code
        webRequestTracer.ResponseCode = response.StatusCode;

        using (HttpContent content = response.Content)
        {
            // ... Read the string.
            downloadedData = await content.ReadAsStringAsync();
        }
    }

    // stop timing
    webRequestTracer.StopTiming();
}

// do something useful with downloadedData

```

If a third party lib is used for HTTP requests, the developer has the possibility to use an overloaded
`TraceWebRequest` method, taking only the URL string as argument. However when using this overloaded
method the developer is responsible for adding the appropriate header field to the request.  
The field name can be obtained from `OpenKitFactory.WEBREQUEST_TAG_HEADER` and the field's value is obtained
from `Tag` property (see class `WebRequestTracer`).

```cs
string url = "http://www.my-backend.com/api/v3/users";

// create the WebRequestTracer
IWebRequestTracer webRequestTracer = action.TraceWebRequest(url);

// this is the HTTP header name & value which needs to be added to the HTTP request.
string headerName = OpenKitFactory.WEBREQUEST_TAG_HEADER;
string headerValue = webRequestTracer.Tag;

webRequestTracer.StartTiming();

// perform the request here & do not forget to add the HTTP header

webRequestTracer.ResponseCode = 200;
webRequestTracer.StopTiming();
```


## Terminating the OpenKit Instance

When an OpenKit instance is no longer needed (e.g. the application using OpenKit is shut down), the previously
obtained instance can be cleared by invoking the `Shutdown` method.  
Calling the `Shutdown` method blocks the calling thread while the OpenKit flushes data which has not been
transmitted yet to the backend (Dynatrace SaaS/Dynatrace Managed/AppMon).  
Details are explained in [internals.md](#internals.md)