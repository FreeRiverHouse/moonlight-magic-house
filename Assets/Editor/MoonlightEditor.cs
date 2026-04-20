using UnityEngine;
using UnityEditor;

namespace MoonlightMagicHouse.Editor
{
    [CustomEditor(typeof(MoonlightCharacter))]
    public class MoonlightCharacterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var ml = (MoonlightCharacter)target;

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("── Debug Controls ──", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Feed (free)"))
            {
                var f = ScriptableObject.CreateInstance<FoodItem>();
                f.hungerBoost = 30;
                f.warmthBoost = 10;
                f.xpReward    = 10;
                ml.Feed(f);
                DestroyImmediate(f);
            }
            if (GUILayout.Button("Cuddle"))    ml.Cuddle();
            if (GUILayout.Button("Sleep"))     ml.PutToSleep();
            if (GUILayout.Button("Explore"))   ml.Explore(RoomType.Garden);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+100 XP"))
                typeof(MoonlightCharacter)
                    .GetMethod("GainXP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(ml, new object[] { 100 });
            if (GUILayout.Button("+50 Stars"))  ml.EarnCoins(50);
            if (GUILayout.Button("Force Stage"))
                typeof(MoonlightCharacter)
                    .GetMethod("GainXP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(ml, new object[] { 2000 });
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField($"Mood:  {ml.stats.GetMood()}", EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Stage: {ml.stage}", EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Days:  {ml.daysInHouse:F2}", EditorStyles.helpBox);
        }
    }

    public static class MoonlightMenuItems
    {
        [MenuItem("MoonlightHouse/Open Data Folder")]
        static void OpenDataFolder() =>
            EditorUtility.RevealInFinder(Application.dataPath + "/Data");

        [MenuItem("MoonlightHouse/Clear Save Data")]
        static void ClearSave()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("[MoonlightHouse] PlayerPrefs cleared.");
        }

        [MenuItem("MoonlightHouse/Run Blender Asset Gen")]
        static void RunBlender()
        {
            var sh = System.IO.Path.Combine(Application.dataPath, "..", "BlenderAssets", "run_blender_gen.sh");
            var proc = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName  = "/bin/zsh",
                    Arguments = $"-c \"{sh}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                }
            };
            proc.Start();
            Debug.Log("[MoonlightHouse] Blender asset gen started — check Console for output.");
        }
    }
}
