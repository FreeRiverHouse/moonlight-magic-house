using UnityEngine;

namespace MoonlightMagicHouse
{
    [RequireComponent(typeof(Animator))]
    public class PetAnimatorController : MonoBehaviour
    {
        static readonly int MoodHash  = Animator.StringToHash("Mood");
        static readonly int StageHash = Animator.StringToHash("Stage");
        static readonly int EatHash   = Animator.StringToHash("Eat");
        static readonly int PlayHash  = Animator.StringToHash("Play");
        static readonly int SleepHash = Animator.StringToHash("Sleep");
        static readonly int EvolveHash = Animator.StringToHash("Evolve");

        Animator _anim;
        MoonlightPet _pet;

        void Awake() => _anim = GetComponent<Animator>();

        void Start()
        {
            _pet = GetComponentInParent<MoonlightPet>();
            if (_pet == null) return;
            _pet.onEvolution.AddListener(OnEvolve);
            _pet.onMoodChange.AddListener(OnMoodChange);
        }

        void Update()
        {
            if (_pet == null) return;
            _anim.SetInteger(StageHash, (int)_pet.stage);
            _anim.SetInteger(MoodHash,  (int)_pet.stats.GetMood());
        }

        public void TriggerEat()   => _anim.SetTrigger(EatHash);
        public void TriggerPlay()  => _anim.SetTrigger(PlayHash);
        public void TriggerSleep() => _anim.SetTrigger(SleepHash);

        void OnEvolve(EvolutionStage stage) => _anim.SetTrigger(EvolveHash);
        void OnMoodChange(PetMood mood) { } // handled via Update
    }
}
