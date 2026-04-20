using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public class FeedingStation : MonoBehaviour
    {
        [SerializeField] List<FoodItem> menu;

        public void OpenMenu() => MoonlightGameManager.Instance?.ui.OpenFeedMenuWith(menu);
    }
}
