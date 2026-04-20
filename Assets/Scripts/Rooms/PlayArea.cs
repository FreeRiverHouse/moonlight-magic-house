using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public class PlayArea : MonoBehaviour
    {
        [SerializeField] List<ActivityItem> activities;

        public void OpenActivities() => PetUIController.Instance?.ShowActivitiesMenu(activities);
    }
}
