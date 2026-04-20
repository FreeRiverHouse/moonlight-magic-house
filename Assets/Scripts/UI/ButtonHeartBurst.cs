using UnityEngine;
using UnityEngine.UI;

namespace MoonlightMagicHouse
{
    // Add to a Button — on click, spawns a short-lived heart particle burst at a given world transform.
    public class ButtonHeartBurst : MonoBehaviour
    {
        [SerializeField] Transform spawnAt;
        [SerializeField] Color     heartColor = new Color(1f, 0.4f, 0.6f);
        [SerializeField] int       count      = 18;

        Button _btn;

        void Awake()
        {
            _btn = GetComponent<Button>();
            if (_btn != null) _btn.onClick.AddListener(Burst);
        }

        public void Configure(Transform at, Color c, int n)
        {
            spawnAt = at; heartColor = c; count = n;
        }

        void Burst()
        {
            if (spawnAt == null) return;
            var burstGO = new GameObject("HeartBurst");
            burstGO.transform.position = spawnAt.position + Vector3.up * 1.3f;
            var ps = burstGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.4f;
            main.startSpeed    = 1.6f;
            main.startSize     = 0.18f;
            main.startColor    = heartColor;
            main.maxParticles  = count;
            main.gravityModifier = -0.6f; // float upward
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius    = 0.35f;
            var colOverLife = ps.colorOverLifetime;
            colOverLife.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(heartColor, 0f), new GradientColorKey(heartColor, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            colOverLife.color = new ParticleSystem.MinMaxGradient(grad);
            var psr = burstGO.GetComponent<ParticleSystemRenderer>();
            psr.renderMode = ParticleSystemRenderMode.Billboard;
            psr.material   = new Material(Shader.Find("Sprites/Default"));
            Destroy(burstGO, 2.5f);
        }
    }
}
