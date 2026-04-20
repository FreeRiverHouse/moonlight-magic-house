using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace MoonlightMagicHouse
{
    // Attach to each stat slider. Shows a tooltip on hover with stat name + value.
    public class StatTooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] string   statKey;          // localization key e.g. "ui.wonder"
        [SerializeField] Slider   slider;
        [SerializeField] GameObject tooltipRoot;
        [SerializeField] TMP_Text   tooltipLabel;

        void Start()
        {
            if (tooltipRoot) tooltipRoot.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData _)
        {
            if (tooltipRoot == null) return;
            int pct = Mathf.RoundToInt(slider.value * 100f);
            tooltipLabel.text = $"{LocalizationManager.Get(statKey)}: {pct}%";
            tooltipRoot.SetActive(true);
        }

        public void OnPointerExit(PointerEventData _)
        {
            tooltipRoot?.SetActive(false);
        }
    }
}
