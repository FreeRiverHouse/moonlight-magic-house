using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlightMagicHouse
{
    public class MoonlightWardrobeUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Transform  grid;
        [SerializeField] GameObject outfitCardPrefab;
        [SerializeField] TMP_Text   coinsLabel;
        [SerializeField] Button     closeBtn;

        void Start()
        {
            closeBtn.onClick.AddListener(() => root.SetActive(false));
            MoonlightGameManager.Instance?.moonlight.onCoinsChanged
                .AddListener(c => coinsLabel.text = $"⭐ {c}");
        }

        public void Open()
        {
            root.SetActive(true);
            var ml       = MoonlightGameManager.Instance?.moonlight;
            coinsLabel.text = $"⭐ {ml?.coins ?? 0}";
            foreach (Transform t in grid) Destroy(t.gameObject);

            var wardrobe = MoonlightGameManager.Instance?.wardrobe;
            if (wardrobe == null) return;

            foreach (var outfit in wardrobe.GetCatalogue())
            {
                var card = Instantiate(outfitCardPrefab, grid);
                var lbl  = card.GetComponentInChildren<TMP_Text>();
                bool owned = wardrobe.CurrentId == outfit.id || outfit.cost == 0;
                lbl.text = owned
                    ? $"{outfit.displayName}\n✓ equipped"
                    : $"{outfit.displayName}\n⭐ {outfit.cost}";

                var btn      = card.GetComponent<Button>();
                var captured = outfit.id;
                btn.onClick.AddListener(() =>
                {
                    if (!wardrobe.TryBuy(captured)) return;
                    MoonlightGameManager.Instance?.Save();
                    Open(); // refresh
                });
            }
        }
    }
}
