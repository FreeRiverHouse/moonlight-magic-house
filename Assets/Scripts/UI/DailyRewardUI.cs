using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlightMagicHouse
{
    public class DailyRewardUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Button claimBtn;
        [SerializeField] TMP_Text streakLabel;
        [SerializeField] TMP_Text coinsLabel;
        [SerializeField] TMP_Text cooldownLabel;
        [SerializeField] GameObject[] dayDots;          // 7 dots
        [SerializeField] Color dotActiveColor  = new Color(0.95f, 0.80f, 0.20f);
        [SerializeField] Color dotInactiveColor = new Color(0.3f, 0.2f, 0.4f);

        DailyRewardManager _mgr;
        MoonlightPet _pet;
        Coroutine _cooldownCo;

        void Start()
        {
            _mgr = GameManager.Instance.GetComponent<DailyRewardManager>();
            _pet = GameManager.Instance.pet;

            claimBtn.onClick.AddListener(Claim);
            _mgr.onRewardClaimed.AddListener(OnClaimed);

            Refresh();

            if (_mgr.CanClaim())
                root.SetActive(true);
        }

        void Refresh()
        {
            int streak = _mgr.GetStreak();
            streakLabel.text = $"Day {streak} streak!";
            for (int i = 0; i < dayDots.Length; i++)
            {
                var img = dayDots[i].GetComponent<Image>();
                if (img) img.color = i < streak ? dotActiveColor : dotInactiveColor;
            }
            claimBtn.interactable = _mgr.CanClaim();
        }

        void Claim()
        {
            _mgr.Claim(_pet);
        }

        void OnClaimed(int coins, int streak)
        {
            coinsLabel.text = $"+{coins} ⭐";
            streakLabel.text = $"Day {streak} streak!";
            claimBtn.interactable = false;
            Refresh();
            if (_cooldownCo != null) StopCoroutine(_cooldownCo);
            _cooldownCo = StartCoroutine(HideAfterDelay(3f));
        }

        IEnumerator HideAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            root.SetActive(false);
        }
    }
}
