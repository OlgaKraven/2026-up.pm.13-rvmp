using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class PracticeBuild
{
    [MenuItem("Practice/Build standalone demonstration")]
    public static void Build()
    {
        string folder=Path.GetFullPath("../verification");Directory.CreateDirectory(folder);
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes=new[]{"Assets/Scenes/Demonstration.unity"},
            locationPathName=Path.Combine(folder,"CrystalRoute.app"),
            target=BuildTarget.StandaloneOSX,
            options=BuildOptions.None
        });
        File.WriteAllText(Path.Combine(folder,"standalone-build.txt"),report.summary.result+"\nErrors: "+report.summary.totalErrors+"\n");
        Debug.Log("STANDALONE_BUILD: "+report.summary.result);
    }
}
