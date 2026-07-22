using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public class MoonlightVisualQA : MonoBehaviour
    {
        public static MoonlightVisualQA Instance { get; private set; }

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
                if (zone.RequiredSteps > 1 && (activityStage == null || !activityStage.IsVisible))
                {
                    Debug.LogError($"[MoonlightVisualQA][FAIL] action={zone.Kind} staged activity props missing");
                    Application.Quit(9);
                    yield break;
                }

                string actionOutput = zone.Kind == MoonlightSpatialActionKind.Play
                    ? output
                    : Path.Combine(directory ?? string.Empty,
                        $"moonlight_action_{zone.Kind.ToString().ToLowerInvariant()}.png");
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(actionOutput);
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
                while ((feedback.IsPerformingAction || feedback.IsCoolingDown) && Time.time < settleDeadline)
                    yield return null;
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
                        yield return new WaitForEndOfFrame();
                        ScreenCapture.CaptureScreenshot(finalOutput);
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
                string cuddleResult = interactor.ExecuteCurrent();
                ui?.ShowContextResult(cuddleResult);
                feedback = moonlight.GetComponent<MoonlightActionFeedback>();
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
                yield return new WaitForSeconds(0.35f);
                if (ui != null && ui.resultLabel != null && !string.IsNullOrEmpty(ui.resultLabel.text))
                {
                    Debug.LogError("[MoonlightVisualQA][FAIL] action=Cuddle result appeared before animation completed");
                    Application.Quit(8);
                    yield break;
                }
                string cuddleOutput = Path.Combine(directory ?? string.Empty, "moonlight_action_cuddle.png");
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(cuddleOutput);
                Debug.Log($"[MoonlightVisualQA][PASS] action=Cuddle prompt=\"{cuddlePrompt}\" " +
                    $"result=\"{cuddleResult}\" effect={feedback.ActiveEffectName} " +
                    $"visual={feedback.ActionVisualSignature} accent={feedback.ActionAccentRendererCount} " +
                    $"colliders={feedback.ActionAccentColliderCount} materials={feedback.ActionAccentMaterialCount} " +
                    $"bounds={feedback.ActionAccentBoundsSize:F3} accentExtent={feedback.ActionAccentWorldExtent:0.000} " +
                    $"signatureMarker={feedback.ActionVisualSignatureMarker} " +
                    $"screenshot={cuddleOutput}");
                passedActions++;
                yield return new WaitForSeconds(1.2f);
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
                pad == null || audio == null)
            {
                Debug.LogError("[MoonlightGameplayQA][FAIL] gameplay controller/rooms/character/pad/audio missing");
                Application.Quit(20);
                yield break;
            }

            if (!MoonlightActivityStage.ValidateSurfaceDepthContract(out string surfaceDepthDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-surface-depth-contract " +
                    surfaceDepthDetail);
                Application.Quit(67);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] activity-surface-depth-contract " +
                $"{surfaceDepthDetail} marker=MOONLIGHT_ACTIVITY_SURFACE_DEPTH_CONTRACT_VERIFIED");
            if (!MoonlightUI.ValidateIPadProgressFeedbackContract(out string progressFeedbackDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-progress-feedback-contract " +
                    progressFeedbackDetail);
                Application.Quit(68);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] ipad-progress-feedback-contract " +
                $"{progressFeedbackDetail} marker=MOONLIGHT_IPAD_PROGRESS_FEEDBACK_CONTRACT_VERIFIED");
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
                Application.Quit(79);
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
            Debug.Log($"[MoonlightGameplayQA][PASS] ipad-gesture-result-feedback " +
                $"{resultFeedbackDetail} marker=MOONLIGHT_IPAD_GESTURE_RESULT_FEEDBACK_VERIFIED");
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
            if (!controller.ValidateIPadSprintRuntimeContract(out string sprintDetail))
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] ipad-sprint-contract {sprintDetail}");
                Application.Quit(85);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] ipad-sprint-contract {sprintDetail} " +
                "marker=MOONLIGHT_IPAD_SPRINT_CONTRACT_VERIFIED");
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
                        expectedCameraDirection) <= 0.001f &&
                    !float.IsNaN(ui.NavigationCueAngleDegrees) &&
                    !float.IsInfinity(ui.NavigationCueAngleDegrees) &&
                    Mathf.Abs(Mathf.DeltaAngle(ui.NavigationCueAngleDegrees,
                        expectedNavigationAngle)) <= 0.1f &&
                    Mathf.Abs(Mathf.DeltaAngle(ui.NavigationCueVisualAngleDegrees,
                        expectedNavigationAngle)) <= 0.1f &&
                    ui.NavigationCueTargetName == spatialInteractor.NearestZone.DisplayName &&
                    ui.NavigationCueRenderedLabel == expectedNavigationLabel &&
                    MoonlightUI.IsValidCompactNavigationLabel(ui.NavigationCueRenderedLabel) &&
                    Approximately(ui.NavigationCueTargetDistance, spatialInteractor.NearestDistance) &&
                    ui.NavigationCueQAMarker == "MOONLIGHT_IPAD_NAVIGATION_CUE_READY";
                bool layoutPass = ui.IsIPadHUDLayoutActive &&
                    ui.HUDLayoutQAMarker == "ipad-activity-focus-v3" &&
                    ui.ActionTouchTargetMeetsIPadMinimum &&
                    ui.ActionTouchTargetIsInsideSafeArea &&
                    ui.ActivityPromptIsInsideSafeArea &&
                    ui.ActivityResultIsInsideSafeArea &&
                    ui.ActivityProgressIsInsideSafeArea &&
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
                        $"touch={ui.ActionTouchTargetLayoutSize} minimum={ui.IPadMinimumTouchTargetLayoutSize} " +
                        $"insideSafe={ui.ActionTouchTargetIsInsideSafeArea} " +
                        $"promptSafe={ui.ActivityPromptIsInsideSafeArea} resultSafe={ui.ActivityResultIsInsideSafeArea} " +
                        $"progressSafe={ui.ActivityProgressIsInsideSafeArea} separated={ui.ActivityHUDPanelsDoNotOverlap} " +
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

            var activityKinds = new[]
            {
                MoonlightSpatialActionKind.Cook,
                MoonlightSpatialActionKind.Play,
                MoonlightSpatialActionKind.Garden,
                MoonlightSpatialActionKind.Read
            };
            var activityRooms = new[]
            {
                RoomType.Kitchen,
                RoomType.LivingRoom,
                RoomType.Garden,
                RoomType.Library
            };
            int completedActivities = 0;

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
                    MoonlightSpatialActionKind.Garden or MoonlightSpatialActionKind.Read)
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
                        _ => "99"
                    };
                    string persistentShot = Path.Combine(output,
                        $"{persistentPrefix}_{expectedKind.ToString().ToLowerInvariant()}_station_before.png");
                    yield return Capture(persistentShot);
                    Debug.Log($"[MoonlightGameplayQA][PASS] persistent-station-before action={expectedKind} " +
                        $"anchor={persistentAnchor:F2} renderers={persistentStation.RendererCount} " +
                        $"materials={persistentStation.UniqueMaterialCount} screenshot={persistentShot}");
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
                controller.TeleportTo(zone.transform.position, controller.RoomBounds);
                yield return new WaitForSeconds(0.65f);
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
                bool lowScoreAccepted = pad.SubmitSynthetic(zone.RequiredGesture, 0.20f);
                yield return new WaitForSeconds(0.15f);
                bool rejectedMovementRetained = !expectIPadHud ||
                    (touchJoystick.ResetSequence == resetSequenceBeforeRejectedAction &&
                     touchJoystick.IsTrackingPointer &&
                     Vector2.Distance(touchJoystick.Value, rejectedHeldInput) <= 0.0001f &&
                     touchJoystick.KnobAnchoredPosition.sqrMagnitude > 0.0001f &&
                     Vector2.Distance(controller.TouchMove, rejectedHeldInput) <= 0.0001f);
                if (lowScoreAccepted || zone.ProgressStep != startStep || zone.LastGesturePassed ||
                    audio.LastCueKey != "activity-try-again" || !rejectedMovementRetained)
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] fail-gesture advanced action={zone.Kind} " +
                        $"accepted={lowScoreAccepted} step={zone.ProgressStep} cue={audio.LastCueKey} " +
                        $"retained={rejectedMovementRetained} " +
                        $"tracking={(touchJoystick != null && touchJoystick.IsTrackingPointer)} " +
                        $"value={(touchJoystick != null ? touchJoystick.Value : Vector2.zero):F3} " +
                        $"controller={controller.TouchMove:F3}");
                    Application.Quit(24);
                    yield break;
                }
                Debug.Log($"[MoonlightGameplayQA][PASS] fail-gesture action={zone.Kind} " +
                    $"score={zone.LastGestureScore:0.00} heldMovementRetained={rejectedMovementRetained} " +
                    "marker=MOONLIGHT_REJECTED_ACTIVITY_MOVEMENT_RETAINED");
                if (expectIPadHud)
                    touchJoystick.ClearInputForQA();

                for (int step = 0; step < zone.RequiredSteps; step++)
                {
                    var expected = zone.RequiredGesture;
                    if (expectIPadHud)
                    {
                        yield return null;
                        string expectedProgress = $"{step + 1}/{zone.RequiredSteps}";
                        float expectedFill = step / (float)zone.RequiredSteps;
                        bool stepHudPass = ui.ActivityProgressQAMarker == expectedProgress &&
                            Approximately(ui.ActivityProgressFill01, expectedFill) &&
                            ui.ActivityProgressFillQAMarker ==
                                "MOONLIGHT_IPAD_ACTIVITY_PROGRESS_FILL_READY" &&
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
                                $"gesture={ui.GestureCommandQAMarker} touch={ui.ActionTouchTargetLayoutSize} " +
                                $"insideSafe={ui.ActionTouchTargetIsInsideSafeArea} " +
                                $"panelsSeparated={ui.ActivityHUDPanelsDoNotOverlap}");
                            Application.Quit(43);
                            yield break;
                        }
                        Debug.Log($"[MoonlightGameplayQA][PASS] ipad-activity-hud action={zone.Kind} " +
                            $"step={step + 1} progress={ui.ActivityProgressQAMarker} " +
                            $"fill={ui.ActivityProgressFill01:0.000} " +
                            $"gesture=\"{ui.GestureCommandQAMarker}\"");
                    }
                    Vector3 acceptedActionPosition = controller.transform.position;
                    var acceptedActionZone = spatialInteractor.CurrentZone;
                    int resetSequenceBeforeAcceptedAction = touchJoystick != null
                        ? touchJoystick.ResetSequence
                        : -1;
                    if (expectIPadHud)
                        touchJoystick.ArmHeldInputForQA(new Vector2(0.72f, 0.38f));
                    bool acceptedActionStarted = pad.SubmitSynthetic(expected, 0.95f);
                    yield return new WaitForSeconds(0.08f);
                    var feedback = moonlight.GetComponent<MoonlightActionFeedback>();
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
                         Vector2.Distance(controller.TouchMove, busyHeldInput) <= 0.0001f);
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
                    Debug.Log($"[MoonlightGameplayQA][PASS] busy-gesture action={zone.Kind} " +
                        $"step={step + 1} reason=\"{pad.LastRejectionReason}\" " +
                        $"heldMovementRetained={busyMovementRetained} " +
                        "marker=MOONLIGHT_BUSY_GESTURE_REJECTED");
                    if (exerciseBusyMovementRetention)
                        touchJoystick.ClearInputForQA();
                    bool finalMasteryStep = step == zone.RequiredSteps - 1;
                    bool masteryStatePass = finalMasteryStep
                        ? zone.ActivitySessionAcceptedSteps == 0 &&
                          Approximately(zone.LastCompletedAverageScore, 0.95f) &&
                          zone.LastCompletedBestCombo == zone.RequiredSteps &&
                          zone.LastCompletedPerfectSteps == zone.RequiredSteps &&
                          zone.LastMasteryBonusCoins == 3
                        : zone.ActivitySessionAcceptedSteps == step + 1 &&
                          Approximately(zone.ActivitySessionAverageScore, 0.95f) &&
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
                        MoonlightSpatialActionKind.Garden or MoonlightSpatialActionKind.Read)
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
                            _ => step switch
                            {
                                0 => "book-cover", 1 => "turning-page", 2 => "bookmark-trace", _ => "memory-motes"
                            }
                        };

                        const float contactWeightThreshold = 0.20f;
                        bool requiresCameraReadableFacing = zone.Kind is MoonlightSpatialActionKind.Garden or
                            MoonlightSpatialActionKind.Read;
                        const float cameraFacingMinAngle = 20f;
                        const float cameraFacingMaxAngle = 38f;
                        float contactMaxDistance = zone.Kind is MoonlightSpatialActionKind.Garden or
                            MoonlightSpatialActionKind.Read ? 4.0f : 5.5f;
                        float visualContactMaxDistance = zone.Kind switch
                        {
                            MoonlightSpatialActionKind.Cook => 1.55f,
                            MoonlightSpatialActionKind.Play => 2.0f,
                            MoonlightSpatialActionKind.Garden => 1.75f,
                            _ => 1.65f
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

                        string contactPrefix = zone.Kind switch
                        {
                            MoonlightSpatialActionKind.Cook => "02",
                            MoonlightSpatialActionKind.Play => "03",
                            MoonlightSpatialActionKind.Garden => "04",
                            _ => "05"
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
                        MoonlightSpatialActionKind.Garden or MoonlightSpatialActionKind.Read)
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
                            bool authoredWorkbenchPass = stage.HasAuthoredCookWorkbench &&
                                stage.AuthoredCookWorkbenchRendererCount >= 8 &&
                                stage.AuthoredCookWorkbenchRendererCount <= 12 &&
                                stage.AuthoredCookWorkbenchMaterialCount >= 8 &&
                                stage.AuthoredCookWorkbenchMaterialCount <= 10 &&
                                stage.AuthoredCookWorkbenchColliderCount == 0 &&
                                stage.AuthoredCookWorkbenchLightCount == 0;
                            if (!authoredWorkbenchPass)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] authored-cook-workbench " +
                                    $"present={stage.HasAuthoredCookWorkbench} " +
                                    $"renderers={stage.AuthoredCookWorkbenchRendererCount}/8-12 " +
                                    $"materials={stage.AuthoredCookWorkbenchMaterialCount}/8-10 " +
                                    $"colliders={stage.AuthoredCookWorkbenchColliderCount} " +
                                    $"lights={stage.AuthoredCookWorkbenchLightCount}");
                                Application.Quit(32);
                                yield break;
                            }
                            Debug.Log("[MoonlightGameplayQA][PASS] authored-cook-workbench " +
                                $"renderers={stage.AuthoredCookWorkbenchRendererCount} " +
                                $"materials={stage.AuthoredCookWorkbenchMaterialCount} " +
                                $"colliders={stage.AuthoredCookWorkbenchColliderCount} " +
                                $"lights={stage.AuthoredCookWorkbenchLightCount} " +
                                "marker=MOONLIGHT_AUTHORED_COOK_WORKBENCH_READY");
                        }
                        else if (zone.Kind == MoonlightSpatialActionKind.Play)
                        {
                            bool authoredArenaPass = stage.HasAuthoredPlayArena &&
                                stage.AuthoredPlayArenaRendererCount >= 7 &&
                                stage.AuthoredPlayArenaRendererCount <= 10 &&
                                stage.AuthoredPlayArenaMaterialCount >= 7 &&
                                stage.AuthoredPlayArenaMaterialCount <= 9 &&
                                stage.AuthoredPlayArenaColliderCount == 0 &&
                                stage.AuthoredPlayArenaLightCount == 0 &&
                                stage.AuthoredPlayArenaBoundsSize.x >= 2.70f &&
                                stage.AuthoredPlayArenaBoundsSize.y >= 0.55f &&
                                stage.AuthoredPlayArenaBoundsSize.y <= 1.25f &&
                                stage.AuthoredPlayArenaBoundsSize.z >= 1.10f;
                            if (!authoredArenaPass)
                            {
                                Debug.LogError("[MoonlightGameplayQA][FAIL] authored-play-arena " +
                                    $"present={stage.HasAuthoredPlayArena} " +
                                    $"renderers={stage.AuthoredPlayArenaRendererCount}/7-10 " +
                                    $"materials={stage.AuthoredPlayArenaMaterialCount}/7-9 " +
                                    $"colliders={stage.AuthoredPlayArenaColliderCount} " +
                                    $"lights={stage.AuthoredPlayArenaLightCount} " +
                                    $"bounds={stage.AuthoredPlayArenaBoundsSize:F2}");
                                Application.Quit(33);
                                yield break;
                            }
                            Debug.Log("[MoonlightGameplayQA][PASS] authored-play-arena " +
                                $"renderers={stage.AuthoredPlayArenaRendererCount} " +
                                $"materials={stage.AuthoredPlayArenaMaterialCount} " +
                                $"colliders={stage.AuthoredPlayArenaColliderCount} " +
                                $"lights={stage.AuthoredPlayArenaLightCount} " +
                                $"bounds={stage.AuthoredPlayArenaBoundsSize:F2} " +
                                "marker=MOONLIGHT_AUTHORED_PLAY_ARENA_READY");
                        }
                        else if (zone.Kind == MoonlightSpatialActionKind.Garden)
                        {
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

                        string phase = zone.Kind switch
                        {
                            MoonlightSpatialActionKind.Cook => step switch { 0 => "add", 1 => "stir", 2 => "bake", _ => "decorate" },
                            MoonlightSpatialActionKind.Play => step switch { 0 => "throw", 1 => "chase", 2 => "jump", _ => "catch" },
                            MoonlightSpatialActionKind.Garden => step switch { 0 => "plant", 1 => "water", 2 => "tend", _ => "bloom" },
                            MoonlightSpatialActionKind.Read => step switch { 0 => "open", 1 => "turn", 2 => "trace", _ => "remember" },
                            _ => "step"
                        };
                        string prefix = zone.Kind switch
                        {
                            MoonlightSpatialActionKind.Cook => "02",
                            MoonlightSpatialActionKind.Play => "03",
                            MoonlightSpatialActionKind.Garden => "04",
                            MoonlightSpatialActionKind.Read => "05",
                            _ => "99"
                        };
                        string stepShot = Path.Combine(output,
                            $"{prefix}_{zone.Kind.ToString().ToLowerInvariant()}_step_{step + 1}_{phase}.png");
                        yield return Capture(stepShot);
                        Debug.Log($"[MoonlightGameplayQA][PASS] activity-visual action={zone.Kind} " +
                            $"step={step + 1}/{zone.RequiredSteps} renderers={stage.ActiveRendererCount} " +
                            $"materials={stage.ActiveUniqueMaterialCount} lights={stage.ActiveLightCount} screenshot={stepShot}");
                    }

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
                            MoonlightSpatialActionKind.Garden => "04_after_gardening_gesture.png",
                            MoonlightSpatialActionKind.Read => "05_after_reading_gesture.png",
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
                        int finalCountdownSeconds = 0;
                        bool finalActionTextValid = ui != null &&
                            MoonlightUI.TryParseFinalActivityCountdownLabel(
                                ui.ActionButtonQAText, out finalCountdownSeconds);
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
                                $"overflow={ui.ContextResultIsOverflowing} roomNav={ui.RoomNavigationQAMarker}");
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
                                "marker=MOONLIGHT_ACTIVITY_FINAL_HUD_VERIFIED " +
                                "marker=MOONLIGHT_ACTIVITY_FINAL_CTA_SEMANTICS_LIVE_VERIFIED");
                        string presentationPrefix = zone.Kind switch
                        {
                            MoonlightSpatialActionKind.Cook => "02",
                            MoonlightSpatialActionKind.Play => "03",
                            MoonlightSpatialActionKind.Garden => "04",
                            MoonlightSpatialActionKind.Read => "05",
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
                }

                if (zone.ProgressStep != 0)
                {
                    Debug.LogError($"[MoonlightGameplayQA][FAIL] activity-loop action={zone.Kind} step={zone.ProgressStep}");
                    Application.Quit(26);
                    yield break;
                }
                if (persistentStation != null)
                {
                    RoomType detour = activityRooms[activityIndex] == RoomType.LivingRoom
                        ? RoomType.Kitchen
                        : RoomType.LivingRoom;
                    rooms.GoToRoom(detour);
                    yield return new WaitForSeconds(0.12f);
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
                        persistentStation.CompletionUniqueMaterialCount > completionMaterialBudget ||
                        persistentStation.CompletionEnabledColliderCount != 0 ||
                        persistentStation.CompletionEnabledLightCount != 0 ||
                        !completionMagicFlowerPass)
                    {
                        Debug.LogError($"[MoonlightGameplayQA][FAIL] persistent-station-reentry action={expectedKind} " +
                            $"completion={persistentStation.HasCompletionState} " +
                            $"renderers={persistentStation.CompletionRendererCount}/{expectedCompletionRenderers} " +
                            $"budget={completionRendererBudget} " +
                            $"materials={persistentStation.CompletionUniqueMaterialCount}/<={completionMaterialBudget} " +
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
                        _ => "99"
                    };
                    string reentryShot = Path.Combine(output,
                        $"{reentryPrefix}_{expectedKind.ToString().ToLowerInvariant()}_persistent_reentry.png");
                    yield return Capture(reentryShot);
                    Debug.Log($"[MoonlightGameplayQA][PASS] persistent-station-reentry action={expectedKind} " +
                        $"anchor={persistentStation.AnchorPosition:F2} renderers={persistentStation.CompletionRendererCount} " +
                        $"materials={persistentStation.CompletionUniqueMaterialCount} screenshot={reentryShot} " +
                        "marker=MOONLIGHT_PERSISTENT_ACTIVITY_STATE_VERIFIED " +
                        "marker=MOONLIGHT_PERSISTENT_ACTIVITY_REENTRY_VERIFIED");
                }
                completedActivities++;
            }

            if (audio.CuePlayCount < 18)
            {
                Debug.LogError($"[MoonlightGameplayQA][FAIL] audio cues count={audio.CuePlayCount}");
                Application.Quit(27);
                yield break;
            }
            Debug.Log($"[MoonlightGameplayQA][PASS] suite collisions={controller.CollisionCount} " +
                $"audioCues={audio.CuePlayCount} rooms={rooms.rooms.Count} activities={completedActivities}");
            Application.Quit(0);
        }

        static bool Approximately(float actual, float expected)
            => Mathf.Abs(actual - expected) <= 0.01f;

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
