using UnityEngine;

namespace MoonlightMagicHouse
{
    // Smooth follow camera with optional orbit on touch/drag.
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(0, 3f, -5f);
        [SerializeField] float followSpeed = 5f;

        [Header("Orbit (mouse/touch drag)")]
        [SerializeField] float orbitSensitivity = 0.4f;
        [SerializeField] float orbitReturnSpeed = 2f;
        [SerializeField] float maxOrbitAngle    = 40f;

        float _orbitX;
        Vector2 _lastPointer;
        bool _dragging;

        void LateUpdate()
        {
            HandleInput();
            ReturnOrbit();
            ApplyPosition();
        }

        void HandleInput()
        {
            if (Input.touchCount == 1)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    _lastPointer = touch.position;
                    _dragging = true;
                }
                else if (touch.phase == TouchPhase.Moved && _dragging)
                {
                    _orbitX += (touch.position.x - _lastPointer.x) * orbitSensitivity;
                    _orbitX = Mathf.Clamp(_orbitX, -maxOrbitAngle, maxOrbitAngle);
                    _lastPointer = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended) _dragging = false;
            }
            else if (Input.GetMouseButton(0))
            {
                float dx = Input.GetAxis("Mouse X");
                _orbitX += dx * orbitSensitivity * 10f;
                _orbitX = Mathf.Clamp(_orbitX, -maxOrbitAngle, maxOrbitAngle);
            }
        }

        void ReturnOrbit()
        {
            if (!_dragging && !Input.GetMouseButton(0))
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
