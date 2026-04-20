using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public class PlayArea : MonoBehaviour
    {
        [SerializeField] List<ActivityItem> activities;

        public void OpenActivities()
        {
            foreach (var act in activities)
                MoonlightGameManager.Instance?.moonlight.Explore(RoomType.LivingRoom);
        }
    }
}
