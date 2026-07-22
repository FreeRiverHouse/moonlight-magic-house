using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MoonlightMagicHouse
{
    public sealed class MoonlightActivityStation : MonoBehaviour
    {
        const string MagicFlowerResourcePath = "Models/Props/Garden/MagicFlowerBloom";
        const int CompletionMagicFlowerRequiredInstances = 5;
        const int CompletionMagicFlowerMaxRenderers = 10;

        readonly List<Material> _completionMaterials = new();
        readonly Dictionary<int, Material> _completionMaterialCache = new();
        readonly List<Transform> _animatedCompletionDetails = new();
        readonly List<Vector3> _animatedBasePositions = new();
        readonly List<Vector3> _animatedBaseScales = new();
        readonly HashSet<int> _completionMagicFlowerMaterialIds = new();
        GameObject _completionRoot;
        GameObject _magicFlowerPrefab;

        public MoonlightSpatialActionKind Kind { get; private set; }
        public Transform VisualRoot { get; private set; }
        public int RendererCount { get; private set; }
        public int UniqueMaterialCount { get; private set; }
        public int ColliderCount { get; private set; }
        public int LightCount { get; private set; }
        public Vector3 BoundsSize { get; private set; }
        public Vector3 AnchorPosition => transform.position;
        public Vector3 AnchorScale => transform.lossyScale;
        public bool HasCompletionState => _completionRoot != null && _completionRoot.activeInHierarchy;
        public int CompletionRendererCount { get; private set; }
        public int CompletionUniqueMaterialCount { get; private set; }
        public int CompletionEnabledColliderCount { get; private set; }
        public int CompletionEnabledLightCount { get; private set; }
        public bool HasCompletionMagicFlowerPrefab =>
            CompletionMagicFlowerInstanceCount == CompletionMagicFlowerRequiredInstances &&
            CompletionMagicFlowerRendererCount > 0;
        public int CompletionMagicFlowerInstanceCount { get; private set; }
        public int CompletionMagicFlowerRendererCount { get; private set; }
        public int CompletionMagicFlowerUniqueMaterialCount => _completionMagicFlowerMaterialIds.Count;
        public int CompletionMagicFlowerColliderCount { get; private set; }
        public int CompletionMagicFlowerLightCount { get; private set; }
        public int CompletionMagicFlowerEnabledColliderCount { get; private set; }
        public int CompletionMagicFlowerEnabledLightCount { get; private set; }
        public bool CompletionMagicFlowerUsesSharedMaterials { get; private set; } = true;
        public int CompletionMagicFlowerRendererBudget => CompletionMagicFlowerMaxRenderers;
        public string CompletionMagicFlowerQAMarker => HasCompletionMagicFlowerPrefab
            ? "MOONLIGHT_MAGIC_FLOWER_PERSISTENT_READY"
            : "MOONLIGHT_MAGIC_FLOWER_PERSISTENT_MISSING";

        public bool Configure(MoonlightSpatialActionKind kind, string resourcePath,
            Vector3 worldPosition, Vector3 stageScale, Vector3 visualLocalPosition,
            Vector3 visualLocalEuler, Vector3 visualLocalScale)
        {
            Kind = kind;
            transform.position = worldPosition;
            transform.rotation = Quaternion.identity;
            transform.localScale = stageScale;

            var prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                Debug.LogError($"[MoonlightActivityStation] missing authored asset kind={kind} path={resourcePath}");
                return false;
            }

            var instance = Instantiate(prefab, transform, false);
            instance.name = $"Persistent{kind}Visual";
            instance.transform.localPosition = visualLocalPosition;
            instance.transform.localRotation = Quaternion.Euler(visualLocalEuler);
            instance.transform.localScale = visualLocalScale;
            VisualRoot = instance.transform;

            var materialIds = new HashSet<int>();
            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
                foreach (var material in renderer.sharedMaterials)
                    if (material != null) materialIds.Add(material.GetInstanceID());
            }

            var colliders = instance.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = false;
            var lights = instance.GetComponentsInChildren<Light>(true);
            for (int i = 0; i < lights.Length; i++) lights[i].enabled = false;

            RendererCount = renderers.Length;
            UniqueMaterialCount = materialIds.Count;
            ColliderCount = colliders.Length;
            LightCount = lights.Length;
            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                BoundsSize = bounds.size;
            }

            Debug.Log($"[MoonlightActivityStation] ready kind={kind} renderers={RendererCount} " +
                $"materials={UniqueMaterialCount} colliders={ColliderCount} lights={LightCount} " +
                $"anchor={AnchorPosition:F2} bounds={BoundsSize:F2} marker=MOONLIGHT_PERSISTENT_STATION_READY");
            return true;
        }

        public void ResetCompletionState()
        {
            if (_completionRoot != null)
            {
                _completionRoot.SetActive(false);
                Destroy(_completionRoot);
            }
            for (int i = 0; i < _completionMaterials.Count; i++)
                if (_completionMaterials[i] != null) Destroy(_completionMaterials[i]);
            _completionMaterials.Clear();
            _completionMaterialCache.Clear();
            _animatedCompletionDetails.Clear();
            _animatedBasePositions.Clear();
            _animatedBaseScales.Clear();
            _completionMagicFlowerMaterialIds.Clear();
            _completionRoot = null;
            CompletionRendererCount = 0;
            CompletionUniqueMaterialCount = 0;
            CompletionEnabledColliderCount = 0;
            CompletionEnabledLightCount = 0;
            CompletionMagicFlowerInstanceCount = 0;
            CompletionMagicFlowerRendererCount = 0;
            CompletionMagicFlowerColliderCount = 0;
            CompletionMagicFlowerLightCount = 0;
            CompletionMagicFlowerEnabledColliderCount = 0;
            CompletionMagicFlowerEnabledLightCount = 0;
            CompletionMagicFlowerUsesSharedMaterials = true;
            Debug.Log($"[MoonlightActivityStation] completion-reset kind={Kind} " +
                "marker=MOONLIGHT_PERSISTENT_ACTIVITY_STATE_RESET");
        }

        public void ApplyCompletionState()
        {
            ResetCompletionState();
            _completionRoot = new GameObject($"Persistent{Kind}Completion");
            _completionRoot.transform.SetParent(transform, false);

            switch (Kind)
            {
                case MoonlightSpatialActionKind.Cook:
                    BuildCookCompletion();
                    break;
                case MoonlightSpatialActionKind.Play:
                    BuildPlayCompletion();
                    break;
                case MoonlightSpatialActionKind.Garden:
                    BuildGardenCompletion();
                    break;
                case MoonlightSpatialActionKind.Read:
                    BuildReadCompletion();
                    break;
            }

            var renderers = _completionRoot.GetComponentsInChildren<Renderer>(true);
            var materialIds = new HashSet<int>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].shadowCastingMode = ShadowCastingMode.On;
                renderers[i].receiveShadows = true;
                foreach (var material in renderers[i].sharedMaterials)
                    if (material != null) materialIds.Add(material.GetInstanceID());
            }
            CompletionRendererCount = renderers.Length;
            CompletionUniqueMaterialCount = materialIds.Count;
            CompletionEnabledColliderCount = CountEnabled(_completionRoot.GetComponentsInChildren<Collider>(true));
            CompletionEnabledLightCount = CountEnabled(_completionRoot.GetComponentsInChildren<Light>(true));
            Debug.Log($"[MoonlightActivityStation] completion-applied kind={Kind} " +
                $"renderers={CompletionRendererCount} materials={CompletionUniqueMaterialCount} " +
                $"colliders={CompletionEnabledColliderCount} lights={CompletionEnabledLightCount} " +
                "marker=MOONLIGHT_PERSISTENT_ACTIVITY_STATE_APPLIED");
        }

        void BuildCookCompletion()
        {
            CompletionPrimitive(PrimitiveType.Cylinder, "MooncakePlatter", new Vector3(0.34f, 0.59f, 0.04f),
                new Vector3(0.72f, 0.028f, 0.46f), new Color(0.80f, 0.74f, 0.68f), 0.04f);
            for (int i = 0; i < 3; i++)
            {
                float x = 0.04f + i * 0.29f;
                var cookie = CompletionPrimitive(PrimitiveType.Cylinder, $"FinishedMooncake-{i + 1}",
                    new Vector3(x, 0.67f, 0.04f + (i % 2) * 0.09f), new Vector3(0.19f, 0.042f, 0.19f),
                    new Color(0.88f, 0.61f, 0.30f), 0.06f);
                cookie.localRotation = Quaternion.Euler(0f, 24f + i * 32f, 0f);
                var mark = CompletionPrimitive(PrimitiveType.Sphere, $"MooncakePearl-{i + 1}",
                    cookie.localPosition + new Vector3(0f, 0.045f, 0f), Vector3.one * 0.045f,
                    new Color(1f, 0.88f, 0.48f), 0.16f);
                AddAnimated(mark);
            }

            CompletionPrimitive(PrimitiveType.Cube, "MoonNapkin", new Vector3(-0.34f, 0.61f, 0.13f),
                new Vector3(0.26f, 0.012f, 0.34f), new Color(0.80f, 0.74f, 0.68f), 0.04f)
                .localRotation = Quaternion.Euler(0f, 18f, 0f);
            CompletionPrimitive(PrimitiveType.Cylinder, "MoonTeaCup", new Vector3(-0.34f, 0.72f, -0.18f),
                new Vector3(0.15f, 0.12f, 0.15f), new Color(0.58f, 0.82f, 0.82f), 0.04f);
            CompletionPrimitive(PrimitiveType.Cylinder, "MoonTea", new Vector3(-0.34f, 0.845f, -0.18f),
                new Vector3(0.125f, 0.008f, 0.125f), new Color(1f, 0.88f, 0.48f), 0.12f);
            for (int i = 0; i < 2; i++)
            {
                var steam = CompletionPrimitive(PrimitiveType.Sphere, $"MoonTeaSteam-{i + 1}",
                    new Vector3(-0.38f + i * 0.08f, 0.98f + i * 0.09f, -0.18f),
                    new Vector3(0.035f, 0.09f, 0.035f), new Color(1f, 0.88f, 0.48f), 0.12f);
                AddAnimated(steam);
            }
        }

        void BuildPlayCompletion()
        {
            CompletionPrimitive(PrimitiveType.Cylinder, "StarBallPodium", new Vector3(0.72f, 0.16f, -0.34f),
                new Vector3(0.34f, 0.10f, 0.34f), new Color(0.58f, 0.48f, 0.70f), 0.06f);
            CompletionPrimitive(PrimitiveType.Cylinder, "StarBallPodiumTrim", new Vector3(0.72f, 0.27f, -0.34f),
                new Vector3(0.25f, 0.035f, 0.25f), new Color(0.98f, 0.82f, 0.38f), 0.14f);
            var ball = CompletionPrimitive(PrimitiveType.Sphere, "RestingStarBall", new Vector3(0.72f, 0.48f, -0.34f),
                Vector3.one * 0.23f, new Color(0.40f, 0.84f, 1f), 0.14f);
            AddAnimated(ball);
            for (int i = 0; i < 4; i++)
            {
                float angle = i * Mathf.PI * 0.5f;
                var sparkle = CompletionPrimitive(PrimitiveType.Sphere, $"StarBallSparkle-{i + 1}",
                    new Vector3(0.72f + Mathf.Cos(angle) * 0.32f, 0.50f + (i % 2) * 0.12f,
                        -0.34f + Mathf.Sin(angle) * 0.18f), Vector3.one * 0.045f,
                    new Color(1f, 0.88f, 0.38f), 0.20f);
                AddAnimated(sparkle);
            }
            for (int i = 0; i < 3; i++)
            {
                float angle = 0.35f + i * 1.65f;
                var orbit = CompletionPrimitive(PrimitiveType.Sphere, $"StarBallOrbit-{i + 1}",
                    new Vector3(0.72f + Mathf.Cos(angle) * 0.42f, 0.58f + i * 0.10f,
                        -0.34f + Mathf.Sin(angle) * 0.24f), Vector3.one * 0.065f,
                    i == 1 ? new Color(0.40f, 0.84f, 1f) : new Color(1f, 0.88f, 0.38f), 0.20f);
                AddAnimated(orbit);
            }
        }

        void BuildGardenCompletion()
        {
            if (_magicFlowerPrefab == null)
                _magicFlowerPrefab = Resources.Load<GameObject>(MagicFlowerResourcePath);
            if (_magicFlowerPrefab == null)
                Debug.LogError($"[MoonlightActivityStation] garden magic flower missing " +
                    $"path={MagicFlowerResourcePath} marker=MOONLIGHT_MAGIC_FLOWER_PERSISTENT_MISSING");

            for (int i = 0; i < 5; i++)
            {
                float x = -0.64f + i * 0.32f;
                float z = (i % 2 == 0) ? -0.16f : 0.10f;
                var flower = CreateCompletionMagicFlower($"PersistentMagicFlower-{i + 1}",
                    new Vector3(x, 0.48f + i * 0.012f, z),
                    Vector3.one * (0.62f + (i % 2) * 0.035f));
                var leaf = CompletionPrimitive(PrimitiveType.Sphere, $"PersistentLeaf-{i + 1}",
                    new Vector3(x + (i % 2 == 0 ? 0.10f : -0.10f), 0.68f, z),
                    new Vector3(0.14f, 0.045f, 0.075f), new Color(0.42f, 0.80f, 0.42f), 0.04f);
                leaf.localRotation = Quaternion.Euler(0f, i * 32f, i % 2 == 0 ? 24f : -24f);
                if (flower != null) AddAnimated(flower);
            }

            Debug.Log($"[MoonlightActivityStation] completion-magic-flower " +
                $"instances={CompletionMagicFlowerInstanceCount}/{CompletionMagicFlowerRequiredInstances} " +
                $"renderers={CompletionMagicFlowerRendererCount}/{CompletionMagicFlowerRendererBudget} " +
                $"materials={CompletionMagicFlowerUniqueMaterialCount} shared={CompletionMagicFlowerUsesSharedMaterials} " +
                $"colliders={CompletionMagicFlowerEnabledColliderCount}/{CompletionMagicFlowerColliderCount} " +
                $"lights={CompletionMagicFlowerEnabledLightCount}/{CompletionMagicFlowerLightCount} " +
                $"marker={CompletionMagicFlowerQAMarker}");
        }

        Transform CreateCompletionMagicFlower(string instanceName, Vector3 localPosition, Vector3 localScale)
        {
            if (_magicFlowerPrefab == null) return null;

            int instanceIndex = CompletionMagicFlowerInstanceCount;
            var instance = Instantiate(_magicFlowerPrefab, _completionRoot.transform, false);
            instance.name = instanceName;
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            instance.transform.localScale = localScale;

            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null) continue;
                    int materialId = material.GetInstanceID();
                    if (instanceIndex > 0 && !_completionMagicFlowerMaterialIds.Contains(materialId))
                        CompletionMagicFlowerUsesSharedMaterials = false;
                    _completionMagicFlowerMaterialIds.Add(materialId);
                }
            }

            var colliders = instance.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = false;
            var lights = instance.GetComponentsInChildren<Light>(true);
            for (int i = 0; i < lights.Length; i++) lights[i].enabled = false;

            CompletionMagicFlowerInstanceCount++;
            CompletionMagicFlowerRendererCount += renderers.Length;
            CompletionMagicFlowerColliderCount += colliders.Length;
            CompletionMagicFlowerLightCount += lights.Length;
            CompletionMagicFlowerEnabledColliderCount += CountEnabled(colliders);
            CompletionMagicFlowerEnabledLightCount += CountEnabled(lights);
            return instance.transform;
        }

        void BuildReadCompletion()
        {
            const float bookX = 0.55f;
            CompletionPrimitive(PrimitiveType.Cube, "RememberedBookSpine", new Vector3(bookX, 0.83f, 0.04f),
                new Vector3(0.065f, 0.055f, 0.48f), new Color(0.50f, 0.18f, 0.32f), 0.04f);
            var leftCover = CompletionPrimitive(PrimitiveType.Cube, "RememberedBookLeft", new Vector3(bookX - 0.20f, 0.85f, 0.04f),
                new Vector3(0.38f, 0.030f, 0.48f), new Color(0.98f, 0.91f, 0.72f), 0.07f);
            var rightCover = CompletionPrimitive(PrimitiveType.Cube, "RememberedBookRight", new Vector3(bookX + 0.20f, 0.85f, 0.04f),
                new Vector3(0.38f, 0.030f, 0.48f), new Color(0.98f, 0.91f, 0.72f), 0.07f);
            leftCover.localRotation = Quaternion.Euler(0f, 0f, 12f);
            rightCover.localRotation = Quaternion.Euler(0f, 0f, -12f);
            var bookmark = CompletionPrimitive(PrimitiveType.Cube, "RememberedBookmark", new Vector3(bookX + 0.04f, 0.89f, -0.11f),
                new Vector3(0.035f, 0.012f, 0.28f), new Color(0.96f, 0.58f, 0.20f), 0.12f);
            bookmark.localRotation = Quaternion.Euler(0f, 0f, -5f);
            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.PI * 2f / 6f;
                var mote = CompletionPrimitive(PrimitiveType.Sphere, $"RememberedStar-{i + 1}",
                    new Vector3(bookX + Mathf.Cos(angle) * 0.42f, 1.03f + (i % 3) * 0.11f,
                        0.04f + Mathf.Sin(angle) * 0.22f), Vector3.one * 0.052f,
                    i % 2 == 0 ? new Color(1f, 0.86f, 0.34f) : new Color(0.66f, 0.84f, 1f), 0.22f);
                AddAnimated(mote);
            }
        }

        Transform CompletionPrimitive(PrimitiveType type, string name, Vector3 localPosition,
            Vector3 localScale, Color color, float emission)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(_completionRoot.transform, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            var collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                Destroy(collider);
            }
            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = CompletionMaterial(color, emission);
            return go.transform;
        }

        Material CompletionMaterial(Color color, float emission)
        {
            var color32 = (Color32)color;
            int key = color32.r | color32.g << 8 | color32.b << 16 |
                Mathf.RoundToInt(emission * 100f) << 24;
            if (_completionMaterialCache.TryGetValue(key, out var cachedMaterial))
                return cachedMaterial;

            var shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Sprites/Default");
            var material = new Material(shader) { color = color };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.30f);
            if (material.HasProperty("_EmissionColor") && emission > 0f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emission);
            }
            _completionMaterials.Add(material);
            _completionMaterialCache.Add(key, material);
            return material;
        }

        void AddAnimated(Transform detail)
        {
            _animatedCompletionDetails.Add(detail);
            _animatedBasePositions.Add(detail.localPosition);
            _animatedBaseScales.Add(detail.localScale);
        }

        void Update()
        {
            if (!HasCompletionState) return;
            for (int i = 0; i < _animatedCompletionDetails.Count; i++)
            {
                var detail = _animatedCompletionDetails[i];
                if (detail == null) continue;
                float wave = Mathf.Sin(Time.time * 1.7f + i * 1.31f);
                detail.localPosition = _animatedBasePositions[i] + Vector3.up * (wave * 0.012f);
                detail.localScale = _animatedBaseScales[i] * (1f + wave * 0.055f);
            }
        }

        static int CountEnabled(Collider[] components)
        {
            int count = 0;
            for (int i = 0; i < components.Length; i++)
                if (components[i] != null && components[i].enabled) count++;
            return count;
        }

        static int CountEnabled(Light[] components)
        {
            int count = 0;
            for (int i = 0; i < components.Length; i++)
                if (components[i] != null && components[i].enabled) count++;
            return count;
        }

        public static MoonlightActivityStation FindNearestActive(
            MoonlightSpatialActionKind kind, Vector3 from)
        {
            MoonlightActivityStation nearest = null;
            float nearestDistance = float.PositiveInfinity;
            foreach (var station in FindObjectsByType<MoonlightActivityStation>(FindObjectsSortMode.None))
            {
                if (!station.gameObject.activeInHierarchy || station.Kind != kind || station.VisualRoot == null)
                    continue;
                float distance = (station.transform.position - from).sqrMagnitude;
                if (distance >= nearestDistance) continue;
                nearest = station;
                nearestDistance = distance;
            }
            return nearest;
        }
    }
}
