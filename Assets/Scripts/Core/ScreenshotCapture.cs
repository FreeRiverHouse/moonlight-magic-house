using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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

        IEnumerator Start()
        {
            if (HasArg("-mmhPlaytest"))
                StartCoroutine(PlaytestForQA());
            else if (HasArg("-mmhCapture"))
                StartCoroutine(CaptureForQA());

            var args = System.Environment.GetCommandLineArgs();
            if (args.Contains("-moonlightVisualQa"))
            {
                int pathIndex = System.Array.IndexOf(args, "-moonlightVisualQaPath");
                string file = pathIndex >= 0 && pathIndex + 1 < args.Length
                    ? args[pathIndex + 1]
                    : Path.Combine(Application.persistentDataPath, "moonlight_visual_qa.png");

                string directory = Path.GetDirectoryName(file);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                Screen.SetResolution(1366, 1024, false);
                yield return new WaitForSeconds(3f);
                var camera = Camera.main;
                if (camera != null)
                    Debug.Log($"[VisualQA] Camera position={camera.transform.position} forward={camera.transform.forward}");
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(file);
                Debug.Log($"[VisualQA] Captured {file}");
                yield return new WaitForSeconds(1f);

                var gm = MoonlightGameManager.Instance;
                if (gm?.moonlight != null && gm.ui != null)
                {
                    float warmthBefore = gm.moonlight.stats.warmth;
                    float restBefore = gm.moonlight.stats.rest;
                    int xpBefore = gm.moonlight.xp;
                    gm.ui.cuddleBtn?.onClick.Invoke();
                    gm.ui.sleepBtn?.onClick.Invoke();
                    gm.ui.feedBtn?.onClick.Invoke();
                    bool feedOpened = gm.ui.feedMenuRoot != null && gm.ui.feedMenuRoot.activeSelf;
                    if (gm.ui.feedMenuRoot != null) gm.ui.feedMenuRoot.SetActive(false);
                    Debug.Log($"[VisualQA] GameplaySmoke feedMenu={feedOpened} " +
                        $"warmthDelta={gm.moonlight.stats.warmth - warmthBefore:0.0} " +
                        $"restDelta={gm.moonlight.stats.rest - restBefore:0.0} " +
                        $"xpDelta={gm.moonlight.xp - xpBefore}");
                }
                else
                {
                    Debug.LogError("[VisualQA] GameplaySmoke missing GameManager, Moonlight or UI");
                }
                Application.Quit(0);
                yield break;
            }

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

        IEnumerator CaptureForQA()
        {
            yield return new WaitForSeconds(2.2f);
            string dir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "MMH-QA");
            Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, "moonlight-photoreal-qa.png");
            ScreenCapture.CaptureScreenshot(file);
            Debug.Log($"[Screenshot][QA] {file}");
            yield return new WaitForSeconds(0.8f);
            Application.Quit();
        }

        IEnumerator PlaytestForQA()
        {
            string dir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "MMH-QA", "playtest");
            Directory.CreateDirectory(dir);

            yield return new WaitForSeconds(1.2f);
            NormalizePlaytestState();
            yield return CaptureStep(dir, "00_initial");

            string[] buttons = { "FeedBtn", "CuddleBtn", "SleepBtn", "PlayBtn", "BathBtn", "DanceBtn" };
            float[] captureDelays = { 0.55f, 0.55f, 1.05f, 2.35f, 0.65f, 0.65f };
            for (int i = 0; i < buttons.Length; i++)
            {
                var go = GameObject.Find(buttons[i]);
                var btn = go ? go.GetComponent<Button>() : null;
                if (btn == null)
                {
                    Debug.LogError($"[Playtest][FAIL] Missing button {buttons[i]}");
                    yield return new WaitForSeconds(0.3f);
                    Application.Quit();
                    yield break;
                }

                Debug.Log($"[Playtest] Click {buttons[i]}");
                bool failed = false;
                try
                {
                    btn.onClick.Invoke();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Playtest][FAIL] {buttons[i]} threw {ex.GetType().Name}: {ex.Message}");
                    failed = true;
                }
                if (failed)
                {
                    yield return new WaitForSeconds(0.3f);
                    Application.Quit();
                    yield break;
                }
                if (buttons[i] == "PlayBtn")
                {
                    yield return new WaitForSeconds(0.95f);
                    yield return CaptureStep(dir, $"{i + 1:00}_PlayDoor");
                    yield return new WaitForSeconds(0.95f);
                    yield return CaptureStep(dir, $"{i + 1:00}_{buttons[i]}");
                    continue;
                }

                yield return new WaitForSeconds(captureDelays[Mathf.Min(i, captureDelays.Length - 1)]);
                yield return CaptureStep(dir, $"{i + 1:00}_{buttons[i]}");
            }

            var ml = MoonlightGameManager.Instance?.moonlight;
            if (ml == null)
            {
                Debug.LogError("[Playtest][FAIL] MoonlightGameManager or Moonlight missing");
            }
            else
            {
                var mood = ml.stats.GetMood();
                if (mood == MoonlightMood.Asleep || mood == MoonlightMood.Grumpy)
                    Debug.LogError($"[Playtest][FAIL] Final mood stayed {mood}");
                else
                    Debug.Log($"[Playtest][PASS] xp={ml.xp} coins={ml.coins} mood={mood} " +
                          $"wonder={ml.stats.wonder:F0} warmth={ml.stats.warmth:F0} rest={ml.stats.rest:F0} " +
                          $"magic={ml.stats.magic:F0} hunger={ml.stats.hunger:F0}");
            }

            yield return new WaitForSeconds(0.8f);
            Application.Quit();
        }

        static void NormalizePlaytestState()
        {
            var gm = MoonlightGameManager.Instance;
            var ml = gm?.moonlight;
            if (ml == null) return;

            ml.coins = Mathf.Max(ml.coins, 30);
            ml.stats.wonder = Mathf.Max(ml.stats.wonder, 70f);
            ml.stats.warmth = Mathf.Max(ml.stats.warmth, 70f);
            ml.stats.rest = Mathf.Max(ml.stats.rest, 70f);
            ml.stats.magic = Mathf.Max(ml.stats.magic, 70f);
            ml.stats.hunger = Mathf.Max(ml.stats.hunger, 70f);
            gm.ui?.Refresh(ml);
            Debug.Log("[Playtest] Normalized in-memory care state for deterministic QA");
        }

        IEnumerator CaptureStep(string dir, string tag)
        {
            string file = Path.Combine(dir, $"{tag}.png");
            ScreenCapture.CaptureScreenshot(file);
            Debug.Log($"[Screenshot][Playtest] {file}");
            yield return new WaitForSeconds(0.45f);
        }

        static bool HasArg(string arg) =>
            System.Array.Exists(System.Environment.GetCommandLineArgs(), a => a == arg);
    }
}
