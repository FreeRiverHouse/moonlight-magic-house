using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace MoonlightMagicHouse
{
    public class MoonlightPet : MonoBehaviour
    {
        [Header("Identity")]
        public string petName = "Luna";
        public PetSpecies species = PetSpecies.Dragon;

        [Header("State")]
        public PetStats stats = new PetStats();
        public EvolutionStage stage = EvolutionStage.Egg;
        public int xp;
        public int level = 1;
        public int coins = 50;
        public float ageMinutes;

        [Header("Decay Rates (per minute)")]
        [SerializeField] float hungerDecay    = 2f;
        [SerializeField] float happinessDecay = 1.5f;
        [SerializeField] float energyDecay    = 1f;
        [SerializeField] float cleanlinessDecay = 0.8f;

        [Header("Evolution XP Thresholds")]
        [SerializeField] int[] evolutionThresholds = { 0, 50, 200, 600, 1500 };

        [Header("Events")]
        public UnityEvent<EvolutionStage> onEvolution;
        public UnityEvent<PetMood>        onMoodChange;
        public UnityEvent<int>            onLevelUp;
        public UnityEvent<int>            onCoinsChanged;

        PetMood _lastMood = PetMood.Happy;
        const float TICK_INTERVAL = 60f;

        void Start()
        {
            StartCoroutine(StatDecayLoop());
        }

        IEnumerator StatDecayLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(TICK_INTERVAL);
                ApplyDecay();
                ageMinutes += 1f;
                CheckEvolution();
                CheckMoodChange();
            }
        }

        void ApplyDecay()
        {
            stats.hunger      = Mathf.Max(0, stats.hunger      - hungerDecay);
            stats.happiness   = Mathf.Max(0, stats.happiness   - happinessDecay);
            stats.energy      = Mathf.Max(0, stats.energy      - energyDecay);
            stats.cleanliness = Mathf.Max(0, stats.cleanliness - cleanlinessDecay);

            // Low stats affect health
            if (stats.hunger < 10f || stats.energy < 10f)
                stats.health = Mathf.Max(0, stats.health - 2f);
            else
                stats.health = Mathf.Min(100, stats.health + 0.5f);
        }

        void CheckEvolution()
        {
            if (stage == EvolutionStage.Adult) return;

            int next = (int)stage + 1;
            if (next < evolutionThresholds.Length && xp >= evolutionThresholds[next])
            {
                stage = (EvolutionStage)next;
                onEvolution?.Invoke(stage);
                AudioManager.Instance?.Play("evolution");
            }
        }

        void CheckMoodChange()
        {
            PetMood current = stats.GetMood();
            if (current != _lastMood)
            {
                _lastMood = current;
                onMoodChange?.Invoke(current);
            }
        }

        // ── Actions ──────────────────────────────────────────────

        public void Feed(FoodItem food)
        {
            if (coins < food.cost) return;
            SpendCoins(food.cost);
            stats.hunger    = Mathf.Min(100, stats.hunger    + food.hungerBoost);
            stats.happiness = Mathf.Min(100, stats.happiness + food.happinessBoost);
            GainXP(food.xpReward);
            AudioManager.Instance?.Play("eat");
        }

        public void Play(ActivityItem activity)
        {
            if (stats.energy < 10f) return;
            stats.happiness = Mathf.Min(100, stats.happiness + activity.happinessBoost);
            stats.energy    = Mathf.Max(0,   stats.energy    - activity.energyCost);
            GainXP(activity.xpReward);
            AudioManager.Instance?.Play("play");
        }

        public void Sleep()
        {
            stats.energy    = Mathf.Min(100, stats.energy    + 40f);
            stats.happiness = Mathf.Min(100, stats.happiness + 10f);
            AudioManager.Instance?.Play("sleep");
        }

        public void Clean()
        {
            stats.cleanliness = Mathf.Min(100, stats.cleanliness + 50f);
            GainXP(5);
            AudioManager.Instance?.Play("clean");
        }

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
            int newLevel = 1 + xp / 100;
            if (newLevel > level)
            {
                level = newLevel;
                EarnCoins(level * 5);
                onLevelUp?.Invoke(level);
            }
            CheckEvolution();
        }
    }
}
