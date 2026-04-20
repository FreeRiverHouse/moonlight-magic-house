using System;
using UnityEngine;

namespace MoonlightMagicHouse
{
    [Serializable]
    public class MoonlightSaveData
    {
        public MoonlightStats  stats;
        public MoonlightStage  stage;
        public int             xp;
        public int             coins;
        public int             roomsUnlocked;
        public float           daysInHouse;
        public string          lastSaveTime;
        public int             currentOutfitId;
        public int[]           unlockedOutfitIds;
        public string[]        discoveredSecrets;  // room easter eggs found
    }

    public static class MoonlightSave
    {
        const string KEY = "moonlight_save_v1";

        public static void Save(MoonlightCharacter m, int outfitId)
        {
            var data = new MoonlightSaveData
            {
                stats          = m.stats,
                stage          = m.stage,
                xp             = m.xp,
                coins          = m.coins,
                roomsUnlocked  = m.roomsUnlocked,
                daysInHouse    = m.daysInHouse,
                lastSaveTime   = DateTime.UtcNow.ToString("O"),
                currentOutfitId = outfitId,
            };
            PlayerPrefs.SetString(KEY, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public static bool TryLoad(out MoonlightSaveData data)
        {
            data = null;
            if (!PlayerPrefs.HasKey(KEY)) return false;
            data = JsonUtility.FromJson<MoonlightSaveData>(PlayerPrefs.GetString(KEY));
            return data != null;
        }

        public static void Apply(MoonlightSaveData data, MoonlightCharacter m)
        {
            m.stats         = data.stats;
            m.stage         = data.stage;
            m.xp            = data.xp;
            m.coins         = data.coins;
            m.roomsUnlocked = data.roomsUnlocked;
            m.daysInHouse   = data.daysInHouse;

            // Offline penalty (max 2 h)
            if (DateTime.TryParse(data.lastSaveTime, out var last))
            {
                float mins = Mathf.Min(120f, (float)(DateTime.UtcNow - last).TotalMinutes);
                m.stats.hunger = Mathf.Max(0, m.stats.hunger - mins * 1.5f);
                m.stats.rest   = Mathf.Max(0, m.stats.rest   - mins * 0.8f);
                m.stats.warmth = Mathf.Max(0, m.stats.warmth - mins * 0.6f);
            }
        }

        public static void Delete() => PlayerPrefs.DeleteKey(KEY);
    }
}
