using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    // Manages Moonlight's outfit — each outfit is a child GameObject
    // containing the relevant mesh accessories / overlay objects.
    public class MoonlightWardrobe : MonoBehaviour
    {
        [System.Serializable]
        public struct Outfit
        {
            public int        id;
            public string     displayName;
            public int        cost;
            public GameObject root;   // activate/deactivate
        }

        [SerializeField] List<Outfit> outfits;

        int _current = 0;
        public int CurrentId => _current;

        public void Equip(int id)
        {
            foreach (var o in outfits) o.root.SetActive(o.id == id);
            _current = id;
        }

        public bool TryBuy(int id)
        {
            var outfit = outfits.Find(o => o.id == id);
            if (outfit.root == null) return false;
            if (!MoonlightGameManager.Instance.moonlight.SpendCoins(outfit.cost)) return false;
            Equip(id);
            AudioManager.Instance?.Play("buy");
            HapticFeedback.Light();
            return true;
        }

        public List<Outfit> GetCatalogue() => outfits;
    }
}
