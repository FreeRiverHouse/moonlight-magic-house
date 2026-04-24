using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlightMagicHouse
{
    public class MoonlightUI : MonoBehaviour
    {
        // Stat bars
        public Slider wonderBar;
        public Slider warmthBar;
        public Slider restBar;
        public Slider magicBar;
        public Slider hungerBar;

        // Info strip
        public TMP_Text stageLabel;
        public TMP_Text coinsLabel;
        public TMP_Text xpLabel;
        public TMP_Text moodEmoji;
        public TMP_Text daysLabel;

        // Action buttons
        public Button feedBtn;
        public Button cuddleBtn;
        public Button sleepBtn;

        // Overlays
        public GameObject stagePanel;
        public TMP_Text   stagePanelLabel;
        public GameObject roomUnlockPanel;
        public TMP_Text   roomUnlockLabel;
        public GameObject offlinePanel;
        public GameObject sleepOverlay;

        // Prompt
        public GameObject promptRoot;
        public TMP_Text   promptLabel;

        // Feed menu
        public GameObject  feedMenuRoot;
        public Transform   feedMenuContent;
        public FoodItem[]  foodCatalogue;

        static readonly string[] MoodEmojis = { "😴", "😠", "😑", "🌸", "✨", "🌟" };
        // The character's name is Moonlight. "Stage" is still tracked internally for evolution/achievements,
        // but the HUD shows her name + stage descriptor rather than the raw stage codename ("Moonbud").
        static readonly string[] StageNames       = { "Moonlight", "Moonlight", "Moonlight", "Moonlight", "Moonlight" };
        static readonly string[] StageDescriptors = { "Sprout",    "Starling",  "Luminary",  "Sorceress", "Moonkeeper" };
        static readonly string[] RoomNames  = { "", "Living Room", "Kitchen", "Bedroom", "Garden", "Library" };

        void Start()
        {
            if (cuddleBtn) cuddleBtn.onClick.AddListener(() =>
            {
                MoonlightGameManager.Instance?.moonlight.Cuddle();
                if (MoonlightGameManager.Instance?.moonlight != null)
                    Refresh(MoonlightGameManager.Instance.moonlight);
            });
            if (sleepBtn) sleepBtn.onClick.AddListener(() =>
            {
                MoonlightGameManager.Instance?.moonlight.PutToSleep();
                if (sleepOverlay) StartCoroutine(ShowThenHide(sleepOverlay, 2f));
                if (MoonlightGameManager.Instance?.moonlight != null)
                    Refresh(MoonlightGameManager.Instance.moonlight);
            });
            if (feedBtn) feedBtn.onClick.AddListener(OpenFeedMenu);
        }

        // Called by MoonlightHouseSetup to inject all UI refs programmatically
        public void Wire(
            Slider wonder, Slider warmth, Slider rest, Slider magic, Slider hunger,
            TMP_Text stage, TMP_Text coins, TMP_Text xp, TMP_Text mood, TMP_Text days,
            Button feed, Button cuddle, Button sleep,
            GameObject stgPanel, TMP_Text stgLabel,
            GameObject roomPanel, TMP_Text roomLabel,
            GameObject offline, GameObject sleepOvr,
            GameObject feedRoot, Transform feedContent)
        {
            wonderBar = wonder; warmthBar = warmth; restBar = rest;
            magicBar  = magic;  hungerBar = hunger;
            stageLabel = stage; coinsLabel = coins; xpLabel = xp;
            moodEmoji  = mood;  daysLabel  = days;
            feedBtn = feed; cuddleBtn = cuddle; sleepBtn = sleep;
            stagePanel = stgPanel; stagePanelLabel = stgLabel;
            roomUnlockPanel = roomPanel; roomUnlockLabel = roomLabel;
            offlinePanel = offline; sleepOverlay = sleepOvr;
            feedMenuRoot = feedRoot; feedMenuContent = feedContent;
        }

        public void ShowPrompt(string text)
        {
            if (promptRoot == null) return;
            if (promptLabel) promptLabel.text = text;
            promptRoot.SetActive(true);
        }

        public void HidePrompt() => promptRoot?.SetActive(false);

        public void Refresh(MoonlightCharacter m)
        {
            if (wonderBar) wonderBar.value = m.stats.wonder / 100f;
            if (warmthBar) warmthBar.value = m.stats.warmth / 100f;
            if (restBar)   restBar.value   = m.stats.rest   / 100f;
            if (magicBar)  magicBar.value  = m.stats.magic  / 100f;
            if (hungerBar) hungerBar.value = m.stats.hunger / 100f;

            if (stageLabel) stageLabel.text = StageNames[(int)m.stage];
            if (coinsLabel) coinsLabel.text = $"⭐ {m.coins}";
            if (xpLabel)    xpLabel.text    = $"XP {m.xp}";
            if (daysLabel)  daysLabel.text  = $"Day {Mathf.FloorToInt(m.daysInHouse) + 1}";
            if (moodEmoji)  moodEmoji.text  = MoodEmojis[(int)m.stats.GetMood()];
        }

        public void OnMoodChange(MoonlightMood mood)
        {
            if (moodEmoji) moodEmoji.text = MoodEmojis[(int)mood];
        }
        public void UpdateCoins(int coins) { if (coinsLabel) coinsLabel.text = $"⭐ {coins}"; }
        public void UpdateXP(int xp)       { if (xpLabel) xpLabel.text = $"XP {xp}"; }

        public void ShowStageCelebration(MoonlightStage stage)
        {
            if (stagePanelLabel) stagePanelLabel.text = $"Moonlight became a {StageNames[(int)stage]}! ✨";
            if (stagePanel) StartCoroutine(ShowThenHide(stagePanel, 4f));
        }

        public void ShowRoomUnlocked(int count)
        {
            if (roomUnlockLabel) roomUnlockLabel.text = $"New room unlocked: {RoomNames[Mathf.Min(count, RoomNames.Length-1)]}! 🌙";
            if (roomUnlockPanel) StartCoroutine(ShowThenHide(roomUnlockPanel, 3f));
        }

        public void ShowOfflineNotice()
        {
            if (offlinePanel) StartCoroutine(ShowThenHide(offlinePanel, 3f));
        }

        public void OpenFeedMenuWith(List<FoodItem> overrideCatalogue)
        {
            PopulateFeedMenu(overrideCatalogue);
            if (feedMenuRoot) feedMenuRoot.SetActive(true);
        }

        void OpenFeedMenu()
        {
            if (foodCatalogue != null)
                PopulateFeedMenu(new List<FoodItem>(foodCatalogue));
            else if (feedMenuRoot)
                feedMenuRoot.SetActive(true);
        }

        void PopulateFeedMenu(List<FoodItem> catalogue)
        {
            if (feedMenuContent == null) return;
            foreach (Transform t in feedMenuContent) Destroy(t.gameObject);
            foreach (var food in catalogue)
            {
                var itemGO = new GameObject(food.itemName);
                itemGO.transform.SetParent(feedMenuContent, false);
                var btn = itemGO.AddComponent<Button>();
                var img = itemGO.AddComponent<Image>();
                img.color = new Color(0.2f, 0.1f, 0.35f);
                var rt = itemGO.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(200, 60);

                var lblGO = new GameObject("Label");
                lblGO.transform.SetParent(itemGO.transform, false);
                var lbl = lblGO.AddComponent<TextMeshProUGUI>();
                lbl.text = $"{food.itemName}\n⭐{food.cost}";
                lbl.fontSize = 18;
                lbl.alignment = TextAlignmentOptions.Center;
                var lblRt = lbl.GetComponent<RectTransform>();
                lblRt.anchorMin = Vector2.zero;
                lblRt.anchorMax = Vector2.one;
                lblRt.offsetMin = Vector2.zero;
                lblRt.offsetMax = Vector2.zero;

                var captured = food;
                btn.onClick.AddListener(() =>
                {
                    MoonlightGameManager.Instance?.moonlight.Feed(captured);
                    if (MoonlightGameManager.Instance?.moonlight != null)
                        Refresh(MoonlightGameManager.Instance.moonlight);
                    if (feedMenuRoot) feedMenuRoot.SetActive(false);
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
