using UnityEngine;

namespace MoonlightMagicHouse
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    [ImageEffectAllowedInSceneView]
    public class BloomPostFx : MonoBehaviour
    {
        [SerializeField] float threshold  = 0.9f;
        [SerializeField] float softKnee   = 0.6f;
        [SerializeField] float intensity  = 1.1f;
        [SerializeField] float vignette   = 0.45f;
        [SerializeField] Color tint       = new Color(1.02f, 0.98f, 1.05f);
        [SerializeField] int   iterations = 4;
        [SerializeField] float downsample = 2f;

        Material _mat;

        void OnEnable()
        {
            var sh = Resources.Load<Shader>("MoonlightBloom");
            if (sh == null) sh = Shader.Find("MoonlightMagicHouse/Bloom");
            if (sh != null) _mat = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
            var cam = GetComponent<Camera>();
            if (cam != null) cam.allowHDR = true;
        }

        void OnDisable()
        {
            if (_mat != null) { DestroyImmediate(_mat); _mat = null; }
        }

        public void Configure(float newThreshold, float newIntensity, float newVignette, Color newTint, int newIterations)
        {
            threshold = newThreshold;
            intensity = newIntensity;
            vignette = newVignette;
            tint = newTint;
            iterations = Mathf.Max(1, newIterations);
        }

        void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (_mat == null) { Graphics.Blit(src, dst); return; }

            _mat.SetFloat("_Threshold", threshold);
            _mat.SetFloat("_SoftKnee",  softKnee);
            _mat.SetFloat("_Intensity", intensity);
            _mat.SetFloat("_Vignette",  vignette);
            _mat.SetColor("_Tint",      tint);

            int w = Mathf.Max(2, (int)(src.width  / downsample));
            int h = Mathf.Max(2, (int)(src.height / downsample));
            var fmt = src.format;

            var bright = RenderTexture.GetTemporary(w, h, 0, fmt);
            Graphics.Blit(src, bright, _mat, 0);

            var pingA = bright;
            var pingB = RenderTexture.GetTemporary(w, h, 0, fmt);
            for (int i = 0; i < iterations; i++)
            {
                _mat.SetVector("_BlurDir", new Vector2(1, 0));
                Graphics.Blit(pingA, pingB, _mat, 1);
                _mat.SetVector("_BlurDir", new Vector2(0, 1));
                Graphics.Blit(pingB, pingA, _mat, 1);
            }

            _mat.SetTexture("_BloomTex", pingA);
            Graphics.Blit(src, dst, _mat, 2);

            RenderTexture.ReleaseTemporary(pingA);
            RenderTexture.ReleaseTemporary(pingB);
        }
    }
}
