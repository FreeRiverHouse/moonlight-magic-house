using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlightMagicHouse
{
    public class MoonlightUI : MonoBehaviour
    {
        // ── Stat bars ─────────────────────────────────────────────────────
        [Header("Stats")]
        [SerializeField] Slider wonderBar;
        [SerializeField] Slider warmthBar;
        [SerializeField] Slider restBar;
        [SerializeField] Slider magicBar;
        [SerializeField] Slider hungerBar;

        // ── Info strip ────────────────────────────────────────────────────
        [Header("Info")]
        [SerializeField] TMP_Text stageLabel;
        [SerializeField] TMP_Text coinsLabel;
        [SerializeField] TMP_Text xpLabel;
        [SerializeField] TMP_Text moodEmoji;
        [SerializeField] TMP_Text daysLabel;

        // ── Action buttons ────────────────────────────────────────────────
        [Header("Actions")]
        [SerializeField] Button feedBtn;
        [SerializeField] Button cuddleBtn;
        [SerializeField] Button sleepBtn;

        // ── Overlays ──────────────────────────────────────────────────────
        [Header("Overlays")]
        [SerializeField] GameObject stagePanel;
        [SerializeField] TMP_Text   stagePanelLabel;
        [SerializeField] GameObject roomUnlockPanel;
        [SerializeField] TMP_Text   roomUnlockLabel;
        [SerializeField] GameObject offlinePanel;
        [SerializeField] GameObject sleepOverlay;

        // ── Prompt ────────────────────────────────────────────────────────
        [Header("Prompt")]
        [SerializeField] GameObject promptRoot;
        [SerializeField] TMP_Text   promptLabel;

        public void ShowPrompt(string text)
        {
            if (promptRoot == null) return;
            promptLabel.text = text;
            promptRoot.SetActive(true);
        }

        public void HidePrompt()
        {
            promptRoot?.SetActive(false);
        }

        // ── Food menu ─────────────────────────────────────────────────────
        [Header("Feed Menu")]
        [SerializeField] GameObject  feedMenuRoot;
        [SerializeField] Transform   feedMenuContent;
        [SerializeField] GameObject  feedItemPrefab;
        [SerializeField] FoodItem[]  foodCatalogue;

        static readonly string[] MoodEmojis = { "😴", "😠", "😑", "🌸", "✨", "🌟" };
        static readonly string[] StageNames =
        {
            "Moonbud", "Starling", "Luminary", "Sorceress", "Moonkeeper"
        };
        static readonly string[] RoomNames =
        {
            "", "Living Room", "Kitchen", "Bedroom", "Garden", "Library"
        };

        void Start()
        {
            cuddleBtn.onClick.AddListener(() =>
            {
                MoonlightGameManager.Instance.moonlight.Cuddle();
                Refresh(MoonlightGameManager.Instance.moonlight);
            });
            sleepBtn.onClick.AddListener(() =>
            {
                MoonlightGameManager.Instance.moonlight.PutToSleep();
                StartCoroutine(ShowThenHide(sleepOverlay, 2f));
                Refresh(MoonlightGameManager.Instance.moonlight);
            });
            feedBtn.onClick.AddListener(OpenFeedMenu);
        }

        public void Refresh(MoonlightCharacter m)
        {
            wonderBar.value = m.stats.wonder / 100f;
            warmthBar.value = m.stats.warmth / 100f;
            restBar.value   = m.stats.rest   / 100f;
            magicBar.value  = m.stats.magic  / 100f;
            hungerBar.value = m.stats.hunger / 100f;

            stageLabel.text = StageNames[(int)m.stage];
            coinsLabel.text = $"⭐ {m.coins}";
            xpLabel.text    = $"XP {m.xp}";
            daysLabel.text  = $"Day {Mathf.FloorToInt(m.daysInHouse) + 1}";
            moodEmoji.text  = MoodEmojis[(int)m.stats.GetMood()];
        }

        public void OnMoodChange(MoonlightMood mood) => moodEmoji.text = MoodEmojis[(int)mood];
        public void UpdateCoins(int coins)            => coinsLabel.text = $"⭐ {coins}";
        public void UpdateXP(int xp)                  => xpLabel.text = $"XP {xp}";

        public void ShowStageCelebration(MoonlightStage stage)
        {
            stagePanelLabel.text = $"Moonlight became a {StageNames[(int)stage]}! ✨";
            StartCoroutine(ShowThenHide(stagePanel, 4f));
        }

        public void ShowRoomUnlocked(int count)
        {
            roomUnlockLabel.text = $"New room unlocked: {RoomNames[count]}! 🌙";
            StartCoroutine(ShowThenHide(roomUnlockPanel, 3f));
        }

        public void ShowOfflineNotice() => StartCoroutine(ShowThenHide(offlinePanel, 3f));

        public void OpenFeedMenuWith(System.Collections.Generic.List<FoodItem> overrideCatalogue)
        {
            PopulateFeedMenu(overrideCatalogue);
            feedMenuRoot.SetActive(true);
        }

        void OpenFeedMenu()
        {
            PopulateFeedMenu(new System.Collections.Generic.List<FoodItem>(foodCatalogue));
            feedMenuRoot.SetActive(true);
        }

        void PopulateFeedMenu(System.Collections.Generic.List<FoodItem> catalogue)
        {
            foreach (Transform t in feedMenuContent) Destroy(t.gameObject);
            foreach (var food in catalogue)
            {
                var item = Instantiate(feedItemPrefab, feedMenuContent);
                item.GetComponentInChildren<TMP_Text>().text = $"{food.itemName}\n⭐{food.cost}";
                var captured = food;
                item.GetComponent<Button>().onClick.AddListener(() =>
                {
                    MoonlightGameManager.Instance.moonlight.Feed(captured);
                    Refresh(MoonlightGameManager.Instance.moonlight);
                    feedMenuRoot.SetActive(false);
                });
            }
        }

        IEnumerator ShowThenHide(GameObject panel, float dur)
        {
            panel.SetActive(true);
            yield return new WaitForSeconds(dur);
            panel.SetActive(false);
        }
    }
}
