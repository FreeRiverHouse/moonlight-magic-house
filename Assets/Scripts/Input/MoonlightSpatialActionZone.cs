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
        Care
    }

    public class MoonlightSpatialActionZone : MonoBehaviour
    {
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
        [SerializeField, Range(0.35f, 0.9f)] float passingScore = 0.58f;
        int _progressStep;
        float _sessionScoreTotal;
        int _sessionAcceptedSteps;
        int _perfectSteps;
        int _currentCombo;
        int _bestCombo;

        public MoonlightSpatialActionKind Kind => kind;
        public float Radius => radius;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? kind.ToString() : displayName;
        public int ProgressStep => _progressStep;
        public int RequiredSteps => kind switch
        {
            MoonlightSpatialActionKind.Cook or MoonlightSpatialActionKind.Play or
                MoonlightSpatialActionKind.Garden or MoonlightSpatialActionKind.Read or
                MoonlightSpatialActionKind.Care => 4,
            _ => 1
        };
        public MoonlightGestureKind RequiredGesture => kind switch
        {
            MoonlightSpatialActionKind.Cook => _progressStep switch
            {
                0 => MoonlightGestureKind.Tap,
                1 => MoonlightGestureKind.Circle,
                2 => MoonlightGestureKind.Hold,
                _ => MoonlightGestureKind.ZigZag
            },
            MoonlightSpatialActionKind.Play => _progressStep switch
            {
                0 => MoonlightGestureKind.Swipe,
                1 => MoonlightGestureKind.ZigZag,
                2 => MoonlightGestureKind.Swipe,
                _ => MoonlightGestureKind.Tap
            },
            MoonlightSpatialActionKind.Garden => _progressStep switch
            {
                0 => MoonlightGestureKind.Tap,
                1 => MoonlightGestureKind.Circle,
                2 => MoonlightGestureKind.ZigZag,
                _ => MoonlightGestureKind.Hold
            },
            MoonlightSpatialActionKind.Read => _progressStep switch
            {
                0 => MoonlightGestureKind.Tap,
                1 => MoonlightGestureKind.Swipe,
                2 => MoonlightGestureKind.Circle,
                _ => MoonlightGestureKind.Hold
            },
            MoonlightSpatialActionKind.Care => CareGestureForStep(_progressStep),
            _ => MoonlightGestureKind.Tap
        };
        public float LastGestureScore { get; private set; }
        public bool LastGesturePassed { get; private set; }
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
                MoonlightSpatialActionKind.SleepCuddle => moonlight != null && moonlight.stats.rest < 82f ? "SLEEP" : "CUDDLE",
                MoonlightSpatialActionKind.Care => CareLabelForStep(_progressStep),
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

        public string ExecuteGesture(MoonlightCharacter moonlight, MoonlightGestureKind gesture, float score)
        {
            if (moonlight == null) return "Moonlight is not ready yet.";
            var feedback = moonlight.GetComponent<MoonlightActionFeedback>();
            if (feedback == null) feedback = moonlight.gameObject.AddComponent<MoonlightActionFeedback>();

            LastGestureScore = Mathf.Clamp01(score);
            LastGesturePassed = false;
            if (!feedback.CanBeginAction)
            {
                LastCueKey = "activity-busy";
                Debug.Log($"[MoonlightActivityQA] gesture-blocked kind={kind} " +
                    $"reason=\"{feedback.InputBlockReason}\" step={_progressStep + 1}/{RequiredSteps}");
                return feedback.InputBlockReason;
            }

            bool gesturePassed = gesture == RequiredGesture && LastGestureScore >= passingScore;
            if (!gesturePassed)
            {
                _currentCombo = 0;
                LastCueKey = "activity-try-again";
                AudioManager.Instance?.Play(LastCueKey);
                HapticFeedback.Failure();
                Debug.Log($"[MoonlightActivityQA] gesture-fail kind={kind} expected={RequiredGesture} " +
                    $"actual={gesture} score={LastGestureScore:0.00} step={_progressStep + 1}/{RequiredSteps}");
                return $"TRY AGAIN  /  {GestureInstruction(RequiredGesture)}  /  SCORE {Mathf.RoundToInt(LastGestureScore * 100f)}";
            }

            switch (kind)
            {
                case MoonlightSpatialActionKind.Cook:
                    if (!TryBeginFeedback(feedback, "Cooking")) return feedback.InputBlockReason;
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
                    if (!TryBeginFeedback(feedback, "Playing")) return feedback.InputBlockReason;
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
                    if (!TryBeginFeedback(feedback, "Gardening")) return feedback.InputBlockReason;
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
                    if (!TryBeginFeedback(feedback, "Reading")) return feedback.InputBlockReason;
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
                    return $"STORY REMEMBERED  /  {readMastery}  /  " +
                        BuildRewardReceipt(readBefore, CaptureRewards(moonlight));

                case MoonlightSpatialActionKind.SleepCuddle:
                    if (moonlight.stats.rest < 82f)
                    {
                        if (!TryBeginFeedback(feedback, "Resting")) return feedback.InputBlockReason;
                        LastCueKey = "sleep";
                        AudioManager.Instance?.Play(LastCueKey);
                        RewardSnapshot sleepBefore = CaptureRewards(moonlight);
                        moonlight.PutToSleep();
                        return "DREAMING  /  " +
                            BuildRewardReceipt(sleepBefore, CaptureRewards(moonlight));
                    }
                    if (!TryBeginFeedback(feedback, "Cuddled")) return feedback.InputBlockReason;
                    LastCueKey = "cuddle";
                    AudioManager.Instance?.Play(LastCueKey);
                    RewardSnapshot cuddleBefore = CaptureRewards(moonlight);
                    moonlight.Cuddle();
                    AchievementSystem.Instance?.OnFirstCuddle();
                    return "CUDDLED  /  " +
                        BuildRewardReceipt(cuddleBefore, CaptureRewards(moonlight));

                case MoonlightSpatialActionKind.Care:
                    if (!TryBeginFeedback(feedback, "Caring")) return feedback.InputBlockReason;
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

        bool TryBeginFeedback(MoonlightActionFeedback feedback, string state)
        {
            bool isScoredActivity = RequiredSteps > 1;
            bool began = isScoredActivity
                ? feedback.TryBegin(kind, DisplayName, state, _progressStep, RequiredSteps,
                    LastGestureScore)
                : feedback.TryBegin(kind, DisplayName, state, _progressStep, RequiredSteps);
            if (!began)
            {
                LastGesturePassed = false;
                return false;
            }

            LastGesturePassed = true;
            // The scored activity owns this pulse for every step. Completion
            // methods suppress their legacy pulse to avoid duplicate feedback.
            if (isScoredActivity)
                feedback.PlayActionQualityHaptic();
            return true;
        }

        public static string GestureInstruction(MoonlightGestureKind gesture) => gesture switch
        {
            MoonlightGestureKind.Circle => "DRAW A CIRCLE TO",
            MoonlightGestureKind.Hold => "PRESS AND HOLD TO",
            MoonlightGestureKind.Swipe => "SWIPE TO",
            MoonlightGestureKind.ZigZag => "DRAW A ZIG-ZAG TO",
            _ => "TAP TO"
        };

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
