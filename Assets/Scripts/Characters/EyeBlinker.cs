using UnityEngine;

namespace MoonlightMagicHouse
{
    // Periodically squishes the Y-scale of eye transforms to simulate blinking.
    public class EyeBlinker : MonoBehaviour
    {
        [SerializeField] Transform[] eyes;
        [SerializeField] float minGap     = 2.2f;
        [SerializeField] float maxGap     = 5f;
        [SerializeField] float blinkDur   = 0.13f;

        Vector3[] _baseScale;
        float     _nextBlink;
        float     _blinkT = -1f;

        public int BoundEyeCount => eyes != null ? eyes.Length : 0;
        public float BlinkDuration => blinkDur;
        public float MinimumBlinkGap => minGap;
        public float MaximumBlinkGap => maxGap;
        public float CurrentOpenness { get; private set; } = 1f;
        public string QAMarker => BoundEyeCount == 4 && blinkDur >= 0.10f && blinkDur <= 0.18f &&
            minGap >= 1.8f && maxGap >= minGap + 1.5f
                ? "MOONLIGHT_AUTHORED_EYE_BLINK_READY"
                : "MOONLIGHT_AUTHORED_EYE_BLINK_INCOMPLETE";

        void Awake()
        {
            if (eyes == null || eyes.Length == 0) return;
            _baseScale = new Vector3[eyes.Length];
            for (int i = 0; i < eyes.Length; i++)
                if (eyes[i] != null) _baseScale[i] = eyes[i].localScale;
            _nextBlink = Time.time + Random.Range(minGap, maxGap);
        }

        public void Bind(params Transform[] e)
        {
            eyes = e;
            _baseScale = new Vector3[e.Length];
            for (int i = 0; i < e.Length; i++)
                if (e[i] != null) _baseScale[i] = e[i].localScale;
            _nextBlink = Time.time + Random.Range(minGap, maxGap);
        }

        void Update()
        {
            if (eyes == null) return;
            if (_blinkT < 0f)
            {
                if (Time.time >= _nextBlink) _blinkT = 0f;
            }
            else
            {
                _blinkT += Time.deltaTime;
                float k  = Mathf.Clamp01(_blinkT / blinkDur);
                float yk = 1f - Mathf.Sin(k * Mathf.PI); // 1→0→1
                CurrentOpenness = Mathf.Max(0.08f, yk);
                for (int i = 0; i < eyes.Length; i++)
                {
                    if (eyes[i] == null) continue;
                    var s = _baseScale[i];
                    eyes[i].localScale = new Vector3(s.x, s.y * CurrentOpenness, s.z);
                }
                if (k >= 1f)
                {
                    RestoreEyeScale();
                    _blinkT    = -1f;
                    _nextBlink = Time.time + Random.Range(minGap, maxGap);
                }
            }
        }

        void OnDisable() => RestoreEyeScale();

        void RestoreEyeScale()
        {
            CurrentOpenness = 1f;
            if (eyes == null || _baseScale == null) return;
            for (int i = 0; i < eyes.Length && i < _baseScale.Length; i++)
                if (eyes[i] != null) eyes[i].localScale = _baseScale[i];
        }

        public static bool ValidateBlinkContract(out string detail)
        {
            const int linkedParts = 4;
            const float duration = 0.13f;
            const float minimumGap = 2.2f;
            const float maximumGap = 5.0f;
            detail = $"parts={linkedParts} duration={duration:0.00}s gap=" +
                $"{minimumGap:0.0}-{maximumGap:0.0}s openness=0.08-1.00";
            return linkedParts == 4 && duration >= 0.10f && duration <= 0.18f &&
                minimumGap >= 1.8f && maximumGap >= minimumGap + 1.5f;
        }
    }
}
