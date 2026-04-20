using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlightMagicHouse
{
    // Displays a moon-phase icon and time of day hint in the HUD.
    // Purely cosmetic — uses real device time.
    public class DayNightCycleUI : MonoBehaviour
    {
        [SerializeField] TMP_Text   timeLabel;
        [SerializeField] Image      moonIcon;
        [SerializeField] Sprite[]   moonPhases;  // 8 phases 0=new→7=waning crescent

        float _updateTimer;

        void Update()
        {
            _updateTimer += Time.deltaTime;
            if (_updateTimer < 30f) return;
            _updateTimer = 0f;
            Refresh();
        }

        void Start() => Refresh();

        void Refresh()
        {
            var now = System.DateTime.Now;
            // Moon phase rough approximation by day of month
            int phase = (now.Day % 8);
            if (moonIcon && moonPhases != null && phase < moonPhases.Length)
                moonIcon.sprite = moonPhases[phase];

            if (timeLabel)
            {
                string period = now.Hour < 6 || now.Hour >= 20 ? "🌙" : now.Hour < 12 ? "🌅" : "☀️";
                timeLabel.text = $"{period} {now:HH:mm}";
            }
        }
    }
}
