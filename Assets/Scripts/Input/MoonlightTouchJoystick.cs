using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MoonlightMagicHouse
{
    public class MoonlightTouchJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] RectTransform knob;
        [SerializeField] float radius = 74f;

        MoonlightPlayerController _controller;
        RectTransform _rect;
        Vector2 _value;
        int _activePointer = int.MinValue;

        public Vector2 Value => _value;

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
            _activePointer = int.MinValue;
            _value = Vector2.zero;
            if (knob) knob.anchoredPosition = Vector2.zero;
            _controller?.SetTouchMove(Vector2.zero);
        }

        void UpdateValue(PointerEventData eventData)
        {
            if (_rect == null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rect, eventData.position, eventData.pressEventCamera, out var local);
            local = Vector2.ClampMagnitude(local, radius);
            _value = local / radius;
            if (knob) knob.anchoredPosition = local;
            _controller?.SetTouchMove(_value);
        }
    }
}
