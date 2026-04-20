Shader "MoonlightMagicHouse/Sky"
{
    Properties
    {
        _Zenith   ("Zenith Color",  Color) = (0.05, 0.03, 0.18, 1)
        _Horizon  ("Horizon Color", Color) = (0.28, 0.14, 0.42, 1)
        _StarDensity ("Star Density", Float) = 60.0
        _StarThreshold ("Star Threshold", Range(0.9, 0.999)) = 0.985
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f    { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; };

            float4 _Zenith;
            float4 _Horizon;
            float  _StarDensity;
            float  _StarThreshold;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;
                return o;
            }

            float hash13(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 19.19);
                return frac((p.x + p.y) * p.z);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 d = normalize(i.dir);
                float up = saturate(d.y * 0.5 + 0.5);
                float t = pow(up, 0.6);
                float3 sky = lerp(_Horizon.rgb, _Zenith.rgb, t);

                // Nebula-ish soft variation
                float n = hash13(floor(d * 14.0));
                sky += (n - 0.5) * 0.025 * float3(0.8, 0.6, 1.0);

                // Stars: quantize direction, hash per cell
                if (d.y > -0.05)
                {
                    float3 cell = floor(d * _StarDensity);
                    float  h    = hash13(cell);
                    if (h > _StarThreshold)
                    {
                        float bright = (h - _StarThreshold) / (1.0 - _StarThreshold);
                        float tw = 0.7 + 0.3 * sin(_Time.y * 2.0 + h * 30.0);
                        sky += float3(1.0, 0.95, 0.85) * bright * tw * 1.4;
                    }
                }

                return fixed4(sky, 1.0);
            }
            ENDCG
        }
    }
    Fallback Off
}
