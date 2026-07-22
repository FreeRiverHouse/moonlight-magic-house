using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlightMagicHouse
{
    // Retained so an old serialized component fails closed instead of becoming
    // a missing script. Production HUD setup must never add this component.
    [DisallowMultipleComponent]
    public class LegacyLabelMirror : MonoBehaviour
    {
        public const string QAMarker = "MOONLIGHT_LEGACY_LABEL_MIRROR_DISABLED";
        public bool IsMirroring => false;

        public void Bind(
            TMP_Text stageT, Text stageL, string stagePrefix,
            TMP_Text moodT, Text moodL, string moodPrefix,
            TMP_Text coinsT, Text coinsL, string coinsPrefix,
            TMP_Text xpT, Text xpL, string xpPrefix,
            TMP_Text daysT, Text daysL, string daysPrefix)
        {
            enabled = false;
        }

        void Awake()
        {
            enabled = false;
            Debug.LogError($"[MoonlightHUDQA] marker={QAMarker} " +
                "Remove LegacyLabelMirror and wire a visible TMP HUD or explicit legacy fallback.");
        }
    }
}
