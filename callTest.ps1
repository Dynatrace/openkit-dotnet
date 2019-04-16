param([string]$option = "Release")

if(Test-Path env.APPVEYOR_API_URL) {
    $msbuild = "msbuild.exe"
    $format = "AppVeyor"
    $outputDir = (Get-Location).toString()
    $nunit3_console = "nunit3-console"
} else {
    $msbuild = "$env:MSBUILDEXE"
    $format = "nunit3"
    $outputDir = "$env:OUTPUTDIR"
    $nunit3_console = "$env:USERPROFILE\.nuget\packages\nunit.consolerunner\3.10.0\tools\nunit3-console.exe"
}

echo $msbuild
echo $format
echo $outputDir
echo $nunit3_console

if($option -eq "Release") {
    Write-Host ("Test $option")
    # Run .NET test assemblies, excluding .NET Core
    $testAssemblies = Get-ChildItem -Recurse -Path .\.build\bin\Release\Dynatrace.OpenKit.Tests -Include Dynatrace.OpenKit.Tests.dll | ? {$_.FullName -notmatch "\\netcoreapp\d\.\d\\"}  | % FullName
    echo $testAssemblies
    & $nunit3_console $testAssemblies "--result=$outputDir\myresults.xml;format=$format"
    # Run .NET Core tests
    & dotnet.exe test openkit-dotnet.sln --no-build --configuration Release --logger trx --results-directory $outputDir
} elseif($option -eq "Coverage") {
    Write-Host ("Test $option")
    $openCoverConsole = "$env:USERPROFILE\.nuget\packages\opencover\4.7.922\tools\OpenCover.Console.exe"
    # Run .NET test assemblies, excluding .NET Core
    $testAssemblies = Get-ChildItem -Recurse -Path .\.build\bin\Coverage\Dynatrace.OpenKit.Tests -Include Dynatrace.OpenKit.Tests.dll | ? {$_.FullName -notmatch "\\netcoreapp\d\.\d\\"}  | % FullName
    & $openCoverConsole -target:"nunit3-console" -targetargs:"'--result=myresults.xml;format=$format' $testAssemblies" -register:user -filter:"+[*]Dynatrace.OpenKit* -[*.Tests]*" -hideskipped:"Filter;MissingPdb" -output:$outputDir\coverage.xml
    # Run .NET Core tests
    & $openCoverConsole -target:"dotnet.exe" -targetargs:"test openkit-dotnet.sln --no-build -c Coverage" -register:user  -filter:"+[*]Dynatrace.OpenKit* -[*.Tests]*" -hideskipped:"Filter;MissingPdb" -oldstyle  -output:$outputDir\coverage.xml -mergeoutput
}