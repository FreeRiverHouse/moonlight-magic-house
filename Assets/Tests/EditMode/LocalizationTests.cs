using NUnit.Framework;
using MoonlightMagicHouse;

namespace MoonlightMagicHouse.Tests
{
    public class LocalizationTests
    {
        [Test]
        public void Get_UnknownKey_ReturnsSameKey()
        {
            var result = LocalizationManager.Get("nonexistent.key");
            Assert.AreEqual("nonexistent.key", result);
        }

        [Test]
        public void Language_Enum_HasFiveEntries()
        {
            Assert.AreEqual(5, System.Enum.GetValues(typeof(Language)).Length);
        }

        [Test]
        public void PetSpecies_Enum_HasEightEntries()
        {
            Assert.AreEqual(8, System.Enum.GetValues(typeof(PetSpecies)).Length);
        }

        [Test]
        public void EvolutionStage_Enum_HasFiveEntries()
        {
            Assert.AreEqual(5, System.Enum.GetValues(typeof(EvolutionStage)).Length);
        }

        [Test]
        public void PetMood_Enum_HasSixEntries()
        {
            Assert.AreEqual(6, System.Enum.GetValues(typeof(PetMood)).Length);
        }

        [Test]
        public void RoomType_Enum_HasFiveEntries()
        {
            Assert.AreEqual(5, System.Enum.GetValues(typeof(RoomType)).Length);
        }
    }
}
