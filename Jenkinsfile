
properties([disableConcurrentBuilds(), [$class: 'BuildDiscarderProperty', strategy: [$class: 'LogRotator', artifactDaysToKeepStr: '', artifactNumToKeepStr: '', daysToKeepStr: '', numToKeepStr: '15']]]);

def msbuildCmd="C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Professional\\MSBuild\\15.0\\Bin\\MSBuild.exe"
def nuget="C:\\nuget\\nuget.exe"
def builds = [:]

builds['Windows'] = {
    node("VS2017") {
        checkout scm

        // restore with nuget because only using msbuild.exe /t:restore doesn't do the stuff
        bat("\"${nuget}\" restore")

        bat("\"${msbuildCmd}\" /p:Configuration=Release")

        try {
            // create reports dir
            def outputDir="reports"
            bat "mkdir ${outputDir}"

            def rv = powershell(returnStatus: true, script: '''
                $testAssemblies = Get-ChildItem -Recurse -Include openkit-dotnetfull-*Tests.dll,openkit-dotnetstandard-*Tests.dll,openkit-dotnetpcl-*Tests.dll  | ? {$_.FullName -match "\\\\bin\\\\Release\\\\" } | % FullName
                packages\\\\NUnit.ConsoleRunner.3.8.0\\\\tools\\\\nunit3-console.exe --result=\"${outputDir}\\\\myresults.xml;format=nunit3\" $testAssemblies


                # Run .NET Core tests
                $testProjects = Get-ChildItem -Recurse -Include openkit-dotnetcore-*Tests.csproj  | % FullName
                foreach ($project in $testProjects)
                {
                    dotnet.exe test -c Release $project --output ${outputDir} --logger trx --no-build
                } 
            ''')
            if(rv != 0) {
                error("nunit test failed.")
            }
        } finally {
            step([$class: 'XUnitBuilder',
                thresholds: [[$class: 'FailedThreshold', unstableThreshold: '1']],
                tools: [[$class: 'JUnitType', pattern: '${outputDir}/**']]])
        }
    }
}

stage("build-and-test") {
    parallel(builds)
}