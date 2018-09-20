
properties([disableConcurrentBuilds(), [$class: 'BuildDiscarderProperty', strategy: [$class: 'LogRotator', artifactDaysToKeepStr: '', artifactNumToKeepStr: '', daysToKeepStr: '', numToKeepStr: '15']]]);

def msbuildCmd="C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Professional\\MSBuild\\15.0\\Bin\\MSBuild.exe"
def builds = [:]

builds['Windows'] = {
    node("VS2017") {
        checkout scm

        bat("${msbuildCmd} /p:Configuration=Release")

        try {
            //

            def stdout = powershell(returnStdout: true, script: '''
                $testAssemblies = Get-ChildItem -Recurse -Include openkit-dotnetfull-*Tests.dll,openkit-dotnetstandard-*Tests.dll,openkit-dotnetpcl-*Tests.dll  | ? {$_.FullName -match "\\bin\\Release\\" } | % FullName
                packages\\NUnit.ConsoleRunner.3.8.0\\tools\\nunit3-console.exe --result=myresults.xml --format=nunit3 $testAssemblies


                # Run .NET Core tests
                $testProjects = Get-ChildItem -Recurse -Include openkit-dotnetcore-*Tests.csproj  | % FullName
                foreach ($project in $testProjects)
                {
                    dotnet.exe test -c Release $project --no-build
                } 
            ''')
            println stdout

        } finally {
            junit("myresults.xml")
        }
    }
}

stage("build-and-test") {
    parallel(builds)
}