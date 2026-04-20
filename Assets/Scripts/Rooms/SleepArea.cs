using UnityEngine;

namespace MoonlightMagicHouse
{
    public class SleepArea : MonoBehaviour
    {
        MoonlightPet _pet;

        void Start() => _pet = GameManager.Instance?.pet;

        public void PutToSleep()
        {
            if (_pet == null) return;
            _pet.Sleep();
            PetUIController.Instance?.ShowSleepAnimation();
        }
    }
}
