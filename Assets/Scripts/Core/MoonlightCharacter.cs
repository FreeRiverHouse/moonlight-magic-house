using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace MoonlightMagicHouse
{
    // Moonlight — the magical girl protagonist of the house.
    // She has needs, moods, and a growth arc tied to how well the player
    // explores and cares for her world.

    public enum MoonlightMood
    {
        Asleep,     // exhausted / sleeping
        Grumpy,     // multiple low stats
        Bored,      // happiness low, everything else ok
        Calm,       // neutral, cosy
        Happy,      // stats healthy
        Radiant     // all stats high, house fully explored
    }

    public enum MoonlightStage
    {
        Moonbud,    // just arrived, shy, house dark
        Starling,   // first discoveries, timid glow
        Luminary,   // comfortable, rooms unlocking
        Sorceress,  // house alive with magic
        Moonkeeper  // fully bonded, permanent starfield
    }

    [System.Serializable]
    public class MoonlightStats
    {
        [Range(0,100)] public float wonder    = 80f;  // curiosity / exploration drive
        [Range(0,100)] public float warmth    = 80f;  // social / care received
        [Range(0,100)] public float rest      = 80f;  // sleep / energy
        [Range(0,100)] public float magic     = 60f;  // charged by exploring, tricks, stories
        [Range(0,100)] public float hunger    = 80f;

        public MoonlightMood GetMood()
        {
            if (rest < 15f)                              return MoonlightMood.Asleep;
            if (hunger < 20f || warmth < 20f)           return MoonlightMood.Grumpy;
            if (wonder < 25f)                            return MoonlightMood.Bored;
            float avg = (wonder + warmth + rest + magic + hunger) / 5f;
            if (avg < 50f)                               return MoonlightMood.Calm;
            if (magic >= 80f && avg >= 80f)              return MoonlightMood.Radiant;
            return MoonlightMood.Happy;
        }
    }

    public class MoonlightCharacter : MonoBehaviour
    {
        // ── State ──────────────────────────────────────────────────────────
        public MoonlightStats stats = new();
        public MoonlightStage stage = MoonlightStage.Moonbud;
        public int xp;
        public int coins = 30;
        public int roomsUnlocked = 1;   // starts with just the Living Room
        public float daysInHouse;       // real elapsed days (minutes / 1440)

        // ── Stage thresholds (XP) ──────────────────────────────────────────
        static readonly int[] StageXP = { 0, 100, 300, 700, 1500 };

        // ── Decay (per real minute) ────────────────────────────────────────
        const float WONDER_DECAY = 1.2f;
        const float WARMTH_DECAY = 1.0f;
        const float REST_DECAY   = 0.8f;
        const float HUNGER_DECAY = 1.5f;
        const float TICK_SEC     = 60f;

        // ── Events ─────────────────────────────────────────────────────────
        public UnityEvent<MoonlightStage> onStageUp;
        public UnityEvent<MoonlightMood>  onMoodChange;
        public UnityEvent<int>            onXPGained;
        public UnityEvent<int>            onCoinsChanged;
        public UnityEvent<int>            onRoomUnlocked;

        MoonlightMood _lastMood;

        // ── Lifecycle ──────────────────────────────────────────────────────
        void Start()
        {
            _lastMood = stats.GetMood();
            StartCoroutine(TickLoop());
        }

        IEnumerator TickLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(TICK_SEC);
                Decay();
                daysInHouse += 1f / 1440f;
                CheckStageUp();
                CheckMoodChange();
            }
        }

        void Decay()
        {
            stats.wonder = Mathf.Max(0, stats.wonder - WONDER_DECAY);
            stats.warmth = Mathf.Max(0, stats.warmth - WARMTH_DECAY);
            stats.rest   = Mathf.Max(0, stats.rest   - REST_DECAY);
            stats.hunger = Mathf.Max(0, stats.hunger - HUNGER_DECAY);
            // Low rest drains magic
            if (stats.rest < 20f) stats.magic = Mathf.Max(0, stats.magic - 2f);
        }

        void CheckStageUp()
        {
            if (stage == MoonlightStage.Moonkeeper) return;
            int next = (int)stage + 1;
            if (xp >= StageXP[next])
            {
                stage = (MoonlightStage)next;
                UnlockNextRoom();
                onStageUp?.Invoke(stage);
                AudioManager.Instance?.Play("stage_up");
            }
        }

        void UnlockNextRoom()
        {
            roomsUnlocked = Mathf.Min(5, roomsUnlocked + 1);
            onRoomUnlocked?.Invoke(roomsUnlocked);
        }

        void CheckMoodChange()
        {
            var mood = stats.GetMood();
            if (mood == _lastMood) return;
            _lastMood = mood;
            onMoodChange?.Invoke(mood);
        }

        // ── Actions ────────────────────────────────────────────────────────

        public void Feed(FoodItem food)
        {
            if (!SpendCoins(food.cost)) return;
            stats.hunger  = Mathf.Min(100, stats.hunger + food.hungerBoost);
            stats.warmth  = Mathf.Min(100, stats.warmth + food.warmthBoost);
            stats.wonder  = Mathf.Min(100, stats.wonder + food.wonderBoost);
            stats.magic   = Mathf.Min(100, stats.magic  + food.magicBoost);
            GainXP(food.xpReward);
            GetComponentInChildren<MoonlightAnimator>()?.TriggerEat();
            AudioManager.Instance?.Play("eat");
            HapticFeedback.Light();
            AchievementSystem.Instance?.OnFirstFeed();
        }

        public void Cuddle()
        {
            stats.warmth = Mathf.Min(100, stats.warmth + 20f);
            stats.wonder = Mathf.Min(100, stats.wonder + 5f);
            GainXP(8);
            GetComponentInChildren<MoonlightAnimator>()?.TriggerCuddle();
            AudioManager.Instance?.Play("cuddle");
            HapticFeedback.Light();
        }

        public void PutToSleep()
        {
            stats.rest   = Mathf.Min(100, stats.rest   + 45f);
            stats.warmth = Mathf.Min(100, stats.warmth + 5f);
            GetComponentInChildren<MoonlightAnimator>()?.TriggerSleep();
            AudioManager.Instance?.Play("sleep");
            AchievementSystem.Instance?.OnFirstSleep();
        }

        public void Explore(RoomType room)
        {
            stats.wonder = Mathf.Min(100, stats.wonder + 15f);
            stats.magic  = Mathf.Min(100, stats.magic  + 8f);
            GainXP(12);
            AudioManager.Instance?.Play("discover");
            AchievementSystem.Instance?.OnRoomVisited(room);
        }

        public void PerformMagic(int magicGain, int coinReward)
        {
            stats.magic  = Mathf.Min(100, stats.magic  + magicGain);
            stats.wonder = Mathf.Min(100, stats.wonder + 10f);
            EarnCoins(coinReward);
            GainXP(20);
            AudioManager.Instance?.Play("reward");
            HapticFeedback.Success();
        }

        public void Play()
        {
            stats.wonder = Mathf.Min(100, stats.wonder + 15f);
            stats.warmth = Mathf.Min(100, stats.warmth + 5f);
            stats.rest   = Mathf.Max(0,   stats.rest   - 5f);
            GainXP(10);
            AudioManager.Instance?.Play("cuddle");
            HapticFeedback.Light();
        }

        public void Bathe()
        {
            stats.warmth = Mathf.Min(100, stats.warmth + 18f);
            stats.wonder = Mathf.Min(100, stats.wonder + 4f);
            stats.magic  = Mathf.Min(100, stats.magic  + 3f);
            GainXP(6);
            AudioManager.Instance?.Play("cuddle");
            HapticFeedback.Light();
        }

        public void Dance()
        {
            stats.wonder = Mathf.Min(100, stats.wonder + 10f);
            stats.magic  = Mathf.Min(100, stats.magic  + 8f);
            stats.rest   = Mathf.Max(0,   stats.rest   - 3f);
            GainXP(8);
            AudioManager.Instance?.Play("cuddle");
        }

        public void ReadStory()
        {
            stats.wonder = Mathf.Min(100, stats.wonder + 12f);
            stats.warmth = Mathf.Min(100, stats.warmth + 8f);
            stats.rest   = Mathf.Min(100, stats.rest   + 5f);
            GainXP(10);
        }

        // ── Economy ────────────────────────────────────────────────────────
        public void EarnCoins(int amount)
        {
            coins += amount;
            onCoinsChanged?.Invoke(coins);
        }

        public bool SpendCoins(int amount)
        {
            if (coins < amount) return false;
            coins -= amount;
            onCoinsChanged?.Invoke(coins);
            return true;
        }

        void GainXP(int amount)
        {
            xp += amount;
            onXPGained?.Invoke(xp);
            CheckStageUp();
        }
    }
}
