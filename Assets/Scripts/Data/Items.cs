using UnityEngine;

namespace MoonlightMagicHouse
{
    [CreateAssetMenu(menuName = "MoonlightHouse/Food Item")]
    public class FoodItem : ScriptableObject
    {
        public string itemName;
        public Sprite icon;
        public int cost;
        public float hungerBoost;
        public float happinessBoost;
        public int xpReward;
    }

    [CreateAssetMenu(menuName = "MoonlightHouse/Activity Item")]
    public class ActivityItem : ScriptableObject
    {
        public string activityName;
        public Sprite icon;
        public float happinessBoost;
        public float energyCost;
        public int xpReward;
    }

    [CreateAssetMenu(menuName = "MoonlightHouse/Outfit Item")]
    public class OutfitItem : ScriptableObject
    {
        public string outfitName;
        public Sprite preview;
        public int cost;
        public int id;
    }
}
