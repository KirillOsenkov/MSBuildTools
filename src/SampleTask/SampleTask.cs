using System.Threading;
using Microsoft.Build.Utilities;

// How to run:
// set MSBUILDNOINPROCNODE=0
// C:\msbuild\artifacts\bin\bootstrap\net472\MSBuild\Current\Bin\MSBuild.exe C:\MSBuildTools\src\SampleTask\sample.proj /nr:false /bl
public class SampleTask : Task
{
    public string StringParam { get; set; }

    public override bool Execute()
    {
        var projectPath = BuildEngine.ProjectFileOfTaskNode;
        Thread.Sleep(10000);
        return !Log.HasLoggedErrors;
    }
}