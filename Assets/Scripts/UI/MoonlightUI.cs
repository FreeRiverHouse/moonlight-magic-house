using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlightMagicHouse
{
    public class MoonlightUI : MonoBehaviour
    {
        // Stat bars
        public Slider wonderBar;
        public Slider warmthBar;
        public Slider restBar;
        public Slider magicBar;
        public Slider hungerBar;

        // Info strip
        public TMP_Text stageLabel;
        public TMP_Text coinsLabel;
        public TMP_Text xpLabel;
        public TMP_Text moodEmoji;
        public TMP_Text daysLabel;
        public Text legacyStageLabel;
        public Text legacyCoinsLabel;
        public Text legacyXPLabel;
        public Text legacyMoodLabel;
        public Text legacyDaysLabel;

        // Action buttons
        public Button feedBtn;
        public Button cuddleBtn;
        public Button sleepBtn;
        public Button actionBtn;
        public TMP_Text actionBtnLabel;

        // Overlays
        public GameObject stagePanel;
        public TMP_Text   stagePanelLabel;
        public Text       legacyStagePanelLabel;
        public GameObject roomUnlockPanel;
        public TMP_Text   roomUnlockLabel;
        public Text       legacyRoomUnlockLabel;
        public GameObject offlinePanel;
        public GameObject sleepOverlay;

        // Prompt
        public GameObject promptRoot;
        public TMP_Text   promptLabel;
        public Text       legacyPromptLabel;
        public bool       keepPromptVisible;
        public TMP_Text   contextLabel;
        public TMP_Text   resultLabel;

        // Feed menu
        public GameObject  feedMenuRoot;
        public Transform   feedMenuContent;
        public FoodItem[]  foodCatalogue;
        public bool        feedOpensMenu = true;

        Coroutine _resultRoutine;
        MoonlightSpatialActionZone _resultZone;
        GameObject _roomNavigationRoot;
        TMP_Text _iPadProgressLabel;
        Image _iPadProgressFill;
        GameObject _iPadProgressRoot;
        RectTransform _iPadNavigationCueRoot;
        RectTransform _iPadNavigationChevron;
        TMP_Text _iPadNavigationLabel;
        RectTransform _iPadJoystickRect;
        MoonlightGesturePad _gesturePad;
        bool _iPadLayoutActive;
        bool _roomNavigationLocked;
        bool _activityPresentationWasVisible;
        string _gestureCommandMarker = "";
        string _activityPhaseMarker = "";
        int _lastQAScreenWidth;
        int _lastQAScreenHeight;
        Rect _lastQASafeArea;
        bool _qaReportPending;

        static readonly Vector2 IPadMinimumTouchTarget = new Vector2(96f, 88f);
        public const float NavigationCueSeparationPaddingPixels = 8f;
        public const int NavigationCueMaximumLabelCharacters = 10;
        const float IPadProgressTrackWidth = 84f;

        static readonly string[] MoodLabels = { "ASLEEP", "GRUMPY", "BORED", "CALM", "HAPPY", "RADIANT" };
        static TMP_FontAsset _runtimeUIFontAsset;
        static bool _runtimeUIFontResolved;
        // The character's name is Moonlight. "Stage" is still tracked internally for evolution/achievements,
        // but the HUD shows her name + stage descriptor rather than the raw stage codename ("Moonbud").
        static readonly string[] StageNames       = { "Moonlight", "Moonlight", "Moonlight", "Moonlight", "Moonlight" };
        static readonly string[] StageDescriptors = { "Sprout",    "Starling",  "Luminary",  "Sorceress", "Moonkeeper" };
        static readonly string[] RoomNames  = { "", "Living Room", "Kitchen", "Bedroom", "Garden", "Library" };

        // Runtime layout metrics used by iPad screenshot and touch-target QA.
        public const int RequiredVisibleTMPHUDLabelCount = 5;
        public bool IsIPadHUDLayoutActive => _iPadLayoutActive;
        public string HUDLayoutQAMarker => _iPadLayoutActive ? "ipad-activity-focus-v3" : "desktop-hud";
        public int VisibleTMPHUDLabelCount
        {
            get
            {
                int count = 0;
                foreach (var label in PrimaryHUDLabels())
                    if (IsVisibleTMPHUDLabel(label)) count++;
                return count;
            }
        }
        public bool VisibleTMPHUDLabelsActive =>
            VisibleTMPHUDLabelCount == RequiredVisibleTMPHUDLabelCount;
        public bool VisibleTMPHUDLabelsInsideSafeArea
        {
            get
            {
                foreach (var label in PrimaryHUDLabels())
                    if (!IsVisibleTMPHUDLabel(label) ||
                        !ContainsRect(Screen.safeArea, ScreenRect(label.transform as RectTransform)))
                        return false;
                return true;
            }
        }
        public bool VisibleTMPHUDLabelsDoNotOverlap
        {
            get
            {
                var labels = PrimaryHUDLabels();
                for (int i = 0; i < labels.Length; i++)
                {
                    if (!IsVisibleTMPHUDLabel(labels[i])) return false;
                    Rect first = ScreenRect(labels[i].transform as RectTransform);
                    for (int j = i + 1; j < labels.Length; j++)
                        if (OverlapsWithPadding(first,
                            ScreenRect(labels[j].transform as RectTransform), 2f))
                            return false;

                    if (promptLabel != null && promptLabel.gameObject.activeInHierarchy &&
                        OverlapsWithPadding(first,
                            ScreenRect(promptLabel.transform as RectTransform), 2f))
                        return false;
                }
                return true;
            }
        }
        public bool VisibleHUDHasNoLegacyMirrorDependency =>
            GetComponent<LegacyLabelMirror>() == null &&
            legacyStageLabel == null && legacyCoinsLabel == null && legacyXPLabel == null &&
            legacyMoodLabel == null && legacyDaysLabel == null;
        public bool VisibleHUDTypographyQAReady => VisibleTMPHUDLabelsActive &&
            VisibleTMPHUDLabelsInsideSafeArea && VisibleTMPHUDLabelsDoNotOverlap &&
            VisibleHUDHasNoLegacyMirrorDependency;
        public string VisibleHUDTypographyQAMarker => VisibleHUDTypographyQAReady
            ? "MOONLIGHT_VISIBLE_TMP_HUD_READY"
            : "MOONLIGHT_VISIBLE_TMP_HUD_INVALID";

        public static bool EnsureRuntimeFont(TMP_Text label)
        {
            if (label == null) return false;
            if (label.font != null && label.fontSharedMaterial != null) return true;

            if (!_runtimeUIFontResolved)
            {
                _runtimeUIFontResolved = true;
                try
                {
                    _runtimeUIFontAsset = TMP_Settings.defaultFontAsset;
                    if (_runtimeUIFontAsset == null)
                    {
                        Font source = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                        if (source != null)
                        {
                            _runtimeUIFontAsset = TMP_FontAsset.CreateFontAsset(source);
                            if (_runtimeUIFontAsset != null)
                                _runtimeUIFontAsset.hideFlags = HideFlags.HideAndDontSave;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[MoonlightHUDQA] Runtime TMP font creation failed: " +
                        exception.Message);
                }
            }

            if (_runtimeUIFontAsset == null) return false;
            label.font = _runtimeUIFontAsset;
            return label.fontSharedMaterial != null;
        }
        public bool IsRoomNavigationVisible => _roomNavigationRoot != null && _roomNavigationRoot.activeSelf;
        public bool IsRoomNavigationLocked => _roomNavigationLocked;
        public string RoomNavigationQAMarker => _roomNavigationLocked
            ? "activity-room-navigation-locked"
            : "room-navigation-ready";
        public string ActivityProgressQAMarker => _iPadProgressLabel != null && _iPadProgressRoot.activeSelf
            ? _iPadProgressLabel.text
            : "";
        public float ActivityProgressFill01 { get; private set; }
        public string ActivityProgressFillQAMarker => _iPadProgressFill != null &&
            _iPadProgressRoot != null && _iPadProgressRoot.activeSelf
                ? "MOONLIGHT_IPAD_ACTIVITY_PROGRESS_FILL_READY"
                : "MOONLIGHT_IPAD_ACTIVITY_PROGRESS_FILL_HIDDEN";
        public bool IsIPadNavigationCueVisible => _iPadNavigationCueRoot != null &&
            _iPadNavigationCueRoot.gameObject.activeSelf;
        public float NavigationCueAngleDegrees { get; private set; }
        public Vector2 NavigationCueCameraRelativeDirection { get; private set; }
        public string NavigationCueTargetName { get; private set; } = "";
        public string NavigationCueRenderedLabel { get; private set; } = "";
        public float NavigationCueTargetDistance { get; private set; } = float.MaxValue;
        public float NavigationCueVisualAngleDegrees => _iPadNavigationChevron != null
            ? Mathf.DeltaAngle(0f, _iPadNavigationChevron.localEulerAngles.z)
            : float.NaN;
        public Rect NavigationCueScreenRect => ScreenRect(_iPadNavigationCueRoot);
        public Rect JoystickScreenRect => ScreenRect(_iPadJoystickRect);
        public bool NavigationCueIsInsideSafeArea => ContainsRect(Screen.safeArea, NavigationCueScreenRect);
        public bool NavigationCueDoesNotOverlapJoystick =>
            !OverlapsWithPadding(NavigationCueScreenRect, JoystickScreenRect,
                NavigationCueSeparationPaddingPixels);
        public bool NavigationCueDoesNotOverlapActionTarget =>
            !OverlapsWithPadding(NavigationCueScreenRect, ActionTouchTargetScreenRect,
                NavigationCueSeparationPaddingPixels);
        public bool NavigationCueGraphicsDoNotBlockRaycasts
        {
            get
            {
                if (_iPadNavigationCueRoot == null) return false;
                foreach (var graphic in _iPadNavigationCueRoot.GetComponentsInChildren<Graphic>(true))
                    if (graphic.raycastTarget) return false;
                var group = _iPadNavigationCueRoot.GetComponent<CanvasGroup>();
                return group != null && !group.blocksRaycasts && !group.interactable;
            }
        }
        public string NavigationCueQAMarker
        {
            get
            {
                if (!IsIPadNavigationCueVisible) return "MOONLIGHT_IPAD_NAVIGATION_CUE_HIDDEN";
                return NavigationCueIsInsideSafeArea && NavigationCueDoesNotOverlapJoystick &&
                    NavigationCueDoesNotOverlapActionTarget &&
                    NavigationCueGraphicsDoNotBlockRaycasts &&
                    NavigationCueCameraRelativeDirection.sqrMagnitude > 0.999f &&
                    !float.IsNaN(NavigationCueAngleDegrees) &&
                    !float.IsInfinity(NavigationCueAngleDegrees) &&
                    !float.IsNaN(NavigationCueVisualAngleDegrees) &&
                    !string.IsNullOrEmpty(NavigationCueTargetName) &&
                    IsValidCompactNavigationLabel(NavigationCueRenderedLabel) &&
                    NavigationCueTargetDistance < float.MaxValue
                        ? "MOONLIGHT_IPAD_NAVIGATION_CUE_READY"
                        : "MOONLIGHT_IPAD_NAVIGATION_CUE_INVALID";
            }
        }
        public string GestureCommandQAMarker => _gestureCommandMarker;
        public string ActivityPhaseQAMarker => _activityPhaseMarker;
        public string ActionButtonQAText => actionBtnLabel != null ? actionBtnLabel.text : "";
        public bool ActionButtonQAInteractable => actionBtn != null && actionBtn.interactable;
        public Rect ActionTouchTargetScreenRect => ScreenRect(actionBtn != null
            ? actionBtn.transform as RectTransform
            : null);
        public Rect ActivityPromptScreenRect => ScreenRect(contextLabel != null
            ? contextLabel.transform as RectTransform
            : null);
        public Rect ActivityResultScreenRect => ScreenRect(resultLabel != null
            ? resultLabel.transform as RectTransform
            : null);
        public Rect ActivityProgressScreenRect => ScreenRect(_iPadProgressRoot != null
            ? _iPadProgressRoot.transform as RectTransform
            : null);
        public Rect HUDSafeAreaScreenRect => Screen.safeArea;
        public Vector4 HUDSafeAreaInsetsPixels => new Vector4(
            Screen.safeArea.xMin,
            Screen.safeArea.yMin,
            Screen.width - Screen.safeArea.xMax,
            Screen.height - Screen.safeArea.yMax);
        public Vector2 IPadMinimumTouchTargetLayoutSize => IPadMinimumTouchTarget;
        public Vector2 ActionTouchTargetLayoutSize
        {
            get
            {
                var rect = actionBtn != null ? actionBtn.transform as RectTransform : null;
                return rect != null ? rect.rect.size : Vector2.zero;
            }
        }
        public bool ActionTouchTargetMeetsIPadMinimum =>
            ActionTouchTargetLayoutSize.x >= IPadMinimumTouchTarget.x &&
            ActionTouchTargetLayoutSize.y >= IPadMinimumTouchTarget.y;
        public bool ActionTouchTargetIsInsideSafeArea => ContainsRect(Screen.safeArea, ActionTouchTargetScreenRect);
        public bool ActivityPromptIsInsideSafeArea => ContainsRect(Screen.safeArea, ActivityPromptScreenRect);
        public bool ActivityResultIsInsideSafeArea => ContainsRect(Screen.safeArea, ActivityResultScreenRect);
        public bool ActivityProgressIsInsideSafeArea => ContainsRect(Screen.safeArea, ActivityProgressScreenRect);
        public bool ActivityHUDPanelsDoNotOverlap =>
            !OverlapsWithPadding(ActivityPromptScreenRect, ActivityProgressScreenRect, 4f) &&
            !OverlapsWithPadding(ActivityResultScreenRect, ActivityProgressScreenRect, 4f) &&
            !OverlapsWithPadding(ActivityPromptScreenRect, ActionTouchTargetScreenRect, 4f) &&
            !OverlapsWithPadding(ActivityResultScreenRect, ActionTouchTargetScreenRect, 4f) &&
            !OverlapsWithPadding(ActivityProgressScreenRect, ActionTouchTargetScreenRect, 4f);
        public float ActivityPromptCenterOffsetPixels => Mathf.Abs(
            ActivityPromptScreenRect.center.x - Screen.safeArea.center.x);
        public bool HasContextResult => resultLabel != null && !string.IsNullOrEmpty(resultLabel.text);
        public int ContextResultLineCount => resultLabel == null || string.IsNullOrEmpty(resultLabel.text)
            ? 0
            : resultLabel.text.Split('\n').Length;
        public bool ContextResultIsOverflowing => resultLabel != null && resultLabel.isTextOverflowing;
        public bool ContextResultMatchesCurrentZone
        {
            get
            {
                if (!HasContextResult || _resultZone == null) return true;
                var moonlight = MoonlightGameManager.Instance?.moonlight;
                var interactor = moonlight != null
                    ? moonlight.GetComponent<MoonlightSpatialInteractor>()
                    : null;
                return interactor != null && interactor.CurrentZone == _resultZone;
            }
        }

        void Start()
        {
            if (cuddleBtn) cuddleBtn.onClick.AddListener(() =>
            {
                MoonlightGameManager.Instance?.moonlight.Cuddle();
                if (MoonlightGameManager.Instance?.moonlight != null)
                    Refresh(MoonlightGameManager.Instance.moonlight);
            });
            if (sleepBtn) sleepBtn.onClick.AddListener(() =>
            {
                MoonlightGameManager.Instance?.moonlight.PutToSleep();
                if (sleepOverlay) StartCoroutine(ShowThenHide(sleepOverlay, 2f));
                if (MoonlightGameManager.Instance?.moonlight != null)
                    Refresh(MoonlightGameManager.Instance.moonlight);
            });
            if (feedBtn && feedOpensMenu) feedBtn.onClick.AddListener(OpenFeedMenu);
            if (actionBtn && actionBtn.GetComponent<MoonlightGesturePad>() == null)
                actionBtn.onClick.AddListener(ExecuteContextAction);
            StartCoroutine(ReportVisibleHUDTypographyAfterCanvasUpdate());
        }

        // Called by MoonlightHouseSetup to inject all UI refs programmatically
        public void Wire(
            Slider wonder, Slider warmth, Slider rest, Slider magic, Slider hunger,
            TMP_Text stage, TMP_Text coins, TMP_Text xp, TMP_Text mood, TMP_Text days,
            Button feed, Button cuddle, Button sleep,
            GameObject stgPanel, TMP_Text stgLabel,
            GameObject roomPanel, TMP_Text roomLabel,
            GameObject offline, GameObject sleepOvr,
            GameObject feedRoot, Transform feedContent)
        {
            wonderBar = wonder; warmthBar = warmth; restBar = rest;
            magicBar  = magic;  hungerBar = hunger;
            stageLabel = stage; coinsLabel = coins; xpLabel = xp;
            moodEmoji  = mood;  daysLabel  = days;
            legacyStageLabel = null; legacyCoinsLabel = null; legacyXPLabel = null;
            legacyMoodLabel = null; legacyDaysLabel = null;
            feedBtn = feed; cuddleBtn = cuddle; sleepBtn = sleep;
            stagePanel = stgPanel; stagePanelLabel = stgLabel;
            legacyStagePanelLabel = null;
            roomUnlockPanel = roomPanel; roomUnlockLabel = roomLabel;
            legacyRoomUnlockLabel = null;
            offlinePanel = offline; sleepOverlay = sleepOvr;
            feedMenuRoot = feedRoot; feedMenuContent = feedContent;
        }

        public void WireLegacy(
            Slider wonder, Slider warmth, Slider rest, Slider magic, Slider hunger,
            Text stage, Text coins, Text xp, Text mood, Text days,
            Button feed, Button cuddle, Button sleep,
            GameObject stgPanel, Text stgLabel,
            GameObject roomPanel, Text roomLabel,
            GameObject offline, GameObject sleepOvr,
            GameObject feedRoot, Transform feedContent)
        {
            wonderBar = wonder; warmthBar = warmth; restBar = rest;
            magicBar  = magic;  hungerBar = hunger;
            stageLabel = null; coinsLabel = null; xpLabel = null;
            moodEmoji = null; daysLabel = null;
            legacyStageLabel = stage; legacyCoinsLabel = coins; legacyXPLabel = xp;
            legacyMoodLabel  = mood;  legacyDaysLabel  = days;
            feedBtn = feed; cuddleBtn = cuddle; sleepBtn = sleep;
            stagePanelLabel = null;
            stagePanel = stgPanel; legacyStagePanelLabel = stgLabel;
            roomUnlockLabel = null;
            roomUnlockPanel = roomPanel; legacyRoomUnlockLabel = roomLabel;
            offlinePanel = offline; sleepOverlay = sleepOvr;
            feedMenuRoot = feedRoot; feedMenuContent = feedContent;
        }

        public void WireSpatialAction(Button action, TMP_Text actionLabel, TMP_Text context, TMP_Text result)
        {
            actionBtn = action;
            actionBtnLabel = actionLabel;
            contextLabel = context;
            resultLabel = result;

            _iPadLayoutActive = ShouldUseIPadLayout();
            if (_iPadLayoutActive)
                ConfigureIPadActivityLayout();

            if (actionBtn != null)
            {
                actionBtn.onClick.RemoveListener(ExecuteContextAction);
                _gesturePad = actionBtn.GetComponent<MoonlightGesturePad>()
                    ?? actionBtn.gameObject.AddComponent<MoonlightGesturePad>();
                _gesturePad.Bind(this);
            }

            if (_iPadLayoutActive)
                RequestIPadLayoutReport();
        }

        public void WireRoomNavigation(GameObject roomNavigationRoot)
        {
            _roomNavigationRoot = roomNavigationRoot;
            ApplyRoomNavigationState();
        }

        public void WireIPadNavigationCue(RectTransform cueRoot, RectTransform chevron,
            TMP_Text label, RectTransform joystickRect)
        {
            _iPadNavigationCueRoot = cueRoot;
            _iPadNavigationChevron = chevron;
            _iPadNavigationLabel = label;
            _iPadJoystickRect = joystickRect;
            if (_iPadNavigationCueRoot != null)
                _iPadNavigationCueRoot.gameObject.SetActive(false);
            if (_iPadLayoutActive)
                RequestIPadLayoutReport();
        }

        void Update()
        {
            RefreshContextAction();
        }

        public void ShowPrompt(string text)
        {
            if (promptRoot == null) return;
            SetText(promptLabel, legacyPromptLabel, text);
            promptRoot.SetActive(true);
        }

        public void HidePrompt()
        {
            if (keepPromptVisible) return;
            promptRoot?.SetActive(false);
        }

        public void ShowContextResult(string text)
        {
            if (resultLabel == null) return;
            if (_resultRoutine != null) StopCoroutine(_resultRoutine);
            var moonlight = MoonlightGameManager.Instance?.moonlight;
            var interactor = moonlight != null
                ? moonlight.GetComponent<MoonlightSpatialInteractor>()
                : null;
            _resultZone = interactor != null ? interactor.CurrentZone : null;
            resultLabel.text = "";
            _resultRoutine = StartCoroutine(ShowContextResultAfterAction(text));
        }

        void RefreshContextAction()
        {
            var moonlight = MoonlightGameManager.Instance?.moonlight;
            var interactor = moonlight != null
                ? moonlight.GetComponent<MoonlightSpatialInteractor>()
                : null;
            var feedback = moonlight != null
                ? moonlight.GetComponent<MoonlightActionFeedback>()
                : null;
            var activityStage = moonlight != null
                ? moonlight.GetComponent<MoonlightActivityStage>()
                : null;

            bool hasAction = interactor != null && interactor.HasAction;
            bool performing = feedback != null && feedback.IsPerformingAction;
            bool coolingDown = feedback != null && feedback.IsCoolingDown;
            bool presenting = activityStage != null && activityStage.IsLingering &&
                interactor != null && interactor.CurrentZone != null &&
                interactor.CurrentZone.Kind == activityStage.CurrentKind;
            bool busy = performing || coolingDown || presenting;
            RefreshRoomNavigationState(interactor, presenting, busy);
            RefreshIPadNavigationCue(interactor, busy);
            _gesturePad?.SetGestureGuide(
                interactor?.CurrentZone != null ? interactor.CurrentZone.RequiredGesture : MoonlightGestureKind.Tap,
                _iPadLayoutActive && hasAction && !busy);
            if (actionBtn != null)
            {
                actionBtn.gameObject.SetActive(hasAction || busy);
                actionBtn.interactable = hasAction && !busy;
                if (actionBtn.image != null)
                    actionBtn.image.color = ActionColor(interactor != null ? interactor.CurrentZone : null, busy);
            }
            if (actionBtnLabel != null)
            {
                if (_iPadLayoutActive)
                    RefreshIPadActivityText(moonlight, interactor, feedback, activityStage,
                        hasAction, performing, coolingDown, presenting);
                else
                {
                    if (presenting) actionBtnLabel.text = "COMPLETE";
                    else if (performing) actionBtnLabel.text = feedback.ProgressText;
                    else if (coolingDown) actionBtnLabel.text = $"READY {feedback.CooldownRemaining:0.0}s";
                    else actionBtnLabel.text = hasAction ? interactor.CurrentActionLabel : "";
                }
            }
            if (!_iPadLayoutActive && contextLabel != null)
            {
                if (presenting)
                    contextLabel.text = "ACTIVITY COMPLETE  /  ENJOY THE RESULT";
                else if (performing)
                    contextLabel.text = $"{feedback.ProgressText}  /  {feedback.ActiveEffectName.ToUpperInvariant()}";
                else if (coolingDown)
                    contextLabel.text = $"READY IN {feedback.CooldownRemaining:0.0}s";
                else if (hasAction)
                    contextLabel.text = interactor.CurrentPrompt;
                else
                    contextLabel.text = interactor != null ? interactor.DiscoveryPrompt : "EXPLORE THIS ROOM";
            }
            if (_iPadLayoutActive &&
                (_lastQAScreenWidth != Screen.width || _lastQAScreenHeight != Screen.height ||
                 !_lastQASafeArea.Equals(Screen.safeArea)))
                RequestIPadLayoutReport();
        }

        void ConfigureIPadActivityLayout()
        {
            if (actionBtn != null)
            {
                var actionRect = actionBtn.transform as RectTransform;
                if (actionRect != null)
                {
                    actionRect.anchoredPosition = new Vector2(530f, 72f);
                    actionRect.sizeDelta = new Vector2(280f, 100f);
                }
            }

            ConfigureActivityLabel(contextLabel, new Vector2(-45f, 106f), new Vector2(650f, 42f),
                27f, 21f, 28f, FontStyles.Bold, Color.white);
            ConfigureActivityLabel(resultLabel, new Vector2(-45f, 58f), new Vector2(650f, 52f),
                18f, 14f, 19f, FontStyles.Bold, new Color(1f, 0.86f, 0.58f));
            if (resultLabel != null)
            {
                resultLabel.enableWordWrapping = true;
                resultLabel.maxVisibleLines = 2;
                resultLabel.overflowMode = TextOverflowModes.Ellipsis;
                resultLabel.lineSpacing = -3f;
                resultLabel.color = new Color(1f, 0.96f, 0.82f);
                var shadow = resultLabel.GetComponent<Shadow>() ?? resultLabel.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0.08f, 0.06f, 0.10f, 0.92f);
                shadow.effectDistance = new Vector2(2f, -2f);
                shadow.useGraphicAlpha = true;
            }
            ConfigureActivityLabel(actionBtnLabel, Vector2.zero, Vector2.zero,
                24f, 18f, 26f, FontStyles.Bold, Color.white, true);

            if (actionBtn == null || _iPadProgressRoot != null) return;

            _iPadProgressRoot = new GameObject("IPadActivityProgressQA");
            _iPadProgressRoot.transform.SetParent(actionBtn.transform.parent, false);
            var progressBackground = _iPadProgressRoot.AddComponent<Image>();
            progressBackground.color = new Color(0.08f, 0.09f, 0.11f, 0.76f);
            progressBackground.raycastTarget = false;
            var progressRect = _iPadProgressRoot.GetComponent<RectTransform>();
            progressRect.anchoredPosition = new Vector2(335f, 106f);
            progressRect.sizeDelta = new Vector2(92f, 42f);

            var fillObject = new GameObject("ProgressFill");
            fillObject.transform.SetParent(_iPadProgressRoot.transform, false);
            _iPadProgressFill = fillObject.AddComponent<Image>();
            _iPadProgressFill.color = new Color(0.42f, 0.86f, 1f, 0.68f);
            _iPadProgressFill.raycastTarget = false;
            var fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0.5f);
            fillRect.anchorMax = new Vector2(0f, 0.5f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.anchoredPosition = new Vector2(4f, 0f);
            fillRect.sizeDelta = new Vector2(0f, 34f);

            var labelObject = new GameObject("ProgressLabel");
            labelObject.transform.SetParent(_iPadProgressRoot.transform, false);
            _iPadProgressLabel = labelObject.AddComponent<TextMeshProUGUI>();
            EnsureRuntimeFont(_iPadProgressLabel);
            _iPadProgressLabel.fontSize = 23f;
            _iPadProgressLabel.fontStyle = FontStyles.Bold;
            _iPadProgressLabel.color = Color.white;
            _iPadProgressLabel.alignment = TextAlignmentOptions.Center;
            _iPadProgressLabel.raycastTarget = false;
            _iPadProgressLabel.characterSpacing = 0f;
            var progressShadow = labelObject.AddComponent<Shadow>();
            progressShadow.effectColor = new Color(0.03f, 0.04f, 0.06f, 0.92f);
            progressShadow.effectDistance = new Vector2(1.5f, -1.5f);
            progressShadow.useGraphicAlpha = true;
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            _iPadProgressRoot.SetActive(false);
        }

        void RefreshIPadActivityText(MoonlightCharacter moonlight, MoonlightSpatialInteractor interactor,
            MoonlightActionFeedback feedback, MoonlightActivityStage activityStage,
            bool hasAction, bool performing, bool coolingDown, bool presenting)
        {
            var zone = interactor != null ? interactor.CurrentZone : null;
            int step = 0;
            int requiredSteps = 1;
            bool showProgress = false;
            float progressFill = 0f;

            if (presenting && activityStage != null)
            {
                step = 4;
                requiredSteps = 4;
                showProgress = true;
                progressFill = 1f;
                string activityName = zone != null && zone.Kind == activityStage.CurrentKind
                    ? zone.DisplayName.ToUpperInvariant()
                    : activityStage.CurrentKind.ToString().ToUpperInvariant();
                if (contextLabel != null)
                    contextLabel.text = $"{activityName}  /  COMPLETE";
                actionBtnLabel.text = FinalActivityCountdownLabel(activityStage.LingerSecondsRemaining);
                _gestureCommandMarker = "FINAL PRESENTATION";
                _activityPhaseMarker = "COMPLETE";
            }
            else if (performing && feedback != null)
            {
                step = feedback.ActivityStep + 1;
                requiredSteps = feedback.ActivityRequiredSteps;
                showProgress = requiredSteps > 1;
                progressFill = CalculateActivityProgress01(feedback.ActivityStep,
                    feedback.ActionProgress01, requiredSteps);
                string verb = ProgressVerb(feedback.ProgressText);
                string phase = ActivityPhaseLabel(feedback.ActiveActivityKind,
                    feedback.ActivityStep, feedback.ActionContactPhase);
                if (contextLabel != null)
                    contextLabel.text = zone != null
                        ? $"{zone.DisplayName.ToUpperInvariant()}  /  {verb}  /  {phase}"
                        : $"{verb}  /  {phase}";
                actionBtnLabel.text = $"{phase}\n{Mathf.RoundToInt(feedback.ActionProgress01 * 100f)}%";
                _gestureCommandMarker = $"IN PROGRESS {phase}";
                _activityPhaseMarker = phase;
            }
            else if (coolingDown && feedback != null)
            {
                step = feedback.ActivityStep + 1;
                requiredSteps = feedback.ActivityRequiredSteps;
                showProgress = requiredSteps > 1;
                progressFill = CalculateActivityProgress01(feedback.ActivityStep + 1,
                    0f, requiredSteps);
                bool finishing = showProgress && step >= requiredSteps;
                if (contextLabel != null)
                    contextLabel.text = finishing
                        ? "ACTIVITY COMPLETE"
                        : zone != null ? $"NEXT  /  {zone.GetActionLabel(moonlight)}" : "GET READY";
                actionBtnLabel.text = $"READY IN\n{feedback.CooldownRemaining:0.0}s";
                _gestureCommandMarker = "COOLDOWN";
                _activityPhaseMarker = "GET READY";
            }
            else if (hasAction && zone != null)
            {
                step = zone.ProgressStep + 1;
                requiredSteps = zone.RequiredSteps;
                showProgress = requiredSteps > 1;
                progressFill = CalculateActivityProgress01(zone.ProgressStep, 0f, requiredSteps);
                string action = zone.GetActionLabel(moonlight);
                string gesture = CompactGestureCommand(zone.RequiredGesture);
                if (contextLabel != null)
                    contextLabel.text = $"{zone.DisplayName.ToUpperInvariant()}  /  {action}";
                if (_gesturePad != null && _gesturePad.IsLiveHoldReadinessActive)
                {
                    actionBtnLabel.text = LiveHoldReadinessLabel(
                        _gesturePad.LiveHoldScore, zone.PassingScore);
                    _gestureCommandMarker = actionBtnLabel.text;
                }
                else
                {
                    actionBtnLabel.text = $"{gesture}\n{action}";
                    _gestureCommandMarker = $"{gesture} {action}";
                }
                _activityPhaseMarker = "";
            }
            else
            {
                if (contextLabel != null)
                    contextLabel.text = CompactDiscoveryPrompt(interactor);
                actionBtnLabel.text = "";
                _gestureCommandMarker = "";
                _activityPhaseMarker = "";
            }

            if (_iPadProgressRoot != null)
            {
                _iPadProgressRoot.SetActive(showProgress);
                if (showProgress && _iPadProgressLabel != null)
                    _iPadProgressLabel.text = ActivityProgressLabel(step, requiredSteps);
                SetIPadProgressFill(showProgress ? progressFill : 0f,
                    zone != null ? ActionColor(zone, false) : new Color(0.42f, 0.86f, 1f, 0.96f));
            }
        }

        void SetIPadProgressFill(float progress, Color color)
        {
            ActivityProgressFill01 = Mathf.Clamp01(progress);
            if (_iPadProgressFill == null) return;
            var rect = _iPadProgressFill.transform as RectTransform;
            if (rect != null)
                rect.sizeDelta = new Vector2(IPadProgressTrackWidth * ActivityProgressFill01, 34f);
            color.a = 0.68f;
            _iPadProgressFill.color = color;
        }

        public static float CalculateActivityProgress01(int completedSteps,
            float activeStepProgress, int requiredSteps)
        {
            requiredSteps = Mathf.Max(1, requiredSteps);
            float total = Mathf.Clamp(completedSteps, 0, requiredSteps) +
                (completedSteps < requiredSteps ? Mathf.Clamp01(activeStepProgress) : 0f);
            return Mathf.Clamp01(total / requiredSteps);
        }

        public static string LiveHoldReadinessLabel(float score, float passingScore)
        {
            score = Mathf.Clamp01(score);
            if (score < Mathf.Clamp01(passingScore))
                return $"HOLD {Mathf.RoundToInt(score * 100f)}%";
            return MoonlightActionFeedback.ActionQualityTierFor(score)
                .ToString().ToUpperInvariant();
        }

        public static string ActivityProgressLabel(int step, int requiredSteps)
        {
            requiredSteps = Mathf.Max(1, requiredSteps);
            return $"{Mathf.Clamp(step, 1, requiredSteps)}/{requiredSteps}";
        }

        public static bool ValidateIPadProgressFeedbackContract(out string detail)
        {
            float start = CalculateActivityProgress01(0, 0f, 4);
            float firstHalf = CalculateActivityProgress01(0, 0.5f, 4);
            float firstComplete = CalculateActivityProgress01(1, 0f, 4);
            float thirdHalf = CalculateActivityProgress01(2, 0.5f, 4);
            float complete = CalculateActivityProgress01(4, 0f, 4);
            bool pass = Mathf.Approximately(start, 0f) &&
                Mathf.Approximately(firstHalf, 0.125f) &&
                Mathf.Approximately(firstComplete, 0.25f) &&
                Mathf.Approximately(thirdHalf, 0.625f) &&
                Mathf.Approximately(complete, 1f) &&
                start < firstHalf && firstHalf < firstComplete &&
                firstComplete < thirdHalf && thirdHalf < complete;
            detail = $"start={start:F3} firstHalf={firstHalf:F3} " +
                $"firstComplete={firstComplete:F3} thirdHalf={thirdHalf:F3} " +
                $"complete={complete:F3} track={IPadProgressTrackWidth:F0}px";
            return pass;
        }

        public static string FinalActivityCountdownLabel(float lingerSecondsRemaining)
        {
            int seconds = Mathf.Max(1, Mathf.CeilToInt(lingerSecondsRemaining));
            return $"NEXT IN\n{seconds}s";
        }

        public static bool TryParseFinalActivityCountdownLabel(string label, out int seconds)
        {
            seconds = 0;
            const string prefix = "NEXT IN\n";
            if (string.IsNullOrEmpty(label) || !label.StartsWith(prefix) ||
                !label.EndsWith("s"))
                return false;
            string value = label.Substring(prefix.Length,
                label.Length - prefix.Length - 1);
            return int.TryParse(value, out seconds) && seconds >= 1;
        }

        public static bool ValidateFinalActivityCtaSemanticsContract(out string detail)
        {
            string low = FinalActivityCountdownLabel(4.4f);
            string middle = FinalActivityCountdownLabel(4.8f);
            string high = FinalActivityCountdownLabel(5.2f);
            string minimum = FinalActivityCountdownLabel(0f);
            string exactFive = FinalActivityCountdownLabel(5f);
            string belowFive = FinalActivityCountdownLabel(4.999f);
            string exactOne = FinalActivityCountdownLabel(1f);
            string belowOne = FinalActivityCountdownLabel(0.999f);
            string negative = FinalActivityCountdownLabel(-0.001f);
            bool countdownPresent = low == "NEXT IN\n5s" && middle == "NEXT IN\n5s" &&
                high == "NEXT IN\n6s" && minimum == "NEXT IN\n1s";
            bool boundariesPass = exactFive == "NEXT IN\n5s" &&
                belowFive == "NEXT IN\n5s" && exactOne == "NEXT IN\n1s" &&
                belowOne == "NEXT IN\n1s" && negative == "NEXT IN\n1s";
            bool formatPass = TryParseFinalActivityCountdownLabel(high, out int parsedHigh) &&
                parsedHigh == 6 && TryParseFinalActivityCountdownLabel(minimum,
                    out int parsedMinimum) && parsedMinimum == 1;
            bool noCommandCta = !low.Contains("CONTINUE") && !middle.Contains("CONTINUE") &&
                !high.Contains("CONTINUE");
            detail = $"range=4.4-5.2s labels={low.Replace('\n', '/')}/" +
                $"{middle.Replace('\n', '/')}/{high.Replace('\n', '/')} " +
                $"minimum={minimum.Replace('\n', '/')} countdown={countdownPresent} " +
                $"boundaries={boundariesPass} format={formatPass} " +
                $"noCommandCta={noCommandCta}";
            return countdownPresent && boundariesPass && formatPass && noCommandCta;
        }

        public static Vector2 CameraRelativeNavigationDirection(Vector2 worldDirectionXZ,
            Vector2 cameraForwardXZ, Vector2 cameraRightXZ)
        {
            if (worldDirectionXZ.sqrMagnitude <= Mathf.Epsilon) return Vector2.zero;
            worldDirectionXZ.Normalize();
            cameraForwardXZ = cameraForwardXZ.sqrMagnitude > Mathf.Epsilon
                ? cameraForwardXZ.normalized
                : Vector2.up;
            cameraRightXZ = cameraRightXZ.sqrMagnitude > Mathf.Epsilon
                ? cameraRightXZ.normalized
                : Vector2.right;
            return new Vector2(Vector2.Dot(worldDirectionXZ, cameraRightXZ),
                Vector2.Dot(worldDirectionXZ, cameraForwardXZ)).normalized;
        }

        public static float CameraRelativeNavigationAngle(Vector2 cameraRelativeDirection)
        {
            if (cameraRelativeDirection.sqrMagnitude <= Mathf.Epsilon) return 0f;
            cameraRelativeDirection.Normalize();
            return Mathf.Atan2(-cameraRelativeDirection.x, cameraRelativeDirection.y) * Mathf.Rad2Deg;
        }

        public static bool ShouldShowIPadNavigationCue(bool isIPadLayout, bool hasTarget, bool busy) =>
            isIPadLayout && hasTarget && !busy;

        public static string CompactNavigationLabel(MoonlightSpatialActionKind kind) => kind switch
        {
            MoonlightSpatialActionKind.Cook => "KITCHEN",
            MoonlightSpatialActionKind.Play => "STAR BALL",
            MoonlightSpatialActionKind.Garden => "GARDEN",
            MoonlightSpatialActionKind.Read => "STORY",
            MoonlightSpatialActionKind.SleepCuddle => "BED",
            MoonlightSpatialActionKind.Care => "CARE",
            _ => "ACTIVITY"
        };

        public static bool IsValidCompactNavigationLabel(string label) =>
            !string.IsNullOrWhiteSpace(label) && label.Length <= NavigationCueMaximumLabelCharacters;

        public static bool ValidateIPadNavigationCueContract(out string detail)
        {
            Vector2 forward = CameraRelativeNavigationDirection(Vector2.up, Vector2.up, Vector2.right);
            Vector2 right = CameraRelativeNavigationDirection(Vector2.right, Vector2.up, Vector2.right);
            Vector2 back = CameraRelativeNavigationDirection(Vector2.down, Vector2.up, Vector2.right);
            Vector2 left = CameraRelativeNavigationDirection(Vector2.left, Vector2.up, Vector2.right);
            float forwardAngle = CameraRelativeNavigationAngle(forward);
            float rightAngle = CameraRelativeNavigationAngle(right);
            float backAngle = Mathf.Abs(CameraRelativeNavigationAngle(back));
            float leftAngle = CameraRelativeNavigationAngle(left);
            var compactLabelDetails = new List<string>();
            bool compactLabelsValid = true;
            foreach (MoonlightSpatialActionKind kind in Enum.GetValues(typeof(MoonlightSpatialActionKind)))
            {
                string label = CompactNavigationLabel(kind);
                compactLabelDetails.Add($"{kind}:{label}");
                compactLabelsValid &= IsValidCompactNavigationLabel(label);
            }
            bool compactLabelSemantics = CompactNavigationLabel(MoonlightSpatialActionKind.Cook) == "KITCHEN" &&
                CompactNavigationLabel(MoonlightSpatialActionKind.Play) == "STAR BALL" &&
                CompactNavigationLabel(MoonlightSpatialActionKind.Garden) == "GARDEN" &&
                CompactNavigationLabel(MoonlightSpatialActionKind.Read) == "STORY" &&
                CompactNavigationLabel(MoonlightSpatialActionKind.SleepCuddle) == "BED" &&
                CompactNavigationLabel(MoonlightSpatialActionKind.Care) == "CARE";
            string fallbackLabel = CompactNavigationLabel((MoonlightSpatialActionKind)int.MaxValue);
            bool compactFallbackValid = fallbackLabel == "ACTIVITY" &&
                IsValidCompactNavigationLabel(fallbackLabel);
            bool pass = ApproximatelyAngle(forwardAngle, 0f) &&
                ApproximatelyAngle(rightAngle, -90f) && ApproximatelyAngle(backAngle, 180f) &&
                ApproximatelyAngle(leftAngle, 90f) &&
                Mathf.Approximately(forward.magnitude, 1f) && Mathf.Approximately(right.magnitude, 1f) &&
                Mathf.Approximately(back.magnitude, 1f) && Mathf.Approximately(left.magnitude, 1f) &&
                ShouldShowIPadNavigationCue(true, true, false) &&
                !ShouldShowIPadNavigationCue(true, true, true) &&
                !ShouldShowIPadNavigationCue(true, false, false) &&
                !ShouldShowIPadNavigationCue(false, true, false) &&
                NavigationCueSeparationPaddingPixels >= 8f && compactLabelsValid &&
                compactLabelSemantics && compactFallbackValid;
            detail = $"angles={forwardAngle:0}/{rightAngle:0}/{backAngle:0}/{leftAngle:0} " +
                $"normalized={forward.magnitude:0.000}/{right.magnitude:0.000}/" +
                $"{back.magnitude:0.000}/{left.magnitude:0.000} visibility=out-of-range-not-busy " +
                $"separationPadding={NavigationCueSeparationPaddingPixels:0}px " +
                $"compactLabels={string.Join(",", compactLabelDetails)} " +
                $"compactFallback={fallbackLabel} compactMax={NavigationCueMaximumLabelCharacters}";
            return pass;
        }

        static bool ApproximatelyAngle(float actual, float expected) =>
            Mathf.Abs(Mathf.DeltaAngle(actual, expected)) <= 0.01f;

        void RefreshIPadNavigationCue(MoonlightSpatialInteractor interactor, bool busy)
        {
            if (_iPadNavigationCueRoot == null) return;

            bool hasTarget = interactor != null && interactor.HasNavigationTarget;
            bool show = ShouldShowIPadNavigationCue(_iPadLayoutActive, hasTarget, busy);
            if (hasTarget)
            {
                var cameraTransform = Camera.main != null ? Camera.main.transform : null;
                Vector2 cameraForward = cameraTransform != null
                    ? new Vector2(cameraTransform.forward.x, cameraTransform.forward.z)
                    : Vector2.up;
                Vector2 cameraRight = cameraTransform != null
                    ? new Vector2(cameraTransform.right.x, cameraTransform.right.z)
                    : Vector2.right;
                NavigationCueCameraRelativeDirection = CameraRelativeNavigationDirection(
                    interactor.NearestZoneDirectionXZ, cameraForward, cameraRight);
                NavigationCueAngleDegrees = CameraRelativeNavigationAngle(
                    NavigationCueCameraRelativeDirection);
                NavigationCueTargetName = interactor.NearestZone.DisplayName;
                NavigationCueRenderedLabel = CompactNavigationLabel(interactor.NearestZone.Kind);
                NavigationCueTargetDistance = interactor.NearestDistance;
                if (_iPadNavigationChevron != null)
                    _iPadNavigationChevron.localRotation = Quaternion.Euler(0f, 0f,
                        NavigationCueAngleDegrees);
                if (_iPadNavigationLabel != null)
                    _iPadNavigationLabel.text =
                        $"{NavigationCueRenderedLabel}\n{NavigationCueTargetDistance:0.0}m";
            }
            else
            {
                NavigationCueCameraRelativeDirection = Vector2.zero;
                NavigationCueAngleDegrees = 0f;
                NavigationCueTargetName = "";
                NavigationCueRenderedLabel = "";
                NavigationCueTargetDistance = float.MaxValue;
            }

            _iPadNavigationCueRoot.gameObject.SetActive(show);
        }

        static void ConfigureActivityLabel(TMP_Text label, Vector2 position, Vector2 size,
            float fontSize, float minimumFontSize, float maximumFontSize, FontStyles style,
            Color color, bool stretch = false)
        {
            if (label == null) return;
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = true;
            label.fontSizeMin = minimumFontSize;
            label.fontSizeMax = maximumFontSize;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.characterSpacing = 0f;
            label.raycastTarget = false;

            var rect = label.transform as RectTransform;
            if (rect == null) return;
            if (stretch)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = new Vector2(12f, 8f);
                rect.offsetMax = new Vector2(-12f, -8f);
            }
            else
            {
                rect.anchoredPosition = position;
                rect.sizeDelta = size;
            }
        }

        static string CompactGestureCommand(MoonlightGestureKind gesture) => gesture switch
        {
            MoonlightGestureKind.Circle => "DRAW CIRCLE",
            MoonlightGestureKind.Hold => "PRESS + HOLD",
            MoonlightGestureKind.Swipe => "SWIPE",
            MoonlightGestureKind.ZigZag => "DRAW ZIG-ZAG",
            _ => "TAP"
        };

        static string CompactDiscoveryPrompt(MoonlightSpatialInteractor interactor)
        {
            if (interactor == null || interactor.NearestZone == null) return "EXPLORE THIS ROOM";
            return interactor.NearestDistance < float.MaxValue
                ? $"MOVE TO {interactor.NearestZone.DisplayName.ToUpperInvariant()}  /  {interactor.NearestDistance:0.0}m"
                : $"MOVE TO {interactor.NearestZone.DisplayName.ToUpperInvariant()}";
        }

        static string ProgressVerb(string progressText)
        {
            if (string.IsNullOrEmpty(progressText)) return "IN PROGRESS";
            int separator = progressText.IndexOf("  ", StringComparison.Ordinal);
            return separator > 0 ? progressText.Substring(0, separator) : progressText;
        }

        public static string ActivityPhaseLabel(string phase) => phase switch
        {
            "anticipation" => "GET READY",
            "approach" => "REACH",
            "contact" => "MAKE CONTACT",
            "follow-through" => "FOLLOW THROUGH",
            "recovery" => "SETTLE",
            _ => "IN MOTION"
        };

        public static string ActivityPhaseLabel(MoonlightSpatialActionKind kind, int step,
            string phase)
        {
            if (kind != MoonlightSpatialActionKind.Play) return ActivityPhaseLabel(phase);
            return (Mathf.Clamp(step, 0, 3), phase) switch
            {
                (0, "anticipation") => "AIM",
                (0, "approach") => "WIND UP",
                (0, "contact") => "RELEASE",
                (0, "follow-through") => "BALL FLIGHT",
                (0, "recovery") => "THROW RESET",
                (1, "anticipation") => "TRACK PATH",
                (1, "approach") => "PURSUE",
                (1, "contact") => "ZIG-ZAG",
                (1, "follow-through") => "DASH THROUGH",
                (1, "recovery") => "CHASE RESET",
                (2, "anticipation") => "CROUCH",
                (2, "approach") => "TAKE OFF",
                (2, "contact") => "CLEAR ARC",
                (2, "follow-through") => "LAND",
                (2, "recovery") => "LANDING SET",
                (3, "anticipation") => "TRACK BALL",
                (3, "approach") => "REACH",
                (3, "contact") => "CATCH",
                (3, "follow-through") => "HOLD BALL",
                (3, "recovery") => "PRESENT",
                _ => "BALL IN MOTION"
            };
        }

        public static bool ValidateActivityPhaseFeedbackContract(out string detail)
        {
            string[] source =
                { "anticipation", "approach", "contact", "follow-through", "recovery" };
            var labels = new HashSet<string>();
            int longest = 0;
            foreach (string phase in source)
            {
                string label = ActivityPhaseLabel(phase);
                labels.Add(label);
                longest = Mathf.Max(longest, label.Length);
            }
            bool semanticPass = ActivityPhaseLabel("contact") == "MAKE CONTACT" &&
                ActivityPhaseLabel("follow-through") == "FOLLOW THROUGH" &&
                ActivityPhaseLabel("") == "IN MOTION";
            bool playSpecific = true;
            int longestPlay = 0;
            var playLabels = new HashSet<string>();
            string[,] expectedPlay =
            {
                { "AIM", "WIND UP", "RELEASE", "BALL FLIGHT", "THROW RESET" },
                { "TRACK PATH", "PURSUE", "ZIG-ZAG", "DASH THROUGH", "CHASE RESET" },
                { "CROUCH", "TAKE OFF", "CLEAR ARC", "LAND", "LANDING SET" },
                { "TRACK BALL", "REACH", "CATCH", "HOLD BALL", "PRESENT" }
            };
            for (int step = 0; step < 4; step++)
            {
                for (int phaseIndex = 0; phaseIndex < source.Length; phaseIndex++)
                {
                    string phase = source[phaseIndex];
                    string label = ActivityPhaseLabel(MoonlightSpatialActionKind.Play, step, phase);
                    longestPlay = Mathf.Max(longestPlay, label.Length);
                    playLabels.Add(label);
                    playSpecific &= label == expectedPlay[step, phaseIndex] && label.Length <= 14;
                }
            }
            playSpecific &= playLabels.Count == 20;
            detail = $"phases={source.Length} unique={labels.Count} longest={longest} " +
                $"playUnique={playLabels.Count}/20 playLongest={longestPlay} " +
                $"playSpecific={playSpecific} semantic={semanticPass}";
            return source.Length == 5 && labels.Count == 5 && longest <= 14 && semanticPass &&
                playSpecific;
        }

        void RefreshRoomNavigationState(MoonlightSpatialInteractor interactor, bool presenting, bool busy)
        {
            if (presenting)
                _activityPresentationWasVisible = true;

            if (_roomNavigationLocked)
            {
                var zone = interactor != null ? interactor.CurrentZone : null;
                bool leftActivity = zone == null || zone.RequiredSteps <= 1;
                bool presentationFinished = _activityPresentationWasVisible && !busy;
                if (leftActivity || presentationFinished)
                    SetRoomNavigationLocked(false);
            }
        }

        void SetRoomNavigationLocked(bool locked)
        {
            if (_roomNavigationLocked == locked)
            {
                ApplyRoomNavigationState();
                return;
            }

            _roomNavigationLocked = locked;
            _activityPresentationWasVisible = false;
            ApplyRoomNavigationState();
            Debug.Log($"[MoonlightHUDQA] room-navigation visible={IsRoomNavigationVisible} " +
                $"locked={_roomNavigationLocked} marker={RoomNavigationQAMarker}");
        }

        void ApplyRoomNavigationState()
        {
            if (_roomNavigationRoot != null)
                _roomNavigationRoot.SetActive(!_roomNavigationLocked);
        }

        public static bool ShouldUseIPadLayout()
        {
            foreach (string argument in Environment.GetCommandLineArgs())
                if (string.Equals(argument, "-moonlightIPadHudQa", StringComparison.OrdinalIgnoreCase))
                    return true;

            if (Application.platform != RuntimePlatform.IPhonePlayer) return false;
            string model = SystemInfo.deviceModel ?? "";
            if (model.IndexOf("iPad", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            float shortSide = Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height));
            float aspect = Mathf.Max(Screen.width, Screen.height) / shortSide;
            return aspect <= 1.55f;
        }

        void RequestIPadLayoutReport()
        {
            if (_qaReportPending) return;
            _qaReportPending = true;
            StartCoroutine(ReportIPadLayoutAfterCanvasUpdate());
        }

        IEnumerator ReportIPadLayoutAfterCanvasUpdate()
        {
            yield return new WaitForEndOfFrame();
            Canvas.ForceUpdateCanvases();
            _lastQAScreenWidth = Screen.width;
            _lastQAScreenHeight = Screen.height;
            _lastQASafeArea = Screen.safeArea;
            _qaReportPending = false;
            Rect touchRect = ActionTouchTargetScreenRect;
            Debug.Log($"[MoonlightHUDQA] marker={HUDLayoutQAMarker} screen={Screen.width}x{Screen.height} " +
                $"typography={VisibleHUDTypographyQAMarker} tmpLabels={VisibleTMPHUDLabelCount}/" +
                $"{RequiredVisibleTMPHUDLabelCount} typographySafe={VisibleTMPHUDLabelsInsideSafeArea} " +
                $"typographySeparated={VisibleTMPHUDLabelsDoNotOverlap} noMirror={VisibleHUDHasNoLegacyMirrorDependency} " +
                $"safe={Screen.safeArea} touchPixels={touchRect.size} touchLayout={ActionTouchTargetLayoutSize} " +
                $"touchMinimumPass={ActionTouchTargetMeetsIPadMinimum} insideSafeArea={ActionTouchTargetIsInsideSafeArea} " +
                $"promptSafe={ActivityPromptIsInsideSafeArea} resultSafe={ActivityResultIsInsideSafeArea} " +
                $"progressSafe={ActivityProgressIsInsideSafeArea} panelsSeparated={ActivityHUDPanelsDoNotOverlap} " +
                $"promptCenterOffset={ActivityPromptCenterOffsetPixels:0.0}px " +
                $"navigationCue={NavigationCueQAMarker} cueSafe={NavigationCueIsInsideSafeArea} " +
                $"cueJoystickSeparated={NavigationCueDoesNotOverlapJoystick} " +
                $"cueActionSeparated={NavigationCueDoesNotOverlapActionTarget} " +
                $"cueRect={NavigationCueScreenRect} actionRect={ActionTouchTargetScreenRect} " +
                $"cueAngle={NavigationCueAngleDegrees:0.0} cueTarget={NavigationCueTargetName} " +
                $"cueLabel={NavigationCueRenderedLabel}/{NavigationCueRenderedLabel.Length} " +
                $"cueDistance={NavigationCueTargetDistance:0.0}m");
        }

        IEnumerator ReportVisibleHUDTypographyAfterCanvasUpdate()
        {
            yield return new WaitForEndOfFrame();
            Canvas.ForceUpdateCanvases();
            Debug.Log($"[MoonlightHUDQA] typography={VisibleHUDTypographyQAMarker} " +
                $"tmpLabels={VisibleTMPHUDLabelCount}/{RequiredVisibleTMPHUDLabelCount} " +
                $"active={VisibleTMPHUDLabelsActive} safe={VisibleTMPHUDLabelsInsideSafeArea} " +
                $"separated={VisibleTMPHUDLabelsDoNotOverlap} " +
                $"noMirror={VisibleHUDHasNoLegacyMirrorDependency}");
        }

        TMP_Text[] PrimaryHUDLabels() =>
            new[] { stageLabel, moodEmoji, coinsLabel, xpLabel, daysLabel };

        static bool IsVisibleTMPHUDLabel(TMP_Text label)
        {
            if (!(label is TextMeshProUGUI) || !label.isActiveAndEnabled ||
                !label.gameObject.activeInHierarchy || label.color.a <= 0.01f ||
                string.IsNullOrEmpty(label.text) || !EnsureRuntimeFont(label) ||
                label.canvasRenderer == null || label.canvasRenderer.cull)
                return false;
            label.ForceMeshUpdate(false, false);
            bool hasVisibleGlyph = false;
            for (int i = 0; i < label.textInfo.characterCount; i++)
            {
                if (!label.textInfo.characterInfo[i].isVisible) continue;
                hasVisibleGlyph = true;
                break;
            }
            Rect rect = ScreenRect(label.transform as RectTransform);
            return hasVisibleGlyph && !label.isTextOverflowing &&
                rect.width > 0f && rect.height > 0f;
        }

        static Rect ScreenRect(RectTransform rect)
        {
            if (rect == null) return Rect.zero;
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }

        static bool ContainsRect(Rect outer, Rect inner)
        {
            if (inner.width <= 0f || inner.height <= 0f) return false;
            const float tolerance = 1f;
            return inner.xMin >= outer.xMin - tolerance && inner.xMax <= outer.xMax + tolerance &&
                inner.yMin >= outer.yMin - tolerance && inner.yMax <= outer.yMax + tolerance;
        }

        static bool OverlapsWithPadding(Rect first, Rect second, float padding)
        {
            if (first.width <= 0f || first.height <= 0f || second.width <= 0f || second.height <= 0f)
                return true;

            float halfPadding = Mathf.Max(0f, padding) * 0.5f;
            first.xMin -= halfPadding;
            first.xMax += halfPadding;
            first.yMin -= halfPadding;
            first.yMax += halfPadding;
            second.xMin -= halfPadding;
            second.xMax += halfPadding;
            second.yMin -= halfPadding;
            second.yMax += halfPadding;
            return first.Overlaps(second);
        }

        static Color ActionColor(MoonlightSpatialActionZone zone, bool coolingDown)
        {
            if (coolingDown) return new Color(0.43f, 0.43f, 0.48f, 0.92f);
            if (zone == null) return new Color(0.35f, 0.38f, 0.44f, 0.82f);
            return zone.Kind switch
            {
                MoonlightSpatialActionKind.Cook => new Color(0.90f, 0.61f, 0.26f, 0.96f),
                MoonlightSpatialActionKind.Play => new Color(0.30f, 0.67f, 0.78f, 0.96f),
                MoonlightSpatialActionKind.Garden => new Color(0.34f, 0.68f, 0.42f, 0.96f),
                MoonlightSpatialActionKind.Read => new Color(0.76f, 0.54f, 0.26f, 0.96f),
                MoonlightSpatialActionKind.SleepCuddle => new Color(0.71f, 0.48f, 0.72f, 0.96f),
                MoonlightSpatialActionKind.Care => new Color(0.16f, 0.74f, 0.66f, 0.96f),
                _ => new Color(0.46f, 0.58f, 0.72f, 0.96f)
            };
        }

        void ExecuteContextAction()
        {
            var interactor = MoonlightGameManager.Instance?.moonlight != null
                ? MoonlightGameManager.Instance.moonlight.GetComponent<MoonlightSpatialInteractor>()
                : null;
            if (interactor == null) return;

            var result = interactor.ExecuteCurrent();
            ShowContextResult(result);
            if (MoonlightGameManager.Instance?.moonlight != null)
                Refresh(MoonlightGameManager.Instance.moonlight);
        }

        public void ExecuteContextGesture(MoonlightGestureKind gesture, float score,
            bool acceptedHapticAlreadyPlayed = false)
            => ExecuteContextGesture(gesture,
                MoonlightGestureSample.Synthetic(gesture, score), acceptedHapticAlreadyPlayed);

        public void ExecuteContextGesture(MoonlightGestureKind gesture,
            MoonlightGestureSample sample, bool acceptedHapticAlreadyPlayed = false)
        {
            var moonlight = MoonlightGameManager.Instance?.moonlight;
            var interactor = moonlight != null
                ? moonlight.GetComponent<MoonlightSpatialInteractor>()
                : null;
            if (interactor == null || interactor.CurrentZone == null) return;

            var zone = interactor.CurrentZone;
            string result = zone.ExecuteGesture(moonlight, gesture, sample,
                acceptedHapticAlreadyPlayed);
            if (zone.LastGesturePassed && zone.RequiredSteps > 1)
                SetRoomNavigationLocked(true);
            MoonlightVisualQA.Instance?.LogContextAction(zone, moonlight.transform.position, result);
            ShowContextResult(result);
            Refresh(moonlight);
        }

        public void Refresh(MoonlightCharacter m)
        {
            if (wonderBar) wonderBar.value = m.stats.wonder / 100f;
            if (warmthBar) warmthBar.value = m.stats.warmth / 100f;
            if (restBar)   restBar.value   = m.stats.rest   / 100f;
            if (magicBar)  magicBar.value  = m.stats.magic  / 100f;
            if (hungerBar) hungerBar.value = m.stats.hunger / 100f;

            int stageIndex = Mathf.Clamp((int)m.stage, 0, StageDescriptors.Length - 1);
            SetText(stageLabel, legacyStageLabel,
                $"{StageNames[stageIndex]} / {StageDescriptors[stageIndex]}");
            SetText(coinsLabel, legacyCoinsLabel, $"COINS {m.coins}");
            SetText(xpLabel, legacyXPLabel, $"XP {m.xp}");
            SetText(daysLabel, legacyDaysLabel, $"DAY {Mathf.FloorToInt(m.daysInHouse) + 1}");
            SetText(moodEmoji, legacyMoodLabel, MoodLabels[(int)m.stats.GetMood()]);
            UpdateCarePrompt(m);
        }

        public void OnMoodChange(MoonlightMood mood)
        {
            SetText(moodEmoji, legacyMoodLabel, MoodLabels[(int)mood]);
        }
        public void UpdateCoins(int coins) => SetText(coinsLabel, legacyCoinsLabel, $"COINS {coins}");
        public void UpdateXP(int xp)       => SetText(xpLabel, legacyXPLabel, $"XP {xp}");

        public void ShowStageCelebration(MoonlightStage stage)
        {
            SetText(stagePanelLabel, legacyStagePanelLabel, $"Moonlight shines brighter!");
            if (stagePanel) StartCoroutine(ShowThenHide(stagePanel, 4f));
        }

        public void ShowRoomUnlocked(int count)
        {
            SetText(roomUnlockLabel, legacyRoomUnlockLabel, $"New room unlocked: {RoomNames[Mathf.Min(count, RoomNames.Length-1)]}!");
            if (roomUnlockPanel) StartCoroutine(ShowThenHide(roomUnlockPanel, 3f));
        }

        public void ShowOfflineNotice()
        {
            if (offlinePanel) StartCoroutine(ShowThenHide(offlinePanel, 1.1f));
        }

        public void OpenFeedMenuWith(List<FoodItem> overrideCatalogue)
        {
            PopulateFeedMenu(overrideCatalogue);
            if (feedMenuRoot) feedMenuRoot.SetActive(true);
        }

        void OpenFeedMenu()
        {
            if (foodCatalogue != null)
                PopulateFeedMenu(new List<FoodItem>(foodCatalogue));
            else if (feedMenuRoot)
                feedMenuRoot.SetActive(true);
        }

        void PopulateFeedMenu(List<FoodItem> catalogue)
        {
            if (feedMenuContent == null) return;
            foreach (Transform t in feedMenuContent) Destroy(t.gameObject);
            foreach (var food in catalogue)
            {
                var itemGO = new GameObject(food.itemName);
                itemGO.transform.SetParent(feedMenuContent, false);
                var btn = itemGO.AddComponent<Button>();
                var img = itemGO.AddComponent<Image>();
                img.color = new Color(0.2f, 0.1f, 0.35f);
                var rt = itemGO.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(200, 60);

                var lblGO = new GameObject("Label");
                lblGO.transform.SetParent(itemGO.transform, false);
                var lbl = lblGO.AddComponent<TextMeshProUGUI>();
                EnsureRuntimeFont(lbl);
                lbl.text = $"{food.itemName}\nCOINS {food.cost}";
                lbl.fontSize = 18;
                lbl.alignment = TextAlignmentOptions.Center;
                var lblRt = lbl.GetComponent<RectTransform>();
                lblRt.anchorMin = Vector2.zero;
                lblRt.anchorMax = Vector2.one;
                lblRt.offsetMin = Vector2.zero;
                lblRt.offsetMax = Vector2.zero;

                var captured = food;
                btn.onClick.AddListener(() =>
                {
                    MoonlightGameManager.Instance?.moonlight.Feed(captured);
                    if (MoonlightGameManager.Instance?.moonlight != null)
                        Refresh(MoonlightGameManager.Instance.moonlight);
                    if (feedMenuRoot) feedMenuRoot.SetActive(false);
                });
            }
        }

        IEnumerator ShowThenHide(GameObject panel, float dur)
        {
            panel.SetActive(true);
            yield return new WaitForSeconds(dur);
            panel.SetActive(false);
        }

        void UpdateCarePrompt(MoonlightCharacter m)
        {
            if (promptRoot == null) return;

            var s = m.stats;
            string prompt;
            if (s.hunger < 35f)
                prompt = "Moonlight wants a snack";
            else if (s.rest < 35f)
                prompt = "Moonlight needs a nap";
            else if (s.warmth < 35f)
                prompt = "Moonlight wants a hug";
            else if (s.wonder < 35f)
                prompt = "Moonlight wants to play";
            else if (s.magic < 35f)
                prompt = "Moonlight wants magic";
            else
            {
                var mood = s.GetMood();
                prompt = mood == MoonlightMood.Radiant ? "Moonlight is glowing" : "Moonlight feels cozy";
            }

            SetText(promptLabel, legacyPromptLabel, prompt);
            promptRoot.SetActive(true);
        }

        static void SetText(TMP_Text tmp, Text legacy, string value)
        {
            if (tmp) tmp.text = value;
            if (legacy) legacy.text = value;
        }

        IEnumerator ClearContextResultAfterDelay()
        {
            yield return new WaitForSeconds(3.5f);
            ClearContextResult();
        }

        IEnumerator ShowContextResultAfterAction(string text)
        {
            var moonlight = MoonlightGameManager.Instance?.moonlight;
            var feedback = moonlight != null
                ? moonlight.GetComponent<MoonlightActionFeedback>()
                : null;
            while (feedback != null && feedback.IsPerformingAction)
                yield return null;

            if (!ResultZoneStillCurrent())
            {
                ClearContextResult();
                yield break;
            }

            if (resultLabel != null)
            {
                resultLabel.text = _iPadLayoutActive ? FormatIPadResult(text) : text;
                resultLabel.ForceMeshUpdate();
            }
            float visibleUntil = Time.time + (IsCompletionResult(text) ? 5.6f : 2.8f);
            while (Time.time < visibleUntil && ResultZoneStillCurrent())
                yield return null;
            ClearContextResult();
        }

        static string FormatIPadResult(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string[] parts = text.Split(new[] { "  /  " }, StringSplitOptions.None);
            if (parts.Length < 2) return text;

            string firstLine = parts.Length > 1 ? $"{parts[0]}  •  {parts[1]}" : parts[0];
            if (parts.Length == 2) return firstLine;

            var secondLine = new System.Text.StringBuilder(parts[2]);
            for (int i = 3; i < parts.Length; i++)
                secondLine.Append("  ").Append(parts[i]);
            return $"{firstLine}\n{secondLine}";
        }

        static bool IsCompletionResult(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            return text.Contains("DECORATED", StringComparison.Ordinal) ||
                text.Contains("COMBO", StringComparison.Ordinal) ||
                text.Contains("BLOOMED", StringComparison.Ordinal) ||
                text.Contains("REMEMBERED", StringComparison.Ordinal) ||
                text.Contains("MOON SPA COMPLETE", StringComparison.Ordinal);
        }

        bool ResultZoneStillCurrent()
        {
            if (_resultZone == null) return true;
            var moonlight = MoonlightGameManager.Instance?.moonlight;
            var interactor = moonlight != null
                ? moonlight.GetComponent<MoonlightSpatialInteractor>()
                : null;
            return interactor != null && interactor.CurrentZone == _resultZone;
        }

        void ClearContextResult()
        {
            if (resultLabel != null) resultLabel.text = "";
            _resultZone = null;
            _resultRoutine = null;
        }
    }
}
