using UnityEngine;

namespace MoonlightMagicHouse
{
    public class SleepArea : MonoBehaviour
    {
        public void PutToSleep()
        {
            MoonlightGameManager.Instance?.moonlight.PutToSleep();
        }
    }
}
