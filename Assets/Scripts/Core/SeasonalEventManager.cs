using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    [System.Serializable]
    public class SeasonalEvent
    {
        public string   id;
        public string   displayName;
        public int      startMonth, startDay;
        public int      endMonth,   endDay;
        public Color    ambientOverride;
        public int      bonusCoins;         // given on first login during event
        public string   exclusiveOutfitId;  // unlocked free during event
    }

    public class SeasonalEventManager : MonoBehaviour
    {
        public static SeasonalEventManager Instance { get; private set; }

        [SerializeField] List<SeasonalEvent> events;

        SeasonalEvent _active;
        public SeasonalEvent Active => _active;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _active = FindActive();
            if (_active != null) Apply(_active);
        }

        SeasonalEvent FindActive()
        {
            var now = DateTime.Now;
            foreach (var ev in events)
            {
                var start = new DateTime(now.Year, ev.startMonth, ev.startDay);
                var end   = new DateTime(now.Year, ev.endMonth,   ev.endDay);
                if (now >= start && now <= end) return ev;
            }
            return null;
        }

        void Apply(SeasonalEvent ev)
        {
            RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, ev.ambientOverride, 0.5f);

            string pref = $"event_claimed_{ev.id}";
            if (PlayerPrefs.GetInt(pref, 0) == 0)
            {
                PlayerPrefs.SetInt(pref, 1);
                // Coins granted via GameManager after Start
                MoonlightGameManager.Instance?.moonlight.EarnCoins(ev.bonusCoins);
                AudioManager.Instance?.Play("reward");
            }
        }

        public bool IsEventActive(string id) => _active?.id == id;
    }
}
