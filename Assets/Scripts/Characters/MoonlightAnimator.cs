using UnityEngine;

namespace MoonlightMagicHouse
{
    [RequireComponent(typeof(Animator))]
    public class MoonlightAnimator : MonoBehaviour
    {
        static readonly int MoodHash   = Animator.StringToHash("Mood");
        static readonly int StageHash  = Animator.StringToHash("Stage");
        static readonly int EatHash    = Animator.StringToHash("Eat");
        static readonly int CuddleHash = Animator.StringToHash("Cuddle");
        static readonly int SleepHash  = Animator.StringToHash("Sleep");
        static readonly int StageUpHash= Animator.StringToHash("StageUp");
        static readonly int WalkHash   = Animator.StringToHash("Walk");
        static readonly int DanceHash  = Animator.StringToHash("Dance");

        Animator           _anim;
        MoonlightCharacter _ml;

        void Awake() => _anim = GetComponent<Animator>();

        void Start()
        {
            _ml = GetComponentInParent<MoonlightCharacter>();
            if (_ml == null) return;
            _ml.onStageUp.AddListener(_ => _anim.SetTrigger(StageUpHash));
            _ml.onMoodChange.AddListener(m =>
            {
                _anim.SetInteger(MoodHash, (int)m);
                if (m == MoonlightMood.Radiant) _anim.SetTrigger(DanceHash);
            });
        }

        void Update()
        {
            if (_ml == null) return;
            _anim.SetInteger(StageHash, (int)_ml.stage);
        }

        public void TriggerEat()    => _anim.SetTrigger(EatHash);
        public void TriggerCuddle() => _anim.SetTrigger(CuddleHash);
        public void TriggerSleep()  => _anim.SetTrigger(SleepHash);
        public void SetWalking(bool walking) => _anim.SetBool(WalkHash, walking);
    }
}
