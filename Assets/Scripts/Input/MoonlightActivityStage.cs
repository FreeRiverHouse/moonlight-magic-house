using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public sealed class MoonlightActivityStage : MonoBehaviour
    {
        const string MagicFlowerResourcePath = "Models/Props/Garden/MagicFlowerBloom";
        const int GardenMagicFlowerRequiredInstances = 5;
        const int GardenMagicFlowerMaxRenderers = 10;
        const float GardenBloomBaseScale = 0.48f;
        const float GardenWaterMaximumXOffset = 0.30f;
        const float GardenWaterMaximumZOffset = 0.22f;
        const float GardenTendMinimumX = -0.44f;
        const float GardenTendMaximumX = 0.24f;
        const float GardenTendMaximumZ = 0.13f;
        const float ActivityLightRange = 3.2f;
        const float ActivityLightSpotAngle = 72f;
        const float ActivityLightBaseIntensity = 0.32f;
        const float ActivityLightPulseIntensity = 0.53f;
        const float CareTowelMinimumX = -0.72f;
        const float CareTowelMaximumX = -0.44f;
        const float CareTowelMinimumZ = -0.08f;
        const float CareTowelMaximumZ = 0.12f;
        const float CareWashMinimumRadius = 0.16f;
        const float CareWashMaximumRadius = 0.28f;
        const float CareCombMinimumX = -0.30f;
        const float CareCombMaximumX = 0.46f;
        const float BakeLoadStart = 0.08f;
        const float BakeLoadEnd = 0.32f;
        const float BakeDoorCloseStart = 0.34f;
        const float BakeDoorCloseEnd = 0.46f;
        const float BakeDoorReopenStart = 0.58f;
        const float BakeDoorReopenEnd = 0.66f;
        const float BakeExtractStart = 0.68f;
        const float BakeExtractEnd = 0.92f;
        const float CookCircleMaximumX = 0.18f;
        const float CookCircleMaximumZ = 0.14f;
        const float CookGestureMinimumY = 0.90f;
        const float CookGestureMaximumY = 0.98f;
        const float CookGestureY = 0.94f;
        const float CookDecorCenterX = 0.30f;
        const float CookDecorCenterZ = 0.16f;
        const float CookDecorMinimumX = -0.35f;
        const float CookDecorMaximumX = 0.75f;
        const float CookDecorMinimumZ = -0.18f;
        const float CookDecorMaximumZ = 0.42f;
        const float ReadPageMaximumX = 0.28f;
        const float ReadPageMaximumZ = 0.22f;
        const float ReadBookmarkMinimumX = -0.19f;
        const float ReadBookmarkMaximumX = 0.29f;
        const float ReadBookmarkMinimumZ = -0.38f;
        const float ReadBookmarkMaximumZ = 0.02f;
        const float ReadFinishMinimumIntensity = 1f;
        const float ReadFinishMaximumIntensity = 1.85f;
        const float PlayContinuationBlendSeconds = 0.24f;
        public const float CookHandoffSeconds = 0.30f;
        public const float CookActionSeconds = 2.25f;
        public const float CookFinalPresentationSeconds = 5.2f;
        public const float CookHandoffProgressFraction =
            CookHandoffSeconds / CookActionSeconds;
        public const float PlayContinuationMaximumDeltaSeconds = 1f / 30f;
        public const string PlayContinuationClockSourceForQA = "Time.unscaledDeltaTime";
        public const string PlayContinuationClockQAMarker =
            "MOONLIGHT_PLAY_CONTINUATION_CLOCK_UNSCALED_CAPPED";
        static readonly Vector3 CookDecorParkedPosition = new(-0.43f, 0.72f, 0.30f);
        static readonly string[] CookFallbackBaseNames =
        {
            "KitchenCounterFallback", "CounterClothFallback"
        };
        static readonly Vector3[] CookFallbackBasePositions =
        {
            new(0f, 0.20f, 0.02f), new(0f, 0.38f, 0.02f)
        };
        static readonly Vector3[] CookFallbackBaseScales =
        {
            new(1.70f, 0.34f, 0.86f), new(1.58f, 0.035f, 0.76f)
        };
        static readonly Vector3[] GardenWateringCanBasePositions =
        {
            new(0.58f, 0.31f, -0.18f),
            new(0.35f, 0.37f, -0.15f),
            new(0.20f, 0.43f, -0.13f),
            new(0.72f, 0.36f, -0.18f)
        };
        public const float PlayMinimumThrowExtent = 1.05f;
        public const float PlayMaximumThrowExtent = 2.30f;
        public const float PlayMinimumJumpExtent = 0.80f;
        public const float PlayMaximumJumpExtent = 1.80f;
        public const float PlayMinimumJumpHeight = 0.72f;
        public const float PlayMaximumJumpHeight = 1.18f;
        public const float PlayCatchContactProgress = 0.38f;
        public const int RequiredAuthoritativePlayBallCount = 1;
        public const int PlayPhaseCount = 4;
        public const int RequiredPlayPhaseLandmarkCount = 11;
        public const int PlayPhaseLandmarkMaterialBudget = 8;
        public const int RequiredPlayFallbackBaseObjectCount = 3;
        public const int PlayAuthoredGeneratedMaterialBudget = 18;
        public const int PlayFallbackGeneratedMaterialBudget = 20;
        public const int PlayAuthoredArenaMaterialBudget = 9;
        public const int PlayRendererBudget = 48;
        public const int PlayMaterialBudget = 28;
        public const int PlayLightBudget = 1;
        public const int RequiredAuthoritativePlayTrailCount = 1;
        public const float PlayBallMaximumHorizontalRadius = 0.165f;
        public const float PlayBallMaximumVerticalRadius = 0.135f;
        public const float PlayCatchArchMinimumVisualClearance = 0.095f;
        public const string PlayPhaseLandmarkStaticQAMarker =
            "MOONLIGHT_PLAY_PHASE_LANDMARK_STATIC_CONTRACT_VERIFIED";
        public const string PlayPhaseLandmarkRuntimeQAMarker =
            "MOONLIGHT_PLAY_PHASE_LANDMARK_RUNTIME_VERIFIED";
        public static readonly Vector3 PlayCatchPoint = new(0.94f, 0.54f, -0.46f);
        static readonly string[] PlayPhaseLandmarkNames =
        {
            "ToyWand", "ToyWandStar", "ToyHoop", "FinishFlagPole", "FinishFlag",
            "JumpArchLeftPost", "JumpArchRightPost", "JumpArchTop",
            "CatchArchLeftPost", "CatchArchRightPost", "CatchArchTop"
        };
        static readonly Vector3[] PlayPhaseLandmarkPositions =
        {
            new(-0.45f, 0.16f, -0.48f),
            new(-0.60f, 0.33f, -0.57f),
            new(-0.82f, 0.13f, -0.08f),
            new(1.04f, 0.33f, -0.32f),
            new(0.92f, 0.50f, -0.32f),
            new(-0.48f, 0.33f, 0.38f),
            new(0.48f, 0.33f, 0.38f),
            new(0f, 0.62f, 0.34f),
            new(0.65f, 0.32f, -0.46f),
            new(1.23f, 0.32f, -0.46f),
            new(0.94f, 0.82f, -0.46f)
        };
        static readonly Vector3[] PlayPhaseLandmarkScales =
        {
            new(0.035f, 0.36f, 0.035f),
            Vector3.one * 0.11f,
            new(0.20f, 0.018f, 0.20f),
            new(0.035f, 0.48f, 0.035f),
            new(0.24f, 0.12f, 0.025f),
            new(0.055f, 0.50f, 0.055f),
            new(0.055f, 0.50f, 0.055f),
            new(1.02f, 0.055f, 0.055f),
            new(0.050f, 0.46f, 0.050f),
            new(0.050f, 0.46f, 0.050f),
            new(0.66f, 0.050f, 0.050f)
        };
        static readonly int[] PlayPhaseLandmarkVisibilityMasks =
        {
            (1 << 0) | (1 << 1),
            1 << 2,
            (1 << 5) | (1 << 6) | (1 << 7),
            (1 << 3) | (1 << 4) | (1 << 8) | (1 << 9) | (1 << 10)
        };
        static readonly string[] PlayFallbackBaseNames =
        {
            "PlayMatFallback", "TargetOuterRingFallback", "TargetInnerDotFallback"
        };
        static readonly Vector3[] PlayFallbackBasePositions =
        {
            new(0f, 0.035f, 0f),
            new(0.86f, 0.07f, -0.18f),
            new(0.86f, 0.085f, -0.18f)
        };
        static readonly Vector3[] PlayFallbackBaseScales =
        {
            new(2.15f, 0.025f, 1.24f),
            new(0.46f, 0.012f, 0.46f),
            new(0.20f, 0.012f, 0.20f)
        };
        public const int CookPhaseCount = 4;
        public const int CookRequiredPhaseMask = (1 << CookPhaseCount) - 1;
        public const int CookRendererBudget = 36;
        public const int CookMaterialBudget = 24;
        public const int CookLightBudget = 1;
        public const int GardenRendererBudget = 48;
        public const int GardenMaterialBudget = 28;
        public const int GardenLightBudget = 1;
        public const int ReadRendererBudget = 48;
        public const int ReadMaterialBudget = 28;
        public const int ReadLightBudget = 1;
        public const float ReadMinimumLightIntensity = ActivityLightBaseIntensity;
        public const int RequiredReadStageRendererCount = 19;
        public const int RequiredReadStageMaterialCount = 7;
        public const float CareFinalPresentationSeconds = 4.6f;
        public const float CarePrepMinimumLandingSeparation = 0.10f;
        public const float CareWashMinimumRadiusDelta = 0.08f;
        public const float CareBrushMinimumEndpointSeparation = 0.50f;
        public const float CareGlowMinimumAuraScaleDelta = 0.08f;
        public const float CareGlowMinimumLightIntensityDelta = 0.15f;
        public const int CareGlowMinimumMoteCountDelta = 1;
        public const int BedtimeVariantCount = 2;
        public const int BedtimeAllocatedRendererBudget = 8;
        public const int BedtimeVisibleRendererCount = 5;
        public const int BedtimeMaterialBudget = 5;
        public const int BedtimeLightBudget = 1;
        public const int BedtimeColliderBudget = 0;
        public const float BedtimeLingerSeconds = 2.0f;
        public const string CookAddChoreographyReadyMarker =
            "MOONLIGHT_COOK_ADD_CHOREOGRAPHY_READY";
        public const string CookStirChoreographyReadyMarker =
            "MOONLIGHT_COOK_STIR_CHOREOGRAPHY_READY";
        public const string CookBakeChoreographyReadyMarker =
            "MOONLIGHT_COOK_BAKE_CHOREOGRAPHY_READY";
        public const string CookPresentChoreographyReadyMarker =
            "MOONLIGHT_COOK_PRESENT_CHOREOGRAPHY_READY";
        public const string CookChoreographyIncompleteMarker =
            "MOONLIGHT_COOK_CHOREOGRAPHY_INCOMPLETE";
        public const string CookGesturePersonalizedResultMarker =
            "MOONLIGHT_COOK_GESTURE_RESULT_PERSONALIZED";
        public const string CookGestureIncompleteResultMarker =
            "MOONLIGHT_COOK_GESTURE_RESULT_INCOMPLETE";

        enum ActivitySurfaceProfile
        {
            Matte,
            Fabric,
            Wood,
            Ceramic,
            Metal,
            Glass,
            Magic
        }

        readonly struct BedtimePartSpec
        {
            public readonly PrimitiveType Primitive;
            public readonly string Name;
            public readonly Vector3 Position;
            public readonly Vector3 Scale;
            public readonly Vector3 Euler;
            public readonly Color Color;
            public readonly float Emission;
            public readonly int MaterialSlot;
            public readonly bool RestingVisible;
            public readonly bool CuddledVisible;

            public BedtimePartSpec(PrimitiveType primitive, string name, Vector3 position,
                Vector3 scale, Vector3 euler, Color color, float emission, int materialSlot,
                bool restingVisible, bool cuddledVisible)
            {
                Primitive = primitive;
                Name = name;
                Position = position;
                Scale = scale;
                Euler = euler;
                Color = color;
                Emission = emission;
                MaterialSlot = materialSlot;
                RestingVisible = restingVisible;
                CuddledVisible = cuddledVisible;
            }
        }

        static readonly BedtimePartSpec[] BedtimeParts =
        {
            new(PrimitiveType.Cube, "BedtimeBedFrame", new Vector3(0f, 0.08f, 0.12f),
                new Vector3(1.72f, 0.12f, 1.02f), Vector3.zero,
                new Color(0.34f, 0.25f, 0.42f), 0f, 0, true, true),
            new(PrimitiveType.Cube, "BedtimeBlanket", new Vector3(0.08f, 0.20f, 0.12f),
                new Vector3(1.38f, 0.13f, 0.78f), new Vector3(0f, 0f, -2f),
                new Color(0.29f, 0.54f, 0.68f), 0f, 1, true, true),
            new(PrimitiveType.Cube, "RestingPillow", new Vector3(-0.48f, 0.32f, 0.12f),
                new Vector3(0.48f, 0.14f, 0.52f), new Vector3(0f, 0f, -8f),
                new Color(0.84f, 0.88f, 0.94f), 0f, 2, true, false),
            new(PrimitiveType.Cylinder, "RestingDreamMoon", new Vector3(-0.52f, 1.05f, 0.16f),
                new Vector3(0.31f, 0.045f, 0.31f), new Vector3(90f, 0f, 0f),
                new Color(0.76f, 0.84f, 1f), 0.35f, 3, true, false),
            new(PrimitiveType.Cube, "RestingDreamStar", new Vector3(-0.06f, 0.89f, 0.13f),
                new Vector3(0.15f, 0.15f, 0.06f), new Vector3(0f, 0f, 45f),
                new Color(0.76f, 0.84f, 1f), 0.35f, 3, true, false),
            new(PrimitiveType.Sphere, "CuddledHeartLeft", new Vector3(-0.16f, 0.70f, 0.12f),
                new Vector3(0.42f, 0.40f, 0.18f), Vector3.zero,
                new Color(1f, 0.48f, 0.68f), 0.30f, 4, false, true),
            new(PrimitiveType.Sphere, "CuddledHeartRight", new Vector3(0.16f, 0.70f, 0.12f),
                new Vector3(0.42f, 0.40f, 0.18f), Vector3.zero,
                new Color(1f, 0.48f, 0.68f), 0.30f, 4, false, true),
            new(PrimitiveType.Cube, "CuddledHeartPoint", new Vector3(0f, 0.51f, 0.12f),
                new Vector3(0.40f, 0.40f, 0.18f), new Vector3(0f, 0f, 45f),
                new Color(1f, 0.48f, 0.68f), 0.30f, 4, false, true)
        };

        readonly List<Material> _materials = new();
        readonly Dictionary<MaterialKey, Material> _materialCache = new();
        readonly HashSet<ActivitySurfaceProfile> _configuredSurfaceProfiles = new();
        readonly List<Renderer> _renderers = new();
        readonly List<Material> _sharedMaterialBuffer = new();
        readonly List<Light> _stageLights = new();
        readonly List<Light> _lightBuffer = new();
        readonly HashSet<int> _cookMaterialIdentityBuffer = new();
        readonly HashSet<int> _gardenMagicFlowerMaterialIds = new();
        static readonly Dictionary<PrimitiveType, Mesh> BedtimePrimitiveMeshes = new();
        bool _cookBakeDoorClearancePass = true;
        int[] _activeMaterialIds = System.Array.Empty<int>();
        GameObject _root;
        Transform _bowl;
        Transform _bowlRim;
        Transform _batter;
        Transform _whisk;
        Transform[] _ingredients;
        Transform[] _pourStreams;
        Transform[] _steam;
        Transform[] _cookies;
        Transform[] _cookProps;
        Transform[] _cookieDetails;
        Transform[] _ovenProps;
        Transform[] _decorateProps;
        Transform[] _servingProps;
        Transform _authoredCookWorkbench;
        Transform[] _cookFallbackBase;
        Transform _ball;
        Transform[] _blocks;
        Transform[] _playProps;
        Transform[] _starDetails;
        Transform[] _pathMarkers;
        Transform[] _celebrationStars;
        Transform[] _playArches;
        Transform[] _podiumProps;
        Transform _authoredPlayArena;
        Transform[] _playFallbackBase;
        Transform[] _gardenProps;
        Transform[] _seeds;
        Transform[] _sprouts;
        Transform[] _flowers;
        Transform[] _gardenSparkles;
        Transform _authoredGardenAtelier;
        GameObject _magicFlowerPrefab;
        Transform[] _bookProps;
        Transform[] _pageFlips;
        Transform[] _readMotes;
        Transform _bookmark;
        Transform _authoredReadingNook;
        Transform[] _careProps;
        Transform[] _careBubbles;
        Transform[] _careMotes;
        Transform _careTowelTray;
        Transform _careTowel;
        Transform _careBrush;
        Transform _careComb;
        Transform _careMirror;
        Transform _careMirrorAura;
        Transform _authoredCareStation;
        Transform[] _bedtimeParts;
        Renderer _playBallRenderer;
        TrailRenderer _ballTrail;
        Light _activityLight;
        MoonlightActivityStation _persistentStation;
        Coroutine _lingerRoutine;
        float _lingerUntil;
        bool _applyPersistentCompletionOnEnd;
        bool _careLiveHarnessIsolationEnabledForQA;
        bool _lingerCompletingNaturally;
        Vector3 _center;
        int _requiredSteps = 1;
        float _playProgress;
        bool _isHoldingPlayStepTerminal;
        bool _isHoldingCookStepTerminal;
        bool _cookHandoffActive;
        int _cookHandoffFromStep = -1;
        int _cookZoneInstanceId;
        readonly List<Transform> _cookHandoffSharedProps = new();
        readonly List<Vector3> _cookHandoffSharedPositions = new();
        readonly List<Quaternion> _cookHandoffSharedRotations = new();
        readonly List<Vector3> _cookHandoffSharedScales = new();
        Vector3 _cookBowlHandoffPosition;
        bool _playContinuationActive;
        bool _playContinuationFirstRenderedFramePending;
        int _playContinuationBeginFrame;
        int _playContinuationLastAdvancedFrame;
        int _playZoneInstanceId;
        float _playContinuationElapsed;
        Vector3 _playContinuationStart;
        float _gardenProgress;
        float _readProgress;
        float _careProgress;
        float _bedtimeProgress;
        string _bedtimeState = "";
        MoonlightGestureSample _gestureSample;

        public bool IsVisible => _root != null;
        public bool IsLingering { get; private set; }
        public float LingerSecondsRemaining => IsLingering
            ? Mathf.Max(0f, _lingerUntil - Time.time)
            : 0f;
        public bool CareLiveHarnessIsolationEnabledForQA =>
            _careLiveHarnessIsolationEnabledForQA;
        public bool HasPersistentStationBindingForQA => _persistentStation != null;
        public int PersistentStationBindingCountForQA { get; private set; }
        public int PersistentStationResetCountForQA { get; private set; }
        public int PersistentCompletionApplicationCountForQA { get; private set; }
        public string CareLiveHarnessIsolationQAMarker =>
            _careLiveHarnessIsolationEnabledForQA && _persistentStation == null &&
            PersistentStationBindingCountForQA == 0 &&
            PersistentStationResetCountForQA == 0 &&
            PersistentCompletionApplicationCountForQA == 0 &&
            UsesProceduralCareStationFallback &&
            CareStationVisualSource == "stage-procedural-fallback"
                ? "MOONLIGHT_CARE_LIVE_HARNESS_PERSISTENT_ISOLATED"
                : "MOONLIGHT_CARE_LIVE_HARNESS_PERSISTENT_NOT_ISOLATED";
        public float LastCareLingerRequestedSecondsForQA { get; private set; }
        public float LastCareLingerStartedAtSecondsForQA { get; private set; }
        public float LastCareLingerEndedAtSecondsForQA { get; private set; }
        public float LastCareLingerObservedSecondsForQA { get; private set; }
        public bool LastCareLingerCompletedNaturallyForQA { get; private set; }
        public float LastBedtimeLingerRequestedSecondsForQA { get; private set; }
        public float LastBedtimeLingerStartedAtSecondsForQA { get; private set; }
        public float LastBedtimeLingerEndedAtSecondsForQA { get; private set; }
        public float LastBedtimeLingerObservedSecondsForQA { get; private set; }
        public bool LastBedtimeLingerCompletedNaturallyForQA { get; private set; }
        public string LastBedtimeLingerStateForQA { get; private set; } = "";
        public MoonlightSpatialActionKind CurrentKind { get; private set; }
        public int CurrentStep { get; private set; }
        public int ActiveRendererCount { get; private set; }
        public int ActiveUniqueMaterialCount { get; private set; }
        public int ActiveLightCount { get; private set; }
        public string BedtimeStateForQA => _bedtimeState;
        public string BedtimeLayoutSignatureForQA => BedtimeLayoutSignatureFor(_bedtimeState);
        public int BedtimeAllocatedRendererCountForQA => _root != null &&
            CurrentKind == MoonlightSpatialActionKind.SleepCuddle
                ? _root.GetComponentsInChildren<Renderer>(true).Length
                : 0;
        public int BedtimeVisibleRendererCountForQA => CountVisibleBedtimeParts();
        public int BedtimeAllocatedMaterialCountForQA =>
            CurrentKind == MoonlightSpatialActionKind.SleepCuddle ? _materials.Count : 0;
        public int BedtimeColliderCountForQA => _root != null &&
            CurrentKind == MoonlightSpatialActionKind.SleepCuddle
                ? _root.GetComponentsInChildren<Collider>(true).Length
                : 0;
        public MoonlightGestureSample ActiveGestureSample => _gestureSample;
        public int CookStageRootInstanceId =>
            CurrentKind == MoonlightSpatialActionKind.Cook && _root != null
                ? _root.GetInstanceID()
                : 0;
        public int CookStageLightInstanceId =>
            CurrentKind == MoonlightSpatialActionKind.Cook && _activityLight != null
                ? _activityLight.GetInstanceID()
                : 0;
        public int CookStageMaterialIdentityCountForQA =>
            CurrentKind == MoonlightSpatialActionKind.Cook
                ? CountCookStageMaterialIdentities(out _)
                : 0;
        public int CookStageMaterialIdentityHashForQA
        {
            get
            {
                if (CurrentKind != MoonlightSpatialActionKind.Cook) return 0;
                CountCookStageMaterialIdentities(out int identityHash);
                return identityHash;
            }
        }
        public bool IsHoldingCookStepTerminal => _isHoldingCookStepTerminal &&
            CurrentKind == MoonlightSpatialActionKind.Cook && _root != null;
        public bool IsCookHandoffActive => _cookHandoffActive &&
            CurrentKind == MoonlightSpatialActionKind.Cook && _root != null;
        public int CookStageBuildCountForQA { get; private set; }
        public int CookHandoffCountForQA { get; private set; }
        public float LastCookSharedPropDiscontinuityForQA { get; private set; }
        public int LastCookSharedPropCountForQA { get; private set; }
        public bool CookIntermediateResultVisibleForQA => IsHoldingCookStepTerminal &&
            CurrentStep < CookPhaseCount - 1 && CookCurrentPhaseProgress == 1f &&
            CookCurrentPhaseStateReady && CookCurrentPhaseVisibleMotionPropCount >=
                CookPhaseMinimumVisibleMotionPropCount(CurrentStep);
        public Vector3 PlayBallLocalPosition => _ball != null
            ? _ball.localPosition
            : new Vector3(float.NaN, float.NaN, float.NaN);
        public int PlayStageRootInstanceId => CurrentKind == MoonlightSpatialActionKind.Play &&
            _root != null ? _root.GetInstanceID() : 0;
        public int PlayBallInstanceId => _ball != null ? _ball.GetInstanceID() : 0;
        public int PlayTrailInstanceId => _ballTrail != null ? _ballTrail.GetInstanceID() : 0;
        public int PlayBallSharedMaterialInstanceId
        {
            get
            {
                Material material = _playBallRenderer != null
                    ? _playBallRenderer.sharedMaterial
                    : null;
                return material != null ? material.GetInstanceID() : 0;
            }
        }
        public int PlayTrailSharedMaterialInstanceId
        {
            get
            {
                Material material = _ballTrail != null ? _ballTrail.sharedMaterial : null;
                return material != null ? material.GetInstanceID() : 0;
            }
        }
        public bool IsHoldingPlayStepTerminal => _isHoldingPlayStepTerminal &&
            CurrentKind == MoonlightSpatialActionKind.Play && _root != null && _ball != null;
        public bool IsPlayContinuationBlending => _playContinuationActive &&
            CurrentKind == MoonlightSpatialActionKind.Play && _root != null && _ball != null;
        public int PlayContinuationCountForQA { get; private set; }
        public float LastPlayContinuationDiscontinuityForQA { get; private set; }
        public float LastPlayContinuationClockDeltaForQA { get; private set; }
        public int AuthoritativePlayBallCount => CountPlayObjectsNamed("StarBall");
        public int AuthoritativePlayTrailCount => CurrentKind == MoonlightSpatialActionKind.Play &&
            _root != null ? _root.GetComponentsInChildren<TrailRenderer>(true).Length : 0;
        public int PlayPhaseLandmarkObjectCountForQA => CountPlayPhaseLandmarks();
        public int PlayPhaseLandmarkNamedObjectCountForQA =>
            CountNamedPlayPhaseLandmarks();
        public int PlayPhaseLandmarkRendererCountForQA =>
            CountPlayPhaseLandmarkComponents<Renderer>(false);
        public int PlayPhaseLandmarkMaterialCountForQA =>
            CountPlayPhaseLandmarkMaterials();
        public int PlayPhaseLandmarkColliderCountForQA =>
            CountPlayPhaseLandmarkComponents<Collider>(false);
        public int PlayPhaseLandmarkEnabledColliderCountForQA =>
            CountPlayPhaseLandmarkComponents<Collider>(true);
        public int PlayPhaseLandmarkLightCountForQA =>
            CountPlayPhaseLandmarkComponents<Light>(false);
        public int PlayPhaseLandmarkScriptCountForQA =>
            CountPlayPhaseLandmarkComponents<MonoBehaviour>(false);
        public int PlayPhaseLandmarkVisibleMaskForQA =>
            CurrentPlayPhaseLandmarkVisibleMask();
        public int PlayPhaseLandmarkExpectedVisibleMaskForQA =>
            PlayPhaseExpectedVisibilityMask(CurrentStep);
        public bool PlayBallRootAndTrailIdentityReadyForQA =>
            CurrentKind == MoonlightSpatialActionKind.Play && _root != null &&
            _ball != null && _ball.parent == _root.transform && _ball.name == "StarBall" &&
            _ballTrail != null && _ballTrail.transform == _ball;
        public bool PlayCatchIsHeld => CurrentKind == MoonlightSpatialActionKind.Play &&
            CurrentStep == 3 && _playProgress >= PlayCatchContactProgress && _ball != null &&
            Vector3.Distance(_ball.localPosition, PlayCatchPoint) <= 0.0001f;
        public bool PlayTrajectoryRuntimeReady => CurrentKind == MoonlightSpatialActionKind.Play &&
            AuthoritativePlayBallCount == 1 && _gestureSample.PointCount == 7 &&
            _gestureSample.HasSevenFiniteNormalizedPoints &&
            AuthoritativePlayTrailCount == 1 &&
            PlayPointIsFiniteAndBounded(PlayBallLocalPosition) &&
            (CurrentStep != 3 || _playProgress < PlayCatchContactProgress || PlayCatchIsHeld);
        public string PlayTrajectoryQAMarker => PlayTrajectoryRuntimeReady
            ? "MOONLIGHT_GESTURE_PLAY_TRAJECTORY_READY"
            : "MOONLIGHT_GESTURE_PLAY_TRAJECTORY_INVALID";

        int CountPlayObjectsNamed(string objectName)
        {
            if (CurrentKind != MoonlightSpatialActionKind.Play || _root == null) return 0;
            int count = 0;
            foreach (Transform candidate in _root.GetComponentsInChildren<Transform>(true))
                if (candidate != null && candidate.name == objectName) count++;
            return count;
        }

        int CountNamedPlayObjects(string[] objectNames)
        {
            int count = 0;
            if (objectNames == null) return count;
            for (int i = 0; i < objectNames.Length; i++)
                count += CountPlayObjectsNamed(objectNames[i]);
            return count;
        }

        public static string PlayPhaseLandmarkName(int index) =>
            index >= 0 && index < PlayPhaseLandmarkNames.Length
                ? PlayPhaseLandmarkNames[index]
                : "invalid";

        public static int PlayPhaseExpectedVisibilityMask(int phaseIndex) =>
            phaseIndex >= 0 && phaseIndex < PlayPhaseLandmarkVisibilityMasks.Length
                ? PlayPhaseLandmarkVisibilityMasks[phaseIndex]
                : 0;

        public static int PlayPhaseExpectedVisibleLandmarkCount(int phaseIndex)
        {
            int mask = PlayPhaseExpectedVisibilityMask(phaseIndex);
            int count = 0;
            while (mask != 0)
            {
                count += mask & 1;
                mask >>= 1;
            }
            return count;
        }

        public bool ValidatePlayPhaseLandmarkRuntimeContract(out string detail)
        {
            int objectCount = PlayPhaseLandmarkObjectCountForQA;
            int namedObjectCount = PlayPhaseLandmarkNamedObjectCountForQA;
            int rendererCount = PlayPhaseLandmarkRendererCountForQA;
            int materialCount = PlayPhaseLandmarkMaterialCountForQA;
            int colliderCount = PlayPhaseLandmarkColliderCountForQA;
            int enabledColliderCount = PlayPhaseLandmarkEnabledColliderCountForQA;
            int lightCount = PlayPhaseLandmarkLightCountForQA;
            int scriptCount = PlayPhaseLandmarkScriptCountForQA;
            int visibleMask = PlayPhaseLandmarkVisibleMaskForQA;
            int expectedMask = PlayPhaseLandmarkExpectedVisibleMaskForQA;
            bool transformsPass = PlayPhaseLandmarkTransformsMatchContract();
            bool identityPass = PlayBallRootAndTrailIdentityReadyForQA &&
                AuthoritativePlayBallCount == RequiredAuthoritativePlayBallCount &&
                AuthoritativePlayTrailCount == RequiredAuthoritativePlayTrailCount;
            bool budgetPass = PlaySourceBudgetReadyForQA();
            bool pass = CurrentKind == MoonlightSpatialActionKind.Play &&
                objectCount == RequiredPlayPhaseLandmarkCount &&
                namedObjectCount == RequiredPlayPhaseLandmarkCount &&
                rendererCount == RequiredPlayPhaseLandmarkCount &&
                materialCount > 0 && materialCount <= PlayPhaseLandmarkMaterialBudget &&
                colliderCount == 0 && enabledColliderCount == 0 &&
                lightCount == 0 && scriptCount == 0 && transformsPass &&
                visibleMask == expectedMask && identityPass && budgetPass;
            detail = $"source={(HasAuthoredPlayArena ? "authored-base" : "fallback-base")} " +
                $"phase={Mathf.Clamp(CurrentStep, 0, PlayPhaseCount - 1) + 1}/" +
                $"{PlayPhaseCount} objects={objectCount}/{RequiredPlayPhaseLandmarkCount} " +
                $"named={namedObjectCount}/{RequiredPlayPhaseLandmarkCount} " +
                $"visible=0x{visibleMask:X3}/0x{expectedMask:X3} " +
                $"renderers={rendererCount} materials={materialCount} " +
                $"colliders={colliderCount}/{enabledColliderCount} lights={lightCount} " +
                $"scripts={scriptCount} transforms={transformsPass} identity={identityPass} " +
                $"budget={ActiveRendererCount}/{PlayRendererBudget}r," +
                $"{ActiveUniqueMaterialCount}/{PlayArenaMaterialCeilingForQA}/" +
                $"{PlayMaterialBudget}m generated={PlayGeneratedMaterialCountForQA}," +
                $"{ActiveLightCount}/{PlayLightBudget}l";
            return pass;
        }

        public bool ValidatePlayArenaSourceRuntimeContract(out string detail)
        {
            bool authored = HasAuthoredPlayArena;
            bool authoredPass = authored && _playFallbackBase == null &&
                AuthoredPlayArenaRendererCount >= 7 &&
                AuthoredPlayArenaRendererCount <= 10 &&
                AuthoredPlayArenaMaterialCount >= 7 &&
                AuthoredPlayArenaMaterialCount <= PlayAuthoredArenaMaterialBudget &&
                AuthoredPlayArenaColliderCount == 0 &&
                AuthoredPlayArenaLightCount == 0 &&
                AuthoredPlayArenaBoundsSize.x >= 2.70f &&
                AuthoredPlayArenaBoundsSize.y >= 0.55f &&
                AuthoredPlayArenaBoundsSize.y <= 1.25f &&
                AuthoredPlayArenaBoundsSize.z >= 1.10f;
            bool fallbackTransformsPass = PlayFallbackBaseTransformsMatchContract();
            bool fallbackPass = !authored && UsesProceduralPlayArenaFallback &&
                PlayFallbackBaseObjectCountForQA == RequiredPlayFallbackBaseObjectCount &&
                PlayFallbackBaseNamedObjectCountForQA == RequiredPlayFallbackBaseObjectCount &&
                PlayFallbackBaseRendererCountForQA == RequiredPlayFallbackBaseObjectCount &&
                PlayFallbackBaseMaterialCountForQA == RequiredPlayFallbackBaseObjectCount &&
                PlayFallbackBaseColliderCountForQA == 0 &&
                PlayFallbackBaseEnabledColliderCountForQA == 0 &&
                PlayFallbackBaseLightCountForQA == 0 &&
                PlayFallbackBaseScriptCountForQA == 0 && fallbackTransformsPass;
            bool sourcePass = authored ? authoredPass : fallbackPass;
            bool budgetPass = PlaySourceBudgetReadyForQA();
            detail = $"source={PlayArenaVisualSourceForQA} valid={sourcePass} " +
                $"authored={AuthoredPlayArenaRendererCount}r/" +
                $"{AuthoredPlayArenaMaterialCount}m/" +
                $"{AuthoredPlayArenaColliderCount}c/{AuthoredPlayArenaLightCount}l " +
                $"bounds={AuthoredPlayArenaBoundsSize:F2} fallback=" +
                $"{PlayFallbackBaseObjectCountForQA}o/" +
                $"{PlayFallbackBaseNamedObjectCountForQA}n/" +
                $"{PlayFallbackBaseRendererCountForQA}r/" +
                $"{PlayFallbackBaseMaterialCountForQA}m/" +
                $"{PlayFallbackBaseColliderCountForQA}c/" +
                $"{PlayFallbackBaseLightCountForQA}l/" +
                $"{PlayFallbackBaseScriptCountForQA}s transforms={fallbackTransformsPass} " +
                $"materials={ActiveUniqueMaterialCount}/" +
                $"{PlayArenaMaterialCeilingForQA}/{PlayMaterialBudget} " +
                $"generated={PlayGeneratedMaterialCountForQA}/" +
                $"{(authored ? PlayAuthoredGeneratedMaterialBudget : PlayFallbackGeneratedMaterialBudget)} " +
                $"budget={budgetPass}";
            return CurrentKind == MoonlightSpatialActionKind.Play && sourcePass && budgetPass;
        }

        bool PlaySourceBudgetReadyForQA()
        {
            int generatedBudget = HasAuthoredPlayArena
                ? PlayAuthoredGeneratedMaterialBudget
                : PlayFallbackGeneratedMaterialBudget;
            return CurrentKind == MoonlightSpatialActionKind.Play &&
                ActiveRendererCount > 0 && ActiveRendererCount <= PlayRendererBudget &&
                PlayGeneratedMaterialCountForQA > 0 &&
                PlayGeneratedMaterialCountForQA <= generatedBudget &&
                ActiveUniqueMaterialCount > 0 &&
                ActiveUniqueMaterialCount <= PlayArenaMaterialCeilingForQA &&
                PlayArenaMaterialCeilingForQA <= PlayMaterialBudget &&
                ActiveLightCount == PlayLightBudget;
        }

        int CountPlayPhaseLandmarks()
            => CountTransforms(_playProps) + CountTransforms(_playArches);

        int CountNamedPlayPhaseLandmarks()
            => CountNamedPlayObjects(PlayPhaseLandmarkNames);

        int CountPlayPhaseLandmarkComponents<T>(bool enabledOnly) where T : Component
            => CountTransformComponents<T>(_playProps, enabledOnly) +
                CountTransformComponents<T>(_playArches, enabledOnly);

        int CountPlayPhaseLandmarkMaterials()
        {
            var materialIds = new HashSet<int>();
            AddTransformMaterialIds(_playProps, materialIds);
            AddTransformMaterialIds(_playArches, materialIds);
            return materialIds.Count;
        }

        static int CountTransforms(Transform[] transforms)
        {
            int count = 0;
            if (transforms == null) return count;
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i] != null) count++;
            return count;
        }

        int CountTransformComponents<T>(Transform[] transforms, bool enabledOnly)
            where T : Component
        {
            int count = 0;
            if (transforms == null) return count;
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] == null) continue;
                T[] components = transforms[i].GetComponents<T>();
                for (int componentIndex = 0; componentIndex < components.Length;
                     componentIndex++)
                {
                    bool enabled = components[componentIndex] switch
                    {
                        Behaviour behaviour => behaviour.enabled,
                        Collider collider => collider.enabled,
                        _ => false
                    };
                    if (!enabledOnly || enabled) count++;
                }
            }
            return count;
        }

        int CountTransformMaterials(Transform[] transforms)
        {
            var materialIds = new HashSet<int>();
            AddTransformMaterialIds(transforms, materialIds);
            return materialIds.Count;
        }

        void AddTransformMaterialIds(Transform[] transforms, HashSet<int> materialIds)
        {
            if (transforms == null) return;
            for (int i = 0; i < transforms.Length; i++)
            {
                Renderer renderer = transforms[i] != null
                    ? transforms[i].GetComponent<Renderer>()
                    : null;
                if (renderer == null) continue;
                _sharedMaterialBuffer.Clear();
                renderer.GetSharedMaterials(_sharedMaterialBuffer);
                for (int materialIndex = 0; materialIndex < _sharedMaterialBuffer.Count;
                     materialIndex++)
                    if (_sharedMaterialBuffer[materialIndex] != null)
                        materialIds.Add(_sharedMaterialBuffer[materialIndex].GetInstanceID());
            }
        }

        int CurrentPlayPhaseLandmarkVisibleMask()
        {
            int mask = 0;
            for (int i = 0; i < RequiredPlayPhaseLandmarkCount; i++)
            {
                Transform landmark = PlayPhaseLandmarkAt(i);
                Renderer renderer = landmark != null ? landmark.GetComponent<Renderer>() : null;
                if (renderer != null && renderer.enabled && !renderer.forceRenderingOff &&
                    renderer.gameObject.activeInHierarchy)
                    mask |= 1 << i;
            }
            return mask;
        }

        bool PlayPhaseLandmarkTransformsMatchContract()
        {
            for (int i = 0; i < RequiredPlayPhaseLandmarkCount; i++)
            {
                Transform landmark = PlayPhaseLandmarkAt(i);
                if (landmark == null || landmark.name != PlayPhaseLandmarkNames[i] ||
                    landmark.parent != _root.transform ||
                    Vector3.Distance(landmark.localPosition,
                        PlayPhaseLandmarkPositions[i]) > 0.0001f)
                    return false;
            }
            return true;
        }

        bool PlayFallbackBaseTransformsMatchContract()
        {
            if (_playFallbackBase == null ||
                _playFallbackBase.Length != RequiredPlayFallbackBaseObjectCount)
                return false;
            for (int i = 0; i < _playFallbackBase.Length; i++)
            {
                Transform candidate = _playFallbackBase[i];
                if (candidate == null || candidate.parent != _root.transform ||
                    candidate.name != PlayFallbackBaseNames[i] ||
                    Vector3.Distance(candidate.localPosition,
                        PlayFallbackBasePositions[i]) > 0.0001f ||
                    Vector3.Distance(candidate.localScale,
                        PlayFallbackBaseScales[i]) > 0.0001f)
                    return false;
            }
            return true;
        }

        Transform PlayPhaseLandmarkAt(int index)
        {
            if (index < 0 || index >= RequiredPlayPhaseLandmarkCount) return null;
            return index < 5
                ? _playProps != null && index < _playProps.Length ? _playProps[index] : null
                : _playArches != null && index - 5 < _playArches.Length
                    ? _playArches[index - 5]
                    : null;
        }
        public int ConfiguredSurfaceProfileCount => _configuredSurfaceProfiles.Count;
        public bool HasDepthLighting => _activityLight != null &&
            _activityLight.type == LightType.Spot &&
            _activityLight.shadows == LightShadows.Soft &&
            _activityLight.range >= ActivityLightRange &&
            _activityLight.spotAngle >= ActivityLightSpotAngle &&
            _activityLight.intensity >= ActivityLightBaseIntensity;
        public string SurfaceDepthQAMarker =>
            ConfiguredSurfaceProfileCount >= 3 && HasDepthLighting
                ? "MOONLIGHT_ACTIVITY_SURFACE_DEPTH_READY"
                : "MOONLIGHT_ACTIVITY_SURFACE_DEPTH_INCOMPLETE";

        public static bool ValidateSurfaceDepthContract(out string detail)
        {
            GetSurfaceResponse(ActivitySurfaceProfile.Fabric,
                out float fabricSmoothness, out float fabricMetallic);
            GetSurfaceResponse(ActivitySurfaceProfile.Metal,
                out float metalSmoothness, out float metalMetallic);
            GetSurfaceResponse(ActivitySurfaceProfile.Ceramic,
                out float ceramicSmoothness, out float ceramicMetallic);
            bool pass = fabricSmoothness <= 0.10f && fabricMetallic == 0f &&
                ceramicSmoothness >= 0.50f && ceramicMetallic == 0f &&
                metalSmoothness >= 0.65f && metalMetallic >= 0.65f &&
                metalSmoothness - fabricSmoothness >= 0.60f &&
                ActivityLightRange >= 3f && ActivityLightSpotAngle >= 68f &&
                ActivityLightBaseIntensity >= 0.30f && ActivityLightPulseIntensity >= 0.50f;
            detail = $"fabric={fabricSmoothness:F2}/{fabricMetallic:F2} " +
                $"ceramic={ceramicSmoothness:F2}/{ceramicMetallic:F2} " +
                $"metal={metalSmoothness:F2}/{metalMetallic:F2} " +
                $"contrast={metalSmoothness - fabricSmoothness:F2} " +
                $"spot={ActivityLightRange:F1}m/{ActivityLightSpotAngle:F0}deg " +
                $"intensity={ActivityLightBaseIntensity:F2}+{ActivityLightPulseIntensity:F2}";
            return pass;
        }
        public bool HasAuthoredCookWorkbench => _authoredCookWorkbench != null;
        public bool UsesProceduralCookWorkbenchFallback =>
            CurrentKind == MoonlightSpatialActionKind.Cook && !HasAuthoredCookWorkbench &&
            CountTransforms(_cookFallbackBase) == CookFallbackBaseNames.Length;
        public string CookWorkbenchVisualSourceForQA => HasAuthoredCookWorkbench
            ? "authored"
            : UsesProceduralCookWorkbenchFallback ? "fallback" : "missing";
        public int AuthoredCookWorkbenchRendererCount { get; private set; }
        public int AuthoredCookWorkbenchMaterialCount { get; private set; }
        public int AuthoredCookWorkbenchColliderCount { get; private set; }
        public int AuthoredCookWorkbenchLightCount { get; private set; }
        public int CookChoreographyReadyMask { get; private set; }
        public bool HasCompleteCookChoreography =>
            CookChoreographyReadyMask == CookRequiredPhaseMask;
        public string CookCurrentPhaseName { get; private set; } = "inactive";
        public float CookCurrentPhaseProgress { get; private set; }
        public int CookCurrentPhaseMotionPropCount { get; private set; }
        public int CookCurrentPhaseVisibleMotionPropCount { get; private set; }
        public bool CookCurrentPhaseMotionReady { get; private set; }
        public bool CookCurrentPhaseStateReady { get; private set; }
        public bool CookBakeDoorClearancePass => _cookBakeDoorClearancePass;
        public string CookCurrentPhaseMotionEvidence =>
            $"phase={CookCurrentPhaseName} progress={CookCurrentPhaseProgress:0.000} " +
            $"matched={CookCurrentPhaseMotionPropCount}/" +
            $"{CookPhaseMinimumMotionPropCount(CurrentStep)} visible=" +
            $"{CookCurrentPhaseVisibleMotionPropCount}/" +
            $"{CookPhaseMinimumVisibleMotionPropCount(CurrentStep)} " +
            $"doorClear={_cookBakeDoorClearancePass}";
        public bool CookBudgetReady => CurrentKind == MoonlightSpatialActionKind.Cook &&
            ActiveRendererCount > 0 && ActiveRendererCount <= CookRendererBudget &&
            ActiveUniqueMaterialCount > 0 && ActiveUniqueMaterialCount <= CookMaterialBudget &&
            ActiveLightCount == CookLightBudget;
        public Vector3 CookGesturePropLocalPosition => CurrentStep switch
        {
            1 when _whisk != null => _whisk.localPosition,
            3 when _decorateProps != null && _decorateProps.Length > 0 &&
                _decorateProps[0] != null => _decorateProps[0].localPosition,
            _ => new Vector3(float.NaN, float.NaN, float.NaN)
        };
        public Vector3 CookExpectedGesturePropLocalPosition =>
            EvaluateCookGesturePropPosition(CurrentStep, CookCurrentPhaseProgress,
                _gestureSample);
        public bool CookGesturePathTransformAgreement =>
            CurrentKind == MoonlightSpatialActionKind.Cook &&
            (CurrentStep == 1 || CurrentStep == 3) &&
            IsFinite(CookGesturePropLocalPosition) &&
            CookGesturePropLocalPosition.Equals(CookExpectedGesturePropLocalPosition);
        public bool CookCookieMarksRetainGestureImprint =>
            CurrentKind == MoonlightSpatialActionKind.Cook && CurrentStep == 3 &&
            CookCookieMarkTransformsMatch(CookCurrentPhaseProgress);
        public bool CookGestureHasMinimumPathSpan =>
            CurrentKind == MoonlightSpatialActionKind.Cook &&
            (CurrentStep == 1 || CurrentStep == 3) &&
            CookGestureSampleHasMinimumPathSpan(CurrentStep, _gestureSample);
        public bool CookGestureTraversalDirectionAgreement =>
            CurrentKind == MoonlightSpatialActionKind.Cook &&
            (CurrentStep == 1 || CurrentStep == 3) &&
            CookGestureTraversalMatchesPath(CurrentStep, _gestureSample);
        public int CookDistinctGestureImprintCount =>
            CurrentKind == MoonlightSpatialActionKind.Cook && CurrentStep == 3
                ? CookGestureDistinctImprintCount(_gestureSample)
                : 0;
        public bool CookGestureInputReady => CookGestureHasMinimumPathSpan &&
            CookGestureTraversalDirectionAgreement &&
            (CurrentStep != 3 || CookDistinctGestureImprintCount == 9);
        public string CookGestureResultQAMarker =>
            CookGestureInputReady && CookCookieMarksRetainGestureImprint &&
            AllActive(_cookieDetails)
                ? CookGesturePersonalizedResultMarker
                : CookGestureIncompleteResultMarker;
        public string CookBudgetEvidence =>
            $"renderers={ActiveRendererCount}/{CookRendererBudget} " +
            $"materials={ActiveUniqueMaterialCount}/{CookMaterialBudget} " +
            $"lights={ActiveLightCount}/{CookLightBudget}";
        public string CookPhaseQAMarker =>
            CookCurrentPhaseMotionReady && CookCurrentPhaseStateReady && CookBudgetReady
                ? CookPhaseReadyMarker(CurrentStep)
                : CookChoreographyIncompleteMarker;

        public bool ValidateCookWorkbenchSourceRuntimeContract(out string detail)
        {
            bool authoredPass = HasAuthoredCookWorkbench && _cookFallbackBase == null &&
                AuthoredCookWorkbenchRendererCount >= 8 &&
                AuthoredCookWorkbenchRendererCount <= 12 &&
                AuthoredCookWorkbenchMaterialCount >= 8 &&
                AuthoredCookWorkbenchMaterialCount <= 10 &&
                AuthoredCookWorkbenchColliderCount == 0 &&
                AuthoredCookWorkbenchLightCount == 0;
            bool fallbackPass = !HasAuthoredCookWorkbench &&
                UsesProceduralCookWorkbenchFallback &&
                CookFallbackBaseTransformsMatchContract();
            bool exactlyOneSource = authoredPass != fallbackPass;
            detail = $"source={CookWorkbenchVisualSourceForQA} exactlyOne={exactlyOneSource} " +
                $"authored={AuthoredCookWorkbenchRendererCount}r/" +
                $"{AuthoredCookWorkbenchMaterialCount}m/" +
                $"{AuthoredCookWorkbenchColliderCount}c/" +
                $"{AuthoredCookWorkbenchLightCount}l fallback=" +
                $"{CountTransforms(_cookFallbackBase)}o/" +
                $"{CountTransformComponents<Renderer>(_cookFallbackBase, false)}r/" +
                $"{CountTransformMaterials(_cookFallbackBase)}m/" +
                $"{CountTransformComponents<Collider>(_cookFallbackBase, true)}c/" +
                $"{CountTransformComponents<Light>(_cookFallbackBase, true)}l " +
                $"budget=({CookBudgetEvidence})";
            return CurrentKind == MoonlightSpatialActionKind.Cook && exactlyOneSource &&
                CookBudgetReady;
        }

        bool CookFallbackBaseTransformsMatchContract()
        {
            if (_cookFallbackBase == null ||
                _cookFallbackBase.Length != CookFallbackBaseNames.Length)
                return false;
            for (int i = 0; i < _cookFallbackBase.Length; i++)
            {
                Transform candidate = _cookFallbackBase[i];
                if (candidate == null || candidate.parent != _root.transform ||
                    candidate.name != CookFallbackBaseNames[i] ||
                    Vector3.Distance(candidate.localPosition,
                        CookFallbackBasePositions[i]) > 0.0001f ||
                    Vector3.Distance(candidate.localScale,
                        CookFallbackBaseScales[i]) > 0.0001f ||
                    candidate.GetComponent<Renderer>() == null ||
                    candidate.GetComponent<Collider>() != null ||
                    candidate.GetComponent<Light>() != null)
                    return false;
            }
            return true;
        }

        public static int CookPhaseMinimumMotionPropCount(int phaseIndex) => phaseIndex switch
        {
            0 => 9,
            1 => 5,
            2 => 15,
            3 => 20,
            _ => 0
        };

        public static int CookPhaseMinimumVisibleMotionPropCount(int phaseIndex) => phaseIndex switch
        {
            0 => 6,
            1 => 5,
            2 => 10,
            3 => 12,
            _ => 0
        };

        public static string CookPhaseName(int phaseIndex) => phaseIndex switch
        {
            0 => "add-pour",
            1 => "stir-circle",
            2 => "bake-rise",
            3 => "decorate-present",
            _ => "inactive"
        };

        public static string CookPhaseReadyMarker(int phaseIndex) => phaseIndex switch
        {
            0 => CookAddChoreographyReadyMarker,
            1 => CookStirChoreographyReadyMarker,
            2 => CookBakeChoreographyReadyMarker,
            3 => CookPresentChoreographyReadyMarker,
            _ => CookChoreographyIncompleteMarker
        };

        public static bool ValidateCookChoreographyContract(out string detail)
        {
            int configuredPhaseMask = 0;
            int totalMotionProps = 0;
            for (int phase = 0; phase < CookPhaseCount; phase++)
            {
                int motionProps = CookPhaseMinimumMotionPropCount(phase);
                if (motionProps > 0 && CookPhaseName(phase) != "inactive")
                    configuredPhaseMask |= 1 << phase;
                totalMotionProps += motionProps;
            }

            bool phaseCountsPass = CookPhaseMinimumMotionPropCount(0) == 9 &&
                CookPhaseMinimumMotionPropCount(1) == 5 &&
                CookPhaseMinimumMotionPropCount(2) == 15 &&
                CookPhaseMinimumMotionPropCount(3) == 20 &&
                CookPhaseMinimumVisibleMotionPropCount(0) == 6 &&
                CookPhaseMinimumVisibleMotionPropCount(1) == 5 &&
                CookPhaseMinimumVisibleMotionPropCount(2) == 10 &&
                CookPhaseMinimumVisibleMotionPropCount(3) == 12;
            bool bakeTimingPass = 0f <= BakeLoadStart && BakeLoadStart < BakeLoadEnd &&
                BakeLoadEnd <= BakeDoorCloseStart &&
                BakeDoorCloseStart < BakeDoorCloseEnd &&
                BakeDoorCloseEnd < BakeDoorReopenStart &&
                BakeDoorReopenStart < BakeDoorReopenEnd &&
                BakeDoorReopenEnd <= BakeExtractStart &&
                BakeExtractStart < BakeExtractEnd && BakeExtractEnd <= 1f;
            bool exactBakeTimingPass =
                Mathf.Approximately(BakeLoadStart, 0.08f) &&
                Mathf.Approximately(BakeLoadEnd, 0.32f) &&
                Mathf.Approximately(BakeDoorCloseStart, 0.34f) &&
                Mathf.Approximately(BakeDoorCloseEnd, 0.46f) &&
                Mathf.Approximately(BakeDoorReopenStart, 0.58f) &&
                Mathf.Approximately(BakeDoorReopenEnd, 0.66f) &&
                Mathf.Approximately(BakeExtractStart, 0.68f) &&
                Mathf.Approximately(BakeExtractEnd, 0.92f);
            bool fullBakeClearancePass = true;
            for (int sample = 0; sample <= 100; sample++)
            {
                float progress = sample / 100f;
                if (IsBakeTrayCrossingDoor(progress) && BakeDoorOpen(progress) < 0.999f)
                {
                    fullBakeClearancePass = false;
                    break;
                }
            }
            bool pass = CookPhaseCount == 4 && configuredPhaseMask == CookRequiredPhaseMask &&
                phaseCountsPass && totalMotionProps == 49 && bakeTimingPass &&
                exactBakeTimingPass && fullBakeClearancePass &&
                CookHandoffProgressFraction >= 0.12f &&
                CookHandoffProgressFraction <= 0.15f &&
                Mathf.Approximately(CookFinalPresentationSeconds, 5.2f) &&
                CookRendererBudget > 0 && CookRendererBudget <= 36 &&
                CookMaterialBudget > 0 && CookMaterialBudget <= 24 && CookLightBudget == 1;
            detail = $"phases={CookPhaseCount} mask=0x{configuredPhaseMask:X}/0x{CookRequiredPhaseMask:X} " +
                $"motionProps={CookPhaseMinimumMotionPropCount(0)}," +
                $"{CookPhaseMinimumMotionPropCount(1)}," +
                $"{CookPhaseMinimumMotionPropCount(2)}," +
                $"{CookPhaseMinimumMotionPropCount(3)} total={totalMotionProps} " +
                $"visibleMinimums={CookPhaseMinimumVisibleMotionPropCount(0)}," +
                $"{CookPhaseMinimumVisibleMotionPropCount(1)}," +
                $"{CookPhaseMinimumVisibleMotionPropCount(2)}," +
                $"{CookPhaseMinimumVisibleMotionPropCount(3)} timing={bakeTimingPass} " +
                $"exactTiming={exactBakeTimingPass} fullDoorClearance={fullBakeClearancePass} " +
                $"handoff={CookHandoffSeconds:0.00}s/" +
                $"{CookActionSeconds:0.00}s={CookHandoffProgressFraction:0.000} " +
                $"linger={CookFinalPresentationSeconds:0.0}s " +
                $"budgets={CookRendererBudget}r/{CookMaterialBudget}m/{CookLightBudget}l";
            return pass;
        }
        public bool HasAuthoredPlayArena => _authoredPlayArena != null;
        public bool UsesProceduralPlayArenaFallback =>
            CurrentKind == MoonlightSpatialActionKind.Play && !HasAuthoredPlayArena &&
            PlayFallbackBaseObjectCountForQA == RequiredPlayFallbackBaseObjectCount;
        public string PlayArenaVisualSourceForQA => HasAuthoredPlayArena
            ? "authored"
            : UsesProceduralPlayArenaFallback ? "fallback" : "missing";
        public string PlayArenaSourceQAMarkerForQA => HasAuthoredPlayArena
            ? "MOONLIGHT_AUTHORED_PLAY_ARENA_READY"
            : UsesProceduralPlayArenaFallback
                ? "MOONLIGHT_PLAY_ARENA_FALLBACK_READY"
                : "MOONLIGHT_PLAY_ARENA_SOURCE_MISSING";
        public int PlayGeneratedMaterialCountForQA =>
            CurrentKind == MoonlightSpatialActionKind.Play ? _materials.Count : 0;
        public int PlayArenaMaterialCeilingForQA =>
            CurrentKind != MoonlightSpatialActionKind.Play
                ? 0
                : _materials.Count + (HasAuthoredPlayArena
                    ? AuthoredPlayArenaMaterialCount
                    : 0);
        public int PlayFallbackBaseObjectCountForQA => CountTransforms(_playFallbackBase);
        public int PlayFallbackBaseNamedObjectCountForQA =>
            CountNamedPlayObjects(PlayFallbackBaseNames);
        public int PlayFallbackBaseRendererCountForQA =>
            CountTransformComponents<Renderer>(_playFallbackBase, false);
        public int PlayFallbackBaseMaterialCountForQA =>
            CountTransformMaterials(_playFallbackBase);
        public int PlayFallbackBaseColliderCountForQA =>
            CountTransformComponents<Collider>(_playFallbackBase, false);
        public int PlayFallbackBaseEnabledColliderCountForQA =>
            CountTransformComponents<Collider>(_playFallbackBase, true);
        public int PlayFallbackBaseLightCountForQA =>
            CountTransformComponents<Light>(_playFallbackBase, false);
        public int PlayFallbackBaseScriptCountForQA =>
            CountTransformComponents<MonoBehaviour>(_playFallbackBase, false);
        public int AuthoredPlayArenaRendererCount { get; private set; }
        public int AuthoredPlayArenaMaterialCount { get; private set; }
        public int AuthoredPlayArenaColliderCount { get; private set; }
        public int AuthoredPlayArenaLightCount { get; private set; }
        public Vector3 AuthoredPlayArenaBoundsSize { get; private set; }
        public bool HasAuthoredGardenAtelier => _authoredGardenAtelier != null;
        public int AuthoredGardenAtelierRendererCount { get; private set; }
        public int AuthoredGardenAtelierMaterialCount { get; private set; }
        public int AuthoredGardenAtelierColliderCount { get; private set; }
        public int AuthoredGardenAtelierLightCount { get; private set; }
        public Vector3 AuthoredGardenAtelierBoundsSize { get; private set; }
        public bool HasGardenMagicFlowerPrefab =>
            GardenMagicFlowerInstanceCount == GardenMagicFlowerRequiredInstances &&
            GardenMagicFlowerRendererCount > 0;
        public int GardenMagicFlowerInstanceCount { get; private set; }
        public int GardenMagicFlowerRendererCount { get; private set; }
        public int GardenMagicFlowerUniqueMaterialCount => _gardenMagicFlowerMaterialIds.Count;
        public int GardenMagicFlowerColliderCount { get; private set; }
        public int GardenMagicFlowerLightCount { get; private set; }
        public int GardenMagicFlowerEnabledColliderCount { get; private set; }
        public int GardenMagicFlowerEnabledLightCount { get; private set; }
        public bool GardenMagicFlowerUsesSharedMaterials { get; private set; } = true;
        public int GardenMagicFlowerRendererBudget => GardenMagicFlowerMaxRenderers;
        public string GardenMagicFlowerQAMarker => HasGardenMagicFlowerPrefab
            ? "MOONLIGHT_MAGIC_FLOWER_STAGE_READY"
            : "MOONLIGHT_MAGIC_FLOWER_STAGE_MISSING";
        public int GardenSelectedPlantSlot => GardenPlantSlotIndex(_gestureSample);
        public Vector3 GardenSelectedPlantSlotLocalPosition =>
            GardenPlantSlotPosition(GardenSelectedPlantSlot);
        public bool GardenSelectedPlantSlotInsidePlanter =>
            GardenPlantPointIsInsidePlanter(GardenSelectedPlantSlotLocalPosition);
        public Vector3 GardenGesturePropLocalPosition => CurrentStep switch
        {
            0 when _seeds != null && _seeds.Length > 2 && _seeds[2] != null =>
                _seeds[2].localPosition,
            1 when _gardenProps != null && _gardenProps.Length > 2 &&
                _gardenProps[2] != null => _gardenProps[2].localPosition,
            2 when _gardenProps != null && _gardenProps.Length > 2 &&
                _gardenProps[2] != null => _gardenProps[2].localPosition,
            _ => new Vector3(float.NaN, float.NaN, float.NaN)
        };
        public Vector3 GardenExpectedGesturePropLocalPosition => CurrentStep switch
        {
            0 => EvaluateGardenPlantSeedPosition(_gardenProgress, _gestureSample),
            1 => EvaluateGardenWaterPath(_gardenProgress, _gestureSample),
            2 => EvaluateGardenTendPath(_gardenProgress, _gestureSample),
            _ => new Vector3(float.NaN, float.NaN, float.NaN)
        };
        public int GardenCurrentTendTargetIndex =>
            GardenTendTargetIndexAtProgress(_gardenProgress);
        public int GardenTendTargetCount => GardenMagicFlowerRequiredInstances;
        public int GardenTendDirectionInversionCount =>
            GardenTendInversionCount(_gestureSample);
        public float GardenWaterSourceSignedArea =>
            GardenSampleSignedArea(_gestureSample);
        public float GardenWaterSignedArea => GardenWaterPathSignedArea(_gestureSample);
        public bool GardenWaterDirectionAgreement =>
            Mathf.Abs(GardenWaterSourceSignedArea) >= 0.08f &&
            Mathf.Abs(GardenWaterSignedArea) >= 0.01f &&
            Mathf.Sign(GardenWaterSourceSignedArea) == Mathf.Sign(GardenWaterSignedArea);
        public float GardenBloomOpeningScale =>
            _flowers != null && _flowers.Length > 2 && _flowers[2] != null
                ? _flowers[2].localScale.x
                : float.NaN;
        public float GardenExpectedBloomOpeningScale =>
            EvaluateGardenBloomScale(_gardenProgress, 2, _gestureSample);
        public float GardenBloomIntensityMultiplier =>
            EvaluateGardenBloomIntensity(_gestureSample);
        public float GardenBloomLightIntensity =>
            _activityLight != null ? _activityLight.intensity : float.NaN;
        public float GardenExpectedBloomLightIntensity =>
            (ActivityLightBaseIntensity +
             Mathf.Sin(_gardenProgress * Mathf.PI) * ActivityLightPulseIntensity) *
            GardenBloomIntensityMultiplier;
        public bool GardenGesturePropTransformAgreement =>
            CurrentKind == MoonlightSpatialActionKind.Garden && CurrentStep < 3 &&
            IsFinite(GardenGesturePropLocalPosition) &&
            Vector3.Distance(GardenGesturePropLocalPosition,
                GardenExpectedGesturePropLocalPosition) <= 0.001f;
        public bool GardenTendSequenceTransformAgreement =>
            CurrentKind == MoonlightSpatialActionKind.Garden && CurrentStep == 2 &&
            GardenFlowerTargetsMatchGesture();
        public bool GardenTendAnchorPathAgreement =>
            CurrentKind == MoonlightSpatialActionKind.Garden && CurrentStep == 2 &&
            GardenTendToolMatchesTargetsAtAnchors(_gestureSample);
        public bool GardenTendCurrentTargetGrowthAgreement =>
            CurrentKind == MoonlightSpatialActionKind.Garden && CurrentStep == 2 &&
            GardenCurrentTendTargetMatchesProgress();
        public bool GardenBloomTransformAgreement =>
            CurrentKind == MoonlightSpatialActionKind.Garden && CurrentStep == 3 &&
            IsFinite(GardenBloomOpeningScale) && IsFinite(GardenBloomLightIntensity) &&
            Mathf.Abs(GardenBloomOpeningScale - GardenExpectedBloomOpeningScale) <= 0.001f &&
            Mathf.Abs(GardenBloomLightIntensity - GardenExpectedBloomLightIntensity) <= 0.001f;
        public bool GardenBudgetReady => CurrentKind == MoonlightSpatialActionKind.Garden &&
            ActiveRendererCount > 0 && ActiveRendererCount <= GardenRendererBudget &&
            ActiveUniqueMaterialCount > 0 &&
            ActiveUniqueMaterialCount <= GardenMaterialBudget &&
            ActiveLightCount == GardenLightBudget;
        public bool GardenBloomPersistsDuringLinger => IsLingering &&
            CurrentKind == MoonlightSpatialActionKind.Garden && CurrentStep == 3 &&
            Mathf.Abs(_gardenProgress - 1f) <= 0.0001f && GardenBloomTransformAgreement;
        public string GardenBudgetEvidence =>
            $"renderers={ActiveRendererCount}/{GardenRendererBudget} " +
            $"materials={ActiveUniqueMaterialCount}/{GardenMaterialBudget} " +
            $"lights={ActiveLightCount}/{GardenLightBudget}";
        public bool HasAuthoredReadingNook => _authoredReadingNook != null;
        public int AuthoredReadingNookRendererCount { get; private set; }
        public int AuthoredReadingNookMaterialCount { get; private set; }
        public int AuthoredReadingNookColliderCount { get; private set; }
        public int AuthoredReadingNookLightCount { get; private set; }
        public Vector3 AuthoredReadingNookBoundsSize { get; private set; }
        public float ReadProgress => _readProgress;
        public bool ReadGestureSampleReady =>
            CurrentKind == MoonlightSpatialActionKind.Read &&
            _gestureSample.PointCount == MoonlightGestureSample.ResampledPointCount &&
            _gestureSample.HasSevenFiniteNormalizedPoints;
        public Vector2 ReadActualOpeningAngles => ReadCoverAngles();
        public Vector2 ReadExpectedOpeningAngles => EvaluateReadOpeningAngles(
            CurrentStep == 0 ? _readProgress : 1f, _gestureSample);
        public bool ReadOpeningTransformAgreement =>
            CurrentKind == MoonlightSpatialActionKind.Read && CurrentStep == 0 &&
            ReadOpeningTransformsMatch();
        public bool ReadPageTurnTransformAgreement =>
            CurrentKind == MoonlightSpatialActionKind.Read && CurrentStep == 1 &&
            ReadPageTransformsMatch();
        public Vector3 ReadActualPrimaryPagePosition =>
            _pageFlips != null && _pageFlips.Length > 0 && _pageFlips[0] != null
                ? _pageFlips[0].localPosition
                : new Vector3(float.NaN, float.NaN, float.NaN);
        public Vector3 ReadExpectedPrimaryPagePosition => EvaluateReadPagePosition(
            Mathf.Repeat(_readProgress * 1.6f, 1f), _gestureSample);
        public bool ReadBookmarkTransformAgreement =>
            CurrentKind == MoonlightSpatialActionKind.Read && CurrentStep == 2 &&
            ReadBookmarkTransformMatches();
        public Vector3 ReadActualBookmarkPosition => _bookmark != null
            ? _bookmark.localPosition
            : new Vector3(float.NaN, float.NaN, float.NaN);
        public Vector3 ReadExpectedBookmarkPosition =>
            EvaluateReadBookmarkPosition(_readProgress, _gestureSample);
        public bool ReadFinishTransformAgreement =>
            CurrentKind == MoonlightSpatialActionKind.Read && CurrentStep == 3 &&
            ReadFinishTransformsMatch();
        public bool ReadCurrentStepTransformAgreement => CurrentStep switch
        {
            0 => ReadOpeningTransformAgreement,
            1 => ReadPageTurnTransformAgreement,
            2 => ReadBookmarkTransformAgreement,
            3 => ReadFinishTransformAgreement,
            _ => false
        };
        public float ReadFinishIntensityMultiplier =>
            EvaluateReadFinishIntensity(_gestureSample);
        public float ReadActualLightIntensity => _activityLight != null
            ? _activityLight.intensity
            : float.NaN;
        public float ReadExpectedLightIntensity => EvaluateReadLightTarget(
            _readProgress, CurrentStep, _gestureSample);
        public int ReadActualFinishMoteCount => CountActiveReadMotes();
        public int ReadExpectedFinishMoteCount =>
            EvaluateReadFinishMoteCount(_gestureSample);
        public int ReadStageRendererCount => CountReadStageRenderers();
        public int ReadStageUniqueMaterialCount => CountReadStageUniqueMaterials();
        public bool ReadBudgetReady => CurrentKind == MoonlightSpatialActionKind.Read &&
            ReadStageRendererCount == RequiredReadStageRendererCount &&
            ReadStageUniqueMaterialCount == RequiredReadStageMaterialCount &&
            ActiveRendererCount > 0 && ActiveRendererCount <= ReadRendererBudget &&
            ActiveUniqueMaterialCount > 0 &&
            ActiveUniqueMaterialCount <= ReadMaterialBudget &&
            ActiveLightCount == ReadLightBudget;
        public bool ReadRuntimeContractReady => ReadGestureSampleReady &&
            ReadCurrentStepTransformAgreement && ReadBudgetReady;
        public string ReadBudgetEvidence =>
            $"activeRenderers={ActiveRendererCount}/{ReadRendererBudget} " +
            $"activeMaterials={ActiveUniqueMaterialCount}/{ReadMaterialBudget} " +
            $"stageRenderers={ReadStageRendererCount}/{RequiredReadStageRendererCount} " +
            $"stageMaterials={ReadStageUniqueMaterialCount}/{RequiredReadStageMaterialCount} " +
            $"lights={ActiveLightCount}/{ReadLightBudget}";
        public string ReadTransformEvidence => CurrentStep switch
        {
            0 => $"opening={ReadActualOpeningAngles:F3}/{ReadExpectedOpeningAngles:F3}",
            1 => $"page0={ReadActualPrimaryPagePosition:F3}/" +
                $"{ReadExpectedPrimaryPagePosition:F3}",
            2 => $"bookmark={ReadActualBookmarkPosition:F3}/" +
                $"{ReadExpectedBookmarkPosition:F3}",
            3 => $"motes={ReadActualFinishMoteCount}/{ReadExpectedFinishMoteCount} " +
                $"light={ReadActualLightIntensity:0.000}/{ReadExpectedLightIntensity:0.000}",
            _ => "inactive"
        };
        public bool HasAuthoredCareStation => _authoredCareStation != null &&
            !UsesProceduralCareStationFallback;
        public bool UsesProceduralCareStationFallback { get; private set; }
        public string CareStationVisualSource { get; private set; } = "missing";
        public string CareStationSourceQAMarker { get; private set; } =
            "MOONLIGHT_CARE_STATION_SOURCE_MISSING";
        public int CareStationRendererCount { get; private set; }
        public int CareStationMaterialCount { get; private set; }
        public int CareStationColliderCount { get; private set; }
        public int CareStationLightCount { get; private set; }
        public Vector3 CareStationBoundsSize { get; private set; }
        public int CareStationRendererBudget => CareStationVisualSource switch
        {
            "persistent-procedural-fallback" => 15,
            "stage-procedural-fallback" => 4,
            _ => 24
        };
        public int CareStationMaterialBudget => CareStationVisualSource switch
        {
            "persistent-procedural-fallback" => 8,
            "stage-procedural-fallback" => 4,
            _ => 12
        };
        public int AuthoredCareStationRendererCount { get; private set; }
        public int AuthoredCareStationMaterialCount { get; private set; }
        public int AuthoredCareStationColliderCount { get; private set; }
        public int AuthoredCareStationLightCount { get; private set; }
        public Vector3 AuthoredCareStationBoundsSize { get; private set; }
        public float CareProgress => _careProgress;
        public bool CareGestureSampleReady =>
            CurrentKind == MoonlightSpatialActionKind.Care &&
            _gestureSample.PointCount == MoonlightGestureSample.ResampledPointCount &&
            _gestureSample.HasSevenFiniteNormalizedPoints;
        public Vector3 CareActualGesturePropLocalPosition => CurrentStep switch
        {
            0 when _careTowel != null => _careTowel.localPosition,
            1 when _careBrush != null => _careBrush.localPosition,
            2 when _careComb != null => _careComb.localPosition,
            _ => new Vector3(float.NaN, float.NaN, float.NaN)
        };
        public Vector3 CareExpectedGesturePropLocalPosition => CurrentStep switch
        {
            0 => EvaluateCareTowelPosition(_careProgress, _gestureSample),
            1 => EvaluateCareWashBrushPosition(_careProgress, _gestureSample),
            2 => EvaluateCareCombPosition(_careProgress, _gestureSample),
            _ => new Vector3(float.NaN, float.NaN, float.NaN)
        };
        public float CareWashSignedDirection => EvaluateCareWashDirection(_gestureSample);
        public float CareWashOrbitRadius => EvaluateCareWashRadius(_gestureSample);
        public Vector3 CareActualPrimaryBubbleLocalPosition =>
            _careBubbles != null && _careBubbles.Length > 0 && _careBubbles[0] != null
                ? _careBubbles[0].localPosition
                : new Vector3(float.NaN, float.NaN, float.NaN);
        public float CareGlowAuraScaleMultiplier =>
            EvaluateCareGlowScaleMultiplier(_gestureSample);
        public Vector3 CareActualGlowAuraScale => _careMirrorAura != null
            ? _careMirrorAura.localScale
            : new Vector3(float.NaN, float.NaN, float.NaN);
        public Vector3 CareExpectedGlowAuraScale =>
            EvaluateCareGlowAuraScale(_careProgress, _gestureSample);
        public float CareActualGlowLightIntensity => _activityLight != null
            ? _activityLight.intensity
            : float.NaN;
        public float CareExpectedGlowLightIntensity =>
            EvaluateCareGlowLightIntensity(_careProgress, _gestureSample);
        public int CareActualGlowMoteCount => CountActiveCareMotes();
        public int CareExpectedGlowMoteCount => EvaluateCareGlowMoteCount(_gestureSample);
        public bool CarePrepTransformAgreement =>
            CurrentKind == MoonlightSpatialActionKind.Care && CurrentStep == 0 &&
            _careTowel != null &&
            Vector3.Distance(_careTowel.localPosition,
                EvaluateCareTowelPosition(_careProgress, _gestureSample)) <= 0.001f &&
            Quaternion.Angle(_careTowel.localRotation,
                Quaternion.Euler(EvaluateCareTowelEuler(_careProgress, _gestureSample))) <= 0.01f;
        public bool CareWashTransformAgreement =>
            CurrentKind == MoonlightSpatialActionKind.Care && CurrentStep == 1 &&
            CareWashTransformsMatch();
        public bool CareBrushTransformAgreement =>
            CurrentKind == MoonlightSpatialActionKind.Care && CurrentStep == 2 &&
            _careComb != null &&
            Vector3.Distance(_careComb.localPosition,
                EvaluateCareCombPosition(_careProgress, _gestureSample)) <= 0.001f &&
            Quaternion.Angle(_careComb.localRotation,
                Quaternion.Euler(EvaluateCareCombEuler(_careProgress, _gestureSample))) <= 0.01f;
        public bool CareGlowTransformAgreement =>
            CurrentKind == MoonlightSpatialActionKind.Care && CurrentStep == 3 &&
            CareGlowTransformsMatch();
        public bool CareCurrentStepTransformAgreement => CurrentStep switch
        {
            0 => CarePrepTransformAgreement,
            1 => CareWashTransformAgreement,
            2 => CareBrushTransformAgreement,
            3 => CareGlowTransformAgreement,
            _ => false
        };
        public bool CareRuntimeContractReady => CareGestureSampleReady &&
            CareCurrentStepTransformAgreement;
        public string CareTransformEvidence => CurrentStep switch
        {
            0 => $"towel={CareActualGesturePropLocalPosition:F3}/" +
                $"{CareExpectedGesturePropLocalPosition:F3}",
            1 => $"brush={CareActualGesturePropLocalPosition:F3}/" +
                $"{CareExpectedGesturePropLocalPosition:F3} " +
                $"direction={CareWashSignedDirection:0}/radius={CareWashOrbitRadius:0.000}",
            2 => $"comb={CareActualGesturePropLocalPosition:F3}/" +
                $"{CareExpectedGesturePropLocalPosition:F3}",
            3 => $"aura={CareActualGlowAuraScale:F3}/" +
                $"{CareExpectedGlowAuraScale:F3} " +
                $"light={CareActualGlowLightIntensity:0.000}/" +
                $"{CareExpectedGlowLightIntensity:0.000} " +
                $"motes={CareActualGlowMoteCount}/{CareExpectedGlowMoteCount}",
            _ => "inactive"
        };

        public void Begin(MoonlightSpatialActionKind kind)
        {
            Begin(kind, 0, 1);
        }

        public bool ConfigureCareLiveHarnessIsolationForQA(bool enabled)
        {
            if (IsVisible || IsLingering) return false;
            _careLiveHarnessIsolationEnabledForQA = enabled;
            return true;
        }

        static bool ShouldBindPersistentStation(MoonlightSpatialActionKind kind,
            bool careLiveHarnessIsolationEnabled) =>
            kind != MoonlightSpatialActionKind.Care ||
            !careLiveHarnessIsolationEnabled;

        public void Begin(MoonlightSpatialActionKind kind, int stepIndex, int requiredSteps)
            => Begin(kind, stepIndex, requiredSteps,
                MoonlightGestureSample.Synthetic(MoonlightGestureKind.Swipe, 1f));

        public void Begin(MoonlightSpatialActionKind kind, int stepIndex, int requiredSteps,
            MoonlightGestureSample gestureSample)
            => Begin(kind, stepIndex, requiredSteps, gestureSample, "");

        public void Begin(MoonlightSpatialActionKind kind, int stepIndex, int requiredSteps,
            MoonlightGestureSample gestureSample, string currentState)
        {
            if (TryBeginRetainedCookStep(kind, stepIndex, requiredSteps, gestureSample))
                return;
            if (TryBeginRetainedPlayStep(kind, stepIndex, requiredSteps, gestureSample))
                return;

            End();
            CurrentKind = kind;
            _gestureSample = gestureSample;
            _bedtimeState = kind == MoonlightSpatialActionKind.SleepCuddle
                ? NormalizeBedtimeState(currentState)
                : "";
            _requiredSteps = kind is MoonlightSpatialActionKind.Cook or MoonlightSpatialActionKind.Play or
                MoonlightSpatialActionKind.Garden or MoonlightSpatialActionKind.Read or
                MoonlightSpatialActionKind.Care
                ? Mathf.Max(4, requiredSteps)
                : Mathf.Max(1, requiredSteps);
            CurrentStep = Mathf.Clamp(stepIndex, 0, _requiredSteps - 1);
            _playZoneInstanceId = kind == MoonlightSpatialActionKind.Play
                ? CurrentInteractionZoneInstanceId()
                : 0;
            _cookZoneInstanceId = kind == MoonlightSpatialActionKind.Cook
                ? CurrentInteractionZoneInstanceId()
                : 0;
            _root = new GameObject($"ActivityStage-{kind}");
            _root.transform.SetParent(null, true);
            bool allowPersistentStation = kind != MoonlightSpatialActionKind.SleepCuddle &&
                ShouldBindPersistentStation(kind, _careLiveHarnessIsolationEnabledForQA);
            _persistentStation = allowPersistentStation
                ? MoonlightActivityStation.FindNearestActive(kind, transform.position)
                : null;
            if (_persistentStation != null)
                PersistentStationBindingCountForQA++;
            if (_persistentStation != null && CurrentStep == 0)
            {
                PersistentStationResetCountForQA++;
                _persistentStation.ResetCompletionState();
            }
            _center = _persistentStation != null
                ? _persistentStation.AnchorPosition
                : transform.position + (kind == MoonlightSpatialActionKind.Cook
                    ? new Vector3(1.58f, 0.72f, 0.22f)
                    : kind == MoonlightSpatialActionKind.Garden
                        ? new Vector3(1.10f, 0.04f, 0.30f)
                    : kind == MoonlightSpatialActionKind.Read
                        ? new Vector3(1.08f, 0.05f, 0.30f)
                    : kind == MoonlightSpatialActionKind.Care
                        ? new Vector3(0.92f, 0.05f, 0.24f)
                    : kind == MoonlightSpatialActionKind.SleepCuddle
                        ? new Vector3(0f, 0f, 0.18f)
                    : new Vector3(-0.58f, 0f, -0.10f));
            _root.transform.position = _center;
            _root.transform.localScale = _persistentStation != null
                ? _persistentStation.AnchorScale
                : kind switch
            {
                MoonlightSpatialActionKind.Cook => Vector3.one * 1.12f,
                MoonlightSpatialActionKind.Play => Vector3.one * 1.10f,
                MoonlightSpatialActionKind.Garden => Vector3.one * 1.08f,
                MoonlightSpatialActionKind.Read => Vector3.one * 1.08f,
                MoonlightSpatialActionKind.Care => Vector3.one * 1.06f,
                MoonlightSpatialActionKind.SleepCuddle => Vector3.one * 0.92f,
                _ => Vector3.one
            };

            if (_persistentStation != null)
                Debug.Log($"[MoonlightActivityStage] bound-persistent-station kind={kind} " +
                    $"anchor={_center:F2} marker=MOONLIGHT_ACTIVITY_STATION_BOUND");

            if (kind == MoonlightSpatialActionKind.Cook) BuildCookStage();
            else if (kind == MoonlightSpatialActionKind.Play) BuildPlayStage();
            else if (kind == MoonlightSpatialActionKind.Garden) BuildGardenStage();
            else if (kind == MoonlightSpatialActionKind.Read) BuildReadStage();
            else if (kind == MoonlightSpatialActionKind.Care) BuildCareStage();
            else if (kind == MoonlightSpatialActionKind.SleepCuddle)
                BuildBedtimeStage(_bedtimeState);

            RefreshStageLights();
            UpdateStage(kind, 0f);
        }

        bool TryBeginRetainedCookStep(MoonlightSpatialActionKind kind, int stepIndex,
            int requiredSteps, MoonlightGestureSample gestureSample)
        {
            int nextStep = Mathf.Clamp(stepIndex, 0, Mathf.Max(3, requiredSteps - 1));
            if (kind != MoonlightSpatialActionKind.Cook || !IsHoldingCookStepTerminal ||
                IsLingering || nextStep != CurrentStep + 1)
                return false;

            int currentZoneInstanceId = CurrentInteractionZoneInstanceId();
            if (_cookZoneInstanceId != 0 && currentZoneInstanceId != _cookZoneInstanceId)
                return false;

            int previousStep = CurrentStep;
            CaptureCookHandoff(previousStep);
            _requiredSteps = Mathf.Max(CookPhaseCount, requiredSteps);
            CurrentStep = Mathf.Clamp(stepIndex, 0, _requiredSteps - 1);
            _gestureSample = gestureSample;
            _cookHandoffFromStep = previousStep;
            _cookHandoffActive = true;
            _isHoldingCookStepTerminal = false;
            UpdateStage(kind, 0f);
            LastCookSharedPropDiscontinuityForQA =
                MeasureCookHandoffDiscontinuity(previousStep);
            CookHandoffCountForQA++;
            Debug.Log($"[MoonlightActivityQA] cook-stage-continued step={CurrentStep + 1}/" +
                $"{_requiredSteps} root={CookStageRootInstanceId} shared=" +
                $"{LastCookSharedPropCountForQA} discontinuity=" +
                $"{LastCookSharedPropDiscontinuityForQA:0.000000}m materials=" +
                $"{CookStageMaterialIdentityCountForQA}/" +
                $"{CookStageMaterialIdentityHashForQA} light={CookStageLightInstanceId} " +
                "marker=MOONLIGHT_COOK_STAGE_CONTINUITY_REUSED");
            return true;
        }

        bool TryBeginRetainedPlayStep(MoonlightSpatialActionKind kind, int stepIndex,
            int requiredSteps, MoonlightGestureSample gestureSample)
        {
            int nextStep = Mathf.Clamp(stepIndex, 0, Mathf.Max(3, requiredSteps - 1));
            if (kind != MoonlightSpatialActionKind.Play || !IsHoldingPlayStepTerminal ||
                IsLingering || nextStep != CurrentStep + 1 || _ballTrail == null)
                return false;

            int currentZoneInstanceId = CurrentInteractionZoneInstanceId();
            if (_playZoneInstanceId != 0 && currentZoneInstanceId != _playZoneInstanceId)
                return false;

            Vector3 heldEndpoint = _ball.localPosition;
            _requiredSteps = Mathf.Max(4, requiredSteps);
            CurrentStep = Mathf.Clamp(stepIndex, 0, _requiredSteps - 1);
            _gestureSample = gestureSample;
            _playProgress = 0f;
            _playContinuationStart = heldEndpoint;
            _playContinuationActive = true;
            _playContinuationFirstRenderedFramePending = true;
            _playContinuationBeginFrame = Time.frameCount;
            _playContinuationLastAdvancedFrame = Time.frameCount;
            _playContinuationElapsed = 0f;
            LastPlayContinuationClockDeltaForQA = 0f;
            _isHoldingPlayStepTerminal = false;
            UpdateStage(kind, 0f);
            LastPlayContinuationDiscontinuityForQA =
                Vector3.Distance(heldEndpoint, _ball.localPosition);
            PlayContinuationCountForQA++;
            Debug.Log($"[MoonlightActivityQA] play-stage-continued step={CurrentStep + 1}/" +
                $"{_requiredSteps} root={PlayStageRootInstanceId} ball={PlayBallInstanceId} " +
                $"trail={PlayTrailInstanceId} discontinuity=" +
                $"{LastPlayContinuationDiscontinuityForQA:0.000000}m " +
                $"materials={ActiveUniqueMaterialCount}/" +
                $"{PlayBallSharedMaterialInstanceId}/" +
                $"{PlayTrailSharedMaterialInstanceId} " +
                "marker=MOONLIGHT_PLAY_STAGE_CONTINUITY_REUSED");
            return true;
        }

        int CurrentInteractionZoneInstanceId()
        {
            var interactor = GetComponent<MoonlightSpatialInteractor>();
            return interactor != null && interactor.CurrentZone != null
                ? interactor.CurrentZone.GetInstanceID()
                : 0;
        }

        public bool HoldPlayStepTerminal()
        {
            if (_root == null || CurrentKind != MoonlightSpatialActionKind.Play ||
                CurrentStep >= _requiredSteps - 1 || _ball == null || _ballTrail == null)
                return false;

            _playContinuationActive = false;
            UpdateStage(CurrentKind, 1f);
            _isHoldingPlayStepTerminal = true;
            Debug.Log($"[MoonlightActivityQA] play-step-terminal-held " +
                $"step={CurrentStep + 1}/{_requiredSteps} root={PlayStageRootInstanceId} " +
                $"ball={PlayBallInstanceId} trail={PlayTrailInstanceId} " +
                $"materials={ActiveUniqueMaterialCount}/" +
                $"{PlayBallSharedMaterialInstanceId}/" +
                $"{PlayTrailSharedMaterialInstanceId} " +
                "marker=MOONLIGHT_PLAY_STEP_TERMINAL_HELD");
            return true;
        }

        public bool HoldCookStepTerminal()
        {
            if (_root == null || CurrentKind != MoonlightSpatialActionKind.Cook ||
                CurrentStep >= _requiredSteps - 1)
                return false;

            _cookHandoffActive = false;
            UpdateStage(CurrentKind, 1f);
            _isHoldingCookStepTerminal = true;
            Debug.Log($"[MoonlightActivityQA] cook-step-terminal-held " +
                $"step={CurrentStep + 1}/{_requiredSteps} root={CookStageRootInstanceId} " +
                $"materials={CookStageMaterialIdentityCountForQA}/" +
                $"{CookStageMaterialIdentityHashForQA} light={CookStageLightInstanceId} " +
                "marker=MOONLIGHT_COOK_STEP_TERMINAL_HELD");
            return true;
        }

        public bool LingerFinalState(float seconds)
        {
            bool bedtimeSingleStep = SingleStepLingerAllowedForQA(CurrentKind,
                _requiredSteps, CurrentStep);
            if (_root == null || (!bedtimeSingleStep && _requiredSteps <= 1) ||
                CurrentStep != _requiredSteps - 1)
                return false;

            if (CurrentKind == MoonlightSpatialActionKind.Care)
                seconds = CareFinalPresentationSeconds;
            else if (bedtimeSingleStep)
                seconds = BedtimeLingerSeconds;

            if (_lingerRoutine != null)
                StopCoroutine(_lingerRoutine);

            UpdateStage(CurrentKind, 1f);
            _isHoldingCookStepTerminal = false;
            _isHoldingPlayStepTerminal = false;
            _playContinuationActive = false;
            _applyPersistentCompletionOnEnd = !bedtimeSingleStep &&
                ShouldBindPersistentStation(CurrentKind,
                    _careLiveHarnessIsolationEnabledForQA);
            IsLingering = true;
            _lingerUntil = Time.time + Mathf.Max(0.5f, seconds);
            _lingerCompletingNaturally = false;
            if (CurrentKind == MoonlightSpatialActionKind.Care)
            {
                LastCareLingerRequestedSecondsForQA = _lingerUntil - Time.time;
                LastCareLingerStartedAtSecondsForQA = Time.time;
                LastCareLingerEndedAtSecondsForQA = 0f;
                LastCareLingerObservedSecondsForQA = 0f;
                LastCareLingerCompletedNaturallyForQA = false;
            }
            else if (CurrentKind == MoonlightSpatialActionKind.SleepCuddle)
            {
                LastBedtimeLingerRequestedSecondsForQA = _lingerUntil - Time.time;
                LastBedtimeLingerStartedAtSecondsForQA = Time.time;
                LastBedtimeLingerEndedAtSecondsForQA = 0f;
                LastBedtimeLingerObservedSecondsForQA = 0f;
                LastBedtimeLingerCompletedNaturallyForQA = false;
                LastBedtimeLingerStateForQA = _bedtimeState;
            }
            _lingerRoutine = StartCoroutine(LingerThenEnd());
            Debug.Log($"[MoonlightActivityQA] final-presentation kind={CurrentKind} " +
                $"step={CurrentStep + 1}/{_requiredSteps} hold={seconds:0.00}s " +
                "marker=MOONLIGHT_ACTIVITY_FINAL_PRESENTATION");
            return true;
        }

        System.Collections.IEnumerator LingerThenEnd()
        {
            while (Time.time < _lingerUntil)
                yield return null;

            _lingerRoutine = null;
            _lingerCompletingNaturally = true;
            End();
            _lingerCompletingNaturally = false;
        }

        public void UpdateStage(MoonlightSpatialActionKind kind, float t)
            => UpdateStage(kind, t, _bedtimeState);

        public void UpdateStage(MoonlightSpatialActionKind kind, float t,
            string currentState)
        {
            if (_root == null) return;
            t = Mathf.Clamp01(t);
            if (CurrentKind == MoonlightSpatialActionKind.Cook) UpdateCook(t);
            else if (CurrentKind == MoonlightSpatialActionKind.Play) UpdatePlay(t);
            else if (CurrentKind == MoonlightSpatialActionKind.Garden) UpdateGarden(t);
            else if (CurrentKind == MoonlightSpatialActionKind.Read) UpdateRead(t);
            else if (CurrentKind == MoonlightSpatialActionKind.Care) UpdateCare(t);
            else if (CurrentKind == MoonlightSpatialActionKind.SleepCuddle)
                UpdateBedtime(currentState, t);

            if (_activityLight != null)
            {
                float intensity = ActivityLightBaseIntensity +
                    Mathf.Sin(t * Mathf.PI) * ActivityLightPulseIntensity;
                if (CurrentKind == MoonlightSpatialActionKind.Garden && CurrentStep == 3)
                    intensity *= EvaluateGardenBloomIntensity(_gestureSample);
                else if (CurrentKind == MoonlightSpatialActionKind.Read && CurrentStep == 3)
                    intensity = EvaluateReadLightTarget(t, CurrentStep, _gestureSample);
                else if (CurrentKind == MoonlightSpatialActionKind.Care && CurrentStep == 3)
                    intensity = EvaluateCareGlowLightIntensity(t, _gestureSample);
                _activityLight.intensity = intensity;
            }

            UpdateActivityCounts();
        }

        public bool TryGetInteractionContactPoint(MoonlightSpatialActionKind kind, int stepIndex,
            float progress, out Vector3 point)
        {
            point = Vector3.zero;
            if (_root == null || kind != CurrentKind) return false;

            int step = Mathf.Clamp(stepIndex, 0, 3);
            progress = Mathf.Clamp01(progress);
            if (kind == MoonlightSpatialActionKind.Cook)
            {
                if (step == 0 && _bowlRim != null)
                {
                    point = _bowlRim.position;
                    return true;
                }
                if (step == 1 && _whisk != null)
                {
                    point = _whisk.position;
                    return true;
                }
                if (step == 2)
                    return TryGetWorldPoint(_servingProps, 0, out point);
                if (step == 3)
                {
                    if (progress < 0.68f && TryGetWorldPoint(_decorateProps, 0, out point))
                        return true;
                    int cookieIndex = Mathf.Clamp(
                        Mathf.RoundToInt(Mathf.InverseLerp(0.68f, 1f, progress) * 2f), 0, 2);
                    return TryGetWorldPoint(_cookies, cookieIndex, out point);
                }
            }

            if (kind == MoonlightSpatialActionKind.Play && _ball != null)
            {
                point = _ball.position;
                return true;
            }

            if (kind == MoonlightSpatialActionKind.Garden)
            {
                if (step == 0)
                    return TryGetWorldPoint(_seeds, 2, out point);
                if (step == 1)
                    return TryGetWorldPoint(_gardenProps, 2, out point);
                if (step == 2)
                {
                    if (TryGetWorldPoint(_flowers, 2, out point))
                    {
                        point += Vector3.up * 0.18f;
                        return true;
                    }
                    return TryGetWorldPoint(_sprouts, 2, out point);
                }

                if (TryGetWorldPoint(_flowers, 2, out point))
                {
                    point += Vector3.up * 0.30f;
                    return true;
                }
                return TryGetWorldPoint(_gardenSparkles,
                    Mathf.Clamp(Mathf.FloorToInt(progress * 6f), 0, 6), out point);
            }

            if (kind == MoonlightSpatialActionKind.Read)
            {
                if (step == 0 && TryGetWorldPoint(_bookProps, 1, out Vector3 leftCover) &&
                    TryGetWorldPoint(_bookProps, 2, out Vector3 rightCover))
                {
                    point = Vector3.Lerp(leftCover, rightCover, 0.5f) + Vector3.up * 0.08f;
                    return true;
                }
                if (step == 1)
                {
                    int pageIndex = Mathf.Clamp(
                        Mathf.FloorToInt(Mathf.Repeat(progress * 1.6f, 1f) * 4f), 0, 3);
                    return TryGetWorldPoint(_pageFlips, pageIndex, out point);
                }
                if (step == 2 && _bookmark != null)
                {
                    point = _bookmark.position + Vector3.up * 0.05f;
                    return true;
                }
                if (step == 3)
                {
                    int moteIndex = Mathf.Clamp(Mathf.FloorToInt(Mathf.Repeat(progress, 1f) * 9f), 0, 8);
                    return TryGetWorldPoint(_readMotes, moteIndex, out point);
                }
            }

            if (kind == MoonlightSpatialActionKind.Care)
            {
                Transform contact = step switch
                {
                    0 => _careTowelTray,
                    1 => _careBrush,
                    2 => _careComb,
                    _ => _careMirror
                };
                if (contact != null && contact.gameObject.activeInHierarchy)
                {
                    point = contact.position;
                    return IsFinite(point);
                }
            }

            return false;
        }

        static bool IsFinite(Vector3 point) =>
            !float.IsNaN(point.x) && !float.IsInfinity(point.x) &&
            !float.IsNaN(point.y) && !float.IsInfinity(point.y) &&
            !float.IsNaN(point.z) && !float.IsInfinity(point.z);

        static bool TryGetWorldPoint(Transform[] candidates, int index, out Vector3 point)
        {
            point = Vector3.zero;
            if (candidates == null || index < 0 || index >= candidates.Length || candidates[index] == null)
                return false;
            point = candidates[index].position;
            return true;
        }

        void Update()
        {
            bool activeBedtime = CurrentKind == MoonlightSpatialActionKind.SleepCuddle &&
                _root != null;
            if (!IsLingering && !IsHoldingPlayStepTerminal &&
                !IsHoldingCookStepTerminal && !activeBedtime) return;

            var interactor = GetComponent<MoonlightSpatialInteractor>();
            var currentZone = interactor != null ? interactor.CurrentZone : null;
            bool samePlayZone = CurrentKind != MoonlightSpatialActionKind.Play ||
                _playZoneInstanceId == 0 ||
                (currentZone != null && currentZone.GetInstanceID() == _playZoneInstanceId);
            bool sameCookZone = CurrentKind != MoonlightSpatialActionKind.Cook ||
                _cookZoneInstanceId == 0 ||
                (currentZone != null && currentZone.GetInstanceID() == _cookZoneInstanceId);
            if (currentZone != null && currentZone.isActiveAndEnabled &&
                currentZone.gameObject.activeInHierarchy &&
                currentZone.Kind == CurrentKind && samePlayZone && sameCookZone)
                return;

            if (IsLingering)
                Debug.Log($"[MoonlightActivityQA] final-presentation-cancel kind={CurrentKind} " +
                    "reason=left-zone marker=MOONLIGHT_ACTIVITY_FINAL_PRESENTATION_CANCELLED");
            else if (activeBedtime)
                Debug.Log("[MoonlightActivityQA] bedtime-stage-cancel kind=SleepCuddle " +
                    "reason=left-zone marker=MOONLIGHT_BEDTIME_STAGE_EXIT_CLEANED");
            else
                Debug.Log($"[MoonlightActivityQA] step-hold-cancel kind={CurrentKind} " +
                    "reason=left-zone marker=MOONLIGHT_ACTIVITY_STEP_HOLD_CANCELLED");
            End();
        }

        public void End()
        {
            if (IsLingering && CurrentKind == MoonlightSpatialActionKind.Care)
            {
                LastCareLingerEndedAtSecondsForQA = Time.time;
                LastCareLingerObservedSecondsForQA = Mathf.Max(0f,
                    LastCareLingerEndedAtSecondsForQA -
                    LastCareLingerStartedAtSecondsForQA);
                LastCareLingerCompletedNaturallyForQA = _lingerCompletingNaturally;
            }
            if (IsLingering && CurrentKind == MoonlightSpatialActionKind.SleepCuddle)
            {
                LastBedtimeLingerEndedAtSecondsForQA = Time.time;
                LastBedtimeLingerObservedSecondsForQA = Mathf.Max(0f,
                    LastBedtimeLingerEndedAtSecondsForQA -
                    LastBedtimeLingerStartedAtSecondsForQA);
                LastBedtimeLingerCompletedNaturallyForQA = _lingerCompletingNaturally;
            }
            if (_applyPersistentCompletionOnEnd)
            {
                var persistentStation = _persistentStation;
                _applyPersistentCompletionOnEnd = false;
                _persistentStation = null;
                if (persistentStation != null)
                {
                    PersistentCompletionApplicationCountForQA++;
                    persistentStation.ApplyCompletionState();
                }
                else
                {
                    Debug.Log("[MoonlightActivityStage] completion-skip reason=station-unavailable " +
                        "marker=MOONLIGHT_PERSISTENT_ACTIVITY_TEARDOWN_SAFE");
                }
            }
            if (_lingerRoutine != null)
            {
                StopCoroutine(_lingerRoutine);
                _lingerRoutine = null;
            }
            if (_root != null) Destroy(_root);
            foreach (var material in _materials)
                if (material != null) Destroy(material);
            _materials.Clear();
            _materialCache.Clear();
            _configuredSurfaceProfiles.Clear();
            _renderers.Clear();
            _sharedMaterialBuffer.Clear();
            _stageLights.Clear();
            _lightBuffer.Clear();
            _root = null;
            _bowl = null;
            _bowlRim = null;
            _batter = null;
            _whisk = null;
            _ingredients = null;
            _pourStreams = null;
            _steam = null;
            _cookies = null;
            _cookProps = null;
            _cookieDetails = null;
            _ovenProps = null;
            _decorateProps = null;
            _servingProps = null;
            _authoredCookWorkbench = null;
            _cookFallbackBase = null;
            AuthoredCookWorkbenchRendererCount = 0;
            AuthoredCookWorkbenchMaterialCount = 0;
            AuthoredCookWorkbenchColliderCount = 0;
            AuthoredCookWorkbenchLightCount = 0;
            CookChoreographyReadyMask = 0;
            CookCurrentPhaseName = "inactive";
            CookCurrentPhaseProgress = 0f;
            CookCurrentPhaseMotionPropCount = 0;
            CookCurrentPhaseVisibleMotionPropCount = 0;
            CookCurrentPhaseMotionReady = false;
            CookCurrentPhaseStateReady = false;
            _cookBakeDoorClearancePass = true;
            _isHoldingCookStepTerminal = false;
            _cookHandoffActive = false;
            _cookHandoffFromStep = -1;
            _cookZoneInstanceId = 0;
            _cookHandoffSharedProps.Clear();
            _cookHandoffSharedPositions.Clear();
            _cookHandoffSharedRotations.Clear();
            _cookHandoffSharedScales.Clear();
            _cookBowlHandoffPosition = Vector3.zero;
            _ball = null;
            _playBallRenderer = null;
            _gestureSample = default;
            _playProgress = 0f;
            _isHoldingPlayStepTerminal = false;
            _playContinuationActive = false;
            _playContinuationFirstRenderedFramePending = false;
            _playContinuationBeginFrame = 0;
            _playContinuationLastAdvancedFrame = 0;
            _playZoneInstanceId = 0;
            _playContinuationElapsed = 0f;
            LastPlayContinuationClockDeltaForQA = 0f;
            _playContinuationStart = Vector3.zero;
            _gardenProgress = 0f;
            _readProgress = 0f;
            _blocks = null;
            _playProps = null;
            _starDetails = null;
            _pathMarkers = null;
            _celebrationStars = null;
            _playArches = null;
            _podiumProps = null;
            _authoredPlayArena = null;
            _playFallbackBase = null;
            AuthoredPlayArenaRendererCount = 0;
            AuthoredPlayArenaMaterialCount = 0;
            AuthoredPlayArenaColliderCount = 0;
            AuthoredPlayArenaLightCount = 0;
            AuthoredPlayArenaBoundsSize = Vector3.zero;
            _gardenProps = null;
            _seeds = null;
            _sprouts = null;
            _flowers = null;
            _gardenSparkles = null;
            _authoredGardenAtelier = null;
            AuthoredGardenAtelierRendererCount = 0;
            AuthoredGardenAtelierMaterialCount = 0;
            AuthoredGardenAtelierColliderCount = 0;
            AuthoredGardenAtelierLightCount = 0;
            AuthoredGardenAtelierBoundsSize = Vector3.zero;
            _gardenMagicFlowerMaterialIds.Clear();
            GardenMagicFlowerInstanceCount = 0;
            GardenMagicFlowerRendererCount = 0;
            GardenMagicFlowerColliderCount = 0;
            GardenMagicFlowerLightCount = 0;
            GardenMagicFlowerEnabledColliderCount = 0;
            GardenMagicFlowerEnabledLightCount = 0;
            GardenMagicFlowerUsesSharedMaterials = true;
            _bookProps = null;
            _pageFlips = null;
            _readMotes = null;
            _bookmark = null;
            _authoredReadingNook = null;
            AuthoredReadingNookRendererCount = 0;
            AuthoredReadingNookMaterialCount = 0;
            AuthoredReadingNookColliderCount = 0;
            AuthoredReadingNookLightCount = 0;
            AuthoredReadingNookBoundsSize = Vector3.zero;
            _careProps = null;
            _careBubbles = null;
            _careMotes = null;
            _careTowelTray = null;
            _careTowel = null;
            _careBrush = null;
            _careComb = null;
            _careMirror = null;
            _careMirrorAura = null;
            _authoredCareStation = null;
            _bedtimeParts = null;
            _bedtimeProgress = 0f;
            _bedtimeState = "";
            UsesProceduralCareStationFallback = false;
            CareStationVisualSource = "missing";
            CareStationSourceQAMarker = "MOONLIGHT_CARE_STATION_SOURCE_MISSING";
            CareStationRendererCount = 0;
            CareStationMaterialCount = 0;
            CareStationColliderCount = 0;
            CareStationLightCount = 0;
            CareStationBoundsSize = Vector3.zero;
            AuthoredCareStationRendererCount = 0;
            AuthoredCareStationMaterialCount = 0;
            AuthoredCareStationColliderCount = 0;
            AuthoredCareStationLightCount = 0;
            AuthoredCareStationBoundsSize = Vector3.zero;
            _ballTrail = null;
            _activityLight = null;
            _persistentStation = null;
            _applyPersistentCompletionOnEnd = false;
            IsLingering = false;
            _lingerUntil = 0f;
            _requiredSteps = 1;
            CurrentKind = default;
            CurrentStep = 0;
            ActiveRendererCount = 0;
            ActiveUniqueMaterialCount = 0;
            ActiveLightCount = 0;
        }

        public bool ValidateLastCareLingerRuntimeContract(float toleranceSeconds,
            out string detail)
        {
            float tolerance = Mathf.Clamp(toleranceSeconds, 0.05f, 0.50f);
            bool entered = LastCareLingerStartedAtSecondsForQA > 0f &&
                LastCareLingerEndedAtSecondsForQA >=
                    LastCareLingerStartedAtSecondsForQA;
            bool requested = Mathf.Abs(LastCareLingerRequestedSecondsForQA -
                CareFinalPresentationSeconds) <= 0.01f;
            bool observed = Mathf.Abs(LastCareLingerObservedSecondsForQA -
                CareFinalPresentationSeconds) <= tolerance;
            bool isolated = _careLiveHarnessIsolationEnabledForQA &&
                _persistentStation == null && !_applyPersistentCompletionOnEnd &&
                PersistentStationBindingCountForQA == 0 &&
                PersistentStationResetCountForQA == 0 &&
                PersistentCompletionApplicationCountForQA == 0;
            detail = $"entered={entered} natural=" +
                $"{LastCareLingerCompletedNaturallyForQA} requested=" +
                $"{LastCareLingerRequestedSecondsForQA:0.000}s observed=" +
                $"{LastCareLingerObservedSecondsForQA:0.000}s expected=" +
                $"{CareFinalPresentationSeconds:0.000}s tolerance={tolerance:0.000}s " +
                $"persistentIsolated={isolated} stationOps=" +
                $"{PersistentStationBindingCountForQA}/" +
                $"{PersistentStationResetCountForQA}/" +
                $"{PersistentCompletionApplicationCountForQA}";
            return entered && requested && observed &&
                LastCareLingerCompletedNaturallyForQA && isolated;
        }

        void CaptureCookHandoff(int fromStep)
        {
            _cookHandoffSharedProps.Clear();
            _cookHandoffSharedPositions.Clear();
            _cookHandoffSharedRotations.Clear();
            _cookHandoffSharedScales.Clear();

            void Capture(Transform prop)
            {
                if (prop == null) return;
                _cookHandoffSharedProps.Add(prop);
                _cookHandoffSharedPositions.Add(prop.localPosition);
                _cookHandoffSharedRotations.Add(prop.localRotation);
                _cookHandoffSharedScales.Add(prop.localScale);
            }

            if (fromStep == 0)
            {
                Capture(_bowl);
                Capture(_bowlRim);
                Capture(_batter);
            }
            else if (fromStep == 1)
            {
                Capture(_bowl);
                Capture(_bowlRim);
                Capture(_batter);
                _cookBowlHandoffPosition = _bowl != null
                    ? _bowl.localPosition
                    : Vector3.zero;
            }
            else if (fromStep == 2)
            {
                if (_servingProps != null)
                    for (int i = 0; i < _servingProps.Length; i++)
                        Capture(_servingProps[i]);
                if (_cookies != null)
                    for (int i = 0; i < _cookies.Length; i++) Capture(_cookies[i]);
            }
        }

        float MeasureCookHandoffDiscontinuity(int fromStep)
        {
            if (fromStep == 1)
            {
                LastCookSharedPropCountForQA = 1;
                return _servingProps != null && _servingProps.Length > 0 &&
                    _servingProps[0] != null
                        ? Vector3.Distance(_cookBowlHandoffPosition,
                            _servingProps[0].localPosition)
                        : float.PositiveInfinity;
            }

            LastCookSharedPropCountForQA = _cookHandoffSharedProps.Count;
            float maximum = 0f;
            for (int i = 0; i < _cookHandoffSharedProps.Count; i++)
            {
                Transform prop = _cookHandoffSharedProps[i];
                if (prop == null) return float.PositiveInfinity;
                maximum = Mathf.Max(maximum,
                    Vector3.Distance(_cookHandoffSharedPositions[i], prop.localPosition));
            }
            return maximum;
        }

        void ApplyCookHandoff(float progress)
        {
            float blend = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress));
            if (_cookHandoffFromStep == 1)
            {
                if (_servingProps != null && _servingProps.Length >= 4)
                {
                    Vector3 targetCenter = _servingProps[0].localPosition;
                    Vector3 center = Vector3.Lerp(_cookBowlHandoffPosition,
                        targetCenter, blend);
                    _servingProps[0].localPosition = center;
                    _servingProps[1].localPosition = center +
                        new Vector3(-0.48f * blend, 0.02f * blend, 0f);
                    _servingProps[2].localPosition = center +
                        new Vector3(0.48f * blend, 0.02f * blend, 0f);
                    _servingProps[3].localPosition = center +
                        new Vector3(0f, -0.075f * blend, 0f);
                }
                for (int i = 0; i < _cookHandoffSharedProps.Count; i++)
                {
                    Transform source = _cookHandoffSharedProps[i];
                    if (source == null) continue;
                    source.gameObject.SetActive(blend < 1f);
                    source.localPosition = _cookHandoffSharedPositions[i];
                    source.localRotation = _cookHandoffSharedRotations[i];
                    source.localScale = Vector3.Lerp(
                        _cookHandoffSharedScales[i], Vector3.zero, blend);
                }
                return;
            }

            for (int i = 0; i < _cookHandoffSharedProps.Count; i++)
            {
                Transform prop = _cookHandoffSharedProps[i];
                if (prop == null) continue;
                prop.localPosition = Vector3.Lerp(
                    _cookHandoffSharedPositions[i], prop.localPosition, blend);
                prop.localRotation = Quaternion.Slerp(
                    _cookHandoffSharedRotations[i], prop.localRotation, blend);
                prop.localScale = Vector3.Lerp(
                    _cookHandoffSharedScales[i], prop.localScale, blend);
            }
        }

        int CountCookStageMaterialIdentities(out int identityHash)
        {
            _cookMaterialIdentityBuffer.Clear();
            for (int rendererIndex = 0; rendererIndex < _renderers.Count; rendererIndex++)
            {
                Renderer renderer = _renderers[rendererIndex];
                if (renderer == null) continue;
                _sharedMaterialBuffer.Clear();
                renderer.GetSharedMaterials(_sharedMaterialBuffer);
                for (int materialIndex = 0; materialIndex < _sharedMaterialBuffer.Count;
                     materialIndex++)
                {
                    Material material = _sharedMaterialBuffer[materialIndex];
                    if (material != null)
                        _cookMaterialIdentityBuffer.Add(material.GetInstanceID());
                }
            }

            unchecked
            {
                int sum = 0;
                int xor = 0;
                foreach (int identity in _cookMaterialIdentityBuffer)
                {
                    sum += identity;
                    xor ^= identity;
                }
                identityHash = (sum * 397) ^ xor ^ _cookMaterialIdentityBuffer.Count;
            }
            return _cookMaterialIdentityBuffer.Count;
        }

        void BuildCookStage()
        {
            CookStageBuildCountForQA++;
            if (!BuildAuthoredCookWorkbench())
            {
                _cookFallbackBase = new[]
                {
                    Primitive(PrimitiveType.Cube, CookFallbackBaseNames[0],
                        CookFallbackBasePositions[0], CookFallbackBaseScales[0],
                        new Color(0.33f, 0.22f, 0.19f), 0.02f),
                    Primitive(PrimitiveType.Cube, CookFallbackBaseNames[1],
                        CookFallbackBasePositions[1], CookFallbackBaseScales[1],
                        new Color(0.93f, 0.79f, 0.58f), 0.04f)
                };
            }
            _servingProps = new[]
            {
                Primitive(PrimitiveType.Cylinder, "BakeTrayServingPlatter", new Vector3(0.45f, 0.46f, 0.18f),
                    new Vector3(0.82f, 0.030f, 0.50f), new Color(0.62f, 0.63f, 0.66f), 0.08f),
                Primitive(PrimitiveType.Cube, "PlatterHandleLeft", new Vector3(0.02f, 0.48f, 0.18f),
                    new Vector3(0.16f, 0.025f, 0.10f), new Color(0.82f, 0.80f, 0.76f), 0.10f),
                Primitive(PrimitiveType.Cube, "PlatterHandleRight", new Vector3(0.88f, 0.48f, 0.18f),
                    new Vector3(0.16f, 0.025f, 0.10f), new Color(0.82f, 0.80f, 0.76f), 0.10f),
                Primitive(PrimitiveType.Cylinder, "ServingPlatterFoot", new Vector3(0.45f, 0.42f, 0.18f),
                    new Vector3(0.40f, 0.035f, 0.30f), new Color(0.82f, 0.80f, 0.76f), 0.10f),
            };
            SetActive(_servingProps, false);

            _bowl = Primitive(PrimitiveType.Sphere, "MooncakeBowl", new Vector3(0f, 0.50f, 0f),
                new Vector3(0.62f, 0.20f, 0.62f), new Color(0.34f, 0.72f, 0.70f), 0.08f);
            _bowlRim = Primitive(PrimitiveType.Cylinder, "BowlRim", new Vector3(0f, 0.64f, 0f),
                new Vector3(0.65f, 0.035f, 0.65f), new Color(0.72f, 0.91f, 0.87f), 0.04f);
            _batter = Primitive(PrimitiveType.Sphere, "MooncakeBatter", new Vector3(0f, 0.65f, 0f),
                new Vector3(0.49f, 0.055f, 0.49f), new Color(0.98f, 0.77f, 0.38f), 0.12f);

            _whisk = Primitive(PrimitiveType.Capsule, "MoonWhisk", new Vector3(0.05f, 1.00f, 0f),
                new Vector3(0.07f, 0.40f, 0.07f), new Color(0.78f, 0.61f, 0.88f), 0.04f);
            _whisk.localRotation = Quaternion.Euler(0f, 0f, -24f);

            _cookProps = new[]
            {
                Primitive(PrimitiveType.Capsule, "RollingPin", new Vector3(-0.52f, 0.54f, 0.24f),
                    new Vector3(0.055f, 0.42f, 0.055f), new Color(0.77f, 0.48f, 0.27f), 0.02f),
                Primitive(PrimitiveType.Cube, "RecipeCard", new Vector3(-0.55f, 0.62f, -0.18f),
                    new Vector3(0.34f, 0.025f, 0.24f), new Color(1f, 0.93f, 0.72f), 0.06f),
                Primitive(PrimitiveType.Cylinder, "MeasuringCup", new Vector3(0.58f, 0.55f, -0.19f),
                    new Vector3(0.14f, 0.16f, 0.14f), new Color(0.74f, 0.90f, 1f), 0.07f),
                Primitive(PrimitiveType.Capsule, "TinySpoon", new Vector3(0.12f, 0.53f, 0.34f),
                    new Vector3(0.035f, 0.32f, 0.035f), new Color(0.84f, 0.81f, 0.75f), 0.08f),
            };
            _cookProps[0].localRotation = Quaternion.Euler(88f, 0f, 70f);
            _cookProps[1].localRotation = Quaternion.Euler(72f, 0f, -8f);
            _cookProps[2].localRotation = Quaternion.Euler(0f, 0f, -6f);
            _cookProps[3].localRotation = Quaternion.Euler(82f, 0f, -42f);

            var ingredientColors = new[]
            {
                new Color(0.96f, 0.48f, 0.58f),
                new Color(0.98f, 0.82f, 0.40f),
                new Color(0.54f, 0.74f, 0.95f),
            };
            _ingredients = new Transform[ingredientColors.Length];
            for (int i = 0; i < _ingredients.Length; i++)
                _ingredients[i] = Primitive(PrimitiveType.Sphere, $"Ingredient-{i + 1}", Vector3.zero,
                    Vector3.one * 0.15f, ingredientColors[i], 0.18f);

            _pourStreams = new Transform[_ingredients.Length];
            for (int i = 0; i < _pourStreams.Length; i++)
            {
                _pourStreams[i] = Primitive(PrimitiveType.Cube, $"IngredientPour-{i + 1}", Vector3.zero,
                    new Vector3(0.035f, 0.35f, 0.035f), ingredientColors[i], 0.10f, true);
                _pourStreams[i].gameObject.SetActive(false);
            }

            _steam = new Transform[3];
            for (int i = 0; i < _steam.Length; i++)
                _steam[i] = Primitive(PrimitiveType.Sphere, $"Steam-{i + 1}", Vector3.zero,
                    new Vector3(0.075f, 0.16f, 0.075f), new Color(1f, 0.94f, 0.82f, 0.52f), 0.36f, true);

            _cookies = new Transform[3];
            _cookieDetails = new Transform[_cookies.Length * 3];
            for (int i = 0; i < _cookies.Length; i++)
            {
                _cookies[i] = Primitive(PrimitiveType.Cylinder, $"Mooncake-{i + 1}", Vector3.zero,
                    new Vector3(0.16f, 0.035f, 0.16f), new Color(0.98f, 0.72f, 0.30f), 0.10f);
                _cookies[i].gameObject.SetActive(false);

                for (int mark = 0; mark < 3; mark++)
                {
                    int detailIndex = i * 3 + mark;
                    _cookieDetails[detailIndex] = Primitive(PrimitiveType.Cube, $"MooncakeMark-{i + 1}-{mark + 1}",
                        Vector3.zero, new Vector3(0.05f, 0.012f, 0.012f),
                        new Color(1f, 0.86f, 0.43f), 0.12f);
                    _cookieDetails[detailIndex].gameObject.SetActive(false);
                }
            }

            _ovenProps = new[]
            {
                Primitive(PrimitiveType.Cube, "TinyOvenBody", new Vector3(-0.48f, 0.59f, -0.20f),
                    new Vector3(0.44f, 0.34f, 0.30f), new Color(0.44f, 0.30f, 0.38f), 0.03f),
                Primitive(PrimitiveType.Cube, "TinyOvenWindow", new Vector3(-0.48f, 0.61f, -0.035f),
                    new Vector3(0.30f, 0.16f, 0.025f), new Color(1f, 0.55f, 0.24f), 0.28f),
                Primitive(PrimitiveType.Cylinder, "OvenKnobLeft", new Vector3(-0.58f, 0.75f, -0.03f),
                    new Vector3(0.045f, 0.020f, 0.045f), new Color(0.82f, 0.80f, 0.76f), 0.08f),
                Primitive(PrimitiveType.Cylinder, "OvenKnobRight", new Vector3(-0.39f, 0.75f, -0.03f),
                    new Vector3(0.045f, 0.020f, 0.045f), new Color(0.82f, 0.80f, 0.76f), 0.08f),
            };

            _decorateProps = new[]
            {
                Primitive(PrimitiveType.Capsule, "FrostingBag", new Vector3(-0.35f, 0.88f, 0.16f),
                    new Vector3(0.10f, 0.34f, 0.10f), new Color(0.96f, 0.52f, 0.68f), 0.12f),
                Primitive(PrimitiveType.Cylinder, "SprinkleCup", new Vector3(0.78f, 0.56f, -0.18f),
                    new Vector3(0.12f, 0.16f, 0.12f), new Color(0.54f, 0.74f, 0.95f), 0.08f),
                Primitive(PrimitiveType.Sphere, "SugarMoon", new Vector3(0.00f, 0.78f, 0.18f),
                    Vector3.one * 0.11f, new Color(1f, 0.93f, 0.72f), 0.18f),
                Primitive(PrimitiveType.Cube, "DecorRibbon", new Vector3(0.45f, 0.52f, 0.36f),
                    new Vector3(0.48f, 0.020f, 0.045f), new Color(0.96f, 0.52f, 0.68f), 0.10f),
            };
            _decorateProps[0].localRotation = Quaternion.Euler(22f, 0f, 34f);
            _decorateProps[3].localRotation = Quaternion.Euler(0f, 22f, 0f);
            SetActive(_decorateProps, false);

            RefreshCookChoreographyConfiguration();
            AddActivityLight(new Color(1f, 0.72f, 0.34f));
        }

        bool BuildAuthoredCookWorkbench()
        {
            if (_persistentStation != null && _persistentStation.Kind == MoonlightSpatialActionKind.Cook &&
                _persistentStation.VisualRoot != null)
            {
                _authoredCookWorkbench = _persistentStation.VisualRoot;
                var stationRenderers = _authoredCookWorkbench.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < stationRenderers.Length; i++) _renderers.Add(stationRenderers[i]);
                AuthoredCookWorkbenchRendererCount = _persistentStation.RendererCount;
                AuthoredCookWorkbenchMaterialCount = _persistentStation.UniqueMaterialCount;
                AuthoredCookWorkbenchColliderCount = _persistentStation.ColliderCount;
                AuthoredCookWorkbenchLightCount = _persistentStation.LightCount;
                Debug.Log($"[MoonlightActivityStage] authored-cook-workbench persistent=true " +
                    $"renderers={AuthoredCookWorkbenchRendererCount} materials={AuthoredCookWorkbenchMaterialCount} " +
                    $"colliders={AuthoredCookWorkbenchColliderCount} lights={AuthoredCookWorkbenchLightCount} " +
                    "marker=MOONLIGHT_AUTHORED_COOK_WORKBENCH_READY");
                return true;
            }

            var prefab = Resources.Load<GameObject>("Models/Hero/MoonKitchenWorkbench");
            if (prefab == null)
            {
                Debug.LogError("[MoonlightActivityStage] authored Cook workbench missing; using fallback");
                return false;
            }

            var instance = Instantiate(prefab, _root.transform, false);
            instance.name = "MoonKitchenWorkbenchAuthored";
            instance.transform.localPosition = new Vector3(0f, 0.01f, 0.02f);
            instance.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            instance.transform.localScale = new Vector3(1f, 0.56f, 1f);
            _authoredCookWorkbench = instance.transform;

            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderers[i].receiveShadows = true;
                _renderers.Add(renderers[i]);
            }

            var materialIds = new HashSet<int>();
            for (int i = 0; i < renderers.Length; i++)
            {
                var shared = renderers[i].sharedMaterials;
                for (int materialIndex = 0; materialIndex < shared.Length; materialIndex++)
                    if (shared[materialIndex] != null) materialIds.Add(shared[materialIndex].GetInstanceID());
            }

            var colliders = instance.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = false;
            var lights = instance.GetComponentsInChildren<Light>(true);
            for (int i = 0; i < lights.Length; i++) lights[i].enabled = false;

            AuthoredCookWorkbenchRendererCount = renderers.Length;
            AuthoredCookWorkbenchMaterialCount = materialIds.Count;
            AuthoredCookWorkbenchColliderCount = colliders.Length;
            AuthoredCookWorkbenchLightCount = lights.Length;
            Debug.Log($"[MoonlightActivityStage] authored-cook-workbench renderers={renderers.Length} " +
                $"materials={materialIds.Count} colliders={colliders.Length} lights={lights.Length} " +
                "marker=MOONLIGHT_AUTHORED_COOK_WORKBENCH_READY");
            return true;
        }

        void UpdateCook(float t)
        {
            if (_ingredients == null || _steam == null || _cookies == null || _cookieDetails == null)
            {
                CookCurrentPhaseMotionReady = false;
                CookCurrentPhaseStateReady = false;
                return;
            }

            float handoffProgress = _cookHandoffActive
                ? Mathf.Clamp01(t / CookHandoffProgressFraction)
                : 1f;
            if (_cookHandoffActive)
                t = t <= CookHandoffProgressFraction
                    ? 0f
                    : Mathf.InverseLerp(CookHandoffProgressFraction, 1f, t);
            int step = Mathf.Clamp(CurrentStep, 0, 3);
            CookCurrentPhaseName = CookPhaseName(step);
            CookCurrentPhaseProgress = t;
            CookCurrentPhaseMotionPropCount = 0;
            CookCurrentPhaseVisibleMotionPropCount = 0;
            CookCurrentPhaseMotionReady = false;
            SetActive(_ovenProps, step == 2);
            SetActive(_decorateProps, step == 3);
            SetActive(_servingProps, step >= 2);
            if (_bowl != null) _bowl.gameObject.SetActive(step <= 1);
            if (_bowlRim != null) _bowlRim.gameObject.SetActive(step <= 1);
            if (_cookProps != null && _cookProps.Length >= 4)
            {
                _cookProps[0].gameObject.SetActive(step == 2);
                _cookProps[1].gameObject.SetActive(step == 0);
                _cookProps[2].gameObject.SetActive(step == 0);
                _cookProps[3].gameObject.SetActive(step == 1);
            }

            if (_activityLight != null)
            {
                _activityLight.color = step switch
                {
                    0 => new Color(1f, 0.76f, 0.38f),
                    1 => new Color(0.48f, 0.90f, 0.84f),
                    2 => new Color(1f, 0.48f, 0.20f),
                    _ => new Color(1f, 0.58f, 0.72f)
                };
            }

            if (_bowl != null)
            {
                float stirRock = step == 1 ? Mathf.Sin(t * Mathf.PI * 6f) * 2.5f : 0f;
                _bowl.localPosition = new Vector3(0f, 0.50f, 0f);
                _bowl.localRotation = Quaternion.Euler(0f, step == 1 ? -t * 38f : 0f, stirRock);
            }
            if (_bowlRim != null)
            {
                _bowlRim.localPosition = new Vector3(0f, 0.64f, 0f);
                _bowlRim.localRotation = Quaternion.Euler(0f, step == 1 ? -t * 38f : 0f,
                    step == 1 ? Mathf.Sin(t * Mathf.PI * 6f) * 2.5f : 0f);
            }

            if (_cookProps != null && _cookProps.Length >= 4)
            {
                if (step == 0)
                {
                    float addSettle = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 1.4f));
                    float cardLift = addSettle * 0.05f +
                        Mathf.Sin(Mathf.Clamp01(t * 1.8f) * Mathf.PI) * 0.035f;
                    _cookProps[1].localPosition = new Vector3(-0.55f, 0.62f + cardLift, -0.18f);
                    _cookProps[1].localRotation = Quaternion.Euler(
                        72f - addSettle * 8f - cardLift * 90f, 0f, -8f);
                    float cupTip = addSettle * 20f +
                        Mathf.Sin(Mathf.Clamp01(t * 1.35f) * Mathf.PI) * 28f;
                    _cookProps[2].localPosition = new Vector3(0.58f - cupTip * 0.0025f,
                        0.55f + addSettle * 0.04f + Mathf.Sin(t * Mathf.PI) * 0.05f, -0.19f);
                    _cookProps[2].localRotation = Quaternion.Euler(0f, 0f, -6f - cupTip);
                }
                else if (step == 1)
                {
                    float tap = Mathf.Sin(t * Mathf.PI * 4f);
                    _cookProps[3].localPosition = new Vector3(0.22f, 0.61f + Mathf.Max(0f, tap) * 0.08f,
                        0.30f);
                    _cookProps[3].localRotation = Quaternion.Euler(72f, t * 120f, -42f + tap * 8f);
                }
            }

            if (_servingProps != null && _servingProps.Length >= 4)
            {
                float present = step == 3
                    ? Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.68f) * 3.125f))
                    : 0f;
                Vector3 trayCenter = Vector3.Lerp(new Vector3(0.45f, 0.46f, 0.18f),
                    new Vector3(0.16f, 0.60f, 0.12f), present);
                _servingProps[0].localPosition = trayCenter;
                _servingProps[0].localScale = Vector3.Lerp(new Vector3(0.82f, 0.030f, 0.50f),
                    new Vector3(0.92f, 0.035f, 0.58f), present);
                _servingProps[1].localPosition = trayCenter + new Vector3(-0.48f, 0.02f, 0f);
                _servingProps[2].localPosition = trayCenter + new Vector3(0.48f, 0.02f, 0f);
                _servingProps[3].localPosition = trayCenter + new Vector3(0f, -0.075f, 0f);
                _servingProps[3].localScale = Vector3.Lerp(new Vector3(0.40f, 0.035f, 0.30f),
                    new Vector3(0.52f, 0.055f, 0.36f), present);

                if (step == 2)
                {
                    trayCenter = BakeTrayCenter(t);
                    _servingProps[0].localPosition = trayCenter;
                    _servingProps[1].localPosition = trayCenter + new Vector3(-0.48f, 0.02f, 0f);
                    _servingProps[2].localPosition = trayCenter + new Vector3(0.48f, 0.02f, 0f);
                    _servingProps[3].localPosition = trayCenter + new Vector3(0f, -0.075f, 0f);
                }
            }
            if (_batter != null)
            {
                _batter.gameObject.SetActive(step <= 1 && (step != 0 || t > 0.20f));
                float batterPulse = step == 1 ? 1f + Mathf.Sin(t * Mathf.PI * 8f) * 0.08f : 1f;
                float batterSize = step == 0
                    ? Mathf.Lerp(0.16f, 0.46f, t)
                    : Mathf.Lerp(0.49f, 0.53f, t) * batterPulse;
                _batter.localPosition = new Vector3(step == 1 ? t * 0.02f : 0f,
                    0.65f + (step == 1
                        ? t * 0.015f + Mathf.Sin(t * Mathf.PI * 6f) * 0.025f
                        : 0f), 0f);
                _batter.localScale = new Vector3(batterSize, 0.055f * batterPulse, batterSize);
            }

            if (_whisk != null)
            {
                _whisk.gameObject.SetActive(step == 1);
                if (step == 1)
                {
                    _whisk.localPosition = EvaluateCookGesturePath(step, t, _gestureSample);
                    _whisk.localRotation = Quaternion.Euler(18f, t * 900f, -22f);
                }
            }

            for (int i = 0; i < _ingredients.Length; i++)
            {
                bool showPrep = step == 0;
                _ingredients[i].gameObject.SetActive(showPrep);
                bool hasPourStream = _pourStreams != null && i < _pourStreams.Length && _pourStreams[i] != null;
                if (!showPrep)
                {
                    if (hasPourStream) _pourStreams[i].gameObject.SetActive(false);
                    continue;
                }

                float phase = Mathf.Clamp01((t - i * 0.12f) * 1.72f);
                if (hasPourStream)
                    _pourStreams[i].gameObject.SetActive(phase > 0.08f && phase < 0.92f);
                float angle = i * Mathf.PI * 0.67f;
                Vector3 start = new Vector3(-0.64f + i * 0.64f, 1.10f + (i % 2) * 0.16f,
                    i == 1 ? -0.20f : 0.18f);
                Vector3 ingredientPosition = Vector3.Lerp(start, new Vector3(0f, 0.67f, 0f), phase);
                ingredientPosition.y += Mathf.Sin(phase * Mathf.PI) * (0.10f + i * 0.025f);
                _ingredients[i].localPosition = ingredientPosition;
                _ingredients[i].localScale = Vector3.one * Mathf.Lerp(0.15f, 0.035f, phase);
                _ingredients[i].localRotation = Quaternion.Euler(phase * 120f,
                    phase * (160f + i * 35f), i * 18f);

                if (hasPourStream)
                {
                    Vector3 streamTop = Vector3.Lerp(start, new Vector3(0f, 0.95f, 0f), phase);
                    _pourStreams[i].localPosition = Vector3.Lerp(streamTop, new Vector3(0f, 0.85f, 0f), 0.5f);
                    _pourStreams[i].localScale = new Vector3(0.030f, Mathf.Lerp(0.34f, 0.08f, phase), 0.030f);
                    _pourStreams[i].localRotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 12f + i * 8f);
                }
            }

            for (int i = 0; i < _steam.Length; i++)
            {
                float phase = Mathf.Repeat(t * 2.3f + i * 0.31f, 1f);
                _steam[i].gameObject.SetActive(step == 2 && t > 0.16f);
                _steam[i].localPosition = new Vector3(
                    -0.48f + Mathf.Sin(phase * Mathf.PI * 2f + i) * (0.10f + i * 0.025f),
                    0.73f + phase * 0.82f,
                    -0.08f + Mathf.Cos(phase * Mathf.PI * 2f + i) * (0.06f + i * 0.018f));
                float scale = Mathf.Lerp(0.90f, 0.10f, phase);
                _steam[i].localScale = new Vector3(0.07f + i * 0.015f, 0.19f, 0.07f) * scale;
            }

            if (_ovenProps != null && _ovenProps.Length >= 4 && step == 2)
            {
                float doorOpen = BakeDoorOpen(t);
                _ovenProps[1].localPosition = new Vector3(-0.48f, 0.61f - doorOpen * 0.055f,
                    -0.035f + doorOpen * 0.035f);
                _ovenProps[1].localRotation = Quaternion.Euler(-doorOpen * 42f, 0f, 0f);
                _ovenProps[2].localRotation = Quaternion.Euler(90f, t * 210f, 0f);
                _ovenProps[3].localRotation = Quaternion.Euler(90f, -t * 165f, 0f);
                float ovenBreath = 1f + t * 0.015f +
                    Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI * 5f) * 0.025f;
                _ovenProps[0].localScale = new Vector3(0.44f, 0.34f, 0.30f) * ovenBreath;
            }

            if (_cookProps != null && _cookProps.Length >= 1 && _cookProps[0] != null && step == 2)
            {
                float roll = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 1.35f));
                _cookProps[0].localPosition = new Vector3(Mathf.Lerp(-0.52f, 0.28f, roll),
                    0.54f + roll * 0.04f + Mathf.Sin(t * Mathf.PI) * 0.08f, 0.24f);
                _cookProps[0].localRotation = Quaternion.Euler(88f, t * 260f, 70f);
            }

            for (int i = 0; i < _cookies.Length; i++)
            {
                float reveal = step == 2 ? Mathf.Clamp01((t - 0.18f - i * 0.08f) * 5f) : step == 3 ? 1f : 0f;
                _cookies[i].gameObject.SetActive(reveal > 0f);
                Vector3 bakeTrayCenter = _servingProps != null && _servingProps.Length > 0
                    ? _servingProps[0].localPosition
                    : new Vector3(0.34f, 0.51f, 0.15f);
                Vector3 bakePosition = bakeTrayCenter + new Vector3(-0.24f + i * 0.24f,
                    0.035f + Mathf.Sin(reveal * Mathf.PI) * 0.06f, i == 1 ? -0.08f : 0.07f);
                float present = step == 3
                    ? Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.68f) * 3.125f))
                    : 0f;
                Vector3 workingDecorPosition = new Vector3(0.20f + i * 0.24f, 0.53f,
                    i == 1 ? 0.10f : 0.24f);
                Vector3 finalDecorPosition = new Vector3(-0.08f + i * 0.24f, 0.66f,
                    i == 1 ? 0.02f : 0.20f);
                Vector3 decorPosition = Vector3.Lerp(workingDecorPosition,
                    finalDecorPosition, present);
                Vector3 cookiePosition = Vector3.Lerp(bakePosition, decorPosition, step == 3 ? Mathf.Clamp01(t * 3f) : 0f);
                _cookies[i].localPosition = cookiePosition;
                _cookies[i].localScale = Vector3.Lerp(new Vector3(0.16f, 0.035f, 0.16f),
                    new Vector3(0.19f, 0.045f, 0.19f), step == 3 ? Mathf.Clamp01(t * 3f) : 0f) * reveal
                    * (step == 3 ? 1f + Mathf.Sin(t * Mathf.PI * 4f + i) * 0.05f : 1f);
                _cookies[i].localRotation = Quaternion.Euler(0f, 24f + i * 32f + (step == 3 ? t * 65f : 0f), 0f);

                for (int mark = 0; mark < 3; mark++)
                {
                    int detailIndex = i * 3 + mark;
                    if (detailIndex >= _cookieDetails.Length || _cookieDetails[detailIndex] == null) continue;
                    float decorReveal = step == 3
                        ? Mathf.Clamp01((t - 0.18f - detailIndex * 0.025f) * 5f)
                        : 0f;
                    _cookieDetails[detailIndex].gameObject.SetActive(decorReveal > 0.65f);
                    Vector3 imprintOffset = CookGestureImprintOffset(detailIndex, _gestureSample);
                    Quaternion imprintRotation = CookGestureImprintRotation(
                        detailIndex, _gestureSample);
                    _cookieDetails[detailIndex].localPosition = cookiePosition +
                        _cookies[i].localRotation * imprintOffset;
                    _cookieDetails[detailIndex].localScale = new Vector3(0.05f, 0.012f, 0.012f) * decorReveal;
                    _cookieDetails[detailIndex].localRotation =
                        _cookies[i].localRotation * imprintRotation;
                }
            }

            if (_decorateProps != null && _decorateProps.Length >= 4 && step == 3)
            {
                float working = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.68f) * 3.125f));
                float squeeze = Mathf.Sin(t * Mathf.PI * 4f) * 0.08f * working;
                _decorateProps[0].localPosition =
                    EvaluateCookGesturePropPosition(step, t, _gestureSample);
                _decorateProps[0].localRotation = Quaternion.Euler(22f, t * 120f, 34f - squeeze * 80f);
                float sprinklePass = Mathf.Sin(Mathf.Clamp01((t - 0.18f) * 2.2f) * Mathf.PI);
                _decorateProps[1].localPosition = new Vector3(0.67f - sprinklePass * 0.34f,
                    0.58f + sprinklePass * 0.22f, -0.16f + sprinklePass * 0.22f);
                _decorateProps[1].localRotation = Quaternion.Euler(0f, t * 110f,
                    -sprinklePass * 38f);
                _decorateProps[2].localPosition = Vector3.Lerp(
                    new Vector3(Mathf.Sin(t * Mathf.PI * 6f) * 0.08f,
                        0.84f + Mathf.Abs(Mathf.Sin(t * Mathf.PI * 3f)) * 0.11f, 0.18f),
                    new Vector3(0.16f, 0.735f, 0.02f), 1f - working);
                _decorateProps[2].localScale = Vector3.one * Mathf.Lerp(
                    0.09f + Mathf.Sin(t * Mathf.PI * 5f) * 0.025f, 0.12f, 1f - working);
                _decorateProps[3].localPosition = new Vector3(0.16f, 0.585f, 0.39f);
                _decorateProps[3].localScale = new Vector3(0.48f, 0.020f, 0.045f)
                    * Mathf.Lerp(0.25f, 1f, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 2.4f)))
                    * (1f + Mathf.Sin(t * Mathf.PI * 5f) * 0.06f * working);
            }

            if (_cookHandoffActive)
            {
                ApplyCookHandoff(handoffProgress);
                if (handoffProgress >= 1f)
                {
                    _cookHandoffActive = false;
                    _cookHandoffFromStep = -1;
                }
            }

            CookCurrentPhaseMotionPropCount = CountCookPhaseMotionMatches(step, t);
            CookCurrentPhaseVisibleMotionPropCount = CountCookVisibleMotionProps(step);
            if (step == 2 && IsBakeTrayCrossingDoor(t) && BakeDoorOpen(t) < 0.999f)
                _cookBakeDoorClearancePass = false;
            CookCurrentPhaseMotionReady = t >= 0.20f &&
                (CookChoreographyReadyMask & (1 << step)) != 0 &&
                CookCurrentPhaseMotionPropCount == CookPhaseMinimumMotionPropCount(step) &&
                CookCurrentPhaseVisibleMotionPropCount >=
                    CookPhaseMinimumVisibleMotionPropCount(step) &&
                (step != 2 || _cookBakeDoorClearancePass);
            CookCurrentPhaseStateReady = ValidateCookPhaseState(step, t);
        }

        int CountCookVisibleMotionProps(int step)
        {
            int count = 0;
            void Count(Transform item)
            {
                if (IsActive(item)) count++;
            }
            void CountAll(Transform[] items)
            {
                if (items == null) return;
                for (int i = 0; i < items.Length; i++) Count(items[i]);
            }

            if (step == 0)
            {
                CountAll(_ingredients);
                CountAll(_pourStreams);
                Count(_batter);
                if (_cookProps != null && _cookProps.Length >= 3)
                {
                    Count(_cookProps[1]);
                    Count(_cookProps[2]);
                }
            }
            else if (step == 1)
            {
                Count(_bowl);
                Count(_bowlRim);
                Count(_batter);
                Count(_whisk);
                if (_cookProps != null && _cookProps.Length >= 4) Count(_cookProps[3]);
            }
            else if (step == 2)
            {
                CountAll(_servingProps);
                CountAll(_ovenProps);
                CountAll(_steam);
                CountAll(_cookies);
                if (_cookProps != null && _cookProps.Length >= 1) Count(_cookProps[0]);
            }
            else if (step == 3)
            {
                CountAll(_servingProps);
                CountAll(_decorateProps);
                CountAll(_cookies);
                CountAll(_cookieDetails);
            }
            return count;
        }

        void RefreshCookChoreographyConfiguration()
        {
            CookChoreographyReadyMask = 0;
            if (AllAssigned(_ingredients, 3) && AllAssigned(_pourStreams, 3) &&
                _batter != null && _cookProps != null && _cookProps.Length >= 3 &&
                _cookProps[1] != null && _cookProps[2] != null)
                CookChoreographyReadyMask |= 1 << 0;
            if (_whisk != null && _batter != null && _bowl != null && _bowlRim != null &&
                _cookProps != null && _cookProps.Length >= 4 && _cookProps[3] != null)
                CookChoreographyReadyMask |= 1 << 1;
            if (AllAssigned(_servingProps, 4) && AllAssigned(_ovenProps, 4) &&
                AllAssigned(_steam, 3) && AllAssigned(_cookies, 3) &&
                _cookProps != null && _cookProps.Length >= 1 && _cookProps[0] != null)
                CookChoreographyReadyMask |= 1 << 2;
            if (AllAssigned(_servingProps, 4) && AllAssigned(_decorateProps, 4) &&
                AllAssigned(_cookies, 3) && AllAssigned(_cookieDetails, 9))
                CookChoreographyReadyMask |= 1 << 3;
        }

        int CountCookPhaseMotionMatches(int step, float t) => step switch
        {
            0 => CountCookAddMotionMatches(t),
            1 => CountCookStirMotionMatches(t),
            2 => CountCookBakeMotionMatches(t),
            3 => CountCookPresentMotionMatches(t),
            _ => 0
        };

        int CountCookAddMotionMatches(float t)
        {
            int matches = 0;
            for (int i = 0; i < 3; i++)
            {
                float phase = CookIngredientPhase(t, i);
                float angle = i * Mathf.PI * 0.67f;
                Vector3 start = CookIngredientStart(i);
                Vector3 ingredientPosition = Vector3.Lerp(start, new Vector3(0f, 0.67f, 0f), phase);
                ingredientPosition.y += Mathf.Sin(phase * Mathf.PI) * (0.10f + i * 0.025f);
                if (TransformMatches(_ingredients[i], true, ingredientPosition,
                        Quaternion.Euler(phase * 120f, phase * (160f + i * 35f), i * 18f),
                        Vector3.one * Mathf.Lerp(0.15f, 0.035f, phase)))
                    matches++;

                Vector3 streamTop = Vector3.Lerp(start, new Vector3(0f, 0.95f, 0f), phase);
                if (TransformMatches(_pourStreams[i], phase > 0.08f && phase < 0.92f,
                        Vector3.Lerp(streamTop, new Vector3(0f, 0.85f, 0f), 0.5f),
                        Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 12f + i * 8f),
                        new Vector3(0.030f, Mathf.Lerp(0.34f, 0.08f, phase), 0.030f)))
                    matches++;
            }

            float addSettle = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 1.4f));
            float cardLift = addSettle * 0.05f +
                Mathf.Sin(Mathf.Clamp01(t * 1.8f) * Mathf.PI) * 0.035f;
            if (TransformMatches(_cookProps[1], true,
                    new Vector3(-0.55f, 0.62f + cardLift, -0.18f),
                    Quaternion.Euler(72f - addSettle * 8f - cardLift * 90f, 0f, -8f),
                    new Vector3(0.34f, 0.025f, 0.24f)))
                matches++;
            float cupTip = addSettle * 20f +
                Mathf.Sin(Mathf.Clamp01(t * 1.35f) * Mathf.PI) * 28f;
            if (TransformMatches(_cookProps[2], true,
                    new Vector3(0.58f - cupTip * 0.0025f,
                        0.55f + addSettle * 0.04f + Mathf.Sin(t * Mathf.PI) * 0.05f, -0.19f),
                    Quaternion.Euler(0f, 0f, -6f - cupTip),
                    new Vector3(0.14f, 0.16f, 0.14f)))
                matches++;
            float batterSize = Mathf.Lerp(0.16f, 0.46f, t);
            if (TransformMatches(_batter, t > 0.20f, new Vector3(0f, 0.65f, 0f),
                    Quaternion.identity, new Vector3(batterSize, 0.055f, batterSize)))
                matches++;
            return matches;
        }

        int CountCookStirMotionMatches(float t)
        {
            int matches = 0;
            float stirRock = Mathf.Sin(t * Mathf.PI * 6f) * 2.5f;
            Quaternion bowlRotation = Quaternion.Euler(0f, -t * 38f, stirRock);
            if (TransformMatches(_bowl, true, new Vector3(0f, 0.50f, 0f), bowlRotation,
                    new Vector3(0.62f, 0.20f, 0.62f)))
                matches++;
            if (TransformMatches(_bowlRim, true, new Vector3(0f, 0.64f, 0f), bowlRotation,
                    new Vector3(0.65f, 0.035f, 0.65f)))
                matches++;

            float batterPulse = 1f + Mathf.Sin(t * Mathf.PI * 8f) * 0.08f;
            float batterSize = Mathf.Lerp(0.49f, 0.53f, t) * batterPulse;
            if (TransformMatches(_batter, true,
                    new Vector3(t * 0.02f,
                        0.65f + t * 0.015f + Mathf.Sin(t * Mathf.PI * 6f) * 0.025f, 0f),
                    Quaternion.identity,
                    new Vector3(batterSize, 0.055f * batterPulse, batterSize)))
                matches++;

            if (TransformMatches(_whisk, true,
                    EvaluateCookGesturePath(1, t, _gestureSample),
                    Quaternion.Euler(18f, t * 900f, -22f),
                    new Vector3(0.07f, 0.40f, 0.07f)))
                matches++;
            float tap = Mathf.Sin(t * Mathf.PI * 4f);
            if (TransformMatches(_cookProps[3], true,
                    new Vector3(0.22f, 0.61f + Mathf.Max(0f, tap) * 0.08f, 0.30f),
                    Quaternion.Euler(72f, t * 120f, -42f + tap * 8f),
                    new Vector3(0.035f, 0.32f, 0.035f)))
                matches++;
            return matches;
        }

        int CountCookBakeMotionMatches(float t)
        {
            int matches = 0;
            Vector3 trayCenter = BakeTrayCenter(t);
            matches += CountTrayMotionMatches(trayCenter,
                new Vector3(0.82f, 0.030f, 0.50f),
                new Vector3(0.40f, 0.035f, 0.30f));

            float doorOpen = BakeDoorOpen(t);
            float ovenBreath = 1f + t * 0.015f +
                Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI * 5f) * 0.025f;
            if (TransformMatches(_ovenProps[0], true, new Vector3(-0.48f, 0.59f, -0.20f),
                    Quaternion.identity, new Vector3(0.44f, 0.34f, 0.30f) * ovenBreath))
                matches++;
            if (TransformMatches(_ovenProps[1], true,
                    new Vector3(-0.48f, 0.61f - doorOpen * 0.055f,
                        -0.035f + doorOpen * 0.035f),
                    Quaternion.Euler(-doorOpen * 42f, 0f, 0f),
                    new Vector3(0.30f, 0.16f, 0.025f)))
                matches++;
            if (TransformMatches(_ovenProps[2], true, new Vector3(-0.58f, 0.75f, -0.03f),
                    Quaternion.Euler(90f, t * 210f, 0f),
                    new Vector3(0.045f, 0.020f, 0.045f)))
                matches++;
            if (TransformMatches(_ovenProps[3], true, new Vector3(-0.39f, 0.75f, -0.03f),
                    Quaternion.Euler(90f, -t * 165f, 0f),
                    new Vector3(0.045f, 0.020f, 0.045f)))
                matches++;

            for (int i = 0; i < 3; i++)
            {
                float phase = Mathf.Repeat(t * 2.3f + i * 0.31f, 1f);
                float scale = Mathf.Lerp(0.90f, 0.10f, phase);
                if (TransformMatches(_steam[i], t > 0.16f,
                        new Vector3(
                            -0.48f + Mathf.Sin(phase * Mathf.PI * 2f + i) * (0.10f + i * 0.025f),
                            0.73f + phase * 0.82f,
                            -0.08f + Mathf.Cos(phase * Mathf.PI * 2f + i) * (0.06f + i * 0.018f)),
                        Quaternion.identity,
                        new Vector3(0.07f + i * 0.015f, 0.19f, 0.07f) * scale))
                    matches++;

                float reveal = CookCookieReveal(2, t, i);
                Vector3 cookiePosition = trayCenter + new Vector3(-0.24f + i * 0.24f,
                    0.035f + Mathf.Sin(reveal * Mathf.PI) * 0.06f,
                    i == 1 ? -0.08f : 0.07f);
                if (TransformMatches(_cookies[i], reveal > 0f, cookiePosition,
                        Quaternion.Euler(0f, 24f + i * 32f, 0f),
                        Vector3.Lerp(new Vector3(0.16f, 0.035f, 0.16f),
                            new Vector3(0.19f, 0.045f, 0.19f), 0f) * reveal))
                    matches++;
            }

            float roll = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 1.35f));
            if (TransformMatches(_cookProps[0], true,
                    new Vector3(Mathf.Lerp(-0.52f, 0.28f, roll),
                        0.54f + roll * 0.04f + Mathf.Sin(t * Mathf.PI) * 0.08f, 0.24f),
                    Quaternion.Euler(88f, t * 260f, 70f),
                    new Vector3(0.055f, 0.42f, 0.055f)))
                matches++;
            return matches;
        }

        int CountCookPresentMotionMatches(float t)
        {
            int matches = 0;
            float present = CookPresentProgress(t);
            Vector3 trayCenter = Vector3.Lerp(new Vector3(0.45f, 0.46f, 0.18f),
                new Vector3(0.16f, 0.60f, 0.12f), present);
            matches += CountTrayMotionMatches(trayCenter,
                Vector3.Lerp(new Vector3(0.82f, 0.030f, 0.50f),
                    new Vector3(0.92f, 0.035f, 0.58f), present),
                Vector3.Lerp(new Vector3(0.40f, 0.035f, 0.30f),
                    new Vector3(0.52f, 0.055f, 0.36f), present));

            float working = 1f - present;
            float squeeze = Mathf.Sin(t * Mathf.PI * 4f) * 0.08f * working;
            if (TransformMatches(_decorateProps[0], true,
                    EvaluateCookGesturePropPosition(3, t, _gestureSample),
                    Quaternion.Euler(22f, t * 120f, 34f - squeeze * 80f),
                    new Vector3(0.10f, 0.34f, 0.10f)))
                matches++;
            float sprinklePass = Mathf.Sin(Mathf.Clamp01((t - 0.18f) * 2.2f) * Mathf.PI);
            if (TransformMatches(_decorateProps[1], true,
                    new Vector3(0.67f - sprinklePass * 0.34f,
                        0.58f + sprinklePass * 0.22f, -0.16f + sprinklePass * 0.22f),
                    Quaternion.Euler(0f, t * 110f, -sprinklePass * 38f),
                    new Vector3(0.12f, 0.16f, 0.12f)))
                matches++;
            if (TransformMatches(_decorateProps[2], true,
                    Vector3.Lerp(new Vector3(Mathf.Sin(t * Mathf.PI * 6f) * 0.08f,
                            0.84f + Mathf.Abs(Mathf.Sin(t * Mathf.PI * 3f)) * 0.11f, 0.18f),
                        new Vector3(0.16f, 0.735f, 0.02f), 1f - working),
                    Quaternion.identity,
                    Vector3.one * Mathf.Lerp(
                        0.09f + Mathf.Sin(t * Mathf.PI * 5f) * 0.025f, 0.12f, 1f - working)))
                matches++;
            if (TransformMatches(_decorateProps[3], true,
                    new Vector3(0.16f, 0.585f, 0.39f), Quaternion.Euler(0f, 22f, 0f),
                    new Vector3(0.48f, 0.020f, 0.045f) *
                    Mathf.Lerp(0.25f, 1f, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 2.4f))) *
                    (1f + Mathf.Sin(t * Mathf.PI * 5f) * 0.06f * working)))
                matches++;

            for (int i = 0; i < 3; i++)
            {
                Vector3 bakePosition = trayCenter + new Vector3(-0.24f + i * 0.24f,
                    0.035f, i == 1 ? -0.08f : 0.07f);
                Vector3 decorPosition = Vector3.Lerp(
                    new Vector3(0.20f + i * 0.24f, 0.53f, i == 1 ? 0.10f : 0.24f),
                    new Vector3(-0.08f + i * 0.24f, 0.66f, i == 1 ? 0.02f : 0.20f),
                    present);
                Vector3 cookiePosition = Vector3.Lerp(bakePosition, decorPosition,
                    Mathf.Clamp01(t * 3f));
                Vector3 cookieScale = Vector3.Lerp(new Vector3(0.16f, 0.035f, 0.16f),
                    new Vector3(0.19f, 0.045f, 0.19f), Mathf.Clamp01(t * 3f)) *
                    (1f + Mathf.Sin(t * Mathf.PI * 4f + i) * 0.05f);
                Quaternion cookieRotation = Quaternion.Euler(
                    0f, 24f + i * 32f + t * 65f, 0f);
                if (TransformMatches(_cookies[i], true, cookiePosition, cookieRotation, cookieScale))
                    matches++;

                for (int mark = 0; mark < 3; mark++)
                {
                    int detailIndex = i * 3 + mark;
                    float decorReveal = CookDetailReveal(t, detailIndex);
                    if (TransformMatches(_cookieDetails[detailIndex], decorReveal > 0.65f,
                            cookiePosition + cookieRotation *
                                CookGestureImprintOffset(detailIndex, _gestureSample),
                            cookieRotation * CookGestureImprintRotation(
                                detailIndex, _gestureSample),
                            new Vector3(0.05f, 0.012f, 0.012f) * decorReveal))
                        matches++;
                }
            }
            return matches;
        }

        int CountTrayMotionMatches(Vector3 trayCenter, Vector3 trayScale, Vector3 footScale)
        {
            int matches = 0;
            if (TransformMatches(_servingProps[0], true, trayCenter, Quaternion.identity, trayScale))
                matches++;
            if (TransformMatches(_servingProps[1], true,
                    trayCenter + new Vector3(-0.48f, 0.02f, 0f), Quaternion.identity,
                    new Vector3(0.16f, 0.025f, 0.10f)))
                matches++;
            if (TransformMatches(_servingProps[2], true,
                    trayCenter + new Vector3(0.48f, 0.02f, 0f), Quaternion.identity,
                    new Vector3(0.16f, 0.025f, 0.10f)))
                matches++;
            if (TransformMatches(_servingProps[3], true,
                    trayCenter + new Vector3(0f, -0.075f, 0f), Quaternion.identity, footScale))
                matches++;
            return matches;
        }

        bool ValidateCookPhaseState(int step, float t)
        {
            bool basePrepHidden = !AnyActive(_ingredients) && !AnyActive(_pourStreams);
            bool bakeHidden = !AnyActive(_ovenProps) && !AnyActive(_steam) &&
                !AnyActive(_cookies) && !AnyActive(_servingProps);
            bool decorateHidden = !AnyActive(_decorateProps) && !AnyActive(_cookieDetails);
            if (step == 0)
            {
                bool streamStatesReady = true;
                for (int i = 0; i < 3; i++)
                {
                    float phase = CookIngredientPhase(t, i);
                    streamStatesReady &= IsActive(_pourStreams[i]) ==
                        (phase > 0.08f && phase < 0.92f);
                }
                return IsActive(_bowl) && IsActive(_bowlRim) && AllActive(_ingredients) &&
                    streamStatesReady && IsActive(_batter) == (t > 0.20f) &&
                    IsActive(_cookProps[1]) && IsActive(_cookProps[2]) &&
                    !IsActive(_whisk) && !IsActive(_cookProps[3]) && !IsActive(_cookProps[0]) &&
                    bakeHidden && decorateHidden;
            }
            if (step == 1)
            {
                return IsActive(_bowl) && IsActive(_bowlRim) && IsActive(_whisk) &&
                    IsActive(_batter) && IsActive(_cookProps[3]) && basePrepHidden &&
                    !IsActive(_cookProps[0]) && !IsActive(_cookProps[1]) &&
                    !IsActive(_cookProps[2]) && bakeHidden && decorateHidden;
            }
            if (step == 2)
            {
                bool steamStatesReady = true;
                bool cookieStatesReady = true;
                for (int i = 0; i < 3; i++)
                {
                    steamStatesReady &= IsActive(_steam[i]) == (t > 0.16f);
                    cookieStatesReady &= IsActive(_cookies[i]) == (CookCookieReveal(2, t, i) > 0f);
                }
                bool trayCrossingDoor = IsBakeTrayCrossingDoor(t);
                bool actualDoorFullyOpen = TransformMatches(_ovenProps[1], true,
                    new Vector3(-0.48f, 0.555f, 0f), Quaternion.Euler(-42f, 0f, 0f),
                    new Vector3(0.30f, 0.16f, 0.025f));
                return !IsActive(_bowl) && !IsActive(_bowlRim) && !IsActive(_batter) &&
                    !IsActive(_whisk) && basePrepHidden && AllActive(_ovenProps) &&
                    AllActive(_servingProps) && IsActive(_cookProps[0]) &&
                    !IsActive(_cookProps[1]) && !IsActive(_cookProps[2]) &&
                    !IsActive(_cookProps[3]) &&
                    steamStatesReady && cookieStatesReady && !AnyActive(_cookieDetails) &&
                    !AnyActive(_decorateProps) && (!trayCrossingDoor || actualDoorFullyOpen);
            }
            if (step == 3)
            {
                bool detailStatesReady = true;
                for (int i = 0; i < 9; i++)
                    detailStatesReady &= IsActive(_cookieDetails[i]) ==
                        (CookDetailReveal(t, i) > 0.65f);
                return !IsActive(_bowl) && !IsActive(_bowlRim) && !IsActive(_batter) &&
                    !IsActive(_whisk) && basePrepHidden && !AnyActive(_ovenProps) &&
                    !AnyActive(_steam) && !AnyActive(_cookProps) &&
                    AllActive(_decorateProps) && AllActive(_cookies) &&
                    AllActive(_servingProps) && detailStatesReady;
            }
            return false;
        }

        static float CookIngredientPhase(float t, int index) =>
            Mathf.Clamp01((t - index * 0.12f) * 1.72f);

        static Vector3 CookIngredientStart(int index) =>
            new(-0.64f + index * 0.64f, 1.10f + (index % 2) * 0.16f,
                index == 1 ? -0.20f : 0.18f);

        static float CookPresentProgress(float t) =>
            Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.68f) * 3.125f));

        static float CookCookieReveal(int step, float t, int index) =>
            step == 2 ? Mathf.Clamp01((t - 0.18f - index * 0.08f) * 5f) :
            step == 3 ? 1f : 0f;

        static float CookDetailReveal(float t, int detailIndex) =>
            Mathf.Clamp01((t - 0.18f - detailIndex * 0.025f) * 5f);

        static Vector3 CookGestureImprintOffset(int detailIndex,
            MoonlightGestureSample sample)
        {
            float progress = Mathf.Clamp(detailIndex, 0, 8) / 8f;
            Vector3 point = EvaluateCookGesturePath(3, progress, sample);
            return new Vector3((point.x - CookDecorCenterX) * 0.22f, 0.050f,
                (point.z - CookDecorCenterZ) * 0.28f);
        }

        static Quaternion CookGestureImprintRotation(int detailIndex,
            MoonlightGestureSample sample)
        {
            float progress = Mathf.Clamp(detailIndex, 0, 8) / 8f;
            Vector3 before = EvaluateCookGesturePath(3, progress - 0.02f, sample);
            Vector3 after = EvaluateCookGesturePath(3, progress + 0.02f, sample);
            Vector3 tangent = after - before;
            if (tangent.sqrMagnitude <= 0.000001f) return Quaternion.identity;
            float yaw = -Mathf.Atan2(tangent.z, tangent.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, yaw, 0f);
        }

        bool CookCookieMarkTransformsMatch(float t)
        {
            if (!AllAssigned(_cookies, 3) || !AllAssigned(_cookieDetails, 9)) return false;
            for (int detailIndex = 0; detailIndex < 9; detailIndex++)
            {
                int cookieIndex = detailIndex / 3;
                float reveal = CookDetailReveal(t, detailIndex);
                Transform cookie = _cookies[cookieIndex];
                if (!TransformMatches(_cookieDetails[detailIndex], reveal > 0.65f,
                        cookie.localPosition + cookie.localRotation *
                            CookGestureImprintOffset(detailIndex, _gestureSample),
                        cookie.localRotation * CookGestureImprintRotation(
                            detailIndex, _gestureSample),
                        new Vector3(0.05f, 0.012f, 0.012f) * reveal))
                    return false;
            }
            return true;
        }

        static Vector3 BakeTrayCenter(float t)
        {
            float load = Mathf.SmoothStep(0f, 1f,
                Mathf.InverseLerp(BakeLoadStart, BakeLoadEnd, t));
            float extract = Mathf.SmoothStep(0f, 1f,
                Mathf.InverseLerp(BakeExtractStart, BakeExtractEnd, t));
            Vector3 trayCenter = Vector3.Lerp(new Vector3(0.45f, 0.46f, 0.18f),
                new Vector3(-0.40f, 0.52f, -0.015f), load);
            return Vector3.Lerp(trayCenter, new Vector3(0.34f, 0.51f, 0.15f), extract);
        }

        static float BakeDoorOpen(float t)
        {
            float close = Mathf.SmoothStep(0f, 1f,
                Mathf.InverseLerp(BakeDoorCloseStart, BakeDoorCloseEnd, t));
            float reopen = Mathf.SmoothStep(0f, 1f,
                Mathf.InverseLerp(BakeDoorReopenStart, BakeDoorReopenEnd, t));
            return Mathf.Max(1f - close, reopen);
        }

        static bool IsBakeTrayCrossingDoor(float t) =>
            (t > BakeLoadStart && t < BakeLoadEnd) ||
            (t > BakeExtractStart && t < BakeExtractEnd);

        static bool TransformMatches(Transform transform, bool expectedActive,
            Vector3 expectedPosition, Quaternion expectedRotation, Vector3 expectedScale)
        {
            return transform != null && IsActive(transform) == expectedActive &&
                Vector3.SqrMagnitude(transform.localPosition - expectedPosition) <= 0.000001f &&
                Quaternion.Angle(transform.localRotation, expectedRotation) <= 0.10f &&
                Vector3.SqrMagnitude(transform.localScale - expectedScale) <= 0.000001f;
        }

        static bool AllAssigned(Transform[] transforms, int requiredCount)
        {
            if (transforms == null || transforms.Length < requiredCount) return false;
            for (int i = 0; i < requiredCount; i++)
                if (transforms[i] == null) return false;
            return true;
        }

        static bool IsActive(Transform transform) =>
            transform != null && transform.gameObject.activeSelf;

        static bool AllActive(Transform[] transforms)
        {
            if (transforms == null || transforms.Length == 0) return false;
            for (int i = 0; i < transforms.Length; i++)
                if (!IsActive(transforms[i])) return false;
            return true;
        }

        static bool AnyActive(Transform[] transforms)
        {
            if (transforms == null) return false;
            for (int i = 0; i < transforms.Length; i++)
                if (IsActive(transforms[i])) return true;
            return false;
        }

        public static Vector3 EvaluateCookGesturePath(int stepIndex, float progress,
            MoonlightGestureSample sample)
        {
            int step = Mathf.Clamp(stepIndex, 0, 3);
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            if (step != 1 && step != 3) return Vector3.zero;

            Vector2 minimum = sample[0];
            Vector2 maximum = minimum;
            for (int i = 1; i < MoonlightGestureSample.ResampledPointCount; i++)
            {
                Vector2 point = sample[i];
                minimum = Vector2.Min(minimum, point);
                maximum = Vector2.Max(maximum, point);
            }

            Vector2 center = (minimum + maximum) * 0.5f;
            Vector2 gesturePoint = InterpolateSamplePoint(sample, t) - center;
            if (step == 1)
            {
                return new Vector3(
                    Mathf.Clamp(gesturePoint.x * 0.50f,
                        -CookCircleMaximumX, CookCircleMaximumX),
                    CookGestureY,
                    Mathf.Clamp(gesturePoint.y * 0.375f,
                        -CookCircleMaximumZ, CookCircleMaximumZ));
            }

            return new Vector3(
                Mathf.Clamp(CookDecorCenterX + gesturePoint.x * 0.72f,
                    CookDecorMinimumX, CookDecorMaximumX),
                CookGestureY,
                Mathf.Clamp(CookDecorCenterZ + gesturePoint.y * 0.36f,
                    CookDecorMinimumZ, CookDecorMaximumZ));
        }

        public static Vector3 EvaluateCookGesturePropPosition(int stepIndex, float progress,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            Vector3 gesturePosition = EvaluateCookGesturePath(stepIndex, t, sample);
            return stepIndex == 3
                ? Vector3.Lerp(gesturePosition, CookDecorParkedPosition,
                    CookPresentProgress(t))
                : gesturePosition;
        }

        public static bool ValidateGestureResponsiveCookContract(out string detail)
        {
            MoonlightGestureSample clockwise = CookCircleSample(0.34f, true);
            MoonlightGestureSample counterClockwise = CookCircleSample(0.34f, false);
            MoonlightGestureSample narrowCircle = CookCircleSample(0.16f, false);
            MoonlightGestureSample wideCircle = CookCircleSample(0.34f, false);
            MoonlightGestureSample narrowZigZag = CookZigZagSample(0.15f, 0.28f, false);
            MoonlightGestureSample wideZigZag = CookZigZagSample(0.40f, 0.46f, false);
            MoonlightGestureSample reverseZigZag = CookZigZagSample(0.40f, 0.46f, true);

            bool oppositeCircleTraversal = true;
            bool finiteAndBounded = true;
            for (int i = 0; i <= 40; i++)
            {
                float t = i / 40f;
                Vector3 clockwisePoint = EvaluateCookGesturePath(1, t, clockwise);
                Vector3 oppositePoint = EvaluateCookGesturePath(1, 1f - t, counterClockwise);
                oppositeCircleTraversal &=
                    Vector3.SqrMagnitude(clockwisePoint - oppositePoint) <= 0.000001f;

                finiteAndBounded &= CookGesturePointIsFiniteAndBounded(1,
                    clockwisePoint) && CookGesturePointIsFiniteAndBounded(1,
                    EvaluateCookGesturePath(1, t, narrowCircle)) &&
                    CookGesturePointIsFiniteAndBounded(3,
                        EvaluateCookGesturePath(3, t, wideZigZag)) &&
                    CookGesturePointIsFiniteAndBounded(3,
                        EvaluateCookGesturePath(3, t, reverseZigZag));
            }
            oppositeCircleTraversal &=
                EvaluateCookGesturePath(1, 0.25f, clockwise).z < 0f &&
                EvaluateCookGesturePath(1, 0.25f, counterClockwise).z > 0f;

            float narrowCircleSpan = CookGesturePathSpan(1, narrowCircle, true);
            float wideCircleSpan = CookGesturePathSpan(1, wideCircle, true);
            float narrowDecorSpan = CookGesturePathSpan(3, narrowZigZag, true);
            float wideDecorSpan = CookGesturePathSpan(3, wideZigZag, true);
            bool distinctSpans = wideCircleSpan > narrowCircleSpan + 0.10f &&
                wideDecorSpan > narrowDecorSpan + 0.20f;
            bool oppositeDecorDirection = Vector3.Distance(
                    EvaluateCookGesturePath(3, 0f, wideZigZag),
                    EvaluateCookGesturePath(3, 1f, reverseZigZag)) <= 0.0001f &&
                Vector3.Distance(EvaluateCookGesturePath(3, 1f, wideZigZag),
                    EvaluateCookGesturePath(3, 0f, reverseZigZag)) <= 0.0001f;
            bool runtimeShapeGate = CookGestureSampleHasMinimumPathSpan(1, narrowCircle) &&
                CookGestureSampleHasMinimumPathSpan(3, narrowZigZag) &&
                CookGestureTraversalMatchesPath(1, clockwise) &&
                CookGestureTraversalMatchesPath(1, counterClockwise) &&
                CookGestureTraversalMatchesPath(3, wideZigZag) &&
                CookGestureTraversalMatchesPath(3, reverseZigZag) &&
                CookGestureDistinctImprintCount(wideZigZag) == 9;
            MoonlightGestureSample flat = MoonlightGestureSample.Synthetic(
                MoonlightGestureKind.Tap, 0.95f);
            bool rejectsFlatGesture = !CookGestureSampleHasMinimumPathSpan(1, flat) &&
                !CookGestureSampleHasMinimumPathSpan(3, flat) &&
                !CookGestureTraversalMatchesPath(1, flat) &&
                !CookGestureTraversalMatchesPath(3, flat) &&
                CookGestureDistinctImprintCount(flat) == 1;
            finiteAndBounded &= CookGesturePointIsFiniteAndBounded(1,
                    EvaluateCookGesturePath(1, float.NaN, clockwise)) &&
                CookGesturePointIsFiniteAndBounded(3,
                    EvaluateCookGesturePath(3, float.PositiveInfinity, wideZigZag));

            detail = $"points={clockwise.PointCount} clockwiseOpposite={oppositeCircleTraversal} " +
                $"zigDirectionOpposite={oppositeDecorDirection} " +
                $"circleSpan={narrowCircleSpan:0.000}/{wideCircleSpan:0.000} " +
                $"decorSpan={narrowDecorSpan:0.000}/{wideDecorSpan:0.000} " +
                $"runtimeShape={runtimeShapeGate} rejectsFlat={rejectsFlatGesture} " +
                $"finiteBounds={finiteAndBounded} " +
                $"budgets={CookRendererBudget}r/{CookMaterialBudget}m/{CookLightBudget}l";
            return clockwise.HasSevenFiniteNormalizedPoints &&
                counterClockwise.HasSevenFiniteNormalizedPoints &&
                oppositeCircleTraversal && oppositeDecorDirection && distinctSpans &&
                runtimeShapeGate && rejectsFlatGesture &&
                finiteAndBounded && CookRendererBudget == 36 &&
                CookMaterialBudget == 24 && CookLightBudget == 1;
        }

        static MoonlightGestureSample CookCircleSample(float radius, bool clockwise)
        {
            var points = new Vector2[MoonlightGestureSample.ResampledPointCount];
            for (int i = 0; i < points.Length; i++)
            {
                float angle = i / (float)(points.Length - 1) * Mathf.PI * 2f;
                if (clockwise) angle = -angle;
                points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
            return MoonlightGestureSample.Create(0.95f, 0.8f, points);
        }

        static MoonlightGestureSample CookZigZagSample(float amplitude, float travel,
            bool reverse)
        {
            var points = new Vector2[MoonlightGestureSample.ResampledPointCount];
            for (int i = 0; i < points.Length; i++)
            {
                int sourceIndex = reverse ? points.Length - 1 - i : i;
                points[i] = new Vector2(sourceIndex % 2 == 0 ? -amplitude : amplitude,
                    Mathf.Lerp(-travel, travel, sourceIndex / (float)(points.Length - 1)));
            }
            return MoonlightGestureSample.Create(0.95f, 0.8f, points);
        }

        static float CookGesturePathSpan(int stepIndex, MoonlightGestureSample sample,
            bool xAxis)
        {
            float minimum = float.PositiveInfinity;
            float maximum = float.NegativeInfinity;
            for (int i = 0; i <= 40; i++)
            {
                Vector3 point = EvaluateCookGesturePath(stepIndex, i / 40f, sample);
                float coordinate = xAxis ? point.x : point.z;
                minimum = Mathf.Min(minimum, coordinate);
                maximum = Mathf.Max(maximum, coordinate);
            }
            return maximum - minimum;
        }

        static bool CookGestureSampleHasMinimumPathSpan(int stepIndex,
            MoonlightGestureSample sample)
        {
            if (!sample.HasSevenFiniteNormalizedPoints) return false;
            float xSpan = CookGesturePathSpan(stepIndex, sample, true);
            float zSpan = CookGesturePathSpan(stepIndex, sample, false);
            return stepIndex == 1
                ? xSpan >= 0.12f && zSpan >= 0.08f
                : stepIndex == 3 && xSpan >= 0.16f && zSpan >= 0.12f;
        }

        static bool CookGestureTraversalMatchesPath(int stepIndex,
            MoonlightGestureSample sample)
        {
            if (!sample.HasSevenFiniteNormalizedPoints) return false;
            if (stepIndex == 1)
            {
                float sampleArea = 0f;
                float pathArea = 0f;
                for (int i = 0; i < MoonlightGestureSample.ResampledPointCount - 1; i++)
                {
                    Vector2 from = sample[i];
                    Vector2 to = sample[i + 1];
                    sampleArea += from.x * to.y - to.x * from.y;
                    Vector3 pathFrom = EvaluateCookGesturePath(1,
                        i / (float)(MoonlightGestureSample.ResampledPointCount - 1), sample);
                    Vector3 pathTo = EvaluateCookGesturePath(1,
                        (i + 1f) / (MoonlightGestureSample.ResampledPointCount - 1), sample);
                    pathArea += pathFrom.x * pathTo.z - pathTo.x * pathFrom.z;
                }
                return Mathf.Abs(sampleArea) >= 0.08f && Mathf.Abs(pathArea) >= 0.004f &&
                    Mathf.Sign(sampleArea) == Mathf.Sign(pathArea);
            }
            if (stepIndex != 3) return false;
            float sampleDirection = sample.End.y - sample.Start.y;
            float pathDirection = EvaluateCookGesturePath(3, 1f, sample).z -
                EvaluateCookGesturePath(3, 0f, sample).z;
            return Mathf.Abs(sampleDirection) >= 0.12f && Mathf.Abs(pathDirection) >= 0.04f &&
                Mathf.Sign(sampleDirection) == Mathf.Sign(pathDirection);
        }

        static int CookGestureDistinctImprintCount(MoonlightGestureSample sample)
        {
            int distinctCount = 0;
            for (int i = 0; i < 9; i++)
            {
                Vector3 candidate = CookGestureImprintOffset(i, sample);
                bool distinct = true;
                for (int previous = 0; previous < i; previous++)
                {
                    if (Vector3.SqrMagnitude(candidate -
                            CookGestureImprintOffset(previous, sample)) <= 0.000004f)
                    {
                        distinct = false;
                        break;
                    }
                }
                if (distinct) distinctCount++;
            }
            return distinctCount;
        }

        static bool CookGesturePointIsFiniteAndBounded(int stepIndex, Vector3 point)
        {
            if (!IsFinite(point)) return false;
            if (stepIndex == 1)
                return Mathf.Abs(point.x) <= CookCircleMaximumX + 0.0001f &&
                    Mathf.Abs(point.z) <= CookCircleMaximumZ + 0.0001f &&
                    point.y >= CookGestureMinimumY && point.y <= CookGestureMaximumY;
            return point.x >= CookDecorMinimumX && point.x <= CookDecorMaximumX &&
                point.z >= CookDecorMinimumZ && point.z <= CookDecorMaximumZ &&
                point.y >= CookGestureMinimumY && point.y <= CookGestureMaximumY;
        }

        public static int GardenPlantSlotIndex(MoonlightGestureSample sample)
        {
            float tapX = sample.HasSevenFiniteNormalizedPoints ? sample.Start.x : 0f;
            return Mathf.Clamp(Mathf.RoundToInt(Mathf.InverseLerp(-0.80f, 0.80f, tapX) *
                (GardenMagicFlowerRequiredInstances - 1)), 0,
                GardenMagicFlowerRequiredInstances - 1);
        }

        public static Vector3 GardenPlantSlotPosition(int slotIndex)
        {
            int slot = Mathf.Clamp(slotIndex, 0, GardenMagicFlowerRequiredInstances - 1);
            return new Vector3(-0.42f + slot * 0.16f, 0.43f,
                slot % 2 == 0 ? -0.07f : 0.08f);
        }

        static int GardenPlantSlotForSeed(int seedIndex, int selectedSlot)
        {
            if (seedIndex == 2) return selectedSlot;
            int remainingIndex = seedIndex < 2 ? seedIndex : seedIndex - 1;
            for (int slot = 0; slot < GardenMagicFlowerRequiredInstances; slot++)
            {
                if (slot == selectedSlot) continue;
                if (remainingIndex-- == 0) return slot;
            }
            return selectedSlot;
        }

        public static Vector3 EvaluateGardenPlantSeedPosition(float progress,
            MoonlightGestureSample sample)
        {
            Vector3 target = GardenPlantSlotPosition(GardenPlantSlotIndex(sample));
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            float drop = Mathf.Clamp01(t * 3.2f - 0.44f);
            return Vector3.Lerp(target + new Vector3(-0.20f, 0.49f, -0.12f), target, drop);
        }

        public static Vector3 EvaluateGardenWaterPath(float progress,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            Vector2 center = GardenSampleBoundsCenter(sample);
            Vector2 point = InterpolateSamplePoint(sample, t) - center;
            Vector3 rose = GardenWateringCanBasePositions[2];
            return new Vector3(
                rose.x + Mathf.Clamp(point.x * 0.55f,
                    -GardenWaterMaximumXOffset, GardenWaterMaximumXOffset),
                rose.y,
                rose.z + Mathf.Clamp(point.y * 0.40f,
                    -GardenWaterMaximumZOffset, GardenWaterMaximumZOffset));
        }

        public static Vector3 EvaluateGardenTendPath(float progress,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            float scaled = t * (GardenMagicFlowerRequiredInstances - 1);
            int from = Mathf.Min(Mathf.FloorToInt(scaled),
                GardenMagicFlowerRequiredInstances - 1);
            int to = Mathf.Min(from + 1, GardenMagicFlowerRequiredInstances - 1);
            Vector2 point = Vector2.Lerp(sample[from], sample[to], scaled - from);
            return GardenTendPoint(point - GardenTendSampleCenter(sample));
        }

        public static Vector3 EvaluateGardenTendTarget(int targetIndex,
            MoonlightGestureSample sample)
        {
            int index = Mathf.Clamp(targetIndex, 0, GardenMagicFlowerRequiredInstances - 1);
            return GardenTendPoint(sample[index] - GardenTendSampleCenter(sample));
        }

        public static float EvaluateGardenTendScale(float progress, int targetIndex)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            int index = Mathf.Clamp(targetIndex, 0, GardenMagicFlowerRequiredInstances - 1);
            float anchor = index / (float)(GardenMagicFlowerRequiredInstances - 1);
            float previousAnchor = index == 0
                ? 0f
                : (index - 1f) / (GardenMagicFlowerRequiredInstances - 1);
            float tended = index == 0
                ? 1f
                : Mathf.InverseLerp(previousAnchor, anchor, t);
            return GardenBloomBaseScale *
                (tended + Mathf.Sin(tended * Mathf.PI) * 0.18f);
        }

        public static float EvaluateGardenBloomScale(float progress, int flowerIndex,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            int index = Mathf.Clamp(flowerIndex, 0, GardenMagicFlowerRequiredInstances - 1);
            float opening = Mathf.Clamp01(t * 3.2f - index * 0.08f);
            float overshoot = Mathf.Sin(opening * Mathf.PI) * 0.18f;
            float qualityScale = Mathf.Lerp(0.84f, 1.12f, GardenBloomQuality(sample));
            return GardenBloomBaseScale * (opening + overshoot) * qualityScale;
        }

        public static float EvaluateGardenBloomIntensity(MoonlightGestureSample sample) =>
            Mathf.Lerp(0.90f, 1.18f, GardenBloomQuality(sample));

        public static bool ValidateGestureResponsiveGardenContract(out string detail)
        {
            MoonlightGestureSample leftTap = GardenTapSample(-0.80f);
            MoonlightGestureSample rightTap = GardenTapSample(0.80f);
            MoonlightGestureSample clockwise = GardenCircleSample(0.34f, true);
            MoonlightGestureSample counterClockwise = GardenCircleSample(0.34f, false);
            MoonlightGestureSample zigZag = GardenZigZagSample(false);
            MoonlightGestureSample reverseZigZag = GardenZigZagSample(true);
            MoonlightGestureSample minimumBloom = GardenHoldSample(0.50f, 0.45f);
            MoonlightGestureSample perfectBloom = GardenHoldSample(0.95f, 1.00f);
            MoonlightGestureSample shortBloom = GardenHoldSample(0.75f, 0.45f);
            MoonlightGestureSample longBloom = GardenHoldSample(0.75f, 1.00f);

            float[] tapXOracle = { -0.80f, -0.40f, 0f, 0.40f, 0.80f };
            Vector3[] plantSlotOracle =
            {
                new(-0.42f, 0.43f, -0.07f),
                new(-0.26f, 0.43f, 0.08f),
                new(-0.10f, 0.43f, -0.07f),
                new(0.06f, 0.43f, 0.08f),
                new(0.22f, 0.43f, -0.07f)
            };
            float minimumTapSeparation = float.PositiveInfinity;
            bool plantOraclePass = true;
            int distinctTapSelections = 0;
            for (int slot = 0; slot < GardenMagicFlowerRequiredInstances; slot++)
            {
                Vector3 expectedPoint = plantSlotOracle[slot];
                MoonlightGestureSample slotTap = GardenTapSample(tapXOracle[slot]);
                if (GardenPlantSlotIndex(slotTap) == slot) distinctTapSelections++;
                plantOraclePass &= Vector3.Distance(
                        GardenPlantSlotPosition(slot), expectedPoint) <= 0.001f &&
                    expectedPoint.x >= -0.51f && expectedPoint.x <= 0.31f &&
                    expectedPoint.y >= 0.40f && expectedPoint.y <= 0.46f &&
                    expectedPoint.z >= -0.16f && expectedPoint.z <= 0.16f;
                for (int other = slot + 1; other < GardenMagicFlowerRequiredInstances; other++)
                    minimumTapSeparation = Mathf.Min(minimumTapSeparation,
                        Vector3.Distance(expectedPoint, plantSlotOracle[other]));
            }
            bool tapSelectionPass = GardenPlantSlotIndex(leftTap) == 0 &&
                GardenPlantSlotIndex(rightTap) == GardenMagicFlowerRequiredInstances - 1 &&
                distinctTapSelections == GardenMagicFlowerRequiredInstances &&
                plantOraclePass &&
                minimumTapSeparation >= 0.16f;

            bool finiteAndBounded = true;
            for (int sampleIndex = 0; sampleIndex <= 40; sampleIndex++)
            {
                float t = sampleIndex / 40f;
                finiteAndBounded &= GardenPlantPathIsFiniteAndBounded(
                        EvaluateGardenPlantSeedPosition(t, leftTap)) &&
                    GardenWaterPointIsFiniteAndBounded(EvaluateGardenWaterPath(
                        t, clockwise)) &&
                    GardenWaterPointIsFiniteAndBounded(EvaluateGardenWaterPath(
                        t, counterClockwise)) &&
                    GardenTendPointIsFiniteAndBounded(EvaluateGardenTendPath(t, zigZag)) &&
                    GardenTendPointIsFiniteAndBounded(EvaluateGardenTendPath(
                        t, reverseZigZag));
                for (int flower = 0; flower < GardenMagicFlowerRequiredInstances; flower++)
                {
                    finiteAndBounded &= GardenTendPointIsFiniteAndBounded(
                            EvaluateGardenTendTarget(flower, zigZag)) &&
                        GardenTendPointIsFiniteAndBounded(
                            EvaluateGardenTendTarget(flower, reverseZigZag));
                    float tendScale = EvaluateGardenTendScale(t, flower);
                    float minimumScale = EvaluateGardenBloomScale(t, flower, minimumBloom);
                    float perfectScale = EvaluateGardenBloomScale(t, flower, perfectBloom);
                    finiteAndBounded &= IsFinite(tendScale) && IsFinite(minimumScale) &&
                        IsFinite(perfectScale) && tendScale >= 0f && tendScale <= 0.66f &&
                        minimumScale >= 0f && perfectScale >= 0f &&
                        minimumScale <= 0.66f && perfectScale <= 0.66f;
                }
            }

            float clockwiseSourceArea = GardenSampleSignedArea(clockwise);
            float counterClockwiseSourceArea = GardenSampleSignedArea(counterClockwise);
            float clockwiseArea = GardenWaterPathSignedArea(clockwise);
            float counterClockwiseArea = GardenWaterPathSignedArea(counterClockwise);
            bool waterDirectionPass = clockwiseSourceArea < -0.08f &&
                counterClockwiseSourceArea > 0.08f &&
                clockwiseArea < -0.01f && counterClockwiseArea > 0.01f &&
                Mathf.Sign(clockwiseSourceArea) == Mathf.Sign(clockwiseArea) &&
                Mathf.Sign(counterClockwiseSourceArea) ==
                    Mathf.Sign(counterClockwiseArea);
            bool waterOraclePass = Vector3.Distance(
                    EvaluateGardenWaterPath(0f, counterClockwise),
                    new Vector3(0.387f, 0.43f, -0.13f)) <= 0.001f &&
                Vector3.Distance(EvaluateGardenWaterPath(0.25f, counterClockwise),
                    new Vector3(0.20f, 0.43f, -0.0122f)) <= 0.001f &&
                Vector3.Distance(EvaluateGardenWaterPath(0.25f, clockwise),
                    new Vector3(0.20f, 0.43f, -0.2478f)) <= 0.001f;
            int zigZagTargets = GardenDistinctTendTargetCount(zigZag);
            int reverseTargets = GardenDistinctTendTargetCount(reverseZigZag);
            int zigZagInversions = GardenTendInversionCount(zigZag);
            int reverseInversions = GardenTendInversionCount(reverseZigZag);
            Vector3[] zigZagTargetOracle =
            {
                new(-0.37f, 0.60f, -0.0784f),
                new(0.17f, 0.60f, -0.0392f),
                new(-0.37f, 0.60f, 0f),
                new(0.17f, 0.60f, 0.0392f),
                new(-0.37f, 0.60f, 0.0784f)
            };
            bool zigZagOraclePass = true;
            bool tendGrowthAnchorPass = true;
            for (int target = 0; target < GardenMagicFlowerRequiredInstances; target++)
            {
                float anchor = target /
                    (float)(GardenMagicFlowerRequiredInstances - 1);
                zigZagOraclePass &= Vector3.Distance(
                        EvaluateGardenTendTarget(target, zigZag),
                        zigZagTargetOracle[target]) <= 0.001f &&
                    Vector3.Distance(EvaluateGardenTendPath(anchor, zigZag),
                        zigZagTargetOracle[target]) <= 0.001f;
                tendGrowthAnchorPass &= GardenTendTargetIndexAtProgress(anchor) == target;
                for (int flower = 0; flower < GardenMagicFlowerRequiredInstances; flower++)
                {
                    float expectedScale = flower <= target ? GardenBloomBaseScale : 0f;
                    tendGrowthAnchorPass &= Mathf.Abs(
                        EvaluateGardenTendScale(anchor, flower) - expectedScale) <= 0.001f;
                }
            }
            bool tendAnchorPass = GardenTendToolMatchesTargetsAtAnchors(zigZag) &&
                GardenTendToolMatchesTargetsAtAnchors(reverseZigZag);
            bool zigZagPass = zigZagTargets == GardenMagicFlowerRequiredInstances &&
                reverseTargets == GardenMagicFlowerRequiredInstances &&
                zigZagInversions >= 3 && reverseInversions >= 3 &&
                zigZagOraclePass && tendAnchorPass && tendGrowthAnchorPass;

            float minimumOpening = EvaluateGardenBloomScale(1f, 2, minimumBloom);
            float perfectOpening = EvaluateGardenBloomScale(1f, 2, perfectBloom);
            float minimumIntensity = EvaluateGardenBloomIntensity(minimumBloom);
            float perfectIntensity = EvaluateGardenBloomIntensity(perfectBloom);
            float openingDelta = (perfectOpening - minimumOpening) /
                Mathf.Max(0.0001f, minimumOpening);
            float intensityDelta = (perfectIntensity - minimumIntensity) /
                Mathf.Max(0.0001f, minimumIntensity);
            float shortDurationOpening = EvaluateGardenBloomScale(1f, 2, shortBloom);
            float longDurationOpening = EvaluateGardenBloomScale(1f, 2, longBloom);
            float shortDurationIntensity = EvaluateGardenBloomIntensity(shortBloom);
            float longDurationIntensity = EvaluateGardenBloomIntensity(longBloom);
            float durationOpeningDelta = (longDurationOpening - shortDurationOpening) /
                Mathf.Max(0.0001f, shortDurationOpening);
            float durationIntensityDelta = (longDurationIntensity - shortDurationIntensity) /
                Mathf.Max(0.0001f, shortDurationIntensity);
            bool durationResponsive = durationOpeningDelta >= 0.12f &&
                durationIntensityDelta >= 0.12f;
            bool bloomPass = openingDelta >= 0.12f && intensityDelta >= 0.12f &&
                durationResponsive;
            bool unchangedBudget = GardenRendererBudget == 48 &&
                GardenMaterialBudget == 28 && GardenLightBudget == 1 &&
                GardenMagicFlowerMaxRenderers == 10;

            detail = $"samples=41 slots={distinctTapSelections}/5 oracle={plantOraclePass} " +
                $"tapSelection={GardenPlantSlotIndex(leftTap)}/{GardenPlantSlotIndex(rightTap)} " +
                $"tapSeparation={minimumTapSeparation:0.000}m " +
                $"circleSourceArea={clockwiseSourceArea:0.0000}/" +
                $"{counterClockwiseSourceArea:0.0000} worldArea=" +
                $"{clockwiseArea:0.0000}/{counterClockwiseArea:0.0000} " +
                $"waterDirection={waterDirectionPass} oracle={waterOraclePass} " +
                $"zigzag={zigZagTargets}/{reverseTargets} targets " +
                $"inversions={zigZagInversions}/{reverseInversions} " +
                $"anchors={tendAnchorPass} growth={tendGrowthAnchorPass} " +
                $"oracle={zigZagOraclePass} " +
                $"bloomDelta={openingDelta:P0}/{intensityDelta:P0} " +
                $"durationDelta={durationOpeningDelta:P0}/{durationIntensityDelta:P0} " +
                $"finiteBounds={finiteAndBounded} " +
                $"budgets={GardenRendererBudget}r/{GardenMaterialBudget}m/" +
                $"{GardenLightBudget}l flowers={GardenMagicFlowerMaxRenderers}r";
            return leftTap.HasSevenFiniteNormalizedPoints &&
                rightTap.HasSevenFiniteNormalizedPoints && tapSelectionPass &&
                waterDirectionPass && waterOraclePass && zigZagPass && bloomPass &&
                finiteAndBounded && unchangedBudget;
        }

        static Vector2 GardenSampleBoundsCenter(MoonlightGestureSample sample)
        {
            Vector2 minimum = sample[0];
            Vector2 maximum = minimum;
            for (int i = 1; i < MoonlightGestureSample.ResampledPointCount; i++)
            {
                minimum = Vector2.Min(minimum, sample[i]);
                maximum = Vector2.Max(maximum, sample[i]);
            }
            return (minimum + maximum) * 0.5f;
        }

        static Vector2 GardenTendSampleCenter(MoonlightGestureSample sample)
        {
            Vector2 minimum = sample[0];
            Vector2 maximum = minimum;
            for (int i = 1; i < GardenMagicFlowerRequiredInstances; i++)
            {
                minimum = Vector2.Min(minimum, sample[i]);
                maximum = Vector2.Max(maximum, sample[i]);
            }
            return (minimum + maximum) * 0.5f;
        }

        static Vector3 GardenTendPoint(Vector2 centeredPoint) => new(
            Mathf.Clamp(-0.10f + centeredPoint.x * 0.75f,
                GardenTendMinimumX, GardenTendMaximumX),
            0.60f,
            Mathf.Clamp(centeredPoint.y * 0.28f,
                -GardenTendMaximumZ, GardenTendMaximumZ));

        static float GardenBloomQuality(MoonlightGestureSample sample)
        {
            float score = Mathf.InverseLerp(0.50f, 0.95f,
                IsFinite(sample.Score) ? sample.Score : 0f);
            float duration = Mathf.InverseLerp(0.45f, 1.00f,
                IsFinite(sample.Duration) ? sample.Duration : 0f);
            return Mathf.Clamp01(score * 0.55f + duration * 0.45f);
        }

        static float GardenSampleSignedArea(MoonlightGestureSample sample)
        {
            float area = 0f;
            for (int i = 0; i < MoonlightGestureSample.ResampledPointCount - 1; i++)
            {
                Vector2 from = sample[i];
                Vector2 to = sample[i + 1];
                area += from.x * to.y - to.x * from.y;
            }
            return area * 0.5f;
        }

        static float GardenWaterPathSignedArea(MoonlightGestureSample sample)
        {
            float area = 0f;
            for (int i = 0; i < MoonlightGestureSample.ResampledPointCount - 1; i++)
            {
                Vector3 from = EvaluateGardenWaterPath(
                    i / (float)(MoonlightGestureSample.ResampledPointCount - 1), sample);
                Vector3 to = EvaluateGardenWaterPath(
                    (i + 1f) / (MoonlightGestureSample.ResampledPointCount - 1), sample);
                area += from.x * to.z - to.x * from.z;
            }
            return area * 0.5f;
        }

        static int GardenTendInversionCount(MoonlightGestureSample sample)
        {
            int inversions = 0;
            float previousDirection = 0f;
            Vector3 previous = EvaluateGardenTendTarget(0, sample);
            for (int i = 1; i < GardenMagicFlowerRequiredInstances; i++)
            {
                Vector3 current = EvaluateGardenTendTarget(i, sample);
                float direction = Mathf.Sign(current.x - previous.x);
                if (direction != 0f)
                {
                    if (previousDirection != 0f && direction != previousDirection) inversions++;
                    previousDirection = direction;
                }
                previous = current;
            }
            return inversions;
        }

        static int GardenTendTargetIndexAtProgress(float progress)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            return Mathf.Clamp(Mathf.CeilToInt(
                    t * (GardenMagicFlowerRequiredInstances - 1)),
                0, GardenMagicFlowerRequiredInstances - 1);
        }

        static bool GardenTendToolMatchesTargetsAtAnchors(MoonlightGestureSample sample)
        {
            for (int i = 0; i < GardenMagicFlowerRequiredInstances; i++)
            {
                float anchor = i / (float)(GardenMagicFlowerRequiredInstances - 1);
                if (Vector3.Distance(EvaluateGardenTendPath(anchor, sample),
                        EvaluateGardenTendTarget(i, sample)) > 0.001f)
                    return false;
            }
            return true;
        }

        static int GardenDistinctTendTargetCount(MoonlightGestureSample sample)
        {
            int distinct = 0;
            for (int i = 0; i < GardenMagicFlowerRequiredInstances; i++)
            {
                Vector3 candidate = EvaluateGardenTendTarget(i, sample);
                bool unique = true;
                for (int previous = 0; previous < i; previous++)
                {
                    if (Vector3.Distance(candidate,
                            EvaluateGardenTendTarget(previous, sample)) <= 0.001f)
                    {
                        unique = false;
                        break;
                    }
                }
                if (unique) distinct++;
            }
            return distinct;
        }

        static bool GardenPlantPointIsInsidePlanter(Vector3 point) => IsFinite(point) &&
            point.x >= -0.51f && point.x <= 0.31f &&
            point.y >= 0.40f && point.y <= 0.46f &&
            point.z >= -0.16f && point.z <= 0.16f;

        static bool GardenPlantPathIsFiniteAndBounded(Vector3 point) => IsFinite(point) &&
            point.x >= -0.71f && point.x <= 0.31f &&
            point.y >= 0.40f && point.y <= 0.92f &&
            point.z >= -0.19f && point.z <= 0.16f;

        static bool GardenWaterPointIsFiniteAndBounded(Vector3 point)
        {
            Vector3 rose = GardenWateringCanBasePositions[2];
            return IsFinite(point) &&
                Mathf.Abs(point.x - rose.x) <= GardenWaterMaximumXOffset + 0.0001f &&
                Mathf.Abs(point.z - rose.z) <= GardenWaterMaximumZOffset + 0.0001f &&
                Mathf.Abs(point.y - rose.y) <= 0.0001f;
        }

        static bool GardenTendPointIsFiniteAndBounded(Vector3 point) => IsFinite(point) &&
            point.x >= GardenTendMinimumX && point.x <= GardenTendMaximumX &&
            Mathf.Abs(point.y - 0.60f) <= 0.0001f &&
            Mathf.Abs(point.z) <= GardenTendMaximumZ + 0.0001f;

        static MoonlightGestureSample GardenTapSample(float x)
        {
            var points = new Vector2[MoonlightGestureSample.ResampledPointCount];
            for (int i = 0; i < points.Length; i++) points[i] = new Vector2(x, 0f);
            return MoonlightGestureSample.Create(0.95f, 0.12f, points);
        }

        static MoonlightGestureSample GardenCircleSample(float radius, bool clockwise)
        {
            var points = new Vector2[MoonlightGestureSample.ResampledPointCount];
            for (int i = 0; i < points.Length; i++)
            {
                float angle = i / (float)(points.Length - 1) * Mathf.PI * 2f;
                if (clockwise) angle = -angle;
                points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
            return MoonlightGestureSample.Create(0.95f, 0.8f, points);
        }

        static MoonlightGestureSample GardenZigZagSample(bool reverse)
        {
            var points = new Vector2[MoonlightGestureSample.ResampledPointCount];
            for (int i = 0; i < points.Length; i++)
            {
                int sourceIndex = reverse ? points.Length - 1 - i : i;
                points[i] = new Vector2(sourceIndex % 2 == 0 ? -0.36f : 0.36f,
                    Mathf.Lerp(-0.42f, 0.42f,
                        sourceIndex / (float)(points.Length - 1)));
            }
            return MoonlightGestureSample.Create(0.95f, 0.8f, points);
        }

        static MoonlightGestureSample GardenHoldSample(float score, float duration)
        {
            var points = new Vector2[MoonlightGestureSample.ResampledPointCount];
            return MoonlightGestureSample.Create(score, duration, points);
        }

        bool GardenFlowerTargetsMatchGesture()
        {
            if (_flowers == null || _flowers.Length != GardenMagicFlowerRequiredInstances)
                return false;
            for (int i = 0; i < _flowers.Length; i++)
            {
                if (_flowers[i] == null || Vector3.Distance(_flowers[i].localPosition,
                        EvaluateGardenTendTarget(i, _gestureSample) + Vector3.down * 0.18f) >
                        0.001f ||
                    Mathf.Abs(_flowers[i].localScale.x -
                        EvaluateGardenTendScale(_gardenProgress, i)) > 0.001f)
                    return false;
            }
            return true;
        }

        bool GardenCurrentTendTargetMatchesProgress()
        {
            if (_flowers == null || _flowers.Length != GardenMagicFlowerRequiredInstances)
                return false;
            int current = GardenTendTargetIndexAtProgress(_gardenProgress);
            for (int i = 0; i < _flowers.Length; i++)
            {
                if (_flowers[i] == null) return false;
                float scale = _flowers[i].localScale.x;
                if (i < current && Mathf.Abs(scale - GardenBloomBaseScale) > 0.001f)
                    return false;
                if (i > current && scale > 0.001f)
                    return false;
            }
            return Vector3.Distance(_flowers[current].localPosition,
                       EvaluateGardenTendTarget(current, _gestureSample) +
                       Vector3.down * 0.18f) <= 0.001f &&
                Mathf.Abs(_flowers[current].localScale.x -
                    EvaluateGardenTendScale(_gardenProgress, current)) <= 0.001f;
        }

        public static Vector3 EvaluatePlayTrajectory(int stepIndex, float progress,
            MoonlightGestureSample sample)
        {
            int step = Mathf.Clamp(stepIndex, 0, 3);
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            Vector2 direction2 = sample.Direction;
            Vector3 direction = new(direction2.x, 0f, -direction2.y);
            if (direction.sqrMagnitude < 0.0001f) direction = Vector3.right;
            direction.Normalize();

            if (step == 0)
            {
                float extent = Mathf.Lerp(PlayMinimumThrowExtent, PlayMaximumThrowExtent,
                    Mathf.InverseLerp(0.24f, 1.20f, sample.DisplacementMagnitude));
                Vector3 point = direction * ((t - 0.5f) * extent);
                point.y = 0.24f + Mathf.Sin(t * Mathf.PI) * (0.78f + extent * 0.15f);
                return point;
            }

            if (step == 1)
            {
                Vector2 gesturePoint = InterpolateSamplePoint(sample, t);
                float minimumX = sample[0].x;
                float maximumX = sample[0].x;
                for (int i = 1; i < MoonlightGestureSample.ResampledPointCount; i++)
                {
                    minimumX = Mathf.Min(minimumX, sample[i].x);
                    maximumX = Mathf.Max(maximumX, sample[i].x);
                }
                float lateralCenter = (minimumX + maximumX) * 0.5f;
                float maximumLateral = 0.08f;
                for (int i = 0; i < MoonlightGestureSample.ResampledPointCount; i++)
                    maximumLateral = Mathf.Max(maximumLateral,
                        Mathf.Abs(sample[i].x - lateralCenter));
                float lateral = Mathf.Clamp((gesturePoint.x - lateralCenter) / maximumLateral,
                    -1f, 1f);
                return new Vector3(Mathf.Lerp(-1.05f, 1.05f, t),
                    0.26f + Mathf.Abs(Mathf.Sin(t * Mathf.PI * 4f)) * 0.30f,
                    -0.18f + lateral * 0.44f);
            }

            if (step == 2)
            {
                float extent = Mathf.Lerp(PlayMinimumJumpExtent, PlayMaximumJumpExtent,
                    Mathf.InverseLerp(0.24f, 1.05f, sample.DisplacementMagnitude));
                float height = Mathf.Lerp(PlayMinimumJumpHeight, PlayMaximumJumpHeight,
                    Mathf.InverseLerp(0.24f, 1.05f, sample.DisplacementMagnitude));
                Vector3 point = direction * ((t - 0.5f) * extent) +
                    new Vector3(0f, 0.24f, 0.34f);
                point.y += Mathf.Sin(t * Mathf.PI) * height;
                return point;
            }

            if (t >= PlayCatchContactProgress) return PlayCatchPoint;
            float catchT = Mathf.SmoothStep(0f, 1f, t / PlayCatchContactProgress);
            return Vector3.Lerp(new Vector3(0.48f, 1.12f, -0.18f), PlayCatchPoint, catchT);
        }

        public static bool ValidateGestureResponsivePlayContract(out string detail)
        {
            MoonlightGestureSample right = DirectionalSample(Vector2.right, 0.54f);
            MoonlightGestureSample left = DirectionalSample(Vector2.left, 0.54f);
            MoonlightGestureSample shortThrow = DirectionalSample(Vector2.right, 0.28f);
            MoonlightGestureSample longThrow = DirectionalSample(Vector2.right, 1.10f);
            MoonlightGestureSample zigZag = MoonlightGestureSample.Synthetic(
                MoonlightGestureKind.ZigZag, 0.95f);

            Vector3 rightEnd = EvaluatePlayTrajectory(0, 1f, right);
            Vector3 leftEnd = EvaluatePlayTrajectory(0, 1f, left);
            float mirroredSeparation = Vector3.Distance(rightEnd, leftEnd);
            bool mirroredGeometry = true;
            for (int i = 0; i <= 40; i++)
            {
                float progress = i / 40f;
                Vector3 rightPoint = EvaluatePlayTrajectory(0, progress, right);
                Vector3 leftPoint = EvaluatePlayTrajectory(0, progress, left);
                mirroredGeometry &= Mathf.Abs(rightPoint.x + leftPoint.x) <= 0.001f &&
                    Mathf.Abs(rightPoint.y - leftPoint.y) <= 0.001f &&
                    Mathf.Abs(rightPoint.z + leftPoint.z) <= 0.001f;
            }
            float shortExtent = PlanarDistance(EvaluatePlayTrajectory(0, 0f, shortThrow),
                EvaluatePlayTrajectory(0, 1f, shortThrow));
            float longExtent = PlanarDistance(EvaluatePlayTrajectory(0, 0f, longThrow),
                EvaluatePlayTrajectory(0, 1f, longThrow));

            int zigZagTurns = 0;
            float previousSign = 0f;
            for (int i = 0; i < MoonlightGestureSample.ResampledPointCount; i++)
            {
                float u = i / (float)(MoonlightGestureSample.ResampledPointCount - 1);
                float sign = Mathf.Sign(EvaluatePlayTrajectory(1, u, zigZag).z + 0.18f);
                if (previousSign != 0f && sign != 0f && sign != previousSign) zigZagTurns++;
                if (sign != 0f) previousSign = sign;
            }

            Vector3 jumpStart = EvaluatePlayTrajectory(2, 0f, longThrow);
            Vector3 jumpPeak = EvaluatePlayTrajectory(2, 0.5f, longThrow);
            Vector3 jumpEnd = EvaluatePlayTrajectory(2, 1f, longThrow);
            float jumpExtent = PlanarDistance(jumpStart, jumpEnd);
            float jumpHeight = jumpPeak.y - jumpStart.y;
            Vector3 catchContact = EvaluatePlayTrajectory(3, PlayCatchContactProgress, right);
            Vector3 catchFinal = EvaluatePlayTrajectory(3, 1f, left);

            bool finiteAndBounded = true;
            Vector2[] directions =
            {
                Vector2.right, Vector2.left, Vector2.up, Vector2.down,
                new Vector2(1f, 1f), new Vector2(-1f, 1f),
                new Vector2(1f, -1f), new Vector2(-1f, -1f)
            };
            for (int step = 0; step < 4; step++)
            {
                foreach (Vector2 testDirection in directions)
                {
                    MoonlightGestureSample directional = DirectionalSample(testDirection, 1.10f);
                    for (int i = 0; i <= 40; i++)
                        finiteAndBounded &= PlayPointIsFiniteAndBounded(EvaluatePlayTrajectory(
                            step, i / 40f, step == 1 ? zigZag : directional));
                }
            }
            bool jumpBounds = jumpExtent >= PlayMinimumJumpExtent - 0.001f &&
                jumpExtent <= PlayMaximumJumpExtent + 0.001f &&
                jumpHeight >= PlayMinimumJumpHeight - 0.001f &&
                jumpHeight <= PlayMaximumJumpHeight + 0.001f;
            bool catchBounds = PlayPointIsFiniteAndBounded(catchContact) &&
                Vector3.Distance(catchContact, catchFinal) <= 0.0001f &&
                Vector3.Distance(catchFinal, PlayCatchPoint) <= 0.0001f;
            detail = $"points={zigZag.PointCount} finiteClamped={finiteAndBounded} " +
                $"mirrored={mirroredGeometry}/{mirroredSeparation:0.000} " +
                $"throwExtent={shortExtent:0.000}-" +
                $"{longExtent:0.000} zigTurns={zigZagTurns} jump={jumpExtent:0.000}/" +
                $"{jumpHeight:0.000} catchHeld={catchBounds}";
            bool authoritativeBallContract = RequiredAuthoritativePlayBallCount == 1 &&
                !MoonlightActionFeedback.ShouldCreateOpaqueActionOrb(
                    MoonlightSpatialActionKind.Play);
            detail += $" authoritativeBalls={RequiredAuthoritativePlayBallCount} " +
                $"feedbackOrb={MoonlightActionFeedback.ShouldCreateOpaqueActionOrb(MoonlightSpatialActionKind.Play)}";
            return zigZag.PointCount == 7 && zigZag.HasSevenFiniteNormalizedPoints &&
                finiteAndBounded && mirroredGeometry && mirroredSeparation >= 0.80f &&
                longExtent > shortExtent + 0.40f && zigZagTurns >= 3 && jumpBounds && catchBounds &&
                authoritativeBallContract;
        }

        public static bool ValidatePlayPhaseLandmarkContract(out string detail)
        {
            bool namesPass = PlayPhaseLandmarkNames.Length ==
                    RequiredPlayPhaseLandmarkCount &&
                new HashSet<string>(PlayPhaseLandmarkNames).Count ==
                    RequiredPlayPhaseLandmarkCount &&
                string.Join("|", PlayPhaseLandmarkNames) ==
                    "ToyWand|ToyWandStar|ToyHoop|FinishFlagPole|FinishFlag|" +
                    "JumpArchLeftPost|JumpArchRightPost|JumpArchTop|" +
                    "CatchArchLeftPost|CatchArchRightPost|CatchArchTop";
            bool geometryPass = PlayPhaseLandmarkPositions.Length ==
                    RequiredPlayPhaseLandmarkCount &&
                PlayPhaseLandmarkScales.Length == RequiredPlayPhaseLandmarkCount;
            bool fallbackDefinitionPass = PlayFallbackBaseNames.Length ==
                    RequiredPlayFallbackBaseObjectCount &&
                PlayFallbackBasePositions.Length == RequiredPlayFallbackBaseObjectCount &&
                PlayFallbackBaseScales.Length == RequiredPlayFallbackBaseObjectCount &&
                string.Join("|", PlayFallbackBaseNames) ==
                    "PlayMatFallback|TargetOuterRingFallback|TargetInnerDotFallback";
            int configuredMask = 0;
            int[] expectedVisibleCounts = { 2, 1, 3, 5 };
            bool visibilityPass = PlayPhaseLandmarkVisibilityMasks.Length == PlayPhaseCount;
            for (int phase = 0; phase < PlayPhaseCount; phase++)
            {
                int mask = PlayPhaseExpectedVisibilityMask(phase);
                configuredMask |= mask;
                visibilityPass &= PlayPhaseExpectedVisibleLandmarkCount(phase) ==
                    expectedVisibleCounts[phase];
                for (int other = phase + 1; other < PlayPhaseCount; other++)
                    visibilityPass &= (mask &
                        PlayPhaseExpectedVisibilityMask(other)) == 0;
            }
            int requiredMask = (1 << RequiredPlayPhaseLandmarkCount) - 1;
            visibilityPass &= configuredMask == requiredMask &&
                PlayPhaseExpectedVisibilityMask(0) == 0x003 &&
                PlayPhaseExpectedVisibilityMask(1) == 0x004 &&
                PlayPhaseExpectedVisibilityMask(2) == 0x0E0 &&
                PlayPhaseExpectedVisibilityMask(3) == 0x718;
            bool jumpCenterlineClear = PlayJumpCenterlineClearsArchParts();
            float catchArchClearance = PlayCatchArchVisualClearanceForQA();
            bool catchArchClear = catchArchClearance >=
                PlayCatchArchMinimumVisualClearance;
            bool identityPass = RequiredAuthoritativePlayBallCount == 1 &&
                RequiredAuthoritativePlayTrailCount == 1 &&
                !MoonlightActionFeedback.ShouldCreateOpaqueActionOrb(
                    MoonlightSpatialActionKind.Play);
            bool budgetsPass = PlayRendererBudget == 48 && PlayMaterialBudget == 28 &&
                PlayLightBudget == 1 && PlayPhaseLandmarkMaterialBudget == 8 &&
                PlayAuthoredGeneratedMaterialBudget +
                    PlayAuthoredArenaMaterialBudget < PlayMaterialBudget &&
                PlayFallbackGeneratedMaterialBudget < PlayMaterialBudget &&
                identityPass;
            detail = $"phases={PlayPhaseCount} landmarks=" +
                $"{RequiredPlayPhaseLandmarkCount} names={namesPass} " +
                $"visibleMasks=0x{PlayPhaseExpectedVisibilityMask(0):X3}," +
                $"0x{PlayPhaseExpectedVisibilityMask(1):X3}," +
                $"0x{PlayPhaseExpectedVisibilityMask(2):X3}," +
                $"0x{PlayPhaseExpectedVisibilityMask(3):X3} " +
                $"visibleCounts={string.Join(",", expectedVisibleCounts)} " +
                $"geometry={geometryPass} jumpCenterlineClear={jumpCenterlineClear} " +
                $"catchClearance={catchArchClearance:0.000}/" +
                $">={PlayCatchArchMinimumVisualClearance:0.000} " +
                $"fallbackDefinition={fallbackDefinitionPass} " +
                $"budgets={PlayRendererBudget}r/" +
                $"{PlayMaterialBudget}m/{PlayLightBudget}l " +
                $"materialCeilings=authored:" +
                $"{PlayAuthoredGeneratedMaterialBudget}+" +
                $"{PlayAuthoredArenaMaterialBudget},fallback:" +
                $"{PlayFallbackGeneratedMaterialBudget} " +
                $"landmarkMaterialBudget=<={PlayPhaseLandmarkMaterialBudget} " +
                $"authoritative={RequiredAuthoritativePlayBallCount}ball/" +
                $"{RequiredAuthoritativePlayTrailCount}trail " +
                $"feedbackOrb={MoonlightActionFeedback.ShouldCreateOpaqueActionOrb(MoonlightSpatialActionKind.Play)}";
            return namesPass && geometryPass && fallbackDefinitionPass && visibilityPass &&
                jumpCenterlineClear && catchArchClear && identityPass && budgetsPass;
        }

        public static float PlayCatchArchVisualClearanceForQA()
        {
            Vector3[] halfExtents =
            {
                new(0.025f, 0.46f, 0.025f),
                new(0.025f, 0.46f, 0.025f),
                new(0.363f, 0.0275f, 0.0275f)
            };
            float minimum = float.PositiveInfinity;
            for (int archIndex = 0; archIndex < halfExtents.Length; archIndex++)
            {
                Vector3 delta = PlayCatchPoint -
                    PlayPhaseLandmarkPositions[archIndex + 8];
                float x = Mathf.Max(0f, Mathf.Abs(delta.x) - halfExtents[archIndex].x);
                float y = Mathf.Max(0f, Mathf.Abs(delta.y) - halfExtents[archIndex].y);
                float z = Mathf.Max(0f, Mathf.Abs(delta.z) - halfExtents[archIndex].z);
                float ballRadius = archIndex < 2
                    ? PlayBallMaximumHorizontalRadius
                    : PlayBallMaximumVerticalRadius;
                float clearance = new Vector3(x, y, z).magnitude - ballRadius;
                minimum = Mathf.Min(minimum, clearance);
            }
            return minimum;
        }

        static bool PlayJumpCenterlineClearsArchParts()
        {
            MoonlightGestureSample sample = DirectionalSample(Vector2.right, 1.10f);
            Vector3[] halfExtents =
            {
                new(0.09f, 0.51f, 0.09f),
                new(0.09f, 0.51f, 0.09f),
                new(0.51f, 0.028f, 0.028f)
            };
            for (int pointIndex = 0; pointIndex <= 80; pointIndex++)
            {
                Vector3 point = EvaluatePlayTrajectory(2, pointIndex / 80f, sample);
                for (int archIndex = 0; archIndex < halfExtents.Length; archIndex++)
                {
                    Vector3 center = PlayPhaseLandmarkPositions[archIndex + 5];
                    Vector3 delta = point - center;
                    if (Mathf.Abs(delta.x) <= halfExtents[archIndex].x &&
                        Mathf.Abs(delta.y) <= halfExtents[archIndex].y &&
                        Mathf.Abs(delta.z) <= halfExtents[archIndex].z)
                        return false;
                }
            }
            return true;
        }

        static MoonlightGestureSample DirectionalSample(Vector2 direction, float displacement)
        {
            direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            var points = new Vector2[MoonlightGestureSample.ResampledPointCount];
            for (int i = 0; i < points.Length; i++)
                points[i] = direction * Mathf.Lerp(-displacement * 0.5f,
                    displacement * 0.5f, i / (float)(points.Length - 1));
            return MoonlightGestureSample.Create(0.95f, 0.35f, points);
        }

        static Vector2 InterpolateSamplePoint(MoonlightGestureSample sample, float t)
        {
            float scaled = Mathf.Clamp01(t) * (MoonlightGestureSample.ResampledPointCount - 1);
            int from = Mathf.Min(Mathf.FloorToInt(scaled),
                MoonlightGestureSample.ResampledPointCount - 1);
            int to = Mathf.Min(from + 1, MoonlightGestureSample.ResampledPointCount - 1);
            return Vector2.Lerp(sample[from], sample[to], scaled - from);
        }

        static float PlanarDistance(Vector3 first, Vector3 second)
        {
            Vector3 delta = second - first;
            delta.y = 0f;
            return delta.magnitude;
        }

        static bool PlayPointIsFiniteAndBounded(Vector3 point) =>
            IsFinite(point.x) && IsFinite(point.y) && IsFinite(point.z) &&
            Mathf.Abs(point.x) <= 1.25f && point.y >= 0.18f && point.y <= 1.65f &&
            Mathf.Abs(point.z) <= 1.25f;

        static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        static float CappedPlayContinuationDelta(float unscaledDeltaTime) =>
            Mathf.Min(Mathf.Max(0f, unscaledDeltaTime),
                PlayContinuationMaximumDeltaSeconds);

        public static bool ValidatePlayContinuationClockContract(out string detail)
        {
            const float normalFrameDelta = 1f / 60f;
            float normalDelta = CappedPlayContinuationDelta(normalFrameDelta);
            float hitchDelta = CappedPlayContinuationDelta(1f);
            float negativeDelta = CappedPlayContinuationDelta(-1f);
            bool sourcePass = string.Equals(PlayContinuationClockSourceForQA,
                "Time.unscaledDeltaTime", System.StringComparison.Ordinal);
            detail = $"source={PlayContinuationClockSourceForQA} " +
                $"normal={normalDelta:0.000000} hitch={hitchDelta:0.000000} " +
                $"cap={PlayContinuationMaximumDeltaSeconds:0.000000} " +
                $"negative={negativeDelta:0.000000} staticOnly=True " +
                $"runtimePauseExecuted=False marker={PlayContinuationClockQAMarker}";
            return sourcePass && Mathf.Approximately(normalDelta, normalFrameDelta) &&
                Mathf.Approximately(hitchDelta, PlayContinuationMaximumDeltaSeconds) &&
                Mathf.Approximately(negativeDelta, 0f);
        }

        void BuildPlayStage()
        {
            bool authoredArena = BuildAuthoredPlayArena();
            if (!authoredArena)
            {
                _playFallbackBase = new[]
                {
                    Primitive(PrimitiveType.Cylinder, PlayFallbackBaseNames[0],
                        PlayFallbackBasePositions[0], PlayFallbackBaseScales[0],
                        new Color(0.22f, 0.32f, 0.43f), 0.03f),
                    Primitive(PrimitiveType.Cylinder, PlayFallbackBaseNames[1],
                        PlayFallbackBasePositions[1], PlayFallbackBaseScales[1],
                        new Color(0.98f, 0.82f, 0.38f), 0.14f),
                    Primitive(PrimitiveType.Cylinder, PlayFallbackBaseNames[2],
                        PlayFallbackBasePositions[2], PlayFallbackBaseScales[2],
                        new Color(0.39f, 0.78f, 0.96f), 0.18f)
                };
            }

            _ball = Primitive(PrimitiveType.Sphere, "StarBall", new Vector3(0f, 0.30f, 0f),
                Vector3.one * 0.27f, new Color(0.42f, 0.86f, 1f), 0.10f);
            _playBallRenderer = _ball.GetComponent<Renderer>();
            _starDetails = new Transform[6];
            for (int i = 0; i < _starDetails.Length; i++)
            {
                float angle = i * Mathf.PI * 2f / _starDetails.Length;
                _starDetails[i] = Primitive(PrimitiveType.Sphere, $"StarBallDot-{i + 1}",
                    new Vector3(Mathf.Cos(angle) * 0.72f, 0.18f, Mathf.Sin(angle) * 0.72f),
                    Vector3.one * 0.20f, new Color(1f, 0.89f, 0.36f), 0.18f);
                _starDetails[i].SetParent(_ball, false);
            }
            _ballTrail = _ball.gameObject.AddComponent<TrailRenderer>();
            _ballTrail.time = 0.5f;
            _ballTrail.minVertexDistance = 0.025f;
            _ballTrail.startWidth = 0.10f;
            _ballTrail.endWidth = 0f;
            _ballTrail.startColor = new Color(0.98f, 0.82f, 0.38f);
            _ballTrail.endColor = new Color(0.42f, 0.86f, 1f, 0f);
            _ballTrail.sharedMaterial = NewMaterial(Color.white, 0.25f, true,
                ActivitySurfaceProfile.Magic);
            _ballTrail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _ballTrail.receiveShadows = false;
            _renderers.Add(_ballTrail);

            var colors = new[]
            {
                new Color(0.96f, 0.56f, 0.68f),
                new Color(0.54f, 0.80f, 0.70f),
                new Color(0.98f, 0.78f, 0.36f),
            };
            _blocks = new Transform[3];
            for (int i = 0; i < _blocks.Length; i++)
            {
                _blocks[i] = Primitive(PrimitiveType.Cube, $"MagicBlock-{i + 1}",
                    new Vector3(-0.68f, 0.16f + i * 0.31f, 0.26f), Vector3.one * 0.30f,
                    colors[i], 0.16f);
                _blocks[i].localScale = Vector3.zero;
            }

            _pathMarkers = new Transform[7];
            for (int i = 0; i < _pathMarkers.Length; i++)
            {
                _pathMarkers[i] = Primitive(PrimitiveType.Cylinder, $"ChasePathMarker-{i + 1}",
                    Vector3.zero, new Vector3(0.12f, 0.010f, 0.12f),
                    i % 2 == 0 ? new Color(0.98f, 0.82f, 0.38f) : new Color(0.42f, 0.86f, 1f), 0.14f);
                SetPlayRendererVisible(_pathMarkers[i], false);
            }

            _playProps = new[]
            {
                Primitive(PrimitiveType.Capsule, PlayPhaseLandmarkNames[0],
                    PlayPhaseLandmarkPositions[0], PlayPhaseLandmarkScales[0],
                    new Color(0.82f, 0.58f, 0.98f), 0.12f),
                Primitive(PrimitiveType.Sphere, PlayPhaseLandmarkNames[1],
                    PlayPhaseLandmarkPositions[1], PlayPhaseLandmarkScales[1],
                    new Color(1f, 0.88f, 0.38f), 0.22f),
                Primitive(PrimitiveType.Cylinder, PlayPhaseLandmarkNames[2],
                    PlayPhaseLandmarkPositions[2], PlayPhaseLandmarkScales[2],
                    new Color(0.95f, 0.55f, 0.68f), 0.12f),
                Primitive(PrimitiveType.Cube, PlayPhaseLandmarkNames[3],
                    PlayPhaseLandmarkPositions[3], PlayPhaseLandmarkScales[3],
                    new Color(0.74f, 0.79f, 0.84f), 0.06f),
                Primitive(PrimitiveType.Cube, PlayPhaseLandmarkNames[4],
                    PlayPhaseLandmarkPositions[4], PlayPhaseLandmarkScales[4],
                    new Color(0.98f, 0.82f, 0.38f), 0.16f),
            };
            _playProps[0].localRotation = Quaternion.Euler(74f, 0f, -38f);
            _playProps[2].localRotation = Quaternion.Euler(0f, 0f, 12f);

            _celebrationStars = new Transform[6];
            for (int i = 0; i < _celebrationStars.Length; i++)
            {
                _celebrationStars[i] = Primitive(PrimitiveType.Sphere, $"CatchSpark-{i + 1}",
                    Vector3.zero, Vector3.one * 0.07f,
                    i % 2 == 0 ? new Color(1f, 0.88f, 0.38f) : new Color(0.98f, 0.56f, 0.68f),
                    0.30f);
                SetPlayRendererVisible(_celebrationStars[i], false);
            }

            _playArches = new[]
            {
                Primitive(PrimitiveType.Capsule, PlayPhaseLandmarkNames[5],
                    PlayPhaseLandmarkPositions[5], PlayPhaseLandmarkScales[5],
                    new Color(0.54f, 0.80f, 0.70f), 0.08f),
                Primitive(PrimitiveType.Capsule, PlayPhaseLandmarkNames[6],
                    PlayPhaseLandmarkPositions[6], PlayPhaseLandmarkScales[6],
                    new Color(0.54f, 0.80f, 0.70f), 0.08f),
                Primitive(PrimitiveType.Cube, PlayPhaseLandmarkNames[7],
                    PlayPhaseLandmarkPositions[7], PlayPhaseLandmarkScales[7],
                    new Color(0.98f, 0.82f, 0.38f), 0.14f),
                Primitive(PrimitiveType.Capsule, PlayPhaseLandmarkNames[8],
                    PlayPhaseLandmarkPositions[8], PlayPhaseLandmarkScales[8],
                    new Color(0.95f, 0.55f, 0.68f), 0.12f),
                Primitive(PrimitiveType.Capsule, PlayPhaseLandmarkNames[9],
                    PlayPhaseLandmarkPositions[9], PlayPhaseLandmarkScales[9],
                    new Color(0.95f, 0.55f, 0.68f), 0.12f),
                Primitive(PrimitiveType.Cube, PlayPhaseLandmarkNames[10],
                    PlayPhaseLandmarkPositions[10], PlayPhaseLandmarkScales[10],
                    new Color(0.42f, 0.86f, 1f), 0.12f),
            };
            _playArches[0].localRotation = Quaternion.Euler(0f, 0f, -4f);
            _playArches[1].localRotation = Quaternion.Euler(0f, 0f, 4f);
            SetPlayRenderersVisible(_playArches, false);

            _podiumProps = new[]
            {
                Primitive(PrimitiveType.Cylinder, "CelebrationPodiumBase", new Vector3(0.94f, 0.11f, -0.46f),
                    new Vector3(0.44f, 0.10f, 0.44f), new Color(0.58f, 0.48f, 0.70f), 0.08f),
                Primitive(PrimitiveType.Cylinder, "CelebrationPodiumTop", new Vector3(0.94f, 0.23f, -0.46f),
                    new Vector3(0.30f, 0.065f, 0.30f), new Color(0.98f, 0.82f, 0.38f), 0.14f),
                Primitive(PrimitiveType.Cube, "CelebrationMedal", new Vector3(0.94f, 0.40f, -0.46f),
                    new Vector3(0.18f, 0.15f, 0.035f), new Color(1f, 0.89f, 0.36f), 0.18f),
            };
            SetPlayRenderersVisible(_podiumProps, false);
            AddActivityLight(new Color(0.42f, 0.86f, 1f));
        }

        bool BuildAuthoredPlayArena()
        {
            if (_persistentStation != null && _persistentStation.Kind == MoonlightSpatialActionKind.Play &&
                _persistentStation.VisualRoot != null)
            {
                _authoredPlayArena = _persistentStation.VisualRoot;
                var stationRenderers = _authoredPlayArena.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < stationRenderers.Length; i++) _renderers.Add(stationRenderers[i]);
                AuthoredPlayArenaRendererCount = _persistentStation.RendererCount;
                AuthoredPlayArenaMaterialCount = _persistentStation.UniqueMaterialCount;
                AuthoredPlayArenaColliderCount = _persistentStation.ColliderCount;
                AuthoredPlayArenaLightCount = _persistentStation.LightCount;
                AuthoredPlayArenaBoundsSize = _persistentStation.BoundsSize;
                Debug.Log($"[MoonlightActivityStage] authored-play-arena persistent=true " +
                    $"renderers={AuthoredPlayArenaRendererCount} materials={AuthoredPlayArenaMaterialCount} " +
                    $"colliders={AuthoredPlayArenaColliderCount} lights={AuthoredPlayArenaLightCount} " +
                    $"bounds={AuthoredPlayArenaBoundsSize:F2} marker=MOONLIGHT_AUTHORED_PLAY_ARENA_READY");
                return true;
            }

            var prefab = Resources.Load<GameObject>("Models/Hero/MoonPlayArena");
            if (prefab == null)
            {
                Debug.LogError("[MoonlightActivityStage] authored Play arena missing; using fallback");
                return false;
            }

            var instance = Instantiate(prefab, _root.transform, false);
            instance.name = "MoonPlayArenaAuthored";
            instance.transform.localPosition = new Vector3(-0.18f, 0.01f, 0.08f);
            instance.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            instance.transform.localScale = Vector3.one * 0.84f;
            _authoredPlayArena = instance.transform;

            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            var materialIds = new HashSet<int>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderers[i].receiveShadows = true;
                _renderers.Add(renderers[i]);
                var shared = renderers[i].sharedMaterials;
                for (int materialIndex = 0; materialIndex < shared.Length; materialIndex++)
                    if (shared[materialIndex] != null) materialIds.Add(shared[materialIndex].GetInstanceID());
            }

            var colliders = instance.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = false;
            var lights = instance.GetComponentsInChildren<Light>(true);
            for (int i = 0; i < lights.Length; i++) lights[i].enabled = false;

            AuthoredPlayArenaRendererCount = renderers.Length;
            AuthoredPlayArenaMaterialCount = materialIds.Count;
            AuthoredPlayArenaColliderCount = colliders.Length;
            AuthoredPlayArenaLightCount = lights.Length;
            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                AuthoredPlayArenaBoundsSize = bounds.size;
            }
            Debug.Log($"[MoonlightActivityStage] authored-play-arena renderers={renderers.Length} " +
                $"materials={materialIds.Count} colliders={colliders.Length} lights={lights.Length} " +
                $"bounds={AuthoredPlayArenaBoundsSize:F2} " +
                "marker=MOONLIGHT_AUTHORED_PLAY_ARENA_READY");
            return true;
        }

        void UpdatePlay(float t)
        {
            if (_ball == null || _starDetails == null || _blocks == null) return;

            int step = Mathf.Clamp(CurrentStep, 0, 3);
            _playProgress = Mathf.Clamp01(t);
            SetPlayRenderersVisible(_podiumProps, step == 3);
            if (_playArches != null)
            {
                for (int i = 0; i < _playArches.Length; i++)
                    if (_playArches[i] != null)
                        SetPlayRendererVisible(_playArches[i],
                            (step == 2 && i < 3) || (step == 3 && i >= 3));
            }
            if (_playProps != null && _playProps.Length >= 5)
            {
                SetPlayRendererVisible(_playProps[0], step == 0);
                SetPlayRendererVisible(_playProps[1], step == 0);
                SetPlayRendererVisible(_playProps[2], step == 1);
                SetPlayRendererVisible(_playProps[3], step == 3);
                SetPlayRendererVisible(_playProps[4], step == 3);
            }
            if (_ballTrail != null)
            {
                bool trailVisible = step != 3 || t < 0.58f;
                _ballTrail.emitting = trailVisible;
                _ballTrail.forceRenderingOff = !trailVisible;
            }
            if (_ball != null)
            {
                float bounce = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 4f));
                Vector3 target = EvaluatePlayTrajectory(step, t, _gestureSample);
                if (_playContinuationActive)
                {
                    bool beginFrame = Time.frameCount == _playContinuationBeginFrame;
                    if (beginFrame || _playContinuationFirstRenderedFramePending)
                    {
                        _ball.localPosition = _playContinuationStart;
                        if (!beginFrame)
                        {
                            _playContinuationFirstRenderedFramePending = false;
                            _playContinuationLastAdvancedFrame = Time.frameCount;
                        }
                    }
                    else
                    {
                        if (Time.frameCount != _playContinuationLastAdvancedFrame)
                        {
                            LastPlayContinuationClockDeltaForQA =
                                CappedPlayContinuationDelta(Time.unscaledDeltaTime);
                            _playContinuationElapsed +=
                                LastPlayContinuationClockDeltaForQA;
                            _playContinuationLastAdvancedFrame = Time.frameCount;
                        }
                        float blend = Mathf.SmoothStep(0f, 1f,
                            Mathf.Clamp01(_playContinuationElapsed /
                                PlayContinuationBlendSeconds));
                        _ball.localPosition = Vector3.Lerp(
                            _playContinuationStart, target, blend);
                        if (blend >= 1f) _playContinuationActive = false;
                    }
                }
                else
                {
                    _ball.localPosition = target;
                }

                float squash = 1f - (1f - bounce) * 0.18f;
                _ball.localScale = new Vector3(0.27f / squash, 0.27f * squash, 0.27f / squash);
                _ball.localRotation = Quaternion.Euler(t * 540f, t * 880f, t * 260f);
            }

            for (int i = 0; i < _starDetails.Length; i++)
            {
                float twinkle = 0.85f + Mathf.Sin(t * Mathf.PI * 10f + i) * 0.18f;
                _starDetails[i].localScale = Vector3.one * 0.20f * twinkle;
            }

            for (int i = 0; i < _blocks.Length; i++)
            {
                SetPlayRendererVisible(_blocks[i], step != 0);
                float reveal = step == 0 ? 0f : Mathf.Clamp01((t - 0.12f - i * 0.08f) * 7f);
                float pop = reveal <= 0f ? 0f : 1f + Mathf.Sin(reveal * Mathf.PI) * 0.24f;
                float jump = step == 2 ? Mathf.Clamp01(t * 3.5f) : step == 3 ? 1f : 0f;
                Vector3 playStack = new Vector3(-0.62f + i * 0.62f, 0.16f,
                    (i % 2 == 0) ? 0.30f : -0.42f);
                Vector3 jumpStack = new Vector3(-0.34f + i * 0.34f, 0.17f + i * 0.17f, 0.38f);
                Vector3 podiumStack = new Vector3(0.94f + (i - 1) * 0.18f, 0.31f + i * 0.11f, -0.46f);
                _blocks[i].localPosition = Vector3.Lerp(Vector3.Lerp(playStack, jumpStack, jump), podiumStack, step == 3 ? Mathf.Clamp01(t * 3f) : 0f);
                _blocks[i].localScale = Vector3.one * 0.30f * reveal * pop * Mathf.Lerp(1f, 0.72f, step == 3 ? 1f : jump * 0.45f);
                _blocks[i].localRotation = Quaternion.Euler(0f, t * (75f + i * 22f), Mathf.Lerp(0f, (i - 1) * 14f, Mathf.Max(jump, step == 3 ? 1f : 0f)));
            }

            if (_pathMarkers != null)
            {
                for (int i = 0; i < _pathMarkers.Length; i++)
                {
                    SetPlayRendererVisible(_pathMarkers[i], step <= 2);
                    float u = i / (float)(_pathMarkers.Length - 1);
                    _pathMarkers[i].localPosition = EvaluatePlayTrajectory(step, u, _gestureSample);
                    _pathMarkers[i].localPosition += Vector3.up * (step == 1 ? -0.18f : -0.04f);
                    float pulse = 1f + Mathf.Sin(t * Mathf.PI * 6f + i) * 0.16f;
                    float markerSize = step == 1 ? 0.14f : 0.11f;
                    _pathMarkers[i].localScale = new Vector3(markerSize, step == 1 ? 0.010f : 0.035f,
                        markerSize) * pulse;
                }
            }

            if (_celebrationStars != null)
            {
                for (int i = 0; i < _celebrationStars.Length; i++)
                {
                    SetPlayRendererVisible(_celebrationStars[i],
                        step == 3 && t > 0.18f);
                    float phase = Mathf.Repeat(t * 1.8f + i * 0.13f, 1f);
                    float angle = i * Mathf.PI * 2f / _celebrationStars.Length + t * Mathf.PI * 2f;
                    _celebrationStars[i].localPosition = new Vector3(0.94f + Mathf.Cos(angle) * (0.18f + phase * 0.42f),
                        0.58f + phase * 0.62f,
                        -0.46f + Mathf.Sin(angle) * (0.14f + phase * 0.30f));
                    _celebrationStars[i].localScale = Vector3.one
                        * (0.035f + Mathf.Sin(phase * Mathf.PI) * 0.075f);
                }
            }

            if (_playArches != null && _playArches.Length >= 6)
            {
                float archPulse = step == 2 ? 1f + Mathf.Sin(t * Mathf.PI * 5f) * 0.08f : 1f;
                _playArches[2].localScale = new Vector3(1.02f, 0.055f, 0.055f) * archPulse;
                _playArches[5].localScale = new Vector3(0.66f, 0.050f, 0.050f)
                    * (step == 3 ? 1f + Mathf.Sin(t * Mathf.PI * 6f) * 0.10f : 1f);
            }

            if (_podiumProps != null && _podiumProps.Length >= 3 && step == 3)
            {
                float podiumPop = 1f + Mathf.Sin(Mathf.Clamp01(t * 4f) * Mathf.PI) * 0.16f;
                _podiumProps[1].localScale = new Vector3(0.30f, 0.065f, 0.30f) * podiumPop;
                _podiumProps[2].localRotation = Quaternion.Euler(0f, t * 180f, 0f);
            }

            if (_playProps != null && _playProps.Length >= 5)
            {
                float finishPulse = step == 3 ? Mathf.Clamp01(t * 4f) : 0f;
                _playProps[1].localScale = Vector3.one * (0.11f + Mathf.Sin(t * Mathf.PI * 8f) * 0.018f);
                _playProps[4].localScale = new Vector3(0.24f, 0.12f, 0.025f)
                    * (1f + Mathf.Sin(finishPulse * Mathf.PI) * 0.22f);
            }
        }

        void BuildGardenStage()
        {
            bool hasAuthoredSet = BindPersistentActivitySet(MoonlightSpatialActionKind.Garden,
                "MOONLIGHT_AUTHORED_GARDEN_ATELIER_READY", out _authoredGardenAtelier,
                out int rendererCount, out int materialCount, out int colliderCount,
                out int lightCount, out Vector3 boundsSize);
            if (!hasAuthoredSet)
                hasAuthoredSet = BuildAuthoredActivitySet("Models/Hero/MoonGardenAtelier", "MoonGardenAtelierAuthored",
                    new Vector3(-0.10f, 0.01f, 0.06f), 0.86f, "MOONLIGHT_AUTHORED_GARDEN_ATELIER_READY",
                    out _authoredGardenAtelier, out rendererCount, out materialCount,
                    out colliderCount, out lightCount, out boundsSize);
            if (!hasAuthoredSet)
            {
                Primitive(PrimitiveType.Cube, "GardenBench", new Vector3(0f, 0.045f, 0.02f),
                    new Vector3(1.48f, 0.09f, 0.72f), new Color(0.42f, 0.28f, 0.18f), 0.02f);
                Primitive(PrimitiveType.Cube, "GardenMat", new Vector3(0f, 0.105f, 0.02f),
                    new Vector3(1.36f, 0.025f, 0.62f), new Color(0.46f, 0.62f, 0.36f), 0.05f);
                Primitive(PrimitiveType.Cube, "PlanterBox", new Vector3(-0.10f, 0.22f, 0.00f),
                    new Vector3(0.92f, 0.25f, 0.42f), new Color(0.58f, 0.34f, 0.20f), 0.03f);
                Primitive(PrimitiveType.Cube, "SoilPatch", new Vector3(-0.10f, 0.39f, 0f),
                    new Vector3(0.82f, 0.035f, 0.32f), new Color(0.22f, 0.13f, 0.09f), 0.01f);
            }
            AuthoredGardenAtelierRendererCount = rendererCount;
            AuthoredGardenAtelierMaterialCount = materialCount;
            AuthoredGardenAtelierColliderCount = colliderCount;
            AuthoredGardenAtelierLightCount = lightCount;
            AuthoredGardenAtelierBoundsSize = boundsSize;

            _gardenProps = new[]
            {
                Primitive(PrimitiveType.Capsule, "WateringCanBody", new Vector3(0.58f, 0.31f, -0.18f),
                    new Vector3(0.20f, 0.25f, 0.20f), new Color(0.44f, 0.68f, 0.82f), 0.06f),
                Primitive(PrimitiveType.Capsule, "WateringCanSpout", new Vector3(0.35f, 0.37f, -0.15f),
                    new Vector3(0.045f, 0.33f, 0.045f), new Color(0.44f, 0.68f, 0.82f), 0.06f),
                Primitive(PrimitiveType.Cylinder, "WateringCanRose", new Vector3(0.20f, 0.43f, -0.13f),
                    new Vector3(0.08f, 0.018f, 0.08f), new Color(0.62f, 0.80f, 0.92f), 0.08f),
                Primitive(PrimitiveType.Capsule, "WateringCanHandle", new Vector3(0.72f, 0.36f, -0.18f),
                    new Vector3(0.045f, 0.30f, 0.045f), new Color(0.62f, 0.80f, 0.92f), 0.06f),
            };
            _gardenProps[1].localRotation = Quaternion.Euler(0f, 0f, 64f);
            _gardenProps[2].localRotation = Quaternion.Euler(90f, 0f, 64f);
            _gardenProps[3].localRotation = Quaternion.Euler(0f, 0f, -28f);

            _seeds = new Transform[5];
            _sprouts = new Transform[5];
            _flowers = new Transform[5];
            _gardenSparkles = new Transform[7];
            if (_magicFlowerPrefab == null)
                _magicFlowerPrefab = Resources.Load<GameObject>(MagicFlowerResourcePath);
            if (_magicFlowerPrefab == null)
                Debug.LogError($"[MoonlightActivityStage] garden magic flower missing " +
                    $"path={MagicFlowerResourcePath} marker=MOONLIGHT_MAGIC_FLOWER_STAGE_MISSING");
            for (int i = 0; i < _seeds.Length; i++)
            {
                float x = -0.42f + i * 0.16f;
                float z = (i % 2 == 0) ? -0.07f : 0.08f;
                _seeds[i] = Primitive(PrimitiveType.Sphere, $"GardenSeed-{i + 1}",
                    new Vector3(x, 0.43f, z), Vector3.one * 0.045f,
                    new Color(0.95f, 0.78f, 0.42f), 0.10f);
                _sprouts[i] = Primitive(PrimitiveType.Capsule, $"GardenSprout-{i + 1}",
                    new Vector3(x, 0.47f, z), new Vector3(0.035f, 0.20f, 0.035f),
                    new Color(0.34f, 0.72f, 0.38f), 0.06f);
                _sprouts[i].localScale = Vector3.zero;
                _flowers[i] = CreateMagicFlower($"GardenMagicFlower-{i + 1}",
                    new Vector3(x, 0.42f, z), Vector3.one * 0.48f);
                if (_flowers[i] != null) _flowers[i].localScale = Vector3.zero;
            }

            Debug.Log($"[MoonlightActivityStage] garden-magic-flower " +
                $"instances={GardenMagicFlowerInstanceCount}/{GardenMagicFlowerRequiredInstances} " +
                $"renderers={GardenMagicFlowerRendererCount}/{GardenMagicFlowerRendererBudget} " +
                $"materials={GardenMagicFlowerUniqueMaterialCount} shared={GardenMagicFlowerUsesSharedMaterials} " +
                $"colliders={GardenMagicFlowerEnabledColliderCount}/{GardenMagicFlowerColliderCount} " +
                $"lights={GardenMagicFlowerEnabledLightCount}/{GardenMagicFlowerLightCount} " +
                $"marker={GardenMagicFlowerQAMarker}");

            for (int i = 0; i < _gardenSparkles.Length; i++)
            {
                _gardenSparkles[i] = Primitive(PrimitiveType.Sphere, $"GardenSparkle-{i + 1}",
                    Vector3.zero, Vector3.one * 0.035f, new Color(1f, 0.92f, 0.52f, 0.72f), 0.30f, true);
                _gardenSparkles[i].gameObject.SetActive(false);
            }

            AddActivityLight(new Color(0.56f, 0.86f, 0.48f));
        }

        Transform CreateMagicFlower(string instanceName, Vector3 localPosition, Vector3 localScale)
        {
            if (_magicFlowerPrefab == null) return null;

            int instanceIndex = GardenMagicFlowerInstanceCount;
            var instance = Instantiate(_magicFlowerPrefab, _root.transform, false);
            instance.name = instanceName;
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            instance.transform.localScale = localScale;

            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows = true;
                _renderers.Add(renderer);
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null) continue;
                    int materialId = material.GetInstanceID();
                    if (instanceIndex > 0 && !_gardenMagicFlowerMaterialIds.Contains(materialId))
                        GardenMagicFlowerUsesSharedMaterials = false;
                    _gardenMagicFlowerMaterialIds.Add(materialId);
                }
            }

            var colliders = instance.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = false;
            var lights = instance.GetComponentsInChildren<Light>(true);
            for (int i = 0; i < lights.Length; i++) lights[i].enabled = false;

            GardenMagicFlowerInstanceCount++;
            GardenMagicFlowerRendererCount += renderers.Length;
            GardenMagicFlowerColliderCount += colliders.Length;
            GardenMagicFlowerLightCount += lights.Length;
            GardenMagicFlowerEnabledColliderCount += CountEnabled(colliders);
            GardenMagicFlowerEnabledLightCount += CountEnabled(lights);
            return instance.transform;
        }

        bool BuildAuthoredActivitySet(string resourcePath, string instanceName, Vector3 localPosition,
            float localScale, string marker, out Transform authoredRoot, out int rendererCount,
            out int materialCount, out int colliderCount, out int lightCount, out Vector3 boundsSize)
        {
            authoredRoot = null;
            rendererCount = 0;
            materialCount = 0;
            colliderCount = 0;
            lightCount = 0;
            boundsSize = Vector3.zero;
            var prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                Debug.LogError($"[MoonlightActivityStage] authored activity set missing path={resourcePath}; using fallback");
                return false;
            }

            var instance = Instantiate(prefab, _root.transform, false);
            instance.name = instanceName;
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            instance.transform.localScale = Vector3.one * localScale;
            authoredRoot = instance.transform;

            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            var materialIds = new HashSet<int>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderers[i].receiveShadows = true;
                _renderers.Add(renderers[i]);
                var shared = renderers[i].sharedMaterials;
                for (int materialIndex = 0; materialIndex < shared.Length; materialIndex++)
                    if (shared[materialIndex] != null) materialIds.Add(shared[materialIndex].GetInstanceID());
            }

            var colliders = instance.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = false;
            var lights = instance.GetComponentsInChildren<Light>(true);
            for (int i = 0; i < lights.Length; i++) lights[i].enabled = false;

            rendererCount = renderers.Length;
            materialCount = materialIds.Count;
            colliderCount = colliders.Length;
            lightCount = lights.Length;
            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                boundsSize = bounds.size;
            }
            Debug.Log($"[MoonlightActivityStage] authored-activity-set name={instanceName} " +
                $"renderers={rendererCount} materials={materialCount} colliders={colliderCount} " +
                $"lights={lightCount} bounds={boundsSize:F2} marker={marker}");
            return true;
        }

        bool BindPersistentActivitySet(MoonlightSpatialActionKind kind, string marker,
            out Transform authoredRoot, out int rendererCount, out int materialCount,
            out int colliderCount, out int lightCount, out Vector3 boundsSize)
        {
            authoredRoot = null;
            rendererCount = 0;
            materialCount = 0;
            colliderCount = 0;
            lightCount = 0;
            boundsSize = Vector3.zero;
            if (_persistentStation == null || _persistentStation.Kind != kind ||
                _persistentStation.VisualRoot == null)
                return false;

            authoredRoot = _persistentStation.VisualRoot;
            var stationRenderers = authoredRoot.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < stationRenderers.Length; i++) _renderers.Add(stationRenderers[i]);
            rendererCount = _persistentStation.RendererCount;
            materialCount = _persistentStation.UniqueMaterialCount;
            colliderCount = _persistentStation.EnabledColliderCount;
            lightCount = _persistentStation.EnabledLightCount;
            boundsSize = _persistentStation.BoundsSize;
            string visualSource = _persistentStation.UsesProceduralFallback
                ? "procedural-fallback"
                : "authored";
            string sourceMarker = _persistentStation.UsesProceduralFallback
                ? _persistentStation.VisualSourceQAMarker
                : marker;
            Debug.Log($"[MoonlightActivityStage] activity-set kind={kind} persistent=true " +
                $"source={visualSource} " +
                $"renderers={rendererCount} materials={materialCount} enabledColliders={colliderCount} " +
                $"enabledLights={lightCount} bounds={boundsSize:F2} marker={sourceMarker}");
            return true;
        }

        void UpdateGarden(float t)
        {
            if (_seeds == null || _sprouts == null || _flowers == null || _gardenSparkles == null) return;

            int step = Mathf.Clamp(CurrentStep, 0, 3);
            _gardenProgress = t;
            int selectedPlantSlot = GardenPlantSlotIndex(_gestureSample);

            for (int i = 0; i < _seeds.Length; i++)
            {
                int seedSlot = GardenPlantSlotForSeed(i, selectedPlantSlot);
                Vector3 seedTarget = GardenPlantSlotPosition(seedSlot);
                Vector3 flowerTarget = step == 2
                    ? EvaluateGardenTendTarget(i, _gestureSample) + Vector3.down * 0.18f
                    : GardenPlantSlotPosition(i);
                bool showSeed = step <= 1;
                bool showSprout = step >= 1 && step < 2;
                bool showFlower = step >= 2;
                _seeds[i].gameObject.SetActive(showSeed);
                _sprouts[i].gameObject.SetActive(showSprout);
                if (_flowers[i] != null) _flowers[i].gameObject.SetActive(showFlower);

                if (showSeed)
                {
                    float seedDrop = step == 0 ? Mathf.Clamp01(t * 3.2f - i * 0.22f) : 1f;
                    _seeds[i].localPosition = Vector3.Lerp(
                        seedTarget + new Vector3(-0.20f, 0.49f, -0.12f),
                        seedTarget, seedDrop);
                    float plantedScale = step == 0
                        ? Mathf.Lerp(1f, 0.45f, Mathf.Clamp01((t - 0.42f) * 4f))
                        : 0.35f;
                    _seeds[i].localScale = Vector3.one * 0.045f * plantedScale;
                }

                if (showSprout)
                {
                    float grow = step == 1
                        ? Mathf.Clamp01(t * 2.8f - i * 0.08f)
                        : 1f;
                    float sway = Mathf.Sin(t * Mathf.PI * (step == 2 ? 6f : 2f) + i) * (step == 2 ? 14f : 5f);
                    _sprouts[i].localScale = new Vector3(0.035f, 0.20f + i * 0.015f, 0.035f)
                        * (grow + Mathf.Sin(grow * Mathf.PI) * 0.12f);
                    _sprouts[i].localRotation = Quaternion.Euler(0f, sway, step == 2 ? sway * 0.35f : 0f);
                }

                if (showFlower && _flowers[i] != null)
                {
                    _flowers[i].localPosition = flowerTarget;
                    float scale;
                    if (step == 2)
                    {
                        scale = EvaluateGardenTendScale(t, i);
                    }
                    else
                    {
                        scale = EvaluateGardenBloomScale(t, i, _gestureSample);
                    }
                    _flowers[i].localScale = Vector3.one * scale;
                    _flowers[i].localRotation = Quaternion.Euler(-90f,
                        i * 28f + Mathf.Sin(t * Mathf.PI * 2f + i) * 5f,
                        step == 2 ? -8f * Mathf.Sin(
                            Mathf.Clamp01(t * GardenMagicFlowerRequiredInstances - i) *
                            Mathf.PI) : 0f);
                }
            }

            if (_gardenProps != null)
            {
                Vector3 rosePosition = step switch
                {
                    1 => EvaluateGardenWaterPath(t, _gestureSample),
                    2 => EvaluateGardenTendPath(t, _gestureSample),
                    _ => GardenWateringCanBasePositions[2]
                };
                Vector3 canOffset = rosePosition - GardenWateringCanBasePositions[2];
                for (int i = 0; i < _gardenProps.Length; i++)
                    _gardenProps[i].localPosition = GardenWateringCanBasePositions[i] + canOffset;
                float pour = step == 1
                    ? Mathf.Clamp01(Mathf.Sin(Mathf.Clamp01((t - 0.08f) * 1.35f) * Mathf.PI))
                    : 0f;
                _gardenProps[0].localRotation = Quaternion.Euler(0f, 0f, -18f * pour);
                _gardenProps[1].localRotation = Quaternion.Euler(0f, 0f, 64f - 18f * pour);
                _gardenProps[2].localRotation = Quaternion.Euler(90f, 0f, 64f - 18f * pour);
                _gardenProps[3].localRotation = Quaternion.Euler(0f, 0f, -28f - 14f * pour);
            }

            for (int i = 0; i < _gardenSparkles.Length; i++)
            {
                float phase = Mathf.Repeat(t * 1.4f + i * 0.17f, 1f);
                _gardenSparkles[i].gameObject.SetActive(step == 3 && t > 0.18f && phase < 0.82f);
                _gardenSparkles[i].localPosition = new Vector3(-0.45f + i * 0.14f,
                    0.62f + phase * 0.28f, Mathf.Sin(i * 1.7f) * 0.18f);
                float size = Mathf.Sin(phase * Mathf.PI) * 0.04f *
                    (step == 3 ? EvaluateGardenBloomIntensity(_gestureSample) : 1f);
                _gardenSparkles[i].localScale = Vector3.one * Mathf.Max(0.01f, size);
            }
        }

        void BuildReadStage()
        {
            bool hasAuthoredSet = BindPersistentActivitySet(MoonlightSpatialActionKind.Read,
                "MOONLIGHT_AUTHORED_READING_NOOK_READY", out _authoredReadingNook,
                out int rendererCount, out int materialCount, out int colliderCount,
                out int lightCount, out Vector3 boundsSize);
            if (!hasAuthoredSet)
                hasAuthoredSet = BuildAuthoredActivitySet("Models/Hero/MoonReadingNook", "MoonReadingNookAuthored",
                    new Vector3(-0.08f, 0.01f, 0.06f), 0.86f, "MOONLIGHT_AUTHORED_READING_NOOK_READY",
                    out _authoredReadingNook, out rendererCount, out materialCount,
                    out colliderCount, out lightCount, out boundsSize);
            if (!hasAuthoredSet)
            {
                Primitive(PrimitiveType.Cylinder, "ReadingPedestal", new Vector3(0f, 0.10f, 0f),
                    new Vector3(0.72f, 0.13f, 0.72f), new Color(0.40f, 0.32f, 0.52f), 0.04f);
                Primitive(PrimitiveType.Cylinder, "PedestalTop", new Vector3(0f, 0.24f, 0f),
                    new Vector3(0.82f, 0.035f, 0.82f), new Color(0.58f, 0.48f, 0.70f), 0.06f);
            }
            AuthoredReadingNookRendererCount = rendererCount;
            AuthoredReadingNookMaterialCount = materialCount;
            AuthoredReadingNookColliderCount = colliderCount;
            AuthoredReadingNookLightCount = lightCount;
            AuthoredReadingNookBoundsSize = boundsSize;

            _bookProps = new[]
            {
                Primitive(PrimitiveType.Cube, "BookSpine", new Vector3(0f, 0.39f, 0f),
                    new Vector3(0.08f, 0.07f, 0.58f), new Color(0.50f, 0.18f, 0.32f), 0.05f),
                Primitive(PrimitiveType.Cube, "LeftCover", new Vector3(-0.23f, 0.42f, 0f),
                    new Vector3(0.44f, 0.045f, 0.58f), new Color(0.62f, 0.24f, 0.38f), 0.05f),
                Primitive(PrimitiveType.Cube, "RightCover", new Vector3(0.23f, 0.42f, 0f),
                    new Vector3(0.44f, 0.045f, 0.58f), new Color(0.62f, 0.24f, 0.38f), 0.05f),
                Primitive(PrimitiveType.Cube, "LeftPageStack", new Vector3(-0.22f, 0.455f, 0f),
                    new Vector3(0.40f, 0.026f, 0.52f), new Color(0.98f, 0.92f, 0.76f), 0.08f),
                Primitive(PrimitiveType.Cube, "RightPageStack", new Vector3(0.22f, 0.455f, 0f),
                    new Vector3(0.40f, 0.026f, 0.52f), new Color(0.98f, 0.92f, 0.76f), 0.08f),
            };
            _bookProps[1].localRotation = Quaternion.Euler(0f, 0f, 7f);
            _bookProps[2].localRotation = Quaternion.Euler(0f, 0f, -7f);
            _bookProps[3].localRotation = Quaternion.Euler(0f, 0f, 7f);
            _bookProps[4].localRotation = Quaternion.Euler(0f, 0f, -7f);

            _bookmark = Primitive(PrimitiveType.Cube, "RibbonBookmark", new Vector3(0.05f, 0.49f, -0.18f),
                new Vector3(0.045f, 0.018f, 0.34f), new Color(0.96f, 0.58f, 0.20f), 0.12f);
            _bookmark.localRotation = Quaternion.Euler(0f, 0f, -5f);

            _pageFlips = new Transform[4];
            for (int i = 0; i < _pageFlips.Length; i++)
            {
                _pageFlips[i] = Primitive(PrimitiveType.Cube, $"PageFlip-{i + 1}",
                    new Vector3(0.17f, 0.49f + i * 0.008f, 0f), new Vector3(0.36f, 0.012f, 0.50f),
                    new Color(1f, 0.95f, 0.80f, 0.82f), 0.10f, true);
                _pageFlips[i].localRotation = Quaternion.Euler(0f, 0f, -10f);
            }

            _readMotes = new Transform[9];
            for (int i = 0; i < _readMotes.Length; i++)
            {
                _readMotes[i] = Primitive(PrimitiveType.Sphere, $"ReadingStarMote-{i + 1}",
                    Vector3.zero, Vector3.one * 0.045f,
                    i % 2 == 0 ? new Color(1f, 0.88f, 0.36f, 0.80f) : new Color(0.70f, 0.86f, 1f, 0.70f),
                    0.32f, true);
            }

            AddActivityLight(new Color(0.94f, 0.76f, 0.42f));
        }

        public static Vector2 EvaluateReadOpeningAngles(float progress,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            float eased = Mathf.Sin(t * Mathf.PI * 0.5f);
            Vector2 start = sample.HasSevenFiniteNormalizedPoints
                ? sample.Start
                : new Vector2(-0.46f, 0f);
            Vector2 displacement = sample.HasSevenFiniteNormalizedPoints
                ? sample.Displacement
                : new Vector2(0.92f, 0f);
            float directionalSignal = Mathf.Clamp(displacement.x + start.x * 0.65f,
                -1f, 1f);
            float gestureAmount = Mathf.Clamp01(
                Mathf.Clamp01(sample.Score) * 0.35f +
                Mathf.Clamp01(displacement.magnitude) * 0.35f +
                Mathf.Abs(start.x) * 0.30f);
            float amount = Mathf.Lerp(0.55f, 1f, gestureAmount);
            float left = 4f + 8f * eased * amount * (1f - directionalSignal * 0.35f);
            float right = 4f + 8f * eased * amount * (1f + directionalSignal * 0.35f);
            return new Vector2(Mathf.Clamp(left, 4f, 14f),
                -Mathf.Clamp(right, 4f, 14f));
        }

        public static Vector3 EvaluateReadPagePosition(float progress,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            Vector2 gesturePoint = ReadSamplePoint(sample, t);
            float arc = Mathf.Max(0f, Mathf.Sin(t * Mathf.PI));
            float pathAmount = sample.HasSevenFiniteNormalizedPoints
                ? Mathf.Clamp01(sample.DisplacementMagnitude)
                : 0.92f;
            return new Vector3(
                Mathf.Clamp(gesturePoint.x * 0.50f, -ReadPageMaximumX, ReadPageMaximumX),
                0.50f + arc * Mathf.Lerp(0.10f, 0.20f, pathAmount),
                Mathf.Clamp(gesturePoint.y * 0.30f, -ReadPageMaximumZ, ReadPageMaximumZ));
        }

        public static Vector3 EvaluateReadPageEuler(float progress,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            Vector2 gesturePoint = ReadSamplePoint(sample, t);
            float direction = ReadTraversalDirection(sample);
            float turn = direction >= 0f
                ? Mathf.Lerp(180f, 0f, t)
                : Mathf.Lerp(0f, 180f, t);
            return new Vector3(-Mathf.Max(0f, Mathf.Sin(t * Mathf.PI)) * 8f, turn,
                Mathf.Clamp(gesturePoint.y * 18f, -18f, 18f));
        }

        public static Vector3 EvaluateReadBookmarkPosition(float progress,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            Vector2 gesturePoint = ReadSamplePoint(sample, t);
            float arc = Mathf.Max(0f, Mathf.Sin(t * Mathf.PI));
            return new Vector3(
                Mathf.Clamp(0.05f + gesturePoint.x * 0.24f,
                    ReadBookmarkMinimumX, ReadBookmarkMaximumX),
                0.49f + arc * 0.015f,
                Mathf.Clamp(-0.18f + gesturePoint.y * 0.20f,
                    ReadBookmarkMinimumZ, ReadBookmarkMaximumZ));
        }

        public static Vector3 EvaluateReadBookmarkEuler(float progress,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            Vector2 gesturePoint = ReadSamplePoint(sample, t);
            Vector2 from = ReadSamplePoint(sample, Mathf.Max(0f, t - 0.025f));
            Vector2 to = ReadSamplePoint(sample, Mathf.Min(1f, t + 0.025f));
            Vector2 tangent = to - from;
            float yaw = tangent.sqrMagnitude > 0.000001f
                ? Mathf.Atan2(tangent.x, tangent.y) * Mathf.Rad2Deg
                : 0f;
            return new Vector3(0f, Mathf.Clamp(yaw, -180f, 180f),
                Mathf.Clamp(gesturePoint.x * 10f, -10f, 10f));
        }

        public static float EvaluateReadFinishIntensity(MoonlightGestureSample sample)
        {
            float score = Mathf.Clamp01(IsFinite(sample.Score) ? sample.Score : 0f);
            float duration = Mathf.Clamp(IsFinite(sample.Duration) ? sample.Duration : 0f,
                0f, 8f);
            float durationResponse = Mathf.InverseLerp(0.25f, 1.25f, duration);
            return ReadFinishMinimumIntensity + score * 0.55f + durationResponse * 0.30f;
        }

        public static float EvaluateReadLightTarget(float progress, int step,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            float target = ActivityLightBaseIntensity +
                Mathf.Sin(t * Mathf.PI) * ActivityLightPulseIntensity;
            if (Mathf.Clamp(step, 0, 3) == 3)
                target *= EvaluateReadFinishIntensity(sample);
            return Mathf.Max(ActivityLightBaseIntensity, target);
        }

        public static int EvaluateReadFinishMoteCount(MoonlightGestureSample sample)
        {
            float response = Mathf.InverseLerp(ReadFinishMinimumIntensity,
                ReadFinishMaximumIntensity, EvaluateReadFinishIntensity(sample));
            return Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(4f, 9f, response)), 4, 9);
        }

        public static Vector3 EvaluateReadFinishMotePosition(int moteIndex, float progress,
            MoonlightGestureSample sample)
        {
            int index = Mathf.Clamp(moteIndex, 0, 8);
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            float score = Mathf.Clamp01(IsFinite(sample.Score) ? sample.Score : 0f);
            float durationResponse = Mathf.InverseLerp(0.25f, 1.25f,
                Mathf.Clamp(IsFinite(sample.Duration) ? sample.Duration : 0f, 0f, 8f));
            float phase = Mathf.Repeat(t * Mathf.Lerp(0.90f, 1.25f, durationResponse) +
                index * 0.13f, 1f);
            float angle = index * Mathf.PI * 2f / 9f +
                t * Mathf.PI * Mathf.Lerp(0.55f, 1f, score);
            float radius = 0.24f + index * 0.02f + score * 0.04f;
            return new Vector3(Mathf.Cos(angle) * radius,
                0.60f + phase * Mathf.Lerp(0.34f, 0.58f, durationResponse),
                Mathf.Sin(angle) * radius);
        }

        public static float EvaluateReadFinishMoteScale(int moteIndex, float progress,
            MoonlightGestureSample sample)
        {
            int index = Mathf.Clamp(moteIndex, 0, 8);
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            float durationResponse = Mathf.InverseLerp(0.25f, 1.25f,
                Mathf.Clamp(IsFinite(sample.Duration) ? sample.Duration : 0f, 0f, 8f));
            float phase = Mathf.Repeat(t * Mathf.Lerp(0.90f, 1.25f, durationResponse) +
                index * 0.13f, 1f);
            return (0.016f + Mathf.Sin(phase * Mathf.PI) * 0.032f) *
                EvaluateReadFinishIntensity(sample);
        }

        public static bool ValidateGestureResponsiveReadContract(out string detail)
        {
            MoonlightGestureSample leftOpen = ReadLineSample(
                new Vector2(-0.80f, -0.05f), new Vector2(-0.40f, 0.05f), 0.85f, 0.45f);
            MoonlightGestureSample rightOpen = ReadLineSample(
                new Vector2(0.80f, -0.05f), new Vector2(0.40f, 0.05f), 0.85f, 0.45f);
            MoonlightGestureSample compact = ReadLineSample(
                new Vector2(-0.10f, 0f), new Vector2(0.10f, 0f), 0.55f, 0.45f);
            MoonlightGestureSample broad = ReadTraceSample(0.80f, false, 0.95f, 0.80f);
            MoonlightGestureSample reverse = ReadTraceSample(0.80f, true, 0.95f, 0.80f);
            MoonlightGestureSample narrow = ReadTraceSample(0.20f, false, 0.95f, 0.80f);
            MoonlightGestureSample lowScore = ReadTraceSample(0.60f, false, 0.20f, 0.70f);
            MoonlightGestureSample highScore = ReadTraceSample(0.60f, false, 0.95f, 0.70f);
            MoonlightGestureSample shortHold = ReadTraceSample(0.60f, false, 0.70f, 0.30f);
            MoonlightGestureSample longHold = ReadTraceSample(0.60f, false, 0.70f, 1.20f);

            Vector2 leftAngles = EvaluateReadOpeningAngles(1f, leftOpen);
            Vector2 rightAngles = EvaluateReadOpeningAngles(1f, rightOpen);
            Vector2 compactAngles = EvaluateReadOpeningAngles(1f, compact);
            Vector2 broadAngles = EvaluateReadOpeningAngles(1f, broad);
            float compactOpening = compactAngles.x - compactAngles.y;
            float broadOpening = broadAngles.x - broadAngles.y;
            float openingDirectionDelta = leftAngles.x - rightAngles.x;
            bool openingDirection = openingDirectionDelta > 0.50f &&
                (-rightAngles.y) - (-leftAngles.y) > 0.50f &&
                Mathf.Abs(leftAngles.x + rightAngles.y) <= 0.001f &&
                Mathf.Abs(leftAngles.y + rightAngles.x) <= 0.001f;
            bool openingAmount = broadOpening > compactOpening + 3f;

            bool pageReversal = true;
            bool bookmarkReversal = true;
            bool bookmarkOrientationReversal = true;
            bool finiteAndBounded = true;
            MoonlightGestureSample[] samples =
            {
                leftOpen, rightOpen, compact, broad, reverse, narrow,
                lowScore, highScore, shortHold, longHold
            };
            for (int i = 0; i <= 40; i++)
            {
                float t = i / 40f;
                pageReversal &= Vector3.Distance(EvaluateReadPagePosition(t, broad),
                        EvaluateReadPagePosition(1f - t, reverse)) <= 0.0001f &&
                    Vector3.Distance(EvaluateReadPageEuler(t, broad),
                        EvaluateReadPageEuler(1f - t, reverse)) <= 0.0001f;
                bookmarkReversal &= Vector3.Distance(EvaluateReadBookmarkPosition(t, broad),
                    EvaluateReadBookmarkPosition(1f - t, reverse)) <= 0.0001f;
                Vector3 bookmarkEuler = EvaluateReadBookmarkEuler(t, broad);
                Vector3 reversedBookmarkEuler = EvaluateReadBookmarkEuler(1f - t, reverse);
                float bookmarkYawDelta = Mathf.Abs(Mathf.DeltaAngle(
                    bookmarkEuler.y, reversedBookmarkEuler.y));
                Vector3 bookmarkAxis = Quaternion.Euler(bookmarkEuler) * Vector3.forward;
                Vector3 reversedBookmarkAxis =
                    Quaternion.Euler(reversedBookmarkEuler) * Vector3.forward;
                // Reversal flips the path tangent by 180 degrees. The ribbon is symmetric
                // along that axis, so opposite longitudinal vectors are visually equivalent.
                bookmarkOrientationReversal &=
                    Mathf.Abs(bookmarkYawDelta - 180f) <= 0.001f &&
                    Mathf.Abs(bookmarkEuler.z - reversedBookmarkEuler.z) <= 0.001f &&
                    Vector3.Dot(bookmarkAxis, reversedBookmarkAxis) <= -0.9999f;
                foreach (MoonlightGestureSample sample in samples)
                {
                    finiteAndBounded &= ReadOpeningIsFiniteAndBounded(
                            EvaluateReadOpeningAngles(t, sample)) &&
                        ReadPageIsFiniteAndBounded(EvaluateReadPagePosition(t, sample)) &&
                        ReadPageEulerIsFiniteAndBounded(EvaluateReadPageEuler(t, sample)) &&
                        ReadBookmarkIsFiniteAndBounded(
                            EvaluateReadBookmarkPosition(t, sample)) &&
                        ReadBookmarkEulerIsFiniteAndBounded(
                            EvaluateReadBookmarkEuler(t, sample));
                    for (int mote = 0; mote < 9; mote++)
                    {
                        Vector3 motePosition = EvaluateReadFinishMotePosition(mote, t, sample);
                        float moteScale = EvaluateReadFinishMoteScale(mote, t, sample);
                        finiteAndBounded &= IsFinite(motePosition) &&
                            Mathf.Abs(motePosition.x) <= 0.45f &&
                            motePosition.y >= 0.60f && motePosition.y <= 1.18f &&
                            Mathf.Abs(motePosition.z) <= 0.45f && IsFinite(moteScale) &&
                            moteScale >= 0.01f && moteScale <= 0.09f;
                    }
                    float finishMultiplier = EvaluateReadFinishIntensity(sample);
                    float lightTarget = EvaluateReadLightTarget(t, 3, sample);
                    finiteAndBounded &= IsFinite(finishMultiplier) &&
                        finishMultiplier >= ReadFinishMinimumIntensity &&
                        finishMultiplier <= ReadFinishMaximumIntensity &&
                        IsFinite(lightTarget) && lightTarget >= ActivityLightBaseIntensity &&
                        lightTarget <= 1.58f;
                }
            }
            finiteAndBounded &= ReadPageIsFiniteAndBounded(
                    EvaluateReadPagePosition(float.NaN, broad)) &&
                ReadBookmarkIsFiniteAndBounded(
                    EvaluateReadBookmarkPosition(float.PositiveInfinity, broad));

            float narrowPageSpan = ReadPathSpan(narrow, true, true);
            float broadPageSpan = ReadPathSpan(broad, true, true);
            float narrowBookmarkSpan = ReadPathSpan(narrow, false, true);
            float broadBookmarkSpan = ReadPathSpan(broad, false, true);
            bool pathSpan = broadPageSpan > narrowPageSpan + 0.30f &&
                broadBookmarkSpan > narrowBookmarkSpan + 0.20f;
            float scoreDelta = EvaluateReadFinishIntensity(highScore) -
                EvaluateReadFinishIntensity(lowScore);
            float durationDelta = EvaluateReadFinishIntensity(longHold) -
                EvaluateReadFinishIntensity(shortHold);
            int lowMotes = EvaluateReadFinishMoteCount(lowScore);
            int highMotes = EvaluateReadFinishMoteCount(highScore);
            int shortMotes = EvaluateReadFinishMoteCount(shortHold);
            int longMotes = EvaluateReadFinishMoteCount(longHold);
            bool finishResponse = scoreDelta >= 0.40f && durationDelta >= 0.25f &&
                highMotes >= lowMotes + 2 && longMotes >= shortMotes + 1 &&
                EvaluateReadFinishIntensity(default) >= 1f &&
                EvaluateReadLightTarget(1f, 3, default) >= ActivityLightBaseIntensity;

            detail = $"points={broad.PointCount} openingDirection=" +
                $"{openingDirection}/{openingDirectionDelta:0.000}deg " +
                $"openingAmount={compactOpening:0.00}/{broadOpening:0.00} " +
                $"reversals={pageReversal}/{bookmarkReversal}/" +
                $"orientation={bookmarkOrientationReversal} " +
                $"pageSpan={narrowPageSpan:0.000}/{broadPageSpan:0.000} " +
                $"bookmarkSpan={narrowBookmarkSpan:0.000}/{broadBookmarkSpan:0.000} " +
                $"scoreDelta={scoreDelta:0.000} motes={lowMotes}/{highMotes} " +
                $"durationDelta={durationDelta:0.000} motes={shortMotes}/{longMotes} " +
                $"finiteBounds={finiteAndBounded} " +
                $"stageObjects={RequiredReadStageRendererCount}renderers/" +
                $"{RequiredReadStageMaterialCount}materials lights={ReadLightBudget}";
            return broad.HasSevenFiniteNormalizedPoints &&
                reverse.HasSevenFiniteNormalizedPoints && openingDirection && openingAmount &&
                pageReversal && bookmarkReversal && bookmarkOrientationReversal && pathSpan &&
                finishResponse && finiteAndBounded && ReadRendererBudget == 48 &&
                ReadMaterialBudget == 28 && ReadLightBudget == 1 &&
                RequiredReadStageRendererCount == 19 &&
                RequiredReadStageMaterialCount == 7;
        }

        static Vector2 ReadSamplePoint(MoonlightGestureSample sample, float progress)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            return sample.HasSevenFiniteNormalizedPoints
                ? InterpolateSamplePoint(sample, t)
                : new Vector2(Mathf.Lerp(-0.46f, 0.46f, t), 0f);
        }

        static float ReadTraversalDirection(MoonlightGestureSample sample)
        {
            if (!sample.HasSevenFiniteNormalizedPoints) return 1f;
            Vector2 displacement = sample.Displacement;
            float primary = Mathf.Abs(displacement.x) >= Mathf.Abs(displacement.y)
                ? displacement.x
                : displacement.y;
            return Mathf.Abs(primary) > 0.0001f ? Mathf.Sign(primary) : 1f;
        }

        static MoonlightGestureSample ReadLineSample(Vector2 start, Vector2 end,
            float score, float duration)
        {
            var points = new Vector2[MoonlightGestureSample.ResampledPointCount];
            for (int i = 0; i < points.Length; i++)
                points[i] = Vector2.Lerp(start, end, i / (float)(points.Length - 1));
            return MoonlightGestureSample.Create(score, duration, points);
        }

        static MoonlightGestureSample ReadTraceSample(float scale, bool reverse,
            float score, float duration)
        {
            Vector2[] shape =
            {
                new(-1f, -0.55f), new(-0.58f, 0.35f), new(-0.24f, -0.25f),
                new(0.05f, 0.72f), new(0.34f, -0.42f), new(0.68f, 0.28f),
                new(1f, 0.58f)
            };
            var points = new Vector2[MoonlightGestureSample.ResampledPointCount];
            for (int i = 0; i < points.Length; i++)
            {
                int source = reverse ? points.Length - 1 - i : i;
                points[i] = shape[source] * scale;
            }
            return MoonlightGestureSample.Create(score, duration, points);
        }

        static float ReadPathSpan(MoonlightGestureSample sample, bool page, bool xAxis)
        {
            float minimum = float.PositiveInfinity;
            float maximum = float.NegativeInfinity;
            for (int i = 0; i <= 40; i++)
            {
                Vector3 point = page
                    ? EvaluateReadPagePosition(i / 40f, sample)
                    : EvaluateReadBookmarkPosition(i / 40f, sample);
                float coordinate = xAxis ? point.x : point.z;
                minimum = Mathf.Min(minimum, coordinate);
                maximum = Mathf.Max(maximum, coordinate);
            }
            return maximum - minimum;
        }

        static bool ReadOpeningIsFiniteAndBounded(Vector2 angles) =>
            IsFinite(angles.x) && IsFinite(angles.y) &&
            angles.x >= 4f && angles.x <= 14f &&
            angles.y <= -4f && angles.y >= -14f;

        static bool ReadPageIsFiniteAndBounded(Vector3 point) => IsFinite(point) &&
            Mathf.Abs(point.x) <= ReadPageMaximumX && point.y >= 0.50f &&
            point.y <= 0.70f && Mathf.Abs(point.z) <= ReadPageMaximumZ;

        static bool ReadPageEulerIsFiniteAndBounded(Vector3 euler) => IsFinite(euler) &&
            euler.x >= -8f && euler.x <= 0f && euler.y >= 0f && euler.y <= 180f &&
            Mathf.Abs(euler.z) <= 26f;

        static bool ReadBookmarkIsFiniteAndBounded(Vector3 point) => IsFinite(point) &&
            point.x >= ReadBookmarkMinimumX && point.x <= ReadBookmarkMaximumX &&
            point.y >= 0.49f && point.y <= 0.505f &&
            point.z >= ReadBookmarkMinimumZ && point.z <= ReadBookmarkMaximumZ;

        static bool ReadBookmarkEulerIsFiniteAndBounded(Vector3 euler) => IsFinite(euler) &&
            Mathf.Abs(euler.x) <= 0.0001f && Mathf.Abs(euler.y) <= 180f &&
            Mathf.Abs(euler.z) <= 10f;

        Vector2 ReadCoverAngles()
        {
            if (_bookProps == null || _bookProps.Length < 5 || _bookProps[1] == null ||
                _bookProps[2] == null)
                return new Vector2(float.NaN, float.NaN);
            return new Vector2(SignedLocalZ(_bookProps[1]), SignedLocalZ(_bookProps[2]));
        }

        bool ReadOpeningTransformsMatch()
        {
            if (_bookProps == null || _bookProps.Length < 5) return false;
            Vector2 expected = ReadExpectedOpeningAngles;
            Vector2 actual = ReadActualOpeningAngles;
            float openingProgress = Mathf.Clamp01(_readProgress);
            float expectedLeftStack = Mathf.Clamp(expected.x + openingProgress, 4f, 15f);
            float expectedRightStack = Mathf.Clamp(expected.y - openingProgress, -15f, -4f);
            return IsFinite(actual.x) && IsFinite(actual.y) &&
                Mathf.Abs(actual.x - expected.x) <= 0.001f &&
                Mathf.Abs(actual.y - expected.y) <= 0.001f &&
                _bookProps[3] != null && _bookProps[4] != null &&
                Mathf.Abs(SignedLocalZ(_bookProps[3]) - expectedLeftStack) <= 0.001f &&
                Mathf.Abs(SignedLocalZ(_bookProps[4]) - expectedRightStack) <= 0.001f;
        }

        bool ReadPageTransformsMatch()
        {
            if (_pageFlips == null || _pageFlips.Length != 4) return false;
            for (int i = 0; i < _pageFlips.Length; i++)
            {
                Transform page = _pageFlips[i];
                if (page == null || !page.gameObject.activeSelf) return false;
                float phase = Mathf.Repeat(_readProgress * 1.6f + i * 0.22f, 1f);
                Vector3 expectedPosition = EvaluateReadPagePosition(phase, _gestureSample) +
                    Vector3.up * (i * 0.006f);
                Quaternion expectedRotation = Quaternion.Euler(
                    EvaluateReadPageEuler(phase, _gestureSample));
                if (Vector3.Distance(page.localPosition, expectedPosition) > 0.001f ||
                    Quaternion.Angle(page.localRotation, expectedRotation) > 0.01f)
                    return false;
            }
            return true;
        }

        bool ReadBookmarkTransformMatches()
        {
            if (_bookmark == null || !_bookmark.gameObject.activeSelf) return false;
            Vector3 expectedPosition = EvaluateReadBookmarkPosition(_readProgress, _gestureSample);
            Quaternion expectedRotation = Quaternion.Euler(
                EvaluateReadBookmarkEuler(_readProgress, _gestureSample));
            return Vector3.Distance(_bookmark.localPosition, expectedPosition) <= 0.001f &&
                Quaternion.Angle(_bookmark.localRotation, expectedRotation) <= 0.01f;
        }

        bool ReadFinishTransformsMatch()
        {
            if (_readMotes == null || _readMotes.Length != 9 || _activityLight == null)
                return false;
            int expectedCount = EvaluateReadFinishMoteCount(_gestureSample);
            for (int i = 0; i < _readMotes.Length; i++)
            {
                Transform mote = _readMotes[i];
                bool expectedActive = i < expectedCount;
                if (mote == null || mote.gameObject.activeSelf != expectedActive) return false;
                if (!expectedActive) continue;
                Vector3 expectedPosition = EvaluateReadFinishMotePosition(
                    i, _readProgress, _gestureSample);
                Vector3 expectedScale = Vector3.one * EvaluateReadFinishMoteScale(
                    i, _readProgress, _gestureSample);
                if (Vector3.Distance(mote.localPosition, expectedPosition) > 0.001f ||
                    Vector3.Distance(mote.localScale, expectedScale) > 0.001f)
                    return false;
            }
            return CountActiveReadMotes() == expectedCount &&
                IsFinite(_activityLight.intensity) &&
                Mathf.Abs(_activityLight.intensity - ReadExpectedLightIntensity) <= 0.001f &&
                _activityLight.intensity >= ActivityLightBaseIntensity;
        }

        int CountActiveReadMotes()
        {
            int count = 0;
            if (_readMotes == null) return count;
            for (int i = 0; i < _readMotes.Length; i++)
                if (_readMotes[i] != null && _readMotes[i].gameObject.activeSelf) count++;
            return count;
        }

        int CountReadStageRenderers()
        {
            int count = CountDirectRenderers(_bookProps) + CountDirectRenderers(_pageFlips) +
                CountDirectRenderers(_readMotes);
            if (_bookmark != null && _bookmark.GetComponent<Renderer>() != null) count++;
            return count;
        }

        int CountReadStageUniqueMaterials()
        {
            var materialIds = new HashSet<int>();
            AddDirectMaterialIds(_bookProps, materialIds);
            AddDirectMaterialIds(_pageFlips, materialIds);
            AddDirectMaterialIds(_readMotes, materialIds);
            if (_bookmark != null)
            {
                Renderer renderer = _bookmark.GetComponent<Renderer>();
                if (renderer != null && renderer.sharedMaterial != null)
                    materialIds.Add(renderer.sharedMaterial.GetInstanceID());
            }
            return materialIds.Count;
        }

        static int CountDirectRenderers(Transform[] transforms)
        {
            int count = 0;
            if (transforms == null) return count;
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i] != null && transforms[i].GetComponent<Renderer>() != null)
                    count++;
            return count;
        }

        static void AddDirectMaterialIds(Transform[] transforms, HashSet<int> materialIds)
        {
            if (transforms == null) return;
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] == null) continue;
                Renderer renderer = transforms[i].GetComponent<Renderer>();
                if (renderer != null && renderer.sharedMaterial != null)
                    materialIds.Add(renderer.sharedMaterial.GetInstanceID());
            }
        }

        static float SignedLocalZ(Transform target) =>
            target != null ? Mathf.DeltaAngle(0f, target.localEulerAngles.z) : float.NaN;

        void UpdateRead(float t)
        {
            if (_pageFlips == null || _readMotes == null) return;

            int step = Mathf.Clamp(CurrentStep, 0, 3);
            _readProgress = t;

            if (_bookProps != null)
            {
                Vector2 openingAngles = EvaluateReadOpeningAngles(step == 0 ? t : 1f,
                    _gestureSample);
                float openingProgress = step == 0 ? Mathf.Clamp01(t) : 1f;
                _bookProps[1].localRotation = Quaternion.Euler(0f, 0f, openingAngles.x);
                _bookProps[2].localRotation = Quaternion.Euler(0f, 0f, openingAngles.y);
                _bookProps[3].localRotation = Quaternion.Euler(0f, 0f,
                    Mathf.Clamp(openingAngles.x + openingProgress, 4f, 15f));
                _bookProps[4].localRotation = Quaternion.Euler(0f, 0f,
                    Mathf.Clamp(openingAngles.y - openingProgress, -15f, -4f));
            }

            for (int i = 0; i < _pageFlips.Length; i++)
            {
                bool showPage = step == 1;
                _pageFlips[i].gameObject.SetActive(showPage);
                if (!showPage) continue;
                float phase = Mathf.Repeat(t * 1.6f + i * 0.22f, 1f);
                float turn = Mathf.Sin(phase * Mathf.PI);
                _pageFlips[i].localPosition = EvaluateReadPagePosition(phase, _gestureSample) +
                    Vector3.up * (i * 0.006f);
                _pageFlips[i].localScale = new Vector3(Mathf.Lerp(0.36f, 0.26f, turn), 0.012f, 0.50f);
                _pageFlips[i].localRotation = Quaternion.Euler(
                    EvaluateReadPageEuler(phase, _gestureSample));
            }

            if (_bookmark != null)
            {
                if (step == 2)
                {
                    _bookmark.localPosition = EvaluateReadBookmarkPosition(t, _gestureSample);
                    _bookmark.localRotation = Quaternion.Euler(
                        EvaluateReadBookmarkEuler(t, _gestureSample));
                }
                else
                {
                    _bookmark.localPosition = new Vector3(0.05f, 0.49f, -0.18f);
                    _bookmark.localRotation = Quaternion.Euler(0f, 0f, -5f);
                }
            }

            for (int i = 0; i < _readMotes.Length; i++)
            {
                int finishMoteCount = EvaluateReadFinishMoteCount(_gestureSample);
                bool showMote = step == 3 ? i < finishMoteCount :
                    step == 2 || (step == 0 && i < 3);
                _readMotes[i].gameObject.SetActive(showMote);
                if (!showMote) continue;
                if (step == 3)
                {
                    _readMotes[i].localPosition = EvaluateReadFinishMotePosition(
                        i, t, _gestureSample);
                    _readMotes[i].localScale = Vector3.one * EvaluateReadFinishMoteScale(
                        i, t, _gestureSample);
                    continue;
                }
                float phase = Mathf.Repeat(t * (step == 3 ? 1.15f : 0.72f) + i * 0.13f, 1f);
                float angle = i * Mathf.PI * 2f / _readMotes.Length + t * Mathf.PI * (step == 2 ? 1.8f : 0.8f);
                float radius = step == 2 ? 0.22f + i * 0.018f : 0.28f + i * 0.025f;
                float rise = step == 3 ? phase * 0.52f : Mathf.Sin(angle * 1.7f) * 0.10f;
                _readMotes[i].localPosition = new Vector3(Mathf.Cos(angle) * radius,
                    0.60f + rise, Mathf.Sin(angle) * (step == 2 ? 0.18f : 0.28f));
                float pulse = step == 3 ? Mathf.Sin(phase * Mathf.PI) : 0.55f + Mathf.Sin(angle * 2f) * 0.20f;
                _readMotes[i].localScale = Vector3.one * (0.025f + Mathf.Max(0f, pulse) * 0.040f);
            }
        }

        public static Vector3 EvaluateCareTowelPosition(float progress,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            Vector2 tap = sample.HasSevenFiniteNormalizedPoints ? sample.Start : Vector2.zero;
            Vector3 target = new(
                Mathf.Clamp(-0.58f + tap.x * 0.175f,
                    CareTowelMinimumX, CareTowelMaximumX),
                0.335f,
                Mathf.Clamp(0.02f + tap.y * 0.125f,
                    CareTowelMinimumZ, CareTowelMaximumZ));
            return Vector3.Lerp(new Vector3(-0.78f, 0.72f, -0.10f), target, eased);
        }

        public static Vector3 EvaluateCareTowelEuler(float progress,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            Vector2 tap = sample.HasSevenFiniteNormalizedPoints ? sample.Start : Vector2.zero;
            return new Vector3(0f, Mathf.Lerp(-18f, tap.x * 7f, eased),
                Mathf.Sin(t * Mathf.PI * 3f) * 4f + tap.y * (1f - eased) * 3f);
        }

        public static float EvaluateCareWashDirection(MoonlightGestureSample sample)
        {
            float signedArea = CareSampleSignedArea(sample);
            return Mathf.Abs(signedArea) > 0.0001f ? Mathf.Sign(signedArea) : 1f;
        }

        public static float EvaluateCareWashRadius(MoonlightGestureSample sample)
        {
            float extent = CareSampleRadialExtent(sample);
            float response = Mathf.InverseLerp(0.12f, 0.48f, extent);
            return Mathf.Lerp(CareWashMinimumRadius, CareWashMaximumRadius, response);
        }

        public static Vector3 EvaluateCareWashBrushPosition(float progress,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            float direction = EvaluateCareWashDirection(sample);
            float radius = EvaluateCareWashRadius(sample);
            float angle = direction * t * Mathf.PI * 4f;
            return new Vector3(Mathf.Cos(angle) * radius,
                0.62f + Mathf.Sin(t * Mathf.PI) * 0.07f,
                0.02f + Mathf.Sin(angle) * radius * 0.75f);
        }

        public static Vector3 EvaluateCareWashBrushEuler(float progress,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            return new Vector3(18f, EvaluateCareWashDirection(sample) * t * 540f, 64f);
        }

        public static Vector3 EvaluateCareWashBubblePosition(int bubbleIndex, float progress,
            MoonlightGestureSample sample)
        {
            int index = Mathf.Clamp(bubbleIndex, 0, 4);
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            float direction = EvaluateCareWashDirection(sample);
            float radius = EvaluateCareWashRadius(sample) * 0.65f + index * 0.025f;
            float phase = Mathf.Repeat(t * 1.45f + index * 0.19f, 1f);
            float angle = index * Mathf.PI * 2f / 5f +
                direction * t * Mathf.PI * 2f;
            return new Vector3(Mathf.Cos(angle) * radius,
                0.53f + phase * 0.30f,
                0.02f + Mathf.Sin(angle) * radius * 0.85f);
        }

        public static float EvaluateCareWashBubbleScale(int bubbleIndex, float progress)
        {
            int index = Mathf.Clamp(bubbleIndex, 0, 4);
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            float phase = Mathf.Repeat(t * 1.45f + index * 0.19f, 1f);
            return Mathf.Max(0.015f,
                Mathf.Sin(phase * Mathf.PI) * (0.055f + index * 0.006f));
        }

        public static Vector3 EvaluateCareCombPosition(float progress,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            float direction = CareSwipeDirection(sample);
            float from = direction >= 0f ? CareCombMaximumX : CareCombMinimumX;
            float to = direction >= 0f ? CareCombMinimumX : CareCombMaximumX;
            return new Vector3(Mathf.Lerp(from, to, eased),
                0.62f + Mathf.Sin(t * Mathf.PI * 3f) * 0.06f, 0.12f);
        }

        public static Vector3 EvaluateCareCombEuler(float progress,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            float direction = CareSwipeDirection(sample);
            float from = direction >= 0f ? -22f : 18f;
            float to = direction >= 0f ? 18f : -22f;
            return new Vector3(0f, 18f, Mathf.Lerp(from, to, t));
        }

        public static float EvaluateCareGlowScaleMultiplier(MoonlightGestureSample sample)
        {
            float score = Mathf.Clamp01(IsFinite(sample.Score) ? sample.Score : 0f);
            float duration = Mathf.Clamp(IsFinite(sample.Duration) ? sample.Duration : 0f,
                0f, 8f);
            float durationResponse = Mathf.InverseLerp(0.25f, 1.25f, duration);
            return 0.80f + score * 0.35f + durationResponse * 0.25f;
        }

        public static Vector3 EvaluateCareGlowAuraScale(float progress,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            float pulse = 1f + Mathf.Sin(t * Mathf.PI * 4f) * 0.10f;
            return new Vector3(0.38f, 0.018f, 0.46f) *
                (pulse * EvaluateCareGlowScaleMultiplier(sample));
        }

        public static float EvaluateCareGlowLightIntensity(float progress,
            MoonlightGestureSample sample)
        {
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            float score = Mathf.Clamp01(IsFinite(sample.Score) ? sample.Score : 0f);
            float duration = Mathf.Clamp(IsFinite(sample.Duration) ? sample.Duration : 0f,
                0f, 8f);
            float durationResponse = Mathf.InverseLerp(0.25f, 1.25f, duration);
            return ActivityLightBaseIntensity +
                Mathf.Sin(t * Mathf.PI) * ActivityLightPulseIntensity +
                score * 0.26f + durationResponse * 0.18f;
        }

        public static int EvaluateCareGlowMoteCount(MoonlightGestureSample sample)
        {
            float score = Mathf.Clamp01(IsFinite(sample.Score) ? sample.Score : 0f);
            float duration = Mathf.Clamp(IsFinite(sample.Duration) ? sample.Duration : 0f,
                0f, 8f);
            int scoreMotes = score >= 0.75f ? 2 : score >= 0.40f ? 1 : 0;
            int durationMotes = duration >= 0.90f ? 2 : duration >= 0.55f ? 1 : 0;
            return Mathf.Clamp(2 + scoreMotes + durationMotes, 2, 6);
        }

        public static Vector3 EvaluateCareGlowMotePosition(int moteIndex, float progress,
            MoonlightGestureSample sample)
        {
            int index = Mathf.Clamp(moteIndex, 0, 5);
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            float score = Mathf.Clamp01(IsFinite(sample.Score) ? sample.Score : 0f);
            float duration = Mathf.Clamp(IsFinite(sample.Duration) ? sample.Duration : 0f,
                0f, 8f);
            float durationResponse = Mathf.InverseLerp(0.25f, 1.25f, duration);
            float phase = Mathf.Repeat(t * Mathf.Lerp(0.72f, 1.02f, durationResponse) +
                index * 0.16f, 1f);
            float angle = index * Mathf.PI * 2f / 6f +
                t * Mathf.PI * Mathf.Lerp(0.62f, 0.96f, score);
            float radius = 0.36f + score * 0.06f;
            return new Vector3(0.59f + Mathf.Cos(angle) * radius,
                0.55f + phase * Mathf.Lerp(0.48f, 0.62f, durationResponse),
                0.20f + Mathf.Sin(angle) * 0.13f);
        }

        public static float EvaluateCareGlowMoteScale(int moteIndex, float progress,
            MoonlightGestureSample sample)
        {
            int index = Mathf.Clamp(moteIndex, 0, 5);
            float t = Mathf.Clamp01(IsFinite(progress) ? progress : 0f);
            float score = Mathf.Clamp01(IsFinite(sample.Score) ? sample.Score : 0f);
            float duration = Mathf.Clamp(IsFinite(sample.Duration) ? sample.Duration : 0f,
                0f, 8f);
            float durationResponse = Mathf.InverseLerp(0.25f, 1.25f, duration);
            float phase = Mathf.Repeat(t * Mathf.Lerp(0.72f, 1.02f, durationResponse) +
                index * 0.16f, 1f);
            return (0.020f + Mathf.Sin(phase * Mathf.PI) * 0.036f) *
                Mathf.Lerp(0.90f, 1.15f, score);
        }

        public static bool ValidateGestureResponsiveCareContract(out string detail)
        {
            MoonlightGestureSample leftTap = CareTapSample(new Vector2(-0.80f, 0f));
            MoonlightGestureSample rightTap = CareTapSample(new Vector2(0.80f, 0f));
            MoonlightGestureSample narrowCircle = CareCircleSample(0.14f, false);
            MoonlightGestureSample wideCircle = CareCircleSample(0.48f, false);
            MoonlightGestureSample reverseCircle = CareCircleSample(0.48f, true);
            MoonlightGestureSample rightSwipe = CareSwipeSample(false);
            MoonlightGestureSample leftSwipe = CareSwipeSample(true);
            MoonlightGestureSample lowScore = CareHoldSample(0.20f, 0.70f);
            MoonlightGestureSample highScore = CareHoldSample(0.95f, 0.70f);
            MoonlightGestureSample shortHold = CareHoldSample(0.70f, 0.30f);
            MoonlightGestureSample longHold = CareHoldSample(0.70f, 1.20f);
            MoonlightGestureSample minimumInput = CareTapSample(new Vector2(-1f, -1f));
            MoonlightGestureSample maximumInput = CareHoldSample(1f, 8f);
            MoonlightGestureSample maximumTap = CareTapSample(new Vector2(1f, 1f));
            MoonlightGestureSample malformedInput = MoonlightGestureSample.Create(
                float.NaN, float.PositiveInfinity,
                new[] { new Vector2(float.NaN, float.NegativeInfinity) });

            float towelSeparation = Vector3.Distance(
                EvaluateCareTowelPosition(1f, leftTap),
                EvaluateCareTowelPosition(1f, rightTap));
            Vector3 leftLanding = EvaluateCareTowelPosition(1f, leftTap);
            Vector3 rightLanding = EvaluateCareTowelPosition(1f, rightTap);
            bool prepLandingBounds = leftLanding.x >= CareTowelMinimumX &&
                leftLanding.x <= CareTowelMaximumX &&
                rightLanding.x >= CareTowelMinimumX &&
                rightLanding.x <= CareTowelMaximumX &&
                leftLanding.z >= CareTowelMinimumZ && leftLanding.z <= CareTowelMaximumZ &&
                rightLanding.z >= CareTowelMinimumZ && rightLanding.z <= CareTowelMaximumZ;
            float narrowRadius = EvaluateCareWashRadius(narrowCircle);
            float wideRadius = EvaluateCareWashRadius(wideCircle);
            float radiusDelta = wideRadius - narrowRadius;
            float brushForwardArea = CareBrushOrbitSignedArea(wideCircle);
            float brushReverseArea = CareBrushOrbitSignedArea(reverseCircle);
            float bubbleForwardArea = CareBubbleOrbitSignedArea(wideCircle);
            float bubbleReverseArea = CareBubbleOrbitSignedArea(reverseCircle);
            bool washReversal = brushForwardArea * brushReverseArea < 0f &&
                bubbleForwardArea * bubbleReverseArea < 0f &&
                Mathf.Sign(brushForwardArea) == EvaluateCareWashDirection(wideCircle) &&
                Mathf.Sign(bubbleForwardArea) == EvaluateCareWashDirection(wideCircle) &&
                Mathf.Sign(brushReverseArea) == EvaluateCareWashDirection(reverseCircle) &&
                Mathf.Sign(bubbleReverseArea) == EvaluateCareWashDirection(reverseCircle);

            Vector3 rightSwipeEnd = EvaluateCareCombPosition(1f, rightSwipe);
            Vector3 leftSwipeEnd = EvaluateCareCombPosition(1f, leftSwipe);
            float combEndpointSeparation = Vector3.Distance(rightSwipeEnd, leftSwipeEnd);
            bool combReversal = true;
            for (int i = 0; i <= 40; i++)
            {
                float t = i / 40f;
                combReversal &= Vector3.Distance(
                    EvaluateCareCombPosition(t, rightSwipe),
                    EvaluateCareCombPosition(1f - t, leftSwipe)) <= 0.0001f;
            }

            float scoreAuraDelta = EvaluateCareGlowAuraScale(0.25f, highScore).x -
                EvaluateCareGlowAuraScale(0.25f, lowScore).x;
            float durationAuraDelta = EvaluateCareGlowAuraScale(0.25f, longHold).x -
                EvaluateCareGlowAuraScale(0.25f, shortHold).x;
            float scoreLightDelta = EvaluateCareGlowLightIntensity(0.5f, highScore) -
                EvaluateCareGlowLightIntensity(0.5f, lowScore);
            float durationLightDelta = EvaluateCareGlowLightIntensity(0.5f, longHold) -
                EvaluateCareGlowLightIntensity(0.5f, shortHold);
            int scoreMoteDelta = EvaluateCareGlowMoteCount(highScore) -
                EvaluateCareGlowMoteCount(lowScore);
            int durationMoteDelta = EvaluateCareGlowMoteCount(longHold) -
                EvaluateCareGlowMoteCount(shortHold);
            bool glowResponse = scoreAuraDelta >= CareGlowMinimumAuraScaleDelta &&
                durationAuraDelta >= CareGlowMinimumAuraScaleDelta &&
                scoreLightDelta >= CareGlowMinimumLightIntensityDelta &&
                durationLightDelta >= CareGlowMinimumLightIntensityDelta &&
                scoreMoteDelta >= CareGlowMinimumMoteCountDelta &&
                durationMoteDelta >= CareGlowMinimumMoteCountDelta;

            MoonlightGestureSample[] samples =
            {
                leftTap, rightTap, narrowCircle, wideCircle, reverseCircle,
                rightSwipe, leftSwipe, lowScore, highScore, shortHold, longHold,
                minimumInput, maximumInput, maximumTap, malformedInput, default
            };
            bool finiteAndBounded = true;
            for (int i = 0; i <= 40; i++)
            {
                float t = i / 40f;
                foreach (MoonlightGestureSample sample in samples)
                {
                    finiteAndBounded &= CarePointIsFiniteAndBounded(
                            EvaluateCareTowelPosition(t, sample)) &&
                        CarePointIsFiniteAndBounded(
                            EvaluateCareWashBrushPosition(t, sample)) &&
                        CarePointIsFiniteAndBounded(
                            EvaluateCareCombPosition(t, sample)) &&
                        CareScaleIsFiniteAndBounded(
                            EvaluateCareGlowAuraScale(t, sample));
                    Vector3 towelEuler = EvaluateCareTowelEuler(t, sample);
                    Vector3 brushEuler = EvaluateCareWashBrushEuler(t, sample);
                    Vector3 combEuler = EvaluateCareCombEuler(t, sample);
                    float direction = EvaluateCareWashDirection(sample);
                    float radius = EvaluateCareWashRadius(sample);
                    float glowScale = EvaluateCareGlowScaleMultiplier(sample);
                    float light = EvaluateCareGlowLightIntensity(t, sample);
                    finiteAndBounded &= IsFinite(towelEuler) &&
                        Mathf.Abs(towelEuler.x) <= 0.001f &&
                        Mathf.Abs(towelEuler.y) <= 18f && Mathf.Abs(towelEuler.z) <= 7f &&
                        IsFinite(brushEuler) && Mathf.Abs(brushEuler.x) <= 18f &&
                        Mathf.Abs(brushEuler.y) <= 540f && Mathf.Abs(brushEuler.z) <= 64f &&
                        IsFinite(combEuler) && Mathf.Abs(combEuler.x) <= 0.001f &&
                        Mathf.Abs(combEuler.y) <= 18f && Mathf.Abs(combEuler.z) <= 22f &&
                        IsFinite(direction) && Mathf.Abs(direction) == 1f &&
                        IsFinite(radius) && radius >= CareWashMinimumRadius &&
                        radius <= CareWashMaximumRadius &&
                        IsFinite(glowScale) && glowScale >= 0.80f && glowScale <= 1.40f &&
                        IsFinite(light) && light >= 0.32f && light <= 1.30f &&
                        EvaluateCareGlowMoteCount(sample) is >= 2 and <= 6;
                    for (int mote = 0; mote < 6; mote++)
                    {
                        Vector3 motePosition = EvaluateCareGlowMotePosition(mote, t, sample);
                        float moteScale = EvaluateCareGlowMoteScale(mote, t, sample);
                        finiteAndBounded &= CarePointIsFiniteAndBounded(motePosition) &&
                            IsFinite(moteScale) && moteScale >= 0.018f && moteScale <= 0.065f;
                    }
                    for (int bubble = 0; bubble < 5; bubble++)
                    {
                        Vector3 bubblePosition = EvaluateCareWashBubblePosition(
                            bubble, t, sample);
                        float bubbleScale = EvaluateCareWashBubbleScale(bubble, t);
                        finiteAndBounded &= CarePointIsFiniteAndBounded(bubblePosition) &&
                            IsFinite(bubbleScale) && bubbleScale >= 0.015f &&
                            bubbleScale <= 0.080f;
                    }
                }
            }
            finiteAndBounded &= CarePointIsFiniteAndBounded(
                    EvaluateCareTowelPosition(float.NaN, maximumTap)) &&
                CarePointIsFiniteAndBounded(
                    EvaluateCareWashBrushPosition(float.PositiveInfinity, wideCircle)) &&
                CarePointIsFiniteAndBounded(
                    EvaluateCareCombPosition(float.NegativeInfinity, leftSwipe)) &&
                CareScaleIsFiniteAndBounded(
                    EvaluateCareGlowAuraScale(float.NaN, maximumInput)) &&
                IsFinite(EvaluateCareGlowLightIntensity(
                    float.PositiveInfinity, maximumInput));
            bool malformedGlowScalePass = malformedInput.HasSevenFiniteNormalizedPoints &&
                malformedInput.Score == 0f && malformedInput.Duration == 0f &&
                IsFinite(EvaluateCareGlowMoteScale(0, 0.5f, malformedInput)) &&
                IsFinite(EvaluateCareGlowMoteScale(0, 0.5f, default));

            bool sequencePreserved =
                MoonlightSpatialActionZone.CareGestureForStep(0) == MoonlightGestureKind.Tap &&
                MoonlightSpatialActionZone.CareGestureForStep(1) == MoonlightGestureKind.Circle &&
                MoonlightSpatialActionZone.CareGestureForStep(2) == MoonlightGestureKind.Swipe &&
                MoonlightSpatialActionZone.CareGestureForStep(3) == MoonlightGestureKind.Hold;
            bool timingPreserved = Mathf.Approximately(CareFinalPresentationSeconds, 4.6f);
            bool persistentIsolationPolicy =
                !ShouldBindPersistentStation(MoonlightSpatialActionKind.Care, true) &&
                ShouldBindPersistentStation(MoonlightSpatialActionKind.Care, false) &&
                ShouldBindPersistentStation(MoonlightSpatialActionKind.Cook, true);
            detail = $"points={wideCircle.PointCount} " +
                $"prepSeparation={towelSeparation:0.000}/" +
                $"{CarePrepMinimumLandingSeparation:0.00} " +
                $"prepBounds={prepLandingBounds} " +
                $"washDirection={brushForwardArea:0.000}/{brushReverseArea:0.000}," +
                $"{bubbleForwardArea:0.000}/{bubbleReverseArea:0.000} " +
                $"radius={narrowRadius:0.000}/{wideRadius:0.000} delta={radiusDelta:0.000} " +
                $"combReversal={combReversal} endpoints={combEndpointSeparation:0.000} " +
                $"glowAuraDelta={scoreAuraDelta:0.000}/{durationAuraDelta:0.000} " +
                $"lightDelta={scoreLightDelta:0.000}/{durationLightDelta:0.000} " +
                $"moteDelta={scoreMoteDelta}/{durationMoteDelta} " +
                $"finiteBounds={finiteAndBounded} malformedGlow={malformedGlowScalePass} " +
                $"sequence={sequencePreserved} " +
                $"linger={CareFinalPresentationSeconds:0.0}s " +
                $"persistentIsolationPolicy={persistentIsolationPolicy}";
            return wideCircle.HasSevenFiniteNormalizedPoints &&
                reverseCircle.HasSevenFiniteNormalizedPoints &&
                towelSeparation >= CarePrepMinimumLandingSeparation && prepLandingBounds &&
                radiusDelta >= CareWashMinimumRadiusDelta && washReversal &&
                combReversal && combEndpointSeparation >= CareBrushMinimumEndpointSeparation &&
                glowResponse && finiteAndBounded && malformedGlowScalePass &&
                sequencePreserved && timingPreserved && persistentIsolationPolicy;
        }

        static float CareSampleSignedArea(MoonlightGestureSample sample)
        {
            if (!sample.HasSevenFiniteNormalizedPoints) return 1f;
            float area = 0f;
            for (int i = 0; i < MoonlightGestureSample.ResampledPointCount; i++)
            {
                Vector2 from = sample[i];
                Vector2 to = sample[(i + 1) % MoonlightGestureSample.ResampledPointCount];
                area += from.x * to.y - to.x * from.y;
            }
            return area * 0.5f;
        }

        static float CareSampleRadialExtent(MoonlightGestureSample sample)
        {
            if (!sample.HasSevenFiniteNormalizedPoints) return 0.32f;
            Vector2 center = Vector2.zero;
            for (int i = 0; i < MoonlightGestureSample.ResampledPointCount; i++)
                center += sample[i];
            center /= MoonlightGestureSample.ResampledPointCount;
            float extent = 0f;
            for (int i = 0; i < MoonlightGestureSample.ResampledPointCount; i++)
                extent = Mathf.Max(extent, Vector2.Distance(sample[i], center));
            return Mathf.Clamp(extent, 0f, 1.5f);
        }

        static float CareSwipeDirection(MoonlightGestureSample sample)
        {
            if (!sample.HasSevenFiniteNormalizedPoints) return 1f;
            Vector2 displacement = sample.Displacement;
            float primary = Mathf.Abs(displacement.x) >= Mathf.Abs(displacement.y)
                ? displacement.x
                : displacement.y;
            return Mathf.Abs(primary) > 0.0001f ? Mathf.Sign(primary) : 1f;
        }

        static float CareBrushOrbitSignedArea(MoonlightGestureSample sample)
        {
            float area = 0f;
            Vector3 previous = EvaluateCareWashBrushPosition(0f, sample);
            for (int i = 1; i <= 40; i++)
            {
                Vector3 current = EvaluateCareWashBrushPosition(i / 40f, sample);
                area += previous.x * (current.z - 0.02f) -
                    current.x * (previous.z - 0.02f);
                previous = current;
            }
            return area * 0.5f;
        }

        static float CareBubbleOrbitSignedArea(MoonlightGestureSample sample)
        {
            float area = 0f;
            Vector3 previous = EvaluateCareWashBubblePosition(0, 0f, sample);
            for (int i = 1; i <= 40; i++)
            {
                Vector3 current = EvaluateCareWashBubblePosition(0, i / 40f, sample);
                area += previous.x * (current.z - 0.02f) -
                    current.x * (previous.z - 0.02f);
                previous = current;
            }
            return area * 0.5f;
        }

        static MoonlightGestureSample CareTapSample(Vector2 position)
        {
            var points = new Vector2[MoonlightGestureSample.ResampledPointCount];
            for (int i = 0; i < points.Length; i++) points[i] = position;
            return MoonlightGestureSample.Create(0.95f, 0.12f, points);
        }

        static MoonlightGestureSample CareCircleSample(float radius, bool reverse)
        {
            var points = new Vector2[MoonlightGestureSample.ResampledPointCount];
            for (int i = 0; i < points.Length; i++)
            {
                int source = reverse ? points.Length - 1 - i : i;
                float angle = source / (float)(points.Length - 1) * Mathf.PI * 2f;
                points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
            return MoonlightGestureSample.Create(0.95f, 0.80f, points);
        }

        static MoonlightGestureSample CareSwipeSample(bool reverse)
        {
            var points = new Vector2[MoonlightGestureSample.ResampledPointCount];
            for (int i = 0; i < points.Length; i++)
            {
                float t = i / (float)(points.Length - 1);
                points[i] = new Vector2(Mathf.Lerp(-0.72f, 0.72f,
                    reverse ? 1f - t : t), 0f);
            }
            return MoonlightGestureSample.Create(0.95f, 0.35f, points);
        }

        static MoonlightGestureSample CareHoldSample(float score, float duration)
        {
            var points = new Vector2[MoonlightGestureSample.ResampledPointCount];
            return MoonlightGestureSample.Create(score, duration, points);
        }

        static bool CarePointIsFiniteAndBounded(Vector3 point) =>
            IsFinite(point) && Mathf.Abs(point.x) <= 1.05f &&
            point.y >= 0.20f && point.y <= 1.18f && Mathf.Abs(point.z) <= 0.40f;

        static bool CareScaleIsFiniteAndBounded(Vector3 scale) =>
            IsFinite(scale) && scale.x >= 0.25f && scale.x <= 0.60f &&
            scale.y >= 0.01f && scale.y <= 0.03f &&
            scale.z >= 0.30f && scale.z <= 0.72f;

        bool CareWashTransformsMatch()
        {
            if (_careBrush == null || _careBubbles == null || _careBubbles.Length != 5)
                return false;
            if (Vector3.Distance(_careBrush.localPosition,
                    EvaluateCareWashBrushPosition(_careProgress, _gestureSample)) > 0.001f ||
                Quaternion.Angle(_careBrush.localRotation,
                    Quaternion.Euler(EvaluateCareWashBrushEuler(
                        _careProgress, _gestureSample))) > 0.01f)
                return false;
            for (int i = 0; i < _careBubbles.Length; i++)
            {
                if (_careBubbles[i] == null || !_careBubbles[i].gameObject.activeSelf ||
                    Vector3.Distance(_careBubbles[i].localPosition,
                        EvaluateCareWashBubblePosition(i, _careProgress, _gestureSample)) > 0.001f ||
                    Vector3.Distance(_careBubbles[i].localScale,
                        Vector3.one * EvaluateCareWashBubbleScale(i, _careProgress)) > 0.001f)
                    return false;
            }
            return true;
        }

        bool CareGlowTransformsMatch()
        {
            if (_careMirrorAura == null || _careMotes == null || _careMotes.Length != 6 ||
                !_careMirrorAura.gameObject.activeSelf ||
                Vector3.Distance(_careMirrorAura.localScale,
                    EvaluateCareGlowAuraScale(_careProgress, _gestureSample)) > 0.001f ||
                !IsFinite(CareActualGlowLightIntensity) ||
                Mathf.Abs(CareActualGlowLightIntensity -
                    CareExpectedGlowLightIntensity) > 0.001f ||
                CareActualGlowMoteCount != CareExpectedGlowMoteCount)
                return false;
            for (int i = 0; i < _careMotes.Length; i++)
            {
                bool expectedActive = i < CareExpectedGlowMoteCount;
                if (_careMotes[i] == null ||
                    _careMotes[i].gameObject.activeSelf != expectedActive)
                    return false;
                if (!expectedActive) continue;
                if (Vector3.Distance(_careMotes[i].localPosition,
                        EvaluateCareGlowMotePosition(i, _careProgress, _gestureSample)) > 0.001f ||
                    Vector3.Distance(_careMotes[i].localScale,
                        Vector3.one * EvaluateCareGlowMoteScale(
                            i, _careProgress, _gestureSample)) > 0.001f)
                    return false;
            }
            return true;
        }

        int CountActiveCareMotes()
        {
            int count = 0;
            if (_careMotes == null) return count;
            for (int i = 0; i < _careMotes.Length; i++)
                if (_careMotes[i] != null && _careMotes[i].gameObject.activeSelf) count++;
            return count;
        }

        public static bool SingleStepLingerAllowedForQA(MoonlightSpatialActionKind kind,
            int requiredSteps, int currentStep) =>
            kind == MoonlightSpatialActionKind.SleepCuddle && requiredSteps == 1 &&
            currentStep == 0;

        public static string BedtimeLayoutSignatureFor(string state) =>
            NormalizeBedtimeState(state) == "Cuddled"
                ? "bedtime-cuddled-heart-pair"
                : "bedtime-resting-dream-moon";

        static string NormalizeBedtimeState(string state) =>
            state == "Cuddled" ? "Cuddled" : "Resting";

        static bool BedtimePartVisible(BedtimePartSpec spec, string state) =>
            NormalizeBedtimeState(state) == "Cuddled"
                ? spec.CuddledVisible
                : spec.RestingVisible;

        static int BedtimeLayoutHash(string state)
        {
            int hash = 17;
            for (int i = 0; i < BedtimeParts.Length; i++)
            {
                BedtimePartSpec part = BedtimeParts[i];
                if (!BedtimePartVisible(part, state)) continue;
                unchecked
                {
                    hash = hash * 31 + (int)part.Primitive;
                    hash = hash * 31 + part.Position.GetHashCode();
                    hash = hash * 31 + part.Scale.GetHashCode();
                    hash = hash * 31 + part.Euler.GetHashCode();
                }
            }
            return hash;
        }

        public static bool ValidateBedtimeStaticContract(out string detail)
        {
            string[] states = { "Resting", "Cuddled" };
            var signatures = new HashSet<string>();
            var layoutHashes = new HashSet<int>();
            var materialSlots = new HashSet<int>();
            int validVariants = 0;
            int minimumVisible = int.MaxValue;
            int maximumVisible = 0;
            int minimumSemanticShapes = int.MaxValue;
            for (int stateIndex = 0; stateIndex < states.Length; stateIndex++)
            {
                string state = states[stateIndex];
                int visible = 0;
                int semanticShapes = 0;
                for (int i = 0; i < BedtimeParts.Length; i++)
                {
                    BedtimePartSpec part = BedtimeParts[i];
                    materialSlots.Add(part.MaterialSlot);
                    if (!BedtimePartVisible(part, state)) continue;
                    visible++;
                    if (part.Primitive != PrimitiveType.Sphere) semanticShapes++;
                }
                signatures.Add(BedtimeLayoutSignatureFor(state));
                layoutHashes.Add(BedtimeLayoutHash(state));
                minimumVisible = Mathf.Min(minimumVisible, visible);
                maximumVisible = Mathf.Max(maximumVisible, visible);
                minimumSemanticShapes = Mathf.Min(minimumSemanticShapes, semanticShapes);
                if (visible == BedtimeVisibleRendererCount && semanticShapes >= 3)
                    validVariants++;
            }

            bool lingerPolicy = SingleStepLingerAllowedForQA(
                    MoonlightSpatialActionKind.SleepCuddle, 1, 0) &&
                !SingleStepLingerAllowedForQA(MoonlightSpatialActionKind.Feed, 1, 0) &&
                !SingleStepLingerAllowedForQA(MoonlightSpatialActionKind.Play, 1, 0) &&
                Mathf.Approximately(BedtimeLingerSeconds, 2.0f);
            detail = $"variants={validVariants}/{BedtimeVariantCount} " +
                $"allocated={BedtimeParts.Length}/{BedtimeAllocatedRendererBudget} " +
                $"visible={minimumVisible}-{maximumVisible}/{BedtimeVisibleRendererCount} " +
                $"materials={materialSlots.Count}/<={BedtimeMaterialBudget} " +
                $"lights={BedtimeLightBudget} colliders={BedtimeColliderBudget} " +
                $"signatures={signatures.Count}/2 layouts={layoutHashes.Count}/2 " +
                $"semanticShapes>={minimumSemanticShapes} linger={BedtimeLingerSeconds:0.0}s " +
                $"singleStepExclusive={lingerPolicy}";
            return validVariants == BedtimeVariantCount &&
                BedtimeParts.Length == BedtimeAllocatedRendererBudget &&
                materialSlots.Count <= BedtimeMaterialBudget && signatures.Count == 2 &&
                layoutHashes.Count == 2 && lingerPolicy;
        }

        public bool ValidateBedtimeRuntimeContract(string expectedState, out string detail)
        {
            string state = NormalizeBedtimeState(expectedState);
            bool stateMatches = _bedtimeState == state;
            bool signatureMatches = BedtimeLayoutSignatureForQA ==
                BedtimeLayoutSignatureFor(state);
            bool visibilityMatches = BedtimeVisibilityMatches(state);
            int allocatedRenderers = BedtimeAllocatedRendererCountForQA;
            int visibleRenderers = BedtimeVisibleRendererCountForQA;
            int colliders = BedtimeColliderCountForQA;
            bool pass = CurrentKind == MoonlightSpatialActionKind.SleepCuddle && IsVisible &&
                stateMatches && signatureMatches && visibilityMatches &&
                allocatedRenderers == BedtimeAllocatedRendererBudget &&
                visibleRenderers == BedtimeVisibleRendererCount &&
                ActiveRendererCount == BedtimeVisibleRendererCount &&
                BedtimeAllocatedMaterialCountForQA > 0 &&
                BedtimeAllocatedMaterialCountForQA <= BedtimeMaterialBudget &&
                ActiveUniqueMaterialCount > 0 &&
                ActiveUniqueMaterialCount <= BedtimeMaterialBudget &&
                ActiveLightCount == BedtimeLightBudget && colliders == BedtimeColliderBudget;
            detail = $"state={_bedtimeState}/{state} visibleState={visibilityMatches} " +
                $"signature={BedtimeLayoutSignatureForQA}/{BedtimeLayoutSignatureFor(state)} " +
                $"renderers={allocatedRenderers}/{BedtimeAllocatedRendererBudget} allocated," +
                $"{visibleRenderers}/{BedtimeVisibleRendererCount} visible " +
                $"active={ActiveRendererCount} materials={BedtimeAllocatedMaterialCountForQA}/" +
                $"{ActiveUniqueMaterialCount}/<={BedtimeMaterialBudget} " +
                $"lights={ActiveLightCount}/{BedtimeLightBudget} " +
                $"colliders={colliders}/{BedtimeColliderBudget} rootVisible={IsVisible}";
            return pass;
        }

        public bool ValidateLastBedtimeLingerRuntimeContract(string expectedState,
            float toleranceSeconds, out string detail)
        {
            string state = NormalizeBedtimeState(expectedState);
            float tolerance = Mathf.Clamp(toleranceSeconds, 0.05f, 0.50f);
            bool entered = LastBedtimeLingerStartedAtSecondsForQA > 0f &&
                LastBedtimeLingerEndedAtSecondsForQA >=
                    LastBedtimeLingerStartedAtSecondsForQA;
            bool requested = Mathf.Abs(LastBedtimeLingerRequestedSecondsForQA -
                BedtimeLingerSeconds) <= 0.01f;
            bool observed = Mathf.Abs(LastBedtimeLingerObservedSecondsForQA -
                BedtimeLingerSeconds) <= tolerance;
            bool cleaned = !IsVisible && !IsLingering && ActiveRendererCount == 0 &&
                ActiveUniqueMaterialCount == 0 && ActiveLightCount == 0;
            detail = $"state={LastBedtimeLingerStateForQA}/{state} entered={entered} " +
                $"natural={LastBedtimeLingerCompletedNaturallyForQA} " +
                $"requested={LastBedtimeLingerRequestedSecondsForQA:0.000}s " +
                $"observed={LastBedtimeLingerObservedSecondsForQA:0.000}s " +
                $"expected={BedtimeLingerSeconds:0.000}s tolerance={tolerance:0.000}s " +
                $"cleaned={cleaned}";
            return LastBedtimeLingerStateForQA == state && entered && requested && observed &&
                LastBedtimeLingerCompletedNaturallyForQA && cleaned;
        }

        int CountVisibleBedtimeParts()
        {
            if (_bedtimeParts == null) return 0;
            int count = 0;
            for (int i = 0; i < _bedtimeParts.Length; i++)
            {
                Transform part = _bedtimeParts[i];
                if (part != null && part.gameObject.activeSelf) count++;
            }
            return count;
        }

        bool BedtimeVisibilityMatches(string state)
        {
            if (_bedtimeParts == null || _bedtimeParts.Length != BedtimeParts.Length)
                return false;
            for (int i = 0; i < BedtimeParts.Length; i++)
            {
                if (_bedtimeParts[i] == null ||
                    _bedtimeParts[i].gameObject.activeSelf !=
                        BedtimePartVisible(BedtimeParts[i], state))
                    return false;
            }
            return true;
        }

        void BuildBedtimeStage(string state)
        {
            _bedtimeState = NormalizeBedtimeState(state);
            _bedtimeParts = new Transform[BedtimeParts.Length];
            for (int i = 0; i < BedtimeParts.Length; i++)
                _bedtimeParts[i] = BuildBedtimePart(BedtimeParts[i]);

            LastBedtimeLingerRequestedSecondsForQA = 0f;
            LastBedtimeLingerStartedAtSecondsForQA = 0f;
            LastBedtimeLingerEndedAtSecondsForQA = 0f;
            LastBedtimeLingerObservedSecondsForQA = 0f;
            LastBedtimeLingerCompletedNaturallyForQA = false;
            LastBedtimeLingerStateForQA = "";
            AddActivityLight(_bedtimeState == "Cuddled"
                ? new Color(1f, 0.58f, 0.74f)
                : new Color(0.58f, 0.72f, 1f));
            _activityLight.gameObject.name = "BedtimeSpotlight";
            Debug.Log($"[MoonlightActivityStage] bedtime-stage state={_bedtimeState} " +
                $"signature={BedtimeLayoutSignatureForQA} " +
                $"renderers={BedtimeParts.Length}/{BedtimeAllocatedRendererBudget} allocated " +
                $"visible={BedtimeVisibleRendererCount} materials={_materials.Count}/" +
                $"{BedtimeMaterialBudget} lights=1 colliders=0 " +
                "marker=MOONLIGHT_BEDTIME_STAGE_READY");
        }

        Transform BuildBedtimePart(BedtimePartSpec spec)
        {
            var part = new GameObject(spec.Name);
            part.transform.SetParent(_root.transform, false);
            part.transform.localPosition = spec.Position;
            part.transform.localScale = spec.Scale;
            part.transform.localRotation = Quaternion.Euler(spec.Euler);
            var meshFilter = part.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = BedtimeMeshFor(spec.Primitive);
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = NewMaterial(spec.Color, spec.Emission, false,
                ResolveSurfaceProfile(spec.Name, spec.Emission, false));
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            _renderers.Add(renderer);
            part.SetActive(BedtimePartVisible(spec, _bedtimeState));
            return part.transform;
        }

        static Mesh BedtimeMeshFor(PrimitiveType primitive)
        {
            if (BedtimePrimitiveMeshes.TryGetValue(primitive, out Mesh cachedMesh) &&
                cachedMesh != null)
                return cachedMesh;
            GameObject source = GameObject.CreatePrimitive(primitive);
            source.SetActive(false);
            Mesh mesh = source.GetComponent<MeshFilter>().sharedMesh;
            BedtimePrimitiveMeshes[primitive] = mesh;
            Object.Destroy(source);
            return mesh;
        }

        void UpdateBedtime(string state, float t)
        {
            if (_bedtimeParts == null || _bedtimeParts.Length != BedtimeParts.Length) return;
            _bedtimeState = NormalizeBedtimeState(state);
            _bedtimeProgress = Mathf.Clamp01(t);
            for (int i = 0; i < BedtimeParts.Length; i++)
            {
                BedtimePartSpec spec = BedtimeParts[i];
                Transform part = _bedtimeParts[i];
                if (part == null) continue;
                part.gameObject.SetActive(BedtimePartVisible(spec, _bedtimeState));
                part.localPosition = spec.Position;
                part.localScale = spec.Scale;
                part.localRotation = Quaternion.Euler(spec.Euler);
            }

            float envelope = Mathf.Sin(_bedtimeProgress * Mathf.PI);
            Transform blanket = _bedtimeParts[1];
            blanket.localScale = Vector3.Scale(BedtimeParts[1].Scale,
                new Vector3(1f, 1f + envelope * 0.10f, 1f));
            if (_bedtimeState == "Resting")
            {
                Transform pillow = _bedtimeParts[2];
                pillow.localPosition += Vector3.down * Mathf.SmoothStep(0f, 0.035f,
                    _bedtimeProgress);
                Transform moon = _bedtimeParts[3];
                moon.localPosition += Vector3.up * Mathf.Sin(_bedtimeProgress * Mathf.PI * 2f) *
                    0.035f;
                moon.localRotation = Quaternion.Euler(BedtimeParts[3].Euler +
                    new Vector3(0f, _bedtimeProgress * 28f, 0f));
                Transform star = _bedtimeParts[4];
                star.localScale = BedtimeParts[4].Scale * (1f + envelope * 0.22f);
            }
            else
            {
                float heartbeat = 1f + Mathf.Sin(_bedtimeProgress * Mathf.PI * 4f) *
                    envelope * 0.10f;
                for (int i = 5; i <= 7; i++)
                    _bedtimeParts[i].localScale = BedtimeParts[i].Scale * heartbeat;
                _bedtimeParts[5].localPosition += Vector3.right * envelope * 0.025f;
                _bedtimeParts[6].localPosition += Vector3.left * envelope * 0.025f;
                _bedtimeParts[7].localPosition += Vector3.up * envelope * 0.025f;
            }
        }

        void BuildCareStage()
        {
            bool hasPersistentSet = BindPersistentActivitySet(MoonlightSpatialActionKind.Care,
                "MOONLIGHT_AUTHORED_CARE_STATION_READY", out Transform careStationRoot,
                out int rendererCount, out int materialCount, out int colliderCount,
                out int lightCount, out Vector3 boundsSize);
            UsesProceduralCareStationFallback = !hasPersistentSet ||
                _persistentStation.UsesProceduralFallback;
            if (hasPersistentSet)
            {
                foreach (var collider in careStationRoot.GetComponentsInChildren<Collider>(true))
                    collider.enabled = false;
                foreach (var light in careStationRoot.GetComponentsInChildren<Light>(true))
                    light.enabled = false;
                if (UsesProceduralCareStationFallback)
                {
                    CareStationVisualSource = "persistent-procedural-fallback";
                    CareStationSourceQAMarker = _persistentStation.VisualSourceQAMarker;
                }
                else
                {
                    _authoredCareStation = careStationRoot;
                    CareStationVisualSource = "authored";
                    CareStationSourceQAMarker = "MOONLIGHT_AUTHORED_CARE_STATION_READY";
                }
            }
            if (!hasPersistentSet)
            {
                Primitive(PrimitiveType.Cube, "CareStationBase", new Vector3(0f, 0.08f, 0f),
                    new Vector3(1.62f, 0.16f, 0.82f), new Color(0.34f, 0.25f, 0.22f), 0.02f);
                Primitive(PrimitiveType.Cube, "CareStationWoodTop", new Vector3(0f, 0.18f, 0f),
                    new Vector3(1.52f, 0.055f, 0.74f), new Color(0.58f, 0.42f, 0.34f), 0.03f);
                Primitive(PrimitiveType.Cube, "CareTowelMat", new Vector3(0f, 0.225f, 0f),
                    new Vector3(1.42f, 0.025f, 0.64f), new Color(0.56f, 0.76f, 0.72f), 0.02f);
                Primitive(PrimitiveType.Cube, "CareStationBackRail", new Vector3(0f, 0.39f, 0.34f),
                    new Vector3(1.42f, 0.30f, 0.035f), new Color(0.72f, 0.76f, 0.78f), 0.04f);
                rendererCount = 4;
                materialCount = 4;
                colliderCount = 0;
                lightCount = 0;
                boundsSize = new Vector3(1.62f, 0.54f, 0.82f);
                CareStationVisualSource = "stage-procedural-fallback";
                CareStationSourceQAMarker = "MOONLIGHT_CARE_STAGE_PROCEDURAL_FALLBACK_READY";
            }
            CareStationRendererCount = rendererCount;
            CareStationMaterialCount = materialCount;
            CareStationColliderCount = colliderCount;
            CareStationLightCount = lightCount;
            CareStationBoundsSize = boundsSize;
            if (HasAuthoredCareStation)
            {
                AuthoredCareStationRendererCount = rendererCount;
                AuthoredCareStationMaterialCount = materialCount;
                AuthoredCareStationColliderCount = colliderCount;
                AuthoredCareStationLightCount = lightCount;
                AuthoredCareStationBoundsSize = boundsSize;
            }

            _careTowelTray = Primitive(PrimitiveType.Cylinder, "care-towel-tray",
                new Vector3(-0.58f, 0.285f, 0.02f), new Vector3(0.31f, 0.025f, 0.24f),
                new Color(0.72f, 0.78f, 0.80f), 0.05f);
            _careTowel = Primitive(PrimitiveType.Cube, "CareTowel",
                new Vector3(-0.58f, 0.335f, 0.02f), new Vector3(0.46f, 0.065f, 0.31f),
                new Color(0.82f, 0.94f, 0.90f), 0.02f);

            _careProps = new[]
            {
                Primitive(PrimitiveType.Sphere, "CareBasin", new Vector3(0f, 0.36f, 0.02f),
                    new Vector3(0.47f, 0.17f, 0.39f), new Color(0.88f, 0.90f, 0.92f), 0.04f),
                Primitive(PrimitiveType.Cylinder, "CareBasinRim", new Vector3(0f, 0.48f, 0.02f),
                    new Vector3(0.48f, 0.025f, 0.40f), new Color(0.76f, 0.84f, 0.86f), 0.05f),
                Primitive(PrimitiveType.Cylinder, "CareBasinWater", new Vector3(0f, 0.505f, 0.02f),
                    new Vector3(0.38f, 0.012f, 0.30f), new Color(0.46f, 0.84f, 0.92f, 0.72f),
                    0.20f, true),
            };

            _careBrush = Primitive(PrimitiveType.Capsule, "care-brush",
                new Vector3(0.52f, 0.42f, -0.13f), new Vector3(0.055f, 0.28f, 0.055f),
                new Color(0.62f, 0.35f, 0.27f), 0.03f);
            _careBrush.localRotation = Quaternion.Euler(0f, 0f, 68f);
            var brushPad = Primitive(PrimitiveType.Cube, "CareBrushPad", Vector3.zero,
                new Vector3(0.17f, 0.055f, 0.12f), new Color(0.94f, 0.72f, 0.64f), 0.02f);
            brushPad.SetParent(_careBrush, false);
            brushPad.localPosition = new Vector3(0f, 0.48f, 0f);
            brushPad.localRotation = Quaternion.identity;

            _careComb = Primitive(PrimitiveType.Cube, "care-comb",
                new Vector3(0.55f, 0.30f, 0.17f), new Vector3(0.34f, 0.045f, 0.08f),
                new Color(0.82f, 0.58f, 0.36f), 0.04f);
            _careComb.localRotation = Quaternion.Euler(0f, 18f, -8f);
            for (int i = 0; i < 4; i++)
            {
                var tooth = Primitive(PrimitiveType.Cube, $"CareCombTooth-{i + 1}", Vector3.zero,
                    new Vector3(0.025f, 0.16f, 0.045f), new Color(0.82f, 0.58f, 0.36f), 0.04f);
                tooth.SetParent(_careComb, false);
                tooth.localPosition = new Vector3(-0.34f + i * 0.23f, -1.15f, 0f);
                tooth.localRotation = Quaternion.identity;
            }

            _careMirror = Primitive(PrimitiveType.Cylinder, "care-mirror",
                new Vector3(0.59f, 0.76f, 0.23f), new Vector3(0.27f, 0.025f, 0.34f),
                new Color(0.64f, 0.84f, 0.90f), 0.12f);
            _careMirror.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var mirrorFrame = Primitive(PrimitiveType.Cylinder, "CareMirrorFrame",
                new Vector3(0.59f, 0.76f, 0.255f), new Vector3(0.32f, 0.035f, 0.39f),
                new Color(0.82f, 0.72f, 0.48f), 0.06f);
            mirrorFrame.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var mirrorStand = Primitive(PrimitiveType.Capsule, "CareMirrorStand",
                new Vector3(0.59f, 0.47f, 0.27f), new Vector3(0.035f, 0.24f, 0.035f),
                new Color(0.82f, 0.72f, 0.48f), 0.05f);
            mirrorStand.localRotation = Quaternion.Euler(0f, 0f, -4f);
            _careMirrorAura = Primitive(PrimitiveType.Cylinder, "CareMirrorAura",
                new Vector3(0.59f, 0.76f, 0.285f), new Vector3(0.38f, 0.018f, 0.46f),
                new Color(0.72f, 0.90f, 1f, 0.48f), 0.34f, true);
            _careMirrorAura.localRotation = Quaternion.Euler(90f, 0f, 0f);
            _careMirrorAura.gameObject.SetActive(false);

            _careBubbles = new Transform[5];
            for (int i = 0; i < _careBubbles.Length; i++)
            {
                _careBubbles[i] = Primitive(PrimitiveType.Sphere, $"CareBubble-{i + 1}",
                    Vector3.zero, Vector3.one * 0.07f,
                    i % 2 == 0 ? new Color(0.76f, 0.94f, 1f, 0.62f) : new Color(1f, 0.84f, 0.92f, 0.58f),
                    0.24f, true);
                _careBubbles[i].gameObject.SetActive(false);
            }

            _careMotes = new Transform[6];
            for (int i = 0; i < _careMotes.Length; i++)
            {
                _careMotes[i] = Primitive(PrimitiveType.Sphere, $"CareMirrorMote-{i + 1}",
                    Vector3.zero, Vector3.one * 0.045f,
                    i % 2 == 0 ? new Color(1f, 0.88f, 0.48f, 0.82f) : new Color(0.66f, 0.88f, 1f, 0.76f),
                    0.36f, true);
                _careMotes[i].gameObject.SetActive(false);
            }

            AddActivityLight(new Color(0.80f, 0.91f, 0.94f));
            _activityLight.gameObject.name = "CareSpotlight";
            Debug.Log($"[MoonlightActivityStage] care-stage persistent={hasPersistentSet} " +
                $"source={CareStationVisualSource} authored={HasAuthoredCareStation} " +
                $"renderers={CareStationRendererCount}/{CareStationRendererBudget} " +
                $"materials={CareStationMaterialCount}/{CareStationMaterialBudget} " +
                $"contacts={_careTowelTray.name},{_careBrush.name},{_careComb.name},{_careMirror.name} " +
                $"steps=4 linger=4.6s sourceMarker={CareStationSourceQAMarker} " +
                "marker=MOONLIGHT_CARE_STAGE_READY");
        }

        void UpdateCare(float t)
        {
            if (_careTowelTray == null || _careTowel == null || _careBrush == null ||
                _careComb == null || _careMirror == null || _careBubbles == null || _careMotes == null)
                return;

            int step = Mathf.Clamp(CurrentStep, 0, 3);
            _careProgress = t;

            _careTowelTray.localScale = new Vector3(0.31f, 0.025f, 0.24f) *
                (step == 0 ? 1f + Mathf.Sin(t * Mathf.PI) * 0.08f : 1f);
            _careTowel.localPosition = step == 0
                ? EvaluateCareTowelPosition(t, _gestureSample)
                : new Vector3(-0.58f, 0.335f, 0.02f);
            _careTowel.localRotation = step == 0
                ? Quaternion.Euler(EvaluateCareTowelEuler(t, _gestureSample))
                : Quaternion.identity;

            if (step == 1)
            {
                _careBrush.localPosition = EvaluateCareWashBrushPosition(t, _gestureSample);
                _careBrush.localRotation = Quaternion.Euler(
                    EvaluateCareWashBrushEuler(t, _gestureSample));
            }
            else
            {
                _careBrush.localPosition = new Vector3(0.52f, 0.42f, -0.13f);
                _careBrush.localRotation = Quaternion.Euler(0f, 0f, 68f);
            }

            if (step == 2)
            {
                _careComb.localPosition = EvaluateCareCombPosition(t, _gestureSample);
                _careComb.localRotation = Quaternion.Euler(
                    EvaluateCareCombEuler(t, _gestureSample));
            }
            else
            {
                _careComb.localPosition = new Vector3(0.55f, 0.30f, 0.17f);
                _careComb.localRotation = Quaternion.Euler(0f, 18f, -8f);
            }

            for (int i = 0; i < _careBubbles.Length; i++)
            {
                bool showBubble = step == 1;
                _careBubbles[i].gameObject.SetActive(showBubble);
                if (!showBubble) continue;
                _careBubbles[i].localPosition = EvaluateCareWashBubblePosition(
                    i, t, _gestureSample);
                _careBubbles[i].localScale = Vector3.one *
                    EvaluateCareWashBubbleScale(i, t);
            }

            bool glow = step == 3;
            _careMirrorAura.gameObject.SetActive(glow);
            if (glow)
            {
                _careMirrorAura.localScale = EvaluateCareGlowAuraScale(t, _gestureSample);
                _careMirror.localScale = new Vector3(0.27f, 0.025f, 0.34f) *
                    (1f + Mathf.Sin(t * Mathf.PI * 3f) * 0.04f);
            }
            else
            {
                _careMirror.localScale = new Vector3(0.27f, 0.025f, 0.34f);
            }

            for (int i = 0; i < _careMotes.Length; i++)
            {
                bool showMote = glow && i < EvaluateCareGlowMoteCount(_gestureSample);
                _careMotes[i].gameObject.SetActive(showMote);
                if (!showMote) continue;
                _careMotes[i].localPosition = EvaluateCareGlowMotePosition(
                    i, t, _gestureSample);
                _careMotes[i].localScale = Vector3.one *
                    EvaluateCareGlowMoteScale(i, t, _gestureSample);
            }
        }

        Transform Primitive(PrimitiveType type, string name, Vector3 localPosition,
            Vector3 localScale, Color color, float emission, bool transparent = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(_root.transform, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            var collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                Destroy(collider);
            }
            var renderer = go.GetComponent<Renderer>();
            ActivitySurfaceProfile surface = ResolveSurfaceProfile(name, emission, transparent);
            renderer.sharedMaterial = NewMaterial(color, emission, transparent, surface);
            renderer.shadowCastingMode = transparent
                ? UnityEngine.Rendering.ShadowCastingMode.Off
                : UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = !transparent;
            _renderers.Add(renderer);
            return go.transform;
        }

        Material NewMaterial(Color color, float emission, bool transparent,
            ActivitySurfaceProfile surface)
        {
            var key = new MaterialKey(color, emission, transparent, surface);
            if (_materialCache.TryGetValue(key, out var cachedMaterial)) return cachedMaterial;

            var shader = Shader.Find(transparent ? "Sprites/Default" : "Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Sprites/Default");
            var material = new Material(shader)
            {
                name = $"MoonlightActivity_{surface}",
                color = color
            };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            GetSurfaceResponse(surface, out float smoothness, out float metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_EmissionColor") && emission > 0f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emission);
            }
            _materials.Add(material);
            _materialCache.Add(key, material);
            _configuredSurfaceProfiles.Add(surface);
            return material;
        }

        static ActivitySurfaceProfile ResolveSurfaceProfile(string name, float emission,
            bool transparent)
        {
            string normalized = name.ToLowerInvariant();
            if (transparent || emission >= 0.18f || normalized.Contains("spark") ||
                normalized.Contains("mote") || normalized.Contains("magic"))
                return ActivitySurfaceProfile.Magic;
            if (normalized.Contains("window") || normalized.Contains("mirror"))
                return ActivitySurfaceProfile.Glass;
            if (normalized.Contains("cloth") || normalized.Contains("ribbon") ||
                normalized.Contains("flag") || normalized.Contains("bookmark") ||
                normalized.Contains("cover") || normalized.Contains("towel"))
                return ActivitySurfaceProfile.Fabric;
            if (normalized.Contains("whisk") || normalized.Contains("tray") ||
                normalized.Contains("platter") || normalized.Contains("handle") ||
                normalized.Contains("spoon") || normalized.Contains("oven") ||
                normalized.Contains("wateringcan") || normalized.Contains("medal") ||
                normalized.Contains("pole"))
                return ActivitySurfaceProfile.Metal;
            if (normalized.Contains("bowl") || normalized.Contains("cup") ||
                normalized.Contains("pot") || normalized.Contains("pedestal") ||
                normalized.Contains("ball") || normalized.Contains("basin"))
                return ActivitySurfaceProfile.Ceramic;
            if (normalized.Contains("counter") || normalized.Contains("rolling") ||
                normalized.Contains("bench") || normalized.Contains("planter") ||
                normalized.Contains("block") || normalized.Contains("spine") ||
                normalized.Contains("brush") || normalized.Contains("comb"))
                return ActivitySurfaceProfile.Wood;
            return ActivitySurfaceProfile.Matte;
        }

        static void GetSurfaceResponse(ActivitySurfaceProfile surface,
            out float smoothness, out float metallic)
        {
            smoothness = surface switch
            {
                ActivitySurfaceProfile.Fabric => 0.08f,
                ActivitySurfaceProfile.Wood => 0.26f,
                ActivitySurfaceProfile.Ceramic => 0.58f,
                ActivitySurfaceProfile.Metal => 0.72f,
                ActivitySurfaceProfile.Glass => 0.82f,
                ActivitySurfaceProfile.Magic => 0.48f,
                _ => 0.18f
            };
            metallic = surface switch
            {
                ActivitySurfaceProfile.Metal => 0.70f,
                ActivitySurfaceProfile.Glass => 0.08f,
                ActivitySurfaceProfile.Magic => 0.05f,
                _ => 0f
            };
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

        void AddActivityLight(Color color)
        {
            var lightObject = new GameObject("ActivityLight");
            lightObject.transform.SetParent(_root.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, 1.55f, -1.10f);
            lightObject.transform.localRotation = Quaternion.LookRotation(
                new Vector3(0f, -1.10f, 1.15f).normalized, Vector3.up);
            _activityLight = lightObject.AddComponent<Light>();
            _activityLight.type = LightType.Spot;
            _activityLight.color = color;
            _activityLight.range = ActivityLightRange;
            _activityLight.spotAngle = ActivityLightSpotAngle;
            _activityLight.intensity = ActivityLightBaseIntensity;
            _activityLight.shadows = LightShadows.Soft;
            _activityLight.shadowResolution = UnityEngine.Rendering.LightShadowResolution.Low;
            _activityLight.shadowBias = 0.06f;
            _activityLight.shadowNormalBias = 0.35f;
        }

        void SetActive(Transform[] transforms, bool active)
        {
            if (transforms == null) return;
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i] != null) transforms[i].gameObject.SetActive(active);
        }

        static void SetPlayRendererVisible(Transform transform, bool visible)
        {
            if (transform == null) return;
            // Preserve the retained Play material set while hiding step-specific props.
            transform.gameObject.SetActive(true);
            Renderer renderer = transform.GetComponent<Renderer>();
            if (renderer != null) renderer.forceRenderingOff = !visible;
        }

        static void SetPlayRenderersVisible(Transform[] transforms, bool visible)
        {
            if (transforms == null) return;
            for (int i = 0; i < transforms.Length; i++)
                SetPlayRendererVisible(transforms[i], visible);
        }

        void UpdateActivityCounts()
        {
            EnsureMaterialIdCapacity(_materials.Count + _renderers.Count);
            ActiveRendererCount = 0;
            ActiveUniqueMaterialCount = 0;

            for (int i = 0; i < _renderers.Count; i++)
            {
                var renderer = _renderers[i];
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;

                ActiveRendererCount++;
                _sharedMaterialBuffer.Clear();
                renderer.GetSharedMaterials(_sharedMaterialBuffer);
                for (int materialIndex = 0; materialIndex < _sharedMaterialBuffer.Count;
                     materialIndex++)
                {
                    var material = _sharedMaterialBuffer[materialIndex];
                    if (material == null) continue;

                    int materialId = material.GetInstanceID();
                    bool seen = false;
                    for (int idIndex = 0; idIndex < ActiveUniqueMaterialCount; idIndex++)
                    {
                        if (_activeMaterialIds[idIndex] != materialId) continue;
                        seen = true;
                        break;
                    }

                    if (seen) continue;
                    EnsureMaterialIdCapacity(ActiveUniqueMaterialCount + 1);
                    _activeMaterialIds[ActiveUniqueMaterialCount] = materialId;
                    ActiveUniqueMaterialCount++;
                }
            }

            ActiveLightCount = 0;
            for (int i = 0; i < _stageLights.Count; i++)
            {
                var light = _stageLights[i];
                if (light != null && light.enabled && light.gameObject.activeInHierarchy)
                    ActiveLightCount++;
            }
        }

        void RefreshStageLights()
        {
            _stageLights.Clear();
            AddStageLights(_root != null ? _root.transform : null);
            if (_persistentStation != null && _persistentStation.VisualRoot != null &&
                !_persistentStation.VisualRoot.IsChildOf(_root.transform))
                AddStageLights(_persistentStation.VisualRoot);
        }

        void AddStageLights(Transform root)
        {
            if (root == null) return;
            _lightBuffer.Clear();
            root.GetComponentsInChildren(true, _lightBuffer);
            for (int i = 0; i < _lightBuffer.Count; i++)
            {
                var light = _lightBuffer[i];
                if (light != null && !_stageLights.Contains(light))
                    _stageLights.Add(light);
            }
        }

        void EnsureMaterialIdCapacity(int required)
        {
            if (_activeMaterialIds.Length >= required) return;
            int capacity = Mathf.NextPowerOfTwo(Mathf.Max(8, required));
            System.Array.Resize(ref _activeMaterialIds, capacity);
        }

        readonly struct MaterialKey : System.IEquatable<MaterialKey>
        {
            readonly Color32 _color;
            readonly int _emission;
            readonly bool _transparent;
            readonly ActivitySurfaceProfile _surface;

            public MaterialKey(Color color, float emission, bool transparent,
                ActivitySurfaceProfile surface)
            {
                _color = color;
                _emission = Mathf.RoundToInt(emission * 1000f);
                _transparent = transparent;
                _surface = surface;
            }

            public bool Equals(MaterialKey other)
            {
                return _color.Equals(other._color) && _emission == other._emission &&
                    _transparent == other._transparent && _surface == other._surface;
            }

            public override bool Equals(object obj)
            {
                return obj is MaterialKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = _color.GetHashCode();
                    hash = (hash * 397) ^ _emission;
                    hash = (hash * 397) ^ (_transparent ? 1 : 0);
                    hash = (hash * 397) ^ (int)_surface;
                    return hash;
                }
            }
        }

        void OnDisable() => End();
        void OnDestroy() => End();
    }
}
