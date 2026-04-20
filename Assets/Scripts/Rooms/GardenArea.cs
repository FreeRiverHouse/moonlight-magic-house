using System.Collections;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public class GardenArea : MonoBehaviour
    {
        [SerializeField] int exploreCoins = 5;
        [SerializeField] float exploreCooldown = 300f; // 5 min
        [SerializeField] ParticleSystem fireflies;

        float _lastExplore = -999f;

        void OnEnable()  { if (fireflies) fireflies.Play(); }
        void OnDisable() { if (fireflies) fireflies.Stop(); }

        public void Explore()
        {
            if (Time.time - _lastExplore < exploreCooldown) return;
            _lastExplore = Time.time;
            GameManager.Instance.pet.EarnCoins(exploreCoins);
            GameManager.Instance.pet.Play(CreateExploreActivity());
            AchievementSystem.Instance?.OnRoomVisited(RoomType.Garden);
            AudioManager.Instance?.Play("explore");
        }

        ActivityItem CreateExploreActivity()
        {
            var a = ScriptableObject.CreateInstance<ActivityItem>();
            a.activityName  = "Garden Explore";
            a.happinessBoost = 15f;
            a.energyCost    = 10f;
            a.xpReward      = 10;
            return a;
        }
    }
}
