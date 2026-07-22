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
        [SerializeField, Range(0.08f, 0.25f)] float movementFloor = 0.14f;

        MoonlightPlayerController _controller;
        RectTransform _rect;
        Image _baseImage;
        Image _knobImage;
        Vector2 _value;
        int _activePointer = int.MinValue;
        readonly Color _idleBaseColor = new(0.12f, 0.12f, 0.14f, 0.42f);
        readonly Color _activeBaseColor = new(0.18f, 0.25f, 0.32f, 0.66f);
        readonly Color _idleKnobColor = new(0.78f, 0.84f, 0.88f, 0.78f);
        readonly Color _activeKnobColor = new(0.70f, 0.94f, 1.00f, 0.96f);

        public Vector2 Value => _value;
        public bool IsTrackingPointer => _activePointer != int.MinValue;
        public float DeadZone => deadZone;
        public float MovementFloor => movementFloor;
        public float ResponseExponent => responseExponent;
        public float Radius => radius;
        public Vector2 TouchTargetSize => _rect != null ? _rect.rect.size : Vector2.zero;
        public string ResponseQAMarker => radius >= 70f && deadZone >= 0.08f && deadZone <= 0.16f &&
            movementFloor >= 0.12f && movementFloor <= 0.18f && responseExponent < 1f &&
            TouchTargetSize.x >= 160f && TouchTargetSize.y >= 160f
                ? "MOONLIGHT_IPAD_JOYSTICK_RESPONSE_READY"
                : "MOONLIGHT_IPAD_JOYSTICK_RESPONSE_INCOMPLETE";

        void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _baseImage = GetComponent<Image>();
            if (knob == null)
            {
                var found = transform.Find("Knob");
                if (found != null) knob = found as RectTransform;
            }
            if (knob != null) _knobImage = knob.GetComponent<Image>();
            SetVisualState(false, 0f);
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
            HapticFeedback.Light();
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
            float magnitude = EvaluateResponse(rawMagnitude, deadZone, responseExponent, movementFloor);
            _value = local.sqrMagnitude > 0.0001f
                ? local.normalized * magnitude
                : Vector2.zero;
            if (knob) knob.anchoredPosition = local;
            SetVisualState(true, magnitude);
            _controller?.SetTouchMove(_value);
        }

        void ResetInput(string reason)
        {
            bool hadInput = _activePointer != int.MinValue || _value.sqrMagnitude > 0.0001f;
            _activePointer = int.MinValue;
            _value = Vector2.zero;
            if (knob) knob.anchoredPosition = Vector2.zero;
            SetVisualState(false, 0f);
            _controller?.SetTouchMove(Vector2.zero);
            if (hadInput)
                Debug.Log($"[MoonlightNavigationQA] joystick-reset reason={reason}");
        }

        void SetVisualState(bool active, float magnitude)
        {
            float amount = active ? Mathf.Lerp(0.45f, 1f, Mathf.Clamp01(magnitude)) : 0f;
            if (_baseImage != null)
                _baseImage.color = Color.Lerp(_idleBaseColor, _activeBaseColor, amount);
            if (_knobImage != null)
                _knobImage.color = Color.Lerp(_idleKnobColor, _activeKnobColor, amount);
            if (knob != null)
                knob.localScale = Vector3.one * (active ? Mathf.Lerp(1.03f, 1.10f, magnitude) : 1f);
        }

        public static float EvaluateResponse(float rawMagnitude, float deadZone,
                                             float responseExponent, float movementFloor)
        {
            rawMagnitude = Mathf.Clamp01(rawMagnitude);
            deadZone = Mathf.Clamp(deadZone, 0f, 0.95f);
            if (rawMagnitude <= deadZone) return 0f;
            float normalized = (rawMagnitude - deadZone) / Mathf.Max(0.01f, 1f - deadZone);
            float curved = Mathf.Pow(Mathf.Clamp01(normalized), Mathf.Max(0.1f, responseExponent));
            return Mathf.Lerp(Mathf.Clamp01(movementFloor), 1f, curved);
        }

        public static bool ValidateResponseContract(out string detail)
        {
            const float contractDeadZone = 0.12f;
            const float contractExponent = 0.82f;
            const float contractFloor = 0.14f;
            float center = EvaluateResponse(0f, contractDeadZone, contractExponent, contractFloor);
            float edgeOfDeadZone = EvaluateResponse(contractDeadZone, contractDeadZone,
                contractExponent, contractFloor);
            float firstMotion = EvaluateResponse(0.13f, contractDeadZone,
                contractExponent, contractFloor);
            float half = EvaluateResponse(0.50f, contractDeadZone,
                contractExponent, contractFloor);
            float full = EvaluateResponse(1f, contractDeadZone,
                contractExponent, contractFloor);
            detail = $"center={center:0.000} dead={edgeOfDeadZone:0.000} " +
                $"first={firstMotion:0.000} half={half:0.000} full={full:0.000}";
            return center == 0f && edgeOfDeadZone == 0f &&
                firstMotion >= MoonlightPlayerController.MovementInputThreshold * 2f &&
                half > firstMotion && half >= 0.50f && full >= 0.999f;
        }
    }
}
