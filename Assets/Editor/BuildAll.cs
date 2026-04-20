using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BuildAll
{
    static string[] Scenes => EditorBuildSettings.scenes
        .Where(s => s.enabled)
        .Select(s => s.path)
        .ToArray();

    [MenuItem("Build/Mac")]
    public static void BuildMac()
    {
        BuildPlayerOptions opt = new BuildPlayerOptions
        {
            scenes           = Scenes,
            locationPathName = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "MMH-Build/MoonlightMagicHouse.app"),
            target           = BuildTarget.StandaloneOSX,
            options          = BuildOptions.None
        };
        BuildPipeline.BuildPlayer(opt);
    }

    [MenuItem("Build/WebGL")]
    public static void BuildWebGL()
    {
        BuildPlayerOptions opt = new BuildPlayerOptions
        {
            scenes           = Scenes,
            locationPathName = "Builds/WebGL",
            target           = BuildTarget.WebGL,
            options          = BuildOptions.None
        };
        BuildPipeline.BuildPlayer(opt);
    }

    [MenuItem("Build/iOS")]
    public static void BuildIOS()
    {
        BuildPlayerOptions opt = new BuildPlayerOptions
        {
            scenes           = Scenes,
            locationPathName = "Builds/iOS",
            target           = BuildTarget.iOS,
            options          = BuildOptions.None
        };
        BuildPipeline.BuildPlayer(opt);
    }
}
