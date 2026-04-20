using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public class PetWardrobe : MonoBehaviour
    {
        [System.Serializable]
        public struct OutfitSlot { public int id; public GameObject root; }

        [SerializeField] List<OutfitSlot> slots;

        int _current = -1;

        public void Equip(int id)
        {
            foreach (var s in slots) s.root.SetActive(s.id == id);
            _current = id;
        }

        public void Unequip()
        {
            foreach (var s in slots) s.root.SetActive(false);
            _current = -1;
        }

        public int CurrentOutfit => _current;
    }
}
