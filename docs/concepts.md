# General Concepts

In this part the concepts used throughout OpenKit are explained. A short sample how to use OpenKit is
also provided. For detailed code samples have a look into [example.md][example].

## DynatraceOpenKitBuilder
A `DynatraceOpenKitBuilder` instance is responsible for setting 
application relevant information, e.g. the application's version and device specific information, and to create
an `IOpenKit` instance.

## OpenKit
The OpenKit is responsible for creating user sessions (see [ISession](#session)).

Although it would be possible to have multiple `IOpenKit` instances connected to the same endpoint
(Dynatrace) within one process, there should be one unique instance. `IOpenKit` is designed to be
thread safe and therefore the instance can be shared among threads.  

On application shutdown, `Shutdown()` needs to be called on the OpenKit instance.

## Session

An `ISession` represents kind of a user session, similar to a browser session in a web application.
However the application developer is free to choose how to treat an `ISession`.  
The `ISession` is used to create `IRootAction` instances, report application crashes or for tracing web requests.

When an `ISession` is no longer required, it's highly recommended to end it, using the `ISession.End()` method. 

## RootAction and Action

The `IRootAction` and `IAction` are named hierarchical nodes for timing and attaching further details.
An `IRootAction` is created from the `ISession` and it can create `IAction` instances. Both, `IRootAction` and
`IAction`, provide the possibility to attach key-value pairs, named events and errors, and can be used 
for tracing web requests.

When an `IRootAction` or `IAction` is no longer required, it's highly recommended to close it, using the `IAction::LeaveAction()` or
`IRootAction::LeaveAction()` method.

## WebRequestTracer

When the application developer wants to trace a web request, which is served by a service 
instrumented by Dynatrace, an `IWebRequestTracer` should be used, which can be
requested from an `ISession` or an `IAction`.  

## Named Events

A named `Event` is attached to an `IAction` and contains a name.

## Key-Value Pairs

For an `IAction` key-value pairs can also be reported. The key is always a string
and the value may be an integer (int), a floating point (double) or a string.

## Errors & Crashes

Errors are a way to report an erroneous condition on an `IAction`.  
Crashes are used to report (unhandled) exceptions on an `ISession`.

## Identify Users

OpenKit enables you to tag sessions with unique user tags. The user tag is a string 
that allows to uniquely identify a single user.

## Example

This small example provides a rough overview how OpenKit can be used.  
Detailed explanation is available in [example.md][example].

```cs
string applicationID = "application-id";                       // Your application's ID
long deviceID = 42L;                                           // Replace with a unique value per device/installation
string endpointURL = "https://tenantid.beaconurl.com/mbeacon"; // Dynatrace endpoint URL

IOpenKit openKit = new DynatraceOpenKitBuilder(endpointURL, applicationID, deviceID)
    .WithApplicationVersion("1.0.0.0")
    .WithOperatingSystem("Windows 10")
    .WithManufacturer("MyCompany")
    .WithModelId("MyModelId")
    .Build();

// Wait up to 10 seconds for OpenKit to complete initialization
int timeoutInMilliseconds = 10 * 1000;
bool success = openKit.WaitForInitCompletion(timeoutInMilliseconds);

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

[example]: ./example.md
