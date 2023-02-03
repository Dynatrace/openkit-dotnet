param([string]$option = "Release")

$format = "nunit3"
$outputDir = "$env:OUTPUTDIR"
$nunit3_console = "$env:USERPROFILE\.nuget\packages\nunit.consolerunner\3.12.0\tools\nunit3-console.exe"

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
}
