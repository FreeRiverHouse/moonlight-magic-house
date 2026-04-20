using UnityEngine;

namespace MoonlightMagicHouse
{
    // Pulsing emissive disc under the character — soft magical footprint.
    public class GroundHalo : MonoBehaviour
    {
        [SerializeField] float baseScale = 1.4f;
        [SerializeField] float pulseAmp  = 0.12f;
        [SerializeField] float speed     = 1.2f;
        Material _mat;
        Color    _baseEmit;

        void Awake()
        {
            var mr = GetComponent<MeshRenderer>();
            if (mr != null)
            {
                _mat = mr.material;
                if (_mat.HasProperty("_EmissionColor"))
                    _baseEmit = _mat.GetColor("_EmissionColor");
            }
        }

        void Update()
        {
            float k = 0.5f + 0.5f * Mathf.Sin(Time.time * speed);
            float s = baseScale * (1f + pulseAmp * (k - 0.5f) * 2f);
            transform.localScale = new Vector3(s, 0.01f, s);
            if (_mat != null && _mat.HasProperty("_EmissionColor"))
                _mat.SetColor("_EmissionColor", _baseEmit * (0.6f + 0.8f * k));
        }
    }
}
