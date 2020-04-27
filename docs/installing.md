# Installing and updating OpenKit .NET

## Prerequisites
OpenKit .NET requires .NET Framework 3.5+ or .NET Core 1.0+.

## Obtaining OpenKit .NET
OpenKit .NET is available as NuGet package on [nuget.org][nuget] and should
be used via Visual Studio's builtin NuGet package manager or via the NuGet console.

An alternative way, if automatic dependency management is not possible, is to obtain the released zip file
from [GitHub Releases][gh-releases]. Unzip the downloaded file and add the dll matching your framework as dependency.

## Updating OpenKit .NET
The recommended approach is to to update OpenKit .NET as NuGet Package from [nuget.org][nuget].

An alternative way, if automatic dependency management is not possible, is to obtain the released zip file
from [GitHub Releases][gh-releases]. Unzip the downloaded file and add the dll matching your framework as dependency.

## Release notifications
GitHub offers the possibility to receive notifications for new releases. Detailed instructions are available
on the [Watching and unwatching releases for a repository][gh-release-notification] page. 

[nuget]: https://www.nuget.org/packages/Dynatrace.OpenKit.NET/
[gh-releases]: https://github.com/Dynatrace/openkit-dotnet/releases
[gh-release-notification]: https://help.github.com/en/github/receiving-notifications-about-activity-on-github/watching-and-unwatching-releases-for-a-repository