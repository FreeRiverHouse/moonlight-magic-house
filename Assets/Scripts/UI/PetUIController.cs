using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlightMagicHouse
{
    public class PetUIController : MonoBehaviour
    {
        public static PetUIController Instance { get; private set; }

        [Header("Stats")]
        [SerializeField] Slider hungerBar;
        [SerializeField] Slider happinessBar;
        [SerializeField] Slider energyBar;
        [SerializeField] Slider cleanlinessBar;
        [SerializeField] Slider healthBar;

        [Header("Info")]
        [SerializeField] TMP_Text petNameLabel;
        [SerializeField] TMP_Text levelLabel;
        [SerializeField] TMP_Text coinsLabel;
        [SerializeField] TMP_Text moodEmoji;
        [SerializeField] TMP_Text stageLabel;

        [Header("Prompt")]
        [SerializeField] GameObject promptRoot;
        [SerializeField] TMP_Text promptLabel;

        [Header("Overlays")]
        [SerializeField] GameObject evolutionPanel;
        [SerializeField] TMP_Text evolutionLabel;
        [SerializeField] GameObject levelUpPanel;
        [SerializeField] TMP_Text levelUpLabel;
        [SerializeField] GameObject offlinePanel;
        [SerializeField] GameObject sleepOverlay;

        [Header("Menus")]
        [SerializeField] GameObject foodMenuRoot;
        [SerializeField] Transform foodMenuContent;
        [SerializeField] GameObject foodButtonPrefab;

        [SerializeField] GameObject activityMenuRoot;
        [SerializeField] Transform activityMenuContent;
        [SerializeField] GameObject activityButtonPrefab;

        static readonly string[] MoodEmojis = { "😢", "😴", "🍽️", "😐", "😊", "🌟" };
        static readonly string[] StageNames = { "Egg", "Baby", "Child", "Teen", "Adult" };

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Refresh(MoonlightPet pet)
        {
            UpdateStats(pet.stats);
            petNameLabel.text  = pet.petName;
            levelLabel.text    = $"Lv. {pet.level}";
            coinsLabel.text    = $"⭐ {pet.coins}";
            stageLabel.text    = StageNames[(int)pet.stage];
            moodEmoji.text     = MoodEmojis[(int)pet.stats.GetMood()];
        }

        void UpdateStats(PetStats s)
        {
            hungerBar.value      = s.hunger      / 100f;
            happinessBar.value   = s.happiness   / 100f;
            energyBar.value      = s.energy      / 100f;
            cleanlinessBar.value = s.cleanliness / 100f;
            healthBar.value      = s.health      / 100f;
        }

        public void UpdateMoodIndicator(PetMood mood) => moodEmoji.text = MoodEmojis[(int)mood];
        public void UpdateCoins(int coins) => coinsLabel.text = $"⭐ {coins}";

        public void ShowPrompt(string text)
        {
            promptLabel.text = text;
            promptRoot.SetActive(true);
        }
        public void HidePrompt() => promptRoot.SetActive(false);

        public void ShowLevelUp(int level)
        {
            levelLabel.text = $"Lv. {level}";
            levelUpLabel.text = $"Level {level}!";
            StartCoroutine(ShowThenHide(levelUpPanel, 2.5f));
        }

        public void ShowEvolutionCelebration(EvolutionStage stage)
        {
            evolutionLabel.text = $"{StageNames[(int)stage]} evolved!";
            StartCoroutine(ShowThenHide(evolutionPanel, 3f));
        }

        public void ShowOfflineNotice() => StartCoroutine(ShowThenHide(offlinePanel, 3f));
        public void ShowSleepAnimation() => StartCoroutine(ShowThenHide(sleepOverlay, 2f));

        public void ShowFoodMenu(List<FoodItem> items)
        {
            PopulateMenu(foodMenuContent, foodButtonPrefab, items.Count, i =>
            {
                var btn = foodMenuContent.GetChild(i).GetComponent<Button>();
                var food = items[i];
                btn.GetComponentInChildren<TMP_Text>().text = $"{food.itemName}\n⭐{food.cost}";
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    GameManager.Instance.pet.Feed(food);
                    GameManager.Instance.ui.Refresh(GameManager.Instance.pet);
                    foodMenuRoot.SetActive(false);
                });
            });
            foodMenuRoot.SetActive(true);
        }

        public void ShowActivitiesMenu(List<ActivityItem> items)
        {
            PopulateMenu(activityMenuContent, activityButtonPrefab, items.Count, i =>
            {
                var btn = activityMenuContent.GetChild(i).GetComponent<Button>();
                var act = items[i];
                btn.GetComponentInChildren<TMP_Text>().text = act.activityName;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    GameManager.Instance.pet.Play(act);
                    GameManager.Instance.ui.Refresh(GameManager.Instance.pet);
                    activityMenuRoot.SetActive(false);
                });
            });
            activityMenuRoot.SetActive(true);
        }

        void PopulateMenu(Transform content, GameObject prefab, int count, System.Action<int> setup)
        {
            while (content.childCount < count)
                Instantiate(prefab, content);
            for (int i = 0; i < content.childCount; i++)
                content.GetChild(i).gameObject.SetActive(i < count);
            for (int i = 0; i < count; i++) setup(i);
        }

        public void ShowStoryPage(StoryPage page)
        {
            var storyUI = FindAnyObjectByType<StoryPageUI>();
            storyUI?.Show(page);
        }

        IEnumerator ShowThenHide(GameObject panel, float duration)
        {
            panel.SetActive(true);
            yield return new WaitForSeconds(duration);
            panel.SetActive(false);
        }
    }
}
