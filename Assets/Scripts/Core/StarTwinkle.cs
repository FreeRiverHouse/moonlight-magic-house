using UnityEngine;

namespace MoonlightMagicHouse
{
    public class StarTwinkle : MonoBehaviour
    {
        [SerializeField] float speed   = 2.2f;
        [SerializeField] float depth   = 0.6f;
        Vector3 _base;
        float   _phase;

        void Awake()
        {
            _base  = transform.localScale;
            _phase = Random.value * Mathf.PI * 2f;
        }

        void Update()
        {
            float k = 1f + (Mathf.Sin(Time.time * speed + _phase) * 0.5f + 0.5f - 0.5f) * depth;
            transform.localScale = _base * k;
        }
    }
}
