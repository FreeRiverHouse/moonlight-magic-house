using UnityEngine;
using UnityEditor;

namespace MoonlightMagicHouse.Editor
{
    [CustomEditor(typeof(MoonlightPet))]
    public class MoonlightPetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var pet = (MoonlightPet)target;

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("── Debug Controls ──", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Feed (free)"))
            {
                var f = ScriptableObject.CreateInstance<FoodItem>();
                f.hungerBoost    = 30;
                f.happinessBoost = 10;
                f.xpReward       = 10;
                pet.Feed(f);
                DestroyImmediate(f);
            }
            if (GUILayout.Button("Play")) pet.Play(CreateDebugActivity());
            if (GUILayout.Button("Sleep")) pet.Sleep();
            if (GUILayout.Button("Clean")) pet.Clean();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+100 XP"))   pet.GetType()
                .GetMethod("GainXP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(pet, new object[] { 100 });
            if (GUILayout.Button("+50 Coins"))  pet.EarnCoins(50);
            if (GUILayout.Button("Force Evolve"))
            {
                var f = ScriptableObject.CreateInstance<FoodItem>();
                f.xpReward = 2000;
                pet.Feed(f);
                DestroyImmediate(f);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField($"Mood: {pet.stats.GetMood()}", EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Age: {pet.ageMinutes:F1} min", EditorStyles.helpBox);
        }

        static ActivityItem CreateDebugActivity()
        {
            var a = ScriptableObject.CreateInstance<ActivityItem>();
            a.happinessBoost = 20;
            a.energyCost     = 10;
            a.xpReward       = 15;
            return a;
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
