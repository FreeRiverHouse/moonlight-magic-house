using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MoonlightMagicHouse
{
    public class MoonlightTouchJoystick : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler, ICancelHandler
    {
        [SerializeField] RectTransform knob;
        [SerializeField] float radius = 74f;
        [SerializeField, Range(0f, 0.35f)] float deadZone = 0.12f;
        [SerializeField, Range(0.5f, 1.5f)] float responseExponent = 0.82f;

        MoonlightPlayerController _controller;
        RectTransform _rect;
        Vector2 _value;
        int _activePointer = int.MinValue;

        public Vector2 Value => _value;
        public bool IsTrackingPointer => _activePointer != int.MinValue;

        void Awake()
        {
            _rect = GetComponent<RectTransform>();
            if (knob == null)
            {
                var found = transform.Find("Knob");
                if (found != null) knob = found as RectTransform;
            }
        }

        void Start()
        {
            _controller = FindAnyObjectByType<MoonlightPlayerController>();
        }

        public void Bind(MoonlightPlayerController controller)
        {
            _controller = controller;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_activePointer != int.MinValue && _activePointer != eventData.pointerId) return;
            _activePointer = eventData.pointerId;
            UpdateValue(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_activePointer != eventData.pointerId) return;
            UpdateValue(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_activePointer != eventData.pointerId) return;
            ResetInput("pointer-up");
        }

        public void OnCancel(BaseEventData eventData) => ResetInput("event-cancel");

        void OnDisable() => ResetInput("disabled");

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) ResetInput("focus-lost");
        }

        void UpdateValue(PointerEventData eventData)
        {
            if (_rect == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rect, eventData.position, eventData.pressEventCamera, out var local)) return;
            local = Vector2.ClampMagnitude(local, radius);
            float rawMagnitude = Mathf.Clamp01(local.magnitude / Mathf.Max(1f, radius));
            float magnitude = rawMagnitude <= deadZone
                ? 0f
                : Mathf.Pow((rawMagnitude - deadZone) / Mathf.Max(0.01f, 1f - deadZone), responseExponent);
            _value = local.sqrMagnitude > 0.0001f
                ? local.normalized * magnitude
                : Vector2.zero;
            if (knob) knob.anchoredPosition = local;
            _controller?.SetTouchMove(_value);
        }

        void ResetInput(string reason)
        {
            bool hadInput = _activePointer != int.MinValue || _value.sqrMagnitude > 0.0001f;
            _activePointer = int.MinValue;
            _value = Vector2.zero;
            if (knob) knob.anchoredPosition = Vector2.zero;
            _controller?.SetTouchMove(Vector2.zero);
            if (hadInput)
                Debug.Log($"[MoonlightNavigationQA] joystick-reset reason={reason}");
        }
    }
}
