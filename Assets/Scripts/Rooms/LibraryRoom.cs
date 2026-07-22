using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    [Serializable]
    public class StoryPage
    {
        [TextArea(3, 8)] public string text;
        public Sprite illustration;
    }

    [Serializable]
    public sealed class AuthoredStoryPage
    {
        public string id;
        public string title;
        [TextArea(4, 12)] public string body;
        public int unlockStage;
        public int coinsReward;
    }

    [Serializable]
    sealed class AuthoredStoryPageCollection
    {
        public AuthoredStoryPage[] pages;
    }

    public class LibraryRoom : MonoBehaviour
    {
        public const string StoryResourcePath = "Data/DefaultStoryPages";
        public const int RequiredAuthoredStoryCount = 10;
        public const string StoryDataReadyMarker = "MOONLIGHT_STORY_DATA_10_READY";

        static readonly int[] ExpectedCumulativeEligibleCounts = { 2, 4, 7, 9, 10 };

        [SerializeField] List<StoryPage> pages;
        [SerializeField] int xpPerRead = 15;
        [SerializeField] int coinsPerRead = 3;

        int _lastReadPage = -1;

        public void ReadNextPage()
        {
            if (pages == null || pages.Count == 0) return;
            int next = (_lastReadPage + 1) % pages.Count;
            _lastReadPage = next;

            FindAnyObjectByType<StoryPageUI>()?.Show(pages[next]);

            var ml = MoonlightGameManager.Instance?.moonlight;
            if (ml != null)
            {
                ml.ReadStory();
                if (_lastReadPage == 0) ml.EarnCoins(coinsPerRead);
            }

            AchievementSystem.Instance?.Check("room_library");
            AudioManager.Instance?.Play("page_turn");
        }

        public StoryPage CurrentPage => pages != null && _lastReadPage >= 0
            ? pages[_lastReadPage]
            : null;

        public static bool TryLoadAuthoredStories(out AuthoredStoryPage[] loaded, out string detail)
        {
            loaded = Array.Empty<AuthoredStoryPage>();
            TextAsset source = Resources.Load<TextAsset>(StoryResourcePath);
            if (source == null)
            {
                detail = $"resource={StoryResourcePath} missing";
                return false;
            }

            bool valid = ValidateAuthoredStoryJson(source.text, out loaded, out detail);
            if (!valid) loaded = Array.Empty<AuthoredStoryPage>();
            return valid;
        }

        public static bool ValidateAuthoredStoryJson(string json, out AuthoredStoryPage[] loaded,
            out string detail)
        {
            loaded = Array.Empty<AuthoredStoryPage>();
            if (string.IsNullOrWhiteSpace(json))
            {
                detail = "json=empty";
                return false;
            }

            AuthoredStoryPageCollection collection;
            try
            {
                collection = JsonUtility.FromJson<AuthoredStoryPageCollection>(json);
            }
            catch (Exception exception)
            {
                detail = $"json=parse-failed type={exception.GetType().Name}";
                return false;
            }

            loaded = collection?.pages ?? Array.Empty<AuthoredStoryPage>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            bool entriesValid = loaded.Length == RequiredAuthoredStoryCount;
            for (int i = 0; i < loaded.Length; i++)
            {
                AuthoredStoryPage page = loaded[i];
                entriesValid &= page != null &&
                    !string.IsNullOrWhiteSpace(page.id) && ids.Add(page.id) &&
                    !string.IsNullOrWhiteSpace(page.title) &&
                    !string.IsNullOrWhiteSpace(page.body) &&
                    page.unlockStage >= 0 && page.unlockStage <= 4 &&
                    page.coinsReward >= 0;
            }

            int[] cumulative = new int[ExpectedCumulativeEligibleCounts.Length];
            for (int stage = 0; stage < cumulative.Length; stage++)
            {
                for (int i = 0; i < loaded.Length; i++)
                    if (loaded[i] != null && loaded[i].unlockStage <= stage) cumulative[stage]++;
            }

            bool eligibilityValid = true;
            for (int stage = 0; stage < cumulative.Length; stage++)
                eligibilityValid &= cumulative[stage] == ExpectedCumulativeEligibleCounts[stage];

            detail = $"loaded={loaded.Length}/{RequiredAuthoredStoryCount} " +
                $"eligible={string.Join(",", cumulative)} validEntries={entriesValid} " +
                $"marker={(entriesValid && eligibilityValid ? StoryDataReadyMarker : "MOONLIGHT_STORY_DATA_INVALID")}";
            return entriesValid && eligibilityValid;
        }

        public static int EligibleCountForStage(IReadOnlyList<AuthoredStoryPage> source, int stage)
        {
            if (source == null) return 0;
            int boundedStage = Mathf.Clamp(stage, 0, 4);
            int count = 0;
            for (int i = 0; i < source.Count; i++)
                if (source[i] != null && source[i].unlockStage <= boundedStage) count++;
            return count;
        }
    }
}
