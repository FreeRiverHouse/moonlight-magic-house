using UnityEngine;

namespace MoonlightMagicHouse
{
    public enum MoonlightSpatialActionKind
    {
        Cook,
        Play,
        Garden,
        Read,
        SleepCuddle
    }

    public class MoonlightSpatialActionZone : MonoBehaviour
    {
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
                MoonlightSpatialActionKind.Garden or MoonlightSpatialActionKind.Read => 4,
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
                HapticFeedback.Light();
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
                    var recipe = ScriptableObject.CreateInstance<FoodItem>();
                    recipe.itemName = "Mooncake bites";
                    recipe.cost = 0;
                    recipe.hungerBoost = 20f;
                    recipe.warmthBoost = 8f;
                    recipe.wonderBoost = 5f;
                    recipe.magicBoost = 5f;
                    recipe.xpReward = 14;
                    moonlight.Feed(recipe);
                    Destroy(recipe);
                    string cookMastery = CompleteActivitySession(moonlight, 0, out int cookCoins);
                    _progressStep = 0;
                    AudioManager.Instance?.Play("activity-complete");
                    return $"MOONCAKES DECORATED  /  {cookMastery}  /  +20 HUNGER  +8 WARMTH  +5 WONDER  +5 MAGIC  +14 XP  +{cookCoins} COINS";

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
                    moonlight.Explore(RoomType.LivingRoom);
                    moonlight.PerformMagic(5, 2);
                    string playMastery = CompleteActivitySession(moonlight, 2, out int playCoins);
                    _progressStep = 0;
                    AudioManager.Instance?.Play("activity-complete");
                    return $"STAR BALL COMBO  /  {playMastery}  /  +25 WONDER  +13 MAGIC  +32 XP  +{playCoins} COINS";

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
                    moonlight.CompleteGardening();
                    string gardenMastery = CompleteActivitySession(moonlight, 3, out int gardenCoins);
                    _progressStep = 0;
                    AudioManager.Instance?.Play("activity-complete");
                    return $"MOON GARDEN BLOOMED  /  {gardenMastery}  /  +16 WONDER  +12 MAGIC  +10 XP  +{gardenCoins} COINS";

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
                    moonlight.CompleteReading();
                    string readMastery = CompleteActivitySession(moonlight, 2, out int readCoins);
                    _progressStep = 0;
                    AudioManager.Instance?.Play("activity-complete");
                    return $"STORY REMEMBERED  /  {readMastery}  /  +14 WONDER  +10 WARMTH  +6 REST  +12 XP  +{readCoins} COINS";

                case MoonlightSpatialActionKind.SleepCuddle:
                    if (moonlight.stats.rest < 82f)
                    {
                        if (!TryBeginFeedback(feedback, "Resting")) return feedback.InputBlockReason;
                        LastCueKey = "sleep";
                        AudioManager.Instance?.Play(LastCueKey);
                        moonlight.PutToSleep();
                        return "DREAMING  +45 REST  +5 WARMTH";
                    }
                    if (!TryBeginFeedback(feedback, "Cuddled")) return feedback.InputBlockReason;
                    LastCueKey = "cuddle";
                    AudioManager.Instance?.Play(LastCueKey);
                    moonlight.Cuddle();
                    AchievementSystem.Instance?.OnFirstCuddle();
                    return "CUDDLED  +20 WARMTH  +5 WONDER  +8 XP";
            }

            return "Moonlight looks around the room.";
        }

        bool TryBeginFeedback(MoonlightActionFeedback feedback, string state)
        {
            if (!feedback.TryBegin(kind, DisplayName, state, _progressStep, RequiredSteps))
            {
                LastGesturePassed = false;
                return false;
            }

            LastGesturePassed = true;
            // Completion actions already emit their own success haptic through
            // MoonlightCharacter. Intermediate steps get a distinct response.
            if (RequiredSteps > 1 && _progressStep < RequiredSteps - 1)
                HapticFeedback.Medium();
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

        void RecordSuccessfulGesture()
        {
            _sessionScoreTotal += LastGestureScore;
            _sessionAcceptedSteps++;
            _currentCombo++;
            _bestCombo = Mathf.Max(_bestCombo, _currentCombo);
            if (LastGestureScore >= 0.88f) _perfectSteps++;
        }

        string CompleteActivitySession(MoonlightCharacter moonlight, int baseCoins,
                                       out int totalCoins)
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
            totalCoins = baseCoins + bonus;
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

        string StepGrade()
        {
            int score = Mathf.RoundToInt(LastGestureScore * 100f);
            return $"{GradeFor(LastGestureScore)} {score}";
        }

        static string GradeFor(float score) =>
            score >= 0.88f ? "PERFECT" : score >= 0.72f ? "GREAT" : "GOOD";
    }
}
