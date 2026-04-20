using UnityEngine;

namespace MoonlightMagicHouse
{
    // Slowly drifts an object along a short vertical arc to suggest passing time.
    public class MoonArc : MonoBehaviour
    {
        [SerializeField] float period = 90f;
        [SerializeField] float xSpan  = 0.6f;
        [SerializeField] float ySpan  = 0.35f;

        Vector3 _base;

        void Awake() { _base = transform.localPosition; }

        void Update()
        {
            float t  = Time.time * Mathf.PI * 2f / period;
            float dx = Mathf.Sin(t) * xSpan;
            float dy = Mathf.Cos(t) * ySpan * 0.5f + ySpan * 0.25f;
            transform.localPosition = _base + new Vector3(dx, dy, 0f);
            transform.Rotate(Vector3.forward, 6f * Time.deltaTime);
        }
    }
}
