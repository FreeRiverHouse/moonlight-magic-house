Shader "MoonlightMagicHouse/Outline"
{
    Properties
    {
        _MainTex         ("Source", 2D) = "white" {}
        _OutlineColor    ("Outline Color", Color) = (0.05, 0.03, 0.12, 1)
        _NormalThresh    ("Normal Threshold", Range(0.01, 1.0)) = 0.35
        _DepthThresh     ("Depth Threshold",  Range(0.0001, 0.05)) = 0.008
        _OutlineStrength ("Outline Strength", Range(0, 1)) = 0.85
        _Thickness       ("Thickness",        Range(1, 4)) = 1.2
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            sampler2D _CameraDepthNormalsTexture;
            float4    _MainTex_TexelSize;
            fixed4    _OutlineColor;
            float     _NormalThresh;
            float     _DepthThresh;
            float     _OutlineStrength;
            float     _Thickness;

            void SampleDN(float2 uv, out float3 n, out float d)
            {
                float4 cn = tex2D(_CameraDepthNormalsTexture, uv);
                DecodeDepthNormal(cn, d, n);
            }

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 src = tex2D(_MainTex, i.uv);
                float2 t   = _MainTex_TexelSize.xy * _Thickness;

                float3 n0; float d0; SampleDN(i.uv, n0, d0);
                float3 n1; float d1; SampleDN(i.uv + float2( t.x,  t.y), n1, d1);
                float3 n2; float d2; SampleDN(i.uv + float2(-t.x,  t.y), n2, d2);
                float3 n3; float d3; SampleDN(i.uv + float2( t.x, -t.y), n3, d3);
                float3 n4; float d4; SampleDN(i.uv + float2(-t.x, -t.y), n4, d4);

                float nDiff = length(n1 - n4) + length(n2 - n3);
                float dDiff = abs(d1 - d4) + abs(d2 - d3);

                float nEdge = step(_NormalThresh, nDiff);
                float dEdge = step(_DepthThresh, dDiff);
                float edge  = saturate(nEdge + dEdge) * _OutlineStrength;

                // Don't outline the sky (depth==1 on all neighbors)
                float skyMask = step(d0, 0.9999);
                edge *= skyMask;

                float3 col = lerp(src.rgb, _OutlineColor.rgb, edge);
                return fixed4(col, 1);
            }
            ENDCG
        }
    }
}
