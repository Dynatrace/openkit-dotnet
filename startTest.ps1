 $testAssemblies = Get-ChildItem -Recurse -Include openkit-dotnetfull-*Tests.dll,openkit-dotnetstandard-*Tests.dll,openkit-dotnetpcl-*Tests.dll  | ? {$_.FullName -match "\\bin\\Release\\" } | % FullName
 packages\\NUnit.ConsoleRunner.3.8.0\\tools\\nunit3-console.exe --result=myresults.xml --format=nunit3 $testAssemblies


# Run .NET Core tests
$testProjects = Get-ChildItem -Recurse -Include openkit-dotnetcore-*Tests.csproj  | % FullName
foreach ($project in $testProjects)
{
    dotnet.exe test -c Release $project --no-build
} 