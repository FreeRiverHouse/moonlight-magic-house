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
        ExtractMixamoTextures();
        BuildPlayerOptions opt = new BuildPlayerOptions
        {
            scenes           = Scenes,
            locationPathName = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "MMH-Build/MoonlightMagicHouse.app"),
            target           = BuildTarget.StandaloneOSX,
            options          = BuildOptions.None
        };
        BuildPipeline.BuildPlayer(opt);
    }

    static void ExtractMixamoTextures()
    {
        string[] folders = { "Assets/Resources/Kenney", "Assets/Resources/Mixamo" };
        foreach (var folder in folders)
        {
            if (!AssetDatabase.IsValidFolder(folder)) continue;
            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { folder });
            foreach (var guid in guids)
            {
                string fbxPath = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
                if (importer == null) continue;
                string dir = System.IO.Path.GetDirectoryName(fbxPath);
                string texFolder = dir + "/Textures";
                if (!AssetDatabase.IsValidFolder(texFolder))
                    AssetDatabase.CreateFolder(dir, "Textures");
                string matFolder = dir + "/Materials";
                if (!AssetDatabase.IsValidFolder(matFolder))
                    AssetDatabase.CreateFolder(dir, "Materials");
                importer.animationType = ModelImporterAnimationType.Legacy;
                importer.importAnimation = true;
                try
                {
                    importer.ExtractTextures(texFolder);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[ExtractMixamo] Texture extract skipped for {fbxPath}: {ex.Message}");
                }
                AssetDatabase.WriteImportSettingsIfDirty(fbxPath);
                AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceUpdate);
                Debug.Log($"[ExtractMixamo] Prepared textures + Legacy anim for {fbxPath}");
            }
        }
        AssetDatabase.Refresh();
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
