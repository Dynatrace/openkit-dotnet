param([string]$option = "Release")

if ("$env:APPVEYOR" -ieq "true") {
    $format = "AppVeyor"
    $outputDir = (Get-Location).toString()
    $nunit3_console = "nunit3-console"
} else {
    $format = "nunit3"
    $outputDir = "$env:OUTPUTDIR"
    $nunit3_console = "$env:USERPROFILE\.nuget\packages\nunit.consolerunner\3.12.0\tools\nunit3-console.exe"
}

$ErrorActionpreference = "Stop"

Write-Output $format
Write-Output $outputDir
Write-Output $nunit3_console

if ($option -eq "Release") {
    Write-Host ("Test $option")
    # Run .NET test assemblies, excluding .NET Core
    $testAssemblies = Get-ChildItem -Recurse -Path .\.build\bin\Release\Dynatrace.OpenKit.Tests -Include Dynatrace.OpenKit.Tests.dll | ? { $_.FullName -notmatch "\\netcoreapp\d\.\d\\" } | % FullName
    Write-Output $testAssemblies
    & $nunit3_console $testAssemblies "--result=$outputDir\myresults.xml;format=$format"
    # Run .NET Core tests
    & dotnet.exe test openkit-dotnet.sln --no-build --configuration Release --logger trx --results-directory $outputDir
} elseif ($option -eq "Coverage") {
    Write-Host ("Test $option")
    $openCoverConsole = "$env:USERPROFILE\.nuget\packages\opencover\4.7.922\tools\OpenCover.Console.exe"
    # Run .NET test assemblies, excluding .NET Core
    $testAssemblies = Get-ChildItem -Recurse -Path .\.build\bin\Coverage\Dynatrace.OpenKit.Tests -Include Dynatrace.OpenKit.Tests.dll | ? { $_.FullName -notmatch "\\netcoreapp\d\.\d\\" } | % FullName
    Write-Output $testAssemblies
    & $openCoverConsole "-target:$nunit3_console" "-targetargs:--result=$outputDir\myresults.xml;format=$format $testAssemblies" -register:user "-filter:+[*]Dynatrace.OpenKit*" "-hideskipped:Filter;MissingPdb" -output:$outputDir\coverage.xml
    # Run .NET Core tests
    & $openCoverConsole "-target:dotnet.exe" "-targetargs:test openkit-dotnet.sln --no-build -c Coverage" -register:user "-filter:+[*]Dynatrace.OpenKit*" "-hideskipped:Filter;MissingPdb" -oldstyle  -output:$outputDir\coverage.xml -mergeoutput
}
