using NUnit.Framework;
using MoonlightMagicHouse;
using UnityEngine;

namespace MoonlightMagicHouse.Tests
{
    public class MoonlightStatsTests
    {
        [Test]
        public void GetMood_AllFull_ReturnsRadiant()
        {
            var s = FullStats();
            Assert.AreEqual(MoonlightMood.Radiant, s.GetMood());
        }

        [Test]
        public void GetMood_LowRest_ReturnsAsleep()
        {
            var s = FullStats();
            s.rest = 10f;
            Assert.AreEqual(MoonlightMood.Asleep, s.GetMood());
        }

        [Test]
        public void GetMood_LowHunger_ReturnsGrumpy()
        {
            var s = FullStats();
            s.hunger = 10f;
            Assert.AreEqual(MoonlightMood.Grumpy, s.GetMood());
        }

        [Test]
        public void GetMood_LowWonder_ReturnsBored()
        {
            var s = FullStats();
            s.wonder = 20f;
            Assert.AreEqual(MoonlightMood.Bored, s.GetMood());
        }

        [Test]
        public void GetMood_AverageStats_ReturnsHappy()
        {
            var s = new MoonlightStats { wonder=70, warmth=70, rest=70, magic=50, hunger=70 };
            Assert.AreEqual(MoonlightMood.Happy, s.GetMood());
        }

        [Test]
        public void SaveData_JsonRoundtrip_PreservesFields()
        {
            var data = new MoonlightSaveData
            {
                stage         = MoonlightStage.Luminary,
                xp            = 300,
                coins         = 55,
                roomsUnlocked = 3,
                stats         = new MoonlightStats { wonder=80f, warmth=75f, rest=65f, magic=90f, hunger=70f },
                daysInHouse   = 7.5f,
                lastSaveTime  = System.DateTime.UtcNow.ToString("O")
            };
            var json = JsonUtility.ToJson(data);
            var back = JsonUtility.FromJson<MoonlightSaveData>(json);

            Assert.AreEqual(MoonlightStage.Luminary, back.stage);
            Assert.AreEqual(300, back.xp);
            Assert.AreEqual(55,  back.coins);
            Assert.AreEqual(3,   back.roomsUnlocked);
            Assert.AreEqual(80f, back.stats.wonder);
        }

        [Test]
        public void FoodItem_HungerBoost_Positive()
        {
            var food = ScriptableObject.CreateInstance<FoodItem>();
            food.hungerBoost = 20f;
            Assert.Greater(food.hungerBoost, 0f);
            Object.DestroyImmediate(food);
        }

        [Test]
        public void FoodItem_WonderBoost_NonNegative()
        {
            var food = ScriptableObject.CreateInstance<FoodItem>();
            food.wonderBoost = 10f;
            Assert.GreaterOrEqual(food.wonderBoost, 0f);
            Object.DestroyImmediate(food);
        }

        static MoonlightStats FullStats() =>
            new MoonlightStats { wonder=100, warmth=100, rest=100, magic=100, hunger=100 };
    }
}
