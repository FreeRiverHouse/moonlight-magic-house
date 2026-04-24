using UnityEngine;

namespace MoonlightMagicHouse
{
    [DisallowMultipleComponent]
    public class BreathingMotion : MonoBehaviour
    {
        [SerializeField] float amplitude = 0.014f;
        [SerializeField] float frequency = 0.42f;
        [SerializeField] string[] parts = { "Bodice", "Body" };

        Transform[] _targets;
        Vector3[]   _baseScale;
        float       _t;

        void Start()
        {
            var list = new System.Collections.Generic.List<Transform>();
            foreach (var n in parts)
            {
                var t = DeepFind(transform, n);
                if (t != null) list.Add(t);
            }
            _targets = list.ToArray();
            _baseScale = new Vector3[_targets.Length];
            for (int i = 0; i < _targets.Length; i++) _baseScale[i] = _targets[i].localScale;
        }

        void Update()
        {
            _t += Time.deltaTime * frequency * Mathf.PI * 2f;
            float s = 1f + Mathf.Sin(_t) * amplitude;
            for (int i = 0; i < _targets.Length; i++)
            {
                var b = _baseScale[i];
                _targets[i].localScale = new Vector3(b.x * s, b.y * (1f + Mathf.Sin(_t) * amplitude * 0.3f), b.z * s);
            }
        }

        static Transform DeepFind(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var r = DeepFind(root.GetChild(i), name);
                if (r != null) return r;
            }
            return null;
        }
    }
}
