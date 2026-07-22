using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public static class HapticFeedback
    {
        const string LightPreset = "LightImpact";
        const string MediumPreset = "MediumImpact";
        const string SuccessPreset = "Success";
        const string FailurePreset = "Failure";

        public static void Light()
        {
            PlayPreset(LightPreset);
        }

        public static void Medium()
        {
            PlayPreset(MediumPreset);
        }

        public static void Success()
        {
            PlayPreset(SuccessPreset);
        }

        public static void Failure()
        {
            PlayPreset(FailurePreset);
        }

        public static bool ValidateSemanticContract(out string detail)
        {
            string[] presets = { LightPreset, MediumPreset, SuccessPreset, FailurePreset };
            bool named = presets.All(name => !string.IsNullOrWhiteSpace(name));
            bool distinct = presets.Distinct(StringComparer.Ordinal).Count() == presets.Length;
            detail = $"start={LightPreset} step={MediumPreset} pass={SuccessPreset} fail={FailurePreset} " +
                     $"distinct={distinct}";
            return named && distinct && FailurePreset == "Failure";
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
