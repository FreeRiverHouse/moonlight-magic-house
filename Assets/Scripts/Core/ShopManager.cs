using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        [SerializeField] List<OutfitItem> outfits;
        [SerializeField] GameObject shopPanel;

        MoonlightPet _pet;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start() => _pet = GameManager.Instance.pet;

        public void OpenShop() => shopPanel.SetActive(true);
        public void CloseShop() => shopPanel.SetActive(false);

        public bool BuyOutfit(OutfitItem outfit)
        {
            if (!_pet.SpendCoins(outfit.cost)) return false;
            EquipOutfit(outfit);
            AudioManager.Instance?.Play("buy");
            return true;
        }

        public void EquipOutfit(OutfitItem outfit)
        {
            var wardrobe = _pet.GetComponent<PetWardrobe>();
            if (wardrobe != null) wardrobe.Equip(outfit.id);
        }

        public List<OutfitItem> GetCatalogue() => outfits;
    }
}
