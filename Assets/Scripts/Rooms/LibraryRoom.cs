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

            PetUIController.Instance?.ShowStoryPage(pages[next]);
            GameManager.Instance.pet.stats.happiness =
                Mathf.Min(100, GameManager.Instance.pet.stats.happiness + 8f);

            // Reward only first full cycle
            if (_lastReadPage == 0)
            {
                GameManager.Instance.pet.EarnCoins(coinsPerRead);
                // XP credited inside pet
            }
            AchievementSystem.Instance?.OnRoomVisited(RoomType.Library);
            AudioManager.Instance?.Play("page_turn");
        }

        public StoryPage CurrentPage => _lastReadPage >= 0 ? pages[_lastReadPage] : null;
    }
}
