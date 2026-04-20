using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlightMagicHouse
{
    public class ShopUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Transform itemGrid;
        [SerializeField] GameObject itemCardPrefab;
        [SerializeField] TMP_Text coinsLabel;
        [SerializeField] Button closeBtn;

        MoonlightPet _pet;

        void Start()
        {
            _pet = GameManager.Instance.pet;
            closeBtn.onClick.AddListener(() => root.SetActive(false));
            _pet.onCoinsChanged.AddListener(c => coinsLabel.text = $"⭐ {c}");
        }

        public void Open()
        {
            root.SetActive(true);
            BuildGrid();
            coinsLabel.text = $"⭐ {_pet.coins}";
        }

        void BuildGrid()
        {
            foreach (Transform t in itemGrid) Destroy(t.gameObject);

            var catalogue = ShopManager.Instance.GetCatalogue();
            foreach (var outfit in catalogue)
            {
                var card = Instantiate(itemCardPrefab, itemGrid);
                card.GetComponentInChildren<TMP_Text>().text =
                    $"{outfit.outfitName}\n⭐ {outfit.cost}";

                var btn = card.GetComponent<Button>();
                var captured = outfit;
                btn.onClick.AddListener(() =>
                {
                    bool ok = ShopManager.Instance.BuyOutfit(captured);
                    if (ok)
                    {
                        AudioManager.Instance?.Play("buy");
                        BuildGrid();
                    }
                    else
                    {
                        AudioManager.Instance?.Play("error");
                    }
                });
            }
        }
    }
}
