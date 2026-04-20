using UnityEngine;

namespace MoonlightMagicHouse
{
    [RequireComponent(typeof(Collider))]
    public class MoonlightInteractable : MonoBehaviour
    {
        [SerializeField] float tapCooldown = 1.5f;
        float _lastTap;

        void OnMouseDown() => OnTap();

        public void OnTap()
        {
            if (Time.time - _lastTap < tapCooldown) return;
            _lastTap = Time.time;

            var ml = MoonlightGameManager.Instance?.moonlight;
            if (ml == null) return;

            ml.Cuddle();
            MoonlightGameManager.Instance?.idleBehavior?.PauseWander(2f);
            AchievementSystem.Instance?.OnFirstCuddle();
            AudioManager.Instance?.Play("cuddle");
        }
    }
}
