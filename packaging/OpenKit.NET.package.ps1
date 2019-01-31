# Powershell script to package OpenKit .NET into a NuGet package
# Keep in mind, that building must be done before

$packagingFolderName = Join-Path -Path "$PSScriptRoot" -ChildPath "OpenKit.NET.package"

# prepare temporary folder
Remove-Item $packagingFolderName -Recurse -ErrorAction Ignore
New-Item -ItemType directory -Path $packagingFolderName | Out-Null

# prepare content directory
$packagingContentFolderName = Join-Path -Path $packagingFolderName -ChildPath "content"
New-Item -ItemType directory -Path $packagingContentFolderName | Out-Null

# copy license
Copy-Item "$PSScriptRoot\..\LICENSE" -Destination "$packagingFolderName\LICENSE.txt"

# Copy documentation
Copy-Item "$PSScriptRoot\..\" -Filter "*.md" -Destination $packagingContentFolderName
Copy-Item "$PSScriptRoot\..\docs" -Destination $packagingContentFolderName -Recurse

# Top level directory for libraries
# Further reading:
# - https://docs.microsoft.com/en-us/nuget/create-packages/supporting-multiple-target-frameworks
# - https://docs.microsoft.com/en-us/nuget/reference/target-frameworks
$packagingLibFolderName = Join-Path -Path $packagingFolderName -ChildPath "lib"
New-Item -ItemType directory -Path $packagingLibFolderName | Out-Null

# output for .NET Core 1.0
$dotNetCore10Name = 'netcoreapp1.0'
New-Item -ItemType directory -Path $packagingLibFolderName -Name $dotNetCore10Name | Out-Null
Get-ChildItem "$PSScriptRoot\..\src\openkit-dotnetcore-1.0\bin\Release\$dotNetCore10Name" | ForEach {
    $newName = $_.Name -replace "^openkit-.*dotnetcore-1\.0", "openkit"
    Copy-Item $_.FullName "$packagingLibFolderName\$dotNetCore10Name\$newName"
}

# output for .NET Core 1.1
$dotNetCore11Name = 'netcoreapp1.1'
New-Item -ItemType directory -Path $packagingLibFolderName -Name $dotNetCore11Name | Out-Null
Get-ChildItem "$PSScriptRoot\..\src\openkit-dotnetcore-1.1\bin\Release\$dotNetCore11Name" | ForEach {
    $newName = $_.Name -replace "^openkit-.*dotnetcore-1\.1", "openkit"
    Copy-Item $_.FullName "$packagingLibFolderName\$dotNetCore11Name\$newName"
}

# output for .NET Core 2.0
$dotNetCore20Name = 'netcoreapp2.0'
New-Item -ItemType directory -Path $packagingLibFolderName -Name $dotNetCore20Name | Out-Null
Get-ChildItem $PSScriptRoot\..\src\openkit-dotnetcore-2.0\bin\Release\$dotNetCore20Name | ForEach {
    $newName = $_.Name -replace "^openkit-.*dotnetcore-2\.0", "openkit"
    Copy-Item $_.FullName "$packagingLibFolderName\$dotNetCore20Name\$newName"
}

# output for .NET 3.5
$dotNetFull35Name = 'net35'
New-Item -ItemType directory -Path $packagingLibFolderName -Name $dotNetFull35Name | Out-Null
Get-ChildItem $PSScriptRoot\..\src\openkit-dotnetfull-3.5\bin\Release | ForEach {
    $newName = $_.Name -replace "^openkit-.*dotnetfull-3\.5", "openkit"
    Copy-Item $_.FullName "$packagingLibFolderName\$dotNetFull35Name\$newName"
}

# output for .NET 4.0
$dotNetFull40Name = 'net40'
New-Item -ItemType directory -Path $packagingLibFolderName -Name $dotNetFull40Name | Out-Null
Get-ChildItem $PSScriptRoot\..\src\openkit-dotnetfull-4.0\bin\Release | ForEach {
    $newName = $_.Name -replace "^openkit-.*dotnetfull-4\.0", "openkit"
    Copy-Item $_.FullName "$packagingLibFolderName\$dotNetFull40Name\$newName"
}

# output for .NET 4.5
$dotNetFull45Name = 'net45'
New-Item -ItemType directory -Path $packagingLibFolderName -Name $dotNetFull45Name | Out-Null
Get-ChildItem $PSScriptRoot\..\src\openkit-dotnetfull-4.5\bin\Release | ForEach {
    $newName = $_.Name -replace "^openkit-.*dotnetfull-4\.5", "openkit"
    Copy-Item $_.FullName "$packagingLibFolderName\$dotNetFull45Name\$newName"
}

# output for .NET 4.6
$dotNetFull46Name = 'net46'
New-Item -ItemType directory -Path $packagingLibFolderName -Name $dotNetFull46Name | Out-Null
Get-ChildItem $PSScriptRoot\..\src\openkit-dotnetfull-4.6\bin\Release | ForEach {
    $newName = $_.Name -replace "^openkit-.*dotnetfull-4\.6", "openkit"
    Copy-Item $_.FullName "$packagingLibFolderName\$dotNetFull46Name\$newName"
}

# output for .NET 4.7
$dotNetFull47Name = 'net47'
New-Item -ItemType directory -Path $packagingLibFolderName -Name $dotNetFull47Name | Out-Null
Get-ChildItem $PSScriptRoot\..\src\openkit-dotnetfull-4.7\bin\Release | ForEach {
    $newName = $_.Name -replace "^openkit-.*dotnetfull-4\.7", "openkit"
    Copy-Item $_.FullName "$packagingLibFolderName\$dotNetFull47Name\$newName"
}

# output for PCL 4.5 (Profile 111)
$dotNetPCL45Name = 'portable-net45+win8+wpa81'
New-Item -ItemType directory -Path $packagingLibFolderName -Name $dotNetPCL45Name | Out-Null
Get-ChildItem $PSScriptRoot\..\src\openkit-dotnetpcl-4.5\bin\Release | ForEach {
    $newName = $_.Name -replace "^openkit-.*dotnetpcl-4\.5", "openkit"
    Copy-Item $_.FullName "$packagingLibFolderName\$dotNetPCL45Name\$newName"
}

# output for .NET Standard 2.0
$dotNetStandard20Name = 'netstandard2.0'
New-Item -ItemType directory -Path $packagingLibFolderName -Name $dotNetStandard20Name | Out-Null
Get-ChildItem $PSScriptRoot\..\src\openkit-dotnetstandard-2.0\bin\Release\$dotNetStandard20Name | ForEach {
    $newName = $_.Name -replace "^openkit-.*dotnetstandard-2\.0", "openkit"
    Copy-Item $_.FullName "$packagingLibFolderName\$dotNetStandard20Name\$newName"
}

# output for Universal Windows Platform
$dotNetUWPName = 'uap10.0'
New-Item -ItemType directory -Path $packagingLibFolderName -Name $dotNetUWPName | Out-Null
Get-ChildItem $PSScriptRoot\..\src\openkit-dotnet-uwp\bin\Release | ForEach {
    $newName = $_.Name -replace "^openkit-.*dotnet-uwp", "openkit"
    Copy-Item $_.FullName "$packagingLibFolderName\$dotNetUWPName\$newName"
}
