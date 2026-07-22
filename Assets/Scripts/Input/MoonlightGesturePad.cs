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

    public readonly struct MoonlightGestureSample
    {
        public const int ResampledPointCount = 7;

        readonly Vector2 _point0;
        readonly Vector2 _point1;
        readonly Vector2 _point2;
        readonly Vector2 _point3;
        readonly Vector2 _point4;
        readonly Vector2 _point5;
        readonly Vector2 _point6;

        public float Score { get; }
        public float Duration { get; }
        public bool IsInitialized { get; }
        public Vector2 Displacement => _point6 - _point0;
        public float DisplacementMagnitude => Displacement.magnitude;
        public int PointCount => IsInitialized ? ResampledPointCount : 0;
        public Vector2 Start => _point0;
        public Vector2 End => _point6;
        public Vector2 Direction => Displacement.sqrMagnitude > 0.000001f
            ? Displacement.normalized
            : Vector2.right;

        public Vector2 this[int index] => index switch
        {
            0 => _point0,
            1 => _point1,
            2 => _point2,
            3 => _point3,
            4 => _point4,
            5 => _point5,
            6 => _point6,
            _ => throw new System.IndexOutOfRangeException()
        };

        MoonlightGestureSample(float score, float duration, Vector2 point0, Vector2 point1,
            Vector2 point2, Vector2 point3, Vector2 point4, Vector2 point5, Vector2 point6)
        {
            IsInitialized = true;
            Score = Mathf.Clamp01(IsFinite(score) ? score : 0f);
            Duration = Mathf.Clamp(IsFinite(duration) ? duration : 0f, 0f, 8f);
            _point0 = Sanitize(point0);
            _point1 = Sanitize(point1);
            _point2 = Sanitize(point2);
            _point3 = Sanitize(point3);
            _point4 = Sanitize(point4);
            _point5 = Sanitize(point5);
            _point6 = Sanitize(point6);
        }

        public static MoonlightGestureSample Create(float score, float duration,
            IReadOnlyList<Vector2> normalizedPoints)
        {
            var points = new Vector2[ResampledPointCount];
            if (normalizedPoints == null || normalizedPoints.Count == 0)
                return new MoonlightGestureSample(score, duration, Vector2.zero, Vector2.zero,
                    Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);

            float pathLength = 0f;
            for (int i = 1; i < normalizedPoints.Count; i++)
                pathLength += Vector2.Distance(Sanitize(normalizedPoints[i - 1]),
                    Sanitize(normalizedPoints[i]));

            if (pathLength <= 0.000001f)
            {
                Vector2 point = Sanitize(normalizedPoints[0]);
                for (int i = 0; i < points.Length; i++) points[i] = point;
            }
            else
            {
                points[0] = Sanitize(normalizedPoints[0]);
                points[^1] = Sanitize(normalizedPoints[^1]);
                int segment = 1;
                float traversed = 0f;
                for (int sample = 1; sample < ResampledPointCount - 1; sample++)
                {
                    float target = pathLength * sample / (ResampledPointCount - 1f);
                    while (segment < normalizedPoints.Count)
                    {
                        Vector2 from = Sanitize(normalizedPoints[segment - 1]);
                        Vector2 to = Sanitize(normalizedPoints[segment]);
                        float segmentLength = Vector2.Distance(from, to);
                        if (traversed + segmentLength >= target ||
                            segment == normalizedPoints.Count - 1)
                        {
                            float segmentT = segmentLength > 0.000001f
                                ? Mathf.Clamp01((target - traversed) / segmentLength)
                                : 0f;
                            points[sample] = Vector2.Lerp(from, to, segmentT);
                            break;
                        }
                        traversed += segmentLength;
                        segment++;
                    }
                }
            }

            return new MoonlightGestureSample(score, duration, points[0], points[1], points[2],
                points[3], points[4], points[5], points[6]);
        }

        public static MoonlightGestureSample Synthetic(MoonlightGestureKind gesture, float score)
        {
            var points = new Vector2[ResampledPointCount];
            for (int i = 0; i < points.Length; i++)
            {
                float t = i / (float)(points.Length - 1);
                points[i] = gesture switch
                {
                    MoonlightGestureKind.Swipe => new Vector2(Mathf.Lerp(-0.46f, 0.46f, t), 0f),
                    MoonlightGestureKind.ZigZag => new Vector2(
                        i % 2 == 0 ? -0.36f : 0.36f, Mathf.Lerp(-0.42f, 0.42f, t)),
                    MoonlightGestureKind.Circle => new Vector2(
                        Mathf.Cos(t * Mathf.PI * 2f), Mathf.Sin(t * Mathf.PI * 2f)) * 0.32f,
                    _ => Vector2.zero
                };
            }
            float duration = gesture switch
            {
                MoonlightGestureKind.Tap => 0.12f,
                MoonlightGestureKind.Hold => 1f,
                MoonlightGestureKind.Swipe => 0.35f,
                _ => 0.8f
            };
            return Create(score, duration, points);
        }

        public bool HasSevenFiniteNormalizedPoints
        {
            get
            {
                if (!IsInitialized) return false;
                for (int i = 0; i < ResampledPointCount; i++)
                {
                    Vector2 point = this[i];
                    if (!IsFinite(point.x) || !IsFinite(point.y) ||
                        Mathf.Abs(point.x) > 1f || Mathf.Abs(point.y) > 1f)
                        return false;
                }
                return IsFinite(Score) && IsFinite(Duration) && IsFinite(Displacement.x) &&
                    IsFinite(Displacement.y) && IsFinite(DisplacementMagnitude);
            }
        }

        public bool ContentEquals(MoonlightGestureSample other, float epsilon = 0.0001f)
        {
            if (!IsInitialized || !other.IsInitialized ||
                Mathf.Abs(Score - other.Score) > epsilon ||
                Mathf.Abs(Duration - other.Duration) > epsilon)
                return false;
            for (int i = 0; i < ResampledPointCount; i++)
                if (Vector2.Distance(this[i], other[i]) > epsilon) return false;
            return true;
        }

        static Vector2 Sanitize(Vector2 point) => new(
            Mathf.Clamp(IsFinite(point.x) ? point.x : 0f, -1f, 1f),
            Mathf.Clamp(IsFinite(point.y) ? point.y : 0f, -1f, 1f));

        static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    public sealed class MoonlightGesturePad : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler, ICancelHandler
    {
        public const int GestureTraceDotCapacity = 24;
        public const int GestureGuideDotCapacity = 12;
        const float GestureTraceFadeSeconds = 0.48f;
        const float ResultFeedbackSeconds = 0.34f;
        const float FailedResultFillScale = 0.70f;
        const float GoodResultFillScale = 0.82f;
        const float GreatResultFillScale = 0.90f;
        const float PerfectResultFillScale = 0.98f;
        const float MinimumResultFillScale = 0.65f;
        const float MaximumResultFillScale = 1f;
        const float MinimumSuccessScaleSeparation = 0.07f;
        const float ResultParentScaleMultiplier = 1f;
        const string ResultOverlayObjectName = "GestureResultOverlay";
        const int ResultOverlaySiblingIndex = 0;
        const float ResultOverlayAnchorMin = 0f;
        const float ResultOverlayAnchorMax = 1f;
        const float ResultOverlayPeakAlpha = 0.46f;
        const bool ResultOverlayRaycastTarget = false;
        public const float FreeHopTapPassingScore = 0.50f;

        readonly struct ResultFeedbackProfile
        {
            public readonly Color Color;
            public readonly float FillScale;

            public ResultFeedbackProfile(Color color, float fillScale)
            {
                Color = color;
                FillScale = fillScale;
            }
        }

        readonly List<Vector2> _points = new();
        readonly RectTransform[] _traceDots = new RectTransform[GestureTraceDotCapacity];
        readonly Image[] _traceImages = new Image[GestureTraceDotCapacity];
        readonly float[] _traceDrawScales = new float[GestureTraceDotCapacity];
        readonly RectTransform[] _guideDots = new RectTransform[GestureGuideDotCapacity];
        readonly Image[] _guideImages = new Image[GestureGuideDotCapacity];

        MoonlightUI _ui;
        RectTransform _rect;
        MoonlightGestureKind _gesture;
        float _startedAt;
        int _pointerId = int.MinValue;
        MoonlightSpatialActionZone _startedZone;
        Image _surface;
        RectTransform _resultOverlayRect;
        Image _resultOverlay;
        Color _baseColor = Color.white;
        Vector3 _baseScale = Vector3.one;
        float _feedbackUntil;
        Color _feedbackColor;
        float _feedbackFillScale = 1f;
        bool _lastResultPassed;
        int _traceDotCursor;
        int _traceDotCount;
        float _traceFadeUntil;
        Color _traceResultColor = Color.white;
        float _traceResultFillScale = 1f;
        MoonlightGestureKind _guideGesture;
        MoonlightGestureKind _appliedGuideGesture;
        bool _guideRequested;
        bool _guideVisible;
        bool _guidePresentationApplied;
        bool _liveHoldReadinessActive;
        bool _liveHoldReady;
        bool _liveHoldHapticPlayed;
        bool _liveHoldOverlayObserved;
        int _liveHoldHapticCount;
        float _liveHoldScore;
        bool _hasLastLiveHoldRuntimeEvidence;
        MoonlightSpatialActionKind _lastLiveHoldActionKind;
        int _lastLiveHoldStartStep;
        int _lastLiveHoldEndStep;
        bool _lastLiveHoldOverlayObserved;
        int _lastLiveHoldHapticCount;
        bool _lastLiveHoldCompletionAccepted;
        bool _lastLiveHoldCompletionHapticSuppressed;
        bool _lastLiveHoldCleanupObserved;
        bool _lastLiveHoldCancellationCleanupObserved;
        string _lastLiveHoldCancellationReason = "";
        bool _trackingFreeHop;
        MoonlightPlayerController _contextMovementController;

        public MoonlightGestureKind ActiveGesture => _gesture;
        public float LastScore { get; private set; }
        public MoonlightGestureSample LastSample { get; private set; }
        public string LastRejectionReason { get; private set; } = "";
        public bool IsTrackingGesture => _pointerId != int.MinValue;
        public bool IsContextGestureMovementLockHeld =>
            _contextMovementController != null &&
            _contextMovementController.IsContextGestureMovementLocked;
        public int ActivePointerIdForQA => _pointerId;
        public int MultitouchRejectionCount { get; private set; }
        public int TraceDotPoolCount => _traceDots.Length;
        public int VisibleTraceDotCount => _traceDotCount;
        public int GuideDotPoolCount => _guideDots.Length;
        public bool GuideIsVisible => _guideVisible;
        public MoonlightGestureKind GuideGesture => _guideGesture;
        public string GuidePathQAMarker => ValidateGestureGuideContract(out _)
            ? "MOONLIGHT_IPAD_GESTURE_GUIDE_READY"
            : "MOONLIGHT_IPAD_GESTURE_GUIDE_INVALID";
        public bool LastResultPassed => _lastResultPassed;
        public bool IsLiveHoldReadinessActive => _liveHoldReadinessActive;
        public bool LiveHoldIsReady => _liveHoldReady;
        public bool LiveHoldReadinessHapticPlayed => _liveHoldHapticPlayed;
        public int LiveHoldReadinessHapticCount => _liveHoldHapticCount;
        public float LiveHoldScore => _liveHoldScore;
        public bool LiveHoldReadinessOverlayVisible => _liveHoldReadinessActive &&
            _resultOverlay != null && _resultOverlay.color.a > 0f;
        public bool LiveHoldReadinessStateIsClean => IsLiveHoldReadinessStateClean(
            _liveHoldReadinessActive, _liveHoldReady, _liveHoldHapticPlayed,
            _liveHoldScore, _liveHoldHapticCount);
        public bool LastLiveHoldCancellationCleanupObserved =>
            _lastLiveHoldCancellationCleanupObserved;
        public string LastLiveHoldCancellationReason => _lastLiveHoldCancellationReason;
        public bool IsResultFeedbackActive => _feedbackUntil > Time.unscaledTime;
        public string ResultFeedbackQAMarker =>
            ValidateResultFeedbackContract(out _) && ResultOverlayIsReady
                ? "MOONLIGHT_IPAD_GESTURE_RESULT_FEEDBACK_READY"
                : "MOONLIGHT_IPAD_GESTURE_RESULT_FEEDBACK_INVALID";
        public bool ResultOverlayIsReady
        {
            get
            {
                if (_resultOverlayRect == null || _resultOverlay == null ||
                    _resultOverlay.transform != _resultOverlayRect ||
                    _resultOverlayRect.gameObject.name != ResultOverlayObjectName ||
                    _resultOverlayRect.parent != transform ||
                    !_resultOverlayRect.gameObject.activeSelf ||
                    _resultOverlayRect.GetSiblingIndex() != ResultOverlaySiblingIndex ||
                    _resultOverlayRect.anchorMin != Vector2.one * ResultOverlayAnchorMin ||
                    _resultOverlayRect.anchorMax != Vector2.one * ResultOverlayAnchorMax ||
                    _resultOverlayRect.offsetMin != Vector2.zero ||
                    _resultOverlayRect.offsetMax != Vector2.zero ||
                    _resultOverlay.raycastTarget != ResultOverlayRaycastTarget)
                    return false;

                Vector3 overlayScale = _resultOverlayRect.localScale;
                if (!ScaleComponentIsFinite(overlayScale.x) ||
                    !ScaleComponentIsFinite(overlayScale.y) ||
                    !ScaleComponentIsFinite(overlayScale.z) ||
                    overlayScale.x < MinimumResultFillScale ||
                    overlayScale.x > MaximumResultFillScale ||
                    Mathf.Abs(overlayScale.y - overlayScale.x) > 0.0001f ||
                    Mathf.Abs(overlayScale.z - overlayScale.x) > 0.0001f)
                    return false;

                for (int i = 0; i < GestureTraceDotCapacity; i++)
                    if (_traceDots[i] == null ||
                        _traceDots[i].GetSiblingIndex() <= ResultOverlaySiblingIndex)
                        return false;
                for (int i = 0; i < GestureGuideDotCapacity; i++)
                    if (_guideDots[i] == null ||
                        _guideDots[i].GetSiblingIndex() <= ResultOverlaySiblingIndex)
                        return false;
                return true;
            }
        }

        static bool ScaleComponentIsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);

        public Vector2 TouchSurfaceSize => _rect != null ? _rect.rect.size : Vector2.zero;
        public string CoordinateQAMarker =>
            ValidateCoordinateNormalization(TouchSurfaceSize, out _)
                ? "MOONLIGHT_GESTURE_COORDINATES_ISOTROPIC"
                : "MOONLIGHT_GESTURE_COORDINATES_DISTORTED";
        public bool TracePoolIsReady
        {
            get
            {
                for (int i = 0; i < GestureTraceDotCapacity; i++)
                    if (_traceDots[i] == null || _traceImages[i] == null ||
                        _traceImages[i].raycastTarget)
                        return false;
                return true;
            }
        }
        public bool GuidePoolIsReady
        {
            get
            {
                for (int i = 0; i < GestureGuideDotCapacity; i++)
                    if (_guideDots[i] == null || _guideImages[i] == null ||
                        _guideImages[i].raycastTarget)
                        return false;
                return true;
            }
        }
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
            BuildTracePool();
            BuildGuidePool();
            BuildResultOverlay();
        }

        void Update()
        {
            UpdateGuideAnimation();
            if (_pointerId != int.MinValue)
            {
                if (CurrentZone() != _startedZone)
                {
                    CancelTracking("zone-changed");
                    return;
                }
                UpdateLiveHoldReadiness();
                return;
            }

            float remaining = Mathf.Clamp01(
                (_feedbackUntil - Time.unscaledTime) / ResultFeedbackSeconds);
            if (remaining > 0f)
                SetResultOverlayStrength(remaining);
            else
                ClearResultFeedback();
            transform.localScale = _baseScale * ResultParentScaleMultiplier;
            UpdateTraceFade();
        }

        public void Bind(MoonlightUI ui) => _ui = ui;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pointerId != int.MinValue)
            {
                RejectAdditionalPointer(eventData.pointerId);
                return;
            }
            var zone = CurrentZone();
            if (!CanAcceptGesture(zone, out string reason))
            {
                Reject(reason);
                return;
            }

            _pointerId = eventData.pointerId;
            _startedZone = zone;
            _trackingFreeHop = zone == null;
            AcquireContextGestureMovementLock(zone);
            _gesture = _trackingFreeHop ? MoonlightGestureKind.Tap : zone.RequiredGesture;
            _startedAt = Time.unscaledTime;
            _points.Clear();
            ResetLiveHoldReadiness(false);
            _lastLiveHoldCancellationCleanupObserved = false;
            _lastLiveHoldCancellationReason = "";
            _liveHoldReadinessActive = ShouldUseLiveHoldReadiness(
                _ui != null && _ui.IsIPadHUDLayoutActive,
                zone != null && zone.SupportsLiveHoldReadiness);
            ClearTrace();
            LastRejectionReason = "";
            SetTrackingVisual();
            RefreshGuideVisibility();
            if (!_liveHoldReadinessActive)
                HapticFeedback.Light();
            AddPoint(eventData);
            UpdateLiveHoldReadiness();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_pointerId != eventData.pointerId) return;
            AddPoint(eventData);
            UpdateLiveHoldReadiness();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_pointerId != eventData.pointerId) return;
            AddPoint(eventData);
            UpdateLiveHoldReadiness();
            var startedZone = _startedZone;
            bool wasTrackingFreeHop = _trackingFreeHop;
            float duration = Time.unscaledTime - _startedAt;
            bool wasLiveHoldReadinessActive = _liveHoldReadinessActive;
            MoonlightSpatialActionKind liveHoldActionKind = startedZone != null
                ? startedZone.Kind
                : default;
            int liveHoldStartStep = startedZone != null ? startedZone.ProgressStep : -1;
            bool liveHoldOverlayObserved = _liveHoldOverlayObserved;
            int liveHoldHapticCount = _liveHoldHapticCount;
            bool acceptedHapticAlreadyPlayed = _liveHoldHapticPlayed;
            _pointerId = int.MinValue;
            _startedZone = null;
            _trackingFreeHop = false;
            ReleaseContextGestureMovementLock();
            ResetLiveHoldReadiness(true);
            bool liveHoldCleanupObserved = LiveHoldReadinessStateIsClean &&
                ResultOverlayIsClear;
            RestoreTrackingVisual();
            RefreshGuideVisibility();

            if (wasTrackingFreeHop)
            {
                CompleteFreeHopTap(duration);
                return;
            }

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
            LastSample = MoonlightGestureSample.Create(LastScore, duration, _points);
            Debug.Log($"[MoonlightActivityQA] gesture kind={_gesture} score={LastScore:0.00} points={_points.Count}");
            _ui?.ExecuteContextGesture(_gesture, LastSample, acceptedHapticAlreadyPlayed);
            if (wasLiveHoldReadinessActive)
            {
                _hasLastLiveHoldRuntimeEvidence = true;
                _lastLiveHoldActionKind = liveHoldActionKind;
                _lastLiveHoldStartStep = liveHoldStartStep;
                _lastLiveHoldEndStep = startedZone.ProgressStep;
                _lastLiveHoldOverlayObserved = liveHoldOverlayObserved;
                _lastLiveHoldHapticCount = liveHoldHapticCount;
                _lastLiveHoldCompletionAccepted = startedZone.LastGesturePassed;
                _lastLiveHoldCompletionHapticSuppressed =
                    startedZone.LastAcceptedHapticWasPreplayed &&
                    !startedZone.LastAcceptedGesturePlayedCompletionHaptic;
                _lastLiveHoldCleanupObserved = liveHoldCleanupObserved;
            }
            SetResultVisual(startedZone.LastGesturePassed, LastScore);
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
            LastSample = MoonlightGestureSample.Synthetic(gesture, LastScore);
            LastRejectionReason = "";
            if (zone == null)
            {
                if (gesture != MoonlightGestureKind.Tap || LastScore < FreeHopTapPassingScore)
                {
                    Reject("TAP TO HOP");
                    return false;
                }
                string freeHopReason = "INPUT NOT READY";
                if (_ui == null || !_ui.TryExecuteFreeHop(out freeHopReason))
                {
                    Reject(freeHopReason);
                    return false;
                }
                SetResultVisual(true, LastScore);
                return true;
            }
            _ui?.ExecuteContextGesture(gesture, LastSample);
            SetResultVisual(zone.LastGesturePassed, LastScore);
            return true;
        }

        public void OnCancel(BaseEventData eventData) => CancelTracking("event-cancel");

        void OnDisable() => CancelTracking("disabled");

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) CancelTracking("focus-lost");
        }

        public void SimulateApplicationFocusLossForQA() => OnApplicationFocus(false);

        public void SimulateApplicationPauseForQA() => OnApplicationPause(true);

        void OnApplicationPause(bool paused)
        {
            if (paused) CancelTracking("paused");
        }

        void CancelTracking(string reason)
        {
            bool wasTracking = _pointerId != int.MinValue;
            bool wasLiveHoldReadinessActive = _liveHoldReadinessActive;
            _pointerId = int.MinValue;
            _startedZone = null;
            _trackingFreeHop = false;
            ReleaseContextGestureMovementLock();
            _points.Clear();
            ResetLiveHoldReadiness(true);
            ClearTrace();
            ClearResultFeedback();
            RestoreTrackingVisual();
            RefreshGuideVisibility();
            if (wasLiveHoldReadinessActive)
            {
                _lastLiveHoldCancellationReason = reason;
                _lastLiveHoldCancellationCleanupObserved =
                    LiveHoldReadinessStateIsClean && ResultOverlayIsClear;
            }
            if (wasTracking)
                Debug.Log($"[MoonlightActivityQA] gesture-cancelled reason={reason}");
        }

        void AddPoint(PointerEventData eventData)
        {
            if (_rect == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rect, eventData.position, eventData.pressEventCamera, out var local)) return;

            var size = _rect.rect.size;
            local.x = Mathf.Clamp(local.x, _rect.rect.xMin + 5f, _rect.rect.xMax - 5f);
            local.y = Mathf.Clamp(local.y, _rect.rect.yMin + 5f, _rect.rect.yMax - 5f);
            Vector2 tracePosition = local;
            local = NormalizeGesturePoint(local, size);
            if (_points.Count == 0 || Vector2.Distance(_points[^1], local) > 0.015f)
            {
                _points.Add(local);
                AddTraceDot(tracePosition);
            }
        }

        MoonlightSpatialActionZone CurrentZone()
        {
            var moonlight = MoonlightGameManager.Instance?.moonlight;
            return moonlight != null
                ? moonlight.GetComponent<MoonlightSpatialInteractor>()?.CurrentZone
                : null;
        }

        void AcquireContextGestureMovementLock(MoonlightSpatialActionZone zone)
        {
            if (!ShouldLockContextGestureMovement(
                    _ui != null && _ui.IsIPadHUDLayoutActive, zone != null, true))
                return;

            var moonlight = MoonlightGameManager.Instance?.moonlight;
            _contextMovementController = moonlight != null
                ? moonlight.GetComponent<MoonlightPlayerController>()
                : null;
            _contextMovementController?.SetContextGestureMovementLocked(true);
        }

        void ReleaseContextGestureMovementLock()
        {
            _contextMovementController?.SetContextGestureMovementLocked(false);
            _contextMovementController = null;
        }

        public static bool ShouldLockContextGestureMovement(bool isIPadLayout,
            bool hasContextZone, bool isTrackingGesture) =>
            isIPadLayout && hasContextZone && isTrackingGesture;

        public static bool ValidateContextGestureMovementLockStaticContract(out string detail)
        {
            bool contextualIPad = ShouldLockContextGestureMovement(true, true, true);
            bool freeHopUnchanged = !ShouldLockContextGestureMovement(true, false, true);
            bool desktopUnchanged = !ShouldLockContextGestureMovement(false, true, true);
            bool completionReleased = !ShouldLockContextGestureMovement(true, true, false);
            bool controllerGate =
                MoonlightPlayerController.ValidateContextGestureMovementLockContract(
                    out string controllerDetail);
            detail = $"contextualIPad={contextualIPad} freeHop={freeHopUnchanged} " +
                $"desktop={desktopUnchanged} released={completionReleased} " +
                $"controller=({controllerDetail})";
            return contextualIPad && freeHopUnchanged && desktopUnchanged &&
                completionReleased && controllerGate;
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
                return _ui.CanBeginFreeHop(out reason);
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

        void CompleteFreeHopTap(float duration)
        {
            if (CurrentZone() != null)
            {
                _points.Clear();
                Reject("CONTEXT ACTION AVAILABLE");
                return;
            }
            if (!CanAcceptGesture(null, out string reason))
            {
                _points.Clear();
                Reject(reason);
                return;
            }

            LastScore = ScoreGesture(MoonlightGestureKind.Tap, _points, duration);
            LastSample = MoonlightGestureSample.Create(LastScore, duration, _points);
            _points.Clear();
            if (LastScore < FreeHopTapPassingScore)
            {
                Reject("TAP TO HOP");
                return;
            }
            if (!_ui.TryExecuteFreeHop(out reason))
            {
                Reject(reason);
                return;
            }

            LastRejectionReason = "";
            SetResultVisual(true, LastScore);
        }

        void RejectAdditionalPointer(int pointerId)
        {
            if (!ShouldRejectAdditionalPointer(_pointerId, pointerId)) return;
            MultitouchRejectionCount++;
            LastRejectionReason = "MULTITOUCH BLOCKED";
            Debug.Log($"[MoonlightActivityQA] gesture-rejected reason=\"{LastRejectionReason}\" " +
                $"activePointer={_pointerId} rejectedPointer={pointerId}");
        }

        public static bool ShouldRejectAdditionalPointer(int activePointerId, int pointerId) =>
            activePointerId != int.MinValue && activePointerId != pointerId;

        public static bool ValidateFreeHopGestureContract(out string detail)
        {
            bool secondPointerRejected = ShouldRejectAdditionalPointer(17, 18);
            bool samePointerRetained = !ShouldRejectAdditionalPointer(17, 17);
            bool idlePointerAccepted = !ShouldRejectAdditionalPointer(int.MinValue, 18);
            float goodTap = ScoreGesture(MoonlightGestureKind.Tap,
                new[] { Vector2.zero }, 0.08f);
            float drag = ScoreGesture(MoonlightGestureKind.Tap,
                new[] { Vector2.zero, Vector2.right }, 0.08f);
            bool thresholdPass = Mathf.Abs(FreeHopTapPassingScore - 0.50f) <= 0.0001f &&
                goodTap >= FreeHopTapPassingScore && drag < FreeHopTapPassingScore;
            detail = $"multitouch={secondPointerRejected} samePointer={samePointerRetained} " +
                $"idlePointer={idlePointerAccepted} threshold={FreeHopTapPassingScore:0.00} " +
                $"tap={goodTap:0.00} drag={drag:0.00}";
            return secondPointerRejected && samePointerRetained && idlePointerAccepted &&
                thresholdPass;
        }

        void Reject(string reason)
        {
            LastScore = 0f;
            LastRejectionReason = string.IsNullOrEmpty(reason) ? "INPUT BLOCKED" : reason;
            SetResultVisual(false, LastScore);
            Debug.Log($"[MoonlightActivityQA] gesture-rejected reason=\"{LastRejectionReason}\"");
        }

        void SetTrackingVisual()
        {
            ClearResultFeedback();
            transform.localScale = _baseScale * 1.035f;
            if (_surface != null)
                _surface.color = Color.Lerp(_baseColor, Color.white, 0.18f);
        }

        void UpdateLiveHoldReadiness()
        {
            if (!_liveHoldReadinessActive || _startedZone == null) return;

            _liveHoldScore = ScoreGesture(MoonlightGestureKind.Hold, _points,
                Time.unscaledTime - _startedAt);
            _liveHoldReady = _liveHoldScore >= _startedZone.PassingScore;
            SetLiveHoldReadinessOverlay(_liveHoldScore, _startedZone.PassingScore);
            if (ShouldPlayLiveHoldReadinessHaptic(_liveHoldHapticPlayed,
                    _liveHoldScore, _startedZone.PassingScore))
            {
                _liveHoldHapticPlayed = true;
                _liveHoldHapticCount++;
                HapticFeedback.Success();
            }
        }

        static bool ShouldPlayLiveHoldReadinessHaptic(bool alreadyPlayed, float score,
            float passingScore) => !alreadyPlayed && score >= passingScore;

        public static bool ShouldUseLiveHoldReadiness(bool isIPadLayout,
            bool zoneSupportsLiveHoldReadiness) =>
            isIPadLayout && zoneSupportsLiveHoldReadiness;

        void SetLiveHoldReadinessOverlay(float score, float passingScore)
        {
            if (_resultOverlay == null || _resultOverlayRect == null) return;

            float readiness = Mathf.Clamp01(score / Mathf.Max(0.0001f, passingScore));
            Color color = score < passingScore
                ? new Color(1f, 0.64f, 0.24f, 1f)
                : ResultFeedbackProfileFor(true, score).Color;
            color.a = Mathf.Lerp(0.14f, ResultOverlayPeakAlpha, readiness);
            _resultOverlay.color = color;
            _resultOverlayRect.localScale = Vector3.one * Mathf.Lerp(
                MinimumResultFillScale, MaximumResultFillScale, Mathf.Clamp01(score));
            _liveHoldOverlayObserved = true;
        }

        void ResetLiveHoldReadiness(bool clearOverlay)
        {
            _liveHoldReadinessActive = false;
            _liveHoldReady = false;
            _liveHoldHapticPlayed = false;
            _liveHoldOverlayObserved = false;
            _liveHoldHapticCount = 0;
            _liveHoldScore = 0f;
            if (clearOverlay)
                ClearResultFeedback();
        }

        bool ResultOverlayIsClear => _resultOverlay == null || _resultOverlay.color.a <= 0f;

        static bool IsLiveHoldReadinessStateClean(bool active, bool ready,
            bool hapticPlayed, float score, int hapticCount) =>
            !active && !ready && !hapticPlayed && Mathf.Approximately(score, 0f) &&
            hapticCount == 0;

        public bool ValidateLastLiveHoldReadinessRuntimeContract(
            MoonlightSpatialActionKind expectedActionKind, out string detail)
        {
            int expectedStartStep = expectedActionKind == MoonlightSpatialActionKind.Cook ? 2 : 3;
            int expectedEndStep = expectedActionKind == MoonlightSpatialActionKind.Cook ? 3 : 0;
            bool identityPass = _hasLastLiveHoldRuntimeEvidence &&
                _lastLiveHoldActionKind == expectedActionKind &&
                _lastLiveHoldStartStep == expectedStartStep &&
                _lastLiveHoldEndStep == expectedEndStep;
            bool hapticOnce = _lastLiveHoldHapticCount == 1 &&
                _lastLiveHoldCompletionHapticSuppressed;
            detail = $"action={_lastLiveHoldActionKind}/{expectedActionKind} " +
                $"step={_lastLiveHoldStartStep}->{_lastLiveHoldEndStep}/" +
                $"{expectedStartStep}->{expectedEndStep} overlay={_lastLiveHoldOverlayObserved} " +
                $"haptic={_lastLiveHoldHapticCount}/1 " +
                $"completion={_lastLiveHoldCompletionAccepted} " +
                $"completionHapticSuppressed={_lastLiveHoldCompletionHapticSuppressed} " +
                $"cleanup={_lastLiveHoldCleanupObserved}";
            return identityPass && _lastLiveHoldOverlayObserved && hapticOnce &&
                _lastLiveHoldCompletionAccepted && _lastLiveHoldCleanupObserved;
        }

        void RestoreTrackingVisual()
        {
            transform.localScale = _baseScale;
            if (_surface != null && Time.unscaledTime >= _feedbackUntil)
                _surface.color = _baseColor;
        }

        void SetResultVisual(bool passed, float score)
        {
            ResultFeedbackProfile profile = ResultFeedbackProfileFor(passed, score);
            _lastResultPassed = passed;
            _feedbackColor = profile.Color;
            _feedbackFillScale = profile.FillScale;
            _feedbackUntil = Time.unscaledTime + ResultFeedbackSeconds;
            transform.localScale = _baseScale * ResultParentScaleMultiplier;
            SetResultOverlayStrength(1f);
            BeginTraceFade(profile);
        }

        void SetResultOverlayStrength(float strength)
        {
            if (_resultOverlay == null) return;
            Color color = _feedbackColor;
            color.a = Mathf.Clamp01(strength) * ResultOverlayPeakAlpha;
            _resultOverlay.color = color;
            _resultOverlayRect.localScale = Vector3.one * _feedbackFillScale;
        }

        void ClearResultFeedback()
        {
            _feedbackUntil = 0f;
            _feedbackColor = Color.clear;
            _feedbackFillScale = 1f;
            if (_resultOverlay == null || _resultOverlayRect == null) return;
            _resultOverlay.color = Color.clear;
            _resultOverlayRect.localScale = Vector3.one;
        }

        static ResultFeedbackProfile ResultFeedbackProfileFor(bool passed, float score)
        {
            if (!passed)
                return new ResultFeedbackProfile(
                    new Color(1f, 0.38f, 0.42f, 1f), FailedResultFillScale);

            return MoonlightActionFeedback.ActionQualityTierFor(score) switch
            {
                MoonlightActionQualityTier.Great => new ResultFeedbackProfile(
                    new Color(0.34f, 0.78f, 1f, 1f), GreatResultFillScale),
                MoonlightActionQualityTier.Perfect => new ResultFeedbackProfile(
                    new Color(1f, 0.86f, 0.30f, 1f), PerfectResultFillScale),
                _ => new ResultFeedbackProfile(
                    new Color(0.42f, 1f, 0.72f, 1f), GoodResultFillScale)
            };
        }

        public void SetGestureGuide(MoonlightGestureKind gesture, bool visible)
        {
            bool geometryChanged = _guideGesture != gesture;
            _guideGesture = gesture;
            _guideRequested = visible;
            if (geometryChanged) PositionGuideDots();
            RefreshGuideVisibility();
        }

        void BuildGuidePool()
        {
            for (int i = 0; i < GestureGuideDotCapacity; i++)
            {
                var dot = new GameObject($"GestureGuideDot-{i + 1:00}");
                dot.transform.SetParent(transform, false);
                dot.transform.SetAsFirstSibling();
                var rect = dot.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(8f, 8f);
                rect.localRotation = Quaternion.Euler(0f, 0f, 45f);
                var image = dot.AddComponent<Image>();
                image.color = new Color(0.78f, 0.94f, 1f, 0.16f);
                image.raycastTarget = false;
                dot.SetActive(false);
                _guideDots[i] = rect;
                _guideImages[i] = image;
            }
            PositionGuideDots();
        }

        void PositionGuideDots()
        {
            for (int i = 0; i < GestureGuideDotCapacity; i++)
            {
                if (_guideDots[i] == null) continue;
                float t = i / (float)(GestureGuideDotCapacity - 1);
                _guideDots[i].anchoredPosition = EvaluateGestureGuidePoint(_guideGesture, t);
            }
        }

        void RefreshGuideVisibility()
        {
            bool show = _guideRequested && _pointerId == int.MinValue;
            if (_guidePresentationApplied && _guideVisible == show &&
                _appliedGuideGesture == _guideGesture)
                return;

            _guideVisible = show;
            _appliedGuideGesture = _guideGesture;
            _guidePresentationApplied = true;
            for (int i = 0; i < GestureGuideDotCapacity; i++)
                if (_guideDots[i] != null)
                    _guideDots[i].gameObject.SetActive(show &&
                        (_guideGesture != MoonlightGestureKind.Hold || i == 0));
        }

        void UpdateGuideAnimation()
        {
            if (!_guideVisible) return;
            float traveling = Mathf.Repeat(Time.unscaledTime * 0.72f, 1f);
            Color baseColor = GuideColor(_guideGesture);
            for (int i = 0; i < GestureGuideDotCapacity; i++)
            {
                if (_guideDots[i] == null || _guideImages[i] == null) continue;
                if (_guideGesture == MoonlightGestureKind.Hold)
                {
                    if (i != 0) continue;
                    float pulse = Mathf.Sin(Time.unscaledTime * Mathf.PI * 2.4f) * 0.5f + 0.5f;
                    Color holdColor = baseColor;
                    holdColor.a = Mathf.Lerp(0.26f, 0.72f, pulse);
                    _guideImages[i].color = holdColor;
                    _guideDots[i].localScale = Vector3.one * Mathf.Lerp(0.88f, 1.64f, pulse);
                    continue;
                }
                float dotT = i / (float)(GestureGuideDotCapacity - 1);
                float distance = Mathf.Abs(Mathf.DeltaAngle(dotT * 360f, traveling * 360f)) / 180f;
                float glow = 1f - Mathf.SmoothStep(0f, 0.34f, distance);
                Color color = baseColor;
                color.a = Mathf.Lerp(0.10f, 0.58f, glow);
                _guideImages[i].color = color;
                _guideDots[i].localScale = Vector3.one * Mathf.Lerp(0.72f, 1.42f, glow);
            }
        }

        static Color GuideColor(MoonlightGestureKind gesture) => gesture switch
        {
            MoonlightGestureKind.Circle => new Color(0.48f, 0.92f, 1f, 1f),
            MoonlightGestureKind.Hold => new Color(1f, 0.78f, 0.42f, 1f),
            MoonlightGestureKind.Swipe => new Color(0.62f, 1f, 0.76f, 1f),
            MoonlightGestureKind.ZigZag => new Color(0.94f, 0.62f, 1f, 1f),
            _ => new Color(0.78f, 0.94f, 1f, 1f)
        };

        public static Vector2 EvaluateGestureGuidePoint(MoonlightGestureKind gesture, float time01)
        {
            float t = Mathf.Clamp01(time01);
            return gesture switch
            {
                MoonlightGestureKind.Circle => new Vector2(
                    Mathf.Cos(t * Mathf.PI * 2f) * 42f,
                    Mathf.Sin(t * Mathf.PI * 2f) * 26f),
                MoonlightGestureKind.Hold => Vector2.zero,
                MoonlightGestureKind.Swipe => new Vector2(Mathf.Lerp(-46f, 46f, t), 0f),
                MoonlightGestureKind.ZigZag => new Vector2(
                    EvaluateZigZagGuideX(t),
                    Mathf.Lerp(-22f, 22f, t)),
                _ => new Vector2(
                    Mathf.Sin(t * Mathf.PI * 4f),
                    Mathf.Cos(t * Mathf.PI * 4f)) * Mathf.Lerp(22f, 0f, t)
            };
        }

        static float EvaluateZigZagGuideX(float t)
        {
            const float firstTurn = 3f / 11f;
            const float secondTurn = 6f / 11f;
            const float thirdTurn = 9f / 11f;
            if (t <= firstTurn)
                return Mathf.Lerp(-46f, 46f, t / firstTurn);
            if (t <= secondTurn)
                return Mathf.Lerp(46f, -46f,
                    (t - firstTurn) / (secondTurn - firstTurn));
            if (t <= thirdTurn)
                return Mathf.Lerp(-46f, 46f,
                    (t - secondTurn) / (thirdTurn - secondTurn));
            return Mathf.Lerp(46f, -46f,
                (t - thirdTurn) / (1f - thirdTurn));
        }

        public static bool ValidateGestureGuideContract(out string detail)
        {
            Vector2 circleStart = EvaluateGestureGuidePoint(MoonlightGestureKind.Circle, 0f);
            Vector2 circleQuarter = EvaluateGestureGuidePoint(MoonlightGestureKind.Circle, 0.25f);
            Vector2 circleEnd = EvaluateGestureGuidePoint(MoonlightGestureKind.Circle, 1f);
            Vector2 swipeStart = EvaluateGestureGuidePoint(MoonlightGestureKind.Swipe, 0f);
            Vector2 swipeEnd = EvaluateGestureGuidePoint(MoonlightGestureKind.Swipe, 1f);
            Vector2 zigStart = EvaluateGestureGuidePoint(MoonlightGestureKind.ZigZag, 0f);
            Vector2 zigEnd = EvaluateGestureGuidePoint(MoonlightGestureKind.ZigZag, 1f);
            Vector2 tapEnd = EvaluateGestureGuidePoint(MoonlightGestureKind.Tap, 1f);
            bool circleClosed = Vector2.Distance(circleStart, circleEnd) <= 0.01f &&
                                Mathf.Abs(circleQuarter.y) >= 25f;
            bool swipeClear = swipeEnd.x - swipeStart.x >= 90f;
            bool tapConverges = tapEnd.magnitude <= 0.01f;

            Vector2 surface = new Vector2(280f, 100f);
            var normalizedZigZag = new Vector2[GestureGuideDotCapacity];
            var normalizedHold = new Vector2[GestureGuideDotCapacity];
            var normalizedLegacyHold = new Vector2[GestureGuideDotCapacity];
            int zigZagTurns = 0;
            float previousXDirection = 0f;
            float minimumX = float.PositiveInfinity;
            float maximumX = float.NegativeInfinity;
            float minimumY = float.PositiveInfinity;
            float maximumY = float.NegativeInfinity;
            for (int i = 0; i < GestureGuideDotCapacity; i++)
            {
                float t = i / (float)(GestureGuideDotCapacity - 1);
                Vector2 guidePoint = EvaluateGestureGuidePoint(MoonlightGestureKind.ZigZag, t);
                minimumX = Mathf.Min(minimumX, guidePoint.x);
                maximumX = Mathf.Max(maximumX, guidePoint.x);
                minimumY = Mathf.Min(minimumY, guidePoint.y);
                maximumY = Mathf.Max(maximumY, guidePoint.y);
                normalizedZigZag[i] = NormalizeGesturePoint(guidePoint, surface);
                normalizedHold[i] = NormalizeGesturePoint(
                    EvaluateGestureGuidePoint(MoonlightGestureKind.Hold, t), surface);
                normalizedLegacyHold[i] = NormalizeGesturePoint(new Vector2(
                    Mathf.Cos(t * Mathf.PI * 2f) * 14f,
                    Mathf.Sin(t * Mathf.PI * 2f) * 14f), surface);
                if (i == 0) continue;

                float dx = normalizedZigZag[i].x - normalizedZigZag[i - 1].x;
                if (Mathf.Abs(dx) < 0.035f) continue;
                float direction = Mathf.Sign(dx);
                if (previousXDirection != 0f && direction != previousXDirection)
                    zigZagTurns++;
                previousXDirection = direction;
            }
            float zigZagScore = ScoreGesture(MoonlightGestureKind.ZigZag,
                normalizedZigZag, 0.8f);
            float legacyHoldScore = ScoreGesture(MoonlightGestureKind.Hold,
                normalizedLegacyHold, 1f);
            float holdEarlyScore = ScoreGesture(MoonlightGestureKind.Hold,
                normalizedHold, 0.45f);
            float holdGuideScore = ScoreGesture(MoonlightGestureKind.Hold,
                normalizedHold, 1f);
            float zigZagWidth = maximumX - minimumX;
            float zigZagHeight = maximumY - minimumY;
            float zigZagCenterX = (minimumX + maximumX) * 0.5f;
            bool zigZagClear = Mathf.Abs(zigZagWidth - 92f) <= 0.01f &&
                Mathf.Abs(zigZagHeight - 44f) <= 0.01f &&
                Mathf.Abs(zigZagCenterX) <= 0.01f &&
                zigStart.x <= -45.99f && zigEnd.x <= -45.99f;
            detail = $"dots={GestureGuideDotCapacity} circleClosed={circleClosed} " +
                     $"swipeSpan={swipeEnd.x - swipeStart.x:0} " +
                     $"zigScore={zigZagScore:0.00} zigTurns={zigZagTurns} " +
                     $"zigBounds={zigZagWidth:0.0}x{zigZagHeight:0.0} " +
                     $"zigCenterX={zigZagCenterX:0.00} " +
                     $"tapEnd={tapEnd.magnitude:0.0} " +
                     $"holdLegacy={legacyHoldScore:0.00} holdEarly={holdEarlyScore:0.00} " +
                     $"holdGuide={holdGuideScore:0.00}";
            return GestureGuideDotCapacity >= 10 && circleClosed && swipeClear &&
                   zigZagClear && zigZagScore >= 0.70f && zigZagTurns == 3 &&
                   tapConverges && legacyHoldScore <= 0.001f &&
                   holdEarlyScore <= 0.001f && holdGuideScore >= 0.70f;
        }

        public static bool ValidateResultFeedbackContract(out string detail)
        {
            float greatThreshold = MoonlightActionFeedback.GreatActionQualityScore;
            float perfectThreshold = MoonlightActionFeedback.PerfectActionQualityScore;
            ResultFeedbackProfile failed = ResultFeedbackProfileFor(false, 1f);
            ResultFeedbackProfile failedLowScore = ResultFeedbackProfileFor(false, 0f);
            ResultFeedbackProfile good = ResultFeedbackProfileFor(true, 0f);
            ResultFeedbackProfile goodBelowGreat = ResultFeedbackProfileFor(
                true, greatThreshold - 0.0001f);
            ResultFeedbackProfile great = ResultFeedbackProfileFor(true, greatThreshold);
            ResultFeedbackProfile greatBelowPerfect = ResultFeedbackProfileFor(
                true, perfectThreshold - 0.0001f);
            ResultFeedbackProfile perfect = ResultFeedbackProfileFor(true, perfectThreshold);
            ResultFeedbackProfile[] profiles = { failed, good, great, perfect };

            bool exactThresholds = greatThreshold == 0.72f && perfectThreshold == 0.88f &&
                ProfilesMatch(good, goodBelowGreat) &&
                ProfilesMatch(great, greatBelowPerfect) &&
                MoonlightActionFeedback.ActionQualityTierFor(greatThreshold - 0.0001f) ==
                    MoonlightActionQualityTier.Good &&
                MoonlightActionFeedback.ActionQualityTierFor(greatThreshold) ==
                    MoonlightActionQualityTier.Great &&
                MoonlightActionFeedback.ActionQualityTierFor(perfectThreshold - 0.0001f) ==
                    MoonlightActionQualityTier.Great &&
                MoonlightActionFeedback.ActionQualityTierFor(perfectThreshold) ==
                    MoonlightActionQualityTier.Perfect;
            bool failureOverridesScore = ProfilesMatch(failed, failedLowScore) &&
                !ProfilesMatch(failed, perfect);
            bool fourDistinctProfiles = ProfilesArePairwiseDistinct(profiles, true) &&
                ProfilesArePairwiseDistinct(profiles, false);
            bool monotonicScale = failed.FillScale < good.FillScale &&
                good.FillScale < great.FillScale &&
                great.FillScale < perfect.FillScale && perfect.FillScale <= 1f;
            float minimumSuccessScaleDelta = Mathf.Min(
                great.FillScale - good.FillScale, perfect.FillScale - great.FillScale);
            bool materialScaleSeparation =
                minimumSuccessScaleDelta >= MinimumSuccessScaleSeparation;
            bool noParentLayoutExpansion = ResultParentScaleMultiplier == 1f &&
                MaximumResultFillScale <= 1f;
            bool overlayContract = ResultOverlayObjectName == "GestureResultOverlay" &&
                ResultOverlaySiblingIndex == 0 &&
                ResultOverlayAnchorMin == 0f && ResultOverlayAnchorMax == 1f &&
                ResultOverlayPeakAlpha >= 0.35f && ResultOverlayPeakAlpha <= 0.60f &&
                !ResultOverlayRaycastTarget;
            bool bounded = true;
            for (int i = 0; i < profiles.Length; i++)
                bounded &= ProfileIsBounded(profiles[i]);

            detail = $"profiles={profiles.Length} thresholds={greatThreshold:0.00}/{perfectThreshold:0.00} " +
                     $"fillScales={failed.FillScale:0.000}/{good.FillScale:0.000}/" +
                     $"{great.FillScale:0.000}/{perfect.FillScale:0.000} " +
                     $"minSuccessDelta={minimumSuccessScaleDelta:0.000} " +
                     $"parentScale={ResultParentScaleMultiplier:0.00} " +
                     $"distinct={fourDistinctProfiles} bounded={bounded} " +
                     $"overlay={overlayContract} override={failureOverridesScore}";
            return ResultFeedbackSeconds >= 0.32f &&
                   GestureTraceFadeSeconds > ResultFeedbackSeconds &&
                   exactThresholds && failureOverridesScore && fourDistinctProfiles &&
                   monotonicScale && materialScaleSeparation && noParentLayoutExpansion &&
                   bounded && overlayContract;
        }

        static bool ProfilesMatch(ResultFeedbackProfile first, ResultFeedbackProfile second)
        {
            return Mathf.Abs(first.FillScale - second.FillScale) <= 0.0001f &&
                   ColorDistance(first.Color, second.Color) <= 0.0001f;
        }

        static bool ProfilesArePairwiseDistinct(ResultFeedbackProfile[] profiles, bool compareColor)
        {
            for (int first = 0; first < profiles.Length - 1; first++)
                for (int second = first + 1; second < profiles.Length; second++)
                {
                    float difference = compareColor
                        ? ColorDistance(profiles[first].Color, profiles[second].Color)
                        : Mathf.Abs(profiles[first].FillScale - profiles[second].FillScale);
                    if (difference <= 0.0001f) return false;
                }
            return true;
        }

        static bool ProfileIsBounded(ResultFeedbackProfile profile)
        {
            Color color = profile.Color;
            return profile.FillScale >= MinimumResultFillScale &&
                   profile.FillScale <= MaximumResultFillScale &&
                   color.r >= 0f && color.r <= 1f &&
                   color.g >= 0f && color.g <= 1f &&
                   color.b >= 0f && color.b <= 1f &&
                   color.a >= 0f && color.a <= 1f;
        }

        static float ColorDistance(Color first, Color second)
        {
            return Mathf.Sqrt(
                Mathf.Pow(first.r - second.r, 2f) +
                Mathf.Pow(first.g - second.g, 2f) +
                Mathf.Pow(first.b - second.b, 2f));
        }

        void BuildResultOverlay()
        {
            var overlay = new GameObject(ResultOverlayObjectName);
            overlay.transform.SetParent(transform, false);
            _resultOverlayRect = overlay.AddComponent<RectTransform>();
            _resultOverlayRect.anchorMin = Vector2.one * ResultOverlayAnchorMin;
            _resultOverlayRect.anchorMax = Vector2.one * ResultOverlayAnchorMax;
            _resultOverlayRect.offsetMin = Vector2.zero;
            _resultOverlayRect.offsetMax = Vector2.zero;
            _resultOverlayRect.SetSiblingIndex(ResultOverlaySiblingIndex);
            _resultOverlay = overlay.AddComponent<Image>();
            _resultOverlay.color = Color.clear;
            _resultOverlay.raycastTarget = ResultOverlayRaycastTarget;
            _resultOverlayRect.localScale = Vector3.one;
        }

        void BuildTracePool()
        {
            for (int i = 0; i < GestureTraceDotCapacity; i++)
            {
                var dot = new GameObject($"GestureTraceDot-{i + 1:00}");
                dot.transform.SetParent(transform, false);
                dot.transform.SetAsFirstSibling();
                var rect = dot.AddComponent<RectTransform>();
                Vector2 traceAnchor = _rect != null ? _rect.pivot : new Vector2(0.5f, 0.5f);
                rect.anchorMin = traceAnchor;
                rect.anchorMax = traceAnchor;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(10f, 10f);
                rect.localRotation = Quaternion.Euler(0f, 0f, 45f);
                var image = dot.AddComponent<Image>();
                image.color = new Color(0.72f, 0.94f, 1f, 0.72f);
                image.raycastTarget = false;
                dot.SetActive(false);
                _traceDots[i] = rect;
                _traceImages[i] = image;
            }
        }

        void AddTraceDot(Vector2 localPosition)
        {
            int index = _traceDotCursor;
            _traceDotCursor = (_traceDotCursor + 1) % GestureTraceDotCapacity;
            _traceDotCount = Mathf.Min(_traceDotCount + 1, GestureTraceDotCapacity);
            var rect = _traceDots[index];
            var image = _traceImages[index];
            if (rect == null || image == null) return;
            rect.anchoredPosition = localPosition;
            _traceDrawScales[index] = Mathf.Lerp(0.78f, 1.12f,
                _traceDotCount / (float)GestureTraceDotCapacity);
            rect.localScale = Vector3.one * _traceDrawScales[index];
            image.color = new Color(0.72f, 0.94f, 1f, 0.72f);
            rect.gameObject.SetActive(true);
        }

        void BeginTraceFade(ResultFeedbackProfile profile)
        {
            _traceResultColor = profile.Color;
            _traceResultColor.a = 0.88f;
            _traceResultFillScale = profile.FillScale;
            _traceFadeUntil = Time.unscaledTime + GestureTraceFadeSeconds;
            for (int i = 0; i < GestureTraceDotCapacity; i++)
                if (_traceImages[i] != null && _traceDots[i].gameObject.activeSelf)
                {
                    _traceImages[i].color = _traceResultColor;
                    _traceDots[i].localScale = Vector3.one *
                        (_traceDrawScales[i] * _traceResultFillScale);
                }
        }

        void UpdateTraceFade()
        {
            if (_traceDotCount == 0 || _traceFadeUntil <= 0f) return;
            float remaining = Mathf.Clamp01(
                (_traceFadeUntil - Time.unscaledTime) / GestureTraceFadeSeconds);
            if (remaining <= 0f)
            {
                ClearTrace();
                return;
            }

            for (int i = 0; i < GestureTraceDotCapacity; i++)
            {
                if (_traceImages[i] == null || !_traceDots[i].gameObject.activeSelf) continue;
                Color color = _traceResultColor;
                color.a *= remaining;
                _traceImages[i].color = color;
            }
        }

        void ClearTrace()
        {
            _traceDotCursor = 0;
            _traceDotCount = 0;
            _traceFadeUntil = 0f;
            _traceResultColor = Color.white;
            _traceResultFillScale = 1f;
            for (int i = 0; i < GestureTraceDotCapacity; i++)
                if (_traceDots[i] != null)
                    _traceDots[i].gameObject.SetActive(false);
        }

        public static Vector2 NormalizeGesturePoint(Vector2 localPoint, Vector2 surfaceSize)
        {
            float referenceSize = Mathf.Max(1f,
                Mathf.Min(Mathf.Abs(surfaceSize.x), Mathf.Abs(surfaceSize.y)));
            return localPoint / referenceSize;
        }

        public static bool ValidateCoordinateNormalization(Vector2 surfaceSize,
                                                           out string detail)
        {
            float sampleRadius = Mathf.Max(1f,
                Mathf.Min(Mathf.Abs(surfaceSize.x), Mathf.Abs(surfaceSize.y)) * 0.35f);
            Vector2 horizontal = NormalizeGesturePoint(
                new Vector2(sampleRadius, 0f), surfaceSize);
            Vector2 vertical = NormalizeGesturePoint(
                new Vector2(0f, sampleRadius), surfaceSize);
            float ratio = vertical.magnitude > 0.0001f
                ? horizontal.magnitude / vertical.magnitude
                : 0f;
            detail = $"surface={surfaceSize.x:0}x{surfaceSize.y:0} " +
                $"horizontal={horizontal.magnitude:0.000} vertical={vertical.magnitude:0.000} " +
                $"ratio={ratio:0.000}";
            return surfaceSize.x >= 1f && surfaceSize.y >= 1f &&
                Mathf.Abs(horizontal.magnitude - vertical.magnitude) <= 0.001f &&
                Mathf.Abs(ratio - 1f) <= 0.001f;
        }

        public static bool ValidateIPadCoordinateContract(out string detail)
        {
            Vector2 surface = new Vector2(280f, 100f);
            bool isotropic = ValidateCoordinateNormalization(surface, out string coordinateDetail);
            var pixelZigZag = new[]
            {
                new Vector2(-35f, -35f), new Vector2(35f, -18f),
                new Vector2(-35f, 0f), new Vector2(35f, 18f),
                new Vector2(-35f, 35f)
            };
            var normalizedZigZag = new Vector2[pixelZigZag.Length];
            for (int i = 0; i < pixelZigZag.Length; i++)
                normalizedZigZag[i] = NormalizeGesturePoint(pixelZigZag[i], surface);
            float zigZagScore = ScoreGesture(MoonlightGestureKind.ZigZag,
                normalizedZigZag, 0.8f);
            detail = $"{coordinateDetail} pixelZigZag={zigZagScore:0.00}";
            return isotropic && zigZagScore >= 0.70f;
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
                    float minX = points[0].x;
                    float maxX = points[0].x;
                    for (int i = 1; i < points.Count; i++)
                    {
                        minX = Mathf.Min(minX, points[i].x);
                        maxX = Mathf.Max(maxX, points[i].x);
                        float dx = points[i].x - points[i - 1].x;
                        if (Mathf.Abs(dx) < 0.035f) continue;
                        float sign = Mathf.Sign(dx);
                        if (previousSign != 0f && sign != previousSign) turns++;
                        previousSign = sign;
                    }
                    float horizontalCoverage = Mathf.Clamp01(((maxX - minX) - 0.16f) / 0.32f);
                    return Mathf.Clamp01(turns / 3f) * Mathf.Clamp01(path / 1.15f) *
                           horizontalCoverage;

                case MoonlightGestureKind.Circle:
                    if (points.Count < 7) return 0f;
                    Vector2 center = Vector2.zero;
                    foreach (var point in points) center += point;
                    center /= points.Count;
                    float signedAngle = 0f;
                    float absoluteAngle = 0f;
                    float radius = 0f;
                    float twiceArea = 0f;
                    for (int i = 1; i < points.Count; i++)
                    {
                        var a = points[i - 1] - center;
                        var b = points[i] - center;
                        if (a.sqrMagnitude < 0.0004f || b.sqrMagnitude < 0.0004f) continue;
                        float segmentAngle = Vector2.SignedAngle(a, b);
                        signedAngle += segmentAngle;
                        absoluteAngle += Mathf.Abs(segmentAngle);
                        twiceArea += a.x * b.y - b.x * a.y;
                        radius += b.magnitude;
                    }
                    radius /= Mathf.Max(1, points.Count - 1);
                    float closure = 1f - Mathf.Clamp01(displacement / 0.28f);
                    float coverage = Mathf.Clamp01((Mathf.Abs(signedAngle) - 190f) / 140f);
                    float directionConsistency = Mathf.Clamp01(
                        (Mathf.Abs(signedAngle) / Mathf.Max(absoluteAngle, 1f) - 0.45f) / 0.45f);
                    float enclosedArea = Mathf.Abs(twiceArea) * 0.5f;
                    float areaCoverage = Mathf.Clamp01(enclosedArea /
                        Mathf.Max(Mathf.PI * radius * radius * 0.55f, 0.0001f));
                    return coverage * Mathf.Clamp01(radius / 0.18f) * closure *
                           Mathf.Sqrt(directionConsistency * areaCoverage);
            }

            return 0f;
        }

        public static bool ValidateGestureSampleContract(out string detail)
        {
            var source = new[]
            {
                new Vector2(-0.72f, -0.20f), new Vector2(-0.20f, 0.34f),
                new Vector2(0.16f, -0.38f), new Vector2(0.72f, 0.22f)
            };
            MoonlightGestureSample sample = MoonlightGestureSample.Create(0.84f, 0.62f, source);
            bool endpointsPreserved = Vector2.Distance(sample.Start, source[0]) <= 0.0001f &&
                Vector2.Distance(sample.End, source[^1]) <= 0.0001f;
            bool immutableShape = typeof(MoonlightGestureSample).IsValueType &&
                typeof(MoonlightGestureSample).IsSealed;
            MoonlightGestureSample empty = MoonlightGestureSample.Create(0.5f, 0.4f, null);
            MoonlightGestureSample single = MoonlightGestureSample.Create(0.5f, 0.4f,
                new[] { new Vector2(0.25f, -0.25f) });
            MoonlightGestureSample repeated = MoonlightGestureSample.Create(0.5f, 0.4f,
                new[] { Vector2.zero, Vector2.zero, new Vector2(0.6f, 0f) });
            MoonlightGestureSample sanitized = MoonlightGestureSample.Create(float.NaN,
                float.PositiveInfinity, new[]
                {
                    new Vector2(float.NaN, float.NegativeInfinity),
                    new Vector2(2f, -2f)
                });
            bool singleExpanded = single.PointCount == MoonlightGestureSample.ResampledPointCount;
            bool repeatedResampled = repeated.PointCount ==
                MoonlightGestureSample.ResampledPointCount;
            for (int i = 0; i < MoonlightGestureSample.ResampledPointCount; i++)
            {
                singleExpanded &= Vector2.Distance(single[i], new Vector2(0.25f, -0.25f)) <=
                    0.0001f;
                repeatedResampled &= Vector2.Distance(repeated[i],
                    new Vector2(i * 0.1f, 0f)) <= 0.0001f;
            }
            bool edgeCases = empty.IsInitialized && empty.PointCount == 7 &&
                empty.HasSevenFiniteNormalizedPoints &&
                singleExpanded && repeatedResampled && repeated.HasSevenFiniteNormalizedPoints &&
                repeated.DisplacementMagnitude >= 0.599f &&
                sanitized.HasSevenFiniteNormalizedPoints && sanitized.Score == 0f &&
                sanitized.Duration == 0f && sanitized.End == new Vector2(1f, -1f) &&
                !default(MoonlightGestureSample).IsInitialized &&
                default(MoonlightGestureSample).PointCount == 0;
            detail = $"points={sample.PointCount}/{MoonlightGestureSample.ResampledPointCount} " +
                $"initialized={sample.IsInitialized} " +
                $"finiteNormalized={sample.HasSevenFiniteNormalizedPoints} " +
                $"score={sample.Score:0.00} duration={sample.Duration:0.00} " +
                $"displacement={sample.Displacement:F3}/{sample.DisplacementMagnitude:0.000} " +
                $"endpoints={endpointsPreserved} edges={edgeCases} " +
                $"valueType={immutableShape}";
            return sample.IsInitialized && sample.PointCount == 7 &&
                sample.HasSevenFiniteNormalizedPoints &&
                Mathf.Approximately(sample.Score, 0.84f) &&
                Mathf.Approximately(sample.Duration, 0.62f) && endpointsPreserved && edgeCases &&
                immutableShape;
        }

        public static bool ValidateRecognizerContract(out string detail)
        {
            var tap = new[] { Vector2.zero, new Vector2(0.01f, 0f) };
            var hold = new[] { Vector2.zero, new Vector2(0.01f, 0.01f) };
            var swipe = new[] { new Vector2(-0.4f, 0f), new Vector2(0.4f, 0f) };
            var zigZag = new[]
            {
                new Vector2(-0.4f, -0.35f), new Vector2(0.35f, -0.15f),
                new Vector2(-0.35f, 0.05f), new Vector2(0.35f, 0.25f),
                new Vector2(-0.4f, 0.4f)
            };
            var circle = new List<Vector2>();
            for (int i = 0; i <= 16; i++)
            {
                float angle = i * Mathf.PI * 2f / 16f;
                circle.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.3f);
            }

            float tapScore = ScoreGesture(MoonlightGestureKind.Tap, tap, 0.12f);
            float holdScore = ScoreGesture(MoonlightGestureKind.Hold, hold, 1f);
            float swipeScore = ScoreGesture(MoonlightGestureKind.Swipe, swipe, 0.35f);
            float zigZagScore = ScoreGesture(MoonlightGestureKind.ZigZag, zigZag, 0.8f);
            float circleScore = ScoreGesture(MoonlightGestureKind.Circle, circle, 0.9f);

            var shortSwipe = new[] { new Vector2(-0.05f, 0f), new Vector2(0.05f, 0f) };
            var tinyZigZag = new[]
            {
                new Vector2(-0.05f, -0.35f), new Vector2(0.05f, -0.15f),
                new Vector2(-0.05f, 0.05f), new Vector2(0.05f, 0.25f),
                new Vector2(-0.05f, 0.4f)
            };
            var lineCircle = new[]
            {
                new Vector2(-0.3f, 0f), new Vector2(0.3f, 0f),
                new Vector2(-0.3f, 0f), new Vector2(0.3f, 0f),
                new Vector2(-0.3f, 0f), new Vector2(0.3f, 0f),
                new Vector2(-0.3f, 0f)
            };
            float shortSwipeScore = ScoreGesture(MoonlightGestureKind.Swipe, shortSwipe, 0.5f);
            float tinyZigZagScore = ScoreGesture(MoonlightGestureKind.ZigZag, tinyZigZag, 0.8f);
            float lineCircleScore = ScoreGesture(MoonlightGestureKind.Circle, lineCircle, 0.9f);

            bool validPass = tapScore >= 0.70f && holdScore >= 0.70f && swipeScore >= 0.70f &&
                zigZagScore >= 0.70f && circleScore >= 0.70f;
            bool invalidPass = shortSwipeScore <= 0.35f && tinyZigZagScore <= 0.35f &&
                lineCircleScore <= 0.35f;
            detail = $"valid tap={tapScore:0.00} hold={holdScore:0.00} " +
                $"swipe={swipeScore:0.00} zigzag={zigZagScore:0.00} circle={circleScore:0.00}; " +
                $"invalid shortSwipe={shortSwipeScore:0.00} tinyZigzag={tinyZigZagScore:0.00} " +
                $"lineCircle={lineCircleScore:0.00}";
            return validPass && invalidPass;
        }

        public static bool ValidateLiveHoldReadinessStaticContract(out string detail)
        {
            const float thresholdCrossingEpsilonSeconds = 0.0001f;
            float passingScore = MoonlightSpatialActionZone.DefaultPassingScore;
            float readyDuration = 0.45f + 0.55f * passingScore +
                thresholdCrossingEpsilonSeconds;
            float greatDuration = 0.45f + 0.55f *
                MoonlightActionFeedback.GreatActionQualityScore +
                thresholdCrossingEpsilonSeconds;
            float perfectDuration = 0.45f + 0.55f *
                MoonlightActionFeedback.PerfectActionQualityScore +
                thresholdCrossingEpsilonSeconds;
            var cleanHold = new[] { Vector2.zero };
            var excessiveMovement = new[] { Vector2.zero, new Vector2(0.21f, 0f) };
            float earlyScore = ScoreGesture(MoonlightGestureKind.Hold, cleanHold, 0.45f);
            float readyScore = ScoreGesture(MoonlightGestureKind.Hold, cleanHold, readyDuration);
            float greatScore = ScoreGesture(MoonlightGestureKind.Hold, cleanHold, greatDuration);
            float perfectScore = ScoreGesture(MoonlightGestureKind.Hold, cleanHold, perfectDuration);
            float movedScore = ScoreGesture(MoonlightGestureKind.Hold, excessiveMovement, 1f);
            MoonlightSpatialActionKind[] liveHoldActionKinds =
            {
                MoonlightSpatialActionKind.Cook,
                MoonlightSpatialActionKind.Garden,
                MoonlightSpatialActionKind.Read,
                MoonlightSpatialActionKind.Care
            };
            int[] liveHoldProgressSteps = { 2, 3, 3, 3 };
            int supportedLiveHoldActions = 0;
            for (int index = 0; index < liveHoldActionKinds.Length; index++)
            {
                if (MoonlightSpatialActionZone.IsLiveHoldReadinessStep(
                        liveHoldActionKinds[index], liveHoldProgressSteps[index],
                        MoonlightGestureKind.Hold))
                    supportedLiveHoldActions++;
            }
            bool fourOfFourActivities = supportedLiveHoldActions == 4 &&
                !MoonlightSpatialActionZone.IsLiveHoldReadinessStep(
                    MoonlightSpatialActionKind.Cook, 1, MoonlightGestureKind.Hold) &&
                !MoonlightSpatialActionZone.IsLiveHoldReadinessStep(
                    MoonlightSpatialActionKind.Garden, 3, MoonlightGestureKind.Swipe) &&
                !MoonlightSpatialActionZone.IsLiveHoldReadinessStep(
                    MoonlightSpatialActionKind.Play, 3, MoonlightGestureKind.Hold);
            bool labelsPass = MoonlightUI.LiveHoldReadinessLabel(earlyScore, passingScore) ==
                    "HOLD 0%" &&
                MoonlightUI.LiveHoldReadinessLabel(readyScore, passingScore) == "GOOD" &&
                MoonlightUI.LiveHoldReadinessLabel(greatScore, passingScore) == "GREAT" &&
                MoonlightUI.LiveHoldReadinessLabel(perfectScore, passingScore) == "PERFECT";
            bool scoresPass = earlyScore < passingScore && readyScore >= passingScore &&
                greatScore >= MoonlightActionFeedback.GreatActionQualityScore &&
                perfectScore >= MoonlightActionFeedback.PerfectActionQualityScore &&
                movedScore < passingScore;
            float bakeProgress = MoonlightUI.CalculateActivityProgress01(2, 0f, 4);
            string bakeProgressLabel = MoonlightUI.ActivityProgressLabel(3, 4);
            float finalHoldProgress = MoonlightUI.CalculateActivityProgress01(3, 0f, 4);
            string finalHoldProgressLabel = MoonlightUI.ActivityProgressLabel(4, 4);
            bool hapticOnce = !ShouldPlayLiveHoldReadinessHaptic(
                    false, earlyScore, passingScore) &&
                ShouldPlayLiveHoldReadinessHaptic(false, readyScore, passingScore) &&
                !ShouldPlayLiveHoldReadinessHaptic(true, greatScore, passingScore) &&
                !ShouldPlayLiveHoldReadinessHaptic(true, movedScore, passingScore) &&
                !MoonlightSpatialActionZone.ShouldPlayAcceptedGestureHaptic(true, true) &&
                MoonlightSpatialActionZone.ShouldPlayAcceptedGestureHaptic(true, false);
            bool existingNonRaycastOverlay = ResultOverlayObjectName ==
                    "GestureResultOverlay" && !ResultOverlayRaycastTarget;
            bool cleanupContract = IsLiveHoldReadinessStateClean(
                    false, false, false, 0f, 0) &&
                !IsLiveHoldReadinessStateClean(true, false, false, 0f, 0) &&
                !IsLiveHoldReadinessStateClean(false, true, false, 0f, 0) &&
                !IsLiveHoldReadinessStateClean(false, false, true, readyScore, 1);
            bool ipadGate = ShouldUseLiveHoldReadiness(true, true) &&
                !ShouldUseLiveHoldReadiness(false, true) &&
                !ShouldUseLiveHoldReadiness(true, false) &&
                !ShouldUseLiveHoldReadiness(false, false);
            detail = $"durations=0.450/{readyDuration:0.000}/{greatDuration:0.000}/" +
                $"{perfectDuration:0.000} scores={earlyScore:0.000}/{readyScore:0.000}/" +
                $"{greatScore:0.000}/{perfectScore:0.000} moved={movedScore:0.000} " +
                $"threshold={passingScore:0.00} labels={labelsPass} " +
                $"progress={bakeProgressLabel}:{bakeProgress:0.000}/" +
                $"{finalHoldProgressLabel}:{finalHoldProgress:0.000} " +
                $"hapticOnce={hapticOnce} overlayExistingNonRaycast={existingNonRaycastOverlay} " +
                $"activities={supportedLiveHoldActions}/4 cleanup={cleanupContract} " +
                $"ipadGate={ipadGate}";
            return Mathf.Approximately(passingScore, 0.58f) && scoresPass && labelsPass &&
                bakeProgressLabel == "3/4" && finalHoldProgressLabel == "4/4" &&
                hapticOnce && existingNonRaycastOverlay && fourOfFourActivities &&
                cleanupContract && ipadGate && Mathf.Approximately(bakeProgress, 0.5f) &&
                Mathf.Approximately(finalHoldProgress, 0.75f);
        }
    }
}
