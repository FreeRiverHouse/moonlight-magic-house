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
        public void MoonlightStage_Enum_HasFiveEntries()
        {
            Assert.AreEqual(5, System.Enum.GetValues(typeof(MoonlightStage)).Length);
        }

        [Test]
        public void MoonlightMood_Enum_HasSixEntries()
        {
            Assert.AreEqual(6, System.Enum.GetValues(typeof(MoonlightMood)).Length);
        }

        [Test]
        public void RoomType_Enum_HasFiveEntries()
        {
            // LivingRoom, Kitchen, Bedroom, Garden, Library
            Assert.AreEqual(5, System.Enum.GetValues(typeof(RoomType)).Length);
        }
    }
}
