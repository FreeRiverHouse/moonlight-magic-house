// DROP on an empty GameObject → hit Play → full game running.
// Zero Inspector configuration required — same pattern as HC2AutoSetup (Pizza Gelato Rush).

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using TMPro;

namespace MoonlightMagicHouse
{
    [DefaultExecutionOrder(-100)]
    public class MoonlightHouseSetup : MonoBehaviour
    {
        void Awake()
        {
            Debug.Log("🌙 MOONLIGHT MAGIC HOUSE — Building scene...");

            CleanScene();
            SetupCamera();
            SetupAtmosphere();

            CreateManagers();

            var mlGO = CreateMoonlightCharacter();
            var ml   = mlGO.GetComponent<MoonlightCharacter>();

            var rm = CreateRooms();

            var (uiGO, ui) = CreateUI();

            // GameManager ties everything together
            var gmGO = new GameObject("MoonlightGameManager");
            var gm   = gmGO.AddComponent<MoonlightGameManager>();
            gm.moonlight    = ml;
            gm.ui           = ui;
            gm.rooms        = rm;
            gm.wardrobe     = mlGO.GetComponent<MoonlightWardrobe>();
            gm.idleBehavior = mlGO.GetComponent<MoonlightIdleBehavior>();

            // Point camera at Moonlight
            Camera.main?.GetComponent<CameraController>()?.SetTarget(mlGO.transform);

            Debug.Log("✅ Moonlight Magic House ready!");
        }

        // ── Clean ────────────────────────────────────────────────────────────
        void CleanScene()
        {
            foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.gameObject != gameObject)
                    Destroy(l.gameObject);
        }

        // ── Camera ───────────────────────────────────────────────────────────
        void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGO = new GameObject("Main Camera");
                camGO.tag = "MainCamera";
                cam = camGO.AddComponent<Camera>();
                camGO.AddComponent<AudioListener>();
            }
            cam.clearFlags      = CameraClearFlags.Skybox;
            cam.backgroundColor = new Color(0.04f, 0.02f, 0.10f);
            if (!gameObject.GetComponent<SkyboxSetup>())
                gameObject.AddComponent<SkyboxSetup>();
            cam.farClipPlane    = 80f;
            cam.fieldOfView     = 60f;
            cam.transform.position = new Vector3(0f, 2.8f, -6.2f);
            cam.transform.LookAt(new Vector3(0f, 1.2f, 0.5f));
            cam.allowHDR = true;

            if (!cam.GetComponent<CameraController>())
                cam.gameObject.AddComponent<CameraController>();
            if (!cam.GetComponent<AmbientCycle>())
                cam.gameObject.AddComponent<AmbientCycle>();
            if (!cam.GetComponent<BloomPostFx>())
                cam.gameObject.AddComponent<BloomPostFx>();
            if (!cam.GetComponent<CelOutlinePostFx>())
                cam.gameObject.AddComponent<CelOutlinePostFx>();
        }

        // ── Atmosphere ───────────────────────────────────────────────────────
        void SetupAtmosphere()
        {
            RenderSettings.ambientMode    = AmbientMode.Flat;
            RenderSettings.ambientLight   = new Color(0.18f, 0.12f, 0.30f);
            RenderSettings.fog            = true;
            RenderSettings.fogMode        = FogMode.Linear;
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance   = 45f;
            RenderSettings.fogColor       = new Color(0.05f, 0.02f, 0.13f);

            // Moon — cool blue-white directional
            MakeLight("Moon", LightType.Directional,
                new Color(0.70f, 0.82f, 1.00f), 1.4f,
                Quaternion.Euler(40f, -20f, 0f), LightShadows.Soft, 0.6f);

            // Warm fill — fireplace/lamp feel
            MakeLight("WarmFill", LightType.Directional,
                new Color(1.00f, 0.60f, 0.30f), 0.6f,
                Quaternion.Euler(30f, 160f, 0f), LightShadows.None, 0f);

            // Magic rim — purple
            MakeLight("MagicRim", LightType.Directional,
                new Color(0.60f, 0.30f, 1.00f), 0.9f,
                Quaternion.Euler(20f, 180f, 0f), LightShadows.None, 0f);

            // Floating dust motes — ambient indoor atmosphere
            var dustGO = new GameObject("DustMotes");
            dustGO.transform.position = new Vector3(0f, 1.8f, 0f);
            var dps = dustGO.AddComponent<ParticleSystem>();
            var dmain = dps.main;
            dmain.startLifetime = 8f;
            dmain.startSpeed    = 0.15f;
            dmain.startSize     = new ParticleSystem.MinMaxCurve(0.04f, 0.10f);
            dmain.startColor    = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.95f, 0.85f, 0.35f), new Color(0.85f, 0.75f, 1f, 0.35f));
            dmain.maxParticles  = 60;
            dmain.gravityModifier = -0.02f;
            var demis = dps.emission;
            demis.rateOverTime = 6f;
            var dshape = dps.shape;
            dshape.shapeType = ParticleSystemShapeType.Box;
            dshape.scale     = new Vector3(8f, 3f, 8f);
            var dvel = dps.velocityOverLifetime;
            dvel.enabled = true;
            dvel.x = new ParticleSystem.MinMaxCurve(-0.12f, 0.12f);
            dvel.z = new ParticleSystem.MinMaxCurve(-0.12f, 0.12f);
            var dcol = dps.colorOverLifetime;
            dcol.enabled = true;
            var dg = new Gradient();
            dg.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.35f, 0.4f), new GradientAlphaKey(0f, 1f) });
            dcol.color = new ParticleSystem.MinMaxGradient(dg);
            var dpsr = dustGO.GetComponent<ParticleSystemRenderer>();
            dpsr.renderMode = ParticleSystemRenderMode.Billboard;
            var dustMat     = new Material(Shader.Find("Sprites/Default"));
            dustMat.mainTexture = MakeSoftCircleTex(32);
            dpsr.material   = dustMat;
        }

        static Texture2D _softCircleTex;
        public static Texture2D MakeSoftCircleTex(int size)
        {
            if (_softCircleTex != null) return _softCircleTex;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode   = TextureWrapMode.Clamp;
            float c = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x - c) / c, dy = (y - c) / c;
                float d  = Mathf.Sqrt(dx * dx + dy * dy);
                float a  = Mathf.Clamp01(1f - d);
                a = a * a; // soft falloff
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
            tex.Apply();
            _softCircleTex = tex;
            return tex;
        }

        Light MakeLight(string n, LightType t, Color c, float intensity,
                        Quaternion rot, LightShadows sh, float shStr)
        {
            var go = new GameObject(n);
            var l  = go.AddComponent<Light>();
            l.type            = t;
            l.color           = c;
            l.intensity       = intensity;
            l.transform.rotation = rot;
            l.shadows         = sh;
            l.shadowStrength  = shStr;
            return l;
        }

        // ── Managers ─────────────────────────────────────────────────────────
        void CreateManagers()
        {
            AddMgr<AudioManager>("AudioManager");
            AddMgr<LocalizationManager>("LocalizationManager");
            AddMgr<NotificationManager>("NotificationManager");
            AddMgr<AchievementSystem>("AchievementSystem");
            AddMgr<StreakTracker>("StreakTracker");
            AddMgr<SeasonalEventManager>("SeasonalEventManager");
            AddMgr<MoonlightTricksSystem>("TricksSystem");
            AddMgr<WebGLBridge>("WebGLBridge");
            AddMgr<ScreenshotCapture>("ScreenshotCapture");
        }

        static void AddMgr<T>(string name) where T : Component
        {
            var go = new GameObject(name);
            go.AddComponent<T>();
        }

        // ── Moonlight Character ──────────────────────────────────────────────
        GameObject CreateMoonlightCharacter()
        {
            var mlGO = new GameObject("Moonlight");
            mlGO.transform.position = Vector3.zero;

            mlGO.AddComponent<MoonlightCharacter>();
            mlGO.AddComponent<MoonlightIdleBehavior>();
            mlGO.AddComponent<MoonlightWardrobe>();
            mlGO.AddComponent<MoonlightStageVFX>();
            mlGO.AddComponent<MoonlightGlowController>();
            mlGO.AddComponent<MoonlightMoodParticles>();
            mlGO.AddComponent<MoonlightAnimator>();

            // Stylised primitive character — guaranteed visible, toon-shaded with outline
            var visual = BuildFallbackCharacter(mlGO.transform);
            // Face the camera (camera is at -Z; character's face is at +Z by default)
            visual.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            // Scale-punch on button presses
            var puncher = visual.AddComponent<ScalePuncher>();
            // Gentle idle micro-motion (head tilt, arm sway, body squash)
            visual.AddComponent<IdleMicroMotion>();

            // Floating name tag above the character (3D TextMesh — billboarded)
            var tagGO = new GameObject("NameTag");
            tagGO.transform.SetParent(mlGO.transform, false);
            tagGO.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            tagGO.transform.localScale    = Vector3.one * 0.25f;
            tagGO.AddComponent<BillboardToCamera>();
            var tm = tagGO.AddComponent<TextMesh>();
            tm.text       = "Moonbud";
            tm.fontSize   = 48;
            tm.fontStyle  = FontStyle.Bold;
            tm.anchor     = TextAnchor.MiddleCenter;
            tm.alignment  = TextAlignment.Center;
            tm.color      = new Color(1f, 0.9f, 1f);
            tm.characterSize = 0.1f;
            var tmr = tagGO.GetComponent<MeshRenderer>();
            tmr.sortingOrder = 5;

            // Procedural idle bob — no Animator required
            mlGO.AddComponent<MoonlightBobber>();

            // Collider
            var col = mlGO.AddComponent<CapsuleCollider>();
            col.height = 2.2f;
            col.radius = 0.38f;
            col.center = new Vector3(0f, 1.1f, 0f);
            mlGO.AddComponent<MoonlightInteractable>();

            // Sparkle particle system (ambient magic around character)
            var sparkGO = new GameObject("Sparkles");
            sparkGO.transform.SetParent(mlGO.transform, false);
            sparkGO.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            var ps = sparkGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 2.2f;
            main.startSpeed    = 0.35f;
            main.startSize     = 0.06f;
            main.startColor    = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.90f, 0.55f), new Color(0.75f, 0.55f, 1f));
            main.maxParticles  = 40;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            var emission = ps.emission;
            emission.rateOverTime = 8f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius    = 0.9f;
            var colOverLife = ps.colorOverLifetime;
            colOverLife.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.85f, 0.6f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.9f, 0.5f), new GradientAlphaKey(0f, 1f) });
            colOverLife.color = new ParticleSystem.MinMaxGradient(grad);
            var psr = sparkGO.GetComponent<ParticleSystemRenderer>();
            psr.renderMode = ParticleSystemRenderMode.Billboard;
            var sparkMat   = new Material(Shader.Find("Sprites/Default"));
            sparkMat.mainTexture = MakeSoftCircleTex(32);
            psr.material   = sparkMat;

            // Ground halo — pulsing disc beneath the character
            var haloGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            haloGO.name = "GroundHalo";
            Object.Destroy(haloGO.GetComponent<Collider>());
            haloGO.transform.SetParent(mlGO.transform, false);
            haloGO.transform.localPosition = new Vector3(0f, 0.005f, 0f);
            haloGO.transform.localScale    = new Vector3(1.4f, 0.01f, 1.4f);
            var haloMat = new Material(ToonShader);
            haloMat.SetColor("_Color",            new Color(0.75f, 0.55f, 1.00f, 0.6f));
            haloMat.SetColor("_EmissionColor",    new Color(0.75f, 0.55f, 1.00f));
            haloMat.SetFloat("_EmissionIntensity", 1.2f);
            haloMat.SetFloat("_OutlineWidth",      0f);
            haloGO.GetComponent<MeshRenderer>().material = haloMat;
            haloGO.AddComponent<GroundHalo>();

            // Orbiting fireflies around the character
            Color[] fireCols = {
                new Color(1.00f, 0.85f, 0.55f),
                new Color(0.80f, 0.60f, 1.00f),
                new Color(0.70f, 0.95f, 0.80f),
                new Color(1.00f, 0.65f, 0.85f),
                new Color(0.55f, 0.85f, 1.00f),
            };
            for (int f = 0; f < fireCols.Length; f++)
            {
                var fly = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                fly.name = $"Firefly{f}";
                fly.transform.SetParent(mlGO.transform, false);
                fly.transform.localScale = Vector3.one * Random.Range(0.05f, 0.08f);
                Object.Destroy(fly.GetComponent<Collider>());
                var fMat = new Material(ToonShader);
                fMat.SetColor("_Color",             fireCols[f]);
                fMat.SetColor("_EmissionColor",     fireCols[f]);
                fMat.SetFloat("_EmissionIntensity", 1.2f);
                fMat.SetFloat("_OutlineWidth",      0f);
                fly.GetComponent<MeshRenderer>().material = fMat;
                var orb = fly.AddComponent<OrbitingFirefly>();
                orb.Configure(
                    Random.Range(1.05f, 1.6f),
                    Random.Range(0.6f, 1.1f) * (Random.value > 0.5f ? 1f : -1f),
                    Random.Range(0f, Mathf.PI * 2f),
                    fireCols[f]);
            }

            // Glow light
            var glowGO = new GameObject("Glow");
            glowGO.transform.SetParent(mlGO.transform, false);
            glowGO.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            var glow = glowGO.AddComponent<Light>();
            glow.type      = LightType.Point;
            glow.color     = new Color(0.65f, 0.40f, 1.00f);
            glow.intensity = 2.8f;
            glow.range     = 6f;

            return mlGO;
        }

        static GameObject BuildFallbackCharacter(Transform parent)
        {
            var visual = new GameObject("Visual");
            visual.transform.SetParent(parent, false);

            // Neck — thin capsule between head and shoulders
            MakePart(visual.transform, PrimitiveType.Capsule, "Neck",
                new Vector3(0f, 1.55f, 0f), new Vector3(0.13f, 0.08f, 0.13f),
                new Color(1.00f, 0.88f, 0.80f));
            // Upper torso / bodice — narrower, warm purple fabric
            MakePart(visual.transform, PrimitiveType.Capsule, "Bodice",
                new Vector3(0f, 1.22f, 0f), new Vector3(0.46f, 0.28f, 0.40f),
                new Color(0.65f, 0.40f, 0.88f));
            // Collar — white lace ring around neck
            MakePart(visual.transform, PrimitiveType.Cylinder, "Collar",
                new Vector3(0f, 1.47f, 0f), new Vector3(0.28f, 0.03f, 0.28f),
                new Color(0.98f, 0.92f, 1.00f));
            // Chest-bow accent
            MakePartRotated(visual.transform, PrimitiveType.Cube, "ChestBowL",
                new Vector3(-0.06f, 1.35f, 0.18f), Quaternion.Euler(0, 0, 30),
                new Vector3(0.09f, 0.07f, 0.04f), new Color(1.0f, 0.45f, 0.75f));
            MakePartRotated(visual.transform, PrimitiveType.Cube, "ChestBowR",
                new Vector3( 0.06f, 1.35f, 0.18f), Quaternion.Euler(0, 0, -30),
                new Vector3(0.09f, 0.07f, 0.04f), new Color(1.0f, 0.45f, 0.75f));
            MakePart(visual.transform, PrimitiveType.Sphere, "ChestBowKnot",
                new Vector3(0f, 1.35f, 0.19f), Vector3.one * 0.045f,
                new Color(1.0f, 0.30f, 0.60f));
            // Old-style "Body" kept but tucked inside dress as hip base
            MakePart(visual.transform, PrimitiveType.Capsule, "Body",
                new Vector3(0f, 0.9f, 0f), new Vector3(0.55f, 0.55f, 0.55f),
                new Color(0.82f, 0.72f, 0.96f));
            // Puff sleeves — spheres at shoulders above arms
            MakePart(visual.transform, PrimitiveType.Sphere, "PuffL",
                new Vector3(-0.33f, 1.22f, 0f), new Vector3(0.22f, 0.20f, 0.22f),
                new Color(0.70f, 0.45f, 0.92f));
            MakePart(visual.transform, PrimitiveType.Sphere, "PuffR",
                new Vector3( 0.33f, 1.22f, 0f), new Vector3(0.22f, 0.20f, 0.22f),
                new Color(0.70f, 0.45f, 0.92f));
            // Hands — small pale spheres at arm tips (ArmL rotated +25°, tip points down-outward)
            MakePart(visual.transform, PrimitiveType.Sphere, "HandL",
                new Vector3(-0.58f, 0.62f, 0f), Vector3.one * 0.11f,
                new Color(1.00f, 0.88f, 0.80f));
            MakePart(visual.transform, PrimitiveType.Sphere, "HandR",
                new Vector3( 0.58f, 0.62f, 0f), Vector3.one * 0.11f,
                new Color(1.00f, 0.88f, 0.80f));
            MakePart(visual.transform, PrimitiveType.Sphere, "Head",
                new Vector3(0f, 1.85f, 0f), Vector3.one * 0.42f,
                new Color(1.00f, 0.88f, 0.80f));
            MakePart(visual.transform, PrimitiveType.Sphere, "Hair",
                new Vector3(0f, 2.06f, 0.04f), new Vector3(0.46f, 0.30f, 0.46f),
                new Color(0.22f, 0.08f, 0.48f));
            // Eyes
            MakePart(visual.transform, PrimitiveType.Sphere, "EyeL",
                new Vector3(-0.08f, 1.88f, 0.19f), Vector3.one * 0.07f,
                new Color(0.08f, 0.04f, 0.18f));
            MakePart(visual.transform, PrimitiveType.Sphere, "EyeR",
                new Vector3( 0.08f, 1.88f, 0.19f), Vector3.one * 0.07f,
                new Color(0.08f, 0.04f, 0.18f));
            // Iris (purple) on top of black eye
            MakeEmissive(visual.transform, "IrisL",
                new Vector3(-0.08f, 1.88f, 0.215f), Vector3.one * 0.045f,
                new Color(0.55f, 0.35f, 0.95f), 1.1f);
            MakeEmissive(visual.transform, "IrisR",
                new Vector3( 0.08f, 1.88f, 0.215f), Vector3.one * 0.045f,
                new Color(0.55f, 0.35f, 0.95f), 1.1f);
            // Eye highlights — tiny white sphere "shine"
            MakeEmissive(visual.transform, "ShineL",
                new Vector3(-0.063f, 1.90f, 0.232f), Vector3.one * 0.022f, Color.white, 2.0f);
            MakeEmissive(visual.transform, "ShineR",
                new Vector3( 0.097f, 1.90f, 0.232f), Vector3.one * 0.022f, Color.white, 2.0f);
            // Eyelashes — tiny tilted cubes above each eye
            MakePartRotated(visual.transform, PrimitiveType.Cube, "LashL",
                new Vector3(-0.08f, 1.94f, 0.215f),
                Quaternion.Euler(0f, 0f, 15f),
                new Vector3(0.10f, 0.012f, 0.018f),
                new Color(0.10f, 0.05f, 0.20f));
            MakePartRotated(visual.transform, PrimitiveType.Cube, "LashR",
                new Vector3( 0.08f, 1.94f, 0.215f),
                Quaternion.Euler(0f, 0f, -15f),
                new Vector3(0.10f, 0.012f, 0.018f),
                new Color(0.10f, 0.05f, 0.20f));
            var blinker = visual.AddComponent<EyeBlinker>();
            blinker.Bind(
                visual.transform.Find("EyeL"),
                visual.transform.Find("EyeR"),
                visual.transform.Find("ShineL"),
                visual.transform.Find("ShineR"));
            // Nose — subtle rose bump
            MakePart(visual.transform, PrimitiveType.Sphere, "Nose",
                new Vector3(0f, 1.84f, 0.215f), Vector3.one * 0.035f,
                new Color(1.0f, 0.75f, 0.78f));
            // Curved mouth — three small cubes arranged as a smile arc
            MakePartRotated(visual.transform, PrimitiveType.Cube, "SmileL",
                new Vector3(-0.035f, 1.795f, 0.205f),
                Quaternion.Euler(0f, 0f,  12f),
                new Vector3(0.045f, 0.014f, 0.014f),
                new Color(0.78f, 0.28f, 0.42f));
            MakePartRotated(visual.transform, PrimitiveType.Cube, "SmileR",
                new Vector3( 0.035f, 1.795f, 0.205f),
                Quaternion.Euler(0f, 0f, -12f),
                new Vector3(0.045f, 0.014f, 0.014f),
                new Color(0.78f, 0.28f, 0.42f));
            // Front bangs — two cute hair tufts over forehead
            MakePartRotated(visual.transform, PrimitiveType.Sphere, "BangL",
                new Vector3(-0.15f, 2.03f, 0.18f),
                Quaternion.Euler(0f, 0f, 20f),
                new Vector3(0.18f, 0.14f, 0.14f),
                new Color(0.22f, 0.08f, 0.48f));
            MakePartRotated(visual.transform, PrimitiveType.Sphere, "BangR",
                new Vector3( 0.15f, 2.03f, 0.18f),
                Quaternion.Euler(0f, 0f, -20f),
                new Vector3(0.18f, 0.14f, 0.14f),
                new Color(0.22f, 0.08f, 0.48f));
            // Side pigtails
            MakePart(visual.transform, PrimitiveType.Sphere, "PigL",
                new Vector3(-0.28f, 1.90f, -0.02f), new Vector3(0.16f, 0.22f, 0.16f),
                new Color(0.22f, 0.08f, 0.48f));
            MakePart(visual.transform, PrimitiveType.Sphere, "PigR",
                new Vector3( 0.28f, 1.90f, -0.02f), new Vector3(0.16f, 0.22f, 0.16f),
                new Color(0.22f, 0.08f, 0.48f));
            // Back hair volume
            MakePart(visual.transform, PrimitiveType.Sphere, "HairBack",
                new Vector3(0f, 1.95f, -0.14f), new Vector3(0.44f, 0.40f, 0.30f),
                new Color(0.18f, 0.06f, 0.40f));
            // Cheeks
            MakePart(visual.transform, PrimitiveType.Sphere, "CheekL",
                new Vector3(-0.14f, 1.82f, 0.17f), Vector3.one * 0.05f,
                new Color(1.0f, 0.6f, 0.7f));
            MakePart(visual.transform, PrimitiveType.Sphere, "CheekR",
                new Vector3( 0.14f, 1.82f, 0.17f), Vector3.one * 0.05f,
                new Color(1.0f, 0.6f, 0.7f));
            // Arms — tilted outward so they hang down from shoulders
            MakePartRotated(visual.transform, PrimitiveType.Capsule, "ArmL",
                new Vector3(-0.38f, 0.95f, 0f),
                Quaternion.Euler(0f, 0f, 25f),
                new Vector3(0.15f, 0.38f, 0.15f),
                new Color(0.78f, 0.68f, 0.92f));
            MakePartRotated(visual.transform, PrimitiveType.Capsule, "ArmR",
                new Vector3( 0.38f, 0.95f, 0f),
                Quaternion.Euler(0f, 0f, -25f),
                new Vector3(0.15f, 0.38f, 0.15f),
                new Color(0.78f, 0.68f, 0.92f));
            // Dress — three stacked flared cylinders (ruffled)
            MakePart(visual.transform, PrimitiveType.Cylinder, "DressTop",
                new Vector3(0f, 0.78f, 0f), new Vector3(0.48f, 0.14f, 0.48f),
                new Color(0.62f, 0.35f, 0.90f));
            MakePart(visual.transform, PrimitiveType.Cylinder, "DressMid",
                new Vector3(0f, 0.55f, 0f), new Vector3(0.62f, 0.12f, 0.62f),
                new Color(0.55f, 0.30f, 0.82f));
            MakePart(visual.transform, PrimitiveType.Cylinder, "DressHem",
                new Vector3(0f, 0.36f, 0f), new Vector3(0.75f, 0.10f, 0.75f),
                new Color(0.48f, 0.25f, 0.74f));
            // Lace trim — thin emissive cylinder at dress hem
            var trim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trim.name = "DressTrim";
            trim.transform.SetParent(visual.transform, false);
            trim.transform.localPosition = new Vector3(0f, 0.28f, 0f);
            trim.transform.localScale    = new Vector3(0.76f, 0.03f, 0.76f);
            var trimMat = new Material(ToonShader);
            trimMat.SetColor("_Color",             new Color(1.00f, 0.85f, 0.95f));
            trimMat.SetColor("_EmissionColor",     new Color(1.00f, 0.85f, 0.95f));
            trimMat.SetFloat("_EmissionIntensity", 0.9f);
            trimMat.SetFloat("_OutlineWidth",      0f);
            trim.GetComponent<MeshRenderer>().material = trimMat;
            Object.Destroy(trim.GetComponent<Collider>());
            // Belt
            MakePart(visual.transform, PrimitiveType.Cylinder, "Belt",
                new Vector3(0f, 0.90f, 0f), new Vector3(0.42f, 0.05f, 0.42f),
                new Color(1.0f, 0.85f, 0.40f));
            // ── LEGS: thigh + knee + calf ──
            var tights = new Color(0.92f, 0.86f, 1.00f);
            MakePart(visual.transform, PrimitiveType.Capsule, "ThighL",
                new Vector3(-0.14f, 0.32f, 0f), new Vector3(0.14f, 0.18f, 0.14f), tights);
            MakePart(visual.transform, PrimitiveType.Capsule, "ThighR",
                new Vector3( 0.14f, 0.32f, 0f), new Vector3(0.14f, 0.18f, 0.14f), tights);
            MakePart(visual.transform, PrimitiveType.Sphere, "KneeL",
                new Vector3(-0.14f, 0.13f, 0f), Vector3.one * 0.13f, tights);
            MakePart(visual.transform, PrimitiveType.Sphere, "KneeR",
                new Vector3( 0.14f, 0.13f, 0f), Vector3.one * 0.13f, tights);
            MakePart(visual.transform, PrimitiveType.Capsule, "CalfL",
                new Vector3(-0.14f, -0.02f, 0f), new Vector3(0.12f, 0.14f, 0.12f), tights);
            MakePart(visual.transform, PrimitiveType.Capsule, "CalfR",
                new Vector3( 0.14f, -0.02f, 0f), new Vector3(0.12f, 0.14f, 0.12f), tights);
            // Ankle strap (emissive pink)
            MakeEmissive(visual.transform, "AnkleL",
                new Vector3(-0.14f, -0.13f, 0f), new Vector3(0.14f, 0.025f, 0.14f),
                new Color(1.0f, 0.55f, 0.80f), 0.7f);
            MakeEmissive(visual.transform, "AnkleR",
                new Vector3( 0.14f, -0.13f, 0f), new Vector3(0.14f, 0.025f, 0.14f),
                new Color(1.0f, 0.55f, 0.80f), 0.7f);
            // ── SHOES: oval main + heel + buckle ──
            var shoeCol = new Color(0.30f, 0.14f, 0.45f);
            MakePart(visual.transform, PrimitiveType.Sphere, "FootL",
                new Vector3(-0.15f, -0.18f, 0.11f), new Vector3(0.19f, 0.10f, 0.26f), shoeCol);
            MakePart(visual.transform, PrimitiveType.Sphere, "FootR",
                new Vector3( 0.15f, -0.18f, 0.11f), new Vector3(0.19f, 0.10f, 0.26f), shoeCol);
            MakePart(visual.transform, PrimitiveType.Cube, "HeelL",
                new Vector3(-0.15f, -0.22f, -0.02f), new Vector3(0.08f, 0.07f, 0.06f), shoeCol);
            MakePart(visual.transform, PrimitiveType.Cube, "HeelR",
                new Vector3( 0.15f, -0.22f, -0.02f), new Vector3(0.08f, 0.07f, 0.06f), shoeCol);
            MakeEmissive(visual.transform, "BuckleL",
                new Vector3(-0.15f, -0.15f, 0.22f), Vector3.one * 0.035f,
                new Color(1.0f, 0.90f, 0.50f), 1.2f);
            MakeEmissive(visual.transform, "BuckleR",
                new Vector3( 0.15f, -0.15f, 0.22f), Vector3.one * 0.035f,
                new Color(1.0f, 0.90f, 0.50f), 1.2f);

            // ── EARS ──
            MakePartRotated(visual.transform, PrimitiveType.Sphere, "EarL",
                new Vector3(-0.21f, 1.85f, 0.02f), Quaternion.Euler(0,0,10),
                new Vector3(0.06f, 0.10f, 0.04f), new Color(1.00f, 0.85f, 0.78f));
            MakePartRotated(visual.transform, PrimitiveType.Sphere, "EarR",
                new Vector3( 0.21f, 1.85f, 0.02f), Quaternion.Euler(0,0,-10),
                new Vector3(0.06f, 0.10f, 0.04f), new Color(1.00f, 0.85f, 0.78f));

            // ── FINGERS: 3 tiny nubs per hand for "mitten" reads ──
            var skin = new Color(1.00f, 0.88f, 0.80f);
            for (int f = 0; f < 3; f++)
            {
                float fx = -0.03f + f * 0.03f;
                MakePart(visual.transform, PrimitiveType.Sphere, $"FingerL{f}",
                    new Vector3(-0.58f + fx, 0.54f, 0.02f), Vector3.one * 0.035f, skin);
                MakePart(visual.transform, PrimitiveType.Sphere, $"FingerR{f}",
                    new Vector3( 0.58f + fx, 0.54f, 0.02f), Vector3.one * 0.035f, skin);
            }

            // ── HAIR LAYERS: side-swept fringe + long back tails ──
            var hairCol  = new Color(0.22f, 0.08f, 0.48f);
            var hairDark = new Color(0.16f, 0.05f, 0.38f);
            MakePartRotated(visual.transform, PrimitiveType.Sphere, "FringeSide",
                new Vector3(0.02f, 2.02f, 0.19f), Quaternion.Euler(0,0,-12),
                new Vector3(0.32f, 0.12f, 0.14f), hairCol);
            MakePartRotated(visual.transform, PrimitiveType.Capsule, "TailL",
                new Vector3(-0.32f, 1.70f, -0.05f), Quaternion.Euler(10,0,0),
                new Vector3(0.16f, 0.30f, 0.16f), hairDark);
            MakePartRotated(visual.transform, PrimitiveType.Capsule, "TailR",
                new Vector3( 0.32f, 1.70f, -0.05f), Quaternion.Euler(10,0,0),
                new Vector3(0.16f, 0.30f, 0.16f), hairDark);

            // ── DRESS EXTRA FRILLS: extra hem ruffle + underskirt peek ──
            MakePart(visual.transform, PrimitiveType.Cylinder, "DressRuffle",
                new Vector3(0f, 0.24f, 0f), new Vector3(0.82f, 0.05f, 0.82f),
                new Color(0.70f, 0.45f, 0.90f));
            MakePart(visual.transform, PrimitiveType.Cylinder, "DressUnder",
                new Vector3(0f, 0.19f, 0f), new Vector3(0.68f, 0.06f, 0.68f),
                new Color(0.95f, 0.88f, 1.00f));
            MakePart(visual.transform, PrimitiveType.Cylinder, "DressApron",
                new Vector3(0f, 0.50f, 0.08f), new Vector3(0.50f, 0.20f, 0.20f),
                new Color(0.98f, 0.94f, 1.00f));

            // ── SPARKLE ACCENTS on dress ──
            for (int s = 0; s < 5; s++)
            {
                float ang = s * (Mathf.PI * 2f / 5f);
                MakeEmissive(visual.transform, $"DressGem{s}",
                    new Vector3(Mathf.Sin(ang) * 0.38f, 0.55f, Mathf.Cos(ang) * 0.20f),
                    Vector3.one * 0.035f,
                    new Color(1.0f, 0.85f, 0.55f), 1.3f);
            }
            // Pink hair bow on right side — anchored right against hair
            MakePartRotated(visual.transform, PrimitiveType.Cube, "BowL",
                new Vector3(0.20f, 2.10f, 0.02f),
                Quaternion.Euler(0f, 0f, 25f),
                new Vector3(0.11f, 0.09f, 0.05f),
                new Color(1.0f, 0.55f, 0.80f));
            MakePartRotated(visual.transform, PrimitiveType.Cube, "BowR",
                new Vector3(0.30f, 2.10f, 0.02f),
                Quaternion.Euler(0f, 0f, -25f),
                new Vector3(0.11f, 0.09f, 0.05f),
                new Color(1.0f, 0.55f, 0.80f));
            MakePart(visual.transform, PrimitiveType.Sphere, "BowKnot",
                new Vector3(0.25f, 2.10f, 0.03f), Vector3.one * 0.05f,
                new Color(1.0f, 0.35f, 0.65f));
            // Little star hair clip
            MakePartRotated(visual.transform, PrimitiveType.Cube, "StarClip",
                new Vector3(-0.28f, 2.08f, 0.02f),
                Quaternion.Euler(0f, 0f, 45f),
                Vector3.one * 0.08f,
                new Color(1.0f, 0.92f, 0.45f));
            // Smile — a tiny curved cube (approx a small rectangle under the eyes)
            MakePart(visual.transform, PrimitiveType.Cube, "Smile",
                new Vector3(0f, 1.80f, 0.20f), new Vector3(0.10f, 0.015f, 0.015f),
                new Color(0.80f, 0.30f, 0.45f));
            return visual;
        }

        static void MakeEmissive(Transform parent, string name,
            Vector3 pos, Vector3 scale, Color color, float intensity)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale    = scale;
            var mat = new Material(ToonShader);
            mat.SetColor("_Color",            color);
            mat.SetColor("_EmissionColor",    color);
            mat.SetFloat("_EmissionIntensity", intensity);
            mat.SetFloat("_OutlineWidth",      0f);
            go.GetComponent<MeshRenderer>().material = mat;
            Object.Destroy(go.GetComponent<Collider>());
        }

        static void MakePartRotated(Transform parent, PrimitiveType type, string name,
            Vector3 pos, Quaternion rot, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot;
            go.transform.localScale    = scale;
            SetToonMat(go, color);
            Object.Destroy(go.GetComponent<Collider>());
        }

        static void MakePart(Transform parent, PrimitiveType type, string name,
            Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale    = scale;
            SetToonMat(go, color);
            Object.Destroy(go.GetComponent<Collider>());
        }

        // ── Kenney CC0 prop spawner ──
        static GameObject SpawnKenney(Transform parent, string resourcePath,
            Vector3 pos, Vector3 eulerRot, float scale, Color tint)
        {
            var prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                Debug.LogWarning($"[Kenney] Missing Resources/{resourcePath}");
                return null;
            }
            var go = Object.Instantiate(prefab, parent);
            go.name = System.IO.Path.GetFileName(resourcePath);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(eulerRot);
            go.transform.localScale    = Vector3.one * scale;
            // Retint using toon shader for stylistic consistency
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                var m = new Material(ToonShader);
                m.SetColor("_Color", tint);
                m.SetFloat("_OutlineWidth", 0.003f);
                r.sharedMaterial = m;
            }
            return go;
        }

        // ── Rooms ────────────────────────────────────────────────────────────
        RoomManager CreateRooms()
        {
            var rmGO = new GameObject("RoomManager");
            var rm   = rmGO.AddComponent<RoomManager>();

            var lr = BuildRoom("LivingRoom", Vector3.zero,
                new Color(0.12f, 0.07f, 0.22f), true,
                new[]{ "Models/Props/LivingRoom/Sofa",
                       "Models/Props/LivingRoom/ToyChest",
                       "Models/Props/Shared/MoonLamp" },
                new[]{ new Vector3(-2f, 0f, 3f),
                       new Vector3( 2.5f, 0f, 2.5f),
                       new Vector3(-3.5f, 0f, -2f) },
                new[]{ Vector3.one * 1.2f, Vector3.one, Vector3.one * 1.1f },
                new[]{ new Color(0.55f, 0.30f, 0.80f),
                       new Color(0.40f, 0.20f, 0.65f),
                       new Color(0.70f, 0.50f, 0.90f) });
            lr.AddComponent<FeedingStation>();
            lr.AddComponent<PlayArea>();

            var kt = BuildRoom("Kitchen", new Vector3(14f, 0f, 0f),
                new Color(0.06f, 0.10f, 0.06f), false,
                new[]{ "Models/Props/Kitchen/Fridge",
                       "Models/Props/Kitchen/FoodBowl",
                       "Models/Props/Kitchen/Cake" },
                new[]{ new Vector3(3.5f, 0f, 3.5f),
                       new Vector3(-1f,  0f, 0f),
                       new Vector3( 1f,  0f, -1f) },
                new[]{ Vector3.one, Vector3.one * 0.8f, Vector3.one * 0.9f },
                new[]{ new Color(0.70f, 0.85f, 0.70f),
                       new Color(0.80f, 0.60f, 0.40f),
                       new Color(0.90f, 0.50f, 0.60f) });

            var bd = BuildRoom("Bedroom", new Vector3(-14f, 0f, 0f),
                new Color(0.04f, 0.04f, 0.18f), false,
                new[]{ "Models/Props/Bedroom/Bed",
                       "Models/Props/Bedroom/Wardrobe",
                       "Models/Props/Shared/MoonLamp" },
                new[]{ new Vector3(0f, 0f, 2f),
                       new Vector3(3.5f, 0f, 3f),
                       new Vector3(-3f, 0f, 0f) },
                new[]{ Vector3.one * 1.3f, Vector3.one * 1.2f, Vector3.one },
                new[]{ new Color(0.45f, 0.45f, 0.80f),
                       new Color(0.35f, 0.25f, 0.60f),
                       new Color(0.55f, 0.55f, 0.90f) });
            bd.AddComponent<SleepArea>();

            var gd = BuildRoom("Garden", new Vector3(0f, 0f, 14f),
                new Color(0.04f, 0.10f, 0.04f), false,
                new[]{ "Models/Props/Garden/MagicFlower",
                       "Models/Props/Garden/MagicWell" },
                new[]{ new Vector3(-2f, 0f, 1f),
                       new Vector3( 2f, 0f, 2f) },
                new[]{ Vector3.one, Vector3.one * 1.2f },
                new[]{ new Color(0.70f, 0.95f, 0.55f),
                       new Color(0.55f, 0.80f, 0.70f) });
            gd.AddComponent<GardenArea>();

            var lb = BuildRoom("Library", new Vector3(0f, 0f, -14f),
                new Color(0.09f, 0.05f, 0.02f), false,
                new[]{ "Models/Props/Library/Bookshelf",
                       "Models/Props/Library/ReadingChair" },
                new[]{ new Vector3(-3.5f, 0f, 3.5f),
                       new Vector3( 1f,   0f, 0f) },
                new[]{ Vector3.one * 1.2f, Vector3.one * 1.1f },
                new[]{ new Color(0.65f, 0.45f, 0.25f),
                       new Color(0.55f, 0.35f, 0.20f) });
            lb.AddComponent<LibraryRoom>();

            rm.AddRoom(RoomType.LivingRoom, lr);
            rm.AddRoom(RoomType.Kitchen,    kt);
            rm.AddRoom(RoomType.Bedroom,    bd);
            rm.AddRoom(RoomType.Garden,     gd);
            rm.AddRoom(RoomType.Library,    lb);

            return rm;
        }

        GameObject BuildRoom(string roomName, Vector3 pos, Color ambColor, bool active,
            string[] propPaths = null, Vector3[] propPositions = null,
            Vector3[] propScales = null, Color[] propColors = null)
        {
            var root = new GameObject(roomName);
            root.transform.position = pos;

            // Floor — warm wood planks (procedural texture)
            var floor = Prim(PrimitiveType.Cube, "Floor", root.transform,
                new Vector3(0f, -0.1f, 0f), new Vector3(10f, 0.18f, 10f),
                new Color(0.55f, 0.32f, 0.28f));
            ApplyTiledTexture(floor, ProcTextures.WoodPlanks(), new Vector2(3f, 3f));

            // Ceiling — starry sky texture
            var ceil = Prim(PrimitiveType.Cube, "Ceiling", root.transform,
                new Vector3(0f, 5.1f, 0f), new Vector3(10f, 0.2f, 10f),
                new Color(0.18f, 0.10f, 0.32f));
            ApplyTiledTexture(ceil, ProcTextures.CeilingSky(), new Vector2(2f, 2f));

            // Ceiling stars (emissive tiny spheres) — 16 random points
            for (int cs = 0; cs < 16; cs++)
            {
                var cStar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cStar.name = $"CStar{cs}";
                cStar.transform.SetParent(root.transform, false);
                cStar.transform.localPosition = new Vector3(
                    Random.Range(-4.5f, 4.5f), 4.95f, Random.Range(-4.5f, 4.5f));
                cStar.transform.localScale = Vector3.one * Random.Range(0.04f, 0.09f);
                var csMat = new Material(ToonShader);
                var c = new Color(1f, Random.Range(0.85f, 1f), Random.Range(0.75f, 1f));
                csMat.SetColor("_Color",            c);
                csMat.SetColor("_EmissionColor",    c);
                csMat.SetFloat("_EmissionIntensity", 2.2f);
                csMat.SetFloat("_OutlineWidth",      0f);
                cStar.GetComponent<MeshRenderer>().material = csMat;
                Object.Destroy(cStar.GetComponent<Collider>());
                cStar.AddComponent<StarTwinkle>();
            }

            // Walls — striped wallpaper (procedural)
            Color wallCol = new Color(0.22f, 0.14f, 0.38f);
            var wb = Prim(PrimitiveType.Cube, "WallBack",  root.transform, new Vector3(0f, 2.5f,  5f), new Vector3(10f, 5f, 0.2f), wallCol);
            var wf = Prim(PrimitiveType.Cube, "WallFront", root.transform, new Vector3(0f, 2.5f, -5f), new Vector3(10f, 5f, 0.2f), wallCol);
            var wr = Prim(PrimitiveType.Cube, "WallRight", root.transform, new Vector3( 5f, 2.5f, 0f), new Vector3(0.2f, 5f, 10f), wallCol);
            var wl = Prim(PrimitiveType.Cube, "WallLeft",  root.transform, new Vector3(-5f, 2.5f, 0f), new Vector3(0.2f, 5f, 10f), wallCol);
            var wallTex = ProcTextures.Wallpaper();
            ApplyTiledTexture(wb, wallTex, new Vector2(3f, 1.2f));
            ApplyTiledTexture(wf, wallTex, new Vector2(3f, 1.2f));
            ApplyTiledTexture(wr, wallTex, new Vector2(3f, 1.2f));
            ApplyTiledTexture(wl, wallTex, new Vector2(3f, 1.2f));

            // Skirting boards
            Color skirt = new Color(0.20f, 0.14f, 0.32f);
            Prim(PrimitiveType.Cube, "SkirtBack",  root.transform, new Vector3(0f, 0.12f,  4.9f), new Vector3(9.8f, 0.24f, 0.1f), skirt);
            Prim(PrimitiveType.Cube, "SkirtFront", root.transform, new Vector3(0f, 0.12f, -4.9f), new Vector3(9.8f, 0.24f, 0.1f), skirt);
            Prim(PrimitiveType.Cube, "SkirtRight", root.transform, new Vector3( 4.9f, 0.12f, 0f), new Vector3(0.1f, 0.24f, 9.8f), skirt);
            Prim(PrimitiveType.Cube, "SkirtLeft",  root.transform, new Vector3(-4.9f, 0.12f, 0f), new Vector3(0.1f, 0.24f, 9.8f), skirt);

            // Ambient
            var amb = root.AddComponent<RoomAmbience>();
            amb.ambientColor = ambColor;

            // Room point light
            var lGO = new GameObject("RoomLight");
            lGO.transform.SetParent(root.transform, false);
            lGO.transform.localPosition = new Vector3(0f, 4f, 0f);
            var rl = lGO.AddComponent<Light>();
            rl.type      = LightType.Point;
            rl.color     = Color.Lerp(ambColor, Color.white, 0.5f);
            rl.intensity = 2.0f;
            rl.range     = 12f;

            // Load FBX props
            if (propPaths != null)
            {
                for (int i = 0; i < propPaths.Length; i++)
                {
                    var prefab = Resources.Load<GameObject>(propPaths[i]);
                    if (prefab == null) continue;
                    var prop = Object.Instantiate(prefab, root.transform);
                    prop.name = propPaths[i].Split('/')[^1];
                    prop.transform.localPosition = propPositions != null && i < propPositions.Length
                        ? propPositions[i] : Vector3.zero;
                    prop.transform.localScale = propScales != null && i < propScales.Length
                        ? propScales[i] : Vector3.one;
                    Color tint = propColors != null && i < propColors.Length
                        ? propColors[i] : Color.white;
                    ApplyToonToAll(prop, tint);
                }
            }

            // Always add a MoonLamp primitive so the room looks furnished even if FBX props fail
            if (active)
            {
                // Lamp post
                Prim(PrimitiveType.Cylinder, "LampBase", root.transform,
                    new Vector3(-3.2f, 0.3f, -2f), new Vector3(0.12f, 0.3f, 0.12f),
                    new Color(0.30f, 0.18f, 0.50f));
                Prim(PrimitiveType.Cylinder, "LampStem", root.transform,
                    new Vector3(-3.2f, 1.1f, -2f), new Vector3(0.06f, 0.8f, 0.06f),
                    new Color(0.35f, 0.22f, 0.55f));
                var lampHead = Prim(PrimitiveType.Sphere, "LampGlobe", root.transform,
                    new Vector3(-3.2f, 2.0f, -2f), Vector3.one * 0.28f,
                    new Color(1.0f, 0.95f, 0.70f));
                // Make lamp globe emissive
                var lampMat = new Material(ToonShader);
                lampMat.SetColor("_Color",     new Color(1.0f, 0.95f, 0.70f));
                lampMat.SetColor("_EmissionColor", new Color(1.0f, 0.90f, 0.50f));
                lampMat.SetFloat("_EmissionIntensity", 1.2f);
                lampMat.SetFloat("_OutlineWidth", 0f);
                lampHead.GetComponent<MeshRenderer>().material = lampMat;
                // Lamp light
                var lampLGO = new GameObject("LampLight");
                lampLGO.transform.SetParent(root.transform, false);
                lampLGO.transform.localPosition = new Vector3(-3.2f, 2.0f, -2f);
                var lampL = lampLGO.AddComponent<Light>();
                lampL.type      = LightType.Point;
                lampL.color     = new Color(1.0f, 0.88f, 0.55f);
                lampL.intensity = 1.5f;
                lampL.range     = 8f;

                // Sofa — velvet fabric (procedural texture)
                var velvet = ProcTextures.Velvet();
                var sofaBase = Prim(PrimitiveType.Cube, "SofaBase", root.transform,
                    new Vector3(-1.5f, 0.3f, 3f), new Vector3(2.4f, 0.55f, 0.9f),
                    new Color(0.72f, 0.38f, 0.90f));
                var sofaBack = Prim(PrimitiveType.Cube, "SofaBack", root.transform,
                    new Vector3(-1.5f, 0.85f, 3.4f), new Vector3(2.4f, 0.65f, 0.2f),
                    new Color(0.66f, 0.33f, 0.84f));
                var sofaArmL = Prim(PrimitiveType.Cube, "SofaArmL", root.transform,
                    new Vector3(-2.75f, 0.55f, 3f), new Vector3(0.2f, 0.6f, 0.9f),
                    new Color(0.60f, 0.28f, 0.80f));
                var sofaArmR = Prim(PrimitiveType.Cube, "SofaArmR", root.transform,
                    new Vector3(-0.25f, 0.55f, 3f), new Vector3(0.2f, 0.6f, 0.9f),
                    new Color(0.60f, 0.28f, 0.80f));
                ApplyTiledTexture(sofaBase, velvet, new Vector2(2f, 1f));
                ApplyTiledTexture(sofaBack, velvet, new Vector2(2f, 1f));
                ApplyTiledTexture(sofaArmL, velvet, new Vector2(1f, 1f));
                ApplyTiledTexture(sofaArmR, velvet, new Vector2(1f, 1f));

                // Cushion
                Prim(PrimitiveType.Cube, "Cushion", root.transform,
                    new Vector3(-1.5f, 0.65f, 2.85f), new Vector3(0.7f, 0.18f, 0.55f),
                    new Color(0.85f, 0.55f, 0.95f));

                // Coffee table — light wood top
                var tableTop = Prim(PrimitiveType.Cube, "TableTop", root.transform,
                    new Vector3(1.2f, 0.5f, 1.5f), new Vector3(1.2f, 0.08f, 0.7f),
                    new Color(0.70f, 0.48f, 0.32f));
                ApplyTiledTexture(tableTop, ProcTextures.LightWood(), new Vector2(1f, 1f));
                Prim(PrimitiveType.Cube, "TableLeg1", root.transform,
                    new Vector3(0.65f, 0.25f, 1.2f), new Vector3(0.1f, 0.5f, 0.1f),
                    new Color(0.25f, 0.14f, 0.40f));
                Prim(PrimitiveType.Cube, "TableLeg2", root.transform,
                    new Vector3(1.75f, 0.25f, 1.2f), new Vector3(0.1f, 0.5f, 0.1f),
                    new Color(0.25f, 0.14f, 0.40f));
                Prim(PrimitiveType.Cube, "TableLeg3", root.transform,
                    new Vector3(0.65f, 0.25f, 1.8f), new Vector3(0.1f, 0.5f, 0.1f),
                    new Color(0.25f, 0.14f, 0.40f));
                Prim(PrimitiveType.Cube, "TableLeg4", root.transform,
                    new Vector3(1.75f, 0.25f, 1.8f), new Vector3(0.1f, 0.5f, 0.1f),
                    new Color(0.25f, 0.14f, 0.40f));

                // Rug — knit diamond pattern (procedural)
                var rug = Prim(PrimitiveType.Cube, "Rug", root.transform,
                    new Vector3(0f, 0.01f, 1f), new Vector3(4.5f, 0.02f, 3.5f),
                    new Color(0.55f, 0.30f, 0.82f));
                ApplyTiledTexture(rug, ProcTextures.Rug(), new Vector2(1f, 1f));

                // Toy chest — light wood
                var chestBody = Prim(PrimitiveType.Cube, "ChestBody", root.transform,
                    new Vector3(3f, 0.28f, 2.8f), new Vector3(0.8f, 0.56f, 0.55f),
                    new Color(0.75f, 0.52f, 0.36f));
                var chestLid  = Prim(PrimitiveType.Cube, "ChestLid", root.transform,
                    new Vector3(3f, 0.62f, 2.8f), new Vector3(0.82f, 0.14f, 0.57f),
                    new Color(0.82f, 0.58f, 0.40f));
                ApplyTiledTexture(chestBody, ProcTextures.LightWood(), new Vector2(1f, 1f));
                ApplyTiledTexture(chestLid,  ProcTextures.LightWood(), new Vector2(1f, 1f));

                // Window frame on back wall
                Prim(PrimitiveType.Cube, "WinFrameOuter", root.transform,
                    new Vector3(2.5f, 2.5f, 4.85f), new Vector3(2.0f, 1.6f, 0.12f),
                    new Color(0.20f, 0.12f, 0.36f));
                // Window glass — deep blue-night
                var winGlass = new GameObject("WinGlass");
                winGlass.transform.SetParent(root.transform, false);
                winGlass.transform.localPosition = new Vector3(2.5f, 2.5f, 4.87f);
                winGlass.transform.localScale    = new Vector3(1.7f, 1.3f, 0.04f);
                var winMat = new Material(Shader.Find("Standard"));
                winMat.color = new Color(0.02f, 0.04f, 0.18f, 0.85f);
                winMat.SetFloat("_Mode", 3);   // Transparent
                winMat.SetInt("_SrcBlend",  (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                winMat.SetInt("_DstBlend",  (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                winMat.SetInt("_ZWrite", 0);
                winMat.DisableKeyword("_ALPHATEST_ON");
                winMat.EnableKeyword("_ALPHABLEND_ON");
                winMat.renderQueue = 3000;
                var winCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                winCube.name = "Glass";
                winCube.transform.SetParent(root.transform, false);
                winCube.transform.localPosition = new Vector3(2.5f, 2.5f, 4.87f);
                winCube.transform.localScale    = new Vector3(1.7f, 1.3f, 0.04f);
                winCube.GetComponent<MeshRenderer>().material = winMat;
                Object.Destroy(winCube.GetComponent<Collider>());
                // Starry sky panel behind window (opaque, shows stars through)
                var sky = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sky.name = "WinSky";
                sky.transform.SetParent(root.transform, false);
                sky.transform.localPosition = new Vector3(2.5f, 2.5f, 4.93f);
                sky.transform.localScale    = new Vector3(1.68f, 1.28f, 0.02f);
                var skyMat = new Material(ToonShader);
                skyMat.SetColor("_Color",            new Color(0.50f, 0.40f, 0.75f));
                skyMat.SetColor("_EmissionColor",    new Color(0.35f, 0.25f, 0.55f));
                skyMat.SetFloat("_EmissionIntensity", 0.6f);
                skyMat.SetFloat("_OutlineWidth",      0f);
                skyMat.mainTexture      = ProcTextures.CeilingSky();
                skyMat.mainTextureScale = new Vector2(1.5f, 1.2f);
                sky.GetComponent<MeshRenderer>().material = skyMat;
                Object.Destroy(sky.GetComponent<Collider>());
                // Moon halo — bigger translucent sphere behind moon for glow
                var halo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                halo.name = "MoonHalo";
                halo.transform.SetParent(root.transform, false);
                halo.transform.localPosition = new Vector3(2.5f, 2.7f, 4.92f);
                halo.transform.localScale    = Vector3.one * 0.55f;
                var haloMat = new Material(ToonShader);
                haloMat.SetColor("_Color",           new Color(0.95f, 0.92f, 0.70f, 0.25f));
                haloMat.SetColor("_EmissionColor",   new Color(0.95f, 0.85f, 0.55f));
                haloMat.SetFloat("_EmissionIntensity", 0.7f);
                haloMat.SetFloat("_OutlineWidth", 0f);
                halo.GetComponent<MeshRenderer>().material = haloMat;
                Object.Destroy(halo.GetComponent<Collider>());

                // Moon circle in window
                var moonCirc = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                moonCirc.name = "MoonCircle";
                moonCirc.transform.SetParent(root.transform, false);
                moonCirc.transform.localPosition = new Vector3(2.5f, 2.7f, 4.9f);
                moonCirc.transform.localScale    = Vector3.one * 0.32f;
                var moonMat = new Material(ToonShader);
                moonMat.SetColor("_Color",           new Color(0.95f, 0.95f, 0.80f));
                moonMat.SetColor("_EmissionColor",   new Color(0.90f, 0.90f, 0.60f));
                moonMat.SetFloat("_EmissionIntensity", 1.5f);
                moonMat.SetFloat("_OutlineWidth", 0f);
                moonCirc.GetComponent<MeshRenderer>().material = moonMat;
                Object.Destroy(moonCirc.GetComponent<Collider>());
                moonCirc.AddComponent<MoonArc>();
                // Stars (small spheres near window)
                for (int s = 0; s < 8; s++)
                {
                    var star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    star.name = $"Star{s}";
                    star.transform.SetParent(root.transform, false);
                    float sx = 1.8f + Random.Range(-0.7f, 0.7f);
                    float sy = 2.0f + Random.Range(-0.5f, 0.7f);
                    star.transform.localPosition = new Vector3(sx, sy, 4.91f);
                    star.transform.localScale    = Vector3.one * Random.Range(0.02f, 0.05f);
                    var sMat = new Material(ToonShader);
                    sMat.SetColor("_Color",           Color.white);
                    sMat.SetColor("_EmissionColor",   Color.white);
                    sMat.SetFloat("_EmissionIntensity", 2f);
                    sMat.SetFloat("_OutlineWidth", 0f);
                    star.GetComponent<MeshRenderer>().material = sMat;
                    Object.Destroy(star.GetComponent<Collider>());
                    star.AddComponent<StarTwinkle>();
                }
                // Bookshelf on left wall
                Prim(PrimitiveType.Cube, "Bookshelf", root.transform,
                    new Vector3(-4.3f, 1.2f, -2.5f), new Vector3(0.35f, 2.4f, 1.6f),
                    new Color(0.32f, 0.18f, 0.48f));
                // Books (colored cubes)
                Color[] bookCols = {
                    new Color(0.90f, 0.45f, 0.55f), new Color(0.40f, 0.75f, 0.95f),
                    new Color(0.95f, 0.85f, 0.40f), new Color(0.55f, 0.85f, 0.55f),
                    new Color(0.85f, 0.55f, 0.95f), new Color(0.95f, 0.65f, 0.40f),
                };
                for (int shelf = 0; shelf < 3; shelf++)
                {
                    float shelfY = 0.5f + shelf * 0.65f;
                    for (int b = 0; b < 6; b++)
                    {
                        float bookZ = -3.1f + b * 0.22f;
                        Prim(PrimitiveType.Cube, $"Book_{shelf}_{b}", root.transform,
                            new Vector3(-4.15f, shelfY, bookZ),
                            new Vector3(0.18f, Random.Range(0.45f, 0.58f), 0.18f),
                            bookCols[(shelf * 6 + b) % bookCols.Length]);
                    }
                }

                // Potted plant
                Prim(PrimitiveType.Cylinder, "PotBase", root.transform,
                    new Vector3(4.2f, 0.25f, -3.8f), new Vector3(0.28f, 0.25f, 0.28f),
                    new Color(0.45f, 0.28f, 0.18f));
                Prim(PrimitiveType.Sphere, "Leaf1", root.transform,
                    new Vector3(4.2f, 0.65f, -3.8f), new Vector3(0.35f, 0.30f, 0.35f),
                    new Color(0.45f, 0.80f, 0.45f));
                Prim(PrimitiveType.Sphere, "Leaf2", root.transform,
                    new Vector3(4.1f, 0.85f, -3.7f), new Vector3(0.28f, 0.25f, 0.28f),
                    new Color(0.40f, 0.72f, 0.40f));
                Prim(PrimitiveType.Sphere, "Leaf3", root.transform,
                    new Vector3(4.3f, 0.85f, -3.9f), new Vector3(0.26f, 0.22f, 0.26f),
                    new Color(0.55f, 0.85f, 0.50f));
                Prim(PrimitiveType.Sphere, "Flower", root.transform,
                    new Vector3(4.2f, 1.05f, -3.8f), Vector3.one * 0.14f,
                    new Color(1.0f, 0.5f, 0.75f));

                // Wall picture above sofa
                Prim(PrimitiveType.Cube, "PictureFrame", root.transform,
                    new Vector3(-1.5f, 3.3f, 4.88f), new Vector3(1.6f, 1.1f, 0.08f),
                    new Color(0.85f, 0.70f, 0.35f));
                Prim(PrimitiveType.Cube, "PictureCanvas", root.transform,
                    new Vector3(-1.5f, 3.3f, 4.87f), new Vector3(1.4f, 0.9f, 0.06f),
                    new Color(0.45f, 0.65f, 0.90f));
                // A little sun + hill on the picture
                Prim(PrimitiveType.Sphere, "PicSun", root.transform,
                    new Vector3(-1.1f, 3.45f, 4.86f), Vector3.one * 0.18f,
                    new Color(1.0f, 0.85f, 0.40f));
                Prim(PrimitiveType.Sphere, "PicHill", root.transform,
                    new Vector3(-1.7f, 3.05f, 4.86f), new Vector3(0.9f, 0.4f, 0.08f),
                    new Color(0.50f, 0.75f, 0.50f));

                // Wall clock — flat disc on right wall (rotated 90° around Z so it's vertical)
                MakePartRotated(root.transform, PrimitiveType.Cylinder, "ClockBody",
                    new Vector3(4.88f, 3.5f, 1f),
                    Quaternion.Euler(0f, 0f, 90f),
                    new Vector3(0.55f, 0.06f, 0.55f),
                    new Color(0.95f, 0.92f, 0.80f));
                // Hour/minute hands (flat cubes on clock face)
                Prim(PrimitiveType.Cube, "ClockHandH", root.transform,
                    new Vector3(4.84f, 3.5f, 1f), new Vector3(0.03f, 0.20f, 0.03f),
                    new Color(0.15f, 0.08f, 0.25f));
                Prim(PrimitiveType.Cube, "ClockHandM", root.transform,
                    new Vector3(4.84f, 3.58f, 1.12f), new Vector3(0.03f, 0.03f, 0.32f),
                    new Color(0.25f, 0.12f, 0.35f));

                // Stuffed bunny on rug
                Prim(PrimitiveType.Sphere, "BunnyBody", root.transform,
                    new Vector3(1.8f, 0.22f, -0.2f), new Vector3(0.28f, 0.30f, 0.30f),
                    new Color(0.95f, 0.85f, 0.90f));
                Prim(PrimitiveType.Sphere, "BunnyHead", root.transform,
                    new Vector3(1.8f, 0.47f, -0.3f), Vector3.one * 0.18f,
                    new Color(1.0f, 0.90f, 0.92f));
                Prim(PrimitiveType.Capsule, "BunnyEarL", root.transform,
                    new Vector3(1.73f, 0.62f, -0.3f), new Vector3(0.05f, 0.12f, 0.05f),
                    new Color(1.0f, 0.75f, 0.85f));
                Prim(PrimitiveType.Capsule, "BunnyEarR", root.transform,
                    new Vector3(1.87f, 0.62f, -0.3f), new Vector3(0.05f, 0.12f, 0.05f),
                    new Color(1.0f, 0.75f, 0.85f));
                Prim(PrimitiveType.Sphere, "BunnyEyeL", root.transform,
                    new Vector3(1.76f, 0.49f, -0.44f), Vector3.one * 0.025f,
                    new Color(0.08f, 0.04f, 0.18f));
                Prim(PrimitiveType.Sphere, "BunnyEyeR", root.transform,
                    new Vector3(1.85f, 0.49f, -0.44f), Vector3.one * 0.025f,
                    new Color(0.08f, 0.04f, 0.18f));

                // Star-shaped cushion on rug
                Prim(PrimitiveType.Cube, "FloorCushion", root.transform,
                    new Vector3(-0.5f, 0.15f, -0.5f), new Vector3(0.6f, 0.18f, 0.6f),
                    new Color(0.95f, 0.70f, 0.95f));

                // Balloon floating in corner
                Prim(PrimitiveType.Sphere, "Balloon", root.transform,
                    new Vector3(3.8f, 3.2f, -3.5f), Vector3.one * 0.45f,
                    new Color(1.0f, 0.55f, 0.75f));
                Prim(PrimitiveType.Cube, "BalloonString", root.transform,
                    new Vector3(3.8f, 2.6f, -3.5f), new Vector3(0.02f, 0.7f, 0.02f),
                    new Color(0.85f, 0.85f, 0.90f));

                // ── Kenney CC0 furniture pass (native scale ~3x, so use ~0.35) ──
                SpawnKenney(root.transform, "Kenney/Furniture/bookcaseOpen",
                    new Vector3(-4.6f, 0f, 0.8f), new Vector3(0f, 90f, 0f), 0.40f,
                    new Color(0.68f, 0.45f, 0.30f));
                SpawnKenney(root.transform, "Kenney/Furniture/books",
                    new Vector3(-4.40f, 0.85f, 0.8f), new Vector3(0f, 90f, 0f), 0.35f,
                    new Color(0.85f, 0.55f, 0.75f));
                SpawnKenney(root.transform, "Kenney/Furniture/lampRoundFloor",
                    new Vector3(-3.8f, 0f, -2.2f), Vector3.zero, 0.42f,
                    new Color(1.00f, 0.88f, 0.72f));
                SpawnKenney(root.transform, "Kenney/Furniture/pottedPlant",
                    new Vector3(4.0f, 0f, -2.6f), Vector3.zero, 0.38f,
                    new Color(0.55f, 0.88f, 0.55f));
                SpawnKenney(root.transform, "Kenney/Furniture/plantSmall2",
                    new Vector3(1.2f, 0.55f, 1.5f), Vector3.zero, 0.22f,
                    new Color(0.55f, 0.88f, 0.55f));
                SpawnKenney(root.transform, "Kenney/Furniture/chairCushion",
                    new Vector3(3.5f, 0f, -0.5f), new Vector3(0f, -120f, 0f), 0.35f,
                    new Color(0.85f, 0.45f, 0.90f));
                SpawnKenney(root.transform, "Kenney/Furniture/pillowLong",
                    new Vector3(-1.5f, 0.62f, 3.1f), new Vector3(0f, 0f, 0f), 0.30f,
                    new Color(1.0f, 0.75f, 0.88f));
                // Layered books on the bookcase shelves
                SpawnKenney(root.transform, "Kenney/Furniture/books",
                    new Vector3(-4.45f, 0.28f, 1.1f), new Vector3(0f, 90f, 0f), 0.30f,
                    new Color(0.55f, 0.75f, 1.00f));
                SpawnKenney(root.transform, "Kenney/Furniture/books",
                    new Vector3(-4.45f, 1.45f, 0.5f), new Vector3(0f, 90f, 0f), 0.32f,
                    new Color(1.00f, 0.72f, 0.55f));
                SpawnKenney(root.transform, "Kenney/Furniture/books",
                    new Vector3(-4.45f, 1.45f, 1.1f), new Vector3(0f, -90f, 0f), 0.30f,
                    new Color(0.72f, 0.92f, 0.70f));
                // TV cabinet on right wall
                SpawnKenney(root.transform, "Kenney/Furniture/cabinetTelevision",
                    new Vector3(4.6f, 0f, 2.0f), new Vector3(0f, -90f, 0f), 0.40f,
                    new Color(0.65f, 0.42f, 0.28f));
                // Table lamp on the cabinet (warm emissive applied separately)
                var tlamp = SpawnKenney(root.transform, "Kenney/Furniture/lampRoundTable",
                    new Vector3(4.5f, 1.30f, 2.0f), Vector3.zero, 0.28f,
                    new Color(1.00f, 0.82f, 0.55f));
                if (tlamp != null)
                {
                    var lgo = new GameObject("TableLampLight");
                    lgo.transform.SetParent(tlamp.transform, false);
                    lgo.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                    var pl = lgo.AddComponent<Light>();
                    pl.type      = LightType.Point;
                    pl.color     = new Color(1.0f, 0.80f, 0.50f);
                    pl.intensity = 1.8f;
                    pl.range     = 4f;
                }
                // Extra small pillow on sofa
                SpawnKenney(root.transform, "Kenney/Furniture/pillowLong",
                    new Vector3(-0.6f, 0.62f, 3.0f), new Vector3(0f, 15f, 0f), 0.24f,
                    new Color(0.78f, 0.62f, 1.00f));
                // Round rug at hallway entrance
                SpawnKenney(root.transform, "Kenney/Furniture/rugRound",
                    new Vector3(4.2f, 0.02f, 0.5f), Vector3.zero, 0.55f,
                    new Color(0.88f, 0.55f, 0.95f));

                // Window light — moonbeam
                var winLGO = new GameObject("WindowMoonbeam");
                winLGO.transform.SetParent(root.transform, false);
                winLGO.transform.localPosition = new Vector3(2.5f, 3f, 4f);
                var winL = winLGO.AddComponent<Light>();
                winL.type      = LightType.Spot;
                winL.color     = new Color(0.78f, 0.88f, 1.00f);
                winL.intensity = 1.2f;
                winL.range     = 8f;
                winL.spotAngle = 45f;
                winL.transform.localRotation = Quaternion.Euler(60f, -160f, 0f);
            }

            root.SetActive(active);
            return root;
        }

        // ── UI ───────────────────────────────────────────────────────────────
        (GameObject go, MoonlightUI ui) CreateUI()
        {
            // Root Canvas
            var canvasGO = new GameObject("UICanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);   // landscape
            scaler.matchWidthOrHeight  = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // HUD panel (top) — taller so info labels fit, very transparent
            var hud = Panel("HUD", canvasGO.transform,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -200f), new Vector2(0f, 0f),
                new Color(0.04f, 0.02f, 0.10f, 0.35f));

            // Stat sliders
            float[] xPos   = { -420f, -210f, 0f, 210f, 420f };
            string[] names = { "Wonder", "Warmth", "Rest", "Magic", "Hunger" };
            Color[]  cols  = {
                new Color(0.4f, 0.8f, 1.0f),
                new Color(1.0f, 0.6f, 0.8f),
                new Color(0.6f, 0.9f, 0.6f),
                new Color(0.8f, 0.4f, 1.0f),
                new Color(1.0f, 0.8f, 0.4f)
            };

            string[] shortNames = { "WONDER", "WARMTH", "REST", "MAGIC", "HUNGER" };
            var sliders = new Slider[5];
            for (int i = 0; i < 5; i++)
            {
                // Anchor to top-center of HUD so positions are predictable
                var res = new DefaultControls.Resources();
                var slGO = DefaultControls.CreateSlider(res);
                slGO.name = names[i] + "Bar";
                slGO.transform.SetParent(hud.transform, false);
                var slRt = slGO.GetComponent<RectTransform>();
                slRt.anchorMin        = new Vector2(0.5f, 1f);
                slRt.anchorMax        = new Vector2(0.5f, 1f);
                slRt.pivot            = new Vector2(0.5f, 0.5f);
                slRt.anchoredPosition = new Vector2(xPos[i], -90f);
                slRt.sizeDelta        = new Vector2(200f, 26f);
                var sl = slGO.GetComponent<Slider>();
                sl.value = 0.8f;
                sl.interactable = false;
                // Color fill
                var fill = slGO.transform.Find("Fill Area/Fill");
                if (fill)
                {
                    var img = fill.GetComponent<Image>();
                    if (img) img.color = cols[i];
                }
                sliders[i] = sl;

                // Label above slider (legacy UI.Text — guaranteed to render)
                MakeLegacyLabel(names[i] + "Lbl", hud.transform,
                    new Vector2(xPos[i], -55f), new Vector2(200f, 26f),
                    shortNames[i], 18, cols[i], FontStyle.Bold);
            }

            // Info labels (stage, mood, coins, xp, days) — legacy Text so they always show
            var stageLblGO = MakeLegacyLabel("StageLabel", hud.transform, new Vector2(-420f, -145f), new Vector2(220f, 30f), "Moonbud", 20, Color.white, FontStyle.Bold);
            var moodLblGO  = MakeLegacyLabel("MoodLabel",  hud.transform, new Vector2(-200f, -145f), new Vector2(120f, 30f), "HAPPY",   18, new Color(1f, 0.7f, 0.9f), FontStyle.Bold);
            var coinsLblGO = MakeLegacyLabel("CoinsLabel", hud.transform, new Vector2(  40f, -145f), new Vector2(180f, 30f), "COINS 30", 20, new Color(1f, 0.9f, 0.3f), FontStyle.Bold);
            var xpLblGO    = MakeLegacyLabel("XPLabel",    hud.transform, new Vector2( 240f, -145f), new Vector2(160f, 30f), "XP 0",    18, new Color(0.75f, 0.55f, 1f), FontStyle.Bold);
            var daysLblGO  = MakeLegacyLabel("DaysLabel",  hud.transform, new Vector2( 420f, -145f), new Vector2(140f, 30f), "DAY 1",   18, new Color(0.7f, 0.9f, 1f), FontStyle.Bold);

            // Wrap legacy Text in TMP_Text-compatible adapters? No — MoonlightUI.Wire needs TMP_Text.
            // Create hidden TMP labels that mirror nothing but satisfy the signature — OR swap Wire signature.
            // Simpler: create invisible TMP labels purely for Wire(), and the visible info is the legacy labels above.
            // MoonlightUI will update the invisible TMP labels; we also write their text to the legacy ones via a small sync component.
            var stageLabel = MakeTMPLabelAnchored("StageLabelTMP", hud.transform, new Vector2(-9999f, 0f), new Vector2(1f, 1f), "Moonbud",  1, new Color(0,0,0,0));
            var moodLabel  = MakeTMPLabelAnchored("MoodLabelTMP",  hud.transform, new Vector2(-9999f, 0f), new Vector2(1f, 1f), "HAPPY",    1, new Color(0,0,0,0));
            var coinsLabel = MakeTMPLabelAnchored("CoinsLabelTMP", hud.transform, new Vector2(-9999f, 0f), new Vector2(1f, 1f), "30",       1, new Color(0,0,0,0));
            var xpLabel    = MakeTMPLabelAnchored("XPLabelTMP",    hud.transform, new Vector2(-9999f, 0f), new Vector2(1f, 1f), "XP 0",     1, new Color(0,0,0,0));
            var daysLabel  = MakeTMPLabelAnchored("DaysLabelTMP",  hud.transform, new Vector2(-9999f, 0f), new Vector2(1f, 1f), "Day 1",    1, new Color(0,0,0,0));

            // Add a TMP→legacy mirror so the visible legacy labels reflect the game state
            var mirror = canvasGO.AddComponent<LegacyLabelMirror>();
            mirror.Bind(stageLabel, stageLblGO, "",
                        moodLabel,  moodLblGO,  "",
                        coinsLabel, coinsLblGO, "COINS ",
                        xpLabel,    xpLblGO,    "",
                        daysLabel,  daysLblGO,  "");

            // Action buttons (bottom)
            var btnPanel = Panel("ActionBar", canvasGO.transform,
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, 0f), new Vector2(0f, 160f),
                new Color(0f, 0f, 0f, 0.55f));

            var feedBtn   = MakeButton("FeedBtn",   btnPanel.transform, new Vector2(-280f, 60f), "FEED",   new Color(0.9f, 0.6f, 0.2f));
            var cuddleBtn = MakeButton("CuddleBtn", btnPanel.transform, new Vector2(   0f, 60f), "CUDDLE", new Color(0.9f, 0.4f, 0.7f));
            var sleepBtn  = MakeButton("SleepBtn",  btnPanel.transform, new Vector2( 280f, 60f), "SLEEP",  new Color(0.3f, 0.5f, 0.9f));

            // Visual feedback: particle bursts on button click (finds Moonlight by name at click time)
            AttachBurst(feedBtn,   new Color(1.0f, 0.75f, 0.35f), 14);
            AttachBurst(cuddleBtn, new Color(1.0f, 0.45f, 0.70f), 20);
            AttachBurst(sleepBtn,  new Color(0.55f, 0.70f, 1.0f), 12);

            // Feed menu overlay
            var feedMenu = Panel("FeedMenu", canvasGO.transform,
                new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.85f),
                Vector2.zero, Vector2.zero,
                new Color(0.08f, 0.04f, 0.18f, 0.95f));
            feedMenu.SetActive(false);

            // Close button for feed menu
            var closeBtn = MakeButton("CloseBtn", feedMenu.transform, new Vector2(0f, -200f), "✕ Close", new Color(0.5f, 0.2f, 0.2f));
            closeBtn.onClick.AddListener(() => feedMenu.SetActive(false));

            // Scroll content area inside feed menu
            var contentGO = new GameObject("FeedContent");
            contentGO.transform.SetParent(feedMenu.transform, false);
            contentGO.AddComponent<RectTransform>().anchoredPosition = Vector2.zero;

            // Stage celebration overlay
            var stgPanel = Panel("StagePanel", canvasGO.transform,
                new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.7f),
                Vector2.zero, Vector2.zero,
                new Color(0.06f, 0.02f, 0.18f, 0.92f));
            stgPanel.SetActive(false);
            var stgLabel = MakeTMPLabel("StgLabel", stgPanel.transform, new Vector2(0f, 0f), new Vector2(600f, 80f), "✨ Stage Up!", 32, Color.white);

            // Room unlock overlay
            var roomPanel = Panel("RoomPanel", canvasGO.transform,
                new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.65f),
                Vector2.zero, Vector2.zero,
                new Color(0.04f, 0.08f, 0.04f, 0.92f));
            roomPanel.SetActive(false);
            var roomLabel = MakeTMPLabel("RoomLabel", roomPanel.transform, new Vector2(0f, 0f), new Vector2(600f, 80f), "🌙 Room Unlocked!", 28, Color.white);

            // Offline panel
            var offlinePanel = Panel("OfflinePanel", canvasGO.transform,
                new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.6f),
                Vector2.zero, Vector2.zero,
                new Color(0.18f, 0.08f, 0.04f, 0.92f));
            offlinePanel.SetActive(false);
            MakeTMPLabel("OfflineLbl", offlinePanel.transform, Vector2.zero, new Vector2(600f, 60f), "Moonlight missed you! 🌙", 26, new Color(1f, 0.8f, 0.5f));

            // Sleep overlay
            var sleepOvr = Panel("SleepOverlay", canvasGO.transform,
                Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero,
                new Color(0.02f, 0.02f, 0.10f, 0.75f));
            sleepOvr.SetActive(false);
            MakeTMPLabel("SleepLbl", sleepOvr.transform, Vector2.zero, new Vector2(400f, 80f), "💤 zzzz...", 42, new Color(0.7f, 0.8f, 1f));

            // Quit button — top-right corner
            var quitRes = new DefaultControls.Resources();
            var quitGO  = DefaultControls.CreateButton(quitRes);
            quitGO.name = "QuitBtn";
            quitGO.transform.SetParent(canvasGO.transform, false);
            var quitRt = quitGO.GetComponent<RectTransform>();
            quitRt.anchorMin        = new Vector2(1f, 1f);
            quitRt.anchorMax        = new Vector2(1f, 1f);
            quitRt.pivot            = new Vector2(1f, 1f);
            quitRt.anchoredPosition = new Vector2(-20f, -20f);
            quitRt.sizeDelta        = new Vector2(56f, 56f);
            var quitImg = quitGO.GetComponent<Image>();
            if (quitImg) quitImg.color = new Color(0.85f, 0.25f, 0.30f, 0.95f);
            var quitLbl = quitGO.GetComponentInChildren<Text>();
            if (quitLbl) { quitLbl.text = "X"; quitLbl.fontSize = 28; quitLbl.color = Color.white; quitLbl.fontStyle = FontStyle.Bold; }
            quitGO.GetComponent<Button>().onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            // EventSystem (needed for UI input)
            if (!FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>())
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Wire MoonlightUI
            var ui = canvasGO.AddComponent<MoonlightUI>();
            ui.Wire(
                sliders[0], sliders[1], sliders[2], sliders[3], sliders[4],
                stageLabel, coinsLabel, xpLabel, moodLabel, daysLabel,
                feedBtn, cuddleBtn, sleepBtn,
                stgPanel,  stgLabel,
                roomPanel, roomLabel,
                offlinePanel, sleepOvr,
                feedMenu, contentGO.transform);

            // Extra UI components
            canvasGO.AddComponent<DayNightCycleUI>();
            canvasGO.AddComponent<StreakToast>();
            canvasGO.AddComponent<AchievementToast>();

            return (canvasGO, ui);
        }

        // ── UI Helpers ───────────────────────────────────────────────────────
        static GameObject Panel(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax,
            Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return go;
        }

        static Text MakeLegacyLabel(string name, Transform parent,
            Vector2 anchoredPos, Vector2 size, string text,
            int fontSize, Color color, FontStyle style)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text      = text;
            t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize  = fontSize;
            t.fontStyle = style;
            t.color     = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 1f);
            rt.anchorMax        = new Vector2(0.5f, 1f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;
            return t;
        }

        static TMP_Text MakeTMPLabelAnchored(string name, Transform parent,
            Vector2 anchoredPos, Vector2 size, string text,
            float fontSize, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text      = text;
            t.fontSize  = fontSize;
            t.fontStyle = FontStyles.Bold;
            t.color     = color;
            t.alignment = TextAlignmentOptions.Center;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 1f);
            rt.anchorMax        = new Vector2(0.5f, 1f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;
            return t;
        }

        static TMP_Text MakeTMPLabel(string name, Transform parent,
            Vector2 anchoredPos, Vector2 size, string text,
            float fontSize, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text      = text;
            t.fontSize  = fontSize;
            t.color     = color;
            t.alignment = TextAlignmentOptions.Center;
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;
            return t;
        }

        static void AttachBurst(Button btn, Color color, int count)
        {
            var burst = btn.gameObject.AddComponent<ButtonHeartBurst>();
            btn.onClick.AddListener(() =>
            {
                var target = GameObject.Find("Moonlight");
                if (target != null)
                {
                    burst.Configure(target.transform, color, count);
                    // Punch the visual child (not the root, since bobber overrides root rotation)
                    var visual = target.transform.Find("Visual");
                    if (visual != null)
                    {
                        var p = visual.GetComponent<ScalePuncher>();
                        if (p != null) p.Punch(0.22f, 0.40f);
                        var smile = visual.Find("Smile");
                        if (smile != null)
                        {
                            var sp = smile.GetComponent<ScalePuncher>();
                            if (sp == null) sp = smile.gameObject.AddComponent<ScalePuncher>();
                            sp.Punch(0.55f, 0.55f);
                        }
                    }
                }
            });
            // Configure once so first click works too
            var initial = GameObject.Find("Moonlight");
            if (initial != null) burst.Configure(initial.transform, color, count);
        }

        static Button MakeButton(string name, Transform parent, Vector2 pos, string label, Color tint)
        {
            var res   = new DefaultControls.Resources();
            var btnGO = DefaultControls.CreateButton(res);
            btnGO.name = name;
            btnGO.transform.SetParent(parent, false);
            var rt = btnGO.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta        = new Vector2(200f, 70f);
            var img = btnGO.GetComponent<Image>();
            if (img) img.color = tint;
            var lbl = btnGO.GetComponentInChildren<Text>();
            if (lbl) { lbl.text = label; lbl.fontSize = 24; lbl.fontStyle = FontStyle.Bold; lbl.color = Color.white; }
            return btnGO.GetComponent<Button>();
        }

        // ── Primitive Helper ─────────────────────────────────────────────────
        static GameObject Prim(PrimitiveType type, string name, Transform parent,
            Vector3 localPos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale    = scale;
            SetToonMat(go, color);
            return go;
        }

        static void ApplyTiledTexture(GameObject go, Texture2D tex, Vector2 tiling)
        {
            var mr = go.GetComponent<MeshRenderer>();
            if (mr == null || tex == null) return;
            var mat = mr.material;
            if (mat.HasProperty("_MainTex"))
            {
                mat.mainTexture      = tex;
                mat.mainTextureScale = tiling;
            }
            if (mat.HasProperty("_BaseMap"))
                mat.SetTexture("_BaseMap", tex);
        }

        static Shader _toonShader;
        static Shader ToonShader =>
            _toonShader ??= Shader.Find("MoonlightHouse/Toon") ?? Shader.Find("Standard");

        static void SetMat(GameObject go, Color color, Color emissive = default)
        {
            var mr = go.GetComponent<MeshRenderer>();
            if (!mr) return;
            var shader = Shader.Find("Standard") ?? Shader.Find("Diffuse");
            var mat = new Material(shader);
            mat.color = color;
            if (emissive != default && emissive != Color.black)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissive);
            }
            mr.material = mat;
        }

        static void SetToonMat(GameObject go, Color color)
        {
            var mr = go.GetComponent<MeshRenderer>();
            if (!mr) return;
            var mat = new Material(ToonShader);
            mat.SetColor("_Color", color);
            mat.SetColor("_ShadowColor", new Color(color.r * 0.50f, color.g * 0.40f, color.b * 0.65f, 1f));
            mat.SetFloat("_ShadowThreshold", 0.32f);
            mat.SetFloat("_OutlineWidth",    0.009f);
            mat.SetColor("_OutlineColor",    new Color(0.06f, 0.03f, 0.12f, 1f));
            mat.SetColor("_EmissionColor",   Color.black);
            mat.SetFloat("_EmissionIntensity", 0f);
            mr.material = mat;
        }

        static void ApplyToonToAll(GameObject root, Color tint)
        {
            foreach (var mr in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mat = new Material(ToonShader);
                // Blend existing albedo with tint
                var origColor = mr.material != null ? mr.material.color : Color.white;
                var blended   = Color.Lerp(origColor, tint, 0.4f);
                mat.SetColor("_Color", blended);
                mat.SetColor("_ShadowColor", new Color(blended.r * 0.5f, blended.g * 0.4f, blended.b * 0.65f));
                mat.SetFloat("_ShadowThreshold", 0.35f);
                mat.SetFloat("_OutlineWidth", 0.006f);
                mat.SetColor("_OutlineColor", new Color(0.06f, 0.03f, 0.12f));
                mr.material = mat;
            }
        }
    }
}
