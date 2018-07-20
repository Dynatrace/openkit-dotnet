# OpenKit .NET Changelog

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
