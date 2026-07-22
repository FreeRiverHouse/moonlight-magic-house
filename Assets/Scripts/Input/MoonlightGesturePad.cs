using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MoonlightMagicHouse
{
    public enum MoonlightGestureKind
    {
        Tap,
        Circle,
        Hold,
        Swipe,
        ZigZag
    }

    public sealed class MoonlightGesturePad : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler, ICancelHandler
    {
        readonly List<Vector2> _points = new();

        MoonlightUI _ui;
        RectTransform _rect;
        MoonlightGestureKind _gesture;
        float _startedAt;
        int _pointerId = int.MinValue;
        MoonlightSpatialActionZone _startedZone;
        Image _surface;
        Color _baseColor = Color.white;
        Vector3 _baseScale = Vector3.one;
        float _feedbackUntil;
        Color _feedbackColor;

        public MoonlightGestureKind ActiveGesture => _gesture;
        public float LastScore { get; private set; }
        public string LastRejectionReason { get; private set; } = "";
        public bool IsTrackingGesture => _pointerId != int.MinValue;
        public bool IsAcceptingGesture
        {
            get
            {
                var zone = CurrentZone();
                return CanAcceptGesture(zone, out _);
            }
        }

        void Awake()
        {
            _rect = transform as RectTransform;
            _surface = GetComponent<Image>();
            if (_surface != null) _baseColor = _surface.color;
            _baseScale = transform.localScale;
        }

        void Update()
        {
            if (_pointerId != int.MinValue) return;

            if (_surface != null)
            {
                float remaining = Mathf.Clamp01((_feedbackUntil - Time.unscaledTime) / 0.22f);
                _surface.color = remaining > 0f
                    ? Color.Lerp(_baseColor, _feedbackColor, remaining * 0.55f)
                    : _baseColor;
            }
            transform.localScale = Vector3.Lerp(transform.localScale, _baseScale,
                Time.unscaledDeltaTime * 18f);
        }

        public void Bind(MoonlightUI ui) => _ui = ui;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pointerId != int.MinValue) return;
            var zone = CurrentZone();
            if (!CanAcceptGesture(zone, out string reason))
            {
                Reject(reason);
                return;
            }

            _pointerId = eventData.pointerId;
            _startedZone = zone;
            _gesture = zone.RequiredGesture;
            _startedAt = Time.unscaledTime;
            _points.Clear();
            LastRejectionReason = "";
            SetTrackingVisual();
            AddPoint(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_pointerId != eventData.pointerId) return;
            AddPoint(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_pointerId != eventData.pointerId) return;
            AddPoint(eventData);
            var startedZone = _startedZone;
            float duration = Time.unscaledTime - _startedAt;
            _pointerId = int.MinValue;
            _startedZone = null;
            RestoreTrackingVisual();

            if (startedZone == null || CurrentZone() != startedZone)
            {
                _points.Clear();
                Reject("ZONE CHANGED");
                return;
            }
            if (!CanAcceptGesture(startedZone, out string reason))
            {
                _points.Clear();
                Reject(reason);
                return;
            }

            LastScore = ScoreGesture(_gesture, _points, duration);
            Debug.Log($"[MoonlightActivityQA] gesture kind={_gesture} score={LastScore:0.00} points={_points.Count}");
            _ui?.ExecuteContextGesture(_gesture, LastScore);
            SetResultVisual(startedZone.LastGesturePassed);
            _points.Clear();
        }

        public bool SubmitSynthetic(MoonlightGestureKind gesture, float score)
        {
            var zone = CurrentZone();
            if (!CanAcceptGesture(zone, out string reason))
            {
                Reject(reason);
                return false;
            }

            _gesture = gesture;
            LastScore = Mathf.Clamp01(score);
            LastRejectionReason = "";
            _ui?.ExecuteContextGesture(gesture, LastScore);
            SetResultVisual(zone.LastGesturePassed);
            return true;
        }

        public void OnCancel(BaseEventData eventData) => CancelTracking("event-cancel");

        void OnDisable() => CancelTracking("disabled");

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) CancelTracking("focus-lost");
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) CancelTracking("paused");
        }

        void CancelTracking(string reason)
        {
            bool wasTracking = _pointerId != int.MinValue;
            _pointerId = int.MinValue;
            _startedZone = null;
            _points.Clear();
            RestoreTrackingVisual();
            if (wasTracking)
                Debug.Log($"[MoonlightActivityQA] gesture-cancelled reason={reason}");
        }

        void AddPoint(PointerEventData eventData)
        {
            if (_rect == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rect, eventData.position, eventData.pressEventCamera, out var local)) return;

            var size = _rect.rect.size;
            local.x /= Mathf.Max(1f, size.x);
            local.y /= Mathf.Max(1f, size.y);
            if (_points.Count == 0 || Vector2.Distance(_points[^1], local) > 0.015f)
                _points.Add(local);
        }

        MoonlightSpatialActionZone CurrentZone()
        {
            var moonlight = MoonlightGameManager.Instance?.moonlight;
            return moonlight != null
                ? moonlight.GetComponent<MoonlightSpatialInteractor>()?.CurrentZone
                : null;
        }

        bool CanAcceptGesture(MoonlightSpatialActionZone zone, out string reason)
        {
            reason = "";
            if (_ui == null)
            {
                reason = "INPUT NOT READY";
                return false;
            }
            if (zone == null)
            {
                reason = "MOVE CLOSER";
                return false;
            }

            var moonlight = MoonlightGameManager.Instance?.moonlight;
            if (moonlight == null)
            {
                reason = "MOONLIGHT NOT READY";
                return false;
            }

            var feedback = moonlight.GetComponent<MoonlightActionFeedback>();
            if (feedback != null && !feedback.CanBeginAction)
            {
                reason = feedback.InputBlockReason;
                return false;
            }
            return true;
        }

        void Reject(string reason)
        {
            LastScore = 0f;
            LastRejectionReason = string.IsNullOrEmpty(reason) ? "INPUT BLOCKED" : reason;
            SetResultVisual(false);
            Debug.Log($"[MoonlightActivityQA] gesture-rejected reason=\"{LastRejectionReason}\"");
        }

        void SetTrackingVisual()
        {
            _feedbackUntil = 0f;
            transform.localScale = _baseScale * 1.035f;
            if (_surface != null)
                _surface.color = Color.Lerp(_baseColor, Color.white, 0.18f);
        }

        void RestoreTrackingVisual()
        {
            transform.localScale = _baseScale;
            if (_surface != null && Time.unscaledTime >= _feedbackUntil)
                _surface.color = _baseColor;
        }

        void SetResultVisual(bool passed)
        {
            _feedbackColor = passed
                ? new Color(0.42f, 1f, 0.72f, _baseColor.a)
                : new Color(1f, 0.38f, 0.42f, _baseColor.a);
            _feedbackUntil = Time.unscaledTime + 0.22f;
            if (_surface != null)
                _surface.color = Color.Lerp(_baseColor, _feedbackColor, 0.55f);
        }

        public static float ScoreGesture(MoonlightGestureKind gesture,
            IReadOnlyList<Vector2> points, float duration)
        {
            if (points == null || points.Count == 0) return 0f;
            float path = 0f;
            for (int i = 1; i < points.Count; i++)
                path += Vector2.Distance(points[i - 1], points[i]);
            float displacement = Vector2.Distance(points[0], points[^1]);

            switch (gesture)
            {
                case MoonlightGestureKind.Tap:
                    return Mathf.Clamp01((0.42f - duration) / 0.22f) *
                           Mathf.Clamp01((0.16f - path) / 0.10f);

                case MoonlightGestureKind.Hold:
                    return Mathf.Clamp01((duration - 0.45f) / 0.55f) *
                           Mathf.Clamp01((0.20f - path) / 0.12f);

                case MoonlightGestureKind.Swipe:
                    float speed = displacement / Mathf.Max(0.08f, duration);
                    return Mathf.Clamp01((displacement - 0.28f) / 0.42f) *
                           Mathf.Clamp01(speed / 1.5f);

                case MoonlightGestureKind.ZigZag:
                    int turns = 0;
                    float previousSign = 0f;
                    for (int i = 1; i < points.Count; i++)
                    {
                        float dx = points[i].x - points[i - 1].x;
                        if (Mathf.Abs(dx) < 0.035f) continue;
                        float sign = Mathf.Sign(dx);
                        if (previousSign != 0f && sign != previousSign) turns++;
                        previousSign = sign;
                    }
                    return Mathf.Clamp01(turns / 3f) * Mathf.Clamp01(path / 1.15f);

                case MoonlightGestureKind.Circle:
                    if (points.Count < 7) return 0f;
                    Vector2 center = Vector2.zero;
                    foreach (var point in points) center += point;
                    center /= points.Count;
                    float angle = 0f;
                    float radius = 0f;
                    for (int i = 1; i < points.Count; i++)
                    {
                        var a = points[i - 1] - center;
                        var b = points[i] - center;
                        if (a.sqrMagnitude < 0.0004f || b.sqrMagnitude < 0.0004f) continue;
                        angle += Mathf.Abs(Vector2.SignedAngle(a, b));
                        radius += b.magnitude;
                    }
                    radius /= Mathf.Max(1, points.Count - 1);
                    float closure = 1f - Mathf.Clamp01(displacement / 0.28f);
                    return Mathf.Clamp01((angle - 170f) / 230f) *
                           Mathf.Clamp01(radius / 0.18f) * closure;
            }

            return 0f;
        }
    }
}
