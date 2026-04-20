using UnityEngine;

namespace MoonlightMagicHouse
{
    // Attached to each room root. Tweens ambient light when room becomes active.
    public class RoomAmbience : MonoBehaviour
    {
        [SerializeField] Color  ambientColor  = new Color(0.10f, 0.06f, 0.20f);
        [SerializeField] Color  fogColor      = new Color(0.08f, 0.04f, 0.16f);
        [SerializeField] bool   enableFog     = false;
        [SerializeField] float  transitionSec = 1.2f;

        Color  _fromAmbient;
        Color  _fromFog;
        float  _t = 1f;

        void OnEnable()
        {
            _fromAmbient = RenderSettings.ambientLight;
            _fromFog     = RenderSettings.fogColor;
            _t = 0f;
            if (enableFog) RenderSettings.fog = true;
        }

        void OnDisable()
        {
            if (enableFog) RenderSettings.fog = false;
        }

        void Update()
        {
            if (_t >= 1f) return;
            _t += Time.deltaTime / transitionSec;
            RenderSettings.ambientLight = Color.Lerp(_fromAmbient, ambientColor, _t);
            if (enableFog) RenderSettings.fogColor = Color.Lerp(_fromFog, fogColor, _t);
        }
    }
}
