using UnityEngine;
using UnityEngine.InputSystem;

namespace MoonlightMagicHouse
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(4.45f, 3.10f, -5.35f);
        [SerializeField] Vector3 lookOffset = new Vector3(0.22f, 1.38f, 0.45f);
        [SerializeField] float followSpeed = 4.2f;

        [Header("Orbit (mouse/touch drag)")]
        [SerializeField] float orbitSensitivity = 0.16f;
        [SerializeField] float orbitReturnSpeed = 3.5f;
        [SerializeField] float maxOrbitAngle    = 18f;

        [Header("Activity Focus")]
        [SerializeField, Range(0.70f, 1f)] float activityDistanceScale = 0.86f;
        [SerializeField, Range(0.90f, 1.20f)] float cookDistanceScale = 0.94f;
        [SerializeField] Vector3 cookFocusOffset = new Vector3(-3.85f, 3.18f, -4.55f);
        [SerializeField, Range(0.85f, 1.10f)] float playDistanceScale = 0.90f;
        [SerializeField] Vector3 playFocusOffset = new Vector3(4.80f, 3.30f, -5.45f);
        [SerializeField, Range(0.85f, 1.10f)] float gardenDistanceScale = 0.90f;
        [SerializeField] Vector3 gardenFocusOffset = new Vector3(3.35f, 2.82f, -4.05f);
        [SerializeField, Range(0.85f, 1.10f)] float readDistanceScale = 0.88f;
        [SerializeField] Vector3 readFocusOffset = new Vector3(3.05f, 2.55f, -3.65f);
        [SerializeField, Min(0.05f)] float activityFocusInTime = 0.28f;
        [SerializeField, Min(0.05f)] float activityFocusOutTime = 0.42f;
        [SerializeField] float activityLookHeight = 0.78f;
        [SerializeField] float cookLookHeight = 1.02f;
        [SerializeField] float playLookHeight = 0.82f;
        [SerializeField] float gardenLookHeight = 0.70f;
        [SerializeField] float readLookHeight = 0.66f;

        float   _orbitX;
        Vector3 _activeOffset;
        Vector3 _activeLookOffset;
        Vector2 _lastPointer;
        bool    _dragging;
        bool    _mouseDown;
        bool    _activityFocusRequested;
        bool    _activityFocusUsesStationAnchor;
        float   _activityFocusBlend;
        float   _activityFocusBlendVelocity;
        Vector3 _activityFocusCenter;
        MoonlightSpatialActionKind _activityFocusKind;

        public bool ActivityFocusRequested => _activityFocusRequested;
        public bool IsActivityFocusActive => _activityFocusRequested || _activityFocusBlend > 0.001f;
        public float ActivityFocusBlend => _activityFocusBlend;
        public Vector3 ActivityFocusCenter => _activityFocusCenter;
        public MoonlightSpatialActionKind ActivityFocusKind => _activityFocusKind;
        public bool ActivityFocusUsesStationAnchor => _activityFocusUsesStationAnchor;
        public string ActivityFocusSource => _activityFocusUsesStationAnchor
            ? "station-anchor"
            : "safe-midpoint";
        public string ActivityFocusFramingProfile => _activityFocusKind switch
        {
            MoonlightSpatialActionKind.Cook => "cook-three-quarter",
            MoonlightSpatialActionKind.Play => "play-wide-arena",
            MoonlightSpatialActionKind.Garden => "garden-close-bloom",
            MoonlightSpatialActionKind.Read => "read-intimate-nook",
            _ => "activity-standard"
        };

        void Awake()
        {
            _activeOffset = offset;
            _activeLookOffset = lookOffset;
        }

        void LateUpdate()
        {
            HandleInput();
            ReturnOrbit();
            UpdateActivityFocusBlend();
            ApplyPosition();
        }

        void UpdateActivityFocusBlend()
        {
            float targetBlend = _activityFocusRequested ? 1f : 0f;
            float smoothTime = _activityFocusRequested ? activityFocusInTime : activityFocusOutTime;
            _activityFocusBlend = Mathf.SmoothDamp(
                _activityFocusBlend,
                targetBlend,
                ref _activityFocusBlendVelocity,
                smoothTime,
                Mathf.Infinity,
                Time.deltaTime);

            if (!_activityFocusRequested && _activityFocusBlend < 0.001f)
            {
                _activityFocusBlend = 0f;
                _activityFocusBlendVelocity = 0f;
            }
        }

        void HandleInput()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.touches.Count > 0)
            {
                var touch = touchscreen.touches[0];
                var phase = touch.phase.ReadValue();

                if (phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    _lastPointer = touch.position.ReadValue();
                    _dragging = true;
                }
                else if (phase == UnityEngine.InputSystem.TouchPhase.Moved && _dragging)
                {
                    var pos = touch.position.ReadValue();
                    _orbitX += (pos.x - _lastPointer.x) * orbitSensitivity;
                    _orbitX = Mathf.Clamp(_orbitX, -maxOrbitAngle, maxOrbitAngle);
                    _lastPointer = pos;
                }
                else if (phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                         phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    _dragging = false;
                }
                return;
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                bool pressed = mouse.leftButton.isPressed;
                if (pressed && !_mouseDown)
                {
                    _lastPointer = mouse.position.ReadValue();
                    _mouseDown   = true;
                }
                else if (pressed && _mouseDown)
                {
                    var pos = mouse.position.ReadValue();
                    float dx = (pos.x - _lastPointer.x) / Screen.width * 100f;
                    _orbitX += dx * orbitSensitivity * 10f;
                    _orbitX = Mathf.Clamp(_orbitX, -maxOrbitAngle, maxOrbitAngle);
                    _lastPointer = pos;
                }
                else
                {
                    _mouseDown = false;
                }
            }
        }

        void ReturnOrbit()
        {
            bool mouseHeld = Mouse.current != null && Mouse.current.leftButton.isPressed;
            if (!_dragging && !mouseHeld)
                _orbitX = Mathf.Lerp(_orbitX, 0f, Time.deltaTime * orbitReturnSpeed);
        }

        void ApplyPosition()
        {
            if (target == null) return;
            float focusBlend = Mathf.SmoothStep(0f, 1f, _activityFocusBlend);
            var orbitRotation = Quaternion.Euler(0f, _orbitX, 0f);
            var followCenter = Vector3.Lerp(target.position, _activityFocusCenter, focusBlend);
            float focusDistanceScale = _activityFocusKind switch
            {
                MoonlightSpatialActionKind.Cook => cookDistanceScale,
                MoonlightSpatialActionKind.Play => playDistanceScale,
                MoonlightSpatialActionKind.Garden => gardenDistanceScale,
                MoonlightSpatialActionKind.Read => readDistanceScale,
                _ => activityDistanceScale
            };
            var focusOffset = _activityFocusKind switch
            {
                MoonlightSpatialActionKind.Cook => cookFocusOffset * focusDistanceScale,
                MoonlightSpatialActionKind.Play => playFocusOffset * focusDistanceScale,
                MoonlightSpatialActionKind.Garden => gardenFocusOffset * focusDistanceScale,
                MoonlightSpatialActionKind.Read => readFocusOffset * focusDistanceScale,
                _ => _activeOffset * focusDistanceScale
            };
            var blendedOffset = Vector3.Lerp(_activeOffset, focusOffset, focusBlend);
            var desired = followCenter + orbitRotation * blendedOffset;
            var normalLookPoint = target.position + _activeLookOffset;
            float focusLookHeight = _activityFocusKind switch
            {
                MoonlightSpatialActionKind.Cook => cookLookHeight,
                MoonlightSpatialActionKind.Play => playLookHeight,
                MoonlightSpatialActionKind.Garden => gardenLookHeight,
                MoonlightSpatialActionKind.Read => readLookHeight,
                _ => activityLookHeight
            };
            var activityLookPoint = _activityFocusCenter + Vector3.up * focusLookHeight;
            var lookPoint = Vector3.Lerp(normalLookPoint, activityLookPoint, focusBlend);
            transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * followSpeed);
            transform.LookAt(lookPoint);
        }

        public void SetTarget(Transform t) => target = t;

        public bool BeginActivityFocus(MoonlightSpatialActionKind kind, Vector3 moonlightPosition,
            Vector3 activityAnchor, bool usesStationAnchor)
        {
            if (kind == MoonlightSpatialActionKind.SleepCuddle)
                return false;

            _activityFocusKind = kind;
            _activityFocusUsesStationAnchor = usesStationAnchor;
            float anchorWeight = kind switch
            {
                MoonlightSpatialActionKind.Cook => 0.62f,
                MoonlightSpatialActionKind.Play => 0.72f,
                MoonlightSpatialActionKind.Garden => 0.68f,
                MoonlightSpatialActionKind.Read => 0.66f,
                _ => 0.5f
            };
            _activityFocusCenter = Vector3.Lerp(moonlightPosition, activityAnchor, anchorWeight);
            _activityFocusRequested = true;
            Debug.Log($"[MoonlightCameraQA] activity-focus-begin kind={kind} " +
                $"source={ActivityFocusSource} center={_activityFocusCenter:F2} " +
                $"anchor={activityAnchor:F2} profile={ActivityFocusFramingProfile} " +
                "marker=MOONLIGHT_ACTIVITY_FOCUS_BEGIN");
            return true;
        }

        public void EndActivityFocus()
        {
            if (!_activityFocusRequested)
                return;

            Debug.Log($"[MoonlightCameraQA] activity-focus-end kind={_activityFocusKind} " +
                $"source={ActivityFocusSource} marker=MOONLIGHT_ACTIVITY_FOCUS_END");
            _activityFocusRequested = false;
        }

        public void SetRoomProfile(RoomType room, bool snap)
        {
            bool heroRoom = room == RoomType.LivingRoom;
            _activeOffset = heroRoom ? offset : new Vector3(3.55f, 3.25f, -4.35f);
            _activeLookOffset = heroRoom ? lookOffset : new Vector3(0.08f, 1.22f, 0.48f);
            _orbitX = 0f;
            if (snap && target != null)
            {
                transform.position = target.position + _activeOffset;
                transform.LookAt(target.position + _activeLookOffset);
            }
            Debug.Log($"[MoonlightRoomQA] camera-profile room={room} offset={_activeOffset:F2}");
        }
    }
}
