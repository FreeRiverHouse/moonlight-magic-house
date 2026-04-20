using System.Runtime.InteropServices;
using UnityEngine;

namespace MoonlightMagicHouse
{
    // Onde portal integration — posts XP + coins to parent frame via JS postMessage.
    public class WebGLBridge : MonoBehaviour
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void PostMessageToParent(string json);
#endif

        static void Send(string json)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            PostMessageToParent(json);
#else
            Debug.Log($"[WebGLBridge] {json}");
#endif
        }

        // Call from GameManager on save / level-up
        public static void ReportProgress(int xp, int coins, int level, string stage)
        {
            Send($"{{\"type\":\"moonlight_progress\",\"xp\":{xp},\"coins\":{coins},\"level\":{level},\"stage\":\"{stage}\"}}");
        }

        public static void ReportAchievement(string id)
        {
            Send($"{{\"type\":\"moonlight_achievement\",\"id\":\"{id}\"}}");
        }

        public static void GameReady()
        {
            Send("{\"type\":\"moonlight_ready\"}");
        }
    }
}
