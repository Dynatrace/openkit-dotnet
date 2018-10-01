
properties([disableConcurrentBuilds(), [$class: 'BuildDiscarderProperty', strategy: [$class: 'LogRotator', artifactDaysToKeepStr: '', artifactNumToKeepStr: '', daysToKeepStr: '', numToKeepStr: '15']]]);

def msbuildCmd="C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Professional\\MSBuild\\15.0\\Bin\\MSBuild.exe"
def nuget="C:\\Program Files (x86)\\NuGet\\nuget.exe"
def builds = [:]

builds['Windows'] = {
    node("VS2017") {
        timeout(activity: true, time: 15) {
            // cleanup all
            deleteDir()

            checkout scm

            // restore with nuget because only using msbuild.exe /t:restore doesn't do the stuff
            bat("\"${nuget}\" restore")

            bat("\"${msbuildCmd}\" /t:restore")

            bat("\"${msbuildCmd}\" /p:Configuration=Release")

            def outputDir="reports"
            try {
                // create reports dir        
                bat "mkdir ${outputDir}"            

                def rv = powershell(returnStatus: true, script: '''
                    $testAssemblies = Get-ChildItem -Recurse -Include openkit-dotnetfull-*Tests.dll,openkit-dotnetstandard-*Tests.dll,openkit-dotnetpcl-*Tests.dll  | ? {$_.FullName -match "\\\\bin\\\\Release\\\\" } | % FullName
                    packages\\\\NUnit.ConsoleRunner.3.8.0\\\\tools\\\\nunit3-console.exe --result=\"''' + outputDir + '''\\\\myresults.xml;format=nunit3\" $testAssemblies

                    # get workspace for restults-directory
                    $workspace=(Get-Location).toString()
                    # Run .NET Core tests
                    $testProjects = Get-ChildItem -Recurse -Include openkit-dotnetcore-*Tests.csproj  | % FullName
                    foreach ($project in $testProjects)
                    {
                        dotnet.exe test -c Release $project --results-directory $workspace\\\\''' + outputDir + ''' --logger trx --no-build
                    } 
                ''')
                if(rv != 0) {
                    error("nunit test failed.")
                }
            } finally {
                xunit testTimeMargin: '3000', thresholdMode: 1, thresholds: [], 
                    tools: [NUnit3(deleteOutputFiles: true, failIfNotNew: true, pattern: "${outputDir}/*.xml", skipNoTestFiles: false, stopProcessingIfError: true),
                            MSTest(deleteOutputFiles: true, failIfNotNew: true, pattern: "${outputDir}/*.trx", skipNoTestFiles: false, stopProcessingIfError: true)]
            }
        }
    }
}

stage("build-and-test") {
    parallel(builds)
}