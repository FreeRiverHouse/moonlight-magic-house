using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MoonlightAutoPlayQA
{
    [MenuItem("MoonlightHouse/QA/Setup and Play")]
    public static void SetupAndPlay()
    {
        string scenePath = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .FirstOrDefault();
        if (string.IsNullOrEmpty(scenePath))
            throw new InvalidOperationException(
                "Moonlight QA requires at least one enabled build scene.");

        EditorSceneManager.OpenScene(scenePath);
        Debug.Log("[MoonlightAutoPlayQA] Opening " + scenePath +
            " and entering Play Mode.");
        EditorApplication.isPlaying = true;
    }
}
