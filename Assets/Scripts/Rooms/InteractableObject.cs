using UnityEngine;
using UnityEngine.Events;

namespace MoonlightMagicHouse
{
    [RequireComponent(typeof(Collider))]
    public class InteractableObject : MonoBehaviour
    {
        [SerializeField] string promptText = "Interact";
        [SerializeField] float glowIntensity = 1.5f;

        public UnityEvent onInteract;

        Renderer _renderer;
        Material _mat;
        bool _isHovered;

        void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            if (_renderer) _mat = _renderer.material;
        }

        void OnMouseEnter()
        {
            _isHovered = true;
            SetGlow(glowIntensity);
            MoonlightGameManager.Instance?.ui.ShowPrompt(promptText);
        }

        void OnMouseExit()
        {
            _isHovered = false;
            SetGlow(0f);
            MoonlightGameManager.Instance?.ui.HidePrompt();
        }

        void OnMouseUpAsButton() => Interact();

        public void Interact()
        {
            onInteract?.Invoke();
            AudioManager.Instance?.Play("interact");
        }

        void SetGlow(float intensity)
        {
            if (_mat == null) return;
            _mat.SetFloat("_EmissionIntensity", intensity);
        }
    }
}
