using UnityEngine;

namespace MoonlightMagicHouse
{
    public sealed class MoonlightSafeAreaBand : MonoBehaviour
    {
        public enum Edge { Top, Bottom }

        RectTransform _rect;
        Edge _edge;
        Vector2 _offsetMin;
        Vector2 _offsetMax;
        Rect _lastSafeArea;
        int _lastWidth;
        int _lastHeight;
        bool _configured;

        public void Configure(Edge edge)
        {
            _rect = GetComponent<RectTransform>();
            _edge = edge;
            _offsetMin = _rect.offsetMin;
            _offsetMax = _rect.offsetMax;
            _configured = true;
            Apply();
        }

        void Update()
        {
            if (!_configured) return;
            if (_lastWidth != Screen.width || _lastHeight != Screen.height ||
                !_lastSafeArea.Equals(Screen.safeArea))
                Apply();
        }

        void Apply()
        {
            if (_rect == null || Screen.width <= 0 || Screen.height <= 0) return;

            Rect safe = Screen.safeArea;
            float xMin = safe.xMin / Screen.width;
            float xMax = safe.xMax / Screen.width;
            float y = (_edge == Edge.Top ? safe.yMax : safe.yMin) / Screen.height;
            _rect.anchorMin = new Vector2(xMin, y);
            _rect.anchorMax = new Vector2(xMax, y);
            _rect.offsetMin = _offsetMin;
            _rect.offsetMax = _offsetMax;

            _lastSafeArea = safe;
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            Debug.Log($"[MoonlightVisualQA] safe-area edge={_edge} screen={Screen.width}x{Screen.height} safe={safe}");
        }
    }
}
