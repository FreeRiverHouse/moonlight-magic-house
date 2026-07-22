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
        public const string IPadSprintReadyMarker = "MOONLIGHT_IPAD_SPRINT_READY";

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
        CapsuleCollider _capsule;
        readonly Collider[] _overlaps = new Collider[24];
        Vector3 _lastSafePosition;
        bool _hasSafePosition;

        public Rect RoomBounds => roomBounds;
        public Vector2 TouchMove => _touchMove;
        public float BaseMoveSpeed => moveSpeed;
        public bool IsIPadSprinting { get; private set; }
        public float CurrentMoveSpeed => moveSpeed *
            (IsIPadSprinting ? IPadSprintSpeedMultiplier : 1f);
        public string IPadSprintQAMarker => ValidateIPadSprintRuntimeContract(out _)
            ? IPadSprintReadyMarker
            : "MOONLIGHT_IPAD_SPRINT_INCOMPLETE";
        public string LastCollisionName { get; private set; } = "";
        public int CollisionCount { get; private set; }
        public int RecoveryCount { get; private set; }
        public string LastRecoveryReason { get; private set; } = "";

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
                GetComponentInChildren<MoonlightAnimator>()?.SetWalking(false);
                return;
            }

            if (_wasPerformingAction)
            {
                _wasPerformingAction = false;
                _wasMoving = true;
            }

            float movementThresholdSquared = MovementInputThreshold * MovementInputThreshold;
            bool usingTouchInput = _touchMove.sqrMagnitude > movementThresholdSquared;
            var move = usingTouchInput ? _touchMove : ReadKeyboardMove();
            move = Vector2.ClampMagnitude(move, 1f);
            SetIPadSprinting(ShouldSprint(move.magnitude, usingTouchInput));

            var delta = new Vector3(move.x, 0f, move.y) * (CurrentMoveSpeed * Time.deltaTime);
            bool clamped = !TryMove(delta);

            bool moving = move.sqrMagnitude > movementThresholdSquared;
            UpdateMovementState(moving, move, clamped);
            UpdateVisualMotion(move, moving);
            GetComponentInChildren<MoonlightAnimator>()?.SetWalking(moving);
        }

        public void SetTouchMove(Vector2 move)
        {
            var actionFeedback = GetComponent<MoonlightActionFeedback>();
            if (actionFeedback != null && actionFeedback.IsPerformingAction)
            {
                ClearTouchMovementState();
                return;
            }
            _touchMove = Vector2.ClampMagnitude(move, 1f);
        }

        public void ClearTouchMovementState()
        {
            _touchMove = Vector2.zero;
            _smoothedMove = Vector2.zero;
            SetIPadSprinting(false);
        }

        public void SetProcessedTouchSprintForQA(Vector2 move)
        {
            SetTouchMove(move);
            float movementThresholdSquared = MovementInputThreshold * MovementInputThreshold;
            bool usingTouchInput = _touchMove.sqrMagnitude > movementThresholdSquared;
            SetIPadSprinting(ShouldSprint(_touchMove.magnitude, usingTouchInput));
        }

        public static bool ShouldSprint(float processedTouchMagnitude, bool isTouchInput) =>
            isTouchInput && processedTouchMagnitude >= IPadSprintProcessedInputThreshold;

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
            var targetPosition = _visualBasePosition;
            if (moving)
                targetPosition.y += Mathf.Abs(step) * walkBobHeight * movementVisualMultiplier;
            _visual.localPosition = Vector3.Lerp(_visual.localPosition, targetPosition, Time.deltaTime * visualReturnSpeed);

            if (CanApplyMovementScale())
            {
                float squash = moving
                    ? Mathf.Abs(step) * walkSquash * movementVisualMultiplier
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
