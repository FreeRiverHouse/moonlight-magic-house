using UnityEngine;
using UnityEngine.Rendering;

namespace MoonlightMagicHouse
{
    public class SkyboxSetup : MonoBehaviour
    {
        void Awake()
        {
            var sh = Resources.Load<Shader>("MoonlightSky");
            if (sh == null) sh = Shader.Find("MoonlightMagicHouse/Sky");
            if (sh == null) { Debug.LogWarning("[SkyboxSetup] MoonlightSky shader missing"); return; }

            var mat = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
            RenderSettings.skybox       = mat;
            RenderSettings.ambientMode  = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 0.9f;
            DynamicGI.UpdateEnvironment();
        }
    }
}
