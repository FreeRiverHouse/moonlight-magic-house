using System;
using UnityEngine;
using UnityEngine.Events;

namespace MoonlightMagicHouse
{
    public class StreakTracker : MonoBehaviour
    {
        public static StreakTracker Instance { get; private set; }

        public UnityEvent<int, int> onDailyLogin; // streak, coinsEarned

        static readonly int[] CoinsPerStreak = { 10, 15, 20, 25, 30, 40, 60 };

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start() => CheckDailyLogin();

        void CheckDailyLogin()
        {
            var today    = DateTime.UtcNow.Date;
            var lastStr  = PlayerPrefs.GetString("streak_date", "");
            int streak   = PlayerPrefs.GetInt("streak_count", 0);

            if (!DateTime.TryParse(lastStr, out var lastDate))
            {
                lastDate = today.AddDays(-2);
            }

            var gap = (today - lastDate.Date).TotalDays;
            if (gap < 1) return;    // same day, already counted

            streak = gap == 1 ? streak + 1 : 1;    // reset on miss
            PlayerPrefs.SetString("streak_date",  today.ToString("O"));
            PlayerPrefs.SetInt("streak_count", streak);
            PlayerPrefs.Save();

            int coins = CoinsPerStreak[Mathf.Min(streak - 1, CoinsPerStreak.Length - 1)];
            MoonlightGameManager.Instance?.moonlight.EarnCoins(coins);
            AudioManager.Instance?.Play("reward");
            onDailyLogin?.Invoke(streak, coins);

            AchievementSystem.Instance?.OnStreakDay(streak);
        }

        public int CurrentStreak => PlayerPrefs.GetInt("streak_count", 0);
    }
}
