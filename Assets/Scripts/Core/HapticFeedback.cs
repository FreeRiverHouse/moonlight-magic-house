using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public static class HapticFeedback
    {
        public static void Light()
        {
            PlayPreset("LightImpact");
        }

        public static void Medium()
        {
            PlayPreset("MediumImpact");
        }

        public static void Success()
        {
            PlayPreset("Success");
        }

        static void PlayPreset(string presetName)
        {
#if UNITY_IOS || UNITY_ANDROID
            if (TryPlayHapticPatternsPreset(presetName))
            {
                return;
            }

            Handheld.Vibrate();
#endif
        }

        static bool TryPlayHapticPatternsPreset(string presetName)
        {
            try
            {
                Type hapticPatternsType = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(assembly => assembly.GetType("HapticPatterns") ?? assembly.GetType("Lofelt.NiceVibrations.HapticPatterns"))
                    .FirstOrDefault(type => type != null);

                if (hapticPatternsType == null)
                {
                    return false;
                }

                Type presetType = hapticPatternsType.GetNestedType("PresetType", BindingFlags.Public);
                if (presetType == null || !presetType.IsEnum || !Enum.IsDefined(presetType, presetName))
                {
                    return false;
                }

                MethodInfo playPreset = hapticPatternsType.GetMethod("PlayPreset", BindingFlags.Public | BindingFlags.Static, null, new[] { presetType }, null);
                if (playPreset == null)
                {
                    return false;
                }

                object preset = Enum.Parse(presetType, presetName);
                playPreset.Invoke(null, new[] { preset });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
