using System.Collections;
using UnityEngine;
using TMPro;

namespace MoonlightMagicHouse
{
    public class SecretDiscoveryToast : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text   descLabel;
        [SerializeField] TMP_Text   rewardLabel;
        [SerializeField] float      displayTime = 4f;

        Coroutine _co;

        void Start()
        {
            root.SetActive(false);
            var secrets = FindAnyObjectByType<HouseSecrets>();
            secrets?.onDiscovered.AddListener(Show);
        }

        public void Show(Secret s)
        {
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(Display(s));
        }

        IEnumerator Display(Secret s)
        {
            descLabel.text   = $"🌟 Secret found!\n{s.description}";
            rewardLabel.text = $"+{s.coinReward} ⭐  +{s.magicReward} magic";
            root.SetActive(true);
            yield return new WaitForSeconds(displayTime);
            root.SetActive(false);
        }
    }
}
