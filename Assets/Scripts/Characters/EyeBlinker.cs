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
                for (int i = 0; i < eyes.Length; i++)
                {
                    if (eyes[i] == null) continue;
                    var s = _baseScale[i];
                    eyes[i].localScale = new Vector3(s.x, s.y * Mathf.Max(0.08f, yk), s.z);
                }
                if (k >= 1f)
                {
                    for (int i = 0; i < eyes.Length; i++)
                        if (eyes[i] != null) eyes[i].localScale = _baseScale[i];
                    _blinkT    = -1f;
                    _nextBlink = Time.time + Random.Range(minGap, maxGap);
                }
            }
        }
    }
}
