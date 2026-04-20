using UnityEngine;

namespace MoonlightMagicHouse
{
    // A small emissive sphere that orbits its parent on a tilted ellipse,
    // pulsing its emission. Multiple with random phases feel like fireflies.
    public class OrbitingFirefly : MonoBehaviour
    {
        [SerializeField] float radius   = 1.2f;
        [SerializeField] float speed    = 0.9f;
        [SerializeField] float bobAmp   = 0.25f;
        [SerializeField] float tilt     = 18f;
        [SerializeField] float phase    = 0f;
        [SerializeField] Color color    = new Color(1f, 0.85f, 0.55f);

        Material _mat;

        public void Configure(float rad, float spd, float pha, Color col)
        {
            radius = rad; speed = spd; phase = pha; color = col;
            ApplyColor();
        }

        void Awake() { ApplyColor(); }

        void ApplyColor()
        {
            var mr = GetComponent<MeshRenderer>();
            if (mr == null) return;
            _mat = mr.material;
            if (_mat.HasProperty("_Color"))         _mat.SetColor("_Color", color);
            if (_mat.HasProperty("_EmissionColor")) _mat.SetColor("_EmissionColor", color);
        }

        void Update()
        {
            float a = Time.time * speed + phase;
            var  lp = new Vector3(Mathf.Cos(a) * radius, 1.2f + Mathf.Sin(a * 1.7f) * bobAmp,
                                  Mathf.Sin(a) * radius);
            lp = Quaternion.Euler(tilt, 0f, 0f) * lp;
            transform.localPosition = lp;

            if (_mat != null && _mat.HasProperty("_EmissionIntensity"))
            {
                float k = 0.6f + 0.8f * (0.5f + 0.5f * Mathf.Sin(Time.time * 3.2f + phase));
                _mat.SetFloat("_EmissionIntensity", k);
            }
        }
    }
}
