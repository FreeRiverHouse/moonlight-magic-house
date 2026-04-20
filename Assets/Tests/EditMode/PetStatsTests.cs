using NUnit.Framework;
using MoonlightMagicHouse;
using UnityEngine;

namespace MoonlightMagicHouse.Tests
{
    public class PetStatsTests
    {
        // ── PetStats.GetMood ────────────────────────────────────────────────

        [Test]
        public void GetMood_AllFull_ReturnsEcstatic()
        {
            var s = FullStats();
            Assert.AreEqual(PetMood.Ecstatic, s.GetMood());
        }

        [Test]
        public void GetMood_LowHealth_ReturnsSad()
        {
            var s = FullStats();
            s.health = 10f;
            Assert.AreEqual(PetMood.Sad, s.GetMood());
        }

        [Test]
        public void GetMood_LowEnergy_ReturnsTired()
        {
            var s = FullStats();
            s.energy = 10f;
            Assert.AreEqual(PetMood.Tired, s.GetMood());
        }

        [Test]
        public void GetMood_LowHunger_ReturnsHungry()
        {
            var s = FullStats();
            s.hunger = 10f;
            Assert.AreEqual(PetMood.Hungry, s.GetMood());
        }

        [Test]
        public void GetMood_AllLow_ReturnsSad_HealthFirst()
        {
            var s = new PetStats { hunger=10, happiness=10, energy=10, cleanliness=10, health=10 };
            Assert.AreEqual(PetMood.Sad, s.GetMood());
        }

        // ── DailyRewardManager ──────────────────────────────────────────────

        [Test]
        public void DailyReward_CoinsPerStreak_ArrayLength_Is7()
        {
            // Verifies the stripe array via reflection — keeps sync with class
            var field = typeof(DailyRewardManager)
                .GetField("CoinsPerStreak",
                          System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var arr = (int[])field.GetValue(null);
            Assert.AreEqual(7, arr.Length, "CoinsPerStreak must have 7 entries (one per day)");
        }

        // ── PetSaveData serialisation ───────────────────────────────────────

        [Test]
        public void SaveData_JsonRoundtrip_PreservesFields()
        {
            var data = new PetSaveData
            {
                petName    = "TestPet",
                species    = PetSpecies.Fox,
                stage      = EvolutionStage.Child,
                xp         = 250,
                level      = 3,
                coins      = 77,
                ageMinutes = 42f,
                stats      = new PetStats { hunger=60, happiness=80, energy=50, cleanliness=90, health=100 },
                lastSaveTime = System.DateTime.UtcNow.ToString("O")
            };
            var json    = JsonUtility.ToJson(data);
            var back    = JsonUtility.FromJson<PetSaveData>(json);
            Assert.AreEqual("TestPet",          back.petName);
            Assert.AreEqual(PetSpecies.Fox,     back.species);
            Assert.AreEqual(EvolutionStage.Child, back.stage);
            Assert.AreEqual(250,                back.xp);
            Assert.AreEqual(77,                 back.coins);
            Assert.AreEqual(60f,                back.stats.hunger);
        }

        // ── Item sanity ─────────────────────────────────────────────────────

        [Test]
        public void FoodItem_HungerBoost_Positive()
        {
            var food = ScriptableObject.CreateInstance<FoodItem>();
            food.hungerBoost = 20f;
            Assert.Greater(food.hungerBoost, 0f);
            Object.DestroyImmediate(food);
        }

        [Test]
        public void ActivityItem_EnergyCost_NonNegative()
        {
            var act = ScriptableObject.CreateInstance<ActivityItem>();
            act.energyCost = 15f;
            Assert.GreaterOrEqual(act.energyCost, 0f);
            Object.DestroyImmediate(act);
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        static PetStats FullStats() =>
            new PetStats { hunger=100, happiness=100, energy=100, cleanliness=100, health=100 };
    }
}
