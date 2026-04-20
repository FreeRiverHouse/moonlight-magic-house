using System;
using UnityEngine;
using UnityEngine.Events;

namespace MoonlightMagicHouse
{
    public class DailyRewardManager : MonoBehaviour
    {
        const string LAST_CLAIM_KEY  = "daily_last_claim";
        const string STREAK_KEY      = "daily_streak";

        public UnityEvent<int, int> onRewardClaimed; // coins, streak

        static readonly int[] CoinsPerStreak = { 10, 15, 20, 25, 30, 40, 60 };

        public bool CanClaim()
        {
            if (!PlayerPrefs.HasKey(LAST_CLAIM_KEY)) return true;
            var last = DateTime.Parse(PlayerPrefs.GetString(LAST_CLAIM_KEY));
            return (DateTime.UtcNow - last).TotalHours >= 20;
        }

        public void Claim(MoonlightPet pet)
        {
            if (!CanClaim()) return;

            int streak = GetStreak();
            bool consecutive = IsConsecutiveDay();
            streak = consecutive ? Mathf.Min(streak + 1, CoinsPerStreak.Length) : 1;

            int coins = CoinsPerStreak[streak - 1];
            pet.EarnCoins(coins);

            PlayerPrefs.SetString(LAST_CLAIM_KEY, DateTime.UtcNow.ToString("O"));
            PlayerPrefs.SetInt(STREAK_KEY, streak);
            PlayerPrefs.Save();

            onRewardClaimed?.Invoke(coins, streak);
            AudioManager.Instance?.Play("reward");
        }

        bool IsConsecutiveDay()
        {
            if (!PlayerPrefs.HasKey(LAST_CLAIM_KEY)) return false;
            var last = DateTime.Parse(PlayerPrefs.GetString(LAST_CLAIM_KEY));
            var hours = (DateTime.UtcNow - last).TotalHours;
            return hours is >= 20 and <= 48;
        }

        public int GetStreak() => PlayerPrefs.GetInt(STREAK_KEY, 0);
    }
}
