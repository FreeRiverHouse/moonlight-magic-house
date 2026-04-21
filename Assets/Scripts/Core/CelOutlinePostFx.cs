using UnityEngine;

namespace MoonlightMagicHouse
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    [ImageEffectAllowedInSceneView]
    public class CelOutlinePostFx : MonoBehaviour
    {
        [SerializeField] Color outlineColor    = new Color(0.05f, 0.03f, 0.12f, 1f);
        [SerializeField, Range(0.01f, 1.0f)] float normalThresh    = 0.35f;
        [SerializeField, Range(0.0001f, 0.05f)] float depthThresh  = 0.008f;
        [SerializeField, Range(0f, 1f)] float outlineStrength      = 0.85f;
        [SerializeField, Range(1f, 4f)] float thickness            = 1.3f;

        Material _mat;

        void OnEnable()
        {
            var sh = Resources.Load<Shader>("MoonlightOutline");
            if (sh == null) sh = Shader.Find("MoonlightMagicHouse/Outline");
            if (sh != null) _mat = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
            var cam = GetComponent<Camera>();
            if (cam != null) cam.depthTextureMode |= DepthTextureMode.DepthNormals | DepthTextureMode.Depth;
        }

        void OnDisable()
        {
            if (_mat != null) { DestroyImmediate(_mat); _mat = null; }
        }

        void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (_mat == null) { Graphics.Blit(src, dst); return; }
            _mat.SetColor("_OutlineColor",    outlineColor);
            _mat.SetFloat("_NormalThresh",    normalThresh);
            _mat.SetFloat("_DepthThresh",     depthThresh);
            _mat.SetFloat("_OutlineStrength", outlineStrength);
            _mat.SetFloat("_Thickness",       thickness);
            Graphics.Blit(src, dst, _mat, 0);
        }
    }
}
