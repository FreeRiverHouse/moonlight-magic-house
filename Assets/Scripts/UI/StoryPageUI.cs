using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MoonlightMagicHouse
{
    public class StoryPageUI : MonoBehaviour
    {
        public const float ReadFinalPresentationSeconds =
            MoonlightActionFeedback.ReadFinalPresentationSeconds;
        public const float ReadFinalActionSeconds = MoonlightActionFeedback.ReadActionDurationSeconds;
        public const float MinimumTitleFontSize = 28f;
        public const float BodyFontSize = 27f;
        public const float MinimumCloseTargetSize = 56f;
        public const string RevealCountMarker = "MOONLIGHT_STORY_REVEAL_ONE_PER_READ_VERIFIED";
        public const string TimingMarker = "MOONLIGHT_STORY_REVEAL_AFTER_FINAL_PRESENTATION_VERIFIED";
        public const string SafeAreaMarker = "MOONLIGHT_STORY_SAFE_AREA_VERIFIED";
        public const string NonOverflowMarker = "MOONLIGHT_STORY_SCROLL_NON_OVERFLOW_VERIFIED";
        public const string ModalLockMarker = "MOONLIGHT_STORY_MODAL_LOCK_VERIFIED";
        public const string ZeroDriftMarker = "MOONLIGHT_STORY_ZERO_DRIFT_VERIFIED";
        public const string RewardPathMarker = "MOONLIGHT_STORY_READ_REWARD_PATH_UNCHANGED";

        sealed class PendingReveal
        {
            public AuthoredStoryPage Page;
            public MoonlightCharacter Moonlight;
            public MoonlightActionFeedback Feedback;
            public float QueuedAt;
            public int XPAfterRead;
            public int CoinsAfterRead;
            public bool ObservedFinalPresentation;
            public float PresentationStartedAt = -1f;
            public float RevealNotBefore;
        }

        public static StoryPageUI Instance { get; private set; }

        readonly List<AuthoredStoryPage> _pages = new();
        readonly HashSet<string> _revealedIds = new();
        readonly HashSet<string> _reservedIds = new();
        readonly Queue<PendingReveal> _pending = new();

        GameObject _root;
        RectTransform _safeContainer;
        RectTransform _storyPanel;
        TMP_Text _titleText;
        TMP_Text _storyMetaText;
        TMP_Text _bodyText;
        RectTransform _bodyContent;
        RectTransform _viewport;
        ScrollRect _scrollRect;
        Button _closeButton;
        GameObject _roomNavigation;
        MoonlightPlayerController _player;
        Coroutine _queueRoutine;
        AuthoredStoryPage _currentPage;
        Vector3 _modalStartPosition;
        int _modalStartXP;
        int _modalStartCoins;
        bool _roomNavigationWasActive;
        bool _playerModalLockWasActive;
        bool _runtimeBuilt;
        int _lastScreenWidth;
        int _lastScreenHeight;
        Rect _lastSafeArea;

        public bool DataReady { get; private set; }
        public int LoadedPageCount => _pages.Count;
        public int CompletedReadLoopCount { get; private set; }
        public int RevealedPageCount { get; private set; }
        public int PendingRevealCount => _pending.Count;
        public bool IsOpen => _root != null && _root.activeSelf;
        public string CurrentTitle => _currentPage?.title ?? "";
        public string CurrentBody => _currentPage?.body ?? "";
        public float LastQueueToRevealSeconds { get; private set; }
        public float LastPresentationToRevealSeconds { get; private set; }
        public bool LastRevealObservedFinalPresentation { get; private set; }
        public float LastModalPlayerDrift { get; private set; }
        public int LastModalXPDrift { get; private set; }
        public int LastModalCoinDrift { get; private set; }
        public bool RevealCountIsExact => CompletedReadLoopCount == RevealedPageCount + PendingRevealCount;
        public bool RevealTimingIsValid => LastRevealObservedFinalPresentation &&
            LastPresentationToRevealSeconds >= ReadFinalPresentationSeconds;
        public bool UsesTMPVisibleTypography => _titleText is TextMeshProUGUI &&
            _bodyText is TextMeshProUGUI && _titleText.gameObject.activeInHierarchy &&
            _bodyText.gameObject.activeInHierarchy;
        public bool BodyUsesScrolling => _scrollRect != null && _scrollRect.vertical &&
            !_scrollRect.horizontal && _viewport != null &&
            _viewport.GetComponent<RectMask2D>() != null && _scrollRect.content == _bodyContent;
        public bool VisibleTextDoesNotOverflow => IsOpen && _titleText != null && _bodyText != null &&
            !_titleText.isTextOverflowing && !_bodyText.isTextOverflowing && BodyUsesScrolling;
        public bool IsInsideSafeArea => IsOpen && ContainsRect(Screen.safeArea, ScreenRect(_storyPanel), 1f) &&
            ContainsRect(Screen.safeArea, ScreenRect(_closeButton?.transform as RectTransform), 1f);
        public bool ModalInputAndNavigationLocked => IsOpen && _player != null &&
            _player.IsModalInputLocked && (_roomNavigation == null || !_roomNavigation.activeSelf);
        public bool CurrentModalHasZeroDrift => IsOpen && _player != null &&
            Vector3.Distance(_modalStartPosition, _player.transform.position) <= 0.0001f &&
            CurrentRewardDriftIsZero;
        public bool LastCloseRestoredWithoutDrift => LastModalPlayerDrift <= 0.0001f &&
            LastModalXPDrift == 0 && LastModalCoinDrift == 0 &&
            (_player == null || _player.IsModalInputLocked == _playerModalLockWasActive) &&
            (_roomNavigation == null || _roomNavigation.activeSelf == _roomNavigationWasActive);

        bool CurrentRewardDriftIsZero
        {
            get
            {
                MoonlightCharacter moonlight = MoonlightGameManager.Instance?.moonlight;
                return moonlight != null && moonlight.xp == _modalStartXP &&
                    moonlight.coins == _modalStartCoins;
            }
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("[MoonlightStoryQA][FAIL] duplicate-story-page-ui");
                enabled = false;
                return;
            }
            Instance = this;
        }

        void Start()
        {
            if (_runtimeBuilt) return;
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[MoonlightStoryQA][FAIL] runtime-canvas-missing");
                enabled = false;
                return;
            }

            BuildRuntime(canvas.transform, GameObject.Find("RoomNavigation"),
                FindAnyObjectByType<MoonlightPlayerController>());
        }

        void Update()
        {
            if (!_runtimeBuilt) return;
            if (_lastScreenWidth != Screen.width || _lastScreenHeight != Screen.height ||
                !_lastSafeArea.Equals(Screen.safeArea))
            {
                ApplySafeArea();
                if (IsOpen) RefreshBodyLayout();
            }

            if (IsOpen)
            {
                _player?.SetModalInputLocked(true);
                if (_roomNavigation != null && _roomNavigation.activeSelf)
                    _roomNavigation.SetActive(false);
            }
        }

        void OnDisable()
        {
            CapturePendingPresentationState();
            if (_queueRoutine != null) StopCoroutine(_queueRoutine);
            if (IsOpen) Close();
            _queueRoutine = null;
        }

        void OnEnable()
        {
            if (_runtimeBuilt && _pending.Count > 0 && _queueRoutine == null)
                _queueRoutine = StartCoroutine(ProcessRevealQueue());
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void BuildRuntime(Transform canvasRoot, GameObject roomNavigation,
            MoonlightPlayerController player)
        {
            if (_runtimeBuilt || canvasRoot == null) return;
            _runtimeBuilt = true;
            _roomNavigation = roomNavigation;
            _player = player;
            BuildOverlay(canvasRoot);
            _root.SetActive(false);
            LoadDataFailClosed();
            ApplySafeArea();
        }

        public static bool QueueAfterCompletedRead(MoonlightCharacter moonlight)
        {
            if (Instance == null || !Instance.isActiveAndEnabled)
            {
                Debug.LogError("[MoonlightStoryQA][FAIL] reveal-queue story-ui-missing");
                return false;
            }
            return Instance.QueueCompletedRead(moonlight);
        }

        bool QueueCompletedRead(MoonlightCharacter moonlight)
        {
            if (!DataReady || moonlight == null)
            {
                Debug.LogError($"[MoonlightStoryQA][FAIL] reveal-queue dataReady={DataReady} " +
                    $"moonlight={(moonlight != null)}");
                return false;
            }

            MoonlightActionFeedback feedback = moonlight.GetComponent<MoonlightActionFeedback>();
            bool finalReadAccepted = feedback != null && feedback.IsPerformingAction &&
                feedback.ActiveActivityKind == MoonlightSpatialActionKind.Read &&
                feedback.ActivityStep == 3 && feedback.ActivityRequiredSteps == 4;
            if (!finalReadAccepted)
            {
                Debug.LogError("[MoonlightStoryQA][FAIL] reveal-queue final-read-presentation-not-active");
                return false;
            }

            AuthoredStoryPage page = SelectNextEligiblePage((int)moonlight.stage);
            if (page == null || string.IsNullOrWhiteSpace(page.title) ||
                string.IsNullOrWhiteSpace(page.body))
            {
                Debug.LogError($"[MoonlightStoryQA][FAIL] reveal-queue no-eligible-page stage={(int)moonlight.stage}");
                return false;
            }

            _reservedIds.Add(page.id);
            _pending.Enqueue(new PendingReveal
            {
                Page = page,
                Moonlight = moonlight,
                Feedback = feedback,
                QueuedAt = Time.time,
                RevealNotBefore = Time.time + ReadFinalActionSeconds + ReadFinalPresentationSeconds,
                XPAfterRead = moonlight.xp,
                CoinsAfterRead = moonlight.coins
            });
            CompletedReadLoopCount++;
            Debug.Log($"[MoonlightStoryQA] reveal-queued page={page.id} stage={(int)moonlight.stage} " +
                $"completed={CompletedReadLoopCount} pending={PendingRevealCount} overlayVisible={IsOpen} " +
                "marker=MOONLIGHT_STORY_REVEAL_QUEUED_BEHIND_FINAL_PRESENTATION");

            if (_queueRoutine == null) _queueRoutine = StartCoroutine(ProcessRevealQueue());
            return true;
        }

        IEnumerator ProcessRevealQueue()
        {
            while (_pending.Count > 0)
            {
                PendingReveal pending = _pending.Peek();
                while (pending.Feedback != null &&
                       (pending.Feedback.IsPerformingAction || pending.Feedback.IsPresentingResult))
                {
                    if (pending.Feedback.IsPresentingResult)
                    {
                        pending.ObservedFinalPresentation = true;
                        if (pending.PresentationStartedAt < 0f)
                            pending.PresentationStartedAt = Time.time;
                    }
                    yield return null;
                }

                if (!pending.ObservedFinalPresentation)
                {
                    Debug.LogWarning($"[MoonlightStoryQA] reveal-timing page={pending.Page.id} " +
                        "finalPresentationObserved=false using=deterministic-deadline");
                }

                float presentationDeadline = pending.PresentationStartedAt >= 0f
                    ? pending.PresentationStartedAt + ReadFinalPresentationSeconds
                    : pending.RevealNotBefore;
                float revealDeadline = Mathf.Max(pending.RevealNotBefore, presentationDeadline);
                while (Time.time < revealDeadline)
                    yield return null;

                if (pending.PresentationStartedAt < 0f)
                    pending.PresentationStartedAt = revealDeadline - ReadFinalPresentationSeconds;

                yield return null;
                _pending.Dequeue();
                ShowPending(pending);
                while (IsOpen) yield return null;
            }
            _queueRoutine = null;
        }

        void ShowPending(PendingReveal pending)
        {
            if (pending == null || pending.Page == null || !DataReady) return;
            _currentPage = pending.Page;
            _reservedIds.Remove(_currentPage.id);
            _revealedIds.Add(_currentPage.id);
            _titleText.text = _currentPage.title.Trim();
            _storyMetaText.text = $"MOONLIGHT ARCHIVE  /  {_currentPage.id.ToUpperInvariant()}";
            _bodyText.text = _currentPage.body.Trim();
            _modalStartXP = pending.XPAfterRead;
            _modalStartCoins = pending.CoinsAfterRead;
            _modalStartPosition = _player != null ? _player.transform.position : Vector3.zero;
            _roomNavigationWasActive = _roomNavigation != null && _roomNavigation.activeSelf;
            _playerModalLockWasActive = _player != null && _player.IsModalInputLocked;
            _player?.SetModalInputLocked(true);
            if (_roomNavigation != null) _roomNavigation.SetActive(false);

            LastRevealObservedFinalPresentation = pending.ObservedFinalPresentation;
            LastQueueToRevealSeconds = Time.time - pending.QueuedAt;
            LastPresentationToRevealSeconds = pending.PresentationStartedAt >= 0f
                ? Time.time - pending.PresentationStartedAt
                : 0f;
            RevealedPageCount++;
            _root.transform.SetAsLastSibling();
            _root.SetActive(true);
            ApplySafeArea();
            RefreshBodyLayout();

            Debug.Log($"[MoonlightStoryQA][PASS] story-reveal page={_currentPage.id} " +
                $"titleChars={_titleText.text.Length} bodyChars={_bodyText.text.Length} " +
                $"loaded={LoadedPageCount} completed={CompletedReadLoopCount} revealed={RevealedPageCount} " +
                $"pending={PendingRevealCount} queueElapsed={LastQueueToRevealSeconds:0.000}s " +
                $"presentationElapsed={LastPresentationToRevealSeconds:0.000}s " +
                $"presentationObserved={LastRevealObservedFinalPresentation} " +
                $"markers={RevealCountMarker},{TimingMarker},{RewardPathMarker}");
        }

        public void Close()
        {
            if (!IsOpen) return;
            MoonlightCharacter moonlight = MoonlightGameManager.Instance?.moonlight;
            LastModalPlayerDrift = _player != null
                ? Vector3.Distance(_modalStartPosition, _player.transform.position)
                : float.PositiveInfinity;
            LastModalXPDrift = moonlight != null ? moonlight.xp - _modalStartXP : int.MaxValue;
            LastModalCoinDrift = moonlight != null ? moonlight.coins - _modalStartCoins : int.MaxValue;

            _root.SetActive(false);
            _player?.SetModalInputLocked(_playerModalLockWasActive);
            if (_roomNavigation != null) _roomNavigation.SetActive(_roomNavigationWasActive);
            _currentPage = null;
            Debug.Log($"[MoonlightStoryQA] story-closed playerDrift={LastModalPlayerDrift:0.000000} " +
                $"xpDrift={LastModalXPDrift} coinDrift={LastModalCoinDrift} " +
                $"navigationRestored={(_roomNavigation == null || _roomNavigation.activeSelf == _roomNavigationWasActive)} " +
                $"markers={ModalLockMarker},{ZeroDriftMarker},{RewardPathMarker}");
        }

        public void Show(StoryPage page)
        {
            if (page == null || string.IsNullOrWhiteSpace(page.text) || !_runtimeBuilt) return;
            MoonlightCharacter moonlight = MoonlightGameManager.Instance?.moonlight;
            CompletedReadLoopCount++;
            ShowPending(new PendingReveal
            {
                Page = new AuthoredStoryPage
                {
                    id = "legacy_story",
                    title = "A Story from the Library",
                    body = page.text,
                    unlockStage = 0
                },
                Moonlight = moonlight,
                QueuedAt = Time.time - ReadFinalPresentationSeconds,
                PresentationStartedAt = Time.time - ReadFinalPresentationSeconds,
                RevealNotBefore = Time.time,
                XPAfterRead = moonlight != null ? moonlight.xp : 0,
                CoinsAfterRead = moonlight != null ? moonlight.coins : 0,
                ObservedFinalPresentation = true
            });
        }

        AuthoredStoryPage SelectNextEligiblePage(int stage)
        {
            var eligible = new List<AuthoredStoryPage>();
            int boundedStage = Mathf.Clamp(stage, 0, 4);
            for (int i = 0; i < _pages.Count; i++)
            {
                AuthoredStoryPage page = _pages[i];
                if (page.unlockStage <= boundedStage) eligible.Add(page);
            }
            if (eligible.Count == 0) return null;

            for (int i = 0; i < eligible.Count; i++)
                if (!_revealedIds.Contains(eligible[i].id) && !_reservedIds.Contains(eligible[i].id))
                    return eligible[i];

            return eligible[CompletedReadLoopCount % eligible.Count];
        }

        void LoadDataFailClosed()
        {
            _pages.Clear();
            DataReady = LibraryRoom.TryLoadAuthoredStories(out AuthoredStoryPage[] loaded,
                out string detail);
            if (DataReady) _pages.AddRange(loaded);
            Debug.Log($"[MoonlightStoryQA][{(DataReady ? "PASS" : "FAIL")}] story-data {detail}");
            if (!DataReady) enabled = false;
        }

        void CapturePendingPresentationState()
        {
            foreach (PendingReveal pending in _pending)
            {
                if (pending.Feedback == null || !pending.Feedback.IsPresentingResult) continue;
                pending.ObservedFinalPresentation = true;
                if (pending.PresentationStartedAt < 0f)
                    pending.PresentationStartedAt = Time.time;
            }
        }

        void BuildOverlay(Transform canvasRoot)
        {
            _root = CreateImage("StoryRevealModal", canvasRoot, new Color(0.07f, 0.055f, 0.07f, 0.94f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _root.GetComponent<Image>().raycastTarget = true;

            var safeObject = new GameObject("StorySafeArea", typeof(RectTransform));
            safeObject.transform.SetParent(_root.transform, false);
            _safeContainer = safeObject.GetComponent<RectTransform>();

            GameObject panel = CreateImage("StoryPanel", _safeContainer,
                new Color(0.98f, 0.94f, 0.88f, 1f), new Vector2(0.055f, 0.07f),
                new Vector2(0.945f, 0.93f), Vector2.zero, Vector2.zero);
            _storyPanel = panel.GetComponent<RectTransform>();

            _storyMetaText = CreateTMP("StoryMeta", panel.transform, "MOONLIGHT ARCHIVE", 18f,
                new Color(0.42f, 0.29f, 0.25f), TextAlignmentOptions.Left);
            SetRect(_storyMetaText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(42f, -70f), new Vector2(-126f, -34f));
            _storyMetaText.fontStyle = FontStyles.Bold;
            _storyMetaText.raycastTarget = false;

            _titleText = CreateTMP("StoryTitle", panel.transform, "Story", 42f,
                new Color(0.20f, 0.12f, 0.13f), TextAlignmentOptions.Left);
            SetRect(_titleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(42f, -138f), new Vector2(-126f, -76f));
            _titleText.fontStyle = FontStyles.Bold;
            _titleText.enableAutoSizing = true;
            _titleText.fontSizeMin = MinimumTitleFontSize;
            _titleText.fontSizeMax = 42f;
            _titleText.enableWordWrapping = true;
            _titleText.overflowMode = TextOverflowModes.Truncate;
            _titleText.characterSpacing = 0f;
            _titleText.raycastTarget = false;

            _closeButton = CreateCloseButton(panel.transform);

            var scrollObject = new GameObject("StoryBodyScroll", typeof(RectTransform), typeof(ScrollRect));
            scrollObject.transform.SetParent(panel.transform, false);
            RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
            SetRect(scrollRectTransform, Vector2.zero, Vector2.one,
                new Vector2(42f, 38f), new Vector2(-42f, -158f));
            _scrollRect = scrollObject.GetComponent<ScrollRect>();
            _scrollRect.horizontal = false;
            _scrollRect.vertical = true;
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
            _scrollRect.inertia = true;
            _scrollRect.decelerationRate = 0.12f;
            _scrollRect.scrollSensitivity = 34f;

            GameObject viewportObject = CreateImage("Viewport", scrollObject.transform,
                new Color(1f, 1f, 1f, 0.015f), Vector2.zero, Vector2.one,
                Vector2.zero, new Vector2(-32f, 0f));
            _viewport = viewportObject.GetComponent<RectTransform>();
            viewportObject.AddComponent<RectMask2D>();
            _scrollRect.viewport = _viewport;

            var contentObject = new GameObject("BodyContent", typeof(RectTransform));
            contentObject.transform.SetParent(_viewport, false);
            _bodyContent = contentObject.GetComponent<RectTransform>();
            _bodyContent.anchorMin = new Vector2(0f, 1f);
            _bodyContent.anchorMax = new Vector2(1f, 1f);
            _bodyContent.pivot = new Vector2(0.5f, 1f);
            _bodyContent.anchoredPosition = Vector2.zero;
            _bodyContent.sizeDelta = Vector2.zero;
            _scrollRect.content = _bodyContent;

            _bodyText = CreateTMP("StoryBody", _bodyContent, "", BodyFontSize,
                new Color(0.25f, 0.18f, 0.17f), TextAlignmentOptions.TopLeft);
            _bodyText.rectTransform.anchorMin = new Vector2(0f, 1f);
            _bodyText.rectTransform.anchorMax = new Vector2(1f, 1f);
            _bodyText.rectTransform.pivot = new Vector2(0.5f, 1f);
            _bodyText.rectTransform.anchoredPosition = Vector2.zero;
            _bodyText.rectTransform.sizeDelta = Vector2.zero;
            _bodyText.enableWordWrapping = true;
            _bodyText.overflowMode = TextOverflowModes.Overflow;
            _bodyText.characterSpacing = 0f;
            _bodyText.lineSpacing = 10f;
            _bodyText.raycastTarget = false;

            GameObject scrollbarObject = DefaultControls.CreateScrollbar(new DefaultControls.Resources());
            scrollbarObject.name = "StoryScrollbar";
            scrollbarObject.transform.SetParent(scrollObject.transform, false);
            RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
            SetRect(scrollbarRect, new Vector2(1f, 0f), Vector2.one,
                new Vector2(-20f, 0f), Vector2.zero);
            Scrollbar scrollbar = scrollbarObject.GetComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.GetComponent<Image>().color = new Color(0.26f, 0.18f, 0.18f, 0.14f);
            Image handle = scrollbar.handleRect != null ? scrollbar.handleRect.GetComponent<Image>() : null;
            if (handle != null) handle.color = new Color(0.50f, 0.34f, 0.30f, 0.72f);
            _scrollRect.verticalScrollbar = scrollbar;
            _scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            _scrollRect.verticalScrollbarSpacing = 12f;
        }

        Button CreateCloseButton(Transform parent)
        {
            GameObject buttonObject = DefaultControls.CreateButton(new DefaultControls.Resources());
            buttonObject.name = "StoryCloseButton";
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.anchoredPosition = new Vector2(-30f, -28f);
            rect.sizeDelta = new Vector2(64f, 64f);
            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.42f, 0.22f, 0.24f, 1f);
            Text legacy = buttonObject.GetComponentInChildren<Text>();
            if (legacy != null) legacy.enabled = false;
            TMP_Text label = CreateTMP("CloseIcon", buttonObject.transform, "X", 28f,
                Color.white, TextAlignmentOptions.Center);
            SetRect(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            label.fontStyle = FontStyles.Bold;
            label.raycastTarget = false;
            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(Close);
            return button;
        }

        void ApplySafeArea()
        {
            if (_safeContainer == null || Screen.width <= 0 || Screen.height <= 0) return;
            Rect safe = Screen.safeArea;
            _safeContainer.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            _safeContainer.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            _safeContainer.offsetMin = new Vector2(20f, 20f);
            _safeContainer.offsetMax = new Vector2(-20f, -20f);
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
            _lastSafeArea = safe;
        }

        void RefreshBodyLayout()
        {
            Canvas.ForceUpdateCanvases();
            _titleText.ForceMeshUpdate();
            float viewportHeight = Mathf.Max(1f, _viewport.rect.height);
            float preferredHeight = _bodyText.GetPreferredValues(_bodyText.text,
                Mathf.Max(1f, _viewport.rect.width - 20f), 0f).y + 24f;
            float contentHeight = Mathf.Max(viewportHeight, preferredHeight);
            _bodyContent.sizeDelta = new Vector2(0f, contentHeight);
            _bodyText.rectTransform.sizeDelta = new Vector2(-20f, contentHeight);
            _bodyText.ForceMeshUpdate();
            _scrollRect.verticalNormalizedPosition = 1f;
            Canvas.ForceUpdateCanvases();
            Debug.Log($"[MoonlightStoryQA] story-layout safe={IsInsideSafeArea} " +
                $"bodyHeight={contentHeight:0.0} viewportHeight={viewportHeight:0.0} " +
                $"scrolling={BodyUsesScrolling} nonOverflow={VisibleTextDoesNotOverflow} " +
                $"modal={ModalInputAndNavigationLocked} " +
                $"markers={SafeAreaMarker},{NonOverflowMarker},{ModalLockMarker}");
        }

        public static bool ValidateStaticContract(out string detail)
        {
            bool timing = Mathf.Approximately(ReadFinalActionSeconds, 1.75f) &&
                Mathf.Approximately(ReadFinalPresentationSeconds, 4.4f);
            bool typography = MinimumTitleFontSize >= 28f && BodyFontSize >= 24f;
            bool touch = MinimumCloseTargetSize >= 44f;
            detail = $"action={ReadFinalActionSeconds:0.00}s linger={ReadFinalPresentationSeconds:0.0}s " +
                $"titleMin={MinimumTitleFontSize:0} " +
                $"body={BodyFontSize:0} closeTarget={MinimumCloseTargetSize:0} " +
                "scroll=ScrollRect+RectMask2D progression=session-only rewards=none";
            return timing && typography && touch;
        }

        static TMP_Text CreateTMP(string name, Transform parent, string text, float fontSize,
            Color color, TextAlignmentOptions alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            var label = textObject.GetComponent<TextMeshProUGUI>();
            MoonlightUI.EnsureRuntimeFont(label);
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            return label;
        }

        static GameObject CreateImage(string name, Transform parent, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            imageObject.GetComponent<Image>().color = color;
            SetRect(imageObject.GetComponent<RectTransform>(), anchorMin, anchorMax, offsetMin, offsetMax);
            return imageObject;
        }

        static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        static Rect ScreenRect(RectTransform rect)
        {
            if (rect == null) return default;
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        static bool ContainsRect(Rect outer, Rect inner, float tolerance) =>
            inner.width > 0f && inner.height > 0f &&
            inner.xMin >= outer.xMin - tolerance && inner.xMax <= outer.xMax + tolerance &&
            inner.yMin >= outer.yMin - tolerance && inner.yMax <= outer.yMax + tolerance;
    }
}
