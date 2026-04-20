using UnityEngine;

namespace MoonlightMagicHouse
{
    // Attach to any prop in the scene to register it as a secret-triggering interactable.
    // When Moonlight's stage reaches requiredStage the prop gains a glow and becomes clickable.
    [RequireComponent(typeof(Collider))]
    public class PropPlacement : MonoBehaviour
    {
        [SerializeField] string secretId;
        [SerializeField] MoonlightStage requiredStage = MoonlightStage.Moonbud;
        [SerializeField] Material glowMaterial;

        Renderer _rend;
        Material _originalMat;
        bool     _glowing;

        void Start()
        {
            _rend = GetComponentInChildren<Renderer>();
            if (_rend) _originalMat = _rend.sharedMaterial;
            UpdateGlow();
            MoonlightGameManager.Instance?.moonlight.onStageUp.AddListener(_ => UpdateGlow());
        }

        void UpdateGlow()
        {
            var ml = MoonlightGameManager.Instance?.moonlight;
            bool shouldGlow = ml != null && ml.stage >= requiredStage;
            if (shouldGlow == _glowing) return;
            _glowing = shouldGlow;
            if (_rend == null || glowMaterial == null) return;
            _rend.sharedMaterial = shouldGlow ? glowMaterial : _originalMat;
        }

        void OnMouseDown()
        {
            if (!_glowing || string.IsNullOrEmpty(secretId)) return;
            FindAnyObjectByType<HouseSecrets>()?.TryReveal(secretId);
        }
    }
}
