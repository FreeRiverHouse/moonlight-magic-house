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

            var ml = MoonlightGameManager.Instance?.moonlight;
            if (ml == null) return;

            ml.Explore(RoomType.Garden);
            ml.EarnCoins(exploreCoins);
            AchievementSystem.Instance?.Check("room_garden");
            AudioManager.Instance?.Play("discover");
        }
    }
}
