using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MoonlightMagicHouse
{
    public class MoonlightVisualQA : MonoBehaviour
    {
        public const string MoonbudArticulatedLocomotionMarker =
            "MOONLIGHT_CONTROLLERLESS_ARTICULATED_LOCOMOTION_VERIFIED";
        public const string MoonbudAnimatorControllerIncompleteMarker =
            "MOONLIGHT_ANIMATOR_CONTROLLER_LOCOMOTION_INCOMPLETE";
        public const string MoonbudPhotorealSpecialistLocomotionMarker =
            "MOONLIGHT_PHOTOREAL_SPECIALIST_LOCOMOTION_VERIFIED";
        public const string ActivityResultTimingMarker =
            "MOONLIGHT_ACTIVITY_RESULT_TIMING_CLASSIFICATION_VERIFIED";
        public const string ActivityResultDisplayBoundaryMarker =
            "MOONLIGHT_ACTIVITY_RESULT_DISPLAY_BOUNDARY_VERIFIED";
        public const string ActivityResultVisibleIntervalMarker =
            "MOONLIGHT_ACTIVITY_RESULT_VISIBLE_INTERVAL_VERIFIED";
        public const string GestureReadStaticContractMarker =
            "MOONLIGHT_GESTURE_READ_STATIC_CONTRACT_VERIFIED";
        public const string GestureReadStaticContractFailureMarker =
            "MOONLIGHT_GESTURE_READ_STATIC_CONTRACT_FAILED";
        public const string GestureReadRuntimeContractMarker =
            "MOONLIGHT_GESTURE_READ_RUNTIME_CONTRACT_VERIFIED";
        public const string GestureReadRuntimeContractFailureMarker =
            "MOONLIGHT_GESTURE_READ_RUNTIME_CONTRACT_FAILED";
        public const string GestureCareStaticContractMarker =
            "MOONLIGHT_GESTURE_CARE_STATIC_CONTRACT_VERIFIED";
        public const string GestureCareStaticContractFailureMarker =
            "MOONLIGHT_GESTURE_CARE_STATIC_CONTRACT_FAILED";
        public const string GestureCareRuntimeContractMarker =
            "MOONLIGHT_GESTURE_CARE_PROPAGATION_4_OF_4_VERIFIED";
        public const string GestureCareRuntimeStepMarker =
            "MOONLIGHT_GESTURE_CARE_RUNTIME_STEP_VERIFIED";
        public const string GestureCareRuntimeContractFailureMarker =
            "MOONLIGHT_GESTURE_CARE_RUNTIME_CONTRACT_FAILED";
        public const string GestureCareContrastRuntimeContractMarker =
            "MOONLIGHT_GESTURE_CARE_LIVE_CONTRASTS_VERIFIED";
        public const string ActivityQualityReadoutStaticContractMarker =
            "MOONLIGHT_IPAD_ACTIVITY_QUALITY_STATIC_CONTRACT_VERIFIED";
        public const string ActivityQualityReadoutRuntimeContractMarker =
            "MOONLIGHT_IPAD_ACTIVITY_QUALITY_RUNTIME_BINDING_VERIFIED";
        public const string ActivityQualityReadoutZoneExitMarker =
            "MOONLIGHT_IPAD_ACTIVITY_QUALITY_ZONE_EXIT_RESET_VERIFIED";
        public const string CompactIPadInstructionStaticContractMarker =
            "MOONLIGHT_IPAD_COMPACT_INSTRUCTION_STATIC_CONTRACT_VERIFIED";
        public const string CompactIPadInstructionStaticContractFailureMarker =
            "MOONLIGHT_IPAD_COMPACT_INSTRUCTION_STATIC_CONTRACT_FAILED";
        public const string RetryResultStaticContractMarker =
            "MOONLIGHT_RETRY_RESULT_22_OF_22_STATIC_CONTRACT_VERIFIED";
        public const string IPadResultFormatterStaticContractMarker =
            "MOONLIGHT_IPAD_RESULT_FORMATTER_STATIC_CONTRACT_VERIFIED";
        public const string LowScoreRetryRuntimeMarker =
            "MOONLIGHT_LOW_SCORE_RETRY_5_OF_5_RUNTIME_VERIFIED";
        public const string PlayContinuityRuntimeMarker =
            "MOONLIGHT_PLAY_CONTINUITY_TRANSACTION_VERIFIED";
        public const string PlayContinuityOrderModeMarker =
            "MOONLIGHT_PLAY_CONTINUITY_ORDER_MODE";
        public const string PlayPhaseLandmarkTransactionMarker =
            "MOONLIGHT_PLAY_PHASE_LANDMARK_TRANSACTION_VERIFIED";
        public const string CookContinuityRuntimeMarker =
            "MOONLIGHT_COOK_CONTINUITY_TRANSACTION_VERIFIED";
        public const string CookMooncakeRevealTrayRuntimeMarker =
            "MOONLIGHT_COOK_MOONCAKE_REVEAL_TRAY_RUNTIME_VERIFIED";
        public const string CookMooncakeRevealTrayLingerMarker =
            "MOONLIGHT_COOK_MOONCAKE_REVEAL_TRAY_LINGER_VERIFIED";

        static readonly string[] IntermediateActivityResults =
        {
            "INGREDIENTS ADDED  /  PERFECT 100  COMBO x1  /  NEXT: CIRCLE TO STIR",
            "BATTER SPARKLING  /  PERFECT 100  COMBO x2  /  NEXT: HOLD TO BAKE",
            "MOONCAKES BAKED  /  PERFECT 100  COMBO x3  /  NEXT: ZIG-ZAG TO DECORATE",
            "STAR BALL THROWN  /  PERFECT 100  COMBO x1  /  NEXT: ZIG-ZAG CHASE",
            "GREAT CHASE  /  PERFECT 100  COMBO x2  /  NEXT: SWIPE TO JUMP",
            "MAGIC JUMP  /  PERFECT 100  COMBO x3  /  NEXT: TAP TO CATCH",
            "MOONSEEDS PLANTED  /  PERFECT 100  COMBO x1  /  NEXT: CIRCLE TO WATER",
            "DEW SPARKLING  /  PERFECT 100  COMBO x2  /  NEXT: ZIG-ZAG TO TEND",
            "SPROUTS TENDED  /  PERFECT 100  COMBO x3  /  NEXT: HOLD TO BLOOM",
            "STORY OPENED  /  PERFECT 100  COMBO x1  /  NEXT: SWIPE TO TURN",
            "STAR PAGE TURNED  /  PERFECT 100  COMBO x2  /  NEXT: CIRCLE TO TRACE",
            "CONSTELLATION TRACED  /  PERFECT 100  COMBO x3  /  NEXT: HOLD TO REMEMBER",
            "SPA PREPARED  /  PERFECT 100  COMBO x1  /  NEXT: CIRCLE TO WASH",
            "MOONLIGHT WASHED  /  PERFECT 100  COMBO x2  /  NEXT: SWIPE TO BRUSH",
            "HAIR BRUSHED  /  PERFECT 100  COMBO x3  /  NEXT: HOLD TO GLOW"
        };

        static readonly string[] FinalActivityResults =
        {
            "MOONCAKES DECORATED  /  RUN PERFECT 100 x4  /  +5 WONDER  +8 WARMTH  +5 MAGIC  +20 HUNGER  +14 XP  +3 COINS",
            "STAR BALL COMBO  /  RUN PERFECT 100 x4  /  +25 WONDER  +13 MAGIC  +32 XP  +5 COINS",
            "MOON GARDEN BLOOMED  /  RUN PERFECT 100 x4  /  +16 WONDER  +12 MAGIC  +10 XP  +6 COINS",
            "STORY REMEMBERED  /  RUN PERFECT 100 x4  /  +14 WONDER  +10 WARMTH  +6 REST  +12 XP  +5 COINS",
            "MOON SPA COMPLETE  /  RUN PERFECT 100 x4  /  +18 WARMTH  +12 REST  +6 MAGIC  +12 XP  +5 COINS"
        };

        static readonly string[] NonFinalActivityResultEdgeCases =
        {
            null,
            "ARBITRARY RESULT",
            "MOONCAKES DECORATED",
            "FED  /  +18 HUNGER  MOON SPA COMPLETE",
            "STAR BALL COMBO  /  RUN  /  +5 COINS",
            "MOON SPA COMPLETE  /  RUN PERFECT 100 x4  /  "
        };

        public static MoonlightVisualQA Instance { get; private set; }

        static readonly List<MoonlightAnimator> MoonlightAnimatorQACandidates = new();
        static readonly List<MoonlightKidAnimator> MoonlightKidAnimatorQACandidates = new();

        sealed class ContextResultVisibilityObservation
        {
            public bool Completed;
            public bool FeedbackWasActiveAtStart;
            public bool FeedbackEnded;
            public bool ResultChangedDuringFeedback;
            public bool ShowAfterFeedback;
            public bool SawExpectedText;
            public bool SawRemoval;
            public string ObservedText = "";
            public float FeedbackEndedAtSeconds;
            public float ShownAtSeconds;
            public float VisibleSeconds;
        }

        sealed class CareLiveProbeSpec
        {
            public readonly string Name;
            public readonly int Step;
            public readonly Vector2[] Points;
            public readonly float HoldSeconds;
            public readonly bool MeasureFinalLinger;

            public CareLiveProbeSpec(string name, int step, Vector2[] points,
                float holdSeconds = 0f, bool measureFinalLinger = false)
            {
                Name = name;
                Step = step;
                Points = points;
                HoldSeconds = holdSeconds;
                MeasureFinalLinger = measureFinalLinger;
            }
        }

        sealed class CareLiveProbeObservation
        {
            public bool Passed;
            public string Detail = "not-run";
            public MoonlightGestureSample Sample;
            public Vector3 PrimaryA;
            public Vector3 PrimaryB;
            public Vector3 BubbleA;
            public Vector3 BubbleB;
            public Vector3 AuraScale;
            public float LightIntensity;
            public int MoteCount;
            public int RendererCount;
            public int MaterialCount;
            public int LightCount;
            public bool PersistentIsolationPassed;
            public bool FinalLingerPassed;
            public string FinalLingerDetail = "not-measured";
        }

        sealed class CareLiveHarnessResult
        {
            public bool Passed;
            public string Detail = "not-run";
        }

        sealed class CareLiveHarnessContext
        {
            public MoonlightActionFeedback Feedback;
            public MoonlightActivityStage Stage;
        }

        sealed class BedtimeRuntimeObservation
        {
            public bool Passed;
            public bool SawVisibleLinger;
            public int HeldPresentationFrames;
            public float ObservedLingerSeconds;
            public bool HadCamera;
            public int CameraInstanceId;
            public Vector3 CameraPositionBefore;
            public Quaternion CameraRotationBefore = Quaternion.identity;
            public bool HadCameraController;
            public int CameraControllerInstanceId;
            public bool CameraControllerEnabledBefore;
            public string Detail = "not-run";
        }

        sealed class BedtimeStageCleanupObservation
        {
            public bool Passed;
            public string Detail = "not-run";
        }

        readonly struct BedtimeOraclePart
        {
            public readonly string Name;
            public readonly string MeshToken;
            public readonly bool RestingVisible;
            public readonly bool CuddledVisible;

            public BedtimeOraclePart(string name, string meshToken, bool restingVisible,
                bool cuddledVisible)
            {
                Name = name;
                MeshToken = meshToken;
                RestingVisible = restingVisible;
                CuddledVisible = cuddledVisible;
            }
        }

        static readonly BedtimeOraclePart[] BedtimeOracleParts =
        {
            new("BedtimeBedFrame", "Cube", true, true),
            new("BedtimeBlanket", "Cube", true, true),
            new("RestingPillow", "Cube", true, false),
            new("RestingDreamMoon", "Cylinder", true, false),
            new("RestingDreamStar", "Cube", true, false),
            new("CuddledHeartLeft", "Sphere", false, true),
            new("CuddledHeartRight", "Sphere", false, true),
            new("CuddledHeartPoint", "Cube", false, true)
        };

        readonly struct PlayRewardSnapshot
        {
            public readonly float Wonder;
            public readonly float Warmth;
            public readonly float Rest;
            public readonly float Magic;
            public readonly float Hunger;
            public readonly int XP;
            public readonly int Coins;

            public PlayRewardSnapshot(MoonlightCharacter moonlight)
            {
                Wonder = moonlight.stats.wonder;
                Warmth = moonlight.stats.warmth;
                Rest = moonlight.stats.rest;
                Magic = moonlight.stats.magic;
                Hunger = moonlight.stats.hunger;
                XP = moonlight.xp;
                Coins = moonlight.coins;
            }
        }

        readonly struct PlayHeldSnapshot
        {
            public readonly int RootId;
            public readonly int BallId;
            public readonly int TrailId;
            public readonly int MaterialCount;
            public readonly int BallMaterialId;
            public readonly int TrailMaterialId;
            public readonly Vector3 BallPosition;
            public readonly int ProgressStep;
            public readonly int AcceptedSteps;
            public readonly float AverageScore;
            public readonly int CurrentCombo;
            public readonly int BestCombo;
            public readonly int PerfectSteps;
            public readonly PlayRewardSnapshot Rewards;

            public PlayHeldSnapshot(MoonlightSpatialActionZone zone,
                MoonlightActivityStage stage, MoonlightCharacter moonlight)
            {
                RootId = stage.PlayStageRootInstanceId;
                BallId = stage.PlayBallInstanceId;
                TrailId = stage.PlayTrailInstanceId;
                MaterialCount = stage.ActiveUniqueMaterialCount;
                BallMaterialId = stage.PlayBallSharedMaterialInstanceId;
                TrailMaterialId = stage.PlayTrailSharedMaterialInstanceId;
                BallPosition = stage.PlayBallLocalPosition;
                ProgressStep = zone.ProgressStep;
                AcceptedSteps = zone.ActivitySessionAcceptedSteps;
                AverageScore = zone.ActivitySessionAverageScore;
                CurrentCombo = zone.ActivityCurrentCombo;
                BestCombo = zone.ActivityBestCombo;
                PerfectSteps = zone.ActivityPerfectSteps;
                Rewards = new PlayRewardSnapshot(moonlight);
            }
        }

        sealed class PlayContinuityObservation
        {
            public readonly List<int> Progression = new();
            public readonly PlayRewardSnapshot[] AcceptedRewards =
                new PlayRewardSnapshot[4];
            public readonly int[] MaterialCounts = new int[4];
            public readonly int[] PhaseLandmarkVisibilityMasks = new int[4];
            public readonly int[] PhaseLandmarkMaterialCounts = new int[4];
            public readonly int[] PhaseLandmarkLightCounts = new int[4];
            public PlayRewardSnapshot BaselineRewards;
            public MoonlightActivityStage Stage;
            public int RootId;
            public int BallId;
            public int TrailId;
            public int InitialMaterialCount;
            public int BallMaterialId;
            public int TrailMaterialId;
            public int CompletionApplicationBaseline;
            public int ContinuationBaseline;
            public Vector3 HeldEndpoint;
            public bool HasHeldEndpoint;
            public bool HasInitialMaterialSnapshot;
            public bool IdentityPreserved = true;
            public bool TrajectoryFiniteAndBounded = true;
            public int TrajectorySampleCount;
            public int TrajectoryStepsObserved;
            public int FirstRenderedContinuationFramesObserved;
            public int RejectedPreservationCount;
            public float MaximumImmediateDiscontinuity;
            public float MaximumFirstRenderedFrameDiscontinuity;
            public float MaximumContinuationClockDelta;
            public bool ContinuationClockAdvanced;
            public bool ContinuationClockSourceIsUnscaled = true;
            public int PhaseLandmarkObservedMask;
            public bool PhaseLandmarkContractPassed = true;
            public string ArenaSource = "";
            public bool ArenaSourceContractPassed = true;
            public string OrderMode = "middle";
            public bool CatchTapOraclePassed;
            public bool CatchTapSamplesDistinct;
            public bool CatchTapFiniteAndBounded = true;
            public bool CatchTapNoIntersectionOrOvershoot = true;
            public int CatchTapSamplesPerTrajectory;
            public float CatchTapSeparationAt020;
            public float CatchTapMaximumContactError;
            public float CatchTapMaximumHeldError;
            public float CatchTapCenterMaximumDelta;
        }

        sealed class CookContinuityObservation
        {
            public MoonlightActivityStage Stage;
            public int RootId;
            public int MaterialIdentityCount;
            public int MaterialIdentityHash;
            public int LightId;
            public int BuildBaseline;
            public int HandoffBaseline;
            public int TerminalHoldCount;
            public int HandoffCount;
            public int SharedPropHandoffMask;
            public int RootIdentityHandoffMask;
            public int MaterialIdentityHandoffMask;
            public int LightIdentityHandoffMask;
            public int SourceObservedMask;
            public string Source = "";
            public bool IdentityPreserved = true;
            public bool SourceContractPassed = true;
            public bool BudgetContractPassed = true;
            public bool FinalLingerObserved;
            public bool RevealTrayCompletionObserved;
            public bool RevealTrayLingerObserved;
            public float MaximumSharedPropDiscontinuity;
        }

        readonly struct CareCompletionRootState
        {
            public readonly int InstanceId;
            public readonly bool ActiveSelf;

            public CareCompletionRootState(Transform root)
            {
                InstanceId = root.GetInstanceID();
                ActiveSelf = root.gameObject.activeSelf;
            }
        }

        sealed class CareLiveStationSnapshot
        {
            readonly MoonlightActivityStation _station;
            readonly int _instanceId;
            readonly bool _activeSelf;
            readonly bool _activeInHierarchy;
            readonly bool _enabled;
            readonly bool _hasCompletion;
            readonly int _completionRenderers;
            readonly int _completionMaterials;
            readonly int _completionColliders;
            readonly int _completionLights;
            readonly bool _completionSeparateMaterials;
            readonly CareCompletionRootState[] _completionRoots;

            public CareLiveStationSnapshot(MoonlightActivityStation station)
            {
                _station = station;
                _instanceId = station.GetInstanceID();
                _activeSelf = station.gameObject.activeSelf;
                _activeInHierarchy = station.gameObject.activeInHierarchy;
                _enabled = station.enabled;
                _hasCompletion = station.HasCompletionState;
                _completionRenderers = station.CompletionRendererCount;
                _completionMaterials = station.CompletionUniqueMaterialCount;
                _completionColliders = station.CompletionEnabledColliderCount;
                _completionLights = station.CompletionEnabledLightCount;
                _completionSeparateMaterials = station.CompletionUsesSeparateMaterials;
                _completionRoots = CareCompletionRoots(station);
            }

            public bool Matches(out string detail)
            {
                CareCompletionRootState[] roots = _station != null
                    ? CareCompletionRoots(_station)
                    : System.Array.Empty<CareCompletionRootState>();
                bool rootIdentity = roots.Length == _completionRoots.Length;
                for (int i = 0; i < roots.Length && rootIdentity; i++)
                    rootIdentity &= roots[i].InstanceId == _completionRoots[i].InstanceId &&
                        roots[i].ActiveSelf == _completionRoots[i].ActiveSelf;
                bool pass = _station != null && _station.GetInstanceID() == _instanceId &&
                    _station.gameObject.activeSelf == _activeSelf &&
                    _station.gameObject.activeInHierarchy == _activeInHierarchy &&
                    _station.enabled == _enabled &&
                    _station.HasCompletionState == _hasCompletion &&
                    _station.CompletionRendererCount == _completionRenderers &&
                    _station.CompletionUniqueMaterialCount == _completionMaterials &&
                    _station.CompletionEnabledColliderCount == _completionColliders &&
                    _station.CompletionEnabledLightCount == _completionLights &&
                    _station.CompletionUsesSeparateMaterials ==
                        _completionSeparateMaterials && rootIdentity;
                detail = $"station={_instanceId} alive={_station != null} " +
                    $"active={(_station != null && _station.gameObject.activeSelf)}/" +
                    $"{_activeSelf} enabled={(_station != null && _station.enabled)}/" +
                    $"{_enabled} completion=" +
                    $"{(_station != null && _station.HasCompletionState)}/" +
                    $"{_hasCompletion} roots={roots.Length}/{_completionRoots.Length} " +
                    $"rootIdentity={rootIdentity}";
                return pass;
            }

            static CareCompletionRootState[] CareCompletionRoots(
                MoonlightActivityStation station) => station == null
                ? System.Array.Empty<CareCompletionRootState>()
                : station.GetComponentsInChildren<Transform>(true)
                    .Where(candidate => candidate != station.transform &&
                        candidate.name == "PersistentCareCompletion")
                    .OrderBy(candidate => candidate.GetInstanceID())
                    .Select(candidate => new CareCompletionRootState(candidate))
                    .ToArray();
        }

        Vector3 _lastLoggedPosition;
        float _lastMoveLogTime;
        bool _registered;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public static bool ValidateActivityResultTimingContract(out string detail)
        {
            int intermediateCompletionCount = 0;
            int intermediateDurationCount = 0;
            for (int i = 0; i < IntermediateActivityResults.Length; i++)
            {
                string result = IntermediateActivityResults[i];
                if (MoonlightUI.IsCompletionResult(result)) intermediateCompletionCount++;
                if (Mathf.Approximately(MoonlightUI.ActivityResultDurationSeconds(result),
                        MoonlightUI.IntermediateActivityResultDurationSeconds))
                    intermediateDurationCount++;
            }

            int finalCompletionCount = 0;
            int finalDurationCount = 0;
            for (int i = 0; i < FinalActivityResults.Length; i++)
            {
                string result = FinalActivityResults[i];
                if (MoonlightUI.IsCompletionResult(result)) finalCompletionCount++;
                if (Mathf.Approximately(MoonlightUI.ActivityResultDurationSeconds(result),
                        MoonlightUI.FinalActivityResultDurationSeconds))
                    finalDurationCount++;
            }

            int edgeCaseNonFinalCount = 0;
            int edgeCaseDurationCount = 0;
            for (int i = 0; i < NonFinalActivityResultEdgeCases.Length; i++)
            {
                string result = NonFinalActivityResultEdgeCases[i];
                if (!MoonlightUI.IsCompletionResult(result)) edgeCaseNonFinalCount++;
                if (Mathf.Approximately(MoonlightUI.ActivityResultDurationSeconds(result),
                        MoonlightUI.IntermediateActivityResultDurationSeconds))
                    edgeCaseDurationCount++;
            }

            bool nonScoredResultsUnchanged =
                !MoonlightUI.IsCompletionResult("FED  /  +18 HUNGER") &&
                !MoonlightUI.IsCompletionResult("DREAMING  /  +20 REST") &&
                !MoonlightUI.IsCompletionResult("CUDDLED  /  +20 WARMTH  +5 WONDER  +8 XP");
            detail = $"intermediateCompletion={intermediateCompletionCount}/" +
                $"{IntermediateActivityResults.Length} intermediate2.8s=" +
                $"{intermediateDurationCount}/{IntermediateActivityResults.Length} " +
                $"finalCompletion={finalCompletionCount}/{FinalActivityResults.Length} " +
                $"final5.6s={finalDurationCount}/{FinalActivityResults.Length} " +
                $"edgeNonFinal={edgeCaseNonFinalCount}/" +
                $"{NonFinalActivityResultEdgeCases.Length} edge2.8s={edgeCaseDurationCount}/" +
                $"{NonFinalActivityResultEdgeCases.Length} " +
                $"nonScoredUnchanged={nonScoredResultsUnchanged}";
            return IntermediateActivityResults.Length == 15 &&
                intermediateCompletionCount == 0 && intermediateDurationCount == 15 &&
                FinalActivityResults.Length == 5 && finalCompletionCount == 5 &&
                finalDurationCount == 5 && NonFinalActivityResultEdgeCases.Length == 6 &&
                edgeCaseNonFinalCount == 6 && edgeCaseDurationCount == 6 &&
                nonScoredResultsUnchanged;
        }

        static bool ValidateActivityQualityReadoutRuntimeBinding(MoonlightUI ui,
            MoonlightSpatialActionZone zone, bool expectedVisible,
            bool expectedCompletedSnapshot, out string detail)
        {
            if (ui == null || zone == null)
            {
                detail = $"ui={(ui != null)} zone={(zone != null)}";
                return false;
            }

            if (!expectedVisible)
            {
                bool hidden = !ui.IsActivityQualityReadoutVisible &&
                    string.IsNullOrEmpty(ui.ActivityQualityReadoutQAText);
                detail = $"expected=hidden visible={ui.IsActivityQualityReadoutVisible} " +
                    $"text=\"{ui.ActivityQualityReadoutQAText.Replace('\n', '/')}\"";
                return hidden;
            }

            int expectedCombo = expectedCompletedSnapshot
                ? zone.LastCompletedBestCombo
                : zone.ActivityCurrentCombo;
            float expectedAverage = expectedCompletedSnapshot
                ? zone.LastCompletedAverageScore
                : zone.ActivitySessionAverageScore;
            string expectedText = MoonlightUI.ActivityQualityReadoutText(
                zone.LastGestureScore, expectedCombo, expectedAverage);
            bool pass = ui.IsActivityQualityReadoutVisible &&
                ui.ActivityQualityUsesCompletedSnapshotForQA == expectedCompletedSnapshot &&
                ui.ActivityQualityReadoutComboForQA == expectedCombo &&
                Approximately(ui.ActivityQualityReadoutRunScoreForQA, expectedAverage) &&
                ui.ActivityQualityReadoutQAText == expectedText &&
                ui.ActivityQualityReadoutMatchesZoneState &&
                ui.ActivityQualityReadoutIsInsideSafeArea &&
                ui.ActivityQualityReadoutDoesNotOverlapPanels &&
                ui.ActivityQualityReadoutGraphicsDoNotBlockRaycasts &&
                ui.ActivityQualityReadoutQAMarker ==
                    "MOONLIGHT_IPAD_ACTIVITY_QUALITY_READY";
            detail = $"expected={expectedText.Replace('\n', '/')} " +
                $"actual={ui.ActivityQualityReadoutQAText.Replace('\n', '/')} " +
                $"combo={ui.ActivityQualityReadoutComboForQA}/{expectedCombo} " +
                $"run={ui.ActivityQualityReadoutRunScoreForQA:0.000}/{expectedAverage:0.000} " +
                $"completedSnapshot={ui.ActivityQualityUsesCompletedSnapshotForQA}/" +
                $"{expectedCompletedSnapshot} zoneBound={ui.ActivityQualityReadoutMatchesZoneState} " +
                $"safe={ui.ActivityQualityReadoutIsInsideSafeArea} " +
                $"separated={ui.ActivityQualityReadoutDoesNotOverlapPanels} " +
                $"raycasts={ui.ActivityQualityReadoutGraphicsDoNotBlockRaycasts} " +
                $"marker={ui.ActivityQualityReadoutQAMarker}";
            return pass;
        }

        IEnumerator ObserveContextResultVisibility(MoonlightUI ui,
            MoonlightActionFeedback feedback, int shownCountBeforeResult,
            string expectedText, float expectedDuration,
            ContextResultVisibilityObservation observation)
        {
            int targetShownCount = shownCountBeforeResult + 1;
            observation.FeedbackWasActiveAtStart =
                feedback != null && feedback.IsPerformingAction;
            float feedbackDeadline = Time.time + 6f;
            while (feedback != null && feedback.IsPerformingAction &&
                   Time.time < feedbackDeadline)
            {
                if (ui.ContextResultShownCountForQA != shownCountBeforeResult ||
                    !string.IsNullOrEmpty(ui.ContextResultQAText))
                    observation.ResultChangedDuringFeedback = true;
                yield return null;
            }
            observation.FeedbackEnded = feedback == null || !feedback.IsPerformingAction;
            observation.FeedbackEndedAtSeconds = Time.time;

            float shownDeadline = Time.time + 0.75f;
            while (ui.ContextResultShownCountForQA < targetShownCount &&
                   Time.time < shownDeadline)
                yield return null;
            observation.ObservedText = ui.ContextResultQAText;
            observation.SawExpectedText =
                ui.ContextResultShownCountForQA == targetShownCount &&
                !string.IsNullOrEmpty(observation.ObservedText) &&
                observation.ObservedText == ui.LastContextResultShownTextForQA &&
                observation.ObservedText.Contains(expectedText,
                    System.StringComparison.Ordinal);
            observation.ShownAtSeconds =
                ui.LastContextResultShownAtSecondsForQA;
            const float showAfterFeedbackTolerance = 0.01f;
            observation.ShowAfterFeedback = observation.SawExpectedText &&
                observation.ShownAtSeconds >=
                    observation.FeedbackEndedAtSeconds - showAfterFeedbackTolerance;

            float hiddenDeadline = Time.time + expectedDuration + 0.75f;
            while (ui.LastContextResultHiddenShownCountForQA < targetShownCount &&
                   Time.time < hiddenDeadline)
                yield return null;
            observation.SawRemoval =
                ui.LastContextResultHiddenShownCountForQA == targetShownCount &&
                string.IsNullOrEmpty(ui.ContextResultQAText);
            observation.VisibleSeconds =
                ui.LastContextResultVisibleDurationSecondsForQA;
            observation.Completed = true;
        }

        IEnumerator Start()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            bool isAutomatedQa = args.Contains("-moonlightGameplayQa") ||
                                 args.Contains("-moonlightRoomQa") ||
                                 args.Contains("-moonlightSpatialQa");
            if (isAutomatedQa)
            {
                Application.runInBackground = true;
            }
            if (args.Contains("-moonlightGameplayQa"))
            {
                yield return RunGameplayQa(args);
                yield break;
            }
            if (args.Contains("-moonlightRoomQa"))
            {
                yield return RunRoomCaptureQa(args);
                yield break;
            }
            if (!args.Contains("-moonlightSpatialQa")) yield break;

            int pathIndex = System.Array.IndexOf(args, "-moonlightSpatialQaPath");
            string output = pathIndex >= 0 && pathIndex + 1 < args.Length
                ? args[pathIndex + 1]
                : Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
                    "MMH-QA/spatial/moonlight_spatial_after.png");
            string directory = Path.GetDirectoryName(output);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            Screen.SetResolution(1366, 1024, false);
            yield return new WaitForSeconds(2.5f);

            var controller = FindAnyObjectByType<MoonlightPlayerController>();
            var interactor = FindAnyObjectByType<MoonlightSpatialInteractor>();
            var moonlight = FindAnyObjectByType<MoonlightCharacter>();
            var ui = FindAnyObjectByType<MoonlightUI>();
            if (controller == null || interactor == null || moonlight == null)
            {
                Debug.LogError("[MoonlightVisualQA][FAIL] spatial controller/interactor/character missing");
                Application.Quit(2);
                yield break;
            }

            if (!MoonlightHouseSetup.ValidateHeroSurfaceContract(out string heroSurfaceDetail))
            {
                Debug.LogError($"[MoonlightVisualQA][FAIL] hero-surface-contract {heroSurfaceDetail}");
                Application.Quit(63);
                yield break;
            }
            var heroQuality = moonlight.GetComponentInChildren<MoonlightHeroVisualQuality>(true);
            bool heroSurfacePass = heroQuality != null &&
                heroQuality.QAMarker == "MOONLIGHT_HERO_SURFACE_SHADING_READY";
            if (!heroSurfacePass)
            {
                Debug.LogError($"[MoonlightVisualQA][FAIL] hero-surface-shading " +
                    $"marker={(heroQuality != null ? heroQuality.QAMarker : "missing")} " +
                    $"contract={heroSurfaceDetail}");
                Application.Quit(64);
                yield break;
            }
            Debug.Log($"[MoonlightVisualQA][PASS] hero-surface-shading " +
                $"renderers={heroQuality.RendererCount} shadows={heroQuality.ShadowRendererCount} " +
                $"materials={heroQuality.MaterialCount}/{MoonlightHouseSetup.HeroMaterialBudget} " +
                $"profiles={heroQuality.SurfaceProfileCount} emissive={heroQuality.EmissiveMaterialCount} " +
                $"contract={heroSurfaceDetail} marker=MOONLIGHT_HERO_SURFACE_SHADING_VERIFIED");
            if (!MoonlightHouseSetup.ValidateHeroEyeContract(out string heroEyeDetail))
            {
                Debug.LogError($"[MoonlightVisualQA][FAIL] hero-eye-contract {heroEyeDetail}");
                Application.Quit(72);
                yield break;
            }
            if (!EyeBlinker.ValidateBlinkContract(out string blinkDetail))
            {
                Debug.LogError($"[MoonlightVisualQA][FAIL] authored-eye-blink-contract {blinkDetail}");
                Application.Quit(74);
                yield break;
            }
            if (!MoonlightHeroEyeQuality.ValidateActivityExpressionContract(
                    out string eyeExpressionDetail))
            {
                Debug.LogError($"[MoonlightVisualQA][FAIL] activity-eye-expression-contract " +
                    eyeExpressionDetail);
                Application.Quit(75);
                yield break;
            }
            var heroEyeQuality = moonlight.GetComponentInChildren<MoonlightHeroEyeQuality>(true);
            bool heroEyePass = heroEyeQuality != null &&
                heroEyeQuality.QAMarker == "MOONLIGHT_HERO_EYE_CATCHLIGHT_READY" &&
                heroEyeQuality.BlinkLinkedPartCount == 4 &&
                heroEyeQuality.BlinkQAMarker == "MOONLIGHT_AUTHORED_EYE_BLINK_READY";
            if (!heroEyePass)
            {
                Debug.LogError($"[MoonlightVisualQA][FAIL] hero-eye-catchlight " +
                    $"marker={(heroEyeQuality != null ? heroEyeQuality.QAMarker : "missing")} " +
                    $"contract={heroEyeDetail}");
                Application.Quit(73);
                yield break;
            }
            Debug.Log($"[MoonlightVisualQA][PASS] hero-eye-catchlight " +
                $"eyes={heroEyeQuality.EyeRendererCount} highlights={heroEyeQuality.HighlightRendererCount} " +
                $"blinkParts={heroEyeQuality.BlinkLinkedPartCount} " +
                $"separation={heroEyeQuality.EyePairSeparation:0.000} " +
                $"emission={heroEyeQuality.CurrentCatchlightEmission:0.00} " +
                $"eyeContract={heroEyeDetail} blinkContract={blinkDetail} " +
                $"expressionContract={eyeExpressionDetail} " +
                "marker=MOONLIGHT_HERO_EYE_CATCHLIGHT_VERIFIED " +
                "MOONLIGHT_AUTHORED_EYE_BLINK_VERIFIED " +
                "MOONLIGHT_ACTIVITY_EYE_EXPRESSION_CONTRACT_VERIFIED");

            Vector3 start = controller.transform.position;
            controller.SetTouchMove(Vector2.up);
            yield return new WaitForSeconds(0.55f);
            controller.SetTouchMove(Vector2.zero);
            yield return new WaitForSeconds(0.65f);

            var zones = FindObjectsByType<MoonlightSpatialActionZone>(FindObjectsSortMode.None)
                .OrderBy(zone => zone.Kind)
                .ToArray();
            var expectedLivingRoomActions = new[]
            {
                MoonlightSpatialActionKind.Play,
                MoonlightSpatialActionKind.SleepCuddle
            };
            bool hasExpectedLivingRoomActions = expectedLivingRoomActions.All(expectedKind =>
                zones.Count(zone => zone.Kind == expectedKind) == 1);
            if (zones.Length != expectedLivingRoomActions.Length || !hasExpectedLivingRoomActions)
            {
                string foundKinds = string.Join(",", zones.Select(zone => zone.Kind.ToString()));
                Debug.LogError($"[MoonlightVisualQA][FAIL] expected active living-room actions " +
                    $"Play,SleepCuddle exactly once; found count={zones.Length} kinds={foundKinds}");
                Application.Quit(3);
                yield break;
            }

            int passedActions = 0;
            foreach (var zone in zones)
            {
                if (zone.Kind == MoonlightSpatialActionKind.SleepCuddle)
                    moonlight.stats.rest = 35f;

                controller.transform.position = zone.transform.position;
                yield return new WaitForSeconds(0.65f);
                if (interactor.CurrentZone != zone)
                {
                    Debug.LogError($"[MoonlightVisualQA][FAIL] action={zone.Kind} zone not acquired");
                    Application.Quit(4);
                    yield break;
                }

                string prompt = interactor.CurrentPrompt;
                BedtimeRuntimeObservation bedtimeNaturalObservation = null;
                if (zone.Kind == MoonlightSpatialActionKind.SleepCuddle)
                {
                    bedtimeNaturalObservation = new BedtimeRuntimeObservation();
                    CaptureBedtimeCameraBaseline(bedtimeNaturalObservation);
                }
                string result = interactor.ExecuteCurrent();
                ui?.ShowContextResult(result);
                var feedback = moonlight.GetComponent<MoonlightActionFeedback>();
                string expectedVisualSignature = MoonlightActionFeedback.ActionVisualSignatureFor(
                    zone.Kind, feedback != null ? feedback.ActivityStep : 0,
                    feedback != null ? feedback.StateText : "");
                string expectedVisualMarker = MoonlightActionFeedback.ActionVisualSignatureMarkerFor(
                    zone.Kind, feedback != null ? feedback.ActivityStep : 0,
                    feedback != null ? feedback.StateText : "");
                bool actionAccentPass = ValidateActionAccent(feedback, expectedVisualSignature,
                    expectedVisualMarker, zone.Kind != MoonlightSpatialActionKind.SleepCuddle,
                    out string actionAccentDetail);
                if (feedback == null || !feedback.IsPerformingAction ||
                    string.IsNullOrEmpty(feedback.ActiveEffectName) ||
                    !actionAccentPass)
                {
                    Debug.LogError($"[MoonlightVisualQA][FAIL] action={zone.Kind} animated feedback " +
                        $"effect={(feedback != null ? feedback.ActiveEffectName : "missing")} " +
                        actionAccentDetail);
                    Application.Quit(5);
                    yield break;
                }
                yield return new WaitForSeconds(0.35f);
                string expectedExpression = MoonlightHeroEyeQuality.ExpressionNameFor(zone.Kind, true);
                Color expectedExpressionColor = MoonlightHeroEyeQuality.ExpressionColorFor(zone.Kind, true);
                float expressionColorDistance = heroEyeQuality != null
                    ? Vector3.Distance(
                        new Vector3(heroEyeQuality.CurrentCatchlightColor.r,
                            heroEyeQuality.CurrentCatchlightColor.g,
                            heroEyeQuality.CurrentCatchlightColor.b),
                        new Vector3(expectedExpressionColor.r,
                            expectedExpressionColor.g,
                            expectedExpressionColor.b))
                    : float.PositiveInfinity;
                bool expressionPass = heroEyeQuality != null &&
                    heroEyeQuality.CurrentExpressionName == expectedExpression &&
                    heroEyeQuality.CurrentCatchlightEmission >= MoonlightHouseSetup.HeroEyeActionEmission &&
                    expressionColorDistance <= 0.01f;
                if (!expressionPass)
                {
                    Debug.LogError($"[MoonlightVisualQA][FAIL] action={zone.Kind} eye-expression " +
                        $"expected={expectedExpression} actual=" +
                        $"{(heroEyeQuality != null ? heroEyeQuality.CurrentExpressionName : "missing")} " +
                        $"emission={(heroEyeQuality != null ? heroEyeQuality.CurrentCatchlightEmission : 0f):0.00} " +
                        $"colorDistance={expressionColorDistance:0.000}");
                    Application.Quit(76);
                    yield break;
                }
                if (ui != null && ui.resultLabel != null && !string.IsNullOrEmpty(ui.resultLabel.text))
                {
                    Debug.LogError($"[MoonlightVisualQA][FAIL] action={zone.Kind} result appeared before animation completed");
                    Application.Quit(7);
                    yield break;
                }

                var activityStage = moonlight.GetComponent<MoonlightActivityStage>();
                bool requiresVisibleStage = zone.RequiredSteps > 1 ||
                    zone.Kind == MoonlightSpatialActionKind.SleepCuddle;
                if (requiresVisibleStage && (activityStage == null || !activityStage.IsVisible))
                {
                    Debug.LogError($"[MoonlightVisualQA][FAIL] action={zone.Kind} staged activity props missing");
                    Application.Quit(9);
                    yield break;
                }
                bool restingStagePass = ValidateBedtimeRuntimeOracle(feedback, "Resting",
                    false, out string restingStageDetail);
                if (zone.Kind == MoonlightSpatialActionKind.SleepCuddle &&
                    !restingStagePass)
                {
                    Debug.LogError($"[MoonlightVisualQA][FAIL] bedtime-runtime state=Resting " +
                        restingStageDetail);
                    Application.Quit(80);
                    yield break;
                }

                string actionOutput = zone.Kind == MoonlightSpatialActionKind.Play
                    ? output
                    : Path.Combine(directory ?? string.Empty,
                        $"moonlight_action_{zone.Kind.ToString().ToLowerInvariant()}.png");
                yield return Capture(actionOutput);
                Debug.Log($"[MoonlightVisualQA][PASS] action={zone.Kind} prompt=\"{prompt}\" " +
                    $"result=\"{result}\" effect={feedback.ActiveEffectName} " +
                    $"visual={feedback.ActionVisualSignature} accents={feedback.ActionAccentRendererCount} " +
                    $"colliders={feedback.ActionAccentColliderCount} materials={feedback.ActionAccentMaterialCount} " +
                    $"bounds={feedback.ActionAccentBoundsSize:F3} accentExtent={feedback.ActionAccentWorldExtent:0.000} " +
                    $"signatureMarker={feedback.ActionVisualSignatureMarker} " +
                    $"eyeExpression={heroEyeQuality.CurrentExpressionName} " +
                    $"eyeColorDistance={expressionColorDistance:0.000} " +
                    $"marker=MOONLIGHT_ACTIVITY_EYE_EXPRESSION_VERIFIED screenshot={actionOutput}");
                passedActions++;
                float settleDeadline = Time.time + 3f;
                if (zone.Kind == MoonlightSpatialActionKind.SleepCuddle)
                {
                    BedtimeRuntimeObservation restingObservation = bedtimeNaturalObservation;
                    yield return ObserveBedtimeRuntime(feedback, activityStage, "Resting",
                        restingObservation);
                    if (!restingObservation.Passed)
                    {
                        Debug.LogError($"[MoonlightVisualQA][FAIL] bedtime-runtime " +
                            $"state=Resting {restingObservation.Detail}");
                        Application.Quit(81);
                        yield break;
                    }
                    Debug.Log($"[MoonlightVisualQA][PASS] bedtime-runtime state=Resting " +
                        $"{restingObservation.Detail} marker=MOONLIGHT_BEDTIME_RUNTIME_VARIANT_VERIFIED");
                }
                else
                {
                    while ((feedback.IsPerformingAction || feedback.IsCoolingDown) &&
                           Time.time < settleDeadline)
                        yield return null;
                }
                yield return new WaitForSeconds(0.2f);

                for (int step = 1; step < zone.RequiredSteps; step++)
                {
                    string stepPrompt = interactor.CurrentPrompt;
                    string stepResult = interactor.ExecuteCurrent();
                    feedback = moonlight.GetComponent<MoonlightActionFeedback>();
                    activityStage = moonlight.GetComponent<MoonlightActivityStage>();
                    if (feedback == null || !feedback.IsPerformingAction || activityStage == null || !activityStage.IsVisible)
                    {
                        Debug.LogError($"[MoonlightVisualQA][FAIL] action={zone.Kind} step={step + 1} feedback missing");
                        Application.Quit(10);
                        yield break;
                    }
                    expectedVisualSignature = MoonlightActionFeedback.ActionVisualSignatureFor(
                        zone.Kind, step, feedback.StateText);
                    expectedVisualMarker = MoonlightActionFeedback.ActionVisualSignatureMarkerFor(
                        zone.Kind, step, feedback.StateText);
                    if (!ValidateActionAccent(feedback, expectedVisualSignature, expectedVisualMarker,
                            true, out actionAccentDetail))
                    {
                        Debug.LogError($"[MoonlightVisualQA][FAIL] action={zone.Kind} " +
                            $"step={step + 1} composite-contact-prop {actionAccentDetail}");
                        Application.Quit(10);
                        yield break;
                    }
                    yield return new WaitForSeconds(0.55f);
                    if (step == zone.RequiredSteps - 1)
                    {
                        string finalOutput = Path.Combine(directory ?? string.Empty,
                            $"moonlight_activity_{zone.Kind.ToString().ToLowerInvariant()}_complete.png");
                        yield return Capture(finalOutput);
                        Debug.Log($"[MoonlightVisualQA][PASS] activity-step action={zone.Kind} step={step + 1}/{zone.RequiredSteps} " +
                            $"prompt=\"{stepPrompt}\" result=\"{stepResult}\" screenshot={finalOutput}");
                    }
                    settleDeadline = Time.time + 4f;
                    while ((feedback.IsPerformingAction || feedback.IsCoolingDown) && Time.time < settleDeadline)
                        yield return null;
                    yield return new WaitForSeconds(0.2f);
                }

                if (zone.RequiredSteps > 1 && zone.ProgressStep != 0)
                {
                    Debug.LogError($"[MoonlightVisualQA][FAIL] action={zone.Kind} did not complete its {zone.RequiredSteps}-step loop");
                    Application.Quit(11);
                    yield break;
                }

                if (zone.Kind != MoonlightSpatialActionKind.SleepCuddle) continue;

                moonlight.stats.rest = 100f;
                yield return null;
                string cuddlePrompt = interactor.CurrentPrompt;
                var cuddledObservation = new BedtimeRuntimeObservation();
                CaptureBedtimeCameraBaseline(cuddledObservation);
                string cuddleResult = interactor.ExecuteCurrent();
                ui?.ShowContextResult(cuddleResult);
                feedback = moonlight.GetComponent<MoonlightActionFeedback>();
                activityStage = moonlight.GetComponent<MoonlightActivityStage>();
                expectedVisualMarker = MoonlightActionFeedback.ActionVisualSignatureMarkerFor(
                    MoonlightSpatialActionKind.SleepCuddle, 0, "Cuddled");
                if (feedback == null || !feedback.IsPerformingAction ||
                    feedback.ActiveEffectName != "cuddle-orbit" ||
                    !ValidateActionAccent(feedback, "cuddle-heart-pair", expectedVisualMarker,
                        false, out actionAccentDetail))
                {
                    Debug.LogError("[MoonlightVisualQA][FAIL] action=Cuddle animated feedback missing " +
                        $"effect={feedback?.ActiveEffectName ?? "missing"} " +
                        actionAccentDetail);
                    Application.Quit(6);
                    yield break;
                }
                bool cuddledStagePass = ValidateBedtimeRuntimeOracle(feedback, "Cuddled",
                    false, out string cuddledStageDetail);
                if (!cuddledStagePass)
                {
                    Debug.LogError($"[MoonlightVisualQA][FAIL] bedtime-runtime state=Cuddled " +
                        cuddledStageDetail);
                    Application.Quit(82);
                    yield break;
                }
                yield return new WaitForSeconds(0.35f);
                if (ui != null && ui.resultLabel != null && !string.IsNullOrEmpty(ui.resultLabel.text))
                {
                    Debug.LogError("[MoonlightVisualQA][FAIL] action=Cuddle result appeared before animation completed");
                    Application.Quit(8);
                    yield break;
                }
                string cuddleOutput = Path.Combine(directory ?? string.Empty, "moonlight_action_cuddle.png");
                yield return Capture(cuddleOutput);
                Debug.Log($"[MoonlightVisualQA][PASS] action=Cuddle prompt=\"{cuddlePrompt}\" " +
                    $"result=\"{cuddleResult}\" effect={feedback.ActiveEffectName} " +
                    $"visual={feedback.ActionVisualSignature} accent={feedback.ActionAccentRendererCount} " +
                    $"colliders={feedback.ActionAccentColliderCount} materials={feedback.ActionAccentMaterialCount} " +
                    $"bounds={feedback.ActionAccentBoundsSize:F3} accentExtent={feedback.ActionAccentWorldExtent:0.000} " +
                    $"signatureMarker={feedback.ActionVisualSignatureMarker} " +
                    $"screenshot={cuddleOutput}");
                passedActions++;
                yield return ObserveBedtimeRuntime(feedback, activityStage, "Cuddled",
                    cuddledObservation);
                if (!cuddledObservation.Passed)
                {
                    Debug.LogError($"[MoonlightVisualQA][FAIL] bedtime-runtime " +
                        $"state=Cuddled {cuddledObservation.Detail}");
                    Application.Quit(83);
                    yield break;
                }
                Debug.Log($"[MoonlightVisualQA][PASS] bedtime-runtime state=Cuddled " +
                    $"{cuddledObservation.Detail} marker=MOONLIGHT_BEDTIME_RUNTIME_VARIANT_VERIFIED");
                var cleanupObservation = new BedtimeStageCleanupObservation();
                yield return ObserveBedtimeStageCleanupRuntime(controller, zone,
                    activityStage, cleanupObservation);
                if (!cleanupObservation.Passed)
                {
                    Debug.LogError($"[MoonlightVisualQA][FAIL] bedtime-stage-cleanup-runtime " +
                        cleanupObservation.Detail);
                    Application.Quit(84);
                    yield break;
                }
                Debug.Log($"[MoonlightVisualQA][PASS] bedtime-stage-cleanup-runtime " +
                    $"{cleanupObservation.Detail} " +
                    "marker=MOONLIGHT_BEDTIME_STAGE_CLEANUP_PATHS_VERIFIED");
                Debug.Log("[MoonlightVisualQA][PASS] bedtime-runtime-contract variants=2/2 " +
                    "visibleStates=2/2 naturalFeedback=2/2 stageExit=1/1 " +
                    "stageDisable=1/1 stageDestroy=1/1 " +
                    "marker=MOONLIGHT_BEDTIME_RUNTIME_2_OF_2_VERIFIED");
            }

            Debug.Log($"[MoonlightVisualQA][PASS] spatial-suite start={start:F2} end={controller.transform.position:F2} " +
                $"distance={Vector3.Distance(start, controller.transform.position):0.00} actions={passedActions}");
            Application.Quit(0);
        }

        IEnumerator RunGameplayQa(string[] args)
        {
            int pathIndex = System.Array.IndexOf(args, "-moonlightGameplayQaPath");
            string output = pathIndex >= 0 && pathIndex + 1 < args.Length
                ? args[pathIndex + 1]
                : Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
                    "MMH-QA/gameplay-v3");
            Directory.CreateDirectory(output);
            Screen.SetResolution(1366, 1024, false);
            yield return new WaitForSeconds(2.5f);

            var controller = FindAnyObjectByType<MoonlightPlayerController>();
            var rooms = FindAnyObjectByType<RoomManager>();
            var moonlight = FindAnyObjectByType<MoonlightCharacter>();
            var spatialInteractor = moonlight != null
                ? moonlight.GetComponent<MoonlightSpatialInteractor>()
                : null;
            var ui = FindAnyObjectByType<MoonlightUI>();
            var pad = ui != null && ui.actionBtn != null
                ? ui.actionBtn.GetComponent<MoonlightGesturePad>()
                : null;
            var audio = AudioManager.Instance;
            if (controller == null || rooms == null || moonlight == null || spatialInteractor == null ||
                audio == null)
            {
                Debug.LogError("[MoonlightGameplayQA][FAIL] gameplay controller/rooms/character/audio missing");
                Application.Quit(20);
                yield break;
            }
            moonlight.SuppressDecayForQA = true;

            if (!ValidateActivityResultTimingContract(out string resultTimingDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-result-timing " +
                    resultTimingDetail);
                Application.Quit(146);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] activity-result-timing " +
                $"{resultTimingDetail} marker={ActivityResultTimingMarker}");

            if (!MoonlightActivityStage.ValidateSurfaceDepthContract(out string surfaceDepthDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-surface-depth-contract " +
                    surfaceDepthDetail);
                Application.Quit(67);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] activity-surface-depth-contract " +
                $"{surfaceDepthDetail} marker=MOONLIGHT_ACTIVITY_SURFACE_DEPTH_CONTRACT_VERIFIED");
            if (!MoonlightActivityStage.ValidateCookChoreographyContract(
                    out string cookChoreographyDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] cook-choreography-contract " +
                    cookChoreographyDetail);
                Application.Quit(105);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] cook-choreography-contract " +
                $"{cookChoreographyDetail} " +
                "marker=MOONLIGHT_COOK_CHOREOGRAPHY_CONTRACT_VERIFIED");
            if (!MoonlightActivityStage.ValidateGestureResponsiveCookContract(
                    out string responsiveCookDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] gesture-cook-contract " +
                    responsiveCookDetail);
                Application.Quit(116);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] gesture-cook-contract " +
                $"{responsiveCookDetail} " +
                "marker=MOONLIGHT_GESTURE_COOK_CONTRACT_VERIFIED");
            if (!MoonlightUI.ValidateIPadProgressFeedbackContract(out string progressFeedbackDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-progress-feedback-contract " +
                    progressFeedbackDetail);
                Application.Quit(68);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] ipad-progress-feedback-contract " +
                $"{progressFeedbackDetail} marker=MOONLIGHT_IPAD_PROGRESS_FEEDBACK_CONTRACT_VERIFIED");
            if (!MoonlightSpatialActionZone.ValidateCompactIPadInstructionContract(
                    out string compactInstructionDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-compact-instruction-contract " +
                    $"{compactInstructionDetail} " +
                    $"marker={CompactIPadInstructionStaticContractFailureMarker}");
                Application.Quit(163);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] ipad-compact-instruction-contract " +
                $"{compactInstructionDetail} " +
                $"marker={CompactIPadInstructionStaticContractMarker}");
            if (!MoonlightSpatialActionZone.ValidateRetryResultContract(
                    out string retryResultDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] retry-result-contract " +
                    retryResultDetail);
                Application.Quit(167);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] retry-result-contract " +
                $"{retryResultDetail} marker={RetryResultStaticContractMarker}");
            if (!MoonlightUI.ValidateIPadResultFormatterContract(
                    out string resultFormatterDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-result-formatter-contract " +
                    resultFormatterDetail);
                Application.Quit(168);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] ipad-result-formatter-contract " +
                $"{resultFormatterDetail} marker={IPadResultFormatterStaticContractMarker}");
            if (!MoonlightUI.ValidateIPadActivityQualityReadoutContract(
                    out string activityQualityDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] " +
                    $"ipad-activity-quality-static-contract {activityQualityDetail}");
                Application.Quit(152);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] ipad-activity-quality-static-contract " +
                $"{activityQualityDetail} marker={ActivityQualityReadoutStaticContractMarker}");
            if (!MoonlightUI.ValidateFinalActivityCtaSemanticsContract(out string finalCtaDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-final-cta-semantics " +
                    finalCtaDetail);
                Application.Quit(88);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] activity-final-cta-semantics " +
                $"{finalCtaDetail} " +
                "marker=MOONLIGHT_ACTIVITY_FINAL_CTA_SEMANTICS_CONTRACT_VERIFIED");
            if (!MoonlightUI.ValidateIPadNavigationCueContract(out string navigationCueDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-navigation-cue-contract " +
                    navigationCueDetail);
                Application.Quit(89);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] ipad-navigation-cue-contract " +
                $"{navigationCueDetail} marker=MOONLIGHT_IPAD_NAVIGATION_CUE_CONTRACT_VERIFIED");
            if (!MoonlightUI.ValidateActivityPhaseFeedbackContract(out string phaseFeedbackDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-activity-phase-feedback " +
                    phaseFeedbackDetail);
                Application.Quit(77);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] ipad-activity-phase-feedback " +
                $"{phaseFeedbackDetail} marker=MOONLIGHT_IPAD_ACTIVITY_PHASE_FEEDBACK_VERIFIED");
            if (!MoonlightActionFeedback.ValidateActionVisualSignatureContract(
                    out string actionVisualDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] action-visual-signatures " +
                    actionVisualDetail);
                Application.Quit(78);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] action-visual-signatures " +
                $"{actionVisualDetail} marker=MOONLIGHT_ACTIVITY_VISUAL_SIGNATURES_VERIFIED");
            bool bedtimeStageStaticPass = MoonlightActivityStage.ValidateBedtimeStaticContract(
                out string bedtimeStageStaticDetail);
            bool bedtimeAccentStaticPass =
                MoonlightActionFeedback.ValidateBedtimeActionVisualContract(
                    out string bedtimeAccentStaticDetail);
            bool bedtimeViewportStaticPass = ValidateBedtimeViewportStaticContract(
                out string bedtimeViewportStaticDetail);
            if (!bedtimeStageStaticPass || !bedtimeAccentStaticPass ||
                !bedtimeViewportStaticPass)
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] bedtime-static-contract " +
                    $"stage=({bedtimeStageStaticDetail}) " +
                    $"accent=({bedtimeAccentStaticDetail}) " +
                    $"viewports=({bedtimeViewportStaticDetail})");
                Application.Quit(160);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] bedtime-static-contract " +
                $"stage=({bedtimeStageStaticDetail}) accent=({bedtimeAccentStaticDetail}) " +
                $"viewports=({bedtimeViewportStaticDetail}) " +
                "marker=MOONLIGHT_BEDTIME_STATIC_2_OF_2_VERIFIED");
            if (!MoonlightActionFeedback.ValidateActionQualityContract(
                    out string actionQualityDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] action-quality-feedback " +
                    actionQualityDetail);
                Application.Quit(79);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] action-quality-feedback " +
                $"{actionQualityDetail} marker=MOONLIGHT_ACTION_QUALITY_CONTRACT_VERIFIED");

            bool recognizerPass = MoonlightGesturePad.ValidateRecognizerContract(
                out string recognizerDetail);
            if (!recognizerPass)
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] gesture-recognizer {recognizerDetail}");
                Application.Quit(57);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] gesture-recognizer {recognizerDetail} " +
                "marker=MOONLIGHT_GESTURE_RECOGNIZER_VERIFIED");
            if (!MoonlightGesturePad.ValidateGestureSampleContract(out string gestureSampleDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] gesture-sample-contract " +
                    gestureSampleDetail);
                Application.Quit(107);
                yield break;
            }
            if (!MoonlightActivityStage.ValidateGestureResponsivePlayContract(
                    out string responsivePlayDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] gesture-play-contract " +
                    responsivePlayDetail);
                Application.Quit(108);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] gesture-play-contract " +
                $"sample=({gestureSampleDetail}) trajectory=({responsivePlayDetail}) " +
                "marker=MOONLIGHT_GESTURE_PLAY_STATIC_CONTRACT_VERIFIED");
            if (!MoonlightActivityStage.ValidatePlayPhaseLandmarkContract(
                    out string playPhaseLandmarkDetail))
            {
                Debug.LogError("[MoonlightGameplayQA][FAIL] " +
                    $"play-phase-landmark-static {playPhaseLandmarkDetail}");
                Application.Quit(164);
                yield break;
            }
            Debug.Log("[MoonlightGameplayQA][PASS] " +
                $"play-phase-landmark-static {playPhaseLandmarkDetail} " +
                $"marker={MoonlightActivityStage.PlayPhaseLandmarkStaticQAMarker}");
            if (!MoonlightActivityStage.ValidatePlayContinuationClockContract(
                    out string playContinuationClockDetail))
            {
                Debug.LogError("[MoonlightGameplayQA][FAIL] " +
                    $"play-continuation-clock-static {playContinuationClockDetail}");
                Application.Quit(162);
                yield break;
            }
            Debug.Log("[MoonlightGameplayQA][PASS] " +
                $"play-continuation-clock-static {playContinuationClockDetail}");
            if (!MoonlightActivityStage.ValidateGestureResponsiveGardenContract(
                    out string responsiveGardenDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] gesture-garden-contract " +
                    responsiveGardenDetail);
                Application.Quit(148);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] gesture-garden-contract " +
                $"sample=({gestureSampleDetail}) garden=({responsiveGardenDetail}) " +
                "marker=MOONLIGHT_GESTURE_GARDEN_STATIC_CONTRACT_VERIFIED");
            if (!MoonlightActivityStage.ValidateGestureResponsiveReadContract(
                    out string responsiveReadDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] gesture-read-contract " +
                    $"{responsiveReadDetail} marker={GestureReadStaticContractFailureMarker}");
                Application.Quit(152);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] gesture-read-contract " +
                $"sample=({gestureSampleDetail}) read=({responsiveReadDetail}) " +
                $"marker={GestureReadStaticContractMarker}");
            if (!MoonlightActivityStage.ValidateGestureResponsiveCareContract(
                    out string responsiveCareDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] gesture-care-contract " +
                    $"{responsiveCareDetail} marker={GestureCareStaticContractFailureMarker}");
                Application.Quit(156);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] gesture-care-contract " +
                $"sample=({gestureSampleDetail}) care=({responsiveCareDetail}) " +
                $"marker={GestureCareStaticContractMarker}");
            bool uiTransactionPass = MoonlightUI.ValidateQATransactionSnapshotContract(
                out string uiTransactionDetail);
            bool padTransactionPass = MoonlightGesturePad.ValidateQATransactionSnapshotContract(
                out string padTransactionDetail);
            if (!uiTransactionPass || !padTransactionPass)
            {
                Debug.LogError("[MoonlightGameplayQA][FAIL] care-live-qa-transaction " +
                    $"ui=({uiTransactionDetail}) pad=({padTransactionDetail})");
                Application.Quit(157);
                yield break;
            }
            Debug.Log("[MoonlightGameplayQA][PASS] care-live-qa-transaction " +
                $"ui=({uiTransactionDetail}) pad=({padTransactionDetail}) " +
                "marker=MOONLIGHT_CARE_LIVE_QA_TRANSACTION_STATIC_VERIFIED");
            if (!MoonlightGesturePad.ValidateLiveHoldReadinessStaticContract(
                    out string liveHoldReadinessDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-live-hold-readiness " +
                    liveHoldReadinessDetail);
                Application.Quit(106);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] ipad-live-hold-readiness " +
                $"{liveHoldReadinessDetail} " +
                "marker=MOONLIGHT_IPAD_LIVE_HOLD_STATIC_CONTRACT_VERIFIED " +
                "marker=MOONLIGHT_IPAD_LIVE_HOLD_4_OF_4_STATIC_VERIFIED");
            if (!MoonlightSpatialActionZone.ValidateGestureResponsiveBedtimeContract(
                    out string bedtimeDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] gesture-bedtime-contract " +
                    bedtimeDetail);
                Application.Quit(159);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] gesture-bedtime-contract " +
                $"{bedtimeDetail} " +
                "marker=MOONLIGHT_GESTURE_BEDTIME_TRANSACTION_VERIFIED");
            if (!MoonlightGesturePad.ValidateContextGestureMovementLockStaticContract(
                    out string gestureMovementLockDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-context-gesture-movement-lock " +
                    gestureMovementLockDetail);
                Application.Quit(143);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] ipad-context-gesture-movement-lock " +
                $"{gestureMovementLockDetail} " +
                "marker=MOONLIGHT_IPAD_CONTEXT_GESTURE_MOVEMENT_LOCK_STATIC_VERIFIED");
            bool freeHopControllerStatic =
                MoonlightPlayerController.ValidateFreeHopStaticContract(
                    out string freeHopControllerDetail);
            bool freeHopGestureStatic = MoonlightGesturePad.ValidateFreeHopGestureContract(
                out string freeHopGestureDetail);
            bool freeHopUIStatic = MoonlightUI.ValidateFreeHopUIContract(
                out string freeHopUIDetail);
            if (!freeHopControllerStatic || !freeHopGestureStatic || !freeHopUIStatic)
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-free-hop-static " +
                    $"controller=({freeHopControllerDetail}) gesture=({freeHopGestureDetail}) " +
                    $"ui=({freeHopUIDetail})");
                Application.Quit(132);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] ipad-free-hop-static " +
                $"controller=({freeHopControllerDetail}) gesture=({freeHopGestureDetail}) " +
                $"ui=({freeHopUIDetail}) marker=MOONLIGHT_IPAD_FREE_HOP_STATIC_VERIFIED");
            if (!MoonlightGesturePad.ValidateIPadCoordinateContract(out string coordinateDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] gesture-coordinates {coordinateDetail}");
                Application.Quit(66);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] gesture-coordinates {coordinateDetail} " +
                "marker=MOONLIGHT_GESTURE_COORDINATES_VERIFIED");
            if (!MoonlightGesturePad.ValidateGestureGuideContract(out string guideDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-gesture-guide {guideDetail}");
                Application.Quit(71);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] ipad-gesture-guide {guideDetail} " +
                "marker=MOONLIGHT_IPAD_GESTURE_GUIDE_VERIFIED");
            if (!MoonlightGesturePad.ValidateResultFeedbackContract(out string resultFeedbackDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-gesture-result-feedback " +
                    resultFeedbackDetail);
                Application.Quit(69);
                yield break;
            }
            bool resultFeedbackPadComponentReady = pad != null && pad.enabled;
            bool resultOverlayReady = resultFeedbackPadComponentReady && pad.ResultOverlayIsReady;
            string resultFeedbackMarker = resultFeedbackPadComponentReady
                ? pad.ResultFeedbackQAMarker
                : "missing";
            if (!resultFeedbackPadComponentReady || !resultOverlayReady ||
                resultFeedbackMarker != "MOONLIGHT_IPAD_GESTURE_RESULT_FEEDBACK_READY")
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-gesture-result-feedback-runtime " +
                    $"padComponentReady={resultFeedbackPadComponentReady} " +
                    $"overlayReady={resultOverlayReady} " +
                    $"marker={resultFeedbackMarker}");
                Application.Quit(91);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] ipad-gesture-result-feedback " +
                $"{resultFeedbackDetail} overlayReady={resultOverlayReady} " +
                $"runtimeMarker={resultFeedbackMarker} " +
                "marker=MOONLIGHT_IPAD_GESTURE_RESULT_FEEDBACK_VERIFIED");
            if (!HapticFeedback.ValidateSemanticContract(out string hapticDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-haptic-semantics {hapticDetail}");
                Application.Quit(70);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] activity-haptic-semantics {hapticDetail} " +
                "marker=MOONLIGHT_ACTIVITY_HAPTIC_SEMANTICS_VERIFIED");
            if (!MoonlightSpatialActionZone.ValidateMasteryContract(out string masteryDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-mastery {masteryDetail}");
                Application.Quit(59);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] activity-mastery {masteryDetail} " +
                "marker=MOONLIGHT_ACTIVITY_MASTERY_CONTRACT_VERIFIED");
            if (!MoonlightSpatialActionZone.ValidateRewardReceiptContract(
                    out string rewardReceiptDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-reward-receipt " +
                    rewardReceiptDetail);
                Application.Quit(93);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] activity-reward-receipt " +
                $"{rewardReceiptDetail} " +
                "marker=MOONLIGHT_ACTIVITY_REWARD_RECEIPT_CONTRACT_VERIFIED");
            if (!MoonlightSpatialActionZone.ValidateFeedStatDeltaAndRejectionContract(
                    out string feedStatDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] feed-stat-rejection-contract " +
                    feedStatDetail);
                Application.Quit(123);
                yield break;
            }
            if (!MoonlightActionFeedback.ValidateFeedVisualContract(out string feedVisualDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] feed-visual-contract " +
                    feedVisualDetail);
                Application.Quit(124);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] feed-static-contracts {feedStatDetail} " +
                $"visual=({feedVisualDetail}) marker=MOONLIGHT_FEED_STATIC_CONTRACT_VERIFIED");
            if (!LibraryRoom.TryLoadAuthoredStories(out AuthoredStoryPage[] storyPages,
                    out string storyDataDetail) || storyPages.Length != 10)
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] story-data-contract {storyDataDetail}");
                Application.Quit(110);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] story-data-contract {storyDataDetail} " +
                $"marker={LibraryRoom.StoryDataReadyMarker}");
            if (!StoryPageUI.ValidateStaticContract(out string storyUIContractDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] story-ui-contract {storyUIContractDetail}");
                Application.Quit(111);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] story-ui-contract {storyUIContractDetail} " +
                $"markers={StoryPageUI.TimingMarker},{StoryPageUI.SafeAreaMarker}," +
                $"{StoryPageUI.NonOverflowMarker}");
            if (!MoonlightPlayerController.ValidateStoryModalInputContract(
                    out string storyModalDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] story-modal-contract {storyModalDetail}");
                Application.Quit(112);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] story-modal-contract {storyModalDetail} " +
                $"marker={StoryPageUI.ModalLockMarker}");
            if (!MoonlightSpatialActionZone.ValidateReadStoryRewardContract(
                    out string storyRewardDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] story-reward-contract {storyRewardDetail}");
                Application.Quit(113);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] story-reward-contract {storyRewardDetail} " +
                $"marker={StoryPageUI.RewardPathMarker}");
            if (!MoonlightSpatialActionZone.ValidateCareSequenceContract(
                    out string careSequenceDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] care-sequence " +
                    careSequenceDetail);
                Application.Quit(94);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] care-sequence {careSequenceDetail} " +
                "marker=MOONLIGHT_CARE_SEQUENCE_CONTRACT_VERIFIED");
            if (!MoonlightCharacter.ValidateCareRewardContract(out string careRewardDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] care-reward " +
                    careRewardDetail);
                Application.Quit(95);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] care-reward {careRewardDetail} " +
                "marker=MOONLIGHT_CARE_REWARD_CONTRACT_VERIFIED");
            if (!MoonlightActionFeedback.ValidateMasteryCelebrationContract(out string celebrationDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] mastery-celebration {celebrationDetail}");
                Application.Quit(61);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] mastery-celebration {celebrationDetail} " +
                "marker=MOONLIGHT_MASTERY_CELEBRATION_CONTRACT_VERIFIED");
            if (!MoonlightTouchJoystick.ValidateResponseContract(out string joystickResponseDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-joystick-response {joystickResponseDetail}");
                Application.Quit(65);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] ipad-joystick-response {joystickResponseDetail} " +
                "marker=MOONLIGHT_IPAD_JOYSTICK_RESPONSE_CONTRACT_VERIFIED");
            if (!MoonlightPlayerController.ValidateTouchCameraRelativeContract(
                    out string cameraRelativeTouchDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] touch-camera-relative-contract " +
                    cameraRelativeTouchDetail);
                Application.Quit(90);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] touch-camera-relative-contract " +
                $"{cameraRelativeTouchDetail} " +
                $"marker={MoonlightPlayerController.TouchCameraRelativeContractMarker}");
            if (!controller.ValidateIPadSprintRuntimeContract(out string sprintDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-sprint-contract {sprintDetail}");
                Application.Quit(85);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] ipad-sprint-contract {sprintDetail} " +
                "marker=MOONLIGHT_IPAD_SPRINT_CONTRACT_VERIFIED");
            if (!ValidateMoonbudLocomotionSourceContract(out string locomotionSourceDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] moonbud-locomotion-source " +
                    locomotionSourceDetail);
                Application.Quit(103);
                yield break;
            }
            bool photorealMode = IsPhotorealMode(args);
            if (!ValidateMoonbudLocomotionRuntimeContract(controller, photorealMode,
                    out string locomotionRuntimeDetail, out string locomotionMarker))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] moonbud-locomotion-runtime " +
                    $"source=({locomotionSourceDetail}) runtime=({locomotionRuntimeDetail})");
                Application.Quit(104);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] moonbud-articulated-locomotion " +
                $"source=({locomotionSourceDetail}) runtime=({locomotionRuntimeDetail}) " +
                $"marker={locomotionMarker}");
            if (!pad.TracePoolIsReady ||
                pad.TraceDotPoolCount != MoonlightGesturePad.GestureTraceDotCapacity ||
                !pad.GuidePoolIsReady ||
                pad.GuideDotPoolCount != MoonlightGesturePad.GestureGuideDotCapacity)
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] gesture-trace poolReady={pad.TracePoolIsReady} " +
                    $"pool={pad.TraceDotPoolCount}/{MoonlightGesturePad.GestureTraceDotCapacity} " +
                    $"guideReady={pad.GuidePoolIsReady} " +
                    $"guidePool={pad.GuideDotPoolCount}/{MoonlightGesturePad.GestureGuideDotCapacity}");
                Application.Quit(58);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] gesture-trace pool={pad.TraceDotPoolCount} " +
                $"guidePool={pad.GuideDotPoolCount} pooled=True raycastBlocking=False " +
                "marker=MOONLIGHT_GESTURE_TRACE_READY MOONLIGHT_IPAD_GESTURE_GUIDE_READY");

            bool expectIPadHud = args.Contains("-moonlightIPadHudQa");
            bool verifyLiveHoldRuntime = expectIPadHud &&
                args.Contains("-moonlightLiveHoldRuntimeQa");
            int verifiedLiveHoldRuntimeActions = 0;
            var verifiedGestureLockCleanup = new System.Collections.Generic.HashSet<string>();
            int verifiedRejectedGestureMovementRetention = 0;
            var touchJoystick = FindAnyObjectByType<MoonlightTouchJoystick>();
            if (expectIPadHud)
            {
                Vector3 sprintProbeStart = controller.transform.position;
                int sprintProbeCollisionCount = controller.CollisionCount;
                int sprintProbeRecoveryCount = controller.RecoveryCount;
                var sprintProbeFeedback = controller.GetComponent<MoonlightActionFeedback>();
                bool sprintProbeActivityIdle = sprintProbeFeedback == null ||
                    !sprintProbeFeedback.IsPerformingAction;

                controller.SetProcessedTouchSprintForQA(Vector2.right * 0.91f);
                bool processed91Pass = !controller.IsIPadSprinting &&
                    Mathf.Abs(controller.TouchMove.magnitude - 0.91f) <= 0.0001f &&
                    Mathf.Abs(controller.CurrentMoveSpeed - controller.BaseMoveSpeed) <= 0.0001f;
                float processed91Speed = controller.CurrentMoveSpeed;
                sprintProbeActivityIdle &= sprintProbeFeedback == null ||
                    !sprintProbeFeedback.IsPerformingAction;

                controller.SetProcessedTouchSprintForQA(Vector2.right * 0.92f);
                bool processed92Pass = controller.IsIPadSprinting &&
                    Mathf.Abs(controller.TouchMove.magnitude - 0.92f) <= 0.0001f &&
                    Mathf.Abs(controller.CurrentMoveSpeed - 3.77f) <= 0.0001f &&
                    controller.CurrentMoveSpeed <= 3.7701f;
                float processed92Speed = controller.CurrentMoveSpeed;

                controller.SetProcessedTouchSprintForQA(Vector2.right);
                bool processedMaximumPass = controller.IsIPadSprinting &&
                    Mathf.Abs(controller.TouchMove.magnitude - 1f) <= 0.0001f &&
                    Mathf.Abs(controller.CurrentMoveSpeed - 3.77f) <= 0.0001f;
                float oneSecondSprintDistance = controller.CurrentMoveSpeed;
                sprintProbeActivityIdle &= sprintProbeFeedback == null ||
                    !sprintProbeFeedback.IsPerformingAction;

                controller.SetProcessedTouchSprintForQA(Vector2.zero);
                bool neutralStatePass = !controller.IsIPadSprinting &&
                    controller.TouchMove.sqrMagnitude <= 0.0001f &&
                    Mathf.Abs(controller.CurrentMoveSpeed - controller.BaseMoveSpeed) <= 0.0001f;
                float probeDrift = Vector3.Distance(sprintProbeStart, controller.transform.position);
                bool navigationStateUnchanged = controller.CollisionCount == sprintProbeCollisionCount &&
                    controller.RecoveryCount == sprintProbeRecoveryCount;
                sprintProbeActivityIdle &= sprintProbeFeedback == null ||
                    !sprintProbeFeedback.IsPerformingAction;
                bool sprintProbePass = processed91Pass && processed92Pass &&
                    processedMaximumPass && neutralStatePass &&
                    Mathf.Abs(oneSecondSprintDistance - 3.77f) <= 0.0001f &&
                    probeDrift <= 0.0001f && navigationStateUnchanged && sprintProbeActivityIdle;
                if (!sprintProbePass)
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-sprint-live " +
                        $"processed91={processed91Pass}/{processed91Speed:0.00} " +
                        $"processed92={processed92Pass}/{processed92Speed:0.00} " +
                        $"processedMaximum={processedMaximumPass}/{oneSecondSprintDistance:0.00} " +
                        $"neutral={neutralStatePass} drift={probeDrift:0.0000} " +
                        $"navigationState={navigationStateUnchanged} " +
                        $"activityIdle={sprintProbeActivityIdle} touch={controller.TouchMove:F3} " +
                        $"base={controller.BaseMoveSpeed:0.00} current={controller.CurrentMoveSpeed:0.00}");
                    Application.Quit(86);
                    yield break;
                }
                Debug.Log($"[MoonlightGameplayQA][PASS] ipad-sprint-live " +
                    $"processed91=false/{processed91Speed:0.00} " +
                    $"processed92=true/{processed92Speed:0.00} " +
                    $"oneSecondDistance={oneSecondSprintDistance:0.00}m " +
                    $"after=false/{controller.CurrentMoveSpeed:0.00} drift={probeDrift:0.0000} " +
                    "activityStarted=false marker=MOONLIGHT_IPAD_SPRINT_LIVE_VERIFIED");

                if (touchJoystick == null)
                {
                    Debug.LogError("[MoonlightGameplayQA][FAIL] ipad-joystick-pause-release " +
                        "joystick=missing");
                    Application.Quit(87);
                    yield break;
                }

                touchJoystick.Bind(controller);
                touchJoystick.ArmHeldInputForQA(Vector2.right);
                controller.SetProcessedTouchSprintForQA(touchJoystick.Value);
                bool pauseInputArmed = touchJoystick.IsTrackingPointer &&
                    touchJoystick.Value.sqrMagnitude > 0.0001f &&
                    touchJoystick.KnobAnchoredPosition.sqrMagnitude > 0.0001f &&
                    controller.TouchMove.sqrMagnitude > 0.0001f &&
                    controller.IsIPadSprinting;
                int pauseResetSequenceBefore = touchJoystick.ResetSequence;
                touchJoystick.SimulateApplicationPauseForQA();
                int pauseResetSequenceAfter = touchJoystick.ResetSequence;
                touchJoystick.SimulateApplicationPauseForQA();
                bool pauseReleasePass = pauseInputArmed &&
                    !touchJoystick.IsTrackingPointer &&
                    touchJoystick.Value.sqrMagnitude <= 0.0001f &&
                    touchJoystick.KnobAnchoredPosition.sqrMagnitude <= 0.0001f &&
                    controller.TouchMove.sqrMagnitude <= 0.0001f &&
                    !controller.IsIPadSprinting &&
                    touchJoystick.LastResetReason == "paused" &&
                    pauseResetSequenceAfter == pauseResetSequenceBefore + 1 &&
                    touchJoystick.ResetSequence == pauseResetSequenceAfter &&
                    touchJoystick.PauseReleaseQAMarker ==
                        "MOONLIGHT_IPAD_JOYSTICK_PAUSE_RELEASE_VERIFIED";
                if (!pauseReleasePass)
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-joystick-pause-release " +
                        $"armed={pauseInputArmed} pointer={touchJoystick.IsTrackingPointer} " +
                        $"value={touchJoystick.Value:F3} knob={touchJoystick.KnobAnchoredPosition:F3} " +
                        $"touch={controller.TouchMove:F3} sprint={controller.IsIPadSprinting} " +
                        $"reason={touchJoystick.LastResetReason} " +
                        $"sequence={pauseResetSequenceBefore}/{pauseResetSequenceAfter}/" +
                        $"{touchJoystick.ResetSequence} marker={touchJoystick.PauseReleaseQAMarker}");
                    Application.Quit(87);
                    yield break;
                }
                Debug.Log($"[MoonlightGameplayQA][PASS] ipad-joystick-pause-release " +
                    $"armed={pauseInputArmed} pointer={touchJoystick.IsTrackingPointer} " +
                    $"value={touchJoystick.Value:F3} knob={touchJoystick.KnobAnchoredPosition:F3} " +
                    $"touch={controller.TouchMove:F3} sprint={controller.IsIPadSprinting} " +
                    $"reason={touchJoystick.LastResetReason} " +
                    $"sequence={pauseResetSequenceBefore}/{pauseResetSequenceAfter}/" +
                    $"{touchJoystick.ResetSequence} " +
                    "marker=MOONLIGHT_IPAD_JOYSTICK_PAUSE_RELEASE_VERIFIED");

                var activeNavigationZones = FindObjectsByType<MoonlightSpatialActionZone>(
                        FindObjectsSortMode.None)
                    .Where(candidate => candidate != null && candidate.gameObject.activeInHierarchy)
                    .ToArray();
                Rect navigationBounds = controller.RoomBounds;
                var navigationCandidates = new[]
                {
                    new Vector3(Mathf.Lerp(navigationBounds.xMin, navigationBounds.xMax, 0.2f), 0f,
                        Mathf.Lerp(navigationBounds.yMin, navigationBounds.yMax, 0.2f)),
                    new Vector3(Mathf.Lerp(navigationBounds.xMin, navigationBounds.xMax, 0.8f), 0f,
                        Mathf.Lerp(navigationBounds.yMin, navigationBounds.yMax, 0.2f)),
                    new Vector3(Mathf.Lerp(navigationBounds.xMin, navigationBounds.xMax, 0.2f), 0f,
                        Mathf.Lerp(navigationBounds.yMin, navigationBounds.yMax, 0.8f)),
                    new Vector3(Mathf.Lerp(navigationBounds.xMin, navigationBounds.xMax, 0.8f), 0f,
                        Mathf.Lerp(navigationBounds.yMin, navigationBounds.yMax, 0.8f))
                };
                Vector3 navigationTestPosition = navigationCandidates.FirstOrDefault(candidate =>
                    activeNavigationZones.All(zone => Vector2.Distance(
                        new Vector2(candidate.x, candidate.z),
                        new Vector2(zone.transform.position.x, zone.transform.position.z)) >
                        zone.Radius + 0.35f));
                bool hasExplicitOutOfRangePosition = activeNavigationZones.Length > 0 &&
                    navigationTestPosition != Vector3.zero;
                if (hasExplicitOutOfRangePosition)
                {
                    controller.TeleportTo(navigationTestPosition, navigationBounds);
                    yield return new WaitForSeconds(0.65f);
                }
                Canvas.ForceUpdateCanvases();
                yield return new WaitForEndOfFrame();
                var cameraTransform = Camera.main != null ? Camera.main.transform : null;
                Vector2 expectedCameraDirection = cameraTransform != null
                    ? MoonlightUI.CameraRelativeNavigationDirection(
                        spatialInteractor.NearestZoneDirectionXZ,
                        new Vector2(cameraTransform.forward.x, cameraTransform.forward.z),
                        new Vector2(cameraTransform.right.x, cameraTransform.right.z))
                    : Vector2.zero;
                float expectedNavigationAngle =
                    MoonlightUI.CameraRelativeNavigationAngle(expectedCameraDirection);
                string expectedNavigationLabel = spatialInteractor.NearestZone != null
                    ? MoonlightUI.CompactNavigationLabel(spatialInteractor.NearestZone.Kind)
                    : "";
                bool navigationCuePass = hasExplicitOutOfRangePosition &&
                    spatialInteractor.HasNavigationTarget &&
                    Mathf.Abs(spatialInteractor.NearestZoneDirectionXZ.magnitude - 1f) <= 0.001f &&
                    ui.IsIPadNavigationCueVisible && ui.NavigationCueIsInsideSafeArea &&
                    ui.NavigationCueDoesNotOverlapJoystick &&
                    ui.NavigationCueDoesNotOverlapActionTarget &&
                    ui.NavigationCueGraphicsDoNotBlockRaycasts &&
                    ui.NavigationCueCameraRelativeDirection.sqrMagnitude >= 0.999f &&
                    Vector2.Distance(ui.NavigationCueCameraRelativeDirection,
                        expectedCameraDirection) <= 0.02f &&
                    !float.IsNaN(ui.NavigationCueAngleDegrees) &&
                    !float.IsInfinity(ui.NavigationCueAngleDegrees) &&
                    Mathf.Abs(Mathf.DeltaAngle(ui.NavigationCueAngleDegrees,
                        expectedNavigationAngle)) <= 0.5f &&
                    Mathf.Abs(Mathf.DeltaAngle(ui.NavigationCueVisualAngleDegrees,
                        expectedNavigationAngle)) <= 0.5f &&
                    ui.NavigationCueTargetName == spatialInteractor.NearestZone.DisplayName &&
                    ui.NavigationCueRenderedLabel == expectedNavigationLabel &&
                    MoonlightUI.IsValidCompactNavigationLabel(ui.NavigationCueRenderedLabel) &&
                    Mathf.Abs(ui.NavigationCueTargetDistance -
                        spatialInteractor.NearestDistance) <= 0.05f &&
                    ui.NavigationCueQAMarker == "MOONLIGHT_IPAD_NAVIGATION_CUE_READY";
                bool layoutPass = ui.IsIPadHUDLayoutActive &&
                    ui.HUDLayoutQAMarker == "ipad-activity-focus-v4" &&
                    ui.VisibleHUDTypographyQAMarker == "MOONLIGHT_VISIBLE_TMP_HUD_READY" &&
                    ui.ActionTouchTargetMeetsIPadMinimum &&
                    ui.ActionTouchTargetIsInsideSafeArea &&
                    ui.ActivityPromptIsInsideSafeArea &&
                    ui.ActivityResultIsInsideSafeArea &&
                    ui.ActivityProgressIsInsideSafeArea &&
                    ui.ActivityQualityReadoutIsInsideSafeArea &&
                    ui.ActivityQualityReadoutDoesNotOverlapPanels &&
                    ui.ActivityQualityReadoutGraphicsDoNotBlockRaycasts &&
                    ui.VisibleActionTextIsInsideSafeArea &&
                    ui.VisibleActionTextDoesNotOverflow &&
                    ui.ActivityHUDPanelsDoNotOverlap &&
                    ui.ActivityPromptCenterOffsetPixels <= Screen.width * 0.10f &&
                    pad.CoordinateQAMarker == "MOONLIGHT_GESTURE_COORDINATES_ISOTROPIC" &&
                    touchJoystick != null && touchJoystick.gameObject.activeInHierarchy &&
                    touchJoystick.ResponseQAMarker == "MOONLIGHT_IPAD_JOYSTICK_RESPONSE_READY" &&
                    navigationCuePass &&
                    ui.IsRoomNavigationVisible && !ui.IsRoomNavigationLocked;
                if (!layoutPass)
                {
                    Debug.LogError("[MoonlightGameplayQA][FAIL] ipad-hud-layout " +
                        $"active={ui.IsIPadHUDLayoutActive} marker={ui.HUDLayoutQAMarker} " +
                        $"typography={ui.VisibleHUDTypographyQAMarker} " +
                        $"tmpLabels={ui.VisibleTMPHUDLabelCount}/{MoonlightUI.RequiredVisibleTMPHUDLabelCount} " +
                        $"careLabels={ui.VisibleCareHUDLabelCount}/{MoonlightUI.RequiredVisibleCareHUDLabelCount} " +
                        $"runtimeLabels={ui.VisibleRuntimeHUDLabelCount}/{MoonlightUI.RequiredVisibleRuntimeHUDLabelCount} " +
                        $"font={ui.RuntimeUIFontSourceForQA} " +
                        $"typographySafe={ui.VisibleTMPHUDLabelsInsideSafeArea} " +
                        $"typographySeparated={ui.VisibleTMPHUDLabelsDoNotOverlap} " +
                        $"noMirror={ui.VisibleHUDHasNoLegacyMirrorDependency} " +
                        $"touch={ui.ActionTouchTargetLayoutSize} minimum={ui.IPadMinimumTouchTargetLayoutSize} " +
                        $"insideSafe={ui.ActionTouchTargetIsInsideSafeArea} " +
                        $"promptSafe={ui.ActivityPromptIsInsideSafeArea} resultSafe={ui.ActivityResultIsInsideSafeArea} " +
                        $"actionTextSafe={ui.VisibleActionTextIsInsideSafeArea} " +
                        $"actionTextNonOverflow={ui.VisibleActionTextDoesNotOverflow} " +
                        $"progressSafe={ui.ActivityProgressIsInsideSafeArea} " +
                        $"qualitySafe={ui.ActivityQualityReadoutIsInsideSafeArea} " +
                        $"qualitySeparated={ui.ActivityQualityReadoutDoesNotOverlapPanels} " +
                        $"qualityRaycasts={ui.ActivityQualityReadoutGraphicsDoNotBlockRaycasts} " +
                        $"separated={ui.ActivityHUDPanelsDoNotOverlap} " +
                        $"gestureCoordinates={pad.CoordinateQAMarker} gestureSurface={pad.TouchSurfaceSize} " +
                        $"touchJoystick={(touchJoystick != null && touchJoystick.gameObject.activeInHierarchy)} " +
                        $"joystickMarker={(touchJoystick != null ? touchJoystick.ResponseQAMarker : "missing")} " +
                        $"explicitOutOfRange={hasExplicitOutOfRangePosition} " +
                        $"navigationTarget={spatialInteractor.HasNavigationTarget} " +
                        $"direction={spatialInteractor.NearestZoneDirectionXZ:F3} " +
                        $"cueVisible={ui.IsIPadNavigationCueVisible} cueSafe={ui.NavigationCueIsInsideSafeArea} " +
                        $"cueJoystickSeparated={ui.NavigationCueDoesNotOverlapJoystick} " +
                        $"cueActionSeparated={ui.NavigationCueDoesNotOverlapActionTarget} " +
                        $"cueRect={ui.NavigationCueScreenRect} actionRect={ui.ActionTouchTargetScreenRect} " +
                        $"cueRaycasts={ui.NavigationCueGraphicsDoNotBlockRaycasts} " +
                        $"cueAngle={ui.NavigationCueAngleDegrees:0.0}/" +
                        $"{ui.NavigationCueVisualAngleDegrees:0.0}/{expectedNavigationAngle:0.0} " +
                        $"cueTarget={ui.NavigationCueTargetName}/{ui.NavigationCueTargetDistance:0.0}m " +
                        $"cueLabel={ui.NavigationCueRenderedLabel}/{expectedNavigationLabel} " +
                        $"cueLabelLength={ui.NavigationCueRenderedLabel.Length} " +
                        $"cueLabelValid={MoonlightUI.IsValidCompactNavigationLabel(ui.NavigationCueRenderedLabel)} " +
                        $"cueMarker={ui.NavigationCueQAMarker} " +
                        $"promptOffset={ui.ActivityPromptCenterOffsetPixels:0.0} " +
                        $"roomNav={ui.RoomNavigationQAMarker}");
                    Application.Quit(42);
                    yield break;
                }
                Debug.Log("[MoonlightGameplayQA][PASS] ipad-hud-layout " +
                    $"marker={ui.HUDLayoutQAMarker} touch={ui.ActionTouchTargetLayoutSize} " +
                    $"typography={ui.VisibleHUDTypographyQAMarker} " +
                    $"tmpLabels={ui.VisibleTMPHUDLabelCount}/{MoonlightUI.RequiredVisibleTMPHUDLabelCount} " +
                    $"careLabels={ui.VisibleCareHUDLabelCount}/{MoonlightUI.RequiredVisibleCareHUDLabelCount} " +
                    $"runtimeLabels={ui.VisibleRuntimeHUDLabelCount}/{MoonlightUI.RequiredVisibleRuntimeHUDLabelCount} " +
                    $"font={ui.RuntimeUIFontSourceForQA} " +
                    $"typographySafe={ui.VisibleTMPHUDLabelsInsideSafeArea} " +
                    $"typographySeparated={ui.VisibleTMPHUDLabelsDoNotOverlap} " +
                    $"noMirror={ui.VisibleHUDHasNoLegacyMirrorDependency} " +
                    $"safe={ui.HUDSafeAreaScreenRect} panelsSeparated={ui.ActivityHUDPanelsDoNotOverlap} " +
                    $"gestureCoordinates={pad.CoordinateQAMarker} gestureSurface={pad.TouchSurfaceSize} " +
                    $"touchJoystick={touchJoystick.gameObject.activeInHierarchy} " +
                    $"joystick={touchJoystick.ResponseQAMarker} size={touchJoystick.TouchTargetSize} " +
                    $"navigationDirection={spatialInteractor.NearestZoneDirectionXZ:F3} " +
                    $"cueRect={ui.NavigationCueScreenRect} actionRect={ui.ActionTouchTargetScreenRect} " +
                    $"cueJoystickSeparated={ui.NavigationCueDoesNotOverlapJoystick} " +
                    $"cueActionSeparated={ui.NavigationCueDoesNotOverlapActionTarget} " +
                    $"cueAngle={ui.NavigationCueAngleDegrees:0.0}/" +
                    $"{ui.NavigationCueVisualAngleDegrees:0.0}/{expectedNavigationAngle:0.0} " +
                    $"cueTarget={ui.NavigationCueTargetName}/{ui.NavigationCueTargetDistance:0.0}m " +
                    $"cueLabel={ui.NavigationCueRenderedLabel}/{expectedNavigationLabel} " +
                    $"cueLabelLength={ui.NavigationCueRenderedLabel.Length} " +
                    $"cueLabelValid={MoonlightUI.IsValidCompactNavigationLabel(ui.NavigationCueRenderedLabel)} " +
                    $"cueMarker={ui.NavigationCueQAMarker} " +
                    $"promptOffset={ui.ActivityPromptCenterOffsetPixels:0.0} " +
                    $"roomNav={ui.RoomNavigationQAMarker} " +
                    "marker=MOONLIGHT_IPAD_HUD_VERIFIED");

                Canvas.ForceUpdateCanvases();
                yield return null;
                bool freeHopAvailabilityPass = spatialInteractor.CurrentZone == null &&
                    ui.FreeHopAvailable && ui.ActionButtonQAInteractable &&
                    ui.ActionButtonQAText == "TAP\nHOP" &&
                    ui.ActionTouchTargetIsInsideSafeArea &&
                    ui.VisibleActionTextDoesNotOverflow &&
                    ui.FreeHopQAMarker == "MOONLIGHT_IPAD_FREE_HOP_UI_READY";
                if (!freeHopAvailabilityPass)
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-free-hop-availability " +
                        $"zone={spatialInteractor.CurrentZone?.DisplayName ?? "none"} " +
                        $"available={ui.FreeHopAvailable} reason={ui.FreeHopBlockReason} " +
                        $"interactable={ui.ActionButtonQAInteractable} " +
                        $"label={ui.ActionButtonQAText.Replace('\n', '/')} " +
                        $"safe={ui.ActionTouchTargetIsInsideSafeArea} " +
                        $"overflow={ui.VisibleActionTextDoesNotOverflow} marker={ui.FreeHopQAMarker}");
                    Application.Quit(133);
                    yield break;
                }

                int freeHopStartsBefore = controller.FreeHopStartCount;
                bool lowTapRejected = !pad.SubmitSynthetic(MoonlightGestureKind.Tap,
                    MoonlightGesturePad.FreeHopTapPassingScore - 0.01f) &&
                    pad.LastRejectionReason == "TAP TO HOP" &&
                    controller.FreeHopStartCount == freeHopStartsBefore;
                bool desktopRejected = !controller.TryBeginFreeHop(false, false, false,
                    false, out string desktopHopReason) && desktopHopReason == "IPAD ONLY";
                bool contextRejected = !controller.TryBeginFreeHop(true, true, false,
                    false, out string contextHopReason) &&
                    contextHopReason == "CONTEXT ACTION AVAILABLE";
                bool busyRejected = !controller.TryBeginFreeHop(true, false, true,
                    false, out string busyHopReason) && busyHopReason == "ACTIVITY BUSY";
                bool modalRejected = !controller.TryBeginFreeHop(true, false, false,
                    true, out string modalHopReason) && modalHopReason == "STORY OPEN";
                if (!lowTapRejected || !desktopRejected || !contextRejected ||
                    !busyRejected || !modalRejected ||
                    controller.FreeHopStartCount != freeHopStartsBefore)
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-free-hop-rejections " +
                        $"lowTap={lowTapRejected}/{pad.LastRejectionReason} " +
                        $"desktop={desktopRejected}/{desktopHopReason} " +
                        $"context={contextRejected}/{contextHopReason} " +
                        $"busy={busyRejected}/{busyHopReason} modal={modalRejected}/{modalHopReason} " +
                        $"starts={freeHopStartsBefore}/{controller.FreeHopStartCount}");
                    Application.Quit(134);
                    yield break;
                }

                var freeHopFeedback = moonlight.GetComponent<MoonlightActionFeedback>();
                var freeHopStage = moonlight.GetComponent<MoonlightActivityStage>();
                var freeHopCollider = controller.GetComponent<CapsuleCollider>();
                float rootYBeforeHop = controller.transform.position.y;
                float colliderCenterYBeforeHop = freeHopCollider != null
                    ? freeHopCollider.bounds.center.y
                    : float.NaN;
                Vector3 movementStart = controller.transform.position;
                float baseSpeedBeforeHop = controller.BaseMoveSpeed;
                float currentSpeedBeforeHop = controller.CurrentMoveSpeed;
                float wonderBeforeHop = moonlight.stats.wonder;
                float warmthBeforeHop = moonlight.stats.warmth;
                float restBeforeHop = moonlight.stats.rest;
                float magicBeforeHop = moonlight.stats.magic;
                float hungerBeforeHop = moonlight.stats.hunger;
                int xpBeforeHop = moonlight.xp;
                int coinsBeforeHop = moonlight.coins;
                int activityStepBeforeHop = freeHopFeedback != null
                    ? freeHopFeedback.ActivityStep
                    : 0;
                int activityRequiredBeforeHop = freeHopFeedback != null
                    ? freeHopFeedback.ActivityRequiredSteps
                    : 0;
                float activityProgressBeforeHop = freeHopFeedback != null
                    ? freeHopFeedback.ActionProgress01
                    : 0f;
                var progressZones = FindObjectsByType<MoonlightSpatialActionZone>(
                    FindObjectsSortMode.None);
                int[] progressBeforeHop = progressZones.Select(zone => zone.ProgressStep).ToArray();

                controller.SetTouchMove(Vector2.right * 0.20f);
                var primaryPointer = new PointerEventData(EventSystem.current)
                {
                    pointerId = 7001,
                    position = RectTransformUtility.WorldToScreenPoint(null,
                        ui.actionBtn.transform.position)
                };
                var secondaryPointer = new PointerEventData(EventSystem.current)
                {
                    pointerId = 7002,
                    position = primaryPointer.position
                };
                int multitouchRejectionsBefore = pad.MultitouchRejectionCount;
                pad.OnPointerDown(primaryPointer);
                bool primaryArmed = pad.IsTrackingGesture &&
                    pad.ActivePointerIdForQA == primaryPointer.pointerId;
                pad.OnPointerDown(secondaryPointer);
                bool multitouchPass = primaryArmed && pad.IsTrackingGesture &&
                    pad.ActivePointerIdForQA == primaryPointer.pointerId &&
                    pad.MultitouchRejectionCount == multitouchRejectionsBefore + 1 &&
                    pad.LastRejectionReason == "MULTITOUCH BLOCKED";
                if (!multitouchPass)
                {
                    controller.ClearTouchMovementState();
                    pad.OnCancel(null);
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-free-hop-multitouch " +
                        $"armed={primaryArmed} tracking={pad.IsTrackingGesture} " +
                        $"pointer={pad.ActivePointerIdForQA}/{primaryPointer.pointerId} " +
                        $"rejections={multitouchRejectionsBefore}/{pad.MultitouchRejectionCount} " +
                        $"reason={pad.LastRejectionReason}");
                    Application.Quit(135);
                    yield break;
                }

                pad.OnPointerUp(primaryPointer);
                bool hopStarted = controller.IsFreeHopping &&
                    controller.FreeHopStartCount == freeHopStartsBefore + 1;
                yield return null;
                bool hopFeedbackPersisted = ui.actionBtn.gameObject.activeSelf &&
                    pad.isActiveAndEnabled && pad.IsResultFeedbackActive;
                bool repeatRejected = !pad.SubmitSynthetic(MoonlightGestureKind.Tap, 0.95f) &&
                    pad.LastRejectionReason == "HOP IN PROGRESS" &&
                    controller.FreeHopStartCount == freeHopStartsBefore + 1;
                float hopDeadline = Time.time + 1.2f;
                float observedVisualPeak = controller.CurrentFreeHopAppliedOffsetY;
                while (controller.IsFreeHopping && Time.time < hopDeadline)
                {
                    yield return null;
                    observedVisualPeak = Mathf.Max(observedVisualPeak,
                        controller.CurrentFreeHopAppliedOffsetY);
                }
                controller.ClearTouchMovementState();

                float movementDistance = Vector2.Distance(
                    new Vector2(movementStart.x, movementStart.z),
                    new Vector2(controller.transform.position.x, controller.transform.position.z));
                bool hopMotionPass = hopStarted && hopFeedbackPersisted && repeatRejected &&
                    controller.LastFreeHopCompleted && !controller.IsFreeHopping &&
                    Mathf.Abs(controller.LastFreeHopPeakHeight -
                        MoonlightPlayerController.FreeHopHeight) <= 0.001f &&
                    controller.LastFreeHopLandingError <=
                        MoonlightPlayerController.FreeHopLandingTolerance &&
                    controller.LastFreeHopRootVerticalDrift <=
                        MoonlightPlayerController.FreeHopLandingTolerance &&
                    observedVisualPeak >= MoonlightPlayerController.FreeHopHeight - 0.01f &&
                    controller.CurrentFreeHopAppliedOffsetY <=
                        MoonlightPlayerController.FreeHopLandingTolerance &&
                    Mathf.Abs(controller.transform.position.y - rootYBeforeHop) <= 0.001f &&
                    (freeHopCollider == null || Mathf.Abs(freeHopCollider.bounds.center.y -
                        colliderCenterYBeforeHop) <= 0.001f) && movementDistance >= 0.02f &&
                    controller.FreeHopQAMarker == MoonlightPlayerController.FreeHopReadyMarker;
                if (!hopMotionPass)
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-free-hop-motion " +
                        $"started={hopStarted} feedback={hopFeedbackPersisted} " +
                        $"repeat={repeatRejected}/{pad.LastRejectionReason} " +
                        $"completed={controller.LastFreeHopCompleted} active={controller.IsFreeHopping} " +
                        $"peak={controller.LastFreeHopPeakHeight:0.000}/" +
                        $"{MoonlightPlayerController.FreeHopHeight:0.000} " +
                        $"landing={controller.LastFreeHopLandingError:0.0000} " +
                        $"observedPeak={observedVisualPeak:0.000} " +
                        $"rootDrift={controller.LastFreeHopRootVerticalDrift:0.0000} " +
                        $"rootY={rootYBeforeHop:0.000}/{controller.transform.position.y:0.000} " +
                        $"colliderY={colliderCenterYBeforeHop:0.000}/" +
                        $"{(freeHopCollider != null ? freeHopCollider.bounds.center.y : float.NaN):0.000} " +
                        $"movement={movementDistance:0.000} marker={controller.FreeHopQAMarker}");
                    Application.Quit(138);
                    yield break;
                }

                var handoffZone = progressZones.FirstOrDefault(zone => zone != null &&
                    zone.isActiveAndEnabled && zone.gameObject.activeInHierarchy);
                Vector3 handoffStartPosition = controller.transform.position;
                bool handoffHopStarted = handoffZone != null &&
                    pad.SubmitSynthetic(MoonlightGestureKind.Tap, 0.95f) &&
                    controller.IsFreeHopping;
                if (handoffHopStarted)
                    yield return new WaitForSeconds(0.20f);
                float handoffHeightBeforeZone = controller.CurrentFreeHopAppliedOffsetY;
                if (handoffZone != null)
                {
                    controller.transform.position = new Vector3(handoffZone.transform.position.x,
                        handoffStartPosition.y, handoffZone.transform.position.z);
                    Physics.SyncTransforms();
                    spatialInteractor.RescanNowForQA();
                    yield return null;
                }
                bool contextHandoffPass = handoffHopStarted &&
                    handoffHeightBeforeZone >= 0.10f &&
                    spatialInteractor.CurrentZone == handoffZone &&
                    !controller.IsFreeHopping &&
                    controller.CurrentFreeHopAppliedOffsetY <=
                        MoonlightPlayerController.FreeHopLandingTolerance;
                controller.transform.position = handoffStartPosition;
                Physics.SyncTransforms();
                spatialInteractor.RescanNowForQA();
                ui.Refresh(moonlight);
                if (!contextHandoffPass)
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-free-hop-context-handoff " +
                        $"started={handoffHopStarted} zone={handoffZone?.DisplayName ?? "none"} " +
                        $"current={spatialInteractor.CurrentZone?.DisplayName ?? "none"} " +
                        $"heightBefore={handoffHeightBeforeZone:0.000} " +
                        $"activeAfter={controller.IsFreeHopping} " +
                        $"heightAfter={controller.CurrentFreeHopVisualHeight:0.000}");
                    Application.Quit(139);
                    yield break;
                }

                // Repeat from a truly idle state so MoonlightBobber is enabled before takeoff.
                yield return null;
                int idleHopStartsBefore = controller.FreeHopStartCount;
                var idleHopBobber = controller.GetComponentInChildren<MoonlightBobber>();
                var idleHopKidAnimator = controller.GetComponentInChildren<MoonlightKidAnimator>();
                bool idleBobberOriginalEnabled = idleHopBobber != null && idleHopBobber.enabled;
                if (idleHopBobber != null && !idleHopBobber.enabled)
                    idleHopBobber.ResumeFromNeutralAfterFreeHop(true);
                bool idleVisualOwnerReady = idleHopBobber != null
                    ? idleHopBobber.enabled
                    : idleHopKidAnimator != null && idleHopKidAnimator.isActiveAndEnabled;
                bool idleHopStarted = pad.SubmitSynthetic(MoonlightGestureKind.Tap, 0.95f) &&
                    controller.IsFreeHopping &&
                    controller.FreeHopStartCount == idleHopStartsBefore + 1;
                bool idleBobberSuspended = idleHopBobber != null
                    ? !idleHopBobber.enabled
                    : idleHopKidAnimator != null && idleHopKidAnimator.isActiveAndEnabled;
                float idleObservedVisualPeak = controller.CurrentFreeHopAppliedOffsetY;
                float idleHopDeadline = Time.time + 1.2f;
                while (controller.IsFreeHopping && Time.time < idleHopDeadline)
                {
                    yield return null;
                    idleObservedVisualPeak = Mathf.Max(idleObservedVisualPeak,
                        controller.CurrentFreeHopAppliedOffsetY);
                }
                bool idleBobberResumed = idleHopBobber != null
                    ? idleHopBobber.enabled
                    : idleHopKidAnimator != null && idleHopKidAnimator.isActiveAndEnabled;
                bool idleHopPass = idleHopStarted && controller.LastFreeHopCompleted &&
                    !controller.IsFreeHopping &&
                    idleVisualOwnerReady && idleBobberSuspended && idleBobberResumed &&
                    idleObservedVisualPeak >= MoonlightPlayerController.FreeHopHeight - 0.01f &&
                    controller.LastFreeHopLandingError <=
                        MoonlightPlayerController.FreeHopLandingTolerance &&
                    Mathf.Abs(controller.transform.position.y - rootYBeforeHop) <= 0.001f;
                if (idleHopBobber != null && !idleBobberOriginalEnabled)
                    idleHopBobber.enabled = false;
                if (!idleHopPass)
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-free-hop-idle " +
                        $"started={idleHopStarted} completed={controller.LastFreeHopCompleted} " +
                        $"active={controller.IsFreeHopping} " +
                        $"bobberSuspended={idleBobberSuspended} " +
                        $"bobberResumed={idleBobberResumed} " +
                        $"observedPeak={idleObservedVisualPeak:0.000} " +
                        $"landing={controller.LastFreeHopLandingError:0.0000}");
                    Application.Quit(136);
                    yield break;
                }

                bool zoneProgressUnchanged = progressZones.Length == progressBeforeHop.Length;
                for (int zoneIndex = 0; zoneProgressUnchanged &&
                        zoneIndex < progressZones.Length; zoneIndex++)
                    zoneProgressUnchanged = progressZones[zoneIndex] != null &&
                        progressZones[zoneIndex].ProgressStep == progressBeforeHop[zoneIndex];
                bool invariantsPass = Approximately(controller.BaseMoveSpeed, baseSpeedBeforeHop) &&
                    Approximately(controller.CurrentMoveSpeed, currentSpeedBeforeHop) &&
                    Approximately(moonlight.stats.wonder, wonderBeforeHop) &&
                    Approximately(moonlight.stats.warmth, warmthBeforeHop) &&
                    Approximately(moonlight.stats.rest, restBeforeHop) &&
                    Approximately(moonlight.stats.magic, magicBeforeHop) &&
                    Approximately(moonlight.stats.hunger, hungerBeforeHop) &&
                    moonlight.xp == xpBeforeHop && moonlight.coins == coinsBeforeHop &&
                    (freeHopFeedback == null ||
                        (!freeHopFeedback.IsPerformingAction &&
                         !freeHopFeedback.IsCoolingDown &&
                         freeHopFeedback.ActivityStep == activityStepBeforeHop &&
                         freeHopFeedback.ActivityRequiredSteps == activityRequiredBeforeHop &&
                         Approximately(freeHopFeedback.ActionProgress01,
                             activityProgressBeforeHop))) &&
                    (freeHopStage == null || !freeHopStage.IsLingering) && zoneProgressUnchanged;
                if (!invariantsPass)
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-free-hop-invariants " +
                        $"speed={baseSpeedBeforeHop:0.00}/{controller.BaseMoveSpeed:0.00} " +
                        $"current={currentSpeedBeforeHop:0.00}/{controller.CurrentMoveSpeed:0.00} " +
                        $"stats={wonderBeforeHop:0.0}/{moonlight.stats.wonder:0.0}," +
                        $"{warmthBeforeHop:0.0}/{moonlight.stats.warmth:0.0}," +
                        $"{restBeforeHop:0.0}/{moonlight.stats.rest:0.0}," +
                        $"{magicBeforeHop:0.0}/{moonlight.stats.magic:0.0}," +
                        $"{hungerBeforeHop:0.0}/{moonlight.stats.hunger:0.0} " +
                        $"reward={xpBeforeHop}/{moonlight.xp},{coinsBeforeHop}/{moonlight.coins} " +
                        $"activity={activityStepBeforeHop}/" +
                        $"{(freeHopFeedback != null ? freeHopFeedback.ActivityStep : 0)} " +
                        $"progress={activityProgressBeforeHop:0.000}/" +
                        $"{(freeHopFeedback != null ? freeHopFeedback.ActionProgress01 : 0f):0.000} " +
                        $"zones={zoneProgressUnchanged}");
                    Application.Quit(137);
                    yield break;
                }
                Debug.Log($"[MoonlightGameplayQA][PASS] ipad-free-hop-runtime " +
                    $"availability=True rejections=True multitouch=True " +
                    $"peak={controller.LastFreeHopPeakHeight:0.000}m " +
                    $"observedPeak={observedVisualPeak:0.000}m feedback=True " +
                    $"idlePeak={idleObservedVisualPeak:0.000}m " +
                    $"contextHandoff=True " +
                    $"landing={controller.LastFreeHopLandingError:0.0000}m " +
                    $"rootDrift={controller.LastFreeHopRootVerticalDrift:0.0000}m " +
                    $"movement={movementDistance:0.000}m invariants=True " +
                    "marker=MOONLIGHT_IPAD_FREE_HOP_RUNTIME_VERIFIED");
            }
            else if (touchJoystick != null)
            {
                Debug.LogError("[MoonlightGameplayQA][FAIL] desktop-touch-joystick-visible");
                Application.Quit(56);
                yield break;
            }
            else
            {
                Debug.Log("[MoonlightGameplayQA][PASS] desktop-touch-joystick-hidden " +
                    "marker=MOONLIGHT_DESKTOP_INPUT_CLEAN");
            }

            audio.SetDeterministicTestMode(true);
            foreach (RoomType room in System.Enum.GetValues(typeof(RoomType)))
            {
                rooms.GoToRoom(room);
                yield return new WaitForSeconds(0.18f);
                if (rooms.CurrentRoom != room || !controller.RoomBounds.Contains(
                        new Vector2(controller.transform.position.x, controller.transform.position.z)))
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] room-navigation room={room} " +
                        $"position={controller.transform.position:F2} bounds={controller.RoomBounds}");
                    Application.Quit(21);
                    yield break;
                }
                Debug.Log($"[MoonlightGameplayQA][PASS] room-navigation room={room} " +
                    $"position={controller.transform.position:F2} cue={audio.LastCueKey}");
            }

            rooms.GoToRoom(RoomType.LivingRoom);
            yield return new WaitForSeconds(0.25f);
            var overlapBlocker = GameObject.Find("KitchenTableCollision")?.GetComponent<Collider>();
            if (overlapBlocker == null)
            {
                Debug.LogError("[MoonlightGameplayQA][FAIL] recovery blocker missing");
                Application.Quit(55);
                yield break;
            }
            int recoveriesBefore = controller.RecoveryCount;
            var forcedOverlap = overlapBlocker.bounds.center;
            forcedOverlap.y = 0f;
            controller.transform.position = forcedOverlap;
            Physics.SyncTransforms();
            controller.TryMove(new Vector3(0.02f, 0f, 0f));
            if (controller.RecoveryCount <= recoveriesBefore ||
                controller.LastRecoveryReason != "overlap-before-move")
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] overlap-recovery " +
                    $"count={controller.RecoveryCount - recoveriesBefore} " +
                    $"reason={controller.LastRecoveryReason} position={controller.transform.position:F2}");
                Application.Quit(56);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] overlap-recovery " +
                $"position={controller.transform.position:F2} count={controller.RecoveryCount} " +
                "marker=MOONLIGHT_PLAYER_RECOVERED");

            controller.TeleportTo(new Vector3(-0.55f, 0f, -2.45f), controller.RoomBounds);
            int collisionsBefore = controller.CollisionCount;
            for (int i = 0; i < 38; i++)
            {
                controller.TryMove(new Vector3(0f, 0f, 0.04f));
                yield return null;
            }
            if (controller.CollisionCount <= collisionsBefore ||
                controller.LastCollisionName != "KitchenTableCollision")
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] collision blocker={controller.LastCollisionName} " +
                    $"count={controller.CollisionCount - collisionsBefore} position={controller.transform.position:F2}");
                Application.Quit(22);
                yield break;
            }
            string collisionShot = Path.Combine(output, "01_after_collision_table.png");
            yield return Capture(collisionShot);
            Debug.Log($"[MoonlightGameplayQA][PASS] collision blocker={controller.LastCollisionName} " +
                $"position={controller.transform.position:F2} screenshot={collisionShot}");

            rooms.GoToRoom(RoomType.Kitchen);
            yield return new WaitForSeconds(0.28f);
            var activeKitchenZones = FindObjectsByType<MoonlightSpatialActionZone>(
                    FindObjectsSortMode.None)
                .Where(candidate => candidate.gameObject.activeInHierarchy)
                .ToArray();
            var feedZones = activeKitchenZones
                .Where(candidate => candidate.Kind == MoonlightSpatialActionKind.Feed).ToArray();
            var cookZones = activeKitchenZones
                .Where(candidate => candidate.Kind == MoonlightSpatialActionKind.Cook).ToArray();
            MoonlightSpatialActionZone feedZone = feedZones.FirstOrDefault();
            bool zoneCountPass = feedZones.Length == 1 && cookZones.Length == 1 &&
                feedZone != null && feedZone.RequiredSteps == 1 &&
                feedZone.RequiredGesture == MoonlightGestureKind.Tap &&
                !MoonlightSpatialActionZone.IsScoredActivityKind(feedZone.Kind) &&
                ui != null && ui.FeedButtonIsHidden;
            if (!zoneCountPass)
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] kitchen-feed-zone-count " +
                    $"feed={feedZones.Length}/1 cook={cookZones.Length}/1 " +
                    $"steps={feedZone?.RequiredSteps ?? 0}/1 gesture={feedZone?.RequiredGesture} " +
                    $"feedButtonHidden={(ui != null && ui.FeedButtonIsHidden)}");
                Application.Quit(125);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] kitchen-feed-zone-count " +
                $"feed={feedZones.Length}/1 cook={cookZones.Length}/1 nonScored=True " +
                "marker=MOONLIGHT_FEED_COOK_ZONE_COUNTS_VERIFIED");

            controller.TeleportTo(feedZone.transform.position, controller.RoomBounds);
            yield return new WaitForSeconds(0.65f);
            if (spatialInteractor.CurrentZone != feedZone)
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] kitchen-feed-context " +
                    $"current={spatialInteractor.CurrentZone?.DisplayName} expected={feedZone.DisplayName}");
                Application.Quit(126);
                yield break;
            }

            moonlight.stats.wonder = 31f;
            moonlight.stats.warmth = 42f;
            moonlight.stats.rest = 53f;
            moonlight.stats.magic = 64f;
            moonlight.stats.hunger = 40f;
            int feedXpBefore = moonlight.xp;
            int feedCoinsBefore = moonlight.coins;
            ui.Refresh(moonlight);
            yield return null;

            MoonlightGestureSample wrongFeedSample = MoonlightGestureSample.Synthetic(
                MoonlightGestureKind.Swipe, 0.95f);
            ui.ExecuteContextGesture(MoonlightGestureKind.Swipe, wrongFeedSample);
            yield return null;
            bool wrongFeedUnchanged = !feedZone.LastGesturePassed &&
                Approximately(moonlight.stats.wonder, 31f) &&
                Approximately(moonlight.stats.warmth, 42f) &&
                Approximately(moonlight.stats.rest, 53f) &&
                Approximately(moonlight.stats.magic, 64f) &&
                Approximately(moonlight.stats.hunger, 40f) && moonlight.xp == feedXpBefore &&
                moonlight.coins == feedCoinsBefore;
            if (!wrongFeedUnchanged)
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] feed-wrong-gesture-mutated " +
                    $"passed={feedZone.LastGesturePassed} hunger={moonlight.stats.hunger:0.0}/40");
                Application.Quit(127);
                yield break;
            }

            MoonlightGestureSample lowFeedSample = MoonlightGestureSample.Synthetic(
                MoonlightGestureKind.Tap, 0.20f);
            ui.ExecuteContextGesture(MoonlightGestureKind.Tap, lowFeedSample);
            yield return null;
            bool lowFeedUnchanged = !feedZone.LastGesturePassed &&
                Approximately(moonlight.stats.wonder, 31f) &&
                Approximately(moonlight.stats.warmth, 42f) &&
                Approximately(moonlight.stats.rest, 53f) &&
                Approximately(moonlight.stats.magic, 64f) &&
                Approximately(moonlight.stats.hunger, 40f) && moonlight.xp == feedXpBefore &&
                moonlight.coins == feedCoinsBefore;
            if (!lowFeedUnchanged)
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] feed-low-score-mutated " +
                    $"passed={feedZone.LastGesturePassed} hunger={moonlight.stats.hunger:0.0}/40");
                Application.Quit(130);
                yield break;
            }

            MoonlightGestureSample acceptedFeedSample = MoonlightGestureSample.Synthetic(
                MoonlightGestureKind.Tap, 0.95f);
            ui.ExecuteContextGesture(MoonlightGestureKind.Tap, acceptedFeedSample);
            MoonlightActionFeedback feedFeedback = moonlight.GetComponent<MoonlightActionFeedback>();
            bool acceptedFeedPass = feedZone.LastGesturePassed &&
                feedZone.RequiredSteps == 1 && feedZone.ProgressStep == 0 &&
                Approximately(moonlight.stats.hunger, 58f) &&
                Approximately(moonlight.stats.wonder, 31f) &&
                Approximately(moonlight.stats.warmth, 42f) &&
                Approximately(moonlight.stats.rest, 53f) &&
                Approximately(moonlight.stats.magic, 64f) && moonlight.xp == feedXpBefore &&
                moonlight.coins == feedCoinsBefore && feedFeedback != null &&
                feedFeedback.IsPerformingAction &&
                feedFeedback.ActiveActivityKind == MoonlightSpatialActionKind.Feed &&
                feedFeedback.ActiveGestureSample.ContentEquals(acceptedFeedSample) &&
                feedFeedback.ActionMotionProfile == "feed-bowl-to-mouth" &&
                feedFeedback.ActionVisualSignature == "feed-bowl-to-mouth" &&
                feedFeedback.ActionAccentVisualObjectCount ==
                    MoonlightActionFeedback.FeedVisualObjectBudget &&
                feedFeedback.ActionAccentRendererCount == MoonlightActionFeedback.FeedRendererBudget &&
                feedFeedback.ActionAccentMaterialCount <= MoonlightActionFeedback.FeedMaterialBudget &&
                feedFeedback.ActionAccentColliderCount == 0 &&
                feedFeedback.ActiveStageRenderers == 0 && feedFeedback.ActiveStageMaterials == 0 &&
                feedFeedback.ActiveStageLights == MoonlightActionFeedback.FeedLightBudget &&
                !feedFeedback.ActionParticlesActive && !feedFeedback.ActionFlashActive &&
                !feedFeedback.HasOpaqueActionOrb;
            float feedHungerAfterAccepted = moonlight.stats.hunger;
            feedZone.ExecuteGesture(moonlight, MoonlightGestureKind.Tap, acceptedFeedSample);
            bool busyFeedUnchanged = !feedZone.LastGesturePassed &&
                Approximately(moonlight.stats.hunger, feedHungerAfterAccepted) &&
                Approximately(moonlight.stats.wonder, 31f) &&
                Approximately(moonlight.stats.warmth, 42f) &&
                Approximately(moonlight.stats.rest, 53f) &&
                Approximately(moonlight.stats.magic, 64f) && moonlight.xp == feedXpBefore &&
                moonlight.coins == feedCoinsBefore &&
                feedFeedback.ActiveGestureSample.ContentEquals(acceptedFeedSample);
            Canvas.ForceUpdateCanvases();
            bool feedActionTextPass = !expectIPadHud ||
                (ui.VisibleActionTextIsInsideSafeArea && ui.VisibleActionTextDoesNotOverflow &&
                 ui.ActionTouchTargetIsInsideSafeArea && ui.ActivityPromptIsInsideSafeArea &&
                 ui.ActivityResultIsInsideSafeArea);
            if (!acceptedFeedPass || !busyFeedUnchanged || !feedActionTextPass)
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] feed-acceptance-feedback " +
                    $"accepted={acceptedFeedPass} busyUnchanged={busyFeedUnchanged} " +
                    $"hunger={moonlight.stats.hunger:0.0}/58 sample=" +
                    $"{(feedFeedback != null && feedFeedback.ActiveGestureSample.ContentEquals(acceptedFeedSample))} " +
                    $"renderers={feedFeedback?.ActionAccentRendererCount ?? 0}/" +
                    $"{MoonlightActionFeedback.FeedRendererBudget} materials=" +
                    $"{feedFeedback?.ActionAccentMaterialCount ?? 0}/<=" +
                    $"{MoonlightActionFeedback.FeedMaterialBudget} " +
                    $"stage={feedFeedback?.ActiveStageRenderers ?? -1}/" +
                    $"{feedFeedback?.ActiveStageMaterials ?? -1}/" +
                    $"{feedFeedback?.ActiveStageLights ?? -1} " +
                    $"actionTextSafe={ui.VisibleActionTextIsInsideSafeArea} " +
                    $"actionTextNonOverflow={ui.VisibleActionTextDoesNotOverflow}");
                Application.Quit(128);
                yield break;
            }

            bool feedContactObserved = false;
            float feedContactDeadline = Time.time + 1.20f;
            while (feedFeedback.IsPerformingAction && Time.time < feedContactDeadline)
            {
                feedContactObserved |= feedFeedback.IsActionContactActive &&
                    feedFeedback.ActionContactPhase == "contact" &&
                    feedFeedback.ActionContactTarget == "mouth" &&
                    feedFeedback.ActionContactSource == "fallback" &&
                    feedFeedback.ActionContactWeight >= 0.20f &&
                    feedFeedback.ActionContactTravelDistance >= 0.30f &&
                    feedFeedback.ActionAccentContactDistance <= 0.01f;
                if (feedContactObserved) break;
                yield return null;
            }
            if (!feedContactObserved)
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] feed-contact-evidence " +
                    $"phase={feedFeedback.ActionContactPhase} target={feedFeedback.ActionContactTarget}/mouth " +
                    $"source={feedFeedback.ActionContactSource}/fallback " +
                    $"weight={feedFeedback.ActionContactWeight:0.00} " +
                    $"travel={feedFeedback.ActionContactTravelDistance:0.00}/>=0.30 " +
                    $"accentDistance={feedFeedback.ActionAccentContactDistance:0.000}");
                Application.Quit(129);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] feed-runtime sample=True feedback=True " +
                $"contact={feedFeedback.ActionContactTarget} travel=" +
                $"{feedFeedback.ActionContactTravelDistance:0.00} hunger=40->58 " +
                $"actionTextSafe={feedActionTextPass} " +
                "marker=MOONLIGHT_FEED_SAMPLE_FEEDBACK_CONTACT_VERIFIED");
            while (feedFeedback.IsPerformingAction || feedFeedback.IsCoolingDown) yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            bool feedResultTextPass = ui.HasContextResult &&
                ui.ContextResultQAText.Contains("FED", System.StringComparison.Ordinal) &&
                ui.ContextResultQAText.Contains("+18 HUNGER", System.StringComparison.Ordinal) &&
                ui.VisibleActionTextIsInsideSafeArea && ui.VisibleActionTextDoesNotOverflow &&
                ui.ActivityResultIsInsideSafeArea && !ui.ContextResultIsOverflowing;
            if (!feedResultTextPass)
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] feed-result-text " +
                    $"text=\"{ui.ContextResultQAText}\" safe={ui.VisibleActionTextIsInsideSafeArea} " +
                    $"nonOverflow={ui.VisibleActionTextDoesNotOverflow}/" +
                    $"{!ui.ContextResultIsOverflowing}");
                Application.Quit(131);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] feed-result-text " +
                $"text=\"{ui.ContextResultQAText}\" marker=MOONLIGHT_FEED_RESULT_TEXT_VERIFIED");

            MoonlightSpatialActionKind[] activityKinds = ResolveGameplayActivityOrder(
                args, out string playContinuityOrderMode);
            RoomType[] activityRooms = activityKinds.Select(ActivityRoomFor).ToArray();
            Debug.Log($"[MoonlightGameplayQA] play-continuity-order " +
                $"mode={playContinuityOrderMode} sequence={string.Join(",", activityKinds)} " +
                $"marker={PlayContinuityOrderModeMarker}_{playContinuityOrderMode.ToUpperInvariant()}");
            bool forceCareFallback = args.Any(argument =>
                string.Equals(argument, "-moonlightForceCareFallback",
                    System.StringComparison.OrdinalIgnoreCase));
            int completedActivities = 0;
            int verifiedPersistentStations = 0;
            int busyGestureProbeCount = 0;
            var verifiedLowScoreRetryKinds =
                new System.Collections.Generic.HashSet<MoonlightSpatialActionKind>();
            bool verifiedIntermediateResultVisibility = false;
            bool verifiedFinalResultVisibility = false;
            var verifiedVisualSignatures = new System.Collections.Generic.HashSet<string>();

            for (int activityIndex = 0; activityIndex < activityKinds.Length; activityIndex++)
            {
                var expectedKind = activityKinds[activityIndex];
                rooms.GoToRoom(activityRooms[activityIndex]);
                yield return new WaitForSeconds(0.28f);
                var activeZones = FindObjectsByType<MoonlightSpatialActionZone>(FindObjectsSortMode.None)
                    .Where(candidate => candidate.gameObject.activeInHierarchy && candidate.Kind == expectedKind)
                    .ToArray();
                var zone = activeZones.FirstOrDefault();
                if (zone == null || activeZones.Length != 1)
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] active-zone-count action={expectedKind} " +
                        $"room={activityRooms[activityIndex]} count={activeZones.Length}");
                    Application.Quit(23);
                    yield break;
                }
                MoonlightActivityStation persistentStation = null;
                Vector3 persistentAnchor = Vector3.zero;
                if (expectedKind is MoonlightSpatialActionKind.Cook or MoonlightSpatialActionKind.Play or
                    MoonlightSpatialActionKind.Garden or MoonlightSpatialActionKind.Read or
                    MoonlightSpatialActionKind.Care)
                {
                    persistentStation = MoonlightActivityStation.FindNearestActive(expectedKind, zone.transform.position);
                    if (persistentStation == null || persistentStation.VisualRoot == null ||
                        persistentStation.RendererCount <= 0)
                    {
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] persistent-station-missing action={expectedKind}");
                        Application.Quit(37);
                        yield break;
                    }
                    persistentAnchor = persistentStation.AnchorPosition;
                    string persistentPrefix = expectedKind switch
                    {
                        MoonlightSpatialActionKind.Cook => "02",
                        MoonlightSpatialActionKind.Play => "03",
                        MoonlightSpatialActionKind.Garden => "04",
                        MoonlightSpatialActionKind.Read => "05",
                        MoonlightSpatialActionKind.Care => "06",
                        _ => "99"
                    };
                    string persistentShot = Path.Combine(output,
                        $"{persistentPrefix}_{expectedKind.ToString().ToLowerInvariant()}_station_before.png");
                    yield return Capture(persistentShot);
                    Debug.Log($"[MoonlightGameplayQA][PASS] persistent-station-before action={expectedKind} " +
                        $"anchor={persistentAnchor:F2} renderers={persistentStation.RendererCount} " +
                        $"materials={persistentStation.UniqueMaterialCount} screenshot={persistentShot}");
                    if (expectedKind == MoonlightSpatialActionKind.Care)
                    {
                        bool usesFallback = persistentStation.UsesProceduralFallback;
                        string expectedCareSourceMarker = persistentStation.UsesProceduralFallback
                            ? "MOONLIGHT_CARE_VANITY_PROCEDURAL_FALLBACK_READY"
                            : "MOONLIGHT_PERSISTENT_STATION_AUTHORED_READY";
                        int careRendererBudget = usesFallback ? 15 : 24;
                        int careMaterialBudget = usesFallback ? 8 : 12;
                        bool careRendererPass = persistentStation.RendererCount > 0 &&
                            persistentStation.RendererCount <= careRendererBudget &&
                            (!usesFallback || persistentStation.RendererCount == careRendererBudget);
                        bool careSourcePass = persistentStation.VisualSourceQAMarker ==
                                expectedCareSourceMarker &&
                            (!forceCareFallback || persistentStation.UsesProceduralFallback) &&
                            careRendererPass &&
                            persistentStation.UniqueMaterialCount > 0 &&
                            persistentStation.UniqueMaterialCount <= careMaterialBudget &&
                            persistentStation.EnabledColliderCount == 0 &&
                            persistentStation.EnabledLightCount == 0;
                        if (!careSourcePass)
                        {
                            Debug.LogError("[MoonlightGameplayQA][FAIL] care-vanity-source " +
                                $"forced={forceCareFallback} fallback={persistentStation.UsesProceduralFallback} " +
                                $"marker={persistentStation.VisualSourceQAMarker}/{expectedCareSourceMarker} " +
                                $"renderers={persistentStation.RendererCount}/" +
                                $"{(usesFallback ? "exactly-15" : "1-24")} " +
                                $"materials={persistentStation.UniqueMaterialCount}/1-{careMaterialBudget} " +
                                $"colliders={persistentStation.EnabledColliderCount}/" +
                                $"{persistentStation.ColliderCount} lights={persistentStation.EnabledLightCount}/" +
                                $"{persistentStation.LightCount}");
                            Application.Quit(96);
                            yield break;
                        }
                        Debug.Log("[MoonlightGameplayQA][PASS] care-vanity-source " +
                            $"forced={forceCareFallback} fallback={persistentStation.UsesProceduralFallback} " +
                            $"renderers={persistentStation.RendererCount}/{careRendererBudget} " +
                            $"materials={persistentStation.UniqueMaterialCount}/{careMaterialBudget} " +
                            $"colliders={persistentStation.EnabledColliderCount} " +
                            $"lights={persistentStation.EnabledLightCount} " +
                            $"marker={persistentStation.VisualSourceQAMarker}");
                    }
                }
                bool verifyRewards = true;
                moonlight.stats.wonder = 30f;
                moonlight.stats.warmth = 30f;
                moonlight.stats.magic = 30f;
                moonlight.stats.hunger = 30f;
                moonlight.stats.rest = 30f;
                float rewardWonder = moonlight.stats.wonder;
                float rewardWarmth = moonlight.stats.warmth;
                float rewardMagic = moonlight.stats.magic;
                float rewardHunger = moonlight.stats.hunger;
                float rewardRest = moonlight.stats.rest;
                int rewardXp = moonlight.xp;
                int rewardCoins = moonlight.coins;
                CookContinuityObservation cookContinuity = null;
                if (expectedKind == MoonlightSpatialActionKind.Cook)
                {
                    cookContinuity = new CookContinuityObservation();
                    var priorStage = moonlight.GetComponent<MoonlightActivityStage>();
                    if (priorStage != null)
                    {
                        cookContinuity.BuildBaseline =
                            priorStage.CookStageBuildCountForQA;
                        cookContinuity.HandoffBaseline =
                            priorStage.CookHandoffCountForQA;
                    }
                }
                PlayContinuityObservation playContinuity = null;
                if (expectedKind == MoonlightSpatialActionKind.Play)
                {
                    playContinuity = new PlayContinuityObservation
                    {
                        BaselineRewards = new PlayRewardSnapshot(moonlight),
                        OrderMode = playContinuityOrderMode
                    };
                    MeasurePlayCatchTapTrajectories(playContinuity);
                    playContinuity.Progression.Add(zone.ProgressStep);
                    var priorStage = moonlight.GetComponent<MoonlightActivityStage>();
                    if (priorStage != null)
                    {
                        playContinuity.CompletionApplicationBaseline =
                            priorStage.PersistentCompletionApplicationCountForQA;
                        playContinuity.ContinuationBaseline =
                            priorStage.PlayContinuationCountForQA;
                    }
                }
                controller.TeleportTo(zone.transform.position, controller.RoomBounds);
                yield return new WaitForSeconds(0.65f);
                if (expectedKind == MoonlightSpatialActionKind.Care)
                {
                    var careLiveHarness = new CareLiveHarnessResult();
                    yield return RunGestureResponsiveCareLiveHarness(moonlight,
                        spatialInteractor, ui, pad, zone, careLiveHarness);
                    if (!careLiveHarness.Passed)
                    {
                        Debug.LogError("[MoonlightGameplayQA][FAIL] " +
                            $"gesture-care-live-contrasts {careLiveHarness.Detail} " +
                            $"marker={GestureCareRuntimeContractFailureMarker}");
                        Application.Quit(158);
                        yield break;
                    }
                    Debug.Log("[MoonlightGameplayQA][PASS] " +
                        $"gesture-care-live-contrasts {careLiveHarness.Detail} " +
                        $"marker={GestureCareContrastRuntimeContractMarker} " +
                        "marker=MOONLIGHT_CARE_LIVE_PERSISTENT_ISOLATION_VERIFIED " +
                        "marker=MOONLIGHT_CARE_LIVE_4_6S_LINGER_VERIFIED");
                    yield return null;
                }
                string freshQualityDetail = "";
                if (expectIPadHud &&
                    !ValidateActivityQualityReadoutRuntimeBinding(ui, zone, false, false,
                        out freshQualityDetail))
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] " +
                        $"activity-quality-fresh-run action={zone.Kind} {freshQualityDetail}");
                    Application.Quit(153);
                    yield break;
                }
                if (expectIPadHud)
                    Debug.Log($"[MoonlightGameplayQA][PASS] activity-quality-fresh-run " +
                        $"action={zone.Kind} {freshQualityDetail} " +
                        $"marker={ActivityQualityReadoutRuntimeContractMarker}");
                if (expectIPadHud && (spatialInteractor.HasNavigationTarget ||
                    ui.IsIPadNavigationCueVisible ||
                    ui.NavigationCueQAMarker != "MOONLIGHT_IPAD_NAVIGATION_CUE_HIDDEN"))
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-navigation-cue-in-range " +
                        $"action={expectedKind} current={spatialInteractor.CurrentZone?.DisplayName} " +
                        $"hasTarget={spatialInteractor.HasNavigationTarget} " +
                        $"visible={ui.IsIPadNavigationCueVisible} marker={ui.NavigationCueQAMarker}");
                    Application.Quit(80);
                    yield break;
                }
                if (expectIPadHud)
                    Debug.Log($"[MoonlightGameplayQA][PASS] ipad-navigation-cue-in-range " +
                        $"action={expectedKind} visible={ui.IsIPadNavigationCueVisible} " +
                        $"marker={ui.NavigationCueQAMarker}");
                int startStep = zone.ProgressStep;
                Vector2 rejectedHeldInput = new(0.58f, -0.44f);
                int resetSequenceBeforeRejectedAction = touchJoystick != null
                    ? touchJoystick.ResetSequence
                    : -1;
                if (expectIPadHud)
                    touchJoystick.ArmHeldInputForQA(rejectedHeldInput);
                bool lowScoreForwarded = pad.SubmitSynthetic(zone.RequiredGesture, 0.20f);
                yield return new WaitForSeconds(0.15f);
                string expectedLowScoreResult =
                    ExpectedInitialLowScoreIPadResult(zone.Kind);
                bool lowScoreResultPass = !expectIPadHud ||
                    (ui.ContextResultQAText == expectedLowScoreResult &&
                     ui.ContextResultLineCount == 2 &&
                     MoonlightUI.IsValidIPadResultFormat(ui.ContextResultQAText) &&
                     ui.ActivityResultIsInsideSafeArea &&
                     !ui.ContextResultIsOverflowing &&
                     ui.VisibleActionTextDoesNotOverflow);
                bool rejectedMovementRetained = !expectIPadHud ||
                    (touchJoystick.ResetSequence == resetSequenceBeforeRejectedAction &&
                     touchJoystick.IsTrackingPointer &&
                     Vector2.Distance(touchJoystick.Value, rejectedHeldInput) <= 0.0001f &&
                     touchJoystick.KnobAnchoredPosition.sqrMagnitude > 0.0001f &&
                     Vector2.Distance(controller.TouchMove, rejectedHeldInput) <= 0.0001f);
                if (!lowScoreForwarded || zone.ProgressStep != startStep ||
                    zone.LastGesturePassed ||
                    audio.LastCueKey != "activity-try-again" || !rejectedMovementRetained ||
                    !lowScoreResultPass)
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] fail-gesture advanced action={zone.Kind} " +
                        $"forwarded={lowScoreForwarded} passed={zone.LastGesturePassed} " +
                        $"step={zone.ProgressStep}/{startStep} cue={audio.LastCueKey} " +
                        $"result=\"{ui.ContextResultQAText.Replace('\n', '/')}\" " +
                        $"expected=\"{expectedLowScoreResult.Replace('\n', '/')}\" " +
                        $"lines={ui.ContextResultLineCount}/2 " +
                        $"safe={ui.ActivityResultIsInsideSafeArea} " +
                        $"overflow={ui.ContextResultIsOverflowing}/" +
                        $"{!ui.VisibleActionTextDoesNotOverflow} " +
                        $"retained={rejectedMovementRetained} " +
                        $"tracking={(touchJoystick != null && touchJoystick.IsTrackingPointer)} " +
                        $"value={(touchJoystick != null ? touchJoystick.Value : Vector2.zero):F3} " +
                        $"controller={controller.TouchMove:F3}");
                    Application.Quit(24);
                    yield break;
                }
                Debug.Log($"[MoonlightGameplayQA][PASS] fail-gesture action={zone.Kind} " +
                    $"forwarded={lowScoreForwarded} passed={zone.LastGesturePassed} " +
                    $"step={zone.ProgressStep}/{startStep} score={zone.LastGestureScore:0.00} " +
                    $"heldMovementRetained={rejectedMovementRetained} " +
                    "marker=MOONLIGHT_REJECTED_ACTIVITY_MOVEMENT_RETAINED");
                if (expectIPadHud)
                {
                    verifiedLowScoreRetryKinds.Add(zone.Kind);
                    Debug.Log($"[MoonlightGameplayQA][PASS] low-score-retry-result " +
                        $"action={zone.Kind} text=\"{ui.ContextResultQAText.Replace('\n', '/')}\" " +
                        $"lines={ui.ContextResultLineCount}/2 safe={ui.ActivityResultIsInsideSafeArea} " +
                        $"overflow={ui.ContextResultIsOverflowing} " +
                        $"verified={verifiedLowScoreRetryKinds.Count}/5");
                }
                string failedQualityDetail = "";
                if (expectIPadHud &&
                    (!ValidateActivityQualityReadoutRuntimeBinding(ui, zone, true, false,
                         out failedQualityDetail) ||
                     ui.ActivityQualityReadoutComboForQA != 0 ||
                     ui.ActivityQualityReadoutRunScoreForQA != 0f ||
                     !ui.ActivityQualityReadoutQAText.StartsWith("GOOD\nCOMBO x0\nRUN 00%",
                         System.StringComparison.Ordinal)))
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] " +
                        $"activity-quality-failure action={zone.Kind} {failedQualityDetail}");
                    Application.Quit(154);
                    yield break;
                }
                if (expectIPadHud)
                    Debug.Log($"[MoonlightGameplayQA][PASS] activity-quality-failure " +
                        $"action={zone.Kind} {failedQualityDetail} " +
                        $"marker={ActivityQualityReadoutRuntimeContractMarker}");
                if (expectIPadHud)
                    touchJoystick.ClearInputForQA();

                var verifiedCareContacts = new System.Collections.Generic.HashSet<string>();
                int verifiedGestureResponsiveCareSteps = 0;
                float expectedSessionScoreTotal = 0f;
                for (int step = 0; step < zone.RequiredSteps; step++)
                {
                    var expected = zone.RequiredGesture;
                    if (zone.Kind == MoonlightSpatialActionKind.Care)
                    {
                        MoonlightGestureKind expectedCareGesture = step switch
                        {
                            0 => MoonlightGestureKind.Tap,
                            1 => MoonlightGestureKind.Circle,
                            2 => MoonlightGestureKind.Swipe,
                            _ => MoonlightGestureKind.Hold
                        };
                        string expectedCareCue = step switch
                        {
                            0 => "care-prep",
                            1 => "care-wash",
                            2 => "care-brush",
                            _ => "care-glow"
                        };
                        if (expected != expectedCareGesture ||
                            MoonlightSpatialActionZone.CareCueForStep(step) != expectedCareCue)
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] care-gesture-cue step={step + 1} " +
                                $"gesture={expected}/{expectedCareGesture} " +
                                $"cue={MoonlightSpatialActionZone.CareCueForStep(step)}/{expectedCareCue}");
                            Application.Quit(97);
                            yield break;
                        }
                    }
                    if (expectIPadHud)
                    {
                        yield return null;
                        string expectedProgress = $"{step + 1}/{zone.RequiredSteps}";
                        float expectedFill = step / (float)zone.RequiredSteps;
                        string expectedInstruction = zone.GetCompactIPadInstruction(moonlight);
                        bool stepHudPass = ui.ActivityProgressQAMarker == expectedProgress &&
                            Approximately(ui.ActivityProgressFill01, expectedFill) &&
                            ui.ActivityProgressFillQAMarker ==
                                "MOONLIGHT_IPAD_ACTIVITY_PROGRESS_FILL_READY" &&
                            ui.ActivityContextQAText == expectedInstruction &&
                            MoonlightSpatialActionZone.IsValidCompactIPadInstruction(
                                ui.ActivityContextQAText) &&
                            !string.IsNullOrEmpty(ui.GestureCommandQAMarker) &&
                            pad.GuideIsVisible && pad.GuideGesture == zone.RequiredGesture &&
                            pad.GuidePathQAMarker == "MOONLIGHT_IPAD_GESTURE_GUIDE_READY" &&
                            ui.ActionTouchTargetMeetsIPadMinimum && ui.ActionTouchTargetIsInsideSafeArea &&
                            ui.ActivityPromptIsInsideSafeArea && ui.ActivityResultIsInsideSafeArea &&
                            ui.ActivityProgressIsInsideSafeArea && ui.ActivityHUDPanelsDoNotOverlap;
                        if (!stepHudPass)
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-activity-hud action={zone.Kind} " +
                                $"step={step + 1} progress={ui.ActivityProgressQAMarker} " +
                                $"fill={ui.ActivityProgressFill01:0.000}/{expectedFill:0.000} " +
                                $"fillMarker={ui.ActivityProgressFillQAMarker} " +
                                $"instruction=\"{ui.ActivityContextQAText}\" " +
                                $"gesture={ui.GestureCommandQAMarker} touch={ui.ActionTouchTargetLayoutSize} " +
                                $"insideSafe={ui.ActionTouchTargetIsInsideSafeArea} " +
                                $"panelsSeparated={ui.ActivityHUDPanelsDoNotOverlap}");
                            Application.Quit(43);
                            yield break;
                        }
                        Debug.Log($"[MoonlightGameplayQA][PASS] ipad-activity-hud action={zone.Kind} " +
                            $"step={step + 1} progress={ui.ActivityProgressQAMarker} " +
                            $"fill={ui.ActivityProgressFill01:0.000} " +
                            $"instruction=\"{ui.ActivityContextQAText}\" " +
                            $"gesture=\"{ui.GestureCommandQAMarker}\"");
                    }
                    Vector3 acceptedActionPosition = controller.transform.position;
                    var acceptedActionZone = spatialInteractor.CurrentZone;
                    int resultTimingSelectionBeforeAccepted = -1;
                    int resultShownCountBeforeAccepted = -1;
                    int resetSequenceBeforeAcceptedAction = touchJoystick != null
                        ? touchJoystick.ResetSequence
                        : -1;
                    Vector2 liveHoldHeldInput = new(0.72f, 0.38f);
                    if (expectIPadHud)
                        touchJoystick.ArmHeldInputForQA(liveHoldHeldInput);
                    bool finalResultStep = step == zone.RequiredSteps - 1;
                    float expectedResultDuration = finalResultStep
                        ? MoonlightUI.FinalActivityResultDurationSeconds
                        : MoonlightUI.IntermediateActivityResultDurationSeconds;
                    bool observeActualResultVisibility =
                        zone.Kind == MoonlightSpatialActionKind.Cook &&
                        (step == 0 || finalResultStep);
                    ContextResultVisibilityObservation resultVisibilityObservation = null;
                    bool verifyThisLiveHold = verifyLiveHoldRuntime &&
                        zone.SupportsLiveHoldReadiness;
                    bool acceptedActionStarted;
                    if (verifyThisLiveHold)
                    {
                        int progressBeforeLiveHold = zone.ProgressStep;
                        var liveHoldPointer = new PointerEventData(EventSystem.current)
                        {
                            pointerId = 7100 + verifiedLiveHoldRuntimeActions,
                            position = RectTransformUtility.WorldToScreenPoint(null,
                                ui.actionBtn.transform.position)
                        };
                        if (!verifiedGestureLockCleanup.Contains("zone-changed"))
                        {
                            pad.OnPointerDown(liveHoldPointer);
                            bool zoneChangeLockObserved = pad.IsTrackingGesture &&
                                pad.IsContextGestureMovementLockHeld &&
                                controller.IsContextGestureMovementLocked;
                            Rect bounds = controller.RoomBounds;
                            Vector3 zoneChangePosition = new(
                                zone.transform.position.x <= bounds.center.x
                                    ? bounds.xMax - 0.35f
                                    : bounds.xMin + 0.35f,
                                0f,
                                zone.transform.position.z <= bounds.center.y
                                    ? bounds.yMax - 0.35f
                                    : bounds.yMin + 0.35f);
                            controller.TeleportTo(zoneChangePosition, bounds);
                            spatialInteractor.RescanNowForQA();
                            bool zoneChanged = spatialInteractor.CurrentZone != acceptedActionZone;
                            controller.SetProcessedTouchSprintForQA(Vector2.zero);
                            yield return null;
                            bool zoneChangeCleanupPass = zoneChangeLockObserved && zoneChanged &&
                                !pad.IsTrackingGesture &&
                                !pad.IsContextGestureMovementLockHeld &&
                                !controller.IsContextGestureMovementLocked &&
                                pad.LastLiveHoldCancellationCleanupObserved &&
                                pad.LastLiveHoldCancellationReason == "zone-changed" &&
                                zone.ProgressStep == progressBeforeLiveHold;
                            controller.TeleportTo(acceptedActionPosition, bounds);
                            spatialInteractor.RescanNowForQA();
                            controller.SetProcessedTouchSprintForQA(liveHoldHeldInput);
                            bool zoneChangeRestorePass =
                                spatialInteractor.CurrentZone == acceptedActionZone &&
                                Vector3.Distance(controller.transform.position,
                                    acceptedActionPosition) <= 0.001f &&
                                Vector2.Distance(controller.TouchMove,
                                    liveHoldHeldInput) <= 0.0001f;
                            if (!zoneChangeCleanupPass || !zoneChangeRestorePass)
                            {
                                Debug.LogError($"[MoonlightGameplayQA][FAIL] " +
                                    $"ipad-live-hold-zone-change-cleanup action={zone.Kind} " +
                                    $"changed={zoneChanged} lock={zoneChangeLockObserved}/" +
                                    $"{controller.IsContextGestureMovementLocked} " +
                                    $"tracking={pad.IsTrackingGesture} " +
                                    $"reason={pad.LastLiveHoldCancellationReason} " +
                                    $"restored={zoneChangeRestorePass} " +
                                    $"zone={spatialInteractor.CurrentZone?.DisplayName}/" +
                                    $"{acceptedActionZone?.DisplayName}");
                                Application.Quit(145);
                                yield break;
                            }
                            verifiedGestureLockCleanup.Add("zone-changed");
                        }

                        pad.OnPointerDown(liveHoldPointer);
                        bool cancellationOverlayObserved = pad.IsTrackingGesture &&
                            pad.IsLiveHoldReadinessActive &&
                            pad.LiveHoldReadinessOverlayVisible &&
                            pad.IsContextGestureMovementLockHeld &&
                            controller.IsContextGestureMovementLocked;
                        string expectedCleanupReason = verifiedLiveHoldRuntimeActions switch
                        {
                            0 => "event-cancel",
                            1 => "focus-lost",
                            2 => "paused",
                            _ => "disabled"
                        };
                        bool cleanupDispatchObserved = true;
                        if (expectedCleanupReason == "event-cancel") pad.OnCancel(null);
                        else if (expectedCleanupReason == "focus-lost")
                            pad.SimulateApplicationFocusLossForQA();
                        else if (expectedCleanupReason == "paused")
                            pad.SimulateApplicationPauseForQA();
                        else
                        {
                            pad.enabled = false;
                            cleanupDispatchObserved = !pad.enabled;
                            pad.enabled = true;
                            cleanupDispatchObserved &= pad.enabled;
                        }
                        bool cancellationCleanupPass = cancellationOverlayObserved &&
                            cleanupDispatchObserved &&
                            !pad.IsTrackingGesture && pad.LiveHoldReadinessStateIsClean &&
                            !pad.LiveHoldReadinessOverlayVisible &&
                            !pad.IsContextGestureMovementLockHeld &&
                            !controller.IsContextGestureMovementLocked &&
                            pad.LastLiveHoldCancellationCleanupObserved &&
                            pad.LastLiveHoldCancellationReason == expectedCleanupReason &&
                            zone.ProgressStep == progressBeforeLiveHold;
                        if (!cancellationCleanupPass)
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-live-hold-cleanup " +
                                $"action={zone.Kind} reason={pad.LastLiveHoldCancellationReason}/" +
                                $"{expectedCleanupReason} overlay={cancellationOverlayObserved} " +
                                $"tracking={pad.IsTrackingGesture} " +
                                $"movementLock={controller.IsContextGestureMovementLocked} " +
                                $"stateClean={pad.LiveHoldReadinessStateIsClean} " +
                                $"cleanup={pad.LastLiveHoldCancellationCleanupObserved} " +
                                $"step={zone.ProgressStep}/{progressBeforeLiveHold}");
                            Application.Quit(140);
                            yield break;
                        }
                        verifiedGestureLockCleanup.Add(expectedCleanupReason);

                        int resetSequenceBeforeRejectedHold = touchJoystick.ResetSequence;
                        pad.OnPointerDown(liveHoldPointer);
                        bool rejectedHoldLockObserved = pad.IsContextGestureMovementLockHeld &&
                            controller.IsContextGestureMovementLocked;
                        pad.OnPointerUp(liveHoldPointer);
                        Vector3 rejectedMovementStartPosition = controller.transform.position;
                        int rejectedMovementCollisionCount = controller.CollisionCount;
                        int rejectedMovementRecoveryCount = controller.RecoveryCount;
                        yield return null;
                        float rejectedMovementDelta = Vector3.Distance(
                            controller.transform.position, rejectedMovementStartPosition);
                        float rejectedMovementLimit =
                            controller.CurrentMoveSpeed * Time.deltaTime + 0.001f;
                        bool rejectedMovementResumed = rejectedMovementDelta > 0.0001f &&
                            rejectedMovementDelta <= rejectedMovementLimit &&
                            controller.CollisionCount == rejectedMovementCollisionCount &&
                            controller.RecoveryCount == rejectedMovementRecoveryCount &&
                            controller.RoomBounds.Contains(new Vector2(
                                controller.transform.position.x,
                                controller.transform.position.z)) &&
                            spatialInteractor.CurrentZone == acceptedActionZone;
                        bool rejectedHoldMovementRetained = rejectedHoldLockObserved &&
                            !zone.LastGesturePassed && zone.ProgressStep == progressBeforeLiveHold &&
                            !pad.IsContextGestureMovementLockHeld &&
                            !controller.IsContextGestureMovementLocked &&
                            touchJoystick.ResetSequence == resetSequenceBeforeRejectedHold &&
                            touchJoystick.IsTrackingPointer &&
                            Vector2.Distance(touchJoystick.Value, liveHoldHeldInput) <= 0.0001f &&
                            Vector2.Distance(controller.TouchMove,
                                touchJoystick.Value) <= 0.0001f &&
                            rejectedMovementResumed;
                        controller.TeleportTo(rejectedMovementStartPosition,
                            controller.RoomBounds);
                        spatialInteractor.RescanNowForQA();
                        controller.SetProcessedTouchSprintForQA(liveHoldHeldInput);
                        bool rejectedMovementRestorePass =
                            Vector3.Distance(controller.transform.position,
                                rejectedMovementStartPosition) <= 0.001f &&
                            spatialInteractor.CurrentZone == acceptedActionZone &&
                            Vector2.Distance(controller.TouchMove,
                                liveHoldHeldInput) <= 0.0001f;
                        if (!rejectedHoldMovementRetained || !rejectedMovementRestorePass)
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] " +
                                $"ipad-context-gesture-rejected-retention action={zone.Kind} " +
                                $"lock={rejectedHoldLockObserved}/" +
                                $"{controller.IsContextGestureMovementLocked} " +
                                $"step={zone.ProgressStep}/{progressBeforeLiveHold} " +
                                $"reset={touchJoystick.ResetSequence}/" +
                                $"{resetSequenceBeforeRejectedHold} " +
                                $"tracking={touchJoystick.IsTrackingPointer} " +
                                $"value={touchJoystick.Value:F3} controller={controller.TouchMove:F3} " +
                                $"delta={rejectedMovementDelta:0.0000}/" +
                                $"{rejectedMovementLimit:0.0000} resumed={rejectedMovementResumed} " +
                                $"restored={rejectedMovementRestorePass}");
                            Application.Quit(144);
                            yield break;
                        }
                        verifiedRejectedGestureMovementRetention++;

                        pad.OnPointerDown(liveHoldPointer);
                        Vector3 movementLockStartPosition = controller.transform.position;
                        float liveHoldDeadline = Time.unscaledTime + 1.5f;
                        while (pad.LiveHoldScore <
                               MoonlightActionFeedback.PerfectActionQualityScore &&
                               Time.unscaledTime < liveHoldDeadline)
                            yield return null;
                        bool liveReadinessPass = pad.IsTrackingGesture &&
                            pad.IsLiveHoldReadinessActive && pad.LiveHoldIsReady &&
                            pad.LiveHoldReadinessOverlayVisible &&
                            pad.LiveHoldReadinessHapticPlayed &&
                            pad.LiveHoldReadinessHapticCount == 1 &&
                            pad.IsContextGestureMovementLockHeld &&
                            controller.IsContextGestureMovementLocked &&
                            touchJoystick.IsTrackingPointer &&
                            Vector2.Distance(touchJoystick.Value, liveHoldHeldInput) <= 0.0001f &&
                            Vector2.Distance(controller.TouchMove,
                                touchJoystick.Value) <= 0.0001f &&
                            Vector3.Distance(controller.transform.position,
                                movementLockStartPosition) <= 0.001f &&
                            spatialInteractor.CurrentZone == acceptedActionZone;
                        yield return null;
                        liveReadinessPass &= pad.LiveHoldReadinessHapticCount == 1 &&
                            controller.IsContextGestureMovementLocked &&
                            Vector3.Distance(controller.transform.position,
                                movementLockStartPosition) <= 0.001f &&
                            spatialInteractor.CurrentZone == acceptedActionZone;
                        resultTimingSelectionBeforeAccepted = ui != null
                            ? ui.ContextResultDurationSelectionCountForQA
                            : -1;
                        resultShownCountBeforeAccepted = ui != null
                            ? ui.ContextResultShownCountForQA
                            : -1;
                        pad.OnPointerUp(liveHoldPointer);
                        bool releaseCleanupPass = !pad.IsContextGestureMovementLockHeld &&
                            !controller.IsContextGestureMovementLocked;
                        if (releaseCleanupPass) verifiedGestureLockCleanup.Add("pointer-up");
                        acceptedActionStarted = zone.LastGesturePassed;
                        bool runtimeContractPass =
                            pad.ValidateLastLiveHoldReadinessRuntimeContract(
                                zone.Kind, out string liveHoldRuntimeDetail);
                        if (!liveReadinessPass || !releaseCleanupPass || !runtimeContractPass)
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] " +
                                $"ipad-live-hold-runtime action={zone.Kind} " +
                                $"readiness={liveReadinessPass} " +
                                $"release={releaseCleanupPass} drift=" +
                                $"{Vector3.Distance(controller.transform.position, movementLockStartPosition):0.0000} " +
                                $"zoneRetained={spatialInteractor.CurrentZone == acceptedActionZone} " +
                                $"score={pad.LastScore:0.000} " +
                                $"contract=({liveHoldRuntimeDetail})");
                            Application.Quit(141);
                            yield break;
                        }
                        verifiedLiveHoldRuntimeActions++;
                        Debug.Log($"[MoonlightGameplayQA][PASS] ipad-live-hold-runtime " +
                            $"{liveHoldRuntimeDetail} cancel={expectedCleanupReason} " +
                            $"marker=MOONLIGHT_IPAD_LIVE_HOLD_RUNTIME_ACTION_VERIFIED");
                    }
                    else
                    {
                        resultTimingSelectionBeforeAccepted = ui != null
                            ? ui.ContextResultDurationSelectionCountForQA
                            : -1;
                        resultShownCountBeforeAccepted = ui != null
                            ? ui.ContextResultShownCountForQA
                            : -1;
                        if (playContinuity != null && ui != null)
                        {
                            ui.ExecuteContextGesture(expected,
                                PlayContinuitySample(step, 0.95f));
                            acceptedActionStarted = zone.LastGesturePassed;
                        }
                        else
                        {
                            acceptedActionStarted = pad.SubmitSynthetic(expected, 0.95f);
                        }
                    }
                    var feedback = moonlight.GetComponent<MoonlightActionFeedback>();
                    if (cookContinuity != null)
                    {
                        var continuityStage = moonlight.GetComponent<MoonlightActivityStage>();
                        cookContinuity.Stage = continuityStage;
                        if (continuityStage == null)
                        {
                            Debug.LogError("[MoonlightGameplayQA][FAIL] " +
                                $"cook-continuity-stage-missing step={step + 1}/4");
                            Application.Quit(162);
                            yield break;
                        }

                        if (step == 0)
                        {
                            cookContinuity.RootId = continuityStage.CookStageRootInstanceId;
                            cookContinuity.MaterialIdentityCount =
                                continuityStage.CookStageMaterialIdentityCountForQA;
                            cookContinuity.MaterialIdentityHash =
                                continuityStage.CookStageMaterialIdentityHashForQA;
                            cookContinuity.LightId = continuityStage.CookStageLightInstanceId;
                        }
                        else
                        {
                            cookContinuity.HandoffCount++;
                            int handoffBit = 1 << (step - 1);
                            if (continuityStage.CookStageRootInstanceId ==
                                cookContinuity.RootId)
                                cookContinuity.RootIdentityHandoffMask |= handoffBit;
                            if (continuityStage.CookStageMaterialIdentityCountForQA ==
                                    cookContinuity.MaterialIdentityCount &&
                                continuityStage.CookStageMaterialIdentityHashForQA ==
                                    cookContinuity.MaterialIdentityHash)
                                cookContinuity.MaterialIdentityHandoffMask |= handoffBit;
                            if (continuityStage.CookStageLightInstanceId ==
                                cookContinuity.LightId)
                                cookContinuity.LightIdentityHandoffMask |= handoffBit;
                            int expectedSharedProps = step switch
                            {
                                1 => 3,
                                2 => 1,
                                _ => 7
                            };
                            if (continuityStage.LastCookSharedPropCountForQA ==
                                expectedSharedProps)
                                cookContinuity.SharedPropHandoffMask |= 1 << (step - 1);
                            cookContinuity.MaximumSharedPropDiscontinuity = Mathf.Max(
                                cookContinuity.MaximumSharedPropDiscontinuity,
                                continuityStage.LastCookSharedPropDiscontinuityForQA);
                        }

                        cookContinuity.IdentityPreserved &=
                            continuityStage.CookStageRootInstanceId == cookContinuity.RootId &&
                            continuityStage.CookStageMaterialIdentityCountForQA ==
                                cookContinuity.MaterialIdentityCount &&
                            continuityStage.CookStageMaterialIdentityHashForQA ==
                                cookContinuity.MaterialIdentityHash &&
                            continuityStage.CookStageLightInstanceId == cookContinuity.LightId &&
                            continuityStage.CookStageRootInstanceId != 0 &&
                            continuityStage.CookStageMaterialIdentityCountForQA > 0 &&
                            continuityStage.CookStageLightInstanceId != 0 &&
                            CountActiveCookStageRoots() == 1;
                        cookContinuity.BudgetContractPassed &=
                            MoonlightActivityStage.CookRendererBudget == 36 &&
                            MoonlightActivityStage.CookMaterialBudget == 24 &&
                            MoonlightActivityStage.CookLightBudget == 1 &&
                            continuityStage.ActiveRendererCount > 0 &&
                            continuityStage.ActiveRendererCount <=
                                MoonlightActivityStage.CookRendererBudget &&
                            continuityStage.ActiveUniqueMaterialCount > 0 &&
                            continuityStage.ActiveUniqueMaterialCount <=
                                MoonlightActivityStage.CookMaterialBudget &&
                            continuityStage.ActiveLightCount ==
                                MoonlightActivityStage.CookLightBudget;
                    }
                    if (playContinuity != null)
                    {
                        var continuityStage = moonlight.GetComponent<MoonlightActivityStage>();
                        playContinuity.Stage = continuityStage;
                        if (continuityStage == null)
                        {
                            Debug.LogError("[MoonlightGameplayQA][FAIL] " +
                                $"play-continuity-stage-missing step={step + 1}/4");
                            Application.Quit(161);
                            yield break;
                        }

                        if (step == 0)
                        {
                            playContinuity.RootId = continuityStage.PlayStageRootInstanceId;
                            playContinuity.BallId = continuityStage.PlayBallInstanceId;
                            playContinuity.TrailId = continuityStage.PlayTrailInstanceId;
                            playContinuity.CompletionApplicationBaseline =
                                continuityStage.PersistentCompletionApplicationCountForQA;
                            playContinuity.ContinuationBaseline =
                                continuityStage.PlayContinuationCountForQA;
                        }
                        else
                        {
                            playContinuity.MaterialCounts[step] =
                                continuityStage.ActiveUniqueMaterialCount;
                            float immediateDiscontinuity = playContinuity.HasHeldEndpoint
                                ? Vector3.Distance(playContinuity.HeldEndpoint,
                                    continuityStage.PlayBallLocalPosition)
                                : float.PositiveInfinity;
                            playContinuity.MaximumImmediateDiscontinuity = Mathf.Max(
                                playContinuity.MaximumImmediateDiscontinuity,
                                immediateDiscontinuity);
                            playContinuity.IdentityPreserved &= playContinuity.HasHeldEndpoint &&
                                playContinuity.HasInitialMaterialSnapshot &&
                                continuityStage.PlayStageRootInstanceId == playContinuity.RootId &&
                                continuityStage.PlayBallInstanceId == playContinuity.BallId &&
                                continuityStage.PlayTrailInstanceId == playContinuity.TrailId &&
                                PlayMaterialsMatchObservation(continuityStage,
                                    playContinuity) &&
                                continuityStage.LastPlayContinuationDiscontinuityForQA <= 0.001f &&
                                immediateDiscontinuity <= 0.001f;
                            StartCoroutine(ObservePlayFirstRenderedContinuationFrame(
                                continuityStage, playContinuity,
                                playContinuity.HeldEndpoint, step));
                        }

                        playContinuity.IdentityPreserved &=
                            continuityStage.AuthoritativePlayBallCount == 1 &&
                            continuityStage.AuthoritativePlayTrailCount == 1 &&
                            continuityStage.PlayBallSharedMaterialInstanceId != 0 &&
                            continuityStage.PlayTrailSharedMaterialInstanceId != 0 &&
                            CountActivePlayStageRoots() == 1;
                        playContinuity.Progression.Add(zone.ProgressStep);
                        playContinuity.AcceptedRewards[step] =
                            new PlayRewardSnapshot(moonlight);
                        StartCoroutine(ObservePlayContinuityTrajectory(feedback,
                            continuityStage, playContinuity));
                        if (step < zone.RequiredSteps - 1)
                            StartCoroutine(ObservePlayHeldBusyRejection(zone, pad,
                                moonlight, feedback, continuityStage, playContinuity));
                    }
                    if (observeActualResultVisibility)
                    {
                        string expectedVisibleText = finalResultStep
                            ? "MOONCAKES DECORATED"
                            : "INGREDIENTS ADDED";
                        resultVisibilityObservation = new ContextResultVisibilityObservation();
                        StartCoroutine(ObserveContextResultVisibility(ui, feedback,
                            resultShownCountBeforeAccepted, expectedVisibleText,
                            expectedResultDuration, resultVisibilityObservation));
                    }
                    yield return new WaitForSeconds(0.08f);
                    bool resultDisplayBoundaryPass = ui != null &&
                        resultTimingSelectionBeforeAccepted >= 0 &&
                        ui.ContextResultDurationSelectionCountForQA ==
                            resultTimingSelectionBeforeAccepted + 1 &&
                        Mathf.Approximately(ui.LastContextResultDurationSecondsForQA,
                            expectedResultDuration);
                    if (!resultDisplayBoundaryPass)
                    {
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-result-display-boundary " +
                            $"action={zone.Kind} step={step + 1}/{zone.RequiredSteps} " +
                            $"duration={(ui != null ? ui.LastContextResultDurationSecondsForQA : -1f):0.0}/" +
                            $"{expectedResultDuration:0.0}s selection=" +
                            $"{(ui != null ? ui.ContextResultDurationSelectionCountForQA : -1)}/" +
                            $"{resultTimingSelectionBeforeAccepted + 1}");
                        Application.Quit(147);
                        yield break;
                    }
                    Debug.Log($"[MoonlightGameplayQA][PASS] activity-result-display-boundary " +
                        $"action={zone.Kind} step={step + 1}/{zone.RequiredSteps} " +
                        $"duration={ui.LastContextResultDurationSecondsForQA:0.0}s " +
                        $"marker={ActivityResultDisplayBoundaryMarker}");
                    expectedSessionScoreTotal += zone.LastGestureScore;
                    float expectedSessionAverage = expectedSessionScoreTotal / (step + 1f);
                    if (zone.Kind == MoonlightSpatialActionKind.Care)
                    {
                        string expectedCareCue = MoonlightSpatialActionZone.CareCueForStep(step);
                        string expectedAudioCue = step == zone.RequiredSteps - 1
                            ? "activity-complete"
                            : expectedCareCue;
                        if (zone.LastCueKey != expectedCareCue || audio.LastCueKey != expectedAudioCue)
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] care-live-cue step={step + 1} " +
                                $"zone={zone.LastCueKey}/{expectedCareCue} " +
                                $"audio={audio.LastCueKey}/{expectedAudioCue}");
                            Application.Quit(102);
                            yield break;
                        }
                        Debug.Log($"[MoonlightGameplayQA][PASS] care-live-cue " +
                            $"step={step + 1}/4 zone={zone.LastCueKey} audio={audio.LastCueKey}");
                    }
                    if (expectIPadHud)
                    {
                        bool acceptedMovementNeutralized = acceptedActionStarted &&
                            feedback != null && feedback.IsPerformingAction &&
                            touchJoystick.ResetSequence == resetSequenceBeforeAcceptedAction + 1 &&
                            touchJoystick.LastResetReason == "activity-accepted" &&
                            touchJoystick.ActivityMovementNeutralizationQAMarker ==
                                "MOONLIGHT_IPAD_ACTIVITY_MOVEMENT_NEUTRALIZED" &&
                            touchJoystick.IsInputNeutral && !touchJoystick.IsTrackingPointer &&
                            touchJoystick.Value.sqrMagnitude <= 0.0001f &&
                            touchJoystick.KnobAnchoredPosition.sqrMagnitude <= 0.0001f &&
                            controller.TouchMove.sqrMagnitude <= 0.0001f &&
                            !controller.IsIPadSprinting;
                        if (!acceptedMovementNeutralized)
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-movement-neutralized " +
                                $"action={zone.Kind} step={step + 1} accepted={acceptedActionStarted} " +
                                $"performing={(feedback != null && feedback.IsPerformingAction)} " +
                                $"reset={touchJoystick.ResetSequence}/" +
                                $"{resetSequenceBeforeAcceptedAction + 1} " +
                                $"reason={touchJoystick.LastResetReason} tracking={touchJoystick.IsTrackingPointer} " +
                                $"value={touchJoystick.Value:F3} knob={touchJoystick.KnobAnchoredPosition:F2} " +
                                $"controller={controller.TouchMove:F3} " +
                                $"marker={touchJoystick.ActivityMovementNeutralizationQAMarker}");
                            Application.Quit(81);
                            yield break;
                        }
                        Debug.Log($"[MoonlightGameplayQA][PASS] activity-movement-neutralized " +
                            $"action={zone.Kind} step={step + 1} pointerReleased={!touchJoystick.IsTrackingPointer} " +
                            $"value={touchJoystick.Value:F3} knob={touchJoystick.KnobAnchoredPosition:F2} " +
                            $"controller={controller.TouchMove:F3} " +
                            "marker=MOONLIGHT_IPAD_ACTIVITY_MOVEMENT_NEUTRALIZED");
                    }
                    int acceptedProgress = zone.ProgressStep;
                    float inFlightProgressFill = ui != null ? ui.ActivityProgressFill01 : 0f;
                    float completedFill = step / (float)zone.RequiredSteps;
                    float stepCeiling = (step + 1f) / zone.RequiredSteps;
                    bool inFlightFillPass = !expectIPadHud || ui == null ||
                        (inFlightProgressFill > completedFill &&
                         inFlightProgressFill < stepCeiling);
                    int resetSequenceBeforeBusyAction = touchJoystick != null
                        ? touchJoystick.ResetSequence
                        : -1;
                    Vector2 busyHeldInput = new(-0.64f, 0.42f);
                    bool exerciseBusyMovementRetention = expectIPadHud && step > 0;
                    if (exerciseBusyMovementRetention)
                        touchJoystick.ArmHeldInputForQA(busyHeldInput);
                    bool acceptedWhileBusy = pad.SubmitSynthetic(expected, 0.95f);
                    bool busyMovementRetained = !exerciseBusyMovementRetention ||
                        (touchJoystick.ResetSequence == resetSequenceBeforeBusyAction &&
                         touchJoystick.IsTrackingPointer &&
                         Vector2.Distance(touchJoystick.Value, busyHeldInput) <= 0.0001f &&
                         touchJoystick.KnobAnchoredPosition.sqrMagnitude > 0.0001f &&
                         controller.TouchMove.sqrMagnitude <= 0.0001f);
                    if (acceptedWhileBusy || !inFlightFillPass ||
                        zone.ProgressStep != acceptedProgress ||
                        string.IsNullOrEmpty(pad.LastRejectionReason) || !busyMovementRetained)
                    {
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] busy-gesture action={zone.Kind} " +
                            $"step={step + 1} accepted={acceptedWhileBusy} progress={zone.ProgressStep} " +
                            $"fill={inFlightProgressFill:0.000} range={completedFill:0.000}-" +
                            $"{stepCeiling:0.000} " +
                            $"reason=\"{pad.LastRejectionReason}\" retained={busyMovementRetained} " +
                            $"tracking={(touchJoystick != null && touchJoystick.IsTrackingPointer)} " +
                            $"value={(touchJoystick != null ? touchJoystick.Value : Vector2.zero):F3} " +
                            $"controller={controller.TouchMove:F3}");
                        Application.Quit(83);
                        yield break;
                    }
                    busyGestureProbeCount++;
                    Debug.Log($"[MoonlightGameplayQA][PASS] busy-gesture action={zone.Kind} " +
                        $"step={step + 1} reason=\"{pad.LastRejectionReason}\" " +
                        $"heldMovementRetained={busyMovementRetained} " +
                        $"count={busyGestureProbeCount}/20 " +
                        "marker=MOONLIGHT_BUSY_GESTURE_REJECTED");
                    if (exerciseBusyMovementRetention)
                        touchJoystick.ClearInputForQA();
                    bool finalMasteryStep = step == zone.RequiredSteps - 1;
                    bool masteryStatePass = finalMasteryStep
                        ? zone.ActivitySessionAcceptedSteps == 0 &&
                          Approximately(zone.LastCompletedAverageScore, expectedSessionAverage) &&
                          zone.LastCompletedBestCombo == zone.RequiredSteps &&
                          zone.LastCompletedPerfectSteps == zone.RequiredSteps &&
                          zone.LastMasteryBonusCoins == 3
                        : zone.ActivitySessionAcceptedSteps == step + 1 &&
                          Approximately(zone.ActivitySessionAverageScore, expectedSessionAverage) &&
                          zone.ActivityCurrentCombo == step + 1 &&
                          zone.ActivityBestCombo == step + 1 &&
                          zone.ActivityPerfectSteps == step + 1;
                    if (!masteryStatePass)
                    {
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-mastery-state action={zone.Kind} " +
                            $"step={step + 1} accepted={zone.ActivitySessionAcceptedSteps} " +
                            $"average={zone.ActivitySessionAverageScore:0.00} combo={zone.ActivityCurrentCombo}/" +
                            $"{zone.ActivityBestCombo} perfect={zone.ActivityPerfectSteps} " +
                            $"lastAverage={zone.LastCompletedAverageScore:0.00} " +
                            $"lastCombo={zone.LastCompletedBestCombo} lastPerfect={zone.LastCompletedPerfectSteps} " +
                            $"bonus={zone.LastMasteryBonusCoins}");
                        Application.Quit(60);
                        yield break;
                    }
                    Debug.Log($"[MoonlightGameplayQA][PASS] activity-mastery-state action={zone.Kind} " +
                        $"step={step + 1}/{zone.RequiredSteps} " +
                        $"average={(finalMasteryStep ? zone.LastCompletedAverageScore : zone.ActivitySessionAverageScore):0.00} " +
                        $"combo={(finalMasteryStep ? zone.LastCompletedBestCombo : zone.ActivityBestCombo)} " +
                        $"bonus={(finalMasteryStep ? zone.LastMasteryBonusCoins : 0)} " +
                        "marker=MOONLIGHT_ACTIVITY_MASTERY_STATE_VERIFIED");
                    string acceptedQualityDetail = "";
                    if (expectIPadHud &&
                        !ValidateActivityQualityReadoutRuntimeBinding(ui, zone, true,
                            finalMasteryStep, out acceptedQualityDetail))
                    {
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] " +
                            $"activity-quality-accepted action={zone.Kind} " +
                            $"step={step + 1}/{zone.RequiredSteps} {acceptedQualityDetail}");
                        Application.Quit(155);
                        yield break;
                    }
                    if (expectIPadHud)
                        Debug.Log($"[MoonlightGameplayQA][PASS] activity-quality-accepted " +
                            $"action={zone.Kind} step={step + 1}/{zone.RequiredSteps} " +
                            $"{acceptedQualityDetail} " +
                            $"marker={ActivityQualityReadoutRuntimeContractMarker}");
                    if (expectIPadHud && ui != null &&
                        (ui.IsRoomNavigationVisible || !ui.IsRoomNavigationLocked))
                    {
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-room-navigation action={zone.Kind} " +
                            $"step={step + 1} visible={ui.IsRoomNavigationVisible} " +
                            $"locked={ui.IsRoomNavigationLocked} marker={ui.RoomNavigationQAMarker}");
                        Application.Quit(50);
                        yield break;
                    }
                    if (expectIPadHud && ui != null)
                        Debug.Log($"[MoonlightGameplayQA][PASS] activity-room-navigation action={zone.Kind} " +
                            $"step={step + 1} marker={ui.RoomNavigationQAMarker}");
                    feedback = moonlight.GetComponent<MoonlightActionFeedback>();
                    if (zone.Kind is MoonlightSpatialActionKind.Cook or MoonlightSpatialActionKind.Play or
                        MoonlightSpatialActionKind.Garden or MoonlightSpatialActionKind.Read or
                        MoonlightSpatialActionKind.Care)
                    {
                        string expectedVisualSignature = MoonlightActionFeedback.ActionVisualSignatureFor(
                            zone.Kind, step, feedback != null ? feedback.StateText : "");
                        string expectedVisualMarker = MoonlightActionFeedback.ActionVisualSignatureMarkerFor(
                            zone.Kind, step, feedback != null ? feedback.StateText : "");
                        if (!ValidateActionAccent(feedback, expectedVisualSignature, expectedVisualMarker,
                                true, out string actionAccentDetail))
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] action-contact-prop " +
                                $"action={zone.Kind} step={step + 1} {actionAccentDetail}");
                            Application.Quit(84);
                            yield break;
                        }
                        Debug.Log($"[MoonlightGameplayQA][PASS] action-contact-prop action={zone.Kind} " +
                            $"step={step + 1} {actionAccentDetail}");

                        string expectedContactTarget = zone.Kind switch
                        {
                            MoonlightSpatialActionKind.Cook => step switch
                            {
                                0 => "bowl", 1 => "whisk", 2 => "oven-tray", _ => "cookies"
                            },
                            MoonlightSpatialActionKind.Play => step switch
                            {
                                0 => "ball-launch", 1 => "ball-path", 2 => "jump-arc", _ => "ball-catch"
                            },
                            MoonlightSpatialActionKind.Garden => step switch
                            {
                                0 => "seed-bed", 1 => "watering-spout", 2 => "flower-bed", _ => "bloom-center"
                            },
                            MoonlightSpatialActionKind.Read => step switch
                            {
                                0 => "book-cover", 1 => "turning-page", 2 => "bookmark-trace", _ => "memory-motes"
                            },
                            _ => step switch
                            {
                                0 => "towel-tray", 1 => "bubble-brush", 2 => "moon-comb", _ => "vanity-mirror"
                            }
                        };

                        const float contactWeightThreshold = 0.20f;
                        bool requiresCameraReadableFacing = zone.Kind is MoonlightSpatialActionKind.Garden or
                            MoonlightSpatialActionKind.Read or MoonlightSpatialActionKind.Care;
                        const float cameraFacingMinAngle = 20f;
                        const float cameraFacingMaxAngle = 38f;
                        float contactMaxDistance = zone.Kind is MoonlightSpatialActionKind.Garden or
                            MoonlightSpatialActionKind.Read ? 4.0f : 5.5f;
                        float visualContactMaxDistance = zone.Kind switch
                        {
                            MoonlightSpatialActionKind.Cook => 1.55f,
                            MoonlightSpatialActionKind.Play => 2.0f,
                            MoonlightSpatialActionKind.Garden => 1.75f,
                            MoonlightSpatialActionKind.Read => 1.65f,
                            _ => 1.75f
                        };
                        float contactDeadline = Time.time + 1.10f;
                        bool observedContact = false;
                        bool observedValidContact = false;
                        bool peakContactPerforming = false;
                        bool peakContactFinite = false;
                        bool peakContactInViewport = false;
                        bool peakCameraReadableFacing = false;
                        bool peakUsesLiveStageContact = false;
                        int contactSamples = 0;
                        float peakContactTime = 0f;
                        float peakContactWeight = -1f;
                        float peakContactDistance = float.PositiveInfinity;
                        float peakVisualContactDistance = float.PositiveInfinity;
                        float peakAccentContactDistance = float.PositiveInfinity;
                        float peakCameraFacingAngle = 180f;
                        string peakContactPhase = "";
                        string peakContactTarget = "";
                        string peakContactSource = "";
                        Vector3 peakContactPoint = Vector3.zero;
                        Vector3 peakContactViewport = Vector3.zero;
                        Camera contactCamera = Camera.main;

                        while (feedback != null && feedback.IsPerformingAction && Time.time < contactDeadline)
                        {
                            bool isContactPhase = feedback.IsActionContactActive &&
                                feedback.ActionContactPhase == "contact";
                            if (isContactPhase)
                            {
                                observedContact = true;
                                contactSamples++;
                                Vector3 sampledPoint = feedback.ActionContactPoint;
                                bool finiteContact = !float.IsNaN(sampledPoint.x) && !float.IsNaN(sampledPoint.y) &&
                                    !float.IsNaN(sampledPoint.z) && !float.IsInfinity(sampledPoint.x) &&
                                    !float.IsInfinity(sampledPoint.y) && !float.IsInfinity(sampledPoint.z);
                                float sampledDistance = finiteContact
                                    ? Vector3.Distance(moonlight.transform.position, sampledPoint)
                                    : float.PositiveInfinity;
                                float sampledWeight = feedback.ActionContactWeight;
                                float sampledVisualDistance = feedback.ActionVisualContactPlanarDistance;
                                float sampledAccentContactDistance = feedback.ActionAccentContactDistance;
                                Vector3 sampledViewport = finiteContact && contactCamera != null
                                    ? contactCamera.WorldToViewportPoint(sampledPoint)
                                    : Vector3.zero;
                                bool sampledInViewport = contactCamera != null && sampledViewport.z > 0f &&
                                    sampledViewport.x >= 0.05f && sampledViewport.x <= 0.95f &&
                                    sampledViewport.y >= 0.08f && sampledViewport.y <= 0.92f;
                                float sampledCameraFacingAngle = feedback.ActionCameraFacingAngle;
                                bool sampledCameraReadableFacing = !requiresCameraReadableFacing ||
                                    (feedback.UsesCameraReadableFacing &&
                                     sampledCameraFacingAngle >= cameraFacingMinAngle &&
                                     sampledCameraFacingAngle <= cameraFacingMaxAngle);
                                bool validContactSample = feedback.IsPerformingAction &&
                                    feedback.ActionContactTarget == expectedContactTarget &&
                                    feedback.UsesLiveStageContact &&
                                    feedback.ActionContactSource == "activity-stage" &&
                                    sampledWeight >= contactWeightThreshold && finiteContact &&
                                    sampledDistance <= contactMaxDistance &&
                                    sampledVisualDistance <= visualContactMaxDistance &&
                                    sampledAccentContactDistance <= 0.01f && sampledInViewport &&
                                    sampledCameraReadableFacing;
                                bool hadValidContact = observedValidContact;
                                observedValidContact |= validContactSample;
                                bool useSample = validContactSample
                                    ? (!hadValidContact || sampledWeight > peakContactWeight)
                                    : (!observedValidContact && sampledWeight > peakContactWeight);
                                if (useSample)
                                {
                                    peakContactTime = Time.time;
                                    peakContactWeight = sampledWeight;
                                    peakContactPhase = feedback.ActionContactPhase;
                                    peakContactTarget = feedback.ActionContactTarget;
                                    peakContactSource = feedback.ActionContactSource;
                                    peakUsesLiveStageContact = feedback.UsesLiveStageContact;
                                    peakContactPoint = sampledPoint;
                                    peakContactDistance = sampledDistance;
                                    peakVisualContactDistance = sampledVisualDistance;
                                    peakAccentContactDistance = sampledAccentContactDistance;
                                    peakContactFinite = finiteContact;
                                    peakContactViewport = sampledViewport;
                                    peakContactInViewport = sampledInViewport;
                                    peakCameraFacingAngle = sampledCameraFacingAngle;
                                    peakCameraReadableFacing = sampledCameraReadableFacing;
                                    peakContactPerforming = feedback.IsPerformingAction;
                                }
                                if (validContactSample)
                                    break;
                            }
                            else if (observedValidContact)
                            {
                                break;
                            }
                            yield return null;
                        }

                        bool contactPass = observedValidContact && peakContactPerforming &&
                            peakContactPhase == "contact" &&
                            peakContactTarget == expectedContactTarget &&
                            peakUsesLiveStageContact && peakContactSource == "activity-stage" &&
                            peakContactWeight >= contactWeightThreshold && peakContactFinite &&
                            peakContactDistance <= contactMaxDistance &&
                            peakVisualContactDistance <= visualContactMaxDistance &&
                            peakAccentContactDistance <= 0.01f && peakContactInViewport &&
                            peakCameraReadableFacing;
                        if (!contactPass)
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] action-contact action={zone.Kind} " +
                                $"step={step + 1} observed={observedContact} valid={observedValidContact} " +
                                $"samples={contactSamples} " +
                                $"phase={peakContactPhase} target={peakContactTarget}/{expectedContactTarget} " +
                                $"source={peakContactSource} liveStage={peakUsesLiveStageContact} " +
                                $"weight={peakContactWeight:0.00} threshold={contactWeightThreshold:0.00} " +
                                $"point={peakContactPoint:F2} distance={peakContactDistance:0.00} " +
                                $"visualDistance={peakVisualContactDistance:0.00}/{visualContactMaxDistance:0.00} " +
                                $"accentContactDistance={peakAccentContactDistance:0.000}/0.010 " +
                                $"viewport={peakContactViewport:F2} inViewport={peakContactInViewport} " +
                                $"cameraFacing={peakCameraFacingAngle:0.0}/{cameraFacingMinAngle:0}-{cameraFacingMaxAngle:0} " +
                                $"readableFacing={peakCameraReadableFacing} " +
                                $"finite={peakContactFinite} performing={peakContactPerforming}");
                            Application.Quit(44);
                            yield break;
                        }
                        if (zone.Kind == MoonlightSpatialActionKind.Care)
                            verifiedCareContacts.Add(peakContactTarget);

                        string contactPrefix = zone.Kind switch
                        {
                            MoonlightSpatialActionKind.Cook => "02",
                            MoonlightSpatialActionKind.Play => "03",
                            MoonlightSpatialActionKind.Garden => "04",
                            MoonlightSpatialActionKind.Read => "05",
                            _ => "06"
                        };
                        string contactShot = Path.Combine(output,
                            $"{contactPrefix}_{zone.Kind.ToString().ToLowerInvariant()}_step_{step + 1}_contact.png");
                        yield return Capture(contactShot);
                        Debug.Log($"[MoonlightGameplayQA][PASS] action-contact action={zone.Kind} " +
                            $"step={step + 1} samples={contactSamples} phase={peakContactPhase} " +
                            $"target={peakContactTarget} weight={peakContactWeight:0.00} " +
                            $"source={peakContactSource} liveStage={peakUsesLiveStageContact} " +
                            $"threshold={contactWeightThreshold:0.00} point={peakContactPoint:F2} " +
                            $"distance={peakContactDistance:0.00} sampleTime={peakContactTime:0.00} " +
                            $"visualDistance={peakVisualContactDistance:0.00}/{visualContactMaxDistance:0.00} " +
                            $"accentContactDistance={peakAccentContactDistance:0.000}/0.010 " +
                            $"viewport={peakContactViewport:F2} inViewport={peakContactInViewport} " +
                            $"cameraFacing={peakCameraFacingAngle:0.0} readableFacing={peakCameraReadableFacing} " +
                            $"signature={feedback.ActionVisualSignature} " +
                            $"signatureMarker={feedback.ActionVisualSignatureMarker} " +
                            $"renderers={feedback.ActionAccentRendererCount} " +
                            $"colliders={feedback.ActionAccentColliderCount} " +
                            $"materials={feedback.ActionAccentMaterialCount} " +
                            $"bounds={feedback.ActionAccentBoundsSize:F3} " +
                            $"screenshot={contactShot} marker=MOONLIGHT_THREE_QUARTER_FACING_VERIFIED");

                        float followThroughDeadline = Time.time + 2.40f;
                        while (feedback.IsPerformingAction && feedback.ActionContactPhase != "follow-through" &&
                               Time.time < followThroughDeadline)
                            yield return null;
                        if (!feedback.IsPerformingAction || feedback.ActionContactPhase != "follow-through")
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] action-follow-through action={zone.Kind} " +
                                $"step={step + 1} phase={feedback.ActionContactPhase}");
                            Application.Quit(45);
                            yield break;
                        }
                        Debug.Log($"[MoonlightGameplayQA][PASS] action-follow-through action={zone.Kind} " +
                            $"step={step + 1} phase={feedback.ActionContactPhase} " +
                            "marker=MOONLIGHT_ACTION_FOLLOW_THROUGH_VERIFIED");
                    }
                    var stage = moonlight.GetComponent<MoonlightActivityStage>();
                    if (!zone.LastGesturePassed || stage == null || !stage.IsVisible ||
                        audio.LastCueKey == "activity-try-again")
                    {
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] scored-step action={zone.Kind} " +
                            $"step={step + 1} gesture={expected} score={zone.LastGestureScore:0.00} cue={audio.LastCueKey}");
                        Application.Quit(25);
                        yield break;
                    }

                    feedback = moonlight.GetComponent<MoonlightActionFeedback>();
                    if (feedback == null || feedback.ActivityStep != step ||
                        feedback.ActivityRequiredSteps != zone.RequiredSteps || stage.CurrentStep != step)
                    {
                        int feedbackStep = feedback != null ? feedback.ActivityStep + 1 : -1;
                        int feedbackTotal = feedback != null ? feedback.ActivityRequiredSteps : -1;
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-step-state action={zone.Kind} " +
                            $"expected={step + 1}/{zone.RequiredSteps} feedback={feedbackStep}/{feedbackTotal} " +
                            $"stage={stage.CurrentStep + 1}");
                        Application.Quit(28);
                        yield break;
                    }

                    if (zone.Kind is MoonlightSpatialActionKind.Cook or MoonlightSpatialActionKind.Play or
                        MoonlightSpatialActionKind.Garden or MoonlightSpatialActionKind.Read or
                        MoonlightSpatialActionKind.Care)
                    {
                        string expectedMotionProfile = zone.Kind switch
                        {
                            MoonlightSpatialActionKind.Cook => step switch
                            {
                                0 => "cook-add-pour", 1 => "cook-stir-circle",
                                2 => "cook-bake-rise", _ => "cook-decorate-dots"
                            },
                            MoonlightSpatialActionKind.Play => step switch
                            {
                                0 => "play-throw-follow-through", 1 => "play-chase-dash",
                                2 => "play-jump-hop", _ => "play-catch-reach"
                            },
                            MoonlightSpatialActionKind.Garden => step switch
                            {
                                0 => "garden-plant-scoop", 1 => "garden-water-circle",
                                2 => "garden-tend-zigzag", _ => "garden-bloom-rise"
                            },
                            MoonlightSpatialActionKind.Read => step switch
                            {
                                0 => "read-open-settle", 1 => "read-turn-swipe",
                                2 => "read-trace-circle", _ => "read-remember-glow"
                            },
                            MoonlightSpatialActionKind.Care => step switch
                            {
                                0 => "care-towel-warm-press", 1 => "care-bubble-brush-circle",
                                2 => "care-moon-comb-sweep", _ => "care-mirror-glow-hold"
                            },
                            _ => string.Empty
                        };
                        if (feedback.ActionMotionProfile != expectedMotionProfile)
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] action-motion-profile action={zone.Kind} " +
                                $"step={step + 1} expected={expectedMotionProfile} actual={feedback.ActionMotionProfile}");
                            Application.Quit(34);
                            yield break;
                        }
                        Debug.Log($"[MoonlightGameplayQA][PASS] action-motion-profile action={zone.Kind} " +
                            $"step={step + 1} profile={feedback.ActionMotionProfile}");

                        var activityCamera = Camera.main != null
                            ? Camera.main.GetComponent<CameraController>()
                            : null;
                        bool focusMatchesAction = activityCamera != null &&
                            activityCamera.ActivityFocusRequested &&
                            activityCamera.ActivityFocusKind == zone.Kind &&
                            feedback.IsCameraFocusActive;
                        if (!focusMatchesAction)
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-camera-focus action={zone.Kind} " +
                                $"step={step + 1} camera={(activityCamera != null)} " +
                                $"requested={(activityCamera != null && activityCamera.ActivityFocusRequested)} " +
                                $"feedback={(feedback != null && feedback.IsCameraFocusActive)}");
                            Application.Quit(40);
                            yield break;
                        }
                        string expectedFramingProfile = zone.Kind switch
                        {
                            MoonlightSpatialActionKind.Cook => "cook-three-quarter",
                            MoonlightSpatialActionKind.Play => "play-wide-arena",
                            MoonlightSpatialActionKind.Garden => "garden-close-bloom",
                            MoonlightSpatialActionKind.Read => "read-intimate-nook",
                            MoonlightSpatialActionKind.Care => "care-intimate-vanity",
                            _ => "activity-standard"
                        };
                        if (activityCamera.ActivityFocusFramingProfile != expectedFramingProfile)
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-camera-profile action={zone.Kind} " +
                                $"expected={expectedFramingProfile} " +
                                $"actual={activityCamera.ActivityFocusFramingProfile}");
                            Application.Quit(46);
                            yield break;
                        }
                        Debug.Log($"[MoonlightGameplayQA][PASS] activity-camera-focus action={zone.Kind} " +
                            $"step={step + 1} source={activityCamera.ActivityFocusSource} " +
                            $"profile={activityCamera.ActivityFocusFramingProfile} " +
                            $"blend={activityCamera.ActivityFocusBlend:0.00} center={activityCamera.ActivityFocusCenter:F2} " +
                            "marker=MOONLIGHT_ACTIVITY_FOCUS_VERIFIED");

                        if (stage.ActiveRendererCount <= 0 || stage.ActiveRendererCount > 48 ||
                            stage.ActiveUniqueMaterialCount <= 0 || stage.ActiveUniqueMaterialCount > 28 ||
                            stage.ActiveLightCount != 1)
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-budget action={zone.Kind} " +
                                $"step={step + 1} renderers={stage.ActiveRendererCount} " +
                                $"materials={stage.ActiveUniqueMaterialCount} lights={stage.ActiveLightCount}");
                            Application.Quit(29);
                            yield break;
                        }

                        bool surfaceDepthPass = stage.ConfiguredSurfaceProfileCount >= 3 &&
                            stage.HasDepthLighting &&
                            stage.SurfaceDepthQAMarker == "MOONLIGHT_ACTIVITY_SURFACE_DEPTH_READY";
                        if (!surfaceDepthPass)
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-surface-depth " +
                                $"action={zone.Kind} profiles={stage.ConfiguredSurfaceProfileCount}/3 " +
                                $"depthLight={stage.HasDepthLighting} marker={stage.SurfaceDepthQAMarker}");
                            Application.Quit(53);
                            yield break;
                        }
                        Debug.Log($"[MoonlightGameplayQA][PASS] activity-surface-depth " +
                            $"action={zone.Kind} profiles={stage.ConfiguredSurfaceProfileCount} " +
                            $"depthLight={stage.HasDepthLighting} " +
                            "marker=MOONLIGHT_ACTIVITY_SURFACE_DEPTH_VERIFIED");

                        if (zone.Kind == MoonlightSpatialActionKind.Cook)
                        {
                            bool cookSampleEqualityPass = pad != null &&
                                zone.LastGestureSample.PointCount ==
                                    MoonlightGestureSample.ResampledPointCount &&
                                zone.LastGestureSample.HasSevenFiniteNormalizedPoints &&
                                feedback.ActiveGestureSample.PointCount ==
                                    MoonlightGestureSample.ResampledPointCount &&
                                feedback.ActiveGestureSample.HasSevenFiniteNormalizedPoints &&
                                stage.ActiveGestureSample.PointCount ==
                                    MoonlightGestureSample.ResampledPointCount &&
                                stage.ActiveGestureSample.HasSevenFiniteNormalizedPoints &&
                                pad.LastSample.ContentEquals(zone.LastGestureSample) &&
                                zone.LastGestureSample.ContentEquals(
                                    feedback.ActiveGestureSample) &&
                                feedback.ActiveGestureSample.ContentEquals(
                                    stage.ActiveGestureSample);
                            bool gesturePathStep = step == 1 || step == 3;
                            bool cookGestureKindPass = step switch
                            {
                                1 => expected == MoonlightGestureKind.Circle,
                                3 => expected == MoonlightGestureKind.ZigZag,
                                _ => true
                            };
                            bool cookTransformAgreementPass = !gesturePathStep ||
                                stage.CookGesturePathTransformAgreement;
                            bool cookGestureShapePass = !gesturePathStep ||
                                stage.CookGestureInputReady;
                            bool cookImprintPass = step != 3 ||
                                (stage.CookCookieMarksRetainGestureImprint &&
                                 stage.CookGestureResultQAMarker ==
                                     MoonlightActivityStage.CookGesturePersonalizedResultMarker);
                            bool unchangedCookBudgets =
                                MoonlightActivityStage.CookRendererBudget == 36 &&
                                MoonlightActivityStage.CookMaterialBudget == 24 &&
                                MoonlightActivityStage.CookLightBudget == 1 &&
                                stage.ActiveRendererCount <=
                                    MoonlightActivityStage.CookRendererBudget &&
                                stage.ActiveUniqueMaterialCount <=
                                    MoonlightActivityStage.CookMaterialBudget &&
                                stage.ActiveLightCount == MoonlightActivityStage.CookLightBudget;
                            if (!cookSampleEqualityPass || !cookGestureKindPass ||
                                !cookTransformAgreementPass || !cookGestureShapePass ||
                                !cookImprintPass || !unchangedCookBudgets)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] gesture-cook-runtime " +
                                    $"step={step + 1}/4 padPoints=" +
                                    $"{(pad != null ? pad.LastSample.PointCount : 0)} " +
                                    $"zonePoints={zone.LastGestureSample.PointCount} " +
                                    $"feedbackPoints={feedback.ActiveGestureSample.PointCount} " +
                                    $"stagePoints={stage.ActiveGestureSample.PointCount} " +
                                    $"sampleEquality={cookSampleEqualityPass} " +
                                    $"kind={expected}/{cookGestureKindPass} " +
                                    $"transformAgreement={cookTransformAgreementPass} " +
                                    $"shape={cookGestureShapePass} span=" +
                                    $"{stage.CookGestureHasMinimumPathSpan} traversal=" +
                                    $"{stage.CookGestureTraversalDirectionAgreement} distinct=" +
                                    $"{stage.CookDistinctGestureImprintCount}/9 " +
                                    $"actual={stage.CookGesturePropLocalPosition:F3} " +
                                    $"expected={stage.CookExpectedGesturePropLocalPosition:F3} " +
                                    $"imprint={stage.CookCookieMarksRetainGestureImprint} " +
                                    $"resultMarker={stage.CookGestureResultQAMarker} " +
                                    $"budget=({stage.CookBudgetEvidence})");
                                Application.Quit(117);
                                yield break;
                            }
                            Debug.Log("[MoonlightGameplayQA][PASS] gesture-cook-runtime " +
                                $"step={step + 1}/4 points={stage.ActiveGestureSample.PointCount} " +
                                $"sampleEquality={cookSampleEqualityPass} " +
                                $"kind={expected} shape={cookGestureShapePass} " +
                                $"transformAgreement={cookTransformAgreementPass} " +
                                $"actual={stage.CookGesturePropLocalPosition:F3} " +
                                $"expected={stage.CookExpectedGesturePropLocalPosition:F3} " +
                                $"imprint={cookImprintPass} resultMarker={stage.CookGestureResultQAMarker} " +
                                $"budget=({stage.CookBudgetEvidence}) " +
                                "marker=MOONLIGHT_GESTURE_COOK_RUNTIME_VERIFIED");

                            bool cookSourcePass =
                                stage.ValidateCookWorkbenchSourceRuntimeContract(
                                    out string cookSourceDetail);
                            if (cookContinuity != null)
                            {
                                cookContinuity.SourceObservedMask |= 1 << step;
                                if (string.IsNullOrEmpty(cookContinuity.Source))
                                    cookContinuity.Source =
                                        stage.CookWorkbenchVisualSourceForQA;
                                cookContinuity.SourceContractPassed &= cookSourcePass &&
                                    cookContinuity.Source ==
                                        stage.CookWorkbenchVisualSourceForQA;
                            }
                            if (!cookSourcePass)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] cook-workbench-source " +
                                    cookSourceDetail);
                                Application.Quit(32);
                                yield break;
                            }
                            Debug.Log("[MoonlightGameplayQA][PASS] cook-workbench-source " +
                                cookSourceDetail +
                                " marker=MOONLIGHT_COOK_WORKBENCH_SOURCE_EXCLUSIVE");

                            string expectedCookPhase = MoonlightActivityStage.CookPhaseName(step);
                            int expectedMotionProps =
                                MoonlightActivityStage.CookPhaseMinimumMotionPropCount(step);
                            int expectedVisibleMotionProps =
                                MoonlightActivityStage.CookPhaseMinimumVisibleMotionPropCount(step);
                            string expectedCookMarker =
                                MoonlightActivityStage.CookPhaseReadyMarker(step);
                            bool cookProgressValid =
                                !float.IsNaN(stage.CookCurrentPhaseProgress) &&
                                !float.IsInfinity(stage.CookCurrentPhaseProgress) &&
                                stage.CookCurrentPhaseProgress > 0f &&
                                stage.CookCurrentPhaseProgress <= 1f;
                            bool cookChoreographyPass = stage.HasCompleteCookChoreography &&
                                stage.CookChoreographyReadyMask ==
                                    MoonlightActivityStage.CookRequiredPhaseMask &&
                                stage.CookCurrentPhaseName == expectedCookPhase &&
                                cookProgressValid &&
                                stage.CookCurrentPhaseMotionPropCount == expectedMotionProps &&
                                stage.CookCurrentPhaseVisibleMotionPropCount >=
                                    expectedVisibleMotionProps &&
                                stage.CookCurrentPhaseMotionReady &&
                                stage.CookCurrentPhaseStateReady &&
                                (step != 2 || stage.CookBakeDoorClearancePass) &&
                                stage.CookBudgetReady &&
                                stage.CookPhaseQAMarker == expectedCookMarker;
                            if (!cookChoreographyPass)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] cook-choreography-live " +
                                    $"step={step + 1}/{MoonlightActivityStage.CookPhaseCount} " +
                                    $"phase={stage.CookCurrentPhaseName}/{expectedCookPhase} " +
                                    $"mask=0x{stage.CookChoreographyReadyMask:X}/" +
                                    $"0x{MoonlightActivityStage.CookRequiredPhaseMask:X} " +
                                    $"motionProps={stage.CookCurrentPhaseMotionPropCount}/" +
                                    $"{expectedMotionProps} visibleMotionProps=" +
                                    $"{stage.CookCurrentPhaseVisibleMotionPropCount}/" +
                                    $">={expectedVisibleMotionProps} " +
                                    $"motionReady={stage.CookCurrentPhaseMotionReady} " +
                                    $"progress={stage.CookCurrentPhaseProgress:0.000} " +
                                    $"motionEvidence=({stage.CookCurrentPhaseMotionEvidence}) " +
                                    $"stateReady={stage.CookCurrentPhaseStateReady} " +
                                    $"doorClear={stage.CookBakeDoorClearancePass} " +
                                    $"budgetReady={stage.CookBudgetReady} " +
                                    $"budget=({stage.CookBudgetEvidence}) " +
                                    $"marker={stage.CookPhaseQAMarker}/{expectedCookMarker}");
                                Application.Quit(106);
                                yield break;
                            }
                            Debug.Log("[MoonlightGameplayQA][PASS] cook-choreography-live " +
                                $"step={step + 1}/{MoonlightActivityStage.CookPhaseCount} " +
                                $"phase={stage.CookCurrentPhaseName} " +
                                $"mask=0x{stage.CookChoreographyReadyMask:X} " +
                                $"motionProps={stage.CookCurrentPhaseMotionPropCount} " +
                                $"visibleMotionProps={stage.CookCurrentPhaseVisibleMotionPropCount} " +
                                $"motionReady={stage.CookCurrentPhaseMotionReady} " +
                                $"motionEvidence=({stage.CookCurrentPhaseMotionEvidence}) " +
                                $"stateReady={stage.CookCurrentPhaseStateReady} " +
                                $"doorClear={stage.CookBakeDoorClearancePass} " +
                                $"budget=({stage.CookBudgetEvidence}) " +
                                $"marker={stage.CookPhaseQAMarker}");
                        }
                        else if (zone.Kind == MoonlightSpatialActionKind.Play)
                        {
                            bool gesturePlayRuntimePass =
                                zone.LastGestureSample.PointCount == 7 &&
                                zone.LastGestureSample.HasSevenFiniteNormalizedPoints &&
                                feedback.ActiveGestureSample.PointCount == 7 &&
                                feedback.ActiveGestureSample.HasSevenFiniteNormalizedPoints &&
                                stage.ActiveGestureSample.PointCount == 7 &&
                                stage.ActiveGestureSample.HasSevenFiniteNormalizedPoints &&
                                stage.PlayTrajectoryRuntimeReady &&
                                stage.AuthoritativePlayBallCount == 1 &&
                                stage.AuthoritativePlayTrailCount == 1 &&
                                feedback.PlayUsesStageBallOnly && !feedback.HasOpaqueActionOrb &&
                                zone.LastGestureSample.ContentEquals(
                                    feedback.ActiveGestureSample) &&
                                feedback.ActiveGestureSample.ContentEquals(
                                    stage.ActiveGestureSample) &&
                                (step != 3 || stage.PlayCatchIsHeld);
                            if (!gesturePlayRuntimePass)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] gesture-play-runtime " +
                                    $"step={step + 1}/4 zonePoints={zone.LastGestureSample.PointCount} " +
                                    $"feedbackPoints={feedback.ActiveGestureSample.PointCount} " +
                                    $"stagePoints={stage.ActiveGestureSample.PointCount} " +
                                    $"trajectory={stage.PlayTrajectoryQAMarker} " +
                                    $"balls={stage.AuthoritativePlayBallCount} " +
                                    $"trails={stage.AuthoritativePlayTrailCount} " +
                                    $"feedbackOrb={feedback.HasOpaqueActionOrb} " +
                                    $"stageOnly={feedback.PlayUsesStageBallOnly} " +
                                    $"catchHeld={stage.PlayCatchIsHeld} " +
                                    $"position={stage.PlayBallLocalPosition:F3}");
                                Application.Quit(109);
                                yield break;
                            }
                            Debug.Log("[MoonlightGameplayQA][PASS] gesture-play-runtime " +
                                $"step={step + 1}/4 points={stage.ActiveGestureSample.PointCount} " +
                                $"score={stage.ActiveGestureSample.Score:0.00} " +
                                $"duration={stage.ActiveGestureSample.Duration:0.00} " +
                                $"displacement={stage.ActiveGestureSample.Displacement:F3}/" +
                                $"{stage.ActiveGestureSample.DisplacementMagnitude:0.000} " +
                                $"balls={stage.AuthoritativePlayBallCount} " +
                                $"trails={stage.AuthoritativePlayTrailCount} " +
                                $"catchHeld={stage.PlayCatchIsHeld} " +
                                "marker=MOONLIGHT_GESTURE_PLAY_RUNTIME_VERIFIED");

                            bool phaseLandmarkPass =
                                stage.ValidatePlayPhaseLandmarkRuntimeContract(
                                    out string phaseLandmarkDetail);
                            if (playContinuity != null)
                            {
                                playContinuity.PhaseLandmarkObservedMask |= 1 << step;
                                playContinuity.PhaseLandmarkVisibilityMasks[step] =
                                    stage.PlayPhaseLandmarkVisibleMaskForQA;
                                playContinuity.PhaseLandmarkMaterialCounts[step] =
                                    stage.ActiveUniqueMaterialCount;
                                playContinuity.PhaseLandmarkLightCounts[step] =
                                    stage.ActiveLightCount;
                                playContinuity.PhaseLandmarkContractPassed &=
                                    phaseLandmarkPass;
                            }
                            if (!phaseLandmarkPass)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] " +
                                    $"play-phase-landmark-runtime {phaseLandmarkDetail}");
                                Application.Quit(165);
                                yield break;
                            }
                            Debug.Log("[MoonlightGameplayQA][PASS] " +
                                $"play-phase-landmark-runtime {phaseLandmarkDetail} " +
                                $"marker={MoonlightActivityStage.PlayPhaseLandmarkRuntimeQAMarker}");

                            bool arenaSourcePass =
                                stage.ValidatePlayArenaSourceRuntimeContract(
                                    out string arenaSourceDetail);
                            if (playContinuity != null)
                            {
                                if (string.IsNullOrEmpty(playContinuity.ArenaSource))
                                    playContinuity.ArenaSource =
                                        stage.PlayArenaVisualSourceForQA;
                                else
                                    playContinuity.ArenaSourceContractPassed &=
                                        playContinuity.ArenaSource ==
                                            stage.PlayArenaVisualSourceForQA;
                                playContinuity.ArenaSourceContractPassed &= arenaSourcePass;
                            }
                            if (!arenaSourcePass)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] " +
                                    $"play-arena-source {arenaSourceDetail}");
                                Application.Quit(33);
                                yield break;
                            }
                            Debug.Log("[MoonlightGameplayQA][PASS] " +
                                $"play-arena-source {arenaSourceDetail} " +
                                $"marker={stage.PlayArenaSourceQAMarkerForQA}");
                        }
                        else if (zone.Kind == MoonlightSpatialActionKind.Garden)
                        {
                            bool gardenSampleEqualityPass = pad != null &&
                                pad.LastSample.HasSevenFiniteNormalizedPoints &&
                                zone.LastGestureSample.HasSevenFiniteNormalizedPoints &&
                                feedback.ActiveGestureSample.HasSevenFiniteNormalizedPoints &&
                                stage.ActiveGestureSample.HasSevenFiniteNormalizedPoints &&
                                pad.LastSample.ContentEquals(zone.LastGestureSample) &&
                                zone.LastGestureSample.ContentEquals(
                                    feedback.ActiveGestureSample) &&
                                feedback.ActiveGestureSample.ContentEquals(
                                    stage.ActiveGestureSample);
                            bool gardenGestureKindPass = step switch
                            {
                                0 => expected == MoonlightGestureKind.Tap,
                                1 => expected == MoonlightGestureKind.Circle,
                                2 => expected == MoonlightGestureKind.ZigZag,
                                _ => expected == MoonlightGestureKind.Hold
                            };
                            bool gardenPropPass = step == 3 ||
                                stage.GardenGesturePropTransformAgreement;
                            float gardenPropError = step == 3
                                ? 0f
                                : Vector3.Distance(stage.GardenGesturePropLocalPosition,
                                    stage.GardenExpectedGesturePropLocalPosition);
                            bool plantPass = step != 0 ||
                                (stage.GardenSelectedPlantSlot == 2 &&
                                 stage.GardenSelectedPlantSlotInsidePlanter &&
                                 Vector3.Distance(stage.GardenSelectedPlantSlotLocalPosition,
                                     new Vector3(-0.10f, 0.43f, -0.07f)) <= 0.001f);
                            bool waterPass = step != 1 ||
                                stage.GardenWaterDirectionAgreement;
                            bool tendPass = step != 2 ||
                                (stage.GardenTendTargetCount == 5 &&
                                 stage.GardenTendDirectionInversionCount >= 3 &&
                                 stage.GardenTendSequenceTransformAgreement &&
                                 stage.GardenTendAnchorPathAgreement &&
                                 stage.GardenTendCurrentTargetGrowthAgreement);
                            bool bloomPass = step != 3 ||
                                (stage.GardenBloomTransformAgreement &&
                                 stage.GardenBloomOpeningScale > 0f &&
                                 stage.GardenBloomIntensityMultiplier >= 1f);
                            bool unchangedGardenBudgets =
                                MoonlightActivityStage.GardenRendererBudget == 48 &&
                                MoonlightActivityStage.GardenMaterialBudget == 28 &&
                                MoonlightActivityStage.GardenLightBudget == 1 &&
                                stage.GardenMagicFlowerRendererBudget == 10 &&
                                stage.GardenBudgetReady;
                            bool gestureGardenRuntimePass = gardenSampleEqualityPass &&
                                gardenGestureKindPass && gardenPropPass && plantPass &&
                                waterPass && tendPass && bloomPass && unchangedGardenBudgets;
                            if (!gestureGardenRuntimePass)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] gesture-garden-runtime " +
                                    $"step={step + 1}/4 sampleEquality={gardenSampleEqualityPass} " +
                                    $"padPoints={(pad != null ? pad.LastSample.PointCount : 0)} " +
                                    $"zonePoints={zone.LastGestureSample.PointCount} " +
                                    $"feedbackPoints={feedback.ActiveGestureSample.PointCount} " +
                                    $"stagePoints={stage.ActiveGestureSample.PointCount} " +
                                    $"kind={expected}/{gardenGestureKindPass} " +
                                    $"prop={stage.GardenGesturePropLocalPosition:F3}/" +
                                    $"{stage.GardenExpectedGesturePropLocalPosition:F3} " +
                                    $"error={gardenPropError:0.0000}m " +
                                    $"slot={stage.GardenSelectedPlantSlot}/5 " +
                                    $"inside={stage.GardenSelectedPlantSlotInsidePlanter} " +
                                    $"circleAreaRaw={stage.GardenWaterSourceSignedArea:0.0000} " +
                                    $"world={stage.GardenWaterSignedArea:0.0000} " +
                                    $"direction={stage.GardenWaterDirectionAgreement} " +
                                    $"tend={stage.GardenTendTargetCount}/5 " +
                                    $"inversions={stage.GardenTendDirectionInversionCount}/3 " +
                                    $"sequence={stage.GardenTendSequenceTransformAgreement} " +
                                    $"anchors={stage.GardenTendAnchorPathAgreement} " +
                                    $"growth={stage.GardenTendCurrentTargetGrowthAgreement} " +
                                    $"bloom={stage.GardenBloomOpeningScale:0.000}/" +
                                    $"{stage.GardenExpectedBloomOpeningScale:0.000} " +
                                    $"intensity={stage.GardenBloomLightIntensity:0.000}/" +
                                    $"{stage.GardenExpectedBloomLightIntensity:0.000} " +
                                    $"budget=({stage.GardenBudgetEvidence})");
                                Application.Quit(150);
                                yield break;
                            }
                            Debug.Log("[MoonlightGameplayQA][PASS] gesture-garden-runtime " +
                                $"step={step + 1}/4 points={stage.ActiveGestureSample.PointCount} " +
                                $"sampleEquality={gardenSampleEqualityPass} gesture={expected} " +
                                $"propError={gardenPropError:0.0000}m " +
                                $"slot={stage.GardenSelectedPlantSlot} " +
                                $"circleAreaRaw={stage.GardenWaterSourceSignedArea:0.0000} " +
                                $"world={stage.GardenWaterSignedArea:0.0000} " +
                                $"direction={stage.GardenWaterDirectionAgreement} " +
                                $"tend={stage.GardenTendTargetCount}/5 " +
                                $"inversions={stage.GardenTendDirectionInversionCount} " +
                                $"anchors={stage.GardenTendAnchorPathAgreement} " +
                                $"growth={stage.GardenTendCurrentTargetGrowthAgreement} " +
                                $"bloom={stage.GardenBloomOpeningScale:0.000} " +
                                $"intensity={stage.GardenBloomIntensityMultiplier:0.000} " +
                                $"budget=({stage.GardenBudgetEvidence}) " +
                                "marker=MOONLIGHT_GESTURE_GARDEN_RUNTIME_VERIFIED");

                            bool authoredGardenPass = stage.HasAuthoredGardenAtelier &&
                                stage.AuthoredGardenAtelierRendererCount >= 18 &&
                                stage.AuthoredGardenAtelierRendererCount <= 26 &&
                                stage.AuthoredGardenAtelierMaterialCount >= 6 &&
                                stage.AuthoredGardenAtelierMaterialCount <= 8 &&
                                stage.AuthoredGardenAtelierColliderCount == 0 &&
                                stage.AuthoredGardenAtelierLightCount == 0 &&
                                stage.AuthoredGardenAtelierBoundsSize.x >= 1.40f &&
                                stage.AuthoredGardenAtelierBoundsSize.y >= 0.45f &&
                                stage.AuthoredGardenAtelierBoundsSize.z >= 0.75f;
                            if (!authoredGardenPass)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] authored-garden-atelier " +
                                    $"present={stage.HasAuthoredGardenAtelier} " +
                                    $"renderers={stage.AuthoredGardenAtelierRendererCount}/18-26 " +
                                    $"materials={stage.AuthoredGardenAtelierMaterialCount}/6-8 " +
                                    $"colliders={stage.AuthoredGardenAtelierColliderCount} " +
                                    $"lights={stage.AuthoredGardenAtelierLightCount} " +
                                    $"bounds={stage.AuthoredGardenAtelierBoundsSize:F2}");
                                Application.Quit(35);
                                yield break;
                            }
                            Debug.Log("[MoonlightGameplayQA][PASS] authored-garden-atelier " +
                                $"renderers={stage.AuthoredGardenAtelierRendererCount} " +
                                $"materials={stage.AuthoredGardenAtelierMaterialCount} " +
                                $"colliders={stage.AuthoredGardenAtelierColliderCount} " +
                                $"lights={stage.AuthoredGardenAtelierLightCount} " +
                                $"bounds={stage.AuthoredGardenAtelierBoundsSize:F2} " +
                                "marker=MOONLIGHT_AUTHORED_GARDEN_ATELIER_READY");

                            bool magicFlowerPass = stage.HasGardenMagicFlowerPrefab &&
                                stage.GardenMagicFlowerInstanceCount == 5 &&
                                stage.GardenMagicFlowerRendererCount <= stage.GardenMagicFlowerRendererBudget &&
                                stage.GardenMagicFlowerUniqueMaterialCount > 0 &&
                                stage.GardenMagicFlowerUsesSharedMaterials &&
                                stage.GardenMagicFlowerEnabledColliderCount == 0 &&
                                stage.GardenMagicFlowerEnabledLightCount == 0 &&
                                stage.GardenMagicFlowerQAMarker == "MOONLIGHT_MAGIC_FLOWER_STAGE_READY";
                            if (!magicFlowerPass)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] garden-magic-flower-stage " +
                                    $"present={stage.HasGardenMagicFlowerPrefab} " +
                                    $"instances={stage.GardenMagicFlowerInstanceCount}/5 " +
                                    $"renderers={stage.GardenMagicFlowerRendererCount}/{stage.GardenMagicFlowerRendererBudget} " +
                                    $"materials={stage.GardenMagicFlowerUniqueMaterialCount} " +
                                    $"shared={stage.GardenMagicFlowerUsesSharedMaterials} " +
                                    $"colliders={stage.GardenMagicFlowerEnabledColliderCount}/{stage.GardenMagicFlowerColliderCount} " +
                                    $"lights={stage.GardenMagicFlowerEnabledLightCount}/{stage.GardenMagicFlowerLightCount} " +
                                    $"marker={stage.GardenMagicFlowerQAMarker}");
                                Application.Quit(52);
                                yield break;
                            }
                            Debug.Log("[MoonlightGameplayQA][PASS] garden-magic-flower-stage " +
                                $"instances={stage.GardenMagicFlowerInstanceCount} " +
                                $"renderers={stage.GardenMagicFlowerRendererCount}/{stage.GardenMagicFlowerRendererBudget} " +
                                $"materials={stage.GardenMagicFlowerUniqueMaterialCount} " +
                                $"shared={stage.GardenMagicFlowerUsesSharedMaterials} " +
                                $"colliders={stage.GardenMagicFlowerEnabledColliderCount} " +
                                $"lights={stage.GardenMagicFlowerEnabledLightCount} " +
                                "marker=MOONLIGHT_MAGIC_FLOWER_STAGE_VERIFIED");
                        }
                        else if (zone.Kind == MoonlightSpatialActionKind.Read)
                        {
                            bool readSampleEqualityPass = pad != null &&
                                pad.LastSample.HasSevenFiniteNormalizedPoints &&
                                zone.LastGestureSample.HasSevenFiniteNormalizedPoints &&
                                feedback.ActiveGestureSample.HasSevenFiniteNormalizedPoints &&
                                stage.ReadGestureSampleReady &&
                                pad.LastSample.ContentEquals(zone.LastGestureSample) &&
                                zone.LastGestureSample.ContentEquals(
                                    feedback.ActiveGestureSample) &&
                                feedback.ActiveGestureSample.ContentEquals(
                                    stage.ActiveGestureSample);
                            bool readGestureKindPass = step switch
                            {
                                0 => expected == MoonlightGestureKind.Tap,
                                1 => expected == MoonlightGestureKind.Swipe,
                                2 => expected == MoonlightGestureKind.Circle,
                                _ => expected == MoonlightGestureKind.Hold
                            };
                            bool readFinishPass = step != 3 ||
                                (stage.ReadFinishIntensityMultiplier >= 1f &&
                                 stage.ReadActualLightIntensity >=
                                     MoonlightActivityStage.ReadMinimumLightIntensity &&
                                 Mathf.Abs(stage.ReadActualLightIntensity -
                                     stage.ReadExpectedLightIntensity) <= 0.001f &&
                                 stage.ReadActualFinishMoteCount ==
                                     stage.ReadExpectedFinishMoteCount);
                            bool unchangedReadBudgets =
                                MoonlightActivityStage.ReadRendererBudget == 48 &&
                                MoonlightActivityStage.ReadMaterialBudget == 28 &&
                                MoonlightActivityStage.ReadLightBudget == 1 &&
                                MoonlightActivityStage.RequiredReadStageRendererCount == 19 &&
                                MoonlightActivityStage.RequiredReadStageMaterialCount == 7 &&
                                stage.ReadBudgetReady;
                            bool gestureReadRuntimePass = readSampleEqualityPass &&
                                readGestureKindPass && stage.ReadRuntimeContractReady &&
                                readFinishPass && unchangedReadBudgets;
                            if (!gestureReadRuntimePass)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] " +
                                    "gesture-read-runtime " +
                                    $"step={step + 1}/4 sampleEquality={readSampleEqualityPass} " +
                                    $"padPoints={(pad != null ? pad.LastSample.PointCount : 0)} " +
                                    $"zonePoints={zone.LastGestureSample.PointCount} " +
                                    $"feedbackPoints={feedback.ActiveGestureSample.PointCount} " +
                                    $"stagePoints={stage.ActiveGestureSample.PointCount} " +
                                    $"kind={expected}/{readGestureKindPass} " +
                                    $"transform={stage.ReadCurrentStepTransformAgreement} " +
                                    $"evidence=({stage.ReadTransformEvidence}) " +
                                    $"finish={readFinishPass} multiplier=" +
                                    $"{stage.ReadFinishIntensityMultiplier:0.000} " +
                                    $"budget=({stage.ReadBudgetEvidence}) " +
                                    $"marker={GestureReadRuntimeContractFailureMarker}");
                                Application.Quit(153);
                                yield break;
                            }
                            Debug.Log("[MoonlightGameplayQA][PASS] gesture-read-runtime " +
                                $"step={step + 1}/4 points={stage.ActiveGestureSample.PointCount} " +
                                $"sampleEquality={readSampleEqualityPass} gesture={expected} " +
                                $"transform=({stage.ReadTransformEvidence}) " +
                                $"multiplier={stage.ReadFinishIntensityMultiplier:0.000} " +
                                $"budget=({stage.ReadBudgetEvidence}) " +
                                $"marker={GestureReadRuntimeContractMarker}");

                            bool authoredReadPass = stage.HasAuthoredReadingNook &&
                                stage.AuthoredReadingNookRendererCount >= 18 &&
                                stage.AuthoredReadingNookRendererCount <= 24 &&
                                stage.AuthoredReadingNookMaterialCount >= 6 &&
                                stage.AuthoredReadingNookMaterialCount <= 8 &&
                                stage.AuthoredReadingNookColliderCount == 0 &&
                                stage.AuthoredReadingNookLightCount == 0 &&
                                stage.AuthoredReadingNookBoundsSize.x >= 1.40f &&
                                stage.AuthoredReadingNookBoundsSize.y >= 0.45f &&
                                stage.AuthoredReadingNookBoundsSize.z >= 0.70f;
                            if (!authoredReadPass)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] authored-reading-nook " +
                                    $"present={stage.HasAuthoredReadingNook} " +
                                    $"renderers={stage.AuthoredReadingNookRendererCount}/18-24 " +
                                    $"materials={stage.AuthoredReadingNookMaterialCount}/6-8 " +
                                    $"colliders={stage.AuthoredReadingNookColliderCount} " +
                                    $"lights={stage.AuthoredReadingNookLightCount} " +
                                    $"bounds={stage.AuthoredReadingNookBoundsSize:F2}");
                                Application.Quit(36);
                                yield break;
                            }
                            Debug.Log("[MoonlightGameplayQA][PASS] authored-reading-nook " +
                                $"renderers={stage.AuthoredReadingNookRendererCount} " +
                                $"materials={stage.AuthoredReadingNookMaterialCount} " +
                                $"colliders={stage.AuthoredReadingNookColliderCount} " +
                                $"lights={stage.AuthoredReadingNookLightCount} " +
                                $"bounds={stage.AuthoredReadingNookBoundsSize:F2} " +
                                "marker=MOONLIGHT_AUTHORED_READING_NOOK_READY");
                        }
                        else if (zone.Kind == MoonlightSpatialActionKind.Care)
                        {
                            bool careSampleEqualityPass = pad != null &&
                                pad.LastSample.HasSevenFiniteNormalizedPoints &&
                                zone.LastGestureSample.HasSevenFiniteNormalizedPoints &&
                                feedback.ActiveGestureSample.HasSevenFiniteNormalizedPoints &&
                                stage.CareGestureSampleReady &&
                                pad.LastSample.ContentEquals(zone.LastGestureSample) &&
                                zone.LastGestureSample.ContentEquals(
                                    feedback.ActiveGestureSample) &&
                                feedback.ActiveGestureSample.ContentEquals(
                                    stage.ActiveGestureSample);
                            bool careGestureKindPass = step switch
                            {
                                0 => expected == MoonlightGestureKind.Tap,
                                1 => expected == MoonlightGestureKind.Circle,
                                2 => expected == MoonlightGestureKind.Swipe,
                                _ => expected == MoonlightGestureKind.Hold
                            };
                            bool careResponsePass = step switch
                            {
                                0 => stage.CareActualGesturePropLocalPosition.x >= -0.78f &&
                                     stage.CareActualGesturePropLocalPosition.x <= -0.44f &&
                                     stage.CareActualGesturePropLocalPosition.z >= -0.10f &&
                                     stage.CareActualGesturePropLocalPosition.z <= 0.12f,
                                1 => Mathf.Abs(stage.CareWashSignedDirection) == 1f &&
                                     stage.CareWashOrbitRadius >= 0.16f &&
                                     stage.CareWashOrbitRadius <= 0.28f,
                                2 => stage.CareActualGesturePropLocalPosition.x >= -0.30f &&
                                     stage.CareActualGesturePropLocalPosition.x <= 0.46f,
                                _ => stage.CareGlowAuraScaleMultiplier >= 0.80f &&
                                     stage.CareActualGlowLightIntensity >= 0.32f &&
                                     stage.CareActualGlowMoteCount ==
                                         stage.CareExpectedGlowMoteCount &&
                                     stage.CareActualGlowMoteCount is >= 2 and <= 6
                            };
                            bool gestureCareRuntimePass = careSampleEqualityPass &&
                                careGestureKindPass && careResponsePass &&
                                stage.CareRuntimeContractReady;
                            if (!gestureCareRuntimePass)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] " +
                                    "gesture-care-runtime " +
                                    $"step={step + 1}/4 sampleEquality={careSampleEqualityPass} " +
                                    $"padPoints={(pad != null ? pad.LastSample.PointCount : 0)} " +
                                    $"zonePoints={zone.LastGestureSample.PointCount} " +
                                    $"feedbackPoints={feedback.ActiveGestureSample.PointCount} " +
                                    $"stagePoints={stage.ActiveGestureSample.PointCount} " +
                                    $"kind={expected}/{careGestureKindPass} " +
                                    $"response={careResponsePass} " +
                                    $"transform={stage.CareCurrentStepTransformAgreement} " +
                                    $"evidence=({stage.CareTransformEvidence}) " +
                                    $"marker={GestureCareRuntimeContractFailureMarker}");
                                Application.Quit(157);
                                yield break;
                            }
                            verifiedGestureResponsiveCareSteps++;
                            Debug.Log("[MoonlightGameplayQA][PASS] gesture-care-runtime " +
                                $"step={step + 1}/4 " +
                                $"sampleEquality={careSampleEqualityPass} gesture={expected} " +
                                $"transform=({stage.CareTransformEvidence}) " +
                                $"verified={verifiedGestureResponsiveCareSteps}/4 " +
                                $"marker={GestureCareRuntimeStepMarker}");

                            bool stageUsesFallback = stage.UsesProceduralCareStationFallback;
                            string expectedStageSource = stageUsesFallback
                                ? "persistent-procedural-fallback"
                                : "authored";
                            string expectedStageMarker = stageUsesFallback
                                ? "MOONLIGHT_CARE_VANITY_PROCEDURAL_FALLBACK_READY"
                                : "MOONLIGHT_AUTHORED_CARE_STATION_READY";
                            bool rendererCountPass = stage.CareStationRendererCount > 0 &&
                                stage.CareStationRendererCount <= stage.CareStationRendererBudget &&
                                (!stageUsesFallback || stage.CareStationRendererCount ==
                                    stage.CareStationRendererBudget);
                            bool authoredMetricsPass = stageUsesFallback
                                ? stage.AuthoredCareStationRendererCount == 0 &&
                                  stage.AuthoredCareStationMaterialCount == 0 &&
                                  stage.AuthoredCareStationColliderCount == 0 &&
                                  stage.AuthoredCareStationLightCount == 0 &&
                                  stage.AuthoredCareStationBoundsSize == Vector3.zero
                                : stage.AuthoredCareStationRendererCount == stage.CareStationRendererCount &&
                                  stage.AuthoredCareStationMaterialCount == stage.CareStationMaterialCount &&
                                  stage.AuthoredCareStationColliderCount == stage.CareStationColliderCount &&
                                  stage.AuthoredCareStationLightCount == stage.CareStationLightCount &&
                                  stage.AuthoredCareStationBoundsSize == stage.CareStationBoundsSize;
                            string rendererContract = stageUsesFallback
                                ? $"exactly-{stage.CareStationRendererBudget}"
                                : $"1-{stage.CareStationRendererBudget}";
                            bool careStationSourcePass =
                                stageUsesFallback == persistentStation.UsesProceduralFallback &&
                                stage.HasAuthoredCareStation == !stageUsesFallback &&
                                authoredMetricsPass &&
                                stage.CareStationVisualSource == expectedStageSource &&
                                stage.CareStationSourceQAMarker == expectedStageMarker &&
                                rendererCountPass &&
                                stage.CareStationMaterialCount > 0 &&
                                stage.CareStationMaterialCount <= stage.CareStationMaterialBudget &&
                                stage.CareStationColliderCount == 0 &&
                                stage.CareStationLightCount == 0 &&
                                stage.CareStationBoundsSize.x >= 1.20f &&
                                stage.CareStationBoundsSize.y >= 0.60f &&
                                stage.CareStationBoundsSize.z >= 0.45f;
                            if (!careStationSourcePass)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] care-station-source " +
                                    $"source={stage.CareStationVisualSource}/{expectedStageSource} " +
                                    $"fallback={stageUsesFallback}/{persistentStation.UsesProceduralFallback} " +
                                    $"authored={stage.HasAuthoredCareStation}/{!stageUsesFallback} " +
                                    $"authoredMetrics={authoredMetricsPass} " +
                                    $"marker={stage.CareStationSourceQAMarker}/{expectedStageMarker} " +
                                    $"renderers={stage.CareStationRendererCount}/" +
                                    $"{rendererContract} " +
                                    $"materials={stage.CareStationMaterialCount}/1-{stage.CareStationMaterialBudget} " +
                                    $"colliders={stage.CareStationColliderCount} " +
                                    $"lights={stage.CareStationLightCount} " +
                                    $"bounds={stage.CareStationBoundsSize:F2}");
                                Application.Quit(98);
                                yield break;
                            }
                            Debug.Log("[MoonlightGameplayQA][PASS] care-station-source " +
                                $"source={stage.CareStationVisualSource} " +
                                $"fallback={stageUsesFallback} authored={stage.HasAuthoredCareStation} " +
                                $"renderers={stage.CareStationRendererCount}/{stage.CareStationRendererBudget} " +
                                $"materials={stage.CareStationMaterialCount}/{stage.CareStationMaterialBudget} " +
                                $"colliders={stage.CareStationColliderCount} " +
                                $"lights={stage.CareStationLightCount} " +
                                $"bounds={stage.CareStationBoundsSize:F2} " +
                                $"marker={stage.CareStationSourceQAMarker}");
                        }

                        string phase = zone.Kind switch
                        {
                            MoonlightSpatialActionKind.Cook => step switch { 0 => "add", 1 => "stir", 2 => "bake", _ => "decorate" },
                            MoonlightSpatialActionKind.Play => step switch { 0 => "throw", 1 => "chase", 2 => "jump", _ => "catch" },
                            MoonlightSpatialActionKind.Garden => step switch { 0 => "plant", 1 => "water", 2 => "tend", _ => "bloom" },
                            MoonlightSpatialActionKind.Read => step switch { 0 => "open", 1 => "turn", 2 => "trace", _ => "remember" },
                            MoonlightSpatialActionKind.Care => step switch { 0 => "prep", 1 => "wash", 2 => "brush", _ => "glow" },
                            _ => "step"
                        };
                        string prefix = zone.Kind switch
                        {
                            MoonlightSpatialActionKind.Cook => "02",
                            MoonlightSpatialActionKind.Play => "03",
                            MoonlightSpatialActionKind.Garden => "04",
                            MoonlightSpatialActionKind.Read => "05",
                            MoonlightSpatialActionKind.Care => "06",
                            _ => "99"
                        };
                        string stepShot = Path.Combine(output,
                            $"{prefix}_{zone.Kind.ToString().ToLowerInvariant()}_step_{step + 1}_{phase}.png");
                        yield return Capture(stepShot);
                        Debug.Log($"[MoonlightGameplayQA][PASS] activity-visual action={zone.Kind} " +
                            $"step={step + 1}/{zone.RequiredSteps} renderers={stage.ActiveRendererCount} " +
                            $"materials={stage.ActiveUniqueMaterialCount} lights={stage.ActiveLightCount} screenshot={stepShot}");
                    }
                    if (feedback != null && !string.IsNullOrEmpty(feedback.ActionVisualSignature))
                        verifiedVisualSignatures.Add(feedback.ActionVisualSignature);

                    if (step == zone.RequiredSteps - 1)
                    {
                        if (verifyRewards)
                        {
                            bool rewardPassed = zone.Kind switch
                            {
                                MoonlightSpatialActionKind.Cook =>
                                    Approximately(moonlight.stats.wonder - rewardWonder, 5f) &&
                                    Approximately(moonlight.stats.warmth - rewardWarmth, 8f) &&
                                    Approximately(moonlight.stats.magic - rewardMagic, 5f) &&
                                    Approximately(moonlight.stats.hunger - rewardHunger, 20f) &&
                                    Approximately(moonlight.stats.rest - rewardRest, 0f) &&
                                    moonlight.xp - rewardXp == 14 && moonlight.coins - rewardCoins == 3,
                                MoonlightSpatialActionKind.Play =>
                                    Approximately(moonlight.stats.wonder - rewardWonder, 25f) &&
                                    Approximately(moonlight.stats.warmth - rewardWarmth, 0f) &&
                                    Approximately(moonlight.stats.magic - rewardMagic, 13f) &&
                                    Approximately(moonlight.stats.hunger - rewardHunger, 0f) &&
                                    Approximately(moonlight.stats.rest - rewardRest, 0f) &&
                                    moonlight.xp - rewardXp == 32 && moonlight.coins - rewardCoins == 5,
                                MoonlightSpatialActionKind.Garden =>
                                    Approximately(moonlight.stats.wonder - rewardWonder, 16f) &&
                                    Approximately(moonlight.stats.warmth - rewardWarmth, 0f) &&
                                    Approximately(moonlight.stats.magic - rewardMagic, 12f) &&
                                    Approximately(moonlight.stats.hunger - rewardHunger, 0f) &&
                                    Approximately(moonlight.stats.rest - rewardRest, 0f) &&
                                    moonlight.xp - rewardXp == 10 && moonlight.coins - rewardCoins == 6,
                                MoonlightSpatialActionKind.Read =>
                                    Approximately(moonlight.stats.wonder - rewardWonder, 14f) &&
                                    Approximately(moonlight.stats.warmth - rewardWarmth, 10f) &&
                                    Approximately(moonlight.stats.magic - rewardMagic, 0f) &&
                                    Approximately(moonlight.stats.hunger - rewardHunger, 0f) &&
                                    Approximately(moonlight.stats.rest - rewardRest, 6f) &&
                                    moonlight.xp - rewardXp == 12 && moonlight.coins - rewardCoins == 5,
                                MoonlightSpatialActionKind.Care =>
                                    Approximately(moonlight.stats.wonder - rewardWonder, 0f) &&
                                    Approximately(moonlight.stats.warmth - rewardWarmth, 18f) &&
                                    Approximately(moonlight.stats.magic - rewardMagic, 6f) &&
                                    Approximately(moonlight.stats.hunger - rewardHunger, 0f) &&
                                    Approximately(moonlight.stats.rest - rewardRest, 12f) &&
                                    moonlight.xp - rewardXp == 12 && moonlight.coins - rewardCoins == 5,
                                _ => false
                            };
                            if (!rewardPassed)
                            {
                                Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-reward action={zone.Kind} " +
                                    $"wonder={moonlight.stats.wonder - rewardWonder:0.0} " +
                                    $"warmth={moonlight.stats.warmth - rewardWarmth:0.0} " +
                                    $"magic={moonlight.stats.magic - rewardMagic:0.0} " +
                                    $"hunger={moonlight.stats.hunger - rewardHunger:0.0} " +
                                    $"rest={moonlight.stats.rest - rewardRest:0.0} " +
                                    $"xp={moonlight.xp - rewardXp} coins={moonlight.coins - rewardCoins}");
                                Application.Quit(31);
                                yield break;
                            }
                            Debug.Log($"[MoonlightGameplayQA][PASS] activity-reward action={zone.Kind} " +
                                $"wonder={moonlight.stats.wonder - rewardWonder:0.0} " +
                                $"warmth={moonlight.stats.warmth - rewardWarmth:0.0} " +
                                $"magic={moonlight.stats.magic - rewardMagic:0.0} " +
                                $"hunger={moonlight.stats.hunger - rewardHunger:0.0} " +
                                $"rest={moonlight.stats.rest - rewardRest:0.0} " +
                                $"xp={moonlight.xp - rewardXp} coins={moonlight.coins - rewardCoins}");
                        }
                        string fileName = zone.Kind switch
                        {
                            MoonlightSpatialActionKind.Cook => "02_after_cooking_gesture.png",
                            MoonlightSpatialActionKind.Play => "03_after_play_gesture.png",
                            MoonlightSpatialActionKind.Garden => "04_gardening_followthrough.png",
                            MoonlightSpatialActionKind.Read => "05_after_reading_gesture.png",
                            MoonlightSpatialActionKind.Care => "06_after_care_gesture.png",
                            _ => "activity.png"
                        };
                        string shot = Path.Combine(output, fileName);
                        yield return Capture(shot);
                        Debug.Log($"[MoonlightGameplayQA][PASS] activity-complete action={zone.Kind} " +
                            $"gesture={expected} score={zone.LastGestureScore:0.00} screenshot={shot}");
                    }

                    float deadline = Time.time + 4f;
                    while (feedback != null && (feedback.IsPerformingAction || feedback.IsCoolingDown) &&
                           Time.time < deadline)
                        yield return null;
                    yield return new WaitForSeconds(0.12f);
                    string postCooldownQualityDetail = "";
                    if (expectIPadHud &&
                        !ValidateActivityQualityReadoutRuntimeBinding(ui, zone, true,
                            finalMasteryStep, out postCooldownQualityDetail))
                    {
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] " +
                            $"activity-quality-post-cooldown action={zone.Kind} " +
                            $"step={step + 1}/{zone.RequiredSteps} " +
                            $"{postCooldownQualityDetail}");
                        Application.Quit(156);
                        yield break;
                    }
                    if (expectIPadHud)
                        Debug.Log($"[MoonlightGameplayQA][PASS] " +
                            $"activity-quality-post-cooldown action={zone.Kind} " +
                            $"step={step + 1}/{zone.RequiredSteps} " +
                            $"{postCooldownQualityDetail} " +
                            $"marker={ActivityQualityReadoutRuntimeContractMarker}");
                    if (expectIPadHud)
                    {
                        float postCooldownDrift = Vector3.Distance(
                            controller.transform.position, acceptedActionPosition);
                        bool stableAfterCooldown = touchJoystick.IsInputNeutral &&
                            !touchJoystick.IsTrackingPointer &&
                            touchJoystick.Value.sqrMagnitude <= 0.0001f &&
                            touchJoystick.KnobAnchoredPosition.sqrMagnitude <= 0.0001f &&
                            controller.TouchMove.sqrMagnitude <= 0.0001f &&
                            !controller.IsIPadSprinting &&
                            postCooldownDrift <= 0.001f &&
                            spatialInteractor.CurrentZone == acceptedActionZone;
                        if (!stableAfterCooldown)
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-movement-post-cooldown " +
                                $"action={zone.Kind} step={step + 1} drift={postCooldownDrift:0.0000} " +
                                $"zone={spatialInteractor.CurrentZone?.DisplayName}/" +
                                $"{acceptedActionZone?.DisplayName} tracking={touchJoystick.IsTrackingPointer} " +
                                $"value={touchJoystick.Value:F3} knob={touchJoystick.KnobAnchoredPosition:F2} " +
                                $"controller={controller.TouchMove:F3}");
                            Application.Quit(82);
                            yield break;
                        }
                        Debug.Log($"[MoonlightGameplayQA][PASS] activity-movement-post-cooldown " +
                            $"action={zone.Kind} step={step + 1} drift={postCooldownDrift:0.0000} " +
                            $"zone={spatialInteractor.CurrentZone.DisplayName} " +
                            "marker=MOONLIGHT_IPAD_ACTIVITY_MOVEMENT_NEUTRALIZED");
                    }
                    var releasedCamera = Camera.main != null
                        ? Camera.main.GetComponent<CameraController>()
                        : null;
                    bool finalPresentationStep = step == zone.RequiredSteps - 1;
                    bool cameraReleasePass = releasedCamera != null &&
                        (finalPresentationStep
                            ? releasedCamera.ActivityFocusRequested && releasedCamera.ActivityFocusKind == zone.Kind
                            : !releasedCamera.ActivityFocusRequested);
                    if (!cameraReleasePass)
                    {
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-camera-state action={zone.Kind} " +
                            $"step={step + 1} camera={(releasedCamera != null)} " +
                            $"requested={(releasedCamera != null && releasedCamera.ActivityFocusRequested)} " +
                            $"expected={(finalPresentationStep ? "held-for-presentation" : "released")}");
                        Application.Quit(41);
                        yield break;
                    }
                    Debug.Log($"[MoonlightGameplayQA][PASS] activity-camera-state action={zone.Kind} " +
                        $"step={step + 1} state={(finalPresentationStep ? "held-for-presentation" : "released")} " +
                        $"blend={releasedCamera.ActivityFocusBlend:0.00}");
                    if (cookContinuity != null && !finalPresentationStep)
                    {
                        stage = moonlight.GetComponent<MoonlightActivityStage>();
                        bool terminalHoldPass = stage != null &&
                            stage.IsHoldingCookStepTerminal &&
                            stage.CookIntermediateResultVisibleForQA &&
                            !stage.IsLingering && feedback != null &&
                            feedback.CanBeginAction &&
                            !feedback.IsCameraFocusRequestedForQA &&
                            feedback.VisualPoseRestoredForQA &&
                            stage.CookStageRootInstanceId == cookContinuity.RootId &&
                            stage.CookStageMaterialIdentityCountForQA ==
                                cookContinuity.MaterialIdentityCount &&
                            stage.CookStageMaterialIdentityHashForQA ==
                                cookContinuity.MaterialIdentityHash &&
                            stage.CookStageLightInstanceId == cookContinuity.LightId &&
                            stage.CookBudgetReady && CountActiveCookStageRoots() == 1;
                        if (!terminalHoldPass)
                        {
                            Debug.LogError("[MoonlightGameplayQA][FAIL] " +
                                $"cook-terminal-hold step={step + 1}/3 " +
                                $"held={(stage != null && stage.IsHoldingCookStepTerminal)} " +
                                $"visible={(stage != null && stage.CookIntermediateResultVisibleForQA)} " +
                                $"lingering={(stage != null && stage.IsLingering)} " +
                                $"feedback={(feedback != null)} " +
                                $"canBegin={(feedback != null && feedback.CanBeginAction)} " +
                                $"cameraRequested={(feedback != null && feedback.IsCameraFocusRequestedForQA)} " +
                                $"cameraBlendActive={(feedback != null && feedback.IsCameraFocusActive)} " +
                                $"poseRestored={(feedback != null && feedback.VisualPoseRestoredForQA)} " +
                                $"root={(stage != null ? stage.CookStageRootInstanceId : 0)}/" +
                                $"{cookContinuity.RootId} materialCount=" +
                                $"{(stage != null ? stage.CookStageMaterialIdentityCountForQA : 0)}/" +
                                $"{cookContinuity.MaterialIdentityCount} materialHash=" +
                                $"{(stage != null ? stage.CookStageMaterialIdentityHashForQA : 0)}/" +
                                $"{cookContinuity.MaterialIdentityHash} light=" +
                                $"{(stage != null ? stage.CookStageLightInstanceId : 0)}/" +
                                $"{cookContinuity.LightId} budget=" +
                                $"{(stage != null && stage.CookBudgetReady)} activeRoots=" +
                                $"{CountActiveCookStageRoots()}/1");
                            Application.Quit(162);
                            yield break;
                        }
                        cookContinuity.TerminalHoldCount++;
                        Debug.Log("[MoonlightGameplayQA][PASS] cook-terminal-hold " +
                            $"step={step + 1}/3 root={stage.CookStageRootInstanceId} " +
                            $"materials={stage.CookStageMaterialIdentityCountForQA}/" +
                            $"{stage.CookStageMaterialIdentityHashForQA} " +
                            $"light={stage.CookStageLightInstanceId} " +
                            $"budget=({stage.CookBudgetEvidence}) " +
                            "marker=MOONLIGHT_COOK_INTERMEDIATE_TERMINAL_VISIBLE");
                    }
                    if (playContinuity != null && !finalPresentationStep)
                    {
                        stage = moonlight.GetComponent<MoonlightActivityStage>();
                        bool heldReady = stage != null && stage.IsHoldingPlayStepTerminal &&
                            !stage.IsLingering && feedback != null && feedback.CanBeginAction &&
                            !feedback.IsCameraFocusRequestedForQA &&
                            feedback.VisualPoseRestoredForQA &&
                            playContinuity.HasHeldEndpoint &&
                            PlayMaterialsMatchObservation(stage, playContinuity);
                        var wrongBefore = heldReady
                            ? new PlayHeldSnapshot(zone, stage, moonlight)
                            : default;
                        MoonlightGestureKind wrongGesture = zone.RequiredGesture ==
                            MoonlightGestureKind.Tap
                                ? MoonlightGestureKind.Swipe
                                : MoonlightGestureKind.Tap;
                        string wrongResult = zone.ExecuteGesture(moonlight, wrongGesture,
                            PlayContinuitySample(zone.ProgressStep, 0.95f));
                        bool wrongPreserved = heldReady && !zone.LastGesturePassed &&
                            PlayHeldStateMatches(wrongBefore, zone, stage, moonlight);

                        var lowBefore = heldReady
                            ? new PlayHeldSnapshot(zone, stage, moonlight)
                            : default;
                        string lowResult = zone.ExecuteGesture(moonlight,
                            zone.RequiredGesture,
                            PlayContinuitySample(zone.ProgressStep, 0.20f));
                        bool lowPreserved = heldReady && !zone.LastGesturePassed &&
                            PlayHeldStateMatches(lowBefore, zone, stage, moonlight);
                        if (!wrongPreserved || !lowPreserved)
                        {
                            Debug.LogError("[MoonlightGameplayQA][FAIL] " +
                                $"play-held-rejection step={step + 1}/3 " +
                                $"ready={heldReady} wrong={wrongPreserved} low={lowPreserved} " +
                                $"wrongResult=\"{wrongResult}\" lowResult=\"{lowResult}\" " +
                                $"root={(stage != null ? stage.PlayStageRootInstanceId : 0)}/" +
                                $"{playContinuity.RootId} ball=" +
                                $"{(stage != null ? stage.PlayBallInstanceId : 0)}/" +
                                $"{playContinuity.BallId} position=" +
                                $"{(stage != null ? stage.PlayBallLocalPosition : Vector3.zero):F4}");
                            Application.Quit(161);
                            yield break;
                        }
                        playContinuity.RejectedPreservationCount += 2;
                        Debug.Log("[MoonlightGameplayQA][PASS] play-held-rejection " +
                            $"step={step + 1}/3 wrong=True low=True root=" +
                            $"{stage.PlayStageRootInstanceId} ball={stage.PlayBallInstanceId} " +
                            $"trail={stage.PlayTrailInstanceId} position=" +
                            $"{stage.PlayBallLocalPosition:F4} materials=" +
                            $"{stage.ActiveUniqueMaterialCount}/" +
                            $"{stage.PlayBallSharedMaterialInstanceId}/" +
                            $"{stage.PlayTrailSharedMaterialInstanceId} " +
                            "marker=MOONLIGHT_PLAY_HELD_REJECTION_PRESERVED");
                    }
                    if (step == zone.RequiredSteps - 1)
                    {
                        stage = moonlight.GetComponent<MoonlightActivityStage>();
                        var masteryFeedback = moonlight.GetComponent<MoonlightActionFeedback>();
                        bool celebrationPass = masteryFeedback != null &&
                            masteryFeedback.MasteryCelebrationQAMarker == "MOONLIGHT_MASTERY_CELEBRATION_PLAYED" &&
                            masteryFeedback.LastMasteryCelebrationTier == 3 &&
                            masteryFeedback.LastMasteryCelebrationParticles == 54 &&
                            masteryFeedback.LastMasteryCelebrationCombo == zone.RequiredSteps;
                        if (!celebrationPass)
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] mastery-celebration action={zone.Kind} " +
                                $"marker={(masteryFeedback != null ? masteryFeedback.MasteryCelebrationQAMarker : "missing")} " +
                                $"tier={(masteryFeedback != null ? masteryFeedback.LastMasteryCelebrationTier : -1)} " +
                                $"particles={(masteryFeedback != null ? masteryFeedback.LastMasteryCelebrationParticles : 0)} " +
                                $"combo={(masteryFeedback != null ? masteryFeedback.LastMasteryCelebrationCombo : 0)}");
                            Application.Quit(62);
                            yield break;
                        }
                        Debug.Log($"[MoonlightGameplayQA][PASS] mastery-celebration action={zone.Kind} " +
                            $"tier={masteryFeedback.LastMasteryCelebrationTier} " +
                            $"particles={masteryFeedback.LastMasteryCelebrationParticles} " +
                            $"combo={masteryFeedback.LastMasteryCelebrationCombo} " +
                            "marker=MOONLIGHT_MASTERY_CELEBRATION_VERIFIED");
                        if (stage == null || !stage.IsVisible || !stage.IsLingering ||
                            stage.CurrentStep != step || stage.LingerSecondsRemaining < 1.5f)
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-final-presentation action={zone.Kind} " +
                                $"visible={(stage != null && stage.IsVisible)} " +
                                $"lingering={(stage != null && stage.IsLingering)} " +
                                $"step={(stage != null ? stage.CurrentStep + 1 : -1)}/{zone.RequiredSteps} " +
                                $"remaining={(stage != null ? stage.LingerSecondsRemaining : 0f):0.00}s");
                            Application.Quit(47);
                            yield break;
                        }
                        if (zone.Kind == MoonlightSpatialActionKind.Cook)
                        {
                            bool revealTrayPass =
                                stage.CookMooncakeRevealTrayExistsForQA &&
                                stage.CookMooncakeRevealTrayVisibleForQA &&
                                stage.CookMooncakeRevealTrayRendererCountForQA == 4 &&
                                stage.CookMooncakeRevealTrayVisibleRendererCountForQA == 4 &&
                                stage.CookMooncakeRevealTrayQAMarker ==
                                    MoonlightActivityStage.CookMooncakeRevealTrayReadyMarker &&
                                stage.CookBudgetReady;
                            if (cookContinuity != null)
                                cookContinuity.RevealTrayCompletionObserved =
                                    revealTrayPass;
                            if (!revealTrayPass)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] " +
                                    "cook-mooncake-reveal-tray-runtime " +
                                    stage.CookMooncakeRevealTrayEvidence);
                                Application.Quit(163);
                                yield break;
                            }
                            Debug.Log("[MoonlightGameplayQA][PASS] " +
                                "cook-mooncake-reveal-tray-runtime " +
                                stage.CookMooncakeRevealTrayEvidence +
                                $" marker={CookMooncakeRevealTrayRuntimeMarker}");
                            bool lingeringCookImprintPass =
                                stage.ActiveGestureSample.ContentEquals(
                                    zone.LastGestureSample) &&
                                stage.CookGesturePathTransformAgreement &&
                                stage.CookGestureInputReady &&
                                stage.CookDistinctGestureImprintCount == 9 &&
                                stage.CookCookieMarksRetainGestureImprint &&
                                stage.CookGestureResultQAMarker ==
                                    MoonlightActivityStage.CookGesturePersonalizedResultMarker &&
                                MoonlightActivityStage.CookRendererBudget == 36 &&
                                MoonlightActivityStage.CookMaterialBudget == 24 &&
                                MoonlightActivityStage.CookLightBudget == 1 &&
                                stage.CookBudgetReady &&
                                stage.CookMooncakeRevealTrayPersistsDuringLingerForQA;
                            if (!lingeringCookImprintPass)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] " +
                                    "gesture-cook-linger " +
                                    $"sample={stage.ActiveGestureSample.ContentEquals(zone.LastGestureSample)} " +
                                    $"transform={stage.CookGesturePathTransformAgreement} " +
                                    $"shape={stage.CookGestureInputReady} distinct=" +
                                    $"{stage.CookDistinctGestureImprintCount}/9 " +
                                    $"marks={stage.CookCookieMarksRetainGestureImprint} " +
                                    $"resultMarker={stage.CookGestureResultQAMarker} " +
                                    $"revealTray=({stage.CookMooncakeRevealTrayEvidence}) " +
                                    $"budget=({stage.CookBudgetEvidence})");
                                Application.Quit(118);
                                yield break;
                            }
                            if (cookContinuity != null)
                            {
                                cookContinuity.FinalLingerObserved = stage.IsLingering &&
                                    stage.CurrentStep == 3 &&
                                    stage.LingerSecondsRemaining > 0f;
                                cookContinuity.RevealTrayLingerObserved =
                                    stage.CookMooncakeRevealTrayPersistsDuringLingerForQA;
                            }
                            Debug.Log("[MoonlightGameplayQA][PASS] gesture-cook-linger " +
                                $"marks={stage.CookCookieMarksRetainGestureImprint} " +
                                $"resultMarker={stage.CookGestureResultQAMarker} " +
                                $"revealTray=({stage.CookMooncakeRevealTrayEvidence}) " +
                                $"budget=({stage.CookBudgetEvidence}) " +
                                "marker=MOONLIGHT_COOK_GESTURE_LINGER_VERIFIED");
                            Debug.Log("[MoonlightGameplayQA][PASS] " +
                                "cook-mooncake-reveal-tray-linger " +
                                stage.CookMooncakeRevealTrayEvidence +
                                $" marker={CookMooncakeRevealTrayLingerMarker}");
                        }
                        if (zone.Kind == MoonlightSpatialActionKind.Garden)
                        {
                            bool lingeringGardenBloomPass =
                                stage.ActiveGestureSample.ContentEquals(
                                    zone.LastGestureSample) &&
                                stage.GardenBloomPersistsDuringLinger &&
                                stage.GardenBloomTransformAgreement &&
                                stage.GardenBloomOpeningScale > 0f &&
                                stage.GardenBloomIntensityMultiplier >= 1f &&
                                MoonlightActivityStage.GardenRendererBudget == 48 &&
                                MoonlightActivityStage.GardenMaterialBudget == 28 &&
                                MoonlightActivityStage.GardenLightBudget == 1 &&
                                stage.GardenBudgetReady;
                            if (!lingeringGardenBloomPass)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] " +
                                    "gesture-garden-linger " +
                                    $"sample={stage.ActiveGestureSample.ContentEquals(zone.LastGestureSample)} " +
                                    $"persist={stage.GardenBloomPersistsDuringLinger} " +
                                    $"transform={stage.GardenBloomTransformAgreement} " +
                                    $"opening={stage.GardenBloomOpeningScale:0.000}/" +
                                    $"{stage.GardenExpectedBloomOpeningScale:0.000} " +
                                    $"light={stage.GardenBloomLightIntensity:0.000}/" +
                                    $"{stage.GardenExpectedBloomLightIntensity:0.000} " +
                                    $"multiplier={stage.GardenBloomIntensityMultiplier:0.000} " +
                                    $"remaining={stage.LingerSecondsRemaining:0.00}s " +
                                    $"budget=({stage.GardenBudgetEvidence})");
                                Application.Quit(151);
                                yield break;
                            }
                            Debug.Log("[MoonlightGameplayQA][PASS] gesture-garden-linger " +
                                $"opening={stage.GardenBloomOpeningScale:0.000} " +
                                $"light={stage.GardenBloomLightIntensity:0.000} " +
                                $"multiplier={stage.GardenBloomIntensityMultiplier:0.000} " +
                                $"remaining={stage.LingerSecondsRemaining:0.00}s " +
                                $"budget=({stage.GardenBudgetEvidence}) " +
                                "marker=MOONLIGHT_GESTURE_GARDEN_LINGER_VERIFIED");
                            string gardenAfterShot = Path.Combine(output,
                                "04_after_gardening_gesture.png");
                            yield return Capture(gardenAfterShot);
                            Debug.Log("[MoonlightGameplayQA][PASS] gesture-garden-after " +
                                $"t=1 linger={stage.IsLingering} " +
                                $"screenshot={gardenAfterShot} " +
                                "marker=MOONLIGHT_GESTURE_GARDEN_AFTER_CAPTURE_VERIFIED");
                        }
                        int finalCountdownSeconds = 0;
                        bool finalActionTextValid = ui != null &&
                            MoonlightUI.TryParseFinalActivityCountdownLabel(
                                ui.ActionButtonQAText, out finalCountdownSeconds);
                        string finalQualityDetail = "";
                        bool finalQualityValid = !expectIPadHud ||
                            ValidateActivityQualityReadoutRuntimeBinding(ui, zone, true, true,
                                out finalQualityDetail);
                        if (expectIPadHud && ui != null &&
                            (ui.ActivityProgressQAMarker != "4/4" ||
                             !Approximately(ui.ActivityProgressFill01, 1f) ||
                             ui.ActivityProgressFillQAMarker !=
                                 "MOONLIGHT_IPAD_ACTIVITY_PROGRESS_FILL_READY" ||
                             ui.GestureCommandQAMarker != "FINAL PRESENTATION" ||
                             !finalActionTextValid ||
                             ui.ActionButtonQAInteractable ||
                             ui.ContextResultLineCount != 2 ||
                             ui.ContextResultIsOverflowing ||
                             !finalQualityValid ||
                             ui.IsRoomNavigationVisible || !ui.IsRoomNavigationLocked))
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-final-hud action={zone.Kind} " +
                                $"progress={ui.ActivityProgressQAMarker} " +
                                $"fill={ui.ActivityProgressFill01:0.000} " +
                                $"fillMarker={ui.ActivityProgressFillQAMarker} " +
                                $"command={ui.GestureCommandQAMarker} " +
                                $"actionText={ui.ActionButtonQAText.Replace('\n', '/')} " +
                                $"countdownSeconds={finalCountdownSeconds} " +
                                $"actionInteractable={ui.ActionButtonQAInteractable} " +
                                $"lines={ui.ContextResultLineCount} " +
                                $"overflow={ui.ContextResultIsOverflowing} " +
                                $"quality=({finalQualityDetail}) roomNav={ui.RoomNavigationQAMarker}");
                            Application.Quit(49);
                            yield break;
                        }
                        if (expectIPadHud && ui != null)
                            Debug.Log($"[MoonlightGameplayQA][PASS] activity-final-hud action={zone.Kind} " +
                                $"progress={ui.ActivityProgressQAMarker} command={ui.GestureCommandQAMarker} " +
                                $"fill={ui.ActivityProgressFill01:0.000} " +
                                $"actionText={ui.ActionButtonQAText.Replace('\n', '/')} " +
                                $"countdownSeconds={finalCountdownSeconds} " +
                                $"actionInteractable={ui.ActionButtonQAInteractable} " +
                                $"quality=({finalQualityDetail}) " +
                                "marker=MOONLIGHT_ACTIVITY_FINAL_HUD_VERIFIED " +
                                "marker=MOONLIGHT_ACTIVITY_FINAL_CTA_SEMANTICS_LIVE_VERIFIED " +
                                $"marker={ActivityQualityReadoutRuntimeContractMarker}");
                        if (zone.Kind == MoonlightSpatialActionKind.Care)
                        {
                            string careResult = ui != null && ui.resultLabel != null
                                ? ui.resultLabel.text
                                : "";
                            bool careUiPass = MoonlightUI.CompactNavigationLabel(zone.Kind) == "CARE" &&
                                careResult.Contains("MOON SPA COMPLETE") &&
                                careResult.Contains("+18 WARMTH") &&
                                careResult.Contains("+12 REST") &&
                                careResult.Contains("+6 MAGIC") &&
                                careResult.Contains("+12 XP") &&
                                careResult.Contains("+5 COINS");
                            if (!careUiPass)
                            {
                                Debug.LogError($"[MoonlightGameplayQA][FAIL] care-ui-receipt " +
                                    $"label={MoonlightUI.CompactNavigationLabel(zone.Kind)} " +
                                    $"result=\"{careResult.Replace('\n', '/')}\"");
                                Application.Quit(99);
                                yield break;
                            }
                            Debug.Log($"[MoonlightGameplayQA][PASS] care-ui-receipt " +
                                $"label=CARE result=\"{careResult.Replace('\n', '/')}\" " +
                                "marker=MOONLIGHT_CARE_UI_RECEIPT_VERIFIED");
                        }
                        string presentationPrefix = zone.Kind switch
                        {
                            MoonlightSpatialActionKind.Cook => "02",
                            MoonlightSpatialActionKind.Play => "03",
                            MoonlightSpatialActionKind.Garden => "04",
                            MoonlightSpatialActionKind.Read => "05",
                            MoonlightSpatialActionKind.Care => "06",
                            _ => "99"
                        };
                        string presentationShot = Path.Combine(output,
                            $"{presentationPrefix}_{zone.Kind.ToString().ToLowerInvariant()}_final_presentation.png");
                        yield return Capture(presentationShot);
                        Debug.Log($"[MoonlightGameplayQA][PASS] activity-final-presentation action={zone.Kind} " +
                            $"step={step + 1}/{zone.RequiredSteps} remaining={stage.LingerSecondsRemaining:0.00}s " +
                            $"screenshot={presentationShot} marker=MOONLIGHT_ACTIVITY_FINAL_PRESENTATION_VERIFIED");

                    }
                    if (persistentStation != null &&
                        (persistentStation.VisualRoot == null || !persistentStation.gameObject.activeInHierarchy ||
                         Vector3.Distance(persistentStation.AnchorPosition, persistentAnchor) > 0.001f))
                    {
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] persistent-station-drift action={expectedKind} " +
                            $"step={step + 1} expected={persistentAnchor:F2} " +
                            $"actual={(persistentStation != null ? persistentStation.AnchorPosition : Vector3.zero):F2}");
                        Application.Quit(38);
                        yield break;
                    }
                    if (resultVisibilityObservation != null)
                    {
                        float observationDeadline = Time.time + expectedResultDuration + 1f;
                        while (!resultVisibilityObservation.Completed &&
                               Time.time < observationDeadline)
                            yield return null;
                        const float visibleDurationTolerance = 0.20f;
                        bool visibleIntervalPass = resultVisibilityObservation.Completed &&
                            resultVisibilityObservation.FeedbackWasActiveAtStart &&
                            resultVisibilityObservation.FeedbackEnded &&
                            !resultVisibilityObservation.ResultChangedDuringFeedback &&
                            resultVisibilityObservation.ShowAfterFeedback &&
                            resultVisibilityObservation.SawExpectedText &&
                            resultVisibilityObservation.SawRemoval &&
                            Mathf.Abs(resultVisibilityObservation.VisibleSeconds -
                                expectedResultDuration) <= visibleDurationTolerance;
                        if (!visibleIntervalPass)
                        {
                            Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-result-visible-interval " +
                                $"action={zone.Kind} step={step + 1}/{zone.RequiredSteps} " +
                                $"feedbackActiveAtStart=" +
                                $"{resultVisibilityObservation.FeedbackWasActiveAtStart} " +
                                $"feedbackEnded={resultVisibilityObservation.FeedbackEnded} " +
                                $"premature={resultVisibilityObservation.ResultChangedDuringFeedback} " +
                                $"showAfterFeedback={resultVisibilityObservation.ShowAfterFeedback} " +
                                $"timestamps={resultVisibilityObservation.FeedbackEndedAtSeconds:0.000}/" +
                                $"{resultVisibilityObservation.ShownAtSeconds:0.000} " +
                                $"text=\"{resultVisibilityObservation.ObservedText.Replace('\n', '/')}\" " +
                                $"expectedText={resultVisibilityObservation.SawExpectedText} " +
                                $"removed={resultVisibilityObservation.SawRemoval} duration=" +
                                $"{resultVisibilityObservation.VisibleSeconds:0.000}/" +
                                $"{expectedResultDuration:0.000}+/-{visibleDurationTolerance:0.00}s");
                            Application.Quit(149);
                            yield break;
                        }
                        if (finalResultStep)
                            verifiedFinalResultVisibility = true;
                        else
                            verifiedIntermediateResultVisibility = true;
                        Debug.Log($"[MoonlightGameplayQA][PASS] activity-result-visible-interval " +
                            $"action={zone.Kind} step={step + 1}/{zone.RequiredSteps} " +
                            $"text=\"{resultVisibilityObservation.ObservedText.Replace('\n', '/')}\" " +
                            $"feedbackEnd={resultVisibilityObservation.FeedbackEndedAtSeconds:0.000}s " +
                            $"shownAt={resultVisibilityObservation.ShownAtSeconds:0.000}s " +
                            $"duration={resultVisibilityObservation.VisibleSeconds:0.000}s " +
                            $"marker={ActivityResultVisibleIntervalMarker}");
                    }
                }

                if (zone.ProgressStep != 0)
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-loop action={zone.Kind} step={zone.ProgressStep}");
                    Application.Quit(26);
                    yield break;
                }
                if (expectedKind == MoonlightSpatialActionKind.Read)
                {
                    StoryPageUI storyUI = StoryPageUI.Instance;
                    float storyDeadline = Time.time + 8f;
                    while (storyUI != null && !storyUI.IsOpen && Time.time < storyDeadline)
                        yield return null;

                    bool storyRevealPass = storyUI != null && storyUI.DataReady &&
                        storyUI.LoadedPageCount == 10 && storyUI.CompletedReadLoopCount == 1 &&
                        storyUI.RevealedPageCount == 1 && storyUI.PendingRevealCount == 0 &&
                        storyUI.RevealCountIsExact && storyUI.RevealTimingIsValid &&
                        !string.IsNullOrWhiteSpace(storyUI.CurrentTitle) &&
                        !string.IsNullOrWhiteSpace(storyUI.CurrentBody) &&
                        storyUI.UsesTMPVisibleTypography && storyUI.BodyUsesScrolling &&
                        storyUI.VisibleTextDoesNotOverflow && storyUI.IsInsideSafeArea &&
                        storyUI.ModalInputAndNavigationLocked && storyUI.CurrentModalHasZeroDrift &&
                        zone.LastStoryRevealQueueAccepted &&
                        zone.LastStoryRevealRewardPathUnchanged &&
                        zone.StoryRevealRewardQAMarker == StoryPageUI.RewardPathMarker;
                    if (!storyRevealPass)
                    {
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] story-reveal-runtime " +
                            $"present={(storyUI != null)} open={(storyUI != null && storyUI.IsOpen)} " +
                            $"loaded={(storyUI != null ? storyUI.LoadedPageCount : 0)} " +
                            $"completed={(storyUI != null ? storyUI.CompletedReadLoopCount : 0)} " +
                            $"revealed={(storyUI != null ? storyUI.RevealedPageCount : 0)} " +
                            $"pending={(storyUI != null ? storyUI.PendingRevealCount : 0)} " +
                            $"queueTiming={(storyUI != null ? storyUI.LastQueueToRevealSeconds : 0f):0.000} " +
                            $"presentationTiming={(storyUI != null ? storyUI.LastPresentationToRevealSeconds : 0f):0.000} " +
                            $"safe={(storyUI != null && storyUI.IsInsideSafeArea)} " +
                            $"nonOverflow={(storyUI != null && storyUI.VisibleTextDoesNotOverflow)} " +
                            $"modal={(storyUI != null && storyUI.ModalInputAndNavigationLocked)} " +
                            $"zeroDrift={(storyUI != null && storyUI.CurrentModalHasZeroDrift)} " +
                            $"reward={zone.StoryRevealRewardQAMarker}");
                        Application.Quit(114);
                        yield break;
                    }
                    Debug.Log($"[MoonlightGameplayQA][PASS] story-reveal-runtime " +
                        $"loaded={storyUI.LoadedPageCount} completed={storyUI.CompletedReadLoopCount} " +
                        $"revealed={storyUI.RevealedPageCount} queueElapsed={storyUI.LastQueueToRevealSeconds:0.000}s " +
                        $"presentationElapsed={storyUI.LastPresentationToRevealSeconds:0.000}s " +
                        $"titleChars={storyUI.CurrentTitle.Length} bodyChars={storyUI.CurrentBody.Length} " +
                        $"markers={LibraryRoom.StoryDataReadyMarker},{StoryPageUI.RevealCountMarker}," +
                        $"{StoryPageUI.TimingMarker},{StoryPageUI.SafeAreaMarker}," +
                        $"{StoryPageUI.NonOverflowMarker},{StoryPageUI.ModalLockMarker}," +
                        $"{StoryPageUI.ZeroDriftMarker},{StoryPageUI.RewardPathMarker}");

                    storyUI.Close();
                    yield return null;
                    if (!storyUI.LastCloseRestoredWithoutDrift)
                    {
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] story-modal-restore " +
                            $"playerDrift={storyUI.LastModalPlayerDrift:0.000000} " +
                            $"xpDrift={storyUI.LastModalXPDrift} coinDrift={storyUI.LastModalCoinDrift} " +
                            $"controllerLocked={controller.IsModalInputLocked}");
                        Application.Quit(115);
                        yield break;
                    }
                    Debug.Log($"[MoonlightGameplayQA][PASS] story-modal-restore " +
                        $"playerDrift={storyUI.LastModalPlayerDrift:0.000000} " +
                        $"xpDrift={storyUI.LastModalXPDrift} coinDrift={storyUI.LastModalCoinDrift} " +
                        $"marker={StoryPageUI.ZeroDriftMarker}");
                }
                if (expectedKind == MoonlightSpatialActionKind.Care &&
                    (verifiedCareContacts.Count != 4 ||
                     verifiedGestureResponsiveCareSteps != 4))
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] care-live-contract " +
                        $"contacts={verifiedCareContacts.Count}/4 " +
                        $"responsiveSteps={verifiedGestureResponsiveCareSteps}/4 " +
                        $"targets={string.Join(",", verifiedCareContacts.OrderBy(target => target))} " +
                        $"marker={GestureCareRuntimeContractFailureMarker}");
                    Application.Quit(100);
                    yield break;
                }
                if (expectedKind == MoonlightSpatialActionKind.Care)
                    Debug.Log($"[MoonlightGameplayQA][PASS] care-live-contract " +
                        $"contacts={verifiedCareContacts.Count}/4 " +
                        $"responsiveSteps={verifiedGestureResponsiveCareSteps}/4 " +
                        $"marker={GestureCareRuntimeContractMarker}");
                if (persistentStation != null)
                {
                    RoomType detour = activityRooms[activityIndex] == RoomType.LivingRoom
                        ? RoomType.Kitchen
                        : RoomType.LivingRoom;
                    rooms.GoToRoom(detour);
                    yield return new WaitForSeconds(0.12f);
                    if (cookContinuity != null)
                    {
                        int buildDelta = cookContinuity.Stage != null
                            ? cookContinuity.Stage.CookStageBuildCountForQA -
                                cookContinuity.BuildBaseline
                            : -1;
                        int handoffDelta = cookContinuity.Stage != null
                            ? cookContinuity.Stage.CookHandoffCountForQA -
                                cookContinuity.HandoffBaseline
                            : -1;
                        bool cleanupPass = cookContinuity.Stage != null &&
                            !cookContinuity.Stage.IsVisible &&
                            !cookContinuity.Stage.IsLingering &&
                            !cookContinuity.Stage.IsHoldingCookStepTerminal &&
                            cookContinuity.Stage.CookStageRootInstanceId == 0 &&
                            cookContinuity.Stage.CookStageMaterialIdentityCountForQA == 0 &&
                            cookContinuity.Stage.CookStageMaterialIdentityHashForQA == 0 &&
                            cookContinuity.Stage.CookStageLightInstanceId == 0 &&
                            cookContinuity.Stage.ActiveRendererCount == 0 &&
                            cookContinuity.Stage.ActiveUniqueMaterialCount == 0 &&
                            cookContinuity.Stage.ActiveLightCount == 0 &&
                            CountActiveCookStageRoots() == 0;
                        bool transactionPass = buildDelta == 1 && handoffDelta == 3 &&
                            cookContinuity.HandoffCount == 3 &&
                            cookContinuity.SharedPropHandoffMask == 0x7 &&
                            cookContinuity.RootIdentityHandoffMask == 0x7 &&
                            cookContinuity.MaterialIdentityHandoffMask == 0x7 &&
                            cookContinuity.LightIdentityHandoffMask == 0x7 &&
                            cookContinuity.TerminalHoldCount == 3 &&
                            cookContinuity.IdentityPreserved &&
                            cookContinuity.MaterialIdentityCount > 0 &&
                            cookContinuity.MaximumSharedPropDiscontinuity <= 0.001f &&
                            cookContinuity.SourceObservedMask ==
                                (1 << MoonlightActivityStage.CookPhaseCount) - 1 &&
                            cookContinuity.SourceContractPassed &&
                            (cookContinuity.Source == "authored" ||
                             cookContinuity.Source == "fallback") &&
                            cookContinuity.BudgetContractPassed &&
                            cookContinuity.RevealTrayCompletionObserved &&
                            cookContinuity.RevealTrayLingerObserved &&
                            Mathf.Approximately(
                                MoonlightActivityStage.CookFinalPresentationSeconds, 5.2f) &&
                            cookContinuity.FinalLingerObserved && cleanupPass;
                        if (!transactionPass)
                        {
                            Debug.LogError("[MoonlightGameplayQA][FAIL] " +
                                $"cook-continuity-transaction build={buildDelta}/1 " +
                                $"handoffs={handoffDelta}/{cookContinuity.HandoffCount}/3 " +
                                $"shared=0x{cookContinuity.SharedPropHandoffMask:X}/7 " +
                                $"identityMasks={cookContinuity.RootIdentityHandoffMask:X}/" +
                                $"{cookContinuity.MaterialIdentityHandoffMask:X}/" +
                                $"{cookContinuity.LightIdentityHandoffMask:X} " +
                                $"terminalHolds={cookContinuity.TerminalHoldCount}/3 " +
                                $"root={cookContinuity.RootId} materials=" +
                                $"{cookContinuity.MaterialIdentityCount}/" +
                                $"{cookContinuity.MaterialIdentityHash} " +
                                $"light={cookContinuity.LightId} identity=" +
                                $"{cookContinuity.IdentityPreserved} discontinuity=" +
                                $"{cookContinuity.MaximumSharedPropDiscontinuity:0.000000}m " +
                                $"source={cookContinuity.Source}/" +
                                $"0x{cookContinuity.SourceObservedMask:X}/" +
                                $"{cookContinuity.SourceContractPassed} budget=" +
                                $"{cookContinuity.BudgetContractPassed} reveal=" +
                                $"{cookContinuity.RevealTrayCompletionObserved}/" +
                                $"{cookContinuity.RevealTrayLingerObserved} linger=" +
                                $"{cookContinuity.FinalLingerObserved} cleanup={cleanupPass}");
                            Application.Quit(162);
                            yield break;
                        }
                        Debug.Log("[MoonlightGameplayQA][PASS] " +
                            $"cook-continuity-transaction build={buildDelta}/1 " +
                            $"handoffs={handoffDelta}/3 shared=0x" +
                            $"{cookContinuity.SharedPropHandoffMask:X}/7 terminalHolds=" +
                            $"{cookContinuity.TerminalHoldCount}/3 root=" +
                            $"{cookContinuity.RootId} materials=" +
                            $"{cookContinuity.MaterialIdentityCount}/" +
                            $"{cookContinuity.MaterialIdentityHash} light=" +
                            $"{cookContinuity.LightId} identityMasks=" +
                            $"{cookContinuity.RootIdentityHandoffMask:X}/" +
                            $"{cookContinuity.MaterialIdentityHandoffMask:X}/" +
                            $"{cookContinuity.LightIdentityHandoffMask:X} " +
                            $"discontinuity=" +
                            $"{cookContinuity.MaximumSharedPropDiscontinuity:0.000000}m " +
                            $"source={cookContinuity.Source} budget=36r/24m/1l " +
                            $"revealTray={cookContinuity.RevealTrayCompletionObserved}/" +
                            $"{cookContinuity.RevealTrayLingerObserved} " +
                            $"linger={MoonlightActivityStage.CookFinalPresentationSeconds:0.0}s " +
                            "cleanup=True " +
                            $"marker={CookContinuityRuntimeMarker}");
                    }
                    if (playContinuity != null)
                    {
                        bool firstThreeRewardsUnchanged = true;
                        for (int acceptedStep = 0; acceptedStep < 3; acceptedStep++)
                            firstThreeRewardsUnchanged &= PlayRewardsEqual(
                                playContinuity.BaselineRewards,
                                playContinuity.AcceptedRewards[acceptedStep]);
                        PlayRewardSnapshot finalRewards =
                            playContinuity.AcceptedRewards[3];
                        bool finalRewardPass = PlayRewardDeltaMatches(
                            playContinuity.BaselineRewards, finalRewards,
                            25f, 0f, 0f, 13f, 0f, 32, 5);
                        bool progressionPass = playContinuity.Progression.SequenceEqual(
                            new[] { 0, 1, 2, 3, 0 });
                        bool cleanupPass = playContinuity.Stage != null &&
                            !playContinuity.Stage.IsVisible &&
                            !playContinuity.Stage.IsLingering &&
                            !playContinuity.Stage.IsHoldingPlayStepTerminal &&
                            playContinuity.Stage.PlayStageRootInstanceId == 0 &&
                            playContinuity.Stage.PlayBallInstanceId == 0 &&
                            playContinuity.Stage.PlayTrailInstanceId == 0 &&
                            playContinuity.Stage.ActiveUniqueMaterialCount == 0 &&
                            playContinuity.Stage.PlayBallSharedMaterialInstanceId == 0 &&
                            playContinuity.Stage.PlayTrailSharedMaterialInstanceId == 0 &&
                            CountActivePlayStageRoots() == 0;
                        int completionDelta = playContinuity.Stage != null
                            ? playContinuity.Stage.PersistentCompletionApplicationCountForQA -
                                playContinuity.CompletionApplicationBaseline
                            : -1;
                        int continuationDelta = playContinuity.Stage != null
                            ? playContinuity.Stage.PlayContinuationCountForQA -
                                playContinuity.ContinuationBaseline
                            : -1;
                        bool phaseLandmarkVisibilityPass = true;
                        for (int phase = 0;
                             phase < MoonlightActivityStage.PlayPhaseCount; phase++)
                            phaseLandmarkVisibilityPass &=
                                playContinuity.PhaseLandmarkVisibilityMasks[phase] ==
                                MoonlightActivityStage.PlayPhaseExpectedVisibilityMask(phase);
                        bool phaseLandmarkTransactionPass =
                            playContinuity.PhaseLandmarkContractPassed &&
                            playContinuity.ArenaSourceContractPassed &&
                            (playContinuity.ArenaSource == "authored" ||
                             playContinuity.ArenaSource == "fallback") &&
                            playContinuity.PhaseLandmarkObservedMask ==
                                (1 << MoonlightActivityStage.PlayPhaseCount) - 1 &&
                            phaseLandmarkVisibilityPass &&
                            playContinuity.PhaseLandmarkMaterialCounts[0] > 0 &&
                            playContinuity.PhaseLandmarkMaterialCounts.All(count =>
                                count == playContinuity.PhaseLandmarkMaterialCounts[0]) &&
                            playContinuity.PhaseLandmarkLightCounts.All(count =>
                                count == MoonlightActivityStage.PlayLightBudget);
                        bool transactionPass = progressionPass &&
                            playContinuity.IdentityPreserved &&
                            playContinuity.MaximumImmediateDiscontinuity <= 0.001f &&
                            playContinuity.FirstRenderedContinuationFramesObserved == 3 &&
                            playContinuity.MaximumFirstRenderedFrameDiscontinuity <= 0.001f &&
                            playContinuity.TrajectoryFiniteAndBounded &&
                            playContinuity.TrajectoryStepsObserved == 4 &&
                            playContinuity.TrajectorySampleCount >= 8 &&
                            playContinuity.RejectedPreservationCount == 9 &&
                            playContinuity.HasInitialMaterialSnapshot &&
                            playContinuity.MaterialCounts.All(count =>
                                count == playContinuity.InitialMaterialCount) &&
                            playContinuity.ContinuationClockSourceIsUnscaled &&
                            playContinuity.ContinuationClockAdvanced &&
                            playContinuity.MaximumContinuationClockDelta <=
                                MoonlightActivityStage.PlayContinuationMaximumDeltaSeconds &&
                            playContinuity.CatchTapOraclePassed &&
                            firstThreeRewardsUnchanged && finalRewardPass &&
                            zone.LastCompletedBestCombo == 4 &&
                            zone.LastCompletedPerfectSteps == 4 &&
                            zone.LastMasteryBonusCoins == 3 &&
                            completionDelta == 1 && continuationDelta == 3 &&
                            phaseLandmarkTransactionPass && cleanupPass;
                        if (!transactionPass)
                        {
                            Debug.LogError("[MoonlightGameplayQA][FAIL] " +
                                $"play-continuity-transaction progression=" +
                                $"{string.Join("->", playContinuity.Progression)} " +
                                $"ids={playContinuity.RootId}/" +
                                $"{playContinuity.BallId}/{playContinuity.TrailId} " +
                                $"identity={playContinuity.IdentityPreserved} " +
                                $"discontinuity=" +
                                $"{playContinuity.MaximumImmediateDiscontinuity:0.000000}m " +
                                $"firstRendered=" +
                                $"{playContinuity.FirstRenderedContinuationFramesObserved}/3@" +
                                $"{playContinuity.MaximumFirstRenderedFrameDiscontinuity:0.000000}m " +
                                $"materials={playContinuity.InitialMaterialCount}/" +
                                $"{playContinuity.BallMaterialId}/" +
                                $"{playContinuity.TrailMaterialId} counts=" +
                                $"{string.Join(",", playContinuity.MaterialCounts)} " +
                                $"trajectory={playContinuity.TrajectoryStepsObserved}/4 " +
                                $"samples={playContinuity.TrajectorySampleCount} " +
                                $"bounded={playContinuity.TrajectoryFiniteAndBounded} " +
                                $"catchTap={playContinuity.CatchTapOraclePassed}/" +
                                $"distinct:{playContinuity.CatchTapSamplesDistinct}/" +
                                $"samples:{playContinuity.CatchTapSamplesPerTrajectory}x2/" +
                                $"sep020:{playContinuity.CatchTapSeparationAt020:0.000}/" +
                                $"contact:{playContinuity.CatchTapMaximumContactError:0.000000}/" +
                                $"held:{playContinuity.CatchTapMaximumHeldError:0.000000}/" +
                                $"bounded:{playContinuity.CatchTapFiniteAndBounded}/" +
                                $"noCrossOvershoot:{playContinuity.CatchTapNoIntersectionOrOvershoot}/" +
                                $"center:{playContinuity.CatchTapCenterMaximumDelta:0.000000} " +
                                $"clock={MoonlightActivityStage.PlayContinuationClockSourceForQA}/" +
                                $"{playContinuity.MaximumContinuationClockDelta:0.000000}/" +
                                $"{playContinuity.ContinuationClockAdvanced} " +
                                $"landmarks={playContinuity.PhaseLandmarkObservedMask:X}/F " +
                                $"arenaSource={playContinuity.ArenaSource}/" +
                                $"{playContinuity.ArenaSourceContractPassed} " +
                                $"visibility=" +
                                $"{string.Join(",", playContinuity.PhaseLandmarkVisibilityMasks.Select(mask => $"0x{mask:X3}"))} " +
                                $"phaseMaterials=" +
                                $"{string.Join(",", playContinuity.PhaseLandmarkMaterialCounts)} " +
                                $"phaseLights=" +
                                $"{string.Join(",", playContinuity.PhaseLandmarkLightCounts)} " +
                                $"landmarkTransaction={phaseLandmarkTransactionPass} " +
                                $"rejections={playContinuity.RejectedPreservationCount}/9 " +
                                $"firstThreeRewards={firstThreeRewardsUnchanged} " +
                                $"finalRewards={finalRewardPass} mastery=" +
                                $"{zone.LastCompletedBestCombo}/" +
                                $"{zone.LastCompletedPerfectSteps}/" +
                                $"{zone.LastMasteryBonusCoins} completion=" +
                                $"{completionDelta}/1 continuations={continuationDelta}/3 " +
                                $"cleanup={cleanupPass}");
                            Application.Quit(161);
                            yield break;
                        }
                        Debug.Log("[MoonlightGameplayQA][PASS] " +
                            $"play-continuity-transaction progression=" +
                            $"{string.Join("->", playContinuity.Progression)} " +
                            $"order={playContinuity.OrderMode} " +
                            $"root={playContinuity.RootId} ball={playContinuity.BallId} " +
                            $"trail={playContinuity.TrailId} discontinuity=" +
                            $"{playContinuity.MaximumImmediateDiscontinuity:0.000000}m " +
                            $"firstRendered=" +
                            $"{playContinuity.MaximumFirstRenderedFrameDiscontinuity:0.000000}m " +
                            $"materials={playContinuity.InitialMaterialCount}/" +
                            $"{playContinuity.BallMaterialId}/" +
                            $"{playContinuity.TrailMaterialId} counts=" +
                            $"{string.Join(",", playContinuity.MaterialCounts)} " +
                            $"trajectorySamples={playContinuity.TrajectorySampleCount} " +
                            $"catchTap=41x2/sep020:" +
                            $"{playContinuity.CatchTapSeparationAt020:0.000}m/" +
                            $"contact:{playContinuity.CatchTapMaximumContactError:0.000000}m/" +
                            $"held:{playContinuity.CatchTapMaximumHeldError:0.000000}m/" +
                            $"center:{playContinuity.CatchTapCenterMaximumDelta:0.000000}m " +
                            $"clock={MoonlightActivityStage.PlayContinuationClockSourceForQA}/" +
                            $"{playContinuity.MaximumContinuationClockDelta:0.000000} " +
                            $"landmarkVisibility=" +
                            $"{string.Join(",", playContinuity.PhaseLandmarkVisibilityMasks.Select(mask => $"0x{mask:X3}"))} " +
                            $"arenaSource={playContinuity.ArenaSource} " +
                            $"phaseMaterials=" +
                            $"{string.Join(",", playContinuity.PhaseLandmarkMaterialCounts)} " +
                            $"phaseLights=" +
                            $"{string.Join(",", playContinuity.PhaseLandmarkLightCounts)} " +
                            "runtimePauseExecuted=False " +
                            $"rejections={playContinuity.RejectedPreservationCount} " +
                            "rewards=+25W/+13M/+32XP/+5C completion=1 cleanup=True " +
                            $"marker={PlayContinuityRuntimeMarker} " +
                            $"marker={PlayPhaseLandmarkTransactionMarker} " +
                            $"marker={PlayContinuityOrderModeMarker}_" +
                            $"{playContinuity.OrderMode.ToUpperInvariant()}");
                    }
                    if (expectIPadHud && ui != null &&
                        (!ui.IsRoomNavigationVisible || ui.IsRoomNavigationLocked))
                    {
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-room-navigation-restore " +
                            $"action={expectedKind} visible={ui.IsRoomNavigationVisible} " +
                            $"locked={ui.IsRoomNavigationLocked} marker={ui.RoomNavigationQAMarker}");
                        Application.Quit(51);
                        yield break;
                    }
                    if (expectIPadHud && ui != null)
                        Debug.Log($"[MoonlightGameplayQA][PASS] activity-room-navigation-restore " +
                            $"action={expectedKind} marker={ui.RoomNavigationQAMarker}");
                    bool qualityReadoutReset = ui != null &&
                        !ui.IsActivityQualityReadoutVisible &&
                        string.IsNullOrEmpty(ui.ActivityQualityReadoutQAText) &&
                        ui.ActivityQualityReadoutComboForQA == 0 &&
                        Approximately(ui.ActivityQualityReadoutRunScoreForQA, 0f) &&
                        !ui.ActivityQualityUsesCompletedSnapshotForQA &&
                        ui.ActivityQualityReadoutGraphicsDoNotBlockRaycasts;
                    if (expectIPadHud && !qualityReadoutReset)
                    {
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] " +
                            $"activity-quality-zone-exit action={expectedKind} " +
                            $"ui={(ui != null)} " +
                            $"visible={(ui != null && ui.IsActivityQualityReadoutVisible)} " +
                            $"text=\"{(ui != null ? ui.ActivityQualityReadoutQAText.Replace('\n', '/') : "missing")}\" " +
                            $"combo={(ui != null ? ui.ActivityQualityReadoutComboForQA : -1)}/0 " +
                            $"run={(ui != null ? ui.ActivityQualityReadoutRunScoreForQA : -1f):0.000}/0.000 " +
                            $"completedSnapshot={(ui != null && ui.ActivityQualityUsesCompletedSnapshotForQA)} " +
                            $"nonBlocking={(ui != null && ui.ActivityQualityReadoutGraphicsDoNotBlockRaycasts)}");
                        Application.Quit(157);
                        yield break;
                    }
                    if (expectIPadHud)
                        Debug.Log($"[MoonlightGameplayQA][PASS] activity-quality-zone-exit " +
                            $"action={expectedKind} visible={ui.IsActivityQualityReadoutVisible} " +
                            $"textEmpty={string.IsNullOrEmpty(ui.ActivityQualityReadoutQAText)} " +
                            $"combo={ui.ActivityQualityReadoutComboForQA} " +
                            $"run={ui.ActivityQualityReadoutRunScoreForQA:0.000} " +
                            $"completedSnapshot={ui.ActivityQualityUsesCompletedSnapshotForQA} " +
                            $"nonBlocking={ui.ActivityQualityReadoutGraphicsDoNotBlockRaycasts} " +
                            $"marker={ActivityQualityReadoutZoneExitMarker}");
                    if (ui != null && (ui.HasContextResult || !ui.ContextResultMatchesCurrentZone))
                    {
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-result-context action={expectedKind} " +
                            $"visible={ui.HasContextResult} matches={ui.ContextResultMatchesCurrentZone}");
                        Application.Quit(48);
                        yield break;
                    }
                    Debug.Log($"[MoonlightGameplayQA][PASS] activity-result-context action={expectedKind} " +
                        "marker=MOONLIGHT_ACTIVITY_RESULT_CONTEXT_CLEARED");
                    rooms.GoToRoom(activityRooms[activityIndex]);
                    yield return new WaitForSeconds(0.18f);
                    int expectedCompletionRenderers = expectedKind switch
                    {
                        MoonlightSpatialActionKind.Cook => 12,
                        MoonlightSpatialActionKind.Play => 10,
                        MoonlightSpatialActionKind.Garden =>
                            5 + persistentStation.CompletionMagicFlowerRendererCount,
                        MoonlightSpatialActionKind.Read => 10,
                        MoonlightSpatialActionKind.Care => 10,
                        _ => 0
                    };
                    int completionRendererBudget = expectedKind == MoonlightSpatialActionKind.Garden
                        ? 5 + persistentStation.CompletionMagicFlowerRendererBudget
                        : expectedCompletionRenderers;
                    int completionMaterialBudget = expectedKind == MoonlightSpatialActionKind.Garden
                        ? 1 + persistentStation.CompletionMagicFlowerUniqueMaterialCount
                        : 6;
                    bool completionMagicFlowerPass = expectedKind != MoonlightSpatialActionKind.Garden ||
                        (persistentStation.HasCompletionMagicFlowerPrefab &&
                         persistentStation.CompletionMagicFlowerInstanceCount == 5 &&
                         persistentStation.CompletionMagicFlowerRendererCount <=
                             persistentStation.CompletionMagicFlowerRendererBudget &&
                         persistentStation.CompletionMagicFlowerUniqueMaterialCount > 0 &&
                         persistentStation.CompletionMagicFlowerUsesSharedMaterials &&
                         persistentStation.CompletionMagicFlowerEnabledColliderCount == 0 &&
                         persistentStation.CompletionMagicFlowerEnabledLightCount == 0 &&
                         persistentStation.CompletionMagicFlowerQAMarker ==
                             "MOONLIGHT_MAGIC_FLOWER_PERSISTENT_READY");
                    if (persistentStation.VisualRoot == null || !persistentStation.gameObject.activeInHierarchy ||
                        Vector3.Distance(persistentStation.AnchorPosition, persistentAnchor) > 0.001f ||
                        !persistentStation.HasCompletionState ||
                        persistentStation.CompletionRendererCount != expectedCompletionRenderers ||
                        persistentStation.CompletionRendererCount > completionRendererBudget ||
                        persistentStation.CompletionUniqueMaterialCount <= 0 ||
                        persistentStation.CompletionUniqueMaterialCount > completionMaterialBudget ||
                        !persistentStation.CompletionUsesSeparateMaterials ||
                        persistentStation.CompletionEnabledColliderCount != 0 ||
                        persistentStation.CompletionEnabledLightCount != 0 ||
                        !completionMagicFlowerPass)
                    {
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] persistent-station-reentry action={expectedKind} " +
                            $"completion={persistentStation.HasCompletionState} " +
                            $"renderers={persistentStation.CompletionRendererCount}/{expectedCompletionRenderers} " +
                            $"budget={completionRendererBudget} " +
                            $"materials={persistentStation.CompletionUniqueMaterialCount}/<={completionMaterialBudget} " +
                            $"separateMaterials={persistentStation.CompletionUsesSeparateMaterials} " +
                            $"colliders={persistentStation.CompletionEnabledColliderCount} " +
                            $"lights={persistentStation.CompletionEnabledLightCount} " +
                            $"magicFlower={completionMagicFlowerPass} " +
                            $"magicInstances={persistentStation.CompletionMagicFlowerInstanceCount}/5 " +
                            $"magicRenderers={persistentStation.CompletionMagicFlowerRendererCount}/" +
                            $"{persistentStation.CompletionMagicFlowerRendererBudget} " +
                            $"magicShared={persistentStation.CompletionMagicFlowerUsesSharedMaterials} " +
                            $"magicMarker={persistentStation.CompletionMagicFlowerQAMarker}");
                        Application.Quit(39);
                        yield break;
                    }
                    if (expectedKind == MoonlightSpatialActionKind.Garden)
                        Debug.Log("[MoonlightGameplayQA][PASS] garden-magic-flower-persistent " +
                            $"instances={persistentStation.CompletionMagicFlowerInstanceCount} " +
                            $"renderers={persistentStation.CompletionMagicFlowerRendererCount}/" +
                            $"{persistentStation.CompletionMagicFlowerRendererBudget} " +
                            $"materials={persistentStation.CompletionMagicFlowerUniqueMaterialCount} " +
                            $"shared={persistentStation.CompletionMagicFlowerUsesSharedMaterials} " +
                            $"colliders={persistentStation.CompletionMagicFlowerEnabledColliderCount} " +
                            $"lights={persistentStation.CompletionMagicFlowerEnabledLightCount} " +
                            "marker=MOONLIGHT_MAGIC_FLOWER_PERSISTENT_VERIFIED");
                    string reentryPrefix = expectedKind switch
                    {
                        MoonlightSpatialActionKind.Cook => "02",
                        MoonlightSpatialActionKind.Play => "03",
                        MoonlightSpatialActionKind.Garden => "04",
                        MoonlightSpatialActionKind.Read => "05",
                        MoonlightSpatialActionKind.Care => "06",
                        _ => "99"
                    };
                    string reentryShot = Path.Combine(output,
                        $"{reentryPrefix}_{expectedKind.ToString().ToLowerInvariant()}_persistent_reentry.png");
                    yield return Capture(reentryShot);
                    Debug.Log($"[MoonlightGameplayQA][PASS] persistent-station-reentry action={expectedKind} " +
                        $"anchor={persistentStation.AnchorPosition:F2} renderers={persistentStation.CompletionRendererCount} " +
                        $"materials={persistentStation.CompletionUniqueMaterialCount} " +
                        $"separateMaterials={persistentStation.CompletionUsesSeparateMaterials} screenshot={reentryShot} " +
                        "marker=MOONLIGHT_PERSISTENT_ACTIVITY_STATE_VERIFIED " +
                        "marker=MOONLIGHT_PERSISTENT_ACTIVITY_REENTRY_VERIFIED");
                    verifiedPersistentStations++;
                }
                completedActivities++;
            }

            bool gestureLockCleanupMatrixPass = verifiedGestureLockCleanup.SetEquals(new[]
            {
                "pointer-up", "event-cancel", "focus-lost", "paused", "disabled",
                "zone-changed"
            });
            if (verifyLiveHoldRuntime &&
                (verifiedLiveHoldRuntimeActions != 4 ||
                 !gestureLockCleanupMatrixPass ||
                 verifiedRejectedGestureMovementRetention != 4))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-live-hold-runtime-matrix " +
                    $"actions={verifiedLiveHoldRuntimeActions}/4 " +
                    $"cleanup={verifiedGestureLockCleanup.Count}/6 " +
                    $"rejectedRetention={verifiedRejectedGestureMovementRetention}/4");
                Application.Quit(142);
                yield break;
            }
            if (verifyLiveHoldRuntime)
                Debug.Log($"[MoonlightGameplayQA][PASS] ipad-live-hold-runtime-matrix " +
                    $"actions={verifiedLiveHoldRuntimeActions}/4 " +
                    $"cleanup={verifiedGestureLockCleanup.Count}/6 " +
                    $"rejectedRetention={verifiedRejectedGestureMovementRetention}/4 " +
                    $"marker=MOONLIGHT_IPAD_LIVE_HOLD_RUNTIME_4_OF_4_VERIFIED " +
                    $"marker=MOONLIGHT_IPAD_CONTEXT_GESTURE_MOVEMENT_LOCK_6_OF_6_VERIFIED");

            int requiredLowScoreRetryKinds = expectIPadHud ? 5 : 0;
            bool lowScoreRetryMatrixPass =
                verifiedLowScoreRetryKinds.Count == requiredLowScoreRetryKinds;
            string lowScoreRetryMetric = expectIPadHud
                ? $"{verifiedLowScoreRetryKinds.Count}/5"
                : $"{verifiedLowScoreRetryKinds.Count}/0(skip)";
            string lowScoreRetryMarker = expectIPadHud
                ? $"marker={LowScoreRetryRuntimeMarker} "
                : "";
            if (completedActivities != 5 || verifiedPersistentStations != 5 ||
                verifiedVisualSignatures.Count != 20 || busyGestureProbeCount != 20 ||
                !lowScoreRetryMatrixPass ||
                !verifiedIntermediateResultVisibility || !verifiedFinalResultVisibility)
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] scored-activity-matrix " +
                    $"completedActivities={completedActivities}/5 " +
                    $"persistentStations={verifiedPersistentStations}/5 " +
                    $"signatures={verifiedVisualSignatures.Count}/20 " +
                    $"busyProbes={busyGestureProbeCount}/20 " +
                    $"lowScoreRetry={lowScoreRetryMetric} " +
                    $"visibleIntervals={verifiedIntermediateResultVisibility}/" +
                    $"{verifiedFinalResultVisibility}");
                Application.Quit(101);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] scored-activity-matrix " +
                $"completedActivities={completedActivities}/5 " +
                $"persistentStations={verifiedPersistentStations}/5 " +
                $"signatures={verifiedVisualSignatures.Count}/20 " +
                $"busyProbes={busyGestureProbeCount}/20 " +
                $"lowScoreRetry={lowScoreRetryMetric} " +
                $"visibleIntervals={verifiedIntermediateResultVisibility}/" +
                $"{verifiedFinalResultVisibility} " +
                lowScoreRetryMarker +
                "marker=MOONLIGHT_FIVE_ACTIVITY_MATRIX_VERIFIED");
            if (audio.CuePlayCount < 23)
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] audio cues count={audio.CuePlayCount}/23");
                Application.Quit(27);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] suite collisions={controller.CollisionCount} " +
                $"audioCues={audio.CuePlayCount} rooms={rooms.rooms.Count} activities={completedActivities}");
            Application.Quit(0);
        }

        static MoonlightSpatialActionKind[] ResolveGameplayActivityOrder(string[] args,
            out string mode)
        {
            mode = "middle";
            int modeIndex = System.Array.FindIndex(args, argument =>
                string.Equals(argument, "-moonlightPlayContinuityOrder",
                    System.StringComparison.OrdinalIgnoreCase));
            if (modeIndex >= 0 && modeIndex + 1 < args.Length)
            {
                string requested = args[modeIndex + 1].ToLowerInvariant();
                if (requested is "first" or "middle" or "last") mode = requested;
            }

            return mode switch
            {
                "first" => new[]
                {
                    MoonlightSpatialActionKind.Play,
                    MoonlightSpatialActionKind.Cook,
                    MoonlightSpatialActionKind.Garden,
                    MoonlightSpatialActionKind.Read,
                    MoonlightSpatialActionKind.Care
                },
                "last" => new[]
                {
                    MoonlightSpatialActionKind.Cook,
                    MoonlightSpatialActionKind.Garden,
                    MoonlightSpatialActionKind.Read,
                    MoonlightSpatialActionKind.Care,
                    MoonlightSpatialActionKind.Play
                },
                _ => new[]
                {
                    MoonlightSpatialActionKind.Cook,
                    MoonlightSpatialActionKind.Play,
                    MoonlightSpatialActionKind.Garden,
                    MoonlightSpatialActionKind.Read,
                    MoonlightSpatialActionKind.Care
                }
            };
        }

        static RoomType ActivityRoomFor(MoonlightSpatialActionKind kind) => kind switch
        {
            MoonlightSpatialActionKind.Cook => RoomType.Kitchen,
            MoonlightSpatialActionKind.Play => RoomType.LivingRoom,
            MoonlightSpatialActionKind.Garden => RoomType.Garden,
            MoonlightSpatialActionKind.Read => RoomType.Library,
            MoonlightSpatialActionKind.Care => RoomType.Bedroom,
            _ => RoomType.LivingRoom
        };

        static string ExpectedInitialLowScoreIPadResult(
            MoonlightSpatialActionKind kind) => kind switch
        {
            MoonlightSpatialActionKind.Cook =>
                "TRY AGAIN / TAP TO ADD\nSTEP 1/4 / SCORE 20",
            MoonlightSpatialActionKind.Play =>
                "TRY AGAIN / SWIPE TO THROW\nSTEP 1/4 / SCORE 20",
            MoonlightSpatialActionKind.Garden =>
                "TRY AGAIN / TAP TO PLANT\nSTEP 1/4 / SCORE 20",
            MoonlightSpatialActionKind.Read =>
                "TRY AGAIN / TAP TO OPEN\nSTEP 1/4 / SCORE 20",
            MoonlightSpatialActionKind.Care =>
                "TRY AGAIN / TAP TO PREP\nSTEP 1/4 / SCORE 20",
            _ => ""
        };

        static MoonlightGestureSample PlayContinuitySample(int step, float score)
        {
            Vector2[] points = Mathf.Clamp(step, 0, 3) switch
            {
                0 => new[]
                {
                    new Vector2(-0.78f, -0.28f), new Vector2(-0.52f, -0.16f),
                    new Vector2(-0.26f, -0.04f), new Vector2(0f, 0.08f),
                    new Vector2(0.26f, 0.20f), new Vector2(0.52f, 0.32f),
                    new Vector2(0.78f, 0.44f)
                },
                1 => new[]
                {
                    new Vector2(-0.78f, -0.42f), new Vector2(-0.52f, 0.42f),
                    new Vector2(-0.26f, -0.42f), new Vector2(0f, 0.42f),
                    new Vector2(0.26f, -0.42f), new Vector2(0.52f, 0.42f),
                    new Vector2(0.78f, -0.42f)
                },
                2 => new[]
                {
                    new Vector2(-0.48f, -0.72f), new Vector2(-0.32f, -0.48f),
                    new Vector2(-0.16f, -0.24f), new Vector2(0f, 0f),
                    new Vector2(0.16f, 0.24f), new Vector2(0.32f, 0.48f),
                    new Vector2(0.48f, 0.72f)
                },
                _ => new[]
                {
                    new Vector2(0.18f, -0.12f), new Vector2(0.18f, -0.12f),
                    new Vector2(0.18f, -0.12f), new Vector2(0.18f, -0.12f),
                    new Vector2(0.18f, -0.12f), new Vector2(0.18f, -0.12f),
                    new Vector2(0.18f, -0.12f)
                }
            };
            return MoonlightGestureSample.Create(score, 0.42f, points);
        }

        static void MeasurePlayCatchTapTrajectories(PlayContinuityObservation observation)
        {
            MoonlightGestureSample leftTap = PlayCatchTapSample(new Vector2(-0.60f, 0f));
            MoonlightGestureSample rightTap = PlayCatchTapSample(new Vector2(0.60f, 0f));
            MoonlightGestureSample centerTap = PlayCatchTapSample(Vector2.zero);
            observation.CatchTapSamplesDistinct = !leftTap.ContentEquals(rightTap);
            observation.CatchTapSeparationAt020 = Vector3.Distance(
                MoonlightActivityStage.EvaluatePlayTrajectory(3, 0.20f, leftTap),
                MoonlightActivityStage.EvaluatePlayTrajectory(3, 0.20f, rightTap));

            float previousLeftDistance = float.PositiveInfinity;
            float previousRightDistance = float.PositiveInfinity;
            for (int i = 0; i <= 40; i++)
            {
                float progress = i / 40f;
                Vector3 leftPoint = MoonlightActivityStage.EvaluatePlayTrajectory(
                    3, progress, leftTap);
                Vector3 rightPoint = MoonlightActivityStage.EvaluatePlayTrajectory(
                    3, progress, rightTap);
                observation.CatchTapSamplesPerTrajectory++;
                observation.CatchTapFiniteAndBounded &=
                    PlayCatchOraclePointIsFiniteAndBounded(leftPoint) &&
                    PlayCatchOraclePointIsFiniteAndBounded(rightPoint);

                float leftDistance = Vector3.Distance(
                    leftPoint, MoonlightActivityStage.PlayCatchPoint);
                float rightDistance = Vector3.Distance(
                    rightPoint, MoonlightActivityStage.PlayCatchPoint);
                observation.CatchTapNoIntersectionOrOvershoot &=
                    leftDistance <= previousLeftDistance + 0.00001f &&
                    rightDistance <= previousRightDistance + 0.00001f;
                if (progress < MoonlightActivityStage.PlayCatchContactProgress)
                    observation.CatchTapNoIntersectionOrOvershoot &=
                        Vector3.Distance(leftPoint, rightPoint) > 0.0001f;
                else
                    observation.CatchTapMaximumHeldError = Mathf.Max(
                        observation.CatchTapMaximumHeldError,
                        Mathf.Max(leftDistance, rightDistance));
                previousLeftDistance = leftDistance;
                previousRightDistance = rightDistance;

                float legacyT = progress >= MoonlightActivityStage.PlayCatchContactProgress
                    ? 1f
                    : Mathf.SmoothStep(0f, 1f,
                        progress / MoonlightActivityStage.PlayCatchContactProgress);
                Vector3 legacyCenter = Vector3.Lerp(
                    new Vector3(0.48f, 1.12f, -0.18f),
                    MoonlightActivityStage.PlayCatchPoint, legacyT);
                Vector3 actualCenter = MoonlightActivityStage.EvaluatePlayTrajectory(
                    3, progress, centerTap);
                observation.CatchTapCenterMaximumDelta = Mathf.Max(
                    observation.CatchTapCenterMaximumDelta,
                    Vector3.Distance(actualCenter, legacyCenter));
            }

            observation.CatchTapMaximumContactError = Mathf.Max(
                Vector3.Distance(MoonlightActivityStage.EvaluatePlayTrajectory(3,
                    MoonlightActivityStage.PlayCatchContactProgress, leftTap),
                    MoonlightActivityStage.PlayCatchPoint),
                Vector3.Distance(MoonlightActivityStage.EvaluatePlayTrajectory(3,
                    MoonlightActivityStage.PlayCatchContactProgress, rightTap),
                    MoonlightActivityStage.PlayCatchPoint));
            observation.CatchTapOraclePassed = observation.CatchTapSamplesDistinct &&
                observation.CatchTapSamplesPerTrajectory == 41 &&
                observation.CatchTapFiniteAndBounded &&
                observation.CatchTapNoIntersectionOrOvershoot &&
                observation.CatchTapSeparationAt020 >= 0.40f &&
                observation.CatchTapMaximumContactError <= 0.0001f &&
                observation.CatchTapMaximumHeldError <= 0.0001f &&
                observation.CatchTapCenterMaximumDelta <= 0.0001f;
        }

        static MoonlightGestureSample PlayCatchTapSample(Vector2 position)
        {
            var points = new Vector2[MoonlightGestureSample.ResampledPointCount];
            for (int i = 0; i < points.Length; i++) points[i] = position;
            return MoonlightGestureSample.Create(0.95f, 0.12f, points);
        }

        static bool PlayCatchOraclePointIsFiniteAndBounded(Vector3 point) =>
            PlayCoordinateIsFinite(point.x) && PlayCoordinateIsFinite(point.y) &&
            PlayCoordinateIsFinite(point.z) && Mathf.Abs(point.x) <= 1.25f &&
            point.y >= 0.18f && point.y <= 1.65f && Mathf.Abs(point.z) <= 1.25f;

        IEnumerator ObservePlayContinuityTrajectory(MoonlightActionFeedback feedback,
            MoonlightActivityStage stage, PlayContinuityObservation observation)
        {
            while (feedback != null && feedback.IsPerformingAction)
            {
                CapturePlayContinuityPoint(stage, observation);
                yield return null;
            }
            CapturePlayContinuityPoint(stage, observation);
            observation.TrajectoryStepsObserved++;
        }

        IEnumerator ObservePlayFirstRenderedContinuationFrame(
            MoonlightActivityStage stage, PlayContinuityObservation observation,
            Vector3 heldEndpoint, int step)
        {
            if (Application.isBatchMode)
                yield return null;
            else
                yield return new WaitForEndOfFrame();
            float beginRenderDiscontinuity = stage != null
                ? Vector3.Distance(heldEndpoint, stage.PlayBallLocalPosition)
                : float.PositiveInfinity;
            yield return null;
            if (Application.isBatchMode)
                yield return null;
            else
                yield return new WaitForEndOfFrame();
            float firstSubsequentRenderDiscontinuity = stage != null
                ? Vector3.Distance(heldEndpoint, stage.PlayBallLocalPosition)
                : float.PositiveInfinity;
            float discontinuity = Mathf.Max(beginRenderDiscontinuity,
                firstSubsequentRenderDiscontinuity);
            int renderedMaterialCount = stage != null
                ? stage.ActiveUniqueMaterialCount
                : 0;
            observation.MaterialCounts[step] = renderedMaterialCount;
            observation.MaximumFirstRenderedFrameDiscontinuity = Mathf.Max(
                observation.MaximumFirstRenderedFrameDiscontinuity, discontinuity);
            bool preserved = stage != null && discontinuity <= 0.001f &&
                stage.PlayStageRootInstanceId == observation.RootId &&
                stage.PlayBallInstanceId == observation.BallId &&
                stage.PlayTrailInstanceId == observation.TrailId &&
                renderedMaterialCount == observation.InitialMaterialCount &&
                PlayMaterialsMatchObservation(stage, observation);
            if (preserved)
                observation.FirstRenderedContinuationFramesObserved++;
            else
                observation.IdentityPreserved = false;
        }

        IEnumerator ObservePlayHeldBusyRejection(MoonlightSpatialActionZone zone,
            MoonlightGesturePad pad, MoonlightCharacter moonlight,
            MoonlightActionFeedback feedback, MoonlightActivityStage stage,
            PlayContinuityObservation observation)
        {
            while (feedback != null && feedback.IsPerformingAction)
                yield return null;

            bool ready = feedback != null && feedback.IsCoolingDown &&
                !feedback.IsCameraFocusRequestedForQA && feedback.VisualPoseRestoredForQA &&
                stage != null && stage.IsHoldingPlayStepTerminal && !stage.IsLingering;
            var before = ready ? new PlayHeldSnapshot(zone, stage, moonlight) : default;
            bool accepted = pad != null && pad.SubmitSynthetic(zone.RequiredGesture, 0.95f);
            bool preserved = ready && !accepted &&
                PlayHeldStateMatches(before, zone, stage, moonlight);
            if (preserved)
            {
                if (!observation.HasInitialMaterialSnapshot)
                {
                    observation.InitialMaterialCount = stage.ActiveUniqueMaterialCount;
                    observation.MaterialCounts[0] = observation.InitialMaterialCount;
                    observation.BallMaterialId =
                        stage.PlayBallSharedMaterialInstanceId;
                    observation.TrailMaterialId =
                        stage.PlayTrailSharedMaterialInstanceId;
                    observation.HasInitialMaterialSnapshot =
                        observation.InitialMaterialCount > 0 &&
                        observation.BallMaterialId != 0 &&
                        observation.TrailMaterialId != 0;
                }
                observation.IdentityPreserved &=
                    PlayMaterialsMatchObservation(stage, observation);
                observation.RejectedPreservationCount++;
                observation.HeldEndpoint = stage.PlayBallLocalPosition;
                observation.HasHeldEndpoint = true;
                Debug.Log("[MoonlightGameplayQA][PASS] play-held-busy-rejection " +
                    $"progress={zone.ProgressStep} root={stage.PlayStageRootInstanceId} " +
                    $"ball={stage.PlayBallInstanceId} trail={stage.PlayTrailInstanceId} " +
                    $"materials={stage.ActiveUniqueMaterialCount}/" +
                    $"{stage.PlayBallSharedMaterialInstanceId}/" +
                    $"{stage.PlayTrailSharedMaterialInstanceId} " +
                    "marker=MOONLIGHT_PLAY_HELD_BUSY_PRESERVED");
            }
            else
            {
                observation.IdentityPreserved = false;
            }
        }

        static void CapturePlayContinuityPoint(MoonlightActivityStage stage,
            PlayContinuityObservation observation)
        {
            if (stage == null)
            {
                observation.TrajectoryFiniteAndBounded = false;
                return;
            }
            Vector3 point = stage.PlayBallLocalPosition;
            observation.TrajectorySampleCount++;
            observation.TrajectoryFiniteAndBounded &=
                PlayCoordinateIsFinite(point.x) && PlayCoordinateIsFinite(point.y) &&
                PlayCoordinateIsFinite(point.z) &&
                Mathf.Abs(point.x) <= 1.35f && point.y >= 0.15f && point.y <= 1.75f &&
                Mathf.Abs(point.z) <= 1.35f;
            observation.IdentityPreserved &=
                stage.PlayStageRootInstanceId == observation.RootId &&
                stage.PlayBallInstanceId == observation.BallId &&
                stage.PlayTrailInstanceId == observation.TrailId &&
                stage.AuthoritativePlayBallCount == 1 &&
                stage.AuthoritativePlayTrailCount == 1 &&
                (!observation.HasInitialMaterialSnapshot ||
                 PlayMaterialIdsMatchObservation(stage, observation)) &&
                CountActivePlayStageRoots() == 1;
            float clockDelta = stage.LastPlayContinuationClockDeltaForQA;
            observation.ContinuationClockSourceIsUnscaled &=
                string.Equals(MoonlightActivityStage.PlayContinuationClockSourceForQA,
                    "Time.unscaledDeltaTime", System.StringComparison.Ordinal) &&
                PlayCoordinateIsFinite(clockDelta) && clockDelta >= 0f &&
                clockDelta <= MoonlightActivityStage.PlayContinuationMaximumDeltaSeconds;
            observation.MaximumContinuationClockDelta = Mathf.Max(
                observation.MaximumContinuationClockDelta, clockDelta);
            observation.ContinuationClockAdvanced |= clockDelta > 0f;
        }

        static bool PlayHeldStateMatches(PlayHeldSnapshot before,
            MoonlightSpatialActionZone zone, MoonlightActivityStage stage,
            MoonlightCharacter moonlight) =>
            stage != null && stage.IsHoldingPlayStepTerminal && !stage.IsLingering &&
            stage.PlayStageRootInstanceId == before.RootId &&
            stage.PlayBallInstanceId == before.BallId &&
            stage.PlayTrailInstanceId == before.TrailId &&
            before.MaterialCount > 0 &&
            before.BallMaterialId != 0 && before.TrailMaterialId != 0 &&
            stage.ActiveUniqueMaterialCount == before.MaterialCount &&
            stage.PlayBallSharedMaterialInstanceId == before.BallMaterialId &&
            stage.PlayTrailSharedMaterialInstanceId == before.TrailMaterialId &&
            stage.AuthoritativePlayBallCount == 1 &&
            stage.AuthoritativePlayTrailCount == 1 &&
            CountActivePlayStageRoots() == 1 &&
            Vector3.Distance(stage.PlayBallLocalPosition, before.BallPosition) <= 0.000001f &&
            zone.ProgressStep == before.ProgressStep &&
            zone.ActivitySessionAcceptedSteps == before.AcceptedSteps &&
            Mathf.Approximately(zone.ActivitySessionAverageScore, before.AverageScore) &&
            zone.ActivityCurrentCombo == before.CurrentCombo &&
            zone.ActivityBestCombo == before.BestCombo &&
            zone.ActivityPerfectSteps == before.PerfectSteps &&
            PlayRewardsEqual(before.Rewards, new PlayRewardSnapshot(moonlight));

        static bool PlayMaterialsMatchObservation(MoonlightActivityStage stage,
            PlayContinuityObservation observation) =>
            observation.HasInitialMaterialSnapshot && stage != null &&
            stage.ActiveUniqueMaterialCount == observation.InitialMaterialCount &&
            PlayMaterialIdsMatchObservation(stage, observation);

        static bool PlayMaterialIdsMatchObservation(MoonlightActivityStage stage,
            PlayContinuityObservation observation) =>
            observation.HasInitialMaterialSnapshot && stage != null &&
            stage.PlayBallSharedMaterialInstanceId == observation.BallMaterialId &&
            stage.PlayTrailSharedMaterialInstanceId == observation.TrailMaterialId;

        static bool PlayRewardsEqual(PlayRewardSnapshot left, PlayRewardSnapshot right) =>
            Mathf.Approximately(left.Wonder, right.Wonder) &&
            Mathf.Approximately(left.Warmth, right.Warmth) &&
            Mathf.Approximately(left.Rest, right.Rest) &&
            Mathf.Approximately(left.Magic, right.Magic) &&
            Mathf.Approximately(left.Hunger, right.Hunger) &&
            left.XP == right.XP && left.Coins == right.Coins;

        static bool PlayRewardDeltaMatches(PlayRewardSnapshot before,
            PlayRewardSnapshot after, float wonder, float warmth, float rest,
            float magic, float hunger, int xp, int coins) =>
            Mathf.Approximately(after.Wonder - before.Wonder, wonder) &&
            Mathf.Approximately(after.Warmth - before.Warmth, warmth) &&
            Mathf.Approximately(after.Rest - before.Rest, rest) &&
            Mathf.Approximately(after.Magic - before.Magic, magic) &&
            Mathf.Approximately(after.Hunger - before.Hunger, hunger) &&
            after.XP - before.XP == xp && after.Coins - before.Coins == coins;

        static int CountActivePlayStageRoots() =>
            FindObjectsByType<Transform>(FindObjectsSortMode.None)
                .Count(candidate => candidate != null &&
                    candidate.name == "ActivityStage-Play");

        static int CountActiveCookStageRoots() =>
            FindObjectsByType<Transform>(FindObjectsSortMode.None)
                .Count(candidate => candidate != null &&
                    candidate.name == "ActivityStage-Cook");

        static bool PlayCoordinateIsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);

        IEnumerator RunGestureResponsiveCareLiveHarness(MoonlightCharacter liveMoonlight,
            MoonlightSpatialInteractor liveInteractor, MoonlightUI ui,
            MoonlightGesturePad pad, MoonlightSpatialActionZone liveZone,
            CareLiveHarnessResult result)
        {
            MoonlightGameManager manager = MoonlightGameManager.Instance;
            if (manager == null || liveMoonlight == null || liveInteractor == null ||
                ui == null || pad == null || liveZone == null || EventSystem.current == null)
            {
                result.Detail = $"prerequisites manager={(manager != null)} " +
                    $"moonlight={(liveMoonlight != null)} interactor={(liveInteractor != null)} " +
                    $"ui={(ui != null)} pad={(pad != null)} zone={(liveZone != null)} " +
                    $"eventSystem={(EventSystem.current != null)}";
                yield break;
            }

            MoonlightCharacter originalManagedMoonlight = manager.moonlight;
            bool liveZoneWasEnabled = liveZone.enabled;
            float liveWonder = liveMoonlight.stats.wonder;
            float liveWarmth = liveMoonlight.stats.warmth;
            float liveRest = liveMoonlight.stats.rest;
            float liveMagic = liveMoonlight.stats.magic;
            float liveHunger = liveMoonlight.stats.hunger;
            float liveDaysInHouse = liveMoonlight.daysInHouse;
            MoonlightStage liveStage = liveMoonlight.stage;
            int liveXP = liveMoonlight.xp;
            int liveCoins = liveMoonlight.coins;
            int liveRoomsUnlocked = liveMoonlight.roomsUnlocked;
            CareLiveStationSnapshot[] liveCareStationSnapshots =
                FindObjectsByType<MoonlightActivityStation>(
                        FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .Where(station => station.Kind == MoonlightSpatialActionKind.Care)
                    .OrderBy(station => station.GetInstanceID())
                    .Select(station => new CareLiveStationSnapshot(station))
                    .ToArray();
            GameObject harnessRoot = null;
            MoonlightUI.QATransactionSnapshot uiSnapshot = null;
            MoonlightGesturePad.QATransactionSnapshot padSnapshot = null;
            var context = new CareLiveHarnessContext();
            try
            {
                if (!ui.TryBeginQATransaction(out uiSnapshot,
                        out string uiTransactionDetail))
                {
                    result.Detail = $"ui-transaction-begin ({uiTransactionDetail})";
                    yield break;
                }
                if (!pad.TryBeginQATransaction(out padSnapshot,
                        out string padTransactionDetail))
                {
                    result.Detail = $"pad-transaction-begin ({padTransactionDetail})";
                    yield break;
                }

                harnessRoot = new GameObject("MoonlightCareGestureLiveHarness-Isolated");
                harnessRoot.transform.position = liveMoonlight.transform.position;
                var probeMoonlight = harnessRoot.AddComponent<MoonlightCharacter>();
                var probeInteractor = harnessRoot.AddComponent<MoonlightSpatialInteractor>();
                context.Stage = harnessRoot.AddComponent<MoonlightActivityStage>();
                if (!context.Stage.ConfigureCareLiveHarnessIsolationForQA(true))
                {
                    result.Detail = "stage-persistent-isolation-config-failed";
                    yield break;
                }
                context.Feedback = harnessRoot.AddComponent<MoonlightActionFeedback>();
                manager.moonlight = probeMoonlight;
                liveZone.enabled = false;

                var tapLeft = new CareLiveProbeObservation();
                var tapRight = new CareLiveProbeObservation();
                var circleNarrow = new CareLiveProbeObservation();
                var circleWide = new CareLiveProbeObservation();
                var circleReverse = new CareLiveProbeObservation();
                var swipeRight = new CareLiveProbeObservation();
                var swipeLeft = new CareLiveProbeObservation();
                var glowScoreLow = new CareLiveProbeObservation();
                var glowScoreHigh = new CareLiveProbeObservation();
                var glowDurationShort = new CareLiveProbeObservation();
                var glowDurationLong = new CareLiveProbeObservation();

                yield return RunCareLiveProbe(probeMoonlight, probeInteractor, context,
                    ui, pad, new CareLiveProbeSpec("tap-left", 0,
                        new[] { new Vector2(-0.38f, 0f) }), tapLeft);
                yield return RunCareLiveProbe(probeMoonlight, probeInteractor, context,
                    ui, pad, new CareLiveProbeSpec("tap-right", 0,
                        new[] { new Vector2(0.38f, 0f) }), tapRight);
                yield return RunCareLiveProbe(probeMoonlight, probeInteractor, context,
                    ui, pad, new CareLiveProbeSpec("circle-narrow", 1,
                        CareCirclePointerPoints(0.16f, false)), circleNarrow);
                yield return RunCareLiveProbe(probeMoonlight, probeInteractor, context,
                    ui, pad, new CareLiveProbeSpec("circle-wide", 1,
                        CareCirclePointerPoints(0.42f, false)), circleWide);
                yield return RunCareLiveProbe(probeMoonlight, probeInteractor, context,
                    ui, pad, new CareLiveProbeSpec("circle-reverse", 1,
                        CareCirclePointerPoints(0.42f, true)), circleReverse);
                yield return RunCareLiveProbe(probeMoonlight, probeInteractor, context,
                    ui, pad, new CareLiveProbeSpec("swipe-right", 2,
                        new[] { new Vector2(-0.42f, 0f), new Vector2(0.42f, 0f) }),
                    swipeRight);
                yield return RunCareLiveProbe(probeMoonlight, probeInteractor, context,
                    ui, pad, new CareLiveProbeSpec("swipe-left", 2,
                        new[] { new Vector2(0.42f, 0f), new Vector2(-0.42f, 0f) }),
                    swipeLeft);
                yield return RunCareLiveProbe(probeMoonlight, probeInteractor, context,
                    ui, pad, new CareLiveProbeSpec("glow-score-low", 3,
                        new[] { Vector2.zero, new Vector2(0.122f, 0f) }, 1f),
                    glowScoreLow);
                yield return RunCareLiveProbe(probeMoonlight, probeInteractor, context,
                    ui, pad, new CareLiveProbeSpec("glow-score-high", 3,
                        new[] { Vector2.zero }, 1f), glowScoreHigh);
                yield return RunCareLiveProbe(probeMoonlight, probeInteractor, context,
                    ui, pad, new CareLiveProbeSpec("glow-duration-short", 3,
                        new[] { Vector2.zero }, 0.80f), glowDurationShort);
                yield return RunCareLiveProbe(probeMoonlight, probeInteractor, context,
                    ui, pad, new CareLiveProbeSpec("glow-duration-long", 3,
                        new[] { Vector2.zero, new Vector2(0.124f, 0f) }, 1.10f, true),
                    glowDurationLong);

                CareLiveProbeObservation[] observations =
                {
                    tapLeft, tapRight, circleNarrow, circleWide, circleReverse,
                    swipeRight, swipeLeft, glowScoreLow, glowScoreHigh,
                    glowDurationShort, glowDurationLong
                };
                bool probesPassed = observations.All(observation => observation.Passed);
                bool budgetPass = observations.All(observation =>
                    observation.RendererCount > 0 && observation.RendererCount <= 48 &&
                    observation.MaterialCount > 0 && observation.MaterialCount <= 28 &&
                    observation.LightCount == 1);
                bool persistentIsolationPass = observations.All(observation =>
                    observation.PersistentIsolationPassed);
                bool finalLingerPass = glowDurationLong.FinalLingerPassed;

                float tapLandingDelta = Vector3.Distance(tapLeft.PrimaryB, tapRight.PrimaryB);
                bool tapPass = tapLandingDelta >=
                    MoonlightActivityStage.CarePrepMinimumLandingSeparation;

                float narrowBrushCross = CareObservedOrbitCross(
                    circleNarrow.PrimaryA, circleNarrow.PrimaryB);
                float wideBrushCross = CareObservedOrbitCross(
                    circleWide.PrimaryA, circleWide.PrimaryB);
                float reverseBrushCross = CareObservedOrbitCross(
                    circleReverse.PrimaryA, circleReverse.PrimaryB);
                float wideBubbleCross = CareObservedOrbitCross(
                    circleWide.BubbleA, circleWide.BubbleB);
                float reverseBubbleCross = CareObservedOrbitCross(
                    circleReverse.BubbleA, circleReverse.BubbleB);
                float narrowRadius = Mathf.Abs(circleNarrow.PrimaryA.x);
                float wideRadius = Mathf.Abs(circleWide.PrimaryA.x);
                float observedRadiusDelta = wideRadius - narrowRadius;
                bool circlePass = Mathf.Abs(narrowBrushCross) >= 0.001f &&
                    Mathf.Abs(wideBrushCross) >= 0.001f &&
                    Mathf.Abs(reverseBrushCross) >= 0.001f &&
                    Mathf.Sign(narrowBrushCross) == Mathf.Sign(wideBrushCross) &&
                    Mathf.Sign(wideBrushCross) == -Mathf.Sign(reverseBrushCross) &&
                    Mathf.Abs(wideBubbleCross) >= 0.001f &&
                    Mathf.Abs(reverseBubbleCross) >= 0.001f &&
                    Mathf.Sign(wideBubbleCross) == -Mathf.Sign(reverseBubbleCross) &&
                    observedRadiusDelta >= MoonlightActivityStage.CareWashMinimumRadiusDelta;

                float rightTraversal = swipeRight.PrimaryB.x - swipeRight.PrimaryA.x;
                float leftTraversal = swipeLeft.PrimaryB.x - swipeLeft.PrimaryA.x;
                float swipeEndpointDelta = Vector3.Distance(
                    swipeRight.PrimaryB, swipeLeft.PrimaryB);
                bool swipePass = rightTraversal * leftTraversal < 0f &&
                    Mathf.Abs(rightTraversal) >=
                        MoonlightActivityStage.CareBrushMinimumEndpointSeparation &&
                    Mathf.Abs(leftTraversal) >=
                        MoonlightActivityStage.CareBrushMinimumEndpointSeparation &&
                    swipeEndpointDelta >=
                        MoonlightActivityStage.CareBrushMinimumEndpointSeparation;

                float scoreDurationDrift = Mathf.Abs(
                    glowScoreHigh.Sample.Duration - glowScoreLow.Sample.Duration);
                float scoreAuraDelta = glowScoreHigh.AuraScale.x - glowScoreLow.AuraScale.x;
                float scoreLightDelta = glowScoreHigh.LightIntensity -
                    glowScoreLow.LightIntensity;
                int scoreMoteDelta = glowScoreHigh.MoteCount - glowScoreLow.MoteCount;
                bool glowScorePass = glowScoreHigh.Sample.Score >=
                        glowScoreLow.Sample.Score + 0.20f &&
                    scoreDurationDrift <= 0.06f && scoreAuraDelta >= 0.04f &&
                    scoreLightDelta >= 0.08f && scoreMoteDelta >= 1;

                float durationScoreDrift = Mathf.Abs(
                    glowDurationLong.Sample.Score - glowDurationShort.Sample.Score);
                float durationAuraDelta = glowDurationLong.AuraScale.x -
                    glowDurationShort.AuraScale.x;
                float durationLightDelta = glowDurationLong.LightIntensity -
                    glowDurationShort.LightIntensity;
                int durationMoteDelta = glowDurationLong.MoteCount -
                    glowDurationShort.MoteCount;
                bool glowDurationPass = glowDurationLong.Sample.Duration >=
                        glowDurationShort.Sample.Duration + 0.20f &&
                    durationScoreDrift <= 0.06f && durationAuraDelta >= 0.02f &&
                    durationLightDelta >= 0.04f && durationMoteDelta >= 1;

                result.Passed = probesPassed && budgetPass && persistentIsolationPass &&
                    finalLingerPass && tapPass && circlePass && swipePass &&
                    glowScorePass && glowDurationPass;
                result.Detail = $"probes={observations.Count(observation => observation.Passed)}/" +
                    $"{observations.Length} propagation={probesPassed} " +
                    $"tapLandingDelta={tapLandingDelta:0.000} " +
                    $"brushOrbit={wideBrushCross:0.0000}/{reverseBrushCross:0.0000} " +
                    $"bubbleOrbit={wideBubbleCross:0.0000}/{reverseBubbleCross:0.0000} " +
                    $"radius={narrowRadius:0.000}/{wideRadius:0.000} " +
                    $"delta={observedRadiusDelta:0.000} " +
                    $"swipeTraversal={rightTraversal:0.000}/{leftTraversal:0.000} " +
                    $"endpoints={swipeEndpointDelta:0.000} " +
                    $"scoreFixedDuration={glowScoreLow.Sample.Score:0.000}->" +
                    $"{glowScoreHigh.Sample.Score:0.000}@" +
                    $"{glowScoreLow.Sample.Duration:0.000}/" +
                    $"{glowScoreHigh.Sample.Duration:0.000}s " +
                    $"deltas={scoreAuraDelta:0.000}/{scoreLightDelta:0.000}/" +
                    $"{scoreMoteDelta} durationFixedScore=" +
                    $"{glowDurationShort.Sample.Duration:0.000}->" +
                    $"{glowDurationLong.Sample.Duration:0.000}s@" +
                    $"{glowDurationShort.Sample.Score:0.000}/" +
                    $"{glowDurationLong.Sample.Score:0.000} deltas=" +
                    $"{durationAuraDelta:0.000}/{durationLightDelta:0.000}/" +
                    $"{durationMoteDelta} contracts={tapPass}/{circlePass}/" +
                    $"{swipePass}/{glowScorePass}/{glowDurationPass} budgets={budgetPass} " +
                    $"persistentIsolation={persistentIsolationPass} " +
                    $"finalLinger=({glowDurationLong.FinalLingerDetail})";
                if (!probesPassed)
                {
                    string failed = string.Join(",", observations
                        .Where(observation => !observation.Passed)
                        .Select(observation => observation.Detail));
                    result.Detail += $" failed=({failed})";
                }
                probeInteractor.RescanNowForQA();
                yield return null;
            }
            finally
            {
                if (context.Feedback != null && context.Feedback.enabled)
                {
                    context.Feedback.enabled = false;
                }
                manager.moonlight = originalManagedMoonlight;
                liveZone.enabled = liveZoneWasEnabled;
                liveMoonlight.stats.wonder = liveWonder;
                liveMoonlight.stats.warmth = liveWarmth;
                liveMoonlight.stats.rest = liveRest;
                liveMoonlight.stats.magic = liveMagic;
                liveMoonlight.stats.hunger = liveHunger;
                liveMoonlight.daysInHouse = liveDaysInHouse;
                liveMoonlight.stage = liveStage;
                liveMoonlight.xp = liveXP;
                liveMoonlight.coins = liveCoins;
                liveMoonlight.roomsUnlocked = liveRoomsUnlocked;
                if (harnessRoot != null)
                {
                    harnessRoot.SetActive(false);
                    Destroy(harnessRoot);
                }
                liveInteractor.RescanNowForQA();
                var stationStateDetails = new List<string>();
                bool liveStationStateUnchanged = liveCareStationSnapshots.Length > 0;
                foreach (CareLiveStationSnapshot stationSnapshot in liveCareStationSnapshots)
                {
                    bool stationMatches = stationSnapshot.Matches(out string stationDetail);
                    liveStationStateUnchanged &= stationMatches;
                    stationStateDetails.Add(stationDetail);
                }
                result.Passed &= liveStationStateUnchanged;
                result.Detail += $" liveCareStations={liveCareStationSnapshots.Length} " +
                    $"unchanged={liveStationStateUnchanged} " +
                    $"stationState=({string.Join(";", stationStateDetails)})";
                string padRestoreDetail = "not-started";
                string uiRestoreDetail = "not-started";
                bool padRestored = padSnapshot == null;
                bool uiRestored = uiSnapshot == null;
                try
                {
                    if (padSnapshot != null)
                        padRestored = pad.RestoreQATransaction(
                            padSnapshot, out padRestoreDetail);
                }
                catch (System.Exception exception)
                {
                    padRestoreDetail = $"exception:{exception.GetType().Name}";
                }
                try
                {
                    if (uiSnapshot != null)
                        uiRestored = ui.RestoreQATransaction(
                            uiSnapshot, out uiRestoreDetail);
                }
                catch (System.Exception exception)
                {
                    uiRestoreDetail = $"exception:{exception.GetType().Name}";
                }
                if (!padRestored || !uiRestored)
                {
                    result.Passed = false;
                    result.Detail += $" restore=pad:{padRestored}" +
                        $"({padRestoreDetail})/ui:{uiRestored}({uiRestoreDetail})";
                }
                else if (padSnapshot != null || uiSnapshot != null)
                {
                    result.Detail += " restore=transactional";
                }
            }
        }

        IEnumerator RunCareLiveProbe(MoonlightCharacter probeMoonlight,
            MoonlightSpatialInteractor probeInteractor, CareLiveHarnessContext context,
            MoonlightUI ui, MoonlightGesturePad pad, CareLiveProbeSpec spec,
            CareLiveProbeObservation observation)
        {
            GameObject zoneRoot = null;
            try
            {
                zoneRoot = new GameObject($"CareLiveProbeZone-{spec.Name}");
                zoneRoot.transform.position = probeMoonlight.transform.position;
                var zone = zoneRoot.AddComponent<MoonlightSpatialActionZone>();
                zone.Configure(MoonlightSpatialActionKind.Care,
                    $"Care live probe {spec.Name}", 0.30f);
                probeInteractor.RescanNowForQA();
                ui.Refresh(probeMoonlight);
                if (probeInteractor.CurrentZone != zone)
                {
                    observation.Detail = $"{spec.Name}:zone-not-current";
                    yield break;
                }

                if (context.Feedback == null || !context.Feedback.CanBeginAction)
                {
                    observation.Detail = $"{spec.Name}:feedback-not-ready";
                    yield break;
                }

                for (int step = 0; step <= spec.Step; step++)
                {
                    MoonlightGestureKind gesture =
                        MoonlightSpatialActionZone.CareGestureForStep(step);
                    Vector2[] points = step == spec.Step
                        ? spec.Points
                        : CareLeadInPointerPoints(gesture);
                    float holdSeconds = step == spec.Step
                        ? spec.HoldSeconds
                        : 0f;
                    yield return DriveCarePadPointerInput(pad, points, holdSeconds,
                        8200 + spec.Step * 20 + step);

                    var stage = context.Stage;
                    MoonlightActionFeedback feedback = context.Feedback;
                    bool persistentIsolation = stage != null &&
                        stage.CareLiveHarnessIsolationEnabledForQA &&
                        !stage.HasPersistentStationBindingForQA &&
                        stage.UsesProceduralCareStationFallback &&
                        stage.CareStationVisualSource == "stage-procedural-fallback" &&
                        stage.CareLiveHarnessIsolationQAMarker ==
                            "MOONLIGHT_CARE_LIVE_HARNESS_PERSISTENT_ISOLATED";
                    bool propagationPass = zone.LastGesturePassed &&
                        stage != null && stage.IsVisible &&
                        feedback.ActivityStep == step && stage.CurrentStep == step &&
                        pad.LastSample.ContentEquals(zone.LastGestureSample) &&
                        zone.LastGestureSample.ContentEquals(feedback.ActiveGestureSample) &&
                        feedback.ActiveGestureSample.ContentEquals(stage.ActiveGestureSample) &&
                        persistentIsolation;
                    if (!propagationPass)
                    {
                        observation.Detail = $"{spec.Name}:propagation step={step + 1} " +
                            $"passed={zone.LastGesturePassed} stage={(stage != null && stage.IsVisible)} " +
                            $"persistentIsolation={persistentIsolation} " +
                            $"pad={pad.LastSample.PointCount} zone={zone.LastGestureSample.PointCount} " +
                            $"feedback={feedback.ActiveGestureSample.PointCount}";
                        yield break;
                    }

                    if (step == spec.Step)
                    {
                        observation.Sample = stage.ActiveGestureSample;
                        CaptureCareLiveObservation(stage, spec.Step, observation);
                        observation.RendererCount = stage.ActiveRendererCount;
                        observation.MaterialCount = stage.ActiveUniqueMaterialCount;
                        observation.LightCount = stage.ActiveLightCount;
                        observation.PersistentIsolationPassed = persistentIsolation;
                        observation.Passed = stage.CareGestureSampleReady &&
                            stage.CareCurrentStepTransformAgreement &&
                            observation.PersistentIsolationPassed;
                        observation.Detail = observation.Passed
                            ? spec.Name
                            : $"{spec.Name}:transform {stage.CareTransformEvidence}";
                    }

                    if (step == spec.Step && spec.MeasureFinalLinger)
                    {
                        float enterDeadline = Time.time + 4f;
                        while (!stage.IsLingering && Time.time < enterDeadline)
                            yield return null;
                        bool enteredLinger = stage.IsLingering &&
                            feedback.IsPresentingResult &&
                            stage.CurrentKind == MoonlightSpatialActionKind.Care &&
                            stage.CurrentStep == 3 &&
                            stage.CareLiveHarnessIsolationQAMarker ==
                                "MOONLIGHT_CARE_LIVE_HARNESS_PERSISTENT_ISOLATED";
                        float endDeadline = Time.time +
                            MoonlightActivityStage.CareFinalPresentationSeconds + 0.75f;
                        while (stage.IsLingering && Time.time < endDeadline)
                            yield return null;
                        string lingerDetail = "linger-timeout";
                        bool lingerContract = !stage.IsLingering &&
                            stage.ValidateLastCareLingerRuntimeContract(0.20f,
                                out lingerDetail);
                        observation.FinalLingerPassed = enteredLinger && lingerContract;
                        observation.FinalLingerDetail = $"entered={enteredLinger} " +
                            lingerDetail;
                        observation.Passed &= observation.FinalLingerPassed;
                        observation.Detail = observation.Passed
                            ? $"{spec.Name}:linger ({observation.FinalLingerDetail})"
                            : $"{spec.Name}:linger-failed ({observation.FinalLingerDetail})";
                    }

                    yield return ReplaceCareProbeFeedback(probeMoonlight, context);
                    if (step < spec.Step &&
                        (context.Feedback == null || !context.Feedback.CanBeginAction))
                    {
                        observation.Passed = false;
                        observation.Detail = $"{spec.Name}:cooldown step={step + 1}";
                        yield break;
                    }
                }
            }
            finally
            {
                if (context.Feedback != null && context.Feedback.enabled &&
                    (context.Feedback.IsPerformingAction ||
                     context.Feedback.IsPresentingResult))
                    context.Feedback.enabled = false;
                if (zoneRoot != null)
                {
                    zoneRoot.SetActive(false);
                    Destroy(zoneRoot);
                }
                probeInteractor.RescanNowForQA();
            }
        }

        static IEnumerator ReplaceCareProbeFeedback(MoonlightCharacter probeMoonlight,
            CareLiveHarnessContext context)
        {
            MoonlightActionFeedback previous = context.Feedback;
            context.Feedback = null;
            if (previous != null)
            {
                previous.enabled = false;
                Destroy(previous);
                yield return null;
            }
            context.Feedback = probeMoonlight.gameObject.AddComponent<MoonlightActionFeedback>();
        }

        static void CaptureCareLiveObservation(MoonlightActivityStage stage, int step,
            CareLiveProbeObservation observation)
        {
            if (step == 0)
            {
                stage.UpdateStage(MoonlightSpatialActionKind.Care, 1f);
                observation.PrimaryB = stage.CareActualGesturePropLocalPosition;
                return;
            }
            if (step == 1)
            {
                stage.UpdateStage(MoonlightSpatialActionKind.Care, 0f);
                observation.PrimaryA = stage.CareActualGesturePropLocalPosition;
                observation.BubbleA = stage.CareActualPrimaryBubbleLocalPosition;
                stage.UpdateStage(MoonlightSpatialActionKind.Care, 0.05f);
                observation.PrimaryB = stage.CareActualGesturePropLocalPosition;
                observation.BubbleB = stage.CareActualPrimaryBubbleLocalPosition;
                return;
            }
            if (step == 2)
            {
                stage.UpdateStage(MoonlightSpatialActionKind.Care, 0f);
                observation.PrimaryA = stage.CareActualGesturePropLocalPosition;
                stage.UpdateStage(MoonlightSpatialActionKind.Care, 1f);
                observation.PrimaryB = stage.CareActualGesturePropLocalPosition;
                return;
            }

            stage.UpdateStage(MoonlightSpatialActionKind.Care, 1f);
            observation.AuraScale = stage.CareActualGlowAuraScale;
            observation.LightIntensity = stage.CareActualGlowLightIntensity;
            observation.MoteCount = stage.CareActualGlowMoteCount;
        }

        static IEnumerator DriveCarePadPointerInput(MoonlightGesturePad pad,
            Vector2[] normalizedPoints, float holdSeconds, int pointerId)
        {
            var rect = pad.transform as RectTransform;
            if (rect == null || normalizedPoints == null || normalizedPoints.Length == 0)
                yield break;
            float referenceSize = Mathf.Max(1f,
                Mathf.Min(Mathf.Abs(rect.rect.width), Mathf.Abs(rect.rect.height)));
            var pointer = new PointerEventData(EventSystem.current)
            {
                pointerId = pointerId,
                button = PointerEventData.InputButton.Left,
                position = CarePadScreenPoint(rect, normalizedPoints[0], referenceSize)
            };
            var raycaster = rect.GetComponentInParent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster != null)
                pointer.pointerPressRaycast = new RaycastResult { module = raycaster };
            pad.OnPointerDown(pointer);
            for (int i = 1; i < normalizedPoints.Length; i++)
            {
                pointer.position = CarePadScreenPoint(rect, normalizedPoints[i], referenceSize);
                pad.OnDrag(pointer);
            }
            if (holdSeconds > 0f)
                yield return new WaitForSecondsRealtime(holdSeconds);
            pad.OnPointerUp(pointer);
        }

        static Vector2 CarePadScreenPoint(RectTransform rect, Vector2 normalizedPoint,
            float referenceSize)
        {
            Vector3 world = rect.TransformPoint(normalizedPoint * referenceSize);
            Canvas canvas = rect.GetComponentInParent<Canvas>();
            Camera eventCamera = canvas != null &&
                canvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? canvas.worldCamera
                    : null;
            return RectTransformUtility.WorldToScreenPoint(eventCamera, world);
        }

        static Vector2[] CareCirclePointerPoints(float radius, bool reverse)
        {
            var points = new Vector2[17];
            for (int i = 0; i < points.Length; i++)
            {
                int source = reverse ? points.Length - 1 - i : i;
                float angle = source / (float)(points.Length - 1) * Mathf.PI * 2f;
                points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
            return points;
        }

        static Vector2[] CareLeadInPointerPoints(MoonlightGestureKind gesture) =>
            gesture switch
            {
                MoonlightGestureKind.Tap => new[] { Vector2.zero },
                MoonlightGestureKind.Circle => CareCirclePointerPoints(0.32f, false),
                MoonlightGestureKind.Swipe => new[]
                {
                    new Vector2(-0.42f, 0f), new Vector2(0.42f, 0f)
                },
                _ => new[] { Vector2.zero }
            };

        static float CareObservedOrbitCross(Vector3 first, Vector3 second)
        {
            const float centerZ = 0.02f;
            return first.x * (second.z - centerZ) -
                second.x * (first.z - centerZ);
        }

        static bool Approximately(float actual, float expected)
            => Mathf.Abs(actual - expected) <= 0.01f;

        public static bool ValidateMoonbudLocomotionSourceContract(out string detail)
        {
            bool animatorPass = MoonlightAnimator.ValidateProceduralLocomotionSourceContract(
                out string animatorDetail);
            bool coordinatedRootPass =
                MoonlightPlayerController.ProceduralWholeRootBobScale == 0f &&
                MoonlightPlayerController.ProceduralWholeRootSquashScale == 0f;
            detail = $"{animatorDetail} " +
                $"rootBobScale={MoonlightPlayerController.ProceduralWholeRootBobScale:0.00} " +
                $"rootSquashScale={MoonlightPlayerController.ProceduralWholeRootSquashScale:0.00}";
            return animatorPass && coordinatedRootPass;
        }

        public static bool ValidateMoonbudLocomotionRuntimeContract(
            MoonlightPlayerController controller, out string detail)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            bool photorealMode = IsPhotorealMode(args);
            return ValidateMoonbudLocomotionRuntimeContract(controller, photorealMode,
                out detail, out _);
        }

        static bool IsPhotorealMode(string[] args) => System.Array.Exists(args,
            argument => string.Equals(argument, "-moonlightPhotoreal",
                System.StringComparison.OrdinalIgnoreCase));

        public static bool ValidateMoonbudLocomotionRuntimeContract(
            MoonlightPlayerController controller, bool photorealMode, out string detail,
            out string marker)
        {
            if (controller == null)
            {
                marker = "MOONLIGHT_LOCOMOTION_RUNTIME_INCOMPLETE";
                detail = "path=missing controller=False";
                return false;
            }

            if (photorealMode)
            {
                MoonlightKidAnimatorQACandidates.Clear();
                controller.GetComponentsInChildren(true, MoonlightKidAnimatorQACandidates);
                int activeSpecialists = 0;
                MoonlightKidAnimator soleSpecialist = null;
                for (int i = 0; i < MoonlightKidAnimatorQACandidates.Count; i++)
                {
                    MoonlightKidAnimator specialist = MoonlightKidAnimatorQACandidates[i];
                    if (specialist == null || !specialist.isActiveAndEnabled ||
                        !specialist.gameObject.activeInHierarchy)
                        continue;
                    activeSpecialists++;
                    soleSpecialist = specialist;
                }

                int specialistControllerActiveCount = controller.ActiveMoonlightKidAnimatorCount;
                MoonlightKidAnimator routedSpecialist = controller.ActiveMoonlightKidAnimator;
                bool specialistControllerRouted = activeSpecialists == 1 &&
                    specialistControllerActiveCount == activeSpecialists &&
                    routedSpecialist == soleSpecialist;
                if (!specialistControllerRouted)
                {
                    marker = MoonlightKidAnimator.ObservedLocomotionIncompleteMarker;
                    detail = $"path=photoreal-specialist activeSpecialists=" +
                        $"{activeSpecialists} controllerActiveSpecialists=" +
                        $"{specialistControllerActiveCount} controllerRouted={specialistControllerRouted} " +
                        "intentionalPhotoreal=True specialist=non-unique-or-unrouted";
                    return false;
                }

                bool observedPass = soleSpecialist.ValidateObservedLocomotionRuntimeContract(
                    out string specialistDetail);
                bool pass = observedPass &&
                    soleSpecialist.ObservedLocomotionQAMarker ==
                        MoonlightKidAnimator.ObservedLocomotionReadyMarker;
                marker = pass
                    ? MoonbudPhotorealSpecialistLocomotionMarker
                    : MoonlightKidAnimator.ObservedLocomotionIncompleteMarker;
                detail = $"path=photoreal-specialist activeSpecialists={activeSpecialists} " +
                    $"controllerActiveSpecialists={specialistControllerActiveCount} " +
                    $"controllerRouted={specialistControllerRouted} {specialistDetail}";
                return pass;
            }

            MoonlightAnimatorQACandidates.Clear();
            controller.GetComponentsInChildren(true, MoonlightAnimatorQACandidates);
            int activeAnimators = 0;
            MoonlightAnimator soleAnimator = null;
            for (int i = 0; i < MoonlightAnimatorQACandidates.Count; i++)
            {
                MoonlightAnimator animator = MoonlightAnimatorQACandidates[i];
                if (animator == null || !animator.isActiveAndEnabled ||
                    !animator.gameObject.activeInHierarchy)
                    continue;
                activeAnimators++;
                soleAnimator = animator;
            }

            int controllerActiveCount = controller.ActiveMoonlightAnimatorCount;
            MoonlightAnimator routedAnimator = controller.ActiveMoonlightAnimator;
            bool controllerRouted = activeAnimators == 1 &&
                controllerActiveCount == activeAnimators && routedAnimator == soleAnimator;
            if (!controllerRouted)
            {
                marker = "MOONLIGHT_LOCOMOTION_RUNTIME_INCOMPLETE";
                detail = $"path=authored activeAnimators={activeAnimators} " +
                    $"controllerActiveAnimators={controllerActiveCount} " +
                    $"controllerRouted={controllerRouted} animator=non-unique-or-unrouted";
                return false;
            }

            bool animatorPass = soleAnimator.ValidateProceduralLocomotionRuntimeContract(
                out string animatorDetail);
            if (soleAnimator.HasRuntimeAnimatorController)
            {
                marker = MoonbudAnimatorControllerIncompleteMarker;
                detail = $"path=animator-controller activeAnimators={activeAnimators} " +
                    $"controllerActiveAnimators={controllerActiveCount} " +
                    $"controllerRouted={controllerRouted} {animatorDetail}";
                return false;
            }

            bool proceduralPass = animatorPass && soleAnimator.UsesProceduralLocomotion &&
                soleAnimator.LiveProceduralRigBindingValid &&
                soleAnimator.ActiveVisibleArticulatedBindingCount >=
                    MoonlightAnimator.MinimumArticulatedTransformCount;
            marker = proceduralPass
                ? MoonbudArticulatedLocomotionMarker
                : "MOONLIGHT_LOCOMOTION_RUNTIME_INCOMPLETE";
            detail = $"path=controllerless-procedural failClosed=True " +
                $"activeAnimators={activeAnimators} " +
                $"controllerActiveAnimators={controllerActiveCount} " +
                $"controllerRouted={controllerRouted} activeVisibleBindings=" +
                $"{soleAnimator.ActiveVisibleArticulatedBindingCount}/" +
                $">={MoonlightAnimator.MinimumArticulatedTransformCount} " +
                $"bindingFailClosed={soleAnimator.ProceduralRigBindingFailedClosed} " +
                animatorDetail;
            return proceduralPass;
        }

        static bool ValidateActionAccent(MoonlightActionFeedback feedback,
            string expectedSignature, string expectedMarker, bool requireContactCenter,
            out string detail)
        {
            Vector3 bounds = feedback != null ? feedback.ActionAccentBoundsSize : Vector3.zero;
            bool finiteBounds = !float.IsNaN(bounds.x) && !float.IsNaN(bounds.y) &&
                !float.IsNaN(bounds.z) && !float.IsInfinity(bounds.x) &&
                !float.IsInfinity(bounds.y) && !float.IsInfinity(bounds.z);
            float extent = feedback != null ? feedback.ActionAccentWorldExtent : 0f;
            float contactDistance = feedback != null
                ? feedback.ActionAccentContactDistance
                : float.PositiveInfinity;
            bool pass = feedback != null && feedback.IsPerformingAction &&
                feedback.ActionVisualSignature == expectedSignature &&
                feedback.ActionVisualSignatureMarker == expectedMarker &&
                feedback.ActionAccentRendererCount >= 3 && feedback.ActionAccentRendererCount <= 5 &&
                feedback.ActionAccentColliderCount == 0 &&
                feedback.ActionAccentMaterialCount > 0 && feedback.ActionAccentMaterialCount <= 5 &&
                finiteBounds && bounds.x > 0f && bounds.y > 0f && bounds.z > 0f &&
                bounds.x <= MoonlightActionFeedback.MaximumActionAccentExtent &&
                bounds.y <= MoonlightActionFeedback.MaximumActionAccentExtent &&
                bounds.z <= MoonlightActionFeedback.MaximumActionAccentExtent &&
                extent >= MoonlightActionFeedback.MinimumActionAccentExtent &&
                extent <= MoonlightActionFeedback.MaximumActionAccentExtent &&
                (!requireContactCenter || contactDistance <= 0.01f);
            detail = $"visual={feedback?.ActionVisualSignature ?? "missing"}/{expectedSignature} " +
                $"marker={feedback?.ActionVisualSignatureMarker ?? "missing"}/{expectedMarker} " +
                $"renderers={feedback?.ActionAccentRendererCount ?? 0}/3-5 " +
                $"colliders={feedback?.ActionAccentColliderCount ?? -1} " +
                $"materials={feedback?.ActionAccentMaterialCount ?? 0}/<=5 bounds={bounds:F3} " +
                $"extent={extent:0.000}/{MoonlightActionFeedback.MinimumActionAccentExtent:0.00}-" +
                $"{MoonlightActionFeedback.MaximumActionAccentExtent:0.00} " +
                $"contactDistance={contactDistance:0.000}";
            return pass;
        }

        static IEnumerator ObserveBedtimeRuntime(MoonlightActionFeedback feedback,
            MoonlightActivityStage stage, string expectedState,
            BedtimeRuntimeObservation observation)
        {
            if (feedback == null || stage == null)
            {
                observation.Detail = "feedback-or-stage-missing";
                yield break;
            }

            bool liveStagePass = true;
            bool finished = false;
            float lingerStartedAt = -1f;
            string liveStageDetail = "linger-not-observed";
            float deadline = Time.time + 5.0f;
            while (Time.time < deadline)
            {
                if (stage.IsLingering)
                {
                    if (lingerStartedAt < 0f) lingerStartedAt = Time.time;
                    observation.SawVisibleLinger = true;
                    bool framePass = ValidateBedtimeRuntimeOracle(feedback, expectedState,
                        true,
                        out liveStageDetail);
                    if (framePass) observation.HeldPresentationFrames++;
                    liveStagePass &= framePass;
                }

                if (!feedback.IsPerformingAction && !feedback.IsPresentingResult)
                {
                    finished = true;
                    break;
                }
                yield return null;
            }

            observation.ObservedLingerSeconds = lingerStartedAt >= 0f
                ? Mathf.Max(0f, Time.time - lingerStartedAt)
                : 0f;
            string expectedAccent = expectedState == "Cuddled"
                ? "ActionAccent-cuddle-heart-pair"
                : "ActionAccent-dream-moon-pair";
            string expectedOrb = expectedState == "Cuddled"
                ? "ActionOrb-cuddle-orbit"
                : "ActionOrb-dream-orbit";
            float minimumCameraPositionDelta = float.PositiveInfinity;
            float minimumCameraRotationDelta = float.PositiveInfinity;
            SampleBedtimeCameraRestore(observation, ref minimumCameraPositionDelta,
                ref minimumCameraRotationDelta);
            if (finished)
            {
                float cleanupDeadline = Time.time + 0.75f;
                do
                {
                    yield return null;
                    SampleBedtimeCameraRestore(observation, ref minimumCameraPositionDelta,
                        ref minimumCameraRotationDelta);
                }
                while (Time.time < cleanupDeadline &&
                       (FindBedtimeStageRoot() != null ||
                        GameObject.Find(expectedAccent) != null ||
                        GameObject.Find(expectedOrb) != null ||
                        feedback.IsCameraFocusActive ||
                        !feedback.VisualPoseRestoredForQA));
            }
            Camera restoredCamera = Camera.main;
            bool cameraIdentityPass = observation.HadCamera
                ? restoredCamera != null &&
                  restoredCamera.GetInstanceID() == observation.CameraInstanceId
                : restoredCamera == null;
            float cameraPositionDelta = cameraIdentityPass
                ? minimumCameraPositionDelta
                : float.PositiveInfinity;
            float cameraRotationDelta = cameraIdentityPass
                ? minimumCameraRotationDelta
                : float.PositiveInfinity;
            CameraController restoredController = restoredCamera != null
                ? restoredCamera.GetComponent<CameraController>()
                : null;
            bool controllerStatePass = observation.HadCameraController
                ? restoredController != null &&
                  restoredController.GetInstanceID() == observation.CameraControllerInstanceId &&
                  restoredController.enabled == observation.CameraControllerEnabledBefore
                : restoredController == null;
            bool cameraRestorePass = cameraIdentityPass &&
                cameraPositionDelta <= 0.05f && cameraRotationDelta <= 1.0f &&
                controllerStatePass;
            bool stageLingerPass = stage.ValidateLastBedtimeLingerRuntimeContract(
                expectedState, 0.35f, out string stageLingerDetail);
            bool naturalCleanup = FindBedtimeStageRoot() == null &&
                GameObject.Find(expectedAccent) == null &&
                GameObject.Find(expectedOrb) == null &&
                !feedback.IsCameraFocusRequestedForQA &&
                feedback.VisualPoseRestoredForQA &&
                cameraRestorePass && stageLingerPass;
            bool lingerPass = Mathf.Abs(observation.ObservedLingerSeconds - 2.0f) <= 0.35f;
            observation.Passed = observation.SawVisibleLinger && liveStagePass && finished &&
                observation.HeldPresentationFrames > 0 && lingerPass && naturalCleanup;
            observation.Detail = $"visibleLinger={observation.SawVisibleLinger} " +
                $"holdFrames={observation.HeldPresentationFrames} " +
                $"liveStage={liveStagePass} finished={finished} " +
                $"linger={observation.ObservedLingerSeconds:0.000}/2.000s " +
                $"cameraRestore={cameraRestorePass} identity={cameraIdentityPass} " +
                $"positionDelta={cameraPositionDelta:0.000}/<=0.050m " +
                $"rotationDelta={cameraRotationDelta:0.000}/<=1.000deg " +
                $"controller={controllerStatePass} " +
                $"controllerEnabled={observation.CameraControllerEnabledBefore}/" +
                $"{(restoredController != null ? restoredController.enabled.ToString() : "missing")} " +
                $"naturalCleanup={naturalCleanup} stageLinger=({stageLingerDetail}) " +
                $"live=({liveStageDetail})";
        }

        static void CaptureBedtimeCameraBaseline(BedtimeRuntimeObservation observation)
        {
            Camera camera = Camera.main;
            observation.HadCamera = camera != null;
            if (camera == null) return;

            observation.CameraInstanceId = camera.GetInstanceID();
            observation.CameraPositionBefore = camera.transform.position;
            observation.CameraRotationBefore = camera.transform.rotation;
            CameraController controller = camera.GetComponent<CameraController>();
            observation.HadCameraController = controller != null;
            if (controller == null) return;

            observation.CameraControllerInstanceId = controller.GetInstanceID();
            observation.CameraControllerEnabledBefore = controller.enabled;
        }

        static void SampleBedtimeCameraRestore(BedtimeRuntimeObservation observation,
            ref float minimumPositionDelta, ref float minimumRotationDelta)
        {
            if (!observation.HadCamera) return;
            Camera camera = Camera.main;
            if (camera == null || camera.GetInstanceID() != observation.CameraInstanceId)
                return;

            minimumPositionDelta = Mathf.Min(minimumPositionDelta,
                Vector3.Distance(camera.transform.position,
                    observation.CameraPositionBefore));
            minimumRotationDelta = Mathf.Min(minimumRotationDelta,
                Quaternion.Angle(camera.transform.rotation,
                    observation.CameraRotationBefore));
        }

        static GameObject FindBedtimeStageRoot() =>
            GameObject.Find("ActivityStage-SleepCuddle");

        static bool ValidateBedtimeRuntimeOracle(MoonlightActionFeedback feedback,
            string expectedState, bool requirePresentationHold, out string detail)
        {
            GameObject root = FindBedtimeStageRoot();
            if (root == null)
            {
                detail = "root=missing";
                return false;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Renderer[] visibleRenderers = renderers.Where(renderer => renderer != null &&
                renderer.enabled && renderer.gameObject.activeInHierarchy &&
                !renderer.forceRenderingOff).ToArray();
            var allocatedMaterials = new HashSet<Material>();
            var visibleMaterials = new HashSet<Material>();
            foreach (Renderer renderer in renderers)
                foreach (Material material in renderer.sharedMaterials)
                    if (material != null) allocatedMaterials.Add(material);
            foreach (Renderer renderer in visibleRenderers)
                foreach (Material material in renderer.sharedMaterials)
                    if (material != null) visibleMaterials.Add(material);

            Light[] lights = root.GetComponentsInChildren<Light>(true);
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            bool cuddled = expectedState == "Cuddled";
            bool partContract = true;
            int matchingParts = 0;
            for (int i = 0; i < BedtimeOracleParts.Length; i++)
            {
                BedtimeOraclePart expected = BedtimeOracleParts[i];
                Transform part = transforms.FirstOrDefault(candidate =>
                    candidate != null && candidate.name == expected.Name);
                bool expectedVisible = cuddled
                    ? expected.CuddledVisible
                    : expected.RestingVisible;
                MeshFilter meshFilter = part != null ? part.GetComponent<MeshFilter>() : null;
                bool primitiveMatches = meshFilter != null && meshFilter.sharedMesh != null &&
                    meshFilter.sharedMesh.name.Contains(expected.MeshToken,
                        System.StringComparison.OrdinalIgnoreCase);
                bool visibleMatches = part != null && part.gameObject.activeSelf == expectedVisible;
                if (part != null) matchingParts++;
                partContract &= part != null && primitiveMatches && visibleMatches;
            }

            Transform blanket = transforms.FirstOrDefault(candidate =>
                candidate != null && candidate.name == "BedtimeBlanket");
            Transform pillow = transforms.FirstOrDefault(candidate =>
                candidate != null && candidate.name == "RestingPillow");
            Transform moon = transforms.FirstOrDefault(candidate =>
                candidate != null && candidate.name == "RestingDreamMoon");
            Transform star = transforms.FirstOrDefault(candidate =>
                candidate != null && candidate.name == "RestingDreamStar");
            Transform heartLeft = transforms.FirstOrDefault(candidate =>
                candidate != null && candidate.name == "CuddledHeartLeft");
            Transform heartRight = transforms.FirstOrDefault(candidate =>
                candidate != null && candidate.name == "CuddledHeartRight");
            Transform heartPoint = transforms.FirstOrDefault(candidate =>
                candidate != null && candidate.name == "CuddledHeartPoint");
            bool semanticLayout = cuddled
                ? heartLeft != null && heartRight != null && heartPoint != null &&
                  heartLeft.localPosition.x < heartPoint.localPosition.x &&
                  heartPoint.localPosition.x < heartRight.localPosition.x &&
                  heartLeft.localPosition.y > heartPoint.localPosition.y &&
                  heartRight.localPosition.y > heartPoint.localPosition.y
                : blanket != null && pillow != null && moon != null && star != null &&
                  moon.localPosition.y > pillow.localPosition.y + 0.40f &&
                  star.localPosition.y > blanket.localPosition.y + 0.40f &&
                  moon.localPosition.x < star.localPosition.x;

            bool lightContract = lights.Length == 1 && lights[0] != null &&
                lights[0].enabled && lights[0].gameObject.activeInHierarchy &&
                lights[0].type == LightType.Spot &&
                lights[0].gameObject.name == "BedtimeSpotlight";
            string expectedAccentName = cuddled
                ? "ActionAccent-cuddle-heart-pair"
                : "ActionAccent-dream-moon-pair";
            string expectedOrbName = cuddled
                ? "ActionOrb-cuddle-orbit"
                : "ActionOrb-dream-orbit";
            GameObject accent = GameObject.Find(expectedAccentName);
            int accentRenderers = accent != null
                ? accent.GetComponentsInChildren<Renderer>(true).Count(renderer =>
                    renderer.enabled && renderer.gameObject.activeInHierarchy)
                : 0;
            Camera camera = Camera.main;
            Vector3 cameraTarget = root.transform.position + Vector3.up * 0.70f;
            float cameraDistance = camera != null
                ? Vector3.Distance(camera.transform.position, cameraTarget)
                : float.PositiveInfinity;
            float cameraFacingAngle = camera != null
                ? Vector3.Angle(camera.transform.forward,
                    (cameraTarget - camera.transform.position).normalized)
                : 180f;
            bool cameraFraming = !requirePresentationHold ||
                (camera != null && cameraDistance >= 3.20f && cameraDistance <= 4.80f &&
                 cameraFacingAngle <= 3f);
            bool presentationHold = !requirePresentationHold ||
                (feedback != null && feedback.IsPresentingResult &&
                 feedback.IsCameraFocusActive && !feedback.VisualPoseRestoredForQA &&
                 accentRenderers == 5 && GameObject.Find(expectedOrbName) != null &&
                 cameraFraming);
            bool pass = root.activeInHierarchy && renderers.Length == 8 &&
                visibleRenderers.Length == 5 && allocatedMaterials.Count > 0 &&
                allocatedMaterials.Count <= 5 && visibleMaterials.Count > 0 &&
                visibleMaterials.Count <= 5 && lightContract && colliders.Length == 0 &&
                matchingParts == 8 && partContract && semanticLayout &&
                feedback != null && feedback.IsCameraFocusActive && presentationHold;
            detail = $"state={expectedState} root={root.activeInHierarchy} " +
                $"renderers={renderers.Length}/8 visible={visibleRenderers.Length}/5 " +
                $"materials={allocatedMaterials.Count}/{visibleMaterials.Count}/<=5 " +
                $"lights={lights.Length}/1 spot={lightContract} colliders={colliders.Length}/0 " +
                $"parts={matchingParts}/8 primitiveMask={partContract} " +
                $"semanticLayout={semanticLayout} focus={feedback?.IsCameraFocusActive ?? false} " +
                $"hold={presentationHold} accents={accentRenderers}/5 " +
                $"camera={cameraDistance:0.000}m/{cameraFacingAngle:0.00}deg";
            return pass;
        }

        public static bool ValidateBedtimeViewportStaticContract(out string detail)
        {
            string[] names = { "desktop", "ipad" };
            int[] widths = { 1366, 1024 };
            int[] heights = { 1024, 768 };
            const float stageWidth = 1.72f;
            const float stageHeight = 1.26f;
            const float conservativeCameraDistance = 3.55f;
            float verticalSpan = 2f * conservativeCameraDistance *
                Mathf.Tan(30f * Mathf.Deg2Rad);
            int passed = 0;
            string evidence = "";
            for (int i = 0; i < names.Length; i++)
            {
                float aspect = widths[i] / (float)heights[i];
                float horizontalSpan = verticalSpan * aspect;
                float horizontalOccupancy = stageWidth / horizontalSpan;
                float verticalOccupancy = stageHeight / verticalSpan;
                bool profilePass = widths[i] >= 1024 && heights[i] >= 768 &&
                    aspect >= 1.30f && horizontalOccupancy <= 0.50f &&
                    verticalOccupancy <= 0.42f;
                if (profilePass) passed++;
                if (evidence.Length > 0) evidence += ";";
                evidence += $"{names[i]}={widths[i]}x{heights[i]}:" +
                    $"{horizontalOccupancy:0.000}/{verticalOccupancy:0.000}:" +
                    $"{profilePass}";
            }
            detail = $"profiles={passed}/2 framing={evidence} " +
                "limits=0.50w/0.42h source=qa-local-bedtime-envelope";
            return passed == 2;
        }

        static IEnumerator ObserveBedtimeStageCleanupRuntime(
            MoonlightPlayerController controller, MoonlightSpatialActionZone zone,
            MoonlightActivityStage stage, BedtimeStageCleanupObservation observation)
        {
            if (controller == null || zone == null || stage == null)
            {
                observation.Detail = "setup=missing";
                yield break;
            }

            MoonlightGestureSample sample = MoonlightGestureSample.Synthetic(
                MoonlightGestureKind.Hold, 0.95f);
            Vector3 originalPosition = controller.transform.position;
            stage.Begin(MoonlightSpatialActionKind.SleepCuddle, 0, 1, sample, "Resting");
            Vector3 outsidePosition = zone.transform.position +
                Vector3.right * (zone.Radius + 2.0f);
            outsidePosition.y = originalPosition.y;
            controller.transform.position = outsidePosition;
            float exitDeadline = Time.time + 1.5f;
            while (stage.IsVisible && Time.time < exitDeadline)
                yield return null;
            yield return null;
            bool exitPass = !stage.IsVisible && FindBedtimeStageRoot() == null;
            controller.transform.position = originalPosition;
            yield return new WaitForSeconds(0.20f);

            stage.Begin(MoonlightSpatialActionKind.SleepCuddle, 0, 1, sample, "Cuddled");
            stage.enabled = false;
            yield return null;
            bool disablePass = !stage.IsVisible && FindBedtimeStageRoot() == null;
            stage.enabled = true;

            var destroyProbe = new GameObject("MoonlightBedtimeDestroyCleanupProbe");
            var destroyStage = destroyProbe.AddComponent<MoonlightActivityStage>();
            destroyStage.enabled = false;
            destroyStage.Begin(MoonlightSpatialActionKind.SleepCuddle, 0, 1, sample,
                "Resting");
            GameObject destroyStageRoot = FindBedtimeStageRoot();
            int destroyStageRootId = destroyStageRoot != null
                ? destroyStageRoot.GetInstanceID()
                : 0;
            Object.Destroy(destroyProbe);
            yield return null;
            GameObject remainingStageRoot = FindBedtimeStageRoot();
            bool destroyPass = destroyStageRootId != 0 &&
                (remainingStageRoot == null ||
                 remainingStageRoot.GetInstanceID() != destroyStageRootId);

            observation.Passed = exitPass && disablePass && destroyPass;
            observation.Detail = $"stageExit={exitPass} stageDisable={disablePass} " +
                $"stageDestroy={destroyPass} scope=stage-root-components";
        }

        IEnumerator RunRoomCaptureQa(string[] args)
        {
            int pathIndex = System.Array.IndexOf(args, "-moonlightRoomQaPath");
            string output = pathIndex >= 0 && pathIndex + 1 < args.Length
                ? args[pathIndex + 1]
                : Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
                    "MMH-QA/room-cycle");
            Directory.CreateDirectory(output);
            Screen.SetResolution(1366, 1024, false);
            yield return new WaitForSeconds(2.5f);

            var rooms = FindAnyObjectByType<RoomManager>();
            var controller = FindAnyObjectByType<MoonlightPlayerController>();
            if (rooms == null || controller == null)
            {
                Debug.LogError("[MoonlightRoomQA][FAIL] rooms/controller missing");
                Application.Quit(30);
                yield break;
            }

            var captureRooms = new[]
            {
                RoomType.LivingRoom, RoomType.Kitchen, RoomType.Bedroom, RoomType.Garden, RoomType.Library
            };
            bool roomSurfaceProfilePass =
                MoonlightHouseSetup.ValidateRoomSurfaceProfileContract(out string roomSurfaceProfileDetail);
            var livingRoom = GameObject.Find("LivingRoom");
            var roomSurfaceQuality = livingRoom != null
                ? livingRoom.GetComponent<MoonlightRoomVisualQuality>()
                : null;
            bool roomSurfaceRuntimePass = roomSurfaceQuality != null &&
                roomSurfaceQuality.QAMarker == "MOONLIGHT_ROOM_SURFACE_SHADING_READY";
            bool roomSurfacePass = roomSurfaceProfilePass && roomSurfaceRuntimePass;
            string roomSurfaceRuntimeDetail = roomSurfaceQuality != null
                ? $"renderers={roomSurfaceQuality.RendererCount} " +
                  $"casters={roomSurfaceQuality.ShadowCasterCount} " +
                  $"receivers={roomSurfaceQuality.ShadowReceiverCount} " +
                  $"materials={roomSurfaceQuality.SourceMaterialCount}/" +
                  $"{roomSurfaceQuality.RuntimeMaterialCount} " +
                  $"delta={roomSurfaceQuality.RuntimeMaterialCountDelta} " +
                  $"semantic={roomSurfaceQuality.SemanticMaterialCount}/" +
                  $"{MoonlightHouseSetup.RoomSurfaceProfileMinimum} " +
                  $"semanticProfiles={roomSurfaceQuality.SemanticProfileMatchCount}/" +
                  $"{roomSurfaceQuality.SemanticMaterialCount} " +
                  $"profiles={roomSurfaceQuality.SurfaceProfileCount} " +
                  $"emissive={roomSurfaceQuality.EmissiveMaterialCount}"
                : "quality=missing";
            Debug.Log(roomSurfacePass
                ? "[MoonlightRoomQA][PASS] room-surface-shading " +
                  $"profile=({roomSurfaceProfileDetail}) runtime=({roomSurfaceRuntimeDetail}) " +
                  "marker=MOONLIGHT_ROOM_SURFACE_SHADING_READY"
                : "[MoonlightRoomQA][FAIL] room-surface-shading " +
                  $"profilePass={roomSurfaceProfilePass} profile=({roomSurfaceProfileDetail}) " +
                  $"runtimePass={roomSurfaceRuntimePass} runtime=({roomSurfaceRuntimeDetail}) " +
                  $"marker={(roomSurfaceQuality != null ? roomSurfaceQuality.QAMarker : "missing")}");
            bool heroDressingPass = ValidateHeroDressing(out string heroDressingDetail);
            Debug.Log(heroDressingPass
                ? "[MoonlightRoomQA][PASS] hero-dressing " + heroDressingDetail
                : "[MoonlightRoomQA][FAIL] hero-dressing " + heroDressingDetail);
            for (int i = 0; i < captureRooms.Length; i++)
            {
                rooms.GoToRoom(captureRooms[i]);
                yield return new WaitForSeconds(1.25f);
                string shot = Path.Combine(output, $"{i + 1:00}_{captureRooms[i].ToString().ToLowerInvariant()}.png");
                yield return Capture(shot);
                Debug.Log($"[MoonlightRoomQA][PASS] room={captureRooms[i]} position={controller.transform.position:F2} screenshot={shot}");
            }

            Application.Quit(!heroDressingPass ? 31 : roomSurfacePass ? 0 : 32);
        }

        static bool ValidateHeroDressing(out string detail)
        {
            var livingRoom = GameObject.Find("LivingRoom");
            var dressing = livingRoom != null ? livingRoom.transform.Find("HeroFurniturePass") : null;
            if (dressing == null)
            {
                detail = "root missing";
                return false;
            }

            string[] required = { "bookcaseOpen", "books", "lampRoundFloor", "pottedPlant" };
            int present = required.Count(name => dressing.Find(name) != null);
            int renderers = dressing.GetComponentsInChildren<Renderer>(true).Count(renderer => renderer.enabled);
            int materials = dressing.GetComponentsInChildren<Renderer>(true)
                .SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => material != null)
                .Distinct()
                .Count();
            int colliders = dressing.GetComponentsInChildren<Collider>(true).Count(collider => collider.enabled);
            int lights = dressing.GetComponentsInChildren<Light>(true).Count(light => light.enabled);
            bool shelfDressed = dressing.Find("booksShelfMid") != null && dressing.Find("booksShelfTop") != null;
            var authoredRoomTransforms = livingRoom.GetComponentsInChildren<Transform>(true);
            string[] authoredBlenderDetails =
            {
                "BackChairRail", "CelestialWallPanel", "CelestialCrescent",
                "CelestialStarA", "StarGarland_1", "StarGarland_5"
            };
            int authoredDetailsPresent = authoredBlenderDetails.Count(requiredName =>
                authoredRoomTransforms.Any(candidate => candidate.name == requiredName));
            detail = $"props={present}/{required.Length} shelfBooks={(shelfDressed ? 3 : 1)}/3 " +
                $"renderers={renderers}/24 materials={materials}/12 " +
                $"colliders={colliders} lights={lights} " +
                $"blenderWallDetails={authoredDetailsPresent}/{authoredBlenderDetails.Length}";
            return present == required.Length && shelfDressed && renderers >= required.Length && renderers <= 24
                && materials <= 12 && colliders == 0 && lights == 0
                && authoredDetailsPresent == authoredBlenderDetails.Length;
        }

        static IEnumerator Capture(string path)
        {
            if (Application.isBatchMode)
            {
                Debug.Log("[MoonlightVisualQA] Screenshot skipped in batch mode: " + path);
                yield break;
            }

            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(path);
            yield return new WaitForSeconds(0.25f);
        }

        public void RegisterController(MoonlightPlayerController controller)
        {
            if (controller == null || _registered) return;
            _registered = true;
            _lastLoggedPosition = controller.transform.position;
            Debug.Log($"[MoonlightVisualQA] controller-registered speedInput={controller.TouchMove} bounds={controller.RoomBounds}");
        }

        public void LogMovement(Vector3 position, Vector2 input, bool clamped)
        {
            if (Time.time - _lastMoveLogTime < 0.75f && Vector3.Distance(position, _lastLoggedPosition) < 0.75f)
                return;

            _lastMoveLogTime = Time.time;
            _lastLoggedPosition = position;
            Debug.Log($"[MoonlightVisualQA] move pos={position:F2} input={input:F2} clamped={clamped}");
        }

        public void LogContextAction(MoonlightSpatialActionZone zone, Vector3 playerPosition, string result)
        {
            if (zone == null) return;
            Debug.Log($"[MoonlightVisualQA] context-action kind={zone.Kind} zone={zone.DisplayName} player={playerPosition:F2} result=\"{result}\"");
        }
    }
}
