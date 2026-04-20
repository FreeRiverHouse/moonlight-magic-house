using UnityEngine;

namespace MoonlightMagicHouse
{
    // Procedural idle: gentle float + head-bob + sway — no Animator needed.
    public class MoonlightBobber : MonoBehaviour
    {
        [SerializeField] float bobHeight  = 0.12f;
        [SerializeField] float bobSpeed   = 1.8f;
        [SerializeField] float swayAngle  = 4f;
        [SerializeField] float swaySpeed  = 1.1f;
        [SerializeField] float glowPulse  = 0.35f;
        [SerializeField] float glowBase   = 2.8f;

        float    _t;
        Light    _glow;
        Vector3  _startPos;

        void Start()
        {
            _startPos = transform.localPosition;
            _glow     = GetComponentInChildren<Light>();
            _t        = Random.Range(0f, Mathf.PI * 2f);   // desync between instances
        }

        void Update()
        {
            _t += Time.deltaTime;
            // Float up/down
            transform.localPosition = _startPos + Vector3.up * (Mathf.Sin(_t * bobSpeed) * bobHeight);
            // Gentle lean side-to-side
            float lean = Mathf.Sin(_t * swaySpeed) * swayAngle;
            transform.localRotation = Quaternion.Euler(0f, 0f, lean);
            // Glow pulse
            if (_glow != null)
                _glow.intensity = glowBase + Mathf.Sin(_t * bobSpeed * 1.5f) * glowPulse;
        }
    }
}
