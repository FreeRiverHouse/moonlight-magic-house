using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlightMagicHouse
{
    // Mirrors TMP_Text content to legacy UI.Text so existing MoonlightUI code
    // (which drives TMP labels) produces visible output on platforms where
    // TMP Essential Resources are not available.
    public class LegacyLabelMirror : MonoBehaviour
    {
        struct Pair { public TMP_Text tmp; public Text legacy; public string prefix; }
        Pair[] _pairs;

        public void Bind(
            TMP_Text stageT, Text stageL, string stagePrefix,
            TMP_Text moodT,  Text moodL,  string moodPrefix,
            TMP_Text coinsT, Text coinsL, string coinsPrefix,
            TMP_Text xpT,    Text xpL,    string xpPrefix,
            TMP_Text daysT,  Text daysL,  string daysPrefix)
        {
            _pairs = new Pair[]
            {
                new Pair { tmp = stageT, legacy = stageL, prefix = stagePrefix },
                new Pair { tmp = moodT,  legacy = moodL,  prefix = moodPrefix  },
                new Pair { tmp = coinsT, legacy = coinsL, prefix = coinsPrefix },
                new Pair { tmp = xpT,    legacy = xpL,    prefix = xpPrefix    },
                new Pair { tmp = daysT,  legacy = daysL,  prefix = daysPrefix  },
            };
        }

        void LateUpdate()
        {
            if (_pairs == null) return;
            for (int i = 0; i < _pairs.Length; i++)
            {
                var p = _pairs[i];
                if (p.tmp == null || p.legacy == null) continue;
                string txt = p.tmp.text ?? "";
                string full = string.IsNullOrEmpty(p.prefix) ? txt : p.prefix + txt;
                if (p.legacy.text != full) p.legacy.text = full;
            }
        }
    }
}
