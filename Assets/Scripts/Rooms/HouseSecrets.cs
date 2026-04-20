using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoonlightMagicHouse
{
    // Each room can have hidden interactable spots that Moonlight discovers
    // as her stage increases — raising her wonder stat and awarding coins.

    [System.Serializable]
    public class Secret
    {
        public string          id;
        public string          description;
        public MoonlightStage  requiredStage;
        public int             coinReward;
        public int             magicReward;
        public GameObject      vfxObject;    // activated on discovery
        public AudioClip       discoveryClip;
    }

    public class HouseSecrets : MonoBehaviour
    {
        [SerializeField] List<Secret> secrets;
        public UnityEvent<Secret> onDiscovered;

        readonly HashSet<string> _found = new();

        void Start()
        {
            // Load already-found secrets
            foreach (var s in secrets)
                if (PlayerPrefs.GetInt($"secret_{s.id}", 0) == 1)
                    _found.Add(s.id);
        }

        public void TryReveal(string id)
        {
            var secret = secrets.Find(s => s.id == id);
            if (secret == null || _found.Contains(id)) return;

            var ml = MoonlightGameManager.Instance?.moonlight;
            if (ml == null) return;
            if (ml.stage < secret.requiredStage) return;  // not enough magic yet

            _found.Add(id);
            PlayerPrefs.SetInt($"secret_{id}", 1);

            ml.EarnCoins(secret.coinReward);
            ml.stats.magic  = Mathf.Min(100, ml.stats.magic  + secret.magicReward);
            ml.stats.wonder = Mathf.Min(100, ml.stats.wonder  + 20f);

            if (secret.vfxObject) StartCoroutine(ShowVFX(secret.vfxObject));
            if (secret.discoveryClip) AudioManager.Instance?.Play("discover");

            onDiscovered?.Invoke(secret);
            AchievementSystem.Instance?.OnSecretFound(_found.Count);
            if (AllFound) AchievementSystem.Instance?.OnAllSecretsFound();
        }

        IEnumerator ShowVFX(GameObject vfx)
        {
            vfx.SetActive(true);
            yield return new WaitForSeconds(3f);
            vfx.SetActive(false);
        }

        public int TotalSecrets  => secrets.Count;
        public int FoundSecrets  => _found.Count;
        public bool AllFound     => _found.Count >= secrets.Count;
    }
}
