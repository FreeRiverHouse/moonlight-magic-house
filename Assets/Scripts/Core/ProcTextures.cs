using UnityEngine;

namespace MoonlightMagicHouse
{
    // Runtime-generated textures for floors, rugs, walls — gives the scene
    // a painted-storybook feel without external art assets.
    public static class ProcTextures
    {
        static Texture2D _wood, _rug, _wall, _ceiling, _velvet, _wood2;

        // Warm plank floor with subtle grain
        public static Texture2D WoodPlanks(int w = 512, int h = 512)
        {
            if (_wood != null) return _wood;
            var t = New(w, h);
            var plankA = new Color(0.42f, 0.22f, 0.18f);
            var plankB = new Color(0.36f, 0.18f, 0.14f);
            int plankH = 64;
            for (int y = 0; y < h; y++)
            {
                int plank = y / plankH;
                var baseCol = (plank % 2 == 0) ? plankA : plankB;
                float offset = (plank * 113) % 256 / 256f;
                for (int x = 0; x < w; x++)
                {
                    float grain = Mathf.PerlinNoise((x + offset * 400) * 0.04f, y * 0.015f);
                    var  c = baseCol * (0.85f + 0.3f * grain);
                    // Plank seam lines
                    if (y % plankH == 0 || y % plankH == plankH - 1) c *= 0.55f;
                    // Vertical seam every ~128px, offset per row
                    int seamX = (int)((plank * 173 + 64) % w);
                    if (Mathf.Abs(x - seamX) < 2) c *= 0.55f;
                    c.a = 1f;
                    t.SetPixel(x, y, c);
                }
            }
            t.Apply();
            _wood = t;
            return t;
        }

        // Soft knit rug with diamond pattern
        public static Texture2D Rug(int w = 256, int h = 256)
        {
            if (_rug != null) return _rug;
            var t = New(w, h);
            var baseCol   = new Color(0.55f, 0.30f, 0.82f);
            var stripeCol = new Color(0.78f, 0.58f, 1.00f);
            var accentCol = new Color(1.00f, 0.78f, 0.55f);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float n  = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                var   c  = Color.Lerp(baseCol, baseCol * 0.85f, n);
                // Diamond accents
                int   dx = Mathf.Abs((x % 64) - 32);
                int   dy = Mathf.Abs((y % 64) - 32);
                if (dx + dy < 6) c = accentCol;
                else if (Mathf.Abs(dx + dy - 20) < 2) c = stripeCol;
                // Border
                int b = 14;
                if (x < b || y < b || x > w - b || y > h - b) c = accentCol * 0.7f;
                c.a = 1f;
                t.SetPixel(x, y, c);
            }
            t.Apply();
            _rug = t;
            return t;
        }

        // Vertical striped wallpaper with tiny sparkle specks
        public static Texture2D Wallpaper(int w = 256, int h = 256)
        {
            if (_wall != null) return _wall;
            var t = New(w, h);
            var a = new Color(0.20f, 0.12f, 0.35f);
            var b = new Color(0.26f, 0.16f, 0.44f);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int   band = (x / 24) % 2;
                var   c    = (band == 0) ? a : b;
                float n    = Mathf.PerlinNoise(x * 0.02f, y * 0.02f);
                c          = Color.Lerp(c, c * 1.15f, n * 0.5f);
                // Random speckle sparkles
                int  hash = (x * 73856093 ^ y * 19349663) & 0xFFFF;
                if (hash < 18) c = new Color(1f, 0.9f, 0.7f);
                c.a = 1f;
                t.SetPixel(x, y, c);
            }
            t.Apply();
            _wall = t;
            return t;
        }

        // Starry dark ceiling with twinkle dots
        public static Texture2D CeilingSky(int w = 512, int h = 512)
        {
            if (_ceiling != null) return _ceiling;
            var t = New(w, h);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float r = y / (float)h;
                var   c = Color.Lerp(new Color(0.06f, 0.03f, 0.15f),
                                     new Color(0.12f, 0.06f, 0.28f), r);
                int   hash = (x * 92821 ^ y * 58417) & 0xFFFF;
                if (hash < 20)
                {
                    float bright = 0.7f + 0.3f * (hash % 100) / 100f;
                    c = new Color(bright, bright * 0.9f, bright * 0.7f);
                }
                c.a = 1f;
                t.SetPixel(x, y, c);
            }
            t.Apply();
            _ceiling = t;
            return t;
        }

        // Soft velvet fabric with faint weave lines
        public static Texture2D Velvet(int w = 256, int h = 256)
        {
            if (_velvet != null) return _velvet;
            var t = New(w, h);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.12f, y * 0.12f);
                var c   = new Color(0.80f + 0.15f * n, 0.78f + 0.12f * n, 0.82f + 0.15f * n);
                // Fine weave: horizontal + vertical gentle darkening
                if ((x % 6) == 0) c *= 0.93f;
                if ((y % 6) == 0) c *= 0.93f;
                c.a = 1f;
                t.SetPixel(x, y, c);
            }
            t.Apply();
            _velvet = t;
            return t;
        }

        // Lighter wood with visible knot spots (for chest/table)
        public static Texture2D LightWood(int w = 256, int h = 256)
        {
            if (_wood2 != null) return _wood2;
            var t = New(w, h);
            var baseA = new Color(0.70f, 0.48f, 0.32f);
            var baseB = new Color(0.62f, 0.40f, 0.26f);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.03f, y * 0.12f);
                var c   = Color.Lerp(baseA, baseB, n);
                // Occasional knot
                float d = Mathf.Sqrt((x - 128) * (x - 128) + (y - 90) * (y - 90));
                if (d < 10) c *= 0.55f;
                c.a = 1f;
                t.SetPixel(x, y, c);
            }
            t.Apply();
            _wood2 = t;
            return t;
        }

        static Texture2D New(int w, int h)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, true);
            t.wrapMode   = TextureWrapMode.Repeat;
            t.filterMode = FilterMode.Trilinear;
            t.anisoLevel = 4;
            return t;
        }
    }
}
