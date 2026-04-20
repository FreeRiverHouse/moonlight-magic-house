using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoonlightMagicHouse
{
    [System.Serializable]
    public class MoonlightTrick
    {
        public string id;
        public string displayName;
        public string description;
        public int    xpRequired;
        public string animTrigger;  // Animator trigger name
        public int    coinReward;
    }

    public class MoonlightTricksSystem : MonoBehaviour
    {
        public static MoonlightTricksSystem Instance { get; private set; }

        [SerializeField] List<MoonlightTrick> tricks;
        public UnityEvent<MoonlightTrick> onTrickLearned;
        public UnityEvent<MoonlightTrick> onTrickPerformed;

        MoonlightCharacter _ml;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            _ml = MoonlightGameManager.Instance?.moonlight;
            if (_ml != null) _ml.onXPGained.AddListener(CheckNewTricks);
        }

        void CheckNewTricks(int xp)
        {
            foreach (var trick in tricks)
            {
                string pref = $"trick_learned_{trick.id}";
                if (PlayerPrefs.GetInt(pref, 0) == 0 && xp >= trick.xpRequired)
                {
                    PlayerPrefs.SetInt(pref, 1);
                    onTrickLearned?.Invoke(trick);
                    AudioManager.Instance?.Play("trick_learned");
                }
            }
        }

        public void Perform(string id)
        {
            if (!IsLearned(id)) return;
            var trick = tricks.Find(t => t.id == id);
            if (trick == null) return;

            var anim = MoonlightGameManager.Instance?.moonlight.GetComponentInChildren<MoonlightAnimator>();
            // anim?.TriggerCustom(trick.animTrigger);  // extend animator as needed

            _ml?.EarnCoins(trick.coinReward);
            onTrickPerformed?.Invoke(trick);
            AudioManager.Instance?.Play("trick");
        }

        public bool IsLearned(string id) =>
            PlayerPrefs.GetInt($"trick_learned_{id}", 0) == 1;

        public List<MoonlightTrick> LearnedTricks() =>
            tricks.FindAll(t => IsLearned(t.id));
    }
}
