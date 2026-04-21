using UnityEngine;

namespace MoonlightMagicHouse
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    [ImageEffectAllowedInSceneView]
    public class GodRaysPostFx : MonoBehaviour
    {
        [SerializeField] Transform sunSource;                      // a transform near the window
        [SerializeField] Vector3   fallbackWorldPos = new Vector3(2.5f, 3.0f, 4.8f);
        [SerializeField, Range(0, 2)]    float density   = 0.9f;
        [SerializeField, Range(0.8f, 1)] float decay     = 0.97f;
        [SerializeField, Range(0, 2)]    float weight    = 0.18f;
        [SerializeField, Range(0, 3)]    float exposure  = 0.22f;
        [SerializeField, Range(0, 2)]    float threshold = 1.05f;
        [SerializeField] Color rayTint = new Color(0.78f, 0.88f, 1.10f);
        [SerializeField, Range(1, 4)]    int   downsample = 2;

        Material _mat;
        Camera   _cam;

        void OnEnable()
        {
            _cam = GetComponent<Camera>();
            _cam.depthTextureMode |= DepthTextureMode.Depth;

            var sh = Resources.Load<Shader>("MoonlightGodRays");
            if (sh == null) sh = Shader.Find("MoonlightMagicHouse/GodRays");
            if (sh != null) _mat = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
        }

        void OnDisable()
        {
            if (_mat != null) { DestroyImmediate(_mat); _mat = null; }
        }

        void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (_mat == null) { Graphics.Blit(src, dst); return; }

            Vector3 wp = sunSource != null ? sunSource.position : fallbackWorldPos;
            Vector3 sp = _cam.WorldToViewportPoint(wp);
            if (sp.z < 0f) { Graphics.Blit(src, dst); return; }

            _mat.SetVector("_SunScreen", new Vector4(sp.x, sp.y, 0, 0));
            _mat.SetFloat ("_Density",   density);
            _mat.SetFloat ("_Decay",     decay);
            _mat.SetFloat ("_Weight",    weight);
            _mat.SetFloat ("_Exposure",  exposure);
            _mat.SetFloat ("_Threshold", threshold);
            _mat.SetColor ("_RayTint",   rayTint);

            int w = Mathf.Max(2, src.width  / downsample);
            int h = Mathf.Max(2, src.height / downsample);
            var fmt = src.format;

            var mask = RenderTexture.GetTemporary(w, h, 0, fmt);
            var rays = RenderTexture.GetTemporary(w, h, 0, fmt);

            Graphics.Blit(src, mask, _mat, 0);   // brightmask
            Graphics.Blit(mask, rays, _mat, 1);  // radial blur

            // Copy source then additively blend rays
            Graphics.Blit(src, dst);
            _mat.SetTexture("_MainTex", rays);
            Graphics.Blit(rays, dst, _mat, 2);

            RenderTexture.ReleaseTemporary(mask);
            RenderTexture.ReleaseTemporary(rays);
        }
    }
}
