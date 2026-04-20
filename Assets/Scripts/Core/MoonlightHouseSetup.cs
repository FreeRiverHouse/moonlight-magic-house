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
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.02f, 0.10f);
            cam.farClipPlane    = 80f;
            cam.fieldOfView     = 60f;
            cam.transform.position = new Vector3(0f, 2f, -4f);
            cam.transform.LookAt(new Vector3(0f, 1f, 0f));
            cam.allowHDR = true;

            if (!cam.GetComponent<CameraController>())
                cam.gameObject.AddComponent<CameraController>();
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
                Quaternion.Euler(40f, -20f, 0f), LightShadows.Soft, 0.4f);

            // Warm fill — fireplace/lamp feel
            MakeLight("WarmFill", LightType.Directional,
                new Color(1.00f, 0.60f, 0.30f), 0.6f,
                Quaternion.Euler(30f, 160f, 0f), LightShadows.None, 0f);

            // Magic rim — purple
            MakeLight("MagicRim", LightType.Directional,
                new Color(0.60f, 0.30f, 1.00f), 0.9f,
                Quaternion.Euler(20f, 180f, 0f), LightShadows.None, 0f);
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

            // Procedural idle bob — no Animator required
            mlGO.AddComponent<MoonlightBobber>();

            // Collider
            var col = mlGO.AddComponent<CapsuleCollider>();
            col.height = 2.2f;
            col.radius = 0.38f;
            col.center = new Vector3(0f, 1.1f, 0f);
            mlGO.AddComponent<MoonlightInteractable>();

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

            MakePart(visual.transform, PrimitiveType.Capsule, "Body",
                new Vector3(0f, 0.9f, 0f), new Vector3(0.55f, 0.75f, 0.55f),
                new Color(0.82f, 0.72f, 0.96f));
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
            // Cheeks
            MakePart(visual.transform, PrimitiveType.Sphere, "CheekL",
                new Vector3(-0.14f, 1.82f, 0.17f), Vector3.one * 0.05f,
                new Color(1.0f, 0.6f, 0.7f));
            MakePart(visual.transform, PrimitiveType.Sphere, "CheekR",
                new Vector3( 0.14f, 1.82f, 0.17f), Vector3.one * 0.05f,
                new Color(1.0f, 0.6f, 0.7f));
            // Arms
            MakePart(visual.transform, PrimitiveType.Capsule, "ArmL",
                new Vector3(-0.42f, 1.0f, 0f), new Vector3(0.16f, 0.35f, 0.16f),
                new Color(0.78f, 0.68f, 0.92f));
            MakePart(visual.transform, PrimitiveType.Capsule, "ArmR",
                new Vector3( 0.42f, 1.0f, 0f), new Vector3(0.16f, 0.35f, 0.16f),
                new Color(0.78f, 0.68f, 0.92f));
            return visual;
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

            // Floor — slightly textured color
            Prim(PrimitiveType.Cube, "Floor", root.transform,
                new Vector3(0f, -0.1f, 0f), new Vector3(10f, 0.2f, 10f),
                new Color(0.14f, 0.09f, 0.24f));

            // Ceiling
            Prim(PrimitiveType.Cube, "Ceiling", root.transform,
                new Vector3(0f, 5.1f, 0f), new Vector3(10f, 0.2f, 10f),
                new Color(0.08f, 0.05f, 0.15f));

            // Walls
            Color wallCol = new Color(0.11f, 0.07f, 0.19f);
            Prim(PrimitiveType.Cube, "WallBack",  root.transform, new Vector3(0f, 2.5f,  5f), new Vector3(10f, 5f, 0.2f), wallCol);
            Prim(PrimitiveType.Cube, "WallFront", root.transform, new Vector3(0f, 2.5f, -5f), new Vector3(10f, 5f, 0.2f), wallCol);
            Prim(PrimitiveType.Cube, "WallRight", root.transform, new Vector3( 5f, 2.5f, 0f), new Vector3(0.2f, 5f, 10f), wallCol);
            Prim(PrimitiveType.Cube, "WallLeft",  root.transform, new Vector3(-5f, 2.5f, 0f), new Vector3(0.2f, 5f, 10f), wallCol);

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

                // Sofa primitive
                Prim(PrimitiveType.Cube, "SofaBase", root.transform,
                    new Vector3(-1.5f, 0.3f, 3f), new Vector3(2.4f, 0.55f, 0.9f),
                    new Color(0.50f, 0.25f, 0.75f));
                Prim(PrimitiveType.Cube, "SofaBack", root.transform,
                    new Vector3(-1.5f, 0.85f, 3.4f), new Vector3(2.4f, 0.65f, 0.2f),
                    new Color(0.45f, 0.22f, 0.68f));
                Prim(PrimitiveType.Cube, "SofaArmL", root.transform,
                    new Vector3(-2.75f, 0.55f, 3f), new Vector3(0.2f, 0.6f, 0.9f),
                    new Color(0.42f, 0.20f, 0.65f));
                Prim(PrimitiveType.Cube, "SofaArmR", root.transform,
                    new Vector3(-0.25f, 0.55f, 3f), new Vector3(0.2f, 0.6f, 0.9f),
                    new Color(0.42f, 0.20f, 0.65f));

                // Cushion
                Prim(PrimitiveType.Cube, "Cushion", root.transform,
                    new Vector3(-1.5f, 0.65f, 2.85f), new Vector3(0.7f, 0.18f, 0.55f),
                    new Color(0.85f, 0.55f, 0.95f));

                // Coffee table
                Prim(PrimitiveType.Cube, "TableTop", root.transform,
                    new Vector3(1.2f, 0.5f, 1.5f), new Vector3(1.2f, 0.08f, 0.7f),
                    new Color(0.28f, 0.16f, 0.45f));
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

                // Rug
                Prim(PrimitiveType.Cube, "Rug", root.transform,
                    new Vector3(0f, 0.01f, 1f), new Vector3(4.5f, 0.02f, 3.5f),
                    new Color(0.40f, 0.18f, 0.65f));

                // Toy chest
                Prim(PrimitiveType.Cube, "ChestBody", root.transform,
                    new Vector3(3f, 0.28f, 2.8f), new Vector3(0.8f, 0.56f, 0.55f),
                    new Color(0.35f, 0.55f, 0.80f));
                Prim(PrimitiveType.Cube, "ChestLid", root.transform,
                    new Vector3(3f, 0.62f, 2.8f), new Vector3(0.82f, 0.14f, 0.57f),
                    new Color(0.45f, 0.65f, 0.90f));

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
                }
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
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // HUD panel (top)
            var hud = Panel("HUD", canvasGO.transform,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -200f), new Vector2(0f, 0f),
                new Color(0f, 0f, 0f, 0.55f));

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

            var sliders = new Slider[5];
            for (int i = 0; i < 5; i++)
            {
                var res = new DefaultControls.Resources();
                var slGO = DefaultControls.CreateSlider(res);
                slGO.name = names[i] + "Bar";
                slGO.transform.SetParent(hud.transform, false);
                var slRt = slGO.GetComponent<RectTransform>();
                slRt.anchoredPosition = new Vector2(xPos[i], -80f);
                slRt.sizeDelta        = new Vector2(180f, 20f);
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

                // Label above slider
                var lblGO = new GameObject(names[i] + "Lbl");
                lblGO.transform.SetParent(hud.transform, false);
                var lbl = lblGO.AddComponent<TextMeshProUGUI>();
                lbl.text      = names[i][0].ToString();
                lbl.fontSize  = 20;
                lbl.color     = cols[i];
                lbl.alignment = TextAlignmentOptions.Center;
                var lblRt = lbl.GetComponent<RectTransform>();
                lblRt.anchoredPosition = new Vector2(xPos[i], -48f);
                lblRt.sizeDelta        = new Vector2(60f, 30f);
            }

            // Info labels (stage, mood, coins, xp, days)
            var stageLabel = MakeTMPLabel("StageLabel", hud.transform, new Vector2(-380f, -145f), new Vector2(200f, 35f), "Moonbud", 22, Color.white);
            var moodLabel  = MakeTMPLabel("MoodLabel",  hud.transform, new Vector2(-180f, -145f), new Vector2(60f, 35f),  "🌸",     28, Color.white);
            var coinsLabel = MakeTMPLabel("CoinsLabel", hud.transform, new Vector2(  60f, -145f), new Vector2(180f, 35f), "⭐ 30",  22, new Color(1f, 0.9f, 0.3f));
            var xpLabel    = MakeTMPLabel("XPLabel",    hud.transform, new Vector2( 240f, -145f), new Vector2(140f, 35f), "XP 0",   20, new Color(0.7f, 0.5f, 1f));
            var daysLabel  = MakeTMPLabel("DaysLabel",  hud.transform, new Vector2( 380f, -145f), new Vector2(120f, 35f), "Day 1",  20, new Color(0.7f, 0.9f, 1f));

            // Action buttons (bottom)
            var btnPanel = Panel("ActionBar", canvasGO.transform,
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, 0f), new Vector2(0f, 160f),
                new Color(0f, 0f, 0f, 0.55f));

            var feedBtn   = MakeButton("FeedBtn",   btnPanel.transform, new Vector2(-250f, 60f), "🍰 Feed",   new Color(0.9f, 0.6f, 0.2f));
            var cuddleBtn = MakeButton("CuddleBtn", btnPanel.transform, new Vector2(   0f, 60f), "🤗 Cuddle", new Color(0.9f, 0.4f, 0.7f));
            var sleepBtn  = MakeButton("SleepBtn",  btnPanel.transform, new Vector2( 250f, 60f), "💤 Sleep",  new Color(0.3f, 0.5f, 0.9f));

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
            if (lbl) { lbl.text = label; lbl.fontSize = 18; lbl.color = Color.white; }
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
