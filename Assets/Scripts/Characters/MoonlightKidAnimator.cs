using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    // Lightweight runtime posing for the SD Moonlight avatar. This keeps the
    // proportions childlike and gives every care button a visible reaction.
    public class MoonlightKidAnimator : MonoBehaviour
    {
        public const int RequiredArticulatedBoneCount = 13;
        public const int MinimumObservedMovingBoneCount = 8;
        public const string ObservedLocomotionReadyMarker =
            "MOONLIGHT_KID_ANIMATOR_LOCOMOTION_OBSERVED";
        public const string ObservedLocomotionIncompleteMarker =
            "MOONLIGHT_KID_ANIMATOR_LOCOMOTION_INCOMPLETE";

        enum ActionPose { None, Snack, Hug, Nap, Play, Bath, Dance }

        Transform _root;
        Transform _hips, _spine, _head;
        Transform _leftArm, _leftForeArm, _rightArm, _rightForeArm;
        Transform _leftUpLeg, _rightUpLeg;
        Transform _leftLeg, _rightLeg, _leftFoot, _rightFoot;
        Transform[] _ribbons;
        Transform[] _articulatedBones;
        Renderer[] _renderers;

        Quaternion _baseRot;
        Vector3 _basePos;
        Vector3 _roomAnchor;
        Vector3 _roomFrom;
        Vector3 _roomTarget;
        BonePose[] _poses;
        ActionPose _pose;
        float _poseT;
        float _poseDur = 1f;
        float _roomT;
        float _roomDur = 0.45f;
        float _walkPhase;
        bool _roomMoving;
        bool _walking;
        bool _running;
        float _facingYaw;
        float _targetFacingYaw;
        bool _sdAvatar;
        TransformPose[] _qaPoseSnapshot;

        struct BonePose
        {
            public Transform bone;
            public Quaternion baseRot;
            public Vector3 baseScale;
        }

        struct TransformPose
        {
            public Transform transform;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }

        public bool IsWalkingCommanded => _walking;
        public bool IsRunningCommanded => _running;
        public float WalkPhase => _walkPhase;
        public int ActiveArticulatedBoneCount => CountActiveArticulatedBones();
        public int ActiveVisibleRendererCount => CountActiveVisibleRenderers();
        public string ObservedLocomotionQAMarker { get; private set; } =
            ObservedLocomotionIncompleteMarker;

        void Awake()
        {
            _root = transform.parent != null ? transform.parent : transform;
            _roomAnchor = _root.position;
            _baseRot = transform.localRotation;
            _basePos = transform.localPosition;

            _hips = FindBone("Hips");
            _spine = FindBone("Spine2") ?? FindBone("Spine1") ?? FindBone("Spine");
            _head = FindBone("Head");
            _leftArm = FindBone("LeftArm");
            _leftForeArm = FindBone("LeftForeArm");
            _rightArm = FindBone("RightArm");
            _rightForeArm = FindBone("RightForeArm");
            _leftUpLeg = FindBone("LeftUpLeg");
            _rightUpLeg = FindBone("RightUpLeg");
            _leftLeg = FindBone("LeftLeg");
            _rightLeg = FindBone("RightLeg");
            _leftFoot = FindBone("LeftFoot");
            _rightFoot = FindBone("RightFoot");
            _articulatedBones = new[]
            {
                _hips, _spine, _head,
                _leftArm, _leftForeArm, _rightArm, _rightForeArm,
                _leftUpLeg, _rightUpLeg, _leftLeg, _rightLeg, _leftFoot, _rightFoot
            };
            _ribbons = new[]
            {
                FindBone("J_L_HeadRibbon_00"), FindBone("J_L_HeadRibbon_01"),
                FindBone("J_L_HeadRibbon_02"), FindBone("J_L_HeadRibbon_03"),
                FindBone("J_R_HeadRibbon_00"), FindBone("J_R_HeadRibbon_01"),
                FindBone("J_R_HeadRibbon_02"), FindBone("J_R_HeadRibbon_03"),
            };
            for (int i = 0; i < _ribbons.Length; i++)
                _sdAvatar |= _ribbons[i] != null;

            ApplyProportionPass();
            CacheBasePoses();
            _renderers = GetComponentsInChildren<Renderer>(true);
            _qaPoseSnapshot = new TransformPose[_poses.Length + 1];
        }

        public void PlaySnack() => StartPose(ActionPose.Snack, 0.95f);
        public void PlayHug() => StartPose(ActionPose.Hug, 1.05f);
        public void PlayNap() => StartPose(ActionPose.Nap, 1.15f);
        public void PlayPlay() => StartPose(ActionPose.Play, 0.95f);
        public void PlayBath() => StartPose(ActionPose.Bath, 1.05f);
        public void PlayDance() => StartPose(ActionPose.Dance, 1.35f);

        public void Play(string id)
        {
            switch (id)
            {
                case "Snack": PlaySnack(); break;
                case "Hug": PlayHug(); break;
                case "Nap": PlayNap(); break;
                case "Play": PlayPlay(); break;
                case "Bath": PlayBath(); break;
                case "Dance": PlayDance(); break;
            }
        }

        public void PlayGesture(string id)
        {
            switch (id)
            {
                case "Snack": StartPose(ActionPose.Snack, 0.95f, false); break;
                case "Hug": StartPose(ActionPose.Hug, 1.05f, false); break;
                case "Nap": StartPose(ActionPose.Nap, 1.15f, false); break;
                case "Play": StartPose(ActionPose.Play, 0.95f, false); break;
                case "Bath": StartPose(ActionPose.Bath, 1.05f, false); break;
                case "Dance": StartPose(ActionPose.Dance, 1.35f, false); break;
            }
        }

        public void SetWalking(bool walking, bool running = false)
        {
            if (walking && !_walking) _walkPhase = 0f;
            _walking = walking;
            _running = walking && running;
        }

        public bool ValidateObservedLocomotionRuntimeContract(out string detail)
        {
            int activeBones = CountActiveArticulatedBones();
            int activeVisibleRenderers = CountActiveVisibleRenderers();
            int visibleArticulatedBones = CountVisibleArticulatedBones();
            bool activeRig = isActiveAndEnabled && gameObject.activeInHierarchy &&
                _poses != null && _qaPoseSnapshot != null &&
                activeBones == RequiredArticulatedBoneCount && activeVisibleRenderers > 0 &&
                visibleArticulatedBones >= MinimumObservedMovingBoneCount;
            if (!activeRig)
            {
                ObservedLocomotionQAMarker = ObservedLocomotionIncompleteMarker;
                detail = $"observed=False activeAndEnabled={isActiveAndEnabled} " +
                    $"activeInHierarchy={gameObject.activeInHierarchy} " +
                    $"activeBones={activeBones}/{RequiredArticulatedBoneCount} " +
                    $"activeVisibleRenderers={activeVisibleRenderers} " +
                    $"visibleArticulatedBones={visibleArticulatedBones} " +
                    $"marker={ObservedLocomotionQAMarker}";
                return false;
            }

            CaptureQAPoses();
            bool walking = _walking;
            bool running = _running;
            float walkPhase = _walkPhase;
            int walkMoved = 0;
            int runMoved = 0;
            float walkAmplitude = 0f;
            float runAmplitude = 0f;
            float walkCadenceAdvance = 0f;
            float runCadenceAdvance = 0f;
            bool exactPoseRestore = false;
            bool exactStateRestore = false;

            try
            {
                PrepareLocomotionSample(false, Mathf.PI * 0.5f);
                ApplyWalkCycle(0f, 0f);
                walkMoved = CountMovedArticulatedBones();
                walkAmplitude = MaximumArticulatedBoneRotation();

                PrepareLocomotionSample(true, Mathf.PI * 0.5f);
                ApplyWalkCycle(0f, 0f);
                runMoved = CountMovedArticulatedBones();
                runAmplitude = MaximumArticulatedBoneRotation();

                const float cadenceSampleSeconds = 0.125f;
                PrepareLocomotionSample(false, 0f);
                ApplyWalkCycle(0f, cadenceSampleSeconds);
                walkCadenceAdvance = _walkPhase;
                PrepareLocomotionSample(true, 0f);
                ApplyWalkCycle(0f, cadenceSampleSeconds);
                runCadenceAdvance = _walkPhase;
            }
            finally
            {
                RestoreQAPoses();
                _walking = walking;
                _running = running;
                _walkPhase = walkPhase;
                exactPoseRestore = QAPosesMatch();
                exactStateRestore = _walking == walking && _running == running &&
                    Mathf.Abs(_walkPhase - walkPhase) <= 0.000001f;
            }

            bool movementPass = walkMoved >= MinimumObservedMovingBoneCount &&
                runMoved >= MinimumObservedMovingBoneCount;
            bool amplitudePass = runAmplitude > walkAmplitude + 0.01f;
            bool cadencePass = runCadenceAdvance > walkCadenceAdvance + 0.01f;
            bool pass = movementPass && amplitudePass && cadencePass &&
                exactPoseRestore && exactStateRestore;
            ObservedLocomotionQAMarker = pass
                ? ObservedLocomotionReadyMarker
                : ObservedLocomotionIncompleteMarker;
            detail = $"observed={pass} activeBones={activeBones}/" +
                $"{RequiredArticulatedBoneCount} activeVisibleRenderers={activeVisibleRenderers} " +
                $"visibleArticulatedBones={visibleArticulatedBones} " +
                $"movedWalkRun={walkMoved}/{runMoved} " +
                $"amplitudeDegWalkRun={walkAmplitude:0.000}/{runAmplitude:0.000} " +
                $"phaseAdvanceWalkRun={walkCadenceAdvance:0.000}/{runCadenceAdvance:0.000} " +
                $"exactPoseRestore={exactPoseRestore} exactStateRestore={exactStateRestore} " +
                $"marker={ObservedLocomotionQAMarker}";
            return pass;
        }

        public void SetFacingYaw(float yawDegrees)
        {
            _targetFacingYaw = yawDegrees;
        }

        void StartPose(ActionPose pose, float duration) => StartPose(pose, duration, true);

        void StartPose(ActionPose pose, float duration, bool moveRoot)
        {
            _pose = pose;
            _poseT = 0f;
            _poseDur = duration;
            if (moveRoot) StartRoomMove(pose);
        }

        void LateUpdate()
        {
            UpdateRoomMove();
            RestoreBones();

            float t = Time.time;
            bool napping = _pose == ActionPose.Nap;
            float breathe = Mathf.Sin(t * 2.0f) * 0.012f;
            _facingYaw = Mathf.LerpAngle(_facingYaw, _targetFacingYaw, 1f - Mathf.Exp(-Time.deltaTime * 9f));
            float idleBob = napping ? 0.004f : (_walking ? 0.006f : 0.018f);
            transform.localPosition = _basePos + Vector3.up * (Mathf.Sin(t * 2.4f) * idleBob);
            float idleYaw = napping ? 0.25f : (_walking ? Mathf.Sin(t * 4.8f) * 0.7f : Mathf.Sin(t * 1.35f) * 1.8f);
            float idleRoll = napping ? 0.18f : (_walking ? Mathf.Sin(t * 5.0f) * 0.8f : Mathf.Sin(t * 1.1f) * 1.2f);
            transform.localRotation = _baseRot * Quaternion.Euler(0f, _facingYaw + idleYaw, idleRoll);
            if (_hips != null) _hips.localScale *= 1f + breathe;
            float idleLayer = napping ? 0.18f : 1f;
            if (_head != null) Add(_head, Mathf.Sin(t * 0.9f) * 2.0f * idleLayer, Mathf.Sin(t * 1.1f) * 3.0f * idleLayer, 0f);
            if (_leftArm != null) Add(_leftArm, Mathf.Sin(t * 1.2f) * 2.5f * idleLayer, 0f, Mathf.Sin(t * 1.5f) * 3.0f * idleLayer);
            if (_rightArm != null) Add(_rightArm, Mathf.Sin(t * 1.2f + 1.5f) * 2.5f * idleLayer, 0f, Mathf.Sin(t * 1.5f + 1.2f) * -3.0f * idleLayer);
            AnimateRibbons(t, napping ? 0.24f : 1f);
            if (_walking) ApplyWalkCycle(t, Time.deltaTime);

            if (_pose == ActionPose.None) return;

            _poseT += Time.deltaTime;
            float k = Mathf.Clamp01(_poseT / _poseDur);
            float punch = Mathf.Sin(k * Mathf.PI);
            float hold = Mathf.SmoothStep(0f, 1f, Mathf.Sin(k * Mathf.PI));

            ApplyPose(_pose, hold, punch, k);
            if (k >= 1f) _pose = ActionPose.None;
        }

        void ApplyPose(ActionPose pose, float hold, float punch, float k)
        {
            switch (pose)
            {
                case ActionPose.Snack:
                    transform.localPosition += Vector3.up * (0.05f * punch);
                    Add(_head, -10f * hold, 0f, 0f);
                    Add(_leftArm, -38f * hold, -4f * hold, 12f * hold);
                    Add(_leftForeArm, -55f * hold, 0f, 8f * hold);
                    Add(_rightArm, -35f * hold, 4f * hold, -12f * hold);
                    Add(_rightForeArm, -58f * hold, 0f, -8f * hold);
                    break;

                case ActionPose.Hug:
                    transform.localPosition += Vector3.up * (0.045f * punch);
                    Add(_head, 5f * hold, 0f, -7f * hold);
                    Add(_spine, -3f * hold, 0f, 0f);
                    Add(_leftArm, -30f * hold, -20f * hold, 62f * hold);
                    Add(_leftForeArm, -82f * hold, 0f, 34f * hold);
                    Add(_rightArm, -30f * hold, 20f * hold, -62f * hold);
                    Add(_rightForeArm, -82f * hold, 0f, -34f * hold);
                    break;

                case ActionPose.Nap:
                    transform.localRotation *= Quaternion.Euler(0f, 0f, 3.5f * hold);
                    transform.localPosition += Vector3.down * (0.045f * hold);
                    Add(_head, 11f * hold, 0f, 4f * hold);
                    Add(_spine, 5f * hold, 0f, 2.5f * hold);
                    Add(_leftArm, 10f * hold, 0f, 18f * hold);
                    Add(_rightArm, 10f * hold, 0f, -18f * hold);
                    Add(_leftUpLeg, -7f * hold, 0f, 4f * hold);
                    Add(_rightUpLeg, -4f * hold, 0f, -3f * hold);
                    break;

                case ActionPose.Play:
                    transform.localPosition += Vector3.up * (0.16f * punch);
                    Add(_spine, -9f * hold, 0f, 0f);
                    Add(_head, -5f * hold, 0f, 0f);
                    Add(_leftArm, -50f * hold, 0f, 36f * hold);
                    Add(_rightArm, -50f * hold, 0f, -36f * hold);
                    Add(_leftUpLeg, 8f * hold, 0f, -5f * hold);
                    Add(_leftLeg, 18f * hold, 0f, -4f * hold);
                    Add(_rightLeg, -12f * hold, 0f, 4f * hold);
                    break;

                case ActionPose.Bath:
                    transform.localRotation *= Quaternion.Euler(0f, Mathf.Sin(k * Mathf.PI * 6f) * 8f, 0f);
                    Add(_head, 0f, Mathf.Sin(k * Mathf.PI * 6f) * 8f, 0f);
                    Add(_leftArm, -25f * hold, 0f, 38f * hold);
                    Add(_rightArm, -25f * hold, 0f, -38f * hold);
                    Add(_leftForeArm, -48f * hold, 0f, 16f * hold);
                    Add(_rightForeArm, -48f * hold, 0f, -16f * hold);
                    break;

                case ActionPose.Dance:
                    transform.localPosition += new Vector3(Mathf.Sin(k * Mathf.PI * 2f) * 0.10f, 0.10f * punch, 0f);
                    transform.localRotation *= Quaternion.Euler(0f, Mathf.Sin(k * Mathf.PI * 2f) * 22f, Mathf.Sin(k * Mathf.PI * 4f) * 9f);
                    Add(_head, -4f * hold, Mathf.Sin(k * Mathf.PI * 4f) * 8f, 0f);
                    Add(_leftArm, -66f * hold, 0f, 58f * hold);
                    Add(_rightArm, -46f * hold, 0f, -68f * hold);
                    Add(_leftLeg, 9f * hold, 0f, -8f * hold);
                    Add(_rightLeg, -9f * hold, 0f, 8f * hold);
                    break;
            }
        }

        void ApplyProportionPass()
        {
            if (_sdAvatar)
            {
                Scale(_head, Vector3.one * 0.64f);
                Scale(_leftUpLeg, new Vector3(0.90f, 1.24f, 0.90f));
                Scale(_rightUpLeg, new Vector3(0.90f, 1.24f, 0.90f));
                Scale(_leftLeg, new Vector3(0.90f, 1.18f, 0.90f));
                Scale(_rightLeg, new Vector3(0.90f, 1.18f, 0.90f));
                Scale(_leftFoot, Vector3.one * 0.84f);
                Scale(_rightFoot, Vector3.one * 0.84f);
            }
            else
            {
                Scale(_head, Vector3.one * 1.07f);
                Scale(_leftArm, new Vector3(0.95f, 0.94f, 0.95f));
                Scale(_rightArm, new Vector3(0.95f, 0.94f, 0.95f));
                Scale(_leftForeArm, new Vector3(0.95f, 0.94f, 0.95f));
                Scale(_rightForeArm, new Vector3(0.95f, 0.94f, 0.95f));
                Scale(_leftUpLeg, new Vector3(0.96f, 0.92f, 0.96f));
                Scale(_rightUpLeg, new Vector3(0.96f, 0.92f, 0.96f));
                Scale(_leftLeg, new Vector3(0.96f, 0.94f, 0.96f));
                Scale(_rightLeg, new Vector3(0.96f, 0.94f, 0.96f));
                Scale(_leftFoot, Vector3.one * 0.88f);
                Scale(_rightFoot, Vector3.one * 0.88f);
            }

            if (_ribbons == null) return;
            for (int i = 0; i < _ribbons.Length; i++)
                Scale(_ribbons[i], Vector3.one * (_sdAvatar ? 0.52f : 0.60f));
        }

        void CacheBasePoses()
        {
            var poses = new List<BonePose>
            {
                Save(_hips), Save(_spine), Save(_head),
                Save(_leftArm), Save(_leftForeArm), Save(_rightArm), Save(_rightForeArm),
                Save(_leftUpLeg), Save(_rightUpLeg),
                Save(_leftLeg), Save(_rightLeg), Save(_leftFoot), Save(_rightFoot),
            };

            if (_ribbons != null)
            {
                for (int i = 0; i < _ribbons.Length; i++)
                    poses.Add(Save(_ribbons[i]));
            }

            _poses = poses.ToArray();
        }

        void StartRoomMove(ActionPose pose)
        {
            if (_root == null || _root == transform) return;

            _roomFrom = _root.position;
            _roomTarget = _roomAnchor + RoomOffset(pose);
            _roomTarget.y = _roomAnchor.y;
            _roomT = 0f;
            _roomDur = pose == ActionPose.Dance ? 0.62f : 0.48f;
            _roomMoving = true;
        }

        Vector3 RoomOffset(ActionPose pose)
        {
            switch (pose)
            {
                case ActionPose.Snack: return new Vector3(0.14f, 0f, 0.02f);
                case ActionPose.Hug:   return new Vector3(-0.16f, 0f, 0.04f);
                case ActionPose.Nap:   return new Vector3(0.34f, 0f, 0.10f);
                case ActionPose.Play:  return new Vector3(-0.34f, 0f, 0.00f);
                case ActionPose.Bath:  return new Vector3(0.00f, 0f, 0.18f);
                case ActionPose.Dance: return new Vector3(0.24f, 0f, -0.08f);
                default:               return Vector3.zero;
            }
        }

        void UpdateRoomMove()
        {
            if (!_roomMoving || _root == null) return;

            _roomT += Time.deltaTime;
            float k = Mathf.Clamp01(_roomT / Mathf.Max(0.01f, _roomDur));
            float ease = Mathf.SmoothStep(0f, 1f, k);
            _root.position = Vector3.Lerp(_roomFrom, _roomTarget, ease);
            if (k >= 1f) _roomMoving = false;
        }

        void AnimateRibbons(float t, float amount)
        {
            if (_ribbons == null) return;
            for (int i = 0; i < _ribbons.Length; i++)
            {
                var ribbon = _ribbons[i];
                if (ribbon == null) continue;
                float side = i < 4 ? 1f : -1f;
                Add(ribbon, Mathf.Sin(t * 1.9f + i * 0.55f) * 2.0f * amount, 0f, side * Mathf.Sin(t * 2.2f + i) * 2.4f * amount);
            }
        }

        void PrepareLocomotionSample(bool running, float phase)
        {
            RestoreBones();
            transform.localPosition = _basePos;
            transform.localRotation = _baseRot;
            _walking = true;
            _running = running;
            _walkPhase = phase;
        }

        int CountActiveArticulatedBones()
        {
            if (_articulatedBones == null) return 0;
            int active = 0;
            for (int i = 0; i < _articulatedBones.Length; i++)
            {
                Transform bone = _articulatedBones[i];
                if (bone != null && bone.gameObject.activeInHierarchy) active++;
            }
            return active;
        }

        int CountActiveVisibleRenderers()
        {
            if (_renderers == null) return 0;
            int active = 0;
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (IsActiveVisibleGeometry(_renderers[i])) active++;
            }
            return active;
        }

        int CountVisibleArticulatedBones()
        {
            if (_articulatedBones == null) return 0;
            int visible = 0;
            for (int i = 0; i < _articulatedBones.Length; i++)
            {
                if (IsBoneDrivingActiveVisibleGeometry(_articulatedBones[i])) visible++;
            }
            return visible;
        }

        bool IsBoneDrivingActiveVisibleGeometry(Transform bone)
        {
            if (bone == null || !bone.gameObject.activeInHierarchy || _renderers == null)
                return false;
            for (int rendererIndex = 0; rendererIndex < _renderers.Length; rendererIndex++)
            {
                if (_renderers[rendererIndex] is not SkinnedMeshRenderer skinnedRenderer ||
                    !IsActiveVisibleGeometry(skinnedRenderer))
                    continue;
                Transform[] bones = skinnedRenderer.bones;
                for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
                {
                    if (bones[boneIndex] == bone) return true;
                }
            }
            return false;
        }

        static bool IsActiveVisibleGeometry(Renderer renderer)
        {
            if (renderer == null || !renderer.enabled || renderer.forceRenderingOff ||
                !renderer.gameObject.activeInHierarchy ||
                renderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly)
                return false;
            if (renderer is SkinnedMeshRenderer skinnedRenderer)
                return skinnedRenderer.sharedMesh != null &&
                    skinnedRenderer.sharedMesh.vertexCount > 0;
            if (renderer is not MeshRenderer) return false;

            var meshFilter = renderer.GetComponent<MeshFilter>();
            return meshFilter != null && meshFilter.sharedMesh != null &&
                meshFilter.sharedMesh.vertexCount > 0;
        }

        void CaptureQAPoses()
        {
            _qaPoseSnapshot[0] = CaptureTransformPose(transform);
            for (int i = 0; i < _poses.Length; i++)
                _qaPoseSnapshot[i + 1] = CaptureTransformPose(_poses[i].bone);
        }

        static TransformPose CaptureTransformPose(Transform target) => new TransformPose
        {
            transform = target,
            position = target != null ? target.localPosition : Vector3.zero,
            rotation = target != null ? target.localRotation : Quaternion.identity,
            scale = target != null ? target.localScale : Vector3.one
        };

        void RestoreQAPoses()
        {
            for (int i = 0; i < _qaPoseSnapshot.Length; i++)
            {
                TransformPose pose = _qaPoseSnapshot[i];
                if (pose.transform == null) continue;
                pose.transform.localPosition = pose.position;
                pose.transform.localRotation = pose.rotation;
                pose.transform.localScale = pose.scale;
            }
        }

        bool QAPosesMatch()
        {
            for (int i = 0; i < _qaPoseSnapshot.Length; i++)
            {
                TransformPose pose = _qaPoseSnapshot[i];
                if (pose.transform == null) continue;
                if (Vector3.Distance(pose.transform.localPosition, pose.position) > 0.0000001f ||
                    Quaternion.Angle(pose.transform.localRotation, pose.rotation) > 0.0001f ||
                    Vector3.Distance(pose.transform.localScale, pose.scale) > 0.0000001f)
                    return false;
            }
            return true;
        }

        int CountMovedArticulatedBones()
        {
            int moved = 0;
            for (int i = 0; i < _articulatedBones.Length; i++)
            {
                Transform bone = _articulatedBones[i];
                if (!IsBoneDrivingActiveVisibleGeometry(bone)) continue;
                Quaternion baseRotation = BaseRotationFor(bone);
                if (Quaternion.Angle(bone.localRotation, baseRotation) > 0.001f) moved++;
            }
            return moved;
        }

        float MaximumArticulatedBoneRotation()
        {
            float maximum = 0f;
            for (int i = 0; i < _articulatedBones.Length; i++)
            {
                Transform bone = _articulatedBones[i];
                if (!IsBoneDrivingActiveVisibleGeometry(bone)) continue;
                maximum = Mathf.Max(maximum,
                    Quaternion.Angle(bone.localRotation, BaseRotationFor(bone)));
            }
            return maximum;
        }

        Quaternion BaseRotationFor(Transform bone)
        {
            for (int i = 0; i < _poses.Length; i++)
            {
                if (_poses[i].bone == bone) return _poses[i].baseRot;
            }
            return bone.localRotation;
        }

        void ApplyWalkCycle(float t, float deltaTime)
        {
            float speed = _running ? 8.0f : 5.2f;
            float stride = _running ? 30f : 18f;
            float lift = _running ? 0.050f : 0.024f;
            _walkPhase += deltaTime * speed;
            float s = Mathf.Sin(_walkPhase);
            float c = Mathf.Cos(_walkPhase);
            float footA = Mathf.Max(0f, s);
            float footB = Mathf.Max(0f, -s);
            float landing = Mathf.Pow(Mathf.Abs(c), 1.7f);
            float doubleStep = Mathf.Sin(_walkPhase * 2f);

            transform.localPosition += new Vector3(0f, landing * lift, doubleStep * (_running ? 0.010f : 0.005f));
            transform.localRotation *= Quaternion.Euler(-1.0f - landing * 0.8f, c * (_running ? 2.1f : 1.0f), s * (_running ? 2.4f : 1.5f));
            Add(_spine, -2.4f - landing * 0.8f, 0f, s * 1.7f);
            Add(_leftUpLeg, stride * s, 0f, 0f);
            Add(_rightUpLeg, -stride * s, 0f, 0f);
            Add(_leftLeg, -stride * 0.42f * footB, 0f, 0f);
            Add(_rightLeg, -stride * 0.42f * footA, 0f, 0f);
            Add(_leftFoot, stride * 0.18f * footA, 0f, 0f);
            Add(_rightFoot, stride * 0.18f * footB, 0f, 0f);
            Add(_leftArm, -stride * 0.55f * s, 0f, 5f + c * 1.4f);
            Add(_rightArm, stride * 0.55f * s, 0f, -5f + c * 1.4f);
            Add(_head, -landing * 0.9f, -s * 0.8f, s * 1.0f);
        }

        void RestoreBones()
        {
            if (_poses == null) return;
            for (int i = 0; i < _poses.Length; i++)
            {
                if (_poses[i].bone == null) continue;
                _poses[i].bone.localRotation = _poses[i].baseRot;
                _poses[i].bone.localScale = _poses[i].baseScale;
            }
        }

        void Add(Transform bone, float x, float y, float z)
        {
            if (bone == null) return;
            bone.localRotation *= Quaternion.Euler(x, y, z);
        }

        void Scale(Transform bone, Vector3 multiplier)
        {
            if (bone == null) return;
            bone.localScale = Vector3.Scale(bone.localScale, multiplier);
        }

        BonePose Save(Transform bone) =>
            new BonePose
            {
                bone = bone,
                baseRot = bone != null ? bone.localRotation : Quaternion.identity,
                baseScale = bone != null ? bone.localScale : Vector3.one,
            };

        Transform FindBone(string suffix)
        {
            var all = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                string n = all[i].name;
                if (n == suffix || n.EndsWith("_" + suffix) || n.EndsWith(":" + suffix))
                    return all[i];
            }
            return null;
        }
    }
}
