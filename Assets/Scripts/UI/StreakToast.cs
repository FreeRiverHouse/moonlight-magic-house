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
            if (root == null)
            {
                enabled = false;
                return;
            }

            root.SetActive(false);
            StreakTracker.Instance?.onDailyLogin?.AddListener(Show);
        }

        public void Show(int streak, int coins)
        {
            if (streakLabel != null) streakLabel.text = $"🌙 Day {streak} streak!";
            if (coinsLabel != null) coinsLabel.text = $"+{coins} ⭐";
            if (root != null) StartCoroutine(Display());
        }

        IEnumerator Display()
        {
            if (root == null) yield break;

            root.SetActive(true);
            yield return new WaitForSeconds(displayTime);
            if (root != null) root.SetActive(false);
        }
    }
}
