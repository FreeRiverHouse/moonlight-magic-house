using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace MoonlightMagicHouse
{
    // Attach to each stat bar — shows tooltip on long press / hover
    public class PetStatsTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
                                                    IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] string statKey;   // e.g. "ui.hunger"
        [SerializeField] GameObject tooltipRoot;
        [SerializeField] TMP_Text tooltipLabel;
        [SerializeField] float longPressTime = 0.5f;

        Coroutine _pressTimer;

        public void OnPointerEnter(PointerEventData _)
        {
            if (Application.isMobilePlatform) return;
            ShowTooltip();
        }

        public void OnPointerExit(PointerEventData _) => HideTooltip();

        public void OnPointerDown(PointerEventData _)
        {
            if (!Application.isMobilePlatform) return;
            _pressTimer = StartCoroutine(LongPress());
        }

        public void OnPointerUp(PointerEventData _)
        {
            if (_pressTimer != null) StopCoroutine(_pressTimer);
            HideTooltip();
        }

        IEnumerator LongPress()
        {
            yield return new WaitForSeconds(longPressTime);
            ShowTooltip();
        }

        void ShowTooltip()
        {
            tooltipLabel.text = LocalizationManager.Get(statKey);
            tooltipRoot.SetActive(true);
        }

        void HideTooltip() => tooltipRoot.SetActive(false);
    }
}
