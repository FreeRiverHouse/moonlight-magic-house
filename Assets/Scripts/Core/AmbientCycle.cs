using UnityEngine;

namespace MoonlightMagicHouse
{
    // Slowly cycles ambient+fog tint to suggest moonlit time passing.
    public class AmbientCycle : MonoBehaviour
    {
        [SerializeField] float period   = 120f;
        [SerializeField] Color tintA    = new Color(0.22f, 0.18f, 0.38f);
        [SerializeField] Color tintB    = new Color(0.35f, 0.22f, 0.55f);
        [SerializeField] Color fogA     = new Color(0.10f, 0.08f, 0.20f);
        [SerializeField] Color fogB     = new Color(0.18f, 0.10f, 0.28f);

        float _t;

        void Start()
        {
            RenderSettings.fog            = true;
            RenderSettings.fogMode        = FogMode.Linear;
            RenderSettings.fogStartDistance = 6f;
            RenderSettings.fogEndDistance   = 18f;
        }

        void Update()
        {
            _t += Time.deltaTime;
            float k = 0.5f * (1f + Mathf.Sin(_t * Mathf.PI * 2f / period));
            RenderSettings.ambientLight = Color.Lerp(tintA, tintB, k);
            RenderSettings.fogColor     = Color.Lerp(fogA,  fogB,  k);
        }
    }
}
