Shader "MoonlightMagicHouse/Bloom"
{
    Properties
    {
        _MainTex    ("Source", 2D) = "white" {}
        _BloomTex   ("Bloom",  2D) = "black" {}
        _Threshold  ("Threshold", Float) = 1.0
        _SoftKnee   ("Soft Knee", Float) = 0.5
        _Intensity  ("Intensity", Float) = 0.9
        _Vignette   ("Vignette",  Float) = 0.35
        _Tint       ("Tint",     Color) = (1,1,1,1)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        // 0: bright-pass
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float _Threshold; float _SoftKnee;
            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                float  brightness = max(c.r, max(c.g, c.b));
                float  knee       = _Threshold * _SoftKnee + 1e-5;
                float  soft       = brightness - _Threshold + knee;
                soft              = clamp(soft, 0, 2 * knee);
                soft              = soft * soft / (4 * knee + 1e-5);
                float  contrib    = max(soft, brightness - _Threshold);
                contrib           = contrib / max(brightness, 1e-5);
                return fixed4(c.rgb * contrib, 1);
            }
            ENDCG
        }

        // 1: 9-tap gaussian
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float4 _MainTex_TexelSize; float2 _BlurDir;
            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 off = _MainTex_TexelSize.xy * _BlurDir;
                fixed4 sum = tex2D(_MainTex, i.uv) * 0.227027;
                sum += tex2D(_MainTex, i.uv + off * 1.3846) * 0.316216;
                sum += tex2D(_MainTex, i.uv - off * 1.3846) * 0.316216;
                sum += tex2D(_MainTex, i.uv + off * 3.2307) * 0.070270;
                sum += tex2D(_MainTex, i.uv - off * 3.2307) * 0.070270;
                return sum;
            }
            ENDCG
        }

        // 2: composite (add bloom + vignette + tint)
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; sampler2D _BloomTex;
            float _Intensity; float _Vignette; float4 _Tint;
            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 src   = tex2D(_MainTex,  i.uv);
                fixed4 bloom = tex2D(_BloomTex, i.uv);
                float2 q     = i.uv - 0.5;
                float  vig   = 1.0 - dot(q, q) * _Vignette * 2.8;
                vig          = saturate(vig);
                float3 col   = src.rgb + bloom.rgb * _Intensity;
                col         *= _Tint.rgb;
                col         *= vig;
                return fixed4(col, 1);
            }
            ENDCG
        }
    }
}
