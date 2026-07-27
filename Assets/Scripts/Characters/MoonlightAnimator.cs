using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    [RequireComponent(typeof(Animator))]
    public class MoonlightAnimator : MonoBehaviour
    {
        public const int ProceduralStateCount = 3;
        public const int MinimumArticulatedTransformCount = 8;
        public const float WalkCadenceHz = 1.6f;
        public const float SprintCadenceHz = 2.1f;
        public const float GaitResponsePerSecond = 14f;
        public const float IdlePositionToleranceMeters = 0.0005f;
        public const float IdleRotationToleranceDegrees = 0.1f;
        public const string AnimatorControllerUnobservedMarker =
            "MOONLIGHT_ANIMATOR_CONTROLLER_LOCOMOTION_UNOBSERVED";
        public const string AnimatorControllerIncompleteMarker =
            "MOONLIGHT_ANIMATOR_CONTROLLER_LOCOMOTION_INCOMPLETE";
        const float IdleScaleTolerance = 0.00001f;

        static readonly int MoodHash = Animator.StringToHash("Mood");
        static readonly int StageHash = Animator.StringToHash("Stage");
        static readonly int EatHash = Animator.StringToHash("Eat");
        static readonly int CuddleHash = Animator.StringToHash("Cuddle");
        static readonly int SleepHash = Animator.StringToHash("Sleep");
        static readonly int StageUpHash = Animator.StringToHash("StageUp");
        static readonly int WalkHash = Animator.StringToHash("Walk");
        static readonly int MoveMagnitudeHash = Animator.StringToHash("MoveMagnitude");
        static readonly int SprintHash = Animator.StringToHash("Sprint");
        static readonly int DanceHash = Animator.StringToHash("Dance");

        static readonly string[] ArticulatedTransformNames =
        {
            "MoonbudArmLeft",
            "MoonbudArmRight",
            "MoonbudBody",
            "MoonbudEarLeft",
            "MoonbudEarRight",
            "MoonbudHead",
            "MoonbudPawLeft",
            "MoonbudPawRight",
            "MoonbudRingTail",
            "MoonbudTailTip"
        };

        const int ArmLeftIndex = 0;
        const int ArmRightIndex = 1;
        const int BodyIndex = 2;
        const int EarLeftIndex = 3;
        const int EarRightIndex = 4;
        const int HeadIndex = 5;
        const int PawLeftIndex = 6;
        const int PawRightIndex = 7;
        const int RingTailIndex = 8;
        const int TailTipIndex = 9;
        const float Tau = Mathf.PI * 2f;
        const float RestWeightThreshold = 0.001f;

        struct LocalPose
        {
            public Transform transform;
            public Renderer geometryRenderer;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }

        readonly LocalPose[] _cachedPoses = new LocalPose[ArticulatedTransformNames.Length];
        readonly Dictionary<int, AnimatorControllerParameterType> _parameterTypesByHash = new();
        readonly List<Transform> _rigTransforms = new();
        readonly List<Renderer> _rigRenderers = new();

        Animator _anim;
        RuntimeAnimatorController _cachedParameterController;
        MoonlightCharacter _ml;
        IdleMicroMotion _idleMicroMotion;
        Transform _cachedAvatarRoot;
        bool _parameterCacheInitialized;
        bool _rigCached;
        bool _rigBindingFailedClosed;
        bool _liveRigBindingValid;
        int _lastRigValidationFrame = int.MinValue;
        int _liveActiveVisibleBindingCount;
        bool _idleMicroMotionSuppressed;
        bool _resumeIdleMicroMotion;
        bool _actionActive;
        bool _wasUsingProceduralLocomotion;
        bool _proceduralPoseApplied;
        float _movementMagnitude;
        float _gaitWeight;
        float _sprintWeight;
        float _gaitPhase;
        bool _sprinting;

        public bool HasRuntimeAnimatorController =>
            _anim != null && _anim.runtimeAnimatorController != null;
        public bool UsesProceduralLocomotion =>
            isActiveAndEnabled && gameObject.activeInHierarchy &&
            !HasRuntimeAnimatorController && HasLiveUniqueProceduralRig();
        public int CachedArticulatedTransformCount { get; private set; }
        public int ActiveVisibleArticulatedBindingCount
        {
            get
            {
                RevalidateActiveRigBindingsOncePerFrame();
                return _liveActiveVisibleBindingCount;
            }
        }
        public bool ProceduralRigBindingFailedClosed
        {
            get
            {
                RevalidateActiveRigBindingsOncePerFrame();
                return _rigBindingFailedClosed || (_rigCached && !_liveRigBindingValid);
            }
        }
        public bool LiveProceduralRigBindingValid => HasLiveUniqueProceduralRig();
        public float MovementMagnitude => _movementMagnitude;
        public bool IsSprinting => _sprinting;

        void Awake() => _anim = GetComponent<Animator>();

        void Start()
        {
            CacheProceduralRig();
            _ml = GetComponentInParent<MoonlightCharacter>();
            if (_ml == null) return;

            _ml.onStageUp.AddListener(_ => SetTriggerIfPresent(StageUpHash));
            _ml.onMoodChange.AddListener(m =>
            {
                SetIntegerIfPresent(MoodHash, (int)m);
                if (m == MoonlightMood.Radiant) SetTriggerIfPresent(DanceHash);
            });
        }

        void Update()
        {
            if (_ml != null) SetIntegerIfPresent(StageHash, (int)_ml.stage);
        }

        void LateUpdate()
        {
            if (!UsesProceduralLocomotion)
            {
                if (_wasUsingProceduralLocomotion)
                {
                    RestoreCachedTransformsExact();
                    ResumeIdleMicroMotion();
                    _wasUsingProceduralLocomotion = false;
                }
                return;
            }
            _wasUsingProceduralLocomotion = true;

            var feedback = GetComponentInParent<MoonlightActionFeedback>();
            bool activityOwnsPose = _actionActive ||
                (feedback != null && feedback.IsPerformingAction);
            if (activityOwnsPose)
            {
                _gaitWeight = 0f;
                SuppressIdleMicroMotion();
                RestoreCachedTransformsExact();
                return;
            }

            if (UpdateProceduralLocomotion(Time.deltaTime)) return;

            _gaitWeight = 0f;
            if (_proceduralPoseApplied || _idleMicroMotionSuppressed)
                RestoreCachedTransformsExact();
            ResumeIdleMicroMotion();
            ApplyProceduralIdleAccents();
        }

        public void TriggerEat() => SetTriggerIfPresent(EatHash);
        public void TriggerCuddle() => SetTriggerIfPresent(CuddleHash);
        public void TriggerSleep() => SetTriggerIfPresent(SleepHash);

        public void SetWalking(bool walking) => SetLocomotion(walking ? 1f : 0f, false);

        public void SetLocomotion(float movementMagnitude, bool sprinting)
        {
            _movementMagnitude = Mathf.Clamp01(movementMagnitude);
            _sprinting = sprinting && _movementMagnitude > 0f;
            if (!HasRuntimeAnimatorController) return;

            SetBoolIfPresent(WalkHash, _movementMagnitude > 0f);
            SetFloatIfPresent(MoveMagnitudeHash, _movementMagnitude);
            SetBoolIfPresent(SprintHash, _sprinting);
        }

        public void SetActionActive(bool active)
        {
            _actionActive = active;
            if (!active || !UsesProceduralLocomotion) return;

            _movementMagnitude = 0f;
            _sprinting = false;
            _gaitWeight = 0f;
            SuppressIdleMicroMotion();
            RestoreCachedTransformsExact();
        }

        public void RestoreForActionHandoff()
        {
            if (!UsesProceduralLocomotion) return;
            SuppressIdleMicroMotion();
            RestoreCachedTransformsExact();
        }

        public bool ValidateProceduralLocomotionRuntimeContract(out string detail)
        {
            bool sourcePass = ValidateProceduralLocomotionSourceContract(out string sourceDetail);
            bool inputPass = _movementMagnitude >= 0f && _movementMagnitude <= 1f;
            if (HasRuntimeAnimatorController)
            {
                bool controllerObservedPass = ObserveAnimatorControllerLocomotionRuntimeContract(
                    out string controllerObservedDetail);
                detail = $"{sourceDetail} runtimeMode=animator-controller " +
                    $"controller={_anim.runtimeAnimatorController.name} " +
                    $"movementMagnitude={_movementMagnitude:0.000} " + controllerObservedDetail;
                return sourcePass && inputPass && controllerObservedPass;
            }

            bool observedPass = ObserveProceduralLocomotionRuntimeContract(
                out string observedDetail);
            detail = $"{sourceDetail} runtimeMode=procedural {observedDetail}";
            return sourcePass && inputPass && observedPass;
        }

        public bool ObserveAnimatorControllerLocomotionRuntimeContract(out string detail)
        {
            detail = $"observed=False reason=live-controller-sampling-disabled " +
                $"observationMarker={AnimatorControllerUnobservedMarker} " +
                $"marker={AnimatorControllerIncompleteMarker}";
            return false;
        }

        public bool ObserveProceduralLocomotionRuntimeContract(out string detail)
        {
            if (!_rigCached || !UsesProceduralLocomotion)
            {
                detail = $"observed=False rigCached={_rigCached} " +
                    $"articulatedTransforms={CachedArticulatedTransformCount}/" +
                    $">={MinimumArticulatedTransformCount} activeVisibleBindings=" +
                    $"{ActiveVisibleArticulatedBindingCount}/" +
                    $">={MinimumArticulatedTransformCount} " +
                    $"bindingFailClosed={ProceduralRigBindingFailedClosed}";
                return false;
            }

            LocalPose[] currentPoseSnapshot = CaptureCurrentPoses();
            float movementMagnitude = _movementMagnitude;
            float gaitWeight = _gaitWeight;
            float sprintWeight = _sprintWeight;
            float gaitPhase = _gaitPhase;
            bool sprinting = _sprinting;
            bool actionActive = _actionActive;
            bool wasUsingProceduralLocomotion = _wasUsingProceduralLocomotion;
            bool proceduralPoseApplied = _proceduralPoseApplied;
            bool idleMicroMotionSuppressed = _idleMicroMotionSuppressed;
            bool resumeIdleMicroMotion = _resumeIdleMicroMotion;
            bool idleMicroMotionEnabled = _idleMicroMotion != null && _idleMicroMotion.enabled;

            int walkMoved = 0;
            int sprintMoved = 0;
            int actionDisplaced = 0;
            int handoffDisplaced = 0;
            float walkAmplitude = 0f;
            float sprintAmplitude = 0f;
            float actionPositionError = float.PositiveInfinity;
            float actionRotationError = float.PositiveInfinity;
            float actionScaleError = float.PositiveInfinity;
            float handoffPositionError = float.PositiveInfinity;
            float handoffRotationError = float.PositiveInfinity;
            float handoffScaleError = float.PositiveInfinity;
            float currentPosePositionError = float.PositiveInfinity;
            float currentPoseRotationError = float.PositiveInfinity;
            float currentPoseScaleError = float.PositiveInfinity;
            bool oppositeArms = false;
            bool oppositePaws = false;
            bool phaseContinuous = false;

            try
            {
                _actionActive = false;
                SuppressIdleMicroMotion();

                _movementMagnitude = 1f;
                _sprinting = false;
                _gaitWeight = 1f;
                _sprintWeight = 0f;
                _gaitPhase = Mathf.PI * 0.25f;
                UpdateProceduralLocomotion(0f);
                walkMoved = CountTransformsDisplacedFromCachedBases();
                walkAmplitude = MaximumRotationFromCachedBases();
                float walkLeftArm = SignedLocalXOffset(ArmLeftIndex);
                float walkRightArm = SignedLocalXOffset(ArmRightIndex);
                float walkLeftPaw = SignedLocalXOffset(PawLeftIndex);
                float walkRightPaw = SignedLocalXOffset(PawRightIndex);
                oppositeArms = walkLeftArm * walkRightArm < 0f;
                oppositePaws = walkLeftPaw * walkRightPaw < 0f;

                _sprinting = true;
                _sprintWeight = 1f;
                UpdateProceduralLocomotion(0f);
                sprintMoved = CountTransformsDisplacedFromCachedBases();
                sprintAmplitude = MaximumRotationFromCachedBases();

                const float phaseStepSeconds = 0.03125f;
                _gaitPhase = 0.35f;
                _sprinting = false;
                _sprintWeight = 0f;
                UpdateProceduralLocomotion(phaseStepSeconds);
                float walkPhase = _gaitPhase;
                LocalPose[] walkPhasePose = CaptureCurrentPoses();
                SetLocomotion(1f, true);
                float sprintCommandPhase = _gaitPhase;
                MeasurePoseErrors(walkPhasePose, out float commandPositionError,
                    out float commandRotationError, out float commandScaleError);
                UpdateProceduralLocomotion(phaseStepSeconds);
                float sprintPhase = _gaitPhase;
                phaseContinuous = walkPhase > 0.35f &&
                    Mathf.Abs(sprintCommandPhase - walkPhase) <= 0.000001f &&
                    PoseErrorsWithinTolerance(commandPositionError,
                        commandRotationError, commandScaleError) &&
                    sprintPhase > sprintCommandPhase;

                actionDisplaced = CountTransformsDisplacedFromCachedBases();
                SetActionActive(true);
                MeasureCachedBaseErrors(out actionPositionError,
                    out actionRotationError, out actionScaleError);

                _actionActive = false;
                _movementMagnitude = 1f;
                _sprinting = false;
                _gaitWeight = 1f;
                _sprintWeight = 0f;
                _gaitPhase = Mathf.PI * 0.25f;
                UpdateProceduralLocomotion(0f);
                handoffDisplaced = CountTransformsDisplacedFromCachedBases();
                RestoreForActionHandoff();
                MeasureCachedBaseErrors(out handoffPositionError,
                    out handoffRotationError, out handoffScaleError);
            }
            finally
            {
                RestorePoseSnapshot(currentPoseSnapshot);
                MeasurePoseErrors(currentPoseSnapshot, out currentPosePositionError,
                    out currentPoseRotationError, out currentPoseScaleError);
                _movementMagnitude = movementMagnitude;
                _gaitWeight = gaitWeight;
                _sprintWeight = sprintWeight;
                _gaitPhase = gaitPhase;
                _sprinting = sprinting;
                _actionActive = actionActive;
                _wasUsingProceduralLocomotion = wasUsingProceduralLocomotion;
                _proceduralPoseApplied = proceduralPoseApplied;
                if (_idleMicroMotion != null) _idleMicroMotion.enabled = idleMicroMotionEnabled;
                _idleMicroMotionSuppressed = idleMicroMotionSuppressed;
                _resumeIdleMicroMotion = resumeIdleMicroMotion;
            }

            bool movementPass = walkMoved >= MinimumArticulatedTransformCount &&
                sprintMoved >= MinimumArticulatedTransformCount;
            bool sprintPass = sprintAmplitude > walkAmplitude + 0.01f;
            bool displacedBeforeRestore = actionDisplaced >= MinimumArticulatedTransformCount &&
                handoffDisplaced >= MinimumArticulatedTransformCount;
            bool actionRestorePass = PoseErrorsWithinTolerance(actionPositionError,
                actionRotationError, actionScaleError);
            bool handoffRestorePass = PoseErrorsWithinTolerance(handoffPositionError,
                handoffRotationError, handoffScaleError);
            bool currentPoseRestorePass = PoseErrorsWithinTolerance(currentPosePositionError,
                currentPoseRotationError, currentPoseScaleError);
            bool pass = movementPass && oppositeArms && oppositePaws && sprintPass &&
                phaseContinuous && displacedBeforeRestore && actionRestorePass &&
                handoffRestorePass && currentPoseRestorePass;
            detail = $"observed={pass} articulatedTransforms={CachedArticulatedTransformCount} " +
                $"activeVisibleBindings={ActiveVisibleArticulatedBindingCount} " +
                $"bindingFailClosed={ProceduralRigBindingFailedClosed} " +
                $"movedWalk={walkMoved} movedSprint={sprintMoved} " +
                $"oppositeArms={oppositeArms} oppositePaws={oppositePaws} " +
                $"amplitudeDeg={walkAmplitude:0.000}/{sprintAmplitude:0.000} " +
                $"phaseContinuous={phaseContinuous} " +
                $"displacedBeforeRestore={actionDisplaced}/{handoffDisplaced} " +
                $"actionRestoreMmDeg={actionPositionError * 1000f:0.000}/" +
                $"{actionRotationError:0.000} handoffRestoreMmDeg=" +
                $"{handoffPositionError * 1000f:0.000}/{handoffRotationError:0.000} " +
                $"sampleStateRestoreMmDeg={currentPosePositionError * 1000f:0.000}/" +
                $"{currentPoseRotationError:0.000}";
            return pass;
        }

        public static bool ValidateProceduralLocomotionSourceContract(out string detail)
        {
            float ninetyPercentSeconds = Mathf.Log(10f) / GaitResponsePerSecond;
            bool statePass = ProceduralStateCount == 3;
            bool transformsPass = ArticulatedTransformNames.Length >= MinimumArticulatedTransformCount;
            bool cadencePass = Mathf.Abs(WalkCadenceHz - 1.6f) <= 0.0001f &&
                Mathf.Abs(SprintCadenceHz - 2.1f) <= 0.0001f;
            bool responsePass = ninetyPercentSeconds <= 0.18f;
            bool restorationPass = IdlePositionToleranceMeters <= 0.0005f &&
                IdleRotationToleranceDegrees <= 0.1f;
            detail = $"articulatedStates={ProceduralStateCount}/3 " +
                $"articulatedTransforms={ArticulatedTransformNames.Length}/" +
                $">={MinimumArticulatedTransformCount} input=magnitude+sprint " +
                $"cadenceHz={WalkCadenceHz:0.0}/{SprintCadenceHz:0.0} " +
                $"gait90Seconds={ninetyPercentSeconds:0.000} " +
                $"idleToleranceMm={IdlePositionToleranceMeters * 1000f:0.0} " +
                $"idleToleranceDeg={IdleRotationToleranceDegrees:0.0}";
            return statePass && transformsPass && cadencePass && responsePass && restorationPass;
        }

        void CacheProceduralRig()
        {
            if (_rigCached) return;

            _rigTransforms.Clear();
            GetComponentsInChildren(true, _rigTransforms);
            for (int poseIndex = 0; poseIndex < ArticulatedTransformNames.Length; poseIndex++)
            {
                Transform match = null;
                Renderer matchRenderer = null;
                int activeVisibleMatches = 0;
                for (int transformIndex = 0; transformIndex < _rigTransforms.Count; transformIndex++)
                {
                    Transform candidate = _rigTransforms[transformIndex];
                    if (candidate.name != ArticulatedTransformNames[poseIndex] ||
                        !candidate.gameObject.activeInHierarchy)
                        continue;

                    Renderer geometryRenderer = FindActiveVisibleGeometryRenderer(candidate);
                    if (geometryRenderer == null) continue;
                    activeVisibleMatches++;
                    if (activeVisibleMatches == 1)
                    {
                        match = candidate;
                        matchRenderer = geometryRenderer;
                    }
                }

                if (activeVisibleMatches > 1)
                {
                    _rigBindingFailedClosed = true;
                    continue;
                }
                if (match == null) continue;

                Transform avatarRoot = FindAvatarRoot(match);
                if (avatarRoot == null ||
                    (_cachedAvatarRoot != null && avatarRoot != _cachedAvatarRoot))
                {
                    _rigBindingFailedClosed = true;
                    continue;
                }
                _cachedAvatarRoot = avatarRoot;
                _cachedPoses[poseIndex] = new LocalPose
                {
                    transform = match,
                    geometryRenderer = matchRenderer,
                    position = match.localPosition,
                    rotation = match.localRotation,
                    scale = match.localScale
                };
                CachedArticulatedTransformCount++;
            }

            _rigCached = true;
            if (_rigBindingFailedClosed)
            {
                for (int i = 0; i < _cachedPoses.Length; i++)
                    _cachedPoses[i] = default;
                CachedArticulatedTransformCount = 0;
                _cachedAvatarRoot = null;
                _liveRigBindingValid = false;
                _liveActiveVisibleBindingCount = 0;
                return;
            }

            _idleMicroMotion = _cachedAvatarRoot != null
                ? _cachedAvatarRoot.GetComponentInChildren<IdleMicroMotion>()
                : null;
            RestoreCachedTransformsExact();
            _liveActiveVisibleBindingCount = CachedArticulatedTransformCount;
            _liveRigBindingValid = CachedArticulatedTransformCount >=
                MinimumArticulatedTransformCount;
            _lastRigValidationFrame = Time.frameCount;
        }

        bool HasLiveUniqueProceduralRig()
        {
            RevalidateActiveRigBindingsOncePerFrame();
            return _liveRigBindingValid;
        }

        void RevalidateActiveRigBindingsOncePerFrame()
        {
            int frame = Time.frameCount;
            if (_lastRigValidationFrame == frame) return;
            _lastRigValidationFrame = frame;
            _liveRigBindingValid = false;
            _liveActiveVisibleBindingCount = 0;
            if (!_rigCached || _rigBindingFailedClosed || _cachedAvatarRoot == null ||
                !_cachedAvatarRoot.gameObject.activeInHierarchy)
                return;

            _rigTransforms.Clear();
            GetComponentsInChildren(true, _rigTransforms);
            int validatedBindings = 0;
            for (int poseIndex = 0; poseIndex < _cachedPoses.Length; poseIndex++)
            {
                LocalPose cachedPose = _cachedPoses[poseIndex];
                if (cachedPose.transform == null) continue;

                Transform soleMatch = null;
                Renderer soleRenderer = null;
                int activeVisibleMatches = 0;
                for (int transformIndex = 0; transformIndex < _rigTransforms.Count; transformIndex++)
                {
                    Transform candidate = _rigTransforms[transformIndex];
                    if (candidate.name != ArticulatedTransformNames[poseIndex] ||
                        !candidate.gameObject.activeInHierarchy)
                        continue;

                    Renderer geometryRenderer = FindActiveVisibleGeometryRenderer(candidate);
                    if (geometryRenderer == null) continue;
                    activeVisibleMatches++;
                    if (activeVisibleMatches == 1)
                    {
                        soleMatch = candidate;
                        soleRenderer = geometryRenderer;
                    }
                }

                if (activeVisibleMatches != 1 || soleMatch != cachedPose.transform ||
                    FindAvatarRoot(soleMatch) != _cachedAvatarRoot)
                {
                    _liveActiveVisibleBindingCount = 0;
                    return;
                }

                cachedPose.geometryRenderer = soleRenderer;
                _cachedPoses[poseIndex] = cachedPose;
                validatedBindings++;
            }

            _liveActiveVisibleBindingCount = validatedBindings;
            _liveRigBindingValid = validatedBindings == CachedArticulatedTransformCount &&
                validatedBindings >= MinimumArticulatedTransformCount;
        }

        Transform FindAvatarRoot(Transform candidate)
        {
            Transform avatarRoot = candidate;
            while (avatarRoot.parent != null && avatarRoot.parent != transform)
                avatarRoot = avatarRoot.parent;
            return avatarRoot.parent == transform ? avatarRoot : null;
        }

        Renderer FindActiveVisibleGeometryRenderer(Transform candidate)
        {
            _rigRenderers.Clear();
            candidate.GetComponentsInChildren(false, _rigRenderers);
            for (int i = 0; i < _rigRenderers.Count; i++)
            {
                if (IsActiveVisibleGeometry(_rigRenderers[i])) return _rigRenderers[i];
            }
            return null;
        }

        static bool IsActiveVisibleGeometry(Renderer renderer)
        {
            if (renderer == null || !renderer.enabled || renderer.forceRenderingOff ||
                !renderer.gameObject.activeInHierarchy ||
                renderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly)
                return false;

            if (renderer is SkinnedMeshRenderer skinnedRenderer)
                return skinnedRenderer.sharedMesh != null &&
                    skinnedRenderer.sharedMesh.vertexCount > 0;
            if (renderer is not MeshRenderer) return false;

            var meshFilter = renderer.GetComponent<MeshFilter>();
            return meshFilter != null && meshFilter.sharedMesh != null &&
                meshFilter.sharedMesh.vertexCount > 0;
        }

        void ApplyProceduralLocomotion()
        {
            RestoreCachedTransformsExact();

            float cycle = Mathf.Sin(_gaitPhase);
            float rebound = Mathf.Sin(_gaitPhase * 2f);
            float weight = _gaitWeight;
            float armAmplitude = Mathf.Lerp(13f, 25f, _sprintWeight) * weight;
            float pawAmplitude = Mathf.Lerp(9f, 18f, _sprintWeight) * weight;
            float bodyPitch = Mathf.Lerp(2.2f, 4.5f, _sprintWeight) * weight;
            float headNod = Mathf.Lerp(1.8f, 3.8f, _sprintWeight) * weight;
            float earBounce = Mathf.Lerp(2.5f, 5f, _sprintWeight) * weight;
            float tailSwing = Mathf.Lerp(10f, 19f, _sprintWeight) * weight;

            SetLocalRotation(ArmLeftIndex, Quaternion.Euler(cycle * armAmplitude, 0f, 0f));
            SetLocalRotation(ArmRightIndex, Quaternion.Euler(-cycle * armAmplitude, 0f, 0f));
            SetLocalRotation(PawLeftIndex, Quaternion.Euler(-cycle * pawAmplitude, 0f, 0f));
            SetLocalRotation(PawRightIndex, Quaternion.Euler(cycle * pawAmplitude, 0f, 0f));
            SetLocalRotation(BodyIndex,
                Quaternion.Euler(rebound * bodyPitch, 0f, cycle * bodyPitch * 0.35f));
            SetLocalRotation(HeadIndex,
                Quaternion.Euler(-rebound * headNod, 0f, -cycle * headNod * 0.4f));
            SetLocalRotation(EarLeftIndex,
                Quaternion.Euler(rebound * earBounce, 0f, cycle * earBounce * 0.45f));
            SetLocalRotation(EarRightIndex,
                Quaternion.Euler(rebound * earBounce, 0f, -cycle * earBounce * 0.45f));
            SetLocalRotation(RingTailIndex,
                Quaternion.Euler(0f, cycle * tailSwing, rebound * tailSwing * 0.25f));
            SetLocalRotation(TailTipIndex,
                Quaternion.Euler(0f, -cycle * tailSwing * 1.35f, rebound * tailSwing * 0.4f));

            LocalPose body = _cachedPoses[BodyIndex];
            if (body.transform != null)
            {
                float squash = Mathf.Abs(rebound) * Mathf.Lerp(0.012f, 0.026f, _sprintWeight) * weight;
                body.transform.localScale = new Vector3(
                    body.scale.x * (1f + squash * 0.45f),
                    body.scale.y * (1f - squash),
                    body.scale.z * (1f + squash * 0.45f));
            }
            _proceduralPoseApplied = true;
        }

        bool UpdateProceduralLocomotion(float deltaTime)
        {
            float response = 1f - Mathf.Exp(-GaitResponsePerSecond * deltaTime);
            _gaitWeight = Mathf.Lerp(_gaitWeight, _movementMagnitude, response);
            _sprintWeight = Mathf.Lerp(_sprintWeight, _sprinting ? 1f : 0f, response);
            if (_movementMagnitude <= 0f && _gaitWeight <= RestWeightThreshold) return false;

            SuppressIdleMicroMotion();
            float cadence = Mathf.Lerp(WalkCadenceHz, SprintCadenceHz, _sprintWeight);
            cadence *= Mathf.Lerp(0.72f, 1f, Mathf.Max(_movementMagnitude, _gaitWeight));
            _gaitPhase = Mathf.Repeat(_gaitPhase + deltaTime * cadence * Tau, Tau);
            ApplyProceduralLocomotion();
            return true;
        }

        void ApplyProceduralIdleAccents()
        {
            float idleTime = Time.time;
            float earTilt = Mathf.Sin(idleTime * 1.15f) * 1.8f;
            float tailSway = Mathf.Sin(idleTime * 0.85f) * 4.5f;
            SetLocalRotation(EarLeftIndex, Quaternion.Euler(earTilt, 0f, earTilt * 0.35f));
            SetLocalRotation(EarRightIndex, Quaternion.Euler(earTilt, 0f, -earTilt * 0.35f));
            SetLocalRotation(RingTailIndex, Quaternion.Euler(0f, tailSway, 0f));
            SetLocalRotation(TailTipIndex, Quaternion.Euler(0f, -tailSway * 1.35f, 0f));
        }

        void SetLocalRotation(int index, Quaternion offset)
        {
            LocalPose pose = _cachedPoses[index];
            if (pose.transform != null) pose.transform.localRotation = pose.rotation * offset;
        }

        LocalPose[] CaptureCurrentPoses()
        {
            var snapshot = new LocalPose[_cachedPoses.Length];
            for (int i = 0; i < _cachedPoses.Length; i++)
            {
                Transform target = _cachedPoses[i].transform;
                if (target == null) continue;
                snapshot[i] = new LocalPose
                {
                    transform = target,
                    position = target.localPosition,
                    rotation = target.localRotation,
                    scale = target.localScale
                };
            }
            return snapshot;
        }

        static void RestorePoseSnapshot(LocalPose[] snapshot)
        {
            for (int i = 0; i < snapshot.Length; i++)
            {
                LocalPose pose = snapshot[i];
                if (pose.transform == null) continue;
                pose.transform.localPosition = pose.position;
                pose.transform.localRotation = pose.rotation;
                pose.transform.localScale = pose.scale;
            }
        }

        int CountTransformsDisplacedFromCachedBases()
        {
            int moved = 0;
            for (int i = 0; i < _cachedPoses.Length; i++)
            {
                LocalPose pose = _cachedPoses[i];
                if (pose.transform == null) continue;
                bool displaced = Vector3.Distance(pose.transform.localPosition, pose.position) > 0.000001f ||
                    Quaternion.Angle(pose.transform.localRotation, pose.rotation) > 0.001f ||
                    Vector3.Distance(pose.transform.localScale, pose.scale) > 0.000001f;
                if (displaced) moved++;
            }
            return moved;
        }

        float MaximumRotationFromCachedBases()
        {
            float maximum = 0f;
            for (int i = 0; i < _cachedPoses.Length; i++)
            {
                LocalPose pose = _cachedPoses[i];
                if (pose.transform == null) continue;
                maximum = Mathf.Max(maximum,
                    Quaternion.Angle(pose.transform.localRotation, pose.rotation));
            }
            return maximum;
        }

        float SignedLocalXOffset(int index)
        {
            LocalPose pose = _cachedPoses[index];
            if (pose.transform == null) return 0f;
            Quaternion offset = Quaternion.Inverse(pose.rotation) * pose.transform.localRotation;
            return Mathf.DeltaAngle(0f, offset.eulerAngles.x);
        }

        void MeasureCachedBaseErrors(out float positionError, out float rotationError,
            out float scaleError) => MeasurePoseErrors(_cachedPoses,
                out positionError, out rotationError, out scaleError);

        static void MeasurePoseErrors(LocalPose[] expected, out float positionError,
            out float rotationError, out float scaleError)
        {
            positionError = 0f;
            rotationError = 0f;
            scaleError = 0f;
            for (int i = 0; i < expected.Length; i++)
            {
                LocalPose pose = expected[i];
                if (pose.transform == null) continue;
                positionError = Mathf.Max(positionError,
                    Vector3.Distance(pose.transform.localPosition, pose.position));
                rotationError = Mathf.Max(rotationError,
                    Quaternion.Angle(pose.transform.localRotation, pose.rotation));
                scaleError = Mathf.Max(scaleError,
                    Vector3.Distance(pose.transform.localScale, pose.scale));
            }
        }

        static bool PoseErrorsWithinTolerance(float positionError, float rotationError,
            float scaleError) => positionError <= IdlePositionToleranceMeters &&
                rotationError <= IdleRotationToleranceDegrees && scaleError <= IdleScaleTolerance;

        void RestoreCachedTransformsExact()
        {
            for (int i = 0; i < _cachedPoses.Length; i++)
            {
                LocalPose pose = _cachedPoses[i];
                if (pose.transform == null) continue;
                pose.transform.localPosition = pose.position;
                pose.transform.localRotation = pose.rotation;
                pose.transform.localScale = pose.scale;
            }
            _proceduralPoseApplied = false;
        }

        void SuppressIdleMicroMotion()
        {
            if (_idleMicroMotion == null || _idleMicroMotionSuppressed) return;
            _resumeIdleMicroMotion = _idleMicroMotion.enabled;
            _idleMicroMotion.enabled = false;
            _idleMicroMotionSuppressed = true;
        }

        void ResumeIdleMicroMotion()
        {
            if (_idleMicroMotion == null || !_idleMicroMotionSuppressed) return;
            _idleMicroMotion.enabled = _resumeIdleMicroMotion;
            _idleMicroMotionSuppressed = false;
            _resumeIdleMicroMotion = false;
        }

        void SetIntegerIfPresent(int hash, int value)
        {
            if (HasParameter(hash, AnimatorControllerParameterType.Int)) _anim.SetInteger(hash, value);
        }

        void SetFloatIfPresent(int hash, float value)
        {
            if (HasParameter(hash, AnimatorControllerParameterType.Float)) _anim.SetFloat(hash, value);
        }

        void SetBoolIfPresent(int hash, bool value)
        {
            if (HasParameter(hash, AnimatorControllerParameterType.Bool)) _anim.SetBool(hash, value);
        }

        void SetTriggerIfPresent(int hash)
        {
            if (HasParameter(hash, AnimatorControllerParameterType.Trigger)) _anim.SetTrigger(hash);
        }

        bool HasParameter(int hash, AnimatorControllerParameterType type)
        {
            RefreshParameterCacheIfControllerChanged();
            return _parameterTypesByHash.TryGetValue(hash, out var parameterType) &&
                parameterType == type;
        }

        void RefreshParameterCacheIfControllerChanged()
        {
            RuntimeAnimatorController controller = _anim != null
                ? _anim.runtimeAnimatorController
                : null;
            if (_parameterCacheInitialized &&
                ReferenceEquals(controller, _cachedParameterController))
                return;

            _parameterCacheInitialized = true;
            _cachedParameterController = controller;
            _parameterTypesByHash.Clear();
            if (controller == null) return;

            var parameters = _anim.parameters;
            for (int i = 0; i < parameters.Length; i++)
                _parameterTypesByHash[parameters[i].nameHash] = parameters[i].type;
        }

        void OnDisable()
        {
            if (!_rigCached || _rigBindingFailedClosed) return;
            if (_wasUsingProceduralLocomotion || _proceduralPoseApplied)
                RestoreCachedTransformsExact();
            if (_idleMicroMotionSuppressed) ResumeIdleMicroMotion();
            _wasUsingProceduralLocomotion = false;
        }
    }
}
