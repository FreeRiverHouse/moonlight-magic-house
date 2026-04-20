using UnityEngine;

namespace MoonlightMagicHouse
{
    // Thin wrapper — add platform-specific haptics here as needed.
    public static class HapticFeedback
    {
        public static void Light()
        {
#if UNITY_IOS
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
#elif UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }

        public static void Medium()
        {
#if UNITY_IOS
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
#elif UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }

        public static void Success()
        {
#if UNITY_IOS
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);
#elif UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }
    }
}
