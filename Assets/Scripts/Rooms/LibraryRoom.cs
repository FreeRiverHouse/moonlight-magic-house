using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    [System.Serializable]
    public class StoryPage
    {
        [TextArea(3, 8)] public string text;
        public Sprite illustration;
    }

    public class LibraryRoom : MonoBehaviour
    {
        [SerializeField] List<StoryPage> pages;
        [SerializeField] int xpPerRead = 15;
        [SerializeField] int coinsPerRead = 3;

        int _lastReadPage = -1;

        public void ReadNextPage()
        {
            if (pages.Count == 0) return;
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

        public StoryPage CurrentPage => _lastReadPage >= 0 ? pages[_lastReadPage] : null;
    }
}
