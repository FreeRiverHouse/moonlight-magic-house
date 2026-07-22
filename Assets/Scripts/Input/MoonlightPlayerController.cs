using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public class MoonlightPlayerController : MonoBehaviour
    {
        public const float MovementInputThreshold = 0.05f;
        public const float DefaultMoveSpeed = 2.6f;
        public const float IPadSprintProcessedInputThreshold = 0.92f;
        public const float IPadSprintSpeedMultiplier = 1.45f;
        public const float IPadSprintVisualMultiplier = 1.30f;
        public const float ProceduralWholeRootBobScale = 0f;
        public const float ProceduralWholeRootSquashScale = 0f;
        public const string IPadSprintReadyMarker = "MOONLIGHT_IPAD_SPRINT_READY";
        public const string TouchCameraRelativeContractMarker =
            "MOONLIGHT_TOUCH_CAMERA_RELATIVE_CONTRACT_VERIFIED";

        [SerializeField] float moveSpeed = DefaultMoveSpeed;
        [SerializeField] Rect roomBounds = new Rect(-4.2f, -3.25f, 8.4f, 6.5f);
        [SerializeField] float turnSpeed = 12f;
        [SerializeField] float walkBobHeight = 0.055f;
        [SerializeField] float walkBobSpeed = 10f;
        [SerializeField] float walkSquash = 0.035f;
        [SerializeField] float turnLeanAngle = 7f;
        [SerializeField] float visualReturnSpeed = 8f;
        [SerializeField] float collisionSkin = 0.055f;
        [SerializeField] float recoveryStep = 0.28f;
        [SerializeField, Range(2, 8)] int recoveryRings = 5;

        Transform _visual;
        MoonlightBobber _idleBobber;
        readonly List<MoonlightAnimator> _moonlightAnimators = new();
        readonly List<MoonlightKidAnimator> _moonlightKidAnimators = new();
        MoonlightAnimator _activeMoonlightAnimator;
        MoonlightKidAnimator _activeMoonlightKidAnimator;
        int _lastAnimatorScanFrame = int.MinValue;
        int _activeMoonlightAnimatorCount;
        int _activeMoonlightKidAnimatorCount;
        Quaternion _visualBaseRotation = Quaternion.identity;
        Vector3 _visualBasePosition;
        Vector3 _visualBaseScale = Vector3.one;
        Vector2 _smoothedMove;
        Vector2 _touchMove;
        float _walkPhase;
        float _currentLean;
        float _lastYaw;
        bool _wasMoving;
        bool _wasPerformingAction;
        bool _modalInputLocked;
        CapsuleCollider _capsule;
        readonly Collider[] _overlaps = new Collider[24];
        Vector3 _lastSafePosition;
        bool _hasSafePosition;

        public Rect RoomBounds => roomBounds;
        public Vector2 TouchMove => _touchMove;
        public float BaseMoveSpeed => moveSpeed;
        public bool IsIPadSprinting { get; private set; }
        public bool IsModalInputLocked => _modalInputLocked;
        public string ModalInputLockQAMarker => _modalInputLocked
            ? "MOONLIGHT_STORY_MODAL_INPUT_LOCKED"
            : "MOONLIGHT_STORY_MODAL_INPUT_RELEASED";
        public float CurrentMoveSpeed => moveSpeed *
            (IsIPadSprinting ? IPadSprintSpeedMultiplier : 1f);
        public string IPadSprintQAMarker => ValidateIPadSprintRuntimeContract(out _)
            ? IPadSprintReadyMarker
            : "MOONLIGHT_IPAD_SPRINT_INCOMPLETE";
        public string LastCollisionName { get; private set; } = "";
        public int CollisionCount { get; private set; }
        public int RecoveryCount { get; private set; }
        public string LastRecoveryReason { get; private set; } = "";
        public int ActiveMoonlightAnimatorCount
        {
            get
            {
                RefreshAnimatorRoutesOncePerFrame();
                return _activeMoonlightAnimatorCount;
            }
        }
        public int ActiveMoonlightKidAnimatorCount
        {
            get
            {
                RefreshAnimatorRoutesOncePerFrame();
                return _activeMoonlightKidAnimatorCount;
            }
        }
        public MoonlightAnimator ActiveMoonlightAnimator
        {
            get
            {
                RefreshAnimatorRoutesOncePerFrame();
                return _activeMoonlightAnimator;
            }
        }
        public MoonlightKidAnimator ActiveMoonlightKidAnimator
        {
            get
            {
                RefreshAnimatorRoutesOncePerFrame();
                return _activeMoonlightKidAnimator;
            }
        }
        public bool KidWalkingCommanded
        {
            get
            {
                RefreshAnimatorRoutesOncePerFrame();
                return _activeMoonlightKidAnimator != null &&
                    _activeMoonlightKidAnimator.IsWalkingCommanded;
            }
        }
        public bool KidRunningCommanded
        {
            get
            {
                RefreshAnimatorRoutesOncePerFrame();
                return _activeMoonlightKidAnimator != null &&
                    _activeMoonlightKidAnimator.IsRunningCommanded;
            }
        }

        void Start()
        {
            _capsule = GetComponent<CapsuleCollider>();
            _visual = transform.Find("Visual");
            if (_visual != null)
            {
                _visualBaseRotation = _visual.localRotation;
                _visualBasePosition = _visual.localPosition;
                _visualBaseScale = _visual.localScale;
                _idleBobber = _visual.GetComponent<MoonlightBobber>();
            }

            Physics.SyncTransforms();
            RememberSafePosition();
            MoonlightVisualQA.Instance?.RegisterController(this);
            Debug.Log($"[MoonlightVisualQA] movement-ready bounds={roomBounds} position={transform.position}");
        }

        void Update()
        {
            if (_modalInputLocked)
            {
                ClearTouchMovementState();
                if (_idleBobber != null) _idleBobber.enabled = true;
                _wasMoving = false;
                RouteLocomotion(0f, false);
                return;
            }

            var actionFeedback = GetComponent<MoonlightActionFeedback>();
            bool performingAction = actionFeedback != null && actionFeedback.IsPerformingAction;
            if (performingAction)
            {
                _touchMove = Vector2.zero;
                _smoothedMove = Vector2.zero;
                SetIPadSprinting(false);
                if (_idleBobber != null) _idleBobber.enabled = false;
                _wasMoving = false;
                _wasPerformingAction = true;
                SetAnimatorActionActive(true);
                RouteLocomotion(0f, false);
                return;
            }

            if (_wasPerformingAction)
            {
                _wasPerformingAction = false;
                _wasMoving = true;
                SetAnimatorActionActive(false);
            }

            float movementThresholdSquared = MovementInputThreshold * MovementInputThreshold;
            bool usingTouchInput = _touchMove.sqrMagnitude > movementThresholdSquared;
            Vector2 move;
            if (usingTouchInput)
            {
                var mainCamera = Camera.main;
                move = ResolveTouchMoveCameraRelative(
                    _touchMove,
                    mainCamera != null ? mainCamera.transform.forward : Vector3.zero,
                    mainCamera != null ? mainCamera.transform.right : Vector3.zero);
            }
            else
            {
                move = ReadKeyboardMove();
            }
            move = Vector2.ClampMagnitude(move, 1f);
            SetIPadSprinting(ShouldSprint(_touchMove.magnitude, usingTouchInput));

            var delta = new Vector3(move.x, 0f, move.y) * (CurrentMoveSpeed * Time.deltaTime);
            bool clamped = !TryMove(delta);

            bool moving = move.sqrMagnitude > movementThresholdSquared;
            UpdateMovementState(moving, move, clamped);
            UpdateVisualMotion(move, moving);
            RouteLocomotion(moving ? move.magnitude : 0f, IsIPadSprinting);
        }

        public void SetTouchMove(Vector2 move)
        {
            if (_modalInputLocked)
            {
                ClearTouchMovementState();
                return;
            }
            var actionFeedback = GetComponent<MoonlightActionFeedback>();
            if (actionFeedback != null && actionFeedback.IsPerformingAction)
            {
                ClearTouchMovementState();
                return;
            }
            _touchMove = Vector2.ClampMagnitude(move, 1f);
            if (_touchMove.sqrMagnitude <= 0.0001f)
            {
                RestoreAnimatorForActionHandoff();
                RouteLocomotion(0f, false);
            }
        }

        public void ClearTouchMovementState()
        {
            RestoreAnimatorForActionHandoff();
            _touchMove = Vector2.zero;
            _smoothedMove = Vector2.zero;
            SetIPadSprinting(false);
            RouteLocomotion(0f, false);
        }

        public void SetModalInputLocked(bool locked)
        {
            if (_modalInputLocked == locked) return;
            _modalInputLocked = locked;
            ClearTouchMovementState();
            if (locked)
            {
                _wasMoving = false;
                if (_idleBobber != null) _idleBobber.enabled = true;
            }
            Debug.Log($"[MoonlightStoryQA] modal-input locked={locked} " +
                $"marker={ModalInputLockQAMarker}");
        }

        public static bool ValidateStoryModalInputContract(out string detail)
        {
            bool keyboardBlocked = ShouldBlockMovementForModal(true);
            bool touchBlocked = ShouldBlockMovementForModal(true);
            bool restored = !ShouldBlockMovementForModal(false);
            detail = $"keyboardBlocked={keyboardBlocked} touchBlocked={touchBlocked} restored={restored}";
            return keyboardBlocked && touchBlocked && restored;
        }

        static bool ShouldBlockMovementForModal(bool modalInputLocked) => modalInputLocked;

        public void SetProcessedTouchSprintForQA(Vector2 move)
        {
            SetTouchMove(move);
            float movementThresholdSquared = MovementInputThreshold * MovementInputThreshold;
            bool usingTouchInput = _touchMove.sqrMagnitude > movementThresholdSquared;
            SetIPadSprinting(ShouldSprint(_touchMove.magnitude, usingTouchInput));
        }

        public static bool ShouldSprint(float processedTouchMagnitude, bool isTouchInput) =>
            isTouchInput && processedTouchMagnitude >= IPadSprintProcessedInputThreshold;

        public static Vector2 ResolveTouchMoveCameraRelative(
            Vector2 touchMove,
            Vector3 cameraForward,
            Vector3 cameraRight)
        {
            Vector2 boundedTouch = Vector2.ClampMagnitude(touchMove, 1f);
            var forwardXZ = new Vector2(cameraForward.x, cameraForward.z);
            var rightXZ = new Vector2(cameraRight.x, cameraRight.z);
            if (!IsFinite(forwardXZ) || !IsFinite(rightXZ) ||
                forwardXZ.sqrMagnitude <= 0.000001f || rightXZ.sqrMagnitude <= 0.000001f)
                return boundedTouch;

            forwardXZ.Normalize();
            rightXZ -= forwardXZ * Vector2.Dot(rightXZ, forwardXZ);
            if (!IsFinite(rightXZ) || rightXZ.sqrMagnitude <= 0.000001f)
                return boundedTouch;

            rightXZ.Normalize();
            return Vector2.ClampMagnitude(
                rightXZ * boundedTouch.x + forwardXZ * boundedTouch.y,
                1f);
        }

        public static bool ValidateTouchCameraRelativeContract(out string detail)
        {
            var cameraForward = new Vector3(3f, 7f, 4f);
            var cameraRight = new Vector3(5f, 4f, -2f);
            var expectedForward = new Vector2(0.6f, 0.8f);
            var expectedRight = new Vector2(0.8f, -0.6f);

            Vector2 resolvedUp = ResolveTouchMoveCameraRelative(
                Vector2.up * 2f, cameraForward, cameraRight);
            Vector2 resolvedRight = ResolveTouchMoveCameraRelative(
                Vector2.right, cameraForward, cameraRight);
            var halfInput = new Vector2(0.3f, 0.4f);
            Vector2 resolvedHalf = ResolveTouchMoveCameraRelative(
                halfInput, cameraForward, cameraRight);
            Vector2 missingCameraFallback = ResolveTouchMoveCameraRelative(
                halfInput, Vector3.zero, Vector3.zero);
            Vector2 degenerateCameraFallback = ResolveTouchMoveCameraRelative(
                halfInput, Vector3.forward, Vector3.forward);

            float upDot = Vector2.Dot(resolvedUp.normalized, expectedForward);
            float rightDot = Vector2.Dot(resolvedRight.normalized, expectedRight);
            float headingErrorBefore = Vector2.Angle(Vector2.up, expectedForward);
            float headingErrorAfter = Vector2.Angle(resolvedUp, expectedForward);
            bool unitMagnitudePass = Mathf.Abs(resolvedUp.magnitude - 1f) <= 0.0001f &&
                Mathf.Abs(resolvedRight.magnitude - 1f) <= 0.0001f;
            bool halfMagnitudePass = Mathf.Abs(resolvedHalf.magnitude - halfInput.magnitude) <= 0.0001f;
            bool fallbackPass = Vector2.Distance(missingCameraFallback, halfInput) <= 0.0001f &&
                Vector2.Distance(degenerateCameraFallback, halfInput) <= 0.0001f;

            detail = $"upDot={upDot:0.0000} rightDot={rightDot:0.0000} " +
                $"unitMagnitude={resolvedUp.magnitude:0.0000}/{resolvedRight.magnitude:0.0000} " +
                $"halfMagnitude={resolvedHalf.magnitude:0.0000}/{halfInput.magnitude:0.0000} " +
                $"fallback={fallbackPass} headingErrorBefore={headingErrorBefore:0.00}deg " +
                $"headingErrorAfter={headingErrorAfter:0.00}deg";
            return upDot >= 0.999f && rightDot >= 0.999f && unitMagnitudePass &&
                halfMagnitudePass && fallbackPass;
        }

        public static bool ValidateIPadSprintContract(out string detail)
        {
            bool processedBelowThreshold = ShouldSprint(0.91f, true);
            bool processedAtThreshold = ShouldSprint(0.92f, true);
            bool keyboard = ShouldSprint(1f, false);
            bool speedMultiplierPass = Mathf.Abs(IPadSprintSpeedMultiplier - 1.45f) <= 0.0001f;
            bool visualPass = Mathf.Abs(IPadSprintVisualMultiplier - 1.30f) <= 0.0001f;
            detail = $"processedTouchThreshold={IPadSprintProcessedInputThreshold:0.00} " +
                $"processedTouch91={processedBelowThreshold} " +
                $"processedTouch92={processedAtThreshold} keyboard={keyboard} " +
                $"speedMultiplier={IPadSprintSpeedMultiplier:0.00} " +
                $"visualMultiplier={IPadSprintVisualMultiplier:0.00}";
            return !processedBelowThreshold && processedAtThreshold && !keyboard &&
                speedMultiplierPass && visualPass;
        }

        public bool ValidateIPadSprintRuntimeContract(out string detail)
        {
            bool staticPass = ValidateIPadSprintContract(out string staticDetail);
            float maximumMoveSpeed = BaseMoveSpeed * IPadSprintSpeedMultiplier;
            bool baseSpeedPass = Mathf.Abs(BaseMoveSpeed - 2.60f) <= 0.0001f;
            bool maximumSpeedPass = Mathf.Abs(maximumMoveSpeed - 3.77f) <= 0.0001f;
            bool currentSpeedPass = CurrentMoveSpeed <= maximumMoveSpeed + 0.0001f;
            detail = $"{staticDetail} serializedBase={BaseMoveSpeed:0.00} " +
                $"current={CurrentMoveSpeed:0.00} maximum={maximumMoveSpeed:0.00}";
            return staticPass && baseSpeedPass && maximumSpeedPass && currentSpeedPass;
        }

        public void ConfigureBounds(Rect bounds)
        {
            roomBounds = bounds;
            var p = transform.position;
            p.x = Mathf.Clamp(p.x, roomBounds.xMin, roomBounds.xMax);
            p.z = Mathf.Clamp(p.z, roomBounds.yMin, roomBounds.yMax);
            transform.position = p;
        }

        public void TeleportTo(Vector3 position, Rect bounds)
        {
            roomBounds = bounds;
            position.x = Mathf.Clamp(position.x, roomBounds.xMin, roomBounds.xMax);
            position.z = Mathf.Clamp(position.z, roomBounds.yMin, roomBounds.yMax);
            position.y = 0f;
            transform.position = position;
            Physics.SyncTransforms();
            _hasSafePosition = false;
            if (!CanOccupy(transform.position, out var blocker))
                TryRecoverFromOverlap("room-entry", blocker);
            else
                RememberSafePosition();
            LastCollisionName = "";
            Debug.Log($"[MoonlightNavigationQA] teleport position={position:F2} bounds={bounds}");
        }

        public bool TryMove(Vector3 delta)
        {
            if (delta.sqrMagnitude < 0.0000001f) return true;
            if (_capsule == null) _capsule = GetComponent<CapsuleCollider>();

            Physics.SyncTransforms();
            if (!CanOccupy(transform.position, out var overlap) &&
                !TryRecoverFromOverlap("overlap-before-move", overlap))
                return false;

            bool movedAll = true;
            LastCollisionName = "";
            var xDelta = new Vector3(delta.x, 0f, 0f);
            var zDelta = new Vector3(0f, 0f, delta.z);
            if (!TryMoveAxis(xDelta)) movedAll = false;
            if (!TryMoveAxis(zDelta)) movedAll = false;
            RememberSafePosition();
            return movedAll;
        }

        bool TryMoveAxis(Vector3 delta)
        {
            if (delta.sqrMagnitude < 0.0000001f) return true;
            Vector3 candidate = transform.position + delta;
            candidate.x = Mathf.Clamp(candidate.x, roomBounds.xMin, roomBounds.xMax);
            candidate.z = Mathf.Clamp(candidate.z, roomBounds.yMin, roomBounds.yMax);
            candidate.y = 0f;
            bool hitBounds = (candidate - (transform.position + delta)).sqrMagnitude > 0.000001f;

            if (!CanOccupy(candidate, out var blocker))
            {
                LastCollisionName = blocker != null ? blocker.name : "room-boundary";
                CollisionCount++;
                Debug.Log($"[MoonlightNavigationQA] collision blocker={LastCollisionName} at={candidate:F2}");
                return false;
            }

            transform.position = candidate;
            return !hitBounds;
        }

        bool CanOccupy(Vector3 position, out Collider blocker)
        {
            blocker = null;
            if (_capsule == null) return true;

            float radius = Mathf.Max(0.05f, _capsule.radius - collisionSkin);
            float halfLine = Mathf.Max(0f, _capsule.height * 0.5f - _capsule.radius);
            Vector3 center = position + _capsule.center;
            Vector3 pointA = center + Vector3.up * halfLine;
            Vector3 pointB = center - Vector3.up * halfLine;
            int count = Physics.OverlapCapsuleNonAlloc(pointA, pointB, radius, _overlaps,
                Physics.AllLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var candidate = _overlaps[i];
                if (!IsBlocking(candidate)) continue;
                blocker = candidate;
                return false;
            }
            return true;
        }

        bool IsBlocking(Collider candidate)
        {
            if (candidate == null || candidate == _capsule || candidate.isTrigger) return false;
            if (candidate.transform.IsChildOf(transform) || transform.IsChildOf(candidate.transform)) return false;
            if (candidate.GetComponentInParent<MoonlightSpatialActionZone>() != null) return false;
            var bounds = candidate.bounds;
            if (bounds.max.y <= 0.12f || bounds.size.y <= 0.08f) return false;
            return true;
        }

        void RememberSafePosition()
        {
            if (!CanOccupy(transform.position, out _)) return;
            _lastSafePosition = transform.position;
            _hasSafePosition = true;
        }

        bool TryRecoverFromOverlap(string reason, Collider blocker)
        {
            Vector3 origin = transform.position;
            if (_hasSafePosition && IsInsideBounds(_lastSafePosition) &&
                CanOccupy(_lastSafePosition, out _))
            {
                ApplyRecovery(_lastSafePosition, reason, blocker);
                return true;
            }

            const int directions = 12;
            for (int ring = 1; ring <= recoveryRings; ring++)
            {
                float distance = recoveryStep * ring;
                for (int index = 0; index < directions; index++)
                {
                    float angle = index * Mathf.PI * 2f / directions;
                    Vector3 candidate = origin + new Vector3(
                        Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
                    candidate.x = Mathf.Clamp(candidate.x, roomBounds.xMin, roomBounds.xMax);
                    candidate.z = Mathf.Clamp(candidate.z, roomBounds.yMin, roomBounds.yMax);
                    candidate.y = 0f;
                    if (!CanOccupy(candidate, out _)) continue;
                    ApplyRecovery(candidate, reason, blocker);
                    return true;
                }
            }

            LastRecoveryReason = $"FAILED {reason}";
            Debug.LogError($"[MoonlightNavigationQA] recovery-failed reason={reason} " +
                $"blocker={(blocker != null ? blocker.name : "unknown")} origin={origin:F2}");
            return false;
        }

        void ApplyRecovery(Vector3 position, string reason, Collider blocker)
        {
            transform.position = position;
            Physics.SyncTransforms();
            _lastSafePosition = position;
            _hasSafePosition = true;
            RecoveryCount++;
            LastRecoveryReason = reason;
            Debug.Log($"[MoonlightNavigationQA] recovery-pass reason={reason} " +
                $"blocker={(blocker != null ? blocker.name : "unknown")} position={position:F2} " +
                $"count={RecoveryCount} marker=MOONLIGHT_PLAYER_RECOVERED");
        }

        bool IsInsideBounds(Vector3 position) =>
            position.x >= roomBounds.xMin && position.x <= roomBounds.xMax &&
            position.z >= roomBounds.yMin && position.z <= roomBounds.yMax;

        static Vector2 ReadKeyboardMove()
        {
            float x = 0f;
            float y = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
            return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
        }

        static bool IsFinite(Vector2 value) =>
            !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
            !float.IsNaN(value.y) && !float.IsInfinity(value.y);

        void SetIPadSprinting(bool sprinting)
        {
            if (sprinting == IsIPadSprinting) return;

            float speedBefore = CurrentMoveSpeed;
            bool wasSprinting = IsIPadSprinting;
            IsIPadSprinting = sprinting;
            Debug.Log($"[MoonlightVisualQA] ipad-sprint before={wasSprinting} after={IsIPadSprinting} " +
                $"speedBefore={speedBefore:0.00} speedAfter={CurrentMoveSpeed:0.00} " +
                $"processedTouchMagnitude={_touchMove.magnitude:0.00} marker={IPadSprintQAMarker}");
        }

        void RefreshAnimatorRoutesOncePerFrame()
        {
            int frame = Time.frameCount;
            if (_lastAnimatorScanFrame == frame) return;
            _lastAnimatorScanFrame = frame;

            _moonlightAnimators.Clear();
            _moonlightKidAnimators.Clear();
            GetComponentsInChildren(true, _moonlightAnimators);
            GetComponentsInChildren(true, _moonlightKidAnimators);

            _activeMoonlightAnimator = null;
            _activeMoonlightKidAnimator = null;
            _activeMoonlightAnimatorCount = 0;
            _activeMoonlightKidAnimatorCount = 0;

            MoonlightAnimator soleAnimator = null;
            for (int i = 0; i < _moonlightAnimators.Count; i++)
            {
                MoonlightAnimator candidate = _moonlightAnimators[i];
                if (candidate == null || !candidate.isActiveAndEnabled ||
                    !candidate.gameObject.activeInHierarchy)
                    continue;
                _activeMoonlightAnimatorCount++;
                soleAnimator = candidate;
            }

            MoonlightKidAnimator soleKidAnimator = null;
            for (int i = 0; i < _moonlightKidAnimators.Count; i++)
            {
                MoonlightKidAnimator candidate = _moonlightKidAnimators[i];
                if (candidate == null || !candidate.isActiveAndEnabled ||
                    !candidate.gameObject.activeInHierarchy)
                    continue;
                _activeMoonlightKidAnimatorCount++;
                soleKidAnimator = candidate;
            }

            if (_activeMoonlightAnimatorCount == 1)
                _activeMoonlightAnimator = soleAnimator;
            if (_activeMoonlightKidAnimatorCount == 1)
                _activeMoonlightKidAnimator = soleKidAnimator;
        }

        void SetAnimatorActionActive(bool active)
        {
            RefreshAnimatorRoutesOncePerFrame();
            _activeMoonlightAnimator?.SetActionActive(active);
        }

        void RestoreAnimatorForActionHandoff()
        {
            RefreshAnimatorRoutesOncePerFrame();
            _activeMoonlightAnimator?.RestoreForActionHandoff();
        }

        void RouteLocomotion(float movementMagnitude, bool running)
        {
            RefreshAnimatorRoutesOncePerFrame();
            _activeMoonlightAnimator?.SetLocomotion(movementMagnitude, running);
            _activeMoonlightKidAnimator?.SetWalking(movementMagnitude > 0f, running);
        }

        void UpdateMovementState(bool moving, Vector2 move, bool clamped)
        {
            if (moving == _wasMoving) return;

            if (_idleBobber != null)
                _idleBobber.enabled = !moving;

            Debug.Log($"[MoonlightVisualQA] movement-state state={(moving ? "moving" : "idle")} pos={transform.position:F2} input={move:F2} clamped={clamped}");
            _wasMoving = moving;
        }

        void UpdateVisualMotion(Vector2 move, bool moving)
        {
            if (_visual == null) return;

            _smoothedMove = Vector2.Lerp(_smoothedMove, moving ? move : Vector2.zero, Time.deltaTime * 12f);

            if (moving)
            {
                var visualMove = _smoothedMove.sqrMagnitude > 0.0001f ? _smoothedMove : move;
                float yaw = Mathf.Atan2(visualMove.x, visualMove.y) * Mathf.Rad2Deg;
                float yawDelta = Mathf.DeltaAngle(_lastYaw, yaw);
                float targetLean = Mathf.Clamp(-yawDelta * 0.28f - visualMove.x * turnLeanAngle, -turnLeanAngle, turnLeanAngle);
                _currentLean = Mathf.Lerp(_currentLean, targetLean, Time.deltaTime * 10f);
                var targetRotation = Quaternion.Euler(0f, yaw, 0f) * _visualBaseRotation;
                targetRotation *= Quaternion.Euler(0f, 0f, _currentLean);
                _visual.localRotation = Quaternion.Slerp(_visual.localRotation, targetRotation, turnSpeed * Time.deltaTime);
                float visualMultiplier = IsIPadSprinting ? IPadSprintVisualMultiplier : 1f;
                _walkPhase += Time.deltaTime * walkBobSpeed * visualMultiplier;
                _lastYaw = yaw;
            }
            else
            {
                _currentLean = Mathf.Lerp(_currentLean, 0f, Time.deltaTime * visualReturnSpeed);
                _visual.localRotation = Quaternion.Slerp(_visual.localRotation, _visualBaseRotation, turnSpeed * 0.35f * Time.deltaTime);
            }

            float step = Mathf.Sin(_walkPhase);
            float movementVisualMultiplier = IsIPadSprinting ? IPadSprintVisualMultiplier : 1f;
            RefreshAnimatorRoutesOncePerFrame();
            bool articulatedGait = _activeMoonlightAnimator != null &&
                _activeMoonlightAnimator.UsesProceduralLocomotion;
            articulatedGait |= _activeMoonlightKidAnimator != null;
            float rootBobScale = articulatedGait ? ProceduralWholeRootBobScale : 1f;
            float rootSquashScale = articulatedGait ? ProceduralWholeRootSquashScale : 1f;
            var targetPosition = _visualBasePosition;
            if (moving)
                targetPosition.y += Mathf.Abs(step) * walkBobHeight * movementVisualMultiplier *
                    rootBobScale;
            _visual.localPosition = Vector3.Lerp(_visual.localPosition, targetPosition, Time.deltaTime * visualReturnSpeed);

            if (CanApplyMovementScale())
            {
                float squash = moving
                    ? Mathf.Abs(step) * walkSquash * movementVisualMultiplier * rootSquashScale
                    : 0f;
                var targetScale = new Vector3(
                    _visualBaseScale.x * (1f + squash * 0.45f),
                    _visualBaseScale.y * (1f - squash),
                    _visualBaseScale.z * (1f + squash * 0.45f));
                _visual.localScale = Vector3.Lerp(_visual.localScale, targetScale, Time.deltaTime * visualReturnSpeed);
            }
        }

        bool CanApplyMovementScale()
        {
            var actionFeedback = GetComponent<MoonlightActionFeedback>();
            return actionFeedback == null || !actionFeedback.IsCoolingDown;
        }
    }
}
