using System.Collections;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public class MoonlightActionFeedback : MonoBehaviour
    {
        [SerializeField] float cooldownSeconds = 1.15f;

        Transform _visual;
        ParticleSystem _particles;
        Light _flash;
        Coroutine _running;
        Coroutine _cameraHoldRoutine;
        Coroutine _masteryFlashRoutine;
        Vector3 _baseScale = Vector3.one;
        Vector3 _basePosition;
        Quaternion _baseRotation = Quaternion.identity;
        GameObject _actionOrb;
        TrailRenderer _actionTrail;
        Material _actionMaterial;
        Material _trailMaterial;
        Material _particleMaterial;
        Texture2D _particleTexture;
        MoonlightActivityStage _activityStage;
        CameraController _cameraController;
        Vector3 _cameraFocusAnchor;
        Vector3 _actionContactPoint;
        Vector3 _actionPresentationDirection;
        Vector3 _actionStageScale = Vector3.one;
        bool _cameraFocusUsesStationAnchor;
        int _contactPhaseIndex = -1;
        float _cooldownUntil;
        string _stateText = "";
        MoonlightSpatialActionKind _activityKind;
        int _activityStep;
        int _activityRequiredSteps = 1;
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
        public int ActivityStep => _activityStep;
        public int ActivityRequiredSteps => _activityRequiredSteps;
        public int ActiveStageRenderers => _activityStage != null ? _activityStage.ActiveRendererCount : 0;
        public int ActiveStageMaterials => _activityStage != null ? _activityStage.ActiveUniqueMaterialCount : 0;
        public int ActiveStageLights => _activityStage != null ? _activityStage.ActiveLightCount : 0;
        public bool IsCameraFocusActive => _cameraController != null && _cameraController.IsActivityFocusActive;
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
        public bool MasteryCelebrationIsQueued => _masteryCelebrationQueued;
        public int QueuedMasteryTier => _queuedMasteryTier;
        public int LastMasteryCelebrationTier { get; private set; } = -1;
        public int LastMasteryCelebrationParticles { get; private set; }
        public int LastMasteryCelebrationCombo { get; private set; }
        public string MasteryCelebrationQAMarker { get; private set; } = "";

        void Awake()
        {
            CacheVisualPose();
            EnsureFxRig();
        }

        public bool TryBegin(MoonlightSpatialActionKind kind, string label, string shortState,
            int activityStep = 0, int activityRequiredSteps = 1)
        {
            if (!CanBeginAction)
            {
                Debug.Log($"[MoonlightVisualQA] action-input-blocked kind={kind} " +
                    $"reason=\"{InputBlockReason}\" remaining={CooldownRemaining:0.00}s");
                return false;
            }

            if (_cameraHoldRoutine != null)
            {
                StopCoroutine(_cameraHoldRoutine);
                _cameraHoldRoutine = null;
                EndCameraFocus();
            }

            _stateText = shortState;
            _activityKind = kind;
            _activityStep = Mathf.Max(0, activityStep);
            _activityRequiredSteps = Mathf.Max(1, activityRequiredSteps);
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
                _ => "MAGIC"
            };
        }

        IEnumerator Play(MoonlightSpatialActionKind kind, string label, string state)
        {
            EnsureFxRig();
            if (_activityStage == null)
                _activityStage = GetComponent<MoonlightActivityStage>() ?? gameObject.AddComponent<MoonlightActivityStage>();
            var color = ColorFor(kind, state);
            float duration = DurationFor(kind, state);
            BeginCameraFocus(kind);
            _activityStage.Begin(kind, _activityStep, _activityRequiredSteps);
            ActionMotionProfile = MotionProfileFor(kind, _activityStep);
            if (kind == MoonlightSpatialActionKind.SleepCuddle)
                ResetContactQA();
            else
                BeginContactQA(kind);
            CreateActionOrb(kind, state, color, duration);
            Debug.Log($"[MoonlightVisualQA] action-start kind={kind} state={state} label=\"{label}\" " +
                $"step={_activityStep + 1}/{_activityRequiredSteps} duration={duration:0.00}s " +
                $"motionProfile=\"{ActionMotionProfile}\" contactTarget=\"{ActionContactTarget}\"");

            if (_flash != null)
            {
                _flash.color = color;
                _flash.intensity = 0.85f;
            }

            if (_particles != null)
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
                emission.SetBurst(0, new ParticleSystem.Burst(0f, BurstCountFor(kind)));
                _particles.Play(true);
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (kind != MoonlightSpatialActionKind.SleepCuddle)
                    UpdateContactQA(kind, t);
                ApplyPose(kind, state, t);
                UpdateActionOrb(kind, state, t);
                _activityStage.UpdateStage(kind, t);
                if (_flash != null)
                    _flash.intensity = Mathf.Lerp(0.85f, 0f, t);
                yield return null;
            }

            RestoreVisualPose();
            bool finalActivityStep = _activityRequiredSteps > 1 &&
                _activityStep == _activityRequiredSteps - 1;
            bool heldFinalPresentation = finalActivityStep &&
                _activityStage.LingerFinalState(FinalPresentationSecondsFor(kind));
            if (!heldFinalPresentation)
            {
                _activityStage.End();
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
            MoonlightSpatialActionKind.Cook => 5.2f,
            MoonlightSpatialActionKind.Play => 4.8f,
            MoonlightSpatialActionKind.Garden => 4.6f,
            MoonlightSpatialActionKind.Read => 4.4f,
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

            _visual.localScale = Vector3.Scale(_baseScale, axisScale);

            Vector3 localOffset = kind switch
            {
                MoonlightSpatialActionKind.Garden => new Vector3(0f, envelope * 0.10f, -envelope * 0.18f),
                MoonlightSpatialActionKind.Read => new Vector3(0f, envelope * 0.04f, -envelope * 0.12f),
                MoonlightSpatialActionKind.SleepCuddle when resting => new Vector3(0f, -Mathf.SmoothStep(0f, 0.16f, t), 0f),
                MoonlightSpatialActionKind.SleepCuddle when cuddled => new Vector3(0f, envelope * 0.24f, -envelope * 0.14f),
                _ => Vector3.zero
            };
            Vector3 localEuler = kind switch
            {
                MoonlightSpatialActionKind.Garden => new Vector3(envelope * 16f, Mathf.Sin(t * Mathf.PI * 4f) * 8f, 0f),
                MoonlightSpatialActionKind.Read => new Vector3(envelope * 9f, Mathf.Sin(t * Mathf.PI * 2f) * 5f, 0f),
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
            float maxApproachDistance = 0f, bool cameraReadableFacing = false)
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
                    float presentationAngle = Mathf.Clamp(Mathf.Abs(targetDelta), 24f, 34f);
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
                _ => ""
            };
        }

        void BeginContactQA(MoonlightSpatialActionKind kind)
        {
            ActionContactTarget = ContactTargetFor(kind, _activityStep);
            ActionContactWeight = 0f;
            _actionContactPoint = ContactPointFor(kind, _activityStep, 0f);
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
            else if (kind == MoonlightSpatialActionKind.Play)
            {
                approachStart = 0.04f;
                contactStart = step switch { 0 => 0.12f, 1 => 0.18f, _ => 0.24f };
                contactEnd = step switch { 0 => 0.30f, 1 => 0.80f, _ => 0.68f };
                recoveryStart = step == 0 ? 0.78f : 0.90f;
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
                MoonlightSpatialActionKind.SleepCuddle when state == "Resting" => "dream-orbit",
                MoonlightSpatialActionKind.SleepCuddle => "cuddle-orbit",
                _ => "magic-orbit"
            };

            _actionOrb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _actionOrb.name = $"ActionOrb-{ActiveEffectName}";
            float orbSize = kind switch
            {
                MoonlightSpatialActionKind.Cook => 0.13f,
                MoonlightSpatialActionKind.Play => 0.15f,
                MoonlightSpatialActionKind.Garden => 0.11f,
                MoonlightSpatialActionKind.Read => 0.09f,
                MoonlightSpatialActionKind.SleepCuddle when state == "Cuddled" => 0.16f,
                _ => 0.10f
            };
            _actionOrb.transform.localScale = Vector3.one * orbSize;
            var collider = _actionOrb.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            var shader = Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
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

            _actionTrail = _actionOrb.AddComponent<TrailRenderer>();
            _actionTrail.time = Mathf.Min(0.75f, duration * 0.55f);
            _actionTrail.minVertexDistance = 0.015f;
            _actionTrail.startWidth = kind == MoonlightSpatialActionKind.Play ? 0.085f : 0.055f;
            _actionTrail.endWidth = 0f;
            _actionTrail.startColor = color;
            _actionTrail.endColor = new Color(color.r, color.g, color.b, 0f);
            _trailMaterial = CreateTransparentMaterial(Color.white);
            _actionTrail.sharedMaterial = _trailMaterial;
            UpdateActionOrb(kind, state, 0f);
        }

        void UpdateActionOrb(MoonlightSpatialActionKind kind, string state, float t)
        {
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
                case MoonlightSpatialActionKind.Play:
                    _actionOrb.transform.position = _actionContactPoint;
                    float playSquash = ActionContactWeight * 0.18f;
                    _actionOrb.transform.localScale = new Vector3(0.15f + playSquash,
                        0.15f - playSquash * 0.45f, 0.15f + playSquash);
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
        }

        void DestroyActionOrb()
        {
            ActiveEffectName = "";
            if (_actionOrb != null) Destroy(_actionOrb);
            if (_actionMaterial != null) Destroy(_actionMaterial);
            if (_trailMaterial != null) Destroy(_trailMaterial);
            _actionOrb = null;
            _actionTrail = null;
            _actionMaterial = null;
            _trailMaterial = null;
        }

        void OnDisable()
        {
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

            var particleRenderer = _particles.GetComponent<ParticleSystemRenderer>();
            if (_particleMaterial == null)
                _particleMaterial = CreateTransparentMaterial(Color.white);
            particleRenderer.sharedMaterial = _particleMaterial;

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
            MoonlightSpatialActionKind.SleepCuddle when state == "Resting" => new Color(0.50f, 0.66f, 1f),
            MoonlightSpatialActionKind.SleepCuddle when state == "Cuddled" => new Color(1f, 0.58f, 0.82f),
            _ => Color.white
        };

        static float DurationFor(MoonlightSpatialActionKind kind, string state) => kind switch
        {
            MoonlightSpatialActionKind.Cook => 2.25f,
            MoonlightSpatialActionKind.Play => 1.85f,
            MoonlightSpatialActionKind.Garden => 2.05f,
            MoonlightSpatialActionKind.Read => 1.75f,
            MoonlightSpatialActionKind.SleepCuddle when state == "Resting" => 1.65f,
            MoonlightSpatialActionKind.SleepCuddle when state == "Cuddled" => 1.05f,
            _ => 1f
        };

        static short BurstCountFor(MoonlightSpatialActionKind kind) => kind switch
        {
            MoonlightSpatialActionKind.Cook => 26,
            MoonlightSpatialActionKind.Play => 34,
            MoonlightSpatialActionKind.Garden => 30,
            MoonlightSpatialActionKind.Read => 22,
            MoonlightSpatialActionKind.SleepCuddle => 20,
            _ => 12
        };
    }
}
