using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoonlightMagicHouse
{
    [Serializable]
    public class Achievement
    {
        public string id;
        public string title;
        public string description;
        public Sprite icon;
        public int coinReward;
        [NonSerialized] public bool unlocked;
    }

    public class AchievementSystem : MonoBehaviour
    {
        public static AchievementSystem Instance { get; private set; }

        [SerializeField] List<Achievement> achievements;
        public UnityEvent<Achievement> onUnlocked;

        const string PREFIX = "ach_";

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            foreach (var a in achievements)
                a.unlocked = PlayerPrefs.GetInt(PREFIX + a.id, 0) == 1;
        }

        public void Check(string id)
        {
            var ach = achievements.Find(a => a.id == id);
            if (ach == null || ach.unlocked) return;
            ach.unlocked = true;
            PlayerPrefs.SetInt(PREFIX + id, 1);
            PlayerPrefs.Save();
            GameManager.Instance.pet.EarnCoins(ach.coinReward);
            onUnlocked?.Invoke(ach);
            AudioManager.Instance?.Play("achievement");
        }

        // Call sites ─────────────────────────────────────────────
        public void OnPetEvolved(EvolutionStage s)  => Check($"evolve_{s}".ToLower());
        public void OnFirstFeed()                    => Check("first_feed");
        public void OnFirstPlay()                    => Check("first_play");
        public void OnStreakDay(int days)            => Check($"streak_{days}");
        public void OnRoomVisited(RoomType r)        => Check($"room_{r}".ToLower());
        public void OnLevel(int lv)                  => Check($"level_{lv}");
    }
}
