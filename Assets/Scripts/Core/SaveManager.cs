using System;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public class SaveManager : MonoBehaviour
    {
        const string SAVE_KEY = "moonlight_pet_save";
        public static SaveManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Save(MoonlightPet pet)
        {
            var data = new PetSaveData
            {
                petName      = pet.petName,
                species      = pet.species,
                stage        = pet.stage,
                stats        = pet.stats,
                xp           = pet.xp,
                level        = pet.level,
                coins        = pet.coins,
                ageMinutes   = pet.ageMinutes,
                lastSaveTime = DateTime.UtcNow.ToString("O"),
            };
            PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public bool TryLoad(out PetSaveData data)
        {
            data = null;
            if (!PlayerPrefs.HasKey(SAVE_KEY)) return false;
            data = JsonUtility.FromJson<PetSaveData>(PlayerPrefs.GetString(SAVE_KEY));
            return data != null;
        }

        public void ApplyToPet(PetSaveData data, MoonlightPet pet)
        {
            pet.petName    = data.petName;
            pet.species    = data.species;
            pet.stage      = data.stage;
            pet.stats      = data.stats;
            pet.xp         = data.xp;
            pet.level      = data.level;
            pet.coins      = data.coins;
            pet.ageMinutes = data.ageMinutes;

            // Penalise offline time (max 120 min decay)
            if (DateTime.TryParse(data.lastSaveTime, out DateTime lastSave))
            {
                float offlineMin = Mathf.Min(120f, (float)(DateTime.UtcNow - lastSave).TotalMinutes);
                pet.stats.hunger    = Mathf.Max(0, pet.stats.hunger    - offlineMin * 2f);
                pet.stats.happiness = Mathf.Max(0, pet.stats.happiness - offlineMin * 1.5f);
                pet.stats.energy    = Mathf.Max(0, pet.stats.energy    - offlineMin * 1f);
            }
        }

        public void Delete() => PlayerPrefs.DeleteKey(SAVE_KEY);
    }
}
