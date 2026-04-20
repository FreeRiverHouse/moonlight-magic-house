using UnityEngine;
using UnityEngine.InputSystem;

namespace MoonlightMagicHouse
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(0, 2f, -4f);
        [SerializeField] float followSpeed = 5f;

        [Header("Orbit (mouse/touch drag)")]
        [SerializeField] float orbitSensitivity = 0.4f;
        [SerializeField] float orbitReturnSpeed = 2f;
        [SerializeField] float maxOrbitAngle    = 40f;

        float   _orbitX;
        Vector2 _lastPointer;
        bool    _dragging;
        bool    _mouseDown;

        void LateUpdate()
        {
            HandleInput();
            ReturnOrbit();
            ApplyPosition();
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
            var rotatedOffset = Quaternion.Euler(0, _orbitX, 0) * offset;
            var desired = target.position + rotatedOffset;
            transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * followSpeed);
            transform.LookAt(target.position + Vector3.up * 0.5f);
        }

        public void SetTarget(Transform t) => target = t;
    }
}
