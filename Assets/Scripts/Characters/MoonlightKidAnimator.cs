using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    // Lightweight runtime posing for the SD Moonlight avatar. This keeps the
    // proportions childlike and gives every care button a visible reaction.
    public class MoonlightKidAnimator : MonoBehaviour
    {
        enum ActionPose { None, Snack, Hug, Nap, Play, Bath, Dance }

        Transform _root;
        Transform _hips, _spine, _head;
        Transform _leftArm, _leftForeArm, _rightArm, _rightForeArm;
        Transform _leftUpLeg, _rightUpLeg;
        Transform _leftLeg, _rightLeg, _leftFoot, _rightFoot;
        Transform[] _ribbons;

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
        bool _roomMoving;
        bool _walking;
        bool _running;
        float _facingYaw;

        struct BonePose
        {
            public Transform bone;
            public Quaternion baseRot;
            public Vector3 baseScale;
        }

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
            _ribbons = new[]
            {
                FindBone("J_L_HeadRibbon_00"), FindBone("J_L_HeadRibbon_01"),
                FindBone("J_L_HeadRibbon_02"), FindBone("J_L_HeadRibbon_03"),
                FindBone("J_R_HeadRibbon_00"), FindBone("J_R_HeadRibbon_01"),
                FindBone("J_R_HeadRibbon_02"), FindBone("J_R_HeadRibbon_03"),
            };

            ApplyProportionPass();
            CacheBasePoses();
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
            _walking = walking;
            _running = walking && running;
        }

        public void SetFacingYaw(float yawDegrees)
        {
            _facingYaw = yawDegrees;
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
            float breathe = Mathf.Sin(t * 2.0f) * 0.012f;
            transform.localPosition = _basePos + Vector3.up * (Mathf.Sin(t * 2.4f) * 0.018f);
            transform.localRotation = _baseRot * Quaternion.Euler(0f, _facingYaw + Mathf.Sin(t * 1.35f) * 1.8f, Mathf.Sin(t * 1.1f) * 1.2f);
            if (_hips != null) _hips.localScale *= 1f + breathe;
            if (_head != null) Add(_head, Mathf.Sin(t * 0.9f) * 2.0f, Mathf.Sin(t * 1.1f) * 3.0f, 0f);
            if (_leftArm != null) Add(_leftArm, Mathf.Sin(t * 1.2f) * 2.5f, 0f, Mathf.Sin(t * 1.5f) * 3.0f);
            if (_rightArm != null) Add(_rightArm, Mathf.Sin(t * 1.2f + 1.5f) * 2.5f, 0f, Mathf.Sin(t * 1.5f + 1.2f) * -3.0f);
            AnimateRibbons(t);
            if (_walking) ApplyWalkCycle(t);

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
                    transform.localRotation *= Quaternion.Euler(0f, 0f, 7f * hold);
                    transform.localPosition += Vector3.down * (0.02f * hold);
                    Add(_head, 16f * hold, 0f, 8f * hold);
                    Add(_spine, 8f * hold, 0f, 5f * hold);
                    Add(_leftArm, 18f * hold, 0f, 14f * hold);
                    Add(_rightArm, 18f * hold, 0f, -14f * hold);
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
            Scale(_head, Vector3.one * 0.76f);
            Scale(_leftUpLeg, new Vector3(0.90f, 1.16f, 0.90f));
            Scale(_rightUpLeg, new Vector3(0.90f, 1.16f, 0.90f));
            Scale(_leftLeg, new Vector3(0.90f, 1.12f, 0.90f));
            Scale(_rightLeg, new Vector3(0.90f, 1.12f, 0.90f));
            Scale(_leftFoot, Vector3.one * 0.88f);
            Scale(_rightFoot, Vector3.one * 0.88f);

            if (_ribbons == null) return;
            for (int i = 0; i < _ribbons.Length; i++)
                Scale(_ribbons[i], Vector3.one * 0.60f);
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

        void AnimateRibbons(float t)
        {
            if (_ribbons == null) return;
            for (int i = 0; i < _ribbons.Length; i++)
            {
                var ribbon = _ribbons[i];
                if (ribbon == null) continue;
                float side = i < 4 ? 1f : -1f;
                Add(ribbon, Mathf.Sin(t * 1.9f + i * 0.55f) * 2.0f, 0f, side * Mathf.Sin(t * 2.2f + i) * 2.4f);
            }
        }

        void ApplyWalkCycle(float t)
        {
            float speed = _running ? 8.0f : 5.2f;
            float stride = _running ? 30f : 18f;
            float lift = _running ? 0.060f : 0.032f;
            float s = Mathf.Sin(t * speed);
            float c = Mathf.Cos(t * speed);

            transform.localPosition += Vector3.up * (Mathf.Abs(c) * lift);
            Add(_spine, -3f, 0f, s * 2.5f);
            Add(_leftUpLeg, stride * s, 0f, 0f);
            Add(_rightUpLeg, -stride * s, 0f, 0f);
            Add(_leftLeg, -stride * 0.55f * Mathf.Max(0f, -s), 0f, 0f);
            Add(_rightLeg, -stride * 0.55f * Mathf.Max(0f, s), 0f, 0f);
            Add(_leftArm, -stride * 0.62f * s, 0f, 9f);
            Add(_rightArm, stride * 0.62f * s, 0f, -9f);
            Add(_head, Mathf.Abs(s) * -1.5f, 0f, s * 2f);
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
