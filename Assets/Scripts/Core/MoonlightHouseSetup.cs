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
        public const int HeroMaterialBudget = 8;
        public const float HeroEyeIdleEmission = 0.18f;
        public const float HeroEyeActionEmission = 0.28f;
        public const int RoomSurfaceProfileMinimum = 8;
        public const int RoomAuthoredMaterialMinimum = 8;
        public const int RoomAuthoredMaterialMaximum = 16;

        sealed class RoomSurfaceProfile
        {
            public readonly string Id;
            public readonly float Smoothness;
            public readonly float Metallic;
            public readonly float Emission;

            public RoomSurfaceProfile(string id, float smoothness, float metallic, float emission)
            {
                Id = id;
                Smoothness = smoothness;
                Metallic = metallic;
                Emission = emission;
            }
        }

        // One bounded profile per semantic surface family in the authored room.
        // These are data only: material allocation remains one clone per source material.
        static readonly RoomSurfaceProfile[] RoomSurfaceProfiles =
        {
            new RoomSurfaceProfile("plaster",        0.18f, 0.00f, 0.00f),
            new RoomSurfaceProfile("painted-trim",   0.32f, 0.00f, 0.00f),
            new RoomSurfaceProfile("wood-floor",     0.28f, 0.00f, 0.00f),
            new RoomSurfaceProfile("wood-furniture", 0.24f, 0.00f, 0.00f),
            new RoomSurfaceProfile("textile-bed",    0.14f, 0.00f, 0.00f),
            new RoomSurfaceProfile("foliage",        0.18f, 0.00f, 0.00f),
            new RoomSurfaceProfile("glass-sky",      0.72f, 0.00f, 0.00f),
            new RoomSurfaceProfile("moon-glow",      0.58f, 0.00f, 0.18f)
        };

        static readonly string[] RequiredRoomSemanticMaterials =
        {
            "Room_Cream", "Trim_Ivory", "Floor_Honey", "Wood_Maple",
            "Blanket_Blush", "Mint_Deep", "Sky_Dusk", "Moon_Butter"
        };

        static readonly string[] RequiredRoomSurfaceProfiles =
        {
            "plaster", "painted-trim", "wood-floor", "wood-furniture",
            "textile-bed", "foliage", "glass-sky", "moon-glow"
        };

        // The authored Blender house is the playable default. The image-plate
        // presentation remains available for dedicated visual comparisons.
        static readonly bool PhotorealMode = System.Array.Exists(
            System.Environment.GetCommandLineArgs(),
            argument => string.Equals(argument, "-moonlightPhotoreal", System.StringComparison.OrdinalIgnoreCase));

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
            if (PhotorealMode)
                new GameObject("MoonlightSceneDirector").AddComponent<MoonlightSceneDirector>();
            rm.BindPlayer(mlGO.GetComponent<MoonlightPlayerController>());

            var (uiGO, ui) = CreateUI();
            var roomNavigation = CreateRoomNavigation(uiGO.transform, rm);
            ui.WireRoomNavigation(roomNavigation);

            // GameManager ties everything together
            var gmGO = new GameObject("MoonlightGameManager");
            var gm   = gmGO.AddComponent<MoonlightGameManager>();
            gmGO.AddComponent<MoonlightVisualQA>();
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
            cam.clearFlags      = PhotorealMode ? CameraClearFlags.SolidColor : CameraClearFlags.Skybox;
            cam.backgroundColor = PhotorealMode
                ? new Color(0.12f, 0.055f, 0.035f)
                : new Color(0.91f, 0.84f, 0.79f);
            if (!PhotorealMode && !gameObject.GetComponent<SkyboxSetup>())
                gameObject.AddComponent<SkyboxSetup>();
            cam.farClipPlane    = PhotorealMode ? 40f : 80f;
            cam.fieldOfView     = PhotorealMode ? 39f : 40.5f;
            cam.transform.position = PhotorealMode
                ? new Vector3(-0.05f, 1.26f, -4.85f)
                : new Vector3(4.45f, 3.10f, -5.35f);
            cam.transform.LookAt(PhotorealMode
                ? new Vector3(0.52f, 0.78f, -0.44f)
                : new Vector3(0.22f, 1.38f, 0.45f));
            cam.allowHDR = true;
            cam.useOcclusionCulling = true;
            cam.depthTextureMode |= DepthTextureMode.DepthNormals;

            if (!PhotorealMode && !cam.GetComponent<CameraController>())
                cam.gameObject.AddComponent<CameraController>();
            if (!PhotorealMode && !cam.GetComponent<AmbientCycle>())
                cam.gameObject.AddComponent<AmbientCycle>();
            if (!cam.GetComponent<GodRaysPostFx>())
                cam.gameObject.AddComponent<GodRaysPostFx>();
            var bloom = cam.GetComponent<BloomPostFx>();
            if (!bloom) bloom = cam.gameObject.AddComponent<BloomPostFx>();
            if (PhotorealMode)
                bloom.Configure(0.58f, 0.78f, 0.24f, new Color(1.08f, 1.00f, 0.94f), 6);
            if (!PhotorealMode && !cam.GetComponent<CelOutlinePostFx>())
                cam.gameObject.AddComponent<CelOutlinePostFx>();
        }

        // ── Atmosphere ───────────────────────────────────────────────────────
        void SetupAtmosphere()
        {
            if (PhotorealMode)
            {
                RenderSettings.ambientMode  = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.68f, 0.56f, 0.47f);
                RenderSettings.fog          = false;
                RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
                RenderSettings.reflectionIntensity   = 0.42f;

                MakeLight("WindowSunWarm", LightType.Directional,
                    new Color(1.00f, 0.78f, 0.58f), 1.18f,
                    Quaternion.Euler(34f, -132f, 0f), LightShadows.Soft, 0.40f);

                MakeLight("SoftRoomFill", LightType.Directional,
                    new Color(1.00f, 0.90f, 0.78f), 0.44f,
                    Quaternion.Euler(18f, 42f, 0f), LightShadows.None, 0f);

                MakeLight("StorybookRim", LightType.Directional,
                    new Color(1.00f, 0.58f, 0.80f), 0.18f,
                    Quaternion.Euler(20f, 168f, 0f), LightShadows.None, 0f);

                CreatePhotorealDustMotes();
                return;
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.58f, 0.53f, 0.52f);
            RenderSettings.ambientEquatorColor = new Color(0.47f, 0.40f, 0.38f);
            RenderSettings.ambientGroundColor = new Color(0.24f, 0.19f, 0.18f);
            RenderSettings.ambientIntensity = 0.70f;
            RenderSettings.reflectionIntensity = 0.55f;
            RenderSettings.fog = false;

            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowDistance = 24f;
            QualitySettings.shadowCascades = 2;
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.shadowProjection = ShadowProjection.StableFit;

            // A warm studio key supplies the soft object modeling visible in the
            // approved Blender frame. The cool fill has no shadows, keeping the
            // iPad path to one realtime shadow caster.
            MakeLight("WindowKey", LightType.Directional,
                new Color(1.00f, 0.94f, 0.86f), 0.76f,
                Quaternion.Euler(38f, -34f, 0f), LightShadows.Soft, 0.48f);
            MakeLight("RoomFill", LightType.Directional,
                new Color(0.76f, 0.84f, 1.00f), 0.12f,
                Quaternion.Euler(24f, 148f, 0f), LightShadows.None, 0f);

            // Floating dust motes — ambient indoor atmosphere
            var dustGO = new GameObject("DustMotes");
            dustGO.transform.position = new Vector3(0f, 1.8f, 0f);
            var dps = dustGO.AddComponent<ParticleSystem>();
            var dmain = dps.main;
            dmain.startLifetime = 8f;
            dmain.startSpeed    = 0.06f;
            dmain.startSize     = new ParticleSystem.MinMaxCurve(0.02f, 0.045f);
            dmain.startColor    = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.95f, 0.85f, 0.35f), new Color(0.85f, 0.75f, 1f, 0.35f));
            dmain.maxParticles  = 12;
            dmain.gravityModifier = -0.02f;
            var demis = dps.emission;
            demis.rateOverTime = 1f;
            var dshape = dps.shape;
            dshape.shapeType = ParticleSystemShapeType.Box;
            dshape.scale     = new Vector3(8f, 3f, 8f);
            var dvel = dps.velocityOverLifetime;
            dvel.enabled = true;
            dvel.x = new ParticleSystem.MinMaxCurve(-0.12f, 0.12f);
            dvel.y = new ParticleSystem.MinMaxCurve(-0.015f, 0.035f);
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

        void CreatePhotorealDustMotes()
        {
            var dustGO = new GameObject("PhotorealWarmDust");
            dustGO.transform.position = new Vector3(0f, 1.7f, -0.1f);
            var dps = dustGO.AddComponent<ParticleSystem>();
            var main = dps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 7f);
            main.startSpeed    = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
            main.startSize     = new ParticleSystem.MinMaxCurve(0.018f, 0.055f);
            main.startColor    = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.84f, 0.58f, 0.20f), new Color(1f, 0.66f, 0.86f, 0.16f));
            main.maxParticles  = 90;
            main.gravityModifier = -0.01f;
            var emission = dps.emission;
            emission.rateOverTime = 10f;
            var shape = dps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(4.8f, 2.2f, 1.1f);
            var renderer = dustGO.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.mainTexture = MakeSoftCircleTex(48);
            renderer.material = mat;
        }

        static Texture2D _softCircleTex;
        static Sprite _uiCircleSprite;
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

        static Sprite UICircleSprite
        {
            get
            {
                if (_uiCircleSprite != null) return _uiCircleSprite;
                const int size = 96;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                float center = (size - 1) * 0.5f;
                float radius = center - 1f;
                var pixels = new Color32[size * size];
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt((radius - distance + 1f) * 255f), 0, 255);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
                tex.SetPixels32(pixels);
                tex.Apply(false, true);
                _uiCircleSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), tex.width);
                return _uiCircleSprite;
            }
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
            mlGO.transform.position = PhotorealMode
                ? new Vector3(0.52f, 0.02f, -0.92f)
                : new Vector3(0.65f, 0f, -0.15f);

            mlGO.AddComponent<MoonlightCharacter>();
            if (!PhotorealMode)
                mlGO.AddComponent<MoonlightIdleBehavior>();
            mlGO.AddComponent<MoonlightWardrobe>();
            mlGO.AddComponent<MoonlightStageVFX>();
            if (!PhotorealMode)
                mlGO.AddComponent<MoonlightGlowController>();
            mlGO.AddComponent<MoonlightMoodParticles>();
            if (!PhotorealMode)
                mlGO.AddComponent<MoonlightAnimator>();
            var controller = mlGO.AddComponent<MoonlightPlayerController>();
            controller.ConfigureBounds(new Rect(-4.25f, -3.25f, 8.5f, 6.5f));
            mlGO.AddComponent<MoonlightSpatialInteractor>();

            GameObject visual;
            if (PhotorealMode)
            {
                visual = SpawnPhotorealMoonlightChild(mlGO.transform);
                if (visual != null) visual.AddComponent<MoonlightKidAnimator>();
            }
            else
            {
                var heroPrefab = Resources.Load<GameObject>("Models/Hero/MoonbudCat");
                visual = heroPrefab != null
                    ? Object.Instantiate(heroPrefab, mlGO.transform)
                    : BuildFallbackCharacter(mlGO.transform);
                if (visual != null)
                {
                    visual.name = "Visual";
                    visual.transform.localPosition = Vector3.zero;
                    visual.transform.localRotation = Quaternion.Euler(-90f, 180f, 0f);
                    visual.transform.localScale = Vector3.one;
                    if (heroPrefab != null) ApplyHeroMaterials(visual);
                }
            }
            if (visual == null) visual = BuildFallbackCharacter(mlGO.transform);
            if (PhotorealMode)
                visual.transform.localRotation = Quaternion.Euler(0f, 180f, 0.6f);
            // Activate SSS + stronger wrap on skin parts (face, neck, ears, hands, forearms) — human warmth
            string[] skinParts = { "Head", "Neck", "EarL", "EarR", "HandL", "HandR", "ForearmL", "ForearmR", "Nose", "CheekL", "CheekR" };
            foreach (var pn in skinParts)
            {
                var t = visual.transform.Find(pn);
                if (t == null) continue;
                var mr = t.GetComponent<MeshRenderer>();
                if (mr == null) continue;
                var mat = mr.material;
                if (mat.HasProperty("_SSSStrength")) mat.SetFloat("_SSSStrength", 0.45f);
                if (mat.HasProperty("_Wrap"))        mat.SetFloat("_Wrap", 0.65f);
                if (mat.HasProperty("_SSSColor"))    mat.SetColor("_SSSColor", new Color(1.00f, 0.45f, 0.38f));
            }
            // Scale-punch on button presses
            visual.AddComponent<ScalePuncher>();
            if (!PhotorealMode)
            {
                // Gentle idle micro-motion (head tilt, arm sway, body squash)
                visual.AddComponent<IdleMicroMotion>();
                visual.AddComponent<MoonlightBobber>();
            }

            // Floating name tag above the character (3D TextMesh — billboarded)
            var tagGO = new GameObject("NameTag");
            tagGO.transform.SetParent(mlGO.transform, false);
            tagGO.transform.localPosition = PhotorealMode ? new Vector3(0f, 1.32f, 0f) : new Vector3(0f, 2.5f, 0f);
            tagGO.transform.localScale    = Vector3.one * (PhotorealMode ? 0.10f : 0.25f);
            tagGO.AddComponent<BillboardToCamera>();
            var tm = tagGO.AddComponent<TextMesh>();
            tm.text       = "Moonlight";
            tm.fontSize   = PhotorealMode ? 42 : 48;
            tm.fontStyle  = FontStyle.Bold;
            tm.anchor     = TextAnchor.MiddleCenter;
            tm.alignment  = TextAlignment.Center;
            tm.color      = PhotorealMode ? new Color(1f, 0.96f, 0.86f) : new Color(1f, 0.9f, 1f);
            tm.characterSize = 0.1f;
            var tmr = tagGO.GetComponent<MeshRenderer>();
            tmr.sortingOrder = 5;
            tagGO.SetActive(false);

            // Collider
            var col = mlGO.AddComponent<CapsuleCollider>();
            col.height = PhotorealMode ? 1.22f : 2.2f;
            col.radius = PhotorealMode ? 0.22f : 0.38f;
            col.center = PhotorealMode ? new Vector3(0f, 0.62f, 0f) : new Vector3(0f, 1.1f, 0f);
            mlGO.AddComponent<MoonlightInteractable>();

            // Sparkle particle system (ambient magic around character)
            var sparkGO = new GameObject("Sparkles");
            sparkGO.transform.SetParent(mlGO.transform, false);
            sparkGO.transform.localPosition = PhotorealMode ? new Vector3(0f, 0.68f, 0f) : new Vector3(0f, 1.2f, 0f);
            var ps = sparkGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = PhotorealMode ? 1.8f : 2.2f;
            main.startSpeed    = PhotorealMode ? 0.18f : 0.12f;
            main.startSize     = 0.035f;
            main.startColor    = new ParticleSystem.MinMaxGradient(
                PhotorealMode ? new Color(1f, 0.78f, 0.42f, 0.70f) : new Color(1f, 0.90f, 0.55f),
                PhotorealMode ? new Color(1f, 0.56f, 0.78f, 0.55f) : new Color(0.75f, 0.55f, 1f));
            main.maxParticles  = PhotorealMode ? 24 : 12;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            var emission = ps.emission;
            emission.rateOverTime = PhotorealMode ? 4f : 1.5f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius    = PhotorealMode ? 0.38f : 0.9f;
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

            if (PhotorealMode)
            {
                AddPhotorealContactShadow(mlGO.transform);
                AddPhotorealCompanionLight(mlGO.transform);
                return mlGO;
            }

            // Ground halo — pulsing disc beneath the character
            var haloGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            haloGO.name = "GroundHalo";
            Object.Destroy(haloGO.GetComponent<Collider>());
            haloGO.transform.SetParent(mlGO.transform, false);
            haloGO.transform.localPosition = new Vector3(0f, 0.005f, 0f);
            haloGO.transform.localScale    = new Vector3(0.85f, 0.006f, 0.85f);
            var haloMat = new Material(ToonShader);
            haloMat.SetColor("_Color",            new Color(0.78f, 0.72f, 0.86f, 0.22f));
            haloMat.SetColor("_EmissionColor",    new Color(0.22f, 0.18f, 0.28f));
            haloMat.SetFloat("_EmissionIntensity", 0.08f);
            haloMat.SetFloat("_OutlineWidth",      0f);
            haloGO.GetComponent<MeshRenderer>().material = haloMat;
            haloGO.AddComponent<GroundHalo>();
            haloGO.SetActive(false);

            // Orbiting fireflies around the character
            Color[] fireCols = {
                new Color(1.00f, 0.86f, 0.58f),
                new Color(0.62f, 0.82f, 0.76f),
            };
            for (int f = 0; f < fireCols.Length; f++)
            {
                var fly = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                fly.name = $"Firefly{f}";
                fly.transform.SetParent(mlGO.transform, false);
                fly.transform.localScale = Vector3.one * Random.Range(0.025f, 0.045f);
                Object.Destroy(fly.GetComponent<Collider>());
                var fMat = new Material(ToonShader);
                fMat.SetColor("_Color",             fireCols[f]);
                fMat.SetColor("_EmissionColor",     fireCols[f]);
                fMat.SetFloat("_EmissionIntensity", 0.25f);
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
            glow.color     = new Color(1.00f, 0.78f, 0.62f);
            glow.intensity = 0.22f;
            glow.range     = 3.5f;

            return mlGO;
        }

        static GameObject SpawnPhotorealMoonlightChild(Transform parent)
        {
            var prefab = Resources.Load<GameObject>("UnityChan/SD_unitychan_humanoid");
            if (prefab == null)
            {
                Debug.LogWarning("[Photoreal] Missing Resources/UnityChan/SD_unitychan_humanoid, falling back to Sophie");
                return SpawnPhotorealSophie(parent);
            }

            var instance = Object.Instantiate(prefab, parent);
            instance.name = "Visual";

            foreach (var animator in instance.GetComponentsInChildren<Animator>(true))
                Object.Destroy(animator);
            foreach (var animation in instance.GetComponentsInChildren<Animation>(true))
                Object.Destroy(animation);

            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Object.Destroy(instance);
                return SpawnPhotorealSophie(parent);
            }

            Bounds b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            float height = Mathf.Max(0.1f, b.size.y);
            const float targetChildHeight = 1.10f;
            const float childHeightStretch = 1.08f;
            float childScale = targetChildHeight / (height * childHeightStretch);
            instance.transform.localScale = new Vector3(childScale * 0.92f, childScale * childHeightStretch, childScale * 0.92f);
            instance.transform.localRotation = Quaternion.identity;

            b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            instance.transform.localPosition = new Vector3(0f, -b.min.y, 0f);

            var standard = Shader.Find("Standard");
            var atlas = Resources.Load<Texture>("UnityChan/Textures/utc_all2");
            var normal = Resources.Load<Texture>("UnityChan/Textures/utc_nomal");
            foreach (var r in renderers)
            {
                r.shadowCastingMode = ShadowCastingMode.On;
                r.receiveShadows = true;

                var srcMats = r.sharedMaterials;
                var mats = new Material[srcMats.Length];
                for (int i = 0; i < srcMats.Length; i++)
                {
                    var src = srcMats[i];
                    var mat = standard != null ? new Material(standard) : new Material(Shader.Find("Diffuse"));
                    Texture main = atlas;
                    string matName = src != null ? src.name.ToLowerInvariant() : string.Empty;

                    if (src != null)
                    {
                        if (src.HasProperty("_MainTex") && src.GetTexture("_MainTex") != null)
                            main = src.GetTexture("_MainTex");
                        else if (src.mainTexture != null)
                            main = src.mainTexture;
                    }

                    mat.name = $"Moonlight_{(string.IsNullOrEmpty(matName) ? "standard" : matName)}";
                    mat.mainTexture = main;
                    if (normal != null && mat.HasProperty("_BumpMap") && !matName.Contains("mouth"))
                    {
                        mat.SetTexture("_BumpMap", normal);
                        mat.EnableKeyword("_NORMALMAP");
                        if (mat.HasProperty("_BumpScale")) mat.SetFloat("_BumpScale", 0.34f);
                    }

                    var tint = Color.white;
                    if (matName.Contains("skin")) tint = new Color(1.00f, 0.82f, 0.70f, 1f);
                    else if (matName.Contains("hair")) tint = new Color(0.82f, 0.70f, 0.60f, 1f);
                    else if (matName.Contains("mouth")) tint = new Color(1.00f, 0.78f, 0.82f, 1f);
                    else tint = new Color(0.92f, 0.86f, 0.82f, 1f);

                    mat.color = tint;
                    if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", matName.Contains("skin") ? 0.32f : 0.20f);
                    if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
                    if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", Color.black);
                    mat.DisableKeyword("_EMISSION");
                    mats[i] = mat;
                }
                r.sharedMaterials = mats;
            }

            Debug.Log("[Photoreal] Using UnityChan SD child-proportioned Moonlight avatar");
            return instance;
        }

        static GameObject SpawnPhotorealSophie(Transform parent)
        {
            var prefab = Resources.Load<GameObject>("Mixamo/SophieHappyIdle");
            if (prefab == null) { Debug.LogWarning("[Photoreal] Missing Resources/Mixamo/SophieHappyIdle"); return null; }

            var instance = Object.Instantiate(prefab, parent);
            instance.name = "Visual";

            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) { Object.Destroy(instance); return null; }

            Bounds b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            float height = Mathf.Max(0.1f, b.size.y);
            instance.transform.localScale = Vector3.one * (1.10f / height);
            instance.transform.localRotation = Quaternion.identity;

            b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            instance.transform.localPosition = new Vector3(0f, -b.min.y, 0f);

            var standard = Shader.Find("Standard");
            foreach (var r in renderers)
            {
                r.shadowCastingMode = ShadowCastingMode.On;
                r.receiveShadows = true;
                var srcMats = r.sharedMaterials;
                var mats = new Material[srcMats.Length];
                for (int i = 0; i < srcMats.Length; i++)
                {
                    var src = srcMats[i];
                    var mat = new Material(standard);
                    Texture tex = null;
                    Color color = Color.white;
                    if (src != null)
                    {
                        if (src.HasProperty("_MainTex")) tex = src.GetTexture("_MainTex");
                        if (tex == null && src.mainTexture != null) tex = src.mainTexture;
                        if (src.HasProperty("_Color")) color = src.color;
                    }
                    mat.color = Color.Lerp(color, new Color(0.76f, 0.70f, 0.64f, 1f), 0.18f);
                    if (tex != null)
                    {
                        mat.SetTexture("_MainTex", tex);
                        mat.mainTexture = tex;
                    }
                    mat.SetFloat("_Metallic", 0f);
                    mat.SetFloat("_Glossiness", 0.34f);
                    mats[i] = mat;
                }
                r.sharedMaterials = mats;
            }

            var clips = Resources.LoadAll<AnimationClip>("Mixamo/SophieHappyIdle");
            if (clips != null && clips.Length > 0)
            {
                foreach (var existing in instance.GetComponentsInChildren<Animator>(true)) Object.Destroy(existing);
                var anim = instance.GetComponent<Animation>() ?? instance.AddComponent<Animation>();
                var clip = clips[0];
                clip.legacy = true;
                clip.wrapMode = WrapMode.Loop;
                anim.AddClip(clip, clip.name);
                anim.clip = clip;
                anim.playAutomatically = true;
                anim.Play(clip.name);
                Debug.Log($"[Photoreal] Playing Sophie clip '{clip.name}' ({clip.length:F2}s)");
            }

            return instance;
        }

        static void AddPhotorealContactShadow(Transform parent)
        {
            var shadow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            shadow.name = "SoftContactShadow";
            shadow.transform.SetParent(parent, false);
            shadow.transform.localPosition = new Vector3(0.02f, 0.015f, 0.02f);
            shadow.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            shadow.transform.localScale = new Vector3(0.72f, 0.26f, 1f);
            Object.Destroy(shadow.GetComponent<Collider>());
            var mat = new Material(TransparentSpriteShader);
            mat.mainTexture = MakeSoftCircleTex(96);
            mat.color = new Color(0.18f, 0.10f, 0.08f, 0.28f);
            shadow.GetComponent<MeshRenderer>().material = mat;
        }

        static void AddPhotorealCompanionLight(Transform parent)
        {
            var lgo = new GameObject("MoonlightWarmFaceLight");
            lgo.transform.SetParent(parent, false);
            lgo.transform.localPosition = new Vector3(-0.28f, 0.82f, -0.42f);
            var l = lgo.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1.0f, 0.70f, 0.52f);
            l.intensity = 0.38f;
            l.range = 1.45f;
            l.shadows = LightShadows.None;
        }

        // Mixamo character — FBX with baked face textures (Adobe license permits commercial game embedding).
        // We keep each imported material's own _MainTex and only overlay ToonShader params for cel look + outline.
        static GameObject SpawnMixamoCharacter(Transform parent, string resourcePath, string skinPath = null)
        {
            var prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null) { Debug.LogWarning($"[Mixamo] Missing Resources/{resourcePath}"); return null; }

            var instance = Object.Instantiate(prefab, parent);
            instance.name = "Visual";

            // Kill Mixamo face-anim overlays — they use a sprite-atlas sampled across the whole UV
            // so without an animator driving frames they render all expressions at once = horror face.
            foreach (var r in instance.GetComponentsInChildren<Renderer>(true))
            {
                string n = r.gameObject.name;
                if (n.Contains("AnimGeo") || n.Contains("Brows") || n.Contains("Eyes") || n.Contains("Mouth"))
                    r.gameObject.SetActive(false);
            }

            var rends = instance.GetComponentsInChildren<Renderer>(false);
            if (rends.Length == 0) { Object.Destroy(instance); return null; }

            Bounds b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);
            float height = Mathf.Max(0.1f, b.size.y);
            instance.transform.localScale    = Vector3.one * (1.8f / height);
            instance.transform.localRotation = Quaternion.identity;

            b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);
            instance.transform.localPosition = new Vector3(0f, -b.min.y, 0f);

            // Use Unity's Standard shader — ToonShader's outline+rim make realistic human faces look zombie-like.
            var standard = Shader.Find("Standard");
            Texture2D skinOverride = !string.IsNullOrEmpty(skinPath) ? Resources.Load<Texture2D>(skinPath) : null;
            if (skinOverride != null) { skinOverride.filterMode = FilterMode.Point; }
            foreach (var r in rends)
            {
                var sharedMats = r.sharedMaterials;
                var newMats = new Material[sharedMats.Length];
                for (int i = 0; i < sharedMats.Length; i++)
                {
                    var src = sharedMats[i];
                    var mat = new Material(standard);
                    Texture srcTex = null;
                    if (src != null)
                    {
                        if (src.HasProperty("_MainTex")) srcTex = src.GetTexture("_MainTex");
                        if (srcTex == null && src.HasProperty("_BaseMap"))    srcTex = src.GetTexture("_BaseMap");
                        if (srcTex == null && src.HasProperty("_BaseColorMap")) srcTex = src.GetTexture("_BaseColorMap");
                        if (srcTex == null && src.mainTexture != null)        srcTex = src.mainTexture;
                    }
                    if (skinOverride != null) srcTex = skinOverride;
                    if (srcTex != null) { mat.SetTexture("_MainTex", srcTex); mat.mainTexture = srcTex; }
                    mat.SetFloat("_Glossiness", 0.15f);
                    mat.SetFloat("_Metallic",   0f);
                    Debug.Log($"[Mixamo] slot={r.gameObject.name}:{i} src={src?.name} tex={srcTex?.name ?? "<none>"}");
                    newMats[i] = mat;
                }
                r.sharedMaterials = newMats;
            }

            // Play the embedded Mixamo idle clip (imported as Legacy — see BuildAll.ExtractMixamoTextures).
            var clips = Resources.LoadAll<AnimationClip>(resourcePath);
            if (clips != null && clips.Length > 0)
            {
                foreach (var existing in instance.GetComponentsInChildren<Animator>(true)) Object.Destroy(existing);
                var anim = instance.GetComponent<Animation>() ?? instance.AddComponent<Animation>();
                var clip = clips[0];
                clip.legacy = true;
                clip.wrapMode = WrapMode.Loop;
                anim.AddClip(clip, clip.name);
                anim.clip = clip;
                anim.playAutomatically = true;
                anim.Play(clip.name);
                Debug.Log($"[Mixamo] Playing legacy clip '{clip.name}' ({clip.length:F2}s)");
            }
            else
            {
                Debug.LogWarning($"[Mixamo] No AnimationClips found for {resourcePath}");
            }

            return instance;
        }

        // SD Unity-Chan — © Unity Technologies Japan / UCL. Commercial OK w/ attribution.
        // The FBX preserves baked face textures so Unity's default importer gives a proper anime face.
        static GameObject SpawnUnityChanSD(Transform parent)
        {
            var prefab = Resources.Load<GameObject>("UnityChan/SD_unitychan_humanoid");
            if (prefab == null) { Debug.LogWarning("[SDUnityChan] Missing Resources/UnityChan/SD_unitychan_humanoid"); return null; }

            var instance = Object.Instantiate(prefab, parent);
            instance.name = "Visual";

            // Auto-scale so the character is ~1.4m tall (SD = "super-deformed" chibi proportions; kept a touch tall for cozy read)
            var rends = instance.GetComponentsInChildren<Renderer>(true);
            if (rends.Length == 0) { Object.Destroy(instance); return null; }
            Bounds b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);
            float height = Mathf.Max(0.1f, b.size.y);
            float targetHeight = 1.55f;
            instance.transform.localScale    = Vector3.one * (targetHeight / height);
            instance.transform.localRotation = Quaternion.identity;

            // Recompute bounds after scaling, then foot-offset to stand on the pedestal.
            b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);
            instance.transform.localPosition = new Vector3(0f, -b.min.y, 0f);

            // Load the SD Unity-Chan albedo atlas directly — the .meta stripping above disconnects
            // imported-material texture references, so we re-bind the single shared atlas by name.
            var atlas = Resources.Load<Texture2D>("UnityChan/Textures/utc_all2");
            // The "mouth" material uses a separate atlas region on the same texture, the default FBX slot
            // for eyes/mouth still points at utc_all2 via UV island — so assigning utc_all2 to every slot
            // gives the correct face read for free.
            foreach (var r in rends)
            {
                var sharedMats = r.sharedMaterials;
                var newMats = new Material[sharedMats.Length];
                for (int i = 0; i < sharedMats.Length; i++)
                {
                    var mat = new Material(ToonShader);
                    if (atlas != null) mat.SetTexture("_MainTex", atlas);
                    mat.SetColor("_Color",           Color.white);
                    mat.SetColor("_ShadowColor",     new Color(0.86f, 0.72f, 0.88f));
                    mat.SetColor("_MidShadow",       new Color(0.93f, 0.85f, 0.94f));
                    mat.SetFloat("_ShadowThreshold", 0.38f);
                    mat.SetFloat("_OutlineWidth",    0.006f);
                    mat.SetColor("_OutlineColor",    new Color(0.08f, 0.04f, 0.14f));
                    mat.SetFloat("_Wrap",            0.55f);
                    mat.SetFloat("_SSSStrength",     0.25f);
                    mat.SetColor("_SSSColor",        new Color(1.00f, 0.55f, 0.48f));
                    mat.SetColor("_RimColor",        new Color(1.00f, 0.80f, 0.95f));
                    mat.SetFloat("_RimPower",        3.0f);
                    newMats[i] = mat;
                }
                r.sharedMaterials = newMats;
            }

            return instance;
        }

        // OGA CC-BY 3.0 — Sara by Mandi Paugh, opengameart.org/content/sara-3d-model (unused path; kept for reference)
        static GameObject SpawnOGASara(Transform parent)
        {
            var prefab = Resources.Load<GameObject>("Models/OGA/Sara");
            if (prefab == null) { Debug.LogWarning("[Sara] Missing Resources/Models/OGA/Sara"); return null; }

            var instance = Object.Instantiate(prefab, parent);
            instance.name = "Visual";
            // Compute world bbox to compute feet offset + auto-scale.
            var rends = instance.GetComponentsInChildren<Renderer>(true);
            if (rends.Length == 0) { Object.Destroy(instance); return null; }
            Bounds b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);
            float height = Mathf.Max(0.1f, b.size.y);
            float targetHeight = 1.85f;
            float scale = targetHeight / height;
            instance.transform.localScale    = Vector3.one * scale;
            instance.transform.localRotation = Quaternion.identity;

            // Recompute bounds after scaling to find foot offset
            b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);
            float footOffset = -b.min.y;
            instance.transform.localPosition = new Vector3(0f, footOffset, 0f);

            // Retint with toon shader — palette: warm skin, Ghibli purple dress, dark hair
            Color skin  = new Color(1.00f, 0.86f, 0.76f);
            Color hair  = new Color(0.22f, 0.08f, 0.48f);
            Color dress = new Color(0.70f, 0.45f, 0.92f);
            Color tights= new Color(0.92f, 0.86f, 1.00f);
            Color belt  = new Color(1.00f, 0.85f, 0.40f);
            Color shoes = new Color(0.30f, 0.14f, 0.45f);

            for (int i = 0; i < rends.Length; i++)
            {
                var r = rends[i];
                string n = r.gameObject.name.ToLower();
                // Sara.blend has materials "Material.001"..."Material.008" — infer by mesh Y-position
                float cy = r.bounds.center.y - b.min.y; // height in character frame (0=feet)
                float ny = cy / b.size.y;                // 0..1
                Color c;
                bool isSkin = false;
                if      (n.Contains("hair") || ny > 0.93f)                 c = hair;          // top of head
                else if (n.Contains("belt") || (ny > 0.55f && ny < 0.62f)) c = belt;
                else if (n.Contains("shoe") || n.Contains("foot") || ny < 0.05f) c = shoes;
                else if (n.Contains("leg")  || n.Contains("tight") || n.Contains("pant")
                      || (ny >= 0.05f && ny < 0.48f))                       c = tights;
                else if (n.Contains("hand") || n.Contains("arm") || n.Contains("head")
                      || n.Contains("face") || n.Contains("skin") || n.Contains("body")
                      || (ny >= 0.85f && ny <= 0.93f))
                    { c = skin; isSkin = true; }
                else                                                        c = dress;

                var mat = new Material(ToonShader);
                mat.SetColor("_Color",             c);
                mat.SetColor("_ShadowColor",       new Color(c.r * 0.55f, c.g * 0.45f, c.b * 0.70f));
                mat.SetFloat("_ShadowThreshold",   0.32f);
                mat.SetFloat("_OutlineWidth",      0.010f);
                mat.SetColor("_OutlineColor",      new Color(0.06f, 0.03f, 0.12f));
                mat.SetFloat("_Wrap",              isSkin ? 0.65f : 0.45f);
                if (isSkin)
                {
                    mat.SetFloat("_SSSStrength", 0.45f);
                    mat.SetColor("_SSSColor",    new Color(1.00f, 0.45f, 0.38f));
                }
                r.material = mat;
            }

            // Sara's FBX has no baked face features — paint them procedurally on top of the head.
            AddFaceOverlay(instance, b);
            return instance;
        }

        static void AddFaceOverlay(GameObject visual, Bounds worldBounds)
        {
            // Find the actual head renderer: the topmost-centered mesh (should be near ny ~ 0.95)
            Renderer headR = null; float bestY = float.NegativeInfinity;
            foreach (var r in visual.GetComponentsInChildren<Renderer>(true))
            {
                if (r.bounds.center.y > bestY) { bestY = r.bounds.center.y; headR = r; }
            }
            Bounds headB = headR != null ? headR.bounds : worldBounds;
            float headTop    = headB.max.y;
            float headBottom = headB.min.y;
            float headSize   = Mathf.Max(headB.size.x, headB.size.z);

            // Caller applies a Y-180° rotation to `visual` so its face ends up at world -Z (camera side).
            // Children we spawn here live in `visual`'s local frame (pre-rotation), so local +Z maps to world -Z.
            float headCYWorld = (headTop + headBottom) * 0.5f;
            float eyeYW   = Mathf.Lerp(headBottom, headTop, 0.55f);
            float mouthYW = Mathf.Lerp(headBottom, headTop, 0.28f);
            float noseYW  = Mathf.Lerp(headBottom, headTop, 0.42f);
            float browYW  = Mathf.Lerp(headBottom, headTop, 0.70f);
            float frontZW = headSize * 0.46f;
            float eyeDXW  = headSize * 0.16f;

            // Convert world-space face anchor points into visual's un-scaled local frame.
            // (visual has no rotation yet; caller applies the Y-180 after.)
            float invS = 1f / Mathf.Max(0.0001f, visual.transform.localScale.x);
            Vector3 hc = new Vector3(headB.center.x, 0f, headB.center.z);
            Vector3 vp = visual.transform.position;
            float headCXlocal = (hc.x - vp.x) * invS;
            float headCZlocal = (hc.z - vp.z) * invS;
            float eyeY  = (eyeYW   - vp.y) * invS;
            float mouY  = (mouthYW - vp.y) * invS;
            float nosY  = (noseYW  - vp.y) * invS;
            float brwY  = (browYW  - vp.y) * invS;
            float frnZ  = headCZlocal + frontZW * invS;
            float eyDX  = eyeDXW  * invS;
            float hS    = headSize * invS;

            Color white = new Color(0.97f, 0.96f, 0.94f);
            Color iris  = new Color(0.28f, 0.18f, 0.38f);
            Color dark  = new Color(0.10f, 0.04f, 0.18f);
            Color nose  = new Color(1.00f, 0.80f, 0.72f);
            Color lip   = new Color(0.72f, 0.28f, 0.32f);
            Color blush = new Color(1.00f, 0.62f, 0.62f);

            // Small dark ovals for eyes (Ghibli-style simple dots, no separate whites/pupils).
            SpawnFacePart(visual, "EyeL",      PrimitiveType.Sphere, new Vector3(headCXlocal - eyDX,eyeY, frnZ),               new Vector3(hS * 0.07f, hS * 0.10f, hS * 0.05f), dark, 0.40f, null);
            SpawnFacePart(visual, "EyeR",      PrimitiveType.Sphere, new Vector3(headCXlocal + eyDX, eyeY, frnZ),               new Vector3(hS * 0.07f, hS * 0.10f, hS * 0.05f), dark, 0.40f, null);
            // Tiny white catchlight
            SpawnFacePart(visual, "GlintL",    PrimitiveType.Sphere, new Vector3(headCXlocal - eyDX + hS * 0.015f, eyeY + hS * 0.025f, frnZ + hS * 0.03f), Vector3.one * hS * 0.025f, Color.white, 0.30f, null);
            SpawnFacePart(visual, "GlintR",    PrimitiveType.Sphere, new Vector3(headCXlocal + eyDX + hS * 0.015f, eyeY + hS * 0.025f, frnZ + hS * 0.03f), Vector3.one * hS * 0.025f, Color.white, 0.30f, null);
            // Soft mouth — small curved smile
            SpawnFacePart(visual, "Mouth",     PrimitiveType.Cube,   new Vector3(headCXlocal, mouY, frnZ),                  new Vector3(hS * 0.10f, hS * 0.018f, hS * 0.03f), lip,  0.45f, null);
            // Cheek blush — small & low opacity via soft color
            SpawnFacePart(visual, "BlushL",    PrimitiveType.Sphere, new Vector3(headCXlocal - eyDX - hS * 0.02f, mouY + hS * 0.06f, frnZ - hS * 0.02f), new Vector3(hS * 0.07f, hS * 0.05f, hS * 0.04f), blush, 0.75f, null);
            SpawnFacePart(visual, "BlushR",    PrimitiveType.Sphere, new Vector3(headCXlocal + eyDX + hS * 0.02f, mouY + hS * 0.06f, frnZ - hS * 0.02f), new Vector3(hS * 0.07f, hS * 0.05f, hS * 0.04f), blush, 0.75f, null);
        }

        static void SpawnFacePart(GameObject parent, string name, PrimitiveType type,
                                  Vector3 localPos, Vector3 localScale, Color c, float wrap,
                                  Quaternion? localRot)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            var col = go.GetComponent<Collider>();
            if (col) Object.DestroyImmediate(col);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot ?? Quaternion.identity;
            go.transform.localScale    = localScale;

            var mat = new Material(ToonShader);
            mat.SetColor("_Color", c);
            mat.SetColor("_ShadowColor", new Color(c.r * 0.55f, c.g * 0.45f, c.b * 0.70f));
            mat.SetFloat("_ShadowThreshold", 0.32f);
            mat.SetFloat("_OutlineWidth", 0.004f);
            mat.SetColor("_OutlineColor", new Color(0.06f, 0.03f, 0.12f));
            mat.SetFloat("_Wrap", wrap);
            go.GetComponent<Renderer>().material = mat;
        }

        static GameObject SpawnKenneyCharacter(Transform parent)
        {
            var prefab = Resources.Load<GameObject>("Kenney/Characters/character-female-a");
            if (prefab == null) return null;

            var instance = Object.Instantiate(prefab, parent);
            instance.name = "Visual";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            instance.transform.localScale    = Vector3.one * 1.5f;

            Color skin  = new Color(1.00f, 0.88f, 0.80f);
            Color hair  = new Color(0.22f, 0.08f, 0.48f);
            Color dress = new Color(0.70f, 0.45f, 0.92f);

            var renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var r in renderers)
            {
                string n = r.gameObject.name.ToLower();
                Color c;
                if (n.Contains("hair") || n.Contains("head-hair"))                   c = hair;
                else if (n.Contains("skin") || n.Contains("head") || n.Contains("body")
                      || n.Contains("arm")  || n.Contains("leg")  || n.Contains("hand")
                      || n.Contains("face") || n.Contains("foot") || n.Contains("neck")) c = skin;
                else                                                                 c = dress;

                var mat = new Material(ToonShader);
                mat.SetColor("_Color",             c);
                mat.SetColor("_EmissionColor",     c * 0.3f);
                mat.SetFloat("_EmissionIntensity", 0.25f);
                mat.SetFloat("_OutlineWidth",      0.012f);
                mat.SetColor("_OutlineColor",      Color.black);
                r.material = mat;
            }

            return instance;
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
            // Hands — at forearm tips, near hips (forearm tilts 18° forward, ends ~0.20 forward of shoulder)
            MakePart(visual.transform, PrimitiveType.Sphere, "HandL",
                new Vector3(-0.24f, 0.52f, 0.22f), Vector3.one * 0.105f,
                new Color(1.00f, 0.88f, 0.80f));
            MakePart(visual.transform, PrimitiveType.Sphere, "HandR",
                new Vector3( 0.24f, 0.52f, 0.22f), Vector3.one * 0.105f,
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
            // Eyelashes — lighter, less angled (was making her look sad/heavy)
            MakePartRotated(visual.transform, PrimitiveType.Cube, "LashL",
                new Vector3(-0.08f, 1.935f, 0.215f),
                Quaternion.Euler(0f, 0f, 6f),
                new Vector3(0.075f, 0.008f, 0.014f),
                new Color(0.14f, 0.07f, 0.24f));
            MakePartRotated(visual.transform, PrimitiveType.Cube, "LashR",
                new Vector3( 0.08f, 1.935f, 0.215f),
                Quaternion.Euler(0f, 0f, -6f),
                new Vector3(0.075f, 0.008f, 0.014f),
                new Color(0.14f, 0.07f, 0.24f));
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
            // Curved mouth — smile curves UP at corners (positive expression, was downturned)
            MakePartRotated(visual.transform, PrimitiveType.Cube, "SmileL",
                new Vector3(-0.040f, 1.790f, 0.208f),
                Quaternion.Euler(0f, 0f, -18f),
                new Vector3(0.048f, 0.013f, 0.013f),
                new Color(0.78f, 0.28f, 0.42f));
            MakePartRotated(visual.transform, PrimitiveType.Cube, "SmileR",
                new Vector3( 0.040f, 1.790f, 0.208f),
                Quaternion.Euler(0f, 0f,  18f),
                new Vector3(0.048f, 0.013f, 0.013f),
                new Color(0.78f, 0.28f, 0.42f));
            MakePart(visual.transform, PrimitiveType.Sphere, "SmileCenter",
                new Vector3(0f, 1.788f, 0.212f), new Vector3(0.020f, 0.010f, 0.010f),
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
            // Arms — relaxed at sides, slight outward tilt + forward bend at elbow (natural, not T-pose)
            MakePartRotated(visual.transform, PrimitiveType.Capsule, "ArmL",
                new Vector3(-0.30f, 0.96f, 0.02f),
                Quaternion.Euler(10f, 0f, 8f),
                new Vector3(0.13f, 0.36f, 0.13f),
                new Color(0.78f, 0.68f, 0.92f));
            MakePartRotated(visual.transform, PrimitiveType.Capsule, "ArmR",
                new Vector3( 0.30f, 0.96f, 0.02f),
                Quaternion.Euler(10f, 0f, -8f),
                new Vector3(0.13f, 0.36f, 0.13f),
                new Color(0.78f, 0.68f, 0.92f));
            // Forearms — subtle forward bend, hands rest near hip
            MakePartRotated(visual.transform, PrimitiveType.Capsule, "ForearmL",
                new Vector3(-0.27f, 0.68f, 0.09f),
                Quaternion.Euler(18f, 0f, 5f),
                new Vector3(0.115f, 0.22f, 0.115f),
                new Color(1.00f, 0.88f, 0.80f));
            MakePartRotated(visual.transform, PrimitiveType.Capsule, "ForearmR",
                new Vector3( 0.27f, 0.68f, 0.09f),
                Quaternion.Euler(18f, 0f, -5f),
                new Vector3(0.115f, 0.22f, 0.115f),
                new Color(1.00f, 0.88f, 0.80f));
            // Dress — narrower A-line (was cone-wide), stops above knees so legs visible
            MakePart(visual.transform, PrimitiveType.Cylinder, "DressTop",
                new Vector3(0f, 0.80f, 0f), new Vector3(0.40f, 0.12f, 0.40f),
                new Color(0.62f, 0.35f, 0.90f));
            MakePart(visual.transform, PrimitiveType.Cylinder, "DressMid",
                new Vector3(0f, 0.62f, 0f), new Vector3(0.46f, 0.10f, 0.46f),
                new Color(0.55f, 0.30f, 0.82f));
            MakePart(visual.transform, PrimitiveType.Cylinder, "DressHem",
                new Vector3(0f, 0.48f, 0f), new Vector3(0.52f, 0.08f, 0.52f),
                new Color(0.48f, 0.25f, 0.74f));
            // Lace trim — thin emissive cylinder at dress hem
            var trim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trim.name = "DressTrim";
            trim.transform.SetParent(visual.transform, false);
            trim.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            trim.transform.localScale    = new Vector3(0.53f, 0.025f, 0.53f);
            var trimMat = new Material(ToonShader);
            trimMat.SetColor("_Color",             new Color(1.00f, 0.85f, 0.95f));
            trimMat.SetColor("_EmissionColor",     new Color(1.00f, 0.85f, 0.95f));
            trimMat.SetFloat("_EmissionIntensity", 0.9f);
            trimMat.SetFloat("_OutlineWidth",      0f);
            trim.GetComponent<MeshRenderer>().material = trimMat;
            Object.Destroy(trim.GetComponent<Collider>());
            // Belt
            MakePart(visual.transform, PrimitiveType.Cylinder, "Belt",
                new Vector3(0f, 0.93f, 0f), new Vector3(0.38f, 0.04f, 0.38f),
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

            // ── DRESS APRON — small white apron accent on front of skirt only (no floor ruffles: legs visible) ──
            MakePart(visual.transform, PrimitiveType.Cylinder, "DressApron",
                new Vector3(0f, 0.62f, 0.14f), new Vector3(0.28f, 0.12f, 0.14f),
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
            // Runtime Resources props: use the active visual language so existing
            // Kenney CC0 furniture reads like part of the same room.
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                Texture tex = null;
                var src = r.sharedMaterial;
                if (src != null)
                {
                    if (src.HasProperty("_MainTex")) tex = src.GetTexture("_MainTex");
                    if (tex == null && src.mainTexture != null) tex = src.mainTexture;
                }

                if (PhotorealMode)
                {
                    var m = MakePhotoMaterial(tint, 0.38f, false, 0f);
                    if (tex != null)
                    {
                        m.mainTexture = tex;
                        if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", tex);
                    }
                    r.sharedMaterial = m;
                    r.shadowCastingMode = ShadowCastingMode.On;
                    r.receiveShadows = true;
                }
                else
                {
                    var m = new Material(ToonShader);
                    m.SetColor("_Color", tint);
                    m.SetFloat("_OutlineWidth", 0.003f);
                    r.sharedMaterial = m;
                }
            }
            return go;
        }

        // ── Rooms ────────────────────────────────────────────────────────────
        RoomManager CreateRooms()
        {
            var rmGO = new GameObject("RoomManager");
            var rm   = rmGO.AddComponent<RoomManager>();

            if (PhotorealMode)
            {
                var photorealRoom = BuildPhotorealBedroom();
                photorealRoom.AddComponent<FeedingStation>();
                photorealRoom.AddComponent<PlayArea>();
                photorealRoom.AddComponent<SleepArea>();
                rm.startRoom = RoomType.LivingRoom;
                rm.AddRoom(RoomType.LivingRoom, photorealRoom);
                return rm;
            }

            // The approved Blender room is the playable starting room. The old
            // primitive room remains only as a resilience fallback.
            var lr = BuildHeroLivingRoom() ?? BuildRoom("LivingRoom", Vector3.zero,
                new Color(0.78f, 0.68f, 0.62f), true,
                new[]{ "Models/Props/Bedroom/Bed" },
                new[]{ new Vector3(-2.8f, 0f, 2.7f) },
                new[]{ Vector3.one * 1.2f },
                new[]{ new Color(0.92f, 0.72f, 0.72f) });
            lr.AddComponent<FeedingStation>();
            lr.AddComponent<PlayArea>();
            AddSpatialActionZones(lr.transform);
            AddHeroCollisionProxies(lr.transform);
            AddPersistentActivityStation(lr.transform, "PersistentPlayArena",
                MoonlightSpatialActionKind.Play, "Models/Hero/MoonPlayArena",
                new Vector3(0.27f, 0f, 0.95f), Vector3.one * 1.10f,
                new Vector3(-0.18f, 0.01f, 0.08f), new Vector3(-90f, 0f, 0f),
                Vector3.one * 0.84f);

            var kt = BuildRoom("Kitchen", new Vector3(14f, 0f, 0f),
                new Color(0.86f, 0.72f, 0.64f), false,
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
            AddAuthoredKitchenNook(kt.transform);
            AddSpatialActionZone(kt.transform, "KitchenCookingAction",
                MoonlightSpatialActionKind.Cook, "moon kitchen",
                kt.transform.position + new Vector3(0f, 0.04f, -0.95f),
                new Color(0.94f, 0.68f, 0.28f), 1.25f);
            Vector3 kitchenStationPosition = kt.transform.position + new Vector3(1.58f, 0.72f, -0.73f);
            AddPersistentActivityStation(kt.transform, "PersistentCookWorkbench",
                MoonlightSpatialActionKind.Cook, "Models/Hero/MoonKitchenWorkbench",
                kitchenStationPosition, Vector3.one,
                new Vector3(0f, 0.01f, 0.02f), new Vector3(0f, 180f, 0f),
                new Vector3(1f, 0.56f, 1f));
            AddCollisionProxy(kt.transform, "CookWorkbenchCollision",
                kitchenStationPosition + new Vector3(0f, 0.38f, 0f),
                new Vector3(1.74f, 0.76f, 0.94f));

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
            AddSpatialActionZone(bd.transform, "BedroomSleepAction",
                MoonlightSpatialActionKind.SleepCuddle, "dream bed",
                bd.transform.position + new Vector3(0f, 0.04f, 0.80f),
                new Color(0.54f, 0.64f, 0.92f), 1.35f);
            AddSpatialActionZone(bd.transform, "BedroomCareAction",
                MoonlightSpatialActionKind.Care, "moon spa",
                bd.transform.position + new Vector3(1.35f, 0.04f, -1.15f),
                new Color(0.28f, 0.72f, 0.68f), 1.35f);
            Vector3 careStationPosition = bd.transform.position + new Vector3(2.25f, 0.08f, -0.30f);
            AddPersistentActivityStation(bd.transform, "PersistentCareVanity",
                MoonlightSpatialActionKind.Care, "Models/Hero/MoonCareAtelier",
                careStationPosition, Vector3.one * 1.08f,
                new Vector3(-0.10f, 0.01f, 0.06f), new Vector3(-90f, 0f, 0f),
                Vector3.one * 0.86f);
            AddCollisionProxy(bd.transform, "CareVanityCollision",
                careStationPosition + new Vector3(0f, 0.45f, 0f),
                new Vector3(1.55f, 0.90f, 0.75f));

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
            AddSpatialActionZone(gd.transform, "GardenBloomAction",
                MoonlightSpatialActionKind.Garden, "moon garden",
                gd.transform.position + new Vector3(-0.75f, 0.04f, 0.35f),
                new Color(0.38f, 0.78f, 0.46f), 1.25f);
            AddPersistentActivityStation(gd.transform, "PersistentGardenAtelier",
                MoonlightSpatialActionKind.Garden, "Models/Hero/MoonGardenAtelier",
                gd.transform.position + new Vector3(0.35f, 0.08f, 0.65f), Vector3.one * 1.08f,
                new Vector3(-0.10f, 0.01f, 0.06f), new Vector3(-90f, 0f, 0f),
                Vector3.one * 0.86f);
            AddCollisionProxy(gd.transform, "GardenPlanterCollision",
                gd.transform.position + new Vector3(-1.35f, 0.40f, 1.65f),
                new Vector3(2.65f, 0.80f, 0.95f));
            AddCollisionProxy(gd.transform, "MagicWellCollision",
                gd.transform.position + new Vector3(2f, 0.55f, 2f),
                new Vector3(1.35f, 1.10f, 1.35f));

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
            AddSpatialActionZone(lb.transform, "LibraryReadingAction",
                MoonlightSpatialActionKind.Read, "star story",
                lb.transform.position + new Vector3(0.55f, 0.04f, -0.75f),
                new Color(0.88f, 0.66f, 0.30f), 1.25f);
            AddPersistentActivityStation(lb.transform, "PersistentReadingNook",
                MoonlightSpatialActionKind.Read, "Models/Hero/MoonReadingNook",
                lb.transform.position + new Vector3(1.63f, 0.09f, -0.45f), Vector3.one * 1.08f,
                new Vector3(-0.08f, 0.01f, 0.06f), new Vector3(-90f, 0f, 0f),
                Vector3.one * 0.86f);
            AddCollisionProxy(lb.transform, "BookshelfCollision",
                lb.transform.position + new Vector3(-3.5f, 0.95f, 3.5f),
                new Vector3(1.65f, 1.90f, 0.75f));
            AddCollisionProxy(lb.transform, "ReadingDeskCollision",
                lb.transform.position + new Vector3(0f, 0.42f, -0.65f),
                new Vector3(2.0f, 0.84f, 0.90f));

            rm.AddRoom(RoomType.LivingRoom, lr);
            rm.AddRoom(RoomType.Kitchen,    kt);
            rm.AddRoom(RoomType.Bedroom,    bd);
            rm.AddRoom(RoomType.Garden,     gd);
            rm.AddRoom(RoomType.Library,    lb);

            return rm;
        }

        GameObject BuildPhotorealBedroom()
        {
            var root = new GameObject("PhotorealBedroom");
            root.transform.position = Vector3.zero;

            var amb = root.AddComponent<RoomAmbience>();
            amb.ambientColor = new Color(0.98f, 0.70f, 0.58f);

            var bedroom = new GameObject("Moonlight3DBedroomVisuals");
            bedroom.transform.SetParent(root.transform, false);
            CreateRoomPhotoBackdrop(bedroom.transform);
            BuildFairytaleBedroom3D(bedroom.transform);
            CreatePhotorealFloorBlend(bedroom.transform);
            CreateGlossyKidTreats(bedroom.transform);
            BuildFairytaleMeadow3D(root.transform);

            var lamp = new GameObject("StorybookRoomGlow");
            lamp.transform.SetParent(root.transform, false);
            lamp.transform.localPosition = new Vector3(-1.92f, 1.42f, -0.40f);
            var lampLight = lamp.AddComponent<Light>();
            lampLight.type = LightType.Point;
            lampLight.color = new Color(1.0f, 0.76f, 0.46f);
            lampLight.intensity = 1.45f;
            lampLight.range = 3.6f;

            var probeGO = new GameObject("BedroomReflectionProbe");
            probeGO.transform.SetParent(root.transform, false);
            probeGO.transform.localPosition = new Vector3(0.35f, 1.1f, -0.35f);
            var probe = probeGO.AddComponent<ReflectionProbe>();
            probe.size = new Vector3(5.4f, 3.2f, 4.1f);
            probe.intensity = 0.36f;

            root.SetActive(true);
            return root;
        }

        static void BuildFairytaleBedroom3D(Transform parent)
        {
            // The GPT Image 2 plate already carries the bedroom floor and rug. Large
            // procedural floor slabs made the prototype read as a pasted-on diorama.
            BuildBedroomPlayProps(parent);
            BuildBedroomBathAndSnack(parent);
            BuildBedroomFairyLights(parent);
        }

        static void BuildBedroomWindow(Transform parent)
        {
            PhotoGlowQuad("PGR_WindowWarmBloom", parent, new Vector3(-0.65f, 1.70f, 1.49f),
                Quaternion.identity, new Vector3(1.95f, 1.30f, 1f), new Color(1f, 0.72f, 0.44f, 0.16f));
            var sky = PhotoPrim(PrimitiveType.Cube, "PGR_WindowMeadowSky", parent,
                new Vector3(-0.65f, 1.70f, 1.50f), new Vector3(1.50f, 0.92f, 0.035f),
                new Color(0.62f, 0.78f, 0.92f), 0.08f, true, 0.25f);
            sky.GetComponent<MeshRenderer>().material.mainTexture = MakeVerticalGradientTex(96, 96,
                new Color(0.40f, 0.68f, 0.98f), new Color(1.00f, 0.76f, 0.52f));

            Color frame = new Color(0.98f, 0.84f, 0.58f);
            PhotoPrim(PrimitiveType.Cube, "PGR_WindowFrameTop", parent, new Vector3(-0.65f, 2.20f, 1.45f), new Vector3(1.72f, 0.07f, 0.08f), frame, 0.34f);
            PhotoPrim(PrimitiveType.Cube, "PGR_WindowFrameBottom", parent, new Vector3(-0.65f, 1.20f, 1.45f), new Vector3(1.72f, 0.07f, 0.08f), frame, 0.34f);
            PhotoPrim(PrimitiveType.Cube, "PGR_WindowFrameLeft", parent, new Vector3(-1.52f, 1.70f, 1.45f), new Vector3(0.07f, 1.06f, 0.08f), frame, 0.34f);
            PhotoPrim(PrimitiveType.Cube, "PGR_WindowFrameRight", parent, new Vector3(0.22f, 1.70f, 1.45f), new Vector3(0.07f, 1.06f, 0.08f), frame, 0.34f);
            PhotoPrim(PrimitiveType.Cube, "PGR_WindowCrossV", parent, new Vector3(-0.65f, 1.70f, 1.44f), new Vector3(0.045f, 0.98f, 0.06f), frame, 0.34f);
            PhotoPrim(PrimitiveType.Cube, "PGR_WindowCrossH", parent, new Vector3(-0.65f, 1.70f, 1.435f), new Vector3(1.52f, 0.045f, 0.06f), frame, 0.34f);

            PhotoPrim(PrimitiveType.Cube, "PGR_CurtainL", parent, new Vector3(-1.68f, 1.63f, 1.40f), new Vector3(0.20f, 1.18f, 0.06f), new Color(0.96f, 0.48f, 0.66f), 0.52f);
            PhotoPrim(PrimitiveType.Cube, "PGR_CurtainR", parent, new Vector3(0.38f, 1.63f, 1.40f), new Vector3(0.20f, 1.18f, 0.06f), new Color(0.96f, 0.48f, 0.66f), 0.52f);

            var sun = PhotoPrim(PrimitiveType.Sphere, "PGR_WindowSunGem", parent,
                new Vector3(-1.08f, 1.93f, 1.39f), Vector3.one * 0.13f,
                new Color(1.0f, 0.82f, 0.34f), 0.42f, true, 1.8f);
            sun.AddComponent<StarTwinkle>();
        }

        static void BuildBedroomBed(Transform parent)
        {
            PhotoPrim(PrimitiveType.Cube, "MoonlightRealBedBase", parent,
                new Vector3(2.16f, 0.17f, -0.54f), new Vector3(1.38f, 0.24f, 0.78f),
                new Color(0.46f, 0.27f, 0.22f), 0.34f);
            PhotoPrim(PrimitiveType.Cube, "MoonlightRealBedHeadboard", parent,
                new Vector3(2.78f, 0.60f, -0.54f), new Vector3(0.14f, 0.88f, 0.96f),
                new Color(0.42f, 0.26f, 0.32f), 0.32f);
            PhotoPrim(PrimitiveType.Sphere, "MoonlightRealMattress", parent,
                new Vector3(2.05f, 0.38f, -0.54f), new Vector3(0.72f, 0.13f, 0.42f),
                new Color(0.86f, 0.76f, 0.68f), 0.42f);
            PhotoPrim(PrimitiveType.Sphere, "MoonlightRealBlanket", parent,
                new Vector3(2.17f, 0.50f, -0.54f), new Vector3(0.56f, 0.10f, 0.46f),
                new Color(0.68f, 0.34f, 0.38f), 0.48f);
            PhotoPrim(PrimitiveType.Sphere, "MoonlightRealPillow", parent,
                new Vector3(1.48f, 0.51f, -0.54f), new Vector3(0.22f, 0.08f, 0.30f),
                new Color(0.86f, 0.80f, 0.66f), 0.46f);

            Color post = new Color(0.66f, 0.45f, 0.28f);
            PhotoPrim(PrimitiveType.Cylinder, "MoonlightBedPostFrontA", parent, new Vector3(1.45f, 0.46f, -0.09f), new Vector3(0.045f, 0.44f, 0.045f), post, 0.42f);
            PhotoPrim(PrimitiveType.Cylinder, "MoonlightBedPostFrontB", parent, new Vector3(1.45f, 0.46f, -0.99f), new Vector3(0.045f, 0.44f, 0.045f), post, 0.42f);
            PhotoPrim(PrimitiveType.Sphere, "MoonlightBedPostOrbA", parent, new Vector3(1.45f, 0.91f, -0.09f), Vector3.one * 0.075f, new Color(1.0f, 0.80f, 0.48f), 0.58f, true, 0.35f);
            PhotoPrim(PrimitiveType.Sphere, "MoonlightBedPostOrbB", parent, new Vector3(1.45f, 0.91f, -0.99f), Vector3.one * 0.075f, new Color(1.0f, 0.80f, 0.48f), 0.58f, true, 0.35f);
            MakePhotoRotated(PrimitiveType.Cube, "MoonlightBedCanopyRibbon", parent,
                new Vector3(2.04f, 1.05f, -0.54f), Quaternion.Euler(0f, 0f, -2f),
                new Vector3(1.12f, 0.032f, 0.78f), new Color(0.66f, 0.36f, 0.48f), 0.34f, true, 0.08f);

            PhotoGlowQuad("MoonlightBedWarmUnderGlow", parent, new Vector3(2.05f, 0.035f, -0.54f),
                Quaternion.Euler(90f, 0f, 0f), new Vector3(1.62f, 0.88f, 1f),
                new Color(0.55f, 0.20f, 0.18f, 0.14f));
        }

        static void BuildBedroomDollhouse(Transform parent)
        {
            PhotoPrim(PrimitiveType.Cube, "MoonlightDollhouseBody", parent,
                new Vector3(-0.78f, 0.40f, -0.62f), new Vector3(0.74f, 0.72f, 0.26f),
                new Color(0.93f, 0.62f, 0.74f), 0.45f);
            MakePhotoRotated(PrimitiveType.Cube, "MoonlightDollhouseRoofL", parent,
                new Vector3(-0.92f, 0.82f, -0.62f), Quaternion.Euler(0f, 0f, 25f),
                new Vector3(0.48f, 0.12f, 0.30f), new Color(0.64f, 0.36f, 0.58f), 0.42f, false, 0f);
            MakePhotoRotated(PrimitiveType.Cube, "MoonlightDollhouseRoofR", parent,
                new Vector3(-0.64f, 0.82f, -0.62f), Quaternion.Euler(0f, 0f, -25f),
                new Vector3(0.48f, 0.12f, 0.30f), new Color(0.64f, 0.36f, 0.58f), 0.42f, false, 0f);
            for (int y = 0; y < 2; y++)
            for (int x = 0; x < 2; x++)
            {
                var w = PhotoPrim(PrimitiveType.Cube, $"MoonlightDollhouseWindow{x}_{y}", parent,
                    new Vector3(-0.94f + x * 0.32f, 0.31f + y * 0.26f, -0.77f),
                    new Vector3(0.12f, 0.12f, 0.035f), new Color(1.0f, 0.75f, 0.36f), 0.40f, true, 1.4f);
                w.AddComponent<StarTwinkle>();
            }
            CreateDollhouseMagicGlow(parent);
        }

        static void BuildBedroomPlayProps(Transform parent)
        {
            PhotoPrim(PrimitiveType.Sphere, "PGR_PlushBody", parent,
                new Vector3(-1.44f, 0.20f, -1.03f), new Vector3(0.24f, 0.27f, 0.21f),
                new Color(0.68f, 0.55f, 0.44f), 0.42f);
            PhotoPrim(PrimitiveType.Sphere, "PGR_PlushHead", parent,
                new Vector3(-1.44f, 0.43f, -1.04f), Vector3.one * 0.15f,
                new Color(0.72f, 0.58f, 0.48f), 0.42f);
            PhotoPrim(PrimitiveType.Sphere, "PGR_PlushEarL", parent,
                new Vector3(-1.55f, 0.52f, -1.04f), new Vector3(0.07f, 0.10f, 0.05f),
                new Color(0.88f, 0.62f, 0.58f), 0.52f);
            PhotoPrim(PrimitiveType.Sphere, "PGR_PlushEarR", parent,
                new Vector3(-1.33f, 0.52f, -1.04f), new Vector3(0.07f, 0.10f, 0.05f),
                new Color(0.88f, 0.62f, 0.58f), 0.52f);
            PhotoPrim(PrimitiveType.Sphere, "PGR_PlushEyeL", parent,
                new Vector3(-1.49f, 0.45f, -1.17f), Vector3.one * 0.018f,
                new Color(0.08f, 0.05f, 0.06f), 0.30f);
            PhotoPrim(PrimitiveType.Sphere, "PGR_PlushEyeR", parent,
                new Vector3(-1.39f, 0.45f, -1.17f), Vector3.one * 0.018f,
                new Color(0.08f, 0.05f, 0.06f), 0.30f);

            SpawnKenney(parent, "Kenney/Furniture/bookcaseOpen",
                new Vector3(-2.05f, 0.00f, 0.72f), new Vector3(0f, 90f, 0f), 0.105f,
                new Color(0.70f, 0.46f, 0.32f));
            SpawnKenney(parent, "Kenney/Furniture/books",
                new Vector3(-2.00f, 0.31f, 0.72f), new Vector3(0f, 90f, 0f), 0.075f,
                new Color(0.64f, 0.78f, 1.00f));
            SpawnKenney(parent, "Kenney/Furniture/plantSmall2",
                new Vector3(-1.95f, 0.55f, 0.68f), Vector3.zero, 0.070f,
                new Color(0.52f, 0.90f, 0.54f));
            SpawnKenney(parent, "Kenney/Furniture/lampRoundFloor",
                new Vector3(-2.06f, 0.00f, -0.42f), new Vector3(0f, 18f, 0f), 0.105f,
                new Color(1.0f, 0.78f, 0.48f));
            SpawnKenney(parent, "Kenney/Furniture/chairCushion",
                new Vector3(-0.40f, 0.00f, -1.42f), new Vector3(0f, -22f, 0f), 0.090f,
                new Color(0.82f, 0.62f, 0.92f));
        }

        static void BuildBedroomBathAndSnack(Transform parent)
        {
            PhotoPrim(PrimitiveType.Cube, "SnackTableTop", parent,
                new Vector3(-0.12f, 0.24f, -1.48f), new Vector3(0.58f, 0.055f, 0.34f),
                new Color(0.40f, 0.26f, 0.18f), 0.30f);
            PhotoPrim(PrimitiveType.Cylinder, "SnackTableLeg", parent,
                new Vector3(-0.12f, 0.11f, -1.48f), new Vector3(0.045f, 0.12f, 0.045f),
                new Color(0.54f, 0.34f, 0.24f), 0.34f);
            PhotoPrim(PrimitiveType.Cylinder, "SnackPlate", parent,
                new Vector3(-0.12f, 0.295f, -1.48f), new Vector3(0.18f, 0.012f, 0.18f),
                new Color(0.92f, 0.84f, 0.70f), 0.58f);

            PhotoPrim(PrimitiveType.Sphere, "BubbleTubBase", parent,
                new Vector3(1.24f, 0.18f, 0.06f), new Vector3(0.38f, 0.14f, 0.24f),
                new Color(0.54f, 0.68f, 0.72f), 0.42f);
            PhotoPrim(PrimitiveType.Sphere, "BubbleTubWater", parent,
                new Vector3(1.24f, 0.31f, 0.06f), new Vector3(0.31f, 0.035f, 0.19f),
                new Color(0.68f, 0.92f, 0.96f), 0.72f, true, 0.14f);
            PhotoPrim(PrimitiveType.Cylinder, "BubbleTubRim", parent,
                new Vector3(1.24f, 0.32f, 0.06f), new Vector3(0.36f, 0.055f, 0.23f),
                new Color(0.78f, 0.88f, 0.88f), 0.54f);
            for (int i = 0; i < 10; i++)
            {
                float x = 0.96f + (i % 5) * 0.14f;
                float z = -0.08f + (i / 5) * 0.18f;
                PhotoPrim(PrimitiveType.Sphere, $"BubbleBathFoam{i}", parent,
                    new Vector3(x, 0.38f + Mathf.Sin(i) * 0.020f, z), Vector3.one * (0.050f + (i % 3) * 0.010f),
                    new Color(0.96f, 1.00f, 1.00f), 0.88f, true, 0.35f);
            }
        }

        static void BuildBedroomFairyLights(Transform parent)
        {
            Vector3 prev = Vector3.zero;
            for (int i = 0; i < 11; i++)
            {
                float t = i / 10f;
                Vector3 p = new Vector3(Mathf.Lerp(-1.95f, 2.18f, t), 2.36f + Mathf.Sin(t * Mathf.PI * 2f) * 0.08f, 1.36f);
                if (i > 0)
                {
                    Vector3 mid = (prev + p) * 0.5f;
                    float len = Vector3.Distance(prev, p);
                    var strand = PhotoPrim(PrimitiveType.Cube, $"PGR_FairyLightWire{i}", parent,
                        mid, new Vector3(len, 0.012f, 0.012f), new Color(0.32f, 0.20f, 0.18f), 0.18f);
                    float angle = Mathf.Atan2(p.y - prev.y, p.x - prev.x) * Mathf.Rad2Deg;
                    strand.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                }
                Color c = i % 3 == 0 ? new Color(1f, 0.52f, 0.68f) : i % 3 == 1 ? new Color(1f, 0.84f, 0.42f) : new Color(0.54f, 0.90f, 1f);
                var bulb = PhotoPrim(PrimitiveType.Sphere, $"PGR_FairyBulb{i}", parent, p, Vector3.one * 0.046f, c, 0.55f, true, 1.7f);
                bulb.AddComponent<StarTwinkle>();
                prev = p;
            }
        }

        static void BuildBedroomMagicDetails(Transform parent)
        {
            PhotoGlowQuad("RoomBedSoftFloorShadow", parent, new Vector3(2.05f, 0.018f, -0.52f),
                Quaternion.Euler(90f, 0f, 0f), new Vector3(1.90f, 0.92f, 1f),
                new Color(0.10f, 0.04f, 0.04f, 0.12f));
            PhotoGlowQuad("RoomDollhouseSoftShadow", parent, new Vector3(-0.82f, 0.018f, -0.64f),
                Quaternion.Euler(90f, 0f, 0f), new Vector3(1.00f, 0.48f, 1f),
                new Color(0.10f, 0.04f, 0.04f, 0.10f));
            PhotoGlowQuad("RoomBathSoftGlow", parent, new Vector3(1.24f, 0.030f, 0.08f),
                Quaternion.Euler(90f, 0f, 0f), new Vector3(0.92f, 0.62f, 1f),
                new Color(0.48f, 0.88f, 1.00f, 0.10f));

            Color frame = new Color(1.0f, 0.74f, 0.50f);
            Color matPink = new Color(0.94f, 0.54f, 0.70f);
            Color matBlue = new Color(0.54f, 0.78f, 0.96f);
            BuildTinyWallFrame(parent, new Vector3(1.28f, 1.84f, 1.42f), frame, matBlue, "MoonFrameA");
            BuildTinyWallFrame(parent, new Vector3(1.74f, 1.66f, 1.42f), frame, matPink, "MoonFrameB");

            PhotoPrim(PrimitiveType.Cylinder, "MoonMobileCord", parent,
                new Vector3(0.48f, 2.33f, -0.22f), new Vector3(0.010f, 0.28f, 0.010f),
                new Color(0.88f, 0.72f, 0.55f), 0.20f);
            var moon = PhotoPrim(PrimitiveType.Sphere, "MoonMobileCrescentGlow", parent,
                new Vector3(0.48f, 2.02f, -0.22f), new Vector3(0.13f, 0.13f, 0.035f),
                new Color(1.0f, 0.86f, 0.38f), 0.50f, true, 0.75f);
            moon.AddComponent<StarTwinkle>();
            PhotoPrim(PrimitiveType.Sphere, "MoonMobileCrescentCut", parent,
                new Vector3(0.53f, 2.03f, -0.245f), new Vector3(0.10f, 0.10f, 0.030f),
                new Color(1.0f, 0.78f, 0.84f), 0.28f);
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f;
                var star = MakePhotoRotated(PrimitiveType.Cube, $"MoonMobileStar{i}", parent,
                    new Vector3(0.48f + Mathf.Cos(a) * 0.26f, 1.91f + Mathf.Sin(a * 1.7f) * 0.06f, -0.22f + Mathf.Sin(a) * 0.10f),
                    Quaternion.Euler(0f, 0f, 45f), Vector3.one * 0.050f,
                    new Color(1.0f, 0.82f, 0.42f), 0.48f, true, 0.55f);
                star.AddComponent<StarTwinkle>();
            }
        }

        static void BuildBedroomProductionDetails(Transform parent)
        {
            // Production-read details: scale cues, contact depth, and warm fairytale light.
            Color seam = new Color(0.42f, 0.24f, 0.16f);
            for (int i = 0; i < 9; i++)
            {
                float z = -3.02f + i * 0.52f;
                var floorLine = PhotoPrim(PrimitiveType.Cube, $"FloorboardSeam{i}", parent,
                    new Vector3(0.25f, 0.010f, z), new Vector3(5.10f, 0.010f, 0.010f),
                    seam, 0.16f);
                floorLine.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            }

            for (int i = 0; i < 5; i++)
            {
                float x = -2.05f + i * 1.12f;
                var plankTint = i % 2 == 0 ? new Color(0.78f, 0.52f, 0.34f, 1f) : new Color(0.66f, 0.43f, 0.28f, 1f);
                var plank = PhotoPrim(PrimitiveType.Cube, $"FloorboardWarmVariation{i}", parent,
                    new Vector3(x, 0.003f, -0.72f), new Vector3(0.025f, 0.008f, 5.00f),
                    plankTint, 0.20f);
                plank.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            }

            PhotoGlowQuad("WindowSunbeamLongFloor", parent, new Vector3(-0.04f, 0.050f, -1.08f),
                Quaternion.Euler(90f, 0f, -17f), new Vector3(2.90f, 0.48f, 1f),
                new Color(1.00f, 0.70f, 0.34f, 0.105f));
            PhotoGlowQuad("WindowSunbeamShortFloor", parent, new Vector3(-0.86f, 0.055f, -0.18f),
                Quaternion.Euler(90f, 0f, -18f), new Vector3(1.65f, 0.28f, 1f),
                new Color(1.00f, 0.82f, 0.46f, 0.080f));
            PhotoGlowQuad("WindowAirGlowColumn", parent, new Vector3(-0.56f, 1.33f, 0.66f),
                Quaternion.Euler(13f, 0f, -10f), new Vector3(0.56f, 1.86f, 1f),
                new Color(1.00f, 0.76f, 0.46f, 0.085f));

            for (int i = 0; i < 6; i++)
            {
                float y = 1.10f + i * 0.19f;
                PhotoPrim(PrimitiveType.Cube, $"CurtainLeftFold{i}", parent,
                    new Vector3(-1.66f + Mathf.Sin(i * 1.7f) * 0.018f, y, 1.355f),
                    new Vector3(0.030f, 0.125f, 0.030f),
                    new Color(0.86f, 0.32f, 0.52f), 0.48f, true, 0.08f);
                PhotoPrim(PrimitiveType.Cube, $"CurtainRightFold{i}", parent,
                    new Vector3(0.36f + Mathf.Sin(i * 1.3f) * 0.018f, y, 1.355f),
                    new Vector3(0.030f, 0.125f, 0.030f),
                    new Color(0.86f, 0.32f, 0.52f), 0.48f, true, 0.08f);
            }

            Color shelfWood = new Color(0.62f, 0.38f, 0.24f);
            PhotoPrim(PrimitiveType.Cube, "MiniWallShelf", parent,
                new Vector3(-1.72f, 1.18f, 1.42f), new Vector3(0.82f, 0.045f, 0.12f),
                shelfWood, 0.34f);
            for (int i = 0; i < 5; i++)
            {
                Color c = i % 3 == 0 ? new Color(0.44f, 0.76f, 1.00f) :
                    i % 3 == 1 ? new Color(1.00f, 0.62f, 0.42f) : new Color(0.82f, 0.58f, 0.96f);
                PhotoPrim(PrimitiveType.Cube, $"MiniShelfBook{i}", parent,
                    new Vector3(-2.04f + i * 0.14f, 1.27f, 1.36f),
                    new Vector3(0.07f, 0.17f + (i % 2) * 0.035f, 0.065f),
                    c, 0.30f, true, 0.06f);
            }

            Color decal = new Color(1f, 0.82f, 0.42f);
            for (int i = 0; i < 9; i++)
            {
                float x = -0.15f + Mathf.Sin(i * 2.31f) * 1.70f;
                float y = 1.50f + Mathf.Abs(Mathf.Cos(i * 1.73f)) * 0.76f;
                var star = MakePhotoRotated(PrimitiveType.Cube, $"WallpaperTinyStar{i}", parent,
                    new Vector3(x, y, 1.415f), Quaternion.Euler(0f, 0f, 45f),
                    Vector3.one * (0.034f + (i % 3) * 0.006f), decal, 0.44f, true, 0.32f);
                star.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                star.AddComponent<StarTwinkle>();
            }

            var lampShade = PhotoPrim(PrimitiveType.Cylinder, "BedsideWarmLampShade", parent,
                new Vector3(1.42f, 0.78f, 0.24f), new Vector3(0.16f, 0.10f, 0.16f),
                new Color(1.00f, 0.74f, 0.42f), 0.62f, true, 0.55f);
            lampShade.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            PhotoPrim(PrimitiveType.Cylinder, "BedsideWarmLampStem", parent,
                new Vector3(1.42f, 0.54f, 0.24f), new Vector3(0.025f, 0.18f, 0.025f),
                new Color(0.64f, 0.42f, 0.28f), 0.36f);
            PhotoGlowQuad("BedsideLampBloom", parent, new Vector3(1.42f, 0.80f, 0.19f),
                Quaternion.identity, new Vector3(0.56f, 0.56f, 1f),
                new Color(1.00f, 0.66f, 0.34f, 0.13f));

            var warm = new GameObject("BedsidePracticalWarmLight");
            warm.transform.SetParent(parent, false);
            warm.transform.localPosition = new Vector3(1.40f, 0.84f, 0.05f);
            var l = warm.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1.00f, 0.64f, 0.38f);
            l.intensity = 0.76f;
            l.range = 2.05f;
            l.shadows = LightShadows.None;
        }

        static void BuildTinyWallFrame(Transform parent, Vector3 center, Color frame, Color fill, string prefix)
        {
            PhotoPrim(PrimitiveType.Cube, $"{prefix}Back", parent,
                center, new Vector3(0.34f, 0.24f, 0.025f), fill, 0.34f, true, 0.08f);
            PhotoPrim(PrimitiveType.Cube, $"{prefix}Top", parent,
                center + new Vector3(0f, 0.135f, -0.012f), new Vector3(0.40f, 0.035f, 0.035f), frame, 0.42f);
            PhotoPrim(PrimitiveType.Cube, $"{prefix}Bottom", parent,
                center + new Vector3(0f, -0.135f, -0.012f), new Vector3(0.40f, 0.035f, 0.035f), frame, 0.42f);
            PhotoPrim(PrimitiveType.Cube, $"{prefix}Left", parent,
                center + new Vector3(-0.205f, 0f, -0.012f), new Vector3(0.035f, 0.28f, 0.035f), frame, 0.42f);
            PhotoPrim(PrimitiveType.Cube, $"{prefix}Right", parent,
                center + new Vector3(0.205f, 0f, -0.012f), new Vector3(0.035f, 0.28f, 0.035f), frame, 0.42f);
            PhotoPrim(PrimitiveType.Sphere, $"{prefix}GlowDot", parent,
                center + new Vector3(0f, 0f, -0.030f), Vector3.one * 0.050f,
                new Color(1f, 0.86f, 0.42f), 0.55f, true, 0.85f).AddComponent<StarTwinkle>();
        }

        static void BuildFairytaleMeadow3D(Transform parent)
        {
            var meadow = new GameObject("Moonlight3DMeadow");
            meadow.transform.SetParent(parent, false);
            CreateMeadowPhotoBackdrop(meadow.transform);

            var lightGO = new GameObject("PGR_MeadowWarmKey");
            lightGO.transform.SetParent(meadow.transform, false);
            lightGO.transform.localPosition = new Vector3(-1.3f, 2.2f, -1.2f);
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.78f, 0.48f);
            light.intensity = 1.65f;
            light.range = 5.0f;

            meadow.SetActive(false);
        }

        static GameObject PhotoPrim(PrimitiveType type, string name, Transform parent,
            Vector3 pos, Vector3 scale, Color color, float gloss, bool emissive = false, float emission = 1.2f)
        {
            return MakePhotoRotated(type, name, parent, pos, Quaternion.identity, scale, color, gloss, emissive, emission);
        }

        static GameObject MakePhotoRotated(PrimitiveType type, string name, Transform parent,
            Vector3 pos, Quaternion rot, Vector3 scale, Color color, float gloss, bool emissive, float emission)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot;
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            var mr = go.GetComponent<MeshRenderer>();
            mr.shadowCastingMode = ShadowCastingMode.On;
            mr.receiveShadows = true;
            mr.material = MakePhotoMaterial(color, gloss, emissive, emission);
            return go;
        }

        static Material MakePhotoMaterial(Color color, float gloss, bool emissive, float emission)
        {
            var shader = Shader.Find("Standard") ?? Shader.Find("Diffuse");
            var mat = new Material(shader);
            mat.color = color;
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", gloss);
            if (emissive && mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * emission);
            }
            return mat;
        }

        static GameObject PhotoGlowQuad(string name, Transform parent, Vector3 pos,
            Quaternion rot, Vector3 scale, Color color)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = pos;
            quad.transform.localRotation = rot;
            quad.transform.localScale = scale;
            Object.Destroy(quad.GetComponent<Collider>());
            var mat = new Material(TransparentSpriteShader);
            mat.mainTexture = MakeSoftCircleTex(128);
            mat.color = color;
            mat.renderQueue = 3000;
            quad.GetComponent<MeshRenderer>().material = mat;
            return quad;
        }

        static Texture2D MakeVerticalGradientTex(int width, int height, Color top, Color bottom)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < height; y++)
            {
                float t = y / Mathf.Max(1f, height - 1f);
                Color c = Color.Lerp(bottom, top, Mathf.SmoothStep(0f, 1f, t));
                for (int x = 0; x < width; x++)
                    tex.SetPixel(x, y, c);
            }
            tex.Apply(false, true);
            return tex;
        }

        static Texture2D MakeSpeckleTex(int size, Color baseColor, Color speckleColor, float amount)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Repeat;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float n = Mathf.Sin((x * 12.9898f + y * 78.233f) * 0.12f) * 43758.5453f;
                float f = n - Mathf.Floor(n);
                Color c = Color.Lerp(baseColor, speckleColor, f > 1f - amount ? 0.38f : 0f);
                tex.SetPixel(x, y, c);
            }
            tex.Apply(false, true);
            return tex;
        }

        static Texture2D MakeStripeTex(int width, int height, Color a, Color b)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Repeat;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float stripe = Mathf.Sin((x / Mathf.Max(1f, width - 1f)) * Mathf.PI * 8f);
                float fade = Mathf.InverseLerp(-0.25f, 1f, stripe);
                float grain = Mathf.Sin((x * 17.17f + y * 9.31f) * 0.08f) * 0.025f;
                Color c = Color.Lerp(a, b, fade * 0.42f + 0.08f);
                c.r = Mathf.Clamp01(c.r + grain);
                c.g = Mathf.Clamp01(c.g + grain);
                c.b = Mathf.Clamp01(c.b + grain);
                tex.SetPixel(x, y, c);
            }
            tex.Apply(false, true);
            return tex;
        }

        static void CreateRoomPhotoBackdrop(Transform parent)
        {
            var useNextPlate = true;
            var tex = Resources.Load<Texture2D>("Photoreal/room-gpt-image-2-v1");
            if (tex == null)
            {
                useNextPlate = false;
                tex = Resources.Load<Texture2D>("Photoreal/room-generated");
            }
            if (tex == null)
            {
                Debug.LogWarning("[Photoreal] Missing generated room backdrop");
                return;
            }

            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "PhotorealRoomBackdrop";
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = useNextPlate ? new Vector3(0.14f, 1.34f, 1.335f) : new Vector3(0.20f, 1.43f, 1.335f);
            quad.transform.localRotation = Quaternion.identity;
            float aspect = (float)tex.width / Mathf.Max(1, tex.height);
            float height = useNextPlate ? 6.65f : 7.20f;
            quad.transform.localScale = new Vector3(height * aspect, height, 1f);
            Object.Destroy(quad.GetComponent<Collider>());

            var mat = new Material(TransparentSpriteShader);
            mat.mainTexture = tex;
            mat.color = Color.white;
            quad.GetComponent<MeshRenderer>().material = mat;
        }

        static void CreateMeadowPhotoBackdrop(Transform parent)
        {
            var useNextPlate = true;
            var tex = Resources.Load<Texture2D>("Photoreal/meadow-gpt-image-2-v1");
            if (tex == null)
            {
                useNextPlate = false;
                tex = Resources.Load<Texture2D>("Photoreal/meadow-generated");
            }
            if (tex == null) return;

            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "PhotorealMeadowBackdrop";
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = useNextPlate ? new Vector3(0.20f, 1.18f, 1.86f) : new Vector3(0.20f, 1.46f, 1.86f);
            quad.transform.localRotation = Quaternion.identity;
            float aspect = (float)tex.width / Mathf.Max(1, tex.height);
            float height = useNextPlate ? 7.45f : 6.40f;
            quad.transform.localScale = new Vector3(height * aspect, height, 1f);
            Object.Destroy(quad.GetComponent<Collider>());

            var mat = new Material(TransparentSpriteShader);
            mat.mainTexture = tex;
            mat.color = Color.white;
            quad.GetComponent<MeshRenderer>().material = mat;
        }

        static void CreatePhotorealFloorBlend(Transform parent)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Quad);
            floor.name = "WarmFloorSheen";
            floor.transform.SetParent(parent, false);
            floor.transform.localPosition = new Vector3(0.32f, 0.006f, -0.98f);
            floor.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            floor.transform.localScale = new Vector3(4.2f, 1.35f, 1f);
            Object.Destroy(floor.GetComponent<Collider>());
            var mat = new Material(TransparentSpriteShader);
            mat.mainTexture = MakeSoftCircleTex(128);
            mat.color = new Color(1.0f, 0.62f, 0.42f, 0.06f);
            floor.GetComponent<MeshRenderer>().material = mat;
        }

        static void CreatePhotoMatchedFairyLights(Transform parent)
        {
            Vector3[] points =
            {
                new Vector3(-0.84f, 2.58f, 0.70f),
                new Vector3(-0.48f, 2.75f, 0.70f),
                new Vector3(-0.12f, 2.84f, 0.70f),
                new Vector3( 0.28f, 2.73f, 0.70f),
                new Vector3( 0.66f, 2.65f, 0.70f),
                new Vector3( 1.05f, 2.72f, 0.70f),
                new Vector3( 1.42f, 2.62f, 0.70f)
            };

            for (int i = 0; i < points.Length; i++)
            {
                var bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bulb.name = $"PhotoFairyBulb{i}";
                bulb.transform.SetParent(parent, false);
                bulb.transform.localPosition = points[i];
                bulb.transform.localScale = Vector3.one * 0.045f;
                Object.Destroy(bulb.GetComponent<Collider>());
                var mat = new Material(Shader.Find("Standard"));
                Color c = i % 3 == 0 ? new Color(1f, 0.70f, 0.48f) : new Color(1f, 0.88f, 0.58f);
                mat.color = c;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", c * 2.2f);
                mat.SetFloat("_Glossiness", 0.55f);
                bulb.GetComponent<MeshRenderer>().material = mat;
            }
        }

        static void CreateDollhouseMagicGlow(Transform parent)
        {
            Vector3[] glowPoints =
            {
                new Vector3(-0.42f, 0.52f, -0.62f),
                new Vector3(-0.05f, 0.45f, -0.66f),
                new Vector3( 0.25f, 0.62f, -0.62f)
            };

            foreach (var p in glowPoints)
            {
                var lgo = new GameObject("DollhouseWindowGlow");
                lgo.transform.SetParent(parent, false);
                lgo.transform.localPosition = p;
                var l = lgo.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(1.0f, 0.58f, 0.32f);
                l.intensity = 0.9f;
                l.range = 1.4f;

                var sparkle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sparkle.name = "TinyDollhouseSparkle";
                sparkle.transform.SetParent(parent, false);
                sparkle.transform.localPosition = p;
                sparkle.transform.localScale = Vector3.one * 0.022f;
                Object.Destroy(sparkle.GetComponent<Collider>());
                var sm = new Material(Shader.Find("Standard"));
                sm.color = new Color(1f, 0.75f, 0.42f);
                sm.EnableKeyword("_EMISSION");
                sm.SetColor("_EmissionColor", new Color(1f, 0.52f, 0.22f) * 1.8f);
                sparkle.GetComponent<MeshRenderer>().material = sm;
            }
        }

        static void CreateDreamyRoomVignette(Transform parent)
        {
            var windowGlow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            windowGlow.name = "StorybookWindowGlow";
            windowGlow.transform.SetParent(parent, false);
            windowGlow.transform.localPosition = new Vector3(0.38f, 1.92f, -0.72f);
            windowGlow.transform.localScale = new Vector3(2.20f, 0.92f, 1f);
            Object.Destroy(windowGlow.GetComponent<Collider>());
            var glowMat = new Material(TransparentSpriteShader);
            glowMat.mainTexture = MakeSoftCircleTex(160);
            glowMat.color = new Color(1.0f, 0.76f, 0.46f, 0.11f);
            windowGlow.GetComponent<MeshRenderer>().material = glowMat;

            var rugGlow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            rugGlow.name = "FairytaleFloorWarmth";
            rugGlow.transform.SetParent(parent, false);
            rugGlow.transform.localPosition = new Vector3(-0.32f, 0.018f, -1.12f);
            rugGlow.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            rugGlow.transform.localScale = new Vector3(2.90f, 0.82f, 1f);
            Object.Destroy(rugGlow.GetComponent<Collider>());
            var rugMat = new Material(TransparentSpriteShader);
            rugMat.mainTexture = MakeSoftCircleTex(160);
            rugMat.color = new Color(1.0f, 0.52f, 0.58f, 0.055f);
            rugGlow.GetComponent<MeshRenderer>().material = rugMat;
        }

        static void CreateGlossyKidTreats(Transform parent)
        {
            MakePhotorealToy(parent, PrimitiveType.Sphere, "StrawberryMacaron",
                new Vector3(-0.23f, 0.335f, -1.51f), new Vector3(0.060f, 0.020f, 0.060f),
                new Color(1.00f, 0.46f, 0.62f), 0.62f);
            MakePhotorealToy(parent, PrimitiveType.Sphere, "LemonMacaron",
                new Vector3(-0.10f, 0.342f, -1.45f), new Vector3(0.058f, 0.020f, 0.058f),
                new Color(1.00f, 0.84f, 0.32f), 0.58f);
            MakePhotorealToy(parent, PrimitiveType.Sphere, "BlueberryMacaron",
                new Vector3(0.03f, 0.335f, -1.52f), new Vector3(0.054f, 0.018f, 0.054f),
                new Color(0.42f, 0.62f, 1.00f), 0.60f);

            // Keep treats on the snack table so the SNACK vignette has a real contact target.
        }

        static GameObject MakePhotorealToy(Transform parent, PrimitiveType type, string name,
            Vector3 pos, Vector3 scale, Color color, float gloss, bool emissive = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Glossiness", gloss);
            if (emissive)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 1.6f);
            }
            go.GetComponent<MeshRenderer>().material = mat;
            return go;
        }

        GameObject BuildHeroLivingRoom()
        {
            var roomPrefab = Resources.Load<GameObject>("Models/Hero/MoonlightHeroRoom");
            if (roomPrefab == null)
            {
                Debug.LogWarning("[Moonlight] Blender hero room not found; using fallback room.");
                return null;
            }

            var root = Object.Instantiate(roomPrefab);
            root.name = "LivingRoom";
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.Euler(-90f, 180f, 0f);
            root.transform.localScale = Vector3.one;
            ApplyRoomMaterials(root);
            AddHeroFurniturePass(root.transform);

            // Fill-only practicals: contact shadows come from the single studio
            // key, avoiding three simultaneous soft-shadow lights on iPad.
            AddHeroPointLight(root.transform, "BedsideWarmth",
                new Vector3(-2.7f, 2.4f, 2.5f), new Color(1.00f, 0.76f, 0.60f), 0.16f, 4.8f);
            AddHeroPointLight(root.transform, "WindowBounce",
                new Vector3(2.8f, 3.0f, 2.8f), new Color(0.78f, 0.88f, 1.00f), 0.10f, 4.6f);

            var ambience = root.AddComponent<RoomAmbience>();
            ambience.ambientColor = new Color(0.88f, 0.78f, 0.72f);
            root.SetActive(true);
            return root;
        }

        static void AddHeroFurniturePass(Transform roomRoot)
        {
            var dressing = new GameObject("HeroFurniturePass");
            dressing.transform.SetParent(roomRoot, true);
            dressing.transform.position = roomRoot.position;
            dressing.transform.rotation = Quaternion.identity;

            AddHeroDecoration(dressing.transform, "Kenney/Furniture/bookcaseOpen",
                new Vector3(-3.85f, 0f, -0.35f), new Vector3(0f, 90f, 0f), 0.40f,
                new Color(0.72f, 0.48f, 0.34f));
            var shelfBooks = AddHeroDecoration(dressing.transform, "Kenney/Furniture/books",
                new Vector3(-3.64f, 0.42f, -0.35f), new Vector3(0f, 90f, 0f), 0.30f,
                new Color(0.92f, 0.58f, 0.72f));
            DuplicateHeroDecoration(shelfBooks, dressing.transform, "booksShelfMid",
                new Vector3(-3.64f, 1.12f, -0.35f), new Vector3(0f, 90f, 0f));
            DuplicateHeroDecoration(shelfBooks, dressing.transform, "booksShelfTop",
                new Vector3(-3.64f, 1.78f, -0.35f), new Vector3(0f, -90f, 0f));
            AddHeroDecoration(dressing.transform, "Kenney/Furniture/lampRoundFloor",
                new Vector3(3.55f, 0f, 1.88f), Vector3.zero, 0.26f,
                new Color(1.00f, 0.86f, 0.66f));
            AddHeroDecoration(dressing.transform, "Kenney/Furniture/pottedPlant",
                new Vector3(-2.88f, 0f, -0.72f), new Vector3(0f, 18f, 0f), 0.30f,
                new Color(0.54f, 0.80f, 0.62f));
        }

        static GameObject AddHeroDecoration(Transform parent, string resourcePath,
            Vector3 position, Vector3 rotation, float scale, Color tint)
        {
            var decoration = SpawnKenney(parent, resourcePath, position, rotation, scale, tint);
            if (decoration == null) return null;

            foreach (var collider in decoration.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
                Object.Destroy(collider);
            }
            foreach (var light in decoration.GetComponentsInChildren<Light>(true))
            {
                light.enabled = false;
                Object.Destroy(light);
            }
            return decoration;
        }

        static void DuplicateHeroDecoration(GameObject source, Transform parent, string name,
            Vector3 position, Vector3 rotation)
        {
            if (source == null) return;
            var copy = Object.Instantiate(source, parent);
            copy.name = name;
            copy.transform.localPosition = position;
            copy.transform.localRotation = Quaternion.Euler(rotation);
        }

        void AddSpatialActionZones(Transform roomRoot)
        {
            AddSpatialActionZone(roomRoot, "StarBallRugAction",
                MoonlightSpatialActionKind.Play, "star ball rug",
                new Vector3(0.85f, 0.04f, 1.05f), new Color(0.58f, 0.78f, 0.76f), 1.0f);
            AddSpatialActionZone(roomRoot, "BedAction",
                MoonlightSpatialActionKind.SleepCuddle, "bed nook",
                new Vector3(-2.65f, 0.04f, 1.45f), new Color(0.54f, 0.64f, 0.92f), 1.35f);
        }

        void AddHeroCollisionProxies(Transform roomRoot)
        {
            AddCollisionProxy(roomRoot, "BedCollision",
                new Vector3(-2.65f, 0.55f, 2.72f), new Vector3(2.15f, 1.10f, 1.25f));
            AddCollisionProxy(roomRoot, "KitchenTableCollision",
                new Vector3(-0.55f, 0.45f, -1.45f), new Vector3(1.45f, 0.90f, 0.72f));
            AddCollisionProxy(roomRoot, "WindowSeatCollision",
                new Vector3(3.15f, 0.45f, 2.55f), new Vector3(1.25f, 0.90f, 1.10f));
        }

        static MoonlightActivityStation AddPersistentActivityStation(Transform roomRoot,
            string name, MoonlightSpatialActionKind kind, string resourcePath,
            Vector3 worldPosition, Vector3 stageScale, Vector3 visualLocalPosition,
            Vector3 visualLocalEuler, Vector3 visualLocalScale)
        {
            var root = new GameObject(name);
            root.transform.SetParent(roomRoot, true);
            var station = root.AddComponent<MoonlightActivityStation>();
            station.Configure(kind, resourcePath, worldPosition, stageScale,
                visualLocalPosition, visualLocalEuler, visualLocalScale);
            return station;
        }

        static void AddAuthoredKitchenNook(Transform roomRoot)
        {
            var prefab = Resources.Load<GameObject>("Models/Hero/MoonKitchenNook");
            if (prefab == null)
            {
                Debug.LogError("[Moonlight] authored kitchen nook missing");
                return;
            }

            var nook = Object.Instantiate(prefab, roomRoot, true);
            nook.name = "MoonKitchenNookAuthored";
            nook.transform.position = roomRoot.position + new Vector3(0.45f, 0f, 3.92f);
            nook.transform.rotation = Quaternion.Euler(-90f, 180f, 0f);
            nook.transform.localScale = Vector3.one * 1.02f;
            ApplyHeroMaterials(nook);

            foreach (var collider in nook.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;
            foreach (var light in nook.GetComponentsInChildren<Light>(true))
                light.enabled = false;

            var renderers = nook.GetComponentsInChildren<Renderer>(true);
            Debug.Log($"[Moonlight] authored-kitchen-nook renderers={renderers.Length} " +
                "marker=MOONLIGHT_AUTHORED_KITCHEN_NOOK_READY");
            AddCollisionProxy(roomRoot, "KitchenNookCollision",
                roomRoot.position + new Vector3(0.45f, 1.55f, 3.78f),
                new Vector3(5.0f, 3.10f, 0.38f));
        }

        static void AddCollisionProxy(Transform parent, string name, Vector3 worldCenter, Vector3 size)
        {
            var proxy = new GameObject(name);
            proxy.transform.SetParent(parent, true);
            proxy.transform.position = worldCenter;
            var collider = proxy.AddComponent<BoxCollider>();
            collider.size = size;
            Debug.Log($"[MoonlightNavigationQA] blocker-ready name={name} center={worldCenter:F2} size={size:F2}");
        }

        void AddSpatialActionZone(Transform parent, string name, MoonlightSpatialActionKind kind,
            string label, Vector3 worldPosition, Color color, float radius)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.name = name;
            marker.transform.position = new Vector3(worldPosition.x, 0.014f, worldPosition.z);
            marker.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            marker.transform.SetParent(parent, true);
            marker.transform.localScale = new Vector3(radius * 1.05f, radius * 1.05f, 1f);
            Object.Destroy(marker.GetComponent<Collider>());

            var markerShader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Transparent");
            var mat = new Material(markerShader);
            mat.color = new Color(color.r, color.g, color.b, 0.20f);
            mat.mainTexture = MakeSoftCircleTex(32);
            var markerRenderer = marker.GetComponent<MeshRenderer>();
            markerRenderer.material = mat;
            markerRenderer.shadowCastingMode = ShadowCastingMode.Off;
            markerRenderer.receiveShadows = false;

            var zoneGO = new GameObject(name + "Zone");
            zoneGO.transform.position = worldPosition;
            zoneGO.transform.SetParent(parent, true);
            var zone = zoneGO.AddComponent<MoonlightSpatialActionZone>();
            zone.Configure(kind, label, radius);

            // The floor marker plus contextual HUD prompt are enough guidance.
            // World-space labels obscure the character at tablet camera distances.
        }

        static void AddHeroPointLight(Transform parent, string name, Vector3 position,
            Color color, float intensity, float range)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        GameObject BuildRoom(string roomName, Vector3 pos, Color ambColor, bool active,
            string[] propPaths = null, Vector3[] propPositions = null,
            Vector3[] propScales = null, Color[] propColors = null)
        {
            var root = new GameObject(roomName);
            root.transform.position = pos;

            Color floorColor = roomName switch
            {
                "Kitchen" => new Color(0.94f, 0.92f, 0.88f),
                "Bedroom" => new Color(0.43f, 0.37f, 0.54f),
                "Garden" => new Color(0.29f, 0.48f, 0.37f),
                "Library" => new Color(0.45f, 0.32f, 0.25f),
                _ => new Color(0.55f, 0.32f, 0.28f)
            };
            Color wallCol = roomName switch
            {
                "Kitchen" => new Color(0.93f, 0.84f, 0.78f),
                "Bedroom" => new Color(0.58f, 0.54f, 0.76f),
                "Garden" => new Color(0.48f, 0.67f, 0.56f),
                "Library" => new Color(0.66f, 0.49f, 0.38f),
                _ => new Color(0.62f, 0.52f, 0.70f)
            };

            // Floor — warm wood planks (procedural texture)
            var floor = Prim(PrimitiveType.Cube, "Floor", root.transform,
                new Vector3(0f, -0.1f, 0f), new Vector3(10f, 0.18f, 10f),
                floorColor);
            ApplyTiledTexture(floor,
                roomName == "Kitchen" ? ProcTextures.KitchenTiles() : ProcTextures.WoodPlanks(),
                new Vector2(3f, 3f));

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

            // Open-front dollhouse walls. The former front wall sat directly
            // between the follow camera and every secondary room.
            var wb = Prim(PrimitiveType.Cube, "WallBack",  root.transform, new Vector3(0f, 2.5f,  5f), new Vector3(10f, 5f, 0.2f), wallCol);
            var wr = Prim(PrimitiveType.Cube, "WallRight", root.transform, new Vector3( 5f, 2.5f, 0f), new Vector3(0.2f, 5f, 10f), wallCol);
            var wl = Prim(PrimitiveType.Cube, "WallLeft",  root.transform, new Vector3(-5f, 2.5f, 0f), new Vector3(0.2f, 5f, 10f), wallCol);
            if (roomName != "Kitchen")
            {
                var wallTex = ProcTextures.Wallpaper();
                ApplyTiledTexture(wb, wallTex, new Vector2(3f, 1.2f));
                ApplyTiledTexture(wr, wallTex, new Vector2(3f, 1.2f));
                ApplyTiledTexture(wl, wallTex, new Vector2(3f, 1.2f));
            }

            // Skirting boards
            Color skirt = roomName == "Kitchen"
                ? new Color(0.52f, 0.36f, 0.40f)
                : new Color(0.20f, 0.14f, 0.32f);
            Prim(PrimitiveType.Cube, "SkirtBack",  root.transform, new Vector3(0f, 0.12f,  4.9f), new Vector3(9.8f, 0.24f, 0.1f), skirt);
            Prim(PrimitiveType.Cube, "SkirtRight", root.transform, new Vector3( 4.9f, 0.12f, 0f), new Vector3(0.1f, 0.24f, 9.8f), skirt);
            Prim(PrimitiveType.Cube, "SkirtLeft",  root.transform, new Vector3(-4.9f, 0.12f, 0f), new Vector3(0.1f, 0.24f, 9.8f), skirt);

            // Ambient
            var amb = root.AddComponent<RoomAmbience>();
            amb.ambientColor = Color.Lerp(ambColor, Color.white, 0.55f);

            // Room point light
            var lGO = new GameObject("RoomLight");
            lGO.transform.SetParent(root.transform, false);
            lGO.transform.localPosition = new Vector3(0f, 4f, 0f);
            var rl = lGO.AddComponent<Light>();
            rl.type      = LightType.Point;
            rl.color     = Color.Lerp(ambColor, Color.white, 0.78f);
            rl.intensity = 1.65f;
            rl.range     = 12f;
            rl.shadows   = LightShadows.None;

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

            AddSecondaryRoomDetails(root.transform, roomName);

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

        void AddSecondaryRoomDetails(Transform root, string roomName)
        {
            if (roomName == "Garden")
            {
                DecorPrim(PrimitiveType.Cube, "MoonPlanter", root,
                    new Vector3(-1.65f, 0.20f, 1.65f), new Vector3(2.35f, 0.38f, 1.15f),
                    new Color(0.72f, 0.42f, 0.28f));
                DecorPrim(PrimitiveType.Cube, "MoonSoil", root,
                    new Vector3(-1.65f, 0.40f, 1.65f), new Vector3(2.10f, 0.08f, 0.92f),
                    new Color(0.25f, 0.16f, 0.13f));
                var flowerColors = new[]
                {
                    new Color(0.98f, 0.58f, 0.72f), new Color(0.95f, 0.78f, 0.34f),
                    new Color(0.60f, 0.72f, 0.98f), new Color(0.78f, 0.58f, 0.94f)
                };
                for (int i = 0; i < 8; i++)
                {
                    float x = -2.45f + (i % 4) * 0.54f;
                    float z = 1.40f + (i / 4) * 0.50f;
                    DecorPrim(PrimitiveType.Cylinder, $"FlowerStem-{i}", root,
                        new Vector3(x, 0.70f, z), new Vector3(0.035f, 0.30f, 0.035f),
                        new Color(0.28f, 0.62f, 0.32f));
                    var bloom = DecorPrim(PrimitiveType.Sphere, $"MoonBloom-{i}", root,
                        new Vector3(x, 1.04f, z), Vector3.one * 0.16f,
                        flowerColors[i % flowerColors.Length]);
                    SetMat(bloom, flowerColors[i % flowerColors.Length], flowerColors[i % flowerColors.Length] * 0.18f);
                }
                for (int i = 0; i < 6; i++)
                    DecorPrim(PrimitiveType.Cylinder, $"GardenStep-{i}", root,
                        new Vector3(-0.8f + i * 0.58f, 0.02f, -1.35f + Mathf.Sin(i * 0.8f) * 0.32f),
                        new Vector3(0.28f, 0.025f, 0.22f), new Color(0.72f, 0.76f, 0.66f));
                AddDecorGlow(root, "GardenGlow", new Vector3(-1.4f, 2.6f, 2.0f),
                    new Color(0.62f, 0.92f, 0.58f), 0.42f, 4.2f);
            }
            else if (roomName == "Library")
            {
                var rug = DecorPrim(PrimitiveType.Cylinder, "ReadingRug", root,
                    new Vector3(1.0f, 0.02f, -0.25f), new Vector3(1.55f, 0.025f, 1.55f),
                    new Color(0.48f, 0.28f, 0.55f));
                ApplyTiledTexture(rug, ProcTextures.Rug(), Vector2.one);
                DecorPrim(PrimitiveType.Cube, "ReadingDesk", root,
                    new Vector3(-0.35f, 0.52f, 1.35f), new Vector3(1.45f, 0.10f, 0.82f),
                    new Color(0.72f, 0.50f, 0.31f));
                DecorPrim(PrimitiveType.Cube, "OpenPageLeft", root,
                    new Vector3(-0.68f, 0.60f, 1.34f), new Vector3(0.58f, 0.025f, 0.64f),
                    new Color(0.96f, 0.89f, 0.72f));
                DecorPrim(PrimitiveType.Cube, "OpenPageRight", root,
                    new Vector3(-0.05f, 0.60f, 1.34f), new Vector3(0.58f, 0.025f, 0.64f),
                    new Color(0.98f, 0.92f, 0.78f));
                for (int i = 0; i < 7; i++)
                    DecorPrim(PrimitiveType.Cube, $"BookStack-{i}", root,
                        new Vector3(2.55f, 0.10f + i * 0.10f, 2.8f),
                        new Vector3(0.72f - i * 0.025f, 0.09f, 0.46f),
                        i % 2 == 0 ? new Color(0.70f, 0.38f, 0.46f) : new Color(0.36f, 0.58f, 0.68f));
                AddDecorGlow(root, "ReadingGlow", new Vector3(-0.35f, 1.55f, 1.2f),
                    new Color(1.00f, 0.72f, 0.38f), 0.48f, 3.8f);
            }
        }

        static GameObject DecorPrim(PrimitiveType type, string name, Transform parent,
            Vector3 localPosition, Vector3 localScale, Color color)
        {
            var go = Prim(type, name, parent, localPosition, localScale, color);
            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);
            return go;
        }

        static void AddDecorGlow(Transform parent, string name, Vector3 localPosition,
            Color color, float intensity, float range)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        // ── UI ───────────────────────────────────────────────────────────────
        (GameObject go, MoonlightUI ui) CreateUI()
        {
            if (PhotorealMode)
                return CreatePhotorealUI();

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

            // Compact warm HUD: readable on iPad without masking the authored room.
            var hud = Panel("HUD", canvasGO.transform,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -142f), new Vector2(0f, 0f),
                new Color(0.20f, 0.15f, 0.18f, 0.48f));
            hud.AddComponent<MoonlightSafeAreaBand>()
                .Configure(MoonlightSafeAreaBand.Edge.Top);

            // Stat sliders
            string[] names = { "Wonder", "Warmth", "Rest", "Magic", "Hunger" };
            Color[]  cols  = {
                new Color(0.45f, 0.76f, 0.82f),
                new Color(0.92f, 0.62f, 0.70f),
                new Color(0.48f, 0.76f, 0.58f),
                new Color(0.68f, 0.58f, 0.82f),
                new Color(0.94f, 0.74f, 0.38f)
            };

            string[] shortNames = { "WONDER", "WARMTH", "REST", "MAGIC", "HUNGER" };
            var sliders = new Slider[5];
            for (int i = 0; i < 5; i++)
            {
                // Normalized anchors keep all five columns inside narrow safe areas.
                var res = new DefaultControls.Resources();
                var slGO = DefaultControls.CreateSlider(res);
                slGO.name = names[i] + "Bar";
                slGO.transform.SetParent(hud.transform, false);
                var slRt = slGO.GetComponent<RectTransform>();
                slRt.anchorMin        = new Vector2(i / 5f, 1f);
                slRt.anchorMax        = new Vector2((i + 1f) / 5f, 1f);
                slRt.pivot            = new Vector2(0.5f, 0.5f);
                slRt.anchoredPosition = new Vector2(0f, -60f);
                slRt.sizeDelta        = new Vector2(-24f, 18f);
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

                // Compact labels leave the authored room visible while remaining readable on iPad.
                var statLabel = MakeTMPLabelAnchored(names[i] + "Lbl", hud.transform,
                    new Vector2(0f, -32f), new Vector2(180f, 22f),
                    shortNames[i], 18f, cols[i]);
                SetHorizontalColumn(statLabel, i);
            }

            // TMP is the visible, state-bearing HUD source. No offscreen mirror is involved.
            var stageLabel = MakeTMPLabelAnchored("StageLabel", hud.transform, new Vector2(0f, -105f), new Vector2(210f, 26f), "Moonlight / Sprout", 20f, Color.white);
            var moodLabel  = MakeTMPLabelAnchored("MoodLabel",  hud.transform, new Vector2(0f, -105f), new Vector2(110f, 26f), "HAPPY", 18f, new Color(1f, 0.72f, 0.78f));
            var coinsLabel = MakeTMPLabelAnchored("CoinsLabel", hud.transform, new Vector2(0f, -105f), new Vector2(150f, 26f), "COINS 30", 20f, new Color(1f, 0.86f, 0.42f));
            var xpLabel    = MakeTMPLabelAnchored("XPLabel",    hud.transform, new Vector2(0f, -105f), new Vector2(130f, 26f), "XP 0", 18f, new Color(0.76f, 0.68f, 0.88f));
            var daysLabel  = MakeTMPLabelAnchored("DaysLabel",  hud.transform, new Vector2(0f, -105f), new Vector2(120f, 26f), "DAY 1", 18f, new Color(0.66f, 0.82f, 0.84f));
            SetHorizontalColumn(stageLabel, 0);
            SetHorizontalColumn(moodLabel, 1);
            SetHorizontalColumn(coinsLabel, 2);
            SetHorizontalColumn(xpLabel, 3);
            SetHorizontalColumn(daysLabel, 4);

            // Action buttons (bottom) — 2×3 grid of 6 actions w/ emoji icons and softer chrome.
            var btnPanel = Panel("ActionBar", canvasGO.transform,
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, 0f), new Vector2(0f, 150f),
                new Color(0.18f, 0.14f, 0.16f, 0.40f));
            btnPanel.AddComponent<MoonlightSafeAreaBand>()
                .Configure(MoonlightSafeAreaBand.Edge.Bottom);

            var feedBtn   = MakeButton("FeedBtn",   btnPanel.transform, new Vector2(-190f, 132f), "FEED",   new Color(0.88f, 0.63f, 0.30f));
            var cuddleBtn = MakeButton("CuddleBtn", btnPanel.transform, new Vector2(   0f, 132f), "CUDDLE", new Color(0.86f, 0.50f, 0.64f));
            var sleepBtn  = MakeButton("SleepBtn",  btnPanel.transform, new Vector2( 190f, 132f), "SLEEP",  new Color(0.42f, 0.62f, 0.76f));
            feedBtn.gameObject.SetActive(false);
            cuddleBtn.gameObject.SetActive(false);
            sleepBtn.gameObject.SetActive(false);

            var contextLabel = MakeTMPLabel("ContextActionLabel", btnPanel.transform,
                new Vector2(-30f, 92f), new Vector2(620f, 34f),
                "EXPLORE THE ROOM", 22, Color.white);
            var resultLabel = MakeTMPLabel("ContextResultLabel", btnPanel.transform,
                new Vector2(-30f, 48f), new Vector2(700f, 36f),
                "", 19, new Color(1f, 0.86f, 0.58f));

            var actionBtn = MakeButton("ContextActionBtn", btnPanel.transform,
                new Vector2(485f, 74f), "ACTION", new Color(0.46f, 0.58f, 0.72f));
            actionBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(250f, 84f);
            var actionBtnLabel = actionBtn.GetComponentInChildren<TMP_Text>();
            if (actionBtnLabel == null)
                actionBtnLabel = MakeTMPButtonLabel("ActionBtnLabel", actionBtn.transform,
                    "ACTION", 24f, Color.white);
            else
            {
                actionBtnLabel.name = "ActionBtnLabel";
                actionBtnLabel.text = "ACTION";
                actionBtnLabel.fontSize = 24f;
                actionBtnLabel.fontSizeMax = 24f;
                actionBtnLabel.color = Color.white;
            }

            bool useIPadLayout = MoonlightUI.ShouldUseIPadLayout();
            var joystick = MakeJoystick(btnPanel.transform);
            joystick.gameObject.SetActive(useIPadLayout);
            RectTransform navigationCueRoot = null;
            RectTransform navigationChevron = null;
            TMP_Text navigationLabel = null;
            if (useIPadLayout)
                (navigationCueRoot, navigationChevron, navigationLabel) =
                    MakeIPadNavigationCue(btnPanel.transform);

            // Visual feedback: particle bursts on button click (finds Moonlight by name at click time)
            AttachBurst(feedBtn,   new Color(1.0f, 0.75f, 0.35f), 14);
            AttachBurst(cuddleBtn, new Color(1.0f, 0.45f, 0.70f), 20);
            AttachBurst(sleepBtn,  new Color(0.55f, 0.70f, 1.0f), 12);
            AttachBurst(actionBtn, new Color(0.70f, 0.86f, 1.0f), 18);

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
            var stgLabel = MakeTMPLabel("StgLabel", stgPanel.transform, new Vector2(0f, 0f), new Vector2(600f, 80f), "Stage Up!", 32, Color.white);

            // Room unlock overlay
            var roomPanel = Panel("RoomPanel", canvasGO.transform,
                new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.65f),
                Vector2.zero, Vector2.zero,
                new Color(0.04f, 0.08f, 0.04f, 0.92f));
            roomPanel.SetActive(false);
            var roomLabel = MakeTMPLabel("RoomLabel", roomPanel.transform, new Vector2(0f, 0f), new Vector2(600f, 80f), "Room Unlocked!", 28, Color.white);

            // Offline panel
            var offlinePanel = Panel("OfflinePanel", canvasGO.transform,
                new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.6f),
                Vector2.zero, Vector2.zero,
                new Color(0.18f, 0.08f, 0.04f, 0.92f));
            offlinePanel.SetActive(false);
            MakeTMPLabel("OfflineLbl", offlinePanel.transform, Vector2.zero, new Vector2(600f, 60f), "Moonlight missed you!", 26, new Color(1f, 0.8f, 0.5f));

            // Sleep overlay
            var sleepOvr = Panel("SleepOverlay", canvasGO.transform,
                Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero,
                new Color(0.02f, 0.02f, 0.10f, 0.75f));
            sleepOvr.SetActive(false);
            MakeTMPLabel("SleepLbl", sleepOvr.transform, Vector2.zero, new Vector2(400f, 80f), "zzzz...", 42, new Color(0.7f, 0.8f, 1f));

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
            ui.WireSpatialAction(actionBtn, actionBtnLabel, contextLabel, resultLabel);
            ui.WireIPadNavigationCue(navigationCueRoot, navigationChevron, navigationLabel,
                joystick.transform as RectTransform);
            joystick.Bind(FindAnyObjectByType<MoonlightPlayerController>());
            Debug.Log($"[MoonlightHUDQA] touch-joystick visible={joystick.gameObject.activeSelf} " +
                $"marker={(joystick.gameObject.activeSelf ? "ipad-touch-navigation" : "desktop-keyboard-navigation")}");

            // Extra UI components
            canvasGO.AddComponent<DayNightCycleUI>();
            canvasGO.AddComponent<StreakToast>();

            return (canvasGO, ui);
        }

        (GameObject go, MoonlightUI ui) CreatePhotorealUI()
        {
            var canvasGO = new GameObject("UICanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            var topSafeBand = MakeSafeAreaBand("PhotoTopSafeArea", canvasGO.transform,
                MoonlightSafeAreaBand.Edge.Top);
            var bottomSafeBand = MakeSafeAreaBand("PhotoBottomSafeArea", canvasGO.transform,
                MoonlightSafeAreaBand.Edge.Bottom);

            var hud = Panel("PhotoHUD", topSafeBand,
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -140f), new Vector2(340f, -22f),
                new Color(1f, 0.92f, 0.84f, 0.42f));

            var stageLabel = MakeTMPLabelAnchored("StageLabel", hud.transform, new Vector2(0f, -25f), new Vector2(276f, 28f), "Moonlight / Sprout", 21f, new Color(0.34f, 0.18f, 0.18f));
            var moodLabel  = MakeTMPLabelAnchored("MoodLabel",  hud.transform, new Vector2(-72f, -57f), new Vector2(124f, 24f), "HAPPY", 15f, new Color(0.72f, 0.30f, 0.46f));
            var coinsLabel = MakeTMPLabelAnchored("CoinsLabel", hud.transform, new Vector2( 72f, -57f), new Vector2(124f, 24f), "COINS 30", 15f, new Color(0.54f, 0.34f, 0.05f));
            var xpLabel    = MakeTMPLabelAnchored("XPLabel",    hud.transform, new Vector2(-72f, -88f), new Vector2(124f, 22f), "XP 0", 14f, new Color(0.38f, 0.30f, 0.52f));
            var daysLabel  = MakeTMPLabelAnchored("DaysLabel",  hud.transform, new Vector2( 72f, -88f), new Vector2(124f, 22f), "DAY 1", 14f, new Color(0.24f, 0.42f, 0.48f));

            var carePanel = Panel("CarePanel", topSafeBand,
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-386f, -388f), new Vector2(-76f, -236f),
                new Color(1f, 0.94f, 0.86f, 0.38f));

            var careHint = MakeTMPLabelAnchored("CareHint", carePanel.transform,
                new Vector2(0f, -20f), new Vector2(276f, 24f),
                "Moonlight feels cozy", 15f, new Color(0.34f, 0.18f, 0.18f));

            var sliders = new Slider[5];
            sliders[0] = MakeCareSlider(carePanel.transform, "FUN",   -48f, new Color(1.00f, 0.78f, 0.30f));
            sliders[1] = MakeCareSlider(carePanel.transform, "LOVE",  -70f, new Color(1.00f, 0.46f, 0.66f));
            sliders[2] = MakeCareSlider(carePanel.transform, "REST",  -92f, new Color(0.52f, 0.70f, 1.00f));
            sliders[3] = MakeCareSlider(carePanel.transform, "MAGIC", -114f, new Color(0.76f, 0.55f, 1.00f));
            sliders[4] = MakeCareSlider(carePanel.transform, "FOOD",  -136f, new Color(1.00f, 0.58f, 0.40f));

            var btnPanel = Panel("CandyActionBar", bottomSafeBand,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-350f, 10f), new Vector2(350f, 116f),
                new Color(1f, 0.88f, 0.78f, 0.00f));

            var feedBtn   = MakePhotorealButton("FeedBtn",   btnPanel.transform, new Vector2(-190f, 72f), "SNACK", new Color(1.00f, 0.58f, 0.42f, 0.88f));
            var cuddleBtn = MakePhotorealButton("CuddleBtn", btnPanel.transform, new Vector2(   0f, 72f), "HUG",   new Color(1.00f, 0.48f, 0.70f, 0.88f));
            var sleepBtn  = MakePhotorealButton("SleepBtn",  btnPanel.transform, new Vector2( 190f, 72f), "NAP",   new Color(0.52f, 0.70f, 1.00f, 0.88f));
            var playBtn   = MakePhotorealButton("PlayBtn",   btnPanel.transform, new Vector2(-190f, 24f), "PLAY",  new Color(1.00f, 0.78f, 0.36f, 0.88f));
            var bathBtn   = MakePhotorealButton("BathBtn",   btnPanel.transform, new Vector2(   0f, 24f), "BATH",  new Color(0.50f, 0.88f, 0.94f, 0.88f));
            var danceBtn  = MakePhotorealButton("DanceBtn",  btnPanel.transform, new Vector2( 190f, 24f), "DANCE", new Color(0.78f, 0.56f, 1.00f, 0.88f));

            AttachBurst(feedBtn,   new Color(1.0f, 0.72f, 0.40f), 18);
            AttachBurst(cuddleBtn, new Color(1.0f, 0.42f, 0.66f), 24);
            AttachBurst(sleepBtn,  new Color(0.54f, 0.72f, 1.0f), 14);
            AttachBurst(playBtn,   new Color(1.0f, 0.84f, 0.35f), 22);
            AttachBurst(bathBtn,   new Color(0.58f, 0.92f, 1.0f), 18);
            AttachBurst(danceBtn,  new Color(0.86f, 0.56f, 1.0f), 28);

            var moonCookie = ScriptableObject.CreateInstance<FoodItem>();
            moonCookie.itemName = "Moon Cookie";
            moonCookie.cost = 1;
            moonCookie.hungerBoost = 72f;
            moonCookie.warmthBoost = 10f;
            moonCookie.wonderBoost = 6f;
            moonCookie.magicBoost = 6f;
            moonCookie.xpReward = 8;
            moonCookie.hideFlags = HideFlags.HideAndDontSave;

            feedBtn.onClick.AddListener(() =>
            {
                TriggerKidAction("Snack");
                MoonlightGameManager.Instance?.moonlight?.Feed(moonCookie);
                var ui = Object.FindAnyObjectByType<MoonlightUI>();
                if (ui != null && MoonlightGameManager.Instance?.moonlight != null) ui.Refresh(MoonlightGameManager.Instance.moonlight);
            });
            cuddleBtn.onClick.AddListener(() => TriggerKidAction("Hug"));
            sleepBtn.onClick.AddListener(() => TriggerKidAction("Nap"));
            playBtn.onClick.AddListener(() =>
            {
                TriggerKidAction("Play");
                MoonlightGameManager.Instance?.moonlight?.Play();
                var ui = Object.FindAnyObjectByType<MoonlightUI>();
                if (ui != null && MoonlightGameManager.Instance?.moonlight != null) ui.Refresh(MoonlightGameManager.Instance.moonlight);
            });
            bathBtn.onClick.AddListener(() =>
            {
                TriggerKidAction("Bath");
                MoonlightGameManager.Instance?.moonlight?.Bathe();
                var ui = Object.FindAnyObjectByType<MoonlightUI>();
                if (ui != null && MoonlightGameManager.Instance?.moonlight != null) ui.Refresh(MoonlightGameManager.Instance.moonlight);
            });
            danceBtn.onClick.AddListener(() =>
            {
                TriggerKidAction("Dance");
                MoonlightGameManager.Instance?.moonlight?.Dance();
                var ui = Object.FindAnyObjectByType<MoonlightUI>();
                if (ui != null && MoonlightGameManager.Instance?.moonlight != null) ui.Refresh(MoonlightGameManager.Instance.moonlight);
            });

            var feedMenu = Panel("FeedMenu", canvasGO.transform,
                new Vector2(0.18f, 0.18f), new Vector2(0.82f, 0.82f),
                Vector2.zero, Vector2.zero,
                new Color(1f, 0.90f, 0.84f, 0.94f));
            feedMenu.SetActive(false);
            var closeBtn = MakePhotorealButton("CloseBtn", feedMenu.transform, new Vector2(0f, -210f), "Close", new Color(0.70f, 0.36f, 0.38f, 0.94f));
            closeBtn.onClick.AddListener(() => feedMenu.SetActive(false));

            var contentGO = new GameObject("FeedContent");
            contentGO.transform.SetParent(feedMenu.transform, false);
            contentGO.AddComponent<RectTransform>().anchoredPosition = Vector2.zero;

            var stgPanel = Panel("StagePanel", canvasGO.transform,
                new Vector2(0.22f, 0.34f), new Vector2(0.78f, 0.66f),
                Vector2.zero, Vector2.zero,
                new Color(1f, 0.78f, 0.90f, 0.92f));
            stgPanel.SetActive(false);
            var stgLabel = MakeTMPLabel("StgLabel", stgPanel.transform, new Vector2(0f, -76f), new Vector2(760f, 90f), "Moonlight shines brighter!", 34f, new Color(0.38f, 0.18f, 0.34f));
            stgLabel.fontStyle = FontStyles.Bold;

            var roomPanel = Panel("RoomPanel", canvasGO.transform,
                new Vector2(0.24f, 0.36f), new Vector2(0.76f, 0.64f),
                Vector2.zero, Vector2.zero,
                new Color(0.86f, 0.96f, 1f, 0.92f));
            roomPanel.SetActive(false);
            var roomLabel = MakeTMPLabel("RoomLabel", roomPanel.transform, new Vector2(0f, -70f), new Vector2(760f, 80f), "A new room is glowing!", 30f, new Color(0.18f, 0.30f, 0.42f));
            roomLabel.fontStyle = FontStyles.Bold;

            var offlinePanel = Panel("OfflinePanel", canvasGO.transform,
                new Vector2(0.26f, 0.42f), new Vector2(0.74f, 0.58f),
                Vector2.zero, Vector2.zero,
                new Color(1f, 0.88f, 0.72f, 0.92f));
            offlinePanel.SetActive(false);
            var offlineLabel = MakeTMPLabel("OfflineLbl", offlinePanel.transform, new Vector2(0f, -58f), new Vector2(720f, 60f), "Moonlight missed you!", 28f, new Color(0.42f, 0.22f, 0.14f));
            offlineLabel.fontStyle = FontStyles.Bold;

            var sleepOvr = Panel("SleepOverlay", canvasGO.transform,
                Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero,
                new Color(0.04f, 0.03f, 0.08f, 0.50f));
            sleepOvr.SetActive(false);
            var sleepLabel = MakeTMPLabel("SleepLbl", sleepOvr.transform, new Vector2(0f, -140f), new Vector2(460f, 80f), "Sweet dreams", 42f, new Color(1f, 0.90f, 0.70f));
            sleepLabel.fontStyle = FontStyles.Bold;

            var quitBtn = MakePhotorealButton("QuitBtn", topSafeBand, Vector2.zero, "X", new Color(0.54f, 0.20f, 0.24f, 0.82f));
            var quitRt = quitBtn.GetComponent<RectTransform>();
            quitRt.anchorMin = new Vector2(1f, 1f);
            quitRt.anchorMax = new Vector2(1f, 1f);
            quitRt.pivot = new Vector2(1f, 1f);
            quitRt.anchoredPosition = new Vector2(-20f, -20f);
            quitRt.sizeDelta = new Vector2(54f, 54f);
            quitBtn.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            if (!FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>())
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            var ui = canvasGO.AddComponent<MoonlightUI>();
            ui.feedOpensMenu = false;
            ui.Wire(
                sliders[0], sliders[1], sliders[2], sliders[3], sliders[4],
                stageLabel, coinsLabel, xpLabel, moodLabel, daysLabel,
                feedBtn, cuddleBtn, sleepBtn,
                stgPanel, stgLabel,
                roomPanel, roomLabel,
                offlinePanel, sleepOvr,
                feedMenu, contentGO.transform);
            ui.promptRoot = carePanel;
            ui.promptLabel = careHint;
            ui.keepPromptVisible = true;

            return (canvasGO, ui);
        }

        GameObject CreateRoomNavigation(Transform canvasRoot, RoomManager rooms)
        {
            var nav = Panel("RoomNavigation", canvasRoot,
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-900f, -214f), new Vector2(-20f, -154f),
                new Color(0.18f, 0.14f, 0.16f, 0.42f));

            var types = new[]
            {
                RoomType.LivingRoom, RoomType.Kitchen, RoomType.Bedroom,
                RoomType.Garden, RoomType.Library
            };
            var labels = new[] { "LIVING", "KITCHEN", "BEDROOM", "GARDEN", "LIBRARY" };
            var colors = new[]
            {
                new Color(0.62f, 0.46f, 0.68f), new Color(0.80f, 0.52f, 0.28f),
                new Color(0.42f, 0.55f, 0.76f), new Color(0.36f, 0.64f, 0.46f),
                new Color(0.58f, 0.42f, 0.30f)
            };
            for (int i = 0; i < types.Length; i++)
            {
                var room = types[i];
                var button = MakeButton($"{room}Nav", nav.transform,
                    new Vector2(-352f + i * 176f, 0f), labels[i], colors[i]);
                button.GetComponent<RectTransform>().sizeDelta = new Vector2(158f, 44f);
                button.onClick.AddListener(() => rooms.GoToRoom(room));
            }
            return nav;
        }

        // ── UI Helpers ───────────────────────────────────────────────────────
        static RectTransform MakeSafeAreaBand(string name, Transform parent,
            MoonlightSafeAreaBand.Edge edge)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            float edgeAnchor = edge == MoonlightSafeAreaBand.Edge.Top ? 1f : 0f;
            rect.anchorMin = new Vector2(0f, edgeAnchor);
            rect.anchorMax = new Vector2(1f, edgeAnchor);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.AddComponent<MoonlightSafeAreaBand>().Configure(edge);
            return rect;
        }

        static Slider MakeHiddenSlider(Transform parent, int index)
        {
            var res = new DefaultControls.Resources();
            var go = DefaultControls.CreateSlider(res);
            go.name = $"HiddenStat{index}";
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.anchoredPosition = new Vector2(-9999f, -9999f);
            rt.sizeDelta = new Vector2(1f, 1f);
            var slider = go.GetComponent<Slider>();
            slider.value = 0.8f;
            slider.interactable = false;
            return slider;
        }

        static Slider MakeCareSlider(Transform parent, string label, float y, Color fillColor)
        {
            var row = new GameObject($"{label}Row");
            row.transform.SetParent(parent, false);
            var rowRt = row.AddComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0f, 1f);
            rowRt.anchorMax = new Vector2(1f, 1f);
            rowRt.pivot = new Vector2(0f, 1f);
            rowRt.offsetMin = new Vector2(18f, y - 18f);
            rowRt.offsetMax = new Vector2(-18f, y);

            var labelGO = new GameObject($"{label}Label");
            labelGO.transform.SetParent(row.transform, false);
            var text = labelGO.AddComponent<TextMeshProUGUI>();
            MoonlightUI.EnsureRuntimeFont(text);
            text.text = label;
            text.fontSize = 11f;
            text.fontStyle = FontStyles.Bold;
            text.color = new Color(0.30f, 0.16f, 0.14f);
            text.alignment = TextAlignmentOptions.Left;
            text.enableAutoSizing = true;
            text.fontSizeMin = 9f;
            text.fontSizeMax = 11f;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.characterSpacing = 0f;
            text.raycastTarget = false;
            var labelRt = labelGO.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0f, 0f);
            labelRt.anchorMax = new Vector2(0f, 1f);
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = new Vector2(58f, 0f);

            var res = new DefaultControls.Resources();
            var go = DefaultControls.CreateSlider(res);
            go.name = $"{label}CareBar";
            go.transform.SetParent(row.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(64f, 2f);
            rt.offsetMax = new Vector2(0f, -2f);

            var slider = go.GetComponent<Slider>();
            slider.value = 0.8f;
            slider.interactable = false;

            var background = go.transform.Find("Background")?.GetComponent<Image>();
            if (background != null) background.color = new Color(0.22f, 0.16f, 0.14f, 0.26f);

            var fill = go.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
            if (fill != null) fill.color = fillColor;

            var handle = go.transform.Find("Handle Slide Area/Handle");
            if (handle != null) handle.gameObject.SetActive(false);

            return slider;
        }

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

        static TMP_Text MakeTMPLabelAnchored(string name, Transform parent,
            Vector2 anchoredPos, Vector2 size, string text,
            float fontSize, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            MoonlightUI.EnsureRuntimeFont(t);
            t.text      = text;
            t.fontSize  = fontSize;
            t.fontStyle = FontStyles.Bold;
            t.color     = color;
            t.alignment = TextAlignmentOptions.Center;
            t.enableAutoSizing = true;
            t.fontSizeMin = Mathf.Max(10f, fontSize * 0.72f);
            t.fontSizeMax = fontSize;
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Ellipsis;
            t.characterSpacing = 0f;
            t.raycastTarget = false;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.08f, 0.06f, 0.08f, 0.58f);
            shadow.effectDistance = new Vector2(1f, -1f);
            shadow.useGraphicAlpha = true;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 1f);
            rt.anchorMax        = new Vector2(0.5f, 1f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;
            return t;
        }

        static void SetHorizontalColumn(TMP_Text label, int column)
        {
            var rect = label != null ? label.transform as RectTransform : null;
            if (rect == null) return;
            float minimum = Mathf.Clamp(column, 0, 4) / 5f;
            float maximum = (Mathf.Clamp(column, 0, 4) + 1f) / 5f;
            rect.anchorMin = new Vector2(minimum, 1f);
            rect.anchorMax = new Vector2(maximum, 1f);
            rect.anchoredPosition = new Vector2(0f, rect.anchoredPosition.y);
            rect.sizeDelta = new Vector2(-24f, rect.sizeDelta.y);
        }

        static TMP_Text MakeTMPLabel(string name, Transform parent,
            Vector2 anchoredPos, Vector2 size, string text,
            float fontSize, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            MoonlightUI.EnsureRuntimeFont(t);
            t.text      = text;
            t.fontSize  = fontSize;
            t.color     = color;
            t.alignment = TextAlignmentOptions.Center;
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;
            return t;
        }

        static TMP_Text MakeTMPButtonLabel(string name, Transform parent, string text,
            float fontSize, Color color)
        {
            var label = MakeTMPLabel(name, parent, Vector2.zero, Vector2.zero, text, fontSize, color);
            label.fontStyle = FontStyles.Bold;
            var rt = label.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return label;
        }

        static MoonlightTouchJoystick MakeJoystick(Transform parent)
        {
            var root = new GameObject("MoveJoystick");
            root.transform.SetParent(parent, false);
            var image = root.AddComponent<Image>();
            image.sprite = UICircleSprite;
            image.color = new Color(0.12f, 0.12f, 0.14f, 0.42f);
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(148f, 136f);
            rt.sizeDelta = new Vector2(168f, 168f);

            var knob = new GameObject("Knob");
            knob.transform.SetParent(root.transform, false);
            var knobImage = knob.AddComponent<Image>();
            knobImage.sprite = UICircleSprite;
            knobImage.color = new Color(0.78f, 0.84f, 0.88f, 0.78f);
            var knobRt = knob.GetComponent<RectTransform>();
            knobRt.anchorMin = new Vector2(0.5f, 0.5f);
            knobRt.anchorMax = new Vector2(0.5f, 0.5f);
            knobRt.pivot = new Vector2(0.5f, 0.5f);
            knobRt.anchoredPosition = Vector2.zero;
            knobRt.sizeDelta = new Vector2(72f, 72f);

            var label = MakeTMPLabel("MoveLabel", root.transform,
                new Vector2(0f, -108f), new Vector2(180f, 26f),
                "MOVE", 16, new Color(1f, 1f, 1f, 0.72f));
            label.fontStyle = FontStyles.Bold;

            var joystick = root.AddComponent<MoonlightTouchJoystick>();
            return joystick;
        }

        static (RectTransform root, RectTransform chevron, TMP_Text label)
            MakeIPadNavigationCue(Transform parent)
        {
            var rootObject = new GameObject("IPadNavigationCue", typeof(RectTransform),
                typeof(CanvasGroup));
            rootObject.transform.SetParent(parent, false);
            var root = rootObject.GetComponent<RectTransform>();
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.zero;
            root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = new Vector2(310f, 112f);
            root.sizeDelta = new Vector2(128f, 104f);
            var group = rootObject.GetComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            var chevronObject = new GameObject("DirectionChevron", typeof(RectTransform));
            chevronObject.transform.SetParent(root, false);
            var chevron = chevronObject.GetComponent<RectTransform>();
            chevron.anchorMin = new Vector2(0.5f, 0.5f);
            chevron.anchorMax = new Vector2(0.5f, 0.5f);
            chevron.pivot = new Vector2(0.5f, 0.5f);
            chevron.anchoredPosition = new Vector2(0f, 25f);
            chevron.sizeDelta = new Vector2(48f, 36f);

            MakeChevronStroke("LeftStroke", chevron, new Vector2(-8f, 1f), 40f);
            MakeChevronStroke("RightStroke", chevron, new Vector2(8f, 1f), -40f);

            var label = MakeTMPLabel("NavigationTargetLabel", root,
                new Vector2(0f, -23f), new Vector2(128f, 48f), "", 18f, Color.white);
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = true;
            label.fontSizeMin = 13f;
            label.fontSizeMax = 18f;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.characterSpacing = 0f;
            label.raycastTarget = false;
            var shadow = label.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.03f, 0.04f, 0.06f, 0.92f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            shadow.useGraphicAlpha = true;

            rootObject.SetActive(false);
            return (root, chevron, label);
        }

        static void MakeChevronStroke(string name, Transform parent, Vector2 position, float angle)
        {
            var strokeObject = new GameObject(name);
            strokeObject.transform.SetParent(parent, false);
            var image = strokeObject.AddComponent<Image>();
            image.color = new Color(0.42f, 0.86f, 1f, 0.96f);
            image.raycastTarget = false;
            var rect = strokeObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(26f, 6f);
            rect.localRotation = Quaternion.Euler(0f, 0f, angle);
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

        static void TriggerKidAction(string action)
        {
            var director = MoonlightSceneDirector.Instance ?? Object.FindAnyObjectByType<MoonlightSceneDirector>();
            if (director != null && director.PlayAction(action)) return;

            var target = GameObject.Find("Moonlight");
            var kid = target != null ? target.GetComponentInChildren<MoonlightKidAnimator>() : null;
            if (kid != null) kid.Play(action);
        }

        static Button MakeButton(string name, Transform parent, Vector2 pos, string label, Color tint)
        {
            var res   = new DefaultControls.Resources();
            var btnGO = DefaultControls.CreateButton(res);
            btnGO.name = name;
            btnGO.transform.SetParent(parent, false);
            var rt = btnGO.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta        = new Vector2(168f, 56f);
            var img = btnGO.GetComponent<Image>();
            if (img) img.color = tint;
            var lbl = btnGO.GetComponentInChildren<Text>();
            if (lbl)
                lbl.enabled = false;
            var tmpLabel = MakeTMPButtonLabel("LabelTMP", btnGO.transform, label, 18f, Color.white);
            tmpLabel.enableAutoSizing = true;
            tmpLabel.fontSizeMin = 14f;
            tmpLabel.fontSizeMax = 18f;
            tmpLabel.enableWordWrapping = false;
            tmpLabel.overflowMode = TextOverflowModes.Ellipsis;
            tmpLabel.characterSpacing = 0f;
            tmpLabel.raycastTarget = false;
            return btnGO.GetComponent<Button>();
        }

        static Button MakePhotorealButton(string name, Transform parent, Vector2 pos, string label, Color tint)
        {
            var res = new DefaultControls.Resources();
            var btnGO = DefaultControls.CreateButton(res);
            btnGO.name = name;
            btnGO.transform.SetParent(parent, false);
            var rt = btnGO.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(136f, 38f);
            var img = btnGO.GetComponent<Image>();
            if (img) img.color = tint;
            var lbl = btnGO.GetComponentInChildren<Text>();
            if (lbl)
                lbl.enabled = false;
            var tmpLabel = MakeTMPButtonLabel("LabelTMP", btnGO.transform, label,
                label.Length <= 3 ? 21f : 18f, new Color(0.26f, 0.12f, 0.10f));
            tmpLabel.enableAutoSizing = true;
            tmpLabel.fontSizeMin = 14f;
            tmpLabel.fontSizeMax = label.Length <= 3 ? 21f : 18f;
            tmpLabel.enableWordWrapping = false;
            tmpLabel.overflowMode = TextOverflowModes.Ellipsis;
            tmpLabel.characterSpacing = 0f;
            tmpLabel.raycastTarget = false;
            var colors = btnGO.GetComponent<Button>().colors;
            colors.highlightedColor = Color.Lerp(tint, Color.white, 0.35f);
            colors.pressedColor = Color.Lerp(tint, new Color(0.75f, 0.45f, 0.40f), 0.30f);
            btnGO.GetComponent<Button>().colors = colors;
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

        static Shader _transparentSpriteShader;
        static Shader TransparentSpriteShader =>
            _transparentSpriteShader ??= Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Texture") ?? Shader.Find("Standard");

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

        static void ApplyHeroMaterials(GameObject root)
        {
            var standard = Shader.Find("Standard") ?? Shader.Find("Diffuse") ?? ToonShader;
            var materialCache = new System.Collections.Generic.Dictionary<string, Material>();
            var surfaceProfiles = new System.Collections.Generic.HashSet<string>();
            int rendererCount = 0;
            int shadowRendererCount = 0;
            int emissiveMaterialCount = 0;
            int texturedMaterialCount = 0;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                rendererCount++;
                var sourceMaterials = renderer.sharedMaterials;
                var authoredMaterials = new Material[sourceMaterials.Length];
                for (int i = 0; i < sourceMaterials.Length; i++)
                {
                    var source = sourceMaterials[i];
                    string materialName = source != null ? source.name : renderer.gameObject.name;
                    string lower = materialName.ToLowerInvariant();
                    if (materialCache.TryGetValue(lower, out Material cached))
                    {
                        authoredMaterials[i] = cached;
                        continue;
                    }
                    var color = source != null && source.HasProperty("_Color")
                        ? source.color
                        : HeroPaletteColor(materialName);

                    // Some FBX import paths return white even though the authored
                    // Blender material name carries the intended palette.
                    var namedColor = HeroPaletteColor(materialName);
                    if (color.r > 0.97f && color.g > 0.97f && color.b > 0.97f &&
                        namedColor != Color.white)
                        color = namedColor;

                    var material = new Material(standard) { name = materialName + "_Runtime" };
                    if (material.HasProperty("_Color")) material.SetColor("_Color", color);
                    if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
                    float smoothness = HeroSmoothness(lower);
                    float metallic = HeroMetallic(lower);
                    float emission = HeroEmission(lower);
                    if (material.HasProperty("_Glossiness"))
                        material.SetFloat("_Glossiness", smoothness);
                    if (material.HasProperty("_Smoothness"))
                        material.SetFloat("_Smoothness", smoothness);
                    if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
                    if (material.HasProperty("_SpecularHighlights"))
                        material.SetFloat("_SpecularHighlights", 1f);
                    if (material.HasProperty("_EmissionColor"))
                    {
                        material.SetColor("_EmissionColor", color * emission);
                        if (emission > 0f)
                        {
                            material.EnableKeyword("_EMISSION");
                            emissiveMaterialCount++;
                        }
                    }
                    if (source != null && source.mainTexture != null)
                    {
                        material.mainTexture = source.mainTexture;
                        texturedMaterialCount++;
                    }
                    materialCache.Add(lower, material);
                    surfaceProfiles.Add(HeroSurfaceProfile(lower));
                    authoredMaterials[i] = material;
                }

                renderer.sharedMaterials = authoredMaterials;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
                shadowRendererCount++;
            }

            var quality = root.GetComponent<MoonlightHeroVisualQuality>() ??
                root.AddComponent<MoonlightHeroVisualQuality>();
            quality.Configure(rendererCount, shadowRendererCount, materialCache.Count,
                surfaceProfiles.Count, emissiveMaterialCount, texturedMaterialCount);
            if (HasHeroEyeGeometry(root))
            {
                var eyeQuality = root.GetComponent<MoonlightHeroEyeQuality>() ??
                    root.AddComponent<MoonlightHeroEyeQuality>();
                eyeQuality.Configure(root);
            }
        }

        static void ApplyRoomMaterials(GameObject root)
        {
            var sourceMaterialKeys = new System.Collections.Generic.HashSet<string>();
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            foreach (Material sourceMaterial in renderer.sharedMaterials)
            {
                if (sourceMaterial != null)
                    sourceMaterialKeys.Add(NormalizeRoomMaterialName(sourceMaterial.name));
            }
            int semanticMaterialCount = 0;
            for (int i = 0; i < RequiredRoomSemanticMaterials.Length; i++)
            {
                string materialName = RequiredRoomSemanticMaterials[i];
                if (!sourceMaterialKeys.Contains(materialName.ToLowerInvariant())) continue;
                semanticMaterialCount++;
            }

            // Preserve the existing one-clone-per-source allocation, then tune those
            // authored instances in place. The room profile pass allocates no materials.
            ApplyHeroMaterials(root);
            var runtimeMaterials = new System.Collections.Generic.HashSet<Material>();
            var runtimeSemanticProfileMatches =
                new System.Collections.Generic.HashSet<string>();
            var surfaceProfiles = new System.Collections.Generic.HashSet<string>();
            int rendererCount = 0;
            int shadowCasterCount = 0;
            int shadowReceiverCount = 0;
            int emissiveMaterialCount = 0;
            int texturedMaterialCount = 0;

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                rendererCount++;
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material == null) continue;
                    string materialName = NormalizeRoomMaterialName(material.name);
                    bool firstRuntimeUse = runtimeMaterials.Add(material);

                    RoomSurfaceProfile profile = RoomSurfaceProfileFor(materialName);
                    if (firstRuntimeUse)
                    {
                        for (int semanticIndex = 0;
                             semanticIndex < RequiredRoomSemanticMaterials.Length;
                             semanticIndex++)
                        {
                            if (materialName == RequiredRoomSemanticMaterials[semanticIndex].ToLowerInvariant() &&
                                profile.Id == RequiredRoomSurfaceProfiles[semanticIndex])
                                runtimeSemanticProfileMatches.Add(materialName);
                        }
                    }
                    if (material.HasProperty("_Glossiness"))
                        material.SetFloat("_Glossiness", profile.Smoothness);
                    if (material.HasProperty("_Smoothness"))
                        material.SetFloat("_Smoothness", profile.Smoothness);
                    if (material.HasProperty("_Metallic"))
                        material.SetFloat("_Metallic", profile.Metallic);
                    if (material.HasProperty("_SpecularHighlights"))
                        material.SetFloat("_SpecularHighlights", 1f);
                    if (material.HasProperty("_EmissionColor"))
                    {
                        Color color = material.HasProperty("_Color")
                            ? material.GetColor("_Color")
                            : HeroPaletteColor(materialName);
                        material.SetColor("_EmissionColor", color * profile.Emission);
                        if (profile.Emission > 0f)
                        {
                            material.EnableKeyword("_EMISSION");
                            if (firstRuntimeUse) emissiveMaterialCount++;
                        }
                        else material.DisableKeyword("_EMISSION");
                    }
                    if (firstRuntimeUse && material.mainTexture != null) texturedMaterialCount++;
                    surfaceProfiles.Add(profile.Id);
                }

                if (renderer.shadowCastingMode != ShadowCastingMode.Off) shadowCasterCount++;
                if (renderer.receiveShadows) shadowReceiverCount++;
            }

            var quality = root.GetComponent<MoonlightRoomVisualQuality>() ??
                root.AddComponent<MoonlightRoomVisualQuality>();
            quality.Configure(rendererCount, shadowCasterCount, shadowReceiverCount,
                sourceMaterialKeys.Count, runtimeMaterials.Count, semanticMaterialCount,
                runtimeSemanticProfileMatches.Count, surfaceProfiles.Count, emissiveMaterialCount,
                texturedMaterialCount);
        }

        static string NormalizeRoomMaterialName(string materialName)
        {
            string normalized = materialName ?? string.Empty;
            const string runtimeSuffix = "_Runtime";
            const string instanceSuffix = " (Instance)";
            if (normalized.EndsWith(instanceSuffix, System.StringComparison.Ordinal))
                normalized = normalized.Substring(0, normalized.Length - instanceSuffix.Length);
            if (normalized.EndsWith(runtimeSuffix, System.StringComparison.Ordinal))
                normalized = normalized.Substring(0, normalized.Length - runtimeSuffix.Length);
            return normalized.ToLowerInvariant();
        }

        static RoomSurfaceProfile RoomSurfaceProfileFor(string materialName)
        {
            string n = (materialName ?? string.Empty).ToLowerInvariant();
            if (n.Contains("trim") || n.Contains("baseboard") || n.Contains("chairrail"))
                return RoomSurfaceProfiles[1];
            if (n.Contains("floor")) return RoomSurfaceProfiles[2];
            if (n.Contains("wood") || n.Contains("maple")) return RoomSurfaceProfiles[3];
            if (n.Contains("bed") || n.Contains("blanket") || n.Contains("pillow") ||
                n.Contains("textile"))
                return RoomSurfaceProfiles[4];
            if (n.Contains("mint") || n.Contains("foliage") || n.Contains("leaf") ||
                n.Contains("plant"))
                return RoomSurfaceProfiles[5];
            if (n.Contains("sky") || n.Contains("glass") || n.Contains("window"))
                return RoomSurfaceProfiles[6];
            if (n.Contains("moon") || n.Contains("glow") || n.Contains("celestial") ||
                n.Contains("highlight"))
                return RoomSurfaceProfiles[7];
            return RoomSurfaceProfiles[0];
        }

        public static bool ValidateRoomSurfaceProfileContract(out string detail)
        {
            var ids = new System.Collections.Generic.HashSet<string>();
            int mapped = 0;
            int emissive = 0;
            float minimumSmoothness = 1f;
            float maximumSmoothness = 0f;
            float maximumMetallic = 0f;
            bool bounded = true;

            foreach (RoomSurfaceProfile profile in RoomSurfaceProfiles)
            {
                ids.Add(profile.Id);
                minimumSmoothness = Mathf.Min(minimumSmoothness, profile.Smoothness);
                maximumSmoothness = Mathf.Max(maximumSmoothness, profile.Smoothness);
                maximumMetallic = Mathf.Max(maximumMetallic, profile.Metallic);
                if (profile.Emission > 0f) emissive++;
                bounded &= profile.Smoothness >= 0.08f && profile.Smoothness <= 0.75f &&
                    profile.Metallic >= 0f && profile.Metallic <= 0.05f &&
                    profile.Emission >= 0f && profile.Emission <= 0.25f;
            }
            for (int i = 0; i < RequiredRoomSemanticMaterials.Length; i++)
            {
                if (RoomSurfaceProfileFor(RequiredRoomSemanticMaterials[i]).Id ==
                    RequiredRoomSurfaceProfiles[i])
                    mapped++;
            }

            detail = $"profiles={ids.Count}/{RoomSurfaceProfileMinimum} " +
                $"semanticMap={mapped}/{RequiredRoomSemanticMaterials.Length} " +
                $"smooth={minimumSmoothness:0.00}-{maximumSmoothness:0.00} " +
                $"metallicMax={maximumMetallic:0.00} emissiveProfiles={emissive}";
            return RoomSurfaceProfiles.Length >= RoomSurfaceProfileMinimum &&
                ids.Count == RoomSurfaceProfiles.Length &&
                mapped == RequiredRoomSemanticMaterials.Length &&
                bounded && emissive >= 1;
        }

        static bool HasHeroEyeGeometry(GameObject root)
        {
            if (root == null) return false;
            int eyeCount = 0;
            int highlightCount = 0;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                string lower = renderer.gameObject.name.ToLowerInvariant();
                if (lower.Contains("eyehighlight")) highlightCount++;
                else if (lower.Contains("eyeleft") || lower.Contains("eyeright")) eyeCount++;
            }
            return eyeCount == 2 && highlightCount == 2;
        }

        static Color HeroPaletteColor(string materialName)
        {
            string n = (materialName ?? string.Empty).ToLowerInvariant();
            if (n.Contains("room_cream")) return new Color(0.93f, 0.84f, 0.72f);
            if (n.Contains("wall_peach")) return new Color(0.89f, 0.70f, 0.61f);
            if (n.Contains("trim_ivory")) return new Color(0.98f, 0.94f, 0.84f);
            if (n.Contains("floor_honey")) return new Color(0.76f, 0.57f, 0.38f);
            if (n.Contains("wood_maple")) return new Color(0.62f, 0.40f, 0.27f);
            if (n.Contains("bed_biscuit")) return new Color(0.88f, 0.68f, 0.43f);
            if (n.Contains("blanket_blush")) return new Color(0.95f, 0.63f, 0.65f);
            if (n.Contains("pillow_cream")) return new Color(0.98f, 0.95f, 0.87f);
            if (n.Contains("mint_soft")) return new Color(0.55f, 0.78f, 0.69f);
            if (n.Contains("mint_deep")) return new Color(0.25f, 0.59f, 0.55f);
            if (n.Contains("sky_dusk")) return new Color(0.34f, 0.45f, 0.65f);
            if (n.Contains("moon_butter")) return new Color(0.98f, 0.86f, 0.48f);
            if (n.Contains("lilac_body")) return new Color(0.66f, 0.57f, 0.82f);
            if (n.Contains("lilac_shadow")) return new Color(0.45f, 0.34f, 0.62f);
            if (n.Contains("lilac_innerear")) return new Color(0.83f, 0.61f, 0.76f);
            if (n.Contains("cat_belly")) return new Color(0.94f, 0.88f, 0.78f);
            if (n.Contains("cat_eye")) return new Color(0.23f, 0.16f, 0.31f);
            if (n.Contains("cat_highlight")) return new Color(1.00f, 0.98f, 0.91f);
            if (n.Contains("cat_nose")) return new Color(0.67f, 0.40f, 0.55f);
            if (n.Contains("pencil_coral")) return new Color(0.89f, 0.35f, 0.35f);
            if (n.Contains("pencil_gold")) return new Color(0.95f, 0.70f, 0.25f);
            if (n.Contains("pencil_blue")) return new Color(0.27f, 0.50f, 0.70f);
            return Color.white;
        }

        static float HeroSmoothness(string materialName)
        {
            string n = materialName ?? string.Empty;
            if (n.Contains("highlight")) return 0.94f;
            if (n.Contains("eye")) return 0.88f;
            if (n.Contains("moon")) return 0.58f;
            if (n.Contains("innerear") || n.Contains("nose")) return 0.34f;
            if (n.Contains("sky")) return 0.55f;
            if (n.Contains("trim") || n.Contains("pencil")) return 0.34f;
            if (n.Contains("lilac_body") || n.Contains("lilac_shadow")) return 0.22f;
            if (n.Contains("cat_belly")) return 0.16f;
            if (n.Contains("blanket") || n.Contains("pillow") || n.Contains("bed_biscuit")) return 0.16f;
            if (n.Contains("wall") || n.Contains("floor")) return 0.18f;
            if (n.Contains("wood")) return 0.24f;
            return 0.30f;
        }

        static float HeroMetallic(string materialName)
        {
            string n = materialName ?? string.Empty;
            if (n.Contains("eye") || n.Contains("highlight")) return 0.04f;
            if (n.Contains("moon") || n.Contains("pencil")) return 0.02f;
            return 0f;
        }

        static float HeroEmission(string materialName)
        {
            string n = materialName ?? string.Empty;
            if (n.Contains("highlight")) return 0.16f;
            if (n.Contains("moon")) return 0.13f;
            if (n.Contains("eye")) return 0.04f;
            return 0f;
        }

        static string HeroSurfaceProfile(string materialName)
        {
            string n = materialName ?? string.Empty;
            if (n.Contains("highlight")) return "highlight-gloss";
            if (n.Contains("eye")) return "eye-glass";
            if (n.Contains("moon")) return "moon-glow";
            if (n.Contains("innerear") || n.Contains("nose")) return "soft-detail";
            if (n.Contains("belly")) return "belly-matte";
            if (n.Contains("lilac")) return "body-velvet";
            return "environment";
        }

        public static bool ValidateHeroSurfaceContract(out string detail)
        {
            float body = HeroSmoothness("Lilac_Body");
            float belly = HeroSmoothness("Cat_Belly");
            float eye = HeroSmoothness("Cat_Eye");
            float highlight = HeroSmoothness("Cat_Highlight");
            float moon = HeroSmoothness("Moon_Butter");
            float moonEmission = HeroEmission("Moon_Butter");
            float highlightEmission = HeroEmission("Cat_Highlight");
            detail = $"body={body:0.00} belly={belly:0.00} eye={eye:0.00} " +
                $"highlight={highlight:0.00} moon={moon:0.00}/{moonEmission:0.00} " +
                $"highlightEmission={highlightEmission:0.00}";
            return body >= 0.18f && body <= 0.28f && belly <= 0.18f &&
                eye >= 0.82f && highlight >= 0.90f && moon >= 0.50f &&
                moonEmission >= 0.10f && highlightEmission >= 0.14f;
        }

        public static bool ValidateHeroEyeContract(out string detail)
        {
            Color eye = HeroPaletteColor("Cat_Eye");
            Color highlight = HeroPaletteColor("Cat_Highlight");
            float eyeLuminance = eye.r * 0.2126f + eye.g * 0.7152f + eye.b * 0.0722f;
            float highlightLuminance = highlight.r * 0.2126f +
                                       highlight.g * 0.7152f + highlight.b * 0.0722f;
            float contrast = highlightLuminance - eyeLuminance;
            float eyeSmoothness = HeroSmoothness("Cat_Eye");
            float highlightSmoothness = HeroSmoothness("Cat_Highlight");
            detail = $"contrast={contrast:0.00} smooth={eyeSmoothness:0.00}/" +
                     $"{highlightSmoothness:0.00} emission={HeroEyeIdleEmission:0.00}->" +
                     $"{HeroEyeActionEmission:0.00}";
            return contrast >= 0.70f && eyeSmoothness >= 0.82f &&
                   highlightSmoothness >= 0.90f && HeroEyeIdleEmission >= 0.16f &&
                   HeroEyeActionEmission - HeroEyeIdleEmission >= 0.08f &&
                   HeroEyeActionEmission <= 0.32f;
        }
    }

    public sealed class MoonlightHeroVisualQuality : MonoBehaviour
    {
        public int RendererCount { get; private set; }
        public int ShadowRendererCount { get; private set; }
        public int MaterialCount { get; private set; }
        public int SurfaceProfileCount { get; private set; }
        public int EmissiveMaterialCount { get; private set; }
        public int TexturedMaterialCount { get; private set; }
        public string QAMarker => RendererCount > 0 && ShadowRendererCount == RendererCount &&
            MaterialCount >= 8 && MaterialCount <= MoonlightHouseSetup.HeroMaterialBudget &&
            SurfaceProfileCount >= 6 && EmissiveMaterialCount >= 2
                ? "MOONLIGHT_HERO_SURFACE_SHADING_READY"
                : "MOONLIGHT_HERO_SURFACE_SHADING_INCOMPLETE";

        public void Configure(int rendererCount, int shadowRendererCount, int materialCount,
                              int surfaceProfileCount, int emissiveMaterialCount,
                              int texturedMaterialCount)
        {
            RendererCount = rendererCount;
            ShadowRendererCount = shadowRendererCount;
            MaterialCount = materialCount;
            SurfaceProfileCount = surfaceProfileCount;
            EmissiveMaterialCount = emissiveMaterialCount;
            TexturedMaterialCount = texturedMaterialCount;
            Debug.Log($"[MoonlightHeroQA] renderers={RendererCount} shadows={ShadowRendererCount} " +
                $"materials={MaterialCount}/{MoonlightHouseSetup.HeroMaterialBudget} " +
                $"profiles={SurfaceProfileCount} emissive={EmissiveMaterialCount} " +
                $"textured={TexturedMaterialCount} marker={QAMarker}");
        }
    }

    public sealed class MoonlightRoomVisualQuality : MonoBehaviour
    {
        public int RendererCount { get; private set; }
        public int ShadowCasterCount { get; private set; }
        public int ShadowReceiverCount { get; private set; }
        public int SourceMaterialCount { get; private set; }
        public int RuntimeMaterialCount { get; private set; }
        public int RuntimeMaterialCountDelta => RuntimeMaterialCount - SourceMaterialCount;
        public int SemanticMaterialCount { get; private set; }
        public int SemanticProfileMatchCount { get; private set; }
        public int SurfaceProfileCount { get; private set; }
        public int EmissiveMaterialCount { get; private set; }
        public int TexturedMaterialCount { get; private set; }
        public string QAMarker => RendererCount > 0 &&
            ShadowCasterCount == RendererCount && ShadowReceiverCount == RendererCount &&
            SourceMaterialCount >= MoonlightHouseSetup.RoomAuthoredMaterialMinimum &&
            SourceMaterialCount <= MoonlightHouseSetup.RoomAuthoredMaterialMaximum &&
            RuntimeMaterialCountDelta == 0 &&
            SemanticMaterialCount >= MoonlightHouseSetup.RoomSurfaceProfileMinimum &&
            SemanticProfileMatchCount == SemanticMaterialCount &&
            SurfaceProfileCount >= MoonlightHouseSetup.RoomSurfaceProfileMinimum &&
            EmissiveMaterialCount >= 1
                ? "MOONLIGHT_ROOM_SURFACE_SHADING_READY"
                : "MOONLIGHT_ROOM_SURFACE_SHADING_INCOMPLETE";

        public void Configure(int rendererCount, int shadowCasterCount, int shadowReceiverCount,
                              int sourceMaterialCount, int runtimeMaterialCount,
                              int semanticMaterialCount, int semanticProfileMatchCount,
                              int surfaceProfileCount, int emissiveMaterialCount,
                              int texturedMaterialCount)
        {
            RendererCount = rendererCount;
            ShadowCasterCount = shadowCasterCount;
            ShadowReceiverCount = shadowReceiverCount;
            SourceMaterialCount = sourceMaterialCount;
            RuntimeMaterialCount = runtimeMaterialCount;
            SemanticMaterialCount = semanticMaterialCount;
            SemanticProfileMatchCount = semanticProfileMatchCount;
            SurfaceProfileCount = surfaceProfileCount;
            EmissiveMaterialCount = emissiveMaterialCount;
            TexturedMaterialCount = texturedMaterialCount;
            Debug.Log($"[MoonlightRoomSurfaceQA] renderers={RendererCount} " +
                $"casters={ShadowCasterCount} receivers={ShadowReceiverCount} " +
                $"materials={SourceMaterialCount}/{RuntimeMaterialCount} " +
                $"delta={RuntimeMaterialCountDelta} semantic={SemanticMaterialCount}/" +
                $"{MoonlightHouseSetup.RoomSurfaceProfileMinimum} " +
                $"semanticProfiles={SemanticProfileMatchCount}/{SemanticMaterialCount} " +
                $"profiles={SurfaceProfileCount} emissive={EmissiveMaterialCount} " +
                $"textured={TexturedMaterialCount} marker={QAMarker}");
        }
    }

    public sealed class MoonlightHeroEyeQuality : MonoBehaviour
    {
        readonly System.Collections.Generic.List<Renderer> _eyeRenderers =
            new System.Collections.Generic.List<Renderer>();
        readonly System.Collections.Generic.List<Renderer> _highlightRenderers =
            new System.Collections.Generic.List<Renderer>();
        readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();
        MoonlightActionFeedback _actionFeedback;
        EyeBlinker _blinker;

        public int EyeRendererCount => _eyeRenderers.Count;
        public int HighlightRendererCount => _highlightRenderers.Count;
        public float EyePairSeparation { get; private set; }
        public float CurrentCatchlightEmission { get; private set; }
        public Color CurrentCatchlightColor { get; private set; } = Color.white;
        public string CurrentExpressionName { get; private set; } = "idle-ivory";
        public int BlinkLinkedPartCount => _blinker != null ? _blinker.BoundEyeCount : 0;
        public float BlinkCurrentOpenness => _blinker != null ? _blinker.CurrentOpenness : 1f;
        public string BlinkQAMarker => _blinker != null ? _blinker.QAMarker : "missing";
        public string QAMarker => EyeRendererCount == 2 && HighlightRendererCount == 2 &&
            EyePairSeparation > 0.01f && _blinker != null &&
            _blinker.QAMarker == "MOONLIGHT_AUTHORED_EYE_BLINK_READY"
                ? "MOONLIGHT_HERO_EYE_CATCHLIGHT_READY"
                : "MOONLIGHT_HERO_EYE_CATCHLIGHT_INCOMPLETE";

        public void Configure(GameObject root)
        {
            _eyeRenderers.Clear();
            _highlightRenderers.Clear();
            if (root == null) return;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                string lower = renderer.gameObject.name.ToLowerInvariant();
                if (lower.Contains("eyehighlight")) _highlightRenderers.Add(renderer);
                else if (lower.Contains("eyeleft") || lower.Contains("eyeright"))
                    _eyeRenderers.Add(renderer);
            }
            EyePairSeparation = _eyeRenderers.Count == 2
                ? Vector3.Distance(_eyeRenderers[0].bounds.center, _eyeRenderers[1].bounds.center)
                : 0f;
            var blinkParts = new Transform[_eyeRenderers.Count + _highlightRenderers.Count];
            int blinkIndex = 0;
            foreach (Renderer renderer in _eyeRenderers) blinkParts[blinkIndex++] = renderer.transform;
            foreach (Renderer renderer in _highlightRenderers) blinkParts[blinkIndex++] = renderer.transform;
            _blinker = root.GetComponent<EyeBlinker>() ?? root.AddComponent<EyeBlinker>();
            _blinker.Bind(blinkParts);
            _actionFeedback = GetComponentInParent<MoonlightActionFeedback>();
            ApplyCatchlight(ExpressionColorFor(default, false),
                MoonlightHouseSetup.HeroEyeIdleEmission);
            Debug.Log($"[MoonlightHeroQA] eyes={EyeRendererCount} highlights={HighlightRendererCount} " +
                $"blinkParts={BlinkLinkedPartCount} separation={EyePairSeparation:0.000} " +
                $"emission={CurrentCatchlightEmission:0.00} blink={BlinkQAMarker} " +
                $"marker={QAMarker}");
        }

        void Update()
        {
            if (_highlightRenderers.Count == 0) return;
            if (_actionFeedback == null) _actionFeedback = GetComponentInParent<MoonlightActionFeedback>();
            bool performing = _actionFeedback != null && _actionFeedback.IsPerformingAction;
            float baseEmission = performing
                ? MoonlightHouseSetup.HeroEyeActionEmission
                : MoonlightHouseSetup.HeroEyeIdleEmission;
            float shimmer = (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 2.4f)) * 0.035f;
            MoonlightSpatialActionKind kind = performing
                ? _actionFeedback.ActiveActivityKind
                : default;
            CurrentExpressionName = ExpressionNameFor(kind, performing);
            ApplyCatchlight(ExpressionColorFor(kind, performing), baseEmission + shimmer);
        }

        void ApplyCatchlight(Color color, float emission)
        {
            CurrentCatchlightEmission = emission;
            CurrentCatchlightColor = color;
            Color catchlight = color * emission;
            foreach (Renderer renderer in _highlightRenderers)
            {
                if (renderer == null) continue;
                renderer.GetPropertyBlock(_block);
                _block.SetColor("_EmissionColor", catchlight);
                renderer.SetPropertyBlock(_block);
                _block.Clear();
            }
        }

        public static Color ExpressionColorFor(MoonlightSpatialActionKind kind, bool performing)
        {
            if (!performing) return new Color(1f, 0.98f, 0.91f);
            return kind switch
            {
                MoonlightSpatialActionKind.Cook => new Color(1f, 0.72f, 0.28f),
                MoonlightSpatialActionKind.Play => new Color(0.38f, 0.86f, 1f),
                MoonlightSpatialActionKind.Garden => new Color(0.42f, 1f, 0.62f),
                MoonlightSpatialActionKind.Read => new Color(0.76f, 0.58f, 1f),
                MoonlightSpatialActionKind.SleepCuddle => new Color(1f, 0.56f, 0.72f),
                MoonlightSpatialActionKind.Care => new Color(0.16f, 0.74f, 0.66f),
                _ => new Color(1f, 0.98f, 0.91f)
            };
        }

        public static string ExpressionNameFor(MoonlightSpatialActionKind kind, bool performing)
        {
            if (!performing) return "idle-ivory";
            return kind switch
            {
                MoonlightSpatialActionKind.Cook => "cook-warm-focus",
                MoonlightSpatialActionKind.Play => "play-sky-excitement",
                MoonlightSpatialActionKind.Garden => "garden-mint-calm",
                MoonlightSpatialActionKind.Read => "read-lilac-focus",
                MoonlightSpatialActionKind.SleepCuddle => "cuddle-rose-soft",
                MoonlightSpatialActionKind.Care => "care-teal-soft",
                _ => "magic-ivory"
            };
        }

        public static bool ValidateActivityExpressionContract(out string detail)
        {
            var colors = new[]
            {
                ExpressionColorFor(MoonlightSpatialActionKind.Cook, true),
                ExpressionColorFor(MoonlightSpatialActionKind.Play, true),
                ExpressionColorFor(MoonlightSpatialActionKind.Garden, true),
                ExpressionColorFor(MoonlightSpatialActionKind.Read, true),
                ExpressionColorFor(MoonlightSpatialActionKind.SleepCuddle, true),
                ExpressionColorFor(MoonlightSpatialActionKind.Care, true)
            };
            float minimumDistance = float.PositiveInfinity;
            for (int i = 0; i < colors.Length; i++)
                for (int j = i + 1; j < colors.Length; j++)
                    minimumDistance = Mathf.Min(minimumDistance, Vector3.Distance(
                        new Vector3(colors[i].r, colors[i].g, colors[i].b),
                        new Vector3(colors[j].r, colors[j].g, colors[j].b)));
            float emissionLift = MoonlightHouseSetup.HeroEyeActionEmission -
                MoonlightHouseSetup.HeroEyeIdleEmission;
            string careExpression = ExpressionNameFor(MoonlightSpatialActionKind.Care, true);
            detail = $"expressions={colors.Length} minColorDistance={minimumDistance:0.00} " +
                $"emissionLift={emissionLift:0.00} care={careExpression}";
            return colors.Length == 6 && minimumDistance >= 0.32f && emissionLift >= 0.08f &&
                careExpression == "care-teal-soft";
        }
    }
}
