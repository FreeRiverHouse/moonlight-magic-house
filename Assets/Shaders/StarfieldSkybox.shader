Shader "MoonlightHouse/StarfieldSkybox"
{
    Properties
    {
        _TopColor    ("Sky Top",    Color) = (0.04, 0.02, 0.12, 1)
        _BotColor    ("Sky Bottom", Color) = (0.15, 0.05, 0.30, 1)
        _StarDensity ("Star Density", Range(10, 200)) = 80
        _StarBright  ("Star Brightness", Range(0, 1)) = 0.8
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _TopColor, _BotColor;
            float _StarDensity, _StarBright;

            struct v2f { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;
                return o;
            }

            float hash(float3 p)
            {
                p = frac(p * float3(443.8975, 397.2973, 491.1871));
                p += dot(p.xyz, p.yzx + 19.19);
                return frac(p.x * p.y * p.z);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);
                float t = dir.y * 0.5 + 0.5;
                fixed4 sky = lerp(_BotColor, _TopColor, t);

                // Stars (only upper hemisphere)
                float3 cell = floor(dir * _StarDensity);
                float star  = hash(cell);
                float glow  = step(0.98, star) * _StarBright * max(0, dir.y);
                // Twinkle
                glow *= 0.7 + 0.3 * sin(_Time.y * (3.0 + star * 10.0));

                sky.rgb += glow;
                return sky;
            }
            ENDCG
        }
    }
}
