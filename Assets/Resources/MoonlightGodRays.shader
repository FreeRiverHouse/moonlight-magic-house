Shader "MoonlightMagicHouse/GodRays"
{
    Properties
    {
        _MainTex     ("Source", 2D) = "white" {}
        _SunScreen   ("Sun Screen Pos (xy)", Vector) = (0.75, 0.78, 0, 0)
        _Density     ("Density",   Range(0, 2))   = 1.0
        _Decay       ("Decay",     Range(0.8, 1)) = 0.965
        _Weight      ("Weight",    Range(0, 2))   = 0.55
        _Exposure    ("Exposure",  Range(0, 3))   = 0.55
        _RayTint     ("Ray Tint",  Color) = (1.00, 0.92, 0.80, 1)
        _Threshold   ("Bright Threshold", Range(0, 2)) = 0.80
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        // 0: brightmask — isolates pixels near sky (depth == 1) + bright
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float     _Threshold;
            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                float  depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                // Only very bright pixels at sky depth contribute (the moon/window)
                float  skyMask = step(0.9999, depth);
                float  bright  = max(0, max(c.r, max(c.g, c.b)) - _Threshold);
                float  m = saturate(bright) * skyMask;
                return fixed4(c.rgb * m, 1);
            }
            ENDCG
        }

        // 1: radial blur toward _SunScreen
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4    _SunScreen;
            float     _Density, _Decay, _Weight, _Exposure;
            fixed4 frag(v2f_img i) : SV_Target
            {
                const int STEPS = 28;
                float2 uv   = i.uv;
                float2 dir  = (uv - _SunScreen.xy) * (_Density / STEPS);
                fixed3 acc  = tex2D(_MainTex, uv).rgb;
                float  illum = 1.0;
                [unroll]
                for (int s = 0; s < STEPS; s++)
                {
                    uv    -= dir;
                    float2 cuv = saturate(uv);
                    float  oob = step(abs(cuv.x - uv.x) + abs(cuv.y - uv.y), 0.0001);
                    fixed3 samp = tex2D(_MainTex, cuv).rgb * illum * _Weight * oob;
                    acc  += samp;
                    illum *= _Decay;
                }
                return fixed4(acc * _Exposure, 1);
            }
            ENDCG
        }

        // 2: composite rays additively onto source
        Pass
        {
            Blend One One
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            fixed4    _RayTint;
            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed3 r = tex2D(_MainTex, i.uv).rgb * _RayTint.rgb;
                return fixed4(r, 1);
            }
            ENDCG
        }
    }
}
