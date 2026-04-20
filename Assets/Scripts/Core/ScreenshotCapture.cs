using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoonlightMagicHouse
{
    // Auto-capture screenshots on evolution / achievement for marketing / QA
    public class ScreenshotCapture : MonoBehaviour
    {
        [SerializeField] bool captureOnEvolution = true;
        [SerializeField] bool captureOnAchievement = true;

        void Start()
        {
            var ml = MoonlightGameManager.Instance?.moonlight;
            if (captureOnEvolution && ml != null)
                ml.onStageUp.AddListener(_ => Capture("evolution"));

            if (captureOnAchievement && AchievementSystem.Instance != null)
                AchievementSystem.Instance.onUnlocked.AddListener(a => Capture($"ach_{a.id}"));
        }

        public void Capture(string tag = "screenshot")
        {
            string dir  = Path.Combine(Application.persistentDataPath, "Screenshots");
            Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, $"{tag}_{System.DateTime.Now:yyyyMMdd_HHmmss}.png");
            ScreenCapture.CaptureScreenshot(file);
            Debug.Log($"[Screenshot] {file}");
        }
    }
}
