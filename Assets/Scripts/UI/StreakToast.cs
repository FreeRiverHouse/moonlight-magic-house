using System.Collections;
using UnityEngine;
using TMPro;

namespace MoonlightMagicHouse
{
    public class StreakToast : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text   streakLabel;
        [SerializeField] TMP_Text   coinsLabel;
        [SerializeField] float      displayTime = 4f;

        void Start()
        {
            root.SetActive(false);
            StreakTracker.Instance?.onDailyLogin.AddListener(Show);
        }

        public void Show(int streak, int coins)
        {
            streakLabel.text = $"🌙 Day {streak} streak!";
            coinsLabel.text  = $"+{coins} ⭐";
            StartCoroutine(Display());
        }

        IEnumerator Display()
        {
            root.SetActive(true);
            yield return new WaitForSeconds(displayTime);
            root.SetActive(false);
        }
    }
}
