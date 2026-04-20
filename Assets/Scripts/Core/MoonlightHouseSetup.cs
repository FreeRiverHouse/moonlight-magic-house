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
            cam.transform.position = new Vector3(0f, 3f, -8f);
            cam.transform.LookAt(new Vector3(0f, 1f, 0f));
            cam.allowHDR = true;

            if (!cam.GetComponent<CameraController>())
                cam.gameObject.AddComponent<CameraController>();
        }

        // ── Atmosphere ───────────────────────────────────────────────────────
        void SetupAtmosphere()
        {
            RenderSettings.ambientMode    = AmbientMode.Flat;
            RenderSettings.ambientLight   = new Color(0.04f, 0.02f, 0.10f);
            RenderSettings.fog            = true;
            RenderSettings.fogMode        = FogMode.Linear;
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance   = 45f;
            RenderSettings.fogColor       = new Color(0.05f, 0.02f, 0.13f);

            // Moon — cool blue-white directional
            var moon = MakeLight("Moon", LightType.Directional,
                new Color(0.70f, 0.82f, 1.00f), 0.55f,
                Quaternion.Euler(40f, -20f, 0f), LightShadows.Soft, 0.4f);

            // Warm fill — fireplace/lamp feel
            MakeLight("WarmFill", LightType.Directional,
                new Color(1.00f, 0.60f, 0.30f), 0.28f,
                Quaternion.Euler(30f, 160f, 0f), LightShadows.None, 0f);

            // Magic rim — purple
            MakeLight("MagicRim", LightType.Directional,
                new Color(0.60f, 0.30f, 1.00f), 0.55f,
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
            mlGO.AddComponent<PetMoodParticles>();
            // MoonlightAnimator requires Animator — Unity auto-adds it via RequireComponent
            mlGO.AddComponent<MoonlightAnimator>();

            // Visual child (rendering only — primitives)
            var visual = new GameObject("Visual");
            visual.transform.SetParent(mlGO.transform, false);

            // Capsule body — pale lavender
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(visual.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale    = new Vector3(0.55f, 0.75f, 0.55f);
            SetMat(body, new Color(0.82f, 0.72f, 0.96f));
            Object.Destroy(body.GetComponent<Collider>());

            // Sphere head — warm skin tone
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(visual.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.85f, 0f);
            head.transform.localScale    = new Vector3(0.42f, 0.42f, 0.42f);
            SetMat(head, new Color(1.00f, 0.90f, 0.82f));
            Object.Destroy(head.GetComponent<Collider>());

            // Hair blob — deep violet
            var hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hair.name = "Hair";
            hair.transform.SetParent(visual.transform, false);
            hair.transform.localPosition = new Vector3(0f, 2.08f, 0.04f);
            hair.transform.localScale    = new Vector3(0.46f, 0.30f, 0.46f);
            SetMat(hair, new Color(0.22f, 0.08f, 0.48f));
            Object.Destroy(hair.GetComponent<Collider>());

            // Collider + interactable on root
            var col = mlGO.AddComponent<CapsuleCollider>();
            col.height = 2.3f;
            col.radius = 0.38f;
            col.center = new Vector3(0f, 1.1f, 0f);
            mlGO.AddComponent<MoonlightInteractable>();

            // Point glow light
            var glowGO = new GameObject("Glow");
            glowGO.transform.SetParent(mlGO.transform, false);
            glowGO.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            var glow = glowGO.AddComponent<Light>();
            glow.type      = LightType.Point;
            glow.color     = new Color(0.65f, 0.40f, 1.00f);
            glow.intensity = 1.2f;
            glow.range     = 4.5f;

            return mlGO;
        }

        // ── Rooms ────────────────────────────────────────────────────────────
        RoomManager CreateRooms()
        {
            var rmGO = new GameObject("RoomManager");
            var rm   = rmGO.AddComponent<RoomManager>();

            var lr = BuildRoom("LivingRoom", Vector3.zero,
                new Color(0.12f, 0.07f, 0.22f), true);
            lr.AddComponent<FeedingStation>();
            lr.AddComponent<PlayArea>();

            var kt = BuildRoom("Kitchen",  new Vector3(14f, 0f,   0f),
                new Color(0.06f, 0.10f, 0.06f), false);

            var bd = BuildRoom("Bedroom",  new Vector3(-14f, 0f,  0f),
                new Color(0.04f, 0.04f, 0.18f), false);
            bd.AddComponent<SleepArea>();

            var gd = BuildRoom("Garden",   new Vector3(0f,   0f, 14f),
                new Color(0.04f, 0.10f, 0.04f), false);
            gd.AddComponent<GardenArea>();

            var lb = BuildRoom("Library",  new Vector3(0f,   0f, -14f),
                new Color(0.09f, 0.05f, 0.02f), false);
            lb.AddComponent<LibraryRoom>();

            rm.AddRoom(RoomType.LivingRoom, lr);
            rm.AddRoom(RoomType.Kitchen,    kt);
            rm.AddRoom(RoomType.Bedroom,    bd);
            rm.AddRoom(RoomType.Garden,     gd);
            rm.AddRoom(RoomType.Library,    lb);

            return rm;
        }

        GameObject BuildRoom(string roomName, Vector3 pos, Color ambColor, bool active)
        {
            var root = new GameObject(roomName);
            root.transform.position = pos;

            // Floor
            var floor = Prim(PrimitiveType.Cube, "Floor", root.transform,
                new Vector3(0f, -0.1f, 0f), new Vector3(10f, 0.2f, 10f),
                new Color(0.16f, 0.10f, 0.26f));

            // Ceiling
            Prim(PrimitiveType.Cube, "Ceiling", root.transform,
                new Vector3(0f, 5.1f, 0f), new Vector3(10f, 0.2f, 10f),
                new Color(0.10f, 0.06f, 0.18f));

            // Walls: back, front, left, right
            Prim(PrimitiveType.Cube, "WallBack",  root.transform, new Vector3(0f, 2.5f,  5f), new Vector3(10f, 5f, 0.2f), new Color(0.12f, 0.07f, 0.20f));
            Prim(PrimitiveType.Cube, "WallFront", root.transform, new Vector3(0f, 2.5f, -5f), new Vector3(10f, 5f, 0.2f), new Color(0.12f, 0.07f, 0.20f));
            Prim(PrimitiveType.Cube, "WallRight", root.transform, new Vector3( 5f, 2.5f, 0f), new Vector3(0.2f, 5f, 10f), new Color(0.12f, 0.07f, 0.20f));
            Prim(PrimitiveType.Cube, "WallLeft",  root.transform, new Vector3(-5f, 2.5f, 0f), new Vector3(0.2f, 5f, 10f), new Color(0.12f, 0.07f, 0.20f));

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
            rl.intensity = 0.8f;
            rl.range     = 12f;

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
            SetMat(go, color);
            return go;
        }

        static void SetMat(GameObject go, Color color)
        {
            var mr = go.GetComponent<MeshRenderer>();
            if (!mr) return;
            var mat = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            mat.color = color;
            mr.material = mat;
        }
    }
}
