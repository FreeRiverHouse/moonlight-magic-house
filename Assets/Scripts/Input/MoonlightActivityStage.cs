using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public sealed class MoonlightActivityStage : MonoBehaviour
    {
        const string MagicFlowerResourcePath = "Models/Props/Garden/MagicFlowerBloom";
        const int GardenMagicFlowerRequiredInstances = 5;
        const int GardenMagicFlowerMaxRenderers = 10;
        const float ActivityLightRange = 3.2f;
        const float ActivityLightSpotAngle = 72f;
        const float ActivityLightBaseIntensity = 0.32f;
        const float ActivityLightPulseIntensity = 0.53f;
        const float CareFinalLingerSeconds = 4.6f;
        const float BakeLoadStart = 0.08f;
        const float BakeLoadEnd = 0.32f;
        const float BakeDoorCloseStart = 0.34f;
        const float BakeDoorCloseEnd = 0.46f;
        const float BakeDoorReopenStart = 0.58f;
        const float BakeDoorReopenEnd = 0.66f;
        const float BakeExtractStart = 0.68f;
        const float BakeExtractEnd = 0.92f;
        public const float PlayMinimumThrowExtent = 1.05f;
        public const float PlayMaximumThrowExtent = 2.30f;
        public const float PlayMinimumJumpExtent = 0.80f;
        public const float PlayMaximumJumpExtent = 1.80f;
        public const float PlayMinimumJumpHeight = 0.72f;
        public const float PlayMaximumJumpHeight = 1.18f;
        public const float PlayCatchContactProgress = 0.38f;
        public const int RequiredAuthoritativePlayBallCount = 1;
        public static readonly Vector3 PlayCatchPoint = new(0.94f, 0.54f, -0.46f);
        public const int CookPhaseCount = 4;
        public const int CookRequiredPhaseMask = (1 << CookPhaseCount) - 1;
        public const int CookRendererBudget = 36;
        public const int CookMaterialBudget = 24;
        public const int CookLightBudget = 1;
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

        readonly List<Material> _materials = new();
        readonly Dictionary<MaterialKey, Material> _materialCache = new();
        readonly HashSet<ActivitySurfaceProfile> _configuredSurfaceProfiles = new();
        readonly List<Renderer> _renderers = new();
        readonly List<Material> _sharedMaterialBuffer = new();
        readonly List<Light> _stageLights = new();
        readonly List<Light> _lightBuffer = new();
        readonly HashSet<int> _gardenMagicFlowerMaterialIds = new();
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
        Transform _ball;
        Transform[] _blocks;
        Transform[] _playProps;
        Transform[] _starDetails;
        Transform[] _pathMarkers;
        Transform[] _celebrationStars;
        Transform[] _playArches;
        Transform[] _podiumProps;
        Transform _authoredPlayArena;
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
        TrailRenderer _ballTrail;
        Light _activityLight;
        MoonlightActivityStation _persistentStation;
        Coroutine _lingerRoutine;
        float _lingerUntil;
        bool _applyPersistentCompletionOnEnd;
        Vector3 _center;
        int _requiredSteps = 1;
        float _playProgress;
        MoonlightGestureSample _gestureSample;

        public bool IsVisible => _root != null;
        public bool IsLingering { get; private set; }
        public float LingerSecondsRemaining => IsLingering
            ? Mathf.Max(0f, _lingerUntil - Time.time)
            : 0f;
        public MoonlightSpatialActionKind CurrentKind { get; private set; }
        public int CurrentStep { get; private set; }
        public int ActiveRendererCount { get; private set; }
        public int ActiveUniqueMaterialCount { get; private set; }
        public int ActiveLightCount { get; private set; }
        public MoonlightGestureSample ActiveGestureSample => _gestureSample;
        public Vector3 PlayBallLocalPosition => _ball != null
            ? _ball.localPosition
            : new Vector3(float.NaN, float.NaN, float.NaN);
        public int AuthoritativePlayBallCount => CountPlayObjectsNamed("StarBall");
        public int AuthoritativePlayTrailCount => CurrentKind == MoonlightSpatialActionKind.Play &&
            _root != null ? _root.GetComponentsInChildren<TrailRenderer>(true).Length : 0;
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
        public string CookBudgetEvidence =>
            $"renderers={ActiveRendererCount}/{CookRendererBudget} " +
            $"materials={ActiveUniqueMaterialCount}/{CookMaterialBudget} " +
            $"lights={ActiveLightCount}/{CookLightBudget}";
        public string CookPhaseQAMarker =>
            CookCurrentPhaseMotionReady && CookCurrentPhaseStateReady && CookBudgetReady
                ? CookPhaseReadyMarker(CurrentStep)
                : CookChoreographyIncompleteMarker;

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
                $"budgets={CookRendererBudget}r/{CookMaterialBudget}m/{CookLightBudget}l";
            return pass;
        }
        public bool HasAuthoredPlayArena => _authoredPlayArena != null;
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
        public bool HasAuthoredReadingNook => _authoredReadingNook != null;
        public int AuthoredReadingNookRendererCount { get; private set; }
        public int AuthoredReadingNookMaterialCount { get; private set; }
        public int AuthoredReadingNookColliderCount { get; private set; }
        public int AuthoredReadingNookLightCount { get; private set; }
        public Vector3 AuthoredReadingNookBoundsSize { get; private set; }
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

        public void Begin(MoonlightSpatialActionKind kind)
        {
            Begin(kind, 0, 1);
        }

        public void Begin(MoonlightSpatialActionKind kind, int stepIndex, int requiredSteps)
            => Begin(kind, stepIndex, requiredSteps,
                MoonlightGestureSample.Synthetic(MoonlightGestureKind.Swipe, 1f));

        public void Begin(MoonlightSpatialActionKind kind, int stepIndex, int requiredSteps,
            MoonlightGestureSample gestureSample)
        {
            End();
            CurrentKind = kind;
            _gestureSample = gestureSample;
            _requiredSteps = kind is MoonlightSpatialActionKind.Cook or MoonlightSpatialActionKind.Play or
                MoonlightSpatialActionKind.Garden or MoonlightSpatialActionKind.Read or
                MoonlightSpatialActionKind.Care
                ? Mathf.Max(4, requiredSteps)
                : Mathf.Max(1, requiredSteps);
            CurrentStep = Mathf.Clamp(stepIndex, 0, _requiredSteps - 1);
            _root = new GameObject($"ActivityStage-{kind}");
            _root.transform.SetParent(null, true);
            _persistentStation = MoonlightActivityStation.FindNearestActive(kind, transform.position);
            if (_persistentStation != null && CurrentStep == 0)
                _persistentStation.ResetCompletionState();
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

            RefreshStageLights();
            UpdateStage(kind, 0f);
        }

        public bool LingerFinalState(float seconds)
        {
            if (_root == null || _requiredSteps <= 1 || CurrentStep != _requiredSteps - 1)
                return false;

            if (CurrentKind == MoonlightSpatialActionKind.Care)
                seconds = CareFinalLingerSeconds;

            if (_lingerRoutine != null)
                StopCoroutine(_lingerRoutine);

            UpdateStage(CurrentKind, 1f);
            _applyPersistentCompletionOnEnd = true;
            IsLingering = true;
            _lingerUntil = Time.time + Mathf.Max(0.5f, seconds);
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
            End();
        }

        public void UpdateStage(MoonlightSpatialActionKind kind, float t)
        {
            if (_root == null) return;
            t = Mathf.Clamp01(t);
            if (CurrentKind == MoonlightSpatialActionKind.Cook) UpdateCook(t);
            else if (CurrentKind == MoonlightSpatialActionKind.Play) UpdatePlay(t);
            else if (CurrentKind == MoonlightSpatialActionKind.Garden) UpdateGarden(t);
            else if (CurrentKind == MoonlightSpatialActionKind.Read) UpdateRead(t);
            else if (CurrentKind == MoonlightSpatialActionKind.Care) UpdateCare(t);

            if (_activityLight != null)
                _activityLight.intensity = ActivityLightBaseIntensity +
                    Mathf.Sin(t * Mathf.PI) * ActivityLightPulseIntensity;

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
            if (!IsLingering) return;

            var interactor = GetComponent<MoonlightSpatialInteractor>();
            var currentZone = interactor != null ? interactor.CurrentZone : null;
            if (currentZone != null && currentZone.Kind == CurrentKind) return;

            Debug.Log($"[MoonlightActivityQA] final-presentation-cancel kind={CurrentKind} " +
                "reason=left-zone marker=MOONLIGHT_ACTIVITY_FINAL_PRESENTATION_CANCELLED");
            End();
        }

        public void End()
        {
            if (_applyPersistentCompletionOnEnd)
            {
                var persistentStation = _persistentStation;
                _applyPersistentCompletionOnEnd = false;
                _persistentStation = null;
                if (persistentStation != null)
                {
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
            _ball = null;
            _gestureSample = default;
            _playProgress = 0f;
            _blocks = null;
            _playProps = null;
            _starDetails = null;
            _pathMarkers = null;
            _celebrationStars = null;
            _playArches = null;
            _podiumProps = null;
            _authoredPlayArena = null;
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

        void BuildCookStage()
        {
            if (!BuildAuthoredCookWorkbench())
            {
                Primitive(PrimitiveType.Cube, "KitchenCounterFallback", new Vector3(0f, 0.20f, 0.02f),
                    new Vector3(1.70f, 0.34f, 0.86f), new Color(0.33f, 0.22f, 0.19f), 0.02f);
                Primitive(PrimitiveType.Cube, "CounterClothFallback", new Vector3(0f, 0.38f, 0.02f),
                    new Vector3(1.58f, 0.035f, 0.76f), new Color(0.93f, 0.79f, 0.58f), 0.04f);
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

            float whiskAngle = t * Mathf.PI * 12f;
            if (_whisk != null)
            {
                _whisk.gameObject.SetActive(step == 1);
                if (step == 1)
                {
                    _whisk.localPosition = new Vector3(Mathf.Cos(whiskAngle) * 0.16f, 0.94f,
                        Mathf.Sin(whiskAngle) * 0.12f);
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
                    _cookieDetails[detailIndex].localPosition = cookiePosition
                        + new Vector3(-0.055f + mark * 0.055f, step == 3 ? 0.050f : 0.028f,
                            step == 3 ? 0.00f : 0.012f);
                    _cookieDetails[detailIndex].localScale = new Vector3(0.05f, 0.012f, 0.012f) * decorReveal;
                    _cookieDetails[detailIndex].localRotation = Quaternion.Euler(0f, 24f + i * 32f, 0f);
                }
            }

            if (_decorateProps != null && _decorateProps.Length >= 4 && step == 3)
            {
                float working = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.68f) * 3.125f));
                float squeeze = Mathf.Sin(t * Mathf.PI * 4f) * 0.08f * working;
                _decorateProps[0].localPosition = Vector3.Lerp(
                    new Vector3(Mathf.Lerp(-0.35f, 0.30f, t), 0.91f + squeeze, 0.16f),
                    new Vector3(-0.43f, 0.72f, 0.30f), 1f - working);
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

            float whiskAngle = t * Mathf.PI * 12f;
            if (TransformMatches(_whisk, true,
                    new Vector3(Mathf.Cos(whiskAngle) * 0.16f, 0.94f,
                        Mathf.Sin(whiskAngle) * 0.12f),
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
                    Vector3.Lerp(new Vector3(Mathf.Lerp(-0.35f, 0.30f, t),
                            0.91f + squeeze, 0.16f),
                        new Vector3(-0.43f, 0.72f, 0.30f), 1f - working),
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
                if (TransformMatches(_cookies[i], true, cookiePosition,
                        Quaternion.Euler(0f, 24f + i * 32f + t * 65f, 0f), cookieScale))
                    matches++;

                for (int mark = 0; mark < 3; mark++)
                {
                    int detailIndex = i * 3 + mark;
                    float decorReveal = CookDetailReveal(t, detailIndex);
                    if (TransformMatches(_cookieDetails[detailIndex], decorReveal > 0.65f,
                            cookiePosition + new Vector3(-0.055f + mark * 0.055f, 0.050f, 0f),
                            Quaternion.Euler(0f, 24f + i * 32f, 0f),
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

        void BuildPlayStage()
        {
            bool authoredArena = BuildAuthoredPlayArena();
            if (!authoredArena)
            {
                Primitive(PrimitiveType.Cylinder, "PlayMatFallback", new Vector3(0f, 0.035f, 0f),
                    new Vector3(2.15f, 0.025f, 1.24f), new Color(0.22f, 0.32f, 0.43f), 0.03f);
                Primitive(PrimitiveType.Cylinder, "TargetOuterRingFallback", new Vector3(0.86f, 0.07f, -0.18f),
                    new Vector3(0.46f, 0.012f, 0.46f), new Color(0.98f, 0.82f, 0.38f), 0.14f);
                Primitive(PrimitiveType.Cylinder, "TargetInnerDotFallback", new Vector3(0.86f, 0.085f, -0.18f),
                    new Vector3(0.20f, 0.012f, 0.20f), new Color(0.39f, 0.78f, 0.96f), 0.18f);
            }

            _ball = Primitive(PrimitiveType.Sphere, "StarBall", new Vector3(0f, 0.30f, 0f),
                Vector3.one * 0.27f, new Color(0.42f, 0.86f, 1f), 0.10f);
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
                _pathMarkers[i].gameObject.SetActive(false);
            }

            _playProps = authoredArena ? null : new[]
            {
                Primitive(PrimitiveType.Capsule, "ToyWand", new Vector3(-0.45f, 0.16f, -0.48f),
                    new Vector3(0.035f, 0.36f, 0.035f), new Color(0.82f, 0.58f, 0.98f), 0.12f),
                Primitive(PrimitiveType.Sphere, "ToyWandStar", new Vector3(-0.60f, 0.33f, -0.57f),
                    Vector3.one * 0.11f, new Color(1f, 0.88f, 0.38f), 0.22f),
                Primitive(PrimitiveType.Cylinder, "ToyHoop", new Vector3(-0.82f, 0.13f, -0.08f),
                    new Vector3(0.20f, 0.018f, 0.20f), new Color(0.95f, 0.55f, 0.68f), 0.12f),
                Primitive(PrimitiveType.Cube, "FinishFlagPole", new Vector3(1.04f, 0.33f, -0.32f),
                    new Vector3(0.035f, 0.48f, 0.035f), new Color(0.74f, 0.79f, 0.84f), 0.06f),
                Primitive(PrimitiveType.Cube, "FinishFlag", new Vector3(0.92f, 0.50f, -0.32f),
                    new Vector3(0.24f, 0.12f, 0.025f), new Color(0.98f, 0.82f, 0.38f), 0.16f),
            };
            if (_playProps != null)
            {
                _playProps[0].localRotation = Quaternion.Euler(74f, 0f, -38f);
                _playProps[2].localRotation = Quaternion.Euler(0f, 0f, 12f);
            }

            _celebrationStars = new Transform[6];
            for (int i = 0; i < _celebrationStars.Length; i++)
            {
                _celebrationStars[i] = Primitive(PrimitiveType.Sphere, $"CatchSpark-{i + 1}",
                    Vector3.zero, Vector3.one * 0.07f,
                    i % 2 == 0 ? new Color(1f, 0.88f, 0.38f) : new Color(0.98f, 0.56f, 0.68f),
                    0.30f);
                _celebrationStars[i].gameObject.SetActive(false);
            }

            _playArches = authoredArena ? null : new[]
            {
                Primitive(PrimitiveType.Capsule, "JumpArchLeftPost", new Vector3(-0.48f, 0.33f, 0.38f),
                    new Vector3(0.055f, 0.50f, 0.055f), new Color(0.54f, 0.80f, 0.70f), 0.08f),
                Primitive(PrimitiveType.Capsule, "JumpArchRightPost", new Vector3(0.48f, 0.33f, 0.38f),
                    new Vector3(0.055f, 0.50f, 0.055f), new Color(0.54f, 0.80f, 0.70f), 0.08f),
                Primitive(PrimitiveType.Cube, "JumpArchTop", new Vector3(0f, 0.62f, 0.34f),
                    new Vector3(1.02f, 0.055f, 0.055f), new Color(0.98f, 0.78f, 0.36f), 0.12f),
                Primitive(PrimitiveType.Capsule, "CatchArchLeftPost", new Vector3(0.65f, 0.32f, -0.46f),
                    new Vector3(0.050f, 0.46f, 0.050f), new Color(0.95f, 0.55f, 0.68f), 0.08f),
                Primitive(PrimitiveType.Capsule, "CatchArchRightPost", new Vector3(1.23f, 0.32f, -0.46f),
                    new Vector3(0.050f, 0.46f, 0.050f), new Color(0.95f, 0.55f, 0.68f), 0.08f),
                Primitive(PrimitiveType.Cube, "CatchArchTop", new Vector3(0.94f, 0.59f, -0.46f),
                    new Vector3(0.66f, 0.050f, 0.050f), new Color(0.42f, 0.86f, 1f), 0.12f),
            };
            if (_playArches != null)
            {
                _playArches[0].localRotation = Quaternion.Euler(0f, 0f, -4f);
                _playArches[1].localRotation = Quaternion.Euler(0f, 0f, 4f);
                SetActive(_playArches, false);
            }

            _podiumProps = new[]
            {
                Primitive(PrimitiveType.Cylinder, "CelebrationPodiumBase", new Vector3(0.94f, 0.11f, -0.46f),
                    new Vector3(0.44f, 0.10f, 0.44f), new Color(0.58f, 0.48f, 0.70f), 0.08f),
                Primitive(PrimitiveType.Cylinder, "CelebrationPodiumTop", new Vector3(0.94f, 0.23f, -0.46f),
                    new Vector3(0.30f, 0.065f, 0.30f), new Color(0.98f, 0.82f, 0.38f), 0.14f),
                Primitive(PrimitiveType.Cube, "CelebrationMedal", new Vector3(0.94f, 0.40f, -0.46f),
                    new Vector3(0.18f, 0.15f, 0.035f), new Color(1f, 0.89f, 0.36f), 0.18f),
            };
            SetActive(_podiumProps, false);
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
            SetActive(_podiumProps, step == 3);
            if (_playArches != null)
            {
                for (int i = 0; i < _playArches.Length; i++)
                    if (_playArches[i] != null)
                        _playArches[i].gameObject.SetActive((step == 2 && i < 3) || (step == 3 && i >= 3));
            }
            if (_playProps != null && _playProps.Length >= 5)
            {
                _playProps[0].gameObject.SetActive(step == 0);
                _playProps[1].gameObject.SetActive(step == 0);
                _playProps[2].gameObject.SetActive(step == 1 || step == 2);
                _playProps[3].gameObject.SetActive(step == 3);
                _playProps[4].gameObject.SetActive(step == 3);
            }
            if (_ballTrail != null) _ballTrail.enabled = step != 3 || t < 0.58f;
            if (_ball != null)
            {
                float bounce = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 4f));
                _ball.localPosition = EvaluatePlayTrajectory(step, t, _gestureSample);

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
                _blocks[i].gameObject.SetActive(step != 0);
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
                    _pathMarkers[i].gameObject.SetActive(step <= 2);
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
                    _celebrationStars[i].gameObject.SetActive(step == 3 && t > 0.18f);
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

            for (int i = 0; i < _seeds.Length; i++)
            {
                float x = -0.42f + i * 0.16f;
                float z = (i % 2 == 0) ? -0.07f : 0.08f;
                bool showSeed = step <= 1;
                bool showSprout = step >= 1 && step < 2;
                bool showFlower = step >= 2;
                _seeds[i].gameObject.SetActive(showSeed);
                _sprouts[i].gameObject.SetActive(showSprout);
                if (_flowers[i] != null) _flowers[i].gameObject.SetActive(showFlower);

                if (showSeed)
                {
                    float seedDrop = step == 0 ? Mathf.Clamp01(t * 3.2f - i * 0.22f) : 1f;
                    _seeds[i].localPosition = Vector3.Lerp(new Vector3(x - 0.20f, 0.92f, z - 0.12f),
                        new Vector3(x, 0.43f, z), seedDrop);
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
                    float bloom = step == 2
                        ? Mathf.Clamp01(t * 3.2f - i * 0.08f)
                        : 1f + Mathf.Sin(t * Mathf.PI * 2f + i * 0.65f) * 0.035f;
                    float overshoot = step == 2 ? Mathf.Sin(bloom * Mathf.PI) * 0.18f : 0f;
                    _flowers[i].localScale = Vector3.one * 0.48f * (bloom + overshoot);
                    _flowers[i].localRotation = Quaternion.Euler(-90f,
                        i * 28f + Mathf.Sin(t * Mathf.PI * 2f + i) * 5f,
                        step == 2 ? -8f * Mathf.Sin(bloom * Mathf.PI) : 0f);
                }
            }

            if (_gardenProps != null)
            {
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
                float size = Mathf.Sin(phase * Mathf.PI) * 0.04f;
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

        void UpdateRead(float t)
        {
            if (_pageFlips == null || _readMotes == null) return;

            int step = Mathf.Clamp(CurrentStep, 0, 3);

            if (_bookProps != null)
            {
                float open = step == 0
                    ? Mathf.Sin(Mathf.Clamp01(t * 2.2f) * Mathf.PI * 0.5f)
                    : 1f;
                _bookProps[1].localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(4f, 11f, open));
                _bookProps[2].localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-4f, -11f, open));
                _bookProps[3].localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(4f, 12f, open));
                _bookProps[4].localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-4f, -12f, open));
            }

            for (int i = 0; i < _pageFlips.Length; i++)
            {
                bool showPage = step == 1;
                _pageFlips[i].gameObject.SetActive(showPage);
                if (!showPage) continue;
                float phase = Mathf.Repeat(t * 1.6f + i * 0.22f, 1f);
                float turn = Mathf.Sin(phase * Mathf.PI);
                _pageFlips[i].localPosition = new Vector3(Mathf.Lerp(0.20f, -0.20f, phase),
                    0.50f + turn * 0.18f + i * 0.006f, 0f);
                _pageFlips[i].localScale = new Vector3(Mathf.Lerp(0.36f, 0.26f, turn), 0.012f, 0.50f);
                _pageFlips[i].localRotation = Quaternion.Euler(0f, Mathf.Lerp(0f, 180f, phase), Mathf.Lerp(-10f, 10f, phase));
            }

            if (_bookmark != null)
            {
                float trace = step == 2 ? 0.07f : 0.025f;
                _bookmark.localPosition = new Vector3(0.05f + Mathf.Sin(t * Mathf.PI * 2f) * trace,
                    0.49f, -0.18f);
                _bookmark.localRotation = Quaternion.Euler(0f, 0f,
                    -5f + Mathf.Sin(t * Mathf.PI * (step == 2 ? 6f : 3f)) * (step == 2 ? 9f : 4f));
            }

            for (int i = 0; i < _readMotes.Length; i++)
            {
                bool showMote = step >= 2 || (step == 0 && i < 3);
                _readMotes[i].gameObject.SetActive(showMote);
                if (!showMote) continue;
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
            float eased = Mathf.SmoothStep(0f, 1f, t);

            _careTowelTray.localScale = new Vector3(0.31f, 0.025f, 0.24f) *
                (step == 0 ? 1f + Mathf.Sin(t * Mathf.PI) * 0.08f : 1f);
            _careTowel.localPosition = step == 0
                ? Vector3.Lerp(new Vector3(-0.78f, 0.72f, -0.10f),
                    new Vector3(-0.58f, 0.335f, 0.02f), eased)
                : new Vector3(-0.58f, 0.335f, 0.02f);
            _careTowel.localRotation = Quaternion.Euler(0f, step == 0 ? Mathf.Lerp(-18f, 0f, eased) : 0f,
                step == 0 ? Mathf.Sin(t * Mathf.PI * 3f) * 4f : 0f);

            if (step == 1)
            {
                float angle = t * Mathf.PI * 4f;
                _careBrush.localPosition = new Vector3(Mathf.Cos(angle) * 0.24f,
                    0.62f + Mathf.Sin(t * Mathf.PI) * 0.07f, 0.02f + Mathf.Sin(angle) * 0.18f);
                _careBrush.localRotation = Quaternion.Euler(18f, t * 540f, 64f);
            }
            else
            {
                _careBrush.localPosition = new Vector3(0.52f, 0.42f, -0.13f);
                _careBrush.localRotation = Quaternion.Euler(0f, 0f, 68f);
            }

            if (step == 2)
            {
                _careComb.localPosition = new Vector3(Mathf.Lerp(0.46f, -0.30f, eased),
                    0.62f + Mathf.Sin(t * Mathf.PI * 3f) * 0.06f, 0.12f);
                _careComb.localRotation = Quaternion.Euler(0f, 18f, Mathf.Lerp(-22f, 18f, t));
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
                float phase = Mathf.Repeat(t * 1.45f + i * 0.19f, 1f);
                float angle = i * Mathf.PI * 2f / _careBubbles.Length + t * Mathf.PI;
                _careBubbles[i].localPosition = new Vector3(Mathf.Cos(angle) * (0.12f + i * 0.035f),
                    0.53f + phase * 0.30f, 0.02f + Mathf.Sin(angle) * 0.18f);
                _careBubbles[i].localScale = Vector3.one *
                    Mathf.Max(0.015f, Mathf.Sin(phase * Mathf.PI) * (0.055f + i * 0.006f));
            }

            bool glow = step == 3;
            _careMirrorAura.gameObject.SetActive(glow);
            if (glow)
            {
                float auraPulse = 1f + Mathf.Sin(t * Mathf.PI * 4f) * 0.10f;
                _careMirrorAura.localScale = new Vector3(0.38f, 0.018f, 0.46f) * auraPulse;
                _careMirror.localScale = new Vector3(0.27f, 0.025f, 0.34f) *
                    (1f + Mathf.Sin(t * Mathf.PI * 3f) * 0.04f);
            }
            else
            {
                _careMirror.localScale = new Vector3(0.27f, 0.025f, 0.34f);
            }

            for (int i = 0; i < _careMotes.Length; i++)
            {
                _careMotes[i].gameObject.SetActive(glow);
                if (!glow) continue;
                float phase = Mathf.Repeat(t * 0.85f + i * 0.16f, 1f);
                float angle = i * Mathf.PI * 2f / _careMotes.Length + t * Mathf.PI * 0.8f;
                _careMotes[i].localPosition = new Vector3(0.59f + Mathf.Cos(angle) * 0.42f,
                    0.55f + phase * 0.62f, 0.20f + Mathf.Sin(angle) * 0.13f);
                _careMotes[i].localScale = Vector3.one *
                    (0.025f + Mathf.Sin(phase * Mathf.PI) * 0.045f);
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
