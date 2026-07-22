using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public enum MoonlightActionQualityTier
    {
        Good,
        Great,
        Perfect
    }

    public class MoonlightActionFeedback : MonoBehaviour
    {
        public const float ReadActionDurationSeconds = 1.75f;
        public const float ReadFinalPresentationSeconds = 4.4f;
        public const float MinimumActionAccentExtent = 0.18f;
        public const float MaximumActionAccentExtent = 0.80f;
        public const float GreatActionQualityScore = 0.72f;
        public const float PerfectActionQualityScore = 0.88f;
        public const int FeedVisualObjectBudget = 4;
        public const int FeedRendererBudget = 3;
        public const int FeedMaterialBudget = 3;
        public const int FeedLightBudget = 0;

        [SerializeField] float cooldownSeconds = 1.15f;

        Transform _visual;
        ParticleSystem _particles;
        ParticleSystemRenderer _particleRenderer;
        Light _flash;
        Coroutine _running;
        Coroutine _cameraHoldRoutine;
        Coroutine _masteryFlashRoutine;
        Vector3 _baseScale = Vector3.one;
        Vector3 _basePosition;
        Quaternion _baseRotation = Quaternion.identity;
        GameObject _actionOrb;
        GameObject _actionAccent;
        TrailRenderer _actionTrail;
        Material _actionMaterial;
        readonly List<Material> _actionAccentMaterials = new();
        readonly List<Transform> _actionAccentParts = new();
        readonly List<Vector3> _actionAccentBasePositions = new();
        readonly List<Vector3> _actionAccentBaseScales = new();
        readonly List<Quaternion> _actionAccentBaseRotations = new();
        static readonly Dictionary<PrimitiveType, Mesh> ActionAccentPrimitiveMeshes = new();
        Material _trailMaterial;
        Material _particleMaterial;
        Texture2D _particleTexture;
        MoonlightActivityStage _activityStage;
        CameraController _cameraController;
        MoonlightTouchJoystick _touchJoystick;
        Vector3 _cameraFocusAnchor;
        Vector3 _actionContactPoint;
        Vector3 _actionContactStartPoint;
        Vector3 _actionPresentationDirection;
        Vector3 _actionStageScale = Vector3.one;
        bool _cameraFocusUsesStationAnchor;
        int _contactPhaseIndex = -1;
        float _cooldownUntil;
        string _stateText = "";
        MoonlightSpatialActionKind _activityKind;
        int _activityStep;
        int _activityRequiredSteps = 1;
        MoonlightGestureSample _gestureSample;
        bool _masteryCelebrationQueued;
        int _queuedMasteryTier;
        int _queuedMasteryCombo;

        public bool IsCoolingDown => Time.time < _cooldownUntil;
        public bool IsPerformingAction => _running != null;
        public bool IsPresentingResult => _activityStage != null && _activityStage.IsLingering;
        public bool CanBeginAction => !IsPerformingAction && !IsCoolingDown && !IsPresentingResult;
        public float CooldownRemaining => Mathf.Max(0f, _cooldownUntil - Time.time);
        public string InputBlockReason => IsPerformingAction
            ? "FINISH THE CURRENT MOVE"
            : IsPresentingResult
                ? "ENJOY THE RESULT"
                : IsCoolingDown
                    ? $"READY IN {CooldownRemaining:0.0}s"
                    : "";
        public string StateText => _stateText;
        public float ActionProgress01 { get; private set; }
        public int ActivityStep => _activityStep;
        public int ActivityRequiredSteps => _activityRequiredSteps;
        public MoonlightSpatialActionKind ActiveActivityKind => _activityKind;
        public MoonlightGestureSample ActiveGestureSample => _gestureSample;
        public bool HasOpaqueActionOrb => _actionOrb != null;
        public bool ActionParticlesActive => _particles != null && _particles.isPlaying &&
            _particleRenderer != null && _particleRenderer.enabled;
        public bool ActionFlashActive => _flash != null && _flash.enabled && _flash.intensity > 0f;
        public bool PlayUsesStageBallOnly => _activityKind != MoonlightSpatialActionKind.Play ||
            (_actionOrb == null && _activityStage != null &&
             _activityStage.AuthoritativePlayBallCount == 1);
        public static bool ShouldCreateOpaqueActionOrb(MoonlightSpatialActionKind kind) =>
            kind is not (MoonlightSpatialActionKind.Play or MoonlightSpatialActionKind.Feed);
        public int ActiveStageRenderers => _activityStage != null ? _activityStage.ActiveRendererCount : 0;
        public int ActiveStageMaterials => _activityStage != null ? _activityStage.ActiveUniqueMaterialCount : 0;
        public int ActiveStageLights => _activityStage != null ? _activityStage.ActiveLightCount : 0;
        public bool IsCameraFocusActive => _cameraController != null && _cameraController.IsActivityFocusActive;
        public bool VisualPoseRestoredForQA => _visual == null ||
            (Vector3.Distance(_visual.localPosition, _basePosition) <= 0.0001f &&
             Vector3.Distance(_visual.localScale, _baseScale) <= 0.0001f &&
             Quaternion.Angle(_visual.localRotation, _baseRotation) <= 0.01f);
        public Vector3 CameraFocusAnchor => _cameraFocusAnchor;
        public string CameraFocusSource => _cameraFocusUsesStationAnchor
            ? "station-anchor"
            : "safe-midpoint";
        public string ActionMotionProfile { get; private set; } = "";
        public string ActionContactPhase { get; private set; } = "";
        public string ActionContactTarget { get; private set; } = "";
        public string ActionContactSource { get; private set; } = "";
        public bool UsesLiveStageContact { get; private set; }
        public float ActionContactWeight { get; private set; }
        public bool IsActionContactActive => _contactPhaseIndex == 2;
        public Vector3 ActionContactPoint => _actionContactPoint;
        public float ActionContactTravelDistance =>
            Vector3.Distance(_actionContactStartPoint, _actionContactPoint);
        public bool UsesCameraReadableFacing { get; private set; }
        public float ActionCameraFacingAngle { get; private set; } = 180f;
        public Vector3 ActionPresentationDirection => _actionPresentationDirection;
        public float ActionVisualContactPlanarDistance
        {
            get
            {
                if (_visual == null) return float.PositiveInfinity;
                Vector3 delta = _actionContactPoint - _visual.position;
                delta.y = 0f;
                return delta.magnitude;
            }
        }
        public string ProgressText
        {
            get
            {
                string label = _stateText switch
                {
                    "Cooking" => "COOKING",
                    "Playing" => "PLAYING",
                    "Gardening" => "GARDENING",
                    "Reading" => "READING",
                    "Caring" => "CARING",
                    "Feeding" => "FEEDING",
                    "Resting" => "DREAMING",
                    "Cuddled" => "CUDDLING",
                    _ => "MAGIC IN PROGRESS"
                };
                return _activityRequiredSteps > 1
                    ? $"{ActivityVerb()}  {_activityStep + 1}/{_activityRequiredSteps}"
                    : label;
            }
        }
        public string ActiveEffectName { get; private set; } = "";
        public string ActionVisualSignature { get; private set; } = "";
        public string ActionVisualSignatureMarker { get; private set; } = "";
        public int ActionAccentRendererCount { get; private set; }
        public int ActionAccentVisualObjectCount => _actionAccent == null
            ? 0
            : _actionAccentParts.Count + 1;
        public int ActionAccentColliderCount { get; private set; }
        public int ActionAccentMaterialCount { get; private set; }
        public Vector3 ActionAccentBoundsSize { get; private set; }
        public float ActionAccentWorldExtent { get; private set; }
        public float ActionAccentContactDistance { get; private set; }
        public bool MasteryCelebrationIsQueued => _masteryCelebrationQueued;
        public int QueuedMasteryTier => _queuedMasteryTier;
        public int LastMasteryCelebrationTier { get; private set; } = -1;
        public int LastMasteryCelebrationParticles { get; private set; }
        public int LastMasteryCelebrationCombo { get; private set; }
        public string MasteryCelebrationQAMarker { get; private set; } = "";
        public MoonlightActionQualityTier ActionQualityTier { get; private set; } =
            MoonlightActionQualityTier.Great;
        public string ActionQualityQAMarker { get; private set; } =
            "MOONLIGHT_ACTION_QUALITY_GREAT";
        public int ActionQualityBurstCount { get; private set; }

        void Awake()
        {
            CacheVisualPose();
            EnsureFxRig();
        }

        public bool TryBegin(MoonlightSpatialActionKind kind, string label, string shortState,
            int activityStep = 0, int activityRequiredSteps = 1)
            => TryBegin(kind, label, shortState, activityStep, activityRequiredSteps,
                GreatActionQualityScore);

        public bool TryBegin(MoonlightSpatialActionKind kind, string label, string shortState,
            int activityStep, int activityRequiredSteps, float acceptedGestureScore)
            => TryBegin(kind, label, shortState, activityStep, activityRequiredSteps,
                MoonlightGestureSample.Synthetic(MoonlightGestureKind.Swipe,
                    acceptedGestureScore));

        public bool TryBegin(MoonlightSpatialActionKind kind, string label, string shortState,
            int activityStep, int activityRequiredSteps, MoonlightGestureSample gestureSample)
        {
            if (!CanBeginAction)
            {
                Debug.Log($"[MoonlightVisualQA] action-input-blocked kind={kind} " +
                    $"reason=\"{InputBlockReason}\" remaining={CooldownRemaining:0.00}s");
                return false;
            }

            if (_touchJoystick == null)
                _touchJoystick = FindAnyObjectByType<MoonlightTouchJoystick>();
            if (_touchJoystick != null)
                _touchJoystick.ReleaseForAcceptedActivity();
            else
                GetComponent<MoonlightPlayerController>()?.SetTouchMove(Vector2.zero);

            if (_cameraHoldRoutine != null)
            {
                StopCoroutine(_cameraHoldRoutine);
                _cameraHoldRoutine = null;
                EndCameraFocus();
            }

            _stateText = shortState;
            ActionProgress01 = 0f;
            _activityKind = kind;
            _activityStep = Mathf.Max(0, activityStep);
            _activityRequiredSteps = Mathf.Max(1, activityRequiredSteps);
            _gestureSample = gestureSample;
            ActionQualityTier = ActionQualityTierFor(gestureSample.Score);
            ActionQualityQAMarker = ActionQualityQAMarkerFor(ActionQualityTier);
            ActionQualityBurstCount = ActionQualityBurstCountFor(kind, ActionQualityTier);
            _cooldownUntil = Time.time + Mathf.Max(
                cooldownSeconds,
                DurationFor(kind, shortState) + 0.18f);
            if (_running != null)
            {
                StopCoroutine(_running);
                RestoreVisualPose();
                DestroyActionOrb();
                ActionMotionProfile = "";
                ResetContactQA();
                _running = null;
            }
            CacheVisualPose();
            _running = StartCoroutine(Play(kind, label, shortState));
            return true;
        }

        public void QueueMasteryCelebration(float averageScore, int bestCombo, int bonusCoins)
        {
            _queuedMasteryTier = MasteryCelebrationTier(averageScore, bonusCoins);
            _queuedMasteryCombo = Mathf.Max(0, bestCombo);
            _masteryCelebrationQueued = true;
            Debug.Log($"[MoonlightActivityQA] mastery-celebration-queued tier={_queuedMasteryTier} " +
                $"average={averageScore:0.00} combo={_queuedMasteryCombo} bonus={bonusCoins} " +
                "marker=MOONLIGHT_MASTERY_CELEBRATION_QUEUED");
        }

        public void PlayActionQualityHaptic()
        {
            switch (ActionQualityHapticRankFor(ActionQualityTier))
            {
                case 0:
                    HapticFeedback.Light();
                    break;
                case 1:
                    HapticFeedback.Medium();
                    break;
                default:
                    HapticFeedback.Success();
                    break;
            }
        }

        string ActivityVerb()
        {
            return _activityKind switch
            {
                MoonlightSpatialActionKind.Cook => _activityStep switch
                {
                    0 => "ADD",
                    1 => "STIR",
                    2 => "BAKE",
                    _ => "DECORATE"
                },
                MoonlightSpatialActionKind.Play => _activityStep switch
                {
                    0 => "THROW",
                    1 => "CHASE",
                    2 => "JUMP",
                    _ => "CATCH"
                },
                MoonlightSpatialActionKind.Garden => _activityStep switch
                {
                    0 => "PLANT",
                    1 => "WATER",
                    2 => "TEND",
                    _ => "BLOOM"
                },
                MoonlightSpatialActionKind.Read => _activityStep switch
                {
                    0 => "OPEN",
                    1 => "TURN",
                    2 => "TRACE",
                    _ => "REMEMBER"
                },
                MoonlightSpatialActionKind.Care => _activityStep switch
                {
                    0 => "PREP",
                    1 => "WASH",
                    2 => "BRUSH",
                    _ => "GLOW"
                },
                MoonlightSpatialActionKind.Feed => "FEED",
                _ => "MAGIC"
            };
        }

        IEnumerator Play(MoonlightSpatialActionKind kind, string label, string state)
        {
            EnsureFxRig();
            bool usesActivityStage = kind != MoonlightSpatialActionKind.Feed;
            bool usesAmbientFx = kind != MoonlightSpatialActionKind.Feed;
            if (usesActivityStage && _activityStage == null)
                _activityStage = GetComponent<MoonlightActivityStage>() ?? gameObject.AddComponent<MoonlightActivityStage>();
            var color = ColorFor(kind, state);
            float duration = DurationFor(kind, state);
            float flashIntensity = ActionQualityFlashIntensityFor(ActionQualityTier);
            BeginCameraFocus(kind);
            if (usesActivityStage)
                _activityStage.Begin(kind, _activityStep, _activityRequiredSteps, _gestureSample);
            ActionMotionProfile = MotionProfileFor(kind, _activityStep);
            if (kind == MoonlightSpatialActionKind.SleepCuddle)
                ResetContactQA();
            else
                BeginContactQA(kind);
            CreateActionOrb(kind, state, color, duration);
            Debug.Log($"[MoonlightVisualQA] action-start kind={kind} state={state} label=\"{label}\" " +
                $"step={_activityStep + 1}/{_activityRequiredSteps} duration={duration:0.00}s " +
                $"motionProfile=\"{ActionMotionProfile}\" contactTarget=\"{ActionContactTarget}\" " +
                $"visual=\"{ActionVisualSignature}\" marker={ActionVisualSignatureMarker} " +
                $"accents={ActionAccentRendererCount} colliders={ActionAccentColliderCount} " +
                $"materials={ActionAccentMaterialCount} bounds={ActionAccentBoundsSize:F3} " +
                $"accentExtent={ActionAccentWorldExtent:0.000} quality={ActionQualityTier} " +
                $"qualityBurst={ActionQualityBurstCount} qualityMarker={ActionQualityQAMarker}");

            if (_flash != null)
            {
                _flash.enabled = usesAmbientFx;
                _flash.color = color;
                _flash.intensity = usesAmbientFx ? flashIntensity : 0f;
            }

            if (_particles != null)
            {
                if (_particleRenderer != null) _particleRenderer.enabled = usesAmbientFx;
                if (usesAmbientFx)
                {
                    var main = _particles.main;
                    main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.75f);
                    main.startSpeed = new ParticleSystem.MinMaxCurve(0.65f, 1.2f);
                    main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.09f);
                    main.maxParticles = 64;
                    main.startColor = new ParticleSystem.MinMaxGradient(color, Color.white);
                    var shape = _particles.shape;
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                    shape.radius = 0.45f;
                    var emission = _particles.emission;
                    emission.SetBurst(0, new ParticleSystem.Burst(0f, (short)ActionQualityBurstCount));
                    _particles.Play(true);
                }
                else
                {
                    _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                ActionProgress01 = t;
                if (kind != MoonlightSpatialActionKind.SleepCuddle)
                    UpdateContactQA(kind, t);
                ApplyPose(kind, state, t);
                UpdateActionOrb(kind, state, t);
                if (usesActivityStage)
                    _activityStage.UpdateStage(kind, t);
                if (usesAmbientFx && _flash != null)
                    _flash.intensity = Mathf.Lerp(flashIntensity, 0f, t);
                yield return null;
            }

            while (kind == MoonlightSpatialActionKind.Play && _activityStage != null &&
                   _activityStage.IsPlayContinuationBlending)
            {
                _activityStage.UpdateStage(kind, 1f);
                yield return null;
            }

            RestoreVisualPose();
            ActionProgress01 = 1f;
            bool finalActivityStep = _activityRequiredSteps > 1 &&
                _activityStep == _activityRequiredSteps - 1;
            bool heldFinalPresentation = finalActivityStep &&
                _activityStage.LingerFinalState(FinalPresentationSecondsFor(kind));
            bool heldIntermediateStep = !finalActivityStep && _activityStage != null &&
                (kind == MoonlightSpatialActionKind.Play
                    ? _activityStage.HoldPlayStepTerminal()
                    : kind == MoonlightSpatialActionKind.Cook &&
                      _activityStage.HoldCookStepTerminal());
            if (!heldFinalPresentation && !heldIntermediateStep)
            {
                _activityStage?.End();
                EndCameraFocus();
            }
            else if (heldIntermediateStep)
            {
                EndCameraFocus();
            }
            else
            {
                _cameraHoldRoutine = StartCoroutine(HoldCameraForFinalPresentation(kind));
            }
            DestroyActionOrb();
            if (_flash != null) _flash.intensity = 0f;
            if (finalActivityStep && _masteryCelebrationQueued)
                PlayMasteryCelebration();
            Debug.Log($"[MoonlightVisualQA] action-end kind={kind} state=\"{_stateText}\" cooldown={CooldownRemaining:0.00}s");
            ActionMotionProfile = "";
            ResetContactQA();
            _masteryCelebrationQueued = false;
            _running = null;
            ActionProgress01 = 0f;
        }

        void PlayMasteryCelebration()
        {
            EnsureFxRig();
            int tier = Mathf.Clamp(_queuedMasteryTier, 0, 3);
            int particleCount = MasteryCelebrationParticleCount(tier);
            Color color = tier switch
            {
                3 => new Color(1.00f, 0.84f, 0.28f),
                2 => new Color(0.50f, 0.92f, 1.00f),
                1 => new Color(0.72f, 0.58f, 1.00f),
                _ => new Color(1.00f, 0.66f, 0.82f)
            };

            if (_particles != null)
            {
                var main = _particles.main;
                main.startColor = new ParticleSystem.MinMaxGradient(color, Color.white);
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.75f, 1.35f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(1.10f, 2.15f + tier * 0.18f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.11f + tier * 0.012f);
                main.maxParticles = Mathf.Max(64, particleCount);
                var shape = _particles.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.32f + tier * 0.05f;
                _particles.Emit(particleCount);
            }

            if (_flash != null)
            {
                _flash.color = color;
                _flash.range = 3.2f + tier * 0.35f;
                _flash.intensity = 0.75f + tier * 0.28f;
                if (_masteryFlashRoutine != null) StopCoroutine(_masteryFlashRoutine);
                _masteryFlashRoutine = StartCoroutine(FadeMasteryFlash(_flash.intensity));
            }

            LastMasteryCelebrationTier = tier;
            LastMasteryCelebrationParticles = particleCount;
            LastMasteryCelebrationCombo = _queuedMasteryCombo;
            MasteryCelebrationQAMarker = "MOONLIGHT_MASTERY_CELEBRATION_PLAYED";
            _masteryCelebrationQueued = false;
            Debug.Log($"[MoonlightActivityQA] mastery-celebration-played tier={tier} " +
                $"particles={particleCount} combo={LastMasteryCelebrationCombo} " +
                $"marker={MasteryCelebrationQAMarker}");
        }

        IEnumerator FadeMasteryFlash(float startIntensity)
        {
            const float duration = 0.72f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (_flash != null)
                    _flash.intensity = Mathf.Lerp(startIntensity, 0f,
                        Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }
            if (_flash != null) _flash.intensity = 0f;
            _masteryFlashRoutine = null;
        }

        public static int MasteryCelebrationTier(float averageScore, int bonusCoins)
        {
            if (bonusCoins >= 3 && averageScore >= 0.90f) return 3;
            if (bonusCoins >= 2 && averageScore >= 0.80f) return 2;
            if (bonusCoins >= 1 && averageScore >= 0.70f) return 1;
            return 0;
        }

        public static int MasteryCelebrationParticleCount(int tier) =>
            18 + Mathf.Clamp(tier, 0, 3) * 12;

        public static bool ValidateMasteryCelebrationContract(out string detail)
        {
            int complete = MasteryCelebrationTier(0.68f, 0);
            int good = MasteryCelebrationTier(0.74f, 1);
            int great = MasteryCelebrationTier(0.83f, 2);
            int perfect = MasteryCelebrationTier(0.95f, 3);
            int completeParticles = MasteryCelebrationParticleCount(complete);
            int perfectParticles = MasteryCelebrationParticleCount(perfect);
            detail = $"tiers={complete}/{good}/{great}/{perfect} " +
                $"particles={completeParticles}->{perfectParticles}";
            return complete == 0 && good == 1 && great == 2 && perfect == 3 &&
                completeParticles == 18 && perfectParticles == 54;
        }

        IEnumerator HoldCameraForFinalPresentation(MoonlightSpatialActionKind kind)
        {
            while (_activityStage != null && _activityStage.IsLingering)
                yield return null;

            EndCameraFocus();
            _cameraHoldRoutine = null;
            Debug.Log($"[MoonlightCameraQA] final-presentation-focus-release kind={kind} " +
                "marker=MOONLIGHT_FINAL_PRESENTATION_FOCUS_RELEASED");
        }

        static float FinalPresentationSecondsFor(MoonlightSpatialActionKind kind) => kind switch
        {
            MoonlightSpatialActionKind.Cook =>
                MoonlightActivityStage.CookFinalPresentationSeconds,
            MoonlightSpatialActionKind.Play => 4.8f,
            MoonlightSpatialActionKind.Garden => 4.6f,
            MoonlightSpatialActionKind.Read => ReadFinalPresentationSeconds,
            MoonlightSpatialActionKind.Care => 4.6f,
            _ => 0f
        };

        void ApplyPose(MoonlightSpatialActionKind kind, string state, float t)
        {
            if (_visual == null) return;
            bool resting = state == "Resting";
            bool cuddled = state == "Cuddled";
            float envelope = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI);

            float pulse = kind switch
            {
                MoonlightSpatialActionKind.Cook => 1f + Mathf.Sin(t * Mathf.PI * 10f) * 0.045f,
                MoonlightSpatialActionKind.Play => 1f + Mathf.Sin(t * Mathf.PI * 7f) * 0.075f,
                MoonlightSpatialActionKind.Garden => 1f + Mathf.Sin(t * Mathf.PI * 6f) * 0.052f,
                MoonlightSpatialActionKind.Read => 1f + Mathf.Sin(t * Mathf.PI * 4f) * 0.025f,
                MoonlightSpatialActionKind.Care => 1f + Mathf.Sin(t * Mathf.PI * 5f) * 0.035f,
                MoonlightSpatialActionKind.Feed => 1f + Mathf.Sin(t * Mathf.PI * 3f) * 0.025f,
                MoonlightSpatialActionKind.SleepCuddle when resting => Mathf.Lerp(1f, 0.92f, Mathf.SmoothStep(0f, 1f, t)),
                MoonlightSpatialActionKind.SleepCuddle when cuddled => 1f + Mathf.Sin(t * Mathf.PI * 4f) * 0.055f,
                _ => 1f
            };

            Vector3 axisScale = kind switch
            {
                MoonlightSpatialActionKind.Cook => new Vector3(1.04f / pulse, pulse, 1.04f / pulse),
                MoonlightSpatialActionKind.Play => Vector3.one * pulse,
                MoonlightSpatialActionKind.Garden => new Vector3(1.02f, pulse, 1.02f),
                MoonlightSpatialActionKind.Read => new Vector3(1.01f, pulse, 1.01f),
                MoonlightSpatialActionKind.Care => new Vector3(1.02f, pulse, 1.02f),
                MoonlightSpatialActionKind.Feed => new Vector3(1.02f, pulse, 1.02f),
                MoonlightSpatialActionKind.SleepCuddle when resting => new Vector3(1.08f, pulse, 1.08f),
                MoonlightSpatialActionKind.SleepCuddle when cuddled => new Vector3(1.03f, pulse, 1.03f),
                _ => Vector3.one
            };

            if (kind == MoonlightSpatialActionKind.Cook)
            {
                ApplyCookStepPose(t, envelope, pulse);
                return;
            }

            if (kind == MoonlightSpatialActionKind.Play)
            {
                ApplyPlayStepPose(t, envelope, pulse);
                return;
            }

            if (kind == MoonlightSpatialActionKind.Garden)
            {
                ApplyGardenStepPose(t, envelope, pulse);
                return;
            }

            if (kind == MoonlightSpatialActionKind.Read)
            {
                ApplyReadStepPose(t, envelope, pulse);
                return;
            }

            if (kind == MoonlightSpatialActionKind.Care)
            {
                ApplyCareStepPose(t, envelope, pulse);
                return;
            }

            _visual.localScale = Vector3.Scale(_baseScale, axisScale);

            Vector3 localOffset = kind switch
            {
                MoonlightSpatialActionKind.Garden => new Vector3(0f, envelope * 0.10f, -envelope * 0.18f),
                MoonlightSpatialActionKind.Read => new Vector3(0f, envelope * 0.04f, -envelope * 0.12f),
                MoonlightSpatialActionKind.Feed => new Vector3(0f, envelope * 0.04f, -envelope * 0.08f),
                MoonlightSpatialActionKind.SleepCuddle when resting => new Vector3(0f, -Mathf.SmoothStep(0f, 0.16f, t), 0f),
                MoonlightSpatialActionKind.SleepCuddle when cuddled => new Vector3(0f, envelope * 0.24f, -envelope * 0.14f),
                _ => Vector3.zero
            };
            Vector3 localEuler = kind switch
            {
                MoonlightSpatialActionKind.Garden => new Vector3(envelope * 16f, Mathf.Sin(t * Mathf.PI * 4f) * 8f, 0f),
                MoonlightSpatialActionKind.Read => new Vector3(envelope * 9f, Mathf.Sin(t * Mathf.PI * 2f) * 5f, 0f),
                MoonlightSpatialActionKind.Feed => new Vector3(envelope * 7f, 0f, Mathf.Sin(t * Mathf.PI) * 4f),
                MoonlightSpatialActionKind.SleepCuddle when resting => new Vector3(0f, 0f, -Mathf.SmoothStep(0f, 14f, t)),
                MoonlightSpatialActionKind.SleepCuddle when cuddled => new Vector3(0f, 0f, Mathf.Sin(t * Mathf.PI * 4f) * 11f),
                _ => Vector3.zero
            };
            _visual.localPosition = _basePosition + localOffset;
            _visual.localRotation = _baseRotation * Quaternion.Euler(localEuler);
        }

        void ApplyCookStepPose(float t, float envelope, float pulse)
        {
            Vector3 axisScale;
            Vector3 motion;
            Vector3 localEuler;

            switch (Mathf.Clamp(_activityStep, 0, 3))
            {
                case 0:
                    float pour = Ease(t, 0.12f, 0.50f);
                    float pourRecover = Ease(t, 0.72f, 0.96f);
                    axisScale = new Vector3(1.03f / pulse, pulse * 1.02f, 1.02f / pulse);
                    motion = new Vector3(Mathf.Lerp(-0.16f, 0.10f, pour) * envelope,
                        (0.08f + pour * 0.12f) * envelope,
                        (0.06f + pour * 0.30f - pourRecover * 0.16f) * envelope);
                    localEuler = new Vector3((8f + pour * 17f - pourRecover * 10f) * envelope,
                        Mathf.Lerp(-14f, 12f, pour) * envelope,
                        Mathf.Lerp(-22f, 16f, Ease(t, 0.28f, 0.58f)) * envelope);
                    break;
                case 1:
                    float stirAngle = t * Mathf.PI * 12f;
                    float stir = Mathf.Sin(stirAngle);
                    float stirCircle = Mathf.Cos(stirAngle);
                    axisScale = new Vector3(1.03f / pulse, pulse, 1.03f / pulse);
                    motion = new Vector3(stirCircle * 0.11f * envelope,
                        envelope * (0.08f + Mathf.Abs(stir) * 0.05f),
                        envelope * (0.22f + stir * 0.045f));
                    localEuler = new Vector3((13f + Mathf.Abs(stir) * 5f) * envelope, stir * 18f * envelope, stirCircle * 12f * envelope);
                    break;
                case 2:
                    float crouch = ContactPulse(t, 0.04f, 0.24f, 0.48f);
                    float rise = Ease(t, 0.54f, 0.84f);
                    axisScale = new Vector3(1.08f - crouch * 0.03f, 0.94f + rise * 0.12f, 1.08f - crouch * 0.03f);
                    motion = new Vector3(0f, envelope * (-0.10f + rise * 0.30f),
                        envelope * (0.10f + Ease(t, 0.16f, 0.42f) * 0.26f - rise * 0.13f));
                    localEuler = new Vector3(Mathf.Lerp(24f, -8f, rise) * envelope,
                        Mathf.Sin(t * Mathf.PI * 3f) * 4f * envelope, -6f * crouch);
                    break;
                default:
                    float dot = Mathf.Sin(t * Mathf.PI * 16f);
                    float glide = Mathf.Sin(t * Mathf.PI * 4f);
                    axisScale = new Vector3(1.02f / pulse, pulse * 1.01f, 1.02f / pulse);
                    motion = new Vector3(glide * 0.12f * envelope,
                        envelope * (0.11f + Mathf.Abs(dot) * 0.08f), 0.24f * envelope);
                    localEuler = new Vector3((13f + Mathf.Abs(dot) * 5f) * envelope,
                        glide * 12f * envelope, dot * 15f * envelope);
                    break;
            }

            axisScale = Vector3.Lerp(Vector3.one, axisScale, envelope);
            ApplyFacingPose(axisScale, motion, localEuler, _actionContactPoint, 0.82f);
        }

        void ApplyPlayStepPose(float t, float envelope, float pulse)
        {
            Vector3 axisScale;
            Vector3 motion;
            Vector3 localEuler;

            switch (Mathf.Clamp(_activityStep, 0, 3))
            {
                case 0:
                    float throwEase = Ease(t, 0.12f, 0.32f);
                    float throwRecover = Ease(t, 0.70f, 0.96f);
                    axisScale = Vector3.one * (1f + (pulse - 1f) * 0.8f);
                    motion = new Vector3(Mathf.Lerp(-0.24f, 0.18f, throwEase) * envelope,
                        (0.08f + Mathf.Sin(t * Mathf.PI) * 0.20f) * envelope,
                        (-0.10f + throwEase * 0.48f - throwRecover * 0.15f) * envelope);
                    localEuler = new Vector3(Mathf.Lerp(-10f, 15f, throwEase) * envelope,
                        Mathf.Lerp(-28f, 34f, throwEase) * envelope,
                        Mathf.Lerp(-13f, 17f, throwEase) * envelope);
                    break;
                case 1:
                    float dash = Mathf.Sin(t * Mathf.PI * 8f);
                    axisScale = new Vector3(1.08f, 0.96f + Mathf.Abs(dash) * 0.05f, 1.03f);
                    motion = new Vector3(dash * 0.10f * envelope,
                        envelope * (0.08f + Mathf.Abs(dash) * 0.11f), 0.36f * envelope);
                    localEuler = new Vector3(12f * envelope, dash * 10f * envelope, -dash * 16f * envelope);
                    break;
                case 2:
                    float hop = Mathf.Sin(Ease(t, 0.10f, 0.86f) * Mathf.PI);
                    float landing = ContactPulse(t, 0.70f, 0.79f, 0.90f);
                    float squash = (ActionContactWeight - hop * 0.18f) * 0.10f;
                    axisScale = new Vector3(1f + squash, 1f + hop * 0.15f - squash, 1f + squash);
                    motion = new Vector3(0.08f * Mathf.Sin(t * Mathf.PI * 2f) * envelope,
                        hop * 0.52f - landing * 0.08f, 0.24f * envelope);
                    localEuler = new Vector3((-10f + hop * 18f - landing * 10f) * envelope,
                        Mathf.Sin(t * Mathf.PI * 2f) * 8f * envelope,
                        Mathf.Sin(t * Mathf.PI * 2f) * 11f * envelope);
                    break;
                default:
                    float reach = Mathf.Sin(Ease(t, 0.04f, 0.58f) * Mathf.PI * 0.5f);
                    float settle = Ease(t, 0.48f, 0.84f);
                    axisScale = new Vector3(0.96f + settle * 0.05f, 1.12f - settle * 0.08f, 0.96f + settle * 0.05f);
                    motion = new Vector3(Mathf.Lerp(0.18f, -0.05f, settle) * envelope,
                        reach * 0.38f - settle * 0.12f, (0.12f + reach * 0.30f - settle * 0.10f) * envelope);
                    localEuler = new Vector3(Mathf.Lerp(-10f, 12f, settle) * envelope,
                        Mathf.Lerp(20f, -5f, settle) * envelope,
                        Mathf.Lerp(16f, -4f, settle) * envelope);
                    break;
            }

            axisScale = Vector3.Lerp(Vector3.one, axisScale, envelope);
            ApplyFacingPose(axisScale, motion, localEuler, _actionContactPoint);
        }

        void ApplyFacingPose(Vector3 axisScale, Vector3 motion, Vector3 localEuler, Vector3 targetWorld,
            float maxApproachDistance = 0f, bool cameraReadableFacing = false,
            float minimumPresentationAngle = 24f, float maximumPresentationAngle = 34f)
        {
            var frame = _visual.parent != null ? _visual.parent : transform;
            Vector3 targetDirection = targetWorld - frame.position;
            targetDirection.y = 0f;
            if (targetDirection.sqrMagnitude < 0.0001f)
                targetDirection = frame.forward;
            float targetPlanarDistance = targetDirection.magnitude;
            targetDirection.Normalize();

            Vector3 presentationDirection = targetDirection;
            UsesCameraReadableFacing = false;
            ActionCameraFacingAngle = 180f;
            var mainCamera = cameraReadableFacing ? Camera.main : null;
            if (mainCamera != null)
            {
                Vector3 cameraDirection = mainCamera.transform.position - frame.position;
                cameraDirection.y = 0f;
                if (cameraDirection.sqrMagnitude > 0.0001f)
                {
                    cameraDirection.Normalize();
                    float targetDelta = Vector3.SignedAngle(cameraDirection, targetDirection, Vector3.up);
                    float side = Mathf.Abs(targetDelta) > 1f ? Mathf.Sign(targetDelta) : 1f;
                    float presentationAngle = Mathf.Clamp(Mathf.Abs(targetDelta),
                        minimumPresentationAngle, maximumPresentationAngle);
                    presentationDirection = Quaternion.AngleAxis(side * presentationAngle, Vector3.up) *
                        cameraDirection;
                    presentationDirection.Normalize();
                    UsesCameraReadableFacing = true;
                    ActionCameraFacingAngle = Vector3.Angle(cameraDirection, presentationDirection);
                }
            }

            _actionPresentationDirection = presentationDirection;
            Vector3 targetLocalForward = frame.InverseTransformDirection(targetDirection);
            targetLocalForward.y = 0f;
            targetLocalForward.Normalize();
            Vector3 presentationLocalForward = frame.InverseTransformDirection(presentationDirection);
            presentationLocalForward.y = 0f;
            presentationLocalForward.Normalize();
            Vector3 localRight = Vector3.Cross(Vector3.up, presentationLocalForward);
            float faceYaw = Mathf.Atan2(presentationLocalForward.x, presentationLocalForward.z) * Mathf.Rad2Deg;
            float approachDistance = maxApproachDistance > 0f
                ? Mathf.Clamp(targetPlanarDistance - 0.72f, 0f, maxApproachDistance) *
                  Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(ActionContactWeight * 4f))
                : 0f;

            _visual.localScale = Vector3.Scale(_baseScale, axisScale);
            _visual.localPosition = _basePosition + localRight * motion.x + Vector3.up * motion.y +
                targetLocalForward * (motion.z + approachDistance);
            _visual.localRotation = Quaternion.Euler(0f, faceYaw, 0f) * _baseRotation * Quaternion.Euler(localEuler);
        }

        void ApplyGardenStepPose(float t, float envelope, float pulse)
        {
            Vector3 axisScale;
            Vector3 motion;
            Vector3 localEuler;

            switch (Mathf.Clamp(_activityStep, 0, 3))
            {
                case 0:
                    float plant = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 1.35f));
                    axisScale = new Vector3(1.05f, 0.95f + plant * 0.05f, 1.04f);
                    motion = new Vector3(Mathf.Lerp(-0.24f, 0.20f, plant) * envelope,
                        -0.10f * envelope, 0.16f * envelope);
                    localEuler = new Vector3(22f * envelope,
                        Mathf.Lerp(-18f, 16f, plant) * envelope, -8f * envelope);
                    break;
                case 1:
                    float water = t * Mathf.PI * 6f;
                    axisScale = new Vector3(1.02f / pulse, pulse, 1.02f / pulse);
                    motion = new Vector3(Mathf.Cos(water) * 0.18f * envelope,
                        0.10f * envelope, 0.15f * envelope + Mathf.Sin(water) * 0.05f * envelope);
                    localEuler = new Vector3(15f * envelope,
                        Mathf.Sin(water) * 15f * envelope, Mathf.Cos(water) * 12f * envelope);
                    break;
                case 2:
                    float tend = Mathf.Sin(t * Mathf.PI * 12f);
                    axisScale = new Vector3(1.04f, 0.98f + Mathf.Abs(tend) * 0.04f, 1.02f);
                    motion = new Vector3(tend * 0.28f * envelope,
                        (0.04f + Mathf.Abs(tend) * 0.08f) * envelope, 0.14f * envelope);
                    localEuler = new Vector3(12f * envelope, tend * 24f * envelope,
                        -tend * 14f * envelope);
                    break;
                default:
                    float bloom = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.22f) * 1.55f));
                    float bloomPulse = Mathf.Sin(t * Mathf.PI * 5f) * 0.04f;
                    axisScale = new Vector3(0.98f - bloomPulse, 1f + bloom * 0.14f + bloomPulse,
                        0.98f - bloomPulse);
                    motion = new Vector3(0f, bloom * 0.38f, 0.10f * envelope);
                    localEuler = new Vector3(Mathf.Lerp(14f, -6f, bloom) * envelope,
                        Mathf.Sin(t * Mathf.PI * 3f) * 8f * envelope, 0f);
                    break;
            }

            axisScale = Vector3.Lerp(Vector3.one, axisScale, envelope);
            ApplyFacingPose(axisScale, motion, localEuler, _actionContactPoint, 0.62f, true);
        }

        void ApplyReadStepPose(float t, float envelope, float pulse)
        {
            Vector3 axisScale;
            Vector3 motion;
            Vector3 localEuler;

            switch (Mathf.Clamp(_activityStep, 0, 3))
            {
                case 0:
                    float open = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 1.45f));
                    axisScale = new Vector3(1.04f, 0.97f + open * 0.03f, 1.03f);
                    motion = new Vector3(0f, -0.05f * envelope, 0.13f * envelope);
                    localEuler = new Vector3(Mathf.Lerp(16f, 8f, open) * envelope,
                        0f, Mathf.Lerp(-7f, 7f, open) * envelope);
                    break;
                case 1:
                    float turn = Mathf.Sin(t * Mathf.PI * 4f);
                    axisScale = new Vector3(1.01f / pulse, pulse, 1.01f / pulse);
                    motion = new Vector3(turn * 0.22f * envelope,
                        0.07f * envelope, 0.12f * envelope);
                    localEuler = new Vector3(10f * envelope, turn * 24f * envelope,
                        -turn * 11f * envelope);
                    break;
                case 2:
                    float traceAngle = t * Mathf.PI * 6f;
                    axisScale = new Vector3(1.02f, 1f + Mathf.Sin(traceAngle) * 0.02f, 1.02f);
                    motion = new Vector3(Mathf.Cos(traceAngle) * 0.13f * envelope,
                        0.10f * envelope + Mathf.Sin(traceAngle) * 0.08f * envelope,
                        0.11f * envelope);
                    localEuler = new Vector3(8f * envelope,
                        Mathf.Sin(traceAngle) * 13f * envelope,
                        Mathf.Cos(traceAngle) * 9f * envelope);
                    break;
                default:
                    float remember = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.18f) * 1.45f));
                    float breathe = Mathf.Sin(t * Mathf.PI * 4f) * 0.025f;
                    axisScale = new Vector3(1f - breathe, 1f + remember * 0.09f + breathe, 1f - breathe);
                    motion = new Vector3(0f, remember * 0.22f, 0.09f * envelope);
                    localEuler = new Vector3(Mathf.Lerp(9f, -3f, remember) * envelope,
                        Mathf.Sin(t * Mathf.PI * 2f) * 5f * envelope, 0f);
                    break;
            }

            axisScale = Vector3.Lerp(Vector3.one, axisScale, envelope);
            ApplyFacingPose(axisScale, motion, localEuler, _actionContactPoint, 0.52f, true);
        }

        void ApplyCareStepPose(float t, float envelope, float pulse)
        {
            Vector3 axisScale;
            Vector3 motion;
            Vector3 localEuler;

            switch (Mathf.Clamp(_activityStep, 0, 3))
            {
                case 0:
                    float warmPress = Ease(t, 0.10f, 0.42f) * (1f - Ease(t, 0.76f, 0.94f));
                    axisScale = new Vector3(1.03f + warmPress * 0.03f,
                        pulse - warmPress * 0.025f, 1.03f + warmPress * 0.03f);
                    motion = new Vector3(-0.10f * envelope, -0.04f * warmPress,
                        (0.10f + warmPress * 0.20f) * envelope);
                    localEuler = new Vector3((10f + warmPress * 11f) * envelope,
                        -8f * envelope, -10f * warmPress);
                    break;
                case 1:
                    float brushAngle = t * Mathf.PI * 8f;
                    float brushStroke = Mathf.Sin(brushAngle);
                    axisScale = new Vector3(1.02f / pulse, pulse, 1.02f / pulse);
                    motion = new Vector3(Mathf.Cos(brushAngle) * 0.14f * envelope,
                        (0.06f + Mathf.Abs(brushStroke) * 0.05f) * envelope,
                        (0.15f + brushStroke * 0.05f) * envelope);
                    localEuler = new Vector3(13f * envelope,
                        brushStroke * 16f * envelope, Mathf.Cos(brushAngle) * 13f * envelope);
                    break;
                case 2:
                    float combSweep = Ease(t, 0.10f, 0.82f);
                    float combRepeat = Mathf.Sin(t * Mathf.PI * 5f);
                    axisScale = new Vector3(1.02f, 0.99f + Mathf.Abs(combRepeat) * 0.025f, 1.02f);
                    motion = new Vector3(Mathf.Lerp(0.18f, -0.18f, combSweep) * envelope,
                        (0.08f + Mathf.Abs(combRepeat) * 0.05f) * envelope, 0.16f * envelope);
                    localEuler = new Vector3(10f * envelope,
                        Mathf.Lerp(18f, -16f, combSweep) * envelope, -combRepeat * 12f * envelope);
                    break;
                default:
                    float mirrorLift = Ease(t, 0.16f, 0.58f);
                    float glow = Mathf.Sin(t * Mathf.PI * 4f) * 0.025f;
                    axisScale = new Vector3(1f - glow, 1f + mirrorLift * 0.07f + glow, 1f - glow);
                    motion = new Vector3(0.08f * envelope, mirrorLift * 0.16f,
                        (0.10f + mirrorLift * 0.10f) * envelope);
                    localEuler = new Vector3(Mathf.Lerp(9f, -3f, mirrorLift) * envelope,
                        7f * envelope, glow * 220f);
                    break;
            }

            axisScale = Vector3.Lerp(Vector3.one, axisScale, envelope);
            ApplyFacingPose(axisScale, motion, localEuler, _actionContactPoint, 0.70f, true, 20f, 38f);
        }

        static string MotionProfileFor(MoonlightSpatialActionKind kind, int activityStep)
        {
            return kind switch
            {
                MoonlightSpatialActionKind.Cook => Mathf.Clamp(activityStep, 0, 3) switch
                {
                    0 => "cook-add-pour",
                    1 => "cook-stir-circle",
                    2 => "cook-bake-rise",
                    _ => "cook-decorate-dots"
                },
                MoonlightSpatialActionKind.Play => Mathf.Clamp(activityStep, 0, 3) switch
                {
                    0 => "play-throw-follow-through",
                    1 => "play-chase-dash",
                    2 => "play-jump-hop",
                    _ => "play-catch-reach"
                },
                MoonlightSpatialActionKind.Garden => Mathf.Clamp(activityStep, 0, 3) switch
                {
                    0 => "garden-plant-scoop",
                    1 => "garden-water-circle",
                    2 => "garden-tend-zigzag",
                    _ => "garden-bloom-rise"
                },
                MoonlightSpatialActionKind.Read => Mathf.Clamp(activityStep, 0, 3) switch
                {
                    0 => "read-open-settle",
                    1 => "read-turn-swipe",
                    2 => "read-trace-circle",
                    _ => "read-remember-glow"
                },
                MoonlightSpatialActionKind.Care => Mathf.Clamp(activityStep, 0, 3) switch
                {
                    0 => "care-towel-warm-press",
                    1 => "care-bubble-brush-circle",
                    2 => "care-moon-comb-sweep",
                    _ => "care-mirror-glow-hold"
                },
                MoonlightSpatialActionKind.Feed => "feed-bowl-to-mouth",
                _ => ""
            };
        }

        void BeginContactQA(MoonlightSpatialActionKind kind)
        {
            ActionContactTarget = ContactTargetFor(kind, _activityStep);
            ActionContactWeight = 0f;
            _actionContactPoint = ContactPointFor(kind, _activityStep, 0f);
            _actionContactStartPoint = _actionContactPoint;
            _contactPhaseIndex = -1;
            SetContactPhase(kind, 0);
        }

        void UpdateContactQA(MoonlightSpatialActionKind kind, float t)
        {
            _actionContactPoint = ContactPointFor(kind, _activityStep, t);
            ActionContactWeight = ContactWeightFor(kind, _activityStep, t);
            SetContactPhase(kind, ContactPhaseIndexFor(kind, _activityStep, t));
        }

        void SetContactPhase(MoonlightSpatialActionKind kind, int phaseIndex)
        {
            if (_contactPhaseIndex == phaseIndex) return;
            int previousPhase = _contactPhaseIndex;
            _contactPhaseIndex = phaseIndex;
            ActionContactPhase = phaseIndex switch
            {
                0 => "anticipation",
                1 => "approach",
                2 => "contact",
                3 => "follow-through",
                _ => "recovery"
            };

            if (phaseIndex == 2)
            {
                Debug.Log($"[MoonlightVisualQA] action-contact kind={kind} step={_activityStep + 1} " +
                    $"profile={ActionMotionProfile} target={ActionContactTarget} " +
                    "marker=MOONLIGHT_ACTION_CONTACT");
            }
            else if (phaseIndex == 3 && previousPhase == 2)
            {
                Debug.Log($"[MoonlightVisualQA] action-follow-through kind={kind} step={_activityStep + 1} " +
                    $"profile={ActionMotionProfile} marker=MOONLIGHT_ACTION_FOLLOW_THROUGH");
            }
        }

        void ResetContactQA()
        {
            ActionContactPhase = "";
            ActionContactTarget = "";
            ActionContactSource = "";
            UsesLiveStageContact = false;
            ActionContactWeight = 0f;
            _actionContactPoint = Vector3.zero;
            _actionContactStartPoint = Vector3.zero;
            _actionPresentationDirection = Vector3.zero;
            UsesCameraReadableFacing = false;
            ActionCameraFacingAngle = 180f;
            _contactPhaseIndex = -1;
        }

        static string ContactTargetFor(MoonlightSpatialActionKind kind, int activityStep)
        {
            int step = Mathf.Clamp(activityStep, 0, 3);
            return kind switch
            {
                MoonlightSpatialActionKind.Cook => step switch
                {
                    0 => "bowl",
                    1 => "whisk",
                    2 => "oven-tray",
                    _ => "cookies"
                },
                MoonlightSpatialActionKind.Play => step switch
                {
                    0 => "ball-launch",
                    1 => "ball-path",
                    2 => "jump-arc",
                    _ => "ball-catch"
                },
                MoonlightSpatialActionKind.Garden => step switch
                {
                    0 => "seed-bed",
                    1 => "watering-spout",
                    2 => "flower-bed",
                    _ => "bloom-center"
                },
                MoonlightSpatialActionKind.Read => step switch
                {
                    0 => "book-cover",
                    1 => "turning-page",
                    2 => "bookmark-trace",
                    _ => "memory-motes"
                },
                MoonlightSpatialActionKind.Care => step switch
                {
                    0 => "towel-tray",
                    1 => "bubble-brush",
                    2 => "moon-comb",
                    _ => "vanity-mirror"
                },
                MoonlightSpatialActionKind.Feed => "mouth",
                _ => ""
            };
        }

        static int ContactPhaseIndexFor(MoonlightSpatialActionKind kind, int activityStep, float t)
        {
            int step = Mathf.Clamp(activityStep, 0, 3);
            if (kind == MoonlightSpatialActionKind.Play && step == 2)
            {
                if (t < 0.06f) return 0;
                if (t < 0.12f) return 1;
                if (t < 0.23f) return 2;
                if (t < 0.68f) return 3;
                if (t < 0.84f) return 2;
                return 4;
            }

            float approachStart;
            float contactStart;
            float contactEnd;
            float recoveryStart;
            if (kind == MoonlightSpatialActionKind.Cook)
            {
                approachStart = 0.07f;
                contactStart = step switch { 0 => 0.26f, 1 => 0.16f, 2 => 0.22f, _ => 0.16f };
                contactEnd = step switch { 0 => 0.66f, 1 => 0.82f, 2 => 0.60f, _ => 0.80f };
                recoveryStart = step == 1 || step == 3 ? 0.92f : 0.86f;
            }
            else if (kind == MoonlightSpatialActionKind.Feed)
            {
                approachStart = 0.05f;
                contactStart = 0.32f;
                contactEnd = 0.72f;
                recoveryStart = 0.90f;
            }
            else if (kind == MoonlightSpatialActionKind.Play)
            {
                approachStart = 0.04f;
                contactStart = step switch
                {
                    0 => 0.12f,
                    1 => 0.18f,
                    3 => MoonlightActivityStage.PlayCatchContactProgress,
                    _ => 0.24f
                };
                contactEnd = step switch { 0 => 0.30f, 1 => 0.80f, _ => 0.68f };
                recoveryStart = step == 0 ? 0.78f : 0.90f;
            }
            else if (kind == MoonlightSpatialActionKind.Care)
            {
                approachStart = 0.05f;
                contactStart = step switch { 0 => 0.18f, 1 => 0.14f, 2 => 0.16f, _ => 0.22f };
                contactEnd = step switch { 0 => 0.74f, 1 => 0.82f, 2 => 0.78f, _ => 0.86f };
                recoveryStart = step switch { 0 => 0.90f, 1 => 0.94f, 2 => 0.92f, _ => 0.95f };
            }
            else
            {
                approachStart = 0.08f;
                contactStart = 0.24f;
                contactEnd = 0.68f;
                recoveryStart = 0.88f;
            }

            if (t < approachStart) return 0;
            if (t < contactStart) return 1;
            if (t < contactEnd) return 2;
            if (t < recoveryStart) return 3;
            return 4;
        }

        static float ContactWeightFor(MoonlightSpatialActionKind kind, int activityStep, float t)
        {
            int step = Mathf.Clamp(activityStep, 0, 3);
            if (kind == MoonlightSpatialActionKind.Cook)
            {
                return step switch
                {
                    0 => ContactPulse(t, 0.16f, 0.48f, 0.72f),
                    1 => Ease(t, 0.10f, 0.22f) * (1f - Ease(t, 0.80f, 0.92f)) *
                        (0.76f + Mathf.Abs(Mathf.Sin(t * Mathf.PI * 12f)) * 0.24f),
                    2 => ContactPulse(t, 0.14f, 0.38f, 0.64f),
                    _ => Ease(t, 0.10f, 0.20f) * (1f - Ease(t, 0.80f, 0.92f)) *
                        (0.58f + Mathf.Abs(Mathf.Sin(t * Mathf.PI * 16f)) * 0.42f)
                };
            }

            if (kind == MoonlightSpatialActionKind.Feed)
                return ContactPulse(t, 0.08f, 0.46f, 0.82f);

            if (kind == MoonlightSpatialActionKind.Play)
            {
                return step switch
                {
                    0 => ContactPulse(t, 0.04f, 0.18f, 0.34f),
                    1 => Ease(t, 0.12f, 0.24f) * (1f - Ease(t, 0.80f, 0.92f)) *
                        (0.70f + Mathf.Abs(Mathf.Sin(t * Mathf.PI * 8f)) * 0.30f),
                    2 => Mathf.Max(ContactPulse(t, 0.06f, 0.16f, 0.25f),
                        ContactPulse(t, 0.67f, 0.78f, 0.88f)),
                    _ => ContactPulse(t, 0.16f, 0.42f, 0.72f)
                };
            }

            if (kind == MoonlightSpatialActionKind.Care)
            {
                return step switch
                {
                    0 => ContactPulse(t, 0.08f, 0.38f, 0.80f),
                    1 => Ease(t, 0.08f, 0.20f) * (1f - Ease(t, 0.82f, 0.94f)) *
                        (0.74f + Mathf.Abs(Mathf.Sin(t * Mathf.PI * 8f)) * 0.26f),
                    2 => Ease(t, 0.10f, 0.24f) * (1f - Ease(t, 0.78f, 0.92f)) *
                        (0.78f + Mathf.Abs(Mathf.Sin(t * Mathf.PI * 5f)) * 0.22f),
                    _ => Ease(t, 0.14f, 0.38f) * (1f - Ease(t, 0.86f, 0.96f))
                };
            }

            return ContactPulse(t, 0.16f, 0.42f, 0.74f);
        }

        Vector3 ContactPointFor(MoonlightSpatialActionKind kind, int activityStep, float t)
        {
            int step = Mathf.Clamp(activityStep, 0, 3);
            if (_activityStage != null &&
                _activityStage.TryGetInteractionContactPoint(kind, step, t, out Vector3 stageContact))
            {
                ActionContactSource = "activity-stage";
                UsesLiveStageContact = true;
                return stageContact;
            }

            ActionContactSource = "fallback";
            UsesLiveStageContact = false;

            if (kind == MoonlightSpatialActionKind.Feed)
            {
                Vector3 characterPosition = _visual != null ? _visual.position : transform.position;
                Vector3 bowl = characterPosition + transform.right * 0.38f +
                    transform.forward * 0.08f + Vector3.up * 0.28f;
                Vector3 mouth = characterPosition + transform.forward * 0.04f + Vector3.up * 0.92f;
                float travel = Ease(t, 0.06f, 0.38f);
                Vector3 point = Vector3.Lerp(bowl, mouth, travel);
                point.y += Mathf.Sin(travel * Mathf.PI) * 0.12f;
                return point;
            }

            if (kind == MoonlightSpatialActionKind.Cook)
            {
                if (step == 0) return StagePoint(new Vector3(0f, 0.68f, 0f));
                if (step == 1)
                {
                    float whiskAngle = t * Mathf.PI * 12f;
                    return StagePoint(new Vector3(Mathf.Cos(whiskAngle) * 0.16f, 0.94f,
                        Mathf.Sin(whiskAngle) * 0.12f));
                }
                if (step == 2) return StagePoint(new Vector3(0.45f, 0.56f, 0.18f));
                float cookieSweep = Mathf.Sin(t * Mathf.PI * 4f) * 0.5f + 0.5f;
                return StagePoint(new Vector3(Mathf.Lerp(-0.08f, 0.40f, cookieSweep), 0.72f, 0.12f));
            }

            if (kind == MoonlightSpatialActionKind.Play)
            {
                if (step == 0)
                {
                    Vector3 launch = Vector3.Lerp(new Vector3(-1.15f, 0.24f, 0.34f),
                        new Vector3(1.12f, 0.30f, -0.30f), t);
                    launch.y += Mathf.Sin(t * Mathf.PI) * 1.12f;
                    return StagePoint(launch);
                }
                if (step == 1)
                {
                    float angle = t * Mathf.PI * 2f;
                    float bounce = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 4f));
                    Vector3 chase = Vector3.Lerp(new Vector3(-1.08f, 0.26f, -0.42f),
                        new Vector3(1.04f, 0.30f, -0.10f), t);
                    chase += new Vector3(Mathf.Sin(angle * 2.5f) * 0.16f, bounce * 0.34f,
                        Mathf.Sin(t * Mathf.PI * 4f) * 0.34f);
                    return StagePoint(chase);
                }
                if (step == 2)
                {
                    Vector3 jump = Vector3.Lerp(new Vector3(-0.90f, 0.24f, 0.38f),
                        new Vector3(0.88f, 0.26f, 0.38f), t);
                    jump.y += Mathf.Sin(t * Mathf.PI) * 1.14f;
                    jump.z += Mathf.Sin(t * Mathf.PI * 2f) * 0.12f;
                    return StagePoint(jump);
                }

                float settle = Mathf.Clamp01(t * 2.5f);
                Vector3 catchPoint = Vector3.Lerp(new Vector3(0.48f, 1.12f, -0.18f),
                    new Vector3(0.94f, 0.54f, -0.46f), settle);
                catchPoint.y += Mathf.Sin(t * Mathf.PI * 5f) * 0.10f * (1f - settle);
                return StagePoint(catchPoint);
            }

            if (kind == MoonlightSpatialActionKind.Garden)
            {
                return step switch
                {
                    0 => StagePoint(new Vector3(-0.34f, 0.18f, 0.12f)),
                    1 => StagePoint(new Vector3(0.42f, 0.46f, 0.02f)),
                    2 => StagePoint(new Vector3(0.18f, 0.54f, 0.08f)),
                    _ => StagePoint(new Vector3(0f, 0.82f, 0.06f))
                };
            }

            if (kind == MoonlightSpatialActionKind.Read)
            {
                return step switch
                {
                    0 => StagePoint(new Vector3(0f, 0.58f, 0f)),
                    1 => StagePoint(new Vector3(Mathf.Lerp(-0.22f, 0.22f, t), 0.68f, 0.02f)),
                    2 => StagePoint(new Vector3(0.28f, 0.64f, -0.02f)),
                    _ => StagePoint(new Vector3(0f, 0.84f, 0f))
                };
            }

            if (kind == MoonlightSpatialActionKind.Care)
            {
                if (step == 0)
                    return StagePoint(new Vector3(-0.58f, 0.285f, 0.02f));
                if (step == 1)
                {
                    float brushAngle = t * Mathf.PI * 4f;
                    return StagePoint(new Vector3(Mathf.Cos(brushAngle) * 0.24f,
                        0.62f + Mathf.Sin(t * Mathf.PI) * 0.07f,
                        0.02f + Mathf.Sin(brushAngle) * 0.18f));
                }
                if (step == 2)
                    return StagePoint(new Vector3(Mathf.Lerp(0.46f, -0.30f, Mathf.SmoothStep(0f, 1f, t)),
                        0.62f + Mathf.Sin(t * Mathf.PI * 3f) * 0.06f, 0.12f));
                return StagePoint(new Vector3(0.59f, 0.76f, 0.23f));
            }

            return _cameraFocusAnchor;
        }

        Vector3 StagePoint(Vector3 localPoint)
        {
            return _cameraFocusAnchor + Vector3.Scale(localPoint, _actionStageScale);
        }

        static float Ease(float t, float start, float end)
        {
            return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(start, end, t));
        }

        static float ContactPulse(float t, float start, float peak, float end)
        {
            return t <= peak ? Ease(t, start, peak) : 1f - Ease(t, peak, end);
        }

        void CacheVisualPose()
        {
            _visual = transform.Find("Visual");
            if (_visual == null) return;
            _baseScale = _visual.localScale;
            _basePosition = _visual.localPosition;
            _baseRotation = _visual.localRotation;
        }

        void RestoreVisualPose()
        {
            if (_visual == null) return;
            _visual.localScale = _baseScale;
            _visual.localPosition = _basePosition;
            _visual.localRotation = _baseRotation;
        }

        void CreateActionOrb(MoonlightSpatialActionKind kind, string state, Color color, float duration)
        {
            DestroyActionOrb();
            ActiveEffectName = kind switch
            {
                MoonlightSpatialActionKind.Cook => "moon-kitchen",
                MoonlightSpatialActionKind.Play => "star-ball",
                MoonlightSpatialActionKind.Garden => "moon-garden",
                MoonlightSpatialActionKind.Read => "story-pages",
                MoonlightSpatialActionKind.Care => "moon-spa-vanity",
                MoonlightSpatialActionKind.Feed => "snack-bowl",
                MoonlightSpatialActionKind.SleepCuddle when state == "Resting" => "dream-orbit",
                MoonlightSpatialActionKind.SleepCuddle => "cuddle-orbit",
                _ => "magic-orbit"
            };

            float orbSize = kind switch
            {
                MoonlightSpatialActionKind.Cook => 0.13f,
                MoonlightSpatialActionKind.Play => 0.15f,
                MoonlightSpatialActionKind.Garden => 0.11f,
                MoonlightSpatialActionKind.Read => 0.09f,
                MoonlightSpatialActionKind.Care => 0.12f,
                MoonlightSpatialActionKind.SleepCuddle when state == "Cuddled" => 0.16f,
                _ => 0.10f
            };

            var shader = Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
            if (ShouldCreateOpaqueActionOrb(kind))
            {
                _actionOrb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _actionOrb.name = $"ActionOrb-{ActiveEffectName}";
                _actionOrb.transform.localScale = Vector3.one * orbSize;
                var collider = _actionOrb.GetComponent<Collider>();
                if (collider != null) Destroy(collider);

                _actionMaterial = new Material(shader);
                _actionMaterial.color = color;
                if (_actionMaterial.HasProperty("_EmissionColor"))
                {
                    _actionMaterial.EnableKeyword("_EMISSION");
                    _actionMaterial.SetColor("_EmissionColor", color * 1.7f);
                }
                var renderer = _actionOrb.GetComponent<Renderer>();
                renderer.sharedMaterial = _actionMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            ActionVisualSignature = ActionVisualSignatureFor(kind, _activityStep, state);
            ActionVisualSignatureMarker = ActionVisualSignatureMarkerFor(kind, _activityStep, state);
            _actionAccent = new GameObject();
            _actionAccent.name = $"ActionAccent-{ActionVisualSignature}";
            CreateActionAccentMaterials(shader, color);
            CreateActionAccentParts(kind, _activityStep);
            UpdateActionAccent(kind, 0f);

            if (_actionOrb != null)
            {
                _actionTrail = _actionOrb.AddComponent<TrailRenderer>();
                _actionTrail.time = Mathf.Min(0.75f, duration * 0.55f);
                _actionTrail.minVertexDistance = 0.015f;
                _actionTrail.startWidth = 0.055f;
                _actionTrail.endWidth = 0f;
                _actionTrail.startColor = color;
                _actionTrail.endColor = new Color(color.r, color.g, color.b, 0f);
                _trailMaterial = CreateTransparentMaterial(Color.white);
                _actionTrail.sharedMaterial = _trailMaterial;
            }
            UpdateActionOrb(kind, state, 0f);
        }

        void UpdateActionOrb(MoonlightSpatialActionKind kind, string state, float t)
        {
            if (kind is MoonlightSpatialActionKind.Play or MoonlightSpatialActionKind.Feed)
            {
                UpdateActionAccent(kind, t);
                return;
            }
            if (_actionOrb == null) return;
            Vector3 center = transform.position + new Vector3(0f, 1.15f, 0f);
            float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

            switch (kind)
            {
                case MoonlightSpatialActionKind.Cook:
                    Vector3 cookApproach = Vector3.Lerp(center + new Vector3(0f, 0.26f, 0f),
                        _actionContactPoint, Ease(t, 0.04f, 0.34f));
                    float cookLift = Mathf.Sin(Ease(t, 0.04f, 0.34f) * Mathf.PI) * 0.20f;
                    _actionOrb.transform.position = cookApproach + Vector3.up * cookLift;
                    _actionOrb.transform.localScale = Vector3.one * (0.09f + ActionContactWeight * 0.075f);
                    _actionOrb.SetActive(t < 0.90f);
                    break;
                case MoonlightSpatialActionKind.Garden:
                    Vector3 gardenApproach = Vector3.Lerp(center + Vector3.down * 0.32f,
                        _actionContactPoint, Ease(t, 0.04f, 0.36f));
                    gardenApproach.y += Mathf.Sin(Ease(t, 0.04f, 0.36f) * Mathf.PI) * 0.16f;
                    _actionOrb.transform.position = gardenApproach;
                    _actionOrb.transform.localScale = Vector3.one * (0.08f + ActionContactWeight * 0.07f);
                    break;
                case MoonlightSpatialActionKind.Read:
                    float pageArc = Mathf.Sin(Ease(t, 0.04f, 0.40f) * Mathf.PI);
                    _actionOrb.transform.position = Vector3.Lerp(center + new Vector3(-0.24f, 0.10f, 0f),
                        _actionContactPoint, Ease(t, 0.04f, 0.40f)) + Vector3.up * pageArc * 0.20f;
                    _actionOrb.transform.localScale = Vector3.one * (0.07f + ActionContactWeight * 0.05f);
                    break;
                case MoonlightSpatialActionKind.Care:
                    Vector3 careOrbOffset = Mathf.Clamp(_activityStep, 0, 3) switch
                    {
                        0 => new Vector3(0f, 0.06f, 0f),
                        1 => new Vector3(0f, 0.04f, 0.02f),
                        2 => new Vector3(0f, 0.05f, 0f),
                        _ => new Vector3(0f, 0.02f, 0.03f)
                    };
                    _actionOrb.transform.position = _actionContactPoint + careOrbOffset;
                    float carePulse = 1f + Mathf.Sin(t * Mathf.PI * 6f) * 0.10f;
                    _actionOrb.transform.localScale = Vector3.one *
                        (0.075f + ActionContactWeight * 0.055f) * carePulse;
                    break;
                case MoonlightSpatialActionKind.SleepCuddle:
                    float orbit = eased * Mathf.PI * (state == "Resting" ? 2.2f : 3.2f);
                    float radius = state == "Resting" ? 0.48f : 0.52f;
                    _actionOrb.transform.position = center + new Vector3(
                        Mathf.Cos(orbit) * radius,
                        Mathf.Lerp(0.05f, state == "Resting" ? 1.05f : 0.55f, eased),
                        Mathf.Sin(orbit) * radius * 0.45f);
                    float startScale = state == "Cuddled" ? 0.16f : 0.11f;
                    _actionOrb.transform.localScale = Vector3.one * Mathf.Lerp(startScale, 0.045f, eased);
                    break;
            }
            UpdateActionAccent(kind, t);
        }

        void UpdateActionAccent(MoonlightSpatialActionKind kind, float t)
        {
            if (_actionAccent == null) return;
            bool contactProp = UsesContactProp(kind);
            if (!contactProp && _actionOrb == null) return;
            Vector3 anchor = contactProp
                ? _actionContactPoint
                : _actionOrb.transform.position + ActionAccentWorldOffsetFor(kind);
            _actionAccent.transform.position = anchor;

            Quaternion facing = CameraFacingRotation(anchor);
            _actionAccent.transform.rotation = facing *
                ActionAccentRotationFor(kind, _activityStep, t);

            float contactEnergy = _contactPhaseIndex switch
            {
                1 => ActionContactWeight * 0.55f,
                2 => ActionContactWeight,
                3 => Mathf.Max(0.25f, ActionContactWeight * 0.70f),
                _ => 0f
            };
            float qualityEnergy = ActionQualityAccentEnergyFor(ActionQualityTier);
            for (int i = 0; i < _actionAccentParts.Count; i++)
            {
                float wave = Mathf.Sin((Mathf.Clamp01(t) * 2f + i * 0.23f) * Mathf.PI * 2f);
                Transform part = _actionAccentParts[i];
                part.localPosition = _actionAccentBasePositions[i] +
                    Vector3.up * wave * (0.006f + contactEnergy * 0.012f) * qualityEnergy;
                part.localRotation = _actionAccentBaseRotations[i] *
                    Quaternion.Euler(0f, contactEnergy * wave * 8f * qualityEnergy,
                        (wave * (3f + i) + contactEnergy * (i % 2 == 0 ? 10f : -10f)) *
                        qualityEnergy);
                part.localScale = _actionAccentBaseScales[i] *
                    (1f + contactEnergy * (0.06f + i * 0.01f) * qualityEnergy);
            }

            ActionAccentContactDistance = contactProp
                ? Vector3.Distance(_actionAccent.transform.position, _actionContactPoint)
                : 0f;
            if (kind != MoonlightSpatialActionKind.Feed || ActionAccentRendererCount == 0)
                RefreshActionAccentMetrics();
        }

        void DestroyActionOrb()
        {
            ActiveEffectName = "";
            ActionVisualSignature = "";
            ActionVisualSignatureMarker = "";
            ActionAccentRendererCount = 0;
            ActionAccentColliderCount = 0;
            ActionAccentMaterialCount = 0;
            ActionAccentBoundsSize = Vector3.zero;
            ActionAccentWorldExtent = 0f;
            ActionAccentContactDistance = 0f;
            if (_actionAccent != null) Destroy(_actionAccent);
            if (_actionOrb != null) Destroy(_actionOrb);
            if (_actionMaterial != null) Destroy(_actionMaterial);
            foreach (Material material in _actionAccentMaterials)
                if (material != null) Destroy(material);
            if (_trailMaterial != null) Destroy(_trailMaterial);
            _actionOrb = null;
            _actionAccent = null;
            _actionTrail = null;
            _actionMaterial = null;
            _trailMaterial = null;
            _actionAccentMaterials.Clear();
            _actionAccentParts.Clear();
            _actionAccentBasePositions.Clear();
            _actionAccentBaseScales.Clear();
            _actionAccentBaseRotations.Clear();
        }

        public static string ActionVisualSignatureFor(MoonlightSpatialActionKind kind,
                                                       string state) =>
            ActionVisualSignatureFor(kind, 0, state);

        public static string ActionVisualSignatureFor(MoonlightSpatialActionKind kind,
                                                       int activityStep,
                                                       string state = "") =>
            (kind, Mathf.Clamp(activityStep, 0, 3)) switch
        {
            (MoonlightSpatialActionKind.Cook, 0) => "cook-scoop",
            (MoonlightSpatialActionKind.Cook, 1) => "cook-whisk",
            (MoonlightSpatialActionKind.Cook, 2) => "cook-cookie-tray",
            (MoonlightSpatialActionKind.Cook, _) => "cook-icing",
            (MoonlightSpatialActionKind.Play, 0) => "play-star-ball",
            (MoonlightSpatialActionKind.Play, 1) => "play-orbit",
            (MoonlightSpatialActionKind.Play, 2) => "play-impact",
            (MoonlightSpatialActionKind.Play, _) => "play-catch-star",
            (MoonlightSpatialActionKind.Garden, 0) => "garden-seed",
            (MoonlightSpatialActionKind.Garden, 1) => "garden-watering",
            (MoonlightSpatialActionKind.Garden, 2) => "garden-droplets",
            (MoonlightSpatialActionKind.Garden, _) => "garden-bloom",
            (MoonlightSpatialActionKind.Read, 0) => "read-open-book",
            (MoonlightSpatialActionKind.Read, 1) => "read-page-fan",
            (MoonlightSpatialActionKind.Read, 2) => "read-bookmark",
            (MoonlightSpatialActionKind.Read, _) => "read-memory-motes",
            (MoonlightSpatialActionKind.Care, 0) => "care-warm-towel",
            (MoonlightSpatialActionKind.Care, 1) => "care-bubble-brush",
            (MoonlightSpatialActionKind.Care, 2) => "care-moon-comb",
            (MoonlightSpatialActionKind.Care, _) => "care-mirror-glow",
            (MoonlightSpatialActionKind.Feed, _) => "feed-bowl-to-mouth",
            (MoonlightSpatialActionKind.SleepCuddle, _) when state == "Resting" => "dream-moon-pair",
            (MoonlightSpatialActionKind.SleepCuddle, _) => "cuddle-heart-pair",
            _ => "magic-accent"
        };

        public static string ActionVisualSignatureMarkerFor(MoonlightSpatialActionKind kind,
                                                             int activityStep,
                                                             string state = "")
        {
            string signature = ActionVisualSignatureFor(kind, activityStep, state)
                .ToUpperInvariant().Replace('-', '_');
            return IsStepSpecificActivity(kind)
                ? $"MOONLIGHT_ACTION_PROP_{kind.ToString().ToUpperInvariant()}_STEP_{Mathf.Clamp(activityStep, 0, 3) + 1}_{signature}"
                : $"MOONLIGHT_ACTION_PROP_{signature}";
        }

        struct ActionAccentPartSpec
        {
            public PrimitiveType Primitive;
            public Vector3 Position;
            public Vector3 Scale;
            public Vector3 Euler;
            public int MaterialSlot;

            public ActionAccentPartSpec(PrimitiveType primitive, Vector3 position,
                Vector3 scale, Vector3 euler, int materialSlot)
            {
                Primitive = primitive;
                Position = position;
                Scale = scale;
                Euler = euler;
                MaterialSlot = materialSlot;
            }
        }

        static ActionAccentPartSpec Part(PrimitiveType primitive, Vector3 position,
            Vector3 scale, float zRotation, int materialSlot) =>
            new(primitive, position, scale, new Vector3(0f, 0f, zRotation), materialSlot);

        static ActionAccentPartSpec[] ActionAccentLayoutFor(MoonlightSpatialActionKind kind,
                                                             int activityStep)
        {
            int step = Mathf.Clamp(activityStep, 0, 3);
            return (kind, step) switch
            {
                (MoonlightSpatialActionKind.Cook, 0) => new[]
                {
                    Part(PrimitiveType.Sphere, new Vector3(-0.04f, 0f, 0f), new Vector3(0.22f, 0.10f, 0.12f), 0f, 0),
                    Part(PrimitiveType.Capsule, new Vector3(0.11f, 0.10f, 0f), new Vector3(0.035f, 0.13f, 0.035f), -40f, 1),
                    Part(PrimitiveType.Sphere, new Vector3(-0.08f, 0.035f, -0.03f), Vector3.one * 0.065f, 0f, 2),
                    Part(PrimitiveType.Sphere, new Vector3(0.00f, 0.045f, -0.03f), Vector3.one * 0.055f, 0f, 2)
                },
                (MoonlightSpatialActionKind.Cook, 1) => new[]
                {
                    Part(PrimitiveType.Capsule, new Vector3(0f, 0.13f, 0f), new Vector3(0.04f, 0.12f, 0.04f), 0f, 1),
                    Part(PrimitiveType.Capsule, new Vector3(-0.055f, -0.06f, 0f), new Vector3(0.018f, 0.10f, 0.018f), -18f, 0),
                    Part(PrimitiveType.Capsule, new Vector3(0f, -0.07f, 0f), new Vector3(0.018f, 0.11f, 0.018f), 0f, 0),
                    Part(PrimitiveType.Capsule, new Vector3(0.055f, -0.06f, 0f), new Vector3(0.018f, 0.10f, 0.018f), 18f, 0)
                },
                (MoonlightSpatialActionKind.Cook, 2) => new[]
                {
                    Part(PrimitiveType.Cube, Vector3.zero, new Vector3(0.36f, 0.035f, 0.23f), 0f, 1),
                    Part(PrimitiveType.Sphere, new Vector3(-0.11f, 0.04f, -0.02f), new Vector3(0.085f, 0.045f, 0.085f), 0f, 2),
                    Part(PrimitiveType.Sphere, new Vector3(0f, 0.04f, 0.02f), new Vector3(0.085f, 0.045f, 0.085f), 0f, 2),
                    Part(PrimitiveType.Sphere, new Vector3(0.11f, 0.04f, -0.02f), new Vector3(0.085f, 0.045f, 0.085f), 0f, 2)
                },
                (MoonlightSpatialActionKind.Cook, _) => new[]
                {
                    Part(PrimitiveType.Sphere, new Vector3(-0.06f, -0.07f, 0f), new Vector3(0.20f, 0.065f, 0.15f), 0f, 1),
                    Part(PrimitiveType.Capsule, new Vector3(0.06f, 0.10f, 0f), new Vector3(0.055f, 0.13f, 0.055f), -20f, 0),
                    Part(PrimitiveType.Sphere, new Vector3(-0.10f, 0.005f, -0.04f), Vector3.one * 0.065f, 0f, 2),
                    Part(PrimitiveType.Sphere, new Vector3(0f, 0.015f, -0.04f), Vector3.one * 0.060f, 0f, 2)
                },
                (MoonlightSpatialActionKind.Play, 0) => StarBallLayout(false),
                (MoonlightSpatialActionKind.Play, 1) => new[]
                {
                    Part(PrimitiveType.Cube, Vector3.zero, new Vector3(0.38f, 0.018f, 0.025f), 0f, 0),
                    Part(PrimitiveType.Cube, Vector3.zero, new Vector3(0.34f, 0.018f, 0.025f), 60f, 0),
                    Part(PrimitiveType.Cube, Vector3.zero, new Vector3(0.34f, 0.018f, 0.025f), -60f, 0),
                    Part(PrimitiveType.Sphere, new Vector3(0.19f, 0f, -0.01f), Vector3.one * 0.055f, 0f, 2)
                },
                (MoonlightSpatialActionKind.Play, 2) => ImpactLayout(),
                (MoonlightSpatialActionKind.Play, _) => StarBallLayout(true),
                (MoonlightSpatialActionKind.Garden, 0) => new[]
                {
                    Part(PrimitiveType.Cube, new Vector3(0f, -0.10f, 0f), new Vector3(0.32f, 0.055f, 0.16f), 0f, 1),
                    Part(PrimitiveType.Sphere, new Vector3(0f, -0.025f, 0f), new Vector3(0.085f, 0.12f, 0.065f), 18f, 2),
                    Part(PrimitiveType.Sphere, new Vector3(-0.07f, 0.075f, 0f), new Vector3(0.13f, 0.065f, 0.055f), -28f, 0),
                    Part(PrimitiveType.Sphere, new Vector3(0.07f, 0.075f, 0f), new Vector3(0.13f, 0.065f, 0.055f), 28f, 0)
                },
                (MoonlightSpatialActionKind.Garden, 1) => new[]
                {
                    Part(PrimitiveType.Cylinder, new Vector3(-0.05f, 0f, 0f), new Vector3(0.10f, 0.08f, 0.10f), 0f, 1),
                    Part(PrimitiveType.Capsule, new Vector3(0.10f, 0.06f, 0f), new Vector3(0.025f, 0.12f, 0.025f), -58f, 0),
                    Part(PrimitiveType.Capsule, new Vector3(-0.14f, 0.07f, 0f), new Vector3(0.025f, 0.09f, 0.025f), 30f, 0),
                    Part(PrimitiveType.Sphere, new Vector3(0.22f, -0.04f, 0f), new Vector3(0.045f, 0.07f, 0.045f), 0f, 2)
                },
                (MoonlightSpatialActionKind.Garden, 2) => new[]
                {
                    Part(PrimitiveType.Capsule, new Vector3(-0.12f, -0.02f, 0f), new Vector3(0.025f, 0.10f, 0.025f), 0f, 1),
                    Part(PrimitiveType.Sphere, new Vector3(-0.02f, 0.09f, 0f), new Vector3(0.055f, 0.09f, 0.055f), 0f, 0),
                    Part(PrimitiveType.Sphere, new Vector3(0.08f, 0.015f, 0f), new Vector3(0.055f, 0.09f, 0.055f), 0f, 0),
                    Part(PrimitiveType.Sphere, new Vector3(0.17f, -0.07f, 0f), new Vector3(0.050f, 0.08f, 0.050f), 0f, 2)
                },
                (MoonlightSpatialActionKind.Garden, _) => BloomLayout(),
                (MoonlightSpatialActionKind.Read, 0) => new[]
                {
                    Part(PrimitiveType.Cube, new Vector3(-0.10f, 0f, 0f), new Vector3(0.22f, 0.025f, 0.17f), -12f, 0),
                    Part(PrimitiveType.Cube, new Vector3(0.10f, 0f, 0f), new Vector3(0.22f, 0.025f, 0.17f), 12f, 0),
                    Part(PrimitiveType.Capsule, new Vector3(0f, -0.02f, -0.01f), new Vector3(0.018f, 0.11f, 0.018f), 90f, 1),
                    Part(PrimitiveType.Cube, new Vector3(0f, -0.035f, 0.035f), new Vector3(0.40f, 0.018f, 0.18f), 0f, 2)
                },
                (MoonlightSpatialActionKind.Read, 1) => PageFanLayout(),
                (MoonlightSpatialActionKind.Read, 2) => new[]
                {
                    Part(PrimitiveType.Cube, new Vector3(-0.08f, 0f, 0f), new Vector3(0.20f, 0.022f, 0.16f), -8f, 0),
                    Part(PrimitiveType.Cube, new Vector3(0.08f, 0f, 0f), new Vector3(0.20f, 0.022f, 0.16f), 8f, 0),
                    Part(PrimitiveType.Cube, new Vector3(0.03f, -0.09f, -0.03f), new Vector3(0.055f, 0.20f, 0.018f), 4f, 2)
                },
                (MoonlightSpatialActionKind.Read, _) => new[]
                {
                    Part(PrimitiveType.Sphere, new Vector3(-0.15f, -0.05f, 0f), Vector3.one * 0.060f, 0f, 0),
                    Part(PrimitiveType.Sphere, new Vector3(-0.05f, 0.08f, 0f), Vector3.one * 0.075f, 0f, 2),
                    Part(PrimitiveType.Sphere, new Vector3(0.07f, -0.01f, 0f), Vector3.one * 0.055f, 0f, 0),
                    Part(PrimitiveType.Sphere, new Vector3(0.17f, 0.10f, 0f), Vector3.one * 0.065f, 0f, 1)
                },
                (MoonlightSpatialActionKind.Care, 0) => new[]
                {
                    Part(PrimitiveType.Cube, Vector3.zero, new Vector3(0.38f, 0.055f, 0.24f), -6f, 0),
                    Part(PrimitiveType.Cube, new Vector3(-0.03f, 0.045f, -0.01f), new Vector3(0.31f, 0.040f, 0.20f), 5f, 1),
                    Part(PrimitiveType.Capsule, new Vector3(-0.16f, 0.075f, 0f), new Vector3(0.025f, 0.065f, 0.025f), 74f, 2),
                    Part(PrimitiveType.Capsule, new Vector3(0.15f, 0.070f, 0f), new Vector3(0.025f, 0.060f, 0.025f), -74f, 2)
                },
                (MoonlightSpatialActionKind.Care, 1) => new[]
                {
                    Part(PrimitiveType.Capsule, new Vector3(-0.08f, -0.01f, 0f), new Vector3(0.045f, 0.16f, 0.045f), -34f, 1),
                    Part(PrimitiveType.Cube, new Vector3(0.03f, 0.11f, 0f), new Vector3(0.16f, 0.065f, 0.11f), -34f, 0),
                    Part(PrimitiveType.Sphere, new Vector3(-0.15f, 0.13f, -0.01f), Vector3.one * 0.075f, 0f, 2),
                    Part(PrimitiveType.Sphere, new Vector3(0.14f, 0.18f, -0.02f), Vector3.one * 0.060f, 0f, 0),
                    Part(PrimitiveType.Sphere, new Vector3(0.19f, 0.05f, -0.01f), Vector3.one * 0.050f, 0f, 2)
                },
                (MoonlightSpatialActionKind.Care, 2) => new[]
                {
                    Part(PrimitiveType.Cube, new Vector3(0f, 0.08f, 0f), new Vector3(0.36f, 0.055f, 0.075f), -5f, 1),
                    Part(PrimitiveType.Cube, new Vector3(-0.12f, -0.015f, 0f), new Vector3(0.025f, 0.17f, 0.045f), 2f, 0),
                    Part(PrimitiveType.Cube, new Vector3(0f, -0.020f, 0f), new Vector3(0.025f, 0.18f, 0.045f), -3f, 0),
                    Part(PrimitiveType.Cube, new Vector3(0.12f, -0.010f, 0f), new Vector3(0.025f, 0.16f, 0.045f), -8f, 0),
                    Part(PrimitiveType.Sphere, new Vector3(-0.16f, 0.10f, -0.02f), new Vector3(0.10f, 0.075f, 0.035f), 0f, 2)
                },
                (MoonlightSpatialActionKind.Care, _) => new[]
                {
                    Part(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.25f, 0.025f, 0.31f), 90f, 1),
                    Part(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.31f, 0.018f, 0.37f), 90f, 0),
                    Part(PrimitiveType.Sphere, new Vector3(-0.23f, 0.15f, -0.02f), Vector3.one * 0.055f, 0f, 2),
                    Part(PrimitiveType.Sphere, new Vector3(0.22f, 0.12f, -0.02f), Vector3.one * 0.050f, 0f, 2),
                    Part(PrimitiveType.Sphere, new Vector3(0f, 0.25f, -0.02f), Vector3.one * 0.045f, 0f, 0)
                },
                (MoonlightSpatialActionKind.Feed, _) => new[]
                {
                    Part(PrimitiveType.Cylinder, new Vector3(0f, -0.035f, 0f), new Vector3(0.23f, 0.055f, 0.18f), 0f, 1),
                    Part(PrimitiveType.Sphere, new Vector3(0f, 0.035f, -0.01f), new Vector3(0.17f, 0.045f, 0.12f), 0f, 2),
                    Part(PrimitiveType.Capsule, new Vector3(0.16f, 0.07f, 0f), new Vector3(0.025f, 0.12f, 0.025f), -48f, 0)
                },
                _ => new[]
                {
                    Part(PrimitiveType.Sphere, new Vector3(-0.08f, 0f, 0f), Vector3.one * 0.16f, 0f, 0),
                    Part(PrimitiveType.Sphere, new Vector3(0.08f, 0.04f, 0f), Vector3.one * 0.13f, 0f, 1),
                    Part(PrimitiveType.Sphere, new Vector3(0f, -0.08f, 0f), Vector3.one * 0.09f, 0f, 2)
                }
            };
        }

        static ActionAccentPartSpec[] StarBallLayout(bool caught)
        {
            float squeeze = caught ? 0.82f : 1f;
            return new[]
            {
                Part(PrimitiveType.Cube, new Vector3(0f, 0.14f * squeeze, 0f), new Vector3(0.05f, 0.17f, 0.035f), 0f, 0),
                Part(PrimitiveType.Cube, new Vector3(0.14f * squeeze, 0f, 0f), new Vector3(0.17f, 0.05f, 0.035f), 0f, 0),
                Part(PrimitiveType.Cube, new Vector3(0f, -0.14f * squeeze, 0f), new Vector3(0.05f, 0.17f, 0.035f), 0f, 2),
                Part(PrimitiveType.Cube, new Vector3(-0.14f * squeeze, 0f, 0f), new Vector3(0.17f, 0.05f, 0.035f), 0f, 2)
            };
        }

        static ActionAccentPartSpec[] ImpactLayout() => new[]
        {
            Part(PrimitiveType.Capsule, new Vector3(0f, 0.14f, 0f), new Vector3(0.022f, 0.085f, 0.022f), 0f, 0),
            Part(PrimitiveType.Capsule, new Vector3(0.14f, 0f, 0f), new Vector3(0.022f, 0.085f, 0.022f), -90f, 2),
            Part(PrimitiveType.Capsule, new Vector3(0f, -0.14f, 0f), new Vector3(0.022f, 0.085f, 0.022f), 0f, 0),
            Part(PrimitiveType.Capsule, new Vector3(-0.14f, 0f, 0f), new Vector3(0.022f, 0.085f, 0.022f), 90f, 2)
        };

        static ActionAccentPartSpec[] BloomLayout() => new[]
        {
            Part(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.11f, 0f, 2),
            Part(PrimitiveType.Sphere, new Vector3(0f, 0.105f, 0f), new Vector3(0.12f, 0.16f, 0.055f), 0f, 0),
            Part(PrimitiveType.Sphere, new Vector3(0.105f, 0f, 0f), new Vector3(0.16f, 0.12f, 0.055f), 0f, 1),
            Part(PrimitiveType.Sphere, new Vector3(0f, -0.105f, 0f), new Vector3(0.12f, 0.16f, 0.055f), 0f, 0),
            Part(PrimitiveType.Sphere, new Vector3(-0.105f, 0f, 0f), new Vector3(0.16f, 0.12f, 0.055f), 0f, 1)
        };

        static ActionAccentPartSpec[] PageFanLayout() => new[]
        {
            Part(PrimitiveType.Cube, new Vector3(-0.09f, 0f, 0f), new Vector3(0.23f, 0.020f, 0.16f), -18f, 1),
            Part(PrimitiveType.Cube, new Vector3(-0.03f, 0.025f, -0.01f), new Vector3(0.23f, 0.018f, 0.16f), -6f, 0),
            Part(PrimitiveType.Cube, new Vector3(0.04f, 0.035f, -0.02f), new Vector3(0.23f, 0.018f, 0.16f), 7f, 0),
            Part(PrimitiveType.Cube, new Vector3(0.10f, 0.015f, -0.03f), new Vector3(0.23f, 0.020f, 0.16f), 19f, 2)
        };

        void CreateActionAccentMaterials(Shader shader, Color color)
        {
            Color[] palette =
            {
                Color.Lerp(color, Color.white, 0.52f),
                Color.Lerp(color, new Color(0.18f, 0.22f, 0.32f), 0.22f),
                Color.Lerp(color, new Color(1f, 0.62f, 0.28f), 0.34f)
            };
            foreach (Color paletteColor in palette)
            {
                var material = new Material(shader);
                material.color = paletteColor;
                if (material.HasProperty("_EmissionColor"))
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", paletteColor * 1.18f);
                }
                _actionAccentMaterials.Add(material);
            }
        }

        void CreateActionAccentParts(MoonlightSpatialActionKind kind, int activityStep)
        {
            ActionAccentPartSpec[] layout = ActionAccentLayoutFor(kind, activityStep);
            for (int i = 0; i < layout.Length; i++)
            {
                ActionAccentPartSpec spec = layout[i];
                var part = new GameObject($"{ActionVisualSignature}-part-{i + 1}");
                part.transform.SetParent(_actionAccent.transform, false);
                part.transform.localPosition = spec.Position;
                part.transform.localScale = spec.Scale;
                part.transform.localRotation = Quaternion.Euler(spec.Euler);

                var meshFilter = part.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = ActionAccentMeshFor(spec.Primitive);
                var partRenderer = part.AddComponent<MeshRenderer>();
                int materialSlot = Mathf.Clamp(spec.MaterialSlot, 0, _actionAccentMaterials.Count - 1);
                partRenderer.sharedMaterial = _actionAccentMaterials[materialSlot];
                partRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                partRenderer.receiveShadows = false;

                _actionAccentParts.Add(part.transform);
                _actionAccentBasePositions.Add(spec.Position);
                _actionAccentBaseScales.Add(spec.Scale);
                _actionAccentBaseRotations.Add(Quaternion.Euler(spec.Euler));
            }
        }

        static Mesh ActionAccentMeshFor(PrimitiveType primitive)
        {
            if (ActionAccentPrimitiveMeshes.TryGetValue(primitive, out Mesh cachedMesh) &&
                cachedMesh != null)
                return cachedMesh;

            GameObject source = GameObject.CreatePrimitive(primitive);
            source.SetActive(false);
            Mesh mesh = source.GetComponent<MeshFilter>().sharedMesh;
            ActionAccentPrimitiveMeshes[primitive] = mesh;
            Object.Destroy(source);
            return mesh;
        }

        void RefreshActionAccentMetrics()
        {
            if (_actionAccent == null) return;
            Renderer[] renderers = _actionAccent.GetComponentsInChildren<Renderer>(true);
            var materials = new HashSet<Material>();
            Bounds bounds = default;
            bool hasBounds = false;
            foreach (Renderer accentRenderer in renderers)
            {
                if (!accentRenderer.enabled) continue;
                if (!hasBounds)
                {
                    bounds = accentRenderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(accentRenderer.bounds);
                }
                foreach (Material material in accentRenderer.sharedMaterials)
                    if (material != null) materials.Add(material);
            }

            ActionAccentRendererCount = renderers.Length;
            ActionAccentColliderCount = _actionAccent.GetComponentsInChildren<Collider>(true).Length;
            ActionAccentMaterialCount = materials.Count;
            ActionAccentBoundsSize = hasBounds ? bounds.size : Vector3.zero;
            ActionAccentWorldExtent = MaximumAxis(ActionAccentBoundsSize);
        }

        static bool IsStepSpecificActivity(MoonlightSpatialActionKind kind) =>
            kind is MoonlightSpatialActionKind.Cook or MoonlightSpatialActionKind.Play or
                MoonlightSpatialActionKind.Garden or MoonlightSpatialActionKind.Read or
                MoonlightSpatialActionKind.Care;

        static bool UsesContactProp(MoonlightSpatialActionKind kind) =>
            IsStepSpecificActivity(kind) || kind == MoonlightSpatialActionKind.Feed;

        static Quaternion CameraFacingRotation(Vector3 position)
        {
            Camera camera = Camera.main;
            if (camera == null) return Quaternion.identity;
            Vector3 toCamera = camera.transform.position - position;
            return toCamera.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(toCamera.normalized, Vector3.up)
                : Quaternion.identity;
        }

        static Vector3 ActionAccentWorldOffsetFor(MoonlightSpatialActionKind kind) => kind switch
        {
            MoonlightSpatialActionKind.Cook => new Vector3(0f, 0.12f, 0f),
            MoonlightSpatialActionKind.Garden => new Vector3(0.10f, 0.06f, 0f),
            MoonlightSpatialActionKind.Read => new Vector3(0f, 0.05f, 0f),
            MoonlightSpatialActionKind.SleepCuddle => new Vector3(0.11f, 0.04f, 0f),
            _ => Vector3.zero
        };

        static float MaximumAxis(Vector3 value) =>
            Mathf.Max(value.x, Mathf.Max(value.y, value.z));

        static Quaternion ActionAccentRotationFor(MoonlightSpatialActionKind kind, int activityStep, float t)
        {
            float wave = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI * 2f);
            int step = Mathf.Clamp(activityStep, 0, 3);
            return (kind, step) switch
            {
                (MoonlightSpatialActionKind.Cook, 1) => Quaternion.Euler(0f, 0f, t * 260f),
                (MoonlightSpatialActionKind.Cook, _) => Quaternion.Euler(0f, 0f, 8f + wave * 10f),
                (MoonlightSpatialActionKind.Play, 1) => Quaternion.Euler(0f, 0f, t * 300f),
                (MoonlightSpatialActionKind.Play, 2) => Quaternion.Euler(0f, 0f, wave * 18f),
                (MoonlightSpatialActionKind.Play, _) => Quaternion.Euler(0f, 0f, 18f + wave * 12f),
                (MoonlightSpatialActionKind.Garden, 2) => Quaternion.Euler(0f, 0f, -8f + wave * 8f),
                (MoonlightSpatialActionKind.Garden, _) => Quaternion.Euler(0f, 0f, wave * 10f),
                (MoonlightSpatialActionKind.Read, 1) => Quaternion.Euler(0f, wave * 7f, wave * 14f),
                (MoonlightSpatialActionKind.Read, _) => Quaternion.Euler(0f, wave * 5f, wave * 7f),
                (MoonlightSpatialActionKind.Care, 0) => Quaternion.Euler(0f, wave * 4f, -6f + wave * 5f),
                (MoonlightSpatialActionKind.Care, 1) => Quaternion.Euler(0f, 0f, t * 240f),
                (MoonlightSpatialActionKind.Care, 2) => Quaternion.Euler(0f, wave * 8f, -12f + wave * 16f),
                (MoonlightSpatialActionKind.Care, _) => Quaternion.Euler(0f, wave * 5f, wave * 6f),
                (MoonlightSpatialActionKind.Feed, _) => Quaternion.Euler(0f, wave * 6f, -8f + t * 24f),
                (MoonlightSpatialActionKind.SleepCuddle, _) => Quaternion.Euler(0f, 0f, wave * 18f),
                _ => Quaternion.identity
            };
        }

        public static bool ValidateActionVisualSignatureContract(out string detail)
        {
            string[] expectedSignatures =
            {
                "cook-scoop", "cook-whisk", "cook-cookie-tray", "cook-icing",
                "play-star-ball", "play-orbit", "play-impact", "play-catch-star",
                "garden-seed", "garden-watering", "garden-droplets", "garden-bloom",
                "read-open-book", "read-page-fan", "read-bookmark", "read-memory-motes",
                "care-warm-towel", "care-bubble-brush", "care-moon-comb", "care-mirror-glow"
            };
            var signatures = new HashSet<string>();
            var markers = new HashSet<string>();
            var primitives = new HashSet<PrimitiveType>();
            int minimumRenderers = int.MaxValue;
            int maximumRenderers = 0;
            int maximumMaterials = 0;
            float minimumExtent = float.PositiveInfinity;
            float maximumExtent = 0f;
            bool correctSignatures = true;
            bool validLayouts = true;
            int sampleIndex = 0;
            MoonlightSpatialActionKind[] activities =
            {
                MoonlightSpatialActionKind.Cook,
                MoonlightSpatialActionKind.Play,
                MoonlightSpatialActionKind.Garden,
                MoonlightSpatialActionKind.Read,
                MoonlightSpatialActionKind.Care
            };
            foreach (MoonlightSpatialActionKind activity in activities)
            for (int step = 0; step < 4; step++)
            {
                string signature = ActionVisualSignatureFor(activity, step);
                correctSignatures &= signature == expectedSignatures[sampleIndex++];
                signatures.Add(signature);
                markers.Add(ActionVisualSignatureMarkerFor(activity, step));
                ActionAccentPartSpec[] layout = ActionAccentLayoutFor(activity, step);
                var materialSlots = new HashSet<int>();
                foreach (ActionAccentPartSpec part in layout)
                {
                    primitives.Add(part.Primitive);
                    materialSlots.Add(part.MaterialSlot);
                    validLayouts &= part.MaterialSlot >= 0 && part.MaterialSlot < 5;
                }
                float extent = ApproximateLayoutExtent(layout);
                minimumRenderers = Mathf.Min(minimumRenderers, layout.Length);
                maximumRenderers = Mathf.Max(maximumRenderers, layout.Length);
                maximumMaterials = Mathf.Max(maximumMaterials, materialSlots.Count);
                minimumExtent = Mathf.Min(minimumExtent, extent);
                maximumExtent = Mathf.Max(maximumExtent, extent);
                validLayouts &= layout.Length >= 3 && layout.Length <= 5 &&
                    materialSlots.Count <= 5 && extent >= MinimumActionAccentExtent &&
                    extent <= MaximumActionAccentExtent;
            }
            detail = $"activities={activities.Length}/5 signatures={signatures.Count}/20 markers={markers.Count}/20 " +
                $"correct={correctSignatures} renderers={minimumRenderers}-{maximumRenderers}/3-5 " +
                $"colliders=0 materials<={maximumMaterials}/5 primitives={primitives.Count} " +
                $"extent={minimumExtent:0.000}-{maximumExtent:0.000}/" +
                $"{MinimumActionAccentExtent:0.00}-{MaximumActionAccentExtent:0.00}";
            return correctSignatures && activities.Length == 5 && signatures.Count == 20 && markers.Count == 20 &&
                validLayouts && primitives.Count >= 3;
        }

        public static bool ValidateFeedVisualContract(out string detail)
        {
            ActionAccentPartSpec[] layout = ActionAccentLayoutFor(MoonlightSpatialActionKind.Feed, 0);
            var materialSlots = new HashSet<int>();
            foreach (ActionAccentPartSpec part in layout) materialSlots.Add(part.MaterialSlot);
            float extent = ApproximateLayoutExtent(layout);
            string signature = ActionVisualSignatureFor(MoonlightSpatialActionKind.Feed, 0);
            string marker = ActionVisualSignatureMarkerFor(MoonlightSpatialActionKind.Feed, 0);
            bool pass = signature == "feed-bowl-to-mouth" &&
                marker == "MOONLIGHT_ACTION_PROP_FEED_BOWL_TO_MOUTH" &&
                MotionProfileFor(MoonlightSpatialActionKind.Feed, 0) == "feed-bowl-to-mouth" &&
                !ShouldCreateOpaqueActionOrb(MoonlightSpatialActionKind.Feed) &&
                layout.Length == FeedRendererBudget && materialSlots.Count <= FeedMaterialBudget &&
                FeedVisualObjectBudget == layout.Length + 1 && FeedLightBudget == 0 &&
                extent >= MinimumActionAccentExtent && extent <= MaximumActionAccentExtent;
            detail = $"signature={signature} marker={marker} motion=feed-bowl-to-mouth " +
                $"objects={layout.Length + 1}/{FeedVisualObjectBudget} " +
                $"renderers={layout.Length}/{FeedRendererBudget} materials={materialSlots.Count}/<={FeedMaterialBudget} " +
                $"lights=0/{FeedLightBudget} opaqueOrb={ShouldCreateOpaqueActionOrb(MoonlightSpatialActionKind.Feed)} " +
                $"extent={extent:0.000}/{MinimumActionAccentExtent:0.00}-{MaximumActionAccentExtent:0.00}";
            return pass;
        }

        static float ApproximateLayoutExtent(ActionAccentPartSpec[] layout)
        {
            Vector3 minimum = new(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 maximum = new(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            foreach (ActionAccentPartSpec part in layout)
            {
                Vector3 half = part.Primitive is PrimitiveType.Capsule or PrimitiveType.Cylinder
                    ? new Vector3(part.Scale.x * 0.5f, part.Scale.y, part.Scale.z * 0.5f)
                    : part.Scale * 0.5f;
                minimum = Vector3.Min(minimum, part.Position - half);
                maximum = Vector3.Max(maximum, part.Position + half);
            }
            return MaximumAxis(maximum - minimum);
        }

        void OnDisable()
        {
            if (_running != null)
            {
                StopCoroutine(_running);
                _running = null;
            }
            if (_cameraHoldRoutine != null)
            {
                StopCoroutine(_cameraHoldRoutine);
                _cameraHoldRoutine = null;
            }
            if (_masteryFlashRoutine != null)
            {
                StopCoroutine(_masteryFlashRoutine);
                _masteryFlashRoutine = null;
            }
            RestoreVisualPose();
            _activityStage?.End();
            EndCameraFocus();
            DestroyActionOrb();
            ActionMotionProfile = "";
            ResetContactQA();
            _masteryCelebrationQueued = false;
            _running = null;
            ActionProgress01 = 0f;
        }

        void BeginCameraFocus(MoonlightSpatialActionKind kind)
        {
            if (kind == MoonlightSpatialActionKind.SleepCuddle)
            {
                _cameraFocusUsesStationAnchor = false;
                _cameraFocusAnchor = transform.position;
                _actionStageScale = Vector3.one;
                return;
            }

            var station = MoonlightActivityStation.FindNearestActive(kind, transform.position);
            _cameraFocusUsesStationAnchor = station != null;
            _cameraFocusAnchor = _cameraFocusUsesStationAnchor
                ? station.AnchorPosition
                : transform.position + FallbackActivityOffset(kind);
            _actionStageScale = _cameraFocusUsesStationAnchor
                ? station.AnchorScale
                : kind switch
                {
                    MoonlightSpatialActionKind.Cook => Vector3.one * 1.12f,
                    MoonlightSpatialActionKind.Play => Vector3.one * 1.10f,
                    MoonlightSpatialActionKind.Garden => Vector3.one * 1.08f,
                    MoonlightSpatialActionKind.Read => Vector3.one * 1.08f,
                    MoonlightSpatialActionKind.Care => Vector3.one * 1.06f,
                    _ => Vector3.one
                };

            if (_cameraController == null)
            {
                var mainCamera = Camera.main;
                if (mainCamera != null)
                    _cameraController = mainCamera.GetComponent<CameraController>();
            }

            if (_cameraController == null)
            {
                Debug.LogWarning($"[MoonlightCameraQA] activity-focus-unavailable kind={kind} " +
                    "marker=MOONLIGHT_ACTIVITY_FOCUS_UNAVAILABLE");
                return;
            }

            _cameraController.BeginActivityFocus(kind, transform.position, _cameraFocusAnchor,
                _cameraFocusUsesStationAnchor);
        }

        void EndCameraFocus()
        {
            if (_cameraController != null)
                _cameraController.EndActivityFocus();
        }

        static Vector3 FallbackActivityOffset(MoonlightSpatialActionKind kind)
        {
            return kind switch
            {
                MoonlightSpatialActionKind.Cook => new Vector3(1.58f, 0.72f, 0.22f),
                MoonlightSpatialActionKind.Play => new Vector3(-0.58f, 0f, -0.10f),
                MoonlightSpatialActionKind.Garden => new Vector3(1.10f, 0.04f, 0.30f),
                MoonlightSpatialActionKind.Read => new Vector3(1.08f, 0.05f, 0.30f),
                MoonlightSpatialActionKind.Care => new Vector3(0.92f, 0.05f, 0.24f),
                _ => Vector3.zero
            };
        }

        void OnDestroy()
        {
            if (_particleMaterial != null) Destroy(_particleMaterial);
            if (_particleTexture != null) Destroy(_particleTexture);
        }

        void EnsureFxRig()
        {
            var rig = transform.Find("ActionFeedbackRig");
            if (rig == null)
            {
                var rigGO = new GameObject("ActionFeedbackRig");
                rigGO.transform.SetParent(transform, false);
                rigGO.transform.localPosition = new Vector3(0f, 1.15f, 0f);
                rig = rigGO.transform;
            }

            if (_particles == null)
            {
                _particles = rig.GetComponent<ParticleSystem>();
                if (_particles == null) _particles = rig.gameObject.AddComponent<ParticleSystem>();
                var main = _particles.main;
                main.loop = false;
                main.playOnAwake = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.75f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(0.65f, 1.2f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.09f);
                main.maxParticles = 48;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                var shape = _particles.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.45f;
            }

            _particleRenderer = _particles.GetComponent<ParticleSystemRenderer>();
            if (_particleMaterial == null)
                _particleMaterial = CreateTransparentMaterial(Color.white);
            _particleRenderer.sharedMaterial = _particleMaterial;

            if (_flash == null)
            {
                var lightGO = new GameObject("ActionFlash");
                lightGO.transform.SetParent(rig, false);
                _flash = lightGO.AddComponent<Light>();
                _flash.type = LightType.Point;
                _flash.range = 2.8f;
                _flash.intensity = 0f;
            }
        }

        Material CreateTransparentMaterial(Color color)
        {
            if (_particleTexture == null)
                _particleTexture = CreateSoftCircleTexture(32);

            var shader = Shader.Find("Sprites/Default")
                ?? Shader.Find("Unlit/Transparent")
                ?? Shader.Find("Standard");
            var material = new Material(shader);
            material.color = color;
            material.mainTexture = _particleTexture;
            if (material.HasProperty("_Mode")) material.SetFloat("_Mode", 3f);
            if (material.HasProperty("_SrcBlend"))
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend"))
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (material.HasProperty("_ZWrite")) material.SetInt("_ZWrite", 0);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return material;
        }

        static Texture2D CreateSoftCircleTexture(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "MoonlightActionSoftCircle";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            var pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float radius = center;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / radius;
                float alpha = 1f - Mathf.SmoothStep(0.58f, 1f, distance);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
        }

        static Color ColorFor(MoonlightSpatialActionKind kind, string state) => kind switch
        {
            MoonlightSpatialActionKind.Cook => new Color(1f, 0.72f, 0.28f),
            MoonlightSpatialActionKind.Play => new Color(0.42f, 0.9f, 1f),
            MoonlightSpatialActionKind.Garden => new Color(0.48f, 0.92f, 0.54f),
            MoonlightSpatialActionKind.Read => new Color(1f, 0.78f, 0.42f),
            MoonlightSpatialActionKind.Care => new Color(0.38f, 0.88f, 0.82f),
            MoonlightSpatialActionKind.Feed => new Color(1f, 0.58f, 0.32f),
            MoonlightSpatialActionKind.SleepCuddle when state == "Resting" => new Color(0.50f, 0.66f, 1f),
            MoonlightSpatialActionKind.SleepCuddle when state == "Cuddled" => new Color(1f, 0.58f, 0.82f),
            _ => Color.white
        };

        static float DurationFor(MoonlightSpatialActionKind kind, string state) => kind switch
        {
            MoonlightSpatialActionKind.Cook => MoonlightActivityStage.CookActionSeconds,
            MoonlightSpatialActionKind.Play => 1.85f,
            MoonlightSpatialActionKind.Garden => 2.05f,
            MoonlightSpatialActionKind.Read => ReadActionDurationSeconds,
            MoonlightSpatialActionKind.Care => 2.15f,
            MoonlightSpatialActionKind.Feed => 1.35f,
            MoonlightSpatialActionKind.SleepCuddle when state == "Resting" => 1.65f,
            MoonlightSpatialActionKind.SleepCuddle when state == "Cuddled" => 1.05f,
            _ => 1f
        };

        public static MoonlightActionQualityTier ActionQualityTierFor(float score)
        {
            float clampedScore = Mathf.Clamp01(score);
            if (clampedScore < GreatActionQualityScore) return MoonlightActionQualityTier.Good;
            if (clampedScore < PerfectActionQualityScore) return MoonlightActionQualityTier.Great;
            return MoonlightActionQualityTier.Perfect;
        }

        public static bool ValidateActionQualityContract(out string detail)
        {
            bool thresholds =
                ActionQualityTierFor(0f) == MoonlightActionQualityTier.Good &&
                ActionQualityTierFor(GreatActionQualityScore - 0.001f) == MoonlightActionQualityTier.Good &&
                ActionQualityTierFor(GreatActionQualityScore) == MoonlightActionQualityTier.Great &&
                ActionQualityTierFor(PerfectActionQualityScore - 0.001f) == MoonlightActionQualityTier.Great &&
                ActionQualityTierFor(PerfectActionQualityScore) == MoonlightActionQualityTier.Perfect &&
                ActionQualityTierFor(1f) == MoonlightActionQualityTier.Perfect;
            bool monotonic = true;
            bool bounded = true;
            MoonlightSpatialActionKind[] activities =
            {
                MoonlightSpatialActionKind.Cook,
                MoonlightSpatialActionKind.Play,
                MoonlightSpatialActionKind.Garden,
                MoonlightSpatialActionKind.Read,
                MoonlightSpatialActionKind.Care
            };
            foreach (MoonlightSpatialActionKind activity in activities)
            {
                int good = ActionQualityBurstCountFor(activity, MoonlightActionQualityTier.Good);
                int great = ActionQualityBurstCountFor(activity, MoonlightActionQualityTier.Great);
                int perfect = ActionQualityBurstCountFor(activity, MoonlightActionQualityTier.Perfect);
                monotonic &= good < great && great < perfect;
                bounded &= good > 0 && perfect <= 64;
            }

            float goodFlash = ActionQualityFlashIntensityFor(MoonlightActionQualityTier.Good);
            float greatFlash = ActionQualityFlashIntensityFor(MoonlightActionQualityTier.Great);
            float perfectFlash = ActionQualityFlashIntensityFor(MoonlightActionQualityTier.Perfect);
            float goodEnergy = ActionQualityAccentEnergyFor(MoonlightActionQualityTier.Good);
            float greatEnergy = ActionQualityAccentEnergyFor(MoonlightActionQualityTier.Great);
            float perfectEnergy = ActionQualityAccentEnergyFor(MoonlightActionQualityTier.Perfect);
            int goodHaptic = ActionQualityHapticRankFor(MoonlightActionQualityTier.Good);
            int greatHaptic = ActionQualityHapticRankFor(MoonlightActionQualityTier.Great);
            int perfectHaptic = ActionQualityHapticRankFor(MoonlightActionQualityTier.Perfect);
            monotonic &= goodFlash < greatFlash && greatFlash < perfectFlash &&
                goodEnergy < greatEnergy && greatEnergy < perfectEnergy &&
                goodHaptic < greatHaptic && greatHaptic < perfectHaptic;
            bounded &= goodFlash >= 0.65f && perfectFlash <= 1.10f &&
                goodEnergy >= 0.90f && perfectEnergy <= 1.12f &&
                goodHaptic >= 0 && perfectHaptic <= 2;
            detail = $"activities={activities.Length}/5 thresholds={thresholds} split={GreatActionQualityScore:0.00}/{PerfectActionQualityScore:0.00} " +
                $"burst={ActionQualityBurstCountFor(MoonlightSpatialActionKind.Read, MoonlightActionQualityTier.Good)}-" +
                $"{ActionQualityBurstCountFor(MoonlightSpatialActionKind.Play, MoonlightActionQualityTier.Perfect)}/64 " +
                $"flash={goodFlash:0.00}/{greatFlash:0.00}/{perfectFlash:0.00} " +
                $"energy={goodEnergy:0.00}/{greatEnergy:0.00}/{perfectEnergy:0.00} " +
                $"haptic={goodHaptic}/{greatHaptic}/{perfectHaptic} " +
                $"monotonic={monotonic} bounded={bounded}";
            return activities.Length == 5 && thresholds && monotonic && bounded;
        }

        static string ActionQualityQAMarkerFor(MoonlightActionQualityTier tier) =>
            $"MOONLIGHT_ACTION_QUALITY_{tier.ToString().ToUpperInvariant()}";

        static int ActionQualityBurstCountFor(MoonlightSpatialActionKind kind,
                                               MoonlightActionQualityTier tier)
        {
            int tierOffset = tier switch
            {
                MoonlightActionQualityTier.Good => -4,
                MoonlightActionQualityTier.Perfect => 6,
                _ => 0
            };
            return Mathf.Clamp(BurstCountFor(kind) + tierOffset, 1, 64);
        }

        static float ActionQualityFlashIntensityFor(MoonlightActionQualityTier tier) => tier switch
        {
            MoonlightActionQualityTier.Good => 0.68f,
            MoonlightActionQualityTier.Perfect => 1.08f,
            _ => 0.85f
        };

        static float ActionQualityAccentEnergyFor(MoonlightActionQualityTier tier) => tier switch
        {
            MoonlightActionQualityTier.Good => 0.90f,
            MoonlightActionQualityTier.Perfect => 1.12f,
            _ => 1f
        };

        static int ActionQualityHapticRankFor(MoonlightActionQualityTier tier) => tier switch
        {
            MoonlightActionQualityTier.Good => 0,
            MoonlightActionQualityTier.Perfect => 2,
            _ => 1
        };

        static short BurstCountFor(MoonlightSpatialActionKind kind) => kind switch
        {
            MoonlightSpatialActionKind.Cook => 26,
            MoonlightSpatialActionKind.Play => 34,
            MoonlightSpatialActionKind.Garden => 30,
            MoonlightSpatialActionKind.Read => 22,
            MoonlightSpatialActionKind.Care => 28,
            MoonlightSpatialActionKind.SleepCuddle => 20,
            _ => 12
        };
    }
}
