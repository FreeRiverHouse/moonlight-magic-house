using UnityEngine;

namespace MoonlightMagicHouse
{
    // Temporary scale-punch: briefly scales a target up then smoothly returns to 1.
    public class ScalePuncher : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float     punchAmount = 0.18f;
        [SerializeField] float     duration    = 0.35f;

        float _t = -1f;
        Vector3 _base = Vector3.one;

        void Awake()
        {
            if (target == null) target = transform;
            _base = target.localScale;
        }

        public void Punch() => Punch(punchAmount, duration);

        public void Punch(float amount, float dur)
        {
            punchAmount = amount;
            duration    = dur;
            _t = 0f;
        }

        void Update()
        {
            if (_t < 0f) return;
            _t += Time.deltaTime;
            float k = Mathf.Clamp01(_t / duration);
            // Smooth in/out punch curve
            float bump = Mathf.Sin(k * Mathf.PI) * punchAmount;
            target.localScale = _base * (1f + bump);
            if (k >= 1f)
            {
                target.localScale = _base;
                _t = -1f;
            }
        }
    }
}
