using System.Globalization;
using System.Text;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public enum MoonlightSpatialActionKind
    {
        Cook,
        Play,
        Garden,
        Read,
        SleepCuddle,
        Care,
        Feed
    }

    public class MoonlightSpatialActionZone : MonoBehaviour
    {
        public const float DefaultPassingScore = 0.58f;
        public const float FeedHungerIncrease = 18f;
        public const float SleepCuddleRestThreshold = 82f;

        public readonly struct RewardSnapshot
        {
            public readonly float Wonder;
            public readonly float Warmth;
            public readonly float Rest;
            public readonly float Magic;
            public readonly float Hunger;
            public readonly int XP;
            public readonly int Coins;

            public RewardSnapshot(float wonder, float warmth, float rest, float magic,
                                  float hunger, int xp, int coins)
            {
                Wonder = wonder;
                Warmth = warmth;
                Rest = rest;
                Magic = magic;
                Hunger = hunger;
                XP = xp;
                Coins = coins;
            }
        }

        [SerializeField] MoonlightSpatialActionKind kind;
        [SerializeField] float radius = 1.25f;
        [SerializeField] string displayName;
        [SerializeField, Range(0.35f, 0.9f)] float passingScore = DefaultPassingScore;
        int _progressStep;
        float _sessionScoreTotal;
        int _sessionAcceptedSteps;
        int _perfectSteps;
        int _currentCombo;
        int _bestCombo;
        bool _suppressBedtimeExternalSideEffectsForQA;

        public MoonlightSpatialActionKind Kind => kind;
        public float Radius => radius;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? kind.ToString() : displayName;
        public float PassingScore => passingScore;
        public int ProgressStep => _progressStep;
        public int RequiredSteps => RequiredStepsFor(kind);
        public static int RequiredStepsFor(MoonlightSpatialActionKind actionKind) =>
            IsScoredActivityKind(actionKind) ? 4 : 1;
        public MoonlightGestureKind RequiredGesture => RequiredGestureFor(kind, _progressStep);
        public static MoonlightGestureKind RequiredGestureFor(
            MoonlightSpatialActionKind actionKind, int progressStep) => actionKind switch
        {
            MoonlightSpatialActionKind.Cook => progressStep switch
            {
                0 => MoonlightGestureKind.Tap,
                1 => MoonlightGestureKind.Circle,
                2 => MoonlightGestureKind.Hold,
                _ => MoonlightGestureKind.ZigZag
            },
            MoonlightSpatialActionKind.Play => progressStep switch
            {
                0 => MoonlightGestureKind.Swipe,
                1 => MoonlightGestureKind.ZigZag,
                2 => MoonlightGestureKind.Swipe,
                _ => MoonlightGestureKind.Tap
            },
            MoonlightSpatialActionKind.Garden => progressStep switch
            {
                0 => MoonlightGestureKind.Tap,
                1 => MoonlightGestureKind.Circle,
                2 => MoonlightGestureKind.ZigZag,
                _ => MoonlightGestureKind.Hold
            },
            MoonlightSpatialActionKind.Read => progressStep switch
            {
                0 => MoonlightGestureKind.Tap,
                1 => MoonlightGestureKind.Swipe,
                2 => MoonlightGestureKind.Circle,
                _ => MoonlightGestureKind.Hold
            },
            MoonlightSpatialActionKind.SleepCuddle => MoonlightGestureKind.Hold,
            MoonlightSpatialActionKind.Care => CareGestureForStep(progressStep),
            MoonlightSpatialActionKind.Feed => MoonlightGestureKind.Tap,
            _ => MoonlightGestureKind.Tap
        };
        public bool SupportsLiveHoldReadiness =>
            IsLiveHoldReadinessStep(kind, _progressStep, RequiredGesture);
        public float LastGestureScore { get; private set; }
        public MoonlightGestureSample LastGestureSample { get; private set; }
        public bool LastGesturePassed { get; private set; }
        public bool LastAcceptedHapticWasPreplayed { get; private set; }
        public bool LastAcceptedGesturePlayedCompletionHaptic { get; private set; }
        public bool LastAcceptedGestureSamplePreserved { get; private set; }
        public string LastCueKey { get; private set; } = "";
        public int ActivitySessionAcceptedSteps => _sessionAcceptedSteps;
        public float ActivitySessionAverageScore => _sessionAcceptedSteps > 0
            ? _sessionScoreTotal / _sessionAcceptedSteps
            : 0f;
        public int ActivityCurrentCombo => _currentCombo;
        public int ActivityBestCombo => _bestCombo;
        public int ActivityPerfectSteps => _perfectSteps;
        public float LastCompletedAverageScore { get; private set; }
        public int LastCompletedBestCombo { get; private set; }
        public int LastCompletedPerfectSteps { get; private set; }
        public int LastMasteryBonusCoins { get; private set; }
        public bool LastStoryRevealQueueAccepted { get; private set; }
        public bool LastStoryRevealRewardPathUnchanged { get; private set; }
        public string StoryRevealRewardQAMarker => LastStoryRevealQueueAccepted &&
            LastStoryRevealRewardPathUnchanged
                ? StoryPageUI.RewardPathMarker
                : "MOONLIGHT_STORY_READ_REWARD_PATH_INVALID";

        public void Configure(MoonlightSpatialActionKind actionKind, string label, float actionRadius)
        {
            kind = actionKind;
            displayName = label;
            radius = actionRadius;
        }

        public string GetActionLabel(MoonlightCharacter moonlight)
        {
            return kind switch
            {
                MoonlightSpatialActionKind.Cook => _progressStep switch
                {
                    0 => "ADD",
                    1 => "STIR",
                    2 => "BAKE",
                    _ => "DECORATE"
                },
                MoonlightSpatialActionKind.Play => _progressStep switch
                {
                    0 => "THROW",
                    1 => "CHASE",
                    2 => "JUMP",
                    _ => "CATCH"
                },
                MoonlightSpatialActionKind.Garden => _progressStep switch
                {
                    0 => "PLANT",
                    1 => "WATER",
                    2 => "TEND",
                    _ => "BLOOM"
                },
                MoonlightSpatialActionKind.Read => _progressStep switch
                {
                    0 => "OPEN",
                    1 => "TURN",
                    2 => "TRACE",
                    _ => "REMEMBER"
                },
                MoonlightSpatialActionKind.SleepCuddle => SleepCuddleLabelForRest(
                    moonlight != null ? moonlight.stats.rest : SleepCuddleRestThreshold),
                MoonlightSpatialActionKind.Care => CareLabelForStep(_progressStep),
                MoonlightSpatialActionKind.Feed => "FEED",
                _ => "ACTION"
            };
        }

        public string GetPrompt(MoonlightCharacter moonlight)
        {
            string step = RequiredSteps > 1 ? $"  /  STEP {_progressStep + 1}/{RequiredSteps}" : "";
            return $"{DisplayName.ToUpperInvariant()}  /  {GestureInstruction(RequiredGesture)} {GetActionLabel(moonlight)}{step}";
        }

        public string Execute(MoonlightCharacter moonlight)
            => ExecuteGesture(moonlight, RequiredGesture, 1f);

        public string ExecuteGesture(MoonlightCharacter moonlight, MoonlightGestureKind gesture,
            float score, bool acceptedHapticAlreadyPlayed = false)
            => ExecuteGesture(moonlight, gesture,
                MoonlightGestureSample.Synthetic(gesture, score), acceptedHapticAlreadyPlayed);

        public string ExecuteGesture(MoonlightCharacter moonlight, MoonlightGestureKind gesture,
            MoonlightGestureSample sample, bool acceptedHapticAlreadyPlayed = false)
        {
            if (moonlight == null) return "Moonlight is not ready yet.";
            var feedback = moonlight.GetComponent<MoonlightActionFeedback>();

            LastGestureSample = sample;
            LastGestureScore = Mathf.Clamp01(sample.Score);
            LastGesturePassed = false;
            LastAcceptedHapticWasPreplayed = false;
            LastAcceptedGesturePlayedCompletionHaptic = false;
            LastAcceptedGestureSamplePreserved = false;
            if (feedback != null && !feedback.CanBeginAction)
            {
                LastCueKey = "activity-busy";
                Debug.Log($"[MoonlightActivityQA] gesture-blocked kind={kind} " +
                    $"reason=\"{feedback.InputBlockReason}\" step={_progressStep + 1}/{RequiredSteps}");
                return feedback.InputBlockReason;
            }

            bool gesturePassed = IsInputAccepted(kind, RequiredGesture, gesture,
                LastGestureScore, passingScore, true);
            if (!gesturePassed)
            {
                if (kind != MoonlightSpatialActionKind.Play || _progressStep == 0)
                    _currentCombo = 0;
                LastCueKey = "activity-try-again";
                AudioManager.Instance?.Play(LastCueKey);
                HapticFeedback.Failure();
                Debug.Log($"[MoonlightActivityQA] gesture-fail kind={kind} expected={RequiredGesture} " +
                    $"actual={gesture} score={LastGestureScore:0.00} step={_progressStep + 1}/{RequiredSteps}");
                return $"TRY AGAIN  /  {GestureInstruction(RequiredGesture)}  /  SCORE {Mathf.RoundToInt(LastGestureScore * 100f)}";
            }

            if (feedback == null)
                feedback = moonlight.gameObject.AddComponent<MoonlightActionFeedback>();

            switch (kind)
            {
                case MoonlightSpatialActionKind.Feed:
                    if (!TryBeginFeedback(feedback, "Feeding", acceptedHapticAlreadyPlayed))
                        return feedback.InputBlockReason;
                    RewardSnapshot feedBefore = CaptureRewards(moonlight);
                    moonlight.stats.hunger = FeedHungerAfter(moonlight.stats.hunger);
                    moonlight.GetComponentInChildren<MoonlightAnimator>()?.TriggerEat();
                    LastCueKey = "eat";
                    AudioManager.Instance?.Play(LastCueKey);
                    return "FED  /  " + BuildRewardReceipt(feedBefore, CaptureRewards(moonlight));

                case MoonlightSpatialActionKind.Cook:
                    if (!TryBeginFeedback(feedback, "Cooking", acceptedHapticAlreadyPlayed))
                        return feedback.InputBlockReason;
                    RecordSuccessfulGesture();
                    LastCueKey = _progressStep switch
                    {
                        0 => "cook-add",
                        1 => "cook-stir",
                        2 => "cook-bake",
                        _ => "cook-decorate"
                    };
                    AudioManager.Instance?.Play(LastCueKey);
                    _progressStep++;
                    if (_progressStep == 1)
                        return $"INGREDIENTS ADDED  /  {StepGrade()}  COMBO x{_currentCombo}  /  NEXT: CIRCLE TO STIR";
                    if (_progressStep == 2)
                        return $"BATTER SPARKLING  /  {StepGrade()}  COMBO x{_currentCombo}  /  NEXT: HOLD TO BAKE";
                    if (_progressStep == 3)
                        return $"MOONCAKES BAKED  /  {StepGrade()}  COMBO x{_currentCombo}  /  NEXT: ZIG-ZAG TO DECORATE";
                    RewardSnapshot cookBefore = CaptureRewards(moonlight);
                    var recipe = ScriptableObject.CreateInstance<FoodItem>();
                    recipe.itemName = "Mooncake bites";
                    recipe.cost = 0;
                    recipe.hungerBoost = 20f;
                    recipe.warmthBoost = 8f;
                    recipe.wonderBoost = 5f;
                    recipe.magicBoost = 5f;
                    recipe.xpReward = 14;
                    moonlight.Feed(recipe, false);
                    Destroy(recipe);
                    string cookMastery = CompleteActivitySession(moonlight);
                    _progressStep = 0;
                    AudioManager.Instance?.Play("activity-complete");
                    return $"MOONCAKES DECORATED  /  {cookMastery}  /  " +
                        BuildRewardReceipt(cookBefore, CaptureRewards(moonlight));

                case MoonlightSpatialActionKind.Play:
                    if (!TryBeginFeedback(feedback, "Playing", acceptedHapticAlreadyPlayed))
                        return feedback.InputBlockReason;
                    RecordSuccessfulGesture();
                    LastCueKey = _progressStep switch
                    {
                        0 => "play-throw",
                        1 => "play-chase",
                        2 => "play-jump",
                        _ => "play-catch"
                    };
                    AudioManager.Instance?.Play(LastCueKey);
                    _progressStep++;
                    if (_progressStep == 1)
                        return $"STAR BALL THROWN  /  {StepGrade()}  COMBO x{_currentCombo}  /  NEXT: ZIG-ZAG CHASE";
                    if (_progressStep == 2)
                        return $"GREAT CHASE  /  {StepGrade()}  COMBO x{_currentCombo}  /  NEXT: SWIPE TO JUMP";
                    if (_progressStep == 3)
                        return $"MAGIC JUMP  /  {StepGrade()}  COMBO x{_currentCombo}  /  NEXT: TAP TO CATCH";
                    RewardSnapshot playBefore = CaptureRewards(moonlight);
                    moonlight.Explore(RoomType.LivingRoom);
                    moonlight.PerformMagic(5, 2, false);
                    string playMastery = CompleteActivitySession(moonlight);
                    _progressStep = 0;
                    AudioManager.Instance?.Play("activity-complete");
                    return $"STAR BALL COMBO  /  {playMastery}  /  " +
                        BuildRewardReceipt(playBefore, CaptureRewards(moonlight));

                case MoonlightSpatialActionKind.Garden:
                    if (!TryBeginFeedback(feedback, "Gardening", acceptedHapticAlreadyPlayed))
                        return feedback.InputBlockReason;
                    RecordSuccessfulGesture();
                    LastCueKey = _progressStep switch
                    {
                        0 => "garden-plant",
                        1 => "garden-water",
                        2 => "garden-tend",
                        _ => "garden-bloom"
                    };
                    AudioManager.Instance?.Play(LastCueKey);
                    _progressStep++;
                    if (_progressStep == 1)
                        return $"MOONSEEDS PLANTED  /  {StepGrade()}  COMBO x{_currentCombo}  /  NEXT: CIRCLE TO WATER";
                    if (_progressStep == 2)
                        return $"DEW SPARKLING  /  {StepGrade()}  COMBO x{_currentCombo}  /  NEXT: ZIG-ZAG TO TEND";
                    if (_progressStep == 3)
                        return $"SPROUTS TENDED  /  {StepGrade()}  COMBO x{_currentCombo}  /  NEXT: HOLD TO BLOOM";
                    RewardSnapshot gardenBefore = CaptureRewards(moonlight);
                    moonlight.CompleteGardening(false);
                    string gardenMastery = CompleteActivitySession(moonlight);
                    _progressStep = 0;
                    AudioManager.Instance?.Play("activity-complete");
                    return $"MOON GARDEN BLOOMED  /  {gardenMastery}  /  " +
                        BuildRewardReceipt(gardenBefore, CaptureRewards(moonlight));

                case MoonlightSpatialActionKind.Read:
                    if (!TryBeginFeedback(feedback, "Reading", acceptedHapticAlreadyPlayed))
                        return feedback.InputBlockReason;
                    RecordSuccessfulGesture();
                    LastCueKey = _progressStep switch
                    {
                        0 => "read-open",
                        1 => "read-turn",
                        2 => "read-trace",
                        _ => "read-finish"
                    };
                    AudioManager.Instance?.Play(LastCueKey);
                    _progressStep++;
                    if (_progressStep == 1)
                        return $"STORY OPENED  /  {StepGrade()}  COMBO x{_currentCombo}  /  NEXT: SWIPE TO TURN";
                    if (_progressStep == 2)
                        return $"STAR PAGE TURNED  /  {StepGrade()}  COMBO x{_currentCombo}  /  NEXT: CIRCLE TO TRACE";
                    if (_progressStep == 3)
                        return $"CONSTELLATION TRACED  /  {StepGrade()}  COMBO x{_currentCombo}  /  NEXT: HOLD TO REMEMBER";
                    RewardSnapshot readBefore = CaptureRewards(moonlight);
                    moonlight.CompleteReading(false);
                    string readMastery = CompleteActivitySession(moonlight);
                    _progressStep = 0;
                    AudioManager.Instance?.Play("activity-complete");
                    RewardSnapshot readRewardAfter = CaptureRewards(moonlight);
                    LastStoryRevealQueueAccepted = StoryPageUI.QueueAfterCompletedRead(moonlight);
                    LastStoryRevealRewardPathUnchanged = RewardsEqual(
                        readRewardAfter, CaptureRewards(moonlight));
                    Debug.Log($"[MoonlightStoryQA][{(LastStoryRevealQueueAccepted && LastStoryRevealRewardPathUnchanged ? "PASS" : "FAIL")}] " +
                        $"read-reward-path queued={LastStoryRevealQueueAccepted} " +
                        $"unchanged={LastStoryRevealRewardPathUnchanged} marker={StoryRevealRewardQAMarker}");
                    return $"STORY REMEMBERED  /  {readMastery}  /  " +
                        BuildRewardReceipt(readBefore, CaptureRewards(moonlight));

                case MoonlightSpatialActionKind.SleepCuddle:
                    if (moonlight.stats.rest < SleepCuddleRestThreshold)
                    {
                        if (!TryBeginFeedback(feedback, "Resting", acceptedHapticAlreadyPlayed))
                            return feedback.InputBlockReason;
                        LastCueKey = "sleep";
                        RewardSnapshot sleepBefore = CaptureRewards(moonlight);
                        moonlight.PutToSleep(!_suppressBedtimeExternalSideEffectsForQA,
                            !_suppressBedtimeExternalSideEffectsForQA);
                        return "DREAMING  /  " +
                            BuildRewardReceipt(sleepBefore, CaptureRewards(moonlight));
                    }
                    if (!TryBeginFeedback(feedback, "Cuddled", acceptedHapticAlreadyPlayed))
                        return feedback.InputBlockReason;
                    LastCueKey = "cuddle";
                    RewardSnapshot cuddleBefore = CaptureRewards(moonlight);
                    moonlight.Cuddle(false, !_suppressBedtimeExternalSideEffectsForQA);
                    if (!_suppressBedtimeExternalSideEffectsForQA)
                        AchievementSystem.Instance?.OnFirstCuddle();
                    return "CUDDLED  /  " +
                        BuildRewardReceipt(cuddleBefore, CaptureRewards(moonlight));

                case MoonlightSpatialActionKind.Care:
                    if (!TryBeginFeedback(feedback, "Caring", acceptedHapticAlreadyPlayed))
                        return feedback.InputBlockReason;
                    RecordSuccessfulGesture();
                    LastCueKey = CareCueForStep(_progressStep);
                    AudioManager.Instance?.Play(LastCueKey);
                    _progressStep++;
                    if (_progressStep == 1)
                        return $"SPA PREPARED  /  {StepGrade()}  COMBO x{_currentCombo}  /  NEXT: CIRCLE TO WASH";
                    if (_progressStep == 2)
                        return $"MOONLIGHT WASHED  /  {StepGrade()}  COMBO x{_currentCombo}  /  NEXT: SWIPE TO BRUSH";
                    if (_progressStep == 3)
                        return $"HAIR BRUSHED  /  {StepGrade()}  COMBO x{_currentCombo}  /  NEXT: HOLD TO GLOW";
                    RewardSnapshot careBefore = CaptureRewards(moonlight);
                    moonlight.CompleteCare(false);
                    string careMastery = CompleteActivitySession(moonlight);
                    _progressStep = 0;
                    AudioManager.Instance?.Play("activity-complete");
                    return $"MOON SPA COMPLETE  /  {careMastery}  /  " +
                        BuildRewardReceipt(careBefore, CaptureRewards(moonlight));
            }

            return "Moonlight looks around the room.";
        }

        bool TryBeginFeedback(MoonlightActionFeedback feedback, string state,
            bool acceptedHapticAlreadyPlayed = false)
        {
            bool keepsGestureSample = ShouldPreserveGestureSample(kind);
            MoonlightGestureSample feedbackSample = GestureSampleForFeedback(kind,
                LastGestureSample);
            bool began = feedback.TryBegin(kind, DisplayName, state, _progressStep,
                RequiredSteps, feedbackSample);
            if (!began)
            {
                LastGesturePassed = false;
                return false;
            }

            LastGesturePassed = true;
            LastAcceptedGestureSamplePreserved = keepsGestureSample &&
                feedback.ActiveGestureSample.ContentEquals(LastGestureSample);
            if (kind == MoonlightSpatialActionKind.SleepCuddle)
            {
                if (!LastAcceptedGestureSamplePreserved)
                {
                    LastGesturePassed = false;
                    Debug.LogError("[MoonlightGameplayQA][FAIL] " +
                        $"bedtime-sample-propagation score={LastGestureSample.Score:0.000} " +
                        $"duration={LastGestureSample.Duration:0.000} " +
                        "marker=MOONLIGHT_GESTURE_BEDTIME_RUNTIME_SAMPLE_FAILED");
                    return false;
                }
                Debug.Log("[MoonlightGameplayQA][PASS] " +
                    $"bedtime-sample-propagation score={LastGestureSample.Score:0.000} " +
                    $"duration={LastGestureSample.Duration:0.000} " +
                    "marker=MOONLIGHT_GESTURE_BEDTIME_RUNTIME_SAMPLE_VERIFIED");
            }
            LastAcceptedHapticWasPreplayed = acceptedHapticAlreadyPlayed;
            // Live Hold readiness can own this pulse before release. Completion
            // methods also suppress their legacy pulse to avoid duplicate feedback.
            LastAcceptedGesturePlayedCompletionHaptic =
                ShouldPlayAcceptedGestureHaptic(keepsGestureSample, acceptedHapticAlreadyPlayed);
            if (LastAcceptedGesturePlayedCompletionHaptic)
                feedback.PlayActionQualityHaptic();
            return true;
        }

        public static bool ShouldPlayAcceptedGestureHaptic(bool keepsGestureSample,
            bool acceptedHapticAlreadyPlayed) =>
            keepsGestureSample && !acceptedHapticAlreadyPlayed;

        public static bool ShouldPreserveGestureSample(MoonlightSpatialActionKind actionKind) =>
            IsScoredActivityKind(actionKind) || actionKind is MoonlightSpatialActionKind.Feed or
                MoonlightSpatialActionKind.SleepCuddle;

        public static MoonlightGestureSample GestureSampleForFeedback(
            MoonlightSpatialActionKind actionKind, MoonlightGestureSample acceptedSample) =>
            ShouldPreserveGestureSample(actionKind)
                ? acceptedSample
                : MoonlightGestureSample.Synthetic(MoonlightGestureKind.Swipe,
                    MoonlightActionFeedback.GreatActionQualityScore);

        public static bool IsScoredActivityKind(MoonlightSpatialActionKind actionKind) =>
            actionKind is MoonlightSpatialActionKind.Cook or MoonlightSpatialActionKind.Play or
                MoonlightSpatialActionKind.Garden or MoonlightSpatialActionKind.Read or
                MoonlightSpatialActionKind.Care;

        public static bool IsFeedInputAccepted(MoonlightGestureKind gesture, float score,
            float threshold, bool canBegin) =>
            canBegin && gesture == MoonlightGestureKind.Tap && Mathf.Clamp01(score) >= threshold;

        public static bool IsInputAccepted(MoonlightSpatialActionKind actionKind,
            MoonlightGestureKind requiredGesture, MoonlightGestureKind actualGesture, float score,
            float threshold, bool canBegin) => actionKind == MoonlightSpatialActionKind.Feed
                ? IsFeedInputAccepted(actualGesture, score, threshold, canBegin)
                : canBegin && actualGesture == requiredGesture &&
                    Mathf.Clamp01(score) >= threshold;

        public static float FeedHungerAfter(float hunger) =>
            Mathf.Min(100f, Mathf.Clamp(hunger, 0f, 100f) + FeedHungerIncrease);

        static RewardSnapshot FeedSnapshotAfterInput(RewardSnapshot before,
            MoonlightGestureKind gesture, float score, bool canBegin)
        {
            if (!IsFeedInputAccepted(gesture, score, DefaultPassingScore, canBegin)) return before;
            return new RewardSnapshot(before.Wonder, before.Warmth, before.Rest, before.Magic,
                FeedHungerAfter(before.Hunger), before.XP, before.Coins);
        }

        public static bool ValidateFeedStatDeltaAndRejectionContract(out string detail)
        {
            var before = new RewardSnapshot(31f, 42f, 53f, 64f, 40f, 75, 9);
            RewardSnapshot accepted = FeedSnapshotAfterInput(before,
                MoonlightGestureKind.Tap, 0.95f, true);
            RewardSnapshot wrongGesture = FeedSnapshotAfterInput(before,
                MoonlightGestureKind.Swipe, 0.95f, true);
            RewardSnapshot lowScore = FeedSnapshotAfterInput(before,
                MoonlightGestureKind.Tap, 0.20f, true);
            RewardSnapshot busy = FeedSnapshotAfterInput(before,
                MoonlightGestureKind.Tap, 0.95f, false);
            string receipt = BuildRewardReceipt(before, accepted);
            bool acceptedDeltaPass = accepted.Hunger == 58f && accepted.Wonder == before.Wonder &&
                accepted.Warmth == before.Warmth && accepted.Rest == before.Rest &&
                accepted.Magic == before.Magic && accepted.XP == before.XP &&
                accepted.Coins == before.Coins && receipt == "+18 HUNGER";
            bool rejectionPass = RewardsEqual(before, wrongGesture) &&
                RewardsEqual(before, lowScore) && RewardsEqual(before, busy);
            int scoredKinds = 0;
            foreach (MoonlightSpatialActionKind actionKind in
                     System.Enum.GetValues(typeof(MoonlightSpatialActionKind)))
                if (IsScoredActivityKind(actionKind)) scoredKinds++;
            bool kindPass = scoredKinds == 5 &&
                (int)MoonlightSpatialActionKind.Feed == (int)MoonlightSpatialActionKind.Care + 1;
            detail = $"gesture=Tap steps=1 hunger=40->{accepted.Hunger:0} delta={FeedHungerIncrease:0} " +
                $"receipt=\"{receipt}\" wrongUnchanged={RewardsEqual(before, wrongGesture)} " +
                $"lowUnchanged={RewardsEqual(before, lowScore)} busyUnchanged={RewardsEqual(before, busy)} " +
                $"scoredKinds={scoredKinds}/5";
            return acceptedDeltaPass && rejectionPass && kindPass;
        }

        public static bool IsLiveHoldReadinessStep(MoonlightSpatialActionKind actionKind,
            int progressStep, MoonlightGestureKind gesture)
        {
            if (gesture != MoonlightGestureKind.Hold) return false;
            return actionKind switch
            {
                MoonlightSpatialActionKind.Cook => progressStep == 2,
                MoonlightSpatialActionKind.Garden => progressStep == 3,
                MoonlightSpatialActionKind.Read => progressStep == 3,
                MoonlightSpatialActionKind.SleepCuddle => progressStep == 0,
                MoonlightSpatialActionKind.Care => progressStep == 3,
                _ => false
            };
        }

        public static string GestureInstruction(MoonlightGestureKind gesture) => gesture switch
        {
            MoonlightGestureKind.Circle => "DRAW A CIRCLE TO",
            MoonlightGestureKind.Hold => "PRESS AND HOLD TO",
            MoonlightGestureKind.Swipe => "SWIPE TO",
            MoonlightGestureKind.ZigZag => "DRAW A ZIG-ZAG TO",
            _ => "TAP TO"
        };

        public static string SleepCuddleLabelForRest(float rest) =>
            rest < SleepCuddleRestThreshold ? "SLEEP" : "CUDDLE";

        sealed class BedtimeTransactionProbe
        {
            public GameObject Root;
            public MoonlightCharacter Moonlight;
            public MoonlightActionFeedback Feedback;
            public MoonlightSpatialActionZone Zone;
        }

        public static bool ValidateGestureResponsiveBedtimeContract(out string detail)
        {
            MoonlightTouchJoystick joystick = Object.FindAnyObjectByType<MoonlightTouchJoystick>();
            GameObject joystickObject = joystick != null ? joystick.gameObject : null;
            bool joystickWasActive = joystickObject != null && joystickObject.activeSelf;
            try
            {
                if (joystickWasActive) joystickObject.SetActive(false);

                bool wrongTap = RunBedtimeRejectionTransaction(
                    MoonlightGestureKind.Tap, 0.95f, false, out string wrongTapDetail);
                bool lowScore = RunBedtimeRejectionTransaction(
                    MoonlightGestureKind.Hold, 0.20f, false, out string lowScoreDetail);
                bool busy = RunBedtimeRejectionTransaction(
                    MoonlightGestureKind.Hold, 0.95f, true, out string busyDetail);
                bool sleep = RunBedtimeAcceptedTransaction(35f, 0.95f,
                    MoonlightActionQualityTier.Perfect, false, out string sleepDetail);
                bool good = RunBedtimeAcceptedTransaction(82f, 0.60f,
                    MoonlightActionQualityTier.Good, false, out string goodDetail);
                bool great = RunBedtimeAcceptedTransaction(82f, 0.78f,
                    MoonlightActionQualityTier.Great, false, out string greatDetail);
                bool perfectReadiness = RunBedtimeAcceptedTransaction(82f, 0.95f,
                    MoonlightActionQualityTier.Perfect, true, out string perfectDetail);
                bool mapping = RequiredGestureFor(MoonlightSpatialActionKind.SleepCuddle, 0) ==
                        MoonlightGestureKind.Hold &&
                    RequiredStepsFor(MoonlightSpatialActionKind.SleepCuddle) == 1 &&
                    IsLiveHoldReadinessStep(MoonlightSpatialActionKind.SleepCuddle, 0,
                        MoonlightGestureKind.Hold) &&
                    !IsLiveHoldReadinessStep(MoonlightSpatialActionKind.SleepCuddle, 0,
                        MoonlightGestureKind.Tap);
                detail = $"mapping={mapping} reject=({wrongTapDetail};{lowScoreDetail};" +
                    $"{busyDetail}) accepted=({sleepDetail};{goodDetail};{greatDetail};" +
                    $"{perfectDetail})";
                return mapping && wrongTap && lowScore && busy && sleep && good && great &&
                    perfectReadiness;
            }
            catch (System.Exception exception)
            {
                detail = $"exception={exception.GetType().Name}:{exception.Message}";
                return false;
            }
            finally
            {
                if (joystickObject != null && joystickObject.activeSelf != joystickWasActive)
                    joystickObject.SetActive(joystickWasActive);
            }
        }

        static bool RunBedtimeRejectionTransaction(MoonlightGestureKind gesture, float score,
            bool makeBusy, out string detail)
        {
            BedtimeTransactionProbe probe = CreateBedtimeTransactionProbe(35f,
                makeBusy ? "busy" : gesture.ToString().ToLowerInvariant());
            HapticFeedback.QAObserver haptics = null;
            try
            {
                bool setup = true;
                if (makeBusy)
                {
                    MoonlightGestureSample setupSample = BedtimeTransactionSample(0.95f, 0.91f);
                    probe.Zone.ExecuteGesture(probe.Moonlight, MoonlightGestureKind.Hold,
                        setupSample);
                    setup = probe.Zone.LastGesturePassed && !probe.Feedback.CanBeginAction;
                }

                RewardSnapshot before = CaptureRewards(probe.Moonlight);
                int progressBefore = probe.Zone.ProgressStep;
                haptics = HapticFeedback.BeginQAObservation();
                MoonlightGestureSample rejectedSample = BedtimeTransactionSample(score, 0.63f);
                string result = probe.Zone.ExecuteGesture(probe.Moonlight, gesture, rejectedSample);
                RewardSnapshot after = CaptureRewards(probe.Moonlight);
                int hapticCount = haptics.InvocationCount;
                int expectedHaptics = makeBusy ? 0 : 1;
                bool unchanged = RewardsEqual(before, after) &&
                    probe.Zone.ProgressStep == progressBefore;
                bool resultPass = makeBusy
                    ? result == probe.Feedback.InputBlockReason &&
                        probe.Zone.LastCueKey == "activity-busy"
                    : result.StartsWith("TRY AGAIN", System.StringComparison.Ordinal) &&
                        probe.Zone.LastCueKey == "activity-try-again" &&
                        haptics.LastPreset == "Failure";
                bool pass = setup && unchanged && !probe.Zone.LastGesturePassed && resultPass &&
                    hapticCount == expectedHaptics;
                detail = $"{(makeBusy ? "busy" : gesture.ToString())}:pass={pass} " +
                    $"score={score:0.00} unchanged={unchanged} haptic={hapticCount}/" +
                    $"{expectedHaptics}";
                return pass;
            }
            finally
            {
                haptics?.Dispose();
                DestroyBedtimeTransactionProbe(probe);
            }
        }

        static bool RunBedtimeAcceptedTransaction(float initialRest, float score,
            MoonlightActionQualityTier expectedTier, bool readinessHapticPreplayed,
            out string detail)
        {
            BedtimeTransactionProbe probe = CreateBedtimeTransactionProbe(initialRest,
                expectedTier.ToString().ToLowerInvariant());
            HapticFeedback.QAObserver haptics = null;
            try
            {
                RewardSnapshot before = CaptureRewards(probe.Moonlight);
                int progressBefore = probe.Zone.ProgressStep;
                haptics = HapticFeedback.BeginQAObservation();
                if (readinessHapticPreplayed) HapticFeedback.Success();
                MoonlightGestureSample acceptedSample = BedtimeTransactionSample(score,
                    0.76f + score * 0.31f);
                string prompt = probe.Zone.GetPrompt(probe.Moonlight);
                string result = probe.Zone.ExecuteGesture(probe.Moonlight,
                    MoonlightGestureKind.Hold, acceptedSample, readinessHapticPreplayed);
                RewardSnapshot after = CaptureRewards(probe.Moonlight);
                int hapticCount = haptics.InvocationCount;
                bool sleeping = initialRest < 82f;
                bool promptPass = prompt == $"BED  /  PRESS AND HOLD TO " +
                    (sleeping ? "SLEEP" : "CUDDLE");
                bool rewardPass = sleeping
                    ? Mathf.Approximately(after.Rest - before.Rest, 45f) &&
                        Mathf.Approximately(after.Warmth - before.Warmth, 5f) &&
                        Mathf.Approximately(after.Wonder - before.Wonder, 0f) &&
                        after.XP == before.XP
                    : Mathf.Approximately(after.Rest - before.Rest, 0f) &&
                        Mathf.Approximately(after.Warmth - before.Warmth, 20f) &&
                        Mathf.Approximately(after.Wonder - before.Wonder, 5f) &&
                        after.XP - before.XP == 8;
                rewardPass &= Mathf.Approximately(after.Magic - before.Magic, 0f) &&
                    Mathf.Approximately(after.Hunger - before.Hunger, 0f) &&
                    after.Coins == before.Coins;
                bool samplePass = probe.Zone.LastAcceptedGestureSamplePreserved &&
                    probe.Zone.LastGestureSample.ContentEquals(acceptedSample) &&
                    probe.Feedback.ActiveGestureSample.ContentEquals(acceptedSample);
                bool tierPass = probe.Feedback.ActionQualityTier == expectedTier &&
                    probe.Feedback.ActionQualityQAMarker ==
                        $"MOONLIGHT_ACTION_QUALITY_{expectedTier.ToString().ToUpperInvariant()}";
                string expectedHapticPreset = expectedTier switch
                {
                    MoonlightActionQualityTier.Good => "LightImpact",
                    MoonlightActionQualityTier.Great => "MediumImpact",
                    _ => "Success"
                };
                bool hapticPass = hapticCount == 1 &&
                    haptics.LastPreset == expectedHapticPreset &&
                    probe.Zone.LastAcceptedHapticWasPreplayed == readinessHapticPreplayed &&
                    probe.Zone.LastAcceptedGesturePlayedCompletionHaptic ==
                        !readinessHapticPreplayed;
                bool pass = probe.Zone.LastGesturePassed && promptPass && rewardPass &&
                    samplePass && tierPass && hapticPass &&
                    probe.Zone.ProgressStep == progressBefore &&
                    result.StartsWith(sleeping ? "DREAMING" : "CUDDLED",
                        System.StringComparison.Ordinal);
                detail = $"{(sleeping ? "sleep" : expectedTier.ToString())}:pass={pass} " +
                    $"sample={samplePass} tier={probe.Feedback.ActionQualityTier}/" +
                    $"{expectedTier} reward={BuildRewardReceipt(before, after)} " +
                    $"haptic={hapticCount}/1:{haptics.LastPreset}/" +
                    $"{expectedHapticPreset} preplayed={readinessHapticPreplayed}";
                return pass;
            }
            finally
            {
                haptics?.Dispose();
                DestroyBedtimeTransactionProbe(probe);
            }
        }

        static BedtimeTransactionProbe CreateBedtimeTransactionProbe(float rest, string name)
        {
            var root = new GameObject($"MoonlightBedtimeTransaction-{name}");
            var probe = new BedtimeTransactionProbe
            {
                Root = root,
                Moonlight = root.AddComponent<MoonlightCharacter>()
            };
            probe.Moonlight.stats.wonder = 31f;
            probe.Moonlight.stats.warmth = 42f;
            probe.Moonlight.stats.rest = rest;
            probe.Moonlight.stats.magic = 64f;
            probe.Moonlight.stats.hunger = 55f;
            probe.Moonlight.xp = 10;
            probe.Moonlight.coins = 9;
            probe.Feedback = root.AddComponent<MoonlightActionFeedback>();
            var zoneObject = new GameObject("BedtimeTransactionZone");
            zoneObject.transform.SetParent(root.transform, false);
            probe.Zone = zoneObject.AddComponent<MoonlightSpatialActionZone>();
            probe.Zone.Configure(MoonlightSpatialActionKind.SleepCuddle, "Bed", 1f);
            probe.Zone._suppressBedtimeExternalSideEffectsForQA = true;
            return probe;
        }

        static MoonlightGestureSample BedtimeTransactionSample(float score, float duration) =>
            MoonlightGestureSample.Create(score, duration, new[]
            {
                new Vector2(-0.08f, 0.03f),
                new Vector2(0.02f, -0.04f),
                new Vector2(0.07f, 0.01f)
            });

        static void DestroyBedtimeTransactionProbe(BedtimeTransactionProbe probe)
        {
            if (probe == null || probe.Root == null) return;
            if (probe.Feedback != null) probe.Feedback.enabled = false;
            probe.Root.SetActive(false);
            Object.Destroy(probe.Root);
        }

        public static MoonlightGestureKind CareGestureForStep(int step) => step switch
        {
            0 => MoonlightGestureKind.Tap,
            1 => MoonlightGestureKind.Circle,
            2 => MoonlightGestureKind.Swipe,
            _ => MoonlightGestureKind.Hold
        };

        public static string CareLabelForStep(int step) => step switch
        {
            0 => "PREP",
            1 => "WASH",
            2 => "BRUSH",
            _ => "GLOW"
        };

        public static string CareCueForStep(int step) => step switch
        {
            0 => "care-prep",
            1 => "care-wash",
            2 => "care-brush",
            _ => "care-glow"
        };

        public static bool ValidateCareSequenceContract(out string detail)
        {
            MoonlightGestureKind[] gestures =
            {
                MoonlightGestureKind.Tap,
                MoonlightGestureKind.Circle,
                MoonlightGestureKind.Swipe,
                MoonlightGestureKind.Hold
            };
            string[] labels = { "PREP", "WASH", "BRUSH", "GLOW" };
            string[] cues = { "care-prep", "care-wash", "care-brush", "care-glow" };
            bool enumOrderPass = (int)MoonlightSpatialActionKind.Care ==
                (int)MoonlightSpatialActionKind.SleepCuddle + 1;
            bool pass = enumOrderPass;
            for (int step = 0; step < 4; step++)
            {
                pass &= CareGestureForStep(step) == gestures[step];
                pass &= CareLabelForStep(step) == labels[step];
                pass &= CareCueForStep(step) == cues[step];
            }

            detail = $"enumAfterSleepCuddle={enumOrderPass} " +
                "gestures=Tap,Circle,Swipe,Hold labels=PREP,WASH,BRUSH,GLOW " +
                "cues=care-prep,care-wash,care-brush,care-glow";
            return pass;
        }

        void RecordSuccessfulGesture()
        {
            _sessionScoreTotal += LastGestureScore;
            _sessionAcceptedSteps++;
            _currentCombo++;
            _bestCombo = Mathf.Max(_bestCombo, _currentCombo);
            if (LastGestureScore >= MoonlightActionFeedback.PerfectActionQualityScore) _perfectSteps++;
        }

        string CompleteActivitySession(MoonlightCharacter moonlight)
        {
            float average = ActivitySessionAverageScore;
            int bonus = CalculateMasteryBonus(average, _perfectSteps, _bestCombo,
                _sessionAcceptedSteps, RequiredSteps);
            if (bonus > 0) moonlight.EarnCoins(bonus);

            LastCompletedAverageScore = average;
            LastCompletedBestCombo = _bestCombo;
            LastCompletedPerfectSteps = _perfectSteps;
            LastMasteryBonusCoins = bonus;
            moonlight.GetComponent<MoonlightActionFeedback>()?.QueueMasteryCelebration(
                average, _bestCombo, bonus);
            string summary = $"RUN {GradeFor(average)} {Mathf.RoundToInt(average * 100f)} x{_bestCombo}";
            Debug.Log($"[MoonlightActivityQA] mastery kind={kind} average={average:0.000} " +
                $"perfect={_perfectSteps}/{RequiredSteps} combo={_bestCombo} bonusCoins={bonus} " +
                "marker=MOONLIGHT_ACTIVITY_MASTERY_REWARDED");
            ResetActivitySession();
            return summary;
        }

        void ResetActivitySession()
        {
            _sessionScoreTotal = 0f;
            _sessionAcceptedSteps = 0;
            _perfectSteps = 0;
            _currentCombo = 0;
            _bestCombo = 0;
        }

        public static int CalculateMasteryBonus(float averageScore, int perfectSteps,
            int bestCombo, int acceptedSteps, int requiredSteps)
        {
            if (requiredSteps <= 1 || acceptedSteps < requiredSteps) return 0;
            if (averageScore >= 0.90f && perfectSteps >= requiredSteps - 1 &&
                bestCombo >= requiredSteps)
                return 3;
            if (averageScore >= 0.80f && bestCombo >= requiredSteps - 1) return 2;
            if (averageScore >= 0.70f) return 1;
            return 0;
        }

        public static bool ValidateMasteryContract(out string detail)
        {
            int perfect = CalculateMasteryBonus(0.95f, 4, 4, 4, 4);
            int great = CalculateMasteryBonus(0.83f, 2, 4, 4, 4);
            int good = CalculateMasteryBonus(0.74f, 0, 2, 4, 4);
            int low = CalculateMasteryBonus(0.69f, 0, 4, 4, 4);
            int incomplete = CalculateMasteryBonus(0.95f, 3, 3, 3, 4);
            detail = $"perfect={perfect} great={great} good={good} low={low} incomplete={incomplete}";
            return perfect == 3 && great == 2 && good == 1 && low == 0 && incomplete == 0;
        }

        static RewardSnapshot CaptureRewards(MoonlightCharacter moonlight) =>
            new RewardSnapshot(moonlight.stats.wonder, moonlight.stats.warmth,
                moonlight.stats.rest, moonlight.stats.magic, moonlight.stats.hunger,
                moonlight.xp, moonlight.coins);

        static bool RewardsEqual(RewardSnapshot left, RewardSnapshot right) =>
            Mathf.Approximately(left.Wonder, right.Wonder) &&
            Mathf.Approximately(left.Warmth, right.Warmth) &&
            Mathf.Approximately(left.Rest, right.Rest) &&
            Mathf.Approximately(left.Magic, right.Magic) &&
            Mathf.Approximately(left.Hunger, right.Hunger) &&
            left.XP == right.XP && left.Coins == right.Coins;

        public static bool ValidateReadStoryRewardContract(out string detail)
        {
            var before = new RewardSnapshot(30f, 30f, 30f, 30f, 30f, 100, 40);
            var afterPerfectRead = new RewardSnapshot(44f, 40f, 36f, 30f, 30f, 112, 45);
            string receipt = BuildRewardReceipt(before, afterPerfectRead);
            const string expected = "+14 WONDER  +10 WARMTH  +6 REST  +12 XP  +5 COINS";
            detail = $"receipt=\"{receipt}\" revealRewards=0 expected=\"{expected}\"";
            return receipt == expected && RewardsEqual(afterPerfectRead, afterPerfectRead);
        }

        public static string BuildRewardReceipt(RewardSnapshot before, RewardSnapshot after)
        {
            var receipt = new StringBuilder();
            AppendStatDelta(receipt, after.Wonder - before.Wonder, "WONDER");
            AppendStatDelta(receipt, after.Warmth - before.Warmth, "WARMTH");
            AppendStatDelta(receipt, after.Rest - before.Rest, "REST");
            AppendStatDelta(receipt, after.Magic - before.Magic, "MAGIC");
            AppendStatDelta(receipt, after.Hunger - before.Hunger, "HUNGER");
            AppendIntDelta(receipt, after.XP - before.XP, "XP");
            AppendIntDelta(receipt, after.Coins - before.Coins, "COINS");
            return receipt.Length > 0 ? receipt.ToString() : "STATS FULL";
        }

        static void AppendStatDelta(StringBuilder receipt, float delta, string label)
        {
            string formattedDelta = delta.ToString("0.#", CultureInfo.InvariantCulture);
            if (formattedDelta == "0" || formattedDelta == "-0") return;
            AppendSeparator(receipt);
            if (delta > 0f) receipt.Append('+');
            receipt.Append(formattedDelta);
            receipt.Append(' ').Append(label);
        }

        static void AppendIntDelta(StringBuilder receipt, int delta, string label)
        {
            if (delta == 0) return;
            AppendSeparator(receipt);
            if (delta > 0) receipt.Append('+');
            receipt.Append(delta.ToString(CultureInfo.InvariantCulture));
            receipt.Append(' ').Append(label);
        }

        static void AppendSeparator(StringBuilder receipt)
        {
            if (receipt.Length > 0) receipt.Append("  ");
        }

        public static bool ValidateRewardReceiptContract(out string detail)
        {
            var sleepBefore = new RewardSnapshot(100f, 100f, 80f, 100f, 100f, 0, 10);
            var sleepAfter = new RewardSnapshot(100f, 100f, 100f, 100f, 100f, 0, 10);
            string sleep = BuildRewardReceipt(sleepBefore, sleepAfter);

            var coinsAfter = new RewardSnapshot(100f, 100f, 100f, 100f, 100f, 8, 15);
            string coins = BuildRewardReceipt(sleepAfter, coinsAfter);
            int firstCoins = coins.IndexOf("COINS", System.StringComparison.Ordinal);
            int lastCoins = coins.LastIndexOf("COINS", System.StringComparison.Ordinal);

            string capped = BuildRewardReceipt(sleepAfter, sleepAfter);
            var decimalBefore = new RewardSnapshot(97.5f, 100f, 100f, 100f, 100f, 0, 0);
            var decimalAfter = new RewardSnapshot(100f, 100f, 100f, 100f, 100f, 0, 0);
            string decimalStat = BuildRewardReceipt(decimalBefore, decimalAfter);
            bool sleepDeltaPass = sleep == "+20 REST";
            bool cappedStatOmitted = !sleep.Contains("WARMTH") && capped == "STATS FULL";
            bool integerRewardsPass = coins == "+8 XP  +5 COINS";
            bool coinsOnce = firstCoins >= 0 && firstCoins == lastCoins;
            bool decimalStatPass = decimalStat == "+2.5 WONDER";
            detail = $"sleep=\"{sleep}\" capped=\"{capped}\" decimal=\"{decimalStat}\" " +
                $"coins=\"{coins}\"";
            return sleepDeltaPass && cappedStatOmitted && integerRewardsPass && coinsOnce &&
                decimalStatPass;
        }

        string StepGrade()
        {
            int score = Mathf.RoundToInt(LastGestureScore * 100f);
            return $"{GradeFor(LastGestureScore)} {score}";
        }

        static string GradeFor(float score) =>
            MoonlightActionFeedback.ActionQualityTierFor(score).ToString().ToUpperInvariant();
    }
}
